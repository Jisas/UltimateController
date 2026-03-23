using UltimateFramework.LocomotionSystem;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor;

namespace UltimateFramework.Editor.TabSystem.Strategies.Locomotion
{
    public class SettingsStrategy : TabStrategy
    {
        private bool callbacksRegistered = false;

        protected override void SetupTabContent(object target, VisualElement container)
        {
            var m_Target = target as BaseLocomotionComponent;

            #region Find Elements
            // Section Elements
            var animatorSettings = UFEditorUtils.FindElementInRoot<VisualElement>(container, "animator-settings");
            var groundingSettings = UFEditorUtils.FindElementInRoot<VisualElement>(container, "grounding-settings");

            // Animator Section Elements
            var rootMotionOnMoveSwitch = UFEditorUtils.FindElementInRoot<Button>(animatorSettings, "root-motion-on-move-button");
            var rootMotionOnSprintSwitch = UFEditorUtils.FindElementInRoot<Button>(animatorSettings, "root-motion-on-sprint-button");
            var rootMotionOnJumpSwitch = UFEditorUtils.FindElementInRoot<Button>(animatorSettings, "root-motion-on-jump-button");

            // grounding Section Element
            var groudOffset = UFEditorUtils.FindElementInRoot<FloatField>(groundingSettings, "ground-offset");
            var grounRadius = UFEditorUtils.FindElementInRoot<FloatField>(groundingSettings, "ground-radius");
            var groundLayers = UFEditorUtils.FindElementInRoot<LayerMaskField>(groundingSettings, "ground-layers");
            #endregion

            #region Value Asignament
            // Animator Section Elements
            UFEditorUtils.SetSwitch(m_Target.useRootMotionOnMovement, ref rootMotionOnMoveSwitch);
            UFEditorUtils.SetSwitch(m_Target.useRootMotionOnSprint, ref rootMotionOnSprintSwitch);
            UFEditorUtils.SetSwitch(m_Target.useRootMotionOnJump, ref rootMotionOnJumpSwitch);

            // grounding Section Element
            groudOffset.value = m_Target.groundedOffset;
            grounRadius.value = m_Target.groundedRadius;
            groundLayers.value = m_Target.groundLayers;
            #endregion

            #region Register Callbacks
            if (!callbacksRegistered)
            {
                RegisterButtonCallbacks(m_Target, rootMotionOnMoveSwitch, rootMotionOnSprintSwitch, rootMotionOnJumpSwitch);
                callbacksRegistered = true;
            }

            groudOffset.RegisterValueChangedCallback(evt =>
            {
                m_Target.groundedOffset = evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });

            grounRadius.RegisterValueChangedCallback(evt =>
            {
                m_Target.groundedRadius = evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });

            groundLayers.RegisterValueChangedCallback(evt =>
            {
                m_Target.groundLayers = evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });
            #endregion
        }
        private void RegisterButtonCallbacks(BaseLocomotionComponent target, params Button[] button)
        {
            button[0].clickable.clicked += () =>
            {
                target.useRootMotionOnMovement = !target.useRootMotionOnMovement;
                UFEditorUtils.SetSwitch(target.useRootMotionOnMovement, ref button[0]);
                EditorUtility.SetDirty(target);
            };

            button[1].clickable.clicked += () =>
            {
                target.useRootMotionOnSprint = !target.useRootMotionOnSprint;
                UFEditorUtils.SetSwitch(target.useRootMotionOnSprint, ref button[1]);
                EditorUtility.SetDirty(target);
            };

            button[2].clickable.clicked += () => 
            {
                target.useRootMotionOnJump = !target.useRootMotionOnJump;
                UFEditorUtils.SetSwitch(target.useRootMotionOnJump, ref button[2]);
                EditorUtility.SetDirty(target);
            };
        }
    }
}
