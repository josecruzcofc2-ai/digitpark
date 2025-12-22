using UnityEngine;
using TMPro;
using DigitPark.Themes;
using DigitPark.Managers;
using DigitPark.UI.Panels;
using DigitPark.Tools;

namespace DigitPark.UI.Components
{
    /// <summary>
    /// Controlador runtime para el dropdown de temas
    /// Conecta el TMP_Dropdown con el ThemeManager para cambiar el tema de la app
    /// Maneja temas premium bloqueados hasta que se compre "Estilos PRO"
    /// </summary>
    [RequireComponent(typeof(TMP_Dropdown))]
    public class ThemeDropdownController : MonoBehaviour
    {
        private TMP_Dropdown dropdown;
        private bool isInitialized = false;
        private int lastValidIndex = 0;

        // El índice 0 (Neon Dark) es gratuito, los demás son premium
        private const int FREE_THEME_INDEX = 0;
        private const string LOCK_ICON = " (PRO)";

        private void Awake()
        {
            dropdown = GetComponent<TMP_Dropdown>();
        }

        private void Start()
        {
            Initialize();
        }

        private void OnEnable()
        {
            ThemeManager.OnThemeChanged += OnThemeChangedExternally;
            PremiumManager.OnPremiumStatusChanged += OnPremiumStatusChanged;
        }

        private void OnDisable()
        {
            ThemeManager.OnThemeChanged -= OnThemeChangedExternally;
            PremiumManager.OnPremiumStatusChanged -= OnPremiumStatusChanged;
        }

        /// <summary>
        /// Inicializa el dropdown con los temas disponibles
        /// </summary>
        public void Initialize()
        {
            if (dropdown == null || isInitialized) return;

            // Limpiar opciones existentes
            dropdown.ClearOptions();

            // Verificar que ThemeManager exista
            if (ThemeManager.Instance == null)
            {
                Debug.LogWarning("[ThemeDropdownController] ThemeManager no disponible");
                return;
            }

            // Agregar los temas disponibles
            var themes = ThemeManager.Instance.AvailableThemes;
            var options = new System.Collections.Generic.List<TMP_Dropdown.OptionData>();

            bool hasStylesPro = PremiumManager.Instance?.HasStylesPro ?? false;

            for (int i = 0; i < themes.Count; i++)
            {
                var theme = themes[i];
                string displayName = theme.themeName;

                // Agregar candado a temas premium si no tiene StylesPro
                if (i != FREE_THEME_INDEX && !hasStylesPro)
                {
                    displayName += LOCK_ICON;
                }

                options.Add(new TMP_Dropdown.OptionData(displayName, theme.themeIcon));
            }

            dropdown.AddOptions(options);

            // Suscribirse al evento de cambio
            dropdown.onValueChanged.RemoveListener(OnThemeSelected);
            dropdown.onValueChanged.AddListener(OnThemeSelected);

            // Sincronizar con el tema actual
            SyncWithCurrentTheme();

            isInitialized = true;
            Debug.Log($"[ThemeDropdownController] Inicializado con {options.Count} temas (StylesPro: {hasStylesPro})");
        }

        /// <summary>
        /// Sincroniza el valor del dropdown con el tema actual
        /// </summary>
        private void SyncWithCurrentTheme()
        {
            if (ThemeManager.Instance == null || dropdown == null) return;

            int currentIndex = ThemeManager.Instance.CurrentThemeIndex;
            if (currentIndex >= 0 && currentIndex < dropdown.options.Count)
            {
                dropdown.SetValueWithoutNotify(currentIndex);
                lastValidIndex = currentIndex;
            }
        }

        /// <summary>
        /// Verifica si un tema es premium (no es el tema gratuito)
        /// </summary>
        private bool IsThemePremium(int index)
        {
            return index != FREE_THEME_INDEX;
        }

        /// <summary>
        /// Verifica si el usuario puede usar un tema
        /// </summary>
        private bool CanUseTheme(int index)
        {
            // El tema gratuito siempre está disponible
            if (!IsThemePremium(index)) return true;

            // Si el debug controller permite cambiar temas, siempre permitir
            if (PremiumDebugController.Instance != null &&
                PremiumDebugController.Instance.AllowThemeChange)
            {
                return true;
            }

            // Los temas premium requieren StylesPro
            return PremiumManager.Instance?.HasStylesPro ?? false;
        }

        /// <summary>
        /// Callback cuando el usuario selecciona un tema
        /// </summary>
        private void OnThemeSelected(int index)
        {
            if (ThemeManager.Instance == null) return;

            // Verificar si el tema está bloqueado
            if (!CanUseTheme(index))
            {
                Debug.Log($"[ThemeDropdownController] Tema premium bloqueado. Mostrando panel de compra...");

                // Revertir al último tema válido
                dropdown.SetValueWithoutNotify(lastValidIndex);

                // Mostrar panel de compra de estilos
                ShowStylesProPurchasePanel();
                return;
            }

            // Aplicar el tema
            lastValidIndex = index;
            ThemeManager.Instance.SetTheme(index);
            Debug.Log($"[ThemeDropdownController] Tema seleccionado: {ThemeManager.Instance.CurrentTheme?.themeName}");
        }

        /// <summary>
        /// Muestra el panel para comprar Estilos PRO
        /// </summary>
        private void ShowStylesProPurchasePanel()
        {
            // Buscar el panel premium existente o crear uno
            var premiumPanel = FindObjectOfType<PremiumPanelUI>(true);

            if (premiumPanel != null)
            {
                premiumPanel.Show(
                    onNoAdsCallback: null,
                    onPremiumCallback: null,
                    onRestoreCallback: () => PremiumManager.Instance?.RestorePurchases(),
                    onCloseCallback: () => premiumPanel.Hide()
                );
            }
            else
            {
                // Crear panel simple para comprar StylesPro
                ShowSimpleStylesProDialog();
            }
        }

        /// <summary>
        /// Muestra un diálogo simple para comprar Estilos PRO
        /// </summary>
        private void ShowSimpleStylesProDialog()
        {
            Debug.Log("[ThemeDropdownController] Creando panel de compra de Estilos PRO...");

            // Buscar un Canvas para crear el panel
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[ThemeDropdownController] No se encontró Canvas para mostrar el panel");
                return;
            }

            // Crear el panel de premium dinámicamente
            var panelUI = PremiumPanelUI.CreateAndShow(canvas.transform);

            // Asegurar que el panel esté al frente de todo
            if (panelUI != null)
            {
                panelUI.transform.SetAsLastSibling();

                // Agregar un Canvas adicional para asegurar que renderice encima
                Canvas panelCanvas = panelUI.gameObject.AddComponent<Canvas>();
                panelCanvas.overrideSorting = true;
                panelCanvas.sortingOrder = 100;
                panelUI.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            // Suscribirse al evento de cambio de premium para refrescar
            void OnPremiumChanged()
            {
                Refresh();
                PremiumManager.OnPremiumStatusChanged -= OnPremiumChanged;
            }
            PremiumManager.OnPremiumStatusChanged += OnPremiumChanged;
        }

        /// <summary>
        /// Callback cuando el tema cambia externamente
        /// </summary>
        private void OnThemeChangedExternally(ThemeData theme)
        {
            SyncWithCurrentTheme();
        }

        /// <summary>
        /// Callback cuando cambia el estado premium
        /// </summary>
        private void OnPremiumStatusChanged()
        {
            // Refrescar el dropdown para actualizar los candados
            Refresh();
        }

        /// <summary>
        /// Fuerza la reinicialización del dropdown
        /// </summary>
        public void Refresh()
        {
            isInitialized = false;
            Initialize();
        }
    }
}
