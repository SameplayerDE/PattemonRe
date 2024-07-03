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

float Delta;
float Total;

cbuffer Constants : register(b1)
{
    float AlphaCutoff;
    int AlphaMode;
   
    float4 BaseColorFactor = float4(1.0, 1.0, 1.0, 1.0);

    bool TextureEnabled;
}


uint  AnimationDirection;
float AnimationSpeed = 1.0;
float3 LightPosition;
float3 LightDirection;
bool TextureAnimation;
bool Indoor;


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
	float4 Normal : TEXCOORD2;
	float3 LightDirection : TEXCOORD3;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 position = input.Position;
    float4 color = input.Color;
    float2 textureCoordinate = input.TextureCoordinate;

    if (TextureAnimation == true)
    {
        // AnimationDirection:
        // 0x00 = none
        // 0x01 = PositivX
        // 0x02 = NegativeX
        // 0x03 = PositivY
        // 0x04 = NegativY
        // 0x05 = PxPy
        // 0x06 = PxNy
        // 0x07 = NxPy
        // 0x08 = NxNy

        //    Right = 0x01,
        //    Left = 0x02,
        //    Up = 0x03,
        //    Down = 0x04,
        //    RightUp = 0x05,
        //    RightDown = 0x06,
        //    LeftUp = 0x07,
        //    LeftDown = 0x08,

       if (TextureAnimation == true)
       {
           float speedX = Total / TextureDimensions.x * AnimationSpeed;
           float speedY = Total / TextureDimensions.y * AnimationSpeed;
           if (AnimationDirection == 0x01)
           {
               textureCoordinate.x += speedX;
           }
           else if (AnimationDirection == 0x02)
           {
               textureCoordinate.x -= speedX;
           }
           else if (AnimationDirection == 0x03)
           {
               textureCoordinate.y += speedY;
           }
           else if (AnimationDirection == 0x04)
           {
               textureCoordinate.y -= speedY;
           }
           else if (AnimationDirection == 0x05)
           {
               textureCoordinate.x -= speedX;
               textureCoordinate.y += speedY;
           }
           else if (AnimationDirection == 0x06)
           {
               textureCoordinate.x -= speedX;
               textureCoordinate.y -= speedY;
           }
           else if (AnimationDirection == 0x07)
           {
               textureCoordinate.x += speedX;
               textureCoordinate.y += speedY;
           }
           else if (AnimationDirection == 0x08)
           {
               textureCoordinate.x += speedX;
               textureCoordinate.y -= speedY;
           }
       }
    }

    float4 worldPosition = mul(position, World);
    float4 viewPosition = mul(worldPosition, View);

    output.Position = mul(viewPosition, Projection);
    output.Color = input.Color;
    output.Normal = mul(input.Normal, World);
    output.LightDirection = LightPosition - mul(input.Position, World);
    output.TextureCoordinate = textureCoordinate;
    output.WorldPosition = worldPosition;
    return output;
}

float4 MainPS(VertexShaderOutput input) : SV_Target
{
    float3 Normal = input.Normal;
    float3 N = normalize(Normal);
    float3 L = normalize(-LightDirection);
    float diffuse = saturate(dot(N, L));
    float4 finalColor = saturate(input.Color * BaseColorFactor);
    float ambient = 0.5; // Wert für Umgebungsbeleuchtung (0.0 - 1.0, je nach gewünschter Helligkeit)
    

    if (TextureEnabled == true)
    {
        float4 textureColor = tex2D(TextureSampler, input.TextureCoordinate);
        finalColor = saturate(finalColor * textureColor);
    }
    
    float alpha = finalColor.a;
    
    if (AlphaMode == 1)
    {
        // Mask Mode
        if (alpha < AlphaCutoff) {
            discard;
        }
        
    }
    if (Indoor == true) {
        return finalColor;
    } 
    // Kombiniere diffuse und ambient Beleuchtung
    float lighting = max(diffuse, ambient);
    finalColor = finalColor * lighting;
    finalColor.a = alpha;
    return finalColor;
}

float4 DepthPS(VertexShaderOutput input) : SV_Target
{
    return saturate(float4(input.Normal.xyz, 1) * 2);
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
