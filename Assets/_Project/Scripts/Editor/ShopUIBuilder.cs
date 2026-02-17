using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Construye la UI completa de Shop (Tienda) - V2 Redesign
    /// 5 Tabs: DESTACADO | GEMAS | MONEDAS | TEMAS | COSMETICOS
    /// Font sizes matched to app standard, BackButton prefab, larger cards
    /// </summary>
    public class ShopUIBuilder : EditorWindow
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
        private static readonly Color ORANGE_OFFER = new Color(1f, 0.5f, 0.1f, 1f);
        private static readonly Color PINK_COSMETIC = new Color(1f, 0.3f, 0.6f, 1f);

        private static readonly Color GEM_COLOR = new Color(0.4f, 0.8f, 1f, 1f);
        private static readonly Color COIN_COLOR = new Color(1f, 0.85f, 0.3f, 1f);

        private static readonly Color BLOCKER_BG = new Color(0f, 0f, 0f, 0.85f);

        private static readonly Color TAB_ACTIVE = CYAN_NEON;
        private static readonly Color TAB_INACTIVE = new Color(0.12f, 0.16f, 0.2f, 1f);

        // ==================== DIMENSIONES (V2 - Larger) ====================
        private const float HEADER_HEIGHT = 110f;
        private const float TABS_HEIGHT = 55f;
        private const float CONTENT_PADDING = 20f;
        private const float ITEM_HEIGHT = 220f;
        private const float ITEM_WIDTH = 180f;

        [MenuItem("DigitPark/UI Builders/Monetization/Shop", false, 180)]
        public static void BuildUI()
        {
            if (!EditorUtility.DisplayDialog("Shop UI Builder",
                "Esto construira la UI completa de Shop (Tienda) V2.\nAsegurate de tener la escena Shop abierta.\n\nContinuar?",
                "Si", "No"))
                return;

            BuildCompleteUI();
        }

        private static void BuildCompleteUI()
        {
            Debug.Log("[ShopUIBuilder] ========== INICIANDO CONSTRUCCION V2 ==========");

            CleanupOldUI();

            Canvas canvas = SetupCanvas();
            if (canvas == null) return;

            CreateBackground(canvas);
            GameObject safeArea = CreateSafeArea(canvas);

            CreateHeader(safeArea);
            CreateTabs(safeArea);

            // Content areas for each tab
            CreateFeaturedContent(safeArea);
            CreateGemsContent(safeArea);
            CreateCoinsContent(safeArea);
            CreateThemesContent(safeArea);
            CreateCosmeticsContent(safeArea);

            CreatePurchaseConfirmPopup(canvas);
            CreateNotEnoughGemsPopup(canvas);

            MarkSceneDirty();
            Debug.Log("[ShopUIBuilder] ========== CONSTRUCCION V2 COMPLETADA ==========");
        }

        // ==================== CANVAS SETUP ====================

        private static Canvas SetupCanvas()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();

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

            // BackButton - Use prefab
            GameObject backBtnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Common/BackButton.prefab");
            if (backBtnPrefab != null)
            {
                // Remove old BackButton if exists
                Transform oldBack = header.transform.Find("BackButton");
                if (oldBack != null) Object.DestroyImmediate(oldBack.gameObject);

                GameObject backBtn = (GameObject)PrefabUtility.InstantiatePrefab(backBtnPrefab, header.transform);
                backBtn.name = "BackButton";
                RectTransform backRT = backBtn.GetComponent<RectTransform>();
                if (backRT != null)
                {
                    backRT.anchorMin = new Vector2(0, 0.5f);
                    backRT.anchorMax = new Vector2(0, 0.5f);
                    backRT.pivot = new Vector2(0, 0.5f);
                    backRT.anchoredPosition = new Vector2(20, 0);
                    backRT.sizeDelta = new Vector2(50, 50);
                }
            }
            else
            {
                // Fallback: manual BackButton
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
            }

            // Title - V2: Font size 78
            GameObject titleObj = FindOrCreateChild(header, "TitleText");
            RectTransform titleRT = GetOrAddComponent<RectTransform>(titleObj);
            titleRT.anchorMin = new Vector2(0, 0.5f);
            titleRT.anchorMax = new Vector2(0, 0.5f);
            titleRT.pivot = new Vector2(0, 0.5f);
            titleRT.anchoredPosition = new Vector2(85, 0);
            titleRT.sizeDelta = new Vector2(300, 80);

            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "TIENDA";
            titleText.fontSize = 78;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = CYAN_NEON;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = 36;
            titleText.fontSizeMax = 78;
            AddOutline(titleObj, CYAN_GLOW, 2);

            // Currency Display
            CreateCurrencyDisplay(header);

            Debug.Log("[ShopUIBuilder] Header creado (V2)");
        }

        private static void CreateCurrencyDisplay(GameObject header)
        {
            GameObject currencyDisplay = FindOrCreateChild(header, "CurrencyDisplay");

            RectTransform currencyRT = GetOrAddComponent<RectTransform>(currencyDisplay);
            currencyRT.anchorMin = new Vector2(1, 0.5f);
            currencyRT.anchorMax = new Vector2(1, 0.5f);
            currencyRT.pivot = new Vector2(1, 0.5f);
            currencyRT.anchoredPosition = new Vector2(-20, 0);
            currencyRT.sizeDelta = new Vector2(280, 50);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(currencyDisplay);
            hlg.spacing = 15;
            hlg.childAlignment = TextAnchor.MiddleRight;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            CreateCurrencyItem(currencyDisplay, "GemsDisplay", "1,250", GEM_COLOR, true);
            CreateCurrencyItem(currencyDisplay, "CoinsDisplay", "5,430", COIN_COLOR, false);
        }

        private static void CreateCurrencyItem(GameObject parent, string name, string amount, Color color, bool isGems)
        {
            GameObject item = FindOrCreateChild(parent, name);

            RectTransform itemRT = GetOrAddComponent<RectTransform>(item);
            itemRT.sizeDelta = new Vector2(125, 45);

            Image itemBg = GetOrAddComponent<Image>(item);
            itemBg.color = new Color(0.1f, 0.15f, 0.2f, 0.9f);
            AddOutline(item, color * 0.5f);

            Button itemBtn = GetOrAddComponent<Button>(item);
            SetupButtonColors(itemBtn, new Color(0.1f, 0.15f, 0.2f, 0.9f));

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(item);
            hlg.spacing = 8;
            hlg.padding = new RectOffset(10, 10, 5, 5);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            LayoutElement itemLE = GetOrAddComponent<LayoutElement>(item);
            itemLE.minWidth = 125;
            itemLE.preferredWidth = 125;

            // Icon
            GameObject iconObj = FindOrCreateChild(item, "Icon");
            Image iconImage = GetOrAddComponent<Image>(iconObj);
            iconImage.color = color;
            iconImage.preserveAspect = true;
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(iconObj);
            iconLE.minWidth = 28;
            iconLE.minHeight = 28;
            iconLE.preferredWidth = 28;
            iconLE.preferredHeight = 28;

            // Amount - V2: Font size 24
            GameObject amountObj = FindOrCreateChild(item, "Amount");
            TextMeshProUGUI amountText = GetOrAddComponent<TextMeshProUGUI>(amountObj);
            amountText.text = amount;
            amountText.fontSize = 24;
            amountText.fontStyle = FontStyles.Bold;
            amountText.color = TEXT_PRIMARY;
            amountText.alignment = TextAlignmentOptions.MidlineLeft;
            amountText.enableAutoSizing = true;
            amountText.fontSizeMin = 14;
            amountText.fontSizeMax = 24;
            LayoutElement amountLE = GetOrAddComponent<LayoutElement>(amountObj);
            amountLE.flexibleWidth = 1;

            // Plus Button
            GameObject plusObj = FindOrCreateChild(item, "PlusButton");
            Image plusBg = GetOrAddComponent<Image>(plusObj);
            plusBg.color = color;
            LayoutElement plusLE = GetOrAddComponent<LayoutElement>(plusObj);
            plusLE.minWidth = 22;
            plusLE.minHeight = 22;
            plusLE.preferredWidth = 22;
            plusLE.preferredHeight = 22;

            GameObject plusTextObj = FindOrCreateChild(plusObj, "Text");
            TextMeshProUGUI plusText = GetOrAddComponent<TextMeshProUGUI>(plusTextObj);
            plusText.text = "+";
            plusText.fontSize = 18;
            plusText.fontStyle = FontStyles.Bold;
            plusText.color = TEXT_DARK;
            plusText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(plusTextObj);
        }

        // ==================== TABS (5 tabs) ====================

        private static void CreateTabs(GameObject parent)
        {
            GameObject tabsPanel = FindOrCreateChild(parent, "TabsPanel");

            float topOffset = HEADER_HEIGHT + 10;

            RectTransform tabsRT = GetOrAddComponent<RectTransform>(tabsPanel);
            tabsRT.anchorMin = new Vector2(0, 1);
            tabsRT.anchorMax = new Vector2(1, 1);
            tabsRT.pivot = new Vector2(0.5f, 1);
            tabsRT.anchoredPosition = new Vector2(0, -topOffset);
            tabsRT.sizeDelta = new Vector2(-CONTENT_PADDING * 2, TABS_HEIGHT);

            Image tabsBg = GetOrAddComponent<Image>(tabsPanel);
            tabsBg.color = new Color(0.04f, 0.07f, 0.11f, 0.9f);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(tabsPanel);
            hlg.spacing = 6f;
            hlg.padding = new RectOffset(6, 6, 5, 5);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // 5 Tabs: DESTACADO | GEMAS | MONEDAS | TEMAS | COSMETICOS
            CreateTab(tabsPanel, "FeaturedTab", "DESTACADO", true, GOLD);
            CreateTab(tabsPanel, "GemsTab", "GEMAS", false, GEM_COLOR);
            CreateTab(tabsPanel, "CoinsTab", "MONEDAS", false, COIN_COLOR);
            CreateTab(tabsPanel, "ThemesTab", "TEMAS", false, PURPLE_PREMIUM);
            CreateTab(tabsPanel, "CosmeticsTab", "COSMETICOS", false, PINK_COSMETIC);

            Debug.Log("[ShopUIBuilder] Tabs creados (5 tabs V2)");
        }

        private static void CreateTab(GameObject parent, string name, string label, bool isActive, Color accentColor)
        {
            GameObject tab = FindOrCreateChild(parent, name);

            Image tabBg = GetOrAddComponent<Image>(tab);
            tabBg.color = isActive ? accentColor : TAB_INACTIVE;

            Button tabButton = GetOrAddComponent<Button>(tab);
            SetupButtonColors(tabButton, isActive ? accentColor : TAB_INACTIVE);
            AddOutline(tab, isActive ? accentColor : CYAN_DARK);

            GameObject textObj = FindOrCreateChild(tab, "Text");
            TextMeshProUGUI tabText = GetOrAddComponent<TextMeshProUGUI>(textObj);
            tabText.text = label;
            tabText.fontSize = 24;
            tabText.fontStyle = FontStyles.Bold;
            tabText.color = isActive ? TEXT_DARK : TEXT_PRIMARY;
            tabText.alignment = TextAlignmentOptions.Center;
            tabText.enableAutoSizing = true;
            tabText.fontSizeMin = 12;
            tabText.fontSizeMax = 24;
            SetRectTransformStretch(textObj);

            LayoutElement le = GetOrAddComponent<LayoutElement>(tab);
            le.minHeight = 45;
            le.flexibleWidth = 1;
        }

        // ==================== CONTENT AREA HELPER ====================

        private static float ContentTopOffset => HEADER_HEIGHT + TABS_HEIGHT + 25;

        private static GameObject CreateScrollableContent(GameObject parent, string scrollName, string contentName)
        {
            GameObject scrollView = FindOrCreateChild(parent, scrollName);

            RectTransform scrollRT = GetOrAddComponent<RectTransform>(scrollView);
            scrollRT.anchorMin = Vector2.zero;
            scrollRT.anchorMax = Vector2.one;
            scrollRT.offsetMin = new Vector2(CONTENT_PADDING, CONTENT_PADDING);
            scrollRT.offsetMax = new Vector2(-CONTENT_PADDING, -ContentTopOffset);

            ScrollRect scrollRect = GetOrAddComponent<ScrollRect>(scrollView);
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.scrollSensitivity = 50f;

            Image scrollBg = GetOrAddComponent<Image>(scrollView);
            scrollBg.color = Color.clear;

            // Viewport
            GameObject viewport = FindOrCreateChild(scrollView, "Viewport");
            SetRectTransformStretch(viewport);
            RectTransform viewportRT = viewport.GetComponent<RectTransform>();
            Image vpImg = GetOrAddComponent<Image>(viewport);
            vpImg.color = Color.clear;
            vpImg.raycastTarget = true;
            GetOrAddComponent<RectMask2D>(viewport);
            scrollRect.viewport = viewportRT;

            // Content
            GameObject content = FindOrCreateChild(viewport, contentName);
            RectTransform contentRT = GetOrAddComponent<RectTransform>(content);
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.sizeDelta = new Vector2(0, 0);
            scrollRect.content = contentRT;

            ContentSizeFitter csf = GetOrAddComponent<ContentSizeFitter>(content);
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(content);
            vlg.spacing = 25;
            vlg.padding = new RectOffset(0, 0, 10, 30);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            return content;
        }

        // ==================== FEATURED TAB CONTENT ====================

        private static void CreateFeaturedContent(GameObject parent)
        {
            GameObject content = CreateScrollableContent(parent, "FeaturedScrollView", "FeaturedContent");

            // Starter Pack Banner
            CreateOfferBanner(content, "StarterPackBanner",
                "STARTER PACK", "500 Gemas + 5,000 Monedas + 1 Tema",
                "$2.99", "$9.99", "70% OFF", BUTTON_SUCCESS);

            // Weekly Deal
            CreateOfferBanner(content, "WeeklyDealBanner",
                "OFERTA SEMANAL", "1,200 Gemas + 10,000 Monedas",
                "$4.99", "$12.99", "60% OFF", PURPLE_PREMIUM);

            // Theme Bundle
            CreateOfferBanner(content, "ThemeBundleBanner",
                "TODOS LOS TEMAS", "9 Temas Premium - Ahorra $7.50!",
                "$14.99", "$22.50", "33% OFF", ORANGE_OFFER);

            Debug.Log("[ShopUIBuilder] FeaturedContent creado");
        }

        private static void CreateOfferBanner(GameObject parent, string name, string title, string subtitle, string price, string originalPrice, string badge, Color accentColor)
        {
            GameObject banner = FindOrCreateChild(parent, name);

            Image bannerBg = GetOrAddComponent<Image>(banner);
            bannerBg.color = new Color(accentColor.r * 0.2f, accentColor.g * 0.2f, accentColor.b * 0.2f, 1f);
            AddOutline(banner, accentColor, 2);

            Shadow shadow = GetOrAddComponent<Shadow>(banner);
            shadow.effectColor = new Color(accentColor.r, accentColor.g, accentColor.b, 0.4f);
            shadow.effectDistance = new Vector2(0, -4);

            LayoutElement bannerLE = GetOrAddComponent<LayoutElement>(banner);
            bannerLE.minHeight = 160;
            bannerLE.preferredHeight = 160;

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(banner);
            hlg.spacing = 20;
            hlg.padding = new RectOffset(25, 25, 15, 15);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            Button bannerBtn = GetOrAddComponent<Button>(banner);
            SetupButtonColors(bannerBtn, bannerBg.color);

            // Left side - Offer info
            GameObject infoPanel = FindOrCreateChild(banner, "InfoPanel");
            VerticalLayoutGroup infoVlg = GetOrAddComponent<VerticalLayoutGroup>(infoPanel);
            infoVlg.spacing = 8;
            infoVlg.childAlignment = TextAnchor.MiddleLeft;
            infoVlg.childControlWidth = true;
            infoVlg.childControlHeight = true;
            infoVlg.childForceExpandWidth = true;
            infoVlg.childForceExpandHeight = false;

            LayoutElement infoLE = GetOrAddComponent<LayoutElement>(infoPanel);
            infoLE.flexibleWidth = 1;

            // Badge
            GameObject badgeObj = FindOrCreateChild(infoPanel, "Badge");
            Image badgeBg = GetOrAddComponent<Image>(badgeObj);
            badgeBg.color = ORANGE_OFFER;
            LayoutElement badgeLE = GetOrAddComponent<LayoutElement>(badgeObj);
            badgeLE.minHeight = 28;
            badgeLE.preferredHeight = 28;
            badgeLE.minWidth = 120;
            badgeLE.preferredWidth = 120;

            HorizontalLayoutGroup badgeHlg = GetOrAddComponent<HorizontalLayoutGroup>(badgeObj);
            badgeHlg.padding = new RectOffset(10, 10, 2, 2);
            badgeHlg.childAlignment = TextAnchor.MiddleCenter;

            GameObject badgeTextObj = FindOrCreateChild(badgeObj, "Text");
            TextMeshProUGUI badgeText = GetOrAddComponent<TextMeshProUGUI>(badgeTextObj);
            badgeText.text = badge;
            badgeText.fontSize = 16;
            badgeText.fontStyle = FontStyles.Bold;
            badgeText.color = TEXT_PRIMARY;
            badgeText.alignment = TextAlignmentOptions.Center;

            // Title
            GameObject titleObj = FindOrCreateChild(infoPanel, "Title");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = title;
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GOLD;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 35;

            // Contents
            GameObject contentsObj = FindOrCreateChild(infoPanel, "Contents");
            TextMeshProUGUI contentsText = GetOrAddComponent<TextMeshProUGUI>(contentsObj);
            contentsText.text = subtitle;
            contentsText.fontSize = 18;
            contentsText.color = TEXT_SECONDARY;
            contentsText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement contentsLE = GetOrAddComponent<LayoutElement>(contentsObj);
            contentsLE.minHeight = 25;

            // Right side - Buy button
            GameObject buyPanel = FindOrCreateChild(banner, "BuyPanel");
            VerticalLayoutGroup buyVlg = GetOrAddComponent<VerticalLayoutGroup>(buyPanel);
            buyVlg.spacing = 5;
            buyVlg.childAlignment = TextAnchor.MiddleCenter;
            buyVlg.childControlWidth = true;
            buyVlg.childControlHeight = true;
            buyVlg.childForceExpandWidth = true;
            buyVlg.childForceExpandHeight = false;

            LayoutElement buyPanelLE = GetOrAddComponent<LayoutElement>(buyPanel);
            buyPanelLE.minWidth = 140;
            buyPanelLE.preferredWidth = 140;

            // Original Price (crossed out)
            GameObject origPriceObj = FindOrCreateChild(buyPanel, "OriginalPrice");
            TextMeshProUGUI origPriceText = GetOrAddComponent<TextMeshProUGUI>(origPriceObj);
            origPriceText.text = $"<s>{originalPrice}</s>";
            origPriceText.fontSize = 18;
            origPriceText.color = TEXT_SECONDARY;
            origPriceText.alignment = TextAlignmentOptions.Center;
            LayoutElement origPriceLE = GetOrAddComponent<LayoutElement>(origPriceObj);
            origPriceLE.minHeight = 24;

            // Buy Button
            GameObject buyBtn = FindOrCreateChild(buyPanel, "BuyButton");
            Image buyBg = GetOrAddComponent<Image>(buyBtn);
            buyBg.color = BUTTON_SUCCESS;
            Button buyButton = GetOrAddComponent<Button>(buyBtn);
            SetupButtonColors(buyButton, BUTTON_SUCCESS);
            AddOutline(buyBtn, new Color(0.3f, 1f, 0.5f, 0.5f), 2);
            LayoutElement buyLE = GetOrAddComponent<LayoutElement>(buyBtn);
            buyLE.minHeight = 55;
            buyLE.preferredHeight = 55;

            GameObject buyTextObj = FindOrCreateChild(buyBtn, "Text");
            TextMeshProUGUI buyText = GetOrAddComponent<TextMeshProUGUI>(buyTextObj);
            buyText.text = price;
            buyText.fontSize = 30;
            buyText.fontStyle = FontStyles.Bold;
            buyText.color = TEXT_DARK;
            buyText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(buyTextObj);
        }

        // ==================== GEMS TAB CONTENT ====================

        private static void CreateGemsContent(GameObject parent)
        {
            GameObject content = CreateScrollableContent(parent, "GemsScrollView", "GemsContent");

            // Section Header
            CreateShopSection(content, "GemsSection", "GEMAS", GEM_COLOR);

            // Gems Grid - V2: Updated tiers
            GameObject grid = FindOrCreateChild(content, "GemsGrid");

            GridLayoutGroup glg = GetOrAddComponent<GridLayoutGroup>(grid);
            glg.cellSize = new Vector2(ITEM_WIDTH, ITEM_HEIGHT);
            glg.spacing = new Vector2(15, 15);
            glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            glg.startAxis = GridLayoutGroup.Axis.Horizontal;
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 3;

            LayoutElement gridLE = GetOrAddComponent<LayoutElement>(grid);
            gridLE.minHeight = ITEM_HEIGHT * 2 + 15;

            // V2 Gem tiers (user-confirmed)
            CreateShopItem(grid, "Gems_80", "80", "$0.99", "", GEM_COLOR, false);
            CreateShopItem(grid, "Gems_500", "500", "$4.99", "+10%", GEM_COLOR, false);
            CreateShopItem(grid, "Gems_1200", "1,200", "$9.99", "+20%", GEM_COLOR, true);
            CreateShopItem(grid, "Gems_2800", "2,800", "$19.99", "+25%", GEM_COLOR, false);
            CreateShopItem(grid, "Gems_7500", "7,500", "$49.99", "+30%", GEM_COLOR, false);
            CreateShopItem(grid, "Gems_16000", "16,000", "$99.99", "+35%", GEM_COLOR, false);

            // Initially inactive - only Featured tab is active on load
            content.transform.parent.parent.gameObject.SetActive(false);

            Debug.Log("[ShopUIBuilder] GemsContent creado (V2 tiers)");
        }

        // ==================== COINS TAB CONTENT ====================

        private static void CreateCoinsContent(GameObject parent)
        {
            GameObject content = CreateScrollableContent(parent, "CoinsScrollView", "CoinsContent");

            CreateShopSection(content, "CoinsSection", "MONEDAS", COIN_COLOR);

            // V2: 4 coin packs
            GameObject grid = FindOrCreateChild(content, "CoinsGrid");

            GridLayoutGroup glg = GetOrAddComponent<GridLayoutGroup>(grid);
            glg.cellSize = new Vector2(ITEM_WIDTH, ITEM_HEIGHT);
            glg.spacing = new Vector2(15, 15);
            glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            glg.startAxis = GridLayoutGroup.Axis.Horizontal;
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 3;

            LayoutElement gridLE = GetOrAddComponent<LayoutElement>(grid);
            gridLE.minHeight = ITEM_HEIGHT * 2 + 15;

            CreateShopItem(grid, "Coins_1000", "1,000", "50", "", COIN_COLOR, false, true);
            CreateShopItem(grid, "Coins_5000", "5,000", "200", "+25%", COIN_COLOR, false, true);
            CreateShopItem(grid, "Coins_15000", "15,000", "500", "+50%", COIN_COLOR, true, true);
            CreateShopItem(grid, "Coins_50000", "50,000", "1,500", "+60%", COIN_COLOR, false, true);

            content.transform.parent.parent.gameObject.SetActive(false);

            Debug.Log("[ShopUIBuilder] CoinsContent creado (V2 - 4 packs)");
        }

        // ==================== THEMES TAB CONTENT ====================

        private static void CreateThemesContent(GameObject parent)
        {
            GameObject content = CreateScrollableContent(parent, "ThemesScrollView", "ThemesContent");

            // Theme Bundle Banner at top
            CreateOfferBanner(content, "ThemeBundleBannerInTab",
                "TODOS LOS TEMAS", "9 Temas Premium - Ahorra $7.50!",
                "$14.99", "$22.50", "BUNDLE", PURPLE_PREMIUM);

            // Section Header
            CreateShopSection(content, "ThemesSection", "TEMAS INDIVIDUALES", PURPLE_PREMIUM);

            // Themes Grid
            GameObject grid = FindOrCreateChild(content, "ThemesGrid");

            GridLayoutGroup glg = GetOrAddComponent<GridLayoutGroup>(grid);
            glg.cellSize = new Vector2(ITEM_WIDTH, ITEM_HEIGHT);
            glg.spacing = new Vector2(15, 15);
            glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            glg.startAxis = GridLayoutGroup.Axis.Horizontal;
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 3;

            LayoutElement gridLE = GetOrAddComponent<LayoutElement>(grid);
            gridLE.minHeight = ITEM_HEIGHT * 3 + 30;

            // 9 premium themes at $2.50 each
            string[] themeNames = { "Volcano", "Ocean", "Clean Light", "Retro Arcade", "Cyberpunk", "Minimalist", "Forest", "Vaporwave", "Steampunk" };
            Color[] themeColors = {
                new Color(1f, 0.3f, 0.1f, 1f),   // Volcano
                new Color(0.1f, 0.5f, 1f, 1f),    // Ocean
                new Color(0.9f, 0.9f, 0.9f, 1f),  // Clean Light
                new Color(1f, 0.5f, 0f, 1f),       // Retro Arcade
                new Color(1f, 0f, 1f, 1f),          // Cyberpunk
                new Color(0.7f, 0.7f, 0.7f, 1f),   // Minimalist
                new Color(0.2f, 0.8f, 0.3f, 1f),   // Forest
                new Color(1f, 0.4f, 0.8f, 1f),      // Vaporwave
                new Color(0.8f, 0.5f, 0.2f, 1f)     // Steampunk
            };

            for (int i = 0; i < themeNames.Length; i++)
            {
                CreateThemeItem(grid, $"Theme_{themeNames[i].Replace(" ", "")}", themeNames[i], "$2.50", themeColors[i]);
            }

            content.transform.parent.parent.gameObject.SetActive(false);

            Debug.Log("[ShopUIBuilder] ThemesContent creado (9 temas)");
        }

        private static void CreateThemeItem(GameObject parent, string name, string themeName, string price, Color themeColor)
        {
            GameObject item = FindOrCreateChild(parent, name);

            Image itemBg = GetOrAddComponent<Image>(item);
            itemBg.color = CARD_BG;
            AddOutline(item, themeColor * 0.6f, 1);

            Button itemBtn = GetOrAddComponent<Button>(item);
            SetupButtonColors(itemBtn, CARD_BG);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(item);
            vlg.spacing = 8;
            vlg.padding = new RectOffset(10, 10, 12, 12);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Color Preview
            GameObject previewObj = FindOrCreateChild(item, "ColorPreview");
            Image previewImage = GetOrAddComponent<Image>(previewObj);
            previewImage.color = themeColor;
            LayoutElement previewLE = GetOrAddComponent<LayoutElement>(previewObj);
            previewLE.minHeight = 60;
            previewLE.preferredHeight = 60;

            // Theme Name
            GameObject nameObj = FindOrCreateChild(item, "ThemeName");
            TextMeshProUGUI nameText = GetOrAddComponent<TextMeshProUGUI>(nameObj);
            nameText.text = themeName;
            nameText.fontSize = 24;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = TEXT_PRIMARY;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.enableAutoSizing = true;
            nameText.fontSizeMin = 14;
            nameText.fontSizeMax = 24;
            LayoutElement nameLE = GetOrAddComponent<LayoutElement>(nameObj);
            nameLE.minHeight = 30;

            // Lock Badge
            GameObject lockObj = FindOrCreateChild(item, "LockBadge");
            Image lockBg = GetOrAddComponent<Image>(lockObj);
            lockBg.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            LayoutElement lockLE = GetOrAddComponent<LayoutElement>(lockObj);
            lockLE.minHeight = 24;
            lockLE.preferredHeight = 24;

            GameObject lockTextObj = FindOrCreateChild(lockObj, "Text");
            TextMeshProUGUI lockText = GetOrAddComponent<TextMeshProUGUI>(lockTextObj);
            lockText.text = "BLOQUEADO";
            lockText.fontSize = 16;
            lockText.fontStyle = FontStyles.Bold;
            lockText.color = TEXT_SECONDARY;
            lockText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(lockTextObj);

            // Price Button
            GameObject priceBtn = FindOrCreateChild(item, "PriceButton");
            Image priceBg = GetOrAddComponent<Image>(priceBtn);
            priceBg.color = BUTTON_SUCCESS;
            Button priceButton = GetOrAddComponent<Button>(priceBtn);
            SetupButtonColors(priceButton, BUTTON_SUCCESS);
            LayoutElement priceLE = GetOrAddComponent<LayoutElement>(priceBtn);
            priceLE.minHeight = 38;
            priceLE.preferredHeight = 38;

            GameObject priceTextObj = FindOrCreateChild(priceBtn, "Text");
            TextMeshProUGUI priceText = GetOrAddComponent<TextMeshProUGUI>(priceTextObj);
            priceText.text = price;
            priceText.fontSize = 24;
            priceText.fontStyle = FontStyles.Bold;
            priceText.color = TEXT_DARK;
            priceText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(priceTextObj);
        }

        // ==================== COSMETICS TAB CONTENT ====================

        private static void CreateCosmeticsContent(GameObject parent)
        {
            GameObject content = CreateScrollableContent(parent, "CosmeticsScrollView", "CosmeticsContent");

            // === MARCOS Section ===
            CreateShopSection(content, "FramesSection", "MARCOS", PINK_COSMETIC);

            // Frames grid (3 columns)
            GameObject framesGrid = FindOrCreateChild(content, "FramesGrid");
            GridLayoutGroup fglg = GetOrAddComponent<GridLayoutGroup>(framesGrid);
            fglg.cellSize = new Vector2(180, 200);
            fglg.spacing = new Vector2(12, 12);
            fglg.padding = new RectOffset(10, 10, 5, 10);
            fglg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            fglg.startAxis = GridLayoutGroup.Axis.Horizontal;
            fglg.childAlignment = TextAnchor.UpperCenter;
            fglg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            fglg.constraintCount = 3;

            ContentSizeFitter fcsf = GetOrAddComponent<ContentSizeFitter>(framesGrid);
            fcsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Coin frames (8)
            CreateCosmeticCard(framesGrid, "Frame_Basic", "Basico", "500", COIN_COLOR, "COMUN", false);
            CreateCosmeticCard(framesGrid, "Frame_Bronze", "Bronce", "1,000", COIN_COLOR, "COMUN", false);
            CreateCosmeticCard(framesGrid, "Frame_Silver", "Plata", "2,500", COIN_COLOR, "COMUN", false);
            CreateCosmeticCard(framesGrid, "Frame_Gold", "Oro", "5,000", COIN_COLOR, "RARO", false);
            CreateCosmeticCard(framesGrid, "Frame_Neon", "Neon", "7,500", COIN_COLOR, "RARO", false);
            CreateCosmeticCard(framesGrid, "Frame_Diamond", "Diamante", "10,000", COIN_COLOR, "EPICO", false);
            CreateCosmeticCard(framesGrid, "Frame_Crystal", "Cristal", "12,000", COIN_COLOR, "EPICO", false);
            CreateCosmeticCard(framesGrid, "Frame_Platinum", "Platino", "15,000", COIN_COLOR, "LEGENDARIO", true);

            // Gem frames section
            CreateShopSection(content, "GemFramesSection", "MARCOS DE GEMAS", GEM_COLOR);

            GameObject gemFramesGrid = FindOrCreateChild(content, "GemFramesGrid");
            GridLayoutGroup gfglg = GetOrAddComponent<GridLayoutGroup>(gemFramesGrid);
            gfglg.cellSize = new Vector2(180, 200);
            gfglg.spacing = new Vector2(12, 12);
            gfglg.padding = new RectOffset(10, 10, 5, 10);
            gfglg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gfglg.startAxis = GridLayoutGroup.Axis.Horizontal;
            gfglg.childAlignment = TextAnchor.UpperCenter;
            gfglg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gfglg.constraintCount = 3;

            ContentSizeFitter gfcsf = GetOrAddComponent<ContentSizeFitter>(gemFramesGrid);
            gfcsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            CreateCosmeticCard(gemFramesGrid, "Frame_Sapphire", "Zafiro", "50", GEM_COLOR, "RARO", false);
            CreateCosmeticCard(gemFramesGrid, "Frame_Ruby", "Rubi", "150", GEM_COLOR, "RARO", false);
            CreateCosmeticCard(gemFramesGrid, "Frame_Emerald", "Esmeralda", "300", GEM_COLOR, "EPICO", false);
            CreateCosmeticCard(gemFramesGrid, "Frame_Amethyst", "Amatista", "500", GEM_COLOR, "EPICO", false);
            CreateCosmeticCard(gemFramesGrid, "Frame_Topaz", "Topacio", "750", GEM_COLOR, "EPICO", false);
            CreateCosmeticCard(gemFramesGrid, "Frame_Obsidian", "Obsidiana", "1,000", GEM_COLOR, "LEGENDARIO", true);

            // Premium frames
            CreateShopSection(content, "PremiumFramesSection", "MARCOS PREMIUM", PURPLE_PREMIUM);

            GameObject premFramesGrid = FindOrCreateChild(content, "PremiumFramesGrid");
            GridLayoutGroup pfglg = GetOrAddComponent<GridLayoutGroup>(premFramesGrid);
            pfglg.cellSize = new Vector2(180, 200);
            pfglg.spacing = new Vector2(12, 12);
            pfglg.padding = new RectOffset(10, 10, 5, 10);
            pfglg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            pfglg.startAxis = GridLayoutGroup.Axis.Horizontal;
            pfglg.childAlignment = TextAnchor.UpperCenter;
            pfglg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            pfglg.constraintCount = 3;

            ContentSizeFitter pfcsf = GetOrAddComponent<ContentSizeFitter>(premFramesGrid);
            pfcsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            CreateCosmeticCard(premFramesGrid, "Frame_Holographic", "Holografico", "$1.99", PURPLE_PREMIUM, "EPICO", false);
            CreateCosmeticCard(premFramesGrid, "Frame_AnimatedFire", "Fuego", "$2.99", PURPLE_PREMIUM, "LEGENDARIO", true);
            CreateCosmeticCard(premFramesGrid, "Frame_Crown", "Corona", "$4.99", PURPLE_PREMIUM, "LEGENDARIO", true);

            // === TITULOS Section ===
            CreateShopSection(content, "TitlesSection", "TITULOS", PINK_COSMETIC);

            GameObject titlesGrid = FindOrCreateChild(content, "TitlesGrid");
            GridLayoutGroup tglg = GetOrAddComponent<GridLayoutGroup>(titlesGrid);
            tglg.cellSize = new Vector2(280, 80);
            tglg.spacing = new Vector2(10, 10);
            tglg.padding = new RectOffset(10, 10, 5, 10);
            tglg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            tglg.startAxis = GridLayoutGroup.Axis.Horizontal;
            tglg.childAlignment = TextAnchor.UpperCenter;
            tglg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            tglg.constraintCount = 2;

            ContentSizeFitter tcsf = GetOrAddComponent<ContentSizeFitter>(titlesGrid);
            tcsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Coin titles
            CreateTitleCard(titlesGrid, "Title_Novato", "Novato", "GRATIS", COIN_COLOR);
            CreateTitleCard(titlesGrid, "Title_Jugador", "Jugador", "500", COIN_COLOR);
            CreateTitleCard(titlesGrid, "Title_Veterano", "Veterano", "3,000", COIN_COLOR);
            CreateTitleCard(titlesGrid, "Title_Leyenda", "Leyenda", "10,000", COIN_COLOR);
            CreateTitleCard(titlesGrid, "Title_Inmortal", "Inmortal", "25,000", COIN_COLOR);

            // Gem titles
            CreateTitleCard(titlesGrid, "Title_Estratega", "Estratega", "100", GEM_COLOR);
            CreateTitleCard(titlesGrid, "Title_Genio", "Genio", "300", GEM_COLOR);
            CreateTitleCard(titlesGrid, "Title_Maestro", "Maestro", "600", GEM_COLOR);
            CreateTitleCard(titlesGrid, "Title_Iluminado", "Iluminado", "1,000", GEM_COLOR);

            // Custom title (IAP)
            CreateTitleCard(titlesGrid, "Title_Custom", "Titulo Personalizado", "$1.99", PURPLE_PREMIUM);

            content.transform.parent.parent.gameObject.SetActive(false);

            Debug.Log("[ShopUIBuilder] CosmeticsContent creado con marcos y titulos");
        }

        /// <summary>
        /// Creates a cosmetic card (frame) with preview, name, price, and rarity badge
        /// </summary>
        private static void CreateCosmeticCard(GameObject parent, string name, string displayName, string price, Color priceColor, string rarity, bool isLegendary)
        {
            GameObject card = FindOrCreateChild(parent, name);

            Image cardBg = GetOrAddComponent<Image>(card);
            cardBg.color = CARD_BG;
            AddOutline(card, isLegendary ? GOLD : PINK_COSMETIC * 0.5f, isLegendary ? 2 : 1);

            Button cardBtn = GetOrAddComponent<Button>(card);
            SetupButtonColors(cardBtn, CARD_BG);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(card);
            vlg.spacing = 6;
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Rarity badge
            Color rarityColor = rarity switch
            {
                "LEGENDARIO" => GOLD,
                "EPICO" => PURPLE_PREMIUM,
                "RARO" => GEM_COLOR,
                _ => TEXT_SECONDARY
            };

            GameObject badgeObj = FindOrCreateChild(card, "RarityBadge");
            Image badgeBg = GetOrAddComponent<Image>(badgeObj);
            badgeBg.color = rarityColor;
            LayoutElement badgeLE = GetOrAddComponent<LayoutElement>(badgeObj);
            badgeLE.minHeight = 22;
            badgeLE.preferredHeight = 22;

            GameObject badgeTextObj = FindOrCreateChild(badgeObj, "Text");
            TextMeshProUGUI badgeText = GetOrAddComponent<TextMeshProUGUI>(badgeTextObj);
            badgeText.text = rarity;
            badgeText.fontSize = 14;
            badgeText.fontStyle = FontStyles.Bold;
            badgeText.color = TEXT_DARK;
            badgeText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(badgeTextObj);

            // Frame preview (color swatch)
            GameObject preview = FindOrCreateChild(card, "Preview");
            Image previewImg = GetOrAddComponent<Image>(preview);
            previewImg.color = PINK_COSMETIC * 0.4f;
            LayoutElement previewLE = GetOrAddComponent<LayoutElement>(preview);
            previewLE.minHeight = 60;
            previewLE.preferredHeight = 60;

            // Name
            GameObject nameObj = FindOrCreateChild(card, "Name");
            TextMeshProUGUI nameText = GetOrAddComponent<TextMeshProUGUI>(nameObj);
            nameText.text = displayName;
            nameText.fontSize = 22;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = TEXT_PRIMARY;
            nameText.alignment = TextAlignmentOptions.Center;
            LayoutElement nameLE = GetOrAddComponent<LayoutElement>(nameObj);
            nameLE.minHeight = 26;

            // Price
            GameObject priceObj = FindOrCreateChild(card, "Price");
            TextMeshProUGUI priceText = GetOrAddComponent<TextMeshProUGUI>(priceObj);
            priceText.text = price;
            priceText.fontSize = 24;
            priceText.fontStyle = FontStyles.Bold;
            priceText.color = priceColor;
            priceText.alignment = TextAlignmentOptions.Center;
            LayoutElement priceLE = GetOrAddComponent<LayoutElement>(priceObj);
            priceLE.minHeight = 28;
        }

        /// <summary>
        /// Creates a title card (horizontal, wider)
        /// </summary>
        private static void CreateTitleCard(GameObject parent, string name, string displayName, string price, Color priceColor)
        {
            GameObject card = FindOrCreateChild(parent, name);

            Image cardBg = GetOrAddComponent<Image>(card);
            cardBg.color = CARD_BG;
            AddOutline(card, PINK_COSMETIC * 0.4f);

            Button cardBtn = GetOrAddComponent<Button>(card);
            SetupButtonColors(cardBtn, CARD_BG);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(card);
            hlg.spacing = 10;
            hlg.padding = new RectOffset(12, 12, 8, 8);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            // Title name
            GameObject nameObj = FindOrCreateChild(card, "Name");
            TextMeshProUGUI nameText = GetOrAddComponent<TextMeshProUGUI>(nameObj);
            nameText.text = displayName;
            nameText.fontSize = 24;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = TEXT_PRIMARY;
            nameText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement nameLE = GetOrAddComponent<LayoutElement>(nameObj);
            nameLE.preferredWidth = 160;
            nameLE.flexibleWidth = 1;

            // Price
            GameObject priceObj = FindOrCreateChild(card, "Price");
            TextMeshProUGUI priceText = GetOrAddComponent<TextMeshProUGUI>(priceObj);
            priceText.text = price;
            priceText.fontSize = 22;
            priceText.fontStyle = FontStyles.Bold;
            priceText.color = priceColor;
            priceText.alignment = TextAlignmentOptions.MidlineRight;
            LayoutElement priceLE = GetOrAddComponent<LayoutElement>(priceObj);
            priceLE.preferredWidth = 80;
        }

        // ==================== SHOP SECTION HEADER ====================

        private static void CreateShopSection(GameObject parent, string name, string title, Color color)
        {
            GameObject section = FindOrCreateChild(parent, name);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(section);
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            LayoutElement sectionLE = GetOrAddComponent<LayoutElement>(section);
            sectionLE.minHeight = 40;

            // Icon
            GameObject iconObj = FindOrCreateChild(section, "Icon");
            Image iconImage = GetOrAddComponent<Image>(iconObj);
            iconImage.color = color;
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(iconObj);
            iconLE.minWidth = 30;
            iconLE.minHeight = 30;

            // Title - V2: Font size 28
            GameObject titleObj = FindOrCreateChild(section, "Title");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = title;
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = color;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minWidth = 200;
        }

        // ==================== SHOP ITEM CARD ====================

        private static void CreateShopItem(GameObject parent, string name, string amount, string price, string bonus, Color color, bool isPopular, bool useGems = false)
        {
            GameObject item = FindOrCreateChild(parent, name);

            Image itemBg = GetOrAddComponent<Image>(item);
            itemBg.color = CARD_BG;
            AddOutline(item, isPopular ? GOLD : color * 0.6f, isPopular ? 2 : 1);

            Button itemBtn = GetOrAddComponent<Button>(item);
            SetupButtonColors(itemBtn, CARD_BG);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(item);
            vlg.spacing = 8;
            vlg.padding = new RectOffset(12, 12, 14, 14);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Popular Badge
            if (isPopular)
            {
                GameObject badgeObj = FindOrCreateChild(item, "PopularBadge");
                Image badgeBg = GetOrAddComponent<Image>(badgeObj);
                badgeBg.color = GOLD;

                LayoutElement badgeLE = GetOrAddComponent<LayoutElement>(badgeObj);
                badgeLE.minHeight = 26;
                badgeLE.preferredHeight = 26;

                GameObject badgeTextObj = FindOrCreateChild(badgeObj, "Text");
                TextMeshProUGUI badgeText = GetOrAddComponent<TextMeshProUGUI>(badgeTextObj);
                badgeText.text = "POPULAR";
                badgeText.fontSize = 16;
                badgeText.fontStyle = FontStyles.Bold;
                badgeText.color = TEXT_DARK;
                badgeText.alignment = TextAlignmentOptions.Center;
                SetRectTransformStretch(badgeTextObj);
            }

            // Icon placeholder - V2: Larger
            GameObject iconObj = FindOrCreateChild(item, "Icon");
            Image iconImage = GetOrAddComponent<Image>(iconObj);
            iconImage.color = color;
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(iconObj);
            iconLE.minHeight = 55;
            iconLE.preferredHeight = 55;
            iconLE.minWidth = 55;
            iconLE.preferredWidth = 55;

            // Amount - V2: Font size 36
            GameObject amountObj = FindOrCreateChild(item, "Amount");
            TextMeshProUGUI amountText = GetOrAddComponent<TextMeshProUGUI>(amountObj);
            amountText.text = amount;
            amountText.fontSize = 36;
            amountText.fontStyle = FontStyles.Bold;
            amountText.color = color;
            amountText.alignment = TextAlignmentOptions.Center;
            amountText.enableAutoSizing = true;
            amountText.fontSizeMin = 18;
            amountText.fontSizeMax = 36;
            LayoutElement amountLE = GetOrAddComponent<LayoutElement>(amountObj);
            amountLE.minHeight = 32;

            // Bonus
            if (!string.IsNullOrEmpty(bonus))
            {
                GameObject bonusObj = FindOrCreateChild(item, "Bonus");
                TextMeshProUGUI bonusText = GetOrAddComponent<TextMeshProUGUI>(bonusObj);
                bonusText.text = bonus + " BONUS";
                bonusText.fontSize = 16;
                bonusText.fontStyle = FontStyles.Bold;
                bonusText.color = BUTTON_SUCCESS;
                bonusText.alignment = TextAlignmentOptions.Center;
                LayoutElement bonusLE = GetOrAddComponent<LayoutElement>(bonusObj);
                bonusLE.minHeight = 20;
            }

            // Price Button - V2: Font size 24
            GameObject priceBtn = FindOrCreateChild(item, "PriceButton");
            Image priceBg = GetOrAddComponent<Image>(priceBtn);
            priceBg.color = useGems ? GEM_COLOR : BUTTON_SUCCESS;
            Button priceButton = GetOrAddComponent<Button>(priceBtn);
            SetupButtonColors(priceButton, useGems ? GEM_COLOR : BUTTON_SUCCESS);

            LayoutElement priceLE = GetOrAddComponent<LayoutElement>(priceBtn);
            priceLE.minHeight = 42;
            priceLE.preferredHeight = 42;

            HorizontalLayoutGroup priceHlg = GetOrAddComponent<HorizontalLayoutGroup>(priceBtn);
            priceHlg.spacing = 5;
            priceHlg.padding = new RectOffset(10, 10, 5, 5);
            priceHlg.childAlignment = TextAnchor.MiddleCenter;
            priceHlg.childControlWidth = false;
            priceHlg.childControlHeight = true;

            if (useGems)
            {
                GameObject gemIconObj = FindOrCreateChild(priceBtn, "GemIcon");
                Image gemIconImage = GetOrAddComponent<Image>(gemIconObj);
                gemIconImage.color = TEXT_DARK;
                LayoutElement gemIconLE = GetOrAddComponent<LayoutElement>(gemIconObj);
                gemIconLE.minWidth = 22;
                gemIconLE.minHeight = 22;
            }

            GameObject priceTextObj = FindOrCreateChild(priceBtn, "Text");
            TextMeshProUGUI priceText = GetOrAddComponent<TextMeshProUGUI>(priceTextObj);
            priceText.text = price;
            priceText.fontSize = 24;
            priceText.fontStyle = FontStyles.Bold;
            priceText.color = TEXT_DARK;
            priceText.alignment = TextAlignmentOptions.Center;
            LayoutElement priceTextLE = GetOrAddComponent<LayoutElement>(priceTextObj);
            priceTextLE.flexibleWidth = 1;
        }

        // ==================== PURCHASE CONFIRM POPUP ====================

        private static void CreatePurchaseConfirmPopup(Canvas canvas)
        {
            GameObject blocker = FindOrCreateChild(canvas.gameObject, "PurchaseBlocker");
            blocker.SetActive(false);

            SetRectTransformStretch(blocker);
            Image blockerBg = GetOrAddComponent<Image>(blocker);
            blockerBg.color = BLOCKER_BG;
            Button blockerBtn = GetOrAddComponent<Button>(blocker);
            blockerBtn.transition = Selectable.Transition.None;
            blocker.transform.SetAsLastSibling();

            GameObject popup = FindOrCreateChild(blocker, "PurchasePopup");
            RectTransform popupRT = GetOrAddComponent<RectTransform>(popup);
            popupRT.anchorMin = new Vector2(0.5f, 0.5f);
            popupRT.anchorMax = new Vector2(0.5f, 0.5f);
            popupRT.sizeDelta = new Vector2(500, 400);

            Image popupBg = GetOrAddComponent<Image>(popup);
            popupBg.color = POPUP_BG;
            AddOutline(popup, CYAN_DARK, 2);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(popup);
            vlg.spacing = 20;
            vlg.padding = new RectOffset(30, 30, 30, 30);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Title - V2: Font size 48
            GameObject titleObj = FindOrCreateChild(popup, "Title");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "Confirmar Compra";
            titleText.fontSize = 48;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = CYAN_NEON;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = 24;
            titleText.fontSizeMax = 48;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 50;

            // Item Preview
            GameObject previewObj = FindOrCreateChild(popup, "Preview");
            HorizontalLayoutGroup previewHlg = GetOrAddComponent<HorizontalLayoutGroup>(previewObj);
            previewHlg.spacing = 15;
            previewHlg.childAlignment = TextAnchor.MiddleCenter;
            previewHlg.childControlWidth = false;
            previewHlg.childControlHeight = true;
            LayoutElement previewLE = GetOrAddComponent<LayoutElement>(previewObj);
            previewLE.minHeight = 65;

            GameObject previewIconObj = FindOrCreateChild(previewObj, "Icon");
            Image previewIconImage = GetOrAddComponent<Image>(previewIconObj);
            previewIconImage.color = GEM_COLOR;
            LayoutElement previewIconLE = GetOrAddComponent<LayoutElement>(previewIconObj);
            previewIconLE.minWidth = 55;
            previewIconLE.minHeight = 55;

            GameObject previewAmountObj = FindOrCreateChild(previewObj, "Amount");
            TextMeshProUGUI previewAmountText = GetOrAddComponent<TextMeshProUGUI>(previewAmountObj);
            previewAmountText.text = "1,200 Gemas";
            previewAmountText.fontSize = 30;
            previewAmountText.fontStyle = FontStyles.Bold;
            previewAmountText.color = TEXT_PRIMARY;
            LayoutElement previewAmountLE = GetOrAddComponent<LayoutElement>(previewAmountObj);
            previewAmountLE.minWidth = 250;

            // Price - V2: Font size 36
            GameObject priceObj = FindOrCreateChild(popup, "Price");
            TextMeshProUGUI priceText = GetOrAddComponent<TextMeshProUGUI>(priceObj);
            priceText.text = "Precio: $9.99";
            priceText.fontSize = 36;
            priceText.color = TEXT_SECONDARY;
            priceText.alignment = TextAlignmentOptions.Center;
            LayoutElement priceLE = GetOrAddComponent<LayoutElement>(priceObj);
            priceLE.minHeight = 40;

            // Buttons - V2: Font size 30
            GameObject buttons = FindOrCreateChild(popup, "Buttons");
            HorizontalLayoutGroup btnHlg = GetOrAddComponent<HorizontalLayoutGroup>(buttons);
            btnHlg.spacing = 20;
            btnHlg.childControlWidth = true;
            btnHlg.childControlHeight = true;
            btnHlg.childForceExpandWidth = true;
            LayoutElement btnLE = GetOrAddComponent<LayoutElement>(buttons);
            btnLE.minHeight = 60;

            // Cancel
            GameObject cancelBtn = FindOrCreateChild(buttons, "CancelButton");
            Image cancelBg = GetOrAddComponent<Image>(cancelBtn);
            cancelBg.color = BUTTON_SECONDARY;
            Button cancelButton = GetOrAddComponent<Button>(cancelBtn);
            SetupButtonColors(cancelButton, BUTTON_SECONDARY);
            AddOutline(cancelBtn, CYAN_DARK);

            GameObject cancelTextObj = FindOrCreateChild(cancelBtn, "Text");
            TextMeshProUGUI cancelText = GetOrAddComponent<TextMeshProUGUI>(cancelTextObj);
            cancelText.text = "Cancelar";
            cancelText.fontSize = 30;
            cancelText.fontStyle = FontStyles.Bold;
            cancelText.color = TEXT_PRIMARY;
            cancelText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(cancelTextObj);

            // Confirm
            GameObject confirmBtn = FindOrCreateChild(buttons, "ConfirmButton");
            Image confirmBg = GetOrAddComponent<Image>(confirmBtn);
            confirmBg.color = BUTTON_SUCCESS;
            Button confirmButton = GetOrAddComponent<Button>(confirmBtn);
            SetupButtonColors(confirmButton, BUTTON_SUCCESS);
            AddOutline(confirmBtn, new Color(0.3f, 1f, 0.5f, 0.5f), 2);

            GameObject confirmTextObj = FindOrCreateChild(confirmBtn, "Text");
            TextMeshProUGUI confirmText = GetOrAddComponent<TextMeshProUGUI>(confirmTextObj);
            confirmText.text = "Comprar";
            confirmText.fontSize = 30;
            confirmText.fontStyle = FontStyles.Bold;
            confirmText.color = TEXT_DARK;
            confirmText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(confirmTextObj);

            Debug.Log("[ShopUIBuilder] PurchaseConfirmPopup creado (V2)");
        }

        // ==================== NOT ENOUGH GEMS POPUP ====================

        private static void CreateNotEnoughGemsPopup(Canvas canvas)
        {
            GameObject blocker = FindOrCreateChild(canvas.gameObject, "NotEnoughBlocker");
            blocker.SetActive(false);

            SetRectTransformStretch(blocker);
            Image blockerBg = GetOrAddComponent<Image>(blocker);
            blockerBg.color = BLOCKER_BG;
            Button blockerBtn = GetOrAddComponent<Button>(blocker);
            blockerBtn.transition = Selectable.Transition.None;
            blocker.transform.SetAsLastSibling();

            GameObject popup = FindOrCreateChild(blocker, "NotEnoughPopup");
            RectTransform popupRT = GetOrAddComponent<RectTransform>(popup);
            popupRT.anchorMin = new Vector2(0.5f, 0.5f);
            popupRT.anchorMax = new Vector2(0.5f, 0.5f);
            popupRT.sizeDelta = new Vector2(480, 380);

            Image popupBg = GetOrAddComponent<Image>(popup);
            popupBg.color = POPUP_BG;
            AddOutline(popup, GEM_COLOR, 2);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(popup);
            vlg.spacing = 20;
            vlg.padding = new RectOffset(30, 30, 30, 30);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Icon
            GameObject iconObj = FindOrCreateChild(popup, "Icon");
            Image iconImage = GetOrAddComponent<Image>(iconObj);
            iconImage.color = GEM_COLOR;
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(iconObj);
            iconLE.minHeight = 65;
            iconLE.minWidth = 65;
            iconLE.preferredHeight = 65;
            iconLE.preferredWidth = 65;

            // Title - V2: Font size 42
            GameObject titleObj = FindOrCreateChild(popup, "Title");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "Gemas Insuficientes";
            titleText.fontSize = 42;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GEM_COLOR;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = 22;
            titleText.fontSizeMax = 42;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 45;

            // Message - V2: Font size 24
            GameObject msgObj = FindOrCreateChild(popup, "Message");
            TextMeshProUGUI msgText = GetOrAddComponent<TextMeshProUGUI>(msgObj);
            msgText.text = "No tienes suficientes gemas.\nConsigue mas en la tienda!";
            msgText.fontSize = 24;
            msgText.color = TEXT_SECONDARY;
            msgText.alignment = TextAlignmentOptions.Center;
            LayoutElement msgLE = GetOrAddComponent<LayoutElement>(msgObj);
            msgLE.minHeight = 60;

            // Buttons - V2: Font size 26
            GameObject buttons = FindOrCreateChild(popup, "Buttons");
            HorizontalLayoutGroup btnHlg = GetOrAddComponent<HorizontalLayoutGroup>(buttons);
            btnHlg.spacing = 20;
            btnHlg.childControlWidth = true;
            btnHlg.childControlHeight = true;
            btnHlg.childForceExpandWidth = true;
            LayoutElement btnLE = GetOrAddComponent<LayoutElement>(buttons);
            btnLE.minHeight = 60;

            // Close
            GameObject closeBtn = FindOrCreateChild(buttons, "CloseButton");
            Image closeBg = GetOrAddComponent<Image>(closeBtn);
            closeBg.color = BUTTON_SECONDARY;
            Button closeButton = GetOrAddComponent<Button>(closeBtn);
            SetupButtonColors(closeButton, BUTTON_SECONDARY);

            GameObject closeTextObj = FindOrCreateChild(closeBtn, "Text");
            TextMeshProUGUI closeText = GetOrAddComponent<TextMeshProUGUI>(closeTextObj);
            closeText.text = "Cerrar";
            closeText.fontSize = 26;
            closeText.fontStyle = FontStyles.Bold;
            closeText.color = TEXT_PRIMARY;
            closeText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(closeTextObj);

            // Get Gems
            GameObject getGemsBtn = FindOrCreateChild(buttons, "GetGemsButton");
            Image getGemsBg = GetOrAddComponent<Image>(getGemsBtn);
            getGemsBg.color = GEM_COLOR;
            Button getGemsButton = GetOrAddComponent<Button>(getGemsBtn);
            SetupButtonColors(getGemsButton, GEM_COLOR);
            AddOutline(getGemsBtn, new Color(0.5f, 0.9f, 1f, 0.5f), 2);

            GameObject getGemsTextObj = FindOrCreateChild(getGemsBtn, "Text");
            TextMeshProUGUI getGemsText = GetOrAddComponent<TextMeshProUGUI>(getGemsTextObj);
            getGemsText.text = "Obtener Gemas";
            getGemsText.fontSize = 26;
            getGemsText.fontStyle = FontStyles.Bold;
            getGemsText.color = TEXT_DARK;
            getGemsText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(getGemsTextObj);

            Debug.Log("[ShopUIBuilder] NotEnoughGemsPopup creado (V2)");
        }

        // ==================== UTILITY METHODS ====================

        private static void CleanupOldUI()
        {
            string[] toClean = { "Background", "SafeArea" };
            foreach (var canvas in Object.FindObjectsOfType<Canvas>(true))
            {
                if (canvas.transform.parent != null) continue;
                if (canvas.gameObject.name.Contains("Transition") ||
                    canvas.gameObject.name.Contains("Effects")) continue;
                foreach (string name in toClean)
                {
                    Transform t = canvas.transform.Find(name);
                    if (t != null) Object.DestroyImmediate(t.gameObject);
                }
            }
        }

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
