using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Games;
using DigitPark.Localization;

namespace DigitPark.UI.CashBattle
{
    /// <summary>
    /// Manager for CashBattle 1v1 scene.
    /// Handles game selection via dropdown + modal, entry fees, online players, and matchmaking initiation.
    /// Supports custom entry fees up to $250 (Triumph withdrawal limit)
    /// </summary>
    public class CashBattle1v1Manager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button backButton;

        [Header("Game Selection")]
        [SerializeField] private TMP_Dropdown gameDropdown;
        [SerializeField] private Button viewDetailsButton;
        [SerializeField] private Image selectedGameIcon;
        [SerializeField] private TextMeshProUGUI selectedGameDescription;

        [Header("Game Selection Modal")]
        [SerializeField] private GameObject gameSelectionModal;
        [SerializeField] private Transform gameCardsContainer;
        [SerializeField] private Button confirmGameButton;
        [SerializeField] private Button closeModalButton;

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
        private const decimal WINNER_PERCENTAGE = 0.70m; // 70% to winner (Triumph takes 20%, platform 10%)

        // State
        private GameType? selectedGame;
        private decimal selectedEntryFee = 1m;
        private List<GameType> selectedSprintGames = new List<GameType>();
        private bool isCognitiveSprintMode = false;
        private int currentOnlinePlayers = 0;
        private int selectedFeeButtonIndex = 0;
        private GameObject selectedModalCard;

        // Available entry fees (preset buttons)
        private readonly decimal[] availableFees = { 1m, 5m, 10m, 25m, 50m, 100m };

        // Dropdown index to GameType mapping
        private static readonly GameType[] dropdownGameTypes = {
            GameType.DigitRush,
            GameType.FlashTap,
            GameType.MemoryPairs,
            GameType.OddOneOut,
            GameType.QuickMath
        };

        // Game description keys (localized via AutoLocalizer)
        private static readonly Dictionary<GameType, string> gameDescriptionKeys = new Dictionary<GameType, string>
        {
            { GameType.DigitRush, "cash_game_desc_digitrush" },
            { GameType.FlashTap, "cash_game_desc_flashtap" },
            { GameType.MemoryPairs, "cash_game_desc_memorypairs" },
            { GameType.OddOneOut, "cash_game_desc_oddoneout" },
            { GameType.QuickMath, "cash_game_desc_quickmath" }
        };

        // Game icon names (matching Assets/_Project/Art/Icons/Games/)
        private static readonly Dictionary<GameType, string> gameIconNames = new Dictionary<GameType, string>
        {
            { GameType.DigitRush, "DigitRushIcon" },
            { GameType.FlashTap, "FlashTapIcon" },
            { GameType.MemoryPairs, "MemoryPairsIcon" },
            { GameType.OddOneOut, "OddOneOutIcon" },
            { GameType.QuickMath, "QuickMathIcon" }
        };

        private void Start()
        {
            SetupListeners();
            SetupGameDropdown();
            CreateEntryFeeButtons();
            SetupCustomAmountInput();
            InitializeOnlinePlayersCounter();
            UpdateUI();
            UpdateEarningsFeedback();
            UpdateMinMaxText();
        }

        private void SetupListeners()
        {
            // Disable auto-navigation from BackButtonGold prefab to prevent double listener
            var autoNav = backButton?.GetComponent<DigitPark.UI.BackButtonGold>();
            if (autoNav != null) autoNav.DisableAutoNavigation();
            backButton?.onClick.AddListener(() => OnBackClicked?.Invoke());
            findOpponentButton?.onClick.AddListener(OnFindOpponentClicked);
            cognitiveSprintButton?.onClick.AddListener(ToggleCognitiveSprintMode);

            // Modal button listeners
            viewDetailsButton?.onClick.AddListener(ShowGameModal);
            closeModalButton?.onClick.AddListener(HideGameModal);
            confirmGameButton?.onClick.AddListener(OnModalConfirmClicked);
        }

        #region Game Dropdown

        private void SetupGameDropdown()
        {
            if (gameDropdown == null) return;

            // Populate dropdown options
            gameDropdown.ClearOptions();
            List<string> options = new List<string>();
            foreach (var gameType in dropdownGameTypes)
            {
                options.Add(gameType.ToString());
            }
            gameDropdown.AddOptions(options);

            // Set default selection
            gameDropdown.value = 0;
            gameDropdown.onValueChanged.AddListener(OnGameDropdownChanged);

            // Update initial details
            OnGameDropdownChanged(0);
        }

        private void OnGameDropdownChanged(int index)
        {
            if (index < 0 || index >= dropdownGameTypes.Length) return;

            GameType gameType = dropdownGameTypes[index];
            selectedGame = gameType;

            // Update selected game icon
            if (selectedGameIcon != null && gameIconNames.TryGetValue(gameType, out string iconName))
            {
                Sprite iconSprite = Resources.Load<Sprite>($"Icons/Games/{iconName}");
                if (iconSprite != null)
                {
                    selectedGameIcon.sprite = iconSprite;
                    selectedGameIcon.color = Color.white;
                }
            }

            // Update description
            if (selectedGameDescription != null && gameDescriptionKeys.TryGetValue(gameType, out string descKey))
            {
                selectedGameDescription.text = AutoLocalizer.Get(descKey);
            }

            UpdateUI();
        }

        #endregion

        #region Game Selection Modal

        private void ShowGameModal()
        {
            if (gameSelectionModal == null) return;

            gameSelectionModal.SetActive(true);

            // Reset card selection highlights
            if (gameCardsContainer != null)
            {
                foreach (Transform child in gameCardsContainer)
                {
                    var checkmark = child.Find("Checkmark");
                    if (checkmark != null) checkmark.gameObject.SetActive(false);

                    // Reset outline color
                    var outline = child.GetComponent<Outline>();
                    if (outline != null) outline.effectColor = new Color(0.4f, 0.3f, 0.55f, 0.35f);
                }
            }

            selectedModalCard = null;
        }

        private void HideGameModal()
        {
            if (gameSelectionModal != null)
            {
                gameSelectionModal.SetActive(false);
            }
        }

        private void OnModalConfirmClicked()
        {
            // If a card was selected in the modal, apply it to the dropdown
            if (selectedModalCard != null)
            {
                string cardName = selectedModalCard.name;

                // Check if CognitiveSprint was selected
                if (cardName.Contains("CognitiveSprint"))
                {
                    // Toggle cognitive sprint mode
                    if (!isCognitiveSprintMode)
                    {
                        ToggleCognitiveSprintMode();
                    }
                }
                else
                {
                    // Find matching game type from card name
                    for (int i = 0; i < dropdownGameTypes.Length; i++)
                    {
                        if (cardName.Contains(dropdownGameTypes[i].ToString()))
                        {
                            if (gameDropdown != null)
                            {
                                gameDropdown.value = i;
                            }
                            OnGameDropdownChanged(i);
                            break;
                        }
                    }
                }
            }

            HideGameModal();
        }

        private void OnGameCardClicked(GameType gameType, GameObject card)
        {
            if (isCognitiveSprintMode)
            {
                ToggleSprintGameSelection(gameType, card);
            }
            else
            {
                SelectModalCard(card);
            }
        }

        private void SelectModalCard(GameObject card)
        {
            // Deselect previous
            if (gameCardsContainer != null)
            {
                foreach (Transform child in gameCardsContainer)
                {
                    var checkmark = child.Find("Checkmark");
                    if (checkmark != null) checkmark.gameObject.SetActive(false);

                    var outline = child.GetComponent<Outline>();
                    if (outline != null) outline.effectColor = new Color(0.4f, 0.3f, 0.55f, 0.35f);
                }
            }

            // Select new card
            selectedModalCard = card;
            var selectedCheck = card.transform.Find("Checkmark");
            if (selectedCheck != null) selectedCheck.gameObject.SetActive(true);

            // Animate glow
            AnimateCardGlow(card);
        }

        private void AnimateCardGlow(GameObject card)
        {
            var outline = card.GetComponent<Outline>();
            if (outline != null)
            {
                outline.effectColor = new Color(1f, 0.84f, 0f, 0.8f); // Gold glow
                StartCoroutine(PulseOutline(outline));
            }
        }

        private IEnumerator PulseOutline(Outline outline)
        {
            if (outline == null) yield break;

            Color goldBright = new Color(1f, 0.84f, 0f, 0.8f);
            Color goldDim = new Color(0.85f, 0.65f, 0.13f, 0.5f);

            float duration = 0.4f;
            float elapsed = 0f;

            // Pulse from bright to dim
            while (elapsed < duration)
            {
                if (outline == null) yield break;
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                outline.effectColor = Color.Lerp(goldBright, goldDim, t);
                yield return null;
            }

            if (outline != null)
            {
                outline.effectColor = goldDim;
            }
        }

        #endregion

        #region Single Game Selection

        private void SelectSingleGame(GameType gameType)
        {
            selectedGame = gameType;

            // Update dropdown to match
            for (int i = 0; i < dropdownGameTypes.Length; i++)
            {
                if (dropdownGameTypes[i] == gameType)
                {
                    if (gameDropdown != null)
                    {
                        gameDropdown.value = i;
                    }
                    break;
                }
            }

            UpdateUI();
        }

        #endregion

        #region Cognitive Sprint

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

            // Reset dropdown
            if (gameDropdown != null && !isCognitiveSprintMode)
            {
                gameDropdown.value = 0;
                OnGameDropdownChanged(0);
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

                sprintSelectionText.text = AutoLocalizer.Get("cash_games_selected", count, max, min);
                sprintSelectionText.color = count >= min ? Color.green : Color.yellow;
            }
        }

        #endregion

        #region Entry Fee

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

        #endregion

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
                placeholder.text = AutoLocalizer.Get("custom_option");
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
                minMaxText.text = $"{AutoLocalizer.Get("cash_bet_min")}: ${MIN_ENTRY_FEE} | {AutoLocalizer.Get("cash_bet_max")}: ${MAX_ENTRY_FEE}";
            }
        }

        #endregion

        #region Earnings Feedback

        private void UpdateEarningsFeedback()
        {
            if (earningsText == null) return;

            decimal potentialWin = CalculatePotentialWinnings(selectedEntryFee);
            earningsText.text = AutoLocalizer.Get("cash_win_amount", potentialWin);
            earningsText.color = new Color(0.2f, 1f, 0.33f, 1f); // Green
        }

        private void UpdateEarningsPreview(decimal previewAmount)
        {
            if (earningsText == null) return;

            decimal clamped = ClampEntryFee(previewAmount);
            decimal potentialWin = CalculatePotentialWinnings(clamped);
            earningsText.text = AutoLocalizer.Get("cash_win_amount", potentialWin);
        }

        private decimal CalculatePotentialWinnings(decimal entryFee)
        {
            // Winner gets 70% of total pot (entry fee x 2)
            // Triumph takes 20%, platform takes 10%
            return entryFee * 2 * WINNER_PERCENTAGE;
        }

        private void UpdateSelectedFeeDisplay()
        {
            if (selectedFeeText != null)
            {
                selectedFeeText.text = AutoLocalizer.Get("entry_fee_display", $"${selectedEntryFee:F2}");
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
                onlinePlayersText.text = AutoLocalizer.Get("cash_players_online", count);
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
                titleText.text = isCognitiveSprintMode
                    ? AutoLocalizer.Get("cognitive_sprint_title")
                    : AutoLocalizer.Get("select_game");
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
                    ? AutoLocalizer.Get("cash_find_opponent", $"${selectedEntryFee}")
                    : AutoLocalizer.Get("cash_select_game");
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
                sprintBtnText.text = isCognitiveSprintMode ? AutoLocalizer.Get("cash_single_game") : AutoLocalizer.Get("cash_cognitive_sprint");
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

            // Reset dropdown to first option
            if (gameDropdown != null)
            {
                gameDropdown.value = 0;
                OnGameDropdownChanged(0);
            }

            // Hide modal if open
            HideGameModal();

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
