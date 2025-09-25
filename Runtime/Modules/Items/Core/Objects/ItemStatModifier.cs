using UltimateFramework.Utils;
using UnityEngine;
using System;

namespace UltimateFramework.ItemSystem
{
    [Serializable]
    public class ItemStatModifier
    {
        private float currentValue = 0;

        public string statType;
        public float startValue;
        public OperationType opType;
        public UltimateFramework.Utils.ValueType valueType;
        public ValueTo valueTo;
        public BaseOn baseOn;

        public int Index { get; set; }
        public float CurrentValue { get => currentValue; }

        public void SetCurrentValue(float newValue)
        {
            this.currentValue = newValue;
            //Debug.Log(this.currentValue);
        }
    }
}
