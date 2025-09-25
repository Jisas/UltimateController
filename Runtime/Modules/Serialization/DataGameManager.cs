using UltimateFramework.StatisticsSystem;
using UltimateFramework.InventorySystem;
using UltimateFramework.SoundSystem;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Collections;

namespace UltimateFramework.SerializationSystem
{
    public class DataGameManager : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private PlayerData playerData;
        [Space(), SerializeField] private StatsAndAttributesData statsAndAttributesData;
        [Space(), SerializeField] private InventoryAndEquipmentData inventoryAndEquipmentData;
        [Space(), SerializeField] private SettingsData settingsData;
        #endregion

        #region Static Instance
        public static DataGameManager Instance { get; private set; }
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
        }
        private void Start()
        {
            LoadPlayerData();
            LoadStatsAndAttributerData();
            LoadInventoryAndEquipmentData();
            LoadSettingsData();
            ApplySettingsByData();
        }
        #endregion

        #region Player Data
        public PlayerData GetPlayerData() => playerData;
        public static bool IsPlayerDataSaved()
        {
            string path = Application.persistentDataPath + "/player.data";
            return File.Exists(path);
        }

        [ContextMenu("SavePlayerData")]
        public void SavePlayerData() => DataSerializer.SavePlayer(playerData);
        public void LoadPlayerData() => playerData = DataSerializer.LoadPlayer();
        public void SetCoins(int coins) => playerData.coins = coins;
        public void SetLevel(int level) => playerData.level = level;
        public void SetPosition(Vector3 position) => playerData.position = new(position);
        public void SetRotation(Quaternion rotation) => playerData.rotation = new(rotation);
        #endregion

        #region Stats and Attributes Data
        public StatsAndAttributesData GetStatsAndAttributesData() => statsAndAttributesData;
        public static bool IsStatsAndAttributesDataSaved()
        {
            string path = Application.persistentDataPath + "/statsandattributes.data";
            return File.Exists(path);
        }
        public void SaveStatsAndAttributesData() => DataSerializer.SaveStatsAndAttributes(statsAndAttributesData);
        public void LoadStatsAndAttributerData() => statsAndAttributesData = DataSerializer.LoadStatsAndAttributes();
        public void SetPrimaryAttributes(List<AttributeData> primaryAttributes) => statsAndAttributesData.primaryAttributes = primaryAttributes;
        public void SetOtherAttributes(List<AttributeData> otherAttributes) => statsAndAttributesData.attributes = otherAttributes;
        public void SetStats(List<Statistic> stats) => statsAndAttributesData.stats = stats;
        #endregion

        #region Inventory And Equipment Data
        public InventoryAndEquipmentData GetInventoryAndEquipmentData() => inventoryAndEquipmentData;

        [ContextMenu("SaveInventoryData")]
        public void SaveInventoryAndEquipmentData() => DataSerializer.SaveInventoryAndEquipment(inventoryAndEquipmentData);
        public void LoadInventoryAndEquipmentData() => inventoryAndEquipmentData = DataSerializer.LoadInventoryAndEquipment();
        public static bool IsInventoryAndEquipmentDataSaved()
        {
            string path = Application.persistentDataPath + "/inventory.data";
            return File.Exists(path);
        }

        public void SetInventorySlotInfoList(List<SlotInfo> inventorySlotInfoList) => inventoryAndEquipmentData.inventorySlotInfoList = inventorySlotInfoList;
        public void SetEquipmentSlotInfoList(List<SlotInfo> equipmentSlotInfoList) => inventoryAndEquipmentData.equipmentSlotInfoList = equipmentSlotInfoList;
        public void SetRightSelectedSlotInfo(SlotInfo rightSelectedSlotInfo) => inventoryAndEquipmentData.rightSelectedSlotInfo = rightSelectedSlotInfo;
        public void SetLeftSelectedSlotInfo(SlotInfo leftSelectedSlotInfo) => inventoryAndEquipmentData.leftSelectedSlotInfo = leftSelectedSlotInfo;
        public void SetBottomSelectedSlotInfo(SlotInfo bottomSelectedSlotInfo) => inventoryAndEquipmentData.bottomSelectedSlotInfo = bottomSelectedSlotInfo;
        #endregion

        #region Settings Data
        public SettingsData GetSettingsData() => settingsData;

        [ContextMenu("SaveSettingsData")]
        public void SaveSettingsData() => DataSerializer.SaveSettings(settingsData);
        public void LoadSettingsData() => settingsData = DataSerializer.LoadSettings();
        public static bool IsSettingsDataSaved()
        {
            string path = Application.persistentDataPath + "/settings.json";
            return File.Exists(path);
        }
        public void ApplySettingsByData()
        {
            Resolution[] resolutions = Screen.resolutions;
            UnityEngine.Resolution selectedResolution = resolutions[settingsData.ScreenResolution];

            UnityEngine.FullScreenMode selectedMode = FullScreenMode.FullScreenWindow;
            switch (settingsData.ScreenMode)
            {
                case 0:
                    selectedMode = FullScreenMode.ExclusiveFullScreen;
                    break;
                case 1:
                    selectedMode = FullScreenMode.FullScreenWindow;
                    break;
                case 2:
                    selectedMode = FullScreenMode.Windowed;
                    break;
            }

            UnityEngine.ShadowQuality selectedShadowQuality = UnityEngine.ShadowQuality.Disable;
            switch (settingsData.ShadowQuality)
            {
                case 0:
                    selectedShadowQuality = UnityEngine.ShadowQuality.Disable;
                    break;
                case 1:
                    selectedShadowQuality = UnityEngine.ShadowQuality.HardOnly;
                    break;
                case 2:
                    selectedShadowQuality = UnityEngine.ShadowQuality.All;
                    break;
            }

            UnityEngine.ShadowResolution selectedShadowResolution = UnityEngine.ShadowResolution.Low;
            switch (settingsData.ShadowResolution)
            {
                case 0:
                    selectedShadowResolution = UnityEngine.ShadowResolution.Low;
                    break;
                case 1:
                    selectedShadowResolution = UnityEngine.ShadowResolution.Medium;
                    break;
                case 2:
                    selectedShadowResolution = UnityEngine.ShadowResolution.High;
                    break;
                case 3:
                    selectedShadowResolution = UnityEngine.ShadowResolution.VeryHigh;
                    break;
            }

            int frameRate = settingsData.FrameRate == 0 ? 30 :
                        settingsData.FrameRate == 1 ? 60 :
                        settingsData.FrameRate == 2 ? 120 : 30;

            QualitySettings.SetQualityLevel(settingsData.Quality);
            Screen.fullScreenMode = selectedMode;
            Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreenMode);
            QualitySettings.shadows = selectedShadowQuality;
            QualitySettings.shadowResolution = selectedShadowResolution;
            QualitySettings.globalTextureMipmapLimit = settingsData.TextureResolution;
            QualitySettings.antiAliasing = settingsData.AntiAliasing ? 2 : 0;
            Application.targetFrameRate = frameRate;
            Screen.brightness = settingsData.Brightness;

            SoundManager.Instance.SetGeneralVolume();
            SoundManager.Instance.SetMusicVolume();
            SoundManager.Instance.SetMusicVolume();
            SoundManager.Instance.SetEffectsVolume();
            SoundManager.Instance.SetUIEffectsVolume();
        }

        #region Graphics
        public void SetQuality(int value) => settingsData.Quality = value;
        public void SetScreenResolution(int value) => settingsData.ScreenResolution = value;
        public void SetScreenMode(int value) => settingsData.ScreenMode = value;
        public void SetTextureResolution(int value) => settingsData.TextureResolution = value;
        public void SetShadowQuality(int value) => settingsData.ShadowQuality = value;
        public void SetShadowResolution(int value) => settingsData.ShadowResolution = value;
        public void SetRenderScale(float value) => settingsData.RenderScale = value;
        public void SetFrameRate(int value) => settingsData.FrameRate = value;
        public void SetBrightness(float value) => settingsData.Brightness = value;
        public void SetAntiAliasing(bool value) => settingsData.AntiAliasing = value;
        #endregion

        #region Sound
        public void SetGeneralVolume(float value) => settingsData.GeneralVolume = value;
        public void SetMusicVolume(float value) => settingsData.MusicVolume = value;
        public void SetEffectsVolume(float value) => settingsData.EffectsVolume = value;
        public void SetAmbientalVolume(float value) => settingsData.AmbientalVolume = value;
        public void SetUIVolume(float value) => settingsData.UIVolume = value;
        #endregion

        #endregion

        #region General
        public static bool IsDataSaved()
        {
            if (IsPlayerDataSaved() || IsStatsAndAttributesDataSaved() ||IsInventoryAndEquipmentDataSaved())
                return true;

            return false;
        }

        [ContextMenu("DeleteAllData")]
        public void DeleteAllGameData() => DataSerializer.DeleteAllGameData();
        public void ExitGame()
        {
            SavePlayerData();
            SaveStatsAndAttributesData();
            SaveInventoryAndEquipmentData();
            SaveSettingsData();
            Application.Quit();
        }
        #endregion
    }
}
