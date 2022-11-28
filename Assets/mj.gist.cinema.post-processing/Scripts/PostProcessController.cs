using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Cinema.PostProcessing
{
    [RequireComponent(typeof(Volume))]
    public class PostProcessController : MonoBehaviour
    {
        [SerializeField] private List<PostProcessWraper> wrapers;

        private Dictionary<PostProcessType, Type> mapper = new Dictionary<PostProcessType, Type>();

        private VolumeProfile profile;

        private void Start()
        {
            profile = GetComponent<Volume>().profile;
            if (profile == null)
            {
                Debug.LogError("Please set a volume profile on ImageEffectManager.");
                return;
            }

            mapper.Add(PostProcessType.ReflectionHorizontal, typeof(Reflection));
            mapper.Add(PostProcessType.ReflectionVertical, typeof(Reflection));
            mapper.Add(PostProcessType.Mosaic, typeof(Mosaic));
            mapper.Add(PostProcessType.RadiationBlur, typeof(RadiationBlur));
            mapper.Add(PostProcessType.RectBlockGlitch, typeof(RectBlockGlitch));
            mapper.Add(PostProcessType.NoiseDistortion, typeof(Distortion));
            mapper.Add(PostProcessType.BarrelDistortion, typeof(Distortion));
            mapper.Add(PostProcessType.RGBShift, typeof(RGBShift));
            mapper.Add(PostProcessType.RandomInvert, typeof(RandomInvert));

            foreach (var key in wrapers)
            {
                var component = profile.components.FirstOrDefault(c => c.GetType() == mapper[key.type]);
                key.SetComponent((PostProcessComponent)component);
                key.Reset();
            }
        }

        private void Update()
        {
            foreach (var key in wrapers)
            {
                if (key.IsValid && Input.GetKeyDown(key.key))
                {
                    key.Execute(this);
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var key in wrapers)
                key.Reset();
        }
    }

    public enum PostProcessType
    {
        ReflectionHorizontal,
        ReflectionVertical,
        Mosaic,
        RadiationBlur,
        RectBlockGlitch,
        NoiseDistortion,
        BarrelDistortion,
        RGBShift,
        RandomInvert
    }

    [Serializable]
    public class PostProcessWraper
    {
        public PostProcessType type;
        public KeyCode key;

        private PostProcessComponent component;
        public bool IsValid => component != null;
        public void SetComponent(PostProcessComponent component) => this.component = component;
        public void Execute(MonoBehaviour go) => component.Execute(go, type);
        public void Reset() => component.Reset();
    }
}