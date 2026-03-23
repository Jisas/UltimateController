using UnityEngine;
using System;

namespace UltimateFramework.AnimatorDataSystem
{
    [Serializable]
    public class StateInfo
    {
        public string FullName { get; set; }
        public string StateName { get; set; }
        public StateMachineBehaviour[] Behaviours { get; set; }
    }
}
