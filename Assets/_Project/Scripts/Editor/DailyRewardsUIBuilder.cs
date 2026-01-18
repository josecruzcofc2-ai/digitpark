using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    using DigitPark.Animations;
    /// <summary>
    /// Construye la UI completa de Daily Rewards (Recompensas Diarias)
    /// Incluye: SafeArea, Header, Streak, Calendario de 7 dias, Claim popup
    /// </summary>
    public class DailyRewardsUIBuilder : EditorWindow
    {
        // ==================== COLORES DEL TEMA NEON ====================
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.4f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 1f, 1f, 0.3f);

        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color PANEL_BG = new Color(0.08f, 0.12f, 0.18f, 0.98f);
        private static readonly Color CARD_BG = new Color(0.06f, 0.1f, 0.15f, 1f);
        private static readonly Color HEADER_BG = new Color(0.03f, 0.06f, 0.1f, 0.95f);
        private static readonly Color POPUP_BG = new Color(0.05f, 0.08f, 0.12f, 0.98f);

        private static readonly Color TEXT_PRIMARY = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.7f, 0.75f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.02f, 0.05f, 0.1f, 1f);

        private static readonly Color BUTTON_PRIMARY = CYAN_NEON;
        private static readonly Color BUTTON_SECONDARY = new Color(0.15f, 0.2f, 0.25f, 1f);
        private static readonly Color BUTTON_SUCCESS = new Color(0.2f, 0.8f, 0.4f, 1f);

        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GOLD_DARK = new Color(0.7f, 0.55f, 0f, 1f);
        private static readonly Color PURPLE_PREMIUM = new Color(0.6f, 0.3f, 0.9f, 1f);
        private static readonly Color ORANGE_STREAK = new Color(1f, 0.5f, 0.1f, 1f);

        private static readonly Color GEM_COLOR = new Color(0.4f, 0.8f, 1f, 1f);
        private static readonly Color COIN_COLOR = new Color(1f, 0.85f, 0.3f, 1f);

        private static readonly Color DAY_CLAIMED = new Color(0.15f, 0.3f, 0.15f, 1f);
        private static readonly Color DAY_TODAY = new Color(0.1f, 0.2f, 0.3f, 1f);
        private static readonly Color DAY_LOCKED = new Color(0.05f, 0.08f, 0.1f, 0.8f);

        private static readonly Color BLOCKER_BG = new Color(0f, 0f, 0f, 0.85f);

        // ==================== DIMENSIONES ====================
        private const float HEADER_HEIGHT = 110f;
        private const float STREAK_HEIGHT = 100f;
        private const float DAY_CARD_SIZE = 130f;
        private const float CONTENT_PADDING = 20f;

        [MenuItem("DigitPark/UI Builders/Monetization/DailyRewards", false, 183)]
        public static void BuildUI()
        {
            if (!EditorUtility.DisplayDialog("Daily Rewards UI Builder",
                "Esto construira la UI completa de Daily Rewards.\nAsegurate de tener la escena DailyRewards abierta.\n\nContinuar?",
                "Si", "No"))
                return;

            BuildCompleteUI();
        }

        private static void BuildCompleteUI()
        {
            Debug.Log("[DailyRewardsUIBuilder] ========== INICIANDO CONSTRUCCION ==========");

            Canvas canvas = SetupCanvas();
            if (canvas == null) return;

            CreateBackground(canvas);
            GameObject safeArea = CreateSafeArea(canvas);

            CreateHeader(safeArea);
            CreateStreakPanel(safeArea);
            CreateDaysGrid(safeArea);
            CreateClaimButton(safeArea);

            CreateRewardClaimPopup(canvas);

            // Add screen effects for reward animations
            GameObject screenEffects = CreateScreenEffects(canvas);

            // Add and configure RewardClaimAnimator
            AddRewardClaimAnimator(canvas, screenEffects);

            MarkSceneDirty();
            Debug.Log("[DailyRewardsUIBuilder] ========== CONSTRUCCION COMPLETADA ==========");
        }

        /// <summary>
        /// Creates screen effects for reward claim animations
        /// </summary>
        private static GameObject CreateScreenEffects(Canvas canvas)
        {
            GameObject effects = FindOrCreateChild(canvas.gameObject, "RewardEffects");
            effects.transform.SetAsLastSibling();

            RectTransform effectsRT = GetOrAddComponent<RectTransform>(effects);
            effectsRT.anchorMin = Vector2.zero;
            effectsRT.anchorMax = Vector2.one;
            effectsRT.sizeDelta = Vector2.zero;

            // Screen Flash
            GameObject flash = FindOrCreateChild(effects, "ScreenFlash");
            RectTransform flashRT = GetOrAddComponent<RectTransform>(flash);
            flashRT.anchorMin = Vector2.zero;
            flashRT.anchorMax = Vector2.one;
            flashRT.sizeDelta = Vector2.zero;
            Image flashImg = GetOrAddComponent<Image>(flash);
            flashImg.color = new Color(1, 1, 1, 0);
            flashImg.raycastTarget = false;
            flash.SetActive(false);

            // Flying icons parent
            GameObject flyingParent = FindOrCreateChild(effects, "FlyingIconsParent");
            RectTransform flyingRT = GetOrAddComponent<RectTransform>(flyingParent);
            flyingRT.anchorMin = Vector2.zero;
            flyingRT.anchorMax = Vector2.one;
            flyingRT.sizeDelta = Vector2.zero;

            Debug.Log("[DailyRewardsUIBuilder] RewardEffects creados");
            return effects;
        }

        /// <summary>
        /// Adds and configures RewardClaimAnimator for fly-to-target animations
        /// </summary>
        private static void AddRewardClaimAnimator(Canvas canvas, GameObject screenEffects)
        {
            RewardClaimAnimator animator = canvas.GetComponent<RewardClaimAnimator>();
            if (animator == null)
                animator = canvas.gameObject.AddComponent<RewardClaimAnimator>();

            // Get references
            Transform header = canvas.transform.Find("SafeArea/Header");
            Transform rewardPopup = canvas.transform.Find("RewardClaimPopup/Popup");
            Transform screenFlash = screenEffects.transform.Find("ScreenFlash");
            Transform flyingParent = screenEffects.transform.Find("FlyingIconsParent");

            // Find currency displays in header
            Transform coinsDisplay = header?.Find("CoinsDisplay");
            Transform gemsDisplay = header?.Find("GemsDisplay");

            // Configure via SerializedObject
            SerializedObject so = new SerializedObject(animator);

            // Currency targets (these will be the currency displays)
            if (coinsDisplay != null)
            {
                so.FindProperty("coinsTarget").objectReferenceValue = coinsDisplay.GetComponent<RectTransform>();
                Transform coinsText = coinsDisplay.Find("Value");
                if (coinsText != null)
                    so.FindProperty("coinsText").objectReferenceValue = coinsText.GetComponent<TextMeshProUGUI>();
            }

            if (gemsDisplay != null)
            {
                so.FindProperty("gemsTarget").objectReferenceValue = gemsDisplay.GetComponent<RectTransform>();
                Transform gemsText = gemsDisplay.Find("Value");
                if (gemsText != null)
                    so.FindProperty("gemsText").objectReferenceValue = gemsText.GetComponent<TextMeshProUGUI>();
            }

            // Screen effects
            if (screenFlash != null)
                so.FindProperty("screenFlash").objectReferenceValue = screenFlash.GetComponent<Image>();

            if (flyingParent != null)
                so.FindProperty("flyingIconsParent").objectReferenceValue = flyingParent;

            // Reward popup
            if (rewardPopup != null)
            {
                so.FindProperty("rewardPopup").objectReferenceValue = rewardPopup.GetComponent<RectTransform>();

                Transform rewardIcon = rewardPopup.Find("RewardIcon");
                Transform rewardName = rewardPopup.Find("RewardName");
                Transform rewardAmount = rewardPopup.Find("RewardAmount");

                if (rewardIcon != null)
                    so.FindProperty("rewardIcon").objectReferenceValue = rewardIcon.GetComponent<Image>();
                if (rewardName != null)
                    so.FindProperty("rewardNameText").objectReferenceValue = rewardName.GetComponent<TextMeshProUGUI>();
                if (rewardAmount != null)
                    so.FindProperty("rewardAmountText").objectReferenceValue = rewardAmount.GetComponent<TextMeshProUGUI>();
            }

            // Animation settings for satisfying fly-to-target
            so.FindProperty("maxFlyingIcons").intValue = 10;
            so.FindProperty("flyDuration").floatValue = 0.6f;
            so.FindProperty("iconSpawnDelay").floatValue = 0.05f;
            so.FindProperty("counterAnimDuration").floatValue = 0.5f;

            so.ApplyModifiedProperties();

            // Add AudioSource if needed
            if (canvas.GetComponent<AudioSource>() == null)
                canvas.gameObject.AddComponent<AudioSource>();

            Debug.Log("[DailyRewardsUIBuilder] RewardClaimAnimator configurado con animaciones fly-to-target");
        }

        // ==================== CANVAS SETUP ====================

        private static Canvas SetupCanvas()
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();

            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;

                canvasObj.AddComponent<GraphicRaycaster>();
            }

            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            if (Camera.main == null)
            {
                GameObject cameraObj = new GameObject("Main Camera");
                Camera cam = cameraObj.AddComponent<Camera>();
                cam.tag = "MainCamera";
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = DARK_BG;
            }

            return canvas;
        }

        private static void CreateBackground(Canvas canvas)
        {
            GameObject bg = FindOrCreateChild(canvas.gameObject, "Background");

            RectTransform bgRT = GetOrAddComponent<RectTransform>(bg);
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;

            Image bgImage = GetOrAddComponent<Image>(bg);
            bgImage.color = DARK_BG;

            bg.transform.SetAsFirstSibling();
        }

        private static GameObject CreateSafeArea(Canvas canvas)
        {
            GameObject safeArea = FindOrCreateChild(canvas.gameObject, "SafeArea");

            RectTransform safeRT = GetOrAddComponent<RectTransform>(safeArea);
            safeRT.anchorMin = Vector2.zero;
            safeRT.anchorMax = Vector2.one;
            safeRT.sizeDelta = Vector2.zero;

            safeArea.transform.SetSiblingIndex(1);
            return safeArea;
        }

        // ==================== HEADER ====================

        private static void CreateHeader(GameObject parent)
        {
            GameObject header = FindOrCreateChild(parent, "Header");

            RectTransform headerRT = GetOrAddComponent<RectTransform>(header);
            headerRT.anchorMin = new Vector2(0, 1);
            headerRT.anchorMax = new Vector2(1, 1);
            headerRT.pivot = new Vector2(0.5f, 1);
            headerRT.anchoredPosition = Vector2.zero;
            headerRT.sizeDelta = new Vector2(0, HEADER_HEIGHT);

            Image headerBg = GetOrAddComponent<Image>(header);
            headerBg.color = HEADER_BG;

            CreateBottomGlow(header);

            // BackButton
            GameObject backBtn = FindOrCreateChild(header, "BackButton");
            RectTransform backRT = GetOrAddComponent<RectTransform>(backBtn);
            backRT.anchorMin = new Vector2(0, 0.5f);
            backRT.anchorMax = new Vector2(0, 0.5f);
            backRT.pivot = new Vector2(0, 0.5f);
            backRT.anchoredPosition = new Vector2(20, 0);
            backRT.sizeDelta = new Vector2(50, 50);

            Image backBg = GetOrAddComponent<Image>(backBtn);
            backBg.color = BUTTON_SECONDARY;

            Button backButton = GetOrAddComponent<Button>(backBtn);
            SetupButtonColors(backButton, BUTTON_SECONDARY);
            AddOutline(backBtn, CYAN_DARK);

            GameObject backTextObj = FindOrCreateChild(backBtn, "Text");
            TextMeshProUGUI backText = GetOrAddComponent<TextMeshProUGUI>(backTextObj);
            backText.text = "<";
            backText.fontSize = 32;
            backText.fontStyle = FontStyles.Bold;
            backText.color = CYAN_NEON;
            backText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(backTextObj);

            // Title
            GameObject titleObj = FindOrCreateChild(header, "TitleText");
            RectTransform titleRT = GetOrAddComponent<RectTransform>(titleObj);
            titleRT.anchorMin = new Vector2(0.5f, 0.5f);
            titleRT.anchorMax = new Vector2(0.5f, 0.5f);
            titleRT.sizeDelta = new Vector2(400, 50);

            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "RECOMPENSAS DIARIAS";
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = CYAN_NEON;
            titleText.alignment = TextAlignmentOptions.Center;
            AddOutline(titleObj, CYAN_GLOW, 2);

            Debug.Log("[DailyRewardsUIBuilder] Header creado");
        }

        // ==================== STREAK PANEL ====================

        private static void CreateStreakPanel(GameObject parent)
        {
            GameObject streakPanel = FindOrCreateChild(parent, "StreakPanel");

            RectTransform streakRT = GetOrAddComponent<RectTransform>(streakPanel);
            streakRT.anchorMin = new Vector2(0, 1);
            streakRT.anchorMax = new Vector2(1, 1);
            streakRT.pivot = new Vector2(0.5f, 1);
            streakRT.anchoredPosition = new Vector2(0, -HEADER_HEIGHT - 15);
            streakRT.sizeDelta = new Vector2(-CONTENT_PADDING * 2, STREAK_HEIGHT);

            Image streakBg = GetOrAddComponent<Image>(streakPanel);
            streakBg.color = new Color(0.15f, 0.1f, 0.05f, 0.95f);
            AddOutline(streakPanel, ORANGE_STREAK, 2);

            // Glow effect
            Shadow shadow = GetOrAddComponent<Shadow>(streakPanel);
            shadow.effectColor = new Color(1f, 0.5f, 0.1f, 0.3f);
            shadow.effectDistance = new Vector2(0, -3);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(streakPanel);
            hlg.spacing = 20;
            hlg.padding = new RectOffset(25, 25, 15, 15);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            // Fire Icon
            GameObject fireIcon = FindOrCreateChild(streakPanel, "FireIcon");
            Image fireImage = GetOrAddComponent<Image>(fireIcon);
            fireImage.color = ORANGE_STREAK;
            LayoutElement fireLE = GetOrAddComponent<LayoutElement>(fireIcon);
            fireLE.minWidth = 55;
            fireLE.minHeight = 55;
            fireLE.preferredWidth = 55;
            fireLE.preferredHeight = 55;

            // Streak Info
            GameObject streakInfo = FindOrCreateChild(streakPanel, "StreakInfo");
            VerticalLayoutGroup infoVlg = GetOrAddComponent<VerticalLayoutGroup>(streakInfo);
            infoVlg.spacing = 5;
            infoVlg.childAlignment = TextAnchor.MiddleLeft;
            infoVlg.childControlWidth = true;
            infoVlg.childControlHeight = true;
            infoVlg.childForceExpandHeight = false;

            LayoutElement infoLE = GetOrAddComponent<LayoutElement>(streakInfo);
            infoLE.flexibleWidth = 1;

            // Streak Title
            GameObject streakTitle = FindOrCreateChild(streakInfo, "Title");
            TextMeshProUGUI streakTitleText = GetOrAddComponent<TextMeshProUGUI>(streakTitle);
            streakTitleText.text = "RACHA ACTUAL";
            streakTitleText.fontSize = 14;
            streakTitleText.color = TEXT_SECONDARY;
            streakTitleText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(streakTitle);
            titleLE.minHeight = 20;

            // Streak Count
            GameObject streakCount = FindOrCreateChild(streakInfo, "Count");
            HorizontalLayoutGroup countHlg = GetOrAddComponent<HorizontalLayoutGroup>(streakCount);
            countHlg.spacing = 8;
            countHlg.childAlignment = TextAnchor.MiddleLeft;
            countHlg.childControlWidth = false;
            countHlg.childControlHeight = true;
            LayoutElement countLE = GetOrAddComponent<LayoutElement>(streakCount);
            countLE.minHeight = 40;

            GameObject countNumber = FindOrCreateChild(streakCount, "Number");
            TextMeshProUGUI countNumberText = GetOrAddComponent<TextMeshProUGUI>(countNumber);
            countNumberText.text = "4";
            countNumberText.fontSize = 42;
            countNumberText.fontStyle = FontStyles.Bold;
            countNumberText.color = ORANGE_STREAK;
            countNumberText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement numberLE = GetOrAddComponent<LayoutElement>(countNumber);
            numberLE.minWidth = 45;

            GameObject countLabel = FindOrCreateChild(streakCount, "Label");
            TextMeshProUGUI countLabelText = GetOrAddComponent<TextMeshProUGUI>(countLabel);
            countLabelText.text = "dias seguidos";
            countLabelText.fontSize = 18;
            countLabelText.color = TEXT_PRIMARY;
            countLabelText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement labelLE = GetOrAddComponent<LayoutElement>(countLabel);
            labelLE.minWidth = 150;

            // Bonus Info
            GameObject bonusInfo = FindOrCreateChild(streakPanel, "BonusInfo");
            VerticalLayoutGroup bonusVlg = GetOrAddComponent<VerticalLayoutGroup>(bonusInfo);
            bonusVlg.spacing = 3;
            bonusVlg.childAlignment = TextAnchor.MiddleRight;
            bonusVlg.childControlWidth = true;
            bonusVlg.childControlHeight = true;
            bonusVlg.childForceExpandHeight = false;

            LayoutElement bonusLE = GetOrAddComponent<LayoutElement>(bonusInfo);
            bonusLE.minWidth = 140;

            GameObject bonusLabel = FindOrCreateChild(bonusInfo, "Label");
            TextMeshProUGUI bonusLabelText = GetOrAddComponent<TextMeshProUGUI>(bonusLabel);
            bonusLabelText.text = "Bonus 7 dias:";
            bonusLabelText.fontSize = 12;
            bonusLabelText.color = TEXT_SECONDARY;
            bonusLabelText.alignment = TextAlignmentOptions.MidlineRight;
            LayoutElement bonusLabelLE = GetOrAddComponent<LayoutElement>(bonusLabel);
            bonusLabelLE.minHeight = 18;

            GameObject bonusReward = FindOrCreateChild(bonusInfo, "Reward");
            HorizontalLayoutGroup rewardHlg = GetOrAddComponent<HorizontalLayoutGroup>(bonusReward);
            rewardHlg.spacing = 5;
            rewardHlg.childAlignment = TextAnchor.MiddleRight;
            rewardHlg.childControlWidth = false;
            rewardHlg.childControlHeight = true;
            LayoutElement rewardLE = GetOrAddComponent<LayoutElement>(bonusReward);
            rewardLE.minHeight = 30;

            GameObject rewardIcon = FindOrCreateChild(bonusReward, "Icon");
            Image rewardIconImage = GetOrAddComponent<Image>(rewardIcon);
            rewardIconImage.color = GEM_COLOR;
            LayoutElement rewardIconLE = GetOrAddComponent<LayoutElement>(rewardIcon);
            rewardIconLE.minWidth = 24;
            rewardIconLE.minHeight = 24;

            GameObject rewardAmount = FindOrCreateChild(bonusReward, "Amount");
            TextMeshProUGUI rewardAmountText = GetOrAddComponent<TextMeshProUGUI>(rewardAmount);
            rewardAmountText.text = "200";
            rewardAmountText.fontSize = 22;
            rewardAmountText.fontStyle = FontStyles.Bold;
            rewardAmountText.color = GEM_COLOR;
            rewardAmountText.alignment = TextAlignmentOptions.MidlineRight;
            LayoutElement rewardAmountLE = GetOrAddComponent<LayoutElement>(rewardAmount);
            rewardAmountLE.minWidth = 60;

            Debug.Log("[DailyRewardsUIBuilder] StreakPanel creado");
        }

        // ==================== DAYS GRID ====================

        private static void CreateDaysGrid(GameObject parent)
        {
            float topOffset = HEADER_HEIGHT + STREAK_HEIGHT + 45;

            GameObject daysContainer = FindOrCreateChild(parent, "DaysContainer");

            RectTransform daysRT = GetOrAddComponent<RectTransform>(daysContainer);
            daysRT.anchorMin = new Vector2(0, 1);
            daysRT.anchorMax = new Vector2(1, 1);
            daysRT.pivot = new Vector2(0.5f, 1);
            daysRT.anchoredPosition = new Vector2(0, -topOffset);
            daysRT.sizeDelta = new Vector2(-CONTENT_PADDING * 2, 0);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(daysContainer);
            vlg.spacing = 15;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            ContentSizeFitter csf = GetOrAddComponent<ContentSizeFitter>(daysContainer);
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Week Label
            GameObject weekLabel = FindOrCreateChild(daysContainer, "WeekLabel");
            TextMeshProUGUI weekText = GetOrAddComponent<TextMeshProUGUI>(weekLabel);
            weekText.text = "Semana 1";
            weekText.fontSize = 20;
            weekText.fontStyle = FontStyles.Bold;
            weekText.color = TEXT_PRIMARY;
            weekText.alignment = TextAlignmentOptions.Center;
            LayoutElement weekLE = GetOrAddComponent<LayoutElement>(weekLabel);
            weekLE.minHeight = 30;

            // Days Row 1 (Days 1-4)
            GameObject row1 = FindOrCreateChild(daysContainer, "DaysRow1");
            HorizontalLayoutGroup row1Hlg = GetOrAddComponent<HorizontalLayoutGroup>(row1);
            row1Hlg.spacing = 12;
            row1Hlg.childAlignment = TextAnchor.MiddleCenter;
            row1Hlg.childControlWidth = false;
            row1Hlg.childControlHeight = true;
            row1Hlg.childForceExpandWidth = false;

            LayoutElement row1LE = GetOrAddComponent<LayoutElement>(row1);
            row1LE.minHeight = DAY_CARD_SIZE + 20;

            CreateDayCard(row1, "Day1", 1, COIN_COLOR, "100", true, false); // Claimed
            CreateDayCard(row1, "Day2", 2, COIN_COLOR, "150", true, false); // Claimed
            CreateDayCard(row1, "Day3", 3, GEM_COLOR, "25", true, false);   // Claimed
            CreateDayCard(row1, "Day4", 4, COIN_COLOR, "200", true, false); // Claimed

            // Days Row 2 (Days 5-7)
            GameObject row2 = FindOrCreateChild(daysContainer, "DaysRow2");
            HorizontalLayoutGroup row2Hlg = GetOrAddComponent<HorizontalLayoutGroup>(row2);
            row2Hlg.spacing = 12;
            row2Hlg.childAlignment = TextAnchor.MiddleCenter;
            row2Hlg.childControlWidth = false;
            row2Hlg.childControlHeight = true;
            row2Hlg.childForceExpandWidth = false;

            LayoutElement row2LE = GetOrAddComponent<LayoutElement>(row2);
            row2LE.minHeight = DAY_CARD_SIZE + 20;

            CreateDayCard(row2, "Day5", 5, GEM_COLOR, "50", false, true);   // Today (claimable)
            CreateDayCard(row2, "Day6", 6, COIN_COLOR, "300", false, false); // Locked
            CreateDayCard(row2, "Day7", 7, GOLD, "BONUS", false, false);    // Locked (special)

            Debug.Log("[DailyRewardsUIBuilder] DaysGrid creado");
        }

        private static void CreateDayCard(GameObject parent, string name, int dayNumber, Color rewardColor, string reward, bool isClaimed, bool isToday)
        {
            GameObject card = FindOrCreateChild(parent, name);

            Image cardBg = GetOrAddComponent<Image>(card);
            if (isClaimed)
            {
                cardBg.color = DAY_CLAIMED;
            }
            else if (isToday)
            {
                cardBg.color = DAY_TODAY;
            }
            else
            {
                cardBg.color = DAY_LOCKED;
            }

            Color outlineColor = isToday ? CYAN_NEON : (isClaimed ? BUTTON_SUCCESS : CYAN_DARK * 0.5f);
            AddOutline(card, outlineColor, isToday ? 3 : 1);

            LayoutElement cardLE = GetOrAddComponent<LayoutElement>(card);
            cardLE.minWidth = DAY_CARD_SIZE;
            cardLE.minHeight = DAY_CARD_SIZE;
            cardLE.preferredWidth = DAY_CARD_SIZE;
            cardLE.preferredHeight = DAY_CARD_SIZE;

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(card);
            vlg.spacing = 6;
            vlg.padding = new RectOffset(8, 8, 10, 10);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Day Label
            GameObject dayLabel = FindOrCreateChild(card, "DayLabel");
            Image dayLabelBg = GetOrAddComponent<Image>(dayLabel);
            dayLabelBg.color = isToday ? CYAN_NEON : (isClaimed ? BUTTON_SUCCESS : BUTTON_SECONDARY);
            LayoutElement dayLabelLE = GetOrAddComponent<LayoutElement>(dayLabel);
            dayLabelLE.minHeight = 24;
            dayLabelLE.preferredHeight = 24;

            GameObject dayTextObj = FindOrCreateChild(dayLabel, "Text");
            TextMeshProUGUI dayText = GetOrAddComponent<TextMeshProUGUI>(dayTextObj);
            dayText.text = isToday ? "HOY" : $"DIA {dayNumber}";
            dayText.fontSize = 12;
            dayText.fontStyle = FontStyles.Bold;
            dayText.color = isToday || isClaimed ? TEXT_DARK : TEXT_PRIMARY;
            dayText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(dayTextObj);

            // Reward Icon
            GameObject rewardIcon = FindOrCreateChild(card, "RewardIcon");
            Image rewardIconImage = GetOrAddComponent<Image>(rewardIcon);
            rewardIconImage.color = isClaimed ? rewardColor * 0.5f : rewardColor;
            LayoutElement rewardIconLE = GetOrAddComponent<LayoutElement>(rewardIcon);
            rewardIconLE.minHeight = 40;
            rewardIconLE.preferredHeight = 40;
            rewardIconLE.minWidth = 40;
            rewardIconLE.preferredWidth = 40;

            // Reward Amount
            GameObject rewardAmount = FindOrCreateChild(card, "RewardAmount");
            TextMeshProUGUI rewardAmountText = GetOrAddComponent<TextMeshProUGUI>(rewardAmount);
            rewardAmountText.text = reward;
            rewardAmountText.fontSize = dayNumber == 7 ? 14 : 18;
            rewardAmountText.fontStyle = FontStyles.Bold;
            rewardAmountText.color = isClaimed ? TEXT_SECONDARY : rewardColor;
            rewardAmountText.alignment = TextAlignmentOptions.Center;
            LayoutElement rewardAmountLE = GetOrAddComponent<LayoutElement>(rewardAmount);
            rewardAmountLE.minHeight = 24;

            // Status indicator (checkmark or glow for today)
            if (isClaimed)
            {
                GameObject checkmark = FindOrCreateChild(card, "Checkmark");
                RectTransform checkRT = GetOrAddComponent<RectTransform>(checkmark);
                checkRT.anchorMin = new Vector2(1, 1);
                checkRT.anchorMax = new Vector2(1, 1);
                checkRT.pivot = new Vector2(1, 1);
                checkRT.anchoredPosition = new Vector2(-5, -5);
                checkRT.sizeDelta = new Vector2(28, 28);

                Image checkImage = GetOrAddComponent<Image>(checkmark);
                checkImage.color = BUTTON_SUCCESS;

                GameObject checkText = FindOrCreateChild(checkmark, "Text");
                TextMeshProUGUI checkTmp = GetOrAddComponent<TextMeshProUGUI>(checkText);
                checkTmp.text = "✓";
                checkTmp.fontSize = 18;
                checkTmp.fontStyle = FontStyles.Bold;
                checkTmp.color = TEXT_DARK;
                checkTmp.alignment = TextAlignmentOptions.Center;
                SetRectTransformStretch(checkText);
            }
            else if (!isToday)
            {
                // Lock overlay for future days
                GameObject lockOverlay = FindOrCreateChild(card, "LockOverlay");
                SetRectTransformStretch(lockOverlay);
                Image lockImage = GetOrAddComponent<Image>(lockOverlay);
                lockImage.color = new Color(0, 0, 0, 0.4f);

                GameObject lockIcon = FindOrCreateChild(lockOverlay, "LockIcon");
                RectTransform lockIconRT = GetOrAddComponent<RectTransform>(lockIcon);
                lockIconRT.anchorMin = new Vector2(0.5f, 0.5f);
                lockIconRT.anchorMax = new Vector2(0.5f, 0.5f);
                lockIconRT.sizeDelta = new Vector2(35, 35);

                Image lockIconImage = GetOrAddComponent<Image>(lockIcon);
                lockIconImage.color = TEXT_SECONDARY;

                GameObject lockText = FindOrCreateChild(lockIcon, "Text");
                TextMeshProUGUI lockTmp = GetOrAddComponent<TextMeshProUGUI>(lockText);
                lockTmp.text = "🔒";
                lockTmp.fontSize = 24;
                lockTmp.alignment = TextAlignmentOptions.Center;
                SetRectTransformStretch(lockText);
            }

            // Add pulse effect indicator for today
            if (isToday)
            {
                GameObject pulseIndicator = FindOrCreateChild(card, "PulseIndicator");
                RectTransform pulseRT = GetOrAddComponent<RectTransform>(pulseIndicator);
                pulseRT.anchorMin = Vector2.zero;
                pulseRT.anchorMax = Vector2.one;
                pulseRT.sizeDelta = new Vector2(6, 6);

                Outline pulseOutline = GetOrAddComponent<Outline>(pulseIndicator);
                pulseOutline.effectColor = CYAN_NEON;
                pulseOutline.effectDistance = new Vector2(3, 3);
            }
        }

        // ==================== CLAIM BUTTON ====================

        private static void CreateClaimButton(GameObject parent)
        {
            GameObject claimArea = FindOrCreateChild(parent, "ClaimArea");

            RectTransform claimRT = GetOrAddComponent<RectTransform>(claimArea);
            claimRT.anchorMin = new Vector2(0, 0);
            claimRT.anchorMax = new Vector2(1, 0);
            claimRT.pivot = new Vector2(0.5f, 0);
            claimRT.anchoredPosition = new Vector2(0, 80);
            claimRT.sizeDelta = new Vector2(-CONTENT_PADDING * 2, 120);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(claimArea);
            vlg.spacing = 15;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Today's Reward Preview
            GameObject previewPanel = FindOrCreateChild(claimArea, "PreviewPanel");
            HorizontalLayoutGroup previewHlg = GetOrAddComponent<HorizontalLayoutGroup>(previewPanel);
            previewHlg.spacing = 10;
            previewHlg.childAlignment = TextAnchor.MiddleCenter;
            previewHlg.childControlWidth = false;
            previewHlg.childControlHeight = true;

            LayoutElement previewLE = GetOrAddComponent<LayoutElement>(previewPanel);
            previewLE.minHeight = 35;

            GameObject previewLabel = FindOrCreateChild(previewPanel, "Label");
            TextMeshProUGUI previewLabelText = GetOrAddComponent<TextMeshProUGUI>(previewLabel);
            previewLabelText.text = "Recompensa de hoy:";
            previewLabelText.fontSize = 16;
            previewLabelText.color = TEXT_SECONDARY;
            previewLabelText.alignment = TextAlignmentOptions.MidlineRight;
            LayoutElement previewLabelLE = GetOrAddComponent<LayoutElement>(previewLabel);
            previewLabelLE.minWidth = 180;

            GameObject previewIcon = FindOrCreateChild(previewPanel, "Icon");
            Image previewIconImage = GetOrAddComponent<Image>(previewIcon);
            previewIconImage.color = GEM_COLOR;
            LayoutElement previewIconLE = GetOrAddComponent<LayoutElement>(previewIcon);
            previewIconLE.minWidth = 28;
            previewIconLE.minHeight = 28;

            GameObject previewAmount = FindOrCreateChild(previewPanel, "Amount");
            TextMeshProUGUI previewAmountText = GetOrAddComponent<TextMeshProUGUI>(previewAmount);
            previewAmountText.text = "50";
            previewAmountText.fontSize = 22;
            previewAmountText.fontStyle = FontStyles.Bold;
            previewAmountText.color = GEM_COLOR;
            previewAmountText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement previewAmountLE = GetOrAddComponent<LayoutElement>(previewAmount);
            previewAmountLE.minWidth = 80;

            // Claim Button
            GameObject claimBtn = FindOrCreateChild(claimArea, "ClaimButton");
            Image claimBg = GetOrAddComponent<Image>(claimBtn);
            claimBg.color = BUTTON_SUCCESS;
            Button claimButton = GetOrAddComponent<Button>(claimBtn);
            SetupButtonColors(claimButton, BUTTON_SUCCESS);
            AddOutline(claimBtn, new Color(0.3f, 1f, 0.5f, 0.5f), 3);

            LayoutElement claimLE = GetOrAddComponent<LayoutElement>(claimBtn);
            claimLE.minHeight = 65;
            claimLE.preferredHeight = 65;

            // Glow effect
            Shadow claimShadow = GetOrAddComponent<Shadow>(claimBtn);
            claimShadow.effectColor = new Color(0.2f, 0.8f, 0.4f, 0.4f);
            claimShadow.effectDistance = new Vector2(0, -4);

            GameObject claimTextObj = FindOrCreateChild(claimBtn, "Text");
            TextMeshProUGUI claimText = GetOrAddComponent<TextMeshProUGUI>(claimTextObj);
            claimText.text = "RECLAMAR RECOMPENSA";
            claimText.fontSize = 22;
            claimText.fontStyle = FontStyles.Bold;
            claimText.color = TEXT_DARK;
            claimText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(claimTextObj);

            Debug.Log("[DailyRewardsUIBuilder] ClaimButton creado");
        }

        // ==================== REWARD CLAIM POPUP ====================

        private static void CreateRewardClaimPopup(Canvas canvas)
        {
            GameObject blocker = FindOrCreateChild(canvas.gameObject, "RewardClaimBlocker");
            blocker.SetActive(false);

            SetRectTransformStretch(blocker);
            Image blockerBg = GetOrAddComponent<Image>(blocker);
            blockerBg.color = BLOCKER_BG;
            Button blockerBtn = GetOrAddComponent<Button>(blocker);
            blockerBtn.transition = Selectable.Transition.None;
            blocker.transform.SetAsLastSibling();

            GameObject popup = FindOrCreateChild(blocker, "RewardPopup");
            RectTransform popupRT = GetOrAddComponent<RectTransform>(popup);
            popupRT.anchorMin = new Vector2(0.5f, 0.5f);
            popupRT.anchorMax = new Vector2(0.5f, 0.5f);
            popupRT.sizeDelta = new Vector2(450, 420);

            Image popupBg = GetOrAddComponent<Image>(popup);
            popupBg.color = POPUP_BG;
            AddOutline(popup, GOLD, 3);

            // Glow effect
            Shadow popupShadow = GetOrAddComponent<Shadow>(popup);
            popupShadow.effectColor = new Color(1f, 0.84f, 0f, 0.3f);
            popupShadow.effectDistance = new Vector2(0, -5);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(popup);
            vlg.spacing = 20;
            vlg.padding = new RectOffset(30, 30, 35, 30);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Celebration Title
            GameObject titleObj = FindOrCreateChild(popup, "Title");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "Recompensa Obtenida!";
            titleText.fontSize = 30;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GOLD;
            titleText.alignment = TextAlignmentOptions.Center;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 45;

            // Day info
            GameObject dayInfoObj = FindOrCreateChild(popup, "DayInfo");
            TextMeshProUGUI dayInfoText = GetOrAddComponent<TextMeshProUGUI>(dayInfoObj);
            dayInfoText.text = "Dia 5 de 7";
            dayInfoText.fontSize = 16;
            dayInfoText.color = TEXT_SECONDARY;
            dayInfoText.alignment = TextAlignmentOptions.Center;
            LayoutElement dayInfoLE = GetOrAddComponent<LayoutElement>(dayInfoObj);
            dayInfoLE.minHeight = 25;

            // Reward Display (large, animated)
            GameObject rewardDisplay = FindOrCreateChild(popup, "RewardDisplay");
            Image rewardDisplayBg = GetOrAddComponent<Image>(rewardDisplay);
            rewardDisplayBg.color = new Color(0.1f, 0.15f, 0.2f, 0.8f);
            AddOutline(rewardDisplay, GEM_COLOR, 2);

            LayoutElement rewardDisplayLE = GetOrAddComponent<LayoutElement>(rewardDisplay);
            rewardDisplayLE.minHeight = 120;
            rewardDisplayLE.preferredHeight = 120;

            VerticalLayoutGroup rewardVlg = GetOrAddComponent<VerticalLayoutGroup>(rewardDisplay);
            rewardVlg.spacing = 10;
            rewardVlg.padding = new RectOffset(20, 20, 15, 15);
            rewardVlg.childAlignment = TextAnchor.MiddleCenter;
            rewardVlg.childControlWidth = true;
            rewardVlg.childControlHeight = true;
            rewardVlg.childForceExpandHeight = false;

            // Reward Icon
            GameObject rewardIconObj = FindOrCreateChild(rewardDisplay, "Icon");
            Image rewardIconImage = GetOrAddComponent<Image>(rewardIconObj);
            rewardIconImage.color = GEM_COLOR;
            LayoutElement rewardIconLE = GetOrAddComponent<LayoutElement>(rewardIconObj);
            rewardIconLE.minWidth = 55;
            rewardIconLE.minHeight = 55;
            rewardIconLE.preferredWidth = 55;
            rewardIconLE.preferredHeight = 55;

            // Reward Amount
            GameObject rewardAmountObj = FindOrCreateChild(rewardDisplay, "Amount");
            TextMeshProUGUI rewardAmountText = GetOrAddComponent<TextMeshProUGUI>(rewardAmountObj);
            rewardAmountText.text = "+50";
            rewardAmountText.fontSize = 42;
            rewardAmountText.fontStyle = FontStyles.Bold;
            rewardAmountText.color = GEM_COLOR;
            rewardAmountText.alignment = TextAlignmentOptions.Center;
            LayoutElement rewardAmountLE = GetOrAddComponent<LayoutElement>(rewardAmountObj);
            rewardAmountLE.minHeight = 50;

            // Streak Bonus Info
            GameObject streakBonus = FindOrCreateChild(popup, "StreakBonus");
            TextMeshProUGUI streakBonusText = GetOrAddComponent<TextMeshProUGUI>(streakBonus);
            streakBonusText.text = "Solo 2 dias mas para el bonus especial!";
            streakBonusText.fontSize = 14;
            streakBonusText.color = ORANGE_STREAK;
            streakBonusText.alignment = TextAlignmentOptions.Center;
            LayoutElement streakBonusLE = GetOrAddComponent<LayoutElement>(streakBonus);
            streakBonusLE.minHeight = 22;

            // Continue Button
            GameObject continueBtn = FindOrCreateChild(popup, "ContinueButton");
            Image continueBg = GetOrAddComponent<Image>(continueBtn);
            continueBg.color = BUTTON_SUCCESS;
            Button continueButton = GetOrAddComponent<Button>(continueBtn);
            SetupButtonColors(continueButton, BUTTON_SUCCESS);
            AddOutline(continueBtn, new Color(0.3f, 1f, 0.5f, 0.5f), 2);
            LayoutElement continueLE = GetOrAddComponent<LayoutElement>(continueBtn);
            continueLE.minHeight = 55;
            continueLE.preferredHeight = 55;

            GameObject continueTextObj = FindOrCreateChild(continueBtn, "Text");
            TextMeshProUGUI continueText = GetOrAddComponent<TextMeshProUGUI>(continueTextObj);
            continueText.text = "Continuar";
            continueText.fontSize = 22;
            continueText.fontStyle = FontStyles.Bold;
            continueText.color = TEXT_DARK;
            continueText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(continueTextObj);

            Debug.Log("[DailyRewardsUIBuilder] RewardClaimPopup creado");
        }

        // ==================== UTILITY METHODS ====================

        private static void CreateBottomGlow(GameObject obj)
        {
            GameObject glow = FindOrCreateChild(obj, "BottomGlow");
            RectTransform glowRT = GetOrAddComponent<RectTransform>(glow);
            glowRT.anchorMin = new Vector2(0, 0);
            glowRT.anchorMax = new Vector2(1, 0);
            glowRT.pivot = new Vector2(0.5f, 1);
            glowRT.anchoredPosition = Vector2.zero;
            glowRT.sizeDelta = new Vector2(0, 3);

            Image glowImage = GetOrAddComponent<Image>(glow);
            glowImage.color = CYAN_NEON;
        }

        private static void SetRectTransformStretch(GameObject obj)
        {
            RectTransform rt = GetOrAddComponent<RectTransform>(obj);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }

        private static T GetOrAddComponent<T>(GameObject obj) where T : Component
        {
            T component = obj.GetComponent<T>();
            if (component == null)
                component = obj.AddComponent<T>();
            return component;
        }

        private static GameObject FindOrCreateChild(GameObject parent, string childName)
        {
            Transform child = parent.transform.Find(childName);
            if (child != null) return child.gameObject;

            GameObject newChild = new GameObject(childName);
            newChild.transform.SetParent(parent.transform, false);

            if (newChild.GetComponent<RectTransform>() == null)
                newChild.AddComponent<RectTransform>();

            return newChild;
        }

        private static void SetupButtonColors(Button btn, Color baseColor)
        {
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            btn.colors = colors;
        }

        private static void AddOutline(GameObject obj, Color color, float distance = 1)
        {
            Outline outline = obj.GetComponent<Outline>();
            if (outline == null)
                outline = obj.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(distance, distance);
        }

        private static void MarkSceneDirty()
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
    }
}
