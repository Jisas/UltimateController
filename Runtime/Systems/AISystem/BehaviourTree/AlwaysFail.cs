using UltimateFramework.AI.BehaviourTree;
using UltimateFramework.Utils;

public class AlwaysFail : Node
{
    public override NodeState Evaluate()
    {
        return NodeState.Failure;
    }
}
