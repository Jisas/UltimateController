using UltimateFramework.AI.BehaviourTree;
using UltimateFramework.Utils;
using UnityEngine;

namespace UltimateFramework.AI.Task
{
    public class CheckEnemyInAttackRange : Node
    {
        readonly Transform _transform;
        readonly float _attackRange;

        public CheckEnemyInAttackRange(Transform transform, float attackRange)
        {
            _transform = transform;
            _attackRange = attackRange;
        }

        public override NodeState Evaluate()
        {
            object t = GetData("target");

            if (t == null)
            {
                state = NodeState.Failure;
                return state;
            }

            Transform target = t as Transform;
            var distance = Vector3.Distance(_transform.position, target.position);

            if (distance < _attackRange)
            {
                state = NodeState.Success;
                return state;
            }

            state = NodeState.Failure;
            return state;
        }
    }
}