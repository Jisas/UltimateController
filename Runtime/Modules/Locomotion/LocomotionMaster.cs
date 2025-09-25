using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UltimateFramework.LocomotionSystem
{
    [CreateAssetMenu(fileName = "LocomotionData", menuName = "Ultimate Framework/Systems/Locomotion/Data/LocomotionData")]
    public class LocomotionMaster : ScriptableObject
    {
        public List<LocomotionMap> locomotionMaps;
        private Dictionary<string, LocomotionMap> locomotionMapsDict;

        public void RegisterDictionary()
        {
            locomotionMapsDict = new Dictionary<string, LocomotionMap>();

            foreach (var map in locomotionMaps)
            {
                locomotionMapsDict.Add(map.name, map);
            }
        }
        public LocomotionMap FindMap(string name)
        {
            if (locomotionMapsDict.TryGetValue(name, out LocomotionMap map))
            {
                return map;
            }

            return null;
        }
        public LocomotionOverrideLayer FindOverrideLayer(LocomotionMovementStructure structure, string name)
        {
            foreach (var layer in structure.overrideLayers)
            {
                if (layer.name == name)
                    return layer;
            }
            return null;
        }

#if UNITY_EDITOR
        public void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            Debug.Log("Datos Guardados");
        }
#endif
    }
}
