using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DigitPark.Themes;

namespace DigitPark.UI.Components
{
    /// <summary>
    /// Adds a subtle neon glow effect to buttons using Outline component
    /// Integrates with the theme system for dynamic color changes
    /// Replaces the old BottomBar style with a modern neon aesthetic
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Image))]
    public class NeonButtonGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        /// <summary>
        /// Preset glow styles for different button types
        /// </summary>
        public enum GlowStyle
        {
            Primary,        // Cyan neon - main actions
            Secondary,      // Subtle gray - secondary actions
            Premium,        // Gold - monetization/premium
            Success,        // Green - confirmations
            Danger,         // Red - destructive actions
            Purple,         // Purple - special/tournaments
            Navy,           // Navy blue buttons - get cyan glow
            Custom          // Use customColor
        }

        [Header("Glow Configuration")]
        [SerializeField] private GlowStyle glowStyle = GlowStyle.Primary;
        [SerializeField] private Color customColor = new Color(0f, 1f, 1f, 1f);

        [Header("Glow Intensity")]
        [Range(0f, 1f)]
        [SerializeField] private float glowAlpha = 0.7f;
        [SerializeField] private Vector2 glowDistance = new Vector2(4, 4);

        [Header("Hover Effect")]
        [SerializeField] private bool enableHoverEffect = true;
        [Range(1f, 2f)]
        [SerializeField] private float hoverIntensityMultiplier = 1.5f;
        [SerializeField] private float hoverTransitionSpeed = 8f;

        [Header("Animation")]
        [SerializeField] private bool enablePulse = false;
        [Range(0.5f, 3f)]
        [SerializeField] private float pulseSpeed = 1.5f;
        [Range(0.1f, 0.5f)]
        [SerializeField] private float pulseAmount = 0.2f;

        // Components
        private Outline outline;
        private Image targetImage;
        private Button button;

        // State
        private bool isHovered = false;
        private bool isPressed = false;
        private float currentIntensity = 1f;
        private float targetIntensity = 1f;
        private Color currentGlowColor;

        // Glow colors - Designed for CONTRAST and NEON aesthetic
        // Each glow is carefully chosen to complement its button color

        // Primary (Cyan buttons) -> Magenta/Pink glow (complementary neon)
        private static readonly Color GLOW_PRIMARY = new Color(1f, 0.2f, 0.6f, 1f);      // #FF3399 Magenta-Pink

        // Secondary (Gray buttons) -> Soft cyan glow (subtle accent)
        private static readonly Color GLOW_SECONDARY = new Color(0.4f, 0.9f, 1f, 1f);    // #66E5FF Soft Cyan

        // Premium (Gold buttons) -> Warm orange glow (reinforces gold)
        private static readonly Color GLOW_PREMIUM = new Color(1f, 0.6f, 0.2f, 1f);      // #FF9933 Warm Orange

        // Success (Green/Confirm buttons) -> White glow (clean success)
        private static readonly Color GLOW_SUCCESS = new Color(0.8f, 1f, 0.8f, 1f);      // #CCFFCC Light Green-White

        // Danger (Red buttons) -> Orange-red glow (warm warning)
        private static readonly Color GLOW_DANGER = new Color(1f, 0.4f, 0.2f, 1f);       // #FF6633 Orange-Red

        // Purple (Tournament/Special) -> Bright magenta glow
        private static readonly Color GLOW_PURPLE = new Color(1f, 0.3f, 1f, 1f);         // #FF4DFF Bright Magenta

        // Navy (Dark blue buttons) -> Bright cyan glow (maximum contrast)
        private static readonly Color GLOW_NAVY = new Color(0f, 1f, 1f, 1f);             // #00FFFF Bright Cyan

        private void Awake()
        {
            targetImage = GetComponent<Image>();
            button = GetComponent<Button>();
            SetupOutline();
        }

        private void OnEnable()
        {
            // Subscribe to theme changes
            ThemeManager.OnThemeChanged += OnThemeChanged;

            // Apply current glow
            UpdateGlowColor();
            ApplyGlow();
        }

        private void OnDisable()
        {
            ThemeManager.OnThemeChanged -= OnThemeChanged;
        }

        private void Update()
        {
            if (!Application.isPlaying) return;

            // Smooth intensity transition for hover
            if (enableHoverEffect)
            {
                currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * hoverTransitionSpeed);
            }

            // Pulse animation
            if (enablePulse)
            {
                float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) * pulseAmount;
                currentIntensity *= pulse;
            }

            UpdateOutlineColor();
        }

        private void SetupOutline()
        {
            outline = GetComponent<Outline>();
            if (outline == null)
            {
                outline = gameObject.AddComponent<Outline>();
            }

            outline.effectDistance = glowDistance;
            outline.useGraphicAlpha = false;
        }

        private void UpdateGlowColor()
        {
            // Glow colors designed for CONTRAST with button colors
            // Creates the "dual neon" aesthetic
            switch (glowStyle)
            {
                case GlowStyle.Primary:
                    // Cyan buttons -> Magenta/Pink glow (complementary)
                    currentGlowColor = GLOW_PRIMARY;
                    break;
                case GlowStyle.Secondary:
                    // Gray buttons -> Soft cyan glow
                    currentGlowColor = GLOW_SECONDARY;
                    break;
                case GlowStyle.Premium:
                    // Gold buttons -> Warm orange glow
                    currentGlowColor = GLOW_PREMIUM;
                    break;
                case GlowStyle.Success:
                    // Confirm/Play buttons -> Light green-white glow
                    currentGlowColor = GLOW_SUCCESS;
                    break;
                case GlowStyle.Danger:
                    // Delete/Logout buttons -> Orange-red glow
                    currentGlowColor = GLOW_DANGER;
                    break;
                case GlowStyle.Purple:
                    // Tournament/Special buttons -> Bright magenta glow
                    currentGlowColor = GLOW_PURPLE;
                    break;
                case GlowStyle.Navy:
                    // Navy blue buttons -> Bright cyan glow
                    currentGlowColor = GLOW_NAVY;
                    break;
                case GlowStyle.Custom:
                    currentGlowColor = customColor;
                    break;
            }
        }

        private void ApplyGlow()
        {
            if (outline == null) return;

            UpdateGlowColor();
            UpdateOutlineColor();
            outline.effectDistance = glowDistance;
        }

        private void UpdateOutlineColor()
        {
            if (outline == null) return;

            float alpha = glowAlpha * currentIntensity;
            alpha = Mathf.Clamp01(alpha);

            Color glowColor = new Color(
                currentGlowColor.r,
                currentGlowColor.g,
                currentGlowColor.b,
                alpha
            );

            outline.effectColor = glowColor;
        }

        private void OnThemeChanged(ThemeData newTheme)
        {
            UpdateGlowColor();
            ApplyGlow();
        }

        #region Pointer Events

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!enableHoverEffect || !Application.isPlaying) return;
            if (button != null && !button.interactable) return;

            isHovered = true;
            targetIntensity = hoverIntensityMultiplier;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!enableHoverEffect || !Application.isPlaying) return;

            isHovered = false;
            if (!isPressed)
            {
                targetIntensity = 1f;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!Application.isPlaying) return;
            if (button != null && !button.interactable) return;

            isPressed = true;
            targetIntensity = hoverIntensityMultiplier * 1.2f;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!Application.isPlaying) return;

            isPressed = false;
            targetIntensity = isHovered ? hoverIntensityMultiplier : 1f;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Sets the glow style at runtime
        /// </summary>
        public void SetGlowStyle(GlowStyle style)
        {
            glowStyle = style;
            UpdateGlowColor();
            ApplyGlow();
        }

        /// <summary>
        /// Sets a custom glow color
        /// </summary>
        public void SetCustomColor(Color color)
        {
            glowStyle = GlowStyle.Custom;
            customColor = color;
            currentGlowColor = color;
            ApplyGlow();
        }

        /// <summary>
        /// Sets the glow alpha/intensity
        /// </summary>
        public void SetGlowAlpha(float alpha)
        {
            glowAlpha = Mathf.Clamp01(alpha);
            ApplyGlow();
        }

        /// <summary>
        /// Enables or disables pulse animation
        /// </summary>
        public void SetPulseEnabled(bool enabled)
        {
            enablePulse = enabled;
        }

        /// <summary>
        /// Forces a refresh of the glow effect
        /// </summary>
        public void Refresh()
        {
            SetupOutline();
            ApplyGlow();
        }

        #endregion

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (outline == null)
                outline = GetComponent<Outline>();

            if (outline != null)
            {
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if (this != null)
                    {
                        UpdateGlowColor();
                        ApplyGlow();
                    }
                };
            }
        }

        private void Reset()
        {
            // Set sensible defaults - visible neon glow
            glowStyle = GlowStyle.Primary;
            glowAlpha = 0.7f;
            glowDistance = new Vector2(4, 4);
            enableHoverEffect = true;
            hoverIntensityMultiplier = 1.5f;
        }
#endif
    }
}
