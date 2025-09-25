using System.Collections.Generic;
using UltimateFramework.Utils;
using UnityEngine;

namespace UltimateFramework.AI.BehaviourTree
{
    public class Selector : Node
    {
        public Selector() : base() { }
        public Selector(List<Node> children) : base(children) { }

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
                        state = NodeState.Success;
                        return state;

                    case NodeState.Failure:
                        continue;

                    default:
                        continue;
                }
            }

            state = NodeState.Failure;
            return state;
        }
    }
}
