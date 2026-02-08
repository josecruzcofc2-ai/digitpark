using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitPark.Progression
{
    /// <summary>
    /// Sistema de Misiones - Tareas temporales que se reinician.
    /// - Misiones Diarias: Se reinician cada 24h
    /// - Misiones Semanales: Se reinician cada 7 días
    /// - Misiones de Temporada: Duran toda la temporada (60 días)
    ///
    /// Diferente a Logros: Las misiones son repetibles y dan XP/monedas.
    /// Los Logros son permanentes y dan puntos/títulos.
    /// </summary>
    public class MissionsManager : MonoBehaviour
    {
        public static MissionsManager Instance { get; private set; }

        [Header("Mission Configuration")]
        [SerializeField] private int dailyMissionsCount = 4;
        [SerializeField] private int weeklyMissionsCount = 5;

        // Active missions
        private List<Mission> _activeDailyMissions = new List<Mission>();
        private List<Mission> _activeWeeklyMissions = new List<Mission>();
        private List<Mission> _activeSeasonMissions = new List<Mission>();

        // Timestamps
        private DateTime _lastDailyReset;
        private DateTime _lastWeeklyReset;
        private DateTime _seasonStartDate;

        // Events
        public event Action<Mission> OnMissionProgress;
        public event Action<Mission> OnMissionCompleted;
        public event Action<MissionReward> OnRewardClaimed;
        public event Action OnDailyMissionsReset;
        public event Action OnWeeklyMissionsReset;

        private const string SAVE_KEY = "MissionsData";

        // All possible missions templates
        private static readonly MissionTemplate[] DailyMissionTemplates = new MissionTemplate[]
        {
            // Play missions
            new MissionTemplate("daily_play_1", "Jugador Casual", "Juega 1 partida", MissionType.PlayGames, 1,
                new MissionReward(25, 50, 0)),
            new MissionTemplate("daily_play_3", "Jugador Activo", "Juega 3 partidas", MissionType.PlayGames, 3,
                new MissionReward(50, 100, 0)),
            new MissionTemplate("daily_play_5", "Maratón Diario", "Juega 5 partidas", MissionType.PlayGames, 5,
                new MissionReward(75, 150, 0)),

            // Win missions
            new MissionTemplate("daily_win_1", "Victoria Diaria", "Gana 1 partida", MissionType.WinGames, 1,
                new MissionReward(50, 100, 0)),
            new MissionTemplate("daily_win_3", "Triple Victoria", "Gana 3 partidas", MissionType.WinGames, 3,
                new MissionReward(100, 200, 0)),

            // Specific game missions
            new MissionTemplate("daily_digitrush", "Especialista DigitRush", "Juega 2 partidas de DigitRush", MissionType.PlaySpecificGame, 2,
                new MissionReward(50, 100, 0), "DigitRush"),
            new MissionTemplate("daily_flashtap", "Reflejos del Día", "Juega 2 partidas de FlashTap", MissionType.PlaySpecificGame, 2,
                new MissionReward(50, 100, 0), "FlashTap"),
            new MissionTemplate("daily_memorypairs", "Memoria Diaria", "Juega 2 partidas de MemoryPairs", MissionType.PlaySpecificGame, 2,
                new MissionReward(50, 100, 0), "MemoryPairs"),
            new MissionTemplate("daily_quickmath", "Matemáticas del Día", "Juega 2 partidas de QuickMath", MissionType.PlaySpecificGame, 2,
                new MissionReward(50, 100, 0), "QuickMath"),
            new MissionTemplate("daily_oddoneout", "Observador Diario", "Juega 2 partidas de OddOneOut", MissionType.PlaySpecificGame, 2,
                new MissionReward(50, 100, 0), "OddOneOut"),

            // Score missions
            new MissionTemplate("daily_score_high", "Puntaje Alto", "Alcanza 5,000 puntos en cualquier juego", MissionType.ReachScore, 5000,
                new MissionReward(75, 150, 0)),

            // Precision missions
            new MissionTemplate("daily_precision", "Precisión", "Completa una partida con 80%+ precisión", MissionType.PrecisionGame, 1,
                new MissionReward(75, 150, 0)),
        };

        private static readonly MissionTemplate[] WeeklyMissionTemplates = new MissionTemplate[]
        {
            // Play missions
            new MissionTemplate("weekly_play_15", "Jugador Dedicado", "Juega 15 partidas esta semana", MissionType.PlayGames, 15,
                new MissionReward(300, 750, 10)),
            new MissionTemplate("weekly_play_30", "Maratón Semanal", "Juega 30 partidas esta semana", MissionType.PlayGames, 30,
                new MissionReward(500, 1500, 25)),

            // Win missions
            new MissionTemplate("weekly_win_7", "Ganador Semanal", "Gana 7 partidas esta semana", MissionType.WinGames, 7,
                new MissionReward(400, 1000, 15)),
            new MissionTemplate("weekly_win_15", "Dominador", "Gana 15 partidas esta semana", MissionType.WinGames, 15,
                new MissionReward(600, 2000, 35)),

            // All games
            new MissionTemplate("weekly_all_games", "Versatilidad", "Juega todos los minijuegos", MissionType.PlayAllGames, 5,
                new MissionReward(400, 1000, 20)),

            // Streak
            new MissionTemplate("weekly_streak", "Racha Semanal", "Consigue una racha de 3 victorias", MissionType.WinStreak, 3,
                new MissionReward(350, 800, 15)),

            // Cash Battle
            new MissionTemplate("weekly_cash_1", "Apostador Semanal", "Completa 1 Cash Battle", MissionType.PlayCashBattle, 1,
                new MissionReward(300, 500, 20)),
            new MissionTemplate("weekly_cash_3", "High Stakes", "Completa 3 Cash Battles", MissionType.PlayCashBattle, 3,
                new MissionReward(500, 1000, 40)),

            // Tournament
            new MissionTemplate("weekly_tournament", "Competidor", "Participa en un torneo", MissionType.PlayTournament, 1,
                new MissionReward(400, 1000, 25)),

            // XP
            new MissionTemplate("weekly_xp", "Grinder", "Gana 3,000 XP esta semana", MissionType.EarnXP, 3000,
                new MissionReward(500, 1500, 30)),

            // Score
            new MissionTemplate("weekly_score", "Alto Rendimiento", "Alcanza 50,000 puntos totales", MissionType.TotalScore, 50000,
                new MissionReward(400, 1000, 20)),
        };

        private static readonly MissionTemplate[] SeasonMissionTemplates = new MissionTemplate[]
        {
            // Wins
            new MissionTemplate("season_wins_50", "Veterano de Temporada", "Gana 50 partidas", MissionType.WinGames, 50,
                new MissionReward(1500, 3000, 75)),
            new MissionTemplate("season_wins_100", "Centurión", "Gana 100 partidas", MissionType.WinGames, 100,
                new MissionReward(3000, 7500, 150)),
            new MissionTemplate("season_wins_250", "Leyenda de Temporada", "Gana 250 partidas", MissionType.WinGames, 250,
                new MissionReward(5000, 15000, 300)),

            // Cash Battle
            new MissionTemplate("season_cash_10", "Apostador Serio", "Gana 10 Cash Battles", MissionType.WinCashBattle, 10,
                new MissionReward(2000, 5000, 100)),
            new MissionTemplate("season_cash_25", "Cash Pro", "Gana 25 Cash Battles", MissionType.WinCashBattle, 25,
                new MissionReward(4000, 10000, 200)),

            // Tournament
            new MissionTemplate("season_tournament_3", "Competidor Elite", "Participa en 3 torneos", MissionType.PlayTournament, 3,
                new MissionReward(1500, 3000, 75)),
            new MissionTemplate("season_tournament_win", "Campeón de Temporada", "Gana un torneo", MissionType.WinTournament, 1,
                new MissionReward(3000, 7500, 150)),

            // Social
            new MissionTemplate("season_friends", "Social Butterfly", "Añade 5 amigos", MissionType.AddFriends, 5,
                new MissionReward(1000, 2500, 50)),

            // Login
            new MissionTemplate("season_login_30", "Dedicación", "Inicia sesión 30 días", MissionType.LoginDays, 30,
                new MissionReward(2000, 5000, 100)),
            new MissionTemplate("season_login_50", "Compromiso Total", "Inicia sesión 50 días", MissionType.LoginDays, 50,
                new MissionReward(3000, 7500, 150)),

            // Mastery
            new MissionTemplate("season_mastery", "Polivalente", "Alcanza 10,000 puntos en cada minijuego", MissionType.MasteryAllGames, 5,
                new MissionReward(4000, 10000, 200)),

        };

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadData();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            CheckResets();
        }

        #region Reset Logic

        private void CheckResets()
        {
            DateTime now = DateTime.UtcNow;

            // Check daily reset (every 24h at midnight UTC)
            if (now.Date > _lastDailyReset.Date)
            {
                ResetDailyMissions();
            }

            // Check weekly reset (every Monday at midnight UTC)
            DateTime lastMonday = now.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
            if (now.DayOfWeek == DayOfWeek.Monday && _lastWeeklyReset.Date < lastMonday.Date)
            {
                ResetWeeklyMissions();
            }
            else if (_lastWeeklyReset.AddDays(7) < now)
            {
                ResetWeeklyMissions();
            }

            // Season missions are set once per season and tracked externally
        }

        private void ResetDailyMissions()
        {
            _activeDailyMissions.Clear();

            // Select random daily missions
            var shuffled = DailyMissionTemplates.OrderBy(x => UnityEngine.Random.value).ToList();
            for (int i = 0; i < Mathf.Min(dailyMissionsCount, shuffled.Count); i++)
            {
                _activeDailyMissions.Add(new Mission(shuffled[i], MissionPeriod.Daily));
            }

            _lastDailyReset = DateTime.UtcNow;
            SaveData();
            OnDailyMissionsReset?.Invoke();

            Debug.Log($"[MissionsManager] Daily missions reset! {_activeDailyMissions.Count} new missions available.");
        }

        private void ResetWeeklyMissions()
        {
            _activeWeeklyMissions.Clear();

            // Select random weekly missions
            var shuffled = WeeklyMissionTemplates.OrderBy(x => UnityEngine.Random.value).ToList();
            for (int i = 0; i < Mathf.Min(weeklyMissionsCount, shuffled.Count); i++)
            {
                _activeWeeklyMissions.Add(new Mission(shuffled[i], MissionPeriod.Weekly));
            }

            _lastWeeklyReset = DateTime.UtcNow;
            SaveData();
            OnWeeklyMissionsReset?.Invoke();

            Debug.Log($"[MissionsManager] Weekly missions reset! {_activeWeeklyMissions.Count} new missions available.");
        }

        public void InitializeSeasonMissions(DateTime seasonStart)
        {
            _seasonStartDate = seasonStart;
            _activeSeasonMissions.Clear();

            // All season missions are active
            foreach (var template in SeasonMissionTemplates)
            {
                _activeSeasonMissions.Add(new Mission(template, MissionPeriod.Season));
            }

            SaveData();
            Debug.Log($"[MissionsManager] Season missions initialized! {_activeSeasonMissions.Count} season missions available.");
        }

        #endregion

        #region Progress Tracking

        /// <summary>
        /// Report a game played
        /// </summary>
        public void ReportGamePlayed(string gameId, bool isWin, int score, float precision, bool isCashBattle)
        {
            UpdateMissions(MissionType.PlayGames, 1);

            if (isWin)
            {
                UpdateMissions(MissionType.WinGames, 1);
            }

            UpdateMissions(MissionType.PlaySpecificGame, 1, gameId);
            UpdateMissions(MissionType.TotalScore, score);

            if (score >= 5000)
            {
                UpdateMissions(MissionType.ReachScore, 1);
            }

            if (precision >= 0.8f)
            {
                UpdateMissions(MissionType.PrecisionGame, 1);
            }

            if (isCashBattle)
            {
                UpdateMissions(MissionType.PlayCashBattle, 1);
                if (isWin)
                {
                    UpdateMissions(MissionType.WinCashBattle, 1);
                }
            }

            // Track which games have been played this week for "play all games"
            TrackGamePlayed(gameId);
        }

        /// <summary>
        /// Report a tournament played
        /// </summary>
        public void ReportTournament(int placement, int totalParticipants)
        {
            UpdateMissions(MissionType.PlayTournament, 1);

            if (placement == 1)
            {
                UpdateMissions(MissionType.WinTournament, 1);
            }
        }

        /// <summary>
        /// Report win streak
        /// </summary>
        public void ReportWinStreak(int streakCount)
        {
            // Check if any mission needs this streak
            var allMissions = GetAllActiveMissions();
            foreach (var mission in allMissions)
            {
                if (mission.Template.type == MissionType.WinStreak && streakCount >= mission.Template.targetAmount)
                {
                    mission.CurrentProgress = mission.Template.targetAmount;
                    CheckMissionCompletion(mission);
                }
            }
        }

        /// <summary>
        /// Report XP earned
        /// </summary>
        public void ReportXPEarned(int amount)
        {
            UpdateMissions(MissionType.EarnXP, amount);
        }

        /// <summary>
        /// Report daily login
        /// </summary>
        public void ReportLogin()
        {
            UpdateMissions(MissionType.LoginDays, 1);
        }

        /// <summary>
        /// Report friend added
        /// </summary>
        public void ReportFriendAdded()
        {
            UpdateMissions(MissionType.AddFriends, 1);
        }

        private HashSet<string> _gamesPlayedThisPeriod = new HashSet<string>();

        private void TrackGamePlayed(string gameId)
        {
            if (!_gamesPlayedThisPeriod.Contains(gameId))
            {
                _gamesPlayedThisPeriod.Add(gameId);
                UpdateMissions(MissionType.PlayAllGames, 1);
            }
        }

        private void UpdateMissions(MissionType type, int amount, string specificTarget = null)
        {
            var allMissions = GetAllActiveMissions();

            foreach (var mission in allMissions)
            {
                if (mission.IsCompleted || mission.IsClaimed) continue;
                if (mission.Template.type != type) continue;

                // Check specific target if needed
                if (!string.IsNullOrEmpty(mission.Template.specificTarget) &&
                    mission.Template.specificTarget != specificTarget)
                {
                    continue;
                }

                mission.CurrentProgress += amount;
                OnMissionProgress?.Invoke(mission);

                CheckMissionCompletion(mission);
            }

            SaveData();
        }

        private void CheckMissionCompletion(Mission mission)
        {
            if (mission.CurrentProgress >= mission.Template.targetAmount && !mission.IsCompleted)
            {
                mission.IsCompleted = true;
                mission.CompletedAt = DateTime.UtcNow;
                OnMissionCompleted?.Invoke(mission);

                Debug.Log($"[MissionsManager] Mission completed: {mission.Template.title}");
            }
        }

        #endregion

        #region Reward Claiming

        /// <summary>
        /// Claim reward for a completed mission
        /// </summary>
        public bool ClaimReward(Mission mission)
        {
            if (!mission.IsCompleted || mission.IsClaimed)
            {
                return false;
            }

            mission.IsClaimed = true;
            SaveData();

            OnRewardClaimed?.Invoke(mission.Template.reward);

            Debug.Log($"[MissionsManager] Reward claimed: {mission.Template.reward.xp} XP, " +
                     $"{mission.Template.reward.coins} coins, {mission.Template.reward.gems} gems");

            return true;
        }

        /// <summary>
        /// Claim all completed mission rewards
        /// </summary>
        public MissionReward ClaimAllRewards()
        {
            var totalReward = new MissionReward(0, 0, 0);
            var allMissions = GetAllActiveMissions();

            foreach (var mission in allMissions)
            {
                if (mission.IsCompleted && !mission.IsClaimed)
                {
                    mission.IsClaimed = true;
                    totalReward.xp += mission.Template.reward.xp;
                    totalReward.coins += mission.Template.reward.coins;
                    totalReward.gems += mission.Template.reward.gems;
                }
            }

            SaveData();

            if (totalReward.xp > 0 || totalReward.coins > 0 || totalReward.gems > 0)
            {
                OnRewardClaimed?.Invoke(totalReward);
            }

            return totalReward;
        }

        #endregion

        #region Public Getters

        public List<Mission> GetDailyMissions() => new List<Mission>(_activeDailyMissions);
        public List<Mission> GetWeeklyMissions() => new List<Mission>(_activeWeeklyMissions);
        public List<Mission> GetSeasonMissions() => new List<Mission>(_activeSeasonMissions);

        public List<Mission> GetAllActiveMissions()
        {
            var all = new List<Mission>();
            all.AddRange(_activeDailyMissions);
            all.AddRange(_activeWeeklyMissions);
            all.AddRange(_activeSeasonMissions);
            return all;
        }

        public int GetCompletedMissionsCount(MissionPeriod period)
        {
            return period switch
            {
                MissionPeriod.Daily => _activeDailyMissions.Count(m => m.IsCompleted),
                MissionPeriod.Weekly => _activeWeeklyMissions.Count(m => m.IsCompleted),
                MissionPeriod.Season => _activeSeasonMissions.Count(m => m.IsCompleted),
                _ => 0
            };
        }

        public int GetUnclaimedRewardsCount()
        {
            return GetAllActiveMissions().Count(m => m.IsCompleted && !m.IsClaimed);
        }

        public TimeSpan GetTimeUntilDailyReset()
        {
            DateTime nextReset = _lastDailyReset.Date.AddDays(1);
            TimeSpan remaining = nextReset - DateTime.UtcNow;
            return remaining.TotalSeconds > 0 ? remaining : TimeSpan.Zero;
        }

        public TimeSpan GetTimeUntilWeeklyReset()
        {
            DateTime nextReset = _lastWeeklyReset.AddDays(7);
            TimeSpan remaining = nextReset - DateTime.UtcNow;
            return remaining.TotalSeconds > 0 ? remaining : TimeSpan.Zero;
        }

        #endregion

        #region Save/Load

        private void SaveData()
        {
            var data = new MissionsData
            {
                lastDailyReset = _lastDailyReset.ToBinary(),
                lastWeeklyReset = _lastWeeklyReset.ToBinary(),
                seasonStartDate = _seasonStartDate.ToBinary(),
                dailyMissions = _activeDailyMissions.Select(m => m.ToSaveData()).ToList(),
                weeklyMissions = _activeWeeklyMissions.Select(m => m.ToSaveData()).ToList(),
                seasonMissions = _activeSeasonMissions.Select(m => m.ToSaveData()).ToList(),
                gamesPlayedThisPeriod = _gamesPlayedThisPeriod.ToList()
            };

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        private void LoadData()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                var data = JsonUtility.FromJson<MissionsData>(json);

                _lastDailyReset = DateTime.FromBinary(data.lastDailyReset);
                _lastWeeklyReset = DateTime.FromBinary(data.lastWeeklyReset);
                _seasonStartDate = DateTime.FromBinary(data.seasonStartDate);

                _activeDailyMissions = data.dailyMissions.Select(m => Mission.FromSaveData(m, MissionPeriod.Daily)).ToList();
                _activeWeeklyMissions = data.weeklyMissions.Select(m => Mission.FromSaveData(m, MissionPeriod.Weekly)).ToList();
                _activeSeasonMissions = data.seasonMissions.Select(m => Mission.FromSaveData(m, MissionPeriod.Season)).ToList();
                _gamesPlayedThisPeriod = new HashSet<string>(data.gamesPlayedThisPeriod ?? new List<string>());
            }
            else
            {
                // First time initialization
                _lastDailyReset = DateTime.UtcNow.AddDays(-1); // Force reset
                _lastWeeklyReset = DateTime.UtcNow.AddDays(-8);
                _seasonStartDate = DateTime.UtcNow;
            }
        }

        [ContextMenu("Force Daily Reset (DEBUG)")]
        public void ForceResetDaily()
        {
            ResetDailyMissions();
        }

        [ContextMenu("Force Weekly Reset (DEBUG)")]
        public void ForceResetWeekly()
        {
            ResetWeeklyMissions();
        }

        #endregion
    }

    #region Data Structures

    public enum MissionType
    {
        PlayGames,          // Play X games
        WinGames,           // Win X games
        PlaySpecificGame,   // Play X games of specific type
        WinStreak,          // Achieve X win streak
        ReachScore,         // Reach X score in single game
        TotalScore,         // Accumulate X total score
        PrecisionGame,      // Complete game with X% precision
        PlayCashBattle,     // Play X cash battles
        WinCashBattle,      // Win X cash battles
        PlayTournament,     // Participate in X tournaments
        WinTournament,      // Win X tournaments
        EarnXP,             // Earn X XP
        AddFriends,         // Add X friends
        LoginDays,          // Login X days
        PlayAllGames,       // Play all game types
        MasteryAllGames,    // Reach mastery in all games
    }

    public enum MissionPeriod
    {
        Daily,
        Weekly,
        Season
    }

    [Serializable]
    public class MissionTemplate
    {
        public string id;
        public string title;
        public string description;
        public MissionType type;
        public int targetAmount;
        public MissionReward reward;
        public string specificTarget; // For specific game missions

        public MissionTemplate(string id, string title, string description, MissionType type, int targetAmount, MissionReward reward, string specificTarget = null)
        {
            this.id = id;
            this.title = title;
            this.description = description;
            this.type = type;
            this.targetAmount = targetAmount;
            this.reward = reward;
            this.specificTarget = specificTarget;
        }
    }

    [Serializable]
    public class MissionReward
    {
        public int xp;
        public int coins;
        public int gems;

        public MissionReward(int xp, int coins, int gems)
        {
            this.xp = xp;
            this.coins = coins;
            this.gems = gems;
        }
    }

    [Serializable]
    public class Mission
    {
        public MissionTemplate Template { get; private set; }
        public MissionPeriod Period { get; private set; }
        public int CurrentProgress { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsClaimed { get; set; }
        public DateTime CompletedAt { get; set; }

        public float Progress => (float)CurrentProgress / Template.targetAmount;
        public string ProgressText => $"{CurrentProgress}/{Template.targetAmount}";

        public Mission(MissionTemplate template, MissionPeriod period)
        {
            Template = template;
            Period = period;
            CurrentProgress = 0;
            IsCompleted = false;
            IsClaimed = false;
        }

        public MissionSaveData ToSaveData()
        {
            return new MissionSaveData
            {
                templateId = Template.id,
                currentProgress = CurrentProgress,
                isCompleted = IsCompleted,
                isClaimed = IsClaimed,
                completedAt = CompletedAt.ToBinary()
            };
        }

        public static Mission FromSaveData(MissionSaveData data, MissionPeriod period)
        {
            MissionTemplate template = FindTemplate(data.templateId, period);
            if (template == null)
            {
                Debug.LogWarning($"[Mission] Template not found: {data.templateId}");
                return null;
            }

            return new Mission(template, period)
            {
                CurrentProgress = data.currentProgress,
                IsCompleted = data.isCompleted,
                IsClaimed = data.isClaimed,
                CompletedAt = DateTime.FromBinary(data.completedAt)
            };
        }

        private static MissionTemplate FindTemplate(string id, MissionPeriod period)
        {
            MissionTemplate[] templates = period switch
            {
                MissionPeriod.Daily => MissionsManager.Instance != null ?
                    typeof(MissionsManager).GetField("DailyMissionTemplates", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?.GetValue(null) as MissionTemplate[] : null,
                MissionPeriod.Weekly => typeof(MissionsManager).GetField("WeeklyMissionTemplates", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?.GetValue(null) as MissionTemplate[],
                MissionPeriod.Season => typeof(MissionsManager).GetField("SeasonMissionTemplates", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?.GetValue(null) as MissionTemplate[],
                _ => null
            };

            return templates?.FirstOrDefault(t => t.id == id);
        }
    }

    [Serializable]
    public class MissionSaveData
    {
        public string templateId;
        public int currentProgress;
        public bool isCompleted;
        public bool isClaimed;
        public long completedAt;
    }

    [Serializable]
    public class MissionsData
    {
        public long lastDailyReset;
        public long lastWeeklyReset;
        public long seasonStartDate;
        public List<MissionSaveData> dailyMissions;
        public List<MissionSaveData> weeklyMissions;
        public List<MissionSaveData> seasonMissions;
        public List<string> gamesPlayedThisPeriod;
    }

    #endregion
}
