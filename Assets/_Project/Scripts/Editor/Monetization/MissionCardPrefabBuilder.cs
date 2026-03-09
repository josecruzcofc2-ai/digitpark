using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
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
        private static readonly Color DESC_GRAY = new Color(0.6f, 0.6f, 0.65f, 1f);
        private static readonly Color PROGRESS_TEXT_COLOR = new Color(0.55f, 0.55f, 0.6f, 1f);

        private const float MISSION_CARD_HEIGHT = 200f;

        // Font sizes for card layout (200px card)
        private const float CARD_TITLE_SIZE = 32f;
        private const float CARD_TITLE_MIN = 20f;
        private const float CARD_DESC_SIZE = 26f;
        private const float CARD_DESC_MIN = 18f;
        private const float CARD_PROGRESS_SIZE = 24f;
        private const float CARD_PROGRESS_MIN = 16f;
        private const float CARD_REWARD_SIZE = 32f;
        private const float CARD_REWARD_MIN = 20f;
        private const float CARD_BUTTON_SIZE = 26f;
        private const float CARD_BUTTON_MIN = 18f;
        private const float CARD_CHECK_SIZE = 42f;
        private const float CARD_CHECK_MIN = 30f;

        [MenuItem("DigitPark/Prefabs/Monetization/MissionCard")]
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

            // CategoryBorder (8px izquierda, full height, cyan)
            GameObject categoryBorder = CreateImageElement(item.transform, "CategoryBorder",
                new Vector2(0, 0), new Vector2(0, 1),
                new Vector2(0, 0), new Vector2(8, 0));
            categoryBorder.GetComponent<Image>().color = CYAN_NEON;

            // IconContainer
            GameObject iconContainer = CreateContainer(item.transform, "IconContainer",
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(24, -60), new Vector2(144, 60));

            // IconGlow
            GameObject iconGlow = CreateImageElement(iconContainer.transform, "IconGlow",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-70, -70), new Vector2(70, 70));
            iconGlow.GetComponent<Image>().color = new Color(CYAN_NEON.r, CYAN_NEON.g, CYAN_NEON.b, 0.15f);

            // MissionIcon (100x100)
            GameObject missionIcon = CreateImageElement(iconContainer.transform, "MissionIcon",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-50, -50), new Vector2(50, 50));
            missionIcon.GetComponent<Image>().color = CYAN_NEON;
            missionIcon.GetComponent<Image>().preserveAspect = true;

            // Content
            GameObject content = CreateContainer(item.transform, "Content",
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(160, 16), new Vector2(-230, -16));

            // TitleText (bold, white)
            GameObject titleObj = CreateTextElement(content.transform, "TitleText", "Mission Title",
                new Vector2(0, 0.65f), new Vector2(1, 1), (int)CARD_TITLE_SIZE, Color.white, FontStyles.Bold, TextAlignmentOptions.Left,
                CARD_TITLE_MIN);

            // DescriptionText (gray)
            GameObject descObj = CreateTextElement(content.transform, "DescriptionText", "Mission description",
                new Vector2(0, 0.35f), new Vector2(1, 0.65f), (int)CARD_DESC_SIZE, DESC_GRAY, FontStyles.Normal, TextAlignmentOptions.Left,
                CARD_DESC_MIN);

            // ProgressBar
            GameObject progressBg = CreateImageElement(content.transform, "ProgressBar",
                new Vector2(0, 0), new Vector2(0.75f, 0.3f),
                new Vector2(0, 6), new Vector2(0, -6));
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
            GameObject progressTextObj = CreateTextElement(content.transform, "ProgressText", "0/3",
                new Vector2(0.78f, 0), new Vector2(1, 0.3f), (int)CARD_PROGRESS_SIZE, PROGRESS_TEXT_COLOR, FontStyles.Bold, TextAlignmentOptions.Left,
                CARD_PROGRESS_MIN);

            // RewardSection
            GameObject rewardSection = CreateContainer(item.transform, "RewardSection",
                new Vector2(1, 0), new Vector2(1, 1),
                new Vector2(-220, 16), new Vector2(-16, -16));

            // RewardIcon (60x60)
            GameObject rewardIcon = CreateImageElement(rewardSection.transform, "RewardIcon",
                new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f),
                new Vector2(-30, -30), new Vector2(30, 30));
            rewardIcon.GetComponent<Image>().color = GOLD;
            rewardIcon.GetComponent<Image>().preserveAspect = true;

            // RewardAmountText ("+50", gold)
            GameObject rewardAmountObj = CreateTextElement(rewardSection.transform, "RewardAmountText", "+50",
                new Vector2(0, 0), new Vector2(1, 0.4f), (int)CARD_REWARD_SIZE, GOLD, FontStyles.Bold, TextAlignmentOptions.Center,
                CARD_REWARD_MIN);

            // DifficultyIndicator (6px bottom strip)
            GameObject diffIndicator = CreateImageElement(item.transform, "DifficultyIndicator",
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 0), new Vector2(0, 6));
            diffIndicator.GetComponent<Image>().color = GREEN;

            // ClaimButton (verde, "Claim", hidden)
            GameObject claimBtn = CreateButton(item.transform, "ClaimButton", "Claim", GREEN, Color.black,
                new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-170, -36), new Vector2(-16, 36));
            claimBtn.SetActive(false);

            // CompletedOverlay (hidden)
            GameObject completedOverlay = CreateOverlay(item.transform, "CompletedOverlay", COMPLETED_BG);
            completedOverlay.SetActive(false);

            // ClaimedCheckmark (TMP "V", hidden)
            GameObject claimedCheck = CreateTextElement(item.transform, "ClaimedCheckmark", "\u2714",
                new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-110, -40), new Vector2(-30, 40),
                (int)CARD_CHECK_SIZE, GREEN, FontStyles.Bold, TextAlignmentOptions.Center,
                CARD_CHECK_MIN);
            claimedCheck.SetActive(false);

            // Add MissionCardUI Component and wire all SerializeField references
            MissionCardUI cardUI = item.AddComponent<MissionCardUI>();
            SerializedObject so = new SerializedObject(cardUI);
            so.FindProperty("backgroundImage").objectReferenceValue = bg;
            so.FindProperty("categoryBorder").objectReferenceValue = categoryBorder.GetComponent<Image>();
            so.FindProperty("difficultyIndicator").objectReferenceValue = diffIndicator.GetComponent<Image>();
            so.FindProperty("missionIcon").objectReferenceValue = missionIcon.GetComponent<Image>();
            so.FindProperty("iconGlow").objectReferenceValue = iconGlow.GetComponent<Image>();
            so.FindProperty("titleText").objectReferenceValue = titleObj.GetComponent<TextMeshProUGUI>();
            so.FindProperty("descriptionText").objectReferenceValue = descObj.GetComponent<TextMeshProUGUI>();
            so.FindProperty("progressText").objectReferenceValue = progressTextObj.GetComponent<TextMeshProUGUI>();
            so.FindProperty("progressBar").objectReferenceValue = slider;
            so.FindProperty("progressFill").objectReferenceValue = fill.GetComponent<Image>();
            so.FindProperty("rewardIcon").objectReferenceValue = rewardIcon.GetComponent<Image>();
            so.FindProperty("rewardAmountText").objectReferenceValue = rewardAmountObj.GetComponent<TextMeshProUGUI>();
            so.FindProperty("claimButton").objectReferenceValue = claimBtn.GetComponent<Button>();
            so.FindProperty("claimButtonText").objectReferenceValue = claimBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            so.FindProperty("completedOverlay").objectReferenceValue = completedOverlay;
            so.FindProperty("claimedCheckmark").objectReferenceValue = claimedCheck;
            so.ApplyModifiedPropertiesWithoutUndo();

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

        private static GameObject CreateTextElement(Transform parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, int fontSize, Color color, FontStyles style, TextAlignmentOptions alignment, float minFontSize = -1f)
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
            tmp.fontSizeMin = minFontSize > 0 ? minFontSize : FontSizes.AutoMinBody;
            tmp.fontSizeMax = fontSize > 0 ? fontSize : FontSizes.Body;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;
            return obj;
        }

        private static GameObject CreateTextElement(Transform parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, int fontSize, Color color, FontStyles style, TextAlignmentOptions alignment, float minFontSize = -1f)
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
            tmp.fontSizeMin = minFontSize > 0 ? minFontSize : FontSizes.AutoMinBody;
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
                tmp.fontSize = CARD_BUTTON_SIZE;
                tmp.color = textColor;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = CARD_BUTTON_MIN;
                tmp.fontSizeMax = CARD_BUTTON_SIZE;
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
