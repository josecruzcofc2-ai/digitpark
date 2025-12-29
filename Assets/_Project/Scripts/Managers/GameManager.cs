using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DigitPark.Services.Firebase;
using DigitPark.Data;
using DigitPark.Localization;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager principal del juego
    /// Controla la mecánica core del grid 3x3 y la secuencia de números 1-9
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Grid")]
        [SerializeField] public Button[] gridButtons; // 9 botones del grid 3x3

        [Header("Game UI")]
        [SerializeField] public TextMeshProUGUI timerText;
        [SerializeField] public TextMeshProUGUI bestTimeText;
        [SerializeField] public Button playAgainButton;
        [SerializeField] public Button backButton;

        [Header("Win Message")]
        [SerializeField] public GameObject winMessagePanel;
        [SerializeField] public CanvasGroup winMessageCanvasGroup;
        [SerializeField] public TextMeshProUGUI successText;

        [Header("Premium Banner (Result Screen)")]
        [SerializeField] public GameObject premiumBannerContainer;
        [SerializeField] public Button premiumBannerButton;
        [SerializeField] public TextMeshProUGUI premiumBannerText;

        // Game State
        private int currentTargetNumber = 1; // Número que el jugador debe tocar
        private bool isGameActive = false;
        private bool isTimerStarted = false;

        // Timer
        private float currentTime = 0f;
        private float bestTime = float.MaxValue;

        // Player Data
        private PlayerData currentPlayer;

        // Números asignados a cada botón
        private int[] gridNumbers = new int[9];

        // Coroutines de shake para poder cancelarlas
        private Coroutine[] shakeCoroutines = new Coroutine[9];
        // Posiciones originales guardadas durante el shake
        private Vector2[] shakeOriginalPositions = new Vector2[9];

        // Claves de mensajes de éxito por nivel (para localización)
        // Nivel 1 = MEJOR tiempo (< 1s) - Mensajes SUPER gratificantes con dopamina
        private readonly string[] level1Keys = {
            "msg_godlike_focus", "msg_mind_on_fire", "msg_exceptional_reflexes",
            "msg_neural_perfection", "msg_time_master", "msg_superhuman",
            "msg_unstoppable_force", "msg_legendary_speed", "msg_pure_genius", "msg_absolute_legend"
        };
        // Nivel 2 = Muy bueno (1-2s) - Muy gratificantes
        private readonly string[] level2Keys = {
            "msg_incredible_focus", "msg_blazing_fast", "msg_sharp_mind",
            "msg_impressive_reflexes", "msg_excellent_timing", "msg_on_fire",
            "msg_amazing_speed", "msg_brilliant_play", "msg_stellar_performance", "msg_remarkable"
        };
        // Nivel 3 = Bueno (2-3s) - Gratificantes
        private readonly string[] level3Keys = {
            "msg_great_job", "msg_well_played", "msg_nice_speed", "msg_good_reflexes", "msg_solid_time"
        };
        // Nivel 4 = Decente (3-4s) - Positivos
        private readonly string[] level4Keys = {
            "msg_good_effort", "msg_not_bad", "msg_keep_going", "msg_nice_try", "msg_getting_better"
        };
        // Nivel 5 = Básico (4-5s) - Motivacionales
        private readonly string[] level5Keys = {
            "msg_completed", "msg_done", "msg_finished", "msg_keep_practicing", "msg_you_can_improve"
        };
        // Nivel 6 = No clasifica (5s+) - Apoyo emocional, sin frustración
        private readonly string[] level6Keys = {
            "msg_almost_there", "msg_breathe_continue", "msg_next_will_be_better",
            "msg_dont_give_up", "msg_patience_wins", "msg_every_try_counts",
            "msg_progress_not_perfection", "msg_keep_calm", "msg_believe_yourself", "msg_stay_focused"
        };

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            Debug.Log("[Game] GameManager iniciado");

            // Obtener datos del jugador
            LoadPlayerData();

            // Configurar listeners
            SetupListeners();

            // Iniciar nuevo juego
            StartNewGame();
        }

        private void Update()
        {
            if (isGameActive && isTimerStarted)
            {
                currentTime += Time.deltaTime;
                UpdateTimerDisplay();
            }
        }

        /// <summary>
        /// Carga los datos del jugador actual
        /// </summary>
        private void LoadPlayerData()
        {
            if (AuthenticationService.Instance != null)
            {
                currentPlayer = AuthenticationService.Instance.GetCurrentPlayerData();

                if (currentPlayer != null)
                {
                    // Cargar mejor tiempo del jugador
                    bestTime = currentPlayer.bestTime;
                    UpdateBestTimeDisplay();
                }
            }
        }

        /// <summary>
        /// Configura los listeners de los botones
        /// </summary>
        private void SetupListeners()
        {
            // Configurar listener para cada botón del grid
            for (int i = 0; i < gridButtons.Length; i++)
            {
                int index = i; // Capturar el índice en una variable local
                gridButtons[i].onClick.AddListener(() => OnGridButtonClicked(index));
                shakeCoroutines[i] = null;
            }

            // Botón de jugar de nuevo
            playAgainButton?.onClick.AddListener(StartNewGame);

            // Botón de volver
            backButton?.onClick.AddListener(OnBackButtonClicked);

            // Banner de premium
            premiumBannerButton?.onClick.AddListener(OnPremiumBannerClicked);

            // Suscribirse a cambios de premium
            PremiumManager.OnPremiumStatusChanged += UpdatePremiumBanner;

            // Actualizar estado inicial del banner
            UpdatePremiumBanner();
        }

        private void OnDestroy()
        {
            PremiumManager.OnPremiumStatusChanged -= UpdatePremiumBanner;
        }

        /// <summary>
        /// Actualiza la visibilidad del banner premium
        /// </summary>
        private void UpdatePremiumBanner()
        {
            if (premiumBannerContainer == null) return;

            bool isPremium = PremiumManager.Instance != null && PremiumManager.Instance.IsPremium;

            // Ocultar banner si ya es premium
            premiumBannerContainer.SetActive(!isPremium);

            // Actualizar texto del banner
            if (premiumBannerText != null && !isPremium)
            {
                premiumBannerText.text = $"{AutoLocalizer.Get("premium_feature_no_ads")} + {AutoLocalizer.Get("premium_feature_tournaments")}";
            }
        }

        /// <summary>
        /// Handler del click en el banner premium
        /// </summary>
        private void OnPremiumBannerClicked()
        {
            Debug.Log("[Game] Banner premium clickeado");

            // Buscar canvas y crear panel de premium
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                DigitPark.UI.Panels.PremiumPanelUI.CreateAndShow(canvas.transform);
            }
        }

        /// <summary>
        /// Inicia un nuevo juego
        /// </summary>
        public void StartNewGame()
        {
            Debug.Log("[Game] Iniciando nuevo juego...");

            // Resetear estado
            currentTargetNumber = 1;
            currentTime = 0f;
            isGameActive = true;
            isTimerStarted = false; // El timer comienza al hacer el primer clic

            // Generar números aleatorios
            GenerateRandomNumbers();

            // Asignar números a los botones
            AssignNumbersToButtons();

            // Resetear visuales de botones
            ResetButtonVisuals();

            // Ocultar mensaje de victoria si está visible
            if (winMessagePanel != null)
            {
                winMessagePanel.SetActive(false);
                winMessageCanvasGroup.alpha = 0;
            }

            // Actualizar UI
            UpdateTimerDisplay();
            UpdateBestTimeDisplay();

            Debug.Log("[Game] Nuevo juego iniciado - Secuencia generada");
        }

        /// <summary>
        /// Genera números aleatorios del 1 al 9 sin repetir
        /// </summary>
        private void GenerateRandomNumbers()
        {
            // Crear lista de números 1-9
            List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            // Mezclar aleatoriamente (Fisher-Yates shuffle)
            for (int i = numbers.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);

                int temp = numbers[i];
                numbers[i] = numbers[randomIndex];
                numbers[randomIndex] = temp;
            }

            // Asignar a gridNumbers
            for (int i = 0; i < 9; i++)
            {
                gridNumbers[i] = numbers[i];
            }

            Debug.Log($"[Game] Números generados: {string.Join(", ", gridNumbers)}");
        }

        /// <summary>
        /// Asigna los números generados a los botones del grid
        /// </summary>
        private void AssignNumbersToButtons()
        {
            for (int i = 0; i < gridButtons.Length; i++)
            {
                // Obtener el texto del botón
                TextMeshProUGUI numberText = gridButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (numberText != null)
                {
                    numberText.text = gridNumbers[i].ToString();
                }
            }
        }

        /// <summary>
        /// Resetea los visuales de todos los botones
        /// </summary>
        private void ResetButtonVisuals()
        {
            for (int i = 0; i < gridButtons.Length; i++)
            {
                // Cancelar cualquier shake en progreso y restaurar posición
                if (shakeCoroutines[i] != null)
                {
                    StopCoroutine(shakeCoroutines[i]);
                    shakeCoroutines[i] = null;

                    // Restaurar posición original
                    RectTransform rt = gridButtons[i].GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchoredPosition = shakeOriginalPositions[i];
                    }
                }

                gridButtons[i].interactable = true;

                // Resetear color a normal
                Image buttonImage = gridButtons[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = new Color(0.15f, 0.15f, 0.25f);
                }

                // Resetear color del texto
                TextMeshProUGUI numberText = gridButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (numberText != null)
                {
                    numberText.color = Color.white;
                }
            }
        }

        /// <summary>
        /// Maneja el click en un botón del grid
        /// </summary>
        private void OnGridButtonClicked(int buttonIndex)
        {
            if (!isGameActive) return;

            // Iniciar timer en el primer clic
            if (!isTimerStarted)
            {
                isTimerStarted = true;
                Debug.Log("[Game] Timer iniciado");
            }

            int clickedNumber = gridNumbers[buttonIndex];
            Debug.Log($"[Game] Botón {buttonIndex} clickeado - Número: {clickedNumber} | Esperado: {currentTargetNumber}");

            // Verificar si es el número correcto
            if (clickedNumber == currentTargetNumber)
            {
                // ¡Correcto!
                OnCorrectNumberClicked(buttonIndex);

                // Avanzar al siguiente número
                currentTargetNumber++;

                // Verificar si completó todos los números
                if (currentTargetNumber > 9)
                {
                    OnGameComplete();
                }
            }
            else
            {
                // Número incorrecto
                OnWrongNumberClicked(buttonIndex);
            }
        }

        /// <summary>
        /// Se llamó cuando se clickea el número correcto
        /// </summary>
        private void OnCorrectNumberClicked(int buttonIndex)
        {
            Debug.Log($"[Game] ¡Correcto! Número {gridNumbers[buttonIndex]}");

            // Cambiar visual del botón a verde/deshabilitado
            Image buttonImage = gridButtons[buttonIndex].GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = new Color(0.2f, 0.7f, 0.3f, 0.5f); // Verde semi-transparente
            }

            // Deshabilitar el botón
            gridButtons[buttonIndex].interactable = false;

            // Cambiar color del texto a gris
            TextMeshProUGUI numberText = gridButtons[buttonIndex].GetComponentInChildren<TextMeshProUGUI>();
            if (numberText != null)
            {
                numberText.color = new Color(0.5f, 0.5f, 0.5f);
            }

            // TODO: Reproducir sonido de éxito
            // AudioManager.Instance?.PlaySFX("CorrectNumber");
        }

        /// <summary>
        /// Se llamó cuando se clickea un número incorrecto
        /// </summary>
        private void OnWrongNumberClicked(int buttonIndex)
        {
            Debug.Log($"[Game] ¡Incorrecto! Clickeó {gridNumbers[buttonIndex]}, esperaba {currentTargetNumber}");

            RectTransform rt = gridButtons[buttonIndex].GetComponent<RectTransform>();

            // Cancelar shake anterior si existe y restaurar posición
            if (shakeCoroutines[buttonIndex] != null)
            {
                StopCoroutine(shakeCoroutines[buttonIndex]);
                rt.anchoredPosition = shakeOriginalPositions[buttonIndex];
            }

            // Guardar posición original antes de iniciar shake
            shakeOriginalPositions[buttonIndex] = rt.anchoredPosition;

            // Iniciar nueva animación de shake
            shakeCoroutines[buttonIndex] = StartCoroutine(ShakeButton(buttonIndex));

            // TODO: Reproducir sonido de error
            // AudioManager.Instance?.PlaySFX("WrongNumber");
        }

        /// <summary>
        /// Animación de shake para el botón
        /// </summary>
        private IEnumerator ShakeButton(int buttonIndex)
        {
            RectTransform rt = gridButtons[buttonIndex].GetComponent<RectTransform>();
            Vector2 originalPos = shakeOriginalPositions[buttonIndex]; // Usar posición guardada

            float elapsed = 0f;
            float duration = 0.3f;
            float magnitude = 10f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                rt.anchoredPosition = originalPos + new Vector2(x, y);

                elapsed += Time.deltaTime;
                yield return null;
            }

            rt.anchoredPosition = originalPos; // Restaurar posición original
            shakeCoroutines[buttonIndex] = null;
        }

        /// <summary>
        /// Se llamó cuando se completa el juego (tocó todos los números 1-9 correctamente)
        /// </summary>
        private void OnGameComplete()
        {
            Debug.Log($"[Game] ¡JUEGO COMPLETADO! Tiempo: {currentTime:F3}s");

            isGameActive = false;
            isTimerStarted = false;

            // Verificar si es nuevo récord
            bool isNewRecord = currentTime < bestTime;

            if (isNewRecord)
            {
                bestTime = currentTime;
                Debug.Log($"[Game] ¡NUEVO RÉCORD! {bestTime:F3}s");

                // Guardar mejor tiempo
                SaveBestTime();
            }

            // Guardar la partida en scores (torneos gratuitos/Firebase)
            SaveScoreToDatabase();

            // Mostrar mensaje de victoria con animación
            StartCoroutine(ShowWinMessage());
        }

        /// <summary>
        /// Obtiene un mensaje aleatorio según el tiempo del jugador
        /// </summary>
        private string GetSuccessMessage(float time)
        {
            string[] keys;

            if (time < 1f)
            {
                // Nivel 1 - PERFECTO (menos de 1 segundo) - SUPER gratificante
                keys = level1Keys;
            }
            else if (time < 2f)
            {
                // Nivel 2 - MUY BUENO (1-2 segundos) - Muy gratificante
                keys = level2Keys;
            }
            else if (time < 3f)
            {
                // Nivel 3 - BUENO (2-3 segundos) - Gratificante
                keys = level3Keys;
            }
            else if (time < 4f)
            {
                // Nivel 4 - DECENTE (3-4 segundos) - Positivo
                keys = level4Keys;
            }
            else if (time < 5f)
            {
                // Nivel 5 - BÁSICO (4-5 segundos) - Motivacional
                keys = level5Keys;
            }
            else
            {
                // Nivel 6 - NO CLASIFICA (5+ segundos) - Apoyo emocional
                keys = level6Keys;
            }

            // Seleccionar clave aleatoria
            string selectedKey = keys[Random.Range(0, keys.Length)];
            Debug.Log($"[Game] GetSuccessMessage - Tiempo: {time:F3}s, Key seleccionada: {selectedKey}");

            // Obtener texto traducido del LocalizationManager
            if (LocalizationManager.Instance != null)
            {
                string translatedText = LocalizationManager.Instance.GetText(selectedKey);
                Debug.Log($"[Game] Texto traducido: '{translatedText}'");
                return translatedText;
            }

            Debug.LogWarning("[Game] LocalizationManager.Instance es NULL! Usando key como fallback.");
            // Fallback: retornar la clave si no hay LocalizationManager
            return selectedKey;
        }

        /// <summary>
        /// Muestra el mensaje de victoria con animación fade
        /// </summary>
        private IEnumerator ShowWinMessage()
        {
            Debug.Log($"[Game] ShowWinMessage - winMessagePanel: {winMessagePanel != null}, winMessageCanvasGroup: {winMessageCanvasGroup != null}, successText: {successText != null}");

            if (winMessagePanel == null)
            {
                Debug.LogError("[Game] winMessagePanel es NULL! Asígnalo en el Inspector.");
                yield break;
            }

            if (winMessageCanvasGroup == null)
            {
                Debug.LogError("[Game] winMessageCanvasGroup es NULL! Asígnalo en el Inspector.");
                yield break;
            }

            // Establecer mensaje de éxito según el tiempo
            if (successText != null)
            {
                string message = GetSuccessMessage(currentTime);
                successText.text = message;
                Debug.Log($"[Game] SuccessText establecido: '{message}'");
            }
            else
            {
                Debug.LogWarning("[Game] successText es NULL! El mensaje no se mostrará.");
            }

            winMessagePanel.SetActive(true);
            Debug.Log("[Game] winMessagePanel activado");

            // Fade in (aparecer como fantasma)
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                winMessageCanvasGroup.alpha = alpha;
                elapsed += Time.deltaTime;
                yield return null;
            }

            winMessageCanvasGroup.alpha = 1f;

            // Esperar 1 segundo
            yield return new WaitForSeconds(1f);

            // Fade out (desaparecer como fantasma)
            elapsed = 0f;
            while (elapsed < duration)
            {
                float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                winMessageCanvasGroup.alpha = alpha;
                elapsed += Time.deltaTime;
                yield return null;
            }

            winMessageCanvasGroup.alpha = 0f;
            winMessagePanel.SetActive(false);
        }

        /// <summary>
        /// Guarda el mejor tiempo del jugador
        /// </summary>
        private async void SaveBestTime()
        {
            if (currentPlayer == null) return;

            currentPlayer.bestTime = bestTime;
            await DatabaseService.Instance.SavePlayerData(currentPlayer);

            UpdateBestTimeDisplay();

            Debug.Log($"[Game] Mejor tiempo guardado: {bestTime:F3}s");
        }

        /// <summary>
        /// Guarda el score de esta partida en la base de datos
        /// </summary>
        private async void SaveScoreToDatabase()
        {
            if (currentPlayer == null)
            {
                Debug.LogWarning("[Game] No se puede guardar score - jugador no encontrado");
                return;
            }

            if (DatabaseService.Instance == null)
            {
                Debug.LogWarning("[Game] DatabaseService no disponible");
                return;
            }

            // Añadir score al historial del jugador
            currentPlayer.AddScore(currentTime);

            // Actualizar estadísticas
            currentPlayer.totalGamesPlayed++;
            currentPlayer.totalGamesWon++; // Por ahora todos los juegos completados cuentan como ganados

            // Guardar datos del jugador en Firebase
            await DatabaseService.Instance.SavePlayerData(currentPlayer);

            // Guardar en leaderboards (global y por país)
            await DatabaseService.Instance.SaveScore(
                currentPlayer.userId,
                currentPlayer.username,
                currentTime,
                currentPlayer.countryCode
            );

            // Actualizar score en TODOS los torneos activos del jugador
            await DatabaseService.Instance.UpdateScoreInAllActiveTournaments(
                currentPlayer.userId,
                currentTime
            );

            Debug.Log($"[Game] Score guardado en historial, leaderboards y torneos: {currentTime:F3}s");
        }

        /// <summary>
        /// Actualiza el display del timer
        /// </summary>
        private void UpdateTimerDisplay()
        {
            if (timerText != null)
            {
                timerText.text = $"{currentTime:F3}{AutoLocalizer.Get("seconds_abbr")}";
            }
        }

        /// <summary>
        /// Actualiza el display del mejor tiempo
        /// </summary>
        private void UpdateBestTimeDisplay()
        {
            if (bestTimeText != null)
            {
                if (bestTime < float.MaxValue)
                {
                    string label = AutoLocalizer.Get("best_label");
                    bestTimeText.text = $"{label} {bestTime:F3}{AutoLocalizer.Get("seconds_abbr")}";
                }
                else
                {
                    bestTimeText.text = AutoLocalizer.Get("no_best_time");
                }
            }
        }

        /// <summary>
        /// Maneja el click del botón Back
        /// </summary>
        private void OnBackButtonClicked()
        {
            BackToMenu();
        }

        /// <summary>
        /// Vuelve al menú principal
        /// </summary>
        public void BackToMenu()
        {
            Debug.Log("[Game] Volviendo al menú principal");
            SceneManager.LoadScene("MainMenu");
        }
    }
}
