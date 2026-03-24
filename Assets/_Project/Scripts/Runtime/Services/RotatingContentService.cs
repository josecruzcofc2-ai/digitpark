using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;
using DigitPark.Services.Firebase;
using FirebaseDB = global::Firebase.Database;

namespace DigitPark.Services
{
    /// <summary>
    /// Economy Rebalance V55 — Rotating Content Service (Whale Ceiling Expansion)
    ///
    /// Manages 3 types of time-limited content to expand whale ceiling from ~$208 to $500-1,000/year:
    ///
    /// 1. SEASONAL BATTLE CARDS (quarterly, 800-1,500 DG each)
    ///    - 4 per year = 3,200-6,000 DG extra/year
    ///    - Fresh content that renews whale interest every 3 months
    ///    - Each card has unique DOTween animations not available in permanent catalog
    ///
    /// 2. MONTHLY EXCLUSIVE IAP FRAMES ($4.99-$9.99 each)
    ///    - 12 per year = $60-120 extra revenue
    ///    - Animated frame available only during 1 calendar month
    ///    - FOMO: "January 2026 Exclusive" — never returns
    ///
    /// 3. LIMITED-TIME THEME VARIANTS (600 DG, 2-week windows)
    ///    - Palette-modified versions of existing themes (e.g. "Aurora Borealis: Midnight")
    ///    - Reutilizes existing theme assets with color swap = high ROI, low production
    ///    - 2-week availability creates urgency
    ///
    /// Architecture: Content is defined as RotatingContentItem entries with start/end dates.
    /// The service checks availability on init and exposes currently-active items.
    /// New content is added by appending to the catalog — no code changes needed per item.
    ///
    /// POST-LAUNCH: This service is designed now but content entries are added post-launch.
    /// The Shop UI queries ActiveItems to show a "LIMITED" section when content exists.
    /// </summary>
    public class RotatingContentService : MonoBehaviour
    {
        private static RotatingContentService _instance;
        public static RotatingContentService Instance => _instance;

        // ==================== CATALOG ====================
        // New items are added here post-launch. No other code changes needed.

        private List<RotatingContentItem> _catalog = new List<RotatingContentItem>();
        private List<RotatingContentItem> _activeItems = new List<RotatingContentItem>();
        private HashSet<string> _purchasedIds = new HashSet<string>();

        private const string PURCHASED_KEY = "RotatingContent_Purchased";
        private bool _initialized;

        // ==================== EVENTS ====================

        public event Action OnContentRefreshed;
        public event Action<RotatingContentItem> OnItemPurchased;

        // ==================== PUBLIC API ====================

        public List<RotatingContentItem> ActiveItems => _activeItems;
        public bool HasActiveContent => _activeItems.Count > 0;
        public bool IsInitialized => _initialized;

        public bool IsOwned(string itemId) => _purchasedIds.Contains(itemId);

        /// <summary>Get active items by type</summary>
        public List<RotatingContentItem> GetActiveByType(RotatingContentType type)
        {
            return _activeItems.FindAll(i => i.contentType == type);
        }

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
            InitializeCatalog();
            LoadPurchased();
            RefreshActiveItems();
            _initialized = true;
            Debug.Log($"[RotatingContent] Initialized — {_activeItems.Count} active items (local), {_purchasedIds.Count} owned");

            // Load catalog from Firebase RTDB — overrides local if entries exist
            _ = LoadCatalogFromFirebase();

            // B-78: Restore purchased IDs from Firebase after local load
            _ = RestoreFromFirebase();
        }

        // ==================== CATALOG SETUP ====================

        /// <summary>
        /// Initialize the content catalog. Post-launch, new items are added here.
        /// Each item has explicit start/end dates — no code changes needed elsewhere.
        /// </summary>
        private void InitializeCatalog()
        {
            // ── EXAMPLE ENTRIES (uncomment and adjust dates for launch) ──

            // Season 1 BattleCard — Q2 2026
            // _catalog.Add(new RotatingContentItem
            // {
            //     itemId = "seasonal_card_s1_phoenix_rising",
            //     displayNameKey = "card_phoenix_rising",
            //     displayNameFallback = "Phoenix Rising",
            //     contentType = RotatingContentType.SeasonalBattleCard,
            //     currency = RotatingCurrency.DG,
            //     price = 1000,
            //     startDate = "2026-04-01",
            //     endDate = "2026-06-30",
            //     seasonLabel = "Season 1",
            //     description = "Animated phoenix border with flame trail"
            // });

            // Monthly Frame — January 2026
            // _catalog.Add(new RotatingContentItem
            // {
            //     itemId = "monthly_frame_jan2026_frost_crown",
            //     displayNameKey = "frame_frost_crown",
            //     displayNameFallback = "Frost Crown",
            //     contentType = RotatingContentType.MonthlyExclusiveFrame,
            //     currency = RotatingCurrency.IAP,
            //     iapPrice = 4.99f,
            //     iapProductId = "com.digitpark.frame_jan2026",
            //     startDate = "2026-01-01",
            //     endDate = "2026-01-31",
            //     seasonLabel = "January 2026 Exclusive",
            //     description = "Animated ice crystal frame — never returns"
            // });

            // Limited Theme Variant — 2-week window
            // _catalog.Add(new RotatingContentItem
            // {
            //     itemId = "variant_aurora_midnight",
            //     displayNameKey = "theme_aurora_midnight",
            //     displayNameFallback = "Aurora Borealis: Midnight",
            //     contentType = RotatingContentType.LimitedThemeVariant,
            //     currency = RotatingCurrency.DG,
            //     price = 600,
            //     startDate = "2026-02-14",
            //     endDate = "2026-02-28",
            //     baseThemeId = "theme_aurora_borealis",
            //     seasonLabel = "Limited Edition",
            //     description = "Deep midnight palette of Aurora Borealis"
            // });

            Debug.Log($"[RotatingContent] Catalog initialized with {_catalog.Count} entries");
        }

        // ==================== FIREBASE CATALOG ====================

        /// <summary>
        /// Loads the rotating content catalog from Firebase RTDB path: rotating_content/catalog/{itemId}
        /// Each child node maps 1:1 to RotatingContentItem fields (string/int/float).
        /// Overrides the local catalog if Firebase has entries.
        /// No app update needed to add new seasonal items — manage from Firebase Console.
        /// </summary>
        private async Task LoadCatalogFromFirebase()
        {
            try
            {
                var db = FirebaseDB.FirebaseDatabase.DefaultInstance;
                var snapshot = await db.GetReference("rotating_content/catalog").GetValueAsync();

                if (!snapshot.Exists || snapshot.ChildrenCount == 0)
                {
                    Debug.Log("[RotatingContent] No catalog in Firebase — using local.");
                    return;
                }

                _catalog.Clear();
                foreach (var child in snapshot.Children)
                {
                    try
                    {
                        var item = ParseItemFromSnapshot(child);
                        if (item != null && !string.IsNullOrEmpty(item.itemId))
                            _catalog.Add(item);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[RotatingContent] Parse error for {child.Key}: {ex.Message}");
                    }
                }

                RefreshActiveItems();
                Debug.Log($"[RotatingContent] Firebase catalog loaded — {_catalog.Count} items, {_activeItems.Count} active");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RotatingContent] Firebase catalog unavailable, using local: {ex.Message}");
            }
        }

        private RotatingContentItem ParseItemFromSnapshot(FirebaseDB.DataSnapshot snap)
        {
            var dict = snap.Value as Dictionary<string, object>;
            if (dict == null) return null;

            string Str(string k) => dict.TryGetValue(k, out var v) ? v?.ToString() ?? "" : "";
            int Int(string k) => dict.TryGetValue(k, out var v) && int.TryParse(v?.ToString(), out int i) ? i : 0;
            float Flt(string k) => dict.TryGetValue(k, out var v) &&
                float.TryParse(v?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out float f) ? f : 0f;

            return new RotatingContentItem
            {
                itemId             = Str("itemId"),
                displayNameKey     = Str("displayNameKey"),
                displayNameFallback = Str("displayNameFallback"),
                contentType        = Enum.TryParse<RotatingContentType>(Str("contentType"), out var ct) ? ct : RotatingContentType.LimitedThemeVariant,
                currency           = Enum.TryParse<RotatingCurrency>(Str("currency"), out var cur) ? cur : RotatingCurrency.DG,
                price              = Int("price"),
                iapPrice           = Flt("iapPrice"),
                iapProductId       = Str("iapProductId"),
                startDate          = Str("startDate"),
                endDate            = Str("endDate"),
                seasonLabel        = Str("seasonLabel"),
                baseThemeId        = Str("baseThemeId"),
                description        = Str("description"),
            };
        }

        // ==================== ACTIVE ITEMS ====================

        /// <summary>Filter catalog to currently-active items based on UTC date</summary>
        private void RefreshActiveItems()
        {
            _activeItems.Clear();
            DateTime now = ServerTimeHelper.UtcNow;

            foreach (var item in _catalog)
            {
                if (!DateTime.TryParse(item.startDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime start))
                    continue;
                if (!DateTime.TryParse(item.endDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime end))
                    continue;

                // End date is inclusive (entire last day)
                end = end.Date.AddDays(1).AddTicks(-1);

                if (now >= start && now <= end)
                {
                    _activeItems.Add(item);
                }
            }

            OnContentRefreshed?.Invoke();
        }

        /// <summary>Time remaining for a specific item</summary>
        public TimeSpan GetTimeRemaining(RotatingContentItem item)
        {
            if (!DateTime.TryParse(item.endDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime end))
                return TimeSpan.Zero;

            end = end.Date.AddDays(1); // Midnight after end day
            var remaining = end - ServerTimeHelper.UtcNow;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        // ==================== PURCHASE ====================

        /// <summary>
        /// Purchase a rotating content item (DG currency).
        /// For IAP items, call MarkPurchased after IAP confirmation.
        /// </summary>
        public bool PurchaseDG(string itemId)
        {
            var item = _activeItems.Find(i => i.itemId == itemId);
            if (item == null || _purchasedIds.Contains(itemId)) return false;
            if (item.currency != RotatingCurrency.DG) return false;

            var currency = DigitPark.Monetization.CurrencyManager.Instance;
            if (currency == null || !currency.SpendGems(item.price)) return false;

            MarkPurchased(itemId);
            return true;
        }

        /// <summary>Mark item as purchased (called after IAP confirmation for IAP items)</summary>
        public void MarkPurchased(string itemId)
        {
            if (_purchasedIds.Contains(itemId)) return;
            _purchasedIds.Add(itemId);
            SavePurchased();

            var item = _catalog.Find(i => i.itemId == itemId);

            // Grant the content
            if (item != null)
                GrantContent(item);

            AnalyticsService.Instance?.LogCustomEvent("rotating_content_purchased", new Dictionary<string, object>
            {
                { "item_id", itemId },
                { "content_type", item?.contentType.ToString() ?? "unknown" },
                { "price", item?.price ?? 0 },
                { "currency", item?.currency.ToString() ?? "unknown" }
            });

            OnItemPurchased?.Invoke(item);
            Debug.Log($"[RotatingContent] Purchased: {itemId}");
        }

        private void GrantContent(RotatingContentItem item)
        {
            switch (item.contentType)
            {
                case RotatingContentType.SeasonalBattleCard:
                    // Unlock card in BattleCardService
                    var cardService = DigitPark.Cosmetics.BattleCardService.Instance;
                    if (cardService != null)
                    {
                        // BattleCardService.UnlockCard(item.itemId) — wire when method exists
                        PlayerPrefs.SetInt($"Card_Owned_{item.itemId}", 1);
                    }
                    break;

                case RotatingContentType.MonthlyExclusiveFrame:
                    PlayerPrefs.SetInt($"Frame_Owned_{item.itemId}", 1);
                    break;

                case RotatingContentType.LimitedThemeVariant:
                    PlayerPrefs.SetInt($"OwnedTheme_{item.itemId}", 1);
                    break;
            }
            PlayerPrefs.Save();
        }

        // ==================== PERSISTENCE ====================

        private void LoadPurchased()
        {
            string json = PlayerPrefs.GetString(PURCHASED_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var data = JsonUtility.FromJson<StringListWrapper>(json);
                    if (data?.items != null)
                        foreach (var id in data.items)
                            _purchasedIds.Add(id);
                }
                catch (System.Exception e) { Debug.LogWarning($"[RotatingContentService] JSON parse error: {e.Message}"); }
            }
        }

        private void SavePurchased()
        {
            var data = new StringListWrapper { items = new List<string>(_purchasedIds) };
            PlayerPrefs.SetString(PURCHASED_KEY, JsonUtility.ToJson(data));
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
                    { "rotatingContent/purchased", JsonUtility.ToJson(new StringListWrapper { items = new List<string>(_purchasedIds) }) }
                };
                await DatabaseService.Instance.UpdatePlayerFields(playerData.userId, updates);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[RotatingContent] Firebase sync failed: {e.Message}");
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
                if (data == null || !data.ContainsKey("rotatingContent")) return;

                var rcData = data["rotatingContent"] as Dictionary<string, object>;
                if (rcData == null || !rcData.ContainsKey("purchased")) return;

                string fbJson = rcData["purchased"]?.ToString();
                if (string.IsNullOrEmpty(fbJson)) return;

                var fbData = JsonUtility.FromJson<StringListWrapper>(fbJson);
                if (fbData?.items == null) return;

                bool changed = false;
                foreach (var id in fbData.items)
                {
                    if (!string.IsNullOrEmpty(id) && _purchasedIds.Add(id))
                        changed = true;
                }

                if (changed)
                {
                    // Save merged set back to PlayerPrefs
                    var merged = new StringListWrapper { items = new List<string>(_purchasedIds) };
                    PlayerPrefs.SetString(PURCHASED_KEY, JsonUtility.ToJson(merged));
                    PlayerPrefs.Save();
                    RefreshActiveItems();
                    Debug.Log($"[RotatingContent] Firebase restore merged — {_purchasedIds.Count} purchased IDs");
                }
                else
                {
                    Debug.Log("[RotatingContent] Firebase restore: no changes needed");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[RotatingContent] RestoreFromFirebase failed: {e.Message}");
            }
        }

        [Serializable]
        private class StringListWrapper
        {
            public List<string> items = new List<string>();
        }
    }

    // ==================== DATA STRUCTURES ====================

    public enum RotatingContentType
    {
        SeasonalBattleCard,      // Quarterly, 800-1,500 DG
        MonthlyExclusiveFrame,   // Monthly, $4.99-$9.99 IAP
        LimitedThemeVariant      // 2-week window, 600 DG
    }

    public enum RotatingCurrency
    {
        DG,   // DigitGems
        IAP   // Real money (Apple/Google IAP)
    }

    /// <summary>
    /// A single rotating content item. Add new entries to the catalog in InitializeCatalog().
    /// No other code changes needed — the Shop UI queries ActiveItems automatically.
    /// </summary>
    [Serializable]
    public class RotatingContentItem
    {
        public string itemId;
        public string displayNameKey;
        public string displayNameFallback;
        public RotatingContentType contentType;
        public RotatingCurrency currency;
        public int price;              // DG price (for DG items)
        public float iapPrice;         // USD price (for IAP items)
        public string iapProductId;    // Apple/Google product ID
        public string startDate;       // "yyyy-MM-dd" UTC
        public string endDate;         // "yyyy-MM-dd" UTC (inclusive)
        public string seasonLabel;     // e.g. "Season 1", "January 2026", "Limited Edition"
        public string baseThemeId;     // For variants: the parent theme ID
        public string description;
    }
}
