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

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            ThemeManager.OnThemeChanged += OnThemeChanged;
            ApplyThemeColor();
        }

        private void OnDisable()
        {
            ThemeManager.OnThemeChanged -= OnThemeChanged;
        }

        private void OnThemeChanged(ThemeData newTheme)
        {
            ApplyThemeColor();
        }

        private void ApplyThemeColor()
        {
            if (image == null) return;

            Color targetColor = GetThemeAccentColor();
            image.color = targetColor;
        }

        private Color GetThemeAccentColor()
        {
            if (ThemeManager.Instance?.CurrentTheme != null)
            {
                return ThemeManager.Instance.CurrentTheme.primaryAccent;
            }

            // Fallback a cyan si no hay tema
            return new Color(0f, 0.9608f, 1f, 1f);
        }
    }
}
