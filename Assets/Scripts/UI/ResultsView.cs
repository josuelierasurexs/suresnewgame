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
        private Text player1Header;
        private Text player2Header;
        private GameObject player1Card;
        private GameObject player2Card;
        private Button rematch;
        private Button menu;
        private UiPanelTransition transition;
        public event Action RematchRequested;
        public event Action MenuRequested;

        public void Configure(GameObject panelObject, Text titleLabel, Text player1Label, Text player2Label,
            Text player1HeaderLabel,Text player2HeaderLabel,GameObject player1CardObject,
            GameObject player2CardObject, Button rematchButton, Button menuButton)
        {
            panel=panelObject; title=titleLabel; player1=player1Label; player2=player2Label;
            player1Header=player1HeaderLabel; player2Header=player2HeaderLabel;
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
                ConfigureCard(player1Card,player1,new Vector2(125,35),new Vector2(650,725),new Vector2(0,-85),new Vector2(540,520));
                player2Card.SetActive(false);
                title.gameObject.SetActive(true);
                player1Header.gameObject.SetActive(true);
                player1Header.rectTransform.anchoredPosition=new Vector2(0,220);
                player1Header.rectTransform.sizeDelta=new Vector2(460,48);
                player1Header.fontSize=30;
                player1Header.text="PLAYER 1";
                player1Header.color=SurexsVisualTheme.Primary;
                title.rectTransform.anchoredPosition=new Vector2(125,325);
                title.rectTransform.sizeDelta=new Vector2(520,82);
                title.font=SurexsVisualTheme.TitleFont;
                title.fontStyle=FontStyle.Normal;
                title.fontSize=48;
                title.text="RESULTADOS";
                title.color=SurexsVisualTheme.TextPrimary;
                player1.text=Format(result.Player1,false);
                player2.text=string.Empty;
            }
            else
            {
                player2Card.SetActive(true);
                title.gameObject.SetActive(false);
                ConfigureCard(player1Card,player1,new Vector2(-210,35),new Vector2(560,625),new Vector2(0,-58),new Vector2(470,450));
                ConfigureCard(player2Card,player2,new Vector2(420,35),new Vector2(560,625),new Vector2(0,-58),new Vector2(470,450));
                ConfigureVersusHeader(player1Header,"PLAYER 1",result.Outcome==VersusOutcome.Player1Wins,result.Outcome==VersusOutcome.Tie,SurexsVisualTheme.Primary);
                ConfigureVersusHeader(player2Header,"PLAYER 2",result.Outcome==VersusOutcome.Player2Wins,result.Outcome==VersusOutcome.Tie,SurexsVisualTheme.Secondary);
                player1.text=Format(result.Player1,true);
                player2.text=Format(result.Player2,true);
            }
            rematch.GetComponentInChildren<Text>().text=result.Mode == GameMode.Solo ? "REINTENTAR" : "REVANCHA";
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(rematch.gameObject);
        }

        public void Hide() { if (transition != null) transition.SetVisible(false,true); else if (panel != null) panel.SetActive(false); }
        private void RequestRematch() { RematchRequested?.Invoke(); }
        private void RequestMenu() { MenuRequested?.Invoke(); }

        private static void ConfigureCard(GameObject card,Text content,Vector2 position,Vector2 size,
            Vector2 contentPosition,Vector2 contentSize)
        {
            var cardRect=card.GetComponent<RectTransform>();
            cardRect.anchoredPosition=position;
            cardRect.sizeDelta=size;
            content.rectTransform.anchoredPosition=contentPosition;
            content.rectTransform.sizeDelta=contentSize;
            content.alignment=TextAnchor.UpperCenter;
            content.lineSpacing=.9f;
        }

        private static void ConfigureVersusHeader(Text header,string player,bool winner,bool draw,Color color)
        {
            header.gameObject.SetActive(true);
            header.rectTransform.anchoredPosition=new Vector2(0,255);
            header.rectTransform.sizeDelta=new Vector2(470,42);
            header.font=SurexsVisualTheme.TitleFont;
            header.fontStyle=FontStyle.Normal;
            header.fontSize=winner || draw ? 23 : 27;
            header.text=winner ? $"{player}  •  GANADOR" : draw ? $"{player}  •  EMPATE" : player;
            header.color=winner || draw ? SurexsVisualTheme.Accent : color;
        }

        private static string Format(PlayerResult r,bool compact)
        {
            var scoreLabelSize=compact ? 24 : 30;
            var scoreSize=compact ? 58 : 82;
            var accuracySize=compact ? 23 : 30;
            var statSize=compact ? 21 : 27;
            var comboSize=compact ? 25 : 32;
            return $"<size={scoreLabelSize}>SCORE</size>\n"+
                   $"<size={scoreSize}>{r.Score:N0}</size>\n"+
                   $"<size={accuracySize}>ACCURACY  <color=#FFD43B>{r.Accuracy:P1}</color></size>\n\n"+
                   $"<size={statSize}><color=#2FC8FF>PERFECT</color>  {r.Perfects}\n"+
                   $"<color=#50E09A>GREAT</color>  {r.Greats}\n"+
                   $"GOOD  {r.Goods}\n"+
                   $"<color=#FF5269>MISS  {r.Misses}</color></size>\n\n"+
                   $"<size={comboSize}>MAX COMBO  <color=#FFD43B>{r.MaxCombo}</color></size>";
        }
        private void OnDestroy() { if (rematch != null) rematch.onClick.RemoveListener(RequestRematch); if (menu != null) menu.onClick.RemoveListener(RequestMenu); }
    }
}
