#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix WorldViewProjection;
matrix World;

float3 LightPosition = float3(0, 0, 0);

bool Unlit = true;
bool HasNormalMap = true;

Texture2D Texture : register(t0);
Texture2D NormalMap : register(t1);

sampler TextureSampler : register(s0)
{
	Texture = (Texture);
	MinFilter = Point; // Minification Filter
    MagFilter = Point;// Magnification Filter
    MipFilter = Linear; // Mip-mapping
	AddressU = Wrap; // Address Mode for U Coordinates
	AddressV = Wrap; // Address Mode for V Coordinates
};

sampler NormalMapSampler : register(s1)
{
    Texture = (NormalMap);
    MinFilter = Anisotropic; // Minification Filter
    MagFilter = Anisotropic;// Magnification Filter
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
	float4 Color : COLOR0;
	float3 Normal : NORMAL0;
	float2 TextureUVs : TEXCOORD0;
	float3 LightDirection : TEXCOORD1;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = mul(input.Position, WorldViewProjection);
	output.Color = float4(1, 1, 1, 1);
	output.Normal = mul(input.Normal, World);
	output.TextureUVs = input.TextureUVs;
	output.LightDirection = LightPosition - mul(input.Position, World);
	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{

    float2 calculatedUVs = input.TextureUVs;
	
	float3 N = normalize(input.Normal);
    float3 L = normalize(input.LightDirection);
	
    float4 diffuseColor = input.Color * Texture.Sample(TextureSampler, calculatedUVs);
     
	if (HasNormalMap == true) {
	    float3 normalMap = tex2D(NormalMapSampler, calculatedUVs);
	    N = normalMap;
    }
    
    clip(diffuseColor.a < 0.75f ? -1 : 1);
    
    if (Unlit == false) {
	    float diffuse = saturate(dot(N, L));
        return diffuseColor * diffuse;
    }
	return diffuseColor;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		ZEnable = true;
		FillMode = Solid;
		ZWriteEnable = true;
		CullMode = None;
		AlphaBlendEnable = false;
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};