using System.Collections;
using Surexs.DanceOff.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Surexs.DanceOff.UI
{
    public sealed class JudgmentStarBurstView : MonoBehaviour
    {
        private const int MaxStars = 13;
        private const float BurstDuration = 0.68f;
        private const float Gravity = 1450f;

        private readonly StarParticle[] particles = new StarParticle[MaxStars];
        private RectTransform particleRoot;
        private Texture blueTexture;
        private Texture yellowTexture;
        private Vector2 leftOrigin;
        private Vector2 centerOrigin;
        private Vector2 rightOrigin;
        private Coroutine animationRoutine;

        public void Configure(RectTransform root, Texture blue, Texture yellow, Vector2 left, Vector2 center,
            Vector2 right)
        {
            particleRoot = root;
            blueTexture = blue;
            yellowTexture = yellow;
            leftOrigin = left;
            centerOrigin = center;
            rightOrigin = right;

            for (var index = 0; index < particles.Length; index++)
            {
                var particleObject = new GameObject($"Judgment Star {index + 1}", typeof(RectTransform),
                    typeof(RawImage), typeof(CanvasGroup));
                var rect = particleObject.GetComponent<RectTransform>();
                rect.SetParent(particleRoot, false);
                rect.sizeDelta = new Vector2(48f, 48f);

                var image = particleObject.GetComponent<RawImage>();
                image.raycastTarget = false;
                var group = particleObject.GetComponent<CanvasGroup>();
                group.alpha = 0f;
                particles[index] = new StarParticle(rect, image, group);
            }
        }

        public void Play(RhythmJudgmentResult result, RhythmDirection? direction)
        {
            StopAndHide();

            if (!direction.HasValue)
            {
                return;
            }

            var blueCount = 0;
            var yellowCount = 0;
            switch (result)
            {
                case RhythmJudgmentResult.Good:
                    blueCount = 5;
                    break;
                case RhythmJudgmentResult.Great:
                    blueCount = 5;
                    yellowCount = 4;
                    break;
                case RhythmJudgmentResult.Perfect:
                    blueCount = 5;
                    yellowCount = 8;
                    break;
                default:
                    return;
            }

            var activeCount = blueCount + yellowCount;
            var origin = OriginFor(direction.Value);
            for (var index = 0; index < activeCount; index++)
            {
                var particle = particles[index];
                particle.Image.texture = index < blueCount ? blueTexture : yellowTexture;
                particle.Rect.anchoredPosition = origin + new Vector2(Random.Range(-72f, 72f), Random.Range(-10f, 10f));
                particle.Rect.localEulerAngles = new Vector3(0f, 0f, Random.Range(-30f, 30f));
                particle.Rect.localScale = Vector3.one * .35f;
                particle.Group.alpha = 0f;
                particle.Velocity = new Vector2(Random.Range(-245f, 245f), Random.Range(640f, 820f));
                particle.RotationSpeed = Random.Range(-420f, 420f);
                particle.Delay = index * .009f;
            }

            animationRoutine = StartCoroutine(Animate(activeCount));
        }

        private Vector2 OriginFor(RhythmDirection direction)
        {
            switch (direction)
            {
                case RhythmDirection.Left:
                    return leftOrigin;
                case RhythmDirection.Right:
                    return rightOrigin;
                default:
                    return centerOrigin;
            }
        }

        public void ResetView()
        {
            StopAndHide();
        }

        private IEnumerator Animate(int activeCount)
        {
            var elapsed = 0f;
            while (elapsed < BurstDuration)
            {
                var delta = Time.unscaledDeltaTime;
                elapsed += delta;
                for (var index = 0; index < activeCount; index++)
                {
                    var particle = particles[index];
                    var localTime = elapsed - particle.Delay;
                    if (localTime < 0f)
                    {
                        continue;
                    }

                    particle.Velocity += Vector2.down * (Gravity * delta);
                    particle.Rect.anchoredPosition += particle.Velocity * delta;
                    particle.Rect.Rotate(0f, 0f, particle.RotationSpeed * delta);

                    var appear = Mathf.Clamp01(localTime / .07f);
                    var disappear = 1f - Mathf.InverseLerp(.44f, BurstDuration, localTime);
                    particle.Group.alpha = Mathf.Min(appear, disappear);
                    var scale = Mathf.Lerp(.35f, 1f, appear) * Mathf.Lerp(1f, .72f,
                        Mathf.InverseLerp(.44f, BurstDuration, localTime));
                    particle.Rect.localScale = Vector3.one * scale;
                }

                yield return null;
            }

            animationRoutine = null;
            HideAll();
        }

        private void StopAndHide()
        {
            if (animationRoutine != null)
            {
                StopCoroutine(animationRoutine);
                animationRoutine = null;
            }

            HideAll();
        }

        private void HideAll()
        {
            for (var index = 0; index < particles.Length; index++)
            {
                if (particles[index] != null)
                {
                    particles[index].Group.alpha = 0f;
                }
            }
        }

        private sealed class StarParticle
        {
            public StarParticle(RectTransform rect, RawImage image, CanvasGroup group)
            {
                Rect = rect;
                Image = image;
                Group = group;
            }

            public RectTransform Rect { get; }
            public RawImage Image { get; }
            public CanvasGroup Group { get; }
            public Vector2 Velocity { get; set; }
            public float RotationSpeed { get; set; }
            public float Delay { get; set; }
        }
    }
}
