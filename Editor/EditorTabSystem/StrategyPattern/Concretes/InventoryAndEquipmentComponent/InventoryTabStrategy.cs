using UltimateFramework.InventorySystem;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine;

namespace UltimateFramework.Editor.TabSystem.Strategies.InventoryAndEquipment
{
#if UNITY_EDITOR
    public class InventoryTabStrategy : TabStrategy
    {
        private bool callbacksRegistered = false;

        protected override void SetupTabContent(object target, VisualElement container)
        {
            var m_Target = target as InventoryAndEquipmentComponent;

            #region Find Elements
            var useInventorySwitch = UFEditorUtils.FindElementInRoot<Button>(container, "use-inventory-button");
            var inventorySettingsContent = UFEditorUtils.FindElementInRoot<VisualElement>(container, "inventory-settings");
            var capacity = UFEditorUtils.FindElementInRoot<IntegerField>(container, "capacity");
            var slotPrefab = UFEditorUtils.FindElementInRoot<ObjectField>(container, "slot-prefab");
            var slotsPanel = UFEditorUtils.FindElementInRoot<ObjectField>(container, "slots-panel");
            #endregion

            #region Value Asignament
            UFEditorUtils.SetSwitch(m_Target.useInventory, ref useInventorySwitch);
            UFEditorUtils.SetElementDisplay(m_Target.useInventory, ref inventorySettingsContent);
            capacity.value = m_Target.Capacity;
            slotPrefab.value = m_Target.slotPrefab;
            slotsPanel.value = m_Target.slotsPanel;
            #endregion

            #region Register Callbacks
            if (!callbacksRegistered)
            {
                useInventorySwitch.clickable.clicked += () =>
                {
                    m_Target.useInventory = !m_Target.useInventory;
                    UFEditorUtils.SetSwitch(m_Target.useInventory, ref useInventorySwitch);
                    UFEditorUtils.SetElementDisplay(m_Target.useInventory, ref inventorySettingsContent);
                    EditorUtility.SetDirty(m_Target);
                };

                callbacksRegistered = true;
            }

            capacity.RegisterValueChangedCallback(evt =>
            {
                m_Target.Capacity = evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });

            slotPrefab.RegisterValueChangedCallback(evt =>
            {
                m_Target.slotPrefab = (GameObject)evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });

            slotsPanel.RegisterValueChangedCallback(evt =>
            {
                m_Target.slotsPanel = (Transform)evt.newValue;
                EditorUtility.SetDirty(m_Target);
            });
            #endregion
        }
    }
#endif
}
