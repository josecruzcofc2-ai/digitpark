using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;
using DigitPark.UI.Items;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor tool que crea el prefab MissionCard.prefab.
    /// Sigue el patron de MonetizationPrefabBuilder.CreateMissionItemPrefab().
    /// </summary>
    public class MissionCardPrefabBuilder : EditorWindow
    {
        // Colores del tema neon
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CARD_BG = new Color(0.06f, 0.08f, 0.12f, 1f);
        private static readonly Color GREEN = new Color(0.3f, 0.9f, 0.4f, 1f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color PROGRESS_BG = new Color(0.1f, 0.12f, 0.15f, 1f);
        private static readonly Color PROGRESS_FILL = new Color(0f, 0.8f, 0.4f, 1f);
        private static readonly Color COMPLETED_BG = new Color(0.1f, 0.25f, 0.15f, 0.95f);

        private const float MISSION_CARD_HEIGHT = 100f;

        [MenuItem("DigitPark/Missions/Create MissionCard Prefab")]
        public static void CreateMissionCardPrefabFromScript()
        {
            CreateMissionCardPrefab();
        }

        private static void CreateMissionCardPrefab()
        {
            GameObject item = new GameObject("MissionCard");

            // RectTransform
            RectTransform rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, MISSION_CARD_HEIGHT);

            LayoutElement le = item.AddComponent<LayoutElement>();
            le.preferredHeight = MISSION_CARD_HEIGHT;
            le.flexibleWidth = 1;

            // Background
            Image bg = item.AddComponent<Image>();
            bg.color = CARD_BG;

            // CategoryBorder (4px izquierda, full height, cyan)
            GameObject categoryBorder = CreateImageElement(item.transform, "CategoryBorder",
                new Vector2(0, 0), new Vector2(0, 1),
                new Vector2(0, 0), new Vector2(4, 0));
            categoryBorder.GetComponent<Image>().color = CYAN_NEON;

            // IconContainer
            GameObject iconContainer = CreateContainer(item.transform, "IconContainer",
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(12, -30), new Vector2(72, 30));

            // IconGlow
            GameObject iconGlow = CreateImageElement(iconContainer.transform, "IconGlow",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-35, -35), new Vector2(35, 35));
            iconGlow.GetComponent<Image>().color = new Color(CYAN_NEON.r, CYAN_NEON.g, CYAN_NEON.b, 0.15f);

            // MissionIcon (50x50)
            GameObject missionIcon = CreateImageElement(iconContainer.transform, "MissionIcon",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-25, -25), new Vector2(25, 25));
            missionIcon.GetComponent<Image>().color = CYAN_NEON;
            missionIcon.GetComponent<Image>().preserveAspect = true;

            // Content
            GameObject content = CreateContainer(item.transform, "Content",
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(80, 8), new Vector2(-115, -8));

            // TitleText (bold, white)
            CreateTextElement(content.transform, "TitleText", "Mission Title",
                new Vector2(0, 0.65f), new Vector2(1, 1), (int)FontSizes.Body, Color.white, FontStyles.Bold, TextAlignmentOptions.Left);

            // DescriptionText (gray)
            CreateTextElement(content.transform, "DescriptionText", "Mission description",
                new Vector2(0, 0.35f), new Vector2(1, 0.65f), (int)FontSizes.Body, new Color(0.6f, 0.6f, 0.6f), FontStyles.Normal, TextAlignmentOptions.Left);

            // ProgressBar
            GameObject progressBg = CreateImageElement(content.transform, "ProgressBar",
                new Vector2(0, 0), new Vector2(0.75f, 0.3f),
                new Vector2(0, 3), new Vector2(0, -3));
            progressBg.GetComponent<Image>().color = PROGRESS_BG;

            Slider slider = progressBg.AddComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 100;
            slider.value = 60;

            GameObject fill = CreateImageElement(progressBg.transform, "ProgressFill",
                new Vector2(0, 0), new Vector2(0.6f, 1),
                Vector2.zero, Vector2.zero);
            fill.GetComponent<Image>().color = PROGRESS_FILL;
            slider.fillRect = fill.GetComponent<RectTransform>();

            // ProgressText ("0/3")
            CreateTextElement(content.transform, "ProgressText", "0/3",
                new Vector2(0.78f, 0), new Vector2(1, 0.3f), (int)FontSizes.Body, Color.white, FontStyles.Bold, TextAlignmentOptions.Left);

            // RewardSection
            GameObject rewardSection = CreateContainer(item.transform, "RewardSection",
                new Vector2(1, 0), new Vector2(1, 1),
                new Vector2(-110, 8), new Vector2(-8, -8));

            // RewardIcon (30x30)
            GameObject rewardIcon = CreateImageElement(rewardSection.transform, "RewardIcon",
                new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f),
                new Vector2(-15, -15), new Vector2(15, 15));
            rewardIcon.GetComponent<Image>().color = GOLD;
            rewardIcon.GetComponent<Image>().preserveAspect = true;

            // RewardAmountText ("+50", gold)
            CreateTextElement(rewardSection.transform, "RewardAmountText", "+50",
                new Vector2(0, 0), new Vector2(1, 0.4f), (int)FontSizes.Body, GOLD, FontStyles.Bold, TextAlignmentOptions.Center);

            // DifficultyIndicator (3px bottom strip)
            GameObject diffIndicator = CreateImageElement(item.transform, "DifficultyIndicator",
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 0), new Vector2(0, 3));
            diffIndicator.GetComponent<Image>().color = GREEN;

            // ClaimButton (verde, "Reclamar", hidden)
            GameObject claimBtn = CreateButton(item.transform, "ClaimButton", "Claim", GREEN, Color.black,
                new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-85, -18), new Vector2(-8, 18));
            claimBtn.SetActive(false);

            // CompletedOverlay (hidden)
            GameObject completedOverlay = CreateOverlay(item.transform, "CompletedOverlay", COMPLETED_BG);
            completedOverlay.SetActive(false);

            // ClaimedCheckmark (TMP "V", hidden)
            GameObject claimedCheck = CreateTextElement(item.transform, "ClaimedCheckmark", "V",
                new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-55, -20), new Vector2(-15, 20),
                (int)FontSizes.Body, GREEN, FontStyles.Bold, TextAlignmentOptions.Center);
            claimedCheck.SetActive(false);

            // Add MissionCardUI Component
            item.AddComponent<MissionCardUI>();

            // Save Prefab
            SavePrefab(item, "Assets/_Project/Prefabs/Monetization/DailyMissions/MissionCard.prefab");
        }

        #region Helper Methods (same as MonetizationPrefabBuilder)

        private static GameObject CreateContainer(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            return obj;
        }

        private static GameObject CreateImageElement(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            Image img = obj.AddComponent<Image>();
            img.raycastTarget = false;
            return obj;
        }

        private static GameObject CreateTextElement(Transform parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, int fontSize, Color color, FontStyles style, TextAlignmentOptions alignment)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = alignment;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinBody;
            tmp.fontSizeMax = fontSize > 0 ? fontSize : FontSizes.Body;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;
            return obj;
        }

        private static GameObject CreateTextElement(Transform parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, int fontSize, Color color, FontStyles style, TextAlignmentOptions alignment)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = alignment;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinBody;
            tmp.fontSizeMax = fontSize > 0 ? fontSize : FontSizes.Body;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;
            return obj;
        }

        private static GameObject CreateButton(Transform parent, string name, string text, Color bgColor, Color textColor, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            Image img = obj.AddComponent<Image>();
            img.color = bgColor;

            Button btn = obj.AddComponent<Button>();
            btn.targetGraphic = img;

            if (!string.IsNullOrEmpty(text))
            {
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(obj.transform, false);
                RectTransform textRt = textObj.AddComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.offsetMin = Vector2.zero;
                textRt.offsetMax = Vector2.zero;
                TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
                tmp.text = text;
                tmp.fontSize = FontSizes.Body;
                tmp.color = textColor;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.raycastTarget = false;
            }

            return obj;
        }

        private static GameObject CreateOverlay(Transform parent, string name, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Image img = obj.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = true;
            return obj;
        }

        private static void SavePrefab(GameObject obj, string path)
        {
            string directory = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            DestroyImmediate(obj);

            Debug.Log($"[MissionCardPrefabBuilder] Prefab creado: {path}");
            Selection.activeObject = prefab;
        }

        #endregion
    }
}
