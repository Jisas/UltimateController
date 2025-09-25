using UltimateFramework.CollisionsAndDamageSystem;
using UltimateFramework.LocomotionSystem;
using UltimateFramework.Utils;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using System;
using UltimateFramework.AnimatorDataSystem;

namespace UltimateFramework.ActionsSystem
{
	[CreateAssetMenu(menuName = "Ultimate Framework/Systems/Actions/Action Asset/Hit")]
	public class HitAction : BaseAction
	{
		#region PublicFields
		public float motionSpeed = 1.0f;
        public bool useDirectionalHit;
		[ConditionalField("useDirectionalHit", true)] 
		public DirectionalHit directionalHit;
		#endregion

		#region PrivateFields
		private CharacterDamageHandler damageHandler;
		#endregion

		#region Structs
		[Serializable]
		public struct DirectionalHit
		{
			public AnimationClip backHit;
            public AnimationClip frontHit;
            public AnimationClip leftHit;
            public AnimationClip rightHit;
        }
        #endregion

        #region Components
        private AnimatorDataHandler m_AnimatorDataHandler;
        private BaseLocomotionComponent m_Locomotion;
        private DamageComponent m_DamageHandler;
        private ActionsComponent m_Actions;
        #endregion

        // Optional function used for initial state settings, will be called from the Start function of the ActionsComponent script.
        public override void StartConfig(BaseAction action, ActionStructure currentStructure)
		{
			action = this;
			base.StartConfig(action, currentStructure);

            m_Actions = GetComponentByName<ActionsComponent>("ActionsComponent");
            m_Locomotion = GetComponentByName<BaseLocomotionComponent>("BaseLocomotionComponent");
            m_DamageHandler = GetComponentByName<DamageComponent>("DamageComponent");
            m_AnimatorDataHandler = GetComponentByName<AnimatorDataHandler>("AnimatorDataHandler");

            damageHandler = m_DamageHandler as CharacterDamageHandler;
        }
		
		// Function used for the logic of the action itself, will be called through the assigned input.
		public override async Task Execute(EntityActionsManager actionsMaster, ActionStructure currentStructure, Animator animator, CancellationToken ct)
		{
            try
            {
				if (actionsMaster.IsHigherOrEqualPriorityActionExecuting(this))
				{
					CancelAction();
					return;
				}

				if (actionsMaster.IsCantBeInterruptedActionExecuting(this))
				{
					CancelAction();
					return;
				}

				this.IsExecuting = true;
				m_Actions.CurrentAction = this;
				actionsMaster.CurrentAction = this;
                m_Locomotion.CanJump = false;

                if (useDirectionalHit)
				{
					if (damageHandler.HitDirectionVerifier(damageHandler.LastHitDirection).z > 0)
                        m_AnimatorDataHandler.OverrideAnimatorController[currentStructure.overrideClip] = directionalHit.frontHit;
					else m_AnimatorDataHandler.OverrideAnimatorController[currentStructure.overrideClip] = directionalHit.backHit;

					if (damageHandler.HitDirectionVerifier(damageHandler.LastHitDirection).x > 0)
                        m_AnimatorDataHandler.OverrideAnimatorController[currentStructure.overrideClip] = directionalHit.leftHit;
					else m_AnimatorDataHandler.OverrideAnimatorController[currentStructure.overrideClip] = directionalHit.rightHit;
				}
				else m_AnimatorDataHandler.OverrideAnimatorController[currentStructure.overrideClip] = currentStructure.motion;

				int layerIndex = animator.GetLayerIndex(currentStructure.layerMask);
				animator.applyRootMotion = true;

                int[] excludeLayers = new int[]
                {
                    layerIndex,
                    animator.GetLayerIndex(m_Locomotion.RightHandMaskName),
                    animator.GetLayerIndex(m_Locomotion.RightAndLeftHandMaskName)
                };
                PlayActionAnimation(animator, layerIndex, currentStructure, motionSpeed, excludeLayersForDesactive: excludeLayers);

                damageHandler.CanTakeDamage = false;
				await ActionFinishNotify(this);

				ResetValues();
				this.IsExecuting = false;
            }
            catch (System.Exception)
            {
				CancelationTS.Cancel();
                throw;
            }
        }

        // Optional function that should be used to reset values of the action itself, should be called at your convenience within the Execute function.
        public override void ResetValues()
		{
            damageHandler.CanTakeDamage = true;
            m_Locomotion.CanJump = true;
        }
		
		// Optional function to add logic when the action is interrupted by a higher-priority action
		public override void InterruptAction()
		{
			ResetValues();
			CancelationTS.Cancel();
			this.IsExecuting = false;
		}

        // Optional function to add logic when the action is cancelled by a higher priority action
        protected override void CancelAction()
		{
			ResetValues();
            CancelationTS.Cancel();
            this.IsExecuting = false;
		}
	}
}