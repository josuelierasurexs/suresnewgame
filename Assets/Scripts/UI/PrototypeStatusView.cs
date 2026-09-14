using Surexs.DanceOff.Core;
using Surexs.DanceOff.Gameplay;
using Surexs.DanceOff.Rhythm;
using UnityEngine;
using UnityEngine.UI;

namespace Surexs.DanceOff.UI
{
    public sealed class PrototypeStatusView : MonoBehaviour
    {
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private ChartManager chartManager;
        [SerializeField] private TileSpawner tileSpawner;
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private ComboManager comboManager;
        [SerializeField] private Text statusLabel;

        public void Configure(AudioManager audio, ChartManager chart, TileSpawner spawner, ScoreManager score,
            ComboManager combo, Text label)
        {
            audioManager = audio;
            chartManager = chart;
            tileSpawner = spawner;
            scoreManager = score;
            comboManager = combo;
            statusLabel = label;
        }

        private void Update()
        {
            if (statusLabel == null)
            {
                return;
            }

            var playbackState = audioManager.IsPlaying ? "PLAYING" : "STOPPED";
            statusLabel.text = $"{playbackState}   SONG TIME  {audioManager.SongTimeSeconds:00.000} s\n" +
                               $"SCORE: {scoreManager.Score}    COMBO: {comboManager.CurrentCombo}    " +
                               $"MULTIPLIER: x{comboManager.CurrentMultiplier}\n" +
                               $"PERFECT: {scoreManager.Perfects}    GREAT: {scoreManager.Greats}    " +
                               $"GOOD: {scoreManager.Goods}    MISS: {scoreManager.Misses}";
        }
    }
}
