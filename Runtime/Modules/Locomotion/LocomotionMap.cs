using System.Collections.Generic;
using UnityEngine;
using System;

namespace UltimateFramework.LocomotionSystem
{
    [Serializable]
    public class LocomotionMap
    {
        public string name;
        public bool useEightDirectional;
        public bool useOverrideAtStartup;
        public LocomotionGeneralStructure general;
        public LocomotionMovementStructure movement = new();

        public LocomotionMap (string name) => this.name = name;
        public AnimatorOverrideController Controller { get; private set; }
        public void InitOverrideController(RuntimeAnimatorController animcontroller)
        {
            Controller = new AnimatorOverrideController(animcontroller);
        }
    }

    [Serializable]
    public struct LocomotionGeneralStructure
    {
        public AnimationClip idle;
        public LocomotionActionCrouchStructure crouch;
        public MovementConfigStructure movementConfig;
        public List<AnimationClip> idleBreaks;
    }

    [Serializable]
    public class LocomotionMovementStructure
    {
        public LocomotionStructure walk;
        public LocomotionStructure jog;
        public LocomotionCrouchStructure crouch;
        public List<LocomotionOverrideLayer> overrideLayers = new();
    }

    [Serializable]
    public struct LocomotionActionCrouchStructure
    {
        public AnimationClip crouchEntry;
        public AnimationClip crouchExit;
    }

    [Serializable]
    public class BaseLocomotionStructure
    {
        [Tooltip("This will apply to all motion animations on the submap.")] 
        public float motionSpeed = 1.0f;
        public CardinalStructure startCardinals;
        public CardinalStructure stopCardinals;
        public CardinalStructure loopCardinalsFour;
        public EightCardinalStructure loopCardinalsEight;
    }

    [Serializable]
    public class LocomotionStructure : BaseLocomotionStructure
    {
        public CardinalStructure pivotCardinals;
        public LocomotionTurnStructure turnInPlace;
        public LocomotionMultiAnimStructure jump;
        public JumpConfigStructure jumpConfig;
    }

    [Serializable]
    public struct LocomotionCrouchStructure
    {
        public AnimationClip idle;
        public CardinalStructure startCardinals;
        public CardinalStructure stopCardinals;
        public CardinalStructure loopCardinalsFour;
        public EightCardinalStructure loopCardinalsEight;
        public CardinalStructure pivotCardinals;
        public LocomotionTurnStructure turnInPlace;       
    }

    [Serializable]
    public class CardinalStructure
    {
        public AnimationClip forward;
        public AnimationClip backward;
        public AnimationClip left;
        public AnimationClip right;
    }

    [Serializable]
    public class EightCardinalStructure : CardinalStructure
    {
        public AnimationClip forwardRight;
        public AnimationClip forwardLeft;
        public AnimationClip backwardRight;
        public AnimationClip backwardLeft;
    }

    [Serializable]
    public struct LocomotionTurnStructure
    {
        public AnimationClip right45;
        public AnimationClip right90;
        public AnimationClip right135;
        public AnimationClip right180;
        public AnimationClip left45;
        public AnimationClip left90;
        public AnimationClip left135;
        public AnimationClip left180;
    }

    [Serializable]
    public struct LocomotionMultiAnimStructure
    {
        public AnimationClip start;
        public AnimationClip loop;
        public AnimationClip end;
    }

    [Serializable]
    public struct MovementConfigStructure
    {
        [Tooltip("Move speed of the character in m/s")]
        public float walkSpeed;
        [Tooltip("Move speed of the character in m/s")]
        public float jogSpeed;
        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float rotationSmoothTime;
        [Tooltip("Acceleration and deceleration")]
        public float speedChangeRate;
    }

    [Serializable]
    public struct JumpConfigStructure
    {
        [Tooltip("The height the player can jump")]
        public float jumpHeight;
        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float gravity;
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float jumpTimeout;
        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float fallTimeout;
    }

    [Serializable]
    public class LocomotionOverrideLayer
    {
        public string name;
        public GlobalPoseOverrideStructure globalPose;
        public MovementOverrideStructure movement;

        public LocomotionOverrideLayer(string name)
        {
            this.name = name;
            this.globalPose = new();
            this.movement = new();
        }
    }

    [Serializable]
    public struct GlobalPoseOverrideStructure
    {
        public AnimationClip motion;
        public string mask;
    }

    [Serializable]
    public struct MovementOverrideStructure
    {
        public string motionMask;
        public AnimationClip idle;
        public BaseLocomotionStructure walk;
        public BaseLocomotionStructure jog;
    }
}