using UltimateFramework.Inputs;
using UnityEngine.InputSystem;
using UltimateFramework.Utils;
using UnityEngine.UI;
using UnityEngine;

namespace UltimateFramework.UISystem
{
    [RequireComponent(typeof(Image))]
    public class InputIconHandler : MonoBehaviour
    {
        public InputIconPathType searchType;
        [MyBox.ConditionalField(nameof(searchType), false, InputIconPathType.InputAction)] public string actionName;
        [MyBox.ConditionalField(nameof(searchType), false, InputIconPathType.InputPath)] public string gamepadPath;
        [MyBox.ConditionalField(nameof(searchType), false, InputIconPathType.InputPath)] public string keyboardPath;

        private EntityActionInputs _actions;
        private InputIconsMap _iconsMap;
        private Image _image;

        private void OnEnable()
        {
            InputSystem.onDeviceChange += OnDeviceChange;

            _image = GetComponent<Image>();
            _iconsMap = Resources.Load<InputIconsMap>("Data/Inputs/InputIconsMap");
            _actions = GameObject.FindGameObjectWithTag("Player").GetComponent<EntityActionInputs>();
        }
        private void Start()
        {
            UpdateInputIcon();
        }
        private void OnDisable()
        {
            InputSystem.onDeviceChange -= OnDeviceChange;
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (change == InputDeviceChange.Added || change == InputDeviceChange.Removed || change == InputDeviceChange.ConfigurationChanged ||
                change == InputDeviceChange.Reconnected || change == InputDeviceChange.Disconnected || change == InputDeviceChange.UsageChanged)
            {
                UpdateInputIcon();
            }
        }
        private void UpdateInputIcon()
        {
            InputActionReference inputActionReference = null;

            if (searchType == InputIconPathType.InputAction) 
                inputActionReference = _actions.FindInputAction(actionName).Input;          

            var device = InputSystem.GetDevice<Gamepad>() != null ?
                InputSystem.GetDevice<Gamepad>()?.displayName :
                InputSystem.GetDevice<Keyboard>() != null ? "Keyboard" :
                null;

            var group = InputSystem.GetDevice<Gamepad>() != null ?  "Gamepad": 
                InputSystem.GetDevice<Keyboard>() != null ? "Keyboard&Mouse" : 
                null;

            if (device != null && group != null)
            {
                string controlPath;

                if (searchType == InputIconPathType.InputAction && inputActionReference != null)
                     controlPath = GetControlPathForDevice(inputActionReference, group);

                else controlPath = InputSystem.GetDevice<Gamepad>() != null ? gamepadPath : keyboardPath;

                if (!string.IsNullOrEmpty(controlPath))
                {
                    var icon = _iconsMap.GetInputIcon(device, controlPath);
                    if (icon != null)
                    {
                        _image.sprite = icon;
                        _image.preserveAspect = true;
                    }
                }
            }
        }
        private string GetControlPathForDevice(InputActionReference inputActionReference, string groupName)
        {
            foreach (var binding in inputActionReference.action.bindings)
            {
                if (binding.groups.Contains(groupName))
                {
                    var pathParts = binding.effectivePath.Split('/');
                    return pathParts.Length > 1 ? pathParts[1] : null;
                }
            }
            return null;
        }
    }
}
