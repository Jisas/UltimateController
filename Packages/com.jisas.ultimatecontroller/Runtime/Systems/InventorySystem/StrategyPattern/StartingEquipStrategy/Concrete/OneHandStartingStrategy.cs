using UltimateFramework.ItemSystem;

namespace UltimateFramework.InventorySystem
{
    public class OneHandStartingStrategy : IStartingEquipStrategy
    {
        public void SetStartingEquip(InventoryAndEquipmentComponent inventory, Item item, int equipSocketIndex, int amonut, bool equipOnbody)
        {
            inventory.EquipItem(item, equipSocketIndex, equipOnBody: equipOnbody);
        }
    }
}
