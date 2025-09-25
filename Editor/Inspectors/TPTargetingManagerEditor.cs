using UltimateFramework.Editor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TPTargetingManager), true)]
public class TPTargetingManagerEditor : Editor
{
    #region Private Fields
    private VisualElement root;
    private VisualTreeAsset baseVisual;
    private TPTargetingManager m_Target;
    private bool showGeneralSettings, showTargetingSettings;
    #endregion

    #region Mono
    private void OnEnable()
    {
        m_Target = (TPTargetingManager)target;
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
        var targetLayers = UFEditorUtils.FindElementInRoot<LayerMaskField>(generalSettingsBody, "target-layers");
        var targetLocator = UFEditorUtils.FindElementInRoot<ObjectField>(generalSettingsBody, "target-locator");
        var lockOnCanvas = UFEditorUtils.FindElementInRoot<ObjectField>(generalSettingsBody, "lockon-canvas");
        var cinemachineAnim = UFEditorUtils.FindElementInRoot<ObjectField>(generalSettingsBody, "cinemachine-animator");

        var targetingSettings = UFEditorUtils.FindElementInRoot<VisualElement>(root, "targeting-settings");
        var selectableTargetingSettingsHeader = UFEditorUtils.FindElementInRoot<VisualElement>(targetingSettings, "header");
        var targetingHeaderArrow = UFEditorUtils.FindElementInRoot<VisualElement>(targetingSettings, "arrow");
        var targetingSettingsBody = UFEditorUtils.FindElementInRoot<VisualElement>(targetingSettings, "body");
        var noticeZone = UFEditorUtils.FindElementInRoot<FloatField>(targetingSettingsBody, "notice-zone");
        var maxNoticeAngle = UFEditorUtils.FindElementInRoot<FloatField>(targetingSettingsBody, "max-notice-angle");
        var crosshairScale = UFEditorUtils.FindElementInRoot<FloatField>(targetingSettingsBody, "corsshair-scale");

        #endregion

        #region Value Asignament
        targetLayers.value = m_Target.TargetLayers;
        targetLocator.value = m_Target.EnemyTarget_Locator;
        cinemachineAnim.value = m_Target.CinemachineAnimator;
        noticeZone.value = m_Target.NoticeZone;
        maxNoticeAngle.value = m_Target.MaxNoticeAngle;
        crosshairScale.value = m_Target.CrossHair_Scale;
        lockOnCanvas.value = m_Target.LockOnCanvas;
        #endregion

        #region Register Callbacks
        selectableGeneralSettingsHeader.RegisterCallback<ClickEvent>(evt =>
        {
            showGeneralSettings = !showGeneralSettings;
            UFEditorUtils.SetElementDisplay(showGeneralSettings, ref generalSettingsBody);
            UFEditorUtils.SetArrowAnim(generalSettingsBody, ref generalHeaderArrow, "arrowmark-toggle-open");
        });

        targetLayers.RegisterValueChangedCallback(evt =>
        {
            m_Target.TargetLayers = evt.newValue;
            EditorUtility.SetDirty(target);
        });

        targetLocator.RegisterValueChangedCallback(evt =>
        {
            m_Target.EnemyTarget_Locator = (Transform)evt.newValue;
            EditorUtility.SetDirty(target);
        });

        lockOnCanvas.RegisterValueChangedCallback(evt =>
        {
            m_Target.LockOnCanvas = (Transform)evt.newValue;
            EditorUtility.SetDirty(target);
        });

        cinemachineAnim.RegisterValueChangedCallback(evt =>
        {
            m_Target.CinemachineAnimator = (Animator)evt.newValue;
            EditorUtility.SetDirty(target);
        });

        selectableTargetingSettingsHeader.RegisterCallback<ClickEvent>(evt =>
        {
            showTargetingSettings = !showTargetingSettings;
            UFEditorUtils.SetElementDisplay(showTargetingSettings, ref targetingSettingsBody);
            UFEditorUtils.SetArrowAnim(targetingSettingsBody, ref targetingHeaderArrow, "arrowmark-toggle-open");
        });

        noticeZone.RegisterValueChangedCallback(evt =>
        {
            m_Target.NoticeZone = evt.newValue;
            EditorUtility.SetDirty(target);
        });

        maxNoticeAngle.RegisterValueChangedCallback(evt =>
        {
            m_Target.MaxNoticeAngle = evt.newValue;
            EditorUtility.SetDirty(target);
        });

        crosshairScale.RegisterValueChangedCallback(evt =>
        {
            m_Target.CrossHair_Scale = evt.newValue;
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
        baseVisual = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Components/TPTargetingManager_Inspector");
    }
    #endregion
}