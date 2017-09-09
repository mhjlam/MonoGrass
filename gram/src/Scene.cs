using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace gram
{
	// Define available scenes
	public enum SceneID
	{
		Terrain,
		Lambertian,
		Phong,
		Normals,
		Checkers,
		Wood,
		CookTorrance,
		Spotlight,
		Multilight,
		Culling,
		Projection,
		Monochrome,
		GaussianBlur
	};

	public struct Scene
	{
		public SceneID Id;
		public Vector3 Eye;
		public List<Model> Models;
		public Shader Shader;
		public Filter PostProcess;

		public string SceneTitle
		{
			get
			{
				switch (Id)
				{
					case SceneID.CookTorrance: return "Cook-Torrance";
					case SceneID.Multilight: return "Multiple Lights";
					case SceneID.Projection: return "Projective Texture Mapping";
					case SceneID.Monochrome: return "Post-Processing: Monochrome";
					case SceneID.GaussianBlur: return "Post-Processing: Gaussian Blur";
					default: return Id.ToString();
				}
			}
		}
	}
}
