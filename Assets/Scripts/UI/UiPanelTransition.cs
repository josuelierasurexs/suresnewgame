using UnityEngine;

namespace Surexs.DanceOff.UI
{
    public sealed class UiPanelTransition : MonoBehaviour
    {
        private CanvasGroup group;
        private bool visible;
        private float target;

        private void Awake() { group = GetComponent<CanvasGroup>(); if (group == null) group = gameObject.AddComponent<CanvasGroup>(); }
        public void SetVisible(bool show, bool immediate = false)
        {
            visible = show; target = show ? 1f : 0f; gameObject.SetActive(true);
            group.interactable = show; group.blocksRaycasts = show;
            if (immediate) { group.alpha = target; transform.localScale = show ? Vector3.one : Vector3.one * .97f; }
        }
        private void Update()
        {
            group.alpha = Mathf.MoveTowards(group.alpha, target, 7f * Time.unscaledDeltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, visible ? Vector3.one : Vector3.one * .97f, 16f * Time.unscaledDeltaTime);
            if (!visible && group.alpha <= 0f) gameObject.SetActive(false);
        }
    }
}
