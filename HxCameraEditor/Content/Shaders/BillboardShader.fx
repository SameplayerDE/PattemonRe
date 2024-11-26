#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

cbuffer Matrices : register(b0)
{
    matrix World;
    matrix View;
    matrix Projection;
}

cbuffer CameraData : register(b1)
{
    float3 CameraPosition;
}

Texture2D Texture : register(t0);
sampler TextureSampler : register(s0)
{
	Texture = (Texture);
};

struct VertexShaderInput
{
    float3 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinate : TEXCOORD0;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;

    float3 billboardPosition = input.Position + CameraPosition;
    float4 worldPosition = mul(float4(billboardPosition, 1.0), World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.TextureCoordinate = input.TextureCoordinate;

    return output;
}

float4 MainPS(VertexShaderOutput input) : SV_Target
{
    float4 textureColor = tex2D(TextureSampler, input.TextureCoordinate);
    if (textureColor.a < 1) {
    	discard;
    }
    return textureColor;
}

technique SimpleBillboard
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
