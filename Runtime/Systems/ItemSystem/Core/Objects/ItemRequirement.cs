
using UltimateFramework.Utils;

namespace UltimateFramework.ItemSystem
{
    [System.Serializable]
    public class ItemRequirement
    {
        public RequirementFor requirementFor;

        public string upgradeName;
        public string itemName;
        public int itemAmmount;

        public string attributeTag;
        public float value;

        public int Index { get; set; }
    }
}