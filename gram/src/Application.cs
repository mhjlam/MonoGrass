using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace gram3
{
	public class Application : Game
	{
		private GraphicsDeviceManager graphicsDeviceManager;
		private GraphicsDevice graphicsDevice;

		private KeyboardState oldKeyState;
		private KeyboardState newKeyState;
		private SceneManager sceneManager;
		private FrameRateCounter frameRateCounter;

		public Application()
		{
			// Setup graphics device
			graphicsDeviceManager = new GraphicsDeviceManager(this)
			{
				IsFullScreen = false,
				GraphicsProfile = GraphicsProfile.HiDef,
				PreferredBackBufferWidth = 1280,
				PreferredBackBufferHeight = 720,
				SynchronizeWithVerticalRetrace = false // disable vsync
			};

			graphicsDeviceManager.ApplyChanges();
			
			frameRateCounter = new FrameRateCounter(this);
			Components.Add(frameRateCounter);

			Content.RootDirectory = "content";
		}

		protected override void Initialize()
		{
			// Call update as often as possible; disable synchronization with Draw
			IsFixedTimeStep = false;
			
			// Center window
			Window.Position = new Point(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2 - Window.ClientBounds.Width / 2, 
										GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 2 - Window.ClientBounds.Height / 2);

			// Preload keyboard state
			oldKeyState = Keyboard.GetState();

			base.Initialize();
		}

		protected override void LoadContent()
		{
			// Setup device
			graphicsDevice = graphicsDeviceManager.GraphicsDevice;
			graphicsDevice.RasterizerState = new RasterizerState();

			// Initialize bitmap renderer and font texture
			SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
			SpriteFont spriteFont = Content.Load<SpriteFont>("fonts/arial12");

			// Initialize scenemanager with device, sprite batch and sprite font
			sceneManager = new SceneManager(graphicsDevice, spriteBatch, spriteFont);

			// Load 3D model
			Model head = new Model(Content.Load<Microsoft.Xna.Framework.Graphics.Model>("models/head"), Vector3.Zero);

			// Load multiple 3D models for Frustrum Culling scene
			List<Model> heads = new List<Model>()
			{
				new Model(Content.Load<Microsoft.Xna.Framework.Graphics.Model>("models/head"), new Vector3(-80f + 0 * 40f, 0f, 0f)),
				new Model(Content.Load<Microsoft.Xna.Framework.Graphics.Model>("models/head"), new Vector3(-80f + 1 * 40f, 0f, 0f)),
				new Model(Content.Load<Microsoft.Xna.Framework.Graphics.Model>("models/head"), new Vector3(-80f + 2 * 40f, 0f, 0f)),
				new Model(Content.Load<Microsoft.Xna.Framework.Graphics.Model>("models/head"), new Vector3(-80f + 3 * 40f, 0f, 0f)),
				new Model(Content.Load<Microsoft.Xna.Framework.Graphics.Model>("models/head"), new Vector3(-80f + 4 * 40f, 0f, 0f)),
				new Model(Content.Load<Microsoft.Xna.Framework.Graphics.Model>("models/head"), new Vector3(-80f + 5 * 40f, 0f, 0f)),
				new Model(Content.Load<Microsoft.Xna.Framework.Graphics.Model>("models/head"), new Vector3(-80f + 6 * 40f, 0f, 0f)),
				new Model(Content.Load<Microsoft.Xna.Framework.Graphics.Model>("models/head"), new Vector3(-80f + 7 * 40f, 0f, 0f))
			};

			// Initialize materials
			LambertianMaterial lambertianMaterial = new LambertianMaterial()
			{
				AmbientColor = Color.Red,
				AmbientIntensity = 0.2f,
				DiffuseColor = Color.Orange,
				DiffuseIntensity = 1f
			};

			PhongMaterial phongMaterial = new PhongMaterial()
			{
				AmbientColor = Color.LightCyan,
				AmbientIntensity = 0.2f,
				DiffuseColor = Color.SteelBlue,
				DiffuseIntensity = 1f,
				SpecularColor = Color.White,
				SpecularIntensity = 2f,
				SpecularPower = 12f
			};

			NormalMaterial normalMaterial = new NormalMaterial()
			{
				AmbientColor = Color.Green,
				AmbientIntensity = 0.2f
			};

			CookTorranceMaterial cookTorranceMaterial = new CookTorranceMaterial()
			{
				AmbientColor = Color.Gold,
				AmbientIntensity = 0.2f,
				DiffuseColor = Color.Goldenrod,
				DiffuseIntensity = 1f,
				SpecularColor = Color.White,
				SpecularIntensity = 2f,
				SpecularPower = 25f,
				Roughness = 0.5f,
				ReflectanceCoefficient = 1.42f
			};

			// Initialize shaders
			Shader lambertian = new LambertianShader(Content.Load<Effect>("shaders/lambertian"), lambertianMaterial);
			Shader phong = new PhongShader(Content.Load<Effect>("shaders/phong"), phongMaterial, sceneManager.SceneCamera);
			Shader normals = new NormalShader(Content.Load<Effect>("shaders/normals"), normalMaterial);
			Shader cooktorrance = new CookTorranceShader(Content.Load<Effect>("shaders/cook-torrance"), cookTorranceMaterial);

			Shader spotlight = new SpotLightShader(Content.Load<Effect>("shaders/spotlight"), phongMaterial);
			Shader multilight = new MultiLightShader(Content.Load<Effect>("shaders/multilight"), cookTorranceMaterial);
			Shader projection = new ProjectionShader(Content.Load<Effect>("shaders/projective-texture"), cookTorranceMaterial, Content.Load<Texture2D>("textures/smiley"));

			// Initialize post-processors
			PostProcess monochrome = new PostProcess(graphicsDevice, spriteBatch, Content.Load<Effect>("shaders/monochrome"));
			GaussianBlur gaussian = new GaussianBlur(graphicsDevice, spriteBatch, Content.Load<Effect>("shaders/gaussian-blur"));

			// Create scenes in the scene manager
			sceneManager.AddScene(SceneID.Lambertian, lambertian, head);
			sceneManager.AddScene(SceneID.Phong, phong, head);
			sceneManager.AddScene(SceneID.Normals, normals, head);
			sceneManager.AddScene(SceneID.CookTorrance, cooktorrance, head);
			sceneManager.AddScene(SceneID.SpotLight, spotlight, head);
			sceneManager.AddScene(SceneID.MultiLight, multilight, head);
			sceneManager.AddScene(SceneID.FrustumCulling, normals, heads);
			sceneManager.AddScene(SceneID.Monochrome, cooktorrance, head, monochrome);
			sceneManager.AddScene(SceneID.GaussianBlur, normals, head, gaussian);
			sceneManager.AddScene(SceneID.ProjectiveTexture, projection, head);
		}

		protected override void Update(GameTime gameTime)
		{
			// Timestep is used for frame independent updating
			float timeStep = (float)gameTime.ElapsedGameTime.TotalSeconds * 60f;

			// Set window title
			Window.Title = "Scene viewer | FPS: " + frameRateCounter.FrameRate;

			// Update the parameters for the scene
			sceneManager.UpdateScene();

			// Get the state of the keyboard
			newKeyState = Keyboard.GetState();

			// Escape exits window
			if (newKeyState.IsKeyDown(Keys.Escape)) Exit();

			// Rotational controls (WASD)
			if (newKeyState.IsKeyDown(Keys.A)) sceneManager.SceneCamera.MoveTo(Vector3.Transform(sceneManager.SceneCamera.Position, Matrix.CreateRotationY(-0.025f * timeStep)));
			if (newKeyState.IsKeyDown(Keys.D)) sceneManager.SceneCamera.MoveTo(Vector3.Transform(sceneManager.SceneCamera.Position, Matrix.CreateRotationY(+0.025f * timeStep)));
			if (newKeyState.IsKeyDown(Keys.W)) sceneManager.SceneCamera.MoveTo(Vector3.Transform(sceneManager.SceneCamera.Position, Matrix.CreateRotationX(-0.025f * timeStep)));
			if (newKeyState.IsKeyDown(Keys.S)) sceneManager.SceneCamera.MoveTo(Vector3.Transform(sceneManager.SceneCamera.Position, Matrix.CreateRotationX(+0.025f * timeStep)));

			// For the frustum culling scene...
			if (sceneManager.SceneID == SceneID.FrustumCulling)
			{
				// Translation of models is enabled
				if (newKeyState.IsKeyDown(Keys.Left)) sceneManager.SceneModels.ForEach(m => m.Translate(Vector3.Left * timeStep)); // translate left
				if (newKeyState.IsKeyDown(Keys.Right)) sceneManager.SceneModels.ForEach(m => m.Translate(Vector3.Right * timeStep)); // translate right
				if (newKeyState.IsKeyDown(Keys.Up)) sceneManager.SceneModels.ForEach(m => m.Translate(Vector3.Up * timeStep)); // translate up
				if (newKeyState.IsKeyDown(Keys.Down)) sceneManager.SceneModels.ForEach(m => m.Translate(Vector3.Down * timeStep)); // translate down
			}
			// Rotational controls for remaining scenes
			else
			{
				// Model rotation controls
				if (newKeyState.IsKeyDown(Keys.Left)) sceneManager.SceneModels.ForEach(m => m.Rotate(-0.05f * timeStep)); // clockwise rotation
				if (newKeyState.IsKeyDown(Keys.Right)) sceneManager.SceneModels.ForEach(m => m.Rotate(0.05f * timeStep)); // counter-clockwise rotation
			}

			// Reset camera/model(s)
			if (newKeyState.IsKeyDown(Keys.R))
			{
				sceneManager.SceneCamera.Reset();

				foreach (Model model in sceneManager.SceneModels)
				{
					model.ResetPosition();
					model.ResetRotation();
				}
			}

			// Scene cycle controls
			if (newKeyState.IsKeyDown(Keys.Space) && oldKeyState.IsKeyUp(Keys.Space))
			{
				if (newKeyState.IsKeyDown(Keys.LeftShift))
					sceneManager.LoadPrevScene();
				else
					sceneManager.LoadNextScene();
			}

			oldKeyState = newKeyState;
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			// All rendering is delegated to the SceneSwitcher as each scene requires different parameters
			sceneManager.DrawScene();

			base.Draw(gameTime);
		}
	}
}
