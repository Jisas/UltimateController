
namespace UltimateFramework.StatisticsSystem
{
    public class SoftCap
    {
        public int Level { get; set; }
        public float Multiplier { get; set; }

        public SoftCap(int level, float multiplier)
        {
            Level = level;
            Multiplier = multiplier;
        }
    }

}
