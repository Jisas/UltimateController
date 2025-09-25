using UltimateFramework.Utils;
using UnityEngine;
using System;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace UltimateFramework.Inputs
{
    [Serializable]
    public class InputActionLogic
    {
        [SerializeField] private string name;
        [SerializeField] private InputActionReference input;
        [SerializeField, Tooltip("This value will enable you to call two different actions based on the value of the input (true and false).")]
        private bool useTwoActionsOnInput;
        [SerializeField, Tooltip("For this value to take effect the input must be of type button, use it when you want it to change between true and false every time you press it.")]
        private bool useButtonAutoNegation;
        [SerializeField] private InputActionStructure primaryAction = new();
        [SerializeField] private InputActionStructure secondaryAction = new();
        private readonly EntityActionInputs entityInputs;

        public InputActionLogic() { }
        public InputActionLogic(
            EntityActionInputs manager, 
            string name, 
            InputActionReference input, 
            bool useTwoActionsOnInput, 
            bool useButtonAutoNegation, 
            InputActionStructure primaryAction, 
            InputActionStructure secondaryAction)
        {
            entityInputs = manager;
            Name = name;
            Input = input;
            UseTwoActionsOnInput = useTwoActionsOnInput;
            UseButtonAutoNegation = useButtonAutoNegation;
            PrimaryAction = primaryAction;
            SecondaryAction = secondaryAction;
        }

        public bool State { get; set; } = false;
        public int PressCount { get; set; } = 0;
        public int ReleaseCount { get; set; } = 0;
        public string Name { get => name; set => name = value; }
        public InputActionReference Input { get => input; set => input = value; }
        public bool UseTwoActionsOnInput { get => useTwoActionsOnInput; set => useTwoActionsOnInput = value; }
        public bool UseButtonAutoNegation { get => useButtonAutoNegation; set => useButtonAutoNegation = value; }
        public InputActionStructure PrimaryAction { get => primaryAction; set => primaryAction = value; }
        public InputActionStructure SecondaryAction { get => secondaryAction; set => secondaryAction = value; }

        public EntityActionInputs EntityInputs => entityInputs;

        public Action logicExtension;

        [Serializable]
        public class InputActionStructure
        {
            public bool isBaseAction;
            public TagSelector actionTag = new("None");
            public ActionsPriority priority;
        }

        public void InputLogic(InputAction.CallbackContext context)
        {
            if (Input.action.type == InputActionType.Button)
            {
                if (UseTwoActionsOnInput)
                {
                    var inputValue = context.action.IsPressed();

                    if (UseButtonAutoNegation)
                    {
                        // Change button value when is pressed
                        if (inputValue) State = !State;
                        inputValue = State;
                    }

                    if (inputValue) {
                        PressCount++;
                        logicExtension?.Invoke();
                        ExecuteAction(PrimaryAction.actionTag.tag, PrimaryAction.priority, PrimaryAction.isBaseAction);
                    }
                    else {
                        PressCount++;
                        logicExtension?.Invoke();
                        ExecuteAction(SecondaryAction.actionTag.tag, SecondaryAction.priority, SecondaryAction.isBaseAction);
                    }
                }
                else
                {
                    var inputValue = context.action.IsPressed();

                    if (inputValue) {
                        PressCount++;
                        logicExtension?.Invoke();
                        ExecuteAction(PrimaryAction.actionTag.tag, PrimaryAction.priority, PrimaryAction.isBaseAction);
                    }
                }
            }
            else if (Input.action.type == InputActionType.PassThrough)
            {
                var inputValue = context.action.IsPressed();
                State = inputValue;

                if (UseTwoActionsOnInput)
                {
                    if (inputValue) {
                        PressCount++;
                        logicExtension?.Invoke();
                        ExecuteAction(PrimaryAction.actionTag.tag, PrimaryAction.priority, PrimaryAction.isBaseAction);
                    }
                    else {
                        ReleaseCount++;
                        logicExtension?.Invoke();
                        ExecuteAction(SecondaryAction.actionTag.tag, SecondaryAction.priority, SecondaryAction.isBaseAction);
                    }
                }
                else
                {
                    if (inputValue) {
                        PressCount++;
                        logicExtension?.Invoke();
                        ExecuteAction(PrimaryAction.actionTag.tag, PrimaryAction.priority, PrimaryAction.isBaseAction);
                    }
                }
            }
        }

        public void ExecuteAction(string tag, ActionsPriority priority, bool isBaseAction)
        {
            if (InputsManager.Player.enabled)
            {
                if (isBaseAction) EntityInputs.ExecuteBaseAction.Invoke(tag, priority);
                else EntityInputs.ExecuteSpecificAction.Invoke(tag, priority);
            }
        }
    }
}