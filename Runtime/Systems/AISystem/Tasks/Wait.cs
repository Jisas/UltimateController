using UltimateFramework.LocomotionSystem;
using UltimateFramework.AI.BehaviourTree;
using UltimateFramework.Utils;
using UnityEngine;

namespace UltimateFramework.AI.Task
{
    public class Wait : Node
    {
        float _waitedTime;
        float _waitCounter;

        public Wait(float waitedTime)
        {
            _waitedTime = waitedTime;
            _waitCounter = 0.0f;
        }

        public override NodeState Evaluate()
        {
            _waitCounter += Time.deltaTime;
            if (_waitCounter >= _waitedTime)
            {
                state = NodeState.Success;
                return state;
            }

            state = NodeState.Running;
            return state;
        }
    }
}
