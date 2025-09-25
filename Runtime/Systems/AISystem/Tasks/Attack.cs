using UltimateFramework.AI.BehaviourTree;
using UltimateFramework.InventorySystem;
using UltimateFramework.Inputs;
using UltimateFramework.Utils;
using UnityEngine;

namespace UltimateFramework.AI.Task
{
    public class Attack : Node
    {
        readonly InventoryAndEquipmentComponent m_InventoryAndEquipment;
        readonly EntityActionInputs m_Inputs;
        readonly Transform _transform;
        readonly float _attackRange;

        public Attack(Transform transform, InventoryAndEquipmentComponent inventoryComp, EntityActionInputs inputs, float attackRange)
        {
            _transform = transform;
            m_InventoryAndEquipment = inventoryComp;
            m_Inputs = inputs;
            _attackRange = attackRange;
        }

        public override NodeState Evaluate()
        {
            Transform target = GetData("target") as Transform;
            float distance = Vector3.Distance(_transform.position, target.position);

            if (target != null && distance < _attackRange)
            {
                InputActionLogic attackInputAction = m_Inputs.FindInputAction("Attack");
                ActionsPriority actionPriority = attackInputAction.PrimaryAction.priority;
                string actionTag = attackInputAction.PrimaryAction.actionTag.tag;
                bool isBaseAction = attackInputAction.PrimaryAction.isBaseAction;

                attackInputAction.ExecuteAction(actionTag, actionPriority, isBaseAction);

                state = NodeState.Running;
                return state;
            }
            else if (distance > _attackRange)
            {
                state = NodeState.Failure;
                return state;
            }

            state = NodeState.Running;
            return state;
        }
    }
}