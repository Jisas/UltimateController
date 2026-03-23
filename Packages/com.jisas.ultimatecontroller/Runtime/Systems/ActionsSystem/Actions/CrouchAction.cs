using UltimateFramework.Utils;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using MyBox;

namespace UltimateFramework.ActionsSystem
{
    [CreateAssetMenu(menuName = "Ultimate Framework/Systems/Actions/Action Asset/Crouch")]
    public class CrouchAction : BaseAction
    {
        public float motionSpeed = 1.0f;
        public int delayToActiveGlobalPose = 50;

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
