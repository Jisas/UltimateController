using UltimateController.ItemSystem;
using UltimateController.Utils;

namespace UltimateController.InventorySystem
{
    public interface IEquipWeaponStrategy
    {
        void Equip(InventoryAndEquipmentComponent inventory, Item item, int socketIndex, bool equipOnBody, bool isSlotSwitching = default, SocketOrientation orientation = default);
    }
}
