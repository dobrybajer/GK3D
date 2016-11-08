#define NUM_LIGHTS 4

#define POINT_LIGHT 0
#define DIRECTIONAL_LIGHT 1
#define SPOT_LIGHT 2

float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;

float4 AmbientColor;
float AmbientIntensity;

float	LightType[NUM_LIGHTS];
float	LightEnabled[NUM_LIGHTS];
float3	LightDirection[NUM_LIGHTS];
float3	LightPosition[NUM_LIGHTS];
float4	DiffuseColors[NUM_LIGHTS];
float	DiffuseIntensities[NUM_LIGHTS];
float4	SpecularColors[NUM_LIGHTS];
float	SpecularIntensities[NUM_LIGHTS];
float	SpecularPower[NUM_LIGHTS];
float	SpotAngle[NUM_LIGHTS];

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float3 Normal : TEXCOORD0;
	float3 ViewDirection : TEXCOORD1;
	float3 WorldPosition : TEXCOORD2;
};

float DoAttenuation(float d)
{
	float att = 1.0f / (0.8f + 0.001 * d + 0.00001 * d * d);
	return att > 1 ? 0 : att;
}

float4 DoDiffuse(int i, float3 N, float3 L)
{
	float diffuseLighting = saturate(dot(N, L));
	return DiffuseColors[i] * DiffuseIntensities[i] * diffuseLighting;
}

float4 DoSpecular(int i, float3 V, float3 L, float3 N)
{
	float3 R = normalize(reflect(-L, N));
	float RdotV = saturate(dot(R, V));

	return SpecularColors[i] * SpecularIntensities[i] * pow(RdotV, SpecularPower[i]);
}

float4 DoPointLight(int i, float3 P, float3 N, float3 V)
{
	float3 L = (LightPosition[i] - P).xyz;
	float distance = length(L);
	L = L / distance;

	return DoDiffuse(i, N, L) +	DoSpecular(i, V, L, N);
}

float4 DoDirectionalLight(int i, float3 P, float3 N, float3 V)
{
	float3 L = LightDirection[i];
	return DoDiffuse(i, N, L) +	DoSpecular(i, V, L, N);
}

float DoSpotCone(int i, float3 L)
{
	float spotMinAngle = cos(SpotAngle[i]);
	float spotMaxAngle = (spotMinAngle + 1.0f) / 2.0f;
	float cosAngle = dot(LightDirection[i], L);
	return cosAngle > spotMinAngle && cosAngle < spotMaxAngle ? cosAngle : spotMinAngle;
}

float4 DoSpotLight(int i, float3 P, float3 N, float3 V)
{
	float3 L = (LightPosition[i] - P).xyz;
	float distance = length(L);
	L = L / distance;

	float attenuation = DoAttenuation(distance);
	float spotIntensity = DoSpotCone(i, -L);

	return DoDiffuse(i, N, L) * attenuation * spotIntensity +
		   DoSpecular(i, V, L, N) * attenuation *spotIntensity;
}

float4 CalculateLights(VertexShaderOutput input)
{
	float3 P = input.WorldPosition;
	float3 N = input.Normal;
	float3 V = normalize(input.ViewDirection).xyz;

	float4 outColor = float4(0, 0, 0, 0);
	for (int i = 0; i < NUM_LIGHTS; i++)
	{
		if (LightEnabled[i] != 1.0f)
			continue;

		if (LightType[i] == POINT_LIGHT)
		{
			outColor += DoPointLight(i, P, N, V);
		}
		if (LightType[i] == DIRECTIONAL_LIGHT)
		{
			outColor += DoDirectionalLight(i, P, N, V);
		}
		if (LightType[i] == SPOT_LIGHT)
		{
			outColor += DoSpotLight(i, P, N, V);
		}
	}

	return outColor;
}

VertexShaderOutput MainVertexShader(VertexShaderInput input)
{
	VertexShaderOutput output;

	input.Position.w = 1;

	float4 worldPosition = mul(input.Position, World);
	float4x4 viewProjection = mul(View, Projection);

	output.Position = mul(worldPosition, viewProjection);
	output.Normal = normalize(mul(input.Normal, World)).xyz;
	output.ViewDirection = (float3)worldPosition - CameraPosition;
	output.WorldPosition = (float3)worldPosition;

	return output;
}

float4 MainPixelShader(VertexShaderOutput input) : Color0
{
	return saturate(CalculateLights(input) + AmbientColor * AmbientIntensity);
}

technique BasicPhongLightning
{
	pass Pass1
	{
		//AlphaBlendEnable = TRUE;
		//DestBlend = INVSRCALPHA;
		//SrcBlend = SRCALPHA;

		VertexShader = compile vs_3_0 MainVertexShader();
		PixelShader = compile ps_3_0 MainPixelShader();
	}
};