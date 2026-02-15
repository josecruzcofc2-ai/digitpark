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
    /// Controlador para el panel de resultados de Cash Battle 1v1
    /// Muestra VS comparación, dinero ganado/perdido y entry fee
    /// </summary>
    public class CashBattleResultPanelController : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject content;

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;

        [Header("Money Display")]
        [SerializeField] private TextMeshProUGUI moneyResultText;
        [SerializeField] private TextMeshProUGUI entryFeeText;
        [SerializeField] private TextMeshProUGUI winnerShareText;

        [Header("VS Section")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI playerTimeText;
        [SerializeField] private TextMeshProUGUI playerErrorsText;
        [SerializeField] private GameObject playerHighlight;
        [SerializeField] private TextMeshProUGUI opponentNameText;
        [SerializeField] private TextMeshProUGUI opponentTimeText;
        [SerializeField] private TextMeshProUGUI opponentErrorsText;
        [SerializeField] private GameObject opponentHighlight;
        [SerializeField] private TextMeshProUGUI vsText;

        [Header("Buttons")]
        [SerializeField] private Button continueButton;
        [SerializeField] private Button newMatchButton;
        [SerializeField] private TextMeshProUGUI continueButtonText;
        [SerializeField] private TextMeshProUGUI newMatchButtonText;

        [Header("Effects")]
        [SerializeField] private UISparkleEffect sparkleEffect;
        [SerializeField] private AudioClip winSound;
        [SerializeField] private AudioClip loseSound;

        [Header("Colors")]
        [SerializeField] private Color winColor = new Color(0.2f, 1f, 0.4f);
        [SerializeField] private Color loseColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField] private Color goldColor = new Color(1f, 0.84f, 0f);
        [SerializeField] private Color playerColor = new Color(0f, 1f, 1f);
        [SerializeField] private Color opponentColor = new Color(0.7f, 0.3f, 1f);

        // Events
        public event Action OnContinueClicked;
        public event Action OnNewMatchClicked;

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
            if (newMatchButton != null)
                newMatchButton.onClick.AddListener(() => OnNewMatchClicked?.Invoke());
        }

        /// <summary>
        /// Muestra el resultado de un cash battle 1v1
        /// </summary>
        public void ShowCashResult(MinigameResult playerResult, MinigameResult opponentResult,
            decimal entryFee, bool playerWon, string opponentName)
        {
            isWinner = playerWon;
            StartCoroutine(ShowResultSequence(playerResult, opponentResult, entryFee, playerWon, opponentName));
        }

        private IEnumerator ShowResultSequence(MinigameResult playerResult, MinigameResult opponentResult,
            decimal entryFee, bool playerWon, string opponentName)
        {
            gameObject.SetActive(true);
            if (canvasGroup != null) canvasGroup.alpha = 0;

            // Fade in
            yield return StartCoroutine(FadeIn());

            yield return new WaitForSeconds(0.4f);

            // Header
            PopulateHeader(playerWon);

            yield return new WaitForSeconds(0.3f);

            // VS Section
            PopulateVSSection(playerResult, opponentResult, playerWon, opponentName);

            yield return new WaitForSeconds(0.3f);

            // Money
            PopulateMoneySection(entryFee, playerWon);

            // Buttons
            PopulateButtons();

            // Effects
            PlayResultEffects(playerWon);
        }

        private void PopulateHeader(bool playerWon)
        {
            if (titleText != null)
            {
                titleText.text = playerWon
                    ? AutoLocalizer.Get("cash_result_won")
                    : AutoLocalizer.Get("cash_result_lost");
                titleText.color = playerWon ? goldColor : loseColor;
            }

            if (subtitleText != null)
            {
                subtitleText.text = playerWon
                    ? AutoLocalizer.Get("result_victory_message")
                    : AutoLocalizer.Get("result_defeat_message");
            }
        }

        private void PopulateVSSection(MinigameResult playerResult, MinigameResult opponentResult,
            bool playerWon, string opponentName)
        {
            string pName = PlayerPrefs.GetString("PlayerName", "Player");

            if (playerNameText != null)
            {
                playerNameText.text = pName;
                playerNameText.color = playerColor;
            }

            if (playerTimeText != null)
                playerTimeText.text = FormatTime(playerResult.TotalTime);
            if (playerErrorsText != null)
                playerErrorsText.text = AutoLocalizer.Get("result_errors", playerResult.Errors);

            if (opponentNameText != null)
            {
                opponentNameText.text = opponentName ?? "Opponent";
                opponentNameText.color = opponentColor;
            }

            if (opponentResult != null)
            {
                if (opponentTimeText != null)
                    opponentTimeText.text = FormatTime(opponentResult.TotalTime);
                if (opponentErrorsText != null)
                    opponentErrorsText.text = AutoLocalizer.Get("result_errors", opponentResult.Errors);
            }

            // Highlight ganador
            if (playerHighlight != null)
                playerHighlight.SetActive(playerWon);
            if (opponentHighlight != null)
                opponentHighlight.SetActive(!playerWon);
        }

        private void PopulateMoneySection(decimal entryFee, bool playerWon)
        {
            decimal prize = playerWon ? entryFee * 1.8m : 0;

            if (moneyResultText != null)
            {
                if (playerWon)
                {
                    moneyResultText.text = $"+${prize:F2}";
                    moneyResultText.color = winColor;
                }
                else
                {
                    moneyResultText.text = $"-${entryFee:F2}";
                    moneyResultText.color = loseColor;
                }
            }

            if (entryFeeText != null)
                entryFeeText.text = AutoLocalizer.Get("cash_entry_fee", $"{entryFee:F2}");

            if (winnerShareText != null)
            {
                if (playerWon)
                    winnerShareText.text = AutoLocalizer.Get("cash_winner_share");
                else
                    winnerShareText.gameObject.SetActive(false);
            }
        }

        private void PopulateButtons()
        {
            if (continueButtonText != null)
                continueButtonText.text = AutoLocalizer.Get("cash_continue");
            if (newMatchButtonText != null)
                newMatchButtonText.text = AutoLocalizer.Get("cash_new_match");
        }

        private void PlayResultEffects(bool playerWon)
        {
            if (audioSource != null)
            {
                var clip = playerWon ? winSound : loseSound;
                if (clip != null)
                    audioSource.PlayOneShot(clip);
            }

            if (playerWon)
            {
                if (sparkleEffect != null)
                {
                    sparkleEffect.PlayVictoryConfetti();
                    sparkleEffect.PlayCoinExplosion(Vector2.zero);
                }

#if UNITY_ANDROID || UNITY_IOS
                Handheld.Vibrate();
#endif
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
