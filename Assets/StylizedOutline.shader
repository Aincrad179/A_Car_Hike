Shader "Custom/StylizedOutline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.01
    }
    SubShader
    {
        Tags 
        { 
            "RenderPipeline" = "UniversalPipeline" 
            "RenderType"="Opaque" 
            "Queue"="Transparent" 
        }
        
        Pass
        {
            Name "Outline"
            // 剔除正面，只渲染被挤出的背面，从而形成描边
            Cull Front
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
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

            float _OutlineWidth;
            half4 _OutlineColor;

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // 在对象空间将顶点沿法线方向挤出
                // 这样无论模型如何旋转，描边都会紧贴边缘
                float3 offsetPositionOS = input.positionOS.xyz + input.normalOS * _OutlineWidth;
                
                output.positionCS = TransformObjectToHClip(offsetPositionOS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Universal Render Pipeline/FallbackError"
}
