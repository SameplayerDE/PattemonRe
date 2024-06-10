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

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float3 AmbientColor = float3(0.5, 0.5, 0.5);
	float3 DiffuseColor = float3(1.2, 1.2, 1.2);
	float3 LightDirection = float3(1.0, 1.0, 1.0);
	float3 LightColor = float3(0.9, 0.9, 0.9);

	float3 color = DiffuseColor;
	color *= tex2D(TextureSampler, input.TextureUVs);

	float3 lighting = AmbientColor;
	float3 lightDir = normalize(LightDirection);
	float3 normal = normalize(input.Normal);
	
	lighting += saturate(dot(lightDir, normal)) * LightColor;

	float3 output = saturate(lighting) * color;
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