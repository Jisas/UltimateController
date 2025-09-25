using UnityEngine.InputSystem;

namespace UltimateFramework.Inputs
{
    public static class InputsManager
    {
        private static readonly UFInputMap input;

        static InputsManager()
        {
            input = new UFInputMap();
            input.Player.Enable();
        }

        public static UFInputMap.PlayerActions Player
        {
            get { return input.Player; }
        }

        public static UFInputMap.UIActions UI
        {
            get { return input.UI; }
        }

        public static void EnablePlayerMap(bool enable)
        {
            if (enable) input.Player.Enable();
            else input.Player.Disable();
        }

        public static void EnableUIMap(bool enable)
        {
            if (enable) input.UI.Enable();
            else input.UI.Disable();
        }

        public static bool GetIsCurrentDiviceMouse(PlayerInput playerInput)
        {
            return playerInput.currentControlScheme == "Keyboard&Mouse";
        }

        public static void SwitchToUI(EntityActionInputs entityInputs, bool isCurrentDiviceMouse)
        {
            if (isCurrentDiviceMouse)
            {
                entityInputs.cursorLocked = false;
                entityInputs.cursorInputForLook = false;
            }

            EnablePlayerMap(false);
            EnableUIMap(true);
        }

        public static void SwitchToPlayer(EntityActionInputs entityInputs, bool isCurrentDiviceMouse)
        {
            if (isCurrentDiviceMouse)
            {
                entityInputs.cursorLocked = true;
                entityInputs.cursorInputForLook = true;
            }

            EnableUIMap(false);
            EnablePlayerMap(true);
        }
    }
}
