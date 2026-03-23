using UltimateFramework.StatisticsSystem;
using UnityEngine.UI;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public TagSelector healthTag;
    public Slider healthSlider;
    [Space(10)]
    public TagSelector manaTag;
    public Slider manahSlider;
    [Space(10)]
    public TagSelector staminaTag;
    public Slider staminaSlider;

    private Statistic healthStat;
    private Statistic manaStat;
    private Statistic staminaStat;
    private StatisticsComponent characterStats;

    void Start()
    {
        characterStats = transform.root.GetComponent<StatisticsComponent>();

        healthStat = characterStats.FindStatistic(healthTag.tag);
        manaStat = characterStats.FindStatistic(manaTag.tag);
        staminaStat = characterStats.FindStatistic(staminaTag.tag);

        healthSlider.minValue = 0;
        manahSlider.minValue = 0;
        staminaSlider.minValue = 0;

        healthSlider.maxValue = healthStat.startMaxValue;
        manahSlider.maxValue = manaStat.startMaxValue;
        staminaSlider.maxValue = staminaStat.startMaxValue;
    }

    void Update()
    {
        healthSlider.maxValue = healthStat.CurrentMaxValue;
        manahSlider.maxValue = manaStat.CurrentMaxValue;
        staminaSlider.maxValue = staminaStat.CurrentMaxValue;

        healthSlider.value = healthStat.CurrentValue;
        manahSlider.value = manaStat.CurrentValue;
        staminaSlider.value = staminaStat.CurrentValue;
    }
}
