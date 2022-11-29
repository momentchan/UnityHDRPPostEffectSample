using System.Collections;
using Cinema.PostProcessing.KMath;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Cinema.PostProcessing
{
    [System.Serializable, VolumeComponentMenu("Post-processing/Cinema/EdgeDetection")]
    public sealed class EdgeDetection : PostProcessComponent
    {
        public ClampedFloatParameter power = new ClampedFloatParameter(1f, 0, 10.0f);
        public ClampedFloatParameter threshold = new ClampedFloatParameter(0.5f, 0, 1.0f);
        public ClampedFloatParameter depthThreshold = new ClampedFloatParameter(0f, 0, 1.0f);
        public ClampedFloatParameter blend = new ClampedFloatParameter(0f, 0, 1.0f);
        public IntParameter downSampling = new IntParameter(1);
        public ColorParameter backColor = new ColorParameter(Color.white);
        public ColorParameter edgeColor = new ColorParameter(Color.black);
        public EdgeModeParameter filterMode = new EdgeModeParameter(EdgeMode.Sobel);
        public FloatParameter effectTime = new FloatParameter(0.25f);

        Material _material;
        private bool edgeDetectSwitcher;

        static float[] hSobelFilter =
        {
            1, 0, -1,
            2, 0, -2,
            1, 0, -1,
        };
        static float[] vSobelFilter =
        {
             1,  2,  1,
             0,  0,  0,
            -1, -2, -1,
        };

        static float[] laplacianFilter =
        {
            1, 1, 1,
            1,-8, 1,
            1, 1, 1,
        };
        static class ShaderIDs
        {
            internal static readonly int HCoef = Shader.PropertyToID("_HCoef");
            internal static readonly int VCoef = Shader.PropertyToID("_VCoef");
            internal static readonly int Coef = Shader.PropertyToID("_Coef");
            internal static readonly int EdgeTex = Shader.PropertyToID("_EdgeTex123");
            internal static readonly int EdgePower = Shader.PropertyToID("_EdgePower");
            internal static readonly int Blend = Shader.PropertyToID("_Blend");
            internal static readonly int DepthThreshold = Shader.PropertyToID("_DepthThreshold");
            internal static readonly int BackColor = Shader.PropertyToID("_BackColor");
            internal static readonly int EdgeColor = Shader.PropertyToID("_EdgeColor");
            internal static readonly int Threshold = Shader.PropertyToID("_Threshold");
            internal static readonly int InputTexture = Shader.PropertyToID("_InputTexture");
        }

        public override bool IsActive() => _material != null && power.value > 0;

        public override CustomPostProcessInjectionPoint injectionPoint =>
            CustomPostProcessInjectionPoint.AfterPostProcess;

        public override void Setup()
        {
            _material = CoreUtils.CreateEngineMaterial("Hidden/Cinema/PostProcess/EdgeDetection");
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle srcRT, RTHandle destRT)
        {
            if (_material == null) return;

            _material.SetTexture(ShaderIDs.InputTexture, srcRT);

            var tw = srcRT.rt.width;
            var th = srcRT.rt.height;
            var ts = downSampling.value;
            var format = RenderTextureFormat.ARGBFloat;
            var rwMode = RenderTextureReadWrite.Linear;
            var edgeTex = RenderTexture.GetTemporary(tw / ts, th / ts, 0, format, rwMode);

            _material.SetFloat(ShaderIDs.Threshold, threshold.value);
            _material.SetFloat(ShaderIDs.Blend, blend.value);
            _material.SetColor(ShaderIDs.BackColor, backColor.value);
            _material.SetColor(ShaderIDs.EdgeColor, edgeColor.value);
            _material.SetFloat(ShaderIDs.EdgePower, power.value);

            // 小さいサイズで輪郭検出
            switch (filterMode.value)
            {
                case EdgeMode.Sobel:
                    _material.SetFloatArray(ShaderIDs.HCoef, hSobelFilter);
                    _material.SetFloatArray(ShaderIDs.VCoef, vSobelFilter);
                    break;
                case EdgeMode.Laplacian:
                    _material.SetFloatArray(ShaderIDs.Coef, laplacianFilter);
                    break;
                case EdgeMode.Depth:
                    _material.SetFloat(ShaderIDs.DepthThreshold, depthThreshold.value);
                    break;
            }

            cmd.Blit(srcRT, edgeTex, _material, (int)filterMode.value);
            cmd.Blit(edgeTex, destRT);
            RenderTexture.ReleaseTemporary(edgeTex);
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(_material);
        }

        public override void Execute(MonoBehaviour go, PostProcessType type)
        {
            go.StartCoroutine(ApplyEdgeDetection());
        }

        public override void Reset()
        {
            blend.value = 1;
        }
        IEnumerator ApplyEdgeDetection()
        {
            float duration = effectTime.value;
            float start = edgeDetectSwitcher ? 0 : 1;
            float end = 1f - start;
            edgeDetectSwitcher = !edgeDetectSwitcher;
            while (duration > 0f)
            {
                duration = Mathf.Max(duration - Time.deltaTime, 0);
                blend.value = Easing.Ease(EaseType.QuadOut, start, end, 1f - duration / effectTime.value);
                yield return null;
            }
        }
        public enum EdgeMode
        {
            Sobel,
            Laplacian,
            Depth,
        }

        [System.Serializable]
        public class EdgeModeParameter : VolumeParameter<EdgeMode>
        {
            public EdgeModeParameter(EdgeMode value, bool overrideState = false) : base(value, overrideState) { }
        }
    }
}
