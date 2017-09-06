float4x4 WorldViewProjection;
float3x3 WorldIT;
float3x3 World;

float3 LightPosition : POSITION0;
float3 LightDirection : POSITION1;

float3 AmbientColor : COLOR0;
float3 DiffuseColor : COLOR1;
float3 LightColor : COLOR2;

float AmbientIntensity;
float InnerAngle;		// fully lit
float OuterAngle;		// linear dropoff from 1.0 to 0.0 from inner angle to here


struct VSIN
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
};

struct VSOUT
{
	float4 Position : POSITION0;
	float3 Normal : TEXCOORD0;
	float3 WorldPos : POSITION1;
};


VSOUT SpotLightVS(VSIN input)
{
	VSOUT output;
	output.Position	= mul(input.Position, WorldViewProjection);
	output.Normal = normalize(mul(input.Normal, WorldIT));
	output.WorldPos	= mul(input.Position.xyz, World);
	return output;
}

float4 SpotLightPS(VSOUT input) : COLOR
{
	float4 output = float4(0,0,0,1);

	// ambient
	float3 ambient = saturate((AmbientColor.rgb * AmbientIntensity).rgb);

	// diffuse
	float3 diffuse = saturate(dot(input.Normal, normalize(LightPosition)) * DiffuseColor.rgb);

	output.rgb = saturate(ambient + diffuse);

	float3 lightdir = LightPosition - input.WorldPos;
	float1 lightdist = length(lightdir);
	lightdir /= lightdist;

	float1 spotangle = dot(normalize(LightDirection), -lightdir);

	if (spotangle > 0.99999884)
	{
		float1 curve = min(pow(lightdist / 10.0, 6.0), 1.0);
		float1 attenuation = (1.0 - curve) * (1.0 / (1.0 + 0.008 * lightdist*lightdist));
		float1 falloff = saturate((spotangle - 0.99999884) / 0.05);

		float3 spotlight = LightColor * attenuation * falloff;
		
		output.rgb = saturate(ambient + spotlight * diffuse);
	}

	return output;
}

technique SpotLight
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 SpotLightVS();
		PixelShader = compile ps_4_0 SpotLightPS();
	}
}
