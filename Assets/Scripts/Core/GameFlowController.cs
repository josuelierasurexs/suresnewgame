using Surexs.DanceOff.Gameplay;
using Surexs.DanceOff.Player;
using Surexs.DanceOff.Rhythm;
using Surexs.DanceOff.UI;
using UnityEngine;

namespace Surexs.DanceOff.Core
{
    public sealed class GameFlowController : MonoBehaviour
    {
        private GameMode mode;
        private RhythmPrototypeController gameplay;
        private PlayerSession[] players;
        private ResultsView resultsView;

        public GameFlowState State { get; private set; }
        public GameResult LastResult { get; private set; }

        public void Configure(GameMode gameMode, RhythmPrototypeController controller, PlayerSession[] playerSessions,
            ResultsView view)
        {
            Disconnect();
            mode = gameMode;
            gameplay = controller;
            players = playerSessions;
            resultsView = view;
            gameplay.ChartCompleted += OnChartCompleted;
            resultsView.RematchRequested += StartSession;
        }

        private void Start()
        {
            StartSession();
        }

        public void StartSession()
        {
            for (var index = 0; index < players.Length; index++) players[index].Reset();
            resultsView.Hide();
            State = GameFlowState.Gameplay;
            if (!gameplay.StartSession()) Debug.LogError("[GameFlow] No fue posible iniciar la partida.", this);
        }

        private void OnChartCompleted()
        {
            for (var index = 0; index < players.Length; index++) players[index].Stop();
            var p1 = players[0].CaptureResult();
            var p2 = mode == GameMode.LocalVersus ? players[1].CaptureResult() : default;
            LastResult = new GameResult(mode, p1, p2);
            State = GameFlowState.Results;
            resultsView.Show(LastResult);
        }

        private void OnDestroy() { Disconnect(); }
        private void Disconnect()
        {
            if (gameplay != null) gameplay.ChartCompleted -= OnChartCompleted;
            if (resultsView != null) resultsView.RematchRequested -= StartSession;
        }
    }

    public sealed class PlayerSession
    {
        private readonly RhythmJudge judge;
        private readonly TileSpawner tiles;
        private readonly ScoreManager score;
        private readonly ComboManager combo;
        private readonly GameplayFeedbackView feedback;
        private readonly PlayerController player;

        public PlayerSession(RhythmJudge rhythmJudge, TileSpawner tileSpawner, ScoreManager scoreManager,
            ComboManager comboManager, GameplayFeedbackView feedbackView, PlayerController playerController)
        { judge=rhythmJudge; tiles=tileSpawner; score=scoreManager; combo=comboManager; feedback=feedbackView; player=playerController; }

        public void Reset() { score.ResetSession(); combo.ResetSession(); feedback.ResetView(); player.ResetVisual(); }
        public void Stop() { judge.StopJudging(); tiles.StopSpawning(); }
        public PlayerResult CaptureResult() => new PlayerResult(score.Score, score.Perfects, score.Greats,
            score.Goods, score.Misses, combo.MaxCombo, combo.CurrentMultiplier);
    }
}
