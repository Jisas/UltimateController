using System.Collections.Generic;
using System;

namespace UltimateFramework.InventorySystem
{
    [Serializable]
    public class InventoryAndEquipmentData
    {
        public List<SlotInfo> inventorySlotInfoList;
        public List<SlotInfo> equipmentSlotInfoList;
        public SlotInfo rightSelectedSlotInfo;
        public SlotInfo leftSelectedSlotInfo;
        public SlotInfo bottomSelectedSlotInfo;
    }
}