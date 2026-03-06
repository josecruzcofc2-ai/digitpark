using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;
using DigitPark.UI.Items;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para crear prefabs de items de Monetizacion
    /// Incluye: Achievements, DailyMissions, DailyRewards, Onboarding (V1)
    /// Resolucion: Portrait 9:16 (1080x1920)
    /// </summary>
    public class MonetizationPrefabBuilder : EditorWindow
    {
        // Colores del tema neon
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.5f, 1f);
        private static readonly Color DARK_BG = new Color(0.08f, 0.12f, 0.18f, 0.98f);
        private static readonly Color CARD_BG = new Color(0.06f, 0.08f, 0.12f, 1f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GREEN = new Color(0.3f, 0.9f, 0.4f, 1f);
        private static readonly Color ORANGE = new Color(1f, 0.6f, 0.2f, 1f);
        private static readonly Color PURPLE = new Color(0.7f, 0.3f, 0.9f, 1f);
        private static readonly Color RED = new Color(0.9f, 0.3f, 0.3f, 1f);
        private static readonly Color PREMIUM_GOLD = new Color(1f, 0.7f, 0.2f, 1f);
        private static readonly Color PROGRESS_BG = new Color(0.1f, 0.12f, 0.15f, 1f);
        private static readonly Color PROGRESS_FILL = new Color(0f, 0.8f, 0.4f, 1f);
        private static readonly Color LOCKED_BG = new Color(0.1f, 0.1f, 0.12f, 0.9f);
        private static readonly Color COMPLETED_BG = new Color(0.1f, 0.25f, 0.15f, 0.95f);

        // Dimensiones
        private const float ACHIEVEMENT_ITEM_HEIGHT = 100f;
        private const float CATEGORY_HEADER_HEIGHT = 50f;
        private const float MISSION_ITEM_HEIGHT = 90f;
        private const float REWARD_DAY_SIZE = 100f;
        private const float STEP_DOT_SIZE = 12f;
        private const float AVATAR_OPTION_SIZE = 90f;
        private const float PRIZE_ROW_HEIGHT = 50f;
        private const float PARTICIPANT_ITEM_HEIGHT = 70f;
        private const float PLAYER_SEARCH_HEIGHT = 80f;

        [MenuItem("DigitPark/Prefabs/Monetization/Create All Monetization Prefabs")]
        private static void CreateAllFromMenu()
        {
            CreateAllPrefabs();
        }

        [MenuItem("DigitPark/Prefabs/Monetization/Open Prefab Builder Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<MonetizationPrefabBuilder>("Monetization Prefabs");
            window.minSize = new Vector2(300, 500);
        }

        private void OnGUI()
        {
            GUILayout.Label("Monetization Prefab Builder", EditorStyles.boldLabel);
            GUILayout.Label("Resolucion: Portrait 9:16 (1080x1920)", EditorStyles.miniLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("CREAR TODOS LOS PREFABS", GUILayout.Height(40)))
                CreateAllPrefabs();

            GUILayout.Space(10);
            GUILayout.Label("O crear individualmente:", EditorStyles.miniBoldLabel);
            GUILayout.Space(5);

            if (GUILayout.Button("AchievementItem")) CreateAchievementItemPrefab();
            if (GUILayout.Button("CategoryHeader")) CreateCategoryHeaderPrefab();
            if (GUILayout.Button("MissionItem")) CreateMissionItemPrefab();
            if (GUILayout.Button("RewardDayItem")) CreateRewardDayItemPrefab();
            if (GUILayout.Button("StepDotItem")) CreateStepDotItemPrefab();
            if (GUILayout.Button("AvatarOptionItem")) CreateAvatarOptionItemPrefab();
            if (GUILayout.Button("PrizeRowItem")) CreatePrizeRowItemPrefab();
            if (GUILayout.Button("ParticipantItem")) CreateParticipantItemPrefab();
            if (GUILayout.Button("PlayerSearchItem")) CreatePlayerSearchItemPrefab();
        }

        private static void CreateAllPrefabs()
        {
            CreateAchievementItemPrefab();
            CreateCategoryHeaderPrefab();
            CreateMissionItemPrefab();
            CreateRewardDayItemPrefab();
            CreateStepDotItemPrefab();
            CreateAvatarOptionItemPrefab();
            CreatePrizeRowItemPrefab();
            CreateParticipantItemPrefab();
            CreatePlayerSearchItemPrefab();

            Debug.Log("[MonetizationPrefabBuilder] Todos los prefabs creados!");
            EditorUtility.DisplayDialog("Completado", "Todos los prefabs han sido creados exitosamente!", "OK");
        }

        #region Achievement Item Prefab

        [MenuItem("DigitPark/Prefabs/Monetization/Prefabs/Create AchievementItem")]
        private static void CreateAchievementItemPrefab()
        {
            GameObject item = new GameObject("AchievementItem");

            // RectTransform
            RectTransform rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, ACHIEVEMENT_ITEM_HEIGHT);

            LayoutElement le = item.AddComponent<LayoutElement>();
            le.preferredHeight = ACHIEVEMENT_ITEM_HEIGHT;
            le.flexibleWidth = 1;

            // Background
            Image bg = item.AddComponent<Image>();
            bg.color = CARD_BG;

            // Icon (izquierda)
            GameObject iconObj = CreateImageElement(item.transform, "Icon",
                new Vector2(0, 0), new Vector2(0, 1),
                new Vector2(15, 15), new Vector2(85, -15));
            Image iconImg = iconObj.GetComponent<Image>();
            iconImg.color = CYAN_NEON;

            // Content Container (centro)
            GameObject content = CreateContainer(item.transform, "Content",
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(100, 10), new Vector2(-120, -10));

            // Name
            CreateTextElement(content.transform, "NameText", "Achievement Name",
                new Vector2(0, 0.6f), new Vector2(1, 1), (int)FontSizes.Body, Color.white, FontStyles.Bold, TextAlignmentOptions.Left);

            // Description
            CreateTextElement(content.transform, "DescriptionText", "Achievement description here",
                new Vector2(0, 0.3f), new Vector2(1, 0.6f), (int)FontSizes.Body, new Color(0.7f, 0.7f, 0.7f), FontStyles.Bold, TextAlignmentOptions.Left);

            // Progress Bar
            GameObject progressBg = CreateImageElement(content.transform, "ProgressBar",
                new Vector2(0, 0), new Vector2(0.7f, 0.25f),
                new Vector2(0, 5), new Vector2(0, -5));
            progressBg.GetComponent<Image>().color = PROGRESS_BG;

            Slider slider = progressBg.AddComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 100;
            slider.value = 50;

            GameObject fill = CreateImageElement(progressBg.transform, "Fill",
                new Vector2(0, 0), new Vector2(0.5f, 1),
                Vector2.zero, Vector2.zero);
            fill.GetComponent<Image>().color = PROGRESS_FILL;
            slider.fillRect = fill.GetComponent<RectTransform>();

            // Progress Text
            CreateTextElement(content.transform, "ProgressText", "50/100",
                new Vector2(0.72f, 0), new Vector2(1, 0.25f), (int)FontSizes.Body, Color.white, FontStyles.Bold, TextAlignmentOptions.Left);

            // Reward Section (derecha)
            GameObject rewardSection = CreateContainer(item.transform, "RewardSection",
                new Vector2(1, 0), new Vector2(1, 1),
                new Vector2(-110, 10), new Vector2(-10, -10));

            // Reward Icon
            GameObject rewardIcon = CreateImageElement(rewardSection.transform, "RewardIcon",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-15, 10), new Vector2(15, 40));
            rewardIcon.GetComponent<Image>().color = GOLD;

            // Reward Text
            CreateTextElement(rewardSection.transform, "RewardText", "+100",
                new Vector2(0, 0), new Vector2(1, 0.4f), (int)FontSizes.Body, GOLD, FontStyles.Bold, TextAlignmentOptions.Center);

            // Claim Button
            GameObject claimBtn = CreateButton(item.transform, "ClaimButton", "Claim", GREEN, Color.black,
                new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-60, -15), new Vector2(-10, 15));
            claimBtn.SetActive(false);

            // Overlays
            GameObject completedOverlay = CreateOverlay(item.transform, "CompletedOverlay", new Color(0, 0, 0, 0.6f));
            CreateTextElement(completedOverlay.transform, "Text", "COMPLETED", Vector2.zero, Vector2.one, (int)FontSizes.Body, GREEN, FontStyles.Bold, TextAlignmentOptions.Center);
            completedOverlay.SetActive(false);

            GameObject lockedOverlay = CreateOverlay(item.transform, "LockedOverlay", LOCKED_BG);
            lockedOverlay.SetActive(false);

            // Add UI Component
            AchievementItemUI ui = item.AddComponent<AchievementItemUI>();

            // Save Prefab
            SavePrefab(item, "Assets/_Project/Prefabs/Monetization/Achievements/AchievementItem.prefab");
        }

        #endregion

        #region Category Header Prefab

        [MenuItem("DigitPark/Prefabs/Monetization/Prefabs/Create CategoryHeader")]
        private static void CreateCategoryHeaderPrefab()
        {
            GameObject item = new GameObject("CategoryHeader");

            RectTransform rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, CATEGORY_HEADER_HEIGHT);

            LayoutElement le = item.AddComponent<LayoutElement>();
            le.preferredHeight = CATEGORY_HEADER_HEIGHT;
            le.flexibleWidth = 1;

            Image bg = item.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.15f, 0.2f, 1f);

            // Category Icon
            GameObject iconObj = CreateImageElement(item.transform, "CategoryIcon",
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(15, -15), new Vector2(45, 15));
            iconObj.GetComponent<Image>().color = CYAN_NEON;

            // Category Name
            CreateTextElement(item.transform, "CategoryNameText", "Category",
                new Vector2(0, 0), new Vector2(0.6f, 1),
                new Vector2(60, 0), new Vector2(0, 0),
                (int)FontSizes.Body, Color.white, FontStyles.Bold, TextAlignmentOptions.Left);

            // Progress Text
            CreateTextElement(item.transform, "ProgressText", "5/10",
                new Vector2(0.6f, 0), new Vector2(0.85f, 1), (int)FontSizes.Body, new Color(0.7f, 0.7f, 0.7f), FontStyles.Bold, TextAlignmentOptions.Right);

            // Expand Button
            GameObject expandBtn = CreateContainer(item.transform, "ExpandButton",
                new Vector2(0.85f, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            expandBtn.AddComponent<Button>().targetGraphic = expandBtn.AddComponent<Image>();
            expandBtn.GetComponent<Image>().color = Color.clear;

            // Expand Arrow
            CreateTextElement(expandBtn.transform, "ExpandArrow", ">",
                Vector2.zero, Vector2.one, (int)FontSizes.Body, CYAN_NEON, FontStyles.Bold, TextAlignmentOptions.Center);

            // Add UI Component
            CategoryHeaderUI ui = item.AddComponent<CategoryHeaderUI>();

            SavePrefab(item, "Assets/_Project/Prefabs/Monetization/Achievements/CategoryHeader.prefab");
        }

        #endregion

        #region Daily Missions - Mission Item Prefab

        [MenuItem("DigitPark/Prefabs/Monetization/Prefabs/Create MissionItem")]
        private static void CreateMissionItemPrefab()
        {
            GameObject item = new GameObject("MissionItem");

            RectTransform rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, MISSION_ITEM_HEIGHT);

            LayoutElement le = item.AddComponent<LayoutElement>();
            le.preferredHeight = MISSION_ITEM_HEIGHT;
            le.flexibleWidth = 1;

            Image bg = item.AddComponent<Image>();
            bg.color = CARD_BG;

            // Difficulty Indicator
            GameObject diffIndicator = CreateImageElement(item.transform, "DifficultyIndicator",
                new Vector2(0, 0), new Vector2(0, 1),
                new Vector2(0, 0), new Vector2(5, 0));
            diffIndicator.GetComponent<Image>().color = GREEN;

            // Mission Icon
            GameObject iconObj = CreateImageElement(item.transform, "MissionIcon",
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(15, -25), new Vector2(65, 25));
            iconObj.GetComponent<Image>().color = CYAN_NEON;

            // Content
            GameObject content = CreateContainer(item.transform, "Content",
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(75, 8), new Vector2(-100, -8));

            CreateTextElement(content.transform, "TitleText", "Mission Title",
                new Vector2(0, 0.65f), new Vector2(1, 1), (int)FontSizes.Body, Color.white, FontStyles.Bold, TextAlignmentOptions.Left);

            CreateTextElement(content.transform, "DescriptionText", "Mission description",
                new Vector2(0, 0.35f), new Vector2(1, 0.65f), (int)FontSizes.Body, new Color(0.6f, 0.6f, 0.6f), FontStyles.Bold, TextAlignmentOptions.Left);

            // Progress Bar
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

            CreateTextElement(content.transform, "ProgressText", "6/10",
                new Vector2(0.78f, 0), new Vector2(1, 0.3f), (int)FontSizes.Body, Color.white, FontStyles.Bold, TextAlignmentOptions.Left);

            // Reward Section
            GameObject rewardSection = CreateContainer(item.transform, "RewardSection",
                new Vector2(1, 0), new Vector2(1, 1),
                new Vector2(-90, 8), new Vector2(-8, -8));

            GameObject rewardIcon = CreateImageElement(rewardSection.transform, "RewardIcon",
                new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f),
                new Vector2(-15, -15), new Vector2(15, 15));
            rewardIcon.GetComponent<Image>().color = GOLD;

            CreateTextElement(rewardSection.transform, "RewardAmountText", "+50",
                new Vector2(0, 0), new Vector2(1, 0.4f), (int)FontSizes.Body, GOLD, FontStyles.Bold, TextAlignmentOptions.Center);

            // Claim Button
            GameObject claimBtn = CreateButton(item.transform, "ClaimButton", "Claim", GREEN, Color.black,
                new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-85, -18), new Vector2(-8, 18));
            claimBtn.SetActive(false);

            // Overlays
            GameObject completedOverlay = CreateOverlay(item.transform, "CompletedOverlay", COMPLETED_BG);
            completedOverlay.SetActive(false);

            GameObject claimedCheck = CreateTextElement(item.transform, "ClaimedCheckmark", "V",
                new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-55, -20), new Vector2(-15, 20),
                (int)FontSizes.Body, GREEN, FontStyles.Bold, TextAlignmentOptions.Center);
            claimedCheck.SetActive(false);

            // Add UI Component
            MissionItemUI ui = item.AddComponent<MissionItemUI>();

            SavePrefab(item, "Assets/_Project/Prefabs/Monetization/DailyMissions/MissionItem.prefab");
        }

        #endregion

        #region Daily Rewards - Reward Day Item Prefab

        [MenuItem("DigitPark/Prefabs/Monetization/Prefabs/Create RewardDayItem")]
        private static void CreateRewardDayItemPrefab()
        {
            GameObject item = new GameObject("RewardDayItem");

            RectTransform rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(REWARD_DAY_SIZE, REWARD_DAY_SIZE + 25);

            LayoutElement le = item.AddComponent<LayoutElement>();
            le.preferredWidth = REWARD_DAY_SIZE;
            le.preferredHeight = REWARD_DAY_SIZE + 25;

            // Background
            Image bg = item.AddComponent<Image>();
            bg.color = CARD_BG;

            // Border
            GameObject border = CreateImageElement(item.transform, "BorderImage",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Outline outline = border.AddComponent<Outline>();
            outline.effectColor = new Color(0.3f, 0.3f, 0.35f);
            outline.effectDistance = new Vector2(2, 2);

            // Day Label
            CreateTextElement(item.transform, "DayLabelText", "Day 1",
                new Vector2(0, 0.85f), new Vector2(1, 1), (int)FontSizes.Body, new Color(0.7f, 0.7f, 0.7f), FontStyles.Bold, TextAlignmentOptions.Center);

            // Day Number
            CreateTextElement(item.transform, "DayNumberText", "1",
                new Vector2(0, 0.65f), new Vector2(1, 0.85f), (int)FontSizes.Body, CYAN_NEON, FontStyles.Bold, TextAlignmentOptions.Center);

            // Reward Icon
            GameObject rewardIcon = CreateImageElement(item.transform, "RewardIcon",
                new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f),
                new Vector2(-25, -25), new Vector2(25, 25));
            rewardIcon.GetComponent<Image>().color = GOLD;

            // Reward Amount
            CreateTextElement(item.transform, "RewardAmountText", "+100",
                new Vector2(0, 0), new Vector2(1, 0.2f), (int)FontSizes.Body, GOLD, FontStyles.Bold, TextAlignmentOptions.Center);

            // Current Day Glow
            GameObject glow = CreateImageElement(item.transform, "CurrentDayGlow",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-55, -55), new Vector2(55, 55));
            glow.GetComponent<Image>().color = new Color(CYAN_NEON.r, CYAN_NEON.g, CYAN_NEON.b, 0.3f);
            glow.SetActive(false);

            // Locked Overlay
            GameObject locked = CreateOverlay(item.transform, "LockedOverlay", LOCKED_BG);
            locked.SetActive(false);

            // Claimed Checkmark
            GameObject check = CreateTextElement(item.transform, "ClaimedCheckmark", "V",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-25, -25), new Vector2(25, 25),
                (int)FontSizes.BodyLarge, GREEN, FontStyles.Bold, TextAlignmentOptions.Center);
            check.SetActive(false);

            // Bonus Tag
            GameObject bonusTag = CreateContainer(item.transform, "BonusTag",
                new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-45, -20), new Vector2(-2, -2));
            Image bonusBg = bonusTag.AddComponent<Image>();
            bonusBg.color = PREMIUM_GOLD;
            CreateTextElement(bonusTag.transform, "BonusTagText", "BONUS",
                Vector2.zero, Vector2.one, (int)FontSizes.Body, Color.black, FontStyles.Bold, TextAlignmentOptions.Center);
            bonusTag.SetActive(false);

            // Button
            Button btn = item.AddComponent<Button>();
            btn.targetGraphic = bg;

            // Add UI Component
            RewardDayItemUI ui = item.AddComponent<RewardDayItemUI>();

            SavePrefab(item, "Assets/_Project/Prefabs/Monetization/DailyRewards/RewardDayItem.prefab");
        }

        #endregion

        #region Onboarding - Step Dot Item Prefab

        [MenuItem("DigitPark/Prefabs/Monetization/Prefabs/Create StepDotItem")]
        private static void CreateStepDotItemPrefab()
        {
            GameObject item = new GameObject("StepDotItem");

            RectTransform rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(STEP_DOT_SIZE, STEP_DOT_SIZE);

            LayoutElement le = item.AddComponent<LayoutElement>();
            le.preferredWidth = STEP_DOT_SIZE;
            le.preferredHeight = STEP_DOT_SIZE;

            // Dot Image
            Image dotImg = item.AddComponent<Image>();
            dotImg.color = new Color(0.3f, 0.3f, 0.35f);

            // Pulse Image (for active state)
            GameObject pulse = CreateImageElement(item.transform, "PulseImage",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-10, -10), new Vector2(10, 10));
            pulse.GetComponent<Image>().color = new Color(CYAN_NEON.r, CYAN_NEON.g, CYAN_NEON.b, 0.3f);
            pulse.SetActive(false);

            // Add UI Component
            StepDotItemUI ui = item.AddComponent<StepDotItemUI>();

            SavePrefab(item, "Assets/_Project/Prefabs/Onboarding/StepDotItem.prefab");
        }

        #endregion

        #region Onboarding - Avatar Option Item Prefab

        [MenuItem("DigitPark/Prefabs/Monetization/Prefabs/Create AvatarOptionItem")]
        private static void CreateAvatarOptionItemPrefab()
        {
            GameObject item = new GameObject("AvatarOptionItem");

            RectTransform rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(AVATAR_OPTION_SIZE, AVATAR_OPTION_SIZE + 20);

            LayoutElement le = item.AddComponent<LayoutElement>();
            le.preferredWidth = AVATAR_OPTION_SIZE;
            le.preferredHeight = AVATAR_OPTION_SIZE + 20;

            // Background
            Image bg = item.AddComponent<Image>();
            bg.color = CARD_BG;

            // Border
            GameObject border = CreateImageElement(item.transform, "BorderImage",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Outline outline = border.AddComponent<Outline>();
            outline.effectColor = new Color(0.3f, 0.3f, 0.35f);
            outline.effectDistance = new Vector2(2, 2);

            // Circular Avatar Frame
            Sprite circleSprite = GenerateCircleSprite();
            GameObject avatarFrame = CreateImageElement(item.transform, "AvatarFrame",
                new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f),
                new Vector2(-37, -37), new Vector2(37, 37));
            avatarFrame.GetComponent<Image>().sprite = circleSprite;
            avatarFrame.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.35f);

            // Circular mask
            GameObject avatarMask = CreateImageElement(avatarFrame.transform, "AvatarMask",
                new Vector2(0.06f, 0.06f), new Vector2(0.94f, 0.94f),
                Vector2.zero, Vector2.zero);
            avatarMask.GetComponent<Image>().sprite = circleSprite;
            avatarMask.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f);
            avatarMask.AddComponent<Mask>().showMaskGraphic = true;

            // Avatar Image (clipped to circle)
            GameObject avatar = CreateImageElement(avatarMask.transform, "AvatarImage",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            avatar.GetComponent<Image>().color = Color.white;
            avatar.GetComponent<Image>().preserveAspect = true;
            Sprite defAvatar = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/Social/AvatarDefault.png");
            if (defAvatar != null) avatar.GetComponent<Image>().sprite = defAvatar;

            // Name Text
            CreateTextElement(item.transform, "NameText", "",
                new Vector2(0, 0), new Vector2(1, 0.18f), (int)FontSizes.Body, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);

            // Selection Ring
            GameObject ring = CreateImageElement(item.transform, "SelectionRing",
                new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f),
                new Vector2(-42, -42), new Vector2(42, 42));
            ring.GetComponent<Image>().color = Color.clear;
            Outline ringOutline = ring.AddComponent<Outline>();
            ringOutline.effectColor = CYAN_NEON;
            ringOutline.effectDistance = new Vector2(3, 3);
            ring.SetActive(false);

            // Locked Overlay
            GameObject locked = CreateOverlay(item.transform, "LockedOverlay", LOCKED_BG);
            GameObject lockIcon = CreateTextElement(locked.transform, "LockIcon", "🔒",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-15, -15), new Vector2(15, 15),
                (int)FontSizes.Body, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
            CreateTextElement(locked.transform, "UnlockRequirementText", "Level 5",
                new Vector2(0, 0), new Vector2(1, 0.25f), (int)FontSizes.Body, new Color(0.7f, 0.7f, 0.7f), FontStyles.Bold, TextAlignmentOptions.Center);
            locked.SetActive(false);

            // Button
            Button btn = item.AddComponent<Button>();
            btn.targetGraphic = bg;

            // Add UI Component
            AvatarOptionItemUI ui = item.AddComponent<AvatarOptionItemUI>();

            SavePrefab(item, "Assets/_Project/Prefabs/Onboarding/AvatarOptionItem.prefab");
        }

        #endregion

        #region Tournament Lobby - Prize Row Item Prefab

        [MenuItem("DigitPark/Prefabs/Monetization/Prefabs/Create PrizeRowItem")]
        private static void CreatePrizeRowItemPrefab()
        {
            // Vertical layout per prize position (shown 3 side-by-side in HLG)
            // Layout: [Position] on top, [Amount] center, [Percentage] bottom
            GameObject item = new GameObject("PrizeRowItem");

            RectTransform rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, PRIZE_ROW_HEIGHT);

            LayoutElement le = item.AddComponent<LayoutElement>();
            le.preferredHeight = PRIZE_ROW_HEIGHT;
            le.flexibleWidth = 1;

            Image bg = item.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.12f, 0.15f, 0f); // Transparent by default

            // Medal Image (hidden by default, shown via Setup for top 3)
            GameObject medal = CreateImageElement(item.transform, "MedalImage",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-15, -15), new Vector2(15, 15));
            medal.GetComponent<Image>().color = GOLD;
            medal.SetActive(false);

            // Position Icon (hidden by default)
            GameObject posIcon = CreateImageElement(item.transform, "PositionIcon",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-12, -12), new Vector2(12, 12));
            posIcon.GetComponent<Image>().color = new Color(0.4f, 0.4f, 0.4f);
            posIcon.SetActive(false);

            // Position Text (top: "1st")
            CreateTextElement(item.transform, "PositionText", "1st",
                new Vector2(0, 0.6f), new Vector2(1, 1),
                (int)FontSizes.Body, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);

            // Prize Amount Text (center: "$40.00")
            CreateTextElement(item.transform, "PrizeAmountText", "$40.00",
                new Vector2(0, 0.2f), new Vector2(1, 0.6f),
                (int)FontSizes.BodyLarge, GOLD, FontStyles.Bold, TextAlignmentOptions.Center);

            // Percentage Text (bottom: hidden by default)
            GameObject pctObj = CreateTextElement(item.transform, "PercentageText", "(50%)",
                new Vector2(0, 0), new Vector2(1, 0.25f),
                (int)FontSizes.Caption, new Color(0.6f, 0.6f, 0.6f), FontStyles.Normal, TextAlignmentOptions.Center);
            pctObj.SetActive(false); // Hidden — only shown if Setup enables it

            // Add UI Component
            PrizeRowItemUI ui = item.AddComponent<PrizeRowItemUI>();

            SavePrefab(item, "Assets/_Project/Prefabs/Tournaments/Lobby/PrizeRowItem.prefab");
        }

        #endregion

        #region Tournament Lobby - Participant Item Prefab

        [MenuItem("DigitPark/Prefabs/Monetization/Prefabs/Create ParticipantItem")]
        private static void CreateParticipantItemPrefab()
        {
            // Layout: [Rank 8%] [Avatar 12%] [Username 30%] [Score 16%] [Time 16%] [Status 18%]
            GameObject item = new GameObject("ParticipantItem");

            RectTransform rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, PARTICIPANT_ITEM_HEIGHT);

            LayoutElement le = item.AddComponent<LayoutElement>();
            le.preferredHeight = PARTICIPANT_ITEM_HEIGHT;
            le.flexibleWidth = 1;

            Image bg = item.AddComponent<Image>();
            bg.color = CARD_BG;

            // === RANK NUMBER (far left, 0-8%) ===
            CreateTextElement(item.transform, "RankText", "#1",
                new Vector2(0, 0.1f), new Vector2(0.08f, 0.9f),
                (int)FontSizes.Body, GOLD, FontStyles.Bold, TextAlignmentOptions.Center);

            // === AVATAR (8-20%) ===
            Sprite pCircle = GenerateCircleSprite();
            GameObject avatarFrame = CreateImageElement(item.transform, "AvatarFrame",
                new Vector2(0.08f, 0.5f), new Vector2(0.08f, 0.5f),
                new Vector2(4, -22), new Vector2(48, 22));
            avatarFrame.GetComponent<Image>().sprite = pCircle;
            avatarFrame.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.35f);

            GameObject pMask = CreateImageElement(avatarFrame.transform, "AvatarMask",
                new Vector2(0.06f, 0.06f), new Vector2(0.94f, 0.94f),
                Vector2.zero, Vector2.zero);
            pMask.GetComponent<Image>().sprite = pCircle;
            pMask.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f);
            pMask.AddComponent<Mask>().showMaskGraphic = true;

            GameObject avatar = CreateImageElement(pMask.transform, "AvatarImage",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            avatar.GetComponent<Image>().color = Color.white;
            avatar.GetComponent<Image>().preserveAspect = true;
            Sprite pDefAvatar = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/Social/AvatarDefault.png");
            if (pDefAvatar != null) avatar.GetComponent<Image>().sprite = pDefAvatar;

            // Online Indicator (small green dot on avatar bottom-right)
            GameObject online = CreateImageElement(avatarFrame.transform, "OnlineIndicator",
                new Vector2(1, 0), new Vector2(1, 0),
                new Vector2(-12, 0), new Vector2(0, 12));
            online.GetComponent<Image>().sprite = pCircle;
            online.GetComponent<Image>().color = GREEN;

            // Avatar Border (subtle ring)
            GameObject avatarBorder = CreateImageElement(avatarFrame.transform, "AvatarBorder",
                Vector2.zero, Vector2.one, new Vector2(-1, -1), new Vector2(1, 1));
            avatarBorder.GetComponent<Image>().sprite = pCircle;
            avatarBorder.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.55f, 0.4f);

            // Country Flag (hidden by default, overlaps avatar corner)
            GameObject flag = CreateImageElement(item.transform, "CountryFlag",
                new Vector2(0.08f, 0.5f), new Vector2(0.08f, 0.5f),
                new Vector2(38, -22), new Vector2(54, -10));
            flag.GetComponent<Image>().color = Color.white;
            flag.SetActive(false);

            // === USERNAME (20-50%) ===
            CreateTextElement(item.transform, "UsernameText", "Username",
                new Vector2(0.20f, 0.1f), new Vector2(0.50f, 0.9f),
                (int)FontSizes.Body, Color.white, FontStyles.Bold, TextAlignmentOptions.Left);

            // === ATTEMPTS (50-64%) ===
            CreateTextElement(item.transform, "AttemptsText", "0/3",
                new Vector2(0.50f, 0.1f), new Vector2(0.64f, 0.9f),
                (int)FontSizes.Body, GOLD, FontStyles.Bold, TextAlignmentOptions.Center);

            // === BEST TIME (64-78%) ===
            CreateTextElement(item.transform, "BestTimeText", "2.34s",
                new Vector2(0.64f, 0.1f), new Vector2(0.78f, 0.9f),
                (int)FontSizes.Body, GREEN, FontStyles.Bold, TextAlignmentOptions.Center);

            // === STATUS (78-100%) ===
            CreateTextElement(item.transform, "StatusText", "Waiting",
                new Vector2(0.78f, 0.1f), new Vector2(0.98f, 0.9f),
                (int)FontSizes.Body, ORANGE, FontStyles.Bold, TextAlignmentOptions.Center);

            // Ready Indicator (hidden, shown via DOTween animation)
            GameObject ready = CreateImageElement(item.transform, "ReadyIndicator",
                new Vector2(0.78f, 0.5f), new Vector2(0.78f, 0.5f),
                new Vector2(-8, -8), new Vector2(8, 8));
            ready.GetComponent<Image>().sprite = pCircle;
            ready.GetComponent<Image>().color = GREEN;
            ready.SetActive(false);

            // Button (whole card clickable for profile)
            Button btn = item.AddComponent<Button>();
            btn.targetGraphic = bg;

            // Add UI Component
            ParticipantItemUI ui = item.AddComponent<ParticipantItemUI>();

            SavePrefab(item, "Assets/_Project/Prefabs/Tournaments/Lobby/ParticipantItem.prefab");
        }

        #endregion

        #region Social - Player Search Item Prefab

        [MenuItem("DigitPark/Prefabs/Monetization/Prefabs/Create PlayerSearchItem")]
        private static void CreatePlayerSearchItemPrefab()
        {
            GameObject item = new GameObject("PlayerSearchItem");

            RectTransform rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, PLAYER_SEARCH_HEIGHT);

            LayoutElement le = item.AddComponent<LayoutElement>();
            le.preferredHeight = PLAYER_SEARCH_HEIGHT;
            le.flexibleWidth = 1;

            Image bg = item.AddComponent<Image>();
            bg.color = CARD_BG;

            // Circular Avatar Frame
            Sprite sCircle = GenerateCircleSprite();
            GameObject avatarFrame3 = CreateImageElement(item.transform, "AvatarFrame",
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(10, -30), new Vector2(70, 30));
            avatarFrame3.GetComponent<Image>().sprite = sCircle;
            avatarFrame3.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.35f);

            GameObject sMask = CreateImageElement(avatarFrame3.transform, "AvatarMask",
                new Vector2(0.06f, 0.06f), new Vector2(0.94f, 0.94f),
                Vector2.zero, Vector2.zero);
            sMask.GetComponent<Image>().sprite = sCircle;
            sMask.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f);
            sMask.AddComponent<Mask>().showMaskGraphic = true;

            GameObject avatar = CreateImageElement(sMask.transform, "AvatarImage",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            avatar.GetComponent<Image>().color = Color.white;
            avatar.GetComponent<Image>().preserveAspect = true;
            Sprite sDefAvatar = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/Social/AvatarDefault.png");
            if (sDefAvatar != null) avatar.GetComponent<Image>().sprite = sDefAvatar;

            // Avatar Glow
            Outline outline = avatarFrame3.AddComponent<Outline>();
            outline.effectColor = new Color(0.3f, 0.3f, 0.35f);
            outline.effectDistance = new Vector2(2, 2);

            // Online Indicator
            GameObject online = CreateImageElement(item.transform, "OnlineIndicator",
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(58, 18), new Vector2(70, 30));
            online.GetComponent<Image>().color = GREEN;

            // Username
            CreateTextElement(item.transform, "UsernameText", "Username",
                new Vector2(0, 0.55f), new Vector2(0.5f, 0.95f),
                new Vector2(80, 0), new Vector2(0, 0),
                (int)FontSizes.Body, Color.white, FontStyles.Bold, TextAlignmentOptions.Left);

            // Level
            CreateTextElement(item.transform, "LevelText", "Level 25",
                new Vector2(0, 0.15f), new Vector2(0.3f, 0.55f),
                new Vector2(80, 0), new Vector2(0, 0),
                (int)FontSizes.Body, CYAN_NEON, FontStyles.Bold, TextAlignmentOptions.Left);

            // Stats Container
            GameObject stats = CreateContainer(item.transform, "StatsContainer",
                new Vector2(0.3f, 0.1f), new Vector2(0.6f, 0.55f), Vector2.zero, Vector2.zero);
            stats.SetActive(false);

            CreateTextElement(stats.transform, "WinsText", "150 wins",
                new Vector2(0, 0.5f), new Vector2(1, 1), (int)FontSizes.Body, Color.white, FontStyles.Bold, TextAlignmentOptions.Left);

            CreateTextElement(stats.transform, "WinRateText", "65%",
                new Vector2(0, 0), new Vector2(1, 0.5f), (int)FontSizes.Body, GREEN, FontStyles.Bold, TextAlignmentOptions.Left);

            // Country Flag
            GameObject flag = CreateImageElement(item.transform, "CountryFlag",
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(200, -10), new Vector2(230, 10));
            flag.GetComponent<Image>().color = Color.white;
            flag.SetActive(false);

            // Friend Badge
            GameObject friendBadge = CreateContainer(item.transform, "FriendBadge",
                new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(5, -18), new Vector2(55, -3));
            Image friendBg = friendBadge.AddComponent<Image>();
            friendBg.color = GREEN;
            CreateTextElement(friendBadge.transform, "Text", "FRIEND",
                Vector2.zero, Vector2.one, (int)FontSizes.Body, Color.black, FontStyles.Bold, TextAlignmentOptions.Center);
            friendBadge.SetActive(false);

            // Pending Badge
            GameObject pendingBadge = CreateContainer(item.transform, "PendingBadge",
                new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(5, -18), new Vector2(65, -3));
            Image pendingBg = pendingBadge.AddComponent<Image>();
            pendingBg.color = ORANGE;
            CreateTextElement(pendingBadge.transform, "Text", "PENDING",
                Vector2.zero, Vector2.one, (int)FontSizes.Body, Color.black, FontStyles.Bold, TextAlignmentOptions.Center);
            pendingBadge.SetActive(false);

            // Action Buttons
            GameObject addFriendBtn = CreateButton(item.transform, "AddFriendButton", "+", CYAN_NEON, Color.black,
                new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-110, -18), new Vector2(-75, 18));

            GameObject challengeBtn = CreateButton(item.transform, "ChallengeButton", "⚔", ORANGE, Color.black,
                new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-65, -18), new Vector2(-30, 18));

            GameObject viewProfileBtn = CreateButton(item.transform, "ViewProfileButton", "👤", new Color(0.3f, 0.3f, 0.35f), Color.white,
                new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-25, -18), new Vector2(-2, 18));

            // Add UI Component
            PlayerSearchItemUI ui = item.AddComponent<PlayerSearchItemUI>();

            SavePrefab(item, "Assets/_Project/Prefabs/Social/PlayerSearchItem.prefab");
        }

        #endregion

        #region Helper Methods

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
                tmp.fontSize = (int)FontSizes.Body;
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

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            Debug.Log($"[MonetizationPrefabBuilder] Prefab creado: {path}");
            Selection.activeObject = prefab;
        }

        private static Sprite GenerateCircleSprite()
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/UI/CircleSprite.png");
            if (s != null) return s;
            // Fallback: generate at runtime (won't survive prefab save)
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size / 2f;
            float radius = center - 1f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dist = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
                    if (dist <= radius) tex.SetPixel(x, y, Color.white);
                    else if (dist <= radius + 1f) tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(radius + 1f - dist)));
                    else tex.SetPixel(x, y, Color.clear);
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        #endregion
    }
}
