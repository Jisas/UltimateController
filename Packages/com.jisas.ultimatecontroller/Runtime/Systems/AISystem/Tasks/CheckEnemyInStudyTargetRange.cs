using UltimateFramework.AI.BehaviourTree;
using UltimateFramework.LocomotionSystem;
using UltimateFramework.Utils;
using UnityEngine;

namespace UltimateFramework.AI.Task
{
    public class CheckEnemyInStudyTargetRange : Node
    {
        readonly AILocomotionCommponent m_Locomotion;
        readonly Transform _transform;
        readonly float _studyTargetRange;

        public CheckEnemyInStudyTargetRange(Transform transform, AILocomotionCommponent locomotionComp, float studyRange)
        {
            m_Locomotion = locomotionComp;
            _studyTargetRange = studyRange;
            _transform = transform;
        }

        public override NodeState Evaluate()
        {
            Transform target = GetData("target") as Transform;
            float distance = Vector3.Distance(_transform.position, target.position);
            m_Locomotion.CanMove = true;

            if (distance > _studyTargetRange)
            {
                state = NodeState.Running;
                return state;
            }

            state = NodeState.Success;
            return state;
        }
    }
}
