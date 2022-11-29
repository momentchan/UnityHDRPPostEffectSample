using System.Collections;
using Cinema.PostProcessing.KMath;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Cinema.PostProcessing
{
    [System.Serializable, VolumeComponentMenu("Post-processing/Cinema/RadiationBlur")]
    public sealed class RadiationBlur : PostProcessComponent
    {
        public Vector2Parameter center = new Vector2Parameter(new Vector3(0.5f, 0.5f));
        public ClampedFloatParameter power = new ClampedFloatParameter(0, 0, 100);
        public ClampedFloatParameter maxPower = new ClampedFloatParameter(64, 0, 100);
        public FloatParameter effectTime = new FloatParameter(0.25f);

        Material _material;

        static class ShaderIDs
        {
            internal static readonly int BlurPower = Shader.PropertyToID("_BlurPower");
            internal static readonly int BlurCenter = Shader.PropertyToID("_BlurCenter");
            internal static readonly int InputTexture = Shader.PropertyToID("_InputTexture");
        }

        public override bool IsActive() => _material != null && (power.value > 0);

        public override CustomPostProcessInjectionPoint injectionPoint =>
            CustomPostProcessInjectionPoint.AfterPostProcess;

        public override void Setup()
        {
            _material = CoreUtils.CreateEngineMaterial("Hidden/Cinema/PostProcess/RadiationBlur");
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle srcRT, RTHandle destRT)
        {
            if (_material == null) return;

            // Invoke the shader.
            _material.SetFloat(ShaderIDs.BlurPower, power.value);
            _material.SetVector(ShaderIDs.BlurCenter, center.value);
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
            go.StartCoroutine(ApplyRadiationBlur());
        }

        public override void Reset()
        {
            power.value = 0;
        }

        private IEnumerator ApplyRadiationBlur()
        {
            float duration = effectTime.value;
            while (duration > 0f)
            {
                duration = Mathf.Max(duration - Time.deltaTime, 0);
                power.value = Easing.Ease(EaseType.QuadOut, maxPower.value, 1, 1f - duration / effectTime.value);
                yield return null;
            }
        }

    }
}
