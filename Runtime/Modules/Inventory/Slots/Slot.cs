using UnityEngine;

namespace UltimateFramework.InventorySystem
{
    [System.Serializable]
    public class Slot : MonoBehaviour
    {
        public SlotInfo SlotInfo { get; set; }
        public virtual Slot GetSlot() => this;
    }
}
