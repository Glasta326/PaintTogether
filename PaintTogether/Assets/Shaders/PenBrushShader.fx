#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
float4 BrushColor;

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

	// Color of the canvas initally
	float4 color = tex2D(SpriteTextureSampler, uv) * input.Color;

	// Compute distance from pixel to brush center
    float2 diff = uv - float2(0.5,0.5);
    float dist = length(diff);

	// 0.41421356 is 1 - sqrt(2)
	// This essentially maps the corners to be sqrt2 away, and the sides to be 1 away.
	// That's not really true, but it will suffice for explanataion
	// Also radius of brush is controlled by the size of the rectangular region this shader gets applied to,
	// so it's controlled entirely outside of this shader and we dont need to worry about it
	float lim = 0.41421356;

	// If the "distance" to the center is inside the circle then we fill in with our brush color
	if (dist < lim)
	{
    	color = BrushColor;
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