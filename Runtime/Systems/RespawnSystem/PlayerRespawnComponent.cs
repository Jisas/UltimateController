using UltimateFramework.SerializationSystem;
using UltimateFramework.LocomotionSystem;
using UltimateFramework.StatisticsSystem;
using UltimateFramework.InventorySystem;
using UltimateFramework.Inputs;
using UltimateFramework.Tools;
using System.Collections;
using UnityEngine;

namespace UltimateFramework.RespawnSystem
{
    public class PlayerRespawnComponent : BaseEntityRespawnComponent
    {
        [SerializeField] private TagSelector healthTag;
        [SerializeField] private Animator HUDAnimator;

        #region Private Fields
        private InventoryAndEquipmentComponent m_InventoryAndEquipmentComponent;
        private BaseLocomotionComponent m_LocomotionComponent;
        private CharacterController m_CharacterController;
        private StatisticsComponent m_StatisticsComponent;
        private Animator m_Animator;
        #endregion

        private void Awake()
        {
            m_InventoryAndEquipmentComponent = GetComponent<InventoryAndEquipmentComponent>();
            m_LocomotionComponent = GetComponent<BaseLocomotionComponent>();
            m_CharacterController = GetComponent<CharacterController>();
            m_StatisticsComponent = GetComponent<StatisticsComponent>();
            m_Animator = GetComponent<Animator>();
        }

        [ContextMenu("Save Respawn Data")]
        public void SetRespawnWithCurrentData() => SetCurrentRespawn(transform.position, transform.rotation);
        public override IEnumerator Respawn()
        {
            transform.SetPositionAndRotation(
                DataGameManager.Instance.GetPlayerData().position.ToVector3(),
                DataGameManager.Instance.GetPlayerData().rotation.ToQuaternion());

            yield return new WaitForSeconds(WaitTimeToRespawnCoroutine() + 3f);
            m_Animator.SetTrigger("Getup");

            yield return new WaitForSeconds(0.3f);
            var health = m_StatisticsComponent.FindStatistic(healthTag.tag);
            health.CurrentValue = health.CurrentMaxValue;

            var item = SettingsMasterData.Instance.itemDB.FindItem("Health Potion");
            var equipmentSlot = m_InventoryAndEquipmentComponent.GetEquipmentSlot(item.index);

            if (equipmentSlot != null && !equipmentSlot.SlotInfo.isEmpty)
            {
                int amount = 10 - equipmentSlot.SlotInfo.amount;
                equipmentSlot.SlotInfo.amount += amount;
                equipmentSlot.UpdateFAUI();
                equipmentSlot.UpdateUI();
            }
            else
            {
                int amount = 10;
                m_InventoryAndEquipmentComponent.AddItem(item.index, amount);
                m_InventoryAndEquipmentComponent.EquipItem(item, 3, amount);
            }

            yield return new WaitForSeconds(0.3f);
            HUDAnimator.Play("HUD_Enter");
            m_CharacterController.enabled = true;
            m_LocomotionComponent.CanMove = true;
            InputsManager.EnablePlayerMap(true);
        }
    }
}