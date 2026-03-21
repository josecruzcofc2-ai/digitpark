using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;
using DigitPark.Services.Firebase;
using DigitPark.Data;
using DigitPark.Localization;

namespace DigitPark.Services
{
    /// <summary>
    /// DEPRECATED — P0-1: DailyRewardService is no longer the source of truth.
    /// All daily reward logic has been migrated to DailyRewardsManager (scene-level).
    /// This class is retained only for backward compatibility during migration.
    /// DO NOT ADD NEW LOGIC HERE. Remove this class in a future cleanup pass.
    /// </summary>
    [System.Obsolete("Use DailyRewardsManager instead. DailyRewardService is deprecated.", false)]
    public class DailyRewardService : MonoBehaviour
    {
        private static DailyRewardService _instance;
        public static DailyRewardService Instance => _instance;

        [Header("Configuration")]
        [SerializeField] private List<DailyReward> rewards = new List<DailyReward>();
        [SerializeField] private int cycleLength = 7; // Días antes de reiniciar el ciclo

        // Estado
        private DailyRewardData _data;
        private bool _isInitialized = false;

        // Eventos
        public event Action<DailyReward, int> OnRewardAvailable;    // (reward, día)
        public event Action<DailyReward, int> OnRewardClaimed;      // (reward, día)
        public event Action<int> OnStreakUpdated;                    // (días consecutivos)

        // Keys
        private const string DAILY_REWARD_KEY = "DailyReward_Data";

        /// <summary>
        /// Datos actuales de recompensas
        /// </summary>
        public DailyRewardData Data => _data;

        /// <summary>
        /// Si hay una recompensa disponible para reclamar
        /// </summary>
        public bool HasRewardAvailable => _isInitialized && CanClaimToday();

        /// <summary>
        /// Día actual en el ciclo (1-7)
        /// </summary>
        public int CurrentDay => _data?.currentDay ?? 1;

        /// <summary>
        /// Días consecutivos
        /// </summary>
        public int ConsecutiveDays => _data?.consecutiveDays ?? 0;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            Initialize();
        }

        #region Initialization

        private void Initialize()
        {
            // Configurar recompensas por defecto si no hay
            if (rewards.Count == 0)
            {
                SetupDefaultRewards();
            }

            // Cargar datos
            LoadData();

            // Verificar si hay recompensa disponible
            CheckDailyReset();

            _isInitialized = true;
            Debug.Log($"[DailyReward] Inicializado - Día {_data.currentDay}, Streak: {_data.consecutiveDays}");

            // Notificar si hay recompensa disponible
            if (CanClaimToday())
            {
                var reward = GetCurrentReward();
                OnRewardAvailable?.Invoke(reward, _data.currentDay);
            }
        }

        private void SetupDefaultRewards()
        {
            // Día 1: 50 DC
            rewards.Add(new DailyReward
            {
                day = 1,
                type = RewardType.DigitCoins,
                amount = 50,
                description = AutoLocalizer.Get("daily_reward_coins", 50)
            });

            // Día 2: 75 DC
            rewards.Add(new DailyReward
            {
                day = 2,
                type = RewardType.DigitCoins,
                amount = 75,
                description = AutoLocalizer.Get("daily_reward_coins", 75)
            });

            // Día 3: 200 DC (era 5 DG — DG eliminado de daily rewards)
            rewards.Add(new DailyReward
            {
                day = 3,
                type = RewardType.DigitCoins,
                amount = 200,
                description = AutoLocalizer.Get("daily_reward_coins", 200)
            });

            // Día 4: 100 DC
            rewards.Add(new DailyReward
            {
                day = 4,
                type = RewardType.DigitCoins,
                amount = 100,
                description = AutoLocalizer.Get("daily_reward_coins", 100)
            });

            // Día 5: 125 DC (era 200 XP — XP eliminado de daily rewards)
            rewards.Add(new DailyReward
            {
                day = 5,
                type = RewardType.DigitCoins,
                amount = 125,
                description = AutoLocalizer.Get("daily_reward_coins", 125)
            });

            // Día 6: 150 DC
            rewards.Add(new DailyReward
            {
                day = 6,
                type = RewardType.DigitCoins,
                amount = 150,
                description = AutoLocalizer.Get("daily_reward_coins", 150)
            });

            // Día 7: 500 DC — gran premio semanal (era 25 DG — DG eliminado)
            rewards.Add(new DailyReward
            {
                day = 7,
                type = RewardType.DigitCoins,
                amount = 500,
                isSpecial = true,
                description = AutoLocalizer.Get("daily_reward_special", 500)
            });

            cycleLength = rewards.Count;
        }

        #endregion

        #region Data Management

        private void LoadData()
        {
            string json = PlayerPrefs.GetString(DAILY_REWARD_KEY, "");

            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    _data = JsonUtility.FromJson<DailyRewardData>(json);
                }
                catch
                {
                    _data = new DailyRewardData();
                }
            }
            else
            {
                _data = new DailyRewardData();
            }
        }

        private void SaveData()
        {
            string json = JsonUtility.ToJson(_data);
            PlayerPrefs.SetString(DAILY_REWARD_KEY, json);
            PlayerPrefs.Save();

            // También guardar en Firebase si hay usuario
            _ = SaveToFirebase().ContinueWith(t =>
            {
                if (t.IsFaulted)
                    Debug.LogWarning($"[DailyReward] Firebase save failed: {t.Exception?.GetBaseException().Message}");
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async Task SaveToFirebase()
        {
            var playerData = AuthenticationService.Instance?.GetCurrentPlayerData();
            if (playerData == null) return;

            try
            {
                var updates = new Dictionary<string, object>
                {
                    { "dailyReward/currentDay", _data.currentDay },
                    { "dailyReward/consecutiveDays", _data.consecutiveDays },
                    { "dailyReward/lastClaimDate", _data.lastClaimDate },
                    { "dailyReward/totalClaimed", _data.totalClaimed }
                };

                if (DatabaseService.Instance != null)
                {
                    await DatabaseService.Instance.UpdatePlayerFields(playerData.userId, updates);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DailyReward] Error guardando en Firebase: {e.Message}");
            }
        }

        #endregion

        #region Daily Logic

        private void CheckDailyReset()
        {
            if (string.IsNullOrEmpty(_data.lastClaimDate))
            {
                // Primera vez, no hacer nada
                return;
            }

            if (!DateTime.TryParse(_data.lastClaimDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime lastClaim))
            {
                lastClaim = DateTime.UtcNow.Date.AddDays(-2); // Fallback: treat as expired
            }
            DateTime today = DateTime.UtcNow.Date;
            int daysDifference = (today - lastClaim.Date).Days;

            if (daysDifference == 0)
            {
                // Ya reclamó hoy
                Debug.Log("[DailyReward] Ya se reclamó la recompensa de hoy");
            }
            else if (daysDifference == 1)
            {
                // Día consecutivo - avanzar al siguiente día
                _data.currentDay++;
                if (_data.currentDay > cycleLength)
                {
                    _data.currentDay = 1; // Reiniciar ciclo
                }
                Debug.Log($"[DailyReward] Día consecutivo! Ahora en día {_data.currentDay}");
            }
            else
            {
                // Se perdió el streak - reiniciar
                _data.currentDay = 1;
                _data.consecutiveDays = 0;
                Debug.Log("[DailyReward] Streak perdido, reiniciando a día 1");
            }
        }

        private bool CanClaimToday()
        {
            if (string.IsNullOrEmpty(_data.lastClaimDate))
            {
                return true; // Primera vez
            }

            if (!DateTime.TryParse(_data.lastClaimDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime lastClaim))
            {
                lastClaim = DateTime.UtcNow.Date.AddDays(-2); // Fallback: treat as expired
            }
            DateTime today = DateTime.UtcNow.Date;

            return lastClaim.Date < today;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Obtiene la recompensa del día actual
        /// </summary>
        public DailyReward GetCurrentReward()
        {
            int index = _data.currentDay - 1;
            if (index >= 0 && index < rewards.Count)
            {
                return rewards[index];
            }
            return rewards[0];
        }

        /// <summary>
        /// Obtiene la recompensa de un día específico
        /// </summary>
        public DailyReward GetRewardForDay(int day)
        {
            int index = day - 1;
            if (index >= 0 && index < rewards.Count)
            {
                return rewards[index];
            }
            return null;
        }

        /// <summary>
        /// Obtiene todas las recompensas del ciclo
        /// </summary>
        public List<DailyReward> GetAllRewards()
        {
            return new List<DailyReward>(rewards);
        }

        /// <summary>
        /// Reclama la recompensa diaria
        /// </summary>
        public bool ClaimReward()
        {
            if (!CanClaimToday())
            {
                Debug.Log("[DailyReward] Ya se reclamó la recompensa de hoy");
                return false;
            }

            var reward = GetCurrentReward();
            if (reward == null)
            {
                Debug.LogError("[DailyReward] No hay recompensa configurada");
                return false;
            }

            // Aplicar recompensa
            ApplyReward(reward);

            // Actualizar datos
            _data.lastClaimDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
            _data.consecutiveDays++;
            _data.totalClaimed++;

            // Guardar
            SaveData();

            // Analytics
            AnalyticsService.Instance?.LogDailyRewardClaimed(
                _data.currentDay,
                reward.type.ToString(),
                reward.amount
            );

            Debug.Log($"[DailyReward] Recompensa reclamada: {reward.description} (Día {_data.currentDay}, Streak: {_data.consecutiveDays})");

            // Notificar
            OnRewardClaimed?.Invoke(reward, _data.currentDay);
            OnStreakUpdated?.Invoke(_data.consecutiveDays);

            return true;
        }

        /// <summary>
        /// Aplica la recompensa al jugador
        /// </summary>
        private void ApplyReward(DailyReward reward)
        {
            var playerData = AuthenticationService.Instance?.GetCurrentPlayerData();
            var currency = DigitPark.Monetization.CurrencyManager.Instance;

            switch (reward.type)
            {
                case RewardType.DigitCoins:
                    if (currency != null)
                    {
                        currency.AddCoins(reward.amount);
                        Debug.Log($"[DailyReward] +{reward.amount} DigitCoins granted");
                    }
                    else
                    {
                        Debug.LogWarning($"[DailyReward] CurrencyManager not available, could not grant {reward.amount} DigitCoins");
                    }
                    AnalyticsService.Instance?.LogVirtualCurrencyEarned("digitcoins", reward.amount, "daily_reward");
                    break;

                case RewardType.DigitGems:
                    if (currency != null)
                    {
                        currency.AddGems(reward.amount);
                        Debug.Log($"[DailyReward] +{reward.amount} DigitGems granted");
                    }
                    else
                    {
                        Debug.LogWarning($"[DailyReward] CurrencyManager not available, could not grant {reward.amount} DigitGems");
                    }
                    AnalyticsService.Instance?.LogVirtualCurrencyEarned("digitgems", reward.amount, "daily_reward");
                    break;

                case RewardType.PremiumTime:
                    // TODO: Implement premium time grant when PremiumManager supports timed trials
                    Debug.Log($"[DailyReward] +{reward.amount} horas premium (pendiente PremiumManager)");
                    break;

                case RewardType.Multiplier:
                    // TODO: Implement score multiplier when multiplier system is added
                    Debug.Log($"[DailyReward] Multiplicador x{reward.amount} (pendiente implementar)");
                    break;

                case RewardType.RandomBox:
                    // TODO: Implement mystery box opening when loot box system is added
                    Debug.Log($"[DailyReward] Caja misteriosa (pendiente implementar)");
                    break;
            }

            // Guardar datos del jugador
            if (playerData != null)
            {
                var saveTask = DatabaseService.Instance?.SavePlayerData(playerData);
                if (saveTask != null)
                {
                    _ = saveTask.ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                            Debug.LogWarning($"[DailyReward] Player data save failed: {t.Exception?.GetBaseException().Message}");
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }

        /// <summary>
        /// Obtiene el tiempo restante hasta la próxima recompensa
        /// </summary>
        public TimeSpan GetTimeUntilNextReward()
        {
            if (CanClaimToday())
            {
                return TimeSpan.Zero;
            }

            DateTime tomorrow = DateTime.UtcNow.Date.AddDays(1);
            return tomorrow - DateTime.UtcNow;
        }

        /// <summary>
        /// Reinicia los datos de recompensas (debug)
        /// </summary>
#if UNITY_EDITOR
        [ContextMenu("Debug: Reset Daily Rewards")]
        public void ResetData()
        {
            _data = new DailyRewardData();
            SaveData();
            Debug.Log("[DailyReward] Datos reseteados");
        }

        /// <summary>
        /// Simula un día pasado (debug)
        /// </summary>
        [ContextMenu("Debug: Simulate Day Passed")]
        public void SimulateDayPassed()
        {
            if (!string.IsNullOrEmpty(_data.lastClaimDate))
            {
                if (!DateTime.TryParse(_data.lastClaimDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime lastClaim))
            {
                lastClaim = DateTime.UtcNow.Date.AddDays(-2); // Fallback: treat as expired
            }
                _data.lastClaimDate = lastClaim.AddDays(-1).ToString("yyyy-MM-dd");
                SaveData();
                CheckDailyReset();
                Debug.Log("[DailyReward] Día simulado pasado");
            }
        }
#endif // UNITY_EDITOR

        #endregion
    }

    #region Data Classes

    /// <summary>
    /// Tipos de recompensa disponibles
    /// </summary>
    public enum RewardType
    {
        DigitCoins,     // DigitCoins del juego
        DigitGems,      // DigitGems premium
        PremiumTime,    // Tiempo de premium gratis
        Multiplier,     // Multiplicador de puntos
        RandomBox       // Caja misteriosa
    }

    /// <summary>
    /// Definición de una recompensa diaria
    /// </summary>
    [Serializable]
    public class DailyReward
    {
        public int day;
        public RewardType type;
        public int amount;
        public string description;
        public bool isSpecial;
        public Sprite icon;
    }

    /// <summary>
    /// Datos persistentes del sistema de recompensas
    /// </summary>
    [Serializable]
    public class DailyRewardData
    {
        public int currentDay = 1;
        public int consecutiveDays = 0;
        public string lastClaimDate = "";
        public int totalClaimed = 0;
    }

    #endregion
}
