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
                    var maxValue = stat.startMaxValue;
                    var currentValue = stat.CurrentValue;
                    var isPercentage = statMod.valueType == UltimateFramework.Utils.ValueType.Percentage;

                    if (statMod.valueTo == ValueTo.MaxValue)
                    {
                        if (isPercentage)
                        {
                            if (statMod.baseOn == BaseOn.None) throw new InvalidOperationException("Invalid operation, it is necessary to assign a value other than \"None\" to the enum BaseOn");

                            if (statMod.baseOn == BaseOn.BaseOrMaxValue)
                            {
                                stat.CurrentMaxValue = characterStats.ApplyModifyAttributesOrStatsOperation(opType, maxValue, statMod.CurrentValue, isPercentage, maxValue);
                                stat.CurrentValue = stat.CurrentMaxValue;
                            }
                            else
                            {
                                stat.CurrentMaxValue = characterStats.ApplyModifyAttributesOrStatsOperation(opType, maxValue, statMod.CurrentValue, isPercentage, currentValue);
                                stat.CurrentValue = stat.CurrentMaxValue;
                            }
                        }
                        else stat.CurrentMaxValue = characterStats.ApplyModifyAttributesOrStatsOperation(opType, maxValue, statMod.CurrentValue, isPercentage);
                    }
                    else
                    {
                        if (isPercentage)
                        {
                            if (statMod.baseOn == BaseOn.None) throw new InvalidOperationException("Invalid operation, it is necessary to assign a value other than \"None\" to the enum BaseOn");

                            if (statMod.baseOn == BaseOn.BaseOrMaxValue)
                            {
                                stat.CurrentValue = characterStats.ApplyModifyAttributesOrStatsOperation(opType, currentValue, statMod.CurrentValue, isPercentage, maxValue);
                            }
                            else stat.CurrentValue = characterStats.ApplyModifyAttributesOrStatsOperation(opType, currentValue, statMod.CurrentValue, isPercentage, currentValue);
                        }
                        else
                        {
                            stat.CurrentValue = characterStats.ApplyModifyAttributesOrStatsOperation(opType, currentValue, statMod.CurrentValue, isPercentage);
                        }
                    }
                }

                inventory.UnequipItem(item.index, 1, false);
            }
        }
    }
}
