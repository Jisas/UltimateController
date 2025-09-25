using UltimateFramework.AnimatorDataSystem;
using System.Collections.Generic;
using UltimateFramework.Inputs;
using UltimateFramework.Utils;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using System;
using MyBox;

namespace UltimateFramework.LocomotionSystem
{
    [RequireComponent(typeof(Animator), typeof(CharacterController))]
    [AbstractClassName("BaseLocomotionComponent")]
    public abstract class BaseLocomotionComponent : MonoBehaviour, IUFComponent
    {
        #region Serialized Fields
        [Header("Starting Settings")]
        [SerializeField] protected LocomotionType movementType;
        [SerializeField] protected LocomotionMode movementMode;
        [SerializeField] protected string locomotionMap = "Unarmed";
        [SerializeField] protected string overrideLayer = "Default";
        [SerializeField] private float idleBreakWaitTime = 2.0f;

        [Header("Movement Settings")]
        [Tooltip("If enabled, this component will not control the movement but will apply the other settings.")]
        public bool useRootMotionOnMovement = true;
        public bool useRootMotionOnSprint = false;
        public bool useRootMotionOnJump = false;
        public bool enableJump = false;
        public bool enableCrouch = false;

        [Header("Feet IK")]
        public bool enableFeetIK = true;
        [Range(0, 2)] public float heightFromGroundRaycast = 1.14f;
        [Range(0, 2)] public float raycastDownDistance = 1.5f;
        [Range(0, 2)] public float pelvisUpAndDownSpeed = 0.28f;
        [Range(0, 2)] public float feetToIKPositionSpeed = 0.5f;
        [Range(0, 10)] public float pelvisOffset = 0.0f;
        public string leftFootAnimVariableName = "LeftFootCurve";
        public string rightFootAnimVariableName = "RightFootCurve";
        public bool showSolverDebug = true;

        [Header("Body Inclination")]
        public bool enableBodyInclination = true;
        [Range(0, 45)] public float minAngleToIncline = 10f;
        [Range(0, 1)] public float inclineSpeed = 0.5f;
        [MinMaxRange(-20, 20)] public MinMaxFloat bodyMinMaxInclination = new(-11, 12);
        [Range(0, 1)] public float sensorOffsetY = 0.1f;

        [Header("Head Tracking")]
        public bool enableHeadTracking = true;
        public Vector2 lookTargetOffset;
        public float lookDistance;
        [Range(0, 1f)] public float smoothSpeed = 0.85f;
        [MinMaxRange(-180f, 180f)] public MinMaxFloat horizontalAngleLimit = new(-60, 60);
        private Transform headLookTarget;

        [Header("SFX")]
        public AudioClip landingAudioClip;
        public List<AudioClip> footstepAudioClips;
        [Range(0, 1)] public float footstepAudioVolume = 0.5f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool grounded = true;

        [Tooltip("Useful for rough ground")]
        public float groundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float groundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask groundLayers;
        #endregion

        #region protected variables
        // player
        protected float _speed;
        protected float _animationBlend;
        protected float _targetRotation;
        protected float _rotationVelocity;
        protected float _verticalVelocity;
        protected readonly float _terminalVelocity = 53.0f;
        protected bool _canMove = true;
        protected float _dirLerpX;
        protected float _dirLerpY;

        // movement
        protected float moveSpeed;
        protected float walkSpeed = 2.0f;
        protected float jogSpeed = 5.33f;
        protected float rotationSmoothTime = 0.12f;
        protected float speedChangeRate = 10.0f;
        protected float lModeSelector = 0;
        protected float inputMagnitude = 0;
        protected Vector2 moveInput;
        protected LocomotionType currentMoveType;
        protected LocomotionMode currentMoveMode;
        protected CombatType currentCombatType;

        // jump
        protected float walkJumpHeight = 4f;
        protected float walkGravity = -15.0f;
        protected float walkJumpTimeout = 0.30f;
        protected float walkFallTimeout = 0.15f;
        protected float jogJumpHeight = 4f;
        protected float jogGravity = -15.0f;
        protected float jogJumpTimeout = 0.30f;
        protected float jogFallTimeout = 0.15f;
        protected float _jumpTimeoutDelta;
        protected float _fallTimeoutDelta;
        protected bool _canJump = true;

        // foot IK
        protected Vector3 rightFootPosition, leftFootPosition, rightFootIKPosition, leftFootIKPosition;
        protected Quaternion rightFootIkRotation, leftFootIKRotation;
        protected float lastPelvisPositionY, lastRightFootPositionY, lastLeftFootPositionY;

        // head tracking
        protected Ray _ray;
        protected Vector3 _rayEnd;

        // animation IDs
        protected int _animIDSpeed;
        protected int _animIDGrounded;
        protected int _animIDJump;
        protected int _animIDCrouch;
        protected int _animIDFreeFall;
        protected int _animIDMotionSpeed;
        protected int _animIDMotionMultiplier;
        protected int _animIDDirX;
        protected int _animIDDirY;
        protected int _animIDLastDirX;
        protected int _animIDLastDirY;
        protected int _animStateIDUseEightDirectional;
        protected int _animIDStrafe;
        protected int _animIDLMode;

        // components
        protected CharacterController m_Controller;
        protected EntityActionInputs m_InputManager;
        protected Animator m_Animator;
        protected GameObject m_Camera;
        protected AnimatorDataHandler m_AnimDataHanlder;

        // other values
        protected bool _hasAnimator;
        protected bool _useEightDirectional;
        protected bool _lockMovement;
        protected bool _isTargetting;
        protected bool _isCrouch;
        protected bool _isInIdleBreak;
        protected int _currentBreackIndex = 0;

        // map
        protected LocomotionMap currentMap;
        #endregion

        #region properties
        public Vector3 CurrentInputDirection { get; protected set; }
        public Vector3 LastInputDirection { get; protected set; }
        public Vector3 CurrentMoveDirection { get; protected set; }
        public LocomotionMaster LocomotionMaster { get; protected set; }
        public bool IsFalling { get; protected set; }
        public string LocomotionMap
        {
            get => locomotionMap;
            set => locomotionMap = value;
        }
        public string OverrideLayer
        {
            get => overrideLayer;
            set => overrideLayer = value;
        }
        public LocomotionType CurrentLocomotionType
        {
            get => currentMoveType; 
            set => currentMoveType = value;
        }
        public LocomotionMode CurrentLocomotionMode
        {
            get => movementMode; 
            set => movementMode = value;
        }
        public float MoveSpeed => moveSpeed;
        public float CurrentSpeed => _animationBlend;
        public bool CanMove
        {
            get => _canMove;
            set => _canMove = value;
        }
        public bool CanJump
        {
            get => _canJump;
            set => _canJump = value;
        }
        public bool IsCrouch
        {
            get => _isCrouch;
            private set => _isCrouch = value;
        }
        public bool LockMovement
        {
            get => _lockMovement;
            set => _lockMovement = value;
        }
        public int MotionMultiplier { get => _animIDMotionMultiplier; }
        public string FullBodyMaskName => "FullBodyMask"; 
        public string LowerBodyMaskName => "LowerBodyMask"; 
        public string UpperBodyMaskName => "UpperBodyMask";
        public string RightHandMaskName => "RightHandMask";
        public string RightAndLeftHandMaskName => "RightAndLeftHandMask";
        public Vector2 CenterScreenPoint => new(Screen.width / 2f, Screen.height / 2f);
        public LocomotionMap CurrentMap => currentMap;
        public bool IsTargetting
        {
            get => _isTargetting;
            set => _isTargetting = value;
        }
        public float Rotationspeed
        {
            get => rotationSmoothTime;
            set => rotationSmoothTime = value;
        }
        public float IdleBreakWaitTime 
        { 
            get => idleBreakWaitTime; 
            set => idleBreakWaitTime = value; 
        }
        #endregion

        #region Inheritance Config
        public string ClassName { get; private set; }
        public BaseLocomotionComponent()
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
        private void Awake()
        {
            // get a reference to our main camera
            if (m_Camera == null)
                m_Camera = GameObject.FindGameObjectWithTag("MainCamera");

            LocomotionMaster = Resources.Load<LocomotionMaster>("Data/Locomotion/LocomotionMasterMap");
            LocomotionMaster.RegisterDictionary();

            _hasAnimator = TryGetComponent(out m_Animator);
            m_Controller = TryGetComponent<CharacterController>(out m_Controller) ? m_Controller: null;
            m_InputManager = TryGetComponent<EntityActionInputs>(out m_InputManager) ? m_InputManager : null;
            m_AnimDataHanlder = GetComponent<AnimatorDataHandler>();

            SetUpLocomotion();
        }
        private void Start()
        {
            OnStart();
        }
        private void Update()
        {
            GroundedCheck();
            Move();
            OnUpdate();

            if (enableJump) JumpAndGravity();
            if (enableCrouch) Crouch();
        }
        private void FixedUpdate()
        {
            if (enableFeetIK != true) return;
            if (m_Animator == null) return;

            AdjustFeetTarget(ref rightFootPosition, HumanBodyBones.RightFoot);
            AdjustFeetTarget(ref leftFootPosition, HumanBodyBones.LeftFoot);

            FeetPositionSolver(rightFootPosition, ref rightFootIKPosition, ref rightFootIkRotation);
            FeetPositionSolver(leftFootPosition, ref leftFootIKPosition, ref leftFootIKRotation);
        }
        private void OnAnimatorIK(int layerIndex)
        {
            if (m_Animator == null) return;

            if (enableHeadTracking)
            {
                _ray = gameObject.CompareTag("Player") ? Camera.main.ScreenPointToRay(CenterScreenPoint) : 
                    new Ray(m_Animator.GetBoneTransform(HumanBodyBones.Head).position, transform.forward);

                _rayEnd = _ray.GetPoint(lookDistance);
                headLookTarget.position = new(_rayEnd.x + lookTargetOffset.x, _rayEnd.y + lookTargetOffset.y, _rayEnd.z);

                // Calcular el ángulo
                float horizontalAngle = Vector3.Angle(transform.forward, _ray.direction);

                // Comprobar si los ángulos están dentro de los límites
                if (horizontalAngle < horizontalAngleLimit.Min || horizontalAngle > horizontalAngleLimit.Max)
                {
                    m_Animator.SetLookAtWeight(Mathf.Lerp(1, 0, Time.time * smoothSpeed));
                }
                else
                {
                    m_Animator.SetLookAtWeight(Mathf.Lerp(0, 1, Time.time * smoothSpeed));
                    m_Animator.SetLookAtPosition(headLookTarget.position);
                }
            }

            if (enableFeetIK)
            {
                MovePelvisHeight();
                InclinateBody();

                m_Animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
                m_Animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, m_Animator.GetFloat(rightFootAnimVariableName));
                MoveFeetToIkPoint(AvatarIKGoal.RightFoot, rightFootIKPosition, rightFootIkRotation, ref lastRightFootPositionY);

                m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
                m_Animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, m_Animator.GetFloat(leftFootAnimVariableName));
                MoveFeetToIkPoint(AvatarIKGoal.LeftFoot, leftFootIKPosition, leftFootIKRotation, ref lastLeftFootPositionY);
            }
        }
        protected virtual void OnStart() { }
        protected virtual void OnUpdate() { }
        #endregion

        #region Private Methods
        private void SetUpLocomotion()
        {
            AssignAnimationIDs();
            OverrideAllMaps();
            SetLocomotionType(movementType);
            SetLocomotionMode(movementMode);
            SetCombatType(CombatType.Unarmed);
            SetLocomotionMap(locomotionMap);

            if (useRootMotionOnMovement) m_Animator.applyRootMotion = true;
            if (enableHeadTracking) InitHeadTrack();

            // reset our timeouts on start
            _jumpTimeoutDelta = walkJumpTimeout;
            _fallTimeoutDelta = walkFallTimeout;
        }
        private void OverrideAllMaps()
        {
            foreach (var map in LocomotionMaster.locomotionMaps)
            {
                map.InitOverrideController(m_Animator.runtimeAnimatorController);
                SetOverride(map.Controller, map.name, overrideLayer);
            }
        }
        #endregion

        #region Public Methods
        public void SetLocomotionMap(string mapName)
        {
            currentMap = LocomotionMaster.FindMap(mapName);
            m_AnimDataHanlder.OverrideAnimatorController = currentMap.Controller;
            m_Animator.runtimeAnimatorController = m_AnimDataHanlder.OverrideAnimatorController;

            #region Movement Config
            walkSpeed = currentMap.general.movementConfig.walkSpeed;
            jogSpeed = currentMap.general.movementConfig.jogSpeed;
            rotationSmoothTime = currentMap.general.movementConfig.rotationSmoothTime;
            speedChangeRate = currentMap.general.movementConfig.speedChangeRate;
            #endregion
        }
        public void SwitchLocomotionMap(string mapName, bool trySetLayerWeight = true, bool tryCrouchVerify = false)
        {
            currentMap = LocomotionMaster.FindMap(mapName);
            SetLocomotionMap(mapName);

            var layer = LocomotionMaster.FindOverrideLayer(currentMap.movement, overrideLayer);
            var layerName = layer?.movement.motionMask;
            var layerID = !string.IsNullOrEmpty(layerName) ? m_Animator.GetLayerIndex(layerName) : -1;

            if (trySetLayerWeight && currentMap.useOverrideAtStartup && layerID > -1)
                m_Animator.SetLayerWeight(layerID, 1);
            else m_Animator.SetLayerWeight(layerID, 0);

            if (tryCrouchVerify)
            {
                layerName = layer.globalPose.mask;
                layerID = m_Animator.GetLayerIndex(layerName);

                if (_isCrouch && layerID > -1)
                    m_Animator.SetLayerWeight(layerID, 1);
                else m_Animator.SetLayerWeight(layerID, 0);
            }
        }
        public async Task SwitchLocomotionMapAsync(string mapName, bool trySetLayerWeight = true, bool tryCrouchVerify = false)
        {
            currentMap = LocomotionMaster.FindMap(mapName);
            SetLocomotionMap(mapName);

            await Task.Delay(10);
            var layer = LocomotionMaster.FindOverrideLayer(currentMap.movement, overrideLayer);
            var layerName = layer?.movement.motionMask;
            var layerID = !string.IsNullOrEmpty(layerName) ? m_Animator.GetLayerIndex(layerName) : -1;

            if (((trySetLayerWeight && currentMap.useOverrideAtStartup) || (trySetLayerWeight && !currentMap.useOverrideAtStartup && _isTargetting)) && layerID > -1)
                 m_Animator.SetLayerWeight(layerID, 1);
            else m_Animator.SetLayerWeight(layerID, 0);

            if (tryCrouchVerify)
            {
                layerName = layer.globalPose.mask;
                layerID = m_Animator.GetLayerIndex(layerName);

                if (_isCrouch && layerID > -1)
                    m_Animator.SetLayerWeight(layerID, 1);
                else m_Animator.SetLayerWeight(layerID, 0);
            }
        }
        public void SetLocomotionType(LocomotionType type, bool trySetLayerWeight = true)
        {
            currentMoveType = type;

            if (trySetLayerWeight)
            {
                var layerID = m_Animator.GetLayerIndex(LowerBodyMaskName);

                if (type == LocomotionType.ForwardFacing) m_Animator.SetLayerWeight(layerID, 0);
                else m_Animator.SetLayerWeight(layerID, 1);
            }
        }
        public void SetLocomotionMode(LocomotionMode mode)
        {
            currentMoveMode = mode;
        }
        public void SetCombatType(CombatType type)
        {
            currentCombatType = type;
        }
        public void EnableMovement()
        {
            CanMove = true;;
        }
        public void DisableMovement()
        {
            CanMove = false;
            moveSpeed = 0;
            inputMagnitude = 0;
            moveInput = Vector2.zero;
            CurrentMoveDirection = Vector3.zero;
        }
        #endregion

        #region Anims Setup
        private void AssignAnimationIDs()
        {
            _animStateIDUseEightDirectional = Animator.StringToHash("UseEightDirectional");
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDCrouch = Animator.StringToHash("Crouch");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDMotionMultiplier = Animator.StringToHash("MotionMultiplier");
            _animIDDirX = Animator.StringToHash("DirX");
            _animIDDirY = Animator.StringToHash("DirY");
            _animIDLastDirX = Animator.StringToHash("LastDirX");
            _animIDLastDirY = Animator.StringToHash("LastDirY");
            _animIDStrafe = Animator.StringToHash("Strafe");
            _animIDLMode = Animator.StringToHash("LMode");
        }
        #endregion

        #region Override Animator
        protected void SetOverride(AnimatorOverrideController overrideController, string mapName, string overrideLayerName = default)
        {
            var currentMap = LocomotionMaster.FindMap(mapName);

            #region Anims
            overrideController[LocomotionAnimsData.Idle] = currentMap.general.idle;
            overrideController[LocomotionAnimsData.CrouchAction] = currentMap.general.crouch.crouchEntry;
            overrideController[LocomotionAnimsData.UncrouchAction] = currentMap.general.crouch.crouchExit;

            LocomotionOverrideLayer overrideLayerMap = LocomotionMaster.FindOverrideLayer(currentMap.movement, overrideLayerName);
            overrideController[LocomotionAnimsData.GlobalPose] = overrideLayerMap != null ? overrideLayerMap.globalPose.motion : null;
            overrideLayer = overrideLayerName;

            if (currentMap.useEightDirectional)
            {
                _useEightDirectional = true;
                m_Animator.SetBool(_animStateIDUseEightDirectional, _useEightDirectional);

                // Base Locomotion
                overrideController[LocomotionAnimsData.Walk_Fwd] = currentMap.movement.walk.loopCardinalsEight.forward;
                overrideController[LocomotionAnimsData.Walk_Fwd_Right] = currentMap.movement.walk.loopCardinalsEight.forwardRight;
                overrideController[LocomotionAnimsData.Walk_Fwd_Left] = currentMap.movement.walk.loopCardinalsEight.forwardLeft;
                overrideController[LocomotionAnimsData.Walk_Bwd] = currentMap.movement.walk.loopCardinalsEight.backward;
                overrideController[LocomotionAnimsData.Walk_Bwd_Right] = currentMap.movement.walk.loopCardinalsEight.backwardRight;
                overrideController[LocomotionAnimsData.Walk_Bwd_Left] = currentMap.movement.walk.loopCardinalsEight.backwardLeft;
                overrideController[LocomotionAnimsData.Walk_Left] = currentMap.movement.walk.loopCardinalsEight.left;
                overrideController[LocomotionAnimsData.Walk_Right] = currentMap.movement.walk.loopCardinalsEight.right;

                overrideController[LocomotionAnimsData.Jog_Fwd] = currentMap.movement.jog.loopCardinalsEight.forward;
                overrideController[LocomotionAnimsData.Jog_Fwd_Right] = currentMap.movement.jog.loopCardinalsEight.forwardRight;
                overrideController[LocomotionAnimsData.Jog_Fwd_Left] = currentMap.movement.jog.loopCardinalsEight.forwardLeft;
                overrideController[LocomotionAnimsData.Jog_Bwd] = currentMap.movement.jog.loopCardinalsEight.backward;
                overrideController[LocomotionAnimsData.Jog_Bwd_Right] = currentMap.movement.jog.loopCardinalsEight.backwardRight;
                overrideController[LocomotionAnimsData.Jog_Bwd_Left] = currentMap.movement.jog.loopCardinalsEight.backwardLeft;
                overrideController[LocomotionAnimsData.Jog_Left] = currentMap.movement.jog.loopCardinalsEight.left;
                overrideController[LocomotionAnimsData.Jog_Right] = currentMap.movement.jog.loopCardinalsEight.right;

                overrideController[LocomotionAnimsData.Crouch_Fwd] = currentMap.movement.crouch.loopCardinalsEight.forward;
                overrideController[LocomotionAnimsData.Crouch_Bwd] = currentMap.movement.crouch.loopCardinalsEight.backward;
                overrideController[LocomotionAnimsData.Crouch_Left] = currentMap.movement.crouch.loopCardinalsEight.left;
                overrideController[LocomotionAnimsData.Crouch_Right] = currentMap.movement.crouch.loopCardinalsEight.right;

                // Lower Body Mask Override locomotion
                if (overrideLayerMap != null)
                {
                    if (overrideLayerMap.movement.motionMask == LowerBodyMaskName)
                    {
                        overrideController[LocomotionAnimsData.Walk_Fwd_LBM] = overrideLayerMap.movement.walk.loopCardinalsEight.forward;
                        overrideController[LocomotionAnimsData.Walk_Fwd_Right_LBM] = overrideLayerMap.movement.walk.loopCardinalsEight.forwardRight;
                        overrideController[LocomotionAnimsData.Walk_Fwd_Left_LBM] = overrideLayerMap.movement.walk.loopCardinalsEight.forwardLeft;
                        overrideController[LocomotionAnimsData.Walk_Bwd_LBM] = overrideLayerMap.movement.walk.loopCardinalsEight.backward;
                        overrideController[LocomotionAnimsData.Walk_Bwd_Right_LBM] = overrideLayerMap.movement.walk.loopCardinalsEight.backwardRight;
                        overrideController[LocomotionAnimsData.Walk_Bwd_Left_LBM] = overrideLayerMap.movement.walk.loopCardinalsEight.backwardLeft;
                        overrideController[LocomotionAnimsData.Walk_Left_LBM] = overrideLayerMap.movement.walk.loopCardinalsEight.left;
                        overrideController[LocomotionAnimsData.Walk_Right_LBM] = overrideLayerMap.movement.walk.loopCardinalsEight.right;

                        overrideController[LocomotionAnimsData.Jog_Fwd_LBM] = overrideLayerMap.movement.jog.loopCardinalsEight.forward;
                        overrideController[LocomotionAnimsData.Jog_Fwd_Right_LBM] = overrideLayerMap.movement.jog.loopCardinalsEight.forwardRight;
                        overrideController[LocomotionAnimsData.Jog_Fwd_Left_LBM] = overrideLayerMap.movement.jog.loopCardinalsEight.forwardLeft;
                        overrideController[LocomotionAnimsData.Jog_Bwd_LBM] = overrideLayerMap.movement.jog.loopCardinalsEight.backward;
                        overrideController[LocomotionAnimsData.Jog_Bwd_Right_LBM] = overrideLayerMap.movement.jog.loopCardinalsEight.backwardRight;
                        overrideController[LocomotionAnimsData.Jog_Bwd_Left_LBM] = overrideLayerMap.movement.jog.loopCardinalsEight.backwardLeft;
                        overrideController[LocomotionAnimsData.Jog_Left_LBM] = overrideLayerMap.movement.jog.loopCardinalsEight.left;
                        overrideController[LocomotionAnimsData.Jog_Right_LBM] = overrideLayerMap.movement.jog.loopCardinalsEight.right;
                    }
                    else
                    {
                        overrideController[LocomotionAnimsData.Walk_Fwd] = overrideLayerMap.movement.walk.loopCardinalsEight.forward;
                        overrideController[LocomotionAnimsData.Walk_Fwd_Right] = overrideLayerMap.movement.walk.loopCardinalsEight.forwardRight;
                        overrideController[LocomotionAnimsData.Walk_Fwd_Left] = overrideLayerMap.movement.walk.loopCardinalsEight.forwardLeft;
                        overrideController[LocomotionAnimsData.Walk_Bwd] = overrideLayerMap.movement.walk.loopCardinalsEight.backward;
                        overrideController[LocomotionAnimsData.Walk_Bwd_Right] = overrideLayerMap.movement.walk.loopCardinalsEight.backwardRight;
                        overrideController[LocomotionAnimsData.Walk_Bwd_Left] = overrideLayerMap.movement.walk.loopCardinalsEight.backwardLeft;
                        overrideController[LocomotionAnimsData.Walk_Left] = overrideLayerMap.movement.walk.loopCardinalsEight.left;
                        overrideController[LocomotionAnimsData.Walk_Right] = overrideLayerMap.movement.walk.loopCardinalsEight.right;

                        overrideController[LocomotionAnimsData.Jog_Fwd] = overrideLayerMap.movement.jog.loopCardinalsEight.forward;
                        overrideController[LocomotionAnimsData.Jog_Fwd_Right] = overrideLayerMap.movement.jog.loopCardinalsEight.forwardRight;
                        overrideController[LocomotionAnimsData.Jog_Fwd_Left] = overrideLayerMap.movement.jog.loopCardinalsEight.forwardLeft;
                        overrideController[LocomotionAnimsData.Jog_Bwd] = overrideLayerMap.movement.jog.loopCardinalsEight.backward;
                        overrideController[LocomotionAnimsData.Jog_Bwd_Right] = overrideLayerMap.movement.jog.loopCardinalsEight.backwardRight;
                        overrideController[LocomotionAnimsData.Jog_Bwd_Left] = overrideLayerMap.movement.jog.loopCardinalsEight.backwardLeft;
                        overrideController[LocomotionAnimsData.Jog_Left] = overrideLayerMap.movement.jog.loopCardinalsEight.left;
                        overrideController[LocomotionAnimsData.Jog_Right] = overrideLayerMap.movement.jog.loopCardinalsEight.right;
                    }
                }
            }
            else
            {
                _useEightDirectional = false;
                m_Animator.SetBool(_animStateIDUseEightDirectional, _useEightDirectional);

                // Base locomotion
                overrideController[LocomotionAnimsData.Walk_Fwd] = currentMap.movement.walk.loopCardinalsFour.forward;
                overrideController[LocomotionAnimsData.Walk_Bwd] = currentMap.movement.walk.loopCardinalsFour.backward;
                overrideController[LocomotionAnimsData.Walk_Left] = currentMap.movement.walk.loopCardinalsFour.left;
                overrideController[LocomotionAnimsData.Walk_Right] = currentMap.movement.walk.loopCardinalsFour.right;

                overrideController[LocomotionAnimsData.Jog_Fwd] = currentMap.movement.jog.loopCardinalsFour.forward;
                overrideController[LocomotionAnimsData.Jog_Bwd] = currentMap.movement.jog.loopCardinalsFour.backward;
                overrideController[LocomotionAnimsData.Jog_Left] = currentMap.movement.jog.loopCardinalsFour.left;
                overrideController[LocomotionAnimsData.Jog_Right] = currentMap.movement.jog.loopCardinalsFour.right;

                // Lower Body Mask Override locomotion
                overrideController[LocomotionAnimsData.Walk_Fwd_LBM] = overrideLayerMap.movement.walk.loopCardinalsFour.forward;
                overrideController[LocomotionAnimsData.Walk_Bwd_LBM] = overrideLayerMap.movement.walk.loopCardinalsFour.backward;
                overrideController[LocomotionAnimsData.Walk_Left_LBM] = overrideLayerMap.movement.walk.loopCardinalsFour.left;
                overrideController[LocomotionAnimsData.Walk_Right_LBM] = overrideLayerMap.movement.walk.loopCardinalsFour.right;

                overrideController[LocomotionAnimsData.Jog_Fwd_LBM] = overrideLayerMap.movement.jog.loopCardinalsFour.forward;
                overrideController[LocomotionAnimsData.Jog_Bwd_LBM] = overrideLayerMap.movement.jog.loopCardinalsFour.backward;
                overrideController[LocomotionAnimsData.Jog_Left_LBM] = overrideLayerMap.movement.jog.loopCardinalsFour.left;
                overrideController[LocomotionAnimsData.Jog_Right_LBM] = overrideLayerMap.movement.jog.loopCardinalsFour.right;

                overrideController[LocomotionAnimsData.Crouch_Fwd] = currentMap.movement.crouch.loopCardinalsFour.forward;
                overrideController[LocomotionAnimsData.Crouch_Bwd] = currentMap.movement.crouch.loopCardinalsFour.backward;
                overrideController[LocomotionAnimsData.Crouch_Left] = currentMap.movement.crouch.loopCardinalsFour.left;
                overrideController[LocomotionAnimsData.Crouch_Right] = currentMap.movement.crouch.loopCardinalsFour.right;
            }

            #region Base locomotion
            overrideController[LocomotionAnimsData.Walk_Fwd_End] = currentMap.movement.walk.stopCardinals.forward;
            overrideController[LocomotionAnimsData.Walk_Bwd_End] = currentMap.movement.walk.stopCardinals.backward;
            overrideController[LocomotionAnimsData.Walk_Left_End] = currentMap.movement.walk.stopCardinals.left;
            overrideController[LocomotionAnimsData.Walk_Right_End] = currentMap.movement.walk.stopCardinals.right;
            overrideController[LocomotionAnimsData.Walk_Fwd_Start] = currentMap.movement.walk.startCardinals.forward;
            overrideController[LocomotionAnimsData.Walk_Bwd_Start] = currentMap.movement.walk.startCardinals.backward;
            overrideController[LocomotionAnimsData.Walk_Left_Start] = currentMap.movement.walk.startCardinals.left;
            overrideController[LocomotionAnimsData.Walk_Right_Start] = currentMap.movement.walk.startCardinals.right;
            overrideController[LocomotionAnimsData.Walk_Jump_Start] = currentMap.movement.walk.jump.start;
            overrideController[LocomotionAnimsData.Walk_Jump_Loop] = currentMap.movement.walk.jump.loop;
            overrideController[LocomotionAnimsData.Walk_Jump_End] = currentMap.movement.walk.jump.end;

            overrideController[LocomotionAnimsData.Jog_Fwd_End] = currentMap.movement.jog.stopCardinals.forward;
            overrideController[LocomotionAnimsData.Jog_Bwd_End] = currentMap.movement.jog.stopCardinals.backward;
            overrideController[LocomotionAnimsData.Jog_Left_End] = currentMap.movement.jog.stopCardinals.left;
            overrideController[LocomotionAnimsData.Jog_Right_End] = currentMap.movement.jog.stopCardinals.right;
            overrideController[LocomotionAnimsData.Jog_Fwd_Start] = currentMap.movement.jog.startCardinals.forward;
            overrideController[LocomotionAnimsData.Jog_Bwd_Start] = currentMap.movement.jog.startCardinals.backward;
            overrideController[LocomotionAnimsData.Jog_Left_Start] = currentMap.movement.jog.startCardinals.left;
            overrideController[LocomotionAnimsData.Jog_Right_Start] = currentMap.movement.jog.startCardinals.right;
            overrideController[LocomotionAnimsData.Jog_Jump_Start] = currentMap.movement.jog.jump.start;
            overrideController[LocomotionAnimsData.Jog_Jump_Loop] = currentMap.movement.jog.jump.loop;
            overrideController[LocomotionAnimsData.Jog_Jump_End] = currentMap.movement.jog.jump.end;

            overrideController[LocomotionAnimsData.Crouch_Idle] = currentMap.movement.crouch.idle;
            overrideController[LocomotionAnimsData.Crouch_Fwd_End] = currentMap.movement.crouch.stopCardinals.forward;
            overrideController[LocomotionAnimsData.Crouch_Bwd_End] = currentMap.movement.crouch.stopCardinals.backward;
            overrideController[LocomotionAnimsData.Crouch_Left_End] = currentMap.movement.crouch.stopCardinals.left;
            overrideController[LocomotionAnimsData.Crouch_Right_End] = currentMap.movement.crouch.stopCardinals.right;
            overrideController[LocomotionAnimsData.Crouch_Fwd_Start] = currentMap.movement.crouch.startCardinals.forward;
            overrideController[LocomotionAnimsData.Crouch_Bwd_Start] = currentMap.movement.crouch.startCardinals.backward;
            overrideController[LocomotionAnimsData.Crouch_Left_Start] = currentMap.movement.crouch.startCardinals.left;
            overrideController[LocomotionAnimsData.Crouch_Right_Start] = currentMap.movement.crouch.startCardinals.right;
            #endregion

            if (overrideLayerMap != null)
            {
                if (overrideLayerMap.movement.motionMask == LowerBodyMaskName)
                {
                    overrideController[LocomotionAnimsData.Walk_Fwd_End_LBM] = overrideLayerMap.movement.walk.stopCardinals.forward;
                    overrideController[LocomotionAnimsData.Walk_Bwd_End_LBM] = overrideLayerMap.movement.walk.stopCardinals.backward;
                    overrideController[LocomotionAnimsData.Walk_Left_End_LBM] = overrideLayerMap.movement.walk.stopCardinals.left;
                    overrideController[LocomotionAnimsData.Walk_Right_End_LBM] = overrideLayerMap.movement.walk.stopCardinals.right;
                    overrideController[LocomotionAnimsData.Walk_Fwd_Start_LBM] = overrideLayerMap.movement.walk.startCardinals.forward;
                    overrideController[LocomotionAnimsData.Walk_Bwd_Start_LBM] = overrideLayerMap.movement.walk.startCardinals.backward;
                    overrideController[LocomotionAnimsData.Walk_Left_Start_LBM] = overrideLayerMap.movement.walk.startCardinals.left;
                    overrideController[LocomotionAnimsData.Walk_Right_Start_LBM] = overrideLayerMap.movement.walk.startCardinals.right;

                    overrideController[LocomotionAnimsData.Jog_Fwd_End_LBM] = overrideLayerMap.movement.jog.stopCardinals.forward;
                    overrideController[LocomotionAnimsData.Jog_Bwd_End_LBM] = overrideLayerMap.movement.jog.stopCardinals.backward;
                    overrideController[LocomotionAnimsData.Jog_Left_End_LBM] = overrideLayerMap.movement.jog.stopCardinals.left;
                    overrideController[LocomotionAnimsData.Jog_Right_End_LBM] = overrideLayerMap.movement.jog.stopCardinals.right;
                    overrideController[LocomotionAnimsData.Jog_Fwd_Start_LBM] = overrideLayerMap.movement.jog.startCardinals.forward;
                    overrideController[LocomotionAnimsData.Jog_Bwd_Start_LBM] = overrideLayerMap.movement.jog.startCardinals.backward;
                    overrideController[LocomotionAnimsData.Jog_Left_Start_LBM] = overrideLayerMap.movement.jog.startCardinals.left;
                    overrideController[LocomotionAnimsData.Jog_Right_Start_LBM] = overrideLayerMap.movement.jog.startCardinals.right;
                }
                else
                {
                    overrideController[LocomotionAnimsData.Walk_Fwd_End] = overrideLayerMap.movement.walk.stopCardinals.forward;
                    overrideController[LocomotionAnimsData.Walk_Bwd_End] = overrideLayerMap.movement.walk.stopCardinals.backward;
                    overrideController[LocomotionAnimsData.Walk_Left_End] = overrideLayerMap.movement.walk.stopCardinals.left;
                    overrideController[LocomotionAnimsData.Walk_Right_End] = overrideLayerMap.movement.walk.stopCardinals.right;
                    overrideController[LocomotionAnimsData.Walk_Fwd_Start] = overrideLayerMap.movement.walk.startCardinals.forward;
                    overrideController[LocomotionAnimsData.Walk_Bwd_Start] = overrideLayerMap.movement.walk.startCardinals.backward;
                    overrideController[LocomotionAnimsData.Walk_Left_Start] = overrideLayerMap.movement.walk.startCardinals.left;
                    overrideController[LocomotionAnimsData.Walk_Right_Start] = overrideLayerMap.movement.walk.startCardinals.right;

                    overrideController[LocomotionAnimsData.Jog_Fwd_End] = overrideLayerMap.movement.jog.stopCardinals.forward;
                    overrideController[LocomotionAnimsData.Jog_Bwd_End] = overrideLayerMap.movement.jog.stopCardinals.backward;
                    overrideController[LocomotionAnimsData.Jog_Left_End] = overrideLayerMap.movement.jog.stopCardinals.left;
                    overrideController[LocomotionAnimsData.Jog_Right_End] = overrideLayerMap.movement.jog.stopCardinals.right;
                    overrideController[LocomotionAnimsData.Jog_Fwd_Start] = overrideLayerMap.movement.jog.startCardinals.forward;
                    overrideController[LocomotionAnimsData.Jog_Bwd_Start] = overrideLayerMap.movement.jog.startCardinals.backward;
                    overrideController[LocomotionAnimsData.Jog_Left_Start] = overrideLayerMap.movement.jog.startCardinals.left;
                    overrideController[LocomotionAnimsData.Jog_Right_Start] = overrideLayerMap.movement.jog.startCardinals.right;
                }
            }
            #endregion

            #region Jump config
            // Walk
            walkJumpHeight = currentMap.movement.walk.jumpConfig.jumpHeight;
            walkGravity = currentMap.movement.walk.jumpConfig.gravity;
            walkJumpTimeout = currentMap.movement.walk.jumpConfig.jumpTimeout;
            walkFallTimeout = currentMap.movement.walk.jumpConfig.fallTimeout;

            // Jog
            jogJumpHeight = currentMap.movement.jog.jumpConfig.jumpHeight;
            jogGravity = currentMap.movement.jog.jumpConfig.gravity;
            jogJumpTimeout = currentMap.movement.jog.jumpConfig.jumpTimeout;
            jogFallTimeout = currentMap.movement.jog.jumpConfig.fallTimeout;
            #endregion
        }
        #endregion

        #region Locomotion Logic
        protected void GroundedCheck()
        {
            // set sphere position, with offset
            var position = transform.position;
            Vector3 spherePosition = new (position.x, position.y - groundedOffset,
                position.z);

            grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers,QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                m_Animator.SetBool(_animIDGrounded, grounded);
            }
        }
        protected abstract Vector2 GetDirection();
        protected abstract float GetMoveMagnitud();
        protected abstract void Move();
        protected abstract void JumpAndGravity();
        protected abstract void Crouch();
        protected IEnumerator IdleBreaks()
        {
            if (currentMap.general.idleBreaks.Count > 0)
            {
                yield return new WaitForSeconds(idleBreakWaitTime);
                _isInIdleBreak = false;

                _currentBreackIndex = (_currentBreackIndex + 1) % currentMap.general.idleBreaks.Count;
                var motion = currentMap.general.idleBreaks[_currentBreackIndex];
                m_AnimDataHanlder.OverrideAnimatorController["IdleBreak_Base"] = motion;

                yield return new WaitForEndOfFrame();
                m_Animator.Play("IdleBreak", 0, 0);

                var currentClipTime = m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                while (currentClipTime < 1)
                {
                    yield return null;
                }
            }
        }
        #endregion

        #region Foot IK
        protected void MoveFeetToIkPoint(AvatarIKGoal foot, Vector3 positionIkHolder, Quaternion rotationIKHolder, ref float lastFootPositionY)
        {
            Vector3 targetIkPosition = m_Animator.GetIKPosition(foot);

            if (positionIkHolder != Vector3.zero)
            {
                targetIkPosition = transform.InverseTransformPoint(targetIkPosition);
                positionIkHolder = transform.InverseTransformPoint(positionIkHolder);

                float yVariable = Mathf.Lerp(lastFootPositionY, positionIkHolder.y, feetToIKPositionSpeed);
                targetIkPosition.y += yVariable;

                lastFootPositionY = yVariable;
                targetIkPosition = transform.TransformPoint(targetIkPosition);
                m_Animator.SetIKRotation(foot, rotationIKHolder);
            }

            m_Animator.SetIKPosition(foot, targetIkPosition);
        }
        protected void FeetPositionSolver(Vector3 fromSkyPosition, ref Vector3 feetIkpositions, ref Quaternion feetIkRotations)
        {
            RaycastHit feetOuthit;

            if (Physics.Raycast(fromSkyPosition, Vector3.down, out feetOuthit, raycastDownDistance + heightFromGroundRaycast, groundLayers))
            {
                feetIkpositions = fromSkyPosition;
                float targetHeight = feetOuthit.point.y + pelvisOffset;
                feetIkpositions.y = targetHeight;
                feetIkRotations = Quaternion.FromToRotation(Vector3.up, feetOuthit.normal) * transform.rotation;

                return;
            }

            feetIkpositions = Vector3.zero;
        }
        protected void AdjustFeetTarget(ref Vector3 feetPosition, HumanBodyBones foot)
        {
            feetPosition = m_Animator.GetBoneTransform(foot).position;
            feetPosition.y = transform.position.y + heightFromGroundRaycast;
        }
        protected void MovePelvisHeight()
        {
            if (rightFootIKPosition == Vector3.zero || leftFootIKPosition == Vector3.zero || lastPelvisPositionY == 0)
            {
                lastPelvisPositionY = m_Animator.bodyPosition.y;
                return;
            }

            float lOffsetPosition = leftFootIKPosition.y - transform.position.y;
            float rOffsetPosition = rightFootIKPosition.y - transform.position.y;
            float totalOfset = (lOffsetPosition < rOffsetPosition) ? lOffsetPosition : rOffsetPosition;

            Vector3 newPelvisPosition = m_Animator.bodyPosition + Vector3.up * totalOfset;
            newPelvisPosition.y = Mathf.Lerp(lastPelvisPositionY, newPelvisPosition.y, pelvisUpAndDownSpeed);

            m_Animator.bodyPosition = newPelvisPosition;
            lastPelvisPositionY = m_Animator.bodyPosition.y;
        }
        protected void InclinateBody()
        {
            if (enableBodyInclination)
            {
                Transform spine = m_Animator.GetBoneTransform(HumanBodyBones.Spine);

                if (Physics.Raycast(transform.position + new Vector3(0f, sensorOffsetY, 0f), Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayers))
                {
                    Vector3 slopeDirection = Vector3.ProjectOnPlane(transform.forward, hit.normal);
                    float slopeAngle = Vector3.Angle(transform.forward, slopeDirection);
                    float direction = Mathf.Sign(Vector3.Dot(transform.forward, hit.normal));
                    float tagerInclination = direction > 0 ? bodyMinMaxInclination.Max : bodyMinMaxInclination.Min;

                    // Ajusta la inclinación del personaje en función de la pendiente dentro de un rango maximo de inclinacion
                    if (Mathf.Abs(slopeAngle) > minAngleToIncline)
                    {
                        // Inclina el personaje hacia adelante
                        float amount = (Mathf.Abs(slopeAngle) - minAngleToIncline) / (tagerInclination - minAngleToIncline);
                        Quaternion targetBodyRotation = Quaternion.Euler(0f, 0f, (tagerInclination / amount) * direction);
                        Quaternion lerpBodyRotation = Quaternion.Slerp(spine.localRotation, targetBodyRotation, inclineSpeed);
                        m_Animator.SetBoneLocalRotation(HumanBodyBones.Spine, lerpBodyRotation);
                    }
                }
            }
        }
        #endregion

        #region Head Tracking
        public void InitHeadTrack()
        {
            if (headLookTarget == null)
            {
                var newTarget = new GameObject("HeadLookTarget");
                newTarget.transform.SetParent(this.transform);
                headLookTarget = newTarget.transform;
            }
        }
        #endregion

        #region Animation Events
        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (!(animationEvent.animatorClipInfo.weight > 0.5f)) return;
            if (footstepAudioClips.Count <= 0) return;
            var index = UnityEngine.Random.Range(0, footstepAudioClips.Count);
            AudioSource.PlayClipAtPoint(footstepAudioClips[index], transform.TransformPoint(m_Controller.center), footstepAudioVolume);
        }
        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(landingAudioClip, transform.TransformPoint(m_Controller.center), footstepAudioVolume);
            }
        }
        private void PlayGlobalPose()
        {
            var overrideLayer = LocomotionMaster.FindOverrideLayer(currentMap.movement, this.overrideLayer);
            var overrideLayerMaskName = overrideLayer != null ? overrideLayer.globalPose.mask : "";
            var layerIndex = m_Animator.GetLayerIndex(overrideLayerMaskName);

            if (!string.IsNullOrEmpty(overrideLayerMaskName) && layerIndex >= 0)
                m_Animator.SetLayerWeight(layerIndex, 1);
        }
        private void StopGlobalPose()
        {
            var overrideLayer = LocomotionMaster.FindOverrideLayer(currentMap.movement, this.overrideLayer);
            var overrideLayerMaskName = overrideLayer != null ? overrideLayer.globalPose.mask : "";
            var layerIndex = m_Animator.GetLayerIndex(overrideLayerMaskName);

            if (!string.IsNullOrEmpty(overrideLayerMaskName) && layerIndex >= 0)
                m_Animator.SetLayerWeight(layerIndex, 0);
        }
        private void EnableFeetIK()
        {
            enableFeetIK = true;
        }
        private void DisableFeetIK()
        {
            enableFeetIK = false;
        }
        #endregion

        #region Gizmos
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            var transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            Gizmos.color = grounded ? transparentGreen : transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            var currentTransform = transform;
            var position = currentTransform.position;
            Gizmos.DrawSphere(
                new Vector3(position.x, position.y - groundedOffset, position.z),
                groundedRadius);

            if (showSolverDebug)
            {
                m_Animator = GetComponent<Animator>();
                var leftFoot = m_Animator.GetBoneTransform(HumanBodyBones.LeftFoot).position;
                leftFoot.y = transform.position.y + heightFromGroundRaycast;

                var rightFoot = m_Animator.GetBoneTransform(HumanBodyBones.RightFoot).position;
                rightFoot.y = transform.position.y + heightFromGroundRaycast;

                Debug.DrawLine(leftFoot, leftFoot + Vector3.down * (raycastDownDistance + heightFromGroundRaycast), Color.blue);
                Debug.DrawLine(rightFoot, rightFoot + Vector3.down * (raycastDownDistance + heightFromGroundRaycast), Color.blue);

                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position + new Vector3(0f, sensorOffsetY, 0f), Vector3.down);
            }

            if (enableHeadTracking)
            {
                if (headLookTarget != null)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(_ray);
                    Gizmos.DrawWireSphere(headLookTarget.position, 0.1f);
                }
            }
        }
#endif
        #endregion
    }
}