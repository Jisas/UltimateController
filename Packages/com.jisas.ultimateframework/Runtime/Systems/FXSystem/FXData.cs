using UltimateFramework.Utils;
using UnityEngine;

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
        [Space(10)]

        [SerializeField] private GameObject leftAttackVFX;
        [SerializeField] private AudioClip leftAttackSFX;
        [Space(10)]

        [SerializeField] private GameObject specialAttackVFX;
        [SerializeField] private AudioClip specialAttackSFX;
        [Space(10)]

        [SerializeField, Range(0, 20)] private float sFXVolume;

        public bool InstantiateOnPlayer {  get => instanceVFXOnPlayer; }
        public GameObject RightAttackVFX { get => rightAttackVFX; }
        public AudioClip RightAttackSFX { get => rightAttackSFX; }
        public GameObject LeftAttackVFX { get => leftAttackVFX; }
        public AudioClip LeftAttackSFX { get => leftAttackSFX; }
        public GameObject SpecialAttackVFX { get => specialAttackVFX; }
        public AudioClip SpecialAttackSFX { get => specialAttackSFX; }
        public float SFXVolume { get => sFXVolume; }

        public FXData Clone()
        {
            return Instantiate(this);
        }
    }
}
