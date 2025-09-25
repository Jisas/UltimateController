using UltimateFramework.CollisionsAndDamageSystem;
using UltimateFramework.LocomotionSystem;
using UltimateFramework.StatisticsSystem;
using UltimateFramework.DeadSystem;
using UltimateFramework.Inputs;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using System;
using UltimateFramework.AnimatorDataSystem;

namespace UltimateFramework.ActionsSystem
{
	[CreateAssetMenu(menuName = "Ultimate Framework/Systems/Actions/Action Asset/Dead")]
	public class DeadAction : BaseAction
	{
		#region PublicFields
		public bool isEnemy = false;
		public AnimationClip motion;
        public float motionSpeed = 1.0f;
        #endregion

        #region Components
        private AnimatorDataHandler m_AnimatorDataHandler;
        private BaseLocomotionComponent m_Locomotion;
        private StatisticsComponent m_Statistics;
        private DamageComponent m_DamageHandler;
        private ActionsComponent m_Actions;
		private BaseDeadComponent m_Dead;

        private CharacterDamageHandler _damageHandler;
        #endregion

        public override void StartConfig(BaseAction action, ActionStructure currentStructure)
		{
			action = this;
			base.StartConfig(action, currentStructure);

            m_Statistics = GetComponentByName<StatisticsComponent>("StatisticsComponent");
            m_Actions = GetComponentByName<ActionsComponent>("ActionsComponent");
            m_Locomotion = GetComponentByName<BaseLocomotionComponent>("BaseLocomotionComponent");
            m_AnimatorDataHandler = GetComponentByName<AnimatorDataHandler>("AnimatorDataHandler");
            m_DamageHandler = GetComponentByName<DamageComponent>("DamageComponent");
			m_Dead = GetComponentByName<BaseDeadComponent>("BaseDeadComponent");
            _damageHandler = m_DamageHandler as CharacterDamageHandler;
        }
		
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
				
				if (!actionsMaster.MeetsActionCost(m_Statistics))
				{
					ResetValues();
					this.IsExecuting = false;
					return;
				}

                this.IsExecuting = true;
                m_Locomotion.CanJump = false;
				m_Locomotion.CanMove = false;
                m_Actions.CurrentAction = this;
                actionsMaster.CurrentAction = this;
                m_Statistics.CanRegenerateStats = false;
				InputsManager.EnablePlayerMap(false);

                m_AnimatorDataHandler.OverrideAnimatorController[currentStructure.overrideClip] = motion;
                int layerIndex = animator.GetLayerIndex(currentStructure.layerMask);
                animator.applyRootMotion = true;

                int[] excludeLayers = new int[]
                {
                    layerIndex,
                    animator.GetLayerIndex(m_Locomotion.RightHandMaskName),
                    animator.GetLayerIndex(m_Locomotion.RightAndLeftHandMaskName)
                };
                PlayActionAnimation(animator, layerIndex, currentStructure, motionSpeed, excludeLayersForDesactive: excludeLayers);

                _damageHandler.CanTakeDamage = false;
                m_Dead.StartDeadCoroutine();

                await ActionFinishNotify(this);
                _damageHandler.CanTakeDamage = true;
                this.IsExecuting = false;
            }
			catch (OperationCanceledException)
			{
				CancelationTS.Cancel();
				throw;
			}
		}
		
		public override void InterruptAction()
		{
			ResetValues();
			this.IsExecuting = false;
		}
		
		protected override void CancelAction()
		{
			ResetValues();
			this.IsExecuting = false;
		}
	}
}