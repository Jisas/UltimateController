using UltimateFramework.ItemSystem;
using UltimateFramework.Utils;

namespace UltimateFramework.InventorySystem
{
    public class AIInventoryAndEquipment : InventoryAndEquipmentComponent
    {
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
                if (startItem.autoEquip) EquipStartingItems(currentItem, startItem.amount);
            }
        }
        private void EquipStartingItems(Item currentItem, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                if (currentItem.type != ItemType.Weapon)
                {
                    otherItemStartingStrategy.SetStartingEquip(this, currentItem, i, amount);
                    continue;
                }
                else
                {
                    WeaponHand weaponHand = currentItem.hand;
                    IStartingEquipStrategy strategy = startingEquipStrategies[weaponHand];
                    strategy.SetStartingEquip(this, currentItem, i);
                }
            }
        }
    }
}