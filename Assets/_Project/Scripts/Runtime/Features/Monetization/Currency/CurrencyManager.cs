using UnityEngine;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DigitPark.Services;
using DigitPark.Services.Firebase;
using DigitPark.Navigation;
using Firebase.Database;

namespace DigitPark.Monetization
{
    /// <summary>
    /// Tipo de moneda para apuestas
    /// </summary>
    public enum BetCurrencyType
    {
        None,
        DigitGems,
        DigitCoins
    }

    /// <summary>
    /// Manager central para el sistema de monedas virtuales (Gemas y Monedas).
    /// Singleton que persiste entre escenas.
    /// </summary>
    public class CurrencyManager : MonoBehaviour
    {
        private static CurrencyManager _instance;
        public static CurrencyManager Instance
        {
            get
            {
                // C-67: Use implicit bool to detect destroyed Unity objects (== null is overloaded)
                if (!_instance)
                {
                    _instance = FindFirstObjectByType<CurrencyManager>();
                    if (_instance)
                    {
                        // Found existing instance — ensure it persists across scenes
                        DontDestroyOnLoad(_instance.gameObject);
                    }
                    else
                    {
                        GameObject go = new GameObject("CurrencyManager");
                        DontDestroyOnLoad(go);
                        // Set _instance BEFORE AddComponent to prevent Awake race condition
                        _instance = go.AddComponent<CurrencyManager>();
                    }
                }
                return _instance;
            }
        }

        // ==================== THREAD SAFETY ====================

        /// <summary>
        /// Lock object to prevent double-spend race conditions (SEC-C11).
        /// All check-and-deduct operations must be atomic under this lock.
        /// </summary>
        private static readonly object _currencyLock = new object();

        // ==================== CONSTANTES ====================

        private const string GEMS_KEY = "Currency_Gems";
        private const string COINS_KEY = "Currency_Coins";
        // Obfuscated keys — new storage uses XOR-masked values under different keys
        private const string GEMS_KEY_V2 = "dp_cg_v2";
        private const string COINS_KEY_V2 = "dp_cc_v2";
        // XOR salt — obscures raw balance from casual memory editors (not cryptographic)
        private const int CURRENCY_XOR_SALT = 0x4D50_3A21;

        // D-8: Escrow persistence keys — survive app crashes between escrow and settle
        private const string ESCROW_GEMS_KEY = "dp_escrow_gems";
        private const string ESCROW_COINS_KEY = "dp_escrow_coins";

        // Valores iniciales para nuevos jugadores
        private const int DEFAULT_GEMS = 0;
        private const int DEFAULT_COINS = 1000;

        // Economy Rebalance: 5% rake on bets (winner gets 1.9x instead of 2x)
        private const float BET_MULTIPLIER = 1.9f;

        // ==================== ESTADO ====================

        [Header("Current Balance (Read Only)")]
        [SerializeField] private int _gems;
        [SerializeField] private int _coins;

        public int Gems => _gems;
        public int Coins => _coins;

        // ==================== EVENTOS ====================

        /// <summary>
        /// Se dispara cuando cambian las gemas. Params: (newAmount, delta)
        /// </summary>
        public event Action<int, int> OnGemsChanged;

        /// <summary>
        /// Se dispara cuando cambian las monedas. Params: (newAmount, delta)
        /// </summary>
        public event Action<int, int> OnCoinsChanged;

        /// <summary>
        /// Se dispara cuando no hay suficientes gemas para una compra
        /// </summary>
        public event Action<int> OnNotEnoughGems;

        /// <summary>
        /// Se dispara cuando no hay suficientes monedas para una compra
        /// </summary>
        public event Action<int> OnNotEnoughCoins;

        // ==================== INICIALIZACION ====================

        private bool _firebaseRestoreCompleted = false;

        /// <summary>
        /// Whether the Firebase restore has completed (used by BootManager to wait if needed)
        /// </summary>
        public bool FirebaseRestoreCompleted => _firebaseRestoreCompleted;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                LoadCurrency();
                Debug.Log($"[CurrencyManager] Iniciado - Gemas: {_gems}, Monedas: {_coins}");

                // D-9 FIX: Async restore from Firebase after loading local cache.
                // If Firebase has higher values (e.g., reinstall scenario before BootManager migration runs),
                // we take the max of local vs Firebase to prevent data loss.
                _ = RestoreFromFirebaseAsync();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void LoadCurrency()
        {
            // Try new obfuscated keys first; fall back to legacy plain keys for migration
            if (PlayerPrefs.HasKey(GEMS_KEY_V2))
                _gems = PlayerPrefs.GetInt(GEMS_KEY_V2, DEFAULT_GEMS) ^ CURRENCY_XOR_SALT;
            else
                _gems = PlayerPrefs.GetInt(GEMS_KEY, DEFAULT_GEMS);

            if (PlayerPrefs.HasKey(COINS_KEY_V2))
                _coins = PlayerPrefs.GetInt(COINS_KEY_V2, DEFAULT_COINS) ^ CURRENCY_XOR_SALT;
            else
                _coins = PlayerPrefs.GetInt(COINS_KEY, DEFAULT_COINS);

            // SEC-M04: Verify balance integrity — if tampered, Firebase restore will override
            if (!VerifyBalanceIntegrity(_gems, _coins))
            {
                // On integrity failure reset to 0 locally — Firebase sync will restore correct values
                _gems = 0;
                _coins = 0;
            }

            // D-8 FIX: Restore persisted escrow state. If escrow > 0 on boot,
            // a crash happened mid-escrow — refund the escrowed amount back to balance.
            _escrowedGems = PlayerPrefs.GetInt(ESCROW_GEMS_KEY, 0);
            _escrowedCoins = PlayerPrefs.GetInt(ESCROW_COINS_KEY, 0);
            if (_escrowedGems > 0 || _escrowedCoins > 0)
            {
                Debug.LogWarning($"[CurrencyManager] Crash recovery: refunding escrowed currency " +
                    $"(Gems: {_escrowedGems}, Coins: {_escrowedCoins})");
                _gems += _escrowedGems;
                _coins += _escrowedCoins;
                _escrowedGems = 0;
                _escrowedCoins = 0;
                _escrowType = BetCurrencyType.None;
                ClearEscrowPrefs();
                // Save the refunded balance immediately
                SaveCurrency();
            }
        }

        /// <summary>
        /// B3-H: Force-set currency to Firebase values, bypassing the MAX check.
        /// Used for chargeback or admin corrections where Firebase has the lower authoritative value.
        /// Only call this from a trusted server-side response.
        /// </summary>
        public void ForceRestoreFromFirebase(int gems, int coins)
        {
            lock (_currencyLock)
            {
                _gems = Mathf.Max(gems, 0);
                _coins = Mathf.Max(coins, 0);
            }
            SaveCurrency();
            OnGemsChanged?.Invoke(_gems, 0);
            OnCoinsChanged?.Invoke(_coins, 0);
            Debug.Log($"[CurrencyManager] ForceRestore (chargeback/admin) — Gems: {_gems}, Coins: {_coins}");
        }

        /// <summary>
        /// D-23 FIX: Called by BootManager when a reinstall is detected.
        /// Sets local currency to the Firebase values and saves to PlayerPrefs.
        /// </summary>
        public void RestoreFromFirebaseValues(int gems, int coins)
        {
            lock (_currencyLock)
            {
                _gems = Mathf.Max(gems, 0);
                _coins = Mathf.Max(coins, 0);
            }

            SaveCurrency();
            OnGemsChanged?.Invoke(_gems, 0);
            OnCoinsChanged?.Invoke(_coins, 0);
            Debug.Log($"[CurrencyManager] Restored from Firebase — Gems: {_gems}, Coins: {_coins}");
        }

        /// <summary>
        /// D-9 FIX: Async Firebase restore on init.
        /// After loading from PlayerPrefs, checks Firebase for the authoritative balance.
        /// Takes the MAX of local vs Firebase to prevent data loss in any direction.
        /// Only runs if the user is authenticated and Firebase is reachable.
        /// </summary>
        private async Task RestoreFromFirebaseAsync()
        {
            try
            {
                // Wait for auth to be ready (CurrencyManager may init before AuthService)
                // Use polling with timeout instead of fixed 2s delay
                float waited = 0f;
                while (waited < 10f)
                {
                    if (AuthenticationService.Instance != null && AuthenticationService.Instance.IsInitialized
                        && AuthenticationService.Instance.GetCurrentPlayerData() != null)
                        break;
                    await Task.Delay(250);
                    waited += 0.25f;
                }

                var playerData = AuthenticationService.Instance?.GetCurrentPlayerData();
                if (playerData == null || string.IsNullOrEmpty(playerData.userId))
                {
                    _firebaseRestoreCompleted = true;
                    return;
                }

                var dbRef = FirebaseDatabase.DefaultInstance?.RootReference;
                if (dbRef == null)
                {
                    _firebaseRestoreCompleted = true;
                    return;
                }

                int firebaseGems = 0;
                int firebaseCoins = 0;

                var gemsSnapshot = await dbRef.Child("players").Child(playerData.userId).Child("gems").GetValueAsync();
                if (gemsSnapshot != null && gemsSnapshot.Exists)
                {
                    try { firebaseGems = Convert.ToInt32(gemsSnapshot.Value); }
                    catch { firebaseGems = 0; }
                }

                var coinsSnapshot = await dbRef.Child("players").Child(playerData.userId).Child("coins").GetValueAsync();
                if (coinsSnapshot != null && coinsSnapshot.Exists)
                {
                    try { firebaseCoins = Convert.ToInt32(coinsSnapshot.Value); }
                    catch { firebaseCoins = 0; }
                }

                // Take the MAX of local vs Firebase to prevent data loss in either direction
                bool changed = false;
                lock (_currencyLock)
                {
                    if (firebaseGems > _gems)
                    {
                        _gems = firebaseGems;
                        changed = true;
                    }
                    if (firebaseCoins > _coins)
                    {
                        _coins = firebaseCoins;
                        changed = true;
                    }
                }

                if (changed)
                {
                    SaveCurrency();
                    OnGemsChanged?.Invoke(_gems, 0);
                    OnCoinsChanged?.Invoke(_coins, 0);
                    Debug.Log($"[CurrencyManager] Firebase restore applied — Gems: {_gems}, Coins: {_coins}");
                }
                else
                {
                    Debug.Log("[CurrencyManager] Firebase restore — local values are current, no change needed");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CurrencyManager] Firebase restore failed (non-fatal): {e.Message}");
            }
            finally
            {
                _firebaseRestoreCompleted = true;
            }
        }

        // ==================== SEC-M04: HMAC-SHA256 BALANCE INTEGRITY ====================

        private const string BALANCE_HMAC_KEY = "dp_bal_hmac";
        // Device-bound secret: SystemInfo.deviceUniqueIdentifier + salt. Not cryptographically
        // perfect on rooted devices, but stops casual PlayerPrefs hex-editing.
        private static string GetHmacSecret() =>
            SystemInfo.deviceUniqueIdentifier + "_dp_bal_v1";

        private static string ComputeBalanceHmac(int gems, int coins)
        {
            try
            {
                byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(GetHmacSecret());
                byte[] data = System.Text.Encoding.UTF8.GetBytes($"{gems}:{coins}");
                using var hmac = new HMACSHA256(keyBytes);
                byte[] hash = hmac.ComputeHash(data);
                return System.Convert.ToBase64String(hash);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[CurrencyManager] HMAC compute failed: {ex.Message}");
                return "";
            }
        }

        /// <summary>
        /// SEC-M04: Returns true if the stored balance HMAC matches expected value.
        /// Call on load to detect tampering. On failure, falls back to Firebase sync.
        /// </summary>
        private bool VerifyBalanceIntegrity(int gems, int coins)
        {
            string storedHmac = PlayerPrefs.GetString(BALANCE_HMAC_KEY, "");
            if (string.IsNullOrEmpty(storedHmac)) return true; // no HMAC yet — first run
            string expectedHmac = ComputeBalanceHmac(gems, coins);
            bool valid = storedHmac == expectedHmac;
            if (!valid)
                Debug.LogWarning("[CurrencyManager] SEC-M04: Balance integrity check FAILED — possible tampering detected. Firebase sync will override.");
            return valid;
        }

        private void SaveCurrency()
        {
            PlayerPrefs.SetInt(GEMS_KEY_V2, _gems ^ CURRENCY_XOR_SALT);
            PlayerPrefs.SetInt(COINS_KEY_V2, _coins ^ CURRENCY_XOR_SALT);
            // SEC-M04: Write HMAC of raw (unobfuscated) balance values
            PlayerPrefs.SetString(BALANCE_HMAC_KEY, ComputeBalanceHmac(_gems, _coins));
            // Remove legacy plain-text keys after migration
            if (PlayerPrefs.HasKey(GEMS_KEY)) PlayerPrefs.DeleteKey(GEMS_KEY);
            if (PlayerPrefs.HasKey(COINS_KEY)) PlayerPrefs.DeleteKey(COINS_KEY);
            PlayerPrefs.Save();

            // Sync to Firebase
            _ = SyncCurrencyToFirebase();
        }

        private async Task SyncCurrencyToFirebase()
        {
            var playerData = AuthenticationService.Instance?.GetCurrentPlayerData();
            if (playerData == null) return;

            try
            {
                var updates = new Dictionary<string, object>
                {
                    { "gems", _gems },
                    { "coins", _coins }
                };

                if (DatabaseService.Instance != null)
                {
                    await DatabaseService.Instance.UpdatePlayerFields(playerData.userId, updates);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CurrencyManager] Error syncing to Firebase: {e.Message}");
            }
        }

        // ==================== GEMS METHODS ====================

        /// <summary>
        /// Agrega gemas al balance
        /// </summary>
        public void AddGems(int amount)
        {
            if (amount <= 0) return;

            int newGems;
            lock (_currencyLock)
            {
                // SEC-M13: Overflow protection - clamp to int.MaxValue
                if (_gems > int.MaxValue - amount)
                    _gems = int.MaxValue;
                else
                    _gems += amount;

                newGems = _gems;
            }

            SaveCurrency();
            OnGemsChanged?.Invoke(newGems, amount);
            Debug.Log($"[CurrencyManager] +{amount} gemas (Total: {newGems})");
        }

        /// <summary>
        /// Gasta gemas si hay suficientes
        /// </summary>
        /// <returns>true si se pudieron gastar, false si no hay suficientes</returns>
        public bool SpendGems(int amount)
        {
            if (amount <= 0) return true;

            int newGems = 0;
            int deficit = 0;
            bool success;
            lock (_currencyLock)
            {
                if (_gems < amount)
                {
                    deficit = amount - _gems;
                    success = false;
                }
                else
                {
                    _gems -= amount;
                    newGems = _gems;
                    success = true;
                }
            }

            if (success)
            {
                SaveCurrency();
                // Use captured newGems (read inside lock) to avoid race condition
                OnGemsChanged?.Invoke(newGems, -amount);
                Debug.Log($"[CurrencyManager] -{amount} gemas (Total: {newGems})");
            }
            else
            {
                OnNotEnoughGems?.Invoke(deficit);
                Debug.Log($"[CurrencyManager] Gemas insuficientes. Necesita: {amount}, Tiene: {_gems}");
            }

            return success;
        }

        /// <summary>
        /// Verifica si hay suficientes gemas
        /// </summary>
        public bool HasEnoughGems(int amount)
        {
            return _gems >= amount;
        }

        /// <summary>
        /// Intenta gastar gemas, si no hay suficientes navega a la tienda
        /// </summary>
        public bool TrySpendGemsOrNavigateToShop(int amount)
        {
            if (SpendGems(amount))
            {
                return true;
            }

            // No hay suficientes, navegar a la tienda
            SceneNavigator.Instance.NavigateToShopForDigitGems();
            return false;
        }

        // ==================== COINS METHODS ====================

        /// <summary>
        /// Agrega monedas al balance
        /// </summary>
        public void AddCoins(int amount)
        {
            if (amount <= 0) return;

            int newCoins;
            lock (_currencyLock)
            {
                // SEC-M13: Overflow protection - clamp to int.MaxValue
                if (_coins > int.MaxValue - amount)
                    _coins = int.MaxValue;
                else
                    _coins += amount;

                newCoins = _coins;
            }

            SaveCurrency();
            OnCoinsChanged?.Invoke(newCoins, amount);
            Debug.Log($"[CurrencyManager] +{amount} monedas (Total: {newCoins})");
        }

        /// <summary>
        /// Gasta monedas si hay suficientes
        /// </summary>
        /// <returns>true si se pudieron gastar, false si no hay suficientes</returns>
        public bool SpendCoins(int amount)
        {
            if (amount <= 0) return true;

            int newCoins = 0;
            int deficit = 0;
            bool success;
            lock (_currencyLock)
            {
                if (_coins < amount)
                {
                    deficit = amount - _coins;
                    success = false;
                }
                else
                {
                    _coins -= amount;
                    newCoins = _coins;
                    success = true;
                }
            }

            if (success)
            {
                SaveCurrency();
                // Use captured newCoins (read inside lock) to avoid race condition
                OnCoinsChanged?.Invoke(newCoins, -amount);
                Debug.Log($"[CurrencyManager] -{amount} monedas (Total: {newCoins})");
            }
            else
            {
                OnNotEnoughCoins?.Invoke(deficit);
                Debug.Log($"[CurrencyManager] Monedas insuficientes. Necesita: {amount}, Tiene: {_coins}");
            }

            return success;
        }

        /// <summary>
        /// Verifica si hay suficientes monedas
        /// </summary>
        public bool HasEnoughCoins(int amount)
        {
            return _coins >= amount;
        }

        // ==================== PURCHASE METHODS ====================

        /// <summary>
        /// [DISABLED] DG es moneda solo-compra. Exchange DG→DC eliminado para preservar valor de DG.
        /// </summary>
        [System.Obsolete("DG→DC exchange disabled. DG is purchase-only currency.")]
        public bool PurchaseCoinsWithGems(int coinsAmount, int gemsPrice)
        {
            Debug.LogWarning("[CurrencyManager] PurchaseCoinsWithGems is DISABLED. DG is purchase-only currency.");
            return false;
        }

        /// <summary>
        /// Procesa una compra de gemas con dinero real (llamado despues de IAP exitoso)
        /// </summary>
        public void ProcessGemsPurchase(int gemsAmount, int bonusGems = 0)
        {
            int totalGems = gemsAmount + bonusGems;
            AddGems(totalGems);

            if (bonusGems > 0)
            {
                Debug.Log($"[CurrencyManager] Compra IAP: {gemsAmount} gemas + {bonusGems} bonus = {totalGems} total");
            }
            else
            {
                Debug.Log($"[CurrencyManager] Compra IAP: {gemsAmount} gemas");
            }
        }

        // ==================== REWARDS ====================

        /// <summary>
        /// Otorga recompensa diaria
        /// </summary>
        public void GrantDailyReward(int gems, int coins)
        {
            if (gems > 0) AddGems(gems);
            if (coins > 0) AddCoins(coins);
            Debug.Log($"[CurrencyManager] Recompensa diaria: {gems} gemas, {coins} monedas");
        }

        /// <summary>
        /// Otorga recompensa de mision
        /// </summary>
        public void GrantMissionReward(int gems, int coins)
        {
            if (gems > 0) AddGems(gems);
            if (coins > 0) AddCoins(coins);
            Debug.Log($"[CurrencyManager] Recompensa de mision: {gems} gemas, {coins} monedas");
        }

        /// <summary>
        /// Otorga recompensa de logro
        /// </summary>
        public void GrantAchievementReward(int gems, int coins)
        {
            if (gems > 0) AddGems(gems);
            if (coins > 0) AddCoins(coins);
            Debug.Log($"[CurrencyManager] Recompensa de logro: {gems} gemas, {coins} monedas");
        }

        // ==================== ESCROW (Betting) ====================

        private int _escrowedGems;
        private int _escrowedCoins;
        private BetCurrencyType _escrowType = BetCurrencyType.None;

        public int EscrowedGems => _escrowedGems;
        public int EscrowedCoins => _escrowedCoins;
        public BetCurrencyType EscrowType => _escrowType;

        /// <summary>
        /// Deducts gems and holds them in escrow for a bet
        /// </summary>
        public bool EscrowGems(int amount)
        {
            if (amount <= 0) return true;

            int newGems = 0;
            int deficit = 0;
            bool success;
            lock (_currencyLock)
            {
                if (_gems < amount)
                {
                    deficit = amount - _gems;
                    success = false;
                }
                else
                {
                    _gems -= amount;
                    _escrowedGems = amount;
                    _escrowedCoins = 0;
                    _escrowType = BetCurrencyType.DigitGems;
                    newGems = _gems;
                    success = true;
                }
            }

            if (success)
            {
                SaveCurrency();
                SaveEscrowPrefs(); // D-8: Persist escrow to survive crashes
                // Use captured newGems (read inside lock) to avoid race condition
                OnGemsChanged?.Invoke(newGems, -amount);
                Debug.Log($"[CurrencyManager] Escrow: {amount} DigitGems held for bet");
            }
            else
            {
                OnNotEnoughGems?.Invoke(deficit);
                Debug.Log($"[CurrencyManager] Gemas insuficientes. Necesita: {amount}, Tiene: {_gems}");
            }

            return success;
        }

        /// <summary>
        /// Deducts coins and holds them in escrow for a bet
        /// </summary>
        public bool EscrowCoins(int amount)
        {
            if (amount <= 0) return true;

            int newCoins = 0;
            int deficit = 0;
            bool success;
            lock (_currencyLock)
            {
                if (_coins < amount)
                {
                    deficit = amount - _coins;
                    success = false;
                }
                else
                {
                    _coins -= amount;
                    _escrowedCoins = amount;
                    _escrowedGems = 0;
                    _escrowType = BetCurrencyType.DigitCoins;
                    newCoins = _coins;
                    success = true;
                }
            }

            if (success)
            {
                SaveCurrency();
                SaveEscrowPrefs(); // D-8: Persist escrow to survive crashes
                // Use captured newCoins (read inside lock) to avoid race condition
                OnCoinsChanged?.Invoke(newCoins, -amount);
                Debug.Log($"[CurrencyManager] Escrow: {amount} DigitCoins held for bet");
            }
            else
            {
                OnNotEnoughCoins?.Invoke(deficit);
                Debug.Log($"[CurrencyManager] Monedas insuficientes. Necesita: {amount}, Tiene: {_coins}");
            }

            return success;
        }

        /// <summary>
        /// Settles the bet. If won, returns 2x escrow. If lost, escrow is forfeited.
        /// </summary>
        public void SettleBet(bool won)
        {
            BetCurrencyType settledType;
            int winAmount = 0;
            int newBalance = 0;
            int escrowedAmount = 0;

            lock (_currencyLock)
            {
                if (_escrowType == BetCurrencyType.None) return;

                settledType = _escrowType;

                if (won)
                {
                    switch (_escrowType)
                    {
                        case BetCurrencyType.DigitGems:
                            winAmount = Mathf.RoundToInt(_escrowedGems * BET_MULTIPLIER);
                            escrowedAmount = _escrowedGems;
                            // SEC-M13: Overflow protection on winnings
                            if (_gems > int.MaxValue - winAmount)
                                _gems = int.MaxValue;
                            else
                                _gems += winAmount;
                            newBalance = _gems;
                            break;
                        case BetCurrencyType.DigitCoins:
                            winAmount = Mathf.RoundToInt(_escrowedCoins * BET_MULTIPLIER);
                            escrowedAmount = _escrowedCoins;
                            // SEC-M13: Overflow protection on winnings
                            if (_coins > int.MaxValue - winAmount)
                                _coins = int.MaxValue;
                            else
                                _coins += winAmount;
                            newBalance = _coins;
                            break;
                    }
                }
                else
                {
                    escrowedAmount = _escrowType == BetCurrencyType.DigitGems ? _escrowedGems : _escrowedCoins;
                }

                ClearEscrow();
            }

            if (won)
            {
                SaveCurrency();
                switch (settledType)
                {
                    case BetCurrencyType.DigitGems:
                        OnGemsChanged?.Invoke(newBalance, winAmount);
                        Debug.Log($"[CurrencyManager] Bet WON: +{winAmount} DigitGems (2x {escrowedAmount})");
                        break;
                    case BetCurrencyType.DigitCoins:
                        OnCoinsChanged?.Invoke(newBalance, winAmount);
                        Debug.Log($"[CurrencyManager] Bet WON: +{winAmount} DigitCoins (2x {escrowedAmount})");
                        break;
                }
            }
            else
            {
                Debug.Log($"[CurrencyManager] Bet LOST: escrow forfeited ({settledType}: {escrowedAmount})");
            }
        }

        /// <summary>
        /// Cancels the bet and returns the escrowed amount
        /// </summary>
        public void CancelEscrow()
        {
            BetCurrencyType cancelledType;
            int refundAmount = 0;
            int newBalance = 0;

            lock (_currencyLock)
            {
                if (_escrowType == BetCurrencyType.None) return;

                cancelledType = _escrowType;

                switch (_escrowType)
                {
                    case BetCurrencyType.DigitGems:
                        refundAmount = _escrowedGems;
                        // SEC-M13: Overflow protection on refund
                        if (_gems > int.MaxValue - _escrowedGems)
                            _gems = int.MaxValue;
                        else
                            _gems += _escrowedGems;
                        newBalance = _gems;
                        break;
                    case BetCurrencyType.DigitCoins:
                        refundAmount = _escrowedCoins;
                        // SEC-M13: Overflow protection on refund
                        if (_coins > int.MaxValue - _escrowedCoins)
                            _coins = int.MaxValue;
                        else
                            _coins += _escrowedCoins;
                        newBalance = _coins;
                        break;
                }

                ClearEscrow();
            }

            SaveCurrency();
            switch (cancelledType)
            {
                case BetCurrencyType.DigitGems:
                    OnGemsChanged?.Invoke(newBalance, refundAmount);
                    Debug.Log($"[CurrencyManager] Escrow cancelled: +{refundAmount} DigitGems returned");
                    break;
                case BetCurrencyType.DigitCoins:
                    OnCoinsChanged?.Invoke(newBalance, refundAmount);
                    Debug.Log($"[CurrencyManager] Escrow cancelled: +{refundAmount} DigitCoins returned");
                    break;
            }
        }

        private void ClearEscrow()
        {
            _escrowedGems = 0;
            _escrowedCoins = 0;
            _escrowType = BetCurrencyType.None;
            ClearEscrowPrefs();
        }

        /// <summary>
        /// D-8: Persist escrow state to PlayerPrefs so it survives app crashes
        /// </summary>
        private void SaveEscrowPrefs()
        {
            PlayerPrefs.SetInt(ESCROW_GEMS_KEY, _escrowedGems);
            PlayerPrefs.SetInt(ESCROW_COINS_KEY, _escrowedCoins);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// D-8: Clear persisted escrow state from PlayerPrefs
        /// </summary>
        private void ClearEscrowPrefs()
        {
            PlayerPrefs.DeleteKey(ESCROW_GEMS_KEY);
            PlayerPrefs.DeleteKey(ESCROW_COINS_KEY);
            PlayerPrefs.Save();
        }

        // ==================== DEBUG ====================

#if UNITY_EDITOR
        [ContextMenu("Debug: Add 1000 Gems")]
        private void DebugAdd1000Gems() => AddGems(1000);

        [ContextMenu("Debug: Add 10000 Coins")]
        private void DebugAdd10000Coins() => AddCoins(10000);

        [ContextMenu("Debug: Reset Currency")]
        private void DebugResetCurrency()
        {
            _gems = DEFAULT_GEMS;
            _coins = DEFAULT_COINS;
            SaveCurrency();
            OnGemsChanged?.Invoke(_gems, 0);
            OnCoinsChanged?.Invoke(_coins, 0);
            Debug.Log("[CurrencyManager] Currency reset to defaults");
        }

        [ContextMenu("Debug: Clear All Currency")]
        private void DebugClearCurrency()
        {
            _gems = 0;
            _coins = 0;
            SaveCurrency();
            OnGemsChanged?.Invoke(_gems, 0);
            OnCoinsChanged?.Invoke(_coins, 0);
            Debug.Log("[CurrencyManager] All currency cleared");
        }
#endif
    }
}
