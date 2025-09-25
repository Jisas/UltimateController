using UnityEngine;

namespace UltimateFramework.AnimatorDataSystem
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorDataHandler : UFBaseComponent
    {
        [SerializeField] private AnimatorData animatorData;
        [SerializeField] private AnimatorOverrideController overrideController;

        public AnimatorOverrideController OverrideAnimatorController 
        { 
            get => overrideController; 
            set => overrideController = value; 
        }

        public AnimatorData GetData() => animatorData;
    }
}
