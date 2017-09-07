using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XnaModel = Microsoft.Xna.Framework.Graphics.Model;

namespace gram3
{
    public class Model
    {
        private Vector3 defaultPosition;
        private float defaultRotation;

        private float rotation;
        private Vector3 position;
        private XnaModel xnaModel;

        public float Rotation => rotation;
        public Vector3 Position => position;
        public XnaModel XnaModel => xnaModel;

        public Matrix RotationMatrix => Matrix.CreateRotationY(Rotation);
        public Matrix TranslationMatrix => Matrix.CreateTranslation(Position);
        public Matrix TransformationMatrix => RotationMatrix * TranslationMatrix;

        public void Rotate(float r) => rotation += r;
        public void Translate(Vector3 t) => position += t;
        public void ResetPosition() => position = defaultPosition;
        public void ResetRotation() => rotation = defaultRotation;


        // Model3D describes an XNA model and manages some of its properties.
        public Model(XnaModel model, Vector3 modelPosition, float modelRotation = 0f)
        {
            xnaModel = model;
            defaultPosition = position = modelPosition;
            defaultRotation = rotation = modelRotation;
        }

        // Draw this model using properties of the scene and the camera.
        public void Draw(Scene scene, Camera camera)
        {
            Matrix World = Matrix.Identity * TransformationMatrix;
            Matrix View = camera.ViewMatrix;
            Matrix Projection = camera.ProjectionMatrix;

            Matrix WorldViewProjection = Matrix.Multiply(World, Matrix.Multiply(View, Projection));

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
    }
}
