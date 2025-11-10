// Cursor, 2025
Shader "Custom/TreeWind"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        
        [Header(Wind Settings)]
        _WindStrength ("Wind Strength", Range(0, 2)) = 0.5
        _WindSpeed ("Wind Speed", Range(0, 5)) = 1.0
        _WindFrequency ("Wind Frequency", Range(0.1, 2)) = 0.5
        _WindDirection ("Wind Direction (XY)", Vector) = (1, 0, 0, 0)
        
        [Header(Branch Sway)]
        _BranchSwayStrength ("Branch Sway Strength", Range(0, 1)) = 0.3
        _BranchSwaySpeed ("Branch Sway Speed", Range(0, 3)) = 1.5
        
        [Header(Trunk Stiffness)]
        _TrunkStiffness ("Trunk Stiffness", Range(0, 1)) = 0.8
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
            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD3;
                float4 shadowCoord : TEXCOORD4;
                float fogCoord : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _WindStrength;
                float _WindSpeed;
                float _WindFrequency;
                float4 _WindDirection;
                float _BranchSwayStrength;
                float _BranchSwaySpeed;
                float _TrunkStiffness;
            CBUFFER_END

            // Simple wind calculation - WebGL friendly
            float3 CalculateWind(float3 positionOS, float3 positionWS)
            {
                // Normalize wind direction
                float2 windDir = normalize(_WindDirection.xy);
                
                // Height factor - more movement at top of tree
                float heightFactor = saturate(positionOS.y);
                
                // Base wind oscillation using world position for variation
                float windTime = _Time.y * _WindSpeed;
                float windPhase = dot(positionWS.xz, windDir) * _WindFrequency + windTime;
                
                // Primary wind sway (side to side)
                float windSway = sin(windPhase) * _WindStrength * heightFactor;
                
                // Secondary branch movement (faster, smaller)
                float branchPhase = windPhase * 2.3 + positionWS.x * 0.7;
                float branchSway = sin(branchPhase) * _BranchSwayStrength * heightFactor;
                
                // Combine wind effects
                float totalSway = windSway + branchSway;
                
                // Apply trunk stiffness - less movement at bottom
                float trunkFactor = lerp(1.0, _TrunkStiffness, 1.0 - heightFactor);
                totalSway *= (1.0 - trunkFactor);
                
                // Calculate displacement direction (perpendicular to wind direction)
                float2 displacementDir = float2(-windDir.y, windDir.x);
                
                // Displace in XZ plane
                float3 windDisplacement = float3(
                    displacementDir.x * totalSway,
                    0.0, // No vertical displacement for basic wind
                    displacementDir.y * totalSway
                );
                
                // Subtle forward/backward movement for more natural look
                float forwardBack = sin(windPhase * 0.7) * _WindStrength * 0.2 * heightFactor * (1.0 - trunkFactor);
                windDisplacement += float3(windDir.x * forwardBack, 0.0, windDir.y * forwardBack);
                
                return windDisplacement;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                
                // Get world position before displacement for wind calculation
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                
                // Calculate wind displacement
                float3 windDisplacement = CalculateWind(input.positionOS.xyz, positionWS);
                
                // Apply wind to vertex position
                float3 displacedPositionOS = input.positionOS.xyz + windDisplacement;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(displacedPositionOS);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.shadowCoord = GetShadowCoord(vertexInput);
                output.fogCoord = ComputeFogFactor(output.positionCS.z);
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                
                // Sample texture
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
                
                // Standard lighting
                Light mainLight = GetMainLight(input.shadowCoord);
                float NdotL = saturate(dot(input.normalWS, mainLight.direction));
                half3 lighting = albedo.rgb * mainLight.color * NdotL;
                lighting += SampleSH(input.normalWS) * albedo.rgb;
                
                // Apply fog
                half3 finalColor = MixFog(lighting, input.fogCoord);
                
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
            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                float _WindStrength;
                float _WindSpeed;
                float _WindFrequency;
                float4 _WindDirection;
                float _BranchSwayStrength;
                float _BranchSwaySpeed;
                float _TrunkStiffness;
            CBUFFER_END

            // Same wind calculation as main pass
            float3 CalculateWind(float3 positionOS, float3 positionWS)
            {
                float2 windDir = normalize(_WindDirection.xy);
                float heightFactor = saturate(positionOS.y);
                float windTime = _Time.y * _WindSpeed;
                float windPhase = dot(positionWS.xz, windDir) * _WindFrequency + windTime;
                
                float windSway = sin(windPhase) * _WindStrength * heightFactor;
                float branchPhase = windPhase * 2.3 + positionWS.x * 0.7;
                float branchSway = sin(branchPhase) * _BranchSwayStrength * heightFactor;
                
                float totalSway = windSway + branchSway;
                float trunkFactor = lerp(1.0, _TrunkStiffness, 1.0 - heightFactor);
                totalSway *= (1.0 - trunkFactor);
                
                float2 displacementDir = float2(-windDir.y, windDir.x);
                float3 windDisplacement = float3(
                    displacementDir.x * totalSway,
                    0.0,
                    displacementDir.y * totalSway
                );
                
                float forwardBack = sin(windPhase * 0.7) * _WindStrength * 0.2 * heightFactor * (1.0 - trunkFactor);
                windDisplacement += float3(windDir.x * forwardBack, 0.0, windDir.y * forwardBack);
                
                return windDisplacement;
            }

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 windDisplacement = CalculateWind(input.positionOS.xyz, positionWS);
                float3 displacedPositionOS = input.positionOS.xyz + windDisplacement;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(displacedPositionOS);
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

// Cursor. 2025. Composer (Version 1). [Large language model]. Available at: https://cursor.com/ [Accessed: 09 November 2025].
