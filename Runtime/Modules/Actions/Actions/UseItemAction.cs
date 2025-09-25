using UltimateFramework.LocomotionSystem;
using UltimateFramework.StatisticsSystem;
using UltimateFramework.InventorySystem;
using UltimateFramework.ItemSystem;
using Ultimateframework.FXSystem;
using UltimateFramework.Inputs;
using UltimateFramework.Utils;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UltimateFramework.AnimatorDataSystem;

namespace UltimateFramework.ActionsSystem
{
	[CreateAssetMenu(menuName = "Ultimate Framework/Systems/Actions/Action Asset/UseItem")]
	public class UseItemAction : BaseAction
	{
        #region PublicFields
        public float motionSpeed = 1.0f;
        public FXData fxData;
        public UseItemLogic usingLogic;
        public WeaponHand actionHand = WeaponHand.OneHand;
        public TagSelector itemSlotTag;
        #endregion

        #region PrivateFields
        private UseItemLogic usingLogicClone;
        private GameObject weaponObj;
        private bool needSetLayerWeight;
        private int overrideLayerIndex;
        #endregion

        #region Components
        private InventoryAndEquipmentComponent m_InventoryAndEquipment;
        private AnimatorDataHandler m_AnimatorDataHandler;
        private BaseLocomotionComponent m_Locomotion;
        private EntityActionInputs m_InputManager;
        private StatisticsComponent m_Statistics;
        private FXManagerComponent m_FXManager;
        private ActionsComponent m_Actions;
        #endregion

        public override void StartConfig(BaseAction action, ActionStructure currentStructure)
		{
			action = this;
			base.StartConfig(action, currentStructure);

            m_Actions = GetComponentByName<ActionsComponent>("ActionsComponent");
            m_FXManager = GetComponentByName<FXManagerComponent>("FXManagerComponent");
            m_Statistics = GetComponentByName<StatisticsComponent>("StatisticsComponent");
            m_InputManager = GetComponentByName<EntityActionInputs>("EntityActionInputs");
            m_Locomotion = GetComponentByName<BaseLocomotionComponent>("BaseLocomotionComponent");
            m_AnimatorDataHandler = GetComponentByName<AnimatorDataHandler>("AnimatorDataHandler");
            m_InventoryAndEquipment = GetComponentByName<InventoryAndEquipmentComponent>("InventoryAndEquipmentComponent");

            usingLogicClone = usingLogic.Clone();
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

                var item = m_InventoryAndEquipment.GetItemInEquipment(itemSlotTag.tag);
                if (item == null)
                {
                    CancelAction();
                    return;
                }

                var itemInfo = m_InventoryAndEquipment.FindSuitableSlotInEquipment(item.index);
                if (itemInfo == null || itemInfo.isEmpty)
                {
                    CancelAction();
                    return;
                }

                IsExecuting = true;
                State = ActionState.Running;
                m_Actions.CurrentAction = this;
                actionsMaster.CurrentAction = this;

                m_InputManager.GetInputActionOnCurrentMap("UseItem").Disable();
                m_AnimatorDataHandler.OverrideAnimatorController[currentStructure.overrideClip] = usingLogicClone.useItemMotion;

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
            catch (System.Exception ex)
            {
                Debug.LogError($"Error executing action: {ex.Message}");
                CancelationTS.Cancel();
            }
        }

        protected override void ExecuteInSubActionEnter()
		{
            usingLogicClone.Use(itemSlotTag.tag, m_InventoryAndEquipment, m_Statistics);
        }

        public override void PerformSpecialFX()
        {
            m_FXManager.PerformAttackFX(fxData);
        }

        public override void ResetValues()
		{
            if (weaponObj != null) weaponObj.SetActive(true);
            if (needSetLayerWeight && overrideLayerIndex > -1) m_Animator.SetLayerWeight(overrideLayerIndex, 1);
            m_InputManager.GetInputActionOnCurrentMap("UseItem").Enable();
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