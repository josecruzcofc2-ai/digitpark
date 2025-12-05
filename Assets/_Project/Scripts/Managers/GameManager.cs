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

        // Claves de mensajes de éxito por nivel (para localización)
        private readonly string[] level1Keys = {
            "msg_good_job", "msg_complete", "msg_nice_try", "msg_well_done", "msg_task_complete"
        };
        private readonly string[] level2Keys = {
            "msg_great_work", "msg_good_timing", "msg_not_bad", "msg_solid", "msg_keep_it_up"
        };
        private readonly string[] level3Keys = {
            "msg_excellent", "msg_impressive", "msg_great_speed", "msg_well_played", "msg_awesome"
        };
        private readonly string[] level4Keys = {
            "msg_amazing", "msg_outstanding", "msg_superb", "msg_incredible",
            "msg_spectacular", "msg_on_fire"
        };
        private readonly string[] level5Keys = {
            "msg_perfect", "msg_legendary", "msg_mind_blowing", "msg_master",
            "msg_unstoppable", "msg_world_class", "msg_godlike", "msg_flawless"
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
            }

            // Botón de jugar de nuevo
            playAgainButton?.onClick.AddListener(StartNewGame);

            // Botón de volver
            backButton?.onClick.AddListener(OnBackButtonClicked);
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

            // Animación de error (shake rápido del botón)
            StartCoroutine(ShakeButton(gridButtons[buttonIndex]));

            // TODO: Reproducir sonido de error
            // AudioManager.Instance?.PlaySFX("WrongNumber");
        }

        /// <summary>
        /// Animación de shake para el botón
        /// </summary>
        private IEnumerator ShakeButton(Button button)
        {
            RectTransform rt = button.GetComponent<RectTransform>();
            Vector2 originalPos = rt.anchoredPosition;

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

            rt.anchoredPosition = originalPos;
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

            // Guardar la partida en scores
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
                // Nivel 5 - PERFECTO (menos de 1 segundo)
                keys = level5Keys;
            }
            else if (time < 2f)
            {
                // Nivel 4 - MUY BUENO (menos de 2 segundos)
                keys = level4Keys;
            }
            else if (time < 3f)
            {
                // Nivel 3 - BUENO (menos de 3 segundos)
                keys = level3Keys;
            }
            else if (time < 4f)
            {
                // Nivel 2 - DECENTE (menos de 4 segundos)
                keys = level2Keys;
            }
            else
            {
                // Nivel 1 - BÁSICO (4 segundos o más)
                keys = level1Keys;
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

            Debug.Log($"[Game] Score guardado en historial personal y leaderboards: {currentTime:F3}s");
        }

        /// <summary>
        /// Actualiza el display del timer
        /// </summary>
        private void UpdateTimerDisplay()
        {
            if (timerText != null)
            {
                timerText.text = $"{currentTime:F3}s";
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
                    bestTimeText.text = $"Mejor: {bestTime:F3}s";
                }
                else
                {
                    bestTimeText.text = "Mejor: --";
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
