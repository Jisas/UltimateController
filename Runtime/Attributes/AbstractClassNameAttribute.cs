using System;

namespace UltimateFramework
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class AbstractClassNameAttribute : Attribute
    {
        public string Name { get; }

        public AbstractClassNameAttribute(string name)
        {
            Name = name;
        }
    }
}
