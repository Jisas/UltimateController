using UltimateFramework.SerializationSystem;
using UltimateFramework.RespawnSystem;
using UnityEngine.Events;
using UnityEngine;

namespace UltimateFramework.Commons
{
    public class EntityManager : UFBaseComponent
    {
        public UnityAction OnPlayerDataSave;
        public EntityState State { get; set; } = EntityState.Normal;

        private PlayerRespawnComponent m_RespawnComponent;

        private void Awake() => m_RespawnComponent = GetComponent<PlayerRespawnComponent>();
        private void OnEnable() => OnPlayerDataSave += SavePositionAnRotation;

        public void SaveAllPlayerData() => OnPlayerDataSave?.Invoke();
        public void SavePlayerDataWithoutPosAndRot()
        {
            OnPlayerDataSave -= SavePositionAnRotation;
            OnPlayerDataSave?.Invoke();
        }

        [ContextMenu("Save Position and Rotation")]
        public void SavePositionAnRotation()
        {
            DataGameManager.Instance.SetPosition(m_RespawnComponent.GetCurreRespawn().position);
            DataGameManager.Instance.SetRotation(m_RespawnComponent.GetCurreRespawn().rotation);
            DataGameManager.Instance.SavePlayerData();
        }
    }
}