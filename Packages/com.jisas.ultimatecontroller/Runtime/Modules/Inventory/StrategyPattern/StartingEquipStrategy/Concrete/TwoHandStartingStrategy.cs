using UltimateController.ItemSystem;
using UltimateController.Utils;
using UnityEngine;
using System;

namespace UltimateController.InventorySystem
{
    public class TwoHandStartingStrategy : IStartingEquipStrategy
    {
        public void SetStartingEquip(InventoryAndEquipmentComponent inventory, Item item, int socketIndex, int amount = 1, bool equipOnbody = true)
        {
            if (item.mainHand == MainHand.None)
                throw new Exception($"The weapon {item.name} is two handed weapon an not have asigned the 'MainWeapon' field");

            int slot = item.mainHand == MainHand.Right ? 0 : 1;

            for (int i = 0; i < 2; i++)
            {
                inventory.EquipItem(item, i, amount, equipOnbody, slot == (i + 1));
            }
        }
    }
}
