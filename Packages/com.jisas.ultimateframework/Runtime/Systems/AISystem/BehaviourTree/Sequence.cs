using System.Collections.Generic;
using UltimateFramework.Utils;

namespace UltimateFramework.AI.BehaviourTree
{
    public class Sequence : Node
    {
        public Sequence() : base() { }
        public Sequence(List<Node> children) : base(children) { }

        public override NodeState Evaluate()
        {
            bool anyChildIsRunning = false;

            foreach (var node in children)
            {
                switch (node.Evaluate())
                {
                    case NodeState.Running:
                        anyChildIsRunning = true;
                        continue;

                    case NodeState.Success:
                        continue;

                    case NodeState.Failure:
                        state = NodeState.Failure;
                        return state;

                    default:
                        state = NodeState.Success;
                        return state;
                }
            }

            state = anyChildIsRunning ? NodeState.Running : NodeState.Success;
            return state;
        }
    }
}
