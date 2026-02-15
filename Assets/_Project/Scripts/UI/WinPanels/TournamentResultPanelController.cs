using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
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
        private Sequence _revealSequence;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            SetupButtons();
            Hide();
        }

        private void OnDestroy()
        {
            _revealSequence?.Kill();
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
            // Build animated reveal sequence for all stat elements
            _revealSequence?.Kill();
            _revealSequence = DOTween.Sequence().SetLink(gameObject).SetUpdate(true);
            int statIndex = 0;

            // Animate time
            if (timeText != null)
            {
                var cg = timeText.GetComponent<CanvasGroup>();
                if (cg == null) cg = timeText.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                timeText.transform.localScale = Vector3.one * 0.9f;
                _revealSequence.Insert(statIndex * 0.25f + 0.3f, cg.DOFade(1f, 0.25f));
                _revealSequence.Insert(statIndex * 0.25f + 0.3f,
                    timeText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
                float displayTime = 0f;
                timeText.text = FormatTime(0f);
                _revealSequence.Insert(statIndex * 0.25f + 0.55f,
                    DOTween.To(() => displayTime, x => { displayTime = x; timeText.text = FormatTime(x); },
                        result.TotalTime, 1.2f).SetEase(Ease.OutQuad)
                    .OnComplete(() => timeText.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 6, 0.5f)));
                statIndex++;
            }

            // Animate errors
            if (errorsText != null)
            {
                var cg = errorsText.GetComponent<CanvasGroup>();
                if (cg == null) cg = errorsText.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                errorsText.transform.localScale = Vector3.one * 0.9f;
                _revealSequence.Insert(statIndex * 0.25f + 0.3f, cg.DOFade(1f, 0.25f));
                _revealSequence.Insert(statIndex * 0.25f + 0.3f,
                    errorsText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
                int displayErrors = 0;
                errorsText.text = "0";
                if (result.Errors > 0)
                {
                    _revealSequence.Insert(statIndex * 0.25f + 0.55f,
                        DOTween.To(() => displayErrors, x => { displayErrors = x; errorsText.text = x.ToString(); },
                            result.Errors, 0.8f).SetEase(Ease.OutQuad)
                        .OnComplete(() => errorsText.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 6, 0.5f)));
                }
                statIndex++;
            }

            // Animate score
            if (scoreText != null)
            {
                var cg = scoreText.GetComponent<CanvasGroup>();
                if (cg == null) cg = scoreText.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                scoreText.transform.localScale = Vector3.one * 0.9f;
                _revealSequence.Insert(statIndex * 0.25f + 0.3f, cg.DOFade(1f, 0.25f));
                _revealSequence.Insert(statIndex * 0.25f + 0.3f,
                    scoreText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
                float displayScore = 0f;
                scoreText.text = FormatTime(0f);
                _revealSequence.Insert(statIndex * 0.25f + 0.55f,
                    DOTween.To(() => displayScore, x => { displayScore = x; scoreText.text = FormatTime(x); },
                        result.FinalScore, 1.2f).SetEase(Ease.OutQuad)
                    .OnComplete(() => scoreText.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 6, 0.5f)));
            }
        }

        private void PopulateTournamentInfo(int position, int attemptsUsed, int maxAttempts, float bestTime)
        {
            // Position with animated entrance
            if (positionText != null)
            {
                positionText.text = $"#{position}";
                positionText.color = GetPositionColor(position);
                positionText.transform.localScale = Vector3.one * 1.5f;
                positionText.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack).SetLink(gameObject);
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
                    prizeText.color = GetPositionColor(position);

                    // Animate prize counter
                    var cg = prizeText.GetComponent<CanvasGroup>();
                    if (cg == null) cg = prizeText.gameObject.AddComponent<CanvasGroup>();
                    cg.alpha = 0f;
                    prizeText.transform.localScale = Vector3.one * 0.9f;

                    float displayPrize = 0f;
                    float targetPrize = (float)prize;
                    prizeText.text = "$0.00";

                    DOTween.Sequence()
                        .AppendInterval(1.5f)
                        .Append(cg.DOFade(1f, 0.25f))
                        .Join(prizeText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack))
                        .Append(DOTween.To(() => displayPrize, x => { displayPrize = x; prizeText.text = $"${x:F2}"; },
                            targetPrize, 1.2f).SetEase(Ease.OutQuad))
                        .AppendCallback(() => prizeText.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 6, 0.5f))
                        .SetLink(gameObject)
                        .SetUpdate(true);
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
