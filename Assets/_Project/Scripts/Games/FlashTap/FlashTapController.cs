using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.UI;
using DigitPark.Effects;
using DigitPark.Localization;

namespace DigitPark.Games
{
    /// <summary>
    /// Controller para el juego Flash Tap
    /// Flujo: Settings Panel → Countdown → Boton naranja (espera 3-6s) → Rojo (TAP!) →
    /// Correcto: verde + siguiente ronda con countdown
    /// Incorrecto: rojo + penalización 2s + 1s cooldown → reinicia espera
    /// </summary>
    public class FlashTapController : MinigameBase
    {
        public override GameType GameType => GameType.FlashTap;

        [Header("Flash Tap - UI")]
        [SerializeField] private Button tapButton;
        [SerializeField] private FlashTapButton3D button3D;
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private TextMeshProUGUI reactionTimeText;
        [SerializeField] private TextMeshProUGUI roundText;
        [SerializeField] private TextMeshProUGUI errorsText;
        [SerializeField] private TextMeshProUGUI roundIndicatorText;
        [SerializeField] private RectTransform progressFill;

        [Header("Flash Tap - Countdown")]
        [SerializeField] private CountdownUI countdownUI;
        [SerializeField] private bool useCountdown = true;

        [Header("Flash Tap - Settings Panel")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Toggle toggleRounds1;
        [SerializeField] private Toggle toggleRounds3;
        [SerializeField] private Toggle toggleRounds5;
        [SerializeField] private Toggle toggleRounds10;
        [SerializeField] private Button startGameButton;

        [Header("Flash Tap - Feedback")]
        [SerializeField] private GameObject feedbackPanel;
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private bool enableSuccessParticles = true;
        [SerializeField] private bool enableHaptics = true;

        [Header("Flash Tap - Settings")]
        [SerializeField] private int totalAttempts = 5;
        [SerializeField] private float minWaitTime = 3f;
        [SerializeField] private float maxWaitTime = 6f;
        [SerializeField] private float penaltyDuration = 2f;
        [SerializeField] private float cooldownAfterError = 1f;
        [SerializeField] private float delayBetweenAttempts = 1.5f;

        // Estado del juego
        private int currentAttempt;
        private List<float> reactionTimes = new List<float>();
        private bool isWaiting;
        private bool isSignalActive;
        private float signalStartTime;
        private float bestTime = float.MaxValue;
        private float penaltyTime;
        private Coroutine waitCoroutine;
        private Coroutine feedbackCoroutine;

        protected override void Awake()
        {
            base.Awake();
            totalAttempts = config?.rounds ?? 5;
        }

        protected override void Start()
        {
            base.Start();

            // Configurar el boton 3D
            if (button3D != null)
            {
                button3D.OnButtonPressed += OnTap;
            }
            else if (tapButton != null)
            {
                tapButton.onClick.AddListener(OnTap);
            }

            // Practice mode: mostrar settings panel
            if (IsPracticeMode())
            {
                SetupSettingsPanel();
                ShowSettingsPanel();
            }
            else
            {
                totalAttempts = config?.rounds ?? 5;
                StartGameWithCountdown();
            }
        }

        private void OnDestroy()
        {
            if (button3D != null)
                button3D.OnButtonPressed -= OnTap;

            if (feedbackCoroutine != null)
                StopCoroutine(feedbackCoroutine);
        }

        #region Settings Panel

        private void SetupSettingsPanel()
        {
            if (toggleRounds1 != null)
                toggleRounds1.onValueChanged.AddListener(v => { if (v) SetRadio(toggleRounds1); });
            if (toggleRounds3 != null)
                toggleRounds3.onValueChanged.AddListener(v => { if (v) SetRadio(toggleRounds3); });
            if (toggleRounds5 != null)
                toggleRounds5.onValueChanged.AddListener(v => { if (v) SetRadio(toggleRounds5); });
            if (toggleRounds10 != null)
                toggleRounds10.onValueChanged.AddListener(v => { if (v) SetRadio(toggleRounds10); });

            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGameClicked);

            // Default: 5 rondas
            SetToggleDefault(toggleRounds5);
        }

        private void SetRadio(Toggle active)
        {
            if (active != toggleRounds1 && toggleRounds1 != null) { toggleRounds1.isOn = false; UpdateToggleVisual(toggleRounds1, false); }
            if (active != toggleRounds3 && toggleRounds3 != null) { toggleRounds3.isOn = false; UpdateToggleVisual(toggleRounds3, false); }
            if (active != toggleRounds5 && toggleRounds5 != null) { toggleRounds5.isOn = false; UpdateToggleVisual(toggleRounds5, false); }
            if (active != toggleRounds10 && toggleRounds10 != null) { toggleRounds10.isOn = false; UpdateToggleVisual(toggleRounds10, false); }
            UpdateToggleVisual(active, true);
        }

        private void UpdateToggleVisual(Toggle toggle, bool active)
        {
            if (toggle == null) return;
            var img = toggle.GetComponent<Image>();
            if (img != null) img.color = active ? new Color(0f, 1f, 1f, 0.3f) : new Color(0.2f, 0.2f, 0.3f, 0.8f);
            var outline = toggle.GetComponent<Outline>();
            if (outline != null) outline.effectColor = active ? new Color(0f, 1f, 1f, 1f) : new Color(0.3f, 0.3f, 0.4f, 0.5f);
            var text = toggle.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.color = active ? Color.white : new Color(0.5f, 0.5f, 0.6f, 1f);
        }

        private void SetToggleDefault(Toggle toggle)
        {
            if (toggle != null)
            {
                toggle.isOn = true;
                SetRadio(toggle);
            }
        }

        private void ShowSettingsPanel()
        {
            if (settingsPanel != null) settingsPanel.SetActive(true);
            SetButtonInteractable(false);
        }

        private void OnStartGameClicked()
        {
            // Leer rondas del toggle activo
            if (toggleRounds1 != null && toggleRounds1.isOn) totalAttempts = 1;
            else if (toggleRounds3 != null && toggleRounds3.isOn) totalAttempts = 3;
            else if (toggleRounds10 != null && toggleRounds10.isOn) totalAttempts = 10;
            else totalAttempts = 5;

            if (settingsPanel != null) settingsPanel.SetActive(false);
            StartGameWithCountdown();
        }

        #endregion

        #region Countdown

        private void StartGameWithCountdown()
        {
            SetButtonInteractable(false);

            if (useCountdown && countdownUI != null)
            {
                // Mostrar boton naranja deshabilitado durante countdown
                if (button3D != null)
                {
                    button3D.SetState(FlashTapButton3D.ButtonState.Wait, true);
                    button3D.SetInputEnabled(false);
                }
                countdownUI.StartCountdown(OnCountdownComplete);
            }
            else
            {
                StartGame();
            }
        }

        private void OnCountdownComplete()
        {
            StartGame();
        }

        #endregion

        #region Game Flow

        public override void StartGame()
        {
            base.StartGame();
            currentAttempt = 1;
            reactionTimes.Clear();
            bestTime = float.MaxValue;
            penaltyTime = 0f;
            UpdateUI();
            StartWaitPhase();
        }

        private void StartWaitPhase()
        {
            isWaiting = true;
            isSignalActive = false;

            if (button3D != null)
            {
                button3D.SetState(FlashTapButton3D.ButtonState.Wait);
                button3D.SetInputEnabled(true);
            }

            if (instructionText != null) instructionText.text = AutoLocalizer.Get("flashtap_wait");
            if (reactionTimeText != null) reactionTimeText.text = "";

            // Espera aleatoria 3-6 segundos
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            waitCoroutine = StartCoroutine(WaitAndShowSignal(waitTime));
        }

        private IEnumerator WaitAndShowSignal(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);

            // Boton pasa de naranja a rojo - AHORA!
            isWaiting = false;
            isSignalActive = true;
            signalStartTime = Time.time;

            if (button3D != null)
            {
                button3D.SetState(FlashTapButton3D.ButtonState.Ready);
            }

            if (instructionText != null) instructionText.text = AutoLocalizer.Get("flashtap_tap");

            if (enableHaptics && FeedbackManager.Instance != null)
            {
                FeedbackManager.Instance.PlayHaptic(FeedbackManager.HapticType.Medium);
            }
        }

        private void OnTap()
        {
            if (!isPlaying || isPaused) return;

            if (isWaiting)
            {
                OnTooEarly();
            }
            else if (isSignalActive)
            {
                OnValidTap();
            }
        }

        private void OnTooEarly()
        {
            if (waitCoroutine != null)
            {
                StopCoroutine(waitCoroutine);
                waitCoroutine = null;
            }

            RegisterError();
            penaltyTime += penaltyDuration;

            // Desactivar input inmediatamente
            if (button3D != null)
                button3D.SetInputEnabled(false);

            StartCoroutine(TooEarlySequence());
        }

        private IEnumerator TooEarlySequence()
        {
            // Dejar ver la animación de press (Orange Down) ~0.4s
            yield return new WaitForSeconds(0.4f);

            // Cambiar a estado Error (rojo + shake) ~0.8s
            if (button3D != null)
                button3D.SetState(FlashTapButton3D.ButtonState.Error);

            if (instructionText != null) instructionText.text = AutoLocalizer.Get("flashtap_too_early");
            ShowFeedback(false);

            yield return new WaitForSeconds(0.8f);

            // Reiniciar con countdown
            StartGameWithCountdown();
        }

        private void OnValidTap()
        {
            isSignalActive = false;

            // Capturar tiempo de reacción INMEDIATAMENTE para precisión
            float reactionTime = (Time.time - signalStartTime) * 1000f;
            reactionTimes.Add(reactionTime);

            if (reactionTime < bestTime)
                bestTime = reactionTime;

            // Desactivar input inmediatamente
            if (button3D != null)
                button3D.SetInputEnabled(false);

            StartCoroutine(ValidTapSequence(reactionTime));
        }

        private IEnumerator ValidTapSequence(float reactionTime)
        {
            // Dejar ver la animación de press (Red Down) ~0.4s
            yield return new WaitForSeconds(0.4f);

            // Ahora cambiar a verde (Success)
            if (button3D != null)
                button3D.SetState(FlashTapButton3D.ButtonState.Success);

            // Mostrar tiempo de reacción con color según rendimiento
            if (reactionTimeText != null)
            {
                string timeText = $"{reactionTime:F0}ms";

                if (reactionTime < 200)
                {
                    timeText += $" {AutoLocalizer.Get("flashtap_incredible")}";
                    reactionTimeText.color = new Color(0f, 1f, 0.5f);
                }
                else if (reactionTime < 300)
                {
                    timeText += $" {AutoLocalizer.Get("flashtap_great")}";
                    reactionTimeText.color = new Color(0.5f, 1f, 0.5f);
                }
                else if (reactionTime < 400)
                {
                    timeText += $" {AutoLocalizer.Get("flashtap_good")}";
                    reactionTimeText.color = new Color(1f, 1f, 0.5f);
                }
                else
                {
                    reactionTimeText.color = new Color(1f, 0.5f, 0.5f);
                }

                reactionTimeText.text = timeText;
            }

            // Feedback correcto
            ShowFeedback(true);

            // Feedback haptic + partículas
            if (enableSuccessParticles && FeedbackManager.Instance != null)
            {
                if (button3D != null)
                    FeedbackManager.Instance.PlaySuccessFeedback(button3D.transform.position);
                if (enableHaptics)
                    FeedbackManager.Instance.PlayHaptic(FeedbackManager.HapticType.Light);
            }

            currentAttempt++;
            UpdateUI();

            if (currentAttempt > totalAttempts)
            {
                StartCoroutine(DelayedEndGame());
            }
            else
            {
                StartCoroutine(NextRoundWithCountdown());
            }
        }

        private IEnumerator DelayedEndGame()
        {
            yield return new WaitForSeconds(delayBetweenAttempts);
            EndGame();
        }

        private IEnumerator NextRoundWithCountdown()
        {
            yield return new WaitForSeconds(delayBetweenAttempts);

            if (useCountdown && countdownUI != null)
            {
                if (button3D != null)
                {
                    button3D.SetState(FlashTapButton3D.ButtonState.Wait, true);
                    button3D.SetInputEnabled(false);
                }
                countdownUI.StartCountdown(() => StartWaitPhase());
            }
            else
            {
                StartWaitPhase();
            }
        }

        #endregion

        #region Feedback

        private void ShowFeedback(bool correct)
        {
            if (feedbackPanel == null || feedbackText == null) return;

            if (feedbackCoroutine != null)
                StopCoroutine(feedbackCoroutine);

            feedbackText.text = correct
                ? AutoLocalizer.Get("flashtap_correct")
                : AutoLocalizer.Get("flashtap_incorrect_penalty");
            feedbackText.color = correct
                ? new Color(0.2f, 1f, 0.4f)
                : new Color(1f, 0.3f, 0.3f);

            feedbackCoroutine = StartCoroutine(AnimateFeedback());
        }

        private IEnumerator AnimateFeedback()
        {
            feedbackPanel.SetActive(true);
            CanvasGroup cg = feedbackPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = feedbackPanel.AddComponent<CanvasGroup>();

            // Pop-in
            cg.alpha = 0f;
            float elapsed = 0f;
            while (elapsed < 0.15f)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Lerp(0f, 1f, elapsed / 0.15f);
                yield return null;
            }
            cg.alpha = 1f;

            // Hold
            yield return new WaitForSeconds(0.6f);

            // Fade out
            elapsed = 0f;
            while (elapsed < 0.25f)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Lerp(1f, 0f, elapsed / 0.25f);
                yield return null;
            }

            cg.alpha = 0f;
            feedbackPanel.SetActive(false);
        }

        #endregion

        #region UI & Stats

        private float CalculateAverage()
        {
            if (reactionTimes.Count == 0) return 0;
            float sum = 0;
            foreach (float time in reactionTimes)
                sum += time;
            return sum / reactionTimes.Count;
        }

        private void UpdateUI()
        {
            if (roundText != null)
                roundText.text = $"{Mathf.Min(currentAttempt, totalAttempts)}/{totalAttempts}";

            if (errorsText != null)
                errorsText.text = errorCount.ToString();

            if (roundIndicatorText != null)
                roundIndicatorText.text = $"{Mathf.Min(currentAttempt, totalAttempts)}/{totalAttempts}";

            if (progressFill != null)
            {
                progressFill.transform.parent.parent.gameObject.SetActive(totalAttempts > 1);
                float progress = (float)(currentAttempt - 1) / totalAttempts;
                progressFill.anchorMax = new Vector2(progress, 1f);
            }
        }

        private string GetPerformanceRating(float avgMs)
        {
            if (avgMs < 200) return AutoLocalizer.Get("flashtap_rating_excellent");
            if (avgMs < 250) return AutoLocalizer.Get("flashtap_rating_very_good");
            if (avgMs < 300) return AutoLocalizer.Get("flashtap_rating_good");
            if (avgMs < 350) return AutoLocalizer.Get("flashtap_rating_ok");
            if (avgMs < 400) return AutoLocalizer.Get("flashtap_rating_average");
            return AutoLocalizer.Get("flashtap_rating_practice");
        }

        private void SetButtonInteractable(bool interactable)
        {
            if (tapButton != null) tapButton.interactable = interactable;
            if (button3D != null) button3D.SetInputEnabled(interactable);
        }

        #endregion

        #region MinigameBase Overrides

        public override void EndGame()
        {
            // Usar mejor ronda individual (mínimo), no promedio.
            // Si el jugador hace 10 rondas y una sale en 180ms, ese 180ms compite en el ranking.
            float bestReaction = reactionTimes.Count > 0 ? reactionTimes.Min() : 0f;
            currentResult.TotalTime = bestReaction / 1000f;
            currentResult.PenaltyTime = penaltyTime;
            currentResult.ExtraData = string.Join(",", reactionTimes);

            base.EndGame();
        }

        protected override void UpdateTimer()
        {
            // Flash Tap no usa timer tradicional
        }

        protected override void OnGameStarted()
        {
            SetButtonInteractable(true);
            if (feedbackPanel != null) feedbackPanel.SetActive(false);
        }

        protected override void OnGamePaused()
        {
            if (waitCoroutine != null) StopCoroutine(waitCoroutine);
        }

        protected override void OnGameResumed()
        {
            if (isWaiting) StartWaitPhase();
        }

        protected override void OnGameEnded()
        {
            SetButtonInteractable(false);

            if (enableSuccessParticles && FeedbackManager.Instance != null)
            {
                FeedbackManager.Instance.PlayImportantFeedback(transform.position);
            }
        }

        protected override void ShowResultPanel(MinigameResult result)
        {
            base.ShowResultPanel(result);
            PopulateExtraStats();
        }

        private void PopulateExtraStats()
        {
            float avg = CalculateAverage();

            // Win panel normal
            if (winPanelNormal != null)
            {
                FindAndSetStatText(winPanelNormal.transform, "AvgTimeValue", $"{avg:F0}ms");
                FindAndSetStatText(winPanelNormal.transform, "BestTimeValue", bestTime < float.MaxValue ? $"{bestTime:F0}ms" : "---");
                FindAndSetStatText(winPanelNormal.transform, "ErrorsValue", $"{errorCount}");
                FindAndSetStatText(winPanelNormal.transform, "PenaltyValue", penaltyTime > 0 ? $"{penaltyTime:F0}s" : "0s");
                FindAndSetStatText(winPanelNormal.transform, "RatingValue", GetPerformanceRating(avg));
            }

            // Lose panel normal
            if (losePanelNormal != null)
            {
                FindAndSetStatText(losePanelNormal.transform, "AvgTimeValue", $"{avg:F0}ms");
                FindAndSetStatText(losePanelNormal.transform, "BestTimeValue", bestTime < float.MaxValue ? $"{bestTime:F0}ms" : "---");
                FindAndSetStatText(losePanelNormal.transform, "ErrorsValue", $"{errorCount}");
                FindAndSetStatText(losePanelNormal.transform, "PenaltyValue", penaltyTime > 0 ? $"{penaltyTime:F0}s" : "0s");
            }
        }

        private void FindAndSetStatText(Transform parent, string name, string value)
        {
            Transform t = FindChildDeep(parent, name);
            if (t != null)
            {
                var tmp = t.GetComponent<TextMeshProUGUI>();
                if (tmp != null) tmp.text = value;
            }
        }

        private Transform FindChildDeep(Transform parent, string name)
        {
            if (parent == null) return null;
            if (parent.name == name) return parent;
            foreach (Transform child in parent)
            {
                Transform result = FindChildDeep(child, name);
                if (result != null) return result;
            }
            return null;
        }

        protected override void OnGameReset()
        {
            currentAttempt = 1;
            reactionTimes.Clear();
            bestTime = float.MaxValue;
            penaltyTime = 0f;
            isWaiting = false;
            isSignalActive = false;

            if (feedbackPanel != null) feedbackPanel.SetActive(false);
            SetButtonInteractable(true);

            if (button3D != null)
                button3D.SetState(FlashTapButton3D.ButtonState.Wait, true);

            if (reactionTimeText != null) reactionTimeText.text = "";

            // Volver a settings en practice mode
            if (IsPracticeMode())
            {
                ShowSettingsPanel();
            }
            else
            {
                StartGameWithCountdown();
            }
        }

        protected override void OnErrorOccurred()
        {
            // Ya manejado en OnTooEarly
        }

        #endregion
    }
}
