using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace UltimateFramework.StatisticsSystem
{
    public class StatisticsComponent : MonoBehaviour
    {
        #region Public Fields
        [Header("Attributes and Stats")]
        public List<Attribute> primaryAttributes;
        public List<Statistic> stats;
        public List<Attribute> attributes;
        public delegate float Operation(float a, float b, bool isPercentage, float c = 0);
        #endregion

        #region Private Fields
        private Dictionary<string, Attribute> primaryAttributesDict;
        private Dictionary<string, Attribute> attributesDict;
        private Dictionary<string, Statistic> statsDict;
        private bool canRegenerateStats;
        private bool canConsumeStats;
        #endregion

        #region Properties
        public Operation SumOnCurrentValue { get; private set; }
        public Operation MultiplyOnCurrentValue { get; private set; }
        public Operation SubtractOnCurrentValue { get; private set; }
        public Operation SumOnBaseOrMaxValue { get; private set; }
        public Operation MultiplyOnBaseOrMaxValue { get; private set; }
        public Operation SubtractOnBaseOrMaxValue { get; private set; }
        public bool CanRegenerateStats { get => canRegenerateStats; set => canRegenerateStats = value; }
        public bool CanConsumeStats { get => canConsumeStats; set => canConsumeStats = value; }
        #endregion

        #region Mono
        private void Awake()
        {
            // Set delegates for current values
            SumOnCurrentValue = (a, b, isPercentage, c) => isPercentage ? a + (a * b / 100) : a + b;
            MultiplyOnCurrentValue = (a, b, isPercentage, c) => isPercentage ? a * (a * b / 100) : a * b;
            SubtractOnCurrentValue = (a, b, isPercentage, c) => isPercentage ? a - (a * b / 100) : a - b;

            // Set delegates for base or max values
            SumOnBaseOrMaxValue = (a, b, isPercentage, c) => isPercentage ? a + (c * b / 100) : a + b;
            MultiplyOnBaseOrMaxValue = (a, b, isPercentage, c) => isPercentage ? a * (c * b / 100) : a * b;
            SubtractOnBaseOrMaxValue = (a, b, isPercentage, c) => isPercentage ? a - (c * b / 100) : a - b;

            primaryAttributesDict = new Dictionary<string, Attribute>();
            attributesDict = new Dictionary<string, Attribute>();
            statsDict = new Dictionary<string, Statistic>();

            SetUpStartValueForAttributesAndStats();
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
            if (pattr != null) pattr.CurrentValue += ammount;

            var attr = FindAttribute(attributeTag);
            if (attr != null) attr.CurrentValue += ammount;
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
        public float ApplyModifyAttributesOrStatsOperation(Operation operation, float a, float b, bool isPercentage, float c = 0)
        {
            return operation(a, b, isPercentage, c);
        }
        #endregion

        #region Testing
        [ContextMenu("Aumentar 10 al atributo Strenght")]
        public void TestStrengthUp()
        {
            IncreaseAttribute("PrimaryAttribute.Strenght", 10);
        }
        #endregion
    }
}

