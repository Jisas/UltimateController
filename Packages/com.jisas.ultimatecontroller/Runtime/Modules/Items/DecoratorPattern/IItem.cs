using UltimateController.StatisticsSystem;

namespace UltimateController.ItemSystem
{
    public interface IItem
    {
        void UpgradeItem(ItemUpgrade currentUpgrade);
        void UpgradeItem(ItemUpgrade currentUpgrade, StatisticsComponent characterStats);
    }
}
