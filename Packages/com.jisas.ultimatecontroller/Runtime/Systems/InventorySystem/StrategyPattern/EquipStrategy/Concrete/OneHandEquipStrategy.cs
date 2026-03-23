using UltimateFramework.StatisticsSystem;
using UltimateFramework.ItemSystem;
using UltimateFramework.Utils;
using UnityEngine;
using System;

namespace UltimateFramework.InventorySystem
{
    public class OneHandEquipStrategy : IEquipWeaponStrategy
    {
        public void Equip(InventoryAndEquipmentComponent inventory, Item item, int socketIndex, bool equipOnBody, bool isSlotSwitching, SocketOrientation orientation)
        {
            GameObject itemObj = null;
            string bodySlot = item.bodySlot;
            string handSlot = item.handSlot;

            if (item.prefab != null)
            {
                string[] bodySlots = bodySlot.Split(',');
                if (socketIndex > bodySlots.Length - 1)
                {
                    throw new Exception
                        ($"The number of weapons must match the number of body slots, check the item: {item.name} " +
                        $"in the item database and make sure that the 'BodySlots' field contains more than one slot " +
                        $"name (slot names must be separated by a comma");
                }
                string concreteBodySocket = bodySlots[socketIndex];

                if (inventory.LastEquippedWeapon == null || inventory.LastEquippedWeapon != item.prefab)
                {
                    Transform socket;

                    if (equipOnBody) socket = inventory.bodyBone.Find(concreteBodySocket.Trim());
                    else
                    {
                        if (isSlotSwitching)
                        {
                            socket = orientation == SocketOrientation.Right ?
                                    inventory.rightHandBone.Find(handSlot) :
                                    inventory.leftHandBone.Find(handSlot);
                        }
                        else
                        {
                            if (inventory.rightHandBone.Find(handSlot) != null && inventory.rightHandBone.Find(handSlot).childCount <= 0)
                                socket = inventory.rightHandBone.Find(handSlot);
                            else socket = inventory.leftHandBone.Find(handSlot);
                        }
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
                    Transform rightHandWeaponSocket = inventory.rightHandBone.Find(handSlot);
                    Transform leftHandWeaponSocket = inventory.leftHandBone.Find(handSlot);

                    if (equipOnBody)
                    {
                        if (inventory.RightWeapon.BodySocket == null)
                            inventory.SetupRightWeapon(itemObj, bodyWeaponSocket, rightHandWeaponSocket, weaponComponent);
                        else inventory.SetupLeftWeapon(itemObj, bodyWeaponSocket, leftHandWeaponSocket, weaponComponent);
                    }
                    else
                    {
                        inventory.EntityInputs.FindInputAction("EquipMelee").State = true;

                        if (inventory.RightWeapon.HandSocket == null)
                            inventory.SetupRightWeapon(itemObj, bodyWeaponSocket, rightHandWeaponSocket, weaponComponent);
                        else inventory.SetupLeftWeapon(itemObj, bodyWeaponSocket, leftHandWeaponSocket, weaponComponent);

                        if (itemBehaviour != null) inventory.BaseLocomotionComponent.SwitchLocomotionMap(itemBehaviour.itemName, true);
                    }
                }
            }
        }
    }
}
