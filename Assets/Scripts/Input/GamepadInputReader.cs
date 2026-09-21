using System;
using Surexs.DanceOff.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Surexs.DanceOff.Input
{
    [DefaultExecutionOrder(-200)]
    public sealed class GamepadInputReader : MonoBehaviour, IRhythmInputSource
    {
        [SerializeField, Min(0)] private int gamepadIndex;
        [SerializeField, Range(0.1f, 0.95f)] private float axisThreshold = 0.5f;
        [SerializeField] private string preferredDevice;
        private bool warnedAboutMissingGamepad;
        private bool stickLeftActive;
        private bool stickCenterActive;
        private bool stickRightActive;
        private Gamepad resolvedGamepad;

        public event Action<RhythmDirection> DirectionPressed;

        public void Configure(int configuredGamepadIndex, float configuredAxisThreshold = 0.5f,
            string configuredPreferredDevice = "")
        {
            gamepadIndex = Mathf.Max(0, configuredGamepadIndex);
            axisThreshold = Mathf.Clamp(configuredAxisThreshold, 0.1f, 0.95f);
            preferredDevice = configuredPreferredDevice?.Trim() ?? string.Empty;
        }

        private void Update()
        {
            var gamepad = ResolveGamepad();
            if (gamepad == null)
            {
                if (!warnedAboutMissingGamepad)
                {
                    warnedAboutMissingGamepad = true;
                    var target = string.IsNullOrWhiteSpace(preferredDevice)
                        ? $"con índice {gamepadIndex}"
                        : $"'{preferredDevice}'";
                    Debug.LogWarning($"[GamepadInputReader] No se encontró el gamepad {target}; no se emitirán acciones de ritmo.", this);
                }

                resolvedGamepad = null;
                stickLeftActive = stickCenterActive = stickRightActive = false;
                return;
            }

            warnedAboutMissingGamepad = false;
            if (resolvedGamepad != gamepad)
            {
                resolvedGamepad = gamepad;
                stickLeftActive = stickCenterActive = stickRightActive = false;
                Debug.Log($"[GamepadInputReader] Usando '{gamepad.displayName}' " +
                          $"(layout {gamepad.layout}, deviceId {gamepad.deviceId}).", this);
            }
            var stick=gamepad.leftStick.ReadValue();
            var nextLeft=stick.x<=-axisThreshold;
            var nextCenter=Mathf.Abs(stick.y)>=axisThreshold;
            var nextRight=stick.x>=axisThreshold;
            var stickLeftPressed=Rising(nextLeft,ref stickLeftActive);
            var stickCenterPressed=Rising(nextCenter,ref stickCenterActive);
            var stickRightPressed=Rising(nextRight,ref stickRightActive);
            var leftPressed=gamepad.buttonWest.wasPressedThisFrame || gamepad.dpad.left.wasPressedThisFrame || stickLeftPressed;
            var centerPressed=gamepad.buttonSouth.wasPressedThisFrame || gamepad.buttonNorth.wasPressedThisFrame ||
                              gamepad.dpad.up.wasPressedThisFrame ||
                              gamepad.dpad.down.wasPressedThisFrame || stickCenterPressed;
            var rightPressed=gamepad.buttonEast.wasPressedThisFrame || gamepad.dpad.right.wasPressedThisFrame || stickRightPressed;
            if (leftPressed) DirectionPressed?.Invoke(RhythmDirection.Left);
            if (centerPressed) DirectionPressed?.Invoke(RhythmDirection.Center);
            if (rightPressed) DirectionPressed?.Invoke(RhythmDirection.Right);
        }

        private Gamepad ResolveGamepad()
        {
            if (!string.IsNullOrWhiteSpace(preferredDevice))
            {
                for (var index = 0; index < Gamepad.all.Count; index++)
                {
                    var candidate = Gamepad.all[index];
                    if (MatchesExact(candidate.name, preferredDevice) ||
                        MatchesExact(candidate.layout, preferredDevice) ||
                        MatchesExact(candidate.displayName, preferredDevice) ||
                        MatchesExact(candidate.description.product, preferredDevice))
                        return candidate;
                }
            }

            return gamepadIndex < Gamepad.all.Count ? Gamepad.all[gamepadIndex] : null;
        }

        private static bool MatchesExact(string actual, string expected)
        {
            if (string.IsNullOrWhiteSpace(actual) || string.IsNullOrWhiteSpace(expected)) return false;
            return string.Equals(actual.Trim(), expected.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private static bool Rising(bool next, ref bool previous)
        {
            var pressed=next&&!previous;
            previous=next;
            return pressed;
        }
    }
}
