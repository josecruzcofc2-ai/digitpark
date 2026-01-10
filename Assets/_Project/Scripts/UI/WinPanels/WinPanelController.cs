using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Games;

namespace DigitPark.UI
{
    /// <summary>
    /// Controlador para los Win Panels globales
    /// Maneja tanto el panel de práctica como el de dinero real
    /// </summary>
    public class WinPanelController : MonoBehaviour
    {
        [Header("Panel Type")]
        [SerializeField] private bool isRealMoneyPanel = false;

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
            if (titleText != null)
            {
                titleText.text = "¡COMPLETADO!";
                titleText.color = normalColor;
            }

            if (timeText != null)
                timeText.text = FormatTime(result.TotalTime);

            if (errorsText != null)
                errorsText.text = result.Errors.ToString();

            // Ocultar elementos de dinero real
            if (moneyWonText != null) moneyWonText.gameObject.SetActive(false);
            if (wagerText != null) wagerText.gameObject.SetActive(false);
            if (vsContainer != null) vsContainer.SetActive(false);

            // Mostrar botón de jugar de nuevo en práctica
            if (playAgainButton != null) playAgainButton.gameObject.SetActive(true);

            Show();
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
                titleText.text = isWinner ? "¡GANASTE!" : "¡PERDISTE!";
                titleText.color = isWinner ? winColor : loseColor;
            }

            if (moneyWonText != null)
            {
                moneyWonText.gameObject.SetActive(true);
                if (isWinner)
                {
                    moneyWonText.text = $"+${moneyWon:F2}";
                    moneyWonText.color = winColor;
                }
                else
                {
                    moneyWonText.text = $"-${entryFee:F2}";
                    moneyWonText.color = loseColor;
                }
            }

            if (wagerText != null)
            {
                wagerText.gameObject.SetActive(true);
                wagerText.text = $"Apuesta: ${entryFee:F2}";
            }

            if (timeText != null)
                timeText.text = FormatTime(playerResult.TotalTime);

            if (errorsText != null)
                errorsText.text = playerResult.Errors.ToString();

            // Comparación VS
            if (vsContainer != null && opponentResult != null)
            {
                vsContainer.SetActive(true);

                if (playerScoreText != null)
                    playerScoreText.text = $"Tú: {playerResult.FinalScore:F2}s";

                if (opponentScoreText != null)
                    opponentScoreText.text = $"{opponentName}: {opponentResult.FinalScore:F2}s";
            }

            // Icono de resultado
            if (resultIcon != null)
            {
                resultIcon.sprite = isWinner ? winIcon : loseIcon;
                resultIcon.color = isWinner ? winColor : loseColor;
            }

            // Ocultar botón de jugar de nuevo en dinero real
            if (playAgainButton != null) playAgainButton.gameObject.SetActive(false);
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
            gameObject.SetActive(true);
            if (canvasGroup != null)
            {
                StartCoroutine(FadeIn());
            }
        }

        public void Hide()
        {
            if (canvasGroup != null)
            {
                StartCoroutine(FadeOut());
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private IEnumerator FadeIn()
        {
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = elapsed / 0.3f;
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOut()
        {
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = 1f - (elapsed / 0.2f);
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
