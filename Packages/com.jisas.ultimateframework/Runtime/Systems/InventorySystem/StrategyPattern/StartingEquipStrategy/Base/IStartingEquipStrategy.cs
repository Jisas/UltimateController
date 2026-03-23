using UltimateFramework.ItemSystem;

namespace UltimateFramework.InventorySystem
{
    public interface IStartingEquipStrategy
    {
        void SetStartingEquip(InventoryAndEquipmentComponent inventory, Item item, int equipSocketIndex, int amount = 1, bool equipOnbody = true);
    }
}
