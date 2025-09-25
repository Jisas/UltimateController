using UltimateFramework.LocomotionSystem;
using UltimateFramework.StatisticsSystem;
using UltimateFramework.InventorySystem;
using UltimateFramework.Inputs;
using UltimateFramework.Utils;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using System;
using UltimateFramework.AnimatorDataSystem;

namespace UltimateFramework.ActionsSystem
{
	[CreateAssetMenu(menuName = "Ultimate Framework/Systems/Actions/Action Asset/Defence")]
	public class DefenceAction : BaseAction
	{
		#region SerializedFields
		[SerializeField] private DefenceData weaponDefenceData;
		[Space(5)]
        [SerializeField] private bool allowShield;
		[SerializeField, ConditionalField("allowShield", true)] private DefenceData shieldDefenceData;
		#endregion

		#region Structs
		[Serializable]
		private struct MotionOverrideStruct
		{
            public AnimationClip overrideClip;
            public AnimationClip newMotion;
        }
        [Serializable]
        private struct DefenceData
		{
            public MotionOverrideStruct startMotion;
            public MotionOverrideStruct loopMotion;
            public MotionOverrideStruct endMotion;
            public string endBoolName;
        }
		#endregion

		#region PrivateFields
		private bool end;
        #endregion

        #region Components
        private InventoryAndEquipmentComponent m_InventoryAndEquipment;
        private AnimatorDataHandler m_AnimatorDataHandler;
        private BaseLocomotionComponent m_Locomotion;
        private EntityActionInputs m_InputManager;
        private StatisticsComponent m_Statistics;
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

				bool state = m_InputManager.FindInputAction("Defence").State;
				
				if (state)
				{
                    end = false;
                    m_Animator.SetBool(weaponDefenceData.endBoolName, end);

                    if (allowShield)
                    {
                        if (m_InventoryAndEquipment.GetCurrentOffHandWeapon().HandSocket.childCount <= 0 && m_Actions.FindSpecificActionsGroup("Moveset.Unarmed") == null)
                        {
                            Debug.LogWarning("Not have any weapon on hand and not have actions for unarmed combat");
                            this.IsExecuting = false;
                            m_Actions.CurrentAction = null;
                            return;
                        }

                        m_AnimatorDataHandler.OverrideAnimatorController[shieldDefenceData.startMotion.overrideClip] = shieldDefenceData.startMotion.newMotion;
                        m_AnimatorDataHandler.OverrideAnimatorController[shieldDefenceData.loopMotion.overrideClip] = shieldDefenceData.loopMotion.newMotion;
                        m_AnimatorDataHandler.OverrideAnimatorController[shieldDefenceData.endMotion.overrideClip] = shieldDefenceData.endMotion.newMotion;

						m_InventoryAndEquipment.GetCurrentOffHandWeapon().WeaponComponent.DefenceComponent.IsBlocking = true;
                    }
                    else
                    {
                        if (m_InventoryAndEquipment.GetCurrentMainWeapon().HandSocket.childCount <= 0 && m_Actions.FindSpecificActionsGroup("Moveset.Unarmed") == null)
                        {
                            Debug.LogWarning("Not have any weapon on hand and not have actions for unarmed combat");
                            this.IsExecuting = false;
                            m_Actions.CurrentAction = null;
                            return;
                        }

                        m_AnimatorDataHandler.OverrideAnimatorController[weaponDefenceData.startMotion.overrideClip] = weaponDefenceData.startMotion.newMotion;
                        m_AnimatorDataHandler.OverrideAnimatorController[weaponDefenceData.loopMotion.overrideClip] = weaponDefenceData.loopMotion.newMotion;
                        m_AnimatorDataHandler.OverrideAnimatorController[weaponDefenceData.endMotion.overrideClip] = weaponDefenceData.endMotion.newMotion;

                        m_InventoryAndEquipment.GetCurrentMainWeapon().WeaponComponent.DefenceComponent.IsBlocking = true;
                    }

                    int layerIndex = animator.GetLayerIndex(currentStructure.layerMask);
                    int[] excludeLayers = new int[]
					{
						animator.GetLayerIndex(m_Locomotion.LowerBodyMaskName)
					};

                    PlayActionAnimation(animator, layerIndex, currentStructure, 1, excludeLayersForActive: excludeLayers);
                    await WaitRelease(state);
                }
				else
				{
                    if (allowShield)
                         m_InventoryAndEquipment.GetCurrentOffHandWeapon().WeaponComponent.DefenceComponent.IsBlocking = false;
                    else m_InventoryAndEquipment.GetCurrentMainWeapon().WeaponComponent.DefenceComponent.IsBlocking = false;

                    ResetValues();
                }
			}
			catch (OperationCanceledException)
			{
				CancelationTS.Cancel();
				throw;
			}
		}

		private async Task WaitRelease(bool inputState)
		{
			while (inputState)
			{
				await Task.Yield();
			}
		}
		
		// Optional function to add logic when sub action start
		protected override void ExecuteInSubActionEnter()
		{
			// Write your own logic
		}
		
		// Optional function to add logic when sub action finish
		protected override void ExecuteInSubActionExit()
		{
			// Write your own logic
		}
		
		// Optional function that should be used to reset values of the action itself, should be called at your convenience within the Execute function.
		public override void ResetValues()
		{
            end = true;
			if (!allowShield) m_Animator.SetBool(weaponDefenceData.endBoolName, end);
			else m_Animator.SetBool(shieldDefenceData.endBoolName, end);
        }
		
		// Optional function to add logic when the action is interrupted by a higher-priority action
		public override void InterruptAction()
		{
			ResetValues(); // --> Call to reset values (not mandatory)
			this.IsExecuting = false; // --> set this action current executing in false (recommended)
		}
		
		// Optional function to add logic when the action is cancelled by a call while a higher priority action is being executed.
		protected override void CancelAction()
		{
			ResetValues(); // --> Call to reset values (not mandatory)
			this.IsExecuting = false; // --> set this action current executing in false (recommended)
		}
	}
}