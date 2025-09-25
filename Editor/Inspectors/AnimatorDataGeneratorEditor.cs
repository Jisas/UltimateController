using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AnimatorDataGenerator))]
public class AnimatorDataGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Get the target object
        AnimatorDataGenerator generator = (AnimatorDataGenerator)target;
        GUILayout.Space(5);

        // Add a button to generate animator data
        if (GUILayout.Button("Generate Data"))
        {
            generator.GenerateAnimatorData();
        }
    }
}

