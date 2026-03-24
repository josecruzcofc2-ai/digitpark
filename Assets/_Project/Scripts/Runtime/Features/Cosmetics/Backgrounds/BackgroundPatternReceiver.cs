using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

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
        private bool _hasAppliedOnce;

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
                if (_hasAppliedOnce && _rawImage.enabled)
                {
                    // Fade out before disabling
                    _rawImage.DOColor(new Color(1f, 1f, 1f, 0f), 0.3f)
                        .SetUpdate(true).SetLink(gameObject)
                        .OnComplete(() => _rawImage.enabled = false);
                }
                else
                {
                    _rawImage.enabled = false;
                }
                return;
            }

            Color targetTint = GetPatternTint(opacity);

            // Tiling: repeat the 512px pattern across the full screen
            float tilesX = Screen.width  / 512f;
            float tilesY = Screen.height / 512f;

            if (_hasAppliedOnce && _rawImage.enabled)
            {
                // Crossfade: fade out → swap → fade in
                _rawImage.DOColor(new Color(targetTint.r, targetTint.g, targetTint.b, 0f), 0.2f)
                    .SetUpdate(true).SetLink(gameObject)
                    .OnComplete(() =>
                    {
                        _rawImage.texture = sprite.texture;
                        _rawImage.uvRect = new Rect(0, 0, tilesX, tilesY);
                        _rawImage.DOColor(targetTint, 0.25f).SetUpdate(true).SetLink(gameObject);
                    });
            }
            else
            {
                // First apply — instant
                _rawImage.enabled = true;
                _rawImage.texture = sprite.texture;
                _rawImage.color   = targetTint;
                _rawImage.uvRect  = new Rect(0, 0, tilesX, tilesY);
            }

            _hasAppliedOnce = true;
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
            // Standard — classic subtle white shimmer
            return new Color(1f, 1f, 1f, opacity);
        }
    }
}
