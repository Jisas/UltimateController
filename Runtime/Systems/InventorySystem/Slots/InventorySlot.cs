using UltimateFramework.ItemSystem;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

namespace UltimateFramework.InventorySystem
{
    [Serializable]
    public class InventorySlotEvent : UnityEvent<InventorySlot> { }

    [System.Serializable]
    public class InventorySlot : Slot, ISlotVisuals, ISelectHandler
    {
        public Image itemImage;
        public TextMeshProUGUI amountText;
        [Space,SerializeField] private InventorySlotEvent onSelectEvent;

        public ItemDatabase ItemDB { get; set; }
        public InventorySlotEvent OnSelectEvent { get => onSelectEvent; }

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
                itemImage.sprite = null;
                amountText.text = string.Empty;
                itemImage.gameObject.SetActive(false);
                amountText.gameObject.SetActive(false);
            }
            else
            {
                var itemIcon = ItemDB.FindItem(SlotInfo.itemId).icon;
                itemImage.gameObject.SetActive(true);
                itemImage.sprite = itemIcon;

                if (SlotInfo.amount > 1)
                {
                    amountText.text = SlotInfo.amount.ToString();
                    amountText.gameObject.SetActive(true);
                }
                else
                {
                    amountText.text = string.Empty;
                    amountText.gameObject.SetActive(false);
                }
            }
        }
        public void ObtainSelectionFocus()
        {
            EventSystem.current.SetSelectedGameObject(this.gameObject);
            onSelectEvent?.Invoke(this);
        }
        public override Slot GetSlot() => this;

        public void OnSelect(BaseEventData eventData)
        {
            onSelectEvent?.Invoke(this);
        }
    }
}
