using UltimateFramework.Editor;
using UltimateFramework.Utils;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace UltimateFramework.Tools
{
    public class ActionCreationWindow : EditorWindow
    {
        private VisualTreeAsset m_UXML;
        private VisualElement root;
        private CreatorToolScriptsData newScriptData;
        private const string actionSuffix = "Action";

        [MenuItem("Ultimate Framework/Create/Script/Action")]
        [MenuItem("Assets/Create/Ultimate Framework/Systems/Actions/Action Script")]
        public static void ShowWindow()
        {
            GetWindow<ActionCreationWindow>("Script Actions Creator");
        }

        private void LoadResources()
        {
            m_UXML = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/ScriptActionsCreatorWindow");
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
            var createButton = UFEditorUtils.FindElementInRoot<Button>(root, "create-button");
            #endregion

            #region Register Callbacks
            classNameField.RegisterValueChangedCallback(evt =>
            {
                newScriptData.scriptName = evt.newValue;
            });

            createButton.RegisterCallback<ClickEvent>(evt =>
            {
                CreateActionScript();
            });
            #endregion
        }

        private string GenerateContent()
        {
            string tab = "\t"; // Tabulation

            string[] lineas =
            {
                "using System.Threading.Tasks;",
                "using System.Threading;",
                "using UnityEngine;",
                "using System;",
                "",
                "namespace UltimateFramework.ActionsSystem",
                "{",
                $"{tab}[CreateAssetMenu(menuName = \"Ultimate Framework/Systems/Actions/Action Asset/#SCRIPTNAMEMENU#\")]",
                $"{tab}public class #SCRIPTNAME# : BaseAction",
                $"{tab}" + "{",
                $"{tab}{tab}// Optional function used for initial state settings, will be called from the Start function of the ActionsComponent script.",
                $"{tab}{tab}public override void StartConfig(BaseAction action, ActionStructure currentStructure)",
                $"{tab}{tab}" + "{",
                $"{tab}{tab}{tab}action = this;",
                $"{tab}{tab}{tab}base.StartConfig(action, currentStructure); // --> Call to the base (not mandatory)",
                $"{tab}{tab}{tab}// Write your own logic",
                $"{tab}{tab}" + "}",
                $"{tab}{tab}",
                $"{tab}{tab}// Function used for the logic of the action itself, will be called through the assigned input.",
                $"{tab}{tab}public override async Task Execute(EntityActionsManager actionsMaster, ActionStructure currentStructure, Animator animator, CancellationToken ct)",
                $"{tab}{tab}" + "{",
                $"{tab}{tab}{tab}try",
                $"{tab}{tab}{tab}" + "{",
                $"{tab}{tab}{tab}{tab}if (actionsMaster.IsHigherOrEqualPriorityActionExecuting(this))",
                $"{tab}{tab}{tab}{tab}" + "{",
                $"{tab}{tab}{tab}{tab}{tab}CancelAction();",
                $"{tab}{tab}{tab}{tab}{tab}return;",
                $"{tab}{tab}{tab}{tab}" + "}",
                $"{tab}{tab}{tab}{tab}",
                $"{tab}{tab}{tab}{tab}if (actionsMaster.IsCantBeInterruptedActionExecuting(this))",
                $"{tab}{tab}{tab} {tab}" + "{",
                $"{tab}{tab}{tab}{tab}{tab} CancelAction();",
                $"{tab}{tab}{tab}{tab}{tab} return;",
                $"{tab}{tab}{tab} {tab}" + "}",
                $"{tab}{tab}{tab}{tab}",
                $"{tab}{tab}{tab}{tab}if (!actionsMaster.MeetsActionCost(m_Statistics))",
                $"{tab}{tab}{tab}{tab}" + "{",
                $"{tab}{tab}{tab}{tab}{tab}ResetValues();",
                $"{tab}{tab}{tab}{tab}{tab} this.IsExecuting = false;",
                $"{tab}{tab}{tab}{tab}{tab}return;",
                $"{tab}{tab}{tab}{tab}" + "}",
                $"{tab}{tab}{tab}{tab}",
                $"{tab}{tab}{tab}{tab}await base.Execute(actionsMaster, currentStructure, animator, ct); // --> Call to the base (not mandatory)",
                $"{tab}{tab}{tab}{tab}// Write your own logic.",
                $"{tab}{tab}{tab}" + "}",
                $"{tab}{tab}{tab}catch (OperationCanceledException)",
                $"{tab}{tab}{tab}" + "{",
                $"{tab}{tab}{tab}{tab}CancelationTS.Cancel();",
                $"{tab}{tab}{tab}{tab}throw;",
                $"{tab}{tab}{tab}" + "}",                 
                $"{tab}{tab}" + "}",
                $"{tab}{tab}",
                $"{tab}{tab}// Optional function to add logic when sub action is active",
                $"{tab}{tab}protected override void ExecuteInSubActionEnter()",
                $"{tab}{tab}" + "{",
                $"{tab}{tab}{tab}// Write your own logic",
                $"{tab}{tab}" + "}",
                $"{tab}{tab}",
                $"{tab}{tab}// Optional function to add logic when sub action finish",
                $"{tab}{tab}protected override void ExecuteInSubActionExit()",
                $"{tab}{tab}" + "{",
                $"{tab}{tab}{tab}// Write your own logic",
                $"{tab}{tab}" + "}",
                $"{tab}{tab}",
                $"{tab}{tab}// Optional function that should be used to reset values of the action itself, should be called at your convenience within the Execute function.",
                $"{tab}{tab}public override void ResetValues()",
                $"{tab}{tab}" + "{",
                $"{tab}{tab}{tab}base.ResetValues(); // --> Call to the base (not mandatory)",
                $"{tab}{tab}{tab}// Write your own logic",
                $"{tab}{tab}" + "}",
                $"{tab}{tab}",
                $"{tab}{tab}// Optional function to add logic when the action is interrupted by a higher-priority action",
                $"{tab}{tab}public override void InterruptAction()",
                $"{tab}{tab}" + "{",
                $"{tab}{tab}{tab}ResetValues(); // --> Call to reset values (not mandatory)",
                $"{tab}{tab}{tab}this.IsExecuting = false; // --> set this action current executing in false (recommended)",
                $"{tab}{tab}" + "}",
                $"{tab}{tab}",
                $"{tab}{tab}// Optional function to add logic when the action is cancelled by a call while a higher priority action is being executed.",
                $"{tab}{tab}protected override void CancelAction()",
                $"{tab}{tab}" + "{",
                $"{tab}{tab}{tab}ResetValues(); // --> Call to reset values (not mandatory)",
                $"{tab}{tab}{tab}this.IsExecuting = false; // --> set this action current executing in false (recommended)",
                $"{tab}{tab}" + "}",
                $"{tab}" + "}",
                "}"
            };

            // Join the lines into a single chain
            string content = string.Join("\r\n", lineas);

            return content;
        }
        private void CreateActionScript()
        {
            if (SettingsMasterData.Instance.actionsPathSelection == PathUseAs.LastInstance)
            {
                newScriptData.scriptPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            }
            else if (SettingsMasterData.Instance.actionsPathSelection == PathUseAs.Default)
            {
                newScriptData.scriptPath = SettingsMasterData.Instance.actionsScriptsPath;
            }

            string fullScriptPath = Path.Combine(newScriptData.scriptPath, $"{newScriptData.scriptName}{actionSuffix}.cs");

            if (!Directory.Exists(newScriptData.scriptPath))
            {
                newScriptData.scriptPath = SettingsMasterData.Instance.actionsScriptsPath;
                Debug.LogError($"The selected path was not found so the script was created in the default path. " +
                               $"To see what the path is go to: Ultimate Framework > Settings Master > Actions, located in the top menu bar of Unity.");
                return;
            }

            if (File.Exists(fullScriptPath))
            {
                Debug.LogError($"A file with the same name already exists {newScriptData.scriptName}{actionSuffix}.cs");
                return;
            }
            else
            {
                string replaceScriptName = newScriptData.scriptContent.Replace("#SCRIPTNAME#", $"{newScriptData.scriptName}{actionSuffix}");
                string replaceMenuName = replaceScriptName.Replace("#SCRIPTNAMEMENU#", $"{newScriptData.scriptName}");
                File.WriteAllText(fullScriptPath, replaceMenuName);

                Debug.Log($"The state script: {newScriptData.scriptName}{actionSuffix}.cs, has been created" +
                          $"you can now create instances of it from the context menu: Ultimate Framework > Create > Action, " +
                          $"located in the top menu bar of Unity.");

                AssetDatabase.Refresh();

                var icon = Resources.Load<Texture2D>("Img/Action_Asset_Icon");
                IconChanger.ChangeIcon(fullScriptPath, icon);
            }
        }
    }
}
