using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

namespace DigitPark.Monetization
{
    /// <summary>
    /// Singleton para manejar la navegacion entre escenas.
    /// Permite pasar parametros entre escenas y manejar transiciones.
    /// </summary>
    public class SceneNavigator : MonoBehaviour
    {
        private static SceneNavigator _instance;
        public static SceneNavigator Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("SceneNavigator");
                    _instance = go.AddComponent<SceneNavigator>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // ==================== SCENE NAMES ====================
        public static class Scenes
        {
            // Main
            public const string LOGIN = "Login";
            public const string MAIN_MENU = "MainMenu";
            public const string PROFILE = "Profile";

            // Cash Battle
            public const string CASH_BATTLE_HUB = "CashBattleHub";
            public const string CASH_BATTLE_1V1 = "CashBattle1v1";
            public const string CASH_TOURNAMENTS = "CashTournaments";
            public const string CASH_WALLET = "CashWallet";
            public const string CASH_HISTORY = "CashHistory";
            public const string CASH_PROFILE = "CashProfile";
            public const string CASH_MATCHMAKING = "CashMatchmaking";
            public const string CASH_TOURNAMENT_CREATE = "CashTournamentCreate";
            public const string CASH_TOURNAMENT_LOBBY = "CashTournamentLobby";

            // Tournaments
            public const string TOURNAMENTS_BROWSER = "TournamentsBrowser";
            public const string TOURNAMENT_CREATE = "TournamentCreate";
            public const string TOURNAMENT_LOBBY = "TournamentLobby";

            // Monetization
            public const string SHOP = "Shop";
            public const string DAILY_MISSIONS = "DailyMissions";
            public const string DAILY_REWARDS = "DailyRewards";
            public const string ACHIEVEMENTS = "Achievements";
            public const string ONBOARDING = "Onboarding";

            // Other
            public const string AGE_VERIFICATION = "AgeVerification";
        }

        // ==================== NAVIGATION PARAMETERS ====================

        /// <summary>
        /// Parametros para pasar a la siguiente escena
        /// </summary>
        public class NavigationParams
        {
            public string SourceScene { get; set; }
            public string TargetTab { get; set; }
            public string ItemId { get; set; }
            public bool ShowPopup { get; set; }
            public object CustomData { get; set; }
        }

        private NavigationParams _pendingParams;
        public NavigationParams PendingParams => _pendingParams;

        private string _previousScene;
        public string PreviousScene => _previousScene;

        // ==================== EVENTS ====================

        public event Action<string> OnSceneLoadStarted;
        public event Action<string> OnSceneLoadCompleted;
        public event Action OnNavigationBack;

        // ==================== NAVIGATION METHODS ====================

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            OnSceneLoadCompleted?.Invoke(scene.name);
        }

        /// <summary>
        /// Navega a una escena sin parametros
        /// </summary>
        public void NavigateTo(string sceneName)
        {
            NavigateTo(sceneName, null);
        }

        /// <summary>
        /// Navega a una escena con parametros
        /// </summary>
        public void NavigateTo(string sceneName, NavigationParams parameters)
        {
            _previousScene = SceneManager.GetActiveScene().name;
            _pendingParams = parameters;

            if (_pendingParams != null)
            {
                _pendingParams.SourceScene = _previousScene;
            }

            OnSceneLoadStarted?.Invoke(sceneName);

            Debug.Log($"[SceneNavigator] Navigating from {_previousScene} to {sceneName}");
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// Navega a una escena de forma asincrona
        /// </summary>
        public void NavigateToAsync(string sceneName, NavigationParams parameters = null, Action onComplete = null)
        {
            StartCoroutine(NavigateToAsyncCoroutine(sceneName, parameters, onComplete));
        }

        private IEnumerator NavigateToAsyncCoroutine(string sceneName, NavigationParams parameters, Action onComplete)
        {
            _previousScene = SceneManager.GetActiveScene().name;
            _pendingParams = parameters;

            if (_pendingParams != null)
            {
                _pendingParams.SourceScene = _previousScene;
            }

            OnSceneLoadStarted?.Invoke(sceneName);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            onComplete?.Invoke();
        }

        /// <summary>
        /// Regresa a la escena anterior
        /// </summary>
        public void GoBack()
        {
            if (!string.IsNullOrEmpty(_previousScene))
            {
                OnNavigationBack?.Invoke();
                string targetScene = _previousScene;
                _previousScene = null;
                _pendingParams = null;

                Debug.Log($"[SceneNavigator] Going back to {targetScene}");
                SceneManager.LoadScene(targetScene);
            }
            else
            {
                Debug.Log("[SceneNavigator] No previous scene, navigating to MainMenu");
                // Default: go to main menu
                SceneManager.LoadScene(Scenes.MAIN_MENU);
            }
        }

        /// <summary>
        /// Consume los parametros pendientes (llamar despues de leerlos)
        /// </summary>
        public void ClearPendingParams()
        {
            _pendingParams = null;
        }

        /// <summary>
        /// Obtiene y consume los parametros pendientes
        /// </summary>
        public NavigationParams ConsumeParams()
        {
            var p = _pendingParams;
            _pendingParams = null;
            return p;
        }

        // ==================== SHOP SPECIFIC NAVIGATION ====================

        /// <summary>
        /// Navega a la tienda con un tab especifico
        /// </summary>
        public void NavigateToShop(ShopTab tab = ShopTab.Featured)
        {
            NavigateTo(Scenes.SHOP, new NavigationParams
            {
                TargetTab = tab.ToString()
            });
        }

        /// <summary>
        /// Navega a la tienda para comprar DigitGems (Not Enough DigitGems flow)
        /// </summary>
        public void NavigateToShopForDigitGems()
        {
            NavigateTo(Scenes.SHOP, new NavigationParams
            {
                TargetTab = ShopTab.Currency.ToString(),
                ShowPopup = true
            });
        }

        /// <summary>
        /// Navega a la tienda mostrando una oferta especifica
        /// </summary>
        public void NavigateToShopOffer(string offerId)
        {
            NavigateTo(Scenes.SHOP, new NavigationParams
            {
                TargetTab = ShopTab.Featured.ToString(),
                ItemId = offerId
            });
        }
    }

    /// <summary>
    /// Tabs disponibles en la tienda
    /// </summary>
    public enum ShopTab
    {
        Featured,
        Currency,
        Styles
    }
}
