Shader "Hidden/Cinema/PostProcess/Distortion"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #define DISTORTION_NOISE_UV
            #include "Distortion.hlsl"
            ENDHLSL
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #define DISTORTION_NOISE_UV
            #define DISTORTION_BARREL_UV
            #include "Distortion.hlsl"
            ENDHLSL
        }
    }
    Fallback Off
}
