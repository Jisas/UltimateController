using UltimateFramework.ItemSystem;
using System.Collections.Generic;
using UltimateFramework.Utils;
using UltimateFramework.Tools;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System;

public class ItemManagerWindow : EditorWindow
{
    #region Fields
    private VisualTreeAsset m_UXML;
    private VisualTreeAsset m_ItemElement;
    private VisualTreeAsset m_StatsElement;
    private VisualTreeAsset m_ScaledElement;
    private VisualTreeAsset m_UpgradeElement;
    private VisualTreeAsset m_SubItemTemplate;
    private VisualTreeAsset m_ArmorRightTemplate;
    private VisualTreeAsset m_AmuletRightTemplate;
    private VisualTreeAsset m_ArmaturePartElement;
    private VisualTreeAsset m_RequirementsElement;
    private VisualTreeAsset m_WeaponRightTemplate;
    private VisualTreeAsset m_StatsModifierElement;
    private VisualTreeAsset m_MaterialRightTemplate;
    private VisualTreeAsset m_QuestItemRightTemplate;
    private VisualTreeAsset m_ArmatureSetItemElement;
    private VisualTreeAsset m_ConsumableRightTemplate;
    private VisualTreeAsset m_AttributeModifierElement;
    private VisualTreeAsset m_ArmatureSetRightTemplate;
    private TemplateContainer base_Root;
    private ToolbarSearchField searchField;
    private VisualElement rightElementContainer;
    private VisualElement currentRightPanelElement;
    private VisualElement armaturesContainer;
    private Label armaturesAmountLabel;
    private Label weaponAmountLabel;
    private Label materialAmountLabel;
    private Label consumableAmountLabel;
    private Label amuletItemAmountLabel;
    private Label questItemAmountLabel;
    private Button saveButton;
    private Color startColor = new (0, 0, 0, 0);
    private Color endColor = new (1, 1, 0.4823529f, 1);
    private readonly List<TemplateContainer> m_ItemAssets = new();
    private readonly Dictionary<int, (Action addStat, Action addScaling, Action addUpgrade, Action addRequirement, Action addAttributeModifier, Action addStatModifier)> itemActions = new();
    private readonly Dictionary<int, (Action addHelment, Action addArmor, Action addGloves, Action addPants, Action addBoots)> armatureSetsActions = new();
    private ItemDatabase itemDB;
    private EditorWindow currentTagSelectorWindow;
    #endregion

    [MenuItem("Ultimate Framework/Windows/Items Database")]
    public static void ShowMyEditor()
    {
        EditorWindow wnd = GetWindow<ItemManagerWindow>();
        Texture2D icon = Resources.Load<Texture2D>("Img/ItemDatabase_Icon");
        wnd.titleContent = new GUIContent("Items Database", icon);       
    }
    private void LoadResources()
    {
        m_UXML = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/ItemManagerWindow");
        itemDB = SettingsMasterData.Instance.itemDB;

        m_ItemElement = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/Templates/ItemElement");
        m_StatsElement = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/Templates/StatElement");
        m_ScaledElement = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/Templates/ScaledElement");
        m_UpgradeElement = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/Templates/UpgradeElement");
        m_SubItemTemplate = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/Templates/SubItemElement");
        m_RequirementsElement = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/Templates/RequirementElement");
        m_ArmaturePartElement = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/Templates/ArmaturePartElement");
        m_StatsModifierElement = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/Templates/StatModifierElement");
        m_ArmorRightTemplate = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/Templates/ArmorRightElementTemplate");
        m_ArmatureSetItemElement = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/Templates/ArmatureSetItemElement");
        m_AmuletRightTemplate = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/Templates/AmuletRightElementTemplate");
        m_WeaponRightTemplate = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/Templates/WeaponRightElementTemplate");
        m_AttributeModifierElement = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/Templates/AttributeModifierElement");
        m_MaterialRightTemplate = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/Templates/MaterialRightElementTemplate");
        m_QuestItemRightTemplate = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/Templates/QuestItemRightElementTemplate");
        m_ConsumableRightTemplate = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/Templates/ConsumableRightElementTemplate");
        m_ArmatureSetRightTemplate = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/Templates/ArmatureSetRightElementTemplate");
    }
    private void CreateGUI()
    {
        LoadResources();

        base_Root = m_UXML.CloneTree();
        rootVisualElement.Add(base_Root);

        var armaturesButton = base_Root.Q<Button>("CreateArmorButton");
        var armaturesToggle = base_Root.Q<Button>("armors-toggle");
        var armaturesFoldoutArrow = base_Root.Q<VisualElement>("armors-checkmark");
        armaturesContainer = base_Root.Q<VisualElement>("armor-content");
        armaturesAmountLabel = base_Root.Q<Label>("armors-amount");

        var weaponButton = base_Root.Q<Button>("CreateWeaponButton");
        var weaponToggle = base_Root.Q<Button>("items-toggle");
        var weaponContainer = base_Root.Q<VisualElement>("weapon-content");
        var weaponFoldoutArrow = base_Root.Q<VisualElement>("weapons-checkmark");
        weaponAmountLabel = base_Root.Q<Label>("weapons-amount");

        var consumableButton = base_Root.Q<Button>("CreateConsumableButton");
        var consumableToggle = base_Root.Q<Button>("consumables-toggle");
        var consumableContainer = base_Root.Q<VisualElement>("consumables-content");
        var consumableFoldoutArrow = base_Root.Q<VisualElement>("consumables-checkmark");
        consumableAmountLabel = base_Root.Q<Label>("consumables-amount");

        var amuletsButton = base_Root.Q<Button>("CreateAmuletButton");
        var amuletsToggle = base_Root.Q<Button>("amulets-toggle");
        var amuletsContainer = base_Root.Q<VisualElement>("amulets-content");
        var amuletsFoldoutArrow = base_Root.Q<VisualElement>("amulets-checkmark");
        amuletItemAmountLabel = base_Root.Q<Label>("amulets-amount");

        var questItemButton = base_Root.Q<Button>("CreateQuestItemButton");
        var questItemToggle = base_Root.Q<Button>("quest-items-toggle");
        var questItemContainer = base_Root.Q<VisualElement>("quest-items-content");
        var questItemFoldoutArrow = base_Root.Q<VisualElement>("quest-items-checkmark");
        questItemAmountLabel = base_Root.Q<Label>("quest-items-amount");

        var materialButton = base_Root.Q<Button>("CreateMaterialButton");
        var materialsToggle = base_Root.Q<Button>("materials-toggle");
        var materialContainer = base_Root.Q<VisualElement>("materials-content");
        var materialFoldoutArrow = base_Root.Q<VisualElement>("materials-checkmark");
        materialAmountLabel = base_Root.Q<Label>("materials-amount");

        searchField = base_Root.Q<ToolbarSearchField>("ToolbarSearchField");
        saveButton = base_Root.Q<Button>("save-database-button");
        rightElementContainer = base_Root.Q<ScrollView>("right-element-scrollview");

        searchField.RegisterValueChangedCallback((evt) => SearchItem(evt));
        saveButton.clickable.clicked += () => SaveData();
        saveButton.style.backgroundColor = startColor;
        saveButton.style.color = Color.white;

        armaturesButton.clickable.clicked += () => CreateArmatureSet(armaturesContainer, armaturesFoldoutArrow);
        weaponButton.clickable.clicked += () => CreateItem(weaponContainer, weaponFoldoutArrow, ItemType.Weapon);
        consumableButton.clickable.clicked += () => CreateItem(consumableContainer, consumableFoldoutArrow, ItemType.Consumable);
        amuletsButton.clickable.clicked += () => CreateItem(amuletsContainer, amuletsFoldoutArrow, ItemType.Amulet);
        questItemButton.clickable.clicked += () => CreateItem(questItemContainer, questItemFoldoutArrow, ItemType.QuestItem);
        materialButton.clickable.clicked += () => CreateItem(materialContainer, materialFoldoutArrow, ItemType.Material);

        armaturesToggle.clickable.clicked += () => SetElementDisplay(armaturesContainer, armaturesFoldoutArrow);
        weaponToggle.clickable.clicked += () => SetElementDisplay(weaponContainer, weaponFoldoutArrow);
        consumableToggle.clickable.clicked += () => SetElementDisplay(consumableContainer, consumableFoldoutArrow);
        amuletsToggle.clickable.clicked += () => SetElementDisplay(amuletsContainer, amuletsFoldoutArrow);
        questItemToggle.clickable.clicked += () => SetElementDisplay(questItemContainer, questItemFoldoutArrow);
        materialsToggle.clickable.clicked += () => SetElementDisplay(materialContainer, materialFoldoutArrow);

        armaturesAmountLabel.text = $"{itemDB.armatureSets.Count} sets";
        weaponAmountLabel.text = $"{itemDB.GetCountByTypeOnItemList(ItemType.Weapon)} items";
        consumableAmountLabel.text = $"{itemDB.GetCountByTypeOnItemList(ItemType.Consumable)} items";
        amuletItemAmountLabel.text = $"{itemDB.GetCountByTypeOnItemList(ItemType.Amulet)} items";
        questItemAmountLabel.text = $"{itemDB.GetCountByTypeOnItemList(ItemType.QuestItem)} items";
        materialAmountLabel.text = $"{itemDB.GetCountByTypeOnItemList(ItemType.Material)} items";

        if (itemDB.items.Count == 0)
        {           
            weaponContainer.style.display = DisplayStyle.None;
            materialContainer.style.display = DisplayStyle.None;
            consumableContainer.style.display = DisplayStyle.None;

            // Actualizar el nombre en el UI
            var currentItemNameField = base_Root.Q<Label>("current-item-name");
            currentItemNameField.text = "";

        }
        else
        {
            for (int i = 0; i < itemDB.items.Count; i++)
            {
                var instance = m_ItemElement.CloneTree();
                m_ItemAssets.Add(instance);

                var sefItemButton = instance.Q<Button>("foldout-element");
                var removeItemButton = instance.Q<Button>("remove-item-button");
                var itemElementIcon = instance.Q<VisualElement>("icon");
                var itemName = instance.Q<Label>("item-name");

                Action addStatAction = null;
                Action addScalingAction = null;
                Action addUpgradeAction = null;
                Action addRequirementAction = null;
                Action addAtributeModifierAction = null;
                Action addStatModifier = null;

                itemActions[i] = (addStatAction, addScalingAction, addUpgradeAction, addRequirementAction, addAtributeModifierAction, addStatModifier);

                sefItemButton.name = i.ToString();
                removeItemButton.name = i.ToString();

                if (itemDB.items[i].icon != null)
                    itemElementIcon.style.backgroundImage = new StyleBackground(itemDB.items[i].icon);

                if (!String.IsNullOrEmpty(itemDB.items[i].name))
                    itemName.text = itemDB.items[i].name;

                TemplateContainer itemRightPanelElement = null;

                switch (itemDB.items[i].type)
                {
                    case ItemType.Weapon:
                        weaponContainer.Add(instance);
                        weaponContainer.style.display = DisplayStyle.None;
                        weaponContainer.style.maxHeight = Math.Min(30 * itemDB.GetCountByTypeOnItemList(ItemType.Weapon), float.PositiveInfinity);
                        itemRightPanelElement = m_WeaponRightTemplate.CloneTree();
                        removeItemButton.clickable.clicked += () => RemoveItem(removeItemButton.name, weaponContainer, weaponFoldoutArrow, instance, ItemType.Weapon);
                        break;

                    case ItemType.Consumable:
                        consumableContainer.Add(instance);
                        consumableContainer.style.display = DisplayStyle.None;
                        consumableContainer.style.maxHeight = Math.Min(30 * itemDB.GetCountByTypeOnItemList(ItemType.Consumable), float.PositiveInfinity);
                        itemRightPanelElement = m_ConsumableRightTemplate.CloneTree();
                        removeItemButton.clickable.clicked += () => RemoveItem(removeItemButton.name, consumableContainer, consumableFoldoutArrow, instance, ItemType.Consumable);
                        break;

                    case ItemType.QuestItem:
                        questItemContainer.Add(instance);
                        questItemContainer.style.display = DisplayStyle.None;
                        questItemContainer.style.maxHeight = Math.Min(30 * itemDB.GetCountByTypeOnItemList(ItemType.QuestItem), float.PositiveInfinity);
                        itemRightPanelElement = m_QuestItemRightTemplate.CloneTree();
                        removeItemButton.clickable.clicked += () => RemoveItem(removeItemButton.name, questItemContainer, questItemFoldoutArrow, instance, ItemType.QuestItem);
                        break;

                    case ItemType.Material:
                        materialContainer.Add(instance);
                        materialContainer.style.display = DisplayStyle.None;
                        materialContainer.style.maxHeight = Math.Min(30 * itemDB.GetCountByTypeOnItemList(ItemType.Material), float.PositiveInfinity);
                        itemRightPanelElement = m_MaterialRightTemplate.CloneTree();
                        removeItemButton.clickable.clicked += () => RemoveItem(removeItemButton.name, materialContainer, materialFoldoutArrow, instance, ItemType.Material);
                        break;

                    case ItemType.Amulet:
                        amuletsContainer.Add(instance);
                        amuletsContainer.style.display = DisplayStyle.None;
                        amuletsContainer.style.maxHeight = Math.Min(30 * itemDB.GetCountByTypeOnItemList(ItemType.Amulet), float.PositiveInfinity);
                        itemRightPanelElement = m_AmuletRightTemplate.CloneTree();
                        amuletsButton.clickable.clicked += () => RemoveItem(removeItemButton.name, amuletsContainer, amuletsFoldoutArrow, instance, ItemType.Amulet);
                        break;
                }

                sefItemButton.clickable.clicked += () => ItemButtonClicked(sefItemButton.name, itemRightPanelElement, instance);
            }
        }

        if (itemDB.armatureSets.Count == 0)
        {
            armaturesContainer.style.display = DisplayStyle.None;

            // Actualizar el nombre en el UI
            var currentItemNameField = base_Root.Q<Label>("current-item-name");
            currentItemNameField.text = "";
        }
        else
        {
            int total = 0;

            for (int i = 0; i < itemDB.armatureSets.Count; i++)
            {
                var instance = m_ArmatureSetItemElement.CloneTree();
                m_ItemAssets.Add(instance);

                var sefItemButton = instance.Q<Button>("foldout-element");
                var removeItemButton = instance.Q<Button>("remove-item-button");
                var itemName = instance.Q<Label>("item-name");
                var armatureSetArrowMark = instance.Q<VisualElement>("arrow-container");
                var armaturePartsContainer = instance.Q<VisualElement>("armature-parts-content-container");

                Action addHelmentAction = null;
                Action addArmorAction = null;
                Action addGlovesAction = null;
                Action addPantsAction = null;
                Action addBootsAction = null;

                armatureSetsActions[i] = (addHelmentAction, addArmorAction, addGlovesAction, addPantsAction, addBootsAction);

                sefItemButton.name = i.ToString();
                removeItemButton.name = i.ToString();
                itemName.text = !String.IsNullOrEmpty(itemDB.armatureSets[i].name) ? itemDB.armatureSets[i].name : "New Set";

                armaturesContainer.Add(instance);
                armaturesContainer.style.display = DisplayStyle.None;

                TemplateContainer itemRightPanelElement = m_ArmatureSetRightTemplate.CloneTree();

                removeItemButton.clickable.clicked += () => RemoveArmatureSet(removeItemButton.name, armaturesContainer, armaturesFoldoutArrow, instance);
                sefItemButton.clickable.clicked += () => 
                {
                    ArmatureSetButtonClicked(sefItemButton.name, itemRightPanelElement, instance);
                    SetArmatureSetDisplay(armaturePartsContainer);
                    SetArrowAnim(armaturePartsContainer, armatureSetArrowMark, "arrowmark-toggle-open");
                };

                if (itemDB.armatureSets[i].armaturePartsInfo.Count > 0)
                {
                    for (int j = 0; j < itemDB.armatureSets[i].armaturePartsInfo.Count; j++)
                    {
                        total++;
                        TemplateContainer subItemRightPanelElement = m_ArmorRightTemplate.CloneTree();
                        var subInstance = m_SubItemTemplate.CloneTree();
                        var armaturePartInfoIndex = itemDB.armatureSets[i].armaturePartsInfo[j].index;
                        var armaturePartTemplate = itemDB.items[armaturePartInfoIndex].SelfUIInstance = subInstance;

                        var subSelfItemButton = subInstance.Q<Button>("foldout-element");
                        var leftContainer = instance.Q<VisualElement>("armature-parts-content-container");
                        leftContainer.style.maxHeight = Math.Min(30 * itemDB.armatureSets[i].armaturePartsInfo.Count, float.PositiveInfinity);

                        Action addStatAction = null;
                        Action addScalingAction = null;
                        Action addUpgradeAction = null;
                        Action addRequirementAction = null;
                        Action addatributeModifierAction = null;
                        Action addStatModifier = null;

                        // Asignar las acciones para los botones del nuevo item
                        itemActions[armaturePartInfoIndex] = (addStatAction, addScalingAction, addUpgradeAction, addRequirementAction, addatributeModifierAction, addStatModifier);

                        var currentArmature = itemDB.armatureSets[i].armaturePartsInfo[j];

                        // Asignacion de valores en la UI
                        var subItemName = subInstance.Q<Label>("item-name");
                        subItemName.text = !String.IsNullOrEmpty(currentArmature.partName) ? currentArmature.partName : $"{currentArmature.type} of {itemName}";

                        // Resto de configuraciones
                        leftContainer.Add(subInstance);
                        subSelfItemButton.name = currentArmature.index.ToString();
                        subSelfItemButton.clickable.clicked += () => ItemButtonClicked(subSelfItemButton.name, subItemRightPanelElement, subInstance);
                    }                    
                    armaturesContainer.style.maxHeight = Math.Min(30 * (itemDB.armatureSets.Count + total), float.PositiveInfinity);             
                }
            }
        }
    }

    // Search Bar
    private void SearchItem(ChangeEvent<string> evt)
    {
        string searchQuery = evt.newValue.ToLower();
        bool hasSearchQuery = !string.IsNullOrEmpty(searchQuery);

        // Establecer el estilo de visualización de los elementos de una sola vez
        SetElementDisplay(base_Root, "foldout-armors", "armors-checkmark", hasSearchQuery ? DisplayStyle.None : DisplayStyle.Flex);
        SetElementDisplay(base_Root, "foldout-weapons", "weapons-checkmark", hasSearchQuery ? DisplayStyle.None : DisplayStyle.Flex);
        SetElementDisplay(base_Root, "foldout-materials", "materials-checkmark", hasSearchQuery ? DisplayStyle.None : DisplayStyle.Flex);
        SetElementDisplay(base_Root, "foldout-consumables", "consumables-checkmark", hasSearchQuery ? DisplayStyle.None : DisplayStyle.Flex);
        SetElementDisplay(base_Root, "foldout-quest-items", "quest-items-checkmark", hasSearchQuery ? DisplayStyle.None : DisplayStyle.Flex);
        SetElementDisplay(base_Root, "armor-content", "armors-checkmark", hasSearchQuery ? DisplayStyle.Flex : DisplayStyle.None);
        SetElementDisplay(base_Root, "weapon-content", "weapons-checkmark", hasSearchQuery ? DisplayStyle.Flex : DisplayStyle.None);
        SetElementDisplay(base_Root, "materials-content", "materials-checkmark", hasSearchQuery ? DisplayStyle.Flex : DisplayStyle.None);
        SetElementDisplay(base_Root, "consumables-content", "consumables-checkmark", hasSearchQuery ? DisplayStyle.Flex : DisplayStyle.None);
        SetElementDisplay(base_Root, "quest-items-content", "quest-items-checkmark", hasSearchQuery ? DisplayStyle.Flex : DisplayStyle.None);

        // Búsqueda y actualización de la visualización de los elementos
        foreach (var element in m_ItemAssets)
        {
            var itemName = element.Q<Label>("item-name");
            if(element != null)
                element.style.display = itemName.text.ToLower().Contains(searchQuery) ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    // Armature Sets
    void CreateArmatureSet(VisualElement container, VisualElement arrow)
    {
        var instance = m_ArmatureSetItemElement.CloneTree();
        m_ItemAssets.Add(instance);
        container.Add(instance);

        // Crear un nuevo item y agregarlo a la base de datos
        var newArmatureSet = new ArmatureSet();
        itemDB.armatureSets.Add(newArmatureSet);

        Action addHelment = null;
        Action addArmor = null;
        Action addGloves = null;
        Action addPants = null;
        Action addBoots = null;

        TemplateContainer itemRightPanelElement = null;
        itemRightPanelElement = m_ArmatureSetRightTemplate.CloneTree();
        armaturesAmountLabel.text = $"{itemDB.armatureSets.Count} sets";

        // Asignar el índice del nuevo item como su id
        newArmatureSet.index = itemDB.armatureSets.IndexOf(newArmatureSet);

        // Capturar el valor actual del índice en una variable local
        int currentArmatureSetIndex = newArmatureSet.index;

        // Asignar las acciones para los botones del nuevo item
        armatureSetsActions[currentArmatureSetIndex] = (addHelment, addArmor, addGloves, addPants, addBoots);

        // Obtener el botón del elemento clonado
        var sefItemButton = instance.Q<Button>("foldout-element");
        var removeItemButton = instance.Q<Button>("remove-item-button");

        // Obtener elementos adicionales
        var armatureSetArrowMark = instance.Q<VisualElement>("arrow-container");
        var armaturePartsContainer = instance.Q<VisualElement>("armature-parts-content-container");
        armaturePartsContainer.style.display = DisplayStyle.None;
        armaturePartsContainer.style.maxHeight = Math.Min(30 * newArmatureSet.armaturePartsInfo.Count, float.PositiveInfinity);

        // Asignar el índice del nuevo item como identificador único del botón
        sefItemButton.name = currentArmatureSetIndex.ToString();
        removeItemButton.name = currentArmatureSetIndex.ToString();

        // Agregar un listener al botón para manejar el evento de click
        sefItemButton.clickable.clicked += () =>
        {
            ArmatureSetButtonClicked(currentArmatureSetIndex.ToString(), itemRightPanelElement, instance);
            SetArmatureSetDisplay(armaturePartsContainer);
            SetArrowAnim(armaturePartsContainer, armatureSetArrowMark, "arrowmark-toggle-open");
        };

        removeItemButton.clickable.clicked += () => RemoveArmatureSet(currentArmatureSetIndex.ToString(), container, arrow, instance);
        removeItemButton.RegisterValueChangedCallback((evt) =>
        {
            this.hasUnsavedChanges = true;
        });

        // Actualiza el display y MaxHeight de itemContent basado en la lista
        if (itemDB.armatureSets.Count == 0)
        {
            container.style.display = DisplayStyle.None;
        }
        else
        {
            container.style.display = DisplayStyle.Flex;
            SetArrowAnim(container, arrow, "chechmark-toggle-open");

            int total = 0;
            for (int i = 0; i < itemDB.armatureSets.Count; i++)
            {
                for (int j = 0; j < itemDB.armatureSets[i].armaturePartsInfo.Count; j++)
                {
                    total++;
                }
            }
            container.style.maxHeight = Math.Min(30 * (itemDB.armatureSets.Count + total), float.PositiveInfinity);
        }

        // Actualiza la ventana
        this.Repaint();
        this.hasUnsavedChanges = true;
    }
    private void RemoveArmatureSet(string buttonName, VisualElement container, VisualElement arrow, TemplateContainer instance)
    {
        // Convertir el nombre del botón a un identificador único
        int id = int.Parse(buttonName);

        // Obtener el item correspondiente al identificador
        var armatureSet = itemDB.armatureSets.Find(i => i.index == id);

        // Remover todas las partes del set de la lista de items
        for (int i = 0; i < armatureSet.armaturePartsInfo.Count; i++)
        {
            var currentPartAsItem = itemDB.FindItem(armatureSet.armaturePartsInfo[i].index);
            itemDB.items.Remove(currentPartAsItem);
        }

        // Remover el item de la lista de sets
        itemDB.armatureSets.Remove(armatureSet);

        // Remover elemento visual de la ventana
        m_ItemAssets.Remove(instance);
        container.Remove(instance);

        // Actualiza el display y MaxHeight de itemContent basado en la lista
        if (itemDB.armatureSets.Count == 0)
        {
            container.style.display = DisplayStyle.None;
            SetArrowAnim(container, arrow, "chechmark-toggle-open");

            // removemos cualquier instancia de rightElementContainer
            if (currentRightPanelElement != null)
            {
                rightElementContainer.Remove(currentRightPanelElement);
                currentRightPanelElement = null;
            }
        }
        else
        {
            int total = 0;
            for (int i = 0; i < itemDB.armatureSets.Count; i++)
            {
                for (int j = 0; j < itemDB.armatureSets[i].armaturePartsInfo.Count; j++)
                {
                    total++;
                }
            }
            container.style.display = DisplayStyle.Flex;
            container.style.maxHeight = Math.Min(30 * (itemDB.armatureSets.Count + total), float.PositiveInfinity);

            // removemos cualquier instancia de rightElementContainer
            if (currentRightPanelElement != null)
            {
                rightElementContainer.Remove(currentRightPanelElement);
                currentRightPanelElement = null;
            }
        }

        armaturesAmountLabel.text = $"{itemDB.GetCountByTypeOnItemList(ItemType.Armor)} items";
        weaponAmountLabel.text = $"{itemDB.GetCountByTypeOnItemList(ItemType.Weapon)} items";
        materialAmountLabel.text = $"{itemDB.GetCountByTypeOnItemList(ItemType.Material)} items";
        consumableAmountLabel.text = $"{itemDB.GetCountByTypeOnItemList(ItemType.Consumable)} items";
        questItemAmountLabel.text = $"{itemDB.GetCountByTypeOnItemList(ItemType.QuestItem)} items";

        this.hasUnsavedChanges = true;

        // Aquí puedes mostrar la información del item
        Debug.Log($"Se ha removido el item: {armatureSet.name}, de id: {id}");
    }

    // Display armature sets info
    void ArmatureSetButtonClicked(string buttonName, TemplateContainer itemRightPanelElement, TemplateContainer elementTemplateInstance)
    {
        // Convertir el nombre del botón a un índice
        int index = int.Parse(buttonName);

        // Obtener el set correspondiente al índice
        var armatureSet = itemDB.FindArmatureSet(index);

        var (oldAddHelment, oldAddArmor, oldAddGloves, oldAddPants, oldAddBoots) = armatureSetsActions[index];

        // Almacenar los botones generales del panel
        var addHelmentButton = itemRightPanelElement.Q<Button>("add-helment-button");
        var addArmorButton = itemRightPanelElement.Q<Button>("add-armor-button");
        var addGlovesButton = itemRightPanelElement.Q<Button>("add-gloves-button");
        var addPantsButton = itemRightPanelElement.Q<Button>("add-pants-button");
        var addBootsButton = itemRightPanelElement.Q<Button>("add-boots-button");

        // Desuscribir los eventos antiguos
        addHelmentButton.clickable.clicked -= oldAddHelment;
        addArmorButton.clickable.clicked -= oldAddArmor;
        addGlovesButton.clickable.clicked -= oldAddGloves;
        addPantsButton.clickable.clicked -= oldAddPants;
        addBootsButton.clickable.clicked -= oldAddBoots;

        // Crear nuevas acciones para este set específico
        void newAddHelment() => AddPartToArmatureSet(armatureSet, ItemType.Helment, itemRightPanelElement, elementTemplateInstance);
        void newAddArmor() => AddPartToArmatureSet(armatureSet, ItemType.Armor, itemRightPanelElement, elementTemplateInstance);
        void newAddGloves() => AddPartToArmatureSet(armatureSet, ItemType.Gloves, itemRightPanelElement, elementTemplateInstance);
        void newAddPants() => AddPartToArmatureSet(armatureSet, ItemType.Pants, itemRightPanelElement, elementTemplateInstance);
        void newAddBoots() => AddPartToArmatureSet(armatureSet, ItemType.Boots, itemRightPanelElement, elementTemplateInstance);

        // Actualizar las acciones en el diccionario
        armatureSetsActions[index] = (newAddHelment, newAddArmor, newAddGloves, newAddPants, newAddBoots);

        // Suscribir los nuevos eventos
        addHelmentButton.clickable.clicked += newAddHelment;
        addArmorButton.clickable.clicked += newAddArmor;
        addGlovesButton.clickable.clicked += newAddGloves;
        addPantsButton.clickable.clicked += newAddPants;
        addBootsButton.clickable.clicked += newAddBoots;

        // Actualizar el nombre en el UI
        var currentItemNameField = base_Root.Q<Label>("current-item-name");
        currentItemNameField.text = armatureSet.name;

        // Mostrar la informacion del set de armadura en el panel izquierdo
        var partsContainer = elementTemplateInstance.Q<VisualElement>("armature-parts-content-container");
        var armaturesFoldoutArrow = elementTemplateInstance.Q<VisualElement>("arrow-container");

        // Mostrar la informacion del item en el panel derecho
        DisplayArmorSetInfo(armatureSet, itemRightPanelElement, elementTemplateInstance);       

        // Debug de la información del set
        Debug.Log($"Se ha hecho clic en el botón del item: {armatureSet.name}, de indice: {index}, que contiene {armatureSet.armaturePartsInfo.Count} partes");
    }
    void DisplayArmorSetInfo(ArmatureSet armatureSet, TemplateContainer itemRightPanelElement, TemplateContainer itemTemplateInstance)
    {
        #region Base
        // Si hay un elemento en el panel derecho, lo removemos
        if (currentRightPanelElement != null)
        {
            rightElementContainer.Remove(currentRightPanelElement);
        }

        // Agregamos el nuevo elemento al panel derecho
        rightElementContainer.Add(itemRightPanelElement);

        // Actualizamos la referencia al elemento actual en el panel derecho
        currentRightPanelElement = itemRightPanelElement;
        #endregion

        #region Sets General Settings
        // Creamos variables para los campos que necesitamos
        var armatureSetNameField = itemRightPanelElement.Q<TextField>("name-field");
        var armatureSetName = itemTemplateInstance.Q<Label>("item-name");

        // Cargamos la información del ítem en los campos
        if (!String.IsNullOrEmpty(armatureSet.name))
        {
            armatureSetNameField.value = armatureSet.name;
        }
        else armatureSetNameField.value = "New Set";

        // Agregamos manejadores de eventos a los campos para actualizar la información del ítem cuando cambien
        string setName = null;
        armatureSetNameField.RegisterValueChangedCallback((evt) =>
        {
            armatureSet.name = evt.newValue;
            armatureSetName.text = armatureSet.name;
            setName = armatureSet.name;
            this.hasUnsavedChanges = true;
        });
        #endregion

        #region Set Parts
        if (armatureSet.armaturePartsInfo.Count > 0)
        {
            // Accedemos al contenedor
            var container = itemRightPanelElement.Q<VisualElement>("armature-parts-content");

            // Limpiamos el contenedor antes de agregar los elementos
            container.Clear();

            int total = 0;
            for (int i = 0; i < itemDB.armatureSets.Count; i++)
            {
                for (int j  = 0; j < itemDB.armatureSets[i].armaturePartsInfo.Count; j++)
                {
                    total++;
                }
            }

            // Recorremos la lista de partes en el set dado
            foreach (var part in armatureSet.armaturePartsInfo)
            {
                // Clona la plantilla del elemento Armature part
                var instance = m_ArmaturePartElement.CloneTree();
                m_ItemAssets.Add(instance);

                // Busca los elementos de la plantilla
                var setNameLabel = instance.Q<Label>("set-name");
                var partPrefix = instance.Q<Label>("prefix");
                var partType = instance.Q<Label>("part-type");

#pragma warning disable CS8509 // La expresión switch no controla todos los valores posibles de su tipo de entrada (no es exhaustiva).
                string defaultPrefix = part.type switch
                {
                    ItemType.Helment => "Helment of",
                    ItemType.Armor => "Armor of",
                    ItemType.Gloves => "Gloves of",
                    ItemType.Pants => "Pants of",
                    ItemType.Boots => "Boots of",
                };
#pragma warning restore CS8509 // La expresión switch no controla todos los valores posibles de su tipo de entrada (no es exhaustiva).

                // Asigna valores a los elementos de la plantilla
                partPrefix.text = defaultPrefix;
                setNameLabel.text = !String.IsNullOrEmpty(armatureSet.name) ? armatureSet.name : "New Set";
                partType.text = part.type.ToString();

                var leftContainer = itemTemplateInstance.Q<VisualElement>("armature-parts-content-container");
                leftContainer.style.maxHeight = Math.Min(30 * armatureSet.armaturePartsInfo.Count, float.PositiveInfinity);

                var currentPartAsItem = itemDB.items[part.index];
                var removeArmaturePartButton = instance.Q<Button>("remove-armature-part-button");
                removeArmaturePartButton.name = part.index.ToString();
                removeArmaturePartButton.clickable.clicked += () => RemovePartOfArmatureSet(
                    removeArmaturePartButton.name, 
                    armatureSet, itemRightPanelElement, 
                    instance, 
                    leftContainer, 
                    currentPartAsItem.SelfUIInstance);

                // Agregamos la plantilla al contenedor
                container.Add(instance);
            }
            armaturesContainer.style.maxHeight = Math.Min(30 * (itemDB.armatureSets.Count + total), float.PositiveInfinity);
        }
        #endregion
    }

    // Armature Parts
    private void AddPartToArmatureSet(ArmatureSet armatureSet, ItemType armaturePartType, VisualElement rightPanelElement, TemplateContainer elementTemplateInstance)
    {
        var instance = m_ArmaturePartElement.CloneTree();

        Item newArmaturePart = new(armaturePartType);
        itemDB.items.Add(newArmaturePart);
        itemDB.FindItem(newArmaturePart).InitializeLists();
        newArmaturePart.index = itemDB.items.IndexOf(newArmaturePart);

        // Capturar el valor actual del índice en una variable local
        int currentPartIndex = newArmaturePart.index;
        itemDB.FindItem(currentPartIndex).InitializeLists();

        Action addStatAction = null;
        Action addScalingAction = null;
        Action addUpgradeAction = null;
        Action addRequirementAction = null;
        Action addatributeModifierAction = null;
        Action addStatModifier = null;

        // Asignar las acciones para los botones del nuevo item
        itemActions[currentPartIndex] = (addStatAction, addScalingAction, addUpgradeAction, addRequirementAction, addatributeModifierAction, addStatModifier);

        var rightContainer = rightPanelElement.Q<VisualElement>("armature-parts-content");
        rightContainer.Add(instance);

        var partPrefix = instance.Q<Label>("prefix");
        var setName = instance.Q<Label>("set-name");
        var partType = instance.Q<Label>("part-type");

#pragma warning disable CS8509 // La expresión switch no controla todos los valores posibles de su tipo de entrada (no es exhaustiva).
        string defaultPrefix = armaturePartType switch
        {
            ItemType.Helment => "Helment of",
            ItemType.Armor => "Armor of",
            ItemType.Gloves => "Gloves of",
            ItemType.Pants => "Pants of",
            ItemType.Boots => "Boots of",
        };
#pragma warning restore CS8509 // La expresión switch no controla todos los valores posibles de su tipo de entrada (no es exhaustiva).

        partPrefix.text = defaultPrefix;
        setName.text = !String.IsNullOrEmpty(armatureSet.name) ? armatureSet.name : "Set name";
        partType.text = newArmaturePart.type.ToString();

        newArmaturePart.name = $"{defaultPrefix} {armatureSet.name}";

        // Add new armature part info
        armatureSet.armaturePartsInfo.Add(new ArmatureSet.ArmaturePartInfo(
            newArmaturePart.name,
            newArmaturePart.index,
            newArmaturePart.type));

        // ADD ITEM ELEMENT (ARMATURE PART) FOR LEFT SIDE OF TEMPLATE
        var subInstance = m_SubItemTemplate.CloneTree();
        newArmaturePart.SelfUIInstance = subInstance;
        m_ItemAssets.Add(subInstance);

        var leftContainer = elementTemplateInstance.Q<VisualElement>("armature-parts-content-container");
        leftContainer.Add(subInstance);
        leftContainer.style.display = DisplayStyle.Flex;
        leftContainer.style.maxHeight = Math.Min(30 * armatureSet.armaturePartsInfo.Count, float.PositiveInfinity);

        int total = 0;
        for (int i = 0; i < itemDB.armatureSets.Count; i++)
        {
            for (int j = 0; j < itemDB.armatureSets[i].armaturePartsInfo.Count; j++)
            {
                total++;
            }
        }
        armaturesContainer.style.maxHeight = Math.Min(30 * (itemDB.armatureSets.Count + total), float.PositiveInfinity);

        // Assignar al boton el metodo que muestra la info del item
        TemplateContainer subItemRightPanelElement = m_ArmorRightTemplate.CloneTree();
        var subSelfItemButton = subInstance.Q<Button>("foldout-element");
        subSelfItemButton.name = newArmaturePart.index.ToString();
        subSelfItemButton.clickable.clicked += () => ItemButtonClicked(subSelfItemButton.name, subItemRightPanelElement, subInstance);

        // Assignar al boton el metodo de remover
        var removeArmaturePartButton = instance.Q<Button>("remove-armature-part-button");
        removeArmaturePartButton.name = itemDB.items.IndexOf(newArmaturePart).ToString();
        removeArmaturePartButton.clickable.clicked += () => RemovePartOfArmatureSet(removeArmaturePartButton.name, armatureSet, currentRightPanelElement, instance, leftContainer, newArmaturePart.SelfUIInstance);

        this.hasUnsavedChanges = true;
        Debug.Log($"Armature part added, total parts = {armatureSet.armaturePartsInfo.Count}");
    }
    private void RemovePartOfArmatureSet(string buttonName, ArmatureSet armatureSet, VisualElement rightPanelElement, TemplateContainer armaturePartElement, VisualElement leftContainer, TemplateContainer subInstance)
    {
        // Convertir el nombre del botón a un identificador único
        int id = int.Parse(buttonName);

        // Obtener el item correspondiente al identificador para la lista de informacion de las partes
        var armaturePartInfo = armatureSet.FindArmatureParts(id);
        armatureSet.armaturePartsInfo.Remove(armaturePartInfo);

        // Obtener el item correspondiente al identificador para la lista de items
        var armaturePart = itemDB.FindItem(armaturePartInfo.index);
        itemDB.items.Remove(armaturePart);

        var container = rightPanelElement.Q<VisualElement>("armature-parts-content");
        container.Remove(armaturePartElement);

        // REMOVE ITEM ELEMENT (ARMATURE PART) FOR LEFT SIDE OF TEMPLATE
        leftContainer.Remove(subInstance);
        armaturePart.SelfUIInstance = null;
        leftContainer.style.maxHeight = Math.Min(30 * armatureSet.armaturePartsInfo.Count, float.PositiveInfinity);

        int total = 0;
        for (int i = 0; i < itemDB.armatureSets.Count; i++)
        {
            for (int j = 0; j < itemDB.armatureSets[i].armaturePartsInfo.Count; j++)
            {
                total++;
            }
        }
        armaturesContainer.style.maxHeight = Math.Min(30 * (itemDB.armatureSets.Count + total), float.PositiveInfinity);

        this.hasUnsavedChanges = true;
        Debug.Log($"Armature part: {armaturePart} removed, at index = {armaturePart.index}, total stat = {armatureSet.armaturePartsInfo.Count}");
    }

    // Items
    void CreateItem(VisualElement container, VisualElement arrow, ItemType type)
    {
        var instance = m_ItemElement.CloneTree();
        m_ItemAssets.Add(instance);
        container.Add(instance);

        // Crear un nuevo item y agregarlo a la base de datos
        var newItem = new Item(type);
        itemDB.items.Add(newItem);
        itemDB.FindItem(newItem).InitializeLists();

        Action addStatAction = null;
        Action addScalingAction = null;
        Action addUpgradeAction = null;
        Action addRequirementAction = null;
        Action addatributeModifierAction = null;
        Action addStatModifier = null;

        TemplateContainer itemRightPanelElement = null;

        switch (newItem.type)
        {
            case ItemType.Weapon:
                itemRightPanelElement = m_WeaponRightTemplate.CloneTree();
                weaponAmountLabel.text = $"{itemDB.GetCountByTypeOnItemList(ItemType.Weapon)} items";
                break;

            case ItemType.Material:
                itemRightPanelElement = m_MaterialRightTemplate.CloneTree();
                materialAmountLabel.text = $"{itemDB.GetCountByTypeOnItemList(ItemType.Material)} items";
                break;

            case ItemType.Consumable:
                itemRightPanelElement = m_ConsumableRightTemplate.CloneTree();
                consumableAmountLabel.text = $"{itemDB.GetCountByTypeOnItemList(ItemType.Consumable)} items";
                break;

            case ItemType.QuestItem:
                itemRightPanelElement = m_QuestItemRightTemplate.CloneTree();
                questItemAmountLabel.text = $"{itemDB.GetCountByTypeOnItemList(ItemType.QuestItem)} items";
                break;

            case ItemType.Amulet:
                itemRightPanelElement = m_AmuletRightTemplate.CloneTree();
                amuletItemAmountLabel.text = $"{itemDB.GetCountByTypeOnItemList(ItemType.Amulet)} items";
                break;
        }

        // Asignar el índice del nuevo item como su id
        newItem.index = itemDB.items.IndexOf(newItem);

        // Capturar el valor actual del índice en una variable local
        int currentItemIndex = newItem.index;

        // Asignar las acciones para los botones del nuevo item
        itemActions[currentItemIndex] = (addStatAction, addScalingAction, addUpgradeAction, addRequirementAction, addatributeModifierAction, addStatModifier);

        // Obtener el botón del elemento clonado
        var sefItemButton = instance.Q<Button>("foldout-element");
        var removeItemButton = instance.Q<Button>("remove-item-button");

        // Asignar el índice del nuevo item como identificador único del botón
        sefItemButton.name = currentItemIndex.ToString();
        removeItemButton.name = currentItemIndex.ToString();

        // Agregar un listener al botón para manejar el evento de click
        sefItemButton.clickable.clicked += () => ItemButtonClicked(currentItemIndex.ToString(), itemRightPanelElement, instance);
        removeItemButton.clickable.clicked += () => RemoveItem(currentItemIndex.ToString(), container, arrow, instance, type);
        removeItemButton.RegisterValueChangedCallback((evt) =>
        {
            this.hasUnsavedChanges = true;
        });

        // Actualiza el display y MaxHeight de itemContent basado en la lista
        if (itemDB.GetCountByTypeOnItemList(type) == 0)
        {
            container.style.display = DisplayStyle.None;
        }
        else
        {
            container.style.display = DisplayStyle.Flex;
            SetArrowAnim(container, arrow, "chechmark-toggle-open");
            container.style.maxHeight = Math.Min(30 * itemDB.GetCountByTypeOnItemList(type), float.PositiveInfinity);
        }

        // Actualiza la ventana
        this.Repaint();
        this.hasUnsavedChanges = true;
    }
    private void RemoveItem(string buttonName, VisualElement container, VisualElement arrow, TemplateContainer instance, ItemType type)
    {
        // Convertir el nombre del botón a un identificador único
        int id = int.Parse(buttonName);

        // Obtener el item correspondiente al identificador
        var item = itemDB.items.Find(i => i.index == id);

        // Remover el item de la lista de items
        itemDB.items.Remove(item);

        // Remover elemento visual de la ventana
        m_ItemAssets.Remove(instance);
        container.Remove(instance);

        // Actualiza el display y MaxHeight de itemContent basado en la lista
        if (itemDB.GetCountByTypeOnItemList(type) == 0)
        {
            container.style.display = DisplayStyle.None;
            SetArrowAnim(container, arrow, "chechmark-toggle-open");

            // removemos cualquier instancia de rightElementContainer
            if (currentRightPanelElement != null)
            {
                rightElementContainer.Remove(currentRightPanelElement);
                currentRightPanelElement = null;
            }
        }
        else
        {
            container.style.display = DisplayStyle.Flex;
            container.style.maxHeight = Math.Min(30 * itemDB.GetCountByTypeOnItemList(type), float.PositiveInfinity);

            // removemos cualquier instancia de rightElementContainer
            if (currentRightPanelElement != null)
            {
                rightElementContainer.Remove(currentRightPanelElement);
                currentRightPanelElement = null;
            }
        }

        armaturesAmountLabel.text = $"{itemDB.GetCountByTypeOnItemList(ItemType.Armor)} items";
        weaponAmountLabel.text = $"{itemDB.GetCountByTypeOnItemList(ItemType.Weapon)} items";
        materialAmountLabel.text = $"{itemDB.GetCountByTypeOnItemList(ItemType.Material)} items";
        consumableAmountLabel.text = $"{itemDB.GetCountByTypeOnItemList(ItemType.Consumable)} items";
        questItemAmountLabel.text = $"{itemDB.GetCountByTypeOnItemList(ItemType.QuestItem)} items";
        amuletItemAmountLabel.text = $"{itemDB.GetCountByTypeOnItemList(ItemType.Amulet)} items";

        this.hasUnsavedChanges = true;

        // Aquí puedes mostrar la información del item
        Debug.Log($"Se ha removido el item: {item.name}, de id: {id}");
    }

    // Display items info
    void ItemButtonClicked(string buttonName, TemplateContainer itemRightPanelElement, TemplateContainer elementTemplateInstance)
    {
        // Convertir el nombre del botón a un índice
        int index = int.Parse(buttonName);

        // Obtener el item correspondiente al índice
        var item = itemDB.FindItem(index);

        var (oldAddStat, oldAddScaling, oldAddUpgrade, oldAddRequirement, oldAddAtributeModifier, oldAddStatModifier) = itemActions[index];

        // Almacenar los botones generales del panel
        var addStatButton = itemRightPanelElement.Q<Button>("add-stat-button");
        var addScalingButton = itemRightPanelElement.Q<Button>("add-scaling-button");
        var addUpgradeButton = itemRightPanelElement.Q<Button>("add-upgrade-button");
        var addRequirementButton = itemRightPanelElement.Q<Button>("add-requirement-button");
        var addAtributeModifierButton = itemRightPanelElement.Q<Button>("add-attribute-modifiers-button");
        var addStatModifierButton = itemRightPanelElement.Q<Button>("add-stats-modifiers-button");

        // Desuscribir los eventos antiguos
        addStatButton.clickable.clicked -= oldAddStat;
        addScalingButton.clickable.clicked -= oldAddScaling;
        addUpgradeButton.clickable.clicked -= oldAddUpgrade;
        addRequirementButton.clickable.clicked -= oldAddRequirement;
        addAtributeModifierButton.clickable.clicked -= oldAddAtributeModifier;
        addStatModifierButton.clickable.clicked -= oldAddStatModifier;

        // Crear nuevas acciones para este ítem específico
        void newAddStat() => AddStatToItem(item, itemRightPanelElement);
        void newAddScaling() => AddScalingToItem(item, itemRightPanelElement);
        void newAddUpgrade() => AddUpgradeToItem(item, itemRightPanelElement);
        void newAddRequirement() => AddRequirementToItem(item, itemRightPanelElement);
        void newAddAtributeModifier() => AddAttributeModifierToItem(item, itemRightPanelElement);
        void newAddStatModifier() => AddStatModifierToItem(item, itemRightPanelElement);

        // Actualizar las acciones en el diccionario
        itemActions[index] = (newAddStat, newAddScaling, newAddUpgrade, newAddRequirement, newAddAtributeModifier, newAddStatModifier);

        // Suscribir los nuevos eventos
        addStatButton.clickable.clicked += newAddStat;
        addScalingButton.clickable.clicked += newAddScaling;
        addUpgradeButton.clickable.clicked += newAddUpgrade;
        addRequirementButton.clickable.clicked += newAddRequirement;
        addAtributeModifierButton.clickable.clicked += newAddAtributeModifier;
        addStatModifierButton.clickable.clicked += newAddStatModifier;

        // Actualizar el nombre en el UI
        var currentItemNameField = base_Root.Q<Label>("current-item-name");
        currentItemNameField.text = item.name;

        // Mostrar la informacion del item en el panel derecho
        DisplayItemInfo(item, itemRightPanelElement, elementTemplateInstance);

        // Debug de la información del item
        Debug.Log($"Se ha hecho clic en el botón del item: {item.name}, de indice: {index}, que contiene {item.Stats.Count} stats y {item.Upgrades.Count} mejoras");
    }
    void DisplayItemInfo(Item item, TemplateContainer itemRightPanelElement, TemplateContainer itemTemplateInstance)
    {
        #region Base
        // Si hay un elemento en el panel derecho, lo removemos
        if (currentRightPanelElement != null)
        {
            rightElementContainer.Remove(currentRightPanelElement);
        }

        // Agregamos el nuevo elemento al panel derecho
        rightElementContainer.Add(itemRightPanelElement);

        // Actualizamos la referencia al elemento actual en el panel derecho
        currentRightPanelElement = itemRightPanelElement;
        #endregion

        #region Item General Settings
        // Creamos variables para los campos que necesitamos
        Color transparent = new(0, 0, 0, 0);
        var defaultIcon = Resources.Load<Texture2D>("Img/Item_Icon");
        var weaponNameField = itemRightPanelElement.Q<TextField>("name-field");
        var weaponIconField = itemRightPanelElement.Q<ObjectField>("icon-field");
        var weaponPrefabField = itemRightPanelElement.Q<ObjectField>("prefab-field");

        var imageOfIconField = itemRightPanelElement.Q<VisualElement>("item-icon");
        var itemElementIcon = itemTemplateInstance.Q<VisualElement>("icon");
        var itemName = itemTemplateInstance.Q<Label>("item-name");

        // Cargamos la información del ítem en los campos
        if (!String.IsNullOrEmpty(item.name))
        {
            weaponNameField.value = item.name;
        }
        else weaponNameField.value = "New Item";

        if (item.icon != null)
        {
            weaponIconField.value = item.icon;
            imageOfIconField.style.backgroundColor = new StyleColor(transparent);
        }
        else weaponIconField.value = defaultIcon;

        weaponPrefabField.value = item.prefab;

        // Agregamos manejadores de eventos a los campos para actualizar la información del ítem cuando cambien
        weaponNameField.RegisterValueChangedCallback((evt) => 
        { 
            item.name = evt.newValue;
            itemName.text = item.name;
            this.hasUnsavedChanges = true;
        });

        weaponIconField.RegisterValueChangedCallback((evt) =>
        {
            item.icon = evt.newValue as Sprite;
            imageOfIconField.style.backgroundImage = new StyleBackground(item.icon);
            itemElementIcon.style.backgroundImage = new StyleBackground(item.icon);
            imageOfIconField.style.backgroundColor = new StyleColor(transparent);
            this.hasUnsavedChanges = true;
        });

        weaponPrefabField.RegisterValueChangedCallback((evt) =>
        {
            item.prefab = evt.newValue as GameObject;
            this.hasUnsavedChanges = true;
        });
        #endregion

        #region Extra Settings
        // Creamos variables para los campos que necesitamos
        var weaponHandEnum = itemRightPanelElement.Q<EnumField>("weapon-hand-enum");
        var weaponMainHandEnum = itemRightPanelElement.Q<EnumField>("weapon-main-hand-enum");
        var itemDamageTypeSelector = itemRightPanelElement.Q<Button>("damage-type-selector-button");
        var itemSlotSelector = itemRightPanelElement.Q<Button>("item-slot-selector-button");
        var actionTagSelector = itemRightPanelElement.Q<Button>("actions-tag-selector-button");
        var movesetSelector = itemRightPanelElement.Q<Button>("moveset-selector-button");
        var itemDamageTypeTagLabel = itemRightPanelElement.Q<Label>("damage-type-tag");
        var itemSlotTagLabel = itemRightPanelElement.Q<Label>("item-slot-tag");
        var actionTagLabel = itemRightPanelElement.Q<Label>("actions-tag");
        var movesetTagLabel = itemRightPanelElement.Q<Label>("moveset-tag");
        var weightField = itemRightPanelElement.Q<FloatField>("weight-field");
        var bodySlotField = itemRightPanelElement.Q<TextField>("body-slot-field");
        var handSlotField = itemRightPanelElement.Q<TextField>("hand-slot-field");
        var sellableToggle = itemRightPanelElement.Q<Toggle>("sellable-toggle");
        var upgradeableToggle = itemRightPanelElement.Q<Toggle>("upgradeable-toggle");
        var discartableToggle = itemRightPanelElement.Q<Toggle>("discartable-toggle");

        // Cargamos la información del ítem en los campos
        weaponHandEnum.value = item.hand;
        weaponMainHandEnum.value = item.mainHand;
        weaponMainHandEnum.style.display = item.hand == WeaponHand.TwoHand ? DisplayStyle.Flex : DisplayStyle.None;

        if (!String.IsNullOrEmpty(item.damageType))
        {
            itemDamageTypeTagLabel.text = item.damageType;
        }
        else itemDamageTypeTagLabel.text = "damage type";

        if (!String.IsNullOrEmpty(item.itemSlot))
        {
            itemSlotTagLabel.text = item.itemSlot;
        }
        else itemSlotTagLabel.text = "item slot";

        if (!String.IsNullOrEmpty(item.actionsTag))
        {
            actionTagLabel.text = item.actionsTag;
        }
        else actionTagLabel.text = "moveset actions";

        if (!String.IsNullOrEmpty(item.movesetTag))
        {
            movesetTagLabel.text = item.movesetTag;
        }
        else movesetTagLabel.text = "moveset overlay";

        if (!String.IsNullOrEmpty(item.name))
        {
            weightField.value = item.weight;
        }
        else weightField.value = 0;

        if (!String.IsNullOrEmpty(item.name))
        {
            bodySlotField.value = item.bodySlot;
        }
        else bodySlotField.value = "None";

        if(!String.IsNullOrEmpty(item.name))
        {
            handSlotField.value = item.handSlot;
        }
        else handSlotField.value = "None";

        if (item.canBeSell)
        {
            sellableToggle.value = item.canBeSell;
        }
        else sellableToggle.value = false;

        if (item.canBeUpgrade)
        {
            upgradeableToggle.value = item.canBeUpgrade;
        }
        else upgradeableToggle.value = false;

        if (item.canBeDiscarted)
        {
            discartableToggle.value = item.canBeDiscarted;
        }
        else discartableToggle.value = false;

        // Agregamos manejadores de eventos a los campos para actualizar la información del ítem cuando cambien
        weaponHandEnum.RegisterValueChangedCallback((evt) =>
        {
            item.hand = (WeaponHand)evt.newValue;

            if (item.hand == WeaponHand.TwoHand) 
                weaponMainHandEnum.style.display = DisplayStyle.Flex;
            else weaponMainHandEnum.style.display = DisplayStyle.None;

            this.hasUnsavedChanges = true;
        });

        weaponMainHandEnum.RegisterValueChangedCallback((evt) =>
        {
            item.mainHand = (MainHand)evt.newValue;
            this.hasUnsavedChanges = true;
        });

        itemDamageTypeSelector.clickable.clicked += () => OpenTagSelectorWindow(itemDamageTypeTagLabel, item, "damageType");
        itemSlotSelector.clickable.clicked += () => OpenTagSelectorWindow(itemSlotTagLabel, item, "itemSlot");
        actionTagSelector.clickable.clicked += () => OpenTagSelectorWindow(actionTagLabel, item, "actionsTag");
        movesetSelector.clickable.clicked += () => OpenTagSelectorWindow(movesetTagLabel, item, "movesetTag");

        weightField.RegisterValueChangedCallback((evt) =>
        {
            item.weight = evt.newValue;
            this.hasUnsavedChanges = true;
        });

        bodySlotField.RegisterValueChangedCallback((evt) =>
        {
            item.bodySlot = evt.newValue;
            this.hasUnsavedChanges = true;
        });

        handSlotField.RegisterValueChangedCallback((evt) =>
        {
            item.handSlot = evt.newValue;
            this.hasUnsavedChanges = true;
        });

        sellableToggle.RegisterValueChangedCallback((evt) =>
        {
            item.canBeSell = evt.newValue;
            this.hasUnsavedChanges = true;
        });

        upgradeableToggle.RegisterValueChangedCallback((evt) =>
        {
            item.canBeUpgrade = evt.newValue;
            this.hasUnsavedChanges = true;
        });

        discartableToggle.RegisterValueChangedCallback((evt) =>
        {
            item.canBeDiscarted = evt.newValue;
            this.hasUnsavedChanges = true;
        });
        #endregion

        #region Item Stats
        if (item.Stats.Count > 0)
        {
            // Accedemos al contenedor
            var container = itemRightPanelElement.Q<ScrollView>("stats-scrollview");

            // Limpiamos el contenedor antes de agregar los elementos
            container.Clear();

            // Recorremos la lista de stats en el item dado
            foreach (var stat in item.Stats)
            {
                // Clona la plantilla del elemento stat
                var instance = m_StatsElement.CloneTree();

                // Accedemos a los campos de la plantilla
                var tagSelectorButton = instance.Q<Button>("tag-selector-button");
                var tagLabel = instance.Q<Label>("stat-tag");
                var statValueField = instance.Q<FloatField>("stat-value-field");
                var removeStatButton = instance.Q<Button>("remove-stat-button");

                // Mostramos la informacion
                tagLabel.text = !String.IsNullOrEmpty(stat.statTag) ? stat.statTag : "Stat Tag";               
                statValueField.value = stat.startValue > 0 ? stat.startValue : 0;
                removeStatButton.name = item.Stats.IndexOf(stat).ToString();

                // Agregamos manejadores de eventos a los campos para actualizar la información del ítem cuando cambien
                statValueField.RegisterValueChangedCallback((evt) =>
                {
                    stat.startValue = evt.newValue;
                    this.hasUnsavedChanges = true;
                });

                // Asignamos funcionalidad a los botones
                tagSelectorButton.clickable.clicked += () => OpenTagSelectorWindow(tagLabel, stat, "statTag");
                removeStatButton.clickable.clicked += () => RemoveStatOnItem(removeStatButton.name, item, currentRightPanelElement, instance);

                // Agregamos la plantilla al contenedor
                container.Add(instance);
            }
        }
        #endregion

        #region Item Scaling
        if (item.Scaled.Count > 0)
        {
            // Accedemos al contenedor
            var container = itemRightPanelElement.Q<ScrollView>("stats-scaling-scrollview");

            // Limpiamos el contenedor antes de agregar los elementos
            container.Clear();

            // Recorremos la lista de stats en el item dado
            foreach (var currentScale in item.Scaled)
            {
                // Clona la plantilla del elemento stat
                var instance = m_ScaledElement.CloneTree();

                // Accedemos a los campos de la plantilla
                var tagSelectorButton = instance.Q<Button>("tag-selector-button");
                var tagLabel = instance.Q<Label>("stat-tag");
                var scaleLevelEnum = instance.Q<EnumField>("scaling-level-enum");
                var scaledMathOP = instance.Q<ObjectField>("scale-math-op-object");
                var removeScaleButton = instance.Q<Button>("remove-scale-button");

                // Mostramos la informacion
                tagLabel.text = !String.IsNullOrEmpty(currentScale.attributeTag) ? currentScale.attributeTag : "Attribute Tag";
                scaleLevelEnum.value = currentScale.startScale;
                scaledMathOP.value = currentScale.operation != null ? currentScale.operation : null;
                removeScaleButton.name = item.Scaled.IndexOf(currentScale).ToString();

                // Agregamos manejadores de eventos a los campos para actualizar la información del ítem cuando cambien
                scaleLevelEnum.RegisterValueChangedCallback((evt) =>
                {
                    currentScale.startScale = (ScalingLevel)evt.newValue;
                    this.hasUnsavedChanges = true;
                });

                scaledMathOP.RegisterValueChangedCallback((evt) =>
                {
                    currentScale.operation = (ScaleMathOperation)evt.newValue;
                    this.hasUnsavedChanges = true;
                });

                // Asignamos funcionalidad a los botones
                tagSelectorButton.clickable.clicked += () => OpenTagSelectorWindow(tagLabel, currentScale, "attributeTag");
                removeScaleButton.clickable.clicked += () => RemoveScalingOnItem(removeScaleButton.name, item, currentRightPanelElement, instance);

                // Agregamos la plantilla al contenedor
                container.Add(instance);
            }
        }
        #endregion

        #region Item Upgrades
        if (item.Upgrades.Count > 0)
        {
            // Accedemos al contenedor
            var container = itemRightPanelElement.Q<ScrollView>("upgrades-scrollview");

            // Limpiamos el contenedor antes de agregar los elementos
            container.Clear();

            // Recorremos la lista de stats en el item dado
            foreach (var upgrade in item.Upgrades)
            {
                // Clona la plantilla del elemento stat
                var instance = m_UpgradeElement.CloneTree();

                // Accedemos a los campos de la plantilla
                var upgradeNameField = instance.Q<TextField>("upgrade-name-field");
                var removeUpgradeButton = instance.Q<Button>("remove-upgrade-button");

                var upgradeStatToggle = instance.Q<ToolbarToggle>("upgrade-stat-toggle");
                var upgradeStatContent = instance.Q<VisualElement>("upgrade-stat-content");
                var upgradeValueField = instance.Q<FloatField>("upgrade-value-field");
                var operationEnum = instance.Q<EnumField>("upgrade-operation-enum");
                var valueTypeEnum = instance.Q<EnumField>("upgrade-value-type-field");
                var baseOnEnum = instance.Q<EnumField>("upgrade-baseon-enum");
                var tagSelectorButton = instance.Q<Button>("tag-selector-button");
                var tagLabel = instance.Q<Label>("upgrade-tag");

                var upgradeScaleToggle = instance.Q<ToolbarToggle>("upgrade-scale-toggle");
                var upgradeScaleContent = instance.Q<VisualElement>("upgrade-scale-content");
                var upgradeScaleIndex = instance.Q<IntegerField>("scale-index-field");
                var upgradeNewScale = instance.Q<EnumField>("new-scale-enum");

                var upgradeAttModToggle = instance.Q<ToolbarToggle>("upgrade-attribute-modifier-toggle");
                var upgradeAttModContent = instance.Q<VisualElement>("upgrade-attribute-modifier-content");
                var upgradeAttModIndex = instance.Q<IntegerField>("attribute-modifier-index-field");
                var upgradeAttModValue = instance.Q<FloatField>("attribute-modifier-newvalue-field");

                var upgradeStatModToggle = instance.Q<ToolbarToggle>("upgrade-stat-modifier-toggle");
                var upgradeStatModContent = instance.Q<VisualElement>("upgrade-stat-modifier-content");
                var upgradeStatModIndex = instance.Q<IntegerField>("stat-modifier-index-field");
                var upgradeStattModValue = instance.Q<FloatField>("stat-modifier-newvalue-field");

                #region Stat Upgrade

                // Mostramos la informacion
                upgradeStatToggle.value = upgrade.useStatUpgrade;
                SetElementDisplay(upgradeStatToggle.value, upgradeStatContent);
                upgradeNameField.value = !String.IsNullOrEmpty(upgrade.name) ? upgrade.name : "";
                upgradeValueField.value = upgrade.value > 0 ? upgrade.value : 0;
                operationEnum.value = upgrade.opType;
                valueTypeEnum.value = upgrade.valueType;
                baseOnEnum.value = upgrade.baseOn;
                tagLabel.text = !String.IsNullOrEmpty(upgrade.statTag) ? upgrade.statTag : "item stat";

                // Agregamos manejadores de eventos a los campos para actualizar la información del ítem cuando cambien
                upgradeStatToggle.RegisterValueChangedCallback((evt) =>
                {
                    upgrade.useStatUpgrade = evt.newValue;
                    SetElementDisplay(evt.newValue, upgradeStatContent);
                    this.hasUnsavedChanges = true;
                });

                upgradeNameField.RegisterValueChangedCallback((evt) =>
                {
                    upgrade.name = evt.newValue;
                    upgradeNameField. value = upgrade.name;
                    this.hasUnsavedChanges = true;
                });

                upgradeValueField.RegisterValueChangedCallback((evt) =>
                {
                    upgrade.value = evt.newValue;
                    upgradeValueField.value = upgrade.value;
                    this.hasUnsavedChanges = true;
                });

                operationEnum.RegisterValueChangedCallback((evt) =>
                {
                    upgrade.opType = (OperationType)evt.newValue;
                    this.hasUnsavedChanges = true;
                });

                valueTypeEnum.RegisterValueChangedCallback((evt) =>
                {
                    upgrade.valueType = (UltimateFramework.Utils.ValueType)evt.newValue;
                    this.hasUnsavedChanges = true;
                });

                baseOnEnum.RegisterValueChangedCallback((evt) =>
                {
                    upgrade.baseOn = (BaseOn)evt.newValue;
                    this.hasUnsavedChanges = true;
                });
                #endregion

                #region Scale Upgrade

                // Mostramos la informacion
                upgradeScaleToggle.value = upgrade.useScaleUpgrade;
                SetElementDisplay(upgradeScaleToggle.value, upgradeScaleContent);
                upgradeScaleIndex.value = upgrade.scaleToAffectIndex > 0 ? upgrade.scaleToAffectIndex : 0;
                upgradeNewScale.value = upgrade.newScaleLevel;

                // Agregamos manejadores de eventos a los campos para actualizar la información del ítem cuando cambien
                upgradeScaleToggle.RegisterValueChangedCallback((evt) =>
                {
                    upgrade.useScaleUpgrade = evt.newValue;
                    SetElementDisplay(evt.newValue, upgradeScaleContent);
                    this.hasUnsavedChanges = true;
                });

                upgradeScaleIndex.RegisterValueChangedCallback((evt) =>
                {
                    upgrade.scaleToAffectIndex = evt.newValue;
                    this.hasUnsavedChanges = true;
                });

                upgradeNewScale.RegisterValueChangedCallback((evt) =>
                {
                    upgrade.newScaleLevel = (ScalingLevel)evt.newValue;
                    this.hasUnsavedChanges = true;
                });
                #endregion

                #region Att Mod Upgrade

                // Mostramos la informacion
                upgradeAttModToggle.value = upgrade.useAttModUpgrade;
                SetElementDisplay(upgradeAttModToggle.value, upgradeAttModContent);
                upgradeAttModIndex.value = upgrade.attModToAffectIndex > 0 ? upgrade.attModToAffectIndex : 0;
                upgradeAttModValue.value = upgrade.attModNewValue;

                // Agregamos manejadores de eventos a los campos para actualizar la información del ítem cuando cambien
                upgradeAttModToggle.RegisterValueChangedCallback((evt) =>
                {
                    upgrade.useAttModUpgrade = evt.newValue;
                    SetElementDisplay(evt.newValue, upgradeAttModContent);
                    this.hasUnsavedChanges = true;
                });

                upgradeAttModIndex.RegisterValueChangedCallback((evt) =>
                {
                    upgrade.attModToAffectIndex = evt.newValue;
                    this.hasUnsavedChanges = true;
                });

                upgradeAttModValue.RegisterValueChangedCallback((evt) =>
                {
                    upgrade.attModNewValue = evt.newValue;
                    this.hasUnsavedChanges = true;
                });
                #endregion

                #region Stat Mod Upgrade

                // Mostramos la informacion
                upgradeStatModToggle.value = upgrade.useStatModUpgrade;
                SetElementDisplay(upgradeStatModToggle.value, upgradeStatModContent);
                upgradeStatModIndex.value = upgrade.statToAffectIndex > 0 ? upgrade.statToAffectIndex : 0;
                upgradeStattModValue.value = upgrade.statModNewValue;

                // Agregamos manejadores de eventos a los campos para actualizar la información del ítem cuando cambien
                upgradeStatModToggle.RegisterValueChangedCallback((evt) =>
                {
                    upgrade.useStatModUpgrade = evt.newValue;
                    SetElementDisplay(evt.newValue, upgradeStatModContent);
                    this.hasUnsavedChanges = true;
                });

                upgradeStatModIndex.RegisterValueChangedCallback((evt) =>
                {
                    upgrade.statToAffectIndex = evt.newValue;
                    this.hasUnsavedChanges = true;
                });

                upgradeStattModValue.RegisterValueChangedCallback((evt) =>
                {
                    upgrade.statModNewValue = evt.newValue;
                    this.hasUnsavedChanges = true;
                });
                #endregion

                removeUpgradeButton.name = item.Upgrades.IndexOf(upgrade).ToString();

                // Asignamos funcionalidad a los botones
                tagSelectorButton.clickable.clicked += () => OpenTagSelectorWindow(tagLabel, upgrade, "statTag");
                removeUpgradeButton.clickable.clicked += () => RemoveUpgradeOnItem(removeUpgradeButton.name, item, currentRightPanelElement, instance);

                // Agregamos la plantilla al contenedor
                container.Add(instance);        
            }
        }
        #endregion

        #region Item Requirements
        if (item.Requirements.Count > 0)
        {
            // Accedemos al contenedor
            var container = currentRightPanelElement.Q<ScrollView>("requirements-scrollview");

            // Limpiamos el contenedor antes de agregar los elementos
            container.Clear();

            // Recorremos la lista de stats en el item dado
            foreach (var requirement in item.Requirements)
            {
                // Clona la plantilla del elemento stat
                var instance = m_RequirementsElement.CloneTree();

                // Accedemos a los campos de la plantilla
                var asUseItemContainer = instance.Q<VisualElement>("useItem-option-container");
                var asUpgradeContainer = instance.Q<VisualElement>("upgrade-option-container");
                var useForEnum = instance.Q<EnumField>("upgrade-usefor-enum");
                var tagSelectorButton = instance.Q<Button>("tag-selector-button");
                var tagLabel = instance.Q<Label>("attribute-tag");
                var attributeValueField = instance.Q<FloatField>("attribute-value-field");
                var upgradeNameField = instance.Q<TextField>("upgrade-name-field");
                var itemNameField = instance.Q<TextField>("item-name-field");
                var itemAmmountField = instance.Q<IntegerField>("item-ammount-field");
                var removeRequirementButton = instance.Q<Button>("remove-requirement-button");

                // Mostramos la informacion
                useForEnum.value = requirement.requirementFor;
                tagLabel.text = !String.IsNullOrEmpty(requirement.attributeTag) ? requirement.attributeTag : "Attribute tag";
                attributeValueField.value = requirement.value > 0 ? requirement.value : 0;
                upgradeNameField.value = !String.IsNullOrEmpty(requirement.upgradeName) ? requirement.upgradeName : "";
                itemNameField.value = !String.IsNullOrEmpty(requirement.itemName) ? requirement.itemName : "";
                itemAmmountField.value = requirement.itemAmmount;
                removeRequirementButton.name = item.Requirements.IndexOf(requirement).ToString();
                DisplayRequirementContainer(requirement.requirementFor, asUseItemContainer, asUpgradeContainer);

                // Agregamos manejadores de eventos a los campos para actualizar la información del ítem cuando cambien
                useForEnum.RegisterValueChangedCallback((evt) =>
                {
                    requirement.requirementFor = (RequirementFor)evt.newValue;
                    useForEnum.value = requirement.requirementFor;
                    DisplayRequirementContainer(requirement.requirementFor, asUseItemContainer, asUpgradeContainer);
                    this.hasUnsavedChanges = true;
                });

                attributeValueField.RegisterValueChangedCallback((evt) =>
                {
                    requirement.value = evt.newValue;
                    attributeValueField.value = requirement.value;
                    this.hasUnsavedChanges = true;
                });

                upgradeNameField.RegisterValueChangedCallback((evt) =>
                {
                    requirement.upgradeName = evt.newValue;
                    upgradeNameField.value = requirement.upgradeName;
                    this.hasUnsavedChanges = true;
                });

                itemNameField.RegisterValueChangedCallback((evt) =>
                {
                    requirement.itemName = evt.newValue;
                    itemNameField.value = requirement.itemName;
                    this.hasUnsavedChanges = true;
                });

                itemAmmountField.RegisterValueChangedCallback((evt) =>
                {
                    requirement.itemAmmount = evt.newValue;
                    itemAmmountField.value = requirement.itemAmmount;
                    this.hasUnsavedChanges = true;
                });

                // Asignamos funcionalidad a los botones
                tagSelectorButton.clickable.clicked += () => OpenTagSelectorWindow(tagLabel, requirement, "attributeTag");
                removeRequirementButton.clickable.clicked += () => RemoveRequirementOnItem(removeRequirementButton.name, item, currentRightPanelElement, instance);

                // Agregamos la plantilla al contenedor
                container.Add(instance);
            }
        }
        #endregion

        #region Item Attribute Modifiers
        if (item.AttributeModifiers.Count > 0)
        {
            // Accedemos al contenedor
            var container = itemRightPanelElement.Q<ScrollView>("attribute-modifiers-scrollview");

            // Limpiamos el contenedor antes de agregar los elementos
            container.Clear();

            // Recorremos la lista de stats en el item dado
            foreach (var attributeModifier in item.AttributeModifiers)
            {
                // Clona la plantilla del elemento stat
                var instance = m_AttributeModifierElement.CloneTree();

                // Accedemos a los campos de la plantilla
                var attModifierValueField = instance.Q<FloatField>("attribute-modifier-value-field");
                var removeAttModifierButton = instance.Q<Button>("remove-attribute-modifier-button");
                var operationEnum = instance.Q<EnumField>("attribute-modifier-operation-enum");
                var valueTypeEnum = instance.Q<EnumField>("attribute-modifier-value-type-field");
  
                var tagSelectorButton = instance.Q<Button>("tag-selector-button");
                var tagLabel = instance.Q<Label>("attribute-modifier-tag");

                // Mostramos la informacion
                attModifierValueField.value = attributeModifier.startValue > 0 ? attributeModifier.startValue : 0;
                operationEnum.value = attributeModifier.opType;
                valueTypeEnum.value = attributeModifier.valueType;
                tagLabel.text = !String.IsNullOrEmpty(attributeModifier.attributeType) ? attributeModifier.attributeType : "character attribute";

                // Agregamos manejadores de eventos a los campos para actualizar la información del ítem cuando cambien
                attModifierValueField.RegisterValueChangedCallback((evt) =>
                {
                    attributeModifier.startValue = evt.newValue;
                    attModifierValueField.value = attributeModifier.startValue;
                    this.hasUnsavedChanges = true;
                });

                operationEnum.RegisterValueChangedCallback((evt) =>
                {
                    attributeModifier.opType = (OperationType)evt.newValue;
                    this.hasUnsavedChanges = true;
                });

                valueTypeEnum.RegisterValueChangedCallback((evt) =>
                {
                    attributeModifier.valueType = (UltimateFramework.Utils.ValueType)evt.newValue;
                    this.hasUnsavedChanges = true;
                });

                removeAttModifierButton.name = item.AttributeModifiers.IndexOf(attributeModifier).ToString();

                // Asignamos funcionalidad a los botones
                tagSelectorButton.clickable.clicked += () => OpenTagSelectorWindow(tagLabel, attributeModifier, "attributeType");
                removeAttModifierButton.clickable.clicked += () => RemoveAttributeModifierOnItem(removeAttModifierButton.name, item, currentRightPanelElement, instance);

                // Agregamos la plantilla al contenedor
                container.Add(instance);
            }
        }
        #endregion

        #region Item Stats Modifiers
        if (item.StatModifiers.Count > 0)
        {
            // Accedemos al contenedor
            var container = itemRightPanelElement.Q<ScrollView>("stats-modifiers-scrollview");

            // Limpiamos el contenedor antes de agregar los elementos
            container.Clear();

            // Recorremos la lista de stats en el item dado
            foreach (var statModifier in item.StatModifiers)
            {
                // Clona la plantilla del elemento stat
                var instance = m_StatsModifierElement.CloneTree();

                // Accedemos a los campos de la plantilla
                var tagSelectorButton = instance.Q<Button>("tag-selector-button");
                var tagLabel = instance.Q<Label>("stat-modifier-tag");
                var removeStatModifierButton = instance.Q<Button>("remove-stat-modifier-button");
                var statModifierValueField = instance.Q<FloatField>("stat-modifier-value-field");
                var operationEnum = instance.Q<EnumField>("stat-modifier-operation-enum");
                var valueTypeEnum = instance.Q<EnumField>("stat-modifier-value-type-field");
                var valueToEnum = instance.Q<EnumField>("stat-modifier-valueto-enum");
                var baseOnEnum = instance.Q<EnumField>("stat-modifier-baseon-enum");
                var baseOnContainer = instance.Q<VisualElement>("baseon-container");

                // Mostramos la informacion
                tagLabel.text = !String.IsNullOrEmpty(statModifier.statType) ? statModifier.statType : "character stat";
                statModifierValueField.value = statModifier.startValue > 0 ? statModifier.startValue : 0;
                operationEnum.value = statModifier.opType;
                valueTypeEnum.value = statModifier.valueType;
                valueToEnum.value = statModifier.valueTo;
                baseOnEnum.value = statModifier.baseOn;
                baseOnContainer.style.display = statModifier.valueType == UltimateFramework.Utils.ValueType.Percentage ? DisplayStyle.Flex : DisplayStyle.None;

                // Agregamos manejadores de eventos a los campos para actualizar la información del ítem cuando cambien
                statModifierValueField.RegisterValueChangedCallback((evt) =>
                {
                    statModifier.startValue = evt.newValue;
                    statModifierValueField.value = statModifier.startValue;
                    this.hasUnsavedChanges = true;
                });

                operationEnum.RegisterValueChangedCallback((evt) =>
                {
                    statModifier.opType = (OperationType)evt.newValue;
                    this.hasUnsavedChanges = true;
                });

                valueTypeEnum.RegisterValueChangedCallback((evt) =>
                {
                    statModifier.valueType = (UltimateFramework.Utils.ValueType)evt.newValue;

                    if ((UltimateFramework.Utils.ValueType)evt.newValue == UltimateFramework.Utils.ValueType.Percentage)
                    {
                        baseOnContainer.style.display = DisplayStyle.Flex;
                    }
                    else baseOnContainer.style.display = DisplayStyle.None;

                    this.hasUnsavedChanges = true;
                });

                valueToEnum.RegisterValueChangedCallback((evt) =>
                {
                    statModifier.valueTo = (UltimateFramework.Utils.ValueTo)evt.newValue;
                    this.hasUnsavedChanges = true;
                });

                baseOnEnum.RegisterValueChangedCallback((evt) =>
                {
                    statModifier.baseOn = (UltimateFramework.Utils.BaseOn)evt.newValue;
                    this.hasUnsavedChanges = true;
                });

                removeStatModifierButton.name = item.StatModifiers.IndexOf(statModifier).ToString();

                // Asignamos funcionalidad a los botones
                tagSelectorButton.clickable.clicked += () => OpenTagSelectorWindow(tagLabel, statModifier, "statType");
                removeStatModifierButton.clickable.clicked += () => RemoveStatModifierOnItem(removeStatModifierButton.name, item, currentRightPanelElement, instance);

                // Agregamos la plantilla al contenedor
                container.Add(instance);
            }
        }
        #endregion

        #region Item Description
        var multilineTextField = itemRightPanelElement.Q<TextField>("custom-text-field__multiline");

        if (!string.IsNullOrEmpty(item.description))
        {
            multilineTextField.value = item.description;
        }
        else multilineTextField.value = "Item description";

        multilineTextField.RegisterValueChangedCallback((evt) =>
        {
            item.description = evt.newValue;
            this.hasUnsavedChanges = true;
        });
        #endregion
    }

    // Stats
    void AddStatToItem(Item item, VisualElement rightPanelElement)
    {
        var instance = m_StatsElement.CloneTree();
        ItemStat newStat = new();

        item.Stats.Add(newStat);
        newStat.Index = item.Stats.IndexOf(newStat);

        var container = rightPanelElement.Q<ScrollView>("stats-scrollview");
        container.Add(instance);

        var tagSelectorButton = instance.Q<Button>("tag-selector-button");
        var tagLabel = instance.Q<Label>("stat-tag");
        tagSelectorButton.clickable.clicked += () => OpenTagSelectorWindow(tagLabel, newStat, "statTag");

        var statValueField = instance.Q<FloatField>("stat-value-field");
        statValueField.RegisterValueChangedCallback((evt) =>
        {
            newStat.startValue = evt.newValue;
            this.hasUnsavedChanges = true;
        });

        var removeStatButton = instance.Q<Button>("remove-stat-button");
        removeStatButton.name = item.Stats.IndexOf(newStat).ToString();
        removeStatButton.clickable.clicked += () => RemoveStatOnItem(removeStatButton.name, item, currentRightPanelElement, instance);

        this.hasUnsavedChanges = true;
        Debug.Log($"Stat added, total stat = {item.Stats.Count}");
    }
    private void RemoveStatOnItem(string buttonName, Item item, VisualElement rightPanelElement, TemplateContainer statElement)
    {
        // Convertir el nombre del botón a un identificador único
        int id = int.Parse(buttonName);

        // Obtener el item correspondiente al identificador
        var stat = item.Stats.Find(i => i.Index == id);

        item.Stats.Remove(stat);

        var container = rightPanelElement.Q<ScrollView>("stats-scrollview");
        container.Remove(statElement);

        this.hasUnsavedChanges = true;
        Debug.Log($"Stat removed = {stat}, at index = {stat.Index}, total stat = {item.Stats.Count}");
    }

    // Upgrades
    void AddUpgradeToItem(Item item, VisualElement rightPanelElement)
    {
        var instance = m_UpgradeElement.CloneTree();
        ItemUpgrade newUpgrade = new();

        item.Upgrades.Add(newUpgrade);
        newUpgrade.index = item.Upgrades.IndexOf(newUpgrade);

        var container = rightPanelElement.Q<ScrollView>("upgrades-scrollview");
        container.Add(instance);

        // Accedemos a los campos de la plantilla
        var upgradeNameField = instance.Q<TextField>("upgrade-name-field");
        var removeUpgradeButton = instance.Q<Button>("remove-upgrade-button");

        var upgradeStatToggle = instance.Q<ToolbarToggle>("upgrade-stat-toggle");
        var upgradeStatContent = instance.Q<VisualElement>("upgrade-stat-content");
        var upgradeValueField = instance.Q<FloatField>("upgrade-value-field");
        var operationEnum = instance.Q<EnumField>("upgrade-operation-enum");
        var valueTypeEnum = instance.Q<EnumField>("upgrade-value-type-field");
        var baseOnEnum = instance.Q<EnumField>("upgrade-baseon-enum");
        var tagSelectorButton = instance.Q<Button>("tag-selector-button");
        var tagLabel = instance.Q<Label>("upgrade-tag");

        var upgradeScaleToggle = instance.Q<ToolbarToggle>("upgrade-scale-toggle");
        var upgradeScaleContent = instance.Q<VisualElement>("upgrade-scale-content");
        var upgradeScaleIndex = instance.Q<IntegerField>("scale-index-field");
        var upgradeNewScale = instance.Q<EnumField>("new-scale-enum");

        var upgradeAttModToggle = instance.Q<ToolbarToggle>("upgrade-attribute-modifier-toggle");
        var upgradeAttModContent = instance.Q<VisualElement>("upgrade-attribute-modifier-content");
        var upgradeAttModIndex = instance.Q<IntegerField>("attribute-modifier-index-field");
        var upgradeAttModValue = instance.Q<FloatField>("attribute-modifier-newvalue-field");

        var upgradeStatModToggle = instance.Q<ToolbarToggle>("upgrade-stat-modifier-toggle");
        var upgradeStatModContent = instance.Q<VisualElement>("upgrade-stat-modifier-content");
        var upgradeStatModIndex = instance.Q<IntegerField>("stat-modifier-index-field");
        var upgradeStattModValue = instance.Q<FloatField>("stat-modifier-newvalue-field");

        #region Stat Upgrade

        // Agregamos manejadores de eventos a los campos para actualizar la información del ítem cuando cambien
        upgradeStatToggle.RegisterValueChangedCallback((evt) =>
        {
            newUpgrade.useStatUpgrade = evt.newValue;
            SetElementDisplay(evt.newValue, upgradeStatContent);
            this.hasUnsavedChanges = true;
        });

        upgradeNameField.RegisterValueChangedCallback((evt) =>
        {
            newUpgrade.name = evt.newValue;
            upgradeNameField.value = newUpgrade.name;
            this.hasUnsavedChanges = true;
        });

        upgradeValueField.RegisterValueChangedCallback((evt) =>
        {
            newUpgrade.value = evt.newValue;
            upgradeValueField.value = newUpgrade.value;
            this.hasUnsavedChanges = true;
        });

        tagSelectorButton.clickable.clicked += () => OpenTagSelectorWindow(tagLabel, newUpgrade, "statTag");

        operationEnum.RegisterValueChangedCallback((evt) =>
        {
            newUpgrade.opType = (OperationType)evt.newValue;
            this.hasUnsavedChanges = true;
        });

        valueTypeEnum.RegisterValueChangedCallback((evt) =>
        {
            newUpgrade.valueType = (UltimateFramework.Utils.ValueType)evt.newValue;
            this.hasUnsavedChanges = true;
        });

        baseOnEnum.RegisterValueChangedCallback((evt) =>
        {
            newUpgrade.baseOn = (BaseOn)evt.newValue;
            this.hasUnsavedChanges = true;
        });
        #endregion

        #region Scale Upgrade

        // Agregamos manejadores de eventos a los campos para actualizar la información del ítem cuando cambien
        upgradeScaleToggle.RegisterValueChangedCallback((evt) =>
        {
            newUpgrade.useScaleUpgrade = evt.newValue;
            SetElementDisplay(evt.newValue, upgradeScaleContent);
            this.hasUnsavedChanges = true;
        });

        upgradeScaleIndex.RegisterValueChangedCallback((evt) =>
        {
            newUpgrade.scaleToAffectIndex = evt.newValue;
            this.hasUnsavedChanges = true;
        });

        upgradeNewScale.RegisterValueChangedCallback((evt) =>
        {
            newUpgrade.newScaleLevel = (ScalingLevel)evt.newValue;
            this.hasUnsavedChanges = true;
        });
        #endregion

        #region Att Mod Upgrade

        // Agregamos manejadores de eventos a los campos para actualizar la información del ítem cuando cambien
        upgradeAttModToggle.RegisterValueChangedCallback((evt) =>
        {
            newUpgrade.useAttModUpgrade = evt.newValue;
            SetElementDisplay(evt.newValue, upgradeAttModContent);
            this.hasUnsavedChanges = true;
        });

        upgradeAttModIndex.RegisterValueChangedCallback((evt) =>
        {
            newUpgrade.attModToAffectIndex = evt.newValue;
            this.hasUnsavedChanges = true;
        });

        upgradeAttModValue.RegisterValueChangedCallback((evt) =>
        {
            newUpgrade.attModNewValue = evt.newValue;
            this.hasUnsavedChanges = true;
        });
        #endregion

        #region Stat Mod Upgrade

        // Agregamos manejadores de eventos a los campos para actualizar la información del ítem cuando cambien
        upgradeStatModToggle.RegisterValueChangedCallback((evt) =>
        {
            newUpgrade.useStatModUpgrade = evt.newValue;
            SetElementDisplay(evt.newValue, upgradeStatModContent);
            this.hasUnsavedChanges = true;
        });

        upgradeStatModIndex.RegisterValueChangedCallback((evt) =>
        {
            newUpgrade.statToAffectIndex = evt.newValue;
            this.hasUnsavedChanges = true;
        });

        upgradeStattModValue.RegisterValueChangedCallback((evt) =>
        {
            newUpgrade.statModNewValue = evt.newValue;
            this.hasUnsavedChanges = true;
        });
        #endregion

        var removeStatButton = instance.Q<Button>("remove-upgrade-button");
        removeStatButton.name = item.Upgrades.IndexOf(newUpgrade).ToString();
        removeStatButton.clickable.clicked += () => RemoveUpgradeOnItem(removeStatButton.name, item, currentRightPanelElement, instance);

        this.hasUnsavedChanges = true;
        Debug.Log($"Upgrade added, total upgrades = {item.Upgrades.Count}");
    }
    private void RemoveUpgradeOnItem(string buttonName, Item item, VisualElement rightPanelElement, TemplateContainer statElement)
    {
        // Convertir el nombre del botón a un identificador único
        int id = int.Parse(buttonName);

        // Obtener el item correspondiente al identificador
        var upgrade = item.Upgrades.Find(i => i.index == id);

        item.Upgrades.Remove(upgrade);

        var container = rightPanelElement.Q<ScrollView>("upgrades-scrollview");
        container.Remove(statElement);

        this.hasUnsavedChanges = true;
        Debug.Log($"Upgrade removed = {upgrade}, at index = {upgrade.index}, total upgrades = {item.Upgrades.Count}");
    }

    // Requirements
    void AddRequirementToItem(Item item, VisualElement rightPanelElement)
    {
        var instance = m_RequirementsElement.CloneTree();
        ItemRequirement newRequirement = new();

        item.Requirements.Add(newRequirement);
        newRequirement.Index = item.Requirements.IndexOf(newRequirement);

        var container = rightPanelElement.Q<ScrollView>("requirements-scrollview");
        container.Add(instance);

        var asUseItemContainer = instance.Q<VisualElement>("useItem-option-container");
        var asUpgradeContainer = instance.Q<VisualElement>("upgrade-option-container");
        DisplayRequirementContainer(newRequirement.requirementFor, asUseItemContainer, asUpgradeContainer);

        var useForEnum = instance.Q<EnumField>("upgrade-usefor-enum");
        useForEnum.RegisterValueChangedCallback((evt) =>
        {
            newRequirement.requirementFor = (RequirementFor)evt.newValue;
            useForEnum.value = newRequirement.requirementFor;
            DisplayRequirementContainer(newRequirement.requirementFor, asUseItemContainer, asUpgradeContainer);
            this.hasUnsavedChanges = true;
        });

        var tagSelectorButton = instance.Q<Button>("tag-selector-button");
        var tagLabel = instance.Q<Label>("attribute-tag");
        tagSelectorButton.clickable.clicked += () => OpenTagSelectorWindow(tagLabel, newRequirement, "attributeTag");

        var attributeValueField = instance.Q<FloatField>("attribute-value-field");
        attributeValueField.RegisterValueChangedCallback((evt) =>
        {
            newRequirement.value = evt.newValue;
            attributeValueField.value = newRequirement.value;
            this.hasUnsavedChanges = true;
        });

        var upgradeNameField = instance.Q<TextField>("upgrade-name-field");
        upgradeNameField.RegisterValueChangedCallback((evt) =>
        {
            newRequirement.upgradeName = evt.newValue;
            upgradeNameField.value = newRequirement.upgradeName;
            this.hasUnsavedChanges = true;
        });

        var itemNameField = instance.Q<TextField>("item-name-field");
        itemNameField.RegisterValueChangedCallback((evt) =>
        {
            newRequirement.itemName = evt.newValue;
            itemNameField.value = newRequirement.itemName;
            this.hasUnsavedChanges = true;
        });

        var itemAmmountField = instance.Q<IntegerField>("item-ammount-field");
        itemAmmountField.RegisterValueChangedCallback((evt) =>
        {
            newRequirement.itemAmmount = evt.newValue;
            itemAmmountField.value = newRequirement.itemAmmount;
            this.hasUnsavedChanges = true;
        });

        var removeRequirementButton = instance.Q<Button>("remove-requirement-button");
        removeRequirementButton.name = item.Requirements.IndexOf(newRequirement).ToString();
        removeRequirementButton.clickable.clicked += () => RemoveRequirementOnItem(removeRequirementButton.name, item, currentRightPanelElement, instance);

        this.hasUnsavedChanges = true;
        Debug.Log($"Requirement added, total requirements = {item.Requirements.Count}");
    }
    private void RemoveRequirementOnItem(string buttonName, Item item, VisualElement rightPanelElement, TemplateContainer statElement)
    {
        // Convertir el nombre del botón a un identificador único
        int id = int.Parse(buttonName);

        // Obtener el item correspondiente al identificador
        var requirement = item.Requirements.Find(i => i.Index == id);

        item.Requirements.Remove(requirement);

        var container = rightPanelElement.Q<ScrollView>("requirements-scrollview");
        container.Remove(statElement);

        this.hasUnsavedChanges = true;
        Debug.Log($"Requirement removed = {requirement}, at index = {requirement.Index}, total requirements = {item.Requirements.Count}");
    }

    // Scaling
    void AddScalingToItem(Item item, VisualElement rightPanelElement)
    {
        var instance = m_ScaledElement.CloneTree();
        ItemScale newScale = new();

        item.Scaled.Add(newScale);
        newScale.Index = item.Scaled.IndexOf(newScale);

        var container = rightPanelElement.Q<ScrollView>("stats-scaling-scrollview");
        container.Add(instance);

        var tagSelectorButton = instance.Q<Button>("tag-selector-button");
        var tagLabel = instance.Q<Label>("stat-tag");
        tagSelectorButton.clickable.clicked += () => OpenTagSelectorWindow(tagLabel, newScale, "attributeTag");

        var scaleLevelEnum = instance.Q<EnumField>("scaling-level-enum");
        scaleLevelEnum.RegisterValueChangedCallback((evt) =>
        {
            newScale.startScale = (ScalingLevel)evt.newValue;
            this.hasUnsavedChanges = true;
        });

        var scaledMathOP = instance.Q<ObjectField>("scale-math-op-object");
        scaledMathOP.RegisterValueChangedCallback((evt) =>
        {
            newScale.operation = (ScaleMathOperation)evt.newValue;
            this.hasUnsavedChanges = true;
        });

        var removeScaleButton = instance.Q<Button>("remove-scale-button");
        removeScaleButton.name = item.Scaled.IndexOf(newScale).ToString();
        removeScaleButton.clickable.clicked += () => RemoveScalingOnItem(removeScaleButton.name, item, currentRightPanelElement, instance);

        this.hasUnsavedChanges = true;
        Debug.Log($"Requirement added, total requirements = {item.Requirements.Count}");
    }
    private void RemoveScalingOnItem(string buttonName, Item item, VisualElement rightPanelElement, TemplateContainer statElement)
    {
        // Convertir el nombre del botón a un identificador único
        int id = int.Parse(buttonName);

        // Obtener el item correspondiente al identificador
        var scaled = item.Scaled.Find(i => i.Index == id);

        item.Scaled.Remove(scaled);

        var container = rightPanelElement.Q<ScrollView>("stats-scaling-scrollview");
        container.Remove(statElement);

        this.hasUnsavedChanges = true;
        Debug.Log($"Scaled removed = {scaled}, at index = {scaled.Index}, total scales = {item.Scaled.Count}");
    }

    // Atribute Modifiers
    void AddAttributeModifierToItem(Item item, VisualElement rightPanelElement)
    {
        var instance = m_AttributeModifierElement.CloneTree();
        ItemAttributeModifier newAttModifier = new();

        item.AttributeModifiers.Add(newAttModifier);
        newAttModifier.Index = item.AttributeModifiers.IndexOf(newAttModifier);

        var container = rightPanelElement.Q<ScrollView>("attribute-modifiers-scrollview");
        container.Add(instance);

        var upgradeValueField = instance.Q<FloatField>("attribute-modifier-value-field");
        upgradeValueField.RegisterValueChangedCallback((evt) =>
        {
            newAttModifier.startValue = evt.newValue;
            upgradeValueField.value = newAttModifier.startValue;
        });

        var tagSelectorButton = instance.Q<Button>("tag-selector-button");
        var tagLabel = instance.Q<Label>("attribute-modifier-tag");
        tagSelectorButton.clickable.clicked += () => OpenTagSelectorWindow(tagLabel, newAttModifier, "attributeType");

        var operationEnum = instance.Q<EnumField>("attribute-modifier-operation-enum");
        operationEnum.RegisterValueChangedCallback((evt) =>
        {
            newAttModifier.opType = (OperationType)evt.newValue;
        });

        var valueTypeEnum = instance.Q<EnumField>("attribute-modifier-value-type-field");
        valueTypeEnum.RegisterValueChangedCallback((evt) =>
        {
            newAttModifier.valueType = (UltimateFramework.Utils.ValueType)evt.newValue;
        });

        var removeStatButton = instance.Q<Button>("remove-attribute-modifier-button");
        removeStatButton.name = item.AttributeModifiers.IndexOf(newAttModifier).ToString();
        removeStatButton.clickable.clicked += () => RemoveAttributeModifierOnItem(removeStatButton.name, item, currentRightPanelElement, instance);

        this.hasUnsavedChanges = true;
        Debug.Log($"Attribute Modifier added, total attribute modifiers = {item.AttributeModifiers.Count}");
    }
    private void RemoveAttributeModifierOnItem(string buttonName, Item item, VisualElement rightPanelElement, TemplateContainer statElement)
    {
        // Convertir el nombre del botón a un identificador único
        int id = int.Parse(buttonName);

        // Obtener el item correspondiente al identificador
        var attModifier = item.AttributeModifiers.Find(i => i.Index == id);

        item.AttributeModifiers.Remove(attModifier);

        var container = rightPanelElement.Q<ScrollView>("attribute-modifiers-scrollview");
        container.Remove(statElement);

        this.hasUnsavedChanges = true;
        Debug.Log($"Attribute Modifier removed = {attModifier}, at index = {attModifier.Index}, total attribute modifiers = {item.AttributeModifiers.Count}");
    }

    // Stat Modifiers
    private void AddStatModifierToItem(Item item, TemplateContainer rightPanelElement)
    {
        var instance = m_StatsModifierElement.CloneTree();
        ItemStatModifier newStatModifier = new();

        item.StatModifiers.Add(newStatModifier);
        newStatModifier.Index = item.StatModifiers.IndexOf(newStatModifier);

        var container = rightPanelElement.Q<ScrollView>("stats-modifiers-scrollview");
        container.Add(instance);

        // Accedemos a los campos de la plantilla
        var tagSelectorButton = instance.Q<Button>("tag-selector-button");
        var tagLabel = instance.Q<Label>("stat-modifier-tag");
        var removeStatModifierButton = instance.Q<Button>("remove-stat-modifier-button");
        var statModifierValueField = instance.Q<FloatField>("stat-modifier-value-field");
        var operationEnum = instance.Q<EnumField>("stat-modifier-operation-enum");
        var valueTypeEnum = instance.Q<EnumField>("stat-modifier-value-type-field");
        var valueToEnum = instance.Q<EnumField>("stat-modifier-valueto-enum");
        var baseOnEnum = instance.Q<EnumField>("stat-modifier-baseon-enum");
        var baseOnContainer = instance.Q<VisualElement>("baseon-container");

        // Agregamos manejadores de eventos a los campos para actualizar la información del ítem cuando cambien
        statModifierValueField.RegisterValueChangedCallback((evt) =>
        {
            newStatModifier.startValue = evt.newValue;
            statModifierValueField.value = newStatModifier.startValue;
            this.hasUnsavedChanges = true;
        });

        operationEnum.RegisterValueChangedCallback((evt) =>
        {
            newStatModifier.opType = (OperationType)evt.newValue;
            this.hasUnsavedChanges = true;
        });

        valueTypeEnum.RegisterValueChangedCallback((evt) =>
        {
            newStatModifier.valueType = (UltimateFramework.Utils.ValueType)evt.newValue;

            if ((UltimateFramework.Utils.ValueType)evt.newValue == UltimateFramework.Utils.ValueType.Percentage)
            {
                baseOnContainer.style.display = DisplayStyle.Flex;
            }
            else baseOnContainer.style.display = DisplayStyle.None;

            this.hasUnsavedChanges = true;
        });

        valueToEnum.RegisterValueChangedCallback((evt) =>
        {
            newStatModifier.valueTo = (UltimateFramework.Utils.ValueTo)evt.newValue;
            this.hasUnsavedChanges = true;
        });

        baseOnEnum.RegisterValueChangedCallback((evt) =>
        {
            newStatModifier.baseOn = (UltimateFramework.Utils.BaseOn)evt.newValue;
            this.hasUnsavedChanges = true;
        });

        removeStatModifierButton.name = item.StatModifiers.IndexOf(newStatModifier).ToString();

        // Asignamos funcionalidad a los botones
        tagSelectorButton.clickable.clicked += () => OpenTagSelectorWindow(tagLabel, newStatModifier, "statType");
        removeStatModifierButton.clickable.clicked += () => RemoveStatModifierOnItem(removeStatModifierButton.name, item, currentRightPanelElement, instance);

        this.hasUnsavedChanges = true;
        Debug.Log($"Stat Modifier added, total attribute modifiers = {item.StatModifiers.Count}");
    }
    private void RemoveStatModifierOnItem(string buttonName, Item item, VisualElement rightPanelElement, TemplateContainer statElement)
    {
        // Convertir el nombre del botón a un identificador único
        int id = int.Parse(buttonName);

        // Obtener el item correspondiente al identificador
        var statModifier = item.StatModifiers.Find(i => i.Index == id);

        item.StatModifiers.Remove(statModifier);

        var container = rightPanelElement.Q<ScrollView>("stats-modifiers-scrollview");
        container.Remove(statElement);

        this.hasUnsavedChanges = true;
        Debug.Log($"Stat Modifier removed = {statModifier}, at index = {statModifier.Index}, total stat modifiers = {item.StatModifiers.Count}");
    }

    // Utilities
    private void OpenTagSelectorWindow(Label tagLabel, object obj, string fieldName)
    {
        if (currentTagSelectorWindow != null) currentTagSelectorWindow.Close();

        TagsTreeViewWindow window = EditorWindow.GetWindow<TagsTreeViewWindow>();
        Texture2D icon = Resources.Load<Texture2D>("Img/Tag_Window_Icon");
        window.titleContent = new GUIContent("Game Tags", icon);
        currentTagSelectorWindow = window;

        window.m_TagsTreeView.OnTagSelected += tag =>
        {
            // Utiliza reflexión para asignar el valor al campo específico
            if (obj != null && !string.IsNullOrEmpty(fieldName))
            {
                // Añade BindingFlags si el campo no es público
                var fieldInfo = obj.GetType().GetField(fieldName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (fieldInfo != null && fieldInfo.FieldType == typeof(string))
                {
                    // Asegúrate de que estás modificando el objeto original
                    fieldInfo.SetValue(obj, tag);
                }
            }

            tagLabel.text = tag;
        };
        window.Show();
    }
    private void SetElementDisplay(VisualElement container, VisualElement arrow)
    {
        if (container.style.display == DisplayStyle.None)
        {
            container.style.display = DisplayStyle.Flex;
            arrow.AddToClassList("chechmark-toggle-open");
        }
        else
        {
            container.style.display = DisplayStyle.None;
            arrow.RemoveFromClassList("chechmark-toggle-open");
        }
    }
    private void SetElementDisplay(bool value, VisualElement container)
    {
        if (value) container.style.display = DisplayStyle.Flex;
        else container.style.display = DisplayStyle.None;
    }
    private void SetElementDisplay(VisualElement root, string queryElement, string queryArrow, DisplayStyle displayStyle)
    {
        root.Q<VisualElement>(queryElement).style.display = displayStyle;
        var arrow = root.Q<VisualElement>(queryArrow);

        if (displayStyle != DisplayStyle.None)
        {
            arrow.AddToClassList("chechmark-toggle-open");
        }
        else
        {
            arrow.RemoveFromClassList("chechmark-toggle-open");
        }
    }
    private void SetArmatureSetDisplay(VisualElement container)
    {
        if (container.style.display == DisplayStyle.None)
        {
            container.style.display = DisplayStyle.Flex;
        }
        else
        {
            container.style.display = DisplayStyle.None;
        }
    }
    private void SetArrowAnim(VisualElement container, VisualElement arrow, string className)
    {
        if (container.style.display == DisplayStyle.None)
        {
            arrow.RemoveFromClassList(className);
        }
        else
        {
            arrow.AddToClassList(className);
        }
    }
    private void DisplayRequirementContainer(RequirementFor  useFor, VisualElement asUseItemContainer, VisualElement asUpgradeContainer)
    {
        if (useFor == RequirementFor.UseItem)
        {
            asUseItemContainer.style.display = DisplayStyle.Flex;
            asUpgradeContainer.style.display = DisplayStyle.None;
        }
        else if (useFor == RequirementFor.Upgrade)
        {
            asUseItemContainer.style.display = DisplayStyle.None;
            asUpgradeContainer.style.display = DisplayStyle.Flex;
        }
        else
        {
            asUseItemContainer.style.display = DisplayStyle.None;
            asUpgradeContainer.style.display = DisplayStyle.None;
        }
    }
    private void SetStartColorToSaveButton()
    {
        saveButton.style.backgroundColor = startColor;
        saveButton.style.color = Color.white;
    }
    private void SetEndColorToSaveButton()
    {
        saveButton.style.backgroundColor = endColor;
        saveButton.style.color = Color.black;
    }

    // Saving
    private void SaveData()
    {
        itemDB.SaveDatabase();
        this.hasUnsavedChanges = false;
    }
}