using UltimateFramework.ItemSystem;
using UltimateFramework.Utils;
using UltimateFramework.Tools;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine;

public class SettingsMasterWindow : EditorWindow
{
    private VisualTreeAsset m_UXML;
    private TemplateContainer base_Root;
    private SettingsMasterData settingsData;
    private string jsonString;

    VisualElement tooltipSection;
    Button helpingMessageButton;
    Button diferentPathsButton;

    [MenuItem("Ultimate Framework/Windows/Settings Master")]
    public static void ShowMyEditor()
    {
        EditorWindow wnd = GetWindow<SettingsMasterWindow>();
        Texture2D icon = Resources.Load<Texture2D>("Img/SettingsMaster_Icon");
        wnd.titleContent = new GUIContent("Settings Master", icon);
    }
    private void OnEnable()
    {
        settingsData = SettingsMasterData.Instance;
        LoadData();
    }
    private void CreateGUI()
    {
        m_UXML = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/SettingsMasterWindow");
        base_Root = m_UXML.CloneTree();
        rootVisualElement.Add(base_Root);

        var actionsButton = base_Root.Q<ToolbarButton>("ActionsConfigButton");
        var itemsButton = base_Root.Q<ToolbarButton>("ItemsConfigButton");
        var statesButton = base_Root.Q<ToolbarButton>("StatesConfigButton");
        var preferencesButton = base_Root.Q<ToolbarButton>("PreferencesButton");

        var Panels = new VisualElement[]
        {
            base_Root.Q<VisualElement>("WelcomeMessage"),
            base_Root.Q<VisualElement>("ActionsContentElement"),
            base_Root.Q<VisualElement>("ItemsContentElement"),
            base_Root.Q<VisualElement>("StatesContentElement"),
            base_Root.Q<VisualElement>("PreferencesContentElement")
        };

        actionsButton.clickable.clicked += () => DisplaySection(Panels, Panels[1].name);
        itemsButton.clickable.clicked += () => DisplaySection(Panels, Panels[2].name);
        statesButton.clickable.clicked += () => DisplaySection(Panels, Panels[3].name);
        preferencesButton.clickable.clicked += () => DisplaySection(Panels, Panels[4].name);
    }

    private void DisplaySection(VisualElement[] panels, string sectionName)
    {
        foreach (var panel in panels)
        {
            if (panel.name == sectionName)
            {
                panel.style.display = DisplayStyle.Flex;

                var saveButton = panel.Q<Button>("SaveDataButton");
                saveButton.clickable.clicked += () => SaveData();

                SynchronizeData(panel, sectionName);
            }
            else panel.style.display = DisplayStyle.None;
        }
    }
    private void SynchronizeData(VisualElement panel, string sectionName)
    {
        #region Fields
        //Globals
        tooltipSection = panel.Q<VisualElement>("TooltipSection");

        // Actions
        ObjectField actionsMaster = panel.Q<ObjectField>("actions-data-master-object-field");
        EnumField actionGroupsPathAs = panel.Q<EnumField>("action-groups-pathas-enum");
        TextField actionGroupsPath = panel.Q<TextField>("action-groups-path-field");
        EnumField actionIndividualsPathAs = panel.Q<EnumField>("action-individuals-pathas-enum");
        TextField actionIndividualsPath = panel.Q<TextField>("action-individuals-path-field");
        EnumField actionsPathAs = panel.Q<EnumField>("ActionsPathAsEnum");
        TextField actionsPath = panel.Q<TextField>("ActionsPathField");

        // Items
        ObjectField itemsDB = panel.Q<ObjectField>("ItemDBField");

        // States
        EnumField statesPathAs = panel.Q<EnumField>("StatesPathAsEnum");
        TextField statesPath = panel.Q<TextField>("StatesPathField");
        TextField armorsPath = panel.Q<TextField>("ArmorsPathField");
        TextField weaponsPath = panel.Q<TextField>("WeaponPathField");
        TextField consumablePath = panel.Q<TextField>("ConsumablesPathField");

        // Preferences
        helpingMessageButton = panel.Q<Button>("HelpingMessageButton");
        diferentPathsButton = panel.Q<Button>("DiferentPathsButton");
        #endregion

        switch (sectionName)
        {
            case "ActionsContentElement":

                actionsMaster.value = settingsData.actionsMasterData;

                actionGroupsPathAs.value = settingsData.actionGroupsPathSelection;
                actionGroupsPath.value = settingsData.actionGroupsPath;

                actionIndividualsPathAs.value = settingsData.actionIndividualsPathSelection;
                actionIndividualsPath.value = settingsData.actionIndividualsPath;

                actionsPathAs.value = settingsData.actionsPathSelection;
                actionsPath.value = settingsData.actionsScriptsPath;

                actionsMaster.RegisterValueChangedCallback((evt) =>
                {
                    settingsData.actionsMasterData = (ActionsMasterData)evt.newValue;
                });

                actionGroupsPathAs.RegisterValueChangedCallback((evt) =>
                {
                    settingsData.actionGroupsPathSelection = (PathUseAs)evt.newValue;
                });

                actionGroupsPath.RegisterValueChangedCallback((evt) =>
                {
                    settingsData.actionGroupsPath = evt.newValue;
                });

                actionsPathAs.RegisterValueChangedCallback((evt) =>
                {
                    settingsData.actionsPathSelection = (PathUseAs)evt.newValue;
                });

                actionIndividualsPath.RegisterValueChangedCallback((evt) =>
                {
                    settingsData.actionIndividualsPath = evt.newValue;
                });

                actionIndividualsPathAs.RegisterValueChangedCallback((evt) =>
                {
                    settingsData.actionIndividualsPathSelection = (PathUseAs)evt.newValue;
                });

                actionsPath.RegisterValueChangedCallback((evt) =>
                {
                    settingsData.actionsScriptsPath = evt.newValue;
                });

                SetHelpingMessageDisplay();
                break;

            case "ItemsContentElement":
                itemsDB.value = settingsData.itemDB;

                itemsDB.RegisterValueChangedCallback((evt) =>
                {
                    settingsData.itemDB = (ItemDatabase)evt.newValue;
                });

                SetHelpingMessageDisplay();
                break;

            case "StatesContentElement":
                statesPathAs.value = settingsData.statesPathSelection;

                statesPathAs.RegisterValueChangedCallback((evt) =>
                {
                    settingsData.statesPathSelection = (PathUseAs)evt.newValue;
                });

                if (!settingsData.useDiferentPaths)
                {
                    statesPath.style.display = DisplayStyle.Flex;
                    armorsPath.style.display = DisplayStyle.None;
                    weaponsPath.style.display = DisplayStyle.None;
                    consumablePath.style.display = DisplayStyle.None;

                    statesPath.value = settingsData.generalStatesScriptsPath;

                    statesPath.RegisterValueChangedCallback((evt) =>
                    {
                        settingsData.generalStatesScriptsPath = evt.newValue;
                    });
                }
                else
                {
                    statesPath.style.display = DisplayStyle.None;
                    armorsPath.style.display = DisplayStyle.Flex;
                    weaponsPath.style.display = DisplayStyle.Flex;
                    consumablePath.style.display = DisplayStyle.Flex;

                    armorsPath.value = settingsData.armorStateScriptsPath;
                    weaponsPath.value = settingsData.weaponStateScriptsPath;
                    consumablePath.value = settingsData.consumableStateScriptsPath;

                    statesPathAs.RegisterValueChangedCallback((evt) =>
                    {
                        settingsData.statesPathSelection = (PathUseAs)evt.newValue;
                    });

                    armorsPath.RegisterValueChangedCallback((evt) =>
                    {
                        settingsData.armorStateScriptsPath = evt.newValue;
                    });

                    weaponsPath.RegisterValueChangedCallback((evt) =>
                    {
                        settingsData.weaponStateScriptsPath = evt.newValue;
                    });

                    consumablePath.RegisterValueChangedCallback((evt) =>
                    {
                        settingsData.consumableStateScriptsPath = evt.newValue;
                    });
                }

                SetHelpingMessageDisplay();
                break;

            case "PreferencesContentElement":
                helpingMessageButton.clickable.clicked -= HelpingMessageButtonClicked;
                diferentPathsButton.clickable.clicked -= DiferentPathsButtonClicked;

                // Registrar los nuevos controladores de eventos
                helpingMessageButton.clickable.clicked += HelpingMessageButtonClicked;
                diferentPathsButton.clickable.clicked += DiferentPathsButtonClicked;

                SetPreferences();
                break;
        }
    }
    void HelpingMessageButtonClicked()
    {
        settingsData.showHelpingMessage = !settingsData.showHelpingMessage;
        SetPreferences();
    }
    void DiferentPathsButtonClicked()
    {
        settingsData.useDiferentPaths = !settingsData.useDiferentPaths;
        SetPreferences();
    }
    private void SetPreferences()
    {
        if (settingsData.showHelpingMessage)
        {
            helpingMessageButton.text = "ON";
            helpingMessageButton.RemoveFromClassList("SwitchOff");
        }
        else
        {
            helpingMessageButton.text = "OFF";
            helpingMessageButton.AddToClassList("SwitchOff");
        }

        if (settingsData.useDiferentPaths)
        {
            diferentPathsButton.text = "ON";
            diferentPathsButton.RemoveFromClassList("SwitchOff");
        }
        else
        {
            diferentPathsButton.text = "OFF";
            diferentPathsButton.AddToClassList("SwitchOff");
        }
    }
    private void SetHelpingMessageDisplay()
    {
        if (settingsData.showHelpingMessage)
        {
            tooltipSection.style.display = DisplayStyle.Flex;
        }
        else tooltipSection.style.display = DisplayStyle.None;
    }

    private void LoadData()
    {
        if (PlayerPrefs.HasKey("ActionsData"))
            LoadSaveData();
        else
            LoadDefaultData();
    }
    private void LoadDefaultData()
    {
        // Actions
        settingsData.actionsMasterData = Resources.Load<ActionsMasterData>("Data/Actions/MasterData/ActionsMasterData");

        settingsData.actionGroupsPathSelection = PathUseAs.Default;
        settingsData.actionGroupsPath = "Assets/UltimateFramework/Resources/Data/Actions/Groups";

        settingsData.actionIndividualsPath = "Assets/UltimateFramework/Resources/Data/Actions/Individual";
        settingsData.actionIndividualsPathSelection = PathUseAs.Default;

        settingsData.actionsScriptsPath = "Assets/UltimateFramework/Systems/ActionsSystem/Actions";
        settingsData.actionsPathSelection = PathUseAs.Default;

        // Items
        settingsData.itemDB = Resources.Load<ItemDatabase>("Data/ItemDB/ItemsDatabase");

        // States
        settingsData.statesPathSelection = PathUseAs.Default;
        settingsData.generalStatesScriptsPath = "Assets/UltimateFramework/FullExample/Assets/States";
        settingsData.armorStateScriptsPath = "Assets/UltimateFramework/FullExample/Assets/States/Armors";
        settingsData.weaponStateScriptsPath = "Assets/UltimateFramework/FullExample/Assets/States/Weapons";
        settingsData.consumableStateScriptsPath = "Assets/UltimateFramework/FullExample/Assets/States/Consumables";

        // Preferences
        settingsData.showHelpingMessage = true;
        settingsData.useDiferentPaths = true;
    }
    private void LoadSaveData()
    {
        jsonString = PlayerPrefs.GetString("ActionsData");
        DataWrapper dataWrapper = JsonUtility.FromJson<DataWrapper>(jsonString);
        settingsData.actionsMasterData = Resources.Load<ActionsMasterData>("Data/Actions/MasterData/ActionsMasterData");

        settingsData.actionGroupsPathSelection = dataWrapper.ActionGroupPathSelection;
        settingsData.actionGroupsPath = dataWrapper.ActionGroupPath;

        settingsData.actionIndividualsPathSelection = dataWrapper.actionIndividualsPathSelection;
        settingsData.actionIndividualsPath = dataWrapper.actionIndividualsPath;

        settingsData.actionsPathSelection = dataWrapper.ActionsPathSelection;
        settingsData.actionsScriptsPath = dataWrapper.ActionsScriptsPath;
    }
    private void SaveData()
    {
        DataWrapper dataWrapper = new()
        {
            ActionGroupPathSelection = settingsData.actionGroupsPathSelection,
            ActionGroupPath = settingsData.actionGroupsPath,

            actionIndividualsPathSelection = settingsData.actionIndividualsPathSelection,
            actionIndividualsPath = settingsData.actionIndividualsPath,

            ActionsPathSelection = settingsData.actionsPathSelection,
            ActionsScriptsPath = settingsData.actionsScriptsPath,
        };

        jsonString = JsonUtility.ToJson(dataWrapper);
        PlayerPrefs.SetString("ActionsData", jsonString);

        settingsData.SaveData();
    }
    private struct DataWrapper
    {
        public PathUseAs ActionGroupPathSelection;
        public string ActionGroupPath;

        public PathUseAs actionIndividualsPathSelection;
        public string actionIndividualsPath;

        public PathUseAs ActionsPathSelection;
        public string ActionsScriptsPath;
    }
}
