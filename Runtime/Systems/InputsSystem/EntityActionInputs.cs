using UltimateFramework.InventorySystem;
using System.Collections.Generic;
using UltimateFramework.Utils;
using System.Threading.Tasks;
using UnityEngine;
using System;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace UltimateFramework.Inputs
{
    public class EntityActionInputs : UFBaseComponent
    {
        #region Public Fields
        [Header("Actions Values On Inputs")]
        public List<InputActionLogic> inputActions;

        [Header("Movement Settings")]
        public bool AnalogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;
        #endregion

        #region Func Events
        public Func<string, ActionsPriority, Task> ExecuteBaseAction;
        public Func<string, ActionsPriority, Task> ExecuteSpecificAction;
        #endregion

        #region Input Events
        public Action MenuInputPerform;
        #endregion

        #region Properties
        public Vector2 Move { get; set; }
        public Vector2 Look { get; set; }
        public bool Targeting { get; set; }
        public bool Jump { get; set; }
        public int AnimIDActionFinished { get; private set; }
        public int AnimIDLeftClickCount { get; private set; }
        public int AnimIDActionExit { get; private set; }
        public PlayerInput Playerinputs { get => _playerInput; private set => _playerInput = value; }
        public InventoryAndEquipmentComponent InventoryAndEquipment { get; private set; }
        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                var playerInput = gameObject.GetComponent<PlayerInput>();
                return InputsManager.GetIsCurrentDiviceMouse(playerInput);
#else
            return false;
#endif
            }
        }
        #endregion

        #region Private Fields
        private Animator m_Animator;
        private PlayerInput _playerInput;
        #endregion

        #region Mono
        private void Awake()
        {
            _playerInput = TryGetComponent<PlayerInput>(out _playerInput) ? _playerInput : null;
            m_Animator = GetComponent<Animator>();
            InventoryAndEquipment = GetComponent<InventoryAndEquipmentComponent>();

            SetUpInputActions();
            AssignAnimationIDs();
        }
        private void OnEnable()
        {
            foreach (var action in inputActions)
            {
                if (action.Input != null)
                    action.Input.action.performed += action.InputLogic;
            }

            ConfigLogicExtensionOnInputActions();
        }
        private void OnDisable()
        {
            foreach (var action in inputActions)
            {
                if (action.Input != null)
                    action.Input.action.performed -= action.InputLogic;
            }
        }
        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }
        #endregion

        #region Internal
        private void SetUpInputActions()
        {
            for (int i = 0; i < inputActions.Count; i++)
            {
                inputActions[i] = new
                (
                    this,
                    inputActions[i].Name,
                    inputActions[i].Input,
                    inputActions[i].UseTwoActionsOnInput,
                    inputActions[i].UseButtonAutoNegation,
                    inputActions[i].PrimaryAction,
                    inputActions[i].SecondaryAction
                );
            }
        }
        private void AssignAnimationIDs()
        {
            AnimIDActionFinished = Animator.StringToHash("ActionFinished");
            AnimIDLeftClickCount = Animator.StringToHash("LeftClickCount");
            AnimIDActionExit = Animator.StringToHash("ActionExit");
        }
        private void SetCursorState(bool newState)
        {
            Cursor.visible = newState;
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }
        #endregion

        #region Input Callbacks
        public void OnMove(InputValue value)
        {
            if (InputsManager.Player.enabled)
                MoveInput(value.Get<Vector2>());
            else MoveInput(Vector2.zero);
        }
        public void OnLook(InputValue value)
        {
            if (InputsManager.Player.enabled && cursorInputForLook)
                LookInput(value.Get<Vector2>());
        }
        public void OnTargetObjetive(InputValue value)
        {
            if (InputsManager.Player.enabled)
                TargetObjetiveInput(value.isPressed);
        }
        public void OnJump(InputValue value)
        {
            if (InputsManager.Player.enabled)
                JumpInput(value.isPressed);
        }
        #endregion

        #region Input Methods
        public void MoveInput(Vector2 newMoveDirection)
        {
            Move = newMoveDirection;
        }
        public void LookInput(Vector2 newLookDirection)
        {
            Look = newLookDirection;
        }
        private void TargetObjetiveInput(bool newTargettingState)
        {
            Targeting = newTargettingState;
        }
        public void JumpInput(bool newJumpState)
        {
            Jump = newJumpState;
        }
        #endregion

        #region Public Methods
        public InputActionLogic FindInputAction(string name)
        {
            foreach (var inputAction in inputActions)
            {
                if (inputAction.Name == name)
                {
                    return inputAction;
                }
            }
            return null;
        }
        public void ResetInputActionState(string name)
        {
            foreach (var inputAction in inputActions)
            {
                if (inputAction.Name == name)
                {
                    inputAction.State = false;
                }
            }
        }
        public InputAction GetInputActionOnCurrentMap(string actionName)
        {
            return Playerinputs.currentActionMap.FindAction(actionName);
        }
        #endregion

        #region Input Actions Extensions
        public void ConfigLogicExtensionOnInputActions()
        {
            if (this.gameObject.CompareTag("Player"))
            {
                InputActionLogic currentAction = FindInputAction("Attack");
                currentAction.logicExtension = () => SetAnimatorInt(AnimIDLeftClickCount, currentAction.PressCount);
            }
        }
        private void SetAnimatorInt(int animID, int value)
        {
            m_Animator.SetInteger(animID, value);
        }
        public void ResetPressCount(string actionName)
        {
            var currentAction = FindInputAction(actionName);
            currentAction.PressCount = 0;
            m_Animator.SetInteger(AnimIDLeftClickCount, currentAction.PressCount);
        }
        public void ResetReleaseCount(string actionName)
        {
            var currentAction = FindInputAction(actionName);
            currentAction.ReleaseCount = 0;
        }
        #endregion

        #region Events Handlers
        public void SwitchToUI() => InputsManager.SwitchToUI(this, IsCurrentDeviceMouse);
        public void SwitchToPlayer() => InputsManager.SwitchToPlayer(this, IsCurrentDeviceMouse);
        public void EnablePlayerInputsMap(bool value) => InputsManager.EnablePlayerMap(value);
        public void EnableUIInputsMap(bool value) => InputsManager.EnableUIMap(value);
        #endregion
    }
}