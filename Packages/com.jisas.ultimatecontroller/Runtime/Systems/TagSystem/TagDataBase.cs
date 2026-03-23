using System.Collections.Generic;
using UnityEngine;

namespace UltimateFramework.Tag
{
    [CreateAssetMenu(fileName = "TagData", menuName = "Ultimate Framework/Systems/Tags/Data/TagData", order = 1)]
    public class TagDataBase : ScriptableObject
    {
        public List<string> tags = new();
    
        public string FindTag(string tag)
        {
            foreach (var item in tags)
            {
                if (item.Contains(tag))
                    return tag;
            }
            return null;
        }
    }
}