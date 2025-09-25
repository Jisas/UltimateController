using UltimateFramework.ItemSystem;
using System.Collections.Generic;
using UltimateFramework.Utils;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace UltimateFramework.InventorySystem
{
    [System.Serializable]
    public class EquipmentSlot : Slot, ISlotVisuals
    {
        public Image itemImage;
        public TextMeshProUGUI amountText;

        public delegate void OnUpdateFAUIHandler();
        public event OnUpdateFAUIHandler OnUpdateFAUI;

        public ItemDatabase ItemDB { get; set; }
        public bool Selected { get; set; } = false;
        public bool UseAmountText { get; set; } = false;
        public SocketOrientation Orientation { get; set; }
        public List<TagSelector> SlotTags { get; set; }
        public void SetSocketType(SocketType newType) => SlotInfo.type = newType;
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
                itemImage.enabled = false;  
                if (UseAmountText) amountText.gameObject.SetActive(false);
            }
            else
            {
                var itemIcon = ItemDB.FindItem(SlotInfo.itemId).icon;
                itemImage.sprite = itemIcon;
                itemImage.enabled = true;

                if (SlotInfo.amount > 1 && UseAmountText)
                {
                    amountText.gameObject.SetActive(true);
                    amountText.text = SlotInfo.amount.ToString();
                }

                else if (SlotInfo.amount <= 1 && UseAmountText)
                {
                    amountText.text = string.Empty;
                    amountText.gameObject.SetActive(false);
                }
            }
        }
        public void UpdateFAUI() => OnUpdateFAUI?.Invoke();
    }
}
