using UnityEngine;
using System;
using System.Threading.Tasks;

namespace DigitPark.Monetization
{
    /// <summary>
    /// Tipo de moneda para apuestas
    /// </summary>
    public enum BetCurrencyType
    {
        None,
        Gems,
        Coins
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
        }

        // ==================== GEMS METHODS ====================

        /// <summary>
        /// Agrega gemas al balance
        /// </summary>
        public void AddGems(int amount)
        {
            if (amount <= 0) return;

            int previousAmount = _gems;
            _gems += amount;
            SaveCurrency();

            OnGemsChanged?.Invoke(_gems, amount);
            Debug.Log($"[CurrencyManager] +{amount} gemas (Total: {_gems})");
        }

        /// <summary>
        /// Gasta gemas si hay suficientes
        /// </summary>
        /// <returns>true si se pudieron gastar, false si no hay suficientes</returns>
        public bool SpendGems(int amount)
        {
            if (amount <= 0) return true;

            if (_gems < amount)
            {
                OnNotEnoughGems?.Invoke(amount - _gems);
                Debug.Log($"[CurrencyManager] Gemas insuficientes. Necesita: {amount}, Tiene: {_gems}");
                return false;
            }

            int previousAmount = _gems;
            _gems -= amount;
            SaveCurrency();

            OnGemsChanged?.Invoke(_gems, -amount);
            Debug.Log($"[CurrencyManager] -{amount} gemas (Total: {_gems})");
            return true;
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
            SceneNavigator.Instance.NavigateToShopForGems();
            return false;
        }

        // ==================== COINS METHODS ====================

        /// <summary>
        /// Agrega monedas al balance
        /// </summary>
        public void AddCoins(int amount)
        {
            if (amount <= 0) return;

            int previousAmount = _coins;
            _coins += amount;
            SaveCurrency();

            OnCoinsChanged?.Invoke(_coins, amount);
            Debug.Log($"[CurrencyManager] +{amount} monedas (Total: {_coins})");
        }

        /// <summary>
        /// Gasta monedas si hay suficientes
        /// </summary>
        /// <returns>true si se pudieron gastar, false si no hay suficientes</returns>
        public bool SpendCoins(int amount)
        {
            if (amount <= 0) return true;

            if (_coins < amount)
            {
                OnNotEnoughCoins?.Invoke(amount - _coins);
                Debug.Log($"[CurrencyManager] Monedas insuficientes. Necesita: {amount}, Tiene: {_coins}");
                return false;
            }

            int previousAmount = _coins;
            _coins -= amount;
            SaveCurrency();

            OnCoinsChanged?.Invoke(_coins, -amount);
            Debug.Log($"[CurrencyManager] -{amount} monedas (Total: {_coins})");
            return true;
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
            if (!SpendGems(amount)) return false;

            _escrowedGems = amount;
            _escrowedCoins = 0;
            _escrowType = BetCurrencyType.Gems;
            Debug.Log($"[CurrencyManager] Escrow: {amount} gems held for bet");
            return true;
        }

        /// <summary>
        /// Deducts coins and holds them in escrow for a bet
        /// </summary>
        public bool EscrowCoins(int amount)
        {
            if (amount <= 0) return true;
            if (!SpendCoins(amount)) return false;

            _escrowedCoins = amount;
            _escrowedGems = 0;
            _escrowType = BetCurrencyType.Coins;
            Debug.Log($"[CurrencyManager] Escrow: {amount} coins held for bet");
            return true;
        }

        /// <summary>
        /// Settles the bet. If won, returns 2x escrow. If lost, escrow is forfeited.
        /// </summary>
        public void SettleBet(bool won)
        {
            if (_escrowType == BetCurrencyType.None) return;

            if (won)
            {
                switch (_escrowType)
                {
                    case BetCurrencyType.Gems:
                        int gemsWon = _escrowedGems * 2;
                        AddGems(gemsWon);
                        Debug.Log($"[CurrencyManager] Bet WON: +{gemsWon} gems (2x {_escrowedGems})");
                        break;
                    case BetCurrencyType.Coins:
                        int coinsWon = _escrowedCoins * 2;
                        AddCoins(coinsWon);
                        Debug.Log($"[CurrencyManager] Bet WON: +{coinsWon} coins (2x {_escrowedCoins})");
                        break;
                }
            }
            else
            {
                Debug.Log($"[CurrencyManager] Bet LOST: escrow forfeited ({_escrowType}: {(_escrowType == BetCurrencyType.Gems ? _escrowedGems : _escrowedCoins)})");
            }

            ClearEscrow();
        }

        /// <summary>
        /// Cancels the bet and returns the escrowed amount
        /// </summary>
        public void CancelEscrow()
        {
            if (_escrowType == BetCurrencyType.None) return;

            switch (_escrowType)
            {
                case BetCurrencyType.Gems:
                    AddGems(_escrowedGems);
                    Debug.Log($"[CurrencyManager] Escrow cancelled: +{_escrowedGems} gems returned");
                    break;
                case BetCurrencyType.Coins:
                    AddCoins(_escrowedCoins);
                    Debug.Log($"[CurrencyManager] Escrow cancelled: +{_escrowedCoins} coins returned");
                    break;
            }

            ClearEscrow();
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
