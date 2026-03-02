using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;
using DigitPark.Monetization;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para conectar automaticamente las referencias
    /// de la escena Shop V4 (scroll continuo) con el ShopManager y otros componentes.
    /// </summary>
    public class ShopSceneConnector : EditorWindow
    {
        [MenuItem("DigitPark/Shop/Connect Shop References", false, 700)]
        public static void ConnectReferences()
        {
            if (!EditorUtility.DisplayDialog("Shop Scene Connector",
                "Esto conectara automaticamente las referencias de la escena Shop V4.\nAsegurate de tener la escena Shop abierta.\n\nContinuar?",
                "Si", "No"))
                return;

            ConnectShopReferences();
        }

        [MenuItem("DigitPark/Shop/Setup Shop Manager", false, 701)]
        public static void SetupShopManager()
        {
            ShopManager manager = Object.FindObjectOfType<ShopManager>();

            if (manager == null)
            {
                Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
                if (canvas == null)
                {
                    Debug.LogError("[ShopConnector] No se encontro Canvas en la escena");
                    return;
                }

                GameObject managerObj = new GameObject("ShopManager");
                managerObj.transform.SetParent(canvas.transform);
                manager = managerObj.AddComponent<ShopManager>();

                Debug.Log("[ShopConnector] ShopManager creado");
            }

            ConnectShopManagerReferences(manager);
            Selection.activeGameObject = manager.gameObject;
        }

        [MenuItem("DigitPark/Shop/Create Shop Item Data Assets", false, 702)]
        public static void CreateShopItemDataAssets()
        {
            string folderPath = "Assets/_Project/Data/Monetization/ShopItems";

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                System.IO.Directory.CreateDirectory(Application.dataPath + "/_Project/Data/Monetization/ShopItems");
                AssetDatabase.Refresh();
            }

            CreateGemPackAssets(folderPath);
            CreateCoinPackAssets(folderPath);
            CreateThemeAssets(folderPath);
            CreateFeaturedAssets(folderPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[ShopConnector] Shop Item Data assets created in: " + folderPath);
            EditorUtility.DisplayDialog("Shop Items Created",
                $"Se crearon los assets de items en:\n{folderPath}",
                "OK");
        }

        private static void CreateGemPackAssets(string folderPath)
        {
            var gemPacks = new (string id, string name, int gems, float price, int bonus, bool popular, bool bestValue)[]
            {
                ("gems_100", "100 DigitGems", 100, 0.99f, 0, false, false),
                ("gems_500", "500 DigitGems", 500, 4.99f, 10, false, false),
                ("gems_1200", "1,200 DigitGems", 1200, 9.99f, 20, true, true),
                ("gems_2500", "2,500 DigitGems", 2500, 19.99f, 25, false, false),
                ("gems_6500", "6,500 DigitGems", 6500, 49.99f, 30, false, false),
                ("gems_14000", "14,000 DigitGems", 14000, 99.99f, 35, false, false),
            };

            string iconBasePath = "Assets/_Project/Art/Icons/Currency";

            foreach (var pack in gemPacks)
            {
                string assetPath = $"{folderPath}/{pack.id}.asset";

                if (AssetDatabase.LoadAssetAtPath<ShopItemData>(assetPath) != null)
                    continue;

                ShopItemData item = ScriptableObject.CreateInstance<ShopItemData>();
                item.itemId = pack.id;
                item.displayName = pack.name;
                item.description = $"Get {pack.gems} DigitGems";
                item.itemType = ShopItemType.DigitGemsPack;
                item.shopTab = ShopTab.Currency;
                item.priceType = PriceType.RealMoney;
                item.realMoneyPrice = pack.price;
                item.iapProductId = $"com.matrixsoftware.digitpark.{pack.id}";
                item.gemsAmount = pack.gems;
                item.bonusPercent = pack.bonus;
                item.isPopular = pack.popular;
                item.isBestValue = pack.bestValue;
                item.accentColor = new Color(0.4f, 0.8f, 1f, 1f);
                item.sortOrder = System.Array.IndexOf(gemPacks, pack);

                string iconPath = $"{iconBasePath}/icon_digitgem_pack_{pack.gems}.png";
                Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
                if (iconSprite != null)
                    item.icon = iconSprite;
                else
                    Debug.LogWarning($"[ShopConnector] Icon not found: {iconPath}");

                AssetDatabase.CreateAsset(item, assetPath);
                Debug.Log($"[ShopConnector] Created: {assetPath}");
            }
        }

        private static void CreateCoinPackAssets(string folderPath)
        {
            var coinPacks = new (string id, string name, int coins, int gemsPrice, int bonus, bool popular, bool bestValue)[]
            {
                ("coins_1000", "1,000 DigitCoins", 1000, 50, 0, false, false),
                ("coins_5000", "5,000 DigitCoins", 5000, 200, 25, false, false),
                ("coins_15000", "15,000 DigitCoins", 15000, 500, 50, true, true),
                ("coins_50000", "50,000 DigitCoins", 50000, 1500, 75, false, false),
            };

            string iconBasePath = "Assets/_Project/Art/Icons/Currency";

            foreach (var pack in coinPacks)
            {
                string assetPath = $"{folderPath}/{pack.id}.asset";

                if (AssetDatabase.LoadAssetAtPath<ShopItemData>(assetPath) != null)
                    continue;

                ShopItemData item = ScriptableObject.CreateInstance<ShopItemData>();
                item.itemId = pack.id;
                item.displayName = pack.name;
                item.description = $"Get {pack.coins} DigitCoins";
                item.itemType = ShopItemType.DigitCoinsPack;
                item.shopTab = ShopTab.Currency;
                item.priceType = PriceType.DigitGems;
                item.gemsPrice = pack.gemsPrice;
                item.coinsAmount = pack.coins;
                item.bonusPercent = pack.bonus;
                item.isPopular = pack.popular;
                item.isBestValue = pack.bestValue;
                item.accentColor = new Color(1f, 0.85f, 0.3f, 1f);
                item.sortOrder = System.Array.IndexOf(coinPacks, pack);

                string iconPath = $"{iconBasePath}/icon_digitcoin_pack_{pack.coins}.png";
                Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
                if (iconSprite != null)
                    item.icon = iconSprite;
                else
                    Debug.LogWarning($"[ShopConnector] Icon not found: {iconPath}");

                AssetDatabase.CreateAsset(item, assetPath);
                Debug.Log($"[ShopConnector] Created: {assetPath}");
            }
        }

        private static void CreateThemeAssets(string folderPath)
        {
            var themes = new (string id, string name, float price)[]
            {
                ("theme_volcano", "Volcano", 2.50f),
                ("theme_ocean", "Ocean", 2.50f),
                ("theme_clean_light", "Clean Light", 2.50f),
                ("theme_retro_arcade", "Retro Arcade", 2.50f),
                ("theme_cyberpunk", "Cyberpunk", 2.50f),
                ("theme_minimalist", "Minimalist", 2.50f),
                ("theme_forest", "Forest", 2.50f),
                ("theme_vaporwave", "Vaporwave", 2.50f),
                ("theme_steampunk", "Steampunk", 2.50f),
            };

            foreach (var theme in themes)
            {
                string assetPath = $"{folderPath}/{theme.id}.asset";
                if (AssetDatabase.LoadAssetAtPath<ShopItemData>(assetPath) != null) continue;

                ShopItemData item = ScriptableObject.CreateInstance<ShopItemData>();
                item.itemId = theme.id;
                item.displayName = theme.name;
                item.description = $"Visual theme: {theme.name}";
                item.itemType = ShopItemType.Theme;
                item.shopTab = ShopTab.Styles;
                item.priceType = PriceType.RealMoney;
                item.realMoneyPrice = theme.price;
                item.iapProductId = $"com.matrixsoftware.digitpark.{theme.id}";
                item.themeId = theme.id.Replace("theme_", "");
                item.accentColor = new Color(0.6f, 0.3f, 0.9f, 1f);
                item.sortOrder = System.Array.IndexOf(themes, theme);

                AssetDatabase.CreateAsset(item, assetPath);
                Debug.Log($"[ShopConnector] Created: {assetPath}");
            }

            // Theme Bundle
            string bundlePath = $"{folderPath}/theme_bundle_all.asset";
            if (AssetDatabase.LoadAssetAtPath<ShopItemData>(bundlePath) == null)
            {
                ShopItemData bundle = ScriptableObject.CreateInstance<ShopItemData>();
                bundle.itemId = "theme_bundle_all";
                bundle.displayName = "All Themes";
                bundle.description = "9 premium themes - Save $7.50!";
                bundle.itemType = ShopItemType.PremiumBundle;
                bundle.shopTab = ShopTab.Featured;
                bundle.priceType = PriceType.RealMoney;
                bundle.realMoneyPrice = 14.99f;
                bundle.originalPrice = 22.50f;
                bundle.discountPercent = 33;
                bundle.iapProductId = "com.matrixsoftware.digitpark.theme_bundle_all";
                bundle.accentColor = new Color(0.6f, 0.3f, 0.9f, 1f);

                AssetDatabase.CreateAsset(bundle, bundlePath);
                Debug.Log($"[ShopConnector] Created: {bundlePath}");
            }
        }

        private static void CreateFeaturedAssets(string folderPath)
        {
            // Starter Pack
            string starterPath = $"{folderPath}/starter_pack.asset";
            if (AssetDatabase.LoadAssetAtPath<ShopItemData>(starterPath) == null)
            {
                ShopItemData starter = ScriptableObject.CreateInstance<ShopItemData>();
                starter.itemId = "starter_pack";
                starter.displayName = "Starter Pack";
                starter.description = "500 DigitGems + 5,000 DigitCoins + 1 Random Theme";
                starter.itemType = ShopItemType.StarterPack;
                starter.shopTab = ShopTab.Featured;
                starter.priceType = PriceType.RealMoney;
                starter.realMoneyPrice = 2.99f;
                starter.originalPrice = 9.99f;
                starter.discountPercent = 70;
                starter.gemsAmount = 500;
                starter.coinsAmount = 5000;
                starter.iapProductId = "com.matrixsoftware.digitpark.starter_pack";
                starter.accentColor = new Color(0.2f, 0.8f, 0.4f, 1f);
                starter.sortOrder = 0;

                AssetDatabase.CreateAsset(starter, starterPath);
                Debug.Log($"[ShopConnector] Created: {starterPath}");
            }

            // Weekly Deal
            string weeklyPath = $"{folderPath}/weekly_deal.asset";
            if (AssetDatabase.LoadAssetAtPath<ShopItemData>(weeklyPath) == null)
            {
                ShopItemData weekly = ScriptableObject.CreateInstance<ShopItemData>();
                weekly.itemId = "weekly_deal";
                weekly.displayName = "Weekly Deal";
                weekly.description = "1,200 DigitGems + 10,000 DigitCoins";
                weekly.itemType = ShopItemType.SpecialOffer;
                weekly.shopTab = ShopTab.Featured;
                weekly.priceType = PriceType.RealMoney;
                weekly.realMoneyPrice = 4.99f;
                weekly.originalPrice = 12.99f;
                weekly.discountPercent = 60;
                weekly.isLimitedTime = true;
                weekly.offerDurationHours = 168f;
                weekly.gemsAmount = 1200;
                weekly.coinsAmount = 10000;
                weekly.iapProductId = "com.matrixsoftware.digitpark.weekly_deal";
                weekly.accentColor = new Color(0.6f, 0.3f, 0.9f, 1f);
                weekly.sortOrder = 1;

                AssetDatabase.CreateAsset(weekly, weeklyPath);
                Debug.Log($"[ShopConnector] Created: {weeklyPath}");
            }
        }

        private static void ConnectShopReferences()
        {
            ShopManager manager = Object.FindObjectOfType<ShopManager>();

            if (manager == null)
            {
                Debug.LogWarning("[ShopConnector] No se encontro ShopManager. Usa 'Setup Shop Manager' primero.");
                return;
            }

            ConnectShopManagerReferences(manager);

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("[ShopConnector] Referencias conectadas exitosamente");
        }

        private static void ConnectShopManagerReferences(ShopManager manager)
        {
            SerializedObject serializedManager = new SerializedObject(manager);

            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            Transform safeArea = canvas.transform.Find("SafeArea");
            if (safeArea == null) safeArea = canvas.transform;

            // ========== SCROLL VIEW ==========
            ConnectScrollView(serializedManager, safeArea);

            // ========== HEADER ==========
            ConnectHeader(serializedManager, safeArea);

            // ========== POPUPS ==========
            ConnectPopups(serializedManager, canvas.transform);

            serializedManager.ApplyModifiedProperties();

            Debug.Log("[ShopConnector] ShopManager references connected (V4 continuous scroll)");
        }

        private static void ConnectScrollView(SerializedObject manager, Transform root)
        {
            Transform scrollView = FindDeep(root, "ShopScrollView");
            if (scrollView == null) { Debug.LogWarning("[ShopConnector] ShopScrollView not found"); return; }

            var svProp = manager.FindProperty("_shopScrollView");
            if (svProp != null) svProp.objectReferenceValue = scrollView.gameObject;

            var svtProp = manager.FindProperty("_scrollViewTransform");
            if (svtProp != null) svtProp.objectReferenceValue = scrollView.GetComponent<RectTransform>();

            Debug.Log("[ShopConnector] ScrollView connected (continuous scroll)");
        }

        private static void ConnectHeader(SerializedObject manager, Transform root)
        {
            Transform header = FindDeep(root, "Header");
            if (header == null) return;

            // Back Button
            Transform backBtn = header.Find("BackButton");
            if (backBtn != null)
            {
                Button btn = backBtn.GetComponent<Button>();
                if (btn != null)
                {
                    manager.FindProperty("_backButton").objectReferenceValue = btn;
                }
            }

            // Currency Display
            Transform currencyDisplay = header.Find("CurrencyDisplay");
            if (currencyDisplay != null)
            {
                Transform gemsDisplay = currencyDisplay.Find("GemsDisplay");
                if (gemsDisplay != null)
                {
                    TextMeshProUGUI gemsText = gemsDisplay.Find("Amount")?.GetComponent<TextMeshProUGUI>();
                    if (gemsText != null)
                    {
                        manager.FindProperty("_headerGemsText").objectReferenceValue = gemsText;
                    }
                }

                Transform coinsDisplay = currencyDisplay.Find("CoinsDisplay");
                if (coinsDisplay != null)
                {
                    TextMeshProUGUI coinsText = coinsDisplay.Find("Amount")?.GetComponent<TextMeshProUGUI>();
                    if (coinsText != null)
                    {
                        manager.FindProperty("_headerCoinsText").objectReferenceValue = coinsText;
                    }
                }
            }

            Debug.Log("[ShopConnector] Header connected");
        }

        private static void ConnectPopups(SerializedObject manager, Transform root)
        {
            // Purchase Popup
            Transform purchaseBlocker = root.Find("PurchaseBlocker");
            if (purchaseBlocker != null)
            {
                manager.FindProperty("_purchasePopup").objectReferenceValue = purchaseBlocker.gameObject;

                Transform popup = purchaseBlocker.Find("PurchasePopup");
                if (popup != null)
                {
                    Transform preview = popup.Find("Preview");
                    if (preview != null)
                    {
                        Image icon = preview.Find("Icon")?.GetComponent<Image>();
                        if (icon != null)
                        {
                            manager.FindProperty("_popupItemIcon").objectReferenceValue = icon;
                        }

                        TextMeshProUGUI amount = preview.Find("Amount")?.GetComponent<TextMeshProUGUI>();
                        if (amount != null)
                        {
                            manager.FindProperty("_popupItemName").objectReferenceValue = amount;
                        }
                    }

                    TextMeshProUGUI price = popup.Find("Price")?.GetComponent<TextMeshProUGUI>();
                    if (price != null)
                    {
                        manager.FindProperty("_popupItemPrice").objectReferenceValue = price;
                    }

                    Transform buttons = popup.Find("Buttons");
                    if (buttons != null)
                    {
                        Button cancelBtn = buttons.Find("CancelButton")?.GetComponent<Button>();
                        if (cancelBtn != null)
                        {
                            manager.FindProperty("_popupCancelButton").objectReferenceValue = cancelBtn;
                        }

                        Button confirmBtn = buttons.Find("ConfirmButton")?.GetComponent<Button>();
                        if (confirmBtn != null)
                        {
                            manager.FindProperty("_popupConfirmButton").objectReferenceValue = confirmBtn;
                        }
                    }
                }
            }

            // Not Enough Gems Popup
            Transform notEnoughBlocker = root.Find("NotEnoughBlocker");
            if (notEnoughBlocker != null)
            {
                manager.FindProperty("_notEnoughGemsPopup").objectReferenceValue = notEnoughBlocker.gameObject;

                Transform popup = notEnoughBlocker.Find("NotEnoughPopup");
                if (popup != null)
                {
                    Transform buttons = popup.Find("Buttons");
                    if (buttons != null)
                    {
                        Button closeBtn = buttons.Find("CloseButton")?.GetComponent<Button>();
                        if (closeBtn != null)
                        {
                            manager.FindProperty("_notEnoughCloseButton").objectReferenceValue = closeBtn;
                        }

                        Button getGemsBtn = buttons.Find("GetGemsButton")?.GetComponent<Button>();
                        if (getGemsBtn != null)
                        {
                            manager.FindProperty("_notEnoughGetGemsButton").objectReferenceValue = getGemsBtn;
                        }
                    }
                }
            }

            Debug.Log("[ShopConnector] Popups connected");
        }

        private static Transform FindDeep(Transform parent, string name)
        {
            if (parent.name == name) return parent;

            foreach (Transform child in parent)
            {
                Transform result = FindDeep(child, name);
                if (result != null) return result;
            }

            return null;
        }

        [MenuItem("DigitPark/Shop/Add ShopItemUI to Items", false, 703)]
        public static void AddShopItemUIToItems()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("[ShopConnector] No Canvas found");
                return;
            }

            Transform safeArea = canvas.transform.Find("SafeArea") ?? canvas.transform;

            // V4: Find grids inside continuous scroll
            int added = 0;
            string[] gridNames = { "GemsGrid", "CoinsGrid", "ThemesGrid", "FramesGrid", "TitlesGrid" };

            // Search in ShopScrollView
            Transform scrollView = FindDeep(safeArea, "ShopScrollView");
            Transform searchRoot = scrollView != null ? scrollView : safeArea;

            foreach (string gridName in gridNames)
            {
                Transform grid = FindDeep(searchRoot, gridName);
                if (grid == null) continue;

                foreach (Transform item in grid)
                {
                    if (item.GetComponent<ShopItemUI>() == null)
                    {
                        ShopItemUI shopItem = item.gameObject.AddComponent<ShopItemUI>();
                        added++;
                    }
                }
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log($"[ShopConnector] Added ShopItemUI to {added} items");
            EditorUtility.DisplayDialog("ShopItemUI Added",
                $"Se agregó ShopItemUI a {added} items.\n\nAhora asigna los ShopItemData a cada item en el Inspector.",
                "OK");
        }
    }
}
