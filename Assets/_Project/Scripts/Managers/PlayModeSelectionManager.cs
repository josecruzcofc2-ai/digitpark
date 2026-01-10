using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DigitPark.Localization;
using DigitPark.Services.Firebase;
using DigitPark.Games;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager para la escena de selección de modo de juego
    /// Opciones: Solo (Practice), 1v1, Tournaments
    /// </summary>
    public class PlayModeSelectionManager : MonoBehaviour
    {
        [Header("UI - Header")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button backButton;

        [Header("UI - Mode Cards")]
        [SerializeField] private Button soloCard;
        [SerializeField] private Button oneVsOneCard;
        [SerializeField] private Button tournamentsCard;

        [Header("UI - Card Texts")]
        [SerializeField] private TextMeshProUGUI soloTitleText;
        [SerializeField] private TextMeshProUGUI soloDescText;
        [SerializeField] private TextMeshProUGUI oneVsOneTitleText;
        [SerializeField] private TextMeshProUGUI oneVsOneDescText;
        [SerializeField] private TextMeshProUGUI tournamentsTitleText;
        [SerializeField] private TextMeshProUGUI tournamentsDescText;

        [Header("UI - Card Icons (optional)")]
        [SerializeField] private Image soloIcon;
        [SerializeField] private Image oneVsOneIcon;
        [SerializeField] private Image tournamentsIcon;

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
        }

        private void SetupListeners()
        {
            backButton?.onClick.AddListener(OnBackClicked);
            soloCard?.onClick.AddListener(OnSoloClicked);
            oneVsOneCard?.onClick.AddListener(OnOneVsOneClicked);
            tournamentsCard?.onClick.AddListener(OnTournamentsClicked);
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

            // Tournaments card
            if (tournamentsTitleText != null)
                tournamentsTitleText.text = AutoLocalizer.Get("tournaments_title");
            if (tournamentsDescText != null)
                tournamentsDescText.text = AutoLocalizer.Get("tournaments_description");
        }

        #region Navigation Callbacks

        private void OnBackClicked()
        {
            Debug.Log("[PlayModeSelection] Volviendo al MainMenu");
            SceneManager.LoadScene("MainMenu");
        }

        private void OnSoloClicked()
        {
            Debug.Log("[PlayModeSelection] Modo Solo seleccionado");

            // Analytics
            AnalyticsService.Instance?.LogGameStart();

            // Navigate to game selector in practice mode
            GameSelectorManager.SetPracticeMode(true);
            SceneManager.LoadScene("GameSelector");
        }

        private void OnOneVsOneClicked()
        {
            Debug.Log("[PlayModeSelection] Modo 1v1 seleccionado");

            // Navigate to game selector in 1v1 online mode
            // After selecting a game, it will search for an opponent
            GameSelectorManager.SetPracticeMode(false); // Online 1v1 mode
            GameSelectorManager.SetOnlineMatchMode(true);
            SceneManager.LoadScene("GameSelector");
        }

        private void OnTournamentsClicked()
        {
            Debug.Log("[PlayModeSelection] Modo Torneos seleccionado");

            // Navigate to Tournaments (free tournaments)
            SceneManager.LoadScene("Tournaments");
        }

        #endregion
    }
}
