using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DigitPark.Services.Firebase;

namespace DigitPark.Services
{
    /// <summary>
    /// Servicio singleton que gestiona marcos de perfil.
    /// 34 marcos con diferentes metodos de obtencion (8 coin, 6 gem, 12 real money, 5 achievement, 3 secret).
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
                primaryColor = new Color(0.702f, 1f, 1f),
                secondaryColor = new Color(0.4f, 1f, 0.933f)
            });

            _allFrames.Add(new FrameData
            {
                frameId = "platinum",
                nameKey = "frame_platinum",
                rarity = FrameRarity.Legendary,
                priceType = FramePriceType.DigitCoins,
                coinPrice = 15000,
                primaryColor = new Color(0.9f, 0.95f, 1f),
                secondaryColor = new Color(0.7f, 0.75f, 0.85f),
                isAnimated = true,
                animationType = FrameAnimationType.Shimmer,
                animationIntensity = 1
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
                primaryColor = new Color(0.8f, 0.533f, 0f),
                secondaryColor = new Color(1f, 0.6f, 0f)
            });

            _allFrames.Add(new FrameData
            {
                frameId = "obsidian",
                nameKey = "frame_obsidian",
                rarity = FrameRarity.Legendary,
                priceType = FramePriceType.DigitGems,
                gemPrice = 1000,
                primaryColor = new Color(0.1f, 0.1f, 0.15f),
                secondaryColor = new Color(0.3f, 0.05f, 0.4f),
                accentColor = new Color(0.1f, 0f, 0.15f),
                isAnimated = true,
                animationType = FrameAnimationType.StarParticles,
                animationIntensity = 1
            });

            // ==================== REAL MONEY (3 frames) ====================
            _allFrames.Add(new FrameData
            {
                frameId = "holographic",
                nameKey = "frame_holographic",
                rarity = FrameRarity.Epic,
                priceType = FramePriceType.RealMoney,
                realMoneyPrice = 1.99f,
                primaryColor = new Color(1f, 1f, 1f),
                secondaryColor = new Color(1f, 0.84f, 0f),
                accentColor = new Color(0.941f, 0.973f, 1f),
                isAnimated = true,
                animationType = FrameAnimationType.SpectrumCycle,
                animationIntensity = 2
            });

            _allFrames.Add(new FrameData
            {
                frameId = "animated_fire",
                nameKey = "frame_quantum_fire",
                rarity = FrameRarity.Epic,
                priceType = FramePriceType.RealMoney,
                realMoneyPrice = 2.99f,
                primaryColor = new Color(0f, 0.533f, 1f),
                secondaryColor = new Color(0f, 1f, 1f),
                accentColor = new Color(1f, 1f, 1f),
                isAnimated = true,
                animationType = FrameAnimationType.PlasmaFire,
                animationIntensity = 3
            });

            _allFrames.Add(new FrameData
            {
                frameId = "legendary_crown",
                nameKey = "frame_legendary_crown",
                rarity = FrameRarity.Legendary,
                priceType = FramePriceType.RealMoney,
                realMoneyPrice = 4.99f,
                primaryColor = new Color(0.176f, 0f, 0.349f),
                secondaryColor = new Color(1f, 0.84f, 0f),
                accentColor = new Color(1f, 0.549f, 0f),
                isAnimated = true,
                animationType = FrameAnimationType.CrownPulse,
                animationIntensity = 2
            });

            // ==================== REAL MONEY — nuevos 9 frames ====================
            _allFrames.Add(new FrameData
            {
                frameId = "plasma_spark",
                nameKey = "frame_plasma_spark",
                rarity = FrameRarity.Rare,
                priceType = FramePriceType.RealMoney,
                realMoneyPrice = 0.99f,
                primaryColor = new Color(0f, 0.749f, 1f),
                secondaryColor = new Color(1f, 1f, 1f),
                accentColor = new Color(0.251f, 0.878f, 0.816f),
                isAnimated = true,
                animationType = FrameAnimationType.ElectricArcs,
                animationIntensity = 2
            });

            _allFrames.Add(new FrameData
            {
                frameId = "prism_shift",
                nameKey = "frame_prism_shift",
                rarity = FrameRarity.Rare,
                priceType = FramePriceType.RealMoney,
                realMoneyPrice = 0.99f,
                primaryColor = new Color(1f, 0f, 0f),
                secondaryColor = new Color(1f, 1f, 1f),
                isAnimated = true,
                animationType = FrameAnimationType.HueRotation,
                animationIntensity = 2
            });

            _allFrames.Add(new FrameData
            {
                frameId = "aurora_borealis",
                nameKey = "frame_aurora_borealis",
                rarity = FrameRarity.Epic,
                priceType = FramePriceType.RealMoney,
                realMoneyPrice = 3.99f,
                primaryColor = new Color(0f, 1f, 0.8f),
                secondaryColor = new Color(0.482f, 0.184f, 0.745f),
                accentColor = new Color(0f, 0.808f, 0.82f),
                isAnimated = true,
                animationType = FrameAnimationType.AuroraRibbons,
                animationIntensity = 2
            });

            _allFrames.Add(new FrameData
            {
                frameId = "void_walker",
                nameKey = "frame_void_walker",
                rarity = FrameRarity.Legendary,
                priceType = FramePriceType.RealMoney,
                realMoneyPrice = 5.99f,
                primaryColor = new Color(0.02f, 0.02f, 0.188f),
                secondaryColor = new Color(0.251f, 0.376f, 1f),
                accentColor = new Color(1f, 1f, 1f),
                isAnimated = true,
                animationType = FrameAnimationType.StarParticles,
                animationIntensity = 1
            });

            _allFrames.Add(new FrameData
            {
                frameId = "storm_surge",
                nameKey = "frame_storm_surge",
                rarity = FrameRarity.Legendary,
                priceType = FramePriceType.RealMoney,
                realMoneyPrice = 5.99f,
                primaryColor = new Color(0.055f, 0.102f, 0.239f),
                secondaryColor = new Color(0.878f, 0.878f, 1f),
                accentColor = new Color(1f, 0.843f, 0f),
                isAnimated = true,
                animationType = FrameAnimationType.LightningFlash,
                animationIntensity = 3
            });

            _allFrames.Add(new FrameData
            {
                frameId = "cosmic_rift",
                nameKey = "frame_cosmic_rift",
                rarity = FrameRarity.Legendary,
                priceType = FramePriceType.RealMoney,
                realMoneyPrice = 9.99f,
                primaryColor = new Color(0.039f, 0f, 0.082f),
                secondaryColor = new Color(1f, 0f, 1f),
                accentColor = new Color(1f, 0.502f, 1f),
                isAnimated = true,
                animationType = FrameAnimationType.CrackGlow,
                animationIntensity = 3
            });

            _allFrames.Add(new FrameData
            {
                frameId = "infernal_god",
                nameKey = "frame_infernal_god",
                rarity = FrameRarity.Legendary,
                priceType = FramePriceType.RealMoney,
                realMoneyPrice = 9.99f,
                primaryColor = new Color(0.102f, 0f, 0f),
                secondaryColor = new Color(1f, 0.133f, 0f),
                accentColor = new Color(1f, 0.533f, 0f),
                isAnimated = true,
                animationType = FrameAnimationType.LayeredFire,
                animationIntensity = 3
            });

            _allFrames.Add(new FrameData
            {
                frameId = "divine_light",
                nameKey = "frame_divine_light",
                rarity = FrameRarity.Legendary,
                priceType = FramePriceType.RealMoney,
                realMoneyPrice = 14.99f,
                primaryColor = new Color(1f, 0.843f, 0f),
                secondaryColor = new Color(1f, 1f, 1f),
                accentColor = new Color(1f, 0.961f, 0.667f),
                isAnimated = true,
                animationType = FrameAnimationType.GodRays,
                animationIntensity = 2
            });

            _allFrames.Add(new FrameData
            {
                frameId = "quantum_break",
                nameKey = "frame_quantum_break",
                rarity = FrameRarity.Legendary,
                priceType = FramePriceType.RealMoney,
                realMoneyPrice = 14.99f,
                primaryColor = new Color(0f, 1f, 0.255f),
                secondaryColor = new Color(0.051f, 0.051f, 0.051f),
                accentColor = new Color(1f, 0f, 0.267f),
                isAnimated = true,
                animationType = FrameAnimationType.GlitchBurst,
                animationIntensity = 3
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
                primaryColor = new Color(0.439f, 0.502f, 0.565f),
                secondaryColor = new Color(0.69f, 0.769f, 0.871f)
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
                secondaryColor = new Color(0.8f, 0.2f, 0.5f),
                isAnimated = true,
                animationType = FrameAnimationType.Shimmer,
                animationIntensity = 1
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
                isAnimated = true,
                animationType = FrameAnimationType.Shimmer,
                animationIntensity = 1
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
                isAnimated = true,
                animationType = FrameAnimationType.LightningFlash,
                animationIntensity = 2
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
            string ownedJson = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(OWNED_FRAMES_KEY, ownedJson);
            PlayerPrefs.Save();

            // Sync to Firebase
            SyncFramesToFirebase(_equippedFrameId, ownedJson).ContinueWith(t =>
            {
                if (t.IsFaulted)
                    Debug.LogWarning($"[PlayerFrameService] Firebase sync failed: {t.Exception?.GetBaseException().Message}");
            }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
        }

        private void SaveEquippedFrame()
        {
            PlayerPrefs.SetString(EQUIPPED_FRAME_KEY, _equippedFrameId);
            PlayerPrefs.Save();

            // Sync to Firebase
            var data = new StringListWrapper { items = new List<string>(_ownedFrames) };
            string ownedJson = JsonUtility.ToJson(data);
            SyncFramesToFirebase(_equippedFrameId, ownedJson).ContinueWith(t =>
            {
                if (t.IsFaulted)
                    Debug.LogWarning($"[PlayerFrameService] Firebase sync failed: {t.Exception?.GetBaseException().Message}");
            }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
        }

        private async Task SyncFramesToFirebase(string equippedFrame, string ownedFramesJson)
        {
            var playerData = AuthenticationService.Instance?.GetCurrentPlayerData();
            if (playerData == null) return;

            try
            {
                var updates = new Dictionary<string, object>
                {
                    { "equippedFrame", equippedFrame },
                    { "ownedFrames", ownedFramesJson }
                };

                if (DatabaseService.Instance != null)
                {
                    await DatabaseService.Instance.UpdatePlayerFields(playerData.userId, updates);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[PlayerFrameService] Error syncing to Firebase: {e.Message}");
            }
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
