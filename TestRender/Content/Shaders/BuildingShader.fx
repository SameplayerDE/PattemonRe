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
    bool SkinningEnabled;
}

cbuffer Skinning : register(b2) {
    uint NumberOfBones;
    matrix Bones[64];
}

uint  AnimationDirection;
float AnimationSpeed = 1.0;
float3 LightPosition;
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
    float4 BlendIndices : BLENDINDICES0;
    float4 BlendWeight : BLENDWEIGHT0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float4 Normal : TEXCOORD2;
	float2 TextureCoordinate : TEXCOORD0;
	float3 LightDirection : TEXCOORD1;
	float4 WorldPosition : TEXCOORD3;
    float depth : TEXCOORD4;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 position = input.Position;
    float3 normal = input.Normal;
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
    
    if (SkinningEnabled == true)
    {
        matrix skinMatrix = 
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
    output.Color = input.Color;
    output.TextureCoordinate = textureCoordinate;
    output.LightDirection = LightPosition - mul(input.Position, World);
    output.WorldPosition = worldPosition;
    // Anpassung für größeren Bereich
    float depthRange = 100.0; // Anpassung je nach deiner Kameraweite
    output.depth = output.Position.z / depthRange;
    return output;
}

float4 MainPS(VertexShaderOutput input) : SV_Target
{

    // Normal und Lichtvektor normalisieren
   float3 N = input.Normal;
   float3 L = normalize(LightDirection);

   // Diffuse Beleuchtung berechnen
   float diffuse = saturate(dot(N, L));

   // Berechnung der Sättigung der Normals als Durchschnitt der absoluten Werte der Achsen
   float saturation = (abs(N.x) + abs(N.y) + abs(N.z)) / 3.0;
   saturation = length(N);

   // Sättigung invertieren, damit niedrigere Werte mehr Licht bekommen
   float adjustedSaturation = 1.0 - saturation;

   // Diffuse Beleuchtung mit angepasster Sättigung multiplizieren
   diffuse = lerp(1.0, diffuse, adjustedSaturation);
    float4 finalColor = input.Color * BaseColorFactor;
    float ambient = 0.7; // Wert für Umgebungsbeleuchtung (0.0 - 1.0, je nach gewünschter Helligkeit)
    

    if (TextureEnabled == true)
    {
        float4 textureColor = tex2D(TextureSampler, input.TextureCoordinate);
        finalColor = finalColor * textureColor;
    }
    
    float alpha = finalColor.a;
    
    if (AlphaMode == 1)
    {
        // Mask Mode
        if (alpha < AlphaCutoff) {
            discard;
        }
        
    }

    float lighting = max(diffuse, ambient);
    
    if (TextureAnimation == true) {
     return finalColor;
    }
    finalColor = finalColor * lighting;
    finalColor.a = alpha;
   return float4(saturation, saturation, saturation, 1);
}

float4 DepthPS(VertexShaderOutput input) : SV_Target
{
    return float4(input.Normal.xyz, 1);
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
