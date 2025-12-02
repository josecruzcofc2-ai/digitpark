using System.Collections.Generic;
using UnityEngine;

namespace DigitPark.Services.Firebase
{
    /// <summary>
    /// Servicio de Analytics (Modo Simulación)
    /// Solo hace logs locales
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
                Debug.Log("[Analytics] Servicio inicializado (Simulación)");
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #region Game Events

        public void LogGameStart()
        {
            LogEvent("game_start");
        }

        public void LogGameComplete(float time, bool isNewRecord)
        {
            LogEvent("game_complete", new Dictionary<string, object>
            {
                { "time", time },
                { "is_new_record", isNewRecord }
            });
        }

        public void LogGameAbandoned()
        {
            LogEvent("game_abandoned");
        }

        #endregion

        #region Tournament Events

        public void LogTournamentCreated(string tournamentId, int entryFee)
        {
            LogEvent("tournament_created", new Dictionary<string, object>
            {
                { "tournament_id", tournamentId },
                { "entry_fee", entryFee }
            });
        }

        public void LogTournamentJoined(string tournamentId)
        {
            LogEvent("tournament_joined", new Dictionary<string, object>
            {
                { "tournament_id", tournamentId }
            });
        }

        public void LogTournamentCompleted(string tournamentId, int participants, int position)
        {
            LogEvent("tournament_completed", new Dictionary<string, object>
            {
                { "tournament_id", tournamentId },
                { "participants", participants },
                { "final_position", position }
            });
        }

        #endregion

        #region Economy Events

        public void LogCoinsEarned(int amount, string source)
        {
            LogEvent("coins_earned", new Dictionary<string, object>
            {
                { "amount", amount },
                { "source", source }
            });
        }

        public void LogCoinsSpent(int amount, string item)
        {
            LogEvent("coins_spent", new Dictionary<string, object>
            {
                { "amount", amount },
                { "item", item }
            });
        }

        public void LogGemsPurchased(int amount, float price)
        {
            LogEvent("gems_purchased", new Dictionary<string, object>
            {
                { "amount", amount },
                { "price", price }
            });
        }

        #endregion

        #region User Progress

        public void LogLevelUp(int newLevel)
        {
            LogEvent("level_up", new Dictionary<string, object>
            {
                { "level", newLevel }
            });
        }

        public void LogAchievementUnlocked(string achievementId)
        {
            LogEvent("achievement_unlocked", new Dictionary<string, object>
            {
                { "achievement_id", achievementId }
            });
        }

        #endregion

        #region Ads & Monetization

        public void LogAdWatched(string adType, bool completed)
        {
            LogEvent("ad_watched", new Dictionary<string, object>
            {
                { "ad_type", adType },
                { "completed", completed }
            });
        }

        public void LogIAPPurchase(string productId, float price, string currency)
        {
            LogEvent("iap_purchase", new Dictionary<string, object>
            {
                { "product_id", productId },
                { "price", price },
                { "currency", currency }
            });
        }

        #endregion

        #region User Properties

        public void SetUserId(string userId)
        {
            Debug.Log($"[Analytics] Usuario ID: {userId}");
        }

        public void SetUserPremiumStatus(bool isPremium)
        {
            Debug.Log($"[Analytics] Premium: {isPremium}");
        }

        public void SetUserCountry(string countryCode)
        {
            Debug.Log($"[Analytics] País: {countryCode}");
        }

        #endregion

        private void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            string log = $"[Analytics] {eventName}";

            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    log += $" | {p.Key}={p.Value}";
                }
            }

            Debug.Log(log);
        }
    }
}
