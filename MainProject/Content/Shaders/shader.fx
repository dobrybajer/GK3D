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

float ClippingPlane;
float Side;

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

float	FogEnabled;
float	FogStart;
float	FogEnd;

/* Texture */
float4x4 TextureMatrix;

texture Texture;
sampler2D TextureSampler : register(s0)
= sampler_state {
	Texture = (Texture);
};

texture Texture1;
sampler2D Texture1Sampler : register(s1)
= sampler_state {
	Texture = (Texture1);
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL;
	float4 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float3 Normal : TEXCOORD0;
	float4 TextureCoordinate : TEXCOORD1;
	float3 ViewDirection : TEXCOORD2;
	float3 WorldPosition : TEXCOORD3;
	float FogFactor : TEXCOORD4;
};

/*----------------------------------------------------------------*/
/*--------------------- Functions - Lights -----------------------*/
/*----------------------------------------------------------------*/
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

/*----------------------------------------------------------------*/
/*---------------------- Functions - Fog -------------------------*/
/*----------------------------------------------------------------*/
float ComputeFogFactor(float d)
{
	return clamp((d - FogStart) / (FogEnd - FogStart), 0, 1);
	//return saturate((d - FogStart) / (FogEnd - FogStart));
}

/*----------------------------------------------------------------*/
/*------------------ Stage 1 - Shader functions ------------------*/
/*----------------------------------------------------------------*/

VertexShaderOutput MainVertexShader(VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	input.Position.w = 1;

	float4 worldPosition = mul(input.Position, World);
	float4x4 viewProjection = mul(View, Projection);

	output.Position = mul(worldPosition, viewProjection);
	output.Normal = normalize(mul(input.Normal, (float3x3)World)).xyz;
	output.ViewDirection = (float3)worldPosition - CameraPosition;
	output.WorldPosition = (float3)worldPosition;

	return output;
}

float4 MainPixelShader(VertexShaderOutput input) : Color0
{
	return saturate(CalculateLights(input) + AmbientColor * AmbientIntensity);
}

/*----------------------------------------------------------------*/
/*-------------- Stage 2 Textured  Shader functions---------------*/
/*----------------------------------------------------------------*/

VertexShaderOutput VertexShaderTexturedFunction(VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	input.Position.w = 1;

	float4 worldPosition = mul(input.Position, World);
	float4x4 viewProjection = mul(View, Projection);

	output.Position = mul(worldPosition, viewProjection);
	output.Normal = normalize(mul(input.Normal, (float3x3)World)).xyz;
	output.ViewDirection = (float3)worldPosition - CameraPosition;
	output.WorldPosition = (float3)worldPosition;

	input.TextureCoordinate.w = 1;
	output.TextureCoordinate = mul(input.TextureCoordinate, TextureMatrix);
	output.FogFactor = ComputeFogFactor(length(output.ViewDirection));

	return output;
}

float4 PixelShaderTexturedFunction(VertexShaderOutput input) : Color0
{
	if (Side > 0)
		if (input.WorldPosition.z < ClippingPlane)
			return float4(1, 1, 1, 0);

	if (Side < 0)
		if (input.WorldPosition.z > ClippingPlane)
			return float4(1, 1, 1, 0);

	float4 textureColor = tex2Dbias(TextureSampler, input.TextureCoordinate);
	float alpha = textureColor.a;

	float4 outColor = CalculateLights(input);
	float4 fogColor = float4(0.2f, 0.2f, 0.2f, 1.0f);
	float4 finalTextureColor = textureColor + outColor + AmbientColor * AmbientIntensity;
	
	return saturate(FogEnabled ? 
		input.FogFactor * fogColor + (1.0 - input.FogFactor) * finalTextureColor :
		finalTextureColor);
}


/*----------------------------------------------------------------*/
/*----------------- PixelShader MultiTextured --------------------*/
/*----------------------------------------------------------------*/

float4 PixelShaderMultiTexturedFunction(VertexShaderOutput input) : Color0
{
	if (Side > 0)
		if (input.WorldPosition.z < ClippingPlane)
			return  float4(1, 1, 1, 0);

	if (Side < 0)
		if (input.WorldPosition.z > ClippingPlane)
			return float4(1, 1, 1, 0);

	float4 outColor = CalculateLights(input);

	float4 secondTextureColor = tex2Dbias(Texture1Sampler, input.TextureCoordinate);
	float alpha = secondTextureColor.a;
	float4 textureColor = tex2Dbias(TextureSampler, input.TextureCoordinate);

	float4 texturesCompositionColor = alpha * secondTextureColor + (1 - alpha) * textureColor;
	float4 fogColor = float4(0.2f, 0.2f, 0.2f, 1.0f);
	float4 finalTextureColor = texturesCompositionColor + outColor + AmbientColor * AmbientIntensity;

	return saturate(FogEnabled ? 
		input.FogFactor * fogColor + (1.0 - input.FogFactor) * finalTextureColor :
		finalTextureColor);
}

/*----------------------------------------------------------------*/
/*---------------------- Techniques ------------------------------*/
/*----------------------------------------------------------------*/

technique BasicPhongLightning
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 MainVertexShader();
		PixelShader = compile ps_3_0 MainPixelShader();
	}
};

technique Textured
{
	pass Pass1
	{
		DestBlend = INVSRCALPHA;
		SrcBlend = SRCALPHA;

		VertexShader = compile vs_3_0 VertexShaderTexturedFunction();
		PixelShader = compile ps_3_0 PixelShaderTexturedFunction();
	}
}

technique MultiTextured
{
	pass Pass1
	{
		DestBlend = INVSRCALPHA;
		SrcBlend = SRCALPHA;

		VertexShader = compile vs_3_0 VertexShaderTexturedFunction();
		PixelShader = compile ps_3_0 PixelShaderMultiTexturedFunction();
	}
}