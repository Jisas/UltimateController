using UltimateFramework.LocomotionSystem;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateFramework.AnimatorDataSystem
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorDataHandler : MonoBehaviour
    {
        public Dictionary<int, Dictionary<string, StateInfo>> LayerStates { get; private set; }
        BaseLocomotionComponent m_Locomotion;
        Animator animator;

        private void Awake()
        {
            m_Locomotion = GetComponent<BaseLocomotionComponent>();
            animator = GetComponent<Animator>();
        }

        public void RegisterAnimatorData()
        {
            LayerStates = new();
            AnimatorUtilities.RegisterLayerStates(animator, m_Locomotion.OverrideAnimatorController, LayerStates);
        }
    }
}
