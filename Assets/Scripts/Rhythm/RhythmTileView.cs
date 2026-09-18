using Surexs.DanceOff.Data;
using Surexs.DanceOff.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Surexs.DanceOff.Rhythm
{
    public sealed class RhythmTileView : MonoBehaviour
    {
        private const float FadeAfterHitSeconds = 0.175f;
        private static readonly Color LeftColor = new Color(0.20f, 0.68f, 1f, 1f);
        private static readonly Color CenterColor = new Color(1f, 0.78f, 0.18f, 1f);
        private static readonly Color RightColor = new Color(1f, 0.34f, 0.48f, 1f);

        private RectTransform rectTransform;
        private Image background;
        private Text label;
        private CanvasGroup canvasGroup;
        private Color baseColor;
        private string baseLabel;
        private RhythmNoteStatus status;

        public static RhythmTileView Create(RectTransform parent)
        {
            var tileObject = new GameObject("Rhythm Tile", typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(RhythmTileView));
            var tileRect = tileObject.GetComponent<RectTransform>();
            tileRect.SetParent(parent, false);
            tileRect.sizeDelta = new Vector2(168f, 78f);
            var tileImage=tileObject.GetComponent<Image>();
            SurexsVisualTheme.ApplyRounded(tileImage);
            var shadow=tileObject.AddComponent<Shadow>(); shadow.effectColor=new Color(0,0,0,.55f); shadow.effectDistance=new Vector2(0,-6f);
            var outline=tileObject.AddComponent<Outline>(); outline.effectColor=new Color(1,1,1,.24f); outline.effectDistance=new Vector2(2,-2);

            var textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            var textRect = textObject.GetComponent<RectTransform>();
            textRect.SetParent(tileRect, false);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = textObject.GetComponent<Text>();
            text.font = SurexsVisualTheme.Font;
            text.fontSize = 25;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            return tileObject.GetComponent<RhythmTileView>();
        }

        public void Configure(ChartEvent chartEvent)
        {
            rectTransform = GetComponent<RectTransform>();
            background = GetComponent<Image>();
            label = GetComponentInChildren<Text>();
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            baseColor = ColorFor(chartEvent.Direction);
            baseLabel = $"{SymbolFor(chartEvent.Direction)}  {chartEvent.Time:0.00}";
            status = RhythmNoteStatus.Pending;
            background.color = baseColor;
            label.text = baseLabel;
            gameObject.name = $"Tile {chartEvent.Direction} {chartEvent.Time:0.00}";
        }

        public void SetVisualPosition(float laneX, float y, double secondsFromHit)
        {
            rectTransform.anchoredPosition = new Vector2(laneX, y);
            canvasGroup.alpha = secondsFromHit >= 0d
                ? 1f
                : Mathf.Clamp01(1f - (float)(-secondsFromHit / FadeAfterHitSeconds));
            if (status != RhythmNoteStatus.Pending)
            {
                return;
            }

            var isAtHitZone = System.Math.Abs(secondsFromHit) <= 0.05d;
            background.color = isAtHitZone ? Color.white : baseColor;
            label.color = isAtHitZone ? Color.black : Color.white;
        }

        public void SetJudgment(RhythmNoteStatus noteStatus, RhythmJudgmentResult result)
        {
            status = noteStatus;
            background.color = noteStatus == RhythmNoteStatus.Hit
                ? new Color(0.18f, 0.86f, 0.48f, 1f)
                : new Color(0.92f, 0.18f, 0.22f, 1f);
            label.color = Color.white;
            label.text = $"{baseLabel}\n{result.ToString().ToUpperInvariant()}";
        }

        private static string SymbolFor(RhythmDirection direction)
        {
            switch (direction)
            {
                case RhythmDirection.Left:
                    return "←";
                case RhythmDirection.Center:
                    return "●";
                case RhythmDirection.Right:
                    return "→";
                default:
                    return "?";
            }
        }

        private static Color ColorFor(RhythmDirection direction)
        {
            switch (direction)
            {
                case RhythmDirection.Left:
                    return LeftColor;
                case RhythmDirection.Center:
                    return CenterColor;
                case RhythmDirection.Right:
                    return RightColor;
                default:
                    return Color.gray;
            }
        }
    }
}
