using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using System;
using UltimateFramework.AnimatorDataSystem;
using UltimateFramework.CollisionsAndDamageSystem;
using Ultimateframework.FXSystem;
using UltimateFramework.Inputs;
using UltimateFramework.InventorySystem;
using UltimateFramework.LocomotionSystem;
using UltimateFramework.StatisticsSystem;

namespace UltimateFramework.ActionsSystem
{
	[CreateAssetMenu(menuName = "Ultimate Framework/Systems/Actions/Action Asset/Simple")]
	public class SimpleAction : BaseAction
	{
        #region Components
        private InventoryAndEquipmentComponent m_InventoryAndEquipment;
        private AnimatorDataHandler m_AnimatorDataHandler;
        private BaseLocomotionComponent m_Locomotion;
        private EntityActionInputs m_InputManager;
        private StatisticsComponent m_Statistics;
        private DamageComponent m_DamageHandler;
        private FXManagerComponent m_FXManager;
        private ActionsComponent m_Actions;
        #endregion

        public override void StartConfig(BaseAction action, ActionStructure currentStructure)
		{
			action = this;
			base.StartConfig(action, currentStructure);

            m_Statistics = GetComponentByName<StatisticsComponent>("StatisticsComponent");
            m_InventoryAndEquipment = GetComponentByName<InventoryAndEquipmentComponent>("InventoryAndEquipmentComponent");
            m_Actions = GetComponentByName<ActionsComponent>("ActionsComponent");
            m_Locomotion = GetComponentByName<BaseLocomotionComponent>("BaseLocomotionComponent");
            m_InputManager = GetComponentByName<EntityActionInputs>("EntityActionInputs");
            m_DamageHandler = GetComponentByName<DamageComponent>("DamageComponent");
            m_FXManager = GetComponentByName<FXManagerComponent>("FXManagerComponent");
            m_AnimatorDataHandler = GetComponentByName<AnimatorDataHandler>("AnimatorDataHandler");
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
                m_Actions.CurrentAction = this;
                actionsMaster.CurrentAction = this;
                m_Locomotion.CanJump = false;
                m_Statistics.CanRegenerateStats = false;

                var layerIndex = animator.GetLayerIndex(currentStructure.layerMask);
                int[] excludeLayers = new int[]
                {
                    layerIndex
                };
                PlayActionAnimation(animator, layerIndex, currentStructure, 1, excludeLayersForDesactive: excludeLayers);

                await ActionFinishNotify(this);
                this.IsExecuting = false;
            }
			catch (OperationCanceledException)
			{
				CancelationTS.Cancel();
				throw;
			}
		}
		protected override void ExecuteInSubActionEnter()
		{
			// Write your own logic
		}	
		protected override void ExecuteInSubActionExit()
		{
			// Write your own logic
		}
		public override void ResetValues()
		{
			base.ResetValues(); // --> Call to the base (not mandatory)
			// Write your own logic
		}
		public override void InterruptAction()
		{
			ResetValues(); // --> Call to reset values (not mandatory)
			this.IsExecuting = false; // --> set this action current executing in false (recommended)
		}
		protected override void CancelAction()
		{
			ResetValues(); // --> Call to reset values (not mandatory)
			this.IsExecuting = false; // --> set this action current executing in false (recommended)
		}
	}
}