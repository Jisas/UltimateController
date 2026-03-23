using System.Collections.Generic;
using UltimateFramework.Utils;
using System.Threading.Tasks;
using System.Threading;
using UnityEditor;
using UnityEngine;
using System;

namespace UltimateFramework.ActionsSystem
{
    [CreateAssetMenu(menuName = "Ultimate Framework/Systems/Actions/Actions Group")]
    public class ActionsGroup : ScriptableObject
    {
        public List<ActionStructure> actions = new();
        private GameObject Owner;

        public int Index { get; set; }
        public ActionStructure CurrentActionStructure { get; private set; }
        public EntityActionsManager ActionsManager { get; private set; }

        public void SetUpActionsGroup(EntityActionsManager actionsManager, GameObject owner)
        {
            this.Owner = owner;

            if (actionsManager != null)
            {
                actionsManager.LastAction = null;
                actionsManager.CurrentAction = null;
                actionsManager.CurrentActionStructure = null;

                foreach (var actionStructure in actions)
                {
                    if (actionStructure.enableActionsForEachWeapon)
                    {
                        foreach (var actionListStruct in actionStructure.actions)
                        {
                            if (actionListStruct.action != null)
                                actionsManager.AddAction(actionListStruct.action);
                        }
                    }
                    else
                    {
                        if (actionStructure.globalAction != null)
                            actionsManager.AddAction(actionStructure.globalAction);
                    }                  
                }

                ActionsManager = actionsManager;           
            }
        }

        public ActionsGroup Clone()
        {
            ActionsGroup clone = Instantiate(this);

            foreach (var actionStruct in clone.actions)
            {
                if (actionStruct.enableActionsForEachWeapon)
                {
                    foreach (var actionListStruct in actionStruct.actions)
                    {
                        actionListStruct.action = actionListStruct.action.Clone();
                        actionListStruct.action.SetupAction(Owner);
                        actionListStruct.action.StartConfig(actionListStruct.action, actionStruct);
                    }
                }
                else
                {
                    actionStruct.globalAction = actionStruct.globalAction.Clone();
                    actionStruct.globalAction.SetupAction(Owner);
                    actionStruct.globalAction.StartConfig(actionStruct.globalAction, actionStruct);
                }
            }

            return clone;
        }

        public ActionStructure FindActionStructure(string actionTag)
        {
            foreach (var actionStruct in actions)
            {
                if (actionStruct.actionTag.tag == actionTag)
                {
                    return actionStruct;
                }
            }
            throw new NullReferenceException("Action not found");
        }

        public Task TriggerAction(string actionTag, Animator animator, ActionsPriority priority, string weaponName, CancellationToken ct)
        {
            ActionStructure newActionStructure = FindActionStructure(actionTag);
            BaseAction newAction;

            CurrentActionStructure = newActionStructure;
            ActionsManager.CurrentActionStructure = this.CurrentActionStructure;

            if (newActionStructure.enableActionsForEachWeapon) 
            {
                newAction = !String.IsNullOrEmpty(weaponName) ? newActionStructure.FindActionInList(weaponName).action : null;

                if (newAction != null)
                {
                    newAction.Priority = priority;
                    ActionsManager.TriggerIterruptEventAction(newAction);
                }
            }
            else
            {
                newAction = newActionStructure.globalAction;
                newAction.Priority = priority;
                ActionsManager.TriggerIterruptEventAction(newAction);
            }

            ActionsManager.LastAction = newAction;
            ActionsManager.EnqueueAction(newAction, priority);

            return ActionsManager.ProcessActions(animator, ct);
        }

        public void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
}
