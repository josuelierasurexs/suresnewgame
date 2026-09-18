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

        private RawImage judgmentImage;
        private Text pointsLabel;
        private Texture perfectTexture;
        private Texture greatTexture;
        private Texture goodTexture;
        private Texture missTexture;
        private JudgmentStarBurstView starBurstView;
        private bool useImageFeedback;

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
            useImageFeedback = false;
            Clear();
        }

        public void Configure(RawImage image, Text points, Text milestone, Texture perfect, Texture great,
            Texture good, Texture miss, JudgmentStarBurstView stars = null)
        {
            judgmentImage=image; pointsLabel=points; milestoneLabel=milestone;
            perfectTexture=perfect; greatTexture=great; goodTexture=good; missTexture=miss;
            starBurstView=stars;
            judgmentGroup=GetOrAddCanvasGroup(judgmentImage);
            milestoneGroup=GetOrAddCanvasGroup(milestoneLabel);
            displaySeconds=.55f;
            useImageFeedback=true;
            Clear();
        }

        public void ShowJudgment(RhythmJudgmentResult result, int points, string milestone,
            RhythmDirection? hitDirection = null)
        {
            if (useImageFeedback)
            {
                judgmentImage.texture=TextureFor(result);
                pointsLabel.text=points>0 ? $"+{points}" : string.Empty;
            }
            else
            {
                judgmentLabel.text = points > 0 ? $"{ResultText(result)}\n+{points}" : ResultText(result);
                judgmentLabel.color = ColorFor(result);
            }
            judgmentRemaining = displaySeconds;
            judgmentGroup.alpha = 1f;
            JudgmentTransform.localScale = Vector3.one * .55f;
            starBurstView?.Play(result, hitDirection);

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
            if (useImageFeedback) AnimateImage();
            else Animate(ref judgmentRemaining, displaySeconds, judgmentLabel, judgmentGroup);
            Animate(ref milestoneRemaining, displaySeconds * 1.75f, milestoneLabel, milestoneGroup);
        }

        private void AnimateImage()
        {
            if (judgmentRemaining<=0f || judgmentImage==null) return;
            judgmentRemaining-=Time.unscaledDeltaTime;
            var elapsed=1f-Mathf.Clamp01(judgmentRemaining/displaySeconds);
            float scale;
            if (elapsed<.28f) scale=Mathf.Lerp(.55f,1.14f,elapsed/.28f);
            else if (elapsed<.48f) scale=Mathf.Lerp(1.14f,1f,(elapsed-.28f)/.20f);
            else scale=Mathf.Lerp(1f,1.07f,(elapsed-.48f)/.52f);
            JudgmentTransform.localScale=Vector3.one*scale;
            judgmentGroup.alpha=elapsed<.68f ? 1f : 1f-Mathf.InverseLerp(.68f,1f,elapsed);
            if (judgmentRemaining<=0f)
            {
                judgmentImage.texture=null;
                pointsLabel.text=string.Empty;
            }
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
            if (judgmentImage != null) judgmentImage.texture=null;
            if (pointsLabel != null) pointsLabel.text=string.Empty;
            if (milestoneLabel != null) milestoneLabel.text = string.Empty;
            if (judgmentGroup != null) judgmentGroup.alpha = 0f;
            if (milestoneGroup != null) milestoneGroup.alpha = 0f;
        }

        public void ResetView()
        {
            judgmentRemaining = 0f;
            milestoneRemaining = 0f;
            starBurstView?.ResetView();
            Clear();
        }

        private static CanvasGroup GetOrAddCanvasGroup(Text label)
        {
            var group = label.GetComponent<CanvasGroup>();
            return group != null ? group : label.gameObject.AddComponent<CanvasGroup>();
        }

        private static CanvasGroup GetOrAddCanvasGroup(RawImage image)
        {
            var group=image.GetComponent<CanvasGroup>();
            return group != null ? group : image.gameObject.AddComponent<CanvasGroup>();
        }

        private RectTransform JudgmentTransform => useImageFeedback ? judgmentImage.rectTransform : judgmentLabel.rectTransform;

        private Texture TextureFor(RhythmJudgmentResult result)
        {
            switch (result)
            {
                case RhythmJudgmentResult.Perfect: return perfectTexture;
                case RhythmJudgmentResult.Great: return greatTexture;
                case RhythmJudgmentResult.Good: return goodTexture;
                default: return missTexture;
            }
        }

        private static string ResultText(RhythmJudgmentResult result)
        {
            return result.ToString().ToUpperInvariant();
        }

        private static Color ColorFor(RhythmJudgmentResult result)
        {
            switch (result)
            {
                case RhythmJudgmentResult.Perfect: return SurexsVisualTheme.Success;
                case RhythmJudgmentResult.Great: return SurexsVisualTheme.Primary;
                case RhythmJudgmentResult.Good: return SurexsVisualTheme.Accent;
                default: return SurexsVisualTheme.Error;
            }
        }
    }
}
