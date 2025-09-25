using UnityEngine;

namespace UltimateFramework
{
    public abstract class UFBaseComponent : MonoBehaviour, IUFComponent
    {
        public string ClassName { get; private set; }
        protected UFBaseComponent()
        {
            ClassName = this.GetType().Name;
        }
    }
}
