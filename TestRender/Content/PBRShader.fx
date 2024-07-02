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
    //float4 EmissiveColorFactor = float4(0.0, 0.0, 0.0, 1);

    bool TextureEnabled;
    //bool NormalMapEnabled;
    //bool OcclusionMapEnabled;
    //bool EmissiveTextureEnabled;
    bool SkinningEnabled;
}

cbuffer Skinning : register(b2) {
    uint NumberOfBones;
    matrix Bones[64];
}

uint  AnimationDirection;
float AnimationSpeed = 1.0;
float3 LightPosition;
bool TextureAnimation;

float2 TextureDimensions;
Texture2D Texture : register(t0);
sampler TextureSampler : register(s0)
{
	Texture = (Texture);
};

//Texture2D NormalMap : register(t1);
//sampler NormalMapSampler : register(s1)
//{
//	Texture = (NormalMap);
//};
//
//Texture2D OcclusionMap : register(t2);
//sampler OcclusionMapSampler : register(s2)
//{
//    Texture = (OcclusionMap);
//};
//
//Texture2D EmissiveTexture : register(t3);
//sampler EmissiveTextureSampler : register(s3)
//{
//    Texture = (EmissiveTexture);
//};

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

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{

    float4 position = input.Position;
    float3 Normal = input.Normal;
    float3 N = normalize(Normal);
    float3 L = normalize(input.LightDirection);
    float4 finalColor = input.Color * BaseColorFactor;

    if (TextureEnabled == true)
    {
        float4 textureColor = tex2D(TextureSampler, input.TextureCoordinate);
        finalColor = finalColor * textureColor;
    }
    
    //if (NormalMapEnabled == true)
    //{
    //    float3 normalMapNormal = tex2D(NormalMapSampler, input.TextureCoordinate).rgb * 2.0 - 1.0;
    //    normal = normalize(normal + normalMapNormal);
    //}
    
    //if (EmissiveTextureEnabled == true)
    //{
    //    finalColor += tex2D(EmissiveTextureSampler, input.TextureCoordinate) * EmissiveColorFactor;
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

    //float diffuse = saturate(dot(N, L));
    // if (dot(L, N) < 0.5 && dot(L, N) > 0.25) {
    //        finalColor.rgb *= 0.9;
    //    }else if (dot(L, N) < 0.25 && dot(L, N) > 0.0) {
    //        finalColor.rgb *= 0.7;
    //    } else if (dot(L, N) < 0.0) {
    //        finalColor.rgb *= 0.5;
    //    }
    // Edge detection (Sobel operator)
        //float2 texelSize = 1.0 / float2(64, 64);
        //float4 edgeColor = float4(0.0, 0.0, 0.0, 0.0);
        //
        //// Sobel operator weights for edge detection
        //float2 weightsX = float2(-1, 1);
        //float2 weightsY = float2(-1, 1);
        //
        //float4 color = tex2D(TextureSampler, input.TextureCoordinate);
        //edgeColor.rgb += weightsX.x * tex2D(TextureSampler, input.TextureCoordinate - texelSize).rgb;
        //edgeColor.rgb += weightsX.y * tex2D(TextureSampler, input.TextureCoordinate + texelSize).rgb;
        //edgeColor.rgb += weightsY.x * tex2D(TextureSampler, input.TextureCoordinate - texelSize).rgb;
        //edgeColor.rgb += weightsY.y * tex2D(TextureSampler, input.TextureCoordinate + texelSize).rgb;
        //
        //edgeColor.a = 1.0;
        //finalColor = lerp(finalColor, edgeColor, 0.1); // Adjust the lerp factor to control the strength of the outline
    
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
        AlphaBlendEnable = true; 
        SrcBlend = SrcAlpha; 
        DestBlend = InvSrcAlpha; 
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
