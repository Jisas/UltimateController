using System.Collections.Generic;
using UltimateFramework.Utils;
using UnityEngine;
using System;

namespace UltimateFramework.Inputs
{
    [CreateAssetMenu(menuName = "Ultimate Framework/Systems/Inputs/InputIconsMap")]
    public class InputIconsMap : ScriptableObject 
    {
        public List<InputIconData> inputIcons;

        public Sprite GetInputIcon(string deviceName, string controlPath)
        {
            foreach (var map in inputIcons)
            {
                if (map.control == ControlType.Gamepad)
                {
                    if (map.Name == deviceName)
                    {
                        return map.GetSprite(controlPath);
                    }
                }
                else
                {
                    if (deviceName == "Keyboard" || deviceName == "Mouse")
                    {
                        return map.GetSprite(controlPath);
                    }
                }
            }

            return null;
        }
        public Sprite GetInputIcon(int id, string controlPath)
        {
            foreach (var icon in inputIcons)
            {
                if (inputIcons.IndexOf(icon) == id)
                    return icon.GetSprite(controlPath);
            }

            return null;
        }
    }

    [Serializable]
    public struct InputIconData
    {
        public string Name;
        public ControlType control;
        [Space(5)]
        [MyBox.ConditionalField(nameof(control),  false, ControlType.Gamepad)] public Sprite buttonSouth;
        [MyBox.ConditionalField(nameof(control), false, ControlType.Gamepad)] public Sprite buttonNorth;
        [MyBox.ConditionalField(nameof(control), false, ControlType.Gamepad)] public Sprite buttonEast;
        [MyBox.ConditionalField(nameof(control), false, ControlType.Gamepad)] public Sprite buttonWest;
        [MyBox.ConditionalField(nameof(control), false, ControlType.Gamepad)] public Sprite startButton;
        [MyBox.ConditionalField(nameof(control), false, ControlType.Gamepad)] public Sprite selectButton;
        [MyBox.ConditionalField(nameof(control), false, ControlType.Gamepad)] public Sprite leftTrigger;
        [MyBox.ConditionalField(nameof(control), false, ControlType.Gamepad)] public Sprite rightTrigger;
        [MyBox.ConditionalField(nameof(control), false, ControlType.Gamepad)] public Sprite leftShoulder;
        [MyBox.ConditionalField(nameof(control), false, ControlType.Gamepad)] public Sprite rightShoulder;
        [MyBox.ConditionalField(nameof(control), false, ControlType.Gamepad)] public Sprite dpad;
        [MyBox.ConditionalField(nameof(control), false, ControlType.Gamepad)] public Sprite dpadUp;
        [MyBox.ConditionalField(nameof(control), false, ControlType.Gamepad)] public Sprite dpadDown;
        [MyBox.ConditionalField(nameof(control), false, ControlType.Gamepad)] public Sprite dpadLeft;
        [MyBox.ConditionalField(nameof(control), false, ControlType.Gamepad)] public Sprite dpadRight;
        [MyBox.ConditionalField(nameof(control), false, ControlType.Gamepad)] public Sprite leftStick;
        [MyBox.ConditionalField(nameof(control), false, ControlType.Gamepad)] public Sprite rightStick;
        [MyBox.ConditionalField(nameof(control), false, ControlType.Gamepad)] public Sprite leftStickPress;
        [MyBox.ConditionalField(nameof(control), false, ControlType.Gamepad)] public Sprite rightStickPress;

        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite LeftClick;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite MiddleClick;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite RightClick;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite F1;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite F2;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite F3;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite F4;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite F5;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite F6;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite F7;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite F8;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite F9;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite F10;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite F11;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite F12;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite Keyboard0;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite Keyboard1;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite Keyboard2;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite Keyboard3;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite Keyboard4;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite Keyboard5;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite Keyboard6;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite Keyboard7;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite Keyboard8;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite Keyboard9;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite Q;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite W;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite E;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite R;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite T;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite Y;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite U;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite I;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite O;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite P;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite A;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite S;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite D;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite F;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite G;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite H;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite J;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite K;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite L;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite Z;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite X;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite C;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite V;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite B;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite N;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite M;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite Tab;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite Enter;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite LeftShift;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite LeftControl;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite LeftAlt;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite Spacebar;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite RightAlt;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite RightControl;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite RightShift;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite UpArrow;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite LeftArrow;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite DownArrow;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite RightArrow;
        [MyBox.ConditionalField(nameof(control), false, ControlType.KeyboardMouse)] public Sprite Escape;

        public readonly Sprite GetSprite(string controlPath)
        {
            // From the input system, we get the path of the control on device. So we can just
            // map from that to the sprites we have for gamepads.

            return control switch
            {
                ControlType.Gamepad => controlPath switch
                {
                    "buttonSouth" => buttonSouth,
                    "buttonNorth" => buttonNorth,
                    "buttonEast" => buttonEast,
                    "buttonWest" => buttonWest,
                    "start" => startButton,
                    "select" => selectButton,
                    "leftTrigger" => leftTrigger,
                    "rightTrigger" => rightTrigger,
                    "leftShoulder" => leftShoulder,
                    "rightShoulder" => rightShoulder,
                    "dpad" => dpad,
                    "dpad/up" => dpadUp,
                    "dpad/down" => dpadDown,
                    "dpad/left" => dpadLeft,
                    "dpad/right" => dpadRight,
                    "leftStick" => leftStick,
                    "rightStick" => rightStick,
                    "leftStickPress" => leftStickPress,
                    "rightStickPress" => rightStickPress,
                    _ => null,
                },

                ControlType.KeyboardMouse => controlPath switch
                {
                    "leftButton" => LeftClick,
                    "middleButton" => MiddleClick,
                    "rightButton" => RightClick,
                    "f1" => F1,
                    "f2" => F2,
                    "f3" => F3,
                    "f4" => F4,
                    "f5" => F5,
                    "f6" => F6,
                    "f7" => F7,
                    "f8" => F8,
                    "f9" => F9,
                    "f10" => F10,
                    "f11" => F11,
                    "f12" => F12,
                    "1" => Keyboard1,
                    "2" => Keyboard2,
                    "3" => Keyboard3,
                    "4" => Keyboard4,
                    "5" => Keyboard5,
                    "6" => Keyboard6,
                    "7" => Keyboard7,
                    "8" => Keyboard8,
                    "9" => Keyboard9,
                    "0" => Keyboard0,
                    "q" => Q,
                    "w" => W,
                    "e" => E,
                    "r" => R,
                    "t" => T,
                    "y" => Y,
                    "u" => U,
                    "i" => I,
                    "o" => O,
                    "p" => P,
                    "a" => A,
                    "s" => S,
                    "d" => D,
                    "f" => F,
                    "g" => G,
                    "h" => H,
                    "j" => J,
                    "k" => K,
                    "l" => L,
                    "z" => Z,
                    "x" => X,
                    "c" => C,
                    "v" => V,
                    "b" => B,
                    "n" => N,
                    "m" => M,
                    "tab" => Tab,
                    "enter" => Enter,
                    "leftShift" => LeftShift,
                    "leftControl" => LeftControl,
                    "leftAlt" => LeftAlt,
                    "space" => Spacebar,
                    "alt" => RightAlt,
                    "control" => RightControl,
                    "shift" => RightShift,
                    "upArrow" => UpArrow,
                    "leftArrow" => LeftArrow,
                    "downArrow" => DownArrow,
                    "rightArrow" => RightArrow,
                    "escape" => Escape,
                    _ => null,
                },

                _ => null,
            };
        }
    }
}
