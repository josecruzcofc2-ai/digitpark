using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Localization;

namespace DigitPark.UI.CashBattle
{
    /// <summary>
    /// Data class for tournament result information.
    /// Passed to ShowTournamentResult to populate the results panel.
    /// </summary>
    [System.Serializable]
    public class TournamentResultData
    {
        public string tournamentName;
        public int position;
        public int totalParticipants;
        public float completionTime;
        public int errors;
        public int attempts;
        public int maxAttempts;
        public float bestTime;
        public decimal prizeAmount;
        public bool hasPrize;
    }

    /// <summary>
    /// Controller for the CashTournamentResults overlay panel.
    /// Handles displaying tournament results with animations,
    /// position-based coloring, and prize counter animation.
    /// This is a panel overlay (not a scene manager).
    /// </summary>
    public class CashTournamentResultsPanelController : MonoBehaviour
    {
        [Header("=== MAIN PANEL ===")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject content;

        [Header("=== HEADER ===")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private Image trophyIcon;

        [Header("=== STATS ===")]
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI errorsText;

        [Header("=== POSITION ===")]
        [SerializeField] private TextMeshProUGUI positionText;
        [SerializeField] private TextMeshProUGUI positionLabel;
        [SerializeField] private TextMeshProUGUI attemptsText;
        [SerializeField] private TextMeshProUGUI bestTimeText;

        [Header("=== PRIZE ===")]
        [SerializeField] private GameObject prizeSection;
        [SerializeField] private TextMeshProUGUI prizeText;
        [SerializeField] private TextMeshProUGUI prizeLabel;

        [Header("=== BUTTONS ===")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button leaderboardButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private TextMeshProUGUI retryButtonText;
        [SerializeField] private TextMeshProUGUI leaderboardButtonText;
        [SerializeField] private TextMeshProUGUI exitButtonText;

        [Header("=== COLORS ===")]
        [SerializeField] private Color goldColor = new Color(1f, 0.84f, 0f, 1f);
        [SerializeField] private Color silverColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        [SerializeField] private Color bronzeColor = new Color(0.8f, 0.5f, 0.2f, 1f);
        [SerializeField] private Color normalColor = new Color(1f, 0.84f, 0f, 1f);

        // Events
        public event Action OnRetry;
        public event Action OnViewLeaderboard;
        public event Action OnExit;

        // Animation state
        private Coroutine fadeCoroutine;
        private Coroutine prizeCounterCoroutine;

        private void Start()
        {
            SetupListeners();
            Hide();
        }

        private void SetupListeners()
        {
            if (retryButton != null)
                retryButton.onClick.AddListener(OnRetryClicked);

            if (leaderboardButton != null)
                leaderboardButton.onClick.AddListener(OnLeaderboardClicked);

            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitClicked);
        }

        #region Show / Hide

        /// <summary>
        /// Display the tournament results with full animation.
        /// </summary>
        public void ShowTournamentResult(TournamentResultData data)
        {
            if (data == null)
            {
                Debug.LogError("[CashTournamentResults] TournamentResultData is null!");
                return;
            }

            // Populate all fields
            PopulateHeader(data);
            PopulateStats(data);
            PopulatePosition(data);
            PopulatePrize(data);

            // Activate and fade in
            gameObject.SetActive(true);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(AnimateFadeIn());
        }

        /// <summary>
        /// Hide the results panel with fade-out animation.
        /// </summary>
        public void Hide()
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            if (prizeCounterCoroutine != null)
                StopCoroutine(prizeCounterCoroutine);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Hide with fade-out animation, then deactivate.
        /// </summary>
        public void HideAnimated()
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(AnimateFadeOut());
        }

        #endregion

        #region Populate Methods

        private void PopulateHeader(TournamentResultData data)
        {
            if (titleText != null)
                titleText.text = AutoLocalizer.Get("cash_tournament_complete");

            if (subtitleText != null)
                subtitleText.text = data.tournamentName ?? AutoLocalizer.Get("tournament_default_name");

            // Tint trophy based on position
            if (trophyIcon != null)
                trophyIcon.color = GetPositionColor(data.position);
        }

        private void PopulateStats(TournamentResultData data)
        {
            if (timeText != null)
            {
                int minutes = Mathf.FloorToInt(data.completionTime / 60f);
                float seconds = data.completionTime % 60f;
                timeText.text = $"{minutes}:{seconds:00.00}";
            }

            if (errorsText != null)
                errorsText.text = data.errors.ToString();

        }

        private void PopulatePosition(TournamentResultData data)
        {
            Color posColor = GetPositionColor(data.position);

            if (positionText != null)
            {
                positionText.text = $"#{data.position}";
                positionText.color = posColor;
            }

            if (positionLabel != null)
            {
                string suffix = GetPositionSuffix(data.position);
                positionLabel.text = AutoLocalizer.Get("tournament_place", data.position, suffix);
                positionLabel.color = posColor;
            }

            if (attemptsText != null)
                attemptsText.text = AutoLocalizer.Get("tournament_attempts_result", data.attempts, data.maxAttempts);

            if (bestTimeText != null)
            {
                int bestMin = Mathf.FloorToInt(data.bestTime / 60f);
                float bestSec = data.bestTime % 60f;
                bestTimeText.text = AutoLocalizer.Get("tournament_best_time", $"{bestMin}:{bestSec:00.00}");
            }
        }

        private void PopulatePrize(TournamentResultData data)
        {
            if (prizeSection != null)
                prizeSection.SetActive(data.hasPrize);

            if (!data.hasPrize) return;

            if (prizeLabel != null)
                prizeLabel.text = AutoLocalizer.Get("cash_you_won");

            if (prizeText != null)
            {
                // Start counter animation
                if (prizeCounterCoroutine != null)
                    StopCoroutine(prizeCounterCoroutine);
                prizeCounterCoroutine = StartCoroutine(AnimatePrizeCounter(data.prizeAmount));
            }
        }

        #endregion

        #region Color / Suffix Helpers

        private Color GetPositionColor(int position)
        {
            switch (position)
            {
                case 1: return goldColor;
                case 2: return silverColor;
                case 3: return bronzeColor;
                default: return normalColor;
            }
        }

        private string GetPositionSuffix(int position)
        {
            // Handle 11th, 12th, 13th special cases
            if (position % 100 >= 11 && position % 100 <= 13)
                return AutoLocalizer.Get("ordinal_th");

            switch (position % 10)
            {
                case 1: return AutoLocalizer.Get("ordinal_st");
                case 2: return AutoLocalizer.Get("ordinal_nd");
                case 3: return AutoLocalizer.Get("ordinal_rd");
                default: return AutoLocalizer.Get("ordinal_th");
            }
        }

        #endregion

        #region Animations

        private IEnumerator AnimateFadeIn()
        {
            if (canvasGroup == null) yield break;

            float duration = 0.3f;
            float elapsed = 0f;

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;

            fadeCoroutine = null;
        }

        private IEnumerator AnimateFadeOut()
        {
            if (canvasGroup == null) yield break;

            float duration = 0.2f;
            float elapsed = 0f;

            canvasGroup.interactable = false;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);

            fadeCoroutine = null;
        }

        private IEnumerator AnimatePrizeCounter(decimal target)
        {
            if (prizeText == null) yield break;

            float duration = 1f;
            float elapsed = 0f;
            float targetFloat = (float)target;

            // Simple scale pulse on prize text at the end
            Transform prizeTransform = prizeText.transform;
            Vector3 originalScale = prizeTransform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Ease out cubic for smooth counting
                float easedT = 1f - Mathf.Pow(1f - t, 3f);
                float currentValue = Mathf.Lerp(0f, targetFloat, easedT);

                prizeText.text = $"${currentValue:F2}";

                yield return null;
            }

            prizeText.text = $"${target:F2}";

            // Scale pulse animation
            float pulseDuration = 0.3f;
            float pulseElapsed = 0f;

            while (pulseElapsed < pulseDuration)
            {
                pulseElapsed += Time.deltaTime;
                float t = pulseElapsed / pulseDuration;

                // Scale up then back down
                float scale;
                if (t < 0.5f)
                    scale = Mathf.Lerp(1f, 1.15f, t * 2f);
                else
                    scale = Mathf.Lerp(1.15f, 1f, (t - 0.5f) * 2f);

                prizeTransform.localScale = originalScale * scale;
                yield return null;
            }

            prizeTransform.localScale = originalScale;
            prizeCounterCoroutine = null;
        }

        #endregion

        #region Button Handlers

        private void OnRetryClicked()
        {
            Debug.Log("[CashTournamentResults] Retry clicked");
            OnRetry?.Invoke();
        }

        private void OnLeaderboardClicked()
        {
            Debug.Log("[CashTournamentResults] Leaderboard clicked");
            OnViewLeaderboard?.Invoke();
        }

        private void OnExitClicked()
        {
            Debug.Log("[CashTournamentResults] Exit clicked");
            HideAnimated();
            OnExit?.Invoke();
        }

        #endregion

        private void OnDestroy()
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            if (prizeCounterCoroutine != null)
                StopCoroutine(prizeCounterCoroutine);
        }
    }
}
