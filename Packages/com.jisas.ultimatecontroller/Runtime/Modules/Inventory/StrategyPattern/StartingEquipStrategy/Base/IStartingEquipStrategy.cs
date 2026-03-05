using UltimateController.ItemSystem;
using UltimateController.Utils;

namespace UltimateController.InventorySystem
{
    public interface IStartingEquipStrategy
    {
        void SetStartingEquip(InventoryAndEquipmentComponent inventory, Item item, int socketIndex, int amount = 1, bool equipOnbody = true);
    }
}
