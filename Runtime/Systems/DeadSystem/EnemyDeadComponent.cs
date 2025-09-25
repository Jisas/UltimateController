using UltimateFramework.InventorySystem;
using UltimateFramework.EconomySystem;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using UnityEngine.Events;
using UltimateFramework.SoundSystem;

namespace UltimateFramework.DeadSystem
{
    [Serializable]
    public struct DeadMaterials
    {
        public SkinnedMeshRenderer renderer;
        public Material newMaterial;
    }

    public class EnemyDeadComponent : BaseDeadComponent
    {
        #region Serialized Fields
        [SerializeField] private GameObject visuals;
        [SerializeField] private SoundsFadeManager soundsFadeManager;
        [SerializeField] private Material disolveMaterial;
        [SerializeField] private Animator deadTextAnimator; 
        [SerializeField] private float transitionDuration;
        [SerializeField] private float dissolveDuration;
        [SerializeField] private List<DeadMaterials> m_MatStruct;
        [Space(10)] public UnityEvent OnDeadEvent;
        #endregion

        #region Private Fields
        private InventoryAndEquipmentComponent playerInventory;
        private EconomyComponent playerEconomy;
        #endregion

        private void Awake()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            playerInventory = player.GetComponent<InventoryAndEquipmentComponent>();
            playerEconomy = player.GetComponent<EconomyComponent>();
        }

        public override void StartDeadCoroutine() => StartCoroutine(this.Dead());
        protected IEnumerator Dead()
        {
            visuals.SetActive(false);
            OnDead.Invoke(GameObject.FindGameObjectWithTag("Player"));

            yield return new WaitForEndOfFrame();
            playerInventory.SaveInventoryAndEquipment();
            playerEconomy.SaveEconomy();

            foreach (var @struct in m_MatStruct)
            {
                @struct.renderer.sharedMaterial = @struct.newMaterial;
            }

            float elapsedTime = 0;
            Color baseInitialColor = new(1, 1, 1, 1);

            foreach (var @struct in m_MatStruct)
            {
                @struct.renderer.sharedMaterial.SetColor("_BaseColor", baseInitialColor);
            }

            yield return new WaitForSeconds(1f);
            deadTextAnimator.Play("Dead_Text_Enter");

            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / transitionDuration;

                var base_R_Lerp = Mathf.Lerp(baseInitialColor.r, 0, t);
                var base_G_Lerp = Mathf.Lerp(baseInitialColor.g, 0, t);
                var base_B_Lerp = Mathf.Lerp(baseInitialColor.b, 0, t);
                var base_A_Lerp = Mathf.Lerp(baseInitialColor.a, 0, t);

                foreach (var @struct in m_MatStruct)
                {
                    @struct.renderer.sharedMaterial.SetColor("_BaseColor", new Color(base_R_Lerp, base_G_Lerp, base_B_Lerp, base_A_Lerp));
                }

                yield return null;
            }

            foreach (var @struct in m_MatStruct)
            {
                @struct.renderer.sharedMaterial = disolveMaterial;
            }

            soundsFadeManager.OnRuntimeChangeSound?.Invoke();
            yield return new WaitForSeconds(.5f);

            deadTextAnimator.Play("Dead_Text_Exit");
            float dissolveElapsedTime = 0;
            float disolveInitialValue = 0f;

            while (dissolveElapsedTime < dissolveDuration)
            {
                dissolveElapsedTime += Time.deltaTime;
                float t = dissolveElapsedTime / dissolveDuration;
                float dissolve = Mathf.Lerp(disolveInitialValue, 1, t);

                disolveMaterial.SetFloat("_Dissolve", dissolve);
                yield return null;
            }

            yield return new WaitForSeconds(.2f);
            OnDeadEvent?.Invoke();
            this.gameObject.SetActive(false);
            deadTextAnimator.gameObject.SetActive(false);
        }
    }
}