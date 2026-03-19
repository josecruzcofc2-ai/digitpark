using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;
using DigitPark.Services.Firebase;
using DigitPark.Localization;

namespace DigitPark.Services
{
    /// <summary>
    /// Economy Rebalance V55 — Daily Offer System
    ///
    /// 3 slots that rotate daily at midnight UTC:
    /// - Slot 1: Theme at 50% off DG (converts Minnow→Dolphin)
    /// - Slot 2: Frame/effect/title/card at 30-40% off (impulse buy)
    /// - Slot 3: Free DC/XP (daily login incentive for F2P)
    ///
    /// Design principles:
    /// - Seed-based on UTC date → all players see the same offers → social buzz
    /// - No repeat in Slot 1 for 7 days, Slot 2 for 5 days
    /// - Items already owned get replaced from same tier
    /// - Exclusive themes (350 DG) only appear here — artificial scarcity
    /// - Single purchase per slot per day — anti-abuse
    /// - ~2,775 DC/month from Slot 3 free rewards ≈ 14% of F2P monthly income
    /// </summary>
    public class DailyOfferService : MonoBehaviour
    {
        private static DailyOfferService _instance;
        public static DailyOfferService Instance => _instance;

        // ==================== OFFER POOLS ====================

        /// <summary>Slot 1 — Theme pool with tier weights: S=20%, A=40%, B=25%, Exclusive=15%</summary>
        private static readonly OfferItem[] ThemePool = new OfferItem[]
        {
            // Tier S (400 DG, weight 20%)
            new OfferItem("theme_aurora_borealis", "Aurora Borealis", OfferCurrency.DG, 400, OfferTier.S),
            new OfferItem("theme_nebula",          "Nebula",          OfferCurrency.DG, 400, OfferTier.S),
            new OfferItem("theme_ice_fire",        "Ice x Fire",      OfferCurrency.DG, 400, OfferTier.S),
            new OfferItem("theme_cyber_fuchsia",   "Cyber Fuchsia",   OfferCurrency.DG, 400, OfferTier.S),
            new OfferItem("theme_vaporwave",       "Vaporwave",       OfferCurrency.DG, 400, OfferTier.S),
            // Tier A (250 DG, weight 40%)
            new OfferItem("theme_glitch",          "Glitch",          OfferCurrency.DG, 250, OfferTier.A),
            new OfferItem("theme_bioluminescence", "Bioluminescence", OfferCurrency.DG, 250, OfferTier.A),
            new OfferItem("theme_volcanic",        "Volcanic",        OfferCurrency.DG, 250, OfferTier.A),
            new OfferItem("theme_matrix",          "Matrix",          OfferCurrency.DG, 250, OfferTier.A),
            new OfferItem("theme_infrared",        "Infrared",        OfferCurrency.DG, 250, OfferTier.A),
            new OfferItem("theme_blood_moon",      "Blood Moon",      OfferCurrency.DG, 250, OfferTier.A),
            new OfferItem("theme_phantom",         "Phantom",         OfferCurrency.DG, 250, OfferTier.A),
            new OfferItem("theme_ultraviolet",     "Ultraviolet",     OfferCurrency.DG, 250, OfferTier.A),
            // Tier B (150 DG, weight 25%)
            new OfferItem("theme_plasma_indigo",   "Plasma Indigo",   OfferCurrency.DG, 150, OfferTier.B),
            new OfferItem("theme_arctic",          "Arctic",          OfferCurrency.DG, 150, OfferTier.B),
            new OfferItem("theme_deep_ocean",      "Deep Ocean",      OfferCurrency.DG, 150, OfferTier.B),
            new OfferItem("theme_coral_surge",     "Coral Surge",     OfferCurrency.DG, 150, OfferTier.B),
            new OfferItem("theme_toxic_lime",      "Toxic Lime",      OfferCurrency.DG, 150, OfferTier.B),
            new OfferItem("theme_electric_orange", "Electric Orange", OfferCurrency.DG, 150, OfferTier.B),
            // Exclusive (350 DG, weight 15% — only available via Daily Offers)
            new OfferItem("theme_synthwave",       "Synthwave",       OfferCurrency.DG, 350, OfferTier.Exclusive),
            new OfferItem("theme_outrun",          "Outrun",          OfferCurrency.DG, 350, OfferTier.Exclusive),
            new OfferItem("theme_thunder",         "Thunder",         OfferCurrency.DG, 350, OfferTier.Exclusive),
            new OfferItem("theme_y2k_chrome",      "Y2K Chrome",      OfferCurrency.DG, 350, OfferTier.Exclusive),
            new OfferItem("theme_void",            "Void",            OfferCurrency.DG, 350, OfferTier.Exclusive),
        };

        /// <summary>Slot 2 — Mixed cosmetics pool (frames, effects, titles, cards)</summary>
        private static readonly OfferItem[] CosmeticsPool = new OfferItem[]
        {
            // Frames DG
            new OfferItem("frame_ruby",       "Ruby Frame",       OfferCurrency.DG, 150, OfferTier.A),
            new OfferItem("frame_emerald",    "Emerald Frame",    OfferCurrency.DG, 300, OfferTier.A),
            new OfferItem("frame_amethyst",   "Amethyst Frame",   OfferCurrency.DG, 500, OfferTier.A),
            new OfferItem("frame_topaz",      "Topaz Frame",      OfferCurrency.DG, 750, OfferTier.A),
            new OfferItem("frame_obsidian",   "Obsidian Frame",   OfferCurrency.DG, 1000, OfferTier.S),
            // Frames DC
            new OfferItem("frame_gold",       "Gold Frame",       OfferCurrency.DC, 5000, OfferTier.A),
            new OfferItem("frame_silver",     "Silver Frame",     OfferCurrency.DC, 2500, OfferTier.B),
            new OfferItem("frame_neon",       "Neon Frame",       OfferCurrency.DC, 7500, OfferTier.A),
            new OfferItem("frame_diamond",    "Diamond Frame",    OfferCurrency.DC, 10000, OfferTier.S),
            new OfferItem("frame_crystal",    "Crystal Frame",    OfferCurrency.DC, 12000, OfferTier.S),
            // Effects DG
            new OfferItem("effect_gold_rain",      "Gold Rain",      OfferCurrency.DG, 250, OfferTier.A),
            new OfferItem("effect_neon_explosion",  "Neon Explosion", OfferCurrency.DG, 400, OfferTier.A),
            // Effects DC
            new OfferItem("effect_fireworks",  "Fireworks",  OfferCurrency.DC, 2000, OfferTier.B),
            new OfferItem("effect_lightning",  "Lightning",  OfferCurrency.DC, 5000, OfferTier.A),
            // Titles DG
            new OfferItem("title_estratega", "Estratega",  OfferCurrency.DG, 100, OfferTier.B),
            new OfferItem("title_genio",     "Genio",      OfferCurrency.DG, 300, OfferTier.A),
            new OfferItem("title_maestro",   "Maestro",    OfferCurrency.DG, 600, OfferTier.A),
            new OfferItem("title_iluminado", "Iluminado",  OfferCurrency.DG, 1000, OfferTier.S),
            // Titles DC
            new OfferItem("title_veterano",  "Veterano",   OfferCurrency.DC, 3000, OfferTier.B),
            // BattleCards DG
            new OfferItem("card_frost",   "Frost Card",   OfferCurrency.DG, 75, OfferTier.B),
            new OfferItem("card_shadow",  "Shadow Card",  OfferCurrency.DG, 150, OfferTier.B),
            new OfferItem("card_phoenix", "Phoenix Card", OfferCurrency.DG, 200, OfferTier.A),
            new OfferItem("card_inferno", "Inferno Card", OfferCurrency.DG, 400, OfferTier.A),
        };

        /// <summary>Slot 3 — Free rewards (DC + XP boosts)</summary>
        private static readonly FreeReward[] FreeRewardsPool = new FreeReward[]
        {
            new FreeReward(FreeRewardType.DC, 50),
            new FreeReward(FreeRewardType.DC, 75),
            new FreeReward(FreeRewardType.DC, 100),
            new FreeReward(FreeRewardType.DC, 150),
            new FreeReward(FreeRewardType.DC, 200),
            new FreeReward(FreeRewardType.XPBoost, 24), // 24h double XP
        };

        // ==================== TIER WEIGHTS (for Slot 1) ====================
        // S=20%, A=40%, B=25%, Exclusive=15%
        private static readonly Dictionary<OfferTier, float> TierWeights = new Dictionary<OfferTier, float>
        {
            { OfferTier.S, 0.20f },
            { OfferTier.A, 0.40f },
            { OfferTier.B, 0.25f },
            { OfferTier.Exclusive, 0.15f },
        };

        // ==================== STATE ====================

        private DailyOfferState _state;
        private DailyOffer[] _todayOffers = new DailyOffer[3];
        private bool _initialized;

        private const string STATE_KEY = "DailyOfferState";
        private const float SLOT1_DISCOUNT = 0.50f; // 50% off
        private const float SLOT2_DISCOUNT = 0.35f; // 35% off (midpoint of 30-40%)
        private const int SLOT1_NO_REPEAT_DAYS = 7;
        private const int SLOT2_NO_REPEAT_DAYS = 5;

        // ==================== EVENTS ====================

        public event Action OnOffersRefreshed;
        public event Action<int, DailyOffer> OnOfferPurchased; // slotIndex, offer

        // ==================== PUBLIC API ====================

        public DailyOffer GetOffer(int slot) => (slot >= 0 && slot < 3) ? _todayOffers[slot] : null;
        public DailyOffer[] TodayOffers => _todayOffers;
        public bool IsInitialized => _initialized;

        /// <summary>Time remaining until midnight UTC (next refresh)</summary>
        public TimeSpan TimeUntilRefresh
        {
            get
            {
                DateTime tomorrow = DateTime.UtcNow.Date.AddDays(1);
                return tomorrow - DateTime.UtcNow;
            }
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
            LoadState();
            GenerateOffersIfNeeded();
            _initialized = true;
            WishlistService.Instance?.CheckAndNotifyWishlist(_todayOffers);
            Debug.Log($"[DailyOfferService] Initialized — today: {GetTodayKey()}");
        }

        // ==================== CORE GENERATION ====================

        private string GetTodayKey() => DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        /// <summary>
        /// Deterministic seed from UTC date — all players see the same offers.
        /// </summary>
        private int GetDaySeed()
        {
            var today = DateTime.UtcNow.Date;
            int dateBase = today.Year * 10000 + today.Month * 100 + today.Day;
            int userId = Mathf.Abs(PlayerPrefs.GetString("DP_SavedUserId", "").GetHashCode());
            return dateBase + userId;
        }

        private void GenerateOffersIfNeeded()
        {
            string today = GetTodayKey();

            if (_state.lastGeneratedDate == today && _state.offers != null && _state.offers.Length == 3)
            {
                // Already generated today — restore
                _todayOffers = _state.offers;
                Debug.Log("[DailyOfferService] Restored today's offers from cache");
                return;
            }

            // New day — generate fresh offers
            System.Random rng = new System.Random(GetDaySeed());

            _todayOffers[0] = GenerateSlot1(rng);
            _todayOffers[1] = GenerateSlot2(rng);
            _todayOffers[2] = GenerateSlot3(rng);

            _state.lastGeneratedDate = today;
            _state.offers = _todayOffers;
            SaveState();

            OnOffersRefreshed?.Invoke();
            Debug.Log($"[DailyOfferService] Generated offers: [{_todayOffers[0].displayName}] [{_todayOffers[1].displayName}] [{_todayOffers[2].displayName}]");
        }

        /// <summary>Slot 1: Theme at 50% off. Weighted by tier. No repeat in 7 days.</summary>
        private DailyOffer GenerateSlot1(System.Random rng)
        {
            var candidates = new List<OfferItem>();
            foreach (var item in ThemePool)
            {
                // Skip if shown in last 7 days
                if (_state.slot1History != null && Array.IndexOf(_state.slot1History, item.itemId) >= 0)
                    continue;
                candidates.Add(item);
            }

            if (candidates.Count == 0)
            {
                // All shown recently — reset history
                _state.slot1History = new string[0];
                candidates.AddRange(ThemePool);
            }

            OfferItem selected = WeightedPick(candidates, rng);
            int discountedPrice = Mathf.RoundToInt(selected.price * (1f - SLOT1_DISCOUNT));

            // Update history (keep last 7)
            var history = new List<string>(_state.slot1History ?? new string[0]);
            history.Add(selected.itemId);
            if (history.Count > SLOT1_NO_REPEAT_DAYS)
                history.RemoveAt(0);
            _state.slot1History = history.ToArray();

            return new DailyOffer
            {
                slotIndex = 0,
                itemId = selected.itemId,
                displayName = selected.displayName,
                currency = selected.currency,
                originalPrice = selected.price,
                discountedPrice = discountedPrice,
                discountPercent = 50,
                purchased = false,
                isFree = false
            };
        }

        /// <summary>Slot 2: Cosmetic at 35% off. No repeat in 5 days.</summary>
        private DailyOffer GenerateSlot2(System.Random rng)
        {
            var candidates = new List<OfferItem>();
            foreach (var item in CosmeticsPool)
            {
                if (_state.slot2History != null && Array.IndexOf(_state.slot2History, item.itemId) >= 0)
                    continue;
                candidates.Add(item);
            }

            if (candidates.Count == 0)
            {
                _state.slot2History = new string[0];
                candidates.AddRange(CosmeticsPool);
            }

            OfferItem selected = candidates[rng.Next(candidates.Count)];
            int discountedPrice = Mathf.RoundToInt(selected.price * (1f - SLOT2_DISCOUNT));

            var history = new List<string>(_state.slot2History ?? new string[0]);
            history.Add(selected.itemId);
            if (history.Count > SLOT2_NO_REPEAT_DAYS)
                history.RemoveAt(0);
            _state.slot2History = history.ToArray();

            return new DailyOffer
            {
                slotIndex = 1,
                itemId = selected.itemId,
                displayName = selected.displayName,
                currency = selected.currency,
                originalPrice = selected.price,
                discountedPrice = discountedPrice,
                discountPercent = 35,
                purchased = false,
                isFree = false
            };
        }

        /// <summary>Slot 3: Free DC or XP boost.</summary>
        private DailyOffer GenerateSlot3(System.Random rng)
        {
            var reward = FreeRewardsPool[rng.Next(FreeRewardsPool.Length)];
            string name = reward.type == FreeRewardType.DC
                ? $"+{reward.amount} DC"
                : $"XP x2 ({reward.amount}h)";

            return new DailyOffer
            {
                slotIndex = 2,
                itemId = $"free_{reward.type}_{reward.amount}",
                displayName = name,
                currency = OfferCurrency.Free,
                originalPrice = 0,
                discountedPrice = 0,
                discountPercent = 100,
                purchased = false,
                isFree = true,
                freeRewardType = reward.type,
                freeRewardAmount = reward.amount
            };
        }

        /// <summary>Weighted random pick by tier. S=20%, A=40%, B=25%, Excl=15%.</summary>
        private OfferItem WeightedPick(List<OfferItem> candidates, System.Random rng)
        {
            // Group by tier
            var byTier = new Dictionary<OfferTier, List<OfferItem>>();
            foreach (var item in candidates)
            {
                if (!byTier.ContainsKey(item.tier))
                    byTier[item.tier] = new List<OfferItem>();
                byTier[item.tier].Add(item);
            }

            // Build weighted list of available tiers
            float totalWeight = 0;
            var availableTiers = new List<(OfferTier tier, float weight)>();
            foreach (var kvp in byTier)
            {
                if (TierWeights.TryGetValue(kvp.Key, out float w))
                {
                    availableTiers.Add((kvp.Key, w));
                    totalWeight += w;
                }
            }

            // Pick tier
            float roll = (float)(rng.NextDouble() * totalWeight);
            float cumulative = 0;
            OfferTier pickedTier = availableTiers[0].tier;
            foreach (var (tier, weight) in availableTiers)
            {
                cumulative += weight;
                if (roll <= cumulative)
                {
                    pickedTier = tier;
                    break;
                }
            }

            // Pick random item from that tier
            var pool = byTier[pickedTier];
            return pool[rng.Next(pool.Count)];
        }

        // ==================== PURCHASE ====================

        /// <summary>
        /// Purchase a daily offer. Returns true if successful.
        /// Single purchase per slot per day — anti-abuse.
        /// </summary>
        public bool PurchaseOffer(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= 3) return false;

            var offer = _todayOffers[slotIndex];
            if (offer == null || offer.purchased) return false;

            var currency = DigitPark.Monetization.CurrencyManager.Instance;
            if (currency == null) return false;

            if (offer.isFree)
            {
                // Free reward — grant directly
                ApplyFreeReward(offer);
            }
            else
            {
                // Paid offer — deduct currency
                bool success;
                switch (offer.currency)
                {
                    case OfferCurrency.DG:
                        success = currency.SpendGems(offer.discountedPrice);
                        if (!success) return false;
                        break;
                    case OfferCurrency.DC:
                        success = currency.SpendCoins(offer.discountedPrice);
                        if (!success) return false;
                        break;
                    default:
                        return false;
                }

                // Grant the actual item based on itemId prefix
                GrantDailyOfferItem(offer);
                Debug.Log($"[DailyOfferService] Purchased: {offer.displayName} for {offer.discountedPrice} {offer.currency}");
            }

            offer.purchased = true;
            SaveState();

            // Analytics
            AnalyticsService.Instance?.LogCustomEvent("daily_offer_purchased", new Dictionary<string, object>
            {
                { "slot", slotIndex },
                { "item_id", offer.itemId },
                { "currency", offer.currency.ToString() },
                { "price", offer.discountedPrice },
                { "discount_pct", offer.discountPercent }
            });

            OnOfferPurchased?.Invoke(slotIndex, offer);
            return true;
        }

        private void GrantDailyOfferItem(DailyOffer offer)
        {
            string id = offer.itemId;
            if (id.StartsWith("theme_"))
            {
                PlayerPrefs.SetInt($"Theme_Owned_{id}", 1);
                PlayerPrefs.Save();
            }
            else if (id.StartsWith("frame_"))
            {
                PlayerPrefs.SetInt($"Frame_Owned_{id}", 1);
                PlayerPrefs.Save();
            }
            else if (id.StartsWith("effect_"))
            {
                VictoryEffectService.Instance?.UnlockEffect(id);
            }
            else if (id.StartsWith("title_"))
            {
                PlayerPrefs.SetInt($"Title_Owned_{id}", 1);
                PlayerPrefs.Save();
            }
            else if (id.StartsWith("card_"))
            {
                PlayerPrefs.SetInt($"BattleCard_Owned_{id}", 1);
                PlayerPrefs.Save();
            }
            else
            {
                Debug.LogWarning($"[DailyOfferService] Unknown item type for grant: {id}");
            }
        }

        private void ApplyFreeReward(DailyOffer offer)
        {
            var currency = DigitPark.Monetization.CurrencyManager.Instance;
            switch (offer.freeRewardType)
            {
                case FreeRewardType.DC:
                    currency?.AddCoins(offer.freeRewardAmount);
                    AnalyticsService.Instance?.LogVirtualCurrencyEarned("digitcoins", offer.freeRewardAmount, "daily_offer_free");
                    break;
                case FreeRewardType.XPBoost:
                    // Store XP boost expiry in PlayerPrefs
                    var expiry = DateTime.UtcNow.AddHours(offer.freeRewardAmount);
                    PlayerPrefs.SetString("DP_XPBoost_Expiry", expiry.ToString("o"));
                    PlayerPrefs.Save();
                    Debug.Log($"[DailyOfferService] XP Boost activated: x2 for {offer.freeRewardAmount}h");
                    break;
            }
        }

        // ==================== PERSISTENCE ====================

        private void LoadState()
        {
            string json = PlayerPrefs.GetString(STATE_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    _state = JsonUtility.FromJson<DailyOfferState>(json);
                    if (_state == null) _state = new DailyOfferState();
                }
                catch
                {
                    _state = new DailyOfferState();
                }
            }
            else
            {
                _state = new DailyOfferState();
            }
        }

        private void SaveState()
        {
            string json = JsonUtility.ToJson(_state);
            PlayerPrefs.SetString(STATE_KEY, json);
            PlayerPrefs.Save();

            // Sync to Firebase
            SyncToFirebase().ContinueWith(t =>
            {
                if (t.IsFaulted)
                    Debug.LogWarning($"[DailyOfferService] Firebase sync failed: {t.Exception?.GetBaseException().Message}");
            }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
        }

        private async Task SyncToFirebase()
        {
            var auth = AuthenticationService.Instance;
            if (auth == null || !auth.IsUserAuthenticated()) return;

            var playerData = auth.GetCurrentPlayerData();
            if (playerData == null || DatabaseService.Instance == null) return;

            try
            {
                string json = JsonUtility.ToJson(_state);
                var updates = new Dictionary<string, object>
                {
                    { "dailyOffers/state", json },
                    { "dailyOffers/lastDate", _state.lastGeneratedDate }
                };
                await DatabaseService.Instance.UpdatePlayerFields(playerData.userId, updates);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DailyOfferService] Firebase sync failed: {e.Message}");
            }
        }
    }

    // ==================== DATA STRUCTURES ====================

    public enum OfferCurrency { DC, DG, Free }
    public enum OfferTier { S, A, B, Exclusive }
    public enum FreeRewardType { DC, XPBoost }

    [Serializable]
    public class OfferItem
    {
        public string itemId;
        public string displayName;
        public OfferCurrency currency;
        public int price;
        public OfferTier tier;

        public OfferItem(string id, string name, OfferCurrency cur, int p, OfferTier t)
        {
            itemId = id; displayName = name; currency = cur; price = p; tier = t;
        }
    }

    [Serializable]
    public class FreeReward
    {
        public FreeRewardType type;
        public int amount;

        public FreeReward(FreeRewardType t, int a) { type = t; amount = a; }
    }

    [Serializable]
    public class DailyOffer
    {
        public int slotIndex;
        public string itemId;
        public string displayName;
        public OfferCurrency currency;
        public int originalPrice;
        public int discountedPrice;
        public int discountPercent;
        public bool purchased;
        public bool isFree;
        public FreeRewardType freeRewardType;
        public int freeRewardAmount;
    }

    [Serializable]
    public class DailyOfferState
    {
        public string lastGeneratedDate = "";
        public DailyOffer[] offers;
        public string[] slot1History = new string[0];
        public string[] slot2History = new string[0];
    }
}
