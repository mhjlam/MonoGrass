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
			// Setup graphics device
			graphicsDevice = graphicsDeviceManager.GraphicsDevice;
			graphicsDevice.RasterizerState = new RasterizerState();

			// Initialize bitmap renderer and font texture
			SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
			SpriteFont spriteFont = Content.Load<SpriteFont>("fonts/arial12");

			// Initialize scenemanager
			sceneManager = new SceneManager(graphicsDevice, spriteBatch, spriteFont);

			// Load models
			Model head = new Model(Content.Load<Microsoft.Xna.Framework.Graphics.Model>("models/head"));
			Model teapot = new Model(Content.Load<Microsoft.Xna.Framework.Graphics.Model>("models/teapot"), Vector3.Zero, new Vector3(MathHelper.ToRadians(30f), 0f, 0f), 10f);
			Model tabletop = new Model(Content.Load<Microsoft.Xna.Framework.Graphics.Model>("models/quad"), Vector3.Zero, new Vector3(MathHelper.ToRadians(-20f), 0f, 0f), 20f);

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
				DiffuseColor = Color.Orange
			};

			PhongMaterial phongMaterial = new PhongMaterial()
			{
				AmbientColor = Color.Red,
				AmbientIntensity = 0.2f,
				DiffuseColor = Color.Orange,
				SpecularColor = Color.White,
				SpecularIntensity = 4f,
				SpecularPower = 32f
			};

			PhongMaterial woodMaterial = new PhongMaterial()
			{
				AmbientColor = Color.Black,
				AmbientIntensity = 0.2f,
				DiffuseColor = Color.BurlyWood,
				SpecularColor = Color.White,
				SpecularIntensity = 0.2f,
				SpecularPower = 32f
			};

			CookTorranceMaterial cookTorranceMaterial = new CookTorranceMaterial()
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
			Shader woodShader = new WoodShader(Content.Load<Effect>("shaders/wooden"), woodMaterial, sceneManager.Camera, Content.Load<Texture2D>("textures/wood"));
			Shader phongShader = new PhongShader(Content.Load<Effect>("shaders/phong"), phongMaterial, sceneManager.Camera);
			Shader normalShader = new Shader(Content.Load<Effect>("shaders/normals"));
			Shader checkersShader = new CheckersShader(Content.Load<Effect>("shaders/checkers"));
			Shader spotLightShader = new SpotLightShader(Content.Load<Effect>("shaders/spotlight"), phongMaterial);
			Shader lambertianShader = new LambertianShader(Content.Load<Effect>("shaders/lambertian"), lambertianMaterial);
			Shader multiLightShader = new MultiLightShader(Content.Load<Effect>("shaders/multilight"), cookTorranceMaterial);
			Shader projectionShader = new ProjectionShader(Content.Load<Effect>("shaders/projective-texture"), phongMaterial, Content.Load<Texture2D>("textures/smiley"));
			Shader cookTorranceShader = new CookTorranceShader(Content.Load<Effect>("shaders/cook-torrance"), cookTorranceMaterial);

			// Initialize post-processors
			Filter monochrome = new Filter(graphicsDevice, spriteBatch, Content.Load<Effect>("shaders/monochrome"));
			Filter gaussian = new GaussianBlur(graphicsDevice, spriteBatch, Content.Load<Effect>("shaders/gaussian-blur"));

			// Create scenes in the scene manager
			sceneManager.AddScene(SceneID.Lambertian, lambertianShader, teapot);
			sceneManager.AddScene(SceneID.Phong, phongShader, teapot);
			sceneManager.AddScene(SceneID.Normals, normalShader, head);
			sceneManager.AddScene(SceneID.Checkers, checkersShader, teapot);
			sceneManager.AddScene(SceneID.Wood, woodShader, tabletop);
			sceneManager.AddScene(SceneID.CookTorrance, cookTorranceShader, head);
			sceneManager.AddScene(SceneID.Spotlight, spotLightShader, head);
			sceneManager.AddScene(SceneID.Multilight, multiLightShader, head);
			sceneManager.AddScene(SceneID.FrustumCulling, normalShader, heads);
			sceneManager.AddScene(SceneID.Projection, projectionShader, head);
			sceneManager.AddScene(SceneID.Monochrome, cookTorranceShader, head, monochrome);
			sceneManager.AddScene(SceneID.GaussianBlur, normalShader, head, gaussian);
		}

		private void HandleInput(float delta)
		{
			newKeyState = Keyboard.GetState();

			// Escape exits window
			if (newKeyState.IsKeyDown(Keys.Escape)) Exit();

			// Camera rotation controls (WASD)
			if (newKeyState.IsKeyDown(Keys.A)) sceneManager.Camera.SetEye(Vector3.Transform(sceneManager.Camera.Position, Matrix.CreateRotationY(-0.025f * delta)));
			if (newKeyState.IsKeyDown(Keys.D)) sceneManager.Camera.SetEye(Vector3.Transform(sceneManager.Camera.Position, Matrix.CreateRotationY(+0.025f * delta)));
			if (newKeyState.IsKeyDown(Keys.W)) sceneManager.Camera.SetEye(Vector3.Transform(sceneManager.Camera.Position, Matrix.CreateRotationX(-0.025f * delta)));
			if (newKeyState.IsKeyDown(Keys.S)) sceneManager.Camera.SetEye(Vector3.Transform(sceneManager.Camera.Position, Matrix.CreateRotationX(+0.025f * delta)));

			// Model translation controls for frustrum culling scene (cursor keys)
			if (sceneManager.SceneID == SceneID.FrustumCulling)
			{
				if (newKeyState.IsKeyDown(Keys.Left)) sceneManager.SceneModels.ForEach(m => m.Translate(Vector3.Left * delta));
				if (newKeyState.IsKeyDown(Keys.Right)) sceneManager.SceneModels.ForEach(m => m.Translate(Vector3.Right * delta));
				if (newKeyState.IsKeyDown(Keys.Up)) sceneManager.SceneModels.ForEach(m => m.Translate(Vector3.Up * delta));
				if (newKeyState.IsKeyDown(Keys.Down)) sceneManager.SceneModels.ForEach(m => m.Translate(Vector3.Down * delta));
			}
			// Model rotation controls for remaining scenes (cursor keys)
			else
			{
				if (newKeyState.IsKeyDown(Keys.Left)) sceneManager.SceneModels.ForEach(m => m.RotateY(-0.05f * delta));
				if (newKeyState.IsKeyDown(Keys.Right)) sceneManager.SceneModels.ForEach(m => m.RotateY(0.05f * delta));
			}

			// Reset camera/model(s)
			if (newKeyState.IsKeyDown(Keys.R))
			{
				sceneManager.Camera.Reset();

				foreach (Model model in sceneManager.SceneModels)
					model.Reset();
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
		}

		protected override void Update(GameTime gameTime)
		{
			float delta = (float)gameTime.ElapsedGameTime.TotalSeconds * 60f;
			Window.Title = "Scene viewer | FPS: " + frameRateCounter.FrameRate;

			sceneManager.Update();
			HandleInput(delta);

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			sceneManager.Draw();
			base.Draw(gameTime);
		}
	}
}
