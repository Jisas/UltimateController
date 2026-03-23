using UnityEngine;

namespace UltimateFramework.InventorySystem
{
    public class EquippedWeapon
    {
        public Transform BodySocket { get; set; }
        public Transform HandSocket { get; set; }
        public GameObject WeaponObject { get; set; }
        public WeaponComponent WeaponComponent { get; set; }
    }
}