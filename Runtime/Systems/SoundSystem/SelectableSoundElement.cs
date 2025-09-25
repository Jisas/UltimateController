using UnityEngine.EventSystems;
using UnityEngine;

namespace UltimateFramework.SoundSystem
{
    [RequireComponent (typeof(AudioSource))]
    public class SelectableSoundElement : MonoBehaviour, ISelectHandler
    {
        public TagSelector audioTag;
        private AudioSource m_Source;

        private void Awake()
        {
            m_Source = GetComponent<AudioSource>();
            string realTag = string.Empty;
            string[] tagPart = audioTag.tag.Split('.');
            if (tagPart[0].Contains("Sound")) realTag = tagPart[1];
            if (!string.IsNullOrEmpty(realTag)) SoundManager.SetRandomClip(ref m_Source, realTag);
        }

        public void OnSelect(BaseEventData eventData) => m_Source.Play();
    }
}
