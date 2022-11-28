using System.Collections;
using Cinema.PostProcessing.KMath;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Cinema.PostProcessing
{
    [System.Serializable, VolumeComponentMenu("Post-processing/Cinema/RGBShift")]
    public sealed class RGBShift : PostProcessComponent
    {
        public ClampedFloatParameter power = new ClampedFloatParameter(0, 0f, 100f);
        public ClampedFloatParameter maxPower = new ClampedFloatParameter(54f, 0f, 100f);
        public FloatParameter effectTime = new FloatParameter(0.25f);

        private Material _material;

        static class ShaderIDs
        {
            internal static readonly int ShiftPower = Shader.PropertyToID("_ShiftPower");
            internal static readonly int InputTexture = Shader.PropertyToID("_InputTexture");
        }

        public override bool IsActive() => _material != null && (power.value > 0);

        public override CustomPostProcessInjectionPoint injectionPoint =>
            CustomPostProcessInjectionPoint.AfterPostProcess;

        public override void Setup()
        {
            _material = CoreUtils.CreateEngineMaterial("Hidden/Cinema/PostProcess/RGBShift");
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle srcRT, RTHandle destRT)
        {
            if (_material == null) return;

            // Invoke the shader.
            float rad = Mathf.PerlinNoise(Time.time, 0) * Mathf.PI * 2f;
            Vector2 shiftUV;
            shiftUV.x = Mathf.Cos(rad) * power.value;
            shiftUV.y = Mathf.Sin(rad) * power.value;
            _material.SetVector(ShaderIDs.ShiftPower, shiftUV);
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
            go.StartCoroutine(ApplyRGBShift());
        }

        public override void Reset()
        {
            power.value = 0;
        }

        IEnumerator ApplyRGBShift()
        {
            float duration = effectTime.value;
            while (duration > 0f)
            {
                duration = Mathf.Max(duration - Time.deltaTime, 0);
                power.value = Easing.Ease(EaseType.QuadOut, maxPower.value, 0, 1f - duration / effectTime.value);
                yield return null;
            }
        }
    }
}
