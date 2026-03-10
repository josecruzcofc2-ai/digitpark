using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DigitPark.Services;
using DigitPark.Services.Firebase;
using DigitPark.Navigation;

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
                if (_instance == null)
                {
                    _instance = FindObjectOfType<CurrencyManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("CurrencyManager");
                        _instance = go.AddComponent<CurrencyManager>();
                        DontDestroyOnLoad(go);
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

        // Valores iniciales para nuevos jugadores
        private const int DEFAULT_GEMS = 100;
        private const int DEFAULT_COINS = 1000;

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

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                LoadCurrency();
                Debug.Log($"[CurrencyManager] Iniciado - Gemas: {_gems}, Monedas: {_coins}");
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void LoadCurrency()
        {
            _gems = PlayerPrefs.GetInt(GEMS_KEY, DEFAULT_GEMS);
            _coins = PlayerPrefs.GetInt(COINS_KEY, DEFAULT_COINS);
        }

        private void SaveCurrency()
        {
            PlayerPrefs.SetInt(GEMS_KEY, _gems);
            PlayerPrefs.SetInt(COINS_KEY, _coins);
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

            int newGems;
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
                OnGemsChanged?.Invoke(_gems, -amount);
                Debug.Log($"[CurrencyManager] -{amount} gemas (Total: {_gems})");
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
                    success = true;
                }
            }

            if (success)
            {
                SaveCurrency();
                OnCoinsChanged?.Invoke(_coins, -amount);
                Debug.Log($"[CurrencyManager] -{amount} monedas (Total: {_coins})");
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
        /// Compra monedas con gemas
        /// </summary>
        /// <returns>true si la compra fue exitosa</returns>
        public bool PurchaseCoinsWithGems(int coinsAmount, int gemsPrice)
        {
            if (!SpendGems(gemsPrice))
            {
                return false;
            }

            AddCoins(coinsAmount);
            Debug.Log($"[CurrencyManager] Compra exitosa: {coinsAmount} monedas por {gemsPrice} gemas");
            return true;
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

            int newGems;
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
                OnGemsChanged?.Invoke(_gems, -amount);
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
                    success = true;
                }
            }

            if (success)
            {
                SaveCurrency();
                OnCoinsChanged?.Invoke(_coins, -amount);
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
                            winAmount = _escrowedGems * 2;
                            escrowedAmount = _escrowedGems;
                            // SEC-M13: Overflow protection on winnings
                            if (_gems > int.MaxValue - winAmount)
                                _gems = int.MaxValue;
                            else
                                _gems += winAmount;
                            newBalance = _gems;
                            break;
                        case BetCurrencyType.DigitCoins:
                            winAmount = _escrowedCoins * 2;
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
