using UnityEngine;

namespace Surexs.DanceOff.UI
{
    public sealed class MenuLogoPulse : MonoBehaviour
    {
        [SerializeField, Range(0.01f, 0.15f)] private float amplitude = 0.045f;
        [SerializeField, Min(0.1f)] private float speed = 2.4f;
        private Vector3 baseScale;

        private void Awake()
        {
            baseScale = transform.localScale;
        }

        private void Update()
        {
            var scale = 1f + Mathf.Sin(Time.unscaledTime * speed) * amplitude;
            transform.localScale = baseScale * scale;
        }
    }
}
