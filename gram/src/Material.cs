using Microsoft.Xna.Framework;

namespace gram3
{
	public class AmbientMaterial
	{
		public Color AmbientColor;
		public float AmbientIntensity;
	}

	public class SolidMaterial
	{
		public Color AmbientColor;
		public float AmbientIntensity;
		public Color SolidColor;
	}

	public class NormalMaterial
	{
		public Color AmbientColor;
		public float AmbientIntensity;
	}

	public class LambertianMaterial
	{
		public Color AmbientColor;
		public float AmbientIntensity;
		public Color DiffuseColor;
		public float DiffuseIntensity;
	}

	public class PhongMaterial
	{
		public Color AmbientColor;
		public float AmbientIntensity;
		public Color DiffuseColor;
		public float DiffuseIntensity;
		public Color SpecularColor;
		public float SpecularIntensity;
		public float SpecularPower;
	}

	public class CookTorranceMaterial
	{
		public Color AmbientColor;
		public float AmbientIntensity;
		public Color DiffuseColor;
		public float DiffuseIntensity;
		public Color SpecularColor;
		public float SpecularIntensity;
		public float SpecularPower;
		public float Roughness;
		public float ReflectanceCoefficient;
	}
}
