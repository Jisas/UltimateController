using System;

namespace UltimateController.ActionsSystem
{
    [Serializable]
    public class ActionsGroupStructure
    {
        public ActionsGroup actionsGroup;
        public TagSelector movesetAction = new("None");
    }
}
