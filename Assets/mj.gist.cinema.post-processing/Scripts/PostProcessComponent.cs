using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Cinema.PostProcessing
{
    [System.Serializable]
    public abstract class PostProcessComponent : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        public FloatParameter transitionT = new FloatParameter(0.25f);
        public abstract bool IsActive();
        public abstract void Execute(MonoBehaviour go, PostProcessType type);
        public abstract void Reset();
    }
}