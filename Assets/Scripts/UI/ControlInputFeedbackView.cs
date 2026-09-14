using Surexs.DanceOff.Data;
using Surexs.DanceOff.Input;
using UnityEngine;
using UnityEngine.UI;

namespace Surexs.DanceOff.UI
{
    public sealed class ControlInputFeedbackView : MonoBehaviour
    {
        private IRhythmInputSource input;
        private Image[] indicators;
        private Color[] baseColors;
        private float[] remaining;
        private const float FlashDuration = 0.12f;

        public void Configure(IRhythmInputSource source, Image left, Image center, Image right)
        {
            Disconnect(); input=source; indicators=new[] {left,center,right}; remaining=new float[3];
            baseColors=new[] {left.color,center.color,right.color}; input.DirectionPressed += OnDirectionPressed;
        }

        private void OnDirectionPressed(RhythmDirection direction)
        {
            var index=(int)direction; remaining[index]=FlashDuration;
            indicators[index].color=Color.white; indicators[index].rectTransform.localScale=Vector3.one*1.12f;
        }

        private void Update()
        {
            if (indicators == null) return;
            for (var i=0;i<3;i++)
            {
                if (remaining[i] <= 0f) continue;
                remaining[i]-=Time.unscaledDeltaTime;
                var amount=Mathf.Clamp01(remaining[i]/FlashDuration);
                indicators[i].color=Color.Lerp(baseColors[i],Color.white,amount);
                indicators[i].rectTransform.localScale=Vector3.one*Mathf.Lerp(1f,1.12f,amount);
            }
        }

        private void OnDestroy() { Disconnect(); }
        private void Disconnect() { if (input != null) input.DirectionPressed -= OnDirectionPressed; }
    }
}
