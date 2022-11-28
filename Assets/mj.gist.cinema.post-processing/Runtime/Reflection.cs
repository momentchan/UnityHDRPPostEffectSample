using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Cinema.PostProcessing
{
    [System.Serializable, VolumeComponentMenu("Post-processing/Cinema/Reflection")]
    public sealed class Reflection : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        public Bool​Parameter isHorizontal = new Bool​Parameter(false);
        public Bool​Parameter isVertical = new Bool​Parameter(false);

        private Material _material;

        static class ShaderIDs
        {
            internal static readonly int Horizontal = Shader.PropertyToID("_Horizontal");
            internal static readonly int Vertical = Shader.PropertyToID("_Vertical");
            internal static readonly int InputTexture = Shader.PropertyToID("_InputTexture");
        }

        public bool IsActive() => _material != null && (isHorizontal.value  || isVertical.value);

        public override CustomPostProcessInjectionPoint injectionPoint =>
            CustomPostProcessInjectionPoint.AfterPostProcess;

        public override void Setup()
        {
            _material = CoreUtils.CreateEngineMaterial("Hidden/Cinema/PostProcess/Reflection");
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle srcRT, RTHandle destRT)
        {
            if (_material == null) return;

            _material.SetInt(ShaderIDs.Horizontal, isHorizontal.value ? 1 : 0);
            _material.SetInt(ShaderIDs.Vertical, isVertical.value ? 1 : 0);
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
