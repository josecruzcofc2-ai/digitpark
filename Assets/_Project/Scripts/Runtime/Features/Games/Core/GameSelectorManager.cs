using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;
using DigitPark.Animations;
using DigitPark.Managers;
using DigitPark.Localization;
using DigitPark.Monetization;
using DigitPark.Navigation;
using DigitPark.Services.Firebase;

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

        private void Start()
        {
            AnalyticsService.Instance?.LogScreenView("GameSelector");

            // Load practice mode from PlayerPrefs (set by PlayModeSelectionManager)
            LoadPracticeModeFromPrefs();

            SetupButtons();
            SetupCognitiveSprintPanel();

            // Suscribirse a cambios de seleccion
            CognitiveSprintManager.Instance.OnSelectionChanged += UpdateSprintUI;
        }

        private void OnDestroy()
        {
            DOTween.Kill(transform);
            if (CognitiveSprintManager.Instance != null)
            {
                CognitiveSprintManager.Instance.OnSelectionChanged -= UpdateSprintUI;
            }
            digitRushButton?.onClick.RemoveAllListeners();
            memoryPairsButton?.onClick.RemoveAllListeners();
            quickMathButton?.onClick.RemoveAllListeners();
            flashTapButton?.onClick.RemoveAllListeners();
            oddOneOutButton?.onClick.RemoveAllListeners();
            cognitiveSprintButton?.onClick.RemoveAllListeners();
            backButton?.onClick.RemoveAllListeners();
            startSprintButton?.onClick.RemoveAllListeners();
            cancelSprintButton?.onClick.RemoveAllListeners();
            if (gameToggles != null)
                foreach (var toggle in gameToggles) toggle?.onValueChanged.RemoveAllListeners();
        }

        private void SetupButtons()
        {
            // Botones de juegos individuales
            if (digitRushButton != null)
                digitRushButton.onClick.AddListener(() => StartSingleGame(GameType.DigitRush));

            if (memoryPairsButton != null)
                memoryPairsButton.onClick.AddListener(() => StartSingleGame(GameType.MemoryPairs));

            if (quickMathButton != null)
                quickMathButton.onClick.AddListener(() => StartSingleGame(GameType.QuickMath));

            if (flashTapButton != null)
                flashTapButton.onClick.AddListener(() => StartSingleGame(GameType.FlashTap));

            if (oddOneOutButton != null)
                oddOneOutButton.onClick.AddListener(() => StartSingleGame(GameType.OddOneOut));

            // Cognitive Sprint
            if (cognitiveSprintButton != null)
                cognitiveSprintButton.onClick.AddListener(OpenCognitiveSprintPanel);

            // Back - disable auto-navigation from BackButton prefab to prevent double listener
            var autoNav = backButton?.GetComponent<DigitPark.UI.BackButton>();
            if (autoNav != null) autoNav.DisableAutoNavigation();
            if (backButton != null)
                backButton.onClick.AddListener(GoBack);
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
                selectedCountText.text = AutoLocalizer.Get("game_selector_count", count, CognitiveSprintManager.MAX_GAMES);

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

        private void AnimatePanelIn(Transform t)
        {
            t.localScale = Vector3.one * 0.85f;
            var cg = t.GetComponent<CanvasGroup>();
            if (cg == null) cg = t.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            DOTween.Sequence()
                .Join(t.DOScale(1f, UIAnimations.DURATION_NORMAL).SetEase(AnimConstants.ENTER))
                .Join(cg.DOFade(1f, AnimConstants.DURATION_MEDIUM))
                .SetUpdate(true)
                .SetLink(t.gameObject);
        }

        private void AnimatePanelOut(Transform t, System.Action onComplete)
        {
            var cg = t.GetComponent<CanvasGroup>();
            if (cg == null) { t.gameObject.SetActive(false); onComplete?.Invoke(); return; }
            DOTween.Sequence()
                .Join(t.DOScale(0.9f, UIAnimations.DURATION_FAST).SetEase(AnimConstants.EXIT))
                .Join(cg.DOFade(0f, UIAnimations.DURATION_FAST))
                .OnComplete(() => { if (t != null) t.localScale = Vector3.one; if (cg != null) cg.alpha = 1f; onComplete?.Invoke(); })
                .SetUpdate(true)
                .SetLink(t.gameObject);
        }
    }

    /// <summary>
    /// Datos de reglas para un juego
    /// </summary>
}
