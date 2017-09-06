float4x4 WorldViewProjection;
float3x3 WorldIT;

// these matrices are used to render the texture from the projector's POV
float4x4 ProjectorViewProjection;

texture2D ProjectedTexture;
sampler2D ProjectorSampler = sampler_state { texture = <ProjectedTexture>; };

// Projector location
float3 ProjectorPosition : POSITION;

// Material properties
float3 DiffuseColor	: COLOR0;
float3 AmbientColor	: COLOR1;

float AmbientIntensity;


struct VSIN
{
	float4 Position : POSITION;
	float3 Normal : NORMAL;
};

struct VSOUT
{
	float4 Position : POSITION;
	float3 Normal : TEXCOORD0;
	float4 PPos : TEXCOORD1;
};

VSOUT VS(VSIN input)
{
	VSOUT output;

	// do the usual matrix multiplications for vertex position and normal
	output.Position	= mul(input.Position, WorldViewProjection);
	output.Normal = normalize(mul(input.Normal, WorldIT));
	
	// do a second multiplication for the position the way the projector sees it
	output.PPos	= mul(input.Position, ProjectorViewProjection);

	return output;
}

float4 PS(VSOUT input) : COLOR
{
	float4 output;

	// the +1.0 will become +0.5 after the multiplication with 0.5
	float UVx = 0.5f * ((input.PPos.x / input.PPos.w) + 1.0);
	float UVy = 0.5f * ((-input.PPos.y / input.PPos.w) + 1.0);
	// note that these operations could've been performed by a matrix as well
	
	float diffuseLight = saturate(dot(input.Normal, normalize(ProjectorPosition)));
	float3 ambient = AmbientColor * AmbientIntensity;
	float3 diffuse = diffuseLight * DiffuseColor;
	
	// manually prevent texture from wrapping
	float3 texel = float3(0.0f, 0.0f, 0.0f);

	// only use texel if uv coordinates are in range of the projection
	if (UVx >= 0.0f && UVx <= 1.0f && UVy >= 0.0f && UVy <= 1.0f)
		texel = tex2D(ProjectorSampler, float2(UVx, UVy)).rgb * diffuseLight;

	output.rgb = saturate(ambient + diffuse + texel);
	output.a = 1.0f;

	return output;
}

technique ProjectiveTextureMapping
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 VS();
		PixelShader = compile ps_4_0 PS();
	}
}
