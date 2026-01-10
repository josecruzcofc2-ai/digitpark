using UnityEngine;
using UnityEngine.UI;

namespace DigitPark.UI
{
    /// <summary>
    /// Componente para dar efecto de glow pulsante a los bordes de las grids
    /// Crea un efecto visual dinámico que hace que las grids se vean "vivas"
    /// </summary>
    [RequireComponent(typeof(Outline))]
    public class GridGlowPulse : MonoBehaviour
    {
        [Header("Pulse Settings")]
        [SerializeField] private float pulseSpeed = 1.5f;
        [SerializeField] private float minAlpha = 0.4f;
        [SerializeField] private float maxAlpha = 1f;
        [SerializeField] private float minIntensity = 0.7f;
        [SerializeField] private float maxIntensity = 1.2f;

        [Header("Distance Pulse")]
        [SerializeField] private bool pulseDistance = true;
        [SerializeField] private float minDistance = 2f;
        [SerializeField] private float maxDistance = 3.5f;

        private Outline outline;
        private Color baseColor;
        private float randomOffset;

        private void Awake()
        {
            outline = GetComponent<Outline>();
            if (outline != null)
            {
                baseColor = outline.effectColor;
            }
            randomOffset = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            if (outline == null) return;

            float t = (Mathf.Sin(Time.time * pulseSpeed + randomOffset) + 1f) * 0.5f;

            // Pulso de color/alpha
            float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

            Color newColor = new Color(
                baseColor.r * intensity,
                baseColor.g * intensity,
                baseColor.b * intensity,
                alpha
            );
            outline.effectColor = newColor;

            // Pulso de distancia
            if (pulseDistance)
            {
                float distance = Mathf.Lerp(minDistance, maxDistance, t);
                outline.effectDistance = new Vector2(distance, -distance);
            }
        }

        /// <summary>
        /// Cambia el color base del glow
        /// </summary>
        public void SetBaseColor(Color color)
        {
            baseColor = color;
        }

        /// <summary>
        /// Intensifica temporalmente el glow (para eventos especiales)
        /// </summary>
        public void FlashGlow()
        {
            StartCoroutine(FlashCoroutine());
        }

        private System.Collections.IEnumerator FlashCoroutine()
        {
            Color originalBase = baseColor;
            baseColor = Color.white;

            yield return new WaitForSeconds(0.1f);

            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                baseColor = Color.Lerp(Color.white, originalBase, t);
                yield return null;
            }

            baseColor = originalBase;
        }
    }
}
