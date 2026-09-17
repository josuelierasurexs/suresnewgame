using UnityEngine;
using UnityEngine.EventSystems;

namespace Surexs.DanceOff.UI
{
    public sealed class ArcadeButtonMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
    {
        private Vector3 targetScale = Vector3.one;
        private bool selected;
        private void Update() { transform.localScale = Vector3.Lerp(transform.localScale, targetScale, 18f * Time.unscaledDeltaTime); }
        public void OnPointerEnter(PointerEventData eventData) { targetScale = Vector3.one * 1.035f; }
        public void OnPointerExit(PointerEventData eventData) { targetScale = selected ? Vector3.one * 1.035f : Vector3.one; }
        public void OnPointerDown(PointerEventData eventData) { targetScale = Vector3.one * .97f; }
        public void OnPointerUp(PointerEventData eventData) { targetScale = Vector3.one * 1.035f; }
        public void OnSelect(BaseEventData eventData) { selected = true; targetScale = Vector3.one * 1.035f; }
        public void OnDeselect(BaseEventData eventData) { selected = false; targetScale = Vector3.one; }
    }
}
