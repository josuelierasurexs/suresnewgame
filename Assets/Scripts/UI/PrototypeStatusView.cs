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
        private Text scoreLabel;
        private Text comboLabel;
        private Text songLabel;
        private Text statsLabel;
        private Image progressFill;
        private bool soloLayout;
        private bool compactLayout;
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
            soloLayout=false;
        }

        public void ConfigureSolo(AudioManager audio, ChartManager chart, TileSpawner spawner, ScoreManager score,
            ComboManager combo, Text scoreText, Text comboText, Text songText, Text statsText, Image progress)
        {
            audioManager=audio; chartManager=chart; tileSpawner=spawner; scoreManager=score; comboManager=combo;
            scoreLabel=scoreText; comboLabel=comboText; songLabel=songText; statsLabel=statsText; progressFill=progress;
            soloLayout=true;
            compactLayout=false;
        }

        public void ConfigureVersus(AudioManager audio, ChartManager chart, TileSpawner spawner, ScoreManager score,
            ComboManager combo, Text scoreText, Text comboText, Text statsText)
        {
            audioManager=audio; chartManager=chart; tileSpawner=spawner; scoreManager=score; comboManager=combo;
            scoreLabel=scoreText; comboLabel=comboText; statsLabel=statsText;
            songLabel=null; progressFill=null; soloLayout=true; compactLayout=true;
        }

        private void Update()
        {
            if (!soloLayout && statusLabel == null)
            {
                return;
            }

            var playbackState = audioManager.IsPlaying ? "PLAYING" : "STOPPED";
            if (previousCombo>=0 && comboManager.CurrentCombo!=previousCombo) comboPunch=.16f;
            previousCombo=comboManager.CurrentCombo;
            comboPunch=Mathf.Max(0f,comboPunch-Time.unscaledDeltaTime);
            if (soloLayout)
            {
                scoreLabel.text=compactLayout
                    ? $"<color=#80E5FF>SCORE</color>\n<size=42>{scoreManager.Score:N0}</size>"
                    : $"SCORE\n<size=58>{scoreManager.Score:N0}</size>";
                var showCombo=comboManager.CurrentMultiplier>1;
                if (compactLayout)
                    comboLabel.text=showCombo
                        ? $"<size=24><color=#FFE36A>COMBO</color></size>\n<size=48><color=#FFD02E>×{comboManager.CurrentMultiplier}</color></size>"
                        : string.Empty;
                else
                    comboLabel.text=showCombo
                        ? $"<size=34><color=#FFE36A>COMBO</color></size>\n<size=78><color=#FFD02E>×{comboManager.CurrentMultiplier}</color></size>\n<size=22>{comboManager.CurrentCombo} HITS</size>"
                        : string.Empty;
                comboLabel.rectTransform.localScale=Vector3.one*(showCombo && comboPunch>0f ? 1.12f : 1f);
                if (songLabel!=null)
                    songLabel.text=$"{playbackState}\n{chartManager.SongId}\n{FormatTime(audioManager.SongTimeSeconds)} / {FormatTime(audioManager.DurationSeconds)}";
                statsLabel.text=$"<color=#80E5FF>PERFECT</color> {scoreManager.Perfects}     <color=#B9A5FF>GREAT</color> {scoreManager.Greats}     <color=#FFE36A>GOOD</color> {scoreManager.Goods}     <color=#FF7188>MISS</color> {scoreManager.Misses}";
                if (progressFill!=null)
                    progressFill.fillAmount=audioManager.DurationSeconds>0d
                        ? Mathf.Clamp01((float)(audioManager.SongTimeSeconds/audioManager.DurationSeconds)) : 0f;
                return;
            }
            statusLabel.rectTransform.localScale=Vector3.one*(comboPunch>0f ? 1.04f : 1f);
            statusLabel.text = $"<color=#8FA8C9>{playbackState}  •  {audioManager.SongTimeSeconds:00.000}s</color>\n" +
                               $"<size=25>SCORE {scoreManager.Score:N0}</size>   <color=#FFC229><size=27>COMBO {comboManager.CurrentCombo}  ×{comboManager.CurrentMultiplier}</size></color>\n" +
                               $"<size=15>P {scoreManager.Perfects}   GR {scoreManager.Greats}   GO {scoreManager.Goods}   <color=#FF405F>MISS {scoreManager.Misses}</color></size>";
        }

        private static string FormatTime(double seconds)
        {
            var totalSeconds=Mathf.Max(0,Mathf.FloorToInt((float)seconds));
            return $"{totalSeconds/60:00}:{totalSeconds%60:00}";
        }
    }
}
