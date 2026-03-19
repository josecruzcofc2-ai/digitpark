#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using DigitPark.Monetization;
using DigitPark.Navigation;

namespace DigitPark.Editor
{
    /// <summary>
    /// E-05: Crea los 3 ShopItemData de tipo TemporaryDecoration en Resources/Shop/.
    /// Idempotente: si el asset ya existe, lo omite.
    /// Menú: DigitPark/Shop/Create Temporary Decoration Items
    /// </summary>
    public static class CreateTemporaryDecorationItems
    {
        private const string SHOP_DIR = "Assets/_Project/Resources/Shop";

        private struct DecoData
        {
            public string assetName;
            public string itemId;
            public string displayName;
            public string description;
            public int    coinsPrice;
            public int    durationDays;
        }

        private static readonly DecoData[] Items = new DecoData[]
        {
            new DecoData
            {
                assetName    = "DecorationNeonBorder",
                itemId       = "decoration_neon_border",
                displayName  = "Neon Border",
                description  = "Glowing neon border around your profile card. Active for 30 days.",
                coinsPrice   = 1000,
                durationDays = 30
            },
            new DecoData
            {
                assetName    = "DecorationGoldAura",
                itemId       = "decoration_gold_aura",
                displayName  = "Gold Aura",
                description  = "Prestigious gold aura effect on your profile. Active for 30 days.",
                coinsPrice   = 2500,
                durationDays = 30
            },
            new DecoData
            {
                assetName    = "DecorationChampionBadge",
                itemId       = "decoration_champion_badge",
                displayName  = "Champion Badge",
                description  = "Champion badge displayed on your profile. Active for 7 days.",
                coinsPrice   = 500,
                durationDays = 7
            }
        };

        [MenuItem("DigitPark/Shop/Create Temporary Decoration Items")]
        public static void CreateItems()
        {
            // Ensure Resources/Shop/ directory exists
            if (!AssetDatabase.IsValidFolder(SHOP_DIR))
            {
                string parent = "Assets/_Project/Resources";
                if (!AssetDatabase.IsValidFolder(parent))
                    AssetDatabase.CreateFolder("Assets/_Project", "Resources");
                AssetDatabase.CreateFolder(parent, "Shop");
                Debug.Log($"[DecoItems] Created folder: {SHOP_DIR}");
            }

            int created = 0;
            int skipped = 0;

            foreach (var data in Items)
            {
                string path = $"{SHOP_DIR}/{data.assetName}.asset";

                // Skip if already exists
                if (AssetDatabase.LoadAssetAtPath<ShopItemData>(path) != null)
                {
                    Debug.Log($"[DecoItems] Already exists, skipping: {data.assetName}");
                    skipped++;
                    continue;
                }

                var so = ScriptableObject.CreateInstance<ShopItemData>();
                so.itemId          = data.itemId;
                so.displayName     = data.displayName;
                so.description     = data.description;
                so.itemType        = ShopItemType.TemporaryDecoration;
                so.shopTab         = ShopTab.Styles;
                so.priceType       = PriceType.DigitCoins;
                so.coinsPrice      = data.coinsPrice;
                so.itemDurationDays = data.durationDays;
                so.sortOrder       = created;

                AssetDatabase.CreateAsset(so, path);
                EditorUtility.SetDirty(so);
                created++;
                Debug.Log($"[DecoItems] Created: {path}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Temporary Decoration Items",
                $"Done.\nCreated: {created}\nSkipped (already exist): {skipped}",
                "OK"
            );
        }
    }
}
#endif
