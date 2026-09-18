using System;
using UnityEngine;

namespace Surexs.DanceOff.Input
{
    [Serializable]
    public sealed class JoystickInputConfig
    {
        [Min(0)] public int joystickIndex;
        [Tooltip("Producto HID exacto. Si se configura, se busca por descripción en vez de depender del índice.")]
        public string deviceProduct = string.Empty;
        [Tooltip("Fabricante HID exacto opcional para desambiguar dispositivos con el mismo producto.")]
        public string deviceManufacturer = string.Empty;
        [Tooltip("Usa solamente los paths configurados y desactiva los fallbacks de stick/hat vectorial.")]
        public bool useExplicitDirectionalPathsOnly;
        [Range(0.1f, 0.95f)] public float axisThreshold = 0.5f;
        [Tooltip("Paths relativos al Joystick, por ejemplo 'trigger' o el nombre observado en Input Debugger.")]
        public string[] leftButtonPaths = Array.Empty<string>();
        public string[] centerButtonPaths = Array.Empty<string>();
        public string[] rightButtonPaths = Array.Empty<string>();
        [Tooltip("Registra solamente pulsaciones y cruces relevantes para descubrir controles HID.")]
        public bool diagnosticLogging;
    }
}
