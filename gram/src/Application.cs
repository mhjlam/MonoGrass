using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace gram
{
	class FrameRateCounter : DrawableGameComponent
	{
		private int frameRate;
		private int frameCounter;
		private int secondsPassed;

		public int FrameRate => frameRate;

		public FrameRateCounter(Application game) : base(game)
		{
			frameRate = 0;
			frameCounter = 0;
			secondsPassed = 0;
		}

		public override void Update(GameTime gameTime)
		{
			if (secondsPassed != gameTime.TotalGameTime.Seconds)
			{
				frameRate = frameCounter;
				secondsPassed = gameTime.TotalGameTime.Seconds;
				frameCounter = 0;
			}

			frameCounter++;
		}
	}

	public class Application : Game
	{
		private GraphicsDeviceManager graphicsDeviceManager;
		private GraphicsDevice graphicsDevice;

		private SceneManager sceneManager;
		private KeyboardState oldKeyState;
		private KeyboardState newKeyState;
		private FrameRateCounter frameRateCounter;

		public Application()
		{
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
			Window.Position = new Point(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width  / 2 - Window.ClientBounds.Width  / 2,
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

			// Load models
			Model head = new Model(Content.Load<Microsoft.Xna.Framework.Graphics.Model>("models/head"));
			Model teapot = new Model(Content.Load<Microsoft.Xna.Framework.Graphics.Model>("models/teapot"), Vector3.Zero, new Vector3(MathHelper.ToRadians(30f), 0f, 0f), 10f);

			Quadrilateral tabletop = new Quadrilateral(new Vector3(-50f, -16f, -50f),
													   new Vector3( 50f, -16f, -50f),
													   new Vector3(-50f, -16f,  50f),
													   new Vector3( 50f, -16f,  50f));

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
			LambertianMaterial lambmat = new LambertianMaterial()
			{
				AmbientColor = Color.Red,
				AmbientIntensity = 0.2f,
				DiffuseColor = Color.Orange
			};

			PhongMaterial phongmat = new PhongMaterial()
			{
				AmbientColor = Color.Red,
				AmbientIntensity = 0.2f,
				DiffuseColor = Color.Orange,
				SpecularColor = Color.White,
				SpecularIntensity = 4f,
				SpecularPower = 32f
			};

			CookTorranceMaterial cooktormat = new CookTorranceMaterial()
			{
				AmbientColor = Color.Gold,
				AmbientIntensity = 0.2f,
				DiffuseColor = Color.Goldenrod,
				SpecularColor = Color.White,
				SpecularIntensity = 2f,
				SpecularPower = 25f,
				Roughness = 0.5f,
				ReflectanceCoefficient = 1.42f
			};

			// Initialize shaders
			Shader lamb = new LambertianShader(Content.Load<Effect>("shaders/lambertian"), lambmat);
			Shader phong = new PhongShader(Content.Load<Effect>("shaders/phong"), phongmat, sceneManager.SceneCamera);
			Shader normals = new Shader(Content.Load<Effect>("shaders/normals"));
			Shader checkers = new CheckersShader(Content.Load<Effect>("shaders/checkers"));
			Shader cooktor = new CookTorranceShader(Content.Load<Effect>("shaders/cook-torrance"), cooktormat);

			Shader spotlight = new SpotLightShader(Content.Load<Effect>("shaders/spotlight"), phongmat);
			Shader multilight = new MultiLightShader(Content.Load<Effect>("shaders/multilight"), cooktormat);
			Shader projection = new ProjectionShader(Content.Load<Effect>("shaders/projective-texture"), phongmat, Content.Load<Texture2D>("textures/smiley"));

			// Initialize post-processors
			Filter monochrome = new Filter(graphicsDevice, spriteBatch, Content.Load<Effect>("shaders/monochrome"));
			Filter gaussian = new GaussianBlur(graphicsDevice, spriteBatch, Content.Load<Effect>("shaders/gaussian-blur"));

			// Create scenes in the scene manager
			sceneManager.AddScene(SceneID.Lambertian, lamb, teapot);
			sceneManager.AddScene(SceneID.Phong, phong, teapot);
			sceneManager.AddScene(SceneID.Normals, normals, head);
			sceneManager.AddScene(SceneID.Checkers, checkers, teapot);
			sceneManager.AddScene(SceneID.CookTorrance, cooktor, head);
			sceneManager.AddScene(SceneID.Spotlight, spotlight, head);
			sceneManager.AddScene(SceneID.Multilight, multilight, head);
			sceneManager.AddScene(SceneID.FrustumCulling, normals, heads);
			sceneManager.AddScene(SceneID.Projection, projection, head);
			sceneManager.AddScene(SceneID.Monochrome, cooktor, head, monochrome);
			sceneManager.AddScene(SceneID.GaussianBlur, normals, head, gaussian);
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

			// Camera rotation controls (WASD)
			if (newKeyState.IsKeyDown(Keys.A)) sceneManager.SceneCamera.MoveTo(Vector3.Transform(sceneManager.SceneCamera.Position, Matrix.CreateRotationY(-0.025f * timeStep)));
			if (newKeyState.IsKeyDown(Keys.D)) sceneManager.SceneCamera.MoveTo(Vector3.Transform(sceneManager.SceneCamera.Position, Matrix.CreateRotationY(+0.025f * timeStep)));
			if (newKeyState.IsKeyDown(Keys.W)) sceneManager.SceneCamera.MoveTo(Vector3.Transform(sceneManager.SceneCamera.Position, Matrix.CreateRotationX(-0.025f * timeStep)));
			if (newKeyState.IsKeyDown(Keys.S)) sceneManager.SceneCamera.MoveTo(Vector3.Transform(sceneManager.SceneCamera.Position, Matrix.CreateRotationX(+0.025f * timeStep)));

			// Model translation controls for frustrum culling scene (cursor keys)
			if (sceneManager.SceneID == SceneID.FrustumCulling)
			{
				if (newKeyState.IsKeyDown(Keys.Left)) sceneManager.SceneModels.ForEach(m => m.Translate(Vector3.Left * timeStep));
				if (newKeyState.IsKeyDown(Keys.Right)) sceneManager.SceneModels.ForEach(m => m.Translate(Vector3.Right * timeStep));
				if (newKeyState.IsKeyDown(Keys.Up)) sceneManager.SceneModels.ForEach(m => m.Translate(Vector3.Up * timeStep));
				if (newKeyState.IsKeyDown(Keys.Down)) sceneManager.SceneModels.ForEach(m => m.Translate(Vector3.Down * timeStep));
			}
			// Model rotation controls for remaining scenes (cursor keys)
			else
			{
				if (newKeyState.IsKeyDown(Keys.Left)) sceneManager.SceneModels.ForEach(m => m.RotateY(-0.05f * timeStep));
				if (newKeyState.IsKeyDown(Keys.Right)) sceneManager.SceneModels.ForEach(m => m.RotateY(0.05f * timeStep));
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
			// Let SceneSwitcher render active scene
			sceneManager.DrawScene();
			base.Draw(gameTime);
		}
	}
}
