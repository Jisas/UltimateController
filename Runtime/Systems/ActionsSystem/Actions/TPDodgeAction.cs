using UltimateFramework.CollisionsAndDamageSystem;
using UltimateFramework.AnimatorDataSystem;
using UltimateFramework.LocomotionSystem;
using UltimateFramework.StatisticsSystem;
using UltimateFramework.InventorySystem;
using UltimateFramework.Inputs;
using UltimateFramework.Utils;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace UltimateFramework.ActionsSystem
{
	[CreateAssetMenu(menuName = "Ultimate Framework/Systems/Actions/Action Asset/TPDodge")]
	public class TPDodgeAction : BaseAction
	{
        #region PublicFields
        public float dodgeDistance = 5.0f;
        public float displacementDuration = 0.5f;
        public float motionSpeed = 1.0f;
        public string cameraState = "DodgeCamera";
        #endregion

        #region PrivateFields
        private Vector3 dodgeDirection;
        private LocomotionType lastLocomotionType;
        private float lastRotationspeed;
        private CharacterDamageHandler damageHandler;
        #endregion

        #region Components
        private InventoryAndEquipmentComponent m_InventoryAndEquipment;
        private AnimatorDataHandler m_AnimatorDataHandler;
        private BaseLocomotionComponent m_Locomotion;
        private EntityActionInputs m_InputManager;
        private StatisticsComponent m_Statistics;
        private DamageComponent m_DamageHandler;
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
            m_AnimatorDataHandler = GetComponentByName<AnimatorDataHandler>("AnimatorDataHandler");
            m_InputManager = GetComponentByName<EntityActionInputs>("EntityActionInputs");
            m_DamageHandler = GetComponentByName<DamageComponent>("DamageComponent");

            damageHandler = m_DamageHandler as CharacterDamageHandler;
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

                IsExecuting = true;
                m_Actions.CurrentAction = this;
                actionsMaster.CurrentAction = this;

                m_Locomotion.CanJump = false;
                m_Actions.IsDodging = true;
                m_Statistics.CanRegenerateStats = false;
                m_InputManager.GetInputActionOnCurrentMap("Dodge").Disable();
                m_AnimatorDataHandler.OverrideAnimatorController[currentStructure.overrideClip] = currentStructure.motion;

                lastLocomotionType = m_Locomotion.CurrentLocomotionType;
                lastRotationspeed = m_Locomotion.Rotationspeed;

                m_Locomotion.Rotationspeed = 0;
                if (m_Locomotion.useRootMotionOnMovement) animator.applyRootMotion = false;
                if (lastLocomotionType != LocomotionType.ForwardFacing) m_Locomotion.SetLocomotionType(LocomotionType.ForwardFacing, false);

                var isCrouching = m_Locomotion.IsCrouch;
                var currentWeapon = m_InventoryAndEquipment.GetCurrentRightWeaponObject();
                int overrideLayerIndex = -1;

                if (currentWeapon != null)
                {
                    var weaponName = currentWeapon.GetComponent<WeaponBehaviour>().itemName;
                    var movementStruct = m_Locomotion.LocomotionMaster.FindMap(weaponName).movement;
                    var overrideLayer = m_Locomotion.LocomotionMaster.FindOverrideLayer(movementStruct, m_Locomotion.OverrideLayer);
                    var overrideLayerMaskName = overrideLayer != null ? overrideLayer.globalPose.mask : "";
                    overrideLayerIndex = animator.GetLayerIndex(overrideLayerMaskName);
                }

                if (isCrouching && overrideLayerIndex > 0) animator.SetLayerWeight(overrideLayerIndex, 0);

                await Task.Delay(50);
                var layerIndex = animator.GetLayerIndex(currentStructure.layerMask);
                int[] excludeLayers = new int[]
                {
                    layerIndex
                };
                PlayActionAnimation(animator, layerIndex, currentStructure, motionSpeed, excludeLayersForDesactive: excludeLayers);

                await Task.Delay(50);
                dodgeDirection = m_Locomotion.CurrentMoveDirection;
                m_InputManager.GetInputActionOnCurrentMap("Move").Disable();
                damageHandler.CanTakeDamage = false;

                await DodgeMovement(animator.transform, dodgeDirection * dodgeDistance, displacementDuration, ct);
                await ActionFinishNotify(this);

                if (isCrouching && overrideLayerIndex > 0) animator.SetLayerWeight(overrideLayerIndex, 1);
                ResetValues();
                IsExecuting = false;
            }
            catch (System.Exception)
            {
                CancelationTS.Cancel();
                throw;
            }
        }

        async Task DodgeMovement(Transform transform, Vector3 direction, float duration, CancellationToken ct)
        {
            if (!ct.IsCancellationRequested)
            {
                Vector3 startPosition = transform.position;
                Vector3 target = startPosition + direction;
                float elapsed = 0;

                while (elapsed < duration)
                {
                    float t = elapsed / duration;
                    transform.position = Vector3.Lerp(startPosition, target, t);
                    elapsed += Time.deltaTime;
                    await Task.Yield();
                }

                transform.position = target;
            }
        }

        public override void ResetValues()
        {
            m_InputManager.GetInputActionOnCurrentMap("Move").Enable();
            m_InputManager.GetInputActionOnCurrentMap("Dodge").Enable();
            m_Locomotion.Rotationspeed = lastRotationspeed;
            m_Locomotion.SetLocomotionType(lastLocomotionType, false);
            if (m_Locomotion.useRootMotionOnMovement) m_Animator.applyRootMotion = true;

            m_Statistics.CanRegenerateStats = true;
            damageHandler.CanTakeDamage = true;
            m_Actions.IsDodging = false;
            m_Locomotion.CanJump = true;
            ResetLayerWeights();
        }

        public override void InterruptAction()
        {
            ResetValues();
            CancelationTS.Cancel();
            this.IsExecuting = false;
        }

        protected override void CancelAction()
        {
            ResetValues();
            CancelationTS.Cancel();
            this.IsExecuting = false;
        }
    }
}