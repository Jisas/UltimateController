using UltimateController.AI.BehaviourTree;
using UltimateController.InventorySystem;
using UltimateController.ActionsSystem;
using UltimateController.Inputs;
using UltimateController.Utils;
using UnityEngine;

namespace UltimateController.AI.Task
{
    public class EquipMeleeWeapon : Node
    {
        readonly InventoryAndEquipmentComponent m_Inventory;
        readonly ActionsComponent m_Actions;
        readonly EntityActionInputs m_Inputs;
        bool ready = false;

        public EquipMeleeWeapon(InventoryAndEquipmentComponent inventoryComp, EntityActionInputs inputs)
        {
            m_Inventory = inventoryComp;
            m_Inputs = inputs;
        }

        public override NodeState Evaluate()
        {

            if (!ready)
            {
                InputActionLogic equipAction = m_Inputs.FindInputAction("EquipMelee");
                ActionsPriority actionPriority = equipAction.PrimaryAction.priority;
                string actionTag = equipAction.PrimaryAction.actionTag.tag;
                bool isBaseAction = equipAction.PrimaryAction.isBaseAction;

                equipAction.ExecuteAction(actionTag, actionPriority, isBaseAction);
                ready = true;
            }

            state = NodeState.Success;
            return state;
        }
    }
}
