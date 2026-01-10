using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DigitPark.Games;
using DigitPark.Services;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager para la escena de Matchmaking
    /// Muestra búsqueda de oponente, VS screen, y countdown
    /// </summary>
    public class MatchmakingManager : MonoBehaviour
    {
        [Header("UI - Header")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI gameTypeText;

        [Header("UI - Player Info")]
        [SerializeField] private Image playerAvatar;
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private GameObject playerCard;

        [Header("UI - Opponent Info")]
        [SerializeField] private Image opponentAvatar;
        [SerializeField] private TextMeshProUGUI opponentNameText;
        [SerializeField] private GameObject opponentCard;
        [SerializeField] private GameObject opponentSearchingIndicator;

        [Header("UI - VS Section")]
        [SerializeField] private GameObject vsContainer;
        [SerializeField] private TextMeshProUGUI vsText;

        [Header("UI - Status")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI timerText;

        [Header("UI - Searching Animation")]
        [SerializeField] private GameObject searchingSpinner;
        [SerializeField] private Image searchingRing;

        [Header("UI - Countdown")]
        [SerializeField] private GameObject countdownPanel;
        [SerializeField] private TextMeshProUGUI countdownText;

        [Header("UI - Buttons")]
        [SerializeField] private Button cancelButton;

        [Header("Settings")]
        [SerializeField] private float countdownDuration = 3f;
        [SerializeField] private float maxSearchTime = 120f; // 2 minutes max search
        [SerializeField] private float spinnerSpeed = 100f;

        // State
        private bool isSearching = false;
        private bool matchFound = false;
        private float searchTime = 0f;
        private GameType currentGameType;
        private bool isCognitiveSprint = false;
        private string matchId;
        private string opponentId;

        // Keys for passing data between scenes
        private const string MATCH_GAME_TYPE_KEY = "DigitPark_MatchGameType";
        private const string MATCH_IS_SPRINT_KEY = "DigitPark_MatchIsSprint";

        private void Start()
        {
            Debug.Log("[Matchmaking] Manager iniciado");

            SetupListeners();
            LoadMatchParameters();
            SetupPlayerInfo();
            StartSearching();
        }

        private void Update()
        {
            if (isSearching)
            {
                // Update search timer
                searchTime += Time.deltaTime;
                UpdateTimerDisplay();

                // Rotate spinner
                if (searchingRing != null)
                {
                    searchingRing.transform.Rotate(0, 0, -spinnerSpeed * Time.deltaTime);
                }

                // Check timeout
                if (searchTime >= maxSearchTime)
                {
                    OnSearchTimeout();
                }
            }
        }

        private void SetupListeners()
        {
            cancelButton?.onClick.AddListener(OnCancelClicked);
        }

        private void LoadMatchParameters()
        {
            // Load game type from PlayerPrefs (set by GameSelectorManager)
            int gameTypeInt = PlayerPrefs.GetInt(MATCH_GAME_TYPE_KEY, 0);
            currentGameType = (GameType)gameTypeInt;
            isCognitiveSprint = PlayerPrefs.GetInt(MATCH_IS_SPRINT_KEY, 0) == 1;

            // Update UI
            if (gameTypeText != null)
            {
                if (isCognitiveSprint)
                    gameTypeText.text = "COGNITIVE SPRINT";
                else
                    gameTypeText.text = currentGameType.ToString().ToUpper();
            }

            Debug.Log($"[Matchmaking] Game: {currentGameType}, IsSprint: {isCognitiveSprint}");
        }

        private void SetupPlayerInfo()
        {
            // Get player info from ProfileManager or Firebase
            string playerName = PlayerPrefs.GetString("PlayerName", "Player");

            if (playerNameText != null)
                playerNameText.text = playerName;

            // Hide opponent info initially
            ShowOpponentSearching(true);
        }

        #region Searching

        private void StartSearching()
        {
            isSearching = true;
            matchFound = false;
            searchTime = 0f;

            // Update UI
            if (titleText != null)
                titleText.text = "SEARCHING...";

            if (statusText != null)
                statusText.text = "Looking for opponent";

            if (searchingSpinner != null)
                searchingSpinner.SetActive(true);

            if (countdownPanel != null)
                countdownPanel.SetActive(false);

            // Start matchmaking service with Firebase
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
                        false, // not a cash match
                        OnMatchFound,
                        OnMatchFailed
                    );
                }
            }
            else
            {
                Debug.LogError("[Matchmaking] MatchmakingService not found! Make sure it exists in the scene.");
                OnMatchFailed("Matchmaking service not available");
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

            if (statusText != null)
                statusText.text = "No opponents found. Try again later.";

            if (searchingSpinner != null)
                searchingSpinner.SetActive(false);

            // Show retry or back options
            Debug.Log("[Matchmaking] Search timed out");
        }

        #endregion

        #region Match Found

        private void OnMatchFound(string matchId, string opponentInfo)
        {
            if (matchFound) return; // Prevent double calls

            this.matchId = matchId;
            this.opponentId = opponentInfo;
            isSearching = false;
            matchFound = true;

            Debug.Log($"[Matchmaking] Match found! ID: {matchId}, Opponent: {opponentInfo}");

            // Stop spinner
            if (searchingSpinner != null)
                searchingSpinner.SetActive(false);

            // Update UI
            if (titleText != null)
                titleText.text = "MATCH FOUND!";

            if (statusText != null)
                statusText.text = "";

            // Show opponent info
            ShowOpponentInfo(opponentInfo);

            // Play match found effects
            StartCoroutine(MatchFoundSequence());
        }

        private void ShowOpponentSearching(bool searching)
        {
            if (opponentSearchingIndicator != null)
                opponentSearchingIndicator.SetActive(searching);

            if (opponentNameText != null)
                opponentNameText.text = searching ? "???" : "";
        }

        private void ShowOpponentInfo(string opponentInfo)
        {
            ShowOpponentSearching(false);

            // Parse opponent info (could be JSON or simple string)
            // For now, treat it as the opponent name
            if (opponentNameText != null)
                opponentNameText.text = opponentInfo;

            // Animate opponent card appearing
            if (opponentCard != null)
            {
                opponentCard.transform.localScale = Vector3.zero;
                StartCoroutine(AnimateScale(opponentCard.transform, Vector3.one, 0.3f));
            }
        }

        private IEnumerator MatchFoundSequence()
        {
            // Wait for opponent card animation
            yield return new WaitForSeconds(0.5f);

            // Flash VS text
            if (vsContainer != null)
            {
                vsContainer.SetActive(true);
                StartCoroutine(AnimatePulse(vsText?.transform, 3));
            }

            // Vibrate on mobile
            #if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
            #endif

            yield return new WaitForSeconds(1.5f);

            // Start countdown
            StartCoroutine(CountdownSequence());
        }

        private IEnumerator CountdownSequence()
        {
            if (countdownPanel != null)
                countdownPanel.SetActive(true);

            for (int i = 3; i > 0; i--)
            {
                if (countdownText != null)
                {
                    countdownText.text = i.ToString();
                    countdownText.transform.localScale = Vector3.one * 1.5f;
                    StartCoroutine(AnimateScale(countdownText.transform, Vector3.one, 0.3f));
                }

                // Play countdown sound
                // AudioManager.Instance?.PlaySFX("Countdown");

                yield return new WaitForSeconds(1f);
            }

            if (countdownText != null)
                countdownText.text = "GO!";

            yield return new WaitForSeconds(0.5f);

            // Start the game!
            StartOnlineGame();
        }

        private void StartOnlineGame()
        {
            Debug.Log($"[Matchmaking] Starting online game: {currentGameType}");

            // Store match info for the game scene
            PlayerPrefs.SetString("CurrentMatchId", matchId);
            PlayerPrefs.SetString("CurrentOpponentId", opponentId);
            PlayerPrefs.SetInt("IsOnlineMatch", 1);
            PlayerPrefs.Save();

            // Start the game session
            if (GameSessionManager.Instance != null)
            {
                GameSessionManager.Instance.StartOnlineMatch(matchId, opponentId);
            }

            // Load game scene
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
            switch (gameType)
            {
                case GameType.DigitRush: return "DigitRush";
                case GameType.MemoryPairs: return "MemoryPairs";
                case GameType.QuickMath: return "QuickMath";
                case GameType.FlashTap: return "FlashTap";
                case GameType.OddOneOut: return "OddOneOut";
                default: return "GameSelector";
            }
        }

        #endregion

        #region Match Failed

        private void OnMatchFailed(string error)
        {
            isSearching = false;

            Debug.LogError($"[Matchmaking] Match failed: {error}");

            if (titleText != null)
                titleText.text = "SEARCH FAILED";

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
            // Cancel matchmaking
            isSearching = false;

            if (MatchmakingService.Instance != null)
                MatchmakingService.Instance.CancelMatchmaking();

            // Go back to GameSelector
            SceneManager.LoadScene("GameSelector");
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
                t = t * t * (3f - 2f * t); // Smooth step
                target.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            target.localScale = targetScale;
        }

        private IEnumerator AnimatePulse(Transform target, int pulseCount)
        {
            if (target == null) yield break;

            for (int i = 0; i < pulseCount; i++)
            {
                yield return AnimateScale(target, Vector3.one * 1.2f, 0.15f);
                yield return AnimateScale(target, Vector3.one, 0.15f);
            }
        }

        #endregion

        #region Static Methods (Called from GameSelector)

        /// <summary>
        /// Sets the game type for matchmaking before loading the scene
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
