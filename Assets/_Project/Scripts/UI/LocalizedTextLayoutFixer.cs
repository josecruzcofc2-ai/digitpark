using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Localization;

namespace DigitPark.UI
{
    /// <summary>
    /// Sistema global que corrige el layout de textos después de cada cambio de idioma.
    /// Se encarga de mantener los textos en su posición correcta independientemente del idioma.
    /// </summary>
    public class LocalizedTextLayoutFixer : MonoBehaviour
    {
        public static LocalizedTextLayoutFixer Instance { get; private set; }

        [Header("Configuración")]
        [SerializeField] private bool autoFixOnLanguageChange = true;
        [SerializeField] private float delayAfterLanguageChange = 0.1f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                if (transform.parent != null)
                {
                    transform.SetParent(null);
                }
                DontDestroyOnLoad(gameObject);
                Debug.Log("[LayoutFixer] LocalizedTextLayoutFixer inicializado");
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Ejecutar fix inicial con delay para asegurar que todo esté cargado
            StartCoroutine(InitialFix());
        }

        private System.Collections.IEnumerator InitialFix()
        {
            yield return new WaitForSeconds(0.2f);
            Debug.Log("[LayoutFixer] Ejecutando fix inicial...");
            FixAllLayouts();
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            // Ejecutar fix cuando se carga una nueva escena
            StartCoroutine(SceneLoadedFix());
        }

        private System.Collections.IEnumerator SceneLoadedFix()
        {
            yield return new WaitForSeconds(0.3f);
            FixAllLayouts();
        }

        private void OnEnable()
        {
            LocalizationManager.OnLanguageChanged += OnLanguageChanged;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnLanguageChanged()
        {
            Debug.Log("[LayoutFixer] Cambio de idioma detectado");
            if (autoFixOnLanguageChange)
            {
                // Esperar un frame para que los textos se actualicen primero
                StartCoroutine(FixLayoutDelayed());
            }
        }

        private System.Collections.IEnumerator FixLayoutDelayed()
        {
            yield return new WaitForSeconds(delayAfterLanguageChange);
            FixAllLayouts();
        }

        /// <summary>
        /// Fuerza la corrección del layout de todos los textos en la escena actual
        /// </summary>
        public void FixAllLayouts()
        {
            // Corregir todos los Toggles (como Remember Me)
            FixToggleLayouts();

            // Corregir textos con ContentSizeFitter
            FixContentSizeFitters();

            // Forzar rebuild de todos los layouts
            ForceLayoutRebuild();

            Debug.Log("[LayoutFixer] Layout de textos corregido");
        }

        /// <summary>
        /// Corrige el layout de toggles tipo checkbox (con Background child).
        /// NO modifica toggles tipo switch (VibrationToggle, NotificationsToggle, etc.)
        /// donde el texto está centrado intencionalmente dentro del switch.
        /// </summary>
        private void FixToggleLayouts()
        {
            var toggles = FindObjectsOfType<Toggle>(true);

            foreach (var toggle in toggles)
            {
                // Solo procesar toggles tipo checkbox (que tienen un child "Background")
                // Los toggles tipo switch (ON/OFF centrado) NO deben modificarse
                Transform checkmarkBg = toggle.transform.Find("Background");
                if (checkmarkBg == null) continue;

                var toggleText = toggle.GetComponentInChildren<TextMeshProUGUI>();
                if (toggleText == null) continue;

                // Alinear texto a la izquierda (junto al checkbox)
                toggleText.alignment = TextAlignmentOptions.Left;

                RectTransform textRect = toggleText.GetComponent<RectTransform>();
                if (textRect != null)
                {
                    RectTransform bgRect = checkmarkBg.GetComponent<RectTransform>();
                    float checkboxWidth = bgRect != null ? bgRect.sizeDelta.x + 10f : 30f;

                    textRect.anchorMin = new Vector2(0f, 0f);
                    textRect.anchorMax = new Vector2(1f, 1f);
                    textRect.offsetMin = new Vector2(checkboxWidth, 0f);
                    textRect.offsetMax = new Vector2(0f, 0f);
                }
            }
        }

        /// <summary>
        /// Corrige elementos con ContentSizeFitter
        /// </summary>
        private void FixContentSizeFitters()
        {
            var fitters = FindObjectsOfType<ContentSizeFitter>(true);
            foreach (var fitter in fitters)
            {
                if (fitter.enabled)
                {
                    // Forzar actualización del ContentSizeFitter
                    fitter.SetLayoutHorizontal();
                    fitter.SetLayoutVertical();
                }
            }
        }

        /// <summary>
        /// Fuerza el rebuild de todos los LayoutGroups
        /// </summary>
        private void ForceLayoutRebuild()
        {
            // HorizontalLayoutGroup
            var hLayouts = FindObjectsOfType<HorizontalLayoutGroup>(true);
            foreach (var layout in hLayouts)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
            }

            // VerticalLayoutGroup
            var vLayouts = FindObjectsOfType<VerticalLayoutGroup>(true);
            foreach (var layout in vLayouts)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
            }

            // GridLayoutGroup
            var gLayouts = FindObjectsOfType<GridLayoutGroup>(true);
            foreach (var layout in gLayouts)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
            }
        }

        /// <summary>
        /// Método público para forzar la corrección manualmente
        /// </summary>
        public static void ForceFixLayouts()
        {
            if (Instance != null)
            {
                Instance.FixAllLayouts();
            }
        }
    }
}
