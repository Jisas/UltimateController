using UnityEngine;

namespace UltimateController.AnimatorDataSystem
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorDataHandler : UFBaseComponent
    {
        [SerializeField] private AnimatorData animatorData;
        [SerializeField] private AnimatorOverrideController overrideController;

        protected int _animIDMotionMultiplier;

        public AnimatorOverrideController OverrideAnimatorController 
        { 
            get => overrideController; 
            set => overrideController = value; 
        }
        public AnimatorData GetData() => animatorData;
        public string FullBodyMaskName => "FullBodyMask";
        public string LowerBodyMaskName => "LowerBodyMask";
        public string UpperBodyMaskName => "UpperBodyMask";
        public string RightHandMaskName => "RightHandMask";
        public string RightAndLeftHandMaskName => "RightAndLeftHandMask";
        public int MotionMultiplier { get => _animIDMotionMultiplier; }

        #region Mono
        private void Awake()
        {
            AssignAnimationIDs();
        }
        #endregion

        #region Anims Setup
        private void AssignAnimationIDs()
        {
            _animIDMotionMultiplier = Animator.StringToHash("MotionMultiplier");
        }
        #endregion

    }
}
