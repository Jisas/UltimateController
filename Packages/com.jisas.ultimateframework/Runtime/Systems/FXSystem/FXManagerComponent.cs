using UltimateFramework.InventorySystem;
using System.Collections.Generic;
using UltimateFramework.Utils;
using System.Collections;
using UnityEngine;
using System;

namespace Ultimateframework.FXSystem
{
    [RequireComponent(typeof(CharacterController), typeof(InventoryAndEquipmentComponent))]
    public class FXManagerComponent : MonoBehaviour
    {
        public Transform VXFHolder;
        public Transform specialVXFHolder;
        //public List<HitSXStructure> hitFX;

        private CharacterController m_Controller;
        private InventoryAndEquipmentComponent m_Equipment;

        #region Structures
        [Serializable]
        public struct FXStructure
        {
            public TagSelector movestAction;
            public FXData fxData;
        }

        [Serializable]
        public struct HitSXStructure
        {
            public LayerMask layer;
            public GameObject hitVFX;
            public AudioClip hitSound;
            [Range(0, 20)] public float volume;
        }
        #endregion

        #region Mono
        private void Awake()
        {
            m_Controller = GetComponent<CharacterController>();
            m_Equipment = GetComponent<InventoryAndEquipmentComponent>();
        }
        #endregion

        //public void PlayHitFX(WeaponBehaviour myWeapon, GameObject hitObj, Vector3 position)
        //{
        //    foreach (var hit in hitFX)
        //    {
        //        if (hitObj.layer != hit.layer) return;
        //        if (hit.hitSound != null) AudioSource.PlayClipAtPoint(hit.hitSound, transform.TransformPoint(position), hit.volume);
        //        if (hit.hitVFX != null && hit.hitVFX.TryGetComponent<ParticleSystem>(out ParticleSystem hitVFXParticles))
        //        {
        //            hitVFXParticles.transform.position = position;
        //            hitVFXParticles.Play();
        //        }
        //    }
        //}
        public void PerformAttackFX(FXData data, MainHand hand = MainHand.None)
        {
            if (data == null) return;

            ParticleSystem particles;
            GameObject newRightVFX = null;
            GameObject newLeftVFX = null;
            GameObject newSpecialVFX = null;
            Transform parent = null;

            if (data.InstantiateOnPlayer)
            {
                switch (hand)
                {
                    case MainHand.Right:
                        newRightVFX = data.RightAttackVFX != null ? Instantiate(data.RightAttackVFX) : null;
                        parent = VXFHolder;
                        break;

                    case MainHand.Left:
                        newLeftVFX = data.LeftAttackVFX != null ? Instantiate(data.LeftAttackVFX) : null;
                        parent = VXFHolder;
                        break;

                    case MainHand.None:
                        newSpecialVFX = data.SpecialAttackVFX != null ? Instantiate(data.SpecialAttackVFX) : null;
                        parent = specialVXFHolder;
                        break;
                }                                             
            }
            else
            {
                GameObject currentWeapon = hand == MainHand.Right ? m_Equipment.GetCurrentRightWeaponObject() : m_Equipment.GetCurrentLeftWeaponObject();

                if (currentWeapon != null)
                {
                    switch (hand)
                    {
                        case MainHand.Right:
                            newRightVFX = data.RightAttackVFX != null ? Instantiate(data.RightAttackVFX) : null;
                            break;

                        case MainHand.Left:
                            newLeftVFX = data.LeftAttackVFX != null ? Instantiate(data.LeftAttackVFX) : null;
                            break;

                        case MainHand.None:
                            newSpecialVFX = data.SpecialAttackVFX != null ? Instantiate(data.SpecialAttackVFX) : null;
                            break;
                    }

                    parent = currentWeapon.transform.Find("End").transform;
                }
            }

            if (parent != null)
            {
                switch (hand)
                {
                    case MainHand.Right:
                        particles = newRightVFX.GetComponent<ParticleSystem>();
                        PlayVFX(newRightVFX, particles);
                        break;

                    case MainHand.Left:
                        particles = newLeftVFX.GetComponent<ParticleSystem>();
                        PlayVFX(newLeftVFX, particles);
                        break;

                    case MainHand.None:
                        particles = newSpecialVFX.GetComponent<ParticleSystem>();
                        PlayVFX(newSpecialVFX, particles);
                        break;
                }

                if (newRightVFX != null)
                {
                    newRightVFX.transform.SetParent(parent);
                    newRightVFX.transform.SetPositionAndRotation(parent.position, Quaternion.identity);
                }

                if (newLeftVFX != null)
                {
                    newLeftVFX.transform.SetParent(parent);
                    newLeftVFX.transform.SetPositionAndRotation(parent.position, Quaternion.identity);
                }

                if (newSpecialVFX != null)
                {
                    newSpecialVFX.transform.SetParent(parent);
                    newSpecialVFX.transform.SetPositionAndRotation(new Vector3(parent.position.x, newSpecialVFX.transform.position.y, parent.position.z), Quaternion.identity);
                }
            }

            if (data.RightAttackSFX != null) AudioSource.PlayClipAtPoint(data.RightAttackSFX, transform.TransformPoint(m_Controller.center), data.SFXVolume);
        }
        private void PlayVFX(GameObject target, ParticleSystem vfx)
        {
            vfx.Play();
            StartCoroutine(DestroyAfterSeconds(target, vfx.main.startLifetime.constant));
        }
        private IEnumerator DestroyAfterSeconds(GameObject target, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            Destroy(target);
        }
    }
}
