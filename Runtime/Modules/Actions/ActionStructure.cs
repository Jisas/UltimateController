using System.Collections.Generic;
using UnityEngine;
using System;

namespace UltimateFramework.ActionsSystem
{
    [Serializable]
    public class ActionStructure
    {
        public string actionName;
        public TagSelector actionTag = new("None");
        public ActionCost actionCost = new();
        public List<ActionStatisticsModifier> modifiers = new();
        [Tooltip("Si es verdadero deberas agregar acciones a la lista de acciones para cada arma, en caso contrario solo deberas agregar una accion al campo \'GlobalAction\'.")]
        public bool enableActionsForEachWeapon;
        public BaseAction globalAction;
        public List<ActionsListStructure> actions;
        public AnimationClip motion;
        public AnimationClip overrideClip;
        public string animStateName;
        public string layerMask;
        public bool forceFullBodyOnly;
        public bool useMotionWarp;
        public WarpMotion motionWarp;

        public ActionStructure(string actionName)
        {
            this.actionName = actionName;
        }

        [Serializable]
        public class ActionCost
        {
            public TagSelector statType = new("None");
            public float value;
        }

        [Serializable]
        public struct WarpMotion
        {
            public float minRange;
            public float maxRange;
            public float speed;
            public float stopDistance;
        }

        public ActionsListStructure FindActionInList(string itemName)
        {
            foreach (var action in actions)
            {
                if (action.itemName == itemName)
                {
                    return action;
                }
            }
            return null;
        }

        public List<ActionsListStructure> GetActionList() => actions;
    }

    [Serializable]
    public class ActionsListStructure
    {
        public string itemName;
        public BaseAction action;
        public int Index { get; set; }
    }
}
