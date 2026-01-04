using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DigitPark.Games
{
    /// <summary>
    /// Controller para el juego Odd One Out
    /// El jugador debe encontrar la diferencia entre DOS cuadriculas 4x4
    /// Una cuadricula tiene UN elemento diferente
    /// </summary>
    public class OddOneOutController : MinigameBase
    {
        public override GameType GameType => GameType.OddOneOut;

        [Header("Odd One Out - Grid Izquierda")]
        [SerializeField] private Button[] leftGridButtons; // 16 botones (4x4)
        [SerializeField] private TextMeshProUGUI[] leftButtonTexts;
        [SerializeField] private Image[] leftButtonImages;

        [Header("Odd One Out - Grid Derecha")]
        [SerializeField] private Button[] rightGridButtons; // 16 botones (4x4)
        [SerializeField] private TextMeshProUGUI[] rightButtonTexts;
        [SerializeField] private Image[] rightButtonImages;

        [Header("Odd One Out - UI")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI roundText;
        [SerializeField] private TextMeshProUGUI errorsText;
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private GameObject winPanel;

        [Header("Odd One Out - Settings")]
        [SerializeField] private int totalRounds = 5;

        // Estado del juego
        private int currentRound;
        private int oddButtonIndex; // Posicion del elemento diferente
        private bool isDifferenceOnRight; // La diferencia esta en grid derecha?
        private int gridSize = 16;

        // Caracteres para generar puzzles
        private readonly string[] characters = {
            "A", "B", "C", "D", "E", "F", "G", "H", "J", "K", "L", "M",
            "N", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
            "1", "2", "3", "4", "5", "6", "7", "8", "9", "0",
            "★", "●", "■", "▲", "♦", "♥", "♠", "♣"
        };

        // Pares confusos
        private readonly string[][] confusablePairs = new string[][]
        {
            new string[] { "6", "9" },
            new string[] { "O", "0" },
            new string[] { "I", "1" },
            new string[] { "S", "5" },
            new string[] { "B", "8" },
            new string[] { "Z", "2" },
            new string[] { "b", "d" },
            new string[] { "p", "q" },
            new string[] { "n", "u" },
            new string[] { "M", "W" }
        };

        protected override void Awake()
        {
            base.Awake();
            totalRounds = config?.rounds ?? 5;
            gridSize = config?.TotalGridElements ?? 16;
        }

        protected override void Start()
        {
            base.Start();
            SetupButtons();
        }

        private void SetupButtons()
        {
            // Setup grid izquierda
            if (leftGridButtons != null)
            {
                for (int i = 0; i < leftGridButtons.Length; i++)
                {
                    int index = i;
                    if (leftGridButtons[i] != null)
                        leftGridButtons[i].onClick.AddListener(() => OnButtonClicked(index, false));
                }
            }

            // Setup grid derecha
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

        public override void StartGame()
        {
            base.StartGame();
            currentRound = 1;
            GeneratePuzzle();
            UpdateUI();
        }

        private void GeneratePuzzle()
        {
            // Seleccionar un par confuso aleatorio
            int pairIndex = Random.Range(0, confusablePairs.Length);
            string baseChar = confusablePairs[pairIndex][0];
            string differentChar = confusablePairs[pairIndex][1];

            // Seleccionar posicion del elemento diferente
            oddButtonIndex = Random.Range(0, gridSize);

            // Decidir en que grid esta la diferencia
            isDifferenceOnRight = Random.Range(0, 2) == 1;

            // Llenar ambas cuadriculas con el caracter base
            for (int i = 0; i < gridSize; i++)
            {
                // Grid izquierda
                if (leftButtonTexts != null && i < leftButtonTexts.Length && leftButtonTexts[i] != null)
                {
                    if (!isDifferenceOnRight && i == oddButtonIndex)
                        leftButtonTexts[i].text = differentChar;
                    else
                        leftButtonTexts[i].text = baseChar;
                }

                // Grid derecha
                if (rightButtonTexts != null && i < rightButtonTexts.Length && rightButtonTexts[i] != null)
                {
                    if (isDifferenceOnRight && i == oddButtonIndex)
                        rightButtonTexts[i].text = differentChar;
                    else
                        rightButtonTexts[i].text = baseChar;
                }

                // Resetear visuales
                ResetButtonVisual(i, leftButtonImages);
                ResetButtonVisual(i, rightButtonImages);
            }

            // Activar todos los botones
            EnableAllButtons(true);

            // Instruccion
            if (instructionText != null)
            {
                instructionText.text = "Encuentra la diferencia!";
            }
        }

        private void ResetButtonVisual(int index, Image[] images)
        {
            if (images != null && index < images.Length && images[index] != null)
            {
                images[index].color = Color.white;
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

            // Verificar si es el boton correcto
            bool isCorrect = (buttonIndex == oddButtonIndex) && (isRightGrid == isDifferenceOnRight);

            if (isCorrect)
            {
                OnCorrectAnswer(buttonIndex, isRightGrid);
            }
            else
            {
                OnWrongAnswer(buttonIndex, isRightGrid);
            }
        }

        private void OnCorrectAnswer(int buttonIndex, bool isRightGrid)
        {
            // Efecto visual de acierto
            Image[] images = isRightGrid ? rightButtonImages : leftButtonImages;
            StartCoroutine(FlashButton(buttonIndex, images, Color.green));

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

        private void OnWrongAnswer(int buttonIndex, bool isRightGrid)
        {
            RegisterError();

            // Efecto visual de error
            Image[] images = isRightGrid ? rightButtonImages : leftButtonImages;
            StartCoroutine(FlashButton(buttonIndex, images, Color.red));
        }

        private IEnumerator FlashButton(int buttonIndex, Image[] images, Color flashColor)
        {
            if (images != null && buttonIndex < images.Length && images[buttonIndex] != null)
            {
                Color originalColor = images[buttonIndex].color;
                images[buttonIndex].color = flashColor;
                yield return new WaitForSeconds(0.3f);
                images[buttonIndex].color = originalColor;
            }
        }

        private IEnumerator NextRoundAfterDelay()
        {
            yield return new WaitForSeconds(0.5f);
            GeneratePuzzle();
            UpdateUI();
        }

        private IEnumerator EndGameAfterDelay()
        {
            yield return new WaitForSeconds(0.5f);
            EndGame();
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
            if (winPanel != null) winPanel.SetActive(true);
            EnableAllButtons(false);
        }

        protected override void OnGameReset()
        {
            currentRound = 1;
            if (winPanel != null) winPanel.SetActive(false);
            EnableAllButtons(true);
        }

        protected override void OnErrorOccurred()
        {
            UpdateUI();
        }
    }
}
