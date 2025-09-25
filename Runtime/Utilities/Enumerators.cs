namespace UltimateFramework.Utils
{
    public enum ItemType
    {
        None = 0,
        Helment = 1,
        Armor = 2,
        Gloves = 3,
        Pants = 4,
        Boots = 5,
        Weapon = 6,
        Consumable = 7,
        Material = 8,
        QuestItem = 9,
        Amulet = 10,
        Spell = 11,
        Invocation = 12,
    }

    public enum OperationType
    {
        Sum = 0,
        Substract = 1
    }

    public enum ValueType
    {
        Flat = 0,
        Percentage = 1
    }

    public enum BaseOn
    {
        None = 0,
        BaseOrMaxValue = 1,
        CurrentValue = 2
    }

    public enum ValueTo
    {
        MaxValue = 0,
        CurrentValue = 1
    }

    public enum StateFlow 
    { 
        Start = 0, 
        Running = 1, 
        Finished = 2 
    };

    public enum WeaponHand
    {
        OneHand = 0,
        TwoHand = 1,
        OffHand = 2,
        DualHand = 3
    };

    public enum MainHand
    {
        None = 0,
        Right = 1,
        Left = 2
    }

    public enum LocomotionType
    {
        ForwardFacing,
        Strafe
    }

    public enum LocomotionMode
    {
        Walk,
        Jog
    }

    public enum ActionsPriority
    {
        Highest = 0,
        High = 1,
        Middle = 2,
        Low = 3,
        Lowest = 4
    }

    public enum ActionState
    {
        NotStarted = 0,
        Running = 1,
        Finished = 2
    }

    public enum PathUseAs 
    { 
        Default = 0, 
        LastInstance = 1 
    };

    public enum StateType
    {
        Armor = 0,
        Weapon = 1,
        Consumable = 2
    };

    public enum CombatType
    {
        Unarmed = 0,
        Melee = 1,
        Ranged = 2
    };

    public enum ScalingLevel
    {
        A = 0,
        B = 1,
        C = 2,
        D = 3,
        F = 4
    }

    public enum ScalingType
    {
        Linear = 0,
        Exponential = 1,
        Logarithmic = 2
    };

    public enum RequirementFor
    {
        None = 0,
        UseItem = 1,
        Upgrade = 2
    }

    public enum IgnoreOptions
    {
        FeetIK = 0,
        HeadTraking = 1,
        Movement = 2,
        AllOtherActiveLayers = 3,
        Crouch = 4
    }

    public enum TakeDefenceValuesAs
    {
        Flat = 0,
        Percentage = 1
    }

    public enum VFXDirection
    {
        NoneSpecificDirection = 0,
        RightToLeft = 1,
        LeftToRight = 2,
        UpToDown = 3,
        DawnToTop = 4
    }

    public enum NodeState
    {
        Running = 0,
        Success = 1,
        Failure = 2
    }

    public enum PatrolType
    {
        ClosedCircuit = 0,
        RoundTrip = 1
    }

    public enum SocketType 
    { 
        Body = 0, 
        Hand = 1 
    }
    public enum SocketOrientation 
    { 
        Right = 0, 
        Left = 1,
        Top = 2,
        Bottom = 3,
    }

    public enum DetectionType 
    { 
        Raycast, 
        Collider 
    }

    public enum RayType 
    { 
        Straight, 
        Fan 
    }

    public enum AxisType 
    { 
        X, 
        Y, 
        Z 
    }

    public enum MaterialAllocatorType
    {
        MeshRendedrer,
        SkinnedMesh
    }

    public enum RotationType
    {
        Identity,
        SameAsParent
    }

    public enum UIType
    {
        UIToolkit,
        Canvas
    }

    public enum SoundZoneType
    {
        None,
        Disc,
        Square
    }

    public enum ElementAnimationType
    {
        Scale,
        Width,
    }

    public enum ControlType
    {
        Gamepad,
        KeyboardMouse,
    }

    public enum InputIconPathType
    {
        InputAction,
        InputPath,
    }

    public enum RewardType
    {
        Economy,
        Items,
        Both,
    }
}