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
    /// Premium Matchmaking Manager
    /// - Professional VS screen with avatar integration
    /// - Dynamic game icon loading
    /// - Smooth animations and transitions
    /// - Firebase matchmaking integration
    /// </summary>
    public class MatchmakingManager : MonoBehaviour
    {
        [Header("=== HEADER ===")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Image gameIconImage;
        [SerializeField] private TextMeshProUGUI gameTypeText;

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

        [Header("=== EFFECTS ===")]
        [SerializeField] private Image screenFlash;

        [Header("=== SETTINGS ===")]
        #pragma warning disable 0414
        [SerializeField] private float countdownDuration = 3f;
        #pragma warning restore 0414
        [SerializeField] private float maxSearchTime = 120f;
        [SerializeField] private float spinnerSpeed = 150f;

        [Header("=== GAME ICONS ===")]
        [SerializeField] private Sprite digitRushIcon;
        [SerializeField] private Sprite memoryPairsIcon;
        [SerializeField] private Sprite quickMathIcon;
        [SerializeField] private Sprite flashTapIcon;
        [SerializeField] private Sprite oddOneOutIcon;
        [SerializeField] private Sprite cognitiveSprintIcon;

        // State
        private bool isSearching = false;
        private bool matchFound = false;
        private float searchTime = 0f;
        private GameType currentGameType;
        private bool isCognitiveSprint = false;
        private string matchId;
        private string opponentId;

        // Keys
        private const string MATCH_GAME_TYPE_KEY = "DigitPark_MatchGameType";
        private const string MATCH_IS_SPRINT_KEY = "DigitPark_MatchIsSprint";

        #region Unity Lifecycle

        private void Start()
        {
            Debug.Log("[Matchmaking] Premium Manager iniciado");

            SetupListeners();
            LoadMatchParameters();
            SetupPlayerInfo();
            SetupGameIcon();
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

            // Subscribe to avatar changes
            if (AvatarService.Instance != null)
            {
                AvatarService.Instance.OnAvatarChanged += OnPlayerAvatarChanged;
            }
        }

        private void LoadMatchParameters()
        {
            int gameTypeInt = PlayerPrefs.GetInt(MATCH_GAME_TYPE_KEY, 0);
            currentGameType = (GameType)gameTypeInt;
            isCognitiveSprint = PlayerPrefs.GetInt(MATCH_IS_SPRINT_KEY, 0) == 1;

            Debug.Log($"[Matchmaking] Game: {currentGameType}, IsSprint: {isCognitiveSprint}");
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
                        Debug.LogWarning($"[Matchmaking] Could not load player avatar: {e.Message}");
                        // Fallback: generar avatar con inicial
                        Sprite initialAvatar = AvatarInitialGenerator.GenerateAvatar(playerName, playerId);
                        playerAvatar.sprite = initialAvatar;
                        playerAvatar.color = Color.white;
                    }
                }
                else
                {
                    // Sin AvatarService: generar avatar con inicial
                    Sprite initialAvatar = AvatarInitialGenerator.GenerateAvatar(playerName, playerId);
                    playerAvatar.sprite = initialAvatar;
                    playerAvatar.color = Color.white;
                }
            }

            // Setup opponent as searching
            ShowOpponentSearching(true);
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

                    // Hide placeholder text if we have an icon
                    var placeholder = gameIconImage.transform.Find("Placeholder");
                    if (placeholder != null)
                        placeholder.gameObject.SetActive(false);
                }
                else
                {
                    // 3. No icon found - show placeholder with game initials
                    gameIconImage.color = new Color(0.1f, 0.15f, 0.25f, 1f); // Dark background
                    ShowIconPlaceholder(gameName);
                }
            }

            // Always show game name text as additional context
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

            // Find or create placeholder text
            Transform placeholder = gameIconImage.transform.Find("Placeholder");
            TextMeshProUGUI placeholderText = null;

            if (placeholder != null)
            {
                placeholderText = placeholder.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                // Create placeholder dynamically
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
                // Get initials from game name (e.g., "DIGIT RUSH" -> "DR")
                string initials = GetInitials(gameName);
                placeholderText.text = initials;
                placeholderText.color = new Color(0f, 0.96f, 1f); // Neon cyan
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

            // Try loading from Resources folder
            Sprite sprite = Resources.Load<Sprite>($"Icons/Games/{iconName}");

            if (sprite != null)
            {
                Debug.Log($"[Matchmaking] Loaded icon from Resources: {iconName}");
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

        private void StartMatchmaking()
        {
            if (MatchmakingService.Instance != null)
            {
                if (isCognitiveSprint)
                {
                    MatchmakingService.Instance.FindCognitiveSprintMatch(
                        CognitiveSprintManager.Instance?.SelectedGames,
                        OnMatchFound,
                        OnMatchFailed
                    );
                }
                else
                {
                    MatchmakingService.Instance.FindMatch(
                        currentGameType,
                        false,
                        OnMatchFound,
                        OnMatchFailed
                    );
                }
            }
            else
            {
                Debug.LogWarning("[Matchmaking] MatchmakingService not found. Using mock delay.");
                // Mock: simulate finding opponent after delay
                StartCoroutine(MockMatchmaking());
            }
        }

        private IEnumerator MockMatchmaking()
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(2f, 5f));

            if (isSearching)
            {
                OnMatchFound("mock_match_123", "Opponent");
            }
        }

        private void AnimateSpinners()
        {
            // Rotate main search spinner
            if (searchingRing != null)
            {
                searchingRing.transform.Rotate(0, 0, -spinnerSpeed * Time.deltaTime);
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

            Debug.Log("[Matchmaking] Search timed out");
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

            Debug.Log($"[Matchmaking] Match found! ID: {matchId}, Opponent: {opponentInfo}");

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
            if (opponentNameText != null)
                opponentNameText.text = searching ? "???" : "";

            if (opponentLevelText != null)
                opponentLevelText.text = searching ? "---" : "";

            // Hide avatar placeholder when searching
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
                        // Intentar cargar avatar real del oponente desde Firebase
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
                        Debug.LogWarning($"[Matchmaking] Could not load opponent avatar: {e.Message}");
                        // Fallback: generar avatar con inicial del oponente
                        Sprite initialAvatar = AvatarInitialGenerator.GenerateAvatar(opponentInfo, opponentId);
                        opponentAvatar.sprite = initialAvatar;
                        opponentAvatar.color = Color.white;
                    }
                }
                else
                {
                    // Sin AvatarService o sin opponentId: generar avatar con inicial
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

            for (int i = 3; i > 0; i--)
            {
                if (countdownText != null)
                {
                    countdownText.text = i.ToString();
                    countdownText.transform.localScale = Vector3.one * 1.5f;
                    StartCoroutine(AnimateScale(countdownText.transform, Vector3.one, 0.3f));
                }

                // Play sound
                // AudioManager.Instance?.PlaySFX("Countdown");

                yield return new WaitForSeconds(1f);
            }

            if (countdownText != null)
            {
                countdownText.text = AutoLocalizer.Get("matchmaking_go");
                countdownText.color = new Color(0.2353f, 1f, 0.4196f); // Neon green
            }

            if (getReadyText != null)
                getReadyText.text = "";

            yield return new WaitForSeconds(0.5f);

            StartOnlineGame();
        }

        private void StartOnlineGame()
        {
            Debug.Log($"[Matchmaking] Starting online game: {currentGameType}");

            PlayerPrefs.SetString("CurrentMatchId", matchId);
            PlayerPrefs.SetString("CurrentOpponentId", opponentId);
            PlayerPrefs.SetInt("IsOnlineMatch", 1);
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
                _ => "GameSelector"
            };
        }

        #endregion

        #region Match Failed

        private void OnMatchFailed(string error)
        {
            isSearching = false;

            Debug.LogError($"[Matchmaking] Match failed: {error}");

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

            if (MatchmakingService.Instance != null)
                MatchmakingService.Instance.CancelMatchmaking();

            SceneManager.LoadScene("GameSelector");
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
        /// Sets the game type before loading matchmaking scene
        /// </summary>
        public static void SetMatchGameType(GameType gameType, bool isCognitiveSprint = false)
        {
            PlayerPrefs.SetInt(MATCH_GAME_TYPE_KEY, (int)gameType);
            PlayerPrefs.SetInt(MATCH_IS_SPRINT_KEY, isCognitiveSprint ? 1 : 0);
            PlayerPrefs.Save();
            Debug.Log($"[Matchmaking] Set game type: {gameType}, IsSprint: {isCognitiveSprint}");
        }

        #endregion
    }
}
