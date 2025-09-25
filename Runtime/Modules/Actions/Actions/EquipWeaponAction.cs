using UltimateFramework.ItemSystem;
using UltimateFramework.Utils;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using System;
using UltimateFramework.AnimatorDataSystem;
using UltimateFramework.InventorySystem;
using UltimateFramework.LocomotionSystem;
using UltimateFramework.StatisticsSystem;
using UltimateFramework.Inputs;

namespace UltimateFramework.ActionsSystem
{
    [CreateAssetMenu(menuName = "Ultimate Framework/Systems/Actions/Action Asset/EquipMelee")]
    public class EquipWeaponAction : BaseAction
    {
        #region PublicFields
        public AnimationClip equipMotion;
        public float motionSpeed = 1.0f;
        public bool tryEquipOffHand = true;
        #endregion

        #region PrivateFields
        private GameObject weaponObj;
        private WeaponComponent weaponComp;
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
                    CancelAction(currentStructure);
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

                if (m_InventoryAndEquipment.GetCurrentMainWeapon().WeaponObject == null)
                {
                    Debug.LogWarning("Not have any weapon on body");
                    this.IsExecuting = false;
                    return;
                }

                IsExecuting = true;
                State = ActionState.Running;
                m_Actions.CurrentAction = this;
                actionsMaster.CurrentAction = this;

                if (Owner.CompareTag("Player")) m_InputManager.GetInputActionOnCurrentMap("EquipMelee").Disable();

                await Task.Delay(10);
                currentStructure.motion = equipMotion;
                m_AnimatorDataHandler.OverrideAnimatorController[currentStructure.overrideClip] = currentStructure.motion;  // sobre escribe la animacion concreta

                int layerIndex = animator.GetLayerIndex(currentStructure.layerMask);
                int[] excludeLayers = new int[]
                {
                    animator.GetLayerIndex(m_Locomotion.LowerBodyMaskName),
                    animator.GetLayerIndex(m_Locomotion.RightHandMaskName),
                    animator.GetLayerIndex(m_Locomotion.RightAndLeftHandMaskName)
                };
                PlayActionAnimation(animator, layerIndex, currentStructure, motionSpeed, excludeLayersForActive: excludeLayers);

                weaponObj = m_InventoryAndEquipment.GetCurrentMainWeapon().WeaponObject;
                weaponComp = weaponObj.GetComponent<WeaponComponent>();

                await ActionFinishNotify(this); // Espera hasta que la accion termine
                var weaponItem_R = weaponComp.WeaponBehaviour.Item;
                bool isNotForcing = currentStructure.layerMask != m_Locomotion.FullBodyMaskName && !currentStructure.forceFullBodyOnly;

                if (m_Locomotion.IsCrouch && isNotForcing)
                    await m_Locomotion.SwitchLocomotionMapAsync(weaponItem_R.name, false, true);
                else await m_Locomotion.SwitchLocomotionMapAsync(weaponItem_R.name);

                ResetLayerWeights();
                if (Owner.CompareTag("Player")) m_InputManager.GetInputActionOnCurrentMap("EquipMelee").Enable();
                IsExecuting = false;
            }
            catch (OperationCanceledException)
            {
                CancelationTS.Cancel();
                throw;
            }
        }

        private void Equip(GameObject weaponObject, Item weaponItem, Transform socket, bool isOffHandWeapon)
        {
            if (socket != null && socket.CompareTag("Socket"))
            {
                // Mueve el arma al handSocket
                weaponObject.transform.SetParent(socket);
                weaponObject.transform.SetPositionAndRotation(socket.position, socket.rotation);

                if (weaponItem.hand == WeaponHand.TwoHand) m_InventoryAndEquipment.OnEquipTwoHandWeapon?.Invoke();
            }

            if (isOffHandWeapon) m_InventoryAndEquipment.SwitchEquipmentSlotType(SocketType.Hand);
            else
            {
                m_InventoryAndEquipment.SwitchEquipmentSlotType(SocketType.Hand);
                m_Locomotion.SetCombatType(weaponItem.combatType);
            }
        }

        protected override void ExecuteInSubActionEnter()
        {
            var weaponItem_R = weaponComp.WeaponBehaviour.Item;
            var socket_R = weaponComp.HandSocket;

            // equipa el arma derecha
            Equip(weaponObj, weaponItem_R, socket_R, false); 

            // intenta equipar un arma izquierda
            if (tryEquipOffHand)
            {
                if (weaponItem_R.hand == WeaponHand.TwoHand)
                {
                    Debug.LogWarning("You cannot equip any offhand weapon because you have a two-handed weapon.");
                    return;
                }

                if (m_InventoryAndEquipment.LeftWeapon.WeaponObject == null)
                {
                    Debug.LogWarning("Not have any weapon on left body socket");
                    this.IsExecuting = false;
                    return;
                }

                var weaponObj_L = m_InventoryAndEquipment.LeftWeapon.WeaponObject;
                var weaponComp_L = weaponObj_L.GetComponent<WeaponComponent>();

                var weaponItem_L = weaponComp_L.Item;
                var socket_L = weaponComp_L.HandSocket;

                Equip(weaponObj_L, weaponItem_L, socket_L, true);
            }
        }

        public override void InterruptAction(ActionStructure currentStructure)
        {
            ResetLayerWeights();
            m_InputManager.FindInputAction(currentStructure.actionName).State = false;
            m_InputManager.GetInputActionOnCurrentMap("EquipMelee").Enable();
            CancelationTS.Cancel();
            this.State = ActionState.NotStarted;
            this.IsExecuting = false;
        }

        protected override void CancelAction(ActionStructure currentStructure)
        {
            ResetLayerWeights();
            m_InputManager.FindInputAction(currentStructure.actionName).State = false;
            m_InputManager.GetInputActionOnCurrentMap("EquipMelee").Enable();
            CancelationTS.Cancel();
            this.State = ActionState.NotStarted;
            this.IsExecuting = false;
        }
    }
}
