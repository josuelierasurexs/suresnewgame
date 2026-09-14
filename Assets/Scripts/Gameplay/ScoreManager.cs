using Surexs.DanceOff.Data;
using UnityEngine;

namespace Surexs.DanceOff.Gameplay
{
    public sealed class ScoreManager : MonoBehaviour
    {
        private RhythmGameplayConfig config;
        private ComboManager comboManager;

        public int Score { get; private set; }
        public int Perfects { get; private set; }
        public int Greats { get; private set; }
        public int Goods { get; private set; }
        public int Misses { get; private set; }
        public int MaxCombo => comboManager != null ? comboManager.MaxCombo : 0;

        public void Configure(RhythmGameplayConfig gameplayConfig, ComboManager combo)
        {
            config = gameplayConfig;
            comboManager = combo;
        }

        public int Register(RhythmJudgmentResult result, int multiplier)
        {
            switch (result)
            {
                case RhythmJudgmentResult.Perfect:
                    Perfects++;
                    break;
                case RhythmJudgmentResult.Great:
                    Greats++;
                    break;
                case RhythmJudgmentResult.Good:
                    Goods++;
                    break;
                case RhythmJudgmentResult.Miss:
                    Misses++;
                    break;
            }

            var awardedPoints = config.BaseScoreFor(result) * multiplier;
            Score += awardedPoints;
            return awardedPoints;
        }

        public void ResetSession()
        {
            Score = 0;
            Perfects = 0;
            Greats = 0;
            Goods = 0;
            Misses = 0;
        }
    }
}
