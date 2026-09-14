using Surexs.DanceOff.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Surexs.DanceOff.UI
{
    public sealed class GameplayFeedbackView : MonoBehaviour
    {
        [SerializeField] private Text judgmentLabel;
        [SerializeField] private Text milestoneLabel;
        [SerializeField, Min(0.1f)] private float displaySeconds = 0.8f;

        private float judgmentRemaining;
        private float milestoneRemaining;
        private CanvasGroup judgmentGroup;
        private CanvasGroup milestoneGroup;

        public void Configure(Text judgment, Text milestone)
        {
            judgmentLabel = judgment;
            milestoneLabel = milestone;
            judgmentGroup = GetOrAddCanvasGroup(judgmentLabel);
            milestoneGroup = GetOrAddCanvasGroup(milestoneLabel);
            Clear();
        }

        public void ShowJudgment(RhythmJudgmentResult result, int points, string milestone)
        {
            judgmentLabel.text = points > 0 ? $"{ResultText(result)}\n+{points}" : ResultText(result);
            judgmentLabel.color = ColorFor(result);
            judgmentRemaining = displaySeconds;
            judgmentGroup.alpha = 1f;
            judgmentLabel.rectTransform.localScale = Vector3.one * 1.3f;

            if (!string.IsNullOrEmpty(milestone))
            {
                milestoneLabel.text = milestone;
                milestoneRemaining = displaySeconds * 1.75f;
                milestoneGroup.alpha = 1f;
                milestoneLabel.rectTransform.localScale = Vector3.one * 1.2f;
            }
        }

        private void Update()
        {
            Animate(ref judgmentRemaining, displaySeconds, judgmentLabel, judgmentGroup);
            Animate(ref milestoneRemaining, displaySeconds * 1.75f, milestoneLabel, milestoneGroup);
        }

        private static void Animate(ref float remaining, float duration, Text label, CanvasGroup group)
        {
            if (remaining <= 0f || label == null)
            {
                return;
            }

            remaining -= Time.unscaledDeltaTime;
            var normalized = Mathf.Clamp01(remaining / duration);
            group.alpha = Mathf.Clamp01(normalized * 2f);
            label.rectTransform.localScale = Vector3.one * Mathf.Lerp(1f, 1.25f, normalized);
            if (remaining <= 0f)
            {
                label.text = string.Empty;
            }
        }

        private void Clear()
        {
            if (judgmentLabel != null) judgmentLabel.text = string.Empty;
            if (milestoneLabel != null) milestoneLabel.text = string.Empty;
            if (judgmentGroup != null) judgmentGroup.alpha = 0f;
            if (milestoneGroup != null) milestoneGroup.alpha = 0f;
        }

        public void ResetView()
        {
            judgmentRemaining = 0f;
            milestoneRemaining = 0f;
            Clear();
        }

        private static CanvasGroup GetOrAddCanvasGroup(Text label)
        {
            var group = label.GetComponent<CanvasGroup>();
            return group != null ? group : label.gameObject.AddComponent<CanvasGroup>();
        }

        private static string ResultText(RhythmJudgmentResult result)
        {
            return result.ToString().ToUpperInvariant();
        }

        private static Color ColorFor(RhythmJudgmentResult result)
        {
            switch (result)
            {
                case RhythmJudgmentResult.Perfect: return new Color(0.30f, 1f, 0.75f);
                case RhythmJudgmentResult.Great: return new Color(0.25f, 0.75f, 1f);
                case RhythmJudgmentResult.Good: return new Color(1f, 0.85f, 0.20f);
                default: return new Color(1f, 0.30f, 0.35f);
            }
        }
    }
}
