using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DigitPark.Games
{
    /// <summary>
    /// Controller para el juego Odd One Out
    /// El jugador debe encontrar el elemento diferente en una cuadricula
    /// </summary>
    public class OddOneOutController : MinigameBase
    {
        public override GameType GameType => GameType.OddOneOut;

        [Header("Odd One Out - Grid")]
        [SerializeField] private Button[] gridButtons; // 16 botones (4x4)
        [SerializeField] private TextMeshProUGUI[] buttonTexts;
        [SerializeField] private Image[] buttonImages;

        [Header("Odd One Out - UI")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI roundText;
        [SerializeField] private TextMeshProUGUI errorsText;
        [SerializeField] private GameObject winPanel;

        [Header("Odd One Out - Settings")]
        [SerializeField] private int totalRounds = 5;

        // Tipos de diferencia
        private enum DifferenceType
        {
            Number,     // 6 vs 9
            Rotation,   // Elemento rotado
            Color,      // Color diferente
            Size        // Tamano diferente
        }

        // Estado del juego
        private int currentRound;
        private int oddButtonIndex;
        private int gridSize = 16;

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
            if (gridButtons == null || gridButtons.Length < 9)
            {
                Debug.LogError("OddOneOut requiere al menos 9 botones");
                return;
            }

            for (int i = 0; i < gridButtons.Length; i++)
            {
                int index = i;
                gridButtons[i].onClick.AddListener(() => OnButtonClicked(index));
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
            // Seleccionar tipo de diferencia aleatorio
            DifferenceType diffType = (DifferenceType)Random.Range(0, 2); // Por ahora solo Number y Rotation

            // Seleccionar posicion del elemento diferente
            oddButtonIndex = Random.Range(0, gridSize);

            switch (diffType)
            {
                case DifferenceType.Number:
                    GenerateNumberPuzzle();
                    break;
                case DifferenceType.Rotation:
                    GenerateRotationPuzzle();
                    break;
                default:
                    GenerateNumberPuzzle();
                    break;
            }
        }

        private void GenerateNumberPuzzle()
        {
            // Pares confusos: 6/9, 2/5, p/d, b/d, etc.
            string[][] confusablePairs = new string[][]
            {
                new string[] { "6", "9" },
                new string[] { "2", "5" },
                new string[] { "M", "W" },
                new string[] { "b", "d" },
                new string[] { "p", "q" },
                new string[] { "n", "u" }
            };

            // Seleccionar un par aleatorio
            int pairIndex = Random.Range(0, confusablePairs.Length);
            string normalChar = confusablePairs[pairIndex][0];
            string oddChar = confusablePairs[pairIndex][1];

            // Asignar a botones
            for (int i = 0; i < gridSize && i < gridButtons.Length; i++)
            {
                if (buttonTexts != null && i < buttonTexts.Length)
                {
                    buttonTexts[i].text = (i == oddButtonIndex) ? oddChar : normalChar;
                }

                // Resetear rotacion
                if (gridButtons[i] != null)
                {
                    gridButtons[i].transform.rotation = Quaternion.identity;
                    gridButtons[i].interactable = true;
                }
            }
        }

        private void GenerateRotationPuzzle()
        {
            // Simbolos que se ven diferentes rotados
            string[] rotatableSymbols = { "▲", "►", "◄", "◆", "→", "←" };
            string symbol = rotatableSymbols[Random.Range(0, rotatableSymbols.Length)];

            // Angulo de rotacion para el elemento diferente
            float[] rotationAngles = { 90f, 180f, 270f };
            float oddRotation = rotationAngles[Random.Range(0, rotationAngles.Length)];

            for (int i = 0; i < gridSize && i < gridButtons.Length; i++)
            {
                if (buttonTexts != null && i < buttonTexts.Length)
                {
                    buttonTexts[i].text = symbol;
                }

                if (gridButtons[i] != null)
                {
                    float rotation = (i == oddButtonIndex) ? oddRotation : 0f;
                    gridButtons[i].transform.rotation = Quaternion.Euler(0, 0, rotation);
                    gridButtons[i].interactable = true;
                }
            }
        }

        private void OnButtonClicked(int buttonIndex)
        {
            if (!isPlaying || isPaused) return;

            if (buttonIndex == oddButtonIndex)
            {
                // Correcto
                OnCorrectAnswer(buttonIndex);
            }
            else
            {
                // Error
                RegisterError();
                StartCoroutine(FlashWrongButton(buttonIndex));
            }
        }

        private void OnCorrectAnswer(int buttonIndex)
        {
            // Efecto visual de acierto
            StartCoroutine(FlashCorrectButton(buttonIndex));

            currentRound++;

            if (currentRound > totalRounds)
            {
                StartCoroutine(EndGameAfterDelay());
            }
            else
            {
                StartCoroutine(NextRoundAfterDelay());
            }
        }

        private IEnumerator FlashCorrectButton(int buttonIndex)
        {
            if (buttonImages != null && buttonIndex < buttonImages.Length)
            {
                Color originalColor = buttonImages[buttonIndex].color;
                buttonImages[buttonIndex].color = Color.green;
                yield return new WaitForSeconds(0.3f);
                buttonImages[buttonIndex].color = originalColor;
            }
        }

        private IEnumerator FlashWrongButton(int buttonIndex)
        {
            if (buttonImages != null && buttonIndex < buttonImages.Length)
            {
                Color originalColor = buttonImages[buttonIndex].color;
                buttonImages[buttonIndex].color = Color.red;
                yield return new WaitForSeconds(0.2f);
                buttonImages[buttonIndex].color = originalColor;
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

            foreach (var btn in gridButtons)
            {
                if (btn != null) btn.interactable = true;
            }
        }

        protected override void OnGamePaused() { }

        protected override void OnGameResumed() { }

        protected override void OnGameEnded()
        {
            if (winPanel != null) winPanel.SetActive(true);

            foreach (var btn in gridButtons)
            {
                if (btn != null) btn.interactable = false;
            }
        }

        protected override void OnGameReset()
        {
            currentRound = 1;
            if (winPanel != null) winPanel.SetActive(false);

            foreach (var btn in gridButtons)
            {
                if (btn != null)
                {
                    btn.interactable = true;
                    btn.transform.rotation = Quaternion.identity;
                }
            }
        }

        protected override void OnErrorOccurred()
        {
            UpdateUI();
        }
    }
}
