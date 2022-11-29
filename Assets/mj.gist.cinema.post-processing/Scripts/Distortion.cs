using System.Collections;
using Cinema.PostProcessing.KMath;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Cinema.PostProcessing
{
    [System.Serializable, VolumeComponentMenu("Post-processing/Cinema/Distortion")]
    public sealed class Distortion : PostProcessComponent
    {
        public ClampedFloatParameter noiseDistortionScale = new ClampedFloatParameter(0.5f, 0, 1.0f);
        public Vector3Parameter noiseDistortionPosition = new Vector3Parameter(new Vector3(0,0,1));
        public ClampedFloatParameter noiseDistortionPower = new ClampedFloatParameter(0, 0, 1.0f);
        public ClampedFloatParameter noiseDistortionTimeScale = new ClampedFloatParameter(5, 0, 10.0f);

        public ClampedFloatParameter maxNoiseDistortionPower = new ClampedFloatParameter(0.15f, 0, 1.0f);
        public ClampedFloatParameter maxBarrelDistortionPower = new ClampedFloatParameter(6f, 0, 10f);

        public Vector2Parameter barrelDistortionPower = new Vector2Parameter(new Vector2(0, 0));
        public FloatParameter effectTime = new FloatParameter(0.25f);

        Material _material;
        private bool barrelDistortionSwitcher = false;

        static class ShaderIDs
        {
            internal static readonly int NoiseDistortionNoiseScale = Shader.PropertyToID("_NoiseDistortionScale");
            internal static readonly int NoiseDistortionNoisePosition = Shader.PropertyToID("_NoiseDistortionPosition");
            internal static readonly int NoiseDistortionPower = Shader.PropertyToID("_NoiseDistortionPower");
            internal static readonly int NoiseDistortionTimeScale = Shader.PropertyToID("_NoiseDistortionTimeScale");

            internal static readonly int BarrelDistortionPower = Shader.PropertyToID("_BarrelDistortionPower");

            internal static readonly int InputTexture = Shader.PropertyToID("_InputTexture");
        }

        public override bool IsActive() => _material != null && (
            (noiseDistortionPower.value > 0 && noiseDistortionScale.value > 0) ||
            (barrelDistortionPower.value != Vector2.zero)
        );

        public override CustomPostProcessInjectionPoint injectionPoint =>
            CustomPostProcessInjectionPoint.AfterPostProcess;

        public override void Setup()
        {
            _material = CoreUtils.CreateEngineMaterial("Hidden/Cinema/PostProcess/Distortion");
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle srcRT, RTHandle destRT)
        {
            if (_material == null) return;

            // Noise distortion
            _material.SetFloat(ShaderIDs.NoiseDistortionNoiseScale, noiseDistortionScale.value);
            _material.SetVector(ShaderIDs.NoiseDistortionNoisePosition, noiseDistortionPosition.value);
            _material.SetFloat(ShaderIDs.NoiseDistortionPower, noiseDistortionPower.value);
            _material.SetFloat(ShaderIDs.NoiseDistortionTimeScale, noiseDistortionTimeScale.value);

            // Barrel distortion
            _material.SetVector(ShaderIDs.BarrelDistortionPower, barrelDistortionPower.value);

            _material.SetTexture(ShaderIDs.InputTexture, srcRT);

            // Shader pass number
            int pass = 0;
            if (noiseDistortionScale.value > 0 && noiseDistortionPower.value > 0) { pass = 0; }
            if (barrelDistortionPower.value != Vector2.zero) { pass++; }
            //Debug.Log("pass: " + pass);

            // Blit
            HDUtils.DrawFullScreen(cmd, _material, destRT, null, pass);
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(_material);
        }

        public override void Execute(MonoBehaviour go, PostProcessType type)
        {
            if (type == PostProcessType.NoiseDistortion)
                go.StartCoroutine(ApplyNoiseDistortion());
            if (type == PostProcessType.BarrelDistortion)
                go.StartCoroutine(ApplyBarrelDistortion());
        }

        public override void Reset()
        {
            noiseDistortionPower.value = 0;
            barrelDistortionPower.value = Vector2.zero;
        }

        private IEnumerator ApplyNoiseDistortion()
        {
            float duration = effectTime.value;
            while (duration > 0f)
            {
                duration = Mathf.Max(duration - Time.deltaTime, 0);
                noiseDistortionPower.value = Easing.Ease(EaseType.QuadOut, maxNoiseDistortionPower.value, 0, 1f - duration / effectTime.value);
                yield return null;
            }
        }

        private IEnumerator ApplyBarrelDistortion()
        {
            float duration = effectTime.value;
            float start = barrelDistortionSwitcher ? maxBarrelDistortionPower.value : 0;
            float end = maxBarrelDistortionPower.value - start;
            barrelDistortionSwitcher = !barrelDistortionSwitcher;

            while (duration > 0f)
            {
                duration = Mathf.Max(duration - Time.deltaTime, 0);
                float power = Easing.Ease(EaseType.QuadOut, start, end, 1f - duration / effectTime.value);
                barrelDistortionPower.value = new Vector2(power, power);
                yield return null;
            }
        }
    }
}
