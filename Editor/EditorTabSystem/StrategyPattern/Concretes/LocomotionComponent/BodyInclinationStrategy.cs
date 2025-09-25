using UltimateFramework.LocomotionSystem;
using UnityEngine.UIElements;
using UnityEditor;

namespace UltimateFramework.Editor.TabSystem.Strategies.Locomotion
{
    public class BodyInclinationStrategy : TabStrategy
    {
        private bool callbacksRegistered = false;

        protected override void SetupTabContent(object target, VisualElement container)
        {
            var m_Target = target as BaseLocomotionComponent;

            #region Find Elements
            // Section Elements
            var enableSettings = UFEditorUtils.FindElementInRoot<VisualElement>(container, "enable-setting");
            var bodyInclinationSettings = UFEditorUtils.FindElementInRoot<VisualElement>(container, "body-inclination-settings");

            // Enable Section Elements
            var bodyInclinationButton = UFEditorUtils.FindElementInRoot<Button>(enableSettings, "body-inclination-button");

            // Body Inclination Section Elements
            var minInclineAngle = UFEditorUtils.FindElementInRoot<Slider>(bodyInclinationSettings, "min-incline-angle");
            var incllineSpeed = UFEditorUtils.FindElementInRoot<Slider>(bodyInclinationSettings, "incline-speed");
            var sensorOffsetY = UFEditorUtils.FindElementInRoot<Slider>(bodyInclinationSettings, "sensor-offset-y");
            var minMaxBodyInclination = UFEditorUtils.FindElementInRoot<MinMaxSlider>(bodyInclinationSettings, "min-max-body-inclination");
            #endregion

            #region Value Asignament
            // Enable Section Elements
            UFEditorUtils.SetSwitch(m_Target.enableBodyInclination, ref bodyInclinationButton);
            UFEditorUtils.SetElementDisplay(m_Target.enableBodyInclination, ref bodyInclinationSettings);

            // Body Inclination Section Elements
            minInclineAngle.value = m_Target.minAngleToIncline;
            incllineSpeed.value = m_Target.inclineSpeed;
            sensorOffsetY.value = m_Target.sensorOffsetY;
            minMaxBodyInclination.minValue = m_Target.bodyMinMaxInclination.Min;
            minMaxBodyInclination.maxValue = m_Target.bodyMinMaxInclination.Max;
            #endregion

            #region Register Callbacks
            if (!callbacksRegistered)
            {
                bodyInclinationButton.clickable.clicked += () =>
                {
                    m_Target.enableBodyInclination = !m_Target.enableBodyInclination;
                    UFEditorUtils.SetSwitch(m_Target.enableBodyInclination, ref bodyInclinationButton);
                    UFEditorUtils.SetElementDisplay(m_Target.enableBodyInclination, ref bodyInclinationSettings);
                };
                callbacksRegistered = true;
            }

            minInclineAngle.RegisterValueChangedCallback(evt =>
            {
                m_Target.minAngleToIncline = evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });

            incllineSpeed.RegisterValueChangedCallback(evt =>
            {
                m_Target.inclineSpeed = evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });

            sensorOffsetY.RegisterValueChangedCallback(evt =>
            {
                m_Target.sensorOffsetY = evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });

            minMaxBodyInclination.RegisterValueChangedCallback(evt =>
            {
                m_Target.bodyMinMaxInclination.Min = evt.newValue.x;
                m_Target.bodyMinMaxInclination.Max = evt.newValue.y;
                EditorUtility.SetDirty(m_Target);
            });
            #endregion
        }
    }
}
