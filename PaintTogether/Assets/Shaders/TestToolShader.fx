#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
float4 Color;
float2 Resolution;

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
	float2 uv = input.TextureCoordinates;
	float2 pixCoords = uv * Resolution;

	// Color of the canvas initally
	float4 color = tex2D(SpriteTextureSampler, uv) * input.Color;

	if (pixCoords.x < 10.0 || pixCoords.x > Resolution.x - 10.0) 
	{
		color = Color;
	}
	if (pixCoords.y < 10.0 || pixCoords.y > Resolution.y - 10.0) 
	{
		color = Color;
	}

	return color;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};