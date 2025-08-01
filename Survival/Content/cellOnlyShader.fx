﻿#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix World;
matrix ViewProjection;

Texture2D Texture : register(t0);
sampler TextureSampler : register(s0)
{
	Texture = (Texture);
	MinFilter = Point; // Minification Filter
	MagFilter = Point; // Magnification Filter
	MipFilter = Linear; // Mip-mapping
	AddressU = Wrap; // Address Mode for U Coordinates
	AddressV = Wrap; // Address Mode for V Coordinates
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 TextureUVs : TEXCOORD0;
	float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float2 TextureUVs : TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float Depth : TEXCOORD2;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	float4 position = mul(input.Position, World);
	float2 textureUVs = input.TextureUVs;
	float3 normal = mul(input.Normal, World);

	output.Position = mul(position, ViewProjection);
	output.TextureUVs = textureUVs;
	output.Normal = normal;
	output.Depth = output.Position.z / output.Position.w;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float3 LightDirection = float3(1.0, 1.0, 1.0); // Richtung des Lichts
	float3 DiffuseColor = float3(1.0, 1.0, 1.0); // Diffuse Farbe

	// Berechne die diffuse Beleuchtung
	float3 normal = normalize(input.Normal);
	float3 lightDir = normalize(LightDirection);
	float NdotL = saturate(dot(normal, lightDir));

	// Cel-Shading: Definiere Schwellenwerte für die Stufen
	float threshold1 = 0.2;
	float threshold2 = 0.5;

	// Wende Cel-Shading basierend auf der Lichtintensität an
	float celShade = 0.5;
	if (NdotL > threshold2)
	{
		celShade = 1.2;
	}
	else if (NdotL > threshold1)
	{
		celShade = 0.8;
	}

	// Kombiniere diffuse Beleuchtung mit Cel-Shading
	float3 color = DiffuseColor * tex2D(TextureSampler, input.TextureUVs).rgb;
	float3 output = saturate(color * celShade);

	return float4(input.Depth, input.Depth, input.Depth, 1);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
		CullMode = NONE;
	}
};
