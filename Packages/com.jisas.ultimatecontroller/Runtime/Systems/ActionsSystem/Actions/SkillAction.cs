using Ultimateframework.FXSystem;
using UltimateFramework.Utils;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace UltimateFramework.ActionsSystem
{
	[CreateAssetMenu(menuName = "Ultimate Framework/Systems/Actions/Action Asset/Skill")]
	public class SkillAction : BaseAction
	{
        [Tooltip("Time in milliseconds that must elapse before the ability can be used again.")]
        public float motionSpeed = 1.0f;
        public int coolDown;
        public FXData fxData;

        public bool doSomethingInSubAction;
        private CancellationTokenSource cooldownCancelationTS;

        // Function used for the logic of the action itself, will be called through the assigned input.
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
                    m_Actions.CurrentAction = null;
                    return;
                }

                this.IsExecuting = true;
                m_Actions.CurrentAction = this;
                actionsMaster.CurrentAction = this;
                m_Locomotion.CanJump = false;
                m_Statistics.CanRegenerateStats = false;
                m_InputManager.GetInputActionOnCurrentMap("Skill").Disable();

                var combatActionsComprobement = m_InventoryAndEquipment.GetCurrentMainWeapon().HandSocket.childCount <= 0 && m_Actions.FindSpecificActionsGroup("Moveset.Unarmed") != null;
                var currentWeapon = combatActionsComprobement ? null : m_InventoryAndEquipment.GetCurrentMainWeapon().WeaponObject.GetComponent<WeaponBehaviour>().Item;
                var currentSpecificActions = combatActionsComprobement ?
                    m_Actions.FindSpecificActionsGroup("Moveset.Unarmed") :
                    m_Actions.FindSpecificActionsGroup(currentWeapon.actionsTag);

                m_Locomotion.OverrideAnimatorController[currentStructure.overrideClip] = currentStructure.motion; // sobre escribe la animacion concreta

                int layerIndex;

                foreach (var actionStructure in currentSpecificActions?.actionsGroup.actions)
                {
                    if (actionStructure.actionTag.tag == currentStructure.actionTag.tag)
                    {
                        layerIndex = animator.GetLayerIndex(currentStructure.layerMask);
                        animator.applyRootMotion = true;

                        int[] excludeLayers = new int[]
                        {
                            layerIndex,
                            animator.GetLayerIndex(m_Locomotion.RightHandMaskName),
                            animator.GetLayerIndex(m_Locomotion.RightAndLeftHandMaskName)
                        };
                        PlayActionAnimation(animator, layerIndex, currentStructure, motionSpeed, excludeLayersForDesactive: excludeLayers);
                    }
                }

                if (doSomethingInSubAction)
                {
                    await SubActionEnter(1, this);
                    await SubActionExit(1, this);
                }

                await ActionFinishNotify(this);
                ResetValues();
                this.IsExecuting = false;

                cooldownCancelationTS = new();
                await TimerToReset(coolDown, cooldownCancelationTS.Token);
                if (!cooldownCancelationTS.IsCancellationRequested) m_InputManager.GetInputActionOnCurrentMap("Skill").Enable();
            }
            catch (System.Exception)
            {
                CancelationTS.Cancel();
                throw;
            }
        }

        // Optional function to add logic when active sub action
        protected override void ExecuteInSubActionEnter()
        {
            if (m_Actions.IsGameRunning)
            {
                m_DamageHandler.SetAllowCollisions(true);
            }
        }

        // Optional function to add logic when desactive sub action
        protected override void ExecuteInSubActionExit()
        {
            if (m_Actions.IsGameRunning)
            {
                m_DamageHandler.SetAllowCollisions(false);
            }
        }

        public override void PerformFX(MainHand hand)
        {
            m_FXManager.PerformAttackFX(fxData, hand);
        }

        public override void PerformSpecialFX()
        {
            m_FXManager.PerformAttackFX(fxData);
        }

        // Optional function that should be used to reset values of the action itself, should be called at your convenience within the Execute function.
        public override void ResetValues()
		{
            if (m_Actions.IsGameRunning)
            {
                if (!m_Locomotion.useRootMotionOnMovement) m_Animator.applyRootMotion = false;
                m_Statistics.CanRegenerateStats = true;
                m_Locomotion.CanJump = true;
                ResetLayerWeights();
            }
        }
		
		// Optional function to add logic when the action is interrupted by a higher-priority action
		public override void InterruptAction()
		{
            if (m_Actions.IsGameRunning)
            {
                ResetValues();
                m_InputManager.GetInputActionOnCurrentMap("Skill").Enable();
                CancelationTS.Cancel();
                cooldownCancelationTS.Cancel();
                this.IsExecuting = false;
            }
		}

        // Optional function to add logic when the action is cancelled by a higher priority action
        protected override void CancelAction()
		{
            if (m_Actions.IsGameRunning)
            {
                ResetValues();
                m_InputManager.GetInputActionOnCurrentMap("Skill").Enable();
                CancelationTS.Cancel();
                cooldownCancelationTS.Cancel();
                this.IsExecuting = false;
            }
		}
	}
}