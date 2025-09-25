using UltimateFramework.StatisticsSystem;
using UltimateFramework.Editor;
using UnityEngine.InputSystem;

using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StatisticsComponent))]
public class StatisticsComponentEditor : Editor
{
    #region Private Fields
    private VisualElement root;
    private VisualTreeAsset baseVisual;
    private StatisticsComponent m_Target;
    private VisualTreeAsset statsElement;
    private VisualTreeAsset attributeElement;
    private bool showPrimaryAtt, showOtherAtt, showStats;
    #endregion

    #region Mono
    private void OnEnable()
    {
        m_Target = (StatisticsComponent)target;
    }
    public override VisualElement CreateInspectorGUI()
    {
        LoadResources();
        baseVisual.CloneTree(root);

        #region Find Elements
        // Primary Attributes
        var primaryAttListBase = UFEditorUtils.FindElementInRoot<VisualElement>(root, "Primary-Attributes-List");
        var p_AttListOpener = UFEditorUtils.FindElementInRoot<VisualElement>(primaryAttListBase, "list-opener");
        var p_AttArrow = UFEditorUtils.FindElementInRoot<VisualElement>(primaryAttListBase, "arrow");
        var p_AttListCount = UFEditorUtils.FindElementInRoot<Label>(primaryAttListBase, "count");
        var p_AttListBody = UFEditorUtils.FindElementInRoot<VisualElement>(primaryAttListBase, "List-Body");
        var p_AddAttButton = UFEditorUtils.FindElementInRoot<Button>(primaryAttListBase, "add-primary-att");

        // Other Attributes
        var otherAttListBase = UFEditorUtils.FindElementInRoot<VisualElement>(root, "Other-Attributes-List");
        var o_AttListOpener = UFEditorUtils.FindElementInRoot<VisualElement>(otherAttListBase, "list-opener");
        var o_AttArrow = UFEditorUtils.FindElementInRoot<VisualElement>(otherAttListBase, "arrow");
        var o_AttListCount = UFEditorUtils.FindElementInRoot<Label>(otherAttListBase, "count");
        var o_AttListBody = UFEditorUtils.FindElementInRoot<VisualElement>(otherAttListBase, "List-Body");
        var o_AddAttButton = UFEditorUtils.FindElementInRoot<Button>(otherAttListBase, "add-other-att");

        // Stats
        var statsListBase = UFEditorUtils.FindElementInRoot<VisualElement>(root, "Stats-List");
        var statsListOpener = UFEditorUtils.FindElementInRoot<VisualElement>(statsListBase, "list-opener");
        var statsArrow = UFEditorUtils.FindElementInRoot<VisualElement>(statsListBase, "arrow");
        var statsListCount = UFEditorUtils.FindElementInRoot<Label>(statsListBase, "count");
        var statsListBody = UFEditorUtils.FindElementInRoot<VisualElement>(statsListBase, "List-Body");
        var statsAddButton = UFEditorUtils.FindElementInRoot<Button>(statsListBase, "add-stat");

        #endregion

        #region Value Asignament
        // Primary Attributes
        p_AttListCount.text = $"{m_Target.primaryAttributes.Count} elements";
        foreach (var primaryAtt in m_Target.primaryAttributes)
        {
            AddAttribute(primaryAtt, p_AttListBody, p_AttListCount);
        }

        // Other Attributes
        o_AttListCount.text = $"{m_Target.attributes.Count} elements";
        foreach (var otherAtt in m_Target.attributes)
        {
            AddAttribute(otherAtt, o_AttListBody, o_AttListCount);
        }

        // Stats
        statsListCount.text = $"{m_Target.stats.Count} elements";
        foreach (var stat in m_Target.stats)
        {
            AddStat(stat, statsListBody, statsListCount);
        }

        #endregion

        #region Register Callbacks
        // Primary Attributes
        p_AttListOpener.RegisterCallback<ClickEvent>(evt =>
        {
            showPrimaryAtt = !showPrimaryAtt;
            UFEditorUtils.SetElementDisplay(showPrimaryAtt, ref p_AttListBody);
            UFEditorUtils.SetArrowAnim(p_AttListBody, ref p_AttArrow, "arrowmark-toggle-open");
        });

        p_AddAttButton.RegisterCallback<ClickEvent>(evt =>
        {
            var newPrimaryAtt = new Attribute();
            m_Target.primaryAttributes.Add(newPrimaryAtt);
            AddAttribute(newPrimaryAtt, p_AttListBody, p_AttListCount);
            EditorUtility.SetDirty(target);
        });

        // Other Attributes
        o_AttListOpener.RegisterCallback<ClickEvent>(evt =>
        {
            showOtherAtt = !showOtherAtt;
            UFEditorUtils.SetElementDisplay(showOtherAtt, ref o_AttListBody);
            UFEditorUtils.SetArrowAnim(o_AttListBody, ref o_AttArrow, "arrowmark-toggle-open");
        });

        o_AddAttButton.RegisterCallback<ClickEvent>(evt =>
        {
            var newAtt = new Attribute();
            m_Target.attributes.Add(newAtt);
            AddAttribute(newAtt, o_AttListBody, o_AttListCount);
            EditorUtility.SetDirty(target);
        });

        // Stats
        statsListOpener.RegisterCallback<ClickEvent>(evt =>
        {
            showStats = !showStats;
            UFEditorUtils.SetElementDisplay(showStats, ref statsListBody);
            UFEditorUtils.SetArrowAnim(statsListBody, ref statsArrow, "arrowmark-toggle-open");
        });

        statsAddButton.RegisterCallback<ClickEvent>(evt =>
        {
            var newStat = new Statistic();
            m_Target.stats.Add(newStat);
            AddStat(newStat, statsListBody, statsListCount);
            EditorUtility.SetDirty(target);
        });
        #endregion

        return root;
    }
    private void LoadResources()
    {
        root = new VisualElement();
        baseVisual = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Components/StatsComponent_Inspector");
        statsElement = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Components/Templates/StatElement");
        attributeElement = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Components/Templates/AttributeElement");
    }
    #endregion

    #region Attributes
    private void AddAttribute(Attribute attribute, VisualElement container, Label listCount)
    {
        listCount.text = m_Target.primaryAttributes.Contains(attribute) ?
            $"{m_Target.primaryAttributes.Count} elements" : $"{m_Target.attributes.Count} elements";

        var instance = attributeElement.CloneTree();
        container.Add(instance);

        #region Find Elements
        var tagSelector = UFEditorUtils.FindElementInRoot<Button>(instance, "tag-selector-button");
        var tag = UFEditorUtils.FindElementInRoot<Label>(tagSelector, "tag");
        var startValue = UFEditorUtils.FindElementInRoot<FloatField>(instance, "start-value");
        var removeButton = UFEditorUtils.FindElementInRoot<Button>(instance, "remove-button");
        #endregion

        #region Value Asignament
        tag.text = attribute.attributeType.tag;
        startValue.value = attribute.startValue;
        #endregion

        #region Register Callbacks
        tagSelector.RegisterCallback<ClickEvent>(evt =>
        {
            UFEditorUtils.OpenTagSelectorWindow(tag, attribute.attributeType, "tag");
            EditorUtility.SetDirty(target);
        });

        startValue.RegisterValueChangedCallback(evt =>
        {
            attribute.startValue = evt.newValue;
            EditorUtility.SetDirty(target);
        });

        removeButton.RegisterCallback<ClickEvent>(evt =>
        {
            if (m_Target.primaryAttributes.Contains(attribute))
            {
                RemovePrimaryAttribute(attribute, instance, container);
                listCount.text = $"{m_Target.primaryAttributes.Count} elements";
            }
            else
            {
                RemoveOtherAttribute(attribute, instance, container);
                listCount.text = $"{m_Target.attributes.Count} elements";
            }

            EditorUtility.SetDirty(target);
        });
        #endregion
    }
    private void RemovePrimaryAttribute(Attribute attribute, TemplateContainer instance, VisualElement container)
    {
        m_Target.primaryAttributes.Remove(attribute);
        container.Remove(instance);
    }
    private void RemoveOtherAttribute(Attribute attribute, TemplateContainer instance, VisualElement container)
    {
        m_Target.attributes.Remove(attribute);
        container.Remove(instance);
    }
    #endregion

    #region Stats
    private void AddStat(Statistic stat, VisualElement container, Label listCount)
    {
        listCount.text = $"{m_Target.stats.Count} elements";

        var instance = statsElement.CloneTree();
        container.Add(instance);

        #region Find Elements
        var tagSelector = UFEditorUtils.FindElementInRoot<Button>(instance, "tag-selector-button");
        var tag = UFEditorUtils.FindElementInRoot<Label>(tagSelector, "tag");
        var startMaxValue = UFEditorUtils.FindElementInRoot<FloatField>(instance, "start-max-value");
        var regenarionFieldscontainer = UFEditorUtils.FindElementInRoot<VisualElement>(instance, "Regeneration-Fields");
        var regenerationValue = UFEditorUtils.FindElementInRoot<FloatField>(regenarionFieldscontainer, "value-field");
        var regenerationDelay = UFEditorUtils.FindElementInRoot<FloatField>(regenarionFieldscontainer, "delay-field");
        var hasRegeneraionSwitch = UFEditorUtils.FindElementInRoot<Button>(instance, "has-regeneration-switch");
        var startFromZeroSwitch = UFEditorUtils.FindElementInRoot<Button>(instance, "start-from-zero-switch");
        var removeButton = UFEditorUtils.FindElementInRoot<Button>(instance, "remove-button");
        #endregion

        #region Value Asignament
        tag.text = stat.statType.tag;
        startMaxValue.value = stat.startMaxValue;
        UFEditorUtils.SetSwitch(stat.startFromZero, ref startFromZeroSwitch);
        UFEditorUtils.SetSwitch(stat.hasRegeneration, ref hasRegeneraionSwitch);
        UFEditorUtils.SetElementDisplay(stat.hasRegeneration, ref regenarionFieldscontainer);
        regenerationValue.value = stat.regenValue;
        regenerationDelay.value = stat.regenDelay;
        #endregion

        #region Register Callbacks
        tagSelector.RegisterCallback<ClickEvent>(evt =>
        {
            UFEditorUtils.OpenTagSelectorWindow(tag, stat.statType, "tag");
            EditorUtility.SetDirty(target);
        });

        startMaxValue.RegisterValueChangedCallback(evt =>
        {
            stat.startMaxValue = evt.newValue;
            EditorUtility.SetDirty(target);
        });

        regenerationValue.RegisterValueChangedCallback(evt =>
        {
            stat.regenValue = evt.newValue;
            EditorUtility.SetDirty(target);
        });

        regenerationDelay.RegisterValueChangedCallback(evt =>
        {
            stat.regenDelay = evt.newValue;
            EditorUtility.SetDirty(target);
        });

        hasRegeneraionSwitch.RegisterCallback<ClickEvent>(evt =>
        {
            stat.hasRegeneration = !stat.hasRegeneration;
            UFEditorUtils.SetSwitch(stat.hasRegeneration, ref hasRegeneraionSwitch);
            UFEditorUtils.SetElementDisplay(stat.hasRegeneration, ref regenarionFieldscontainer);
            EditorUtility.SetDirty(target);
        });

        startFromZeroSwitch.RegisterCallback<ClickEvent>(evt =>
        {
            stat.startFromZero = !stat.startFromZero;
            UFEditorUtils.SetSwitch(stat.startFromZero, ref startFromZeroSwitch);
            EditorUtility.SetDirty(target);
        });

        removeButton.RegisterCallback<ClickEvent>(evt =>
        {
            RemoveStat(stat, instance, container);
            EditorUtility.SetDirty(target);
        });
        #endregion
    }
    private void RemoveStat(Statistic stat, TemplateContainer instance, VisualElement container)
    {
        m_Target.stats.Remove(stat);
        container.Remove(instance);
    }
    #endregion
}
