#if UNITY_EDITOR
using UltimateFramework.LocomotionSystem;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEditor;
using UnityEngine;
using UltimateFramework.AnimatorDataSystem;

[ExecuteInEditMode]
public class AnimatorDataGenerator : MonoBehaviour
{
    public Animator animator;
    public AnimatorData animatorData;
    private AnimatorOverrideController overrideAnimator;

    public void GenerateAnimatorData()
    {
        overrideAnimator = gameObject.GetComponent<AnimatorDataHandler>().OverrideAnimatorController;

        if (overrideAnimator != null)
        {
            AnimatorController ac = overrideAnimator.runtimeAnimatorController as AnimatorController;

            animatorData.Layers.Clear();
            for (int i = 0; i < ac.layers.Length; i++)
            {
                AnimatorStateMachine sm = ac.layers[i].stateMachine;
                var layerData = new LayerData { LayerIndex = i, States = GetStates(animator, i, sm) };
                animatorData.Layers.Add(layerData);
            }

            EditorUtility.SetDirty(animatorData);
            AssetDatabase.SaveAssets();
            Debug.Log("Animator Data Generated");
        }
    }

    private List<StateInfo> GetStates(Animator animator, int layerIndex, AnimatorStateMachine sm, string path = "")
    {
        var states = new List<StateInfo>();

        foreach (ChildAnimatorState state in sm.states)
        {
            string fullPath = path + sm.name + "." + state.state.name;
            int fullPathHash = Animator.StringToHash(fullPath);
            StateMachineBehaviour[] stateBehaviours = animator.GetBehaviours(fullPathHash, layerIndex);

            states.Add(new StateInfo { FullName = fullPath, StateName = state.state.name, Behaviours = stateBehaviours });
        }

        foreach (ChildAnimatorStateMachine subSm in sm.stateMachines)
        {
            var subStates = GetStates(animator, layerIndex, subSm.stateMachine, path + sm.name + ".");
            states.AddRange(subStates);
        }

        return states;
    }
}
#endif

