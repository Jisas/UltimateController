using UltimateFramework.LocomotionSystem;
using UnityEngine.UIElements;
using UnityEditor;

namespace UltimateFramework.Editor.TabSystem.Strategies.Locomotion
{
    public class HeadTrackingStrategy : TabStrategy
    {
        private bool callbacksRegistered = false;

        protected override void SetupTabContent(object target, VisualElement container)
        {
            var m_Target = target as BaseLocomotionComponent;

            #region Find Elements
            // Section Elements
            var enableSettings = UFEditorUtils.FindElementInRoot<VisualElement>(container, "enable-setting");
            var headTrackingSettings = UFEditorUtils.FindElementInRoot<VisualElement>(container, "head-track-settings");

            // Enable Section Elements
            var headTrackingButton = UFEditorUtils.FindElementInRoot<Button>(enableSettings, "head-tracking-button");

            // Body Inclination Section Elements
            var lookTargetOffset = UFEditorUtils.FindElementInRoot<Vector2Field>(headTrackingSettings, "look-target-offset");
            var lookDistance = UFEditorUtils.FindElementInRoot<FloatField>(headTrackingSettings, "look-distance");
            var smoothSpeed = UFEditorUtils.FindElementInRoot<Slider>(headTrackingSettings, "smooth-speed");
            var horizontalAngleLimit = UFEditorUtils.FindElementInRoot<MinMaxSlider>(headTrackingSettings, "horizontal-angle-limit");
            #endregion

            #region Value Asignament
            // Enable Section Elements
            UFEditorUtils.SetSwitch(m_Target.enableHeadTracking, ref headTrackingButton);
            UFEditorUtils.SetElementDisplay(m_Target.enableHeadTracking, ref headTrackingSettings);

            // Body Inclination Section Elements
            lookTargetOffset.value = m_Target.lookTargetOffset;
            lookDistance.value = m_Target.lookDistance;
            smoothSpeed.value = m_Target.smoothSpeed;
            horizontalAngleLimit.minValue = m_Target.horizontalAngleLimit.Min;
            horizontalAngleLimit.maxValue = m_Target.horizontalAngleLimit.Max;
            #endregion

            #region Register Callbacks
            if (!callbacksRegistered)
            {
                headTrackingButton.clickable.clicked += () =>
                {
                    m_Target.enableHeadTracking = !m_Target.enableHeadTracking;
                    UFEditorUtils.SetSwitch(m_Target.enableHeadTracking, ref headTrackingButton);
                    UFEditorUtils.SetElementDisplay(m_Target.enableHeadTracking, ref headTrackingSettings);
                    EditorUtility.SetDirty(m_Target);
                };
                callbacksRegistered = true;
            }

            lookDistance.RegisterValueChangedCallback(evt =>
            {
                m_Target.lookDistance = evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });

            lookTargetOffset.RegisterValueChangedCallback(evt =>
            {
                m_Target.lookTargetOffset = evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });

            smoothSpeed.RegisterValueChangedCallback(evt =>
            {
                m_Target.smoothSpeed = evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });

            horizontalAngleLimit.RegisterValueChangedCallback(evt =>
            {
                m_Target.horizontalAngleLimit.Min = evt.newValue.x;
                m_Target.horizontalAngleLimit.Max = evt.newValue.y;
                EditorUtility.SetDirty(m_Target);
            });
            #endregion
        }
    }
}
