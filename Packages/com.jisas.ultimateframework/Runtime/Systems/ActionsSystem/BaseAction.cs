using UltimateFramework.AnimatorDataSystem;
using UltimateFramework.LocomotionSystem;
using UltimateFramework.StatisticsSystem;
using UltimateFramework.CollisionsAndDamageSystem;
using UltimateFramework.InventorySystem;
using UltimateFramework.ItemSystem;
using Ultimateframework.FXSystem;
using System.Collections.Generic;
using UltimateFramework.Inputs;
using UltimateFramework.Utils;
using UltimateFramework.Tools;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using System;

namespace UltimateFramework.ActionsSystem
{
    [Icon("Assets/UltimateFramework/Resources/Img/Action_Asset_Icon.png")]
    public class BaseAction : ScriptableObject
    {
        public bool cantBeInterrupted;

        // properties
        public ActionState State { get; set; }
        public ActionsPriority Priority { get; set; }
        public bool IsExecuting { get; protected set; } = false;
        public ActionStructure CurrentStructure { get; set; }
        public CancellationTokenSource CancelationTS { get; set; }

        // components
        protected GameObject Owner;
        protected ItemDatabase itemDB;
        protected Animator m_Animator;
        protected ActionsComponent m_Actions;
        protected EntityActionInputs m_InputManager;
        protected TPTargetingManager m_Targeting;
        protected FXManagerComponent m_FXManager;
        protected DamageComponent m_DamageHandler;
        protected StatisticsComponent m_Statistics;
        protected BaseLocomotionComponent m_Locomotion;
        protected AnimatorDataHandler m_AnimatorStatesDataHandler;
        protected InventoryAndEquipmentComponent m_InventoryAndEquipment;

        // protected values
        protected bool inSubAction;
        protected int offsetTime;
        protected bool needResetLastActiveLayers = false;
        protected bool needResetLastDesactiveLayers = false;
        protected bool jumpNextAttackAnim = false;

        // private values
        private List<int> tempActiveLayers = new();
        private List<int> tempDesactiveLayer = new();

        public BaseAction Clone()
        {
            return Instantiate(this);
        }
        public void SetupAction(GameObject owner)
        {
            Owner = owner;
            itemDB = SettingsMasterData.Instance.itemDB;
            m_Animator = owner.GetComponent<Animator>();
            m_Actions = owner.GetComponent<ActionsComponent>();
            m_InputManager = owner.GetComponent<EntityActionInputs>();
            m_Targeting = owner.GetComponent<TPTargetingManager>();
            m_FXManager = owner.GetComponent<FXManagerComponent>();
            m_Statistics = owner.GetComponent<StatisticsComponent>();
            m_Locomotion = owner.GetComponent<BaseLocomotionComponent>();
            m_DamageHandler = owner.GetComponent<CharacterDamageHandler>();
            m_AnimatorStatesDataHandler = owner.GetComponent<AnimatorDataHandler>();
            m_InventoryAndEquipment = owner.GetComponent<InventoryAndEquipmentComponent>();

            State = ActionState.NotStarted;
        }

        public virtual void StartConfig(BaseAction action, ActionStructure currentStructure) 
        {
            action.CurrentStructure = currentStructure;
            action.CancelationTS = new();
            action.State = ActionState.NotStarted;
            action.IsExecuting = false;
        }
        public virtual async Task Execute(EntityActionsManager actionsMaster, ActionStructure currentStructure, Animator animator, CancellationToken ct) => await Task.Yield();
        public virtual void InterruptAction() { }
        public virtual void InterruptAction(ActionStructure currentStructure) { }
        protected virtual void CancelAction() { }
        protected virtual void CancelAction(ActionStructure currentStructure) { }
        public virtual void ResetValues() { }
        protected async Task TimerToReset(int time)
        {
            await Task.Delay(time);
        }
        protected async Task TimerToReset(int time, CancellationToken token)
        {
            await Task.Delay(time, token);
        }
        protected async Task ActionFinishNotify(BaseAction action)
        {
            while (action.State == ActionState.Running)
            {
                await Task.Yield();
            }
        }

        protected void PlayActionAnimation(Animator animator, int layerIndex, ActionStructure currentStructure, float motionSpeed, int[] excludeLayersForDesactive = default, int[] excludeLayersForActive = default, CancellationToken cancelToken = default)
        {
            if (!cancelToken.IsCancellationRequested)
            {
                bool hasIgnoreBehaviour = AnimatorUtilities.HasStateBehaviour<IgnoreBehaviour>(m_AnimatorStatesDataHandler.LayerStates, layerIndex, currentStructure.animStateName);
                bool hasMultipleActiveLayers = AnimatorUtilities.HasMultipleActiveLayers(animator);
                bool isUpperBodyLayer = layerIndex == animator.GetLayerIndex(m_Locomotion.UpperBodyMaskName);
                bool layerAreControlledByBehaviour = false;

                if (hasIgnoreBehaviour)
                {
                    string stateFullPath = AnimatorUtilities.GetStateFullPath(m_AnimatorStatesDataHandler.LayerStates, currentStructure.animStateName, layerIndex);
                    IgnoreBehaviour currentBehaviour = AnimatorUtilities.GetStateBehaviourOfType<IgnoreBehaviour>(m_AnimatorStatesDataHandler.LayerStates, layerIndex, stateFullPath) as IgnoreBehaviour;
                    bool ignoreAllOthersLayer = currentBehaviour.thingsToIgnore.Contains(IgnoreOptions.AllOtherActiveLayers);
                    layerAreControlledByBehaviour = ignoreAllOthersLayer;
                    needResetLastActiveLayers = !ignoreAllOthersLayer;
                }

                if (currentStructure.forceFullBodyOnly && hasMultipleActiveLayers && !layerAreControlledByBehaviour)
                {
                    needResetLastActiveLayers = true;
                    var activeLayers = AnimatorUtilities.GetActiveLayersIndices(animator, excludeLayersForDesactive);
                    tempActiveLayers = activeLayers;

                    foreach (var index in activeLayers)
                    {
                        animator.SetLayerWeight(index, 0);
                    }
                }
                else if (!currentStructure.forceFullBodyOnly && hasMultipleActiveLayers)
                {
                    if (isUpperBodyLayer)
                    {
                        needResetLastDesactiveLayers = true;
                        var desactiveLayers = AnimatorUtilities.GetDesactiveLayersIndices(animator, excludeLayersForActive);
                        tempDesactiveLayer = desactiveLayers;

                        foreach (var index in desactiveLayers)
                        {
                            animator.SetLayerWeight(index, 1);
                        }
                    }
                    else
                    {
                        List<int> layerWithState = AnimatorUtilities.GetActiveLayersWithState(animator, m_AnimatorStatesDataHandler.LayerStates, currentStructure.animStateName);
                        foreach (var index in layerWithState)
                        {
                            animator.Play(currentStructure.animStateName, index, 0);
                        }
                        return;
                    }
                }
                else if (!currentStructure.forceFullBodyOnly && !hasMultipleActiveLayers && isUpperBodyLayer)
                {
                    needResetLastDesactiveLayers = true;
                    var desactiveLayers = AnimatorUtilities.GetDesactiveLayersIndices(animator, excludeLayersForActive);
                    tempDesactiveLayer = desactiveLayers;

                    foreach (var index in desactiveLayers)
                    {
                        animator.SetLayerWeight(index, 1);
                    }
                }

                animator.SetFloat(m_Locomotion.MotionMultiplier, motionSpeed);
                animator.Play(currentStructure.animStateName, layerIndex, 0);
            }
        }
        protected void ResetLayerWeights()
        {
            if (needResetLastActiveLayers)
            {
                foreach (var index in tempActiveLayers)
                {
                    m_Animator.SetLayerWeight(index, 1);
                }
                tempActiveLayers.Clear();               
            }
            else if (needResetLastDesactiveLayers)
            {
                foreach (var index in tempDesactiveLayer)
                {
                    m_Animator.SetLayerWeight(index, 0);
                }
                tempActiveLayers.Clear();
            }
        }

        public void HanddlerEnterSubAction(ActionStructure currentStructure)
        {
            inSubAction = true;
            ExecuteInSubActionEnter();

            if (!String.IsNullOrEmpty(currentStructure.actionCost.statType.tag) && currentStructure.actionCost.value > 0)
            {
                var cost = currentStructure.actionCost;
                ApplyActionCost(cost.statType.tag, cost.value);
            }
        }
        protected async Task SubActionEnter(int delay, BaseAction action)
        {
            while (!inSubAction)
            {
                await Task.Delay(delay, action.CancelationTS.Token);
            }
        }
        protected virtual void ExecuteInSubActionEnter() { }

        public void HanddlerExitSubAction()
        {
            inSubAction = false;
            ExecuteInSubActionExit();
        }
        protected async Task SubActionExit(int delay, BaseAction action)
        {
            while (inSubAction)
            {
                await Task.Delay(delay, action.CancelationTS.Token);
            }
        }
        protected virtual void ExecuteInSubActionExit() { }

        public void HanddlerApplyModifiers(bool revert = false)
        {
            if (CurrentStructure.modifiers.Count > 0)
            {
                foreach (var modifier in CurrentStructure.modifiers)
                {
                    var tag = modifier.tag;
                    var valueType = modifier.valueType;
                    var value = modifier.value;
                    var currentOperation = GetOperation(modifier);
                    var isPercentage = valueType == UltimateFramework.Utils.ValueType.Percentage;

                    if (m_Statistics.primaryAttributes.Count > 0)
                    {
                        var pattr = m_Statistics.FindPrimaryAttribute(tag);

                        if (pattr != null)
                        {
                            pattr.CurrentValue = !revert ?
                                m_Statistics.ApplyModifyAttributesOrStatsOperation(currentOperation, pattr.CurrentValue, value, isPercentage) :
                                m_Statistics.ApplyModifyAttributesOrStatsOperation(m_Statistics.SubtractOnCurrentValue, pattr.CurrentValue, value, isPercentage);
                        }
                    }

                    if (m_Statistics.attributes.Count > 0)
                    {
                        var attr = m_Statistics.FindAttribute(tag);

                        if (attr != null)
                        {
                            attr.CurrentValue = !revert ?
                                m_Statistics.ApplyModifyAttributesOrStatsOperation(currentOperation, attr.CurrentValue, value, isPercentage) :
                                m_Statistics.ApplyModifyAttributesOrStatsOperation(m_Statistics.SubtractOnCurrentValue, attr.CurrentValue, value, isPercentage);
                        }
                    }

                    if (m_Statistics.stats.Count > 0)
                    {
                        if (m_Statistics.stats.Count > 0)
                        {
                            var stat = m_Statistics.FindStatistic(tag);

                            if (stat != null)
                            {
                                stat.CurrentValue = !revert ?
                                    m_Statistics.ApplyModifyAttributesOrStatsOperation(currentOperation, stat.CurrentValue, value, isPercentage) :
                                    m_Statistics.ApplyModifyAttributesOrStatsOperation(m_Statistics.SubtractOnCurrentValue, stat.CurrentValue, value, isPercentage);
                            }
                        }
                    }
                }
            }
        }
        private void ApplyActionCost(string stat, float cost)
        {
            //if (m_Statistics.CanConsumeStats) --> Apply when de game states (combat, out combat) are implemented
            m_Statistics.DecreaseCurrentValueOfStat(stat, cost);
        }
        private StatisticsComponent.Operation GetOperation(ActionStatisticsModifier modifier)
        {
            return modifier.opType switch
            {
                OperationType.Sum => m_Statistics.SumOnCurrentValue,
                OperationType.Multiply => m_Statistics.MultiplyOnCurrentValue,
                _ => throw new InvalidOperationException("Invalid operation"),
            };
        }

        public void HanddlerNextAttackAnim()
        {
            jumpNextAttackAnim = true;
        }
        protected void ResetJumpToNexAttack()
        {
            jumpNextAttackAnim = false;
        }
        protected async Task NextAttackNotify(BaseAction action)
        {
            while (action.jumpNextAttackAnim == false)
            {
                await Task.Yield();
            }
        }

        public virtual void PerformFX(MainHand hand) { }
        public virtual void PerformSpecialFX() { }
    }
}
