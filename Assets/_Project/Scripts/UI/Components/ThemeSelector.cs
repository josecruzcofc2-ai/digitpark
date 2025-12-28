using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DigitPark.Themes;
using DigitPark.Localization;

namespace DigitPark.UI.Components
{
    /// <summary>
    /// Componente UI para seleccionar temas
    /// Muestra todos los temas disponibles con preview
    /// </summary>
    public class ThemeSelector : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform themesContainer;
        [SerializeField] private GameObject themeItemPrefab;
        [SerializeField] private TextMeshProUGUI currentThemeText;
        [SerializeField] private TextMeshProUGUI titleText;

        [Header("Preview Panel")]
        [SerializeField] private Image previewBackground;
        [SerializeField] private Image previewCard;
        [SerializeField] private Image previewButton;
        [SerializeField] private TextMeshProUGUI previewTitle;
        [SerializeField] private TextMeshProUGUI previewText;

        [Header("Navigation")]
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button applyButton;

        [Header("Style")]
        [SerializeField] private Color selectedColor = new Color(0, 1, 1, 1);
        [SerializeField] private Color unselectedColor = new Color(0.3f, 0.3f, 0.3f, 1);

        private List<ThemeItemUI> themeItems = new List<ThemeItemUI>();
        private int selectedIndex = 0;
        private ThemeData previewTheme;

        private void Start()
        {
            Initialize();
        }

        private void OnEnable()
        {
            ThemeManager.OnThemeChanged += OnThemeChanged;
            LocalizationManager.OnLanguageChanged += UpdateLocalizedTexts;
            UpdateLocalizedTexts();
        }

        private void OnDisable()
        {
            ThemeManager.OnThemeChanged -= OnThemeChanged;
            LocalizationManager.OnLanguageChanged -= UpdateLocalizedTexts;
        }

        /// <summary>
        /// Inicializa el selector de temas
        /// </summary>
        public void Initialize()
        {
            if (ThemeManager.Instance == null)
            {
                Debug.LogWarning("[ThemeSelector] ThemeManager no disponible");
                return;
            }

            // Configurar botones de navegación (limpiar antes para evitar duplicados)
            if (prevButton != null)
            {
                prevButton.onClick.RemoveListener(SelectPrevious);
                prevButton.onClick.AddListener(SelectPrevious);
            }

            if (nextButton != null)
            {
                nextButton.onClick.RemoveListener(SelectNext);
                nextButton.onClick.AddListener(SelectNext);
            }

            if (applyButton != null)
            {
                applyButton.onClick.RemoveListener(ApplySelectedTheme);
                applyButton.onClick.AddListener(ApplySelectedTheme);
            }

            // Generar items si hay contenedor
            if (themesContainer != null)
            {
                GenerateThemeItems();
            }

            // Seleccionar tema actual
            selectedIndex = ThemeManager.Instance.CurrentThemeIndex;
            if (selectedIndex < 0) selectedIndex = 0;

            UpdateSelection();
            UpdatePreview();
            UpdateCurrentThemeText();
        }

        /// <summary>
        /// Genera los items de tema en el contenedor
        /// </summary>
        private void GenerateThemeItems()
        {
            // Limpiar items existentes
            foreach (Transform child in themesContainer)
            {
                Destroy(child.gameObject);
            }
            themeItems.Clear();

            var themes = ThemeManager.Instance.AvailableThemes;

            for (int i = 0; i < themes.Count; i++)
            {
                ThemeData theme = themes[i];
                GameObject item;

                if (themeItemPrefab != null)
                {
                    item = Instantiate(themeItemPrefab, themesContainer);
                }
                else
                {
                    item = CreateDefaultThemeItem(themesContainer);
                }

                ThemeItemUI itemUI = item.GetComponent<ThemeItemUI>();
                if (itemUI == null)
                {
                    itemUI = item.AddComponent<ThemeItemUI>();
                }

                int index = i; // Capturar índice para closure
                itemUI.Setup(theme, () => SelectTheme(index));
                themeItems.Add(itemUI);
            }
        }

        /// <summary>
        /// Crea un item de tema por defecto si no hay prefab
        /// </summary>
        private GameObject CreateDefaultThemeItem(Transform parent)
        {
            GameObject item = new GameObject("ThemeItem");
            item.transform.SetParent(parent, false);

            // RectTransform
            RectTransform rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(120, 80);

            // Layout Element
            LayoutElement le = item.AddComponent<LayoutElement>();
            le.preferredWidth = 120;
            le.preferredHeight = 80;

            // Background
            Image bg = item.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.25f, 1f);

            // Button
            Button btn = item.AddComponent<Button>();
            btn.targetGraphic = bg;

            // Color preview (pequeño círculo con los colores del tema)
            GameObject colorPreview = new GameObject("ColorPreview");
            colorPreview.transform.SetParent(item.transform, false);

            RectTransform cpRt = colorPreview.AddComponent<RectTransform>();
            cpRt.anchorMin = new Vector2(0.5f, 0.7f);
            cpRt.anchorMax = new Vector2(0.5f, 0.7f);
            cpRt.sizeDelta = new Vector2(40, 20);

            Image cpImg = colorPreview.AddComponent<Image>();
            cpImg.color = Color.cyan;

            // Nombre del tema
            GameObject nameObj = new GameObject("ThemeName");
            nameObj.transform.SetParent(item.transform, false);

            RectTransform nameRt = nameObj.AddComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0, 0);
            nameRt.anchorMax = new Vector2(1, 0.5f);
            nameRt.offsetMin = new Vector2(5, 5);
            nameRt.offsetMax = new Vector2(-5, 0);

            TextMeshProUGUI nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
            nameTmp.text = "Theme";
            nameTmp.fontSize = 12;
            nameTmp.alignment = TextAlignmentOptions.Center;
            nameTmp.color = Color.white;

            return item;
        }

        /// <summary>
        /// Selecciona un tema por índice
        /// </summary>
        public void SelectTheme(int index)
        {
            if (index < 0 || index >= ThemeManager.Instance.AvailableThemes.Count)
                return;

            selectedIndex = index;
            previewTheme = ThemeManager.Instance.AvailableThemes[index];

            UpdateSelection();
            UpdatePreview();
        }

        /// <summary>
        /// Selecciona el tema anterior
        /// </summary>
        public void SelectPrevious()
        {
            int newIndex = selectedIndex - 1;
            if (newIndex < 0)
                newIndex = ThemeManager.Instance.AvailableThemes.Count - 1;

            SelectTheme(newIndex);
        }

        /// <summary>
        /// Selecciona el siguiente tema
        /// </summary>
        public void SelectNext()
        {
            int newIndex = (selectedIndex + 1) % ThemeManager.Instance.AvailableThemes.Count;
            SelectTheme(newIndex);
        }

        /// <summary>
        /// Aplica el tema seleccionado
        /// </summary>
        public void ApplySelectedTheme()
        {
            if (previewTheme != null)
            {
                ThemeManager.Instance.SetTheme(previewTheme);
                Debug.Log($"[ThemeSelector] Tema aplicado: {previewTheme.themeName}");
            }
        }

        /// <summary>
        /// Actualiza la visualización de selección
        /// </summary>
        private void UpdateSelection()
        {
            for (int i = 0; i < themeItems.Count; i++)
            {
                themeItems[i].SetSelected(i == selectedIndex);
            }
        }

        /// <summary>
        /// Actualiza el preview del tema seleccionado
        /// </summary>
        private void UpdatePreview()
        {
            if (previewTheme == null) return;

            if (previewBackground != null)
                previewBackground.color = previewTheme.primaryBackground;

            if (previewCard != null)
                previewCard.color = previewTheme.cardBackground;

            if (previewButton != null)
                previewButton.color = previewTheme.buttonPrimary;

            if (previewTitle != null)
            {
                previewTitle.color = previewTheme.textTitle;
                previewTitle.text = previewTheme.themeName;
            }

            if (previewText != null)
                previewText.color = previewTheme.textPrimary;

            // Actualizar texto del tema actual
            if (currentThemeText != null)
            {
                currentThemeText.text = previewTheme.themeName;
                currentThemeText.color = previewTheme.primaryAccent;
            }
        }

        /// <summary>
        /// Actualiza el texto del tema actual
        /// </summary>
        private void UpdateCurrentThemeText()
        {
            if (currentThemeText != null && ThemeManager.Instance?.CurrentTheme != null)
            {
                currentThemeText.text = ThemeManager.Instance.CurrentTheme.themeName;
            }
        }

        /// <summary>
        /// Callback cuando cambia el tema
        /// </summary>
        private void OnThemeChanged(ThemeData newTheme)
        {
            UpdateCurrentThemeText();

            // Actualizar selección al tema actual
            int index = ThemeManager.Instance.AvailableThemes.IndexOf(newTheme);
            if (index >= 0)
            {
                selectedIndex = index;
                previewTheme = newTheme;
                UpdateSelection();
            }
        }

        /// <summary>
        /// Actualiza los textos localizados
        /// </summary>
        private void UpdateLocalizedTexts()
        {
            if (titleText != null)
            {
                titleText.text = AutoLocalizer.Get("theme_selector_title");
            }

            if (applyButton != null)
            {
                var btnText = applyButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.text = AutoLocalizer.Get("apply");
                }
            }
        }
    }

    /// <summary>
    /// Componente para cada item individual de tema
    /// </summary>
    public class ThemeItemUI : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private Image colorPreview;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private GameObject selectedIndicator;
        [SerializeField] private Image premiumBadge;

        private ThemeData theme;
        private Button button;
        private System.Action onClick;

        /// <summary>
        /// Configura el item con los datos del tema
        /// </summary>
        public void Setup(ThemeData themeData, System.Action onClickAction)
        {
            theme = themeData;
            onClick = onClickAction;

            // Auto-detectar componentes
            if (background == null)
                background = GetComponent<Image>();

            if (colorPreview == null)
                colorPreview = transform.Find("ColorPreview")?.GetComponent<Image>();

            if (nameText == null)
                nameText = GetComponentInChildren<TextMeshProUGUI>();

            button = GetComponent<Button>();

            // Configurar visuales
            if (colorPreview != null)
                colorPreview.color = theme.primaryAccent;

            if (nameText != null)
            {
                nameText.text = theme.themeName;
                nameText.color = theme.textPrimary;
            }

            if (background != null)
                background.color = theme.secondaryBackground;

            if (premiumBadge != null)
                premiumBadge.gameObject.SetActive(theme.isPremium);

            // Configurar click
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onClick?.Invoke());
            }
        }

        /// <summary>
        /// Establece si este item está seleccionado
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (selectedIndicator != null)
                selectedIndicator.SetActive(selected);

            if (background != null)
            {
                // Destacar el seleccionado
                Color targetColor = selected ? theme.primaryAccent : theme.secondaryBackground;
                background.color = selected ?
                    new Color(theme.primaryAccent.r, theme.primaryAccent.g, theme.primaryAccent.b, 0.3f) :
                    theme.secondaryBackground;
            }

            // Añadir outline al seleccionado
            Outline outline = GetComponent<Outline>();
            if (outline == null && selected)
            {
                outline = gameObject.AddComponent<Outline>();
            }

            if (outline != null)
            {
                outline.enabled = selected;
                outline.effectColor = theme.primaryAccent;
                outline.effectDistance = new Vector2(2, 2);
            }
        }
    }
}
