using UltimateFramework.LocomotionSystem;
using UnityEngine.UIElements;

namespace UltimateFramework.Editor.TabSystem
{
    public abstract class TabStrategy
    {
        public void ShowContent(object target, VisualElement container)
        {
            UFEditorUtils.SetElementDisplay(true, ref container);
            SetupTabContent(target, container);
        }
        public void HideContent(object target, VisualElement container)
        {
            UFEditorUtils.SetElementDisplay(false, ref container);
        }
        protected abstract void SetupTabContent(object target, VisualElement container);
        public virtual void UpdateTabContent(object target, VisualElement container) { }
    }
}
