using System;
using UnityEngine;

namespace DigitPark.Services
{
    /// <summary>
    /// Rareza del marco
    /// </summary>
    public enum FrameRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// Tipo de precio del marco
    /// </summary>
    public enum FramePriceType
    {
        Coins,
        Gems,
        RealMoney,
        Achievement,
        Secret
    }

    /// <summary>
    /// Datos de un marco de perfil
    /// </summary>
    [Serializable]
    public class FrameData
    {
        public string frameId;
        public string nameKey;          // Localization key
        public FrameRarity rarity;
        public FramePriceType priceType;
        public int coinPrice;
        public int gemPrice;
        public float realMoneyPrice;
        public string achievementId;    // Required achievement (if achievement type)
        public Color primaryColor;
        public Color secondaryColor;
        public bool isAnimated;
    }
}
