using UltimateFramework.ItemSystem;

namespace UltimateFramework.InventorySystem
{
    public class OffHandStartingStrategy : IStartingEquipStrategy
    {
        public void SetStartingEquip(InventoryAndEquipmentComponent inventory, Item item, int equipSocketIndex, int amount, bool equipOnbody)
        {
            inventory.EquipItem(item, 1, equipOnBody: equipOnbody);
        }
    }
}
