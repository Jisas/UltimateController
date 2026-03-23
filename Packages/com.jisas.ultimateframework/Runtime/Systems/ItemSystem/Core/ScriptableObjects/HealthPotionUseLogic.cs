using UltimateFramework.StatisticsSystem;
using UltimateFramework.InventorySystem;
using UltimateFramework.Utils;
using UnityEngine;
using System;

namespace UltimateFramework.ItemSystem
{
    [CreateAssetMenu(menuName = "Ultimate Framework/Systems/Items/Data/Use Items Logic/Health Potion")]
    public class HealthPotionUseLogic : UseItemLogic
    {
        public override void Use(string slotTag, InventoryAndEquipmentComponent inventory, StatisticsComponent characterStats)
        {
            var item = inventory.GetItemInEquipment(slotTag);

            if (item != null && item.StatModifiers.Count > 0)
            {
                foreach (var statMod in item.StatModifiers)
                {
                    var stat = characterStats.FindStatistic(statMod.statType);
                    var opType = GetOperation(statMod, characterStats);
                    var maxValue = stat.CurrentMaxValue;
                    var currentValue = stat.CurrentValue;
                    var valueType = statMod.valueType == UltimateFramework.Utils.ValueType.Percentage;

                    if (statMod.valueTo == ValueTo.MaxValue)
                    {
                        if (valueType)
                        {
                            if (statMod.baseOn == BaseOn.None) throw new InvalidOperationException("Invalid operation, it is necessary to assign a value other than \"None\" to the enum BaseOn");

                            if (statMod.baseOn == BaseOn.BaseOrMaxValue)
                            {
                                stat.CurrentMaxValue = characterStats.ApplyModifyAttributesOrStatsOperation(opType, maxValue, statMod.CurrentValue, valueType, maxValue);
                            }
                            else stat.CurrentMaxValue = characterStats.ApplyModifyAttributesOrStatsOperation(opType, maxValue, statMod.CurrentValue, valueType, currentValue);
                        }
                        else stat.CurrentMaxValue = characterStats.ApplyModifyAttributesOrStatsOperation(opType, maxValue, statMod.CurrentValue, valueType);
                    }
                    else
                    {
                        if (valueType)
                        {
                            if (statMod.baseOn == BaseOn.None) throw new InvalidOperationException("Invalid operation, it is necessary to assign a value other than \"None\" to the enum BaseOn");

                            if (statMod.baseOn == BaseOn.BaseOrMaxValue)
                            {
                                stat.CurrentValue = characterStats.ApplyModifyAttributesOrStatsOperation(opType, currentValue, statMod.CurrentValue, valueType, maxValue);
                            }
                            else stat.CurrentValue = characterStats.ApplyModifyAttributesOrStatsOperation(opType, currentValue, statMod.CurrentValue, valueType, currentValue);
                        }
                        else stat.CurrentValue = characterStats.ApplyModifyAttributesOrStatsOperation(opType, currentValue, statMod.CurrentValue, valueType);
                    }
                }

                inventory.UnequipItem(item.index, 1);
            }
        }
    }
}
