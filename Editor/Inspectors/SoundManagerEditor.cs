using UltimateFramework.SoundSystem;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SoundManager))]
public class SoundManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Get the target object
        SoundManager generator = (SoundManager)target;
        GUILayout.Space(5);

        // Add a button to generate animator data
        if (GUILayout.Button("Update Data"))
        {
            generator.UpdateData();
        }
    }
}
