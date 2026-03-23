using UltimateFramework.UISystem;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

namespace UltimateFramework.InventorySystem
{
    public class EquipmentVisuals : RuntimeVisualsBase
    {
        #region Serialized Fields
        [SerializeField] private InventoryAndEquipmentComponent inventory;
        #endregion

        #region Private Fields
        private readonly List<VisualElement> equipmentSlots = new();
        #endregion

        #region Mono
        private void OnEnable()
        {
            MenuVisualsExample.OpenEquipment += OnShow;
            inventory.OnEquipmentSlotVisuals += SetupEquipmentSlot;

            UIDoc.enabled = true;
            root = UIDoc.rootVisualElement;
            OnHide();
        }
        private void OnDisable()
        {
            MenuVisualsExample.OpenEquipment -= OnShow;
            inventory.OnEquipmentSlotVisuals -= SetupEquipmentSlot;
        }
        #endregion

        #region Internal
        public void SetupEquipmentSlot(EquipmentSlot newSlot, int newSlotIndex)
        {
            var slot = GetEquipmentSlot(newSlotIndex);

            if (slot != null)
            {
                newSlot.itemImage = FindElementInRoot<VisualElement>(slot, "icon");
                newSlot.amountText = FindElementInRoot<Label>(slot, "text");

                equipmentSlots.Add(slot);
            }
            else Debug.LogWarning($"The index element: {newSlotIndex} was not found, " +
                $"please make sure to assign a value to the 'Slot Index' property of " +
                $"the 'EquipmentSlotElement' custom element in the UI Builder window.");
        }
        private EquipmentSlotElement GetEquipmentSlot(int index)
        {
            List<EquipmentSlotElement> queriedElements = root.Query<EquipmentSlotElement>().ToList();

            foreach (EquipmentSlotElement element in queriedElements)
            {
                if (element.SlotIndex == index)
                    return element;
            }

            return null;
        }
        #endregion
    }
}