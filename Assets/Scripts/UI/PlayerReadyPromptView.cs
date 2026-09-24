using UnityEngine;
using UnityEngine.UI;

namespace Surexs.DanceOff.UI
{
    public sealed class PlayerReadyPromptView : MonoBehaviour
    {
        private Text label;
        private Image progressFill;
        private Outline outline;
        private string playerName;
        private Color accent;

        public void Configure(string configuredPlayerName, Text configuredLabel, Image configuredProgressFill,
            Outline configuredOutline, Color configuredAccent)
        {
            playerName = configuredPlayerName;
            label = configuredLabel;
            progressFill = configuredProgressFill;
            outline = configuredOutline;
            accent = configuredAccent;
            SetState(0f, false, true, false);
        }

        public void SetState(float progress, bool ready, bool available, bool deviceConflict)
        {
            progressFill.fillAmount = ready ? 1f : Mathf.Clamp01(progress);
            progressFill.color = ready ? new Color(.16f, .95f, .52f, .78f) : new Color(accent.r, accent.g, accent.b, .68f);
            outline.effectColor = ready
                ? new Color(.28f, 1f, .62f, 1f)
                : new Color(accent.r, accent.g, accent.b, .95f);

            if (deviceConflict)
                label.text = $"{playerName} READY\nCONTROL DUPLICADO";
            else if (!available)
                label.text = $"{playerName} READY\nCONECTA TU CONTROL";
            else if (ready)
                label.text = $"{playerName} READY\n¡LISTO!";
            else if (progress > 0f)
                label.text = $"{playerName} READY\nMANTÉN  {Mathf.RoundToInt(progress * 100f)}%";
            else
                label.text = $"{playerName} READY\nMANTÉN UN BOTÓN";
        }
    }
}
