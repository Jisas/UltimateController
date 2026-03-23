using UltimateFramework.StatisticsSystem;

namespace UltimateFramework.ItemSystem
{
    class ItemAttModUpgradeDecorator : ItemUpgradeDecorator
    {
        public ItemAttModUpgradeDecorator(Item itemToDecorate) : base(itemToDecorate) { }

        public override void UpgradeItem(ItemUpgrade currentUpgrade, StatisticsComponent characterStats)
        {
            if (!CanBeUpgradeVerification(this.decoratedItem, this.decoratedItem.Scaled.Count)) return;

            var currentAttMod = this.decoratedItem.FindAttributeModifier(currentUpgrade.attModToAffectIndex);
            currentAttMod.SetCurrentValue(currentUpgrade.attModNewValue);

            var operation = GetOperation(currentAttMod, characterStats);
            var valueType = currentAttMod.valueType == Utils.ValueType.Percentage;
            var att = characterStats.FindAttribute(currentAttMod.attributeType);
            att.CurrentValue = characterStats.ApplyModifyAttributesOrStatsOperation(operation, currentAttMod.CurrentValue, att.CurrentValue, valueType);
        }
    }
}
