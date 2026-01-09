using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using DigitPark.Localization;
using DigitPark.UI.CashBattle;
using DigitPark.Games;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager de Cash Battle - Competencias con dinero real (18+)
    /// Integra con Triumph para pagos
    /// Premium UI with card-based layout
    /// </summary>
    public class CashBattleManager : MonoBehaviour
    {
        [Header("UI - Header")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI balanceText;
        [SerializeField] private Button backButton;

        [Header("UI - Main Panel (Cards)")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private Button battles1v1Card;
        [SerializeField] private Button cashTournamentsCard;
        [SerializeField] private Button walletCard;
        [SerializeField] private Button historyCard;

        [Header("UI - Sub Panels")]
        [SerializeField] private GameSelectionPanel gameSelectionPanel;
        [SerializeField] private TournamentListPanel tournamentListPanel;
        [SerializeField] private GameObject walletPanel;
        [SerializeField] private GameObject historyPanel;

        [Header("UI - Age Verification")]
        [SerializeField] private GameObject ageVerificationPanel;
        [SerializeField] private Button verifyAgeButton;
        [SerializeField] private TextMeshProUGUI verificationStatusText;
        [SerializeField] private TextMeshProUGUI verificationTitleText;
        [SerializeField] private TextMeshProUGUI verificationDescText;

        [Header("UI - Matchmaking")]
        [SerializeField] private GameObject matchmakingPanel;
        [SerializeField] private TextMeshProUGUI matchmakingStatusText;
        [SerializeField] private Button cancelMatchmakingButton;

        [Header("Settings")]
        [SerializeField] private bool requireAgeVerification = true;

        private bool isAgeVerified = false;
        private float currentBalance = 0f;
        private CashBattleState currentState = CashBattleState.Main;

        private void Start()
        {
            Debug.Log("[CashBattle] CashBattleManager iniciado");

            SetupListeners();
            CheckAgeVerification();
            UpdateUI();
            UpdateLocalizedTexts();

            // Subscribe to language changes
            LocalizationManager.OnLanguageChanged += UpdateLocalizedTexts;
        }

        private void OnDestroy()
        {
            LocalizationManager.OnLanguageChanged -= UpdateLocalizedTexts;
        }

        private void SetupListeners()
        {
            // Navigation
            backButton?.onClick.AddListener(OnBackClicked);

            // Main cards
            battles1v1Card?.onClick.AddListener(OnBattles1v1Clicked);
            cashTournamentsCard?.onClick.AddListener(OnCashTournamentsClicked);
            walletCard?.onClick.AddListener(OnWalletClicked);
            historyCard?.onClick.AddListener(OnHistoryClicked);

            // Age verification
            verifyAgeButton?.onClick.AddListener(OnVerifyAgeClicked);

            // Game Selection Panel events
            if (gameSelectionPanel != null)
            {
                gameSelectionPanel.OnBackClicked += () => NavigateTo(CashBattleState.Main);
                gameSelectionPanel.OnGameSelected += OnSingleGameSelected;
                gameSelectionPanel.OnCognitiveSprintSelected += OnCognitiveSprintSelected;
            }

            // Tournament List Panel events
            if (tournamentListPanel != null)
            {
                tournamentListPanel.OnBackClicked += () => NavigateTo(CashBattleState.Main);
                tournamentListPanel.OnTournamentSelected += OnTournamentSelected;
            }

            // Matchmaking
            cancelMatchmakingButton?.onClick.AddListener(CancelMatchmaking);
        }

        private void UpdateLocalizedTexts()
        {
            // Title is always "Cash Battle" - could be localized if needed
            if (titleText != null)
                titleText.text = "Cash Battle";

            // Verification texts could be localized
            if (verificationTitleText != null)
                verificationTitleText.text = GetLocalizedText("cash_battle_verification_title");

            if (verificationDescText != null)
                verificationDescText.text = GetLocalizedText("cash_battle_verification_desc");
        }

        private string GetLocalizedText(string key)
        {
            if (LocalizationManager.Instance != null)
                return LocalizationManager.Instance.GetText(key);
            return key;
        }

        #region Age Verification

        private void CheckAgeVerification()
        {
            // TODO: Verificar con Triumph si el usuario ya está verificado
            isAgeVerified = PlayerPrefs.GetInt("AgeVerified", 0) == 1;

            if (requireAgeVerification && !isAgeVerified)
            {
                ShowAgeVerificationPanel();
            }
            else
            {
                ShowMainPanel();
            }
        }

        private void ShowAgeVerificationPanel()
        {
            if (ageVerificationPanel != null)
                ageVerificationPanel.SetActive(true);
            if (mainPanel != null)
                mainPanel.SetActive(false);

            Debug.Log("[CashBattle] Mostrando panel de verificación de edad");
        }

        private void ShowMainPanel()
        {
            if (ageVerificationPanel != null)
                ageVerificationPanel.SetActive(false);
            if (mainPanel != null)
                mainPanel.SetActive(true);

            Debug.Log("[CashBattle] Mostrando panel principal");
        }

        private void OnVerifyAgeClicked()
        {
            Debug.Log("[CashBattle] Iniciando verificación de edad");

            // TODO: Integrar con Triumph para verificación real
            StartAgeVerification();
        }

        private void StartAgeVerification()
        {
            // TODO: Llamar a Triumph SDK para verificación de ID
            // Triumph.VerifyAge(OnAgeVerificationComplete);

            if (verificationStatusText != null)
            {
                verificationStatusText.text = "Verificando...";
                verificationStatusText.color = new Color(1f, 0.84f, 0f, 1f); // Gold
            }

            // Disable button while verifying
            if (verifyAgeButton != null)
                verifyAgeButton.interactable = false;

            // Simular delay de verificación
            Invoke(nameof(SimulateVerificationComplete), 2f);
        }

        private void SimulateVerificationComplete()
        {
            // Simulación - en producción esto viene de Triumph
            OnAgeVerificationComplete(true);
        }

        private void OnAgeVerificationComplete(bool verified)
        {
            isAgeVerified = verified;
            PlayerPrefs.SetInt("AgeVerified", verified ? 1 : 0);
            PlayerPrefs.Save();

            if (verified)
            {
                Debug.Log("[CashBattle] Edad verificada exitosamente");

                if (verificationStatusText != null)
                {
                    verificationStatusText.text = "¡Verificado!";
                    verificationStatusText.color = new Color(0.3f, 1f, 0.5f, 1f); // Green
                }

                // Mostrar panel principal después de un momento
                Invoke(nameof(ShowMainPanel), 1f);
            }
            else
            {
                Debug.Log("[CashBattle] Verificación de edad fallida");

                if (verificationStatusText != null)
                {
                    verificationStatusText.text = "Verificación fallida. Debes ser mayor de 18.";
                    verificationStatusText.color = new Color(1f, 0.4f, 0.4f, 1f); // Red
                }

                // Re-enable button
                if (verifyAgeButton != null)
                    verifyAgeButton.interactable = true;
            }
        }

        #endregion

        #region UI Updates

        private void UpdateUI()
        {
            UpdateBalance();
        }

        private void UpdateBalance()
        {
            // TODO: Obtener balance real de Triumph
            currentBalance = PlayerPrefs.GetFloat("CashBalance", 0f);

            if (balanceText != null)
                balanceText.text = $"{currentBalance:F2}";
        }

        /// <summary>
        /// Called when balance is updated from Triumph
        /// </summary>
        public void SetBalance(float balance)
        {
            currentBalance = balance;
            PlayerPrefs.SetFloat("CashBalance", balance);

            if (balanceText != null)
                balanceText.text = $"{balance:F2}";
        }

        #endregion

        #region Navigation System

        private void NavigateTo(CashBattleState newState)
        {
            Debug.Log($"[CashBattle] Navegando de {currentState} a {newState}");

            // Hide current panel
            HideCurrentPanel();

            // Update state
            currentState = newState;

            // Show new panel
            ShowCurrentPanel();

            // Update header
            UpdateHeaderForState();
        }

        private void HideCurrentPanel()
        {
            switch (currentState)
            {
                case CashBattleState.Main:
                    if (mainPanel != null) mainPanel.SetActive(false);
                    break;
                case CashBattleState.GameSelection:
                    if (gameSelectionPanel != null) gameSelectionPanel.Hide();
                    break;
                case CashBattleState.TournamentList:
                    if (tournamentListPanel != null) tournamentListPanel.Hide();
                    break;
                case CashBattleState.Wallet:
                    if (walletPanel != null) walletPanel.SetActive(false);
                    break;
                case CashBattleState.History:
                    if (historyPanel != null) historyPanel.SetActive(false);
                    break;
                case CashBattleState.Matchmaking:
                    if (matchmakingPanel != null) matchmakingPanel.SetActive(false);
                    break;
            }
        }

        private void ShowCurrentPanel()
        {
            switch (currentState)
            {
                case CashBattleState.Main:
                    if (mainPanel != null) mainPanel.SetActive(true);
                    break;
                case CashBattleState.GameSelection:
                    if (gameSelectionPanel != null) gameSelectionPanel.Show();
                    break;
                case CashBattleState.TournamentList:
                    if (tournamentListPanel != null) tournamentListPanel.Show();
                    break;
                case CashBattleState.Wallet:
                    if (walletPanel != null) walletPanel.SetActive(true);
                    break;
                case CashBattleState.History:
                    if (historyPanel != null) historyPanel.SetActive(true);
                    break;
                case CashBattleState.Matchmaking:
                    if (matchmakingPanel != null) matchmakingPanel.SetActive(true);
                    break;
            }
        }

        private void UpdateHeaderForState()
        {
            if (titleText == null) return;

            switch (currentState)
            {
                case CashBattleState.Main:
                    titleText.text = "Cash Battle";
                    break;
                case CashBattleState.GameSelection:
                    titleText.text = "Batallas 1v1";
                    break;
                case CashBattleState.TournamentList:
                    titleText.text = "Torneos Cash";
                    break;
                case CashBattleState.Wallet:
                    titleText.text = "Mi Wallet";
                    break;
                case CashBattleState.History:
                    titleText.text = "Historial";
                    break;
                case CashBattleState.Matchmaking:
                    titleText.text = "Buscando Oponente...";
                    break;
            }
        }

        #endregion

        #region Navigation Callbacks

        private void OnBackClicked()
        {
            // Handle back based on current state
            switch (currentState)
            {
                case CashBattleState.Main:
                    Debug.Log("[CashBattle] Volviendo al Main Menu");
                    SceneManager.LoadScene("MainMenu");
                    break;
                case CashBattleState.Matchmaking:
                    CancelMatchmaking();
                    break;
                default:
                    NavigateTo(CashBattleState.Main);
                    break;
            }
        }

        private void OnBattles1v1Clicked()
        {
            Debug.Log("[CashBattle] Navegando a Battles 1v1");

            if (!isAgeVerified && requireAgeVerification)
            {
                ShowAgeVerificationPanel();
                return;
            }

            NavigateTo(CashBattleState.GameSelection);
        }

        private void OnCashTournamentsClicked()
        {
            Debug.Log("[CashBattle] Navegando a Cash Tournaments");

            if (!isAgeVerified && requireAgeVerification)
            {
                ShowAgeVerificationPanel();
                return;
            }

            NavigateTo(CashBattleState.TournamentList);
        }

        private void OnWalletClicked()
        {
            Debug.Log("[CashBattle] Abriendo Wallet");

            if (!isAgeVerified && requireAgeVerification)
            {
                ShowAgeVerificationPanel();
                return;
            }

            // TODO: Implement wallet panel
            NavigateTo(CashBattleState.Wallet);
        }

        private void OnHistoryClicked()
        {
            Debug.Log("[CashBattle] Abriendo Historial");

            if (!isAgeVerified && requireAgeVerification)
            {
                ShowAgeVerificationPanel();
                return;
            }

            // TODO: Implement history panel
            NavigateTo(CashBattleState.History);
        }

        #endregion

        #region Game Selection Handlers

        private void OnSingleGameSelected(GameType gameType, decimal entryFee)
        {
            Debug.Log($"[CashBattle] Juego seleccionado: {gameType}, Entry: ${entryFee}");

            // Check balance
            if (currentBalance < (float)entryFee)
            {
                Debug.LogWarning("[CashBattle] Balance insuficiente");
                // TODO: Show insufficient balance popup
                return;
            }

            // Start matchmaking for single game
            StartMatchmaking(new List<GameType> { gameType }, entryFee);
        }

        private void OnCognitiveSprintSelected(List<GameType> games, decimal entryFee)
        {
            Debug.Log($"[CashBattle] Cognitive Sprint seleccionado: {games.Count} juegos, Entry: ${entryFee}");

            // Check balance
            if (currentBalance < (float)entryFee)
            {
                Debug.LogWarning("[CashBattle] Balance insuficiente");
                // TODO: Show insufficient balance popup
                return;
            }

            // Start matchmaking for cognitive sprint
            StartMatchmaking(games, entryFee);
        }

        private void OnTournamentSelected(TournamentInfo tournament)
        {
            Debug.Log($"[CashBattle] Torneo seleccionado: {tournament.Name}, Entry: ${tournament.EntryFee}");

            // Check balance
            if (currentBalance < (float)tournament.EntryFee)
            {
                Debug.LogWarning("[CashBattle] Balance insuficiente");
                // TODO: Show insufficient balance popup
                return;
            }

            // TODO: Join tournament via Triumph API
            JoinTournament(tournament);
        }

        #endregion

        #region Matchmaking

        private void StartMatchmaking(List<GameType> games, decimal entryFee)
        {
            NavigateTo(CashBattleState.Matchmaking);

            if (matchmakingStatusText != null)
            {
                matchmakingStatusText.text = "Buscando oponente...";
            }

            // TODO: Implement actual matchmaking with Triumph
            // Triumph.FindMatch(games, entryFee, OnMatchFound, OnMatchmakingFailed);

            // For now, simulate finding an opponent
            Invoke(nameof(SimulateMatchFound), 3f);
        }

        private void SimulateMatchFound()
        {
            Debug.Log("[CashBattle] Match encontrado (simulación)");

            if (matchmakingStatusText != null)
            {
                matchmakingStatusText.text = "¡Oponente encontrado!";
            }

            // TODO: Load game scene with context
            Invoke(nameof(LoadGameScene), 1.5f);
        }

        private void LoadGameScene()
        {
            // TODO: Set up GameContext and load appropriate scene
            Debug.Log("[CashBattle] Cargando escena de juego...");

            // For now, just return to main
            NavigateTo(CashBattleState.Main);
        }

        private void CancelMatchmaking()
        {
            Debug.Log("[CashBattle] Matchmaking cancelado");

            // Cancel any pending invokes
            CancelInvoke(nameof(SimulateMatchFound));
            CancelInvoke(nameof(LoadGameScene));

            // TODO: Cancel matchmaking on server
            // Triumph.CancelMatchmaking();

            NavigateTo(CashBattleState.GameSelection);
        }

        #endregion

        #region Tournament

        private void JoinTournament(TournamentInfo tournament)
        {
            Debug.Log($"[CashBattle] Unirse a torneo: {tournament.Name}");

            // TODO: Implement tournament join via Triumph
            // Triumph.JoinTournament(tournament.Id, OnTournamentJoined, OnTournamentJoinFailed);
        }

        #endregion

        #region Public API for Triumph Integration

        /// <summary>
        /// Called by Triumph SDK when age verification completes
        /// </summary>
        public void OnTriumphAgeVerified(bool success)
        {
            OnAgeVerificationComplete(success);
        }

        /// <summary>
        /// Called by Triumph SDK when balance updates
        /// </summary>
        public void OnTriumphBalanceUpdated(float newBalance)
        {
            SetBalance(newBalance);
        }

        /// <summary>
        /// Opens Triumph deposit flow
        /// </summary>
        public void OpenDeposit()
        {
            // TODO: Triumph.OpenDeposit();
            Debug.Log("[CashBattle] Opening deposit flow");
        }

        /// <summary>
        /// Opens Triumph withdraw flow
        /// </summary>
        public void OpenWithdraw()
        {
            // TODO: Triumph.OpenWithdraw();
            Debug.Log("[CashBattle] Opening withdraw flow");
        }

        #endregion
    }

    /// <summary>
    /// States for CashBattle navigation
    /// </summary>
    public enum CashBattleState
    {
        Main,
        GameSelection,
        TournamentList,
        Wallet,
        History,
        Matchmaking
    }
}
