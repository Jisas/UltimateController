using UltimateFramework.Utils;
using UnityEngine;
using System;
using MyBox;

namespace Ultimateframework.FXSystem
{
    [CreateAssetMenu(menuName = "Ultimate Framework/Systems/Effects/FXData")]
    public class FXData : ScriptableObject
    {
        [SerializeField, Tooltip("If false, it is instantiated at the 'End' point of the weapon.")] 
        private bool instanceVFXOnPlayer;
        [Space(10)]

        [SerializeField] private GameObject rightAttackVFX;
        [SerializeField] private AudioClip rightAttackSFX;
        [SerializeField, MyBox.ConditionalField(nameof(instanceVFXOnPlayer), false, false)] private Vector3 rightPositionOffset;
        [SerializeField, MyBox.ConditionalField(nameof(instanceVFXOnPlayer), false, false)] private RotationType rightRotationType;
        [Space(10)]

        [SerializeField] private GameObject leftAttackVFX;
        [SerializeField] private AudioClip leftAttackSFX;
        [SerializeField, MyBox.ConditionalField(nameof(instanceVFXOnPlayer), false, false)] private Vector3 leftPositionOffset;
        [SerializeField, MyBox.ConditionalField(nameof(instanceVFXOnPlayer), false, false)] private RotationType leftRotationType;
        [Space(10)]

        [SerializeField] private GameObject specialAttackVFX;
        [SerializeField] private AudioClip specialAttackSFX;
        [SerializeField, MyBox.ConditionalField(nameof(instanceVFXOnPlayer), false, false)] private Vector3 specialPositionOffset;
        [SerializeField, MyBox.ConditionalField(nameof(instanceVFXOnPlayer), false, false)] private RotationType specialRotationType;
        [Space(10)]

        [SerializeField, Range(0, 20)] private float sFXVolume;

        #region Properties
        public bool InstantiateOnPlayer {  get => instanceVFXOnPlayer; }
        public GameObject RightAttackVFX { get => rightAttackVFX; }
        public AudioClip RightAttackSFX { get => rightAttackSFX; }
        public Vector3 RightPositionOffset { get => rightPositionOffset; }
        public RotationType RightRotatioTypet { get => rightRotationType; }
        public GameObject LeftAttackVFX { get => leftAttackVFX; }
        public AudioClip LeftAttackSFX { get => leftAttackSFX; }
        public Vector3 LeftPositionOffset { get => leftPositionOffset; }
        public RotationType LeftRotatioTypet { get => leftRotationType; }
        public GameObject SpecialAttackVFX { get => specialAttackVFX; }
        public AudioClip SpecialAttackSFX { get => specialAttackSFX; }
        public Vector3 SpecialPositionOffset { get => specialPositionOffset; }
        public RotationType SpecialRotatioTypet { get => specialRotationType; }
        public float SFXVolume { get => sFXVolume; }

        #endregion
    }
}
