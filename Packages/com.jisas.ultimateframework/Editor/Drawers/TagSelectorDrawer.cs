using UnityEditor;
using UnityEngine;

namespace UltimateFramework.Editor
{
    public class TagSelectorDrawer
    {
        private string tag;
        public string Tag
        {
            get { return tag; }
        }

        public void DrawSelector(SerializedProperty value, bool includeChildrens = default)
        {
            // Si el contenido es null, muestra "None"
            if (value != null)
                tag = "None";
            else
                tag = value.stringValue;

            // Dibuja el elemento entero
            GUI.color = Color.white;
            EditorGUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.MaxWidth(30));
            GUILayout.Space(1.7f);
            EditorGUILayout.LabelField("Tag:", GUILayout.MaxWidth(30));
            GUILayout.EndVertical();
            EditorGUILayout.PropertyField(value, includeChildrens, GUILayout.MaxWidth(200));
            EditorGUILayout.EndHorizontal();
        }
    }
}