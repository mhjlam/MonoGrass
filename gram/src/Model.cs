using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XnaModel = Microsoft.Xna.Framework.Graphics.Model;

namespace gram
{
	public struct VertexPositionNormalColor : IVertexType
	{
		public Vector3 Position;
		public Vector3 Normal;
		public Color Color;

		public VertexPositionNormalColor(Vector3 position, Color color, Vector3 normal)
		{
			Position = position;
			Color = color;
			Normal = normal;
		}

		public static VertexElement[] VertexElements =
		{
			new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
			new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
			new VertexElement(sizeof(float) * 6, VertexElementFormat.Color, VertexElementUsage.Color, 0)
		};

		public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(VertexElements);
		VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
	}

	public class Model
	{
		private float defaultScale, scale;
		private Vector3 defaultRotation, rotation;
		private Vector3 defaultPosition, position;

		private XnaModel xnaModel = null;
		private GraphicsDevice graphicsDevice = null;

		private IndexBuffer indexBuffer = null;
		private VertexBuffer vertexBuffer = null;

		public Vector3 Rotation
		{
			get { return rotation; }
			set { rotation = value; }
		}

		public Vector3 Position
		{
			get { return position; }
			set { position = value; }
		}

		public XnaModel XnaModel => xnaModel;
		public Matrix ScaleMatrix => Matrix.CreateScale(scale);
		public Matrix RotationMatrix => Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);
		public Matrix TranslationMatrix => Matrix.CreateTranslation(Position);
		public Matrix TransformationMatrix => ScaleMatrix * RotationMatrix * TranslationMatrix;

		public void Scale(float s) => scale = s;
		public void RotateX(float r) => rotation.X += r;
		public void RotateY(float r) => rotation.Y += r;
		public void RotateZ(float r) => rotation.Z += r;
		public void Translate(Vector3 t) => position += t;

		// Create Model from model resource
		public Model(XnaModel model, Vector3 position = new Vector3(), Vector3 rotation = new Vector3(), float scale = 1f)
		{
			xnaModel = model;
			indexBuffer = null;
			vertexBuffer = null;

			defaultPosition = this.position = position;
			defaultRotation = this.rotation = rotation;
			defaultScale = this.scale = scale;
		}

		// Create Model from vertex buffer
		public Model(VertexBuffer vbuffer, IndexBuffer ibuffer, Vector3 position = new Vector3(), Vector3 rotation = new Vector3(), float scale = 1f)
		{
			xnaModel = null;
			indexBuffer = ibuffer;
			vertexBuffer = vbuffer;

			defaultPosition = this.position = position;
			defaultRotation = this.rotation = rotation;
			defaultScale = this.scale = scale;
		}

		// Create Model from heightmap
		public Model(GraphicsDevice device, Texture2D heightmap, Vector3 position = new Vector3(), Vector3 rotation = new Vector3(), float scale = 1f)
		{
			graphicsDevice = device;

			defaultPosition = this.position = position;
			defaultRotation = this.rotation = rotation;
			defaultScale = this.scale = scale;

			int width = heightmap.Width;
			int height = heightmap.Height;

			int halfWidth = width / 2;
			int halfHeight = height / 2;

			VertexPositionNormalColor[] vertices = new VertexPositionNormalColor[width * height];
			short[] indices = new short[(width - 1) * (height - 1) * 6]; // number of indices = width * height * 6 (3 vertices per triangle; 2 triangles per 'block')

			Color[] bitmap = new Color[width * height];
			byte[,] data = new byte[width, height]; // color values can be described with an 8-bit int (0-255); only one value is used because image is in grayscale
			heightmap.GetData(bitmap);

			// Compute vertices
			for (int x = 0; x < width; ++x)
			{
				for (int z = 0; z < height; ++z)
				{
					int v = x + z * width;
					float h = bitmap[x + width * z].R * 0.25f; // height scale

					vertices[v].Position = new Vector3(-halfWidth + x, h, -halfWidth + z);
					vertices[v].Color = new Color(0.025f * h, 0.5f, 15f / (h * 2.5f)); // gradient color for height (simulates water effect), values are chosen empirically
				}
			}

			// Compute indices
			for (int x = 0, c = 0; x < width - 1; ++x)
			{
				for (int y = 0; y < height - 1; ++y)
				{
					short tl = (short)((x)     + (y)     * width);
					short tr = (short)((x + 1) + (y)     * width);
					short bl = (short)((x)     + (y + 1) * width);
					short br = (short)((x + 1) + (y + 1) * width);

					indices[c++] = tl;
					indices[c++] = tr;
					indices[c++] = br;

					indices[c++] = br;
					indices[c++] = bl;
					indices[c++] = tl;
				}
			}

			// Compute normals
			for (int i = 0; i < vertices.Length; ++i)
				vertices[i].Normal = new Vector3();

			for (int i = 0; i < indices.Length / 3; ++i)
			{
				int index1 = indices[i * 3];
				int index2 = indices[i * 3 + 1];
				int index3 = indices[i * 3 + 2];

				Vector3 side1 = vertices[index1].Position - vertices[index3].Position;
				Vector3 side2 = vertices[index1].Position - vertices[index2].Position;
				Vector3 normal = Vector3.Cross(side1, side2);

				Vector3.Normalize(vertices[index1].Normal += normal);
				Vector3.Normalize(vertices[index2].Normal += normal);
				Vector3.Normalize(vertices[index3].Normal += normal);
			}

			// Initialize vertex and index buffers
			vertexBuffer = new VertexBuffer(device, VertexPositionNormalColor.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
			vertexBuffer.SetData(vertices);

			indexBuffer = new IndexBuffer(device, typeof(short), indices.Length, BufferUsage.WriteOnly);
			indexBuffer.SetData(indices);
		}

		public void Draw(Scene scene, Camera camera)
		{
			Matrix World = Matrix.Identity * TransformationMatrix;
			Matrix View = camera.ViewMatrix;
			Matrix Projection = camera.ProjectionMatrix;

			Matrix WorldViewProjection = Matrix.Multiply(World, Matrix.Multiply(View, Projection));

			if (xnaModel != null)
			{
				foreach (ModelMesh mesh in XnaModel.Meshes)
				{
					foreach (ModelMeshPart part in mesh.MeshParts)
					{
						part.Effect = scene.Shader.Effect;
						part.Effect.CurrentTechnique = part.Effect.Techniques[0];

						part.Effect.Parameters["WorldViewProjection"].SetValue(WorldViewProjection);
						part.Effect.Parameters["WorldIT"].SetValue(Matrix.Transpose(Matrix.Invert(World)));

						if (part.Effect.Parameters["World"] != null)
							part.Effect.Parameters["World"].SetValue(World);
					}

					mesh.Draw();
				}
			}
			// Super awkward code, needs refactoring!
			else if (graphicsDevice != null && indexBuffer != null && vertexBuffer != null)
			{
				BasicEffect basicEffect = (BasicEffect)scene.Shader.Effect;
				
				basicEffect.World = World;
				basicEffect.View = View;
				basicEffect.Projection = Projection;
				
				foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
				{
					pass.Apply();

					graphicsDevice.Indices = indexBuffer;
					graphicsDevice.SetVertexBuffer(vertexBuffer);
					graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indexBuffer.IndexCount / 3);
				}
			}
		}

		public void Reset()
		{
			scale = defaultScale;
			position = defaultPosition;
			rotation = defaultRotation;
		}
	}
}
