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
    float4 canvasColor = tex2D(SpriteTextureSampler,input.TextureCoordinates) * input.Color;

    // Compute distance from pixel to brush center
    float2 diff = input.TextureCoordinates - float2(0.5,0.5);
    float dist = length(diff);

    // If inside brush radius, apply brush color
    if (dist < 0.41421356) // Magic number is 1-sqrt2. Essentially means the corners of the square are sqrt 2 away, and the sides are 1 away from the center
	{			   
        // simple overwrite; could do alpha blending here
        canvasColor = BrushColor;
    }

    return canvasColor; // still multiply by vertex color
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};