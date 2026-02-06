using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Construye la UI completa de TournamentCreate.
    /// Diseño profesional con tarjetas por sección, tema neon.
    /// Todos los campos alineados con TournamentCreateManager SerializeFields.
    /// </summary>
    public class TournamentCreateUIBuilder : EditorWindow
    {
        // ==================== COLORES DEL TEMA NEON ====================
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.4f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 1f, 1f, 0.3f);
        private static readonly Color CYAN_ACCENT = new Color(0f, 0.85f, 0.85f, 1f);

        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color PANEL_BG = new Color(0.06f, 0.09f, 0.14f, 0.98f);
        private static readonly Color CARD_BG = new Color(0.08f, 0.12f, 0.18f, 0.98f);
        private static readonly Color INPUT_BG = new Color(0.04f, 0.07f, 0.11f, 1f);
        private static readonly Color HEADER_BG = new Color(0.03f, 0.06f, 0.1f, 0.95f);
        private static readonly Color POPUP_BG = new Color(0.05f, 0.08f, 0.12f, 0.98f);

        private static readonly Color TEXT_PRIMARY = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.7f, 0.75f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color TEXT_MUTED = new Color(0.4f, 0.5f, 0.55f, 1f);

        private static readonly Color GREEN_ACCENT = new Color(0f, 1f, 0.5f, 1f);
        private static readonly Color BUTTON_PRIMARY = CYAN_NEON;
        private static readonly Color BUTTON_SECONDARY = new Color(0.12f, 0.16f, 0.22f, 1f);
        private static readonly Color BLOCKER_BG = new Color(0f, 0f, 0f, 0.85f);

        // ==================== DIMENSIONES ====================
        private const float HEADER_HEIGHT = 100f;
        private const float CONTENT_PADDING = 20f;
        private const float SECTION_SPACING = 18f;
        private const float CARD_PADDING = 18f;
        private const float CARD_INNER_SPACING = 12f;
        private const float DROPDOWN_HEIGHT = 50f;
        private const float INPUT_HEIGHT = 52f;
        private const float TOGGLE_ROW_HEIGHT = 48f;
        private const float GLOW_LINE_HEIGHT = 2f;

        [MenuItem("DigitPark/UI Builders/Tournaments/TournamentCreate", false, 233)]
        public static void BuildUI()
        {
            if (!EditorUtility.DisplayDialog("TournamentCreate UI Builder",
                "Esto reconstruira TODA la UI de TournamentCreate con el nuevo diseño profesional.\n\nAsegurate de tener la escena TournamentCreate abierta.\n\nContinuar?",
                "Si, Reconstruir", "Cancelar"))
                return;

            BuildCompleteUI();
        }

        private static void BuildCompleteUI()
        {
            Debug.Log("[TournamentCreateUIBuilder] ========== INICIANDO CONSTRUCCION V2 ==========");

            Canvas canvas = SetupCanvas();
            if (canvas == null) return;

            // Clean old children to avoid duplicates
            CleanCanvas(canvas);

            CreateBackground(canvas);
            GameObject safeArea = CreateSafeArea(canvas);

            CreateHeader(safeArea);
            CreateFormScrollView(safeArea);
            CreateLoadingOverlay(canvas);
            CreateConfirmPopup(canvas);

            MarkSceneDirty();
            Debug.Log("[TournamentCreateUIBuilder] ========== CONSTRUCCION V2 COMPLETADA ==========");
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
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            if (Camera.main == null)
            {
                GameObject cam = new GameObject("Main Camera");
                Camera c = cam.AddComponent<Camera>();
                c.tag = "MainCamera";
                c.clearFlags = CameraClearFlags.SolidColor;
                c.backgroundColor = DARK_BG;
            }

            return canvas;
        }

        private static void CleanCanvas(Canvas canvas)
        {
            // Remove old UI children (except EventSystem/Camera)
            for (int i = canvas.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(canvas.transform.GetChild(i).gameObject);
            }
        }

        private static void CreateBackground(Canvas canvas)
        {
            GameObject bg = CreateChild(canvas.gameObject, "Background");
            StretchFull(bg);
            SetImage(bg, DARK_BG);
            bg.transform.SetAsFirstSibling();
        }

        private static GameObject CreateSafeArea(Canvas canvas)
        {
            GameObject sa = CreateChild(canvas.gameObject, "SafeArea");
            StretchFull(sa);
            sa.transform.SetSiblingIndex(1);
            return sa;
        }

        // ==================== HEADER ====================

        private static void CreateHeader(GameObject parent)
        {
            GameObject header = CreateChild(parent, "Header");
            RectTransform rt = header.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0, HEADER_HEIGHT);
            SetImage(header, HEADER_BG);

            // Glow line
            CreateGlowLine(header, false);

            // BackButton
            GameObject backBtn = CreateChild(header, "BackButton");
            RectTransform backRT = backBtn.GetComponent<RectTransform>();
            backRT.anchorMin = new Vector2(0, 0.5f);
            backRT.anchorMax = new Vector2(0, 0.5f);
            backRT.pivot = new Vector2(0, 0.5f);
            backRT.anchoredPosition = new Vector2(20, 0);
            backRT.sizeDelta = new Vector2(50, 50);
            SetImage(backBtn, BUTTON_SECONDARY);
            Button backButton = backBtn.AddComponent<Button>();
            SetupButtonColors(backButton, BUTTON_SECONDARY);
            AddOutline(backBtn, CYAN_DARK);

            GameObject backText = CreateChild(backBtn, "Text");
            StretchFull(backText);
            SetText(backText, "<", 32, FontStyles.Bold, CYAN_NEON, TextAlignmentOptions.Center);

            // Title
            GameObject title = CreateChild(header, "TitleText");
            RectTransform titleRT = title.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 0.5f);
            titleRT.anchorMax = new Vector2(0.5f, 0.5f);
            titleRT.sizeDelta = new Vector2(500, 60);
            SetText(title, "CREAR TORNEO", 36, FontStyles.Bold, CYAN_NEON, TextAlignmentOptions.Center);
            AddOutline(title, CYAN_GLOW, 2);

            Debug.Log("[TournamentCreateUIBuilder] Header creado");
        }

        // ==================== FORM SCROLL VIEW ====================

        private static void CreateFormScrollView(GameObject parent)
        {
            GameObject scrollView = CreateChild(parent, "FormScrollView");
            RectTransform scrollRT = scrollView.GetComponent<RectTransform>();
            scrollRT.anchorMin = Vector2.zero;
            scrollRT.anchorMax = Vector2.one;
            scrollRT.offsetMin = new Vector2(0, 0);
            scrollRT.offsetMax = new Vector2(0, -HEADER_HEIGHT);

            ScrollRect sr = scrollView.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Elastic;
            SetImage(scrollView, Color.clear);

            // Viewport
            GameObject viewport = CreateChild(scrollView, "Viewport");
            StretchFull(viewport);
            viewport.AddComponent<RectMask2D>();
            sr.viewport = viewport.GetComponent<RectTransform>();

            // Content
            GameObject content = CreateChild(viewport, "Content");
            RectTransform contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.sizeDelta = Vector2.zero;
            sr.content = contentRT;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = SECTION_SPACING;
            vlg.padding = new RectOffset((int)CONTENT_PADDING, (int)CONTENT_PADDING, 20, 120);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // === SECTION CARDS ===
            CreateNameCard(content);
            CreateGameCard(content);
            CreateEntryFeeCard(content);
            CreatePlayersCard(content);
            CreateScheduleCard(content);
            CreateRulesCard(content);
            CreatePrivacyCard(content);
            CreatePreviewCard(content);
            CreateActionButtons(content);

            Debug.Log("[TournamentCreateUIBuilder] Form completo creado");
        }

        // ==================== CARD: NOMBRE ====================

        private static void CreateNameCard(GameObject parent)
        {
            GameObject card = CreateSectionCard(parent, "NameSection");

            CreateSectionHeader(card, "Nombre del Torneo");

            // Input
            GameObject inputObj = CreateChild(card, "TournamentNameInput");
            SetImage(inputObj, INPUT_BG);
            AddOutline(inputObj, CYAN_DARK);
            CreateInputField(inputObj, "Ingresa un nombre...");
            SetLayoutElement(inputObj, INPUT_HEIGHT);

            // Char counter
            GameObject counter = CreateChild(card, "NameCharCountText");
            SetText(counter, "0/50", 14, FontStyles.Normal, TEXT_MUTED, TextAlignmentOptions.Right);
            SetLayoutElement(counter, 20);
        }

        // ==================== CARD: TIPO DE JUEGO ====================

        private static void CreateGameCard(GameObject parent)
        {
            GameObject card = CreateSectionCard(parent, "GameSelectionSection");

            CreateSectionHeader(card, "Tipo de Juego");

            // Row: Dropdown + Icon
            GameObject row = CreateChild(card, "GameRow");
            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            SetLayoutElement(row, DROPDOWN_HEIGHT);

            // GameTypeDropdown
            GameObject dropObj = CreateChild(row, "GameTypeDropdown");
            CreateStyledDropdown(dropObj, "Seleccionar Juego",
                new string[] { "Memory Pairs", "Quick Math", "Flash Tap", "Odd One Out", "Digit Rush", "Cognitive Sprint" });
            LayoutElement dropLE = dropObj.AddComponent<LayoutElement>();
            dropLE.flexibleWidth = 1;
            dropLE.minHeight = DROPDOWN_HEIGHT;

            // SelectedGameIcon
            GameObject iconObj = CreateChild(row, "SelectedGameIcon");
            Image iconImg = SetImage(iconObj, CYAN_DARK);
            iconImg.preserveAspect = true;
            AddOutline(iconObj, CYAN_DARK);
            LayoutElement iconLE = iconObj.AddComponent<LayoutElement>();
            iconLE.minWidth = DROPDOWN_HEIGHT;
            iconLE.preferredWidth = DROPDOWN_HEIGHT;
            iconLE.minHeight = DROPDOWN_HEIGHT;
        }

        // ==================== CARD: COSTO DE ENTRADA ====================

        private static void CreateEntryFeeCard(GameObject parent)
        {
            GameObject card = CreateSectionCard(parent, "EntryFeeSection");

            CreateSectionHeader(card, "Costo de Entrada");

            // EntryFeeDropdown
            GameObject dropObj = CreateChild(card, "EntryFeeDropdown");
            CreateStyledDropdown(dropObj, "Seleccionar Entrada",
                new string[] { "Gratis", "$1.00", "$2.00", "$5.00", "$10.00", "$25.00", "$50.00", "$100.00" });
            SetLayoutElement(dropObj, DROPDOWN_HEIGHT);

            // EntryFeeSlider
            GameObject sliderSection = CreateChild(card, "SliderRow");
            SetLayoutElement(sliderSection, 35);
            HorizontalLayoutGroup sliderHLG = sliderSection.AddComponent<HorizontalLayoutGroup>();
            sliderHLG.spacing = 10;
            sliderHLG.childControlWidth = true;
            sliderHLG.childControlHeight = true;
            sliderHLG.childForceExpandWidth = false;
            sliderHLG.childForceExpandHeight = true;

            GameObject sliderLabel = CreateChild(sliderSection, "SliderLabel");
            SetText(sliderLabel, "Ajustar:", 14, FontStyles.Normal, TEXT_SECONDARY, TextAlignmentOptions.Left);
            LayoutElement sliderLabelLE = sliderLabel.AddComponent<LayoutElement>();
            sliderLabelLE.minWidth = 80;

            GameObject sliderObj = CreateChild(sliderSection, "EntryFeeSlider");
            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 7;
            slider.wholeNumbers = true;
            LayoutElement sliderLE = sliderObj.AddComponent<LayoutElement>();
            sliderLE.flexibleWidth = 1;
            sliderLE.minHeight = 30;

            // CustomEntryFeeInput (hidden)
            GameObject customObj = CreateChild(card, "CustomEntryFeeInput");
            SetImage(customObj, INPUT_BG);
            AddOutline(customObj, CYAN_DARK);
            CreateInputField(customObj, "Monto personalizado...");
            SetLayoutElement(customObj, 48);
            customObj.SetActive(false);

            // EntryFeeDisplayText
            GameObject displayObj = CreateChild(card, "EntryFeeDisplayText");
            SetText(displayObj, "GRATIS", 26, FontStyles.Bold, CYAN_NEON, TextAlignmentOptions.Center);
            SetLayoutElement(displayObj, 35);
        }

        // ==================== CARD: JUGADORES & PREMIO ====================

        private static void CreatePlayersCard(GameObject parent)
        {
            GameObject card = CreateSectionCard(parent, "MaxPlayersSection");

            CreateSectionHeader(card, "Jugadores & Premio");

            // MaxPlayersDropdown
            GameObject dropObj = CreateChild(card, "MaxPlayersDropdown");
            CreateStyledDropdown(dropObj, "Max Jugadores",
                new string[] { "8 jugadores", "16 jugadores", "32 jugadores", "64 jugadores", "128 jugadores" });
            SetLayoutElement(dropObj, DROPDOWN_HEIGHT);

            // EstimatedPrizeText
            GameObject prizeObj = CreateChild(card, "EstimatedPrizeText");
            SetText(prizeObj, "Premio estimado: $0.00", 18, FontStyles.Bold, GREEN_ACCENT, TextAlignmentOptions.Center);
            SetLayoutElement(prizeObj, 30);
        }

        // ==================== CARD: HORARIO ====================

        private static void CreateScheduleCard(GameObject parent)
        {
            GameObject card = CreateSectionCard(parent, "ScheduleSection");

            CreateSectionHeader(card, "Horario de Inicio");

            // StartImmediatelyToggle
            CreateStyledToggleRow(card, "StartImmediatelyToggle", "Iniciar Inmediatamente", true);

            // StartTimeDropdown (hidden when immediate)
            GameObject dropObj = CreateChild(card, "StartTimeDropdown");
            CreateStyledDropdown(dropObj, "Tiempo de Inicio",
                new string[] { "En 15 minutos", "En 30 minutos", "En 1 hora", "En 2 horas", "En 6 horas", "Mañana" });
            SetLayoutElement(dropObj, DROPDOWN_HEIGHT);
            dropObj.SetActive(false);

            // ScheduledTimeText
            GameObject timeObj = CreateChild(card, "ScheduledTimeText");
            SetText(timeObj, "Inicia inmediatamente al llenarse", 14, FontStyles.Italic, TEXT_MUTED, TextAlignmentOptions.Center);
            SetLayoutElement(timeObj, 22);
        }

        // ==================== CARD: REGLAS ====================

        private static void CreateRulesCard(GameObject parent)
        {
            GameObject card = CreateSectionCard(parent, "RulesSection");

            CreateSectionHeader(card, "Reglas del Torneo");

            // RoundsDropdown
            GameObject roundsObj = CreateChild(card, "RoundsDropdown");
            CreateStyledDropdown(roundsObj, "Rondas",
                new string[] { "1 ronda", "3 rondas (mejor de 3)", "5 rondas (mejor de 5)" });
            SetLayoutElement(roundsObj, DROPDOWN_HEIGHT);

            // TimeLimitDropdown
            GameObject timeObj = CreateChild(card, "TimeLimitDropdown");
            CreateStyledDropdown(timeObj, "Tiempo Limite",
                new string[] { "30 segundos", "1 minuto", "2 minutos", "5 minutos", "Sin limite" });
            SetLayoutElement(timeObj, DROPDOWN_HEIGHT);

            // AllowSpectatorsToggle
            CreateStyledToggleRow(card, "AllowSpectatorsToggle", "Permitir Espectadores", true);
        }

        // ==================== CARD: PRIVACIDAD ====================

        private static void CreatePrivacyCard(GameObject parent)
        {
            GameObject card = CreateSectionCard(parent, "PrivacySection");

            CreateSectionHeader(card, "Privacidad");

            // PrivateToggle
            CreateStyledToggleRow(card, "PrivateToggle", "Torneo Privado", false);

            // PrivateCodeInput (hidden)
            GameObject codeObj = CreateChild(card, "PrivateCodeInput");
            SetImage(codeObj, INPUT_BG);
            AddOutline(codeObj, CYAN_DARK);
            CreateInputField(codeObj, "Codigo de acceso...");
            SetLayoutElement(codeObj, 48);
            codeObj.SetActive(false);
        }

        // ==================== CARD: VISTA PREVIA ====================

        private static void CreatePreviewCard(GameObject parent)
        {
            GameObject card = CreateSectionCard(parent, "PreviewPanel");
            card.SetActive(false);

            // Override card outline for preview
            Outline ol = card.GetComponent<Outline>();
            if (ol) ol.effectColor = CYAN_ACCENT;

            CreateSectionHeader(card, "VISTA PREVIA");

            // Preview fields
            string[] fieldNames = { "PreviewNameText", "PreviewGameText", "PreviewEntryText", "PreviewPrizeText", "PreviewPlayersText" };
            string[] fieldLabels = { "Nombre: ---", "Juego: ---", "Entrada: ---", "Premio: ---", "Jugadores: ---" };

            for (int i = 0; i < fieldNames.Length; i++)
            {
                // Row container with optional background
                GameObject row = CreateChild(card, fieldNames[i] + "_Row");
                if (i % 2 == 0)
                    SetImage(row, new Color(0.04f, 0.07f, 0.1f, 0.5f));

                HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
                hlg.padding = new RectOffset(12, 12, 4, 4);
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = true;
                hlg.childForceExpandHeight = true;
                SetLayoutElement(row, 30);

                // Text child (named for AutoAssigner matching)
                GameObject textObj = CreateChild(row, fieldNames[i]);
                SetText(textObj, fieldLabels[i], 16, FontStyles.Normal, TEXT_PRIMARY, TextAlignmentOptions.Left);
            }
        }

        // ==================== ACTION BUTTONS ====================

        private static void CreateActionButtons(GameObject parent)
        {
            GameObject section = CreateChild(parent, "ActionSection");
            VerticalLayoutGroup vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 12;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            SetLayoutElement(section, 200);

            // Preview Button (secondary)
            GameObject previewBtn = CreateChild(section, "PreviewButton");
            SetImage(previewBtn, BUTTON_SECONDARY);
            Button prevButton = previewBtn.AddComponent<Button>();
            SetupButtonColors(prevButton, BUTTON_SECONDARY);
            AddOutline(previewBtn, CYAN_DARK);
            SetLayoutElement(previewBtn, 52);

            GameObject prevText = CreateChild(previewBtn, "Text");
            StretchFull(prevText);
            SetText(prevText, "Vista Previa", 18, FontStyles.Bold, TEXT_PRIMARY, TextAlignmentOptions.Center);

            // Creation Fee Text
            GameObject feeObj = CreateChild(section, "CreationFeeText");
            SetText(feeObj, "Costo de creacion: $5.00", 14, FontStyles.Normal, TEXT_MUTED, TextAlignmentOptions.Center);
            SetLayoutElement(feeObj, 22);

            // Main Create Button
            GameObject createBtn = CreateChild(section, "CreateTournamentButton");
            SetImage(createBtn, BUTTON_PRIMARY);
            Button createButton = createBtn.AddComponent<Button>();
            SetupButtonColors(createButton, BUTTON_PRIMARY);
            AddOutline(createBtn, CYAN_GLOW, 3);
            SetLayoutElement(createBtn, 65);

            // Glow behind button
            GameObject btnGlow = CreateChild(section, "CreateButtonGlow");
            SetImage(btnGlow, new Color(0f, 1f, 1f, 0.06f));
            SetLayoutElement(btnGlow, 4);

            GameObject createText = CreateChild(createBtn, "Text");
            StretchFull(createText);
            SetText(createText, "CREAR TORNEO", 24, FontStyles.Bold, TEXT_DARK, TextAlignmentOptions.Center);

            // CreateButtonText (info below)
            GameObject createInfoObj = CreateChild(section, "CreateButtonText");
            SetText(createInfoObj, "Se requiere nombre de al menos 3 caracteres", 13, FontStyles.Italic, TEXT_MUTED, TextAlignmentOptions.Center);
            SetLayoutElement(createInfoObj, 20);
        }

        // ==================== LOADING OVERLAY ====================

        private static void CreateLoadingOverlay(Canvas canvas)
        {
            GameObject overlay = CreateChild(canvas.gameObject, "LoadingOverlay");
            overlay.SetActive(false);
            StretchFull(overlay);
            SetImage(overlay, new Color(0, 0, 0, 0.75f));

            // Center container
            GameObject center = CreateChild(overlay, "Center");
            RectTransform centerRT = center.GetComponent<RectTransform>();
            centerRT.anchorMin = new Vector2(0.2f, 0.4f);
            centerRT.anchorMax = new Vector2(0.8f, 0.6f);
            centerRT.offsetMin = Vector2.zero;
            centerRT.offsetMax = Vector2.zero;

            VerticalLayoutGroup vlg = center.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 20;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Spinner
            GameObject spinner = CreateChild(center, "Spinner");
            SetImage(spinner, CYAN_NEON);
            AddOutline(spinner, CYAN_GLOW, 3);
            LayoutElement spinLE = spinner.AddComponent<LayoutElement>();
            spinLE.minWidth = 60;
            spinLE.preferredWidth = 60;
            spinLE.minHeight = 60;
            spinLE.preferredHeight = 60;

            // StatusText
            GameObject textObj = CreateChild(center, "StatusText");
            SetText(textObj, "Creando torneo...", 20, FontStyles.Normal, TEXT_PRIMARY, TextAlignmentOptions.Center);
            LayoutElement textLE = textObj.AddComponent<LayoutElement>();
            textLE.minHeight = 30;

            Debug.Log("[TournamentCreateUIBuilder] LoadingOverlay creado");
        }

        // ==================== CONFIRM POPUP ====================

        private static void CreateConfirmPopup(Canvas canvas)
        {
            // Blocker
            GameObject blocker = CreateChild(canvas.gameObject, "ConfirmBlocker");
            blocker.SetActive(false);
            StretchFull(blocker);
            SetImage(blocker, BLOCKER_BG);
            Button blockerBtn = blocker.AddComponent<Button>();
            blockerBtn.transition = Selectable.Transition.None;
            blocker.transform.SetAsLastSibling();

            // Popup card
            GameObject popup = CreateChild(blocker, "ConfirmPopup");
            RectTransform popupRT = popup.GetComponent<RectTransform>();
            popupRT.anchorMin = new Vector2(0.5f, 0.5f);
            popupRT.anchorMax = new Vector2(0.5f, 0.5f);
            popupRT.sizeDelta = new Vector2(460, 300);
            SetImage(popup, POPUP_BG);
            AddOutline(popup, CYAN_DARK, 2);

            VerticalLayoutGroup vlg = popup.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 20;
            vlg.padding = new RectOffset(30, 30, 25, 25);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Glow line top
            CreateGlowLine(popup, true);

            // Title
            GameObject titleObj = CreateChild(popup, "Title");
            SetText(titleObj, "Confirmar Creacion", 26, FontStyles.Bold, CYAN_NEON, TextAlignmentOptions.Center);
            SetLayoutElement(titleObj, 40);

            // Message
            GameObject msgObj = CreateChild(popup, "Message");
            SetText(msgObj, "¿Estas seguro de que quieres\ncrear este torneo?", 18, FontStyles.Normal, TEXT_PRIMARY, TextAlignmentOptions.Center);
            SetLayoutElement(msgObj, 60);

            // Buttons row
            GameObject buttons = CreateChild(popup, "Buttons");
            HorizontalLayoutGroup btnHlg = buttons.AddComponent<HorizontalLayoutGroup>();
            btnHlg.spacing = 20;
            btnHlg.childControlWidth = true;
            btnHlg.childControlHeight = true;
            btnHlg.childForceExpandWidth = true;
            btnHlg.childForceExpandHeight = true;
            SetLayoutElement(buttons, 55);

            // Cancel
            GameObject cancelObj = CreateChild(buttons, "CancelButton");
            SetImage(cancelObj, BUTTON_SECONDARY);
            Button cancelBtn = cancelObj.AddComponent<Button>();
            SetupButtonColors(cancelBtn, BUTTON_SECONDARY);
            AddOutline(cancelObj, CYAN_DARK);
            GameObject cancelText = CreateChild(cancelObj, "Text");
            StretchFull(cancelText);
            SetText(cancelText, "Cancelar", 18, FontStyles.Bold, TEXT_PRIMARY, TextAlignmentOptions.Center);

            // Confirm
            GameObject confirmObj = CreateChild(buttons, "ConfirmButton");
            SetImage(confirmObj, BUTTON_PRIMARY);
            Button confirmBtn = confirmObj.AddComponent<Button>();
            SetupButtonColors(confirmBtn, BUTTON_PRIMARY);
            AddOutline(confirmObj, CYAN_GLOW, 2);
            GameObject confirmText = CreateChild(confirmObj, "Text");
            StretchFull(confirmText);
            SetText(confirmText, "Confirmar", 18, FontStyles.Bold, TEXT_DARK, TextAlignmentOptions.Center);

            Debug.Log("[TournamentCreateUIBuilder] ConfirmPopup creado");
        }

        // ==================== SECTION CARD FACTORY ====================

        private static GameObject CreateSectionCard(GameObject parent, string name)
        {
            GameObject card = CreateChild(parent, name);

            SetImage(card, CARD_BG);
            AddOutline(card, CYAN_DARK);

            VerticalLayoutGroup vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = CARD_INNER_SPACING;
            vlg.padding = new RectOffset((int)CARD_PADDING, (int)CARD_PADDING, 15, 15);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            return card;
        }

        private static void CreateSectionHeader(GameObject card, string label)
        {
            GameObject headerRow = CreateChild(card, "SectionHeader");
            HorizontalLayoutGroup hlg = headerRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            SetLayoutElement(headerRow, 28);

            // Label
            GameObject labelObj = CreateChild(headerRow, "Label");
            SetText(labelObj, label, 17, FontStyles.Bold, TEXT_SECONDARY, TextAlignmentOptions.Left);
            LayoutElement labelLE = labelObj.AddComponent<LayoutElement>();
            labelLE.minWidth = 200;
            labelLE.preferredWidth = 200;

            // Separator line
            GameObject line = CreateChild(headerRow, "Line");
            SetImage(line, new Color(CYAN_NEON.r, CYAN_NEON.g, CYAN_NEON.b, 0.2f));
            LayoutElement lineLE = line.AddComponent<LayoutElement>();
            lineLE.flexibleWidth = 1;
            lineLE.minHeight = GLOW_LINE_HEIGHT;
            lineLE.preferredHeight = GLOW_LINE_HEIGHT;
        }

        // ==================== STYLED COMPONENTS ====================

        private static void CreateStyledDropdown(GameObject obj, string caption, string[] options)
        {
            SetImage(obj, INPUT_BG);
            AddOutline(obj, CYAN_DARK);

            TMP_Dropdown dropdown = obj.AddComponent<TMP_Dropdown>();
            dropdown.ClearOptions();
            var optList = new System.Collections.Generic.List<TMP_Dropdown.OptionData>();
            foreach (string opt in options)
                optList.Add(new TMP_Dropdown.OptionData(opt));
            dropdown.AddOptions(optList);

            // Caption text
            GameObject capObj = CreateChild(obj, "Label");
            StretchFull(capObj);
            RectTransform capRT = capObj.GetComponent<RectTransform>();
            capRT.offsetMin = new Vector2(14, 0);
            capRT.offsetMax = new Vector2(-35, 0);
            TextMeshProUGUI capText = SetText(capObj, caption, 16, FontStyles.Normal, TEXT_PRIMARY, TextAlignmentOptions.Left);
            dropdown.captionText = capText;

            // Arrow indicator
            GameObject arrow = CreateChild(obj, "Arrow");
            RectTransform arrowRT = arrow.GetComponent<RectTransform>();
            arrowRT.anchorMin = new Vector2(1, 0.5f);
            arrowRT.anchorMax = new Vector2(1, 0.5f);
            arrowRT.pivot = new Vector2(1, 0.5f);
            arrowRT.anchoredPosition = new Vector2(-12, 0);
            arrowRT.sizeDelta = new Vector2(20, 20);
            SetText(arrow, "▼", 12, FontStyles.Normal, TEXT_MUTED, TextAlignmentOptions.Center);
        }

        private static void CreateStyledToggleRow(GameObject parent, string name, string label, bool defaultValue)
        {
            GameObject row = CreateChild(parent, name);
            SetImage(row, INPUT_BG);
            AddOutline(row, CYAN_DARK);
            SetLayoutElement(row, TOGGLE_ROW_HEIGHT);

            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.padding = new RectOffset(14, 14, 8, 8);
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            // Toggle component on the row
            Toggle toggle = row.AddComponent<Toggle>();
            toggle.isOn = defaultValue;

            // Checkmark box
            GameObject checkBg = CreateChild(row, "Background");
            SetImage(checkBg, new Color(0.03f, 0.05f, 0.08f, 1f));
            AddOutline(checkBg, CYAN_DARK);
            LayoutElement checkLE = checkBg.AddComponent<LayoutElement>();
            checkLE.minWidth = 32;
            checkLE.preferredWidth = 32;
            checkLE.minHeight = 32;

            GameObject checkmark = CreateChild(checkBg, "Checkmark");
            StretchFull(checkmark);
            RectTransform cmRT = checkmark.GetComponent<RectTransform>();
            cmRT.offsetMin = new Vector2(4, 4);
            cmRT.offsetMax = new Vector2(-4, -4);
            Image cmImage = SetImage(checkmark, CYAN_NEON);

            toggle.graphic = cmImage;
            toggle.targetGraphic = checkBg.GetComponent<Image>();

            // Label
            GameObject labelObj = CreateChild(row, "Label");
            SetText(labelObj, label, 16, FontStyles.Normal, TEXT_PRIMARY, TextAlignmentOptions.Left);
            LayoutElement labelLE = labelObj.AddComponent<LayoutElement>();
            labelLE.flexibleWidth = 1;
        }

        private static void CreateInputField(GameObject obj, string placeholder)
        {
            TMP_InputField inputField = obj.GetComponent<TMP_InputField>();
            if (inputField != null) return;

            inputField = obj.AddComponent<TMP_InputField>();

            GameObject textArea = CreateChild(obj, "Text Area");
            StretchFull(textArea);
            RectTransform textAreaRT = textArea.GetComponent<RectTransform>();
            textAreaRT.offsetMin = new Vector2(14, 5);
            textAreaRT.offsetMax = new Vector2(-14, -5);
            textArea.AddComponent<RectMask2D>();

            GameObject placeholderObj = CreateChild(textArea, "Placeholder");
            TextMeshProUGUI phText = SetText(placeholderObj, placeholder, 17, FontStyles.Italic, TEXT_MUTED, TextAlignmentOptions.MidlineLeft);
            StretchFull(placeholderObj);

            GameObject textObj = CreateChild(textArea, "Text");
            TextMeshProUGUI inputText = SetText(textObj, "", 17, FontStyles.Normal, TEXT_PRIMARY, TextAlignmentOptions.MidlineLeft);
            StretchFull(textObj);

            inputField.textViewport = textAreaRT;
            inputField.textComponent = inputText;
            inputField.placeholder = phText;
            inputField.caretColor = CYAN_NEON;
            inputField.selectionColor = new Color(0f, 1f, 1f, 0.25f);
        }

        private static void CreateGlowLine(GameObject parent, bool atTop)
        {
            GameObject glow = CreateChild(parent, atTop ? "TopGlow" : "BottomGlow");
            RectTransform rt = glow.GetComponent<RectTransform>();
            if (atTop)
            {
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(0.5f, 1);
            }
            else
            {
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(1, 0);
                rt.pivot = new Vector2(0.5f, 1);
            }
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0, 3);
            SetImage(glow, CYAN_NEON);
        }

        // ==================== UTILITY: LOW LEVEL ====================

        private static GameObject CreateChild(GameObject parent, string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent.transform, false);
            child.AddComponent<RectTransform>();
            return child;
        }

        private static void StretchFull(GameObject obj)
        {
            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt == null) rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }

        private static Image SetImage(GameObject obj, Color color)
        {
            Image img = obj.GetComponent<Image>();
            if (img == null) img = obj.AddComponent<Image>();
            img.color = color;
            return img;
        }

        private static TextMeshProUGUI SetText(GameObject obj, string text, float fontSize, FontStyles style, Color color, TextAlignmentOptions align)
        {
            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = align;
            return tmp;
        }

        private static void SetLayoutElement(GameObject obj, float minHeight)
        {
            LayoutElement le = obj.GetComponent<LayoutElement>();
            if (le == null) le = obj.AddComponent<LayoutElement>();
            le.minHeight = minHeight;
            le.preferredHeight = minHeight;
        }

        private static void SetupButtonColors(Button btn, Color baseColor)
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
            if (outline == null) outline = obj.AddComponent<Outline>();
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
