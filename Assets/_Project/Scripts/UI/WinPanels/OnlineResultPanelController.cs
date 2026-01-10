using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Games;

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
        [SerializeField] private float countdownDuration = 2f;

        // Events
        public event Action OnContinueClicked;
        public event Action OnRematchClicked;

        private AudioSource audioSource;
        private bool isWinner;

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
                if (playerTimeText != null) playerTimeText.text = FormatTime(time);
                if (playerErrorsText != null) playerErrorsText.text = $"{errors} errores";
            }
            else
            {
                if (opponentNameText != null) opponentNameText.text = name;
                if (opponentTimeText != null) opponentTimeText.text = FormatTime(time);
                if (opponentErrorsText != null) opponentErrorsText.text = $"{errors} errores";
            }
        }

        private void RevealResult(float playerTime, float opponentTime)
        {
            float timeDiff = Mathf.Abs(playerTime - opponentTime);

            // Título de resultado
            if (resultTitleText != null)
            {
                resultTitleText.text = isWinner ? "VICTORIA" : "DERROTA";
                resultTitleText.color = isWinner ? winColor : loseColor;

                // Animación de escala
                resultTitleText.transform.localScale = Vector3.one * 1.5f;
                StartCoroutine(AnimateScale(resultTitleText.transform, Vector3.one, 0.3f));
            }

            // Subtítulo
            if (resultSubtitleText != null)
            {
                if (isWinner)
                {
                    resultSubtitleText.text = "Mejor tiempo que tu oponente";
                    resultSubtitleText.color = winColor;
                }
                else
                {
                    resultSubtitleText.text = "Tu oponente fue más rápido";
                    resultSubtitleText.color = loseColor;
                }
            }

            // Icono de resultado
            if (resultIcon != null)
            {
                resultIcon.sprite = isWinner ? winSprite : loseSprite;
                resultIcon.color = isWinner ? winColor : loseColor;
            }

            // Diferencia de tiempo
            if (timeDifferenceText != null)
            {
                string sign = isWinner ? "-" : "+";
                timeDifferenceText.text = $"{sign}{FormatTime(timeDiff)}";
                timeDifferenceText.color = isWinner ? winColor : loseColor;
            }

            if (timeDifferenceLabel != null)
            {
                timeDifferenceLabel.text = isWinner ? "Más rápido por" : "Más lento por";
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
