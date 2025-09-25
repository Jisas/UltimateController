using UltimateFramework.ActionsSystem;
using UltimateFramework.Editor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(ActionsComponent), true)]
public class ActionsManagerComponentEditor : Editor
{
    #region Private Fields
    private VisualElement root;
    private VisualTreeAsset baseVisual;
    private VisualTreeAsset specificActionElement;
    private ActionsComponent m_Target;
    private bool showActions = false;
    #endregion

    #region Mono
    private void OnEnable()
    {
        m_Target = (ActionsComponent)target;
    }
    public override VisualElement CreateInspectorGUI()
    {
        LoadResources();
        baseVisual.CloneTree(root);

        #region Find Elements
        var baseAction = UFEditorUtils.FindElementInRoot<ObjectField>(root, "base-actions");
        var listOpeber = UFEditorUtils.FindElementInRoot<VisualElement>(root, "list-opener");
        var listCount = UFEditorUtils.FindElementInRoot<Label>(root, "count");
        var addSpecificAction = UFEditorUtils.FindElementInRoot<Button>(root, "add-specific-action");
        var listBody = UFEditorUtils.FindElementInRoot<VisualElement>(root, "List-Body");
        #endregion

        #region Values Asignament
        baseAction.value = m_Target.BaseActions;
        listCount.text = m_Target.SpecificActions.Count > 0 ? $"{m_Target.SpecificActions.Count} elements" : "0";
        foreach (var specificAG in m_Target.SpecificActions)
        {
            AddSpecificAction(specificAG, listBody, listCount);
        }
        #endregion

        #region RegisterCallbacks
        baseAction.RegisterValueChangedCallback(evt =>
        {
            m_Target.BaseActions = (ActionsGroup)evt.newValue;
        });

        listOpeber.RegisterCallback<ClickEvent>(evt =>
        {
            showActions = !showActions;
            UFEditorUtils.SetElementDisplay(showActions, ref listBody);
        });

        addSpecificAction.RegisterCallback<ClickEvent>(evt =>
        {
            var newActionGroupStruct = new ActionsGroupStructure();
            m_Target.SpecificActions.Add(newActionGroupStruct);
            AddSpecificAction(newActionGroupStruct, listBody, listCount);
            EditorUtility.SetDirty(target);
        });
        #endregion

        return root;
    }
    #endregion

    #region Internal
    private void LoadResources()
    {
        root = new VisualElement();
        baseVisual = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Components/ActionsComponent_Inspector");
        specificActionElement = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Components/Templates/SpecificActionElement");
    }
    private void AddSpecificAction(ActionsGroupStructure actionGroupStruct, VisualElement container, Label listCount)
    {
        var instance = specificActionElement.CloneTree();
        container.Add(instance);

        #region Find Element
        var actionGroup = UFEditorUtils.FindElementInRoot<ObjectField>(instance, "action-group");
        var tagSelector = UFEditorUtils.FindElementInRoot<Button>(instance, "tag-selector-button");
        var elementTag = UFEditorUtils.FindElementInRoot<Label>(tagSelector, "tag");
        var removeButton = UFEditorUtils.FindElementInRoot<Button>(instance, "remove-button");
        #endregion

        #region Value Asignment
        listCount.text = $"{m_Target.SpecificActions.Count} elements";
        actionGroup.value = actionGroupStruct.actionsGroup;
        elementTag.text = actionGroupStruct.movesetAction.tag;
        #endregion

        #region Register Callbacks
        actionGroup.RegisterValueChangedCallback(evt =>
        {
            actionGroupStruct.actionsGroup = (ActionsGroup)evt.newValue;
            EditorUtility.SetDirty(target);
        });

        tagSelector.RegisterCallback<ClickEvent>(evt =>
        {
            UFEditorUtils.OpenTagSelectorWindow(elementTag, actionGroupStruct.movesetAction, "tag");
            EditorUtility.SetDirty(target);
        });

        removeButton.RegisterCallback<ClickEvent>(evt =>
        {
            RemoveSpecificAction(actionGroupStruct, instance, container);
            listCount.text = $"{m_Target.SpecificActions.Count} elements";
            EditorUtility.SetDirty(target);
        });
        #endregion
    }
    private void RemoveSpecificAction(ActionsGroupStructure actionGroupStruct, TemplateContainer instance, VisualElement container)
    {
        m_Target.SpecificActions.Remove(actionGroupStruct);
        container.Remove(instance);
    }
    #endregion
}
#endif