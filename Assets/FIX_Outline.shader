Shader "Custom/FIX_Outline"
{
    Properties
    {
        [HideInInspector] _BlitTexture("Source", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        ZTest Always ZWrite Off Cull Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Varyings { float4 posCS : SV_POSITION; float2 uv : TEXCOORD0; };

            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            Varyings vert(float4 posOS : POSITION, float2 uv : TEXCOORD0) {
                Varyings o;
                o.posCS = TransformObjectToHClip(posOS.xyz);
                o.uv = uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target {
                // 【直通测试】直接输出场景原本的颜色
                return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv);
            }
            ENDHLSL
        }
    }
}
