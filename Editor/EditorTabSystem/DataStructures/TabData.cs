using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace UltimateFramework.Editor.TabSystem
{
#if UNITY_EDITOR
    public class TabData
    {
        public ToolbarButton toolbarButton;
        public VisualElement contentContainer;
        public TabStrategy strategy;

        public TabData(ToolbarButton toolbarButton, VisualElement contentContainer, TabStrategy strategy)
        {
            this.toolbarButton = toolbarButton;
            this.contentContainer = contentContainer;
            this.strategy = strategy;
        }
    }
#endif
}
