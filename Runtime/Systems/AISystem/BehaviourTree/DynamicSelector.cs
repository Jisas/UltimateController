using System.Collections.Generic;
using UltimateFramework.Utils;

namespace UltimateFramework.AI.BehaviourTree
{
    public class DynamicSelector : Node
    {
        public DynamicSelector() : base() { }
        public DynamicSelector(List<Node> children) : base(children) { }

        public override NodeState Evaluate()
        {
            bool anyChildRunning = false;

            foreach (var node in children)
            {
                switch (node.Evaluate())
                {
                    case NodeState.Running:
                        anyChildRunning = true;
                        continue;

                    case NodeState.Success:
                        state = NodeState.Success;
                        continue;

                    case NodeState.Failure:
                        continue;

                    default:
                        continue;
                }
            }

            state = anyChildRunning ? NodeState.Running : NodeState.Failure;
            return state;
        }
    }
}
