using System.Collections.Generic;
using UnityEngine;
using Firebase.Analytics;

namespace DigitPark.Services.Firebase
{
    /// <summary>
    /// Servicio de Analytics para tracking de eventos y métricas
    /// </summary>
    public class AnalyticsService : MonoBehaviour
    {
        public static AnalyticsService Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAnalytics();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeAnalytics()
        {
            Debug.Log("[Analytics] Inicializando Firebase Analytics...");
            
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

            Debug.Log("[Analytics] Analytics inicializado");
        }

        #region Game Events

        /// <summary>
        /// Registra el inicio de una partida
        /// </summary>
        public void LogGameStart()
        {
            LogEvent("game_start");
        }

        /// <summary>
        /// Registra el fin de una partida con el tiempo
        /// </summary>
        public void LogGameComplete(float time, bool isNewRecord)
        {
            var parameters = new Dictionary<string, object>
            {
                { "time", time },
                { "is_new_record", isNewRecord }
            };

            LogEvent("game_complete", parameters);
        }

        /// <summary>
        /// Registra cuando un jugador abandona una partida
        /// </summary>
        public void LogGameAbandoned()
        {
            LogEvent("game_abandoned");
        }

        #endregion

        #region Tournament Events

        /// <summary>
        /// Registra cuando se crea un torneo
        /// </summary>
        public void LogTournamentCreated(string tournamentId, int entryFee)
        {
            var parameters = new Dictionary<string, object>
            {
                { "tournament_id", tournamentId },
                { "entry_fee", entryFee }
            };

            LogEvent("tournament_created", parameters);
        }

        /// <summary>
        /// Registra cuando un jugador se une a un torneo
        /// </summary>
        public void LogTournamentJoined(string tournamentId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "tournament_id", tournamentId }
            };

            LogEvent("tournament_joined", parameters);
        }

        /// <summary>
        /// Registra cuando un torneo finaliza
        /// </summary>
        public void LogTournamentCompleted(string tournamentId, int participants, int position)
        {
            var parameters = new Dictionary<string, object>
            {
                { "tournament_id", tournamentId },
                { "participants", participants },
                { "final_position", position }
            };

            LogEvent("tournament_completed", parameters);
        }

        #endregion

        #region Economy Events

        /// <summary>
        /// Registra ganancia de monedas
        /// </summary>
        public void LogCoinsEarned(int amount, string source)
        {
            var parameters = new Dictionary<string, object>
            {
                { "amount", amount },
                { "source", source }
            };

            LogEvent("coins_earned", parameters);
        }

        /// <summary>
        /// Registra gasto de monedas
        /// </summary>
        public void LogCoinsSpent(int amount, string item)
        {
            var parameters = new Dictionary<string, object>
            {
                { "amount", amount },
                { "item", item }
            };

            LogEvent("coins_spent", parameters);
        }

        /// <summary>
        /// Registra compra de gemas
        /// </summary>
        public void LogGemsPurchased(int amount, float price)
        {
            var parameters = new Dictionary<string, object>
            {
                { "amount", amount },
                { "price", price }
            };

            LogEvent("gems_purchased", parameters);
        }

        #endregion

        #region User Progress

        /// <summary>
        /// Registra subida de nivel
        /// </summary>
        public void LogLevelUp(int newLevel)
        {
            var parameters = new Dictionary<string, object>
            {
                { "level", newLevel }
            };

            LogEvent("level_up", parameters);

            // Descomentar con Firebase SDK:
            // FirebaseAnalytics.SetUserProperty("player_level", newLevel.ToString());
        }

        /// <summary>
        /// Registra logro desbloqueado
        /// </summary>
        public void LogAchievementUnlocked(string achievementId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "achievement_id", achievementId }
            };

            LogEvent("achievement_unlocked", parameters);
        }

        #endregion

        #region Ads & Monetization

        /// <summary>
        /// Registra visualización de anuncio
        /// </summary>
        public void LogAdWatched(string adType, bool completed)
        {
            var parameters = new Dictionary<string, object>
            {
                { "ad_type", adType },
                { "completed", completed }
            };

            LogEvent("ad_watched", parameters);
        }

        /// <summary>
        /// Registra compra in-app
        /// </summary>
        public void LogIAPPurchase(string productId, float price, string currency)
        {
            var parameters = new Dictionary<string, object>
            {
                { "product_id", productId },
                { "price", price },
                { "currency", currency }
            };

            LogEvent("iap_purchase", parameters);

            // Descomentar con Firebase SDK:
            // FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPurchase, parameters);
        }

        #endregion

        #region User Properties

        /// <summary>
        /// Establece el ID del usuario
        /// </summary>
        public void SetUserId(string userId)
        {
            Debug.Log($"[Analytics] Usuario ID: {userId}");

            // Descomentar con Firebase SDK:
            // FirebaseAnalytics.SetUserId(userId);
        }

        /// <summary>
        /// Establece si el usuario es premium
        /// </summary>
        public void SetUserPremiumStatus(bool isPremium)
        {
            // Descomentar con Firebase SDK:
            // FirebaseAnalytics.SetUserProperty("is_premium", isPremium.ToString());
        }

        /// <summary>
        /// Establece el país del usuario
        /// </summary>
        public void SetUserCountry(string countryCode)
        {
            // Descomentar con Firebase SDK:
            // FirebaseAnalytics.SetUserProperty("country", countryCode);
        }

        #endregion

        #region Core

        /// <summary>
        /// Método principal para registrar eventos
        /// </summary>
        private void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            Debug.Log($"[Analytics] Evento: {eventName}");

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    Debug.Log($"  - {param.Key}: {param.Value}");
                }
            }

            // Descomentar con Firebase SDK:
            /*
            if (parameters != null)
            {
                var paramArray = new Parameter[parameters.Count];
                int i = 0;
                foreach (var param in parameters)
                {
                    paramArray[i++] = new Parameter(param.Key, param.Value.ToString());
                }
                FirebaseAnalytics.LogEvent(eventName, paramArray);
            }
            else
            {
                FirebaseAnalytics.LogEvent(eventName);
            }
            */
        }

        #endregion
    }
}
