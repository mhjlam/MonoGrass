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

		public FrameRateCounter(Renderer game) : base(game)
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

	public class Renderer : Game
	{
		private GraphicsDeviceManager graphicsDeviceManager;
		private GraphicsDevice graphicsDevice;

		private SceneManager sceneManager;
		private KeyboardState oldKeyState;
		private KeyboardState newKeyState;
		private FrameRateCounter frameRateCounter;

		public Renderer()
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
			// Update as often as possible, disable synchronization with Draw
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
			Microsoft.Xna.Framework.Graphics.Model xhead = Content.Load<Microsoft.Xna.Framework.Graphics.Model>("models/head");

			Model head = new Model(xhead);
			Model terrain = new Model(graphicsDevice, Content.Load<Texture2D>("textures/heightmap"), Vector3.Zero, Vector3.Zero, 0.5f);
			Model teapotModel = new Model(Content.Load<Microsoft.Xna.Framework.Graphics.Model>("models/teapot"), Vector3.Zero, Vector3.Zero, 10f);
			Model tabletopModel = new Model(Content.Load<Microsoft.Xna.Framework.Graphics.Model>("models/tabletop"), Vector3.Zero, Vector3.Zero, 20f);

			List<Model> headgerow = new List<Model>()
			{
				new Model(xhead, new Vector3(-80f + 0 * 40f, 0f, 0f)),
				new Model(xhead, new Vector3(-80f + 1 * 40f, 0f, 0f)),
				new Model(xhead, new Vector3(-80f + 2 * 40f, 0f, 0f)),
				new Model(xhead, new Vector3(-80f + 3 * 40f, 0f, 0f)),
				new Model(xhead, new Vector3(-80f + 4 * 40f, 0f, 0f)),
				new Model(xhead, new Vector3(-80f + 5 * 40f, 0f, 0f)),
				new Model(xhead, new Vector3(-80f + 6 * 40f, 0f, 0f)),
				new Model(xhead, new Vector3(-80f + 7 * 40f, 0f, 0f))
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
				SpecularIntensity = 1f,
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
			BasicEffect basicEffect = new BasicEffect(graphicsDevice);
			basicEffect.VertexColorEnabled = true;
			basicEffect.LightingEnabled = true;
			basicEffect.AmbientLightColor = new Vector3(0.3f);
			basicEffect.DirectionalLight0.Enabled = true;
			basicEffect.DirectionalLight0.DiffuseColor = Color.White.ToVector3();
			basicEffect.DirectionalLight0.Direction = new Vector3(0, -1, 0);

			Shader basicShader = new Shader(basicEffect);
			Shader woodShader = new WoodShader(Content.Load<Effect>("shaders/wooden"), woodMaterial, sceneManager.Camera, Content.Load<Texture2D>("textures/wood"));
			Shader phongShader = new PhongShader(Content.Load<Effect>("shaders/phong"), phongMaterial, sceneManager.Camera);
			Shader normalShader = new Shader(Content.Load<Effect>("shaders/normals"));
			Shader checkersShader = new Shader(Content.Load<Effect>("shaders/checkers"));
			Shader spotLightShader = new SpotLightShader(Content.Load<Effect>("shaders/spotlight"), phongMaterial);
			Shader lambertianShader = new LambertianShader(Content.Load<Effect>("shaders/lambertian"), lambertianMaterial);
			Shader multiLightShader = new MultiLightShader(Content.Load<Effect>("shaders/multilight"), cookTorranceMaterial);
			Shader projectionShader = new ProjectionShader(Content.Load<Effect>("shaders/projective-texture"), phongMaterial, Content.Load<Texture2D>("textures/smiley"));
			Shader cookTorranceShader = new CookTorranceShader(Content.Load<Effect>("shaders/cook-torrance"), cookTorranceMaterial);

			// Initialize post-processors
			Filter monochromeFilter = new Filter(graphicsDevice, spriteBatch, Content.Load<Effect>("shaders/monochrome"));
			Filter gaussianFilter = new GaussianBlur(graphicsDevice, spriteBatch, Content.Load<Effect>("shaders/gaussian-blur"));

			// Create scenes in the scene manager
			sceneManager.AddScene(SceneID.Terrain, basicShader, terrain, null, Vector3.Transform(new Vector3(0f, 0f, 100f), Matrix.CreateRotationX(MathHelper.ToRadians(-40f))));
			sceneManager.AddScene(SceneID.Wood, woodShader, tabletopModel, null, Vector3.Transform(new Vector3(0f, 0f, 100f), Matrix.CreateRotationX(MathHelper.ToRadians(20f))));
			sceneManager.AddScene(SceneID.Lambertian, lambertianShader, teapotModel, null, Vector3.Transform(new Vector3(0f, 0f, 100f), Matrix.CreateRotationX(MathHelper.ToRadians(-20f))));
			sceneManager.AddScene(SceneID.Phong, phongShader, teapotModel, null, Vector3.Transform(new Vector3(0f, 0f, 100f), Matrix.CreateRotationX(MathHelper.ToRadians(-20f))));
			sceneManager.AddScene(SceneID.Normals, normalShader, head);
			sceneManager.AddScene(SceneID.Checkers, checkersShader, teapotModel, null, Vector3.Transform(new Vector3(0f, 0f, 100f), Matrix.CreateRotationX(MathHelper.ToRadians(-20f))));
			sceneManager.AddScene(SceneID.CookTorrance, cookTorranceShader, head);
			sceneManager.AddScene(SceneID.Spotlight, spotLightShader, head);
			sceneManager.AddScene(SceneID.Multilight, multiLightShader, head);
			sceneManager.AddScene(SceneID.Culling, normalShader, headgerow);
			sceneManager.AddScene(SceneID.Projection, projectionShader, head);
			sceneManager.AddScene(SceneID.Monochrome, cookTorranceShader, head, monochromeFilter);
			sceneManager.AddScene(SceneID.GaussianBlur, normalShader, head, gaussianFilter);
		}

		private void HandleInput(float delta)
		{
			newKeyState = Keyboard.GetState();

			// Escape exits window
			if (newKeyState.IsKeyDown(Keys.Escape)) Exit();

			// Camera rotation controls (WASD)
			if (newKeyState.IsKeyDown(Keys.W)) sceneManager.Camera.SetEye(Vector3.Transform(sceneManager.Camera.Position, Matrix.CreateRotationX(-0.025f * delta)));
			if (newKeyState.IsKeyDown(Keys.A)) sceneManager.Camera.SetEye(Vector3.Transform(sceneManager.Camera.Position, Matrix.CreateRotationY(-0.025f * delta)));
			if (newKeyState.IsKeyDown(Keys.S)) sceneManager.Camera.SetEye(Vector3.Transform(sceneManager.Camera.Position, Matrix.CreateRotationX(+0.025f * delta)));
			if (newKeyState.IsKeyDown(Keys.D)) sceneManager.Camera.SetEye(Vector3.Transform(sceneManager.Camera.Position, Matrix.CreateRotationY(+0.025f * delta)));

			// Model translation controls for frustrum culling scene (cursor keys)
			if (sceneManager.SceneID == SceneID.Culling)
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
				sceneManager.SceneModels.ForEach(m => m.Reset());
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
