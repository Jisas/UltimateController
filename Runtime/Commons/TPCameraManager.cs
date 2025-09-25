using UltimateFramework.Inputs;
using UnityEngine.InputSystem;
using UnityEngine;
using UltimateFramework;

[RequireComponent(typeof(PlayerInput))]
public class TPCameraManager : UFBaseComponent
{
    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    // ReSharper disable once IdentifierTypo
    public GameObject target;

    [Tooltip("How far in degrees can you move the camera up")]
    public float topClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float bottomClamp = -30.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float cameraAngleOverride;

    [Header("Sensitivity")]
    [Tooltip("How far in degrees can you move the camera up")]
    [Range(0f, 10f)] public float mouseSens;

    [Tooltip("How far in degrees can you move the camera up")]
    [Range(0f, 10f)] public float stickSens;

    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    // sensitivity
    private float _currentSens;

    // references
#if ENABLE_INPUT_SYSTEM
    private PlayerInput m_PlayerInput;
#endif
    private EntityActionInputs m_InputsManager;

    // constants
    private const float Threshold = 0.01f;

    // properties
    private bool IsCurrentDeviceMouse
    {
        get
        {
#if ENABLE_INPUT_SYSTEM           
            return m_PlayerInput.currentControlScheme == "Keyboard&Mouse";
#else
			return false;
#endif
        }
    }

    // ------------- METHODS ------------- //

    #region Unity Flow
    private void Start()
    {
        _cinemachineTargetYaw = target.transform.rotation.eulerAngles.y;
        m_PlayerInput = GetComponent<PlayerInput>();
        m_InputsManager = GetComponent<EntityActionInputs>();
    }

    private void Update()
    {
        _currentSens = IsCurrentDeviceMouse ? mouseSens : stickSens;
    }

    private void LateUpdate()
    {
        CameraRotation();
    }
    #endregion

    #region Camera Logic
    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (m_InputsManager.Look.sqrMagnitude >= Threshold)
        {
            //Don't multiply mouse input by Time.deltaTime;
            var deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += (m_InputsManager.Look.x * deltaTimeMultiplier) * _currentSens;
            _cinemachineTargetPitch += (m_InputsManager.Look.y * deltaTimeMultiplier) *_currentSens;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, bottomClamp, topClamp);

        // Cinemachine will follow this target
        target.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + cameraAngleOverride, _cinemachineTargetYaw, 0.0f);
    }
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
    #endregion
}