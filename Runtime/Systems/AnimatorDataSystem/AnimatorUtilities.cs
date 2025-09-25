using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace UltimateFramework.AnimatorDataSystem
{
    public static class AnimatorUtilities
    {
        //public Dictionary<int, Dictionary<string, StateInfo>> LayerStates;

        public static string GetStateFullPath(List<LayerData> layers, string stateName, int layerIndex)
        {
            var layer = layers.FirstOrDefault(l => l.LayerIndex == layerIndex);
            if (layer != null)
            {
                var stateInfo = layer.States.FirstOrDefault(s => s.StateName == stateName);
                if (stateInfo != null)
                {
                    return stateInfo.FullName;
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
        public static bool HasStateBehaviour<T>(List<LayerData> layers, int layerIndex, string stateName) where T : StateMachineBehaviour
        {
            var layer = layers.FirstOrDefault(l => l.LayerIndex == layerIndex);
            if (layer != null)
            {
                return layer.States.Any(stateInfo => stateInfo.StateName == stateName && stateInfo.Behaviours.OfType<T>().Any());
            }
            return false;
        }
        public static StateMachineBehaviour GetStateBehaviourOfType<T>(List<LayerData> layers, int layerIndex, string stateFullName) where T : StateMachineBehaviour
        {
            var layer = layers.FirstOrDefault(l => l.LayerIndex == layerIndex);
            if (layer != null)
            {
                var stateInfo = layer.States.FirstOrDefault(s => s.FullName == stateFullName);
                if (stateInfo != null)
                {
                    return stateInfo.Behaviours.OfType<T>().FirstOrDefault();
                }
            }
            return null;
        }
        public static List<int> GetActiveLayersWithState(Animator animator, List<LayerData> layers, string stateName)
        {
            List<int> layerIndices = new();

            for (int i = 0; i < animator.layerCount; i++)
            {
                if (animator.GetLayerWeight(i) > 0)
                {
                    var layer = layers.FirstOrDefault(l => l.LayerIndex == i);
                    if (layer != null && layer.States.Any(stateInfo => stateInfo.StateName == stateName))
                    {
                        layerIndices.Add(i);
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
