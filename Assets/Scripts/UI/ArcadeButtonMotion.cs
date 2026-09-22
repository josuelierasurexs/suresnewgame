using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Surexs.DanceOff.UI
{
    public sealed class ArcadeButtonMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
    {
        private const float HoverScale = 1.10f;
        private const float SelectedScale = 1.12f;
        private const float PressedScale = 1.04f;

        private Vector3 targetScale = Vector3.one;
        private Graphic targetGraphic;
        private Outline glow;
        private Color baseColor = Color.white;
        private Color targetColor = Color.white;
        private Color targetGlow = Color.clear;
        private bool pointerInside;
        private bool pointerDown;
        private bool selected;

        private void Awake()
        {
            targetGraphic = GetComponent<Graphic>();
            if (targetGraphic != null) baseColor = targetGraphic.color;
            glow = GetComponent<Outline>();
            if (glow == null) glow = gameObject.AddComponent<Outline>();
            glow.effectDistance = new Vector2(5f, -5f);
            glow.useGraphicAlpha = true;
            glow.effectColor = Color.clear;
            RefreshTargets();
        }

        private void Update()
        {
            var blend = 20f * Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, blend);
            if (targetGraphic != null)
                targetGraphic.color = Color.Lerp(targetGraphic.color, targetColor, blend);
            if (glow != null)
                glow.effectColor = Color.Lerp(glow.effectColor, targetGlow, blend);
        }

        private void OnDisable()
        {
            pointerInside = false;
            pointerDown = false;
            selected = false;
            transform.localScale = Vector3.one;
            if (targetGraphic != null) targetGraphic.color = baseColor;
            if (glow != null) glow.effectColor = Color.clear;
            RefreshTargets();
        }

        public void OnPointerEnter(PointerEventData eventData) { pointerInside = true; RefreshTargets(); }
        public void OnPointerExit(PointerEventData eventData) { pointerInside = false; pointerDown = false; RefreshTargets(); }
        public void OnPointerDown(PointerEventData eventData) { pointerDown = true; RefreshTargets(); }
        public void OnPointerUp(PointerEventData eventData) { pointerDown = false; RefreshTargets(); }
        public void OnSelect(BaseEventData eventData) { selected = true; RefreshTargets(); }
        public void OnDeselect(BaseEventData eventData) { selected = false; RefreshTargets(); }

        private void RefreshTargets()
        {
            var active = selected || pointerInside;
            targetScale = Vector3.one * (pointerDown ? PressedScale : selected ? SelectedScale : active ? HoverScale : 1f);
            targetColor = active ? Brighten(baseColor, 1.18f) : baseColor;
            targetGlow = active ? new Color(.08f, .78f, 1f, selected ? .92f : .72f) : Color.clear;
        }

        private static Color Brighten(Color color, float amount)
        {
            return new Color(color.r * amount, color.g * amount, color.b * amount, color.a);
        }
    }
}
