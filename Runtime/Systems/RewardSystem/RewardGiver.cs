using UltimateFramework.InventorySystem;
using UltimateFramework.EconomySystem;
using UltimateFramework.DeadSystem;
using System.Collections.Generic;
using UltimateFramework.Tools;
using UltimateFramework.Utils;
using UnityEngine;

namespace UltimateFramework.RewardSystem
{
    [RequireComponent(typeof(BaseDeadComponent))]
    public class RewardGiver : MonoBehaviour
    {
        public Reward reward = new();
        private BaseDeadComponent deadComponent;

        private void Awake()
        {
            deadComponent = GetComponent<BaseDeadComponent>();
        }
        private void OnEnable()
        {
            deadComponent.OnDead += SetRewards;
        }

        private object GetRewarByType(RewardType type)
        {
            return type switch
            {
                RewardType.Economy => reward.economyAmmount,
                RewardType.Items => reward.items,
                RewardType.Both => reward,
                _ => null,
            };
        }
        private void SetRewards(GameObject reciver)
        {
            object obj = GetRewarByType(reward.type);

            if (obj is int value)
            {
                var entityEconomy = reciver.GetComponent<EconomyComponent>();
                entityEconomy.AddToEconomy(value);
            }
            else if (obj is List<string> list)
            {
                var inventory = reciver.GetComponent<InventoryAndEquipmentComponent>();
                foreach (var itemName in list)
                {
                    int currentItemID = SettingsMasterData.Instance.itemDB.GetItemID(itemName);
                    inventory.AddItem(currentItemID);
                }
            }
            else if (obj is Reward entityReward)
            {
                var entityEconomy = reciver.GetComponent<EconomyComponent>();
                var inventory = reciver.GetComponent<InventoryAndEquipmentComponent>();

                entityEconomy.AddToEconomy(entityReward.economyAmmount);
                foreach (var itemName in entityReward.items.Value)
                {
                    int currentItemID = SettingsMasterData.Instance.itemDB.GetItemID(itemName);
                    inventory.AddItem(currentItemID);
                }
            }
        }
    }
}