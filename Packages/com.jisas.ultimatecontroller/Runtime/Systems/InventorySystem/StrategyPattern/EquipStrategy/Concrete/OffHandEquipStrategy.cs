using UltimateFramework.StatisticsSystem;
using UltimateFramework.ItemSystem;
using UltimateFramework.Utils;
using UnityEngine;
using System;

namespace UltimateFramework.InventorySystem
{
    public class OffHandEquipStrategy : IEquipWeaponStrategy
    {
        public void Equip(InventoryAndEquipmentComponent inventory, Item item, int socketIndex, bool equipOnBody, bool isSlotSwitching, SocketOrientation orientation)
        {
            GameObject itemObj = null;
            string bodySlot = item.bodySlot;
            string handSlot = item.handSlot;

            if (item.prefab != null)
            {
                string[] bodySockets = bodySlot.Split(',');
                if (socketIndex > bodySockets.Length - 1)
                {
                    throw new Exception
                        ($"The number of weapons must match the number of body slots, check the item: {item.name} " +
                        $"in the item database and make sure that the 'BodySlots' field contains more than one slot " +
                        $"name (slot names must be separated by a comma");
                }

                string concreteBodySocket = bodySockets[socketIndex];
                string concreteHandSocket = handSlot;

                if (inventory.LastEquippedWeapon == null || inventory.LastEquippedWeapon != item.prefab)
                {
                    Transform socket;

                    if (equipOnBody) socket = inventory.bodyBone.Find(concreteBodySocket.Trim());
                    else
                    {
                        if (inventory.rightHandBone.Find(concreteHandSocket) != null && inventory.rightHandBone.Find(concreteHandSocket).childCount <= 0)
                            socket = inventory.rightHandBone.Find(concreteHandSocket);
                        else socket = inventory.leftHandBone.Find(concreteHandSocket);
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
                    Transform bodyWeaponSocket = inventory.bodyBone.Find(concreteBodySocket.Trim());
                    Transform leftHandWeaponSocket = inventory.leftHandBone.Find(concreteHandSocket);

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
