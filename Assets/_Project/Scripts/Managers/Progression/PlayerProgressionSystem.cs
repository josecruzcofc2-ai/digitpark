using UnityEngine;
using System;
using System.Collections.Generic;

namespace DigitPark.Progression
{
    /// <summary>
    /// Sistema de Nivel Permanente del Jugador.
    /// - Solo sube, NUNCA baja
    /// - XP ganado en: Practice Mode, Torneos gratis, 1v1 gratis
    /// - NO incluye Cash Battles
    /// - Desbloquea: Avatares, Títulos, Cosméticos
    /// </summary>
    public class PlayerProgressionSystem : MonoBehaviour
    {
        public static PlayerProgressionSystem Instance { get; private set; }

        [Header("Level Configuration")]
        [SerializeField] private int maxLevel = 500;
        [SerializeField] private int baseXPPerLevel = 100;
        [SerializeField] private float xpScalingFactor = 1.15f; // Each level needs 15% more XP

        [Header("XP Rewards by Activity")]
        [SerializeField] private int xpPerGamePlayed = 25;
        [SerializeField] private int xpPerWin = 50;
        [SerializeField] private int xpPerPerfectGame = 100;
        [SerializeField] private int xpTournamentParticipation = 75;
        [SerializeField] private int xpTournamentTop3 = 200;
        [SerializeField] private int xpTournamentWin = 500;

        // Player Data
        private int _currentLevel = 1;
        private int _currentXP = 0;
        private int _totalXPEarned = 0;
        private int _gamesPlayed = 0;
        private int _gamesWon = 0;

        // Events
        public event Action<int, int> OnXPGained; // (xpGained, totalXP)
        public event Action<int> OnLevelUp; // (newLevel)
        public event Action<LevelReward> OnRewardUnlocked; // (reward)

        // Level rewards cache
        private Dictionary<int, LevelReward> _levelRewards;

        private const string SAVE_KEY = "PlayerProgression";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeLevelRewards();
                LoadProgress();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #region XP Calculation

        /// <summary>
        /// Calculate XP required for a specific level
        /// Formula: baseXP * (scalingFactor ^ (level - 1))
        /// Level 1: 100 XP
        /// Level 10: ~350 XP
        /// Level 50: ~3,500 XP
        /// Level 100: ~45,000 XP
        /// </summary>
        public int GetXPRequiredForLevel(int level)
        {
            if (level <= 1) return 0;
            return Mathf.RoundToInt(baseXPPerLevel * Mathf.Pow(xpScalingFactor, level - 1));
        }

        /// <summary>
        /// Get total XP required to reach a level from level 1
        /// </summary>
        public int GetTotalXPForLevel(int level)
        {
            int total = 0;
            for (int i = 1; i <= level; i++)
            {
                total += GetXPRequiredForLevel(i);
            }
            return total;
        }

        /// <summary>
        /// Get XP progress within current level (0.0 to 1.0)
        /// </summary>
        public float GetLevelProgress()
        {
            int xpForCurrentLevel = GetXPRequiredForLevel(_currentLevel);
            if (xpForCurrentLevel <= 0) return 1f;
            return (float)_currentXP / xpForCurrentLevel;
        }

        #endregion

        #region XP Gaining

        /// <summary>
        /// Add XP from completing a game (Practice/Free modes only)
        /// </summary>
        public void AddGameXP(GameResult result)
        {
            if (result.isCashBattle)
            {
                Debug.LogWarning("[PlayerProgression] Cash Battles don't give permanent XP.");
                return;
            }

            int xpGained = CalculateGameXP(result);
            AddXP(xpGained);

            _gamesPlayed++;
            if (result.isWin) _gamesWon++;

            SaveProgress();
        }

        /// <summary>
        /// Add XP from tournament participation
        /// </summary>
        public void AddTournamentXP(TournamentResult result)
        {
            if (result.isCashTournament)
            {
                Debug.LogWarning("[PlayerProgression] Cash Tournaments don't give permanent XP.");
                return;
            }

            int xpGained = xpTournamentParticipation;

            if (result.placement == 1)
                xpGained += xpTournamentWin;
            else if (result.placement <= 3)
                xpGained += xpTournamentTop3;

            AddXP(xpGained);
            SaveProgress();
        }

        /// <summary>
        /// Add raw XP (for missions, achievements, etc.)
        /// </summary>
        public void AddXP(int amount)
        {
            if (amount <= 0) return;
            if (_currentLevel >= maxLevel) return;

            _currentXP += amount;
            _totalXPEarned += amount;

            OnXPGained?.Invoke(amount, _totalXPEarned);

            // Check for level ups
            CheckLevelUp();
        }

        private int CalculateGameXP(GameResult result)
        {
            int xp = xpPerGamePlayed;

            if (result.isWin)
            {
                xp += xpPerWin;
            }

            if (result.isPerfect)
            {
                xp += xpPerPerfectGame;
            }

            // Bonus for high scores
            if (result.scorePercentile >= 90)
            {
                xp = Mathf.RoundToInt(xp * 1.25f);
            }
            else if (result.scorePercentile >= 75)
            {
                xp = Mathf.RoundToInt(xp * 1.1f);
            }

            return xp;
        }

        private void CheckLevelUp()
        {
            while (_currentLevel < maxLevel)
            {
                int xpNeeded = GetXPRequiredForLevel(_currentLevel);

                if (_currentXP >= xpNeeded)
                {
                    _currentXP -= xpNeeded;
                    _currentLevel++;

                    Debug.Log($"[PlayerProgression] LEVEL UP! Now level {_currentLevel}");
                    OnLevelUp?.Invoke(_currentLevel);

                    // Check for rewards
                    if (_levelRewards.TryGetValue(_currentLevel, out LevelReward reward))
                    {
                        OnRewardUnlocked?.Invoke(reward);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        #endregion

        #region Level Rewards

        private void InitializeLevelRewards()
        {
            _levelRewards = new Dictionary<int, LevelReward>
            {
                // Early levels - frequent rewards
                { 5, new LevelReward("Avatar: Principiante", RewardType.Avatar, "avatar_beginner") },
                { 10, new LevelReward("Título: Novato", RewardType.Title, "title_novice") },
                { 15, new LevelReward("500 Monedas", RewardType.Coins, "500") },
                { 20, new LevelReward("Avatar: Jugador", RewardType.Avatar, "avatar_player") },
                { 25, new LevelReward("Título: Jugador", RewardType.Title, "title_player") },

                // Mid levels
                { 30, new LevelReward("1000 Monedas", RewardType.Coins, "1000") },
                { 40, new LevelReward("Avatar: Veterano", RewardType.Avatar, "avatar_veteran") },
                { 50, new LevelReward("Título: Veterano", RewardType.Title, "title_veteran") },
                { 60, new LevelReward("Marco: Bronce", RewardType.Frame, "frame_bronze") },
                { 75, new LevelReward("2000 Monedas", RewardType.Coins, "2000") },

                // High levels
                { 100, new LevelReward("Título: Centurión", RewardType.Title, "title_centurion") },
                { 100, new LevelReward("Avatar: Centurión", RewardType.Avatar, "avatar_centurion") },
                { 125, new LevelReward("Marco: Plata", RewardType.Frame, "frame_silver") },
                { 150, new LevelReward("5000 Monedas", RewardType.Coins, "5000") },
                { 175, new LevelReward("Título: Experto", RewardType.Title, "title_expert") },
                { 200, new LevelReward("Avatar: Experto", RewardType.Avatar, "avatar_expert") },

                // Elite levels
                { 250, new LevelReward("Marco: Oro", RewardType.Frame, "frame_gold") },
                { 300, new LevelReward("Título: Maestro", RewardType.Title, "title_master") },
                { 350, new LevelReward("Avatar: Maestro", RewardType.Avatar, "avatar_master") },
                { 400, new LevelReward("Marco: Platino", RewardType.Frame, "frame_platinum") },
                { 450, new LevelReward("Título: Gran Maestro", RewardType.Title, "title_grandmaster") },

                // Max level
                { 500, new LevelReward("Título: Leyenda", RewardType.Title, "title_legend") },
                { 500, new LevelReward("Avatar: Leyenda", RewardType.Avatar, "avatar_legend") },
                { 500, new LevelReward("Marco: Diamante", RewardType.Frame, "frame_diamond") },
            };
        }

        public LevelReward GetRewardForLevel(int level)
        {
            return _levelRewards.TryGetValue(level, out LevelReward reward) ? reward : null;
        }

        public List<LevelReward> GetAllRewards()
        {
            return new List<LevelReward>(_levelRewards.Values);
        }

        #endregion

        #region Public Getters

        public int CurrentLevel => _currentLevel;
        public int CurrentXP => _currentXP;
        public int TotalXPEarned => _totalXPEarned;
        public int MaxLevel => maxLevel;
        public int GamesPlayed => _gamesPlayed;
        public int GamesWon => _gamesWon;
        public float WinRate => _gamesPlayed > 0 ? (float)_gamesWon / _gamesPlayed : 0f;

        public int XPToNextLevel => GetXPRequiredForLevel(_currentLevel) - _currentXP;
        public bool IsMaxLevel => _currentLevel >= maxLevel;

        /// <summary>
        /// Get display string for current level progress
        /// </summary>
        public string GetProgressString()
        {
            if (IsMaxLevel) return "MAX";
            return $"{_currentXP:N0} / {GetXPRequiredForLevel(_currentLevel):N0} XP";
        }

        #endregion

        #region Save/Load

        private void SaveProgress()
        {
            var data = new PlayerProgressionData
            {
                level = _currentLevel,
                currentXP = _currentXP,
                totalXPEarned = _totalXPEarned,
                gamesPlayed = _gamesPlayed,
                gamesWon = _gamesWon
            };

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        private void LoadProgress()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                var data = JsonUtility.FromJson<PlayerProgressionData>(json);

                _currentLevel = data.level;
                _currentXP = data.currentXP;
                _totalXPEarned = data.totalXPEarned;
                _gamesPlayed = data.gamesPlayed;
                _gamesWon = data.gamesWon;
            }
        }

        /// <summary>
        /// Reset all progress (for testing only)
        /// </summary>
        [ContextMenu("Reset Progress (DEBUG)")]
        public void ResetProgress()
        {
            _currentLevel = 1;
            _currentXP = 0;
            _totalXPEarned = 0;
            _gamesPlayed = 0;
            _gamesWon = 0;
            SaveProgress();
            Debug.Log("[PlayerProgression] Progress reset!");
        }

        #endregion
    }

    #region Data Structures

    [Serializable]
    public class PlayerProgressionData
    {
        public int level;
        public int currentXP;
        public int totalXPEarned;
        public int gamesPlayed;
        public int gamesWon;
    }

    [Serializable]
    public class GameResult
    {
        public string gameId;
        public bool isWin;
        public bool isPerfect;
        public int score;
        public float scorePercentile; // 0-100, how this score compares to others
        public bool isCashBattle; // If true, doesn't give permanent XP
    }

    [Serializable]
    public class TournamentResult
    {
        public string tournamentId;
        public int placement; // 1 = first, 2 = second, etc.
        public int totalParticipants;
        public bool isCashTournament; // If true, doesn't give permanent XP
    }

    [Serializable]
    public class LevelReward
    {
        public string name;
        public RewardType type;
        public string rewardId;

        public LevelReward(string name, RewardType type, string rewardId)
        {
            this.name = name;
            this.type = type;
            this.rewardId = rewardId;
        }
    }

    public enum RewardType
    {
        Avatar,
        Title,
        Frame,
        Coins,
        Gems,
        Cosmetic
    }

    #endregion
}
