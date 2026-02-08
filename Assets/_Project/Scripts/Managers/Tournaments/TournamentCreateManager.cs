using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using DigitPark.Monetization;
using DigitPark.Localization;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager para la escena de creación de torneos.
    /// Permite a usuarios premium crear sus propios torneos.
    /// </summary>
    public class TournamentCreateManager : MonoBehaviour
    {
        [Header("UI - Header")]
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI titleText;

        [Header("UI - Tournament Name")]
        [SerializeField] private TMP_InputField tournamentNameInput;
        [SerializeField] private TextMeshProUGUI nameCharCountText;
        [SerializeField] private int maxNameLength = 50;

        [Header("UI - Game Selection")]
        [SerializeField] private TMP_Dropdown gameTypeDropdown;
        [SerializeField] private Image selectedGameIcon;

        [Header("UI - Entry Fee")]
        [SerializeField] private TMP_Dropdown entryFeeDropdown;
        [SerializeField] private Slider entryFeeSlider;
        [SerializeField] private TMP_InputField customEntryFeeInput;
        [SerializeField] private TextMeshProUGUI entryFeeDisplayText;

        [Header("UI - Players")]
        [SerializeField] private TMP_Dropdown maxPlayersDropdown;
        [SerializeField] private TextMeshProUGUI estimatedPrizeText;

        [Header("UI - Schedule")]
        [SerializeField] private TMP_Dropdown startTimeDropdown;
        [SerializeField] private Toggle startImmediatelyToggle;
        [SerializeField] private TextMeshProUGUI scheduledTimeText;

        [Header("UI - Rules")]
        [SerializeField] private TMP_Dropdown roundsDropdown;
        [SerializeField] private TMP_Dropdown timeLimitDropdown;
        [SerializeField] private Toggle allowSpectatorsToggle;
        [SerializeField] private Toggle privateToggle;
        [SerializeField] private TMP_InputField privateCodeInput;

        [Header("UI - Preview")]
        [SerializeField] private GameObject previewPanel;
        [SerializeField] private TextMeshProUGUI previewNameText;
        [SerializeField] private TextMeshProUGUI previewGameText;
        [SerializeField] private TextMeshProUGUI previewEntryText;
        [SerializeField] private TextMeshProUGUI previewPrizeText;
        [SerializeField] private TextMeshProUGUI previewPlayersText;

        [Header("UI - Actions")]
        [SerializeField] private Button createButton;
        [SerializeField] private Button previewButton;
        [SerializeField] private TextMeshProUGUI createButtonText;
        [SerializeField] private TextMeshProUGUI creationFeeText;

        [Header("UI - Status")]
        [SerializeField] private GameObject loadingOverlay;
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("Configuration")]
        [SerializeField] private decimal creationFee = 5m;
        [SerializeField] private decimal platformFeePercent = 10m;

        // State
        private TournamentConfig currentConfig = new TournamentConfig();
        private bool isCreating = false;

        // Entry fee options
        private readonly decimal[] entryFeeOptions = { 0, 1, 2, 5, 10, 25, 50, 100 };
        private readonly int[] maxPlayersOptions = { 8, 16, 32, 64, 128 };

        private void Start()
        {
            SetupUI();
            SetupListeners();
            SetupDropdowns();
            UpdatePreview();
        }

        private void SetupUI()
        {
            if (loadingOverlay) loadingOverlay.SetActive(false);
            if (previewPanel) previewPanel.SetActive(false);
            if (privateCodeInput) privateCodeInput.gameObject.SetActive(false);

            if (creationFeeText)
            {
                creationFeeText.text = $"Costo de creación: ${creationFee:F2}";
            }

            UpdateCreateButtonState();
        }

        private void SetupListeners()
        {
            if (backButton) backButton.onClick.AddListener(OnBackClicked);
            if (createButton) createButton.onClick.AddListener(OnCreateClicked);
            if (previewButton) previewButton.onClick.AddListener(TogglePreview);

            if (tournamentNameInput)
            {
                tournamentNameInput.onValueChanged.AddListener(OnNameChanged);
                tournamentNameInput.characterLimit = maxNameLength;
            }

            if (gameTypeDropdown) gameTypeDropdown.onValueChanged.AddListener(OnGameTypeChanged);
            if (entryFeeDropdown) entryFeeDropdown.onValueChanged.AddListener(OnEntryFeeChanged);
            if (entryFeeSlider) entryFeeSlider.onValueChanged.AddListener(OnEntryFeeSliderChanged);
            if (maxPlayersDropdown) maxPlayersDropdown.onValueChanged.AddListener(OnMaxPlayersChanged);
            if (startImmediatelyToggle) startImmediatelyToggle.onValueChanged.AddListener(OnStartImmediatelyChanged);
            if (privateToggle) privateToggle.onValueChanged.AddListener(OnPrivateChanged);
        }

        private void SetupDropdowns()
        {
            // Game types
            if (gameTypeDropdown)
            {
                gameTypeDropdown.ClearOptions();
                gameTypeDropdown.AddOptions(new List<string> {
                    "Memory Pairs",
                    "Quick Math",
                    "Flash Tap",
                    "Odd One Out",
                    "Digit Rush",
                    "Cognitive Sprint"
                });
            }

            // Entry fees
            if (entryFeeDropdown)
            {
                entryFeeDropdown.ClearOptions();
                var options = new List<string>();
                foreach (var fee in entryFeeOptions)
                {
                    options.Add(fee == 0 ? "Gratis" : $"${fee:F2}");
                }
                entryFeeDropdown.AddOptions(options);
            }

            // Max players
            if (maxPlayersDropdown)
            {
                maxPlayersDropdown.ClearOptions();
                var options = new List<string>();
                foreach (var count in maxPlayersOptions)
                {
                    options.Add($"{count} jugadores");
                }
                maxPlayersDropdown.AddOptions(options);
            }

            // Start time
            if (startTimeDropdown)
            {
                startTimeDropdown.ClearOptions();
                startTimeDropdown.AddOptions(new List<string> {
                    "En 15 minutos",
                    "En 30 minutos",
                    "En 1 hora",
                    "En 2 horas",
                    "En 6 horas",
                    "Mañana"
                });
            }

            // Rounds
            if (roundsDropdown)
            {
                roundsDropdown.ClearOptions();
                roundsDropdown.AddOptions(new List<string> {
                    "1 ronda",
                    "3 rondas (mejor de 3)",
                    "5 rondas (mejor de 5)"
                });
            }

            // Time limit
            if (timeLimitDropdown)
            {
                timeLimitDropdown.ClearOptions();
                timeLimitDropdown.AddOptions(new List<string> {
                    "30 segundos",
                    "1 minuto",
                    "2 minutos",
                    "5 minutos",
                    "Sin límite"
                });
            }
        }

        private void OnNameChanged(string name)
        {
            currentConfig.name = name;

            if (nameCharCountText)
            {
                nameCharCountText.text = $"{name.Length}/{maxNameLength}";
            }

            UpdateCreateButtonState();
            UpdatePreview();
        }

        private void OnGameTypeChanged(int index)
        {
            currentConfig.gameType = gameTypeDropdown.options[index].text;
            UpdatePreview();
        }

        private void OnEntryFeeChanged(int index)
        {
            if (index < entryFeeOptions.Length)
            {
                currentConfig.entryFee = entryFeeOptions[index];
                if (entryFeeSlider) entryFeeSlider.value = index;
            }
            UpdateEntryFeeDisplay();
            UpdateEstimatedPrize();
            UpdatePreview();
        }

        private void OnEntryFeeSliderChanged(float value)
        {
            int index = Mathf.RoundToInt(value);
            if (entryFeeDropdown && entryFeeDropdown.value != index)
            {
                entryFeeDropdown.value = index;
            }
        }

        private void OnMaxPlayersChanged(int index)
        {
            if (index < maxPlayersOptions.Length)
            {
                currentConfig.maxPlayers = maxPlayersOptions[index];
            }
            UpdateEstimatedPrize();
            UpdatePreview();
        }

        private void OnStartImmediatelyChanged(bool immediate)
        {
            currentConfig.startImmediately = immediate;
            if (startTimeDropdown)
            {
                startTimeDropdown.interactable = !immediate;
            }
            UpdatePreview();
        }

        private void OnPrivateChanged(bool isPrivate)
        {
            currentConfig.isPrivate = isPrivate;
            if (privateCodeInput)
            {
                privateCodeInput.gameObject.SetActive(isPrivate);
                if (isPrivate && string.IsNullOrEmpty(privateCodeInput.text))
                {
                    privateCodeInput.text = GeneratePrivateCode();
                }
            }
            UpdatePreview();
        }

        private string GeneratePrivateCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] code = new char[6];
            for (int i = 0; i < code.Length; i++)
            {
                code[i] = chars[UnityEngine.Random.Range(0, chars.Length)];
            }
            return new string(code);
        }

        private void UpdateEntryFeeDisplay()
        {
            if (entryFeeDisplayText)
            {
                entryFeeDisplayText.text = currentConfig.entryFee == 0
                    ? "GRATIS"
                    : $"${currentConfig.entryFee:F2}";
            }
        }

        private void UpdateEstimatedPrize()
        {
            decimal totalPool = currentConfig.entryFee * currentConfig.maxPlayers;
            decimal platformFee = totalPool * (platformFeePercent / 100m);
            decimal prizePool = totalPool - platformFee;

            currentConfig.estimatedPrize = prizePool;

            if (estimatedPrizeText)
            {
                estimatedPrizeText.text = $"Premio estimado: ${prizePool:F2}";
            }
        }

        private void UpdateCreateButtonState()
        {
            if (createButton == null) return;

            bool canCreate = !isCreating &&
                            !string.IsNullOrWhiteSpace(currentConfig.name) &&
                            currentConfig.name.Length >= 3;

            createButton.interactable = canCreate;
        }

        private void TogglePreview()
        {
            if (previewPanel)
            {
                previewPanel.SetActive(!previewPanel.activeSelf);
                if (previewPanel.activeSelf)
                {
                    UpdatePreview();
                }
            }
        }

        private void UpdatePreview()
        {
            if (previewNameText) previewNameText.text = currentConfig.name ?? "Sin nombre";
            if (previewGameText) previewGameText.text = currentConfig.gameType;
            if (previewEntryText) previewEntryText.text = currentConfig.entryFee == 0 ? "Gratis" : $"${currentConfig.entryFee:F2}";
            if (previewPrizeText) previewPrizeText.text = $"${currentConfig.estimatedPrize:F2}";
            if (previewPlayersText) previewPlayersText.text = $"{currentConfig.maxPlayers} jugadores";
        }

        private void OnCreateClicked()
        {
            if (isCreating) return;

            // Validate
            if (string.IsNullOrWhiteSpace(currentConfig.name))
            {
                ShowStatus("Por favor ingresa un nombre para el torneo", true);
                return;
            }

            StartCreation();
        }

        private void StartCreation()
        {
            isCreating = true;
            if (loadingOverlay) loadingOverlay.SetActive(true);
            ShowStatus("Creando torneo...", false);
            UpdateCreateButtonState();

            // Simulate API call
            Invoke(nameof(ProcessCreation), 2f);
        }

        private void ProcessCreation()
        {
            // Simulate success
            bool success = UnityEngine.Random.value > 0.1f;

            if (success)
            {
                string tournamentId = Guid.NewGuid().ToString();
                ShowStatus("¡Torneo creado exitosamente!", false);

                // Navigate to tournament lobby
                Invoke(nameof(NavigateToCreatedTournament), 1.5f);
            }
            else
            {
                ShowStatus(AutoLocalizer.Get("tournament_create_error"), true);
                isCreating = false;
                if (loadingOverlay) loadingOverlay.SetActive(false);
                UpdateCreateButtonState();
            }
        }

        private void NavigateToCreatedTournament()
        {
            SceneNavigator.Instance?.NavigateTo(SceneNavigator.Scenes.TOURNAMENT_LOBBY,
                new SceneNavigator.NavigationParams
                {
                    CustomData = currentConfig
                });
        }

        private void ShowStatus(string message, bool isError)
        {
            if (statusText)
            {
                statusText.text = message;
                statusText.color = isError ? new Color(1f, 0.4f, 0.4f) : new Color(0f, 1f, 0.5f);
            }
        }

        private void OnBackClicked()
        {
            SceneNavigator.Instance?.GoBack();
        }
    }

    [Serializable]
    public class TournamentConfig
    {
        public string name = "";
        public string gameType = "Memory Pairs";
        public decimal entryFee = 5m;
        public int maxPlayers = 32;
        public decimal estimatedPrize = 0m;
        public bool startImmediately = false;
        public bool isPrivate = false;
        public string privateCode = "";
        public int rounds = 1;
        public int timeLimitSeconds = 60;
        public bool allowSpectators = true;
    }
}
