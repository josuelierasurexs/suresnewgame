using System;
using Surexs.DanceOff.Data;
using UnityEngine;

namespace Surexs.DanceOff.Gameplay
{
    public sealed class ComboManager : MonoBehaviour
    {
        private RhythmGameplayConfig config;

        public int CurrentCombo { get; private set; }
        public int MaxCombo { get; private set; }
        public int CurrentMultiplier => config != null ? config.MultiplierFor(CurrentCombo) : 1;

        public void Configure(RhythmGameplayConfig gameplayConfig)
        {
            config = gameplayConfig;
        }

        public string RegisterHit()
        {
            CurrentCombo++;
            MaxCombo = Math.Max(MaxCombo, CurrentCombo);
            return config != null ? config.MilestoneFor(CurrentCombo) : string.Empty;
        }

        public void RegisterMiss()
        {
            CurrentCombo = 0;
        }

        public void ResetSession()
        {
            CurrentCombo = 0;
            MaxCombo = 0;
        }

    }
}
