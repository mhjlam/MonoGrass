using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace gram3
{
	public class SceneManager
	{
		private SpriteFont font;
		private SpriteBatch batch;
		private GraphicsDevice device;

		private Scene scene;
		private Camera camera;
		private RenderTarget2D capture;
		private BoundingFrustum frustum;
		private List<Scene> scenes;
		
		public SceneID SceneID => scene.Id;
		public String SceneTitle => scene.SceneTitle;
		public Camera SceneCamera => camera;
		public Shader SceneShader => scene.Shader;
		public Model FirstModel => scene.Models[0];
		public List<Model> SceneModels => scene.Models;
		
		public SceneManager(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SpriteFont spriteFont)
		{
			device = graphicsDevice;
			batch = spriteBatch;
			font = spriteFont;

			scenes = new List<Scene>(Enum.GetNames(typeof(SceneID)).Length);
			capture = new RenderTarget2D(device, device.Viewport.Width, device.Viewport.Height, false, device.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

			camera = new Camera(Vector3.Zero, Vector3.Zero, (float)device.PresentationParameters.BackBufferWidth / (float)device.PresentationParameters.BackBufferHeight);
			frustum = new BoundingFrustum(camera.ViewMatrix * camera.ProjectionMatrix);
		}

		public void AddScene(SceneID id, Shader shader, Model model, PostProcess postProcess = null, Vector3? eye = null)
		{
			scenes.Add(new Scene()
			{
				Id = id,
				Models = new List<Model> { model },
				Shader = shader,
				Eye = eye ?? new Vector3(0f, 10f, 100f),
				PostProcess = postProcess
			});

			// if this the only scene, load it automatically
			if (scenes.Count == 1) LoadScene(0);
		}

		public void AddScene(SceneID id, Shader shader, List<Model> models, PostProcess postProcess = null, Vector3? eye = null)
		{
			scenes.Add(new Scene()
			{
				Id = id,
				Models = models,
				Shader = shader,
				Eye = eye ?? new Vector3(0f, 10f, 100f),
				PostProcess = postProcess
			});

			// if this the only scene, load it automatically
			if (scenes.Count == 1) LoadScene(0);
		}

		// Loads the scene with the given id. If no scene with that id exists, nothing happens.
		public void LoadScene(int index)
		{
			if (scenes.Count == 0) return;
			if (scenes.Count == 1) index = 0;

			scene = scenes[index];

			foreach (Model model in scene.Models)
			{
				model.ResetPosition();
				model.ResetRotation();
			}

			camera.MoveTo(scene.Eye, true);
		}

		public void LoadNextScene()
		{
			if (scenes.Count <= 1) return;
			LoadScene(((int)(scene.Id + 1) >= Enum.GetNames(typeof(SceneID)).Length) ? 0 : (int)(scene.Id + 1));
		}

		public void LoadPrevScene()
		{
			if (scenes.Count <= 1) return;
			LoadScene(((int)(scene.Id - 1) < 0) ? Enum.GetNames(typeof(SceneID)).Length - 1 : (int)(scene.Id - 1));
		}

		// Updates properties required for certain scenes.
		public void UpdateScene()
		{
			// update view frustum
			frustum.Matrix = camera.ViewMatrix * camera.ProjectionMatrix;

			// update camera position if required by scene
			if (scene.Shader.Effect.Parameters["CameraPosition"] != null)
			{
				scene.Shader.Effect.Parameters["CameraPosition"].SetValue(camera.Position);
			}
			
			// update projector projection if required by scene
			if (scene.Shader.Effect.Parameters["ProjectorViewProjection"] != null)
			{
				// need model transformation matrix to generate the projector's WorldViewProjection matrix
				Vector3 ProjectorPosition = (scene.Shader.Effect.Parameters["ProjectorPosition"] != null) ?
					scene.Shader.Effect.Parameters["ProjectorPosition"].GetValueVector3() : new Vector3(0f, 20f, 30f);

				Matrix ProjectorViewProjection = Matrix.Identity * FirstModel.TransformationMatrix *
											     Matrix.CreateLookAt(ProjectorPosition, new Vector3(0f, 10f, 0f), Vector3.Up) *
											     Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(20f), 1f, 1f, 100f);

				// alternatively, an orthographic matrix can be used to prevent the projection from scaling over distance
				scene.Shader.Effect.Parameters["ProjectorViewProjection"].SetValue(ProjectorViewProjection);
			}
		}

		// Draws the current scene.
		public void DrawScene()
		{
			// no need to draw if there are no scenes
			if (scenes.Count == 0) return;

			// keep track of number of objects that are not drawn due to frustrum culling
			int numCulled = 0;

			// fix the spritebatch messing up the renderer
			device.BlendState = BlendState.Opaque;
			device.DepthStencilState = DepthStencilState.Default;
			device.SamplerStates[0] = SamplerState.LinearWrap;
			
			// if this scene has a post-processor attached, render to texture instead of default renderer
			if (scene.PostProcess != null)
			{
				device.SetRenderTarget(capture);
			}

			device.Clear(Color.Black);

			// for each model, test if bounding volumes collide, then draw the model
			if (SceneModels != null)
			{
				foreach (Model model in SceneModels)
				{
					// The bounding sphere of the model is non-optimal because it only looks at the first mesh
					// Fortunately, femalehead.fbx only consists of one mesh
					if (frustum.Intersects(new BoundingSphere(model.Position, model.XnaModel.Meshes[0].BoundingSphere.Radius)))
					{
						model.Draw(scene, camera);
					}
					else
					{
						numCulled++; // an object was culled, as its bounding sphere did not intersect with the bounding frustum
					}
				}
			}

			// store the rendered image into a buffer and redraw via post-processor using the captured image
			if (scene.PostProcess != null)
			{
				device.SetRenderTarget(null);
				scene.PostProcess.Draw(capture);
			}

			// show on-screen information with a sprite font
			string message = SceneTitle;
			if (scene.Id == SceneID.FrustumCulling)
			{
				message = SceneTitle + " (" + numCulled + " objects are culled)";
			}

			batch.Begin();
			batch.DrawString(font, message, new Vector2(20f, 20f), Color.White);
			batch.End();
		}
	}
}
