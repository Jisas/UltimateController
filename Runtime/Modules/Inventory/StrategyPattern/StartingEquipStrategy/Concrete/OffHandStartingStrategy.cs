using UltimateFramework.ItemSystem;
using UltimateFramework.Utils;

namespace UltimateFramework.InventorySystem
{
    public class OffHandStartingStrategy : IStartingEquipStrategy
    {
        public void SetStartingEquip(InventoryAndEquipmentComponent inventory, Item item, int socketIndex, int amount = 1, bool equipOnbody = true)
        {
            inventory.EquipItem(item, 1, equipOnBody: equipOnbody);
        }
    }
}
