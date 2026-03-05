using UltimateController.StatisticsSystem;
using System.Collections.Generic;
using System;

namespace UltimateController.SerializationSystem
{
    [Serializable]
    public class StatsAndAttributesData
    {
        public List<AttributeData> primaryAttributes;
        public List<AttributeData> attributes;
        public List<Statistic> stats;
    }
}
