using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DigitPark.Services.Firebase;

namespace DigitPark.Services
{
    /// <summary>
    /// Tipo de precio del titulo
    /// </summary>
    public enum TitlePriceType
    {
        Free,
        DigitCoins,
        DigitGems,
        RealMoney,
        Achievement,
        Secret
    }

    /// <summary>
    /// Datos de un titulo de perfil
    /// </summary>
    [Serializable]
    public class TitleData
    {
        public string titleId;
        public string nameKey;          // Localization key for the display name
        public TitlePriceType priceType;
        public int coinPrice;
        public int gemPrice;
        public float realMoneyPrice;
        public string achievementId;    // Required achievement
        public bool isCustom;           // If true, player can set custom text
    }

    /// <summary>
    /// Servicio singleton que gestiona titulos de perfil.
    /// 24 titulos: 1 free + 4 DC + 5 DG + 4 USD + 1 custom + 6 achievement + 4 secret.
    /// Persistencia via PlayerPrefs.
    /// </summary>
    public class PlayerTitleService : MonoBehaviour
    {
        private static PlayerTitleService _instance;
        public static PlayerTitleService Instance => _instance;

        // PlayerPrefs keys
        private const string OWNED_TITLES_KEY = "OwnedTitles";
        private const string EQUIPPED_TITLE_KEY = "EquippedTitle";
        private const string CUSTOM_TITLE_KEY = "CustomTitle";
        private const string DEFAULT_TITLE_ID = "novice";
        private const int MAX_CUSTOM_LENGTH = 20;

        // State
        private HashSet<string> _ownedTitles = new HashSet<string>();
        private string _equippedTitleId;
        private string _customTitleText = "";
        private List<TitleData> _allTitles = new List<TitleData>();

        // Events
        public event Action<string> OnTitleChanged;
        public event Action<string> OnTitleUnlocked;

        public List<TitleData> AllTitles => _allTitles;
        public string EquippedTitleId => _equippedTitleId;
        public string CustomTitleText => _customTitleText;

        // Simple word filter
        private static readonly string[] BLOCKED_WORDS = {
            "fuck", "shit", "ass", "dick", "bitch", "damn", "hell",
            "puta", "mierda", "culo", "pene", "joder", "coño",
            "merda", "caralho", "foda", "scheisse", "arsch", "fick"
        };

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
            SetupTitles();
            LoadState();

            // Ensure default title is always owned
            if (!_ownedTitles.Contains(DEFAULT_TITLE_ID))
            {
                _ownedTitles.Add(DEFAULT_TITLE_ID);
                SaveOwnedTitles();
            }

            if (string.IsNullOrEmpty(_equippedTitleId))
            {
                _equippedTitleId = DEFAULT_TITLE_ID;
                SaveEquippedTitle();
            }

            Debug.Log($"[PlayerTitleService] Initialized: {_ownedTitles.Count} owned, equipped: {_equippedTitleId}");
        }

        private void SetupTitles()
        {
            // ==================== FREE (1 title — default) ====================
            _allTitles.Add(new TitleData
            {
                titleId = "novice",
                nameKey = "title_novice",
                priceType = TitlePriceType.Free,
                coinPrice = 0
            });

            // ==================== DIGIT COINS (4 titles) ====================
            _allTitles.Add(new TitleData
            {
                titleId = "strategist",
                nameKey = "title_strategist",
                priceType = TitlePriceType.DigitCoins,
                coinPrice = 8000
            });

            _allTitles.Add(new TitleData
            {
                titleId = "analyst",
                nameKey = "title_analyst",
                priceType = TitlePriceType.DigitCoins,
                coinPrice = 8000
            });

            _allTitles.Add(new TitleData
            {
                titleId = "champion",
                nameKey = "title_champion",
                priceType = TitlePriceType.DigitCoins,
                coinPrice = 20000
            });

            _allTitles.Add(new TitleData
            {
                titleId = "gladiator",
                nameKey = "title_gladiator",
                priceType = TitlePriceType.DigitCoins,
                coinPrice = 20000
            });

            // ==================== DIGIT GEMS (5 titles) ====================
            _allTitles.Add(new TitleData
            {
                titleId = "mastermind",
                nameKey = "title_mastermind",
                priceType = TitlePriceType.DigitGems,
                gemPrice = 150
            });

            _allTitles.Add(new TitleData
            {
                titleId = "prodigy",
                nameKey = "title_prodigy",
                priceType = TitlePriceType.DigitGems,
                gemPrice = 150
            });

            _allTitles.Add(new TitleData
            {
                titleId = "titan",
                nameKey = "title_titan",
                priceType = TitlePriceType.DigitGems,
                gemPrice = 300
            });

            _allTitles.Add(new TitleData
            {
                titleId = "oracle",
                nameKey = "title_oracle",
                priceType = TitlePriceType.DigitGems,
                gemPrice = 300
            });

            _allTitles.Add(new TitleData
            {
                titleId = "phoenix",
                nameKey = "title_phoenix",
                priceType = TitlePriceType.DigitGems,
                gemPrice = 500
            });

            // ==================== REAL MONEY (4 titles via IAP) ====================
            _allTitles.Add(new TitleData
            {
                titleId = "quantum",
                nameKey = "title_quantum",
                priceType = TitlePriceType.RealMoney,
                realMoneyPrice = 1.99f
            });

            _allTitles.Add(new TitleData
            {
                titleId = "immortal_title",
                nameKey = "title_immortal",
                priceType = TitlePriceType.RealMoney,
                realMoneyPrice = 2.99f
            });

            _allTitles.Add(new TitleData
            {
                titleId = "transcendent",
                nameKey = "title_transcendent",
                priceType = TitlePriceType.RealMoney,
                realMoneyPrice = 4.99f
            });

            _allTitles.Add(new TitleData
            {
                titleId = "apex_predator",
                nameKey = "title_apex_predator",
                priceType = TitlePriceType.RealMoney,
                realMoneyPrice = 9.99f
            });

            // ==================== CUSTOM (1 title — player sets text) ====================
            _allTitles.Add(new TitleData
            {
                titleId = "custom_title",
                nameKey = "title_custom",
                priceType = TitlePriceType.RealMoney,
                realMoneyPrice = 1.99f,
                isCustom = true
            });

            // ==================== ACHIEVEMENT (6 titles) ====================
            _allTitles.Add(new TitleData
            {
                titleId = "first_step",
                nameKey = "title_first_step",
                priceType = TitlePriceType.Achievement,
                achievementId = "first_game"
            });

            _allTitles.Add(new TitleData
            {
                titleId = "unstoppable",
                nameKey = "title_unstoppable",
                priceType = TitlePriceType.Achievement,
                achievementId = "streak_10"
            });

            _allTitles.Add(new TitleData
            {
                titleId = "night_owl",
                nameKey = "title_night_owl",
                priceType = TitlePriceType.Achievement,
                achievementId = "night_owl"
            });

            _allTitles.Add(new TitleData
            {
                titleId = "perfectionist",
                nameKey = "title_perfectionist",
                priceType = TitlePriceType.Achievement,
                achievementId = "perfect_game"
            });

            _allTitles.Add(new TitleData
            {
                titleId = "tournament_champion",
                nameKey = "title_tournament_champion",
                priceType = TitlePriceType.Achievement,
                achievementId = "tournament_win"
            });

            _allTitles.Add(new TitleData
            {
                titleId = "collector",
                nameKey = "title_collector",
                priceType = TitlePriceType.Achievement,
                achievementId = "days_365"
            });

            // ==================== SECRET (4 titles) ====================
            _allTitles.Add(new TitleData
            {
                titleId = "ghost",
                nameKey = "title_ghost",
                priceType = TitlePriceType.Secret,
                achievementId = "night_owl"
            });

            _allTitles.Add(new TitleData
            {
                titleId = "speed_demon",
                nameKey = "title_speed_demon",
                priceType = TitlePriceType.Secret,
                achievementId = "speed_demon"
            });

            _allTitles.Add(new TitleData
            {
                titleId = "comeback_king",
                nameKey = "title_comeback_king",
                priceType = TitlePriceType.Secret,
                achievementId = "comeback_king"
            });

            _allTitles.Add(new TitleData
            {
                titleId = "completionist",
                nameKey = "title_completionist",
                priceType = TitlePriceType.Secret,
                achievementId = "level_100"
            });
        }

        // ==================== PERSISTENCE ====================

        private void LoadState()
        {
            // Load owned titles
            string ownedJson = PlayerPrefs.GetString(OWNED_TITLES_KEY, "");
            if (!string.IsNullOrEmpty(ownedJson))
            {
                try
                {
                    var data = JsonUtility.FromJson<StringListWrapper>(ownedJson);
                    if (data?.items != null)
                    {
                        foreach (var id in data.items)
                            _ownedTitles.Add(id);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[PlayerTitleService] Error loading owned titles: {e.Message}");
                }
            }

            _equippedTitleId = PlayerPrefs.GetString(EQUIPPED_TITLE_KEY, DEFAULT_TITLE_ID);
            _customTitleText = PlayerPrefs.GetString(CUSTOM_TITLE_KEY, "");
        }

        private void SaveOwnedTitles()
        {
            var data = new StringListWrapper { items = new List<string>(_ownedTitles) };
            PlayerPrefs.SetString(OWNED_TITLES_KEY, JsonUtility.ToJson(data));
            PlayerPrefs.Save();

            // Sync to Firebase
            SyncTitlesToFirebase().ContinueWith(t =>
            {
                if (t.IsFaulted)
                    Debug.LogWarning($"[PlayerTitleService] Firebase sync failed: {t.Exception?.GetBaseException().Message}");
            }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
        }

        private void SaveEquippedTitle()
        {
            PlayerPrefs.SetString(EQUIPPED_TITLE_KEY, _equippedTitleId);
            PlayerPrefs.Save();

            // Sync to Firebase
            SyncTitlesToFirebase().ContinueWith(t =>
            {
                if (t.IsFaulted)
                    Debug.LogWarning($"[PlayerTitleService] Firebase sync failed: {t.Exception?.GetBaseException().Message}");
            }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
        }

        private async Task SyncTitlesToFirebase()
        {
            var playerData = AuthenticationService.Instance?.GetCurrentPlayerData();
            if (playerData == null) return;

            try
            {
                var ownedData = new StringListWrapper { items = new List<string>(_ownedTitles) };
                string ownedJson = JsonUtility.ToJson(ownedData);

                var updates = new Dictionary<string, object>
                {
                    { "equippedTitle", _equippedTitleId },
                    { "ownedTitles", ownedJson },
                    { "customTitle", _customTitleText }
                };

                if (DatabaseService.Instance != null)
                {
                    await DatabaseService.Instance.UpdatePlayerFields(playerData.userId, updates);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[PlayerTitleService] Error syncing to Firebase: {e.Message}");
            }
        }

        /// <summary>
        /// B4-D: Restore owned titles from Firebase after reinstall.
        /// Call from BootManager in reinstall path.
        /// </summary>
        public async Task RestoreFromFirebaseAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId) || DatabaseService.Instance == null) return;
            try
            {
                var db = global::Firebase.Database.FirebaseDatabase.DefaultInstance;
                var snapshot = await db.GetReference("players").Child(userId).Child("ownedTitles").GetValueAsync();
                if (snapshot == null || !snapshot.Exists) return;

                string ownedJson = snapshot.Value?.ToString() ?? "";
                if (string.IsNullOrEmpty(ownedJson)) return;

                var data = JsonUtility.FromJson<StringListWrapper>(ownedJson);
                if (data?.items == null) return;

                bool changed = false;
                foreach (var id in data.items)
                {
                    if (!_ownedTitles.Contains(id))
                    {
                        _ownedTitles.Add(id);
                        changed = true;
                    }
                }
                if (changed)
                {
                    SaveOwnedTitles();
                    Debug.Log($"[PlayerTitleService] Titles restored from Firebase");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[PlayerTitleService] RestoreFromFirebaseAsync failed: {e.Message}");
            }
        }

        // ==================== PUBLIC API ====================

        /// <summary>B4-F: Force a Firebase sync of current owned titles (used during migration).</summary>
        public void TriggerFirebaseSync() => SaveOwnedTitles();

        public bool IsOwned(string titleId)
        {
            return _ownedTitles.Contains(titleId);
        }

        public void UnlockTitle(string titleId)
        {
            if (_ownedTitles.Contains(titleId)) return;

            _ownedTitles.Add(titleId);
            SaveOwnedTitles();

            Debug.Log($"[PlayerTitleService] Title unlocked: {titleId}");
            OnTitleUnlocked?.Invoke(titleId);
        }

        public void EquipTitle(string titleId)
        {
            if (!_ownedTitles.Contains(titleId))
            {
                Debug.LogWarning($"[PlayerTitleService] Cannot equip title not owned: {titleId}");
                return;
            }

            _equippedTitleId = titleId;
            SaveEquippedTitle();

            Debug.Log($"[PlayerTitleService] Title equipped: {titleId}");
            OnTitleChanged?.Invoke(titleId);
        }

        public TitleData GetTitleData(string titleId)
        {
            return _allTitles.Find(t => t.titleId == titleId);
        }

        public TitleData GetEquippedTitle()
        {
            return GetTitleData(_equippedTitleId);
        }

        /// <summary>
        /// Gets the display text for the currently equipped title
        /// </summary>
        public string GetEquippedTitleDisplay()
        {
            var title = GetEquippedTitle();
            if (title == null) return "";

            if (title.isCustom && !string.IsNullOrEmpty(_customTitleText))
                return _customTitleText;

            return DigitPark.Localization.AutoLocalizer.Get(title.nameKey);
        }

        public List<TitleData> GetOwnedTitles()
        {
            var result = new List<TitleData>();
            foreach (var title in _allTitles)
            {
                if (_ownedTitles.Contains(title.titleId))
                    result.Add(title);
            }
            return result;
        }

        /// <summary>
        /// Sets the custom title text. Must own the custom_title first.
        /// Returns false if word filter blocks it.
        /// </summary>
        public bool SetCustomTitle(string text)
        {
            if (!IsOwned("custom_title"))
            {
                Debug.LogWarning("[PlayerTitleService] Custom title not owned");
                return false;
            }

            if (string.IsNullOrWhiteSpace(text)) return false;
            if (text.Length > MAX_CUSTOM_LENGTH) text = text.Substring(0, MAX_CUSTOM_LENGTH);

            if (ContainsBlockedWord(text))
            {
                Debug.LogWarning("[PlayerTitleService] Custom title contains blocked word");
                return false;
            }

            _customTitleText = text.Trim();
            PlayerPrefs.SetString(CUSTOM_TITLE_KEY, _customTitleText);
            PlayerPrefs.Save();

            // Sync to Firebase
            SyncTitlesToFirebase().ContinueWith(t =>
            {
                if (t.IsFaulted)
                    Debug.LogWarning($"[PlayerTitleService] Firebase sync failed: {t.Exception?.GetBaseException().Message}");
            }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);

            Debug.Log($"[PlayerTitleService] Custom title set: {_customTitleText}");

            if (string.Equals(_equippedTitleId, "custom_title", StringComparison.OrdinalIgnoreCase))
                OnTitleChanged?.Invoke(_equippedTitleId);

            return true;
        }

        /// <summary>
        /// Attempts to purchase a title with the appropriate currency.
        /// </summary>
        public bool TryPurchaseTitle(string titleId)
        {
            if (IsOwned(titleId)) return false;

            var title = GetTitleData(titleId);
            if (title == null) return false;

            var currency = DigitPark.Monetization.CurrencyManager.Instance;
            if (currency == null) return false;

            switch (title.priceType)
            {
                case TitlePriceType.Free:
                    break;
                case TitlePriceType.DigitCoins:
                    if (!currency.SpendCoins(title.coinPrice)) return false;
                    break;
                case TitlePriceType.DigitGems:
                    if (!currency.SpendGems(title.gemPrice)) return false;
                    break;
                case TitlePriceType.RealMoney:
                    Debug.LogWarning("[PlayerTitleService] Real money titles should use IAP system");
                    return false;
                case TitlePriceType.Achievement:
                case TitlePriceType.Secret:
                    if (!string.IsNullOrEmpty(title.achievementId))
                    {
                        if (AchievementService.Instance == null || !AchievementService.Instance.IsUnlocked(title.achievementId))
                            return false;
                    }
                    break;
            }

            UnlockTitle(titleId);
            return true;
        }

        // ==================== WORD FILTER ====================

        private static bool ContainsBlockedWord(string text)
        {
            string lower = text.ToLower();
            foreach (var word in BLOCKED_WORDS)
            {
                if (lower.Contains(word))
                    return true;
            }
            return false;
        }

        // ==================== DEBUG ====================

#if UNITY_EDITOR
        [ContextMenu("Debug: Unlock All Titles")]
        private void DebugUnlockAll()
        {
            foreach (var title in _allTitles)
                UnlockTitle(title.titleId);
        }

        [ContextMenu("Debug: Reset Titles")]
        private void DebugReset()
        {
            _ownedTitles.Clear();
            _ownedTitles.Add(DEFAULT_TITLE_ID);
            _equippedTitleId = DEFAULT_TITLE_ID;
            _customTitleText = "";
            SaveOwnedTitles();
            SaveEquippedTitle();
            PlayerPrefs.DeleteKey(CUSTOM_TITLE_KEY);
            PlayerPrefs.Save();
            Debug.Log("[PlayerTitleService] Reset to defaults");
        }
#endif
    }
}
