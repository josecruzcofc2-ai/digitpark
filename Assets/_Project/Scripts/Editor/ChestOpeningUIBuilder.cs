using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Construye la UI completa de Chest Opening (Apertura de Cofres)
    /// Incluye: SafeArea, Chest Display, Tap Area, Reward Reveals, Collect Button
    /// </summary>
    public class ChestOpeningUIBuilder : EditorWindow
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
        private static readonly Color PURPLE_PREMIUM = new Color(0.6f, 0.3f, 0.9f, 1f);
        private static readonly Color PURPLE_EPIC = new Color(0.5f, 0.2f, 0.8f, 1f);
        private static readonly Color ORANGE_LEGENDARY = new Color(1f, 0.5f, 0.1f, 1f);

        private static readonly Color GEM_COLOR = new Color(0.4f, 0.8f, 1f, 1f);
        private static readonly Color COIN_COLOR = new Color(1f, 0.85f, 0.3f, 1f);

        // Chest Rarity Colors
        private static readonly Color COMMON_CHEST = new Color(0.5f, 0.5f, 0.5f, 1f);
        private static readonly Color RARE_CHEST = new Color(0.3f, 0.6f, 1f, 1f);
        private static readonly Color EPIC_CHEST = PURPLE_EPIC;
        private static readonly Color LEGENDARY_CHEST = ORANGE_LEGENDARY;

        private static readonly Color BLOCKER_BG = new Color(0f, 0f, 0f, 0.85f);

        // ==================== DIMENSIONES ====================
        private const float HEADER_HEIGHT = 90f;
        private const float CHEST_AREA_HEIGHT = 450f;
        private const float REWARD_CARD_SIZE = 140f;
        private const float CONTENT_PADDING = 20f;

        [MenuItem("DigitPark/Monetization/Build Chest Opening UI", false, 24)]
        public static void BuildUI()
        {
            if (!EditorUtility.DisplayDialog("Chest Opening UI Builder",
                "Esto construira la UI completa de Chest Opening.\nAsegurate de tener la escena ChestOpening abierta.\n\nContinuar?",
                "Si", "No"))
                return;

            BuildCompleteUI();
        }

        private static void BuildCompleteUI()
        {
            Debug.Log("[ChestOpeningUIBuilder] ========== INICIANDO CONSTRUCCION ==========");

            Canvas canvas = SetupCanvas();
            if (canvas == null) return;

            CreateBackground(canvas);
            GameObject safeArea = CreateSafeArea(canvas);

            CreateHeader(safeArea);
            CreateChestDisplay(safeArea);
            CreateTapToOpenArea(safeArea);
            CreateRewardsRevealArea(safeArea);
            CreateCollectButton(safeArea);

            CreateChestInfoPanel(canvas);

            MarkSceneDirty();
            Debug.Log("[ChestOpeningUIBuilder] ========== CONSTRUCCION COMPLETADA ==========");
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

            // Radial glow effect for chest opening
            GameObject glowEffect = FindOrCreateChild(bg, "RadialGlow");
            RectTransform glowRT = GetOrAddComponent<RectTransform>(glowEffect);
            glowRT.anchorMin = new Vector2(0.5f, 0.6f);
            glowRT.anchorMax = new Vector2(0.5f, 0.6f);
            glowRT.sizeDelta = new Vector2(800, 800);

            Image glowImage = GetOrAddComponent<Image>(glowEffect);
            glowImage.color = new Color(0.6f, 0.3f, 0.9f, 0.15f);
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
            headerBg.color = new Color(0, 0, 0, 0.5f);

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

            // Chest Type Title
            GameObject titleObj = FindOrCreateChild(header, "TitleText");
            RectTransform titleRT = GetOrAddComponent<RectTransform>(titleObj);
            titleRT.anchorMin = new Vector2(0.5f, 0.5f);
            titleRT.anchorMax = new Vector2(0.5f, 0.5f);
            titleRT.sizeDelta = new Vector2(400, 50);

            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "COFRE EPICO";
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = EPIC_CHEST;
            titleText.alignment = TextAlignmentOptions.Center;
            AddOutline(titleObj, new Color(0.5f, 0.2f, 0.8f, 0.4f), 2);

            // Info Button
            GameObject infoBtn = FindOrCreateChild(header, "InfoButton");
            RectTransform infoRT = GetOrAddComponent<RectTransform>(infoBtn);
            infoRT.anchorMin = new Vector2(1, 0.5f);
            infoRT.anchorMax = new Vector2(1, 0.5f);
            infoRT.pivot = new Vector2(1, 0.5f);
            infoRT.anchoredPosition = new Vector2(-20, 0);
            infoRT.sizeDelta = new Vector2(45, 45);

            Image infoBg = GetOrAddComponent<Image>(infoBtn);
            infoBg.color = BUTTON_SECONDARY;

            Button infoButton = GetOrAddComponent<Button>(infoBtn);
            SetupButtonColors(infoButton, BUTTON_SECONDARY);
            AddOutline(infoBtn, CYAN_DARK);

            GameObject infoTextObj = FindOrCreateChild(infoBtn, "Text");
            TextMeshProUGUI infoText = GetOrAddComponent<TextMeshProUGUI>(infoTextObj);
            infoText.text = "i";
            infoText.fontSize = 24;
            infoText.fontStyle = FontStyles.Bold;
            infoText.color = CYAN_NEON;
            infoText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(infoTextObj);

            Debug.Log("[ChestOpeningUIBuilder] Header creado");
        }

        // ==================== CHEST DISPLAY ====================

        private static void CreateChestDisplay(GameObject parent)
        {
            GameObject chestArea = FindOrCreateChild(parent, "ChestDisplay");

            RectTransform chestRT = GetOrAddComponent<RectTransform>(chestArea);
            chestRT.anchorMin = new Vector2(0.5f, 0.5f);
            chestRT.anchorMax = new Vector2(0.5f, 0.5f);
            chestRT.anchoredPosition = new Vector2(0, 100);
            chestRT.sizeDelta = new Vector2(400, CHEST_AREA_HEIGHT);

            // Chest Image Container
            GameObject chestContainer = FindOrCreateChild(chestArea, "ChestContainer");
            RectTransform containerRT = GetOrAddComponent<RectTransform>(chestContainer);
            containerRT.anchorMin = new Vector2(0.5f, 0.5f);
            containerRT.anchorMax = new Vector2(0.5f, 0.5f);
            containerRT.sizeDelta = new Vector2(280, 280);

            // Chest Glow
            GameObject chestGlow = FindOrCreateChild(chestContainer, "ChestGlow");
            RectTransform glowRT = GetOrAddComponent<RectTransform>(chestGlow);
            glowRT.anchorMin = new Vector2(0.5f, 0.5f);
            glowRT.anchorMax = new Vector2(0.5f, 0.5f);
            glowRT.sizeDelta = new Vector2(350, 350);

            Image glowImage = GetOrAddComponent<Image>(chestGlow);
            glowImage.color = new Color(0.5f, 0.2f, 0.8f, 0.3f);

            // Chest Image Placeholder
            GameObject chestImage = FindOrCreateChild(chestContainer, "ChestImage");
            RectTransform chestImageRT = GetOrAddComponent<RectTransform>(chestImage);
            chestImageRT.anchorMin = Vector2.zero;
            chestImageRT.anchorMax = Vector2.one;
            chestImageRT.sizeDelta = Vector2.zero;

            Image chestImg = GetOrAddComponent<Image>(chestImage);
            chestImg.color = EPIC_CHEST;

            // Chest Lid (for animation)
            GameObject chestLid = FindOrCreateChild(chestContainer, "ChestLid");
            RectTransform lidRT = GetOrAddComponent<RectTransform>(chestLid);
            lidRT.anchorMin = new Vector2(0, 0.6f);
            lidRT.anchorMax = new Vector2(1, 1);
            lidRT.sizeDelta = Vector2.zero;

            Image lidImage = GetOrAddComponent<Image>(chestLid);
            lidImage.color = EPIC_CHEST * 0.8f;

            // Sparkle Effects Placeholder
            GameObject sparkles = FindOrCreateChild(chestContainer, "Sparkles");
            SetRectTransformStretch(sparkles);

            for (int i = 0; i < 5; i++)
            {
                GameObject sparkle = FindOrCreateChild(sparkles, $"Sparkle_{i}");
                RectTransform sparkleRT = GetOrAddComponent<RectTransform>(sparkle);
                sparkleRT.anchorMin = new Vector2(0.5f, 0.5f);
                sparkleRT.anchorMax = new Vector2(0.5f, 0.5f);
                sparkleRT.sizeDelta = new Vector2(20, 20);
                sparkleRT.anchoredPosition = new Vector2(
                    Mathf.Cos(i * 72 * Mathf.Deg2Rad) * 120,
                    Mathf.Sin(i * 72 * Mathf.Deg2Rad) * 120
                );

                Image sparkleImage = GetOrAddComponent<Image>(sparkle);
                sparkleImage.color = GOLD;
            }

            Debug.Log("[ChestOpeningUIBuilder] ChestDisplay creado");
        }

        // ==================== TAP TO OPEN AREA ====================

        private static void CreateTapToOpenArea(GameObject parent)
        {
            GameObject tapArea = FindOrCreateChild(parent, "TapToOpenArea");

            RectTransform tapRT = GetOrAddComponent<RectTransform>(tapArea);
            tapRT.anchorMin = new Vector2(0.5f, 0);
            tapRT.anchorMax = new Vector2(0.5f, 0);
            tapRT.pivot = new Vector2(0.5f, 0);
            tapRT.anchoredPosition = new Vector2(0, 180);
            tapRT.sizeDelta = new Vector2(500, 100);

            Button tapButton = GetOrAddComponent<Button>(tapArea);
            tapButton.transition = Selectable.Transition.None;

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(tapArea);
            vlg.spacing = 10;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Tap Instruction
            GameObject tapText = FindOrCreateChild(tapArea, "TapText");
            TextMeshProUGUI tapTmp = GetOrAddComponent<TextMeshProUGUI>(tapText);
            tapTmp.text = "TOCA PARA ABRIR";
            tapTmp.fontSize = 28;
            tapTmp.fontStyle = FontStyles.Bold;
            tapTmp.color = GOLD;
            tapTmp.alignment = TextAlignmentOptions.Center;
            LayoutElement tapLE = GetOrAddComponent<LayoutElement>(tapText);
            tapLE.minHeight = 40;
            AddOutline(tapText, new Color(1f, 0.84f, 0f, 0.4f), 2);

            // Hint Text
            GameObject hintText = FindOrCreateChild(tapArea, "HintText");
            TextMeshProUGUI hintTmp = GetOrAddComponent<TextMeshProUGUI>(hintText);
            hintTmp.text = "o mantén presionado para abrir rápido";
            hintTmp.fontSize = 14;
            hintTmp.color = TEXT_SECONDARY;
            hintTmp.alignment = TextAlignmentOptions.Center;
            LayoutElement hintLE = GetOrAddComponent<LayoutElement>(hintText);
            hintLE.minHeight = 20;

            // Pulsing Finger Icon
            GameObject fingerIcon = FindOrCreateChild(tapArea, "FingerIcon");
            RectTransform fingerRT = GetOrAddComponent<RectTransform>(fingerIcon);
            fingerRT.anchorMin = new Vector2(0.5f, 0);
            fingerRT.anchorMax = new Vector2(0.5f, 0);
            fingerRT.sizeDelta = new Vector2(50, 50);
            fingerRT.anchoredPosition = new Vector2(0, 80);

            Image fingerImage = GetOrAddComponent<Image>(fingerIcon);
            fingerImage.color = TEXT_SECONDARY;

            Debug.Log("[ChestOpeningUIBuilder] TapToOpenArea creado");
        }

        // ==================== REWARDS REVEAL AREA ====================

        private static void CreateRewardsRevealArea(GameObject parent)
        {
            GameObject rewardsArea = FindOrCreateChild(parent, "RewardsRevealArea");
            rewardsArea.SetActive(false);

            RectTransform rewardsRT = GetOrAddComponent<RectTransform>(rewardsArea);
            rewardsRT.anchorMin = new Vector2(0, 0);
            rewardsRT.anchorMax = new Vector2(1, 1);
            rewardsRT.offsetMin = new Vector2(CONTENT_PADDING, 150);
            rewardsRT.offsetMax = new Vector2(-CONTENT_PADDING, -HEADER_HEIGHT - 50);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(rewardsArea);
            vlg.spacing = 30;
            vlg.padding = new RectOffset(10, 10, 20, 20);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Title
            GameObject rewardsTitle = FindOrCreateChild(rewardsArea, "RewardsTitle");
            TextMeshProUGUI rewardsTitleText = GetOrAddComponent<TextMeshProUGUI>(rewardsTitle);
            rewardsTitleText.text = "RECOMPENSAS";
            rewardsTitleText.fontSize = 32;
            rewardsTitleText.fontStyle = FontStyles.Bold;
            rewardsTitleText.color = GOLD;
            rewardsTitleText.alignment = TextAlignmentOptions.Center;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(rewardsTitle);
            titleLE.minHeight = 45;
            AddOutline(rewardsTitle, new Color(1f, 0.84f, 0f, 0.4f), 2);

            // Rewards Grid
            GameObject rewardsGrid = FindOrCreateChild(rewardsArea, "RewardsGrid");
            GridLayoutGroup glg = GetOrAddComponent<GridLayoutGroup>(rewardsGrid);
            glg.cellSize = new Vector2(REWARD_CARD_SIZE, REWARD_CARD_SIZE + 30);
            glg.spacing = new Vector2(20, 20);
            glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            glg.startAxis = GridLayoutGroup.Axis.Horizontal;
            glg.childAlignment = TextAnchor.MiddleCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 3;

            LayoutElement gridLE = GetOrAddComponent<LayoutElement>(rewardsGrid);
            gridLE.minHeight = (REWARD_CARD_SIZE + 50) * 2;
            gridLE.preferredHeight = (REWARD_CARD_SIZE + 50) * 2;

            // Sample rewards
            CreateRewardCard(rewardsGrid, "Reward1", COIN_COLOR, "500", "Monedas", false);
            CreateRewardCard(rewardsGrid, "Reward2", GEM_COLOR, "25", "Gemas", false);
            CreateRewardCard(rewardsGrid, "Reward3", EPIC_CHEST, "", "Avatar", true);
            CreateRewardCard(rewardsGrid, "Reward4", COIN_COLOR, "200", "Monedas", false);
            CreateRewardCard(rewardsGrid, "Reward5", PURPLE_PREMIUM, "", "Tema", true);

            Debug.Log("[ChestOpeningUIBuilder] RewardsRevealArea creado");
        }

        private static void CreateRewardCard(GameObject parent, string name, Color color, string amount, string label, bool isSpecial)
        {
            GameObject card = FindOrCreateChild(parent, name);

            Image cardBg = GetOrAddComponent<Image>(card);
            cardBg.color = isSpecial ? new Color(0.15f, 0.1f, 0.25f, 1f) : CARD_BG;
            AddOutline(card, isSpecial ? PURPLE_PREMIUM : color * 0.6f, isSpecial ? 2 : 1);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(card);
            vlg.spacing = 8;
            vlg.padding = new RectOffset(10, 10, 15, 10);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // NEW badge for special items
            if (isSpecial)
            {
                GameObject newBadge = FindOrCreateChild(card, "NewBadge");
                RectTransform badgeRT = GetOrAddComponent<RectTransform>(newBadge);
                badgeRT.anchorMin = new Vector2(0, 1);
                badgeRT.anchorMax = new Vector2(0, 1);
                badgeRT.pivot = new Vector2(0, 1);
                badgeRT.anchoredPosition = new Vector2(5, -5);
                badgeRT.sizeDelta = new Vector2(45, 22);

                Image badgeBg = GetOrAddComponent<Image>(newBadge);
                badgeBg.color = GOLD;

                GameObject badgeText = FindOrCreateChild(newBadge, "Text");
                TextMeshProUGUI badgeTmp = GetOrAddComponent<TextMeshProUGUI>(badgeText);
                badgeTmp.text = "NUEVO";
                badgeTmp.fontSize = 10;
                badgeTmp.fontStyle = FontStyles.Bold;
                badgeTmp.color = TEXT_DARK;
                badgeTmp.alignment = TextAlignmentOptions.Center;
                SetRectTransformStretch(badgeText);
            }

            // Icon
            GameObject iconObj = FindOrCreateChild(card, "Icon");
            Image iconImage = GetOrAddComponent<Image>(iconObj);
            iconImage.color = color;
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(iconObj);
            iconLE.minHeight = 55;
            iconLE.preferredHeight = 55;
            iconLE.minWidth = 55;
            iconLE.preferredWidth = 55;

            // Amount (if any)
            if (!string.IsNullOrEmpty(amount))
            {
                GameObject amountObj = FindOrCreateChild(card, "Amount");
                TextMeshProUGUI amountText = GetOrAddComponent<TextMeshProUGUI>(amountObj);
                amountText.text = $"+{amount}";
                amountText.fontSize = 22;
                amountText.fontStyle = FontStyles.Bold;
                amountText.color = color;
                amountText.alignment = TextAlignmentOptions.Center;
                LayoutElement amountLE = GetOrAddComponent<LayoutElement>(amountObj);
                amountLE.minHeight = 28;
            }

            // Label
            GameObject labelObj = FindOrCreateChild(card, "Label");
            TextMeshProUGUI labelText = GetOrAddComponent<TextMeshProUGUI>(labelObj);
            labelText.text = label;
            labelText.fontSize = 14;
            labelText.color = TEXT_SECONDARY;
            labelText.alignment = TextAlignmentOptions.Center;
            LayoutElement labelLE = GetOrAddComponent<LayoutElement>(labelObj);
            labelLE.minHeight = 20;
        }

        // ==================== COLLECT BUTTON ====================

        private static void CreateCollectButton(GameObject parent)
        {
            GameObject collectArea = FindOrCreateChild(parent, "CollectButtonArea");
            collectArea.SetActive(false);

            RectTransform collectRT = GetOrAddComponent<RectTransform>(collectArea);
            collectRT.anchorMin = new Vector2(0, 0);
            collectRT.anchorMax = new Vector2(1, 0);
            collectRT.pivot = new Vector2(0.5f, 0);
            collectRT.anchoredPosition = new Vector2(0, 30);
            collectRT.sizeDelta = new Vector2(-CONTENT_PADDING * 2, 70);

            Image collectBg = GetOrAddComponent<Image>(collectArea);
            collectBg.color = BUTTON_SUCCESS;

            Button collectButton = GetOrAddComponent<Button>(collectArea);
            SetupButtonColors(collectButton, BUTTON_SUCCESS);
            AddOutline(collectArea, new Color(0.3f, 1f, 0.5f, 0.5f), 3);

            // Glow effect
            Shadow glow = GetOrAddComponent<Shadow>(collectArea);
            glow.effectColor = new Color(0.2f, 0.8f, 0.4f, 0.4f);
            glow.effectDistance = new Vector2(0, -4);

            GameObject collectTextObj = FindOrCreateChild(collectArea, "Text");
            TextMeshProUGUI collectText = GetOrAddComponent<TextMeshProUGUI>(collectTextObj);
            collectText.text = "RECOGER TODO";
            collectText.fontSize = 24;
            collectText.fontStyle = FontStyles.Bold;
            collectText.color = TEXT_DARK;
            collectText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(collectTextObj);

            Debug.Log("[ChestOpeningUIBuilder] CollectButton creado");
        }

        // ==================== CHEST INFO PANEL ====================

        private static void CreateChestInfoPanel(Canvas canvas)
        {
            GameObject blocker = FindOrCreateChild(canvas.gameObject, "ChestInfoBlocker");
            blocker.SetActive(false);

            SetRectTransformStretch(blocker);
            Image blockerBg = GetOrAddComponent<Image>(blocker);
            blockerBg.color = BLOCKER_BG;
            Button blockerBtn = GetOrAddComponent<Button>(blocker);
            blockerBtn.transition = Selectable.Transition.None;
            blocker.transform.SetAsLastSibling();

            GameObject popup = FindOrCreateChild(blocker, "ChestInfoPopup");
            RectTransform popupRT = GetOrAddComponent<RectTransform>(popup);
            popupRT.anchorMin = new Vector2(0.5f, 0.5f);
            popupRT.anchorMax = new Vector2(0.5f, 0.5f);
            popupRT.sizeDelta = new Vector2(500, 550);

            Image popupBg = GetOrAddComponent<Image>(popup);
            popupBg.color = POPUP_BG;
            AddOutline(popup, EPIC_CHEST, 2);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(popup);
            vlg.spacing = 15;
            vlg.padding = new RectOffset(25, 25, 25, 25);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Close Button
            GameObject closeBtn = FindOrCreateChild(popup, "CloseButton");
            RectTransform closeRT = GetOrAddComponent<RectTransform>(closeBtn);
            closeRT.anchorMin = new Vector2(1, 1);
            closeRT.anchorMax = new Vector2(1, 1);
            closeRT.pivot = new Vector2(1, 1);
            closeRT.anchoredPosition = new Vector2(-10, -10);
            closeRT.sizeDelta = new Vector2(35, 35);

            Image closeBg = GetOrAddComponent<Image>(closeBtn);
            closeBg.color = BUTTON_SECONDARY;
            Button closeButton = GetOrAddComponent<Button>(closeBtn);
            SetupButtonColors(closeButton, BUTTON_SECONDARY);

            GameObject closeText = FindOrCreateChild(closeBtn, "Text");
            TextMeshProUGUI closeTmp = GetOrAddComponent<TextMeshProUGUI>(closeText);
            closeTmp.text = "X";
            closeTmp.fontSize = 20;
            closeTmp.fontStyle = FontStyles.Bold;
            closeTmp.color = TEXT_PRIMARY;
            closeTmp.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(closeText);

            // Title
            GameObject titleObj = FindOrCreateChild(popup, "Title");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "COFRE EPICO";
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = EPIC_CHEST;
            titleText.alignment = TextAlignmentOptions.Center;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 40;

            // Description
            GameObject descObj = FindOrCreateChild(popup, "Description");
            TextMeshProUGUI descText = GetOrAddComponent<TextMeshProUGUI>(descObj);
            descText.text = "Contiene 4-6 recompensas con alta probabilidad de items epicos y raros.";
            descText.fontSize = 14;
            descText.color = TEXT_SECONDARY;
            descText.alignment = TextAlignmentOptions.Center;
            LayoutElement descLE = GetOrAddComponent<LayoutElement>(descObj);
            descLE.minHeight = 45;

            // Possible Rewards Header
            GameObject rewardsHeader = FindOrCreateChild(popup, "RewardsHeader");
            TextMeshProUGUI rewardsHeaderText = GetOrAddComponent<TextMeshProUGUI>(rewardsHeader);
            rewardsHeaderText.text = "POSIBLES RECOMPENSAS";
            rewardsHeaderText.fontSize = 16;
            rewardsHeaderText.fontStyle = FontStyles.Bold;
            rewardsHeaderText.color = CYAN_NEON;
            rewardsHeaderText.alignment = TextAlignmentOptions.Center;
            LayoutElement rewardsHeaderLE = GetOrAddComponent<LayoutElement>(rewardsHeader);
            rewardsHeaderLE.minHeight = 25;

            // Rewards List
            GameObject rewardsList = FindOrCreateChild(popup, "RewardsList");
            VerticalLayoutGroup listVlg = GetOrAddComponent<VerticalLayoutGroup>(rewardsList);
            listVlg.spacing = 8;
            listVlg.padding = new RectOffset(10, 10, 10, 10);
            listVlg.childAlignment = TextAnchor.UpperCenter;
            listVlg.childControlWidth = true;
            listVlg.childControlHeight = true;
            listVlg.childForceExpandWidth = true;
            listVlg.childForceExpandHeight = false;

            Image listBg = GetOrAddComponent<Image>(rewardsList);
            listBg.color = CARD_BG;

            LayoutElement listLE = GetOrAddComponent<LayoutElement>(rewardsList);
            listLE.minHeight = 200;

            // Reward entries
            CreateRewardEntry(rewardsList, "Entry1", COIN_COLOR, "Monedas", "200 - 1,000");
            CreateRewardEntry(rewardsList, "Entry2", GEM_COLOR, "Gemas", "10 - 50");
            CreateRewardEntry(rewardsList, "Entry3", PURPLE_PREMIUM, "Temas Raros", "15% probabilidad");
            CreateRewardEntry(rewardsList, "Entry4", EPIC_CHEST, "Avatares Epicos", "5% probabilidad");
            CreateRewardEntry(rewardsList, "Entry5", GOLD, "Power-ups", "Garantizado x1");

            // Chest Rarities
            GameObject raritiesPanel = FindOrCreateChild(popup, "RaritiesPanel");
            HorizontalLayoutGroup raritiesHlg = GetOrAddComponent<HorizontalLayoutGroup>(raritiesPanel);
            raritiesHlg.spacing = 10;
            raritiesHlg.childAlignment = TextAnchor.MiddleCenter;
            raritiesHlg.childControlWidth = true;
            raritiesHlg.childControlHeight = true;
            raritiesHlg.childForceExpandWidth = true;

            LayoutElement raritiesLE = GetOrAddComponent<LayoutElement>(raritiesPanel);
            raritiesLE.minHeight = 50;

            CreateRarityBadge(raritiesPanel, "Common", "COMUN", COMMON_CHEST, false);
            CreateRarityBadge(raritiesPanel, "Rare", "RARO", RARE_CHEST, false);
            CreateRarityBadge(raritiesPanel, "Epic", "EPICO", EPIC_CHEST, true);
            CreateRarityBadge(raritiesPanel, "Legendary", "LEGENDARIO", LEGENDARY_CHEST, false);

            Debug.Log("[ChestOpeningUIBuilder] ChestInfoPanel creado");
        }

        private static void CreateRewardEntry(GameObject parent, string name, Color color, string rewardName, string details)
        {
            GameObject entry = FindOrCreateChild(parent, name);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(entry);
            hlg.spacing = 12;
            hlg.padding = new RectOffset(5, 5, 5, 5);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            LayoutElement entryLE = GetOrAddComponent<LayoutElement>(entry);
            entryLE.minHeight = 32;

            // Icon
            GameObject iconObj = FindOrCreateChild(entry, "Icon");
            Image iconImage = GetOrAddComponent<Image>(iconObj);
            iconImage.color = color;
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(iconObj);
            iconLE.minWidth = 24;
            iconLE.minHeight = 24;

            // Name
            GameObject nameObj = FindOrCreateChild(entry, "Name");
            TextMeshProUGUI nameText = GetOrAddComponent<TextMeshProUGUI>(nameObj);
            nameText.text = rewardName;
            nameText.fontSize = 15;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = TEXT_PRIMARY;
            nameText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement nameLE = GetOrAddComponent<LayoutElement>(nameObj);
            nameLE.minWidth = 140;

            // Details
            GameObject detailsObj = FindOrCreateChild(entry, "Details");
            TextMeshProUGUI detailsText = GetOrAddComponent<TextMeshProUGUI>(detailsObj);
            detailsText.text = details;
            detailsText.fontSize = 13;
            detailsText.color = TEXT_SECONDARY;
            detailsText.alignment = TextAlignmentOptions.MidlineRight;
            LayoutElement detailsLE = GetOrAddComponent<LayoutElement>(detailsObj);
            detailsLE.flexibleWidth = 1;
        }

        private static void CreateRarityBadge(GameObject parent, string name, string label, Color color, bool isSelected)
        {
            GameObject badge = FindOrCreateChild(parent, name);

            Image badgeBg = GetOrAddComponent<Image>(badge);
            badgeBg.color = isSelected ? color : BUTTON_SECONDARY;
            AddOutline(badge, color, isSelected ? 2 : 1);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(badge);
            vlg.spacing = 2;
            vlg.padding = new RectOffset(8, 8, 5, 5);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Label
            GameObject labelObj = FindOrCreateChild(badge, "Label");
            TextMeshProUGUI labelText = GetOrAddComponent<TextMeshProUGUI>(labelObj);
            labelText.text = label;
            labelText.fontSize = 10;
            labelText.fontStyle = FontStyles.Bold;
            labelText.color = isSelected ? TEXT_DARK : color;
            labelText.alignment = TextAlignmentOptions.Center;
            LayoutElement labelLE = GetOrAddComponent<LayoutElement>(labelObj);
            labelLE.minHeight = 16;
        }

        // ==================== UTILITY METHODS ====================

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
