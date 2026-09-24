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
        private HitPhraseFeedbackView hitPhraseView;
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
            Texture good, Texture miss, JudgmentStarBurstView stars = null,
            HitPhraseFeedbackView hitPhrases = null)
        {
            judgmentImage=image; pointsLabel=points; milestoneLabel=milestone;
            perfectTexture=perfect; greatTexture=great; goodTexture=good; missTexture=miss;
            starBurstView=stars;
            hitPhraseView=hitPhrases;
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
            if (result != RhythmJudgmentResult.Miss) hitPhraseView?.Play();

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
            hitPhraseView?.ResetView();
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

    public sealed class HitPhraseFeedbackView : MonoBehaviour
    {
        private const float MaxRotation = 30f;

        private static readonly string[] Phrases =
        {
            "Gestionando Polizas",
            "Dando alta a asegurados",
            "Atendiendo Siniestros",
            "Mejorando tiempos operativos"
        };

        private static readonly Color[] PastelColors =
        {
            new Color(.52f,.76f,.92f),
            new Color(.95f,.82f,.48f),
            new Color(.55f,.84f,.67f)
        };

        private Text label;
        private CanvasGroup group;
        private Vector2 basePosition;
        private float duration;
        private float remaining;
        private float targetScale;

        public void Configure(Text phraseLabel,bool compact)
        {
            label=phraseLabel;
            group=label.GetComponent<CanvasGroup>();
            if (group == null) group=label.gameObject.AddComponent<CanvasGroup>();
            basePosition=label.rectTransform.anchoredPosition;
            duration=compact ? .58f : .72f;
            targetScale=compact ? .9f : 1f;
            ResetView();
        }

        public void Play()
        {
            if (label == null) return;
            label.text=Phrases[Random.Range(0,Phrases.Length)];
            label.color=PastelColors[Random.Range(0,PastelColors.Length)];
            label.rectTransform.anchoredPosition=basePosition+new Vector2(Random.Range(-18f,18f),Random.Range(-8f,9f));
            label.rectTransform.localEulerAngles=new Vector3(0f,0f,Random.Range(-MaxRotation,MaxRotation));
            label.rectTransform.localScale=Vector3.one*(targetScale*.48f);
            group.alpha=1f;
            remaining=duration;
        }

        private void Update()
        {
            if (remaining<=0f || label==null) return;
            remaining-=Time.unscaledDeltaTime;
            var elapsed=1f-Mathf.Clamp01(remaining/duration);
            float scale;
            if (elapsed<.24f) scale=Mathf.Lerp(.48f,1.13f,elapsed/.24f);
            else if (elapsed<.42f) scale=Mathf.Lerp(1.13f,1f,(elapsed-.24f)/.18f);
            else scale=Mathf.Lerp(1f,1.05f,(elapsed-.42f)/.58f);
            label.rectTransform.localScale=Vector3.one*(targetScale*scale);
            group.alpha=elapsed<.55f ? 1f : 1f-Mathf.InverseLerp(.55f,1f,elapsed);
            if (remaining<=0f) ResetView();
        }

        public void ResetView()
        {
            remaining=0f;
            if (label==null) return;
            label.text=string.Empty;
            label.rectTransform.anchoredPosition=basePosition;
            label.rectTransform.localEulerAngles=Vector3.zero;
            label.rectTransform.localScale=Vector3.one*targetScale;
            if (group!=null) group.alpha=0f;
        }
    }
}
