using Surexs.DanceOff.Rhythm;
using UnityEngine;

namespace Surexs.DanceOff.Player
{
    public sealed class PlayerController : MonoBehaviour
    {
        private RhythmJudge rhythmJudge;
        private PoseController poseController;
        private float poseDuration;
        private float poseTimeRemaining;

        public void Configure(RhythmJudge judge, PoseController poses, float duration)
        {
            DisconnectJudge();
            rhythmJudge = judge;
            poseController = poses;
            poseDuration = Mathf.Max(0.05f, duration);
            rhythmJudge.NoteJudged += OnNoteJudged;
        }

        private void Update()
        {
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
                return;
            }

            poseController.ShowPose(judgment.ChartEvent.Pose);
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
            poseController.ShowNeutral();
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
