using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.Editor.AutoAssigners;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// Shop Premium UI Builder V3 - Estilo Clash Royale
    /// Scroll vertical continuo con cards profesionales grandes
    /// Diseño TOP 10 iOS - 1080x1920 reference
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
        private static readonly Color RED_URGENT = new Color(1f, 0.25f, 0.25f, 1f);
        private static readonly Color GREEN_FREE = new Color(0.3f, 0.9f, 0.4f, 1f);

        private static readonly Color GEM_COLOR = new Color(0.4f, 0.85f, 1f, 1f);
        private static readonly Color COIN_COLOR = new Color(1f, 0.85f, 0.3f, 1f);

        private static readonly Color FRAME_COLOR = new Color(0.85f, 0.6f, 0.2f, 1f);
        private static readonly Color TITLE_COLOR = new Color(0.9f, 0.75f, 1f, 1f);

        private static readonly Color BLOCKER_BG = new Color(0f, 0f, 0f, 0.9f);

        // ==================== DIMENSIONES V3.1 (consistente con toda la app) ====================
        private const float HEADER_HEIGHT = 100f;
        private const float CONTENT_PADDING = 16f;
        private const float SECTION_SPACING = 32f;

        // Section dividers (estilo BetSelection, prominentes)
        private const float SECTION_DIVIDER_HEIGHT = 100f;

        // Banners
        private const float FEATURED_HEIGHT = 220f;
        private const float OFFER_HEIGHT = 175f;
        private const float VIP_HEIGHT = 175f;

        // Grid cells (mas altos para textos grandes)
        private const float GRID_CELL_W = 330f;
        private const float GRID_GEM_COIN_H = 330f;
        private const float GRID_THEME_H = 350f;
        private const float GRID_COSMETIC_H = 290f;
        private const float GRID_SPACING = 12f;
        private const int GRID_COLUMNS = 3;

        // Title cards (2 columns)
        private const float TITLE_CELL_W = 510f;
        private const float TITLE_CELL_H = 100f;
        private const int TITLE_COLUMNS = 2;

        // Daily Deals
        private const float DAILY_ITEM_HEIGHT = 210f;

        // Non-font dimensions
        private const float PRICE_BTN_HEIGHT = 52f;
        private const float BADGE_HEIGHT = 34f;

        [MenuItem("DigitPark/UI Builders/Monetization/Shop Premium (Clash Royale Style)", false, 144)]
        public static void BuildUI()
        {
            if (!EditorUtility.DisplayDialog("Shop Premium UI Builder V3",
                "Esto construira la UI PREMIUM V3 de Shop estilo Clash Royale.\n\n" +
                "Incluye:\n" +
                "- Featured Banner con countdown\n" +
                "- Ofertas Especiales (2 banners)\n" +
                "- Daily Deals (3 items)\n" +
                "- DigitGems (6 packs)\n" +
                "- DigitCoins (4 packs)\n" +
                "- Temas (9 items)\n" +
                "- Marcos (8 items)\n" +
                "- Marcos DigitGems (6 items)\n" +
                "- Marcos Premium (3 items)\n" +
                "- Titulos (10 items)\n" +
                "- VIP Bundle\n\n" +
                "Cards grandes 330px, 3 columnas.\n" +
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
            Debug.Log("[ShopPremiumUIBuilder] ========== INICIANDO CONSTRUCCION V3 ==========");

            CleanupOldUI();

            Canvas canvas = SetupCanvas();
            if (canvas == null) return;

            ClearExistingUI(canvas);

            // Estructura base
            CreateBackground(canvas);
            GameObject safeArea = CreateSafeArea(canvas);

            // Header fijo
            CreatePremiumHeader(safeArea);

            // Scroll con todo el contenido
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
            Debug.Log("[ShopPremiumUIBuilder] ========== CONSTRUCCION V3 COMPLETADA ==========");

            // Auto-assign references
            AutoAssignReferences();

            if (!AllScenesBatchBuilder.SilentMode)
                EditorUtility.DisplayDialog("Completado",
                    "Shop Premium V3 UI construida y referencias asignadas automaticamente!",
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
            bgImage.color = DARK_BG;
            bgImage.raycastTarget = false;
            bg.transform.SetAsFirstSibling();
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
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0, HEADER_HEIGHT);

            Image headerBg = header.AddComponent<Image>();
            headerBg.color = HEADER_BG;
            headerBg.raycastTarget = false;

            CreateGlowLine(header, CYAN_NEON, false);

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
                // Fallback: boton manual
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
                TextMeshProUGUI backText = backIcon.AddComponent<TextMeshProUGUI>();
                backText.text = "<";
                backText.fontSize = FontSizes.Body;
                backText.fontStyle = FontStyles.Bold;
                backText.color = CYAN_NEON;
                backText.alignment = TextAlignmentOptions.Center;
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
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = FontSizes.AutoMinTitle;
            titleText.fontSizeMax = FontSizes.H4;
            titleText.overflowMode = TextOverflowModes.Ellipsis;
            titleText.raycastTarget = false;

            // Currency Display
            CreateHeaderCurrency(header);

            Debug.Log("[ShopPremiumUIBuilder] Header creado");
        }

        private static void CreateHeaderCurrency(GameObject header)
        {
            GameObject container = CreateChild(header, "CurrencyDisplay");
            RectTransform rt = container.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(1, 0.5f);
            rt.anchoredPosition = new Vector2(-20, 0);
            rt.sizeDelta = new Vector2(380, 50);

            HorizontalLayoutGroup hlg = container.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 14;
            hlg.childAlignment = TextAnchor.MiddleRight;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            CreateCurrencyPill(container, "GemsDisplay", "1,250", GEM_COLOR);
            CreateCurrencyPill(container, "CoinsDisplay", "5,430", COIN_COLOR);
        }

        private static void CreateCurrencyPill(GameObject parent, string name, string amount, Color color)
        {
            GameObject pill = CreateChild(parent, name);
            RectTransform rt = pill.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(135, 46);

            Image bg = pill.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.12f, 0.18f, 0.95f);
            AddOutline(pill, color * 0.6f, 1);

            HorizontalLayoutGroup hlg = pill.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6;
            hlg.padding = new RectOffset(10, 10, 4, 4);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;

            LayoutElement le = pill.AddComponent<LayoutElement>();
            le.minWidth = 135;
            le.preferredWidth = 135;

            // Icon
            GameObject icon = CreateChild(pill, "Icon");
            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = color;
            iconImg.preserveAspect = true;
            LayoutElement iconLE = icon.AddComponent<LayoutElement>();
            iconLE.minWidth = 28;
            iconLE.minHeight = 28;
            iconLE.preferredWidth = 28;
            iconLE.preferredHeight = 28;

            // Amount
            GameObject amountObj = CreateChild(pill, "Amount");
            TextMeshProUGUI amountText = amountObj.AddComponent<TextMeshProUGUI>();
            amountText.text = amount;
            amountText.fontSize = FontSizes.Body;
            amountText.fontStyle = FontStyles.Bold;
            amountText.color = TEXT_PRIMARY;
            amountText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement amountLE = amountObj.AddComponent<LayoutElement>();
            amountLE.flexibleWidth = 1;

            // Plus
            GameObject plus = CreateChild(pill, "Plus");
            Image plusBg = plus.AddComponent<Image>();
            plusBg.color = color;
            LayoutElement plusLE = plus.AddComponent<LayoutElement>();
            plusLE.minWidth = 22;
            plusLE.minHeight = 22;

            GameObject plusText = CreateChild(plus, "Text");
            SetStretch(plusText);
            TextMeshProUGUI pt = plusText.AddComponent<TextMeshProUGUI>();
            pt.text = "+";
            pt.fontSize = FontSizes.Body;
            pt.fontStyle = FontStyles.Bold;
            pt.color = TEXT_DARK;
            pt.alignment = TextAlignmentOptions.Center;
        }

        // ==================== MAIN SCROLL CONTENT ====================

        private static void CreateMainScrollContent(GameObject parent)
        {
            GameObject scrollView = CreateChild(parent, "ShopScrollView");

            RectTransform scrollRT = scrollView.GetComponent<RectTransform>();
            scrollRT.anchorMin = Vector2.zero;
            scrollRT.anchorMax = Vector2.one;
            scrollRT.offsetMin = new Vector2(0, 0);
            scrollRT.offsetMax = new Vector2(0, -HEADER_HEIGHT);

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

            // Transparent background catches raycasts in dead zones (spacing/padding)
            // ensuring scroll works when touching ANY part of the content area
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

            // === TODAS LAS SECCIONES ===

            // 1. Featured Banner
            CreateFeaturedBanner(content);

            // 2. Ofertas Especiales (2 banners)
            CreateSpecialOffersSection(content);

            // 3. Daily Deals
            CreateDailyDealsSection(content);

            // 4. DigitGems
            CreateGemsSection(content);

            // 5. DigitCoins
            CreateCoinsSection(content);

            // 6. Themes
            CreateThemesSection(content);

            // 7. Marcos
            CreateFramesSection(content);

            // 8. Marcos DigitGems
            CreateGemFramesSection(content);

            // 9. Marcos Premium
            CreatePremiumFramesSection(content);

            // 10. Titulos
            CreateTitlesSection(content);

            // 11. VIP Bundle
            CreateVIPSection(content);

            Debug.Log("[ShopPremiumUIBuilder] Scroll content V3 creado con todas las secciones");
        }

        // ==================== FEATURED BANNER ====================

        private static void CreateFeaturedBanner(GameObject parent)
        {
            GameObject banner = CreateChild(parent, "FeaturedBanner");

            LayoutElement le = banner.AddComponent<LayoutElement>();
            le.minHeight = FEATURED_HEIGHT;
            le.preferredHeight = FEATURED_HEIGHT;

            Image bannerBg = banner.AddComponent<Image>();
            bannerBg.color = new Color(0.12f, 0.06f, 0.22f, 1f);
            AddOutline(banner, PURPLE_PREMIUM, 3);

            // Shadow (child element for 3D depth)
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

            // Side (3D depth strip below banner)
            GameObject sideObj = CreateChild(banner, "Side");
            sideObj.transform.SetSiblingIndex(1);
            RectTransform sideRT = sideObj.GetComponent<RectTransform>();
            sideRT.anchorMin = new Vector2(0, 0);
            sideRT.anchorMax = new Vector2(1, 0);
            sideRT.offsetMin = new Vector2(0, -8);
            sideRT.offsetMax = new Vector2(0, 0);
            Image sideImg = sideObj.AddComponent<Image>();
            sideImg.color = new Color(PURPLE_PREMIUM.r * 0.3f, PURPLE_PREMIUM.g * 0.3f, PURPLE_PREMIUM.b * 0.3f, 1f);
            sideImg.raycastTarget = false;
            LayoutElement sideLE = sideObj.AddComponent<LayoutElement>();
            sideLE.ignoreLayout = true;

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
            iconLE.minWidth = 130;
            iconLE.preferredWidth = 130;

            GameObject icon = CreateChild(iconContainer, "Icon");
            RectTransform iconRT = icon.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.sizeDelta = new Vector2(110, 110);
            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = GOLD;

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

            // Badge
            CreateInlineBadge(info, "70% OFF", ORANGE_HOT, 110);

            // Title
            GameObject title = CreateChild(info, "Title");
            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "STARTER PACK";
            titleText.fontSize = FontSizes.Body;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GOLD;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement titleLE = title.AddComponent<LayoutElement>();
            titleLE.minHeight = 42;

            // Contents
            GameObject contents = CreateChild(info, "Contents");
            TextMeshProUGUI contentsText = contents.AddComponent<TextMeshProUGUI>();
            contentsText.text = "500 DigitGems + Exclusive Theme + Avatar";
            contentsText.fontSize = FontSizes.Body;
            contentsText.color = TEXT_SECONDARY;
            LayoutElement contentsLE = contents.AddComponent<LayoutElement>();
            contentsLE.minHeight = 26;

            // Timer
            GameObject timer = CreateChild(info, "Timer");
            TextMeshProUGUI timerText = timer.AddComponent<TextMeshProUGUI>();
            timerText.text = "Expires in: 23:45:12";
            timerText.fontSize = FontSizes.Body;
            timerText.color = ORANGE_HOT;
            LayoutElement timerLE = timer.AddComponent<LayoutElement>();
            timerLE.minHeight = 26;

            // Right - Buy
            GameObject buyContainer = CreateChild(banner, "BuyContainer");
            LayoutElement buyContainerLE = buyContainer.AddComponent<LayoutElement>();
            buyContainerLE.minWidth = 140;
            buyContainerLE.preferredWidth = 140;

            VerticalLayoutGroup buyVlg = buyContainer.AddComponent<VerticalLayoutGroup>();
            buyVlg.spacing = 4;
            buyVlg.childAlignment = TextAnchor.MiddleCenter;
            buyVlg.childControlWidth = true;
            buyVlg.childControlHeight = true;
            buyVlg.childForceExpandHeight = false;

            // Original price
            GameObject origPrice = CreateChild(buyContainer, "OriginalPrice");
            TextMeshProUGUI origText = origPrice.AddComponent<TextMeshProUGUI>();
            origText.text = "<s>$9.99</s>";
            origText.fontSize = FontSizes.Body;
            origText.color = TEXT_MUTED;
            origText.alignment = TextAlignmentOptions.Center;
            LayoutElement origLE = origPrice.AddComponent<LayoutElement>();
            origLE.minHeight = 26;

            // Buy button
            CreatePriceButton(buyContainer, "$2.99", BUTTON_SUCCESS, TEXT_DARK, 58, FontSizes.Body);

            Debug.Log("[ShopPremiumUIBuilder] Featured Banner V3 creado");
        }

        // ==================== SPECIAL OFFERS ====================

        private static void CreateSpecialOffersSection(GameObject parent)
        {
            GameObject section = CreateChild(parent, "SpecialOffersSection");

            VerticalLayoutGroup vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            CreateSectionHeader(section, "SPECIAL OFFERS", ORANGE_HOT);

            // Offer 1
            CreateOfferBanner(section, "Offer_WeekendGems", "WEEKEND PACK",
                "2,000 DigitGems + 10,000 DigitCoins", "50% OFF", "$4.99", "<s>$9.99</s>",
                new Color(0.08f, 0.12f, 0.25f, 1f), GEM_COLOR);

            // Offer 2
            CreateOfferBanner(section, "Offer_MegaCoins", "MEGA DIGITCOINS",
                "50,000 DigitCoins + 3 Frames", "LIMITED", "$2.99", "<s>$5.99</s>",
                new Color(0.15f, 0.1f, 0.05f, 1f), COIN_COLOR);

            Debug.Log("[ShopPremiumUIBuilder] Special Offers V3 creado");
        }

        private static void CreateOfferBanner(GameObject parent, string name, string title,
            string contents, string badge, string price, string origPrice,
            Color bgColor, Color accentColor)
        {
            GameObject banner = CreateChild(parent, name);

            LayoutElement le = banner.AddComponent<LayoutElement>();
            le.minHeight = OFFER_HEIGHT;
            le.preferredHeight = OFFER_HEIGHT;

            Image bannerBg = banner.AddComponent<Image>();
            bannerBg.color = bgColor;
            AddOutline(banner, accentColor, 2);

            // Shadow (child element for 3D depth)
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

            // Side (3D depth strip below banner)
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

            HorizontalLayoutGroup hlg = banner.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 16;
            hlg.padding = new RectOffset(20, 20, 16, 16);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            // Icon
            GameObject iconContainer = CreateChild(banner, "IconContainer");
            LayoutElement iconLE = iconContainer.AddComponent<LayoutElement>();
            iconLE.minWidth = 100;
            iconLE.preferredWidth = 100;

            GameObject icon = CreateChild(iconContainer, "Icon");
            RectTransform iconRT = icon.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.sizeDelta = new Vector2(85, 85);
            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = accentColor;

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

            CreateInlineBadge(info, badge, RED_URGENT, 110);

            GameObject titleObj = CreateChild(info, "Title");
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = title;
            titleText.fontSize = FontSizes.Body;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = TEXT_PRIMARY;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement titleLE = titleObj.AddComponent<LayoutElement>();
            titleLE.minHeight = 36;

            GameObject contentsObj = CreateChild(info, "Contents");
            TextMeshProUGUI contentsText = contentsObj.AddComponent<TextMeshProUGUI>();
            contentsText.text = contents;
            contentsText.fontSize = FontSizes.Body;
            contentsText.color = TEXT_SECONDARY;
            LayoutElement contentsLE = contentsObj.AddComponent<LayoutElement>();
            contentsLE.minHeight = 26;

            // Buy
            GameObject buyContainer = CreateChild(banner, "BuyContainer");
            LayoutElement buyContainerLE = buyContainer.AddComponent<LayoutElement>();
            buyContainerLE.minWidth = 130;
            buyContainerLE.preferredWidth = 130;

            VerticalLayoutGroup buyVlg = buyContainer.AddComponent<VerticalLayoutGroup>();
            buyVlg.spacing = 3;
            buyVlg.childAlignment = TextAnchor.MiddleCenter;
            buyVlg.childControlWidth = true;
            buyVlg.childControlHeight = true;
            buyVlg.childForceExpandHeight = false;

            GameObject origObj = CreateChild(buyContainer, "OriginalPrice");
            TextMeshProUGUI origText = origObj.AddComponent<TextMeshProUGUI>();
            origText.text = origPrice;
            origText.fontSize = FontSizes.Body;
            origText.color = TEXT_MUTED;
            origText.alignment = TextAlignmentOptions.Center;
            LayoutElement origLE = origObj.AddComponent<LayoutElement>();
            origLE.minHeight = 26;

            CreatePriceButton(buyContainer, price, BUTTON_SUCCESS, TEXT_DARK, 52, FontSizes.Body);
        }

        // ==================== DAILY DEALS ====================

        private static void CreateDailyDealsSection(GameObject parent)
        {
            GameObject section = CreateChild(parent, "DailyDealsSection");

            VerticalLayoutGroup vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Header divider estilo BetSelection + timer
            GameObject headerRow = CreateChild(section, "Header");
            LayoutElement headerLE = headerRow.AddComponent<LayoutElement>();
            headerLE.minHeight = SECTION_DIVIDER_HEIGHT;
            headerLE.preferredHeight = SECTION_DIVIDER_HEIGHT;

            HorizontalLayoutGroup headerHlg = headerRow.AddComponent<HorizontalLayoutGroup>();
            headerHlg.padding = new RectOffset(0, 0, 0, 0);
            headerHlg.spacing = 20;
            headerHlg.childAlignment = TextAnchor.MiddleCenter;
            headerHlg.childControlWidth = true;
            headerHlg.childControlHeight = false;
            headerHlg.childForceExpandWidth = true;
            headerHlg.childForceExpandHeight = false;

            // Left line
            GameObject lineL = CreateChild(headerRow, "LineLeft");
            LayoutElement llLE = lineL.AddComponent<LayoutElement>();
            llLE.flexibleWidth = 1;
            llLE.preferredHeight = 2;
            lineL.AddComponent<Image>().color = new Color(ORANGE_HOT.r, ORANGE_HOT.g, ORANGE_HOT.b, 0.35f);

            // Title
            GameObject titleObj = CreateChild(headerRow, "SectionTitle");
            LayoutElement titleLE = titleObj.AddComponent<LayoutElement>();
            titleLE.preferredHeight = SECTION_DIVIDER_HEIGHT;
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "DAILY OFFERS";
            titleText.fontSize = FontSizes.H4;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = ORANGE_HOT;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.enableWordWrapping = false;

            // Right line
            GameObject lineR = CreateChild(headerRow, "LineRight");
            LayoutElement lrLE = lineR.AddComponent<LayoutElement>();
            lrLE.flexibleWidth = 1;
            lrLE.preferredHeight = 2;
            lineR.AddComponent<Image>().color = new Color(ORANGE_HOT.r, ORANGE_HOT.g, ORANGE_HOT.b, 0.35f);

            // Timer (after right line)
            GameObject timerObj = CreateChild(headerRow, "Timer");
            LayoutElement timerLE = timerObj.AddComponent<LayoutElement>();
            timerLE.preferredHeight = SECTION_DIVIDER_HEIGHT;
            timerLE.minWidth = 130;
            TextMeshProUGUI timerText = timerObj.AddComponent<TextMeshProUGUI>();
            timerText.text = "12:34:56";
            timerText.fontSize = FontSizes.Body;
            timerText.color = TEXT_SECONDARY;
            timerText.alignment = TextAlignmentOptions.MidlineRight;

            // Items container
            GameObject itemsContainer = CreateChild(section, "Items");
            LayoutElement itemsLE = itemsContainer.AddComponent<LayoutElement>();
            itemsLE.minHeight = DAILY_ITEM_HEIGHT;

            HorizontalLayoutGroup itemsHlg = itemsContainer.AddComponent<HorizontalLayoutGroup>();
            itemsHlg.spacing = GRID_SPACING;
            itemsHlg.childAlignment = TextAnchor.MiddleCenter;
            itemsHlg.childControlWidth = true;
            itemsHlg.childControlHeight = true;
            itemsHlg.childForceExpandWidth = true;

            // 3 Daily Items
            CreateDailyItem(itemsContainer, "Daily_Free", "200 DigitGems", "FREE", GEM_COLOR, true, "1x DAILY");
            CreateDailyItem(itemsContainer, "Daily_Gems", "25 DigitGems", "100", GEM_COLOR, false, "HOT");
            CreateDailyItem(itemsContainer, "Daily_Coins", "5,000 DigitCoins", "50", COIN_COLOR, false, "");

            Debug.Log("[ShopPremiumUIBuilder] Daily Deals V3 creado");
        }

        private static void CreateDailyItem(GameObject parent, string name, string itemName,
            string price, Color iconColor, bool isFree, string badgeText)
        {
            GameObject item = CreateChild(parent, name);

            Image itemBg = item.AddComponent<Image>();
            itemBg.color = CARD_BG;
            AddOutline(item, isFree ? GREEN_FREE : iconColor * 0.5f, isFree ? 3 : 1);

            // Subtle shadow for depth
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

            // Badge
            if (!string.IsNullOrEmpty(badgeText))
            {
                Color badgeColor = isFree ? GREEN_FREE : ORANGE_HOT;
                CreateInlineBadge(item, badgeText, badgeColor, 0);
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
            LayoutElement nameLE = nameObj.AddComponent<LayoutElement>();
            nameLE.minHeight = 30;

            // Price button
            Color priceBtnColor = isFree ? GREEN_FREE : GEM_COLOR;
            CreatePriceButton(item, price, priceBtnColor, TEXT_DARK, 46, FontSizes.Body);
        }

        // ==================== DIGITGEMS SECTION ====================

        private static void CreateGemsSection(GameObject parent)
        {
            GameObject section = CreateChild(parent, "GemsSection");

            VerticalLayoutGroup vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            CreateSectionHeader(section, "DIGITGEMS", GEM_COLOR);

            // Grid
            GameObject grid = CreateChild(section, "GemsGrid");

            GridLayoutGroup glg = grid.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(GRID_CELL_W, GRID_GEM_COIN_H);
            glg.spacing = new Vector2(GRID_SPACING, GRID_SPACING);
            glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            glg.startAxis = GridLayoutGroup.Axis.Horizontal;
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = GRID_COLUMNS;

            LayoutElement gridLE = grid.AddComponent<LayoutElement>();
            gridLE.minHeight = GRID_GEM_COIN_H * 2 + GRID_SPACING;

            // 6 DigitGem packs - neon coherent color tiers
            CreateShopCard(grid, "Gems_100", "100", "$0.99", "", CYAN_DARK, "", false);
            CreateShopCard(grid, "Gems_500", "500", "$4.99", "+10%", CYAN_DARK, "", false);
            CreateShopCard(grid, "Gems_1200", "1,200", "$9.99", "+20%", new Color(0f, 1f, 1f, 0.7f), "BEST VALUE", false);
            CreateShopCard(grid, "Gems_2500", "2,500", "$19.99", "+25%", new Color(0f, 1f, 1f, 0.7f), "", false);
            CreateShopCard(grid, "Gems_6500", "6,500", "$49.99", "+30%", new Color(0.24f, 1f, 0.42f, 0.7f), "POPULAR", false);
            CreateShopCard(grid, "Gems_14000", "14,000", "$99.99", "+40%", new Color(1f, 0.84f, 0f, 0.7f), "MEGA PACK", false);

            Debug.Log("[ShopPremiumUIBuilder] DigitGems Section V3 creado");
        }

        // ==================== DIGITCOINS SECTION ====================

        private static void CreateCoinsSection(GameObject parent)
        {
            GameObject section = CreateChild(parent, "CoinsSection");

            VerticalLayoutGroup vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            CreateSectionHeader(section, "DIGITCOINS", COIN_COLOR);

            // Grid
            GameObject grid = CreateChild(section, "CoinsGrid");

            GridLayoutGroup glg = grid.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(GRID_CELL_W, GRID_GEM_COIN_H);
            glg.spacing = new Vector2(GRID_SPACING, GRID_SPACING);
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = GRID_COLUMNS;

            LayoutElement gridLE = grid.AddComponent<LayoutElement>();
            gridLE.minHeight = GRID_GEM_COIN_H * 2 + GRID_SPACING;

            // 4 DigitCoin packs (bought with DigitGems)
            CreateShopCard(grid, "Coins_1000", "1,000", "50", "", COIN_COLOR, "", true);
            CreateShopCard(grid, "Coins_5000", "5,000", "200", "+25%", COIN_COLOR, "", true);
            CreateShopCard(grid, "Coins_15000", "15,000", "500", "+50%", COIN_COLOR, "BEST VALUE", true);
            CreateShopCard(grid, "Coins_50000", "50,000", "1,500", "+75%", COIN_COLOR, "MEGA PACK", true);

            Debug.Log("[ShopPremiumUIBuilder] DigitCoins Section V3 creado");
        }

        // ==================== THEMES SECTION ====================

        private static void CreateThemesSection(GameObject parent)
        {
            GameObject section = CreateChild(parent, "ThemesSection");

            VerticalLayoutGroup vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            CreateSectionHeader(section, "THEMES", PURPLE_PREMIUM);

            // Grid
            GameObject grid = CreateChild(section, "ThemesGrid");

            GridLayoutGroup glg = grid.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(GRID_CELL_W, GRID_THEME_H);
            glg.spacing = new Vector2(GRID_SPACING, GRID_SPACING);
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = GRID_COLUMNS;

            LayoutElement gridLE = grid.AddComponent<LayoutElement>();
            gridLE.minHeight = GRID_THEME_H * 3 + GRID_SPACING * 2;

            // 9 Theme items
            CreateThemeCard(grid, "Theme_Neon", "Neon Cyan", "EQUIPPED", CYAN_NEON, true);
            CreateThemeCard(grid, "Theme_Gold", "Royal Gold", "500", GOLD, false);
            CreateThemeCard(grid, "Theme_Purple", "Amethyst", "500", PURPLE_PREMIUM, false);
            CreateThemeCard(grid, "Theme_Red", "Ruby", "750", new Color(1f, 0.2f, 0.3f, 1f), false);
            CreateThemeCard(grid, "Theme_Green", "Emerald", "750", new Color(0.2f, 0.9f, 0.4f, 1f), false);
            CreateThemeCard(grid, "Theme_Blue", "Sapphire", "750", new Color(0.2f, 0.5f, 1f, 1f), false);
            CreateThemeCard(grid, "Theme_Orange", "Amber", "1,000", ORANGE_HOT, false);
            CreateThemeCard(grid, "Theme_Pink", "Sakura", "1,000", new Color(1f, 0.5f, 0.7f, 1f), false);
            CreateThemeCard(grid, "Theme_Rainbow", "Rainbow", "$2.99", new Color(1f, 0.5f, 0.8f, 1f), false);

            Debug.Log("[ShopPremiumUIBuilder] Themes Section V3 creado (9 items)");
        }

        // ==================== FRAMES SECTION ====================

        private static void CreateFramesSection(GameObject parent)
        {
            GameObject section = CreateChild(parent, "FramesSection");

            VerticalLayoutGroup vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            CreateSectionHeader(section, "FRAMES", FRAME_COLOR);

            // Grid
            GameObject grid = CreateChild(section, "FramesGrid");

            GridLayoutGroup glg = grid.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(GRID_CELL_W, GRID_COSMETIC_H);
            glg.spacing = new Vector2(GRID_SPACING, GRID_SPACING);
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = GRID_COLUMNS;

            LayoutElement gridLE = grid.AddComponent<LayoutElement>();
            gridLE.minHeight = GRID_COSMETIC_H * 3 + GRID_SPACING * 2;

            // 8 Frame items
            CreateCosmeticCard(grid, "Frame_Basic", "Basic", "EQUIPPED", FRAME_COLOR, true);
            CreateCosmeticCard(grid, "Frame_Bronze", "Bronze", "200", new Color(0.8f, 0.5f, 0.2f, 1f), false);
            CreateCosmeticCard(grid, "Frame_Silver", "Silver", "400", new Color(0.75f, 0.75f, 0.8f, 1f), false);
            CreateCosmeticCard(grid, "Frame_Gold", "Gold", "800", GOLD, false);
            CreateCosmeticCard(grid, "Frame_Platinum", "Platinum", "1,200", new Color(0.8f, 0.85f, 0.9f, 1f), false);
            CreateCosmeticCard(grid, "Frame_Diamond", "Diamond", "2,000", GEM_COLOR, false);
            CreateCosmeticCard(grid, "Frame_Fire", "Fire", "1,500", new Color(1f, 0.35f, 0.1f, 1f), false);
            CreateCosmeticCard(grid, "Frame_Ice", "Ice", "1,500", new Color(0.5f, 0.8f, 1f, 1f), false);

            Debug.Log("[ShopPremiumUIBuilder] Frames Section V3 creado (8 items)");
        }

        // ==================== GEM FRAMES SECTION ====================

        private static void CreateGemFramesSection(GameObject parent)
        {
            GameObject section = CreateChild(parent, "GemFramesSection");

            VerticalLayoutGroup vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            CreateSectionHeader(section, "DIGITGEM FRAMES", GEM_COLOR);

            // Grid
            GameObject grid = CreateChild(section, "GemFramesGrid");

            GridLayoutGroup glg = grid.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(GRID_CELL_W, GRID_COSMETIC_H);
            glg.spacing = new Vector2(GRID_SPACING, GRID_SPACING);
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = GRID_COLUMNS;

            LayoutElement gridLE = grid.AddComponent<LayoutElement>();
            gridLE.minHeight = GRID_COSMETIC_H * 2 + GRID_SPACING;

            // 6 DigitGem frame items (bought with DigitGems)
            CreateCosmeticCard(grid, "GemFrame_Sapphire", "Sapphire", "100", new Color(0.2f, 0.4f, 1f, 1f), false);
            CreateCosmeticCard(grid, "GemFrame_Emerald", "Emerald", "150", new Color(0.2f, 0.8f, 0.4f, 1f), false);
            CreateCosmeticCard(grid, "GemFrame_Ruby", "Ruby", "200", new Color(1f, 0.2f, 0.3f, 1f), false);
            CreateCosmeticCard(grid, "GemFrame_Amethyst", "Amethyst", "250", PURPLE_LIGHT, false);
            CreateCosmeticCard(grid, "GemFrame_Topaz", "Topaz", "300", new Color(1f, 0.8f, 0.2f, 1f), false);
            CreateCosmeticCard(grid, "GemFrame_Obsidian", "Obsidian", "500", new Color(0.3f, 0.25f, 0.35f, 1f), false);

            Debug.Log("[ShopPremiumUIBuilder] DigitGem Frames Section V3 creado (6 items)");
        }

        // ==================== PREMIUM FRAMES SECTION ====================

        private static void CreatePremiumFramesSection(GameObject parent)
        {
            GameObject section = CreateChild(parent, "PremiumFramesSection");

            VerticalLayoutGroup vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            CreateSectionHeader(section, "PREMIUM FRAMES", PURPLE_PREMIUM);

            // Grid
            GameObject grid = CreateChild(section, "PremiumFramesGrid");

            GridLayoutGroup glg = grid.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(GRID_CELL_W, GRID_COSMETIC_H);
            glg.spacing = new Vector2(GRID_SPACING, GRID_SPACING);
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = GRID_COLUMNS;

            LayoutElement gridLE = grid.AddComponent<LayoutElement>();
            gridLE.minHeight = GRID_COSMETIC_H;

            // 3 Premium frame items (real money)
            CreateCosmeticCard(grid, "PremFrame_Legendary", "Legendary", "$1.99", GOLD, false);
            CreateCosmeticCard(grid, "PremFrame_Mythic", "Mythic", "$2.99", PURPLE_PREMIUM, false);
            CreateCosmeticCard(grid, "PremFrame_Celestial", "Celestial", "$4.99", CYAN_NEON, false);

            Debug.Log("[ShopPremiumUIBuilder] Premium Frames Section V3 creado (3 items)");
        }

        // ==================== TITLES SECTION ====================

        private static void CreateTitlesSection(GameObject parent)
        {
            GameObject section = CreateChild(parent, "TitlesSection");

            VerticalLayoutGroup vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            CreateSectionHeader(section, "TITLES", TITLE_COLOR);

            // Grid (2 columns, wider cards)
            GameObject grid = CreateChild(section, "TitlesGrid");

            GridLayoutGroup glg = grid.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(TITLE_CELL_W, TITLE_CELL_H);
            glg.spacing = new Vector2(GRID_SPACING, GRID_SPACING);
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = TITLE_COLUMNS;

            LayoutElement gridLE = grid.AddComponent<LayoutElement>();
            gridLE.minHeight = TITLE_CELL_H * 5 + GRID_SPACING * 4;

            // 10 Title items
            CreateTitleCard(grid, "Title_Novato", "Rookie", "EQUIPPED", TEXT_SECONDARY, true);
            CreateTitleCard(grid, "Title_Veterano", "Veteran", "500", new Color(0.6f, 0.8f, 0.6f, 1f), false);
            CreateTitleCard(grid, "Title_Campeon", "Champion", "1,000", GOLD, false);
            CreateTitleCard(grid, "Title_Leyenda", "Legend", "2,000", PURPLE_LIGHT, false);
            CreateTitleCard(grid, "Title_Maestro", "Grand Master", "3,000", ORANGE_HOT, false);
            CreateTitleCard(grid, "Title_Genio", "Digital Genius", "150", GEM_COLOR, false);
            CreateTitleCard(grid, "Title_Flash", "Speedster", "100", new Color(1f, 0.9f, 0.2f, 1f), false);
            CreateTitleCard(grid, "Title_Memoria", "Bright Mind", "200", new Color(0.5f, 1f, 0.8f, 1f), false);
            CreateTitleCard(grid, "Title_Elite", "Elite", "$0.99", RED_URGENT, false);
            CreateTitleCard(grid, "Title_Inmortal", "Immortal", "$1.99", new Color(1f, 0.85f, 0.5f, 1f), false);

            Debug.Log("[ShopPremiumUIBuilder] Titles Section V3 creado (10 items)");
        }

        private static void CreateTitleCard(GameObject parent, string name, string displayName,
            string price, Color titleColor, bool isEquipped)
        {
            GameObject item = CreateChild(parent, name);

            Image itemBg = item.AddComponent<Image>();
            itemBg.color = CARD_BG;
            AddOutline(item, isEquipped ? titleColor : titleColor * 0.4f, isEquipped ? 2 : 1);

            // Subtle shadow for depth
            Shadow itemShadow = item.AddComponent<Shadow>();
            itemShadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            itemShadow.effectDistance = new Vector2(3, -4);

            Button btn = item.AddComponent<Button>();
            SetupButton(btn, CARD_BG);

            HorizontalLayoutGroup hlg = item.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.padding = new RectOffset(16, 16, 10, 10);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            // Title name
            GameObject nameObj = CreateChild(item, "TitleName");
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = displayName;
            nameText.fontSize = FontSizes.Body;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = titleColor;
            nameText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement nameLE = nameObj.AddComponent<LayoutElement>();
            nameLE.flexibleWidth = 1;

            // Price/Status
            if (isEquipped)
            {
                GameObject statusObj = CreateChild(item, "Status");
                TextMeshProUGUI statusText = statusObj.AddComponent<TextMeshProUGUI>();
                statusText.text = "EQUIPPED";
                statusText.fontSize = FontSizes.Body;
                statusText.fontStyle = FontStyles.Bold;
                statusText.color = TEXT_SECONDARY;
                statusText.alignment = TextAlignmentOptions.MidlineRight;
                LayoutElement statusLE = statusObj.AddComponent<LayoutElement>();
                statusLE.minWidth = 120;
            }
            else
            {
                // Price button inline
                GameObject priceContainer = CreateChild(item, "PriceContainer");
                LayoutElement pcLE = priceContainer.AddComponent<LayoutElement>();
                pcLE.minWidth = 120;
                pcLE.preferredWidth = 120;

                bool isGemPrice = !price.StartsWith("$") && !price.Equals("FREE");
                Color btnColor = price.StartsWith("$") ? BUTTON_SUCCESS : GEM_COLOR;

                GameObject priceBtn = CreateChild(priceContainer, "PriceButton");
                RectTransform priceBtnRT = priceBtn.GetComponent<RectTransform>();
                priceBtnRT.anchorMin = new Vector2(0.5f, 0.5f);
                priceBtnRT.anchorMax = new Vector2(0.5f, 0.5f);
                priceBtnRT.sizeDelta = new Vector2(110, 40);

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
                LayoutElement ptLE = priceText.AddComponent<LayoutElement>();
                ptLE.flexibleWidth = 1;
            }
        }

        // ==================== VIP SECTION ====================

        private static void CreateVIPSection(GameObject parent)
        {
            GameObject section = CreateChild(parent, "VIPSection");

            LayoutElement sectionLE = section.AddComponent<LayoutElement>();
            sectionLE.minHeight = VIP_HEIGHT;

            Image sectionBg = section.AddComponent<Image>();
            sectionBg.color = new Color(0.1f, 0.05f, 0.18f, 1f);
            AddOutline(section, GOLD, 3);

            // Shadow (child element for 3D depth)
            GameObject shadowObj = CreateChild(section, "Shadow");
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

            // Side (3D depth strip below banner)
            GameObject sideObj = CreateChild(section, "Side");
            sideObj.transform.SetSiblingIndex(1);
            RectTransform sideRT = sideObj.GetComponent<RectTransform>();
            sideRT.anchorMin = new Vector2(0, 0);
            sideRT.anchorMax = new Vector2(1, 0);
            sideRT.offsetMin = new Vector2(0, -8);
            sideRT.offsetMax = new Vector2(0, 0);
            Image sideImg = sideObj.AddComponent<Image>();
            sideImg.color = new Color(GOLD.r * 0.3f, GOLD.g * 0.3f, GOLD.b * 0.3f, 1f);
            sideImg.raycastTarget = false;
            LayoutElement sideLE = sideObj.AddComponent<LayoutElement>();
            sideLE.ignoreLayout = true;

            HorizontalLayoutGroup hlg = section.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.padding = new RectOffset(24, 24, 18, 18);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            // Icon
            GameObject iconContainer = CreateChild(section, "IconContainer");
            LayoutElement iconLE = iconContainer.AddComponent<LayoutElement>();
            iconLE.minWidth = 90;

            GameObject icon = CreateChild(iconContainer, "Icon");
            RectTransform iconRT = icon.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.sizeDelta = new Vector2(75, 75);
            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = GOLD;

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

            GameObject title = CreateChild(info, "Title");
            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "BUNDLE PREMIUM";
            titleText.fontSize = FontSizes.Body;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GOLD;
            LayoutElement titleLE = title.AddComponent<LayoutElement>();
            titleLE.minHeight = 36;

            GameObject desc = CreateChild(info, "Description");
            TextMeshProUGUI descText = desc.AddComponent<TextMeshProUGUI>();
            descText.text = "50 levels of exclusive rewards";
            descText.fontSize = FontSizes.Body;
            descText.color = TEXT_SECONDARY;
            LayoutElement descLE = desc.AddComponent<LayoutElement>();
            descLE.minHeight = 22;

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

            Debug.Log("[ShopPremiumUIBuilder] VIP Section V3 creado");
        }

        // ==================== CARD BUILDERS ====================

        /// <summary>
        /// Card para items de DigitGems/DigitCoins (330x280)
        /// </summary>
        private static void CreateShopCard(GameObject parent, string name, string amount,
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
            else if (badge == "MEGA PACK")
            {
                outlineColor = ORANGE_HOT;
                outlineWidth = 3;
            }

            AddOutline(item, outlineColor, outlineWidth);

            // Subtle shadow for depth
            Shadow itemShadow = item.AddComponent<Shadow>();
            itemShadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            itemShadow.effectDistance = new Vector2(3, -4);

            Button btn = item.AddComponent<Button>();
            SetupButton(btn, CARD_BG);

            VerticalLayoutGroup vlg = item.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6;
            vlg.padding = new RectOffset(12, 12, 12, 12);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Badge
            if (hasBadge)
            {
                Color badgeColor = GOLD;
                if (badge == "BEST VALUE") badgeColor = GREEN_FREE;
                else if (badge == "MEGA PACK") badgeColor = ORANGE_HOT;

                CreateInlineBadge(item, badge, badgeColor, 0);
            }

            // Icon
            GameObject icon = CreateChild(item, "Icon");
            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = color;
            iconImg.raycastTarget = false;
            LayoutElement iconLE = icon.AddComponent<LayoutElement>();
            iconLE.minHeight = 65;
            iconLE.preferredHeight = 65;
            iconLE.minWidth = 65;
            iconLE.preferredWidth = 65;

            // Amount
            GameObject amountObj = CreateChild(item, "Amount");
            TextMeshProUGUI amountText = amountObj.AddComponent<TextMeshProUGUI>();
            amountText.text = amount;
            amountText.fontSize = FontSizes.BodyLarge;
            amountText.fontStyle = FontStyles.Bold;
            amountText.color = color;
            amountText.alignment = TextAlignmentOptions.Center;
            LayoutElement amountLE = amountObj.AddComponent<LayoutElement>();
            amountLE.minHeight = 38;

            // Bonus
            if (!string.IsNullOrEmpty(bonus))
            {
                GameObject bonusObj = CreateChild(item, "Bonus");
                TextMeshProUGUI bonusText = bonusObj.AddComponent<TextMeshProUGUI>();
                bonusText.text = bonus + " BONUS";
                bonusText.fontSize = FontSizes.Body;
                bonusText.fontStyle = FontStyles.Bold;
                bonusText.color = BUTTON_SUCCESS;
                bonusText.alignment = TextAlignmentOptions.Center;
                LayoutElement bonusLE = bonusObj.AddComponent<LayoutElement>();
                bonusLE.minHeight = 22;
            }

            // Price button
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
            LayoutElement ptLE = priceText.AddComponent<LayoutElement>();
            ptLE.flexibleWidth = 1;
        }

        /// <summary>
        /// Card para Temas (330x300) con preview de color
        /// </summary>
        private static void CreateThemeCard(GameObject parent, string name, string displayName,
            string price, Color themeColor, bool isEquipped)
        {
            GameObject item = CreateChild(parent, name);

            Image itemBg = item.AddComponent<Image>();
            itemBg.color = CARD_BG;
            AddOutline(item, isEquipped ? themeColor : themeColor * 0.4f, isEquipped ? 3 : 1);

            // Subtle shadow for depth
            Shadow itemShadow = item.AddComponent<Shadow>();
            itemShadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            itemShadow.effectDistance = new Vector2(3, -4);

            Button btn = item.AddComponent<Button>();
            SetupButton(btn, CARD_BG);

            VerticalLayoutGroup vlg = item.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6;
            vlg.padding = new RectOffset(12, 12, 12, 12);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            if (isEquipped)
            {
                CreateInlineBadge(item, "EQUIPPED", themeColor, 0);
            }

            // Theme preview (color swatch)
            GameObject preview = CreateChild(item, "Preview");
            Image previewImg = preview.AddComponent<Image>();
            previewImg.color = themeColor;
            previewImg.raycastTarget = false;
            LayoutElement previewLE = preview.AddComponent<LayoutElement>();
            previewLE.minHeight = 90;
            previewLE.preferredHeight = 90;
            AddOutline(preview, themeColor * 0.5f, 3);

            // Name
            GameObject nameObj = CreateChild(item, "Name");
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = displayName;
            nameText.fontSize = FontSizes.Body;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = themeColor;
            nameText.alignment = TextAlignmentOptions.Center;
            LayoutElement nameLE = nameObj.AddComponent<LayoutElement>();
            nameLE.minHeight = 30;

            // Price/Action button
            if (isEquipped)
            {
                GameObject statusObj = CreateChild(item, "Status");
                TextMeshProUGUI statusText = statusObj.AddComponent<TextMeshProUGUI>();
                statusText.text = "IN USE";
                statusText.fontSize = FontSizes.Body;
                statusText.color = TEXT_SECONDARY;
                statusText.alignment = TextAlignmentOptions.Center;
                LayoutElement statusLE = statusObj.AddComponent<LayoutElement>();
                statusLE.minHeight = PRICE_BTN_HEIGHT;
            }
            else
            {
                bool isGemPrice = !price.StartsWith("$") && !price.Equals("FREE");
                Color btnColor = price.StartsWith("$") ? BUTTON_SUCCESS : (isGemPrice ? GEM_COLOR : GREEN_FREE);

                GameObject priceBtn = CreateChild(item, "PriceButton");
                Image priceBg = priceBtn.AddComponent<Image>();
                priceBg.color = btnColor;
                AddOutline(priceBtn, btnColor * 1.2f, 1);
                LayoutElement priceLE = priceBtn.AddComponent<LayoutElement>();
                priceLE.minHeight = PRICE_BTN_HEIGHT;
                priceLE.preferredHeight = PRICE_BTN_HEIGHT;

                HorizontalLayoutGroup priceHlg = priceBtn.AddComponent<HorizontalLayoutGroup>();
                priceHlg.spacing = 6;
                priceHlg.padding = new RectOffset(14, 14, 6, 6);
                priceHlg.childAlignment = TextAnchor.MiddleCenter;
                priceHlg.childControlWidth = false;
                priceHlg.childControlHeight = true;

                if (isGemPrice)
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
                LayoutElement ptLE = priceText.AddComponent<LayoutElement>();
                ptLE.flexibleWidth = 1;
            }
        }

        /// <summary>
        /// Card para Marcos/Cosmeticos (330x240)
        /// </summary>
        private static void CreateCosmeticCard(GameObject parent, string name, string displayName,
            string price, Color itemColor, bool isEquipped)
        {
            GameObject item = CreateChild(parent, name);

            Image itemBg = item.AddComponent<Image>();
            itemBg.color = CARD_BG;
            AddOutline(item, isEquipped ? itemColor : itemColor * 0.4f, isEquipped ? 2 : 1);

            // Subtle shadow for depth
            Shadow itemShadow = item.AddComponent<Shadow>();
            itemShadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            itemShadow.effectDistance = new Vector2(3, -4);

            Button btn = item.AddComponent<Button>();
            SetupButton(btn, CARD_BG);

            VerticalLayoutGroup vlg = item.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6;
            vlg.padding = new RectOffset(12, 12, 10, 10);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            if (isEquipped)
            {
                CreateInlineBadge(item, "EQUIPPED", itemColor, 0);
            }

            // Icon/Preview
            GameObject icon = CreateChild(item, "Icon");
            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = itemColor;
            iconImg.raycastTarget = false;
            LayoutElement iconLE = icon.AddComponent<LayoutElement>();
            iconLE.minHeight = 60;
            iconLE.preferredHeight = 60;
            iconLE.minWidth = 60;
            iconLE.preferredWidth = 60;

            // Name
            GameObject nameObj = CreateChild(item, "Name");
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = displayName;
            nameText.fontSize = FontSizes.Body;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = itemColor;
            nameText.alignment = TextAlignmentOptions.Center;
            LayoutElement nameLE = nameObj.AddComponent<LayoutElement>();
            nameLE.minHeight = 30;

            // Price
            if (isEquipped)
            {
                GameObject statusObj = CreateChild(item, "Status");
                TextMeshProUGUI statusText = statusObj.AddComponent<TextMeshProUGUI>();
                statusText.text = "IN USE";
                statusText.fontSize = FontSizes.Body;
                statusText.color = TEXT_SECONDARY;
                statusText.alignment = TextAlignmentOptions.Center;
                LayoutElement statusLE = statusObj.AddComponent<LayoutElement>();
                statusLE.minHeight = 40;
            }
            else
            {
                bool isGemPrice = !price.StartsWith("$") && !price.Equals("EQUIPPED") && !price.Equals("FREE");
                Color btnColor = price.StartsWith("$") ? BUTTON_SUCCESS : (isGemPrice ? GEM_COLOR : GREEN_FREE);

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

                if (isGemPrice)
                {
                    GameObject gemIcon = CreateChild(priceBtn, "GemIcon");
                    Image gemImg = gemIcon.AddComponent<Image>();
                    gemImg.color = TEXT_DARK;
                    LayoutElement gemLE = gemIcon.AddComponent<LayoutElement>();
                    gemLE.minWidth = 20;
                    gemLE.minHeight = 20;
                }

                GameObject priceText = CreateChild(priceBtn, "Text");
                TextMeshProUGUI pt = priceText.AddComponent<TextMeshProUGUI>();
                pt.text = price;
                pt.fontSize = FontSizes.Body;
                pt.fontStyle = FontStyles.Bold;
                pt.color = TEXT_DARK;
                pt.alignment = TextAlignmentOptions.Center;
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
            GameObject title = CreateChild(popup, "Title");
            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "Confirm Purchase";
            titleText.fontSize = FontSizes.H4;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = CYAN_NEON;
            titleText.alignment = TextAlignmentOptions.Center;
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
            LayoutElement amountLE = previewAmount.AddComponent<LayoutElement>();
            amountLE.minWidth = 220;

            // Price
            GameObject priceObj = CreateChild(popup, "Price");
            TextMeshProUGUI priceText = priceObj.AddComponent<TextMeshProUGUI>();
            priceText.text = "Price: $9.99";
            priceText.fontSize = FontSizes.Body;
            priceText.color = TEXT_SECONDARY;
            priceText.alignment = TextAlignmentOptions.Center;
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

            Debug.Log("[ShopPremiumUIBuilder] PurchasePopup V3 creado");
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
            popupRT.sizeDelta = new Vector2(500, 420);

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
            GameObject title = CreateChild(popup, "Title");
            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "Insufficient DigitGems";
            titleText.fontSize = FontSizes.H4;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GEM_COLOR;
            titleText.alignment = TextAlignmentOptions.Center;
            LayoutElement titleLE = title.AddComponent<LayoutElement>();
            titleLE.minHeight = 52;

            // Message
            GameObject msg = CreateChild(popup, "Message");
            TextMeshProUGUI msgText = msg.AddComponent<TextMeshProUGUI>();
            msgText.text = "You don't have enough DigitGems.\nGet more in the shop!";
            msgText.fontSize = FontSizes.Body;
            msgText.color = TEXT_SECONDARY;
            msgText.alignment = TextAlignmentOptions.Center;
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

            Debug.Log("[ShopPremiumUIBuilder] NotEnoughPopup V3 creado");
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

            GameObject textObj = CreateChild(btn, "Text");
            SetStretch(textObj);
            TextMeshProUGUI txt = textObj.AddComponent<TextMeshProUGUI>();
            txt.text = text;
            txt.fontSize = FontSizes.Body;
            txt.fontStyle = FontStyles.Bold;
            txt.color = textColor;
            txt.alignment = TextAlignmentOptions.Center;
        }

        // ==================== SHOP MANAGER ====================

        private static void AddShopManager(Canvas canvas)
        {
            var existing = Object.FindObjectOfType<Monetization.ShopManager>();
            if (existing != null)
            {
                Debug.Log($"[ShopPremiumUIBuilder] ShopManager ya existe en '{existing.gameObject.name}', no se duplica");
                return;
            }

            canvas.gameObject.AddComponent<Monetization.ShopManager>();
            Debug.Log("[ShopPremiumUIBuilder] ShopManager agregado al Canvas");
        }

        private static void AutoAssignReferences()
        {
            Debug.Log("[ShopPremiumUIBuilder] Auto-asignando referencias...");
            ShopReferenceAssigner.ResetLog();
            ShopReferenceAssigner.AssignAllReferences();
            Debug.Log("[ShopPremiumUIBuilder] Referencias auto-asignadas");
        }

        // ==================== HELPER: SECTION HEADER ====================

        /// <summary>
        /// Section divider estilo BetSelection: [--- linea ---] TITULO [--- linea ---]
        /// Elegante, centrado, prominente.
        /// </summary>
        private static void CreateSectionHeader(GameObject parent, string title, Color color)
        {
            GameObject divider = CreateChild(parent, title.Replace(" ", "") + "Divider");
            LayoutElement divLE = divider.AddComponent<LayoutElement>();
            divLE.minHeight = SECTION_DIVIDER_HEIGHT;
            divLE.preferredHeight = SECTION_DIVIDER_HEIGHT;

            HorizontalLayoutGroup hlg = divider.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(0, 0, 0, 0);
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;

            // Left line
            GameObject lineL = CreateChild(divider, "LineLeft");
            LayoutElement llLE = lineL.AddComponent<LayoutElement>();
            llLE.flexibleWidth = 1;
            llLE.preferredHeight = 2;
            Image lineLeftImg = lineL.AddComponent<Image>();
            lineLeftImg.color = new Color(color.r, color.g, color.b, 0.35f);

            // Section title (centered, very prominent)
            GameObject titleObj = CreateChild(divider, "SectionTitle");
            LayoutElement tLE = titleObj.AddComponent<LayoutElement>();
            tLE.preferredHeight = SECTION_DIVIDER_HEIGHT;
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = title;
            titleText.fontSize = FontSizes.H4;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = color;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.enableWordWrapping = false;

            // Right line
            GameObject lineR = CreateChild(divider, "LineRight");
            LayoutElement lrLE = lineR.AddComponent<LayoutElement>();
            lrLE.flexibleWidth = 1;
            lrLE.preferredHeight = 2;
            Image lineRightImg = lineR.AddComponent<Image>();
            lineRightImg.color = new Color(color.r, color.g, color.b, 0.35f);
        }

        // ==================== HELPER: INLINE BADGE ====================

        private static void CreateInlineBadge(GameObject parent, string text, Color accentColor, float width)
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

            GameObject badgeText = CreateChild(badge, "Text");
            SetStretch(badgeText);
            TextMeshProUGUI bt = badgeText.AddComponent<TextMeshProUGUI>();
            bt.text = text;
            bt.fontSize = FontSizes.Body;
            bt.fontStyle = FontStyles.Bold;
            bt.color = accentColor;
            bt.alignment = TextAlignmentOptions.Center;
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
