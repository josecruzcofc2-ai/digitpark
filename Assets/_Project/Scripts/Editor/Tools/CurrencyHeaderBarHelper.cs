using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.Monetization;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// Shared utility for creating currency pills (DigitGems + DigitCoins)
    /// in scene headers. Each pill shows icon + amount + "+" button
    /// and has CurrencyDisplayUI for auto-subscription at runtime.
    /// </summary>
    public static class CurrencyHeaderBarHelper
    {
        // Icon paths
        private const string ICONS_BASE = "Assets/_Project/Art/Icons";
        private const string ICON_GEM = ICONS_BASE + "/Currency/icon_digitgem_single.png";
        private const string ICON_COIN = ICONS_BASE + "/Currency/icon_digitcoin_single.png";

        // Colors matching CurrencyDisplayUI defaults
        private static readonly Color GEM_COLOR = new Color(0.4f, 0.8f, 1f, 1f);
        private static readonly Color COIN_COLOR = new Color(1f, 0.85f, 0.3f, 1f);

        // Pill styling — GREEN square pills (Clash Royale style)
        private static readonly Color PILL_BG = new Color(0.05f, 0.22f, 0.12f, 0.95f);
        private static readonly Color PILL_OUTLINE = new Color(0.2f, 0.85f, 0.4f, 0.7f);
        private static readonly Color TEXT_WHITE = new Color(0.95f, 0.95f, 0.95f, 1f);

        /// <summary>
        /// Creates an HorizontalLayoutGroup container with two currency pills (Gems + Coins).
        /// Returns the container GameObject for positioning by the caller.
        /// Pills are named "GemsDisplay" and "CoinsDisplay" by default.
        /// </summary>
        public static GameObject CreateCurrencyPills(Transform parent, string containerName = "CurrencyPills")
        {
            var container = FindOrCreate(parent, containerName);
            var cRT = GetOrAdd<RectTransform>(container);

            var hlg = GetOrAdd<HorizontalLayoutGroup>(container);
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleRight;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.reverseArrangement = false;
            hlg.padding = new RectOffset(0, 15, 4, 4);

            // Remove old children if rebuilding
            for (int i = container.transform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(container.transform.GetChild(i).gameObject);

            // Gems pill
            CreateSingleCurrencyPill(container.transform, "GemsDisplay", CurrencyType.DigitGems);

            // Coins pill
            CreateSingleCurrencyPill(container.transform, "CoinsDisplay", CurrencyType.DigitCoins);

            return container;
        }

        /// <summary>
        /// Creates a single pill: [Icon] [Amount TMP] [+ circle]
        /// Attaches CurrencyDisplayUI component via SerializedObject for auto-subscription at runtime.
        /// </summary>
        public static GameObject CreateSingleCurrencyPill(Transform parent, string name, CurrencyType currencyType)
        {
            Color color = currencyType == CurrencyType.DigitGems ? GEM_COLOR : COIN_COLOR;

            var pill = FindOrCreate(parent, name);
            var rt = GetOrAdd<RectTransform>(pill);
            rt.sizeDelta = new Vector2(245, 80);

            // Pill background
            var bg = GetOrAdd<Image>(pill);
            bg.color = PILL_BG;
            var outline = GetOrAdd<Outline>(pill);
            outline.effectColor = PILL_OUTLINE;
            outline.effectDistance = new Vector2(1.5f, 1.5f);

            // Button for tap → navigate to shop
            var btn = GetOrAdd<Button>(pill);
            btn.targetGraphic = bg;

            // Layout
            var pillHLG = GetOrAdd<HorizontalLayoutGroup>(pill);
            pillHLG.spacing = 6;
            pillHLG.padding = new RectOffset(8, 8, 4, 4);
            pillHLG.childAlignment = TextAnchor.MiddleCenter;
            pillHLG.childControlWidth = false;
            pillHLG.childControlHeight = true;

            var le = GetOrAdd<LayoutElement>(pill);
            le.minWidth = 245;
            le.preferredWidth = 245;

            // Remove old children if rebuilding
            for (int i = pill.transform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(pill.transform.GetChild(i).gameObject);

            // Icon (explicit sizeDelta — HLG childControlWidth=false ignores LayoutElement)
            var icon = new GameObject("Icon");
            icon.transform.SetParent(pill.transform, false);
            var iconRT = icon.AddComponent<RectTransform>();
            iconRT.sizeDelta = new Vector2(62, 62);
            var iconImg = icon.AddComponent<Image>();
            iconImg.color = color;
            iconImg.preserveAspect = true;
            var iconLE = icon.AddComponent<LayoutElement>();
            iconLE.minWidth = 62;
            iconLE.minHeight = 62;
            iconLE.preferredWidth = 62;
            iconLE.preferredHeight = 62;

            // Load icon sprite
            string iconPath = currencyType == CurrencyType.DigitGems ? ICON_GEM : ICON_COIN;
            Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            if (iconSprite != null)
            {
                iconImg.sprite = iconSprite;
                iconImg.color = Color.white;
            }

            // Amount text
            var amountObj = new GameObject("Amount");
            amountObj.transform.SetParent(pill.transform, false);
            var amountRT = amountObj.AddComponent<RectTransform>();
            amountRT.sizeDelta = new Vector2(75, 30);
            var amountText = amountObj.AddComponent<TextMeshProUGUI>();
            amountText.text = "0";
            amountText.fontSize = FontSizes.Body;
            amountText.fontSizeMin = FontSizes.AutoMinBody;
            amountText.enableAutoSizing = true;
            amountText.fontStyle = FontStyles.Bold;
            amountText.color = TEXT_WHITE;
            amountText.alignment = TextAlignmentOptions.MidlineLeft;
            var amountLE = amountObj.AddComponent<LayoutElement>();
            amountLE.minWidth = 75;
            amountLE.preferredWidth = 75;

            // Plus indicator — circle with "+" text
            var plus = new GameObject("Plus");
            plus.transform.SetParent(pill.transform, false);
            var plusRT = plus.AddComponent<RectTransform>();
            plusRT.sizeDelta = new Vector2(58, 58);
            var plusImg = plus.AddComponent<Image>();
            plusImg.color = new Color(0.2f, 0.85f, 0.4f, 1f); // GREEN CTA (Clash Royale style)
            var plusLE = plus.AddComponent<LayoutElement>();
            plusLE.minWidth = 58;
            plusLE.minHeight = 58;
            plusLE.preferredWidth = 58;
            plusLE.preferredHeight = 58;

            // "+" text inside the circle
            var plusTextObj = new GameObject("PlusText");
            plusTextObj.transform.SetParent(plus.transform, false);
            var plusTextRT = plusTextObj.AddComponent<RectTransform>();
            plusTextRT.anchorMin = Vector2.zero;
            plusTextRT.anchorMax = Vector2.one;
            plusTextRT.offsetMin = Vector2.zero;
            plusTextRT.offsetMax = Vector2.zero;
            var plusTMP = plusTextObj.AddComponent<TextMeshProUGUI>();
            plusTMP.text = "+";
            plusTMP.fontSize = 30;
            plusTMP.fontStyle = FontStyles.Bold;
            plusTMP.color = new Color(0.02f, 0.04f, 0.08f, 1f); // dark text on colored bg
            plusTMP.alignment = TextAlignmentOptions.Center;

            // Attach CurrencyDisplayUI component for runtime auto-subscription
            var currencyUI = GetOrAdd<CurrencyDisplayUI>(pill);
            var so = new SerializedObject(currencyUI);
            so.FindProperty("_currencyType").enumValueIndex = (int)currencyType;
            so.FindProperty("_iconImage").objectReferenceValue = iconImg;
            so.FindProperty("_amountText").objectReferenceValue = amountText;
            so.FindProperty("_button").objectReferenceValue = btn;
            so.FindProperty("_plusButton").objectReferenceValue = plus;
            if (iconSprite != null)
            {
                if (currencyType == CurrencyType.DigitGems)
                    so.FindProperty("_gemsIcon").objectReferenceValue = iconSprite;
                else
                    so.FindProperty("_coinsIcon").objectReferenceValue = iconSprite;
            }
            so.ApplyModifiedProperties();

            return pill;
        }

        // ==================== Helpers ====================

        private static GameObject FindOrCreate(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null) return existing.gameObject;

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private static T GetOrAdd<T>(GameObject obj) where T : Component
        {
            T comp = obj.GetComponent<T>();
            if (comp == null) comp = obj.AddComponent<T>();
            return comp;
        }
    }
}
