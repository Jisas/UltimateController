using UltimateFramework.LocomotionSystem;
using UltimateFramework.StatisticsSystem;
using UltimateFramework.InventorySystem;
using UltimateFramework.Inputs;
using UltimateFramework.Utils;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UltimateFramework.AnimatorDataSystem;

namespace UltimateFramework.ActionsSystem
{
	[CreateAssetMenu(menuName = "Ultimate Framework/Systems/Actions/Action Asset/Buff")]
	public class BuffAction : BaseAction
	{
        #region SerializedFields
        public bool weaponRequired = true;
        public float motionSpeed = 1.0f;
        public string inputName;
        [Tooltip("Modifier duration time in milliseconds, only effective if the list of modifiers has at least one (1) modifier.")]
        public int modifiersTime;
        [Tooltip("Time in milliseconds needed to be able to use the skill again, must be longer than the time of the modifiers")]
        public int cooldown;
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
                    return;
                }

                this.IsExecuting = true;
                State = ActionState.Running;
                m_Actions.CurrentAction = this;
                actionsMaster.CurrentAction = this;
                m_Locomotion.CanJump = false;

                var combatActionsComprobement = m_InventoryAndEquipment.GetCurrentMainWeapon().HandSocket.childCount <= 0 && m_Actions.FindSpecificActionsGroup("Moveset.Unarmed") != null;
                var currentWeapon = combatActionsComprobement ? null : m_InventoryAndEquipment.GetCurrentMainWeapon().WeaponObject.GetComponent<WeaponBehaviour>().Item;
                var currentSpecificActions = combatActionsComprobement ?
                    m_Actions.FindSpecificActionsGroup("Moveset.Unarmed") :
                    m_Actions.FindSpecificActionsGroup(currentWeapon.actionsTag);

                m_AnimatorDataHandler.OverrideAnimatorController[currentStructure.overrideClip] = currentStructure.motion; // sobre escribe la animacion concreta

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

                m_InputManager.GetInputActionOnCurrentMap(inputName).Disable();
                await ActionFinishNotify(this);

                m_Locomotion.CanJump = true;
                this.IsExecuting = false;

                await TimerToReset(modifiersTime, CancelationTS.Token);
                HanddlerApplyModifiers(true);

                if (cooldown > modifiersTime)
                {
                    var time = cooldown - modifiersTime;
                    await TimerToReset(time, CancelationTS.Token);
                }

                m_InputManager.GetInputActionOnCurrentMap(inputName).Enable();
            }
            catch (System.Exception)
            {
                CancelationTS.Cancel();
                throw;
            }
        }
		
		// Optional function to add logic when the action is interrupted by a higher-priority action
		public override void InterruptAction()
		{
            m_InputManager.GetInputActionOnCurrentMap(inputName).Enable();
            this.IsExecuting = false;
        }

        // Optional function to add logic when the action is cancelled by a call while a higher priority action is being executed.
        protected override void CancelAction()
		{
            m_InputManager.GetInputActionOnCurrentMap(inputName).Enable();
            this.IsExecuting = false;
        }
	}
}