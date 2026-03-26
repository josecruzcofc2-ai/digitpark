using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using DigitPark.Localization;
using DigitPark.Services.Firebase;
using DigitPark.Games;
using DigitPark.Monetization;
using DigitPark.Navigation;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager para la escena de selección de modo de juego
    /// Opciones: Solo (Practice), 1v1
    /// </summary>
    public class PlayModeSelectionManager : MonoBehaviour
    {
        private bool _isNavigating = false;

        [Header("UI - Header")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button backButton;

        [Header("UI - Mode Cards")]
        [SerializeField] private Button soloCard;
        [SerializeField] private Button oneVsOneCard;
        [Header("UI - Card Texts")]
        [SerializeField] private TextMeshProUGUI soloTitleText;
        [SerializeField] private TextMeshProUGUI soloDescText;
        [SerializeField] private TextMeshProUGUI oneVsOneTitleText;
        [SerializeField] private TextMeshProUGUI oneVsOneDescText;
        [Header("UI - Card Icons (optional)")]
        [SerializeField] private Image soloIcon;
        [SerializeField] private Image oneVsOneIcon;
        private void Start()
        {
            Debug.Log("[PlayModeSelection] Manager iniciado");

            SetupListeners();
            UpdateTexts();

            // Subscribe to language changes
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.OnLanguageChanged += UpdateTexts;
            }
        }

        private void OnDestroy()
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.OnLanguageChanged -= UpdateTexts;
            }

            backButton?.onClick.RemoveAllListeners();
            soloCard?.onClick.RemoveAllListeners();
            oneVsOneCard?.onClick.RemoveAllListeners();
        }

        private void SetupListeners()
        {
            // Disable auto-navigation from BackButton prefab to prevent double listener
            var autoNav = backButton?.GetComponent<DigitPark.UI.BackButton>();
            if (autoNav != null) autoNav.DisableAutoNavigation();
            backButton?.onClick.AddListener(OnBackClicked);
            soloCard?.onClick.AddListener(OnSoloClicked);
            oneVsOneCard?.onClick.AddListener(OnOneVsOneClicked);
        }

        private void UpdateTexts()
        {
            // Title
            if (titleText != null)
                titleText.text = AutoLocalizer.Get("play_mode_title");

            // Solo card
            if (soloTitleText != null)
                soloTitleText.text = AutoLocalizer.Get("solo_title");
            if (soloDescText != null)
                soloDescText.text = AutoLocalizer.Get("solo_description");

            // 1v1 card
            if (oneVsOneTitleText != null)
                oneVsOneTitleText.text = AutoLocalizer.Get("1v1_title");
            if (oneVsOneDescText != null)
                oneVsOneDescText.text = AutoLocalizer.Get("1v1_description");

        }

        #region Navigation Callbacks

        private void OnBackClicked()
        {
            Debug.Log("[PlayModeSelection] Volviendo atrás");
            SceneNavigator.Instance?.GoBack();
        }

        private void OnSoloClicked()
        {
            if (_isNavigating) return;
            _isNavigating = true;

            Debug.Log("[PlayModeSelection] Modo Solo seleccionado");

            AnalyticsService.Instance?.LogCustomEvent("mode_selected", new Dictionary<string, object> { { "mode", "solo" } });

            GameSelectorManager.SetPracticeMode(true);
            SceneManager.LoadScene("GameSelector");
        }

        private void OnOneVsOneClicked()
        {
            if (_isNavigating) return;

            // B3-I: Require network for online modes
            if (DigitPark.Services.NetworkService.Instance != null &&
                !DigitPark.Services.NetworkService.Instance.IsOnline)
            {
                Debug.LogWarning("[PlayModeSelection] Sin conexión — 1v1 no disponible");
                return;
            }

            _isNavigating = true;

            Debug.Log("[PlayModeSelection] Modo 1v1 seleccionado");

            AnalyticsService.Instance?.LogCustomEvent("mode_selected", new Dictionary<string, object> { { "mode", "1v1" } });

            GameSelectorManager.SetPracticeMode(false);
            GameSelectorManager.SetOnlineMatchMode(true);
            SceneManager.LoadScene("GameSelector");
        }

        #endregion
    }
}
