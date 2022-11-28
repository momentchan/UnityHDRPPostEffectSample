using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Cinema.PostProcessing
{
    [System.Serializable, VolumeComponentMenu("Post-processing/Cinema/RGBShift")]
    public sealed class RGBShift : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        public ClampedFloatParameter power = new ClampedFloatParameter(0, 0f, 100f);

        private Material _material;

        static class ShaderIDs
        {
            internal static readonly int ShiftPower = Shader.PropertyToID("_ShiftPower");
            internal static readonly int InputTexture = Shader.PropertyToID("_InputTexture");
        }

        public bool IsActive() => _material != null && (power.value > 0);

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
    }
}
