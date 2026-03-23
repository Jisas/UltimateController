using UltimateFramework.AI.BehaviourTree;
using UltimateFramework.LocomotionSystem;
using UltimateFramework.Utils;
using UnityEngine;

namespace UltimateFramework.AI.Task
{
    public class ChaseTarget : Node
    {
        readonly AILocomotionCommponent m_Locomotion;
        readonly Transform _transform;
        readonly float _studyTargetRange;
        readonly float _visionRange;
        readonly float _stopDistance;

        public ChaseTarget(Transform transform, AILocomotionCommponent locomotionComp, float studyRange, float visionRange, float stopDistance)
        {
            m_Locomotion = locomotionComp;
            _studyTargetRange = studyRange;
            _visionRange = visionRange;
            _transform = transform;
            _stopDistance = stopDistance;
        }

        public override NodeState Evaluate()
        {
            Transform target = GetData("target") as Transform;
            float distance = Vector3.Distance(_transform.position, target.position);

            m_Locomotion.CanMove = true;
            m_Locomotion.SetLocomotionMode(LocomotionMode.Walk);
            m_Locomotion.SetLocomotionType(LocomotionType.ForwardFacing, false);

            if (distance < _visionRange && distance > _studyTargetRange)
            {
                m_Locomotion.IsTargetting = true;
                m_Locomotion.enableHeadTracking = true;
                m_Locomotion.SetTarget(target);
                m_Locomotion.SetDirection(target.position - _transform.position);

                state = NodeState.Running;
                return state;
            }
            else if (distance < _visionRange && distance < _studyTargetRange && distance < _stopDistance)
            {
                m_Locomotion.IsTargetting = true;
                m_Locomotion.SetLocomotionType(LocomotionType.Strafe, false);
                m_Locomotion.SetDirection(Vector3.back);

                state = NodeState.Running;
                return state;
            }
            else if (distance > _visionRange)
            {
                m_Locomotion.IsTargetting = false;
                m_Locomotion.enableHeadTracking = false;

                state = NodeState.Failure;
                return state;
            }

            state = NodeState.Success;
            return state;
        }
    }
}