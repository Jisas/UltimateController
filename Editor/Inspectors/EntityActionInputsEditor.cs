using UltimateFramework.Inputs;
using UltimateFramework.Editor;
using UnityEngine.InputSystem;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using System;

#if UNITY_EDITOR
[CustomEditor(typeof(EntityActionInputs), true)]
public class EntityActionInputsEditor : Editor
{
    #region Private Fields
    private VisualElement root;
    private VisualTreeAsset baseVisual;
    private VisualTreeAsset inputActionElement;
    private EntityActionInputs m_Target;
    private bool showActions = false;
    #endregion

    #region Mono
    private void OnEnable()
    {
        m_Target = (EntityActionInputs)target;
    }
    public override VisualElement CreateInspectorGUI()
    {
        LoadResources();
        baseVisual.CloneTree(root);

        #region Find Elements
        var listOpener = UFEditorUtils.FindElementInRoot<VisualElement>(root, "list-opener");
        var listArrow = UFEditorUtils.FindElementInRoot<VisualElement>(root, "arrow");
        var listCount = UFEditorUtils.FindElementInRoot<Label>(root, "count");
        var listContainer = UFEditorUtils.FindElementInRoot<VisualElement>(root, "List-Body");
        var addButton = UFEditorUtils.FindElementInRoot<Button>(root, "add-input-action");
        var analogMovement = UFEditorUtils.FindElementInRoot<Toggle>(root, "analog-movement");
        var cursorLocked = UFEditorUtils.FindElementInRoot<Toggle>(root, "cursor-locked");
        var cursorForLook = UFEditorUtils.FindElementInRoot<Toggle>(root, "cursor-look");
        #endregion

        #region Value Asignament
        listCount.text = $"{m_Target.inputActions.Count} elements";
        analogMovement.value = m_Target.AnalogMovement;
        cursorLocked.value = m_Target.cursorLocked;
        cursorForLook.value = m_Target.cursorInputForLook;
        foreach (var actionLogic in m_Target.inputActions)
        {
            AddInputAction(actionLogic, listContainer, listCount);
        }
        #endregion

        #region Register Callbacks
        listOpener.RegisterCallback<ClickEvent>(evt =>
        {
            showActions = !showActions;
            UFEditorUtils.SetElementDisplay(showActions, ref listContainer);
            UFEditorUtils.SetArrowAnim(listContainer, ref listArrow, "arrowmark-toggle-open");
        });

        addButton.clickable.clicked += () =>
        {
            var newInutLogic = new InputActionLogic();
            m_Target.inputActions.Add(newInutLogic);
            AddInputAction(newInutLogic, listContainer, listCount);
            EditorUtility.SetDirty(target);
        };

        analogMovement.RegisterValueChangedCallback(evt =>
        {
            m_Target.AnalogMovement = evt.newValue;
            EditorUtility.SetDirty(target);
        });

        cursorLocked.RegisterValueChangedCallback(evt =>
        {
            m_Target.cursorLocked = evt.newValue;
            EditorUtility.SetDirty(target);
        });

        cursorForLook.RegisterValueChangedCallback(evt =>
        {
            m_Target.cursorInputForLook = evt.newValue;
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
        baseVisual = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Components/EntityActionInputs_Inspector");
        inputActionElement = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Components/Templates/InputActionElement");
    }
    private void AddInputAction(InputActionLogic inputLogic, VisualElement container, Label listCount)
    {
        if (inputLogic == null) throw new Exception("Input data is null");

        listCount.text = $"{m_Target.inputActions.Count} elements";
        var instance = inputActionElement.CloneTree();
        container.Add(instance);

        #region Base
        // Find Elements
        var actionName = UFEditorUtils.FindElementInRoot<TextField>(instance, "input-action-name");
        var inputRef = UFEditorUtils.FindElementInRoot<ObjectField>(instance, "input-ref");
        var removeButton = UFEditorUtils.FindElementInRoot<Button>(instance, "remove-input-action");
        var autoValueInversion = UFEditorUtils.FindElementInRoot<ToolbarToggle>(instance, "auto-value-inversion");
        var twoActionsInput = UFEditorUtils.FindElementInRoot<ToolbarToggle>(instance, "two-actions-input");
        var primaryActionElement = UFEditorUtils.FindElementInRoot<VisualElement>(instance, "primary-action");
        var secondaryActionElement = UFEditorUtils.FindElementInRoot<VisualElement>(instance, "secondary-action");

        // Value Asignation
        actionName.value = inputLogic.Name;
        inputRef.value = inputLogic.Input;

        autoValueInversion.value = inputLogic.UseButtonAutoNegation;
        UFEditorUtils.SwitchToolbarToggleColor(autoValueInversion, inputLogic.UseButtonAutoNegation);

        twoActionsInput.value = inputLogic.UseTwoActionsOnInput;
        UFEditorUtils.SwitchToolbarToggleColor(twoActionsInput, inputLogic.UseTwoActionsOnInput);
        if(twoActionsInput.value) UFEditorUtils.SetElementDisplay(inputLogic.UseTwoActionsOnInput, ref secondaryActionElement);

        // Register callbacks
        inputRef.RegisterValueChangedCallback((evt) =>
        {
            inputLogic.Input = (InputActionReference)evt.newValue;
            EditorUtility.SetDirty(target);
        });

        actionName.RegisterValueChangedCallback((evt) =>
        {
            inputLogic.Name = evt.newValue;
            EditorUtility.SetDirty(target);
        });

        removeButton.clickable.clicked += () =>
        {
            RemoveInputAction(inputLogic, instance, container);
            listCount.text = $"{m_Target.inputActions.Count} elements";
            EditorUtility.SetDirty(target);
        };

        autoValueInversion.RegisterCallback<ClickEvent>(evt =>
        {
            inputLogic.UseButtonAutoNegation = !inputLogic.UseButtonAutoNegation;
            UFEditorUtils.SwitchToolbarToggleColor(autoValueInversion, inputLogic.UseButtonAutoNegation);
            EditorUtility.SetDirty(target);
        });

        twoActionsInput.RegisterCallback<ClickEvent>(evt =>
        {
            inputLogic.UseTwoActionsOnInput = !inputLogic.UseTwoActionsOnInput;
            UFEditorUtils.SwitchToolbarToggleColor(twoActionsInput, inputLogic.UseTwoActionsOnInput);
            UFEditorUtils.SetElementDisplay(inputLogic.UseTwoActionsOnInput, ref secondaryActionElement);
            EditorUtility.SetDirty(target);
        });
        #endregion

        #region Primary Action
        // Find Elements
        var p_isBaseAction = UFEditorUtils.FindElementInRoot<Button>(primaryActionElement, "is-base-action");
        var p_tagSeletor = UFEditorUtils.FindElementInRoot<Button>(primaryActionElement, "tag-selector-button");
        var p_inputActionTag = UFEditorUtils.FindElementInRoot<Label>(primaryActionElement, "input-action-tag");
        var p_actionPriority = UFEditorUtils.FindElementInRoot<EnumField>(primaryActionElement, "action-priority");

        // Value Asignation
        UFEditorUtils.SwitchButonColor(p_isBaseAction, inputLogic.PrimaryAction.isBaseAction);

        p_inputActionTag.text = !String.IsNullOrEmpty(inputLogic.PrimaryAction.actionTag.tag) ? 
            inputLogic.PrimaryAction.actionTag.tag : "None";

        p_actionPriority.value = inputLogic.PrimaryAction.priority;

        // Register Callbacks
        p_isBaseAction.clickable.clicked += () => 
        {
            inputLogic.PrimaryAction.isBaseAction = !inputLogic.PrimaryAction.isBaseAction;
            UFEditorUtils.SwitchButonColor(p_isBaseAction, inputLogic.PrimaryAction.isBaseAction);
            EditorUtility.SetDirty(target);
        };

        p_tagSeletor.clickable.clicked += () =>
        {
            UFEditorUtils.OpenTagSelectorWindow(p_inputActionTag, inputLogic.PrimaryAction.actionTag, "tag");
            EditorUtility.SetDirty(target);
        };

        p_actionPriority.RegisterValueChangedCallback((evt) =>
        {
            inputLogic.PrimaryAction.priority = (UltimateFramework.Utils.ActionsPriority)evt.newValue;
            EditorUtility.SetDirty(target);
        });
        #endregion

        #region Secondary Action
        // Find Elements
        var s_isBaseAction = UFEditorUtils.FindElementInRoot<Button>(secondaryActionElement, "is-base-action");
        var s_tagSeletor = UFEditorUtils.FindElementInRoot<Button>(secondaryActionElement, "tag-selector-button");
        var s_inputActionTag = UFEditorUtils.FindElementInRoot<Label>(secondaryActionElement, "input-action-tag");
        var s_actionPriority = UFEditorUtils.FindElementInRoot<EnumField>(secondaryActionElement, "action-priority");

        // Value Asignation
        UFEditorUtils.SwitchButonColor(s_isBaseAction, inputLogic.SecondaryAction.isBaseAction);

        s_inputActionTag.text = !String.IsNullOrEmpty(inputLogic.SecondaryAction.actionTag.tag) ?
            inputLogic.SecondaryAction.actionTag.tag : "None";

        s_actionPriority.value = inputLogic.SecondaryAction.priority;

        // Register Callbacks
        s_isBaseAction.clickable.clicked += () =>
        {
            inputLogic.SecondaryAction.isBaseAction = !inputLogic.SecondaryAction.isBaseAction;
            UFEditorUtils.SwitchButonColor(s_isBaseAction, inputLogic.SecondaryAction.isBaseAction);
            EditorUtility.SetDirty(target);
        };

        s_tagSeletor.clickable.clicked += () =>
        {
            UFEditorUtils.OpenTagSelectorWindow(s_inputActionTag, inputLogic.SecondaryAction.actionTag, "tag");
            EditorUtility.SetDirty(target);
        };

        s_actionPriority.RegisterValueChangedCallback((evt) =>
        {
            inputLogic.SecondaryAction.priority = (UltimateFramework.Utils.ActionsPriority)evt.newValue;
            EditorUtility.SetDirty(target);
        });
        #endregion
    }
    private void RemoveInputAction(InputActionLogic inputLogic, TemplateContainer instance, VisualElement container)
    {
        m_Target.inputActions.Remove(inputLogic);
        container.Remove(instance);
    }
    #endregion
}
#endif