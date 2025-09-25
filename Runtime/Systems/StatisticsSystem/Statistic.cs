using UnityEngine;
using System;

namespace UltimateFramework.StatisticsSystem
{
    [Serializable]
    public class Statistic
    {
        public TagSelector statType = new("None");
        public float startMaxValue;
        public bool hasRegeneration;
        public bool startFromZero;
        public float regenValue;
        public float regenDelay;
        private float _currentMaxValue;
        private float _currentValue;

        public bool IsRegenerating { get; set; } = false;
        public float CurrentMaxValue
        {
            get => _currentMaxValue;
            set => _currentMaxValue = value;
        }
        public float CurrentValue 
        { 
            get => _currentValue;
            set
            {
                _currentValue = value;
                //Debug.Log($"The stat: {statType.tag}, have a value of = {_currentValue}");
                if (_currentValue >= _currentMaxValue) _currentValue = _currentMaxValue;
                if (_currentValue <= 0) _currentValue = 0;
            }
        }
    }
}

