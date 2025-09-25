using UltimateFramework.ItemSystem;
using UltimateFramework.Utils;
using UnityEngine.InputSystem;

namespace UltimateFramework.InventorySystem
{
    public class AIInventoryAndEquipment : InventoryAndEquipmentComponent
    {
        protected override void LoadInventoryAndEquipment()
        {
            LoadEmptyInventoryAndEquipment();
        }
        protected override void LoadEmptyInventoryAndEquipment()
        {
            // Create and setup new invetory slots
            for (int i = 0; i < capacity; i++)
                CreateAndSetUpInventorySlots(i, true);

            // Setup the equipment slots of game objects
            for (int i = 0; i < equipSlotsData.Count; i++)
                CreateAndSetUpEquipmentSlots(i, equipSlotsData[i], true);

            RightSelectedSlot = FindSelectedEquipmentSlot(SocketOrientation.Right);
            LeftSelectedSlot = FindSelectedEquipmentSlot(SocketOrientation.Left);

            // Equip the starting items
            foreach (var startItem in startingItems)
            {
                Item currentItem = itemDatabase.FindItem(startItem.itemName);
                int itemID = itemDatabase.GetItemID(currentItem);

                if (useInventory) AddItem(itemID, startItem.amount);
                if (startItem.autoEquip) EquipStartingItems(currentItem, startItem.amount, startItem.equipOnBody);
            }
        }
        private void EquipStartingItems(Item currentItem, int amount, bool equipOnBody)
        {
            if (currentItem.type != ItemType.Weapon)
            {
                if (currentItem.type == ItemType.Consumable)
                    otherItemStartingStrategy.SetStartingEquip(this, currentItem, 3, amount);

                else if (currentItem.type == ItemType.Spell)
                    otherItemStartingStrategy.SetStartingEquip(this, currentItem, 2, amount);
            }
            else
            {
                for (int i = 0; i < amount; i++)
                {
                    IStartingEquipStrategy strategy = startingEquipStrategies[currentItem.hand];
                    strategy.SetStartingEquip(this, currentItem, i, equipOnbody: equipOnBody);
                }
            }
        }

        #region Callbacks
        private void SwitchRightSlot(InputAction.CallbackContext context)
        {
            if (context.performed)
                SwitchRightSelectedSlot();
        }
        private void SwitchLeftSlot(InputAction.CallbackContext context)
        {
            if (context.performed)
                SwitchLeftSelectedSlot();
        }
        #endregion
    }
}