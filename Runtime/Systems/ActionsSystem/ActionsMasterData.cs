using UltimateFramework.ActionsSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "Ultimate Framework/Systems/Actions/Actions Master Data")]
public class ActionsMasterData : ScriptableObject
{
    public List<ActionsGroup> actionGroups;

#if UNITY_EDITOR
    public void Save()
    {
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();

        foreach (var actionGroup in actionGroups)
        {
            actionGroup.Save();
        }

        Debug.Log("Datos Guardados");
    }
#endif
}
