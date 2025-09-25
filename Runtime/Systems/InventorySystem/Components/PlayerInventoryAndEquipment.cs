using UltimateFramework.SerializationSystem;
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
            m_EntityManager.OnPlayerDataSave += SaveInventoryAndEquipment;
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
            BottomSelectedSlot = FindSelectedEquipmentSlot(SocketOrientation.Bottom);

            // Equip the starting items
            foreach (var startItem in startingItems)
            {
                Item currentItem = itemDatabase.FindItem(startItem.itemName);
                int itemID = itemDatabase.GetItemID(startItem.itemName);

                if (useInventory) AddItem(itemID, startItem.amount);
                if (startItem.autoEquip) EquipStartingItems(currentItem, startItem);
            }
        }
        protected override void LoadSaveInventoryAndEquipment()
        {
            InventoryAndEquipmentData inventoryWrapper = DataGameManager.Instance.GetInventoryAndEquipmentData();
            inventorySlotInfoList = inventoryWrapper.inventorySlotInfoList;
            equipmentSlotInfoList = inventoryWrapper.equipmentSlotInfoList;
            SetSavedInventory();
            SetSavedEquipment(inventoryWrapper);
        }

        private void EquipStartingItems(Item currentItem, StartingItems startItems)
        {
            if (currentItem.type != ItemType.Weapon)
            {
                if (currentItem.type == ItemType.Consumable)
                    otherItemStartingStrategy.SetStartingEquip(this, currentItem, 3, startItems.amount);

                else if (currentItem.type == ItemType.Spell)
                    otherItemStartingStrategy.SetStartingEquip(this, currentItem, 2, startItems.amount);

                else 
                    otherItemStartingStrategy.SetStartingEquip(this, currentItem, 0, startItems.amount);
            }
            else
            {
                for (int i = 0; i < startItems.amount; i++)
                {
                    IStartingEquipStrategy strategy = startingEquipStrategies[currentItem.hand];
                    strategy.SetStartingEquip(this, currentItem, i, equipOnbody: startItems.equipOnBody);
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
        private void SetSavedEquipment(InventoryAndEquipmentData inventoryWrapper)
        {
            for (int i = 0; i < equipSlotsData.Count; i++)
            {
                EquipmentSlot newSlot = CreateAndSetUpEquipmentSlots(i, equipSlotsData[i], false);
                newSlot.SlotInfo = equipmentSlotInfoList[i];

                if (useEquipmentUI) newSlot.UpdateUI();
                if (useFastAccessUI) newSlot.UpdateFAUI();

                if (inventoryWrapper.rightSelectedSlotInfo.id == i)
                {
                    RightSelectedSlot = FindEquipmentSlot(i);
                    RightSelectedSlot.SlotInfo = inventoryWrapper.rightSelectedSlotInfo;
                }

                if (inventoryWrapper.leftSelectedSlotInfo.id == i)
                {
                    LeftSelectedSlot = FindEquipmentSlot(i);
                    LeftSelectedSlot.SlotInfo = inventoryWrapper.leftSelectedSlotInfo;
                }

                if (inventoryWrapper.bottomSelectedSlotInfo.id == i)
                {
                    BottomSelectedSlot = FindEquipmentSlot(i);
                    BottomSelectedSlot.SlotInfo = inventoryWrapper.bottomSelectedSlotInfo;
                }

                if (!newSlot.SlotInfo.isEmpty)
                {
                    int itemID = newSlot.SlotInfo.itemId;
                    Item item = itemDatabase.FindItem(itemID);
                    item.SetAllValuesToBase();
                    int socketIndex = GetSocketIndex(itemID, newSlot);
                    int amount = newSlot.SlotInfo.amount;
                    bool onBody = newSlot.SlotInfo.type == SocketType.Body;

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