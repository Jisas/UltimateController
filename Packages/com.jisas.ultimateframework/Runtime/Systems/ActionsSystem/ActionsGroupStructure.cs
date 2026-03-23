using System;

namespace UltimateFramework.ActionsSystem
{
    [Serializable]
    public class ActionsGroupStructure
    {
        public ActionsGroup actionsGroup;
        public TagSelector movesetAction = new("None");
    }
}
