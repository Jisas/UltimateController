using UnityEngine;

namespace UltimateFramework.InventorySystem
{
    [System.Serializable]
    public class Slot
    {
        public SlotInfo SlotInfo { get; set; } = new();
        public virtual Slot GetSlot() => this;
        public virtual void OnSlotClicked() { }
    }
}
