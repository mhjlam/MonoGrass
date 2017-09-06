float4x4 WorldViewProjection;
float3x3 WorldIT;

float3 CameraPosition : POSITION0;
float3 LightPosition : POSITION1;

float3 DiffuseColor : COLOR0;
float3 AmbientColor : COLOR1;
float3 SpecularColor : COLOR2;

float AmbientIntensity;
float SpecularIntensity;
float SpecularPower;
float Roughness;
float R0;


struct VSIN
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
};

struct VSOUT
{
	float4 Position : POSITION0;
	float3 Normal : TEXCOORD0;
};


VSOUT CookTorranceVS(VSIN input)
{
	VSOUT output;

	output.Position	= mul(input.Position, WorldViewProjection);
	output.Normal = normalize(mul(input.Normal, WorldIT));

	return output;
}

float4 CookTorrancePS(VSOUT input) : COLOR
{
	float4 output;

	// normalized light direction vector
	float3 L = normalize(LightPosition);	// from the object to the light
	// our eye direction vector in normalized form
	float3 E = normalize(CameraPosition);	// from the object to the eye
	// the half-way vector
	float3 H = normalize(L + E);

	// we'll be using this a few times, so define up front (note that dot product is commutative)
	float HdotN = saturate(dot(input.Normal, H));	// prevent negative dot product (would light the other side of the model as if light went straight through)

	// D is the Beckmann distribution
	float a = acos(HdotN);							// compute the angle by using the dot product (since H and N are unit vectors this is as simple as using cos-1)
	float m2 = pow(Roughness, 2.0f);				// represents the rms (quadratic mean) slope of the surface microfacets (the roughness of the material)
	float D = exp(-pow(tan(a), 2.0f) / m2) / (3.14 * m2 * pow(cos(a), 4));

	// G is the geometric attenuation term which describes the selfshadowing based on the microfacets
	float EdotH = dot(E, H);						// used only here
	float G1 = (2 * HdotN * dot(E, input.Normal)) / EdotH;
	float G2 = (2 * HdotN * dot(L, input.Normal)) / EdotH;
	float G = min(1.0f, min(G1, G2));

	// F is the Fresnel Term based on Schlick's approximation
	// R0 is the reflectance at normal incidence (i.e. the value of the Fresnel term when the reflection angle equals 0).
	float F = R0 + (1.0f - R0) * pow(1.0f - dot(input.Normal, L), 5.0f);	// cos(acos(dot(input.Normal, L))) is the same as just the dot product

	// the specular form is defined as DFG / E dot N
	float3 kSpec = saturate((D * F * G) / dot(E, input.Normal));

	// finalize
	float3 ambient = saturate((AmbientColor.rgb * AmbientIntensity).rgb);
	float3 diffuse = saturate(dot(input.Normal, L) * DiffuseColor.rgb);
	float3 specular = pow(kSpec, SpecularPower) * SpecularIntensity;
	
	// depending the material properties it may occur that the specular reaches the back of the head and we want to prevent this
	specular *= saturate(dot(input.Normal, L));	// using the same method as diffuse
	
	output.rgb = saturate(ambient + diffuse + (specular * SpecularColor));	// clamp
	output.a = 1.0f;
	
	return output;
}

technique CookTorrance
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 CookTorranceVS();
		PixelShader = compile ps_4_0 CookTorrancePS();
	}
}
