using System;
using Surexs.DanceOff.Data;

namespace Surexs.DanceOff.Input
{
    public interface IRhythmInputSource
    {
        event Action<RhythmDirection> DirectionPressed;
    }

}
