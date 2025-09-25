using UltimateFramework.Utils;

namespace UltimateFramework.ItemSystem
{
    class ItemStatUpgradeDecorator : ItemUpgradeDecorator
    {
        public ItemStatUpgradeDecorator(Item itemToDecorate) : base(itemToDecorate) { }

        public override void UpgradeItem(ItemUpgrade currentUpgrade)
        {
            if (!CanBeUpgradeVerification(decoratedItem, decoratedItem.Stats.Count)) return;

            var opType = GetOperation(currentUpgrade);
            var baseValue = decoratedItem.FindStat(currentUpgrade.statTag).startValue;
            var currentValue = decoratedItem.FindStat(currentUpgrade.statTag).CurrentValue;
            var valueType = currentUpgrade.valueType == ValueType.Percentage;

            if (currentUpgrade.baseOn == BaseOn.BaseOrMaxValue)
            {
                decoratedItem.FindStat(currentUpgrade.statTag).SetCurrentValue(
                    ApplyOperation(opType, currentValue, currentUpgrade.value, valueType, baseValue));
            }
            else
            {
                decoratedItem.FindStat(currentUpgrade.statTag).SetCurrentValue(
                    ApplyOperation(opType, currentValue, currentUpgrade.value, valueType));
            }
        }
    }
}