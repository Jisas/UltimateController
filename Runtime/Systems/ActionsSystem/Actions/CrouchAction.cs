using UltimateFramework.LocomotionSystem;
using UltimateFramework.InventorySystem;
using UltimateFramework.Inputs;
using UltimateFramework.Utils;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;


namespace UltimateFramework.ActionsSystem
{
    [CreateAssetMenu(menuName = "Ultimate Framework/Systems/Actions/Action Asset/Crouch")]
    public class CrouchAction : BaseAction
    {
        #region PublicFields
        public float motionSpeed = 1.0f;
        public int delayToActiveGlobalPose = 50;
        #endregion

        #region Components
        private InventoryAndEquipmentComponent m_InventoryAndEquipment;
        private BaseLocomotionComponent m_Locomotion;
        private EntityActionInputs m_InputManager;
        private ActionsComponent m_Actions;
        #endregion

        public override void StartConfig(BaseAction action, ActionStructure currentStructure)
        {
            action = this;
            base.StartConfig(action, currentStructure);

            m_InventoryAndEquipment = GetComponentByName<InventoryAndEquipmentComponent>("InventoryAndEquipmentComponent");
            m_Actions = GetComponentByName<ActionsComponent>("ActionsComponent");
            m_Locomotion = GetComponentByName<BaseLocomotionComponent>("BaseLocomotionComponent");
            m_InputManager = GetComponentByName<EntityActionInputs>("EntityActionInputs");
        }

        public override async Task Execute(EntityActionsManager actionsMaster, ActionStructure currentStructure, Animator animator, CancellationToken ct)
        {
            try
            {
                if (actionsMaster.IsHigherOrEqualPriorityActionExecuting(this))
                {
                    CancelAction(currentStructure);
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
                m_Locomotion.CanJump = false;

                var layerIndex = animator.GetLayerIndex(currentStructure.layerMask);
                int[] excludeLayers = new int[] { layerIndex };
                PlayActionAnimation(animator, layerIndex, currentStructure, motionSpeed, excludeLayersForDesactive: excludeLayers);

                var currentWeaponName = m_InventoryAndEquipment.GetCurrentRightWeaponObject().GetComponent<WeaponBehaviour>().itemName;
                var movementStruct = m_Locomotion.LocomotionMaster.FindMap(currentWeaponName).movement;
                var overrideLayer = m_Locomotion.LocomotionMaster.FindOverrideLayer(movementStruct, m_Locomotion.OverrideLayer);
                var overrideLayerMaskName = overrideLayer != null ? overrideLayer.globalPose.mask : "";
                var overrideLayerIndex = animator.GetLayerIndex(overrideLayerMaskName);

                await Task.Delay(delayToActiveGlobalPose);
                animator.SetLayerWeight(overrideLayerIndex, 1);

                await ActionFinishNotify(this); // Espera hasta que la accion termine           
                IsExecuting = false;
            }
            catch (System.Exception)
            {
                CancelationTS.Cancel();
                throw;
            }
        }

        public override void InterruptAction(ActionStructure currentStructure)
        {
            m_InputManager.FindInputAction(currentStructure.actionName).State = false;
            this.IsExecuting = false;
        }

        protected override void CancelAction(ActionStructure currentStructure)
        {
            m_InputManager.FindInputAction(currentStructure.actionName).State = false;
            this.IsExecuting = false;
        }
    }
}
