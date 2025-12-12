using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace DigitPark.UI.Theme
{
    /// <summary>
    /// Manager que aplica el tema Neon Gamer a todos los elementos UI de la escena
    /// </summary>
    public class NeonThemeManager : MonoBehaviour
    {
        [Header("Tema")]
        [SerializeField] private NeonThemeColors theme;

        [Header("Configuración")]
        [SerializeField] private bool applyOnStart = true;
        [SerializeField] private bool applyToChildren = true;

        [Header("Referencias Opcionales")]
        [SerializeField] private Image mainBackground;
        [SerializeField] private List<Image> cardBackgrounds = new List<Image>();
        [SerializeField] private List<Button> primaryButtons = new List<Button>();
        [SerializeField] private List<Button> secondaryButtons = new List<Button>();
        [SerializeField] private List<TMP_InputField> inputFields = new List<TMP_InputField>();

        private static NeonThemeManager _instance;
        public static NeonThemeManager Instance => _instance;
        public static NeonThemeColors Theme => _instance?.theme;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }

            // Crear tema por defecto si no está asignado
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<NeonThemeColors>();
                Debug.LogWarning("[NeonTheme] Tema no asignado, usando valores por defecto");
            }
        }

        private void Start()
        {
            if (applyOnStart)
            {
                ApplyTheme();
            }
        }

        /// <summary>
        /// Aplica el tema a todos los elementos de la escena
        /// </summary>
        [ContextMenu("Apply Theme")]
        public void ApplyTheme()
        {
            if (theme == null) return;

            Debug.Log("[NeonTheme] Aplicando tema Neon Gamer...");

            // Aplicar a elementos específicos
            ApplyToMainBackground();
            ApplyToCardBackgrounds();
            ApplyToPrimaryButtons();
            ApplyToSecondaryButtons();
            ApplyToInputFields();

            // Buscar y aplicar a todos los elementos si está habilitado
            if (applyToChildren)
            {
                ApplyToAllImages();
                ApplyToAllTexts();
                ApplyToAllButtons();
                ApplyToAllSliders();
                ApplyToAllToggles();
            }

            Debug.Log("[NeonTheme] Tema aplicado correctamente");
        }

        private void ApplyToMainBackground()
        {
            if (mainBackground != null)
            {
                mainBackground.color = theme.bgDeepSpace;
            }
        }

        private void ApplyToCardBackgrounds()
        {
            foreach (var card in cardBackgrounds)
            {
                if (card != null)
                {
                    card.color = theme.bgCard;
                }
            }
        }

        private void ApplyToPrimaryButtons()
        {
            foreach (var btn in primaryButtons)
            {
                if (btn != null)
                {
                    ApplyPrimaryButtonStyle(btn);
                }
            }
        }

        private void ApplyToSecondaryButtons()
        {
            foreach (var btn in secondaryButtons)
            {
                if (btn != null)
                {
                    ApplySecondaryButtonStyle(btn);
                }
            }
        }

        private void ApplyToInputFields()
        {
            foreach (var input in inputFields)
            {
                if (input != null)
                {
                    ApplyInputFieldStyle(input);
                }
            }
        }

        /// <summary>
        /// Aplica estilo de botón principal (gradiente cyan-purple con texto oscuro)
        /// </summary>
        public void ApplyPrimaryButtonStyle(Button button)
        {
            if (button == null || theme == null) return;

            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = theme.neonCyan;
            }

            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.9f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = theme.textDisabled;
            button.colors = colors;

            // Texto del botón oscuro para contraste
            var text = button.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.color = theme.bgDeepSpace;
                text.fontStyle = FontStyles.Bold;
            }
        }

        /// <summary>
        /// Aplica estilo de botón secundario (fondo card con borde cyan)
        /// </summary>
        public void ApplySecondaryButtonStyle(Button button)
        {
            if (button == null || theme == null) return;

            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = theme.bgCard;
            }

            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = theme.textDisabled;
            button.colors = colors;

            // Texto del botón claro
            var text = button.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.color = theme.textSecondary;
            }
        }

        /// <summary>
        /// Aplica estilo a un input field
        /// </summary>
        public void ApplyInputFieldStyle(TMP_InputField inputField)
        {
            if (inputField == null || theme == null) return;

            // Fondo del input
            var image = inputField.GetComponent<Image>();
            if (image != null)
            {
                image.color = theme.bgCard;
            }

            // Texto del input
            if (inputField.textComponent != null)
            {
                inputField.textComponent.color = theme.textPrimary;
            }

            // Placeholder
            if (inputField.placeholder != null)
            {
                var placeholderText = inputField.placeholder as TextMeshProUGUI;
                if (placeholderText != null)
                {
                    placeholderText.color = theme.textDisabled;
                }
            }

            // Color del caret
            inputField.caretColor = theme.neonCyan;
            inputField.selectionColor = new Color(theme.neonCyan.r, theme.neonCyan.g, theme.neonCyan.b, 0.3f);
        }

        /// <summary>
        /// Busca y aplica colores a todas las imágenes por nombre
        /// </summary>
        private void ApplyToAllImages()
        {
            var images = FindObjectsOfType<Image>(true);
            foreach (var img in images)
            {
                string name = img.gameObject.name.ToLower();

                // Fondos principales
                if (name.Contains("background") || name.Contains("bg"))
                {
                    if (name.Contains("card") || name.Contains("panel") || name.Contains("popup"))
                    {
                        img.color = theme.bgCard;
                    }
                    else if (name.Contains("main") || name.Contains("screen"))
                    {
                        img.color = theme.bgDeepSpace;
                    }
                }
                // Blocker panels
                else if (name.Contains("blocker"))
                {
                    img.color = new Color(theme.bgDeepSpace.r, theme.bgDeepSpace.g, theme.bgDeepSpace.b, 0.85f);
                }
            }
        }

        /// <summary>
        /// Busca y aplica colores a todos los textos por nombre
        /// </summary>
        private void ApplyToAllTexts()
        {
            var texts = FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (var text in texts)
            {
                string name = text.gameObject.name.ToLower();

                // Títulos - Neon Cyan
                if (name.Contains("title") || name.Contains("header"))
                {
                    text.color = theme.neonCyan;
                }
                // Placeholders - Disabled
                else if (name.Contains("placeholder"))
                {
                    text.color = theme.textDisabled;
                }
                // Labels secundarios
                else if (name.Contains("label") || name.Contains("subtitle"))
                {
                    text.color = theme.textSecondary;
                }
                // Error texts
                else if (name.Contains("error"))
                {
                    text.color = theme.error;
                }
                // Success texts
                else if (name.Contains("success"))
                {
                    text.color = theme.neonGreen;
                }
            }
        }

        /// <summary>
        /// Busca y aplica colores a todos los botones por nombre
        /// </summary>
        private void ApplyToAllButtons()
        {
            var buttons = FindObjectsOfType<Button>(true);
            foreach (var btn in buttons)
            {
                string name = btn.gameObject.name.ToLower();

                // Botones de acción principal
                if (name.Contains("login") || name.Contains("play") || name.Contains("confirm") ||
                    name.Contains("create") || name.Contains("join") || name.Contains("submit") ||
                    name.Contains("register") || name.Contains("signin"))
                {
                    ApplyPrimaryButtonStyle(btn);
                }
                // Botones con iconos (preservar color del icono)
                else if (name.Contains("back"))
                {
                    ApplyIconButtonStyle(btn);
                }
                // Botones secundarios / navegación
                else if (name.Contains("cancel") || name.Contains("close") ||
                         name.Contains("settings") || name.Contains("scores") || name.Contains("tournament"))
                {
                    ApplySecondaryButtonStyle(btn);
                }
                // Botones de peligro
                else if (name.Contains("delete") || name.Contains("logout") || name.Contains("exit"))
                {
                    ApplyDangerButtonStyle(btn);
                }
            }
        }

        /// <summary>
        /// Aplica estilo para botones con iconos (back, settings, etc.)
        /// Preserva el color blanco de la imagen para que el icono se vea correctamente
        /// </summary>
        public void ApplyIconButtonStyle(Button button)
        {
            if (button == null || theme == null) return;

            // NO cambiar el color de la imagen para preservar el icono original
            var image = button.GetComponent<Image>();
            if (image != null)
            {
                // Mantener el color blanco para que el icono se vea correctamente
                image.color = Color.white;
            }

            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = theme.textDisabled;
            button.colors = colors;
        }

        /// <summary>
        /// Aplica estilo de botón de peligro (rojo/rosa)
        /// </summary>
        public void ApplyDangerButtonStyle(Button button)
        {
            if (button == null || theme == null) return;

            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = theme.bgCard;
            }

            var text = button.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.color = theme.error;
            }
        }

        /// <summary>
        /// Aplica tema a sliders
        /// </summary>
        private void ApplyToAllSliders()
        {
            var sliders = FindObjectsOfType<Slider>(true);
            foreach (var slider in sliders)
            {
                // Background del slider
                var background = slider.transform.Find("Background")?.GetComponent<Image>();
                if (background != null)
                {
                    background.color = theme.borderNormal;
                }

                // Fill del slider
                var fill = slider.fillRect?.GetComponent<Image>();
                if (fill != null)
                {
                    fill.color = theme.neonCyan;
                }

                // Handle del slider
                var handle = slider.handleRect?.GetComponent<Image>();
                if (handle != null)
                {
                    handle.color = theme.neonCyan;
                }
            }
        }

        /// <summary>
        /// Aplica tema a toggles
        /// </summary>
        private void ApplyToAllToggles()
        {
            var toggles = FindObjectsOfType<Toggle>(true);
            foreach (var toggle in toggles)
            {
                // Background del toggle
                var background = toggle.targetGraphic as Image;
                if (background != null)
                {
                    background.color = theme.bgCard;
                }

                // Checkmark
                var checkmark = toggle.graphic as Image;
                if (checkmark != null)
                {
                    checkmark.color = theme.neonCyan;
                }

                // Texto del toggle
                var text = toggle.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.color = theme.textPrimary;
                }
            }
        }

        /// <summary>
        /// Aplica estilo especial para el botón PLAY (verde brillante)
        /// </summary>
        public void ApplyPlayButtonStyle(Button button)
        {
            if (button == null || theme == null) return;

            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = theme.neonGreen;
            }

            var text = button.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.color = theme.bgDeepSpace;
                text.fontStyle = FontStyles.Bold;
            }
        }

        /// <summary>
        /// Aplica estilo para tarjetas de torneo (borde rosa-amarillo)
        /// </summary>
        public void ApplyTournamentCardStyle(Image cardBackground, TextMeshProUGUI titleText, TextMeshProUGUI prizeText)
        {
            if (theme == null) return;

            if (cardBackground != null)
            {
                cardBackground.color = theme.bgCard;
            }

            if (titleText != null)
            {
                titleText.color = theme.textPrimary;
            }

            if (prizeText != null)
            {
                prizeText.color = theme.neonYellow;
            }
        }

        /// <summary>
        /// Obtiene el color según la posición en el ranking
        /// </summary>
        public Color GetRankingColor(int position)
        {
            if (theme == null) return Color.white;

            switch (position)
            {
                case 1: return theme.gold;
                case 2: return theme.silver;
                case 3: return theme.bronze;
                default: return theme.borderNormal;
            }
        }

        /// <summary>
        /// Aplica colores a un tile del juego
        /// </summary>
        public void ApplyGameTileStyle(Image tileBackground, TextMeshProUGUI numberText, bool isIdle = true)
        {
            if (theme == null) return;

            if (tileBackground != null)
            {
                tileBackground.color = theme.bgCard;
            }

            if (numberText != null)
            {
                numberText.color = isIdle ? theme.neonCyan : theme.bgDeepSpace;
            }
        }

        /// <summary>
        /// Aplica feedback de tile correcto
        /// </summary>
        public void ApplyTileCorrectFeedback(Image tileBackground, TextMeshProUGUI numberText)
        {
            if (theme == null) return;

            if (tileBackground != null)
            {
                tileBackground.color = theme.neonGreen;
            }

            if (numberText != null)
            {
                numberText.color = theme.bgDeepSpace;
            }
        }

        /// <summary>
        /// Aplica feedback de tile error
        /// </summary>
        public void ApplyTileErrorFeedback(Image tileBackground, TextMeshProUGUI numberText)
        {
            if (theme == null) return;

            if (tileBackground != null)
            {
                tileBackground.color = theme.error;
            }

            if (numberText != null)
            {
                numberText.color = theme.textPrimary;
            }
        }
    }
}
