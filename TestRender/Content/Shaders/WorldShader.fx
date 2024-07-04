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

cbuffer FogBuffer
{
    float4 fogColor;
    float fogStart;
    float fogEnd;
};

cbuffer Constants : register(b1)
{
    float AlphaCutoff;
    int AlphaMode;
    float4 BaseColorFactor = float4(1.0, 1.0, 1.0, 1.0);
    bool TextureEnabled;
}

float Delta;
float Total;
uint  AnimationDirection;
float AnimationSpeed = 1.0;
float3 LightPosition;
float3 CameraPosition;
float3 CameraDirection;
float3 LightDirection;
bool TextureAnimation;

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

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 position = input.Position;
    float4 color = input.Color;
    float2 textureCoordinate = input.TextureCoordinate;

    if (TextureAnimation == true)
    {
        float speedX = Total / TextureDimensions.x * AnimationSpeed;
        float speedY = Total / TextureDimensions.y * AnimationSpeed;
        
        switch (AnimationDirection)
        {
            case 0x01: textureCoordinate.x += speedX; break;
            case 0x02: textureCoordinate.x -= speedX; break;
            case 0x03: textureCoordinate.y += speedY; break;
            case 0x04: textureCoordinate.y -= speedY; break;
            case 0x05: textureCoordinate.x -= speedX; textureCoordinate.y += speedY; break;
            case 0x06: textureCoordinate.x -= speedX; textureCoordinate.y -= speedY; break;
            case 0x07: textureCoordinate.x += speedX; textureCoordinate.y += speedY; break;
            case 0x08: textureCoordinate.x += speedX; textureCoordinate.y -= speedY; break;
        }
    }

    float4 worldPosition = mul(position, World);
    float4 viewPosition = mul(worldPosition, View);
    float4 projectionPosition = mul(viewPosition, Projection);

    output.Position = projectionPosition;
    output.Color = input.Color;
    output.Normal = mul(input.Normal, World);
    output.LightDirection = LightPosition - mul(input.Position, World);
    output.TextureCoordinate = textureCoordinate;
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
        AlphaBlendEnable = true; 
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
