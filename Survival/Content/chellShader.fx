#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix World;
matrix ViewProjection;

bool UseTexture = false;

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
	float4 Color : COLOR0;
	float2 TextureUVs : TEXCOORD0;
	float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureUVs : TEXCOORD0;
	float3 Normal : TEXCOORD1;
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
	output.Color = input.Color;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float3 color = input.Color;
	float3 AmbientColor = float3(0.5, 0.5, 0.5);
	float3 DiffuseColor = float3(1.0, 1.0, 1.0);
	float3 LightDirection = float3(1.0, 1.0, 1.0);
	float3 LightColor = float3(0.9, 0.9, 0.9);

	color = color * DiffuseColor;
	if (UseTexture == true) {
		color = color * tex2D(TextureSampler, input.TextureUVs);
	}

	// Berechne die diffuse Beleuchtung
	float3 normal = normalize(input.Normal);
	float3 lightDir = normalize(LightDirection);
	float NdotL = saturate(dot(normal, lightDir));
	float3 diffuse = NdotL * LightColor;

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
	float3 output = saturate(diffuse * celShade) * color;

	// Füge Umgebungslicht hinzu
	output += AmbientColor * color;

	return float4(output, 1);
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
