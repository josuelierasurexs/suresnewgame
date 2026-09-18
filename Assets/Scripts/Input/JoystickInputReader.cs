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
                    var target = string.IsNullOrWhiteSpace(config.deviceProduct)
                        ? $"con índice {config.joystickIndex}"
                        : $"'{config.deviceProduct}'";
                    Debug.LogWarning($"[JoystickInputReader] No se encontró el joystick HID {target}; no se emitirán acciones de ritmo.", this);
                    LogAvailableJoysticks();
                }
                ResolveBindings(null);
                return;
            }

            warnedAboutMissingJoystick = false;
            if (resolvedJoystick != joystick) ResolveBindings(joystick);

            var useVectorFallbacks = !config.useExplicitDirectionalPathsOnly;
            var stick = useVectorFallbacks && joystick.stick != null ? joystick.stick.ReadValue() : Vector2.zero;
            var hat = useVectorFallbacks && joystick.hatswitch != null ? joystick.hatswitch.ReadValue() : Vector2.zero;
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
            if (!string.IsNullOrWhiteSpace(config.deviceProduct) ||
                !string.IsNullOrWhiteSpace(config.deviceManufacturer))
            {
                for (var index = 0; index < Joystick.all.Count; index++)
                {
                    var candidate = Joystick.all[index];
                    if (MatchesDescription(candidate, config.deviceProduct, config.deviceManufacturer))
                    {
                        return candidate;
                    }
                }

                return null;
            }

            return config.joystickIndex < Joystick.all.Count ? Joystick.all[config.joystickIndex] : null;
        }

        private static bool MatchesDescription(Joystick joystick, string product, string manufacturer)
        {
            var expectedProduct = product?.Trim();
            var expectedManufacturer = manufacturer?.Trim();
            var productMatches = string.IsNullOrWhiteSpace(expectedProduct) ||
                                 ContainsEitherWay(joystick.description.product, expectedProduct) ||
                                 ContainsEitherWay(joystick.displayName, expectedProduct) ||
                                 ContainsEitherWay(joystick.layout, expectedProduct);
            var actualManufacturer = joystick.description.manufacturer;
            var manufacturerMatches = string.IsNullOrWhiteSpace(expectedManufacturer) ||
                                      string.IsNullOrWhiteSpace(actualManufacturer) ||
                                      ContainsEitherWay(actualManufacturer, expectedManufacturer);
            return productMatches && manufacturerMatches;
        }

        private static bool ContainsEitherWay(string first, string second)
        {
            if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(second)) return false;
            return first.IndexOf(second, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   second.IndexOf(first, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void LogAvailableJoysticks()
        {
            if (Joystick.all.Count == 0)
            {
                Debug.LogWarning("[JoystickInputReader] Input System no reporta ningún dispositivo Joystick.", this);
                return;
            }

            var descriptions = new string[Joystick.all.Count];
            for (var index = 0; index < Joystick.all.Count; index++)
            {
                var joystick = Joystick.all[index];
                descriptions[index] = $"#{index}: manufacturer='{joystick.description.manufacturer}', " +
                                      $"product='{joystick.description.product}', displayName='{joystick.displayName}', " +
                                      $"layout='{joystick.layout}', deviceId={joystick.deviceId}";
            }

            Debug.LogWarning("[JoystickInputReader] Joysticks disponibles: " + string.Join(" | ", descriptions), this);
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
                      $"(layout {joystick.layout}, deviceId {joystick.deviceId}). Bindings: " +
                      $"LEFT={ButtonPaths(leftButtons)}, CENTER={ButtonPaths(centerButtons)}, " +
                      $"RIGHT={ButtonPaths(rightButtons)}.", this);
        }

        private static string ButtonPaths(List<ButtonControl> controls)
        {
            if (controls.Count == 0) return "NINGUNO";
            var paths = new string[controls.Count];
            for (var index = 0; index < controls.Count; index++) paths[index] = controls[index].path;
            return string.Join(", ", paths);
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
