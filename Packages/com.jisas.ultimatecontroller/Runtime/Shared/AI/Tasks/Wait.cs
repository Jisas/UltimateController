using UltimateController.LocomotionSystem;
using UltimateController.AI.BehaviourTree;
using UltimateController.Utils;
using UnityEngine;

namespace UltimateController.AI.Task
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
