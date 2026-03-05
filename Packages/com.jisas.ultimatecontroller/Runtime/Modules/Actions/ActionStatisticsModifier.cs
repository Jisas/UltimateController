using UltimateController.Utils;
using System;

namespace UltimateController.ActionsSystem
{
    [Serializable]
    public class ActionStatisticsModifier
    {
        public string tag;
        public float value;
        public OperationType opType;
        public UltimateController.Utils.ValueType valueType;
        public int Index { get; set; }
    }
}
