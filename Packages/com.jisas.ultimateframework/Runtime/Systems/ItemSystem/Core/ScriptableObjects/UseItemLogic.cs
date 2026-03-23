using System;
using UltimateFramework.InventorySystem;
using UltimateFramework.StatisticsSystem;
using UltimateFramework.Utils;
using UnityEngine;

namespace UltimateFramework.ItemSystem
{
    public abstract class UseItemLogic : ScriptableObject
    {
        public AnimationClip useItemMotion;

        public UseItemLogic Clone()
        {
            UseItemLogic clone  = Instantiate(this);
            return clone;
        }
        public abstract void Use(string slotTag, InventoryAndEquipmentComponent inventory, StatisticsComponent characterStats);
        protected StatisticsComponent.Operation GetOperation(ItemStatModifier statModifier, StatisticsComponent characterStats)
        {
            var valueType = statModifier.valueType == UltimateFramework.Utils.ValueType.Percentage;

            switch (statModifier.opType)
            {
                case OperationType.Sum:

                    if (valueType)
                    {
                        switch (statModifier.baseOn)
                        {
                            case BaseOn.None:
                                throw new InvalidOperationException("Invalid operation");

                            case BaseOn.BaseOrMaxValue:
                                return characterStats.SumOnBaseOrMaxValue;

                            case BaseOn.CurrentValue:
                                return characterStats.SumOnCurrentValue;
                        }
                    }
                    else
                    {
                        switch (statModifier.valueTo)
                        {
                            case ValueTo.MaxValue:
                                return characterStats.SumOnBaseOrMaxValue;

                            case ValueTo.CurrentValue:
                                return characterStats.SumOnCurrentValue;
                        }
                    }
                    break;

                case OperationType.Multiply:

                    if (valueType)
                    {
                        switch (statModifier.baseOn)
                        {
                            case BaseOn.None:
                                throw new InvalidOperationException("Invalid operation");

                            case BaseOn.BaseOrMaxValue:
                                return characterStats.MultiplyOnBaseOrMaxValue;

                            case BaseOn.CurrentValue:
                                return characterStats.MultiplyOnCurrentValue;
                        }
                    }
                    else
                    {
                        switch (statModifier.valueTo)
                        {
                            case ValueTo.MaxValue:
                                return characterStats.MultiplyOnBaseOrMaxValue;

                            case ValueTo.CurrentValue:
                                return characterStats.MultiplyOnCurrentValue;
                        }
                    }
                    break;

                case OperationType.Substract:

                    if (valueType)
                    {
                        switch (statModifier.baseOn)
                        {
                            case BaseOn.None:
                                throw new InvalidOperationException("Invalid operation");

                            case BaseOn.BaseOrMaxValue:
                                return characterStats.SubtractOnBaseOrMaxValue;

                            case BaseOn.CurrentValue:
                                return characterStats.SubtractOnCurrentValue;
                        }
                    }
                    else
                    {
                        switch (statModifier.valueTo)
                        {
                            case ValueTo.MaxValue:
                                return characterStats.SubtractOnBaseOrMaxValue;

                            case ValueTo.CurrentValue:
                                return characterStats.SubtractOnCurrentValue;
                        }
                    }
                    break;
            }

            throw new InvalidOperationException("Invalid operation");
        }
    }
}