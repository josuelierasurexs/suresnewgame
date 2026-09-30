using System;
using Surexs.DanceOff.Data;
using Surexs.DanceOff.Rhythm;
using UnityEngine;

namespace Surexs.DanceOff.Core
{
    public sealed class RhythmPrototypeController : MonoBehaviour
    {
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private ChartManager chartManager;
        [SerializeField] private TileSpawner[] tileSpawners;
        [SerializeField] private RhythmJudge[] rhythmJudges;

        private bool sessionRunning;
        private bool completed;
        private bool paused;

        public event Action ChartCompleted;
        public bool IsCompleted => completed;
        public bool IsPaused => paused;

        public void Configure(AudioManager audio, ChartManager chart, TileSpawner spawner, RhythmJudge judge,
            RhythmGameplayConfig gameplayConfig)
        {
            Configure(audio, chart, new[] { spawner }, new[] { judge }, gameplayConfig);
        }

        public void Configure(AudioManager audio, ChartManager chart, TileSpawner[] spawners, RhythmJudge[] judges,
            RhythmGameplayConfig gameplayConfig)
        {
            audioManager = audio;
            chartManager = chart;
            tileSpawners = spawners;
            rhythmJudges = judges;
        }

        public bool StartSession()
        {
            audioManager.StopMusic();
            sessionRunning = false;
            paused = false;
            if (!chartManager.LoadChart())
            {
                Debug.LogError("[RhythmPrototype] No se inició el prototipo porque el chart no es válido.", this);
                return false;
            }

            completed = false;
            for (var index = 0; index < rhythmJudges.Length; index++)
            {
                rhythmJudges[index].Begin(chartManager.Events);
            }

            for (var index = 0; index < tileSpawners.Length; index++)
            {
                tileSpawners[index].Begin();
            }
            if (!audioManager.PlayFromStart())
            {
                Debug.LogWarning("[RhythmPrototype] El chart se cargó, pero no avanzará sin un AudioClip.", this);
                return false;
            }

            sessionRunning = true;
            var events = chartManager.Events;
            var lastEventTime = events[events.Count - 1].Time;
            if (lastEventTime > audioManager.DurationSeconds)
            {
                Debug.LogWarning($"[RhythmPrototype] El último evento ({lastEventTime:F2} s) está fuera del clip ({audioManager.DurationSeconds:F2} s).", this);
            }
            return true;
        }

        public void StopSession()
        {
            sessionRunning = false;
            paused = false;
            audioManager.StopMusic();
            for (var index=0; index<rhythmJudges.Length; index++) rhythmJudges[index].StopJudging();
            for (var index=0; index<tileSpawners.Length; index++) tileSpawners[index].StopSpawning();
        }

        public bool PauseSession()
        {
            if (!sessionRunning || completed || paused || !audioManager.PauseMusic()) return false;
            paused=true;
            return true;
        }

        public bool ResumeSession()
        {
            if (!sessionRunning || completed || !paused || !audioManager.ResumeMusic()) return false;
            paused=false;
            return true;
        }

        private void Update()
        {
            if (completed || !sessionRunning || paused || audioManager.IsPlaying)
            {
                return;
            }

            sessionRunning = false;
            completed = true;
            ChartCompleted?.Invoke();
            Debug.Log($"[RhythmPrototype] Canción terminada en {audioManager.DurationSeconds:F3} s. Gameplay en espera.", this);
        }
    }
}
