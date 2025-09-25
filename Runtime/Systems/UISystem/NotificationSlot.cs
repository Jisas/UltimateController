using UnityEngine;

namespace UltimateFramework.UISystem
{
    public class NotificationSlot : MonoBehaviour
    {
        public bool isEmpty;

        private void Start()
        {
            UpdateSlot();
        }

        public void UpdateSlot(bool? value = null)
        {
            if (!value.HasValue)
                isEmpty = transform.childCount == 0;

            else isEmpty = value.Value;
        }
    }
}
