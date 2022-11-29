#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

struct Attributes
{
	uint vertexID : SV_VertexID;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
	float4 positionCS : SV_POSITION;
	float2 texcoord   : TEXCOORD0;
	UNITY_VERTEX_OUTPUT_STEREO
};

Varyings Vertex(Attributes input)
{
	Varyings output;
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
	output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
	output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
	return output;
}

float _NoiseDistortionScale;
float3 _NoiseDistortionPosition;
float _NoiseDistortionPower;
float _NoiseDistortionTimeScale;
float2 _BarrelDistortionPower;

TEXTURE2D_X(_InputTexture);
TEXTURE2D_X(_MainTex);
float4  _InputTexture_TexelSize;
TEXTURE2D_X(_EdgeTex);
//SAMPLER(sampler_InputTexture);

float _HCoef[9];
float _VCoef[9];
float _Coef[9];

float _Threshold;
float _Blend;
float4 _BackColor;
float4 _EdgeColor;
float _EdgePower;

float4 Composite(Varyings input) : SV_Target
{
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

	float2 uv = input.texcoord;
	float4 color = LOAD_TEXTURE2D_X(_EdgeTex, uv * _ScreenSize.xy);

	return color;//lerp(edge, color, _Blend);
}

float4 SobelFilter(Varyings input) : SV_Target
{
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

	float2 uv = input.texcoord;
	float4 color = LOAD_TEXTURE2D_X(_InputTexture, uv * _ScreenSize.xy);

	float3 hcol = float3(0, 0, 0);
	float3 vcol = float3(0, 0, 0);

	hcol += LOAD_TEXTURE2D_X(_InputTexture, (uv + float2(-1, -1) * _InputTexture_TexelSize.xy)* _ScreenSize.xy) * _HCoef[0];
	hcol += LOAD_TEXTURE2D_X(_InputTexture, (uv + float2(0, -1) * _InputTexture_TexelSize.xy)* _ScreenSize.xy) * _HCoef[1];
	hcol += LOAD_TEXTURE2D_X(_InputTexture, (uv + float2(1, -1) * _InputTexture_TexelSize.xy)* _ScreenSize.xy) * _HCoef[2];
	hcol += LOAD_TEXTURE2D_X(_InputTexture, (uv + float2(-1,  0) * _InputTexture_TexelSize.xy)* _ScreenSize.xy) * _HCoef[3];
	hcol += LOAD_TEXTURE2D_X(_InputTexture, (uv + float2(0,  0) * _InputTexture_TexelSize.xy)* _ScreenSize.xy) * _HCoef[4];
	hcol += LOAD_TEXTURE2D_X(_InputTexture, (uv + float2(1,  0) * _InputTexture_TexelSize.xy)* _ScreenSize.xy) * _HCoef[5];
	hcol += LOAD_TEXTURE2D_X(_InputTexture, (uv + float2(-1,  1) * _InputTexture_TexelSize.xy)* _ScreenSize.xy) * _HCoef[6];
	hcol += LOAD_TEXTURE2D_X(_InputTexture, (uv + float2(0,  1) * _InputTexture_TexelSize.xy)* _ScreenSize.xy) * _HCoef[7];
	hcol += LOAD_TEXTURE2D_X(_InputTexture, (uv + float2(1,  1) * _InputTexture_TexelSize.xy)* _ScreenSize.xy) * _HCoef[8];

	vcol += LOAD_TEXTURE2D_X(_InputTexture, (uv + float2(-1, -1) * _InputTexture_TexelSize.xy)* _ScreenSize.xy) * _VCoef[0];
	vcol += LOAD_TEXTURE2D_X(_InputTexture, (uv + float2(0, -1) * _InputTexture_TexelSize.xy)* _ScreenSize.xy) * _VCoef[1];
	vcol += LOAD_TEXTURE2D_X(_InputTexture, (uv + float2(1, -1) * _InputTexture_TexelSize.xy)* _ScreenSize.xy) * _VCoef[2];
	vcol += LOAD_TEXTURE2D_X(_InputTexture, (uv + float2(-1,  0) * _InputTexture_TexelSize.xy)* _ScreenSize.xy) * _VCoef[3];
	vcol += LOAD_TEXTURE2D_X(_InputTexture, (uv + float2(0,  0) * _InputTexture_TexelSize.xy)* _ScreenSize.xy) * _VCoef[4];
	vcol += LOAD_TEXTURE2D_X(_InputTexture, (uv + float2(1,  0) * _InputTexture_TexelSize.xy)* _ScreenSize.xy) * _VCoef[5];
	vcol += LOAD_TEXTURE2D_X(_InputTexture, (uv + float2(-1,  1) * _InputTexture_TexelSize.xy)* _ScreenSize.xy) * _VCoef[6];
	vcol += LOAD_TEXTURE2D_X(_InputTexture, (uv + float2(0,  1) * _InputTexture_TexelSize.xy)* _ScreenSize.xy) * _VCoef[7];
	vcol += LOAD_TEXTURE2D_X(_InputTexture, (uv + float2(1,  1) * _InputTexture_TexelSize.xy)* _ScreenSize.xy) * _VCoef[8];

	float d = sqrt(hcol * hcol + vcol * vcol);
	float4 edge = lerp(_BackColor, _EdgeColor, step(_Threshold, d));

	return lerp(edge, color, _Blend) * _EdgePower;
}

float4 LaplacianFilter(Varyings input) : SV_Target
{
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

	float2 uv = input.texcoord;
	float4 color = LOAD_TEXTURE2D_X(_InputTexture, uv * _ScreenSize.xy);

	float3 fcol = float3(0, 0, 0);

	fcol += LOAD_TEXTURE2D_X(_InputTexture, (uv + float2(-1, -1) * _InputTexture_TexelSize.xy)* _ScreenSize.xy) * _Coef[0];
	fcol += LOAD_TEXTURE2D_X(_InputTexture, (uv + float2(0, -1) * _InputTexture_TexelSize.xy)* _ScreenSize.xy) * _Coef[1];
	fcol += LOAD_TEXTURE2D_X(_InputTexture, (uv + float2(1, -1) * _InputTexture_TexelSize.xy)* _ScreenSize.xy) * _Coef[2];
	fcol += LOAD_TEXTURE2D_X(_InputTexture, (uv + float2(-1,  0) * _InputTexture_TexelSize.xy)* _ScreenSize.xy) * _Coef[3];
	fcol += LOAD_TEXTURE2D_X(_InputTexture, (uv + float2(0,  0) * _InputTexture_TexelSize.xy)* _ScreenSize.xy) * _Coef[4];
	fcol += LOAD_TEXTURE2D_X(_InputTexture, (uv + float2(1,  0) * _InputTexture_TexelSize.xy)* _ScreenSize.xy) * _Coef[5];
	fcol += LOAD_TEXTURE2D_X(_InputTexture, (uv + float2(-1,  1) * _InputTexture_TexelSize.xy)* _ScreenSize.xy) * _Coef[6];
	fcol += LOAD_TEXTURE2D_X(_InputTexture, (uv + float2(0,  1) * _InputTexture_TexelSize.xy)* _ScreenSize.xy) * _Coef[7];
	fcol += LOAD_TEXTURE2D_X(_InputTexture, (uv + float2(1,  1) * _InputTexture_TexelSize.xy)* _ScreenSize.xy) * _Coef[8];

	float4 edge = lerp(_BackColor, _EdgeColor, step(_Threshold, length(fcol)));

	return lerp(edge, color, _Blend)* _EdgePower;
}
					float _DepthThreshold;

float detectEdge(float2 uv)
{
	float4 duv = float4(0, 0, _InputTexture_TexelSize.xy);

	float d11 = SampleCameraDepth( uv + duv.xy);
	float d12 = SampleCameraDepth( uv + duv.zy);
	float d21 = SampleCameraDepth( uv + duv.xw);
	float d22 = SampleCameraDepth( uv + duv.zw);

	float g_d = length(float2(d11 - d22, d12 - d21));
	g_d = saturate((g_d - _DepthThreshold) * 40);

	return g_d;
}

float4 DepthFilter(Varyings input) : SV_Target
{
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

	float2 uv = input.texcoord;
	float4 col = LOAD_TEXTURE2D_X(_InputTexture, uv * _ScreenSize.xy);

	float edge = detectEdge(uv);

	float4 col2 = lerp(_BackColor, _EdgeColor, step(_Threshold, edge));

	return lerp(col2, col, _Blend) * _EdgePower;
}