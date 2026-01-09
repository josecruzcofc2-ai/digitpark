using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DigitPark.Services.Firebase;
using DigitPark.Data;
using DigitPark.Localization;
using DigitPark.UI;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager principal del juego DigitRush
    /// Controla la mecánica core del grid 3x3 y la secuencia de números 1-9
    /// Con sistema de combos, partículas y vibración háptica
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Grid")]
        [SerializeField] public Button[] gridButtons; // 9 botones del grid 3x3

        [Header("Game UI")]
        [SerializeField] public TextMeshProUGUI timerText;
        [SerializeField] public TextMeshProUGUI bestTimeText;
        [SerializeField] public TextMeshProUGUI comboText;
        [SerializeField] public Button playAgainButton;
        [SerializeField] public Button backButton;

        [Header("Countdown")]
        [SerializeField] private CountdownUI countdownUI;
        [SerializeField] private bool useCountdown = true;

        [Header("Effects")]
        [SerializeField] private UISparkleEffect sparkleEffect;
        [SerializeField] private bool enableHapticFeedback = true;

        [Header("Win Message")]
        [SerializeField] public GameObject winMessagePanel;
        [SerializeField] public CanvasGroup winMessageCanvasGroup;
        [SerializeField] public TextMeshProUGUI successText;

        [Header("Premium Banner (Result Screen)")]
        [SerializeField] public GameObject premiumBannerContainer;
        [SerializeField] public Button premiumBannerButton;
        [SerializeField] public TextMeshProUGUI premiumBannerText;

        // Game State
        private int currentTargetNumber = 1;
        private bool isGameActive = false;
        private bool isTimerStarted = false;

        // Timer
        private float currentTime = 0f;
        private float bestTime = float.MaxValue;

        // Combo System
        private int currentCombo = 0;
        private int maxCombo = 0;

        // Player Data
        private PlayerData currentPlayer;

        // Números asignados a cada botón
        private int[] gridNumbers = new int[9];

        // Coroutines de shake
        private Coroutine[] shakeCoroutines = new Coroutine[9];
        private Vector2[] shakeOriginalPositions = new Vector2[9];

        // Mensajes de éxito
        private readonly string[] level1Keys = {
            "msg_godlike_focus", "msg_mind_on_fire", "msg_exceptional_reflexes",
            "msg_neural_perfection", "msg_time_master", "msg_superhuman",
            "msg_unstoppable_force", "msg_legendary_speed", "msg_pure_genius", "msg_absolute_legend"
        };
        private readonly string[] level2Keys = {
            "msg_incredible_focus", "msg_blazing_fast", "msg_sharp_mind",
            "msg_impressive_reflexes", "msg_excellent_timing", "msg_on_fire",
            "msg_amazing_speed", "msg_brilliant_play", "msg_stellar_performance", "msg_remarkable"
        };
        private readonly string[] level3Keys = {
            "msg_great_job", "msg_well_played", "msg_nice_speed", "msg_good_reflexes", "msg_solid_time"
        };
        private readonly string[] level4Keys = {
            "msg_good_effort", "msg_not_bad", "msg_keep_going", "msg_nice_try", "msg_getting_better"
        };
        private readonly string[] level5Keys = {
            "msg_completed", "msg_done", "msg_finished", "msg_keep_practicing", "msg_you_can_improve"
        };
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

            // Buscar UISparkleEffect si no está asignado
            if (sparkleEffect == null)
            {
                sparkleEffect = FindFirstObjectByType<UISparkleEffect>();
            }
        }

        private void Start()
        {
            Debug.Log("[Game] GameManager iniciado");
            LoadPlayerData();
            SetupListeners();
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

        private void LoadPlayerData()
        {
            if (AuthenticationService.Instance != null)
            {
                currentPlayer = AuthenticationService.Instance.GetCurrentPlayerData();

                if (currentPlayer != null)
                {
                    bestTime = currentPlayer.bestTime;
                    UpdateBestTimeDisplay();
                }
            }
        }

        private void SetupListeners()
        {
            for (int i = 0; i < gridButtons.Length; i++)
            {
                int index = i;
                gridButtons[i].onClick.AddListener(() => OnGridButtonClicked(index));
                shakeCoroutines[i] = null;
            }

            playAgainButton?.onClick.AddListener(StartNewGame);
            backButton?.onClick.AddListener(OnBackButtonClicked);
            premiumBannerButton?.onClick.AddListener(OnPremiumBannerClicked);

            PremiumManager.OnPremiumStatusChanged += UpdatePremiumBanner;
            UpdatePremiumBanner();
        }

        private void OnDestroy()
        {
            PremiumManager.OnPremiumStatusChanged -= UpdatePremiumBanner;
        }

        private void UpdatePremiumBanner()
        {
            if (premiumBannerContainer == null) return;

            bool isPremium = PremiumManager.Instance != null && PremiumManager.Instance.IsPremium;
            premiumBannerContainer.SetActive(!isPremium);

            if (premiumBannerText != null && !isPremium)
            {
                premiumBannerText.text = $"{AutoLocalizer.Get("premium_feature_no_ads")} + {AutoLocalizer.Get("premium_feature_tournaments")}";
            }
        }

        private void OnPremiumBannerClicked()
        {
            Debug.Log("[Game] Banner premium clickeado");
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                DigitPark.UI.Panels.PremiumPanelUI.CreateAndShow(canvas.transform);
            }
        }

        public void StartNewGame()
        {
            Debug.Log("[Game] Iniciando nuevo juego...");

            currentTargetNumber = 1;
            currentTime = 0f;
            isGameActive = false;
            isTimerStarted = false;
            currentCombo = 0;
            maxCombo = 0;

            GenerateRandomNumbers();
            ResetButtonVisuals();
            SetGridButtonsInteractable(false);
            HideNumbers();
            UpdateComboDisplay();

            if (winMessagePanel != null)
            {
                winMessagePanel.SetActive(false);
                winMessageCanvasGroup.alpha = 0;
            }

            UpdateTimerDisplay();
            UpdateBestTimeDisplay();

            if (useCountdown && countdownUI != null)
            {
                countdownUI.StartCountdown(OnCountdownComplete);
            }
            else
            {
                OnCountdownComplete();
            }

            Debug.Log("[Game] Nuevo juego iniciado - Números ocultos hasta GO!");
        }

        private void OnCountdownComplete()
        {
            Debug.Log("[Game] Countdown completado - Revelando números!");

            AssignNumbersToButtons();
            AnimateButtonsPopUp();
            StartCoroutine(ActivateGameAfterDelay(0.3f));
        }

        private IEnumerator ActivateGameAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            isGameActive = true;
            isTimerStarted = true;
            SetGridButtonsInteractable(true);

            Debug.Log("[Game] ¡Juego activo! Timer corriendo...");
        }

        private void SetGridButtonsInteractable(bool interactable)
        {
            foreach (var button in gridButtons)
            {
                if (button != null)
                {
                    button.interactable = interactable;
                }
            }
        }

        private void HideNumbers()
        {
            for (int i = 0; i < gridButtons.Length; i++)
            {
                if (gridButtons[i] != null)
                {
                    TextMeshProUGUI numberText = gridButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (numberText != null)
                    {
                        numberText.text = "";
                    }
                }
            }
        }

        private void AnimateButtonsPopUp()
        {
            foreach (var button in gridButtons)
            {
                if (button != null)
                {
                    var cell3D = button.GetComponent<Cell3DButton>();
                    if (cell3D != null)
                    {
                        cell3D.AnimateGameStart();
                    }
                }
            }
        }

        private void GenerateRandomNumbers()
        {
            List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            for (int i = numbers.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                int temp = numbers[i];
                numbers[i] = numbers[randomIndex];
                numbers[randomIndex] = temp;
            }

            for (int i = 0; i < 9; i++)
            {
                gridNumbers[i] = numbers[i];
            }

            Debug.Log($"[Game] Números generados: {string.Join(", ", gridNumbers)}");
        }

        private void AssignNumbersToButtons()
        {
            for (int i = 0; i < gridButtons.Length; i++)
            {
                TextMeshProUGUI numberText = gridButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (numberText != null)
                {
                    numberText.text = gridNumbers[i].ToString();
                }
            }
        }

        private void ResetButtonVisuals()
        {
            for (int i = 0; i < gridButtons.Length; i++)
            {
                if (shakeCoroutines[i] != null)
                {
                    StopCoroutine(shakeCoroutines[i]);
                    shakeCoroutines[i] = null;

                    RectTransform rt = gridButtons[i].GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchoredPosition = shakeOriginalPositions[i];
                    }
                }

                gridButtons[i].interactable = true;

                Image buttonImage = gridButtons[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = new Color(0.15f, 0.15f, 0.25f);
                }

                TextMeshProUGUI numberText = gridButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (numberText != null)
                {
                    numberText.color = Color.white;
                }

                // Reset Cell3DButton state
                var cell3D = gridButtons[i].GetComponent<Cell3DButton>();
                if (cell3D != null)
                {
                    cell3D.ResetToNormal();
                }
            }
        }

        private void OnGridButtonClicked(int buttonIndex)
        {
            if (!isGameActive) return;

            if (!isTimerStarted)
            {
                isTimerStarted = true;
                Debug.Log("[Game] Timer iniciado");
            }

            int clickedNumber = gridNumbers[buttonIndex];
            Debug.Log($"[Game] Botón {buttonIndex} clickeado - Número: {clickedNumber} | Esperado: {currentTargetNumber}");

            if (clickedNumber == currentTargetNumber)
            {
                OnCorrectNumberClicked(buttonIndex);
                currentTargetNumber++;

                if (currentTargetNumber > 9)
                {
                    OnGameComplete();
                }
            }
            else
            {
                OnWrongNumberClicked(buttonIndex);
            }
        }

        private void OnCorrectNumberClicked(int buttonIndex)
        {
            Debug.Log($"[Game] ¡Correcto! Número {gridNumbers[buttonIndex]}");

            // Incrementar combo
            currentCombo++;
            if (currentCombo > maxCombo) maxCombo = currentCombo;

            // Vibración de éxito (más fuerte con combo)
            if (currentCombo >= 7)
                TriggerHaptic(HapticType.Heavy);
            else if (currentCombo >= 4)
                TriggerHaptic(HapticType.Medium);
            else
                TriggerHaptic(HapticType.Light);

            // Partículas de éxito
            PlayCorrectParticles(buttonIndex);

            // Animar botón como completado
            var cell3D = gridButtons[buttonIndex].GetComponent<Cell3DButton>();
            if (cell3D != null)
            {
                cell3D.AnimateToCompleted(currentCombo);
            }
            else
            {
                // Fallback visual
                Image buttonImage = gridButtons[buttonIndex].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = new Color(0.2f, 0.7f, 0.3f, 0.5f);
                }
            }

            gridButtons[buttonIndex].interactable = false;

            TextMeshProUGUI numberText = gridButtons[buttonIndex].GetComponentInChildren<TextMeshProUGUI>();
            if (numberText != null)
            {
                numberText.color = new Color(0.5f, 0.5f, 0.5f);
            }

            UpdateComboDisplay();
        }

        private void OnWrongNumberClicked(int buttonIndex)
        {
            Debug.Log($"[Game] ¡Incorrecto! Clickeó {gridNumbers[buttonIndex]}, esperaba {currentTargetNumber}");

            // Resetear combo
            currentCombo = 0;
            UpdateComboDisplay();

            // Vibración de error
            TriggerHaptic(HapticType.Error);

            // Partículas de error
            PlayErrorParticles(buttonIndex);

            RectTransform rt = gridButtons[buttonIndex].GetComponent<RectTransform>();

            if (shakeCoroutines[buttonIndex] != null)
            {
                StopCoroutine(shakeCoroutines[buttonIndex]);
                rt.anchoredPosition = shakeOriginalPositions[buttonIndex];
            }

            shakeOriginalPositions[buttonIndex] = rt.anchoredPosition;
            shakeCoroutines[buttonIndex] = StartCoroutine(ShakeButton(buttonIndex));
        }

        private IEnumerator ShakeButton(int buttonIndex)
        {
            RectTransform rt = gridButtons[buttonIndex].GetComponent<RectTransform>();
            Vector2 originalPos = shakeOriginalPositions[buttonIndex];

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
            shakeCoroutines[buttonIndex] = null;
        }

        #region Particle Effects

        private void PlayCorrectParticles(int buttonIndex)
        {
            if (sparkleEffect == null) return;

            Vector2 pos = GetButtonPosition(buttonIndex);
            sparkleEffect.PlayMatchSparkles(pos, currentCombo);

            // Estrellas extra para combo alto
            if (currentCombo >= 5)
            {
                sparkleEffect.PlayStarBurst(pos, currentCombo - 3);
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
            if (buttonIndex < gridButtons.Length && gridButtons[buttonIndex] != null)
            {
                RectTransform rt = gridButtons[buttonIndex].GetComponent<RectTransform>();
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
            if (comboText == null) return;

            if (currentCombo >= 3)
            {
                comboText.gameObject.SetActive(true);
                comboText.text = $"x{currentCombo}";

                // Color basado en combo
                if (currentCombo >= 8)
                    comboText.color = new Color(1f, 0.85f, 0.2f); // Dorado
                else if (currentCombo >= 6)
                    comboText.color = new Color(1f, 0.5f, 0.2f); // Naranja
                else if (currentCombo >= 4)
                    comboText.color = new Color(0.3f, 1f, 0.5f); // Verde
                else
                    comboText.color = new Color(0f, 0.9f, 1f); // Cyan
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Victory

        private void OnGameComplete()
        {
            Debug.Log($"[Game] ¡JUEGO COMPLETADO! Tiempo: {currentTime:F3}s | Max Combo: {maxCombo}");

            isGameActive = false;
            isTimerStarted = false;

            bool isNewRecord = currentTime < bestTime;

            if (isNewRecord)
            {
                bestTime = currentTime;
                Debug.Log($"[Game] ¡NUEVO RÉCORD! {bestTime:F3}s");
                SaveBestTime();
            }

            SaveScoreToDatabase();

            // Vibración de victoria
            TriggerHaptic(HapticType.Heavy);

            // Secuencia de victoria
            StartCoroutine(PlayVictorySequence());
        }

        private IEnumerator PlayVictorySequence()
        {
            yield return new WaitForSeconds(0.1f);
            TriggerHaptic(HapticType.Heavy);

            // Animación de "ola" de celebración en los botones
            float waveDelay = 0.08f;
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    int index = row * 3 + col;
                    if (index < gridButtons.Length && gridButtons[index] != null)
                    {
                        var cell3D = gridButtons[index].GetComponent<Cell3DButton>();
                        if (cell3D != null)
                        {
                            float delay = (row + col) * waveDelay;
                            cell3D.PlayVictoryCelebration(delay);
                        }
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

            // Mostrar mensaje de victoria
            StartCoroutine(ShowWinMessage());
        }

        private string GetSuccessMessage(float time)
        {
            string[] keys;

            if (time < 1f)
                keys = level1Keys;
            else if (time < 2f)
                keys = level2Keys;
            else if (time < 3f)
                keys = level3Keys;
            else if (time < 4f)
                keys = level4Keys;
            else if (time < 5f)
                keys = level5Keys;
            else
                keys = level6Keys;

            string selectedKey = keys[Random.Range(0, keys.Length)];
            Debug.Log($"[Game] GetSuccessMessage - Tiempo: {time:F3}s, Key seleccionada: {selectedKey}");

            if (LocalizationManager.Instance != null)
            {
                string translatedText = LocalizationManager.Instance.GetText(selectedKey);
                Debug.Log($"[Game] Texto traducido: '{translatedText}'");
                return translatedText;
            }

            Debug.LogWarning("[Game] LocalizationManager.Instance es NULL! Usando key como fallback.");
            return selectedKey;
        }

        private IEnumerator ShowWinMessage()
        {
            Debug.Log($"[Game] ShowWinMessage - winMessagePanel: {winMessagePanel != null}");

            if (winMessagePanel == null || winMessageCanvasGroup == null)
            {
                Debug.LogError("[Game] winMessagePanel o winMessageCanvasGroup es NULL!");
                yield break;
            }

            if (successText != null)
            {
                string message = GetSuccessMessage(currentTime);
                successText.text = message;
                Debug.Log($"[Game] SuccessText establecido: '{message}'");
            }

            winMessagePanel.SetActive(true);

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

            yield return new WaitForSeconds(1f);

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

        #endregion

        #region Save Data

        private async void SaveBestTime()
        {
            if (currentPlayer == null) return;

            currentPlayer.bestTime = bestTime;
            await DatabaseService.Instance.SavePlayerData(currentPlayer);

            UpdateBestTimeDisplay();

            Debug.Log($"[Game] Mejor tiempo guardado: {bestTime:F3}s");
        }

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

            currentPlayer.AddScore(currentTime);
            currentPlayer.totalGamesPlayed++;
            currentPlayer.totalGamesWon++;

            await DatabaseService.Instance.SavePlayerData(currentPlayer);

            await DatabaseService.Instance.SaveScore(
                currentPlayer.userId,
                currentPlayer.username,
                currentTime,
                currentPlayer.countryCode
            );

            await DatabaseService.Instance.UpdateScoreInAllActiveTournaments(
                currentPlayer.userId,
                currentTime
            );

            Debug.Log($"[Game] Score guardado: {currentTime:F3}s");
        }

        #endregion

        #region UI Updates

        private void UpdateTimerDisplay()
        {
            if (timerText != null)
            {
                timerText.text = $"{currentTime:F3}{AutoLocalizer.Get("seconds_abbr")}";
            }
        }

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

        private void OnBackButtonClicked()
        {
            BackToMenu();
        }

        public void BackToMenu()
        {
            Debug.Log("[Game] Volviendo al menú principal");
            SceneManager.LoadScene("MainMenu");
        }

        #endregion
    }
}
