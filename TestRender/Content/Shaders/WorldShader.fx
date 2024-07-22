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

cbuffer Constants : register(b1)
{
    float AlphaCutoff;
    int AlphaMode;
    float4 BaseColorFactor = float4(1.0, 1.0, 1.0, 1.0);
    bool TextureEnabled;
}

bool ShouldAnimate;	
float2 Offset;

float Delta;
float Total;
float3 LightPosition;
float3 CameraPosition;
float3 CameraDirection;
float3 LightDirection;
float TimeOfDay = 1;

float2 TextureDimensions;
Texture2D Texture : register(t0);
sampler TextureSampler : register(s0)
{
	Texture = (Texture);
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	float3 Normal : NORMAL0;
	float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinate : TEXCOORD0;
	float4 WorldPosition : TEXCOORD1;
	float4 ViewPosition : TEXCOORD2;
	float4 ViewDistance : TEXCOORD3;
	float4 Normal : TEXCOORD4;
	float3 LightDirection : TEXCOORD5;
	float Depth : TEXCOORD6;
};

float2 ApplyTextureAnimation(float2 textureCoordinate)
{
    if (ShouldAnimate == true)
    {
    	textureCoordinate += Offset;
    }
    return textureCoordinate;
}

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 position = input.Position;
    float4 color = input.Color;
    float2 textureCoordinate = input.TextureCoordinate;

    float4 worldPosition = mul(position, World);
    float4 viewPosition = mul(worldPosition, View);
    float4 projectionPosition = mul(viewPosition, Projection);

    output.Position = projectionPosition;
    output.Color = input.Color;
    output.Normal = mul(input.Normal, World);
    output.LightDirection = LightPosition - mul(input.Position, World);
    output.TextureCoordinate = ApplyTextureAnimation(textureCoordinate);
    output.WorldPosition = worldPosition;
    output.ViewPosition = viewPosition;
    output.ViewDistance = (worldPosition - float4(CameraPosition, 1));
    output.Depth = output.Position.z / output.Position.w;
    return output;
}

float4 MainPS(VertexShaderOutput input) : SV_Target
{
    
    float4 finalColor = saturate(input.Color * BaseColorFactor);

    if (TextureEnabled == true)
    {
        float4 textureColor = tex2D(TextureSampler, input.TextureCoordinate);
        finalColor = saturate(finalColor * textureColor);
    }
    
    float alpha = finalColor.a;
    
    if (AlphaMode == 1)
    {
        if (alpha < AlphaCutoff) {
            discard;
        } 
    }

    return finalColor;
}

float4 DepthPS(VertexShaderOutput input) : SV_Target
{
    return saturate(float4(input.Normal.xyz, 1) );
}

technique T0
{
	pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        ZEnable = true;
        FillMode = Solid;
        ZWriteEnable = true;
        CullMode = CW;
        AlphaBlendEnable = false;
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};

technique T1
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        ZEnable = true;
        FillMode = Solid;
        ZWriteEnable = true;
        CullMode = CW;
        DestBlend = INVSRCALPHA;
        SrcBlend = SRCALPHA;
        AlphaBlendEnable = false; 
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
