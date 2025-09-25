using UltimateFramework.SerializationSystem;
using UltimateFramework.StatisticsSystem;
using UltimateFramework.LocomotionSystem;
using UltimateFramework.ItemSystem;
using System.Collections.Generic;
using UltimateFramework.Commons;
using UltimateFramework.Inputs;
using UltimateFramework.Utils;
using UltimateFramework.Tools;
using UnityEngine;
using System;

namespace UltimateFramework.InventorySystem
{
    [AbstractClassName("InventoryAndEquipmentComponent")]
    public abstract class InventoryAndEquipmentComponent : MonoBehaviour, IUFComponent
    {
        #region SerializedFields
        [Header("Inventory")]
        public bool useInventory;
        public GameObject slotPrefab;
        public Transform slotsPanel;
        [SerializeField] protected int capacity;

        [Header("Equipment")]
        public bool useEquipmentUI;
        public bool useFastAccessUI;
        public Transform bodyBone;
        public Transform leftHandBone;
        public Transform rightHandBone;
        [SerializeField] protected List<StartingItems> startingItems;
        [SerializeField] protected List<EquipSlotData> equipSlotsData;
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
        public List<InventorySlot> InventorySlots
        {
            get => inventorySlots;
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
        public EquipmentSlot BottomSelectedSlot { get; set; }
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
        public Action OnEquipTwoHandWeapon;
        public Action OnUnequipTwoHandWeapon;
        public Action<Item,int> OnAddItemToInventory;
        #endregion

        #region Strategies
        //Starting Items
        protected Dictionary<WeaponHand, IStartingEquipStrategy> startingEquipStrategies;
        protected IStartingEquipStrategy otherItemStartingStrategy = new OtherItemsStartingStrategy();
        readonly IStartingEquipStrategy oneHandStartingStrategy = new OneHandStartingStrategy();
        readonly IStartingEquipStrategy twoHandStartingStrategy = new TwoHandStartingStrategy();
        readonly IStartingEquipStrategy offHandStaringStrategy = new OffHandStartingStrategy();
        readonly IStartingEquipStrategy dualHandStaringStrategy = new DualHandStartingEquipStrategy();

        //Eqiup
        Dictionary<WeaponHand, IEquipWeaponStrategy> equipStrategies;
        readonly IEquipWeaponStrategy oneHandStrategy = new OneHandEquipStrategy();
        readonly IEquipWeaponStrategy twoHandStrategy = new TwoHandEquipStrategy();
        readonly IEquipWeaponStrategy offHandStrategy = new OffHandEquipStrategy();
        readonly IEquipWeaponStrategy dualHandStrategy = new DualHandEquipStrategy();
        #endregion

        #region ProtectedFields
        protected string jsonString;
        protected ItemDatabase itemDatabase;
        protected EntityActionInputs m_InputsManager;
        protected StatisticsComponent m_Statistics;
        protected SettingsMasterData m_MasterData;
        protected EntityManager m_EntityManager;
        protected List<InventorySlot> invetorySlots = new();
        protected readonly List<InventorySlot> inventorySlots = new();
        protected readonly List<EquipmentSlot> equipmentSlots = new();
        protected List<SlotInfo> inventorySlotInfoList = new();
        protected List<SlotInfo> equipmentSlotInfoList = new();
        protected List<EquipmentSlot> rightWeaponSlots;
        protected List<EquipmentSlot> leftWeaponsSlots;
        protected List<EquipmentSlot> consumableSlots;
        #endregion

        #region PrivateFields
        private GameObject _lastEquippedWeapon;
        private bool isSwitchingWeaponSlot = false;
        private int currentLeftSlotIndex = 0, 
                    currentRightSlotIndex = 0,
                    currentBottomSlotIndex = 0;
        #endregion

        #region InheritanceConfig
        public string ClassName { get; private set; }
        public EntityState State { get; set; }
        public InventoryAndEquipmentComponent()
        {
            ClassName = GetAbstractClassName() ?? this.GetType().Name;
        }
        private string GetAbstractClassName()
        {
            var type = this.GetType();

            while (type != null)
            {
                var attribute = (AbstractClassNameAttribute)System.Attribute.GetCustomAttribute(type, typeof(AbstractClassNameAttribute));
                if (attribute != null) return attribute.Name;
                type = type.BaseType;
            }

            return null;
        }
        #endregion

        #region Mono
        protected void Awake()
        {
            m_EntityManager = GetComponent<EntityManager>();
            m_MasterData = Resources.Load<SettingsMasterData>("Data/Settings/SettingsConfigData");
            itemDatabase = m_MasterData.itemDB;

            startingEquipStrategies = new()
            {
                { WeaponHand.OneHand, oneHandStartingStrategy },
                { WeaponHand.TwoHand, twoHandStartingStrategy },
                { WeaponHand.OffHand, offHandStaringStrategy },
                { WeaponHand.DualHand, dualHandStaringStrategy },
            };

            equipStrategies = new()
            {
                { WeaponHand.OneHand, oneHandStrategy },
                { WeaponHand.TwoHand, twoHandStrategy },
                { WeaponHand.OffHand, offHandStrategy },
                { WeaponHand.DualHand, dualHandStrategy },
            };
        }
        private void OnEnable()
        {
            OnEquipTwoHandWeapon += SetPlaceHolderForTwoHandedWeapon;
            OnUnequipTwoHandWeapon += RemovePlaceHolderForTwoHandedWeapon;
        }
        protected void Start()
        {
            BaseLocomotionComponent = transform.root.GetComponent<BaseLocomotionComponent>();
            m_Statistics = transform.root.GetComponent<StatisticsComponent>();
            m_InputsManager = transform.root.GetComponent<EntityActionInputs>();

            inventorySlotInfoList = new List<SlotInfo>();
            equipmentSlotInfoList = new List<SlotInfo>();

            LoadInventoryAndEquipment();

            rightWeaponSlots = GetWeaponSlots(SocketOrientation.Right);
            leftWeaponsSlots = GetWeaponSlots(SocketOrientation.Left);
            consumableSlots = GetConsumableSlots(SocketOrientation.Bottom);
        }
        private void OnDisable()
        {
            OnEquipTwoHandWeapon -= SetPlaceHolderForTwoHandedWeapon;
            OnUnequipTwoHandWeapon -= RemovePlaceHolderForTwoHandedWeapon;
        }
        #endregion

        #region Load And Save
        protected virtual void LoadInventoryAndEquipment()
        {
            if (DataGameManager.IsInventoryAndEquipmentDataSaved())
                 LoadSaveInventoryAndEquipment();
            else LoadEmptyInventoryAndEquipment();
        }
        public void SaveInventoryAndEquipment()
        {
            DataGameManager.Instance.SetInventorySlotInfoList(inventorySlotInfoList);
            DataGameManager.Instance.SetEquipmentSlotInfoList(equipmentSlotInfoList);

            if (RightSelectedSlot != null)
            {
                DataGameManager.Instance.SetRightSelectedSlotInfo(RightSelectedSlot.SlotInfo);
            }
            else
            {
                var emptySlot = RightSelectedSlot.SlotInfo;
                emptySlot.EmptySlot();
                DataGameManager.Instance.SetBottomSelectedSlotInfo(emptySlot);
            }

            if (LeftSelectedSlot != null)
            {
                DataGameManager.Instance.SetLeftSelectedSlotInfo(LeftSelectedSlot.SlotInfo);
            }
            else
            {
                var emptySlot = LeftSelectedSlot.SlotInfo;
                emptySlot.EmptySlot();
                DataGameManager.Instance.SetBottomSelectedSlotInfo(emptySlot);
            }

            if (BottomSelectedSlot != null)
            {
                DataGameManager.Instance.SetBottomSelectedSlotInfo(BottomSelectedSlot.SlotInfo);
            }
            else
            {
                var emptySlot = BottomSelectedSlot.SlotInfo;
                emptySlot.EmptySlot();
                DataGameManager.Instance.SetBottomSelectedSlotInfo(emptySlot);
            }

            DataGameManager.Instance.SaveInventoryAndEquipmentData();
        }
        protected virtual void LoadEmptyInventoryAndEquipment() { }
        protected virtual void LoadSaveInventoryAndEquipment() { }
        #endregion

        #region Create And Setup Slots
        protected InventorySlot CreateAndSetUpInventorySlots(int index, bool setList)
        {
            GameObject slot = Instantiate<GameObject>(slotPrefab, slotsPanel);
            InventorySlot newSlot = slot.GetComponent<InventorySlot>();
            newSlot.ItemDB = this.itemDatabase;
            newSlot.SetUp(index);
            inventorySlots.Add(newSlot);

            if (setList)
            {
                SlotInfo newSlotInfo = newSlot.SlotInfo;
                inventorySlotInfoList.Add(newSlotInfo);
            }

            newSlot.UpdateUI();
            return newSlot;
        }
        protected EquipmentSlot CreateAndSetUpEquipmentSlots(int index, EquipSlotData data, bool setList)
        {
            var newSlot = data.slotObject.GetComponent<EquipmentSlot>();
            newSlot.ItemDB = this.itemDatabase;
            newSlot.ItemDB = itemDatabase;
            newSlot.Selected = data.selected;
            newSlot.UseAmountText = data.useAmountText;
            newSlot.SlotTags = data.slotTags;
            newSlot.Orientation = data.orientation;
            newSlot.SetUp(index);
            equipmentSlots.Add(newSlot);

            if (setList)
            {
                SlotInfo newSlotinfo = newSlot.SlotInfo;
                equipmentSlotInfoList.Add(newSlotinfo);
            }

            if (useEquipmentUI) newSlot.UpdateUI();

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
                    OnAddItemToInventory?.Invoke(item, 1);
                }
            }
            else Debug.LogError($"The item with ID: {itemId} no found in database");
        }
        public void AddItem(int itemId, int amount)
        {
            Item item = itemDatabase.FindItem(itemId);

            if (item != null)
            {
                SlotInfo slotInfo = FindSuitableSlotInInventory(itemId);

                if (slotInfo != null)
                {
                    slotInfo.amount += amount;
                    slotInfo.itemId = itemId;
                    slotInfo.isEmpty = false;
                    FindInvetorySlot(slotInfo.id).UpdateUI();
                    OnAddItemToInventory?.Invoke(item, amount);
                }
            }
            else Debug.LogError($"The item with ID: {itemId} no found in database");
        }
        public void RemoveItem(int itemId, int ammount)
        {
            SlotInfo slotInfo = GetInventorySlotInfo(itemId);

            if (slotInfo != null)
            {
                slotInfo.amount -= ammount;

                if (slotInfo.amount < 0) slotInfo.amount = 0;
                if (slotInfo.amount == 0) slotInfo.EmptySlot();

                FindInvetorySlot(slotInfo.id).UpdateUI();
            }
        }
        public SlotInfo GetInventorySlotInfo(int itemId)
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
        public Item GetItemInInventory(int itemIndex)
        {
            foreach (InventorySlot inventorySlot in inventorySlots)
            {
                if (!inventorySlot.SlotInfo.isEmpty && inventorySlot.SlotInfo.itemId == itemIndex)
                    return itemDatabase.FindItem(itemIndex);
            }
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
        public void EquipItem(Item item, int socketIndex, int amount = 1, bool equipOnBody = true, bool removeOfInvetory = true, bool isSaveData = false, SlotInfo savedSlotInfo = default)
        {
            bool twoHandComprobement = item.type == ItemType.Weapon && item.hand == WeaponHand.TwoHand;
            var itemID = itemDatabase.GetItemID(item);

            if (useInventory && removeOfInvetory)
            {
                var inventoryItem = GetInventorySlotInfo(itemID) ?? throw new Exception($"Item {item.name}, not found in inventory");
                RemoveItem(inventoryItem.itemId, amount);
            }

            SlotInfo slotInfo = null;
            if (!isSaveData)
            {
                SocketOrientation orientation = 
                    socketIndex == 0 ? SocketOrientation.Right : 
                    socketIndex == 1 ? SocketOrientation.Left : 
                    socketIndex == 2 ? SocketOrientation.Top :
                    SocketOrientation.Bottom;

                if (removeOfInvetory)
                {
                    slotInfo = FindEmptySlotInEquipment(item.itemSlot, orientation);
                }
                else
                {
                    slotInfo = twoHandComprobement ?
                        FindEmptySlotInEquipment(item.itemSlot, orientation) :
                        FindSuitableSlotInEquipment(itemID, orientation);
                }

                if (slotInfo == null) return;
                slotInfo.amount += amount;
                slotInfo.itemId = itemID;
                slotInfo.isEmpty = false;
                if (useEquipmentUI) FindEquipmentSlot(slotInfo.id).UpdateUI();
                if (useFastAccessUI) FindEquipmentSlot(slotInfo.id).UpdateFAUI();
                ModifyAttributesByItem(item, true);
            }

            if (item.type == ItemType.Weapon) 
            { 
                var currentSlotInfo = isSaveData ? savedSlotInfo : slotInfo; 
                bool shouldApplyStrategy = false; 

                if (twoHandComprobement) 
                { 
                    switch (item.mainHand) 
                    { 
                        case MainHand.Right: 
                            shouldApplyStrategy = (RightSelectedSlot != null && RightSelectedSlot.SlotInfo.id == currentSlotInfo.id); 
                            break; 

                        case MainHand.Left: 
                            shouldApplyStrategy = (LeftSelectedSlot != null && LeftSelectedSlot.SlotInfo.id == currentSlotInfo.id); 
                            break; 
                    } 
                } 
                else 
                { 
                    shouldApplyStrategy = (RightSelectedSlot != null && RightSelectedSlot.SlotInfo.id == currentSlotInfo.id) || 
                                          (LeftSelectedSlot != null && LeftSelectedSlot.SlotInfo.id == currentSlotInfo.id); 
                } 
                
                if (shouldApplyStrategy) 
                { 
                    ApplyEquipWeaponStrategy(item, socketIndex, equipOnBody); 
                } 
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
        public void UnequipItem(int itemID, int ammount, bool returnToInventory = true)
        {
            SlotInfo slotInfo = FindSuitableSlotInEquipment(itemID);

            if (slotInfo != null)
            {
                slotInfo.amount -= ammount;

                if (slotInfo.amount < 0) slotInfo.amount = 0;
                if (slotInfo.amount == 0) slotInfo.EmptySlot();
                if (useEquipmentUI) FindEquipmentSlot(slotInfo.id).UpdateUI();
                if (useFastAccessUI) FindEquipmentSlot(slotInfo.id).UpdateFAUI();

                if(returnToInventory) AddItem(itemID, ammount);

                var item = itemDatabase.FindItem(itemID);
                ModifyAttributesByItem(item, false);
            }
        }
        public void SwitchItem(int currenItemID, int newItemID, int amount)
        {
            UnequipItem(currenItemID, amount);
            Item item = itemDatabase.FindItem(newItemID);
            int socketIndex = item.type == ItemType.Consumable ? 3 : item.type == ItemType.Spell ? 2 : 0;
            EquipItem(item, socketIndex, amount);
        }
        public void UnequipWeapon(int itemID, int ammount, EquipmentSlot equipmentSlot, bool returnToInventory = true)
        {
            if (equipmentSlot == null) return;

            equipmentSlot.SlotInfo.amount = Math.Max(0, equipmentSlot.SlotInfo.amount - ammount);
            if (equipmentSlot.SlotInfo.amount == 0) equipmentSlot.SlotInfo.EmptySlot();
            if (useEquipmentUI) FindEquipmentSlot(equipmentSlot.SlotInfo.id).UpdateUI();
            if (useFastAccessUI) FindEquipmentSlot(equipmentSlot.SlotInfo.id).UpdateFAUI();

            Item item = itemDatabase.FindItem(itemID);
            Transform socketTransform = GetSocketTransform(item, equipmentSlot);
            if (socketTransform == null) return;

            if (item.hand == WeaponHand.DualHand)
            {
                RightWeapon.BodySocket = null;
                RightWeapon.HandSocket = null;
                LeftWeapon.BodySocket = null;
                LeftWeapon.HandSocket = null;

                Destroy(rightHandBone.Find(item.handSlot).GetChild(0).gameObject);
                Destroy(leftHandBone.Find(item.handSlot).GetChild(0).gameObject);
            }
            else
            {
                if (equipmentSlot.Orientation == SocketOrientation.Right)
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
            }

            BaseLocomotionComponent.SwitchLocomotionMap("Unarmed");
            m_InputsManager.ResetInputActionState("EquipMelee");

            if(returnToInventory) AddItem(itemID, ammount);

            ModifyAttributesByItem(item, false);
        }
        public void SwitchWeapon(int currentWeaponID, int newWeaponID, SocketOrientation orientation)
        {
            int ammount = 1;
            EquipmentSlot equipmentSlot = FindEquipmentSlot(currentWeaponID, orientation);
            int itemID = equipmentSlot.SlotInfo.itemId;

            UnequipWeapon(itemID, ammount, equipmentSlot);

            bool onBody = equipmentSlot.SlotInfo.type == SocketType.Body;
            int socketIndex = GetSocketIndex(newWeaponID, equipmentSlot);
            Item weapon = itemDatabase.FindItem(newWeaponID);

            EquipItem(weapon, socketIndex, equipOnBody: onBody);
        }
        public void SwitchRightSelectedSlot(bool instanceWeaponObj = true)
        {
            currentRightSlotIndex = (currentRightSlotIndex + 1) % rightWeaponSlots.Count;

            string tag = "";
            foreach (var slotTag in RightSelectedSlot.SlotTags)
            {
                if (slotTag.tag == "ItemSlot.MeleeWeapon")
                    tag = slotTag.tag;
            }

            if (String.IsNullOrEmpty(tag)) throw new Exception("No slots were found with the tag: ItemSlot.MeleeWeapon");

            int slotID = rightWeaponSlots[currentRightSlotIndex].SlotInfo.id;
            var newSelecetedSlot = FindEquipmentSlot(slotID, SocketOrientation.Right, tag);

            if (newSelecetedSlot == null)
            {
                Debug.LogWarning("There are no more slots with weapons, only the one currently in use.");
                return;
            }

            RightWeapon.BodySocket = null;
            RightWeapon.HandSocket = null;
            RightSelectedSlot.Selected = false;
            RightSelectedSlot = newSelecetedSlot;
            RightSelectedSlot.Selected = true;

            var oldItemObj = RightWeapon.WeaponObject;
            var newItem = itemDatabase.FindItem(RightSelectedSlot.SlotInfo.itemId);

            if (newItem.hand == WeaponHand.TwoHand && !isSwitchingWeaponSlot)
            {
                isSwitchingWeaponSlot = true;
                SwitchLeftSelectedSlot(false);
                isSwitchingWeaponSlot = false;
            }

            if (instanceWeaponObj)
                ApplyEquipWeaponStrategy(newItem, (int)SocketOrientation.Right, RightSelectedSlot.SlotInfo.type == SocketType.Body, true, newSelecetedSlot.Orientation);

            if (oldItemObj != null) Destroy(oldItemObj);
        }
        public void SwitchLeftSelectedSlot(bool instanceWeaponObj = true)
        {
            currentLeftSlotIndex = (currentLeftSlotIndex + 1) % leftWeaponsSlots.Count;

            string tag = "";
            foreach (var slotTag in LeftSelectedSlot.SlotTags)
            {
                if (slotTag.tag == "ItemSlot.MeleeWeapon")
                    tag = slotTag.tag;
            }

            if (String.IsNullOrEmpty(tag)) throw new Exception($"No slots were found with the tag: ItemSlot.MeleeWeapon on object: {gameObject.name}");

            int slotID = leftWeaponsSlots[currentLeftSlotIndex].SlotInfo.id;
            var newSelecetedSlot = FindEquipmentSlot(slotID, SocketOrientation.Left, tag);

            if (newSelecetedSlot == null)
            {
                Debug.LogWarning("There are no more slots with weapons, only the one currently in use.");
                return;
            }

            LeftWeapon.BodySocket = null;
            LeftWeapon.HandSocket = null;
            LeftSelectedSlot.Selected = false;
            LeftSelectedSlot = newSelecetedSlot;
            LeftSelectedSlot.Selected = true;

            var oldItemObj = LeftWeapon.WeaponObject;
            var newItem = itemDatabase.FindItem(LeftSelectedSlot.SlotInfo.itemId);

            if (newItem.hand == WeaponHand.TwoHand && !isSwitchingWeaponSlot)
            {
                isSwitchingWeaponSlot = true;
                SwitchRightSelectedSlot(false);
                isSwitchingWeaponSlot = false;
            }

            if (instanceWeaponObj)
                ApplyEquipWeaponStrategy(newItem, (int)SocketOrientation.Left, LeftSelectedSlot.SlotInfo.type == SocketType.Body, true, newSelecetedSlot.Orientation);

            if (oldItemObj != null) Destroy(oldItemObj);
        }
        public void SwitchCosumableSelectedSlot()
        {
            List<EquipmentSlot> notEmptyConsubleSlots = new();

            foreach (var consumablelot in consumableSlots)
            {
                if (!consumablelot.SlotInfo.isEmpty)
                    notEmptyConsubleSlots.Add(consumablelot);
            }

            currentBottomSlotIndex = (currentBottomSlotIndex + 1) % notEmptyConsubleSlots.Count;

            string tag = "";
            foreach (var slotTag in BottomSelectedSlot.SlotTags)
            {
                if (slotTag.tag.Contains("ItemSlot.Consumable"))
                    tag = slotTag.tag;
            }

            if (String.IsNullOrEmpty(tag)) throw new Exception("No slots were found with the tag: ItemSlot.Consumable");

            int slotID = notEmptyConsubleSlots[currentBottomSlotIndex].SlotInfo.id;
            var newSelecetedSlot = FindEquipmentSlot(slotID, SocketOrientation.Bottom, tag);

            if (newSelecetedSlot == null)
            {
                Debug.LogWarning("There are no more slots with consumables, only the one currently in use.");
                return;
            }

            BottomSelectedSlot.Selected = false;
            BottomSelectedSlot = newSelecetedSlot;
            BottomSelectedSlot.Selected = true;
        }
        public Item GetItemInEquipment(string slotTag)
        {
            foreach (EquipmentSlot equipSlot in equipmentSlots)
            {
                foreach (var tag in equipSlot.SlotTags)
                {
                    if (!equipSlot.SlotInfo.isEmpty && slotTag == tag.tag && equipSlot.Selected)
                        return itemDatabase.FindItem(equipSlot.SlotInfo.itemId);
                }
            }
            return null;
        }
        public EquipmentSlot GetEquipmentSlot(int itemId)
        {
            foreach (EquipmentSlot equipSlot in equipmentSlots)
            {
                if (equipSlot.SlotInfo.itemId == itemId && !equipSlot.SlotInfo.isEmpty && equipSlot.Selected)
                    return equipSlot;
            }

            return null;
        }
        public SlotInfo FindSuitableSlotInEquipment(int itemId)
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
                foreach (var slot in equipmentSlot.SlotTags)
                {
                    if (!equipmentSlot.SlotInfo.isEmpty &&  equipmentSlot.Orientation == orientation && item.itemSlot == slot.tag)
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
                foreach (var slot in equipmentSlot.SlotTags)
                {
                    if (equipmentSlot.SlotInfo.isEmpty && equipmentSlot.Orientation == orientation && slot.tag == slotTag)
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

                if (currentSlot.SlotInfo.itemId == itemID && currentSlot.Orientation == orientation)
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
                foreach (var slot in equipmentSlot.SlotTags)
                {
                    if (!equipmentSlot.SlotInfo.isEmpty &&
                        equipmentSlot.SlotInfo.id == slotID &&
                        equipmentSlot.Orientation == orientation &&
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
                if (slot.Selected && slot.Orientation == socketOrientation)
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
                    socketIndex = equipmentSlot.Orientation == SocketOrientation.Left ? 1 : 0;
                    break;

                case WeaponHand.TwoHand:
                    socketIndex = item.mainHand == MainHand.Right ? 0 : 1;
                    break;

                case WeaponHand.OffHand:
                    socketIndex = 1;
                    break;

                case WeaponHand.DualHand:
                    socketIndex = equipmentSlot.Orientation == SocketOrientation.Left ? 1 : 0;
                    break;
            }

            return socketIndex;
        }
        protected Transform GetSocketTransform(Item item, EquipmentSlot equipmentSlot)
        {
            string[] socketNames;
            string socketName;
            Transform bone;

            switch (equipmentSlot.SlotInfo.type)
            {
                case SocketType.Body:
                    socketNames = item.bodySlot.Split(",");
                    socketName = socketNames[(int)equipmentSlot.Orientation];
                    bone = bodyBone;
                    break;
                case SocketType.Hand:
                    socketName = item.handSlot;
                    bone = equipmentSlot.Orientation == SocketOrientation.Right ? rightHandBone : leftHandBone;
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
        private List<EquipmentSlot> GetConsumableSlots(SocketOrientation orientation)
        {
            List<EquipmentSlot> consumableSlots = new();

            foreach (var equipmentSlot in equipSlotsData)
            {
                if (equipmentSlot.orientation == orientation)
                {
                    foreach (var tagSelector in equipmentSlot.slotTags)
                    {
                        if (tagSelector.tag.Contains("ItemSlot.Consumable"))
                        {
                            var index = equipSlotsData.IndexOf(equipmentSlot);
                            consumableSlots.Add(equipmentSlots[index]);
                        }
                    }
                }
            }

            return consumableSlots;
        }
        protected StatisticsComponent.Operation GetOperation(ItemAttributeModifier atributeModifier)
        {
            return atributeModifier.opType switch
            {
                OperationType.Sum => m_Statistics.Sum,
                OperationType.Substract => m_Statistics.Subtract,
                _ => throw new InvalidOperationException("Invalid operation"),
            };
        }
        protected void ModifyAttributesByItem(Item item, bool isAdditiveOperation)
        {
            // Attribute Modifiers
            foreach (var attmod in item.AttributeModifiers)
            {
                var attributeType = attmod.attributeType;
                var valueType = attmod.valueType;
                var value = attmod.CurrentValue;
                var currentOperation = GetOperation(attmod);
                var isPercentage = valueType == UltimateFramework.Utils.ValueType.Percentage;

                if (m_Statistics.primaryAttributes.Count > 0)
                {
                    var pattr = m_Statistics.FindPrimaryAttribute(attributeType);

                    if (pattr != null)
                    {
                        pattr.CurrentValue = isAdditiveOperation ?
                            m_Statistics.ApplyModifyAttributesOrStatsOperation(currentOperation, pattr.CurrentValue, value, isPercentage, pattr.startValue) :
                            m_Statistics.ApplyModifyAttributesOrStatsOperation(m_Statistics.Subtract, pattr.CurrentValue, value, isPercentage, pattr.startValue);
                    }
                }

                if (m_Statistics.attributes.Count > 0)
                {
                    var attr = m_Statistics.FindAttribute(attributeType);

                    if (attr != null)
                    {
                        attr.CurrentValue = isAdditiveOperation ?
                            m_Statistics.ApplyModifyAttributesOrStatsOperation(currentOperation, attr.CurrentValue, value, isPercentage, attr.startValue) :
                            m_Statistics.ApplyModifyAttributesOrStatsOperation(m_Statistics.Subtract, attr.CurrentValue, value, isPercentage, attr.startValue);
                    }
                }
            }
        }
        #endregion

        #region Public General Methods
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
        public List<SlotInfo> GetAllItemsOfTypeOnInventory(ItemType type)
        {
            List<SlotInfo> slotInfo = new();

            foreach (var slot in inventorySlotInfoList)
            {
                var currentItem = itemDatabase.FindItem(slot.itemId);
                if (currentItem == null || currentItem.type != type) continue;
                slotInfo.Add(slot);
            }

            return slotInfo;
        }
        public List<SlotInfo> GetAllItemsByTagOnInventory(string tag)
        {
            List<SlotInfo> slotInfo = new();

            foreach (var slot in inventorySlotInfoList)
            {
                var currentItem = itemDatabase.FindItem(slot.itemId);
                if (currentItem == null || currentItem.itemSlot != tag) continue;
                slotInfo.Add(slot);
            }

            return slotInfo;
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

        [ContextMenu("SaveData")]
        private void Save() => SaveInventoryAndEquipment();
        #endregion
    }
}