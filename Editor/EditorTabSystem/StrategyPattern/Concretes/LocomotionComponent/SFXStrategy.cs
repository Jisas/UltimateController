using UltimateFramework.LocomotionSystem;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEditor;


namespace UltimateFramework.Editor.TabSystem.Strategies.Locomotion
{
#if UNITY_EDITOR
    public class SFXStrategy : TabStrategy
    {
        private bool showListBody = false;
        private bool callbacksRegistered = false;
        private VisualTreeAsset AudioClipElement;

        protected override void SetupTabContent(object target, VisualElement container)
        {
            var m_Target = target as BaseLocomotionComponent;

            #region Find Elements
            AudioClipElement = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Components/Templates/AudioClipElement");

            var footStepVolume = UFEditorUtils.FindElementInRoot<Slider>(container, "footstep-audio-volume");
            var landingAudioClip = UFEditorUtils.FindElementInRoot<ObjectField>(container, "landing-audio-clip");
            var listHeader = UFEditorUtils.FindElementInRoot<VisualElement>(container, "list-opener");
            var listHeaderArrow = UFEditorUtils.FindElementInRoot<VisualElement>(listHeader, "arrow");
            var listCount = UFEditorUtils.FindElementInRoot<Label>(listHeader, "count");
            var listAddButton = UFEditorUtils.FindElementInRoot<Button>(container, "add-audio-clip");
            var listBody = UFEditorUtils.FindElementInRoot<VisualElement>(container, "List-Body");
            #endregion

            #region Value Asignament
            footStepVolume.value = m_Target.footstepAudioVolume;
            landingAudioClip.value = m_Target.landingAudioClip;
            #endregion

            #region Register Callbacks  
            if (!callbacksRegistered)
            {
                foreach (var audioClip in m_Target.footstepAudioClips)
                {
                    AddAudioElement(m_Target, listBody, m_Target.footstepAudioClips.IndexOf(audioClip), listCount);
                }

                listHeader.RegisterCallback<ClickEvent>(evt =>
                {
                    showListBody = !showListBody;
                    UFEditorUtils.SetElementDisplay(showListBody, ref listBody);
                    UFEditorUtils.SetArrowAnim(listBody, ref listHeaderArrow, "arrowmark-toggle-open");
                });

                listAddButton.clickable.clicked += () =>
                {
                    int sampleRate = 44100;
                    int lengthSamples = sampleRate * 2;
                    int channels = 1;
                    var newClip = AudioClip.Create("EmptyClip", lengthSamples, channels, sampleRate, false);

                    m_Target.footstepAudioClips.Add(newClip);
                    AddAudioElement(m_Target, listBody, m_Target.footstepAudioClips.IndexOf(newClip), listCount);
                    EditorUtility.SetDirty(m_Target);
                };

                callbacksRegistered = true;
            }

            footStepVolume.RegisterValueChangedCallback(evt =>
            {
                m_Target.footstepAudioVolume = evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });

            landingAudioClip.RegisterValueChangedCallback(evt =>
            {
                m_Target.landingAudioClip = (AudioClip)evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });
            #endregion
        }

        private void AddAudioElement(BaseLocomotionComponent target, VisualElement container, int clipIndex, Label listCount)
        {
            listCount.text = $"{target.footstepAudioClips.Count} elements";

            var instance = AudioClipElement.CloneTree();
            var currentClip = target.footstepAudioClips[clipIndex];
            var adioClipField = UFEditorUtils.FindElementInRoot<ObjectField>(instance, "audio-clip");
            var removeButton = UFEditorUtils.FindElementInRoot<Button>(instance, "remove-button");

            adioClipField.value = currentClip;
            adioClipField.RegisterValueChangedCallback(evt =>
            {
                target.footstepAudioClips[clipIndex] = (AudioClip)evt.newValue;
                EditorUtility.SetDirty(target);
            });

            removeButton.clickable.clicked += () =>
            {
                RemoveAudioElement(target, instance, container, currentClip);
                listCount.text = $"{target.footstepAudioClips.Count} elements";
                EditorUtility.SetDirty(target);
            };

            container.Add(instance);
        }
        private void RemoveAudioElement(BaseLocomotionComponent target, VisualElement instance, VisualElement container, AudioClip clip)
        {
            target.footstepAudioClips.Remove(clip);
            container.Remove(instance);
        }
    }
#endif
}
