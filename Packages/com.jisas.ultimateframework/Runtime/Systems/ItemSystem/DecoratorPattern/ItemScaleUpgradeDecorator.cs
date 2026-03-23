using UltimateFramework.StatisticsSystem;

namespace UltimateFramework.ItemSystem
{
    class ItemScaleUpgradeDecorator : ItemUpgradeDecorator
    {
        public ItemScaleUpgradeDecorator(Item itemToDecorate) : base(itemToDecorate) { }

        public override void UpgradeItem(ItemUpgrade currentUpgrade, StatisticsComponent characterStats)
        {
            if (!CanBeUpgradeVerification(this.decoratedItem, this.decoratedItem.Scaled.Count)) return;
            
            var currentScale = this.decoratedItem.FindScale(currentUpgrade.scaleToAffectIndex);
            currentScale.SetCurrentScale(currentUpgrade.newScaleLevel);
            this.decoratedItem.ApplyScale(characterStats, false, false);
        }
    }
}
