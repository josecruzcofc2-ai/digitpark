using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.UI;
using DigitPark.Localization;
using DigitPark.Animations;

namespace DigitPark.Games
{
    /// <summary>
    /// Controller para el juego Odd One Out
    /// El jugador debe encontrar la diferencia entre DOS cuadriculas 4x4
    /// Con sistema de combos, particulas y vibracion haptica
    /// Settings panel, countdown, feedback, WinPanelController
    /// </summary>
    public class OddOneOutController : MinigameBase
    {
        public override GameType GameType => GameType.OddOneOut;

        [Header("Odd One Out - Grid Izquierda")]
        [SerializeField] private Button[] leftGridButtons;
        [SerializeField] private TextMeshProUGUI[] leftButtonTexts;

        [Header("Odd One Out - Grid Derecha")]
        [SerializeField] private Button[] rightGridButtons;
        [SerializeField] private TextMeshProUGUI[] rightButtonTexts;

        [Header("Odd One Out - UI")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI roundText;
        [SerializeField] private TextMeshProUGUI errorsText;
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private TextMeshProUGUI roundIndicatorText;
        [SerializeField] private RectTransform progressFill;

        [Header("Countdown")]
        [SerializeField] private CountdownUI countdownUI;
        [SerializeField] private bool useCountdown = true;

        [Header("Settings Panel")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Toggle toggleRounds1;
        [SerializeField] private Toggle toggleRounds3;
        [SerializeField] private Toggle toggleRounds5;
        [SerializeField] private Toggle toggleRounds10;
        [SerializeField] private Button startGameButton;

        [Header("Feedback")]
        [SerializeField] private GameObject feedbackPanel;
        [SerializeField] private TextMeshProUGUI feedbackText;

        [Header("Effects")]
        [SerializeField] private UISparkleEffect sparkleEffect;
        [SerializeField] private bool enableHapticFeedback = true;

        [Header("Odd One Out - Settings")]
        [SerializeField] private int totalRounds = 5;

        // Estado del juego
        private int currentRound;
        private int oddButtonIndex;
        private int gridSize = 16;
        private string[] gridDigits;
        private string differentDigit;

        // Combo System
        private int currentCombo = 0;
        private int maxCombo = 0;
        private ComboVisualController comboVisualController;

        // Penalty
        private float penaltyTime;

        // Animation state
        private Coroutine feedbackCoroutine;

        protected override void Awake()
        {
            base.Awake();
            totalRounds = config?.rounds ?? 5;
            gridSize = config?.TotalGridElements ?? 16;

            if (sparkleEffect == null)
            {
                sparkleEffect = FindFirstObjectByType<UISparkleEffect>();
            }
        }

        protected override void Start()
        {
            base.Start();
            SetupButtons();

            // Initialize combo visual controller
            comboVisualController = gameObject.AddComponent<ComboVisualController>();
            Canvas canvas = GetComponentInParent<Canvas>();
            comboVisualController.Initialize(canvas);

            if (IsPracticeMode())
            {
                SetupSettingsPanel();
                ShowSettingsPanel();
            }
            else
            {
                StartGameWithCountdown();
            }
        }

        private void SetupButtons()
        {
            if (leftGridButtons != null)
            {
                for (int i = 0; i < leftGridButtons.Length; i++)
                {
                    int index = i;
                    if (leftGridButtons[i] != null)
                        leftGridButtons[i].onClick.AddListener(() => OnButtonClicked(index, false));
                }
            }

            if (rightGridButtons != null)
            {
                for (int i = 0; i < rightGridButtons.Length; i++)
                {
                    int index = i;
                    if (rightGridButtons[i] != null)
                        rightGridButtons[i].onClick.AddListener(() => OnButtonClicked(index, true));
                }
            }
        }

        #region Settings Panel

        private void SetupSettingsPanel()
        {
            if (settingsPanel == null) return;

            // Wire rounds toggles with radio enforcement
            if (toggleRounds1 != null)
                toggleRounds1.onValueChanged.AddListener(on => {
                    UpdateToggleVisual(toggleRounds1, on);
                    if (on) SetRadio(toggleRounds1, toggleRounds3, toggleRounds5, toggleRounds10);
                });
            if (toggleRounds3 != null)
                toggleRounds3.onValueChanged.AddListener(on => {
                    UpdateToggleVisual(toggleRounds3, on);
                    if (on) SetRadio(toggleRounds3, toggleRounds1, toggleRounds5, toggleRounds10);
                });
            if (toggleRounds5 != null)
                toggleRounds5.onValueChanged.AddListener(on => {
                    UpdateToggleVisual(toggleRounds5, on);
                    if (on) SetRadio(toggleRounds5, toggleRounds1, toggleRounds3, toggleRounds10);
                });
            if (toggleRounds10 != null)
                toggleRounds10.onValueChanged.AddListener(on => {
                    UpdateToggleVisual(toggleRounds10, on);
                    if (on) SetRadio(toggleRounds10, toggleRounds1, toggleRounds3, toggleRounds5);
                });

            // Wire start button
            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGameClicked);

            // Set defaults without triggering listener chaos
            SetToggleDefault(toggleRounds1, false);
            SetToggleDefault(toggleRounds3, false);
            SetToggleDefault(toggleRounds5, true);
            SetToggleDefault(toggleRounds10, false);
        }

        private void ShowSettingsPanel()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(true);
            EnableAllButtons(false);
        }

        private void OnStartGameClicked()
        {
            // Read rounds from toggle
            if (toggleRounds1 != null && toggleRounds1.isOn)
                totalRounds = 1;
            else if (toggleRounds3 != null && toggleRounds3.isOn)
                totalRounds = 3;
            else if (toggleRounds10 != null && toggleRounds10.isOn)
                totalRounds = 10;
            else
                totalRounds = 5;

            if (settingsPanel != null)
                settingsPanel.SetActive(false);

            StartGameWithCountdown();
        }

        private void SetRadio(Toggle selected, params Toggle[] others)
        {
            foreach (var t in others)
            {
                if (t != null && t.isOn)
                {
                    t.SetIsOnWithoutNotify(false);
                    UpdateToggleVisual(t, false);
                }
            }
        }

        private void UpdateToggleVisual(Toggle toggle, bool isOn)
        {
            if (toggle == null) return;
            var bg = toggle.GetComponent<Image>();
            var label = toggle.GetComponentInChildren<TextMeshProUGUI>();
            Color onColor = new Color(0f, 1f, 1f, 1f);
            Color offColor = new Color(0.08f, 0.12f, 0.18f, 1f);
            if (bg != null) bg.color = isOn ? onColor : offColor;
            if (label != null) label.color = isOn ? new Color(0.02f, 0.05f, 0.1f, 1f) : Color.white;
        }

        private void SetToggleDefault(Toggle toggle, bool value)
        {
            if (toggle == null) return;
            toggle.SetIsOnWithoutNotify(value);
            UpdateToggleVisual(toggle, value);
        }

        #endregion

        #region Countdown

        private void StartGameWithCountdown()
        {
            if (useCountdown)
            {
                ShowQuestionMarks();
                EnableAllButtons(false);
                Canvas canvas = GetComponentInParent<Canvas>();
                CountdownAnimator.Play(canvas, OnCountdownComplete);
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

        private void ShowQuestionMarks()
        {
            for (int i = 0; i < gridSize; i++)
            {
                if (leftButtonTexts != null && i < leftButtonTexts.Length && leftButtonTexts[i] != null)
                    leftButtonTexts[i].text = "?";
                if (rightButtonTexts != null && i < rightButtonTexts.Length && rightButtonTexts[i] != null)
                    rightButtonTexts[i].text = "?";
            }
        }

        #endregion

        public override void StartGame()
        {
            base.StartGame();
            currentRound = 1;
            currentCombo = 0;
            maxCombo = 0;
            penaltyTime = 0f;
            GeneratePuzzle();
            UpdateUI();
            AnimateRoundStart();
        }

        private void GeneratePuzzle()
        {
            // Generar digitos aleatorios del 1-9 para cada celda (1-9 mas diferenciables)
            gridDigits = new string[gridSize];
            for (int i = 0; i < gridSize; i++)
            {
                gridDigits[i] = Random.Range(1, 10).ToString();
            }

            // Seleccionar posicion de la diferencia
            oddButtonIndex = Random.Range(0, gridSize);

            // Generar un digito DIFERENTE para esa posicion en el grid derecho
            string originalDigit = gridDigits[oddButtonIndex];
            do
            {
                differentDigit = Random.Range(1, 10).ToString();
            } while (differentDigit == originalDigit);

            // Asignar digitos a ambos grids
            for (int i = 0; i < gridSize; i++)
            {
                // Grid izquierda - todos los digitos originales
                if (leftButtonTexts != null && i < leftButtonTexts.Length && leftButtonTexts[i] != null)
                {
                    leftButtonTexts[i].text = gridDigits[i];
                }

                // Grid derecha - igual que izquierda EXCEPTO en oddButtonIndex
                if (rightButtonTexts != null && i < rightButtonTexts.Length && rightButtonTexts[i] != null)
                {
                    if (i == oddButtonIndex)
                        rightButtonTexts[i].text = differentDigit;
                    else
                        rightButtonTexts[i].text = gridDigits[i];
                }

                // Resetear celdas 3D
                ResetCell(i, false);
                ResetCell(i, true);
            }

            EnableAllButtons(true);
        }

        private void ResetCell(int index, bool isRight)
        {
            Button[] buttons = isRight ? rightGridButtons : leftGridButtons;
            if (buttons != null && index < buttons.Length && buttons[index] != null)
            {
                OddOneOutCell3D cell3D = buttons[index].GetComponent<OddOneOutCell3D>();
                if (cell3D != null)
                {
                    cell3D.ResetToNormal();
                }
            }
        }

        private void AnimateRoundStart()
        {
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    int index = row * 4 + col;
                    float delay = (row + col) * 0.03f;

                    AnimateCellStart(index, false, delay);
                    AnimateCellStart(index, true, delay + 0.02f);
                }
            }
        }

        private void AnimateCellStart(int index, bool isRight, float delay)
        {
            Button[] buttons = isRight ? rightGridButtons : leftGridButtons;
            if (buttons != null && index < buttons.Length && buttons[index] != null)
            {
                OddOneOutCell3D cell3D = buttons[index].GetComponent<OddOneOutCell3D>();
                if (cell3D != null)
                {
                    cell3D.AnimateRoundStart(delay);
                }
            }
        }

        private void EnableAllButtons(bool enable)
        {
            if (leftGridButtons != null)
            {
                foreach (var btn in leftGridButtons)
                {
                    if (btn != null) btn.interactable = enable;
                }
            }

            if (rightGridButtons != null)
            {
                foreach (var btn in rightGridButtons)
                {
                    if (btn != null) btn.interactable = enable;
                }
            }
        }

        private void OnButtonClicked(int buttonIndex, bool isRightGrid)
        {
            if (!isPlaying || isPaused) return;

            // Only the RIGHT grid contains the different digit, so taps on the left grid are always wrong
            bool isCorrect = (buttonIndex == oddButtonIndex && isRightGrid);

            if (isCorrect)
            {
                OnCorrectAnswer(buttonIndex, isRightGrid);
                currentRound++;
                UpdateUI();

                if (currentRound > totalRounds)
                {
                    StartCoroutine(EndGameAfterDelay());
                }
                else
                {
                    StartCoroutine(NextRoundAfterDelay());
                }
            }
            else
            {
                OnWrongAnswer(buttonIndex, isRightGrid);
            }
        }

        private void OnCorrectAnswer(int buttonIndex, bool isRightGrid)
        {
            Debug.Log($"[OddOneOut] Correcto! Ronda {currentRound}");

            // Incrementar combo
            currentCombo++;
            if (currentCombo > maxCombo) maxCombo = currentCombo;

            // Vibracion segun combo
            if (currentCombo >= 4)
                TriggerHaptic(HapticType.Heavy);
            else if (currentCombo >= 2)
                TriggerHaptic(HapticType.Medium);
            else
                TriggerHaptic(HapticType.Light);

            // Particulas de acierto
            PlayCorrectParticles(buttonIndex, isRightGrid);

            // Animar celda correcta
            AnimateCellCorrect(buttonIndex, isRightGrid);

            // Actualizar combo display
            UpdateComboDisplay();
            comboVisualController?.OnComboChanged(currentCombo);

            // Show green feedback
            ShowFeedback(AutoLocalizer.Get("oddoneout_correct"), new Color(0.3f, 1f, 0.5f, 1f));
        }

        private void OnWrongAnswer(int buttonIndex, bool isRightGrid)
        {
            Debug.Log($"[OddOneOut] Incorrecto en posicion {buttonIndex}");

            RegisterError();

            // Penalty time +1s (tracked in penaltyTime only, not currentTime)
            penaltyTime += 1f;
            UpdateTimer();

            // Resetear combo
            comboVisualController?.OnComboReset();
            currentCombo = 0;
            UpdateComboDisplay();

            // Vibracion de error
            TriggerHaptic(HapticType.Error);

            // Particulas de error
            PlayErrorParticles(buttonIndex, isRightGrid);

            // Animar celda error
            AnimateCellError(buttonIndex, isRightGrid);

            // Show red feedback with penalty
            ShowFeedback(AutoLocalizer.Get("oddoneout_incorrect_penalty"), new Color(1f, 0.3f, 0.3f, 1f));

            // Texto "+1" flotante rojo
            ShowPenaltyText(buttonIndex, isRightGrid);
        }

        #region Feedback

        private void ShowFeedback(string message, Color color)
        {
            if (feedbackText == null) return;

            if (feedbackCoroutine != null)
                StopCoroutine(feedbackCoroutine);

            feedbackCoroutine = StartCoroutine(AnimateFeedback(message, color));
        }

        private IEnumerator AnimateFeedback(string message, Color color)
        {
            feedbackText.text = message;
            feedbackText.color = color;

            if (feedbackPanel != null)
            {
                feedbackPanel.SetActive(true);
                var outline = feedbackPanel.GetComponent<Outline>();
                if (outline != null)
                    outline.effectColor = new Color(color.r, color.g, color.b, 0.8f);
            }
            else
            {
                feedbackText.gameObject.SetActive(true);
            }

            Transform t = feedbackPanel != null ? feedbackPanel.transform : feedbackText.transform;
            t.localScale = Vector3.zero;

            // Pop-in (0.15s)
            float popDuration = 0.15f;
            float elapsed = 0f;
            while (elapsed < popDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / popDuration;
                float scale = 1f + 0.3f * Mathf.Sin(progress * Mathf.PI);
                t.localScale = Vector3.one * Mathf.Min(scale, 1.3f) * progress;
                yield return null;
            }
            t.localScale = Vector3.one;

            // Hold (0.6s)
            yield return new WaitForSeconds(0.6f);

            // Fade out (0.25s)
            float fadeDuration = 0.25f;
            elapsed = 0f;
            Color startColor = feedbackText.color;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - (elapsed / fadeDuration);
                feedbackText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                t.localScale = Vector3.one * (0.8f + 0.2f * alpha);
                yield return null;
            }

            if (feedbackPanel != null)
                feedbackPanel.SetActive(false);
            else
                feedbackText.gameObject.SetActive(false);

            feedbackCoroutine = null;
        }

        #endregion

        private void AnimateCellCorrect(int buttonIndex, bool isRightGrid)
        {
            Button[] buttons = isRightGrid ? rightGridButtons : leftGridButtons;
            if (buttons != null && buttonIndex < buttons.Length && buttons[buttonIndex] != null)
            {
                OddOneOutCell3D cell3D = buttons[buttonIndex].GetComponent<OddOneOutCell3D>();
                if (cell3D != null)
                {
                    cell3D.AnimateCorrect(currentCombo);
                }
            }
        }

        private void AnimateCellError(int buttonIndex, bool isRightGrid)
        {
            Button[] buttons = isRightGrid ? rightGridButtons : leftGridButtons;
            if (buttons != null && buttonIndex < buttons.Length && buttons[buttonIndex] != null)
            {
                OddOneOutCell3D cell3D = buttons[buttonIndex].GetComponent<OddOneOutCell3D>();
                if (cell3D != null)
                {
                    cell3D.AnimateError();
                }
            }
        }

        #region Particle Effects

        private void PlayCorrectParticles(int buttonIndex, bool isRightGrid)
        {
            if (sparkleEffect == null) return;

            Vector2 pos = GetCellPosition(buttonIndex, isRightGrid);
            sparkleEffect.PlayMatchSparkles(pos, currentCombo);

            if (currentCombo >= 3)
            {
                sparkleEffect.PlayStarBurst(pos, currentCombo - 1);
            }
        }

        private void PlayErrorParticles(int buttonIndex, bool isRightGrid)
        {
            if (sparkleEffect == null) return;

            Vector2 pos = GetCellPosition(buttonIndex, isRightGrid);
            sparkleEffect.PlayErrorSparkles(pos);
        }

        private Vector2 GetCellPosition(int buttonIndex, bool isRightGrid)
        {
            Button[] buttons = isRightGrid ? rightGridButtons : leftGridButtons;
            if (buttons != null && buttonIndex < buttons.Length && buttons[buttonIndex] != null)
            {
                RectTransform rt = buttons[buttonIndex].GetComponent<RectTransform>();
                if (rt != null)
                {
                    return rt.anchoredPosition;
                }
            }
            return Vector2.zero;
        }

        private void ShowPenaltyText(int buttonIndex, bool isRightGrid)
        {
            Button[] buttons = isRightGrid ? rightGridButtons : leftGridButtons;
            if (buttons == null || buttonIndex >= buttons.Length || buttons[buttonIndex] == null) return;

            RectTransform buttonRT = buttons[buttonIndex].GetComponent<RectTransform>();
            if (buttonRT == null) return;

            Canvas canvas = buttons[buttonIndex].GetComponentInParent<Canvas>();
            if (canvas == null) return;

            StartCoroutine(AnimatePenaltyText(buttonRT, canvas));
        }

        private IEnumerator AnimatePenaltyText(RectTransform buttonRT, Canvas canvas)
        {
            GameObject penaltyObj = new GameObject("PenaltyText");
            penaltyObj.transform.SetParent(canvas.transform, false);

            RectTransform rt = penaltyObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(120, 60);

            TextMeshProUGUI tmp = penaltyObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "+1";
            tmp.fontSize = FontSizes.Subtitle;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(1f, 0.3f, 0.3f, 1f);
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            Outline outline = penaltyObj.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
            outline.effectDistance = new Vector2(2, -2);

            Vector3 buttonWorldPos = buttonRT.position;
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, buttonWorldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform, screenPos, canvas.worldCamera, out Vector2 localPos);
            rt.anchoredPosition = localPos + new Vector2(0, 30f);

            Vector2 startPos = rt.anchoredPosition;

            // Punch scale
            float punchDur = 0.12f;
            float punchElapsed = 0f;
            while (punchElapsed < punchDur)
            {
                punchElapsed += Time.deltaTime;
                float t = punchElapsed / punchDur;
                float scale = t < 0.5f
                    ? Mathf.Lerp(0.3f, 1.4f, t * 2f)
                    : Mathf.Lerp(1.4f, 1f, (t - 0.5f) * 2f);
                rt.localScale = Vector3.one * scale;
                yield return null;
            }
            rt.localScale = Vector3.one;

            // Float up + fade out
            float duration = 1.2f;
            float elapsed = 0f;
            Color startColor = tmp.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                float yOffset = 80f * t * (1f - t * 0.3f);
                rt.anchoredPosition = startPos + new Vector2(0, yOffset);

                if (t > 0.5f)
                {
                    float fadeT = (t - 0.5f) / 0.5f;
                    tmp.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(1f, 0f, fadeT));
                }

                yield return null;
            }

            Destroy(penaltyObj);
        }

        #endregion

        #region Haptic Feedback

        private enum HapticType
        {
            Light,
            Medium,
            Heavy,
            Error
        }

        private void TriggerHaptic(HapticType type)
        {
            if (!enableHapticFeedback) return;

#if UNITY_ANDROID || UNITY_IOS
            switch (type)
            {
                case HapticType.Light:
                    Vibrate(10);
                    break;
                case HapticType.Medium:
                    Vibrate(25);
                    break;
                case HapticType.Heavy:
                    Vibrate(50);
                    break;
                case HapticType.Error:
                    StartCoroutine(ErrorVibratePattern());
                    break;
            }
#endif
        }

        private void Vibrate(long milliseconds)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    AndroidJavaObject vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");

                    if (vibrator != null)
                    {
                        using (AndroidJavaClass vibrationEffect = new AndroidJavaClass("android.os.VibrationEffect"))
                        {
                            AndroidJavaObject effect = vibrationEffect.CallStatic<AndroidJavaObject>(
                                "createOneShot", milliseconds, -1);
                            vibrator.Call("vibrate", effect);
                        }
                    }
                }
            }
            catch (System.Exception)
            {
                Handheld.Vibrate();
            }
#elif UNITY_IOS && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

        private IEnumerator ErrorVibratePattern()
        {
            Vibrate(30);
            yield return new WaitForSeconds(0.1f);
            Vibrate(30);
        }

        #endregion

        #region Combo Display

        private void UpdateComboDisplay()
        {
            if (comboText == null) return;

            if (currentCombo >= 2)
            {
                comboText.gameObject.SetActive(true);
                comboText.text = $"x{currentCombo}";

                if (currentCombo >= 5)
                    comboText.color = new Color(1f, 0.85f, 0.2f); // Dorado
                else if (currentCombo >= 4)
                    comboText.color = new Color(1f, 0.5f, 0.2f); // Naranja
                else if (currentCombo >= 3)
                    comboText.color = new Color(0.3f, 1f, 0.5f); // Verde
                else
                    comboText.color = new Color(0f, 0.9f, 1f); // Cyan
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Victory

        private IEnumerator NextRoundAfterDelay()
        {
            EnableAllButtons(false);
            yield return new WaitForSeconds(0.6f);
            GeneratePuzzle();
            UpdateUI();
            AnimateRoundStart();
        }

        private IEnumerator EndGameAfterDelay()
        {
            EnableAllButtons(false);
            yield return new WaitForSeconds(0.3f);

            TriggerHaptic(HapticType.Heavy);

            StartCoroutine(PlayVictorySequence());
        }

        private IEnumerator PlayVictorySequence()
        {
            float waveDelay = 0.05f;

            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    int index = row * 4 + col;
                    float delay = (row + col) * waveDelay;

                    if (leftGridButtons != null && index < leftGridButtons.Length && leftGridButtons[index] != null)
                    {
                        OddOneOutCell3D cell3D = leftGridButtons[index].GetComponent<OddOneOutCell3D>();
                        if (cell3D != null)
                        {
                            cell3D.PlayVictoryCelebration(delay);
                        }
                    }

                    if (rightGridButtons != null && index < rightGridButtons.Length && rightGridButtons[index] != null)
                    {
                        OddOneOutCell3D cell3D = rightGridButtons[index].GetComponent<OddOneOutCell3D>();
                        if (cell3D != null)
                        {
                            cell3D.PlayVictoryCelebration(delay + 0.02f);
                        }
                    }
                }
            }

            if (sparkleEffect != null)
            {
                sparkleEffect.PlayVictoryConfetti();

                yield return new WaitForSeconds(0.4f);
                sparkleEffect.PlayConfettiExplosion(Vector2.zero);
            }

            yield return new WaitForSeconds(0.6f);

            EndGame();
        }

        #endregion

        public override void EndGame()
        {
            currentResult.PenaltyTime = penaltyTime;
            base.EndGame();
            currentResult.PenaltyTime = penaltyTime;
        }

        private void UpdateUI()
        {
            if (roundText != null)
            {
                roundText.text = $"{Mathf.Min(currentRound, totalRounds)}/{totalRounds}";
            }

            if (errorsText != null)
            {
                errorsText.text = errorCount.ToString();
            }

            if (roundIndicatorText != null)
            {
                roundIndicatorText.text = $"{Mathf.Min(currentRound, totalRounds)}/{totalRounds}";
            }

            if (progressFill != null)
            {
                Transform parent = progressFill.transform.parent;
                if (parent != null && parent.parent != null)
                    parent.parent.gameObject.SetActive(totalRounds > 1);
                float progress = (float)(currentRound - 1) / totalRounds;
                progressFill.anchorMax = new Vector2(progress, 1f);
            }
        }

        protected override void UpdateTimer()
        {
            if (timerText != null)
            {
                timerText.text = GetFormattedTime();
            }
        }

        protected override void OnGameStarted()
        {
            EnableAllButtons(true);
            UpdateComboDisplay();
        }

        protected override void OnGamePaused() { }

        protected override void OnGameResumed() { }

        protected override void OnGameEnded()
        {
            // Only disable buttons - base class ShowResultPanel handles panel display
            EnableAllButtons(false);
        }

        protected override void ShowResultPanel(MinigameResult result)
        {
            base.ShowResultPanel(result);
            PopulateExtraStats(winPanelNormal);
            PopulateExtraStats(losePanelNormal);
        }

        private void PopulateExtraStats(WinPanelController panel)
        {
            if (panel == null || !panel.gameObject.activeSelf) return;

            Transform root = panel.transform;

            var comboVal = FindChildTMP(root, "MaxComboValue") ?? FindChildTMP(root, "LoseMaxComboValue");
            if (comboVal != null) comboVal.text = maxCombo.ToString();

            var penaltyVal = FindChildTMP(root, "PenaltyValue") ?? FindChildTMP(root, "LosePenaltyValue");
            if (penaltyVal != null)
                penaltyVal.text = penaltyTime > 0 ? $"+{penaltyTime:F1}s" : "-";
        }

        private TextMeshProUGUI FindChildTMP(Transform parent, string name)
        {
            if (parent.name == name) return parent.GetComponent<TextMeshProUGUI>();
            foreach (Transform child in parent)
            {
                var found = FindChildTMP(child, name);
                if (found != null) return found;
            }
            return null;
        }

        protected override void OnGameReset()
        {
            currentRound = 1;
            currentCombo = 0;
            maxCombo = 0;
            penaltyTime = 0f;
            EnableAllButtons(true);
            UpdateComboDisplay();

            // In practice mode, show settings panel again
            if (IsPracticeMode() && settingsPanel != null)
            {
                ShowSettingsPanel();
            }
        }

        protected override void OnErrorOccurred()
        {
            UpdateUI();
        }

        private void OnDestroy()
        {
            if (feedbackCoroutine != null)
            {
                StopCoroutine(feedbackCoroutine);
                feedbackCoroutine = null;
            }
        }
    }
}
