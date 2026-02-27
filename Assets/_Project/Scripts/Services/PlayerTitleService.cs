using System;
using System.Collections.Generic;
using UnityEngine;

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
    /// ~20 titulos predefinidos + 1 titulo custom (IAP).
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
        private const string DEFAULT_TITLE_ID = "novato";
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
            // ==================== COINS (5 titles) ====================
            _allTitles.Add(new TitleData
            {
                titleId = "novato",
                nameKey = "title_novato",
                priceType = TitlePriceType.Free,
                coinPrice = 0
            });

            _allTitles.Add(new TitleData
            {
                titleId = "jugador",
                nameKey = "title_jugador",
                priceType = TitlePriceType.DigitCoins,
                coinPrice = 500
            });

            _allTitles.Add(new TitleData
            {
                titleId = "veterano",
                nameKey = "title_veterano",
                priceType = TitlePriceType.DigitCoins,
                coinPrice = 3000
            });

            _allTitles.Add(new TitleData
            {
                titleId = "leyenda",
                nameKey = "title_leyenda",
                priceType = TitlePriceType.DigitCoins,
                coinPrice = 10000
            });

            _allTitles.Add(new TitleData
            {
                titleId = "inmortal",
                nameKey = "title_inmortal",
                priceType = TitlePriceType.DigitCoins,
                coinPrice = 25000
            });

            // ==================== GEMS (4 titles) ====================
            _allTitles.Add(new TitleData
            {
                titleId = "estratega",
                nameKey = "title_estratega",
                priceType = TitlePriceType.DigitGems,
                gemPrice = 100
            });

            _allTitles.Add(new TitleData
            {
                titleId = "genio",
                nameKey = "title_genio",
                priceType = TitlePriceType.DigitGems,
                gemPrice = 300
            });

            _allTitles.Add(new TitleData
            {
                titleId = "maestro",
                nameKey = "title_maestro",
                priceType = TitlePriceType.DigitGems,
                gemPrice = 600
            });

            _allTitles.Add(new TitleData
            {
                titleId = "iluminado",
                nameKey = "title_iluminado",
                priceType = TitlePriceType.DigitGems,
                gemPrice = 1000
            });

            // ==================== ACHIEVEMENT (6 titles) ====================
            _allTitles.Add(new TitleData
            {
                titleId = "primer_paso",
                nameKey = "title_primer_paso",
                priceType = TitlePriceType.Achievement,
                achievementId = "first_game"
            });

            _allTitles.Add(new TitleData
            {
                titleId = "imparable",
                nameKey = "title_imparable",
                priceType = TitlePriceType.Achievement,
                achievementId = "streak_10"
            });

            _allTitles.Add(new TitleData
            {
                titleId = "madrugador",
                nameKey = "title_madrugador",
                priceType = TitlePriceType.Achievement,
                achievementId = "night_owl"
            });

            _allTitles.Add(new TitleData
            {
                titleId = "perfeccionista",
                nameKey = "title_perfeccionista",
                priceType = TitlePriceType.Achievement,
                achievementId = "perfect_game"
            });

            _allTitles.Add(new TitleData
            {
                titleId = "campeon",
                nameKey = "title_campeon",
                priceType = TitlePriceType.Achievement,
                achievementId = "tournament_win"
            });

            _allTitles.Add(new TitleData
            {
                titleId = "coleccionista",
                nameKey = "title_coleccionista",
                priceType = TitlePriceType.Achievement,
                achievementId = "days_365"  // Placeholder for "all themes" - maps to dedication
            });

            // ==================== REAL MONEY (1 title) ====================
            _allTitles.Add(new TitleData
            {
                titleId = "custom_title",
                nameKey = "title_custom",
                priceType = TitlePriceType.RealMoney,
                realMoneyPrice = 1.99f,
                isCustom = true
            });

            // ==================== SECRET (4 titles) ====================
            _allTitles.Add(new TitleData
            {
                titleId = "fantasma",
                nameKey = "title_fantasma",
                priceType = TitlePriceType.Secret,
                achievementId = "night_owl"  // play 100 games at night (reuses achievement)
            });

            _allTitles.Add(new TitleData
            {
                titleId = "velocista",
                nameKey = "title_velocista",
                priceType = TitlePriceType.Secret,
                achievementId = "speed_demon"
            });

            _allTitles.Add(new TitleData
            {
                titleId = "rey_comeback",
                nameKey = "title_rey_comeback",
                priceType = TitlePriceType.Secret,
                achievementId = "comeback_king"
            });

            _allTitles.Add(new TitleData
            {
                titleId = "completo",
                nameKey = "title_completo",
                priceType = TitlePriceType.Secret,
                achievementId = "level_100"  // Placeholder for 100% achievements
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
        }

        private void SaveEquippedTitle()
        {
            PlayerPrefs.SetString(EQUIPPED_TITLE_KEY, _equippedTitleId);
            PlayerPrefs.Save();
        }

        // ==================== PUBLIC API ====================

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

            Debug.Log($"[PlayerTitleService] Custom title set: {_customTitleText}");

            if (_equippedTitleId == "custom_title")
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
