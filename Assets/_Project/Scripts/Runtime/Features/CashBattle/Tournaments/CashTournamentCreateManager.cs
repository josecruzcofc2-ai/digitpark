using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DigitPark.Games;
using DigitPark.Localization;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager for the CashTournamentCreate scene.
    /// Handles form validation, preview updates, prize estimation, and tournament creation.
    /// Gold-themed tournament creation form for the CashBattle ecosystem.
    /// </summary>
    public class CashTournamentCreateManager : MonoBehaviour
    {
        [Header("=== HEADER ===")]
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI titleText;

        [Header("=== TOURNAMENT NAME ===")]
        [SerializeField] private TMP_InputField tournamentNameInput;
        [SerializeField] private TextMeshProUGUI nameCharCountText;

        [Header("=== GAME SELECTION ===")]
        [SerializeField] private TMP_Dropdown gameTypeDropdown;
        [SerializeField] private Image selectedGameIcon;

        [Header("=== ENTRY FEE ===")]
        [SerializeField] private TMP_Dropdown entryFeeDropdown;
        [SerializeField] private Slider entryFeeSlider;
        [SerializeField] private TMP_InputField customEntryFeeInput;
        [SerializeField] private TextMeshProUGUI entryFeeDisplayText;

        [Header("=== PLAYERS ===")]
        [SerializeField] private TMP_Dropdown maxPlayersDropdown;
        [SerializeField] private TextMeshProUGUI estimatedPrizeText;

        [Header("=== SCHEDULE ===")]
        [SerializeField] private TMP_Dropdown startTimeDropdown;
        [SerializeField] private Toggle startImmediatelyToggle;
        [SerializeField] private TextMeshProUGUI scheduledTimeText;

        [Header("=== RULES ===")]
        [SerializeField] private TMP_Dropdown roundsDropdown;
        [SerializeField] private TMP_Dropdown timeLimitDropdown;
        [SerializeField] private TMP_Dropdown maxAttemptsDropdown;
        [SerializeField] private Toggle allowSpectatorsToggle;
        [SerializeField] private Toggle privateToggle;
        [SerializeField] private TMP_InputField privateCodeInput;

        [Header("=== PREVIEW ===")]
        [SerializeField] private GameObject previewPanel;
        [SerializeField] private TextMeshProUGUI previewNameText;
        [SerializeField] private TextMeshProUGUI previewGameText;
        [SerializeField] private TextMeshProUGUI previewEntryText;
        [SerializeField] private TextMeshProUGUI previewPrizeText;
        [SerializeField] private TextMeshProUGUI previewPlayersText;

        [Header("=== ACTIONS ===")]
        [SerializeField] private Button createButton;
        [SerializeField] private TextMeshProUGUI createButtonText;
        [SerializeField] private TextMeshProUGUI creationFeeText;

        [Header("=== STATUS ===")]
        [SerializeField] private GameObject loadingOverlay;
        [SerializeField] private TextMeshProUGUI statusText;

        // Internal state
        private float currentEntryFee = 5f;
        private int currentMaxPlayers = 8;
        private bool isCreating = false;

        // Entry fee presets matching dropdown order
        private static readonly float[] ENTRY_FEE_PRESETS = { 1f, 5f, 10f, 25f, 50f, 100f };

        // Player count options matching dropdown order
        private static readonly int[] PLAYER_COUNT_OPTIONS = { 4, 8, 16, 32 };

        // Platform fee percentage
        private const float PLATFORM_FEE_PERCENT = 0.30f;

        private void Start()
        {
            Debug.Log("[CashTournamentCreate] Manager started");

            SetupListeners();
            PopulateDropdowns();
            UpdatePreview();
            UpdateEstimatedPrize();
            UpdateLocalizedTexts();

            if (loadingOverlay != null)
                loadingOverlay.SetActive(false);

            LocalizationManager.OnLanguageChanged += UpdateLocalizedTexts;
        }

        private void OnDestroy()
        {
            LocalizationManager.OnLanguageChanged -= UpdateLocalizedTexts;
        }

        #region Setup

        private void SetupListeners()
        {
            // Back button - disable auto-nav from BackButtonGold prefab, handle manually
            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(OnBackClicked);
            }

            // Create button
            if (createButton != null)
            {
                createButton.onClick.RemoveAllListeners();
                createButton.onClick.AddListener(OnCreateClicked);
            }

            // Dropdown listeners
            if (gameTypeDropdown != null)
                gameTypeDropdown.onValueChanged.AddListener(OnGameTypeChanged);

            if (entryFeeDropdown != null)
                entryFeeDropdown.onValueChanged.AddListener(OnEntryFeeChanged);

            if (maxPlayersDropdown != null)
                maxPlayersDropdown.onValueChanged.AddListener(OnMaxPlayersChanged);

            if (startTimeDropdown != null)
                startTimeDropdown.onValueChanged.AddListener(delegate { UpdatePreview(); });

            if (roundsDropdown != null)
                roundsDropdown.onValueChanged.AddListener(delegate { UpdatePreview(); });

            if (timeLimitDropdown != null)
                timeLimitDropdown.onValueChanged.AddListener(delegate { UpdatePreview(); });

            if (maxAttemptsDropdown != null)
                maxAttemptsDropdown.onValueChanged.AddListener(delegate { UpdatePreview(); });

            // Toggle listeners
            if (startImmediatelyToggle != null)
                startImmediatelyToggle.onValueChanged.AddListener(OnStartImmediatelyChanged);

            if (allowSpectatorsToggle != null)
                allowSpectatorsToggle.onValueChanged.AddListener(delegate { UpdatePreview(); });

            if (privateToggle != null)
                privateToggle.onValueChanged.AddListener(OnPrivateToggleChanged);

            // Slider listener
            if (entryFeeSlider != null)
            {
                entryFeeSlider.onValueChanged.AddListener(OnEntryFeeSliderChanged);
                entryFeeSlider.minValue = 1;
                entryFeeSlider.maxValue = 100;
                entryFeeSlider.value = currentEntryFee;
            }

            // Input field listeners
            if (tournamentNameInput != null)
                tournamentNameInput.onValueChanged.AddListener(OnNameChanged);

            if (customEntryFeeInput != null)
                customEntryFeeInput.onEndEdit.AddListener(OnCustomEntryFeeEndEdit);
        }

        private void PopulateDropdowns()
        {
            // Game types
            if (gameTypeDropdown != null)
            {
                gameTypeDropdown.ClearOptions();
                gameTypeDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    AutoLocalizer.Get("select_game"),
                    AutoLocalizer.Get("gameinfo_digitrush_name"),
                    AutoLocalizer.Get("gameinfo_memorypairs_name"),
                    AutoLocalizer.Get("gameinfo_quickmath_name"),
                    AutoLocalizer.Get("gameinfo_flashtap_name"),
                    AutoLocalizer.Get("gameinfo_oddoneout_name")
                });
            }

            // Entry fees
            if (entryFeeDropdown != null)
            {
                entryFeeDropdown.ClearOptions();
                var feeOptions = new System.Collections.Generic.List<string>();
                foreach (float fee in ENTRY_FEE_PRESETS)
                    feeOptions.Add($"${fee:F2}");
                entryFeeDropdown.AddOptions(feeOptions);
                entryFeeDropdown.value = 1; // Default $5
            }

            // Player counts
            if (maxPlayersDropdown != null)
            {
                maxPlayersDropdown.ClearOptions();
                var playerOptions = new System.Collections.Generic.List<string>();
                foreach (int count in PLAYER_COUNT_OPTIONS)
                    playerOptions.Add(count.ToString());
                maxPlayersDropdown.AddOptions(playerOptions);
                maxPlayersDropdown.value = 1; // Default 8
            }

            // Rounds
            if (roundsDropdown != null)
            {
                roundsDropdown.ClearOptions();
                roundsDropdown.AddOptions(new System.Collections.Generic.List<string>
                    { "1", "2", "3", "4", "5" });
                roundsDropdown.value = 0;
            }

            // Time limits
            if (timeLimitDropdown != null)
            {
                timeLimitDropdown.ClearOptions();
                timeLimitDropdown.AddOptions(new System.Collections.Generic.List<string>
                    { "30s", "60s", "90s", "120s", "180s" });
                timeLimitDropdown.value = 1; // Default 60s
            }

            // Max attempts
            if (maxAttemptsDropdown != null)
            {
                maxAttemptsDropdown.ClearOptions();
                maxAttemptsDropdown.AddOptions(new System.Collections.Generic.List<string>
                    { "1", "2", "3", "5", AutoLocalizer.Get("tournament_unlimited") });
                maxAttemptsDropdown.value = 0;
            }

            // Start time
            if (startTimeDropdown != null)
            {
                startTimeDropdown.ClearOptions();
                startTimeDropdown.AddOptions(new System.Collections.Generic.List<string>
                    {
                        AutoLocalizer.Get("schedule_now"),
                        AutoLocalizer.Get("schedule_30min"),
                        AutoLocalizer.Get("schedule_1hour"),
                        AutoLocalizer.Get("schedule_2hours"),
                        AutoLocalizer.Get("schedule_6hours"),
                        AutoLocalizer.Get("schedule_24hours")
                    });
                startTimeDropdown.value = 0;
            }
        }

        #endregion

        #region Event Handlers

        private void OnGameTypeChanged(int index)
        {
            // Update selected game icon
            if (selectedGameIcon != null && index > 0)
            {
                string[] gameIds = { "", "DigitRush", "MemoryPairs", "QuickMath", "FlashTap", "OddOneOut" };
                if (index < gameIds.Length)
                {
                    string iconPath = $"Icons/Games/{gameIds[index]}Icon";
                    Sprite sprite = Resources.Load<Sprite>(iconPath);
                    if (sprite != null)
                    {
                        selectedGameIcon.sprite = sprite;
                        selectedGameIcon.color = Color.white;
                    }
                }
            }

            UpdatePreview();
        }

        private void OnEntryFeeChanged(int index)
        {
            if (index >= 0 && index < ENTRY_FEE_PRESETS.Length)
            {
                currentEntryFee = ENTRY_FEE_PRESETS[index];

                if (entryFeeSlider != null)
                    entryFeeSlider.value = currentEntryFee;

                UpdateEntryFeeDisplay();
                UpdateEstimatedPrize();
                UpdatePreview();
            }
        }

        private void OnEntryFeeSliderChanged(float value)
        {
            currentEntryFee = Mathf.Round(value);
            UpdateEntryFeeDisplay();
            UpdateEstimatedPrize();
            UpdatePreview();
        }

        private void OnCustomEntryFeeEndEdit(string text)
        {
            if (float.TryParse(text, out float value))
            {
                value = Mathf.Clamp(value, 1f, 100f);
                currentEntryFee = value;

                if (entryFeeSlider != null)
                    entryFeeSlider.value = value;

                UpdateEntryFeeDisplay();
                UpdateEstimatedPrize();
                UpdatePreview();
            }
        }

        private void OnMaxPlayersChanged(int index)
        {
            if (index >= 0 && index < PLAYER_COUNT_OPTIONS.Length)
            {
                currentMaxPlayers = PLAYER_COUNT_OPTIONS[index];
                UpdateEstimatedPrize();
                UpdatePreview();
            }
        }

        private void OnNameChanged(string name)
        {
            if (nameCharCountText != null)
                nameCharCountText.text = $"{name.Length}/40";

            UpdatePreview();
        }

        private void OnStartImmediatelyChanged(bool isImmediate)
        {
            if (startTimeDropdown != null)
                startTimeDropdown.interactable = !isImmediate;

            if (scheduledTimeText != null)
                scheduledTimeText.text = isImmediate ? AutoLocalizer.Get("tournament_starts_now") : AutoLocalizer.Get("tournament_starts_scheduled");

            UpdatePreview();
        }

        private void OnPrivateToggleChanged(bool isPrivate)
        {
            if (privateCodeInput != null)
                privateCodeInput.gameObject.SetActive(isPrivate);

            UpdatePreview();
        }

        #endregion

        #region Update Methods

        private void UpdateEntryFeeDisplay()
        {
            if (entryFeeDisplayText != null)
                entryFeeDisplayText.text = $"${currentEntryFee:F2}";
        }

        private void UpdateEstimatedPrize()
        {
            // Prize = entry fee * players * (1 - platform fee)
            float estimatedPrize = currentEntryFee * currentMaxPlayers * (1f - PLATFORM_FEE_PERCENT);

            if (estimatedPrizeText != null)
                estimatedPrizeText.text = AutoLocalizer.Get("tournament_estimated_prize", estimatedPrize);
        }

        private void UpdatePreview()
        {
            if (previewPanel == null) return;

            // Name
            if (previewNameText != null)
            {
                string name = tournamentNameInput != null ? tournamentNameInput.text : "--";
                previewNameText.text = $"{AutoLocalizer.Get("tournament_preview_name")}: {(string.IsNullOrEmpty(name) ? "--" : name)}";
            }

            // Game
            if (previewGameText != null && gameTypeDropdown != null)
            {
                string gameName = gameTypeDropdown.value > 0 ? gameTypeDropdown.captionText.text : "--";
                previewGameText.text = $"{AutoLocalizer.Get("tournament_preview_game")}: {gameName}";
            }

            // Entry Fee
            if (previewEntryText != null)
                previewEntryText.text = AutoLocalizer.Get("cash_entry_fee", currentEntryFee);

            // Prize
            if (previewPrizeText != null)
            {
                float prize = currentEntryFee * currentMaxPlayers * (1f - PLATFORM_FEE_PERCENT);
                previewPrizeText.text = AutoLocalizer.Get("tournament_estimated_prize", prize);
            }

            // Players
            if (previewPlayersText != null)
                previewPlayersText.text = AutoLocalizer.Get("tournament_max_players", currentMaxPlayers);
        }

        private void UpdateLocalizedTexts()
        {
            if (titleText != null)
                titleText.text = AutoLocalizer.Get("cashTournamentCreate_title", "Create Tournament");

            if (createButtonText != null)
                createButtonText.text = AutoLocalizer.Get("cashTournamentCreate_createButton", "Create Tournament");

            if (creationFeeText != null)
                creationFeeText.text = AutoLocalizer.Get("cashTournamentCreate_creationFee", "Creation fee: $5.00");
        }

        #endregion

        #region Actions

        private void OnCreateClicked()
        {
            if (isCreating) return;

            if (!ValidateForm())
                return;

            isCreating = true;

            if (loadingOverlay != null)
                loadingOverlay.SetActive(true);

            if (statusText != null)
                statusText.text = AutoLocalizer.Get("tournament_creating");

            if (createButton != null)
                createButton.interactable = false;

            // Simulate creation delay then navigate
            Invoke(nameof(OnCreateComplete), 2f);
        }

        private void OnCreateComplete()
        {
            isCreating = false;

            if (loadingOverlay != null)
                loadingOverlay.SetActive(false);

            if (createButton != null)
                createButton.interactable = true;

            Debug.Log("[CashTournamentCreate] Tournament created successfully!");

            // Navigate to tournament lobby
            SceneManager.LoadScene("CashTournamentLobby");
        }

        private bool ValidateForm()
        {
            // Check tournament name
            if (tournamentNameInput == null || string.IsNullOrWhiteSpace(tournamentNameInput.text))
            {
                ShowValidationError(AutoLocalizer.Get("validation_tournament_name_required"));
                return false;
            }

            if (tournamentNameInput.text.Length < 3)
            {
                ShowValidationError(AutoLocalizer.Get("validation_tournament_name_min"));
                return false;
            }

            // Check game selected
            if (gameTypeDropdown == null || gameTypeDropdown.value == 0)
            {
                ShowValidationError(AutoLocalizer.Get("validation_select_game"));
                return false;
            }

            // Check entry fee
            if (currentEntryFee <= 0)
            {
                ShowValidationError(AutoLocalizer.Get("validation_entry_fee_positive"));
                return false;
            }

            return true;
        }

        private void ShowValidationError(string message)
        {
            Debug.LogWarning($"[CashTournamentCreate] Validation: {message}");

            if (statusText != null)
                statusText.text = message;

            if (loadingOverlay != null)
            {
                loadingOverlay.SetActive(true);
                Invoke(nameof(HideValidationError), 2f);
            }
        }

        private void HideValidationError()
        {
            if (loadingOverlay != null)
                loadingOverlay.SetActive(false);
        }

        private void OnBackClicked()
        {
            Debug.Log("[CashTournamentCreate] Back clicked, navigating to CashTournaments");
            SceneManager.LoadScene("CashTournaments");
        }

        #endregion
    }
}
