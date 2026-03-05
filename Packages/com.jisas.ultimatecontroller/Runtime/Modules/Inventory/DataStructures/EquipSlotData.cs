using System.Collections.Generic;
using UltimateController.Utils;
using UnityEngine;
using System;

namespace UltimateController.InventorySystem
{
    [Serializable]
    public class EquipSlotData
    {
        public bool selected = false;
        public bool useAmountText = false;
        public GameObject slotObject;
        public List<TagSelector> slotTags = new();
        public SocketOrientation orientation;
    }
}