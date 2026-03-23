using UltimateFramework.Utils;
using UnityEngine;

namespace UltimateFramework.ItemSystem
{
    [System.Serializable]
    public class ItemUpgrade
    {
        public string name;
        public int index;
        [Space(10)]

        // Stat upgrade
        public bool useStatUpgrade;
        public string statTag;
        public float value;
        public OperationType opType;
        public ValueType valueType;
        public BaseOn baseOn;
        [Space(10)]

        // Scale upgrade
        public bool useScaleUpgrade;
        public int scaleToAffectIndex;
        public ScalingLevel newScaleLevel;
        [Space(10)]

        // Att Mod. upgrade
        public bool useAttModUpgrade;
        public int attModToAffectIndex;
        public float attModNewValue;
        [Space(10)]

        // Stat Mod. upgrade
        public bool useStatModUpgrade;
        public int statToAffectIndex;
        public float statModNewValue;
    }
}