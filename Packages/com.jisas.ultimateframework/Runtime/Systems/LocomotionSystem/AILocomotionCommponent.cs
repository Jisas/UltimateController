using UltimateFramework.Utils;
using UnityEngine;

namespace UltimateFramework.LocomotionSystem
{
    public class AILocomotionCommponent : BaseLocomotionComponent
    {
        Transform currentTarget;
        Vector3 currentDirection;
        bool _jump = false;
        float threshold = 0.8f;
        float distance = 0;

        public bool Jump
        {
            get => _jump;
            set => _jump = value;
        }

        public void SetTarget(Transform target)
        {
            currentTarget = target;
        }
        public void SetDirection(Vector3 direction)
        {
            currentDirection = direction;
        }

        protected override void OnStart()
        {
            currentTarget = this.transform;
            SetDirection(currentTarget.position - transform.position);
        }
        protected override void OnUpdate()
        {
            distance = Vector3.Distance(transform.position, currentTarget.position);
        }
        protected override Vector2 GetDirection()
        {
            return new Vector2(currentDirection.x, currentDirection.z).normalized;
        }
        protected override float GetMoveMagnitud()
        {
            return distance > threshold ? 1f : 0f;
        }
        protected override void Move()
        {
            float inputMagnitude = GetMoveMagnitud();
            Vector2 moveInput = GetDirection();
            moveInput.Normalize();

            if (_canMove)
            {
                moveSpeed = currentMoveMode == LocomotionMode.Walk ? walkSpeed : jogSpeed;
                CurrentMoveDirection = moveInput;
            }
            else
            {
                moveSpeed = 0;
                inputMagnitude = 0;
            }

            CurrentInputDirection = new Vector3(moveInput.x, 0, moveInput.y);
            CurrentMoveDirection = moveInput;

            float targetSpeed = moveInput == Vector2.zero ? 0.0f : moveSpeed;
            Vector3 velocity = m_Controller.velocity;
            float currentHorizontalSpeed = new Vector3(velocity.x, 0.0f, velocity.z).magnitude;
            float speedOffset = 0.1f;

            if (currentMoveType == LocomotionType.ForwardFacing && inputMagnitude > 1) inputMagnitude = 1;

            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * speedChangeRate);

                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else _speed = targetSpeed;

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed * inputMagnitude, Time.deltaTime * speedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            if (moveInput != Vector2.zero)
            {
                LastInputDirection = new Vector3(moveInput.x, 0.0f, moveInput.y);

                _dirLerpX = Mathf.Lerp(_dirLerpX, moveInput.x, Time.deltaTime * speedChangeRate);
                _dirLerpY = Mathf.Lerp(_dirLerpY, moveInput.y, Time.deltaTime * speedChangeRate);

                if (currentMoveType != LocomotionType.Strafe)
                {
                    _targetRotation = Mathf.Atan2(CurrentInputDirection.x, CurrentInputDirection.z) * Mathf.Rad2Deg +
                        currentTarget.eulerAngles.y;
                }
                else _targetRotation = currentTarget.eulerAngles.y;
            }
            else _targetRotation = transform.eulerAngles.y;

            if (!IsTargetting)
            {
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, rotationSmoothTime);
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }
            else transform.LookAt(currentTarget, Vector3.up);

            if (!m_Animator.applyRootMotion || !grounded)
            {
                if (currentMoveType == LocomotionType.Strafe)
                {
                    Vector3 targetForward = currentTarget.transform.forward;
                    Vector3 targetRight = currentTarget.transform.right;
                    targetForward.y = 0f;
                    targetRight.y = 0f;
                    targetForward.Normalize();
                    targetRight.Normalize();
                    Vector3 moveDirection = targetForward * CurrentInputDirection.z + targetRight * CurrentInputDirection.x;
                    CurrentMoveDirection = moveDirection;
                    m_Controller.Move(moveDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
                }
                else
                {
                    Vector3 moveDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
                    CurrentMoveDirection = moveDirection;
                    m_Controller.Move(moveDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
                }
            }

            lModeSelector = currentMoveMode == LocomotionMode.Walk ? 0 : 1;

            if (_hasAnimator)
            {
                m_Animator.SetFloat(_animIDSpeed, _animationBlend);
                m_Animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
                m_Animator.SetFloat(_animIDDirX, _dirLerpX);
                m_Animator.SetFloat(_animIDDirY, _dirLerpY);
                m_Animator.SetFloat(_animIDLastDirX, LastInputDirection.x);
                m_Animator.SetFloat(_animIDLastDirY, LastInputDirection.z);
                m_Animator.SetFloat(_animIDStrafe, currentMoveType == LocomotionType.Strafe ? 1 : 0);
                m_Animator.SetFloat(_animIDLMode, lModeSelector);
            }
        }
        protected override void JumpAndGravity()
        {
            if (_canJump)
            {
                var currentJumpHeght = movementMode == LocomotionMode.Walk ? walkJumpHeight : jogJumpHeight;
                var currentGravity = movementMode == LocomotionMode.Walk ? walkGravity : jogGravity;
                var currentJumpTimeout = movementMode == LocomotionMode.Walk ? walkJumpTimeout : jogJumpTimeout;
                var currentFallTimeout = movementMode == LocomotionMode.Walk ? walkFallTimeout : jogFallTimeout;

                if (grounded)
                {
                    // reset the fall timeout timer
                    _fallTimeoutDelta = currentFallTimeout;
                    IsFalling = false;

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        m_Animator.SetBool(_animIDJump, false);
                        m_Animator.SetBool(_animIDFreeFall, IsFalling);
                    }

                    // stop our velocity dropping infinitely when grounded
                    if (_verticalVelocity < 0.0f)
                    {
                        _verticalVelocity = -2f;
                    }

                    // Jump
                    if (_jump && _jumpTimeoutDelta <= 0.0f)
                    {
                        // the square root of H * -2 * G = how much velocity needed to reach desired height
                        _verticalVelocity = Mathf.Sqrt(currentJumpHeght * -2f * currentGravity);

                        // update animator if using character
                        if (_hasAnimator)
                        {
                            if (useRootMotionOnMovement && !useRootMotionOnJump) m_Animator.applyRootMotion = false;
                            m_Animator.SetBool(_animIDJump, true);
                        }
                    }

                    // jump timeout
                    if (_jumpTimeoutDelta >= 0.0f)
                    {
                        _jumpTimeoutDelta -= Time.deltaTime;
                        if (useRootMotionOnMovement && !useRootMotionOnJump) m_Animator.applyRootMotion = true;
                    }
                }
                else
                {
                    // reset the jump timeout timer
                    _jumpTimeoutDelta = currentJumpTimeout;

                    // fall timeout
                    if (_fallTimeoutDelta >= 0.0f)
                    {
                        _fallTimeoutDelta -= Time.deltaTime;
                    }
                    else
                    {
                        IsFalling = true;

                        // update animator if using character
                        if (_hasAnimator)
                        {
                            m_Animator.SetBool(_animIDFreeFall, IsFalling);
                        }
                    }

                    // if we are not grounded, do not jump
                    _jump = false;
                }

                // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
                if (_verticalVelocity < _terminalVelocity)
                {
                    _verticalVelocity += currentGravity * Time.deltaTime;
                }
            }
            else _jump = false;
        }
        protected override void Crouch()
        {
            if (_isCrouch) SetLocomotionMode(LocomotionMode.Walk);
            else SetLocomotionMode(movementMode);

            if (_hasAnimator) m_Animator.SetBool(_animIDCrouch, _isCrouch);
        }
    }
}