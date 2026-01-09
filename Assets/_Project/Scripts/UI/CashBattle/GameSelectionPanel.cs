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

        // State
        private GameType? selectedGame;
        private decimal selectedEntryFee = 1m;
        private List<GameType> selectedSprintGames = new List<GameType>();
        private bool isCognitiveSprintMode = false;

        // Available entry fees
        private readonly decimal[] availableFees = { 1m, 5m, 10m, 25m, 50m, 100m };

        private void Start()
        {
            SetupListeners();
            CreateGameCards();
            CreateEntryFeeButtons();
            UpdateUI();
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
            }
        }

        private void SelectEntryFee(decimal fee, int buttonIndex)
        {
            selectedEntryFee = fee;

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

            if (selectedFeeText != null)
            {
                selectedFeeText.text = $"Entry: ${selectedEntryFee}";
            }

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
            selectedEntryFee = 1m;
            UpdateUI();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
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
