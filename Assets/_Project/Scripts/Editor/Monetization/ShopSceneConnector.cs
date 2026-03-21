using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;
using DigitPark.Monetization;
using DigitPark.Navigation;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para conectar automaticamente las referencias
    /// de la escena Shop V4 (scroll continuo) con el ShopManager y otros componentes.
    /// </summary>
    public class ShopSceneConnector : EditorWindow
    {
        [MenuItem("DigitPark/Setup/Shop/Connect References", false, 700)]
        public static void ConnectReferences()
        {
            if (!EditorUtility.DisplayDialog("Shop Scene Connector",
                "Esto conectara automaticamente las referencias de la escena Shop V4.\nAsegurate de tener la escena Shop abierta.\n\nContinuar?",
                "Si", "No"))
                return;

            ConnectShopReferences();
        }

        [MenuItem("DigitPark/Setup/Shop/Setup Manager", false, 701)]
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

        [MenuItem("DigitPark/Setup/Shop/Add ShopItemUI", false, 703)]
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
