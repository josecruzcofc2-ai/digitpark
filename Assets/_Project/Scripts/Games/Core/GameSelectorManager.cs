using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;
using DigitPark.Managers;
using DigitPark.Localization;
using DigitPark.Monetization;

namespace DigitPark.Games
{
    /// <summary>
    /// Manager para la pantalla de seleccion de juegos
    /// Muestra los 5 juegos disponibles + opcion de Cognitive Sprint
    /// </summary>
    public class GameSelectorManager : MonoBehaviour
    {
        [Header("Game Buttons")]
        [SerializeField] private Button digitRushButton;
        [SerializeField] private Button memoryPairsButton;
        [SerializeField] private Button quickMathButton;
        [SerializeField] private Button flashTapButton;
        [SerializeField] private Button oddOneOutButton;

        [Header("Cognitive Sprint")]
        [SerializeField] private Button cognitiveSprintButton;
        [SerializeField] private GameObject cognitiveSprintPanel;
        [SerializeField] private Toggle[] gameToggles; // 5 toggles para seleccionar juegos
        [SerializeField] private Button startSprintButton;
        [SerializeField] private Button cancelSprintButton;
        [SerializeField] private TextMeshProUGUI selectedCountText;

        [Header("Navigation")]
        [SerializeField] private Button backButton;

        [Header("Rules Panel")]
        [SerializeField] private GameObject rulesPanel;
        [SerializeField] private TextMeshProUGUI rulesTitleText;
        [SerializeField] private TextMeshProUGUI rulesContentText;
        [SerializeField] private Toggle dontShowToggle;
        [SerializeField] private Button rulesPlayButton;
        [SerializeField] private Button rulesCancelButton;

        [Header("Mode Selection")]
        [SerializeField] private bool isPracticeMode = true; // Por defecto practica
        [SerializeField] private bool isOnlineMatchMode = false; // Modo 1v1 online

        [Header("Matchmaking UI")]
        [SerializeField] private GameObject matchmakingPanel;
        [SerializeField] private TextMeshProUGUI matchmakingStatusText;
        [SerializeField] private Button cancelMatchmakingButton;

        // Keys for storing mode in PlayerPrefs
        private const string PRACTICE_MODE_KEY = "DigitPark_IsPracticeMode";
        private const string ONLINE_MATCH_MODE_KEY = "DigitPark_IsOnlineMatchMode";

        // Juego actual seleccionado para las reglas
        private GameType currentRulesGame;

        // Claves de PlayerPrefs para cada juego
        private const string PREFS_PREFIX = "DigitPark_ShowRules_";

        // Diccionario de reglas para cada juego
        // Mapeo GameType → claves de localización para título y reglas
        private static readonly Dictionary<GameType, (string titleKey, string rulesKey)> gameRulesKeys = new Dictionary<GameType, (string, string)>
        {
            { GameType.DigitRush, ("rules_digitrush_title", "rules_digitrush_content") },
            { GameType.MemoryPairs, ("rules_memorypairs_title", "rules_memorypairs_content") },
            { GameType.QuickMath, ("rules_quickmath_title", "rules_quickmath_content") },
            { GameType.FlashTap, ("rules_flashtap_title", "rules_flashtap_content") },
            { GameType.OddOneOut, ("rules_oddoneout_title", "rules_oddoneout_content") }
        };

        private void Start()
        {
            // Load practice mode from PlayerPrefs (set by PlayModeSelectionManager)
            LoadPracticeModeFromPrefs();

            SetupButtons();
            SetupCognitiveSprintPanel();
            SetupRulesPanel();

            // Suscribirse a cambios de seleccion
            CognitiveSprintManager.Instance.OnSelectionChanged += UpdateSprintUI;
        }

        private void OnDestroy()
        {
            if (CognitiveSprintManager.Instance != null)
            {
                CognitiveSprintManager.Instance.OnSelectionChanged -= UpdateSprintUI;
            }
        }

        private void SetupButtons()
        {
            // Botones de juegos individuales - ahora pasan por el sistema de reglas
            if (digitRushButton != null)
                digitRushButton.onClick.AddListener(() => TryStartGame(GameType.DigitRush));

            if (memoryPairsButton != null)
                memoryPairsButton.onClick.AddListener(() => TryStartGame(GameType.MemoryPairs));

            if (quickMathButton != null)
                quickMathButton.onClick.AddListener(() => TryStartGame(GameType.QuickMath));

            if (flashTapButton != null)
                flashTapButton.onClick.AddListener(() => TryStartGame(GameType.FlashTap));

            if (oddOneOutButton != null)
                oddOneOutButton.onClick.AddListener(() => TryStartGame(GameType.OddOneOut));

            // Cognitive Sprint
            if (cognitiveSprintButton != null)
                cognitiveSprintButton.onClick.AddListener(OpenCognitiveSprintPanel);

            // Back - disable auto-navigation from BackButton prefab to prevent double listener
            var autoNav = backButton?.GetComponent<DigitPark.UI.BackButton>();
            if (autoNav != null) autoNav.DisableAutoNavigation();
            if (backButton != null)
                backButton.onClick.AddListener(GoBack);
        }

        private void SetupRulesPanel()
        {
            if (rulesPanel != null)
                rulesPanel.SetActive(false);

            if (rulesPlayButton != null)
                rulesPlayButton.onClick.AddListener(OnRulesPlayClicked);

            if (rulesCancelButton != null)
                rulesCancelButton.onClick.AddListener(CloseRulesPanel);

            if (dontShowToggle != null)
                dontShowToggle.isOn = false;
        }

        /// <summary>
        /// Intenta iniciar un juego, mostrando reglas si es necesario
        /// </summary>
        private void TryStartGame(GameType gameType)
        {
            // Verificar si debemos mostrar las reglas
            if (ShouldShowRules(gameType))
            {
                ShowRulesPanel(gameType);
            }
            else
            {
                // Iniciar directamente
                StartSingleGame(gameType);
            }
        }

        /// <summary>
        /// Verifica si debemos mostrar las reglas para este juego
        /// </summary>
        private bool ShouldShowRules(GameType gameType)
        {
            string key = PREFS_PREFIX + gameType.ToString();
            // Por defecto mostramos las reglas (1 = mostrar, 0 = no mostrar)
            return PlayerPrefs.GetInt(key, 1) == 1;
        }

        /// <summary>
        /// Guarda la preferencia de no mostrar reglas para un juego
        /// </summary>
        private void SaveDontShowRules(GameType gameType)
        {
            string key = PREFS_PREFIX + gameType.ToString();
            PlayerPrefs.SetInt(key, 0); // 0 = no mostrar
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Muestra el panel de reglas para un juego
        /// </summary>
        private void ShowRulesPanel(GameType gameType)
        {
            if (rulesPanel == null) return;

            currentRulesGame = gameType;

            // Obtener datos de reglas localizadas
            if (gameRulesKeys.TryGetValue(gameType, out var keys))
            {
                if (rulesTitleText != null)
                    rulesTitleText.text = AutoLocalizer.Get(keys.titleKey);

                if (rulesContentText != null)
                    rulesContentText.text = AutoLocalizer.Get(keys.rulesKey);
            }

            // Resetear toggle
            if (dontShowToggle != null)
                dontShowToggle.isOn = false;

            rulesPanel.SetActive(true);
            AnimatePanelIn(rulesPanel.transform);
        }

        /// <summary>
        /// Cierra el panel de reglas
        /// </summary>
        private void CloseRulesPanel()
        {
            if (rulesPanel != null)
                AnimatePanelOut(rulesPanel.transform, () => rulesPanel.SetActive(false));
        }

        /// <summary>
        /// Cuando se presiona el botón Jugar en el panel de reglas
        /// </summary>
        private void OnRulesPlayClicked()
        {
            // Guardar preferencia si el toggle está marcado
            if (dontShowToggle != null && dontShowToggle.isOn)
            {
                SaveDontShowRules(currentRulesGame);
            }

            // Cerrar panel e iniciar juego
            CloseRulesPanel();
            StartSingleGame(currentRulesGame);
        }

        private void SetupCognitiveSprintPanel()
        {
            if (cognitiveSprintPanel != null)
                cognitiveSprintPanel.SetActive(false);

            // Setup toggles
            if (gameToggles != null)
            {
                GameType[] types = { GameType.DigitRush, GameType.MemoryPairs, GameType.QuickMath,
                                    GameType.FlashTap, GameType.OddOneOut };

                for (int i = 0; i < gameToggles.Length && i < types.Length; i++)
                {
                    int index = i;
                    GameType gameType = types[i];

                    if (gameToggles[i] != null)
                    {
                        gameToggles[i].onValueChanged.AddListener((isOn) => OnToggleChanged(gameType, isOn));
                    }
                }
            }

            // Botones del panel
            if (startSprintButton != null)
                startSprintButton.onClick.AddListener(StartCognitiveSprint);

            if (cancelSprintButton != null)
                cancelSprintButton.onClick.AddListener(CloseCognitiveSprintPanel);
        }

        /// <summary>
        /// Inicia un juego individual
        /// </summary>
        private void StartSingleGame(GameType gameType)
        {
            if (isPracticeMode)
            {
                GameSessionManager.Instance.StartPracticeSession(gameType);
            }
            else if (isOnlineMatchMode)
            {
                // Modo 1v1 online - navegar a seleccion de apuesta
                Debug.Log($"[GameSelector] Iniciando seleccion de apuesta para {gameType}");
                MatchmakingManager.SetMatchGameType(gameType, false);
                SceneManager.LoadScene("BetSelection");
            }
            else
            {
                // Modo competitivo sin matchmaking (ej: torneos)
                Debug.Log($"Modo competitivo para {gameType}");
                GameSessionManager.Instance.StartPracticeSession(gameType);
            }
        }

        /// <summary>
        /// Abre el panel de Cognitive Sprint
        /// </summary>
        private void OpenCognitiveSprintPanel()
        {
            if (cognitiveSprintPanel != null)
            {
                cognitiveSprintPanel.SetActive(true);
                AnimatePanelIn(cognitiveSprintPanel.transform);
                CognitiveSprintManager.Instance.ClearSelection();
                ResetToggles();
                UpdateSprintUI(new List<GameType>());
            }
        }

        /// <summary>
        /// Cierra el panel de Cognitive Sprint
        /// </summary>
        private void CloseCognitiveSprintPanel()
        {
            if (cognitiveSprintPanel != null)
            {
                AnimatePanelOut(cognitiveSprintPanel.transform, () => cognitiveSprintPanel.SetActive(false));
                CognitiveSprintManager.Instance.ClearSelection();
            }
        }

        /// <summary>
        /// Cuando un toggle cambia
        /// </summary>
        private void OnToggleChanged(GameType gameType, bool isOn)
        {
            if (isOn)
            {
                CognitiveSprintManager.Instance.AddGame(gameType);
            }
            else
            {
                CognitiveSprintManager.Instance.RemoveGame(gameType);
            }
        }

        /// <summary>
        /// Actualiza la UI del sprint segun la seleccion
        /// </summary>
        private void UpdateSprintUI(List<GameType> selectedGames)
        {
            if (selectedCountText != null)
            {
                int count = selectedGames?.Count ?? 0;
                selectedCountText.text = $"{count}/{CognitiveSprintManager.MAX_GAMES} juegos seleccionados";

                // Cambiar color segun validez
                if (count >= CognitiveSprintManager.MIN_GAMES)
                {
                    selectedCountText.color = Color.green;
                }
                else
                {
                    selectedCountText.color = Color.white;
                }
            }

            // Habilitar/deshabilitar boton de inicio
            if (startSprintButton != null)
            {
                startSprintButton.interactable = CognitiveSprintManager.Instance.IsValidSelection();
            }
        }

        /// <summary>
        /// Resetea todos los toggles
        /// </summary>
        private void ResetToggles()
        {
            if (gameToggles != null)
            {
                foreach (var toggle in gameToggles)
                {
                    if (toggle != null)
                        toggle.isOn = false;
                }
            }
        }

        /// <summary>
        /// Inicia el Cognitive Sprint
        /// </summary>
        private void StartCognitiveSprint()
        {
            if (!CognitiveSprintManager.Instance.IsValidSelection())
            {
                Debug.LogWarning("Seleccion invalida para Cognitive Sprint");
                return;
            }

            if (isPracticeMode)
            {
                CognitiveSprintManager.Instance.StartPracticeSprint();
            }
            else if (isOnlineMatchMode)
            {
                // Modo 1v1 online con Cognitive Sprint - seleccion de apuesta primero
                Debug.Log("[GameSelector] Iniciando seleccion de apuesta para Cognitive Sprint");
                MatchmakingManager.SetMatchGameType(GameType.DigitRush, true); // true = isCognitiveSprint
                SceneManager.LoadScene("BetSelection");
            }
            else
            {
                // Modo competitivo sin matchmaking
                CognitiveSprintManager.Instance.StartPracticeSprint();
            }
        }

        /// <summary>
        /// Vuelve al menu principal
        /// </summary>
        private void GoBack()
        {
            SceneNavigator.Instance?.GoBack();
        }

        /// <summary>
        /// Cambia entre modo practica y competitivo (instancia)
        /// </summary>
        public void SetPracticeModeInstance(bool practice)
        {
            isPracticeMode = practice;
        }

        /// <summary>
        /// Establece el modo de práctica antes de cargar la escena (estático)
        /// Usa PlayerPrefs para persistir entre escenas
        /// Llamar desde PlayModeSelectionManager antes de cargar GameSelector
        /// </summary>
        public static void SetPracticeMode(bool practice)
        {
            PlayerPrefs.SetInt("DigitPark_IsPracticeMode", practice ? 1 : 0);
            PlayerPrefs.Save();
            Debug.Log($"[GameSelector] Practice mode set to: {practice}");
        }

        /// <summary>
        /// Establece el modo de partida online 1v1 antes de cargar la escena (estático)
        /// Cuando está activo, al seleccionar un juego se inicia matchmaking
        /// </summary>
        public static void SetOnlineMatchMode(bool online)
        {
            PlayerPrefs.SetInt("DigitPark_IsOnlineMatchMode", online ? 1 : 0);
            PlayerPrefs.Save();
            Debug.Log($"[GameSelector] Online match mode set to: {online}");
        }

        /// <summary>
        /// Lee los modos guardados en PlayerPrefs
        /// </summary>
        private void LoadPracticeModeFromPrefs()
        {
            // Default to practice mode if not set
            isPracticeMode = PlayerPrefs.GetInt(PRACTICE_MODE_KEY, 1) == 1;
            isOnlineMatchMode = PlayerPrefs.GetInt(ONLINE_MATCH_MODE_KEY, 0) == 1;
            Debug.Log($"[GameSelector] Loaded modes - Practice: {isPracticeMode}, Online 1v1: {isOnlineMatchMode}");

            // Update title based on mode
            UpdateModeTitle();
        }

        /// <summary>
        /// Actualiza el título según el modo actual
        /// </summary>
        private void UpdateModeTitle()
        {
            // Could update a title text here to show "SOLO MODE" or "1v1 MODE"
            if (isOnlineMatchMode)
            {
                Debug.Log("[GameSelector] Modo 1v1 Online - Selecciona un juego para buscar oponente");
            }
        }

        /// <summary>
        /// Resetea las preferencias de reglas (para testing)
        /// </summary>
        public static void ResetAllRulesPreferences()
        {
            foreach (GameType gameType in System.Enum.GetValues(typeof(GameType)))
            {
                string key = PREFS_PREFIX + gameType.ToString();
                PlayerPrefs.DeleteKey(key);
            }
            PlayerPrefs.Save();
            Debug.Log("Todas las preferencias de reglas han sido reseteadas");
        }

        private void AnimatePanelIn(Transform t)
        {
            t.localScale = Vector3.one * 0.85f;
            var cg = t.GetComponent<CanvasGroup>();
            if (cg == null) cg = t.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            DOTween.Sequence()
                .Join(t.DOScale(1f, 0.3f).SetEase(Ease.OutBack))
                .Join(cg.DOFade(1f, 0.25f))
                .SetUpdate(true);
        }

        private void AnimatePanelOut(Transform t, System.Action onComplete)
        {
            var cg = t.GetComponent<CanvasGroup>();
            if (cg == null) { t.gameObject.SetActive(false); onComplete?.Invoke(); return; }
            DOTween.Sequence()
                .Join(t.DOScale(0.9f, 0.2f).SetEase(Ease.InQuad))
                .Join(cg.DOFade(0f, 0.2f))
                .OnComplete(() => { t.localScale = Vector3.one; cg.alpha = 1f; onComplete?.Invoke(); })
                .SetUpdate(true);
        }
    }

    /// <summary>
    /// Datos de reglas para un juego
    /// </summary>
}
