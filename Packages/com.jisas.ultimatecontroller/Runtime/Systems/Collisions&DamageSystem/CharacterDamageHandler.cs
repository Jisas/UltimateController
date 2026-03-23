using UltimateFramework.StatisticsSystem;
using UltimateFramework.InventorySystem;
using UltimateFramework.ItemSystem;
using System.Collections.Generic;
using UltimateFramework.Inputs;
using UltimateFramework.Utils;
using UnityEngine;
using System;

namespace UltimateFramework.CollisionsAndDamageSystem
{
    [RequireComponent(typeof(StatisticsComponent), typeof(EntityActionInputs), typeof(InventoryAndEquipmentComponent))]
    public class CharacterDamageHandler : DamageComponent
    {
        #region Serialized Fields
        [SerializeField] private TagSelector healthTag;
        [SerializeField] private TakeDefenceValuesAs defenceValueAs;
        [SerializeField] private float blinkIntensity = 10.0f;
        [SerializeField] private float blinkDuration = 0.05f;
        #endregion

        #region Properties
        public bool CanTakeDamage { get; set; } = true;
        public Vector3 LastHitDirection { get; private set; }
        #endregion

        #region Private Fields
        private float blinkTimer;
        private SkinnedMeshRenderer skinnedMesh;
        private readonly List<ItemStat> armatureStats = new();
        #endregion

        #region Mono
        private void Awake()
        {
            m_StatsAnAttributes = GetComponent<StatisticsComponent>();
            m_EntityActionInputs = GetComponent<EntityActionInputs>();
            m_InventoryAndEquipment = GetComponent<InventoryAndEquipmentComponent>();
            skinnedMesh = GetComponentInChildren<SkinnedMeshRenderer>();
        }
        private void FixedUpdate()
        {
            blinkTimer -= Time.fixedDeltaTime;
            float lerp = Mathf.Clamp01(blinkTimer / blinkDuration);
            float intensity = gameObject.CompareTag("Enemy") ? lerp * blinkIntensity : (lerp * blinkIntensity) + 1.0f;
            skinnedMesh.material.color = Color.white * intensity;
        }
        #endregion

        #region Internal
        private void AddArmatureStat(GameObject armorPiece, string damageSplitTag)
        {
            if (armorPiece != null)
            {
                if (armorPiece.TryGetComponent<ArmorBehaviour>(out var armorBehavior))
                {
                    var stat = armorBehavior.Item.FindStat($"Attribute.Defence.{damageSplitTag}");

                    if (stat != null)
                        armatureStats.Add(stat);
                }
            }
        }
        private bool SelfDamageVerification(GameObject enemyWeapon)
        {
            var myMainWeapon = m_InventoryAndEquipment.GetCurrentRightWeaponObject();
            var myOffHandWeapon = m_InventoryAndEquipment.GetCurrentLeftWeaponObject();

            if (enemyWeapon != null && myMainWeapon != null && myOffHandWeapon != null)
            {
                if (myMainWeapon.GetInstanceID() != enemyWeapon.GetInstanceID() && myOffHandWeapon.GetInstanceID() != enemyWeapon.GetInstanceID())
                    return false;
            }
            else if (enemyWeapon != null && myMainWeapon != null && myOffHandWeapon == null)
            {
                if (myMainWeapon.GetInstanceID() != enemyWeapon.GetInstanceID())
                    return false;
            }

            return true;
        }
        private float CalculeDamageReductionByDefence(ref float currentDamage, float defenceValue, TakeDefenceValuesAs defenceValueAs)
        {
            float result;

            if (defenceValueAs == TakeDefenceValuesAs.Flat) 
                 result = currentDamage -= defenceValue;
            else result = currentDamage -= (currentDamage * defenceValue / 100);

            if(result <= 0) return 0;

            return result;
        }
        #endregion

        #region Public Methods
        public void ReceiveDamage(GameObject weapon, float damage, Vector3 hitDirection)
        {
            if (!CanTakeDamage) return;
            if (SelfDamageVerification(weapon)) return;

            LastHitDirection = Vector3.Normalize(hitDirection);
            var weaponComponent = weapon.GetComponent<WeaponComponent>();
            var weaponItem = weaponComponent.Item;

            DefenceComponent myCurrentBlockingWeapon;
            var myLeftWeapon = m_InventoryAndEquipment.GetCurrentOffHandWeapon();
            var myRightWeapon = m_InventoryAndEquipment.GetCurrentMainWeapon();
            
            if (myLeftWeapon?.WeaponComponent.Item.hand == WeaponHand.OffHand)
                 myCurrentBlockingWeapon = myLeftWeapon.WeaponComponent.DefenceComponent;
            else myCurrentBlockingWeapon = myRightWeapon.WeaponComponent.DefenceComponent;

            #region Visual Feedback
            if (myCurrentBlockingWeapon == null || !myCurrentBlockingWeapon.IsBlocking)
            {
                blinkTimer = blinkDuration;
                var inputAction = m_EntityActionInputs.FindInputAction("Hit");

                if (inputAction != null) 
                    inputAction.ExecuteAction(inputAction.PrimaryAction.actionTag.tag, 
                    inputAction.PrimaryAction.priority, weaponItem.name, inputAction.PrimaryAction.isBaseAction);
                else Debug.LogWarning("There is no input action with the name: \"Hit\".");
            }
            #endregion

            #region Get Info
            string damageType = weaponItem.damageType;
            string[] parts = damageType.Split('.');
            string damageSplitTag;

            try {
                damageSplitTag = parts[1];
            }
            catch (IndexOutOfRangeException)
            {
                Debug.LogError($"The damage type: {damageType}, of the weapon: {weaponItem.name}, is not valid; Please assign a valid damage type to the weapon in order to verify the defenses.");
                throw;
            }
            #endregion

            #region Damage Calculation
            var startDamage = damage;
            var characterDefenceAttr = m_StatsAnAttributes.FindAttribute($"Attribute.Defence.{damageSplitTag}", 
                $"'{gameObject.name}' not have defence for the damage type: {damageSplitTag}.");

            armatureStats.Clear();
            AddArmatureStat(m_InventoryAndEquipment.EquipedHelment, damageSplitTag);
            AddArmatureStat(m_InventoryAndEquipment.EquipedArmor, damageSplitTag);
            AddArmatureStat(m_InventoryAndEquipment.EquipedGloves, damageSplitTag);
            AddArmatureStat(m_InventoryAndEquipment.EquipedPants, damageSplitTag);
            AddArmatureStat(m_InventoryAndEquipment.EquipedBoots, damageSplitTag);

            if (characterDefenceAttr != null)
            {
                // Si es así, reduce el daño en función del valor de defensa
                float defenseValue = characterDefenceAttr.CurrentValue;
                damage = CalculeDamageReductionByDefence(ref damage, defenseValue, defenceValueAs);
            }

            foreach ( var itemStat in armatureStats )
            {
                if (itemStat != null)
                {
                    float defenseValue = itemStat.CurrentValue;
                    damage = CalculeDamageReductionByDefence(ref damage, defenseValue, defenceValueAs);
                }
            }

            if (myCurrentBlockingWeapon.IsBlocking)
            {
                string blockTag = myCurrentBlockingWeapon.blockTag.tag;

                if (!String.IsNullOrEmpty(blockTag))
                {
                    foreach (var data in myCurrentBlockingWeapon.blockData)
                    {
                        var currentBlockDefence = m_StatsAnAttributes.FindAttribute(blockTag, $"'{gameObject.name}' not have an attribute for: {blockTag}.");

                        if (currentBlockDefence != null)
                            damage = CalculeDamageReductionByDefence(ref damage, data.blockPercentage, TakeDefenceValuesAs.Percentage);
                    }
                }
                else Debug.LogError($"You must assign a value to the \"BlockTag\" field of the \"DefenceComponent\" component " +
                    $"of the weapon: {weaponItem.name} in order to block the damage.");
            }

            // Set Damage
            this.m_StatsAnAttributes.DecreaseCurrentValueOfStat(healthTag.tag, damage);
            #endregion

            #region Debug
            Debug.Log($"Take a starting damage of: {startDamage}, and final dame of: {damage}");
            #endregion
        }
        public Vector3 HitDirectionVerifier(Vector3 hitDirection)
        {
            return transform.InverseTransformDirection(hitDirection);
        }
        #endregion
    }
}