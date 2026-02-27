using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DigitPark.Services.Firebase;
using DigitPark.Data;

namespace DigitPark.Services
{
    /// <summary>
    /// Servicio de recompensas diarias
    /// Recompensa a los jugadores por iniciar sesión consecutivamente
    /// </summary>
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
            // Día 1: 100 DigitCoins
            rewards.Add(new DailyReward
            {
                day = 1,
                type = RewardType.DigitCoins,
                amount = 100,
                description = "100 DigitCoins"
            });

            // Día 2: 150 DigitCoins
            rewards.Add(new DailyReward
            {
                day = 2,
                type = RewardType.DigitCoins,
                amount = 150,
                description = "150 DigitCoins"
            });

            // Día 3: 200 DigitCoins
            rewards.Add(new DailyReward
            {
                day = 3,
                type = RewardType.DigitCoins,
                amount = 200,
                description = "200 DigitCoins"
            });

            // Día 4: 250 DigitCoins
            rewards.Add(new DailyReward
            {
                day = 4,
                type = RewardType.DigitCoins,
                amount = 250,
                description = "250 DigitCoins"
            });

            // Día 5: 300 DigitCoins + bonus
            rewards.Add(new DailyReward
            {
                day = 5,
                type = RewardType.DigitCoins,
                amount = 300,
                description = "300 DigitCoins"
            });

            // Día 6: 400 DigitCoins
            rewards.Add(new DailyReward
            {
                day = 6,
                type = RewardType.DigitCoins,
                amount = 400,
                description = "400 DigitCoins"
            });

            // Día 7: Gran premio - 500 DigitCoins
            rewards.Add(new DailyReward
            {
                day = 7,
                type = RewardType.DigitCoins,
                amount = 500,
                isSpecial = true,
                description = "500 DigitCoins (Premio Especial)"
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
            _ = SaveToFirebase();
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

                // TODO: Implementar actualización parcial en DatabaseService
                // await DatabaseService.Instance?.UpdatePlayerFields(playerData.userId, updates);
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

            DateTime lastClaim = DateTime.Parse(_data.lastClaimDate);
            DateTime today = DateTime.Today;
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

            DateTime lastClaim = DateTime.Parse(_data.lastClaimDate);
            DateTime today = DateTime.Today;

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
            _data.lastClaimDate = DateTime.Today.ToString("yyyy-MM-dd");
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

            switch (reward.type)
            {
                case RewardType.DigitCoins:
                    // TODO: Implementar sistema de DigitCoins cuando se agregue al PlayerData
                    Debug.Log($"[DailyReward] +{reward.amount} DigitCoins (pendiente implementar)");
                    // Analytics para DigitCoins ganadas
                    AnalyticsService.Instance?.LogVirtualCurrencyEarned("digitcoins", reward.amount, "daily_reward");
                    break;

                case RewardType.DigitGems:
                    // TODO: Implementar sistema de DigitGems si es necesario
                    Debug.Log($"[DailyReward] +{reward.amount} DigitGems");
                    break;

                case RewardType.PremiumTime:
                    // TODO: Dar tiempo premium temporal
                    Debug.Log($"[DailyReward] +{reward.amount} horas premium");
                    break;

                case RewardType.Multiplier:
                    // TODO: Activar multiplicador temporal
                    Debug.Log($"[DailyReward] Multiplicador x{reward.amount} activado");
                    break;

                case RewardType.RandomBox:
                    // TODO: Abrir caja random
                    Debug.Log($"[DailyReward] Caja misteriosa obtenida");
                    break;
            }

            // Guardar datos del jugador
            if (playerData != null)
            {
                _ = DatabaseService.Instance?.SavePlayerData(playerData);
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

            DateTime tomorrow = DateTime.Today.AddDays(1);
            return tomorrow - DateTime.Now;
        }

        /// <summary>
        /// Reinicia los datos de recompensas (debug)
        /// </summary>
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
                DateTime lastClaim = DateTime.Parse(_data.lastClaimDate);
                _data.lastClaimDate = lastClaim.AddDays(-1).ToString("yyyy-MM-dd");
                SaveData();
                CheckDailyReset();
                Debug.Log("[DailyReward] Día simulado pasado");
            }
        }

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
