using UltimateFramework.StatisticsSystem;
using UltimateFramework.InventorySystem;
using UltimateFramework.ItemSystem;
using Ultimateframework.FXSystem;
using System.Collections.Generic;
using UltimateFramework.Inputs;
using UltimateFramework.Utils;
using System.Collections;
using UnityEngine.VFX;
using UnityEngine;
using System;
using MyBox;

namespace UltimateFramework.CollisionsAndDamageSystem
{
    [Serializable]
    public class MeshRenderers : CollectionWrapper<MeshRenderer> { }
    [Serializable]
    public class SinnedMeshRenderers : CollectionWrapper<SkinnedMeshRenderer> { }
    [Serializable]
    public class HitVFX : CollectionWrapper<GameObject> { }
    [Serializable]
    public class TextPoints : CollectionWrapper<Transform> { }

    [RequireComponent(typeof(VFXManager))]
    [RequireComponent(typeof(StatisticsComponent), typeof(EntityActionInputs), typeof(InventoryAndEquipmentComponent))]
    public class CharacterDamageHandler : DamageComponent
    {
        #region Serialized Fields
        [Space(10)]
        [SerializeField] private TagSelector healthTag;
        [SerializeField] private TakeDefenceValuesAs defenceValueAs;

        [Space(10)]
        [SerializeField] private bool allowHitAction = true;
        [SerializeField] private bool useHitEffects = true;
        [SerializeField] private bool useScreenEffect = true;
        [SerializeField] private float blinkDuration = 0.05f;
        [SerializeField] private MaterialAllocatorType materialAllocatrType;
        [Space]
        [MyBox.ConditionalField(nameof(materialAllocatrType), false, MaterialAllocatorType.MeshRendedrer)]
        public MeshRenderers meshRenderers;
        [MyBox.ConditionalField(nameof(materialAllocatrType), false, MaterialAllocatorType.SkinnedMesh)]
        public SinnedMeshRenderers skinnedMeshRenderers;
        [MyBox.ConditionalField(nameof(useHitEffects), false, true)] 
        public HitVFX hitVFX;
        #endregion

        #region Properties
        public bool CanTakeDamage { get; set; } = true;
        public Vector3 LastHitDirection { get; private set; }
        #endregion

        #region DelegatesAndEvents
        public delegate void OnUITakeDamageHanddler();
        public event OnUITakeDamageHanddler OnUITakeDamage;
        #endregion

        #region Private Fields
        private Material m_BlinkMaterial;
        private FXManagerComponent m_VFXManager;
        private readonly List<ItemStat> armatureStats = new();
        private readonly List<Material> _tempMaterialsAllocator = new();
        #endregion

        #region Mono
        private void Awake()
        {
            m_VFXManager = GetComponent<FXManagerComponent>();
            m_StatsAndAttributes = GetComponent<StatisticsComponent>();
            m_EntityActionInputs = GetComponent<EntityActionInputs>();
            m_InventoryAndEquipment = GetComponent<InventoryAndEquipmentComponent>();
            m_BlinkMaterial = Resources.Load<Material>("Materials/Blink_Material");
            SetTempMaterials();
        }
        #endregion

        #region Internal
        private void SetTempMaterials()
        {
            if (materialAllocatrType == MaterialAllocatorType.MeshRendedrer)
            {
                foreach (var meshRender in meshRenderers.Value)
                {
                    for (int i = 0; i < meshRender.materials.Length; i++)
                        _tempMaterialsAllocator.Add(meshRender.materials[i]);
                }
            }
            else
            {
                foreach (var meshRender in skinnedMeshRenderers.Value)
                {
                    for (int i = 0; i < meshRender.materials.Length; i++)
                        _tempMaterialsAllocator.Add(meshRender.materials[i]);
                }
            }
        }
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

            if (myMainWeapon == null && myOffHandWeapon == null) 
                return false;

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
        private IEnumerator BlinkCoroutine()
        {
            yield return new WaitForSeconds(blinkDuration);

            if (materialAllocatrType == MaterialAllocatorType.MeshRendedrer)
            {
                for (int i = 0; i < meshRenderers.Value.Length; i++)
                    meshRenderers.Value[i].sharedMaterial = _tempMaterialsAllocator[i];
            }
            else
            {
                for (int i = 0; i < skinnedMeshRenderers.Value.Length; i++)
                    skinnedMeshRenderers.Value[i].sharedMaterial = _tempMaterialsAllocator[i];
            }

            StopCoroutine(BlinkCoroutine());
        }
        private IEnumerator WaitToRegenerate()
        {
            var health = m_StatsAndAttributes.FindStatistic(healthTag.tag);
            if (health.hasRegeneration)
            {
                yield return new WaitForSeconds(health.regenDelay);
                m_StatsAndAttributes.CanRegenerateStats = true;
            }
        }
        #endregion

        #region Public Methods
        public void ReceiveDamage(GameObject weapon, float damage, Vector3 hitDirection, Vector3 contactPoint)
        {
            if (!CanTakeDamage) return;
            if (SelfDamageVerification(weapon)) return;

            #region Set Data
            m_StatsAndAttributes.CanRegenerateStats = false;
            LastHitDirection = Vector3.Normalize(hitDirection);
            var weaponComponent = weapon.GetComponent<WeaponComponent>();
            var weaponItem = weaponComponent.Item;

            DefenceComponent myCurrentBlockingWeapon = null;
            var myLeftWeapon = m_InventoryAndEquipment.GetCurrentOffHandWeapon();
            var myRightWeapon = m_InventoryAndEquipment.GetCurrentMainWeapon();

            if (myLeftWeapon != null || myRightWeapon != null) 
            {
                if (myLeftWeapon?.WeaponComponent.Item.hand == WeaponHand.OffHand)
                     myCurrentBlockingWeapon = myLeftWeapon.WeaponComponent.DefenceComponent;
                else myCurrentBlockingWeapon = myRightWeapon.WeaponComponent.DefenceComponent;
            }
            #endregion

            #region Visual Feedback
            if (myCurrentBlockingWeapon == null || !myCurrentBlockingWeapon.IsBlocking)
            {
                if (materialAllocatrType == MaterialAllocatorType.MeshRendedrer)
                {
                    for (int i = 0; i < meshRenderers.Value.Length; i++)
                        meshRenderers.Value[i].sharedMaterial = m_BlinkMaterial;
                }
                else
                {
                    for (int i = 0; i < skinnedMeshRenderers.Value.Length; i++)
                        skinnedMeshRenderers.Value[i].sharedMaterial = m_BlinkMaterial;
                }

                var intensity = UnityEngine.Random.Range(0.6f, 1f);

                if (useScreenEffect) m_VFXManager.ScreenDamageEffect(intensity);
                StartCoroutine(BlinkCoroutine());

                if (allowHitAction)
                {
                    var inputAction = m_EntityActionInputs.FindInputAction("Hit");

                    if (inputAction != null)
                    {
                        inputAction.ExecuteAction(inputAction.PrimaryAction.actionTag.tag,
                        inputAction.PrimaryAction.priority, inputAction.PrimaryAction.isBaseAction);
                    }
                    else Debug.LogWarning("There is no input action with the name: \"Hit\".");
                }

                OnUITakeDamage?.Invoke();

                if (useHitEffects)
                {
                    var randomNum = UnityEngine.Random.Range(0, hitVFX.Value.Length);
                    Instantiate(hitVFX.Value[randomNum], contactPoint, Quaternion.identity);
                }
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
            var characterDefenceAttr = m_StatsAndAttributes.FindAttribute($"Attribute.Defence.{damageSplitTag}");

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

            if (myCurrentBlockingWeapon != null && myCurrentBlockingWeapon.IsBlocking)
            {
                string blockTag = myCurrentBlockingWeapon.blockTag.tag;

                if (!String.IsNullOrEmpty(blockTag))
                {
                    foreach (var data in myCurrentBlockingWeapon.blockData)
                    {
                        var currentBlockDefence = m_StatsAndAttributes.FindAttribute(blockTag, $"'{gameObject.name}' not have an attribute for: {blockTag}.");

                        if (currentBlockDefence != null)
                            damage = CalculeDamageReductionByDefence(ref damage, data.blockPercentage, TakeDefenceValuesAs.Percentage);
                    }
                }
                else Debug.LogError($"You must assign a value to the \"BlockTag\" field of the \"DefenceComponent\" component " +
                    $"of the weapon: {weaponItem.name} in order to block the damage.");
            }

            // Set Damage
            this.m_StatsAndAttributes.DecreaseCurrentValueOfStat(healthTag.tag, damage);

            // Wait and start reageneration if it has
            StartCoroutine(WaitToRegenerate());
            #endregion

            #region Dead Comprobement
            if (m_StatsAndAttributes.FindStatistic(healthTag.tag).CurrentValue <= 0)
            {
                var inputAction = m_EntityActionInputs.FindInputAction("Dead");

                if (inputAction != null)
                {
                    inputAction.ExecuteAction(inputAction.PrimaryAction.actionTag.tag,
                    inputAction.PrimaryAction.priority, inputAction.PrimaryAction.isBaseAction);
                }
                else Debug.LogWarning("There is no input action with the name: \"Dead\".");
            }
            #endregion

            #region Debug
            //Debug.Log($"Take a starting damage of: {startDamage}, and final dame of: {damage}");
            #endregion
        }
        public Vector3 HitDirectionVerifier(Vector3 hitDirection)
        {
            return transform.InverseTransformDirection(hitDirection);
        }
        #endregion
    }
}