using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine;

namespace UltimateFramework
{
    public class SelectableElement : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        public UnityEvent onSelect;
        public UnityEvent onDeselect;

        public void OnSelect(BaseEventData eventData)
        {
            onSelect?.Invoke();
        }
        public void OnDeselect(BaseEventData eventData)
        {
            onDeselect?.Invoke();
        }
    }
}
