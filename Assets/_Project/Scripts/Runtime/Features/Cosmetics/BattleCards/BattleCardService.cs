using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DigitPark.Cosmetics
{
    /// <summary>
    /// Servicio singleton que gestiona BattleCards del jugador.
    /// Independiente de ThemeManager — solo afecta matchmaking cards.
    /// </summary>
    public class BattleCardService : MonoBehaviour
    {
        public static BattleCardService Instance { get; private set; }

        private const string EQUIPPED_KEY = "EquippedBattleCard";
        private const string OWNED_KEY = "OwnedBattleCards";
        private const string DEFAULT_CARD_ID = "battlecard_neon_core";

        private BattleCardData[] _allCards;
        private Dictionary<string, BattleCardData> _cardLookup;
        private HashSet<string> _ownedCardIds;
        private string _equippedCardId;

        // Events
        public event Action<BattleCardData> OnCardEquipped;
        public event Action<string> OnCardUnlocked;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
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
            // Load all BattleCard assets from Resources
            _allCards = Resources.LoadAll<BattleCardData>("BattleCards");
            _cardLookup = new Dictionary<string, BattleCardData>();

            foreach (var card in _allCards)
            {
                if (!string.IsNullOrEmpty(card.cardId))
                    _cardLookup[card.cardId] = card;
            }

            // Load owned cards
            _ownedCardIds = new HashSet<string>();
            _ownedCardIds.Add(DEFAULT_CARD_ID); // Free card always owned

            string ownedJson = PlayerPrefs.GetString(OWNED_KEY, "");
            if (!string.IsNullOrEmpty(ownedJson))
            {
                try
                {
                    var wrapper = JsonUtility.FromJson<StringListWrapper>(ownedJson);
                    if (wrapper != null && wrapper.items != null)
                    {
                        foreach (var id in wrapper.items)
                            _ownedCardIds.Add(id);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[BattleCardService] Failed to parse owned cards: {e.Message}");
                }
            }

            // Load equipped card
            _equippedCardId = PlayerPrefs.GetString(EQUIPPED_KEY, DEFAULT_CARD_ID);
            if (!_ownedCardIds.Contains(_equippedCardId))
                _equippedCardId = DEFAULT_CARD_ID;

            if (_allCards.Length == 0)
                Debug.LogWarning("[BattleCardService] No BattleCard assets found in Resources/BattleCards/");

            Debug.Log($"[BattleCardService] Initialized: {_allCards.Length} cards loaded, {_ownedCardIds.Count} owned, equipped={_equippedCardId}");

            // Pull data from Firebase after local init
            _ = LoadFromFirebase();
        }

        #region Public API

        public BattleCardData GetEquippedCard()
        {
            return GetCardById(_equippedCardId) ?? GetCardById(DEFAULT_CARD_ID);
        }

        public string EquippedCardId => _equippedCardId;

        public BattleCardData GetCardById(string cardId)
        {
            if (string.IsNullOrEmpty(cardId)) return null;
            _cardLookup.TryGetValue(cardId, out BattleCardData card);
            return card;
        }

        public BattleCardData GetDefault()
        {
            return GetCardById(DEFAULT_CARD_ID);
        }

        public bool IsOwned(string cardId)
        {
            return _ownedCardIds.Contains(cardId);
        }

        public BattleCardData[] AllCards => _allCards;

        public List<BattleCardData> GetByCategory(BattleCardCategory category)
        {
            var result = new List<BattleCardData>();
            foreach (var card in _allCards)
            {
                if (card.category == category)
                    result.Add(card);
            }
            return result;
        }

        public void EquipCard(string cardId)
        {
            if (!_ownedCardIds.Contains(cardId))
            {
                Debug.LogWarning($"[BattleCardService] Cannot equip unowned card: {cardId}");
                return;
            }

            _equippedCardId = cardId;
            PlayerPrefs.SetString(EQUIPPED_KEY, cardId);
            PlayerPrefs.Save();

            // Sync to Firebase
            SyncToFirebase();

            var card = GetCardById(cardId);
            OnCardEquipped?.Invoke(card);
            Debug.Log($"[BattleCardService] Equipped: {cardId}");
        }

        public void UnlockCard(string cardId)
        {
            if (_ownedCardIds.Contains(cardId)) return;

            _ownedCardIds.Add(cardId);
            SaveOwnedCards();

            OnCardUnlocked?.Invoke(cardId);
            Debug.Log($"[BattleCardService] Unlocked: {cardId}");
        }

        /// <summary>
        /// Try to purchase a BattleCard with virtual currency.
        /// Returns true if successful.
        /// </summary>
        public bool TryPurchaseCard(string cardId)
        {
            if (_ownedCardIds.Contains(cardId)) return false;

            var card = GetCardById(cardId);
            if (card == null) return false;

            var currency = DigitPark.Monetization.CurrencyManager.Instance;
            if (currency == null) return false;

            switch (card.category)
            {
                case BattleCardCategory.DC:
                    if (!currency.SpendCoins(card.dcPrice)) return false;
                    break;
                case BattleCardCategory.DGStandard:
                case BattleCardCategory.DGPremium:
                    if (!currency.SpendGems(card.dgPrice)) return false;
                    break;
                case BattleCardCategory.Free:
                    break;
                case BattleCardCategory.Earn:
                    Debug.LogWarning($"[BattleCardService] Earn cards cannot be purchased: {cardId}");
                    return false;
            }

            UnlockCard(cardId);
            EquipCard(cardId);
            return true;
        }

        #endregion

        #region Persistence

        private void SaveOwnedCards()
        {
            var wrapper = new StringListWrapper();
            wrapper.items = new List<string>(_ownedCardIds);
            string json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(OWNED_KEY, json);
            PlayerPrefs.Save();

            SyncToFirebase();
        }

        private async void SyncToFirebase()
        {
            try
            {
                var db = DigitPark.Services.DatabaseService.Instance;
                var auth = DigitPark.Services.AuthenticationService.Instance;
                if (db == null || auth == null) return;

                string uid = auth.GetCurrentUserId();
                if (string.IsNullOrEmpty(uid)) return;

                var fields = new Dictionary<string, object>
                {
                    { "equippedBattleCard", _equippedCardId },
                    { "ownedBattleCards", new List<string>(_ownedCardIds) }
                };
                await db.UpdatePlayerFields(uid, fields);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BattleCardService] Firebase sync failed: {e.Message}");
            }
        }

        private async Task LoadFromFirebase()
        {
            try
            {
                var auth = DigitPark.Services.AuthenticationService.Instance;
                if (auth == null) return;

                string uid = auth.GetCurrentUserId();
                if (string.IsNullOrEmpty(uid)) return;

                var dbRef = Firebase.Database.FirebaseDatabase.DefaultInstance?.RootReference;
                if (dbRef == null) return;

                var snapshot = await dbRef.Child("players").Child(uid).GetValueAsync();
                if (snapshot == null || !snapshot.Exists) return;

                var data = snapshot.Value as Dictionary<string, object>;
                if (data == null) return;

                if (data.ContainsKey("equippedBattleCard"))
                {
                    string fbEquipped = data["equippedBattleCard"]?.ToString();
                    if (!string.IsNullOrEmpty(fbEquipped) && _cardLookup.ContainsKey(fbEquipped))
                    {
                        _equippedCardId = fbEquipped;
                        PlayerPrefs.SetString(EQUIPPED_KEY, _equippedCardId);
                    }
                }

                if (data.ContainsKey("ownedBattleCards"))
                {
                    var ownedList = data["ownedBattleCards"] as List<object>;
                    if (ownedList != null)
                    {
                        foreach (var item in ownedList)
                        {
                            string id = item?.ToString();
                            if (!string.IsNullOrEmpty(id))
                                _ownedCardIds.Add(id);
                        }
                        SaveOwnedCards();
                    }
                }

                // Ensure equipped is owned
                if (!_ownedCardIds.Contains(_equippedCardId))
                    _equippedCardId = DEFAULT_CARD_ID;

                Debug.Log($"[BattleCardService] Firebase sync complete: equipped={_equippedCardId}, owned={_ownedCardIds.Count}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BattleCardService] LoadFromFirebase failed: {e.Message}");
            }
        }

        #endregion

        [Serializable]
        private class StringListWrapper
        {
            public List<string> items = new List<string>();
        }
    }
}
