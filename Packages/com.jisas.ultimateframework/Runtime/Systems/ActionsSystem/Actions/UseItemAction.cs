using UltimateFramework.ItemSystem;
using UltimateFramework.Utils;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace UltimateFramework.ActionsSystem
{
	[CreateAssetMenu(menuName = "Ultimate Framework/Systems/Actions/Action Asset/UseItem")]
	public class UseItemAction : BaseAction
	{
        public float motionSpeed = 1.0f;
        public WeaponHand actionHand = WeaponHand.OneHand;
        public UseItemLogic usingLogic;
        public TagSelector itemSlotTag;

        private UseItemLogic usingLogicClone;
        private GameObject weaponObj;
        private bool needSetLayerWeight;
        private int overrideLayerIndex;

        // Optional function used for initial state settings, will be called from the Start function of the ActionsComponent script.
        public override void StartConfig(BaseAction action, ActionStructure currentStructure)
		{
			action = this;
			base.StartConfig(action, currentStructure);
            usingLogicClone = usingLogic.Clone();
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

                IsExecuting = true;
                State = ActionState.Running;
                m_Actions.CurrentAction = this;
                actionsMaster.CurrentAction = this;

                m_InputManager.GetInputActionOnCurrentMap("UseItem").Disable();
                m_Locomotion.OverrideAnimatorController[currentStructure.overrideClip] = usingLogicClone.useItemMotion;

                var RightHandWeapon = m_InventoryAndEquipment.GetCurrentRightWeaponObject();
                var LeftHandWeapon = m_InventoryAndEquipment.GetCurrentLeftWeaponObject();
                weaponObj = (actionHand == WeaponHand.OneHand || actionHand == WeaponHand.TwoHand) ? RightHandWeapon : LeftHandWeapon;
                if (weaponObj != null) weaponObj.SetActive(false);

                // Si estas agachado y esta accion no se ejecuta en la capa FullBodyMask y no fuerza el uso del cuerpo completo, desactiva las capas del GlobalPose
                
                if (RightHandWeapon != null)
                {
                    var currentWeaponName = RightHandWeapon.GetComponent<WeaponComponent>().Item.name;
                    var currentlocomotionMap = m_Locomotion.LocomotionMaster.FindMap(currentWeaponName);
                    var overrideLayer = m_Locomotion.LocomotionMaster.FindOverrideLayer(currentlocomotionMap.movement, m_Locomotion.OverrideLayer);
                    var overrideLayerMaskName = overrideLayer != null ? overrideLayer.globalPose.mask : "";
                    overrideLayerIndex = animator.GetLayerIndex(overrideLayerMaskName);
                }

                needSetLayerWeight = m_Locomotion.IsCrouch && currentStructure.layerMask != m_Locomotion.FullBodyMaskName && !currentStructure.forceFullBodyOnly;
                if (needSetLayerWeight && overrideLayerIndex > -1) animator.SetLayerWeight(overrideLayerIndex, 0);

                await Task.Delay(50);
                int[] excludeLayers = new int[]
                {
                    animator.GetLayerIndex(m_Locomotion.LowerBodyMaskName),
                    animator.GetLayerIndex(m_Locomotion.RightHandMaskName),
                    animator.GetLayerIndex(m_Locomotion.RightAndLeftHandMaskName)
                };
                int layerIndex = animator.GetLayerIndex(currentStructure.layerMask);
                PlayActionAnimation(animator, layerIndex, currentStructure, motionSpeed, excludeLayersForActive: excludeLayers);

                await ActionFinishNotify(this);
                ResetValues();
                IsExecuting = false;
            }
            catch (System.Exception)
            {
                CancelationTS.Cancel();
                throw;
            }
        }

        // Optional function to add logic when sub action is active
        protected override void ExecuteInSubActionEnter()
		{
            usingLogicClone.Use(itemSlotTag.tag, m_InventoryAndEquipment, m_Statistics);
        }

        // Optional function that should be used to reset values of the action itself, should be called at your convenience within the Execute function.
        public override void ResetValues()
		{
            if (weaponObj != null) weaponObj.SetActive(true);
            if (needSetLayerWeight && overrideLayerIndex > -1) m_Animator.SetLayerWeight(overrideLayerIndex, 1);
            m_InputManager.GetInputActionOnCurrentMap("UseItem").Enable();
        }
		
		// Optional function to add logic when the action is interrupted by a higher-priority action
		public override void InterruptAction()
		{
			ResetValues();
			this.IsExecuting = false;
		}

        // Optional function to add logic when the action is cancelled by a higher priority action
        protected override void CancelAction()
		{
			ResetValues();
            this.IsExecuting = false;
		}
	}
}