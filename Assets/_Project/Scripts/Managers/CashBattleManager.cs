using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using DigitPark.Localization;
using DigitPark.UI.CashBattle;
using DigitPark.Games;
using DigitPark.CashBattle;
using DigitPark.Monetization;
using DigitPark.Services;
using DG.Tweening;

// Alias para resolver conflictos de nombres entre namespaces
using ServiceMatchResult = DigitPark.Services.MatchResult;
using UITournamentInfo = DigitPark.UI.CashBattle.TournamentInfo;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager de Cash Battle - Competencias con dinero real (18+)
    /// Usa ServiceLocator para KYC, Wallet, Matchmaking y Tournaments
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
        [SerializeField] private CashBattle1v1Manager gameSelectionPanel;
        [SerializeField] private TournamentListPanel tournamentListPanel;

        [Header("UI - Bet Confirmation")]
        [SerializeField] private GameObject confirmBetPanel;
        [SerializeField] private TextMeshProUGUI confirmBetText;
        [SerializeField] private Button confirmBetButton;
        [SerializeField] private Button cancelBetButton;

        [Header("UI - Matchmaking")]
        [SerializeField] private GameObject matchmakingPanel;
        [SerializeField] private TextMeshProUGUI matchmakingStatusText;
        [SerializeField] private TextMeshProUGUI matchmakingTimerText;
        [SerializeField] private TextMeshProUGUI opponentNameText;
        [SerializeField] private Button cancelMatchmakingButton;

        // Services
        private IKYCService _kycService;
        private IWalletService _walletService;
        private IMatchmakingService _matchmakingService;
        private ITournamentService _tournamentService;

        private bool isKYCVerified = false;
        private CashBattleState currentState = CashBattleState.Main;
        private float matchmakingTimer = 0f;

        // Pending bet confirmation
        private CashGameType _pendingGameType;
        private decimal _pendingEntryFee;
        private List<GameType> _pendingSprintGames;

        // Propiedad para obtener balance
        private decimal CurrentBalance => _walletService?.CurrentBalance ?? 0m;

        private void Start()
        {
            Debug.Log("[CashBattle] CashBattleManager iniciado");

            InitializeServices();
            SetupListeners();
            SetupServiceListeners();
            CheckKYCVerification();
            UpdateUI();
            UpdateLocalizedTexts();

            LocalizationManager.OnLanguageChanged += UpdateLocalizedTexts;
        }

        private void OnDestroy()
        {
            CancelInvoke();
            LocalizationManager.OnLanguageChanged -= UpdateLocalizedTexts;
            UnsubscribeServiceListeners();
        }

        private void Update()
        {
            // Actualizar timer de matchmaking
            if (currentState == CashBattleState.Matchmaking && _matchmakingService?.IsSearching == true)
            {
                matchmakingTimer += Time.deltaTime;
                UpdateMatchmakingTimer();
            }
        }

        #region Initialization

        private void InitializeServices()
        {
            _kycService = ServiceLocator.KYC;
            _walletService = ServiceLocator.Wallet;
            _matchmakingService = ServiceLocator.Matchmaking;
            _tournamentService = ServiceLocator.Tournament;

            if (_kycService == null || _walletService == null || _matchmakingService == null)
            {
                Debug.LogError("[CashBattle] Servicios no disponibles. Verifica que ServiceLocator esté en Boot scene.");
            }

            Debug.Log($"[CashBattle] Servicios inicializados. Modo: {ServiceLocator.CurrentMode}");
        }

        private void SetupServiceListeners()
        {
            if (_walletService != null)
            {
                _walletService.OnBalanceChanged += OnWalletBalanceChanged;
            }

            if (_kycService != null)
            {
                _kycService.OnStatusChanged += OnKYCStatusChanged;
            }

            if (_matchmakingService != null)
            {
                _matchmakingService.OnStatusChanged += OnMatchmakingStatusChanged;
                _matchmakingService.OnMatchFound += OnMatchFound;
                _matchmakingService.OnMatchCompleted += OnMatchCompleted;
            }
        }

        private void UnsubscribeServiceListeners()
        {
            if (_walletService != null)
            {
                _walletService.OnBalanceChanged -= OnWalletBalanceChanged;
            }

            if (_kycService != null)
            {
                _kycService.OnStatusChanged -= OnKYCStatusChanged;
            }

            if (_matchmakingService != null)
            {
                _matchmakingService.OnStatusChanged -= OnMatchmakingStatusChanged;
                _matchmakingService.OnMatchFound -= OnMatchFound;
                _matchmakingService.OnMatchCompleted -= OnMatchCompleted;
            }
        }

        private void SetupListeners()
        {
            backButton?.onClick.AddListener(OnBackClicked);

            battles1v1Card?.onClick.AddListener(OnBattles1v1Clicked);
            cashTournamentsCard?.onClick.AddListener(OnCashTournamentsClicked);
            walletCard?.onClick.AddListener(OnWalletClicked);
            historyCard?.onClick.AddListener(OnHistoryClicked);

            if (gameSelectionPanel != null)
            {
                gameSelectionPanel.OnBackClicked += () => NavigateTo(CashBattleState.Main);
                gameSelectionPanel.OnGameSelected += OnSingleGameSelected;
                gameSelectionPanel.OnCognitiveSprintSelected += OnCognitiveSprintSelected;
            }

            if (tournamentListPanel != null)
            {
                tournamentListPanel.OnBackClicked += () => NavigateTo(CashBattleState.Main);
                tournamentListPanel.OnTournamentSelected += OnTournamentSelected;
            }

            cancelMatchmakingButton?.onClick.AddListener(CancelMatchmaking);

            confirmBetButton?.onClick.AddListener(OnConfirmBet);
            cancelBetButton?.onClick.AddListener(OnCancelBet);
        }

        #endregion

        #region Service Event Handlers

        private void OnWalletBalanceChanged(decimal newBalance)
        {
            UpdateBalanceDisplay();
            Debug.Log($"[CashBattle] Balance actualizado: ${newBalance:F2}");
        }

        private void OnKYCStatusChanged(KYCStatus newStatus)
        {
            Debug.Log($"[CashBattle] KYC Status cambió a: {newStatus}");
            isKYCVerified = newStatus == KYCStatus.FullyVerified;

            if (isKYCVerified)
            {
                ShowMainPanel();
            }
        }

        private void OnMatchmakingStatusChanged(MatchmakingStatus status)
        {
            Debug.Log($"[CashBattle] Matchmaking status: {status}");

            if (matchmakingStatusText != null)
            {
                matchmakingStatusText.text = status switch
                {
                    MatchmakingStatus.Searching => L("matchmaking_searching"),
                    MatchmakingStatus.Found => L("matchmaking_found"),
                    MatchmakingStatus.Confirming => L("matchmaking_confirming"),
                    MatchmakingStatus.Ready => L("matchmaking_ready"),
                    MatchmakingStatus.Cancelled => L("matchmaking_cancelled"),
                    MatchmakingStatus.Timeout => L("matchmaking_timeout"),
                    _ => ""
                };
            }
        }

        private void OnMatchFound(MatchInfo match)
        {
            Debug.Log($"[CashBattle] Match encontrado! Oponente: {match.Opponent.DisplayName}");

            if (opponentNameText != null)
            {
                opponentNameText.text = match.Opponent.DisplayName;
            }

            // Auto-confirmar y cargar juego después de un momento
            Invoke(nameof(ConfirmAndLoadGame), 2f);
        }

        private void OnMatchCompleted(ServiceMatchResult result)
        {
            Debug.Log($"[CashBattle] Match completado. Victoria: {result.IsWinner}, Ganancia: ${result.AmountWon:F2}");

            // Mostrar resultado y volver al menú
            NavigateTo(CashBattleState.Main);
        }

        #endregion

        #region Localization

        private void UpdateLocalizedTexts()
        {
            if (titleText != null)
                titleText.text = "Cash Battle";

        }

        private string GetLocalizedText(string key)
        {
            if (LocalizationManager.Instance != null)
                return LocalizationManager.Instance.GetText(key);
            return key;
        }

        #endregion

        #region KYC Verification

        private void CheckKYCVerification()
        {
            if (SettingsManager.IsCashBattleAuthBypassed())
            {
                Debug.Log("[CashBattle] Bypass de auth activo - saltando verificacion KYC");
                isKYCVerified = true;
                ShowMainPanel();
                return;
            }

            if (_kycService != null)
            {
                isKYCVerified = _kycService.CanAccessCashBattle;
            }

            if (!isKYCVerified)
            {
                Debug.Log("[CashBattle] Usuario no verificado KYC - volviendo al menu principal");
                SceneManager.LoadScene("MainMenu");
            }
            else
            {
                ShowMainPanel();
            }
        }

        private void ShowMainPanel()
        {
            if (mainPanel != null)
            {
                mainPanel.SetActive(true);
                AnimatePanelIn(mainPanel.transform);
            }

            Debug.Log("[CashBattle] Mostrando panel principal");
        }

        #endregion

        #region UI Updates

        private void UpdateUI()
        {
            UpdateBalanceDisplay();
        }

        private void UpdateBalanceDisplay()
        {
            if (balanceText != null)
            {
                balanceText.text = $"${CurrentBalance:F2}";
            }
        }

        private void UpdateMatchmakingTimer()
        {
            if (matchmakingTimerText != null)
            {
                int seconds = (int)matchmakingTimer;
                matchmakingTimerText.text = $"{seconds / 60:00}:{seconds % 60:00}";
            }
        }

        #endregion

        #region Navigation System

        private void NavigateTo(CashBattleState newState)
        {
            Debug.Log($"[CashBattle] Navegando de {currentState} a {newState}");

            HideCurrentPanel();
            currentState = newState;
            ShowCurrentPanel();
            UpdateHeaderForState();
        }

        private void HideCurrentPanel()
        {
            switch (currentState)
            {
                case CashBattleState.Main:
                    if (mainPanel != null) AnimatePanelOut(mainPanel.transform, () => mainPanel.SetActive(false));
                    break;
                case CashBattleState.GameSelection:
                    if (gameSelectionPanel != null) gameSelectionPanel.Hide();
                    break;
                case CashBattleState.TournamentList:
                    if (tournamentListPanel != null) tournamentListPanel.Hide();
                    break;
                case CashBattleState.Matchmaking:
                    if (matchmakingPanel != null) AnimatePanelOut(matchmakingPanel.transform, () => matchmakingPanel.SetActive(false));
                    break;
            }
        }

        private void ShowCurrentPanel()
        {
            switch (currentState)
            {
                case CashBattleState.Main:
                    if (mainPanel != null)
                    {
                        mainPanel.SetActive(true);
                        AnimatePanelIn(mainPanel.transform);
                    }
                    break;
                case CashBattleState.GameSelection:
                    if (gameSelectionPanel != null) gameSelectionPanel.Show();
                    break;
                case CashBattleState.TournamentList:
                    if (tournamentListPanel != null) tournamentListPanel.Show();
                    break;
                case CashBattleState.Matchmaking:
                    if (matchmakingPanel != null)
                    {
                        matchmakingPanel.SetActive(true);
                        AnimatePanelIn(matchmakingPanel.transform);
                    }
                    matchmakingTimer = 0f;
                    break;
            }
        }

        private void UpdateHeaderForState()
        {
            if (titleText == null) return;

            titleText.text = currentState switch
            {
                CashBattleState.Main => "Cash Battle",
                CashBattleState.GameSelection => L("cashbattle_battles_1v1"),
                CashBattleState.TournamentList => L("cashbattle_cash_tournaments"),
                CashBattleState.Matchmaking => L("matchmaking_searching"),
                _ => "Cash Battle"
            };
        }

        #endregion

        #region Navigation Callbacks

        private void OnBackClicked()
        {
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
            Debug.Log("[CashBattle] Navegando a escena CashBattle1v1");
            SceneNavigator.Instance?.NavigateTo(SceneNavigator.Scenes.CASH_BATTLE_1V1);
        }

        private void OnCashTournamentsClicked()
        {
            Debug.Log("[CashBattle] Navegando a escena CashTournaments");
            SceneNavigator.Instance?.NavigateTo(SceneNavigator.Scenes.CASH_TOURNAMENTS);
        }

        private void OnWalletClicked()
        {
            Debug.Log("[CashBattle] Navegando a escena Wallet");
            SceneNavigator.Instance?.NavigateTo(SceneNavigator.Scenes.CASH_WALLET);
        }

        private void OnHistoryClicked()
        {
            Debug.Log("[CashBattle] Navegando a escena History");
            SceneNavigator.Instance?.NavigateTo(SceneNavigator.Scenes.CASH_HISTORY);
        }

        #endregion

        #region Game Selection Handlers

        private void OnSingleGameSelected(GameType gameType, decimal entryFee)
        {
            Debug.Log($"[CashBattle] Juego seleccionado: {gameType}, Entry: ${entryFee}");

            if (_walletService == null || !_walletService.HasSufficientFunds(entryFee))
            {
                Debug.LogWarning("[CashBattle] Balance insuficiente o wallet no disponible");
                ShowInsufficientBalancePopup(entryFee);
                return;
            }

            _pendingGameType = ConvertToCashGameType(gameType);
            _pendingEntryFee = entryFee;
            _pendingSprintGames = null;
            ShowBetConfirmation(gameType.ToString(), entryFee);
        }

        private void OnCognitiveSprintSelected(List<GameType> games, decimal entryFee)
        {
            Debug.Log($"[CashBattle] Cognitive Sprint seleccionado: {games.Count} juegos, Entry: ${entryFee}");

            if (_walletService == null || !_walletService.HasSufficientFunds(entryFee))
            {
                Debug.LogWarning("[CashBattle] Balance insuficiente o wallet no disponible");
                ShowInsufficientBalancePopup(entryFee);
                return;
            }

            _pendingGameType = ConvertToCashGameType(games[0]);
            _pendingEntryFee = entryFee;
            _pendingSprintGames = games;
            ShowBetConfirmation("Cognitive Sprint", entryFee);
        }

        private void OnTournamentSelected(UITournamentInfo tournament)
        {
            Debug.Log($"[CashBattle] Torneo seleccionado: {tournament.Name}, Entry: ${tournament.EntryFee}");

            if (_walletService == null || !_walletService.HasSufficientFunds(tournament.EntryFee))
            {
                Debug.LogWarning("[CashBattle] Balance insuficiente o wallet no disponible");
                ShowInsufficientBalancePopup(tournament.EntryFee);
                return;
            }

            JoinTournamentAsync(tournament);
        }

        private CashGameType ConvertToCashGameType(GameType gameType)
        {
            return gameType switch
            {
                GameType.DigitRush => CashGameType.DigitRush,
                GameType.FlashTap => CashGameType.FlashTap,
                GameType.MemoryPairs => CashGameType.MemoryPairs,
                GameType.OddOneOut => CashGameType.OddOneOut,
                GameType.QuickMath => CashGameType.QuickMath,
                _ => CashGameType.DigitRush
            };
        }

        private void ShowBetConfirmation(string gameName, decimal entryFee)
        {
            if (confirmBetPanel == null)
            {
                // Sin panel de confirmación, ir directo a matchmaking
                OnConfirmBet();
                return;
            }

            if (confirmBetText != null)
            {
                confirmBetText.text = AutoLocalizer.Get("cashbattle_confirm_bet", gameName, entryFee.ToString("F2"));
            }

            confirmBetPanel.SetActive(true);
            AnimatePanelIn(confirmBetPanel.transform);
        }

        private void OnConfirmBet()
        {
            if (confirmBetPanel != null)
                AnimatePanelOut(confirmBetPanel.transform, () => confirmBetPanel.SetActive(false));

            StartMatchmakingAsync(_pendingGameType, _pendingEntryFee);
        }

        private void OnCancelBet()
        {
            if (confirmBetPanel != null)
                AnimatePanelOut(confirmBetPanel.transform, () => confirmBetPanel.SetActive(false));

            _pendingEntryFee = 0;
            _pendingSprintGames = null;
        }

        private void ShowInsufficientBalancePopup(decimal requiredAmount)
        {
            decimal currentBal = CurrentBalance;
            decimal needed = requiredAmount - currentBal;

            Debug.Log($"[CashBattle] Balance insuficiente. Necesita: ${requiredAmount:F2}, Tiene: ${currentBal:F2}");

            // Navegar a wallet para depositar
            SceneNavigator.Instance?.NavigateTo(SceneNavigator.Scenes.CASH_WALLET,
                new SceneNavigator.NavigationParams
                {
                    TargetTab = "Deposit",
                    ShowPopup = true
                });
        }

        #endregion

        #region Matchmaking

        private async void StartMatchmakingAsync(CashGameType gameType, decimal entryFee)
        {
            if (_walletService == null || _matchmakingService == null)
            {
                Debug.LogError("[CashBattle] Servicios no disponibles para matchmaking");
                return;
            }

            NavigateTo(CashBattleState.Matchmaking);

            if (matchmakingStatusText != null)
            {
                matchmakingStatusText.text = L("cashbattle_reserving_funds");
            }

            // Reservar fondos
            var reserveResult = await _walletService.ReserveFunds(entryFee, "pending_match");

            // Verificar que no se destruyó el objeto durante el await
            if (this == null) return;

            if (!reserveResult.Success)
            {
                Debug.LogError("[CashBattle] Error al reservar fondos");
                NavigateTo(CashBattleState.GameSelection);
                return;
            }

            if (matchmakingStatusText != null)
            {
                matchmakingStatusText.text = L("matchmaking_searching");
            }

            // Iniciar matchmaking
            var result = await _matchmakingService.FindMatch(gameType, entryFee);

            // Verificar que no se destruyó el objeto durante el await
            if (this == null) return;

            if (!result.Success)
            {
                Debug.LogError($"[CashBattle] Error en matchmaking: {result.Message}");
                await _walletService.ReleaseFunds("pending_match");
                NavigateTo(CashBattleState.GameSelection);
            }
            // Si es exitoso, OnMatchFound se encargará
        }

        private async void ConfirmAndLoadGame()
        {
            if (_matchmakingService == null || _matchmakingService.CurrentMatch == null) return;

            var result = await _matchmakingService.ConfirmMatch(_matchmakingService.CurrentMatch.MatchId);

            if (result.Success)
            {
                LoadGameScene();
            }
        }

        private void LoadGameScene()
        {
            var match = _matchmakingService.CurrentMatch;
            if (match == null) return;

            Debug.Log($"[CashBattle] Cargando juego: {match.GameType}");

            // Mapear CashGameType a GameType
            GameType gameType = match.GameType switch
            {
                CashGameType.DigitRush => GameType.DigitRush,
                CashGameType.FlashTap => GameType.FlashTap,
                CashGameType.MemoryPairs => GameType.MemoryPairs,
                CashGameType.OddOneOut => GameType.OddOneOut,
                CashGameType.QuickMath => GameType.QuickMath,
                _ => GameType.DigitRush
            };

            // Pasar contexto de Cash Battle al juego via GameSessionManager
            string opponentId = match.Opponent?.UserId ?? "";
            string opponentName = match.Opponent?.DisplayName ?? "Opponent";

            GameSessionManager.Instance.StartSingleGameSession(
                gameType,
                opponentId,
                opponentName,
                match.EntryFee,
                match.MatchId
            );
        }

        private async void CancelMatchmaking()
        {
            Debug.Log("[CashBattle] Cancelando matchmaking...");

            CancelInvoke(nameof(ConfirmAndLoadGame));

            if (_matchmakingService != null && _matchmakingService.IsSearching)
            {
                await _matchmakingService.CancelSearch();
            }

            if (_walletService != null)
                await _walletService.ReleaseFunds("pending_match");

            NavigateTo(CashBattleState.GameSelection);
        }

        #endregion

        #region Tournament

        private async void JoinTournamentAsync(UITournamentInfo tournament)
        {
            Debug.Log($"[CashBattle] Unirse a torneo: {tournament.Name}");

            // Usar el servicio de torneos
            var result = await _tournamentService.JoinTournament(tournament.Id);

            if (result.Success)
            {
                Debug.Log("[CashBattle] Inscripción exitosa");
                // TODO: Mostrar confirmación y navegar a lobby del torneo
            }
            else
            {
                Debug.LogError($"[CashBattle] Error al unirse: {result.Message}");
            }
        }

        #endregion

        #region Public API (Legacy Support)

        /// <summary>
        /// Called by Triumph SDK when balance updates (legacy)
        /// </summary>
        public void OnTriumphBalanceUpdated(float newBalance)
        {
            // El servicio de wallet maneja esto automáticamente ahora
            UpdateBalanceDisplay();
        }

        /// <summary>
        /// Opens deposit flow
        /// </summary>
        public void OpenDeposit()
        {
            SceneNavigator.Instance?.NavigateTo(SceneNavigator.Scenes.CASH_WALLET);
        }

        /// <summary>
        /// Opens withdraw flow
        /// </summary>
        public void OpenWithdraw()
        {
            SceneNavigator.Instance?.NavigateTo(SceneNavigator.Scenes.CASH_WALLET);
        }

        #endregion

        private string L(string key, params object[] args)
        {
            if (LocalizationManager.Instance == null) return key;
            string text = LocalizationManager.Instance.GetText(key);
            return args.Length > 0 ? string.Format(text, args) : text;
        }

        private void AnimatePanelIn(Transform t)
        {
            t.localScale = Vector3.one * 0.85f;
            var cg = t.GetComponent<CanvasGroup>();
            if (cg == null) cg = t.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            DG.Tweening.DOTween.Sequence()
                .Join(t.DOScale(1f, 0.3f).SetEase(Ease.OutBack))
                .Join(cg.DOFade(1f, 0.25f))
                .SetUpdate(true);
        }

        private void AnimatePanelOut(Transform t, System.Action onComplete)
        {
            var cg = t.GetComponent<CanvasGroup>();
            if (cg == null) { t.gameObject.SetActive(false); onComplete?.Invoke(); return; }
            DG.Tweening.DOTween.Sequence()
                .Join(t.DOScale(0.9f, 0.2f).SetEase(Ease.InQuad))
                .Join(cg.DOFade(0f, 0.2f))
                .OnComplete(() => { t.localScale = Vector3.one; cg.alpha = 1f; onComplete?.Invoke(); })
                .SetUpdate(true);
        }
    }

    /// <summary>
    /// States for CashBattle navigation (within this scene only)
    /// </summary>
    public enum CashBattleState
    {
        Main,
        GameSelection,
        TournamentList,
        Matchmaking
    }
}
