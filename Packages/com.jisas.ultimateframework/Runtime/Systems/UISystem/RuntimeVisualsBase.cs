using UltimateFramework.Inputs;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UltimateFramework.UISystem
{
    public abstract class RuntimeVisualsBase : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] protected UIDocument UIDoc;
        [SerializeField] private PlayerInput playerInputs;
        [SerializeField] private EntityActionInputs entityInputs;

        protected VisualElement root;
        protected bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return InputsManager.GetIsCurrentDiviceMouse(playerInputs);
#else
			return false;
#endif
            }
        }

        public virtual void OnShow()
        {
            InputsManager.SwitchToUI(entityInputs, IsCurrentDeviceMouse);
            root.style.display = DisplayStyle.Flex;
        }
        public virtual void OnHide()
        {
            InputsManager.SwitchToPlayer(entityInputs, IsCurrentDeviceMouse);
            root.style.display = DisplayStyle.None;
        }

        protected T FindElementInRoot<T>(VisualElement root, string name) where T : VisualElement
        {
            return root.Q<T>(name);
        }
    }
}
