using Surexs.DanceOff.Gameplay;

namespace Surexs.DanceOff.Core
{
    public static class GameSession
    {
        public static GameMode SelectedMode { get; private set; } = GameMode.LocalVersus;
        public static void SelectMode(GameMode mode) { SelectedMode = mode; }
    }
}
