using System.Collections.Generic;
using UltimateFramework.Utils;

namespace UltimateFramework.AI.BehaviourTree
{
    public class Node
    {
        public NodeState state;
        public Node parent;
        protected List<Node> children = new();
        private readonly Dictionary<string, object> blackboard = new();

        public Node()
        {
            parent = null;
        }
        public Node(List<Node> children)
        {
            foreach (var child in children)
                Attach(child);
        }

        private void Attach(Node node)
        {
            node.parent = this;
            children.Add(node);
        }
        public virtual NodeState Evaluate() => NodeState.Failure;

        public void SetData(string key, object value)
        {
            blackboard[key] = value;
        }
        public object GetData(string key) 
        {
            if (blackboard.TryGetValue(key, out object value))
                return value;

            Node node = parent;

            while (node != null)
            {
                value = node.GetData(key);
                if (value != null) return value;
                node = node.parent;
            }

            return null;
        }
        public T? GetData<T>(string key) where T : struct
        {
            if (blackboard.TryGetValue(key, out object value))
            {
                if (value is T typedValue)
                {
                    return typedValue;
                }
            }

            Node node = parent;

            while (node != null)
            {
                T? parentValue = node.GetData<T>(key);
                if (parentValue.HasValue)
                {
                    return parentValue;
                }
                node = node.parent;
            }

            return null;
        }
        public bool ClearData(string key)
        {
            if (blackboard.ContainsKey(key))
            {
                blackboard.Remove(key);
                return true;
            }

            Node node = parent;

            while (node != null)
            {
                bool cleared = node.ClearData(key);
                if (cleared) return true;
                node = node.parent;
            }

            return false;
        }
    }
}
