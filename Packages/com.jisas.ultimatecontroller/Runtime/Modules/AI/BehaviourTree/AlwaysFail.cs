using UltimateController.AI.BehaviourTree;
using UltimateController.Utils;

public class AlwaysFail : Node
{
    public override NodeState Evaluate()
    {
        return NodeState.Failure;
    }
}
