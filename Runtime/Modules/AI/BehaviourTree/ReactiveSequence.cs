

using System.Collections.Generic;
using UltimateFramework.Utils;

namespace UltimateFramework.AI.BehaviourTree
{
    public class ReactiveSequence : Node
    {
        public ReactiveSequence() : base() { }
        public ReactiveSequence(List<Node> children) : base(children) { }

        public override NodeState Evaluate()
        {
            foreach (var node in children)
            {
                switch (node.Evaluate())
                {
                    case NodeState.Running:
                        state = NodeState.Running;
                        return state;

                    case NodeState.Success:
                        continue;

                    case NodeState.Failure:
                        state = NodeState.Failure;
                        return state;

                    default:
                        continue;
                }
            }

            // If all children succeed, reset their states to force re-evaluation in the next tick
            foreach (var node in children)
            {
                node.state = NodeState.Running;
            }

            state = NodeState.Success;
            return state;
        }
    }

}
