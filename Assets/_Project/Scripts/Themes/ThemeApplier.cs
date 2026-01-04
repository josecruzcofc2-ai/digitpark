using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace DigitPark.Themes
{
    /// <summary>
    /// Componente que aplica el tema actual a un elemento UI
    /// Añadir a cualquier GameObject con elementos UI para que respondan a cambios de tema
    /// </summary>
    public class ThemeApplier : MonoBehaviour
    {
        /// <summary>
        /// Tipo de elemento UI para aplicar el tema correcto
        /// </summary>
        public enum ElementType
        {
            None,
            // Backgrounds
            PrimaryBackground,
            SecondaryBackground,
            TertiaryBackground,
            CardBackground,
            Overlay,
            // Buttons
            ButtonPrimary,
            ButtonSecondary,
            ButtonDanger,
            ButtonSuccess,
            // Text
            TextPrimary,
            TextSecondary,
            TextDisabled,
            TextTitle,
            TextOnPrimary,
            TextOnDanger,
            TextOnSuccess,
            // Input Fields
            InputBackground,
            InputBorder,
            InputPlaceholder,
            // Special
            Accent,
            AccentSecondary,
            AccentTertiary,
            Premium,
            Glow,
            // Tabs
            TabActive,
            TabInactive,
            // Toggle/Slider
            ToggleBackground,
            ToggleCheckmark,
            SliderTrack,
            SliderFill,
            SliderHandle,
            // Scrollbar
            ScrollbarTrack,
            ScrollbarHandle,
            // Status
            Error,
            Warning,
            Success,
            Info,
            // Leaderboard
            Rank1,
            Rank2,
            Rank3,
            RowEven,
            RowOdd,
            // Scene Specific
            HeaderPurple,
            HeaderNavy,
            BackgroundNavy,
            BackgroundPurple
        }

        [Header("Element Configuration")]
        [SerializeField] private ElementType elementType = ElementType.None;
        [SerializeField] private bool applyToImage = true;
        [SerializeField] private bool applyToText = true;
        [SerializeField] private bool applyToOutline = false;
        [SerializeField] private bool applyToShadow = false;

        [Header("Target Components (Auto-detected if null)")]
        [SerializeField] private Image targetImage;
        [SerializeField] private TextMeshProUGUI targetTMPText;
        [SerializeField] private Text targetLegacyText;
        [SerializeField] private Outline targetOutline;
        [SerializeField] private Shadow targetShadow;
        [SerializeField] private Button targetButton;

        [Header("Animation")]
        [SerializeField] private bool animateTransition = true;
#pragma warning disable 0414 // Se usa colorTransitionDuration del tema en su lugar
        [SerializeField] private float transitionDuration = 0.2f;
#pragma warning restore 0414

        private Coroutine colorTransitionCoroutine;

        private void Awake()
        {
            // Auto-detectar componentes si no están asignados
            DetectComponents();
        }

        private void OnEnable()
        {
            // Suscribirse a cambios de tema
            ThemeManager.OnThemeChanged += OnThemeChanged;

            // Aplicar tema actual
            if (ThemeManager.Instance?.CurrentTheme != null)
            {
                ApplyTheme(ThemeManager.Instance.CurrentTheme, false);
            }
        }

        private void OnDisable()
        {
            ThemeManager.OnThemeChanged -= OnThemeChanged;
        }

        /// <summary>
        /// Detecta automáticamente los componentes UI en este GameObject
        /// </summary>
        private void DetectComponents()
        {
            if (targetImage == null && applyToImage)
                targetImage = GetComponent<Image>();

            if (targetTMPText == null && applyToText)
                targetTMPText = GetComponent<TextMeshProUGUI>();

            if (targetLegacyText == null && applyToText)
                targetLegacyText = GetComponent<Text>();

            if (targetOutline == null && applyToOutline)
                targetOutline = GetComponent<Outline>();

            if (targetShadow == null && applyToShadow)
                targetShadow = GetComponent<Shadow>();

            if (targetButton == null)
                targetButton = GetComponent<Button>();
        }

        /// <summary>
        /// Callback cuando cambia el tema
        /// </summary>
        private void OnThemeChanged(ThemeData newTheme)
        {
            if (newTheme != null)
            {
                ApplyTheme(newTheme, animateTransition);
            }
        }

        /// <summary>
        /// Aplica el tema especificado a este elemento
        /// </summary>
        public void ApplyTheme(ThemeData theme, bool animate = true)
        {
            if (theme == null || elementType == ElementType.None) return;

            Color targetColor = GetColorForElement(theme, elementType);

            if (animate && animateTransition && Application.isPlaying)
            {
                if (colorTransitionCoroutine != null)
                    StopCoroutine(colorTransitionCoroutine);
                colorTransitionCoroutine = StartCoroutine(AnimateColorTransition(targetColor, theme.colorTransitionDuration));
            }
            else
            {
                ApplyColorImmediate(targetColor);
            }

            // Aplicar efectos adicionales
            ApplyEffects(theme);

            // Configurar botón si existe
            if (targetButton != null)
            {
                ConfigureButtonColors(theme);
            }
        }

        /// <summary>
        /// Obtiene el color correspondiente al tipo de elemento
        /// </summary>
        private Color GetColorForElement(ThemeData theme, ElementType type)
        {
            switch (type)
            {
                // Backgrounds
                case ElementType.PrimaryBackground: return theme.primaryBackground;
                case ElementType.SecondaryBackground: return theme.secondaryBackground;
                case ElementType.TertiaryBackground: return theme.tertiaryBackground;
                case ElementType.CardBackground: return theme.cardBackground;
                case ElementType.Overlay: return theme.overlayColor;

                // Buttons
                case ElementType.ButtonPrimary: return theme.buttonPrimary;
                case ElementType.ButtonSecondary: return theme.buttonSecondary;
                case ElementType.ButtonDanger: return theme.buttonDanger;
                case ElementType.ButtonSuccess: return theme.buttonSuccess;

                // Text
                case ElementType.TextPrimary: return theme.textPrimary;
                case ElementType.TextSecondary: return theme.textSecondary;
                case ElementType.TextDisabled: return theme.textDisabled;
                case ElementType.TextTitle: return theme.textTitle;
                case ElementType.TextOnPrimary: return theme.textOnPrimary;
                case ElementType.TextOnDanger: return theme.textOnDanger;
                case ElementType.TextOnSuccess: return theme.textOnSuccess;

                // Input
                case ElementType.InputBackground: return theme.inputBackground;
                case ElementType.InputBorder: return theme.inputBorder;
                case ElementType.InputPlaceholder: return theme.inputPlaceholder;

                // Special
                case ElementType.Accent: return theme.primaryAccent;
                case ElementType.AccentSecondary: return theme.secondaryAccent;
                case ElementType.AccentTertiary: return theme.tertiaryAccent;
                case ElementType.Premium: return theme.premiumColor;
                case ElementType.Glow: return theme.glowColor;

                // Tabs
                case ElementType.TabActive: return theme.tabActive;
                case ElementType.TabInactive: return theme.tabInactive;

                // Toggle/Slider
                case ElementType.ToggleBackground: return theme.toggleOff;
                case ElementType.ToggleCheckmark: return theme.toggleCheckmark;
                case ElementType.SliderTrack: return theme.sliderTrack;
                case ElementType.SliderFill: return theme.sliderFill;
                case ElementType.SliderHandle: return theme.sliderHandle;

                // Scrollbar
                case ElementType.ScrollbarTrack: return theme.scrollbarTrack;
                case ElementType.ScrollbarHandle: return theme.scrollbarHandle;

                // Status
                case ElementType.Error: return theme.errorColor;
                case ElementType.Warning: return theme.warningColor;
                case ElementType.Success: return theme.successColor;
                case ElementType.Info: return theme.infoColor;

                // Leaderboard
                case ElementType.Rank1: return theme.rank1Color;
                case ElementType.Rank2: return theme.rank2Color;
                case ElementType.Rank3: return theme.rank3Color;
                case ElementType.RowEven: return theme.rowEven;
                case ElementType.RowOdd: return theme.rowOdd;

                // Scene Specific
                case ElementType.HeaderPurple: return theme.headerPurple;
                case ElementType.HeaderNavy: return theme.headerNavy;
                case ElementType.BackgroundNavy: return theme.backgroundNavy;
                case ElementType.BackgroundPurple: return theme.backgroundPurple;

                default: return Color.white;
            }
        }

        /// <summary>
        /// Aplica el color inmediatamente sin animación
        /// </summary>
        private void ApplyColorImmediate(Color color)
        {
            if (targetImage != null && applyToImage)
                targetImage.color = color;

            if (targetTMPText != null && applyToText)
                targetTMPText.color = color;

            if (targetLegacyText != null && applyToText)
                targetLegacyText.color = color;

            if (targetOutline != null && applyToOutline)
                targetOutline.effectColor = color;

            if (targetShadow != null && applyToShadow)
                targetShadow.effectColor = color;
        }

        /// <summary>
        /// Anima la transición de color
        /// </summary>
        private IEnumerator AnimateColorTransition(Color targetColor, float duration)
        {
            Color startImageColor = targetImage != null ? targetImage.color : Color.white;
            Color startTextColor = targetTMPText != null ? targetTMPText.color : (targetLegacyText != null ? targetLegacyText.color : Color.white);

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                if (targetImage != null && applyToImage)
                    targetImage.color = Color.Lerp(startImageColor, targetColor, t);

                if (targetTMPText != null && applyToText)
                    targetTMPText.color = Color.Lerp(startTextColor, targetColor, t);

                if (targetLegacyText != null && applyToText)
                    targetLegacyText.color = Color.Lerp(startTextColor, targetColor, t);

                yield return null;
            }

            ApplyColorImmediate(targetColor);
        }

        /// <summary>
        /// Aplica efectos adicionales del tema
        /// </summary>
        private void ApplyEffects(ThemeData theme)
        {
            // Aplicar sombra si está habilitado
            if (applyToShadow && targetShadow != null && theme.useShadows)
            {
                targetShadow.effectColor = theme.shadowColor;
                targetShadow.effectDistance = theme.shadowDistance;
            }

            // Aplicar outline/glow si está habilitado
            if (applyToOutline && targetOutline != null)
            {
                targetOutline.effectColor = new Color(
                    theme.glowColor.r,
                    theme.glowColor.g,
                    theme.glowColor.b,
                    theme.glowIntensity
                );
            }
        }

        /// <summary>
        /// Configura los colores del botón según el tipo de elemento
        /// </summary>
        private void ConfigureButtonColors(ThemeData theme)
        {
            if (targetButton == null) return;

            ColorBlock colors = targetButton.colors;

            switch (elementType)
            {
                case ElementType.ButtonPrimary:
                    colors.normalColor = theme.buttonPrimary;
                    colors.highlightedColor = theme.buttonPrimaryHover;
                    colors.pressedColor = theme.buttonPrimaryPressed;
                    colors.selectedColor = theme.buttonPrimaryHover;
                    colors.disabledColor = theme.textDisabled;
                    break;

                case ElementType.ButtonSecondary:
                    colors.normalColor = theme.buttonSecondary;
                    colors.highlightedColor = theme.buttonSecondaryHover;
                    colors.pressedColor = theme.secondaryBackground;
                    colors.selectedColor = theme.buttonSecondaryHover;
                    colors.disabledColor = theme.textDisabled;
                    break;

                case ElementType.ButtonDanger:
                    colors.normalColor = theme.buttonDanger;
                    colors.highlightedColor = new Color(theme.buttonDanger.r * 1.2f, theme.buttonDanger.g * 1.2f, theme.buttonDanger.b * 1.2f);
                    colors.pressedColor = new Color(theme.buttonDanger.r * 0.8f, theme.buttonDanger.g * 0.8f, theme.buttonDanger.b * 0.8f);
                    break;

                case ElementType.ButtonSuccess:
                    colors.normalColor = theme.buttonSuccess;
                    colors.highlightedColor = new Color(theme.buttonSuccess.r * 1.2f, theme.buttonSuccess.g * 1.2f, theme.buttonSuccess.b * 1.2f);
                    colors.pressedColor = new Color(theme.buttonSuccess.r * 0.8f, theme.buttonSuccess.g * 0.8f, theme.buttonSuccess.b * 0.8f);
                    break;
            }

            colors.colorMultiplier = 1f;
            colors.fadeDuration = theme.colorTransitionDuration;

            targetButton.colors = colors;
        }

        /// <summary>
        /// Cambia el tipo de elemento en runtime
        /// </summary>
        public void SetElementType(ElementType newType)
        {
            elementType = newType;
            if (ThemeManager.Instance?.CurrentTheme != null)
            {
                ApplyTheme(ThemeManager.Instance.CurrentTheme, animateTransition);
            }
        }

        /// <summary>
        /// Fuerza la re-aplicación del tema
        /// </summary>
        public void Refresh()
        {
            if (ThemeManager.Instance?.CurrentTheme != null)
            {
                ApplyTheme(ThemeManager.Instance.CurrentTheme, false);
            }
        }
    }
}
