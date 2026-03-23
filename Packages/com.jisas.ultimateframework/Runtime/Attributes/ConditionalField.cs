using UnityEngine;

namespace UltimateFramework.Utils
{
    public class ConditionalField : PropertyAttribute
    {
        public string conditionalSourceField = "";
        public object expectedValue = null;

        public ConditionalField(string conditionalSourceField, object expectedValue)
        {
            this.conditionalSourceField = conditionalSourceField;
            this.expectedValue = expectedValue;
        }
    }
}
