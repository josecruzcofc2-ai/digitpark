using UnityEngine;
using UnityEngine.UI;
using DigitPark.Themes;

namespace DigitPark.UI
{
    /// <summary>
    /// Aplica el color del tema activo a los back buttons.
    /// Escucha cambios de tema y actualiza automáticamente.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class BackButtonColorFixer : MonoBehaviour
    {
        private Image image;
        private Color currentColor;
        private int frameCount = 0;

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        private void Start()
        {
            ApplyThemeColor();

            // Suscribirse a cambios de tema
            ThemeManager.OnThemeChanged += OnThemeChanged;
        }

        private void OnDestroy()
        {
            // Desuscribirse
            ThemeManager.OnThemeChanged -= OnThemeChanged;
        }

        private void OnEnable()
        {
            frameCount = 0;
            ApplyThemeColor();
        }

        private void LateUpdate()
        {
            // Aplicar color durante los primeros frames para asegurar prioridad
            if (frameCount < 10)
            {
                ApplyThemeColor();
                frameCount++;
            }
        }

        private void OnThemeChanged(ThemeData newTheme)
        {
            frameCount = 0; // Reset para re-aplicar
            ApplyThemeColor();
        }

        private void ApplyThemeColor()
        {
            if (image == null) return;

            Color targetColor = GetThemeAccentColor();

            if (image.color != targetColor)
            {
                image.color = targetColor;
                currentColor = targetColor;
            }
        }

        private Color GetThemeAccentColor()
        {
            // Usar el color primario del tema activo
            if (ThemeManager.Instance?.CurrentTheme != null)
            {
                return ThemeManager.Instance.CurrentTheme.primaryAccent;
            }

            // Fallback a cyan si no hay tema
            return new Color(0f, 0.9608f, 1f, 1f);
        }
    }
}
