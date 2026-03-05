using UltimateController.ItemSystem;
using UltimateController.Utils;

namespace UltimateController.InventorySystem
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
