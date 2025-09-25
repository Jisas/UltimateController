using UnityEngine;
using System;
using UltimateFramework.SerializationSystem;

namespace UltimateFramework.StatisticsSystem
{
    [Serializable]
    public class Attribute
    {
        public TagSelector attributeType = new("None");
        public float startValue;
        private float _value;

        public delegate void OnValueChangeDelegate(Attribute att, float newValue);
        public event OnValueChangeDelegate OnValueChange;

        public float CurrentValue
        {
            get { return _value; }
            set
            {
                _value = value;
                //Debug.Log($"The Attribute {attributeType.tag}, have a value of = {_value}");
                if (_value != startValue) OnValueChange?.Invoke(this, _value);
            }
        }

        public AttributeData GetSerializableData()
        {
            return new AttributeData
            {
                attributeType = attributeType.tag,
                startValue = startValue,
                currentValue = _value
            };
        }

        public void LoadFromSerializableData(AttributeData data)
        {
            attributeType = new TagSelector(data.attributeType);
            startValue = data.startValue;
            _value = data.currentValue;
        }
    }
}

