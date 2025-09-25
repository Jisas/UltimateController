using UltimateFramework.SerializationSystem;
using System.Collections.Generic;
using UltimateFramework.Commons;
using System.Collections;
using UnityEngine;


namespace UltimateFramework.StatisticsSystem
{
    public class StatisticsComponent : UFBaseComponent
    {
        #region Public Fields
        [Header("Attributes and Stats")]
        public List<Attribute> primaryAttributes = new();
        public List<Attribute> attributes = new();
        public List<Statistic> stats = new();
        public delegate float Operation(float a, float b, bool isPercentage, float c = 0);
        #endregion

        #region Private Fields
        private Dictionary<string, Attribute> primaryAttributesDict = new();
        private Dictionary<string, Attribute> attributesDict = new();
        private Dictionary<string, Statistic> statsDict = new();
        private Dictionary<string, List<SoftCap>> softCaps = new();
        private EntityManager m_EntityManager;
        private bool canRegenerateStats;
        private bool canConsumeStats;
        #endregion

        #region Properties
        public Operation Sum { get; private set; }
        public Operation Subtract { get; private set; }
        public bool CanRegenerateStats { get => canRegenerateStats; set => canRegenerateStats = value; }
        public bool CanConsumeStats { get => canConsumeStats; set => canConsumeStats = value; }
        #endregion

        #region Mono
        private void Awake()
        {
            m_EntityManager = GetComponent<EntityManager>();

            Sum = (a, b, isPercentage, c) => SumOP(a, b, isPercentage, c);
            Subtract = (a, b, isPercentage, c) => isPercentage ? a - (c * b / 100) : a - b;

            primaryAttributesDict = new Dictionary<string, Attribute>();
            attributesDict = new Dictionary<string, Attribute>();
            statsDict = new Dictionary<string, Statistic>();

            //softCaps = new Dictionary<string, List<SoftCap>>
            //{
            //    { "Vigor", new List<SoftCap> { new(30, 0.5f), new(50, 0.2f) } },
            //    // Añade más atributos y sus soft caps aquí
            //};
        }
        private void Start() => StartCoroutine(DelayedStart());
        private IEnumerator DelayedStart()
        {
            yield return new WaitForEndOfFrame();

            if (gameObject.CompareTag("Player"))
            {
                m_EntityManager.OnPlayerDataSave += SaveAttributesAndStats;

                if (DataGameManager.IsStatsAndAttributesDataSaved())
                    SetUpSavedValueForAttributesAndStats();
                else SetUpStartValueForAttributesAndStats();
            }
            else SetUpStartValueForAttributesAndStats();

            SetUpDictionaries();
        }
        private void Update()
        {
            foreach (var stat in stats)
            {
                if (stat.hasRegeneration && stat.CurrentValue < stat.CurrentMaxValue)
                {
                    StartCoroutine(DelayToRegenerateStat(stat));
                }
            }
        }
        #endregion

        #region Internal
        private float SumOP(float a, float b, bool isPercentage, float c)
        {
            if (isPercentage)
            {
                var finalValue = a / CalculatePercentajeConvertion(a, b);
                return finalValue;
            }
            else return a + b;
        }
        private float CalculatePercentajeConvertion(float inicialValue, float percentaje)
        {
            var percentageValue = (inicialValue * percentaje) / 100;
            var ajustedValue = inicialValue + percentageValue;
            var percentageConvertion = inicialValue / ajustedValue;
            return percentageConvertion;
        }
        IEnumerator DelayToRegenerateStat(Statistic stat)
        {
            if (!stat.IsRegenerating)
                yield return new WaitForSeconds(stat.regenDelay);

            if (canRegenerateStats)
            {
                stat.IsRegenerating = true;
                stat.CurrentValue += Time.deltaTime * stat.regenValue;

                if (stat.CurrentValue >= stat.CurrentMaxValue)
                {
                    stat.CurrentValue = stat.CurrentMaxValue;
                    stat.IsRegenerating = false;
                }
            }
            else
            {
                stat.IsRegenerating = false;
                yield break;
            }
        }
        private void SetUpStartValueForAttributesAndStats()
        {
            foreach (var stat in stats)
            {
                if (stat.startFromZero)
                {
                    stat.CurrentValue = 0;
                }
                else
                {
                    stat.CurrentMaxValue = stat.startMaxValue;
                    stat.CurrentValue = stat.startMaxValue;
                }
            }
            foreach (var patt in primaryAttributes)
            {
                patt.CurrentValue = patt.startValue;
            }
            foreach (var attr in attributes)
            {
                attr.CurrentValue = attr.startValue;
            }
        }
        private void SetUpSavedValueForAttributesAndStats()
        {
            var savedPrimaryAttributes = DataGameManager.Instance.GetStatsAndAttributesData().primaryAttributes;
            for (var i = 0; i < savedPrimaryAttributes.Count; i++)
            {
                primaryAttributes[i].LoadFromSerializableData(savedPrimaryAttributes[i]);
            }

            var savedAttributes = DataGameManager.Instance.GetStatsAndAttributesData().attributes;
            for (var i = 0; i < savedAttributes.Count; i++)
            {
                attributes[i].LoadFromSerializableData(savedAttributes[i]);
            }

            stats = DataGameManager.Instance.GetStatsAndAttributesData().stats;
        }
        private void SetUpDictionaries()
        {
            foreach (var pattr in primaryAttributes)
            {
                primaryAttributesDict[pattr.attributeType.tag] = pattr;
            }
            foreach (var attr in attributes)
            {
                attributesDict[attr.attributeType.tag] = attr;
            }
            foreach (var stat in stats)
            {
                statsDict[stat.statType.tag] = stat;
            }
        }
        #endregion

        #region Public Methods
        public Attribute FindPrimaryAttribute(string attributeTag)
        {
            if (primaryAttributesDict.TryGetValue(attributeTag, out var pattr))
            {
                return pattr;
            }
            return null;
        }
        public Statistic FindStatistic(string statisticTag)
        {
            if (statsDict.TryGetValue(statisticTag, out var stat))
            {
                return stat;
            }
            return null;
        }
        public Attribute FindAttribute(string attributeTag)
        {
            if (attributesDict.TryGetValue(attributeTag, out var attr))
            {
                return attr;
            }
            return null;
        }
        public Attribute FindAttribute(string attributeTag, string nullMessage)
        {
            if (attributesDict.TryGetValue(attributeTag, out var attr))
            {
                return attr;
            }

            Debug.Log(nullMessage);
            return null;
        }
        public void IncreaseAttribute(string attributeTag, float ammount)
        {
            var pattr = FindPrimaryAttribute(attributeTag);
            if (pattr != null)
            {
                pattr.CurrentValue += ammount;
                ApplySoftCaps(pattr);
            }

            var attr = FindAttribute(attributeTag);
            if (attr != null)
            {
                attr.CurrentValue += ammount;
                ApplySoftCaps(attr);
            }
        }
        public void DecreaseAttribute(string attributeTag, float ammount)
        {
            var pattr = FindPrimaryAttribute(attributeTag);
            if (pattr != null) pattr.CurrentValue -= ammount;

            var attr = FindAttribute(attributeTag);
            if (attr != null) attr.CurrentValue -= ammount;
        }
        public void IncreaseMaxValueOfStat(string statTag, float ammount)
        {
            var stat = FindStatistic(statTag);
            if (stat != null) stat.startMaxValue += ammount;
        }
        public void IncreaseCurrentValueOfStat(string statTag, float ammount)
        {
            var stat = FindStatistic(statTag);
            if (stat != null) stat.CurrentValue += ammount;
        }
        public void DecreaseMaxValueOfStat(string statTag, float ammount)
        {
            var stat = FindStatistic(statTag);
            if (stat != null) stat.startMaxValue -= ammount;
        }
        public void DecreaseCurrentValueOfStat(string statTag, float ammount)
        {
            var stat = FindStatistic(statTag);
            if (stat != null) stat.CurrentValue -= ammount;
        }
        private void ApplySoftCaps(Attribute attribute)
        {
            if (softCaps.TryGetValue(attribute.attributeType.tag, out var caps))
            {
                float adjustedValue = attribute.CurrentValue;
                foreach (var cap in caps)
                {
                    if (attribute.CurrentValue > cap.Level)
                    {
                        adjustedValue = cap.Level + (attribute.CurrentValue - cap.Level) * cap.Multiplier;
                    }
                }
                attribute.CurrentValue = adjustedValue;
            }
        }
        public float ApplyModifyAttributesOrStatsOperation(Operation operation, float a, float b, bool isPercentage, float c = 0)
        {
            return operation(a, b, isPercentage, c);
        }

        [ContextMenu("SaveData")]
        public void SaveAttributesAndStats()
        {
            var pAtt = new List<AttributeData>();
            var oAtt = new List<AttributeData>();

            foreach (var primaryAtt in primaryAttributes)
            {
                pAtt.Add(primaryAtt.GetSerializableData());
            }
            foreach (var otherAtt in attributes)
            {
                oAtt.Add(otherAtt.GetSerializableData());
            }

            DataGameManager.Instance.SetPrimaryAttributes(pAtt);
            DataGameManager.Instance.SetOtherAttributes(oAtt);
            DataGameManager.Instance.SetStats(stats);
            DataGameManager.Instance.SaveStatsAndAttributesData();
        }
        #endregion
    }
}

