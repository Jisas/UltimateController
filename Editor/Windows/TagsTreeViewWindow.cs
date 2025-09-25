using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEditor;

public class TagsTreeViewWindow : EditorWindow
{
    // SerializeField is used to ensure the view state is written to the window 
    // layout file. This means that the state survives restarting Unity as long as the window
    // is not closed. If the attribute is omitted then the state is still serialized/deserialized.
    [SerializeField] TreeViewState m_TreeViewState;

    //The TreeView is not serializable, so it should be reconstructed from the tree data.
    public TagsTreeView m_TagsTreeView;

    void OnEnable()
    {
        // Check whether there is already a serialized view state (state 
        // that survived assembly reloading)
        m_TreeViewState ??= new TreeViewState();
        m_TagsTreeView = new TagsTreeView(m_TreeViewState);
    }

    void OnGUI()
    {
        m_TagsTreeView.OnGUI(new Rect(3, 10, position.width, position.height));
    }
}