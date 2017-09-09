using Microsoft.Xna.Framework;

namespace gram
{
	public class AmbientMaterial
	{
		public Color AmbientColor;
		public float AmbientIntensity;
	}

	public class LambertianMaterial
	{
		public Color AmbientColor;
		public float AmbientIntensity;
		public Color DiffuseColor;
	}

	public class PhongMaterial
	{
		public Color AmbientColor;
		public float AmbientIntensity;
		public Color DiffuseColor;
		public Color SpecularColor;
		public float SpecularIntensity;
		public float SpecularPower;
	}

	public class CookTorranceMaterial
	{
		public Color AmbientColor;
		public float AmbientIntensity;
		public Color DiffuseColor;
		public Color SpecularColor;
		public float SpecularIntensity;
		public float SpecularPower;
		public float Roughness;
		public float ReflectanceCoefficient;
	}
}
