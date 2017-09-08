using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace gram
{
	public class SceneManager
	{
		private SpriteFont font;
		private SpriteBatch batch;
		private GraphicsDevice device;

		private Scene scene;
		private List<Scene> scenes;

		private Camera camera;
		private RenderTarget2D capture;
		private BoundingFrustum frustum;
		
		public SceneID SceneID => scene.Id;
		public Camera Camera => camera;
		public List<Model> SceneModels => scene.Models;
		
		public SceneManager(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SpriteFont spriteFont)
		{
			device = graphicsDevice;
			batch = spriteBatch;
			font = spriteFont;

			scenes = new List<Scene>();
			capture = new RenderTarget2D(device, device.Viewport.Width, device.Viewport.Height, false, device.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

			camera = new Camera(Vector3.Zero, Vector3.Zero, (float)device.PresentationParameters.BackBufferWidth / (float)device.PresentationParameters.BackBufferHeight);
			frustum = new BoundingFrustum(camera.ViewMatrix * camera.ProjectionMatrix);
		}

		public void AddScene(SceneID id, Shader shader, Model model, Filter postProcess = null, Vector3? eye = null)
		{
			scenes.Add(new Scene()
			{
				Id = id,
				Models = new List<Model> { model },
				Shader = shader,
				Eye = eye ?? new Vector3(0f, 10f, 100f),
				PostProcess = postProcess
			});

			// Load first scene automatically
			if (scenes.Count == 1) LoadScene(0);
		}

		public void AddScene(SceneID id, Shader shader, List<Model> models, Filter postProcess = null, Vector3? eye = null)
		{
			scenes.Add(new Scene()
			{
				Id = id,
				Models = models,
				Shader = shader,
				Eye = eye ?? new Vector3(0f, 10f, 100f),
				PostProcess = postProcess
			});
			
			if (scenes.Count == 1) LoadScene(0);
		}
		
		public void LoadScene(int index)
		{
			if (scenes.Count == 0) return;
			if (scenes.Count == 1) index = 0;

			scene = scenes[index];

			foreach (Model model in scene.Models)
				model.Reset();

			camera.SetEye(scene.Eye, true);
		}

		public void LoadNextScene()
		{
			if (scenes.Count <= 1) return;
			int index = scenes.FindIndex(s => s.Equals(scene));
			LoadScene(index + 1 >= scenes.Count ? 0 : index + 1);
		}

		public void LoadPrevScene()
		{
			if (scenes.Count <= 1) return;
			int index = scenes.FindIndex(s => s.Equals(scene));
			LoadScene(index - 1 < 0 ? scenes.Count - 1 : index - 1);
		}
		
		public void Update()
		{
			// Update camera matrix
			camera.Update();

			// Update view frustum
			frustum.Matrix = camera.ViewMatrix * camera.ProjectionMatrix;

			// Update camera position if required by scene
			if (scene.Shader.Effect.Parameters["CameraPosition"] != null)
				scene.Shader.Effect.Parameters["CameraPosition"].SetValue(camera.Position);
			
			// Update projector projection if required by scene
			if (scene.Shader.Effect.Parameters["ProjectorViewProjection"] != null)
			{
				// Need model transformation matrix to generate projector's WorldViewProjection matrix
				Vector3 ProjectorPosition = (scene.Shader.Effect.Parameters["ProjectorPosition"] != null) ?
											 scene.Shader.Effect.Parameters["ProjectorPosition"].GetValueVector3() : 
											 new Vector3(0f, 20f, 30f);

				Matrix ProjectorViewProjection = Matrix.Identity * SceneModels.First().TransformationMatrix *
											     Matrix.CreateLookAt(ProjectorPosition, new Vector3(0f, 10f, 0f), Vector3.Up) *
											     Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(20f), 1f, 1f, 100f);

				// An orthographic matrix prevents the projection from scaling over distance
				scene.Shader.Effect.Parameters["ProjectorViewProjection"].SetValue(ProjectorViewProjection);
			}
		}

		public void Draw()
		{
			if (scenes.Count == 0) return;
			if (scene.Models.Count == 0) return;

			// For post-process scenes, render to texture instead of to back buffer
			if (scene.PostProcess != null)
			{
				device.SetRenderTarget(capture);
			}

			device.Clear(Color.Black);

			foreach (Model model in scene.Models)
			{
				// Accumulate bounding spheres of all mesh parts
				BoundingSphere boundingSphere = new BoundingSphere();

				foreach (ModelMesh mesh in model.XModel.Meshes)
					boundingSphere = BoundingSphere.CreateMerged(boundingSphere, mesh.BoundingSphere);

				boundingSphere.Center = model.Position;
				
				// Only draw model when its bounding sphere intersects the viewing frustum
				if (frustum.Intersects(boundingSphere))
					model.Draw(scene, camera);
			}

			// Use captured image to draw to back buffer using post-process shader
			if (scene.PostProcess != null)
			{
				device.SetRenderTarget(null);
				scene.PostProcess.Draw(capture);
			}

			// Show on-screen scene information
			batch.Begin();
			batch.DrawString(font, scene.SceneTitle, new Vector2(20f, 20f), Color.White);
			batch.End();

			// Reset device parameters messed up by SpriteBatch
			device.BlendState = BlendState.Opaque;
			device.DepthStencilState = DepthStencilState.Default;
			device.SamplerStates[0] = SamplerState.LinearWrap;
		}
	}
}
