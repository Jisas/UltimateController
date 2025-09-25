using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UltimateFramework.Tag;
using UnityEditor;
using UnityEngine;
using System;

public class TagsTreeView : TreeView
{
    private string selectedTag;
    public string SelectedTag { get => selectedTag; }
    public SerializedProperty TagProperty { get; set; }
    public string TagString { get; set; }

    public event Action<string> OnTagSelected;

    public TagsTreeView(TreeViewState treeViewState)
        : base(treeViewState)
    {
        Reload();
    }

    private void AddTagsToTree(TreeViewItem root, List<string> tags)
    {
        Dictionary<string, TreeViewItem> nodes = new();
        int id = 1;

        foreach (var tag in tags)
        {
            var parts = tag.Split('.');
            TreeViewItem parent = root;

            string path = "";
            foreach (var part in parts)
            {
                path += (path == "" ? "" : ".") + part;
                if (!nodes.ContainsKey(path))
                {
                    var node = new TreeViewItem { id = id++, displayName = part };
                    parent.AddChild(node);
                    nodes[path] = node;
                }
                parent = nodes[path];
            }
        }

        SetupDepthsFromParentsAndChildren(root);
    }
    protected override void SelectionChanged(IList<int> selectedIds)
    {
        if (selectedIds.Count > 0)
        {
            var selectedItem = FindItem(selectedIds[0], rootItem);
            if (selectedItem != null)
            {
                selectedTag = GetFullPath(selectedItem);
                TagString = selectedTag;

                if (TagProperty != null)
                {
                    TagProperty.stringValue = selectedTag; // Actualiza el valor de la propiedad 'tag'
                    TagProperty.serializedObject.ApplyModifiedProperties(); // Aplica los cambios
                }

                OnTagSelected?.Invoke(selectedTag); // Dispara el evento
            }
        }
    }
    private string GetFullPath(TreeViewItem item)
    {
        string path = item.displayName;
        while (item.parent != null)
        {
            item = item.parent;
            if (item.depth == -1) // Skip the root node
                break;
            path = item.displayName + "." + path;
        }
        return path;
    }
    protected override TreeViewItem BuildRoot()
    {
        var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
        var tagDataBase = Resources.Load<TagDataBase>("Data/Tags/TagData");
        AddTagsToTree(root, tagDataBase.tags);
        return root;
    }
}