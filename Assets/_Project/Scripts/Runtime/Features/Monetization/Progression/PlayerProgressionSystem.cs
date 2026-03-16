using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DigitPark.Services.Firebase;

namespace DigitPark.Progression
{
    /// <summary>
    /// Permanent Player Level System.
    /// - Only goes up, NEVER goes down
    /// - XP earned in: Practice Mode, Free Tournaments, Free 1v1
    /// - CashBattle gives XP at 50% rate (cosmetic-only, no gameplay advantage)
    /// - Unlocks: Avatars, Titles, Frames, Cosmetics
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
        [SerializeField] [Range(0f, 1f)] private float cashBattleXPMultiplier = 0.5f;

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
        /// Level 1: 100 XP | Level 10: ~350 XP | Level 50: ~3,500 XP | Level 100: ~45,000 XP
        /// </summary>
        /// <summary>
        /// Economy Rebalance V55: x1.15 for levels 1-50, x1.12 for 51+.
        /// This makes level 100 achievable in ~1.5 years (was 3.3 years with flat x1.15).
        /// </summary>
        public int GetXPRequiredForLevel(int level)
        {
            if (level <= 1) return 0;
            float exponent;
            if (level <= 50)
            {
                exponent = level - 1;
            }
            else
            {
                // First 49 levels at x1.15, then remaining at x1.12
                float xpAt50 = baseXPPerLevel * Mathf.Pow(xpScalingFactor, 49);
                float xpRequired = xpAt50 * Mathf.Pow(1.12f, level - 50);
                return Mathf.RoundToInt(Mathf.Min(xpRequired, 10000000f));
            }
            float result = baseXPPerLevel * Mathf.Pow(xpScalingFactor, exponent);
            return Mathf.RoundToInt(Mathf.Min(result, 10000000f));
        }

        /// <summary>Get total XP required to reach a level from level 1</summary>
        public int GetTotalXPForLevel(int level)
        {
            int total = 0;
            for (int i = 1; i <= level; i++)
                total += GetXPRequiredForLevel(i);
            return total;
        }

        /// <summary>Get XP progress within current level (0.0 to 1.0)</summary>
        public float GetLevelProgress()
        {
            int xpForCurrentLevel = GetXPRequiredForLevel(_currentLevel);
            if (xpForCurrentLevel <= 0) return 1f;
            return (float)_currentXP / xpForCurrentLevel;
        }

        #endregion

        #region XP Gaining

        /// <summary>
        /// Add XP from completing a game. Returns the XP actually awarded (after multipliers).
        /// CashBattle gives XP at reduced rate (cosmetic only — no gameplay advantage).
        /// </summary>
        public int AddGameXP(GameResult result)
        {
            int xpGained = CalculateGameXP(result);

            if (result.isCashBattle)
                xpGained = Mathf.RoundToInt(xpGained * cashBattleXPMultiplier);

            AddXP(xpGained);

            _gamesPlayed++;
            if (result.isWin) _gamesWon++;

            SaveProgress();
            return xpGained;
        }

        /// <summary>
        /// Add XP from tournament participation.
        /// CashBattle tournaments give XP at reduced rate.
        /// </summary>
        public void AddTournamentXP(TournamentResult result)
        {
            int xpGained = xpTournamentParticipation;

            if (result.placement == 1)
                xpGained += xpTournamentWin;
            else if (result.placement <= 3)
                xpGained += xpTournamentTop3;

            if (result.isCashTournament)
                xpGained = Mathf.RoundToInt(xpGained * cashBattleXPMultiplier);

            AddXP(xpGained);
            SaveProgress();
        }

        /// <summary>Add raw XP (for missions, achievements, etc.)</summary>
        public void AddXP(int amount)
        {
            if (amount <= 0) return;
            if (_currentLevel >= maxLevel) return;

            // Economy Rebalance V55 — XP boost x1.25 for 30-day streak (7 days duration)
            if (IsXPBoostActive())
                amount = Mathf.RoundToInt(amount * 1.25f);

            _currentXP += amount;
            _totalXPEarned += amount;

            OnXPGained?.Invoke(amount, _totalXPEarned);

            CheckLevelUp();
        }

        /// <summary>
        /// Economy Rebalance V55: Check if XP boost is active.
        /// Sources: 30-day streak milestone (x1.25 for 7 days) OR Daily Offer XP boost.
        /// </summary>
        private bool IsXPBoostActive()
        {
            string expiryStr = PlayerPrefs.GetString("XPBoost_Expiry", "");
            if (string.IsNullOrEmpty(expiryStr)) return false;
            if (System.DateTime.TryParse(expiryStr, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind, out System.DateTime expiry))
            {
                return System.DateTime.UtcNow < expiry;
            }
            return false;
        }

        private int CalculateGameXP(GameResult result)
        {
            int xp = xpPerGamePlayed;

            if (result.isWin)
                xp += xpPerWin;

            if (result.isPerfect)
                xp += xpPerPerfectGame;

            if (result.scorePercentile >= 90)
                xp = Mathf.RoundToInt(xp * 1.25f);
            else if (result.scorePercentile >= 75)
                xp = Mathf.RoundToInt(xp * 1.1f);

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

                    if (_levelRewards.TryGetValue(_currentLevel, out LevelReward reward))
                    {
                        GrantLevelReward(reward);
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

        /// <summary>
        /// Grants currency rewards for level-up milestones
        /// </summary>
        private void GrantLevelReward(LevelReward reward)
        {
            var currency = DigitPark.Monetization.CurrencyManager.Instance;
            if (currency == null) return;

            // Grant coins if reward type is DigitCoins
            if (reward.type == RewardType.DigitCoins && int.TryParse(reward.rewardId, out int coinAmount))
            {
                currency.AddCoins(coinAmount);
                AnalyticsService.Instance?.LogVirtualCurrencyEarned("digitcoins", coinAmount, "level_reward");
            }

            // Economy Rebalance: grant bonus gems on milestone levels
            if (reward.bonusGems > 0)
            {
                currency.AddGems(reward.bonusGems);
                AnalyticsService.Instance?.LogVirtualCurrencyEarned("digitgems", reward.bonusGems, "level_reward_gems");
            }

            Debug.Log($"[PlayerProgression] Granted reward: {reward.name}" +
                      (reward.bonusGems > 0 ? $" + {reward.bonusGems} DG" : ""));
        }

        #region Level Rewards

        private void InitializeLevelRewards()
        {
            // Economy Rebalance: bonusGems added to milestone levels (~225 DG lifetime)
            _levelRewards = new Dictionary<int, LevelReward>
            {
                // Early levels
                // Economy Rebalance: DG es premium — drip muy pequeño en milestones (~67 DG lifetime total)
                { 5,   new LevelReward("Avatar: Beginner",      RewardType.Avatar,      "avatar_beginner",  3) },
                { 10,  new LevelReward("Title: Novice",         RewardType.Title,       "title_novice",     3) },
                { 15,  new LevelReward("500 DigitCoins",        RewardType.DigitCoins,  "500",              2) },
                { 20,  new LevelReward("Avatar: Player",        RewardType.Avatar,      "avatar_player") },
                { 25,  new LevelReward("Title: Player",         RewardType.Title,       "title_player",     5) },

                // Mid levels
                { 30,  new LevelReward("1000 DigitCoins",       RewardType.DigitCoins,  "1000",             3) },
                { 40,  new LevelReward("Avatar: Veteran",       RewardType.Avatar,      "avatar_veteran") },
                { 50,  new LevelReward("Title: Veteran",        RewardType.Title,       "title_veteran",    7) },
                { 60,  new LevelReward("Frame: Bronze",         RewardType.Frame,       "frame_bronze") },
                { 75,  new LevelReward("2000 DigitCoins",       RewardType.DigitCoins,  "2000",             8) },

                // High levels
                { 100, new LevelReward("Title: Centurion",      RewardType.Title,       "title_centurion",  10) },
                { 105, new LevelReward("Avatar: Centurion",     RewardType.Avatar,      "avatar_centurion") },
                { 125, new LevelReward("Frame: Silver",         RewardType.Frame,       "frame_silver") },
                { 150, new LevelReward("5000 DigitCoins",       RewardType.DigitCoins,  "5000",             8) },
                { 175, new LevelReward("Title: Expert",         RewardType.Title,       "title_expert") },
                { 200, new LevelReward("Avatar: Expert",        RewardType.Avatar,      "avatar_expert",    10) },

                // Elite levels
                { 250, new LevelReward("Frame: Gold",           RewardType.Frame,       "frame_gold") },
                { 300, new LevelReward("Title: Master",         RewardType.Title,       "title_master",     8) },
                { 350, new LevelReward("Avatar: Master",        RewardType.Avatar,      "avatar_master") },
                { 400, new LevelReward("Frame: Platinum",       RewardType.Frame,       "frame_platinum") },
                { 450, new LevelReward("Title: Grand Master",   RewardType.Title,       "title_grandmaster") },

                // Max level
                { 475, new LevelReward("Avatar: Legend",        RewardType.Avatar,      "avatar_legend") },
                { 490, new LevelReward("Frame: Diamond",        RewardType.Frame,       "frame_diamond") },
                { 500, new LevelReward("Title: Legend",         RewardType.Title,       "title_legend") },

                // Micro-rewards to fill gaps (~1,400 DC extra lifetime)
                { 65,  new LevelReward("50 DigitCoins",         RewardType.DigitCoins,  "50") },
                { 70,  new LevelReward("75 DigitCoins",         RewardType.DigitCoins,  "75") },
                { 85,  new LevelReward("100 DigitCoins",        RewardType.DigitCoins,  "100") },
                { 90,  new LevelReward("100 DigitCoins",        RewardType.DigitCoins,  "100") },
                { 95,  new LevelReward("100 DigitCoins",        RewardType.DigitCoins,  "100") },
                { 110, new LevelReward("100 DigitCoins",        RewardType.DigitCoins,  "100") },
                { 115, new LevelReward("100 DigitCoins",        RewardType.DigitCoins,  "100") },
                { 120, new LevelReward("150 DigitCoins",        RewardType.DigitCoins,  "150") },
                { 130, new LevelReward("150 DigitCoins",        RewardType.DigitCoins,  "150") },
                { 140, new LevelReward("150 DigitCoins",        RewardType.DigitCoins,  "150") },
                { 160, new LevelReward("150 DigitCoins",        RewardType.DigitCoins,  "150") },
                { 170, new LevelReward("200 DigitCoins",        RewardType.DigitCoins,  "200") },
                { 180, new LevelReward("200 DigitCoins",        RewardType.DigitCoins,  "200") },
                { 190, new LevelReward("200 DigitCoins",        RewardType.DigitCoins,  "200") },
            };
        }

        public LevelReward GetRewardForLevel(int level) =>
            _levelRewards.TryGetValue(level, out LevelReward reward) ? reward : null;

        public List<LevelReward> GetAllRewards() =>
            new List<LevelReward>(_levelRewards.Values);

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

        public string GetProgressString()
        {
            if (IsMaxLevel) return "MAX";
            return $"{_currentXP:N0} / {GetXPRequiredForLevel(_currentLevel):N0} XP";
        }

        /// <summary>
        /// Economy Rebalance V55 — Level badge tier.
        /// Bronze (1-25), Silver (26-75), Gold (76-200), Diamond (200+).
        /// Visible in profile and pre-match. Does NOT show numeric level.
        /// </summary>
        public string GetBadgeTier()
        {
            if (_currentLevel >= 200) return "DIAMOND";
            if (_currentLevel >= 76) return "GOLD";
            if (_currentLevel >= 26) return "SILVER";
            return "BRONZE";
        }

        public Color GetBadgeColor()
        {
            if (_currentLevel >= 200) return new Color(0.7f, 0.9f, 1f, 1f);   // Diamond
            if (_currentLevel >= 76) return new Color(1f, 0.84f, 0f, 1f);     // Gold
            if (_currentLevel >= 26) return new Color(0.75f, 0.75f, 0.82f, 1f); // Silver
            return new Color(0.8f, 0.5f, 0.2f, 1f);                            // Bronze
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

            SyncProgressionToFirebase();
        }

        private void SyncProgressionToFirebase()
        {
            var playerData = AuthenticationService.Instance?.GetCurrentPlayerData();
            if (playerData == null || DatabaseService.Instance == null) return;

            var updates = new Dictionary<string, object>
            {
                { "playerLevel", _currentLevel },
                { "playerXP", _currentXP },
                { "gamesPlayed", _gamesPlayed },
                { "gamesWon", _gamesWon }
            };

            DatabaseService.Instance.UpdatePlayerFields(playerData.userId, updates).ContinueWith(t =>
            {
                if (t.IsFaulted)
                    Debug.LogWarning($"[PlayerProgression] Firebase sync failed: {t.Exception?.GetBaseException().Message}");
            }, TaskContinuationOptions.OnlyOnFaulted);
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

        #if UNITY_EDITOR
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
        #endif

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
        public float scorePercentile; // 0-100
        public bool isCashBattle;
    }

    [Serializable]
    public class TournamentResult
    {
        public string tournamentId;
        public int placement;
        public int totalParticipants;
        public bool isCashTournament;
    }

    [Serializable]
    public class LevelReward
    {
        public string name;
        public RewardType type;
        public string rewardId;
        public int bonusGems; // Economy Rebalance: gem drip on level milestones

        public LevelReward(string name, RewardType type, string rewardId, int bonusGems = 0)
        {
            this.name = name;
            this.type = type;
            this.rewardId = rewardId;
            this.bonusGems = bonusGems;
        }
    }

    public enum RewardType
    {
        Avatar,
        Title,
        Frame,
        DigitCoins,
        DigitGems,
        Cosmetic
    }

    #endregion
}
