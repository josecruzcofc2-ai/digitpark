using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Games;
using DigitPark.Localization;

namespace DigitPark.UI
{
    /// <summary>
    /// Controlador para el panel de resumen de Cognitive Sprint
    /// Muestra tabla de juegos con tiempos/errores, totales y variantes por contexto
    /// </summary>
    public class SprintSummaryPanelController : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject content;

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private Image headerIcon;

        [Header("Games Table")]
        [SerializeField] private Transform gameRowsContainer;
        [SerializeField] private GameObject gameRowPrefab;

        [Header("Totals")]
        [SerializeField] private TextMeshProUGUI totalTimeText;
        [SerializeField] private TextMeshProUGUI totalErrorsText;
        [SerializeField] private TextMeshProUGUI totalScoreText;

        [Header("VS Section (Online/Cash)")]
        [SerializeField] private GameObject vsSection;
        [SerializeField] private TextMeshProUGUI gamesWonText;
        [SerializeField] private TextMeshProUGUI overallResultText;
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI opponentNameText;

        [Header("Money Section (Cash)")]
        [SerializeField] private GameObject moneySection;
        [SerializeField] private TextMeshProUGUI moneyResultText;
        [SerializeField] private TextMeshProUGUI entryFeeText;

        [Header("Buttons")]
        [SerializeField] private Button continueButton;
        [SerializeField] private Button rematchButton;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI continueButtonText;
        [SerializeField] private TextMeshProUGUI rematchButtonText;
        [SerializeField] private TextMeshProUGUI playAgainButtonText;
        [SerializeField] private TextMeshProUGUI backButtonText;

        [Header("Effects")]
        [SerializeField] private AudioClip winSound;
        [SerializeField] private AudioClip loseSound;

        [Header("Colors")]
        [SerializeField] private Color winColor = new Color(0.2f, 1f, 0.4f);
        [SerializeField] private Color loseColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField] private Color drawColor = new Color(1f, 0.84f, 0f);
        [SerializeField] private Color playerColor = new Color(0f, 1f, 1f);
        [SerializeField] private Color opponentColor = new Color(0.7f, 0.3f, 1f);

        // Events
        public event Action OnContinueClicked;
        public event Action OnRematchClicked;
        public event Action OnPlayAgainClicked;
        public event Action OnBackClicked;
        public event Action OnNewMatchClicked;

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
            if (continueButton != null)
                continueButton.onClick.AddListener(() => OnContinueClicked?.Invoke());
            if (rematchButton != null)
                rematchButton.onClick.AddListener(() => OnRematchClicked?.Invoke());
            if (playAgainButton != null)
                playAgainButton.onClick.AddListener(() => OnPlayAgainClicked?.Invoke());
            if (backButton != null)
                backButton.onClick.AddListener(() => OnBackClicked?.Invoke());
        }

        /// <summary>
        /// Muestra resumen de sprint de práctica (solo resultados del jugador)
        /// </summary>
        public void ShowPracticeSummary(GameContext ctx)
        {
            if (titleText != null)
                titleText.text = AutoLocalizer.Get("sprint_complete");

            if (subtitleText != null)
                subtitleText.text = AutoLocalizer.Get("sprint_summary");

            // Ocultar secciones que no aplican
            if (vsSection != null) vsSection.SetActive(false);
            if (moneySection != null) moneySection.SetActive(false);

            // Mostrar botones de práctica
            SetButtonVisibility(playAgainButton, true);
            SetButtonVisibility(backButton, true);
            SetButtonVisibility(continueButton, false);
            SetButtonVisibility(rematchButton, false);

            PopulateGameTable(ctx.Results, null);
            PopulateTotals(ctx.Results);

            Show();
        }

        /// <summary>
        /// Muestra resumen de sprint online (VS con resultado por juego)
        /// </summary>
        public void ShowOnlineSummary(GameContext ctx)
        {
            int winner = ctx.CalculateWinner();
            bool playerWon = winner > 0;

            if (titleText != null)
            {
                if (winner > 0)
                    titleText.text = AutoLocalizer.Get("sprint_result_win");
                else if (winner < 0)
                    titleText.text = AutoLocalizer.Get("sprint_result_lose");
                else
                    titleText.text = AutoLocalizer.Get("sprint_result_draw");

                titleText.color = winner > 0 ? winColor : winner < 0 ? loseColor : drawColor;
            }

            if (subtitleText != null)
                subtitleText.text = AutoLocalizer.Get("sprint_summary");

            // VS section
            if (vsSection != null)
            {
                vsSection.SetActive(true);
                PopulateVSSection(ctx, winner);
            }

            if (moneySection != null) moneySection.SetActive(false);

            // Botones online
            SetButtonVisibility(continueButton, true);
            SetButtonVisibility(rematchButton, true);
            SetButtonVisibility(playAgainButton, false);
            SetButtonVisibility(backButton, false);

            PopulateGameTable(ctx.Results, ctx.OpponentResults);
            PopulateTotals(ctx.Results);

            Show();
            PlayResultEffects(playerWon);
        }

        /// <summary>
        /// Muestra resumen de sprint con dinero real
        /// </summary>
        public void ShowCashSummary(GameContext ctx, decimal entryFee)
        {
            int winner = ctx.CalculateWinner();
            bool playerWon = winner > 0;
            decimal prize = playerWon ? entryFee * 1.8m : 0;

            if (titleText != null)
            {
                if (winner > 0)
                    titleText.text = AutoLocalizer.Get("cash_result_won");
                else if (winner < 0)
                    titleText.text = AutoLocalizer.Get("cash_result_lost");
                else
                    titleText.text = AutoLocalizer.Get("sprint_result_draw");

                titleText.color = winner > 0 ? winColor : winner < 0 ? loseColor : drawColor;
            }

            if (subtitleText != null)
                subtitleText.text = AutoLocalizer.Get("sprint_summary");

            // VS section
            if (vsSection != null)
            {
                vsSection.SetActive(true);
                PopulateVSSection(ctx, winner);
            }

            // Money section
            if (moneySection != null)
            {
                moneySection.SetActive(true);

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
            }

            // Botones cash
            SetButtonVisibility(continueButton, true);
            SetButtonVisibility(rematchButton, false);
            SetButtonVisibility(playAgainButton, false);
            SetButtonVisibility(backButton, false);

            // Reusar rematch como "Nueva Partida"
            if (rematchButton != null)
            {
                rematchButton.gameObject.SetActive(true);
                rematchButton.onClick.RemoveAllListeners();
                rematchButton.onClick.AddListener(() => OnNewMatchClicked?.Invoke());
                if (rematchButtonText != null)
                    rematchButtonText.text = AutoLocalizer.Get("cash_new_match");
            }

            PopulateGameTable(ctx.Results, ctx.OpponentResults);
            PopulateTotals(ctx.Results);

            Show();
            PlayResultEffects(playerWon);
        }

        private void PopulateGameTable(List<MinigameResult> playerResults, List<MinigameResult> opponentResults)
        {
            if (gameRowsContainer == null) return;

            // Limpiar filas anteriores (excepto el prefab template)
            for (int i = gameRowsContainer.childCount - 1; i >= 0; i--)
            {
                var child = gameRowsContainer.GetChild(i);
                if (child.gameObject != gameRowPrefab)
                    Destroy(child.gameObject);
            }

            if (playerResults == null) return;

            for (int i = 0; i < playerResults.Count; i++)
            {
                var result = playerResults[i];
                GameObject row = gameRowPrefab != null
                    ? Instantiate(gameRowPrefab, gameRowsContainer)
                    : CreateDefaultRow(gameRowsContainer);

                row.SetActive(true);
                row.name = $"GameRow_{i}";

                // Buscar textos en la fila
                var texts = row.GetComponentsInChildren<TextMeshProUGUI>(true);
                if (texts.Length >= 3)
                {
                    // GameName | Time | Errors
                    texts[0].text = AutoLocalizer.Get($"game_{result.GameType.ToString().ToLower()}",
                        result.GameType.ToString());
                    texts[1].text = FormatTime(result.TotalTime);
                    texts[2].text = result.Errors.ToString();

                    // Si hay resultados del oponente, mostrar comparación
                    if (opponentResults != null && i < opponentResults.Count && texts.Length >= 5)
                    {
                        texts[3].text = FormatTime(opponentResults[i].TotalTime);
                        texts[4].text = opponentResults[i].Errors.ToString();

                        // Colorear al ganador
                        bool playerBetter = result.IsBetterThan(opponentResults[i]);
                        texts[1].color = playerBetter ? winColor : loseColor;
                        texts[3].color = playerBetter ? loseColor : winColor;
                    }
                }
            }
        }

        private GameObject CreateDefaultRow(Transform parent)
        {
            var row = new GameObject("GameRow");
            row.transform.SetParent(parent, false);
            var rt = row.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 40);

            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 10;
            layout.childForceExpandWidth = true;

            // Crear 3 columnas básicas
            for (int i = 0; i < 3; i++)
            {
                var col = new GameObject($"Col_{i}");
                col.transform.SetParent(row.transform, false);
                col.AddComponent<RectTransform>();
                var tmp = col.AddComponent<TextMeshProUGUI>();
                tmp.fontSize = 20;
                tmp.color = Color.white;
                tmp.alignment = TextAlignmentOptions.Center;
            }

            return row;
        }

        private void PopulateTotals(List<MinigameResult> results)
        {
            if (results == null || results.Count == 0) return;

            float totalTime = 0;
            int totalErrors = 0;
            foreach (var r in results)
            {
                totalTime += r.TotalTime;
                totalErrors += r.Errors;
            }

            if (totalTimeText != null)
                totalTimeText.text = FormatTime(totalTime);
            if (totalErrorsText != null)
                totalErrorsText.text = totalErrors.ToString();
            if (totalScoreText != null)
            {
                float totalScore = 0;
                foreach (var r in results) totalScore += r.FinalScore;
                totalScoreText.text = FormatTime(totalScore);
            }
        }

        private void PopulateVSSection(GameContext ctx, int winner)
        {
            if (playerNameText != null)
            {
                string pName = PlayerPrefs.GetString("PlayerName", "Player");
                playerNameText.text = pName;
                playerNameText.color = playerColor;
            }

            if (opponentNameText != null)
            {
                opponentNameText.text = ctx.OpponentName ?? "Opponent";
                opponentNameText.color = opponentColor;
            }

            // Contar juegos ganados
            int playerWins = 0, opponentWins = 0;
            for (int i = 0; i < ctx.Results.Count && i < ctx.OpponentResults.Count; i++)
            {
                if (ctx.Results[i].IsBetterThan(ctx.OpponentResults[i]))
                    playerWins++;
                else if (ctx.OpponentResults[i].IsBetterThan(ctx.Results[i]))
                    opponentWins++;
            }

            if (gamesWonText != null)
                gamesWonText.text = AutoLocalizer.Get("sprint_games_won", playerWins.ToString(), opponentWins.ToString());

            if (overallResultText != null)
            {
                overallResultText.text = $"{playerWins} - {opponentWins}";
                overallResultText.color = winner > 0 ? winColor : winner < 0 ? loseColor : drawColor;
            }
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
#if UNITY_ANDROID || UNITY_IOS
                Handheld.Vibrate();
#endif
            }
        }

        private void SetButtonVisibility(Button button, bool visible)
        {
            if (button != null)
                button.gameObject.SetActive(visible);
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
