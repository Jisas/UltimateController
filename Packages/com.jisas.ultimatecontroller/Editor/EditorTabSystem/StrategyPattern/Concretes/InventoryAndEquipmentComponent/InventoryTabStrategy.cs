using UltimateFramework.InventorySystem;
using UnityEngine.UIElements;
using UnityEditor;

namespace UltimateFramework.Editor.TabSystem.Strategies.InventoryAndEquipment
{
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
            #endregion

            #region Value Asignament
            UFEditorUtils.SetSwitch(m_Target.useInventory, ref useInventorySwitch);
            UFEditorUtils.SetElementDisplay(m_Target.useInventory, ref inventorySettingsContent);
            capacity.value = m_Target.Capacity;
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
            #endregion
        }
    }
}
