using UltimateFramework.LocomotionSystem;
using UltimateFramework.Utils;
using UnityEngine.UIElements;
using UnityEditor;

namespace UltimateFramework.Editor.TabSystem.Strategies.Locomotion
{
    public class StartConfigStrategy : TabStrategy
    {
        protected override void SetupTabContent(object target, VisualElement container)
        {
            var m_Target = target as BaseLocomotionComponent;

            #region Find Elements
            var locomotionType = UFEditorUtils.FindElementInRoot<EnumField>(container, "locomotion-type");
            var locomotionMode = UFEditorUtils.FindElementInRoot<EnumField>(container, "locomotion-mode");
            var locomotionMap = UFEditorUtils.FindElementInRoot<TextField>(container, "locomotion-map");
            var overrideLayer = UFEditorUtils.FindElementInRoot<TextField>(container, "override-layer");
            var idleBreakWaiting = UFEditorUtils.FindElementInRoot<FloatField>(container, "idle-break-wait-time");
            #endregion

            #region Value Asignament
            locomotionType.value = m_Target.CurrentLocomotionType;
            locomotionMode.value = m_Target.CurrentLocomotionMode;
            locomotionMap.value = m_Target.LocomotionMap;
            overrideLayer.value = m_Target.OverrideLayer;
            idleBreakWaiting.value = m_Target.IdleBreakWaitTime;
            #endregion

            #region Register Callbacks
            locomotionType.RegisterValueChangedCallback(evt =>
            {
                m_Target.CurrentLocomotionType = (LocomotionType)evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });

            locomotionMode.RegisterValueChangedCallback(evt =>
            {
                m_Target.CurrentLocomotionMode = (LocomotionMode)evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });

            locomotionMap.RegisterValueChangedCallback(evt =>
            {
                m_Target.LocomotionMap = evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });

            overrideLayer.RegisterValueChangedCallback(evt =>
            {
                m_Target.OverrideLayer = evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });

            idleBreakWaiting.RegisterValueChangedCallback(evt =>
            {
                m_Target.IdleBreakWaitTime = evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });
            #endregion
        }
    }
}
