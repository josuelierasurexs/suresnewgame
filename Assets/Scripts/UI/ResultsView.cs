using System;
using Surexs.DanceOff.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Surexs.DanceOff.UI
{
    public sealed class ResultsView : MonoBehaviour
    {
        private GameObject panel;
        private Text title;
        private Text player1;
        private Text player2;
        private GameObject player1Card;
        private GameObject player2Card;
        private Button rematch;
        private Button menu;
        public event Action RematchRequested;
        public event Action MenuRequested;

        public void Configure(GameObject panelObject, Text titleLabel, Text player1Label, Text player2Label,
            GameObject player1CardObject, GameObject player2CardObject, Button rematchButton, Button menuButton)
        {
            panel=panelObject; title=titleLabel; player1=player1Label; player2=player2Label;
            player1Card=player1CardObject; player2Card=player2CardObject; rematch=rematchButton; menu=menuButton;
            rematch.onClick.RemoveListener(RequestRematch);
            rematch.onClick.AddListener(RequestRematch);
            menu.onClick.RemoveListener(RequestMenu);
            menu.onClick.AddListener(RequestMenu);
            Hide();
        }

        public void Show(GameResult result)
        {
            panel.SetActive(true);
            if (result.Mode == GameMode.Solo)
            {
                player1.rectTransform.anchoredPosition=new Vector2(0,60);
                player1Card.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,55);
                player2Card.SetActive(false);
                title.text="RESULTADOS";
                player1.text=Format("PLAYER 1",result.Player1);
                player2.text=string.Empty;
            }
            else
            {
                player1.rectTransform.anchoredPosition=new Vector2(-430,60);
                player1Card.GetComponent<RectTransform>().anchoredPosition=new Vector2(-430,55);
                player2Card.SetActive(true);
                title.text=result.Outcome == VersusOutcome.Player1Wins ? "PLAYER 1 WINS" :
                    result.Outcome == VersusOutcome.Player2Wins ? "PLAYER 2 WINS" : "DRAW";
                player1.text=Format("PLAYER 1",result.Player1);
                player2.text=Format("PLAYER 2",result.Player2);
            }
            rematch.GetComponentInChildren<Text>().text=result.Mode == GameMode.Solo ? "REINTENTAR" : "REVANCHA";
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(rematch.gameObject);
        }

        public void Hide() { if (panel != null) panel.SetActive(false); }
        private void RequestRematch() { RematchRequested?.Invoke(); }
        private void RequestMenu() { MenuRequested?.Invoke(); }
        private static string Format(string name, PlayerResult r) => $"{name}\n\nSCORE  {r.Score}\n\n"+
            $"PERFECT  {r.Perfects}\nGREAT  {r.Greats}\nGOOD  {r.Goods}\nMISS  {r.Misses}\n\n"+
            $"MAX COMBO  {r.MaxCombo}\nACCURACY  {r.Accuracy:P1}";
        private void OnDestroy() { if (rematch != null) rematch.onClick.RemoveListener(RequestRematch); if (menu != null) menu.onClick.RemoveListener(RequestMenu); }
    }
}
