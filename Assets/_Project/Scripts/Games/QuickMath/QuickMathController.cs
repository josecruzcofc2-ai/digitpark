using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.UI;
using DigitPark.Localization;

namespace DigitPark.Games
{
    public enum QuickMathDifficulty { Easy, Normal, Hard }

    [System.Flags]
    public enum QuickMathOperations
    {
        Addition       = 1 << 0,
        Subtraction    = 1 << 1,
        Multiplication = 1 << 2,
        Division       = 1 << 3
    }

    /// <summary>
    /// Controller para el juego Quick Math
    /// Settings panel, division, feedback, difficulty system
    /// </summary>
    public class QuickMathController : MinigameBase
    {
        public override GameType GameType => GameType.QuickMath;

        [Header("QuickMath - Equation Display")]
        [SerializeField] private TextMeshProUGUI problemText;
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
        [SerializeField] private TextMeshProUGUI roundIndicatorText;
        [SerializeField] private CanvasGroup comboCanvasGroup;
        [SerializeField] private RectTransform progressFill;

        [Header("QuickMath - Settings Panel")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Toggle toggleAddition;
        [SerializeField] private Toggle toggleSubtraction;
        [SerializeField] private Toggle toggleMultiplication;
        [SerializeField] private Toggle toggleDivision;
        [SerializeField] private Toggle toggleEasy;
        [SerializeField] private Toggle toggleNormal;
        [SerializeField] private Toggle toggleHard;
        [SerializeField] private Toggle toggleRounds1;
        [SerializeField] private Toggle toggleRounds3;
        [SerializeField] private Toggle toggleRounds5;
        [SerializeField] private Toggle toggleRounds10;
        [SerializeField] private Button startGameButton;
        [SerializeField] private TextMeshProUGUI difficultyDescText;

        [Header("Feedback")]
        [SerializeField] private GameObject feedbackPanel;
        [SerializeField] private TextMeshProUGUI feedbackText;

        [Header("Effects")]
        [SerializeField] private UISparkleEffect sparkleEffect;
        [SerializeField] private bool enableHapticFeedback = true;

        // Settings state
        private QuickMathDifficulty currentDifficulty = QuickMathDifficulty.Normal;
        private QuickMathOperations enabledOperations = QuickMathOperations.Addition | QuickMathOperations.Subtraction;
        private int totalRounds = 10;

        // Game state
        private int currentRound;
        private int correctAnswer;
        private int[] answers = new int[3];
        private int numberA, numberB;
        private string currentOperator;
        private float penaltyTime;

        // Streak/Combo System
        private int currentStreak = 0;
        private int maxStreak = 0;

        // Animation state
        private Coroutine questionPulseCoroutine;
        private Coroutine feedbackCoroutine;

        // Computed properties based on difficulty
        private int MaxNumber
        {
            get
            {
                switch (currentDifficulty)
                {
                    case QuickMathDifficulty.Easy: return 10;
                    case QuickMathDifficulty.Normal: return 25;
                    case QuickMathDifficulty.Hard: return 50;
                    default: return 25;
                }
            }
        }

        private int MaxMultiplicationOperand
        {
            get
            {
                switch (currentDifficulty)
                {
                    case QuickMathDifficulty.Easy: return 5;
                    case QuickMathDifficulty.Normal: return 10;
                    case QuickMathDifficulty.Hard: return 12;
                    default: return 10;
                }
            }
        }

        private int MaxDivisor
        {
            get
            {
                switch (currentDifficulty)
                {
                    case QuickMathDifficulty.Easy: return 5;
                    case QuickMathDifficulty.Normal: return 10;
                    case QuickMathDifficulty.Hard: return 12;
                    default: return 10;
                }
            }
        }

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

            if (IsPracticeMode())
            {
                SetupSettingsPanel();
                ShowSettingsPanel();
            }
        }

        private void SetupButtons()
        {
            if (answerButtons == null || answerButtons.Length == 0)
            {
                Debug.LogError("[QuickMath] Requiere botones de respuesta asignados");
                return;
            }

            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (answerButtons[i] == null) continue;
                int index = i;
                answerButtons[i].onClick.AddListener(() => OnAnswerClicked(index));
            }
        }

        #region Settings Panel

        private void SetupSettingsPanel()
        {
            if (settingsPanel == null) return;

            // Wire difficulty toggles with radio enforcement
            if (toggleEasy != null)
                toggleEasy.onValueChanged.AddListener(on => {
                    UpdateToggleVisual(toggleEasy, on);
                    if (on) { OnDifficultyChanged(QuickMathDifficulty.Easy); SetRadio(toggleEasy, toggleNormal, toggleHard); }
                });
            if (toggleNormal != null)
                toggleNormal.onValueChanged.AddListener(on => {
                    UpdateToggleVisual(toggleNormal, on);
                    if (on) { OnDifficultyChanged(QuickMathDifficulty.Normal); SetRadio(toggleNormal, toggleEasy, toggleHard); }
                });
            if (toggleHard != null)
                toggleHard.onValueChanged.AddListener(on => {
                    UpdateToggleVisual(toggleHard, on);
                    if (on) { OnDifficultyChanged(QuickMathDifficulty.Hard); SetRadio(toggleHard, toggleEasy, toggleNormal); }
                });

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

            // Wire operation toggles with visual updates
            if (toggleAddition != null)
                toggleAddition.onValueChanged.AddListener(on => { ValidateOperations(); UpdateToggleVisual(toggleAddition, on); });
            if (toggleSubtraction != null)
                toggleSubtraction.onValueChanged.AddListener(on => { ValidateOperations(); UpdateToggleVisual(toggleSubtraction, on); });
            if (toggleMultiplication != null)
                toggleMultiplication.onValueChanged.AddListener(on => { ValidateOperations(); UpdateToggleVisual(toggleMultiplication, on); });
            if (toggleDivision != null)
                toggleDivision.onValueChanged.AddListener(on => { ValidateOperations(); UpdateToggleVisual(toggleDivision, on); });

            // Wire start button
            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGameClicked);

            // Set defaults without triggering listener chaos
            SetToggleDefault(toggleAddition, true);
            SetToggleDefault(toggleSubtraction, true);
            SetToggleDefault(toggleMultiplication, false);
            SetToggleDefault(toggleDivision, false);
            SetToggleDefault(toggleEasy, false);
            SetToggleDefault(toggleNormal, true);
            SetToggleDefault(toggleHard, false);
            SetToggleDefault(toggleRounds1, false);
            SetToggleDefault(toggleRounds3, false);
            SetToggleDefault(toggleRounds5, false);
            SetToggleDefault(toggleRounds10, true);

            // Apply initial difficulty
            OnDifficultyChanged(QuickMathDifficulty.Normal);
        }

        private void ShowSettingsPanel()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(true);
            EnableAllButtons(false);
        }

        private void OnDifficultyChanged(QuickMathDifficulty difficulty)
        {
            currentDifficulty = difficulty;

            // Easy: disable multiplication and division toggles
            bool allowAdvanced = difficulty != QuickMathDifficulty.Easy;
            if (toggleMultiplication != null)
            {
                toggleMultiplication.interactable = allowAdvanced;
                if (!allowAdvanced) toggleMultiplication.isOn = false;
            }
            if (toggleDivision != null)
            {
                toggleDivision.interactable = allowAdvanced;
                if (!allowAdvanced) toggleDivision.isOn = false;
            }

            // Update description text
            if (difficultyDescText != null)
            {
                switch (difficulty)
                {
                    case QuickMathDifficulty.Easy:
                        difficultyDescText.text = AutoLocalizer.Get("quickmath_difficulty_easy_desc");
                        break;
                    case QuickMathDifficulty.Normal:
                        difficultyDescText.text = AutoLocalizer.Get("quickmath_difficulty_normal_desc");
                        break;
                    case QuickMathDifficulty.Hard:
                        difficultyDescText.text = AutoLocalizer.Get("quickmath_difficulty_hard_desc");
                        break;
                }
            }
        }

        private void ValidateOperations()
        {
            // Count enabled operations
            int count = 0;
            if (toggleAddition != null && toggleAddition.isOn) count++;
            if (toggleSubtraction != null && toggleSubtraction.isOn) count++;
            if (toggleMultiplication != null && toggleMultiplication.isOn) count++;
            if (toggleDivision != null && toggleDivision.isOn) count++;

            // If none selected, force addition on
            if (count == 0 && toggleAddition != null)
            {
                toggleAddition.isOn = true;
            }
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

        private void ReadSettingsFromUI()
        {
            // Read operations
            enabledOperations = 0;
            if (toggleAddition != null && toggleAddition.isOn)
                enabledOperations |= QuickMathOperations.Addition;
            if (toggleSubtraction != null && toggleSubtraction.isOn)
                enabledOperations |= QuickMathOperations.Subtraction;
            if (toggleMultiplication != null && toggleMultiplication.isOn)
                enabledOperations |= QuickMathOperations.Multiplication;
            if (toggleDivision != null && toggleDivision.isOn)
                enabledOperations |= QuickMathOperations.Division;

            // Read difficulty (already set via OnDifficultyChanged)

            // Read rounds
            if (toggleRounds1 != null && toggleRounds1.isOn)
                totalRounds = 1;
            else if (toggleRounds3 != null && toggleRounds3.isOn)
                totalRounds = 3;
            else if (toggleRounds5 != null && toggleRounds5.isOn)
                totalRounds = 5;
            else
                totalRounds = 10;
        }

        private void OnStartGameClicked()
        {
            ReadSettingsFromUI();

            if (settingsPanel != null)
                settingsPanel.SetActive(false);

            StartGame();
        }

        #endregion

        public override void StartGame()
        {
            base.StartGame();
            currentRound = 1;
            currentStreak = 0;
            maxStreak = 0;
            penaltyTime = 0f;
            UpdateComboDisplay();
            GenerateProblem();
            UpdateUI();
            AnimateNewQuestion();
        }

        private void GenerateProblem()
        {
            // Build list of enabled operations
            var ops = new System.Collections.Generic.List<int>();
            if ((enabledOperations & QuickMathOperations.Addition) != 0) ops.Add(0);
            if ((enabledOperations & QuickMathOperations.Subtraction) != 0) ops.Add(1);
            if ((enabledOperations & QuickMathOperations.Multiplication) != 0) ops.Add(2);
            if ((enabledOperations & QuickMathOperations.Division) != 0) ops.Add(3);

            if (ops.Count == 0) ops.Add(0); // fallback to addition

            int operation = ops[Random.Range(0, ops.Count)];

            switch (operation)
            {
                case 0: // Addition
                    numberA = Random.Range(1, MaxNumber + 1);
                    numberB = Random.Range(1, MaxNumber + 1);
                    correctAnswer = numberA + numberB;
                    currentOperator = "+";
                    break;

                case 1: // Subtraction (no negatives)
                    numberA = Random.Range(2, MaxNumber + 1);
                    numberB = Random.Range(1, numberA);
                    correctAnswer = numberA - numberB;
                    currentOperator = "-";
                    break;

                case 2: // Multiplication
                    numberA = Random.Range(1, MaxMultiplicationOperand + 1);
                    numberB = Random.Range(1, MaxMultiplicationOperand + 1);
                    correctAnswer = numberA * numberB;
                    currentOperator = "\u00D7";
                    break;

                case 3: // Division (integer results, divisor >= 2)
                    int divisor = Random.Range(2, MaxDivisor + 1);
                    int maxQuotient = Mathf.Max(1, MaxNumber / divisor);
                    int quotient = Random.Range(1, maxQuotient + 1);
                    numberA = divisor * quotient;
                    numberB = divisor;
                    correctAnswer = quotient;
                    currentOperator = "\u00F7";
                    break;

                default:
                    numberA = 1; numberB = 1; correctAnswer = 2; currentOperator = "+";
                    break;
            }

            UpdateEquationDisplay();
            GenerateAnswers();
        }

        private void UpdateEquationDisplay()
        {
            if (numberAText != null) numberAText.text = numberA.ToString();
            if (numberBText != null) numberBText.text = numberB.ToString();
            if (operatorText != null) operatorText.text = currentOperator;
            if (questionMarkText != null) questionMarkText.text = "?";

            if (problemText != null)
                problemText.text = $"{numberA} {currentOperator} {numberB} = ?";

            if (questionPulseCoroutine != null)
                StopCoroutine(questionPulseCoroutine);
            questionPulseCoroutine = StartCoroutine(AnimateQuestionMark());
        }

        private IEnumerator AnimateQuestionMark()
        {
            if (questionMarkText == null) yield break;

            Color baseColor = new Color(1f, 0.84f, 0f, 1f);
            float pulseSpeed = 2f;
            float time = 0f;

            while (true)
            {
                time += Time.deltaTime * pulseSpeed;
                float pulse = (Mathf.Sin(time) + 1f) * 0.5f;

                questionMarkText.transform.localScale = Vector3.one * (1f + pulse * 0.1f);

                Color currentColor = Color.Lerp(baseColor, Color.white, pulse * 0.3f);
                questionMarkText.color = currentColor;

                yield return null;
            }
        }

        private void GenerateAnswers()
        {
            int correctIndex = Random.Range(0, 3);
            answers[correctIndex] = correctAnswer;

            // Offset range based on difficulty
            int maxOffset;
            switch (currentDifficulty)
            {
                case QuickMathDifficulty.Easy: maxOffset = 2; break;
                case QuickMathDifficulty.Normal: maxOffset = 3; break;
                case QuickMathDifficulty.Hard: maxOffset = 2; break; // tighter = harder
                default: maxOffset = 3; break;
            }

            for (int i = 0; i < 3; i++)
            {
                if (i == correctIndex) continue;

                int wrongAnswer;
                int attempts = 0;
                do
                {
                    int offset = Random.Range(1, maxOffset + 1);
                    if (Random.value < 0.5f) offset = -offset;

                    wrongAnswer = correctAnswer + offset;
                    attempts++;
                }
                while ((wrongAnswer == correctAnswer || wrongAnswer < 0 ||
                        System.Array.IndexOf(answers, wrongAnswer) != -1) && attempts < 30);

                // Fallback if couldn't find unique answer
                if (wrongAnswer == correctAnswer || wrongAnswer < 0)
                    wrongAnswer = correctAnswer + (i == 0 ? 1 : -1);

                answers[i] = wrongAnswer;
            }

            // Update button displays
            for (int i = 0; i < 3; i++)
            {
                if (answerTexts != null && i < answerTexts.Length && answerTexts[i] != null)
                {
                    answerTexts[i].text = answers[i].ToString();
                }

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
            Debug.Log($"[QuickMath] Correcto! Ronda {currentRound}");

            currentStreak++;
            if (currentStreak > maxStreak) maxStreak = currentStreak;

            if (currentStreak >= 7)
                TriggerHaptic(HapticType.Heavy);
            else if (currentStreak >= 4)
                TriggerHaptic(HapticType.Medium);
            else
                TriggerHaptic(HapticType.Light);

            PlayCorrectParticles(buttonIndex);
            AnimateCorrectButton(buttonIndex);
            UpdateComboDisplay();

            // Show green feedback
            ShowFeedback(AutoLocalizer.Get("quickmath_correct"), new Color(0.3f, 1f, 0.5f, 1f));

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
            Debug.Log($"[QuickMath] Incorrecto en boton {buttonIndex}");

            RegisterError();

            // Track penalty time (+1s per error) - visible en timer
            penaltyTime += 1f;
            currentTime += 1f;
            UpdateTimer();

            currentStreak = 0;
            UpdateComboDisplay();

            TriggerHaptic(HapticType.Error);
            PlayErrorParticles(buttonIndex);
            AnimateErrorButton(buttonIndex);

            // Show red feedback with penalty
            ShowFeedback(AutoLocalizer.Get("quickmath_incorrect"), new Color(1f, 0.3f, 0.3f, 1f));

            // Texto "+1" flotante rojo
            ShowPenaltyText(buttonIndex);
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

            // Pop-in (0.2s)
            float popDuration = 0.2f;
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

            // Hold (0.6s - faster than MemoryPairs)
            yield return new WaitForSeconds(0.6f);

            // Fade out (0.2s)
            float fadeDuration = 0.2f;
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

            if (equationPanel != null)
            {
                StartCoroutine(AnimateEquationIn());
            }
        }

        private IEnumerator AnimateEquationIn()
        {
            Vector3 originalScale = Vector3.one;
            equationPanel.localScale = Vector3.zero;

            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

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
            yield return new WaitForSeconds(0.4f);

            if (equationPanel != null)
            {
                yield return StartCoroutine(AnimateEquationOut());
            }

            GenerateProblem();
            UpdateUI();
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

        private void ShowPenaltyText(int buttonIndex)
        {
            if (answerButtons == null || buttonIndex >= answerButtons.Length || answerButtons[buttonIndex] == null) return;

            RectTransform buttonRT = answerButtons[buttonIndex].GetComponent<RectTransform>();
            if (buttonRT == null) return;

            Canvas canvas = answerButtons[buttonIndex].GetComponentInParent<Canvas>();
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
            tmp.fontSize = 42;
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
            if (comboText != null && comboCanvasGroup != null)
            {
                if (currentStreak >= 2)
                {
                    comboCanvasGroup.gameObject.SetActive(true);
                    comboText.text = $"x{currentStreak} STREAK!";

                    if (currentStreak >= 8)
                        comboText.color = new Color(1f, 0.85f, 0.2f);
                    else if (currentStreak >= 6)
                        comboText.color = new Color(1f, 0.5f, 0.2f);
                    else if (currentStreak >= 4)
                        comboText.color = new Color(0.3f, 1f, 0.5f);
                    else
                        comboText.color = new Color(1f, 0.6f, 0.2f);

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
            EnableAllButtons(false);

            TriggerHaptic(HapticType.Heavy);

            yield return new WaitForSeconds(0.3f);

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

        public override void EndGame()
        {
            // Pre-set penalty on currentResult so base.EndGame() uses our tracked value
            // (base would calculate errorCount * config.errorPenalty, we want our penaltyTime)
            currentResult.PenaltyTime = penaltyTime;
            base.EndGame();
            // Fix penalty that base.EndGame() overwrote
            currentResult.PenaltyTime = penaltyTime;
        }

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

            if (progressFill != null)
            {
                progressFill.transform.parent.parent.gameObject.SetActive(totalRounds > 1);
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
        }

        protected override void OnGamePaused() { }

        protected override void OnGameResumed() { }

        protected override void OnGameEnded()
        {
            if (questionPulseCoroutine != null)
                StopCoroutine(questionPulseCoroutine);

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

            // Try both win and lose panel naming conventions
            var streakVal = FindChildTMP(root, "MaxStreakValue") ?? FindChildTMP(root, "LoseMaxStreakValue");
            if (streakVal != null) streakVal.text = maxStreak.ToString();

            var penaltyVal = FindChildTMP(root, "PenaltyValue") ?? FindChildTMP(root, "LosePenaltyValue");
            if (penaltyVal != null)
                penaltyVal.text = penaltyTime > 0 ? $"+{penaltyTime:F1}s" : "-";

            var roundsVal = FindChildTMP(root, "RoundsValue") ?? FindChildTMP(root, "LoseRoundsValue");
            if (roundsVal != null)
                roundsVal.text = $"{totalRounds}/{totalRounds}";
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
            currentStreak = 0;
            maxStreak = 0;
            penaltyTime = 0f;

            EnableAllButtons(true);
            UpdateComboDisplay();

            if (progressFill != null)
            {
                progressFill.anchorMax = new Vector2(0f, 1f);
            }

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
            if (questionPulseCoroutine != null)
            {
                StopCoroutine(questionPulseCoroutine);
                questionPulseCoroutine = null;
            }
            if (feedbackCoroutine != null)
            {
                StopCoroutine(feedbackCoroutine);
                feedbackCoroutine = null;
            }
        }
    }
}
