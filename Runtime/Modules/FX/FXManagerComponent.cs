using UltimateFramework.InventorySystem;
using UltimateFramework.Utils;
using System.Collections;
using UltimateFramework;
using Cinemachine;
using UnityEngine;
using System;

namespace Ultimateframework.FXSystem
{
    [RequireComponent(typeof(CharacterController), typeof(InventoryAndEquipmentComponent))]
    public class FXManagerComponent : UFBaseComponent
    {
        #region PublicFields
        public Transform VXFHolder;
        public Transform specialVXFHolder;
        public CinemachineImpulseSource impulseSource;
        #endregion

        #region PrivateFields
        private Coroutine screenDamageTask;
        private Material m_ScreenDamageMaterial;
        private CharacterController m_Controller;
        private InventoryAndEquipmentComponent m_Equipment;
        #endregion

        #region Mono
        private void Awake()
        {
            m_Controller = GetComponent<CharacterController>();
            m_Equipment = GetComponent<InventoryAndEquipmentComponent>();
            m_ScreenDamageMaterial = Resources.Load<Material>("Materials/ScreenDamage_Material");
        }
        #endregion

        #region SFX
        public void PlaySFX(AudioClip clip)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
        #endregion

        #region AttackVFX
        public void PerformAttackFX(FXData data, MainHand hand = MainHand.None)
        {
            if (data == null) return;

            AudioClip audio = null;
            ParticleSystem particles;
            GameObject newRightVFX = null;
            GameObject newLeftVFX = null;
            GameObject newSpecialVFX = null;
            Vector3 positionOffset = Vector3.zero;
            RotationType rotationType = default;
            Transform parent = null;

            if (data.InstantiateOnPlayer)
            {
                switch (hand)
                {
                    case MainHand.Right:
                        newRightVFX = data.RightAttackVFX != null ? Instantiate(data.RightAttackVFX) : null;
                        audio = data.RightAttackSFX; 
                        parent = VXFHolder;
                        break;

                    case MainHand.Left:
                        newLeftVFX = data.LeftAttackVFX != null ? Instantiate(data.LeftAttackVFX) : null;
                        audio = data.LeftAttackSFX;
                        parent = VXFHolder;
                        break;

                    case MainHand.None:
                        newSpecialVFX = data.SpecialAttackVFX != null ? Instantiate(data.SpecialAttackVFX) : null;                    
                        audio = data.SpecialAttackSFX;
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
                            positionOffset = data.RightPositionOffset;
                            rotationType = data.RightRotatioTypet;
                            audio = data.RightAttackSFX;
                            break;

                        case MainHand.Left:
                            newLeftVFX = data.LeftAttackVFX != null ? Instantiate(data.LeftAttackVFX) : null;
                            positionOffset = data.LeftPositionOffset;
                            rotationType = data.LeftRotatioTypet;
                            audio = data.LeftAttackSFX;
                            break;

                        case MainHand.None:
                            newSpecialVFX = data.SpecialAttackVFX != null ? Instantiate(data.SpecialAttackVFX) : null;
                            positionOffset = data.SpecialPositionOffset;
                            rotationType = data.SpecialRotatioTypet;
                            audio = data.SpecialAttackSFX;
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
                        PlayVFX(newRightVFX, particles, audio);
                        break;

                    case MainHand.Left:
                        particles = newLeftVFX.GetComponent<ParticleSystem>();
                        PlayVFX(newLeftVFX, particles, audio);
                        break;

                    case MainHand.None:
                        particles = newSpecialVFX.GetComponent<ParticleSystem>();
                        PlayVFX(newSpecialVFX, particles, audio);
                        break;
                }

                var rotationToApply = rotationType == RotationType.Identity ? Quaternion.identity : parent.rotation;

                if (newRightVFX != null)
                {
                    newRightVFX.transform.SetParent(parent);
                    newRightVFX.transform.SetPositionAndRotation(parent.position + positionOffset, rotationToApply);
                }

                if (newLeftVFX != null)
                {
                    newLeftVFX.transform.SetParent(parent);
                    newLeftVFX.transform.SetPositionAndRotation(parent.position + positionOffset, rotationToApply);
                }

                if (newSpecialVFX != null)
                {
                    newSpecialVFX.transform.SetParent(parent);
                    newSpecialVFX.transform.SetPositionAndRotation(
                        new Vector3(parent.position.x, newSpecialVFX.transform.position.y, parent.position.z) + positionOffset,
                        rotationToApply);
                }
            }

            if (data.RightAttackSFX != null) AudioSource.PlayClipAtPoint(data.RightAttackSFX, transform.TransformPoint(m_Controller.center), data.SFXVolume);
        }
        private void PlayVFX(GameObject target, ParticleSystem vfx, AudioClip sfx)
        {
            vfx.Play();
            if(sfx != null) AudioSource.PlayClipAtPoint(sfx, transform.position, 1);
            StartCoroutine(DestroyAfterSeconds(target, vfx.main.startLifetime.constant));
        }
        private IEnumerator DestroyAfterSeconds(GameObject target, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            Destroy(target);
        }
        #endregion

        #region TakeDamageVFX
        public void ScreenDamageEffect(float intensity)
        {
            if (screenDamageTask != null)
                StopCoroutine(screenDamageTask);

            screenDamageTask = StartCoroutine(ScreenDamage(intensity));
        }
        private IEnumerator ScreenDamage(float intensity)
        {
            // Cinemachine Camera shake
            var velocity = new Vector3(0, -0.5f, -1);
            velocity.Normalize();
            impulseSource.GenerateImpulse(0.4f * intensity * velocity);

            // Screen Effect
            var targetRadius = Remap(intensity, 0, 1, 1.5f, 1.3f);
            var curRadius = 10f;

            for (float t = 0; curRadius != targetRadius; t += Time.deltaTime)
            {
                var value = Mathf.Lerp(1, targetRadius, t);
                curRadius = Mathf.Clamp(value, 1, targetRadius);

                m_ScreenDamageMaterial.SetFloat("_Vignette_Radius", curRadius);
                yield return null;
            }
            for (float t = 0; curRadius < 10f; t += Time.deltaTime)
            {
                curRadius = Mathf.Lerp(targetRadius, 10, t);
                m_ScreenDamageMaterial.SetFloat("_Vignette_Radius", curRadius);
                yield return null;
            }
        }
        private float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return Mathf.Lerp(toMin, toMax, Mathf.InverseLerp(fromMin, fromMax, value));
        }
        #endregion
    }
}
