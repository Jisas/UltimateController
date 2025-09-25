using UltimateFramework.Utils;

namespace UltimateFramework.InventorySystem
{
    [System.Serializable]
    public class SlotInfo
    {
        public bool isEmpty;
        public int id;
        public int itemId;
        public int amount;
        public int maxAmount = 20;
        public SocketType type;

        public void EmptySlot()
        {
            isEmpty = true;
            amount = 0;
            itemId = -1;
            type = SocketType.Body;
        }
    }
}
