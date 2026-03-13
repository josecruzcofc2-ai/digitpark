using UnityEngine;
using UnityEditor;
using DigitPark.Cosmetics;

namespace DigitPark.Editor
{
    /// <summary>
    /// Genera los 19 BattleCard ScriptableObject assets en Resources/BattleCards/.
    /// Menu: DigitPark/BattleCards/Build All BattleCard Assets
    /// </summary>
    public static class BattleCardCatalogBuilder
    {
        private static readonly Color CYAN = new Color(0f, 0.898f, 1f);        // #00E5FF

        [MenuItem("DigitPark/BattleCards/Build All BattleCard Assets")]
        public static void BuildAll()
        {
            string dir = "Assets/_Project/Resources/BattleCards";
            if (!AssetDatabase.IsValidFolder(dir))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Project/Resources"))
                    AssetDatabase.CreateFolder("Assets/_Project", "Resources");
                AssetDatabase.CreateFolder("Assets/_Project/Resources", "BattleCards");
            }

            int count = 0;

            // ── TIER 0 — DEFAULT ──
            count += Create(dir, "battlecard_neon_core", "battlecard_neon_core_name",
                BattleCardCategory.Free, 0, 0, null,
                HexColor("#101428"), CYAN, Color.clear, false,
                2f, true, 0.4f, false, BorderAnimationType.None,
                CYAN, false, Color.white, false, 0, Color.clear, null);

            // ── TIER 1 — F2P (DC) ──
            count += Create(dir, "battlecard_solar_flare", "battlecard_solar_flare_name",
                BattleCardCategory.DC, 2000, 0, null,
                HexColor("#0D0805"), HexColor("#FF6B35"), Color.clear, false,
                3f, true, 0.6f, false, BorderAnimationType.None,
                HexColor("#FF6B35"), false, Color.white, false, 0, Color.clear, null);

            count += Create(dir, "battlecard_deep_violet", "battlecard_deep_violet_name",
                BattleCardCategory.DC, 3000, 0, null,
                HexColor("#0A0614"), HexColor("#BF00FF"), Color.clear, false,
                3f, true, 0.7f, false, BorderAnimationType.None,
                HexColor("#BF00FF"), false, Color.white, true, 0.5f, HexColor("#BF00FF"), null);

            count += Create(dir, "battlecard_arctic_edge", "battlecard_arctic_edge_name",
                BattleCardCategory.DC, 4000, 0, null,
                HexColor("#060A12"), HexColor("#E8F4FF"), HexColor("#00BBFF"), true,
                2.5f, true, 0.55f, false, BorderAnimationType.None,
                HexColor("#E8F4FF"), false, HexColor("#E8F4FF"), false, 0, Color.clear, null);

            count += Create(dir, "battlecard_gold_rush", "battlecard_gold_rush_name",
                BattleCardCategory.DC, 5000, 0, null,
                HexColor("#0C0800"), HexColor("#FFD700"), Color.clear, false,
                3.5f, true, 0.75f, false, BorderAnimationType.None,
                HexColor("#FFD700"), false, HexColor("#FFF8B0"), true, 0.3f, HexColor("#FFD700"), null);

            // ── TIER 2 — PREMIUM (DG Standard) ──
            count += Create(dir, "battlecard_void_circuit", "battlecard_void_circuit_name",
                BattleCardCategory.DGStandard, 0, 200, null,
                HexColor("#06071A"), HexColor("#BF00FF"), CYAN, true,
                4f, true, 0.85f, false, BorderAnimationType.None,
                HexColor("#BF00FF"), false, Color.white, true, 0.5f, HexColor("#BF00FF"), null);

            count += Create(dir, "battlecard_phoenix_protocol", "battlecard_phoenix_protocol_name",
                BattleCardCategory.DGStandard, 0, 250, null,
                HexColor("#140602"), HexColor("#FF6B35"), HexColor("#FFD700"), true,
                4f, true, 0.8f, false, BorderAnimationType.None,
                HexColor("#FF6B35"), false, HexColor("#FFF0DC"), true, 0.3f, HexColor("#FF6B35"), null);

            count += Create(dir, "battlecard_stellar_drift", "battlecard_stellar_drift_name",
                BattleCardCategory.DGStandard, 0, 300, null,
                HexColor("#04060F"), HexColor("#4A90FF"), HexColor("#BF00FF"), true,
                3.5f, true, 0.7f, false, BorderAnimationType.None,
                HexColor("#4A90FF"), false, HexColor("#DDEEFF"), true, 0.3f, HexColor("#4A90FF"), null);

            count += Create(dir, "battlecard_iron_protocol", "battlecard_iron_protocol_name",
                BattleCardCategory.DGStandard, 0, 350, null,
                HexColor("#080810"), HexColor("#C0C0FF"), HexColor("#6090FF"), true,
                5f, true, 0.8f, false, BorderAnimationType.None,
                HexColor("#C0C0FF"), false, HexColor("#E8F0FF"), true, 0.3f, HexColor("#6090FF"), null);

            count += Create(dir, "battlecard_neon_sakura", "battlecard_neon_sakura_name",
                BattleCardCategory.DGStandard, 0, 350, null,
                HexColor("#120510"), HexColor("#FF6CA8"), HexColor("#FF3860"), true,
                3f, true, 0.75f, false, BorderAnimationType.None,
                HexColor("#FF6CA8"), false, HexColor("#FFE0EE"), true, 0.3f, HexColor("#FF6CA8"), null);

            count += Create(dir, "battlecard_obsidian_edge", "battlecard_obsidian_edge_name",
                BattleCardCategory.DGStandard, 0, 400, null,
                HexColor("#030305"), HexColor("#E8F4FF"), CYAN, true,
                3f, true, 1f, false, BorderAnimationType.None,
                HexColor("#E8F4FF"), false, Color.white, true, 0.5f, CYAN, null);

            count += Create(dir, "battlecard_cyber_monarch", "battlecard_cyber_monarch_name",
                BattleCardCategory.DGStandard, 0, 500, null,
                HexColor("#090600"), HexColor("#FFD700"), HexColor("#FF6B35"), true,
                5f, true, 0.9f, false, BorderAnimationType.None,
                HexColor("#FFD700"), false, HexColor("#FFF0A0"), true, 0.5f, HexColor("#FFD700"), null);

            // ── TIER 3 — ELITE (DG Premium, animados) ──
            count += Create(dir, "battlecard_infernal_circuit", "battlecard_infernal_circuit_name",
                BattleCardCategory.DGPremium, 0, 800, null,
                HexColor("#0F0200"), HexColor("#FF3860"), HexColor("#FF6B35"), false,
                5f, true, 0.9f, true, BorderAnimationType.Electric,
                HexColor("#FF3860"), true, Color.white, true, 0.5f, HexColor("#FF3860"), null);

            count += Create(dir, "battlecard_quantum_ghost", "battlecard_quantum_ghost_name",
                BattleCardCategory.DGPremium, 0, 1000, null,
                HexColor("#050A10"), CYAN, HexColor("#BF00FF"), true,
                3f, true, 0.8f, true, BorderAnimationType.DataStream,
                CYAN, true, Color.white, false, 0, Color.clear, null);

            count += Create(dir, "battlecard_void_sovereign", "battlecard_void_sovereign_name",
                BattleCardCategory.DGPremium, 0, 1500, null,
                HexColor("#020008"), HexColor("#BF00FF"), HexColor("#FFD700"), true,
                5f, true, 1f, true, BorderAnimationType.Pulse,
                HexColor("#BF00FF"), true, Color.white, true, 0.5f, HexColor("#FFD700"), null);

            count += Create(dir, "battlecard_zero_kelvin", "battlecard_zero_kelvin_name",
                BattleCardCategory.DGPremium, 0, 800, null,
                HexColor("#020810"), HexColor("#E8F4FF"), HexColor("#00AAFF"), true,
                4f, true, 0.7f, true, BorderAnimationType.Pulse,
                HexColor("#E8F4FF"), true, HexColor("#E8F4FF"), false, 0, Color.clear, null);

            // ── TIER 4 — LEGEND (Earn) ──
            count += Create(dir, "battlecard_the_unbroken", "battlecard_the_unbroken_name",
                BattleCardCategory.Earn, 0, 0, "days_365",
                HexColor("#020010"), Color.white, HexColor("#FFD700"), true,
                6f, true, 1f, true, BorderAnimationType.DataStream,
                Color.white, true, Color.white, true, 0.5f, HexColor("#FFD700"), null);

            count += Create(dir, "battlecard_the_relentless", "battlecard_the_relentless_name",
                BattleCardCategory.Earn, 0, 0, "wins_500",
                HexColor("#0A0100"), HexColor("#FF3860"), HexColor("#FFD700"), true,
                5f, true, 1f, true, BorderAnimationType.Flame,
                HexColor("#FF3860"), true, Color.white, true, 0.7f, HexColor("#FF3860"), null);

            count += Create(dir, "battlecard_the_prodigy", "battlecard_the_prodigy_name",
                BattleCardCategory.Earn, 0, 0, "perfect_game",
                HexColor("#090600"), HexColor("#FFD700"), HexColor("#E8F4FF"), true,
                4f, true, 0.85f, true, BorderAnimationType.Pulse,
                HexColor("#FFD700"), true, HexColor("#FFF8B0"), true, 0.4f, HexColor("#FFD700"), null);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[BattleCardCatalogBuilder] {count} BattleCard assets creados en {dir}");
        }

        private static int Create(string dir, string cardId, string nameKey,
            BattleCardCategory category, int dcPrice, int dgPrice, string earnAchId,
            Color bgColor, Color accentPrimary, Color accentSecondary, bool useGradient,
            float borderWidth, bool hasBorderGlow, float borderGlowIntensity,
            bool isAnimatedBorder, BorderAnimationType borderAnim,
            Color avatarFrameColor, bool isAnimatedAvatarFrame,
            Color nameTextColor, bool nameHasGlow, float nameGlowIntensity, Color nameGlowColor,
            string descKey)
        {
            string path = $"{dir}/{cardId}.asset";

            var existing = AssetDatabase.LoadAssetAtPath<BattleCardData>(path);
            BattleCardData card;
            if (existing != null)
            {
                card = existing;
            }
            else
            {
                card = ScriptableObject.CreateInstance<BattleCardData>();
                AssetDatabase.CreateAsset(card, path);
            }

            card.cardId = cardId;
            card.displayNameKey = nameKey;
            card.category = category;
            card.dcPrice = dcPrice;
            card.dgPrice = dgPrice;
            card.earnAchievementId = earnAchId;
            card.cardBackgroundColor = bgColor;
            card.accentPrimary = accentPrimary;
            card.accentSecondary = accentSecondary;
            card.useGradientAccent = useGradient;
            card.borderWidth = borderWidth;
            card.hasBorderGlow = hasBorderGlow;
            card.borderGlowIntensity = borderGlowIntensity;
            card.isAnimatedBorder = isAnimatedBorder;
            card.borderAnimation = borderAnim;
            card.avatarFrameColor = avatarFrameColor;
            card.isAnimatedAvatarFrame = isAnimatedAvatarFrame;
            card.nameTextColor = nameTextColor;
            card.nameHasGlow = nameHasGlow;
            card.nameGlowIntensity = nameGlowIntensity;
            card.nameGlowColor = nameGlowColor;
            card.descriptionLocKey = descKey;

            EditorUtility.SetDirty(card);
            return 1;
        }

        private static Color HexColor(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color color);
            return color;
        }
    }
}
