using System;

namespace Surexs.DanceOff.Data
{
    public enum RhythmDirection
    {
        Left,
        Center,
        Right
    }

    public enum RhythmNoteStatus
    {
        Pending,
        Hit,
        Missed
    }

    public enum RhythmJudgmentResult
    {
        Perfect,
        Great,
        Good,
        Miss
    }

    public sealed class ChartEvent
    {
        public ChartEvent(float time, RhythmDirection direction, string pose)
        {
            Time = time;
            Direction = direction;
            Pose = pose;
        }

        public float Time { get; }
        public RhythmDirection Direction { get; }
        public string Pose { get; }
    }

    [Serializable]
    internal sealed class ChartJson
    {
        public string song;
        public float visualSpeed = 1f;
        public ChartTileJson[] tiles;
    }

    [Serializable]
    internal sealed class ChartTileJson
    {
        public float time;
        public string direction;
        public string pose;
    }
}
