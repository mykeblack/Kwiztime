using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Kwiztime.UI
{
    /// <summary>
    /// Pulses a Unity UI Outline by animating effect distance and alpha.
    /// Attach to the same GameObject as the Outline (your answer Button).
    /// </summary>
    [RequireComponent(typeof(Outline))]
    public class OutlinePulse : MonoBehaviour
    {
        [SerializeField] private Outline outline;

        [Header("Pulse")]
        [SerializeField] private float duration = 0.6f;
        [SerializeField] private int pulses = 2;
        [SerializeField] private Vector2 baseDistance = new Vector2(6f, -6f);
        [SerializeField] private Vector2 peakDistance = new Vector2(12f, -12f);

        [Header("Alpha")]
        [SerializeField, Range(0f, 1f)] private float baseAlpha = 0.65f;
        [SerializeField, Range(0f, 1f)] private float peakAlpha = 1.0f;

        private Coroutine _routine;

        private void Awake()
        {
            if (outline == null)
                outline = GetComponent<Outline>();
        }

        public void Play()
        {
            if (outline == null) return;

            if (_routine != null)
                StopCoroutine(_routine);

            _routine = StartCoroutine(PulseRoutine());
        }

        public void Stop()
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }
        }

        private IEnumerator PulseRoutine()
        {
            outline.enabled = true;

            Color c = outline.effectColor;
            c.a = baseAlpha;
            outline.effectColor = c;

            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float normalized = Mathf.Clamp01(t / duration);

                // pulses times: sin wave 0..1
                float wave = Mathf.Sin(normalized * Mathf.PI * pulses);
                float k = Mathf.Abs(wave); // 0..1..0..1..0

                outline.effectDistance = Vector2.Lerp(baseDistance, peakDistance, k);

                Color col = outline.effectColor;
                col.a = Mathf.Lerp(baseAlpha, peakAlpha, k);
                outline.effectColor = col;

                yield return null;
            }

            // Return to base
            outline.effectDistance = baseDistance;
            Color end = outline.effectColor;
            end.a = baseAlpha;
            outline.effectColor = end;

            _routine = null;
        }
    }
}