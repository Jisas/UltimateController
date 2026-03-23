using UltimateFramework.Utils;
using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEngine.UIElements;
using UltimateFramework.Editor;

namespace UltimateFramework.Tools
{   
    public class StatesCreatorWindow : EditorWindow
    {
        private VisualTreeAsset m_UXML;
        private VisualElement root;
        private CreatorToolScriptsData newScriptData;
        private const string actionSuffix = "State";
        private StateType stateType;

        [MenuItem("Ultimate Framework/Create/Script/State")]
        [MenuItem("Assets/Create/Ultimate Framework/Systems/Items/State Script")]
        public static void ShowWindow()
        {
            GetWindow<StatesCreatorWindow>("Script States Creator");
        }

        private void LoadResources()
        {
            m_UXML = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/ScriptStatesCreatorWindow");
        }
        private void OnEnable()
        {
            newScriptData = CreateInstance<CreatorToolScriptsData>();
            newScriptData.scriptContent = GenerateContent();
        }
        private void CreateGUI()
        {
            LoadResources();
            root = rootVisualElement;
            m_UXML.CloneTree(root);

            #region Find Elements
            var contentcontainer = UFEditorUtils.FindElementInRoot<VisualElement>(root, "content-container");
            var classNameField = UFEditorUtils.FindElementInRoot<TextField>(contentcontainer, "class-name");
            var stateTypeField = UFEditorUtils.FindElementInRoot<EnumField>(contentcontainer, "type");
            var createButton = UFEditorUtils.FindElementInRoot<Button>(root, "create-button");
            #endregion

            #region Register Callbacks
            classNameField.RegisterValueChangedCallback(evt =>
            {
                newScriptData.scriptName = evt.newValue;
            });

            stateTypeField.RegisterValueChangedCallback(evt =>
            {
                stateType = (StateType)evt.newValue;
            });

            createButton.RegisterCallback<ClickEvent>(evt =>
            {
                CreateStateScript();
            });
            #endregion
        }

        private string GenerateContent()
        {
            string tab = "\t"; // Tabulation

            string[] armorLines =
            {
                "using UltimateFramework.Utils;",
                "using UltimateFramework.ItemSystem;",
                "using UnityEngine;",
                "",
                $"[CreateAssetMenu(menuName = \"Ultimate Framework/Systems/Items/State Asset/Armor State/#SCRIPTNAME#\")]",
                $"public class #SCRIPTNAME# : ArmorState",
                "{",
                $"{tab}// Function that is called once at the start of the state.",
                $"{tab}public override void StateStart(ArmorBehaviour machine)",
                $"{tab}" + "{",
                $"{tab}{tab}this.stateFlow = StateFlow.Start; // controls the flow of the state",
                $"{tab}{tab}",
                $"{tab}{tab}// Adds transitions to the state so that it can go to other states based on conditions",
                $"{tab}{tab}this.AddTransition(new Transition(",
                $"{tab}{tab}{tab}() => GenericCondition(machine),",
                $"{tab}{tab}{tab}() => GenericTransition(machine),",
                $"{tab}{tab}{tab}machine.FindState(\"OtherStateName\"))); // Change \"OtherStateName\" to the name of the state you want to transition to",
                $"{tab}" + "}",
                $"{tab}",
                $"{tab}// Function called multiple times per frame",
                $"{tab}public override void StateUpdate(ArmorBehaviour machine)",
                $"{tab}" + "{",
                $"{tab}{tab}this.stateFlow = StateFlow.Running; // controls the flow of the state",
                $"{tab}{tab}",
                $"{tab}{tab}// If the list of transitions is not empty, it constantly checks if any of the conditions are met to execute the transition.",
                $"{tab}{tab}if (this.Transitions.Count > 0)",
                $"{tab}{tab}{tab}machine.ProcessEvent(this);",
                $"{tab}" + "}",
                $"{tab}",
                $"{tab}",
                $"{tab}// Conditional functions for transitions, one should be created for each transition if necessary.",
                $"{tab}#region Conditions",
                $"{tab}private bool GenericCondition(ArmorBehaviour machine)",
                $"{tab}" + "{",
                $"{tab}{tab}return true; // Write your own logic",
                $"{tab}" + "}",
                $"{tab}#endregion",
                $"{tab}",
                $"{tab}",
                $"{tab}// Functions that will be executed before the next state when transitioning",
                $"{tab}#region Actions",
                $"{tab}private void GenericTransition(ArmorBehaviour machine)",
                $"{tab}" + "{",
                $"{tab}{tab}this.stateFlow = StateFlow.Finished; // controls the flow of the state",
                $"{tab}{tab}machine.SetLastState(this); // set this state as the last state the machine was in",
                $"{tab}" + "}",
                $"{tab}#endregion",
                "}"
            };

            string[] weaponLines =
            {
                "using UltimateFramework.Utils;",
                "using UltimateFramework.ItemSystem;",
                "using UnityEngine;",
                "",
                $"[CreateAssetMenu(menuName = \"Ultimate Framework/Systems/Items/State Asset/Weapon State/#SCRIPTNAME#\")]",
                $"public class #SCRIPTNAME# : WeaponState",
                "{",
                $"{tab}// Function that is called once at the start of the state.",
                $"{tab}public override void StateStart(WeaponBehaviour machine)",
                $"{tab}" + "{",
                $"{tab}{tab}this.stateFlow = StateFlow.Start; // controls the flow of the state",
                $"{tab}{tab}",
                $"{tab}{tab}// Adds transitions to the state so that it can go to other states based on conditions",
                $"{tab}{tab}this.AddTransition(new Transition(",
                $"{tab}{tab}{tab}() => GenericCondition(machine),",
                $"{tab}{tab}{tab}() => GenericTransition(machine),",
                $"{tab}{tab}{tab}machine.FindState(\"OtherStateName\")); // Change \"OtherStateName\" to the name of the state you want to transition to",
                $"{tab}" + "}",
                $"{tab}",
                $"{tab}// Function called multiple times per frame",
                $"{tab}public override void StateUpdate(WeaponBehaviour machine)",
                $"{tab}" + "{",
                $"{tab}{tab}this.stateFlow = StateFlow.Running; // controls the flow of the state",
                $"{tab}{tab}",
                $"{tab}{tab}// If the list of transitions is not empty, it constantly checks if any of the conditions are met to execute the transition.",
                $"{tab}{tab}if (this.Transitions.Count > 0)",
                $"{tab}{tab}{tab}machine.ProcessEvent(this);",
                $"{tab}" + "}",
                $"{tab}",
                $"{tab}",
                $"{tab}// Conditional functions for transitions, one should be created for each transition if necessary.",
                $"{tab}#region Conditions",
                $"{tab}private bool GenericCondition(WeaponBehaviour machine)",
                $"{tab}" + "{",
                $"{tab}{tab}return true; // Write your own logic",
                $"{tab}" + "}",
                $"{tab}#endregion",
                $"{tab}",
                $"{tab}",
                $"{tab}// Functions that will be executed before the next state when transitioning",
                $"{tab}#region Actions",
                $"{tab}private void GenericTransition(WeaponBehaviour machine)",
                $"{tab}" + "{",
                $"{tab}{tab}this.stateFlow = StateFlow.Finished; // controls the flow of the state",
                $"{tab}{tab}machine.SetLastState(this); // set this state as the last state the machine was in",
                $"{tab}" + "}",
                $"{tab}#endregion",
                "}"
            };

            string[] consumableLines =
            {
                "using UltimateFramework.Utils;",
                "using UltimateFramework.ItemSystem;",
                "using UnityEngine;",
                "",
                $"[CreateAssetMenu(menuName = \"Ultimate Framework/Systems/Items/State Asset/Consumable State/#SCRIPTNAME#\")]",
                $"public class #SCRIPTNAME# : ConsumableState",
                "{",
                $"{tab}// Function that is called once at the start of the state.",
                $"{tab}public override void StateStart(ConsumableBehaviour machine)",
                $"{tab}" + "{",
                $"{tab}{tab}this.stateFlow = StateFlow.Start; // controls the flow of the state",
                $"{tab}{tab}",
                $"{tab}{tab}// Adds transitions to the state so that it can go to other states based on conditions",
                $"{tab}{tab}this.AddTransition(new Transition(",
                $"{tab}{tab}{tab}() => GenericCondition(machine),",
                $"{tab}{tab}{tab}() => GenericTransition(machine),",
                $"{tab}{tab}{tab}machine.FindState(\"OtherStateName\"))); // Change \"OtherStateName\" to the name of the state you want to transition to",
                $"{tab}" + "}",
                $"{tab}",
                $"{tab}// Function called multiple times per frame",
                $"{tab}public override void StateUpdate(ConsumableBehaviour machine)",
                $"{tab}" + "{",
                $"{tab}{tab}this.stateFlow = StateFlow.Running; // controls the flow of the state",
                $"{tab}{tab}",
                $"{tab}{tab}// If the list of transitions is not empty, it constantly checks if any of the conditions are met to execute the transition.",
                $"{tab}{tab}if (this.Transitions.Count > 0)",
                $"{tab}{tab}{tab}machine.ProcessEvent(this);",
                $"{tab}" + "}",
                $"{tab}",
                $"{tab}",
                $"{tab}// Conditional functions for transitions, one should be created for each transition if necessary.",
                $"{tab}#region Conditions",
                $"{tab}private bool GenericCondition(ConsumableBehaviour machine)",
                $"{tab}" + "{",
                $"{tab}{tab}return true; // Write your own logic",
                $"{tab}" + "}",
                $"{tab}#endregion",
                $"{tab}",
                $"{tab}",
                $"{tab}// Functions that will be executed before the next state when transitioning",
                $"{tab}#region Actions",
                $"{tab}private void GenericTransition(ConsumableBehaviour machine)",
                $"{tab}" + "{",
                $"{tab}{tab}this.stateFlow = StateFlow.Finished; // controls the flow of the state",
                $"{tab}{tab}machine.SetLastState(this); // set this state as the last state the machine was in",
                $"{tab}" + "}",
                $"{tab}#endregion",
                "}"
            };

            string content = "";

            // Join the lines into a single chain
            switch (stateType)
            {
                case StateType.Armor:
                    content = string.Join("\r\n", armorLines);
                    break;
                case StateType.Weapon:
                    content = string.Join("\r\n", weaponLines);
                    break;
                case StateType.Consumable:
                    content = string.Join("\r\n", consumableLines);
                    break;
            }           

            return content;
        }
        private void CreateStateScript()
        {
            if (SettingsMasterData.Instance.useDiferentPaths)
            {
                // Armors
                if (SettingsMasterData.Instance.statesPathSelection == PathUseAs.LastInstance && stateType == StateType.Armor)
                {
                    newScriptData.scriptPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                }
                else if (SettingsMasterData.Instance.statesPathSelection == PathUseAs.Default && stateType == StateType.Armor)
                {
                    newScriptData.scriptPath = SettingsMasterData.Instance.armorStateScriptsPath;
                }

                // Weapons
                if (SettingsMasterData.Instance.statesPathSelection == PathUseAs.LastInstance && stateType == StateType.Weapon)
                {
                    newScriptData.scriptPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                }
                else if (SettingsMasterData.Instance.statesPathSelection == PathUseAs.Default && stateType == StateType.Weapon)
                {
                    newScriptData.scriptPath = SettingsMasterData.Instance.weaponStateScriptsPath;
                }

                // Consumables
                if (SettingsMasterData.Instance.statesPathSelection == PathUseAs.LastInstance && stateType == StateType.Consumable)
                {
                    newScriptData.scriptPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                }
                else if (SettingsMasterData.Instance.statesPathSelection == PathUseAs.Default && stateType == StateType.Consumable)
                {
                    newScriptData.scriptPath = SettingsMasterData.Instance.consumableStateScriptsPath;
                }
            }
            else
            {
                if (SettingsMasterData.Instance.statesPathSelection == PathUseAs.LastInstance)
                {
                    newScriptData.scriptPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                }
                else if (SettingsMasterData.Instance.statesPathSelection == PathUseAs.Default)
                {
                    newScriptData.scriptPath = SettingsMasterData.Instance.generalStatesScriptsPath;
                }
            }

            string fullScriptPath = Path.Combine(newScriptData.scriptPath, $"{newScriptData.scriptName}{actionSuffix}.cs");

            if (SettingsMasterData.Instance.useDiferentPaths)
            {
                // Armors
                if (!Directory.Exists(newScriptData.scriptPath) && stateType == StateType.Armor)
                {
                    newScriptData.scriptPath = SettingsMasterData.Instance.armorStateScriptsPath;
                    Debug.LogError($"The selected path was not found so the script was created in the default path. " +
                                   $"To see what the path is go to: Ultimate Framework > Settings Master > Actions, located in the top menu bar of Unity."); ;
                    return;
                }

                // Weapons
                if (!Directory.Exists(newScriptData.scriptPath) && stateType == StateType.Armor)
                {
                    newScriptData.scriptPath = SettingsMasterData.Instance.weaponStateScriptsPath;
                    Debug.LogError($"The selected path was not found so the script was created in the default path. " +
                                   $"To see what the path is go to: Ultimate Framework > Settings Master > Actions, located in the top menu bar of Unity."); ;
                    return;
                }

                // Consumables
                if (!Directory.Exists(newScriptData.scriptPath) && stateType == StateType.Armor)
                {
                    newScriptData.scriptPath = SettingsMasterData.Instance.consumableStateScriptsPath;
                    Debug.LogError($"The selected path was not found so the script was created in the default path. " +
                                   $"To see what the path is go to: Ultimate Framework > Settings Master > Actions, located in the top menu bar of Unity."); ;
                    return;
                }
            }
            else
            {
                if (!Directory.Exists(newScriptData.scriptPath))
                {
                    newScriptData.scriptPath = SettingsMasterData.Instance.generalStatesScriptsPath;
                    Debug.LogError($"The selected path was not found so the script was created in the default path. " +
                                   $"To see what the path is go to: Ultimate Framework > Settings Master > Actions, located in the top menu bar of Unity.");
                    return;
                }
            }

            if (File.Exists(fullScriptPath))
            {
                Debug.LogError($"A file with the same name already exists {newScriptData.scriptName}{actionSuffix}.cs");
                return;
            }
            else
            {
                string content = newScriptData.scriptContent.Replace("#SCRIPTNAME#", $"{newScriptData.scriptName}{actionSuffix}");

                File.WriteAllText(fullScriptPath, content);

                Debug.Log($"The state script: {newScriptData.scriptName}{actionSuffix}.cs, has been created" +
                    $"you can now create instances of it from the context menu: Ultimate Framework > Create > Action, located in the top menu bar of Unity.");
                AssetDatabase.Refresh();
            }
        }
    }
}
