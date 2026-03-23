using UltimateFramework.ItemSystem;
using UltimateFramework.Tools;
using UnityEngine.UIElements;
using UnityEngine;

namespace UltimateFramework.InventorySystem
{
    [System.Serializable]
    public class InventorySlot : Slot, ISlotVisuals
    {
        public VisualElement itemImage;
        public Label amountText;

        public ItemDatabase ItemDB { get; set; }

        public void SetUp(int id)
        {
            SlotInfo = new()
            {
                id = id
            };

            SlotInfo.EmptySlot();
        }

        public void UpdateUI()
        {
            if (SlotInfo.isEmpty)
            {
                itemImage.style.backgroundImage = null;
                amountText.text = string.Empty;
            }
            else
            {
                var itemIcon = ItemDB.FindItem(SlotInfo.itemId).icon;
                itemImage.style.backgroundImage = new StyleBackground(itemIcon);

                if (SlotInfo.amount > 1)
                     amountText.text = SlotInfo.amount.ToString();
                else amountText.text = string.Empty;
            }
        }

        public override Slot GetSlot() => this;
        public override void OnSlotClicked()
        {
            throw new System.NotImplementedException();
        }

    }
}
