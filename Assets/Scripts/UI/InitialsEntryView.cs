using System;
using Surexs.DanceOff.Gameplay;
using Surexs.DanceOff.Input;
using UnityEngine;
using UnityEngine.UI;

namespace Surexs.DanceOff.UI
{
    public sealed class InitialsEntryView : MonoBehaviour
    {
        private sealed class PlayerInitials
        {
            public readonly char[] Letters = { 'A', 'A', 'A' };
            public int Position;
            public bool Complete => Position >= Letters.Length;
            public string Value => new string(Letters);
        }

        private GameObject page;
        private GameObject player1Card;
        private GameObject player2Card;
        private Text player1Letters;
        private Text player2Letters;
        private Text player1Status;
        private Text player2Status;
        private Text instructions;
        private IInitialsInputSource player1Input;
        private IInitialsInputSource player2Input;
        private PlayerInitials player1;
        private PlayerInitials player2;
        private GameMode mode;
        private Action<string, string> completed;
        private float acceptInputAt;

        public void Configure(GameObject pageObject, GameObject firstPlayerCard, GameObject secondPlayerCard,
            Text firstLetters, Text secondLetters, Text firstStatus, Text secondStatus,
            Text instructionsLabel, IInitialsInputSource firstInput, IInitialsInputSource secondInput)
        {
            page = pageObject;
            player1Card = firstPlayerCard;
            player2Card = secondPlayerCard;
            player1Letters = firstLetters;
            player2Letters = secondLetters;
            player1Status = firstStatus;
            player2Status = secondStatus;
            instructions = instructionsLabel;
            player1Input = firstInput;
            player2Input = secondInput;
            if (player1Input != null) player1Input.ActionPressed += OnPlayer1Action;
            if (player2Input != null) player2Input.ActionPressed += OnPlayer2Action;
            page.SetActive(false);
        }

        public void Begin(GameMode gameMode, Action<string, string> onCompleted)
        {
            mode = gameMode;
            completed = onCompleted;
            player1 = new PlayerInitials();
            player2 = new PlayerInitials();
            player1Card.GetComponent<RectTransform>().anchoredPosition = mode == GameMode.LocalVersus
                ? new Vector2(-360f,20f)
                : new Vector2(0f,20f);
            player2Card.SetActive(mode == GameMode.LocalVersus);
            acceptInputAt = Time.unscaledTime + .25f;
            page.SetActive(true);
            Refresh();
        }

        public void Hide()
        {
            if (page != null) page.SetActive(false);
        }

        private void OnPlayer1Action(InitialsInputAction action)
        {
            ApplyAction(player1, action);
        }

        private void OnPlayer2Action(InitialsInputAction action)
        {
            if (mode == GameMode.LocalVersus) ApplyAction(player2, action);
        }

        private void ApplyAction(PlayerInitials state, InitialsInputAction action)
        {
            if (page == null || !page.activeInHierarchy || state == null ||
                Time.unscaledTime < acceptInputAt) return;

            if (action == InitialsInputAction.Back)
            {
                state.Position=Mathf.Max(0,state.Position-1);
            }
            else if (!state.Complete && action == InitialsInputAction.Confirm)
            {
                state.Position++;
            }
            else if (!state.Complete && (action == InitialsInputAction.PreviousLetter ||
                                          action == InitialsInputAction.NextLetter))
            {
                var delta = action == InitialsInputAction.PreviousLetter ? -1 : 1;
                var value = state.Letters[state.Position] - 'A';
                state.Letters[state.Position] = (char)('A' + (value + delta + 26) % 26);
            }

            Refresh();
            if (!player1.Complete || mode == GameMode.LocalVersus && !player2.Complete) return;
            var callback = completed;
            completed = null;
            callback?.Invoke(player1.Value, mode == GameMode.LocalVersus ? player2.Value : null);
        }

        private void Refresh()
        {
            Render(player1, player1Letters, player1Status);
            if (mode == GameMode.LocalVersus) Render(player2, player2Letters, player2Status);
            instructions.text = mode == GameMode.LocalVersus
                ? "CADA JUGADOR: ↑ / ↓ CAMBIA  •  A CONFIRMA  •  B REGRESA"
                : "↑ / ↓ CAMBIA LA LETRA  •  A CONFIRMA  •  B REGRESA";
        }

        private static void Render(PlayerInitials state, Text letters, Text status)
        {
            var value = string.Empty;
            for (var index = 0; index < state.Letters.Length; index++)
            {
                var active = !state.Complete && index == state.Position;
                var color = active ? "#FFD43B" : "#F5FAFF";
                var size = active ? 88 : 72;
                value += $"<size={size}><color={color}>{state.Letters[index]}</color></size>";
                if (index < state.Letters.Length - 1) value += "   ";
            }
            letters.text = value;
            status.text = state.Complete ? "LISTO" : $"LETRA {state.Position + 1} DE 3";
            status.color = state.Complete ? SurexsVisualTheme.Success : SurexsVisualTheme.TextSecondary;
        }

        private void OnDestroy()
        {
            if (player1Input != null) player1Input.ActionPressed -= OnPlayer1Action;
            if (player2Input != null) player2Input.ActionPressed -= OnPlayer2Action;
        }
    }
}
