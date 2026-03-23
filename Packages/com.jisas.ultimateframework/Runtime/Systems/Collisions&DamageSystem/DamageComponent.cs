using UltimateFramework.StatisticsSystem;
using UltimateFramework.InventorySystem;
using UltimateFramework.Inputs;
using UnityEngine;

namespace UltimateFramework.CollisionsAndDamageSystem
{
    public abstract class DamageComponent : MonoBehaviour
    {       
        protected StatisticsComponent m_StatsAnAttributes;
        protected EntityActionInputs m_EntityActionInputs;
        protected InventoryAndEquipmentComponent m_InventoryAndEquipment;

        public void SetAllowCollisions(bool value)
        {
            var mainWeaponDamageHandler = m_InventoryAndEquipment.GetCurrentMainWeapon().WeaponObject.GetComponent<WeaponDamageHandler>();
            var offHandWeapon = m_InventoryAndEquipment.GetCurrentLeftWeaponObject();
            var offHandWeaponDamageHandler = offHandWeapon != null ? offHandWeapon.GetComponent<WeaponDamageHandler>() : null;

            mainWeaponDamageHandler.AllowCollisions = value;
            if(offHandWeaponDamageHandler != null) offHandWeaponDamageHandler.AllowCollisions = value;
        }
    }
}
