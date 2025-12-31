using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DigitPark.Games
{
    /// <summary>
    /// Controller para el juego Quick Math
    /// El jugador debe resolver operaciones matematicas rapidamente
    /// </summary>
    public class QuickMathController : MinigameBase
    {
        public override GameType GameType => GameType.QuickMath;

        [Header("Quick Math - UI")]
        [SerializeField] private TextMeshProUGUI problemText;
        [SerializeField] private Button[] answerButtons; // 3 botones de respuesta
        [SerializeField] private TextMeshProUGUI[] answerTexts;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI roundText;
        [SerializeField] private TextMeshProUGUI errorsText;
        [SerializeField] private GameObject winPanel;

        [Header("Quick Math - Settings")]
        [SerializeField] private int totalRounds = 10;
        [SerializeField] private int maxNumber = 20;
        [SerializeField] private bool includeMultiplication = false;

        // Estado del juego
        private int currentRound;
        private int correctAnswer;
        private int[] answers = new int[3];

        protected override void Awake()
        {
            base.Awake();
            totalRounds = config?.rounds ?? 10;
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
                Debug.LogError("QuickMath requiere exactamente 3 botones de respuesta");
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
            GenerateProblem();
            UpdateUI();
        }

        private void GenerateProblem()
        {
            int a, b;
            int operation = includeMultiplication ? Random.Range(0, 3) : Random.Range(0, 2);
            string operatorSymbol;

            switch (operation)
            {
                case 0: // Suma
                    a = Random.Range(1, maxNumber);
                    b = Random.Range(1, maxNumber);
                    correctAnswer = a + b;
                    operatorSymbol = "+";
                    break;
                case 1: // Resta (asegurar resultado positivo)
                    a = Random.Range(1, maxNumber);
                    b = Random.Range(1, a + 1);
                    correctAnswer = a - b;
                    operatorSymbol = "-";
                    break;
                case 2: // Multiplicacion
                    a = Random.Range(1, 10);
                    b = Random.Range(1, 10);
                    correctAnswer = a * b;
                    operatorSymbol = "x";
                    break;
                default:
                    a = 1; b = 1; correctAnswer = 2; operatorSymbol = "+";
                    break;
            }

            // Mostrar problema
            if (problemText != null)
            {
                problemText.text = $"{a} {operatorSymbol} {b} = ?";
            }

            // Generar respuestas (una correcta, dos incorrectas)
            GenerateAnswers();
        }

        private void GenerateAnswers()
        {
            // Colocar respuesta correcta en posicion aleatoria
            int correctIndex = Random.Range(0, 3);
            answers[correctIndex] = correctAnswer;

            // Generar respuestas incorrectas
            for (int i = 0; i < 3; i++)
            {
                if (i == correctIndex) continue;

                int wrongAnswer;
                int attempts = 0;
                do
                {
                    // Generar respuesta cercana a la correcta
                    int offset = Random.Range(-5, 6);
                    if (offset == 0) offset = Random.Range(0, 2) == 0 ? -1 : 1;
                    wrongAnswer = correctAnswer + offset;
                    attempts++;
                }
                while ((wrongAnswer == correctAnswer || wrongAnswer < 0 ||
                        System.Array.IndexOf(answers, wrongAnswer) != -1) && attempts < 20);

                answers[i] = wrongAnswer;
            }

            // Mostrar respuestas en botones
            for (int i = 0; i < 3; i++)
            {
                if (answerTexts != null && i < answerTexts.Length)
                {
                    answerTexts[i].text = answers[i].ToString();
                }
            }
        }

        private void OnAnswerClicked(int buttonIndex)
        {
            if (!isPlaying || isPaused) return;

            if (answers[buttonIndex] == correctAnswer)
            {
                // Correcto
                OnCorrectAnswer();
            }
            else
            {
                // Error
                RegisterError();
                StartCoroutine(FlashWrongAnswer(buttonIndex));
            }
        }

        private void OnCorrectAnswer()
        {
            currentRound++;

            if (currentRound > totalRounds)
            {
                EndGame();
                return;
            }

            GenerateProblem();
            UpdateUI();
        }

        private IEnumerator FlashWrongAnswer(int buttonIndex)
        {
            if (answerButtons[buttonIndex] == null) yield break;

            var image = answerButtons[buttonIndex].GetComponent<Image>();
            if (image == null) yield break;

            Color originalColor = image.color;
            image.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            image.color = originalColor;
        }

        private void UpdateUI()
        {
            if (roundText != null)
            {
                roundText.text = $"{currentRound}/{totalRounds}";
            }

            if (errorsText != null)
            {
                errorsText.text = errorCount.ToString();
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

            foreach (var btn in answerButtons)
            {
                if (btn != null) btn.interactable = true;
            }
        }

        protected override void OnGamePaused() { }

        protected override void OnGameResumed() { }

        protected override void OnGameEnded()
        {
            if (winPanel != null) winPanel.SetActive(true);

            foreach (var btn in answerButtons)
            {
                if (btn != null) btn.interactable = false;
            }
        }

        protected override void OnGameReset()
        {
            currentRound = 1;
            if (winPanel != null) winPanel.SetActive(false);

            foreach (var btn in answerButtons)
            {
                if (btn != null) btn.interactable = true;
            }
        }

        protected override void OnErrorOccurred()
        {
            UpdateUI();
        }
    }
}
