using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Games;
using DigitPark.Localization;

namespace DigitPark.UI
{
    /// <summary>
    /// Controlador para el panel de resultados de torneos
    /// Muestra tiempo/errores, posición actual, intentos y premio potencial
    /// </summary>
    public class TournamentResultPanelController : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject content;

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private Image trophyIcon;

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI errorsText;
        [SerializeField] private TextMeshProUGUI scoreText;

        [Header("Tournament Info")]
        [SerializeField] private TextMeshProUGUI positionText;
        [SerializeField] private TextMeshProUGUI positionLabel;
        [SerializeField] private TextMeshProUGUI attemptsText;
        [SerializeField] private TextMeshProUGUI bestTimeText;

        [Header("Prize (Cash tournaments)")]
        [SerializeField] private GameObject prizeSection;
        [SerializeField] private TextMeshProUGUI prizeText;
        [SerializeField] private TextMeshProUGUI prizeLabel;

        [Header("Buttons")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button leaderboardButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private TextMeshProUGUI retryButtonText;
        [SerializeField] private TextMeshProUGUI leaderboardButtonText;
        [SerializeField] private TextMeshProUGUI exitButtonText;

        [Header("Effects")]
        [SerializeField] private UISparkleEffect sparkleEffect;
        [SerializeField] private AudioClip resultSound;

        [Header("Colors")]
        [SerializeField] private Color goldColor = new Color(1f, 0.84f, 0f);
        [SerializeField] private Color silverColor = new Color(0.75f, 0.75f, 0.75f);
        [SerializeField] private Color bronzeColor = new Color(0.8f, 0.5f, 0.2f);
        [SerializeField] private Color normalColor = new Color(0f, 1f, 1f);
        [SerializeField] private Color disabledColor = new Color(0.4f, 0.4f, 0.4f);

        // Events
        public event Action OnRetryClicked;
        public event Action OnLeaderboardClicked;
        public event Action OnExitClicked;

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            SetupButtons();
            Hide();
        }

        private void SetupButtons()
        {
            if (retryButton != null)
                retryButton.onClick.AddListener(() => OnRetryClicked?.Invoke());
            if (leaderboardButton != null)
                leaderboardButton.onClick.AddListener(() => OnLeaderboardClicked?.Invoke());
            if (exitButton != null)
                exitButton.onClick.AddListener(() => OnExitClicked?.Invoke());
        }

        /// <summary>
        /// Muestra el resultado de un intento de torneo
        /// </summary>
        public void ShowTournamentResult(MinigameResult result, int position,
            int attemptsUsed, int maxAttempts, float bestTime, decimal prize)
        {
            StartCoroutine(ShowResultSequence(result, position, attemptsUsed, maxAttempts, bestTime, prize));
        }

        private IEnumerator ShowResultSequence(MinigameResult result, int position,
            int attemptsUsed, int maxAttempts, float bestTime, decimal prize)
        {
            gameObject.SetActive(true);
            if (canvasGroup != null) canvasGroup.alpha = 0;

            // Fade in
            yield return StartCoroutine(FadeIn());

            yield return new WaitForSeconds(0.3f);

            // Populate data
            PopulateHeader(position);
            PopulateStats(result);
            PopulateTournamentInfo(position, attemptsUsed, maxAttempts, bestTime);
            PopulatePrize(prize, position);
            ConfigureButtons(attemptsUsed, maxAttempts);

            // Efectos
            if (audioSource != null && resultSound != null)
                audioSource.PlayOneShot(resultSound);

            if (position <= 3 && sparkleEffect != null)
                sparkleEffect.PlayVictoryConfetti();

            if (position == 1)
            {
#if UNITY_ANDROID || UNITY_IOS
                Handheld.Vibrate();
#endif
            }
        }

        private void PopulateHeader(int position)
        {
            if (titleText != null)
            {
                titleText.text = AutoLocalizer.Get("tournament_result");
                titleText.color = GetPositionColor(position);
            }

            if (subtitleText != null)
            {
                subtitleText.text = AutoLocalizer.Get("tournament_position", position.ToString());
                subtitleText.color = GetPositionColor(position);
            }

            if (trophyIcon != null)
                trophyIcon.color = GetPositionColor(position);
        }

        private void PopulateStats(MinigameResult result)
        {
            if (timeText != null)
                timeText.text = FormatTime(result.TotalTime);
            if (errorsText != null)
                errorsText.text = result.Errors.ToString();
            if (scoreText != null)
                scoreText.text = FormatTime(result.FinalScore);
        }

        private void PopulateTournamentInfo(int position, int attemptsUsed, int maxAttempts, float bestTime)
        {
            if (positionText != null)
            {
                positionText.text = $"#{position}";
                positionText.color = GetPositionColor(position);
            }

            if (positionLabel != null)
                positionLabel.text = AutoLocalizer.Get("tournament_position", position.ToString());

            if (attemptsText != null)
            {
                int remaining = maxAttempts - attemptsUsed;
                attemptsText.text = AutoLocalizer.Get("tournament_attempts_left",
                    remaining.ToString(), maxAttempts.ToString());
                attemptsText.color = remaining > 0 ? normalColor : disabledColor;
            }

            if (bestTimeText != null)
            {
                if (bestTime > 0)
                    bestTimeText.text = AutoLocalizer.Get("tournament_best_time", FormatTime(bestTime));
                else
                    bestTimeText.text = AutoLocalizer.Get("tournament_best_time", "--");
            }
        }

        private void PopulatePrize(decimal prize, int position)
        {
            if (prizeSection != null)
            {
                bool hasPrize = prize > 0;
                prizeSection.SetActive(hasPrize);

                if (hasPrize && prizeText != null)
                {
                    prizeText.text = $"${prize:F2}";
                    prizeText.color = GetPositionColor(position);
                }

                if (hasPrize && prizeLabel != null)
                    prizeLabel.text = AutoLocalizer.Get("tournament_potential_prize");
            }
        }

        private void ConfigureButtons(int attemptsUsed, int maxAttempts)
        {
            bool canRetry = attemptsUsed < maxAttempts;

            if (retryButton != null)
            {
                retryButton.interactable = canRetry;
                if (retryButtonText != null)
                {
                    retryButtonText.text = canRetry
                        ? AutoLocalizer.Get("tournament_retry")
                        : AutoLocalizer.Get("tournament_no_attempts");
                    retryButtonText.color = canRetry ? Color.white : disabledColor;
                }
            }

            if (leaderboardButtonText != null)
                leaderboardButtonText.text = AutoLocalizer.Get("tournament_leaderboard");

            if (exitButtonText != null)
                exitButtonText.text = AutoLocalizer.Get("tournament_exit");
        }

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

        public void Show()
        {
            gameObject.SetActive(true);
            if (canvasGroup != null)
                StartCoroutine(FadeIn());
        }

        public void Hide()
        {
            if (canvasGroup != null)
                StartCoroutine(FadeOut());
            else
                gameObject.SetActive(false);
        }

        private IEnumerator FadeIn()
        {
            float elapsed = 0f;
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                if (canvasGroup != null) canvasGroup.alpha = elapsed / 0.3f;
                yield return null;
            }
            if (canvasGroup != null) canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOut()
        {
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;
                if (canvasGroup != null) canvasGroup.alpha = 1f - (elapsed / 0.2f);
                yield return null;
            }
            gameObject.SetActive(false);
        }

        private string FormatTime(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            int ms = Mathf.FloorToInt((time * 100f) % 100f);

            if (minutes > 0)
                return $"{minutes}:{seconds:00}.{ms:00}";
            return $"{seconds}.{ms:00}s";
        }
    }
}
