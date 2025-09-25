using UltimateFramework.Utils;

namespace UltimateFramework.ItemSystem
{
    [System.Serializable]
    public class ItemAttributeModifier
    {
        public string attributeType;
        public float startValue;
        public OperationType opType;
        public ValueType valueType;
        private float currentValue = 0;
        public int Index { get; set; }
        public float CurrentValue { get => currentValue; }
        public void SetCurrentValue(float newValue)
        {
            this.currentValue = newValue;
        }
    }
}
