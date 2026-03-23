using UltimateFramework.AI.BehaviourTree;
using UltimateFramework.InventorySystem;
using UltimateFramework.ActionsSystem;
using UltimateFramework.Inputs;
using UltimateFramework.Utils;
using UnityEngine;

namespace UltimateFramework.AI.Task
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
            object data = GetData("target");

            if (data == null)
            {
                state = NodeState.Failure;
                return state;
            }

            if (!ready)
            {
                GameObject currentWeapon = m_Inventory.GetCurrentRightWeaponObject();
                WeaponComponent weaponComponent = currentWeapon.GetComponent<WeaponComponent>();
                string currentWeaponName = weaponComponent.Item.name;

                InputActionLogic equipAction = m_Inputs.FindInputAction("EquipMelee");
                ActionsPriority actionPriority = equipAction.PrimaryAction.priority;
                string actionTag = equipAction.PrimaryAction.actionTag.tag;
                bool isBaseAction = equipAction.PrimaryAction.isBaseAction;

                equipAction.ExecuteAction(actionTag, actionPriority, currentWeaponName, isBaseAction);
                ready = true;
            }

            state = NodeState.Success;
            return state;
        }
    }
}
