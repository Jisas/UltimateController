using UnityEngine;
using System;

namespace UltimateFramework.SerializationSystem
{
    [Serializable]
    public class SettingsData
    {
        [Header("Screen")]
        public int Quality;
        public int ScreenResolution;
        public int ScreenMode;
        public int TextureResolution;
        public int ShadowQuality;
        public int ShadowResolution;
        public float RenderScale;
        public int FrameRate;
        public float Brightness;
        public bool AntiAliasing;

        [Header("Sound")]
        public float GeneralVolume;
        public float MusicVolume;
        public float EffectsVolume;
        public float AmbientalVolume;
        public float UIVolume;
    }
}
