using System;
using System.Collections.Generic;
using Surexs.DanceOff.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Surexs.DanceOff.Input
{
    [DefaultExecutionOrder(-200)]
    public sealed class JoystickInputReader : MonoBehaviour, IRhythmInputSource
    {
        private readonly List<ButtonControl> leftButtons = new List<ButtonControl>();
        private readonly List<ButtonControl> centerButtons = new List<ButtonControl>();
        private readonly List<ButtonControl> rightButtons = new List<ButtonControl>();
        private readonly Dictionary<AxisControl, float> diagnosticAxisValues = new Dictionary<AxisControl, float>();
        private JoystickInputConfig config;
        private Joystick resolvedJoystick;
        private bool warnedAboutMissingJoystick;
        private bool stickLeftActive;
        private bool stickCenterActive;
        private bool stickRightActive;
        private bool hatLeftActive;
        private bool hatCenterActive;
        private bool hatRightActive;

        public event Action<RhythmDirection> DirectionPressed;

        public void Configure(JoystickInputConfig configured)
        {
            config = configured ?? new JoystickInputConfig();
            ResolveBindings(null);
        }

        private void Update()
        {
            if (config == null) config = new JoystickInputConfig();
            var joystick = ResolveJoystick();
            if (joystick == null)
            {
                if (!warnedAboutMissingJoystick)
                {
                    warnedAboutMissingJoystick = true;
                    Debug.LogWarning($"[JoystickInputReader] No se encontró el joystick HID {config.joystickIndex + 1}; no se emitirán acciones de ritmo.", this);
                }
                ResolveBindings(null);
                return;
            }

            warnedAboutMissingJoystick = false;
            if (resolvedJoystick != joystick) ResolveBindings(joystick);

            var stick = joystick.stick != null ? joystick.stick.ReadValue() : Vector2.zero;
            var hat = joystick.hatswitch != null ? joystick.hatswitch.ReadValue() : Vector2.zero;
            var threshold = Mathf.Clamp(config.axisThreshold, 0.1f, 0.95f);

            var nextStickLeft = stick.x <= -threshold;
            var nextStickRight = stick.x >= threshold;
            var nextStickCenter = Mathf.Abs(stick.y) >= threshold;
            var nextHatLeft = hat.x <= -threshold;
            var nextHatRight = hat.x >= threshold;
            var nextHatCenter = Mathf.Abs(hat.y) >= threshold;

            var stickLeftPressed=Rising(nextStickLeft, ref stickLeftActive);
            var stickCenterPressed=Rising(nextStickCenter, ref stickCenterActive);
            var stickRightPressed=Rising(nextStickRight, ref stickRightActive);
            var hatLeftPressed=Rising(nextHatLeft, ref hatLeftActive);
            var hatCenterPressed=Rising(nextHatCenter, ref hatCenterActive);
            var hatRightPressed=Rising(nextHatRight, ref hatRightActive);
            var leftPressed = WasAnyPressed(leftButtons) || stickLeftPressed || hatLeftPressed;
            var centerPressed = WasAnyPressed(centerButtons) || stickCenterPressed || hatCenterPressed;
            var rightPressed = WasAnyPressed(rightButtons) || stickRightPressed || hatRightPressed;

            if (leftPressed) DirectionPressed?.Invoke(RhythmDirection.Left);
            if (centerPressed) DirectionPressed?.Invoke(RhythmDirection.Center);
            if (rightPressed) DirectionPressed?.Invoke(RhythmDirection.Right);

            if (config.diagnosticLogging) LogRelevantChanges(joystick, threshold);
        }

        private Joystick ResolveJoystick()
        {
            return config.joystickIndex < Joystick.all.Count ? Joystick.all[config.joystickIndex] : null;
        }

        private void ResolveBindings(Joystick joystick)
        {
            resolvedJoystick = joystick;
            leftButtons.Clear(); centerButtons.Clear(); rightButtons.Clear(); diagnosticAxisValues.Clear();
            stickLeftActive = stickCenterActive = stickRightActive = false;
            hatLeftActive = hatCenterActive = hatRightActive = false;
            if (joystick == null || config == null) return;
            ResolveButtons(joystick, config.leftButtonPaths, leftButtons);
            ResolveButtons(joystick, config.centerButtonPaths, centerButtons);
            ResolveButtons(joystick, config.rightButtonPaths, rightButtons);
            Debug.Log($"[JoystickInputReader] Usando {joystick.description.manufacturer} {joystick.description.product} " +
                      $"(layout {joystick.layout}, índice {config.joystickIndex}).", this);
        }

        private void ResolveButtons(Joystick joystick, string[] paths, List<ButtonControl> destination)
        {
            if (paths == null) return;
            for (var i = 0; i < paths.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(paths[i])) continue;
                var control = joystick.TryGetChildControl<ButtonControl>(paths[i].Trim());
                if (control != null) destination.Add(control);
                else Debug.LogWarning($"[JoystickInputReader] El control '{paths[i]}' no existe en {joystick.displayName}.", this);
            }
        }

        private static bool WasAnyPressed(List<ButtonControl> controls)
        {
            for (var i = 0; i < controls.Count; i++)
                if (controls[i].wasPressedThisFrame) return true;
            return false;
        }

        private static bool Rising(bool next, ref bool previous)
        {
            var pressed = next && !previous;
            previous = next;
            return pressed;
        }

        private void LogRelevantChanges(Joystick joystick, float threshold)
        {
            foreach (var control in joystick.allControls)
            {
                if (control is ButtonControl button)
                {
                    if (button.wasPressedThisFrame)
                        Debug.Log($"[JoystickInputReader:DEBUG] control={button.path} value={button.ReadValue():0.###}", this);
                    continue;
                }
                if (!(control is AxisControl axis)) continue;
                var value = axis.ReadValue();
                diagnosticAxisValues.TryGetValue(axis, out var previous);
                if (Mathf.Abs(value - previous) >= 0.25f &&
                    (Mathf.Abs(value) >= threshold || Mathf.Abs(previous) >= threshold))
                    Debug.Log($"[JoystickInputReader:DEBUG] control={axis.path} value={value:0.###}", this);
                diagnosticAxisValues[axis] = value;
            }
        }
    }
}
