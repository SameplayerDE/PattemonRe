#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix World;
matrix View;
matrix Projection;

float3 LightPosition = float3(10, 10, 10);

float AlphaCutoff;
int AlphaMode;

bool TextureEnabled;

Texture2D Texture : register(t0);
sampler TextureSampler : register(s0)
{
	Texture = (Texture);
};

bool NormalMapEnabled;

Texture2D NormalMap : register(t1);
sampler NormalMapSampler : register(s1)
{
	Texture = (NormalMap);
};

bool OcclusionMapEnabled;

Texture2D OcclusionMap : register(t2);
sampler OcclusionMapSampler : register(s2)
{
    Texture = (OcclusionMap);
};

bool EmissiveTextureEnabled;

Texture2D EmissiveTexture : register(t3);
sampler EmissiveTextureSampler : register(s3)
{
    Texture = (EmissiveTexture);
};

float4 BaseColorFactor = float4(1.0, 1.0, 1.0, 1.0);
float4 EmissiveColorFactor = float4(0.0, 0.0, 0.0, 1);

bool SkinningEnabled;
Matrix Bones[128];

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	float3 Normal : NORMAL0;
	float2 TextureCoordinate : TEXCOORD0;
    float4 BlendIndices : BLENDINDICES0;
    float4 BlendWeight : BLENDWEIGHT0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float3 Normal : NORMAL0;
	float2 TextureCoordinate : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	
	float4 position = input.Position;
    float3 normal = input.Normal;
	float4 color = input.Color;
	float2 textureCoordinate = input.TextureCoordinate;
    
    if (SkinningEnabled == true)
    {
        float4x4 skinMatrix = 
            input.BlendWeight.x * Bones[int(input.BlendIndices.x)] +
            input.BlendWeight.y * Bones[int(input.BlendIndices.y)] +
            input.BlendWeight.z * Bones[int(input.BlendIndices.z)] +
            input.BlendWeight.w * Bones[int(input.BlendIndices.w)];

        position = mul(position, skinMatrix);
        normal = mul(normal, skinMatrix);
    }

    float4 worldPosition = mul(position, World);
    float4 viewPosition = mul(worldPosition, View);
	
    output.Position = mul(viewPosition, Projection);
    output.Normal = mul(normal, World);
    output.Color = color;
    output.TextureCoordinate = textureCoordinate;
    
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 position = input.Position;
    float4 finalColor = input.Color * BaseColorFactor;

    if (TextureEnabled == true)
    {
        float4 textureColor = tex2D(TextureSampler, input.TextureCoordinate);
        finalColor = finalColor * textureColor;
    }
    
    //if (EmissiveTextureEnabled == true) {
    //    finalColor = finalColor + (tex2D(EmissiveTextureSampler, input.TextureCoordinate) * EmissiveColorFactor);
    //}
    //else
    //{
    //    finalColor = finalColor + (float4(1, 1, 1, 1) * EmissiveColorFactor);
    //}
    
    float alpha = finalColor.a;
        
    if (AlphaMode == 1)
    {
        // Mask Mode
        if (alpha < AlphaCutoff)
            discard;
    }
    else if (AlphaMode == 2)
    {
        // Blend Mode
        finalColor.a = alpha;
    }

    
	return finalColor;
}

technique T0
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
    //pass P1
    //{
    //    VertexShader = compile VS_SHADERMODEL MainVS();
    //    ZEnable = true;
    //    FillMode = WireFrame;
    //    ZWriteEnable = true;
    //    CullMode = CCW;
    //    AlphaBlendEnable = false;
    //    PixelShader = compile PS_SHADERMODEL MainPS();
    //}
};

technique T1
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        ZEnable = true;
        FillMode = Solid;
        ZWriteEnable = true;
        CullMode = None;
        AlphaBlendEnable = true;
        DestBlend = INVSRCALPHA;
        SrcBlend = SRCALPHA;
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};

//Texture2D NormalMap : register(t1);
//Texture2D OcclusionMap : register(t2);