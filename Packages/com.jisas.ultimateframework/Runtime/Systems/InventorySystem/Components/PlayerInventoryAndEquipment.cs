using UltimateFramework.ItemSystem;
using UltimateFramework.Inputs;
using UltimateFramework.Utils;
using UnityEngine.InputSystem;
using UnityEngine;

namespace UltimateFramework.InventorySystem
{
    public class PlayerInventoryAndEquipment : InventoryAndEquipmentComponent
    {
        private void OnEnable()
        {
            InputsManager.Player.SwitchRigthWeapon.performed += SwitchRightSlot;
            InputsManager.Player.SwitchLeftWeapon.performed += SwitchLeftSlot;
        }
        private void OnDisable()
        {
            InputsManager.Player.SwitchRigthWeapon.performed -= SwitchRightSlot;
            InputsManager.Player.SwitchLeftWeapon.performed -= SwitchLeftSlot;
        }

        protected override void LoadEmptyInventoryAndEquipment()
        {
            // Create and setup new invetory slots
            for (int i = 0; i < capacity; i++)
                CreateAndSetUpInventorySlots(i, true);

            // Setup the equipment slots of game objects
            for (int i = 0; i < equipSlotsData.Count; i++)
                CreateAndSetUpEquipmentSlots(i, equipSlotsData[i], true);

            RightSelectedSlot = FindSelectedEquipmentSlot(SocketOrientation.Right);
            LeftSelectedSlot = FindSelectedEquipmentSlot(SocketOrientation.Left);

            // Equip the starting items
            foreach (var startItem in startingItems)
            {
                Item currentItem = itemDatabase.FindItem(startItem.itemName);
                int itemID = itemDatabase.GetItemID(currentItem);

                if (useInventory) AddItem(itemID, startItem.amount);
                if (startItem.autoEquip) EquipStartingItems(currentItem, startItem);
            }
        }
        protected override void LoadSaveInventoryAndEquipment()
        {
            jsonString = PlayerPrefs.GetString("inventory");
            InventoryAndEquipmentWrapper inventoryWrapper = JsonUtility.FromJson<InventoryAndEquipmentWrapper>(jsonString);
            inventorySlotInfoList = inventoryWrapper.inventorySlotInfoList;
            equipmentSlotInfoList = inventoryWrapper.equipmentSlotInfoList;
            SetSavedInventory();
            SetSavedEquipment(inventoryWrapper);
        }

        private void EquipStartingItems(Item currentItem, StartingItems startItem)
        {
            for (int i = 0; i < startItem.amount; i++)
            {
                if (currentItem.type != ItemType.Weapon)
                {
                    otherItemStartingStrategy.SetStartingEquip(this, currentItem, i, startItem.amount);
                    continue;
                }
                else
                {
                    WeaponHand weaponHand = currentItem.hand;
                    IStartingEquipStrategy strategy = startingEquipStrategies[weaponHand];
                    strategy.SetStartingEquip(this, currentItem, i, equipOnbody: startItem.equipOnBody);
                }              
            }
        }
        private void SetSavedInventory()
        {
            for (int i = 0; i < capacity; i++)
            {
                InventorySlot newSlot = CreateAndSetUpInventorySlots(i, false);
                newSlot.SlotInfo = inventorySlotInfoList[i];
                newSlot.UpdateUI();
            }
        }
        private void SetSavedEquipment(InventoryAndEquipmentWrapper inventoryWrapper)
        {
            for (int i = 0; i < equipSlotsData.Count; i++)
            {
                EquipmentSlot newSlot = CreateAndSetUpEquipmentSlots(i, equipSlotsData[i], false);
                newSlot.SlotInfo = equipmentSlotInfoList[i];
                if (useEquipmentUI) newSlot.UpdateUI();

                RightSelectedSlot = inventoryWrapper.rightSelectedSlot;
                LeftSelectedSlot = inventoryWrapper.leftSelectedSlot;
                RightSelectedSlot.SlotInfo = inventoryWrapper.rightSelectedSlotInfo;
                LeftSelectedSlot.SlotInfo = inventoryWrapper.leftSelectedSlotInfo;

                if (!newSlot.SlotInfo.isEmpty)
                {
                    int itemID = newSlot.SlotInfo.itemId;
                    Item item = itemDatabase.FindItem(itemID);
                    int socketIndex = GetSocketIndex(itemID, newSlot);
                    int amount = newSlot.SlotInfo.amount;
                    bool onBody = newSlot.Type == SocketType.Body;

                    EquipItem(item, socketIndex, amount, onBody, false, true, newSlot.SlotInfo);
                }
            }
        }

        #region Callbacks
        private void SwitchRightSlot(InputAction.CallbackContext context)
        {
            if (context.performed)
                SwitchRightSelectedSlot();
        }
        private void SwitchLeftSlot(InputAction.CallbackContext context)
        {
            if (context.performed)
                SwitchLeftSelectedSlot();
        }
        #endregion
    }
}