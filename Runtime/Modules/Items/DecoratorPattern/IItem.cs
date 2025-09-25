using UltimateFramework.StatisticsSystem;

namespace UltimateFramework.ItemSystem
{
    public interface IItem
    {
        void UpgradeItem(ItemUpgrade currentUpgrade);
        void UpgradeItem(ItemUpgrade currentUpgrade, StatisticsComponent characterStats);
    }
}
