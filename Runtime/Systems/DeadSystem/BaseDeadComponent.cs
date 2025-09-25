using System.Collections;
using UnityEngine;
using System;

namespace UltimateFramework.DeadSystem
{
    [AbstractClassName("BaseDeadComponent")]
    public abstract class BaseDeadComponent : MonoBehaviour, IUFComponent
    {
        #region Inheritance Config
        public string ClassName { get; private set; }

        public BaseDeadComponent()
        {
            ClassName = GetAbstractClassName() ?? this.GetType().Name;
        }

        private string GetAbstractClassName()
        {
            var type = this.GetType();

            while (type != null)
            {
                var attribute = (AbstractClassNameAttribute)Attribute.GetCustomAttribute(type, typeof(AbstractClassNameAttribute));
                if (attribute != null) return attribute.Name;
                type = type.BaseType;
            }

            return null;
        }
        #endregion

        public Action<GameObject> OnDead;
        public abstract void StartDeadCoroutine();
    }
}