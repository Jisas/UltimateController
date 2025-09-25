using UltimateFramework.RespawnSystem;
using UltimateFramework.Inputs;
using System.Collections;
using UnityEngine.Events;
//using EasyTransition;
using UnityEngine;

namespace UltimateFramework.DeadSystem
{
    [RequireComponent(typeof(BaseEntityRespawnComponent))]
    public class PlayerDeadComponent : BaseDeadComponent
    {
        #region Serialized Fields
        [SerializeField] private Animator DeadTextAnimator;
        [SerializeField] private Animator HUDAnimator;
        //[SerializeField] private TransitionSettings transition;
        [Space(5)]
        [SerializeField] private float startDelay;
        [SerializeField] private float fadeInDelay;
        [SerializeField] private float fadeOutDelay;
        [Space(10)] public UnityEvent OnDeadEvent;
        #endregion

        #region Private Fields
        private BaseEntityRespawnComponent m_RespawnComponent;
        private CharacterController m_CharacterController;
        private Animator m_Animator;
        #endregion

        #region Mono
        private void Awake()
        {
            m_RespawnComponent = GetComponent<BaseEntityRespawnComponent>();
            m_CharacterController = GetComponent<CharacterController>();
            m_Animator = GetComponent<Animator>();
        }
        private void OnEnable()
        {
            m_RespawnComponent.WaitTimeToRespawnCoroutine = GetTransitionTime;
            //TransitionManager.Instance.onTransitionCutPointReached += () => m_Animator.SetTrigger("Respawn");
            //TransitionManager.Instance.onTransitionCutPointReached += () => StartCoroutine(m_RespawnComponent.Respawn());
        }
        #endregion

        public override void StartDeadCoroutine() => StartCoroutine(this.Dead());
        protected IEnumerator Dead()
        {
            m_CharacterController.enabled = false;
            InputsManager.EnablePlayerMap(false);
            HUDAnimator.Play("HUD_Exit");

            yield return new WaitForSeconds(startDelay);
            DeadTextAnimator.Play("Dead_Text_Enter");

            yield return new WaitForSeconds(3);
            DeadTextAnimator.Play("Dead_Text_Exit");

            yield return new WaitForSeconds(1);
            DeadTextAnimator.gameObject.SetActive(false);
            //TransitionManager.Instance.Transition(transition, fadeInDelay, fadeOutDelay);
            OnDeadEvent?.Invoke();
        }
        private float GetTransitionTime() => 0; //transition.destroyTime;
    }
}