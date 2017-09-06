sampler TextureSampler : register(s0);

float2 Offsets[15];
float  Weights[15];


float4 PS(float4 position : SV_Position, float4 color : COLOR0, float2 texcoord : TEXCOORD0) : COLOR0
{
	float4 output = float4(0,0,0,1);

	// iterate over the samples to get the final pixel color at texel coordinate uv, 
	// averaged with the offsets and weights for that texel
	for (int i = 0; i < 15; ++i)
	{
		output += tex2D(TextureSampler, texcoord + Offsets[i]) * Weights[i];
	}

	return output;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_4_0 PS();
	}
}
