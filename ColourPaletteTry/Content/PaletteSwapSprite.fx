#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
float4 NormalPalette[32];
float4 SwapPalette[32];
bool UseSwapPalette;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 texColor = tex2D(SpriteTextureSampler,input.TextureCoordinates);
	
	if (UseSwapPalette == true)
	{
		int index = -1;
		for (int i = 0; i < 32; i++)
		{
			if (all(texColor == NormalPalette[i]))
			{
				index = i;
				break;
			}
		}	
		
		if (index >= 0)
		{
			return SwapPalette[index] * input.Color;
		}
	}
	
	return texColor * input.Color;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};