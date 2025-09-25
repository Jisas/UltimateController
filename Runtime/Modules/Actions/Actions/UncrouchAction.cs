using UltimateFramework.LocomotionSystem;
using UltimateFramework.InventorySystem;
using UltimateFramework.Inputs;
using UltimateFramework.Utils;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace UltimateFramework.ActionsSystem
{
    [CreateAssetMenu(menuName = "Ultimate Framework/Systems/Actions/Action Asset/Uncrouch")]
    public class UncrouchAction : BaseAction
    {
        #region PublicFields
        public float motionSpeed = 1.0f;
        public int delayToDesactiveGlobalPose = 50;
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
                m_Locomotion.CanJump = true;

                var layerIndex = animator.GetLayerIndex(currentStructure.layerMask);
                PlayActionAnimation(animator, layerIndex, currentStructure, motionSpeed);

                await Task.Delay(delayToDesactiveGlobalPose, CancelationTS.Token); // espera antes de desactivar la pose global
                var currentWeaponName = m_InventoryAndEquipment.GetCurrentRightWeaponObject().GetComponent<WeaponBehaviour>().itemName; // busca el nombre del arma actual
                var currentlocomotionMap = m_Locomotion.LocomotionMaster.FindMap(currentWeaponName); // busca el mapa de locomocion actual
                var movementStruct = currentlocomotionMap.movement; // extare la extructura de movimiento del mapa de locomocion actual
                var overrideLayer = m_Locomotion.LocomotionMaster.FindOverrideLayer(movementStruct, m_Locomotion.OverrideLayer);
                var overrideLayerMaskName = overrideLayer != null ? overrideLayer.globalPose.mask : ""; // busca la mascara de la pose global asignada a la capa de sobre escritura del mapa de locomocion actual

                // Desactiva el global pose ne base a la mascara asignada en el mapa de locomocion actual
                var overrideLayerIndex = animator.GetLayerIndex(overrideLayerMaskName);
                animator.SetLayerWeight(overrideLayerIndex, 0);

                await ActionFinishNotify(this); // Espera hasta que la accion termine

                // Si el mapa de locomocion actial necesita usar dos capas desde el inicio y su capa de sobre escritura es LoweBodyMask, entonces la activa
                overrideLayer = m_Locomotion.LocomotionMaster.FindOverrideLayer(movementStruct, m_Locomotion.OverrideLayer);
                overrideLayerMaskName = overrideLayer != null ? overrideLayer.movement.motionMask : "";

                if (currentlocomotionMap.useOverrideAtStartup && !string.IsNullOrEmpty(overrideLayerMaskName) && overrideLayerMaskName == m_Locomotion.LowerBodyMaskName)
                {
                    overrideLayerIndex = animator.GetLayerIndex(overrideLayerMaskName);
                    animator.SetLayerWeight(overrideLayerIndex, 1);
                }

                IsExecuting = false;
            }
            catch (System.Exception)
            {
                CancelationTS.Cancel();
                throw;
            }
        }

        public override void InterruptAction()
        {
            m_InputManager.FindInputAction("Crouch").State = true;
            CancelationTS.Cancel();

            this.IsExecuting = false;
        }

        protected override void CancelAction()
        {
            m_InputManager.FindInputAction("Crouch").State = true;
            CancelationTS.Cancel();

            this.IsExecuting = false;
        }
    }
}
