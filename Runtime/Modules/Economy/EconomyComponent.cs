using UltimateFramework.SerializationSystem;
using UltimateFramework.Commons;
using UltimateFramework.Utils;
using UnityEngine;
using System;

namespace UltimateFramework.EconomySystem
{
    [RequireComponent(typeof(EntityManager))]
    public class EconomyComponent : MonoBehaviour
    {
        [SerializeField, ReadOnly] protected int economy;
        public Action<int> OnEconomyChange;

        private void OnEnable()
        {
            GetComponent<EntityManager>().OnPlayerDataSave += SaveEconomy;
        }
        private void Start()
        {
            if (DataGameManager.IsDataSaved())
                SetEconomy(DataGameManager.Instance.GetPlayerData().coins);
        }

        public int GetEconomy() => economy;
        public void SetEconomy(int newValue)
        {
            economy = newValue;
            OnEconomyChange.Invoke(economy);
        }
        public void AddToEconomy(int value)
        {
            economy += value;
            OnEconomyChange.Invoke(economy);
        }
        public void SubstractToEconomy(int value)
        {
            economy -= value;
            OnEconomyChange.Invoke(economy);
        }
        public void SaveEconomy()
        {
            DataGameManager.Instance.SetCoins(economy);
            DataGameManager.Instance.SavePlayerData();
        }
    }
}
