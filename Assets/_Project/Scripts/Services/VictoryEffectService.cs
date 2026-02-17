using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitPark.Services
{
    /// <summary>
    /// Tipo de precio del efecto de victoria
    /// </summary>
    public enum EffectPriceType
    {
        Free,
        Coins,
        Gems,
        RealMoney
    }

    /// <summary>
    /// Datos de un efecto de victoria
    /// </summary>
    [Serializable]
    public class VictoryEffectData
    {
        public string effectId;
        public string nameKey;          // Localization key
        public EffectPriceType priceType;
        public int coinPrice;
        public int gemPrice;
        public float realMoneyPrice;
        public Color primaryColor;
        public Color secondaryColor;
        public string particlePrefabName; // Resources path for particle prefab
    }

    /// <summary>
    /// Servicio singleton que gestiona efectos de victoria post-juego.
    /// 8 efectos con diferentes metodos de obtencion.
    /// Persistencia via PlayerPrefs.
    /// </summary>
    public class VictoryEffectService : MonoBehaviour
    {
        private static VictoryEffectService _instance;
        public static VictoryEffectService Instance => _instance;

        private const string OWNED_EFFECTS_KEY = "OwnedVictoryEffects";
        private const string EQUIPPED_EFFECT_KEY = "EquippedVictoryEffect";
        private const string DEFAULT_EFFECT_ID = "confetti";

        private HashSet<string> _ownedEffects = new HashSet<string>();
        private string _equippedEffectId;
        private List<VictoryEffectData> _allEffects = new List<VictoryEffectData>();

        public event Action<string> OnEffectChanged;
        public event Action<string> OnEffectUnlocked;

        public List<VictoryEffectData> AllEffects => _allEffects;
        public string EquippedEffectId => _equippedEffectId;

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
            SetupEffects();
            LoadState();

            if (!_ownedEffects.Contains(DEFAULT_EFFECT_ID))
            {
                _ownedEffects.Add(DEFAULT_EFFECT_ID);
                SaveOwnedEffects();
            }

            if (string.IsNullOrEmpty(_equippedEffectId))
            {
                _equippedEffectId = DEFAULT_EFFECT_ID;
                SaveEquippedEffect();
            }

            Debug.Log($"[VictoryEffectService] Initialized: {_ownedEffects.Count} owned, equipped: {_equippedEffectId}");
        }

        private void SetupEffects()
        {
            _allEffects.Add(new VictoryEffectData
            {
                effectId = "confetti",
                nameKey = "effect_confetti",
                priceType = EffectPriceType.Free,
                primaryColor = new Color(1f, 0.8f, 0f),
                secondaryColor = new Color(0f, 0.8f, 1f),
                particlePrefabName = "Effects/Confetti"
            });

            _allEffects.Add(new VictoryEffectData
            {
                effectId = "fireworks",
                nameKey = "effect_fireworks",
                priceType = EffectPriceType.Coins,
                coinPrice = 2000,
                primaryColor = new Color(1f, 0.3f, 0.1f),
                secondaryColor = new Color(1f, 0.8f, 0f),
                particlePrefabName = "Effects/Fireworks"
            });

            _allEffects.Add(new VictoryEffectData
            {
                effectId = "lightning",
                nameKey = "effect_lightning",
                priceType = EffectPriceType.Coins,
                coinPrice = 5000,
                primaryColor = new Color(0.3f, 0.6f, 1f),
                secondaryColor = new Color(0.8f, 0.9f, 1f),
                particlePrefabName = "Effects/Lightning"
            });

            _allEffects.Add(new VictoryEffectData
            {
                effectId = "gold_rain",
                nameKey = "effect_gold_rain",
                priceType = EffectPriceType.Gems,
                gemPrice = 300,
                primaryColor = new Color(1f, 0.84f, 0f),
                secondaryColor = new Color(0.85f, 0.65f, 0f),
                particlePrefabName = "Effects/GoldRain"
            });

            _allEffects.Add(new VictoryEffectData
            {
                effectId = "neon_explosion",
                nameKey = "effect_neon_explosion",
                priceType = EffectPriceType.Gems,
                gemPrice = 500,
                primaryColor = new Color(0f, 1f, 1f),
                secondaryColor = new Color(1f, 0f, 1f),
                particlePrefabName = "Effects/NeonExplosion"
            });

            _allEffects.Add(new VictoryEffectData
            {
                effectId = "rainbow",
                nameKey = "effect_rainbow",
                priceType = EffectPriceType.Gems,
                gemPrice = 750,
                primaryColor = new Color(1f, 0f, 0f),
                secondaryColor = new Color(0.5f, 0f, 1f),
                particlePrefabName = "Effects/Rainbow"
            });

            _allEffects.Add(new VictoryEffectData
            {
                effectId = "crown_drop",
                nameKey = "effect_crown_drop",
                priceType = EffectPriceType.RealMoney,
                realMoneyPrice = 1.99f,
                primaryColor = new Color(1f, 0.84f, 0f),
                secondaryColor = new Color(0.6f, 0.2f, 0.8f),
                particlePrefabName = "Effects/CrownDrop"
            });

            _allEffects.Add(new VictoryEffectData
            {
                effectId = "fire_ring",
                nameKey = "effect_fire_ring",
                priceType = EffectPriceType.RealMoney,
                realMoneyPrice = 2.99f,
                primaryColor = new Color(1f, 0.4f, 0f),
                secondaryColor = new Color(1f, 0.8f, 0f),
                particlePrefabName = "Effects/FireRing"
            });
        }

        // ==================== PERSISTENCE ====================

        private void LoadState()
        {
            string ownedJson = PlayerPrefs.GetString(OWNED_EFFECTS_KEY, "");
            if (!string.IsNullOrEmpty(ownedJson))
            {
                try
                {
                    var data = JsonUtility.FromJson<StringListWrapper>(ownedJson);
                    if (data?.items != null)
                    {
                        foreach (var id in data.items)
                            _ownedEffects.Add(id);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[VictoryEffectService] Error loading: {e.Message}");
                }
            }

            _equippedEffectId = PlayerPrefs.GetString(EQUIPPED_EFFECT_KEY, DEFAULT_EFFECT_ID);
        }

        private void SaveOwnedEffects()
        {
            var data = new StringListWrapper { items = new List<string>(_ownedEffects) };
            PlayerPrefs.SetString(OWNED_EFFECTS_KEY, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        private void SaveEquippedEffect()
        {
            PlayerPrefs.SetString(EQUIPPED_EFFECT_KEY, _equippedEffectId);
            PlayerPrefs.Save();
        }

        // ==================== PUBLIC API ====================

        public bool IsOwned(string effectId) => _ownedEffects.Contains(effectId);

        public void UnlockEffect(string effectId)
        {
            if (_ownedEffects.Contains(effectId)) return;
            _ownedEffects.Add(effectId);
            SaveOwnedEffects();
            Debug.Log($"[VictoryEffectService] Effect unlocked: {effectId}");
            OnEffectUnlocked?.Invoke(effectId);
        }

        public void EquipEffect(string effectId)
        {
            if (!_ownedEffects.Contains(effectId)) return;
            _equippedEffectId = effectId;
            SaveEquippedEffect();
            Debug.Log($"[VictoryEffectService] Effect equipped: {effectId}");
            OnEffectChanged?.Invoke(effectId);
        }

        public VictoryEffectData GetEffectData(string effectId) => _allEffects.Find(e => e.effectId == effectId);
        public VictoryEffectData GetEquippedEffect() => GetEffectData(_equippedEffectId);

        /// <summary>
        /// Attempts to purchase a victory effect with currency.
        /// </summary>
        public bool TryPurchaseEffect(string effectId)
        {
            if (IsOwned(effectId)) return false;

            var effect = GetEffectData(effectId);
            if (effect == null) return false;

            var currency = DigitPark.Monetization.CurrencyManager.Instance;
            if (currency == null) return false;

            switch (effect.priceType)
            {
                case EffectPriceType.Free:
                    break;
                case EffectPriceType.Coins:
                    if (!currency.SpendCoins(effect.coinPrice)) return false;
                    break;
                case EffectPriceType.Gems:
                    if (!currency.SpendGems(effect.gemPrice)) return false;
                    break;
                case EffectPriceType.RealMoney:
                    Debug.LogWarning("[VictoryEffectService] Real money effects should use IAP system");
                    return false;
            }

            UnlockEffect(effectId);
            return true;
        }

        /// <summary>
        /// Plays the equipped victory effect at the specified position
        /// </summary>
        public void PlayEquippedEffect(Vector3 position)
        {
            var effect = GetEquippedEffect();
            if (effect == null) return;

            // Try to load and instantiate particle prefab
            if (!string.IsNullOrEmpty(effect.particlePrefabName))
            {
                var prefab = Resources.Load<GameObject>(effect.particlePrefabName);
                if (prefab != null)
                {
                    var instance = Instantiate(prefab, position, Quaternion.identity);
                    Destroy(instance, 5f); // Auto-cleanup after 5 seconds
                    Debug.Log($"[VictoryEffectService] Playing effect: {effect.effectId}");
                }
                else
                {
                    Debug.Log($"[VictoryEffectService] Effect prefab not found: {effect.particlePrefabName} (will be created later)");
                }
            }
        }

        // ==================== DEBUG ====================

#if UNITY_EDITOR
        [ContextMenu("Debug: Unlock All Effects")]
        private void DebugUnlockAll()
        {
            foreach (var effect in _allEffects)
                UnlockEffect(effect.effectId);
        }

        [ContextMenu("Debug: Reset Effects")]
        private void DebugReset()
        {
            _ownedEffects.Clear();
            _ownedEffects.Add(DEFAULT_EFFECT_ID);
            _equippedEffectId = DEFAULT_EFFECT_ID;
            SaveOwnedEffects();
            SaveEquippedEffect();
            Debug.Log("[VictoryEffectService] Reset to defaults");
        }
#endif
    }
}
