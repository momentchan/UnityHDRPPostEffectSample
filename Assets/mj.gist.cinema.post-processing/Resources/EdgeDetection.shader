Shader "Hidden/Cinema/PostProcess/EdgeDetection"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment SobelFilter
            #include "EdgeDetection.hlsl"
            ENDHLSL
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment LaplacianFilter
            #include "EdgeDetection.hlsl"
            ENDHLSL
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment DepthFilter
            #include "EdgeDetection.hlsl"
            ENDHLSL
        }
    }
    Fallback Off
}
