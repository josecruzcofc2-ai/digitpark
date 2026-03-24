using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;
using DigitPark.Services;
using DigitPark.Services.Firebase;
using FirebaseDB = global::Firebase.Database;

namespace DigitPark.Monetization
{
    /// <summary>
    /// Economy Rebalance V55 — Welcome Pack System
    ///
    /// 2 packs for new players, designed as first-conversion funnels:
    ///
    /// Pack 1 — "Starter Pack" ($2.99, visible D1-D3)
    ///   Contents: 1 Tier A theme (random) + Ruby frame + Estratega title + 200 DG
    ///   Value: 700 DG perceived → 57% off at $2.99 (~300 DG equiv)
    ///   Target: Minnow conversion — $2.99 is below impulse threshold
    ///   Reappears D7 as "LAST CHANCE" if not purchased
    ///
    /// Pack 2 — "Premium Welcome" ($9.99, visible D1-D5)
    ///   Contents: Aurora Borealis (Tier S) + Holographic frame + Gold Rain effect + 500 DG
    ///   Value: ~1,350 DG perceived → 26% off real, ~50% off perceived
    ///   Target: Dolphin conversion — gets the best theme + effect + DG on day 1
    ///   Does NOT reappear — real exclusivity creates urgency
    ///
    /// Timer: Persistido via PlayerPrefs (first_login_date).
    /// Posición: PRIMER item del Shop (antes de todo).
    /// </summary>
    public class WelcomePackService : MonoBehaviour
    {
        private static WelcomePackService _instance;
        public static WelcomePackService Instance => _instance;

        // ==================== PACK DEFINITIONS ====================

        public static readonly WelcomePackDef StarterPack = new WelcomePackDef
        {
            packId = "starter_pack_v55",
            displayNameKey = "shop_starter_pack",
            iapProductId = "com.digitpark.starter_pack",
            priceUSD = 2.99f,
            perceivedValueDG = 700,
            discountLabel = "57% OFF",
            contentsKey = "shop_starter_pack_contents",
            contentsFallback = "1 Theme + Ruby Frame + Title + 200 DG",
            visibleFromDay = 1,
            visibleUntilDay = 3,
            reappearDay = 7,      // "LAST CHANCE" on D7
            reappearLabel = "LAST CHANCE",
            // Actual contents granted on purchase:
            grantDG = 200,
            grantThemeId = "", // Random Tier A — resolved at purchase time
            grantFrameId = "frame_ruby",
            grantTitleId = "mastermind",
            grantEffectId = "",
        };

        public static readonly WelcomePackDef PremiumWelcome = new WelcomePackDef
        {
            packId = "premium_welcome_v55",
            displayNameKey = "shop_premium_welcome",
            iapProductId = "com.digitpark.premium_welcome",
            priceUSD = 9.99f,
            perceivedValueDG = 1350,
            discountLabel = "50% OFF",
            contentsKey = "shop_premium_welcome_contents",
            contentsFallback = "Aurora Borealis + Holographic + Gold Rain + 500 DG",
            visibleFromDay = 1,
            visibleUntilDay = 5,
            reappearDay = -1,     // Does NOT reappear — exclusivity
            reappearLabel = "",
            grantDG = 500,
            grantThemeId = "theme_aurora_borealis",
            grantFrameId = "frame_holographic",
            grantTitleId = "",
            grantEffectId = "gold_rain",
        };

        // ==================== STATE ====================

        private const string FIRST_LOGIN_KEY = "WelcomePack_FirstLoginDate";
        private const string STARTER_PURCHASED_KEY = "WelcomePack_StarterPurchased";
        private const string PREMIUM_PURCHASED_KEY = "WelcomePack_PremiumPurchased";

        private DateTime _firstLoginDate;
        private bool _starterPurchased;
        private bool _premiumPurchased;
        private bool _initialized;

        // ==================== EVENTS ====================

        public event Action<WelcomePackDef> OnPackPurchased;
        public event Action OnVisibilityChanged;

        // ==================== PUBLIC API ====================

        public bool IsInitialized => _initialized;

        /// <summary>Days since first login (1-based: D1 = first day)</summary>
        public int DaysSinceFirstLogin
        {
            get
            {
                // B3-F: Use ServerTimeHelper to prevent client clock manipulation
                int days = (ServerTimeHelper.UtcNow.Date - _firstLoginDate.Date).Days + 1;
                return Mathf.Max(1, days);
            }
        }

        /// <summary>Is the Starter Pack visible right now?</summary>
        public bool IsStarterVisible
        {
            get
            {
                if (_starterPurchased) return false;
                int day = DaysSinceFirstLogin;
                // Visible D1-D3 or reappear on D7
                return (day >= StarterPack.visibleFromDay && day <= StarterPack.visibleUntilDay)
                    || (day == StarterPack.reappearDay);
            }
        }

        /// <summary>Is the Premium Welcome visible right now?</summary>
        public bool IsPremiumVisible
        {
            get
            {
                if (_premiumPurchased) return false;
                int day = DaysSinceFirstLogin;
                return day >= PremiumWelcome.visibleFromDay && day <= PremiumWelcome.visibleUntilDay;
            }
        }

        /// <summary>Is "LAST CHANCE" badge showing for Starter?</summary>
        public bool IsStarterLastChance => !_starterPurchased && DaysSinceFirstLogin == StarterPack.reappearDay;

        /// <summary>Time remaining for Starter Pack visibility (until end of D3 or D7)</summary>
        public TimeSpan StarterTimeRemaining
        {
            get
            {
                int day = DaysSinceFirstLogin;
                int endDay = (day == StarterPack.reappearDay) ? StarterPack.reappearDay : StarterPack.visibleUntilDay;
                DateTime expiry = _firstLoginDate.Date.AddDays(endDay); // midnight after end day
                var remaining = expiry - DateTime.UtcNow;
                return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
            }
        }

        /// <summary>Time remaining for Premium Welcome visibility (until end of D5)</summary>
        public TimeSpan PremiumTimeRemaining
        {
            get
            {
                DateTime expiry = _firstLoginDate.Date.AddDays(PremiumWelcome.visibleUntilDay);
                var remaining = expiry - DateTime.UtcNow;
                return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
            }
        }

        public bool StarterPurchased => _starterPurchased;
        public bool PremiumPurchased => _premiumPurchased;

        // ==================== LIFECYCLE ====================

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
                return;
            }
        }

        private void Start()
        {
            LoadState();
            _initialized = true;
            Debug.Log($"[WelcomePackService] Init — D{DaysSinceFirstLogin}, Starter:{(IsStarterVisible ? "VISIBLE" : "hidden")}, Premium:{(IsPremiumVisible ? "VISIBLE" : "hidden")}");

            // B-66: Restore from Firebase after local load (merge/override if Firebase has data)
            _ = RestoreFromFirebase();
        }

        // ==================== PERSISTENCE ====================

        private void LoadState()
        {
            // First login date
            string dateStr = PlayerPrefs.GetString(FIRST_LOGIN_KEY, "");
            if (string.IsNullOrEmpty(dateStr))
            {
                // First ever login — record now
                _firstLoginDate = DateTime.UtcNow.Date;
                PlayerPrefs.SetString(FIRST_LOGIN_KEY, _firstLoginDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                PlayerPrefs.Save();
                Debug.Log("[WelcomePackService] First login recorded");
            }
            else
            {
                if (!DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out _firstLoginDate))
                    _firstLoginDate = DateTime.UtcNow.Date;
            }

            _starterPurchased = PlayerPrefs.GetInt(STARTER_PURCHASED_KEY, 0) == 1;
            _premiumPurchased = PlayerPrefs.GetInt(PREMIUM_PURCHASED_KEY, 0) == 1;
        }

        private void SaveState()
        {
            PlayerPrefs.SetInt(STARTER_PURCHASED_KEY, _starterPurchased ? 1 : 0);
            PlayerPrefs.SetInt(PREMIUM_PURCHASED_KEY, _premiumPurchased ? 1 : 0);
            PlayerPrefs.Save();
            _ = SyncToFirebase();
        }

        private async Task SyncToFirebase()
        {
            var auth = AuthenticationService.Instance;
            if (auth == null || !auth.IsUserAuthenticated()) return;
            var playerData = auth.GetCurrentPlayerData();
            if (playerData == null || DatabaseService.Instance == null) return;

            try
            {
                var updates = new Dictionary<string, object>
                {
                    { "welcomePacks/firstLogin", _firstLoginDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) },
                    { "welcomePacks/starterPurchased", _starterPurchased },
                    { "welcomePacks/premiumPurchased", _premiumPurchased }
                };
                await DatabaseService.Instance.UpdatePlayerFields(playerData.userId, updates);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[WelcomePackService] Firebase sync failed: {e.Message}");
            }
        }

        private async Task RestoreFromFirebase()
        {
            try
            {
                var auth = AuthenticationService.Instance;
                if (auth == null || !auth.IsUserAuthenticated()) return;

                string uid = auth.GetCurrentUserId();
                if (string.IsNullOrEmpty(uid)) return;

                var dbRef = FirebaseDB.FirebaseDatabase.DefaultInstance?.RootReference;
                if (dbRef == null) return;

                var snapshot = await dbRef.Child("players").Child(uid).GetValueAsync();
                if (snapshot == null || !snapshot.Exists) return;

                var data = snapshot.Value as Dictionary<string, object>;
                if (data == null || !data.ContainsKey("welcomePacks")) return;

                var wpData = data["welcomePacks"] as Dictionary<string, object>;
                if (wpData == null) return;

                bool changed = false;

                // Merge: Firebase purchased flags override local (if true in Firebase, mark true locally)
                if (wpData.ContainsKey("starterPurchased") && !_starterPurchased)
                {
                    bool fbStarter = TryParseBool(wpData["starterPurchased"]);
                    if (fbStarter)
                    {
                        _starterPurchased = true;
                        changed = true;
                    }
                }

                if (wpData.ContainsKey("premiumPurchased") && !_premiumPurchased)
                {
                    bool fbPremium = TryParseBool(wpData["premiumPurchased"]);
                    if (fbPremium)
                    {
                        _premiumPurchased = true;
                        changed = true;
                    }
                }

                // Restore firstLogin from Firebase if local is newer (reinstall scenario)
                if (wpData.ContainsKey("firstLogin"))
                {
                    string fbDate = wpData["firstLogin"]?.ToString();
                    if (!string.IsNullOrEmpty(fbDate) &&
                        DateTime.TryParse(fbDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fbFirstLogin))
                    {
                        if (fbFirstLogin < _firstLoginDate)
                        {
                            _firstLoginDate = fbFirstLogin;
                            PlayerPrefs.SetString(FIRST_LOGIN_KEY, _firstLoginDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                            changed = true;
                        }
                    }
                }

                if (changed)
                {
                    PlayerPrefs.SetInt(STARTER_PURCHASED_KEY, _starterPurchased ? 1 : 0);
                    PlayerPrefs.SetInt(PREMIUM_PURCHASED_KEY, _premiumPurchased ? 1 : 0);
                    PlayerPrefs.Save();
                    OnVisibilityChanged?.Invoke();
                    Debug.Log($"[WelcomePackService] Firebase restore merged — starter:{_starterPurchased}, premium:{_premiumPurchased}");
                }
                else
                {
                    Debug.Log("[WelcomePackService] Firebase restore: no changes needed");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[WelcomePackService] RestoreFromFirebase failed: {e.Message}");
            }
        }

        // ==================== PURCHASE ====================

        /// <summary>
        /// Called after IAP payment confirmed. Grants pack contents.
        /// </summary>
        public void OnStarterPackPurchased(string verifiedTransactionId = "") // B2-E: solo PaymentManager debe llamar esto con un txId válido
        {
            if (string.IsNullOrEmpty(verifiedTransactionId))
            {
                Debug.LogWarning("[WelcomePack] OnStarterPackPurchased ignorado — se requiere verifiedTransactionId");
                return;
            }
            if (_starterPurchased) return;
            _starterPurchased = true;

            GrantPackContents(StarterPack);
            SaveState();

            AnalyticsService.Instance?.LogCustomEvent("welcome_pack_purchased", new Dictionary<string, object>
            {
                { "pack_id", StarterPack.packId },
                { "price_usd", StarterPack.priceUSD },
                { "day", DaysSinceFirstLogin },
                { "is_last_chance", IsStarterLastChance }
            });

            OnPackPurchased?.Invoke(StarterPack);
            OnVisibilityChanged?.Invoke();
            Debug.Log($"[WelcomePackService] Starter Pack purchased on D{DaysSinceFirstLogin}");
        }

        public void OnPremiumWelcomePurchased()
        {
            if (_premiumPurchased) return;
            _premiumPurchased = true;

            GrantPackContents(PremiumWelcome);
            SaveState();

            AnalyticsService.Instance?.LogCustomEvent("welcome_pack_purchased", new Dictionary<string, object>
            {
                { "pack_id", PremiumWelcome.packId },
                { "price_usd", PremiumWelcome.priceUSD },
                { "day", DaysSinceFirstLogin }
            });

            OnPackPurchased?.Invoke(PremiumWelcome);
            OnVisibilityChanged?.Invoke();
            Debug.Log($"[WelcomePackService] Premium Welcome purchased on D{DaysSinceFirstLogin}");
        }

        private void GrantPackContents(WelcomePackDef pack)
        {
            var currency = CurrencyManager.Instance;

            // Grant DG
            if (pack.grantDG > 0 && currency != null)
            {
                currency.AddGems(pack.grantDG);
                AnalyticsService.Instance?.LogVirtualCurrencyEarned("digitgems", pack.grantDG, pack.packId);
            }

            if (!string.IsNullOrEmpty(pack.grantThemeId))
            {
                Debug.Log($"[WelcomePackService] Granted theme: {pack.grantThemeId}");
            }
            else if (pack.packId == StarterPack.packId)
            {
                // Random Tier A theme for Starter Pack
                string[] tierAThemes = { "theme_glitch", "theme_bioluminescence", "theme_volcanic", "theme_matrix",
                    "theme_infrared", "theme_blood_moon", "theme_phantom", "theme_ultraviolet" };
                string picked = tierAThemes[UnityEngine.Random.Range(0, tierAThemes.Length)];
                Debug.Log($"[WelcomePackService] Granted random Tier A theme: {picked}");
            }

            // Frame — B3-E: use PlayerFrameService
            if (!string.IsNullOrEmpty(pack.grantFrameId))
            {
                PlayerFrameService.Instance?.UnlockFrame(pack.grantFrameId);
                Debug.Log($"[WelcomePackService] Granted frame: {pack.grantFrameId}");
            }

            // Title — B3-E: use PlayerTitleService
            if (!string.IsNullOrEmpty(pack.grantTitleId))
            {
                PlayerTitleService.Instance?.UnlockTitle(pack.grantTitleId);
                Debug.Log($"[WelcomePackService] Granted title: {pack.grantTitleId}");
            }

            // Effect
            if (!string.IsNullOrEmpty(pack.grantEffectId))
            {
                VictoryEffectService.Instance?.UnlockEffect(pack.grantEffectId);
                Debug.Log($"[WelcomePackService] Granted effect: {pack.grantEffectId}");
            }

            PlayerPrefs.Save();
        }

        /// <summary>
        /// Safely parse a Firebase value as bool. Handles bool, long (0/1), and string ("true"/"false").
        /// </summary>
        private static bool TryParseBool(object value)
        {
            if (value is bool b) return b;
            if (value is long l) return l != 0;
            if (value is int i) return i != 0;
            if (value is string s) return s.Equals("true", System.StringComparison.OrdinalIgnoreCase);
            try { return System.Convert.ToBoolean(value); } catch { return false; }
        }
    }

    // ==================== DATA ====================

    [Serializable]
    public class WelcomePackDef
    {
        public string packId;
        public string displayNameKey;
        public string iapProductId;
        public float priceUSD;
        public int perceivedValueDG;
        public string discountLabel;
        public string contentsKey;
        public string contentsFallback;
        public int visibleFromDay;
        public int visibleUntilDay;
        public int reappearDay;
        public string reappearLabel;
        // Grant contents
        public int grantDG;
        public string grantThemeId;
        public string grantFrameId;
        public string grantTitleId;
        public string grantEffectId;
    }
}
