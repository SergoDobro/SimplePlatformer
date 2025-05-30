#if OPENGL
#define PS_SHADERMODEL ps_3_0
#else
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float2 Resolution;
float Time;
float GridScale = 10.0;
float BorderThickness = 0.1;
float PulseSpeed = 2.0;
float3 LineColor = float3(0, 0.8, 1);
float3 GlowColor = float3(1, 0.5, 0);

float2 MousePos;
float MouseRadius = 0.2;

// --- Pixel Shader Only ---
float4 HexPS(float2 uv : TEXCOORD0) : COLOR0
{
    // Aspect ratio correction
    uv.x *= Resolution.x / Resolution.y;
    
    
    // In Pixel Shader, before color calculation:
    float mouseDist = distance(uv, MousePos);
    float mouseWave = sin(mouseDist * 20 - Time * 5) * exp(-mouseDist * 10);
    
    // Add noise-based distortion
    //float WarpStrength = 0.001;

//// Modify UV before grid calculations:
//    float2 warp = float2(
//    sin(Time + uv.y * 5) * 0.3,
//    cos(Time * 0.8 + uv.x * 4) * 0.3
//);
//    uv += warp * WarpStrength;
    
    // Create parallax effect
    float DepthScale = 0.5;
    float2 center = float2(0.5, 0.5);
    float depth = 1 - length(uv - center); // 1 at center, 0 at edges
    uv *= 1 + depth * DepthScale; // Zoom toward center
    
    // Hex grid calculations
    float2 scaledUV = uv * GridScale;
    float2 grid = floor(scaledUV);
    float2 fracUV = frac(scaledUV) - 0.5;
    
    // Hex distance calculation
    float2 p = abs(fracUV);
    float d = max(p.x * 0.866025 + p.y * 0.5, p.y);
    
    // Border effect
    float border = smoothstep(BorderThickness - 0.01, BorderThickness + 0.01, d);
    
    
    
    // Animated highlights
    float2 cellID = grid + 0.5;
    float highlight = sin(Time * PulseSpeed + cellID.x * 2.0 + cellID.y * 3.0) * 0.5 + 0.5;
    highlight *= 1.0 - smoothstep(0.3, 0.5, d);
    // Add to PS after highlight calculation:
    float random = frac(sin(dot(grid, float2(12.9898, 78.233))) * 43758.5453);
    highlight *= step(0.7, sin(Time * 2 + random * 10)); // Random flashing
    

    
    
    // Combine colors
    float3 color = lerp(GlowColor, LineColor, border);
    color += highlight * GlowColor * (sin(Time * 3.0) * 0.5 + 0.5);
    color += mouseWave * float3(1, 1, 0); // Yellow ripple
    
    
    // Add flowing particles between cells
    float2 particleUV = uv * GridScale * 0.04;
    float particle = sin(particleUV.x * 10 + Time * 5 / 10.0) * sin(particleUV.y * 8 + Time * 3 / 10.0);
    particle = pow(saturate(particle), 1);
    color += particle * float3(0.4, 0.9, 0); // Orange particles
    
    
    return float4(color, 0.4);
}

technique HexGrid
{
    pass Pass0
    {
        PixelShader = compile PS_SHADERMODEL HexPS();
    }
}