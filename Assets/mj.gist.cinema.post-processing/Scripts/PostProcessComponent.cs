using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Cinema.PostProcessing
{
    [System.Serializable]
    public abstract class PostProcessComponent : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        public abstract bool IsActive();
        public abstract void Execute(MonoBehaviour go, PostProcessType type);
        public abstract void Reset();
    }
}