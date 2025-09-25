using UltimateFramework.AI.BehaviourTree;
using UltimateFramework.LocomotionSystem;
using UltimateFramework.Utils;
using UnityEngine;

namespace UltimateFramework.AI.Task
{
    public class LookAtTarget : Node
    {
        readonly AILocomotionCommponent m_Locomotion;
        readonly Transform _transform;

        public LookAtTarget(Transform transform, AILocomotionCommponent locomotionComp)
        {
            _transform = transform;
            m_Locomotion = locomotionComp;
        }

        public override NodeState Evaluate()
        {
            object data = GetData("target");

            if (data != null)
            {
                Transform target = data as Transform;

                m_Locomotion.CanMove = false;
                m_Locomotion.IsTargetting = true;
                _transform.LookAt(target, Vector3.up);

                state = NodeState.Success;
                return state;
            }

            state = NodeState.Running;
            return state;
        }
    }
}
