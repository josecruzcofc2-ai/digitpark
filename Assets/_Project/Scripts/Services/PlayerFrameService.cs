using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitPark.Services
{
    /// <summary>
    /// Servicio singleton que gestiona marcos de perfil.
    /// 25 marcos con diferentes metodos de obtencion.
    /// Persistencia via PlayerPrefs.
    /// </summary>
    public class PlayerFrameService : MonoBehaviour
    {
        private static PlayerFrameService _instance;
        public static PlayerFrameService Instance => _instance;

        // PlayerPrefs keys
        private const string OWNED_FRAMES_KEY = "OwnedFrames";
        private const string EQUIPPED_FRAME_KEY = "EquippedFrame";
        private const string DEFAULT_FRAME_ID = "basic";

        // State
        private HashSet<string> _ownedFrames = new HashSet<string>();
        private string _equippedFrameId;
        private List<FrameData> _allFrames = new List<FrameData>();

        // Events
        public event Action<string> OnFrameChanged;
        public event Action<string> OnFrameUnlocked;

        public List<FrameData> AllFrames => _allFrames;
        public string EquippedFrameId => _equippedFrameId;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            SetupFrames();
            LoadState();

            // Ensure basic frame is always owned
            if (!_ownedFrames.Contains(DEFAULT_FRAME_ID))
            {
                _ownedFrames.Add(DEFAULT_FRAME_ID);
                SaveOwnedFrames();
            }

            if (string.IsNullOrEmpty(_equippedFrameId))
            {
                _equippedFrameId = DEFAULT_FRAME_ID;
                SaveEquippedFrame();
            }

            Debug.Log($"[PlayerFrameService] Initialized: {_ownedFrames.Count} owned, equipped: {_equippedFrameId}");
        }

        private void SetupFrames()
        {
            // ==================== COINS (8 frames) ====================
            _allFrames.Add(new FrameData
            {
                frameId = "basic",
                nameKey = "frame_basic",
                rarity = FrameRarity.Common,
                priceType = FramePriceType.DigitCoins,
                coinPrice = 500,
                primaryColor = new Color(0.5f, 0.5f, 0.5f),
                secondaryColor = new Color(0.3f, 0.3f, 0.3f)
            });

            _allFrames.Add(new FrameData
            {
                frameId = "bronze",
                nameKey = "frame_bronze",
                rarity = FrameRarity.Common,
                priceType = FramePriceType.DigitCoins,
                coinPrice = 1000,
                primaryColor = new Color(0.8f, 0.5f, 0.2f),
                secondaryColor = new Color(0.6f, 0.35f, 0.1f)
            });

            _allFrames.Add(new FrameData
            {
                frameId = "silver",
                nameKey = "frame_silver",
                rarity = FrameRarity.Common,
                priceType = FramePriceType.DigitCoins,
                coinPrice = 2500,
                primaryColor = new Color(0.75f, 0.75f, 0.8f),
                secondaryColor = new Color(0.5f, 0.5f, 0.55f)
            });

            _allFrames.Add(new FrameData
            {
                frameId = "gold",
                nameKey = "frame_gold",
                rarity = FrameRarity.Rare,
                priceType = FramePriceType.DigitCoins,
                coinPrice = 5000,
                primaryColor = new Color(1f, 0.84f, 0f),
                secondaryColor = new Color(0.85f, 0.65f, 0f)
            });

            _allFrames.Add(new FrameData
            {
                frameId = "neon",
                nameKey = "frame_neon",
                rarity = FrameRarity.Rare,
                priceType = FramePriceType.DigitCoins,
                coinPrice = 7500,
                primaryColor = new Color(0f, 1f, 0.5f),
                secondaryColor = new Color(0f, 0.8f, 1f)
            });

            _allFrames.Add(new FrameData
            {
                frameId = "diamond",
                nameKey = "frame_diamond",
                rarity = FrameRarity.Epic,
                priceType = FramePriceType.DigitCoins,
                coinPrice = 10000,
                primaryColor = new Color(0.7f, 0.9f, 1f),
                secondaryColor = new Color(0.4f, 0.7f, 1f)
            });

            _allFrames.Add(new FrameData
            {
                frameId = "crystal",
                nameKey = "frame_crystal",
                rarity = FrameRarity.Epic,
                priceType = FramePriceType.DigitCoins,
                coinPrice = 12000,
                primaryColor = new Color(0.8f, 0.6f, 1f),
                secondaryColor = new Color(0.5f, 0.2f, 0.8f)
            });

            _allFrames.Add(new FrameData
            {
                frameId = "platinum",
                nameKey = "frame_platinum",
                rarity = FrameRarity.Legendary,
                priceType = FramePriceType.DigitCoins,
                coinPrice = 15000,
                primaryColor = new Color(0.9f, 0.95f, 1f),
                secondaryColor = new Color(0.7f, 0.75f, 0.85f)
            });

            // ==================== GEMS (6 frames) ====================
            _allFrames.Add(new FrameData
            {
                frameId = "sapphire",
                nameKey = "frame_sapphire",
                rarity = FrameRarity.Rare,
                priceType = FramePriceType.DigitGems,
                gemPrice = 50,
                primaryColor = new Color(0.05f, 0.2f, 0.8f),
                secondaryColor = new Color(0.1f, 0.4f, 1f)
            });

            _allFrames.Add(new FrameData
            {
                frameId = "ruby",
                nameKey = "frame_ruby",
                rarity = FrameRarity.Rare,
                priceType = FramePriceType.DigitGems,
                gemPrice = 150,
                primaryColor = new Color(0.9f, 0.1f, 0.2f),
                secondaryColor = new Color(1f, 0.3f, 0.4f)
            });

            _allFrames.Add(new FrameData
            {
                frameId = "emerald",
                nameKey = "frame_emerald",
                rarity = FrameRarity.Epic,
                priceType = FramePriceType.DigitGems,
                gemPrice = 300,
                primaryColor = new Color(0.1f, 0.8f, 0.3f),
                secondaryColor = new Color(0.05f, 0.5f, 0.15f)
            });

            _allFrames.Add(new FrameData
            {
                frameId = "amethyst",
                nameKey = "frame_amethyst",
                rarity = FrameRarity.Epic,
                priceType = FramePriceType.DigitGems,
                gemPrice = 500,
                primaryColor = new Color(0.6f, 0.2f, 0.8f),
                secondaryColor = new Color(0.4f, 0.1f, 0.6f)
            });

            _allFrames.Add(new FrameData
            {
                frameId = "topaz",
                nameKey = "frame_topaz",
                rarity = FrameRarity.Epic,
                priceType = FramePriceType.DigitGems,
                gemPrice = 750,
                primaryColor = new Color(1f, 0.75f, 0f),
                secondaryColor = new Color(1f, 0.5f, 0f)
            });

            _allFrames.Add(new FrameData
            {
                frameId = "obsidian",
                nameKey = "frame_obsidian",
                rarity = FrameRarity.Legendary,
                priceType = FramePriceType.DigitGems,
                gemPrice = 1000,
                primaryColor = new Color(0.1f, 0.1f, 0.15f),
                secondaryColor = new Color(0.3f, 0.05f, 0.4f)
            });

            // ==================== REAL MONEY (3 frames) ====================
            _allFrames.Add(new FrameData
            {
                frameId = "holographic",
                nameKey = "frame_holographic",
                rarity = FrameRarity.Epic,
                priceType = FramePriceType.RealMoney,
                realMoneyPrice = 1.99f,
                primaryColor = new Color(0.8f, 0.9f, 1f),
                secondaryColor = new Color(1f, 0.6f, 0.8f),
                isAnimated = true
            });

            _allFrames.Add(new FrameData
            {
                frameId = "animated_fire",
                nameKey = "frame_animated_fire",
                rarity = FrameRarity.Legendary,
                priceType = FramePriceType.RealMoney,
                realMoneyPrice = 2.99f,
                primaryColor = new Color(1f, 0.4f, 0f),
                secondaryColor = new Color(1f, 0.8f, 0f),
                isAnimated = true
            });

            _allFrames.Add(new FrameData
            {
                frameId = "legendary_crown",
                nameKey = "frame_legendary_crown",
                rarity = FrameRarity.Legendary,
                priceType = FramePriceType.RealMoney,
                realMoneyPrice = 4.99f,
                primaryColor = new Color(1f, 0.84f, 0f),
                secondaryColor = new Color(0.6f, 0.2f, 0.8f),
                isAnimated = true
            });

            // ==================== ACHIEVEMENT (5 frames) ====================
            _allFrames.Add(new FrameData
            {
                frameId = "first_win_frame",
                nameKey = "frame_first_win",
                rarity = FrameRarity.Common,
                priceType = FramePriceType.Achievement,
                achievementId = "first_win",
                primaryColor = new Color(0.2f, 0.8f, 0.4f),
                secondaryColor = new Color(0.1f, 0.5f, 0.2f)
            });

            _allFrames.Add(new FrameData
            {
                frameId = "centurion_frame",
                nameKey = "frame_centurion",
                rarity = FrameRarity.Rare,
                priceType = FramePriceType.Achievement,
                achievementId = "games_100",
                primaryColor = new Color(0.8f, 0.6f, 0.2f),
                secondaryColor = new Color(0.6f, 0.4f, 0.1f)
            });

            _allFrames.Add(new FrameData
            {
                frameId = "master_frame",
                nameKey = "frame_master",
                rarity = FrameRarity.Epic,
                priceType = FramePriceType.Achievement,
                achievementId = "wins_100",
                primaryColor = new Color(0.9f, 0.1f, 0.1f),
                secondaryColor = new Color(1f, 0.5f, 0f)
            });

            _allFrames.Add(new FrameData
            {
                frameId = "social_butterfly",
                nameKey = "frame_social_butterfly",
                rarity = FrameRarity.Rare,
                priceType = FramePriceType.Achievement,
                achievementId = "friends_50",
                primaryColor = new Color(1f, 0.4f, 0.7f),
                secondaryColor = new Color(0.8f, 0.2f, 0.5f)
            });

            _allFrames.Add(new FrameData
            {
                frameId = "streak_king",
                nameKey = "frame_streak_king",
                rarity = FrameRarity.Epic,
                priceType = FramePriceType.Achievement,
                achievementId = "streak_20",
                primaryColor = new Color(1f, 0.5f, 0f),
                secondaryColor = new Color(1f, 0.2f, 0f),
                isAnimated = true
            });

            // ==================== SECRET (3 frames) ====================
            _allFrames.Add(new FrameData
            {
                frameId = "night_owl_frame",
                nameKey = "frame_night_owl",
                rarity = FrameRarity.Rare,
                priceType = FramePriceType.Secret,
                achievementId = "night_owl",
                primaryColor = new Color(0.1f, 0.1f, 0.3f),
                secondaryColor = new Color(0.3f, 0.3f, 0.6f)
            });

            _allFrames.Add(new FrameData
            {
                frameId = "perfect_frame",
                nameKey = "frame_perfect",
                rarity = FrameRarity.Epic,
                priceType = FramePriceType.Secret,
                achievementId = "perfect_game",
                primaryColor = new Color(1f, 1f, 0.8f),
                secondaryColor = new Color(1f, 0.9f, 0.4f),
                isAnimated = true
            });

            _allFrames.Add(new FrameData
            {
                frameId = "speed_demon_frame",
                nameKey = "frame_speed_demon",
                rarity = FrameRarity.Epic,
                priceType = FramePriceType.Secret,
                achievementId = "speed_demon",
                primaryColor = new Color(1f, 0f, 0.3f),
                secondaryColor = new Color(0.8f, 0f, 0.6f),
                isAnimated = true
            });
        }

        // ==================== PERSISTENCE ====================

        private void LoadState()
        {
            // Load owned frames
            string ownedJson = PlayerPrefs.GetString(OWNED_FRAMES_KEY, "");
            if (!string.IsNullOrEmpty(ownedJson))
            {
                try
                {
                    var data = JsonUtility.FromJson<StringListWrapper>(ownedJson);
                    if (data?.items != null)
                    {
                        foreach (var id in data.items)
                            _ownedFrames.Add(id);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[PlayerFrameService] Error loading owned frames: {e.Message}");
                }
            }

            // Load equipped frame
            _equippedFrameId = PlayerPrefs.GetString(EQUIPPED_FRAME_KEY, DEFAULT_FRAME_ID);
        }

        private void SaveOwnedFrames()
        {
            var data = new StringListWrapper { items = new List<string>(_ownedFrames) };
            PlayerPrefs.SetString(OWNED_FRAMES_KEY, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        private void SaveEquippedFrame()
        {
            PlayerPrefs.SetString(EQUIPPED_FRAME_KEY, _equippedFrameId);
            PlayerPrefs.Save();
        }

        // ==================== PUBLIC API ====================

        public bool IsOwned(string frameId)
        {
            return _ownedFrames.Contains(frameId);
        }

        public void UnlockFrame(string frameId)
        {
            if (_ownedFrames.Contains(frameId)) return;

            _ownedFrames.Add(frameId);
            SaveOwnedFrames();

            Debug.Log($"[PlayerFrameService] Frame unlocked: {frameId}");
            OnFrameUnlocked?.Invoke(frameId);
        }

        public void EquipFrame(string frameId)
        {
            if (!_ownedFrames.Contains(frameId))
            {
                Debug.LogWarning($"[PlayerFrameService] Cannot equip frame not owned: {frameId}");
                return;
            }

            _equippedFrameId = frameId;
            SaveEquippedFrame();

            Debug.Log($"[PlayerFrameService] Frame equipped: {frameId}");
            OnFrameChanged?.Invoke(frameId);
        }

        public FrameData GetFrameData(string frameId)
        {
            return _allFrames.Find(f => f.frameId == frameId);
        }

        public FrameData GetEquippedFrame()
        {
            return GetFrameData(_equippedFrameId);
        }

        public List<FrameData> GetOwnedFrames()
        {
            var result = new List<FrameData>();
            foreach (var frame in _allFrames)
            {
                if (_ownedFrames.Contains(frame.frameId))
                    result.Add(frame);
            }
            return result;
        }

        public List<FrameData> GetFramesByPriceType(FramePriceType priceType)
        {
            return _allFrames.FindAll(f => f.priceType == priceType);
        }

        /// <summary>
        /// Attempts to purchase a frame with the appropriate currency.
        /// Returns true if purchase succeeded.
        /// </summary>
        public bool TryPurchaseFrame(string frameId)
        {
            if (IsOwned(frameId)) return false;

            var frame = GetFrameData(frameId);
            if (frame == null) return false;

            var currency = DigitPark.Monetization.CurrencyManager.Instance;
            if (currency == null) return false;

            switch (frame.priceType)
            {
                case FramePriceType.DigitCoins:
                    if (!currency.SpendCoins(frame.coinPrice)) return false;
                    break;
                case FramePriceType.DigitGems:
                    if (!currency.SpendGems(frame.gemPrice)) return false;
                    break;
                case FramePriceType.RealMoney:
                    Debug.LogWarning("[PlayerFrameService] Real money frames should use IAP system");
                    return false;
                case FramePriceType.Achievement:
                case FramePriceType.Secret:
                    // Check if achievement is completed
                    if (!string.IsNullOrEmpty(frame.achievementId))
                    {
                        if (AchievementService.Instance == null || !AchievementService.Instance.IsUnlocked(frame.achievementId))
                        {
                            Debug.Log($"[PlayerFrameService] Achievement not unlocked: {frame.achievementId}");
                            return false;
                        }
                    }
                    break;
            }

            UnlockFrame(frameId);
            return true;
        }

        // ==================== DEBUG ====================

#if UNITY_EDITOR
        [ContextMenu("Debug: Unlock All Frames")]
        private void DebugUnlockAll()
        {
            foreach (var frame in _allFrames)
                UnlockFrame(frame.frameId);
        }

        [ContextMenu("Debug: Reset Frames")]
        private void DebugReset()
        {
            _ownedFrames.Clear();
            _ownedFrames.Add(DEFAULT_FRAME_ID);
            _equippedFrameId = DEFAULT_FRAME_ID;
            SaveOwnedFrames();
            SaveEquippedFrame();
            Debug.Log("[PlayerFrameService] Reset to defaults");
        }
#endif
    }

    /// <summary>
    /// Helper for JSON serialization of string lists
    /// </summary>
    [Serializable]
    public class StringListWrapper
    {
        public List<string> items = new List<string>();
    }
}
