sampler TextureSampler : register(s0);

float4 PS(float4 position : SV_Position, float4 color : COLOR0, float2 texcoord : TEXCOORD0) : COLOR0
{
	float4 output = tex2D(TextureSampler, texcoord);

	// uniform RGB intensity
	output.rgb = output.r * 0.3 + output.g * 0.59 + output.b * 0.11;
	output.a = 1.0;

	return output;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_4_0 PS();
	}
}
