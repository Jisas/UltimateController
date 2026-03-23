using UltimateFramework.LocomotionSystem;
using Ultimateframework.FXSystem;
using UltimateFramework.Utils;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using System;

namespace UltimateFramework.ActionsSystem
{
    [CreateAssetMenu(menuName = "Ultimate Framework/Systems/Actions/Action Asset/Attack")]
    public class AttackAction : BaseAction
    {
        [SerializeField, Range(0, 0.3f), Tooltip("Smmoth of rotation on targeting attack")] 
        private float smootRotationOnAttack = 0.0f;
        [SerializeField, Range(100, 2000), Tooltip("Time in mili-seconds")]
        private int timeToResetCombo;
        [SerializeField] 
        private AttackData[] attacksData;
        [SerializeField] 
        private bool enableJumpAttack;
        [SerializeField, ConditionalField("enableJumpAttack", true)]
        private AttackData jumpAttackData;

        private int currentClipIndex;
        private int callAmmount = 0;
        private bool isClipA = true;
        private bool waitingForClick;
        private bool isProcessingClick;
        private Task clickListener;
        private Task reset;
        private Task completeTask;
        private float lastSmoothRotation;

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

            callAmmount = 0;
            currentClipIndex = 0;
            waitingForClick = false;
            isProcessingClick = false;
            clickListener = null;
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

                bool jump = Owner.CompareTag("Player") ? m_InputManager.Jump : Owner.TryGetComponent<AILocomotionCommponent>(out var AILocomotion) && AILocomotion.Jump;

                if (!m_Locomotion.IsFalling && !jump)
                {
                    callAmmount++;
                    isProcessingClick = false;

                    AnimationClip tempActionClip;

                    if (callAmmount == 1)
                    {
                        if (attacksData.Length > 0)
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                currentStructure.motion = attacksData[currentClipIndex].attackClip;
                                tempActionClip = currentStructure.overrideClip;
                                m_Locomotion.OverrideAnimatorController[tempActionClip] = currentStructure.motion;
                            }
                        }

                        foreach (var actionStructure in currentSpecificActions?.actionsGroup.actions)
                        {
                            if (actionStructure.actionTag.tag == currentStructure.actionTag.tag)
                            {
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
                        }
                    }

                    if (Owner.CompareTag("Player")) m_InputManager.GetInputActionOnCurrentMap("Attack").Disable();

                    if (currentStructure.useMotionWarp)
                    {
                        var minRange = currentStructure.motionWarp.minRange;
                        var maxRange = currentStructure.motionWarp.maxRange;
                        var speed = currentStructure.motionWarp.speed;

                        if (m_Actions.EvaluateGapCloser(minRange, maxRange))
                        {
                            await GapCloser(speed, currentStructure);
                            m_Animator.applyRootMotion = true;
                        }
                    }

                    await NextAttackNotify(this);
                    if (Owner.CompareTag("Player")) m_InputManager.GetInputActionOnCurrentMap("Attack").Enable();
                    IsExecuting = false;

                    if (!waitingForClick)
                    {
                        if (!actionsMaster.MeetsActionCost(m_Statistics))
                        {
                            ResetValues();
                            IsExecuting = false;
                            return;
                        }

                        waitingForClick = true;
                        m_InputManager.ResetPressCount(currentStructure.actionName);
                        clickListener = ClickListener(currentStructure);
                        reset = TimerToReset(timeToResetCombo);
                        completeTask = await Task.WhenAny(clickListener, reset);
                        waitingForClick = false;
                    }

                    if (clickListener != null && completeTask == clickListener)
                    {
                        if (isProcessingClick) return;

                        isProcessingClick = true;
                        currentClipIndex = (currentClipIndex + 1) % attacksData.Length;

                        currentStructure.motion = attacksData[currentClipIndex].attackClip;
                        tempActionClip = currentStructure.overrideClip;
                        m_Locomotion.OverrideAnimatorController[tempActionClip] = currentStructure.motion;

                        var layerIndex = animator.GetLayerIndex(currentStructure.layerMask);
                        int[] excludeLayers = new int[]
                        {
                            layerIndex,
                            animator.GetLayerIndex(m_Locomotion.RightHandMaskName),
                            animator.GetLayerIndex(m_Locomotion.RightAndLeftHandMaskName)
                        };
                        PlayActionAnimation(animator, layerIndex, currentStructure, attacksData[currentClipIndex].motionSpeed, excludeLayersForDesactive: excludeLayers);

                        await Task.Delay(70);
                        isClipA = !isClipA;
                        isProcessingClick = false;

                        m_InputManager.ResetPressCount(currentStructure.actionName);
                        ResetJumpToNexAttack();
                    }

                    if (reset != null && completeTask == reset)
                    {
                        await ActionFinishNotify(this);
                        ResetValues();
                    }
                }
                else
                {
                    if (!enableJumpAttack || jumpAttackData.attackClip == null)
                    {
                        Debug.LogError("None jump attack motion");
                        IsExecuting = false;
                        return;
                    }

                    animator.applyRootMotion = true;
                    m_Locomotion.OverrideAnimatorController[currentStructure.overrideClip] = jumpAttackData.attackClip;

                    int layerIndex;
                    m_InputManager.GetInputActionOnCurrentMap("Attack").Disable();

                    foreach (var actionStructure in currentSpecificActions?.actionsGroup.actions)
                    {
                        if (actionStructure.actionTag.tag == currentStructure.actionTag.tag)
                        {
                            layerIndex = animator.GetLayerIndex(currentStructure.layerMask);
                            animator.applyRootMotion = true;
                            animator.Play(currentStructure.animStateName, layerIndex, 0);
                        }
                    }

                    await ActionFinishNotify(this);
                    m_Animator.applyRootMotion = m_Locomotion.useRootMotionOnMovement;
                    m_InputManager.GetInputActionOnCurrentMap("Attack").Enable();
                    IsExecuting = false;
                }
            }
            catch (OperationCanceledException)
            {
                CancelationTS.Cancel();
                throw;
            }
        }

        private async Task ClickListener(ActionStructure currentStructure)
        {
            while (m_InputManager.FindInputAction(currentStructure.actionName).PressCount <= 0)
            {
                await Task.Yield();
            }
        }

        private async Task GapCloser(float speed, ActionStructure currentStructure)
        {
            while (m_Actions.GapCloserDistanceVerify() > currentStructure.motionWarp.stopDistance)
            {
                m_Animator.applyRootMotion = false;
                m_Actions.PerformGapCloaser(speed);
                await Task.Delay(1, CancelationTS.Token);
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
            waitingForClick = false;
            isProcessingClick = false;
            clickListener = null;
            reset = null;
            completeTask = null;
            m_Statistics.CanRegenerateStats = true;
            m_Locomotion.CanJump = true;
            m_Locomotion.Rotationspeed = lastSmoothRotation;
            currentClipIndex = (currentClipIndex + 1) % attacksData.Length;
            if (!m_Locomotion.useRootMotionOnMovement) m_Animator.applyRootMotion = false;
            if (Owner.CompareTag("Player")) m_InputManager.GetInputActionOnCurrentMap("Attack").Enable();
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