using UltimateFramework.Utils;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using System;
using UnityEditor.Experimental.GraphView;

namespace UltimateFramework.ActionsSystem
{
	[CreateAssetMenu(menuName = "Ultimate Framework/Systems/Actions/Action Asset/Defence")]
	public class DefenceAction : BaseAction
	{
        [SerializeField] private DefenceData weaponDefenceData;
		[Space(5)]
        [SerializeField] private bool allowShield;
		[SerializeField, ConditionalField("allowShield", true)] private DefenceData shieldDefenceData;
		private bool end;

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

		public override void StartConfig(BaseAction action, ActionStructure currentStructure)
		{
			action = this;
			base.StartConfig(action, currentStructure);
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

                        m_Locomotion.OverrideAnimatorController[shieldDefenceData.startMotion.overrideClip] = shieldDefenceData.startMotion.newMotion;
                        m_Locomotion.OverrideAnimatorController[shieldDefenceData.loopMotion.overrideClip] = shieldDefenceData.loopMotion.newMotion;
                        m_Locomotion.OverrideAnimatorController[shieldDefenceData.endMotion.overrideClip] = shieldDefenceData.endMotion.newMotion;

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

                        m_Locomotion.OverrideAnimatorController[weaponDefenceData.startMotion.overrideClip] = weaponDefenceData.startMotion.newMotion;
                        m_Locomotion.OverrideAnimatorController[weaponDefenceData.loopMotion.overrideClip] = weaponDefenceData.loopMotion.newMotion;
                        m_Locomotion.OverrideAnimatorController[weaponDefenceData.endMotion.overrideClip] = weaponDefenceData.endMotion.newMotion;

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