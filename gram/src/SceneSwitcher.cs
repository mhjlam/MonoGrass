using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace gram3
{
	public class SceneSwitcher
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
		public Model FirstSceneModel => scene.Models[0];
		public List<Model> SceneModels => scene.Models;

		// SceneManager makes it easy to switch between scenes that require different shaders, post-processors, and other properties.
		public SceneSwitcher(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SpriteFont spriteFont)
		{
			device = graphicsDevice;
			batch = spriteBatch;
			font = spriteFont;

			scenes = new List<Scene>(Enum.GetNames(typeof(SceneID)).Length);
			capture = new RenderTarget2D(device, device.Viewport.Width, device.Viewport.Height, false, device.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

			camera = new Camera(Vector3.Zero, Vector3.Zero, (float)device.PresentationParameters.BackBufferWidth / (float)device.PresentationParameters.BackBufferHeight);
			frustum = new BoundingFrustum(camera.ViewMatrix * camera.ProjectionMatrix);
		}

		// Creates a scene with the specified parameters and automatically loads it if the created scene is the only scene.
		public void CreateScene(SceneID id, Shader shader, Model model, PostProcess postProcess = null)
		{
			scenes.Add(new Scene()
			{
				Id = id,
				Eye = new Vector3(0.0f, 10.0f, 100.0f),
				Shader = shader,
				PostProcess = postProcess,
				Models = new List<Model>() { model }
			});

			// if this the only scene, load it automatically
			if (scenes.Count == 1) LoadScene(0);
		}

		// Creates a scene that contains multiple models and automatically loads it if the created scene is the only one.
		public void CreateScene(SceneID id, Shader shader, List<Model> models, PostProcess postProcess = null)
		{
			scenes.Add(new Scene()
			{
				Id = id,
				Eye = new Vector3(0.0f, 10.0f, 200.0f), // zoomed out a little to get a better overview
				Models = models,
				Shader = shader,
				PostProcess = postProcess
			});

			camera.ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1.25f, 1.0f, 500.0f); // set the far distance further along

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

		public void LoadScene(SceneID id)
		{
			if (scenes.Count == 0) return;
			if (scenes.Count == 1) id = (SceneID)0;
			LoadScene(id);
		}
		
		public void LoadFirstScene()
		{
			if (scenes.Count <= 1) return;
			LoadScene(0);
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

			// update parameters for certain scenes
			if (scene.Shader is PhongShader || scene.Shader is CookTorranceShader || scene.Shader is MultiLightShader)
			{
				// required for specular lighting
				scene.Shader.Effect.Parameters["CameraPosition"].SetValue(camera.Position);
			}
			else if (scene.Shader is ProjectionShader)
			{
				// we need the model's transformation matrix to generate the projector's WorldViewProjection matrix
				Vector3 ProjectorPosition = scene.Shader.Effect.Parameters["ProjectorPosition"].GetValueVector3();

				Matrix ProjectorViewProjection = Matrix.Identity * FirstSceneModel.TransformationMatrix *
											     Matrix.CreateLookAt(ProjectorPosition, new Vector3(0.0f, 10.0f, 0.0f), Vector3.Up) *
											     Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(20f), 1.0f, 1.0f, 100.0f);

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
			batch.DrawString(font, message, new Vector2(20, 20), Color.White);
			batch.End();
		}
	}
}
