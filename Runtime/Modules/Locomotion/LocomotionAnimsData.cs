namespace UltimateFramework.LocomotionSystem
{
    public static class LocomotionAnimsData
    {
        private static readonly string baseSufix = "_Base";
        private static readonly string lowerSufix = "_LBM";

        #region Base Locomotion
        public static string GlobalPose = $"GlobalPose{baseSufix}";
        public static string Idle = $"Idle{baseSufix}";
        public static string CrouchAction = $"CrouchAction{baseSufix}";
        public static string UncrouchAction = $"UncrouchAction{baseSufix}";

        public static string Walk_Fwd = $"Walk_Fwd{baseSufix}";
        public static string Walk_Fwd_Right = $"Walk_Fwd_Right{baseSufix}";
        public static string Walk_Fwd_Left = $"Walk_Fwd_Left{baseSufix}";
        public static string Walk_Bwd = $"Walk_Bwd{baseSufix}";
        public static string Walk_Bwd_Right = $"Walk_Bwd_Right{baseSufix}";
        public static string Walk_Bwd_Left = $"Walk_Bwd_Left{baseSufix}";
        public static string Walk_Left = $"Walk_Left{baseSufix}";
        public static string Walk_Right = $"Walk_Right{baseSufix}";
        public static string Walk_Fwd_End = $"Walk_Fwd_End{baseSufix}";
        public static string Walk_Bwd_End = $"Walk_Bwd_End{baseSufix}";
        public static string Walk_Left_End = $"Walk_Left_End{baseSufix}";
        public static string Walk_Right_End = $"Walk_Right_End{baseSufix}";
        public static string Walk_Fwd_Start = $"Walk_Fwd_Start{baseSufix}";
        public static string Walk_Bwd_Start = $"Walk_Bwd_Start{baseSufix}";
        public static string Walk_Left_Start = $"Walk_Left_Start{baseSufix}";
        public static string Walk_Right_Start = $"Walk_Right_Start{baseSufix}";
        public static string Walk_Jump_Start = $"Walk_Jump_Start{baseSufix}";
        public static string Walk_Jump_Loop = $"Walk_Jump_Loop{baseSufix}";
        public static string Walk_Jump_End = $"Walk_Jump_End{baseSufix}";

        public static string Jog_Fwd = $"Jog_Fwd{baseSufix}";
        public static string Jog_Fwd_Right = $"Jog_Fwd_Right{baseSufix}";
        public static string Jog_Fwd_Left = $"Jog_Fwd_Left{baseSufix}";
        public static string Jog_Bwd = $"Jog_Bwd{baseSufix}";
        public static string Jog_Bwd_Right = $"Jog_Bwd_Right{baseSufix}";
        public static string Jog_Bwd_Left = $"Jog_Bwd_Left{baseSufix}";
        public static string Jog_Left = $"Jog_Left{baseSufix}";
        public static string Jog_Right = $"Jog_Right{baseSufix}";
        public static string Jog_Fwd_End = $"Jog_Fwd_End{baseSufix}";
        public static string Jog_Bwd_End = $"Jog_Bwd_End{baseSufix}";
        public static string Jog_Left_End = $"Jog_Left_End{baseSufix}";
        public static string Jog_Right_End = $"Jog_Right_End{baseSufix}";
        public static string Jog_Fwd_Start = $"Jog_Fwd_Start{baseSufix}";
        public static string Jog_Bwd_Start = $"Jog_Bwd_Start{baseSufix}";
        public static string Jog_Left_Start = $"Jog_Left_Start{baseSufix}";
        public static string Jog_Right_Start = $"Jog_Right_Start{baseSufix}";
        public static string Jog_Jump_Start = $"Jog_Jump_Start{baseSufix}";
        public static string Jog_Jump_Loop = $"Jog_Jump_Loop{baseSufix}";
        public static string Jog_Jump_End = $"Jog_Jump_End{baseSufix}";

        public static string Crouch_Idle = $"Crouch_Idle{baseSufix}";
        public static string Crouch_Fwd = $"Crouch_Fwd{baseSufix}";
        public static string Crouch_Bwd = $"Crouch_Bwd{baseSufix}";
        public static string Crouch_Left = $"Crouch_Left{baseSufix}";
        public static string Crouch_Right = $"Crouch_Right{baseSufix}";
        public static string Crouch_Fwd_End = $"Crouch_Fwd_End{baseSufix}";
        public static string Crouch_Bwd_End = $"Crouch_Bwd_End{baseSufix}";
        public static string Crouch_Left_End = $"Crouch_Left_End{baseSufix}";
        public static string Crouch_Right_End = $"Crouch_Right_End{baseSufix}";
        public static string Crouch_Fwd_Start = $"Crouch_Fwd_Start{baseSufix}";
        public static string Crouch_Bwd_Start = $"Crouch_Bwd_Start{baseSufix}";
        public static string Crouch_Left_Start = $"Crouch_Left_Start{baseSufix}";
        public static string Crouch_Right_Start = $"JCrouch_Right_Start{baseSufix}";

        /*public string Walk_Turn_Right45 = $"Walk_Turn_Right45";
        public string Walk_Turn_Right90 = $"Walk_Turn_Right90";
        public string Walk_Turn_Right135 = $"Walk_Turn_Right135";
        public string Walk_Turn_Right180 = $"Walk_Turn_Right180";
        public string Walk_Turn_Left45 = $"Walk_Turn_Left45";
        public string Walk_Turn_Left90 = $"Walk_Turn_Left90";
        public string Walk_Turn_Left135 = $"Walk_Turn_Left135";
        public string Walk_Turn_Left180 = $"Walk_Turn_Left180";*/
        #endregion

        #region Lower Body Mask Override locomotion
        public static string Idle_LBM = $"Idle{baseSufix}";

        public static string Walk_Fwd_LBM = $"Walk_Fwd{lowerSufix}";
        public static string Walk_Fwd_Right_LBM = $"Walk_Fwd_Right{lowerSufix}";
        public static string Walk_Fwd_Left_LBM = $"Walk_Fwd_Left{lowerSufix}";
        public static string Walk_Bwd_LBM = $"Walk_Bwd{lowerSufix}";
        public static string Walk_Bwd_Right_LBM = $"Walk_Bwd_Right{lowerSufix}";
        public static string Walk_Bwd_Left_LBM = $"Walk_Bwd_Left{lowerSufix}";
        public static string Walk_Left_LBM = $"Walk_Left{lowerSufix}";
        public static string Walk_Right_LBM = $"Walk_Right{lowerSufix}";
        public static string Walk_Fwd_End_LBM = $"Walk_Fwd_End{lowerSufix}";
        public static string Walk_Bwd_End_LBM = $"Walk_Bwd_End{lowerSufix}";
        public static string Walk_Left_End_LBM = $"Walk_Left_End{lowerSufix}";
        public static string Walk_Right_End_LBM = $"Walk_Right_End{lowerSufix}";
        public static string Walk_Fwd_Start_LBM = $"Walk_Fwd_Start{lowerSufix}";
        public static string Walk_Bwd_Start_LBM = $"Walk_Bwd_Start{lowerSufix}";
        public static string Walk_Left_Start_LBM = $"Walk_Left_Start{lowerSufix}";
        public static string Walk_Right_Start_LBM = $"Walk_Right_Start{lowerSufix}";
        public static string Walk_Jump_Start_LBM = $"Walk_Jump_Start{lowerSufix}";
        public static string Walk_Jump_Loop_LBM = $"Walk_Jump_Loop{lowerSufix}";
        public static string Walk_Jump_End_LBM = $"Walk_Jump_End{lowerSufix}";

        public static string Jog_Fwd_LBM = $"Jog_Fwd{lowerSufix}";
        public static string Jog_Fwd_Right_LBM = $"Jog_Fwd_Right{lowerSufix}";
        public static string Jog_Fwd_Left_LBM = $"Jog_Fwd_Left{lowerSufix}";
        public static string Jog_Bwd_LBM = $"Jog_Bwd{lowerSufix}";
        public static string Jog_Bwd_Right_LBM = $"Jog_Bwd_Right{lowerSufix}";
        public static string Jog_Bwd_Left_LBM = $"Jog_Bwd_Left{lowerSufix}";
        public static string Jog_Left_LBM = $"Jog_Left{lowerSufix}";
        public static string Jog_Right_LBM = $"Jog_Right{lowerSufix}";
        public static string Jog_Fwd_End_LBM = $"Jog_Fwd_End{lowerSufix}";
        public static string Jog_Bwd_End_LBM = $"Jog_Bwd_End{lowerSufix}";
        public static string Jog_Left_End_LBM = $"Jog_Left_End{lowerSufix}";
        public static string Jog_Right_End_LBM = $"Jog_Right_End{lowerSufix}";
        public static string Jog_Fwd_Start_LBM = $"Jog_Fwd_Start{lowerSufix}";
        public static string Jog_Bwd_Start_LBM = $"Jog_Bwd_Start{lowerSufix}";
        public static string Jog_Left_Start_LBM = $"Jog_Left_Start{lowerSufix}";
        public static string Jog_Right_Start_LBM = $"Jog_Right_Start{lowerSufix}";
        public static string Jog_Jump_Start_LBM = $"Jog_Jump_Start{lowerSufix}";
        public static string Jog_Jump_Loop_LBM = $"Jog_Jump_Loop{lowerSufix}";
        public static string Jog_Jump_End_LBM = $"Jog_Jump_End{lowerSufix}";

        public static string Crouch_Idle_LBM = $"Crouch_Idle{lowerSufix}";
        public static string Crouch_Fwd_LBM = $"Crouch_Fwd{lowerSufix}";
        public static string Crouch_Bwd_LBM = $"Crouch_Bwd{lowerSufix}";
        public static string Crouch_Left_LBM = $"Crouch_Left{lowerSufix}";
        public static string Crouch_Right_LBM = $"Crouch_Right{lowerSufix}";
        public static string Crouch_Fwd_End_LBM = $"Crouch_Fwd_End{lowerSufix}";
        public static string Crouch_Bwd_End_LBM = $"Crouch_Bwd_End{lowerSufix}";
        public static string Crouch_Left_End_LBM = $"Crouch_Left_End{lowerSufix}";
        public static string Crouch_Right_End_LBM = $"Crouch_Right_End{lowerSufix}";
        public static string Crouch_Fwd_Start_LBM = $"Crouch_Fwd_Start{lowerSufix}";
        public static string Crouch_Bwd_Start_LBM = $"Crouch_Bwd_Start{lowerSufix}";
        public static string Crouch_Left_Start_LBM = $"Crouch_Left_Start{lowerSufix}";
        public static string Crouch_Right_Start_LBM = $"JCrouch_Right_Start{lowerSufix}";
        #endregion
    }
}