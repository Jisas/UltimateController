using UltimateFramework.Editor.TabSystem.Strategies.Locomotion;
using UltimateFramework.Editor.TabSystem;
using UltimateFramework.LocomotionSystem;
using System.Collections.Generic;
using UltimateFramework.Editor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(BaseLocomotionComponent), true)]
public class TPLocomotionComponentEditor : Editor
{
    #region Private Fields
    private VisualElement root;
    private VisualTreeAsset baseVisual;
    private BaseLocomotionComponent m_Target;

    private int lastPressedIndex = 0;
    private bool showBaseContentContainer = false;
    private Dictionary<int, ToolbarButton> TabButtonsDictionary;
    private Dictionary<int, VisualElement> TabContentDictionary;
    private Dictionary<int, TabStrategy> TabStrategyDictionary;
    #endregion

    #region Mono
    private void OnEnable()
    {
        m_Target = (BaseLocomotionComponent)target;
    }
    public override VisualElement CreateInspectorGUI()
    {
        LoadResources();
        baseVisual.CloneTree(root);

        #region Find Elements
        // Base
        var baseButtonsContainer = UFEditorUtils.FindElementInRoot<VisualElement>(root, "Buttons-Header");
        var baseContentContainer = UFEditorUtils.FindElementInRoot<VisualElement>(root, "Content-Container");

        // Tab Butons
        var startConfigButton = UFEditorUtils.FindElementInRoot<ToolbarButton>(baseButtonsContainer, "start-config-button");
        var settingsButton = UFEditorUtils.FindElementInRoot<ToolbarButton>(baseButtonsContainer, "settings-button");
        var footIKButton = UFEditorUtils.FindElementInRoot<ToolbarButton>(baseButtonsContainer, "foot-ik-button");
        var bodyInclinationButton = UFEditorUtils.FindElementInRoot<ToolbarButton>(baseButtonsContainer, "body-inclination-button");
        var headTrackingButton = UFEditorUtils.FindElementInRoot<ToolbarButton>(baseButtonsContainer, "head-tracking-button");
        var sfxButton = UFEditorUtils.FindElementInRoot<ToolbarButton>(baseButtonsContainer, "sfx-button");

        // Tab Contet
        var startConfigBody = UFEditorUtils.FindElementInRoot<VisualElement>(baseContentContainer, "star-config-body");
        var settingsBody = UFEditorUtils.FindElementInRoot<VisualElement>(baseContentContainer, "settings-body");
        var footIKBody = UFEditorUtils.FindElementInRoot<VisualElement>(baseContentContainer, "foot-ik-body");
        var bodyInclinationBody = UFEditorUtils.FindElementInRoot<VisualElement>(baseContentContainer, "body-inclination-body");
        var headTrackingBody = UFEditorUtils.FindElementInRoot<VisualElement>(baseContentContainer, "head-tracking-body");
        var sfxBody = UFEditorUtils.FindElementInRoot<VisualElement>(baseContentContainer, "sfx-body");
        #endregion

        #region Dictionaries Asignament
        TabButtonsDictionary = new()
        {
            {0, startConfigButton},
            {1, settingsButton},
            {2, footIKButton},
            {3, bodyInclinationButton},
            {4, headTrackingButton},
            {5, sfxButton},
        };

        TabContentDictionary = new()
        {
            {0, startConfigBody},
            {1, settingsBody},
            {2, footIKBody},
            {3, bodyInclinationBody},
            {4, headTrackingBody},
            {5, sfxBody},
        };

        TabStrategyDictionary = new()
        {
            {0, new StartConfigStrategy()},
            {1, new SettingsStrategy()},
            {2, new FootIKStrategy()},
            {3, new BodyInclinationStrategy()},
            {4, new HeadTrackingStrategy()},
            {5, new SFXStrategy()},
        };
        #endregion

        // Setup Functionality
        foreach (var tabButton in TabButtonsDictionary)
            tabButton.Value.clickable.clicked += () => ShowTabContent(tabButton, baseContentContainer);

        return root;
    }
    #endregion

    #region Internal
    private void LoadResources()
    {
        root = new VisualElement();
        baseVisual = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Components/TPLocomotionComponent_Inspector");
    }
    private void ShowTabContent(KeyValuePair<int, ToolbarButton> tabButton, VisualElement baseContentContainer)
    {
        int currentIndex = tabButton.Key;

        if (currentIndex == lastPressedIndex)
        {
            showBaseContentContainer = !showBaseContentContainer;
            UFEditorUtils.SetElementDisplay(showBaseContentContainer, ref baseContentContainer);
        }
        else UFEditorUtils.SetElementDisplay(true, ref baseContentContainer);

        TabStrategyDictionary[currentIndex].ShowContent(m_Target, TabContentDictionary[currentIndex]);
        lastPressedIndex = currentIndex;

        foreach (var strategy in TabStrategyDictionary)
        {
            if (strategy.Key == currentIndex) continue;
            strategy.Value.HideContent(m_Target, TabContentDictionary[strategy.Key]);
        }
    }
    #endregion
}
#endif