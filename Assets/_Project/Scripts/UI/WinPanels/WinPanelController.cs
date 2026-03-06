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
    /// Controlador para los Win Panels globales
    /// Maneja tanto el panel de práctica como el de dinero real
    /// </summary>
    public class WinPanelController : MonoBehaviour
    {
        [Header("Panel Type")]
        #pragma warning disable 0414
        [SerializeField] private bool isRealMoneyPanel = false;
        #pragma warning restore 0414

        [Header("Common Elements")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject content;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI errorsText;
        [SerializeField] private Button acceptButton;
        [SerializeField] private Button playAgainButton;

        [Header("Real Money Elements")]
        [SerializeField] private TextMeshProUGUI moneyWonText;
        [SerializeField] private TextMeshProUGUI wagerText;
        [SerializeField] private TextMeshProUGUI opponentScoreText;
        [SerializeField] private GameObject vsContainer;
        [SerializeField] private TextMeshProUGUI playerScoreText;
        [SerializeField] private Image resultIcon;
        [SerializeField] private Sprite winIcon;
        [SerializeField] private Sprite loseIcon;

        [Header("Effects")]
        [SerializeField] private UISparkleEffect sparkleEffect;
        [SerializeField] private AudioClip winSound;
        [SerializeField] private AudioClip loseSound;
        [SerializeField] private float countdownDuration = 3f;

        [Header("Colors")]
        [SerializeField] private Color winColor = new Color(1f, 0.84f, 0f, 1f); // Gold
        [SerializeField] private Color loseColor = new Color(1f, 0.3f, 0.3f, 1f); // Red
        [SerializeField] private Color normalColor = new Color(0f, 1f, 1f, 1f); // Cyan

        // Callbacks
        public event Action OnAcceptClicked;
        public event Action OnPlayAgainClicked;

        private AudioSource audioSource;
        private bool isWinner;
        private decimal moneyWon;
        private bool hasInitialized;
        private Sequence _activeSequence;
        private Sequence _revealSequence;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            SetupButtons();
            hasInitialized = true;
        }

        private void OnDestroy()
        {
            _activeSequence?.Kill();
            _revealSequence?.Kill();
        }

        private void SetupButtons()
        {
            if (acceptButton != null)
                acceptButton.onClick.AddListener(() => OnAcceptClicked?.Invoke());

            if (playAgainButton != null)
                playAgainButton.onClick.AddListener(() => OnPlayAgainClicked?.Invoke());
        }

        /// <summary>
        /// Muestra el panel de práctica/normal
        /// </summary>
        public void ShowNormalResult(MinigameResult result)
        {
            var session = GameSessionManager.Instance;
            bool isSprint = session?.HasActiveSession == true &&
                            session.CurrentContext?.Mode == GameMode.CognitiveSprint;

            if (titleText != null)
            {
                if (isSprint)
                {
                    int current = session.CurrentContext.CurrentGameIndex + 1;
                    int total = session.CurrentContext.Games.Count;
                    titleText.text = AutoLocalizer.Get("sprint_game_progress", current.ToString(), total.ToString());
                }
                else
                {
                    titleText.text = AutoLocalizer.Get("completed");
                }
                titleText.color = normalColor;
            }

            // Ocultar elementos de dinero real
            if (moneyWonText != null) moneyWonText.gameObject.SetActive(false);
            if (wagerText != null) wagerText.gameObject.SetActive(false);
            if (vsContainer != null) vsContainer.SetActive(false);

            // En CognitiveSprint, ocultar Play Again y cambiar texto del botón Accept
            if (isSprint && session.CurrentContext.HasMoreGames)
            {
                if (playAgainButton != null) playAgainButton.gameObject.SetActive(false);
                // El texto del botón Accept se actualiza si tiene un TextMeshProUGUI hijo
                if (acceptButton != null)
                {
                    var btnText = acceptButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (btnText != null)
                        btnText.text = AutoLocalizer.Get("sprint_next_game");
                }
            }
            else
            {
                if (playAgainButton != null) playAgainButton.gameObject.SetActive(true);
            }

            Show();

            // Animated score reveal for time and errors
            _revealSequence?.Kill();
            _revealSequence = DOTween.Sequence().SetLink(gameObject).SetUpdate(true);
            int statIndex = 0;

            if (timeText != null)
            {
                var cg = timeText.GetComponent<CanvasGroup>();
                if (cg == null) cg = timeText.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                timeText.transform.localScale = Vector3.one * 0.9f;
                _revealSequence.Insert(statIndex * 0.25f + 0.3f, cg.DOFade(1f, 0.25f));
                _revealSequence.Insert(statIndex * 0.25f + 0.3f,
                    timeText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack));

                // Animate time counter
                float displayTime = 0f;
                float targetTime = result.TotalTime;
                timeText.text = FormatTime(0f);
                _revealSequence.Insert(statIndex * 0.25f + 0.55f,
                    DOTween.To(() => displayTime, x => { displayTime = x; timeText.text = FormatTime(x); },
                        targetTime, 1.2f).SetEase(Ease.OutQuad)
                    .OnComplete(() => timeText.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 6, 0.5f)));
                statIndex++;
            }

            if (errorsText != null)
            {
                var cg = errorsText.GetComponent<CanvasGroup>();
                if (cg == null) cg = errorsText.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                errorsText.transform.localScale = Vector3.one * 0.9f;
                _revealSequence.Insert(statIndex * 0.25f + 0.3f, cg.DOFade(1f, 0.25f));
                _revealSequence.Insert(statIndex * 0.25f + 0.3f,
                    errorsText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack));

                // Animate error counter
                int displayErrors = 0;
                int targetErrors = result.Errors;
                errorsText.text = "0";
                if (targetErrors > 0)
                {
                    _revealSequence.Insert(statIndex * 0.25f + 0.55f,
                        DOTween.To(() => displayErrors, x => { displayErrors = x; errorsText.text = x.ToString(); },
                            targetErrors, 0.8f).SetEase(Ease.OutQuad)
                        .OnComplete(() => errorsText.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 6, 0.5f)));
                }
                else
                {
                    errorsText.text = "0";
                }
            }
        }

        /// <summary>
        /// Muestra el panel de dinero real con countdown dramático
        /// </summary>
        public void ShowRealMoneyResult(MinigameResult playerResult, MinigameResult opponentResult,
            decimal entryFee, bool playerWon, string opponentName = "Oponente")
        {
            isWinner = playerWon;
            moneyWon = playerWon ? entryFee * 1.8m : 0; // 90% del pool (10% comisión)

            StartCoroutine(ShowRealMoneySequence(playerResult, opponentResult, entryFee, opponentName));
        }

        private IEnumerator ShowRealMoneySequence(MinigameResult playerResult, MinigameResult opponentResult,
            decimal entryFee, string opponentName)
        {
            // Mostrar panel con overlay oscuro
            if (canvasGroup != null) canvasGroup.alpha = 0;
            gameObject.SetActive(true);

            // Fade in
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                if (canvasGroup != null) canvasGroup.alpha = elapsed / 0.3f;
                yield return null;
            }

            // Countdown dramático (opcional)
            if (countdownDuration > 0)
            {
                yield return StartCoroutine(PlayCountdownAnimation());
            }

            // Revelar resultado
            RevealRealMoneyResult(playerResult, opponentResult, entryFee, opponentName);

            // Efectos de victoria o derrota
            if (isWinner)
            {
                PlayWinEffects();
            }
            else
            {
                PlayLoseEffects();
            }
        }

        private IEnumerator PlayCountdownAnimation()
        {
            // Implementar animación de countdown aquí
            // Por ahora solo espera
            yield return new WaitForSeconds(countdownDuration);
        }

        private void RevealRealMoneyResult(MinigameResult playerResult, MinigameResult opponentResult,
            decimal entryFee, string opponentName)
        {
            if (titleText != null)
            {
                titleText.text = isWinner ? AutoLocalizer.Get("you_won") : AutoLocalizer.Get("you_lost");
                titleText.color = isWinner ? winColor : loseColor;
            }

            // Icono de resultado
            if (resultIcon != null)
            {
                resultIcon.sprite = isWinner ? winIcon : loseIcon;
                resultIcon.color = isWinner ? winColor : loseColor;
            }

            // Ocultar botón de jugar de nuevo en dinero real
            if (playAgainButton != null) playAgainButton.gameObject.SetActive(false);

            // Animated reveal sequence for real money stats
            _revealSequence?.Kill();
            _revealSequence = DOTween.Sequence().SetLink(gameObject).SetUpdate(true);
            int statIndex = 0;

            // Animate money result
            if (moneyWonText != null)
            {
                moneyWonText.gameObject.SetActive(true);
                moneyWonText.color = isWinner ? winColor : loseColor;
                var cg = moneyWonText.GetComponent<CanvasGroup>();
                if (cg == null) cg = moneyWonText.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                moneyWonText.transform.localScale = Vector3.one * 0.9f;
                _revealSequence.Insert(statIndex * 0.25f + 0.3f, cg.DOFade(1f, 0.25f));
                _revealSequence.Insert(statIndex * 0.25f + 0.3f,
                    moneyWonText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack));

                // Counter animation for money
                float displayMoney = 0f;
                float targetMoney = isWinner ? (float)moneyWon : (float)entryFee;
                string moneyPrefix = isWinner ? "+$" : "-$";
                moneyWonText.text = $"{moneyPrefix}0.00";
                _revealSequence.Insert(statIndex * 0.25f + 0.55f,
                    DOTween.To(() => displayMoney, x => { displayMoney = x; moneyWonText.text = $"{moneyPrefix}{x:F2}"; },
                        targetMoney, 1.2f).SetEase(Ease.OutQuad)
                    .OnComplete(() => moneyWonText.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 6, 0.5f)));
                statIndex++;
            }

            // Animate wager text
            if (wagerText != null)
            {
                wagerText.gameObject.SetActive(true);
                wagerText.text = AutoLocalizer.Get("result_wager", $"{entryFee:F2}");
                var cg = wagerText.GetComponent<CanvasGroup>();
                if (cg == null) cg = wagerText.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                wagerText.transform.localScale = Vector3.one * 0.9f;
                _revealSequence.Insert(statIndex * 0.25f + 0.3f, cg.DOFade(1f, 0.25f));
                _revealSequence.Insert(statIndex * 0.25f + 0.3f,
                    wagerText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
                statIndex++;
            }

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
                        playerResult.TotalTime, 1.2f).SetEase(Ease.OutQuad)
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
                if (playerResult.Errors > 0)
                {
                    _revealSequence.Insert(statIndex * 0.25f + 0.55f,
                        DOTween.To(() => displayErrors, x => { displayErrors = x; errorsText.text = x.ToString(); },
                            playerResult.Errors, 0.8f).SetEase(Ease.OutQuad)
                        .OnComplete(() => errorsText.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 6, 0.5f)));
                }
                statIndex++;
            }

            // Animate VS comparison
            if (vsContainer != null && opponentResult != null)
            {
                vsContainer.SetActive(true);
                var cg = vsContainer.GetComponent<CanvasGroup>();
                if (cg == null) cg = vsContainer.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                vsContainer.transform.localScale = Vector3.one * 0.9f;
                _revealSequence.Insert(statIndex * 0.25f + 0.3f, cg.DOFade(1f, 0.25f));
                _revealSequence.Insert(statIndex * 0.25f + 0.3f,
                    vsContainer.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack));

                if (playerScoreText != null)
                    playerScoreText.text = AutoLocalizer.Get("result_your_time", $"{playerResult.FinalScore:F2}");
                if (opponentScoreText != null)
                    opponentScoreText.text = AutoLocalizer.Get("result_opponent_time", opponentName, $"{opponentResult.FinalScore:F2}");
            }
        }

        private void PlayWinEffects()
        {
            // Sonido de victoria
            if (audioSource != null && winSound != null)
                audioSource.PlayOneShot(winSound);

            // Partículas de victoria
            if (sparkleEffect != null)
            {
                sparkleEffect.PlayVictoryConfetti();
                sparkleEffect.PlayCoinExplosion(Vector2.zero);
            }

            // Vibración de victoria
            TriggerHapticWin();
        }

        private void PlayLoseEffects()
        {
            // Sonido de derrota
            if (audioSource != null && loseSound != null)
                audioSource.PlayOneShot(loseSound);
        }

        private void TriggerHapticWin()
        {
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }

        public void Show()
        {
            _activeSequence?.Kill();
            StopAllCoroutines();

            // If Awake hasn't run yet (first activation), initialize now
            if (!hasInitialized)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                    audioSource = gameObject.AddComponent<AudioSource>();
                SetupButtons();
                hasInitialized = true;
            }

            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            transform.localScale = Vector3.one * 0.9f;
            gameObject.SetActive(true);

            _activeSequence = DOTween.Sequence()
                .Join(canvasGroup != null
                    ? canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutQuad)
                    : DOTween.Sequence()) // no-op if null
                .Join(transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack))
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        public void Hide()
        {
            if (!gameObject.activeSelf) return;

            _activeSequence?.Kill();
            StopAllCoroutines();

            _activeSequence = DOTween.Sequence()
                .Join(canvasGroup != null
                    ? canvasGroup.DOFade(0f, 0.2f).SetEase(Ease.InQuad)
                    : DOTween.Sequence())
                .Join(transform.DOScale(0.9f, 0.2f).SetEase(Ease.InQuad))
                .SetUpdate(true)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    transform.localScale = Vector3.one;
                    if (canvasGroup != null)
                        canvasGroup.alpha = 1f;
                });
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
