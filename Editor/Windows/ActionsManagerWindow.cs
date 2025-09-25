using UltimateFramework.ActionsSystem;
using System.Collections.Generic;
using UltimateFramework.Utils;
using UltimateFramework.Tools;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System;

public class ActionsManagerWindow : EditorWindow
{
    private VisualTreeAsset m_UXML;
    private VisualElement base_Root;
    private SettingsMasterData settingsData;
    private ActionsMasterData actionsData;
    private VisualTreeAsset ActionGroupElement; 
    private VisualElement ActionGroupStructureContainer;
    private VisualTreeAsset ActionElement;
    private VisualTreeAsset ActionAssetElement;
    private VisualTreeAsset ActionStatModifierElement;
    private Button saveButton;
    private EditorWindow currentTagSelectorWindow;
    private readonly List<VisualElement> m_ActionGroupsTemplates = new();

    private Color startColor = new(0, 0, 0, 0);
    private Color endColor = new(1, 1, 0.4823529f, 1);

    [MenuItem("Ultimate Framework/Windows/Actions Manager")]
    public static void ShowMyEditor()
    {
        EditorWindow wnd = GetWindow<ActionsManagerWindow>();
        Texture2D icon = Resources.Load<Texture2D>("Img/ActionsManager_Icon");
        wnd.titleContent = new GUIContent("Actions Manager", icon);
    }
    private void LoadResources()
    {
        settingsData = SettingsMasterData.Instance;
        actionsData = settingsData.actionsMasterData;
        m_UXML = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/ActionsManagerWindow");
        ActionGroupElement = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/Templates/ActionsGroupUpPanelElement");
        ActionElement = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/Templates/ActionElement");
        ActionAssetElement = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/Templates/ActionAssetElement");
        ActionStatModifierElement = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/Templates/ActionStatModifierElement");
    }
    private void CreateGUI()
    {
        LoadResources();
        base_Root = rootVisualElement;
        m_UXML.CloneTree(base_Root);

        var searchField = base_Root.Q<ToolbarSearchField>("search-field");
        searchField.RegisterValueChangedCallback((evt) => Search(evt));

        var actionGroupsContainer = FindElementInRoot<ScrollView>("action-groups-elements-scroll-view");
        ActionGroupStructureContainer = FindElementInRoot<ScrollView>("action-groups-content-scroll-view");

        var createActionGroupsButton = FindElementInRoot<Button>("create-action-gruop-button");
        createActionGroupsButton.clickable.clicked += () =>
        {
            CreateActionGroup(actionGroupsContainer);
            SetEndColorToSaveButton();
        };

        var addExistentGroupButton = FindElementInRoot<Button>("add-existent-action-gruop-button");
        var existentGroupSubPanel = FindElementInRoot<VisualElement>("existen-asset-panel");
        var existenGroupObjectField = FindElementInRoot<ObjectField>("existen-object-field");
        var addExistentObjectButton = FindElementInRoot<Button>("add-existent-button");
        ActionsGroup existenGroup = null;

        addExistentGroupButton.clickable.clicked += () =>
        {
            SetElementDisplay(existentGroupSubPanel);
        };

        existenGroupObjectField.RegisterValueChangedCallback((evt) =>
        {
            existenGroup = evt.newValue as ActionsGroup;
        });

        AddObjectFieldManipulators(existenGroupObjectField);
        addExistentObjectButton.clickable.clicked += () => 
        { 
            AddExistentGroup(existenGroup, actionGroupsContainer); 
            SetEndColorToSaveButton(); 
        };

        var groupsAmmonut = FindElementInRoot<Label>("groups-ammount-label");
        groupsAmmonut.text = actionsData.actionGroups.Count.ToString();

        saveButton = FindElementInRoot<ToolbarButton>("save-data-button");
        saveButton.clickable.clicked += () => SaveData();
        SetStartColorToSaveButton();

        if (actionsData.actionGroups.Count > 0)
        {
            for (int i = 0; i < actionsData.actionGroups.Count; i++)
            {
                InstanceInfoOfGroup(actionsData.actionGroups[i], actionGroupsContainer);
            }
        }
    }
    private void InstanceInfoOfGroup(ActionsGroup group, VisualElement container)
    {
        var instance = ActionGroupElement.CloneTree();
        m_ActionGroupsTemplates.Add(instance);
        container.Add(instance);

        #region Fields
        var groupName = FindElementInRoot<TextField>(instance, "gruop-name");
        var viewGroupButton = FindElementInRoot<Button>(instance, "view-group-button");
        var addActionButton = FindElementInRoot<Button>(instance, "add-action-button");
        var actionsCount = FindElementInRoot<Label>(instance, "actions-value-label");
        var actionGroupPath = FindElementInRoot<Label>(instance, "path-label");
        var deletGroupButton = FindElementInRoot<Button>(instance, "delete-group-button");
        var removeGroupButton = FindElementInRoot<Button>(instance, "remove-action-gruop-button");
        #endregion

        #region Synchronize Data
        groupName.value = group.name;
        string newName = "";
        string path = AssetDatabase.GetAssetPath(group);

        groupName.RegisterValueChangedCallback((evt) =>
        {
            group.name = evt.newValue;
            newName = $"{evt.newValue}.asset";
            SetEndColorToSaveButton();
        });
        groupName.RegisterCallback<KeyDownEvent>((evt) =>
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                AssetDatabase.RenameAsset(path, newName);
                AssetDatabase.Refresh();
                SetEndColorToSaveButton();
            }
        });

        actionsCount.text = group.actions.Count.ToString();
        actionGroupPath.text = AssetDatabase.GetAssetPath(group);
        viewGroupButton.clickable.clicked += () => ViewActionGroup(ActionGroupStructureContainer, group, instance);
        addActionButton.clickable.clicked += () =>
        {
            AddActionToGroup(ActionGroupStructureContainer, group, instance);
            actionsCount.text = group.actions.Count.ToString();
            SetEndColorToSaveButton();
        };
        deletGroupButton.clickable.clicked += () =>
        {
            bool result = EditorUtility.DisplayDialog("Confirmation", "Are you sure you want to delete this asset?", "Yes", "No");
            RemoveActionGroup(group, container, instance);
            DeleteActionGroupAsset(group, result);
            SetEndColorToSaveButton();
        };
        removeGroupButton.clickable.clicked += () =>
        {
            RemoveActionGroup(group, container, instance);
            SetEndColorToSaveButton();
        };
        #endregion

        this.Repaint();
    }

    private void Search(ChangeEvent<string> evt)
    {
        string searchQuery = evt.newValue.ToLower();
        bool hasSearchQuery = !string.IsNullOrEmpty(searchQuery);

        SetElementDisplay(base_Root, "ActionsGroup-UpPanel-Element", hasSearchQuery ? DisplayStyle.None : DisplayStyle.Flex);

        // B�squeda y actualizaci�n de la visualizaci�n de los elementos
        foreach (var element in m_ActionGroupsTemplates)
        {
            var groupName = element.Q<TextField>("gruop-name");
            if (element != null)
                element.style.display = groupName.value.ToLower().Contains(searchQuery) ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
    private void CreateActionGroup(VisualElement container)
    {
        var instance = ActionGroupElement.CloneTree();
        m_ActionGroupsTemplates.Add(instance);
        ActionsGroup newActionGroup = ScriptableObject.CreateInstance(typeof(ActionsGroup)) as ActionsGroup;
        string path = $"{settingsData.actionGroupsPath}/NewActionsGroup.asset";
        AssetDatabase.CreateAsset(newActionGroup, path);
        AssetDatabase.Refresh();
        actionsData.actionGroups.Add(newActionGroup);
        container.Add(instance);

        #region Fields
        var groupsAmmonut = FindElementInRoot<Label>("groups-ammount-label");
        var groupName = FindElementInRoot<TextField>(instance, "gruop-name");
        var actionsCount = FindElementInRoot<Label>(instance, "actions-value-label");
        var actionGroupPath = FindElementInRoot<Label>(instance, "path-label");
        var viewGroupButton = FindElementInRoot<Button>(instance, "view-group-button");
        var addActionButton = FindElementInRoot<Button>(instance, "add-action-button");
        var deletGroupButton = FindElementInRoot<Button>(instance, "delete-group-button");
        var removeGroupButton = FindElementInRoot<Button>(instance, "remove-action-gruop-button");
        #endregion

        #region Callbacks
        groupsAmmonut.text = actionsData.actionGroups.Count.ToString();

        string newName = "";
        groupName.RegisterValueChangedCallback((evt) =>
        {
            newActionGroup.name = evt.newValue;
            newName = $"{evt.newValue}.asset";
            SetEndColorToSaveButton();
        });
        groupName.RegisterCallback<KeyDownEvent>((evt) =>
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                AssetDatabase.RenameAsset(path, newName);
                AssetDatabase.Refresh();
            }
        });

        actionsCount.text = newActionGroup.actions.Count.ToString();
        actionGroupPath.text = AssetDatabase.GetAssetPath(newActionGroup);

        viewGroupButton.clickable.clicked += () => ViewActionGroup(ActionGroupStructureContainer, newActionGroup, instance);
        addActionButton.clickable.clicked += () =>
        {
            AddActionToGroup(ActionGroupStructureContainer, newActionGroup, instance);
            actionsCount.text = newActionGroup.actions.Count.ToString();
            SetEndColorToSaveButton();
        };
        deletGroupButton.clickable.clicked += () =>
        {
            bool result = EditorUtility.DisplayDialog("Confirmation", "Are you sure you want to delete this asset?", "Yes", "No");
            RemoveActionGroup(newActionGroup, container, instance);
            DeleteActionGroupAsset(newActionGroup, result);
            actionsCount.text = newActionGroup.actions.Count.ToString();
            SetEndColorToSaveButton();
        };
        removeGroupButton.clickable.clicked += () =>
        {
            RemoveActionGroup(newActionGroup, container, instance);
            actionsCount.text = newActionGroup.actions.Count.ToString();
            SetEndColorToSaveButton();
        };
        #endregion

        this.Repaint();
    }
    private void AddExistentGroup(ActionsGroup existenGroup, VisualElement groupsContainer)
    {
        if (existenGroup != null)
        {
            string path = AssetDatabase.GetAssetPath(existenGroup);
            var newExistentGroup = AssetDatabase.LoadAssetAtPath<ActionsGroup>(path);
            actionsData.actionGroups.Add(newExistentGroup);
            InstanceInfoOfGroup(newExistentGroup, groupsContainer);
            this.Repaint();
        }
    }

    private void ViewActionGroup(VisualElement container, ActionsGroup actionGroup, VisualElement groupElementInstance)
    {
        container.Clear();

        foreach (var actionStructure in actionGroup.actions)
        {
            SetActionStructureInfo(actionStructure, actionGroup, container, groupElementInstance);           
        }
        this.Repaint();
    }
    private void AddActionToGroup(VisualElement container, ActionsGroup actionGroup, VisualElement groupElementInstance)
    {
        var newActionStructure = new ActionStructure("NewAction");
        actionGroup.actions.Add(newActionStructure);
        SetActionStructureInfo(newActionStructure, actionGroup, container, groupElementInstance);
        this.Repaint();
    }
    private void SetActionStructureInfo(ActionStructure actionStructure, ActionsGroup actionGroup, VisualElement container, VisualElement groupElementInstance)
    {
        var instance = ActionElement.CloneTree();
        container.Add(instance);

        #region General
        var actionNameLabel = FindElementInRoot<Label>(instance, "action-name");
        var selftButton = FindElementInRoot<Button>(instance, "foldout-element");
        var removeActionButton = FindElementInRoot<Button>(instance, "remove-action-button");
        var actionContentContainer = FindElementInRoot<VisualElement>(instance, "foldout-contenct");
        var actionsCount = FindElementInRoot<Label>(groupElementInstance, "actions-value-label");

        actionNameLabel.text = String.IsNullOrEmpty(actionStructure.actionName) ? actionStructure.actionName : "NewAction";
        actionContentContainer.style.display = DisplayStyle.None;
        selftButton.clickable.clicked += () => SetElementDisplay(actionContentContainer);
        removeActionButton.clickable.clicked += () =>
        {
            RemoveActionInGroup(instance, container, actionGroup, actionStructure);
            actionsCount.text = actionGroup.actions.Count.ToString();
            SetEndColorToSaveButton();
        };
        #endregion

        #region General Settings
        var actionNameField = FindElementInRoot<TextField>(instance, "action-name-field");
        var actionTagSelectorButton = FindElementInRoot<Button>(instance, "action-tag-selector-button");
        var actionTagSelectorLabel = FindElementInRoot<Label>(instance, "action-tag");
        var globalActionAsset = FindElementInRoot<ObjectField>(instance, "global-action-asset");
        var editGlobalAssetButton = FindElementInRoot<Button>(instance, "edit-action-asset-button");

        actionNameField.value = actionStructure.actionName;
        actionNameField.RegisterValueChangedCallback((evt) =>
        {
            actionStructure.actionName = evt.newValue;
            actionNameLabel.text = evt.newValue;
            SetEndColorToSaveButton();
        });

        actionTagSelectorLabel.text = actionStructure.actionTag.tag;
        actionTagSelectorButton.clickable.clicked += () => OpenTagSelectorWindow(actionTagSelectorLabel, actionStructure.actionTag, "tag");

        globalActionAsset.value = actionStructure.globalAction;
        globalActionAsset.RegisterValueChangedCallback((evt) =>
        {
            actionStructure.globalAction = (BaseAction)evt.newValue;
            SetEndColorToSaveButton();
        });
        AddObjectFieldManipulators(globalActionAsset);

        editGlobalAssetButton.clickable.clicked += () =>
        {
            var actionName = globalActionAsset.value.name;
            EditAssetToActionStructure(actionName, actionStructure.globalAction);
            SetEndColorToSaveButton();
        };
        #endregion

        #region Cost
        var costTagSelectorButton = FindElementInRoot<Button>(instance,"character-stat-selector-button");
        var costTagSelectorLabel = FindElementInRoot<Label>(instance, "character-stat-tag");
        var costValueFloatField = FindElementInRoot<FloatField>(instance, "cost-value");

        costTagSelectorLabel.text = actionStructure.actionCost.statType.tag;
        costTagSelectorButton.clickable.clicked += () => OpenTagSelectorWindow(costTagSelectorLabel, actionStructure.actionCost.statType, "tag");

        costValueFloatField.value = actionStructure.actionCost.value;
        costValueFloatField.RegisterValueChangedCallback((evt) =>
        {
            actionStructure.actionCost.value = evt.newValue;
            SetEndColorToSaveButton();
        });
        #endregion

        #region Action Assets and Motion Warp
        var actionAssetsContainer = FindElementInRoot<VisualElement>(instance, "action-assets-container");
        var actionAssetsScrollView = FindElementInRoot<ScrollView>(instance, "action-assets-scrollview");
        var addActionAssetButton = FindElementInRoot<Button>(instance, "add-action-asset-button");

        if (actionStructure.enableActionsForEachWeapon && actionStructure.actions.Count > 0)
        {
            foreach (var actionAsset in actionStructure.actions)
            {
                var newElement = ActionAssetElement.CloneTree();
                actionAssetsScrollView.Add(newElement);
                actionAsset.Index = actionStructure.actions.IndexOf(actionAsset);

                var itemNameField = FindElementInRoot<TextField>(newElement, "item-name-field");
                var assetField = FindElementInRoot<ObjectField>(newElement, "action-asset-field");
                var editAssetButton = FindElementInRoot<Button>(newElement, "edit-action-asset-button");
                var removeButton = FindElementInRoot<Button>(newElement, "remove-action-asset-button");

                itemNameField.value = actionAsset.itemName;
                itemNameField.RegisterValueChangedCallback((evt) =>
                {
                    actionAsset.itemName = evt.newValue;
                    SetEndColorToSaveButton();
                });

                assetField.value = actionAsset.action != null ? actionAsset.action : null;
                assetField.RegisterValueChangedCallback((evt) =>
                {
                    actionAsset.action = (BaseAction)evt.newValue;
                    SetEndColorToSaveButton();
                });

                editAssetButton.name = actionStructure.actions.IndexOf(actionAsset).ToString();
                editAssetButton.clickable.clicked += () =>
                {
                    var actionName = assetField.value.name;
                    EditAssetToActionStructure(actionName, actionAsset.action);
                    SetEndColorToSaveButton();
                };

                removeButton.name = actionStructure.actions.IndexOf(actionAsset).ToString();
                removeButton.clickable.clicked += () =>
                {
                    RemoveActionAssetToActionStructure(removeButton.name, actionAssetsScrollView, actionStructure, newElement);
                    SetEndColorToSaveButton();
                };
            }
        }

        addActionAssetButton.clickable.clicked += () =>
        {
            AddActionAssetToActionStructure(actionAssetsScrollView, actionStructure);
            SetEndColorToSaveButton();
        };

        var motionWarpContainer = FindElementInRoot<VisualElement>(instance, "motion-warp-container");
        var minRange = FindElementInRoot<FloatField>(instance, "min-range-float-field");
        var maxRange = FindElementInRoot<FloatField>(instance, "max-range-float-field");
        var speed = FindElementInRoot<FloatField>(instance, "speed-float-field");
        var stopDistance = FindElementInRoot<FloatField>(instance, "stop-distance-float-field");

        minRange.value = actionStructure.useMotionWarp ? actionStructure.motionWarp.minRange : 0;
        minRange.RegisterValueChangedCallback((evt) =>
        {
            actionStructure.motionWarp.minRange = evt.newValue;
            SetEndColorToSaveButton();
        });

        maxRange.value = actionStructure.useMotionWarp ? actionStructure.motionWarp.maxRange : 0;
        maxRange.RegisterValueChangedCallback((evt) =>
        {
            actionStructure.motionWarp.maxRange = evt.newValue;
            SetEndColorToSaveButton();
        });

        speed.value = actionStructure.useMotionWarp ? actionStructure.motionWarp.speed : 0;
        speed.RegisterValueChangedCallback((evt) =>
        {
            actionStructure.motionWarp.speed = evt.newValue;
            SetEndColorToSaveButton();
        });

        stopDistance.value = actionStructure.useMotionWarp ? actionStructure.motionWarp.stopDistance : 0;
        stopDistance.RegisterValueChangedCallback((evt) =>
        {
            actionStructure.motionWarp.stopDistance = evt.newValue;
            SetEndColorToSaveButton();
        });
        #endregion

        #region Special Settings
        var forceFullBodyOnlyButton = FindElementInRoot<Button>(instance, "force-full-body-only-button");
        var enableActionsForEachWeaponButton = FindElementInRoot<Button>(instance, "actions-for-each-weapon-button");
        var useWarpMotionButton = FindElementInRoot<Button>(instance, "use-warp-motion-button");

        SetSwitch(actionStructure.forceFullBodyOnly, forceFullBodyOnlyButton);
        forceFullBodyOnlyButton.clickable.clicked += () =>
        {
            actionStructure.forceFullBodyOnly = !actionStructure.forceFullBodyOnly;
            SetSwitch(actionStructure.forceFullBodyOnly, forceFullBodyOnlyButton);
            SetEndColorToSaveButton();
        };

        SetSwitch(actionStructure.enableActionsForEachWeapon, enableActionsForEachWeaponButton, actionAssetsContainer);
        enableActionsForEachWeaponButton.clickable.clicked += () =>
        {
            actionStructure.enableActionsForEachWeapon = !actionStructure.enableActionsForEachWeapon;
            SetSwitch(actionStructure.enableActionsForEachWeapon, enableActionsForEachWeaponButton);
            SetElementDisplay(actionAssetsContainer, actionStructure.enableActionsForEachWeapon);
            SetEndColorToSaveButton();
        };

        SetSwitch(actionStructure.useMotionWarp, useWarpMotionButton, motionWarpContainer);
        useWarpMotionButton.clickable.clicked += () =>
        {
            actionStructure.useMotionWarp = !actionStructure.useMotionWarp;
            SetSwitch(actionStructure.useMotionWarp, useWarpMotionButton);
            SetElementDisplay(motionWarpContainer, actionStructure.useMotionWarp);
            SetEndColorToSaveButton();
        };
        #endregion

        #region Animation Config
        var motionClip = FindElementInRoot<ObjectField>(instance, "motion-animation-clip");
        var principalOverrideClip = FindElementInRoot<ObjectField>(instance, "principal-override-animation-clip");
        var animatorStatename = FindElementInRoot<TextField>(instance, "animator-state-name");
        var animatorLayerMask = FindElementInRoot<TextField>(instance, "layer-mask");

        motionClip.value = actionStructure.motion;
        motionClip.RegisterValueChangedCallback((evt) =>
        {
            actionStructure.motion = (AnimationClip)evt.newValue;
            SetEndColorToSaveButton();
        });
        AddObjectFieldManipulators(motionClip);

        principalOverrideClip.value = actionStructure.overrideClip;
        principalOverrideClip.RegisterValueChangedCallback((evt) =>
        {
            actionStructure.overrideClip = (AnimationClip)evt.newValue;
            SetEndColorToSaveButton();
        });
        AddObjectFieldManipulators(principalOverrideClip);

        animatorStatename.value = actionStructure.animStateName;
        animatorStatename.RegisterValueChangedCallback((evt) =>
        {
            actionStructure.animStateName = evt.newValue;
            SetEndColorToSaveButton();
        });

        animatorLayerMask.value = actionStructure.layerMask;
        animatorLayerMask.RegisterValueChangedCallback((evt) =>
        {
            actionStructure.layerMask = evt.newValue;
            SetEndColorToSaveButton();
        });
        #endregion

        #region Attribute Modifiers
        var addAttributeModifierButton = FindElementInRoot<Button>(instance, "add-stat-modifiers-button");
        var statModifiersContainer = FindElementInRoot<ScrollView>(instance, "stat-modifiers-scrollview");

        addAttributeModifierButton.clickable.clicked += () =>
        {
            AddStatModifierOnAction(actionStructure, statModifiersContainer);
            SetEndColorToSaveButton();
        };

        if (actionStructure.modifiers.Count > 0)
        {
            for (int i = 0; i < actionStructure.modifiers.Count; i++)
            {
                AddStatModifierOnAction(actionStructure, statModifiersContainer, false, actionStructure.modifiers[i]);
            }
        }
        #endregion

        this.Repaint();
    }
    private void RemoveActionInGroup(TemplateContainer instance, VisualElement container, ActionsGroup actionGroup, ActionStructure actionStructure)
    {
        container.Remove(instance);
        actionGroup.actions.Remove(actionStructure);
        this.Repaint();
    }
    private void DeleteActionGroupAsset(ActionsGroup actionGroup, bool result)
    {
        if (result)
        {
            var path = AssetDatabase.GetAssetPath(actionGroup);
            AssetDatabase.DeleteAsset(path);
            Debug.Log("Action Group Deleted");
        }
        else Debug.Log("Delete Canceled");
    }
    private void RemoveActionGroup(ActionsGroup group, VisualElement container, TemplateContainer instance)
    {
        container.Remove(instance);
        m_ActionGroupsTemplates.Remove(instance);
        actionsData.actionGroups.Remove(group);

        var groupsAmmonut = FindElementInRoot<Label>("groups-ammount-label");
        groupsAmmonut.text = actionsData.actionGroups.Count.ToString();

        this.Repaint();
    }

    private void AddStatModifierOnAction(ActionStructure actionStructure, ScrollView statModifiersView, bool addAsNew = true, ActionStatisticsModifier modifier = null)
    {
        ActionStatisticsModifier newModifier = null;

        if (addAsNew)
        {
            newModifier = new();
            actionStructure.modifiers.Add(newModifier);
            newModifier.Index = actionStructure.modifiers.IndexOf(newModifier);
        }
        else newModifier = modifier;

        var instance = ActionStatModifierElement.CloneTree();
        statModifiersView.Add(instance);

        var tagSelegtorButton = FindElementInRoot<Button>(instance, "tag-selector-button");
        var statModifierTag = FindElementInRoot<Label>(instance, "stat-modifier-tag");
        var removeStatModButton = FindElementInRoot<Button>(instance, "remove-stat-modifier-button");
        var statModOperationEnum = FindElementInRoot<EnumField>(instance, "stat-modifier-operation-enum");
        var statModValue = FindElementInRoot<FloatField>(instance, "stat-modifier-value-field");
        var statModValueTypeEnum = FindElementInRoot<EnumField>(instance, "stat-modifier-value-type-field");

        tagSelegtorButton.clickable.clicked += () => OpenTagSelectorWindow(statModifierTag, newModifier, "tag");
        statModifierTag.text = !String.IsNullOrEmpty(newModifier.tag) ? newModifier.tag : "None";

        removeStatModButton.clickable.clicked += () =>
        {
            RemoveStatModifierOnAction(actionStructure, newModifier, statModifiersView, instance);
            SetEndColorToSaveButton();
        };

        statModOperationEnum.value = newModifier.opType;
        statModOperationEnum.RegisterValueChangedCallback((evt) =>
        {
            newModifier.opType = (OperationType)evt.newValue;
            SetEndColorToSaveButton();
        });

        statModValue.value = newModifier.value;
        statModValue.RegisterValueChangedCallback((evt) =>
        {
            newModifier.value = evt.newValue;
            SetEndColorToSaveButton();
        });

        statModValueTypeEnum.value = newModifier.valueType;
        statModValueTypeEnum.RegisterValueChangedCallback((evt) =>
        {
            newModifier.valueType = (UltimateFramework.Utils.ValueType)evt.newValue;
            SetEndColorToSaveButton();
        });

        this.Repaint();
    }
    private void RemoveStatModifierOnAction(ActionStructure actionStructure, ActionStatisticsModifier modifier, ScrollView statModifiersView, VisualElement modifierElementInstance)
    {
        actionStructure.modifiers.Remove(modifier);
        statModifiersView.Remove(modifierElementInstance);
        this.Repaint();
    }

    private void AddActionAssetToActionStructure(ScrollView actionAssetsContainer, ActionStructure actionStructure)
    {
        var newElement = ActionAssetElement.CloneTree();
        ActionsListStructure newActiionAsset = new();
        newActiionAsset.Index = actionStructure.actions.IndexOf(newActiionAsset);

        var itemNameField = FindElementInRoot<TextField>(newElement, "item-name-field");
        var assetField = FindElementInRoot<ObjectField>(newElement, "action-asset-field");
        var removeButton = FindElementInRoot<Button>(newElement, "remove-action-asset-button");

        itemNameField.value = "Item name";
        itemNameField.RegisterValueChangedCallback((evt) =>
        {
            newActiionAsset.itemName = evt.newValue;
            SetEndColorToSaveButton();
        });

        assetField.value = null;
        assetField.RegisterValueChangedCallback((evt) =>
        {
            newActiionAsset.action = (BaseAction)evt.newValue;
            SetEndColorToSaveButton();
        });

        removeButton.name = actionStructure.actions.IndexOf(newActiionAsset).ToString();
        removeButton.clickable.clicked += () =>
        {
            RemoveActionAssetToActionStructure(removeButton.name, actionAssetsContainer, actionStructure, newElement);
            SetEndColorToSaveButton();
        };

        actionStructure.actions.Add(newActiionAsset);
        actionAssetsContainer.Add(newElement);

        this.Repaint();
    }
    private void EditAssetToActionStructure(string actionName, BaseAction actionAsset)
    {
        ActionsInspectorViewWindow window = GetWindow<ActionsInspectorViewWindow>();
        Texture2D icon = Resources.Load<Texture2D>("Img/Action_Asset_Icon_v2.1");
        window.titleContent = new GUIContent(actionName, icon);
        window.OnEditActionAsset.Invoke(actionAsset);
        window.Show();
    }
    private void RemoveActionAssetToActionStructure(string buttonName, ScrollView scrollView, ActionStructure actionStructure, TemplateContainer actionAssetElement)
    {
        int id = int.Parse(buttonName);
        var actionAsset = actionStructure.actions.Find(i => i.Index == id);
        actionStructure.actions.Remove(actionAsset);
        scrollView.Remove(actionAssetElement);
        this.Repaint();
    }

    private T FindElementInRoot<T>(string name) where T : VisualElement
    {
        return base_Root.Q<T>(name);
    }
    private T FindElementInRoot<T>(VisualElement root, string name) where T : VisualElement
    {
        return root.Q<T>(name);
    }
    private void OpenTagSelectorWindow(Label tagLabel, object obj, string fieldName)
    {
        if (currentTagSelectorWindow != null) currentTagSelectorWindow.Close();

        TagsTreeViewWindow window = EditorWindow.GetWindow<TagsTreeViewWindow>();
        Texture2D icon = Resources.Load<Texture2D>("Img/Tag_Window_Icon");
        window.titleContent = new GUIContent("Game Tags", icon);
        currentTagSelectorWindow = window;

        window.m_TagsTreeView.OnTagSelected += tag =>
        {
            // Utiliza reflexi�n para asignar el valor al campo espec�fico
            if (obj != null && !string.IsNullOrEmpty(fieldName))
            {
                // A�ade BindingFlags si el campo no es p�blico
                var fieldInfo = obj.GetType().GetField(fieldName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (fieldInfo != null && fieldInfo.FieldType == typeof(string))
                {
                    // Aseg�rate de que est�s modificando el objeto original
                    fieldInfo.SetValue(obj, tag);
                    SetEndColorToSaveButton();
                }
            }

            tagLabel.text = tag;
        };
        window.Show();
    }
    private void SetSwitch(bool value, Button buton)
    {
        if (value == true)
        {
            buton.text = "ON";
            buton.RemoveFromClassList("SwitchOff");
        }
        else
        {
            buton.text = "OFF";
            buton.AddToClassList("SwitchOff");
        }
    }
    private void SetSwitch( bool value, Button buton, VisualElement container) 
    {
        if (value == true)
        {
            buton.text = "ON";
            buton.RemoveFromClassList("SwitchOff");
            SetElementDisplay(container, value);
        }
        else
        {
            buton.text = "OFF";
            buton.AddToClassList("SwitchOff");
            SetElementDisplay(container, value);
        }
    }
    private void SetElementDisplay(VisualElement container)
    {
        if (container.style.display == DisplayStyle.None)
        {
            container.style.display = DisplayStyle.Flex;
        }
        else
        {
            container.style.display = DisplayStyle.None;
        }
    }
    private void SetElementDisplay(VisualElement container, bool value)
    {
        if (value) container.style.display = DisplayStyle.Flex;
        else container.style.display = DisplayStyle.None;
    }
    private void SetElementDisplay(VisualElement root, string queryElement, DisplayStyle displayStyle)
    {
        root.Q<VisualElement>(queryElement).style.display = displayStyle;
    }
    private void AddObjectFieldManipulators(ObjectField objectField)
    {
        // Agrega un manejador para el evento de click
        objectField.RegisterCallback<ClickEvent>(evt =>
        {
            if (objectField.value != null)
            {
                // Abre el archivo en el Inspector
                EditorGUIUtility.PingObject(objectField.value);
            }
        });

        // Agrega un manejador para el evento de click derecho
        objectField.RegisterCallback<ContextClickEvent>(evt =>
        {
            // Crea un men� de contexto
            GenericMenu menu = new ();

            // Agrega una opci�n para copiar el valor
            menu.AddItem(new GUIContent("Copy"), false, () =>
            {
                // Copia el valor al portapapeles
                EditorGUIUtility.systemCopyBuffer = AssetDatabase.GetAssetPath(objectField.value);
            });

            // Agrega una opci�n para pegar el valor
            menu.AddItem(new GUIContent("Paste"), false, () =>
            {
                // Pega el valor desde el portapapeles
                objectField.value = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(EditorGUIUtility.systemCopyBuffer);
            });

            // Muestra el men� de contexto
            menu.ShowAsContext();
        });
    }
    private void SetStartColorToSaveButton()
    {
        saveButton.style.backgroundColor = startColor;
        saveButton.style.color = Color.white;
    }
    private void SetEndColorToSaveButton()
    {
        saveButton.style.backgroundColor = endColor;
        saveButton.style.color = Color.black;
    }

    private void SaveData()
    {
        actionsData.Save();
        SetStartColorToSaveButton();
    }
}
