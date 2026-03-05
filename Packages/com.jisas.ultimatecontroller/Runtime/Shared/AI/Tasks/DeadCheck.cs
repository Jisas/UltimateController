using UltimateController.AI.BehaviourTree;
using UltimateController.StatisticsSystem;
using UltimateController.LocomotionSystem;
using UltimateController.Utils;

namespace UltimateController.AI.Task
{
    public class DeadCheck : Node
    {
        readonly StatisticsComponent _statisticsComponent;
        readonly AILocomotionCommponent _locomotionComponent;

        public DeadCheck(StatisticsComponent statsComponent, AILocomotionCommponent locomotionComponent)
        {
            _statisticsComponent = statsComponent;  
            _locomotionComponent = locomotionComponent;
        }

        public override NodeState Evaluate()
        {
            if (_statisticsComponent != null && _statisticsComponent.FindStatistic("Stats.Health").CurrentValue <= 0)
            {
                _locomotionComponent.enabled = false;

                state = NodeState.Failure;
                return state;
            }

            state = NodeState.Running;
            return state;
        }
    }
}
