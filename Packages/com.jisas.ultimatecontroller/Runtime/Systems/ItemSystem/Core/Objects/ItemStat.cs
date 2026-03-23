
namespace UltimateFramework.ItemSystem
{
    [System.Serializable]
    public class ItemStat
    {
        public string statTag;
        public float startValue;
        private float currentValue = 0;
        public int Index {  get; set; }
        public float CurrentValue { get => currentValue; }
        public void SetCurrentValue(float newValue)
        {
            this.currentValue = newValue;
        }
    }
}