using UltimateController.Utils;
using System;
using MyBox;

namespace UltimateController.RewardSystem
{
    [Serializable]
    public class ObjectsWrapper : CollectionWrapper<string> { }

    [Serializable]
    public class Reward
    {
        public RewardType type;
        [MyBox.ConditionalField(nameof(type), false, RewardType.Economy, RewardType.Both)]
        public int economyAmmount;
        [MyBox.ConditionalField(nameof(type), false, RewardType.Items, RewardType.Both)]
        public ObjectsWrapper items;
    }
}
