using System.Runtime.Serialization.Formatters.Binary;
using UltimateFramework.InventorySystem;
using UnityEngine;
using System.IO;
using MyBox;

namespace UltimateFramework.SerializationSystem
{
    public static class DataSerializer
    {
        public static void SavePlayer(PlayerData playerData)
        {
            BinaryFormatter formatter = new();
            string path = Application.persistentDataPath + "/player.data";
            FileStream stream = new(path, FileMode.Create);

            formatter.Serialize(stream, playerData);
            stream.Close();
        }
        public static PlayerData LoadPlayer()
        {
            string path = Application.persistentDataPath + "/player.data";

            if (File.Exists(path))
            {
                BinaryFormatter formatter = new();
                FileStream stream = new(path, FileMode.Open);

                PlayerData data = formatter.Deserialize(stream) as PlayerData;
                stream.Close();

                return data;
            }
            else
            {
                PlayerData data = new()
                {
                    coins = 0,
                    level = 0,
                    position = new(Vector3.zero),
                    rotation = new(Quaternion.identity),
                };

                return data;
            }
        }

        public static void SaveStatsAndAttributes(StatsAndAttributesData statsAndAttributesData)
        {
            BinaryFormatter formatter = new();
            string path = Application.persistentDataPath + "/statsandattributes.data";
            FileStream stream = new(path, FileMode.Create);

            formatter.Serialize(stream, statsAndAttributesData);
            stream.Close();
        }
        public static StatsAndAttributesData LoadStatsAndAttributes()
        {
            string path = Application.persistentDataPath + "/statsandattributes.data";

            if (File.Exists(path))
            {
                BinaryFormatter formatter = new();
                FileStream stream = new(path, FileMode.Open);

                StatsAndAttributesData data = formatter.Deserialize(stream) as StatsAndAttributesData;
                stream.Close();

                return data;
            }
            else
            {
                StatsAndAttributesData data = new()
                {
                    primaryAttributes = new(),
                    attributes = new(),
                    stats = new(),
                };

                return data;
            }
        }

        public static void SaveInventoryAndEquipment(InventoryAndEquipmentData inventoryAndEquipmentData)
        {
            BinaryFormatter formatter = new();
            string path = Application.persistentDataPath + "/inventory.data";
            FileStream stream = new(path, FileMode.Create);

            formatter.Serialize(stream, inventoryAndEquipmentData);
            stream.Close();
        }
        public static InventoryAndEquipmentData LoadInventoryAndEquipment()
        {
            string path = Application.persistentDataPath + "/inventory.data";

            if (File.Exists(path))
            {
                BinaryFormatter formatter = new();
                FileStream stream = new(path, FileMode.Open);

                InventoryAndEquipmentData data = formatter.Deserialize(stream) as InventoryAndEquipmentData;
                stream.Close();

                return data;
            }
            else
            {
                InventoryAndEquipmentData data = new()
                {
                    inventorySlotInfoList = new(),
                    equipmentSlotInfoList = new(),
                    rightSelectedSlotInfo = new(),
                    leftSelectedSlotInfo = new(),
                    bottomSelectedSlotInfo = new(),
                };

                return data;
            }
        }

        public static void SaveSettings(SettingsData settingsData)
        {
            string path = Application.persistentDataPath + "/settings.json";
            string json = JsonUtility.ToJson(settingsData);
            File.WriteAllText(path, json);
        }
        public static SettingsData LoadSettings()
        {
            string path = Application.persistentDataPath + "/settings.json";

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<SettingsData>(json);
            }
            else
            {
                Resolution[] resolutions = Screen.resolutions;
                Resolution currentResolution = Screen.currentResolution;
                int currentResolutionIndex = -1;

                foreach (var res in resolutions)
                {
                    if (res.width == currentResolution.width && res.height == currentResolution.height)
                        currentResolutionIndex = resolutions.IndexOfItem(res);
                }

                return new SettingsData()
                {
                    Quality = 0,
                    ScreenResolution = currentResolutionIndex,
                    ScreenMode = 0,
                    TextureResolution = 0,
                    ShadowQuality = 2,
                    ShadowResolution = 2,
                    RenderScale = 1,
                    FrameRate = 2,
                    Brightness = 1,
                    AntiAliasing = true,

                    GeneralVolume = 1,
                    MusicVolume = 1,
                    AmbientalVolume = 1,
                    EffectsVolume = 1,
                    UIVolume = 0.1f,
                };
            }
        }

        public static void DeleteAllGameData()
        {
            string playerfilePath = Application.persistentDataPath + "/player.data";
            if (File.Exists(playerfilePath))
            {
                File.Delete(playerfilePath);
                Debug.Log("The player data was deleted.");
            }

            string statsfilePath = Application.persistentDataPath + "/statsandattributes.data";
            if (File.Exists(statsfilePath))
            {
                File.Delete(statsfilePath);
                Debug.Log("The stats and attributes data was deleted.");
            }

            string inventoryfilePath = Application.persistentDataPath + "/inventory.data";
            if (File.Exists(inventoryfilePath))
            {
                File.Delete(inventoryfilePath);
                Debug.Log("The inventory and equipment data was deleted.");
            }

            string settingsfilePath = Application.persistentDataPath + "/settings.json";
            if (File.Exists(settingsfilePath))
            {
                File.Delete(settingsfilePath);
                Debug.Log("The settings data was deleted.");
            }
        }
    }
}
