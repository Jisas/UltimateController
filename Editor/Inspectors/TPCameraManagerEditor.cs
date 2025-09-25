using UltimateFramework.Editor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(TPCameraManager), true)]
public class TPCameraManagerEditor : Editor
{
    #region Private Fields
    private VisualElement root;
    private VisualTreeAsset baseVisual;
    private TPCameraManager m_Target;
    private bool showGeneralSettings, showMovementSettings;
    #endregion

    #region Mono
    private void OnEnable()
    {
        m_Target = (TPCameraManager)target;
    }
    public override VisualElement CreateInspectorGUI()
    {
        LoadResources();
        baseVisual.CloneTree(root);

        #region Find Elements
        var generalSettings = UFEditorUtils.FindElementInRoot<VisualElement>(root, "general-settings");
        var selectableGeneralSettingsHeader = UFEditorUtils.FindElementInRoot<VisualElement>(generalSettings, "header");
        var generalHeaderArrow = UFEditorUtils.FindElementInRoot<VisualElement>(generalSettings, "arrow");
        var generalSettingsBody = UFEditorUtils.FindElementInRoot<VisualElement>(generalSettings, "body");
        var cameraTarget = UFEditorUtils.FindElementInRoot<ObjectField>(generalSettingsBody, "camera-target");
        var topClamp = UFEditorUtils.FindElementInRoot<FloatField>(generalSettingsBody, "top-clamp");
        var bottomClamp = UFEditorUtils.FindElementInRoot<FloatField>(generalSettingsBody, "bottom-clamp");
        var cameraAngle = UFEditorUtils.FindElementInRoot<FloatField>(generalSettingsBody, "camera-angle");

        var movementSettings = UFEditorUtils.FindElementInRoot<VisualElement>(root, "movement-settings");
        var selectableMovementSettingsHeader = UFEditorUtils.FindElementInRoot<VisualElement>(movementSettings, "header");
        var movementHeaderArrow = UFEditorUtils.FindElementInRoot<VisualElement>(movementSettings, "arrow");
        var movementSettingsBody = UFEditorUtils.FindElementInRoot<VisualElement>(movementSettings, "body");
        var mouseSens = UFEditorUtils.FindElementInRoot<Slider>(movementSettingsBody, "mouse-sens");
        var StickSens = UFEditorUtils.FindElementInRoot<Slider>(movementSettingsBody, "stick-sens");
        #endregion

        #region Value Asignament
        cameraTarget.value = m_Target.target;
        topClamp.value = m_Target.topClamp;
        bottomClamp.value = m_Target.bottomClamp;
        cameraAngle.value = m_Target.cameraAngleOverride;
        mouseSens.value = m_Target.mouseSens;
        StickSens.value = m_Target.stickSens;
        #endregion

        #region Register Callbacks
        selectableGeneralSettingsHeader.RegisterCallback<ClickEvent>(evt =>
        {
            showGeneralSettings = !showGeneralSettings;
            UFEditorUtils.SetElementDisplay(showGeneralSettings, ref generalSettingsBody);
            UFEditorUtils.SetArrowAnim(generalSettingsBody, ref generalHeaderArrow, "arrowmark-toggle-open");
        });

        cameraTarget.RegisterValueChangedCallback(evt =>
        {
            m_Target.target = (GameObject)evt.newValue;
            EditorUtility.SetDirty(target);
        });

        topClamp.RegisterValueChangedCallback(evt =>
        {
            m_Target.topClamp = evt.newValue;
            EditorUtility.SetDirty(target);
        });

        bottomClamp.RegisterValueChangedCallback(evt =>
        {
            m_Target.bottomClamp = evt.newValue;
            EditorUtility.SetDirty(target);
        });

        cameraAngle.RegisterValueChangedCallback(evt =>
        {
            m_Target.cameraAngleOverride = evt.newValue;
            EditorUtility.SetDirty(target);
        });

        selectableMovementSettingsHeader.RegisterCallback<ClickEvent>(evt =>
        {
            showMovementSettings = !showMovementSettings;
            UFEditorUtils.SetElementDisplay(showMovementSettings, ref movementSettingsBody);
            UFEditorUtils.SetArrowAnim(movementSettingsBody, ref movementHeaderArrow, "arrowmark-toggle-open");
        });

        mouseSens.RegisterValueChangedCallback(evt =>
        {
            m_Target.mouseSens = evt.newValue;
            EditorUtility.SetDirty(target);
        });

        StickSens.RegisterValueChangedCallback(evt =>
        {
            m_Target.stickSens = evt.newValue;
            EditorUtility.SetDirty(target);
        });
        #endregion

        return root;
    }
    #endregion

    #region Internal
    private void LoadResources()
    {
        root = new VisualElement();
        baseVisual = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Components/TPCameraManager_Inspector");
    }
    #endregion
}
#endif