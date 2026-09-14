using System.Collections.Generic;
using Surexs.DanceOff.Core;
using Surexs.DanceOff.Data;
using UnityEngine;

namespace Surexs.DanceOff.Rhythm
{
    public sealed class TileSpawner : MonoBehaviour
    {
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private ChartManager chartManager;
        [SerializeField] private RectTransform tileContainer;
        [SerializeField] private float leftLaneX = -350f;
        [SerializeField] private float centerLaneX;
        [SerializeField] private float rightLaneX = 350f;
        [SerializeField] private float hitZoneY = -250f;
        [SerializeField, Min(1f)] private float pixelsPerSecond = 220f;
        [SerializeField, Min(0.1f)] private float leadTimeSeconds = 4f;
        [SerializeField, Min(0f)] private float despawnDelaySeconds = 0.75f;

        private readonly List<ActiveTile> activeTiles = new List<ActiveTile>();
        private IReadOnlyList<ChartEvent> chartEvents;
        private RhythmJudge rhythmJudge;
        private int nextEventIndex;
        private bool isRunning;

        public int ActiveTileCount => activeTiles.Count;
        public int SpawnedTileCount => nextEventIndex;

        public void Configure(AudioManager audio, ChartManager chart, RectTransform container, RhythmJudge judge)
        {
            Configure(audio, chart, container, judge, leftLaneX, centerLaneX, rightLaneX, hitZoneY);
        }

        public void Configure(AudioManager audio, ChartManager chart, RectTransform container, RhythmJudge judge,
            float leftX, float centerX, float rightX, float configuredHitZoneY)
        {
            DisconnectJudge();
            audioManager = audio;
            chartManager = chart;
            tileContainer = container;
            rhythmJudge = judge;
            leftLaneX = leftX;
            centerLaneX = centerX;
            rightLaneX = rightX;
            hitZoneY = configuredHitZoneY;
            rhythmJudge.NoteJudged += OnNoteJudged;
        }

        public void Begin()
        {
            ClearTiles();
            chartEvents = chartManager.Events;
            nextEventIndex = 0;
            isRunning = true;
            UpdateTiles(audioManager.SongTimeSeconds);
        }

        public void StopSpawning()
        {
            isRunning = false;
        }

        private void Update()
        {
            if (isRunning)
            {
                UpdateTiles(audioManager.SongTimeSeconds);
            }
        }

        private void OnDisable()
        {
            isRunning = false;
        }

        private void OnDestroy()
        {
            DisconnectJudge();
        }

        private void OnNoteJudged(RhythmJudgment judgment)
        {
            for (var index = 0; index < activeTiles.Count; index++)
            {
                var activeTile = activeTiles[index];
                if (ReferenceEquals(activeTile.ChartEvent, judgment.ChartEvent))
                {
                    activeTile.View.SetJudgment(judgment.NoteStatus, judgment.Result);
                    return;
                }
            }
        }

        private void DisconnectJudge()
        {
            if (rhythmJudge != null)
            {
                rhythmJudge.NoteJudged -= OnNoteJudged;
            }
        }

        private void UpdateTiles(double songTime)
        {
            while (nextEventIndex < chartEvents.Count && chartEvents[nextEventIndex].Time - songTime <= leadTimeSeconds)
            {
                SpawnTile(chartEvents[nextEventIndex]);
                nextEventIndex++;
            }

            var visualPixelsPerSecond = pixelsPerSecond * chartManager.VisualSpeed;
            for (var index = activeTiles.Count - 1; index >= 0; index--)
            {
                var activeTile = activeTiles[index];
                var secondsFromHit = activeTile.ChartEvent.Time - songTime;
                var y = hitZoneY + (float)secondsFromHit * visualPixelsPerSecond;
                activeTile.View.SetVisualPosition(LaneX(activeTile.ChartEvent.Direction), y, secondsFromHit);

                if (!activeTile.HasReachedHitZone && secondsFromHit <= 0d)
                {
                    activeTile.HasReachedHitZone = true;
                    var timingDelta = songTime - activeTile.ChartEvent.Time;
                    Debug.Log($"[TileSpawner] {activeTile.ChartEvent.Direction} llegó a Hit Zone. " +
                              $"Objetivo {activeTile.ChartEvent.Time:F3} s, reloj {songTime:F3} s, frame delta +{timingDelta:F3} s.", this);
                }

                if (songTime >= activeTile.ChartEvent.Time + despawnDelaySeconds)
                {
                    Destroy(activeTile.View.gameObject);
                    activeTiles.RemoveAt(index);
                }
            }
        }

        private void SpawnTile(ChartEvent chartEvent)
        {
            var view = RhythmTileView.Create(tileContainer);
            view.Configure(chartEvent);
            activeTiles.Add(new ActiveTile(chartEvent, view));
        }

        private float LaneX(RhythmDirection direction)
        {
            switch (direction)
            {
                case RhythmDirection.Left:
                    return leftLaneX;
                case RhythmDirection.Center:
                    return centerLaneX;
                case RhythmDirection.Right:
                    return rightLaneX;
                default:
                    return centerLaneX;
            }
        }

        private void ClearTiles()
        {
            for (var index = activeTiles.Count - 1; index >= 0; index--)
            {
                if (activeTiles[index].View != null)
                {
                    Destroy(activeTiles[index].View.gameObject);
                }
            }

            activeTiles.Clear();
        }

        private sealed class ActiveTile
        {
            public ActiveTile(ChartEvent chartEvent, RhythmTileView view)
            {
                ChartEvent = chartEvent;
                View = view;
            }

            public ChartEvent ChartEvent { get; }
            public RhythmTileView View { get; }
            public bool HasReachedHitZone { get; set; }
        }
    }
}
