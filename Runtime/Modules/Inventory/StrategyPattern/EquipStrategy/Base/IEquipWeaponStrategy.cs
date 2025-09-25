using UltimateFramework.ItemSystem;
using UltimateFramework.Utils;

namespace UltimateFramework.InventorySystem
{
    public interface IEquipWeaponStrategy
    {
        void Equip(InventoryAndEquipmentComponent inventory, Item item, int socketIndex, bool equipOnBody, bool isSlotSwitching = default, SocketOrientation orientation = default);
    }
}
