using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Managers;
using DigitPark.Themes;
using DigitPark.Localization;
using DigitPark.Tools;
using DigitPark.UI;

namespace DigitPark.UI.Panels
{
    /// <summary>
    /// Panel profesional que muestra todos los temas disponibles con grid scrollable.
    /// Candado dorado = tema de paga ($2.50 USD), candado plateado = desbloqueable por logros.
    /// Bundle de 14 temas pagos con 30% de descuento.
    /// </summary>
    public class StylesProPromptPanel : MonoBehaviour
    {
        // UI References
        private GameObject blocker;
        private GameObject panel;
        private Button buyButton;
        private Button cancelButton;
        private Button closeButton;
        private Transform gridContent;

        // Lock icons
        private Sprite lockGoldSprite;
        private Sprite lockSilverSprite;

        // Pricing
        private const float PRICE_PER_THEME = 2.50f;
        private const int PAID_THEME_COUNT = 14;
        private const float BUNDLE_DISCOUNT = 0.30f;

        // Colors (matches ShopPremiumUIBuilder style)
        private static readonly Color DARK_BG = new Color(0.02f, 0.04f, 0.08f, 1f);
        private static readonly Color PANEL_BG = new Color(0.06f, 0.1f, 0.16f, 0.98f);
        private static readonly Color CARD_BG = new Color(0.08f, 0.12f, 0.18f, 1f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color TEXT_PRIMARY = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.7f, 0.8f, 1f);
        private static readonly Color GREEN_FREE = new Color(0.3f, 0.9f, 0.4f, 1f);
        private static readonly Color SILVER = new Color(0.7f, 0.75f, 0.82f, 1f);
        private static readonly Color BTN_SECONDARY = new Color(0.15f, 0.2f, 0.28f, 1f);

        /// <summary>
        /// Crea y muestra el panel de Styles PRO
        /// </summary>
        public static StylesProPromptPanel CreateAndShow()
        {
            CloseAllDropdowns();

            GameObject canvasObj = new GameObject("StylesProPromptCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10000;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject panelObj = new GameObject("StylesProPrompt");
            panelObj.transform.SetParent(canvasObj.transform, false);

            StylesProPromptPanel promptPanel = panelObj.AddComponent<StylesProPromptPanel>();
            promptPanel.CreateUI();

            Debug.Log("[StylesProPromptPanel] Panel creado y mostrado");
            return promptPanel;
        }

        private static void CloseAllDropdowns()
        {
            var dropdowns = FindObjectsOfType<TMP_Dropdown>();
            foreach (var dropdown in dropdowns)
            {
                dropdown.Hide();
            }
        }

        private void CreateUI()
        {
            // Load lock icons
            lockGoldSprite = Resources.Load<Sprite>("UI/Icons/icon_lock_gold");
            lockSilverSprite = Resources.Load<Sprite>("UI/Icons/icon_lock_silver");

            RectTransform rt = gameObject.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            blocker = CreateBlocker();
            panel = CreateMainPanel();
        }

        private GameObject CreateBlocker()
        {
            GameObject obj = new GameObject("Blocker");
            obj.transform.SetParent(transform, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image img = obj.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.9f);
            img.raycastTarget = true;

            return obj;
        }

        private GameObject CreateMainPanel()
        {
            GameObject obj = new GameObject("Panel");
            obj.transform.SetParent(transform, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(980, 1500);

            Image bg = obj.AddComponent<Image>();
            bg.color = PANEL_BG;

            Outline outline = obj.AddComponent<Outline>();
            outline.effectColor = new Color(GOLD.r, GOLD.g, GOLD.b, 0.7f);
            outline.effectDistance = new Vector2(3, 3);

            // Vertical layout for all content
            VerticalLayoutGroup vlg = obj.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(25, 25, 15, 15);
            vlg.spacing = 12;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            CreateCloseButton(obj.transform);
            CreateTitle(obj.transform);
            CreateSubtitle(obj.transform);
            CreateScrollView(obj.transform);
            CreatePriceSection(obj.transform);
            CreateBuyButton(obj.transform);
            CreateCancelButton(obj.transform);

            return obj;
        }

        private void CreateCloseButton(Transform parent)
        {
            GameObject obj = new GameObject("CloseButton");
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(-12, -12);
            rt.sizeDelta = new Vector2(50, 50);

            LayoutElement le = obj.AddComponent<LayoutElement>();
            le.ignoreLayout = true;

            Image bg = obj.AddComponent<Image>();
            bg.color = CYAN_NEON;

            closeButton = obj.AddComponent<Button>();
            closeButton.targetGraphic = bg;
            closeButton.onClick.AddListener(OnCancelClicked);

            ColorBlock colors = closeButton.colors;
            colors.normalColor = CYAN_NEON;
            colors.highlightedColor = new Color(0.3f, 1f, 1f, 1f);
            colors.pressedColor = new Color(0f, 0.7f, 0.8f, 1f);
            closeButton.colors = colors;

            // Try icon, fallback to X text
            Sprite closeIcon = Resources.Load<Sprite>("Icons/CloseIcon");
            if (closeIcon != null)
            {
                GameObject iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(obj.transform, false);
                RectTransform iconRt = iconObj.AddComponent<RectTransform>();
                iconRt.anchorMin = new Vector2(0.15f, 0.15f);
                iconRt.anchorMax = new Vector2(0.85f, 0.85f);
                iconRt.offsetMin = Vector2.zero;
                iconRt.offsetMax = Vector2.zero;
                Image iconImg = iconObj.AddComponent<Image>();
                iconImg.sprite = closeIcon;
                iconImg.color = Color.black;
                iconImg.raycastTarget = false;
            }
            else
            {
                GameObject textObj = new GameObject("X");
                textObj.transform.SetParent(obj.transform, false);
                RectTransform textRt = textObj.AddComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.offsetMin = Vector2.zero;
                textRt.offsetMax = Vector2.zero;
                TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
                tmp.text = "X";
                tmp.fontSize = FontSizes.Body;
                tmp.fontStyle = FontStyles.Bold;
                tmp.color = Color.black;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.raycastTarget = false;
            }
        }

        private void CreateTitle(Transform parent)
        {
            GameObject obj = new GameObject("Title");
            obj.transform.SetParent(parent, false);

            LayoutElement le = obj.AddComponent<LayoutElement>();
            le.preferredHeight = 55;

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = AutoLocalizer.Get("styles_pro_title");
            tmp.fontSize = FontSizes.H4;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = GOLD;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        private void CreateSubtitle(Transform parent)
        {
            GameObject obj = new GameObject("Subtitle");
            obj.transform.SetParent(parent, false);

            LayoutElement le = obj.AddComponent<LayoutElement>();
            le.preferredHeight = 40;

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = AutoLocalizer.Get("styles_pro_subtitle");
            tmp.fontSize = FontSizes.Body;
            tmp.color = TEXT_SECONDARY;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        private void CreateScrollView(Transform parent)
        {
            // ScrollView container
            GameObject scrollObj = new GameObject("ScrollView");
            scrollObj.transform.SetParent(parent, false);

            LayoutElement scrollLE = scrollObj.AddComponent<LayoutElement>();
            scrollLE.preferredHeight = 1050;
            scrollLE.flexibleWidth = 1;

            RectTransform scrollRT = scrollObj.GetComponent<RectTransform>();
            if (scrollRT == null) scrollRT = scrollObj.AddComponent<RectTransform>();

            Image scrollBg = scrollObj.AddComponent<Image>();
            scrollBg.color = new Color(DARK_BG.r, DARK_BG.g, DARK_BG.b, 0.5f);

            ScrollRect scrollRect = scrollObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;
            scrollRect.scrollSensitivity = 30f;

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollObj.transform, false);

            RectTransform viewportRT = viewport.AddComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.offsetMin = Vector2.zero;
            viewportRT.offsetMax = Vector2.zero;

            Image viewportMask = viewport.AddComponent<Image>();
            viewportMask.color = Color.white;
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            scrollRect.viewport = viewportRT;

            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);

            RectTransform contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.offsetMin = new Vector2(0, 0);
            contentRT.offsetMax = new Vector2(0, 0);

            GridLayoutGroup grid = content.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(290, 200);
            grid.spacing = new Vector2(16, 16);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.padding = new RectOffset(20, 20, 10, 10);

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRT;
            gridContent = content.transform;

            // Populate themes
            PopulateThemes();
        }

        private void PopulateThemes()
        {
            if (ThemeManager.Instance == null)
            {
                Debug.LogWarning("[StylesProPromptPanel] ThemeManager no disponible");
                return;
            }

            var themes = ThemeManager.Instance.AvailableThemes;

            foreach (var theme in themes)
            {
                CreateThemeCard(gridContent, theme);
            }

            Debug.Log($"[StylesProPromptPanel] {themes.Count} temas cargados en grid");
        }

        private void CreateThemeCard(Transform parent, ThemeData theme)
        {
            bool isUnlocked = IsThemeUnlocked(theme);
            bool isFree = !theme.isPremium;
            bool isEarnable = theme.isPremium && theme.isEarnable;
            bool isPaid = theme.isPremium && !theme.isEarnable;

            GameObject card = new GameObject("Theme_" + theme.themeId);
            card.transform.SetParent(parent, false);

            // Card background
            Image cardBg = card.AddComponent<Image>();
            cardBg.color = CARD_BG;

            // Border with theme accent
            Outline cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = new Color(theme.primaryAccent.r, theme.primaryAccent.g, theme.primaryAccent.b, 0.8f);
            cardOutline.effectDistance = new Vector2(2, 2);

            // CanvasGroup for lock dimming
            CanvasGroup cg = card.AddComponent<CanvasGroup>();
            cg.alpha = isUnlocked || isFree ? 1f : 0.7f;

            // === Color Swatch Area (top 130px) ===
            GameObject swatchArea = new GameObject("SwatchArea");
            swatchArea.transform.SetParent(card.transform, false);

            RectTransform swatchRT = swatchArea.AddComponent<RectTransform>();
            swatchRT.anchorMin = new Vector2(0, 0.35f);
            swatchRT.anchorMax = new Vector2(1, 1);
            swatchRT.offsetMin = new Vector2(4, 0);
            swatchRT.offsetMax = new Vector2(-4, -4);

            Image swatchBg = swatchArea.AddComponent<Image>();
            swatchBg.color = theme.primaryBackground;

            // 3 color dots
            CreateColorDots(swatchArea.transform, theme);

            // Accent stripe (bottom of swatch)
            GameObject stripe = new GameObject("AccentStripe");
            stripe.transform.SetParent(swatchArea.transform, false);

            RectTransform stripeRT = stripe.AddComponent<RectTransform>();
            stripeRT.anchorMin = new Vector2(0, 0);
            stripeRT.anchorMax = new Vector2(1, 0);
            stripeRT.pivot = new Vector2(0.5f, 0);
            stripeRT.sizeDelta = new Vector2(0, 4);

            Image stripeImg = stripe.AddComponent<Image>();
            stripeImg.color = theme.primaryAccent;

            // === Info Area (bottom 70px) ===
            GameObject infoArea = new GameObject("InfoArea");
            infoArea.transform.SetParent(card.transform, false);

            RectTransform infoRT = infoArea.AddComponent<RectTransform>();
            infoRT.anchorMin = new Vector2(0, 0);
            infoRT.anchorMax = new Vector2(1, 0.35f);
            infoRT.offsetMin = new Vector2(8, 4);
            infoRT.offsetMax = new Vector2(-8, 0);

            // Theme name (left-aligned)
            GameObject nameObj = new GameObject("ThemeName");
            nameObj.transform.SetParent(infoArea.transform, false);

            RectTransform nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0);
            nameRT.anchorMax = new Vector2(0.65f, 1);
            nameRT.offsetMin = Vector2.zero;
            nameRT.offsetMax = Vector2.zero;

            TextMeshProUGUI nameTMP = nameObj.AddComponent<TextMeshProUGUI>();
            nameTMP.text = theme.themeName;
            nameTMP.fontSize = 26;
            nameTMP.fontStyle = FontStyles.Bold;
            nameTMP.color = TEXT_PRIMARY;
            nameTMP.alignment = TextAlignmentOptions.Left;
            nameTMP.enableAutoSizing = true;
            nameTMP.fontSizeMin = 18;
            nameTMP.fontSizeMax = 26;
            nameTMP.enableWordWrapping = true;

            // Badge area (right-aligned)
            GameObject badgeArea = new GameObject("BadgeArea");
            badgeArea.transform.SetParent(infoArea.transform, false);

            RectTransform badgeRT = badgeArea.AddComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(0.65f, 0);
            badgeRT.anchorMax = new Vector2(1, 1);
            badgeRT.offsetMin = Vector2.zero;
            badgeRT.offsetMax = Vector2.zero;

            // Badge
            CreateBadge(badgeArea.transform, isFree, isPaid, isEarnable, isUnlocked);

            // Lock icon overlay (on swatch area, centered)
            if (!isUnlocked && !isFree)
            {
                CreateLockOverlay(swatchArea.transform, isPaid);
            }
        }

        private void CreateColorDots(Transform parent, ThemeData theme)
        {
            GameObject dotsContainer = new GameObject("ColorDots");
            dotsContainer.transform.SetParent(parent, false);

            RectTransform dotsRT = dotsContainer.AddComponent<RectTransform>();
            dotsRT.anchorMin = new Vector2(0.5f, 0.5f);
            dotsRT.anchorMax = new Vector2(0.5f, 0.5f);
            dotsRT.pivot = new Vector2(0.5f, 0.5f);
            dotsRT.anchoredPosition = new Vector2(0, 5);
            dotsRT.sizeDelta = new Vector2(180, 36);

            HorizontalLayoutGroup hlg = dotsContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 18;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            Color[] dotColors = { theme.primaryAccent, theme.secondaryAccent, theme.tertiaryAccent };
            for (int i = 0; i < dotColors.Length; i++)
            {
                GameObject dot = new GameObject($"Dot_{i}");
                dot.transform.SetParent(dotsContainer.transform, false);

                RectTransform dotRT = dot.AddComponent<RectTransform>();
                dotRT.sizeDelta = new Vector2(36, 36);

                LayoutElement dotLE = dot.AddComponent<LayoutElement>();
                dotLE.preferredWidth = 36;
                dotLE.preferredHeight = 36;

                Image dotImg = dot.AddComponent<Image>();
                dotImg.color = dotColors[i];

                // Round effect via outline
                Outline dotOutline = dot.AddComponent<Outline>();
                dotOutline.effectColor = new Color(dotColors[i].r, dotColors[i].g, dotColors[i].b, 0.5f);
                dotOutline.effectDistance = new Vector2(2, 2);
            }
        }

        private void CreateBadge(Transform parent, bool isFree, bool isPaid, bool isEarnable, bool isUnlocked)
        {
            GameObject badge = new GameObject("Badge");
            badge.transform.SetParent(parent, false);

            RectTransform badgeRT = badge.AddComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(0.5f, 0.5f);
            badgeRT.anchorMax = new Vector2(0.5f, 0.5f);
            badgeRT.pivot = new Vector2(0.5f, 0.5f);
            badgeRT.anchoredPosition = Vector2.zero;

            Color bgColor;
            Color textColor;
            string text;

            if (isFree)
            {
                bgColor = GREEN_FREE;
                textColor = Color.white;
                text = AutoLocalizer.Get("badge_free");
                badgeRT.sizeDelta = new Vector2(70, 26);
            }
            else if (isUnlocked)
            {
                bgColor = GREEN_FREE;
                textColor = Color.white;
                text = AutoLocalizer.Get("acquired_text");
                badgeRT.sizeDelta = new Vector2(110, 26);
            }
            else if (isEarnable)
            {
                bgColor = SILVER;
                textColor = new Color(0.1f, 0.1f, 0.15f, 1f);
                text = AutoLocalizer.Get("badge_earn");
                badgeRT.sizeDelta = new Vector2(70, 26);
            }
            else
            {
                bgColor = GOLD;
                textColor = Color.black;
                text = AutoLocalizer.Get("badge_pro");
                badgeRT.sizeDelta = new Vector2(55, 26);
            }

            Image badgeBg = badge.AddComponent<Image>();
            badgeBg.color = bgColor;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(badge.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 22;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
        }

        private void CreateLockOverlay(Transform parent, bool isPaid)
        {
            Sprite lockSprite = isPaid ? lockGoldSprite : lockSilverSprite;

            GameObject lockObj = new GameObject("LockOverlay");
            lockObj.transform.SetParent(parent, false);

            RectTransform lockRT = lockObj.AddComponent<RectTransform>();
            lockRT.anchorMin = new Vector2(0.5f, 0.5f);
            lockRT.anchorMax = new Vector2(0.5f, 0.5f);
            lockRT.pivot = new Vector2(0.5f, 0.5f);
            lockRT.anchoredPosition = new Vector2(0, -8);
            lockRT.sizeDelta = new Vector2(48, 48);

            Image lockImg = lockObj.AddComponent<Image>();
            if (lockSprite != null)
            {
                lockImg.sprite = lockSprite;
                lockImg.preserveAspect = true;
                lockImg.color = new Color(1f, 1f, 1f, 0.9f);
            }
            else
            {
                // Fallback: semi-transparent overlay if no sprite
                lockImg.color = new Color(0f, 0f, 0f, 0.4f);
            }
            lockImg.raycastTarget = false;
        }

        private void CreatePriceSection(Transform parent)
        {
            GameObject obj = new GameObject("PriceSection");
            obj.transform.SetParent(parent, false);

            LayoutElement le = obj.AddComponent<LayoutElement>();
            le.preferredHeight = 55;

            // Bundle price
            float bundleTotal = PRICE_PER_THEME * PAID_THEME_COUNT;
            float bundlePrice = bundleTotal * (1f - BUNDLE_DISCOUNT);
            string priceText = $"${bundlePrice:F2} USD";

            // Try to get real price from store
            if (PremiumManager.Instance != null)
            {
                string storePrice = PremiumManager.Instance.GetProductPrice(PremiumProduct.StylesPro);
                if (!string.IsNullOrEmpty(storePrice))
                {
                    priceText = storePrice;
                }
            }

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = priceText;
            tmp.fontSize = FontSizes.Subtitle;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = GOLD;
            tmp.alignment = TextAlignmentOptions.Center;

            // Strikethrough original price below
            GameObject originalObj = new GameObject("OriginalPrice");
            originalObj.transform.SetParent(parent, false);

            LayoutElement origLE = originalObj.AddComponent<LayoutElement>();
            origLE.preferredHeight = 30;

            TextMeshProUGUI origTMP = originalObj.AddComponent<TextMeshProUGUI>();
            origTMP.text = $"<s>${bundleTotal:F2}</s>  -30%";
            origTMP.fontSize = 28;
            origTMP.color = TEXT_SECONDARY;
            origTMP.alignment = TextAlignmentOptions.Center;
        }

        private void CreateBuyButton(Transform parent)
        {
            GameObject obj = new GameObject("BuyButton");
            obj.transform.SetParent(parent, false);

            LayoutElement le = obj.AddComponent<LayoutElement>();
            le.preferredHeight = 56;
            le.flexibleWidth = 1;

            Image bg = obj.AddComponent<Image>();
            bg.color = GOLD;

            Outline outline = obj.AddComponent<Outline>();
            outline.effectColor = new Color(GOLD.r, GOLD.g, GOLD.b, 0.6f);
            outline.effectDistance = new Vector2(2, 2);

            buyButton = obj.AddComponent<Button>();
            buyButton.targetGraphic = bg;
            buyButton.onClick.AddListener(OnBuyClicked);

            ColorBlock colors = buyButton.colors;
            colors.normalColor = GOLD;
            colors.highlightedColor = new Color(1f, 0.9f, 0.3f, 1f);
            colors.pressedColor = new Color(0.8f, 0.67f, 0f, 1f);
            buyButton.colors = colors;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = AutoLocalizer.Get("unlock_all_themes");
            tmp.fontSize = FontSizes.Body;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
        }

        private void CreateCancelButton(Transform parent)
        {
            GameObject obj = new GameObject("CancelButton");
            obj.transform.SetParent(parent, false);

            LayoutElement le = obj.AddComponent<LayoutElement>();
            le.preferredHeight = 50;
            le.flexibleWidth = 1;

            Image bg = obj.AddComponent<Image>();
            bg.color = BTN_SECONDARY;

            cancelButton = obj.AddComponent<Button>();
            cancelButton.targetGraphic = bg;
            cancelButton.onClick.AddListener(OnCancelClicked);

            ColorBlock colors = cancelButton.colors;
            colors.normalColor = BTN_SECONDARY;
            colors.highlightedColor = new Color(0.2f, 0.26f, 0.35f, 1f);
            colors.pressedColor = new Color(0.1f, 0.14f, 0.2f, 1f);
            cancelButton.colors = colors;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = AutoLocalizer.Get("cancel");
            tmp.fontSize = FontSizes.Body;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = TEXT_PRIMARY;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
        }

        // ── Helpers ──

        private bool IsThemeUnlocked(ThemeData theme)
        {
            if (theme == null) return false;
            if (!theme.isPremium) return true;

            // Debug override
            if (PremiumDebugController.Instance != null && PremiumDebugController.Instance.AllowThemeChange)
                return true;

            // Individual theme ownership
            if (ThemeManager.Instance != null && ThemeManager.Instance.IsThemeOwned(theme.themeId))
                return true;

            // Legacy StylesPro unlocks all paid (non-earnable) themes
            if (!theme.isEarnable && PremiumManager.Instance != null && PremiumManager.Instance.HasStylesPro)
                return true;

            return false;
        }

        // ── Button Callbacks ──

        private void OnBuyClicked()
        {
            Debug.Log("[StylesProPromptPanel] Comprar clickeado - desbloqueando todos los temas pagos");
            PremiumManager.Instance?.UnlockProduct(PremiumProduct.StylesPro);
            Hide();
        }

        private void OnCancelClicked()
        {
            Debug.Log("[StylesProPromptPanel] Cancelar/Cerrar clickeado");
            Hide();
        }

        public void Hide()
        {
            Debug.Log("[StylesProPromptPanel] Cerrando panel...");

            if (transform.parent != null)
            {
                Destroy(transform.parent.gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
