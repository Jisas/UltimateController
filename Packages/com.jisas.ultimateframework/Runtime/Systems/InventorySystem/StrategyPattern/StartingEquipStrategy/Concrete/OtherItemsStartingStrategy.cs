using UltimateFramework.ItemSystem;

namespace UltimateFramework.InventorySystem
{
    internal class OtherItemsStartingStrategy : IStartingEquipStrategy
    {
        public void SetStartingEquip(InventoryAndEquipmentComponent inventory, Item item, int equipSocketIndex, int amount, bool equipOnbody)
        {
            inventory.EquipItem(item, 0, amount);
        }
    }
}
