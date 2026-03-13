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
        DigitCoins,
        DigitGems,
        RealMoney,
        Achievement,
        Secret
    }

    /// <summary>
    /// Datos de un marco de perfil
    /// </summary>
    /// <summary>
    /// Tipo de animación del marco. Usado por el renderer para elegir el shader/efecto correcto.
    /// </summary>
    public enum FrameAnimationType
    {
        None,
        Shimmer,          // Brillo sutil idle — para frames no-IAP
        ElectricArcs,     // Arcos de electricidad estática
        HueRotation,      // Cycling de color espectral completo
        AuroraRibbons,    // Cintas de aurora borealis
        StarParticles,    // Polvo estelar flotante
        LightningFlash,   // Destello de rayo aleatorio
        CrackGlow,        // Grieta con luz pulsante que escapa
        LayeredFire,      // Fuego multicapa con convección real
        GodRays,          // Rayos de luz divina rotantes
        GlitchBurst,      // Glitch digital con chromatic aberration
        SpectrumCycle,    // Iridiscencia espectral
        PlasmaFire,       // Llamas de plasma (violeta/cyan, no naranja)
        CrownPulse,       // Pulso de corona con partículas doradas
    }

    [Serializable]
    public class FrameData
    {
        public string frameId;
        public string nameKey;              // Localization key
        public FrameRarity rarity;
        public FramePriceType priceType;
        public int coinPrice;
        public int gemPrice;
        public float realMoneyPrice;
        public string achievementId;        // Required achievement (if achievement type)
        public Color primaryColor;
        public Color secondaryColor;
        public Color accentColor;           // Tercer color para efectos de animación complejos
        public bool isAnimated;
        public FrameAnimationType animationType;
        public int animationIntensity;      // 1=sutil, 2=medio, 3=alto impacto
    }
}
