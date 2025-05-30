#if OPENGL
#define PS_SHADERMODEL ps_3_0
#else
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

texture SceneTexture;
sampler2D sceneSampler = sampler_state
{
    Texture = <SceneTexture>;
};

float VignettePower = 1.5; // 1.0-3.0
float Saturation = 1.2; // 0.0-2.0
float FilmGrainIntensity = 0.05; // 0.0-0.1
float ChromaticAmount = 0.002; // 0.0-0.01
float ScanlineIntensity = 0.1; // 0.0-0.3

float2 Resolution;
float Time;

// --- Pixel Shader ---
float4 PostProcessPS(float2 uv : TEXCOORD0) : COLOR0
{
    // Base color with chromatic aberration
    float2 offset = (uv - 0.5) * ChromaticAmount;
    float3 color = float3(
        tex2D(sceneSampler, uv - offset).r,
        tex2D(sceneSampler, uv).g,
        tex2D(sceneSampler, uv + offset).b
    );

    // Saturation adjustment
    float luminance = dot(color, float3(0.299, 0.587, 0.114));
    color = lerp(luminance, color, Saturation);

    // Vignette (smooth radial gradient)
    float2 centerUV = uv * 2.0 - 1.0;
    float vignette = 1.0 - dot(centerUV, centerUV) * VignettePower;
    vignette = smoothstep(0.0, 1.0, vignette);
    color *= vignette;

    // Subtle film grain (procedural noise)
    float grain = frac(sin(dot(uv + Time, float2(12.9898, 78.233))) * 43758.5453);
    color += grain * FilmGrainIntensity;

    // Scanlines (vertical)
    float scanline = sin(uv.y * Resolution.y * 2.0 + Time * 5.0) * 0.5 + 0.5;
    color *= 1.0 - ScanlineIntensity * scanline;
    
    float2 focusUV = uv * 2.0 - 1.0;
    float focus = pow(1.0 - length(focusUV), 4.0);
    color = lerp(color, tex2D(sceneSampler, uv + focusUV * 0.005 * focus).rgb, 0.2);

    

    return float4(color, 1.0);
}

technique PostProcess
{
    pass Pass0
    {
        PixelShader = compile PS_SHADERMODEL PostProcessPS();
    }
}