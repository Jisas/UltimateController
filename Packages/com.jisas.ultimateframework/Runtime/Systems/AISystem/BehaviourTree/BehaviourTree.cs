using UnityEngine;

namespace UltimateFramework.AI.BehaviourTree
{
    public abstract class BehaviourTree : MonoBehaviour
    {
        private Node root = null;

        void Start()
        {
            root = SetupTree();
        }

        void Update()
        {
            root?.Evaluate();
        }

        protected abstract Node SetupTree();
    }
}