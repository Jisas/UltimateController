using UnityEditor;
using UnityEngine;

namespace UltimateFramework.Editor
{
    [CustomPropertyDrawer(typeof(TagSelector))]
    public class TagSelectorPropertyDrawer : PropertyDrawer
    {
        private EditorWindow currentWindow;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // almacena la propiedad para luego pasarsela a TreeViewWindow
            SerializedProperty tagProperty = property.FindPropertyRelative("tag");

            // Si el contenido es null, muestra "None"
            if (string.IsNullOrEmpty(tagProperty.stringValue))
                tagProperty.stringValue = "None";
          
            // Crea un estilo personalizado para el botón
            GUIStyle buttonStyle = new(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 12,
                padding = new RectOffset(3, 3, 3, 3)
            };

            // Crea una nueva textura
            Texture2D texture = new(1, 1);

            // Establece el color de la textura
            Color backgraundColor = new(0.1333333f, 0.1333333f, 0.1333333f, 0.8f);
            texture.SetPixel(0, 0, backgraundColor);
            texture.Apply();

            // Asigna la textura al fondo del botón
            buttonStyle.normal.background = texture;

            // Ajusta los bordes
            buttonStyle.border = new RectOffset(1, 1, 1, 1);

            float width = position.width / 2;
            float offset = 50;

            // Divide el espacio disponible en la fila
            Rect labelRect = new (position.x, position.y, width - offset, position.height);
            Rect buttonRect = new (((position.x - 5) + width ) - offset, position.y, width + offset + 5, position.height);

            if (label.text == tagProperty.stringValue) label.text = "Tag Selector";

            GUI.color = Color.white;

            // Dibuja el label
            EditorGUI.LabelField(labelRect, label);

            // Dibuja el botón
            if (GUI.Button(buttonRect, tagProperty.stringValue, buttonStyle))
            {
                UpdateTagSelectorWindow(tagProperty);
            }
        }

        private void UpdateTagSelectorWindow(SerializedProperty tagProperty)
        {
            if (currentWindow != null) currentWindow.Close();

            // Abre la ventana de selección de etiquetas cuando se hace clic en el botón
            TagsTreeViewWindow window = EditorWindow.GetWindow<TagsTreeViewWindow>();
            Texture2D icon = Resources.Load<Texture2D>("Img/Tag_Window_Icon");
            window.titleContent = new GUIContent("Game Tags", icon);
            currentWindow = window;

            // Almacena la referencia a la propiedad 'tag'
            window.m_TagsTreeView.TagProperty = tagProperty;
            window.Show();
        }
    }
}