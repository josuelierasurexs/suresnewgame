using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Surexs.DanceOff.Input
{
    [DefaultExecutionOrder(-300)]
    public sealed class PlayerReadyInputMonitor : MonoBehaviour
    {
        private RhythmInputSourceType sourceType;
        private int deviceIndex;
        private string preferredGamepad = string.Empty;
        private JoystickInputConfig joystickConfig;
        private Key[] keyboardKeys = Array.Empty<Key>();
        private InputDevice resolvedDevice;

        public bool IsHeld { get; private set; }
        public bool WasPressedThisFrame { get; private set; }
        public bool WasBackPressedThisFrame { get; private set; }
        public bool IsAvailable => resolvedDevice != null;
        public int DeviceId => resolvedDevice?.deviceId ?? -1;
        public RhythmInputSourceType SourceType => sourceType;

        public void Configure(RhythmInputSourceType configuredSourceType, int configuredDeviceIndex,
            string configuredPreferredGamepad, JoystickInputConfig configuredJoystick, params Key[] configuredKeys)
        {
            sourceType = configuredSourceType;
            deviceIndex = Mathf.Max(0, configuredDeviceIndex);
            preferredGamepad = configuredPreferredGamepad?.Trim() ?? string.Empty;
            joystickConfig = configuredJoystick ?? new JoystickInputConfig();
            keyboardKeys = configuredKeys ?? Array.Empty<Key>();
        }

        private void Update()
        {
            resolvedDevice = ResolveDevice();
            IsHeld = false;
            WasPressedThisFrame = false;
            WasBackPressedThisFrame = false;
            if (resolvedDevice == null) return;

            if (sourceType == RhythmInputSourceType.Keyboard)
            {
                var keyboard = resolvedDevice as Keyboard;
                if (keyboard == null) return;
                WasBackPressedThisFrame = keyboard.bKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame;
                for (var index = 0; index < keyboardKeys.Length; index++)
                {
                    var key = keyboard[keyboardKeys[index]];
                    IsHeld |= key.isPressed;
                    WasPressedThisFrame |= key.wasPressedThisFrame;
                }
                return;
            }

            if (resolvedDevice is Gamepad gamepad)
            {
                WasBackPressedThisFrame = gamepad.buttonEast.wasPressedThisFrame;
                ReadGamepad(gamepad);
                return;
            }

            WasBackPressedThisFrame = WasConfiguredJoystickBackPressed(resolvedDevice);

            foreach (var control in resolvedDevice.allControls)
            {
                if (!(control is ButtonControl button)) continue;
                IsHeld |= button.isPressed;
                WasPressedThisFrame |= button.wasPressedThisFrame;
            }
        }

        private void ReadGamepad(Gamepad gamepad)
        {
            ReadButton(gamepad.buttonWest);
            ReadButton(gamepad.buttonSouth);
            ReadButton(gamepad.buttonEast);
            ReadButton(gamepad.buttonNorth);
            ReadButton(gamepad.dpad.left);
            ReadButton(gamepad.dpad.right);
            ReadButton(gamepad.dpad.up);
            ReadButton(gamepad.dpad.down);
            ReadButton(gamepad.leftStick.left);
            ReadButton(gamepad.leftStick.right);
            ReadButton(gamepad.leftStick.up);
            ReadButton(gamepad.leftStick.down);
            ReadButton(gamepad.startButton);
            ReadButton(gamepad.selectButton);
            ReadButton(gamepad.leftShoulder);
            ReadButton(gamepad.rightShoulder);
            ReadButton(gamepad.leftStickButton);
            ReadButton(gamepad.rightStickButton);
        }

        private void ReadButton(ButtonControl button)
        {
            if (button == null) return;
            IsHeld |= button.isPressed;
            WasPressedThisFrame |= button.wasPressedThisFrame;
        }

        private bool WasConfiguredJoystickBackPressed(InputDevice device)
        {
            var paths=joystickConfig?.rightButtonPaths;
            if (paths==null) return false;
            for (var index=0;index<paths.Length;index++)
            {
                if (string.IsNullOrWhiteSpace(paths[index])) continue;
                var button=device.TryGetChildControl<ButtonControl>(paths[index].Trim());
                if (button!=null && button.wasPressedThisFrame) return true;
            }
            return false;
        }

        private InputDevice ResolveDevice()
        {
            if (sourceType == RhythmInputSourceType.Keyboard) return Keyboard.current;
            if (sourceType == RhythmInputSourceType.Joystick) return ResolveJoystick();
            return ResolveGamepad();
        }

        private Gamepad ResolveGamepad()
        {
            if (!string.IsNullOrWhiteSpace(preferredGamepad))
            {
                for (var index = 0; index < Gamepad.all.Count; index++)
                {
                    var candidate = Gamepad.all[index];
                    if (MatchesExact(candidate.name, preferredGamepad) ||
                        MatchesExact(candidate.layout, preferredGamepad) ||
                        MatchesExact(candidate.displayName, preferredGamepad) ||
                        MatchesExact(candidate.description.product, preferredGamepad))
                        return candidate;
                }
            }

            return deviceIndex < Gamepad.all.Count ? Gamepad.all[deviceIndex] : null;
        }

        private Joystick ResolveJoystick()
        {
            var product = joystickConfig?.deviceProduct?.Trim();
            var manufacturer = joystickConfig?.deviceManufacturer?.Trim();
            if (!string.IsNullOrWhiteSpace(product) || !string.IsNullOrWhiteSpace(manufacturer))
            {
                for (var index = 0; index < Joystick.all.Count; index++)
                {
                    var candidate = Joystick.all[index];
                    var productMatches = string.IsNullOrWhiteSpace(product) ||
                                         ContainsEitherWay(candidate.description.product, product) ||
                                         ContainsEitherWay(candidate.displayName, product) ||
                                         ContainsEitherWay(candidate.layout, product);
                    var actualManufacturer = candidate.description.manufacturer;
                    var manufacturerMatches = string.IsNullOrWhiteSpace(manufacturer) ||
                                              string.IsNullOrWhiteSpace(actualManufacturer) ||
                                              ContainsEitherWay(actualManufacturer, manufacturer);
                    if (productMatches && manufacturerMatches) return candidate;
                }
                return null;
            }

            var indexToUse = Mathf.Max(0, joystickConfig?.joystickIndex ?? deviceIndex);
            return indexToUse < Joystick.all.Count ? Joystick.all[indexToUse] : null;
        }

        private static bool MatchesExact(string actual, string expected)
        {
            return !string.IsNullOrWhiteSpace(actual) &&
                   string.Equals(actual.Trim(), expected, StringComparison.OrdinalIgnoreCase);
        }

        private static bool ContainsEitherWay(string first, string second)
        {
            if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(second)) return false;
            return first.IndexOf(second, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   second.IndexOf(first, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
