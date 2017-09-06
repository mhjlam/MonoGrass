float4x4 WorldViewProjection;
float3x3 WorldIT;

float3 AmbientColor : COLOR0;
float1 AmbientIntensity;

struct VSIN
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
};

struct VSOUT
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
};

VSOUT NormalsVS(VSIN input)
{
	VSOUT output;

	float3 normal = normalize(mul(input.Normal, WorldIT));
	output.Position = mul(input.Position, WorldViewProjection);
	output.Color = float4(0.5 + (0.5 * normal), 1.0);

	return output;
}
 
float4 NormalsPS(VSOUT input) : COLOR
{
	return float4((AmbientColor * AmbientIntensity) + input.Color.rgb, 1.0);
}

technique Normals
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 NormalsVS();
		PixelShader  = compile ps_4_0 NormalsPS();
	}
}
