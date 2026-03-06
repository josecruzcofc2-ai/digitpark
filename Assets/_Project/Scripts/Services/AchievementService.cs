using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DigitPark.Services.Firebase;
using DigitPark.Data;

namespace DigitPark.Services
{
    /// <summary>
    /// Servicio central de logros/achievements
    /// Trackea progreso, desbloquea logros, y notifica a la UI
    /// 53 logros en 11 categorias
    /// </summary>
    public class AchievementService : MonoBehaviour
    {
        private static AchievementService _instance;
        public static AchievementService Instance => _instance;

        [Header("Achievements")]
        [SerializeField] private List<AchievementData> achievements = new List<AchievementData>();

        // Estado
        private Dictionary<string, AchievementProgress> _progress = new Dictionary<string, AchievementProgress>();
        #pragma warning disable 0414
        private bool _isInitialized = false;
        #pragma warning restore 0414

        // Eventos
        public event Action<AchievementData> OnAchievementUnlocked;
        public event Action<AchievementData, float> OnAchievementProgress; // (logro, progreso 0-1)

        // Keys
        private const string ACHIEVEMENTS_KEY = "Achievements_Progress";
        private const string MIGRATION_KEY = "Achievements_Migrated_V2";

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

            // Migrar datos antiguos del Manager (one-time)
            MigrateFromManagerPrefs();

            // Cargar progreso
            LoadProgress();

            _isInitialized = true;
            Debug.Log($"[Achievements] Inicializado con {achievements.Count} logros, {UnlockedAchievements.Count} desbloqueados");
        }

        private void SetupDefaultAchievements()
        {
            // ==================== BEGINNER (Onboarding) - 4 logros ====================
            achievements.Add(new AchievementData
            {
                id = "first_game",
                titleKey = "ach_first_game",
                descriptionKey = "ach_first_game_desc",
                category = AchievementCategory.Beginner,
                targetValue = 1,
                rewardCoins = 50,
                rewardGems = 5,
                points = 10,
                iconName = "Logro_Primeros_Pasos"
            });

            achievements.Add(new AchievementData
            {
                id = "tutorial_complete",
                titleKey = "ach_tutorial_complete",
                descriptionKey = "ach_tutorial_complete_desc",
                category = AchievementCategory.Beginner,
                targetValue = 1,
                rewardCoins = 50,
                rewardGems = 5,
                points = 10,
                iconName = "Logro_Graduado"
            });

            achievements.Add(new AchievementData
            {
                id = "first_win",
                titleKey = "ach_first_win",
                descriptionKey = "ach_first_win_desc",
                category = AchievementCategory.Beginner,
                targetValue = 1,
                rewardCoins = 75,
                rewardGems = 5,
                points = 15,
                iconName = "Logro_Primera_Victoria"
            });

            achievements.Add(new AchievementData
            {
                id = "profile_complete",
                titleKey = "ach_profile_complete",
                descriptionKey = "ach_profile_complete_desc",
                category = AchievementCategory.Beginner,
                targetValue = 1,
                rewardCoins = 50,
                rewardGems = 5,
                points = 10,
                iconName = "Logro_Perfil_Completo"
            });

            // ==================== MASTERY (Per Game) - 5 logros ====================
            achievements.Add(new AchievementData
            {
                id = "digitrush_master",
                titleKey = "ach_digitrush_master",
                descriptionKey = "ach_digitrush_master_desc",
                category = AchievementCategory.Mastery,
                targetValue = 10000,
                rewardCoins = 250,
                rewardGems = 15,
                points = 50,
                iconName = "Logro_Maestro_Numeros"
            });

            achievements.Add(new AchievementData
            {
                id = "flashtap_master",
                titleKey = "ach_flashtap_master",
                descriptionKey = "ach_flashtap_master_desc",
                category = AchievementCategory.Mastery,
                targetValue = 100,
                rewardCoins = 250,
                rewardGems = 15,
                points = 50,
                iconName = "Logro_Reflejos_Rayo"
            });

            achievements.Add(new AchievementData
            {
                id = "memorypairs_master",
                titleKey = "ach_memorypairs_master",
                descriptionKey = "ach_memorypairs_master_desc",
                category = AchievementCategory.Mastery,
                targetValue = 1,
                rewardCoins = 250,
                rewardGems = 15,
                points = 50,
                iconName = "Logro_Genio"
            });

            achievements.Add(new AchievementData
            {
                id = "quickmath_master",
                titleKey = "ach_quickmath_master",
                descriptionKey = "ach_quickmath_master_desc",
                category = AchievementCategory.Mastery,
                targetValue = 50,
                rewardCoins = 250,
                rewardGems = 15,
                points = 50,
                iconName = "Logro_Maestro_Matematicas"
            });

            achievements.Add(new AchievementData
            {
                id = "oddoneout_master",
                titleKey = "ach_oddoneout_master",
                descriptionKey = "ach_oddoneout_master_desc",
                category = AchievementCategory.Mastery,
                targetValue = 100,
                rewardCoins = 250,
                rewardGems = 15,
                points = 50,
                iconName = "Logro_Ojo_Aguila"
            });

            // ==================== GAMES PLAYED - 3 logros ====================
            achievements.Add(new AchievementData
            {
                id = "games_10",
                titleKey = "ach_games_10",
                descriptionKey = "ach_games_10_desc",
                category = AchievementCategory.Beginner,
                targetValue = 10,
                rewardCoins = 100,
                rewardGems = 5,
                points = 15,
                iconName = "Logro_10_Partidas"
            });

            achievements.Add(new AchievementData
            {
                id = "games_50",
                titleKey = "ach_games_50",
                descriptionKey = "ach_games_50_desc",
                category = AchievementCategory.Progression,
                targetValue = 50,
                rewardCoins = 250,
                rewardGems = 15,
                points = 30,
                iconName = "Logro_50_Partidas"
            });

            achievements.Add(new AchievementData
            {
                id = "games_100",
                titleKey = "ach_games_100",
                descriptionKey = "ach_games_100_desc",
                category = AchievementCategory.Progression,
                targetValue = 100,
                rewardCoins = 500,
                rewardGems = 15,
                points = 50,
                iconName = "Logro_100_Partidas"
            });

            // ==================== VICTORIES - 5 logros ====================
            achievements.Add(new AchievementData
            {
                id = "wins_10",
                titleKey = "ach_wins_10",
                descriptionKey = "ach_wins_10_desc",
                category = AchievementCategory.Victories,
                targetValue = 10,
                rewardCoins = 100,
                rewardGems = 5,
                points = 20,
                iconName = "Logro_10_Victorias"
            });

            achievements.Add(new AchievementData
            {
                id = "wins_50",
                titleKey = "ach_wins_50",
                descriptionKey = "ach_wins_50_desc",
                category = AchievementCategory.Victories,
                targetValue = 50,
                rewardCoins = 250,
                rewardGems = 15,
                points = 40,
                iconName = "Logro_50_Victorias"
            });

            achievements.Add(new AchievementData
            {
                id = "wins_100",
                titleKey = "ach_wins_100",
                descriptionKey = "ach_wins_100_desc",
                category = AchievementCategory.Victories,
                targetValue = 100,
                rewardCoins = 500,
                rewardGems = 15,
                points = 60,
                iconName = "Logro_Centurion"
            });

            achievements.Add(new AchievementData
            {
                id = "wins_500",
                titleKey = "ach_wins_500",
                descriptionKey = "ach_wins_500_desc",
                category = AchievementCategory.Victories,
                targetValue = 500,
                rewardCoins = 1000,
                rewardGems = 50,
                points = 100,
                iconName = "Logro_500_Victorias"
            });

            achievements.Add(new AchievementData
            {
                id = "wins_1000",
                titleKey = "ach_wins_1000",
                descriptionKey = "ach_wins_1000_desc",
                category = AchievementCategory.Victories,
                targetValue = 1000,
                rewardCoins = 2000,
                rewardGems = 75,
                points = 200,
                iconName = "Logro_1000_Victorias"
            });

            // ==================== STREAKS - 4 logros ====================
            achievements.Add(new AchievementData
            {
                id = "streak_3",
                titleKey = "ach_streak_3",
                descriptionKey = "ach_streak_3_desc",
                category = AchievementCategory.Streaks,
                targetValue = 3,
                rewardCoins = 150,
                rewardGems = 5,
                points = 25,
                iconName = "Logro_Racha_Fuego"
            });

            achievements.Add(new AchievementData
            {
                id = "streak_5",
                titleKey = "ach_streak_5",
                descriptionKey = "ach_streak_5_desc",
                category = AchievementCategory.Streaks,
                targetValue = 5,
                rewardCoins = 250,
                rewardGems = 15,
                points = 40,
                iconName = "Logro_Victoria_Racha_7"
            });

            achievements.Add(new AchievementData
            {
                id = "streak_10",
                titleKey = "ach_streak_10",
                descriptionKey = "ach_streak_10_desc",
                category = AchievementCategory.Streaks,
                targetValue = 10,
                rewardCoins = 500,
                rewardGems = 50,
                points = 75,
                iconName = "Logro_Demoledor"
            });

            achievements.Add(new AchievementData
            {
                id = "streak_20",
                titleKey = "ach_streak_20",
                descriptionKey = "ach_streak_20_desc",
                category = AchievementCategory.Streaks,
                targetValue = 20,
                rewardCoins = 1000,
                rewardGems = 50,
                points = 150,
                iconName = "Logro_Victoria_Racha_30",
                isHidden = true
            });

            // ==================== CASH BATTLE - 7 logros ====================
            achievements.Add(new AchievementData
            {
                id = "cash_first",
                titleKey = "ach_cash_first",
                descriptionKey = "ach_cash_first_desc",
                category = AchievementCategory.CashBattle,
                targetValue = 1,
                rewardCoins = 100,
                rewardGems = 5,
                points = 25,
                iconName = "Logro_Ficha_Cash"
            });

            achievements.Add(new AchievementData
            {
                id = "cash_first_win",
                titleKey = "ach_cash_first_win",
                descriptionKey = "ach_cash_first_win_desc",
                category = AchievementCategory.CashBattle,
                targetValue = 1,
                rewardCoins = 150,
                rewardGems = 15,
                points = 35,
                iconName = "Logro_Rey_Monedas"
            });

            achievements.Add(new AchievementData
            {
                id = "cash_10_wins",
                titleKey = "ach_cash_10_wins",
                descriptionKey = "ach_cash_10_wins_desc",
                category = AchievementCategory.CashBattle,
                targetValue = 10,
                rewardCoins = 300,
                rewardGems = 15,
                points = 50,
                iconName = "Logro_VIP_1000"
            });

            achievements.Add(new AchievementData
            {
                id = "cash_50_wins",
                titleKey = "ach_cash_50_wins",
                descriptionKey = "ach_cash_50_wins_desc",
                category = AchievementCategory.CashBattle,
                targetValue = 50,
                rewardCoins = 750,
                rewardGems = 50,
                points = 100,
                iconName = "Logro_VIP_Dados"
            });

            achievements.Add(new AchievementData
            {
                id = "cash_100_wins",
                titleKey = "ach_cash_100_wins",
                descriptionKey = "ach_cash_100_wins_desc",
                category = AchievementCategory.CashBattle,
                targetValue = 100,
                rewardCoins = 1500,
                rewardGems = 75,
                points = 200,
                iconName = "Logro_Tiburon_Cash"
            });

            achievements.Add(new AchievementData
            {
                id = "cash_earnings_100",
                titleKey = "ach_cash_earnings_100",
                descriptionKey = "ach_cash_earnings_100_desc",
                category = AchievementCategory.CashBattle,
                targetValue = 100,
                rewardCoins = 500,
                rewardGems = 50,
                points = 75,
                iconName = "Logro_Bolsa_100"
            });

            achievements.Add(new AchievementData
            {
                id = "cash_earnings_1000",
                titleKey = "ach_cash_earnings_1000",
                descriptionKey = "ach_cash_earnings_1000_desc",
                category = AchievementCategory.CashBattle,
                targetValue = 1000,
                rewardCoins = 2000,
                rewardGems = 75,
                points = 250,
                iconName = "Logro_Millonario",
                isHidden = true
            });

            // ==================== TOURNAMENTS - 5 logros ====================
            achievements.Add(new AchievementData
            {
                id = "tournament_first",
                titleKey = "ach_tournament_first",
                descriptionKey = "ach_tournament_first_desc",
                category = AchievementCategory.Tournaments,
                targetValue = 1,
                rewardCoins = 100,
                rewardGems = 5,
                points = 20,
                iconName = "Logro_Torneo_Bracket"
            });

            achievements.Add(new AchievementData
            {
                id = "tournament_top3",
                titleKey = "ach_tournament_top3",
                descriptionKey = "ach_tournament_top3_desc",
                category = AchievementCategory.Tournaments,
                targetValue = 1,
                rewardCoins = 300,
                rewardGems = 15,
                points = 50,
                iconName = "Logro_Coleccion_Trofeos"
            });

            achievements.Add(new AchievementData
            {
                id = "tournament_win",
                titleKey = "ach_tournament_win",
                descriptionKey = "ach_tournament_win_desc",
                category = AchievementCategory.Tournaments,
                targetValue = 1,
                rewardCoins = 500,
                rewardGems = 50,
                points = 100,
                iconName = "Logro_Campeon_1"
            });

            achievements.Add(new AchievementData
            {
                id = "tournament_5_wins",
                titleKey = "ach_tournament_5_wins",
                descriptionKey = "ach_tournament_5_wins_desc",
                category = AchievementCategory.Tournaments,
                targetValue = 5,
                rewardCoins = 1000,
                rewardGems = 75,
                points = 200,
                iconName = "Logro_4_Estrellas"
            });

            achievements.Add(new AchievementData
            {
                id = "tournament_create",
                titleKey = "ach_tournament_create",
                descriptionKey = "ach_tournament_create_desc",
                category = AchievementCategory.Tournaments,
                targetValue = 1,
                rewardCoins = 150,
                rewardGems = 15,
                points = 30,
                iconName = "Logro_Organizador_Torneo"
            });

            // ==================== SOCIAL - 5 logros ====================
            achievements.Add(new AchievementData
            {
                id = "friend_first",
                titleKey = "ach_friend_first",
                descriptionKey = "ach_friend_first_desc",
                category = AchievementCategory.Social,
                targetValue = 1,
                rewardCoins = 75,
                rewardGems = 5,
                points = 15,
                iconName = "Logro_Primer_Rival"
            });

            achievements.Add(new AchievementData
            {
                id = "friends_10",
                titleKey = "ach_friends_10",
                descriptionKey = "ach_friends_10_desc",
                category = AchievementCategory.Social,
                targetValue = 10,
                rewardCoins = 200,
                rewardGems = 15,
                points = 30,
                iconName = "Logro_Social_10_Amigos"
            });

            achievements.Add(new AchievementData
            {
                id = "friends_50",
                titleKey = "ach_friends_50",
                descriptionKey = "ach_friends_50_desc",
                category = AchievementCategory.Social,
                targetValue = 50,
                rewardCoins = 500,
                rewardGems = 50,
                points = 75,
                iconName = "Logro_Influencer"
            });

            achievements.Add(new AchievementData
            {
                id = "challenge_friend",
                titleKey = "ach_challenge_friend",
                descriptionKey = "ach_challenge_friend_desc",
                category = AchievementCategory.Social,
                targetValue = 1,
                rewardCoins = 100,
                rewardGems = 5,
                points = 20,
                iconName = "Logro_Versus"
            });

            achievements.Add(new AchievementData
            {
                id = "beat_friend",
                titleKey = "ach_beat_friend",
                descriptionKey = "ach_beat_friend_desc",
                category = AchievementCategory.Social,
                targetValue = 1,
                rewardCoins = 125,
                rewardGems = 5,
                points = 25,
                iconName = "Logro_Amigo_Rival"
            });

            // ==================== PROGRESSION - 4 logros ====================
            achievements.Add(new AchievementData
            {
                id = "level_10",
                titleKey = "ach_level_10",
                descriptionKey = "ach_level_10_desc",
                category = AchievementCategory.Progression,
                targetValue = 10,
                rewardCoins = 150,
                rewardGems = 5,
                points = 25,
                iconName = "Logro_Nivel_10"
            });

            achievements.Add(new AchievementData
            {
                id = "level_25",
                titleKey = "ach_level_25",
                descriptionKey = "ach_level_25_desc",
                category = AchievementCategory.Progression,
                targetValue = 25,
                rewardCoins = 300,
                rewardGems = 15,
                points = 50,
                iconName = "Logro_Nivel_25"
            });

            achievements.Add(new AchievementData
            {
                id = "level_50",
                titleKey = "ach_level_50",
                descriptionKey = "ach_level_50_desc",
                category = AchievementCategory.Progression,
                targetValue = 50,
                rewardCoins = 500,
                rewardGems = 50,
                points = 75,
                iconName = "Logro_Nivel_50"
            });

            achievements.Add(new AchievementData
            {
                id = "level_100",
                titleKey = "ach_level_100",
                descriptionKey = "ach_level_100_desc",
                category = AchievementCategory.Progression,
                targetValue = 100,
                rewardCoins = 1000,
                rewardGems = 50,
                points = 150,
                iconName = "Logro_Avance_Epico"
            });

            // ==================== TIME - 6 logros ====================
            achievements.Add(new AchievementData
            {
                id = "days_7",
                titleKey = "ach_days_7",
                descriptionKey = "ach_days_7_desc",
                category = AchievementCategory.Time,
                targetValue = 7,
                rewardCoins = 150,
                rewardGems = 5,
                points = 25,
                iconName = "Logro_Racha_7_Dias"
            });

            achievements.Add(new AchievementData
            {
                id = "days_30",
                titleKey = "ach_days_30",
                descriptionKey = "ach_days_30_desc",
                category = AchievementCategory.Time,
                targetValue = 30,
                rewardCoins = 300,
                rewardGems = 15,
                points = 50,
                iconName = "Logro_Racha_30_Dias"
            });

            achievements.Add(new AchievementData
            {
                id = "days_100",
                titleKey = "ach_days_100",
                descriptionKey = "ach_days_100_desc",
                category = AchievementCategory.Time,
                targetValue = 100,
                rewardCoins = 500,
                rewardGems = 50,
                points = 100,
                iconName = "Logro_Racha_100_Dias"
            });

            achievements.Add(new AchievementData
            {
                id = "days_365",
                titleKey = "ach_days_365",
                descriptionKey = "ach_days_365_desc",
                category = AchievementCategory.Time,
                targetValue = 365,
                rewardCoins = 2000,
                rewardGems = 75,
                points = 300,
                iconName = "Logro_Racha_365_Dias",
                isHidden = true
            });

            achievements.Add(new AchievementData
            {
                id = "daily_streak_7",
                titleKey = "ach_daily_streak_7",
                descriptionKey = "ach_daily_streak_7_desc",
                category = AchievementCategory.Time,
                targetValue = 7,
                rewardCoins = 200,
                rewardGems = 15,
                points = 30,
                iconName = "Logro_Login_Semanal"
            });

            achievements.Add(new AchievementData
            {
                id = "daily_streak_30",
                titleKey = "ach_daily_streak_30",
                descriptionKey = "ach_daily_streak_30_desc",
                category = AchievementCategory.Time,
                targetValue = 30,
                rewardCoins = 500,
                rewardGems = 50,
                points = 75,
                iconName = "Logro_Login_Mensual"
            });

            // ==================== SECRET - 4 logros ====================
            achievements.Add(new AchievementData
            {
                id = "night_owl",
                titleKey = "ach_night_owl",
                descriptionKey = "ach_night_owl_desc",
                category = AchievementCategory.Secret,
                targetValue = 1,
                rewardCoins = 250,
                rewardGems = 100,
                points = 50,
                iconName = "Logro_Buho_Nocturno",
                isHidden = true
            });

            achievements.Add(new AchievementData
            {
                id = "perfect_game",
                titleKey = "ach_perfect_game",
                descriptionKey = "ach_perfect_game_desc",
                category = AchievementCategory.Secret,
                targetValue = 1,
                rewardCoins = 500,
                rewardGems = 100,
                points = 100,
                iconName = "Logro_Perfeccionista",
                isHidden = true
            });

            achievements.Add(new AchievementData
            {
                id = "comeback_king",
                titleKey = "ach_comeback_king",
                descriptionKey = "ach_comeback_king_desc",
                category = AchievementCategory.Secret,
                targetValue = 1,
                rewardCoins = 400,
                rewardGems = 100,
                points = 75,
                iconName = "Logro_Ave_Fenix",
                isHidden = true
            });

            achievements.Add(new AchievementData
            {
                id = "speed_demon",
                titleKey = "ach_speed_demon",
                descriptionKey = "ach_speed_demon_desc",
                category = AchievementCategory.Secret,
                targetValue = 1,
                rewardCoins = 500,
                rewardGems = 100,
                points = 100,
                iconName = "Logro_Demonio_Velocidad",
                isHidden = true
            });

            // ==================== LEGACY compatibility (mapped to new categories) ====================
            // These were in the old service but are kept as references
            // first_game -> Beginner (already added)
            // first_win -> Beginner (already added)
            // games_10, games_50, games_100 -> Beginner/Progression (already added)
            // wins_10, wins_50 -> Victories (already added)
            // streak_3 -> Streaks (already added)
            // friend_first replaces first_friend -> Social (already added)
            // daily_7/daily_30 are replaced by days_7/days_30 and daily_streak_7/daily_streak_30

            // Legacy records - map sub_5_seconds / sub_3_seconds to speed_demon (secret)
            // These are no longer separate achievements

            // Legacy friends_5 is now friends_10 (threshold increased)
            // Legacy streak_7 is now streak_5/streak_10 (split)

            // Pre-load icon sprites from Resources
            foreach (var achievement in achievements)
            {
                if (achievement.icon == null && !string.IsNullOrEmpty(achievement.iconName))
                {
                    achievement.icon = Resources.Load<Sprite>($"Icons/Achievements/{achievement.iconName}");
                }
            }
        }

        /// <summary>
        /// One-time migration from old Manager PlayerPrefs to Service JSON
        /// </summary>
        private void MigrateFromManagerPrefs()
        {
            if (PlayerPrefs.GetInt(MIGRATION_KEY, 0) == 1) return;

            Debug.Log("[Achievements] Migrando datos del Manager antiguo...");
            int migrated = 0;

            foreach (var achievement in achievements)
            {
                string progressKey = $"Achievement_{achievement.id}_progress";
                string completedKey = $"Achievement_{achievement.id}_completed";
                string claimedKey = $"Achievement_{achievement.id}_claimed";

                int oldProgress = PlayerPrefs.GetInt(progressKey, 0);
                bool oldCompleted = PlayerPrefs.GetInt(completedKey, 0) == 1;
                bool oldClaimed = PlayerPrefs.GetInt(claimedKey, 0) == 1;

                if (oldProgress > 0 || oldCompleted)
                {
                    if (!_progress.TryGetValue(achievement.id, out var existing))
                    {
                        existing = new AchievementProgress { id = achievement.id };
                        _progress[achievement.id] = existing;
                    }

                    // Take the max progress
                    if (oldProgress > existing.currentValue)
                        existing.currentValue = oldProgress;

                    if (oldCompleted && !existing.unlocked)
                    {
                        existing.unlocked = true;
                        existing.unlockedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        existing.currentValue = achievement.targetValue;
                    }

                    if (oldClaimed)
                        existing.claimed = true;

                    migrated++;
                }
            }

            // Also migrate legacy keys that had different IDs
            MigrateLegacyKey("first_friend", "friend_first");
            MigrateLegacyKey("friends_5", "friends_10");
            MigrateLegacyKey("streak_7", "streak_5");
            MigrateLegacyKey("daily_7", "days_7");
            MigrateLegacyKey("daily_30", "days_30");

            if (migrated > 0)
            {
                SaveProgress();
                Debug.Log($"[Achievements] Migrados {migrated} logros del sistema antiguo");
            }

            PlayerPrefs.SetInt(MIGRATION_KEY, 1);
            PlayerPrefs.Save();
        }

        private void MigrateLegacyKey(string oldId, string newId)
        {
            int oldProgress = PlayerPrefs.GetInt($"Achievement_{oldId}_progress", 0);
            bool oldCompleted = PlayerPrefs.GetInt($"Achievement_{oldId}_completed", 0) == 1;
            bool oldClaimed = PlayerPrefs.GetInt($"Achievement_{oldId}_claimed", 0) == 1;

            if (oldProgress <= 0 && !oldCompleted) return;

            // Also check the Service JSON format
            if (_progress.TryGetValue(oldId, out var oldData))
            {
                oldProgress = Mathf.Max(oldProgress, oldData.currentValue);
                oldCompleted = oldCompleted || oldData.unlocked;
                oldClaimed = oldClaimed || oldData.claimed;
            }

            if (!_progress.TryGetValue(newId, out var newData))
            {
                newData = new AchievementProgress { id = newId };
                _progress[newId] = newData;
            }

            if (oldProgress > newData.currentValue)
                newData.currentValue = oldProgress;

            if (oldCompleted && !newData.unlocked)
            {
                newData.unlocked = true;
                newData.unlockedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }

            if (oldClaimed)
                newData.claimed = true;
        }

        #endregion

        #region Progress Management

        private void LoadProgress()
        {
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
        /// Verifica si un logro esta desbloqueado
        /// </summary>
        public bool IsUnlocked(string achievementId)
        {
            return _progress.TryGetValue(achievementId, out var progress) && progress.unlocked;
        }

        /// <summary>
        /// Verifica si la recompensa de un logro fue reclamada
        /// </summary>
        public bool IsClaimed(string achievementId)
        {
            return _progress.TryGetValue(achievementId, out var progress) && progress.claimed;
        }

        /// <summary>
        /// Reclama la recompensa de un logro completado
        /// </summary>
        public bool ClaimReward(string achievementId)
        {
            if (!IsUnlocked(achievementId)) return false;
            if (IsClaimed(achievementId)) return false;

            if (_progress.TryGetValue(achievementId, out var progress))
            {
                progress.claimed = true;

                var achievement = achievements.Find(a => a.id == achievementId);
                if (achievement != null)
                {
                    GiveReward(achievement);
                }

                SaveProgress();
                Debug.Log($"[Achievements] Reward claimed: {achievementId}");
                return true;
            }
            return false;
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
        /// Establece el progreso de un logro a un valor especifico
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

            SaveProgress();

            // Analytics
            AnalyticsService.Instance?.LogMissionCompleted(
                achievementId,
                "achievement",
                achievement.rewardCoins
            );

            Debug.Log($"[Achievements] Desbloqueado: {achievementId}");

            OnAchievementUnlocked?.Invoke(achievement);

            // Intentar mostrar review prompt
            ReviewService.Instance?.TryRequestReview(justLost: false);
        }

        private void GiveReward(AchievementData achievement)
        {
            var currency = DigitPark.Monetization.CurrencyManager.Instance;
            if (currency != null)
            {
                if (achievement.rewardCoins > 0)
                {
                    currency.AddCoins(achievement.rewardCoins);
                    AnalyticsService.Instance?.LogVirtualCurrencyEarned("digitcoins", achievement.rewardCoins, "achievement");
                }
                if (achievement.rewardGems > 0)
                {
                    currency.AddGems(achievement.rewardGems);
                    AnalyticsService.Instance?.LogVirtualCurrencyEarned("digitgems", achievement.rewardGems, "achievement");
                }
                Debug.Log($"[Achievements] Reward: +{achievement.rewardCoins} DigitCoins, +{achievement.rewardGems} DigitGems for {achievement.id}");
            }
        }

        /// <summary>
        /// Obtiene logros por categoria
        /// </summary>
        public List<AchievementData> GetByCategory(AchievementCategory category)
        {
            return achievements.FindAll(a => a.category == category);
        }

        /// <summary>
        /// Total de logros completados
        /// </summary>
        public int GetCompletedCount()
        {
            int total = 0;
            foreach (var achievement in achievements)
            {
                if (IsUnlocked(achievement.id))
                    total++;
            }
            return total;
        }

        #endregion

        #region Game Event Handlers

        /// <summary>
        /// Llamar cuando se completa un juego
        /// </summary>
        public void OnGameCompleted(bool won, float time, string gameType = null, int score = 0, int accuracy = 0, bool wasBehind = false)
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
                AddProgress("wins_100", 1);
                AddProgress("wins_500", 1);
                AddProgress("wins_1000", 1);
            }

            // Mastery por juego
            if (!string.IsNullOrEmpty(gameType))
            {
                switch (gameType.ToLower())
                {
                    case "digitrush":
                        if (score > 0) SetProgress("digitrush_master", Mathf.Max(GetCurrentValue("digitrush_master"), score));
                        break;
                    case "flashtap":
                        if (accuracy > 0) AddProgress("flashtap_master", accuracy);
                        break;
                    case "memorypairs":
                        if (won && accuracy >= 100) UnlockAchievement("memorypairs_master");
                        break;
                    case "quickmath":
                        if (score > 0) SetProgress("quickmath_master", Mathf.Max(GetCurrentValue("quickmath_master"), score));
                        break;
                    case "oddoneout":
                        if (score > 0) AddProgress("oddoneout_master", score);
                        break;
                }
            }

            // Logros secretos especiales
            CheckSpecialAchievements(won, time, accuracy, wasBehind);
        }

        /// <summary>
        /// Llamar cuando se completa una Cash Battle
        /// </summary>
        public void OnCashBattleCompleted(bool won, decimal earnings)
        {
            // Primera cash battle
            AddProgress("cash_first", 1);

            if (won)
            {
                AddProgress("cash_first_win", 1);
                AddProgress("cash_10_wins", 1);
                AddProgress("cash_50_wins", 1);
                AddProgress("cash_100_wins", 1);
            }

            // Ganancias acumuladas
            if (earnings > 0)
            {
                int currentEarnings100 = GetCurrentValue("cash_earnings_100");
                SetProgress("cash_earnings_100", currentEarnings100 + (int)earnings);

                int currentEarnings1000 = GetCurrentValue("cash_earnings_1000");
                SetProgress("cash_earnings_1000", currentEarnings1000 + (int)earnings);
            }
        }

        /// <summary>
        /// Llamar para eventos de torneo
        /// </summary>
        public void OnTournamentEvent(string eventType, int placement = 0)
        {
            switch (eventType)
            {
                case "join":
                    AddProgress("tournament_first", 1);
                    break;
                case "finish":
                    if (placement > 0 && placement <= 3)
                        UnlockAchievement("tournament_top3");
                    if (placement == 1)
                    {
                        UnlockAchievement("tournament_win");
                        AddProgress("tournament_5_wins", 1);
                    }
                    break;
                case "create":
                    UnlockAchievement("tournament_create");
                    break;
            }
        }

        /// <summary>
        /// Llamar cuando el nivel del jugador cambia
        /// </summary>
        public void OnPlayerLevelChanged(int newLevel)
        {
            SetProgress("level_10", newLevel);
            SetProgress("level_25", newLevel);
            SetProgress("level_50", newLevel);
            SetProgress("level_100", newLevel);
        }

        /// <summary>
        /// Llamar cuando el jugador hace login diario
        /// </summary>
        public void OnDailyLogin(int totalDays, int consecutiveDays)
        {
            SetProgress("days_7", totalDays);
            SetProgress("days_30", totalDays);
            SetProgress("days_100", totalDays);
            SetProgress("days_365", totalDays);

            SetProgress("daily_streak_7", consecutiveDays);
            SetProgress("daily_streak_30", consecutiveDays);
        }

        /// <summary>
        /// Llamar cuando cambia la racha de victorias
        /// </summary>
        public void OnWinStreakChanged(int currentStreak)
        {
            SetProgress("streak_3", currentStreak);
            SetProgress("streak_5", currentStreak);
            SetProgress("streak_10", currentStreak);
            SetProgress("streak_20", currentStreak);
        }

        /// <summary>
        /// Llamar cuando se agrega un amigo
        /// </summary>
        public void OnFriendAdded(int totalFriends)
        {
            AddProgress("friend_first", 1);
            SetProgress("friends_10", totalFriends);
            SetProgress("friends_50", totalFriends);
        }

        /// <summary>
        /// Llamar cuando se reta o vence a un amigo
        /// </summary>
        public void OnFriendChallenge(string type)
        {
            switch (type)
            {
                case "challenge":
                    UnlockAchievement("challenge_friend");
                    break;
                case "beat":
                    UnlockAchievement("beat_friend");
                    break;
            }
        }

        /// <summary>
        /// Llamar cuando se reclama recompensa diaria
        /// </summary>
        public void OnDailyRewardClaimed(int consecutiveDays)
        {
            OnDailyLogin(
                PlayerPrefs.GetInt("TotalDaysPlayed", consecutiveDays),
                consecutiveDays
            );
        }

        /// <summary>
        /// Verifica logros especiales/secretos post-juego
        /// </summary>
        private void CheckSpecialAchievements(bool won, float time, int accuracy, bool wasBehind)
        {
            // Night Owl - jugar a las 3 AM
            int hour = DateTime.Now.Hour;
            if (hour >= 3 && hour < 4)
            {
                UnlockAchievement("night_owl");
            }

            // Perfect Game - 100% accuracy
            if (won && accuracy >= 100)
            {
                UnlockAchievement("perfect_game");
            }

            // Comeback King - ganar estando detras por 50%+
            if (won && wasBehind)
            {
                UnlockAchievement("comeback_king");
            }

            // Speed Demon - completar en menos de 10 segundos
            if (won && time < 10f)
            {
                UnlockAchievement("speed_demon");
            }
        }

        #endregion

        #region Debug

        [ContextMenu("Debug: Reset All Achievements")]
        public void ResetAllAchievements()
        {
            _progress.Clear();
            PlayerPrefs.DeleteKey(ACHIEVEMENTS_KEY);
            PlayerPrefs.DeleteKey(MIGRATION_KEY);
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
        // New unified categories (match Manager's 11 categories)
        Beginner,       // Onboarding
        Mastery,        // Per-game mastery
        Victories,      // Win-based
        Streaks,        // Win streaks
        CashBattle,     // Cash battle
        Tournaments,    // Tournament
        Social,         // Friends
        Progression,    // Level/rank
        Collector,      // Reserved for V2
        Time,           // Login/dedication
        Secret,         // Hidden

        // Legacy aliases (kept for backward compatibility, map to new)
        Games = Beginner,
        Wins = Victories,
        Streak = Streaks,
        Records = Secret,
        Daily = Time,
        Special = Secret
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
        public int rewardGems = 0;
        public int points = 0;
        public string iconName;
        public Sprite icon;
        public bool isHidden = false;
    }

    [Serializable]
    public class AchievementProgress
    {
        public string id;
        public int currentValue = 0;
        public bool unlocked = false;
        public bool claimed = false;
        public string unlockedDate = "";
    }

    [Serializable]
    public class AchievementSaveData
    {
        public List<AchievementProgress> items = new List<AchievementProgress>();
    }

    #endregion
}
