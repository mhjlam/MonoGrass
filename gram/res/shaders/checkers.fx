float4x4 WorldViewProjection;
float3x3 WorldIT;

struct VSIN
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
};

struct VSOUT
{
	float4 Position : POSITION0;
	float3 WorldPosition : TEXCOORD0;
};


VSOUT CheckersVS(VSIN input)
{
	VSOUT output;
	output.Position = mul(input.Position, WorldViewProjection);
	output.WorldPosition = mul(input.Position.xyz, WorldIT);
	return output;
}

float4 CheckersPS(VSOUT input) : COLOR
{
	float x = saturate(sign(sin(3.141592 * input.WorldPosition.x * 50.0)));
	float y = saturate(sign(cos(3.141592 * input.WorldPosition.y * 50.0)));
	float z = saturate(sign(sin(3.141592 * input.WorldPosition.z * 50.0)));
	return (fmod(x + y + z, 2.0)) ? 0.9 : 0.1;
}

technique Checkers
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 CheckersVS();
		PixelShader = compile ps_4_0 CheckersPS();
	}
}
