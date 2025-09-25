using UltimateFramework.LocomotionSystem;
using UltimateFramework.InventorySystem;
using UltimateFramework.ActionsSystem;
using System.Collections.Generic;
using UltimateFramework.Inputs;
using UltimateFramework.Utils;
using UnityEngine;

namespace UltimateFramework.AnimatorDataSystem
{
    public class IgnoreBehaviour : StateMachineBehaviour
    {
        public bool ResetLayerWeightOnExit = true;
        public List<IgnoreOptions> thingsToIgnore = new();

        private List<int> lastActiveLayers = new();
        private EntityActionInputs m_InputManager;
        private ActionsComponent m_ActionsComponent;
        private BaseLocomotionComponent m_Locomotion;
        private InventoryAndEquipmentComponent m_InventoryAndEquipment;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            lastActiveLayers.Clear();
            m_InputManager = animator.gameObject.GetComponent<EntityActionInputs>();
            m_ActionsComponent = animator.gameObject.GetComponent<ActionsComponent>();
            m_Locomotion = animator.gameObject.GetComponent<BaseLocomotionComponent>();
            m_InventoryAndEquipment = animator.gameObject.GetComponent<InventoryAndEquipmentComponent>();
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            foreach (var item in thingsToIgnore)
            {
                switch (item)
                {
                    case IgnoreOptions.FeetIK:
                        m_Locomotion.enableFeetIK = false;
                        break;

                    case IgnoreOptions.HeadTraking:
                        m_Locomotion.enableHeadTracking = false;
                        break;

                    case IgnoreOptions.Movement:
                        m_Locomotion.CanMove = false;
                        break;

                    case IgnoreOptions.AllOtherActiveLayers:
                        if (AnimatorUtilities.HasMultipleActiveLayers(animator))
                        {
                            int[] excludeLayers = new int[] { layerIndex };
                            var activeLayers = AnimatorUtilities.GetActiveLayersIndices(animator, excludeLayers);
                            lastActiveLayers = activeLayers;
                            SetLayersWeight(animator, activeLayers.ToArray(), 0);
                        }

                        if (m_ActionsComponent.IsDodging && (stateInfo.IsName("Attack_A") || stateInfo.IsName("Attack_B") || stateInfo.IsName("Skill")) ||
                            m_ActionsComponent.CurrentAction.State == ActionState.Finished)
                        {
                            SetLayersWeight(animator, lastActiveLayers.ToArray(), 1);
                        }
                        break;

                    case IgnoreOptions.Crouch:
                        m_InputManager.FindInputAction("Crouch").State = false;
                        int[] layers = new int[]
                        {
                            animator.GetLayerIndex(m_Locomotion.RightHandMaskName),
                            animator.GetLayerIndex(m_Locomotion.RightAndLeftHandMaskName)
                        };
                        SetLayersWeight(animator, layers, 0);
                        break;
                }
            }
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            foreach (var item in thingsToIgnore)
            {
                switch (item)
                {
                    case IgnoreOptions.FeetIK:
                        m_Locomotion.enableFeetIK = true;
                        break;

                    case IgnoreOptions.HeadTraking:
                        m_Locomotion.enableHeadTracking = true;
                        break;

                    case IgnoreOptions.AllOtherActiveLayers:
                        if (ResetLayerWeightOnExit)
                            SetLayersWeight(animator, lastActiveLayers.ToArray(), 1);
                        break;

                    case IgnoreOptions.Movement:
                        m_Locomotion.CanMove = true;
                        break;

                    case IgnoreOptions.Crouch:
                        int[] layers = new int[]
                        {
                            animator.GetLayerIndex(m_Locomotion.RightHandMaskName),
                            animator.GetLayerIndex(m_Locomotion.RightAndLeftHandMaskName)
                        };
                        SetLayersWeight(animator, layers, 0);

                        var currentWeaponName = m_InventoryAndEquipment.GetCurrentMainWeapon().WeaponObject.GetComponent<WeaponBehaviour>().itemName; // busca el nombre del arma actual
                        var currentlocomotionMap = m_Locomotion.LocomotionMaster.FindMap(currentWeaponName); // busca el mapa de locomocion actual
                        var movementStruct = currentlocomotionMap.movement; // extare la estructura de movimiento del mapa de locomocion actual                          

                        // Si el mapa de locomocion actial necesita usar dos capas desde el inicio y su capa de sobre escritura es LoweBodyMask, entonces la activa
                        var overrideLayer = m_Locomotion.LocomotionMaster.FindOverrideLayer(movementStruct, m_Locomotion.OverrideLayer);
                        var overrideLayerMaskName = overrideLayer != null ? overrideLayer.globalPose.mask : "";
                        if (currentlocomotionMap.useOverrideAtStartup && (overrideLayerMaskName == m_Locomotion.LowerBodyMaskName || overrideLayerMaskName == m_Locomotion.UpperBodyMaskName))
                        {
                            var overrideLayerIndex = animator.GetLayerIndex(overrideLayerMaskName);
                            animator.SetLayerWeight(overrideLayerIndex, 1);
                        }
                        break;
                }
            }
        }

        private void SetLayersWeight(Animator animator, int[] layers, int weight)
        {
            foreach (var index in layers)
            {
                animator.SetLayerWeight(index, weight);
            }
        }
    }
}
