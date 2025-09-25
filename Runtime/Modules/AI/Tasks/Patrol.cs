using UltimateFramework.LocomotionSystem;
using UltimateFramework.AI.BehaviourTree;
using UltimateFramework.Utils;
using UnityEngine;

namespace UltimateFramework.AI.Task
{
    public class Patrol : Node
    {
        readonly AILocomotionCommponent m_Locomotion;
        readonly Transform[] _wayPoints;
        Transform _transform;

        PatrolType _patrolType;
        int _currentWayPointIndex = 0;
        float _waitedTime = 1f;
        float _waitCounter = 0f;
        float _threshold = 0.7f;
        bool _waiting = false;
        bool _goingBackwards = false;

        public Patrol(Transform transform, AILocomotionCommponent locomotionComp, Transform[] wayPoints, PatrolType patrolType)
        {
            m_Locomotion = locomotionComp;
            _wayPoints = wayPoints;
            _transform = transform;
            _patrolType = patrolType;
        }

        public override NodeState Evaluate()
        {
            if (_waiting)
            {
                _waitCounter += Time.deltaTime;

                if (_waitCounter >= _waitedTime) 
                    _waiting = false;
            }
            else
            {
                Transform wp = _wayPoints[_currentWayPointIndex];
                var distance = Vector3.Distance(_transform.position, wp.position);

                if (distance <= _threshold)
                {
                    _waitCounter = 0;
                    _waiting = true;
                    m_Locomotion.CanMove = false;

                    switch (_patrolType)
                    {
                        case PatrolType.ClosedCircuit:
                            _currentWayPointIndex = (_currentWayPointIndex + 1) % _wayPoints.Length;
                            break;

                        case PatrolType.RoundTrip:
                            if (_currentWayPointIndex == 0)
                                _goingBackwards = false;

                            else if (_currentWayPointIndex == (_wayPoints.Length - 1))
                                _goingBackwards = true;

                            _currentWayPointIndex = _goingBackwards ?
                                (_currentWayPointIndex - 1) :
                                (_currentWayPointIndex + 1);
                            break;
                    }
                }
                else
                {
                    m_Locomotion.CanMove = true;
                    m_Locomotion.SetTarget(wp);
                    m_Locomotion.SetDirection(wp.position - _transform.position);
                }

            }

            state = NodeState.Running;
            return state;
        }
    }
}
