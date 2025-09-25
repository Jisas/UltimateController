using UltimateFramework.SerializationSystem;
using System.Collections.Generic;
using UltimateFramework.Tag;
using UnityEngine.Audio;
using UnityEngine;
using System.Linq;
using System;

namespace UltimateFramework.SoundSystem
{
    public class SoundManager : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private string keyWord = "Sound";
        [SerializeField] private SoundTrack[] soundTacks;
        #endregion

        #region Properties
        public static SoundManager Instance { get; private set; }
        public SoundsFadeManager CurrentZoneMusic { get; set; }
        public SoundsFadeManager LastZoneMusic { get; set; }
        public AudioSource CurrentMusicSource
        {
            get => CurrentZoneMusic.AudioSource;
            private set => CurrentMusicSource = value;
        }
        public AudioClip CurrentMusicClip 
        { 
            get => CurrentZoneMusic.AudioSource.clip; 
            private set => CurrentMusicClip = value; 
        }
        #endregion

        #region Private Fields
        private TagDataBase tagsDB;
        private AudioMixer m_SoundMixer;
        #endregion

        #region Mono
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);

            m_SoundMixer = Resources.Load<AudioMixer>("Data/Audio/AudioMixer");
        }
        #endregion

        #region Public Methods
        public void UpdateData()
        {
            tagsDB = Resources.Load<TagDataBase>("Data/Tags/TagData");

            List<string> newNames = new();

            foreach (var tag in tagsDB.tags)
            {
                string[] tagPart = tag.Split('.');

                if (tagPart[0].Contains(keyWord))
                    newNames.Add(tagPart[1]);
            }

            Array.Resize(ref soundTacks, newNames.Count);

            for (int i = 0; i < soundTacks.Length; i++)
                soundTacks[i].name = newNames[i];
        }
        public void SetGeneralVolume()
        {
            var generalVolume = Mathf.Log10(DataGameManager.Instance.GetSettingsData().GeneralVolume) * 20;
            m_SoundMixer.SetFloat("Volume", generalVolume);

            AudioListener.pause = (DataGameManager.Instance.GetSettingsData().GeneralVolume <= 0);
        }
        public void SetMusicVolume()
        {
            var musicVolume = Mathf.Log10(DataGameManager.Instance.GetSettingsData().MusicVolume) * 20;
            m_SoundMixer.SetFloat("MusicVolume", musicVolume);
        }
        public void SetAmbientalVolume()
        {
            var ambientalVolume = Mathf.Log10(DataGameManager.Instance.GetSettingsData().AmbientalVolume) * 20;
            m_SoundMixer.SetFloat("AmbientalVolume", ambientalVolume);
        }
        public void SetEffectsVolume()
        {
            var effectsVolume = Mathf.Log10(DataGameManager.Instance.GetSettingsData().EffectsVolume) * 20;
            m_SoundMixer.SetFloat("EffectsVolume", effectsVolume);
        }
        public void SetUIEffectsVolume()
        {
            var UIVolume = Mathf.Log10(DataGameManager.Instance.GetSettingsData().UIVolume) * 20;
            m_SoundMixer.SetFloat("UIVolume", UIVolume);
        }
        public static void SetRandomClip(ref AudioSource source, string name)
        {
            var track = Instance.soundTacks.FirstOrDefault(st => st.name == name);
            if (track != null)
            {
                var clips = track.clips;
                var randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
                source.clip = randomClip;
            }
        }       
        public static void SwitchBetweenZoneMusic(float fadeInDuration, float maxVolume)
        {
            Instance.StartCoroutine(Instance.LastZoneMusic.FadeOut(Instance.LastZoneMusic.AudioSource, 2, 0, true));
            Instance.StartCoroutine(Instance.CurrentZoneMusic.FadeIn(Instance.CurrentZoneMusic.AudioSource, fadeInDuration, maxVolume));
        }
        #endregion
    }
}

[Serializable]
public class SoundTrack
{
    [HideInInspector] public string name;
    public AudioClip[] clips;
}
