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
        private int previousCombo=-1;
        private float comboPunch;

        public void Configure(AudioManager audio, ChartManager chart, TileSpawner spawner, ScoreManager score,
            ComboManager combo, Text label)
        {
            audioManager = audio;
            chartManager = chart;
            tileSpawner = spawner;
            scoreManager = score;
            comboManager = combo;
            statusLabel = label;
            statusLabel.supportRichText = true;
        }

        private void Update()
        {
            if (statusLabel == null)
            {
                return;
            }

            var playbackState = audioManager.IsPlaying ? "PLAYING" : "STOPPED";
            if (previousCombo>=0 && comboManager.CurrentCombo!=previousCombo) comboPunch=.16f;
            previousCombo=comboManager.CurrentCombo;
            comboPunch=Mathf.Max(0f,comboPunch-Time.unscaledDeltaTime);
            statusLabel.rectTransform.localScale=Vector3.one*(comboPunch>0f ? 1.04f : 1f);
            statusLabel.text = $"<color=#8FA8C9>{playbackState}  •  {audioManager.SongTimeSeconds:00.000}s</color>\n" +
                               $"<size=25>SCORE {scoreManager.Score:N0}</size>   <color=#FFC229><size=27>COMBO {comboManager.CurrentCombo}  ×{comboManager.CurrentMultiplier}</size></color>\n" +
                               $"<size=15>P {scoreManager.Perfects}   GR {scoreManager.Greats}   GO {scoreManager.Goods}   <color=#FF405F>MISS {scoreManager.Misses}</color></size>";
        }
    }
}
