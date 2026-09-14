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

        private double chartEndTime;
        private double completionGrace;
        private bool completed;

        public event Action ChartCompleted;
        public bool IsCompleted => completed;

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
            completionGrace = gameplayConfig.goodWindow;
        }

        public bool StartSession()
        {
            audioManager.StopMusic();
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

            var events = chartManager.Events;
            var lastEventTime = events[events.Count - 1].Time;
            chartEndTime = lastEventTime + completionGrace;
            if (lastEventTime > audioManager.DurationSeconds)
            {
                Debug.LogWarning($"[RhythmPrototype] El último evento ({lastEventTime:F2} s) está fuera del clip ({audioManager.DurationSeconds:F2} s).", this);
            }
            return true;
        }

        public void StopSession()
        {
            audioManager.StopMusic();
            for (var index=0; index<rhythmJudges.Length; index++) rhythmJudges[index].StopJudging();
            for (var index=0; index<tileSpawners.Length; index++) tileSpawners[index].StopSpawning();
        }

        private void Update()
        {
            if (completed || !audioManager.IsPlaying || audioManager.SongTimeSeconds <= chartEndTime)
            {
                return;
            }

            completed = true;
            audioManager.StopMusic();
            ChartCompleted?.Invoke();
            Debug.Log($"[RhythmPrototype] Chart terminado en {chartEndTime:F3} s. Gameplay en espera.", this);
        }
    }
}
