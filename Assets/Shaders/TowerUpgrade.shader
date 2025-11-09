Shader "Custom/TowerUpgrade"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        
        _UpgradeIntensity ("Upgrade Intensity", Range(0, 1)) = 0
        
        _Level1Color ("Level 1 Tint", Color) = (1, 0.7, 0.3, 1)
        _Level1GlowIntensity ("Level 1 Glow", Range(0, 2)) = 0.5
        
        _Level2Color ("Level 2 Tint", Color) = (0.5, 0.3, 1, 1)
        _Level2GlowIntensity ("Level 2 Glow", Range(0, 2)) = 1.0
        
        _GlowPower ("Glow Power", Range(1, 10)) = 3
        _RimPower ("Rim Light Power", Range(0.5, 8)) = 2
        _BaseEmission ("Base Emission", Range(0, 1)) = 0.2
        _ColorTintAmount ("Color Tint Amount", Range(0, 1)) = 0.15
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "RenderPipeline"="UniversalPipeline"
        }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD3;
                float4 shadowCoord : TEXCOORD4;
                float fogCoord : TEXCOORD5;
                float3 viewDirWS : TEXCOORD6;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _UpgradeIntensity;
                float4 _Level1Color;
                float _Level1GlowIntensity;
                float4 _Level2Color;
                float _Level2GlowIntensity;
                float _GlowPower;
                float _RimPower;
                float _BaseEmission;
                float _ColorTintAmount;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.shadowCoord = GetShadowCoord(vertexInput);
                output.fogCoord = ComputeFogFactor(output.positionCS.z);
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Sample base texture
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
                
                // Calculate upgrade level (0 = base, 0.5 = level 1, 1.0 = level 2)
                float level1Factor = smoothstep(0.1, 0.6, _UpgradeIntensity);
                float level2Factor = smoothstep(0.5, 1.0, _UpgradeIntensity);
                
                // Blend colors based on upgrade level
                half3 upgradeTint = lerp(
                    half3(1, 1, 1),  // Base: no tint
                    lerp(
                        _Level1Color.rgb,  // Level 1: bronze/gold
                        _Level2Color.rgb,  // Level 2: purple/blue
                        level2Factor
                    ),
                    max(level1Factor, level2Factor)
                );
                
                // Keep original texture mostly intact, minimal color tinting
                half3 tintedColor = albedo.rgb * lerp(half3(1, 1, 1), upgradeTint, _UpgradeIntensity * _ColorTintAmount);
                
                // Calculate rim light for glow effect
                float3 viewDir = normalize(input.viewDirWS);
                float3 normal = normalize(input.normalWS);
                float rim = 1.0 - saturate(dot(viewDir, normal));
                rim = pow(rim, _RimPower);
                
                // Calculate glow intensity based on upgrade level
                float glowIntensity = lerp(
                    0.0,
                    lerp(_Level1GlowIntensity, _Level2GlowIntensity, level2Factor),
                    max(level1Factor, level2Factor)
                );
                
                // Apply glow color
                half3 glowColor = lerp(
                    _Level1Color.rgb,
                    _Level2Color.rgb,
                    level2Factor
                );
                
                // Start with original texture (minimal tinting)
                half3 finalColor = tintedColor;
                
                // Simple lighting on base texture
                Light mainLight = GetMainLight(input.shadowCoord);
                float NdotL = saturate(dot(normal, mainLight.direction));
                half3 lighting = finalColor * mainLight.color * NdotL;
                lighting += SampleSH(normal) * finalColor;
                
                // Add rim lighting glow - this is the main upgrade effect
                half3 rimGlow = glowColor * rim * glowIntensity * _UpgradeIntensity * 2.0;
                
                // Add base emission that's always visible when upgraded (makes it glow even in shadows)
                half3 baseEmission = glowColor * _UpgradeIntensity * _BaseEmission;
                
                // Combine: base lighting + rim glow + base emission
                finalColor = lighting + rimGlow + baseEmission;
                
                // Apply fog
                finalColor = MixFog(finalColor, input.fogCoord);
                
                return half4(finalColor, albedo.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;

                #if UNITY_REVERSED_Z
                    output.positionCS.z = min(output.positionCS.z, output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    output.positionCS.z = max(output.positionCS.z, output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif

                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Lit"
}

