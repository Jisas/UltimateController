using System;

namespace UltimateController.Attributes
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
