using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace gram
{
	public class Quadrilateral
	{
		public int[] indices;
		public VertexPositionNormalTexture[] vertices;

		public Quadrilateral(Vector3 tl, Vector3 tr, Vector3 br, Vector3 bl)
		{
			vertices = new VertexPositionNormalTexture[]
			{
				new VertexPositionNormalTexture(tl, Vector3.Up, new Vector2(0f, 0f)),
				new VertexPositionNormalTexture(tr, Vector3.Up, new Vector2(1f, 0f)),
				new VertexPositionNormalTexture(br, Vector3.Up, new Vector2(0f, 1f)),
				new VertexPositionNormalTexture(bl, Vector3.Up, new Vector2(1f, 1f)),
			};

			// indices in clockwise order
			indices = new int[]
			{
				0, 1, 2,
				2, 3, 0
			};
		}
	}

}
