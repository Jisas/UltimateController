using UltimateFramework.AI.BehaviourTree;
using UltimateFramework.LocomotionSystem;
using UltimateFramework.StatisticsSystem;
using UltimateFramework.Utils;
using UnityEngine;

namespace UltimateFramework.AI.Task
{
    public class CheckEnemyInFOVRange : Node
    {
        readonly AILocomotionCommponent m_Locomotion;
        readonly Transform _transform;
        readonly LayerMask _mask;
        readonly float _radius;

        public CheckEnemyInFOVRange(Transform transform, float radius, LayerMask mask, AILocomotionCommponent locomotionComp)
        {
            _transform = transform;
            _radius = radius;
            _mask = mask;
            m_Locomotion = locomotionComp;
        }

        public override NodeState Evaluate()
        {
            object data = GetData("target");

            if (data == null)
            {
                var colliders = Physics.OverlapSphere(_transform.position, _radius, _mask, QueryTriggerInteraction.Ignore);

                if (colliders.Length > 0)
                {
                    var newTarget = colliders[0].transform.root;
                    parent.SetData("target", newTarget);
                    m_Locomotion.SetTarget(newTarget);
                }

                state = NodeState.Success;
                return state;
            }
            else
            {
                Transform target = data as Transform;
                var targetStats = target.GetComponent<StatisticsComponent>();

                if (targetStats != null && targetStats.FindStatistic("Stats.Health").CurrentValue <= 0)
                {
                    m_Locomotion.CanMove = false;
                    state = NodeState.Failure;
                    return state;
                }
            }

            state = NodeState.Running;
            return state;
        }
    }
}
