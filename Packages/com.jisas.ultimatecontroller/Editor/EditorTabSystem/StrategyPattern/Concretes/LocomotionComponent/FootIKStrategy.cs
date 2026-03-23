using UltimateFramework.LocomotionSystem;
using UnityEngine.UIElements;
using UnityEditor;

namespace UltimateFramework.Editor.TabSystem.Strategies.Locomotion
{
    public class FootIKStrategy : TabStrategy
    {
        private bool callbacksRegistered = false;

        protected override void SetupTabContent(object target, VisualElement container)
        {
            var m_Target = target as BaseLocomotionComponent;

            #region Find Elements
            // Section Elements
            var enableSettings = UFEditorUtils.FindElementInRoot<VisualElement>(container, "enable-setting");
            var footIKSettings = UFEditorUtils.FindElementInRoot<VisualElement>(container, "foot-ik-settings");

            // Enable Section Elements
            var enableFootIKSwitch = UFEditorUtils.FindElementInRoot<Button>(enableSettings, "foot-ik-button");
            var showSolverSwitch = UFEditorUtils.FindElementInRoot<Button>(enableSettings, "show-solver-button");

            // Foot IK Section Element
            var rayGroundHeight = UFEditorUtils.FindElementInRoot<Slider>(footIKSettings, "ray-ground-height");
            var rayDownDistance = UFEditorUtils.FindElementInRoot<Slider>(footIKSettings, "ray-down-distance");
            var pelvisResposSpeed = UFEditorUtils.FindElementInRoot<Slider>(footIKSettings, "pelvis-repos-speed");
            var feetIKPosSpeed = UFEditorUtils.FindElementInRoot<Slider>(footIKSettings, "feet-ik-pos-speed");
            var pelvisOffset = UFEditorUtils.FindElementInRoot<Slider>(footIKSettings, "pelvis-offset");
            var lefFootAnimVariable = UFEditorUtils.FindElementInRoot<TextField>(footIKSettings, "left-foot-anim-variable");
            var rightFootAnimVariable = UFEditorUtils.FindElementInRoot<TextField>(footIKSettings, "right-foot-anim-variable");
            #endregion

            #region Value Asignament
            // Enable Section Elements
            UFEditorUtils.SetSwitch(m_Target.enableFeetIK, ref enableFootIKSwitch);
            UFEditorUtils.SetSwitch(m_Target.showSolverDebug, ref showSolverSwitch);
            UFEditorUtils.SetElementDisplay(m_Target.enableFeetIK, ref footIKSettings);

            // Foot IK Section Elements
            rayGroundHeight.value = m_Target.heightFromGroundRaycast;
            rayDownDistance.value = m_Target.raycastDownDistance;
            pelvisResposSpeed.value = m_Target.pelvisUpAndDownSpeed;
            feetIKPosSpeed.value = m_Target.feetToIKPositionSpeed;
            pelvisOffset.value = m_Target.pelvisOffset;
            lefFootAnimVariable.value = m_Target.leftFootAnimVariableName;
            rightFootAnimVariable.value = m_Target.rightFootAnimVariableName;
            #endregion

            #region Register Callbacks
            if (!callbacksRegistered)
            {
                RegisterButtonCallbacks(m_Target, footIKSettings, enableFootIKSwitch, showSolverSwitch);
                callbacksRegistered = true;
            }

            rayGroundHeight.RegisterValueChangedCallback(evt =>
            {
                m_Target.heightFromGroundRaycast = evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });

            rayDownDistance.RegisterValueChangedCallback(evt =>
            {
                m_Target.raycastDownDistance = evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });

            pelvisResposSpeed.RegisterValueChangedCallback(evt =>
            {
                m_Target.pelvisUpAndDownSpeed = evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });

            feetIKPosSpeed.RegisterValueChangedCallback(evt =>
            {
                m_Target.feetToIKPositionSpeed = evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });

            pelvisOffset.RegisterValueChangedCallback(evt =>
            {
                m_Target.pelvisOffset = evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });

            lefFootAnimVariable.RegisterValueChangedCallback(evt =>
            {
                m_Target.leftFootAnimVariableName = evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });

            rightFootAnimVariable.RegisterValueChangedCallback(evt =>
            {
                m_Target.rightFootAnimVariableName = evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });
            #endregion
        }
        private void RegisterButtonCallbacks(BaseLocomotionComponent target, VisualElement footIKSettings, params Button[] button)
        {
            button[0].clickable.clicked += () =>
            {
                target.enableFeetIK = !target.enableFeetIK;
                UFEditorUtils.SetSwitch(target.enableFeetIK, ref button[0]);
                UFEditorUtils.SetElementDisplay(target.enableFeetIK, ref footIKSettings);
                EditorUtility.SetDirty(target);
            };

            button[1].clickable.clicked += () =>
            {
                target.showSolverDebug = !target.showSolverDebug;
                UFEditorUtils.SetSwitch(target.showSolverDebug, ref button[1]);
                EditorUtility.SetDirty(target);
            };
        }
    }
}
