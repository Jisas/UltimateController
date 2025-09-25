using System;
using UltimateFramework.Utils;

namespace UltimateFramework.ActionsSystem
{
    [Serializable]
    public class ActionStatisticsModifier
    {
        public string tag;
        public float value;
        public OperationType opType;
        public UltimateFramework.Utils.ValueType valueType;
        public int Index { get; set; }
    }
}
