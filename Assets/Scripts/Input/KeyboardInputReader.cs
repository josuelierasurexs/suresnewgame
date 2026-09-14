using System;
using Surexs.DanceOff.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Surexs.DanceOff.Input
{
    [DefaultExecutionOrder(-200)]
    public sealed class KeyboardInputReader : MonoBehaviour, IRhythmInputSource
    {
        [SerializeField] private Key leftKey = Key.A;
        [SerializeField] private Key centerPrimaryKey = Key.W;
        [SerializeField] private Key centerSecondaryKey = Key.S;
        [SerializeField] private Key rightKey = Key.D;
        private bool warnedAboutMissingKeyboard;

        public event Action<RhythmDirection> DirectionPressed;

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                if (!warnedAboutMissingKeyboard)
                {
                    warnedAboutMissingKeyboard = true;
                    Debug.LogWarning("[KeyboardInputReader] No hay teclado disponible; no se emitirán acciones de ritmo.", this);
                }

                return;
            }

            var leftControl = keyboard[leftKey];
            var centerPrimaryControl = keyboard[centerPrimaryKey];
            var centerSecondaryControl = keyboard[centerSecondaryKey];
            var rightControl = keyboard[rightKey];

            if (leftControl.wasPressedThisFrame)
            {
                DirectionPressed?.Invoke(RhythmDirection.Left);
            }

            if (centerPrimaryControl.wasPressedThisFrame || centerSecondaryControl.wasPressedThisFrame)
            {
                DirectionPressed?.Invoke(RhythmDirection.Center);
            }

            if (rightControl.wasPressedThisFrame)
            {
                DirectionPressed?.Invoke(RhythmDirection.Right);
            }
        }

        public void Configure(Key configuredLeftKey, Key configuredCenterPrimaryKey,
            Key configuredCenterSecondaryKey, Key configuredRightKey)
        {
            leftKey = configuredLeftKey;
            centerPrimaryKey = configuredCenterPrimaryKey;
            centerSecondaryKey = configuredCenterSecondaryKey;
            rightKey = configuredRightKey;
        }
    }
}
