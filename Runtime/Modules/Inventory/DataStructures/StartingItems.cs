using System;

namespace UltimateFramework.InventorySystem
{
    [Serializable]
    public class StartingItems
    {
        public string itemName;
        public bool autoEquip;
        public bool equipOnBody = true;
        public float dropChancePercentage;
        public int amount;
    }
}
