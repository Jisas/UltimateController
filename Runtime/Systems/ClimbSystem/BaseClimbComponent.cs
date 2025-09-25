using UltimateFramework.LocomotionSystem;
using UltimateFramework.Commons;
using UltimateFramework.Inputs;
using UnityEngine.InputSystem;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using System;

namespace UltimateFramework
{
    [AbstractClassName("BaseClimbComponent")]
    [RequireComponent(typeof(BaseLocomotionComponent))]
    public class BaseClimbComponent : MonoBehaviour, IUFComponent
    {
        #region Public Fields
        public LayerMask wallLayer;
        [Space(5)]

        public float nearLedgeCheckOffset;
        public float nearLedgeCheckRadius;
        [Space(5)]

        public Vector3 ledgeCheckOffset;
        public float maxDeep;
        [Space(5)]

        public Vector3 handUpLedgeOffset;
        [Space(10)]

        public Transform leftHandBone;
        public Transform rightHandBone;
        public Transform leftFootBone;
        public Transform rightFootBone;
        [Space(10)]

        public Vector3 separationCheckOffset;
        public float separationRayDistance = 2f;
        public float minSeparationDistance = 1.0f;
        public float maxSeparationDistance = 1.0f;
        public float rotationAlingSmoothingFactor = 0.8f;
        [Space(10)]

        public float smoothingFactor = 0.6f;
        public float timeToRotateOnDrop = .5f;
        [Space(10)]

        public float upStepDistance = 1.0f;
        public float upInterpolationDuration = 1f;
        [Space(5)]
        public float downStepDistance = 1.0f;
        public float downInterpolationDuration = 1f;
        #endregion

        #region Properties
        public bool HasEntityManager
        {
            get => TryGetComponent<EntityManager>(out m_EntityManager);
        }
        #endregion

        #region Private Fields
        // References
        protected BaseLocomotionComponent m_Locomotion;
        protected EntityActionInputs m_EntityInputs;
        protected AnimationEvents m_AnimationEvents;
        protected PointsGenerator pointsGenerator;
        protected EntityManager m_EntityManager;
        protected Animator m_Animator;

        // Fields
        protected bool canDropLedge, isDropingLedge, canUpLedge, canUpFromGround;
        protected bool controlLeftHand, controlRightHand, controlLeftFoot, controlRightFoot;
        protected bool hasProcessedRT, hasProcessedLT, hasProcessedRB, hasProcessedLB;
        protected bool isClimbingActionInProgress = false;
        protected bool firstActionNeeded = false;
        protected bool startMovement = false;
        protected bool enableIK = false;
        protected float verticalInput;
        protected string lastAction = "";
        protected Vector3 targetPosition;
        protected Vector3 nearPointOnDrop;
        protected Ray ledgeCheckRay;

        // Animator States
        int AnimStateID_DropFromLedge;
        int AnimStateID_UpFromLedge;
        int AnimStateID_DropFromWall;
        int AnimStateID_UpFromGround;
        int AnimStateID_ClimbDown;
        int AnimStateID_ClimbUp;
        #endregion

        #region Inheritance Config
        public string ClassName { get; private set; }
        public BaseClimbComponent()
        {
            ClassName = GetAbstractClassName() ?? this.GetType().Name;
        }

        private string GetAbstractClassName()
        {
            var type = this.GetType();

            while (type != null)
            {
                var attribute = (AbstractClassNameAttribute)Attribute.GetCustomAttribute(type, typeof(AbstractClassNameAttribute));
                if (attribute != null) return attribute.Name;
                type = type.BaseType;
            }

            return null;
        }
        #endregion

        #region Mono
        void Awake()
        {
            m_Locomotion = GetComponent<BaseLocomotionComponent>();
            m_EntityInputs = GetComponent<EntityActionInputs>();
            m_AnimationEvents = GetComponent<AnimationEvents>();
            m_EntityManager = GetComponent<EntityManager>();
            m_Animator = GetComponent<Animator>();          
        }
        void OnEnable()
        {
            InputsManager.Player.Jump.performed += OnUpFromGround;
            InputsManager.Player.ClimbUpRight.performed += OnControlRightHand;
            InputsManager.Player.ClimbUpLeft.performed += OnControlLeftHand;
            InputsManager.Player.ClimbDownRight.performed += OnControlRightFoot;
            InputsManager.Player.ClimbDownLeft.performed += OnControlLeftFoot;
        }
        void OnDisable()
        {
            InputsManager.Player.Jump.performed -= OnUpFromGround;
            InputsManager.Player.ClimbUpRight.performed -= OnControlRightHand;
            InputsManager.Player.ClimbUpLeft.performed -= OnControlLeftHand;
            InputsManager.Player.ClimbDownRight.performed -= OnControlRightFoot;
            InputsManager.Player.ClimbDownLeft.performed -= OnControlLeftFoot;
        }
        void Start()
        {
            // Set Animator config
            SetAnimatorPropertiesAndStates();
        }
        void Update()
        {
            m_Animator.SetBool("Climb", m_EntityManager.State == EntityState.Climb);
            if (m_EntityManager.State != EntityState.Climb) return;
            HandleClimbInputs();
        }
        void FixedUpdate()
        {
            ledgeCheckRay = new(transform.position + (transform.forward * ledgeCheckOffset.z), Vector3.down);
            Physics.Raycast(ledgeCheckRay, out RaycastHit onLedgeHit, maxDeep, m_Locomotion.groundLayers);
            canDropLedge = onLedgeHit.collider == null;
            NearToLedgeCheck();

            if (m_EntityManager.State != EntityState.Climb) return;

            AdjustPositionToWall();
            CheckHandsForUpLedge();
        }
        void OnAnimatorIK(int layerIndex)
        {
            if (enableIK)
            {
                SetIKPositions();
            }
        }
        #endregion

        #region Internal
        public void SetClimbingSurface(PointsGenerator surface)
        {
            pointsGenerator = surface;
        }
        void SetAnimatorPropertiesAndStates()
        {
            AnimStateID_DropFromLedge = Animator.StringToHash("Drop From Ledge");
            AnimStateID_UpFromGround = Animator.StringToHash("Ground To Climb");
            AnimStateID_DropFromWall = Animator.StringToHash("Drop From Wall");
            AnimStateID_UpFromLedge = Animator.StringToHash("Up Ledge");
            AnimStateID_ClimbDown = Animator.StringToHash("Climb Down");
            AnimStateID_ClimbUp = Animator.StringToHash("Climb Up");
        }
        void SetIKPositions()
        {
            SetIKForExtremity(AvatarIKGoal.LeftHand, leftHandBone);
            SetIKForExtremity(AvatarIKGoal.RightHand, rightHandBone);
            SetIKForExtremity(AvatarIKGoal.LeftFoot, leftFootBone);
            SetIKForExtremity(AvatarIKGoal.RightFoot, rightFootBone);
        }
        void SetIKForExtremity(AvatarIKGoal goal, Transform ikTarget)
        {
            Vector3 closestPoint = GetClosestPoint(ikTarget.position);

            m_Animator.SetIKPositionWeight(goal, 1.0f);
            m_Animator.SetIKPosition(goal, closestPoint);
        }
        void ResetIK()
        {
            m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            m_Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
            m_Animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
        }
        Vector3 GetClosestPoint(Vector3 targetPosition)
        {
            Vector3 closestPoint = targetPosition;
            float minDistance = Mathf.Infinity;

            foreach (Vector3 point in pointsGenerator.SurfacePoints)
            {
                float distance = Vector3.Distance(targetPosition, point);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPoint = point;
                }
            }

            return closestPoint;
        }
        void AdjustPositionToWall()
        {
            Ray ray = new(transform.position + separationCheckOffset, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, separationRayDistance, wallLayer))
            {
                float distanceToWall = hit.distance;
                Vector3 newPosition = transform.position;

                if (distanceToWall > maxSeparationDistance)
                {
                    newPosition.z = hit.point.z - maxSeparationDistance;
                }
                else if (distanceToWall < minSeparationDistance)
                {
                    newPosition.z = hit.point.z + minSeparationDistance;
                }
                newPosition = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * smoothingFactor);

                // Ajusta la rotación para mirar hacia la normal de la superficie escalada
                Vector3 directionToTarget = hit.point - transform.position;
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget, Vector3.up);
                Vector3 euler = targetRotation.eulerAngles;

                // Ajusta sólo el ángulo X basándote en la normal
                euler.x = Quaternion.LookRotation(-hit.normal).eulerAngles.x;

                // Aplicar la rotación suavizada
                Quaternion finalRotation = Quaternion.Euler(euler);

                transform.SetPositionAndRotation(newPosition, Quaternion.Lerp(transform.rotation, finalRotation, Time.deltaTime * rotationAlingSmoothingFactor));
            }
        }
        void ResetOtherControls()
        {
            if (controlLeftHand)
            {
                controlRightHand = controlLeftFoot = controlRightFoot = false;
            }
            else if (controlRightHand)
            {
                controlLeftHand = controlLeftFoot = controlRightFoot = false;
            }
            else if (controlLeftFoot)
            {
                controlLeftHand = controlRightHand = controlRightFoot = false;
            }
            else if (controlRightFoot)
            {
                controlLeftHand = controlRightHand = controlLeftFoot = false;
            }
        }
        void NearToLedgeCheck()
        {
            if (!isDropingLedge && m_EntityManager.State != EntityState.Climb)
            {
                float dist = nearLedgeCheckRadius;
                bool shouldDisableMovement = false; // Variable para controlar el estado del movimiento
                Vector3 sphereCenter = transform.position + (transform.forward * nearLedgeCheckOffset);
                Collider[] objects = Physics.OverlapSphere(sphereCenter, nearLedgeCheckRadius, wallLayer);

                if (objects.Length > 0)
                {
                    for (int i = 0; i < objects.Length; i++)
                    {
                        if (objects[i].gameObject.TryGetComponent<PointsGenerator>(out PointsGenerator surface))
                        {
                            SetClimbingSurface(surface);

                            foreach (Vector3 point in surface.SurfacePoints)
                            {
                                float nearPoint = Vector3.Distance(point, sphereCenter);

                                if (nearPoint < dist && canDropLedge)
                                {
                                    dist = nearPoint;
                                    nearPointOnDrop = point;
                                    shouldDisableMovement = true; // Indicamos que se debe deshabilitar el movimiento
                                    break; // Salimos del bucle si encontramos un punto cercano
                                }
                            }
                            if (shouldDisableMovement) break; // Salimos del bucle externo si ya sabemos que debemos deshabilitar el movimiento
                        }
                    }
                }

                // Llamamos al método correspondiente una sola vez después de evaluar todos los puntos
                if (shouldDisableMovement) m_Locomotion.DisableMovement();
                else m_Locomotion.EnableMovement();
            }
        }
        void CheckHandsForUpLedge()
        {
            if (m_EntityManager.State != EntityState.Climb) return;

            Ray lefHandRay = new(leftHandBone.position + handUpLedgeOffset, transform.forward);
            Ray rightHandRay = new(rightHandBone.position + handUpLedgeOffset, transform.forward);
            canUpLedge = !Physics.Raycast(lefHandRay, .5f, wallLayer) || !Physics.Raycast(rightHandRay, .5f, wallLayer);

            //if (canUpLedge) m_EntityInputs.DisableUpClimbInputs();
            //else m_EntityInputs.EnableUpClimbInputs();
        }
        #endregion

        #region Climb Movement
        private void HandleClimbInputs()
        {
            verticalInput = InputsManager.Player.Move.ReadValue<Vector2>().y;

            if (isClimbingActionInProgress) return;

            if (verticalInput < 0) // Bajar
            {
                if (firstActionNeeded)
                {
                    if (controlRightFoot && !hasProcessedRT)
                    {
                        hasProcessedRT = true;
                        StartClimbRoutine(AnimStateID_ClimbDown, 0, downStepDistance, downInterpolationDuration);
                        isClimbingActionInProgress = true;
                        firstActionNeeded = false;
                        lastAction = "ClimbDownRight";
                    }
                }
                else
                {
                    if (controlRightFoot && !hasProcessedRT && (lastAction == "ClimbDownLeft" || lastAction == "ClimbUpLeft"))
                    {
                        hasProcessedRT = true;
                        StartClimbRoutine(AnimStateID_ClimbDown, 0, downStepDistance, downInterpolationDuration);
                        isClimbingActionInProgress = true;
                        lastAction = "ClimbDownRight";
                    }
                    else if (controlLeftFoot && !hasProcessedLT && (lastAction == "ClimbDownRight" || lastAction == "ClimbUpRight"))
                    {
                        hasProcessedLT = true;
                        StartClimbRoutine(AnimStateID_ClimbDown, 0.5f, downStepDistance, downInterpolationDuration);
                        isClimbingActionInProgress = true;
                        lastAction = "ClimbDownLeft";
                    }
                }
            }
            else if (verticalInput > 0) // Subir
            {
                if (firstActionNeeded)
                {
                    if (controlRightHand && !hasProcessedRB)
                    {
                        hasProcessedRB = true;
                        StartClimbRoutine(AnimStateID_ClimbUp, 0, upStepDistance, upInterpolationDuration);
                        isClimbingActionInProgress = true;
                        firstActionNeeded = false;
                        lastAction = "ClimbUpRight";
                    }
                }
                else
                {
                    if (controlRightHand && !hasProcessedRB && (lastAction == "ClimbDownLeft" || lastAction == "ClimbUpLeft"))
                    {
                        hasProcessedRB = true;
                        StartClimbRoutine(AnimStateID_ClimbUp, 0, upStepDistance, upInterpolationDuration);
                        isClimbingActionInProgress = true;
                        lastAction = "ClimbUpRight";
                    }
                    else if (controlLeftHand && !hasProcessedLB && (lastAction == "ClimbDownRight" || lastAction == "ClimbUpRight"))
                    {
                        hasProcessedLB = true;
                        StartClimbRoutine(AnimStateID_ClimbUp, 0.5f, upStepDistance, upInterpolationDuration);
                        isClimbingActionInProgress = true;
                        lastAction = "ClimbUpLeft";
                    }
                }
            }

            if (!controlRightFoot) { hasProcessedRT = false; }
            if (!controlLeftFoot) { hasProcessedLT = false; }
            if (!controlRightHand) { hasProcessedRB = false; }
            if (!controlLeftHand) { hasProcessedLB = false; }
        }
        private async void StartClimbRoutine(int animationHash, float animationStartTime, float distance, float duration)
        {
            m_Animator.Play(animationHash, 0, animationStartTime);
            m_Animator.speed = 1;

            await WaitForAnimationEvent();
            await ClimbRoutine(distance, duration, animationStartTime);
        }
        private async Task WaitForAnimationEvent()
        {
            // Esperar a que el flag 'startMovement' sea verdadero
            while (!startMovement)
            {
                await Task.Yield();
            }
            startMovement = false; // Reiniciar el flag para la próxima acción
        }
        private async Task ClimbRoutine(float distance, float duration, float animationStartTime)
        {
            float elapsedTime = 0;
            Vector3 startPosition = transform.position;

            targetPosition = new Vector3(
                transform.position.x,
                transform.position.y + (distance * (verticalInput < 0 ? -1 : 1)),
                transform.position.z);

            while (elapsedTime < duration)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                await Task.Yield();
            }
            transform.position = targetPosition;

            StartCoroutine(WaitAnimationFinish(animationStartTime));
        }
        private IEnumerator WaitAnimationFinish(float animationStartTime)
        {
            var finishTime = animationStartTime == 0 ? 0.5f : 1;
            var currentAnimationTime = m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

            while (currentAnimationTime == finishTime)
            {
                yield return null;
            }

            m_Animator.speed = 0;
            isClimbingActionInProgress = false;
        }
        #endregion

        #region Callbacks
        public void OnDropFromLedge(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (canDropLedge)
            {
                isDropingLedge = true;
                transform.position = nearPointOnDrop;
                m_Animator.Play(AnimStateID_DropFromLedge, 0, 0);
            }
        }
        public void OnDropFromWall(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            if (m_EntityManager.State != EntityState.Climb) return;

            m_Animator.Play(AnimStateID_DropFromWall, 0, 0);
        }
        public void OnUpFromLedge(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            if (m_EntityManager.State != EntityState.Climb) return;

            m_Animator.Play(AnimStateID_UpFromLedge, 0, 0);
        }
        public void OnUpFromGround(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            if (m_EntityManager.State != EntityState.Climb) return;

            m_Animator.Play(AnimStateID_UpFromGround, 0, 0);
        }
        public void OnControlLeftHand(InputAction.CallbackContext context)
        {
            controlLeftHand = context.ReadValueAsButton();
            ResetOtherControls();
        }
        public void OnControlRightHand(InputAction.CallbackContext context)
        {
            controlRightHand = context.ReadValueAsButton();
            ResetOtherControls();
        }
        public void OnControlLeftFoot(InputAction.CallbackContext context)
        {
            controlLeftFoot = context.ReadValueAsButton();
            ResetOtherControls();
        }
        public void OnControlRightFoot(InputAction.CallbackContext context)
        {
            controlRightFoot = context.ReadValueAsButton();
            ResetOtherControls();
        }
        #endregion

        #region Animation Events
        public void SwitchToClimb()
        {
            m_EntityManager.State = EntityState.Climb;
            m_Animator.applyRootMotion = true;
            firstActionNeeded = true;
            isDropingLedge = false;
            lastAction = "";
        }
        public void SwitchToNormal()
        {
            m_EntityManager.State = EntityState.Normal;
            m_Animator.applyRootMotion = true;
        }
        public void Rotate()
        {
            Quaternion targetRotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + 180, transform.eulerAngles.z);
            transform.rotation = targetRotation;
        }
        public IEnumerator AjustZOnDrop()
        {
            float time = 0;
            Vector3 targetPosition = transform.position + (transform.forward * 0.5f);

            while (time < 1.5f)
            {
                time += Time.deltaTime;
                transform.position = Vector3.Lerp(transform.position, targetPosition, time);
                yield return null;
            }
        }
        public IEnumerator AjustYOnDrop()
        {
            float time = 0;
            Vector3 targetPosition = transform.position + (Vector3.down * 2f);

            while (time < 1.5f)
            {
                time += Time.deltaTime;
                transform.position = Vector3.Lerp(transform.position, targetPosition, time);
                yield return null;
            }
        }
        public IEnumerator AjustYOnUp()
        {
            float time = 0;
            Vector3 targetPosition = transform.position + (Vector3.up * 1.2f);

            while (time < 1.5f)
            {
                time += Time.deltaTime;
                transform.position = Vector3.Lerp(transform.position, targetPosition, time);
                yield return null;
            }
        }
        public void OntStartMovement() => startMovement = true;
        #endregion

        #region Gizmos
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var green = Color.green;
            var red = Color.red;
            var blue = Color.blue;
            var yellow = Color.yellow;

            // Dibujar la esfera de cercania
            if (!isDropingLedge && HasEntityManager && m_EntityManager.State == EntityState.Normal)
            {
                Gizmos.color = red;
                Gizmos.DrawSphere(transform.position + (transform.forward * nearLedgeCheckOffset), nearLedgeCheckRadius);
            }

            // Dibujar el rayo principal
            ledgeCheckRay = new(
                transform.position +
                ((transform.right * ledgeCheckOffset.x) +
                (transform.forward * ledgeCheckOffset.z) +
                (transform.up * ledgeCheckOffset.y)),
                Vector3.down);

            Gizmos.color = green;
            Gizmos.DrawRay(ledgeCheckRay.origin, ledgeCheckRay.direction * maxDeep);

            Vector3 start = ledgeCheckRay.origin + ledgeCheckRay.direction * maxDeep;
            float crossSize = 0.1f;
            Gizmos.color = blue;
            Gizmos.DrawLine(start + Vector3.left * crossSize, start + Vector3.right * crossSize);
            Gizmos.DrawLine(start + Vector3.forward * crossSize, start + Vector3.back * crossSize);

            // Dibujar el rayo desde el centro del personaje hacia adelante
            Gizmos.color = red;
            Ray separationRay = new(transform.position + separationCheckOffset, transform.forward);
            Gizmos.DrawRay(separationRay.origin, separationRay.direction * separationRayDistance);

            // Dibujar la cruz en el punto de minSeparationDistance
            Vector3 crossPointMin = separationRay.origin + separationRay.direction * minSeparationDistance;
            Gizmos.color = blue;
            Gizmos.DrawLine(crossPointMin + Vector3.left * crossSize, crossPointMin + Vector3.right * crossSize);
            Gizmos.DrawLine(crossPointMin + Vector3.up * crossSize, crossPointMin + Vector3.down * crossSize);

            // Dibujar la cruz en el punto de maxSeparationDistance
            Vector3 crossPointMax = separationRay.origin + separationRay.direction * maxSeparationDistance;
            Gizmos.color = yellow;

            // Línea horizontal
            Gizmos.DrawLine(crossPointMax + Vector3.left * crossSize, crossPointMax + Vector3.right * crossSize);
            // Línea vertical
            Gizmos.DrawLine(crossPointMax + Vector3.up * crossSize, crossPointMax + Vector3.down * crossSize);

            Gizmos.color = yellow;
            Ray lefHandRay = new(leftHandBone.position + handUpLedgeOffset, transform.forward);
            Ray rightHandRay = new(rightHandBone.position + handUpLedgeOffset, transform.forward);
            Gizmos.DrawRay(lefHandRay);
            Gizmos.DrawRay(rightHandRay);
        }
#endif
        #endregion
    }
}
