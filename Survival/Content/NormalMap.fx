#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix World;
matrix ViewProjection;

bool UseNormalMap = false;

Texture2D Texture : register(t0);
sampler TextureSampler : register(s0)
{
	Texture = (Texture);
	MinFilter = Point; // Minification Filter
	MagFilter = Point; // Magnification Filter
	MipFilter = Linear; // Mip-mapping
	AddressU = Wrap; // Address Mode for U Coordinates
	AddressV = Wrap; // Address Mode for V Coordinates
};

Texture2D NormalMap : register(t1);
sampler NormalMapSampler : register(s1)
{
	Texture = (NormalMap);
	MinFilter = Point; // Minification Filter
	MagFilter = Point; // Magnification Filter
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
	float2 TextureUVs : TEXCOORD0;
	float3 Normal : TEXCOORD1;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	float4 position = mul(input.Position, World);
	float2 textureUVs = input.TextureUVs;
	float3 normal = mul(input.Normal, World);

	output.Position = mul(position, ViewProjection);
	output.TextureUVs = textureUVs;
	output.Normal = normal;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float3 AmbientColor = float3(0.1, 0.1, 0.1);
	float3 DiffuseColor = float3(1.0, 1.0, 1.0);
	float3 LightDirection = float3(1.0, 1.0, 1.0);
	float3 LightColor = float3(1, 1, 1);
	float3 EyeDirection = normalize(float3(0.0, 0.0, 1.0)); // Blickrichtung der Kamera
	float Shininess = 32.0; // Glanzgrad

	float3 color = DiffuseColor;
	color *= tex2D(TextureSampler, input.TextureUVs);

	float3 normal = normalize(input.Normal);
	float3 lightDir = normalize(LightDirection);

	// Combine the vertex normal with the normal map if useNormalMap is true
	if (UseNormalMap == true)
	{
		float3 normalMapNormal = tex2D(NormalMapSampler, input.TextureUVs).rgb * 2.0 - 1.0;
		normal = normalize(normal + normalMapNormal);
	}

	// Berechne den Diffuslichtanteil
	float diffuseFactor = saturate(dot(normal, lightDir));
	float3 diffuse = LightColor * color * diffuseFactor;

	// Berechne den Reflexionsvektor
	float3 reflectDir = reflect(-lightDir, normal);

	// Berechne den Glanzlichtanteil basierend auf dem Winkel zwischen Reflexionsvektor und Blickrichtung der Kamera
	float specularFactor = pow(saturate(dot(reflectDir, EyeDirection)), Shininess);
	float3 specular = LightColor * specularFactor;

	// Kombiniere den Diffuslicht- und Glanzlichtanteil
	float3 lighting = AmbientColor + diffuse + specular;

	float3 output = saturate(lighting);
	return float4(output, 1);
}


technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
		CullMode = NONE;
	}
};
