using UltimateFramework.StatisticsSystem;
using UltimateFramework.LocomotionSystem;
using UltimateFramework.ItemSystem;
using System.Collections.Generic;
using UltimateFramework.Inputs;
using UltimateFramework.Utils;
using UltimateFramework.Tools;
using UnityEngine;
using System;

namespace UltimateFramework.InventorySystem
{
    public abstract class InventoryAndEquipmentComponent : MonoBehaviour
    {
        #region SerializedFields
        [Header("Inventory")]
        public bool useInventory;
        [SerializeField, ConditionalField("useInventory", true)] 
        protected int capacity;
        [SerializeField, ReadOnly, ConditionalField("useInventory", true)]
        protected List<SlotInfo> inventorySlotInfoList = new();


        [Header("Equipment")]
        public bool useEquipmentUI;
        public Transform bodyBone;
        public Transform leftHandBone;
        public Transform rightHandBone;
        [SerializeField] protected List<StartingItems> startingItems;
        [SerializeField] protected List<EquipSlotData> equipSlotsData;
        [SerializeField, ReadOnly] protected List<SlotInfo> equipmentSlotInfoList;
        #endregion

        #region Properties
        public int Capacity 
        { 
            get => capacity; 
            set => capacity = value; 
        }
        public List<StartingItems> StartingItems 
        { 
            get => startingItems; 
        }
        public List<EquipSlotData> EquipSlotsData 
        { 
            get => equipSlotsData; 
        }
        public List<EquipmentSlot> EquipSlots
        {
            get => equipmentSlots;
        }
        public List<SlotInfo> EquipSlotsInfo 
        { 
            get => equipmentSlotInfoList; 
        }
        public List<EquipmentSlot> RightWeaponSlots
        {
            get => rightWeaponSlots;
        }
        public List<EquipmentSlot> LeftWeaponsSlots
        {
            get => leftWeaponsSlots;
        }
        public List<EquipmentSlot> ConsumableSlots
        {
            get => consumableSlots;
        }
        public EquipmentSlot RightSelectedSlot { get; set; }
        public EquipmentSlot LeftSelectedSlot { get; set; }
        public EquippedWeapon RightWeapon { get; set; } = new();
        public EquippedWeapon LeftWeapon { get; set; } = new();
        public GameObject LastEquippedWeapon 
        { 
            get => _lastEquippedWeapon; 
            set => _lastEquippedWeapon = value; 
        } 
        public GameObject EquipedHelment
        {
            get;
            private set;
        }
        public GameObject EquipedArmor 
        { 
            get; 
            private set; 
        }
        public GameObject EquipedGloves
        {
            get;
            private set;
        }
        public GameObject EquipedPants
        {
            get;
            private set;
        }
        public GameObject EquipedBoots
        {
            get;
            private set;
        }
        public BaseLocomotionComponent BaseLocomotionComponent { get; private set; }
        public EntityActionInputs EntityInputs { get => m_InputsManager; }
        #endregion

        #region Actions
        public Action<InventorySlot> OnInventorySlotVisuals;
        public Action<EquipmentSlot, int> OnEquipmentSlotVisuals;
        public Action OnEquipTwoHandWeapon;
        public Action OnUnequipTwoHandWeapon;
        #endregion

        #region Strategies
        //Starting Items
        protected Dictionary<WeaponHand, IStartingEquipStrategy> startingEquipStrategies;
        protected IStartingEquipStrategy otherItemStartingStrategy = new OtherItemsStartingStrategy();
        readonly IStartingEquipStrategy oneHandStartingStrategy = new OneHandStartingStrategy();
        readonly IStartingEquipStrategy twoHandStartingStrategy = new TwoHandStartingStrategy();
        readonly IStartingEquipStrategy offHandStaringStrategy = new OffHandStartingStrategy();

        //Eqiup
        Dictionary<WeaponHand, IEquipWeaponStrategy> equipStrategies;
        readonly IEquipWeaponStrategy oneHandStrategy = new OneHandEquipStrategy();
        readonly IEquipWeaponStrategy twoHandStrategy = new TwoHandEquipStrategy();
        readonly IEquipWeaponStrategy offHandStrategy = new OffHandEquipStrategy();
        #endregion

        #region ProtectedFields
        protected string jsonString;
        protected ItemDatabase itemDatabase;
        protected EntityActionInputs m_InputsManager;
        protected StatisticsComponent m_Statistics;
        protected SettingsMasterData m_MasterData;
        protected List<InventorySlot> invetorySlots = new();
        protected readonly List<InventorySlot> inventorySlots = new();
        protected readonly List<EquipmentSlot> equipmentSlots = new();
        protected List<EquipmentSlot> rightWeaponSlots;
        protected List<EquipmentSlot> leftWeaponsSlots;
        protected List<EquipmentSlot> consumableSlots;
        #endregion

        #region PrivateFields
        private GameObject _lastEquippedWeapon;
        private bool isSwitchingWeaponSlot = false;
        private int currentLeftSlotIndex = 0, 
                    currentRightSlotIndex = 0;
        #endregion

        #region Mono
        private void OnEnable()
        {
            OnEquipTwoHandWeapon += SetPlaceHolderForTwoHandedWeapon;
            OnUnequipTwoHandWeapon += RemovePlaceHolderForTwoHandedWeapon;
        }
        private void OnDisable()
        {
            OnEquipTwoHandWeapon -= SetPlaceHolderForTwoHandedWeapon;
            OnUnequipTwoHandWeapon -= RemovePlaceHolderForTwoHandedWeapon;
        }
        protected void Awake()
        {
            m_MasterData = Resources.Load<SettingsMasterData>("Data/Settings/SettingsConfigData");
            itemDatabase = m_MasterData.itemDB;

            startingEquipStrategies = new()
            {
                { WeaponHand.OneHand, oneHandStartingStrategy },
                { WeaponHand.TwoHand, twoHandStartingStrategy },
                { WeaponHand.OffHand, offHandStaringStrategy },
            };

            equipStrategies = new()
            {
                { WeaponHand.OneHand, oneHandStrategy },
                { WeaponHand.TwoHand, twoHandStrategy },
                { WeaponHand.OffHand, offHandStrategy }
            };
        }
        protected void Start()
        {
            BaseLocomotionComponent = transform.root.GetComponent<BaseLocomotionComponent>();
            m_Statistics = transform.root.GetComponent<StatisticsComponent>();
            m_InputsManager = transform.root.GetComponent<EntityActionInputs>();

            inventorySlotInfoList = new List<SlotInfo>();
            equipmentSlotInfoList = new List<SlotInfo>();

            LoadInventory();

            rightWeaponSlots = GetWeaponSlots(SocketOrientation.Right);
            leftWeaponsSlots = GetWeaponSlots(SocketOrientation.Left);
            consumableSlots = GetConsumableSlots();
        }
        #endregion

        #region Load And Save
        protected void LoadInventory()
        {
            if (PlayerPrefs.HasKey("inventory") == true)
                 LoadSaveInventoryAndEquipment();
            else LoadEmptyInventoryAndEquipment();
        }
        protected virtual void LoadEmptyInventoryAndEquipment() { }
        protected virtual void LoadSaveInventoryAndEquipment() { }
        protected void SaveInventoryAndEquipment()
        {
            InventoryAndEquipmentWrapper inventoryWrapper = new()
            {
                inventorySlotInfoList = inventorySlotInfoList,
                equipmentSlotInfoList = equipmentSlotInfoList,

                rightSelectedSlot = RightSelectedSlot,
                leftSelectedSlot = LeftSelectedSlot,

                rightSelectedSlotInfo = RightSelectedSlot.SlotInfo,
                leftSelectedSlotInfo = LeftSelectedSlot.SlotInfo
            };

            jsonString = JsonUtility.ToJson(inventoryWrapper);
            PlayerPrefs.SetString("inventory", jsonString);
        }
        protected class InventoryAndEquipmentWrapper
        {
            public List<SlotInfo> inventorySlotInfoList;
            public List<SlotInfo> equipmentSlotInfoList;
            public EquipmentSlot rightSelectedSlot;
            public EquipmentSlot leftSelectedSlot;
            public SlotInfo rightSelectedSlotInfo;
            public SlotInfo leftSelectedSlotInfo;
        }
        #endregion

        #region Create And Setup Slots
        protected InventorySlot CreateAndSetUpInventorySlots(int index, bool setList)
        {
            InventorySlot newSlot = new()
            {
                ItemDB = itemDatabase
            };

            newSlot.SetUp(index);
            inventorySlots.Add(newSlot);
            OnInventorySlotVisuals?.Invoke(newSlot);

            if (setList)
            {
                SlotInfo newSlotInfo = newSlot.SlotInfo;
                inventorySlotInfoList.Add(newSlotInfo);
            }

            return newSlot;
        }
        protected EquipmentSlot CreateAndSetUpEquipmentSlots(int index, EquipSlotData data, bool setList)
        {
            EquipmentSlot newSlot = new()
            {
                ItemDB = itemDatabase,
                selected = data.selected,
                useAmountText = data.useAmountText,
                slotTags = data.slotTags,
                orientation = data.orientation
            };

            newSlot.SetUp(index);
            equipmentSlots.Add(newSlot);
            OnEquipmentSlotVisuals?.Invoke(newSlot, index);

            if (setList)
            {
                SlotInfo newSlotinfo = newSlot.SlotInfo;
                equipmentSlotInfoList.Add(newSlotinfo);
            }

            return newSlot;
        }
        #endregion

        #region Inventory
        public void AddItem(int itemId)
        {
            Item item = itemDatabase.FindItem(itemId); //Buscar en la base de datos

            if (item != null)
            {
                SlotInfo slotInfo = FindSuitableSlotInInventory(itemId);

                if (slotInfo != null)
                {
                    slotInfo.amount++;
                    slotInfo.itemId = itemId;
                    slotInfo.isEmpty = false;
                    FindInvetorySlot(slotInfo.id).UpdateUI();
                }
            }
        }
        public void AddItem(int itemId, int ammount)
        {
            Item item = itemDatabase.FindItem(itemId); //Buscar en la base de datos

            if (item != null)
            {
                SlotInfo slotInfo = FindSuitableSlotInInventory(itemId);

                if (slotInfo != null)
                {
                    slotInfo.amount += ammount;
                    slotInfo.itemId = itemId;
                    slotInfo.isEmpty = false;
                    FindInvetorySlot(slotInfo.id).UpdateUI();
                }
            }
        }
        public void RemoveItem(int itemId, int ammount)
        {
            SlotInfo slotInfo = FindItemInInventory(itemId);

            if (slotInfo != null)
            {
                slotInfo.amount -= ammount;

                if (slotInfo.amount < 0) slotInfo.amount = 0;
                if (slotInfo.amount == 0) slotInfo.EmptySlot();

                FindInvetorySlot(slotInfo.id).UpdateUI();
            }
        }
        public SlotInfo FindItemInInventory(int itemId)
        {
            foreach (SlotInfo slotInfo in inventorySlotInfoList)
            {
                if (slotInfo.itemId == itemId && !slotInfo.isEmpty)
                {
                    return slotInfo;
                }
            }

            Debug.LogError("Item not found in inventory");
            return null;
        }
        protected SlotInfo FindSuitableSlotInInventory(int itemId)
        {
            foreach (SlotInfo slotInfo in inventorySlotInfoList)
            {
                if (slotInfo.itemId == itemId && slotInfo.amount < slotInfo.maxAmount && !slotInfo.isEmpty && itemDatabase.FindItem(itemId).IsStackable)
                {
                    return slotInfo;
                }
            }
            foreach (SlotInfo slotInfo in inventorySlotInfoList)
            {
                if (slotInfo.isEmpty)
                {
                    slotInfo.EmptySlot();
                    return slotInfo;
                }
            }

            Debug.LogError("Suitable slot not found in inventory");
            return null;
        }
        protected InventorySlot FindInvetorySlot(int id)
        {
            return inventorySlots[id];
        }
        #endregion

        #region Equipment
        public void EquipItem(Item item, int socketIndex, int ammount = 1, bool equipOnBody = true, bool removeOfInvetory = true, bool isSaveData = false, SlotInfo savedSlotInfo = default)
        {
            bool twoHandComprobement = item.type == ItemType.Weapon && item.hand == WeaponHand.TwoHand;
            var itemID = itemDatabase.GetItemID(item);

            if (useInventory && removeOfInvetory)
            {
                var inventoryItem = FindItemInInventory(itemID) ?? throw new Exception($"Item {item.name}, not found in inventory");
                RemoveItem(inventoryItem.itemId, 1);
            }

            SlotInfo slotInfo = null;
            if (!isSaveData)
            {
                SocketOrientation orientation = socketIndex == 0 ? SocketOrientation.Right : SocketOrientation.Left;

                if (removeOfInvetory) slotInfo = FindEmptySlotInEquipment(item.itemSlot, orientation);
                else slotInfo = twoHandComprobement ?
                        FindEmptySlotInEquipment(item.itemSlot, orientation) :
                        FindSuitableSlotInEquipment(itemID, orientation);

                if (slotInfo == null) return;
                slotInfo.amount += ammount;
                slotInfo.itemId = itemID;
                slotInfo.isEmpty = false;
                if (useEquipmentUI) FindEquipmentSlot(slotInfo.id).UpdateUI();
            }

            var currentSlotInfo = isSaveData ? savedSlotInfo : slotInfo;
            if (twoHandComprobement)
            {
                switch (item.mainHand)
                {
                    case MainHand.Right:
                        if (RightSelectedSlot != null && RightSelectedSlot.SlotInfo.id == currentSlotInfo.id)
                            ApplyEquipWeaponStrategy(item, socketIndex, equipOnBody);
                        break;

                    case MainHand.Left:
                        if (LeftSelectedSlot != null && LeftSelectedSlot.SlotInfo.id == currentSlotInfo.id)
                            ApplyEquipWeaponStrategy(item, socketIndex, equipOnBody);
                        break;
                }
            }
            else
            {
                if (RightSelectedSlot != null && RightSelectedSlot.SlotInfo.id == currentSlotInfo.id)
                    ApplyEquipWeaponStrategy(item, socketIndex, equipOnBody);

                if (LeftSelectedSlot != null && LeftSelectedSlot.SlotInfo.id == currentSlotInfo.id)
                    ApplyEquipWeaponStrategy(item, socketIndex, equipOnBody);
            }
        }
        private void ApplyEquipWeaponStrategy(Item item, int socketIndex, bool equipOnBody, bool isSlotSwitching = default, SocketOrientation orientation = default)
        {
            if (item != null && item.prefab != null && item.type == ItemType.Weapon)
            {
                WeaponHand weaponHand = item.hand;
                IEquipWeaponStrategy strategy = equipStrategies[weaponHand];
                strategy.Equip(this, item, socketIndex, equipOnBody, isSlotSwitching, orientation);
                item.ApplyScale(m_Statistics, true, false);
                ModifyAttributesByItem(item, true);
            }
        }
        public void UnequipItem(int itemID, int ammount)
        {
            SlotInfo slotInfo = FindSuitableSlotInEquipment(itemID);

            if (slotInfo != null)
            {
                slotInfo.amount -= ammount;

                if (slotInfo.amount < 0) slotInfo.amount = 0;
                if (slotInfo.amount == 0) slotInfo.EmptySlot();
                if (useEquipmentUI) FindEquipmentSlot(slotInfo.id).UpdateUI();
            }
        }
        public void UnequipWeapon(int itemID, int ammount, EquipmentSlot equipmentSlot)
        {
            if (equipmentSlot == null) return;

            equipmentSlot.SlotInfo.amount = Math.Max(0, equipmentSlot.SlotInfo.amount - ammount);
            if (equipmentSlot.SlotInfo.amount == 0) equipmentSlot.SlotInfo.EmptySlot();
            if (useEquipmentUI) FindEquipmentSlot(equipmentSlot.SlotInfo.id).UpdateUI();

            Item item = itemDatabase.FindItem(itemID);
            Transform socketTransform = GetSocketTransform(item, equipmentSlot);
            if (socketTransform == null) return;

            if (equipmentSlot.orientation == SocketOrientation.Right)
            {
                RightWeapon.BodySocket = null;
                RightWeapon.HandSocket = null;
            }
            else
            {
                LeftWeapon.BodySocket = null;
                LeftWeapon.HandSocket = null;
            }

            Destroy(socketTransform.GetChild(0).gameObject);

            if (equipmentSlot.Type == SocketType.Hand && equipmentSlot.orientation == SocketOrientation.Right)
                BaseLocomotionComponent.SwitchLocomotionMap(item.name);

            if (equipmentSlot.Type == SocketType.Body)
            {
                BaseLocomotionComponent.SwitchLocomotionMap("Unarmed");
                m_InputsManager.ResetInputActionState("EquipMelee");
            }
        }
        public void SwitchWeapon(int currentWeaponID, int newWeaponID, SocketOrientation orientation)
        {
            int ammount = 1;
            EquipmentSlot equipmentSlot = FindEquipmentSlot(currentWeaponID, orientation);
            int itemID = equipmentSlot.SlotInfo.itemId;

            UnequipWeapon(itemID, ammount, equipmentSlot);

            bool onBody = equipmentSlot.Type == SocketType.Body;
            int socketIndex = GetSocketIndex(newWeaponID, equipmentSlot);
            Item weapon = itemDatabase.FindItem(newWeaponID);

            EquipItem(weapon, socketIndex, equipOnBody: onBody);
            AddItem(itemID, ammount);
        }
        public void SwitchRightSelectedSlot(bool instanceWeaponObj = true)
        {
            RightWeapon.BodySocket = null;
            RightWeapon.HandSocket = null;
            RightSelectedSlot.selected = false;

            string tag = "";
            foreach (var slotTag in RightSelectedSlot.slotTags)
            {
                if (slotTag.tag == "ItemSlot.MeleeWeapon")
                    tag = slotTag.tag;
            }

            if (String.IsNullOrEmpty(tag)) throw new Exception("No slots were found with the tag: ItemSlot.MeleeWeapon");

            var oldItemObj = RightWeapon.WeaponObject;

            currentRightSlotIndex = (currentRightSlotIndex + 1) % rightWeaponSlots.Count;
            int slotID = rightWeaponSlots[currentRightSlotIndex].SlotInfo.id;
            var newSelecetedSlot = FindEquipmentSlot(slotID, SocketOrientation.Right, tag);

            RightSelectedSlot = newSelecetedSlot ?? throw new Exception("There are no more slots with weapons, only the one currently in use.");
            RightSelectedSlot.selected = true;

            var newItem = itemDatabase.FindItem(RightSelectedSlot.SlotInfo.itemId);
            if (newItem.hand == WeaponHand.TwoHand && !isSwitchingWeaponSlot)
            {
                isSwitchingWeaponSlot = true;
                SwitchLeftSelectedSlot(false);
                isSwitchingWeaponSlot = false;
            }

            if (instanceWeaponObj)
                ApplyEquipWeaponStrategy(newItem, (int)SocketOrientation.Right, RightSelectedSlot.Type == SocketType.Body, true, newSelecetedSlot.orientation);

            if (oldItemObj != null) Destroy(oldItemObj);
        }
        public void SwitchLeftSelectedSlot(bool instanceWeaponObj = true)
        {
            LeftWeapon.BodySocket = null;
            LeftWeapon.HandSocket = null;
            LeftSelectedSlot.selected = false;

            string tag = "";
            foreach (var slotTag in LeftSelectedSlot.slotTags)
            {
                if (slotTag.tag == "ItemSlot.MeleeWeapon")
                    tag = slotTag.tag;
            }

            if (String.IsNullOrEmpty(tag)) throw new Exception($"No slots were found with the tag: ItemSlot.MeleeWeapon on object: {gameObject.name}");

            var oldItemObj = LeftWeapon.WeaponObject;

            currentLeftSlotIndex = (currentLeftSlotIndex + 1) % leftWeaponsSlots.Count;
            int slotID = leftWeaponsSlots[currentLeftSlotIndex].SlotInfo.id;
            var newSelecetedSlot = FindEquipmentSlot(slotID, SocketOrientation.Left, tag);

            if (newSelecetedSlot == null)
            {
                Debug.LogWarning("There are no more slots with weapons, only the one currently in use.");
                return;
            }

            LeftSelectedSlot = newSelecetedSlot;
            LeftSelectedSlot.selected = true;

            var newItem = itemDatabase.FindItem(LeftSelectedSlot.SlotInfo.itemId);
            if (newItem.hand == WeaponHand.TwoHand && !isSwitchingWeaponSlot)
            {
                isSwitchingWeaponSlot = true;
                SwitchRightSelectedSlot(false);
                isSwitchingWeaponSlot = false;
            }

            if (instanceWeaponObj) 
                ApplyEquipWeaponStrategy(newItem, (int)SocketOrientation.Left, LeftSelectedSlot.Type == SocketType.Body, true, newSelecetedSlot.orientation);

            if (oldItemObj != null) Destroy(oldItemObj);
        }
        public Item GetItemInEquipment(string slotTag)
        {
            foreach (EquipmentSlot equipmentSlot in equipmentSlots)
            {
                foreach (var slot in equipmentSlot.slotTags)
                {
                    if (!equipmentSlot.SlotInfo.isEmpty && slotTag == slot.tag)
                    {
                        return itemDatabase.FindItem(equipmentSlot.SlotInfo.itemId);
                    }
                }
            }
            return null;
        }
        protected SlotInfo FindSuitableSlotInEquipment(int itemId)
        {
            foreach (EquipmentSlot equipmentSlot in equipmentSlots)
            {
                if (equipmentSlot.SlotInfo.itemId == itemId && !equipmentSlot.SlotInfo.isEmpty)
                {
                    return equipmentSlot.SlotInfo;
                }
            }
            return null;
        }
        protected SlotInfo FindSuitableSlotInEquipment(int itemId, SocketOrientation orientation)
        {
            Item item = itemDatabase.FindItem(itemId);

            foreach (EquipmentSlot equipmentSlot in equipmentSlots)
            {
                foreach (var slot in equipmentSlot.slotTags)
                {
                    if (!equipmentSlot.SlotInfo.isEmpty &&  equipmentSlot.orientation == orientation && item.itemSlot == slot.tag)
                    {
                        return equipmentSlot.SlotInfo;
                    }
                }
            }

            return null;
        }
        protected SlotInfo FindEmptySlotInEquipment(string slotTag, SocketOrientation orientation)
        {
            foreach (EquipmentSlot equipmentSlot in equipmentSlots)
            {
                foreach (var slot in equipmentSlot.slotTags)
                {
                    if (equipmentSlot.SlotInfo.isEmpty && equipmentSlot.orientation == orientation && slot.tag == slotTag)
                    {
                        return equipmentSlot.SlotInfo;
                    }
                }
            }

            return null;
        }
        protected EquipmentSlot FindEquipmentSlot(int slotID)
        {
            EquipmentSlot currentSlot;

            foreach (var slot in equipmentSlots)
            {
                currentSlot = slot.GetSlot() as EquipmentSlot;

                if (currentSlot.SlotInfo.id == slotID)
                {
                    return currentSlot;
                }
            }

            Debug.LogError("Equipment slot not found");
            return null;
        }
        protected EquipmentSlot FindEquipmentSlot(int itemID, SocketOrientation orientation)
        {
            EquipmentSlot currentSlot;

            foreach (var slot in equipmentSlots)
            {
                currentSlot = slot.GetSlot() as EquipmentSlot;

                if (currentSlot.SlotInfo.itemId == itemID && currentSlot.orientation == orientation)
                {
                    return currentSlot;
                }
            }
            return null;
        }
        protected EquipmentSlot FindEquipmentSlot(int slotID, SocketOrientation orientation, string itemSlotTag)
        {
            foreach (EquipmentSlot equipmentSlot in equipmentSlots)
            {
                foreach (var slot in equipmentSlot.slotTags)
                {
                    if (!equipmentSlot.SlotInfo.isEmpty &&
                        equipmentSlot.SlotInfo.id == slotID &&
                        equipmentSlot.orientation == orientation &&
                        slot.tag == itemSlotTag)
                    {
                        return equipmentSlot;
                    }
                }
            }

            return null;
        }
        protected EquipmentSlot FindSelectedEquipmentSlot(SocketOrientation socketOrientation)
        {
            foreach (var slot in equipmentSlots)
            {
                if (slot.selected && slot.orientation == socketOrientation)
                    return slot;
            }
            return null;
        }
        #endregion

        #region Internal
        protected int GetSocketIndex(int itemID, EquipmentSlot equipmentSlot)
        {
            Item item = itemDatabase.FindItem(itemID) ?? throw new Exception("Item not found");
            int socketIndex = -1;

            switch (item.hand)
            {
                case WeaponHand.OneHand:
                    socketIndex = equipmentSlot.orientation == SocketOrientation.Left ? 1 : 0;
                    break;

                case WeaponHand.TwoHand:
                    socketIndex = item.mainHand == MainHand.Right ? 0 : 1;
                    break;

                case WeaponHand.OffHand:
                    socketIndex = 1;
                    break;
            }

            return socketIndex;
        }
        protected Transform GetSocketTransform(Item item, EquipmentSlot equipmentSlot)
        {
            string[] socketNames;
            string socketName;
            Transform bone;

            switch (equipmentSlot.Type)
            {
                case SocketType.Body:
                    socketNames = item.bodySlot.Split(",");
                    socketName = socketNames[(int)equipmentSlot.orientation];
                    bone = bodyBone;
                    break;
                case SocketType.Hand:
                    socketName = item.handSlot;
                    bone = equipmentSlot.orientation == SocketOrientation.Right ? rightHandBone : leftHandBone;
                    break;
                default:
                    return null;
            }

            return bone.Find(socketName.Trim());
        }
        private List<EquipmentSlot> GetWeaponSlots(SocketOrientation orientation)
        {
            List<EquipmentSlot> weaponSlots = new();

            foreach (var equipmentSlot in equipSlotsData)
            {
                if (equipmentSlot.orientation == orientation)
                {
                    foreach (var tagSelector in equipmentSlot.slotTags)
                    {
                        if (tagSelector.tag.Contains("ItemSlot.MeleeWeapon"))
                        {
                            var index = equipSlotsData.IndexOf(equipmentSlot);
                            weaponSlots.Add(equipmentSlots[index]);
                        }
                    }
                }
            }

            return weaponSlots;
        }
        private List<EquipmentSlot> GetConsumableSlots()
        {
            List<EquipmentSlot> weaponSlots = new();

            foreach (var equipmentSlot in equipSlotsData)
            {
                foreach (var tagSelector in equipmentSlot.slotTags)
                {
                    if (tagSelector.tag.Contains("ItemSlot.Consumable"))
                    {
                        var index = equipSlotsData.IndexOf(equipmentSlot);
                        weaponSlots.Add(equipmentSlots[index]);
                    }
                }
            }

            return weaponSlots;
        }
        protected StatisticsComponent.Operation GetOperation(ItemAttributeModifier atributeModifier)
        {
            return atributeModifier.opType switch
            {
                OperationType.Sum => m_Statistics.SumOnCurrentValue,
                OperationType.Multiply => m_Statistics.MultiplyOnCurrentValue,
                _ => throw new InvalidOperationException("Invalid operation"),
            };
        }
        protected void ModifyAttributesByItem(Item item, bool isAdditiveOperation)
        {
            if (item.type == ItemType.Weapon || item.type == ItemType.Armor)
            {
                foreach (var attmod in item.AttributeModifiers)
                {
                    var attributeType = attmod.attributeType;
                    var valueType = attmod.valueType;
                    var value = attmod.startValue;
                    var currentOperation = GetOperation(attmod);
                    var isPercentage = valueType == UltimateFramework.Utils.ValueType.Percentage;

                    if (m_Statistics.primaryAttributes.Count > 0)
                    {
                        var pattr = m_Statistics.FindPrimaryAttribute(attributeType);

                        if (pattr != null)
                        {
                            pattr.CurrentValue = isAdditiveOperation ?
                                m_Statistics.ApplyModifyAttributesOrStatsOperation(currentOperation, pattr.CurrentValue, value, isPercentage) :
                                m_Statistics.ApplyModifyAttributesOrStatsOperation(m_Statistics.SubtractOnCurrentValue, pattr.CurrentValue, value, isPercentage);
                        }
                    }

                    if (m_Statistics.attributes.Count > 0)
                    {
                        var attr = m_Statistics.FindAttribute(attributeType);

                        if (attr != null)
                        {
                            attr.CurrentValue = isAdditiveOperation ?
                                m_Statistics.ApplyModifyAttributesOrStatsOperation(currentOperation, attr.CurrentValue, value, isPercentage) :
                                m_Statistics.ApplyModifyAttributesOrStatsOperation(m_Statistics.SubtractOnCurrentValue, attr.CurrentValue, value, isPercentage);
                        }
                    }
                }
            }
        }
        #endregion

        #region Public Methods
        public EquippedWeapon GetCurrentMainWeapon()
        {
            EquippedWeapon weapon = null;

            if (RightWeapon.WeaponObject != null) weapon = RightWeapon;
            else if (LeftWeapon.WeaponObject != null) weapon= LeftWeapon;

            return weapon;
        }
        public EquippedWeapon GetCurrentOffHandWeapon()
        {
            EquippedWeapon weapon = null;
            if (LeftWeapon.WeaponObject != null) weapon = LeftWeapon;

            return weapon;
        }
        public GameObject GetCurrentRightWeaponObject()
        {
            return RightWeapon.WeaponObject;
        }
        public GameObject GetCurrentLeftWeaponObject()
        {
            return LeftWeapon.WeaponObject;
        }
        public List<SlotInfo> GetAllWeaponInInventory()
        {
            List<SlotInfo> info = new();

            foreach (var slot in inventorySlots)
            {
                var currentItem = itemDatabase.FindItem(slot.SlotInfo.itemId);
                if (currentItem.type == ItemType.Weapon)
                    info.Add(slot.SlotInfo);
            }

            return info;
        }
        public List<SlotInfo> GetAllCosumablesInInventory()
        {
            List<SlotInfo> info = new();

            foreach (var slot in inventorySlots)
            {
                var currentItem = itemDatabase.FindItem(slot.SlotInfo.itemId);
                if (currentItem.type == ItemType.Consumable)
                    info.Add(slot.SlotInfo);
            }

            return info;
        }
        public GameObject InstantiateItem(GameObject original, Transform parent)
        {
            return Instantiate(original, parent);
        }
        public void SwitchEquipmentSlotType(SocketType newType)
        {
            foreach (var equipmentSlot in equipmentSlots)
                equipmentSlot.SetSocketType(newType);
        }
        public void SetupRightWeapon(GameObject weapon, Transform bodySocket, Transform handSocket, WeaponComponent weaponComponent)
        {
            RightWeapon.WeaponObject = weapon;
            RightWeapon.BodySocket = bodySocket;
            RightWeapon.HandSocket = handSocket;
            RightWeapon.WeaponComponent = weaponComponent;

            weaponComponent.SetBodySocket(RightWeapon.BodySocket);
            weaponComponent.SetHandSocket(RightWeapon.HandSocket);
        }
        public void SetupLeftWeapon(GameObject weapon, Transform bodySocket, Transform handSocket, WeaponComponent weaponComponent)
        {
            LeftWeapon.WeaponObject = weapon;
            LeftWeapon.BodySocket = bodySocket;
            LeftWeapon.HandSocket = handSocket;
            LeftWeapon.WeaponComponent = weaponComponent;

            weaponComponent.SetBodySocket(LeftWeapon.BodySocket);
            weaponComponent.SetHandSocket(LeftWeapon.HandSocket);
        }
        #endregion

        #region Callbacks
        private void SetPlaceHolderForTwoHandedWeapon()
        {
            var currentWeapon = GetCurrentMainWeapon();
            if (currentWeapon != null && currentWeapon.WeaponComponent.Item.hand == WeaponHand.TwoHand)
            {
                Transform socket;
                switch (currentWeapon.WeaponComponent.Item.mainHand)
                {
                    case MainHand.Right:
                        socket = LeftWeapon.HandSocket;
                        Instantiate(new GameObject("Two-Handed Weapon Place Holder"), socket);
                        break;

                    case MainHand.Left:
                        socket = RightWeapon.HandSocket;
                        Instantiate(new GameObject("Two-Handed Weapon Place Holder"), socket);
                        break;
                }
            }
        }
        private void RemovePlaceHolderForTwoHandedWeapon()
        {
            var currentWeapon = GetCurrentMainWeapon();
            if (currentWeapon != null && currentWeapon.WeaponComponent.Item.hand == WeaponHand.TwoHand)
            {
                Transform socket;
                switch (currentWeapon.WeaponComponent.Item.mainHand)
                {
                    case MainHand.Right:
                        socket = LeftWeapon.HandSocket;
                        Destroy(socket.GetChild(0).gameObject);
                        break;

                    case MainHand.Left:
                        socket = RightWeapon.HandSocket;
                        Destroy(socket.GetChild(0).gameObject);
                        break;
                }
            }
        }
        #endregion

        #region Test Methods
        [ContextMenu("Switch Right Weapon")]
        private void SwitchRightWeapon()
        {
            SwitchWeapon(RightSelectedSlot.SlotInfo.itemId, 2, SocketOrientation.Right);
        }

        [ContextMenu("Switch Left Weapon")]
        private void SwitchLeftWeapon()
        {
            SwitchWeapon(LeftSelectedSlot.SlotInfo.itemId, 3, SocketOrientation.Left);
        }

        [ContextMenu("Unequip Right Weapon")]
        private void UnequipRightWeapon()
        {
            UnequipWeapon(RightSelectedSlot.SlotInfo.itemId, 1, RightSelectedSlot);
        }

        [ContextMenu("Unequip Left Weapon")]
        private void UnequipLeftWeapon()
        {
            UnequipWeapon(LeftSelectedSlot.SlotInfo.itemId, 1, LeftSelectedSlot);
        }

        //[ContextMenu("Switch Right Slot")]
        //private void SwitchRightSlot()
        //{
        //    SwitchRightSelectedSlot();
        //}        
        
        //[ContextMenu("Switch Left Slot")]
        //private void SwitchLeftSlot()
        //{
        //    SwitchLeftSelectedSlot();
        //}

        [ContextMenu("Save Inventory")]
        private void Save()
        {
            SaveInventoryAndEquipment();
            Debug.Log(jsonString);
        }
        #endregion
    }
}