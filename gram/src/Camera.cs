using Microsoft.Xna.Framework;

namespace gram
{
	public class Camera
	{
		private float defaultAspect;
		private Matrix view;
		private Matrix proj;

		private Vector3 position;
		private Vector3 target;
		private Vector3 defaultPosition;
		private Vector3 defaultTarget;

		public Vector3 Position => position;
		public Matrix ViewMatrix => view;
		public Matrix ProjectionMatrix => proj;

		public Camera(Vector3 position, Vector3 lookAt, float aspectRatio = 1.0f)
		{
			defaultAspect = aspectRatio;
			defaultPosition = position;
			defaultTarget = lookAt;

			// Reset the camera, setting it to the default position and target
			Reset();
		}

		// Apply an arbitrary transformation matrix to the view matrix.
		public void Transform(Matrix transformationMatrix)
		{
			view = Matrix.Multiply(view, transformationMatrix);
			Vector3.Transform(position, transformationMatrix);
		}

		// Translate the camera.
		public void MoveTo(Vector3 position, bool isDefault = false)
		{
			this.position = position;
			view = Matrix.CreateLookAt(position, target, Vector3.UnitY);
			if (isDefault) defaultPosition = position;
		}

		// Modify the gaze vector.
		public void LookAt(Vector3 target, bool isDefault = false)
		{
			this.target = target;
			view = Matrix.CreateLookAt(position, target, Vector3.UnitY);
			if (isDefault) defaultTarget = target;
		}
		
		public void Reset()
		{
			LookAt(defaultTarget);
			MoveTo(defaultPosition);

			// field of view of the camera (view angle, aspect ratio, near, far)
			proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, defaultAspect, 1.0f, 500.0f);
		}
	}
}
