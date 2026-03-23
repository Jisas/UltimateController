using UltimateFramework.AI.BehaviourTree;
using UltimateFramework.LocomotionSystem;
using UltimateFramework.Utils;
using UnityEngine;

namespace UltimateFramework.AI.Task
{
    public class CheckEnemyInAttackRange : Node
    {
        Transform _transform;
        float _range;

        public CheckEnemyInAttackRange(Transform transform, float range)
        {
            _transform = transform;
            _range = range;
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

            if (distance <= _range)
            {
                state = NodeState.Success;
                return state;
            }

            state = NodeState.Failure;
            return state;
        }
    }
}