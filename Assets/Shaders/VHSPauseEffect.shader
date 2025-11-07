// Anthropic, 2025
Shader "Hidden/VHSPauseEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "VHSPauseEffect"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D_X(_MainTex);
            SAMPLER(sampler_MainTex);

            float _GrainIntensity;
            float _ScanlineIntensity;
            float _VignetteIntensity;
            float _ChromaticAberration;
            float _Desaturation;
            float _UnscaledTime;

            // Random noise function
            float rand(float2 co)
            {
                return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
            }

            // Film grain
            float grain(float2 uv, float strength)
            {
                float x = (uv.x + 4.0) * (uv.y + 4.0) * (_UnscaledTime * 10.0);
                return (fmod((fmod(x, 13.0) + 1.0) * (fmod(x, 123.0) + 1.0), 0.01) - 0.005) * strength;
            }

            // Scanlines
            float scanline(float2 uv)
            {
                return sin(uv.y * 800.0 + _UnscaledTime * 10.0) * 0.5 + 0.5;
            }

            // Vignette
            float vignette(float2 uv)
            {
                uv = uv * 2.0 - 1.0;
                return 1.0 - dot(uv, uv) * 0.3;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                
                // Chromatic aberration
                float2 offset = (_ChromaticAberration * (uv - 0.5));
                float r = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, uv - offset).r;
                float g = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, uv).g;
                float b = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, uv + offset).b;
                half4 col = half4(r, g, b, 1.0);
                
                // Desaturation
                float gray = dot(col.rgb, float3(0.299, 0.587, 0.114));
                col.rgb = lerp(col.rgb, gray.xxx, _Desaturation);
                
                // Film grain
                float grainValue = grain(uv, _GrainIntensity);
                col.rgb += grainValue;
                
                // Scanlines
                float scanlineValue = scanline(uv);
                col.rgb -= (1.0 - scanlineValue) * _ScanlineIntensity;
                
                // Vignette
                float vignetteValue = vignette(uv);
                col.rgb *= lerp(1.0, vignetteValue, _VignetteIntensity);
                
                // Slight random jitter (very subtle)
                float jitter = rand(float2(_UnscaledTime, uv.y)) * 0.001;
                col.rgb += jitter;
                
                return col;
            }
            ENDHLSL
        }
    }
}

// Anthropic. 2025. Claude Sonnet (Version 4.5). [Large language model]. Available at: https://claude.ai/ [Accessed: 07 November 2025].