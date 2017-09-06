using Microsoft.Xna.Framework;

namespace gram3
{
	public class Camera
	{
		private Matrix viewMatrix;
		private Matrix projectionMatrix;

		private Vector3 currentPosition;
		private Vector3 currentTarget;
		private Vector3 defaultPosition;
		private Vector3 defaultTarget;

		public Vector3 Position => currentPosition;
		public Matrix ViewMatrix => viewMatrix;

		public Matrix ProjectionMatrix
		{
			get { return projectionMatrix; }
			set { projectionMatrix = value; }
		}

		public Camera(Vector3 position, Vector3 lookAt, float aspectRatio = 1.25f)
		{
			// Save default position and target
			defaultPosition = position;
			defaultTarget = lookAt;

			// Create a projection matrix
			projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1.0f, 100.0f); // field of view of the camera (view angle, aspect ratio, near, far)

			// Reset the camera, setting it to the default position and target
			Reset();
		}

		// Apply an arbitrary transformation matrix to the view matrix.
		public void Transform(Matrix transformationMatrix)
		{
			viewMatrix = Matrix.Multiply(viewMatrix, transformationMatrix);
			Vector3.Transform(currentPosition, transformationMatrix);
		}

		// Translate the camera.
		public void MoveTo(Vector3 position, bool isDefault = false)
		{
			this.currentPosition = position;
			viewMatrix = Matrix.CreateLookAt(position, currentTarget, Vector3.UnitY);
			if (isDefault) defaultPosition = position;
		}

		// Modify the gaze vector.
		public void LookAt(Vector3 target, bool isDefault = false)
		{
			currentTarget = target;
			viewMatrix = Matrix.CreateLookAt(currentPosition, target, Vector3.UnitY);
			if (isDefault) defaultTarget = target;
		}
		
		public void Reset()
		{
			LookAt(defaultTarget);
			MoveTo(defaultPosition);
		}
	}
}
