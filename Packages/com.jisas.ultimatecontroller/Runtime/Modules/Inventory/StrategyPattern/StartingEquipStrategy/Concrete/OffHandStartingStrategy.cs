using UltimateController.ItemSystem;
using UltimateController.Utils;

namespace UltimateController.InventorySystem
{
    public class OffHandStartingStrategy : IStartingEquipStrategy
    {
        public void SetStartingEquip(InventoryAndEquipmentComponent inventory, Item item, int socketIndex, int amount = 1, bool equipOnbody = true)
        {
            inventory.EquipItem(item, 1, equipOnBody: equipOnbody);
        }
    }
}
