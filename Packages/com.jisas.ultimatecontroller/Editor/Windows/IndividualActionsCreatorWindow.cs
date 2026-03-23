using UltimateFramework.ActionsSystem;
using UltimateFramework.Editor;
using UltimateFramework.Tools;
using UnityEngine.UIElements;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System;

public class IndividualActionsCreatorWindow : EditorWindow
{
    private VisualTreeAsset m_UXML;
    private VisualElement root;
    private SettingsMasterData settingsData;
    private string assetName;
    private string classeName;

    [MenuItem("Ultimate Framework/Create/Asset/Individual Action")]
    public static void ShowWindow()
    {
        GetWindow<IndividualActionsCreatorWindow>("Individual Actions Creator");
    }

    private void LoadResources()
    {
        settingsData = SettingsMasterData.Instance;
        m_UXML = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/IndividualActionsCreatorWindow");
    }

    private void CreateGUI()
    {
        LoadResources();
        root = rootVisualElement;
        m_UXML.CloneTree(root);

        #region Find Elements
        var contentcontainer = UFEditorUtils.FindElementInRoot<VisualElement>(root, "content-container");
        var actionNameField = UFEditorUtils.FindElementInRoot<TextField>(contentcontainer, "action-name");
        var createButton = UFEditorUtils.FindElementInRoot<Button>(root, "create-button");
        #endregion

        #region Create An Add Popup
        var baseType = typeof(BaseAction);
        var assembly = Assembly.GetAssembly(baseType);

        var subclases = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(baseType))
            .Select(t => t.Name)
            .ToArray();

        var popup = new PopupField<string>(subclases.ToList(), 0);
        popup.style.flexGrow = 1;

        contentcontainer.Add(popup);
        #endregion

        #region Value Asignament
        classeName = subclases[0];
        #endregion

        #region Register Callbacks
        actionNameField.RegisterValueChangedCallback(evt =>
        {
            assetName = evt.newValue;
        });

        popup.RegisterValueChangedCallback(evt =>
        {
            classeName = evt.newValue;
        });

        createButton.RegisterCallback<ClickEvent>(evt =>
        {
            CreateAsset(assetName, classeName);
        });
        #endregion
    }

    private void CreateAsset(string assetName, string className)
    {
        string newAssetName = !String.IsNullOrEmpty(assetName) ? assetName : $"New {className}";

        // Create a new instance of the ScriptableObject
        var assembly = Assembly.GetAssembly(typeof(BaseAction));
        var type = assembly.GetTypes().FirstOrDefault(t => t.Name == className);
        if (type == null)
        {
            Debug.LogError($"No class was found with the name {className}.");
            return;
        }

        var newObject = ScriptableObject.CreateInstance(type);
        if (newObject == null)
        {
            Debug.LogError($"Failed to create an instance of the class {className}.");
            return;
        }

        // Create a new asset in the project
        string path = settingsData.actionIndividualsPath;
        path = AssetDatabase.GenerateUniqueAssetPath($"{path}/{newAssetName}.asset");
        AssetDatabase.CreateAsset(newObject, path);
        AssetDatabase.SaveAssets();

        Debug.Log($"A new asset of class: {className}, with the name: {newAssetName}, was created in: {path}.");
    }
}
