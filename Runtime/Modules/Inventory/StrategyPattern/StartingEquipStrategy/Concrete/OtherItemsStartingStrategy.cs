using UltimateFramework.ItemSystem;
using UltimateFramework.Utils;

namespace UltimateFramework.InventorySystem
{
    internal class OtherItemsStartingStrategy : IStartingEquipStrategy
    {
        public void SetStartingEquip(InventoryAndEquipmentComponent inventory, Item item, int socketIndex, int amount = 1, bool equipOnbody = true)
        {
            item.SetAllValuesToBase();
            inventory.EquipItem(item, 3, amount);
        }
    }
}
