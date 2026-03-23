using UltimateFramework.UISystem;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

namespace UltimateFramework.InventorySystem
{
    public class InventoryVisuals : RuntimeVisualsBase
    {
        #region Serialized Fields
        [SerializeField] private InventoryAndEquipmentComponent inventory;
        [SerializeField] private string slotsContainer = "inventory_slots_container";
        #endregion

        #region Private Values
        private readonly List<VisualElement> inventorySlots = new();
        private VisualTreeAsset inventorySlotTemplate;
        #endregion

        #region Mono
        private void OnEnable()
        {
            MenuVisualsExample.OpenInventory += OnShow;
            inventory.OnInventorySlotVisuals += GenerateAndSetupInventorySlot;

            UIDoc.enabled = true;
            root = UIDoc.rootVisualElement;
            LoadResources();
            OnHide();
        }
        private void OnDisable()
        {
            MenuVisualsExample.OpenInventory -= OnShow;
            inventory.OnInventorySlotVisuals -= GenerateAndSetupInventorySlot;
        }
        #endregion

        #region Internal
        private void LoadResources()
        {
            inventorySlotTemplate = Resources.Load<VisualTreeAsset>("UI/UXML/InventorySlot");
        }
        public void GenerateAndSetupInventorySlot(InventorySlot newSlot)
        {
            var container = FindElementInRoot<VisualElement>(root, slotsContainer);
            var instance = inventorySlotTemplate.CloneTree();

            newSlot.itemImage = FindElementInRoot<VisualElement>(instance, "icon");
            newSlot.amountText = FindElementInRoot<Label>(instance, "text");

            inventorySlots.Add(instance);
            container.Add(instance);
        }
        #endregion
    }
}