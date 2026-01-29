using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Construye la UI completa de Onboarding (Tutorial Inicial)
    /// Incluye: SafeArea, Slides, Progress Dots, Skip Button, Welcome Gift
    /// </summary>
    public class OnboardingUIBuilder : EditorWindow
    {
        // ==================== COLORES DEL TEMA NEON ====================
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.4f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 1f, 1f, 0.3f);

        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color PANEL_BG = new Color(0.08f, 0.12f, 0.18f, 0.98f);
        private static readonly Color CARD_BG = new Color(0.06f, 0.1f, 0.15f, 1f);

        private static readonly Color TEXT_PRIMARY = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.7f, 0.75f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.02f, 0.05f, 0.1f, 1f);

        private static readonly Color BUTTON_PRIMARY = CYAN_NEON;
        private static readonly Color BUTTON_SECONDARY = new Color(0.15f, 0.2f, 0.25f, 1f);
        private static readonly Color BUTTON_SUCCESS = new Color(0.2f, 0.8f, 0.4f, 1f);

        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color PURPLE_PREMIUM = new Color(0.6f, 0.3f, 0.9f, 1f);

        private static readonly Color GEM_COLOR = new Color(0.4f, 0.8f, 1f, 1f);
        private static readonly Color COIN_COLOR = new Color(1f, 0.85f, 0.3f, 1f);

        private static readonly Color DOT_ACTIVE = CYAN_NEON;
        private static readonly Color DOT_INACTIVE = new Color(0.3f, 0.35f, 0.4f, 1f);

        private static readonly Color BLOCKER_BG = new Color(0f, 0f, 0f, 0.85f);

        // ==================== DIMENSIONES ====================
        private const float CONTENT_PADDING = 30f;
        private const float SLIDE_IMAGE_HEIGHT = 400f;
        private const float DOTS_HEIGHT = 30f;
        private const float BUTTON_HEIGHT = 60f;

        [MenuItem("DigitPark/UI Builders/Core/Onboarding", false, 153)]
        public static void BuildUI()
        {
            if (!EditorUtility.DisplayDialog("Onboarding UI Builder",
                "Esto construira la UI completa de Onboarding.\nAsegurate de tener la escena Onboarding abierta.\n\nContinuar?",
                "Si", "No"))
                return;

            BuildCompleteUI();
        }

        private static void BuildCompleteUI()
        {
            Debug.Log("[OnboardingUIBuilder] ========== INICIANDO CONSTRUCCION ==========");

            Canvas canvas = SetupCanvas();
            if (canvas == null) return;

            CreateBackground(canvas);
            GameObject safeArea = CreateSafeArea(canvas);

            CreateSkipButton(safeArea);
            CreateSlidesContainer(safeArea);
            CreateProgressDots(safeArea);
            CreateNavigationButtons(safeArea);

            CreateWelcomeGiftPopup(canvas);

            MarkSceneDirty();
            Debug.Log("[OnboardingUIBuilder] ========== CONSTRUCCION COMPLETADA ==========");
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

        // ==================== SKIP BUTTON ====================

        private static void CreateSkipButton(GameObject parent)
        {
            GameObject skipBtn = FindOrCreateChild(parent, "SkipButton");

            RectTransform skipRT = GetOrAddComponent<RectTransform>(skipBtn);
            skipRT.anchorMin = new Vector2(1, 1);
            skipRT.anchorMax = new Vector2(1, 1);
            skipRT.pivot = new Vector2(1, 1);
            skipRT.anchoredPosition = new Vector2(-20, -40);
            skipRT.sizeDelta = new Vector2(100, 45);

            Image skipBg = GetOrAddComponent<Image>(skipBtn);
            skipBg.color = new Color(0, 0, 0, 0.3f);

            Button skipButton = GetOrAddComponent<Button>(skipBtn);
            SetupButtonColors(skipButton, skipBg.color);
            AddOutline(skipBtn, CYAN_DARK * 0.5f);

            GameObject skipTextObj = FindOrCreateChild(skipBtn, "Text");
            TextMeshProUGUI skipText = GetOrAddComponent<TextMeshProUGUI>(skipTextObj);
            skipText.text = "Saltar";
            skipText.fontSize = 16;
            skipText.color = TEXT_SECONDARY;
            skipText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(skipTextObj);

            Debug.Log("[OnboardingUIBuilder] SkipButton creado");
        }

        // ==================== SLIDES CONTAINER ====================

        private static void CreateSlidesContainer(GameObject parent)
        {
            GameObject slidesContainer = FindOrCreateChild(parent, "SlidesContainer");

            RectTransform slidesRT = GetOrAddComponent<RectTransform>(slidesContainer);
            slidesRT.anchorMin = new Vector2(0, 0.25f);
            slidesRT.anchorMax = new Vector2(1, 0.85f);
            slidesRT.offsetMin = new Vector2(CONTENT_PADDING, 0);
            slidesRT.offsetMax = new Vector2(-CONTENT_PADDING, 0);

            // Create individual slides (only first one visible)
            CreateSlide(slidesContainer, "Slide1",
                "Bienvenido a DigitPark!",
                "El mejor lugar para jugar, competir y ganar premios reales.",
                CYAN_NEON, true, 1);

            CreateSlide(slidesContainer, "Slide2",
                "Juega y Gana",
                "Participa en torneos, desafia a otros jugadores y gana monedas reales.",
                GOLD, false, 2);

            CreateSlide(slidesContainer, "Slide3",
                "Misiones Diarias",
                "Completa misiones cada dia para obtener gemas, monedas y recompensas exclusivas.",
                GEM_COLOR, false, 3);

            CreateSlide(slidesContainer, "Slide4",
                "Pase de Batalla",
                "Desbloquea recompensas increibles mientras subes de nivel cada temporada.",
                PURPLE_PREMIUM, false, 4);

            CreateSlide(slidesContainer, "Slide5",
                "Listo para Comenzar!",
                "Reclama tu regalo de bienvenida y empieza a jugar ahora.",
                BUTTON_SUCCESS, false, 5);

            Debug.Log("[OnboardingUIBuilder] SlidesContainer creado");
        }

        private static void CreateSlide(GameObject parent, string name, string title, string description, Color accentColor, bool isActive, int slideNumber)
        {
            GameObject slide = FindOrCreateChild(parent, name);
            slide.SetActive(isActive);

            SetRectTransformStretch(slide);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(slide);
            vlg.spacing = 30;
            vlg.padding = new RectOffset(20, 20, 40, 40);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Illustration Container
            GameObject illustrationContainer = FindOrCreateChild(slide, "IllustrationContainer");
            LayoutElement illustrationLE = GetOrAddComponent<LayoutElement>(illustrationContainer);
            illustrationLE.minHeight = SLIDE_IMAGE_HEIGHT;
            illustrationLE.preferredHeight = SLIDE_IMAGE_HEIGHT;
            illustrationLE.flexibleHeight = 1;

            // Illustration Background Circle
            GameObject illustrationBg = FindOrCreateChild(illustrationContainer, "IllustrationBg");
            RectTransform bgRT = GetOrAddComponent<RectTransform>(illustrationBg);
            bgRT.anchorMin = new Vector2(0.5f, 0.5f);
            bgRT.anchorMax = new Vector2(0.5f, 0.5f);
            bgRT.sizeDelta = new Vector2(300, 300);

            Image bgImage = GetOrAddComponent<Image>(illustrationBg);
            bgImage.color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.15f);

            // Illustration Placeholder
            GameObject illustration = FindOrCreateChild(illustrationContainer, "Illustration");
            RectTransform illustrationRT = GetOrAddComponent<RectTransform>(illustration);
            illustrationRT.anchorMin = new Vector2(0.5f, 0.5f);
            illustrationRT.anchorMax = new Vector2(0.5f, 0.5f);
            illustrationRT.sizeDelta = new Vector2(200, 200);

            Image illustrationImage = GetOrAddComponent<Image>(illustration);
            illustrationImage.color = accentColor;

            // Slide Number Badge
            GameObject numberBadge = FindOrCreateChild(illustrationContainer, "NumberBadge");
            RectTransform badgeRT = GetOrAddComponent<RectTransform>(numberBadge);
            badgeRT.anchorMin = new Vector2(0.5f, 0.5f);
            badgeRT.anchorMax = new Vector2(0.5f, 0.5f);
            badgeRT.sizeDelta = new Vector2(60, 60);
            badgeRT.anchoredPosition = new Vector2(0, 0);

            Image badgeImage = GetOrAddComponent<Image>(numberBadge);
            badgeImage.color = accentColor;

            GameObject badgeText = FindOrCreateChild(numberBadge, "Text");
            TextMeshProUGUI badgeTmp = GetOrAddComponent<TextMeshProUGUI>(badgeText);
            badgeTmp.text = slideNumber.ToString();
            badgeTmp.fontSize = 28;
            badgeTmp.fontStyle = FontStyles.Bold;
            badgeTmp.color = TEXT_DARK;
            badgeTmp.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(badgeText);

            // Title
            GameObject titleObj = FindOrCreateChild(slide, "Title");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = title;
            titleText.fontSize = 32;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = accentColor;
            titleText.alignment = TextAlignmentOptions.Center;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 45;
            AddOutline(titleObj, new Color(accentColor.r, accentColor.g, accentColor.b, 0.4f), 2);

            // Description
            GameObject descObj = FindOrCreateChild(slide, "Description");
            TextMeshProUGUI descText = GetOrAddComponent<TextMeshProUGUI>(descObj);
            descText.text = description;
            descText.fontSize = 18;
            descText.color = TEXT_SECONDARY;
            descText.alignment = TextAlignmentOptions.Center;
            LayoutElement descLE = GetOrAddComponent<LayoutElement>(descObj);
            descLE.minHeight = 60;

            // Feature Highlights (for some slides)
            if (slideNumber == 2 || slideNumber == 3)
            {
                GameObject features = FindOrCreateChild(slide, "Features");
                HorizontalLayoutGroup featuresHlg = GetOrAddComponent<HorizontalLayoutGroup>(features);
                featuresHlg.spacing = 30;
                featuresHlg.childAlignment = TextAnchor.MiddleCenter;
                featuresHlg.childControlWidth = false;
                featuresHlg.childControlHeight = true;

                LayoutElement featuresLE = GetOrAddComponent<LayoutElement>(features);
                featuresLE.minHeight = 80;

                if (slideNumber == 2)
                {
                    CreateFeatureItem(features, "Feature1", "Torneos", GOLD);
                    CreateFeatureItem(features, "Feature2", "1v1", CYAN_NEON);
                    CreateFeatureItem(features, "Feature3", "Premios", BUTTON_SUCCESS);
                }
                else
                {
                    CreateFeatureItem(features, "Feature1", "Gemas", GEM_COLOR);
                    CreateFeatureItem(features, "Feature2", "Monedas", COIN_COLOR);
                    CreateFeatureItem(features, "Feature3", "Cofres", PURPLE_PREMIUM);
                }
            }
        }

        private static void CreateFeatureItem(GameObject parent, string name, string label, Color color)
        {
            GameObject item = FindOrCreateChild(parent, name);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(item);
            vlg.spacing = 8;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            LayoutElement itemLE = GetOrAddComponent<LayoutElement>(item);
            itemLE.minWidth = 80;

            // Icon
            GameObject iconObj = FindOrCreateChild(item, "Icon");
            Image iconImage = GetOrAddComponent<Image>(iconObj);
            iconImage.color = color;
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(iconObj);
            iconLE.minWidth = 45;
            iconLE.minHeight = 45;
            iconLE.preferredWidth = 45;
            iconLE.preferredHeight = 45;

            // Label
            GameObject labelObj = FindOrCreateChild(item, "Label");
            TextMeshProUGUI labelText = GetOrAddComponent<TextMeshProUGUI>(labelObj);
            labelText.text = label;
            labelText.fontSize = 14;
            labelText.fontStyle = FontStyles.Bold;
            labelText.color = color;
            labelText.alignment = TextAlignmentOptions.Center;
            LayoutElement labelLE = GetOrAddComponent<LayoutElement>(labelObj);
            labelLE.minHeight = 20;
        }

        // ==================== PROGRESS DOTS ====================

        private static void CreateProgressDots(GameObject parent)
        {
            GameObject dotsContainer = FindOrCreateChild(parent, "ProgressDots");

            RectTransform dotsRT = GetOrAddComponent<RectTransform>(dotsContainer);
            dotsRT.anchorMin = new Vector2(0.5f, 0);
            dotsRT.anchorMax = new Vector2(0.5f, 0);
            dotsRT.pivot = new Vector2(0.5f, 0);
            dotsRT.anchoredPosition = new Vector2(0, 170);
            dotsRT.sizeDelta = new Vector2(200, DOTS_HEIGHT);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(dotsContainer);
            hlg.spacing = 15;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            // Create 5 dots
            for (int i = 1; i <= 5; i++)
            {
                CreateDot(dotsContainer, $"Dot{i}", i == 1);
            }

            Debug.Log("[OnboardingUIBuilder] ProgressDots creado");
        }

        private static void CreateDot(GameObject parent, string name, bool isActive)
        {
            GameObject dot = FindOrCreateChild(parent, name);

            Image dotImage = GetOrAddComponent<Image>(dot);
            dotImage.color = isActive ? DOT_ACTIVE : DOT_INACTIVE;

            LayoutElement dotLE = GetOrAddComponent<LayoutElement>(dot);
            dotLE.minWidth = isActive ? 30 : 12;
            dotLE.minHeight = 12;
            dotLE.preferredWidth = isActive ? 30 : 12;
            dotLE.preferredHeight = 12;
        }

        // ==================== NAVIGATION BUTTONS ====================

        private static void CreateNavigationButtons(GameObject parent)
        {
            GameObject navContainer = FindOrCreateChild(parent, "NavigationButtons");

            RectTransform navRT = GetOrAddComponent<RectTransform>(navContainer);
            navRT.anchorMin = new Vector2(0, 0);
            navRT.anchorMax = new Vector2(1, 0);
            navRT.pivot = new Vector2(0.5f, 0);
            navRT.anchoredPosition = new Vector2(0, 50);
            navRT.sizeDelta = new Vector2(-CONTENT_PADDING * 2, BUTTON_HEIGHT + 30);

            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(navContainer);
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;

            // Back Button (hidden on first slide)
            GameObject backBtn = FindOrCreateChild(navContainer, "BackButton");
            backBtn.SetActive(false);

            Image backBg = GetOrAddComponent<Image>(backBtn);
            backBg.color = BUTTON_SECONDARY;

            Button backButton = GetOrAddComponent<Button>(backBtn);
            SetupButtonColors(backButton, BUTTON_SECONDARY);
            AddOutline(backBtn, CYAN_DARK);

            LayoutElement backLE = GetOrAddComponent<LayoutElement>(backBtn);
            backLE.minHeight = BUTTON_HEIGHT;

            GameObject backTextObj = FindOrCreateChild(backBtn, "Text");
            TextMeshProUGUI backText = GetOrAddComponent<TextMeshProUGUI>(backTextObj);
            backText.text = "Atras";
            backText.fontSize = 20;
            backText.fontStyle = FontStyles.Bold;
            backText.color = TEXT_PRIMARY;
            backText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(backTextObj);

            // Next/Start Button
            GameObject nextBtn = FindOrCreateChild(navContainer, "NextButton");

            Image nextBg = GetOrAddComponent<Image>(nextBtn);
            nextBg.color = CYAN_NEON;

            Button nextButton = GetOrAddComponent<Button>(nextBtn);
            SetupButtonColors(nextButton, CYAN_NEON);
            AddOutline(nextBtn, CYAN_GLOW, 2);

            // Glow effect
            Shadow glow = GetOrAddComponent<Shadow>(nextBtn);
            glow.effectColor = new Color(0f, 1f, 1f, 0.3f);
            glow.effectDistance = new Vector2(0, -3);

            LayoutElement nextLE = GetOrAddComponent<LayoutElement>(nextBtn);
            nextLE.minHeight = BUTTON_HEIGHT;

            GameObject nextTextObj = FindOrCreateChild(nextBtn, "Text");
            TextMeshProUGUI nextText = GetOrAddComponent<TextMeshProUGUI>(nextTextObj);
            nextText.text = "Siguiente";
            nextText.fontSize = 20;
            nextText.fontStyle = FontStyles.Bold;
            nextText.color = TEXT_DARK;
            nextText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(nextTextObj);

            Debug.Log("[OnboardingUIBuilder] NavigationButtons creado");
        }

        // ==================== WELCOME GIFT POPUP ====================

        private static void CreateWelcomeGiftPopup(Canvas canvas)
        {
            GameObject blocker = FindOrCreateChild(canvas.gameObject, "WelcomeGiftBlocker");
            blocker.SetActive(false);

            SetRectTransformStretch(blocker);
            Image blockerBg = GetOrAddComponent<Image>(blocker);
            blockerBg.color = BLOCKER_BG;
            blocker.transform.SetAsLastSibling();

            GameObject popup = FindOrCreateChild(blocker, "WelcomeGiftPopup");
            RectTransform popupRT = GetOrAddComponent<RectTransform>(popup);
            popupRT.anchorMin = new Vector2(0.5f, 0.5f);
            popupRT.anchorMax = new Vector2(0.5f, 0.5f);
            popupRT.sizeDelta = new Vector2(500, 600);

            Image popupBg = GetOrAddComponent<Image>(popup);
            popupBg.color = new Color(0.05f, 0.08f, 0.12f, 0.98f);
            AddOutline(popup, GOLD, 3);

            // Glow effect
            Shadow popupShadow = GetOrAddComponent<Shadow>(popup);
            popupShadow.effectColor = new Color(1f, 0.84f, 0f, 0.3f);
            popupShadow.effectDistance = new Vector2(0, -5);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(popup);
            vlg.spacing = 20;
            vlg.padding = new RectOffset(30, 30, 40, 30);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Celebration Icon
            GameObject celebrationIcon = FindOrCreateChild(popup, "CelebrationIcon");
            Image celebrationImage = GetOrAddComponent<Image>(celebrationIcon);
            celebrationImage.color = GOLD;
            LayoutElement celebrationLE = GetOrAddComponent<LayoutElement>(celebrationIcon);
            celebrationLE.minWidth = 100;
            celebrationLE.minHeight = 100;
            celebrationLE.preferredWidth = 100;
            celebrationLE.preferredHeight = 100;

            // Title
            GameObject titleObj = FindOrCreateChild(popup, "Title");
            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "REGALO DE BIENVENIDA!";
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GOLD;
            titleText.alignment = TextAlignmentOptions.Center;
            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 40;
            AddOutline(titleObj, new Color(1f, 0.84f, 0f, 0.4f), 2);

            // Subtitle
            GameObject subtitleObj = FindOrCreateChild(popup, "Subtitle");
            TextMeshProUGUI subtitleText = GetOrAddComponent<TextMeshProUGUI>(subtitleObj);
            subtitleText.text = "Gracias por unirte a DigitPark!";
            subtitleText.fontSize = 16;
            subtitleText.color = TEXT_SECONDARY;
            subtitleText.alignment = TextAlignmentOptions.Center;
            LayoutElement subtitleLE = GetOrAddComponent<LayoutElement>(subtitleObj);
            subtitleLE.minHeight = 25;

            // Rewards Container
            GameObject rewardsContainer = FindOrCreateChild(popup, "RewardsContainer");
            Image rewardsBg = GetOrAddComponent<Image>(rewardsContainer);
            rewardsBg.color = CARD_BG;
            LayoutElement rewardsLE = GetOrAddComponent<LayoutElement>(rewardsContainer);
            rewardsLE.minHeight = 180;

            VerticalLayoutGroup rewardsVlg = GetOrAddComponent<VerticalLayoutGroup>(rewardsContainer);
            rewardsVlg.spacing = 15;
            rewardsVlg.padding = new RectOffset(20, 20, 20, 20);
            rewardsVlg.childAlignment = TextAnchor.MiddleCenter;
            rewardsVlg.childControlWidth = true;
            rewardsVlg.childControlHeight = true;
            rewardsVlg.childForceExpandWidth = true;
            rewardsVlg.childForceExpandHeight = false;

            // Rewards header
            GameObject rewardsHeader = FindOrCreateChild(rewardsContainer, "Header");
            TextMeshProUGUI rewardsHeaderText = GetOrAddComponent<TextMeshProUGUI>(rewardsHeader);
            rewardsHeaderText.text = "TUS REGALOS";
            rewardsHeaderText.fontSize = 14;
            rewardsHeaderText.fontStyle = FontStyles.Bold;
            rewardsHeaderText.color = CYAN_NEON;
            rewardsHeaderText.alignment = TextAlignmentOptions.Center;
            LayoutElement headerLE = GetOrAddComponent<LayoutElement>(rewardsHeader);
            headerLE.minHeight = 22;

            // Rewards Grid
            GameObject rewardsGrid = FindOrCreateChild(rewardsContainer, "RewardsGrid");
            HorizontalLayoutGroup gridHlg = GetOrAddComponent<HorizontalLayoutGroup>(rewardsGrid);
            gridHlg.spacing = 25;
            gridHlg.childAlignment = TextAnchor.MiddleCenter;
            gridHlg.childControlWidth = false;
            gridHlg.childControlHeight = true;
            gridHlg.childForceExpandWidth = false;

            LayoutElement gridLE = GetOrAddComponent<LayoutElement>(rewardsGrid);
            gridLE.minHeight = 100;

            // Gift items
            CreateGiftItem(rewardsGrid, "Gift1", GEM_COLOR, "100", "Gemas");
            CreateGiftItem(rewardsGrid, "Gift2", COIN_COLOR, "500", "Monedas");
            CreateGiftItem(rewardsGrid, "Gift3", PURPLE_PREMIUM, "1", "Cofre Epico");

            // Claim Button
            GameObject claimBtn = FindOrCreateChild(popup, "ClaimButton");
            Image claimBg = GetOrAddComponent<Image>(claimBtn);
            claimBg.color = BUTTON_SUCCESS;
            Button claimButton = GetOrAddComponent<Button>(claimBtn);
            SetupButtonColors(claimButton, BUTTON_SUCCESS);
            AddOutline(claimBtn, new Color(0.3f, 1f, 0.5f, 0.5f), 3);

            // Glow effect
            Shadow claimShadow = GetOrAddComponent<Shadow>(claimBtn);
            claimShadow.effectColor = new Color(0.2f, 0.8f, 0.4f, 0.4f);
            claimShadow.effectDistance = new Vector2(0, -4);

            LayoutElement claimLE = GetOrAddComponent<LayoutElement>(claimBtn);
            claimLE.minHeight = 65;

            GameObject claimTextObj = FindOrCreateChild(claimBtn, "Text");
            TextMeshProUGUI claimText = GetOrAddComponent<TextMeshProUGUI>(claimTextObj);
            claimText.text = "RECLAMAR Y JUGAR!";
            claimText.fontSize = 22;
            claimText.fontStyle = FontStyles.Bold;
            claimText.color = TEXT_DARK;
            claimText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(claimTextObj);

            Debug.Log("[OnboardingUIBuilder] WelcomeGiftPopup creado");
        }

        private static void CreateGiftItem(GameObject parent, string name, Color color, string amount, string label)
        {
            GameObject item = FindOrCreateChild(parent, name);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(item);
            vlg.spacing = 8;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            LayoutElement itemLE = GetOrAddComponent<LayoutElement>(item);
            itemLE.minWidth = 100;

            // Icon
            GameObject iconObj = FindOrCreateChild(item, "Icon");
            Image iconImage = GetOrAddComponent<Image>(iconObj);
            iconImage.color = color;
            LayoutElement iconLE = GetOrAddComponent<LayoutElement>(iconObj);
            iconLE.minWidth = 50;
            iconLE.minHeight = 50;
            iconLE.preferredWidth = 50;
            iconLE.preferredHeight = 50;

            // Amount
            GameObject amountObj = FindOrCreateChild(item, "Amount");
            TextMeshProUGUI amountText = GetOrAddComponent<TextMeshProUGUI>(amountObj);
            amountText.text = $"+{amount}";
            amountText.fontSize = 20;
            amountText.fontStyle = FontStyles.Bold;
            amountText.color = color;
            amountText.alignment = TextAlignmentOptions.Center;
            LayoutElement amountLE = GetOrAddComponent<LayoutElement>(amountObj);
            amountLE.minHeight = 26;

            // Label
            GameObject labelObj = FindOrCreateChild(item, "Label");
            TextMeshProUGUI labelText = GetOrAddComponent<TextMeshProUGUI>(labelObj);
            labelText.text = label;
            labelText.fontSize = 12;
            labelText.color = TEXT_SECONDARY;
            labelText.alignment = TextAlignmentOptions.Center;
            LayoutElement labelLE = GetOrAddComponent<LayoutElement>(labelObj);
            labelLE.minHeight = 18;
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
