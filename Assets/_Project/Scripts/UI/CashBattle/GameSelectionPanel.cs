using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Games;

namespace DigitPark.UI.CashBattle
{
    /// <summary>
    /// Panel for selecting game type in Cash Battle 1v1
    /// Shows individual games and Cognitive Sprint option
    /// Supports custom entry fees up to $250 (Triumph withdrawal limit)
    /// </summary>
    public class GameSelectionPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button backButton;
        [SerializeField] private Transform gamesContainer;
        [SerializeField] private GameObject gameCardPrefab;

        [Header("Entry Fee Selection")]
        [SerializeField] private Transform entryFeeContainer;
        [SerializeField] private Button[] entryFeeButtons;
        [SerializeField] private TextMeshProUGUI selectedFeeText;

        [Header("Custom Entry Fee")]
        [SerializeField] private TMP_InputField customAmountInput;
        [SerializeField] private TextMeshProUGUI earningsText;
        [SerializeField] private TextMeshProUGUI minMaxText;

        [Header("Online Players")]
        [SerializeField] private TextMeshProUGUI onlinePlayersText;
        [SerializeField] private Image onlineIndicator;

        [Header("Action Buttons")]
        [SerializeField] private Button findOpponentButton;
        [SerializeField] private TextMeshProUGUI findOpponentText;

        [Header("Cognitive Sprint")]
        [SerializeField] private Button cognitiveSprintButton;
        [SerializeField] private GameObject cognitiveSprintPanel;
        [SerializeField] private TextMeshProUGUI sprintSelectionText;

        // Events
        public event Action OnBackClicked;
        public event Action<GameType, decimal> OnGameSelected;
        public event Action<List<GameType>, decimal> OnCognitiveSprintSelected;

        // Constants - Triumph SDK limits
        private const decimal MIN_ENTRY_FEE = 1m;
        private const decimal MAX_ENTRY_FEE = 250m;
        private const float WINNER_PERCENTAGE = 0.70f; // 70% to winner (Triumph takes 20%, platform 10%)

        // State
        private GameType? selectedGame;
        private decimal selectedEntryFee = 1m;
        private List<GameType> selectedSprintGames = new List<GameType>();
        private bool isCognitiveSprintMode = false;
        private int currentOnlinePlayers = 0;
        private int selectedFeeButtonIndex = 0;

        // Available entry fees (preset buttons)
        private readonly decimal[] availableFees = { 1m, 5m, 10m, 25m, 50m, 100m };

        private void Start()
        {
            SetupListeners();
            CreateGameCards();
            CreateEntryFeeButtons();
            SetupCustomAmountInput();
            InitializeOnlinePlayersCounter();
            UpdateUI();
            UpdateEarningsFeedback();
            UpdateMinMaxText();
        }

        private void SetupListeners()
        {
            backButton?.onClick.AddListener(() => OnBackClicked?.Invoke());
            findOpponentButton?.onClick.AddListener(OnFindOpponentClicked);
            cognitiveSprintButton?.onClick.AddListener(ToggleCognitiveSprintMode);
        }

        private void CreateGameCards()
        {
            if (gamesContainer == null || gameCardPrefab == null) return;

            // Clear existing
            foreach (Transform child in gamesContainer)
            {
                Destroy(child.gameObject);
            }

            // Create cards for each game
            var gameInfos = CognitiveSprintManager.GetAllGameInfos();
            foreach (var info in gameInfos)
            {
                CreateGameCard(info);
            }
        }

        private void CreateGameCard(GameInfo info)
        {
            GameObject card = Instantiate(gameCardPrefab, gamesContainer);

            // Setup card visuals
            var nameText = card.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            var descText = card.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
            var skillText = card.transform.Find("Skill")?.GetComponent<TextMeshProUGUI>();
            var button = card.GetComponent<Button>();
            var checkmark = card.transform.Find("Checkmark")?.gameObject;

            if (nameText != null) nameText.text = info.Name;
            if (descText != null) descText.text = info.Description;
            if (skillText != null) skillText.text = info.Skill;
            if (checkmark != null) checkmark.SetActive(false);

            // Store game type reference
            var gameCard = card.AddComponent<GameCardReference>();
            gameCard.GameType = info.Type;
            gameCard.Checkmark = checkmark;

            // Click handler
            button?.onClick.AddListener(() => OnGameCardClicked(info.Type, card));
        }

        private void OnGameCardClicked(GameType gameType, GameObject card)
        {
            if (isCognitiveSprintMode)
            {
                // Toggle selection for sprint
                ToggleSprintGameSelection(gameType, card);
            }
            else
            {
                // Single game selection
                SelectSingleGame(gameType, card);
            }
        }

        private void SelectSingleGame(GameType gameType, GameObject selectedCard)
        {
            selectedGame = gameType;

            // Update all cards visual state
            foreach (Transform child in gamesContainer)
            {
                var cardRef = child.GetComponent<GameCardReference>();
                if (cardRef != null && cardRef.Checkmark != null)
                {
                    cardRef.Checkmark.SetActive(child.gameObject == selectedCard);
                }
            }

            UpdateUI();
        }

        private void ToggleSprintGameSelection(GameType gameType, GameObject card)
        {
            var cardRef = card.GetComponent<GameCardReference>();

            if (selectedSprintGames.Contains(gameType))
            {
                selectedSprintGames.Remove(gameType);
                if (cardRef?.Checkmark != null) cardRef.Checkmark.SetActive(false);
            }
            else
            {
                if (selectedSprintGames.Count < CognitiveSprintManager.MAX_GAMES)
                {
                    selectedSprintGames.Add(gameType);
                    if (cardRef?.Checkmark != null) cardRef.Checkmark.SetActive(true);
                }
            }

            UpdateSprintSelectionText();
            UpdateUI();
        }

        private void ToggleCognitiveSprintMode()
        {
            isCognitiveSprintMode = !isCognitiveSprintMode;

            // Clear selections when switching modes
            selectedGame = null;
            selectedSprintGames.Clear();

            // Reset all checkmarks
            foreach (Transform child in gamesContainer)
            {
                var cardRef = child.GetComponent<GameCardReference>();
                if (cardRef?.Checkmark != null) cardRef.Checkmark.SetActive(false);
            }

            UpdateSprintSelectionText();
            UpdateUI();
        }

        private void UpdateSprintSelectionText()
        {
            if (sprintSelectionText != null)
            {
                int count = selectedSprintGames.Count;
                int min = CognitiveSprintManager.MIN_GAMES;
                int max = CognitiveSprintManager.MAX_GAMES;

                sprintSelectionText.text = $"Juegos seleccionados: {count}/{max} (min: {min})";
                sprintSelectionText.color = count >= min ? Color.green : Color.yellow;
            }
        }

        private void CreateEntryFeeButtons()
        {
            // If buttons already exist in inspector, set them up
            if (entryFeeButtons != null && entryFeeButtons.Length > 0)
            {
                for (int i = 0; i < entryFeeButtons.Length && i < availableFees.Length; i++)
                {
                    decimal fee = availableFees[i];
                    int index = i;

                    var text = entryFeeButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null) text.text = $"${fee}";

                    entryFeeButtons[i].onClick.AddListener(() => SelectEntryFee(fee, index));
                }

                // Select first button by default
                SelectEntryFee(availableFees[0], 0);
            }
        }

        #region Custom Amount Input

        private void SetupCustomAmountInput()
        {
            if (customAmountInput == null) return;

            // Configure input field
            customAmountInput.contentType = TMP_InputField.ContentType.DecimalNumber;
            customAmountInput.characterLimit = 6; // Max "250.00"

            // Add listeners
            customAmountInput.onEndEdit.AddListener(OnCustomAmountEndEdit);
            customAmountInput.onValueChanged.AddListener(OnCustomAmountChanged);

            // Set placeholder
            var placeholder = customAmountInput.placeholder as TextMeshProUGUI;
            if (placeholder != null)
            {
                placeholder.text = "Custom...";
            }
        }

        private void OnCustomAmountChanged(string value)
        {
            // Real-time validation feedback (optional)
            if (string.IsNullOrEmpty(value)) return;

            if (decimal.TryParse(value, out decimal amount))
            {
                // Update earnings preview in real-time
                UpdateEarningsPreview(amount);
            }
        }

        private void OnCustomAmountEndEdit(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                // Revert to last selected preset
                SelectEntryFee(availableFees[selectedFeeButtonIndex], selectedFeeButtonIndex);
                return;
            }

            if (decimal.TryParse(value, out decimal amount))
            {
                // Clamp to valid range
                amount = ClampEntryFee(amount);

                // Update input field with clamped value
                customAmountInput.text = amount.ToString("F2");

                // Deselect preset buttons
                DeselectAllPresetButtons();

                // Set as selected fee
                selectedEntryFee = amount;
                UpdateEarningsFeedback();
                UpdateSelectedFeeDisplay();
                UpdateUI();

                Debug.Log($"[GameSelection] Custom entry fee: ${amount:F2}");
            }
            else
            {
                // Invalid input, revert
                customAmountInput.text = "";
                SelectEntryFee(availableFees[selectedFeeButtonIndex], selectedFeeButtonIndex);
            }
        }

        private decimal ClampEntryFee(decimal amount)
        {
            if (amount < MIN_ENTRY_FEE) return MIN_ENTRY_FEE;
            if (amount > MAX_ENTRY_FEE) return MAX_ENTRY_FEE;
            return amount;
        }

        private void DeselectAllPresetButtons()
        {
            if (entryFeeButtons == null) return;

            for (int i = 0; i < entryFeeButtons.Length; i++)
            {
                var img = entryFeeButtons[i].GetComponent<Image>();
                if (img != null)
                {
                    img.color = new Color(0.2f, 0.18f, 0.25f, 1f); // Dark (unselected)
                }
            }
        }

        private void UpdateMinMaxText()
        {
            if (minMaxText != null)
            {
                minMaxText.text = $"Min: ${MIN_ENTRY_FEE} | Max: ${MAX_ENTRY_FEE}";
            }
        }

        #endregion

        #region Earnings Feedback

        private void UpdateEarningsFeedback()
        {
            if (earningsText == null) return;

            decimal potentialWin = CalculatePotentialWinnings(selectedEntryFee);
            earningsText.text = $"Ganas: ${potentialWin:F2}";
            earningsText.color = new Color(0.2f, 1f, 0.33f, 1f); // Green
        }

        private void UpdateEarningsPreview(decimal previewAmount)
        {
            if (earningsText == null) return;

            decimal clamped = ClampEntryFee(previewAmount);
            decimal potentialWin = CalculatePotentialWinnings(clamped);
            earningsText.text = $"Ganas: ${potentialWin:F2}";
        }

        private decimal CalculatePotentialWinnings(decimal entryFee)
        {
            // Winner gets 70% of total pot (entry fee x 2)
            // Triumph takes 20%, platform takes 10%
            return entryFee * 2 * (decimal)WINNER_PERCENTAGE;
        }

        private void UpdateSelectedFeeDisplay()
        {
            if (selectedFeeText != null)
            {
                selectedFeeText.text = $"Entry: ${selectedEntryFee:F2}";
            }
        }

        #endregion

        #region Online Players Counter

        private void InitializeOnlinePlayersCounter()
        {
            // Start with simulated count (replace with real service later)
            UpdateOnlinePlayersCount(UnityEngine.Random.Range(80, 200));

            // Simulate periodic updates
            InvokeRepeating(nameof(SimulateOnlinePlayersUpdate), 5f, 10f);
        }

        private void SimulateOnlinePlayersUpdate()
        {
            // Simulate fluctuation (+/- 5-15 players)
            int change = UnityEngine.Random.Range(-15, 16);
            int newCount = Mathf.Clamp(currentOnlinePlayers + change, 50, 500);
            UpdateOnlinePlayersCount(newCount);
        }

        private void UpdateOnlinePlayersCount(int count)
        {
            currentOnlinePlayers = count;

            if (onlinePlayersText != null)
            {
                onlinePlayersText.text = $"{count} JUGADORES EN LÍNEA";
            }

            // Update indicator color based on player count
            if (onlineIndicator != null)
            {
                if (count >= 100)
                    onlineIndicator.color = new Color(0.2f, 1f, 0.33f, 1f); // Green - many players
                else if (count >= 50)
                    onlineIndicator.color = new Color(1f, 0.84f, 0f, 1f); // Yellow - moderate
                else
                    onlineIndicator.color = new Color(1f, 0.4f, 0.3f, 1f); // Red - few players
            }
        }

        /// <summary>
        /// Call this from a real matchmaking service to update player count
        /// </summary>
        public void SetOnlinePlayersCount(int count)
        {
            CancelInvoke(nameof(SimulateOnlinePlayersUpdate));
            UpdateOnlinePlayersCount(count);
        }

        #endregion

        private void SelectEntryFee(decimal fee, int buttonIndex)
        {
            selectedEntryFee = fee;
            selectedFeeButtonIndex = buttonIndex;

            // Clear custom input when selecting preset
            if (customAmountInput != null)
            {
                customAmountInput.text = "";
            }

            // Update button visuals
            if (entryFeeButtons != null)
            {
                for (int i = 0; i < entryFeeButtons.Length; i++)
                {
                    var img = entryFeeButtons[i].GetComponent<Image>();
                    if (img != null)
                    {
                        // Highlight selected button
                        img.color = i == buttonIndex
                            ? new Color(1f, 0.84f, 0f, 1f) // Gold
                            : new Color(0.2f, 0.18f, 0.25f, 1f); // Dark
                    }
                }
            }

            UpdateSelectedFeeDisplay();
            UpdateEarningsFeedback();
            UpdateUI();
        }

        private void OnFindOpponentClicked()
        {
            if (isCognitiveSprintMode)
            {
                if (selectedSprintGames.Count >= CognitiveSprintManager.MIN_GAMES)
                {
                    OnCognitiveSprintSelected?.Invoke(selectedSprintGames, selectedEntryFee);
                }
            }
            else
            {
                if (selectedGame.HasValue)
                {
                    OnGameSelected?.Invoke(selectedGame.Value, selectedEntryFee);
                }
            }
        }

        private void UpdateUI()
        {
            // Update title
            if (titleText != null)
            {
                titleText.text = isCognitiveSprintMode ? "Cognitive Sprint" : "Selecciona un Juego";
            }

            // Update find opponent button
            bool canProceed = isCognitiveSprintMode
                ? selectedSprintGames.Count >= CognitiveSprintManager.MIN_GAMES
                : selectedGame.HasValue;

            if (findOpponentButton != null)
            {
                findOpponentButton.interactable = canProceed;
            }

            if (findOpponentText != null)
            {
                findOpponentText.text = canProceed
                    ? $"Buscar Oponente (${selectedEntryFee})"
                    : "Selecciona un juego";
            }

            // Show/hide sprint panel
            if (cognitiveSprintPanel != null)
            {
                cognitiveSprintPanel.SetActive(isCognitiveSprintMode);
            }

            // Update cognitive sprint button text
            var sprintBtnText = cognitiveSprintButton?.GetComponentInChildren<TextMeshProUGUI>();
            if (sprintBtnText != null)
            {
                sprintBtnText.text = isCognitiveSprintMode ? "Juego Individual" : "Cognitive Sprint";
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);

            // Reset state
            selectedGame = null;
            selectedSprintGames.Clear();
            isCognitiveSprintMode = false;
            selectedEntryFee = MIN_ENTRY_FEE;
            selectedFeeButtonIndex = 0;

            // Reset custom input
            if (customAmountInput != null)
            {
                customAmountInput.text = "";
            }

            // Reset to first preset button
            if (entryFeeButtons != null && entryFeeButtons.Length > 0)
            {
                SelectEntryFee(availableFees[0], 0);
            }

            // Reset all game card checkmarks
            foreach (Transform child in gamesContainer)
            {
                var cardRef = child.GetComponent<GameCardReference>();
                if (cardRef?.Checkmark != null)
                {
                    cardRef.Checkmark.SetActive(false);
                }
            }

            UpdateUI();
            UpdateEarningsFeedback();
            UpdateSprintSelectionText();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            // Cleanup
            CancelInvoke(nameof(SimulateOnlinePlayersUpdate));
        }

        #region Public Getters

        /// <summary>
        /// Get current selected entry fee
        /// </summary>
        public decimal GetSelectedEntryFee() => selectedEntryFee;

        /// <summary>
        /// Get potential winnings for current entry fee
        /// </summary>
        public decimal GetPotentialWinnings() => CalculatePotentialWinnings(selectedEntryFee);

        /// <summary>
        /// Check if entry fee is valid
        /// </summary>
        public bool IsValidEntryFee(decimal fee) => fee >= MIN_ENTRY_FEE && fee <= MAX_ENTRY_FEE;

        /// <summary>
        /// Get minimum entry fee
        /// </summary>
        public static decimal MinEntryFee => MIN_ENTRY_FEE;

        /// <summary>
        /// Get maximum entry fee
        /// </summary>
        public static decimal MaxEntryFee => MAX_ENTRY_FEE;

        #endregion
    }

    /// <summary>
    /// Helper component to store game type reference on card
    /// </summary>
    public class GameCardReference : MonoBehaviour
    {
        public GameType GameType;
        public GameObject Checkmark;
    }
}
