using UltimateFramework.CollisionsAndDamageSystem;
using UltimateFramework.LocomotionSystem;
using UltimateFramework.StatisticsSystem;
using UltimateFramework.InventorySystem;
using Ultimateframework.FXSystem;
using UltimateFramework.Inputs;
using UltimateFramework.Utils;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using System;
using UltimateFramework.AnimatorDataSystem;

namespace UltimateFramework.ActionsSystem
{
    [CreateAssetMenu(menuName = "Ultimate Framework/Systems/Actions/Action Asset/Attack")]
    public class AttackAction : BaseAction
    {
        #region SerialiedFields
        [SerializeField, Range(0, 0.3f), Tooltip("Smmoth of rotation on targeting attack")] 
        private float smootRotationOnAttack = 0.0f;
        [SerializeField, Range(0, 2000), Tooltip("Time in mili-seconds")]
        private int timeToResetCombo;
        [SerializeField] 
        private AttackData[] attacksData;
        [SerializeField] 
        private bool enableJumpAttack;
        [SerializeField, ConditionalField("enableJumpAttack", true)]
        private AttackData jumpAttackData;
        private CancellationTokenSource inputListenerCTS;
        private CancellationTokenSource resetTimerCTS;
        #endregion

        #region Components
        private InventoryAndEquipmentComponent m_InventoryAndEquipment;
        private AnimatorDataHandler m_AnimatorDataHandler;
        private BaseLocomotionComponent m_Locomotion;
        private EntityActionInputs m_InputManager;
        private StatisticsComponent m_Statistics;
        private DamageComponent m_DamageHandler;
        private FXManagerComponent m_FXManager; 
        private ActionsComponent m_Actions;
        #endregion

        #region PrivateFields
        private int currentClipIndex;
        private int callAmmount = 0;
        private bool isClipA = true;
        private bool isProcessingInput;
        private float lastSmoothRotation;
        private Task inputListener;
        private Task completeTask;
        private Task reset;
        #endregion

        [Serializable]
        public struct AttackData
        {
            public AnimationClip attackClip;
            public float motionSpeed;
            public FXData fxData;
        }

        public override void StartConfig(BaseAction action, ActionStructure currentStructure)
        {
            action = this;
            base.StartConfig(action, currentStructure);

            m_Statistics = GetComponentByName<StatisticsComponent>("StatisticsComponent");
            m_InventoryAndEquipment = GetComponentByName<InventoryAndEquipmentComponent>("InventoryAndEquipmentComponent");
            m_Actions = GetComponentByName<ActionsComponent>("ActionsComponent");
            m_Locomotion = GetComponentByName<BaseLocomotionComponent>("BaseLocomotionComponent");
            m_InputManager = GetComponentByName<EntityActionInputs>("EntityActionInputs");
            m_DamageHandler = GetComponentByName<DamageComponent>("DamageComponent");
            m_FXManager = GetComponentByName<FXManagerComponent>("FXManagerComponent");
            m_AnimatorDataHandler = GetComponentByName<AnimatorDataHandler>("AnimatorDataHandler");

            callAmmount = 0;
            currentClipIndex = 0;
            isProcessingInput = false;
            inputListener = null;
            reset = null;
            completeTask = null;
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

                if (m_InventoryAndEquipment.GetCurrentMainWeapon().HandSocket.childCount <= 0 && m_Actions.FindSpecificActionsGroup("Moveset.Unarmed") == null)
                {
                    Debug.LogWarning("Not have any weapon on hand and not have actions for unarmed combat");
                    this.IsExecuting = false;
                    callAmmount = 0;
                    return;
                }

                IsExecuting = true;
                m_Actions.CurrentAction = this;
                actionsMaster.CurrentAction = this;
                m_Statistics.CanRegenerateStats = false;
                m_Locomotion.CanJump = false;

                lastSmoothRotation = m_Locomotion.Rotationspeed;
                m_Locomotion.Rotationspeed = smootRotationOnAttack;

                var combatActionsComprobement = m_InventoryAndEquipment.GetCurrentMainWeapon().HandSocket.childCount <= 0 && m_Actions.FindSpecificActionsGroup("Moveset.Unarmed") != null;
                var currentWeapon = combatActionsComprobement ? null : m_InventoryAndEquipment.GetCurrentMainWeapon().WeaponObject.GetComponent<WeaponBehaviour>().Item;
                var currentSpecificActions = combatActionsComprobement ?
                    m_Actions.FindSpecificActionsGroup("Moveset.Unarmed") :
                    m_Actions.FindSpecificActionsGroup(currentWeapon.actionsTag);

                callAmmount++;
                isProcessingInput = false;
                AnimationClip tempActionClip;

                if (callAmmount == 1)
                {
                    if (attacksData.Length > 0)
                    {
                        currentStructure.motion = attacksData[currentClipIndex].attackClip;
                        tempActionClip = currentStructure.overrideClip;
                        m_AnimatorDataHandler.OverrideAnimatorController[tempActionClip] = currentStructure.motion;
                    }

                    var layerIndex = animator.GetLayerIndex(currentStructure.layerMask);
                    animator.applyRootMotion = true;

                    int[] excludeLayers = new int[]
                    {
                        layerIndex,
                        animator.GetLayerIndex(m_Locomotion.RightHandMaskName),
                        animator.GetLayerIndex(m_Locomotion.RightAndLeftHandMaskName)
                    };
                    PlayActionAnimation(animator, layerIndex, currentStructure, attacksData[currentClipIndex].motionSpeed, excludeLayersForDesactive: excludeLayers);
                }

                if (Owner.CompareTag("Player")) InputsManager.Player.Attack.Disable();

                await NextAttackNotify(this);

                if (Owner.CompareTag("Player")) InputsManager.Player.Attack.Enable();

                if (!actionsMaster.MeetsActionCost(m_Statistics))
                {
                    ResetValues();
                    IsExecuting = false;
                    return;
                }

                m_InputManager.ResetPressCount(currentStructure.actionName);

                if (inputListener != null && !inputListener.IsCompleted)
                {
                    inputListenerCTS?.Cancel();
                    await inputListener;
                }

                inputListenerCTS = new CancellationTokenSource();
                inputListener = InputListener(currentStructure, inputListenerCTS.Token);

                if (reset != null && !reset.IsCompleted)
                {
                    resetTimerCTS?.Cancel();
                    await reset;
                }

                resetTimerCTS = new CancellationTokenSource();
                reset = TimerToReset(timeToResetCombo, resetTimerCTS.Token);

                completeTask = await Task.WhenAny(inputListener, reset);
                await completeTask;

                if (inputListener != null && completeTask == inputListener)
                {
                    if (isProcessingInput) return;

                    isProcessingInput = true;
                    currentClipIndex = (currentClipIndex + 1) % attacksData.Length;

                    currentStructure.motion = attacksData[currentClipIndex].attackClip;
                    tempActionClip = currentStructure.overrideClip;
                    m_AnimatorDataHandler.OverrideAnimatorController[tempActionClip] = currentStructure.motion;

                    var layerIndex = animator.GetLayerIndex(currentStructure.layerMask);
                    int[] excludeLayers = new int[]
                    {
                            layerIndex,
                            animator.GetLayerIndex(m_Locomotion.RightHandMaskName),
                            animator.GetLayerIndex(m_Locomotion.RightAndLeftHandMaskName)
                    };
                    PlayActionAnimation(animator, layerIndex, currentStructure, attacksData[currentClipIndex].motionSpeed, excludeLayersForDesactive: excludeLayers);

                    isClipA = !isClipA;
                    isProcessingInput = false;

                    m_InputManager.ResetPressCount(currentStructure.actionName);
                    ResetJumpToNexAttack();
                }

                if (reset != null && completeTask == reset)
                {
                    await ActionFinishNotify(this);
                    ResetValues();
                    this.IsExecuting = false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error executing action: {ex.Message}");
                CancelAction();
            }
            finally
            {
                m_Locomotion.Rotationspeed = lastSmoothRotation;
                m_Locomotion.CanJump = true;
            }
        }

        private async Task InputListener(ActionStructure currentStructure, CancellationToken ct)
        {
            try
            {
                while (m_InputManager.FindInputAction(currentStructure.actionName).PressCount <= 0)
                {
                    await Task.Delay(1, ct);
                }
            }
            catch (TaskCanceledException) { }
        }

        protected override async Task GapCloser(ActionStructure currentStructure)
        {
            if (currentStructure.useMotionWarp)
            {
                var minRange = currentStructure.motionWarp.minRange;
                var maxRange = currentStructure.motionWarp.maxRange;
                var speed = currentStructure.motionWarp.speed;

                if (m_Actions.EvaluateGapCloser(minRange, maxRange))
                    m_Animator.applyRootMotion = true;

                while (m_Actions.GapCloserDistanceVerify() > currentStructure.motionWarp.stopDistance)
                {
                    m_Animator.applyRootMotion = false;
                    m_Actions.PerformGapCloaser(speed);
                    await Task.Yield();
                }
            }
        }

        protected override void ExecuteInSubActionEnter()
        {
            if (m_Actions.IsGameRunning)
            {
                m_DamageHandler.SetAllowCollisions(true);
            }
        }

        protected override void ExecuteInSubActionExit()
        {
            if (m_Actions.IsGameRunning)
            {
                m_DamageHandler.SetAllowCollisions(false);                
            }
        }

        public override void PerformFX(MainHand hand)
        {
            m_FXManager.PerformAttackFX(attacksData[currentClipIndex].fxData, hand);
        }

        public override void ResetValues()
        {
            callAmmount = 0;
            isClipA = true;
            isProcessingInput = false;
            inputListener = null;
            reset = null;
            completeTask = null;
            m_Statistics.CanRegenerateStats = true;
            m_Locomotion.CanJump = true;
            m_Locomotion.Rotationspeed = lastSmoothRotation;
            currentClipIndex = (currentClipIndex + 1) % attacksData.Length;
            if (!m_Locomotion.useRootMotionOnMovement) m_Animator.applyRootMotion = false;
            ResetJumpToNexAttack();
            ResetLayerWeights();
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