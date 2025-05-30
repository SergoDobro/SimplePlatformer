#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float Time;
float2 Resolution;
float Speed = 0.25;
float3 Color1 = float3(0.13, 0.58, 0.95); // Vivid Blue
float3 Color2 = float3(0.95, 0.29, 0.51); // Pink
float3 Color3 = float3(0.99, 0.72, 0.07); // Gold

struct PixelShaderInput
{
    float2 TextureCoordinate : TEXCOORD0;
};

float4 PixelShaderFunction(PixelShaderInput input) : COLOR0
{
    float2 uv = input.TextureCoordinate;
    float time = Time * Speed;
    
    // Create three overlapping gradients
    float gradient1 = sin(-uv.x * 2.0 + time) * cos(uv.y * 1.2 + time);
    float gradient2 = cos(uv.y * 2.5 + time * 1.2) * sin(uv.x * 0.7 + time);
    float gradient3 = sin((uv.x + uv.y*0.31) * 4.0 + time * 0.8);
    
    // Blend colors using gradients
    float3 color = lerp(Color1, Color2, gradient1 * 0.5 + 0.5);
    color = lerp(color, Color3, gradient2 * 0.5 + 0.5);
    color = lerp(color, Color1, gradient3 * 0.5 + 0.5);
    
    // Enhance contrast and saturation
    color = pow(color, 1.2);
    
    return float4(color, 1.0);
}

technique GradientBackground
{
    pass Pass1
    {
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}
