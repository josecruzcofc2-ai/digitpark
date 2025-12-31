using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DigitPark.Games
{
    /// <summary>
    /// Controller para el juego Digit Rush
    /// El jugador debe tocar los numeros del 1 al 9 en orden ascendente
    /// </summary>
    public class DigitRushController : MinigameBase
    {
        public override GameType GameType => GameType.DigitRush;

        [Header("Digit Rush - Grid")]
        [SerializeField] private Button[] gridButtons; // 9 botones
        [SerializeField] private TextMeshProUGUI[] buttonTexts; // Textos de los botones

        [Header("Digit Rush - UI")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI targetNumberText;
        [SerializeField] private TextMeshProUGUI errorsText;
        [SerializeField] private GameObject winPanel;

        [Header("Digit Rush - Settings")]
        [SerializeField] private bool descendingMode = false; // 9 a 1 en lugar de 1 a 9

        // Estado del juego
        private int currentTargetNumber;
        private int[] gridNumbers = new int[9];
        private int totalNumbers = 9;

        protected override void Awake()
        {
            base.Awake();
            totalNumbers = config?.TotalGridElements ?? 9;
        }

        protected override void Start()
        {
            base.Start();
            SetupButtons();
        }

        private void SetupButtons()
        {
            if (gridButtons == null || gridButtons.Length != 9)
            {
                Debug.LogError("DigitRush requiere exactamente 9 botones");
                return;
            }

            for (int i = 0; i < gridButtons.Length; i++)
            {
                int index = i; // Captura para closure
                gridButtons[i].onClick.AddListener(() => OnButtonClicked(index));
            }
        }

        public override void StartGame()
        {
            base.StartGame();
            ShuffleNumbers();
            currentTargetNumber = descendingMode ? totalNumbers : 1;
            UpdateUI();
        }

        private void ShuffleNumbers()
        {
            // Crear lista de numeros 1-9
            List<int> numbers = new List<int>();
            for (int i = 1; i <= totalNumbers; i++)
            {
                numbers.Add(i);
            }

            // Fisher-Yates shuffle
            for (int i = numbers.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (numbers[i], numbers[j]) = (numbers[j], numbers[i]);
            }

            // Asignar a grid
            for (int i = 0; i < gridNumbers.Length && i < numbers.Count; i++)
            {
                gridNumbers[i] = numbers[i];
                if (buttonTexts != null && i < buttonTexts.Length)
                {
                    buttonTexts[i].text = numbers[i].ToString();
                }
            }
        }

        private void OnButtonClicked(int buttonIndex)
        {
            if (!isPlaying || isPaused) return;

            int clickedNumber = gridNumbers[buttonIndex];

            if (clickedNumber == currentTargetNumber)
            {
                // Correcto
                OnCorrectNumber(buttonIndex);
            }
            else
            {
                // Error
                RegisterError();
                StartCoroutine(ShakeButton(buttonIndex));
            }
        }

        private void OnCorrectNumber(int buttonIndex)
        {
            // Ocultar o marcar boton
            if (gridButtons[buttonIndex] != null)
            {
                gridButtons[buttonIndex].interactable = false;
                // Opcional: cambiar color o animar
            }

            // Avanzar al siguiente numero
            if (descendingMode)
            {
                currentTargetNumber--;
                if (currentTargetNumber < 1)
                {
                    EndGame();
                    return;
                }
            }
            else
            {
                currentTargetNumber++;
                if (currentTargetNumber > totalNumbers)
                {
                    EndGame();
                    return;
                }
            }

            UpdateUI();
        }

        private IEnumerator ShakeButton(int buttonIndex)
        {
            if (gridButtons[buttonIndex] == null) yield break;

            var rectTransform = gridButtons[buttonIndex].GetComponent<RectTransform>();
            Vector2 originalPos = rectTransform.anchoredPosition;
            float shakeDuration = 0.3f;
            float shakeAmount = 10f;
            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                float x = originalPos.x + Random.Range(-shakeAmount, shakeAmount);
                float y = originalPos.y + Random.Range(-shakeAmount, shakeAmount);
                rectTransform.anchoredPosition = new Vector2(x, y);
                elapsed += Time.deltaTime;
                yield return null;
            }

            rectTransform.anchoredPosition = originalPos;
        }

        private void UpdateUI()
        {
            if (targetNumberText != null)
            {
                targetNumberText.text = currentTargetNumber.ToString();
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

            // Activar todos los botones
            foreach (var btn in gridButtons)
            {
                if (btn != null) btn.interactable = true;
            }
        }

        protected override void OnGamePaused()
        {
            // Opcional: mostrar panel de pausa
        }

        protected override void OnGameResumed()
        {
            // Opcional: ocultar panel de pausa
        }

        protected override void OnGameEnded()
        {
            if (winPanel != null) winPanel.SetActive(true);

            // Desactivar botones
            foreach (var btn in gridButtons)
            {
                if (btn != null) btn.interactable = false;
            }
        }

        protected override void OnGameReset()
        {
            currentTargetNumber = descendingMode ? totalNumbers : 1;
            if (winPanel != null) winPanel.SetActive(false);

            // Resetear botones
            foreach (var btn in gridButtons)
            {
                if (btn != null) btn.interactable = true;
            }
        }

        protected override void OnErrorOccurred()
        {
            UpdateUI();
            // Opcional: reproducir sonido de error
        }
    }
}
