using System;
using UnityEngine;

namespace Surexs.DanceOff.Data
{
    [Serializable]
    public sealed class ComboMilestone
    {
        public ComboMilestone(int combo, string message)
        {
            this.combo = combo;
            this.message = message;
        }

        [Min(1)] public int combo;
        public string message;
    }

    [Serializable]
    public sealed class RhythmGameplayConfig
    {
        [Header("Timing Windows (seconds)")]
        [Min(0f)] public double perfectWindow = 0.050d;
        [Min(0f)] public double greatWindow = 0.100d;
        [Min(0f)] public double goodWindow = 0.175d;

        [Header("Base Score")]
        [Min(0)] public int perfectScore = 100;
        [Min(0)] public int greatScore = 75;
        [Min(0)] public int goodScore = 50;

        [Header("Combo Multipliers")]
        [Min(1)] public int comboFor2x = 5;
        [Min(1)] public int comboFor3x = 10;
        [Min(1)] public int comboFor4x = 20;
        [Min(1)] public int comboFor5x = 30;

        [Header("Combo Feedback")]
        public ComboMilestone[] comboMilestones =
        {
            new ComboMilestone(5, "GOOD START!"),
            new ComboMilestone(10, "COMBO!"),
            new ComboMilestone(20, "AMAZING!"),
            new ComboMilestone(30, "FANTASTIC!"),
            new ComboMilestone(50, "UNSTOPPABLE!"),
            new ComboMilestone(75, "SUREXS LEGEND!")
        };

        public bool IsValid(out string error)
        {
            if (!IsFinite(perfectWindow) || !IsFinite(greatWindow) || !IsFinite(goodWindow) ||
                perfectWindow < 0f || greatWindow < perfectWindow || goodWindow < greatWindow)
            {
                error = "Las ventanas deben cumplir 0 <= PERFECT <= GREAT <= GOOD.";
                return false;
            }

            if (perfectScore < 0 || greatScore < 0 || goodScore < 0)
            {
                error = "Las puntuaciones base no pueden ser negativas.";
                return false;
            }

            if (comboFor2x < 1 || comboFor3x <= comboFor2x || comboFor4x <= comboFor3x || comboFor5x <= comboFor4x)
            {
                error = "Los umbrales de multiplicador deben ser positivos y estar en orden ascendente.";
                return false;
            }

            if (comboMilestones == null)
            {
                error = "La lista de mensajes de combo no puede ser nula.";
                return false;
            }

            for (var index = 0; index < comboMilestones.Length; index++)
            {
                var milestone = comboMilestones[index];
                if (milestone == null || milestone.combo < 1 || string.IsNullOrWhiteSpace(milestone.message))
                {
                    error = $"El mensaje de combo {index} no es válido.";
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }

        public int MultiplierFor(int combo)
        {
            if (combo >= comboFor5x) return 5;
            if (combo >= comboFor4x) return 4;
            if (combo >= comboFor3x) return 3;
            if (combo >= comboFor2x) return 2;
            return 1;
        }

        public int BaseScoreFor(RhythmJudgmentResult result)
        {
            switch (result)
            {
                case RhythmJudgmentResult.Perfect:
                    return perfectScore;
                case RhythmJudgmentResult.Great:
                    return greatScore;
                case RhythmJudgmentResult.Good:
                    return goodScore;
                default:
                    return 0;
            }
        }

        public string MilestoneFor(int combo)
        {
            for (var index = 0; index < comboMilestones.Length; index++)
            {
                if (comboMilestones[index].combo == combo)
                {
                    return comboMilestones[index].message;
                }
            }

            return string.Empty;
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }
}
