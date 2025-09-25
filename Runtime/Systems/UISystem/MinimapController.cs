using UnityEngine;

namespace UltimateFramework.UISystem
{
    public class MinimapController : MonoBehaviour
    {
        public Transform player;

        private void LateUpdate()
        {
            Vector3 newPos = player.position;
            newPos.y = transform.position.y;
            transform.position = newPos;
        }
    }
}