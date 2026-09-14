namespace Surexs.DanceOff.Gameplay
{
    public enum GameMode { Solo, LocalVersus }
    public enum GameFlowState { Gameplay, Results }
    public enum VersusOutcome { Tie, Player1Wins, Player2Wins }

    public readonly struct PlayerResult
    {
        public PlayerResult(int score, int perfects, int greats, int goods, int misses, int maxCombo, int multiplier)
        {
            Score = score; Perfects = perfects; Greats = greats; Goods = goods; Misses = misses;
            MaxCombo = maxCombo; FinalMultiplier = multiplier;
        }
        public int Score { get; }
        public int Perfects { get; }
        public int Greats { get; }
        public int Goods { get; }
        public int Misses { get; }
        public int MaxCombo { get; }
        public int FinalMultiplier { get; }
        public int TotalNotes => Perfects + Greats + Goods + Misses;
        public double Accuracy => CalculateAccuracy(Perfects, Greats, Goods, TotalNotes);

        public static double CalculateAccuracy(int perfects, int greats, int goods, int totalNotes)
        {
            if (totalNotes <= 0) return 0d;
            return (perfects * 100d + greats * 75d + goods * 50d) / (totalNotes * 100d);
        }
    }

    public readonly struct GameResult
    {
        public GameResult(GameMode mode, PlayerResult player1, PlayerResult player2)
        { Mode = mode; Player1 = player1; Player2 = player2; }
        public GameMode Mode { get; }
        public PlayerResult Player1 { get; }
        public PlayerResult Player2 { get; }
        public VersusOutcome Outcome => VersusOutcomeResolver.Determine(Player1.Score, Player2.Score);
    }

    public static class VersusOutcomeResolver
    {
        public static VersusOutcome Determine(int player1Score, int player2Score)
        {
            if (player1Score == player2Score) return VersusOutcome.Tie;
            return player1Score > player2Score ? VersusOutcome.Player1Wins : VersusOutcome.Player2Wins;
        }
    }

    public sealed class LocalVersusMatchState : UnityEngine.MonoBehaviour
    {
        private ScoreManager player1Score;
        private ScoreManager player2Score;
        public int Player1Score => player1Score != null ? player1Score.Score : 0;
        public int Player2Score => player2Score != null ? player2Score.Score : 0;
        public VersusOutcome Outcome => VersusOutcomeResolver.Determine(Player1Score, Player2Score);
        public void Configure(ScoreManager score1, ScoreManager score2) { player1Score = score1; player2Score = score2; }
    }
}
