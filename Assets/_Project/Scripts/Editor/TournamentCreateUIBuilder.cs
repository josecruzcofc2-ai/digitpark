using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Construye la UI completa de TournamentCreate
    /// Incluye: SafeArea, Header, Formulario de creacion, ConfirmPopup
    /// </summary>
    public class TournamentCreateUIBuilder : EditorWindow
    {
        // ==================== COLORES DEL TEMA NEON ====================
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.4f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 1f, 1f, 0.3f);

        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color PANEL_BG = new Color(0.08f, 0.12f, 0.18f, 0.98f);
        private static readonly Color POPUP_BG = new Color(0.05f, 0.08f, 0.12f, 0.98f);
        private static readonly Color INPUT_BG = new Color(0.04f, 0.08f, 0.12f, 1f);
        private static readonly Color HEADER_BG = new Color(0.03f, 0.06f, 0.1f, 0.95f);

        private static readonly Color TEXT_PRIMARY = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.7f, 0.75f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color TEXT_MUTED = new Color(0.4f, 0.5f, 0.55f, 1f);

        private static readonly Color BUTTON_PRIMARY = CYAN_NEON;
        private static readonly Color BUTTON_SECONDARY = new Color(0.15f, 0.2f, 0.25f, 1f);
        private static readonly Color BUTTON_DANGER = new Color(0.9f, 0.2f, 0.2f, 1f);

        private static readonly Color BLOCKER_BG = new Color(0f, 0f, 0f, 0.85f);

        // ==================== DIMENSIONES ====================
        private const float HEADER_HEIGHT = 100f;
        private const float CONTENT_PADDING = 25f;
        private const float SECTION_SPACING = 25f;
        private const float FIELD_HEIGHT = 55f;

        [MenuItem("DigitPark/Tournaments/Build TournamentCreate UI", false, 11)]
        public static void BuildUI()
        {
            if (!EditorUtility.DisplayDialog("TournamentCreate UI Builder",
                "Esto construira la UI completa de TournamentCreate.\nAsegurate de tener la escena TournamentCreate abierta.\n\nContinuar?",
                "Si", "No"))
                return;

            BuildCompleteUI();
        }

        private static void BuildCompleteUI()
        {
            Debug.Log("[TournamentCreateUIBuilder] ========== INICIANDO CONSTRUCCION ==========");

            Canvas canvas = SetupCanvas();
            if (canvas == null) return;

            CreateBackground(canvas);
            GameObject safeArea = CreateSafeArea(canvas);

            CreateHeader(safeArea);
            CreateFormScrollView(safeArea);
            CreateConfirmPopup(canvas);

            MarkSceneDirty();
            Debug.Log("[TournamentCreateUIBuilder] ========== CONSTRUCCION COMPLETADA ==========");
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
            titleRT.sizeDelta = new Vector2(500, 60);

            TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleObj);
            titleText.text = "CREAR TORNEO";
            titleText.fontSize = 38;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = CYAN_NEON;
            titleText.alignment = TextAlignmentOptions.Center;
            AddOutline(titleObj, CYAN_GLOW, 2);

            Debug.Log("[TournamentCreateUIBuilder] Header creado");
        }

        // ==================== FORM SCROLL VIEW ====================

        private static void CreateFormScrollView(GameObject parent)
        {
            GameObject scrollView = FindOrCreateChild(parent, "FormScrollView");

            RectTransform scrollRT = GetOrAddComponent<RectTransform>(scrollView);
            scrollRT.anchorMin = Vector2.zero;
            scrollRT.anchorMax = Vector2.one;
            scrollRT.offsetMin = new Vector2(CONTENT_PADDING, CONTENT_PADDING);
            scrollRT.offsetMax = new Vector2(-CONTENT_PADDING, -HEADER_HEIGHT - 10);

            ScrollRect scrollRect = GetOrAddComponent<ScrollRect>(scrollView);
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;

            Image scrollBg = GetOrAddComponent<Image>(scrollView);
            scrollBg.color = Color.clear;

            // Viewport
            GameObject viewport = FindOrCreateChild(scrollView, "Viewport");
            RectTransform viewportRT = GetOrAddComponent<RectTransform>(viewport);
            SetRectTransformStretch(viewport);
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
            vlg.spacing = SECTION_SPACING;
            vlg.padding = new RectOffset(10, 10, 20, 100);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Form Fields
            CreateInputSection(content, "TournamentNameSection", "Nombre del Torneo", "Ingresa un nombre...", "TournamentNameInput");
            CreateGameSelectionSection(content);
            CreateEntryFeeSection(content);
            CreateMaxPlayersSection(content);
            CreatePrivacySection(content);
            CreateCreateButton(content);

            Debug.Log("[TournamentCreateUIBuilder] Form creado");
        }

        private static void CreateInputSection(GameObject parent, string sectionName, string label, string placeholder, string inputName)
        {
            GameObject section = FindOrCreateChild(parent, sectionName);

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(section);
            vlg.spacing = 10;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            LayoutElement sectionLE = GetOrAddComponent<LayoutElement>(section);
            sectionLE.minHeight = 95;

            // Label
            GameObject labelObj = FindOrCreateChild(section, "Label");
            TextMeshProUGUI labelText = GetOrAddComponent<TextMeshProUGUI>(labelObj);
            labelText.text = label;
            labelText.fontSize = 18;
            labelText.fontStyle = FontStyles.Bold;
            labelText.color = TEXT_SECONDARY;

            LayoutElement labelLE = GetOrAddComponent<LayoutElement>(labelObj);
            labelLE.minHeight = 25;

            // Input
            GameObject inputObj = FindOrCreateChild(section, inputName);
            CreateInputField(inputObj, placeholder);

            LayoutElement inputLE = GetOrAddComponent<LayoutElement>(inputObj);
            inputLE.minHeight = FIELD_HEIGHT;
            inputLE.preferredHeight = FIELD_HEIGHT;

            Image inputBg = GetOrAddComponent<Image>(inputObj);
            inputBg.color = INPUT_BG;
            AddOutline(inputObj, CYAN_DARK);
        }

        private static void CreateGameSelectionSection(GameObject parent)
        {
            GameObject section = FindOrCreateChild(parent, "GameSelectionSection");

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(section);
            vlg.spacing = 15;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            LayoutElement sectionLE = GetOrAddComponent<LayoutElement>(section);
            sectionLE.minHeight = 180;

            // Label
            GameObject labelObj = FindOrCreateChild(section, "Label");
            TextMeshProUGUI labelText = GetOrAddComponent<TextMeshProUGUI>(labelObj);
            labelText.text = "Selecciona el Juego";
            labelText.fontSize = 18;
            labelText.fontStyle = FontStyles.Bold;
            labelText.color = TEXT_SECONDARY;

            LayoutElement labelLE = GetOrAddComponent<LayoutElement>(labelObj);
            labelLE.minHeight = 25;

            // Games Grid
            GameObject grid = FindOrCreateChild(section, "GamesGrid");
            GridLayoutGroup glg = GetOrAddComponent<GridLayoutGroup>(grid);
            glg.cellSize = new Vector2(180, 70);
            glg.spacing = new Vector2(15, 15);
            glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            glg.startAxis = GridLayoutGroup.Axis.Horizontal;
            glg.childAlignment = TextAnchor.MiddleCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 2;

            LayoutElement gridLE = GetOrAddComponent<LayoutElement>(grid);
            gridLE.minHeight = 140;

            string[] games = { "Digit Rush", "Memory Pairs", "Quick Math", "Flash Tap" };
            for (int i = 0; i < games.Length; i++)
            {
                CreateGameToggle(grid, games[i], i == 0);
            }
        }

        private static void CreateGameToggle(GameObject parent, string gameName, bool isSelected)
        {
            string safeName = gameName.Replace(" ", "");
            GameObject toggle = FindOrCreateChild(parent, $"Game_{safeName}");

            Image toggleBg = GetOrAddComponent<Image>(toggle);
            toggleBg.color = isSelected ? CYAN_DARK : INPUT_BG;

            Button toggleBtn = GetOrAddComponent<Button>(toggle);
            SetupButtonColors(toggleBtn, isSelected ? CYAN_DARK : INPUT_BG);
            AddOutline(toggle, isSelected ? CYAN_NEON : CYAN_DARK);

            GameObject textObj = FindOrCreateChild(toggle, "Text");
            TextMeshProUGUI text = GetOrAddComponent<TextMeshProUGUI>(textObj);
            text.text = gameName;
            text.fontSize = 16;
            text.fontStyle = isSelected ? FontStyles.Bold : FontStyles.Normal;
            text.color = isSelected ? CYAN_NEON : TEXT_PRIMARY;
            text.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(textObj);
        }

        private static void CreateEntryFeeSection(GameObject parent)
        {
            GameObject section = FindOrCreateChild(parent, "EntryFeeSection");

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(section);
            vlg.spacing = 15;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            LayoutElement sectionLE = GetOrAddComponent<LayoutElement>(section);
            sectionLE.minHeight = 120;

            // Label
            GameObject labelObj = FindOrCreateChild(section, "Label");
            TextMeshProUGUI labelText = GetOrAddComponent<TextMeshProUGUI>(labelObj);
            labelText.text = "Costo de Entrada";
            labelText.fontSize = 18;
            labelText.fontStyle = FontStyles.Bold;
            labelText.color = TEXT_SECONDARY;

            LayoutElement labelLE = GetOrAddComponent<LayoutElement>(labelObj);
            labelLE.minHeight = 25;

            // Fee Options
            GameObject options = FindOrCreateChild(section, "FeeOptions");
            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(options);
            hlg.spacing = 12;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            LayoutElement optionsLE = GetOrAddComponent<LayoutElement>(options);
            optionsLE.minHeight = 55;

            string[] fees = { "GRATIS", "$1", "$5", "$10" };
            for (int i = 0; i < fees.Length; i++)
            {
                CreateFeeButton(options, fees[i], i == 0);
            }
        }

        private static void CreateFeeButton(GameObject parent, string fee, bool isSelected)
        {
            string safeName = fee.Replace("$", "").Replace(" ", "");
            if (safeName == "GRATIS") safeName = "Free";
            GameObject btn = FindOrCreateChild(parent, $"Fee_{safeName}");

            Image btnBg = GetOrAddComponent<Image>(btn);
            btnBg.color = isSelected ? BUTTON_PRIMARY : BUTTON_SECONDARY;

            Button button = GetOrAddComponent<Button>(btn);
            SetupButtonColors(button, isSelected ? BUTTON_PRIMARY : BUTTON_SECONDARY);
            AddOutline(btn, isSelected ? CYAN_GLOW : CYAN_DARK);

            GameObject textObj = FindOrCreateChild(btn, "Text");
            TextMeshProUGUI text = GetOrAddComponent<TextMeshProUGUI>(textObj);
            text.text = fee;
            text.fontSize = 16;
            text.fontStyle = FontStyles.Bold;
            text.color = isSelected ? TEXT_DARK : TEXT_PRIMARY;
            text.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(textObj);

            LayoutElement le = GetOrAddComponent<LayoutElement>(btn);
            le.flexibleWidth = 1;
        }

        private static void CreateMaxPlayersSection(GameObject parent)
        {
            GameObject section = FindOrCreateChild(parent, "MaxPlayersSection");

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(section);
            vlg.spacing = 15;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            LayoutElement sectionLE = GetOrAddComponent<LayoutElement>(section);
            sectionLE.minHeight = 120;

            // Label
            GameObject labelObj = FindOrCreateChild(section, "Label");
            TextMeshProUGUI labelText = GetOrAddComponent<TextMeshProUGUI>(labelObj);
            labelText.text = "Maximo de Jugadores";
            labelText.fontSize = 18;
            labelText.fontStyle = FontStyles.Bold;
            labelText.color = TEXT_SECONDARY;

            LayoutElement labelLE = GetOrAddComponent<LayoutElement>(labelObj);
            labelLE.minHeight = 25;

            // Player Count Options
            GameObject options = FindOrCreateChild(section, "PlayerOptions");
            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(options);
            hlg.spacing = 15;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            LayoutElement optionsLE = GetOrAddComponent<LayoutElement>(options);
            optionsLE.minHeight = 55;

            int[] counts = { 4, 8, 16, 32 };
            for (int i = 0; i < counts.Length; i++)
            {
                CreatePlayerCountButton(options, counts[i], i == 1);
            }
        }

        private static void CreatePlayerCountButton(GameObject parent, int count, bool isSelected)
        {
            GameObject btn = FindOrCreateChild(parent, $"Players_{count}");

            Image btnBg = GetOrAddComponent<Image>(btn);
            btnBg.color = isSelected ? BUTTON_PRIMARY : BUTTON_SECONDARY;

            Button button = GetOrAddComponent<Button>(btn);
            SetupButtonColors(button, isSelected ? BUTTON_PRIMARY : BUTTON_SECONDARY);
            AddOutline(btn, isSelected ? CYAN_GLOW : CYAN_DARK);

            GameObject textObj = FindOrCreateChild(btn, "Text");
            TextMeshProUGUI text = GetOrAddComponent<TextMeshProUGUI>(textObj);
            text.text = count.ToString();
            text.fontSize = 20;
            text.fontStyle = FontStyles.Bold;
            text.color = isSelected ? TEXT_DARK : TEXT_PRIMARY;
            text.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(textObj);

            LayoutElement le = GetOrAddComponent<LayoutElement>(btn);
            le.flexibleWidth = 1;
        }

        private static void CreatePrivacySection(GameObject parent)
        {
            GameObject section = FindOrCreateChild(parent, "PrivacySection");

            VerticalLayoutGroup vlg = GetOrAddComponent<VerticalLayoutGroup>(section);
            vlg.spacing = 15;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            LayoutElement sectionLE = GetOrAddComponent<LayoutElement>(section);
            sectionLE.minHeight = 120;

            // Label
            GameObject labelObj = FindOrCreateChild(section, "Label");
            TextMeshProUGUI labelText = GetOrAddComponent<TextMeshProUGUI>(labelObj);
            labelText.text = "Privacidad";
            labelText.fontSize = 18;
            labelText.fontStyle = FontStyles.Bold;
            labelText.color = TEXT_SECONDARY;

            LayoutElement labelLE = GetOrAddComponent<LayoutElement>(labelObj);
            labelLE.minHeight = 25;

            // Privacy Options
            GameObject options = FindOrCreateChild(section, "PrivacyOptions");
            HorizontalLayoutGroup hlg = GetOrAddComponent<HorizontalLayoutGroup>(options);
            hlg.spacing = 20;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            LayoutElement optionsLE = GetOrAddComponent<LayoutElement>(options);
            optionsLE.minHeight = 55;

            CreatePrivacyButton(options, "Publico", true);
            CreatePrivacyButton(options, "Privado", false);
        }

        private static void CreatePrivacyButton(GameObject parent, string label, bool isSelected)
        {
            GameObject btn = FindOrCreateChild(parent, $"Privacy_{label}");

            Image btnBg = GetOrAddComponent<Image>(btn);
            btnBg.color = isSelected ? BUTTON_PRIMARY : BUTTON_SECONDARY;

            Button button = GetOrAddComponent<Button>(btn);
            SetupButtonColors(button, isSelected ? BUTTON_PRIMARY : BUTTON_SECONDARY);
            AddOutline(btn, isSelected ? CYAN_GLOW : CYAN_DARK);

            GameObject textObj = FindOrCreateChild(btn, "Text");
            TextMeshProUGUI text = GetOrAddComponent<TextMeshProUGUI>(textObj);
            text.text = label;
            text.fontSize = 18;
            text.fontStyle = FontStyles.Bold;
            text.color = isSelected ? TEXT_DARK : TEXT_PRIMARY;
            text.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(textObj);

            LayoutElement le = GetOrAddComponent<LayoutElement>(btn);
            le.flexibleWidth = 1;
        }

        private static void CreateCreateButton(GameObject parent)
        {
            GameObject btn = FindOrCreateChild(parent, "CreateTournamentButton");

            Image btnBg = GetOrAddComponent<Image>(btn);
            btnBg.color = BUTTON_PRIMARY;

            Button button = GetOrAddComponent<Button>(btn);
            SetupButtonColors(button, BUTTON_PRIMARY);
            AddOutline(btn, CYAN_GLOW, 3);

            LayoutElement le = GetOrAddComponent<LayoutElement>(btn);
            le.minHeight = 70;
            le.preferredHeight = 70;

            GameObject textObj = FindOrCreateChild(btn, "Text");
            TextMeshProUGUI text = GetOrAddComponent<TextMeshProUGUI>(textObj);
            text.text = "CREAR TORNEO";
            text.fontSize = 24;
            text.fontStyle = FontStyles.Bold;
            text.color = TEXT_DARK;
            text.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(textObj);
        }

        // ==================== CONFIRM POPUP ====================

        private static void CreateConfirmPopup(Canvas canvas)
        {
            // Blocker
            GameObject blocker = FindOrCreateChild(canvas.gameObject, "ConfirmBlocker");
            blocker.SetActive(false);

            RectTransform blockerRT = GetOrAddComponent<RectTransform>(blocker);
            SetRectTransformStretch(blocker);

            Image blockerBg = GetOrAddComponent<Image>(blocker);
            blockerBg.color = BLOCKER_BG;

            Button blockerBtn = GetOrAddComponent<Button>(blocker);
            blockerBtn.transition = Selectable.Transition.None;

            blocker.transform.SetAsLastSibling();

            // Popup
            GameObject popup = FindOrCreateChild(blocker, "ConfirmPopup");

            RectTransform popupRT = GetOrAddComponent<RectTransform>(popup);
            popupRT.anchorMin = new Vector2(0.5f, 0.5f);
            popupRT.anchorMax = new Vector2(0.5f, 0.5f);
            popupRT.sizeDelta = new Vector2(450, 280);

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
            titleText.text = "Confirmar Creacion";
            titleText.fontSize = 26;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = CYAN_NEON;
            titleText.alignment = TextAlignmentOptions.Center;

            LayoutElement titleLE = GetOrAddComponent<LayoutElement>(titleObj);
            titleLE.minHeight = 40;

            // Message
            GameObject msgObj = FindOrCreateChild(popup, "Message");
            TextMeshProUGUI msgText = GetOrAddComponent<TextMeshProUGUI>(msgObj);
            msgText.text = "Estas seguro de que quieres\ncrear este torneo?";
            msgText.fontSize = 18;
            msgText.color = TEXT_PRIMARY;
            msgText.alignment = TextAlignmentOptions.Center;

            LayoutElement msgLE = GetOrAddComponent<LayoutElement>(msgObj);
            msgLE.minHeight = 60;

            // Buttons
            GameObject buttons = FindOrCreateChild(popup, "Buttons");
            HorizontalLayoutGroup btnHlg = GetOrAddComponent<HorizontalLayoutGroup>(buttons);
            btnHlg.spacing = 20;
            btnHlg.childControlWidth = true;
            btnHlg.childControlHeight = true;
            btnHlg.childForceExpandWidth = true;
            btnHlg.childForceExpandHeight = true;

            LayoutElement btnLE = GetOrAddComponent<LayoutElement>(buttons);
            btnLE.minHeight = 55;

            // Cancel Button
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

            // Confirm Button
            GameObject confirmBtn = FindOrCreateChild(buttons, "ConfirmButton");
            Image confirmBg = GetOrAddComponent<Image>(confirmBtn);
            confirmBg.color = BUTTON_PRIMARY;
            Button confirmButton = GetOrAddComponent<Button>(confirmBtn);
            SetupButtonColors(confirmButton, BUTTON_PRIMARY);
            AddOutline(confirmBtn, CYAN_GLOW, 2);

            GameObject confirmTextObj = FindOrCreateChild(confirmBtn, "Text");
            TextMeshProUGUI confirmText = GetOrAddComponent<TextMeshProUGUI>(confirmTextObj);
            confirmText.text = "Confirmar";
            confirmText.fontSize = 18;
            confirmText.fontStyle = FontStyles.Bold;
            confirmText.color = TEXT_DARK;
            confirmText.alignment = TextAlignmentOptions.Center;
            SetRectTransformStretch(confirmTextObj);

            Debug.Log("[TournamentCreateUIBuilder] ConfirmPopup creado");
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

        private static void CreateInputField(GameObject obj, string placeholder)
        {
            TMP_InputField inputField = obj.GetComponent<TMP_InputField>();

            if (inputField == null)
            {
                inputField = obj.AddComponent<TMP_InputField>();

                GameObject textArea = FindOrCreateChild(obj, "Text Area");
                SetRectTransformStretch(textArea);
                RectTransform textAreaRT = textArea.GetComponent<RectTransform>();
                textAreaRT.offsetMin = new Vector2(15, 5);
                textAreaRT.offsetMax = new Vector2(-15, -5);
                GetOrAddComponent<RectMask2D>(textArea);

                GameObject placeholderObj = FindOrCreateChild(textArea, "Placeholder");
                TextMeshProUGUI placeholderText = GetOrAddComponent<TextMeshProUGUI>(placeholderObj);
                placeholderText.text = placeholder;
                placeholderText.fontSize = 18;
                placeholderText.fontStyle = FontStyles.Italic;
                placeholderText.color = TEXT_MUTED;
                placeholderText.alignment = TextAlignmentOptions.MidlineLeft;
                SetRectTransformStretch(placeholderObj);

                GameObject textObj = FindOrCreateChild(textArea, "Text");
                TextMeshProUGUI textComponent = GetOrAddComponent<TextMeshProUGUI>(textObj);
                textComponent.fontSize = 18;
                textComponent.color = TEXT_PRIMARY;
                textComponent.alignment = TextAlignmentOptions.MidlineLeft;
                SetRectTransformStretch(textObj);

                inputField.textViewport = textAreaRT;
                inputField.textComponent = textComponent;
                inputField.placeholder = placeholderText;
            }

            inputField.caretColor = CYAN_NEON;
            inputField.selectionColor = new Color(0f, 1f, 1f, 0.3f);
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
