using UltimateFramework.ActionsSystem;
using UnityEngine.UIElements;
using UnityEditor;

public class InspectorView : VisualElement
{
    Editor editor;

    public new class UxmlFactory : UxmlFactory<InspectorView, VisualElement.UxmlTraits> { }
    public void UpdateInspector(BaseAction action)
    {
        Clear();
        UnityEngine.Object.DestroyImmediate(editor);
        editor = Editor.CreateEditor(action);
        IMGUIContainer container = new(() => { editor.OnInspectorGUI(); });
        Add(container);
    }
}