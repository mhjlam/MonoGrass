using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gram3
{
    public class AmbientMaterial
    {
        public Color AmbientColor;
        public float AmbientIntensity;
    }

    public class SolidMaterial : AmbientMaterial
    {
        public Color SolidColor;
    }

    public class NormalMaterial : AmbientMaterial
    {
        
    }

    public class LambertianMaterial : AmbientMaterial
    {
        public Color DiffuseColor;
        public float DiffuseIntensity;
    }

    public class PhongMaterial : LambertianMaterial
    {
        public Color SpecularColor;
        public float SpecularIntensity;
        public float SpecularPower;
    }

    public class CookTorranceMaterial : AmbientMaterial
    {
        public Color DiffuseColor;
        public float DiffuseIntensity;
        public Color SpecularColor;
        public float SpecularIntensity;
        public float SpecularPower;
        public float Roughness;
        public float ReflectanceCoefficient;
    }
}
