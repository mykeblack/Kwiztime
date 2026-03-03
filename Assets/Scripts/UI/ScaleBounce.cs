using System.Collections;
using UnityEngine;

namespace Kwiztime.UI
{
    /// <summary>
    /// Small scale bounce animation for UI elements.
    /// Safe for buttons (uses localScale).
    /// </summary>
    public class ScaleBounce : MonoBehaviour
    {
        [Header("Bounce Settings")]
        [SerializeField] private float duration = 0.35f;
        [SerializeField] private float scaleMultiplier = 1.08f;
        [SerializeField] private AnimationCurve curve =
            AnimationCurve.EaseInOut(0, 0, 1, 1);

        private Vector3 _baseScale;
        private Coroutine _routine;

        private void Awake()
        {
            _baseScale = transform.localScale;
        }

        public void Play()
        {
            if (_routine != null)
                StopCoroutine(_routine);

            _routine = StartCoroutine(BounceRoutine());
        }

        private IEnumerator BounceRoutine()
        {
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float k = curve.Evaluate(Mathf.Clamp01(t / duration));

                float scale = Mathf.Lerp(1f, scaleMultiplier, k);
                transform.localScale = _baseScale * scale;

                yield return null;
            }

            // Return to base scale
            transform.localScale = _baseScale;
            _routine = null;
        }

        private void OnDisable()
        {
            transform.localScale = _baseScale;
        }
    }
}