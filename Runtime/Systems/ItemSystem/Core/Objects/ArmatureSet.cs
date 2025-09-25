using System.Collections.Generic;
using UltimateFramework.Utils;
using System;

namespace UltimateFramework.ItemSystem
{
    [Serializable] 
    public class ArmatureSet
    {
        public string name;
        public int index;
        public List<ArmaturePartInfo> armaturePartsInfo = new();

        [Serializable]
        public class ArmaturePartInfo
        {
            public ArmaturePartInfo(string name, int index, ItemType type)
            {
                this.partName = name;
                this.index = index;
                this.type = type;
            }

            [ReadOnly] public string partName;
            [ReadOnly] public int index;
            [ReadOnly] public ItemType type;
        }

        public ArmaturePartInfo FindArmatureParts(int index)
        {
            foreach (var part in armaturePartsInfo)
            {
                if (part.index == index)
                {
                    return part;
                }
            }
            return null;
        }
    }
}
