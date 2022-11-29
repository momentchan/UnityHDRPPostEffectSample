using System.Collections;
using Cinema.PostProcessing.KMath;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Cinema.PostProcessing
{
    [System.Serializable, VolumeComponentMenu("Post-processing/Cinema/Mosaic")]
    public sealed class Mosaic : PostProcessComponent
    {
        public ClampedFloatParameter scale = new ClampedFloatParameter(0f, 0f, 100.0f);
        public ClampedFloatParameter maxScale = new ClampedFloatParameter(64f, 0f, 100.0f);
        public Bool​Parameter isCircle = new Bool​Parameter(false);

        Material _material;

        static class ShaderIDs
        {
            internal static readonly int MosaicScale = Shader.PropertyToID("_MosaicScale");
            internal static readonly int IsCricle = Shader.PropertyToID("_IsCricle");
            internal static readonly int InputTexture = Shader.PropertyToID("_InputTexture");
        }

        public override bool IsActive() => _material != null && (scale.value > 0);

        public override CustomPostProcessInjectionPoint injectionPoint =>
            CustomPostProcessInjectionPoint.AfterPostProcess;

        public override void Setup()
        {
            _material = CoreUtils.CreateEngineMaterial("Hidden/Cinema/PostProcess/Mosaic");
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle srcRT, RTHandle destRT)
        {
            if (_material == null) return;

            // Invoke the shader.
            _material.SetFloat(ShaderIDs.MosaicScale, scale.value);
            _material.SetInt(ShaderIDs.IsCricle, isCircle.value ? 1 : 0);
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
            go.StartCoroutine(ApplyMosaic());
        }

        public override void Reset()
        {
            isCircle.value = false;
        }

        IEnumerator ApplyMosaic()
        {
            float duration = transitionT.value;
            while (duration > 0f)
            {
                duration = Mathf.Max(duration - Time.deltaTime, 0);
                scale.value = Easing.Ease(EaseType.QuadOut, maxScale.value, 1, 1f - duration / transitionT.value);
                yield return null;
            }
        }
    }
}
