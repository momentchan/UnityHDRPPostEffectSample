using System.Collections;
using Cinema.PostProcessing.KMath;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Cinema.PostProcessing
{
    [System.Serializable, VolumeComponentMenu("Post-processing/Cinema/Negative")]
    public sealed class Negative : PostProcessComponent
    {
        public ClampedFloatParameter ratio = new ClampedFloatParameter(0f, 0, 1.0f);
        public FloatParameter effectTime = new FloatParameter(0.25f);

        private bool isNegative;

        Material _material;
        static class ShaderIDs
        {
            internal static readonly int NegativeRatio = Shader.PropertyToID("_NegativeRatio");
            internal static readonly int InputTexture = Shader.PropertyToID("_InputTexture");
        }

        public override bool IsActive() => _material != null && ratio.value > 0;

        public override CustomPostProcessInjectionPoint injectionPoint =>
            CustomPostProcessInjectionPoint.AfterPostProcess;

        public override void Setup()
        {
            _material = CoreUtils.CreateEngineMaterial("Hidden/Cinema/PostProcess/Negative");
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle srcRT, RTHandle destRT)
        {
            if (_material == null) return;

            _material.SetTexture(ShaderIDs.InputTexture, srcRT);
            _material.SetFloat(ShaderIDs.NegativeRatio, ratio.value);
            cmd.Blit(srcRT, destRT, _material);
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(_material);
        }

        public override void Execute(MonoBehaviour go, PostProcessType type)
        {
            go.StartCoroutine(ApplyNegative());
        }

        public override void Reset()
        {
            ratio.value = 0;
        }
        IEnumerator ApplyNegative()
        {
            yield return null;
            float duration = effectTime.value;
            float start = isNegative ? 1 : 0;
            float end = 1f - start;
            isNegative = !isNegative;
            while (duration > 0f)
            {
                duration = Mathf.Max(duration - Time.deltaTime, 0);
                ratio.value = Easing.Ease(EaseType.QuadOut, start, end, 1f - duration / effectTime.value);
                yield return null;
            }
        }
    }
}
