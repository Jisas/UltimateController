using UltimateFramework.AI.BehaviourTree;
using UltimateFramework.LocomotionSystem;
using UltimateFramework.InventorySystem;
using System.Collections.Generic;
using UltimateFramework.AI.Task;
using UltimateFramework.Inputs;
using UltimateFramework.Utils;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(AILocomotionCommponent))]
public class EnemyTree : BehaviourTree
{
    [Header("Patroll")]
    [SerializeField] PatrolType patrolType = PatrolType.RoundTrip;
    [SerializeField] Transform[] wayPoints;

    [Header("Detection")]
    [SerializeField] LayerMask enemiesLayer;
    [SerializeField] float visionRadius = 15.0f;
    [SerializeField] float studyTargetRange = 6.0f;
    [SerializeField] float studyTargetTime = 10.0f;
    [SerializeField] float stopDistance = 1.0f;
    [SerializeField] float attackRange = 3.0f;

    EntityActionInputs m_InputManager;
    AILocomotionCommponent m_Locomotion;
    AIInventoryAndEquipment m_Inventory;

    private void Awake()
    {
        m_InputManager = GetComponent<EntityActionInputs>();
        m_Locomotion = GetComponent<AILocomotionCommponent>();
        m_Inventory = GetComponent<AIInventoryAndEquipment>();
    }

    protected override Node SetupTree()
    {
        Node root = new Selector(new List<Node>
        {
            new Sequence(new List<Node>
            {
                new CheckEnemyInFOVRange(transform, visionRadius, enemiesLayer, m_Locomotion),
                new LookAtTarget(transform, m_Locomotion),
                new EquipMeleeWeapon(m_Inventory, m_InputManager),
                new ChaseTarget(transform, m_Locomotion, visionRadius, stopDistance),
                new Selector(new List<Node>
                {
                    new Sequence(new List<Node>
                    {
                        new CheckEnemyInStudyTargetRange(transform, m_Locomotion, studyTargetRange),
                        new StudyEnemy(m_Locomotion, studyTargetTime)
                    }),
                    new Sequence(new List<Node>
                    {
                        new CheckEnemyInAttackRange(transform, attackRange),
                        new Attack(transform, m_Inventory, m_InputManager, attackRange)
                    })
                })
            }),
            new Patrol(transform, m_Locomotion, wayPoints, patrolType)
        });

        return root;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = Color.green;
        Handles.DrawWireDisc(transform.position, Vector3.up, visionRadius, 3);

        Handles.color = Color.yellow;
        Handles.DrawWireDisc(transform.position, Vector3.up, studyTargetRange, 3);

        Handles.color = Color.blue;
        Handles.DrawWireDisc(transform.position, Vector3.up, attackRange, 3);

        Handles.color = Color.red;
        Handles.DrawWireDisc(transform.position, Vector3.up, stopDistance, 3);
    }
#endif
}
