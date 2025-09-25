using UltimateFramework.AI.BehaviourTree;
using UltimateFramework.LocomotionSystem;
using UltimateFramework.Utils;
using UnityEngine;

namespace UltimateFramework.AI.Task
{
    public class ChaseTarget : Node
    {
        readonly AILocomotionCommponent _locomotion;
        readonly Transform _transform;
        readonly float _visionRange;
        readonly float _stopDistance;

        public ChaseTarget(Transform transform, AILocomotionCommponent locomotionComp, float visionRange, float stopDistance)
        {
            _transform = transform;
            _locomotion = locomotionComp;
            _visionRange = visionRange;
            _stopDistance = stopDistance;
        }

        public override NodeState Evaluate()
        {
            Transform target = GetData("target") as Transform;

            if (target == null)
            {
                state = NodeState.Failure;
                return state;
            }

            float distance = Vector3.Distance(_transform.position, target.position);            

            if (distance < _stopDistance)
            {
                _locomotion.CanMove = false;

                state = NodeState.Success;
                return state;
            }
            else if (distance < _visionRange && distance > _stopDistance)
            {
                _locomotion.CanMove = true;
                _locomotion.IsTargetting = true;
                _locomotion.SetTarget(target);
                _locomotion.SetDirection(target.position - _transform.position);

                state = NodeState.Running;
                return state;
            }
            else if (distance > _visionRange)
            {
                _locomotion.IsTargetting = false;
                _locomotion.CanMove = false;

                state = NodeState.Failure;
                return state;
            }

            state = NodeState.Running;
            return state;
        }
    }
}