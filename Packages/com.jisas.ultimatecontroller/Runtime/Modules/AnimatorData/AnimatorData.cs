using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimatorData", menuName = "Ultimate Framework/Systems/Animation/AnimatorData")]
public class AnimatorData : ScriptableObject
{
    public List<LayerData> Layers = new();
}

[System.Serializable]
public class LayerData
{
    public int LayerIndex;
    public List<StateInfo> States = new();
}

[System.Serializable]
public class StateInfo
{
    public string FullName;
    public string StateName;
    public StateMachineBehaviour[] Behaviours;
}

