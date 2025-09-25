using UltimateFramework.CollisionsAndDamageSystem;
using UltimateFramework.LocomotionSystem;
using UltimateFramework.StatisticsSystem;
using UltimateFramework.InventorySystem;
using Ultimateframework.FXSystem;
using UltimateFramework.Tools;
using UltimateFramework.Utils;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using System;
using UltimateFramework.AnimatorDataSystem;

namespace UltimateFramework.ActionsSystem
{
	[CreateAssetMenu(menuName = "Ultimate Framework/Systems/Actions/Action Asset/HeavyAttack")]
	public class HeavyAttackAction : BaseAction
	{
        #region PublicFields
        [Tooltip("Time in milliseconds that must elapse before the ability can be used again.")]
        public float motionSpeed = 1.0f;
        public FXData fxData;
        #endregion

        #region Components
        private InventoryAndEquipmentComponent m_InventoryAndEquipment;
        private AnimatorDataHandler m_AnimatorDataHandler;
        private BaseLocomotionComponent m_Locomotion;
        private StatisticsComponent m_Statistics;
        private DamageComponent m_DamageHandler;
        private FXManagerComponent m_FXManager;
        private ActionsComponent m_Actions;
        private GameObject projectile;
        #endregion

        public override void StartConfig(BaseAction action, ActionStructure currentStructure)
        {
            action = this;
			base.StartConfig(action, currentStructure); // --> Call to the base (not mandatory)

            m_Statistics = GetComponentByName<StatisticsComponent>("StatisticsComponent");
            m_InventoryAndEquipment = GetComponentByName<InventoryAndEquipmentComponent>("InventoryAndEquipmentComponent");
            m_Actions = GetComponentByName<ActionsComponent>("ActionsComponent");
            m_Locomotion = GetComponentByName<BaseLocomotionComponent>("BaseLocomotionComponent");
            m_DamageHandler = GetComponentByName<DamageComponent>("DamageComponent");
            m_FXManager = GetComponentByName<FXManagerComponent>("FXManagerComponent");
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

                if (m_InventoryAndEquipment.GetCurrentMainWeapon() != null)
                {
                    if (m_InventoryAndEquipment.GetCurrentMainWeapon()?.HandSocket.childCount <= 0 && m_Actions.FindSpecificActionsGroup("Moveset.Unarmed") == null)
                    {
                        Debug.LogWarning("Not have any weapon on hand and not have actions for unarmed combat");
                        this.IsExecuting = false;
                        m_Actions.CurrentAction = null;
                        return;
                    }
                }
                else
                {
                    if (m_Actions.FindSpecificActionsGroup("Moveset.Unarmed") == null)
                    {
                        Debug.LogWarning("Not have any weapon on hand and not have actions for unarmed combat");
                        this.IsExecuting = false;
                        m_Actions.CurrentAction = null;
                        return;
                    }
                }

                this.IsExecuting = true;
                m_Locomotion.CanJump = false;
                m_Actions.CurrentAction = this;
                actionsMaster.CurrentAction = this;
                m_Statistics.CanRegenerateStats = false;

                var combatActionsComprobement = m_InventoryAndEquipment.GetCurrentMainWeapon().HandSocket.childCount <= 0 && m_Actions.FindSpecificActionsGroup("Moveset.Unarmed") != null;
                var currentWeapon = combatActionsComprobement ? null : m_InventoryAndEquipment.GetCurrentMainWeapon().WeaponObject.GetComponent<WeaponBehaviour>().Item;
                var currentSpecificActions = combatActionsComprobement ?
                    m_Actions.FindSpecificActionsGroup("Moveset.Unarmed") :
                    m_Actions.FindSpecificActionsGroup(currentWeapon.actionsTag);

                m_AnimatorDataHandler.OverrideAnimatorController[currentStructure.overrideClip] = currentStructure.motion;

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

                await ActionFinishNotify(this);
  
                ResetValues();
                this.IsExecuting = false;
            }
			catch (Exception ex)
			{
                Debug.LogError($"Error executing action: {ex.Message}");
                this.IsExecuting = false;
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

        public override void SetProjectile()
        {
            var item = SettingsMasterData.Instance.itemDB.FindItem("Rock");
            var socket = m_InventoryAndEquipment.leftHandBone.Find(item.handSlot);
            projectile = Instantiate(item.prefab, socket);
        }
        public override void ShootProjectile()
        {
            if (projectile != null)
            {
                ParabolicProjectile parabolicProjectile = projectile.GetComponent<ParabolicProjectile>();
                parabolicProjectile.DamageHandler.AllowCollisions = true;

                AILocomotionCommponent AILocomotion = m_Locomotion as AILocomotionCommponent;
                parabolicProjectile.Launch(AILocomotion.CurrentTarget.position);
            }
        }
        public override void PerformFX(MainHand hand)
        {
            m_FXManager.PerformAttackFX(fxData, hand);
        }

        public override void ResetValues()
		{
            if (!m_Locomotion.useRootMotionOnMovement) m_Animator.applyRootMotion = false;           
            m_Statistics.CanRegenerateStats = true;
            m_Locomotion.CanJump = true;
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