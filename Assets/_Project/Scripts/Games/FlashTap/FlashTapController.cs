using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.UI;
using DigitPark.Effects;

namespace DigitPark.Games
{
    /// <summary>
    /// Controller para el juego Flash Tap
    /// El jugador debe reaccionar a una senal visual lo mas rapido posible
    /// Usa el boton 3D con sprites Up/Down para mejor feedback visual
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
        [SerializeField] private TextMeshProUGUI averageText;
        [SerializeField] private TextMeshProUGUI bestTimeText;
        [SerializeField] private GameObject winPanel;

        [Header("Flash Tap - 3D Button Sprites")]
        [SerializeField] private Sprite buttonUpSprite;
        [SerializeField] private Sprite buttonDownSprite;

        [Header("Flash Tap - Settings")]
        [SerializeField] private int totalAttempts = 5;
        [SerializeField] private float minWaitTime = 2.5f;
        [SerializeField] private float maxWaitTime = 6f;
        [SerializeField] private float restartDelayAfterError = 1.2f;
        [SerializeField] private float delayBetweenAttempts = 1.5f;

        [Header("Flash Tap - Feedback")]
        [SerializeField] private bool enableSuccessParticles = true;
        [SerializeField] private bool enableHaptics = true;

        // Estado del juego
        private int currentAttempt;
        private List<float> reactionTimes = new List<float>();
        private bool isWaiting;
        private bool isSignalActive;
        private float signalStartTime;
        private float bestTime = float.MaxValue;
        private Coroutine waitCoroutine;

        protected override void Awake()
        {
            base.Awake();
            totalAttempts = config?.rounds ?? 5;
        }

        protected override void Start()
        {
            base.Start();

            // Configurar el boton 3D si existe
            if (button3D != null)
            {
                button3D.OnButtonPressed += OnTap;

                // Asignar sprites si estan configurados
                if (buttonUpSprite != null && buttonDownSprite != null)
                {
                    button3D.SetSprites(buttonUpSprite, buttonDownSprite);
                }
            }
            // Fallback al boton normal
            else if (tapButton != null)
            {
                tapButton.onClick.AddListener(OnTap);
            }
        }

        private void OnDestroy()
        {
            if (button3D != null)
            {
                button3D.OnButtonPressed -= OnTap;
            }
        }

        public override void StartGame()
        {
            base.StartGame();
            currentAttempt = 1;
            reactionTimes.Clear();
            bestTime = float.MaxValue;
            UpdateUI();
            StartWaitPhase();
        }

        private void StartWaitPhase()
        {
            isWaiting = true;
            isSignalActive = false;

            // Visual - usar boton 3D si existe
            if (button3D != null)
            {
                button3D.SetState(FlashTapButton3D.ButtonState.Wait);
            }

            if (instructionText != null) instructionText.text = "ESPERA...";
            if (reactionTimeText != null) reactionTimeText.text = "";

            // Iniciar espera aleatoria (2.5 a 6 segundos para mayor impredecibilidad)
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            waitCoroutine = StartCoroutine(WaitAndShowSignal(waitTime));
        }

        private IEnumerator WaitAndShowSignal(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);

            // Mostrar senal - AHORA!
            isWaiting = false;
            isSignalActive = true;
            signalStartTime = Time.time;

            // Visual - activar estado Ready (rojo)
            if (button3D != null)
            {
                button3D.SetState(FlashTapButton3D.ButtonState.Ready);
            }

            if (instructionText != null) instructionText.text = "TAP!";

            // Haptic feedback cuando se activa
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

            // Visual feedback - estado Error
            if (button3D != null)
            {
                button3D.SetState(FlashTapButton3D.ButtonState.Error);
            }

            if (instructionText != null) instructionText.text = "MUY PRONTO!";

            // Este intento no cuenta, reiniciar fase de espera
            StartCoroutine(RestartAfterTooEarly());
        }

        private IEnumerator RestartAfterTooEarly()
        {
            yield return new WaitForSeconds(restartDelayAfterError);
            StartWaitPhase();
        }

        private void OnValidTap()
        {
            isSignalActive = false;

            // Calcular tiempo de reaccion
            float reactionTime = (Time.time - signalStartTime) * 1000f; // En milisegundos
            reactionTimes.Add(reactionTime);

            // Actualizar mejor tiempo
            if (reactionTime < bestTime)
            {
                bestTime = reactionTime;
            }

            // Mostrar resultado con color segun rendimiento
            if (reactionTimeText != null)
            {
                string timeText = $"{reactionTime:F0}ms";

                // Agregar indicador de rendimiento
                if (reactionTime < 200)
                {
                    timeText += " INCREIBLE!";
                    reactionTimeText.color = new Color(0f, 1f, 0.5f); // Verde brillante
                }
                else if (reactionTime < 300)
                {
                    timeText += " Genial!";
                    reactionTimeText.color = new Color(0.5f, 1f, 0.5f); // Verde
                }
                else if (reactionTime < 400)
                {
                    timeText += " Bien";
                    reactionTimeText.color = new Color(1f, 1f, 0.5f); // Amarillo
                }
                else
                {
                    reactionTimeText.color = new Color(1f, 0.5f, 0.5f); // Rojo suave
                }

                reactionTimeText.text = timeText;
            }

            // Feedback de exito
            if (enableSuccessParticles && FeedbackManager.Instance != null)
            {
                if (button3D != null)
                {
                    FeedbackManager.Instance.PlaySuccessFeedback(button3D.transform.position);
                }

                // Haptic de exito
                if (enableHaptics)
                {
                    FeedbackManager.Instance.PlayHaptic(FeedbackManager.HapticType.Light);
                }
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
            // Breve pausa para que el usuario vea su tiempo
            yield return new WaitForSeconds(delayBetweenAttempts);
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
                roundText.text = $"Ronda {Mathf.Min(currentAttempt, totalAttempts)}/{totalAttempts}";
            }

            if (averageText != null && reactionTimes.Count > 0)
            {
                averageText.text = $"Promedio: {CalculateAverage():F0}ms";
            }

            if (bestTimeText != null && bestTime < float.MaxValue)
            {
                bestTimeText.text = $"Mejor: {bestTime:F0}ms";
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
                float avg = CalculateAverage();
                string rating = GetPerformanceRating(avg);
                averageText.text = $"Promedio Final: {avg:F0}ms\n{rating}";
            }

            // Feedback final
            if (enableSuccessParticles && FeedbackManager.Instance != null)
            {
                FeedbackManager.Instance.PlayImportantFeedback(transform.position);
            }
        }

        private string GetPerformanceRating(float avgMs)
        {
            if (avgMs < 200) return "REFLEJOS DE RAYO!";
            if (avgMs < 250) return "Excelente!";
            if (avgMs < 300) return "Muy Bien!";
            if (avgMs < 350) return "Bien";
            if (avgMs < 400) return "Normal";
            return "Sigue practicando";
        }

        protected override void OnGameReset()
        {
            currentAttempt = 1;
            reactionTimes.Clear();
            bestTime = float.MaxValue;
            isWaiting = false;
            isSignalActive = false;

            if (winPanel != null) winPanel.SetActive(false);
            if (tapButton != null) tapButton.interactable = true;

            if (button3D != null)
            {
                button3D.SetState(FlashTapButton3D.ButtonState.Wait, true);
            }
        }

        protected override void OnErrorOccurred()
        {
            // Ya manejado en OnTooEarly
        }
    }
}
