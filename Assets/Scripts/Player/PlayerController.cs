using Surexs.DanceOff.Rhythm;
using Surexs.DanceOff.Gameplay;
using UnityEngine;

namespace Surexs.DanceOff.Player
{
    public sealed class PlayerController : MonoBehaviour
    {
        private RhythmJudge rhythmJudge;
        private PoseController poseController;
        private float poseDuration;
        private float poseTimeRemaining;
        private ComboManager comboManager;
        private SoloCharacterAnimationView soloAnimation;
        private int observedMultiplier = 1;

        public void Configure(RhythmJudge judge, PoseController poses, float duration,
            ComboManager combo = null, SoloCharacterAnimationView characterAnimation = null)
        {
            DisconnectJudge();
            rhythmJudge = judge;
            poseController = poses;
            poseDuration = Mathf.Max(0.05f, duration);
            comboManager = combo;
            soloAnimation = characterAnimation;
            observedMultiplier = comboManager != null ? comboManager.CurrentMultiplier : 1;
            rhythmJudge.NoteJudged += OnNoteJudged;
        }

        private void Update()
        {
            if (soloAnimation != null) return;
            if (poseTimeRemaining <= 0f)
            {
                return;
            }

            poseTimeRemaining -= Time.unscaledDeltaTime;
            if (poseTimeRemaining <= 0f)
            {
                poseController.ShowNeutral();
            }
        }

        private void OnNoteJudged(RhythmJudgment judgment)
        {
            if (!judgment.IsHit)
            {
                if (soloAnimation != null)
                {
                    poseTimeRemaining = 0f;
                    observedMultiplier = 1;
                    poseController.ShowNeutral();
                    soloAnimation.PlayMiss();
                }
                return;
            }

            poseController.ShowPose(judgment.ChartEvent.Pose);
            if (soloAnimation != null)
            {
                var multiplier = comboManager != null ? comboManager.CurrentMultiplier : 1;
                soloAnimation.PlayPose(judgment.ChartEvent.Pose, multiplier);
                if (multiplier > observedMultiplier) soloAnimation.PlayCombo(multiplier);
                observedMultiplier = multiplier;
                Debug.Log($"[PlayerController] Animación Solo '{soloAnimation.CurrentState}' por {judgment.Result} a x{soloAnimation.PlaybackSpeed:0.##}.", this);
                return;
            }

            poseTimeRemaining = poseDuration;
            Debug.Log($"[PlayerController] Pose '{poseController.CurrentPoseId}' por {judgment.Result}.", this);
        }

        private void OnDestroy()
        {
            DisconnectJudge();
        }

        public void ResetVisual()
        {
            poseTimeRemaining = 0f;
            observedMultiplier = 1;
            poseController.ShowNeutral();
            if (soloAnimation != null) soloAnimation.ResetVisual();
        }

        private void DisconnectJudge()
        {
            if (rhythmJudge != null)
            {
                rhythmJudge.NoteJudged -= OnNoteJudged;
            }
        }
    }
}
