using UltimateFramework.ActionsSystem;
using UnityEngine.UIElements;

#if UNITY_EDITOR
public class InspectorView : VisualElement
{
    private UnityEditor.Editor editor;

    public new class UxmlFactory : UxmlFactory<InspectorView, VisualElement.UxmlTraits> { }
    public void UpdateInspector(BaseAction action)
    {
        Clear();
        UnityEngine.Object.DestroyImmediate(editor);
        editor = UnityEditor.Editor.CreateEditor(action);
        IMGUIContainer container = new(() => { editor.OnInspectorGUI(); });
        Add(container);
    }
}
#endif