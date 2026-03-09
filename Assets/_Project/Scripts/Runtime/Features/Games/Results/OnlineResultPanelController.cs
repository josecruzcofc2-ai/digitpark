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
    /// Controlador para el panel de resultados de partidas 1v1 online
    /// Muestra victoria/derrota, tiempos y diferencia (sin dinero)
    /// </summary>
    public class OnlineResultPanelController : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject content;

        [Header("Result Display")]
        [SerializeField] private TextMeshProUGUI resultTitleText;
        [SerializeField] private TextMeshProUGUI resultSubtitleText;
        [SerializeField] private Image resultIcon;
        [SerializeField] private Sprite winSprite;
        [SerializeField] private Sprite loseSprite;

        [Header("Player Section")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI playerTimeText;
        [SerializeField] private TextMeshProUGUI playerErrorsText;
        [SerializeField] private Image playerAvatar;
        [SerializeField] private GameObject playerHighlight;

        [Header("Opponent Section")]
        [SerializeField] private TextMeshProUGUI opponentNameText;
        [SerializeField] private TextMeshProUGUI opponentTimeText;
        [SerializeField] private TextMeshProUGUI opponentErrorsText;
        [SerializeField] private Image opponentAvatar;
        [SerializeField] private GameObject opponentHighlight;

        [Header("Time Difference")]
        [SerializeField] private TextMeshProUGUI timeDifferenceText;
        [SerializeField] private TextMeshProUGUI timeDifferenceLabel;

        [Header("VS Display")]
        [SerializeField] private TextMeshProUGUI vsText;

        [Header("Buttons")]
        [SerializeField] private Button continueButton;
        [SerializeField] private Button rematchButton;
        [SerializeField] private TextMeshProUGUI continueButtonText;
        [SerializeField] private TextMeshProUGUI rematchButtonText;

        [Header("Effects")]
        [SerializeField] private UISparkleEffect sparkleEffect;
        [SerializeField] private AudioClip winSound;
        [SerializeField] private AudioClip loseSound;
        [SerializeField] private ParticleSystem confettiParticles;

        [Header("Colors")]
        [SerializeField] private Color winColor = new Color(0.2f, 1f, 0.4f); // Verde brillante
        [SerializeField] private Color loseColor = new Color(1f, 0.3f, 0.3f); // Rojo
        [SerializeField] private Color cyanColor = new Color(0f, 1f, 1f); // Cyan
        [SerializeField] private Color purpleColor = new Color(0.7f, 0.3f, 1f); // Morado para oponente

        [Header("Animation")]
        [SerializeField] private float revealDelay = 0.5f;
        #pragma warning disable 0414
        [SerializeField] private float countdownDuration = 2f;
        #pragma warning restore 0414

        // Events
        public event Action OnContinueClicked;
        public event Action OnRematchClicked;

        private AudioSource audioSource;
        private bool isWinner;
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
            if (continueButton != null)
                continueButton.onClick.AddListener(() => OnContinueClicked?.Invoke());

            if (rematchButton != null)
                rematchButton.onClick.AddListener(() => OnRematchClicked?.Invoke());
        }

        /// <summary>
        /// Muestra el resultado de una partida online 1v1
        /// </summary>
        public void ShowOnlineResult(
            string playerName,
            float playerTime,
            int playerErrors,
            string opponentName,
            float opponentTime,
            int opponentErrors,
            bool playerWon)
        {
            isWinner = playerWon;
            StartCoroutine(ShowResultSequence(
                playerName, playerTime, playerErrors,
                opponentName, opponentTime, opponentErrors));
        }

        /// <summary>
        /// Muestra el resultado usando MinigameResult
        /// </summary>
        public void ShowOnlineResult(MinigameResult playerResult, MinigameResult opponentResult,
            string playerName, string opponentName, bool playerWon)
        {
            ShowOnlineResult(
                playerName,
                playerResult.TotalTime,
                playerResult.Errors,
                opponentName,
                opponentResult.TotalTime,
                opponentResult.Errors,
                playerWon
            );
        }

        private IEnumerator ShowResultSequence(
            string playerName, float playerTime, int playerErrors,
            string opponentName, float opponentTime, int opponentErrors)
        {
            // Inicializar panel
            gameObject.SetActive(true);
            if (canvasGroup != null) canvasGroup.alpha = 0;

            // Fade in
            yield return StartCoroutine(FadeIn());

            // Pequeña pausa dramática
            yield return new WaitForSeconds(revealDelay);

            // Mostrar información de jugadores primero
            ShowPlayerInfo(playerName, playerTime, playerErrors, true);
            yield return new WaitForSeconds(0.3f);

            ShowPlayerInfo(opponentName, opponentTime, opponentErrors, false);
            yield return new WaitForSeconds(0.5f);

            // Revelar resultado con animación
            RevealResult(playerTime, opponentTime);

            // Efectos según victoria o derrota
            if (isWinner)
            {
                PlayWinEffects();
            }
            else
            {
                PlayLoseEffects();
            }
        }

        private void ShowPlayerInfo(string name, float time, int errors, bool isPlayer)
        {
            if (isPlayer)
            {
                if (playerNameText != null) playerNameText.text = name;

                // Animated time counter for player
                if (playerTimeText != null)
                {
                    var cg = playerTimeText.GetComponent<CanvasGroup>();
                    if (cg == null) cg = playerTimeText.gameObject.AddComponent<CanvasGroup>();
                    cg.alpha = 0f;
                    playerTimeText.transform.localScale = Vector3.one * 0.9f;
                    playerTimeText.text = FormatTime(0f);
                    float displayTime = 0f;
                    DOTween.Sequence()
                        .Append(cg.DOFade(1f, 0.25f))
                        .Join(playerTimeText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack))
                        .Append(DOTween.To(() => displayTime, x => { displayTime = x; playerTimeText.text = FormatTime(x); },
                            time, 1.2f).SetEase(Ease.OutQuad))
                        .AppendCallback(() => playerTimeText.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 6, 0.5f))
                        .SetLink(gameObject).SetUpdate(true);
                }

                // Animated errors reveal for player
                if (playerErrorsText != null)
                {
                    var cg = playerErrorsText.GetComponent<CanvasGroup>();
                    if (cg == null) cg = playerErrorsText.gameObject.AddComponent<CanvasGroup>();
                    cg.alpha = 0f;
                    playerErrorsText.transform.localScale = Vector3.one * 0.9f;
                    playerErrorsText.text = AutoLocalizer.Get("result_errors", errors);
                    DOTween.Sequence()
                        .AppendInterval(0.25f)
                        .Append(cg.DOFade(1f, 0.25f))
                        .Join(playerErrorsText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack))
                        .SetLink(gameObject).SetUpdate(true);
                }
            }
            else
            {
                if (opponentNameText != null) opponentNameText.text = name;

                // Animated time counter for opponent
                if (opponentTimeText != null)
                {
                    var cg = opponentTimeText.GetComponent<CanvasGroup>();
                    if (cg == null) cg = opponentTimeText.gameObject.AddComponent<CanvasGroup>();
                    cg.alpha = 0f;
                    opponentTimeText.transform.localScale = Vector3.one * 0.9f;
                    opponentTimeText.text = FormatTime(0f);
                    float displayTime = 0f;
                    DOTween.Sequence()
                        .Append(cg.DOFade(1f, 0.25f))
                        .Join(opponentTimeText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack))
                        .Append(DOTween.To(() => displayTime, x => { displayTime = x; opponentTimeText.text = FormatTime(x); },
                            time, 1.2f).SetEase(Ease.OutQuad))
                        .AppendCallback(() => opponentTimeText.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 6, 0.5f))
                        .SetLink(gameObject).SetUpdate(true);
                }

                // Animated errors reveal for opponent
                if (opponentErrorsText != null)
                {
                    var cg = opponentErrorsText.GetComponent<CanvasGroup>();
                    if (cg == null) cg = opponentErrorsText.gameObject.AddComponent<CanvasGroup>();
                    cg.alpha = 0f;
                    opponentErrorsText.transform.localScale = Vector3.one * 0.9f;
                    opponentErrorsText.text = AutoLocalizer.Get("result_errors", errors);
                    DOTween.Sequence()
                        .AppendInterval(0.25f)
                        .Append(cg.DOFade(1f, 0.25f))
                        .Join(opponentErrorsText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack))
                        .SetLink(gameObject).SetUpdate(true);
                }
            }
        }

        private void RevealResult(float playerTime, float opponentTime)
        {
            float timeDiff = Mathf.Abs(playerTime - opponentTime);

            _revealSequence?.Kill();
            _revealSequence = DOTween.Sequence().SetLink(gameObject).SetUpdate(true);
            int statIndex = 0;

            // Titulo de resultado con animacion DOTween
            if (resultTitleText != null)
            {
                resultTitleText.text = isWinner ? AutoLocalizer.Get("result_victory") : AutoLocalizer.Get("result_defeat");
                resultTitleText.color = isWinner ? winColor : loseColor;
                resultTitleText.transform.localScale = Vector3.one * 1.5f;
                _revealSequence.Append(resultTitleText.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
                statIndex++;
            }

            // Subtitulo
            if (resultSubtitleText != null)
            {
                resultSubtitleText.text = isWinner
                    ? AutoLocalizer.Get("result_victory_message")
                    : AutoLocalizer.Get("result_defeat_message");
                resultSubtitleText.color = isWinner ? winColor : loseColor;

                var cg = resultSubtitleText.GetComponent<CanvasGroup>();
                if (cg == null) cg = resultSubtitleText.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                resultSubtitleText.transform.localScale = Vector3.one * 0.9f;
                _revealSequence.Insert(statIndex * 0.25f + 0.3f, cg.DOFade(1f, 0.25f));
                _revealSequence.Insert(statIndex * 0.25f + 0.3f,
                    resultSubtitleText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
                statIndex++;
            }

            // Icono de resultado
            if (resultIcon != null)
            {
                resultIcon.sprite = isWinner ? winSprite : loseSprite;
                resultIcon.color = isWinner ? winColor : loseColor;
                resultIcon.transform.localScale = Vector3.zero;
                _revealSequence.Insert(0.2f, resultIcon.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
            }

            // Diferencia de tiempo con counter animation
            if (timeDifferenceText != null)
            {
                string sign = isWinner ? "-" : "+";
                timeDifferenceText.color = isWinner ? winColor : loseColor;

                var cg = timeDifferenceText.GetComponent<CanvasGroup>();
                if (cg == null) cg = timeDifferenceText.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                timeDifferenceText.transform.localScale = Vector3.one * 0.9f;
                timeDifferenceText.text = $"{sign}{FormatTime(0f)}";

                float displayDiff = 0f;
                _revealSequence.Insert(statIndex * 0.25f + 0.3f, cg.DOFade(1f, 0.25f));
                _revealSequence.Insert(statIndex * 0.25f + 0.3f,
                    timeDifferenceText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
                _revealSequence.Insert(statIndex * 0.25f + 0.55f,
                    DOTween.To(() => displayDiff, x => { displayDiff = x; timeDifferenceText.text = $"{sign}{FormatTime(x)}"; },
                        timeDiff, 1.0f).SetEase(Ease.OutQuad)
                    .OnComplete(() => timeDifferenceText.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 6, 0.5f)));
                statIndex++;
            }

            if (timeDifferenceLabel != null)
            {
                timeDifferenceLabel.text = isWinner ? AutoLocalizer.Get("result_faster_by") : AutoLocalizer.Get("result_slower_by");
                var cg = timeDifferenceLabel.GetComponent<CanvasGroup>();
                if (cg == null) cg = timeDifferenceLabel.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                _revealSequence.Insert(statIndex * 0.25f + 0.3f, cg.DOFade(1f, 0.25f));
            }

            // Highlight al ganador
            if (isWinner)
            {
                if (playerHighlight != null) playerHighlight.SetActive(true);
                if (opponentHighlight != null) opponentHighlight.SetActive(false);
            }
            else
            {
                if (playerHighlight != null) playerHighlight.SetActive(false);
                if (opponentHighlight != null) opponentHighlight.SetActive(true);
            }
        }

        private void PlayWinEffects()
        {
            // Sonido de victoria
            if (audioSource != null && winSound != null)
                audioSource.PlayOneShot(winSound);

            // Confetti
            if (confettiParticles != null)
                confettiParticles.Play();

            // Sparkles
            if (sparkleEffect != null)
                sparkleEffect.PlayVictoryConfetti();

            // Vibración
            TriggerHaptic();
        }

        private void PlayLoseEffects()
        {
            // Sonido de derrota
            if (audioSource != null && loseSound != null)
                audioSource.PlayOneShot(loseSound);
        }

        private void TriggerHaptic()
        {
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
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

        private IEnumerator AnimateScale(Transform target, Vector3 targetScale, float duration)
        {
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
