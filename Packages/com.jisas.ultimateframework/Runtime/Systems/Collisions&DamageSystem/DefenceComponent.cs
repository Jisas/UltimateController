using System.Collections.Generic;
using UnityEngine;
using System;

namespace UltimateFramework.CollisionsAndDamageSystem
{
    public class DefenceComponent : MonoBehaviour
    {
        public TagSelector blockTag;
        public List<DefenceData> blockData;

        public bool IsBlocking { get; set; } = false;

        [Serializable]
        public class DefenceData
        {
            public TagSelector damageType;
            public float blockPercentage;
        }
    }
}