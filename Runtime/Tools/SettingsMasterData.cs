using UltimateFramework.ItemSystem;
using UltimateFramework.Utils;
using UnityEditor;
using UnityEngine;

namespace UltimateFramework.Tools
{
    [CreateAssetMenu(fileName = "SettingsConfigData", menuName = "Ultimate Framework/Settings/Settings Data")]
    public class SettingsMasterData : ScriptableObject
    {
        private static SettingsMasterData _instance = null;
        public static SettingsMasterData Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<SettingsMasterData>("Data/Settings/SettingsConfigData");
                }
                return _instance;
            }
        }

        // Actions
        [Header("Actions")]
        public ActionsMasterData actionsMasterData;
        public PathUseAs actionGroupsPathSelection;
        public string actionGroupsPath;
        public PathUseAs actionIndividualsPathSelection;
        public string actionIndividualsPath;
        public PathUseAs actionsPathSelection;
        public string actionsScriptsPath;

        // Items
        [Header("Items")]
        public ItemDatabase itemDB;

        // States
        [Header("States")]
        public PathUseAs statesPathSelection;
        public string generalStatesScriptsPath;
        public string armorStateScriptsPath;
        public string weaponStateScriptsPath;
        public string consumableStateScriptsPath;

        // Preferences
        [Header("Preferences")]
        public bool showHelpingMessage = true;
        public bool useDiferentPaths = true;

#if UNITY_EDITOR
        public void SaveData()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            Debug.Log("Datos Guardados");
        }
#endif
    }
}