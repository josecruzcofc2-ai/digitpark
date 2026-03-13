using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.Progression;

namespace DigitPark.Editor
{
    /// <summary>
    /// Genera el prefab UI/LevelUpPanel en Resources/.
    /// Menu: DigitPark/Progression/Build LevelUp Panel Prefab
    /// </summary>
    public static class LevelUpPanelBuilder
    {
        // Colors
        private static readonly Color CYAN_NEON = new Color(0f, 0.898f, 1f);
        private static readonly Color MAGENTA_NEON = new Color(1f, 0f, 0.647f);
        private static readonly Color GOLD = new Color(1f, 0.843f, 0f);
        private static readonly Color PANEL_BG = new Color(0.05f, 0.07f, 0.11f, 1f);
        private static readonly Color CARD_BG = new Color(0.08f, 0.10f, 0.16f, 1f);
        private static readonly Color XP_BAR_BG = new Color(0.15f, 0.15f, 0.20f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.6f, 0.65f);

        [MenuItem("DigitPark/Progression/Build LevelUp Panel Prefab")]
        public static void BuildPrefab()
        {
            // Ensure Resources/UI/ exists
            string resourcesPath = "Assets/_Project/Resources/UI";
            if (!AssetDatabase.IsValidFolder(resourcesPath))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Project/Resources"))
                    AssetDatabase.CreateFolder("Assets/_Project", "Resources");
                AssetDatabase.CreateFolder("Assets/_Project/Resources", "UI");
            }

            // Root GO
            GameObject root = new GameObject("LevelUpPanel");
            RectTransform rootRT = root.AddComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            Canvas canvas = root.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 100;
            root.AddComponent<GraphicRaycaster>();

            var panel = root.AddComponent<LevelUpPanel>();

            // ── OVERLAY ──
            GameObject overlayGO = CreateChild(root.transform, "Overlay");
            RectTransform overlayRT = overlayGO.GetComponent<RectTransform>();
            StretchFull(overlayRT);
            Image overlayImg = overlayGO.AddComponent<Image>();
            overlayImg.color = new Color(0, 0, 0, 0);
            overlayImg.raycastTarget = true;

            // ── PANEL ROOT ──
            GameObject panelRoot = CreateChild(root.transform, "PanelRoot");
            RectTransform panelRootRT = panelRoot.GetComponent<RectTransform>();
            panelRootRT.anchorMin = new Vector2(0.08f, 0.12f);
            panelRootRT.anchorMax = new Vector2(0.92f, 0.88f);
            panelRootRT.offsetMin = Vector2.zero;
            panelRootRT.offsetMax = Vector2.zero;

            Image panelBg = panelRoot.AddComponent<Image>();
            panelBg.color = PANEL_BG;

            var outline = panelRoot.AddComponent<Outline>();
            outline.effectColor = CYAN_NEON * 0.6f;
            outline.effectDistance = new Vector2(2, -2);

            CanvasGroup panelCG = panelRoot.AddComponent<CanvasGroup>();

            // ── LEVEL UP LABEL ──
            GameObject levelUpLabelGO = CreateChild(panelRoot.transform, "LevelUpLabel");
            RectTransform labelRT = levelUpLabelGO.GetComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0.85f);
            labelRT.anchorMax = new Vector2(1, 0.95f);
            labelRT.offsetMin = new Vector2(20, 0);
            labelRT.offsetMax = new Vector2(-20, 0);

            TextMeshProUGUI levelUpTMP = levelUpLabelGO.AddComponent<TextMeshProUGUI>();
            levelUpTMP.text = "LEVEL UP!";
            levelUpTMP.fontSize = 28;
            levelUpTMP.fontStyle = FontStyles.Bold;
            levelUpTMP.color = CYAN_NEON;
            levelUpTMP.alignment = TextAlignmentOptions.Center;
            levelUpTMP.enableAutoSizing = true;
            levelUpTMP.fontSizeMin = 20;
            levelUpTMP.fontSizeMax = 36;

            // ── MILESTONE LABEL ──
            GameObject milestoneLabelGO = CreateChild(panelRoot.transform, "MilestoneLabel");
            RectTransform milestoneRT = milestoneLabelGO.GetComponent<RectTransform>();
            milestoneRT.anchorMin = new Vector2(0.2f, 0.91f);
            milestoneRT.anchorMax = new Vector2(0.8f, 0.98f);
            milestoneRT.offsetMin = Vector2.zero;
            milestoneRT.offsetMax = Vector2.zero;

            TextMeshProUGUI milestoneTMP = milestoneLabelGO.AddComponent<TextMeshProUGUI>();
            milestoneTMP.text = "MILESTONE!";
            milestoneTMP.fontSize = 18;
            milestoneTMP.fontStyle = FontStyles.Bold;
            milestoneTMP.color = GOLD;
            milestoneTMP.alignment = TextAlignmentOptions.Center;
            milestoneTMP.enableAutoSizing = true;
            milestoneTMP.fontSizeMin = 14;
            milestoneTMP.fontSizeMax = 22;

            // ── BADGE AREA ──
            GameObject badgeArea = CreateChild(panelRoot.transform, "BadgeArea");
            RectTransform badgeAreaRT = badgeArea.GetComponent<RectTransform>();
            badgeAreaRT.anchorMin = new Vector2(0.25f, 0.60f);
            badgeAreaRT.anchorMax = new Vector2(0.75f, 0.85f);
            badgeAreaRT.offsetMin = Vector2.zero;
            badgeAreaRT.offsetMax = Vector2.zero;

            // Badge Ring
            GameObject ringGO = CreateChild(badgeArea.transform, "BadgeRing");
            RectTransform ringRT = ringGO.GetComponent<RectTransform>();
            StretchFull(ringRT);
            Image ringImg = ringGO.AddComponent<Image>();
            ringImg.color = CYAN_NEON;
            var ringOutline = ringGO.AddComponent<Outline>();
            ringOutline.effectColor = MAGENTA_NEON * 0.5f;
            ringOutline.effectDistance = new Vector2(3, -3);

            // Old Level Text
            GameObject oldLevelGO = CreateChild(badgeArea.transform, "OldLevelText");
            RectTransform oldLevelRT = oldLevelGO.GetComponent<RectTransform>();
            StretchFull(oldLevelRT);
            TextMeshProUGUI oldLevelTMP = oldLevelGO.AddComponent<TextMeshProUGUI>();
            oldLevelTMP.text = "49";
            oldLevelTMP.fontSize = 72;
            oldLevelTMP.fontStyle = FontStyles.Bold;
            oldLevelTMP.color = Color.white;
            oldLevelTMP.alignment = TextAlignmentOptions.Center;
            oldLevelTMP.enableAutoSizing = true;
            oldLevelTMP.fontSizeMin = 48;
            oldLevelTMP.fontSizeMax = 96;

            // New Level Text
            GameObject newLevelGO = CreateChild(badgeArea.transform, "NewLevelText");
            RectTransform newLevelRT = newLevelGO.GetComponent<RectTransform>();
            StretchFull(newLevelRT);
            TextMeshProUGUI newLevelTMP = newLevelGO.AddComponent<TextMeshProUGUI>();
            newLevelTMP.text = "50";
            newLevelTMP.fontSize = 96;
            newLevelTMP.fontStyle = FontStyles.Bold;
            newLevelTMP.color = Color.white;
            newLevelTMP.alignment = TextAlignmentOptions.Center;
            newLevelTMP.enableAutoSizing = true;
            newLevelTMP.fontSizeMin = 48;
            newLevelTMP.fontSizeMax = 96;

            // ── XP BAR SECTION ──
            GameObject xpSection = CreateChild(panelRoot.transform, "XPBarSection");
            RectTransform xpSectionRT = xpSection.GetComponent<RectTransform>();
            xpSectionRT.anchorMin = new Vector2(0.08f, 0.50f);
            xpSectionRT.anchorMax = new Vector2(0.92f, 0.60f);
            xpSectionRT.offsetMin = Vector2.zero;
            xpSectionRT.offsetMax = Vector2.zero;
            CanvasGroup xpBarCG = xpSection.AddComponent<CanvasGroup>();

            // Experience title
            GameObject expTitleGO = CreateChild(xpSection.transform, "ExperienceTitle");
            RectTransform expTitleRT = expTitleGO.GetComponent<RectTransform>();
            expTitleRT.anchorMin = new Vector2(0, 0.6f);
            expTitleRT.anchorMax = new Vector2(0.5f, 1);
            expTitleRT.offsetMin = Vector2.zero;
            expTitleRT.offsetMax = Vector2.zero;
            TextMeshProUGUI expTitleTMP = expTitleGO.AddComponent<TextMeshProUGUI>();
            expTitleTMP.text = "EXPERIENCE";
            expTitleTMP.fontSize = 14;
            expTitleTMP.fontStyle = FontStyles.Bold;
            expTitleTMP.color = TEXT_SECONDARY;
            expTitleTMP.alignment = TextAlignmentOptions.MidlineLeft;
            expTitleTMP.enableAutoSizing = true;
            expTitleTMP.fontSizeMin = 10;
            expTitleTMP.fontSizeMax = 16;

            // XP label (right side)
            GameObject xpLabelGO = CreateChild(xpSection.transform, "XPLabel");
            RectTransform xpLabelRT = xpLabelGO.GetComponent<RectTransform>();
            xpLabelRT.anchorMin = new Vector2(0.5f, 0.6f);
            xpLabelRT.anchorMax = new Vector2(1, 1);
            xpLabelRT.offsetMin = Vector2.zero;
            xpLabelRT.offsetMax = Vector2.zero;
            TextMeshProUGUI xpLabelTMP = xpLabelGO.AddComponent<TextMeshProUGUI>();
            xpLabelTMP.text = "250 / 1,000 XP";
            xpLabelTMP.fontSize = 14;
            xpLabelTMP.color = Color.white;
            xpLabelTMP.alignment = TextAlignmentOptions.MidlineRight;
            xpLabelTMP.enableAutoSizing = true;
            xpLabelTMP.fontSizeMin = 10;
            xpLabelTMP.fontSizeMax = 16;

            // Bar background
            GameObject barBgGO = CreateChild(xpSection.transform, "XPBarBg");
            RectTransform barBgRT = barBgGO.GetComponent<RectTransform>();
            barBgRT.anchorMin = new Vector2(0, 0);
            barBgRT.anchorMax = new Vector2(1, 0.55f);
            barBgRT.offsetMin = Vector2.zero;
            barBgRT.offsetMax = Vector2.zero;
            Image barBgImg = barBgGO.AddComponent<Image>();
            barBgImg.color = XP_BAR_BG;

            // Bar fill
            GameObject barFillGO = CreateChild(barBgGO.transform, "XPBarFill");
            RectTransform barFillRT = barFillGO.GetComponent<RectTransform>();
            StretchFull(barFillRT);
            Image barFillImg = barFillGO.AddComponent<Image>();
            barFillImg.color = CYAN_NEON;
            barFillImg.type = Image.Type.Filled;
            barFillImg.fillMethod = Image.FillMethod.Horizontal;
            barFillImg.fillAmount = 0.68f;

            // Bar flash overlay
            GameObject barFlashGO = CreateChild(barBgGO.transform, "XPBarFlash");
            RectTransform barFlashRT = barFlashGO.GetComponent<RectTransform>();
            StretchFull(barFlashRT);
            Image barFlashImg = barFlashGO.AddComponent<Image>();
            barFlashImg.color = new Color(1, 1, 1, 0);
            barFlashImg.raycastTarget = false;

            // ── REWARD CARD ──
            GameObject rewardCardGO = CreateChild(panelRoot.transform, "RewardCard");
            RectTransform rewardCardRT = rewardCardGO.GetComponent<RectTransform>();
            rewardCardRT.anchorMin = new Vector2(0.06f, 0.22f);
            rewardCardRT.anchorMax = new Vector2(0.94f, 0.48f);
            rewardCardRT.offsetMin = Vector2.zero;
            rewardCardRT.offsetMax = Vector2.zero;

            Image rewardCardBg = rewardCardGO.AddComponent<Image>();
            rewardCardBg.color = CARD_BG;
            var rewardOutline = rewardCardGO.AddComponent<Outline>();
            rewardOutline.effectColor = GOLD * 0.5f;
            rewardOutline.effectDistance = new Vector2(1, -1);
            CanvasGroup rewardCG = rewardCardGO.AddComponent<CanvasGroup>();

            // Reward display component
            var rewardDisplay = rewardCardGO.AddComponent<LevelUpRewardDisplay>();

            // Reward Unlocked Label
            GameObject rewardLabelGO = CreateChild(rewardCardGO.transform, "RewardUnlockedLabel");
            RectTransform rewardLabelRT = rewardLabelGO.GetComponent<RectTransform>();
            rewardLabelRT.anchorMin = new Vector2(0, 0.65f);
            rewardLabelRT.anchorMax = new Vector2(1, 0.95f);
            rewardLabelRT.offsetMin = new Vector2(10, 0);
            rewardLabelRT.offsetMax = new Vector2(-10, 0);
            TextMeshProUGUI rewardLabelTMP = rewardLabelGO.AddComponent<TextMeshProUGUI>();
            rewardLabelTMP.text = "REWARD UNLOCKED!";
            rewardLabelTMP.fontSize = 16;
            rewardLabelTMP.fontStyle = FontStyles.Bold;
            rewardLabelTMP.color = GOLD;
            rewardLabelTMP.alignment = TextAlignmentOptions.Center;
            rewardLabelTMP.enableAutoSizing = true;
            rewardLabelTMP.fontSizeMin = 12;
            rewardLabelTMP.fontSizeMax = 20;

            // Reward Icon
            GameObject rewardIconGO = CreateChild(rewardCardGO.transform, "RewardIcon");
            RectTransform rewardIconRT = rewardIconGO.GetComponent<RectTransform>();
            rewardIconRT.anchorMin = new Vector2(0.35f, 0.15f);
            rewardIconRT.anchorMax = new Vector2(0.65f, 0.65f);
            rewardIconRT.offsetMin = Vector2.zero;
            rewardIconRT.offsetMax = Vector2.zero;
            Image rewardIconImg = rewardIconGO.AddComponent<Image>();
            rewardIconImg.color = Color.white;
            rewardIconImg.preserveAspect = true;

            // Reward Name
            GameObject rewardNameGO = CreateChild(rewardCardGO.transform, "RewardNameText");
            RectTransform rewardNameRT = rewardNameGO.GetComponent<RectTransform>();
            rewardNameRT.anchorMin = new Vector2(0, 0);
            rewardNameRT.anchorMax = new Vector2(1, 0.20f);
            rewardNameRT.offsetMin = new Vector2(10, 0);
            rewardNameRT.offsetMax = new Vector2(-10, 0);
            TextMeshProUGUI rewardNameTMP = rewardNameGO.AddComponent<TextMeshProUGUI>();
            rewardNameTMP.text = "Frame: Bronze";
            rewardNameTMP.fontSize = 22;
            rewardNameTMP.fontStyle = FontStyles.Bold;
            rewardNameTMP.color = Color.white;
            rewardNameTMP.alignment = TextAlignmentOptions.Center;
            rewardNameTMP.enableAutoSizing = true;
            rewardNameTMP.fontSizeMin = 16;
            rewardNameTMP.fontSizeMax = 22;

            // Reward Hint Text
            GameObject rewardHintGO = CreateChild(panelRoot.transform, "RewardHintText");
            RectTransform rewardHintRT = rewardHintGO.GetComponent<RectTransform>();
            rewardHintRT.anchorMin = new Vector2(0.1f, 0.16f);
            rewardHintRT.anchorMax = new Vector2(0.9f, 0.22f);
            rewardHintRT.offsetMin = Vector2.zero;
            rewardHintRT.offsetMax = Vector2.zero;
            TextMeshProUGUI rewardHintTMP = rewardHintGO.AddComponent<TextMeshProUGUI>();
            rewardHintTMP.text = "Equip it from your Profile";
            rewardHintTMP.fontSize = 14;
            rewardHintTMP.color = TEXT_SECONDARY;
            rewardHintTMP.alignment = TextAlignmentOptions.Center;
            rewardHintTMP.enableAutoSizing = true;
            rewardHintTMP.fontSizeMin = 10;
            rewardHintTMP.fontSizeMax = 16;

            // Next Reward Hint (shown when no reward)
            GameObject nextRewardGO = CreateChild(panelRoot.transform, "NextRewardHint");
            RectTransform nextRewardRT = nextRewardGO.GetComponent<RectTransform>();
            nextRewardRT.anchorMin = new Vector2(0.1f, 0.30f);
            nextRewardRT.anchorMax = new Vector2(0.9f, 0.48f);
            nextRewardRT.offsetMin = Vector2.zero;
            nextRewardRT.offsetMax = Vector2.zero;
            TextMeshProUGUI nextRewardTMP = nextRewardGO.AddComponent<TextMeshProUGUI>();
            nextRewardTMP.text = "Next reward at level 25";
            nextRewardTMP.fontSize = 16;
            nextRewardTMP.color = TEXT_SECONDARY;
            nextRewardTMP.alignment = TextAlignmentOptions.Center;
            nextRewardTMP.enableAutoSizing = true;
            nextRewardTMP.fontSizeMin = 12;
            nextRewardTMP.fontSizeMax = 18;

            // ── CONTINUE BUTTON ──
            GameObject btnGO = CreateChild(panelRoot.transform, "ContinueButton");
            RectTransform btnRT = btnGO.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.2f, 0.03f);
            btnRT.anchorMax = new Vector2(0.8f, 0.13f);
            btnRT.offsetMin = Vector2.zero;
            btnRT.offsetMax = Vector2.zero;

            Image btnBg = btnGO.AddComponent<Image>();
            btnBg.color = CYAN_NEON;
            Button btnComp = btnGO.AddComponent<Button>();
            CanvasGroup btnCG = btnGO.AddComponent<CanvasGroup>();

            GameObject btnTextGO = CreateChild(btnGO.transform, "ContinueButtonText");
            RectTransform btnTextRT = btnTextGO.GetComponent<RectTransform>();
            StretchFull(btnTextRT);
            btnTextRT.offsetMin = new Vector2(10, 0);
            btnTextRT.offsetMax = new Vector2(-10, 0);
            TextMeshProUGUI btnTMP = btnTextGO.AddComponent<TextMeshProUGUI>();
            btnTMP.text = "Continue";
            btnTMP.fontSize = 22;
            btnTMP.fontStyle = FontStyles.Bold;
            btnTMP.color = Color.white;
            btnTMP.alignment = TextAlignmentOptions.Center;
            btnTMP.enableAutoSizing = true;
            btnTMP.fontSizeMin = 16;
            btnTMP.fontSizeMax = 26;

            // ── WIRE SERIALIZED FIELDS ──
            SerializedObject so = new SerializedObject(panel);
            so.FindProperty("overlay").objectReferenceValue = overlayImg;
            so.FindProperty("panelCanvasGroup").objectReferenceValue = panelCG;
            so.FindProperty("panelRoot").objectReferenceValue = panelRootRT;
            so.FindProperty("levelUpLabel").objectReferenceValue = levelUpTMP;
            so.FindProperty("oldLevelText").objectReferenceValue = oldLevelTMP;
            so.FindProperty("newLevelText").objectReferenceValue = newLevelTMP;
            so.FindProperty("badgeRing").objectReferenceValue = ringImg;
            so.FindProperty("milestoneLabel").objectReferenceValue = milestoneTMP;
            so.FindProperty("xpBarGroup").objectReferenceValue = xpBarCG;
            so.FindProperty("xpBarFill").objectReferenceValue = barFillImg;
            so.FindProperty("xpBarFlash").objectReferenceValue = barFlashImg;
            so.FindProperty("xpLabel").objectReferenceValue = xpLabelTMP;
            so.FindProperty("experienceTitle").objectReferenceValue = expTitleTMP;
            so.FindProperty("rewardCard").objectReferenceValue = rewardCardRT;
            so.FindProperty("rewardCardGroup").objectReferenceValue = rewardCG;
            so.FindProperty("rewardUnlockedLabel").objectReferenceValue = rewardLabelTMP;
            so.FindProperty("rewardNameText").objectReferenceValue = rewardNameTMP;
            so.FindProperty("rewardHintText").objectReferenceValue = rewardHintTMP;
            so.FindProperty("rewardIcon").objectReferenceValue = rewardIconImg;
            so.FindProperty("nextRewardHint").objectReferenceValue = nextRewardTMP;
            so.FindProperty("continueButton").objectReferenceValue = btnComp;
            so.FindProperty("continueButtonGroup").objectReferenceValue = btnCG;
            so.FindProperty("continueButtonRT").objectReferenceValue = btnRT;
            so.ApplyModifiedPropertiesWithoutUndo();

            // ── SAVE PREFAB ──
            string prefabPath = "Assets/_Project/Resources/UI/LevelUpPanel.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);

            Debug.Log($"[LevelUpPanelBuilder] Prefab creado en {prefabPath}");
        }

        private static GameObject CreateChild(Transform parent, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
