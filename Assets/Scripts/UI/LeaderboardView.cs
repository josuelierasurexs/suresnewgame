using System.Text;
using Surexs.DanceOff.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Surexs.DanceOff.UI
{
    public sealed class LeaderboardView : MonoBehaviour
    {
        private GameObject page;
        private Text entries;
        private Button continueButton;

        public void Configure(GameObject pageObject, Text entriesLabel, Button actionButton)
        {
            page = pageObject;
            entries = entriesLabel;
            continueButton = actionButton;
            page.SetActive(false);
        }

        public void Show()
        {
            entries.text = Format();
            page.SetActive(true);
            if (EventSystem.current != null && continueButton != null)
                EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
        }

        public void Hide()
        {
            if (page != null) page.SetActive(false);
        }

        private static string Format()
        {
            var topEntries = LeaderboardStore.GetTop(10);
            if (topEntries.Count == 0) return "<color=#99B8D8>AÚN NO HAY REGISTROS</color>";

            var builder = new StringBuilder();
            builder.AppendLine("<color=#2FC8FF>POS     INICIALES          SCORE          PREMIOS</color>");
            for (var index = 0; index < topEntries.Count; index++)
            {
                var entry = topEntries[index];
                var rankColor = index < 3 ? "#FFD43B" : "#F5FAFF";
                builder.Append("<color=").Append(rankColor).Append('>')
                    .Append((index + 1).ToString("00")).Append("      ")
                    .Append(entry.initials).Append("          ")
                    .Append(entry.score.ToString("N0").PadLeft(7)).Append("        ")
                    .Append(entry.prizeScore.ToString("N0").PadLeft(4))
                    .AppendLine("</color>");
            }
            return builder.ToString();
        }
    }
}
