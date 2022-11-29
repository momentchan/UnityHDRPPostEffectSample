using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using Cinema.PostProcessing.KMath;
using System.Collections;

namespace Cinema.PostProcessing
{
    [System.Serializable, VolumeComponentMenu("Post-processing/Cinema/RandomInvert")]
    public sealed class RandomInvert : PostProcessComponent
    {
        public ClampedFloatParameter fadeTime = new ClampedFloatParameter(0.25f, 0f, 3f);
        public ClampedFloatParameter noiseScale = new ClampedFloatParameter(250f, 0f, 500f);
        public FloatParameter threshold = new FloatParameter(0);
        public Bool​Parameter isInvert = new BoolParameter(false);

        private Material _material;
        private float startTime = 0;
        public EaseType easeType = EaseType.QuintOut;

        static class ShaderIDs
        {
            internal static readonly int Threshold = Shader.PropertyToID("_Threshold");
            internal static readonly int Invert = Shader.PropertyToID("_Invert");
            internal static readonly int StartTime = Shader.PropertyToID("_StartTime");
            internal static readonly int NoiseScale = Shader.PropertyToID("_NoiseScale");
            internal static readonly int NegativeRatio = Shader.PropertyToID("_NegativeRatio");
            internal static readonly int InputTexture = Shader.PropertyToID("_InputTexture");
        }

        public override bool IsActive() => _material != null && (fadeTime.value > 0);

        public override CustomPostProcessInjectionPoint injectionPoint =>
            CustomPostProcessInjectionPoint.AfterPostProcess;

        public override void Setup()
        {
            _material = CoreUtils.CreateEngineMaterial("Hidden/Cinema/PostProcess/RandomInvert");
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle srcRT, RTHandle destRT)
        {
            if (_material == null) return;

            _material.SetFloat(ShaderIDs.Threshold, threshold.value);
            _material.SetInt(ShaderIDs.Invert, (isInvert.value ? 1 : 0));
            _material.SetFloat(ShaderIDs.StartTime, startTime);
            _material.SetFloat(ShaderIDs.NoiseScale, noiseScale.value);
            _material.SetTexture(ShaderIDs.InputTexture, srcRT);

            // Shader pass number
            var pass = 0;

            // Blit
            HDUtils.DrawFullScreen(cmd, _material, destRT, null, pass);
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(_material);
        }

        public override void Execute(MonoBehaviour go, PostProcessType type)
        {
            go.StartCoroutine(ApplyRandomInvert());
        }

        IEnumerator ApplyRandomInvert()
        {
            yield return null;
            float duration = transitionT.value;
            startTime = Time.time;
            isInvert.value = !isInvert.value;
            while (duration > 0f)
            {
                duration = Mathf.Max(duration - Time.deltaTime, 0);
                threshold.value = Easing.Ease(EaseType.QuadOut, 1, 0, duration / transitionT.value);
                yield return null;
            }
        }
        public override void Reset()
        {
            isInvert.value = false;
        }
    }
}
