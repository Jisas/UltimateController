using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace UltimateFramework.Editor
{
    public static class UFEditorUtils
    {
        private static EditorWindow currentTagSelectorWindow;
        private static Color selectedColor = new(0.4245283f, 0.4245283f, 0.2382965f, 1);
        private static Color toolbarToggleNonSelectedColor = new(0.1568628f, 0.1568628f, 0.1568628f, 1);
        private static Color buttonNonSelectedColor = new(0.345098f, 0.345098f, 0.345098f, 1);

        public static void DrawSectionHeader(string labelText, GUIStyle labelStyle, Color labelTextColor = default)
        {
            GUI.color = labelTextColor != default ? labelTextColor : Color.white;
            EditorGUILayout.LabelField(labelText, labelStyle, GUILayout.ExpandWidth(true));
            GUI.color = Color.white;
        }
        public static void DrawEditorHeader(GUIContent logo, GUIStyle tittleStyle)
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField(logo, tittleStyle, GUILayout.Height(70));
            EditorGUILayout.LabelField("Weapon Behaviour", tittleStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(10);
        }
        public static T FindElementInRoot<T>(VisualElement root, string name) where T : VisualElement
        {
            return root.Q<T>(name);
        }
        public static void SetElementDisplay(bool value, ref VisualElement element)
        {
            if (value) element.style.display = DisplayStyle.Flex;
            else element.style.display = DisplayStyle.None;
        }
        public static void OpenTagSelectorWindow(Label tagLabel, object obj, string fieldName)
        {
            if (currentTagSelectorWindow != null) currentTagSelectorWindow.Close();

            TagsTreeViewWindow window = EditorWindow.GetWindow<TagsTreeViewWindow>();
            Texture2D icon = Resources.Load<Texture2D>("Img/Tag_Window_Icon");
            window.titleContent = new GUIContent("Game Tags", icon);
            currentTagSelectorWindow = window;

            window.m_TagsTreeView.OnTagSelected += tag =>
            {
                // Uses reflection to assign the value to the specific field
                if (obj != null && !string.IsNullOrEmpty(fieldName))
                {
                    // Adds BindingFlags if the field is not public
                    var fieldInfo = obj.GetType().GetField(fieldName,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    if (fieldInfo != null && fieldInfo.FieldType == typeof(string))
                    {
                        // Make sure you are modifying the original object.
                        fieldInfo.SetValue(obj, tag);
                    }
                }

                tagLabel.text = tag;
            };
            window.Show();
        }
        public static void SetSwitch(bool value, ref Button buton)
        {
            if (value)
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
        public static void SetArrowAnim(VisualElement container, ref VisualElement arrow, string className)
        {
            if (container.style.display == DisplayStyle.None)
                 arrow.RemoveFromClassList(className);
            else arrow.AddToClassList(className);
        }
        public static void SwitchToolbarToggleColor(ToolbarToggle toggle, bool value)
        {
            toggle.style.backgroundColor = value ? selectedColor : toolbarToggleNonSelectedColor;
        }
        public static void SwitchButonColor(Button button, bool value)
        {
            button.style.backgroundColor = value ? selectedColor : buttonNonSelectedColor;
        }
    }
}
