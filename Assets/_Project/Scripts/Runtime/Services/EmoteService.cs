using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DigitPark.Services.Firebase;

namespace DigitPark.Services
{
    /// <summary>
    /// Tipo de precio del pack de emotes
    /// </summary>
    public enum EmotePackPriceType
    {
        Free,
        DigitCoins,
        DigitGems,
        RealMoney
    }

    /// <summary>
    /// Datos de un emote individual
    /// </summary>
    [Serializable]
    public class EmoteData
    {
        public string emoteId;
        public string nameKey;      // Localization key
        public string packId;       // Which pack this belongs to
        public string spriteName;   // Resources path for sprite
        public bool isAnimated;
    }

    /// <summary>
    /// Datos de un pack de emotes
    /// </summary>
    [Serializable]
    public class EmotePackData
    {
        public string packId;
        public string nameKey;
        public EmotePackPriceType priceType;
        public int coinPrice;
        public int gemPrice;
        public float realMoneyPrice;
        public List<string> emoteIds;
    }

    /// <summary>
    /// Servicio singleton que gestiona emotes para partidas 1v1.
    /// 4 packs de 4 emotes cada uno (16 total).
    /// Persistencia via PlayerPrefs.
    /// </summary>
    public class EmoteService : MonoBehaviour
    {
        private static EmoteService _instance;
        public static EmoteService Instance => _instance;

        private const string OWNED_PACKS_KEY = "OwnedEmotePacks";
        private const string EQUIPPED_EMOTES_KEY = "EquippedEmotes";
        private const int MAX_EQUIPPED = 4;

        private HashSet<string> _ownedPacks = new HashSet<string>();
        private List<string> _equippedEmotes = new List<string>();
        private List<EmoteData> _allEmotes = new List<EmoteData>();
        private List<EmotePackData> _allPacks = new List<EmotePackData>();

        public event Action<string> OnEmoteUsed;
        public event Action<string> OnPackUnlocked;

        public List<EmoteData> AllEmotes => _allEmotes;
        public List<EmotePackData> AllPacks => _allPacks;
        public List<string> EquippedEmotes => _equippedEmotes;

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
            SetupEmotes();
            LoadState();

            // Basic pack is always owned
            if (!_ownedPacks.Contains("basic_pack"))
            {
                _ownedPacks.Add("basic_pack");
                SaveOwnedPacks();
            }

            // Default equipped: first 4 from basic pack
            if (_equippedEmotes.Count == 0)
            {
                _equippedEmotes.AddRange(new[] { "gg", "thumbs_up", "clap", "wave" });
                SaveEquippedEmotes();
            }

            Debug.Log($"[EmoteService] Initialized: {_ownedPacks.Count} packs owned, {_equippedEmotes.Count} equipped");
        }

        private void SetupEmotes()
        {
            // ==================== BASIC PACK (Free) ====================
            _allEmotes.Add(new EmoteData { emoteId = "gg", nameKey = "emote_gg", packId = "basic_pack", spriteName = "Emotes/GG" });
            _allEmotes.Add(new EmoteData { emoteId = "thumbs_up", nameKey = "emote_thumbs_up", packId = "basic_pack", spriteName = "Emotes/ThumbsUp" });
            _allEmotes.Add(new EmoteData { emoteId = "clap", nameKey = "emote_clap", packId = "basic_pack", spriteName = "Emotes/Clap" });
            _allEmotes.Add(new EmoteData { emoteId = "wave", nameKey = "emote_wave", packId = "basic_pack", spriteName = "Emotes/Wave" });

            _allPacks.Add(new EmotePackData
            {
                packId = "basic_pack",
                nameKey = "emote_pack_basic",
                priceType = EmotePackPriceType.Free,
                emoteIds = new List<string> { "gg", "thumbs_up", "clap", "wave" }
            });

            // ==================== REACTIONS PACK (1500 coins) ====================
            _allEmotes.Add(new EmoteData { emoteId = "laugh", nameKey = "emote_laugh", packId = "reactions_pack", spriteName = "Emotes/Laugh" });
            _allEmotes.Add(new EmoteData { emoteId = "cry", nameKey = "emote_cry", packId = "reactions_pack", spriteName = "Emotes/Cry" });
            _allEmotes.Add(new EmoteData { emoteId = "angry", nameKey = "emote_angry", packId = "reactions_pack", spriteName = "Emotes/Angry" });
            _allEmotes.Add(new EmoteData { emoteId = "surprised", nameKey = "emote_surprised", packId = "reactions_pack", spriteName = "Emotes/Surprised" });

            _allPacks.Add(new EmotePackData
            {
                packId = "reactions_pack",
                nameKey = "emote_pack_reactions",
                priceType = EmotePackPriceType.DigitCoins,
                coinPrice = 1500,
                emoteIds = new List<string> { "laugh", "cry", "angry", "surprised" }
            });

            // ==================== FLEX PACK (300 gems) ====================
            _allEmotes.Add(new EmoteData { emoteId = "crown", nameKey = "emote_crown", packId = "flex_pack", spriteName = "Emotes/Crown" });
            _allEmotes.Add(new EmoteData { emoteId = "fire", nameKey = "emote_fire", packId = "flex_pack", spriteName = "Emotes/Fire" });
            _allEmotes.Add(new EmoteData { emoteId = "diamond", nameKey = "emote_diamond", packId = "flex_pack", spriteName = "Emotes/Diamond" });
            _allEmotes.Add(new EmoteData { emoteId = "star", nameKey = "emote_star", packId = "flex_pack", spriteName = "Emotes/Star" });

            _allPacks.Add(new EmotePackData
            {
                packId = "flex_pack",
                nameKey = "emote_pack_flex",
                priceType = EmotePackPriceType.DigitGems,
                gemPrice = 300,
                emoteIds = new List<string> { "crown", "fire", "diamond", "star" }
            });

            // ==================== PREMIUM PACK ($1.99, animated) ====================
            _allEmotes.Add(new EmoteData { emoteId = "animated_gg", nameKey = "emote_animated_gg", packId = "premium_pack", spriteName = "Emotes/AnimatedGG", isAnimated = true });
            _allEmotes.Add(new EmoteData { emoteId = "animated_trophy", nameKey = "emote_animated_trophy", packId = "premium_pack", spriteName = "Emotes/AnimatedTrophy", isAnimated = true });
            _allEmotes.Add(new EmoteData { emoteId = "animated_fireworks", nameKey = "emote_animated_fireworks", packId = "premium_pack", spriteName = "Emotes/AnimatedFireworks", isAnimated = true });
            _allEmotes.Add(new EmoteData { emoteId = "animated_lightning", nameKey = "emote_animated_lightning", packId = "premium_pack", spriteName = "Emotes/AnimatedLightning", isAnimated = true });

            _allPacks.Add(new EmotePackData
            {
                packId = "premium_pack",
                nameKey = "emote_pack_premium",
                priceType = EmotePackPriceType.RealMoney,
                realMoneyPrice = 1.99f,
                emoteIds = new List<string> { "animated_gg", "animated_trophy", "animated_fireworks", "animated_lightning" }
            });
        }

        // ==================== PERSISTENCE ====================

        private void LoadState()
        {
            string packsJson = PlayerPrefs.GetString(OWNED_PACKS_KEY, "");
            if (!string.IsNullOrEmpty(packsJson))
            {
                try
                {
                    var data = JsonUtility.FromJson<StringListWrapper>(packsJson);
                    if (data?.items != null)
                    {
                        foreach (var id in data.items)
                            _ownedPacks.Add(id);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[EmoteService] Error loading packs: {e.Message}");
                }
            }

            string equippedJson = PlayerPrefs.GetString(EQUIPPED_EMOTES_KEY, "");
            if (!string.IsNullOrEmpty(equippedJson))
            {
                try
                {
                    var data = JsonUtility.FromJson<StringListWrapper>(equippedJson);
                    if (data?.items != null)
                        _equippedEmotes = data.items;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[EmoteService] Error loading equipped: {e.Message}");
                }
            }
        }

        private void SaveOwnedPacks()
        {
            var data = new StringListWrapper { items = new List<string>(_ownedPacks) };
            string ownedJson = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(OWNED_PACKS_KEY, ownedJson);
            PlayerPrefs.Save();

            // Sync to Firebase
            var equippedData = new StringListWrapper { items = new List<string>(_equippedEmotes) };
            string equippedJson = JsonUtility.ToJson(equippedData);
            SyncEmotesToFirebase(ownedJson, equippedJson).ContinueWith(t =>
            {
                if (t.IsFaulted)
                    Debug.LogWarning($"[EmoteService] Firebase sync failed: {t.Exception?.GetBaseException().Message}");
            }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
        }

        private void SaveEquippedEmotes()
        {
            var data = new StringListWrapper { items = new List<string>(_equippedEmotes) };
            string equippedJson = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(EQUIPPED_EMOTES_KEY, equippedJson);
            PlayerPrefs.Save();

            // Sync to Firebase
            var ownedData = new StringListWrapper { items = new List<string>(_ownedPacks) };
            string ownedJson = JsonUtility.ToJson(ownedData);
            SyncEmotesToFirebase(ownedJson, equippedJson).ContinueWith(t =>
            {
                if (t.IsFaulted)
                    Debug.LogWarning($"[EmoteService] Firebase sync failed: {t.Exception?.GetBaseException().Message}");
            }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
        }

        private async Task SyncEmotesToFirebase(string ownedPacksJson, string equippedEmotesJson)
        {
            var playerData = AuthenticationService.Instance?.GetCurrentPlayerData();
            if (playerData == null) return;

            try
            {
                var updates = new Dictionary<string, object>
                {
                    { "ownedEmotePacks", ownedPacksJson },
                    { "equippedEmotes", equippedEmotesJson }
                };

                if (DatabaseService.Instance != null)
                {
                    await DatabaseService.Instance.UpdatePlayerFields(playerData.userId, updates);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[EmoteService] Error syncing to Firebase: {e.Message}");
            }
        }

        // ==================== PUBLIC API ====================

        public bool IsPackOwned(string packId) => _ownedPacks.Contains(packId);

        public bool IsEmoteOwned(string emoteId)
        {
            var emote = _allEmotes.Find(e => e.emoteId == emoteId);
            if (emote == null) return false;
            return _ownedPacks.Contains(emote.packId);
        }

        public void UnlockPack(string packId)
        {
            if (_ownedPacks.Contains(packId)) return;
            _ownedPacks.Add(packId);
            SaveOwnedPacks();
            Debug.Log($"[EmoteService] Pack unlocked: {packId}");
            OnPackUnlocked?.Invoke(packId);
        }

        /// <summary>
        /// Sets the equipped emote at a slot (0-3)
        /// </summary>
        public void EquipEmote(int slot, string emoteId)
        {
            if (slot < 0 || slot >= MAX_EQUIPPED) return;
            if (!IsEmoteOwned(emoteId)) return;

            while (_equippedEmotes.Count <= slot)
                _equippedEmotes.Add("");

            _equippedEmotes[slot] = emoteId;
            SaveEquippedEmotes();
            Debug.Log($"[EmoteService] Emote equipped at slot {slot}: {emoteId}");
        }

        /// <summary>
        /// Uses an emote in a match (fires event for networking/UI)
        /// </summary>
        public void UseEmote(string emoteId)
        {
            if (!IsEmoteOwned(emoteId))
            {
                Debug.LogWarning($"[EmoteService] Emote not owned: {emoteId}");
                return;
            }

            OnEmoteUsed?.Invoke(emoteId);
            Debug.Log($"[EmoteService] Emote used: {emoteId}");
        }

        public EmoteData GetEmoteData(string emoteId) => _allEmotes.Find(e => e.emoteId == emoteId);
        public EmotePackData GetPackData(string packId) => _allPacks.Find(p => p.packId == packId);

        public List<EmoteData> GetEmotesInPack(string packId) => _allEmotes.FindAll(e => e.packId == packId);

        public List<EmoteData> GetOwnedEmotes()
        {
            var result = new List<EmoteData>();
            foreach (var emote in _allEmotes)
            {
                if (IsEmoteOwned(emote.emoteId))
                    result.Add(emote);
            }
            return result;
        }

        /// <summary>
        /// Attempts to purchase an emote pack with currency.
        /// </summary>
        public bool TryPurchasePack(string packId)
        {
            if (IsPackOwned(packId)) return false;

            var pack = GetPackData(packId);
            if (pack == null) return false;

            var currency = DigitPark.Monetization.CurrencyManager.Instance;
            if (currency == null) return false;

            switch (pack.priceType)
            {
                case EmotePackPriceType.Free:
                    break;
                case EmotePackPriceType.DigitCoins:
                    if (!currency.SpendCoins(pack.coinPrice)) return false;
                    break;
                case EmotePackPriceType.DigitGems:
                    if (!currency.SpendGems(pack.gemPrice)) return false;
                    break;
                case EmotePackPriceType.RealMoney:
                    Debug.LogWarning("[EmoteService] Real money packs should use IAP system");
                    return false;
            }

            UnlockPack(packId);
            return true;
        }

        /// <summary>
        /// Loads emote sprite from Resources
        /// </summary>
        public Sprite LoadEmoteSprite(string emoteId)
        {
            var emote = GetEmoteData(emoteId);
            if (emote == null) return null;

            return Resources.Load<Sprite>(emote.spriteName);
        }

        // ==================== DEBUG ====================

#if UNITY_EDITOR
        [ContextMenu("Debug: Unlock All Packs")]
        private void DebugUnlockAll()
        {
            foreach (var pack in _allPacks)
                UnlockPack(pack.packId);
        }

        [ContextMenu("Debug: Reset Emotes")]
        private void DebugReset()
        {
            _ownedPacks.Clear();
            _ownedPacks.Add("basic_pack");
            _equippedEmotes = new List<string> { "gg", "thumbs_up", "clap", "wave" };
            SaveOwnedPacks();
            SaveEquippedEmotes();
            Debug.Log("[EmoteService] Reset to defaults");
        }
#endif
    }
}
