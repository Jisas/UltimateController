using UltimateController.AnimatorDataSystem;
using UltimateController.LocomotionSystem;
using UltimateController.StatisticsSystem;
using UltimateController.InventorySystem;
using UltimateController.Inputs;
using UltimateController.Utils;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace UltimateController.ActionsSystem
{
    [CreateAssetMenu(menuName = "Ultimate Framework/Systems/Actions/Action Asset/UnequipMelee")]
    public class UnequipWeaponAction : BaseAction
    {
        #region PublicFields
        public AnimationClip unequipMotion;
        public float motionSpeed = 1.0f;
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

                if (m_InventoryAndEquipment.GetCurrentMainWeapon().WeaponObject == null)
                {
                    Debug.LogWarning("Not have any weapon on hand");
                    this.IsExecuting = false;
                    return;
                }

                IsExecuting = true;
                State = ActionState.Running;
                m_Actions.CurrentAction = this;
                actionsMaster.CurrentAction = this;

                if (Owner.CompareTag("Player")) m_InputManager.GetInputActionOnCurrentMap("EquipMelee").Disable();

                currentStructure.motion = unequipMotion;
                m_AnimatorDataHandler.OverrideAnimatorController[currentStructure.overrideClip] = currentStructure.motion;

                await Task.Delay(50, CancelationTS.Token);
                int layerIndex = animator.GetLayerIndex(currentStructure.layerMask);

                int[] excludeLayers = new int[]
                {
                    animator.GetLayerIndex(m_AnimatorDataHandler.LowerBodyMaskName),
                    animator.GetLayerIndex(m_AnimatorDataHandler.RightHandMaskName),
                    animator.GetLayerIndex(m_AnimatorDataHandler.RightAndLeftHandMaskName)
                };
                PlayActionAnimation(animator, layerIndex, currentStructure, motionSpeed, excludeLayersForActive: excludeLayers);

                weaponObj = m_InventoryAndEquipment.GetCurrentMainWeapon().WeaponObject;
                weaponComp = weaponObj.GetComponent<WeaponComponent>();

                // Si estas agachado y esta accion no se ejecuta en la capa FullBodyMask y no fuerza el uso del cuerpo completo,
                // cuando equipas un arma con un mapa de locomocion que contiene capas de sobre escritura que se usan desde el principio, las desactiva.
                if (m_Locomotion.IsCrouch)
                {
                    var currentWeaponName = weaponComp.WeaponBehaviour.itemName;
                    var currentlocomotionMap = m_Locomotion.LocomotionMaster.FindMap(currentWeaponName);
                    var movementStruct = currentlocomotionMap.movement;
                    var overrideLayer = m_Locomotion.LocomotionMaster.FindOverrideLayer(movementStruct, m_Locomotion.OverrideLayer);
                    var overrideLayerMaskName = overrideLayer != null ? overrideLayer.globalPose.mask : "";

                    var overrideLayerIndex = animator.GetLayerIndex(overrideLayerMaskName);
                    animator.SetLayerWeight(overrideLayerIndex, 0);
                }

                await ActionFinishNotify(this);
                bool isNotForcing = currentStructure.layerMask != m_AnimatorDataHandler.FullBodyMaskName && !currentStructure.forceFullBodyOnly;

                if (m_Locomotion.IsCrouch && isNotForcing)
                    await m_Locomotion.SwitchLocomotionMapAsync(m_Locomotion.LocomotionMap, false, true);
                else await m_Locomotion.SwitchLocomotionMapAsync(m_Locomotion.LocomotionMap);

                ResetLayerWeights();
                if (Owner.CompareTag("Player")) m_InputManager.GetInputActionOnCurrentMap("EquipMelee").Enable();
                IsExecuting = false;
            }
            catch (System.Exception)
            {
                CancelationTS.Cancel();
                throw;
            }
        }

        private void Unequip(GameObject weaponObject, Transform socket, WeaponComponent weaponComponent, bool isOffHandWeapon)
        {
            if (socket != null && socket.CompareTag("Socket"))
            {
                // Mueve el arma al bodySocket
                weaponObject.transform.SetParent(socket);
                weaponObject.transform.SetPositionAndRotation(socket.position, socket.rotation);

                if (weaponComponent.Item.hand == WeaponHand.TwoHand) m_InventoryAndEquipment.OnUnequipTwoHandWeapon?.Invoke();
            }

            if (isOffHandWeapon) m_InventoryAndEquipment.SwitchEquipmentSlotType(SocketType.Body);
            else
            {
                m_InventoryAndEquipment.SwitchEquipmentSlotType(SocketType.Body);
                m_Locomotion.SetCombatType(CombatType.Unarmed);
            }
        }

        protected override void ExecuteInSubActionEnter()
        {
            var weaponItem_R = weaponComp.WeaponBehaviour.Item;
            var socket_R = weaponComp.BodySocket;

            Unequip(weaponObj, socket_R, weaponComp, false);

            if (m_InventoryAndEquipment.LeftWeapon.WeaponObject != null && weaponItem_R.hand != WeaponHand.TwoHand)
            {
                var weaponObj_L = m_InventoryAndEquipment.LeftWeapon.WeaponObject;
                var weaponComp_L = weaponObj_L.GetComponent<WeaponComponent>();
                var socket_L = weaponComp_L.BodySocket;
                Unequip(weaponObj_L, socket_L, weaponComp_L, true);
            }
        }

        public override void InterruptAction(ActionStructure currentStructure)
        {
            ResetLayerWeights();
            m_InputManager.FindInputAction(currentStructure.actionName).State = true;
            m_InputManager.GetInputActionOnCurrentMap("EquipMelee").Enable();
            CancelationTS.Cancel();
            this.State = ActionState.NotStarted;
            this.IsExecuting = false;
        }

        protected override void CancelAction(ActionStructure currentStructure)
        {
            ResetLayerWeights();
            m_InputManager.FindInputAction(currentStructure.actionName).State = true;
            m_InputManager.GetInputActionOnCurrentMap("EquipMelee").Enable();
            CancelationTS.Cancel();
            State = ActionState.NotStarted;
            this.IsExecuting = false;
        }
    }
}
