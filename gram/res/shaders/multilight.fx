float4x4 WorldViewProjection;
float4x4 World;
float3x3 WorldIT;

// the smallest of these two values will be used in the iteration below
#define MAX_LIGHTS 5
int LightCount;

float3 CameraPosition				: POSITION0;
float3 LightDirections[MAX_LIGHTS]	: POSITION1;

float3 AmbientColor					: COLOR0;
float3 SpecularColor				: COLOR2;
float3 LightColors[MAX_LIGHTS]		: COLOR3;	// occupies COLOR[3] to COLOR[3+MAX_LIGHTS]

float AmbientIntensity;
float SpecularIntensity;
float SpecularPower;


struct VSIN
{
	float4 Position : POSITION;
	float3 Normal	: NORMAL;
};

struct VSOUT
{
	float4 Position	  : POSITION;
	float3 Normal	  : TEXCOORD0;
	float3 CameraView : TEXCOORD1;
};

VSOUT MultiLightVS(VSIN input)
{
	VSOUT output;

	output.Position	= mul(input.Position, WorldViewProjection);
	output.Normal = mul(input.Normal, WorldIT);
	output.CameraView = CameraPosition - mul(input.Position, World).xyz;

	return output;
}

float4 MultiLightPS(VSOUT input) : COLOR
{
	float4 output;

	float3 normal = normalize(input.Normal);
	
	// ambient
	float3 ambient = AmbientColor * AmbientIntensity;

	// total diffuse and specular shading vectors
	float3 diffuse = float3(0,0,0);
	float3 specular = float3(0,0,0);

	for (int i = 0; i < min(LightCount, MAX_LIGHTS); ++i)
	{
		// normalize the light direction vector
		float3 lightdir = normalize(LightDirections[i]);

		// calculate the halfway vector according to Blinn-Phong algorithm
		float3 halfway  = normalize(lightdir + normalize(input.CameraView));

		// total diffuse and specular values are a summation for all lights
		diffuse += saturate(dot(lightdir, normal)) * normalize(LightColors[i]);
		specular += pow(saturate(dot(normal, halfway)), SpecularPower) * normalize(LightColors[i]);
	}

	output.rgb = ambient + diffuse + (saturate(specular) * SpecularColor * SpecularIntensity);
	output.a = 1;

	return output;
}

technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 MultiLightVS();
		PixelShader = compile ps_4_0 MultiLightPS();
	}
}
