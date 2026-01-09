using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.UI;

namespace DigitPark.Games
{
    /// <summary>
    /// Controller para el juego Quick Math - REDISEÑO IMPACTANTE
    /// Ecuación gigante, combo system, partículas, haptics
    /// </summary>
    public class QuickMathController : MinigameBase
    {
        public override GameType GameType => GameType.QuickMath;

        [Header("QuickMath - Equation Display")]
        [SerializeField] private TextMeshProUGUI problemText; // Para compatibilidad
        [SerializeField] private TextMeshProUGUI numberAText;
        [SerializeField] private TextMeshProUGUI numberBText;
        [SerializeField] private TextMeshProUGUI operatorText;
        [SerializeField] private TextMeshProUGUI questionMarkText;
        [SerializeField] private RectTransform equationPanel;

        [Header("QuickMath - Answer Buttons")]
        [SerializeField] private Button[] answerButtons;
        [SerializeField] private TextMeshProUGUI[] answerTexts;

        [Header("QuickMath - UI")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI roundText;
        [SerializeField] private TextMeshProUGUI errorsText;
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private TextMeshProUGUI roundIndicatorText;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private CanvasGroup winPanelCanvasGroup;
        [SerializeField] private CanvasGroup comboCanvasGroup;
        [SerializeField] private RectTransform progressFill;

        [Header("Effects")]
        [SerializeField] private UISparkleEffect sparkleEffect;
        [SerializeField] private bool enableHapticFeedback = true;

        [Header("QuickMath - Settings")]
        [SerializeField] private int totalRounds = 10;
        [SerializeField] private int maxNumber = 20;
        [SerializeField] private bool includeMultiplication = false;

        // Estado del juego
        private int currentRound;
        private int correctAnswer;
        private int[] answers = new int[3];
        private int numberA, numberB;
        private string currentOperator;

        // Streak/Combo System
        private int currentStreak = 0;
        private int maxStreak = 0;

        // Animation state
        private Coroutine questionPulseCoroutine;

        protected override void Awake()
        {
            base.Awake();
            totalRounds = config?.rounds ?? 10;

            if (sparkleEffect == null)
            {
                sparkleEffect = FindFirstObjectByType<UISparkleEffect>();
            }
        }

        protected override void Start()
        {
            base.Start();
            SetupButtons();
        }

        private void SetupButtons()
        {
            if (answerButtons == null || answerButtons.Length != 3)
            {
                Debug.LogError("[QuickMath] Requiere exactamente 3 botones de respuesta");
                return;
            }

            for (int i = 0; i < answerButtons.Length; i++)
            {
                int index = i;
                answerButtons[i].onClick.AddListener(() => OnAnswerClicked(index));
            }
        }

        public override void StartGame()
        {
            base.StartGame();
            currentRound = 1;
            currentStreak = 0;
            maxStreak = 0;
            UpdateComboDisplay();
            GenerateProblem();
            UpdateUI();
            AnimateNewQuestion();
        }

        private void GenerateProblem()
        {
            int operation = includeMultiplication ? Random.Range(0, 3) : Random.Range(0, 2);

            switch (operation)
            {
                case 0: // Suma
                    numberA = Random.Range(1, maxNumber);
                    numberB = Random.Range(1, maxNumber);
                    correctAnswer = numberA + numberB;
                    currentOperator = "+";
                    break;
                case 1: // Resta
                    numberA = Random.Range(1, maxNumber);
                    numberB = Random.Range(1, numberA + 1);
                    correctAnswer = numberA - numberB;
                    currentOperator = "-";
                    break;
                case 2: // Multiplicación
                    numberA = Random.Range(1, 10);
                    numberB = Random.Range(1, 10);
                    correctAnswer = numberA * numberB;
                    currentOperator = "×";
                    break;
                default:
                    numberA = 1; numberB = 1; correctAnswer = 2; currentOperator = "+";
                    break;
            }

            // Actualizar displays
            UpdateEquationDisplay();

            // Generar respuestas
            GenerateAnswers();
        }

        private void UpdateEquationDisplay()
        {
            // Textos individuales para animación
            if (numberAText != null) numberAText.text = numberA.ToString();
            if (numberBText != null) numberBText.text = numberB.ToString();
            if (operatorText != null) operatorText.text = currentOperator;
            if (questionMarkText != null) questionMarkText.text = "?";

            // Texto completo para compatibilidad
            if (problemText != null)
                problemText.text = $"{numberA} {currentOperator} {numberB} = ?";

            // Iniciar animación del signo de interrogación
            if (questionPulseCoroutine != null)
                StopCoroutine(questionPulseCoroutine);
            questionPulseCoroutine = StartCoroutine(AnimateQuestionMark());
        }

        private IEnumerator AnimateQuestionMark()
        {
            if (questionMarkText == null) yield break;

            Color baseColor = new Color(1f, 0.84f, 0f, 1f); // Dorado
            float pulseSpeed = 2f;
            float time = 0f;

            while (true)
            {
                time += Time.deltaTime * pulseSpeed;
                float pulse = (Mathf.Sin(time) + 1f) * 0.5f;

                // Pulso de escala
                questionMarkText.transform.localScale = Vector3.one * (1f + pulse * 0.1f);

                // Pulso de brillo
                Color currentColor = Color.Lerp(baseColor, Color.white, pulse * 0.3f);
                questionMarkText.color = currentColor;

                yield return null;
            }
        }

        private void GenerateAnswers()
        {
            int correctIndex = Random.Range(0, 3);
            answers[correctIndex] = correctAnswer;

            for (int i = 0; i < 3; i++)
            {
                if (i == correctIndex) continue;

                int wrongAnswer;
                int attempts = 0;
                do
                {
                    int offset = Random.Range(-5, 6);
                    if (offset == 0) offset = Random.Range(0, 2) == 0 ? -1 : 1;
                    wrongAnswer = correctAnswer + offset;
                    attempts++;
                }
                while ((wrongAnswer == correctAnswer || wrongAnswer < 0 ||
                        System.Array.IndexOf(answers, wrongAnswer) != -1) && attempts < 20);

                answers[i] = wrongAnswer;
            }

            // Actualizar textos de botones
            for (int i = 0; i < 3; i++)
            {
                if (answerTexts != null && i < answerTexts.Length && answerTexts[i] != null)
                {
                    answerTexts[i].text = answers[i].ToString();
                }

                // También actualizar via Cell3D
                if (answerButtons != null && i < answerButtons.Length && answerButtons[i] != null)
                {
                    QuickMathCell3D cell = answerButtons[i].GetComponent<QuickMathCell3D>();
                    if (cell != null)
                    {
                        cell.SetAnswer(answers[i].ToString());
                        cell.ResetToNormal();
                    }
                }
            }
        }

        private void OnAnswerClicked(int buttonIndex)
        {
            if (!isPlaying || isPaused) return;

            if (answers[buttonIndex] == correctAnswer)
            {
                OnCorrectAnswer(buttonIndex);
            }
            else
            {
                OnWrongAnswer(buttonIndex);
            }
        }

        private void OnCorrectAnswer(int buttonIndex)
        {
            Debug.Log($"[QuickMath] ¡Correcto! Ronda {currentRound}");

            // Incrementar streak
            currentStreak++;
            if (currentStreak > maxStreak) maxStreak = currentStreak;

            // Vibración según streak
            if (currentStreak >= 7)
                TriggerHaptic(HapticType.Heavy);
            else if (currentStreak >= 4)
                TriggerHaptic(HapticType.Medium);
            else
                TriggerHaptic(HapticType.Light);

            // Partículas
            PlayCorrectParticles(buttonIndex);

            // Animar botón correcto
            AnimateCorrectButton(buttonIndex);

            // Actualizar combo display
            UpdateComboDisplay();

            // Siguiente ronda
            currentRound++;

            if (currentRound > totalRounds)
            {
                StartCoroutine(EndGameSequence());
            }
            else
            {
                StartCoroutine(NextQuestionSequence());
            }
        }

        private void OnWrongAnswer(int buttonIndex)
        {
            Debug.Log($"[QuickMath] Incorrecto en botón {buttonIndex}");

            RegisterError();

            // Resetear streak
            currentStreak = 0;
            UpdateComboDisplay();

            // Vibración de error
            TriggerHaptic(HapticType.Error);

            // Partículas de error
            PlayErrorParticles(buttonIndex);

            // Animar botón error
            AnimateErrorButton(buttonIndex);
        }

        private void AnimateCorrectButton(int buttonIndex)
        {
            if (answerButtons != null && buttonIndex < answerButtons.Length && answerButtons[buttonIndex] != null)
            {
                QuickMathCell3D cell = answerButtons[buttonIndex].GetComponent<QuickMathCell3D>();
                if (cell != null)
                {
                    cell.AnimateCorrect(currentStreak);
                }
            }
        }

        private void AnimateErrorButton(int buttonIndex)
        {
            if (answerButtons != null && buttonIndex < answerButtons.Length && answerButtons[buttonIndex] != null)
            {
                QuickMathCell3D cell = answerButtons[buttonIndex].GetComponent<QuickMathCell3D>();
                if (cell != null)
                {
                    cell.AnimateError();
                }
            }
        }

        private void AnimateNewQuestion()
        {
            // Animar botones de respuesta con pop-in
            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (answerButtons[i] != null)
                {
                    QuickMathCell3D cell = answerButtons[i].GetComponent<QuickMathCell3D>();
                    if (cell != null)
                    {
                        cell.AnimateNewQuestion(i * 0.08f);
                    }
                }
            }

            // Animar ecuación
            if (equationPanel != null)
            {
                StartCoroutine(AnimateEquationIn());
            }
        }

        private IEnumerator AnimateEquationIn()
        {
            Vector3 originalScale = equationPanel.localScale;
            equationPanel.localScale = Vector3.zero;

            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Ease out back
                float overshoot = 1.2f;
                float s = t - 1f;
                float scale = 1f + s * s * ((overshoot + 1f) * s + overshoot);

                equationPanel.localScale = originalScale * Mathf.Max(0, scale);

                yield return null;
            }

            equationPanel.localScale = originalScale;
        }

        private IEnumerator NextQuestionSequence()
        {
            // Breve pausa
            yield return new WaitForSeconds(0.4f);

            // Animar ecuación saliendo
            if (equationPanel != null)
            {
                yield return StartCoroutine(AnimateEquationOut());
            }

            // Generar nueva pregunta
            GenerateProblem();
            UpdateUI();

            // Animar entrada
            AnimateNewQuestion();
        }

        private IEnumerator AnimateEquationOut()
        {
            if (equationPanel == null) yield break;

            Vector3 originalScale = equationPanel.localScale;
            float duration = 0.15f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                equationPanel.localScale = originalScale * (1f - t);

                yield return null;
            }

            equationPanel.localScale = Vector3.zero;
        }

        #region Particle Effects

        private void PlayCorrectParticles(int buttonIndex)
        {
            if (sparkleEffect == null) return;

            Vector2 pos = GetButtonPosition(buttonIndex);
            sparkleEffect.PlayMatchSparkles(pos, currentStreak);

            if (currentStreak >= 4)
            {
                sparkleEffect.PlayStarBurst(pos, currentStreak - 2);
            }
        }

        private void PlayErrorParticles(int buttonIndex)
        {
            if (sparkleEffect == null) return;

            Vector2 pos = GetButtonPosition(buttonIndex);
            sparkleEffect.PlayErrorSparkles(pos);
        }

        private Vector2 GetButtonPosition(int buttonIndex)
        {
            if (answerButtons != null && buttonIndex < answerButtons.Length && answerButtons[buttonIndex] != null)
            {
                RectTransform rt = answerButtons[buttonIndex].GetComponent<RectTransform>();
                if (rt != null)
                {
                    return rt.anchoredPosition;
                }
            }
            return Vector2.zero;
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
            if (comboText != null && comboCanvasGroup != null)
            {
                if (currentStreak >= 2)
                {
                    comboCanvasGroup.gameObject.SetActive(true);
                    comboText.text = $"x{currentStreak} STREAK!";

                    // Color basado en streak
                    if (currentStreak >= 8)
                        comboText.color = new Color(1f, 0.85f, 0.2f); // Dorado
                    else if (currentStreak >= 6)
                        comboText.color = new Color(1f, 0.5f, 0.2f); // Naranja
                    else if (currentStreak >= 4)
                        comboText.color = new Color(0.3f, 1f, 0.5f); // Verde
                    else
                        comboText.color = new Color(1f, 0.6f, 0.2f); // Naranja claro

                    // Animar entrada
                    StartCoroutine(AnimateComboIn());
                }
                else
                {
                    StartCoroutine(AnimateComboOut());
                }
            }
        }

        private IEnumerator AnimateComboIn()
        {
            if (comboCanvasGroup == null) yield break;

            float duration = 0.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                comboCanvasGroup.alpha = t;
                comboCanvasGroup.transform.localScale = Vector3.one * (0.8f + 0.2f * t);
                yield return null;
            }

            comboCanvasGroup.alpha = 1f;
            comboCanvasGroup.transform.localScale = Vector3.one;
        }

        private IEnumerator AnimateComboOut()
        {
            if (comboCanvasGroup == null) yield break;

            float duration = 0.15f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                comboCanvasGroup.alpha = 1f - t;
                yield return null;
            }

            comboCanvasGroup.alpha = 0f;
            comboCanvasGroup.gameObject.SetActive(false);
        }

        #endregion

        #region Victory

        private IEnumerator EndGameSequence()
        {
            // Deshabilitar botones
            EnableAllButtons(false);

            // Vibración de victoria
            TriggerHaptic(HapticType.Heavy);

            yield return new WaitForSeconds(0.3f);

            // Animación de victoria en botones
            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (answerButtons[i] != null)
                {
                    QuickMathCell3D cell = answerButtons[i].GetComponent<QuickMathCell3D>();
                    if (cell != null)
                    {
                        cell.PlayVictoryCelebration(i * 0.1f);
                    }
                }
            }

            // Confeti
            if (sparkleEffect != null)
            {
                sparkleEffect.PlayVictoryConfetti();
                yield return new WaitForSeconds(0.3f);
                sparkleEffect.PlayConfettiExplosion(Vector2.zero);
            }

            yield return new WaitForSeconds(0.5f);

            EndGame();
        }

        #endregion

        private void EnableAllButtons(bool enable)
        {
            if (answerButtons != null)
            {
                foreach (var btn in answerButtons)
                {
                    if (btn != null) btn.interactable = enable;
                }
            }
        }

        private void UpdateUI()
        {
            if (roundText != null)
            {
                roundText.text = $"{currentRound}/{totalRounds}";
            }

            if (roundIndicatorText != null)
            {
                roundIndicatorText.text = $"{currentRound}/{totalRounds}";
            }

            if (errorsText != null)
            {
                errorsText.text = errorCount.ToString();
            }

            // Actualizar barra de progreso
            if (progressFill != null)
            {
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
            if (winPanel != null) winPanel.SetActive(false);
            EnableAllButtons(true);
        }

        protected override void OnGamePaused() { }

        protected override void OnGameResumed() { }

        protected override void OnGameEnded()
        {
            if (questionPulseCoroutine != null)
                StopCoroutine(questionPulseCoroutine);

            if (winPanel != null)
            {
                winPanel.SetActive(true);
                StartCoroutine(ShowWinPanel());
            }
            EnableAllButtons(false);
        }

        private IEnumerator ShowWinPanel()
        {
            if (winPanelCanvasGroup == null) yield break;

            // Actualizar stats
            if (statsText != null)
            {
                statsText.text = $"Tiempo: {GetFormattedTime()}\nErrores: {errorCount}\nMax Streak: x{maxStreak}";
            }

            // Fade in
            float duration = 0.4f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                winPanelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }

            winPanelCanvasGroup.alpha = 1f;
        }

        protected override void OnGameReset()
        {
            currentRound = 1;
            currentStreak = 0;
            maxStreak = 0;

            if (winPanel != null) winPanel.SetActive(false);
            if (winPanelCanvasGroup != null) winPanelCanvasGroup.alpha = 0;

            EnableAllButtons(true);
            UpdateComboDisplay();

            // Reset progress bar
            if (progressFill != null)
            {
                progressFill.anchorMax = new Vector2(0f, 1f);
            }
        }

        protected override void OnErrorOccurred()
        {
            UpdateUI();
        }
    }
}
