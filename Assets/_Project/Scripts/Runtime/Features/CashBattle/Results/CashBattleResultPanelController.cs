using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using DigitPark.Games;
using DigitPark.Localization;
using DigitPark.Progression;

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
        private Sequence _revealSequence;
        private Sequence _moneySeq;
        private Sequence _entryFeeSeq;
        private Sequence _winnerShareSeq;

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
            _moneySeq?.Kill();
            _entryFeeSeq?.Kill();
            _winnerShareSeq?.Kill();
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

            // XP cosmético al 50% por ser CashBattle (no afecta matchmaking)
            var xpResult = new GameResult
            {
                gameId       = "CashBattle",
                isWin        = playerWon,
                isPerfect    = playerResult.Errors == 0 && playerResult.Completed,
                score        = (int)playerResult.FinalScore,
                scorePercentile = 0f,
                isCashBattle = true
            };
            if (PlayerProgressionSystem.Instance != null)
            {
                int xpGained = PlayerProgressionSystem.Instance.AddGameXP(xpResult);
                if (xpGained > 0)
                    MissionsManager.Instance?.ReportXPEarned(xpGained);
            }

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
            string pName = PlayerPrefs.GetString("PlayerName", AutoLocalizer.Get("default_player_name"));

            if (playerNameText != null)
            {
                playerNameText.text = pName;
                playerNameText.color = playerColor;
            }

            if (opponentNameText != null)
            {
                opponentNameText.text = opponentName ?? AutoLocalizer.Get("default_opponent");
                opponentNameText.color = opponentColor;
            }

            // Highlight ganador
            if (playerHighlight != null)
                playerHighlight.SetActive(playerWon);
            if (opponentHighlight != null)
                opponentHighlight.SetActive(!playerWon);

            // Animated reveal for VS stats
            _revealSequence?.Kill();
            _revealSequence = DOTween.Sequence().SetLink(gameObject).SetUpdate(true);
            int statIndex = 0;

            // Animate player time
            if (playerTimeText != null)
            {
                var cg = playerTimeText.GetComponent<CanvasGroup>();
                if (cg == null) cg = playerTimeText.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                playerTimeText.transform.localScale = Vector3.one * 0.9f;
                _revealSequence.Insert(statIndex * 0.25f + 0.3f, cg.DOFade(1f, 0.25f));
                _revealSequence.Insert(statIndex * 0.25f + 0.3f,
                    playerTimeText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
                float displayPTime = 0f;
                playerTimeText.text = FormatTime(0f);
                _revealSequence.Insert(statIndex * 0.25f + 0.55f,
                    DOTween.To(() => displayPTime, x => { displayPTime = x; playerTimeText.text = FormatTime(x); },
                        playerResult.TotalTime, 1.2f).SetEase(Ease.OutQuad)
                    .OnComplete(() => { if (playerTimeText != null) playerTimeText.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 6, 0.5f); }));
                statIndex++;
            }

            // Animate player errors
            if (playerErrorsText != null)
            {
                var cg = playerErrorsText.GetComponent<CanvasGroup>();
                if (cg == null) cg = playerErrorsText.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                playerErrorsText.transform.localScale = Vector3.one * 0.9f;
                _revealSequence.Insert(statIndex * 0.25f + 0.3f, cg.DOFade(1f, 0.25f));
                _revealSequence.Insert(statIndex * 0.25f + 0.3f,
                    playerErrorsText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
                playerErrorsText.text = AutoLocalizer.Get("result_errors", playerResult.Errors);
                statIndex++;
            }

            // Animate opponent time
            if (opponentResult != null)
            {
                if (opponentTimeText != null)
                {
                    var cg = opponentTimeText.GetComponent<CanvasGroup>();
                    if (cg == null) cg = opponentTimeText.gameObject.AddComponent<CanvasGroup>();
                    cg.alpha = 0f;
                    opponentTimeText.transform.localScale = Vector3.one * 0.9f;
                    _revealSequence.Insert(statIndex * 0.25f + 0.3f, cg.DOFade(1f, 0.25f));
                    _revealSequence.Insert(statIndex * 0.25f + 0.3f,
                        opponentTimeText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
                    float displayOTime = 0f;
                    opponentTimeText.text = FormatTime(0f);
                    _revealSequence.Insert(statIndex * 0.25f + 0.55f,
                        DOTween.To(() => displayOTime, x => { displayOTime = x; opponentTimeText.text = FormatTime(x); },
                            opponentResult.TotalTime, 1.2f).SetEase(Ease.OutQuad)
                        .OnComplete(() => { if (opponentTimeText != null) opponentTimeText.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 6, 0.5f); }));
                    statIndex++;
                }

                if (opponentErrorsText != null)
                {
                    var cg = opponentErrorsText.GetComponent<CanvasGroup>();
                    if (cg == null) cg = opponentErrorsText.gameObject.AddComponent<CanvasGroup>();
                    cg.alpha = 0f;
                    opponentErrorsText.transform.localScale = Vector3.one * 0.9f;
                    _revealSequence.Insert(statIndex * 0.25f + 0.3f, cg.DOFade(1f, 0.25f));
                    _revealSequence.Insert(statIndex * 0.25f + 0.3f,
                        opponentErrorsText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
                    opponentErrorsText.text = AutoLocalizer.Get("result_errors", opponentResult.Errors);
                }
            }
        }

        private void PopulateMoneySection(decimal entryFee, bool playerWon)
        {
            // Kill any previous money sequences before starting new ones (handles ShowCashResult re-entry)
            _moneySeq?.Kill();
            _entryFeeSeq?.Kill();
            _winnerShareSeq?.Kill();

            decimal prize = playerWon ? entryFee * 1.8m : 0;

            // Animate money result with counter
            if (moneyResultText != null)
            {
                moneyResultText.color = playerWon ? winColor : loseColor;
                var cg = moneyResultText.GetComponent<CanvasGroup>();
                if (cg == null) cg = moneyResultText.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                moneyResultText.transform.localScale = Vector3.one * 0.9f;

                float displayMoney = 0f;
                float targetMoney = playerWon ? (float)prize : (float)entryFee;
                string moneyPrefix = playerWon ? "+$" : "-$";
                moneyResultText.text = $"{moneyPrefix}0.00";

                _moneySeq = DOTween.Sequence()
                    .AppendInterval(1.8f)
                    .Append(cg.DOFade(1f, 0.25f))
                    .Join(moneyResultText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack))
                    .Append(DOTween.To(() => displayMoney, x => { displayMoney = x; moneyResultText.text = $"{moneyPrefix}{x:F2}"; },
                        targetMoney, 1.2f).SetEase(Ease.OutQuad))
                    .AppendCallback(() => { if (moneyResultText != null) moneyResultText.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 6, 0.5f); })
                    .SetLink(gameObject)
                    .SetUpdate(true);
            }

            // Animate entry fee text
            if (entryFeeText != null)
            {
                entryFeeText.text = AutoLocalizer.Get("cash_entry_fee", $"{entryFee:F2}");
                var cg = entryFeeText.GetComponent<CanvasGroup>();
                if (cg == null) cg = entryFeeText.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                entryFeeText.transform.localScale = Vector3.one * 0.9f;
                _entryFeeSeq = DOTween.Sequence()
                    .AppendInterval(2.0f)
                    .Append(cg.DOFade(1f, 0.25f))
                    .Join(entryFeeText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack))
                    .SetLink(gameObject)
                    .SetUpdate(true);
            }

            // Animate winner share text
            if (winnerShareText != null)
            {
                if (playerWon)
                {
                    winnerShareText.text = AutoLocalizer.Get("cash_winner_share");
                    var cg = winnerShareText.GetComponent<CanvasGroup>();
                    if (cg == null) cg = winnerShareText.gameObject.AddComponent<CanvasGroup>();
                    cg.alpha = 0f;
                    winnerShareText.transform.localScale = Vector3.one * 0.9f;
                    _winnerShareSeq = DOTween.Sequence()
                        .AppendInterval(2.2f)
                        .Append(cg.DOFade(1f, 0.25f))
                        .Join(winnerShareText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack))
                        .SetLink(gameObject)
                        .SetUpdate(true);
                }
                else
                {
                    winnerShareText.gameObject.SetActive(false);
                }
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
