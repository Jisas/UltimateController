using UnityEngine.UIElements;

public class EquipmentSlotElement : VisualElement
{
    public new class UxmlFactory : UxmlFactory<EquipmentSlotElement, UxmlTraits> { }

    // Add the custom UXML attributes.
    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        UxmlIntAttributeDescription m_Int =
            new UxmlIntAttributeDescription { name = "slot-index", defaultValue = 0 };

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            var ate = ve as EquipmentSlotElement;

            ate.SlotIndex = m_Int.GetValueFromBag(bag, cc);
        }
    }

    public int SlotIndex { get; set; }
}
