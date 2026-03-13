using UnityEngine;
using DigitPark.Localization;
using DigitPark.Navigation;

namespace DigitPark.Monetization
{
    /// <summary>
    /// Tipo de item en la tienda
    /// </summary>
    public enum ShopItemType
    {
        DigitGemsPack,  // Paquete de DigitGems (compra con dinero real)
        DigitCoinsPack, // Paquete de DigitCoins (compra con DigitGems)
        Theme,          // Tema visual
        Avatar,         // Avatar
        SpecialOffer,   // Oferta especial (bundle)
        PremiumBundle,  // Bundle premium
        StarterPack,    // Paquete de inicio
        WinEffect,      // Efecto de victoria
        WinEffectBundle, // Bundle de efectos de victoria
        BattleCard      // BattleCard cosmético de matchmaking
    }

    /// <summary>
    /// Tipo de precio del item
    /// </summary>
    public enum PriceType
    {
        RealMoney,      // Dinero real (USD/MXN)
        DigitGems,      // DigitGems
        DigitCoins,     // DigitCoins
        Free            // Gratis
    }

    /// <summary>
    /// ScriptableObject que define un item de la tienda.
    /// Crear via: Assets > Create > DigitPark > Shop > Shop Item
    /// </summary>
    [CreateAssetMenu(fileName = "NewShopItem", menuName = "DigitPark/Shop/Shop Item", order = 1)]
    public class ShopItemData : ScriptableObject
    {
        [Header("Identification")]
        [Tooltip("ID unico del item (para analytics y backend)")]
        public string itemId;

        [Tooltip("Nombre mostrado en la UI")]
        public string displayName;

        [TextArea(2, 4)]
        [Tooltip("Descripcion del item")]
        public string description;

        [Header("Type")]
        public ShopItemType itemType = ShopItemType.DigitGemsPack;
        public ShopTab shopTab = ShopTab.Currency;

        [Header("Visuals")]
        public Sprite icon;
        public Color accentColor = Color.cyan;

        [Header("Pricing")]
        public PriceType priceType = PriceType.RealMoney;

        [Tooltip("Precio en dinero real (ej: 0.99, 4.99)")]
        public float realMoneyPrice;

        [Tooltip("Precio en DigitGems")]
        public int gemsPrice;

        [Tooltip("Precio en DigitCoins")]
        public int coinsPrice;

        [Tooltip("ID del producto IAP (para compras reales)")]
        public string iapProductId;

        [Header("Rewards")]
        [Tooltip("Cantidad de DigitGems que otorga")]
        public int gemsAmount;

        [Tooltip("Cantidad de DigitCoins que otorga")]
        public int coinsAmount;

        [Tooltip("Porcentaje de bonus (ej: 20 = +20%)")]
        [Range(0, 100)]
        public int bonusPercent;

        [Header("Theme/Avatar Data")]
        [Tooltip("ID del tema que desbloquea (si aplica)")]
        public string themeId;

        [Tooltip("ID del avatar que desbloquea (si aplica)")]
        public string avatarId;

        [Header("Special Offer Settings")]
        public bool isLimitedTime;
        public float offerDurationHours = 24f;

        [Tooltip("Precio original antes del descuento")]
        public float originalPrice;

        [Tooltip("Porcentaje de descuento mostrado")]
        [Range(0, 99)]
        public int discountPercent;

        [Header("Display Settings")]
        public bool isPopular;
        public bool isBestValue;
        public bool showBonusBadge = true;

        [Tooltip("Orden de display (menor = primero)")]
        public int sortOrder;

        // ==================== COMPUTED PROPERTIES ====================

        /// <summary>
        /// Obtiene el precio formateado como string
        /// </summary>
        public string GetFormattedPrice()
        {
            switch (priceType)
            {
                case PriceType.RealMoney:
                    return $"${realMoneyPrice:F2}";
                case PriceType.DigitGems:
                    return gemsPrice.ToString("N0");
                case PriceType.DigitCoins:
                    return coinsPrice.ToString("N0");
                case PriceType.Free:
                    return AutoLocalizer.Get("free_label");
                default:
                    return "";
            }
        }

        /// <summary>
        /// Obtiene el texto del bonus (ej: "+20%")
        /// </summary>
        public string GetBonusText()
        {
            if (bonusPercent > 0)
            {
                return $"+{bonusPercent}%";
            }
            return "";
        }

        /// <summary>
        /// Obtiene el total de DigitGems incluyendo bonus
        /// </summary>
        public int GetTotalGems()
        {
            if (gemsAmount > 0 && bonusPercent > 0)
            {
                int bonus = Mathf.RoundToInt(gemsAmount * (bonusPercent / 100f));
                return gemsAmount + bonus;
            }
            return gemsAmount;
        }

        /// <summary>
        /// Obtiene el total de DigitCoins incluyendo bonus
        /// </summary>
        public int GetTotalCoins()
        {
            if (coinsAmount > 0 && bonusPercent > 0)
            {
                int bonus = Mathf.RoundToInt(coinsAmount * (bonusPercent / 100f));
                return coinsAmount + bonus;
            }
            return coinsAmount;
        }

        /// <summary>
        /// Verifica si el jugador puede comprar este item
        /// </summary>
        public bool CanAfford()
        {
            var currency = CurrencyManager.Instance;
            if (currency == null) return false;

            switch (priceType)
            {
                case PriceType.DigitGems:
                    return currency.HasEnoughGems(gemsPrice);
                case PriceType.DigitCoins:
                    return currency.HasEnoughCoins(coinsPrice);
                case PriceType.Free:
                    return true;
                case PriceType.RealMoney:
                    return true; // IAP handles this
                default:
                    return false;
            }
        }

        /// <summary>
        /// Intenta comprar el item
        /// </summary>
        /// <returns>true si la compra fue exitosa</returns>
        public bool TryPurchase()
        {
            var currency = CurrencyManager.Instance;
            if (currency == null) return false;

            switch (priceType)
            {
                case PriceType.DigitGems:
                    if (!currency.SpendGems(gemsPrice)) return false;
                    break;

                case PriceType.DigitCoins:
                    if (!currency.SpendCoins(coinsPrice)) return false;
                    break;

                case PriceType.Free:
                    // Free items always succeed
                    break;

                case PriceType.RealMoney:
                    // IAP purchase - this should be handled externally
                    Debug.LogWarning("[ShopItemData] Real money purchases should use IAP system");
                    return false;
            }

            // Grant rewards
            GrantRewards();
            return true;
        }

        /// <summary>
        /// Otorga las recompensas del item
        /// </summary>
        public void GrantRewards()
        {
            var currency = CurrencyManager.Instance;
            if (currency == null) return;

            switch (itemType)
            {
                case ShopItemType.DigitGemsPack:
                    currency.ProcessGemsPurchase(gemsAmount, GetBonusGems());
                    break;

                case ShopItemType.DigitCoinsPack:
                    currency.AddCoins(GetTotalCoins());
                    break;

                case ShopItemType.Theme:
                    if (!string.IsNullOrEmpty(themeId))
                        DigitPark.Themes.ThemeManager.Instance?.UnlockTheme(themeId);
                    break;

                case ShopItemType.Avatar:
                    // TODO: Unlock avatar
                    Debug.Log($"[ShopItemData] Avatar desbloqueado: {avatarId}");
                    break;

                case ShopItemType.WinEffect:
                case ShopItemType.WinEffectBundle:
                    if (!string.IsNullOrEmpty(itemId))
                    {
                        var effectService = DigitPark.Services.VictoryEffectService.Instance;
                        if (effectService != null)
                            effectService.UnlockEffect(itemId);
                    }
                    break;

                case ShopItemType.BattleCard:
                    if (!string.IsNullOrEmpty(itemId))
                    {
                        var bcService = DigitPark.Cosmetics.BattleCardService.Instance;
                        if (bcService != null)
                            bcService.UnlockCard(itemId);
                    }
                    break;

                case ShopItemType.SpecialOffer:
                case ShopItemType.StarterPack:
                    // Bundle - grant multiple rewards
                    if (gemsAmount > 0) currency.AddGems(GetTotalGems());
                    if (coinsAmount > 0) currency.AddCoins(GetTotalCoins());
                    if (!string.IsNullOrEmpty(themeId))
                    {
                        DigitPark.Themes.ThemeManager.Instance?.UnlockTheme(themeId);
                    }
                    break;
            }

            Debug.Log($"[ShopItemData] Recompensas otorgadas para: {displayName}");
        }

        private int GetBonusGems()
        {
            if (bonusPercent > 0)
            {
                return Mathf.RoundToInt(gemsAmount * (bonusPercent / 100f));
            }
            return 0;
        }

        // ==================== VALIDATION ====================

        private void OnValidate()
        {
            // Auto-generate itemId if empty
            if (string.IsNullOrEmpty(itemId))
            {
                itemId = name.ToLower().Replace(" ", "_");
            }

            // Ensure proper tab assignment based on type
            switch (itemType)
            {
                case ShopItemType.DigitGemsPack:
                case ShopItemType.DigitCoinsPack:
                    shopTab = ShopTab.Currency;
                    if (itemType == ShopItemType.DigitCoinsPack)
                        priceType = PriceType.DigitGems;
                    break;
                case ShopItemType.Theme:
                case ShopItemType.Avatar:
                    shopTab = ShopTab.Styles;
                    break;
                case ShopItemType.SpecialOffer:
                case ShopItemType.PremiumBundle:
                case ShopItemType.StarterPack:
                    shopTab = ShopTab.Featured;
                    break;
                case ShopItemType.WinEffect:
                case ShopItemType.WinEffectBundle:
                    shopTab = ShopTab.Effects;
                    break;
                case ShopItemType.BattleCard:
                    shopTab = ShopTab.BattleCards;
                    break;
            }
        }
    }
}
