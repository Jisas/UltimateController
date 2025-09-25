using UltimateFramework.StatisticsSystem;
using UltimateFramework.ItemSystem;
using UltimateFramework.Utils;
using UnityEngine;
using System;

namespace UltimateFramework.InventorySystem
{
    public class TwoHandEquipStrategy : IEquipWeaponStrategy
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
                    Transform socket = null;

                    if (equipOnBody) socket = inventory.bodyBone.Find(bodySlot);
                    else
                    {
                        switch (item.mainHand)
                        {
                            case MainHand.Right:
                                socket = inventory.rightHandBone.Find(handSlot);
                                break;

                            case MainHand.Left:
                                socket = inventory.leftHandBone.Find(handSlot);
                                break;
                        }

                        inventory.SwitchEquipmentSlotType(Utils.SocketType.Hand);
                    }

                    itemObj = inventory.InstantiateItem(item.prefab, socket);
                    inventory.LastEquippedWeapon = itemObj;
                }

                if (itemObj != null)
                {
                    GameObject owner = inventory.gameObject;
                    ItemBehaviour itemBehaviour = itemObj.GetComponent<WeaponBehaviour>();
                    StatisticsComponent ownerStats = owner.GetComponent<StatisticsComponent>();

                    itemBehaviour.Owner = itemBehaviour != null ? owner : throw new NullReferenceException($"item itemName not found on {item.name}");
                    if (itemBehaviour != null && itemBehaviour.Item.Scaled.Count > 0) itemBehaviour.SetUpScaling(ownerStats);

                    WeaponComponent weaponComponent = itemObj.GetComponent<WeaponComponent>();
                    Transform bodyWeaponSocket = inventory.bodyBone.Find(bodySlot);
                    Transform rightHandWeaponSocket = inventory.rightHandBone.Find(handSlot);
                    Transform leftHandWeaponSocket = inventory.leftHandBone.Find(handSlot);

                    inventory.SetupRightWeapon(itemObj, bodyWeaponSocket, rightHandWeaponSocket, weaponComponent);
                    inventory.SetupLeftWeapon(itemObj, bodyWeaponSocket, leftHandWeaponSocket, weaponComponent);

                    if (!equipOnBody && isSlotSwitching)
                        if (itemBehaviour != null) inventory.BaseLocomotionComponent.SwitchLocomotionMap(itemBehaviour.itemName, true);
                }
            }              
        }
    }
}
