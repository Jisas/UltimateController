using UltimateFramework.InventorySystem;
using UltimateFramework.StatisticsSystem;
using UltimateFramework.Utils;
using UnityEngine;
using System;

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
            StatisticsComponent.Operation operation = null;

            switch (statModifier.opType)
            {
                case OperationType.Sum:
                    operation = characterStats.Sum;
                    break;

                case OperationType.Substract:
                    operation = characterStats.Subtract;
                    break;
            }

            return operation;
        }
    }
}