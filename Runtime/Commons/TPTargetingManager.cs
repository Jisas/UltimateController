using UltimateFramework.LocomotionSystem;
using UltimateFramework.ActionsSystem;
using System.Collections.Generic;
using UltimateFramework.Inputs;
using UltimateFramework.Utils;
using UltimateFramework;
using UnityEngine;

public class TPTargetingManager : UFBaseComponent
{
    public Transform CurrentTarget { get; private set; }

    [SerializeField] LayerMask targetLayers;
    [SerializeField] Transform enemyTarget_Locator;
    [SerializeField] Transform lockOnCanvas;
    [Tooltip("StateDrivenMethod for Switching Cameras")]
    [SerializeField] Animator cinemachineAnimator;

    [Header("Settings")]
    [SerializeField] float noticeZone = 10;
    [SerializeField, Tooltip("Angle_Degree")] float maxNoticeAngle = 60;
    [SerializeField] float crossHair_Scale = 0.1f;

    private Vector3 pos;
    private Transform cam;
    private bool enemyLocked;
    private bool canTarget = true;
    private float currentYOffset;
    private ActionsComponent m_Actions;
    private EntityActionInputs m_InputsManager;
    private BaseLocomotionComponent m_Locomotion;
    private readonly List<Transform> targetedObjects = new();

    public LayerMask TargetLayers { get => targetLayers; set => targetLayers = value; }
    public Transform EnemyTarget_Locator { get => enemyTarget_Locator; set => enemyTarget_Locator = value; }
    public Animator CinemachineAnimator { get => cinemachineAnimator; set => cinemachineAnimator = value; }
    public float NoticeZone { get => noticeZone; set => noticeZone = value; }
    public float MaxNoticeAngle { get => maxNoticeAngle; set => maxNoticeAngle = value; }
    public float CrossHair_Scale { get => crossHair_Scale; set => crossHair_Scale = value; }
    public Transform LockOnCanvas { get => lockOnCanvas; set => lockOnCanvas = value; }

    #region Mono
    void Start()
    {
        m_Locomotion = GetComponent<BaseLocomotionComponent>();
        m_InputsManager = GetComponent<EntityActionInputs>();
        m_Actions = GetComponent<ActionsComponent>();
        lockOnCanvas.gameObject.SetActive(false);
        cam = Camera.main.transform;
    }
    void Update()
    {
        if (canTarget)
        {
            if (m_InputsManager.Targeting)
            {
                CurrentTarget = ScanNearBy();
                m_Actions.CurrentTarget = CurrentTarget;

                if (CurrentTarget != null) FoundTarget();
                else ResetTarget();

                m_InputsManager.Targeting = false;
            }

            if (enemyLocked)
                LookAtTarget();

            if (!TargetOnRange())
                ResetTarget();
        }
    }
    #endregion

    #region Internal
    void FoundTarget()
    {
        lockOnCanvas.gameObject.SetActive(true);
        cinemachineAnimator.Play("TargetCamera");

        if (!m_Locomotion.IsCrouch)
            m_Locomotion.SetLocomotionType(LocomotionType.Strafe);
        else m_Locomotion.SetLocomotionType(LocomotionType.Strafe, false);

        enemyLocked = true;
    }
    void ResetTarget()
    {
        lockOnCanvas.gameObject.SetActive(false);
        CurrentTarget = null;
        enemyLocked = false;
        cinemachineAnimator.Play("FollowCamera");
        m_Locomotion.IsTargetting = false;

        if (m_Locomotion.CurrentMap.useOverrideAtStartup)
            m_Locomotion.SetLocomotionType(LocomotionType.ForwardFacing, false);
        else m_Locomotion.SetLocomotionType(LocomotionType.ForwardFacing);
    }
    Transform ScanNearBy()
    {
        Collider[] nearbyTargets = Physics.OverlapSphere(transform.position, noticeZone, targetLayers);
        float closestAngle = maxNoticeAngle;
        Transform closestTarget = null;
        if (nearbyTargets.Length <= 0 || targetedObjects.Count == nearbyTargets.Length)
        {
            ResetTarget();
            targetedObjects.Clear();
            return null;
        }

        for (int i = 0; i < nearbyTargets.Length; i++)
        {
            if (targetedObjects.Contains(nearbyTargets[i].transform))
                continue;

            Vector3 dir = nearbyTargets[i].transform.position - cam.position;
            dir.y = 0;
            float _angle = Vector3.Angle(cam.forward, dir);

            if (_angle < closestAngle)
            {
                closestTarget = nearbyTargets[i].transform.root;
                closestAngle = _angle;
            }
        }

        if (!closestTarget)
        {
            ResetTarget();
            return null;
        }

        targetedObjects.Add(closestTarget);

        float h1 = closestTarget.GetComponent<CharacterController>().height;
        float h2 = closestTarget.localScale.y;
        float h = h1 * h2;
        float half_h = (h / 2) / 2;
        currentYOffset = h - half_h;

        if (currentYOffset > 1.6f && currentYOffset < 1.6f * 3) currentYOffset = 1.6f;
        Vector3 tarPos = closestTarget.position + new Vector3(0, currentYOffset, 0);

        if (Blocked(tarPos))
        {
            ResetTarget();
            return null;
        }

        m_Locomotion.IsTargetting = true;
        return closestTarget;
    }
    bool Blocked(Vector3 t)
    {       
        if (Physics.Linecast(transform.position + Vector3.up * 0.5f, t, out RaycastHit hit))
        {
            if (!hit.transform.CompareTag("Enemy") && !hit.transform.CompareTag(this.tag))
                return true;
        }
        return false;
    }
    bool TargetOnRange()
    {
        float dis = (transform.position - pos).magnitude;

        if (dis > noticeZone) return false; 
        else return true;
    }
    void LookAtTarget()
    {
        if (CurrentTarget == null) return;

        pos = CurrentTarget.position + new Vector3(0, currentYOffset, 0);
        lockOnCanvas.position = pos;
        lockOnCanvas.localScale = Vector3.one * ((cam.position - pos).magnitude * crossHair_Scale);
        enemyTarget_Locator.position = pos;
    }
    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, noticeZone);
    }
#endif
}
