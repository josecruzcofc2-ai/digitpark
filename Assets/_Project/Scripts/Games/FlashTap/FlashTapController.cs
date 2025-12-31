using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DigitPark.Games
{
    /// <summary>
    /// Controller para el juego Flash Tap
    /// El jugador debe reaccionar a una senal visual lo mas rapido posible
    /// </summary>
    public class FlashTapController : MinigameBase
    {
        public override GameType GameType => GameType.FlashTap;

        [Header("Flash Tap - UI")]
        [SerializeField] private Button tapButton;
        [SerializeField] private Image tapButtonImage;
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private TextMeshProUGUI reactionTimeText;
        [SerializeField] private TextMeshProUGUI roundText;
        [SerializeField] private TextMeshProUGUI averageText;
        [SerializeField] private GameObject winPanel;

        [Header("Flash Tap - Colors")]
        [SerializeField] private Color waitColor = Color.gray;
        [SerializeField] private Color goColor = Color.green;
        [SerializeField] private Color tooEarlyColor = Color.red;

        [Header("Flash Tap - Settings")]
        [SerializeField] private int totalAttempts = 5;
        [SerializeField] private float minWaitTime = 1.5f;
        [SerializeField] private float maxWaitTime = 4f;

        // Estado del juego
        private int currentAttempt;
        private List<float> reactionTimes = new List<float>();
        private bool isWaiting; // Esperando senal
        private bool isSignalActive; // Senal activa, puede tocar
        private float signalStartTime;
        private Coroutine waitCoroutine;

        protected override void Awake()
        {
            base.Awake();
            totalAttempts = config?.rounds ?? 5;
        }

        protected override void Start()
        {
            base.Start();

            if (tapButton != null)
            {
                tapButton.onClick.AddListener(OnTap);
            }
        }

        public override void StartGame()
        {
            base.StartGame();
            currentAttempt = 1;
            reactionTimes.Clear();
            UpdateUI();
            StartWaitPhase();
        }

        private void StartWaitPhase()
        {
            isWaiting = true;
            isSignalActive = false;

            // Visual
            if (tapButtonImage != null) tapButtonImage.color = waitColor;
            if (instructionText != null) instructionText.text = "Espera...";
            if (reactionTimeText != null) reactionTimeText.text = "";

            // Iniciar espera aleatoria
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            waitCoroutine = StartCoroutine(WaitAndShowSignal(waitTime));
        }

        private IEnumerator WaitAndShowSignal(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);

            // Mostrar senal
            isWaiting = false;
            isSignalActive = true;
            signalStartTime = Time.time;

            if (tapButtonImage != null) tapButtonImage.color = goColor;
            if (instructionText != null) instructionText.text = "TAP!";
        }

        private void OnTap()
        {
            if (!isPlaying || isPaused) return;

            if (isWaiting)
            {
                // Toco muy temprano
                OnTooEarly();
            }
            else if (isSignalActive)
            {
                // Toco a tiempo
                OnValidTap();
            }
        }

        private void OnTooEarly()
        {
            // Cancelar espera actual
            if (waitCoroutine != null)
            {
                StopCoroutine(waitCoroutine);
            }

            RegisterError();

            // Visual feedback
            if (tapButtonImage != null) tapButtonImage.color = tooEarlyColor;
            if (instructionText != null) instructionText.text = "Muy pronto!";

            // Este intento no cuenta, reiniciar fase de espera
            StartCoroutine(RestartAfterTooEarly());
        }

        private IEnumerator RestartAfterTooEarly()
        {
            yield return new WaitForSeconds(1f);
            StartWaitPhase();
        }

        private void OnValidTap()
        {
            isSignalActive = false;

            // Calcular tiempo de reaccion
            float reactionTime = (Time.time - signalStartTime) * 1000f; // En milisegundos
            reactionTimes.Add(reactionTime);

            // Mostrar resultado
            if (reactionTimeText != null)
            {
                reactionTimeText.text = $"{reactionTime:F0}ms";
            }

            currentAttempt++;
            UpdateUI();

            if (currentAttempt > totalAttempts)
            {
                EndGame();
            }
            else
            {
                // Siguiente intento despues de breve pausa
                StartCoroutine(NextAttemptAfterDelay());
            }
        }

        private IEnumerator NextAttemptAfterDelay()
        {
            yield return new WaitForSeconds(1.5f);
            StartWaitPhase();
        }

        private float CalculateAverage()
        {
            if (reactionTimes.Count == 0) return 0;

            float sum = 0;
            foreach (float time in reactionTimes)
            {
                sum += time;
            }
            return sum / reactionTimes.Count;
        }

        private void UpdateUI()
        {
            if (roundText != null)
            {
                roundText.text = $"{Mathf.Min(currentAttempt, totalAttempts)}/{totalAttempts}";
            }

            if (averageText != null && reactionTimes.Count > 0)
            {
                averageText.text = $"Promedio: {CalculateAverage():F0}ms";
            }
        }

        public override void EndGame()
        {
            // Para Flash Tap, el score es el promedio de tiempos de reaccion
            float avgReaction = CalculateAverage();
            currentResult.TotalTime = avgReaction / 1000f; // Convertir a segundos para consistencia
            currentResult.ExtraData = string.Join(",", reactionTimes); // Guardar tiempos individuales

            base.EndGame();
        }

        protected override void UpdateTimer()
        {
            // Flash Tap no usa timer tradicional
        }

        protected override void OnGameStarted()
        {
            if (winPanel != null) winPanel.SetActive(false);
            if (tapButton != null) tapButton.interactable = true;
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
            if (winPanel != null) winPanel.SetActive(true);
            if (tapButton != null) tapButton.interactable = false;

            // Mostrar resultado final
            if (averageText != null)
            {
                averageText.text = $"Promedio Final: {CalculateAverage():F0}ms";
            }
        }

        protected override void OnGameReset()
        {
            currentAttempt = 1;
            reactionTimes.Clear();
            isWaiting = false;
            isSignalActive = false;

            if (winPanel != null) winPanel.SetActive(false);
            if (tapButton != null) tapButton.interactable = true;
            if (tapButtonImage != null) tapButtonImage.color = waitColor;
        }

        protected override void OnErrorOccurred()
        {
            // Ya manejado en OnTooEarly
        }
    }
}
