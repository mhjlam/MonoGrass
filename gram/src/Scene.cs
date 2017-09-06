using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace gram3
{
	// define the available scenes
	public enum SceneID
	{
		Lambertian,
		Phong,
		Normals,
		CookTorrance,
		SpotLight,
		MultiLight,
		FrustumCulling,
		Monochrome,
		GaussianBlur,
		ProjectiveTexture
	};

	public struct Scene
	{
		public SceneID Id;
		public Vector3 Eye;
		public List<Model> Models;
		public Shader Shader;
		public PostProcess PostProcess;

		public string SceneTitle
		{
			get
			{
				switch (Id)
				{
					case SceneID.Lambertian: return "Lambertian";
					case SceneID.Phong: return "Phong";
					case SceneID.Normals: return "Normals";
					case SceneID.CookTorrance: return "Cook-Torrance";
					case SceneID.SpotLight: return "Spotlight";
					case SceneID.MultiLight: return "Multiple Lights";
					case SceneID.FrustumCulling: return "Frustrum Culling";
					case SceneID.Monochrome: return "Post-Processing: Monochrome";
					case SceneID.GaussianBlur: return "Post-Processing: Gaussian Blur";
					case SceneID.ProjectiveTexture: return "Projective Texture Mapping";
					default: return "Unknown scene ID!";
				}
			}
		}
	}
}
