using UltimateFramework.ItemSystem;
using System.Collections.Generic;
using UltimateFramework.Utils;
using UnityEngine.UIElements;
using UnityEngine;

namespace UltimateFramework.InventorySystem
{
    [System.Serializable]
    public class EquipmentSlot : Slot, ISlotVisuals
    {
        public bool selected = false;
        public bool useAmountText = false;
        public VisualElement itemImage;
        public Label amountText;
        public List<TagSelector> slotTags;
        public SocketOrientation orientation;
        [SerializeField, ReadOnly] private SocketType type;

        public ItemDatabase ItemDB { get; set; }
        public SocketType Type { get => type; }
        public void SetSocketType(SocketType newType) => type = newType;

        public override Slot GetSlot() => this;
        public void SetUp(int id)
        {
            SlotInfo = new()
            {
                id = id,
                maxAmount = 1
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

                if (SlotInfo.amount > 1 && useAmountText)
                    amountText.text = SlotInfo.amount.ToString();

                else if (SlotInfo.amount <= 1 && useAmountText)
                    amountText.text = string.Empty;
            }
        }
        public override void OnSlotClicked()
        {
            throw new System.NotImplementedException();
        }

    }
}
