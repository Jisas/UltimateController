using UnityEngine;

namespace UltimateController
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
