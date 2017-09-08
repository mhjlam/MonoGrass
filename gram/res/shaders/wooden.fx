float4x4 WorldViewProjection;
float3x3 WorldIT;
float3x3 World;

float3 LightPosition : POSITION0;
float3 CameraPosition : POSITION1;
float3 AmbientColor : COLOR0;
float1 AmbientIntensity;
float3 DiffuseColor : COLOR1;
float3 SpecularColor : COLOR2;
float1 SpecularIntensity;
float1 SpecularPower;

Texture2D Texture;

SamplerState Sampler
{
	Filter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

struct VSIN
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 Texcoord : TEXCOORD0;
};

struct VSOUT
{
	float4 Position : POSITION0;
	float3 Normal : TEXCOORD0;
	float2 Texcoord : TEXCOORD1;
	float3 WorldPos : POSITION1;
};


VSOUT WoodVS(VSIN input)
{
	VSOUT output;
	output.Position = mul(input.Position, WorldViewProjection);
	output.Normal = normalize(mul(input.Normal, WorldIT));
	output.Texcoord = input.Texcoord;
	output.WorldPos = mul(input.Position.xyz, World);
	return output;
}

float4 WoodPS(VSOUT input) : COLOR
{
	float3 viewdir = normalize(CameraPosition - input.WorldPos);
	float3 lightdir = normalize(LightPosition - input.WorldPos);
	float3 halfway = normalize(lightdir + viewdir);

	float3 ambient = saturate((AmbientColor.rgb * AmbientIntensity).rgb);
	float3 diffuse = saturate(dot(input.Normal, lightdir) * DiffuseColor.rgb);
	float3 specular = saturate(pow(saturate(dot(input.Normal, halfway)), SpecularPower));

	float3 texel = Texture.Sample(Sampler, input.Texcoord).rgb;

	return float4(saturate(ambient + texel + (specular * SpecularColor * SpecularIntensity)), 1.0);
}

technique Wood
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 WoodVS();
		PixelShader = compile ps_4_0 WoodPS();
	}
}
