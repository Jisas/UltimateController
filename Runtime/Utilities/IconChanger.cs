using UnityEditor;
using UnityEngine;

namespace UltimateFramework.Utils
{
#if UNITY_EDITOR
    public static class IconChanger
    {
        public static void ChangeIcon(string scriptPath, Texture2D icon)
        {
            MonoImporter importer = AssetImporter.GetAtPath(scriptPath) as MonoImporter;
            importer.SetIcon(icon);
            importer.SaveAndReimport();
        }
    }
#endif
}
