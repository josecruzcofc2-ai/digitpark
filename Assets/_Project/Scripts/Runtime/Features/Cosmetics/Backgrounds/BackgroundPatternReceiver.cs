using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DigitPark.Themes;

namespace DigitPark.Cosmetics
{
    /// <summary>
    /// Componente que se coloca en Canvas/BackgroundPattern (RawImage) en cada escena.
    /// Se registra con BackgroundPatternManager en OnEnable y se desregistra en OnDisable.
    /// Recibe Apply() del Manager cada vez que cambia el patrón o la escena.
    /// NO usa ThemeApplier — el color del tint se calcula internamente según la escena.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class BackgroundPatternReceiver : MonoBehaviour
    {
        private RawImage _rawImage;

        private void Awake()
        {
            _rawImage = GetComponent<RawImage>();
            _rawImage.raycastTarget = false;
        }

        private void OnEnable()
        {
            if (BackgroundPatternManager.Instance != null)
                BackgroundPatternManager.Instance.Register(this);
        }

        private void OnDisable()
        {
            if (BackgroundPatternManager.Instance != null)
                BackgroundPatternManager.Instance.Unregister(this);
        }

        // ==================== Apply ====================

        /// <summary>
        /// Called by BackgroundPatternManager. Applies sprite + tint to the RawImage.
        /// sprite == null means bg_solid (no pattern).
        /// </summary>
        public void Apply(Sprite sprite, float opacity)
        {
            if (sprite == null)
            {
                _rawImage.enabled = false;
                return;
            }

            _rawImage.enabled = true;
            _rawImage.texture = sprite.texture;
            _rawImage.color   = GetPatternTint(opacity);

            // Tiling: repeat the 512px pattern across the full screen
            float tilesX = Screen.width  / 512f;
            float tilesY = Screen.height / 512f;
            _rawImage.uvRect = new Rect(0, 0, tilesX, tilesY);
        }

        // ==================== Tint Logic ====================

        /// <summary>
        /// Priority:
        ///   1. Real money scenes (Cash* + AgeVerification) → gold #FFD700, subtle
        ///   2. Chromatic themes → theme.patternTintColor
        ///   3. Standard → white
        /// </summary>
        private Color GetPatternTint(float opacity)
        {
            // 1. Real money scenes — gold tint, always, regardless of active theme.
            //    These scenes never use ThemeApplier so there's no theme context to read.
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName.StartsWith("Cash") || sceneName == "AgeVerification")
                return new Color(1f, 0.843f, 0f, opacity); // #FFD700 gold tenue

            // 2. Chromatic themes — use the accent color defined per theme
            var theme = ThemeManager.Instance?.CurrentTheme;
            if (theme != null && theme.isChromatic)
                return new Color(
                    theme.patternTintColor.r,
                    theme.patternTintColor.g,
                    theme.patternTintColor.b,
                    opacity);

            // 3. Standard — classic subtle white shimmer
            return new Color(1f, 1f, 1f, opacity);
        }
    }
}
