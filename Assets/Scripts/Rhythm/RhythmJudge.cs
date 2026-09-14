using System;
using System.Collections.Generic;
using Surexs.DanceOff.Core;
using Surexs.DanceOff.Data;
using Surexs.DanceOff.Input;
using UnityEngine;

namespace Surexs.DanceOff.Rhythm
{
    [DefaultExecutionOrder(-100)]
    public sealed class RhythmJudge : MonoBehaviour
    {
        private readonly List<JudgedNote> notes = new List<JudgedNote>();
        private AudioManager audioManager;
        private IRhythmInputSource inputReader;
        private RhythmGameplayConfig config;
        private bool isRunning;

        public event Action<RhythmJudgment> NoteJudged;

        public void Configure(AudioManager audio, IRhythmInputSource input, RhythmGameplayConfig gameplayConfig)
        {
            DisconnectInput();
            audioManager = audio;
            inputReader = input;
            config = gameplayConfig;
            inputReader.DirectionPressed += OnDirectionPressed;
        }

        public void Begin(IReadOnlyList<ChartEvent> chartEvents)
        {
            notes.Clear();
            for (var index = 0; index < chartEvents.Count; index++)
            {
                notes.Add(new JudgedNote(chartEvents[index]));
            }

            isRunning = true;
        }

        public void StopJudging()
        {
            isRunning = false;
        }

        private void Update()
        {
            if (!isRunning || audioManager == null || !audioManager.IsPlaying)
            {
                return;
            }

            var songTime = audioManager.SongTimeSeconds;
            for (var index = 0; index < notes.Count; index++)
            {
                var note = notes[index];
                if (note.Status != RhythmNoteStatus.Pending)
                {
                    continue;
                }

                if (songTime > note.ChartEvent.Time + config.goodWindow)
                {
                    Complete(note, RhythmJudgmentResult.Miss, songTime - note.ChartEvent.Time);
                }
            }
        }

        private void OnDirectionPressed(RhythmDirection inputDirection)
        {
            if (!isRunning || audioManager == null || !audioManager.IsPlaying)
            {
                return;
            }

            var songTime = audioManager.SongTimeSeconds;
            JudgedNote closest = null;
            var closestDistance = double.MaxValue;

            for (var index = 0; index < notes.Count; index++)
            {
                var candidate = notes[index];
                if (candidate.Status != RhythmNoteStatus.Pending)
                {
                    continue;
                }

                var distance = Math.Abs(songTime - candidate.ChartEvent.Time);
                if (distance <= config.goodWindow && distance < closestDistance)
                {
                    closest = candidate;
                    closestDistance = distance;
                }
            }

            if (closest == null)
            {
                CompleteInputMiss(inputDirection);
                return;
            }

            JudgeNote(closest, inputDirection, songTime);
        }

        private void JudgeNote(JudgedNote note, RhythmDirection inputDirection, double songTime)
        {
            var delta = songTime - note.ChartEvent.Time;
            var result = inputDirection == note.ChartEvent.Direction
                ? ClassifyTiming(delta, config)
                : RhythmJudgmentResult.Miss;
            Complete(note, result, delta);
        }

        private void CompleteInputMiss(RhythmDirection inputDirection)
        {
            Debug.Log($"[RhythmJudge] {inputDirection} sin nota registrable => MISS.", this);
            NoteJudged?.Invoke(RhythmJudgment.InputMiss(inputDirection));
        }

        private void Complete(JudgedNote note, RhythmJudgmentResult result, double delta)
        {
            if (note.Status != RhythmNoteStatus.Pending)
            {
                return;
            }

            note.Status = result == RhythmJudgmentResult.Miss ? RhythmNoteStatus.Missed : RhythmNoteStatus.Hit;
            Debug.Log($"[RhythmJudge] {note.ChartEvent.Direction} @ {note.ChartEvent.Time:F3} s => " +
                      $"{result.ToString().ToUpperInvariant()} (delta {delta:+0.000;-0.000;0.000} s).", this);
            NoteJudged?.Invoke(new RhythmJudgment(note.ChartEvent, result, delta));
        }

        public static RhythmJudgmentResult ClassifyTiming(double deltaSeconds, RhythmGameplayConfig gameplayConfig)
        {
            var absoluteDelta = Math.Abs(deltaSeconds);
            if (absoluteDelta <= gameplayConfig.perfectWindow)
            {
                return RhythmJudgmentResult.Perfect;
            }

            if (absoluteDelta <= gameplayConfig.greatWindow)
            {
                return RhythmJudgmentResult.Great;
            }

            if (absoluteDelta <= gameplayConfig.goodWindow)
            {
                return RhythmJudgmentResult.Good;
            }

            return RhythmJudgmentResult.Miss;
        }

        private void OnDestroy()
        {
            DisconnectInput();
        }

        private void DisconnectInput()
        {
            if (inputReader != null)
            {
                inputReader.DirectionPressed -= OnDirectionPressed;
            }
        }

        private sealed class JudgedNote
        {
            public JudgedNote(ChartEvent chartEvent)
            {
                ChartEvent = chartEvent;
            }

            public ChartEvent ChartEvent { get; }
            public RhythmNoteStatus Status { get; set; } = RhythmNoteStatus.Pending;
        }
    }
}
