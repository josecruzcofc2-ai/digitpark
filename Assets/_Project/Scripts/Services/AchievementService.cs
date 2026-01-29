using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DigitPark.Services.Firebase;
using DigitPark.Data;

namespace DigitPark.Services
{
    /// <summary>
    /// Servicio de logros/achievements
    /// Trackea progreso y desbloquea logros
    /// </summary>
    public class AchievementService : MonoBehaviour
    {
        private static AchievementService _instance;
        public static AchievementService Instance => _instance;

        [Header("Achievements")]
        [SerializeField] private List<AchievementData> achievements = new List<AchievementData>();

        // Estado
        private Dictionary<string, AchievementProgress> _progress = new Dictionary<string, AchievementProgress>();
        private bool _isInitialized = false;

        // Eventos
        public event Action<AchievementData> OnAchievementUnlocked;
        public event Action<AchievementData, float> OnAchievementProgress; // (logro, progreso 0-1)

        // Keys
        private const string ACHIEVEMENTS_KEY = "Achievements_Progress";

        /// <summary>
        /// Lista de todos los logros
        /// </summary>
        public List<AchievementData> AllAchievements => achievements;

        /// <summary>
        /// Logros desbloqueados
        /// </summary>
        public List<AchievementData> UnlockedAchievements
        {
            get
            {
                var unlocked = new List<AchievementData>();
                foreach (var achievement in achievements)
                {
                    if (IsUnlocked(achievement.id))
                        unlocked.Add(achievement);
                }
                return unlocked;
            }
        }

        /// <summary>
        /// Porcentaje total de completado
        /// </summary>
        public float CompletionPercentage
        {
            get
            {
                if (achievements.Count == 0) return 0;
                return (float)UnlockedAchievements.Count / achievements.Count;
            }
        }

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
            // Configurar logros por defecto
            if (achievements.Count == 0)
            {
                SetupDefaultAchievements();
            }

            // Cargar progreso
            LoadProgress();

            _isInitialized = true;
            Debug.Log($"[Achievements] Inicializado con {achievements.Count} logros, {UnlockedAchievements.Count} desbloqueados");
        }

        private void SetupDefaultAchievements()
        {
            // === JUEGOS ===
            achievements.Add(new AchievementData
            {
                id = "first_game",
                titleKey = "ach_first_game",
                descriptionKey = "ach_first_game_desc",
                category = AchievementCategory.Games,
                targetValue = 1,
                rewardCoins = 50
            });

            achievements.Add(new AchievementData
            {
                id = "games_10",
                titleKey = "ach_games_10",
                descriptionKey = "ach_games_10_desc",
                category = AchievementCategory.Games,
                targetValue = 10,
                rewardCoins = 100
            });

            achievements.Add(new AchievementData
            {
                id = "games_50",
                titleKey = "ach_games_50",
                descriptionKey = "ach_games_50_desc",
                category = AchievementCategory.Games,
                targetValue = 50,
                rewardCoins = 250
            });

            achievements.Add(new AchievementData
            {
                id = "games_100",
                titleKey = "ach_games_100",
                descriptionKey = "ach_games_100_desc",
                category = AchievementCategory.Games,
                targetValue = 100,
                rewardCoins = 500
            });

            // === VICTORIAS ===
            achievements.Add(new AchievementData
            {
                id = "first_win",
                titleKey = "ach_first_win",
                descriptionKey = "ach_first_win_desc",
                category = AchievementCategory.Wins,
                targetValue = 1,
                rewardCoins = 100
            });

            achievements.Add(new AchievementData
            {
                id = "wins_10",
                titleKey = "ach_wins_10",
                descriptionKey = "ach_wins_10_desc",
                category = AchievementCategory.Wins,
                targetValue = 10,
                rewardCoins = 200
            });

            achievements.Add(new AchievementData
            {
                id = "wins_50",
                titleKey = "ach_wins_50",
                descriptionKey = "ach_wins_50_desc",
                category = AchievementCategory.Wins,
                targetValue = 50,
                rewardCoins = 500
            });

            // === RACHA ===
            achievements.Add(new AchievementData
            {
                id = "streak_3",
                titleKey = "ach_streak_3",
                descriptionKey = "ach_streak_3_desc",
                category = AchievementCategory.Streak,
                targetValue = 3,
                rewardCoins = 150
            });

            achievements.Add(new AchievementData
            {
                id = "streak_7",
                titleKey = "ach_streak_7",
                descriptionKey = "ach_streak_7_desc",
                category = AchievementCategory.Streak,
                targetValue = 7,
                rewardCoins = 350
            });

            // === SOCIAL ===
            achievements.Add(new AchievementData
            {
                id = "first_friend",
                titleKey = "ach_first_friend",
                descriptionKey = "ach_first_friend_desc",
                category = AchievementCategory.Social,
                targetValue = 1,
                rewardCoins = 100
            });

            achievements.Add(new AchievementData
            {
                id = "friends_5",
                titleKey = "ach_friends_5",
                descriptionKey = "ach_friends_5_desc",
                category = AchievementCategory.Social,
                targetValue = 5,
                rewardCoins = 250
            });

            // === RECORDS ===
            achievements.Add(new AchievementData
            {
                id = "sub_5_seconds",
                titleKey = "ach_sub_5",
                descriptionKey = "ach_sub_5_desc",
                category = AchievementCategory.Records,
                targetValue = 1,
                rewardCoins = 200
            });

            achievements.Add(new AchievementData
            {
                id = "sub_3_seconds",
                titleKey = "ach_sub_3",
                descriptionKey = "ach_sub_3_desc",
                category = AchievementCategory.Records,
                targetValue = 1,
                rewardCoins = 500
            });

            // === DAILY ===
            achievements.Add(new AchievementData
            {
                id = "daily_7",
                titleKey = "ach_daily_7",
                descriptionKey = "ach_daily_7_desc",
                category = AchievementCategory.Daily,
                targetValue = 7,
                rewardCoins = 300
            });

            achievements.Add(new AchievementData
            {
                id = "daily_30",
                titleKey = "ach_daily_30",
                descriptionKey = "ach_daily_30_desc",
                category = AchievementCategory.Daily,
                targetValue = 30,
                rewardCoins = 1000
            });
        }

        #endregion

        #region Progress Management

        private void LoadProgress()
        {
            _progress.Clear();
            string json = PlayerPrefs.GetString(ACHIEVEMENTS_KEY, "");

            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var data = JsonUtility.FromJson<AchievementSaveData>(json);
                    if (data?.items != null)
                    {
                        foreach (var item in data.items)
                        {
                            _progress[item.id] = item;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Achievements] Error cargando progreso: {e.Message}");
                }
            }
        }

        private void SaveProgress()
        {
            var data = new AchievementSaveData
            {
                items = new List<AchievementProgress>(new List<AchievementProgress>(_progress.Values))
            };

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(ACHIEVEMENTS_KEY, json);
            PlayerPrefs.Save();

            // Guardar en Firebase
            _ = SaveToFirebase();
        }

        private async Task SaveToFirebase()
        {
            var playerData = AuthenticationService.Instance?.GetCurrentPlayerData();
            if (playerData == null) return;

            try
            {
                // TODO: Implementar guardado en Firebase
                await Task.CompletedTask;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Achievements] Error guardando en Firebase: {e.Message}");
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Verifica si un logro está desbloqueado
        /// </summary>
        public bool IsUnlocked(string achievementId)
        {
            return _progress.TryGetValue(achievementId, out var progress) && progress.unlocked;
        }

        /// <summary>
        /// Obtiene el progreso de un logro (0-1)
        /// </summary>
        public float GetProgress(string achievementId)
        {
            var achievement = achievements.Find(a => a.id == achievementId);
            if (achievement == null) return 0;

            if (_progress.TryGetValue(achievementId, out var progress))
            {
                if (progress.unlocked) return 1f;
                return (float)progress.currentValue / achievement.targetValue;
            }
            return 0;
        }

        /// <summary>
        /// Obtiene el valor actual de progreso
        /// </summary>
        public int GetCurrentValue(string achievementId)
        {
            if (_progress.TryGetValue(achievementId, out var progress))
            {
                return progress.currentValue;
            }
            return 0;
        }

        /// <summary>
        /// Incrementa el progreso de un logro
        /// </summary>
        public void AddProgress(string achievementId, int amount = 1)
        {
            if (IsUnlocked(achievementId)) return;

            var achievement = achievements.Find(a => a.id == achievementId);
            if (achievement == null) return;

            if (!_progress.TryGetValue(achievementId, out var progress))
            {
                progress = new AchievementProgress { id = achievementId };
                _progress[achievementId] = progress;
            }

            progress.currentValue += amount;

            float progressPercent = (float)progress.currentValue / achievement.targetValue;
            OnAchievementProgress?.Invoke(achievement, progressPercent);

            if (progress.currentValue >= achievement.targetValue)
            {
                UnlockAchievement(achievementId);
            }
            else
            {
                SaveProgress();
            }
        }

        /// <summary>
        /// Establece el progreso de un logro a un valor específico
        /// </summary>
        public void SetProgress(string achievementId, int value)
        {
            if (IsUnlocked(achievementId)) return;

            var achievement = achievements.Find(a => a.id == achievementId);
            if (achievement == null) return;

            if (!_progress.TryGetValue(achievementId, out var progress))
            {
                progress = new AchievementProgress { id = achievementId };
                _progress[achievementId] = progress;
            }

            progress.currentValue = value;

            if (progress.currentValue >= achievement.targetValue)
            {
                UnlockAchievement(achievementId);
            }
            else
            {
                float progressPercent = (float)progress.currentValue / achievement.targetValue;
                OnAchievementProgress?.Invoke(achievement, progressPercent);
                SaveProgress();
            }
        }

        /// <summary>
        /// Desbloquea un logro directamente
        /// </summary>
        public void UnlockAchievement(string achievementId)
        {
            if (IsUnlocked(achievementId)) return;

            var achievement = achievements.Find(a => a.id == achievementId);
            if (achievement == null) return;

            if (!_progress.TryGetValue(achievementId, out var progress))
            {
                progress = new AchievementProgress { id = achievementId };
                _progress[achievementId] = progress;
            }

            progress.unlocked = true;
            progress.unlockedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            progress.currentValue = achievement.targetValue;

            // Dar recompensa
            GiveReward(achievement);

            SaveProgress();

            // Analytics - usar LogMissionCompleted para achievements
            AnalyticsService.Instance?.LogMissionCompleted(
                achievementId,
                "achievement",
                achievement.rewardCoins
            );

            Debug.Log($"[Achievements] Desbloqueado: {achievementId}");

            OnAchievementUnlocked?.Invoke(achievement);
        }

        private void GiveReward(AchievementData achievement)
        {
            if (achievement.rewardCoins > 0)
            {
                // TODO: Implementar sistema de monedas cuando se agregue al PlayerData
                Debug.Log($"[Achievements] +{achievement.rewardCoins} monedas por logro (pendiente implementar)");
                // Analytics para monedas virtuales ganadas
                AnalyticsService.Instance?.LogVirtualCurrencyEarned("coins", achievement.rewardCoins, "achievement");
            }
        }

        /// <summary>
        /// Obtiene logros por categoría
        /// </summary>
        public List<AchievementData> GetByCategory(AchievementCategory category)
        {
            return achievements.FindAll(a => a.category == category);
        }

        #endregion

        #region Game Event Handlers

        /// <summary>
        /// Llamar cuando se completa un juego
        /// </summary>
        public void OnGameCompleted(bool won, float time)
        {
            // Juegos jugados
            AddProgress("first_game", 1);
            AddProgress("games_10", 1);
            AddProgress("games_50", 1);
            AddProgress("games_100", 1);

            // Victorias
            if (won)
            {
                AddProgress("first_win", 1);
                AddProgress("wins_10", 1);
                AddProgress("wins_50", 1);
            }

            // Records de tiempo
            if (time < 5f)
            {
                UnlockAchievement("sub_5_seconds");
            }
            if (time < 3f)
            {
                UnlockAchievement("sub_3_seconds");
            }
        }

        /// <summary>
        /// Llamar cuando se agrega un amigo
        /// </summary>
        public void OnFriendAdded(int totalFriends)
        {
            AddProgress("first_friend", 1);
            SetProgress("friends_5", totalFriends);
        }

        /// <summary>
        /// Llamar cuando se reclama recompensa diaria
        /// </summary>
        public void OnDailyRewardClaimed(int consecutiveDays)
        {
            SetProgress("daily_7", consecutiveDays);
            SetProgress("daily_30", consecutiveDays);
            SetProgress("streak_3", consecutiveDays);
            SetProgress("streak_7", consecutiveDays);
        }

        #endregion

        #region Debug

        [ContextMenu("Debug: Reset All Achievements")]
        public void ResetAllAchievements()
        {
            _progress.Clear();
            PlayerPrefs.DeleteKey(ACHIEVEMENTS_KEY);
            PlayerPrefs.Save();
            Debug.Log("[Achievements] Todos los logros reseteados");
        }

        [ContextMenu("Debug: Unlock All Achievements")]
        public void UnlockAllAchievements()
        {
            foreach (var achievement in achievements)
            {
                UnlockAchievement(achievement.id);
            }
        }

        #endregion
    }

    #region Data Classes

    public enum AchievementCategory
    {
        Games,      // Juegos jugados
        Wins,       // Victorias
        Streak,     // Racha
        Social,     // Amigos
        Records,    // Records de tiempo
        Daily,      // Recompensas diarias
        Special     // Especiales
    }

    [Serializable]
    public class AchievementData
    {
        public string id;
        public string titleKey;
        public string descriptionKey;
        public AchievementCategory category;
        public int targetValue = 1;
        public int rewardCoins = 0;
        public Sprite icon;
        public bool isHidden = false;
    }

    [Serializable]
    public class AchievementProgress
    {
        public string id;
        public int currentValue = 0;
        public bool unlocked = false;
        public string unlockedDate = "";
    }

    [Serializable]
    public class AchievementSaveData
    {
        public List<AchievementProgress> items = new List<AchievementProgress>();
    }

    #endregion
}
