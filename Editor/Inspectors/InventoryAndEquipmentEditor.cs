using UltimateFramework.Editor.TabSystem.Strategies.InventoryAndEquipment;
using UltimateFramework.Editor.TabSystem;
using UltimateFramework.InventorySystem;
using System.Collections.Generic;
using UltimateFramework.Editor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InventoryAndEquipmentComponent), true)]
public class InventoryAndEquipmentEditor : Editor
{
    #region Private Fields
    private VisualElement root;
    private VisualTreeAsset baseVisual;
    private InventoryAndEquipmentComponent m_Target;
    private VisualElement baseContentContainer;

    private int lastPressedIndex = 0;
    private bool showBaseContentContainer = false;
    private Dictionary<int, TabData> TabsDictionary;
    #endregion

    #region Mono
    private void OnEnable()
    {
        m_Target = (InventoryAndEquipmentComponent)target;
        EditorApplication.update += UpdateUI;
    }
    private void OnDisable()
    {
        EditorApplication.update -= UpdateUI;
    }
    public override VisualElement CreateInspectorGUI()
    {
        LoadResources();
        baseVisual.CloneTree(root);

        #region Find Elements
        // Base
        var baseButtonsHeader = UFEditorUtils.FindElementInRoot<VisualElement>(root, "Buttons-Header");
        baseContentContainer = UFEditorUtils.FindElementInRoot<VisualElement>(root, "Content-Container");

        // Buttons
        var inventoryTab = UFEditorUtils.FindElementInRoot<ToolbarButton>(baseButtonsHeader, "inventory-tab");
        var equipmentTab = UFEditorUtils.FindElementInRoot<ToolbarButton>(baseButtonsHeader, "equipment-tab");

        // Conent
        var inventoryContent = UFEditorUtils.FindElementInRoot<VisualElement>(baseContentContainer, "inventory-body");
        var equipmentContent = UFEditorUtils.FindElementInRoot<VisualElement>(baseContentContainer, "equipment-body");
        #endregion

        #region Dictionary Asignament
        TabsDictionary = new Dictionary<int, TabData>()
        {
            {0, new TabData(inventoryTab, inventoryContent, new InventoryTabStrategy())},
            {1, new TabData(equipmentTab, equipmentContent, new EquipmentTabStrategy())},
        };
        #endregion

        foreach (var tadData in TabsDictionary)
            tadData.Value.toolbarButton.clickable.clicked += () => ShowTabContent(tadData, baseContentContainer);

        return root;
    }
    private void UpdateUI()
    {
        if (Application.isPlaying)
        {
            foreach (var tadData in TabsDictionary)
                UpdateTabContent(tadData);
        }
    }
    #endregion

    #region Internal
    private void LoadResources()
    {
        root = new VisualElement();
        baseVisual = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Components/InventoryAndEquiment_Inspector");
    }
    private void ShowTabContent(KeyValuePair<int, TabData> tabData, VisualElement baseContentContainer)
    {
        int currentIndex = tabData.Key;
        VisualElement curentContent = tabData.Value.contentContainer;
        TabStrategy currentStrategy = tabData.Value.strategy;

        if (currentIndex == lastPressedIndex)
        {
            showBaseContentContainer = !showBaseContentContainer;
            UFEditorUtils.SetElementDisplay(showBaseContentContainer, ref baseContentContainer);
        }
        else UFEditorUtils.SetElementDisplay(true, ref baseContentContainer);

        currentStrategy.ShowContent(m_Target, curentContent);
        lastPressedIndex = currentIndex;

        foreach (var othersTadData in TabsDictionary)
        {
            if (othersTadData.Key == currentIndex) continue;
            othersTadData.Value.strategy.HideContent(m_Target, othersTadData.Value.contentContainer);
        }
    }
    private void UpdateTabContent(KeyValuePair<int, TabData> tabData)
    {
        VisualElement curentContent = tabData.Value.contentContainer;
        TabStrategy currentStrategy = tabData.Value.strategy;
        currentStrategy.UpdateTabContent(m_Target, curentContent);
    }
    #endregion
}