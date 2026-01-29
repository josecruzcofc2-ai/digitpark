using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Construye la UI completa de Shop (Tienda)
    /// Incluye: SafeArea, Header con CurrencyDisplay, Tabs, Ofertas, Items de compra
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

        private static readonly Color GEM_COLOR = new Color(0.4f, 0.8f, 1f, 1f);
        private static readonly Color COIN_COLOR = new Color(1f, 0.85f, 0.3f, 1f);

        private static readonly Color BLOCKER_BG = new Color(0f, 0f, 0f, 0.85f);

        private static readonly Color TAB_ACTIVE = CYAN_NEON;
        private static readonly Color TAB_INACTIVE = new Color(0.12f, 0.16f, 0.2f, 1f);

        // ==================== DIMENSIONES ====================
        private const float HEADER_HEIGHT = 110f;
        private const float OFFER_BANNER_HEIGHT = 160f;
        private const float TABS_HEIGHT = 55f;
        private const float CONTENT_PADDING = 20f;
        private const float ITEM_HEIGHT = 180f;
        private const float ITEM_WIDTH = 160f;

        [MenuItem("DigitPark/UI Builders/Monetization/Shop", false, 180)]
        public static void BuildUI()
        {
            if (!EditorUtility.DisplayDialog("Shop UI Builder",
                "Esto construira la UI completa de Shop (Tienda).\nAsegurate de tener la escena Shop abierta.\n\nContinuar?",
                "Si", "No"))
                return;

            BuildCompleteUI();
        }

        private static void BuildCompleteUI()
        {
            Debug.Log("[ShopUIBuilder] ========== INICIANDO CONSTRUCCION ==========");

            Canvas canvas = SetupCanvas();
            if (canvas == null) return;

            CreateBackground(canvas);
            GameObject safeArea = CreateSafeArea(canvas);

            CreateHeader(safeArea);
            CreateSpecialOfferBanner(safeArea);
            CreateTabs(safeArea);
            CreateContentArea(safeArea);

            CreatePurchaseConfirmPopup(canvas);
            CreateNotEnoughGemsPopup(canvas);

            MarkSceneDirty();
            Debug.Log("[ShopUIBuilder] ========== CONSTRUCCION COMPLETADA ==========");
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
            titleRT.anchorMin = new Vector2(0, 0.5f);
            titleRT.anchorMax = new Vector2(0, 0.5f);
            titleRT.pivot = new Vector2(0, 0.5f);
            titleRT.anchoredPosition = new Vector2(85, 0);
            titleRT.sizeDelta = new Vector2(200, 50);

            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "TIENDA";
            titleText.fontSize = 36;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = CYAN_NEON;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            AddOutline(titleObj, CYAN_GLOW, 2);

            // Currency Display
            CreateCurrencyDisplay(header);

            Debug.Log("[ShopUIBuilder] Header creado");
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

            // Gems Display
            CreateCurrencyItem(currencyDisplay, "GemsDisplay", "1,250", GEM_COLOR, true);

            // Coins Display
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

            // Make it a button to go to shop
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

            // Amount
            GameObject amountObj = FindOrCreateChild(item, "Amount");
            TextMeshProUGUI amountText = GetOrAddComponent<TextMeshProUGUI>(amountObj);
            amountText.text = amount;
            amountText.fontSize = 18;
            amountText.fontStyle = FontStyles.Bold;
            amountText.color = TEXT_PRIMARY;
            amountText.alignment = TextAlignmentOptions.MidlineLeft;
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

        // ==================== SPECIAL OFFER BANNER ====================

        private static void CreateSpecialOfferBanner(GameObject parent)
        {
            GameObject banner = FindOrCreateChild(parent, "SpecialOfferBanner");

            RectTransform bannerRT = GetOrAddComponent<RectTransform>(banner);
            bannerRT.anchorMin = new Vector2(0, 1);
            bannerRT.anchorMax = new Vector2(1, 1);
            bannerRT.pivot = new Vector2(0.5f, 1);
            bannerRT.anchoredPosition = new Vector2(0, -HEADER_HEIGHT - 10);
            bannerRT.sizeDelta = new Vector2(-CONTENT_PADDING * 2, OFFER_BANNER_HEIGHT);

            Image bannerBg = GetOrAddComponent<Image>(banner);
            bannerBg.color = new Color(0.15f, 0.08f, 0.25f, 1f); // Purple tint
            AddOutline(banner, PURPLE_PREMIUM, 2);

            // Glow effect
            Shadow shadow = GetOrAddComponent<Shadow>(banner);
            shadow.effectColor = new Color(0.6f, 0.3f, 0.9f, 0.4f);
            shadow.effectDistance = new Vector2(0, -4);

            // Layout
            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(banner);
            hlg.spacing = 20;
            hlg.padding = new RectOffset(25, 25, 15, 15);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

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
            badgeText.text = "70% OFF";
            badgeText.fontSize = 16;
            badgeText.fontStyle = FontStyles.Bold;
            badgeText.color = TEXT_PRIMARY;
            badgeText.alignment = TextAlignmentOptions.Center;

            // Title
            GameObject titleObj = FindOrCreateChild(infoPanel, "Title");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "STARTER PACK";
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GOLD;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 35;

            // Contents
            GameObject contentsObj = FindOrCreateChild(infoPanel, "Contents");
            TextMeshProUGUI contentsText = GetOrAddComponent<TextMeshProUGUI>(contentsObj);
            contentsText.text = "500 Gemas + Tema Exclusivo + Avatar Premium";
            contentsText.fontSize = 14;
            contentsText.color = TEXT_SECONDARY;
            contentsText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement contentsLE = GetOrAddComponent<LayoutElement>(contentsObj);
            contentsLE.minHeight = 22;

            // Timer
            GameObject timerObj = FindOrCreateChild(infoPanel, "Timer");
            TextMeshProUGUI timerText = GetOrAddComponent<TextMeshProUGUI>(timerObj);
            timerText.text = "Expira en: 23:45:12";
            timerText.fontSize = 12;
            timerText.color = ORANGE_OFFER;
            timerText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement timerLE = GetOrAddComponent<LayoutElement>(timerObj);
            timerLE.minHeight = 18;

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
            origPriceText.text = "<s>$9.99</s>";
            origPriceText.fontSize = 14;
            origPriceText.color = TEXT_SECONDARY;
            origPriceText.alignment = TextAlignmentOptions.Center;
            LayoutElement origPriceLE = GetOrAddComponent<LayoutElement>(origPriceObj);
            origPriceLE.minHeight = 20;

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
            buyText.text = "$2.99";
            buyText.fontSize = 24;
            buyText.fontStyle = FontStyles.Bold;
            buyText.color = TEXT_DARK;
            buyText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(buyTextObj);

            Debug.Log("[ShopUIBuilder] SpecialOfferBanner creado");
        }

        // ==================== TABS ====================

        private static void CreateTabs(GameObject parent)
        {
            GameObject tabsPanel = FindOrCreateChild(parent, "TabsPanel");

            float topOffset = HEADER_HEIGHT + OFFER_BANNER_HEIGHT + 25;

            RectTransform tabsRT = GetOrAddComponent<RectTransform>(tabsPanel);
            tabsRT.anchorMin = new Vector2(0, 1);
            tabsRT.anchorMax = new Vector2(1, 1);
            tabsRT.pivot = new Vector2(0.5f, 1);
            tabsRT.anchoredPosition = new Vector2(0, -topOffset);
            tabsRT.sizeDelta = new Vector2(-CONTENT_PADDING * 2, TABS_HEIGHT);

            Image tabsBg = GetOrAddComponent<Image>(tabsPanel);
            tabsBg.color = new Color(0.04f, 0.07f, 0.11f, 0.9f);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(tabsPanel);
            hlg.spacing = 8f;
            hlg.padding = new RectOffset(8, 8, 5, 5);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // Tabs
            CreateTab(tabsPanel, "GemsTab", "GEMAS", true, GEM_COLOR);
            CreateTab(tabsPanel, "CoinsTab", "MONEDAS", false, COIN_COLOR);
            CreateTab(tabsPanel, "CosmeticsTab", "TEMAS", false, PURPLE_PREMIUM);
            CreateTab(tabsPanel, "OffersTab", "OFERTAS", false, ORANGE_OFFER);

            Debug.Log("[ShopUIBuilder] Tabs creados");
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
            tabText.fontSize = 14;
            tabText.fontStyle = FontStyles.Bold;
            tabText.color = isActive ? TEXT_DARK : TEXT_PRIMARY;
            tabText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(textObj);

            LayoutElement le = GetOrAddComponent<LayoutElement>(tab);
            le.minHeight = 45;
            le.flexibleWidth = 1;
        }

        // ==================== CONTENT AREA ====================

        private static void CreateContentArea(GameObject parent)
        {
            float topOffset = HEADER_HEIGHT + OFFER_BANNER_HEIGHT + TABS_HEIGHT + 40;

            GameObject scrollView = FindOrCreateChild(parent, "ShopScrollView");

            RectTransform scrollRT = GetOrAddComponent<RectTransform>(scrollView);
            scrollRT.anchorMin = Vector2.zero;
            scrollRT.anchorMax = Vector2.one;
            scrollRT.offsetMin = new Vector2(CONTENT_PADDING, CONTENT_PADDING);
            scrollRT.offsetMax = new Vector2(-CONTENT_PADDING, -topOffset);

            ScrollRect scrollRect = GetOrAddComponent<ScrollRect>(scrollView);
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;

            Image scrollBg = GetOrAddComponent<Image>(scrollView);
            scrollBg.color = Color.clear;

            // Viewport
            GameObject viewport = FindOrCreateChild(scrollView, "Viewport");
            SetRectTransformStretch(viewport);
            RectTransform viewportRT = viewport.GetComponent<RectTransform>();
            GetOrAddComponent<RectMask2D>(viewport);
            scrollRect.viewport = viewportRT;

            // Content
            GameObject content = FindOrCreateChild(viewport, "Content");
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

            // Gems Section
            CreateShopSection(content, "GemsSection", "GEMAS", GEM_COLOR);
            CreateGemsGrid(content);

            // Coins Section
            CreateShopSection(content, "CoinsSection", "MONEDAS", COIN_COLOR);
            CreateCoinsGrid(content);

            Debug.Log("[ShopUIBuilder] Content area creado");
        }

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
            sectionLE.minHeight = 35;

            // Icon
            GameObject iconObj = FindOrCreateChild(section, "Icon");
            Image iconImage = GetOrAddComponent<Image>(iconObj);
            iconImage.color = color;
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(iconObj);
            iconLE.minWidth = 30;
            iconLE.minHeight = 30;

            // Title
            GameObject titleObj = FindOrCreateChild(section, "Title");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = title;
            titleText.fontSize = 22;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = color;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minWidth = 150;
        }

        private static void CreateGemsGrid(GameObject parent)
        {
            GameObject grid = FindOrCreateChild(parent, "GemsGrid");

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

            // Gem Items
            CreateShopItem(grid, "Gems_100", "100", "$0.99", "", GEM_COLOR, false);
            CreateShopItem(grid, "Gems_500", "500", "$4.99", "+10%", GEM_COLOR, false);
            CreateShopItem(grid, "Gems_1200", "1,200", "$9.99", "+20%", GEM_COLOR, true);
            CreateShopItem(grid, "Gems_2500", "2,500", "$19.99", "+25%", GEM_COLOR, false);
            CreateShopItem(grid, "Gems_6500", "6,500", "$49.99", "+30%", GEM_COLOR, false);
            CreateShopItem(grid, "Gems_14000", "14,000", "$99.99", "+35%", GEM_COLOR, false);
        }

        private static void CreateCoinsGrid(GameObject parent)
        {
            GameObject grid = FindOrCreateChild(parent, "CoinsGrid");

            GridLayoutGroup glg = GetOrAddComponent<GridLayoutGroup>(grid);
            glg.cellSize = new Vector2(ITEM_WIDTH, ITEM_HEIGHT);
            glg.spacing = new Vector2(15, 15);
            glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            glg.startAxis = GridLayoutGroup.Axis.Horizontal;
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 3;

            LayoutElement gridLE = GetOrAddComponent<LayoutElement>(grid);
            gridLE.minHeight = ITEM_HEIGHT + 15;

            // Coin Items (bought with gems)
            CreateShopItem(grid, "Coins_1000", "1,000", "50", "", COIN_COLOR, false, true);
            CreateShopItem(grid, "Coins_5000", "5,000", "200", "+25%", COIN_COLOR, false, true);
            CreateShopItem(grid, "Coins_15000", "15,000", "500", "+50%", COIN_COLOR, true, true);
        }

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
            vlg.padding = new RectOffset(10, 10, 12, 12);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Popular Badge (if applicable)
            if (isPopular)
            {
                GameObject badgeObj = FindOrCreateChild(item, "PopularBadge");
                Image badgeBg = GetOrAddComponent<Image>(badgeObj);
                badgeBg.color = GOLD;

                LayoutElement badgeLE = GetOrAddComponent<LayoutElement>(badgeObj);
                badgeLE.minHeight = 22;
                badgeLE.preferredHeight = 22;

                GameObject badgeTextObj = FindOrCreateChild(badgeObj, "Text");
                TextMeshProUGUI badgeText = GetOrAddComponent<TextMeshProUGUI>(badgeTextObj);
                badgeText.text = "POPULAR";
                badgeText.fontSize = 11;
                badgeText.fontStyle = FontStyles.Bold;
                badgeText.color = TEXT_DARK;
                badgeText.alignment = TextAlignmentOptions.Center;
                SetRectTransformStretch(badgeTextObj);
            }

            // Icon placeholder
            GameObject iconObj = FindOrCreateChild(item, "Icon");
            Image iconImage = GetOrAddComponent<Image>(iconObj);
            iconImage.color = color;
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(iconObj);
            iconLE.minHeight = 45;
            iconLE.preferredHeight = 45;
            iconLE.minWidth = 45;
            iconLE.preferredWidth = 45;

            // Amount
            GameObject amountObj = FindOrCreateChild(item, "Amount");
            TextMeshProUGUI amountText = GetOrAddComponent<TextMeshProUGUI>(amountObj);
            amountText.text = amount;
            amountText.fontSize = 22;
            amountText.fontStyle = FontStyles.Bold;
            amountText.color = color;
            amountText.alignment = TextAlignmentOptions.Center;
            LayoutElement amountLE = GetOrAddComponent<LayoutElement>(amountObj);
            amountLE.minHeight = 28;

            // Bonus (if any)
            if (!string.IsNullOrEmpty(bonus))
            {
                GameObject bonusObj = FindOrCreateChild(item, "Bonus");
                TextMeshProUGUI bonusText = GetOrAddComponent<TextMeshProUGUI>(bonusObj);
                bonusText.text = bonus + " BONUS";
                bonusText.fontSize = 11;
                bonusText.fontStyle = FontStyles.Bold;
                bonusText.color = BUTTON_SUCCESS;
                bonusText.alignment = TextAlignmentOptions.Center;
                LayoutElement bonusLE = GetOrAddComponent<LayoutElement>(bonusObj);
                bonusLE.minHeight = 16;
            }

            // Price Button
            GameObject priceBtn = FindOrCreateChild(item, "PriceButton");
            Image priceBg = GetOrAddComponent<Image>(priceBtn);
            priceBg.color = useGems ? GEM_COLOR : BUTTON_SUCCESS;
            Button priceButton = GetOrAddComponent<Button>(priceBtn);
            SetupButtonColors(priceButton, useGems ? GEM_COLOR : BUTTON_SUCCESS);

            LayoutElement priceLE = GetOrAddComponent<LayoutElement>(priceBtn);
            priceLE.minHeight = 38;
            priceLE.preferredHeight = 38;

            HorizontalLayoutGroup priceHlg = GetOrAddComponent<HorizontalLayoutGroup>(priceBtn);
            priceHlg.spacing = 5;
            priceHlg.padding = new RectOffset(10, 10, 5, 5);
            priceHlg.childAlignment = TextAnchor.MiddleCenter;
            priceHlg.childControlWidth = false;
            priceHlg.childControlHeight = true;

            if (useGems)
            {
                // Gem icon for price
                GameObject gemIconObj = FindOrCreateChild(priceBtn, "GemIcon");
                Image gemIconImage = GetOrAddComponent<Image>(gemIconObj);
                gemIconImage.color = TEXT_DARK;
                LayoutElement gemIconLE = GetOrAddComponent<LayoutElement>(gemIconObj);
                gemIconLE.minWidth = 20;
                gemIconLE.minHeight = 20;
            }

            GameObject priceTextObj = FindOrCreateChild(priceBtn, "Text");
            TextMeshProUGUI priceText = GetOrAddComponent<TextMeshProUGUI>(priceTextObj);
            priceText.text = price;
            priceText.fontSize = 16;
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
            popupRT.sizeDelta = new Vector2(450, 350);

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

            // Title
            GameObject titleObj = FindOrCreateChild(popup, "Title");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "Confirmar Compra";
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = CYAN_NEON;
            titleText.alignment = TextAlignmentOptions.Center;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 40;

            // Item Preview
            GameObject previewObj = FindOrCreateChild(popup, "Preview");
            HorizontalLayoutGroup previewHlg = GetOrAddComponent<HorizontalLayoutGroup>(previewObj);
            previewHlg.spacing = 15;
            previewHlg.childAlignment = TextAnchor.MiddleCenter;
            previewHlg.childControlWidth = false;
            previewHlg.childControlHeight = true;
            LayoutElement previewLE = GetOrAddComponent<LayoutElement>(previewObj);
            previewLE.minHeight = 60;

            GameObject previewIconObj = FindOrCreateChild(previewObj, "Icon");
            Image previewIconImage = GetOrAddComponent<Image>(previewIconObj);
            previewIconImage.color = GEM_COLOR;
            LayoutElement previewIconLE = GetOrAddComponent<LayoutElement>(previewIconObj);
            previewIconLE.minWidth = 50;
            previewIconLE.minHeight = 50;

            GameObject previewAmountObj = FindOrCreateChild(previewObj, "Amount");
            TextMeshProUGUI previewAmountText = GetOrAddComponent<TextMeshProUGUI>(previewAmountObj);
            previewAmountText.text = "1,200 Gemas";
            previewAmountText.fontSize = 24;
            previewAmountText.fontStyle = FontStyles.Bold;
            previewAmountText.color = TEXT_PRIMARY;
            LayoutElement previewAmountLE = GetOrAddComponent<LayoutElement>(previewAmountObj);
            previewAmountLE.minWidth = 200;

            // Price
            GameObject priceObj = FindOrCreateChild(popup, "Price");
            TextMeshProUGUI priceText = GetOrAddComponent<TextMeshProUGUI>(priceObj);
            priceText.text = "Precio: $9.99";
            priceText.fontSize = 20;
            priceText.color = TEXT_SECONDARY;
            priceText.alignment = TextAlignmentOptions.Center;
            LayoutElement priceLE = GetOrAddComponent<LayoutElement>(priceObj);
            priceLE.minHeight = 30;

            // Buttons
            GameObject buttons = FindOrCreateChild(popup, "Buttons");
            HorizontalLayoutGroup btnHlg = GetOrAddComponent<HorizontalLayoutGroup>(buttons);
            btnHlg.spacing = 20;
            btnHlg.childControlWidth = true;
            btnHlg.childControlHeight = true;
            btnHlg.childForceExpandWidth = true;
            LayoutElement btnLE = GetOrAddComponent<LayoutElement>(buttons);
            btnLE.minHeight = 55;

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
            cancelText.fontSize = 18;
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
            confirmText.fontSize = 18;
            confirmText.fontStyle = FontStyles.Bold;
            confirmText.color = TEXT_DARK;
            confirmText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(confirmTextObj);

            Debug.Log("[ShopUIBuilder] PurchaseConfirmPopup creado");
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
            popupRT.sizeDelta = new Vector2(420, 320);

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
            iconLE.minHeight = 60;
            iconLE.minWidth = 60;
            iconLE.preferredHeight = 60;
            iconLE.preferredWidth = 60;

            // Title
            GameObject titleObj = FindOrCreateChild(popup, "Title");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "Gemas Insuficientes";
            titleText.fontSize = 26;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GEM_COLOR;
            titleText.alignment = TextAlignmentOptions.Center;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 35;

            // Message
            GameObject msgObj = FindOrCreateChild(popup, "Message");
            TextMeshProUGUI msgText = GetOrAddComponent<TextMeshProUGUI>(msgObj);
            msgText.text = "No tienes suficientes gemas.\nConsigue mas en la tienda!";
            msgText.fontSize = 16;
            msgText.color = TEXT_SECONDARY;
            msgText.alignment = TextAlignmentOptions.Center;
            LayoutElement msgLE = GetOrAddComponent<LayoutElement>(msgObj);
            msgLE.minHeight = 50;

            // Buttons
            GameObject buttons = FindOrCreateChild(popup, "Buttons");
            HorizontalLayoutGroup btnHlg = GetOrAddComponent<HorizontalLayoutGroup>(buttons);
            btnHlg.spacing = 20;
            btnHlg.childControlWidth = true;
            btnHlg.childControlHeight = true;
            btnHlg.childForceExpandWidth = true;
            LayoutElement btnLE = GetOrAddComponent<LayoutElement>(buttons);
            btnLE.minHeight = 55;

            // Close
            GameObject closeBtn = FindOrCreateChild(buttons, "CloseButton");
            Image closeBg = GetOrAddComponent<Image>(closeBtn);
            closeBg.color = BUTTON_SECONDARY;
            Button closeButton = GetOrAddComponent<Button>(closeBtn);
            SetupButtonColors(closeButton, BUTTON_SECONDARY);

            GameObject closeTextObj = FindOrCreateChild(closeBtn, "Text");
            TextMeshProUGUI closeText = GetOrAddComponent<TextMeshProUGUI>(closeTextObj);
            closeText.text = "Cerrar";
            closeText.fontSize = 16;
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
            getGemsText.fontSize = 16;
            getGemsText.fontStyle = FontStyles.Bold;
            getGemsText.color = TEXT_DARK;
            getGemsText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(getGemsTextObj);

            Debug.Log("[ShopUIBuilder] NotEnoughGemsPopup creado");
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
