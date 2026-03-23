using System.Collections.Generic;
using UnityEditor.Animations;
using System.Linq;
using UnityEngine;
using System;

namespace UltimateFramework.AnimatorDataSystem
{
    public static class AnimatorUtilities
    {
        public static void RegisterLayerStates(Animator animator, AnimatorOverrideController overrideAnimator, Dictionary<int, Dictionary<string, StateInfo>> layerStates)
        {
            if (overrideAnimator != null)
            {
                AnimatorController ac = overrideAnimator.runtimeAnimatorController as AnimatorController;

                for (int i = 0; i < ac.layers.Length; i++)
                {
                    AnimatorStateMachine sm = ac.layers[i].stateMachine;
                    layerStates[i] = GetStates(animator, i, sm);
                }
            }
        }
        static Dictionary<string, StateInfo> GetStates(Animator animator, int layerIndex, AnimatorStateMachine sm, string path = "")
        {
            Dictionary<string, StateInfo> states = new();

            foreach (ChildAnimatorState state in sm.states)
            {
                string fullPath = path + sm.name + "." + state.state.name;
                int fullPathHash = Animator.StringToHash(fullPath);
                StateMachineBehaviour[] stateBehaviours = animator.GetBehaviours(fullPathHash, layerIndex);
                //Debug.Log($"{fullPath} : {stateBehaviours.Length}");

                states[fullPath] = new StateInfo { FullName = fullPath, StateName = state.state.name, Behaviours = stateBehaviours };
            }

            foreach (ChildAnimatorStateMachine subSm in sm.stateMachines)
            {
                var subStates = GetStates(animator, layerIndex, subSm.stateMachine, path + sm.name + ".");
                foreach (var subState in subStates)
                {
                    states[subState.Key] = subState.Value;
                }
            }

            return states;
        }

        public static string GetStateFullPath(Dictionary<int, Dictionary<string, StateInfo>> layerStates, string stateName, int layerIndex)
        {
            foreach (var stateInfo in layerStates[layerIndex])
            {
                if (stateInfo.Value.StateName == stateName)
                {
                    return stateInfo.Key;
                }
            }

            return null;
        }
        public static bool HasMultipleActiveLayers(Animator animator)
        {
            int activeLayers = 0;

            for (int i = 0; i < animator.layerCount; i++)
            {
                if (animator.GetLayerWeight(i) > 0)
                {
                    activeLayers++;
                }
            }

            if (activeLayers > 1) return true;
            else return false;
        }
        public static bool HasStateBehaviour<T>(Dictionary<int, Dictionary<string, StateInfo>> layerStates, int layerIndex, string stateName) where T : StateMachineBehaviour
        {
            return layerStates[layerIndex].Values.Any(stateInfo => stateInfo.StateName == stateName && stateInfo.Behaviours.OfType<T>().Any());
        }
        public static StateMachineBehaviour GetStateBehaviourOfType<T>(Dictionary<int, Dictionary<string, StateInfo>> layerStates, int layerIndex, string stateFullName) where T : StateMachineBehaviour
        {
            if (layerStates.TryGetValue(layerIndex, out var stateInfos) && stateInfos.TryGetValue(stateFullName, out var stateInfo))
            {
                foreach (var behaviour in stateInfo.Behaviours)
                {
                    if (behaviour is T typedBehaviour)
                    {
                        return typedBehaviour;
                    }
                }
            }
            return null;
        }
        public static List<int> GetActiveLayersWithState(Animator animator, Dictionary<int, Dictionary<string, StateInfo>> layerStates, string stateName)
        {
            List<int> layerIndices = new();

            for (int i = 0; i < animator.layerCount; i++)
            {
                if (animator.GetLayerWeight(i) > 0)
                {
                    foreach (var stateInfo in layerStates[i])
                    {
                        if (stateInfo.Value.StateName == stateName)
                        {
                            layerIndices.Add(i);
                            break;
                        }
                    }
                }
            }

            return layerIndices;
        }
        public static List<int> GetActiveLayersIndices(Animator animator)
        {
            List<int> activeLayersIndices = new();

            for (int i = 0; i < animator.layerCount; i++)
            {
                if (animator.GetLayerWeight(i) > 0)
                {
                    activeLayersIndices.Add(i);
                }
            }

            return activeLayersIndices;
        }
        public static List<int> GetActiveLayersIndices(Animator animator, params int[] excludeLayers)
        {
            List<int> activeLayersIndices = new();

            for (int i = 0; i < animator.layerCount; i++)
            {
                if (animator.GetLayerWeight(i) > 0 && !excludeLayers.Contains(i))
                {
                    activeLayersIndices.Add(i);
                }
            }

            return activeLayersIndices;
        }
        public static List<int> GetDesactiveLayersIndices(Animator animator)
        {
            List<int> activeLayersIndices = new();

            for (int i = 0; i < animator.layerCount; i++)
            {
                if (animator.GetLayerWeight(i) == 0)
                {
                    activeLayersIndices.Add(i);
                }
            }

            return activeLayersIndices;
        }
        public static List<int> GetDesactiveLayersIndices(Animator animator, params int[] excludeLayers)
        {
            List<int> activeLayersIndices = new();

            for (int i = 0; i < animator.layerCount; i++)
            {
                if (animator.GetLayerWeight(i) == 0 && !excludeLayers.Contains(i))
                {
                    activeLayersIndices.Add(i);
                }
            }

            return activeLayersIndices;
        }
    }
}
