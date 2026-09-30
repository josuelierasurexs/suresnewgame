using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Surexs.DanceOff.Input
{
    public enum InitialsInputAction { PreviousLetter, NextLetter, Confirm, Back }

    public interface IInitialsInputSource
    {
        event Action<InitialsInputAction> ActionPressed;
    }

    [DefaultExecutionOrder(-190)]
    public sealed class InitialsInputReader : MonoBehaviour, IInitialsInputSource
    {
        private RhythmInputSourceType sourceType;
        private int deviceIndex;
        private float axisThreshold;
        private string preferredGamepad;
        private JoystickInputConfig joystickConfig;
        private Key keyboardUp;
        private Key keyboardDown;
        private Key keyboardConfirm;
        private Key keyboardBack;
        private bool stickUpActive;
        private bool stickDownActive;
        private bool hatUpActive;
        private bool hatDownActive;

        public event Action<InitialsInputAction> ActionPressed;

        public void Configure(RhythmInputSourceType configuredSourceType, int configuredDeviceIndex,
            float configuredAxisThreshold, string configuredPreferredGamepad,
            JoystickInputConfig configuredJoystick, Key up, Key down, Key confirm, Key back)
        {
            sourceType=configuredSourceType;
            deviceIndex=Mathf.Max(0,configuredDeviceIndex);
            axisThreshold=Mathf.Clamp(configuredAxisThreshold,.1f,.95f);
            preferredGamepad=configuredPreferredGamepad?.Trim() ?? string.Empty;
            joystickConfig=configuredJoystick ?? new JoystickInputConfig();
            keyboardUp=up;
            keyboardDown=down;
            keyboardConfirm=confirm;
            keyboardBack=back;
        }

        private void Update()
        {
            if (sourceType==RhythmInputSourceType.Keyboard) ReadKeyboard();
            else if (sourceType==RhythmInputSourceType.Joystick) ReadJoystick();
            else ReadGamepad();
        }

        private void ReadKeyboard()
        {
            var keyboard=Keyboard.current;
            if (keyboard==null) return;
            if (keyboard[keyboardUp].wasPressedThisFrame) ActionPressed?.Invoke(InitialsInputAction.NextLetter);
            if (keyboard[keyboardDown].wasPressedThisFrame) ActionPressed?.Invoke(InitialsInputAction.PreviousLetter);
            if (keyboard[keyboardConfirm].wasPressedThisFrame) ActionPressed?.Invoke(InitialsInputAction.Confirm);
            if (keyboard[keyboardBack].wasPressedThisFrame) ActionPressed?.Invoke(InitialsInputAction.Back);
        }

        private void ReadGamepad()
        {
            var gamepad=ResolveGamepad();
            if (gamepad==null) return;
            var stick=gamepad.leftStick.ReadValue();
            var nextUp=stick.y>=axisThreshold;
            var nextDown=stick.y<=-axisThreshold;
            if (gamepad.dpad.up.wasPressedThisFrame || Rising(nextUp,ref stickUpActive))
                ActionPressed?.Invoke(InitialsInputAction.NextLetter);
            if (gamepad.dpad.down.wasPressedThisFrame || Rising(nextDown,ref stickDownActive))
                ActionPressed?.Invoke(InitialsInputAction.PreviousLetter);
            if (gamepad.buttonSouth.wasPressedThisFrame) ActionPressed?.Invoke(InitialsInputAction.Confirm);
            if (gamepad.buttonEast.wasPressedThisFrame) ActionPressed?.Invoke(InitialsInputAction.Back);
        }

        private void ReadJoystick()
        {
            var joystick=ResolveJoystick();
            if (joystick==null) return;
            var threshold=Mathf.Clamp(joystickConfig.axisThreshold,.1f,.95f);
            var stick=joystick.stick!=null ? joystick.stick.ReadValue() : Vector2.zero;
            var hat=joystick.hatswitch!=null ? joystick.hatswitch.ReadValue() : Vector2.zero;
            var stickUp=Rising(stick.y>=threshold,ref stickUpActive);
            var hatUp=Rising(hat.y>=threshold,ref hatUpActive);
            var stickDown=Rising(stick.y<=-threshold,ref stickDownActive);
            var hatDown=Rising(hat.y<=-threshold,ref hatDownActive);
            var up=stickUp || hatUp;
            var down=stickDown || hatDown;
            if (up) ActionPressed?.Invoke(InitialsInputAction.NextLetter);
            if (down) ActionPressed?.Invoke(InitialsInputAction.PreviousLetter);
            if (WasAnyPressed(joystick,joystickConfig.centerButtonPaths))
                ActionPressed?.Invoke(InitialsInputAction.Confirm);
            if (WasAnyPressed(joystick,joystickConfig.rightButtonPaths))
                ActionPressed?.Invoke(InitialsInputAction.Back);
        }

        private Gamepad ResolveGamepad()
        {
            if (!string.IsNullOrWhiteSpace(preferredGamepad))
            {
                for (var index=0;index<Gamepad.all.Count;index++)
                {
                    var candidate=Gamepad.all[index];
                    if (MatchesExact(candidate.name,preferredGamepad) || MatchesExact(candidate.layout,preferredGamepad) ||
                        MatchesExact(candidate.displayName,preferredGamepad) ||
                        MatchesExact(candidate.description.product,preferredGamepad)) return candidate;
                }
            }
            return deviceIndex<Gamepad.all.Count ? Gamepad.all[deviceIndex] : null;
        }

        private Joystick ResolveJoystick()
        {
            var product=joystickConfig.deviceProduct?.Trim();
            var manufacturer=joystickConfig.deviceManufacturer?.Trim();
            if (!string.IsNullOrWhiteSpace(product) || !string.IsNullOrWhiteSpace(manufacturer))
            {
                for (var index=0;index<Joystick.all.Count;index++)
                {
                    var candidate=Joystick.all[index];
                    var productMatches=string.IsNullOrWhiteSpace(product) ||
                                       ContainsEitherWay(candidate.description.product,product) ||
                                       ContainsEitherWay(candidate.displayName,product) ||
                                       ContainsEitherWay(candidate.layout,product);
                    var actualManufacturer=candidate.description.manufacturer;
                    var manufacturerMatches=string.IsNullOrWhiteSpace(manufacturer) ||
                                            string.IsNullOrWhiteSpace(actualManufacturer) ||
                                            ContainsEitherWay(actualManufacturer,manufacturer);
                    if (productMatches && manufacturerMatches) return candidate;
                }
                return null;
            }
            var indexToUse=Mathf.Max(0,joystickConfig.joystickIndex);
            return indexToUse<Joystick.all.Count ? Joystick.all[indexToUse] : null;
        }

        private static bool WasAnyPressed(InputDevice device,string[] paths)
        {
            if (paths==null) return false;
            for (var index=0;index<paths.Length;index++)
            {
                if (string.IsNullOrWhiteSpace(paths[index])) continue;
                var button=device.TryGetChildControl<ButtonControl>(paths[index].Trim());
                if (button!=null && button.wasPressedThisFrame) return true;
            }
            return false;
        }

        private static bool Rising(bool next,ref bool previous)
        {
            var pressed=next&&!previous;
            previous=next;
            return pressed;
        }

        private static bool MatchesExact(string actual,string expected)
        {
            return !string.IsNullOrWhiteSpace(actual) &&
                   string.Equals(actual.Trim(),expected,StringComparison.OrdinalIgnoreCase);
        }

        private static bool ContainsEitherWay(string first,string second)
        {
            if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(second)) return false;
            return first.IndexOf(second,StringComparison.OrdinalIgnoreCase)>=0 ||
                   second.IndexOf(first,StringComparison.OrdinalIgnoreCase)>=0;
        }
    }
}
