using UltimateFramework.ItemSystem;
using UltimateFramework.Utils;

namespace UltimateFramework.InventorySystem
{
    public class OneHandStartingStrategy : IStartingEquipStrategy
    {
        public void SetStartingEquip(InventoryAndEquipmentComponent inventory, Item item, int socketIndex, int amount = 1, bool equipOnbody = true)
        {
            inventory.EquipItem(item, socketIndex, equipOnBody: equipOnbody);
        }
    }
}
