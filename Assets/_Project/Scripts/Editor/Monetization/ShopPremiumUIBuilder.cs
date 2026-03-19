using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.Editor.AutoAssigners;
using DigitPark.UI;
using DigitPark.Themes;
using ET = DigitPark.Themes.ThemeApplier.ElementType;

namespace DigitPark.Editor
{
    /// <summary>
    /// Shop Premium UI Builder V5 - Clash Royale Professional Style
    /// Continuous scroll — all sections in one view
    /// 2-column grids, real theme previews, clean visual hierarchy
    /// Massive ribbon dividers, generous spacing, zero visual noise
    /// 1080x1920 reference
    /// </summary>
    public class ShopPremiumUIBuilder : EditorWindow
    {
        // ==================== COLORES PREMIUM ====================
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.4f, 1f);

        private static readonly Color DARK_BG = new Color(0.02f, 0.04f, 0.08f, 1f);
        private static readonly Color PANEL_BG = new Color(0.06f, 0.1f, 0.16f, 0.98f);
        private static readonly Color CARD_BG = new Color(0.08f, 0.12f, 0.18f, 1f);
        private static readonly Color HEADER_BG = new Color(0.02f, 0.04f, 0.08f, 0.98f);

        private static readonly Color TEXT_PRIMARY = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.7f, 0.8f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color TEXT_MUTED = new Color(0.4f, 0.5f, 0.6f, 1f);

        private static readonly Color BUTTON_SUCCESS = new Color(0.2f, 0.85f, 0.4f, 1f);
        private static readonly Color BUTTON_SECONDARY = new Color(0.15f, 0.2f, 0.28f, 1f);

        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color PURPLE_PREMIUM = new Color(0.6f, 0.3f, 0.95f, 1f);
        private static readonly Color PURPLE_LIGHT = new Color(0.75f, 0.5f, 1f, 1f);
        private static readonly Color ORANGE_HOT = new Color(1f, 0.45f, 0.1f, 1f);
        private static readonly Color GREEN_FREE = new Color(0.3f, 0.9f, 0.4f, 1f);
        private static readonly Color SILVER = new Color(0.7f, 0.75f, 0.82f, 1f);

        private static readonly Color GEM_COLOR = new Color(0.4f, 0.85f, 1f, 1f);
        private static readonly Color COIN_COLOR = new Color(1f, 0.85f, 0.3f, 1f);

        private static readonly Color FRAME_COLOR = new Color(0.85f, 0.6f, 0.2f, 1f);
        private static readonly Color TITLE_COLOR = new Color(0.9f, 0.75f, 1f, 1f);

        private static readonly Color BADGE_DEAL = new Color(1f, 0.45f, 0.1f, 1f);

        private static readonly Color BLOCKER_BG = new Color(0f, 0f, 0f, 0.9f);

        // ==================== DIMENSIONES V5 ====================
        private const float HEADER_HEIGHT = 100f;
        private const float CONTENT_PADDING = 20f;
        private const float SECTION_SPACING = 44f;

        // Section headers (V5: massive ribbon dividers)
        private const float SECTION_HEADER_HEIGHT = 90f;

        // Banners
        private const float HERO_BANNER_HEIGHT = 260f;
        private const float OFFER_BANNER_HEIGHT = 140f;
        private const float VIP_BANNER_HEIGHT = 180f;
        private const float THEME_BUNDLE_HEIGHT = 150f;

        // Grid cells (V5: spacious cards)
        private const float GRID_CELL_W = 500f;
        private const float GRID_CURRENCY_H = 360f;
        private const float GRID_THEME_H = 360f;
        private const float GRID_COSMETIC_H = 290f;
        private const float GRID_SPACING = 14f;
        private const int GRID_COLUMNS = 2;

        // Title cards
        private const float TITLE_CELL_W = 500f;
        private const float TITLE_CELL_H = 110f;

        // Daily Deals (keep 3 columns)
        private const float DAILY_ITEM_HEIGHT = 210f;

        // Non-font dimensions
        private const float PRICE_BTN_HEIGHT = 50f;
        private const float BADGE_HEIGHT = 32f;

        [MenuItem("DigitPark/Scenes/Build Scene/Monetization/Shop", false, 144)]
        public static void BuildUI()
        {
            if (!EditorUtility.DisplayDialog("Shop Premium UI Builder V5",
                "Esto construira la UI PREMIUM V5 de Shop estilo Clash Royale.\n\n" +
                "Scroll continuo con todas las secciones:\n" +
                "- Hero Banner, Special Offers, Daily Deals\n" +
                "- DigitGems (6), DigitCoins (4)\n" +
                "- 2 Bundle Banners, Themes (15 premium + 4 earnable), Frames (42), Titles (21)\n" +
                "- Victory Effects (10), BattleCards (19)\n" +
                "- VIP Bundle\n\n" +
                "2 columnas, grids anchos.\n" +
                "Asegurate de tener la escena Shop abierta.\n\nContinuar?",
                "Si, Construir", "Cancelar"))
                return;

            BuildCompleteUI();
        }

        /// <summary>Called by AllScenesBatchBuilder — no dialogs.</summary>
        public static void BuildSilent()
        {
            BuildCompleteUI();
        }

        private static void BuildCompleteUI()
        {
            Debug.Log("[ShopPremiumUIBuilder] ========== INICIANDO CONSTRUCCION V5 ==========");

            Canvas canvas = SetupCanvas();
            if (canvas == null) return;

            ClearExistingUI(canvas);

            // Base structure
            CreateBackground(canvas);
            GameObject safeArea = CreateSafeArea(canvas);

            // Header (fixed)
            CreatePremiumHeader(safeArea);

            // Continuous scroll with all sections
            CreateMainScrollContent(safeArea);

            // Popups
            CreatePurchasePopup(canvas);
            CreateNotEnoughPopup(canvas);

            // ShopManager
            AddShopManager(canvas);

            // Force layout rebuild
            foreach (var layout in canvas.GetComponentsInChildren<LayoutGroup>(true))
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
            }

            MarkSceneDirty();
            Debug.Log("[ShopPremiumUIBuilder] ========== CONSTRUCCION V5 COMPLETADA (SCROLL CONTINUO) ==========");

            // Auto-assign references
            AutoAssignReferences();

            if (!AllScenesBatchBuilder.SilentMode)
                EditorUtility.DisplayDialog("Completado",
                    "Shop Premium V5 UI construida (scroll continuo) y referencias asignadas automaticamente!",
                    "OK");
        }

        // ==================== CANVAS & BASE ====================

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

        private static void ClearExistingUI(Canvas canvas)
        {
            for (int i = canvas.transform.childCount - 1; i >= 0; i--)
            {
                var child = canvas.transform.GetChild(i);
                if (child.name != "EventSystem")
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        private static void CreateBackground(Canvas canvas)
        {
            GameObject bg = CreateChild(canvas.gameObject, "Background");
            SetStretch(bg);
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = Color.white; // ThemeApplier tints at runtime
            bgImage.raycastTarget = false;
            bg.transform.SetAsFirstSibling();
            ThemeApplierHelper.Apply(bg, ET.PrimaryBackground);
        }

        private static GameObject CreateSafeArea(Canvas canvas)
        {
            GameObject safeArea = CreateChild(canvas.gameObject, "SafeArea");
            SetStretch(safeArea);
            safeArea.transform.SetSiblingIndex(1);
            return safeArea;
        }

        // ==================== PREMIUM HEADER ====================

        private static void CreatePremiumHeader(GameObject parent)
        {
            GameObject header = CreateChild(parent, "Header");
            RectTransform rt = header.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, -29); // consistent top margin across all scenes
            rt.sizeDelta = new Vector2(0, HEADER_HEIGHT);

            Image headerBg = header.AddComponent<Image>();
            headerBg.color = HEADER_BG;
            headerBg.raycastTarget = false;

            // Back Button (prefab)
            GameObject backPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Common/BackButton.prefab");
            if (backPrefab != null)
            {
                GameObject backBtn = (GameObject)PrefabUtility.InstantiatePrefab(backPrefab, header.transform);
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
                Debug.LogWarning("[ShopPremiumUIBuilder] BackButton.prefab no encontrado en Prefabs/Common/");
                GameObject backBtn = CreateChild(header, "BackButton");
                RectTransform backRT = backBtn.GetComponent<RectTransform>();
                backRT.anchorMin = new Vector2(0, 0.5f);
                backRT.anchorMax = new Vector2(0, 0.5f);
                backRT.pivot = new Vector2(0, 0.5f);
                backRT.anchoredPosition = new Vector2(20, 0);
                backRT.sizeDelta = new Vector2(50, 50);

                Image backBg = backBtn.AddComponent<Image>();
                backBg.color = BUTTON_SECONDARY;
                AddOutline(backBtn, CYAN_DARK, 1);
                Button backButton = backBtn.AddComponent<Button>();
                SetupButton(backButton, BUTTON_SECONDARY);

                GameObject backIcon = CreateChild(backBtn, "Icon");
                SetStretch(backIcon);
                Image backText = backIcon.AddComponent<Image>();
                Sprite arrowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/UI/icon_back_arrow.png");
                if (arrowSprite != null) backText.sprite = arrowSprite;
                backText.color = CYAN_NEON;
                backText.preserveAspect = true;
                backText.raycastTarget = false;
            }

            // Title
            GameObject title = CreateChild(header, "Title");
            RectTransform titleRT = title.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.07f, 0f);
            titleRT.anchorMax = new Vector2(0.53f, 1f);
            titleRT.pivot = new Vector2(0.5f, 0.5f);
            titleRT.sizeDelta = Vector2.zero;
            titleRT.anchoredPosition = Vector2.zero;

            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "SHOP";
            titleText.fontSize = FontSizes.H4;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = CYAN_NEON;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = FontSizes.AutoMinTitle;
            titleText.fontSizeMax = FontSizes.H4;
            titleText.overflowMode = TextOverflowModes.Ellipsis;
            titleText.raycastTarget = false;

            // Currency Pills (same as MainMenu)
            var pills = CurrencyHeaderBarHelper.CreateCurrencyPills(header.transform, "CurrencyDisplay");
            var pillsRT = pills.GetComponent<RectTransform>();
            pillsRT.anchorMin = new Vector2(0.52f, 0.5f);
            pillsRT.anchorMax = new Vector2(0.95f, 0.5f);
            pillsRT.pivot = new Vector2(0.5f, 0.5f);
            pillsRT.sizeDelta = new Vector2(0, 65);

            Debug.Log("[ShopPremiumUIBuilder] Header V5 creado");
        }

        // ==================== MAIN SCROLL (CONTINUOUS) ====================

        /// <summary>
        /// Creates a single ScrollView with all shop sections in continuous scroll.
        /// Order: Hero → Offers → Daily → Gems → Coins → ThemeBundle → Themes → Frames → Titles → VIP
        /// </summary>
        private static void CreateMainScrollContent(GameObject parent)
        {
            GameObject scrollView = CreateChild(parent, "ShopScrollView");
            RectTransform svRT = scrollView.GetComponent<RectTransform>();
            svRT.anchorMin = Vector2.zero;
            svRT.anchorMax = Vector2.one;
            svRT.offsetMin = new Vector2(0, 0);
            svRT.offsetMax = new Vector2(0, -(HEADER_HEIGHT + 29));

            ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;
            scrollRect.decelerationRate = 0.135f;
            scrollRect.scrollSensitivity = 50f;

            // Viewport
            GameObject viewport = CreateChild(scrollView, "Viewport");
            SetStretch(viewport);
            viewport.AddComponent<RectMask2D>();
            scrollRect.viewport = viewport.GetComponent<RectTransform>();

            // Content
            GameObject content = CreateChild(viewport, "Content");
            RectTransform contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.sizeDelta = new Vector2(0, 0);
            scrollRect.content = contentRT;

            // Transparent bg catches raycasts for scrolling
            Image contentBg = content.AddComponent<Image>();
            contentBg.color = Color.clear;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = SECTION_SPACING;
            vlg.padding = new RectOffset((int)CONTENT_PADDING, (int)CONTENT_PADDING, 16, 40);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // === All sections in continuous scroll order ===
            // Featured
            CreateHeroBanner(content);
            CreateSpecialOffersSection(content);
            CreateDailyDealsSection(content);

            // Limited/Rotating Content — whale ceiling expansion (13F)
            CreateLimitedContentSection(content);

            // Currency
            CreateGemsSection(content);
            CreateCoinsSection(content);

            // Styles (bundles first, then themes)
            CreateThemesSection(content);  // Theme bundles are inside this section now
            CreateFramesSection(content);
            CreateTitlesSection(content);

            // Victory Effects (economy rebalance V55)
            CreateEffectsSection(content);

            // BattleCards (economy rebalance V55)
            CreateBattleCardsSection(content);

            // Cosmetic disclaimer (Economy Rebalance V55 — anti-P2W perception)
            CreateCosmeticDisclaimer(content);

            // VIP (bottom)
            CreateVIPBanner(content);

            Debug.Log("[ShopPremiumUIBuilder] Scroll continuo creado con 10 secciones");
        }

        // ==================== FEATURED SECTIONS ====================

        /// <summary>
        /// Welcome Packs section — Economy Rebalance V55
        /// 2 banners: Starter Pack ($2.99, D1-D3) + Premium Welcome ($9.99, D1-D5)
        /// Controlled at runtime by WelcomePackService (visibility, timer, purchase)
        /// </summary>
        private static void CreateHeroBanner(GameObject parent)
        {
            // Container for both packs (WelcomePackUIController manages visibility at runtime)
            GameObject container = CreateChild(parent, "WelcomePacksContainer");
            VerticalLayoutGroup containerVlg = container.AddComponent<VerticalLayoutGroup>();
            containerVlg.spacing = 14;
            containerVlg.childControlWidth = true;
            containerVlg.childControlHeight = true;
            containerVlg.childForceExpandHeight = false;

            // === PACK 1: Starter Pack ($2.99) ===
            CreateWelcomePackBanner(container, "StarterPackBanner",
                "57% OFF", "STARTER PACK",
                "1 Theme + Ruby Frame + Title + 200 DG",
                "Value: 700 DG", "$2.99",
                GOLD, new Color(0.12f, 0.06f, 0.22f, 1f));

            // === PACK 2: Premium Welcome ($9.99) ===
            CreateWelcomePackBanner(container, "PremiumWelcomeBanner",
                "50% OFF", "PREMIUM WELCOME",
                "Aurora Borealis + Holographic + Gold Rain + 500 DG",
                "Value: 1,350 DG", "$9.99",
                PURPLE_PREMIUM, new Color(0.08f, 0.04f, 0.16f, 1f));

            // Add runtime controller
            container.AddComponent<DigitPark.Monetization.WelcomePackUIController>();

            Debug.Log("[ShopPremiumUIBuilder] Welcome Packs V7 created (2 packs with timer)");
        }

        private static void CreateWelcomePackBanner(GameObject parent, string goName,
            string badgeText, string titleStr, string contentsStr,
            string valueStr, string priceStr,
            Color accentColor, Color bgColor)
        {
            GameObject banner = CreateChild(parent, goName);

            LayoutElement le = banner.AddComponent<LayoutElement>();
            le.minHeight = HERO_BANNER_HEIGHT;
            le.preferredHeight = HERO_BANNER_HEIGHT;

            Image bannerBg = banner.AddComponent<Image>();
            bannerBg.color = bgColor;
            AddOutline(banner, accentColor, 3);

            CreateBannerShadow(banner);
            CreateBannerSide(banner, accentColor);

            HorizontalLayoutGroup hlg = banner.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.padding = new RectOffset(24, 24, 20, 20);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            // Left - Icon
            GameObject iconContainer = CreateChild(banner, "IconContainer");
            LayoutElement iconLE = iconContainer.AddComponent<LayoutElement>();
            iconLE.minWidth = 150;
            iconLE.preferredWidth = 150;

            GameObject icon = CreateChild(iconContainer, "Icon");
            RectTransform iconRT = icon.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.sizeDelta = new Vector2(150, 150);
            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = accentColor;

            // Center - Info
            GameObject info = CreateChild(banner, "Info");
            LayoutElement infoLE = info.AddComponent<LayoutElement>();
            infoLE.flexibleWidth = 1;

            VerticalLayoutGroup infoVlg = info.AddComponent<VerticalLayoutGroup>();
            infoVlg.spacing = 6;
            infoVlg.childAlignment = TextAnchor.MiddleLeft;
            infoVlg.childControlWidth = true;
            infoVlg.childControlHeight = true;
            infoVlg.childForceExpandHeight = false;

            CreateInlineBadge(info, badgeText, BADGE_DEAL, 110);

            GameObject title = CreateChild(info, goName + "Title");
            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = titleStr;
            titleText.fontSize = FontSizes.H3;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = accentColor;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = FontSizes.AutoMinTitle;
            titleText.fontSizeMax = FontSizes.H3;
            LayoutElement titleLE = title.AddComponent<LayoutElement>();
            titleLE.minHeight = 44;

            GameObject contents = CreateChild(info, goName + "Contents");
            TextMeshProUGUI contentsText = contents.AddComponent<TextMeshProUGUI>();
            contentsText.text = contentsStr;
            contentsText.fontSize = FontSizes.Body;
            contentsText.fontStyle = FontStyles.Bold;
            contentsText.fontSizeMin = FontSizes.AutoMinBody;
            contentsText.enableAutoSizing = true;
            contentsText.enableWordWrapping = true;
            contentsText.color = TEXT_SECONDARY;
            LayoutElement contentsLE = contents.AddComponent<LayoutElement>();
            contentsLE.minHeight = 36;

            GameObject timer = CreateChild(info, goName + "Timer");
            TextMeshProUGUI timerText = timer.AddComponent<TextMeshProUGUI>();
            timerText.text = "Expires in: --:--:--";
            timerText.fontSize = FontSizes.Body;
            timerText.fontStyle = FontStyles.Bold;
            timerText.fontSizeMin = FontSizes.AutoMinBody;
            timerText.enableAutoSizing = true;
            timerText.color = ORANGE_HOT;
            LayoutElement timerLE = timer.AddComponent<LayoutElement>();
            timerLE.minHeight = 28;

            // Right - Buy
            GameObject buyContainer = CreateChild(banner, "BuyContainer");
            LayoutElement buyContainerLE = buyContainer.AddComponent<LayoutElement>();
            buyContainerLE.minWidth = 150;
            buyContainerLE.preferredWidth = 150;

            VerticalLayoutGroup buyVlg = buyContainer.AddComponent<VerticalLayoutGroup>();
            buyVlg.spacing = 4;
            buyVlg.childAlignment = TextAnchor.MiddleCenter;
            buyVlg.childControlWidth = true;
            buyVlg.childControlHeight = true;
            buyVlg.childForceExpandHeight = false;

            // Value (strikethrough)
            GameObject origPrice = CreateChild(buyContainer, "OriginalPrice");
            TextMeshProUGUI origText = origPrice.AddComponent<TextMeshProUGUI>();
            origText.text = $"<s>{valueStr}</s>";
            origText.fontSize = FontSizes.Body;
            origText.fontStyle = FontStyles.Bold;
            origText.color = TEXT_MUTED;
            origText.alignment = TextAlignmentOptions.Center;
            origText.enableAutoSizing = true;
            origText.fontSizeMin = FontSizes.AutoMinSmall;
            origText.fontSizeMax = FontSizes.Body;
            LayoutElement origLE = origPrice.AddComponent<LayoutElement>();
            origLE.minHeight = 26;

            // Buy button
            CreatePriceButton(buyContainer, priceStr, BUTTON_SUCCESS, TEXT_DARK, 58, FontSizes.Body);
        }

        private static void CreateSpecialOffersSection(GameObject parent)
        {
            GameObject section = CreateChild(parent, "SpecialOffersSection");

            VerticalLayoutGroup vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            CreateSectionDividerV5(section, "SPECIAL OFFERS", ORANGE_HOT);

            // Offer 1 — cleaner V5 banner
            CreateOfferBannerV4(section, "Offer_WeekendGems", "WEEKEND PACK",
                "2,000 DigitGems + 10,000 DigitCoins", "50%", "$4.99", "<s>$9.99</s>",
                new Color(0.08f, 0.12f, 0.25f, 1f), GEM_COLOR);

            // Offer 2
            CreateOfferBannerV4(section, "Offer_MegaCoins", "MEGA DIGITCOINS",
                "50,000 DigitCoins + 3 Frames", "40%", "$2.99", "<s>$5.99</s>",
                new Color(0.15f, 0.1f, 0.05f, 1f), COIN_COLOR);

            Debug.Log("[ShopPremiumUIBuilder] Special Offers V5 creado");
        }

        private static void CreateOfferBannerV4(GameObject parent, string name, string title,
            string contents, string discountPct, string price, string origPrice,
            Color bgColor, Color accentColor)
        {
            GameObject banner = CreateChild(parent, name);

            LayoutElement le = banner.AddComponent<LayoutElement>();
            le.minHeight = OFFER_BANNER_HEIGHT;
            le.preferredHeight = OFFER_BANNER_HEIGHT;

            Image bannerBg = banner.AddComponent<Image>();
            bannerBg.color = bgColor;

            // Accent left stripe (8px)
            GameObject stripe = CreateChild(banner, "AccentStripe");
            RectTransform stripeRT = stripe.GetComponent<RectTransform>();
            stripeRT.anchorMin = new Vector2(0, 0);
            stripeRT.anchorMax = new Vector2(0, 1);
            stripeRT.pivot = new Vector2(0, 0.5f);
            stripeRT.anchoredPosition = Vector2.zero;
            stripeRT.sizeDelta = new Vector2(8, 0);
            Image stripeImg = stripe.AddComponent<Image>();
            stripeImg.color = accentColor;
            stripeImg.raycastTarget = false;
            LayoutElement stripeLE = stripe.AddComponent<LayoutElement>();
            stripeLE.ignoreLayout = true;

            HorizontalLayoutGroup hlg = banner.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 16;
            hlg.padding = new RectOffset(24, 20, 14, 14);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            // Info (no icon section in V4)
            GameObject info = CreateChild(banner, "Info");
            LayoutElement infoLE = info.AddComponent<LayoutElement>();
            infoLE.flexibleWidth = 1;

            VerticalLayoutGroup infoVlg = info.AddComponent<VerticalLayoutGroup>();
            infoVlg.spacing = 4;
            infoVlg.childAlignment = TextAnchor.MiddleLeft;
            infoVlg.childControlWidth = true;
            infoVlg.childControlHeight = true;
            infoVlg.childForceExpandHeight = false;

            GameObject titleObj = CreateChild(info, "Title");
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = title;
            titleText.fontSize = FontSizes.Body;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = TEXT_PRIMARY;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = FontSizes.AutoMinSmall;
            titleText.fontSizeMax = FontSizes.Body;
            LayoutElement titleLE = titleObj.AddComponent<LayoutElement>();
            titleLE.minHeight = 36;

            GameObject contentsObj = CreateChild(info, "Contents");
            TextMeshProUGUI contentsText = contentsObj.AddComponent<TextMeshProUGUI>();
            contentsText.text = contents;
            contentsText.fontSize = FontSizes.BodySmall;
            contentsText.fontStyle = FontStyles.Bold;
            contentsText.color = TEXT_SECONDARY;
            contentsText.enableAutoSizing = true;
            contentsText.fontSizeMin = FontSizes.AutoMinSmall;
            contentsText.fontSizeMax = FontSizes.BodySmall;
            LayoutElement contentsLE = contentsObj.AddComponent<LayoutElement>();
            contentsLE.minHeight = 24;

            // Buy area
            GameObject buyContainer = CreateChild(banner, "BuyContainer");
            LayoutElement buyContainerLE = buyContainer.AddComponent<LayoutElement>();
            buyContainerLE.minWidth = 140;
            buyContainerLE.preferredWidth = 140;

            VerticalLayoutGroup buyVlg = buyContainer.AddComponent<VerticalLayoutGroup>();
            buyVlg.spacing = 2;
            buyVlg.childAlignment = TextAnchor.MiddleCenter;
            buyVlg.childControlWidth = true;
            buyVlg.childControlHeight = true;
            buyVlg.childForceExpandHeight = false;

            // Discount badge
            CreateInlineBadge(buyContainer, discountPct + " OFF", BADGE_DEAL, 100);

            GameObject origObj = CreateChild(buyContainer, "OriginalPrice");
            TextMeshProUGUI origText = origObj.AddComponent<TextMeshProUGUI>();
            origText.text = origPrice;
            origText.fontSize = FontSizes.Caption;
            origText.fontStyle = FontStyles.Bold;
            origText.color = TEXT_MUTED;
            origText.alignment = TextAlignmentOptions.Center;
            origText.enableAutoSizing = true;
            origText.fontSizeMin = FontSizes.AutoMinSmall;
            origText.fontSizeMax = FontSizes.Caption;
            LayoutElement origLE = origObj.AddComponent<LayoutElement>();
            origLE.minHeight = 22;

            CreatePriceButton(buyContainer, price, BUTTON_SUCCESS, TEXT_DARK, 48, FontSizes.Body);
        }

        private static void CreateDailyDealsSection(GameObject parent)
        {
            GameObject section = CreateChild(parent, "DailyDealsSection");

            VerticalLayoutGroup vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Ribbon divider (same style as other sections)
            CreateSectionDividerV5(section, "DAILY OFFERS", ORANGE_HOT);

            // Timer row (below divider)
            GameObject timerObj = CreateChild(section, "Timer");
            LayoutElement timerLE = timerObj.AddComponent<LayoutElement>();
            timerLE.minHeight = 34;
            timerLE.preferredHeight = 34;
            TextMeshProUGUI timerText = timerObj.AddComponent<TextMeshProUGUI>();
            timerText.text = "12:34:56";
            timerText.fontSize = FontSizes.BodySmall;
            timerText.fontStyle = FontStyles.Bold;
            timerText.color = TEXT_SECONDARY;
            timerText.alignment = TextAlignmentOptions.Center;
            timerText.enableAutoSizing = true;
            timerText.fontSizeMin = FontSizes.AutoMinSmall;
            timerText.fontSizeMax = FontSizes.BodySmall;

            // Items container (3 columns for daily deals)
            GameObject itemsContainer = CreateChild(section, "Items");
            LayoutElement itemsLE = itemsContainer.AddComponent<LayoutElement>();
            itemsLE.minHeight = DAILY_ITEM_HEIGHT;

            HorizontalLayoutGroup itemsHlg = itemsContainer.AddComponent<HorizontalLayoutGroup>();
            itemsHlg.spacing = GRID_SPACING;
            itemsHlg.childAlignment = TextAnchor.MiddleCenter;
            itemsHlg.childControlWidth = true;
            itemsHlg.childControlHeight = true;
            itemsHlg.childForceExpandWidth = true;

            // 3 Daily Items — V4: only "FREE" badge or nothing
            CreateDailyItem(itemsContainer, "Daily_Free", "200 DigitGems", "FREE", GEM_COLOR, true);
            CreateDailyItem(itemsContainer, "Daily_Gems", "25 DigitGems", "100", GEM_COLOR, false);
            CreateDailyItem(itemsContainer, "Daily_Coins", "5,000 DigitCoins", "50", COIN_COLOR, false);

            // Add DailyOfferUIController component to connect with DailyOfferService at runtime
            section.AddComponent<DigitPark.Monetization.DailyOfferUIController>();

            Debug.Log("[ShopPremiumUIBuilder] Daily Deals V7 created (connected to DailyOfferService)");
        }

        private static void CreateDailyItem(GameObject parent, string name, string itemName,
            string price, Color iconColor, bool isFree)
        {
            GameObject item = CreateChild(parent, name);

            Image itemBg = item.AddComponent<Image>();
            itemBg.color = CARD_BG;
            AddOutline(item, isFree ? GREEN_FREE : iconColor * 0.5f, isFree ? 3 : 1);

            Shadow itemShadow = item.AddComponent<Shadow>();
            itemShadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            itemShadow.effectDistance = new Vector2(3, -4);

            Button btn = item.AddComponent<Button>();
            SetupButton(btn, CARD_BG);

            VerticalLayoutGroup vlg = item.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.padding = new RectOffset(12, 12, 12, 12);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Badge — V4: only FREE badge
            if (isFree)
            {
                CreateInlineBadge(item, "Free", GREEN_FREE, 0, "FreeBadgeText");
            }

            // Icon
            GameObject icon = CreateChild(item, "Icon");
            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = iconColor;
            iconImg.raycastTarget = false;
            LayoutElement iconLE = icon.AddComponent<LayoutElement>();
            iconLE.minHeight = 55;
            iconLE.preferredHeight = 55;
            iconLE.minWidth = 55;
            iconLE.preferredWidth = 55;

            // Name
            GameObject nameObj = CreateChild(item, "Name");
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = itemName;
            nameText.fontSize = FontSizes.Body;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = TEXT_PRIMARY;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.enableAutoSizing = true;
            nameText.fontSizeMin = FontSizes.AutoMinSmall;
            nameText.fontSizeMax = FontSizes.Body;
            LayoutElement nameLE = nameObj.AddComponent<LayoutElement>();
            nameLE.minHeight = 30;

            // Price button
            Color priceBtnColor = isFree ? GREEN_FREE : GEM_COLOR;
            CreatePriceButton(item, price, priceBtnColor, TEXT_DARK, 46, FontSizes.Body);
        }

        private static void CreateVIPBanner(GameObject parent)
        {
            GameObject section = CreateChild(parent, "VIPBanner");

            LayoutElement sectionLE = section.AddComponent<LayoutElement>();
            sectionLE.minHeight = VIP_BANNER_HEIGHT;
            sectionLE.preferredHeight = VIP_BANNER_HEIGHT;

            Image sectionBg = section.AddComponent<Image>();
            sectionBg.color = new Color(0.1f, 0.05f, 0.18f, 1f);
            AddOutline(section, GOLD, 3);

            CreateBannerShadow(section);
            CreateBannerSide(section, GOLD);

            HorizontalLayoutGroup hlg = section.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.padding = new RectOffset(24, 24, 18, 18);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            // Icon (text-based star glyph, no sprite needed)
            GameObject iconContainer = CreateChild(section, "IconContainer");
            LayoutElement iconLE = iconContainer.AddComponent<LayoutElement>();
            iconLE.minWidth = 90;
            iconLE.preferredWidth = 90;

            GameObject icon = CreateChild(iconContainer, "Icon");
            RectTransform iconRT = icon.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.sizeDelta = new Vector2(75, 75);
            TextMeshProUGUI iconText = icon.AddComponent<TextMeshProUGUI>();
            iconText.text = "<size=60><color=#FFD700>*</color></size>";
            iconText.fontSize = FontSizes.H2;
            iconText.fontStyle = FontStyles.Bold;
            iconText.color = GOLD;
            iconText.alignment = TextAlignmentOptions.Center;
            iconText.raycastTarget = false;

            // Info
            GameObject info = CreateChild(section, "Info");
            LayoutElement infoLE = info.AddComponent<LayoutElement>();
            infoLE.flexibleWidth = 1;

            VerticalLayoutGroup infoVlg = info.AddComponent<VerticalLayoutGroup>();
            infoVlg.spacing = 5;
            infoVlg.childAlignment = TextAnchor.MiddleLeft;
            infoVlg.childControlHeight = true;
            infoVlg.childForceExpandHeight = false;

            CreateInlineBadge(info, "VIP", GOLD, 65);

            GameObject titleObj = CreateChild(info, "BundlePremiumTitle");
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "BUNDLE PREMIUM";
            titleText.fontSize = FontSizes.Body;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GOLD;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = FontSizes.AutoMinBody;
            titleText.fontSizeMax = FontSizes.Body;
            titleText.enableWordWrapping = false;
            titleText.overflowMode = TextOverflowModes.Ellipsis;
            LayoutElement titleLE = titleObj.AddComponent<LayoutElement>();
            titleLE.minHeight = 40;

            GameObject desc = CreateChild(info, "BundlePremiumDesc");
            TextMeshProUGUI descText = desc.AddComponent<TextMeshProUGUI>();
            descText.text = "50 levels of exclusive rewards";
            descText.fontSize = FontSizes.BodySmall;
            descText.fontStyle = FontStyles.Bold;
            descText.fontSizeMin = FontSizes.AutoMinSmall;
            descText.fontSizeMax = FontSizes.BodySmall;
            descText.enableAutoSizing = true;
            descText.enableWordWrapping = true;
            descText.overflowMode = TextOverflowModes.Ellipsis;
            descText.color = TEXT_SECONDARY;
            LayoutElement descLE = desc.AddComponent<LayoutElement>();
            descLE.minHeight = 36;
            descLE.flexibleHeight = 1;

            // Buy
            GameObject buyContainer = CreateChild(section, "BuyContainer");
            LayoutElement buyContainerLE = buyContainer.AddComponent<LayoutElement>();
            buyContainerLE.minWidth = 130;

            GameObject buyBtn = CreateChild(buyContainer, "BuyButton");
            RectTransform buyRT = buyBtn.GetComponent<RectTransform>();
            buyRT.anchorMin = new Vector2(0.5f, 0.5f);
            buyRT.anchorMax = new Vector2(0.5f, 0.5f);
            buyRT.sizeDelta = new Vector2(120, 55);

            Image buyBg = buyBtn.AddComponent<Image>();
            buyBg.color = PURPLE_PREMIUM;
            Button buyButton = buyBtn.AddComponent<Button>();
            SetupButton(buyButton, PURPLE_PREMIUM);
            AddOutline(buyBtn, PURPLE_LIGHT, 2);

            GameObject buyText = CreateChild(buyBtn, "Text");
            SetStretch(buyText);
            TextMeshProUGUI buyTxt = buyText.AddComponent<TextMeshProUGUI>();
            buyTxt.text = "$9.99";
            buyTxt.fontSize = FontSizes.Body;
            buyTxt.fontStyle = FontStyles.Bold;
            buyTxt.color = TEXT_PRIMARY;
            buyTxt.alignment = TextAlignmentOptions.Center;
            buyTxt.enableAutoSizing = true;
            buyTxt.fontSizeMin = FontSizes.AutoMinSmall;
            buyTxt.fontSizeMax = FontSizes.Body;

            Debug.Log("[ShopPremiumUIBuilder] VIP Banner V5 creado");
        }

        // ==================== CURRENCY SECTIONS ====================

        private static void CreateGemsSection(GameObject parent)
        {
            GameObject section = CreateChild(parent, "GemsSection");

            VerticalLayoutGroup vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            CreateSectionDividerV5(section, "DIGITGEMS", GEM_COLOR);

            // Grid — V4: 2 columns, 500x280
            GameObject grid = CreateChild(section, "GemsGrid");

            GridLayoutGroup glg = grid.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(GRID_CELL_W, GRID_CURRENCY_H);
            glg.spacing = new Vector2(GRID_SPACING, GRID_SPACING);
            glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            glg.startAxis = GridLayoutGroup.Axis.Horizontal;
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = GRID_COLUMNS;

            LayoutElement gridLE = grid.AddComponent<LayoutElement>();
            gridLE.minHeight = GRID_CURRENCY_H * 4 + GRID_SPACING * 3; // 7 packs in 2-col = 4 rows

            // 7 DigitGem packs — Economy Rebalance V55: added $1.99 Mini pack
            CreateCurrencyCardV4(grid, "Gems_100", "100", "$0.99", "", GEM_COLOR, "", false);
            CreateCurrencyCardV4(grid, "Gems_220", "220", "$1.99", "+10%", GEM_COLOR, "NEW", false);
            CreateCurrencyCardV4(grid, "Gems_500", "500", "$4.99", "+10%", GEM_COLOR, "", false);
            CreateCurrencyCardV4(grid, "Gems_1200", "1,200", "$9.99", "+20%", GEM_COLOR, "BEST VALUE", false);
            CreateCurrencyCardV4(grid, "Gems_2500", "2,500", "$19.99", "+25%", GEM_COLOR, "", false);
            CreateCurrencyCardV4(grid, "Gems_6500", "6,500", "$49.99", "+30%", GEM_COLOR, "POPULAR", false);
            CreateCurrencyCardV4(grid, "Gems_14000", "14,000", "$99.99", "+35%", GEM_COLOR, "", false);

            Debug.Log("[ShopPremiumUIBuilder] DigitGems Section V7 created (7 packs incl. $1.99 Mini)");
        }

        private static void CreateCoinsSection(GameObject parent)
        {
            GameObject section = CreateChild(parent, "CoinsSection");

            VerticalLayoutGroup vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            CreateSectionDividerV5(section, "DIGITCOINS", COIN_COLOR);

            // Grid — V4: 2 columns
            GameObject grid = CreateChild(section, "CoinsGrid");

            GridLayoutGroup glg = grid.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(GRID_CELL_W, GRID_CURRENCY_H);
            glg.spacing = new Vector2(GRID_SPACING, GRID_SPACING);
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = GRID_COLUMNS;

            LayoutElement gridLE = grid.AddComponent<LayoutElement>();
            gridLE.minHeight = GRID_CURRENCY_H * 2 + GRID_SPACING;

            // 4 DigitCoin packs (bought with DigitGems)
            CreateCurrencyCardV4(grid, "Coins_1000", "1,000", "50", "", COIN_COLOR, "", true);
            CreateCurrencyCardV4(grid, "Coins_5000", "5,000", "200", "+25%", COIN_COLOR, "", true);
            CreateCurrencyCardV4(grid, "Coins_15000", "15,000", "500", "+50%", COIN_COLOR, "BEST VALUE", true);
            CreateCurrencyCardV4(grid, "Coins_50000", "50,000", "1,500", "+75%", COIN_COLOR, "POPULAR", true);

            Debug.Log("[ShopPremiumUIBuilder] DigitCoins Section V5 creado");
        }

        private static void CreateCurrencyCardV4(GameObject parent, string name, string amount,
            string price, string bonus, Color color, string badge, bool useGems)
        {
            GameObject item = CreateChild(parent, name);

            Image itemBg = item.AddComponent<Image>();
            itemBg.color = CARD_BG;

            bool hasBadge = !string.IsNullOrEmpty(badge);
            Color outlineColor = color * 0.5f;
            int outlineWidth = 1;

            if (badge == "BEST VALUE")
            {
                outlineColor = GREEN_FREE;
                outlineWidth = 3;
            }
            else if (badge == "POPULAR")
            {
                outlineColor = GOLD;
                outlineWidth = 3;
            }

            AddOutline(item, outlineColor, outlineWidth);

            Shadow itemShadow = item.AddComponent<Shadow>();
            itemShadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            itemShadow.effectDistance = new Vector2(3, -4);

            Button btn = item.AddComponent<Button>();
            SetupButton(btn, CARD_BG);

            VerticalLayoutGroup vlg = item.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.padding = new RectOffset(24, 24, 20, 20);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Badge — V5: only BEST VALUE or POPULAR
            if (hasBadge)
            {
                Color badgeColor = badge == "BEST VALUE" ? GREEN_FREE : GOLD;
                CreateInlineBadge(item, badge, badgeColor, 0);
            }

            // Icon (110x110 centered)
            GameObject icon = CreateChild(item, "Icon");
            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = color;
            iconImg.raycastTarget = false;
            LayoutElement iconLE = icon.AddComponent<LayoutElement>();
            iconLE.minHeight = 110;
            iconLE.preferredHeight = 110;
            iconLE.minWidth = 110;
            iconLE.preferredWidth = 110;

            // Amount (Subtitle, bold)
            GameObject amountObj = CreateChild(item, "Amount");
            TextMeshProUGUI amountText = amountObj.AddComponent<TextMeshProUGUI>();
            amountText.text = amount;
            amountText.fontSize = FontSizes.Subtitle;
            amountText.fontStyle = FontStyles.Bold;
            amountText.color = color;
            amountText.alignment = TextAlignmentOptions.Center;
            amountText.enableAutoSizing = true;
            amountText.fontSizeMin = FontSizes.AutoMinSmall;
            amountText.fontSizeMax = FontSizes.Subtitle;
            LayoutElement amountLE = amountObj.AddComponent<LayoutElement>();
            amountLE.minHeight = 44;

            // Bonus (BodySmall, green, only if >0%)
            if (!string.IsNullOrEmpty(bonus))
            {
                GameObject bonusObj = CreateChild(item, "Bonus");
                TextMeshProUGUI bonusText = bonusObj.AddComponent<TextMeshProUGUI>();
                bonusText.text = bonus + " BONUS";
                bonusText.fontSize = FontSizes.BodySmall;
                bonusText.fontStyle = FontStyles.Bold;
                bonusText.color = BUTTON_SUCCESS;
                bonusText.alignment = TextAlignmentOptions.Center;
                bonusText.enableAutoSizing = true;
                bonusText.fontSizeMin = FontSizes.AutoMinSmall;
                bonusText.fontSizeMax = FontSizes.BodySmall;
                LayoutElement bonusLE = bonusObj.AddComponent<LayoutElement>();
                bonusLE.minHeight = 22;
            }

            // Price button (full-width, 50px)
            Color priceBtnColor = useGems ? GEM_COLOR : BUTTON_SUCCESS;
            GameObject priceBtn = CreateChild(item, "PriceButton");
            Image priceBg = priceBtn.AddComponent<Image>();
            priceBg.color = priceBtnColor;
            AddOutline(priceBtn, priceBtnColor * 1.2f, 1);
            LayoutElement priceLE = priceBtn.AddComponent<LayoutElement>();
            priceLE.minHeight = PRICE_BTN_HEIGHT;
            priceLE.preferredHeight = PRICE_BTN_HEIGHT;

            HorizontalLayoutGroup priceHlg = priceBtn.AddComponent<HorizontalLayoutGroup>();
            priceHlg.spacing = 6;
            priceHlg.padding = new RectOffset(14, 14, 6, 6);
            priceHlg.childAlignment = TextAnchor.MiddleCenter;
            priceHlg.childControlWidth = false;
            priceHlg.childControlHeight = true;

            if (useGems)
            {
                GameObject gemIcon = CreateChild(priceBtn, "GemIcon");
                Image gemImg = gemIcon.AddComponent<Image>();
                gemImg.color = TEXT_DARK;
                LayoutElement gemLE = gemIcon.AddComponent<LayoutElement>();
                gemLE.minWidth = 22;
                gemLE.minHeight = 22;
            }

            GameObject priceText = CreateChild(priceBtn, "Text");
            TextMeshProUGUI pt = priceText.AddComponent<TextMeshProUGUI>();
            pt.text = price;
            pt.fontSize = FontSizes.Body;
            pt.fontStyle = FontStyles.Bold;
            pt.color = TEXT_DARK;
            pt.alignment = TextAlignmentOptions.Center;
            pt.enableAutoSizing = true;
            pt.fontSizeMin = FontSizes.AutoMinSmall;
            pt.fontSizeMax = FontSizes.Body;
            LayoutElement ptLE = priceText.AddComponent<LayoutElement>();
            ptLE.flexibleWidth = 1;
        }

        // ==================== STYLES SECTIONS ====================

        private static void CreateThemeBundleBanners(GameObject parent)
        {
            // === Premium Bundle Banner (19 premium themes, DG pricing — economy rebalance V55) ===
            // Total: 5S×400 + 8A×250 + 6B×150 = 4,900 DG → 30% off = 3,430 DG
            CreateBundleBanner(parent, "PremiumBundleBanner",
                "PREMIUM BUNDLE", "shop_premium_bundle", "shop_premium_bundle_desc",
                "19 premium themes", "<s>4,900 DG</s>", "3,430 DG",
                GOLD, new Color(0.12f, 0.08f, 0.02f, 1f));

            // Complete Bundle ELIMINATED — earnable themes are trophies, not purchasable

            Debug.Log("[ShopPremiumUIBuilder] Premium Bundle Banner created (DG pricing, Complete Bundle removed)");
        }

        private static void CreateBundleBanner(GameObject parent, string goName,
            string headerText, string titleKey, string descKey,
            string descFallback, string origPrice, string salePrice,
            Color accentColor, Color bgColor)
        {
            GameObject banner = CreateChild(parent, goName);

            LayoutElement le = banner.AddComponent<LayoutElement>();
            le.minHeight = THEME_BUNDLE_HEIGHT;
            le.preferredHeight = THEME_BUNDLE_HEIGHT;

            Image bannerBg = banner.AddComponent<Image>();
            bannerBg.color = bgColor;
            AddOutline(banner, accentColor, 3);

            CreateBannerShadow(banner);
            CreateBannerSide(banner, accentColor);

            HorizontalLayoutGroup hlg = banner.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 16;
            hlg.padding = new RectOffset(24, 24, 18, 18);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            // Info
            GameObject info = CreateChild(banner, "Info");
            LayoutElement infoLE = info.AddComponent<LayoutElement>();
            infoLE.flexibleWidth = 1;

            VerticalLayoutGroup infoVlg = info.AddComponent<VerticalLayoutGroup>();
            infoVlg.spacing = 5;
            infoVlg.childAlignment = TextAnchor.MiddleLeft;
            infoVlg.childControlWidth = true;
            infoVlg.childControlHeight = true;
            infoVlg.childForceExpandHeight = false;

            CreateInlineBadge(info, "SAVE 30%", BADGE_DEAL, 120);

            GameObject title = CreateChild(info, goName + "Title");
            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = headerText;
            titleText.fontSize = FontSizes.Subtitle;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = accentColor;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = FontSizes.AutoMinTitle;
            titleText.fontSizeMax = FontSizes.Subtitle;
            LayoutElement titleLE = title.AddComponent<LayoutElement>();
            titleLE.minHeight = 40;

            GameObject desc = CreateChild(info, goName + "Desc");
            TextMeshProUGUI descText = desc.AddComponent<TextMeshProUGUI>();
            descText.text = descFallback;
            descText.fontSize = FontSizes.Body;
            descText.fontStyle = FontStyles.Bold;
            descText.color = TEXT_SECONDARY;
            descText.enableAutoSizing = true;
            descText.fontSizeMin = FontSizes.AutoMinSmall;
            descText.fontSizeMax = FontSizes.Body;
            LayoutElement descLE = desc.AddComponent<LayoutElement>();
            descLE.minHeight = 26;

            // Buy
            GameObject buyContainer = CreateChild(banner, "BuyContainer");
            LayoutElement buyContainerLE = buyContainer.AddComponent<LayoutElement>();
            buyContainerLE.minWidth = 140;
            buyContainerLE.preferredWidth = 140;

            VerticalLayoutGroup buyVlg = buyContainer.AddComponent<VerticalLayoutGroup>();
            buyVlg.spacing = 2;
            buyVlg.childAlignment = TextAnchor.MiddleCenter;
            buyVlg.childControlWidth = true;
            buyVlg.childControlHeight = true;
            buyVlg.childForceExpandHeight = false;

            GameObject origObj = CreateChild(buyContainer, "OriginalPrice");
            TextMeshProUGUI origText = origObj.AddComponent<TextMeshProUGUI>();
            origText.text = origPrice;
            origText.fontSize = FontSizes.Caption;
            origText.fontStyle = FontStyles.Bold;
            origText.color = TEXT_MUTED;
            origText.alignment = TextAlignmentOptions.Center;
            origText.enableAutoSizing = true;
            origText.fontSizeMin = FontSizes.AutoMinSmall;
            origText.fontSizeMax = origText.fontSize;
            LayoutElement origLE = origObj.AddComponent<LayoutElement>();
            origLE.minHeight = 22;

            CreatePriceButton(buyContainer, salePrice, BUTTON_SUCCESS, TEXT_DARK, 52, FontSizes.Body);
        }

        private static void CreateThemesSection(GameObject parent)
        {
            GameObject section = CreateChild(parent, "ThemesSection");

            VerticalLayoutGroup vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Theme bundle banners at the top of themes section
            CreateThemeBundleBanners(section);

            // === Premium Themes — DG pricing by tier (Economy Rebalance V55) ===
            // Tier S = 400 DG, Tier A = 250 DG, Tier B = 150 DG
            CreateSectionDividerV5(section, "PREMIUM THEMES", PURPLE_PREMIUM);

            GameObject premiumGrid = CreateChild(section, "PremiumThemesGrid");
            GridLayoutGroup pGlg = premiumGrid.AddComponent<GridLayoutGroup>();
            pGlg.cellSize = new Vector2(GRID_CELL_W, GRID_THEME_H);
            pGlg.spacing = new Vector2(GRID_SPACING, GRID_SPACING);
            pGlg.childAlignment = TextAnchor.UpperCenter;
            pGlg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            pGlg.constraintCount = GRID_COLUMNS;
            LayoutElement pGridLE = premiumGrid.AddComponent<LayoutElement>();
            pGridLE.minHeight = GRID_THEME_H * 10 + GRID_SPACING * 9; // 19 themes in 2-col = 10 rows

            // ── TIER S (400 DG) — Top visual appeal ──
            CreateThemeCardV4(premiumGrid, "Theme_AuroraBorealis", "Aurora Borealis", "400 DG",
                new Color(0.02f, 0.06f, 0.12f, 1f),
                new Color(0.1f, 0.9f, 0.6f, 1f), new Color(0.2f, 0.5f, 0.9f, 1f), new Color(0.7f, 0.95f, 0.8f, 1f),
                false, false);
            CreateThemeCardV4(premiumGrid, "Theme_Nebula", "Nebula", "400 DG",
                new Color(0.047f, 0.031f, 0.086f, 1f),
                new Color(0.545f, 0.361f, 0.965f, 1f), new Color(0.925f, 0.282f, 0.6f, 1f), new Color(0.769f, 0.71f, 0.992f, 1f),
                false, false);
            CreateThemeCardV4(premiumGrid, "Theme_IceFire", "Ice x Fire", "400 DG",
                new Color(0.04f, 0.06f, 0.1f, 1f),
                new Color(0.2f, 0.7f, 1f, 1f), new Color(1f, 0.3f, 0.1f, 1f), new Color(0.85f, 0.9f, 1f, 1f),
                false, false);
            CreateThemeCardV4(premiumGrid, "Theme_CyberFuchsia", "Cyber Fuchsia", "400 DG",
                new Color(0.102f, 0.039f, 0.102f, 1f),
                new Color(0.851f, 0.275f, 0.937f, 1f), new Color(0.91f, 0.475f, 0.976f, 1f), new Color(0.961f, 0.816f, 0.996f, 1f),
                false, false);
            CreateThemeCardV4(premiumGrid, "Theme_Vaporwave", "Vaporwave", "400 DG",
                new Color(0.102f, 0.063f, 0.031f, 1f),
                new Color(0.984f, 0.573f, 0.235f, 1f), new Color(0.984f, 0.749f, 0.141f, 1f), new Color(0.992f, 0.902f, 0.541f, 1f),
                false, false);

            // ── TIER A (250 DG) — Strong visual appeal ──
            CreateThemeCardV4(premiumGrid, "Theme_Glitch", "Glitch", "250 DG",
                new Color(0.102f, 0.039f, 0.039f, 1f),
                new Color(0.937f, 0.267f, 0.267f, 1f), new Color(0.976f, 0.451f, 0.086f, 1f), new Color(0.988f, 0.647f, 0.647f, 1f),
                false, false);
            CreateThemeCardV4(premiumGrid, "Theme_Bioluminescence", "Bioluminescence", "250 DG",
                new Color(0.102f, 0.059f, 0.078f, 1f),
                new Color(0.957f, 0.447f, 0.714f, 1f), new Color(0.925f, 0.282f, 0.6f, 1f), new Color(0.984f, 0.812f, 0.91f, 1f),
                false, false);
            CreateThemeCardV4(premiumGrid, "Theme_Volcanic", "Volcanic", "250 DG",
                new Color(0.102f, 0.047f, 0.031f, 1f),
                new Color(0.957f, 0.247f, 0.369f, 1f), new Color(0.984f, 0.573f, 0.235f, 1f), new Color(0.988f, 0.647f, 0.647f, 1f),
                false, false);
            CreateThemeCardV4(premiumGrid, "Theme_Matrix", "Matrix", "250 DG",
                new Color(0.031f, 0.059f, 0.031f, 1f),
                new Color(0.133f, 0.773f, 0.369f, 1f), new Color(0.29f, 0.871f, 0.502f, 1f), new Color(0.733f, 0.969f, 0.816f, 1f),
                false, false);
            CreateThemeCardV4(premiumGrid, "Theme_Infrared", "Infrared", "250 DG",
                new Color(0.102f, 0.031f, 0.031f, 1f),
                new Color(0.882f, 0.114f, 0.282f, 1f), new Color(0.984f, 0.443f, 0.522f, 1f), new Color(1f, 0.894f, 0.902f, 1f),
                false, false);
            CreateThemeCardV4(premiumGrid, "Theme_BloodMoon", "Blood Moon", "250 DG",
                new Color(0.08f, 0.02f, 0.02f, 1f),
                new Color(0.8f, 0.1f, 0.15f, 1f), new Color(0.6f, 0.05f, 0.1f, 1f), new Color(0.95f, 0.5f, 0.5f, 1f),
                false, false);
            CreateThemeCardV4(premiumGrid, "Theme_Phantom", "Phantom", "250 DG",
                new Color(0.031f, 0.024f, 0.055f, 1f),
                new Color(0.486f, 0.227f, 0.929f, 1f), new Color(0.427f, 0.157f, 0.851f, 1f), new Color(0.655f, 0.545f, 0.98f, 1f),
                false, false);
            CreateThemeCardV4(premiumGrid, "Theme_Ultraviolet", "Ultraviolet", "250 DG",
                new Color(0.063f, 0.075f, 0.102f, 1f),
                new Color(0.58f, 0.639f, 0.722f, 1f), new Color(0.796f, 0.835f, 0.882f, 1f), new Color(0.886f, 0.91f, 0.941f, 1f),
                false, false);

            // ── TIER B (150 DG) — Entry-level premium ──
            CreateThemeCardV4(premiumGrid, "Theme_PlasmaIndigo", "Plasma Indigo", "150 DG",
                new Color(0.039f, 0.039f, 0.118f, 1f),
                new Color(0.388f, 0.4f, 0.945f, 1f), new Color(0.506f, 0.549f, 0.973f, 1f), new Color(0.78f, 0.824f, 0.996f, 1f),
                false, false);
            CreateThemeCardV4(premiumGrid, "Theme_Arctic", "Arctic", "150 DG",
                new Color(0.047f, 0.098f, 0.161f, 1f),
                new Color(0.22f, 0.741f, 0.973f, 1f), new Color(0.49f, 0.827f, 0.988f, 1f), new Color(0.878f, 0.949f, 0.996f, 1f),
                false, false);
            CreateThemeCardV4(premiumGrid, "Theme_DeepOcean", "Deep Ocean", "150 DG",
                new Color(0.039f, 0.082f, 0.125f, 1f),
                new Color(0.078f, 0.722f, 0.651f, 1f), new Color(0.176f, 0.831f, 0.749f, 1f), new Color(0.6f, 0.965f, 0.894f, 1f),
                false, false);
            CreateThemeCardV4(premiumGrid, "Theme_CoralSurge", "Coral Surge", "150 DG",
                new Color(0.102f, 0.047f, 0.063f, 1f),
                new Color(0.984f, 0.443f, 0.522f, 1f), new Color(0.992f, 0.643f, 0.686f, 1f), new Color(1f, 0.894f, 0.902f, 1f),
                false, false);
            CreateThemeCardV4(premiumGrid, "Theme_ToxicLime", "Toxic Lime", "150 DG",
                new Color(0.047f, 0.102f, 0.031f, 1f),
                new Color(0.518f, 0.8f, 0.086f, 1f), new Color(0.639f, 0.902f, 0.208f, 1f), new Color(0.851f, 0.976f, 0.616f, 1f),
                false, false);
            CreateThemeCardV4(premiumGrid, "Theme_ElectricOrange", "Electric Orange", "150 DG",
                new Color(0.1f, 0.05f, 0.01f, 1f),
                new Color(1f, 0.5f, 0f, 1f), new Color(1f, 0.7f, 0.2f, 1f), new Color(1f, 0.9f, 0.6f, 1f),
                false, false);

            // === Earnable Themes — TROPHY ONLY (not purchasable, achievement-locked) ===
            CreateSectionDividerV5(section, "EARNABLE THEMES (TROPHY)", SILVER);

            GameObject earnableGrid = CreateChild(section, "EarnableThemesGrid");
            GridLayoutGroup eGlg = earnableGrid.AddComponent<GridLayoutGroup>();
            eGlg.cellSize = new Vector2(GRID_CELL_W, GRID_THEME_H);
            eGlg.spacing = new Vector2(GRID_SPACING, GRID_SPACING);
            eGlg.childAlignment = TextAnchor.UpperCenter;
            eGlg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            eGlg.constraintCount = GRID_COLUMNS;
            LayoutElement eGridLE = earnableGrid.AddComponent<LayoutElement>();
            eGridLE.minHeight = GRID_THEME_H * 2 + GRID_SPACING;

            // 4 Earnable themes — TROPHY ONLY, NOT purchasable (economy rebalance V55)
            CreateThemeCardV4(earnableGrid, "Theme_Emerald", "Emerald", "365d LOGIN",
                new Color(0.039f, 0.102f, 0.078f, 1f),
                new Color(0.063f, 0.725f, 0.506f, 1f), new Color(0.204f, 0.827f, 0.6f, 1f), new Color(0.655f, 0.953f, 0.816f, 1f),
                false, false);
            CreateThemeCardV4(earnableGrid, "Theme_ElectricBlue", "Electric Blue", "1,000 WINS",
                new Color(0.031f, 0.063f, 0.118f, 1f),
                new Color(0.231f, 0.51f, 0.965f, 1f), new Color(0.376f, 0.647f, 0.98f, 1f), new Color(0.749f, 0.859f, 0.996f, 1f),
                false, false);
            CreateThemeCardV4(earnableGrid, "Theme_ElectricViolet", "Electric Violet", "100 PERFECTS",
                new Color(0.059f, 0.039f, 0.102f, 1f),
                new Color(0.659f, 0.333f, 0.969f, 1f), new Color(0.753f, 0.518f, 0.988f, 1f), new Color(0.914f, 0.835f, 1f, 1f),
                false, false);
            CreateThemeCardV4(earnableGrid, "Theme_Monochrome", "Monochrome", "LEVEL 50",
                new Color(0.094f, 0.094f, 0.106f, 1f),
                new Color(0.82f, 0.835f, 0.859f, 1f), new Color(0.976f, 0.98f, 0.984f, 1f), new Color(1f, 1f, 1f, 1f),
                false, false);

            Debug.Log("[ShopPremiumUIBuilder] Themes Section V7 created (19 premium DG-priced: 5S+8A+6B + 4 earnable trophy-only)");
        }

        private static void CreateThemeCardV4(GameObject parent, string name, string displayName,
            string price, Color bgColor,
            Color accent1, Color accent2, Color accent3,
            bool isEquipped, bool isLocked)
        {
            GameObject item = CreateChild(parent, name);

            Image itemBg = item.AddComponent<Image>();
            itemBg.color = CARD_BG;
            AddOutline(item, isEquipped ? accent1 : accent1 * 0.4f, isEquipped ? 3 : 1);

            Shadow itemShadow = item.AddComponent<Shadow>();
            itemShadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            itemShadow.effectDistance = new Vector2(3, -4);

            Button btn = item.AddComponent<Button>();
            SetupButton(btn, CARD_BG);

            VerticalLayoutGroup vlg = item.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.padding = new RectOffset(16, 16, 14, 14);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Swatch area (160px) — real color preview
            GameObject swatchArea = CreateChild(item, "SwatchArea");
            Image swatchBg = swatchArea.AddComponent<Image>();
            swatchBg.color = bgColor;
            swatchBg.raycastTarget = false;
            LayoutElement swatchLE = swatchArea.AddComponent<LayoutElement>();
            swatchLE.minHeight = 160;
            swatchLE.preferredHeight = 160;

            // 3 accent dots (centered in swatch)
            GameObject dotsRow = CreateChild(swatchArea, "AccentDots");
            RectTransform dotsRT = dotsRow.GetComponent<RectTransform>();
            dotsRT.anchorMin = new Vector2(0.5f, 0.5f);
            dotsRT.anchorMax = new Vector2(0.5f, 0.5f);
            dotsRT.sizeDelta = new Vector2(130, 32);

            HorizontalLayoutGroup dotsHlg = dotsRow.AddComponent<HorizontalLayoutGroup>();
            dotsHlg.spacing = 12;
            dotsHlg.childAlignment = TextAnchor.MiddleCenter;
            dotsHlg.childControlWidth = false;
            dotsHlg.childControlHeight = false;

            CreateAccentDot(dotsRow, "Accent1", accent1);
            CreateAccentDot(dotsRow, "Accent2", accent2);
            CreateAccentDot(dotsRow, "Accent3", accent3);

            // 4px accent stripe
            GameObject accentStripe = CreateChild(item, "AccentStripe");
            Image accentStripeImg = accentStripe.AddComponent<Image>();
            accentStripeImg.color = accent1;
            accentStripeImg.raycastTarget = false;
            LayoutElement accentStripeLE = accentStripe.AddComponent<LayoutElement>();
            accentStripeLE.preferredHeight = 4;

            // Theme name
            GameObject nameObj = CreateChild(item, "Name");
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = displayName;
            nameText.fontSize = FontSizes.Body;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = accent1;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.enableAutoSizing = true;
            nameText.fontSizeMin = FontSizes.AutoMinSmall;
            nameText.fontSizeMax = FontSizes.Body;
            LayoutElement nameLE = nameObj.AddComponent<LayoutElement>();
            nameLE.minHeight = 30;

            // Price/Status
            if (isEquipped)
            {
                GameObject statusObj = CreateChild(item, "PurchasedBadgeText");
                TextMeshProUGUI statusText = statusObj.AddComponent<TextMeshProUGUI>();
                statusText.text = "Purchased";
                statusText.fontSize = FontSizes.Body;
                statusText.fontStyle = FontStyles.Bold;
                statusText.color = TEXT_SECONDARY;
                statusText.alignment = TextAlignmentOptions.Center;
                statusText.enableAutoSizing = true;
                statusText.fontSizeMin = FontSizes.AutoMinBody;
                statusText.fontSizeMax = statusText.fontSize;
                LayoutElement statusLE = statusObj.AddComponent<LayoutElement>();
                statusLE.minHeight = PRICE_BTN_HEIGHT;
            }
            else
            {
                CreatePriceButton(item, price, BUTTON_SUCCESS, TEXT_DARK, PRICE_BTN_HEIGHT, FontSizes.Body);
            }

            // Lock overlay
            if (isLocked)
            {
                GameObject lockOverlay = CreateChild(item, "LockOverlay");
                SetStretch(lockOverlay);
                Image lockImg = lockOverlay.AddComponent<Image>();
                lockImg.color = new Color(0f, 0f, 0f, 0.7f);
                lockImg.raycastTarget = false;
                LayoutElement lockLE = lockOverlay.AddComponent<LayoutElement>();
                lockLE.ignoreLayout = true;
            }
        }

        private static void CreateAccentDot(GameObject parent, string name, Color color)
        {
            GameObject dot = CreateChild(parent, name);
            RectTransform dotRT = dot.GetComponent<RectTransform>();
            dotRT.sizeDelta = new Vector2(40, 40);
            Image dotImg = dot.AddComponent<Image>();
            dotImg.color = color;
            dotImg.raycastTarget = false;
        }

        private static void CreateFramesSection(GameObject parent)
        {
            GameObject section = CreateChild(parent, "FramesSection");

            VerticalLayoutGroup vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            CreateSectionDividerV5(section, "FRAMES", FRAME_COLOR);

            // Grid — V4: 2 columns, merged all 17 frames
            GameObject grid = CreateChild(section, "FramesGrid");

            GridLayoutGroup glg = grid.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(GRID_CELL_W, GRID_COSMETIC_H);
            glg.spacing = new Vector2(GRID_SPACING, GRID_SPACING);
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = GRID_COLUMNS;

            LayoutElement gridLE = grid.AddComponent<LayoutElement>();
            gridLE.minHeight = GRID_COSMETIC_H * 21 + GRID_SPACING * 20; // 42 items in 2-col = 21 rows

            // 8 Coin frames — precios corregidos (fuente de verdad: PlayerFrameService)
            CreateCosmeticCardV4(grid, "Frame_Basic",    "Basic",    "EQUIPPED", new Color(0.5f, 0.5f, 0.5f, 1f),    true,  "coin");
            CreateCosmeticCardV4(grid, "Frame_Bronze",   "Bronze",   "1,000",    new Color(0.8f, 0.5f, 0.2f, 1f),    false, "coin");
            CreateCosmeticCardV4(grid, "Frame_Silver",   "Silver",   "2,500",    new Color(0.75f, 0.75f, 0.8f, 1f),  false, "coin");
            CreateCosmeticCardV4(grid, "Frame_Gold",     "Gold",     "5,000",    GOLD,                                false, "coin");
            CreateCosmeticCardV4(grid, "Frame_Neon",     "Neon",     "7,500",    new Color(0f, 1f, 0.5f, 1f),        false, "coin");
            CreateCosmeticCardV4(grid, "Frame_Diamond",  "Diamond",  "10,000",   new Color(0.7f, 0.9f, 1f, 1f),      false, "coin");
            CreateCosmeticCardV4(grid, "Frame_Crystal",  "Crystal",  "12,000",   new Color(0.7f, 1f, 1f, 1f),        false, "coin");
            CreateCosmeticCardV4(grid, "Frame_Platinum", "Platinum", "15,000",   new Color(0.9f, 0.95f, 1f, 1f),     false, "coin");

            // 6 Gem frames — precios corregidos
            CreateCosmeticCardV4(grid, "GemFrame_Sapphire",  "Sapphire",  "50",    new Color(0.05f, 0.2f, 0.8f, 1f),  false, "gem");
            CreateCosmeticCardV4(grid, "GemFrame_Ruby",      "Ruby",      "150",   new Color(0.9f, 0.1f, 0.2f, 1f),   false, "gem");
            CreateCosmeticCardV4(grid, "GemFrame_Emerald",   "Emerald",   "300",   new Color(0.1f, 0.8f, 0.3f, 1f),   false, "gem");
            CreateCosmeticCardV4(grid, "GemFrame_Amethyst",  "Amethyst",  "500",   PURPLE_LIGHT,                       false, "gem");
            CreateCosmeticCardV4(grid, "GemFrame_Topaz",     "Topaz",     "750",   new Color(0.8f, 0.53f, 0f, 1f),    false, "gem");
            CreateCosmeticCardV4(grid, "GemFrame_Obsidian",  "Obsidian",  "1,000", new Color(0.1f, 0.1f, 0.15f, 1f),  false, "gem");

            // 12 Real Money frames — orden por precio
            CreateCosmeticCardV4(grid, "PremFrame_PlasmaSparkA",    "Plasma Spark",     "$0.99",  new Color(0f, 0.75f, 1f, 1f),      false, "real");
            CreateCosmeticCardV4(grid, "PremFrame_PrismShift",      "Prism Shift",      "$0.99",  new Color(1f, 0.5f, 0f, 1f),       false, "real");
            CreateCosmeticCardV4(grid, "PremFrame_Holographic",     "Holographic",      "$1.99",  new Color(1f, 1f, 1f, 1f),         false, "real");
            CreateCosmeticCardV4(grid, "PremFrame_QuantumFire",     "Quantum Fire",     "$2.99",  new Color(0f, 0.53f, 1f, 1f),      false, "real");
            CreateCosmeticCardV4(grid, "PremFrame_AuroraBorealis",  "Aurora Borealis",  "$3.99",  new Color(0f, 1f, 0.8f, 1f),       false, "real");
            CreateCosmeticCardV4(grid, "PremFrame_LegendaryCrown",  "Legendary Crown",  "$4.99",  new Color(0.18f, 0f, 0.35f, 1f),   false, "real");
            CreateCosmeticCardV4(grid, "PremFrame_VoidWalker",      "Void Walker",      "$5.99",  new Color(0.02f, 0.02f, 0.19f, 1f), false, "real");
            CreateCosmeticCardV4(grid, "PremFrame_StormSurge",      "Storm Surge",      "$5.99",  new Color(0.06f, 0.1f, 0.24f, 1f),  false, "real");
            CreateCosmeticCardV4(grid, "PremFrame_CosmicRift",      "Cosmic Rift",      "$9.99",  new Color(0.04f, 0f, 0.08f, 1f),    false, "real");
            CreateCosmeticCardV4(grid, "PremFrame_InfernalGod",     "Infernal God",     "$9.99",  new Color(0.1f, 0f, 0f, 1f),        false, "real");
            CreateCosmeticCardV4(grid, "PremFrame_DivineLight",     "Divine Light",     "$14.99", GOLD,                               false, "real");
            CreateCosmeticCardV4(grid, "PremFrame_QuantumBreak",    "Quantum Break",    "$14.99", new Color(0f, 1f, 0.26f, 1f),       false, "real");

            // 5 Achievement frames (shown as locked)
            CreateCosmeticCardV4(grid, "Frame_FirstWin",        "First Win",        "LOCKED",  GOLD,                               false, "earn");
            CreateCosmeticCardV4(grid, "Frame_Centurion",       "Centurion",        "LOCKED",  new Color(0.7f, 0.3f, 0.1f, 1f),   false, "earn");
            CreateCosmeticCardV4(grid, "Frame_Master",          "Master",           "LOCKED",  new Color(0.9f, 0.2f, 0.3f, 1f),   false, "earn");
            CreateCosmeticCardV4(grid, "Frame_SocialButterfly", "Social Butterfly", "LOCKED",  new Color(1f, 0.5f, 0.8f, 1f),     false, "earn");
            CreateCosmeticCardV4(grid, "Frame_StreakKing",      "Streak King",      "LOCKED",  ORANGE_HOT,                         false, "earn");

            // 3 Secret frames (shown as locked with ??? hint)
            CreateCosmeticCardV4(grid, "Frame_NightOwl",        "???",              "LOCKED",  new Color(0.2f, 0.1f, 0.4f, 1f),   false, "earn");
            CreateCosmeticCardV4(grid, "Frame_PerfectFrame",    "???",              "LOCKED",  CYAN_NEON,                          false, "earn");
            CreateCosmeticCardV4(grid, "Frame_SpeedDemon",      "???",              "LOCKED",  new Color(1f, 0.3f, 0f, 1f),       false, "earn");

            Debug.Log("[ShopPremiumUIBuilder] Frames Section V7 created (34 purchasable + 8 earn/secret = 42 total)");
        }

        private static void CreateCosmeticCardV4(GameObject parent, string name, string displayName,
            string price, Color itemColor, bool isEquipped, string priceType)
        {
            GameObject item = CreateChild(parent, name);

            Image itemBg = item.AddComponent<Image>();
            itemBg.color = CARD_BG;
            AddOutline(item, isEquipped ? itemColor : itemColor * 0.4f, isEquipped ? 2 : 1);

            Shadow itemShadow = item.AddComponent<Shadow>();
            itemShadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            itemShadow.effectDistance = new Vector2(3, -4);

            Button btn = item.AddComponent<Button>();
            SetupButton(btn, CARD_BG);

            VerticalLayoutGroup vlg = item.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.padding = new RectOffset(16, 16, 14, 14);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            if (isEquipped)
            {
                CreateInlineBadge(item, "Purchased", itemColor, 0, "PurchasedBadgeText");
            }

            // Icon/Preview
            GameObject icon = CreateChild(item, "Icon");
            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = itemColor;
            iconImg.raycastTarget = false;
            LayoutElement iconLE = icon.AddComponent<LayoutElement>();
            iconLE.minHeight = 80;
            iconLE.preferredHeight = 80;
            iconLE.minWidth = 80;
            iconLE.preferredWidth = 80;

            // Name
            GameObject nameObj = CreateChild(item, "Name");
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = displayName;
            nameText.fontSize = FontSizes.Body;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = itemColor;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.enableAutoSizing = true;
            nameText.fontSizeMin = FontSizes.AutoMinSmall;
            nameText.fontSizeMax = FontSizes.Body;
            LayoutElement nameLE = nameObj.AddComponent<LayoutElement>();
            nameLE.minHeight = 30;

            // Price — V4: color differentiates price type
            if (isEquipped)
            {
                GameObject statusObj = CreateChild(item, "PurchasedBadgeText");
                TextMeshProUGUI statusText = statusObj.AddComponent<TextMeshProUGUI>();
                statusText.text = "Purchased";
                statusText.fontSize = FontSizes.Body;
                statusText.fontStyle = FontStyles.Bold;
                statusText.color = TEXT_SECONDARY;
                statusText.alignment = TextAlignmentOptions.Center;
                statusText.enableAutoSizing = true;
                statusText.fontSizeMin = FontSizes.AutoMinBody;
                statusText.fontSizeMax = statusText.fontSize;
                LayoutElement statusLE = statusObj.AddComponent<LayoutElement>();
                statusLE.minHeight = 40;
            }
            else
            {
                Color btnColor;
                bool showGemIcon = false;
                switch (priceType)
                {
                    case "gem": btnColor = GEM_COLOR; showGemIcon = true; break;
                    case "real": btnColor = BUTTON_SUCCESS; break;
                    default: btnColor = COIN_COLOR; break; // coin
                }

                GameObject priceBtn = CreateChild(item, "PriceButton");
                Image priceBg = priceBtn.AddComponent<Image>();
                priceBg.color = btnColor;
                LayoutElement priceLE = priceBtn.AddComponent<LayoutElement>();
                priceLE.minHeight = 42;
                priceLE.preferredHeight = 42;

                HorizontalLayoutGroup priceHlg = priceBtn.AddComponent<HorizontalLayoutGroup>();
                priceHlg.spacing = 5;
                priceHlg.padding = new RectOffset(12, 12, 5, 5);
                priceHlg.childAlignment = TextAnchor.MiddleCenter;
                priceHlg.childControlWidth = false;
                priceHlg.childControlHeight = true;

                if (showGemIcon)
                {
                    GameObject gemIcon = CreateChild(priceBtn, "GemIcon");
                    Image gemImg = gemIcon.AddComponent<Image>();
                    gemImg.color = TEXT_DARK;
                    LayoutElement gemLE = gemIcon.AddComponent<LayoutElement>();
                    gemLE.minWidth = 20;
                    gemLE.minHeight = 20;
                }

                GameObject priceTextObj = CreateChild(priceBtn, "Text");
                TextMeshProUGUI pt = priceTextObj.AddComponent<TextMeshProUGUI>();
                pt.text = price;
                pt.fontSize = FontSizes.Body;
                pt.fontStyle = FontStyles.Bold;
                pt.color = TEXT_DARK;
                pt.alignment = TextAlignmentOptions.Center;
                pt.enableAutoSizing = true;
                pt.fontSizeMin = FontSizes.AutoMinBody;
                pt.fontSizeMax = pt.fontSize;
                LayoutElement ptLE = priceTextObj.AddComponent<LayoutElement>();
                ptLE.flexibleWidth = 1;
            }
        }

        private static void CreateTitlesSection(GameObject parent)
        {
            GameObject section = CreateChild(parent, "TitlesSection");

            VerticalLayoutGroup vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            CreateSectionDividerV5(section, "TITLES", TITLE_COLOR);

            // Grid — V4: 2 columns, 90px cells
            GameObject grid = CreateChild(section, "TitlesGrid");

            GridLayoutGroup glg = grid.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(TITLE_CELL_W, TITLE_CELL_H);
            glg.spacing = new Vector2(GRID_SPACING, GRID_SPACING);
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = GRID_COLUMNS;

            LayoutElement gridLE = grid.AddComponent<LayoutElement>();
            gridLE.minHeight = TITLE_CELL_H * 11 + GRID_SPACING * 10; // 21 items in 2-col = 11 rows

            // DC Titles (4) + Free (1)
            CreateTitleCardV4(grid, "Title_Novato",     "Novato",         "EQUIPPED",  TEXT_SECONDARY,                     true);
            CreateTitleCardV4(grid, "Title_Jugador",    "Jugador",        "500",       new Color(0.6f, 0.8f, 0.6f, 1f),   false);
            CreateTitleCardV4(grid, "Title_Veterano",   "Veterano",       "3,000",     new Color(0.8f, 0.7f, 0.3f, 1f),   false);
            CreateTitleCardV4(grid, "Title_Leyenda",    "Leyenda",        "10,000",    PURPLE_LIGHT,                       false);
            CreateTitleCardV4(grid, "Title_Inmortal",   "Inmortal",       "25,000",    GOLD,                               false);

            // DG Titles (4)
            CreateTitleCardV4(grid, "Title_Estratega",  "Estratega",      "100",       GEM_COLOR,                          false);
            CreateTitleCardV4(grid, "Title_Genio",      "Genio",          "300",       new Color(0.5f, 1f, 0.8f, 1f),     false);
            CreateTitleCardV4(grid, "Title_Maestro",    "Maestro",        "600",       ORANGE_HOT,                         false);
            CreateTitleCardV4(grid, "Title_Iluminado",  "Iluminado",      "1,000",     new Color(1f, 0.9f, 0.4f, 1f),     false);

            // IAP Title (1)
            CreateTitleCardV4(grid, "Title_Custom",     "Custom Title",   "$1.99",     new Color(1f, 0.25f, 0.25f, 1f),   false);

            // Achievement Titles (6)
            CreateTitleCardV4(grid, "Title_PrimerPaso",  "Primer Paso",   "LOCKED",    new Color(0.5f, 0.8f, 0.5f, 1f),   false);
            CreateTitleCardV4(grid, "Title_Imparable",   "Imparable",     "LOCKED",    ORANGE_HOT,                         false);
            CreateTitleCardV4(grid, "Title_Madrugador",  "Madrugador",    "LOCKED",    new Color(0.3f, 0.2f, 0.6f, 1f),   false);
            CreateTitleCardV4(grid, "Title_Perfeccionista","Perfeccionista","LOCKED",   CYAN_NEON,                          false);
            CreateTitleCardV4(grid, "Title_CampeonT",    "Campeon",       "LOCKED",    GOLD,                               false);
            CreateTitleCardV4(grid, "Title_Coleccionista","Coleccionista", "LOCKED",    SILVER,                             false);

            // Secret Titles (4)
            CreateTitleCardV4(grid, "Title_Fantasma",    "???",           "LOCKED",    new Color(0.2f, 0.1f, 0.3f, 1f),   false);
            CreateTitleCardV4(grid, "Title_Velocista",   "???",           "LOCKED",    new Color(1f, 0.3f, 0f, 1f),       false);
            CreateTitleCardV4(grid, "Title_ReyComeback", "???",           "LOCKED",    new Color(0.9f, 0.2f, 0.2f, 1f),   false);
            CreateTitleCardV4(grid, "Title_Completo",    "???",           "LOCKED",    PURPLE_PREMIUM,                     false);

            Debug.Log("[ShopPremiumUIBuilder] Titles Section V7 created (21 titles: 1 free + 4 DC + 4 DG + 1 IAP + 6 achievement + 4 secret)");
        }

        private static void CreateTitleCardV4(GameObject parent, string name, string displayName,
            string price, Color titleColor, bool isEquipped)
        {
            GameObject item = CreateChild(parent, name);

            Image itemBg = item.AddComponent<Image>();
            itemBg.color = CARD_BG;
            AddOutline(item, isEquipped ? titleColor : titleColor * 0.4f, isEquipped ? 2 : 1);

            Shadow itemShadow = item.AddComponent<Shadow>();
            itemShadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            itemShadow.effectDistance = new Vector2(3, -4);

            Button btn = item.AddComponent<Button>();
            SetupButton(btn, CARD_BG);

            HorizontalLayoutGroup hlg = item.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 14;
            hlg.padding = new RectOffset(20, 20, 14, 14);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            // Title name (flex)
            GameObject nameObj = CreateChild(item, "TitleName");
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = displayName;
            nameText.fontSize = FontSizes.Body;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = titleColor;
            nameText.alignment = TextAlignmentOptions.MidlineLeft;
            nameText.enableAutoSizing = true;
            nameText.fontSizeMin = FontSizes.AutoMinSmall;
            nameText.fontSizeMax = FontSizes.Body;
            LayoutElement nameLE = nameObj.AddComponent<LayoutElement>();
            nameLE.flexibleWidth = 1;

            // Price/Status
            if (isEquipped)
            {
                GameObject statusObj = CreateChild(item, "PurchasedBadgeText");
                TextMeshProUGUI statusText = statusObj.AddComponent<TextMeshProUGUI>();
                statusText.text = "Purchased";
                statusText.fontSize = FontSizes.Body;
                statusText.fontStyle = FontStyles.Bold;
                statusText.color = TEXT_SECONDARY;
                statusText.alignment = TextAlignmentOptions.MidlineRight;
                statusText.enableAutoSizing = true;
                statusText.fontSizeMin = FontSizes.AutoMinBody;
                statusText.fontSizeMax = statusText.fontSize;
                LayoutElement statusLE = statusObj.AddComponent<LayoutElement>();
                statusLE.minWidth = 100;
            }
            else
            {
                bool isGemPrice = !price.StartsWith("$") && !price.Equals("Free");
                Color btnColor = price.StartsWith("$") ? BUTTON_SUCCESS : (isGemPrice ? GEM_COLOR : COIN_COLOR);

                GameObject priceContainer = CreateChild(item, "PriceContainer");
                LayoutElement pcLE = priceContainer.AddComponent<LayoutElement>();
                pcLE.minWidth = 140;
                pcLE.preferredWidth = 140;

                GameObject priceBtn = CreateChild(priceContainer, "PriceButton");
                RectTransform priceBtnRT = priceBtn.GetComponent<RectTransform>();
                priceBtnRT.anchorMin = new Vector2(0.5f, 0.5f);
                priceBtnRT.anchorMax = new Vector2(0.5f, 0.5f);
                priceBtnRT.sizeDelta = new Vector2(130, 40);

                Image priceBg = priceBtn.AddComponent<Image>();
                priceBg.color = btnColor;

                HorizontalLayoutGroup priceHlg = priceBtn.AddComponent<HorizontalLayoutGroup>();
                priceHlg.spacing = 5;
                priceHlg.padding = new RectOffset(10, 10, 4, 4);
                priceHlg.childAlignment = TextAnchor.MiddleCenter;
                priceHlg.childControlWidth = false;
                priceHlg.childControlHeight = true;

                if (isGemPrice)
                {
                    GameObject gemIcon = CreateChild(priceBtn, "GemIcon");
                    Image gemImg = gemIcon.AddComponent<Image>();
                    gemImg.color = TEXT_DARK;
                    LayoutElement gemLE = gemIcon.AddComponent<LayoutElement>();
                    gemLE.minWidth = 18;
                    gemLE.minHeight = 18;
                }

                GameObject priceText = CreateChild(priceBtn, "Text");
                TextMeshProUGUI pt = priceText.AddComponent<TextMeshProUGUI>();
                pt.text = price;
                pt.fontSize = FontSizes.Body;
                pt.fontStyle = FontStyles.Bold;
                pt.color = TEXT_DARK;
                pt.alignment = TextAlignmentOptions.Center;
                pt.enableAutoSizing = true;
                pt.fontSizeMin = FontSizes.AutoMinBody;
                pt.fontSizeMax = pt.fontSize;
                LayoutElement ptLE = priceText.AddComponent<LayoutElement>();
                ptLE.flexibleWidth = 1;
            }
        }

        // ==================== POPUPS ====================

        private static void CreatePurchasePopup(Canvas canvas)
        {
            GameObject blocker = CreateChild(canvas.gameObject, "PurchaseBlocker");
            blocker.SetActive(false);
            SetStretch(blocker);

            Image blockerBg = blocker.AddComponent<Image>();
            blockerBg.color = BLOCKER_BG;
            Button blockerBtn = blocker.AddComponent<Button>();
            blockerBtn.transition = Selectable.Transition.None;
            blocker.transform.SetAsLastSibling();

            GameObject popup = CreateChild(blocker, "PurchasePopup");
            RectTransform popupRT = popup.GetComponent<RectTransform>();
            popupRT.anchorMin = new Vector2(0.5f, 0.5f);
            popupRT.anchorMax = new Vector2(0.5f, 0.5f);
            popupRT.sizeDelta = new Vector2(520, 440);

            Image popupBg = popup.AddComponent<Image>();
            popupBg.color = PANEL_BG;
            AddOutline(popup, CYAN_NEON, 2);

            VerticalLayoutGroup vlg = popup.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 18;
            vlg.padding = new RectOffset(28, 28, 28, 28);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Title
            GameObject title = CreateChild(popup, "ShopConfirmTitle");
            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "Confirm Purchase";
            titleText.fontSize = FontSizes.H4;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = CYAN_NEON;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = FontSizes.AutoMinSmall;
            titleText.fontSizeMax = FontSizes.H4;
            LayoutElement titleLE = title.AddComponent<LayoutElement>();
            titleLE.minHeight = 52;

            // Preview
            GameObject preview = CreateChild(popup, "Preview");
            HorizontalLayoutGroup previewHlg = preview.AddComponent<HorizontalLayoutGroup>();
            previewHlg.spacing = 15;
            previewHlg.childAlignment = TextAnchor.MiddleCenter;
            previewHlg.childControlWidth = false;
            previewHlg.childControlHeight = true;
            LayoutElement previewLE = preview.AddComponent<LayoutElement>();
            previewLE.minHeight = 60;

            GameObject previewIcon = CreateChild(preview, "Icon");
            Image previewImg = previewIcon.AddComponent<Image>();
            previewImg.color = GEM_COLOR;
            LayoutElement iconLE = previewIcon.AddComponent<LayoutElement>();
            iconLE.minWidth = 55;
            iconLE.minHeight = 55;

            GameObject previewAmount = CreateChild(preview, "Amount");
            TextMeshProUGUI amountText = previewAmount.AddComponent<TextMeshProUGUI>();
            amountText.text = "1,200 DigitGems";
            amountText.fontSize = FontSizes.Body;
            amountText.fontStyle = FontStyles.Bold;
            amountText.color = TEXT_PRIMARY;
            amountText.enableAutoSizing = true;
            amountText.fontSizeMin = FontSizes.AutoMinBody;
            amountText.fontSizeMax = amountText.fontSize;
            LayoutElement amountLE = previewAmount.AddComponent<LayoutElement>();
            amountLE.minWidth = 220;

            // Price
            GameObject priceObj = CreateChild(popup, "Price");
            TextMeshProUGUI priceText = priceObj.AddComponent<TextMeshProUGUI>();
            priceText.text = "Price: $9.99";
            priceText.fontSize = FontSizes.Body;
            priceText.fontStyle = FontStyles.Bold;
            priceText.color = TEXT_SECONDARY;
            priceText.alignment = TextAlignmentOptions.Center;
            priceText.enableAutoSizing = true;
            priceText.fontSizeMin = FontSizes.AutoMinBody;
            priceText.fontSizeMax = priceText.fontSize;
            LayoutElement priceLE = priceObj.AddComponent<LayoutElement>();
            priceLE.minHeight = 28;

            // Buttons
            GameObject buttons = CreateChild(popup, "Buttons");
            HorizontalLayoutGroup btnHlg = buttons.AddComponent<HorizontalLayoutGroup>();
            btnHlg.spacing = 16;
            btnHlg.childControlWidth = true;
            btnHlg.childControlHeight = true;
            btnHlg.childForceExpandWidth = true;
            LayoutElement btnLE = buttons.AddComponent<LayoutElement>();
            btnLE.minHeight = 65;

            CreatePopupButton(buttons, "CancelButton", "Cancel", BUTTON_SECONDARY, TEXT_PRIMARY);
            CreatePopupButton(buttons, "ConfirmButton", "Purchase", BUTTON_SUCCESS, TEXT_DARK);

            Debug.Log("[ShopPremiumUIBuilder] PurchasePopup V5 creado");
        }

        private static void CreateNotEnoughPopup(Canvas canvas)
        {
            GameObject blocker = CreateChild(canvas.gameObject, "NotEnoughBlocker");
            blocker.SetActive(false);
            SetStretch(blocker);

            Image blockerBg = blocker.AddComponent<Image>();
            blockerBg.color = BLOCKER_BG;
            Button blockerBtn = blocker.AddComponent<Button>();
            blockerBtn.transition = Selectable.Transition.None;
            blocker.transform.SetAsLastSibling();

            GameObject popup = CreateChild(blocker, "NotEnoughPopup");
            RectTransform popupRT = popup.GetComponent<RectTransform>();
            popupRT.anchorMin = new Vector2(0.5f, 0.5f);
            popupRT.anchorMax = new Vector2(0.5f, 0.5f);
            popupRT.sizeDelta = new Vector2(540, 420);

            Image popupBg = popup.AddComponent<Image>();
            popupBg.color = PANEL_BG;
            AddOutline(popup, GEM_COLOR, 2);

            VerticalLayoutGroup vlg = popup.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 15;
            vlg.padding = new RectOffset(28, 28, 28, 28);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Icon
            GameObject icon = CreateChild(popup, "Icon");
            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = GEM_COLOR;
            LayoutElement iconLE = icon.AddComponent<LayoutElement>();
            iconLE.minHeight = 60;
            iconLE.minWidth = 60;
            iconLE.preferredHeight = 60;
            iconLE.preferredWidth = 60;

            // Title
            GameObject title = CreateChild(popup, "ShopInsufficientTitle");
            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "Insufficient DigitGems";
            titleText.fontSize = FontSizes.H4;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GEM_COLOR;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = FontSizes.AutoMinSmall;
            titleText.fontSizeMax = FontSizes.H4;
            LayoutElement titleLE = title.AddComponent<LayoutElement>();
            titleLE.minHeight = 52;

            // Message
            GameObject msg = CreateChild(popup, "ShopInsufficientMessage");
            TextMeshProUGUI msgText = msg.AddComponent<TextMeshProUGUI>();
            msgText.text = "You don't have enough DigitGems.\nGet more in the shop!";
            msgText.fontSize = FontSizes.Body;
            msgText.fontStyle = FontStyles.Bold;
            msgText.color = TEXT_SECONDARY;
            msgText.alignment = TextAlignmentOptions.Center;
            msgText.enableAutoSizing = true;
            msgText.fontSizeMin = FontSizes.AutoMinBody;
            msgText.fontSizeMax = msgText.fontSize;
            LayoutElement msgLE = msg.AddComponent<LayoutElement>();
            msgLE.minHeight = 48;

            // Buttons
            GameObject buttons = CreateChild(popup, "Buttons");
            HorizontalLayoutGroup btnHlg = buttons.AddComponent<HorizontalLayoutGroup>();
            btnHlg.spacing = 16;
            btnHlg.childControlWidth = true;
            btnHlg.childControlHeight = true;
            btnHlg.childForceExpandWidth = true;
            LayoutElement btnLE = buttons.AddComponent<LayoutElement>();
            btnLE.minHeight = 65;

            CreatePopupButton(buttons, "CloseButton", "Close", BUTTON_SECONDARY, TEXT_PRIMARY);
            CreatePopupButton(buttons, "GetGemsButton", "Get DigitGems", GEM_COLOR, TEXT_DARK);

            Debug.Log("[ShopPremiumUIBuilder] NotEnoughPopup V5 creado");
        }

        private static void CreatePopupButton(GameObject parent, string name, string text, Color bgColor, Color textColor)
        {
            GameObject btn = CreateChild(parent, name);
            Image btnBg = btn.AddComponent<Image>();
            btnBg.color = bgColor;
            Button button = btn.AddComponent<Button>();
            SetupButton(button, bgColor);

            if (bgColor == BUTTON_SUCCESS || bgColor == GEM_COLOR)
            {
                AddOutline(btn, bgColor * 1.3f, 1);
            }

            GameObject textObj = CreateChild(btn, name + "Text");
            SetStretch(textObj);
            TextMeshProUGUI txt = textObj.AddComponent<TextMeshProUGUI>();
            txt.text = text;
            txt.fontSize = FontSizes.Body;
            txt.fontStyle = FontStyles.Bold;
            txt.color = textColor;
            txt.alignment = TextAlignmentOptions.Center;
            txt.enableAutoSizing = true;
            txt.fontSizeMin = FontSizes.AutoMinSmall;
            txt.fontSizeMax = FontSizes.Body;
        }

        // ==================== SHOP MANAGER ====================

        private static void AddShopManager(Canvas canvas)
        {
            var existing = Object.FindObjectOfType<DigitPark.Monetization.ShopManager>();
            if (existing != null)
            {
                Debug.Log($"[ShopPremiumUIBuilder] ShopManager ya existe en '{existing.gameObject.name}', no se duplica");
                return;
            }

            canvas.gameObject.AddComponent<DigitPark.Monetization.ShopManager>();
            Debug.Log("[ShopPremiumUIBuilder] ShopManager agregado al Canvas");
        }

        private static void AutoAssignReferences()
        {
            Debug.Log("[ShopPremiumUIBuilder] Auto-asignando referencias...");
            ShopReferenceAssigner.ResetLog();
            ShopReferenceAssigner.AssignAllReferences();
            Debug.Log("[ShopPremiumUIBuilder] Referencias auto-asignadas");
        }

        // ==================== VICTORY EFFECTS SECTION (Economy Rebalance V55) ====================

        private static readonly Color EFFECT_COLOR = new Color(0f, 1f, 0.6f, 1f);

        private static void CreateEffectsSection(GameObject parent)
        {
            GameObject section = CreateChild(parent, "EffectsSection");

            VerticalLayoutGroup vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            CreateSectionDividerV5(section, "VICTORY EFFECTS", EFFECT_COLOR);

            // Grid — 2 columns
            GameObject grid = CreateChild(section, "EffectsGrid");

            GridLayoutGroup glg = grid.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(GRID_CELL_W, GRID_COSMETIC_H);
            glg.spacing = new Vector2(GRID_SPACING, GRID_SPACING);
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = GRID_COLUMNS;

            LayoutElement gridLE = grid.AddComponent<LayoutElement>();
            gridLE.minHeight = GRID_COSMETIC_H * 5 + GRID_SPACING * 4; // 10 items in 2-col = 5 rows

            // 8 effects + 2 bundles = 10 items
            CreateCosmeticCardV4(grid, "Effect_Confetti",       "Confetti",        "EQUIPPED",  new Color(1f, 0.8f, 0f, 1f),   true,  "free");
            CreateCosmeticCardV4(grid, "Effect_Fireworks",      "Fireworks",       "2,000",     new Color(1f, 0.3f, 0.1f, 1f), false, "coin");
            CreateCosmeticCardV4(grid, "Effect_Lightning",      "Lightning",       "5,000",     new Color(0.3f, 0.6f, 1f, 1f), false, "coin");
            CreateCosmeticCardV4(grid, "Effect_GoldRain",       "Gold Rain",       "250",       GOLD,                           false, "gem");
            CreateCosmeticCardV4(grid, "Effect_NeonExplosion",  "Neon Explosion",  "400",       CYAN_NEON,                      false, "gem");
            CreateCosmeticCardV4(grid, "Effect_Rainbow",        "Rainbow",         "750",       new Color(1f, 0f, 0.5f, 1f),   false, "gem");
            CreateCosmeticCardV4(grid, "Effect_CrownDrop",      "Crown Drop",      "$1.99",     GOLD,                           false, "real");
            CreateCosmeticCardV4(grid, "Effect_FireRing",       "Fire Ring",       "$2.99",     ORANGE_HOT,                     false, "real");
            // Bundles
            CreateCosmeticCardV4(grid, "EffectBundle_DC",       "DC Bundle",       "5,000",     COIN_COLOR,                     false, "coin");
            CreateCosmeticCardV4(grid, "EffectBundle_DG",       "DG Bundle",       "1,000",     GEM_COLOR,                      false, "gem");

            Debug.Log("[ShopPremiumUIBuilder] Effects Section created (8 effects + 2 bundles)");
        }

        // ==================== BATTLE CARDS SECTION (Economy Rebalance V55) ====================

        private static readonly Color BATTLECARD_COLOR = new Color(0.4f, 0.7f, 1f, 1f);

        private static void CreateBattleCardsSection(GameObject parent)
        {
            GameObject section = CreateChild(parent, "BattleCardsSection");

            VerticalLayoutGroup vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            CreateSectionDividerV5(section, "BATTLE CARDS", BATTLECARD_COLOR);

            // Grid — 2 columns
            GameObject grid = CreateChild(section, "BattleCardsGrid");

            GridLayoutGroup glg = grid.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(GRID_CELL_W, GRID_COSMETIC_H);
            glg.spacing = new Vector2(GRID_SPACING, GRID_SPACING);
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = GRID_COLUMNS;

            LayoutElement gridLE = grid.AddComponent<LayoutElement>();
            gridLE.minHeight = GRID_COSMETIC_H * 10 + GRID_SPACING * 9; // 19 items in 2-col = 10 rows

            // Free (1)
            CreateCosmeticCardV4(grid, "Card_NeonCore",     "Neon Core",      "EQUIPPED",  CYAN_NEON,                      true,  "free");

            // DC (4)
            CreateCosmeticCardV4(grid, "Card_Circuit",      "Circuit",        "500",       new Color(0.3f, 0.8f, 0.3f),    false, "coin");
            CreateCosmeticCardV4(grid, "Card_DataGrid",     "Data Grid",      "1,500",     new Color(0.2f, 0.6f, 0.8f),    false, "coin");
            CreateCosmeticCardV4(grid, "Card_Chromatic",    "Chromatic",      "3,000",     new Color(0.8f, 0.4f, 0.9f),    false, "coin");
            CreateCosmeticCardV4(grid, "Card_Titan",        "Titan",          "6,000",     new Color(0.9f, 0.7f, 0.2f),    false, "coin");

            // DG Standard (5)
            CreateCosmeticCardV4(grid, "Card_Frost",        "Frost",          "75",        new Color(0.7f, 0.9f, 1f),      false, "gem");
            CreateCosmeticCardV4(grid, "Card_Shadow",       "Shadow",         "150",       new Color(0.2f, 0.15f, 0.3f),   false, "gem");
            CreateCosmeticCardV4(grid, "Card_Prism",        "Prism",          "250",       new Color(0.9f, 0.3f, 0.8f),    false, "gem");
            CreateCosmeticCardV4(grid, "Card_NebulaCard",   "Nebula Card",    "350",       new Color(0.3f, 0.1f, 0.6f),    false, "gem");
            CreateCosmeticCardV4(grid, "Card_Quantum",      "Quantum",        "500",       new Color(0f, 0.8f, 0.9f),      false, "gem");

            // DG Premium animated (5)
            CreateCosmeticCardV4(grid, "Card_Phoenix",      "Phoenix",        "200",       new Color(1f, 0.4f, 0.1f),      false, "gem");
            CreateCosmeticCardV4(grid, "Card_Inferno",      "Inferno",        "400",       new Color(1f, 0.2f, 0f),        false, "gem");
            CreateCosmeticCardV4(grid, "Card_Storm",        "Storm",          "600",       new Color(0.3f, 0.5f, 1f),      false, "gem");
            CreateCosmeticCardV4(grid, "Card_VoidWalker",   "Void Walker",    "800",       new Color(0.1f, 0f, 0.2f),      false, "gem");
            CreateCosmeticCardV4(grid, "Card_CosmicKing",   "Cosmic King",    "1,200",     GOLD,                            false, "gem");

            // Earn (4) — shown as locked with achievement hint
            CreateCosmeticCardV4(grid, "Card_Champion",     "Champion",       "LOCKED",    GOLD,                            false, "earn");
            CreateCosmeticCardV4(grid, "Card_Perfectionist","Perfectionist",  "LOCKED",    PURPLE_PREMIUM,                  false, "earn");
            CreateCosmeticCardV4(grid, "Card_Veteran",      "Veteran",        "LOCKED",    SILVER,                          false, "earn");
            CreateCosmeticCardV4(grid, "Card_Legend",        "Legend",         "LOCKED",    ORANGE_HOT,                      false, "earn");

            Debug.Log("[ShopPremiumUIBuilder] BattleCards Section created (19 cards: 1 free + 4 DC + 5 DG Std + 5 DG Premium + 4 Earn)");
        }

        // ==================== HELPER: COSMETIC DISCLAIMER ====================

        /// <summary>
        /// Economy Rebalance V55: Anti-P2W disclaimer before VIP banner.
        /// </summary>
        // ==================== LIMITED CONTENT SECTION (13F — Whale Ceiling) ====================

        private static readonly Color LIMITED_COLOR = new Color(1f, 0.3f, 0.6f, 1f);

        /// <summary>
        /// Economy Rebalance V55 / 13F — Limited/Rotating Content Section
        /// Populated at runtime by RotatingContentService.ActiveItems.
        /// Shows: Seasonal BattleCards, Monthly IAP Frames, Limited Theme Variants.
        /// Hidden when no active content exists (RotatingContentUIController manages visibility).
        /// </summary>
        private static void CreateLimitedContentSection(GameObject parent)
        {
            GameObject section = CreateChild(parent, "LimitedContentSection");

            VerticalLayoutGroup vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            CreateSectionDividerV5(section, "LIMITED TIME", LIMITED_COLOR);

            // Placeholder slots — populated at runtime by RotatingContentUIController
            GameObject grid = CreateChild(section, "LimitedGrid");

            GridLayoutGroup glg = grid.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(GRID_CELL_W, GRID_COSMETIC_H);
            glg.spacing = new Vector2(GRID_SPACING, GRID_SPACING);
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = GRID_COLUMNS;

            LayoutElement gridLE = grid.AddComponent<LayoutElement>();
            gridLE.minHeight = GRID_COSMETIC_H; // Will be adjusted at runtime

            // 3 placeholder slots (replaced at runtime with actual active items)
            CreateCosmeticCardV4(grid, "Limited_Slot1", "Seasonal Card",    "COMING SOON", LIMITED_COLOR,  false, "limited");
            CreateCosmeticCardV4(grid, "Limited_Slot2", "Monthly Frame",    "COMING SOON", GOLD,           false, "limited");
            CreateCosmeticCardV4(grid, "Limited_Slot3", "Theme Variant",    "COMING SOON", PURPLE_PREMIUM, false, "limited");

            // Timer for limited content
            GameObject timerObj = CreateChild(section, "LimitedTimer");
            LayoutElement timerLE = timerObj.AddComponent<LayoutElement>();
            timerLE.minHeight = 30;
            TextMeshProUGUI timerText = timerObj.AddComponent<TextMeshProUGUI>();
            timerText.text = "Ends in: --d --:--:--";
            timerText.fontSize = FontSizes.BodySmall;
            timerText.fontStyle = FontStyles.Bold;
            timerText.color = LIMITED_COLOR;
            timerText.alignment = TextAlignmentOptions.Center;
            timerText.enableAutoSizing = true;
            timerText.fontSizeMin = FontSizes.AutoMinSmall;
            timerText.fontSizeMax = FontSizes.BodySmall;

            Debug.Log("[ShopPremiumUIBuilder] Limited Content Section created (runtime-populated by RotatingContentService)");
        }

        private static void CreateCosmeticDisclaimer(GameObject parent)
        {
            GameObject disclaimer = CreateChild(parent, "CosmeticDisclaimer");
            LayoutElement le = disclaimer.AddComponent<LayoutElement>();
            le.minHeight = 40;
            le.preferredHeight = 40;

            TextMeshProUGUI text = disclaimer.AddComponent<TextMeshProUGUI>();
            text.text = "All items are cosmetic only. They do not affect gameplay.";
            text.fontSize = FontSizes.Caption;
            text.fontStyle = FontStyles.Italic;
            text.color = TEXT_MUTED;
            text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = true;
            text.fontSizeMin = FontSizes.AutoMinSmall;
            text.fontSizeMax = FontSizes.Caption;
        }

        // ==================== HELPER: SECTION DIVIDER V5 ====================

        /// <summary>
        /// V5: Massive ribbon-style section divider (90px) — colored background, centered title, top/bottom borders
        /// </summary>
        private static void CreateSectionDividerV5(GameObject parent, string title, Color accentColor)
        {
            GameObject divider = CreateChild(parent, title.Replace(" ", "") + "Divider");
            LayoutElement divLE = divider.AddComponent<LayoutElement>();
            divLE.minHeight = SECTION_HEADER_HEIGHT;
            divLE.preferredHeight = SECTION_HEADER_HEIGHT;

            VerticalLayoutGroup vlg = divider.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(0, 0, 0, 0);
            vlg.spacing = 0;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;

            Shadow divShadow = divider.AddComponent<Shadow>();
            divShadow.effectColor = new Color(0f, 0f, 0f, 0.35f);
            divShadow.effectDistance = new Vector2(2, -3);

            // Top border (2px accent line)
            GameObject topBorder = CreateChild(divider, "TopBorder");
            LayoutElement topLE = topBorder.AddComponent<LayoutElement>();
            topLE.minHeight = 2;
            topLE.preferredHeight = 2;
            Image topImg = topBorder.AddComponent<Image>();
            topImg.color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.6f);
            topImg.raycastTarget = false;

            // Content area (colored background)
            GameObject contentArea = CreateChild(divider, "ContentArea");
            LayoutElement contentLE = contentArea.AddComponent<LayoutElement>();
            contentLE.flexibleHeight = 1;
            Image contentBg = contentArea.AddComponent<Image>();
            contentBg.color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.12f);
            contentBg.raycastTarget = false;

            // Section title (centered, auto-sizing)
            GameObject titleObj = CreateChild(contentArea, "SectionTitle");
            RectTransform titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = Vector2.zero;
            titleRT.anchorMax = Vector2.one;
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = title;
            titleText.fontSize = FontSizes.H4;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = accentColor;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.enableWordWrapping = false;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = FontSizes.Subtitle;
            titleText.fontSizeMax = FontSizes.H4;

            // Bottom border (2px accent line)
            GameObject bottomBorder = CreateChild(divider, "BottomBorder");
            LayoutElement bottomLE = bottomBorder.AddComponent<LayoutElement>();
            bottomLE.minHeight = 2;
            bottomLE.preferredHeight = 2;
            Image bottomImg = bottomBorder.AddComponent<Image>();
            bottomImg.color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.6f);
            bottomImg.raycastTarget = false;
        }

        // ==================== HELPER: BANNER DEPTH ====================

        private static void CreateBannerShadow(GameObject banner)
        {
            GameObject shadowObj = CreateChild(banner, "Shadow");
            shadowObj.transform.SetAsFirstSibling();
            RectTransform shadowRT = shadowObj.GetComponent<RectTransform>();
            shadowRT.anchorMin = Vector2.zero;
            shadowRT.anchorMax = Vector2.one;
            shadowRT.offsetMin = new Vector2(8, -10);
            shadowRT.offsetMax = Vector2.zero;
            Image shadowImg = shadowObj.AddComponent<Image>();
            shadowImg.color = new Color(0f, 0f, 0f, 0.5f);
            shadowImg.raycastTarget = false;
            LayoutElement shadowLE = shadowObj.AddComponent<LayoutElement>();
            shadowLE.ignoreLayout = true;
        }

        private static void CreateBannerSide(GameObject banner, Color accentColor)
        {
            GameObject sideObj = CreateChild(banner, "Side");
            sideObj.transform.SetSiblingIndex(1);
            RectTransform sideRT = sideObj.GetComponent<RectTransform>();
            sideRT.anchorMin = new Vector2(0, 0);
            sideRT.anchorMax = new Vector2(1, 0);
            sideRT.offsetMin = new Vector2(0, -8);
            sideRT.offsetMax = new Vector2(0, 0);
            Image sideImg = sideObj.AddComponent<Image>();
            sideImg.color = new Color(accentColor.r * 0.3f, accentColor.g * 0.3f, accentColor.b * 0.3f, 1f);
            sideImg.raycastTarget = false;
            LayoutElement sideLE = sideObj.AddComponent<LayoutElement>();
            sideLE.ignoreLayout = true;
        }

        // ==================== HELPER: INLINE BADGE ====================

        private static void CreateInlineBadge(GameObject parent, string text, Color accentColor, float width, string textGoName = "BadgeText")
        {
            GameObject badge = CreateChild(parent, "Badge");
            Image badgeBg = badge.AddComponent<Image>();
            badgeBg.color = new Color(0.05f, 0.07f, 0.12f, 0.95f);
            AddOutline(badge, accentColor, 2);
            LayoutElement badgeLE = badge.AddComponent<LayoutElement>();
            badgeLE.minHeight = BADGE_HEIGHT;
            badgeLE.preferredHeight = BADGE_HEIGHT;
            if (width > 0)
            {
                badgeLE.minWidth = width;
                badgeLE.preferredWidth = width;
            }

            GameObject badgeText = CreateChild(badge, textGoName);
            SetStretch(badgeText);
            TextMeshProUGUI bt = badgeText.AddComponent<TextMeshProUGUI>();
            bt.text = text;
            bt.fontSize = FontSizes.BodySmall;
            bt.fontStyle = FontStyles.Bold;
            bt.color = accentColor;
            bt.alignment = TextAlignmentOptions.Center;
            bt.enableAutoSizing = true;
            bt.fontSizeMin = FontSizes.AutoMinBody;
            bt.fontSizeMax = bt.fontSize;
        }

        // ==================== HELPER: PRICE BUTTON ====================

        private static void CreatePriceButton(GameObject parent, string price, Color bgColor, Color textColor, float height, float fontSize)
        {
            GameObject priceBtn = CreateChild(parent, "BuyButton");
            Image priceBg = priceBtn.AddComponent<Image>();
            priceBg.color = bgColor;
            Button buyButton = priceBtn.AddComponent<Button>();
            SetupButton(buyButton, bgColor);
            AddOutline(priceBtn, bgColor * 1.3f, 2);
            LayoutElement priceLE = priceBtn.AddComponent<LayoutElement>();
            priceLE.minHeight = height;
            priceLE.preferredHeight = height;

            GameObject priceText = CreateChild(priceBtn, "Text");
            SetStretch(priceText);
            TextMeshProUGUI pt = priceText.AddComponent<TextMeshProUGUI>();
            pt.text = price;
            pt.fontSize = fontSize;
            pt.fontStyle = FontStyles.Bold;
            pt.color = textColor;
            pt.alignment = TextAlignmentOptions.Center;
            pt.enableAutoSizing = true;
            pt.fontSizeMin = FontSizes.AutoMinSmall;
            pt.fontSizeMax = fontSize;
        }

        // ==================== UTILITIES ====================

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

        private static GameObject CreateChild(GameObject parent, string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent.transform, false);
            child.AddComponent<RectTransform>();
            return child;
        }

        private static void SetStretch(GameObject obj)
        {
            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt == null) rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }

        private static void SetupButton(Button btn, Color baseColor)
        {
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.92f, 0.92f, 0.92f, 1f);
            colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
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

        private static void CreateGlowLine(GameObject parent, Color color, bool isTop)
        {
            GameObject glow = CreateChild(parent, isTop ? "TopGlow" : "BottomGlow");
            RectTransform rt = glow.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, isTop ? 1 : 0);
            rt.anchorMax = new Vector2(1, isTop ? 1 : 0);
            rt.pivot = new Vector2(0.5f, isTop ? 0 : 1);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0, 3);

            Image img = glow.AddComponent<Image>();
            img.color = color;
        }

        private static void MarkSceneDirty()
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
    }
}
