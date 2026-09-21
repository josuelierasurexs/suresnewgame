using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Surexs.DanceOff.Player
{
    /// <summary>
    /// Reproduce las secuencias visuales finales de un personaje.
    /// No participa en el timing, el judge ni el score.
    /// </summary>
    public sealed class SoloCharacterAnimationView : MonoBehaviour
    {
        private const float BaseFrameDuration = 0.04f;
        private const string Neutral = "neutral";
        private const string Combo = "combo";
        private const string Miss = "miss";
        private static readonly string[] SupportedStates =
            { Neutral, "phone", "laptop", "tablet", Combo, Miss };

        private readonly Dictionary<string, Texture2D[]> clips =
            new Dictionary<string, Texture2D[]>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> missingClipWarnings =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private RawImage target;
        private RawImage glowTarget;
        private Texture2D[] activeFrames = Array.Empty<Texture2D>();
        private string activeState = Neutral;
        private string requestedLoopState = Neutral;
        private float requestedLoopSpeed = 1f;
        private float playbackSpeed = 1f;
        private float frameTime;
        private int frameIndex;
        private PlaybackMode playbackMode;
        private int pendingCombos;
        private float pendingComboSpeed = 1f;
        private bool pendingMiss;

        public string CurrentState => activeState;
        public float PlaybackSpeed => playbackSpeed;

        public void Configure(RawImage characterImage, string resourcesPath = "Animations/Player1",
            RawImage characterGlow = null)
        {
            target = characterImage;
            glowTarget = characterGlow;
            clips.Clear();
            for (var index = 0; index < SupportedStates.Length; index++)
            {
                var state = SupportedStates[index];
                var frames = Resources.LoadAll<Texture2D>($"{resourcesPath}/{state}");
                Array.Sort(frames, (left, right) => string.CompareOrdinal(left.name, right.name));
                clips[state] = frames;
                if (frames.Length == 0) WarnMissingClip(state);
            }

            ResetVisual();
        }

        public void PlayPose(string poseId, int multiplier)
        {
            var resolved = ResolveLoopState(poseId);
            var speed = SpeedForMultiplier(multiplier);
            if (playbackMode == PlaybackMode.MissOneShot) return;

            requestedLoopState = resolved;
            requestedLoopSpeed = speed;
            if (playbackMode == PlaybackMode.ComboOneShot) return;

            if (playbackMode == PlaybackMode.Loop &&
                string.Equals(activeState, resolved, StringComparison.OrdinalIgnoreCase))
            {
                playbackSpeed = speed;
                return;
            }

            StartClip(resolved, PlaybackMode.Loop, speed);
        }

        public void PlayCombo(int multiplier)
        {
            var speed = SpeedForMultiplier(multiplier);
            if (playbackMode == PlaybackMode.MissOneShot) return;
            if (playbackMode == PlaybackMode.ComboOneShot)
            {
                pendingCombos++;
                pendingComboSpeed = speed;
                return;
            }

            StartClip(Combo, PlaybackMode.ComboOneShot, speed);
        }

        public void PlayMiss()
        {
            requestedLoopState = Neutral;
            requestedLoopSpeed = 1f;
            pendingCombos = 0;
            if (playbackMode == PlaybackMode.ComboOneShot)
            {
                pendingMiss = true;
                return;
            }

            pendingMiss = false;
            StartClip(Miss, PlaybackMode.MissOneShot, 1f);
        }

        public void ResetVisual()
        {
            pendingCombos = 0;
            pendingMiss = false;
            requestedLoopState = Neutral;
            requestedLoopSpeed = 1f;
            StartClip(Neutral, PlaybackMode.Loop, 1f);
        }

        private void Update()
        {
            if (activeFrames.Length == 0) return;
            frameTime += Time.unscaledDeltaTime * playbackSpeed;
            while (frameTime >= BaseFrameDuration)
            {
                frameTime -= BaseFrameDuration;
                frameIndex++;
                if (frameIndex < activeFrames.Length)
                {
                    ShowFrame(activeFrames[frameIndex]);
                    continue;
                }

                if (playbackMode == PlaybackMode.Loop)
                {
                    frameIndex = 0;
                    ShowFrame(activeFrames[0]);
                    continue;
                }

                CompleteOneShot();
                break;
            }
        }

        private void CompleteOneShot()
        {
            if (playbackMode == PlaybackMode.ComboOneShot)
            {
                if (pendingMiss)
                {
                    pendingMiss = false;
                    StartClip(Miss, PlaybackMode.MissOneShot, 1f);
                    return;
                }

                if (pendingCombos > 0)
                {
                    pendingCombos--;
                    StartClip(Combo, PlaybackMode.ComboOneShot, pendingComboSpeed);
                    return;
                }

                StartClip(requestedLoopState, PlaybackMode.Loop, requestedLoopSpeed);
                return;
            }

            StartClip(Neutral, PlaybackMode.Loop, 1f);
        }

        private void StartClip(string state, PlaybackMode mode, float speed)
        {
            if (!clips.TryGetValue(state, out var frames) || frames.Length == 0)
            {
                WarnMissingClip(state);
                if (!clips.TryGetValue(Neutral, out frames) || frames.Length == 0) return;
                state = Neutral;
                mode = PlaybackMode.Loop;
                speed = 1f;
            }

            activeState = state;
            playbackMode = mode;
            playbackSpeed = Mathf.Clamp(speed, 1f, 2f);
            activeFrames = frames;
            frameIndex = 0;
            frameTime = 0f;
            if (target != null)
            {
                target.color = Color.white;
                target.enabled = true;
            }
            if (glowTarget != null) glowTarget.enabled = true;
            ShowFrame(frames[0]);
        }

        private void ShowFrame(Texture2D frame)
        {
            if (target != null) target.texture = frame;
            if (glowTarget != null) glowTarget.texture = frame;
        }

        private string ResolveLoopState(string poseId)
        {
            if (string.Equals(poseId, "phone", StringComparison.OrdinalIgnoreCase)) return "phone";
            if (string.Equals(poseId, "laptop", StringComparison.OrdinalIgnoreCase)) return "laptop";
            if (string.Equals(poseId, "tablet", StringComparison.OrdinalIgnoreCase)) return "tablet";
            return Neutral;
        }

        private static float SpeedForMultiplier(int multiplier)
        {
            return Mathf.Clamp(1f + (Mathf.Max(1, multiplier) - 1) * .25f, 1f, 2f);
        }

        private void WarnMissingClip(string state)
        {
            if (missingClipWarnings.Add(state))
                Debug.LogWarning($"[CharacterAnimationView] No se encontraron frames para '{state}'.", this);
        }

        private enum PlaybackMode
        {
            Loop,
            ComboOneShot,
            MissOneShot
        }
    }
}
