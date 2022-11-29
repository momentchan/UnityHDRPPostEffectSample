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
                Debug.LogError("There is no volume attached.");
                return;
            }

            mapper.Add(PostProcessType.BarrelDistortion, typeof(Distortion));
            mapper.Add(PostProcessType.NoiseDistortion, typeof(Distortion));
            mapper.Add(PostProcessType.Mosaic, typeof(Mosaic));
            mapper.Add(PostProcessType.RectBlockGlitch, typeof(RectBlockGlitch));
            mapper.Add(PostProcessType.RGBShift, typeof(RGBShift));
            mapper.Add(PostProcessType.RandomInvert, typeof(RandomInvert));
            mapper.Add(PostProcessType.RadiationBlur, typeof(RadiationBlur));
            mapper.Add(PostProcessType.ReflectionHorizontal, typeof(Reflection));
            mapper.Add(PostProcessType.ReflectionVertical, typeof(Reflection));
            mapper.Add(PostProcessType.EdgeDetection, typeof(EdgeDetection));

            foreach (var wraper in wrapers)
            {
                var component = profile.components.FirstOrDefault(c => c.GetType() == mapper[wraper.type]);
                wraper.SetComponent((PostProcessComponent)component);
                wraper.Reset();
            }
        }

        private void Update()
        {
            foreach (var wrapper in wrapers)
            {
                if (wrapper.IsValid && Input.GetKeyDown(wrapper.key))
                {
                    wrapper.Execute(this);
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var wrapper in wrapers)
                wrapper.Reset();
        }
    }

    public enum PostProcessType
    {
        BarrelDistortion,
        NoiseDistortion,
        Mosaic,
        RectBlockGlitch,
        RGBShift,
        RandomInvert,
        RadiationBlur,
        ReflectionHorizontal,
        ReflectionVertical,
        EdgeDetection,
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