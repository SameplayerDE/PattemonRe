#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

cbuffer Camera : register(b0)
{
    matrix World;
    matrix View;
    matrix Projection;
}

float Delta;
float Total;

bool ShouldAnimate;	
float2 Offset;

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
	float4 Normal : TEXCOORD1;
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
	VertexShaderOutput output = (VertexShaderOutput)0;

	float4 position = input.Position;
	float4 color = input.Color;
	float2 textureCoordinate = input.TextureCoordinate;
	
	float4 worldPosition = mul(position, World);
	float4 viewPosition = mul(worldPosition, View);
	float4 projectionPosition = mul(viewPosition, Projection);

 	output.Position = projectionPosition;
    output.Color = input.Color;
    output.Normal = mul(input.Normal, World);
    output.TextureCoordinate = ApplyTextureAnimation(textureCoordinate);

	return output;
}



float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 finalColor = tex2D(TextureSampler, input.TextureCoordinate) * input.Color;
    
	if (finalColor.a < 0.1) {
		discard;
	}
	
	return finalColor;
}

technique T0
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		FillMode = Solid;
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
        FillMode = Solid;
        CullMode = CW;
        DestBlend = INVSRCALPHA;
        SrcBlend = SRCALPHA;
        AlphaBlendEnable = true;
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};