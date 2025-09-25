using UltimateFramework.InventorySystem;
using System.Collections.Generic;
using UltimateFramework.Utils;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;

namespace UltimateFramework.Editor.TabSystem.Strategies.InventoryAndEquipment
{
#if UNITY_EDITOR
    public class EquipmentTabStrategy : TabStrategy
    {
        #region Private Fields
        private VisualTreeAsset StartingItemElement;
        private VisualTreeAsset EquipSlotDataElement;
        private VisualTreeAsset TagSelectorListElement;
        private Label itemsListCount, slotsListCount;

        private bool callbacksRegistered = false;
        private bool showStartingItemListBody, 
                     showSlotDataListBody, 
                     showTagSelectorListBody,
                     showSlotInfoBody;

        private readonly List<TemplateContainer> slotsData = new();
        #endregion

        protected override void SetupTabContent(object target, VisualElement container)
        {
            var m_Target = target as InventoryAndEquipmentComponent;

            #region Find Elements
            // Templates
            StartingItemElement = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Components/Templates/StartingItemElement");
            EquipSlotDataElement = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Components/Templates/EquipSlotDataElement");
            TagSelectorListElement = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Components/Templates/TagSelectorListElement");

            // Sections Base
            var enableSettion = UFEditorUtils.FindElementInRoot<VisualElement>(container, "enable-setting");
            var bonesSection = UFEditorUtils.FindElementInRoot<VisualElement>(container, "bones-settings");
            var startingItemsSection = UFEditorUtils.FindElementInRoot<VisualElement>(container, "starting-items");
            var slotsDataSection = UFEditorUtils.FindElementInRoot<VisualElement>(container, "slots-settings");

            // Enable Section Elements
            var useEquipmentUISwitch = UFEditorUtils.FindElementInRoot<Button>(enableSettion, "use-equipment-ui-button");
            var useFastAccessUISwitch = UFEditorUtils.FindElementInRoot<Button>(enableSettion, "use-fast-access-ui-button");

            // Bones Section Elements
            var bodyBone = UFEditorUtils.FindElementInRoot<ObjectField>(bonesSection, "body-bone");
            var leftHandBone = UFEditorUtils.FindElementInRoot<ObjectField>(bonesSection, "left-hand-bone");
            var rightHandBone = UFEditorUtils.FindElementInRoot<ObjectField>(bonesSection, "right-hand-bone");

            // Startin Items Section Elements
            var itemslistSelectableHeader = UFEditorUtils.FindElementInRoot<VisualElement>(startingItemsSection, "list-opener");
            var itemsListArrow = UFEditorUtils.FindElementInRoot<VisualElement>(itemslistSelectableHeader, "arrow");
            itemsListCount = UFEditorUtils.FindElementInRoot<Label>(itemslistSelectableHeader, "count");
            var addStartingitemButton = UFEditorUtils.FindElementInRoot<Button>(startingItemsSection, "add-starting-item");
            var itemlistBodyContainer = UFEditorUtils.FindElementInRoot<VisualElement>(startingItemsSection, "List-Body");

            // Slots Data Elements
            var slotslistSelectableHeader = UFEditorUtils.FindElementInRoot<VisualElement>(slotsDataSection, "list-opener");
            var slotsListArrow = UFEditorUtils.FindElementInRoot<VisualElement>(slotslistSelectableHeader, "arrow");
            slotsListCount = UFEditorUtils.FindElementInRoot<Label>(slotslistSelectableHeader, "count");
            var addSlotDataButton = UFEditorUtils.FindElementInRoot<Button>(slotsDataSection, "add-slot-data");
            var slotslistBodyContainer = UFEditorUtils.FindElementInRoot<VisualElement>(slotsDataSection, "List-Body");
            #endregion

            #region Value Asignament
            UFEditorUtils.SetSwitch(m_Target.useEquipmentUI, ref useEquipmentUISwitch);
            UFEditorUtils.SetSwitch(m_Target.useFastAccessUI, ref useFastAccessUISwitch);
            bodyBone.value = m_Target.bodyBone;
            leftHandBone.value = m_Target.leftHandBone;
            rightHandBone.value = m_Target.rightHandBone;
            itemsListCount.text = m_Target.StartingItems.Count > 0 ? $"{m_Target.StartingItems.Count} elements" : "0";
            slotsListCount.text = m_Target.EquipSlotsData.Count > 0 ? $"{m_Target.EquipSlotsData.Count} elements" : "0";
            #endregion

            #region Register Callbacks
            bodyBone.RegisterValueChangedCallback(evt =>
            {
                m_Target.bodyBone = (Transform)evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });

            leftHandBone.RegisterValueChangedCallback(evt =>
            {
                m_Target.leftHandBone = (Transform)evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });

            rightHandBone.RegisterValueChangedCallback(evt =>
            {
                m_Target.rightHandBone = (Transform)evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });

            if (!callbacksRegistered)
            {
                foreach (var startingItem in m_Target.StartingItems)
                    AddStartingItem(itemlistBodyContainer, startingItem, m_Target);

                foreach (var equipSlotData in m_Target.EquipSlotsData)
                    AddSlotData(slotslistBodyContainer, equipSlotData, m_Target);

                useEquipmentUISwitch.clickable.clicked += () =>
                {
                    m_Target.useEquipmentUI = !m_Target.useEquipmentUI;
                    UFEditorUtils.SetSwitch(m_Target.useEquipmentUI, ref useEquipmentUISwitch);
                    EditorUtility.SetDirty(m_Target);
                };

                useFastAccessUISwitch.clickable.clicked += () =>
                {
                    m_Target.useFastAccessUI = !m_Target.useFastAccessUI;
                    UFEditorUtils.SetSwitch(m_Target.useFastAccessUI, ref useFastAccessUISwitch);
                    EditorUtility.SetDirty(m_Target);
                };

                itemslistSelectableHeader.RegisterCallback<ClickEvent>(evt =>
                {
                    showStartingItemListBody = !showStartingItemListBody;
                    UFEditorUtils.SetElementDisplay(showStartingItemListBody, ref itemlistBodyContainer);
                    UFEditorUtils.SetArrowAnim(itemlistBodyContainer, ref itemsListArrow, "arrowmark-toggle-open");
                });

                addStartingitemButton.clickable.clicked += () =>
                {
                    var newItem = new StartingItems();
                    m_Target.StartingItems.Add(newItem);
                    AddStartingItem(itemlistBodyContainer, newItem, m_Target);
                    itemsListCount.text = $"{m_Target.StartingItems.Count} elements";
                    EditorUtility.SetDirty(m_Target);
                };

                slotslistSelectableHeader.RegisterCallback<ClickEvent>(evt =>
                {
                    showSlotDataListBody = !showSlotDataListBody;
                    UFEditorUtils.SetElementDisplay(showSlotDataListBody, ref slotslistBodyContainer);
                    UFEditorUtils.SetArrowAnim(slotslistBodyContainer, ref slotsListArrow, "arrowmark-toggle-open");
                });

                addSlotDataButton.clickable.clicked += () =>
                {
                    var newSlotData = new EquipSlotData();
                    m_Target.EquipSlotsData.Add(newSlotData);
                    AddSlotData(slotslistBodyContainer, newSlotData, m_Target);
                    slotsListCount.text = $"{m_Target.EquipSlotsData.Count} elements";
                    EditorUtility.SetDirty(m_Target);
                };

                callbacksRegistered = true;
            }
            #endregion
        }
        public override void UpdateTabContent(object target, VisualElement container)
        {
            var m_Target = target as InventoryAndEquipmentComponent;

            foreach (var slot in slotsData)
                UpdateSlotData(slot, m_Target.EquipSlotsData[slotsData.IndexOf(slot)], m_Target);
        }

        private void AddStartingItem(VisualElement container, StartingItems item, InventoryAndEquipmentComponent target)
        {
            var instance = StartingItemElement.CloneTree();

            #region Find Elements
            var autoEquip = UFEditorUtils.FindElementInRoot<Button>(instance, "auto-equip-button");
            var equipOnBody = UFEditorUtils.FindElementInRoot<Button>(instance, "equip-on-body-button");
            var itemName = UFEditorUtils.FindElementInRoot<TextField>(instance, "item-name");
            var itemAmount = UFEditorUtils.FindElementInRoot<IntegerField>(instance, "item-amount");
            var dropPercentage = UFEditorUtils.FindElementInRoot<Slider>(instance, "drop-percentage");
            var removeButton = UFEditorUtils.FindElementInRoot<Button>(instance, "remove-button");
            #endregion

            #region Value Asignament
            UFEditorUtils.SwitchButonColor(autoEquip, item.autoEquip);
            equipOnBody.style.display = item.autoEquip ? DisplayStyle.Flex : DisplayStyle.None;
            UFEditorUtils.SwitchButonColor(equipOnBody, item.equipOnBody);
            itemName.value = item.itemName;
            itemAmount.value = item.amount;
            dropPercentage.value = item.dropChancePercentage;
            #endregion

            #region Register Callbacks
            autoEquip.clickable.clicked += () =>
            {
                item.autoEquip = !item.autoEquip;
                UFEditorUtils.SwitchButonColor(autoEquip, item.autoEquip);
                equipOnBody.style.display = item.autoEquip ? DisplayStyle.Flex : DisplayStyle.None;
                EditorUtility.SetDirty(target);
            };

            equipOnBody.clickable.clicked += () =>
            {
                item.equipOnBody = !item.equipOnBody;
                UFEditorUtils.SwitchButonColor(equipOnBody, item.equipOnBody);
                EditorUtility.SetDirty(target);
            };

            itemName.RegisterValueChangedCallback(evt =>
            {
                item.itemName = evt.newValue;
                EditorUtility.SetDirty(target);
            });

            itemAmount.RegisterValueChangedCallback(evt =>
            {
                item.amount = evt.newValue;
                EditorUtility.SetDirty(target);
            });

            dropPercentage.RegisterValueChangedCallback(evt =>
            {
                item.dropChancePercentage = evt.newValue;
                EditorUtility.SetDirty(target);
            });

            removeButton.clickable.clicked += () =>
            {
                RemoveStartingItem(container, instance, target, item);
                itemsListCount.text = $"{target.StartingItems.Count} elements";
                EditorUtility.SetDirty(target);
            };
            #endregion

            container.Add(instance);
        }
        private void RemoveStartingItem(VisualElement container, TemplateContainer instance, InventoryAndEquipmentComponent target, StartingItems item)
        {
            target.StartingItems.Remove(item);
            container.Remove(instance);
        }

        private void AddSlotData(VisualElement container, EquipSlotData slotData, InventoryAndEquipmentComponent target)
        {
            var instance = EquipSlotDataElement.CloneTree();
            slotsData.Add(instance);

            #region Find Elements
            var selectSlotButton = UFEditorUtils.FindElementInRoot<Button>(instance, "select-slot-button");
            var checkMark = UFEditorUtils.FindElementInRoot<VisualElement>(instance, "checkmark");
            var useAmountTextButton = UFEditorUtils.FindElementInRoot<Button>(instance, "use-amount-text-button");
            var slotOrientation = UFEditorUtils.FindElementInRoot<EnumField>(instance, "slot-orientation");

            var slotObj = UFEditorUtils.FindElementInRoot<ObjectField>(instance, "slot-obj-field");

            var tagsListSelectableHeader = UFEditorUtils.FindElementInRoot<VisualElement>(instance, "list-opener");
            var tagsListArrow = UFEditorUtils.FindElementInRoot<VisualElement>(tagsListSelectableHeader, "arrow");
            var tagsListCount = UFEditorUtils.FindElementInRoot<Label>(tagsListSelectableHeader, "count");
            var addTagButton = UFEditorUtils.FindElementInRoot<Button>(instance, "add-tag-selector");
            var taglistBodyContainer = UFEditorUtils.FindElementInRoot<VisualElement>(instance, "List-Body");
            var removeButton = UFEditorUtils.FindElementInRoot<Button>(instance, "remove-button");

            var botSection = UFEditorUtils.FindElementInRoot<VisualElement>(instance, "bot-section");
            var slotInfoSelectableHeader = UFEditorUtils.FindElementInRoot<VisualElement>(botSection, "list-opener");
            var slotInfoArrow = UFEditorUtils.FindElementInRoot<VisualElement>(slotInfoSelectableHeader, "arrow");
            var slotInfoBody = UFEditorUtils.FindElementInRoot<VisualElement>(botSection, "List-Body");
            #endregion

            #region Value Asignament
            UFEditorUtils.SetElementDisplay(slotData.selected, ref checkMark);
            UFEditorUtils.SwitchButonColor(useAmountTextButton, slotData.useAmountText);
            slotObj.value = slotData.slotObject;
            slotOrientation.value = slotData.orientation;
            tagsListCount.text = $"{slotData.slotTags.Count} elements";

            foreach (var tagSelector in slotData.slotTags)
                AddTagSelector(taglistBodyContainer, tagSelector, slotData, tagsListCount, target);
            #endregion

            #region Register Callbacks
            selectSlotButton.RegisterCallback<ClickEvent>(evt =>
            {
                slotData.selected = !slotData.selected;
                UFEditorUtils.SetElementDisplay(slotData.selected, ref checkMark);
                EditorUtility.SetDirty(target);
            });

            useAmountTextButton.RegisterCallback<ClickEvent>(evt =>
            {
                slotData.useAmountText = !slotData.useAmountText;
                UFEditorUtils.SwitchButonColor(useAmountTextButton, slotData.useAmountText);
                EditorUtility.SetDirty(target);
            });

            slotOrientation.RegisterValueChangedCallback(evt =>
            {
                slotData.orientation = (SocketOrientation)evt.newValue;
                EditorUtility.SetDirty(target);
            });

            slotObj.RegisterValueChangedCallback(evt =>
            {
                slotData.slotObject = (GameObject)evt.newValue;
                EditorUtility.SetDirty(target);
            });

            tagsListSelectableHeader.RegisterCallback<ClickEvent>(ect =>
            {
                showTagSelectorListBody = !showTagSelectorListBody;
                UFEditorUtils.SetElementDisplay(showTagSelectorListBody, ref taglistBodyContainer);
                UFEditorUtils.SetArrowAnim(taglistBodyContainer, ref tagsListArrow, "arrowmark-toggle-open");
            });

            addTagButton.clickable.clicked += () =>
            {
                var newSelector = new TagSelector("");
                slotData.slotTags.Add(newSelector);
                AddTagSelector(taglistBodyContainer, newSelector, slotData, tagsListCount, target);
                EditorUtility.SetDirty(target);
            };

            removeButton.clickable.clicked += () =>
            {
                RemoveSlotData(container, instance, target, slotData);
                slotsListCount.text = $"{target.EquipSlotsData.Count} elements";
                EditorUtility.SetDirty(target);
            };

            slotInfoSelectableHeader.RegisterCallback<ClickEvent>(ect =>
            {
                showSlotInfoBody = !showSlotInfoBody;
                UFEditorUtils.SetElementDisplay(showSlotInfoBody, ref slotInfoBody);
                UFEditorUtils.SetArrowAnim(slotInfoBody, ref slotInfoArrow, "arrowmark-toggle-open");
            });
            #endregion

            container.Add(instance);
        }
        private void UpdateSlotData(TemplateContainer instance, EquipSlotData slotData, InventoryAndEquipmentComponent target)
        {
            var checkMark = UFEditorUtils.FindElementInRoot<VisualElement>(instance, "checkmark");

            int index = target.EquipSlotsData.IndexOf(slotData);
            var botSection = UFEditorUtils.FindElementInRoot<VisualElement>(instance, "bot-section");
            var slotInfoBody = UFEditorUtils.FindElementInRoot<VisualElement>(botSection, "List-Body");
            var isEmpty = UFEditorUtils.FindElementInRoot<Toggle>(slotInfoBody, "is-empty");
            var slotID = UFEditorUtils.FindElementInRoot<IntegerField>(slotInfoBody, "slot-id");
            var itemID = UFEditorUtils.FindElementInRoot<IntegerField>(slotInfoBody, "item-id");
            var amount = UFEditorUtils.FindElementInRoot<IntegerField>(slotInfoBody, "amount");
            var maxAmount = UFEditorUtils.FindElementInRoot<IntegerField>(slotInfoBody, "max-amount");

            UFEditorUtils.SetElementDisplay(target.EquipSlots[index].Selected, ref checkMark);
            isEmpty.value = target.EquipSlotsInfo[index].isEmpty;
            slotID.value = target.EquipSlotsInfo[index].id;
            itemID.value = target.EquipSlotsInfo[index].itemId;
            amount.value = target.EquipSlotsInfo[index].amount;
            maxAmount.value = target.EquipSlotsInfo[index].maxAmount;
        }
        private void RemoveSlotData(VisualElement container, TemplateContainer instance, InventoryAndEquipmentComponent target, EquipSlotData slotData)
        {
            target.EquipSlotsData.Remove(slotData);
            container.Remove(instance);
        }

        private void AddTagSelector(VisualElement container, TagSelector selector, EquipSlotData slotData, Label tagsListCount, InventoryAndEquipmentComponent target)
        {
            tagsListCount.text = $"{slotData.slotTags.Count} elements";
            var instance = TagSelectorListElement.CloneTree();

            #region Find Elements
            var tagSeletorButton = UFEditorUtils.FindElementInRoot<Button>(instance, "tag-selector-button");
            var tag = UFEditorUtils.FindElementInRoot<Label>(tagSeletorButton, "tag");
            var removeButton = UFEditorUtils.FindElementInRoot<Button>(instance, "remove-button");
            #endregion

            #region Value Asignament
            tag.text = !string.IsNullOrEmpty(selector.tag) ? selector.tag : "None";
            #endregion

            #region Register Callbacks
            tagSeletorButton.clickable.clicked += () =>
            {
                UFEditorUtils.OpenTagSelectorWindow(tag, selector, "tag");
                EditorUtility.SetDirty(target);
            };

            removeButton.clickable.clicked += () =>
            {
                RemoveTagSelector(container, instance, slotData, selector);
                tagsListCount.text = $"{slotData.slotTags.Count} elements";
                EditorUtility.SetDirty(target);
            };
            #endregion

            container.Add(instance);
        }
        private void RemoveTagSelector(VisualElement container, TemplateContainer instance, EquipSlotData slotData, TagSelector selector)
        {
            slotData.slotTags.Remove(selector);
            container.Remove(instance);
        }
    }
#endif
}
