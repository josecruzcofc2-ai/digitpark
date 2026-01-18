using UnityEngine;
using System;
using System.Collections.Generic;

namespace DigitPark.Progression
{
    /// <summary>
    /// Sistema de Rangos para Cash Battles.
    /// - 10 Rangos visibles: Novato → Inmortal
    /// - MMR oculto para matchmaking justo
    /// - Soft Reset cada temporada (60 días)
    /// - SOLO para partidas con dinero real
    /// </summary>
    public class CashBattleRankSystem : MonoBehaviour
    {
        public static CashBattleRankSystem Instance { get; private set; }

        [Header("Season Configuration")]
        [SerializeField] private int seasonDurationDays = 60;

        [Header("MMR Configuration")]
        [SerializeField] private int startingMMR = 1000;
        [SerializeField] private int minMMR = 0;
        [SerializeField] private int maxMMR = 3000;
        [SerializeField] private int baseMMRChange = 25;
        [SerializeField] private int maxMMRDifferenceForMatch = 200;

        [Header("Matchmaking")]
        [SerializeField] private float matchmakingExpandRate = 50f; // Expand search by 50 MMR per 10 seconds
        [SerializeField] private float maxMatchmakingWaitTime = 120f; // 2 minutes max wait

        // Player Data
        private int _currentMMR;
        private CashBattleRank _currentRank;
        private int _seasonWins;
        private int _seasonLosses;
        private int _seasonHighestMMR;
        private int _totalCashBattles;
        private int _totalCashWins;
        private float _totalEarnings;
        private DateTime _seasonStartDate;
        private int _currentSeasonNumber;

        // Events
        public event Action<int, int> OnMMRChanged; // (oldMMR, newMMR)
        public event Action<CashBattleRank, CashBattleRank> OnRankChanged; // (oldRank, newRank)
        public event Action<int> OnSeasonReset; // (newSeasonNumber)
        public event Action<SeasonRewards> OnSeasonRewardsEarned;

        private const string SAVE_KEY = "CashBattleRank";

        // Rank definitions
        private static readonly RankDefinition[] RankDefinitions = new RankDefinition[]
        {
            new RankDefinition(CashBattleRank.Novato,      0,    499,  "Novato",      "rank_novato",      new Color(0.6f, 0.6f, 0.6f)),
            new RankDefinition(CashBattleRank.Bronce,     500,   799,  "Bronce",      "rank_bronce",      new Color(0.8f, 0.5f, 0.2f)),
            new RankDefinition(CashBattleRank.Plata,      800,  1099,  "Plata",       "rank_plata",       new Color(0.75f, 0.75f, 0.75f)),
            new RankDefinition(CashBattleRank.Oro,       1100,  1399,  "Oro",         "rank_oro",         new Color(1f, 0.84f, 0f)),
            new RankDefinition(CashBattleRank.Platino,   1400,  1699,  "Platino",     "rank_platino",     new Color(0.9f, 0.9f, 0.95f)),
            new RankDefinition(CashBattleRank.Diamante,  1700,  1999,  "Diamante",    "rank_diamante",    new Color(0.2f, 0.8f, 1f)),
            new RankDefinition(CashBattleRank.Maestro,   2000,  2299,  "Maestro",     "rank_maestro",     new Color(0.6f, 0.2f, 0.8f)),
            new RankDefinition(CashBattleRank.GranMaestro, 2300, 2599, "Gran Maestro", "rank_granmaestro", new Color(1f, 0.4f, 0.4f)),
            new RankDefinition(CashBattleRank.Leyenda,   2600,  2899,  "Leyenda",     "rank_leyenda",     new Color(1f, 0.6f, 0f)),
            new RankDefinition(CashBattleRank.Inmortal,  2900,  3000,  "Inmortal",    "rank_inmortal",    new Color(1f, 1f, 1f)),
        };

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadData();
                CheckSeasonReset();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Check season reset on start
            CheckSeasonReset();
        }

        #region MMR Calculations

        /// <summary>
        /// Calculate MMR change based on match result.
        /// Uses modified Elo system.
        /// </summary>
        public int CalculateMMRChange(int playerMMR, int opponentMMR, bool isWin)
        {
            // Expected score based on MMR difference
            float expectedScore = 1f / (1f + Mathf.Pow(10f, (opponentMMR - playerMMR) / 400f));

            // Actual score
            float actualScore = isWin ? 1f : 0f;

            // K-factor (how much MMR can change)
            // Higher K for lower ranks, lower K for higher ranks
            int kFactor = GetKFactor(playerMMR);

            // Calculate change
            int change = Mathf.RoundToInt(kFactor * (actualScore - expectedScore));

            // Minimum change of ±10 to avoid stagnation
            if (isWin && change < 10) change = 10;
            if (!isWin && change > -10) change = -10;

            return change;
        }

        private int GetKFactor(int mmr)
        {
            // More volatile at lower ranks, more stable at higher ranks
            if (mmr < 500) return 40;      // Novato
            if (mmr < 1000) return 35;     // Bronce
            if (mmr < 1500) return 30;     // Plata-Oro
            if (mmr < 2000) return 25;     // Platino-Diamante
            if (mmr < 2500) return 20;     // Maestro-GranMaestro
            return 15;                      // Leyenda-Inmortal
        }

        /// <summary>
        /// Process a Cash Battle result
        /// </summary>
        public void ProcessMatchResult(CashBattleResult result)
        {
            int oldMMR = _currentMMR;
            CashBattleRank oldRank = _currentRank;

            // Calculate MMR change
            int mmrChange = CalculateMMRChange(_currentMMR, result.opponentMMR, result.isWin);

            // Apply streak bonus/penalty
            if (result.isWin)
            {
                _seasonWins++;
                _totalCashWins++;

                // Win streak bonus (max 50% extra)
                int streak = GetCurrentWinStreak();
                if (streak >= 3)
                {
                    float streakBonus = Mathf.Min(0.5f, (streak - 2) * 0.1f);
                    mmrChange = Mathf.RoundToInt(mmrChange * (1f + streakBonus));
                }
            }
            else
            {
                _seasonLosses++;

                // Loss protection at rank floor (lose less MMR)
                if (IsAtRankFloor())
                {
                    mmrChange = Mathf.RoundToInt(mmrChange * 0.5f);
                }
            }

            // Apply MMR change
            _currentMMR = Mathf.Clamp(_currentMMR + mmrChange, minMMR, maxMMR);
            _totalCashBattles++;
            _totalEarnings += result.earnings;

            // Update highest MMR
            if (_currentMMR > _seasonHighestMMR)
            {
                _seasonHighestMMR = _currentMMR;
            }

            // Check rank change
            CashBattleRank newRank = GetRankForMMR(_currentMMR);
            if (newRank != _currentRank)
            {
                _currentRank = newRank;
                OnRankChanged?.Invoke(oldRank, newRank);
            }

            OnMMRChanged?.Invoke(oldMMR, _currentMMR);
            SaveData();

            Debug.Log($"[CashBattleRank] Match result: {(result.isWin ? "WIN" : "LOSS")} | " +
                     $"MMR: {oldMMR} → {_currentMMR} ({mmrChange:+0;-0}) | Rank: {_currentRank}");
        }

        private bool IsAtRankFloor()
        {
            var rankDef = GetRankDefinition(_currentRank);
            return _currentMMR <= rankDef.minMMR + 50;
        }

        private int GetCurrentWinStreak()
        {
            // This would need match history - simplified for now
            // In production, track last N matches
            return 0;
        }

        #endregion

        #region Matchmaking

        /// <summary>
        /// Check if two players can be matched based on MMR
        /// </summary>
        public bool CanMatch(int playerMMR, int opponentMMR, float waitTimeSeconds)
        {
            // Expand search range based on wait time
            float expandedRange = maxMMRDifferenceForMatch + (waitTimeSeconds / 10f) * matchmakingExpandRate;
            expandedRange = Mathf.Min(expandedRange, 500f); // Max 500 MMR difference

            return Mathf.Abs(playerMMR - opponentMMR) <= expandedRange;
        }

        /// <summary>
        /// Get ideal opponent MMR range for matchmaking
        /// </summary>
        public (int min, int max) GetMatchmakingRange(float waitTimeSeconds = 0)
        {
            float expandedRange = maxMMRDifferenceForMatch + (waitTimeSeconds / 10f) * matchmakingExpandRate;
            expandedRange = Mathf.Min(expandedRange, 500f);

            int minOpponentMMR = Mathf.Max(minMMR, _currentMMR - Mathf.RoundToInt(expandedRange));
            int maxOpponentMMR = Mathf.Min(maxMMR, _currentMMR + Mathf.RoundToInt(expandedRange));

            return (minOpponentMMR, maxOpponentMMR);
        }

        #endregion

        #region Season Management

        /// <summary>
        /// Check if season should reset
        /// </summary>
        public void CheckSeasonReset()
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan elapsed = now - _seasonStartDate;

            if (elapsed.TotalDays >= seasonDurationDays)
            {
                PerformSeasonReset();
            }
        }

        /// <summary>
        /// Perform soft reset for new season
        /// </summary>
        private void PerformSeasonReset()
        {
            // Calculate season rewards based on highest rank achieved
            SeasonRewards rewards = CalculateSeasonRewards();

            // Soft reset MMR: (currentMMR + startingMMR) / 2
            int oldMMR = _currentMMR;
            _currentMMR = (_currentMMR + startingMMR) / 2;

            // Update rank
            CashBattleRank oldRank = _currentRank;
            _currentRank = GetRankForMMR(_currentMMR);

            // Reset season stats
            _seasonWins = 0;
            _seasonLosses = 0;
            _seasonHighestMMR = _currentMMR;
            _seasonStartDate = DateTime.UtcNow;
            _currentSeasonNumber++;

            SaveData();

            Debug.Log($"[CashBattleRank] SEASON RESET! Season {_currentSeasonNumber} started. " +
                     $"MMR: {oldMMR} → {_currentMMR} (soft reset)");

            OnSeasonReset?.Invoke(_currentSeasonNumber);
            OnMMRChanged?.Invoke(oldMMR, _currentMMR);

            if (oldRank != _currentRank)
            {
                OnRankChanged?.Invoke(oldRank, _currentRank);
            }

            OnSeasonRewardsEarned?.Invoke(rewards);
        }

        private SeasonRewards CalculateSeasonRewards()
        {
            CashBattleRank highestRank = GetRankForMMR(_seasonHighestMMR);

            return new SeasonRewards
            {
                seasonNumber = _currentSeasonNumber,
                highestRank = highestRank,
                totalWins = _seasonWins,
                totalLosses = _seasonLosses,
                highestMMR = _seasonHighestMMR,

                // Rewards based on rank
                coinsReward = GetCoinRewardForRank(highestRank),
                gemsReward = GetGemRewardForRank(highestRank),
                titleReward = GetTitleForRank(highestRank),
                frameReward = GetFrameForRank(highestRank)
            };
        }

        private int GetCoinRewardForRank(CashBattleRank rank)
        {
            return rank switch
            {
                CashBattleRank.Novato => 100,
                CashBattleRank.Bronce => 250,
                CashBattleRank.Plata => 500,
                CashBattleRank.Oro => 1000,
                CashBattleRank.Platino => 2000,
                CashBattleRank.Diamante => 3500,
                CashBattleRank.Maestro => 5000,
                CashBattleRank.GranMaestro => 7500,
                CashBattleRank.Leyenda => 10000,
                CashBattleRank.Inmortal => 15000,
                _ => 0
            };
        }

        private int GetGemRewardForRank(CashBattleRank rank)
        {
            return rank switch
            {
                CashBattleRank.Novato => 0,
                CashBattleRank.Bronce => 10,
                CashBattleRank.Plata => 25,
                CashBattleRank.Oro => 50,
                CashBattleRank.Platino => 100,
                CashBattleRank.Diamante => 175,
                CashBattleRank.Maestro => 250,
                CashBattleRank.GranMaestro => 400,
                CashBattleRank.Leyenda => 600,
                CashBattleRank.Inmortal => 1000,
                _ => 0
            };
        }

        private string GetTitleForRank(CashBattleRank rank)
        {
            if (rank >= CashBattleRank.Diamante)
            {
                return $"season_{_currentSeasonNumber}_{rank.ToString().ToLower()}";
            }
            return null;
        }

        private string GetFrameForRank(CashBattleRank rank)
        {
            if (rank >= CashBattleRank.Oro)
            {
                return $"frame_season_{_currentSeasonNumber}_{rank.ToString().ToLower()}";
            }
            return null;
        }

        #endregion

        #region Rank Helpers

        public static CashBattleRank GetRankForMMR(int mmr)
        {
            for (int i = RankDefinitions.Length - 1; i >= 0; i--)
            {
                if (mmr >= RankDefinitions[i].minMMR)
                {
                    return RankDefinitions[i].rank;
                }
            }
            return CashBattleRank.Novato;
        }

        public static RankDefinition GetRankDefinition(CashBattleRank rank)
        {
            foreach (var def in RankDefinitions)
            {
                if (def.rank == rank) return def;
            }
            return RankDefinitions[0];
        }

        public static RankDefinition[] GetAllRankDefinitions() => RankDefinitions;

        /// <summary>
        /// Get progress within current rank (0.0 to 1.0)
        /// </summary>
        public float GetRankProgress()
        {
            var rankDef = GetRankDefinition(_currentRank);
            int rangeSize = rankDef.maxMMR - rankDef.minMMR;
            if (rangeSize <= 0) return 1f;

            return (float)(_currentMMR - rankDef.minMMR) / rangeSize;
        }

        /// <summary>
        /// Get MMR needed for next rank
        /// </summary>
        public int GetMMRToNextRank()
        {
            if (_currentRank == CashBattleRank.Inmortal) return 0;

            int nextRankIndex = (int)_currentRank + 1;
            if (nextRankIndex < RankDefinitions.Length)
            {
                return RankDefinitions[nextRankIndex].minMMR - _currentMMR;
            }
            return 0;
        }

        #endregion

        #region Public Getters

        public int CurrentMMR => _currentMMR;
        public CashBattleRank CurrentRank => _currentRank;
        public int SeasonWins => _seasonWins;
        public int SeasonLosses => _seasonLosses;
        public int SeasonGames => _seasonWins + _seasonLosses;
        public float SeasonWinRate => SeasonGames > 0 ? (float)_seasonWins / SeasonGames : 0f;
        public int SeasonHighestMMR => _seasonHighestMMR;
        public int TotalCashBattles => _totalCashBattles;
        public int TotalCashWins => _totalCashWins;
        public float TotalEarnings => _totalEarnings;
        public int CurrentSeasonNumber => _currentSeasonNumber;
        public DateTime SeasonStartDate => _seasonStartDate;

        public TimeSpan TimeUntilSeasonEnd
        {
            get
            {
                DateTime seasonEnd = _seasonStartDate.AddDays(seasonDurationDays);
                TimeSpan remaining = seasonEnd - DateTime.UtcNow;
                return remaining.TotalSeconds > 0 ? remaining : TimeSpan.Zero;
            }
        }

        public string GetRankDisplayName() => GetRankDefinition(_currentRank).displayName;
        public Color GetRankColor() => GetRankDefinition(_currentRank).color;
        public string GetRankIconId() => GetRankDefinition(_currentRank).iconId;

        #endregion

        #region Save/Load

        private void SaveData()
        {
            var data = new CashBattleRankData
            {
                mmr = _currentMMR,
                seasonWins = _seasonWins,
                seasonLosses = _seasonLosses,
                seasonHighestMMR = _seasonHighestMMR,
                totalCashBattles = _totalCashBattles,
                totalCashWins = _totalCashWins,
                totalEarnings = _totalEarnings,
                seasonStartDate = _seasonStartDate.ToBinary(),
                seasonNumber = _currentSeasonNumber
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
                var data = JsonUtility.FromJson<CashBattleRankData>(json);

                _currentMMR = data.mmr;
                _seasonWins = data.seasonWins;
                _seasonLosses = data.seasonLosses;
                _seasonHighestMMR = data.seasonHighestMMR;
                _totalCashBattles = data.totalCashBattles;
                _totalCashWins = data.totalCashWins;
                _totalEarnings = data.totalEarnings;
                _seasonStartDate = DateTime.FromBinary(data.seasonStartDate);
                _currentSeasonNumber = data.seasonNumber;
                _currentRank = GetRankForMMR(_currentMMR);
            }
            else
            {
                // First time - initialize
                _currentMMR = startingMMR;
                _currentRank = GetRankForMMR(_currentMMR);
                _seasonStartDate = DateTime.UtcNow;
                _currentSeasonNumber = 1;
                _seasonHighestMMR = _currentMMR;
                SaveData();
            }
        }

        [ContextMenu("Reset Rank Data (DEBUG)")]
        public void ResetData()
        {
            _currentMMR = startingMMR;
            _currentRank = CashBattleRank.Novato;
            _seasonWins = 0;
            _seasonLosses = 0;
            _seasonHighestMMR = startingMMR;
            _totalCashBattles = 0;
            _totalCashWins = 0;
            _totalEarnings = 0;
            _seasonStartDate = DateTime.UtcNow;
            _currentSeasonNumber = 1;
            SaveData();
            Debug.Log("[CashBattleRank] Data reset!");
        }

        [ContextMenu("Force Season Reset (DEBUG)")]
        public void ForceSeasonReset()
        {
            PerformSeasonReset();
        }

        #endregion
    }

    #region Enums and Data Structures

    public enum CashBattleRank
    {
        Novato = 0,
        Bronce = 1,
        Plata = 2,
        Oro = 3,
        Platino = 4,
        Diamante = 5,
        Maestro = 6,
        GranMaestro = 7,
        Leyenda = 8,
        Inmortal = 9
    }

    [Serializable]
    public class RankDefinition
    {
        public CashBattleRank rank;
        public int minMMR;
        public int maxMMR;
        public string displayName;
        public string iconId;
        public Color color;

        public RankDefinition(CashBattleRank rank, int minMMR, int maxMMR, string displayName, string iconId, Color color)
        {
            this.rank = rank;
            this.minMMR = minMMR;
            this.maxMMR = maxMMR;
            this.displayName = displayName;
            this.iconId = iconId;
            this.color = color;
        }
    }

    [Serializable]
    public class CashBattleRankData
    {
        public int mmr;
        public int seasonWins;
        public int seasonLosses;
        public int seasonHighestMMR;
        public int totalCashBattles;
        public int totalCashWins;
        public float totalEarnings;
        public long seasonStartDate;
        public int seasonNumber;
    }

    [Serializable]
    public class CashBattleResult
    {
        public bool isWin;
        public int opponentMMR;
        public float earnings; // Net earnings (positive or negative)
        public string opponentId;
        public string gameId;
    }

    [Serializable]
    public class SeasonRewards
    {
        public int seasonNumber;
        public CashBattleRank highestRank;
        public int totalWins;
        public int totalLosses;
        public int highestMMR;
        public int coinsReward;
        public int gemsReward;
        public string titleReward; // Can be null
        public string frameReward; // Can be null
    }

    #endregion
}
