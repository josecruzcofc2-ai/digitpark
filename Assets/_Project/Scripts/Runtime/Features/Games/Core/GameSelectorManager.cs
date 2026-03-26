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
    /// Muestra los 5 juegos disponibles
    /// </summary>
    public class GameSelectorManager : MonoBehaviour
    {
        [Header("Game Buttons")]
        [SerializeField] private Button digitRushButton;
        [SerializeField] private Button memoryPairsButton;
        [SerializeField] private Button quickMathButton;
        [SerializeField] private Button flashTapButton;
        [SerializeField] private Button oddOneOutButton;

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
        }

        private void OnDestroy()
        {
            DOTween.Kill(transform);
            digitRushButton?.onClick.RemoveAllListeners();
            memoryPairsButton?.onClick.RemoveAllListeners();
            quickMathButton?.onClick.RemoveAllListeners();
            flashTapButton?.onClick.RemoveAllListeners();
            oddOneOutButton?.onClick.RemoveAllListeners();
            backButton?.onClick.RemoveAllListeners();
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

            // Back - disable auto-navigation from BackButton prefab to prevent double listener
            var autoNav = backButton?.GetComponent<DigitPark.UI.BackButton>();
            if (autoNav != null) autoNav.DisableAutoNavigation();
            if (backButton != null)
                backButton.onClick.AddListener(GoBack);
        }

        /// <summary>
        /// Inicia un juego individual
        /// </summary>
        private void StartSingleGame(GameType gameType)
        {
            if (GameSessionManager.Instance == null)
            {
                Debug.LogError("[GameSelector] GameSessionManager not available");
                return;
            }

            if (isPracticeMode)
            {
                GameSessionManager.Instance.StartPracticeSession(gameType);
            }
            else if (isOnlineMatchMode)
            {
                Debug.Log($"[GameSelector] Iniciando seleccion de apuesta para {gameType}");
                MatchmakingManager.SetMatchGameType(gameType);
                SceneManager.LoadScene("BetSelection");
            }
            else
            {
                Debug.Log($"Modo competitivo para {gameType}");
                GameSessionManager.Instance.StartPracticeSession(gameType);
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
