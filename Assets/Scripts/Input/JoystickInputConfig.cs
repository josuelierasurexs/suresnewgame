using System;
using UnityEngine;

namespace Surexs.DanceOff.Input
{
    [Serializable]
    public sealed class JoystickInputConfig
    {
        [Min(0)] public int joystickIndex;
        [Range(0.1f, 0.95f)] public float axisThreshold = 0.5f;
        [Tooltip("Paths relativos al Joystick, por ejemplo 'trigger' o el nombre observado en Input Debugger.")]
        public string[] leftButtonPaths = Array.Empty<string>();
        public string[] centerButtonPaths = Array.Empty<string>();
        public string[] rightButtonPaths = Array.Empty<string>();
        [Tooltip("Registra solamente pulsaciones y cruces relevantes para descubrir controles HID.")]
        public bool diagnosticLogging;
    }
}
