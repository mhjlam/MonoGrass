using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MeshModel = Microsoft.Xna.Framework.Graphics.Model;

namespace gram
{
	public class Model
	{
		private float defaultScale;
		private Vector3 defaultRotation;
		private Vector3 defaultPosition;

		private float scale;
		private Vector3 rotation;
		private Vector3 position;

		private MeshModel meshModel;

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

		public MeshModel MeshModel => meshModel;
		public Matrix ScaleMatrix => Matrix.CreateScale(scale);
		public Matrix RotationMatrix => Matrix.CreateFromAxisAngle(Vector3.UnitX, Rotation.X) * 
										Matrix.CreateFromAxisAngle(Vector3.UnitY, Rotation.Y) * 
										Matrix.CreateFromAxisAngle(Vector3.UnitZ, Rotation.Z);
		public Matrix TranslationMatrix => Matrix.CreateTranslation(Position);
		public Matrix TransformationMatrix => ScaleMatrix * RotationMatrix * TranslationMatrix;

		public void Scale(float s) => scale = s;
		public void RotateX(float r) => rotation.X += r;
		public void RotateY(float r) => rotation.Y += r;
		public void RotateZ(float r) => rotation.Z += r;
		public void Translate(Vector3 t) => position += t;
		public void ResetPosition() => position = defaultPosition;
		public void ResetRotation() => rotation = defaultRotation;
		public void ResetScale() => scale = defaultScale;


		// Model3D describes an XNA model and manages some of its properties.
		public Model(MeshModel model, Vector3 position = new Vector3(), Vector3 rotation = new Vector3(), float scale = 1f)
		{
			meshModel = model;
			defaultPosition = this.position = position;
			defaultRotation = this.rotation = rotation;
			defaultScale = this.scale = scale;
		}

		// Draw this model using properties of the scene and the camera.
		public void Draw(Scene scene, Camera camera)
		{
			Matrix World = Matrix.Identity * TransformationMatrix;
			Matrix View = camera.ViewMatrix;
			Matrix Projection = camera.ProjectionMatrix;

			Matrix WorldViewProjection = Matrix.Multiply(World, Matrix.Multiply(View, Projection));

			foreach (ModelMesh mesh in MeshModel.Meshes)
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
	}
}
