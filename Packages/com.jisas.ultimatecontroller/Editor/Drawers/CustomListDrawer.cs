using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Collections.Generic;

namespace UltimateFramework.Editor
{
    public class CustomListDrawer
    {
        public void DrawCustomList(ReorderableList list, string propertyA, string propertyB)
        {
            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                rect.y += 2;
                const int offset = 3;
                var width = rect.width;
                var height = EditorGUIUtility.singleLineHeight;
                var element = list.serializedProperty.GetArrayElementAtIndex(index);

                var varPropiertyA = element.FindPropertyRelative(propertyA);
                var varPropiertyB = element.FindPropertyRelative(propertyB);

                width = rect.width / 2;

                // Draw List
                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y, width - offset, height),
                    varPropiertyA,
                    GUIContent.none);

                EditorGUI.PropertyField(
                    new Rect(rect.x + (width * 1), rect.y, width - offset, height),
                    varPropiertyB,
                    GUIContent.none);
            };
        }
        public void DrawCustomList(ReorderableList list, string propertyA, string propertyB, string propertyC)
        {
            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                rect.y += 2;
                const int offset = 3;
                var width = rect.width;
                var height = EditorGUIUtility.singleLineHeight;
                var element = list.serializedProperty.GetArrayElementAtIndex(index);

                var varPropiertyA = element.FindPropertyRelative(propertyA);
                var varPropiertyB = element.FindPropertyRelative(propertyB);
                var varPropiertyC = element.FindPropertyRelative(propertyC);

                width = rect.width / 3;

                // Draw List
                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y, width - offset, height),
                    varPropiertyA,
                    GUIContent.none);

                EditorGUI.PropertyField(
                    new Rect(rect.x + (width * 1), rect.y, width - offset, height),
                    varPropiertyB,
                    GUIContent.none);

                EditorGUI.PropertyField(
                   new Rect(rect.x + (width * 2), rect.y, width - offset, height),
                   varPropiertyC,
                   GUIContent.none);
            };
        }
        public void DrawCustomList(ReorderableList list, List<string> properties)
        {
            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                rect.y += 2;
                const int offset = 3;
                var width = rect.width;
                var height = EditorGUIUtility.singleLineHeight;
                var element = list.serializedProperty.GetArrayElementAtIndex(index);

                width = rect.width / properties.Count;
                int i = 0;

                foreach (var item in properties)
                {
                    var xPos = rect.x + (width * i);

                    EditorGUI.PropertyField(
                       new Rect(xPos, rect.y, width - offset, height),
                       element.FindPropertyRelative(item),
                       GUIContent.none);

                    i++;
                }
            };
        }
        public void DrawItemsCustomList(ReorderableList list, string buttonText)
        {
            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                rect.y += 2;
                const int offset = 5;
                var width = rect.width;
                var height = EditorGUIUtility.singleLineHeight;
                var element = list.serializedProperty.GetArrayElementAtIndex(index);

                var itemName = element.FindPropertyRelative("name").stringValue;
                var icon = element.FindPropertyRelative("icon").objectReferenceValue as Texture;
                var itemSlot = element.FindPropertyRelative("itemSlot");

                var itemNameStyle = new GUIStyle()
                {
                    fontStyle = FontStyle.Bold,
                    normal = new GUIStyleState() { textColor = Color.white }
                };

                width = rect.width / 4;

                // Draw List
                if (icon == null)
                {
                    EditorGUI.DrawRect(
                        new Rect(rect.x, rect.y, height, height), 
                        Color.white);
                }
                else
                {
                    EditorGUI.DrawTextureTransparent(
                        new Rect(rect.x, rect.y, height, height),
                        icon,
                        ScaleMode.ScaleToFit);
                }

                if (string.IsNullOrEmpty(itemName))
                {
                    EditorGUI.LabelField(
                        new Rect(rect.x + (height * 1) + offset, rect.y, width - offset, height),
                        "None", itemNameStyle);
                }
                else
                {
                    EditorGUI.LabelField(
                        new Rect(rect.x + (height * 1) + offset, rect.y, width - offset, height),
                        itemName, itemNameStyle);
                }

                EditorGUI.PropertyField(
                    new Rect(rect.x + width, rect.y, (width * 2) - offset, height),
                    itemSlot,
                    GUIContent.none);

                Rect buttonRect = new (rect.x + (width * 3), rect.y, width - offset, height);
                if (GUI.Button(buttonRect, buttonText))
                {
                    // Abre la ventana de selección de etiquetas cuando se hace clic en el botón
                    var window = EditorWindow.GetWindow<ItemManagerWindow>("Item Manager");
                    window.Show();
                }
            };
        }
    }
}