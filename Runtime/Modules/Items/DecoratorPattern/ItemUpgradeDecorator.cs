using UltimateFramework.StatisticsSystem;
using UltimateFramework.Utils;
using System;

namespace UltimateFramework.ItemSystem
{
    abstract class ItemUpgradeDecorator : IItem
    {
        protected Item decoratedItem;
        protected delegate float Operation(float a, float b, bool isPercentage, float c = 0);

        // Instance delegates
        protected Operation sumOnBaseValue;
        protected Operation sumOnCurrentValue;
        protected Operation multiplyOnBaseValue;
        protected Operation multiplyOnCurrentValue;

        public ItemUpgradeDecorator(Item itemToDecorate)
        {
            this.decoratedItem = itemToDecorate;

            // Set delegates for base values
            sumOnBaseValue = (a, b, isPercentage, c) => isPercentage ? a + (c * b / 100) : a + b;
            multiplyOnBaseValue = (a, b, isPercentage, c) => isPercentage ? a * (c * b / 100) : a * b;

            // Set delegates for current values
            sumOnCurrentValue = (a, b, isPercentage, c) => isPercentage ? a + (a * b / 100) : a + b;
            multiplyOnCurrentValue = (a, b, isPercentage, c) => isPercentage ? a * (a * b / 100) : a * b;
        }
        protected float ApplyOperation(Operation operation, float a, float b, bool isPercentage, float c = 0)
        {
            return operation(a, b, isPercentage, c);
        }
        protected Operation GetOperation(ItemUpgrade upgrade) 
        {
            switch (upgrade.opType)
            {
                case OperationType.Sum:

                    switch (upgrade.baseOn)
                    {
                        case BaseOn.BaseOrMaxValue:
                            return sumOnBaseValue;

                        case BaseOn.CurrentValue:
                            return sumOnCurrentValue;

                    }
                    break;
            }

            throw new InvalidOperationException("Invalid operation");
        }
        public StatisticsComponent.Operation GetOperation(ItemAttributeModifier atributeModifier, StatisticsComponent characterStats)
        {
            return atributeModifier.opType switch
            {
                OperationType.Sum => characterStats.Sum,
                OperationType.Substract => characterStats.Subtract,
                _ => throw new InvalidOperationException("Invalid operation"),
            };
        }
        public bool CanBeUpgradeVerification(Item decoratedItem, int listCount)
        {
            if (!decoratedItem.canBeUpgrade)
                return false;

            if (listCount <= 0)
                return false;

            return true;
        }
        public virtual void UpgradeItem(ItemUpgrade currentUpgrade) { }
        public virtual void UpgradeItem(ItemUpgrade currentUpgrade, StatisticsComponent characterStats) { }
    }
}