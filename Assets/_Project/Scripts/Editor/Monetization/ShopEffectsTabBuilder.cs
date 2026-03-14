using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Genera el contenido del tab "Effects" en el Shop.
    /// Crea un grid de 8 cards (una por efecto de victoria).
    /// Menu: DigitPark/Shop/Rebuild Effects Tab
    /// </summary>
    public static class ShopEffectsTabBuilder
    {
        private static readonly Color BG_CARD = new Color(0.08f, 0.08f, 0.15f, 0.95f);
        private static readonly Color CYAN = new Color(0f, 1f, 1f, 1f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color TEXT_WHITE = Color.white;
        private static readonly Color TEXT_SECONDARY = new Color(0.7f, 0.7f, 0.7f, 1f);

        private static readonly string[] EFFECT_IDS = {
            "confetti", "fireworks", "lightning", "gold_rain",
            "neon_explosion", "rainbow", "crown_drop", "fire_ring"
        };
        private static readonly string[] EFFECT_KEYS = {
            "effect_confetti", "effect_fireworks", "effect_lightning", "effect_gold_rain",
            "effect_neon_explosion", "effect_rainbow", "effect_crown_drop", "effect_fire_ring"
        };
        private static readonly string[] EFFECT_PRICES = {
            "Free", "2,000 DC", "5,000 DC", "250 DG",
            "400 DG", "750 DG", "$1.99", "$2.99"
        };
        private static readonly Color[] EFFECT_PRIMARY = {
            new Color(1f, 0.8f, 0f), new Color(1f, 0.3f, 0.1f), new Color(0.3f, 0.6f, 1f), new Color(1f, 0.84f, 0f),
            new Color(0f, 1f, 1f), new Color(1f, 0f, 0f), new Color(1f, 0.84f, 0f), new Color(1f, 0.4f, 0f)
        };
        private static readonly Color[] EFFECT_SECONDARY = {
            new Color(0f, 0.8f, 1f), new Color(1f, 0.8f, 0f), new Color(0.8f, 0.9f, 1f), new Color(0.85f, 0.65f, 0f),
            new Color(1f, 0f, 1f), new Color(0.5f, 0f, 1f), new Color(0.6f, 0.2f, 0.8f), new Color(1f, 0.8f, 0f)
        };

        [MenuItem("DigitPark/Shop/Rebuild Effects Tab")]
        public static void BuildEffectsTab()
        {
            // Find Shop canvas
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[ShopEffectsTabBuilder] No Canvas found in scene. Open the Shop scene first.");
                return;
            }

            // Find or create EffectsSection under content
            Transform contentRoot = canvas.transform;
            Transform scrollContent = FindDeep(contentRoot, "Content") ?? FindDeep(contentRoot, "ScrollContent") ?? contentRoot;

            // Create effects section
            GameObject section = FindOrCreate(scrollContent, "EffectsSection");
            RectTransform sectionRT = GetOrAdd<RectTransform>(section);
            sectionRT.anchorMin = new Vector2(0, 0);
            sectionRT.anchorMax = new Vector2(1, 0);
            sectionRT.pivot = new Vector2(0.5f, 1);

            // Clear old children
            for (int i = section.transform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(section.transform.GetChild(i).gameObject);

            // Section title
            GameObject titleObj = new GameObject("EffectsTitle");
            titleObj.transform.SetParent(section.transform, false);
            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0.5f, 1);
            titleRT.anchoredPosition = new Vector2(0, -10);
            titleRT.sizeDelta = new Vector2(-40, 50);

            TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "Effects";
            titleTMP.fontSize = 32;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.color = CYAN;
            titleTMP.alignment = TextAlignmentOptions.MidlineLeft;
            titleTMP.enableAutoSizing = true;
            titleTMP.fontSizeMin = 24;
            titleTMP.fontSizeMax = 32;

            // Effect cards grid (2 columns)
            float cardWidth = 480f;
            float cardHeight = 160f;
            float spacing = 15f;
            float startY = -70f;

            for (int i = 0; i < EFFECT_IDS.Length; i++)
            {
                int col = i % 2;
                int row = i / 2;
                float x = col == 0 ? -cardWidth / 2 - spacing / 2 : cardWidth / 2 + spacing / 2;
                float y = startY - row * (cardHeight + spacing);

                CreateEffectCard(section.transform, i, new Vector2(x, y), new Vector2(cardWidth, cardHeight));
            }

            // Set section height
            int totalRows = (EFFECT_IDS.Length + 1) / 2;
            float sectionHeight = 70f + totalRows * (cardHeight + spacing) + 20f;
            sectionRT.sizeDelta = new Vector2(0, sectionHeight);

            EditorUtility.SetDirty(canvas.gameObject);
            Debug.Log($"[ShopEffectsTabBuilder] Effects tab creado con {EFFECT_IDS.Length} cards");
        }

        private static void CreateEffectCard(Transform parent, int index, Vector2 pos, Vector2 size)
        {
            string cardName = $"EffectCard_{EFFECT_IDS[index]}";
            GameObject card = new GameObject(cardName);
            card.transform.SetParent(parent, false);

            RectTransform cardRT = card.AddComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0.5f, 1);
            cardRT.anchorMax = new Vector2(0.5f, 1);
            cardRT.pivot = new Vector2(0.5f, 1);
            cardRT.anchoredPosition = pos;
            cardRT.sizeDelta = size;

            // Background with gradient colors
            Image bg = card.AddComponent<Image>();
            bg.color = Color.Lerp(EFFECT_PRIMARY[index], EFFECT_SECONDARY[index], 0.3f) * 0.25f + BG_CARD * 0.75f;

            // Outline
            var outline = card.AddComponent<Outline>();
            outline.effectColor = Color.Lerp(EFFECT_PRIMARY[index], EFFECT_SECONDARY[index], 0.5f) * 0.5f;
            outline.effectDistance = new Vector2(1, -1);

            // Effect name
            GameObject nameObj = new GameObject("EffectNameText");
            nameObj.transform.SetParent(card.transform, false);
            RectTransform nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0.5f);
            nameRT.anchorMax = new Vector2(0.6f, 1);
            nameRT.offsetMin = new Vector2(15, 5);
            nameRT.offsetMax = new Vector2(-5, -10);

            TextMeshProUGUI nameTMP = nameObj.AddComponent<TextMeshProUGUI>();
            nameTMP.text = EFFECT_IDS[index].Replace("_", " ");
            nameTMP.fontSize = 24;
            nameTMP.fontStyle = FontStyles.Bold;
            nameTMP.color = TEXT_WHITE;
            nameTMP.alignment = TextAlignmentOptions.MidlineLeft;
            nameTMP.enableAutoSizing = true;
            nameTMP.fontSizeMin = 18;
            nameTMP.fontSizeMax = 24;

            // Price label
            GameObject priceObj = new GameObject("PriceText");
            priceObj.transform.SetParent(card.transform, false);
            RectTransform priceRT = priceObj.AddComponent<RectTransform>();
            priceRT.anchorMin = new Vector2(0, 0);
            priceRT.anchorMax = new Vector2(0.6f, 0.5f);
            priceRT.offsetMin = new Vector2(15, 5);
            priceRT.offsetMax = new Vector2(-5, -5);

            TextMeshProUGUI priceTMP = priceObj.AddComponent<TextMeshProUGUI>();
            priceTMP.text = EFFECT_PRICES[index];
            priceTMP.fontSize = 20;
            priceTMP.color = index == 0 ? new Color(0.3f, 1f, 0.3f) : GOLD;
            priceTMP.alignment = TextAlignmentOptions.MidlineLeft;
            priceTMP.enableAutoSizing = true;
            priceTMP.fontSizeMin = 16;
            priceTMP.fontSizeMax = 20;

            // Preview button
            GameObject btnObj = new GameObject("PreviewButton");
            btnObj.transform.SetParent(card.transform, false);
            RectTransform btnRT = btnObj.AddComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.62f, 0.15f);
            btnRT.anchorMax = new Vector2(0.98f, 0.85f);
            btnRT.offsetMin = Vector2.zero;
            btnRT.offsetMax = Vector2.zero;

            Image btnBg = btnObj.AddComponent<Image>();
            btnBg.color = new Color(EFFECT_PRIMARY[index].r, EFFECT_PRIMARY[index].g, EFFECT_PRIMARY[index].b, 0.3f);
            btnObj.AddComponent<Button>();

            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(btnObj.transform, false);
            RectTransform btnTextRT = btnTextObj.AddComponent<RectTransform>();
            btnTextRT.anchorMin = Vector2.zero;
            btnTextRT.anchorMax = Vector2.one;
            btnTextRT.offsetMin = Vector2.zero;
            btnTextRT.offsetMax = Vector2.zero;

            TextMeshProUGUI btnTMP = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnTMP.text = "Preview";
            btnTMP.fontSize = 20;
            btnTMP.fontStyle = FontStyles.Bold;
            btnTMP.color = TEXT_WHITE;
            btnTMP.alignment = TextAlignmentOptions.Center;
            btnTMP.enableAutoSizing = true;
            btnTMP.fontSizeMin = 16;
            btnTMP.fontSizeMax = 20;
        }

        private static GameObject FindOrCreate(Transform parent, string name)
        {
            Transform t = parent.Find(name);
            if (t != null) return t.gameObject;
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go;
        }

        private static T GetOrAdd<T>(GameObject go) where T : Component
        {
            T comp = go.GetComponent<T>();
            if (comp == null) comp = go.AddComponent<T>();
            return comp;
        }

        private static Transform FindDeep(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                Transform found = FindDeep(child, name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
