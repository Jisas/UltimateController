using System.Collections.Generic;
using UnityEngine;

namespace UltimateController.SerializationSystem
{
    [System.Serializable]
    public class PlayerData
    {
        public int level;
        public int coins;
        public SerializableVector3 position;
        public SerializableQuaternion rotation;
    }
}
