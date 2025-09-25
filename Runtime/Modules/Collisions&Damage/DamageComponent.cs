using UltimateFramework.StatisticsSystem;
using UltimateFramework.InventorySystem;
using UltimateFramework.Inputs;
using UltimateFramework.Utils;
using UnityEngine;

namespace UltimateFramework.CollisionsAndDamageSystem
{
    [AbstractClassName("DamageComponent")]
    public abstract class DamageComponent : MonoBehaviour, IUFComponent
    {
        protected InventoryAndEquipmentComponent m_InventoryAndEquipment;
        protected StatisticsComponent m_StatsAndAttributes;
        protected EntityActionInputs m_EntityActionInputs;

        public string ClassName { get; private set; }
        public EntityState State { get; set; }
        public DamageComponent()
        {
            ClassName = GetAbstractClassName() ?? this.GetType().Name;
        }

        private string GetAbstractClassName()
        {
            var type = this.GetType();

            while (type != null)
            {
                var attribute = (AbstractClassNameAttribute)System.Attribute.GetCustomAttribute(type, typeof(AbstractClassNameAttribute));
                if (attribute != null) return attribute.Name;
                type = type.BaseType;
            }

            return null;
        }

        public void SetAllowCollisions(bool value)
        {
            var mainWeaponDamageHandler = m_InventoryAndEquipment.GetCurrentMainWeapon().WeaponObject.GetComponent<WeaponDamageHandler>();
            var offHandWeapon = m_InventoryAndEquipment.GetCurrentLeftWeaponObject();

            var offHandWeaponDamageHandler = offHandWeapon != null ? offHandWeapon.GetComponent<WeaponDamageHandler>() : null;

            mainWeaponDamageHandler.AllowCollisions = value;
            if (offHandWeaponDamageHandler != null) offHandWeaponDamageHandler.AllowCollisions = value;
        }
    }
}
