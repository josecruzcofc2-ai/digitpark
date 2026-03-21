using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using DigitPark.Monetization;

namespace DigitPark.Editor
{
    /// <summary>
    /// Bloque 2 — Editor Scripts de setup automatizado.
    /// Menu: DigitPark > Setup > ...
    /// Cada herramienta hace una sola cosa y no requiere intervencion manual adicional.
    /// </summary>
    public static class DigitParkSetupTools
    {
        private const string BASE_URL = "https://us-central1-digitpark-7d772.cloudfunctions.net";

        // ============================================================
        // C-14: Rellenar URLs de Cloud Functions en PaymentManager
        // DigitPark > Setup > Fill PaymentManager URLs
        // Abrir Boot.unity antes de ejecutar
        // ============================================================
        [MenuItem("DigitPark/Setup/Fill PaymentManager URLs", false, 10)]
        public static void FillPaymentManagerUrls()
        {
            var pm = Object.FindFirstObjectByType<DigitPark.Payments.PaymentManager>();
            if (pm == null)
            {
                EditorUtility.DisplayDialog("No encontrado",
                    "PaymentManager no encontrado en la escena activa.\n\nAbre Boot.unity e intenta de nuevo.",
                    "OK");
                return;
            }

            var so = new SerializedObject(pm);
            var configProp = so.FindProperty("_config");
            if (configProp == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontro el campo _config en PaymentManager.", "OK");
                return;
            }

            SetStringField(configProp, "paymentsHealthUrl",        $"{BASE_URL}/paymentsHealth");
            SetStringField(configProp, "stripeCreateCheckoutUrl",  $"{BASE_URL}/stripeCreateCheckout");
            SetStringField(configProp, "stripeSessionStatusUrl",   $"{BASE_URL}/stripeSessionStatus");
            SetStringField(configProp, "stripeWebhookUrl",         $"{BASE_URL}/stripeWebhook");
            SetStringField(configProp, "iapValidateReceiptUrl",    $"{BASE_URL}/iapValidateReceipt");
            SetStringField(configProp, "getEntitlementsUrl",       $"{BASE_URL}/getEntitlements");
            SetStringField(configProp, "syncEntitlementsUrl",      $"{BASE_URL}/syncEntitlements");
            SetStringField(configProp, "adminForceSwitchUrl",      $"{BASE_URL}/adminForceSwitch");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(pm);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("URLs configuradas",
                "Todas las Cloud Function URLs fueron rellenadas en PaymentManager.\n\n" +
                "PENDIENTE MANUAL:\n" +
                "- stripePublishableKey (pk_live_xxx)\n" +
                "- IapProductIds (los 7 IDs de Gem Packs)\n\n" +
                "Guarda la escena (Ctrl+S).",
                "OK");

            Debug.Log($"[SetupTools] PaymentManager URLs rellenadas en: {pm.gameObject.name}");
        }

        private static void SetStringField(SerializedProperty parent, string fieldName, string value)
        {
            var prop = parent.FindPropertyRelative(fieldName);
            if (prop != null && string.IsNullOrEmpty(prop.stringValue))
                prop.stringValue = value;
        }

        // ============================================================
        // C-15: Agregar tag "FrameLayer" al proyecto
        // DigitPark > Setup > Add FrameLayer Tag
        // ============================================================
        [MenuItem("DigitPark/Setup/Add FrameLayer Tag", false, 11)]
        public static void AddFrameLayerTag()
        {
            const string TAG = "FrameLayer";

            // Verificar si ya existe
            foreach (var existingTag in UnityEditorInternal.InternalEditorUtility.tags)
            {
                if (existingTag == TAG)
                {
                    EditorUtility.DisplayDialog("Tag ya existe",
                        $"El tag '{TAG}' ya esta en el proyecto.", "OK");
                    return;
                }
            }

            // Agregar via SerializedObject del TagManager
            var tagManagerAsset = AssetDatabase.LoadAssetAtPath<Object>(
                "ProjectSettings/TagManager.asset");

            if (tagManagerAsset == null)
            {
                EditorUtility.DisplayDialog("Error",
                    "No se pudo cargar TagManager.asset.\nAgregar manualmente: Edit > Project Settings > Tags and Layers",
                    "OK");
                return;
            }

            var so = new SerializedObject(tagManagerAsset);
            var tagsProp = so.FindProperty("tags");

            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = TAG;

            so.ApplyModifiedProperties();

            EditorUtility.DisplayDialog("Tag agregado",
                $"Tag '{TAG}' agregado correctamente.\n\nYa disponible para FrameRenderer.cs y CashThemeForcer.cs.",
                "OK");

            Debug.Log($"[SetupTools] Tag '{TAG}' agregado al proyecto.");
        }

        // ============================================================
        // C-16: Economy Rebalance — batch update de precios DC en ShopItemData
        // DigitPark > Setup > Apply Economy Rebalance (DC Prices)
        // ============================================================
        [MenuItem("DigitPark/Setup/Apply Economy Rebalance (DC Prices)", false, 30)]
        public static void ApplyEconomyRebalance()
        {
            // Mapas: itemId (lowercase match) → nuevo coinsPrice
            var frameRebalance = new Dictionary<string, int>
            {
                { "basic",    2000  },
                { "bronze",   5000  },
                { "silver",   12000 },
                { "gold",     25000 },
                { "neon",     40000 },
                { "diamond",  60000 },
                { "crystal",  80000 },
                { "platinum", 100000 },
            };

            var titleRebalance = new Dictionary<string, int>
            {
                { "strategist", 8000  },
                { "analyst",    8000  },
                { "champion",   20000 },
                { "gladiator",  20000 },
            };

            var effectRebalance = new Dictionary<string, int>
            {
                { "confetti", 12000 },
                { "fireworks", 20000 },
            };

            var allItems = Resources.LoadAll<ShopItemData>("Shop");
            int changed = 0;
            var log = new System.Text.StringBuilder();

            foreach (var item in allItems)
            {
                if (item.priceType != PriceType.DigitCoins) continue;

                string nameLower = item.itemId?.ToLower() ?? item.name.ToLower();
                int newPrice = -1;

                if (item.itemType == ShopItemType.Frame)
                {
                    foreach (var kv in frameRebalance)
                        if (nameLower.Contains(kv.Key)) { newPrice = kv.Value; break; }
                }
                else if (item.itemType == ShopItemType.Title)
                {
                    foreach (var kv in titleRebalance)
                        if (nameLower.Contains(kv.Key)) { newPrice = kv.Value; break; }
                }
                else if (item.itemType == ShopItemType.WinEffect)
                {
                    foreach (var kv in effectRebalance)
                        if (nameLower.Contains(kv.Key)) { newPrice = kv.Value; break; }
                }

                if (newPrice > 0 && item.coinsPrice != newPrice)
                {
                    log.AppendLine($"  {item.name}: {item.coinsPrice} DC -> {newPrice} DC");
                    var itemSO = new SerializedObject(item);
                    itemSO.FindProperty("coinsPrice").intValue = newPrice;
                    itemSO.ApplyModifiedProperties();
                    EditorUtility.SetDirty(item);
                    changed++;
                }
            }

            if (changed == 0)
            {
                EditorUtility.DisplayDialog("Economy Rebalance",
                    "No se encontraron items para actualizar.\n\n" +
                    "Verifica que los ShopItemData esten en Resources/Shop/ y tengan priceType = DigitCoins.",
                    "OK");
                return;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Economy Rebalance completo",
                $"Actualizados {changed} items:\n\n{log}",
                "OK");

            Debug.Log($"[SetupTools] Economy Rebalance: {changed} items actualizados.\n{log}");
        }

        // ============================================================
        // C-17: Verificar/fijar priceType en temas Tier B
        // DigitPark > Setup > Fix Tier B Theme Prices
        // Tier B = themes con priceType DigitCoins que deberian ser DigitGems (350 DG)
        // ============================================================
        [MenuItem("DigitPark/Setup/Fix Tier B Theme Prices", false, 31)]
        public static void FixTierBThemePrices()
        {
            const int TIER_B_GEMS_PRICE = 350;

            var allItems = Resources.LoadAll<ShopItemData>("Shop");
            int fixed_ = 0;
            int alreadyOk = 0;
            var log = new System.Text.StringBuilder();
            var okLog = new System.Text.StringBuilder();

            foreach (var item in allItems)
            {
                if (item.itemType != ShopItemType.Theme) continue;
                // Solo temas de pago (no gratis ni dinero real)
                if (item.priceType == PriceType.RealMoney || item.priceType == PriceType.Free) continue;

                if (item.priceType == PriceType.DigitGems && item.gemsPrice == TIER_B_GEMS_PRICE)
                {
                    alreadyOk++;
                    okLog.AppendLine($"  OK  {item.name}");
                    continue;
                }

                // Necesita correccion
                log.AppendLine($"  FIX {item.name}: {item.priceType} {item.coinsPrice}DC/{item.gemsPrice}DG -> DigitGems {TIER_B_GEMS_PRICE}DG");
                var so = new SerializedObject(item);
                so.FindProperty("priceType").enumValueIndex = (int)PriceType.DigitGems;
                so.FindProperty("gemsPrice").intValue = TIER_B_GEMS_PRICE;
                so.FindProperty("coinsPrice").intValue = 0;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(item);
                fixed_++;
            }

            if (fixed_ == 0 && alreadyOk == 0)
            {
                EditorUtility.DisplayDialog("Fix Tier B Prices",
                    "No se encontraron ShopItemData de tipo Theme en Resources/Shop/.",
                    "OK");
                return;
            }

            if (fixed_ > 0)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            string summary = $"Corregidos: {fixed_} | Ya correctos: {alreadyOk}";
            string detail = fixed_ > 0 ? $"\nCorregidos:\n{log}" : "";
            detail += alreadyOk > 0 ? $"\nYa correctos:\n{okLog}" : "";

            EditorUtility.DisplayDialog("Fix Tier B Theme Prices", summary + detail, "OK");
            Debug.Log($"[SetupTools] Tier B Themes — {summary}\n{log}");
        }

        // ============================================================
        // Fix BackButtonGold missing sprite
        // DigitPark > Setup > Fix BackButtonGold Sprite
        // ============================================================
        [MenuItem("DigitPark/Setup/Fix BackButtonGold Sprite", false, 50)]
        public static void FixBackButtonGoldSprite()
        {
            const string prefabPath = "Assets/_Project/Prefabs/Common/BackButtonGold.prefab";
            const string spritePath = "Assets/_Project/Art/Icons/Navigation/BackIconGold.png";

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite == null)
            {
                EditorUtility.DisplayDialog("Error",
                    $"No se encontro el sprite:\n{spritePath}\n\nVerifica que BackIconGold.png exista.", "OK");
                return;
            }

            using (var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
            {
                var root = scope.prefabContentsRoot;
                var images = root.GetComponentsInChildren<Image>(true);
                bool found = false;

                foreach (var img in images)
                {
                    if (img.gameObject.name == "Icon")
                    {
                        img.sprite = sprite;
                        EditorUtility.SetDirty(img.gameObject);
                        found = true;
                        Debug.Log($"[SetupTools] BackButtonGold.Icon sprite asignado: {spritePath}");
                        break;
                    }
                }

                if (!found)
                {
                    EditorUtility.DisplayDialog("Error",
                        "No se encontro un GameObject 'Icon' con Image en BackButtonGold.prefab.", "OK");
                    return;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Listo",
                "BackButtonGold.prefab — sprite asignado correctamente.\n\n" +
                "Todas las escenas CashBattle que usen este prefab verán el icono gold.",
                "OK");
        }
    }
}
