using UltimateController.AI.BehaviourTree;
using UltimateController.LocomotionSystem;
using UltimateController.Utils;
using UnityEngine;

namespace UltimateController.AI.Task
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
