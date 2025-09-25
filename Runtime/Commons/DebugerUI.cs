using UltimateFramework.StatisticsSystem;
using UltimateFramework.LocomotionSystem;
using UltimateFramework.InventorySystem;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;


public class DebugerUI : MonoBehaviour
{
    public bool enableDebugMode;

    // UI elements
    private UIDocument document;
    private VisualElement root;
    private VisualTreeAsset debugElement;

    //  Locomotion elements
    private Label locomotionType;
    private Label locomotionMode;
    private Label speed;
    private Label dirx;
    private Label diry;

    // Stats and attributes elements
    private VisualElement statsContainer;

    // Equpment elements
    private Label mainWeapon;
    private Label offHandWeapon;

    // References
    private TPPlayerLocomotion m_Locomotion;
    private StatisticsComponent m_Statistics;
    private InventoryAndEquipmentComponent m_InventoryAndEquipment;

    // Other
    private readonly List<TemplateContainer> newStatsAndAttributesWlements = new();

    void Start()
    {
        if (enableDebugMode)
        {
            document = GetComponent<UIDocument>();
            document.enabled = true;

            root = document.rootVisualElement;
            debugElement = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/Templates/DebugElement");

            //  Locomotion elements
            locomotionType = root.Q<Label>("locomotionType-value");
            locomotionMode = root.Q<Label>("locomotionMode-value");
            speed = root.Q<Label>("speed-value");
            dirx = root.Q<Label>("dirx-value");
            diry = root.Q<Label>("diry-value");

            // Stats and attributes elements
            statsContainer = root.Q<VisualElement>("character-stats");

            // Equpment elements
            mainWeapon = root.Q<Label>("mainWeapon-value");
            offHandWeapon = root.Q<Label>("offHandWeapon-value");

            // References
            m_Locomotion = transform.root.GetComponent<TPPlayerLocomotion>();
            m_Statistics = transform.root.GetComponent<StatisticsComponent>();
            m_InventoryAndEquipment = transform.root.GetComponent<PlayerInventoryAndEquipment>();

            StatsAndAttributesInicialization();
        }
        else
        {
            document = GetComponent<UIDocument>();
            document.enabled = false;
        }
    }

    private void Update()
    {
        if (enableDebugMode)
        {
            #region Locomotion
            locomotionType.text = m_Locomotion.CurrentLocomotionType.ToString();
            locomotionMode.text = m_Locomotion.CurrentLocomotionMode.ToString();
            speed.text = m_Locomotion.CurrentSpeed.ToString();
            dirx.text = m_Locomotion.CurrentMoveDirection.x.ToString();
            diry.text = m_Locomotion.CurrentMoveDirection.y.ToString();
            #endregion

            #region Stats And Attributes
            foreach (var elemet in newStatsAndAttributesWlements)
            {
                Label name = elemet.Q<Label>("name");
                Label value = elemet.Q<Label>("value");

                foreach (var pattr in m_Statistics.primaryAttributes)
                {
                    string[] tagSplit = pattr.attributeType.tag.Split('.');
                    string statName = tagSplit.Length > 2 ? $"{tagSplit[2]} {tagSplit[1]}:" : $"{tagSplit[1]}:";

                    if (name.text == statName)
                    {
                        value.text = pattr.CurrentValue.ToString();
                    }
                }
                foreach (var stat in m_Statistics.stats)
                {
                    string[] tagSplit = stat.statType.tag.Split('.');
                    string statName = tagSplit.Length > 2 ? $"{tagSplit[2]} {tagSplit[1]}:" : $"{tagSplit[1]}:";

                    if (name.text == statName)
                    {
                        value.text = stat.CurrentValue.ToString();
                    }
                }
                foreach (var attr in m_Statistics.attributes)
                {
                    string[] tagSplit = attr.attributeType.tag.Split('.');
                    string statName = tagSplit.Length > 2 ? $"{tagSplit[2]} {tagSplit[1]}:" : $"{tagSplit[1]}:";

                    if (name.text == statName)
                    {
                        value.text = attr.CurrentValue.ToString();
                    }
                }
            }
            #endregion

            #region Equipment
            mainWeapon.text = m_InventoryAndEquipment.GetCurrentRightWeaponObject() != null ? m_InventoryAndEquipment.GetCurrentRightWeaponObject().name : "None";
            offHandWeapon.text = m_InventoryAndEquipment.GetCurrentLeftWeaponObject() != null ?m_InventoryAndEquipment.GetCurrentLeftWeaponObject().name : "None";
            #endregion
        }
    }

    private void StatsAndAttributesInicialization()
    {
        foreach (var pattr in m_Statistics.primaryAttributes)
        {
            TemplateContainer newDebugElement = debugElement.CloneTree();
            Label name = newDebugElement.Q<Label>("name");
            Label value = newDebugElement.Q<Label>("value");

            string[] tagSplit = pattr.attributeType.tag.Split('.');
            string statName = tagSplit.Length > 2 ? $"{tagSplit[2]} {tagSplit[1]}:" : $"{tagSplit[1]}:";

            name.text = statName;
            value.text = pattr.startValue.ToString();

            newStatsAndAttributesWlements.Add(newDebugElement);
            statsContainer.Add(newDebugElement);
        }
        Spacer();
        foreach (var stat in m_Statistics.stats)
        {
            TemplateContainer newDebugElement = debugElement.CloneTree();
            Label name = newDebugElement.Q<Label>("name");
            Label value = newDebugElement.Q<Label>("value");

            string[] tagSplit = stat.statType.tag.Split('.');
            string statName = tagSplit.Length > 2 ? $"{tagSplit[2]} {tagSplit[1]}:" : $"{tagSplit[1]}:";

            name.text = statName;
            value.text = stat.CurrentValue.ToString();

            newStatsAndAttributesWlements.Add(newDebugElement);
            statsContainer.Add(newDebugElement);
        }
        Spacer();
        foreach (var attr in m_Statistics.attributes)
        {
            TemplateContainer newDebugElement = debugElement.CloneTree();
            Label name = newDebugElement.Q<Label>("name");
            Label value = newDebugElement.Q<Label>("value");

            string[] tagSplit = attr.attributeType.tag.Split('.');
            string statName = tagSplit.Length > 2 ? $"{tagSplit[2]} {tagSplit[1]}:" : $"{tagSplit[1]}:";

            name.text = statName;
            value.text = attr.startValue.ToString();

            newStatsAndAttributesWlements.Add(newDebugElement);
            statsContainer.Add(newDebugElement);
        }
    }

    private VisualElement Spacer()
    {
        VisualElement space = new();
        space.style.height = 10; 
        statsContainer.Add(space);
        return space;
    }
}
