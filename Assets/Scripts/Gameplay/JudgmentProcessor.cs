using Surexs.DanceOff.Data;
using Surexs.DanceOff.Rhythm;
using Surexs.DanceOff.UI;
using UnityEngine;

namespace Surexs.DanceOff.Gameplay
{
    public sealed class JudgmentProcessor : MonoBehaviour
    {
        private RhythmJudge rhythmJudge;
        private ScoreManager scoreManager;
        private ComboManager comboManager;
        private GameplayFeedbackView feedbackView;

        public void Configure(RhythmJudge judge, ScoreManager score, ComboManager combo, GameplayFeedbackView feedback)
        {
            DisconnectJudge();
            rhythmJudge = judge;
            scoreManager = score;
            comboManager = combo;
            feedbackView = feedback;
            rhythmJudge.NoteJudged += OnNoteJudged;
        }

        private void OnNoteJudged(RhythmJudgment judgment)
        {
            string milestone = string.Empty;
            if (judgment.Result == RhythmJudgmentResult.Miss)
            {
                comboManager.RegisterMiss();
            }
            else
            {
                milestone = comboManager.RegisterHit();
            }

            var points = scoreManager.Register(judgment.Result, comboManager.CurrentMultiplier);
            feedbackView.ShowJudgment(judgment.Result, points, milestone);
        }

        private void OnDestroy()
        {
            DisconnectJudge();
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
