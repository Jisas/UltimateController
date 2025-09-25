using UnityEngine.Rendering.Universal;
using UltimateFramework.Inputs;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine;

namespace UltimateFramework.UISystem
{
    public abstract class RuntimeVisualsBase : MonoBehaviour
    {
        #region Serialized Fields
        [Header("References")]
        [SerializeField] protected GameObject UICanvas;
        [SerializeField] protected PlayerInput playerInputs;
        [SerializeField] protected EntityActionInputs entityInputs;
        [SerializeField] protected Volume volume;
        #endregion

        #region Private And Protected Fields
        protected DepthOfField depthOfField;
        #endregion

        #region Properties
        public static RuntimeVisualsBase ActiveWindow { get; private set; } = null;
        protected abstract RuntimeVisualsBase PreviousWindow { get; }
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
        #endregion

        #region Private Fields
        private RuntimeVisualsBase firstWindow;
        #endregion

        #region Mono
        private void Start() => ClearWindowStack();
        #endregion

        #region Internal
        protected void InitializeFirstWindow(RuntimeVisualsBase firstWindow)
        {
            ActiveWindow = firstWindow;
            this.firstWindow = firstWindow;
        }
        protected void ClearWindowStack() => ActiveWindow = null;

        protected virtual void OnShow()
        {
            if (ActiveWindow != null && ActiveWindow != firstWindow)
                ActiveWindow.OnHide();

            ActiveWindow = this;
            UICanvas.SetActive(true);
            InputsManager.SwitchToUI(entityInputs, IsCurrentDeviceMouse);
        }

        protected virtual void OnHide()
        {
            UICanvas.SetActive(false);
            InputsManager.SwitchToPlayer(entityInputs, IsCurrentDeviceMouse);
        }

        protected void OnBack()
        {
            if (PreviousWindow != null)
            {
                OnHide();
                PreviousWindow.OnShow();
            }
            else
            {
                OnHide();
                ActiveWindow = null;
            }
        }
        #endregion
    }
}
