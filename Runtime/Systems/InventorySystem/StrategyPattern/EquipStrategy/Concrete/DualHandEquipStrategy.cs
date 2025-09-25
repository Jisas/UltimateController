using UltimateFramework.StatisticsSystem;
using UltimateFramework.ItemSystem;
using UltimateFramework.Utils;
using UnityEngine;
using System;


namespace UltimateFramework.InventorySystem
{
    public class DualHandEquipStrategy : IEquipWeaponStrategy
    {
        public void Equip(InventoryAndEquipmentComponent inventory, Item item, int socketIndex, bool equipOnBody, bool isSlotSwitching = default, SocketOrientation orientation = default)
        {
            GameObject itemObj_A = null;
            GameObject itemObj_B = null;
            string bodySlot = "BodyWeaponsHolder";
            string handSlot = item.handSlot;

            if (item.prefab != null)
            {
                if (inventory.LastEquippedWeapon == null || inventory.LastEquippedWeapon != item.prefab)
                {
                    if (equipOnBody)
                    {
                        var rightHandsocket = inventory.bodyBone.Find(bodySlot);
                        var leftHandsocket = inventory.bodyBone.Find(bodySlot);

                        itemObj_A = inventory.InstantiateItem(item.prefab, rightHandsocket);
                        itemObj_B = inventory.InstantiateItem(item.prefab, leftHandsocket);

                        inventory.LastEquippedWeapon = itemObj_A;
                    }
                    else
                    {
                        var rightHandsocket = inventory.rightHandBone.Find(handSlot);
                        var leftHandsocket = inventory.leftHandBone.Find(handSlot);

                        itemObj_A = inventory.InstantiateItem(item.prefab, rightHandsocket);
                        itemObj_B = inventory.InstantiateItem(item.prefab, leftHandsocket);

                        inventory.LastEquippedWeapon = itemObj_A;
                        inventory.SwitchEquipmentSlotType(Utils.SocketType.Hand);
                    }
                }

                if (itemObj_A != null && itemObj_B != null)
                {
                    GameObject owner = inventory.gameObject;
                    ItemBehaviour itemBehaviour_A = itemObj_A.GetComponent<WeaponBehaviour>();
                    ItemBehaviour itemBehaviour_B = itemObj_B.GetComponent<WeaponBehaviour>();
                    StatisticsComponent ownerStats = owner.GetComponent<StatisticsComponent>();

                    itemBehaviour_A.Owner = itemBehaviour_A != null ? owner : throw new NullReferenceException($"item behaviour not found on {item.name}");
                    if (itemBehaviour_A != null && itemBehaviour_A.Item.Scaled.Count > 0) itemBehaviour_A.SetUpScaling(ownerStats);

                    itemBehaviour_B.Owner = itemBehaviour_B != null ? owner : throw new NullReferenceException($"item behaviour not found on {item.name}");
                    if (itemBehaviour_B != null && itemBehaviour_B.Item.Scaled.Count > 0) itemBehaviour_B.SetUpScaling(ownerStats);

                    WeaponComponent weaponComponent_A = itemObj_A.GetComponent<WeaponComponent>();
                    WeaponComponent weaponComponent_B = itemObj_B.GetComponent<WeaponComponent>();
                    Transform bodyWeaponSocket = inventory.bodyBone.Find(bodySlot);
                    Transform rightHandWeaponSocket = inventory.rightHandBone.Find(handSlot);
                    Transform leftHandWeaponSocket = inventory.leftHandBone.Find(handSlot);

                    if (!equipOnBody)
                    {
                        var equipInput = inventory.EntityInputs.FindInputAction("EquipMelee");
                        if (equipInput != null) equipInput.State = true;
                    }

                    inventory.SetupRightWeapon(itemObj_A, bodyWeaponSocket, rightHandWeaponSocket, weaponComponent_A);
                    inventory.SetupLeftWeapon(itemObj_B, bodyWeaponSocket, leftHandWeaponSocket, weaponComponent_B);

                    if (itemBehaviour_A != null) inventory.BaseLocomotionComponent.SwitchLocomotionMap(itemBehaviour_A.itemName, true);
                }
            }
        }
    }
}
