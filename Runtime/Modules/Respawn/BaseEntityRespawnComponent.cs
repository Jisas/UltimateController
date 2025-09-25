using System.Collections;
using UnityEngine;
using System;

namespace UltimateFramework.RespawnSystem
{
    [AbstractClassName("BaseEntityRespawnComponent")]
    public abstract class BaseEntityRespawnComponent : MonoBehaviour, IUFComponent
    {
        #region Inheritance Config
        public string ClassName { get; private set; }
        public EntityState State { get; set; }
        public BaseEntityRespawnComponent()
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

        public Func<float> WaitTimeToRespawnCoroutine;
        private RespawnData currentRespawn;

        public RespawnData GetCurreRespawn() => currentRespawn;
        public void SetCurrentRespawn(Vector3 position, Quaternion rotation)
        {
            currentRespawn.position = position;
            currentRespawn.rotation = rotation;
        }
        public virtual IEnumerator Respawn() { yield return null; }
        public virtual void RespawnOn(Vector3 position, Quaternion rotation) { }
        public virtual void RespawnOn(Transform parent, Vector3 position, Quaternion rotation) { }
    }

    [Serializable]
    public struct RespawnData
    {
        public Vector3 position; 
        public Quaternion rotation;
    }
}