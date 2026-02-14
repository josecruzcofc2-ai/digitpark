using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Shop Premium UI Builder - Estilo Clash Royale
    /// Scroll vertical continuo con todas las secciones visibles
    /// Diseño TOP 10 iOS
    /// </summary>
    public class ShopPremiumUIBuilder : EditorWindow
    {
        // ==================== COLORES PREMIUM ====================
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.4f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 1f, 1f, 0.3f);

        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color PANEL_BG = new Color(0.06f, 0.1f, 0.16f, 0.98f);
        private static readonly Color CARD_BG = new Color(0.08f, 0.12f, 0.18f, 1f);
        private static readonly Color CARD_HIGHLIGHT = new Color(0.1f, 0.15f, 0.22f, 1f);
        private static readonly Color HEADER_BG = new Color(0.02f, 0.04f, 0.08f, 0.98f);
        private static readonly Color SECTION_BG = new Color(0.04f, 0.07f, 0.12f, 0.95f);

        private static readonly Color TEXT_PRIMARY = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.7f, 0.8f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color TEXT_MUTED = new Color(0.4f, 0.5f, 0.6f, 1f);

        private static readonly Color BUTTON_SUCCESS = new Color(0.2f, 0.85f, 0.4f, 1f);
        private static readonly Color BUTTON_SECONDARY = new Color(0.15f, 0.2f, 0.28f, 1f);

        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GOLD_LIGHT = new Color(1f, 0.92f, 0.5f, 1f);
        private static readonly Color PURPLE_PREMIUM = new Color(0.6f, 0.3f, 0.95f, 1f);
        private static readonly Color PURPLE_LIGHT = new Color(0.75f, 0.5f, 1f, 1f);
        private static readonly Color ORANGE_HOT = new Color(1f, 0.45f, 0.1f, 1f);
        private static readonly Color RED_URGENT = new Color(1f, 0.25f, 0.25f, 1f);
        private static readonly Color GREEN_FREE = new Color(0.3f, 0.9f, 0.4f, 1f);

        private static readonly Color GEM_COLOR = new Color(0.4f, 0.85f, 1f, 1f);
        private static readonly Color GEM_DARK = new Color(0.2f, 0.5f, 0.7f, 1f);
        private static readonly Color COIN_COLOR = new Color(1f, 0.85f, 0.3f, 1f);
        private static readonly Color COIN_DARK = new Color(0.8f, 0.6f, 0.1f, 1f);

        private static readonly Color BLOCKER_BG = new Color(0f, 0f, 0f, 0.9f);

        // ==================== DIMENSIONES PREMIUM ====================
        private const float HEADER_HEIGHT = 100f;
        private const float CONTENT_PADDING = 16f;
        private const float SECTION_SPACING = 24f;

        // Featured Banner
        private const float FEATURED_HEIGHT = 180f;

        // Daily Deals
        private const float DAILY_SECTION_HEIGHT = 200f;
        private const float DAILY_ITEM_WIDTH = 180f;
        private const float DAILY_ITEM_HEIGHT = 160f;

        // Shop Items
        private const float ITEM_WIDTH = 170f;
        private const float ITEM_HEIGHT = 200f;
        private const float ITEM_SPACING = 12f;

        // Premium Section
        private const float PREMIUM_HEIGHT = 140f;

        [MenuItem("DigitPark/UI Builders/Monetization/Shop Premium (Clash Royale Style)", false, 181)]
        public static void BuildUI()
        {
            if (!EditorUtility.DisplayDialog("Shop Premium UI Builder",
                "Esto construira la UI PREMIUM de Shop estilo Clash Royale.\n\n" +
                "Incluye:\n" +
                "- Featured Banner con countdown\n" +
                "- Daily Deals (3 items + 1 GRATIS)\n" +
                "- Packs de Gemas (6 items)\n" +
                "- Packs de Monedas (3 items)\n" +
                "- Temas/Cosmeticos (6 items)\n" +
                "- Seccion Premium/VIP\n\n" +
                "Asegurate de tener la escena Shop abierta.\n\nContinuar?",
                "Si, Construir", "Cancelar"))
                return;

            BuildCompleteUI();
        }

        private static void BuildCompleteUI()
        {
            Debug.Log("[ShopPremiumUIBuilder] ========== INICIANDO CONSTRUCCION PREMIUM ==========");

            CleanupOldUI();

            Canvas canvas = SetupCanvas();
            if (canvas == null) return;

            // Limpiar canvas existente
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

            // Agregar ShopManager si no existe
            AddShopManager(canvas);

            MarkSceneDirty();
            Debug.Log("[ShopPremiumUIBuilder] ========== CONSTRUCCION PREMIUM COMPLETADA ==========");

            EditorUtility.DisplayDialog("Completado",
                "Shop Premium UI construida exitosamente!\n\n" +
                "Ejecuta 'DigitPark > Tools > Auto Assign > Shop Manager References' para conectar las referencias.",
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
            // Eliminar hijos existentes excepto EventSystem
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

            // Glow inferior
            CreateGlowLine(header, CYAN_NEON, false);

            // Back Button
            GameObject backBtn = CreateChild(header, "BackButton");
            RectTransform backRT = backBtn.GetComponent<RectTransform>();
            backRT.anchorMin = new Vector2(0, 0.5f);
            backRT.anchorMax = new Vector2(0, 0.5f);
            backRT.pivot = new Vector2(0, 0.5f);
            backRT.anchoredPosition = new Vector2(20, 0);
            backRT.sizeDelta = new Vector2(55, 55);

            Image backBg = backBtn.AddComponent<Image>();
            backBg.color = BUTTON_SECONDARY;
            AddOutline(backBtn, CYAN_DARK, 1);

            Button backButton = backBtn.AddComponent<Button>();
            SetupButton(backButton, BUTTON_SECONDARY);

            GameObject backIcon = CreateChild(backBtn, "Icon");
            SetStretch(backIcon);
            TextMeshProUGUI backText = backIcon.AddComponent<TextMeshProUGUI>();
            backText.text = "<";
            backText.fontSize = 36;
            backText.fontStyle = FontStyles.Bold;
            backText.color = CYAN_NEON;
            backText.alignment = TextAlignmentOptions.Center;

            // Title
            GameObject title = CreateChild(header, "Title");
            RectTransform titleRT = title.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.5f);
            titleRT.anchorMax = new Vector2(0, 0.5f);
            titleRT.pivot = new Vector2(0, 0.5f);
            titleRT.anchoredPosition = new Vector2(90, 0);
            titleRT.sizeDelta = new Vector2(200, 50);

            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "TIENDA";
            titleText.fontSize = 38;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = TEXT_PRIMARY;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;

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
            rt.sizeDelta = new Vector2(260, 50);

            HorizontalLayoutGroup hlg = container.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.childAlignment = TextAnchor.MiddleRight;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            // Gems
            CreateCurrencyPill(container, "GemsDisplay", "1,250", GEM_COLOR, true);

            // Coins
            CreateCurrencyPill(container, "CoinsDisplay", "5,430", COIN_COLOR, false);
        }

        private static void CreateCurrencyPill(GameObject parent, string name, string amount, Color color, bool isGems)
        {
            GameObject pill = CreateChild(parent, name);
            RectTransform rt = pill.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(120, 44);

            Image bg = pill.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.12f, 0.18f, 0.95f);
            AddOutline(pill, color * 0.6f, 1);

            HorizontalLayoutGroup hlg = pill.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6;
            hlg.padding = new RectOffset(8, 8, 4, 4);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;

            LayoutElement le = pill.AddComponent<LayoutElement>();
            le.minWidth = 120;
            le.preferredWidth = 120;

            // Icon
            GameObject icon = CreateChild(pill, "Icon");
            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = color;
            iconImg.preserveAspect = true;
            LayoutElement iconLE = icon.AddComponent<LayoutElement>();
            iconLE.minWidth = 26;
            iconLE.minHeight = 26;
            iconLE.preferredWidth = 26;
            iconLE.preferredHeight = 26;

            // Amount
            GameObject amountObj = CreateChild(pill, "Amount");
            TextMeshProUGUI amountText = amountObj.AddComponent<TextMeshProUGUI>();
            amountText.text = amount;
            amountText.fontSize = 17;
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
            plusLE.minWidth = 20;
            plusLE.minHeight = 20;

            GameObject plusText = CreateChild(plus, "Text");
            SetStretch(plusText);
            TextMeshProUGUI pt = plusText.AddComponent<TextMeshProUGUI>();
            pt.text = "+";
            pt.fontSize = 16;
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

            // === SECCIONES ===

            // 1. Featured Banner
            CreateFeaturedBanner(content);

            // 2. Daily Deals
            CreateDailyDealsSection(content);

            // 3. Gems Section
            CreateGemsSection(content);

            // 4. Coins Section
            CreateCoinsSection(content);

            // 5. Themes Section
            CreateThemesSection(content);

            // 6. Premium Section
            CreatePremiumSection(content);

            Debug.Log("[ShopPremiumUIBuilder] Scroll content creado");
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
            AddOutline(banner, PURPLE_PREMIUM, 2);

            // Gradient overlay would go here with shader

            HorizontalLayoutGroup hlg = banner.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 15;
            hlg.padding = new RectOffset(20, 20, 15, 15);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            // Left - Icon/Image
            GameObject iconContainer = CreateChild(banner, "IconContainer");
            LayoutElement iconLE = iconContainer.AddComponent<LayoutElement>();
            iconLE.minWidth = 120;
            iconLE.preferredWidth = 120;

            GameObject icon = CreateChild(iconContainer, "Icon");
            RectTransform iconRT = icon.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.sizeDelta = new Vector2(100, 100);
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
            GameObject badge = CreateChild(info, "Badge");
            Image badgeBg = badge.AddComponent<Image>();
            badgeBg.color = ORANGE_HOT;
            LayoutElement badgeLE = badge.AddComponent<LayoutElement>();
            badgeLE.minHeight = 26;
            badgeLE.preferredHeight = 26;
            badgeLE.minWidth = 100;
            badgeLE.preferredWidth = 100;

            GameObject badgeText = CreateChild(badge, "Text");
            SetStretch(badgeText);
            TextMeshProUGUI bt = badgeText.AddComponent<TextMeshProUGUI>();
            bt.text = "70% OFF";
            bt.fontSize = 14;
            bt.fontStyle = FontStyles.Bold;
            bt.color = TEXT_PRIMARY;
            bt.alignment = TextAlignmentOptions.Center;

            // Title
            GameObject title = CreateChild(info, "Title");
            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "STARTER PACK";
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GOLD;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement titleLE = title.AddComponent<LayoutElement>();
            titleLE.minHeight = 35;

            // Contents
            GameObject contents = CreateChild(info, "Contents");
            TextMeshProUGUI contentsText = contents.AddComponent<TextMeshProUGUI>();
            contentsText.text = "500 Gemas + Tema Exclusivo + Avatar";
            contentsText.fontSize = 13;
            contentsText.color = TEXT_SECONDARY;
            LayoutElement contentsLE = contents.AddComponent<LayoutElement>();
            contentsLE.minHeight = 20;

            // Timer
            GameObject timer = CreateChild(info, "Timer");
            TextMeshProUGUI timerText = timer.AddComponent<TextMeshProUGUI>();
            timerText.text = "⏱ Expira en: 23:45:12";
            timerText.fontSize = 12;
            timerText.color = ORANGE_HOT;
            LayoutElement timerLE = timer.AddComponent<LayoutElement>();
            timerLE.minHeight = 18;

            // Right - Buy
            GameObject buyContainer = CreateChild(banner, "BuyContainer");
            LayoutElement buyContainerLE = buyContainer.AddComponent<LayoutElement>();
            buyContainerLE.minWidth = 130;
            buyContainerLE.preferredWidth = 130;

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
            origText.fontSize = 14;
            origText.color = TEXT_MUTED;
            origText.alignment = TextAlignmentOptions.Center;
            LayoutElement origLE = origPrice.AddComponent<LayoutElement>();
            origLE.minHeight = 20;

            // Buy button
            GameObject buyBtn = CreateChild(buyContainer, "BuyButton");
            Image buyBg = buyBtn.AddComponent<Image>();
            buyBg.color = BUTTON_SUCCESS;
            Button buyButton = buyBtn.AddComponent<Button>();
            SetupButton(buyButton, BUTTON_SUCCESS);
            AddOutline(buyBtn, new Color(0.4f, 1f, 0.5f, 0.6f), 2);
            LayoutElement buyLE = buyBtn.AddComponent<LayoutElement>();
            buyLE.minHeight = 55;
            buyLE.preferredHeight = 55;

            GameObject buyText = CreateChild(buyBtn, "Text");
            SetStretch(buyText);
            TextMeshProUGUI buyTxt = buyText.AddComponent<TextMeshProUGUI>();
            buyTxt.text = "$2.99";
            buyTxt.fontSize = 26;
            buyTxt.fontStyle = FontStyles.Bold;
            buyTxt.color = TEXT_DARK;
            buyTxt.alignment = TextAlignmentOptions.Center;

            Debug.Log("[ShopPremiumUIBuilder] Featured Banner creado");
        }

        // ==================== DAILY DEALS ====================

        private static void CreateDailyDealsSection(GameObject parent)
        {
            GameObject section = CreateChild(parent, "DailyDealsSection");

            LayoutElement sectionLE = section.AddComponent<LayoutElement>();
            sectionLE.minHeight = DAILY_SECTION_HEIGHT + 45;

            VerticalLayoutGroup vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 12;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Header
            GameObject header = CreateChild(section, "Header");
            LayoutElement headerLE = header.AddComponent<LayoutElement>();
            headerLE.minHeight = 35;

            HorizontalLayoutGroup headerHlg = header.AddComponent<HorizontalLayoutGroup>();
            headerHlg.childAlignment = TextAnchor.MiddleLeft;
            headerHlg.childControlWidth = false;
            headerHlg.childControlHeight = true;

            GameObject titleObj = CreateChild(header, "Title");
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "⚡ OFERTAS DIARIAS";
            titleText.fontSize = 22;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = ORANGE_HOT;
            LayoutElement titleLE = titleObj.AddComponent<LayoutElement>();
            titleLE.minWidth = 250;

            GameObject timerObj = CreateChild(header, "Timer");
            TextMeshProUGUI timerText = timerObj.AddComponent<TextMeshProUGUI>();
            timerText.text = "⏱ 12:34:56";
            timerText.fontSize = 16;
            timerText.color = TEXT_SECONDARY;
            timerText.alignment = TextAlignmentOptions.MidlineRight;
            LayoutElement timerLE = timerObj.AddComponent<LayoutElement>();
            timerLE.flexibleWidth = 1;

            // Items container
            GameObject itemsContainer = CreateChild(section, "Items");
            LayoutElement itemsLE = itemsContainer.AddComponent<LayoutElement>();
            itemsLE.minHeight = DAILY_ITEM_HEIGHT;

            HorizontalLayoutGroup itemsHlg = itemsContainer.AddComponent<HorizontalLayoutGroup>();
            itemsHlg.spacing = 12;
            itemsHlg.childAlignment = TextAnchor.MiddleCenter;
            itemsHlg.childControlWidth = true;
            itemsHlg.childControlHeight = true;
            itemsHlg.childForceExpandWidth = true;

            // 3 Daily Items
            CreateDailyItem(itemsContainer, "Daily_Bonus", "25 Gemas", "100", GEM_COLOR, new Color(0.3f, 0.6f, 1f, 1f), false);
            CreateDailyItem(itemsContainer, "Daily_Gems", "200 Gemas", "GRATIS", GEM_COLOR, GEM_COLOR, true);
            CreateDailyItem(itemsContainer, "Daily_Coins", "5,000 Monedas", "50", GEM_COLOR, COIN_COLOR, false);

            Debug.Log("[ShopPremiumUIBuilder] Daily Deals creado");
        }

        private static void CreateDailyItem(GameObject parent, string name, string itemName, string price, Color priceColor, Color iconColor, bool isFree)
        {
            GameObject item = CreateChild(parent, name);

            Image itemBg = item.AddComponent<Image>();
            itemBg.color = CARD_BG;
            AddOutline(item, isFree ? GREEN_FREE : iconColor * 0.5f, isFree ? 2 : 1);

            Button btn = item.AddComponent<Button>();
            SetupButton(btn, CARD_BG);

            VerticalLayoutGroup vlg = item.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.padding = new RectOffset(10, 10, 12, 12);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Free badge
            if (isFree)
            {
                GameObject badge = CreateChild(item, "FreeBadge");
                Image badgeBg = badge.AddComponent<Image>();
                badgeBg.color = GREEN_FREE;
                LayoutElement badgeLE = badge.AddComponent<LayoutElement>();
                badgeLE.minHeight = 22;
                badgeLE.preferredHeight = 22;

                GameObject badgeText = CreateChild(badge, "Text");
                SetStretch(badgeText);
                TextMeshProUGUI bt = badgeText.AddComponent<TextMeshProUGUI>();
                bt.text = "1x DIARIO";
                bt.fontSize = 11;
                bt.fontStyle = FontStyles.Bold;
                bt.color = TEXT_DARK;
                bt.alignment = TextAlignmentOptions.Center;
            }

            // Icon
            GameObject icon = CreateChild(item, "Icon");
            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = iconColor;
            LayoutElement iconLE = icon.AddComponent<LayoutElement>();
            iconLE.minHeight = 50;
            iconLE.preferredHeight = 50;
            iconLE.minWidth = 50;
            iconLE.preferredWidth = 50;

            // Name
            GameObject nameObj = CreateChild(item, "Name");
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = itemName;
            nameText.fontSize = 14;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = TEXT_PRIMARY;
            nameText.alignment = TextAlignmentOptions.Center;
            LayoutElement nameLE = nameObj.AddComponent<LayoutElement>();
            nameLE.minHeight = 22;

            // Price button
            GameObject priceBtn = CreateChild(item, "PriceButton");
            Image priceBg = priceBtn.AddComponent<Image>();
            priceBg.color = isFree ? GREEN_FREE : priceColor;
            LayoutElement priceLE = priceBtn.AddComponent<LayoutElement>();
            priceLE.minHeight = 36;
            priceLE.preferredHeight = 36;

            HorizontalLayoutGroup priceHlg = priceBtn.AddComponent<HorizontalLayoutGroup>();
            priceHlg.spacing = 5;
            priceHlg.padding = new RectOffset(12, 12, 4, 4);
            priceHlg.childAlignment = TextAnchor.MiddleCenter;
            priceHlg.childControlWidth = false;
            priceHlg.childControlHeight = true;

            if (!isFree)
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
            pt.fontSize = 15;
            pt.fontStyle = FontStyles.Bold;
            pt.color = TEXT_DARK;
            pt.alignment = TextAlignmentOptions.Center;
            LayoutElement ptLE = priceText.AddComponent<LayoutElement>();
            ptLE.flexibleWidth = 1;
        }

        // ==================== GEMS SECTION ====================

        private static void CreateGemsSection(GameObject parent)
        {
            GameObject section = CreateChild(parent, "GemsSection");

            VerticalLayoutGroup vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 12;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Header
            CreateSectionHeader(section, "💎 GEMAS", GEM_COLOR);

            // Grid
            GameObject grid = CreateChild(section, "GemsGrid");

            GridLayoutGroup glg = grid.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(ITEM_WIDTH, ITEM_HEIGHT);
            glg.spacing = new Vector2(ITEM_SPACING, ITEM_SPACING);
            glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            glg.startAxis = GridLayoutGroup.Axis.Horizontal;
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 3;

            LayoutElement gridLE = grid.AddComponent<LayoutElement>();
            gridLE.minHeight = ITEM_HEIGHT * 2 + ITEM_SPACING;

            // 6 Gem packs
            CreateShopItem(grid, "Gems_100", "100", "$0.99", "", GEM_COLOR, "");
            CreateShopItem(grid, "Gems_500", "500", "$4.99", "+10%", GEM_COLOR, "");
            CreateShopItem(grid, "Gems_1200", "1,200", "$9.99", "+20%", GEM_COLOR, "BEST_VALUE");
            CreateShopItem(grid, "Gems_2500", "2,500", "$19.99", "+25%", GEM_COLOR, "");
            CreateShopItem(grid, "Gems_6500", "6,500", "$49.99", "+30%", GEM_COLOR, "POPULAR");
            CreateShopItem(grid, "Gems_14000", "14,000", "$99.99", "+40%", GEM_COLOR, "MEGA");

            Debug.Log("[ShopPremiumUIBuilder] Gems Section creado");
        }

        // ==================== COINS SECTION ====================

        private static void CreateCoinsSection(GameObject parent)
        {
            GameObject section = CreateChild(parent, "CoinsSection");

            VerticalLayoutGroup vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 12;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Header
            CreateSectionHeader(section, "🪙 MONEDAS", COIN_COLOR);

            // Grid
            GameObject grid = CreateChild(section, "CoinsGrid");

            GridLayoutGroup glg = grid.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(ITEM_WIDTH, ITEM_HEIGHT);
            glg.spacing = new Vector2(ITEM_SPACING, ITEM_SPACING);
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 3;

            LayoutElement gridLE = grid.AddComponent<LayoutElement>();
            gridLE.minHeight = ITEM_HEIGHT;

            // 3 Coin packs (bought with gems)
            CreateShopItem(grid, "Coins_1000", "1,000", "50", "", COIN_COLOR, "", true);
            CreateShopItem(grid, "Coins_5000", "5,000", "200", "+25%", COIN_COLOR, "", true);
            CreateShopItem(grid, "Coins_15000", "15,000", "500", "+50%", COIN_COLOR, "BEST_VALUE", true);

            Debug.Log("[ShopPremiumUIBuilder] Coins Section creado");
        }

        // ==================== THEMES SECTION ====================

        private static void CreateThemesSection(GameObject parent)
        {
            GameObject section = CreateChild(parent, "ThemesSection");

            VerticalLayoutGroup vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 12;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Header
            CreateSectionHeader(section, "\ud83c\udfa8 TEMAS", PURPLE_PREMIUM);

            // Grid
            GameObject grid = CreateChild(section, "ThemesGrid");

            GridLayoutGroup glg = grid.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(ITEM_WIDTH, ITEM_HEIGHT + 20);
            glg.spacing = new Vector2(ITEM_SPACING, ITEM_SPACING);
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 3;

            LayoutElement gridLE = grid.AddComponent<LayoutElement>();
            gridLE.minHeight = (ITEM_HEIGHT + 20) * 2 + ITEM_SPACING;

            // Theme items
            CreateThemeItem(grid, "Theme_Neon", "Neon Cyan", "EQUIPADO", CYAN_NEON, true);
            CreateThemeItem(grid, "Theme_Gold", "Oro Real", "500", GOLD, false);
            CreateThemeItem(grid, "Theme_Purple", "Amatista", "500", PURPLE_PREMIUM, false);
            CreateThemeItem(grid, "Theme_Red", "Rubi", "750", new Color(1f, 0.2f, 0.3f, 1f), false);
            CreateThemeItem(grid, "Theme_Green", "Esmeralda", "750", new Color(0.2f, 0.9f, 0.4f, 1f), false);
            CreateThemeItem(grid, "Theme_Rainbow", "Arcoiris", "$2.99", new Color(1f, 0.5f, 0.8f, 1f), false);

            Debug.Log("[ShopPremiumUIBuilder] Themes Section creado");
        }

        private static void CreateThemeItem(GameObject parent, string name, string displayName, string price, Color themeColor, bool isEquipped)
        {
            GameObject item = CreateChild(parent, name);

            Image itemBg = item.AddComponent<Image>();
            itemBg.color = CARD_BG;
            AddOutline(item, isEquipped ? themeColor : themeColor * 0.4f, isEquipped ? 2 : 1);

            Button btn = item.AddComponent<Button>();
            SetupButton(btn, CARD_BG);

            VerticalLayoutGroup vlg = item.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6;
            vlg.padding = new RectOffset(8, 8, 10, 10);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            if (isEquipped)
            {
                GameObject badge = CreateChild(item, "EquippedBadge");
                Image badgeBg = badge.AddComponent<Image>();
                badgeBg.color = themeColor;
                LayoutElement badgeLE = badge.AddComponent<LayoutElement>();
                badgeLE.minHeight = 20;
                badgeLE.preferredHeight = 20;

                GameObject badgeText = CreateChild(badge, "Text");
                SetStretch(badgeText);
                TextMeshProUGUI bt = badgeText.AddComponent<TextMeshProUGUI>();
                bt.text = "EQUIPADO";
                bt.fontSize = 10;
                bt.fontStyle = FontStyles.Bold;
                bt.color = TEXT_DARK;
                bt.alignment = TextAlignmentOptions.Center;
            }

            // Theme preview (color swatch)
            GameObject preview = CreateChild(item, "Preview");
            Image previewImg = preview.AddComponent<Image>();
            previewImg.color = themeColor;
            LayoutElement previewLE = preview.AddComponent<LayoutElement>();
            previewLE.minHeight = 70;
            previewLE.preferredHeight = 70;

            // Create inner glow effect
            AddOutline(preview, themeColor * 0.5f, 3);

            // Name
            GameObject nameObj = CreateChild(item, "Name");
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = displayName;
            nameText.fontSize = 16;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = themeColor;
            nameText.alignment = TextAlignmentOptions.Center;
            LayoutElement nameLE = nameObj.AddComponent<LayoutElement>();
            nameLE.minHeight = 24;

            // Price/Action button
            GameObject priceBtn = CreateChild(item, "PriceButton");
            Image priceBg = priceBtn.AddComponent<Image>();
            priceBg.color = isEquipped ? BUTTON_SECONDARY : BUTTON_SUCCESS;
            LayoutElement priceLE = priceBtn.AddComponent<LayoutElement>();
            priceLE.minHeight = 36;
            priceLE.preferredHeight = 36;

            HorizontalLayoutGroup priceHlg = priceBtn.AddComponent<HorizontalLayoutGroup>();
            priceHlg.spacing = 5;
            priceHlg.padding = new RectOffset(10, 10, 4, 4);
            priceHlg.childAlignment = TextAnchor.MiddleCenter;
            priceHlg.childControlWidth = false;
            priceHlg.childControlHeight = true;

            if (!isEquipped && !price.StartsWith("$"))
            {
                // Gem price icon
                GameObject gemIcon = CreateChild(priceBtn, "GemIcon");
                Image gemImg = gemIcon.AddComponent<Image>();
                gemImg.color = TEXT_DARK;
                LayoutElement gemLE = gemIcon.AddComponent<LayoutElement>();
                gemLE.minWidth = 18;
                gemLE.minHeight = 18;
            }

            GameObject priceText = CreateChild(priceBtn, "Text");
            TextMeshProUGUI pt = priceText.AddComponent<TextMeshProUGUI>();
            pt.text = isEquipped ? "EQUIPADO" : price;
            pt.fontSize = isEquipped ? 12 : 15;
            pt.fontStyle = FontStyles.Bold;
            pt.color = isEquipped ? TEXT_SECONDARY : TEXT_DARK;
            pt.alignment = TextAlignmentOptions.Center;
            LayoutElement ptLE = priceText.AddComponent<LayoutElement>();
            ptLE.flexibleWidth = 1;
        }

        // ==================== PREMIUM SECTION ====================

        private static void CreatePremiumSection(GameObject parent)
        {
            GameObject section = CreateChild(parent, "PremiumSection");

            LayoutElement sectionLE = section.AddComponent<LayoutElement>();
            sectionLE.minHeight = PREMIUM_HEIGHT;

            Image sectionBg = section.AddComponent<Image>();
            sectionBg.color = new Color(0.1f, 0.05f, 0.18f, 1f);
            AddOutline(section, PURPLE_PREMIUM, 2);

            HorizontalLayoutGroup hlg = section.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.padding = new RectOffset(20, 20, 15, 15);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            // Icon
            GameObject iconContainer = CreateChild(section, "IconContainer");
            LayoutElement iconLE = iconContainer.AddComponent<LayoutElement>();
            iconLE.minWidth = 80;

            GameObject icon = CreateChild(iconContainer, "Icon");
            RectTransform iconRT = icon.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.sizeDelta = new Vector2(70, 70);
            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = GOLD;

            // Info
            GameObject info = CreateChild(section, "Info");
            LayoutElement infoLE = info.AddComponent<LayoutElement>();
            infoLE.flexibleWidth = 1;

            VerticalLayoutGroup infoVlg = info.AddComponent<VerticalLayoutGroup>();
            infoVlg.spacing = 4;
            infoVlg.childAlignment = TextAnchor.MiddleLeft;
            infoVlg.childControlHeight = true;
            infoVlg.childForceExpandHeight = false;

            // VIP Badge
            GameObject badge = CreateChild(info, "Badge");
            Image badgeBg = badge.AddComponent<Image>();
            badgeBg.color = GOLD;
            LayoutElement badgeLE = badge.AddComponent<LayoutElement>();
            badgeLE.minHeight = 24;
            badgeLE.preferredHeight = 24;
            badgeLE.minWidth = 60;
            badgeLE.preferredWidth = 60;

            GameObject badgeText = CreateChild(badge, "Text");
            SetStretch(badgeText);
            TextMeshProUGUI bt = badgeText.AddComponent<TextMeshProUGUI>();
            bt.text = "VIP";
            bt.fontSize = 12;
            bt.fontStyle = FontStyles.Bold;
            bt.color = TEXT_DARK;
            bt.alignment = TextAlignmentOptions.Center;

            // Title
            GameObject title = CreateChild(info, "Title");
            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "BUNDLE PREMIUM";
            titleText.fontSize = 20;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GOLD;
            LayoutElement titleLE = title.AddComponent<LayoutElement>();
            titleLE.minHeight = 28;

            // Desc
            GameObject desc = CreateChild(info, "Description");
            TextMeshProUGUI descText = desc.AddComponent<TextMeshProUGUI>();
            descText.text = "50 niveles de recompensas exclusivas";
            descText.fontSize = 13;
            descText.color = TEXT_SECONDARY;
            LayoutElement descLE = desc.AddComponent<LayoutElement>();
            descLE.minHeight = 20;

            // Buy
            GameObject buyContainer = CreateChild(section, "BuyContainer");
            LayoutElement buyContainerLE = buyContainer.AddComponent<LayoutElement>();
            buyContainerLE.minWidth = 120;

            GameObject buyBtn = CreateChild(buyContainer, "BuyButton");
            RectTransform buyRT = buyBtn.GetComponent<RectTransform>();
            buyRT.anchorMin = new Vector2(0.5f, 0.5f);
            buyRT.anchorMax = new Vector2(0.5f, 0.5f);
            buyRT.sizeDelta = new Vector2(110, 50);

            Image buyBg = buyBtn.AddComponent<Image>();
            buyBg.color = PURPLE_PREMIUM;
            Button buyButton = buyBtn.AddComponent<Button>();
            SetupButton(buyButton, PURPLE_PREMIUM);
            AddOutline(buyBtn, PURPLE_LIGHT, 2);

            GameObject buyText = CreateChild(buyBtn, "Text");
            SetStretch(buyText);
            TextMeshProUGUI buyTxt = buyText.AddComponent<TextMeshProUGUI>();
            buyTxt.text = "$9.99";
            buyTxt.fontSize = 20;
            buyTxt.fontStyle = FontStyles.Bold;
            buyTxt.color = TEXT_PRIMARY;
            buyTxt.alignment = TextAlignmentOptions.Center;

            Debug.Log("[ShopPremiumUIBuilder] Premium Section creado");
        }

        // ==================== HELPER: SECTION HEADER ====================

        private static void CreateSectionHeader(GameObject parent, string title, Color color)
        {
            GameObject header = CreateChild(parent, "Header");
            LayoutElement headerLE = header.AddComponent<LayoutElement>();
            headerLE.minHeight = 35;

            HorizontalLayoutGroup hlg = header.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;

            GameObject titleObj = CreateChild(header, "Title");
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = title;
            titleText.fontSize = 22;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = color;
            LayoutElement titleLE = titleObj.AddComponent<LayoutElement>();
            titleLE.minWidth = 200;

            // Separator line
            GameObject line = CreateChild(header, "Line");
            Image lineImg = line.AddComponent<Image>();
            lineImg.color = color * 0.3f;
            LayoutElement lineLE = line.AddComponent<LayoutElement>();
            lineLE.flexibleWidth = 1;
            lineLE.minHeight = 2;
            lineLE.preferredHeight = 2;
        }

        // ==================== HELPER: SHOP ITEM ====================

        private static void CreateShopItem(GameObject parent, string name, string amount, string price, string bonus, Color color, string badge, bool useGems = false)
        {
            GameObject item = CreateChild(parent, name);

            Image itemBg = item.AddComponent<Image>();
            itemBg.color = CARD_BG;

            bool hasBadge = !string.IsNullOrEmpty(badge);
            Color outlineColor = color * 0.5f;
            int outlineWidth = 1;

            if (badge == "BEST_VALUE")
            {
                outlineColor = GREEN_FREE;
                outlineWidth = 2;
            }
            else if (badge == "POPULAR")
            {
                outlineColor = GOLD;
                outlineWidth = 2;
            }
            else if (badge == "MEGA")
            {
                outlineColor = ORANGE_HOT;
                outlineWidth = 2;
            }

            AddOutline(item, outlineColor, outlineWidth);

            Button btn = item.AddComponent<Button>();
            SetupButton(btn, CARD_BG);

            VerticalLayoutGroup vlg = item.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6;
            vlg.padding = new RectOffset(8, 8, 10, 10);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Badge
            if (hasBadge)
            {
                GameObject badgeObj = CreateChild(item, "Badge");
                Image badgeBg = badgeObj.AddComponent<Image>();

                string badgeText = "";
                Color badgeColor = GOLD;

                switch (badge)
                {
                    case "BEST_VALUE":
                        badgeText = "MEJOR VALOR";
                        badgeColor = GREEN_FREE;
                        break;
                    case "POPULAR":
                        badgeText = "POPULAR";
                        badgeColor = GOLD;
                        break;
                    case "MEGA":
                        badgeText = "MEGA PACK";
                        badgeColor = ORANGE_HOT;
                        break;
                }

                badgeBg.color = badgeColor;
                LayoutElement badgeLE = badgeObj.AddComponent<LayoutElement>();
                badgeLE.minHeight = 20;
                badgeLE.preferredHeight = 20;

                GameObject badgeTextObj = CreateChild(badgeObj, "Text");
                SetStretch(badgeTextObj);
                TextMeshProUGUI bt = badgeTextObj.AddComponent<TextMeshProUGUI>();
                bt.text = badgeText;
                bt.fontSize = 10;
                bt.fontStyle = FontStyles.Bold;
                bt.color = TEXT_DARK;
                bt.alignment = TextAlignmentOptions.Center;
            }

            // Icon
            GameObject icon = CreateChild(item, "Icon");
            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = color;
            LayoutElement iconLE = icon.AddComponent<LayoutElement>();
            iconLE.minHeight = 50;
            iconLE.preferredHeight = 50;
            iconLE.minWidth = 50;
            iconLE.preferredWidth = 50;

            // Amount
            GameObject amountObj = CreateChild(item, "Amount");
            TextMeshProUGUI amountText = amountObj.AddComponent<TextMeshProUGUI>();
            amountText.text = amount;
            amountText.fontSize = 22;
            amountText.fontStyle = FontStyles.Bold;
            amountText.color = color;
            amountText.alignment = TextAlignmentOptions.Center;
            LayoutElement amountLE = amountObj.AddComponent<LayoutElement>();
            amountLE.minHeight = 28;

            // Bonus
            if (!string.IsNullOrEmpty(bonus))
            {
                GameObject bonusObj = CreateChild(item, "Bonus");
                TextMeshProUGUI bonusText = bonusObj.AddComponent<TextMeshProUGUI>();
                bonusText.text = bonus + " BONUS";
                bonusText.fontSize = 11;
                bonusText.fontStyle = FontStyles.Bold;
                bonusText.color = BUTTON_SUCCESS;
                bonusText.alignment = TextAlignmentOptions.Center;
                LayoutElement bonusLE = bonusObj.AddComponent<LayoutElement>();
                bonusLE.minHeight = 16;
            }

            // Price button
            GameObject priceBtn = CreateChild(item, "PriceButton");
            Image priceBg = priceBtn.AddComponent<Image>();
            priceBg.color = useGems ? GEM_COLOR : BUTTON_SUCCESS;
            LayoutElement priceLE = priceBtn.AddComponent<LayoutElement>();
            priceLE.minHeight = 36;
            priceLE.preferredHeight = 36;

            HorizontalLayoutGroup priceHlg = priceBtn.AddComponent<HorizontalLayoutGroup>();
            priceHlg.spacing = 5;
            priceHlg.padding = new RectOffset(10, 10, 4, 4);
            priceHlg.childAlignment = TextAnchor.MiddleCenter;
            priceHlg.childControlWidth = false;
            priceHlg.childControlHeight = true;

            if (useGems)
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
            pt.fontSize = 16;
            pt.fontStyle = FontStyles.Bold;
            pt.color = TEXT_DARK;
            pt.alignment = TextAlignmentOptions.Center;
            LayoutElement ptLE = priceText.AddComponent<LayoutElement>();
            ptLE.flexibleWidth = 1;
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
            popupRT.sizeDelta = new Vector2(420, 340);

            Image popupBg = popup.AddComponent<Image>();
            popupBg.color = PANEL_BG;
            AddOutline(popup, CYAN_NEON, 2);

            VerticalLayoutGroup vlg = popup.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 18;
            vlg.padding = new RectOffset(25, 25, 25, 25);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Title
            GameObject title = CreateChild(popup, "Title");
            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "Confirmar Compra";
            titleText.fontSize = 26;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = CYAN_NEON;
            titleText.alignment = TextAlignmentOptions.Center;
            LayoutElement titleLE = title.AddComponent<LayoutElement>();
            titleLE.minHeight = 35;

            // Preview
            GameObject preview = CreateChild(popup, "Preview");
            HorizontalLayoutGroup previewHlg = preview.AddComponent<HorizontalLayoutGroup>();
            previewHlg.spacing = 15;
            previewHlg.childAlignment = TextAnchor.MiddleCenter;
            previewHlg.childControlWidth = false;
            previewHlg.childControlHeight = true;
            LayoutElement previewLE = preview.AddComponent<LayoutElement>();
            previewLE.minHeight = 55;

            GameObject previewIcon = CreateChild(preview, "Icon");
            Image previewImg = previewIcon.AddComponent<Image>();
            previewImg.color = GEM_COLOR;
            LayoutElement iconLE = previewIcon.AddComponent<LayoutElement>();
            iconLE.minWidth = 50;
            iconLE.minHeight = 50;

            GameObject previewAmount = CreateChild(preview, "Amount");
            TextMeshProUGUI amountText = previewAmount.AddComponent<TextMeshProUGUI>();
            amountText.text = "1,200 Gemas";
            amountText.fontSize = 24;
            amountText.fontStyle = FontStyles.Bold;
            amountText.color = TEXT_PRIMARY;
            LayoutElement amountLE = previewAmount.AddComponent<LayoutElement>();
            amountLE.minWidth = 200;

            // Price
            GameObject priceObj = CreateChild(popup, "Price");
            TextMeshProUGUI priceText = priceObj.AddComponent<TextMeshProUGUI>();
            priceText.text = "Precio: $9.99";
            priceText.fontSize = 18;
            priceText.color = TEXT_SECONDARY;
            priceText.alignment = TextAlignmentOptions.Center;
            LayoutElement priceLE = priceObj.AddComponent<LayoutElement>();
            priceLE.minHeight = 28;

            // Buttons
            GameObject buttons = CreateChild(popup, "Buttons");
            HorizontalLayoutGroup btnHlg = buttons.AddComponent<HorizontalLayoutGroup>();
            btnHlg.spacing = 15;
            btnHlg.childControlWidth = true;
            btnHlg.childControlHeight = true;
            btnHlg.childForceExpandWidth = true;
            LayoutElement btnLE = buttons.AddComponent<LayoutElement>();
            btnLE.minHeight = 55;

            // Cancel
            CreatePopupButton(buttons, "CancelButton", "Cancelar", BUTTON_SECONDARY, TEXT_PRIMARY);

            // Confirm
            CreatePopupButton(buttons, "ConfirmButton", "Comprar", BUTTON_SUCCESS, TEXT_DARK);

            Debug.Log("[ShopPremiumUIBuilder] PurchasePopup creado");
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
            popupRT.sizeDelta = new Vector2(400, 300);

            Image popupBg = popup.AddComponent<Image>();
            popupBg.color = PANEL_BG;
            AddOutline(popup, GEM_COLOR, 2);

            VerticalLayoutGroup vlg = popup.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 15;
            vlg.padding = new RectOffset(25, 25, 25, 25);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // Icon
            GameObject icon = CreateChild(popup, "Icon");
            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = GEM_COLOR;
            LayoutElement iconLE = icon.AddComponent<LayoutElement>();
            iconLE.minHeight = 55;
            iconLE.minWidth = 55;
            iconLE.preferredHeight = 55;
            iconLE.preferredWidth = 55;

            // Title
            GameObject title = CreateChild(popup, "Title");
            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "Gemas Insuficientes";
            titleText.fontSize = 24;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GEM_COLOR;
            titleText.alignment = TextAlignmentOptions.Center;
            LayoutElement titleLE = title.AddComponent<LayoutElement>();
            titleLE.minHeight = 32;

            // Message
            GameObject msg = CreateChild(popup, "Message");
            TextMeshProUGUI msgText = msg.AddComponent<TextMeshProUGUI>();
            msgText.text = "No tienes suficientes gemas.\n¡Consigue más en la tienda!";
            msgText.fontSize = 15;
            msgText.color = TEXT_SECONDARY;
            msgText.alignment = TextAlignmentOptions.Center;
            LayoutElement msgLE = msg.AddComponent<LayoutElement>();
            msgLE.minHeight = 45;

            // Buttons
            GameObject buttons = CreateChild(popup, "Buttons");
            HorizontalLayoutGroup btnHlg = buttons.AddComponent<HorizontalLayoutGroup>();
            btnHlg.spacing = 15;
            btnHlg.childControlWidth = true;
            btnHlg.childControlHeight = true;
            btnHlg.childForceExpandWidth = true;
            LayoutElement btnLE = buttons.AddComponent<LayoutElement>();
            btnLE.minHeight = 50;

            CreatePopupButton(buttons, "CloseButton", "Cerrar", BUTTON_SECONDARY, TEXT_PRIMARY);
            CreatePopupButton(buttons, "GetGemsButton", "Obtener Gemas", GEM_COLOR, TEXT_DARK);

            Debug.Log("[ShopPremiumUIBuilder] NotEnoughPopup creado");
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
            txt.fontSize = 17;
            txt.fontStyle = FontStyles.Bold;
            txt.color = textColor;
            txt.alignment = TextAlignmentOptions.Center;
        }

        // ==================== SHOP MANAGER ====================

        private static void AddShopManager(Canvas canvas)
        {
            var existing = canvas.GetComponent<Monetization.ShopManager>();
            if (existing == null)
            {
                canvas.gameObject.AddComponent<Monetization.ShopManager>();
                Debug.Log("[ShopPremiumUIBuilder] ShopManager agregado");
            }
        }

        // ==================== UTILITIES ====================

        private static void CleanupOldUI()
        {
            string[] toClean = { "Background", "SafeArea" };
            foreach (var canvas in Object.FindObjectsOfType<Canvas>(true))
            {
                if (canvas.transform.parent != null) continue;
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
