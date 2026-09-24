#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Surexs.DanceOff.Data;
using Surexs.DanceOff.Input;
using Surexs.DanceOff.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Surexs.DanceOff.Core
{
    public static class ChartRecorderAccess
    {
        private const string LaunchRequestKey = "SurexsDanceOff.ChartRecorder.LaunchRequested";

        public static void RequestLaunch()
        {
            SessionState.SetBool(LaunchRequestKey, true);
        }

        public static bool ConsumeLaunchRequest()
        {
            if (!SessionState.GetBool(LaunchRequestKey, false)) return false;
            SessionState.SetBool(LaunchRequestKey, false);
            return true;
        }

        public static void CancelLaunch()
        {
            SessionState.SetBool(LaunchRequestKey, false);
        }
    }

    public sealed class ChartRecorderController : MonoBehaviour
    {
        private static readonly string[] Poses = { "phone", "laptop", "tablet" };

        private readonly List<RecordedNote> notes = new List<RecordedNote>();
        private AudioManager audioManager;
        private AudioSource audioSource;
        private IRhythmInputSource inputSource;
        private RectTransform tileRoot;
        private float[] lanePositions;
        private float hitZoneY;
        private Text timeLabel;
        private Text countLabel;
        private Text poseLabel;
        private Text offsetLabel;
        private Text recentNotesLabel;
        private Text statusLabel;
        private InputField fileNameInput;
        private Text playPauseLabel;
        private int poseIndex;
        private double globalOffsetSeconds;
        private bool paused;
        private bool configured;
        private bool naturalEndReported;
        private double lastPlaybackTime;

        public void Configure(AudioManager audio, AudioSource source, IRhythmInputSource input,
            RectTransform previewRoot, float[] lanes, float configuredHitZoneY, Text songTime,
            Text noteCount, Text selectedPose, Text offset, Text recentNotes, Text status,
            InputField fileName, Text playbackButtonLabel)
        {
            audioManager = audio;
            audioSource = source;
            inputSource = input;
            tileRoot = previewRoot;
            lanePositions = lanes;
            hitZoneY = configuredHitZoneY;
            timeLabel = songTime;
            countLabel = noteCount;
            poseLabel = selectedPose;
            offsetLabel = offset;
            recentNotesLabel = recentNotes;
            statusLabel = status;
            fileNameInput = fileName;
            playPauseLabel = playbackButtonLabel;
            inputSource.DirectionPressed += RecordNote;
            configured = true;
            RefreshInterface();
        }

        private void Start()
        {
            if (!configured || !audioManager.PlayFromStart())
            {
                SetStatus("No se pudo iniciar: revisa el AudioClip de Game.", true);
                return;
            }

            paused = false;
            SetStatus("Grabando. Usa A, W/S y D siguiendo la canción.");
            RefreshInterface();
        }

        private void Update()
        {
            if (!configured) return;
            var songTime = audioManager.SongTimeSeconds;
            if (audioManager.IsPlaying)
            {
                lastPlaybackTime = songTime;
                naturalEndReported = false;
            }

            var displayedTime = !audioManager.IsPlaying && !paused && naturalEndReported
                ? audioManager.DurationSeconds
                : songTime;
            timeLabel.text = $"SONG TIME  {FormatTime(displayedTime)}";

            if (!paused && !audioManager.IsPlaying && !naturalEndReported && lastPlaybackTime > 0d)
            {
                naturalEndReported = true;
                playPauseLabel.text = "REPRODUCIR";
                SetStatus("La canción terminó. Guarda el JSON o reinicia para revisar.");
            }
        }

        public void TogglePlayback()
        {
            if (audioSource == null || audioSource.clip == null) return;

            if (audioSource.isPlaying)
            {
                audioSource.Pause();
                paused = true;
                playPauseLabel.text = "REPRODUCIR";
                SetStatus("Pausa. Las teclas de dirección no registran notas.");
                return;
            }

            if (audioSource.timeSamples >= audioSource.clip.samples - 1)
                audioSource.timeSamples = 0;

            audioSource.UnPause();
            if (!audioSource.isPlaying) audioSource.Play();
            paused = false;
            naturalEndReported = false;
            playPauseLabel.text = "PAUSA";
            SetStatus("Grabando. Usa A, W/S y D siguiendo la canción.");
        }

        public void RestartPlayback()
        {
            if (!audioManager.PlayFromStart()) return;
            paused = false;
            naturalEndReported = false;
            lastPlaybackTime = 0d;
            playPauseLabel.text = "PAUSA";
            SetStatus("Audio reiniciado. Las notas existentes se conservaron.");
        }

        public void UndoLastNote()
        {
            if (notes.Count == 0)
            {
                SetStatus("No hay notas para deshacer.", true);
                return;
            }

            var removed = notes[notes.Count - 1];
            notes.RemoveAt(notes.Count - 1);
            SetStatus($"Eliminada {removed.Direction.ToString().ToUpperInvariant()} en {removed.Time:0.000} s.");
            RefreshInterface();
        }

        public void NudgeLastNote(int milliseconds)
        {
            if (notes.Count == 0)
            {
                SetStatus("No hay una última nota que ajustar.", true);
                return;
            }

            var last = notes[notes.Count - 1];
            last.Time = Math.Max(0d, last.Time + milliseconds / 1000d);
            notes[notes.Count - 1] = last;
            SetStatus($"Última nota ajustada a {last.Time + globalOffsetSeconds:0.000} s.");
            RefreshInterface();
        }

        public void AdjustGlobalOffset(int milliseconds)
        {
            globalOffsetSeconds += milliseconds / 1000d;
            SetStatus($"Offset global: {globalOffsetSeconds * 1000d:+0;-0;0} ms.");
            RefreshInterface();
        }

        public void SelectNextPose()
        {
            poseIndex = (poseIndex + 1) % Poses.Length;
            SetStatus($"Las próximas notas utilizarán la pose {Poses[poseIndex].ToUpperInvariant()}.");
            RefreshInterface();
        }

        public void SaveChart()
        {
            if (notes.Count == 0)
            {
                SetStatus("Agrega al menos una nota antes de guardar.", true);
                return;
            }

            var requestedName = SanitizeFileName(fileNameInput.text);
            if (string.IsNullOrWhiteSpace(requestedName)) requestedName = "surexs_recorded_chart";

            var chart = new RecordedChartJson
            {
                song = audioSource.clip != null ? audioSource.clip.name : requestedName,
                visualSpeed = 1f,
                tiles = notes
                    .Select(note => new RecordedTileJson
                    {
                        time = (float)Math.Max(0d, note.Time + globalOffsetSeconds),
                        direction = note.Direction.ToString().ToLowerInvariant(),
                        pose = note.Pose
                    })
                    .OrderBy(tile => tile.time)
                    .ToArray()
            };

            const string chartFolder = "Assets/Data/Charts";
            if (!AssetDatabase.IsValidFolder(chartFolder))
            {
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "Data", "Charts"));
                AssetDatabase.Refresh();
            }

            var assetPath = AssetDatabase.GenerateUniqueAssetPath($"{chartFolder}/{requestedName}.json");
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            var absolutePath = Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
            File.WriteAllText(absolutePath, JsonUtility.ToJson(chart, true));
            AssetDatabase.Refresh();
            SetStatus($"Guardado: {assetPath} ({chart.tiles.Length} notas).", false);
            Debug.Log($"[ChartRecorder] Chart guardado en '{assetPath}' con {chart.tiles.Length} notas.", this);
        }

        private void RecordNote(RhythmDirection direction)
        {
            if (fileNameInput != null && (fileNameInput.isFocused ||
                EventSystem.current != null && EventSystem.current.currentSelectedGameObject == fileNameInput.gameObject))
                return;

            if (!configured || !audioManager.IsPlaying)
            {
                SetStatus("La canción debe estar reproduciéndose para registrar notas.", true);
                return;
            }

            var time = audioManager.SongTimeSeconds;
            var pose = Poses[poseIndex];
            notes.Add(new RecordedNote(time, direction, pose));
            ChartRecorderNoteFlash.Create(tileRoot, LanePosition(direction), hitZoneY, direction, time);
            SetStatus($"{direction.ToString().ToUpperInvariant()}  {time + globalOffsetSeconds:0.000} s  •  {pose.ToUpperInvariant()}");
            RefreshInterface();
        }

        private float LanePosition(RhythmDirection direction)
        {
            switch (direction)
            {
                case RhythmDirection.Left: return lanePositions[0];
                case RhythmDirection.Center: return lanePositions[1];
                case RhythmDirection.Right: return lanePositions[2];
                default: return lanePositions[1];
            }
        }

        private void RefreshInterface()
        {
            if (!configured) return;
            countLabel.text = $"NOTAS REGISTRADAS  {notes.Count}";
            poseLabel.text = $"POSE: {Poses[poseIndex].ToUpperInvariant()}";
            offsetLabel.text = $"OFFSET GLOBAL  {globalOffsetSeconds * 1000d:+0;-0;0} ms";
            playPauseLabel.text = audioManager.IsPlaying ? "PAUSA" : "REPRODUCIR";

            var recent = notes.Skip(Math.Max(0, notes.Count - 5))
                .Select((note, index) =>
                    $"{notes.Count - Math.Min(5, notes.Count) + index + 1:000}   " +
                    $"{note.Time + globalOffsetSeconds,7:0.000}   " +
                    $"{note.Direction.ToString().ToUpperInvariant(),-6}   {note.Pose.ToUpperInvariant()}");
            recentNotesLabel.text = notes.Count == 0
                ? "AÚN NO HAY NOTAS"
                : string.Join("\n", recent);
        }

        private void SetStatus(string message, bool warning = false)
        {
            if (statusLabel == null) return;
            statusLabel.text = message;
            statusLabel.color = warning ? SurexsVisualTheme.Accent : SurexsVisualTheme.TextSecondary;
        }

        private void OnDestroy()
        {
            if (inputSource != null) inputSource.DirectionPressed -= RecordNote;
        }

        private static string FormatTime(double seconds)
        {
            var minutes = (int)(seconds / 60d);
            var remainingSeconds = seconds - minutes * 60d;
            return $"{minutes:00}:{remainingSeconds:00.000}";
        }

        private static string SanitizeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            var invalid = Path.GetInvalidFileNameChars();
            var sanitized = new string(value.Trim().Select(character =>
                invalid.Contains(character) || char.IsWhiteSpace(character) ? '_' : character).ToArray());
            return sanitized.Trim('_');
        }

        private struct RecordedNote
        {
            public RecordedNote(double time, RhythmDirection direction, string pose)
            {
                Time = time;
                Direction = direction;
                Pose = pose;
            }

            public double Time;
            public RhythmDirection Direction;
            public string Pose;
        }

        [Serializable]
        private sealed class RecordedChartJson
        {
            public string song;
            public float visualSpeed;
            public RecordedTileJson[] tiles;
        }

        [Serializable]
        private sealed class RecordedTileJson
        {
            public float time;
            public string direction;
            public string pose;
        }
    }

    internal sealed class ChartRecorderNoteFlash : MonoBehaviour
    {
        private const float Lifetime = 0.48f;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private float elapsed;

        public static void Create(RectTransform parent, float laneX, float hitY, RhythmDirection direction,
            double time)
        {
            var go = new GameObject("Recorded Note Preview", typeof(RectTransform), typeof(Image),
                typeof(CanvasGroup), typeof(ChartRecorderNoteFlash));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchoredPosition = new Vector2(laneX, hitY);
            rect.sizeDelta = new Vector2(168f, 78f);
            var image = go.GetComponent<Image>();
            image.color = DirectionColor(direction);
            image.raycastTarget = false;
            SurexsVisualTheme.ApplyRounded(image);

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.SetParent(rect, false);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var label = labelObject.GetComponent<Text>();
            label.font = SurexsVisualTheme.BodyFont;
            label.fontSize = 22;
            label.fontStyle = FontStyle.Normal;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;
            label.text = $"{DirectionSymbol(direction)}  {time:0.000}";
        }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            var progress = Mathf.Clamp01(elapsed / Lifetime);
            rectTransform.anchoredPosition += Vector2.down * (260f * Time.unscaledDeltaTime);
            rectTransform.localScale = Vector3.one * Mathf.Lerp(1.08f, 0.9f, progress);
            canvasGroup.alpha = 1f - progress;
            if (elapsed >= Lifetime) Destroy(gameObject);
        }

        private static Color DirectionColor(RhythmDirection direction)
        {
            switch (direction)
            {
                case RhythmDirection.Left: return new Color(0.20f, 0.68f, 1f, 1f);
                case RhythmDirection.Center: return new Color(1f, 0.78f, 0.18f, 1f);
                case RhythmDirection.Right: return new Color(1f, 0.34f, 0.48f, 1f);
                default: return Color.white;
            }
        }

        private static string DirectionSymbol(RhythmDirection direction)
        {
            switch (direction)
            {
                case RhythmDirection.Left: return "←";
                case RhythmDirection.Center: return "●";
                case RhythmDirection.Right: return "→";
                default: return "?";
            }
        }
    }
}
#endif
