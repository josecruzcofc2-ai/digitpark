using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Analytics;
using DigitPark.Services;

namespace DigitPark.Services.Firebase
{
    /// <summary>
    /// Servicio de Firebase Analytics
    /// Trackea eventos importantes para entender el comportamiento del usuario
    /// </summary>
    public class AnalyticsService : MonoBehaviour
    {
        public static AnalyticsService Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool enableAnalytics = true;
        [SerializeField] private bool debugMode = true;

        private bool _isInitialized = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                // SEC-B06: Force debugMode off in production builds
#if !UNITY_EDITOR
                debugMode = false;
#endif

                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            if (!enableAnalytics)
            {
                Debug.Log("[Analytics] Analytics deshabilitado");
                return;
            }

            // AUDIT-FIXED [2026-03-10] H-06: Verificar que Firebase esté listo antes de SetAnalyticsCollectionEnabled
            _ = InitializeAsync();
        }

        private async System.Threading.Tasks.Task InitializeAsync()
        {
            try
            {
                var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
                if (dependencyStatus != DependencyStatus.Available)
                {
                    Debug.LogError($"[Analytics] Firebase no disponible: {dependencyStatus}");
                    return;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Analytics] Error verificando dependencias Firebase: {e.Message}");
                return;
            }

            // Verificar ATT status antes de habilitar analytics (iOS 14.5+)
            bool trackingAllowed = true;

#if UNITY_IOS
            if (ATTService.Instance != null)
            {
                trackingAllowed = ATTService.Instance.IsTrackingAuthorized;
                Debug.Log($"[Analytics] ATT Status: {ATTService.Instance.TrackingStatus}, Tracking allowed: {trackingAllowed}");
            }
#endif

            // Firebase Analytics se inicializa automaticamente con FirebaseApp
            // Solo habilitar coleccion si ATT lo permite
            try
            {
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(trackingAllowed);
                _isInitialized = true;
                Debug.Log($"[Analytics] Firebase Analytics inicializado (collection: {trackingAllowed})");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Analytics] Error inicializando Firebase Analytics: {e.Message}");
                _isInitialized = false;
            }
        }

        #region Screen Tracking

        /// <summary>
        /// Registra cuando el usuario ve una pantalla
        /// </summary>
        public void LogScreenView(string screenName, string screenClass = null)
        {
            if (!_isInitialized) return;

            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventScreenView,
                new Parameter(FirebaseAnalytics.ParameterScreenName, screenName),
                new Parameter(FirebaseAnalytics.ParameterScreenClass, screenClass ?? screenName)
            );

            LogDebug($"screen_view: {screenName}");
        }

        #endregion

        #region Game Events

        public void LogGameStart(string gameType)
        {
            if (!_isInitialized) return;

            FirebaseAnalytics.LogEvent("game_start",
                new Parameter("game_type", gameType)
            );

            LogDebug($"game_start: {gameType}");
        }

        public void LogGameComplete(string gameType, float time, int score, bool isWin, bool isNewRecord)
        {
            if (!_isInitialized) return;

            FirebaseAnalytics.LogEvent("game_complete",
                new Parameter("game_type", gameType),
                new Parameter("time_seconds", time),
                new Parameter("score", score),
                new Parameter("is_win", isWin ? 1 : 0),
                new Parameter("is_new_record", isNewRecord ? 1 : 0)
            );

            LogDebug($"game_complete: {gameType}, time={time}, win={isWin}");
        }

        public void LogGameAbandoned(string gameType, float timeElapsed)
        {
            if (!_isInitialized) return;

            FirebaseAnalytics.LogEvent("game_abandoned",
                new Parameter("game_type", gameType),
                new Parameter("time_elapsed", timeElapsed)
            );

            LogDebug($"game_abandoned: {gameType}");
        }

        #endregion

        #region Matchmaking Events

        public void LogMatchmakingStart(string gameType, decimal entryFee)
        {
            if (!_isInitialized) return;

            FirebaseAnalytics.LogEvent("matchmaking_start",
                new Parameter("game_type", gameType),
                new Parameter("entry_fee", (double)entryFee)
            );

            LogDebug($"matchmaking_start: {gameType}, fee={entryFee}");
        }

        public void LogMatchFound(string gameType, float searchTime)
        {
            if (!_isInitialized) return;

            FirebaseAnalytics.LogEvent("match_found",
                new Parameter("game_type", gameType),
                new Parameter("search_time_seconds", searchTime)
            );

            LogDebug($"match_found: {gameType}, search_time={searchTime}s");
        }

        public void LogMatchComplete(string gameType, bool isWin, decimal prizeWon)
        {
            if (!_isInitialized) return;

            FirebaseAnalytics.LogEvent("match_complete",
                new Parameter("game_type", gameType),
                new Parameter("is_win", isWin ? 1 : 0),
                new Parameter("prize_won", (double)prizeWon)
            );

            LogDebug($"match_complete: {gameType}, win={isWin}, prize={prizeWon}");
        }

        #endregion

        #region Economy Events

        public void LogVirtualCurrencyEarned(string currencyType, int amount, string source)
        {
            if (!_isInitialized) return;

            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventEarnVirtualCurrency,
                new Parameter(FirebaseAnalytics.ParameterVirtualCurrencyName, currencyType),
                new Parameter(FirebaseAnalytics.ParameterValue, amount),
                new Parameter("source", source)
            );

            LogDebug($"earn_virtual_currency: {amount} {currencyType} from {source}");
        }

        public void LogVirtualCurrencySpent(string currencyType, int amount, string itemName)
        {
            if (!_isInitialized) return;

            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventSpendVirtualCurrency,
                new Parameter(FirebaseAnalytics.ParameterVirtualCurrencyName, currencyType),
                new Parameter(FirebaseAnalytics.ParameterValue, amount),
                new Parameter(FirebaseAnalytics.ParameterItemName, itemName)
            );

            LogDebug($"spend_virtual_currency: {amount} {currencyType} on {itemName}");
        }

        #endregion

        #region IAP Events

        public void LogPurchaseStarted(string productId, double price, string currency)
        {
            if (!_isInitialized) return;

            FirebaseAnalytics.LogEvent("purchase_started",
                new Parameter("item_id", productId),
                new Parameter(FirebaseAnalytics.ParameterPrice, price),
                new Parameter(FirebaseAnalytics.ParameterCurrency, currency)
            );

            LogDebug($"purchase_started: {productId} at {price} {currency}");
        }

        public void LogPurchaseCompleted(string productId, double price, string currency, string transactionId)
        {
            if (!_isInitialized) return;

            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPurchase,
                new Parameter("item_id", productId),
                new Parameter(FirebaseAnalytics.ParameterPrice, price),
                new Parameter(FirebaseAnalytics.ParameterCurrency, currency),
                new Parameter("transaction_id", transactionId)
            );

            LogDebug($"purchase: {productId} completed, txn={transactionId}");
        }

        public void LogPurchaseFailed(string productId, string errorReason)
        {
            if (!_isInitialized) return;

            FirebaseAnalytics.LogEvent("purchase_failed",
                new Parameter("item_id", productId),
                new Parameter("error_reason", errorReason)
            );

            LogDebug($"purchase_failed: {productId}, reason={errorReason}");
        }

        #endregion

        #region Social Events

        public void LogChallengeCreated(string gameType)
        {
            if (!_isInitialized) return;

            FirebaseAnalytics.LogEvent("challenge_created",
                new Parameter("game_type", gameType)
            );

            LogDebug($"challenge_created: {gameType}");
        }

        #endregion

        #region User Properties

        // AUDIT-FIXED [2026-03-10] M-05: corregido typo "odId" → "userId"
        public void SetUserId(string userId)
        {
            if (!_isInitialized) return;

            FirebaseAnalytics.SetUserId(userId);
            LogDebug($"set_user_id: (redacted)");
        }

        public void SetUserProperty(string name, string value)
        {
            if (!_isInitialized) return;

            FirebaseAnalytics.SetUserProperty(name, value);
            LogDebug($"set_user_property: {name}={value}");
        }

        public void SetUserPremiumStatus(bool isPremium)
        {
            SetUserProperty("is_premium", isPremium ? "true" : "false");
        }

        public void SetUserCountry(string countryCode)
        {
            SetUserProperty("country", countryCode);
        }

        public void SetUserLevel(int level)
        {
            SetUserProperty("player_level", level.ToString());
        }

        #endregion

        #region Daily Rewards / Missions

        public void LogDailyRewardClaimed(int day, string rewardType, int rewardAmount)
        {
            if (!_isInitialized) return;

            FirebaseAnalytics.LogEvent("daily_reward_claimed",
                new Parameter("day", day),
                new Parameter("reward_type", rewardType),
                new Parameter("reward_amount", rewardAmount)
            );

            LogDebug($"daily_reward_claimed: day {day}, {rewardAmount} {rewardType}");
        }

        public void LogMissionCompleted(string missionId, string rewardType, int rewardAmount)
        {
            if (!_isInitialized) return;

            FirebaseAnalytics.LogEvent("mission_completed",
                new Parameter("mission_id", missionId),
                new Parameter("reward_type", rewardType),
                new Parameter("reward_amount", rewardAmount)
            );

            LogDebug($"mission_completed: {missionId}");
        }

        #endregion

        #region Login Events

        public void LogLogin(string method)
        {
            if (!_isInitialized) return;

            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLogin,
                new Parameter(FirebaseAnalytics.ParameterMethod, method)
            );

            LogDebug($"login: {method}");
        }

        public void LogSignUp(string method)
        {
            if (!_isInitialized) return;

            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventSignUp,
                new Parameter(FirebaseAnalytics.ParameterMethod, method)
            );

            LogDebug($"sign_up: {method}");
        }

        #endregion

        #region Helpers

        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[Analytics] {message}");
            }
        }

        /// <summary>
        /// Desactiva la coleccion de analytics (para GDPR/privacidad)
        /// </summary>
        public void DisableAnalytics()
        {
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(false);
            _isInitialized = false;
            Debug.Log("[Analytics] Analytics deshabilitado por usuario");
        }

        /// <summary>
        /// Reactiva la coleccion de analytics
        /// </summary>
        public async void EnableAnalytics()
        {
            try
            {
                var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
                if (dependencyStatus != DependencyStatus.Available)
                {
                    Debug.LogError($"[Analytics] Cannot enable analytics — Firebase not available: {dependencyStatus}");
                    return;
                }

                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                _isInitialized = true;
                Debug.Log("[Analytics] Analytics habilitado");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Analytics] Error enabling analytics: {e.Message}");
            }
        }

        /// <summary>
        /// Loga un evento personalizado con parametros arbitrarios.
        /// Usado por sistemas externos (ej: payment failure handlers) que no tienen metodo especifico.
        /// </summary>
        public void LogCustomEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (!_isInitialized) return;

            if (parameters == null || parameters.Count == 0)
            {
                FirebaseAnalytics.LogEvent(eventName);
            }
            else
            {
                var firebaseParams = new List<Parameter>();
                foreach (var kvp in parameters)
                {
                    if (kvp.Value is string s)
                        firebaseParams.Add(new Parameter(kvp.Key, s));
                    else if (kvp.Value is int i)
                        firebaseParams.Add(new Parameter(kvp.Key, i));
                    else if (kvp.Value is long l)
                        firebaseParams.Add(new Parameter(kvp.Key, l));
                    else if (kvp.Value is double d)
                        firebaseParams.Add(new Parameter(kvp.Key, d));
                    else if (kvp.Value is float f)
                        firebaseParams.Add(new Parameter(kvp.Key, (double)f));
                    else
                        firebaseParams.Add(new Parameter(kvp.Key, kvp.Value?.ToString() ?? ""));
                }
                FirebaseAnalytics.LogEvent(eventName, firebaseParams.ToArray());
            }

            LogDebug(eventName);
        }

        #endregion

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
