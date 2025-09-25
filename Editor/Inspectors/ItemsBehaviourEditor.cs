using UltimateFramework.Editor;
using UnityEditorInternal;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(ItemBehaviour), true)]
public class ItemsBehaviourEditor : Editor
{
    private Texture ultimateFrameworkLogo;
    private int toolbarInt;
    readonly string[] toolbarStrings = { "Settings", "States", "Events"};
    private ReorderableList statesList;

    void OnEnable()
    {
        toolbarInt = 0;

        if (ultimateFrameworkLogo == null)
            ultimateFrameworkLogo = Resources.Load("Img/UltimateFramework_Logo_v2") as Texture;

        DrawCustomList();
    }

    private void DrawCustomList()
    {
        statesList = new(serializedObject, serializedObject.FindProperty("statesList"), true, false, true, true);
        statesList.drawElementCallback =
            (Rect rect, int index, bool isActive, bool isFocused) => {

                rect.y += 2;
                const int offset = 3;
                var width = rect.width;
                var height = EditorGUIUtility.singleLineHeight;

                var element = statesList.serializedProperty.GetArrayElementAtIndex(index);
                var actionAsset = element.FindPropertyRelative("stateAsset");

                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y, width - offset, height),
                    actionAsset,
                    GUIContent.none);
            };       
    }

    public override void OnInspectorGUI()
    {
        #region Variables
        // Gui Content
        GUIContent logo = new()
        {
            image = ultimateFrameworkLogo
        };

        // Text
        var headerLabel = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, fontSize = 13 };
        var tittleStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
        var subTittleStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft };

        // Color
        Color color = new(0.6470588f, 0.8588235f, 0.9647059f);

        // Styles
        GUIStyle myHelpBox = new(EditorStyles.helpBox);
        GUIStyle myToolbarButton = new(EditorStyles.toolbarButton);
        #endregion

        // Editor Header
        UFEditorUtils.DrawEditorHeader(logo, tittleStyle);

        #region Toolbars
        toolbarInt = GUILayout.Toolbar(toolbarInt, toolbarStrings);

        // Settings
        if (toolbarInt == 0)
        {
            EditorGUILayout.BeginVertical();
            serializedObject.Update();
            EditorGUILayout.BeginVertical(myHelpBox);
            UFEditorUtils.DrawSectionHeader("Item", headerLabel, color);
            GUILayout.Space(3);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("itemName"), new GUIContent("Item Name"));
            GUILayout.Space(3);
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);

            serializedObject.ApplyModifiedProperties();
            GUILayout.Space(3);
            EditorGUILayout.EndVertical();
        }

        // States
        if (toolbarInt == 1)
        {
            serializedObject.Update();
            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginVertical(myHelpBox);
            UFEditorUtils.DrawSectionHeader("List of States", headerLabel, color);
            GUILayout.Space(3);
            statesList.DoLayoutList();
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);

            GUILayout.Space(3);
            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }

        // Events
        if (toolbarInt == 2)
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Space(3);

            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(3);
            EditorGUILayout.EndVertical();
        }
        #endregion
    }

}
#endif
