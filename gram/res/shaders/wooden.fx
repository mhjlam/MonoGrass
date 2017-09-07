float4x4 WorldViewProjection;
float3x3 WorldIT;

Texture2D Texture;

float3 ViewPosition : POSITION0;
float3 LightDirection : POSITION1;

float1 SpecularIntensity;
float1 SpecularPower;
float4 SpecularColor : COLOR0;

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
	float2 Texcoord : TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float3 ViewPosition : TEXCOORD2;
};


VSOUT WoodVS(VSIN input)
{
	VSOUT output;

	output.Position = mul(input.Position, WorldViewProjection);
	output.Texcoord = input.Texcoord;
	output.Normal = normalize(mul(input.Normal, WorldIT));
	output.ViewPosition = ViewPosition - output.Position.xyz;

	return output;
}

float4 WoodPS(VSOUT input) : COLOR
{
	float3 halfway = normalize(-LightDirection + input.ViewPosition);
	float3 specular = pow(saturate(dot(input.Normal, halfway)), SpecularPower) * (SpecularIntensity * 0.3f);
	float3 texel = float3(Texture.Sample(Sampler, input.Texcoord).rg, 0.0f);

	return float4(saturate((texel + specular) * SpecularColor.rgb), 1.0);
}

technique Wood
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 WoodVS();
		PixelShader = compile ps_4_0 WoodPS();
	}
}
