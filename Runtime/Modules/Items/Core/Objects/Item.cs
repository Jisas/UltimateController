using UltimateFramework.StatisticsSystem;
using System.Collections.Generic;
using UltimateFramework.Utils;
using UnityEngine.UIElements;
using UnityEngine;

namespace UltimateFramework.ItemSystem
{
    [System.Serializable]
    public class Item
    {
        #region Public And Serialized Fields
        public string name;
        public int index = -1;
        public Sprite icon;
        public GameObject prefab;
        public ItemType type;
        public CombatType combatType;
        public WeaponHand hand;
        public MainHand mainHand;
        public float weight;
        public string damageType;
        public string itemSlot;
        public string bodySlot;
        public string handSlot;
        public string actionsTag;
        public string movesetTag;
        public string description;
        public string notes;
        public bool canBeSell;
        public bool canBeUpgrade;
        public bool canBeDiscarted;
        public bool IsStackable;
        [SerializeField] private List<ItemStat> stats;
        [SerializeField] private List<ItemScale> scaled;
        [SerializeField] private List<ItemUpgrade> upgrades;
        [SerializeField] private List<ItemRequirement> requirements;
        [SerializeField] private List<ItemStatModifier> statsModifiers;
        [SerializeField] private List<ItemAttributeModifier> attributeModifiers;
        #endregion

        #region Properties
        public List<ItemStat> Stats
        {
            get => stats;
        }
        public List<ItemScale> Scaled
        {
            get => scaled;
        }
        public List<ItemUpgrade> Upgrades
        {
            get => upgrades;
        }
        public List<ItemRequirement> Requirements
        {
            get => requirements;
        }
        public List<ItemStatModifier> StatModifiers
        {
            get => statsModifiers;
        }
        public List<ItemAttributeModifier> AttributeModifiers
        {
            get => attributeModifiers;
        }
        public TemplateContainer SelfUIInstance { get; set; }
        #endregion

        #region Constructor
        public Item(ItemType type)
        {
            this.type = type;
        }
        #endregion

        #region Public Methods
        public void InitializeLists()
        {
            stats = new List<ItemStat>();
            scaled = new List<ItemScale>();
            upgrades = new List<ItemUpgrade>();
            requirements = new List<ItemRequirement>();
            statsModifiers = new List<ItemStatModifier>();
            attributeModifiers = new List<ItemAttributeModifier>();
        }
        public void SetAllValuesToBase()
        {
            foreach (var stat in stats)
            {
                stat.SetCurrentValue(stat.startValue);
            }
            foreach (var scale in scaled)
            {
                scale.SetCurrentScale(scale.startScale);
            }
            foreach (var statMod in statsModifiers)
            {
                
                statMod.SetCurrentValue(statMod.startValue);
            }
            foreach (var attMod in attributeModifiers)
            {
                attMod.SetCurrentValue(attMod.startValue);
            }
        }
        public ItemStat FindStat(string tag)
        {
            foreach (var stat in stats)
            {
                if (stat.statTag == tag)
                    return stat;
            }
            return null;
        }
        public ItemScale FindScale(int index)
        {
            foreach (var scale in scaled)
            {
                if (scale.Index == index)
                    return scale;
            }
            return null;
        }
        public ItemUpgrade FindUpgrade(int index)
        {
            foreach (var upgrade in upgrades)
            {
                if (upgrade.index == index)
                {
                    return upgrade;
                }
            }
            return null;
        }
        public ItemStatModifier FindStatModifier(int index)
        {
            foreach (var statMod in statsModifiers)
            {
                if (statMod.Index == index)
                    return statMod;
            }
            return null;
        }
        public ItemAttributeModifier FindAttributeModifier(int index)
        {
            foreach (var attMod in attributeModifiers)
            {
                if (attMod.Index == index)
                    return attMod;
            }
            return null;
        }
        public void ApplyScale(StatisticsComponent statistics, bool applyStartScale, bool isSubtraction)
        {
            if (scaled.Count > 0 && stats.Count > 0 && statistics.primaryAttributes.Count > 0)
            {
                foreach (var scale in scaled)
                {
                    var pattr = statistics.FindPrimaryAttribute(scale.attributeTag);
                    if (pattr != null)
                    {
                        foreach (var itemStat in stats)
                        {
                            float newValue;
                            ScalingLevel itemScale = applyStartScale ? scale.startScale : scale.CurrentScaleLevel;

                            if (itemStat.CurrentValue > 0 || itemStat.CurrentValue > itemStat.startValue)
                                newValue = scale.operation.CalculateScale(itemStat.CurrentValue, scale.attributeTag, itemScale, isSubtraction);
                            else newValue = scale.operation.CalculateScale(itemStat.startValue, scale.attributeTag, itemScale, isSubtraction);

                            //Debug.Log($"The scale of {name} is: {itemScale} with the attribute: {scale.attributeTag}, and the new value for attribute {itemStat.statTag}, is = {newValue}");
                            itemStat.SetCurrentValue(newValue);
                        }
                    }
                }
            }
            else return;
        }
        #endregion
    }
}
