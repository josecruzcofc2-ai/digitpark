using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DigitPark.Games;
using DigitPark.Services;
using DigitPark.UI.Components;
using DigitPark.Localization;
using DigitPark.UI;

namespace DigitPark.Managers
{
    /// <summary>
    /// Cash Matchmaking Manager - Gold Premium Theme
    /// - Professional VS screen with avatar integration
    /// - Entry fee display ($X.XX format)
    /// - ServiceLocator matchmaking (with mock fallback)
    /// - Dynamic game icon loading
    /// - Smooth animations and transitions
    /// - Gold theme UI updates
    /// </summary>
    public class CashMatchmakingManager : MonoBehaviour
    {
        [Header("=== HEADER ===")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Image gameIconImage;
        [SerializeField] private TextMeshProUGUI gameTypeText;

        [Header("=== ENTRY FEE ===")]
        [SerializeField] private TextMeshProUGUI entryFeeText;

        [Header("=== PLAYER CARD ===")]
        [SerializeField] private Image playerAvatar;
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI playerLevelText;
        [SerializeField] private GameObject playerCard;

        [Header("=== OPPONENT CARD ===")]
        [SerializeField] private Image opponentAvatar;
        [SerializeField] private TextMeshProUGUI opponentNameText;
        [SerializeField] private TextMeshProUGUI opponentLevelText;
        [SerializeField] private GameObject opponentCard;
        [SerializeField] private GameObject opponentSearchingIndicator;
        [SerializeField] private Image opponentSearchRing;

        [Header("=== VS SECTION ===")]
        [SerializeField] private GameObject vsContainer;
        [SerializeField] private TextMeshProUGUI vsText;

        [Header("=== SEARCH STATUS ===")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private GameObject searchingSpinner;
        [SerializeField] private Image searchingRing;

        [Header("=== COUNTDOWN ===")]
        [SerializeField] private GameObject countdownPanel;
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private TextMeshProUGUI getReadyText;

        [Header("=== BUTTONS ===")]
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button backButton;

        [Header("=== EFFECTS ===")]
        [SerializeField] private Image screenFlash;

        [Header("=== SETTINGS ===")]
        [SerializeField] private float countdownDuration = 3f;
        [SerializeField] private float maxSearchTime = 120f;
        [SerializeField] private float spinnerSpeed = 150f;
        [SerializeField] private float opponentRingSpeed = 100f;

        [Header("=== GAME ICONS ===")]
        [SerializeField] private Sprite digitRushIcon;
        [SerializeField] private Sprite memoryPairsIcon;
        [SerializeField] private Sprite quickMathIcon;
        [SerializeField] private Sprite flashTapIcon;
        [SerializeField] private Sprite oddOneOutIcon;
        [SerializeField] private Sprite cognitiveSprintIcon;

        // Gold theme colors for runtime UI updates
        private static readonly Color GOLD_PRIMARY = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color TEXT_GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GREEN_GO = new Color(0.24f, 1f, 0.42f, 1f);

        // State
        private bool isSearching = false;
        private bool matchFound = false;
        private float searchTime = 0f;
        private GameType currentGameType;
        private bool isCognitiveSprint = false;
        private decimal entryFee = 0m;
        private string matchId;
        private string opponentId;

        // PlayerPrefs Keys
        private const string CASH_MATCH_GAME_TYPE_KEY = "DigitPark_CashMatchGameType";
        private const string CASH_MATCH_IS_SPRINT_KEY = "DigitPark_CashMatchIsSprint";
        private const string CASH_MATCH_ENTRY_FEE_KEY = "DigitPark_CashMatchEntryFee";

        #region Unity Lifecycle

        private void Start()
        {
            Debug.Log("[CashMatchmaking] Gold Premium Manager iniciado");

            SetupListeners();
            LoadMatchParameters();
            SetupPlayerInfo();
            SetupGameIcon();
            UpdateEntryFeeDisplay();
            StartSearching();
        }

        private void Update()
        {
            if (isSearching)
            {
                searchTime += Time.deltaTime;
                UpdateTimerDisplay();
                AnimateSpinners();

                if (searchTime >= maxSearchTime)
                {
                    OnSearchTimeout();
                }
            }
        }

        private void OnDestroy()
        {
            // Cleanup
            if (AvatarService.Instance != null)
            {
                AvatarService.Instance.OnAvatarChanged -= OnPlayerAvatarChanged;
            }
        }

        #endregion

        #region Setup

        private void SetupListeners()
        {
            cancelButton?.onClick.AddListener(OnCancelClicked);
            // Disable auto-navigation from BackButton prefab to prevent double listener
            var autoNav = backButton?.GetComponent<DigitPark.UI.BackButton>();
            if (autoNav != null) autoNav.DisableAutoNavigation();
            backButton?.onClick.AddListener(OnCancelClicked);

            // Subscribe to avatar changes
            if (AvatarService.Instance != null)
            {
                AvatarService.Instance.OnAvatarChanged += OnPlayerAvatarChanged;
            }
        }

        private void LoadMatchParameters()
        {
            int gameTypeInt = PlayerPrefs.GetInt(CASH_MATCH_GAME_TYPE_KEY, 0);
            currentGameType = (GameType)gameTypeInt;
            isCognitiveSprint = PlayerPrefs.GetInt(CASH_MATCH_IS_SPRINT_KEY, 0) == 1;

            // Read entry fee from PlayerPrefs (stored as string for decimal precision)
            string feeStr = PlayerPrefs.GetString(CASH_MATCH_ENTRY_FEE_KEY, "0");
            if (!decimal.TryParse(feeStr, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out entryFee))
            {
                entryFee = 0m;
            }

            Debug.Log($"[CashMatchmaking] Game: {currentGameType}, IsSprint: {isCognitiveSprint}, EntryFee: ${entryFee:F2}");
        }

        private async void SetupPlayerInfo()
        {
            // Get player name
            string playerName = PlayerPrefs.GetString("PlayerName", "Player");
            string playerId = PlayerPrefs.GetString("PlayerId", "");
            if (playerNameText != null)
                playerNameText.text = playerName;

            // Get player level
            int level = PlayerPrefs.GetInt("PlayerLevel", 1);
            if (playerLevelText != null)
                playerLevelText.text = $"Lv. {level}";

            // Load player avatar
            if (playerAvatar != null)
            {
                if (AvatarService.Instance != null)
                {
                    try
                    {
                        Sprite avatarSprite = await AvatarService.Instance.LoadCurrentUserAvatar();
                        if (avatarSprite != null)
                        {
                            playerAvatar.sprite = avatarSprite;
                            playerAvatar.color = Color.white;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[CashMatchmaking] Could not load player avatar: {e.Message}");
                        Sprite initialAvatar = AvatarInitialGenerator.GenerateAvatar(playerName, playerId);
                        playerAvatar.sprite = initialAvatar;
                        playerAvatar.color = Color.white;
                    }
                }
                else
                {
                    Sprite initialAvatar = AvatarInitialGenerator.GenerateAvatar(playerName, playerId);
                    playerAvatar.sprite = initialAvatar;
                    playerAvatar.color = Color.white;
                }
            }

            // Setup opponent as searching
            ShowOpponentSearching(true);
        }

        private void UpdateEntryFeeDisplay()
        {
            if (entryFeeText != null)
            {
                entryFeeText.text = AutoLocalizer.Get("cash_matchmaking_entry_fee", $"${entryFee:F2}");
            }
        }

        private void SetupGameIcon()
        {
            string gameName = isCognitiveSprint ? AutoLocalizer.Get("matchmaking_cognitive_sprint") : FormatGameName(currentGameType);

            // 1. Try to get assigned icon first
            Sprite icon = GetGameIcon(currentGameType);

            // 2. If no icon assigned, try to load from Resources
            if (icon == null)
            {
                icon = LoadGameIconFromResources(currentGameType);
            }

            // Setup game icon image
            if (gameIconImage != null)
            {
                if (icon != null)
                {
                    gameIconImage.sprite = icon;
                    gameIconImage.color = Color.white;

                    var placeholder = gameIconImage.transform.Find("Placeholder");
                    if (placeholder != null)
                        placeholder.gameObject.SetActive(false);
                }
                else
                {
                    gameIconImage.color = new Color(0.12f, 0.1f, 0.15f, 1f);
                    ShowIconPlaceholder(gameName);
                }
            }

            // Always show game name text
            if (gameTypeText != null)
            {
                gameTypeText.gameObject.SetActive(true);
                gameTypeText.text = gameName;
            }
        }

        private string FormatGameName(GameType gameType)
        {
            return gameType switch
            {
                GameType.DigitRush => AutoLocalizer.Get("game_name_digitrush"),
                GameType.MemoryPairs => AutoLocalizer.Get("game_name_memorypairs"),
                GameType.QuickMath => AutoLocalizer.Get("game_name_quickmath"),
                GameType.FlashTap => AutoLocalizer.Get("game_name_flashtap"),
                GameType.OddOneOut => AutoLocalizer.Get("game_name_oddoneout"),
                _ => gameType.ToString().ToUpper()
            };
        }

        private void ShowIconPlaceholder(string gameName)
        {
            if (gameIconImage == null) return;

            Transform placeholder = gameIconImage.transform.Find("Placeholder");
            TextMeshProUGUI placeholderText = null;

            if (placeholder != null)
            {
                placeholderText = placeholder.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                GameObject placeholderGO = new GameObject("Placeholder");
                placeholderGO.transform.SetParent(gameIconImage.transform, false);

                RectTransform rt = placeholderGO.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                placeholderText = placeholderGO.AddComponent<TextMeshProUGUI>();
                placeholderText.alignment = TextAlignmentOptions.Center;
                placeholderText.fontSize = FontSizes.Body;
                placeholderText.fontStyle = FontStyles.Bold;
            }

            if (placeholderText != null)
            {
                string initials = GetInitials(gameName);
                placeholderText.text = initials;
                placeholderText.color = GOLD_PRIMARY;
                placeholderText.gameObject.SetActive(true);
            }
        }

        private string GetInitials(string text)
        {
            if (string.IsNullOrEmpty(text)) return "?";

            string[] words = text.Split(' ');
            string initials = "";
            foreach (var word in words)
            {
                if (!string.IsNullOrEmpty(word))
                    initials += word[0];
            }
            return initials.Length > 0 ? initials : "?";
        }

        private Sprite LoadGameIconFromResources(GameType gameType)
        {
            string iconName = gameType switch
            {
                GameType.DigitRush => "DigitRushIcon",
                GameType.MemoryPairs => "MemoryPairsIcon",
                GameType.QuickMath => "QuickMathIcon",
                GameType.FlashTap => "FlashTapIcon",
                GameType.OddOneOut => "OddOneOutIcon",
                _ => null
            };

            if (isCognitiveSprint)
                iconName = "CognitiveSprintIcon";

            if (string.IsNullOrEmpty(iconName))
                return null;

            Sprite sprite = Resources.Load<Sprite>($"Icons/Games/{iconName}");

            if (sprite != null)
            {
                Debug.Log($"[CashMatchmaking] Loaded icon from Resources: {iconName}");
            }

            return sprite;
        }

        private Sprite GetGameIcon(GameType gameType)
        {
            if (isCognitiveSprint && cognitiveSprintIcon != null)
                return cognitiveSprintIcon;

            return gameType switch
            {
                GameType.DigitRush => digitRushIcon,
                GameType.MemoryPairs => memoryPairsIcon,
                GameType.QuickMath => quickMathIcon,
                GameType.FlashTap => flashTapIcon,
                GameType.OddOneOut => oddOneOutIcon,
                _ => null
            };
        }

        #endregion

        #region Searching

        private void StartSearching()
        {
            isSearching = true;
            matchFound = false;
            searchTime = 0f;

            // Update UI
            if (titleText != null)
                titleText.text = AutoLocalizer.Get("matchmaking_searching_title");

            if (statusText != null)
                statusText.text = AutoLocalizer.Get("matchmaking_looking");

            if (searchingSpinner != null)
                searchingSpinner.SetActive(true);

            if (countdownPanel != null)
                countdownPanel.SetActive(false);

            if (vsContainer != null)
                vsContainer.SetActive(false);

            // Start matchmaking
            StartMatchmaking();
        }

        private async void StartMatchmaking()
        {
            // Use ServiceLocator pattern for Cash Battle matchmaking
            if (ServiceLocator.Matchmaking != null)
            {
                try
                {
                    // Convert GameType to CashGameType
                    CashGameType cashGameType = ConvertToCashGameType(currentGameType);

                    var result = await ServiceLocator.Matchmaking.FindMatch(cashGameType, entryFee);

                    if (result.Success && result.Match != null)
                    {
                        OnMatchFound(
                            result.Match.MatchId,
                            result.Match.Opponent?.DisplayName ?? "Opponent"
                        );
                    }
                    else
                    {
                        OnMatchFailed(result.Message ?? AutoLocalizer.Get("cash_matchmaking_error"));
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[CashMatchmaking] ServiceLocator matchmaking error: {e.Message}");
                    OnMatchFailed(e.Message);
                }
            }
            else
            {
                Debug.LogWarning("[CashMatchmaking] ServiceLocator.Matchmaking not available. Using mock delay.");
                StartCoroutine(MockMatchmaking());
            }
        }

        private CashGameType ConvertToCashGameType(GameType gameType)
        {
            return gameType switch
            {
                GameType.DigitRush => CashGameType.DigitRush,
                GameType.MemoryPairs => CashGameType.MemoryPairs,
                GameType.QuickMath => CashGameType.QuickMath,
                GameType.FlashTap => CashGameType.FlashTap,
                GameType.OddOneOut => CashGameType.OddOneOut,
                _ => CashGameType.DigitRush
            };
        }

        private IEnumerator MockMatchmaking()
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(2f, 5f));

            if (isSearching)
            {
                OnMatchFound("cash_mock_match_123", "Opponent");
            }
        }

        private void AnimateSpinners()
        {
            // Rotate main search spinner
            if (searchingRing != null)
            {
                searchingRing.transform.Rotate(0, 0, -spinnerSpeed * Time.deltaTime);
            }

            // Rotate opponent search ring
            if (opponentSearchRing != null && opponentSearchingIndicator != null && opponentSearchingIndicator.activeSelf)
            {
                opponentSearchRing.transform.Rotate(0, 0, -opponentRingSpeed * Time.deltaTime);
            }
        }

        private void UpdateTimerDisplay()
        {
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(searchTime / 60f);
                int seconds = Mathf.FloorToInt(searchTime % 60f);
                timerText.text = $"{minutes}:{seconds:00}";
            }
        }

        private void OnSearchTimeout()
        {
            isSearching = false;

            if (titleText != null)
                titleText.text = AutoLocalizer.Get("matchmaking_no_match");

            if (statusText != null)
                statusText.text = AutoLocalizer.Get("matchmaking_timeout");

            if (searchingSpinner != null)
                searchingSpinner.SetActive(false);

            Debug.Log("[CashMatchmaking] Search timed out");
        }

        #endregion

        #region Match Found

        private void OnMatchFound(string matchId, string opponentInfo)
        {
            if (matchFound) return;

            this.matchId = matchId;
            this.opponentId = opponentInfo;
            isSearching = false;
            matchFound = true;

            Debug.Log($"[CashMatchmaking] Match found! ID: {matchId}, Opponent: {opponentInfo}");

            // Update UI
            if (searchingSpinner != null)
                searchingSpinner.SetActive(false);

            if (titleText != null)
                titleText.text = AutoLocalizer.Get("matchmaking_match_found");

            if (statusText != null)
                statusText.text = "";

            // Show opponent
            ShowOpponentInfo(opponentInfo);

            // Start match sequence
            StartCoroutine(MatchFoundSequence());
        }

        private void ShowOpponentSearching(bool searching)
        {
            if (opponentSearchingIndicator != null)
                opponentSearchingIndicator.SetActive(searching);

            if (opponentNameText != null)
                opponentNameText.text = searching ? "???" : "";

            if (opponentLevelText != null)
                opponentLevelText.text = searching ? "---" : "";

            // Dim avatar when searching
            if (opponentAvatar != null)
            {
                opponentAvatar.color = searching ? new Color(1, 1, 1, 0.3f) : Color.white;
            }
        }

        private async void ShowOpponentInfo(string opponentInfo)
        {
            ShowOpponentSearching(false);

            // Parse opponent name
            if (opponentNameText != null)
                opponentNameText.text = opponentInfo;

            // Mock opponent level
            if (opponentLevelText != null)
                opponentLevelText.text = $"Lv. {UnityEngine.Random.Range(1, 50)}";

            // Load opponent avatar
            if (opponentAvatar != null)
            {
                if (AvatarService.Instance != null && !string.IsNullOrEmpty(opponentId))
                {
                    try
                    {
                        Sprite avatarSprite = await AvatarService.Instance.LoadAvatar(
                            opponentId, "", opponentInfo);
                        if (avatarSprite != null)
                        {
                            opponentAvatar.sprite = avatarSprite;
                            opponentAvatar.color = Color.white;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[CashMatchmaking] Could not load opponent avatar: {e.Message}");
                        Sprite initialAvatar = AvatarInitialGenerator.GenerateAvatar(opponentInfo, opponentId);
                        opponentAvatar.sprite = initialAvatar;
                        opponentAvatar.color = Color.white;
                    }
                }
                else
                {
                    Sprite initialAvatar = AvatarInitialGenerator.GenerateAvatar(
                        opponentInfo, opponentId ?? opponentInfo);
                    opponentAvatar.sprite = initialAvatar;
                    opponentAvatar.color = Color.white;
                }
            }

            // Animate opponent card
            if (opponentCard != null)
            {
                opponentCard.transform.localScale = Vector3.zero;
                StartCoroutine(AnimateScale(opponentCard.transform, Vector3.one, 0.4f));
            }
        }

        private IEnumerator MatchFoundSequence()
        {
            // Flash effect
            if (screenFlash != null)
            {
                screenFlash.gameObject.SetActive(true);
                yield return StartCoroutine(FlashScreen());
            }

            yield return new WaitForSeconds(0.5f);

            // Show VS
            if (vsContainer != null)
            {
                vsContainer.SetActive(true);
                vsContainer.transform.localScale = Vector3.zero;
                yield return StartCoroutine(AnimateVSPop());
            }

            // Vibrate on mobile
            #if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
            #endif

            yield return new WaitForSeconds(1f);

            // Start countdown
            StartCoroutine(CountdownSequence());
        }

        private IEnumerator FlashScreen()
        {
            Color flashColor = new Color(1, 1, 1, 0);
            screenFlash.color = flashColor;

            // Flash in
            float elapsed = 0f;
            while (elapsed < 0.1f)
            {
                elapsed += Time.deltaTime;
                flashColor.a = Mathf.Lerp(0, 0.6f, elapsed / 0.1f);
                screenFlash.color = flashColor;
                yield return null;
            }

            // Flash out
            elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                flashColor.a = Mathf.Lerp(0.6f, 0, elapsed / 0.3f);
                screenFlash.color = flashColor;
                yield return null;
            }

            screenFlash.gameObject.SetActive(false);
        }

        private IEnumerator AnimateVSPop()
        {
            Transform vs = vsContainer.transform;

            // Pop in with overshoot
            float elapsed = 0f;
            float duration = 0.3f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float scale = EaseOutBack(t) * 1.2f;
                vs.localScale = Vector3.one * scale;
                yield return null;
            }

            // Settle
            elapsed = 0f;
            duration = 0.15f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float scale = Mathf.Lerp(1.2f, 1f, t);
                vs.localScale = Vector3.one * scale;
                yield return null;
            }

            vs.localScale = Vector3.one;

            // Pulse effect
            StartCoroutine(PulseVS());
        }

        private IEnumerator PulseVS()
        {
            if (vsText == null) yield break;

            for (int i = 0; i < 2; i++)
            {
                yield return StartCoroutine(AnimateScale(vsText.transform, Vector3.one * 1.15f, 0.1f));
                yield return StartCoroutine(AnimateScale(vsText.transform, Vector3.one, 0.1f));
            }
        }

        private IEnumerator CountdownSequence()
        {
            if (countdownPanel != null)
                countdownPanel.SetActive(true);

            if (getReadyText != null)
                getReadyText.text = AutoLocalizer.Get("matchmaking_get_ready");

            for (int i = (int)countdownDuration; i > 0; i--)
            {
                if (countdownText != null)
                {
                    countdownText.text = i.ToString();
                    countdownText.color = GOLD_PRIMARY;
                    countdownText.transform.localScale = Vector3.one * 1.5f;
                    StartCoroutine(AnimateScale(countdownText.transform, Vector3.one, 0.3f));
                }

                yield return new WaitForSeconds(1f);
            }

            if (countdownText != null)
            {
                countdownText.text = AutoLocalizer.Get("matchmaking_go");
                countdownText.color = GREEN_GO;
            }

            if (getReadyText != null)
                getReadyText.text = "";

            yield return new WaitForSeconds(0.5f);

            StartOnlineGame();
        }

        private void StartOnlineGame()
        {
            Debug.Log($"[CashMatchmaking] Starting cash game: {currentGameType}, EntryFee: ${entryFee:F2}");

            PlayerPrefs.SetString("CurrentMatchId", matchId);
            PlayerPrefs.SetString("CurrentOpponentId", opponentId);
            PlayerPrefs.SetInt("IsOnlineMatch", 1);
            PlayerPrefs.SetInt("IsCashMatch", 1);
            PlayerPrefs.SetString(CASH_MATCH_ENTRY_FEE_KEY, entryFee.ToString(System.Globalization.CultureInfo.InvariantCulture));
            PlayerPrefs.SetInt(CASH_MATCH_GAME_TYPE_KEY, (int)currentGameType);
            PlayerPrefs.Save();

            if (GameSessionManager.Instance != null)
            {
                GameSessionManager.Instance.StartOnlineMatch(matchId, opponentId);
            }

            if (isCognitiveSprint)
            {
                CognitiveSprintManager.Instance?.StartOnlineSprint();
            }
            else
            {
                string sceneName = GetSceneNameForGameType(currentGameType);
                SceneManager.LoadScene(sceneName);
            }
        }

        private string GetSceneNameForGameType(GameType gameType)
        {
            return gameType switch
            {
                GameType.DigitRush => "DigitRush",
                GameType.MemoryPairs => "MemoryPairs",
                GameType.QuickMath => "QuickMath",
                GameType.FlashTap => "FlashTap",
                GameType.OddOneOut => "OddOneOut",
                _ => "CashBattle1v1"
            };
        }

        #endregion

        #region Match Failed

        private void OnMatchFailed(string error)
        {
            isSearching = false;

            Debug.LogError($"[CashMatchmaking] Match failed: {error}");

            if (titleText != null)
                titleText.text = AutoLocalizer.Get("matchmaking_connection_error");

            if (statusText != null)
                statusText.text = error;

            if (searchingSpinner != null)
                searchingSpinner.SetActive(false);
        }

        #endregion

        #region Navigation

        private void OnCancelClicked()
        {
            CancelAndGoBack();
        }

        private void CancelAndGoBack()
        {
            isSearching = false;

            // Cancel via ServiceLocator if available
            if (ServiceLocator.Matchmaking != null)
            {
                _ = ServiceLocator.Matchmaking.CancelSearch();
            }

            SceneManager.LoadScene("CashBattle1v1");
        }

        #endregion

        #region Avatar Events

        private void OnPlayerAvatarChanged(Sprite newAvatar)
        {
            if (playerAvatar != null && newAvatar != null)
            {
                playerAvatar.sprite = newAvatar;
            }
        }

        #endregion

        #region Animation Helpers

        private IEnumerator AnimateScale(Transform target, Vector3 targetScale, float duration)
        {
            if (target == null) yield break;

            Vector3 startScale = target.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = t * t * (3f - 2f * t); // Smoothstep
                target.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            target.localScale = targetScale;
        }

        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1;
            return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
        }

        #endregion

        #region Static API

        /// <summary>
        /// Sets the game type before loading the cash matchmaking scene
        /// </summary>
        public static void SetMatchGameType(GameType gameType, bool isCognitiveSprint = false)
        {
            PlayerPrefs.SetInt(CASH_MATCH_GAME_TYPE_KEY, (int)gameType);
            PlayerPrefs.SetInt(CASH_MATCH_IS_SPRINT_KEY, isCognitiveSprint ? 1 : 0);
            PlayerPrefs.Save();
            Debug.Log($"[CashMatchmaking] Set game type: {gameType}, IsSprint: {isCognitiveSprint}");
        }

        /// <summary>
        /// Sets the entry fee before loading the cash matchmaking scene
        /// </summary>
        public static void SetEntryFee(decimal fee)
        {
            PlayerPrefs.SetString(CASH_MATCH_ENTRY_FEE_KEY, fee.ToString(System.Globalization.CultureInfo.InvariantCulture));
            PlayerPrefs.Save();
            Debug.Log($"[CashMatchmaking] Set entry fee: ${fee:F2}");
        }

        #endregion
    }
}
