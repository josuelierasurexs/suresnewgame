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
        private UiPanelTransition transition;
        public event Action RematchRequested;
        public event Action MenuRequested;

        public void Configure(GameObject panelObject, Text titleLabel, Text player1Label, Text player2Label,
            GameObject player1CardObject, GameObject player2CardObject, Button rematchButton, Button menuButton)
        {
            panel=panelObject; title=titleLabel; player1=player1Label; player2=player2Label;
            player1Card=player1CardObject; player2Card=player2CardObject; rematch=rematchButton; menu=menuButton;
            transition=panel.GetComponent<UiPanelTransition>();
            if (transition == null) transition=panel.AddComponent<UiPanelTransition>();
            rematch.onClick.RemoveListener(RequestRematch);
            rematch.onClick.AddListener(RequestRematch);
            menu.onClick.RemoveListener(RequestMenu);
            menu.onClick.AddListener(RequestMenu);
            Hide();
        }

        public void Show(GameResult result)
        {
            transition.SetVisible(true);
            if (result.Mode == GameMode.Solo)
            {
                player1.rectTransform.anchoredPosition=new Vector2(0,60);
                player1Card.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,55);
                player2Card.SetActive(false);
                title.text="RESULTADOS";
                title.color=SurexsVisualTheme.Accent;
                player1.text=Format("PLAYER 1",result.Player1);
                player2.text=string.Empty;
            }
            else
            {
                player1.rectTransform.anchoredPosition=new Vector2(-430,60);
                player1Card.GetComponent<RectTransform>().anchoredPosition=new Vector2(-430,55);
                player2Card.SetActive(true);
                title.text=result.Outcome == VersusOutcome.Player1Wins ? "RESULTADOS  •  PLAYER 1 GANA" :
                    result.Outcome == VersusOutcome.Player2Wins ? "RESULTADOS  •  PLAYER 2 GANA" : "RESULTADOS  •  EMPATE";
                title.color=result.Outcome == VersusOutcome.Player1Wins ? SurexsVisualTheme.Primary :
                    result.Outcome == VersusOutcome.Player2Wins ? SurexsVisualTheme.Secondary : SurexsVisualTheme.Accent;
                player1.text=Format("PLAYER 1",result.Player1);
                player2.text=Format("PLAYER 2",result.Player2);
            }
            rematch.GetComponentInChildren<Text>().text=result.Mode == GameMode.Solo ? "REINTENTAR" : "REVANCHA";
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(rematch.gameObject);
        }

        public void Hide() { if (transition != null) transition.SetVisible(false,true); else if (panel != null) panel.SetActive(false); }
        private void RequestRematch() { RematchRequested?.Invoke(); }
        private void RequestMenu() { MenuRequested?.Invoke(); }
        private static string Format(string name, PlayerResult r) => $"<size=24><color=#9CB8DC>{name}</color></size>\n"+
            $"<size=46>SCORE  {r.Score:N0}</size>\n<size=31><color=#FFC229>ACCURACY  {r.Accuracy:P1}</color></size>\n\n"+
            $"PERFECT  {r.Perfects}\nGREAT  {r.Greats}\nGOOD  {r.Goods}\n<color=#FF405F>MISS  {r.Misses}</color>\n\n"+
            $"<size=29>MAX COMBO  {r.MaxCombo}</size>";
        private void OnDestroy() { if (rematch != null) rematch.onClick.RemoveListener(RequestRematch); if (menu != null) menu.onClick.RemoveListener(RequestMenu); }
    }
}
