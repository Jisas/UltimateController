using UltimateFramework.ItemSystem;
using System.Collections.Generic;
using UltimateFramework.Inputs;
using UltimateFramework.Tools;
using UltimateFramework.Utils;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace UltimateFramework.ActionsSystem
{
    [RequireComponent(typeof(Animator))]
    public class ActionsComponent : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private ActionsGroup baseActions;
        [Space(5)]
        [SerializeField] private List<ActionsGroupStructure> specificActions;
        #endregion

        #region Prperties
        public ActionsGroup BaseAG { get; private set; }
        public List<ActionsGroup> SpecificsAGList { get; private set; } = new();
        public BaseAction CurrentAction { get => _currentAction; set => _currentAction = value; }
        public bool IsGameRunning { get => _isGameRunning; set => _isGameRunning = value; }
        public Transform CurrentTarget { get; set; }
        public bool IsDodging { get => _isDodging; set => _isDodging = value; }
        public ActionsGroup BaseActions { get => baseActions; set => baseActions = value; }
        public List<ActionsGroupStructure> SpecificActions { get => specificActions; set => specificActions = value; }
        #endregion

        #region Private Values
        // references
        private Animator m_Animator;
        private EntityActionInputs m_InputsManager;
        private ItemDatabase itemDB;

        // other values
        private BaseAction _currentAction;
        private bool _isGameRunning;
        private bool _isDodging = false;
        private EntityActionsManager myActionsManager;
        private readonly CancellationTokenSource cancelationTokenSource = new();
        #endregion

        #region Unity Flow
        private void Awake()
        {
            myActionsManager = new();
            m_Animator = GetComponent<Animator>();
            itemDB = SettingsMasterData.Instance.itemDB;

            m_InputsManager = GetComponent<EntityActionInputs>();
            m_InputsManager.ExecuteBaseAction = TriggerActionOnBaseAG;
            m_InputsManager.ExecuteSpecificAction = TriggerActionOnSpecificsAG;
        }
        private void Start()
        {
            SetupActions();
            SetupStartActionsConfig();
        }
        private void Update()
        {
            _isGameRunning = Application.isPlaying;

            if (m_Animator != null && _currentAction != null)
                m_Animator.SetBool(m_InputsManager.AnimIDActionFinished, _currentAction.State == ActionState.Finished);
        }
        private void OnEnable() => myActionsManager.OnEnable();
        private void OnDisable() => myActionsManager.OnDisable();
        private void OnApplicationQuit() => cancelationTokenSource.Cancel();
        #endregion

        #region Common Logic
        private void SetupActions()
        {
            baseActions.SetUpActionsGroup(myActionsManager, this.gameObject);
            BaseAG = baseActions.Clone();
            BaseAG.SetUpActionsGroup(myActionsManager, this.gameObject);

            foreach (var actionStructure in BaseAG.actions)
            {
                if (actionStructure.enableActionsForEachWeapon)
                {
                    foreach (var actionList in actionStructure.actions)
                    {                      
                        actionList.action.SetupAction(this.gameObject);
                    }
                }
                else actionStructure.globalAction.SetupAction(this.gameObject);              
            }

            SpecificsAGList.Clear();
            foreach (var actionGroupStructure in specificActions)
            {
                actionGroupStructure.actionsGroup.SetUpActionsGroup(myActionsManager, this.gameObject);
                var newSpecificAG = actionGroupStructure.actionsGroup.Clone();
                SpecificsAGList.Add(newSpecificAG);
                newSpecificAG.SetUpActionsGroup(myActionsManager, this.gameObject);

                foreach (var actionStructure in newSpecificAG.actions)
                {
                    if (actionStructure.enableActionsForEachWeapon)
                    {
                        foreach (var actionList in actionStructure.actions)
                        {
                            actionList.action.SetupAction(this.gameObject);
                        }
                    }
                    else actionStructure.globalAction.SetupAction(this.gameObject);
                }
            }
        }
        private void SetupStartActionsConfig()
        {
            foreach (var actionStructure in baseActions.actions)
            {
                if (actionStructure.enableActionsForEachWeapon)
                {
                    for (int j = 0; j < actionStructure.actions.Count; j++)
                    {
                        var action = actionStructure.actions[j].action;
                        action.StartConfig(action, actionStructure);
                    }
                }
                else
                {
                    var action = actionStructure.globalAction;
                    actionStructure.globalAction.StartConfig(action, actionStructure);
                }
            }

            foreach (var actionGroupST in specificActions)
            {
                for (int i = 0; i < actionGroupST.actionsGroup.actions.Count; i++)
                {
                    if (actionGroupST.actionsGroup.actions[i].enableActionsForEachWeapon)
                    {
                        for (int j = 0; j < actionGroupST.actionsGroup.actions[i].actions.Count; j++)
                        {
                            var action = actionGroupST.actionsGroup.actions[i].actions[j].action;
                            action.StartConfig(action, actionGroupST.actionsGroup.actions[i]);
                        }
                    }
                    else
                    {
                        var action = actionGroupST.actionsGroup.actions[i].globalAction;
                        action.StartConfig(action, actionGroupST.actionsGroup.actions[i]);
                    }
                }
            }
        }
        public ActionsGroupStructure FindSpecificActionsGroup(string actionMovesetTag)
        {
            foreach (var action in specificActions)
            {
                if (action.movesetAction.tag == actionMovesetTag)
                {
                    return action;
                }
            }
            return null;
        }
        public async Task TriggerActionOnBaseAG(string actionTag, ActionsPriority priority, string weaponName)
        {
            if (!cancelationTokenSource.IsCancellationRequested)
            {
                await BaseAG.TriggerAction(actionTag, m_Animator, priority, weaponName, cancelationTokenSource.Token);
            }
        }
        public async Task TriggerActionOnSpecificsAG(string actionTag, ActionsPriority priority, string weaponName)
        {
            if (!cancelationTokenSource.IsCancellationRequested)
            {
                var currentWeapon = itemDB.FindItem(weaponName);

                for (int i = 0; i < specificActions.Count; i++)
                {
                    if (specificActions[i].movesetAction.tag == currentWeapon.actionsTag)
                    {
                        await SpecificsAGList[i].TriggerAction(actionTag, m_Animator, priority, weaponName, cancelationTokenSource.Token);
                        break;
                    }
                    else continue;
                }
            }
        }
        #endregion

        #region Gap Closer
        public bool EvaluateGapCloser(float minRange, float maxRange)
        {
            if (CurrentTarget != null)
            {
                float distance = Vector3.Distance(transform.position, CurrentTarget.position);

                if (distance <= maxRange && distance >= minRange)
                {
                    return true;
                }
                else return false;
            }

            return false;
        }
        public float GapCloserDistanceVerify()
        {
            return Vector3.Distance(transform.position, CurrentTarget.position);
        }
        public void PerformGapCloaser(float speed)
        {
            transform.position = Vector3.MoveTowards(transform.position, CurrentTarget.position, speed * Time.deltaTime);
        }
        #endregion

        #region Animation Events
        private void SetActionRunning() => CurrentAction.State = ActionState.Running;
        private void SetActionFinished() => CurrentAction.State = ActionState.Finished;
        private void SetSubActionEnter() => CurrentAction.HanddlerEnterSubAction(CurrentAction.CurrentStructure);
        private void SetSubActionExit() => CurrentAction.HanddlerExitSubAction();
        private void ActionResetValues() => CurrentAction.ResetValues();
        private void ApplyModifiers() => CurrentAction.HanddlerApplyModifiers();
        private void PerfomFX(MainHand hand) => CurrentAction.PerformFX(hand);
        private void PerfomSpecialFX() => CurrentAction.PerformSpecialFX();
        private void JumpToNextAttackAnim() => CurrentAction.HanddlerNextAttackAnim();
        #endregion
    }
}