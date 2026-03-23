using UltimateFramework.ActionsSystem;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine;
using System;

public class ActionsInspectorViewWindow : EditorWindow
{
    public Action<BaseAction> OnEditActionAsset;
    private VisualTreeAsset m_UXML;
    private VisualElement base_Root;

    private void CreateGUI()
    {
        m_UXML = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/ActionAssetInspectorWindow");
        base_Root = rootVisualElement;
        m_UXML.CloneTree(base_Root);
        OnEditActionAsset += ActionAsset;
    }

    private void ActionAsset(BaseAction action)
    {
        var inspectorView = base_Root.Q<InspectorView>("action-inspector");
        inspectorView.UpdateInspector(action);
    }
}