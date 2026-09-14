using Surexs.DanceOff.Data;

namespace Surexs.DanceOff.Rhythm
{
    public readonly struct RhythmJudgment
    {
        public RhythmJudgment(ChartEvent chartEvent, RhythmJudgmentResult result, double deltaSeconds,
            bool isInputMiss = false, RhythmDirection inputDirection = default)
        {
            ChartEvent = chartEvent;
            Result = result;
            DeltaSeconds = deltaSeconds;
            IsInputMiss = isInputMiss;
            InputDirection = inputDirection;
        }

        public ChartEvent ChartEvent { get; }
        public RhythmJudgmentResult Result { get; }
        public double DeltaSeconds { get; }
        public bool IsInputMiss { get; }
        public RhythmDirection InputDirection { get; }
        public bool IsHit => Result != RhythmJudgmentResult.Miss;
        public RhythmNoteStatus NoteStatus => IsHit ? RhythmNoteStatus.Hit : RhythmNoteStatus.Missed;

        public static RhythmJudgment InputMiss(RhythmDirection direction)
        {
            return new RhythmJudgment(null, RhythmJudgmentResult.Miss, 0d, true, direction);
        }
    }
}
