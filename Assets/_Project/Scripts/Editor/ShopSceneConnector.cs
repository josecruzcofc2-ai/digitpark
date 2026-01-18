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
    /// de la escena Shop con el ShopManager y otros componentes.
    /// </summary>
    public class ShopSceneConnector : EditorWindow
    {
        [MenuItem("DigitPark/Shop/Connect Shop References", false, 700)]
        public static void ConnectReferences()
        {
            if (!EditorUtility.DisplayDialog("Shop Scene Connector",
                "Esto conectara automaticamente las referencias de la escena Shop.\nAsegurate de tener la escena Shop abierta.\n\nContinuar?",
                "Si", "No"))
                return;

            ConnectShopReferences();
        }

        [MenuItem("DigitPark/Shop/Setup Shop Manager", false, 701)]
        public static void SetupShopManager()
        {
            // Find or create ShopManager
            ShopManager manager = Object.FindObjectOfType<ShopManager>();

            if (manager == null)
            {
                // Find Canvas
                Canvas canvas = Object.FindObjectOfType<Canvas>();
                if (canvas == null)
                {
                    Debug.LogError("[ShopConnector] No se encontro Canvas en la escena");
                    return;
                }

                // Create ShopManager object
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

            // Create folder if not exists
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                System.IO.Directory.CreateDirectory(Application.dataPath + "/_Project/Data/Monetization/ShopItems");
                AssetDatabase.Refresh();
            }

            // Create gem pack items
            CreateGemPackAssets(folderPath);

            // Create coin pack items
            CreateCoinPackAssets(folderPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[ShopConnector] Shop Item Data assets created in: " + folderPath);
            EditorUtility.DisplayDialog("Shop Items Created",
                $"Se crearon los assets de items en:\n{folderPath}",
                "OK");
        }

        private static void CreateGemPackAssets(string folderPath)
        {
            var gemPacks = new (string id, string name, int gems, float price, int bonus, bool popular)[]
            {
                ("gems_100", "100 Gemas", 100, 0.99f, 0, false),
                ("gems_500", "500 Gemas", 500, 4.99f, 10, false),
                ("gems_1200", "1,200 Gemas", 1200, 9.99f, 20, true),
                ("gems_2500", "2,500 Gemas", 2500, 19.99f, 25, false),
                ("gems_6500", "6,500 Gemas", 6500, 49.99f, 30, false),
                ("gems_14000", "14,000 Gemas", 14000, 99.99f, 35, false),
            };

            foreach (var pack in gemPacks)
            {
                string assetPath = $"{folderPath}/{pack.id}.asset";

                // Skip if already exists
                if (AssetDatabase.LoadAssetAtPath<ShopItemData>(assetPath) != null)
                    continue;

                ShopItemData item = ScriptableObject.CreateInstance<ShopItemData>();
                item.itemId = pack.id;
                item.displayName = pack.name;
                item.description = $"Obtén {pack.gems} gemas";
                item.itemType = ShopItemType.GemsPack;
                item.shopTab = ShopTab.Gems;
                item.priceType = PriceType.RealMoney;
                item.realMoneyPrice = pack.price;
                item.iapProductId = $"com.digitpark.{pack.id}";
                item.gemsAmount = pack.gems;
                item.bonusPercent = pack.bonus;
                item.isPopular = pack.popular;
                item.accentColor = new Color(0.4f, 0.8f, 1f, 1f); // Gem blue
                item.sortOrder = System.Array.IndexOf(gemPacks, pack);

                AssetDatabase.CreateAsset(item, assetPath);
                Debug.Log($"[ShopConnector] Created: {assetPath}");
            }
        }

        private static void CreateCoinPackAssets(string folderPath)
        {
            var coinPacks = new (string id, string name, int coins, int gemsPrice, int bonus, bool popular)[]
            {
                ("coins_1000", "1,000 Monedas", 1000, 50, 0, false),
                ("coins_5000", "5,000 Monedas", 5000, 200, 25, false),
                ("coins_15000", "15,000 Monedas", 15000, 500, 50, true),
            };

            foreach (var pack in coinPacks)
            {
                string assetPath = $"{folderPath}/{pack.id}.asset";

                // Skip if already exists
                if (AssetDatabase.LoadAssetAtPath<ShopItemData>(assetPath) != null)
                    continue;

                ShopItemData item = ScriptableObject.CreateInstance<ShopItemData>();
                item.itemId = pack.id;
                item.displayName = pack.name;
                item.description = $"Obtén {pack.coins} monedas";
                item.itemType = ShopItemType.CoinsPack;
                item.shopTab = ShopTab.Coins;
                item.priceType = PriceType.Gems;
                item.gemsPrice = pack.gemsPrice;
                item.coinsAmount = pack.coins;
                item.bonusPercent = pack.bonus;
                item.isPopular = pack.popular;
                item.accentColor = new Color(1f, 0.85f, 0.3f, 1f); // Gold
                item.sortOrder = System.Array.IndexOf(coinPacks, pack);

                AssetDatabase.CreateAsset(item, assetPath);
                Debug.Log($"[ShopConnector] Created: {assetPath}");
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

            // Mark scene dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("[ShopConnector] Referencias conectadas exitosamente");
        }

        private static void ConnectShopManagerReferences(ShopManager manager)
        {
            SerializedObject serializedManager = new SerializedObject(manager);

            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null) return;

            // Find SafeArea
            Transform safeArea = canvas.transform.Find("SafeArea");
            if (safeArea == null) safeArea = canvas.transform;

            // ========== TAB BUTTONS ==========
            ConnectTabButtons(serializedManager, safeArea);

            // ========== CONTENT AREAS ==========
            ConnectContentAreas(serializedManager, safeArea);

            // ========== HEADER ==========
            ConnectHeader(serializedManager, safeArea);

            // ========== POPUPS ==========
            ConnectPopups(serializedManager, canvas.transform);

            serializedManager.ApplyModifiedProperties();

            Debug.Log("[ShopConnector] ShopManager references connected");
        }

        private static void ConnectTabButtons(SerializedObject manager, Transform root)
        {
            Transform tabsPanel = FindDeep(root, "TabsPanel");
            if (tabsPanel == null) return;

            // Gems Tab
            Transform gemsTab = tabsPanel.Find("GemsTab");
            if (gemsTab != null)
            {
                Button btn = gemsTab.GetComponent<Button>();
                if (btn != null)
                {
                    manager.FindProperty("_gemsTabButton").objectReferenceValue = btn;
                }
            }

            // Coins Tab
            Transform coinsTab = tabsPanel.Find("CoinsTab");
            if (coinsTab != null)
            {
                Button btn = coinsTab.GetComponent<Button>();
                if (btn != null)
                {
                    manager.FindProperty("_coinsTabButton").objectReferenceValue = btn;
                }
            }

            // Cosmetics/Themes Tab
            Transform cosmeticsTab = tabsPanel.Find("CosmeticsTab");
            if (cosmeticsTab != null)
            {
                Button btn = cosmeticsTab.GetComponent<Button>();
                if (btn != null)
                {
                    manager.FindProperty("_themesTabButton").objectReferenceValue = btn;
                }
            }

            // Offers Tab
            Transform offersTab = tabsPanel.Find("OffersTab");
            if (offersTab != null)
            {
                Button btn = offersTab.GetComponent<Button>();
                if (btn != null)
                {
                    manager.FindProperty("_offersTabButton").objectReferenceValue = btn;
                }
            }

            Debug.Log("[ShopConnector] Tab buttons connected");
        }

        private static void ConnectContentAreas(SerializedObject manager, Transform root)
        {
            // Find the scroll view content
            Transform scrollView = FindDeep(root, "ShopScrollView");
            if (scrollView == null) return;

            Transform content = FindDeep(scrollView, "Content");
            if (content == null) return;

            // Gems Section
            Transform gemsSection = content.Find("GemsSection");
            Transform gemsGrid = content.Find("GemsGrid");
            if (gemsGrid != null)
            {
                manager.FindProperty("_gemsContent").objectReferenceValue = gemsGrid.gameObject;
            }

            // Coins Section
            Transform coinsSection = content.Find("CoinsSection");
            Transform coinsGrid = content.Find("CoinsGrid");
            if (coinsGrid != null)
            {
                manager.FindProperty("_coinsContent").objectReferenceValue = coinsGrid.gameObject;
            }

            Debug.Log("[ShopConnector] Content areas connected");
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
                    // Preview icon
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

                    // Price text
                    TextMeshProUGUI price = popup.Find("Price")?.GetComponent<TextMeshProUGUI>();
                    if (price != null)
                    {
                        manager.FindProperty("_popupItemPrice").objectReferenceValue = price;
                    }

                    // Buttons
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
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[ShopConnector] No Canvas found");
                return;
            }

            Transform safeArea = canvas.transform.Find("SafeArea") ?? canvas.transform;
            Transform scrollView = FindDeep(safeArea, "ShopScrollView");
            if (scrollView == null)
            {
                Debug.LogError("[ShopConnector] No ShopScrollView found");
                return;
            }

            Transform content = FindDeep(scrollView, "Content");
            if (content == null)
            {
                Debug.LogError("[ShopConnector] No Content found");
                return;
            }

            // Find grids and add ShopItemUI
            string[] gridNames = { "GemsGrid", "CoinsGrid" };

            int added = 0;
            foreach (string gridName in gridNames)
            {
                Transform grid = content.Find(gridName);
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
