using UltimateFramework.AI.BehaviourTree;
using UltimateFramework.LocomotionSystem;
using UltimateFramework.Utils;
using UnityEngine;

namespace UltimateFramework.AI.Task
{
    public class StudyEnemy : Node
    {
        readonly AILocomotionCommponent m_Locomotion;
        readonly float _studyTime;
        float _timeCounter = 0f;
        bool _studing = true;
        bool _setProbability = true;

        public StudyEnemy(AILocomotionCommponent locomotionComp, float studyTime)
        {
            m_Locomotion = locomotionComp;
            _studyTime = studyTime;
        }

        public override NodeState Evaluate()
        {
            Transform target = GetData("target") as Transform;

            if (target == null)
            {
                state = NodeState.Failure;
                return state;
            }

            if (_studing)
            {
                _timeCounter += Time.deltaTime;
                if (_timeCounter >= _studyTime) _studing = false;

                m_Locomotion.CanMove = true;
                m_Locomotion.IsTargetting = true;
                m_Locomotion.SetLocomotionMode(LocomotionMode.Walk);
                m_Locomotion.SetLocomotionType(LocomotionType.Strafe);
                m_Locomotion.SetTarget(target);

                if (_setProbability)
                {
                    int randomNumber = Random.Range(0, 2);
                    Vector3 direction = randomNumber == 0 ? Vector3.left : Vector3.right;
                     m_Locomotion.SetDirection(direction);
                    _setProbability = false;
                }
            }

            state = _studing ? NodeState.Running : NodeState.Failure;
            return state;
        }
    }
}
