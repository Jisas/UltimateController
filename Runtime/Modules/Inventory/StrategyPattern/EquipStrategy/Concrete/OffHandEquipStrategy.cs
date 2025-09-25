using UltimateFramework.StatisticsSystem;
using UltimateFramework.ItemSystem;
using UltimateFramework.Utils;
using UnityEngine;
using System;

namespace UltimateFramework.InventorySystem
{
    public class OffHandEquipStrategy : IEquipWeaponStrategy
    {
        public void Equip(InventoryAndEquipmentComponent inventory, Item item, int socketIndex, bool equipOnBody, bool isSlotSwitching = default, SocketOrientation orientation = default)
        {
            GameObject itemObj = null;
            string bodySlot = "BodyWeaponsHolder";
            string handSlot = item.handSlot;

            if (item.prefab != null)
            {

                if (inventory.LastEquippedWeapon == null || inventory.LastEquippedWeapon != item.prefab)
                {
                    Transform socket;

                    if (equipOnBody) socket = inventory.bodyBone.Find(bodySlot);
                    else
                    {
                        if (inventory.rightHandBone.Find(handSlot) != null && inventory.rightHandBone.Find(handSlot).childCount <= 0)
                            socket = inventory.rightHandBone.Find(handSlot);
                        else socket = inventory.leftHandBone.Find(handSlot);
                    }

                    itemObj = inventory.InstantiateItem(item.prefab, socket);
                    inventory.LastEquippedWeapon = itemObj;
                }

                if (itemObj != null)
                {
                    GameObject owner = inventory.gameObject;
                    ItemBehaviour itemBehaviour = itemObj.GetComponent<WeaponBehaviour>();
                    StatisticsComponent ownerStats = owner.GetComponent<StatisticsComponent>();

                    itemBehaviour.Owner = itemBehaviour != null ? owner : throw new NullReferenceException($"item behaviour not found on {item.name}");
                    if (itemBehaviour != null && itemBehaviour.Item.Scaled.Count > 0) itemBehaviour.SetUpScaling(ownerStats);

                    WeaponComponent weaponComponent = itemObj.GetComponent<WeaponComponent>();
                    Transform bodyWeaponSocket = inventory.bodyBone.Find(bodySlot);
                    Transform leftHandWeaponSocket = inventory.leftHandBone.Find(handSlot);

                    if (equipOnBody)
                    {
                        if (inventory.LeftWeapon.BodySocket == null)
                            inventory.SetupLeftWeapon(itemObj, bodyWeaponSocket, leftHandWeaponSocket, weaponComponent);
                    }
                    else
                    {
                        if (inventory.LeftWeapon.HandSocket == null)
                            inventory.SetupLeftWeapon(itemObj, bodyWeaponSocket, leftHandWeaponSocket, weaponComponent);
                    }
                }
            }
        }
    }
}
