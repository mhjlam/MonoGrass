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

VSOUT PhongVS(VSIN input)
{
	VSOUT output;
	output.Position = mul(input.Position, WorldViewProjection);
	output.Normal = normalize(mul(input.Normal, WorldIT));
	output.WorldPos = mul(input.Position.xyz, World);
	return output;
}
 
float4 PhongPS(VSOUT input) : COLOR
{
	float3 viewdir = normalize(CameraPosition - input.WorldPos);
	float3 lightdir = normalize(LightPosition - input.WorldPos);
	float3 halfway = normalize(lightdir + viewdir);
	
	float3 ambient = saturate((AmbientColor.rgb * AmbientIntensity).rgb);
	float3 diffuse = saturate(dot(input.Normal, lightdir) * DiffuseColor.rgb);
	float3 specular = saturate(pow(saturate(dot(input.Normal, halfway)), SpecularPower));

	return float4(saturate(ambient + diffuse + (specular * SpecularColor * SpecularIntensity)), 1.0);
}

technique Phong
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 PhongVS();
		PixelShader  = compile ps_4_0 PhongPS();
	}
}
