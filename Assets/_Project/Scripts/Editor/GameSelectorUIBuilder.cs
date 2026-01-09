using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;
using System.Collections.Generic;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para reconstruir la UI del GameSelector con diseño profesional
    /// </summary>
    public class GameSelectorUIBuilder : EditorWindow
    {
        // Colores del tema neón
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color CARD_BG = new Color(0.05f, 0.1f, 0.15f, 0.9f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.5f, 0.5f, 1f);

        [MenuItem("DigitPark/Rebuild GameSelector UI")]
        public static void ShowWindow()
        {
            GetWindow<GameSelectorUIBuilder>("GameSelector UI Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("GameSelector UI Builder", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Este script reconstruirá la UI del GameSelector.\n" +
                "Asegúrate de tener la escena GameSelector abierta.",
                MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("Reconstruir GameSelector UI", GUILayout.Height(40)))
            {
                RebuildGameSelectorUI();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Solo crear Cards (mantener estructura)", GUILayout.Height(30)))
            {
                CreateGameCardsOnly();
            }
        }

        private static void RebuildGameSelectorUI()
        {
            // Buscar el Canvas existente
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No se encontró Canvas en la escena");
                return;
            }

            // Buscar o crear el contenedor principal
            Transform canvasTransform = canvas.transform;

            // Buscar Background
            Transform background = canvasTransform.Find("Background");
            if (background == null)
            {
                Debug.LogError("No se encontró Background");
                return;
            }

            // Limpiar GamesPanel existente si existe
            Transform existingGamesPanel = canvasTransform.Find("GamesPanel");
            if (existingGamesPanel != null)
            {
                // Guardar referencias antes de destruir
                Debug.Log("Encontrado GamesPanel existente, se reconstruirá");
            }

            // Limpiar CognitiveSprintButton duplicados fuera del GamesPanel
            // Primero recolectar, luego destruir para evitar MissingReferenceException
            List<GameObject> toDestroy = new List<GameObject>();
            for (int i = 0; i < canvasTransform.childCount; i++)
            {
                Transform child = canvasTransform.GetChild(i);
                if (child != null && child.name == "CognitiveSprintButton")
                {
                    toDestroy.Add(child.gameObject);
                }
            }
            foreach (GameObject obj in toDestroy)
            {
                Debug.Log("Eliminando CognitiveSprintButton duplicado fuera de GamesPanel");
                DestroyImmediate(obj);
            }

            // Crear nueva estructura
            CreateNewGameSelectorLayout(canvasTransform);

            Debug.Log("GameSelector UI reconstruida exitosamente!");
            EditorUtility.SetDirty(canvas.gameObject);
        }

        private static void CreateNewGameSelectorLayout(Transform canvasTransform)
        {
            // ========== HEADER ==========
            GameObject header = CreateOrFind(canvasTransform, "Header");
            RectTransform headerRect = SetupRectTransform(header,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -60), new Vector2(0, 120)); // Header con más presencia

            // BackButton - NO crear, el usuario usa su propio prefab

            // Title - valores ajustados por el usuario
            GameObject title = CreateOrFind(header.transform, "TitleText");
            SetupRectTransform(title,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -5), new Vector2(800, 70)); // Height 70
            SetupTitleText(title, "Selecciona un Juego");

            // ========== GAMES GRID ==========
            GameObject gamesPanel = CreateOrFind(canvasTransform, "GamesPanel");
            RectTransform gamesPanelRect = SetupRectTransform(gamesPanel,
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(0, -40), new Vector2(-60, -120)); // Más espacio para los cards

            // Agregar Grid Layout
            GridLayoutGroup grid = gamesPanel.GetComponent<GridLayoutGroup>();
            if (grid == null) grid = gamesPanel.AddComponent<GridLayoutGroup>();

            grid.cellSize = new Vector2(420, 420); // Cards cuadrados para las imágenes
            grid.spacing = new Vector2(30, 30);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.MiddleCenter; // CENTRADO
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.padding = new RectOffset(40, 40, 20, 20);

            // ========== CREAR GAME CARDS ==========
            CreateGameCard(gamesPanel.transform, "DigitRushButton", "Digit Rush", "IconPlaceholder_DigitRush", false);
            CreateGameCard(gamesPanel.transform, "MemoryPairsButton", "Memory Pairs", "IconPlaceholder_MemoryPairs", false);
            CreateGameCard(gamesPanel.transform, "QuickMathButton", "Quick Math", "IconPlaceholder_QuickMath", false);
            CreateGameCard(gamesPanel.transform, "FlashTapButton", "Flash Tap", "IconPlaceholder_FlashTap", false);
            CreateGameCard(gamesPanel.transform, "OddOneOutButton", "Odd One Out", "IconPlaceholder_OddOneOut", false);
            CreateGameCard(gamesPanel.transform, "CognitiveSprintButton", "Cognitive Sprint", "IconPlaceholder_Sprint", true);

            // ========== COGNITIVE SPRINT PANEL (oculto por defecto) ==========
            CreateCognitiveSprintPanel(canvasTransform);

            // ========== RULES PANEL (oculto por defecto) ==========
            CreateRulesPanel(canvasTransform);
        }

        private static void CreateGameCard(Transform parent, string buttonName, string gameName, string iconName, bool isGold)
        {
            // Card Container (Button)
            GameObject card = CreateOrFind(parent, buttonName);

            // Limpiar children antiguos que ya no necesitamos
            string[] oldChildren = { "IconContainer", "GameNameText", "IconText", "Border", "InnerBackground", "SpecialBadge",
                                     "DigitRushBtnText", "MemoryPairsBtnText", "QuickMathBtnText", "FlashTapBtnText",
                                     "OddOneOutBtnText", "CognitiveSprintBtnText", "ArrowText" };
            foreach (string childName in oldChildren)
            {
                Transform oldChild = card.transform.Find(childName);
                if (oldChild != null) DestroyImmediate(oldChild.gameObject);
            }

            // Limpiar Outlines duplicados
            Outline[] existingOutlines = card.GetComponents<Outline>();
            for (int i = existingOutlines.Length - 1; i >= 0; i--)
            {
                DestroyImmediate(existingOutlines[i]);
            }

            // Configurar Button
            Button btn = card.GetComponent<Button>();
            if (btn == null) btn = card.AddComponent<Button>();

            // Imagen de fondo del card (será la imagen del juego)
            Image cardBg = card.GetComponent<Image>();
            if (cardBg == null) cardBg = card.AddComponent<Image>();
            cardBg.color = Color.white; // Blanco para mostrar la imagen sin tinte
            cardBg.preserveAspect = true;
            // La imagen se asignará manualmente desde el inspector
            // Ruta: Assets/_Project/Art/Icons/Games/[NombreJuego]Icon.png

            // Configurar colores del botón para efecto hover
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); // Brillo sutil en hover
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            colors.selectedColor = Color.white;
            btn.colors = colors;

            // ========== BORDE NEÓN EXTERIOR ==========
            // Usamos Outline para el efecto de borde neón
            Outline cardOutline = card.GetComponent<Outline>();
            if (cardOutline == null) cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = isGold ? GOLD : CYAN_NEON;
            cardOutline.effectDistance = new Vector2(4, 4);

            // Segundo outline para más glow
            Outline cardOutline2 = card.GetComponents<Outline>().Length > 1
                ? card.GetComponents<Outline>()[1]
                : card.AddComponent<Outline>();
            cardOutline2.effectColor = isGold
                ? new Color(1f, 0.84f, 0f, 0.5f)
                : new Color(0f, 1f, 1f, 0.5f);
            cardOutline2.effectDistance = new Vector2(8, 8);

            // ========== BADGE PRO (solo para Cognitive Sprint) ==========
            if (isGold)
            {
                GameObject specialBadge = CreateOrFind(card.transform, "SpecialBadge");
                RectTransform badgeRect = SetupRectTransform(specialBadge,
                    new Vector2(1, 1), new Vector2(1, 1),
                    new Vector2(-15, -15), new Vector2(70, 28));

                Image badgeImg = specialBadge.GetComponent<Image>();
                if (badgeImg == null) badgeImg = specialBadge.AddComponent<Image>();
                badgeImg.color = GOLD;
                badgeImg.raycastTarget = false;

                GameObject badgeText = CreateOrFind(specialBadge.transform, "BadgeText");
                SetupRectTransform(badgeText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

                TextMeshProUGUI badgeTmp = badgeText.GetComponent<TextMeshProUGUI>();
                if (badgeTmp == null) badgeTmp = badgeText.AddComponent<TextMeshProUGUI>();
                badgeTmp.text = "PRO";
                badgeTmp.fontSize = 16;
                badgeTmp.fontStyle = FontStyles.Bold;
                badgeTmp.color = DARK_BG;
                badgeTmp.alignment = TextAlignmentOptions.Center;
                badgeTmp.raycastTarget = false;
            }

            // Agregar componente de efectos
            GameCardEffect effect = card.GetComponent<GameCardEffect>();
            if (effect == null) effect = card.AddComponent<GameCardEffect>();

            // Configurar colores según si es gold o no
            if (isGold)
            {
                effect.SetColors(GOLD, new Color(1f, 0.9f, 0.4f, 1f), new Color(0.8f, 0.67f, 0f, 1f));
            }
            else
            {
                effect.SetColors(CYAN_NEON, new Color(0.4f, 1f, 1f, 1f), CYAN_DARK);
            }
        }

        private static void CreateCognitiveSprintPanel(Transform canvasTransform)
        {
            GameObject panel = CreateOrFind(canvasTransform, "CognitiveSprintPanel");
            RectTransform panelRect = SetupRectTransform(panel,
                Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero);

            // Fondo oscuro semi-transparente
            Image panelBg = panel.GetComponent<Image>();
            if (panelBg == null) panelBg = panel.AddComponent<Image>();
            panelBg.color = new Color(0, 0, 0, 0.9f);

            // Panel interior
            GameObject innerPanel = CreateOrFind(panel.transform, "InnerPanel");
            RectTransform innerPanelRect = SetupRectTransform(innerPanel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(800, 900));

            Image innerPanelBg = innerPanel.GetComponent<Image>();
            if (innerPanelBg == null) innerPanelBg = innerPanel.AddComponent<Image>();
            innerPanelBg.color = CARD_BG;

            // Borde neón dorado para el panel
            Outline panelOutline = innerPanel.GetComponent<Outline>();
            if (panelOutline == null) panelOutline = innerPanel.AddComponent<Outline>();
            panelOutline.effectColor = GOLD;
            panelOutline.effectDistance = new Vector2(3, 3);

            // Título del panel
            GameObject panelTitle = CreateOrFind(innerPanel.transform, "PanelTitle");
            SetupRectTransform(panelTitle,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -50), new Vector2(0, 80));

            TextMeshProUGUI titleTmp = panelTitle.GetComponent<TextMeshProUGUI>();
            if (titleTmp == null) titleTmp = panelTitle.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "Cognitive Sprint";
            titleTmp.fontSize = 48;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = GOLD;
            titleTmp.alignment = TextAlignmentOptions.Center;

            // Subtítulo
            GameObject subtitle = CreateOrFind(innerPanel.transform, "Subtitle");
            SetupRectTransform(subtitle,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -120), new Vector2(-40, 40));

            TextMeshProUGUI subtitleTmp = subtitle.GetComponent<TextMeshProUGUI>();
            if (subtitleTmp == null) subtitleTmp = subtitle.AddComponent<TextMeshProUGUI>();
            subtitleTmp.text = "Selecciona 3-5 juegos para el sprint";
            subtitleTmp.fontSize = 24;
            subtitleTmp.color = Color.white;
            subtitleTmp.alignment = TextAlignmentOptions.Center;

            // Contenedor de toggles
            GameObject togglesContainer = CreateOrFind(innerPanel.transform, "TogglesContainer");
            RectTransform togglesRect = SetupRectTransform(togglesContainer,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 80), new Vector2(600, 400)); // Subido a 80

            VerticalLayoutGroup vLayout = togglesContainer.GetComponent<VerticalLayoutGroup>();
            if (vLayout == null) vLayout = togglesContainer.AddComponent<VerticalLayoutGroup>();
            vLayout.spacing = 20;
            vLayout.childAlignment = TextAnchor.UpperCenter;
            vLayout.childForceExpandWidth = true;
            vLayout.childForceExpandHeight = false;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = false;

            // Crear toggles para cada juego
            string[] gameNames = { "Digit Rush", "Memory Pairs", "Quick Math", "Flash Tap", "Odd One Out" };
            string[] toggleNames = { "Toggle_DigitRush", "Toggle_MemoryPairs", "Toggle_QuickMath", "Toggle_FlashTap", "Toggle_OddOneOut" };

            for (int i = 0; i < gameNames.Length; i++)
            {
                CreateSprintToggle(togglesContainer.transform, toggleNames[i], gameNames[i]);
            }

            // Texto de seleccion - entre los toggles y los botones
            GameObject selectedText = CreateOrFind(innerPanel.transform, "SelectedCountText");
            SetupRectTransform(selectedText,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 120), new Vector2(400, 35)); // Valor ajustado por el usuario

            TextMeshProUGUI selectedTmp = selectedText.GetComponent<TextMeshProUGUI>();
            if (selectedTmp == null) selectedTmp = selectedText.AddComponent<TextMeshProUGUI>();
            selectedTmp.text = "0/5 juegos seleccionados";
            selectedTmp.fontSize = 24;
            selectedTmp.color = Color.white;
            selectedTmp.alignment = TextAlignmentOptions.Center;

            // Botones de acción - dentro del panel con espacio
            GameObject buttonsContainer = CreateOrFind(innerPanel.transform, "ButtonsContainer");
            SetupRectTransform(buttonsContainer,
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 50), new Vector2(-60, 90)); // Subido y centrado

            HorizontalLayoutGroup hLayout = buttonsContainer.GetComponent<HorizontalLayoutGroup>();
            if (hLayout == null) hLayout = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 30;
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.childForceExpandWidth = false;
            hLayout.childForceExpandHeight = false;
            hLayout.childControlWidth = false;
            hLayout.childControlHeight = false;

            // Botón Cancelar
            CreateActionButton(buttonsContainer.transform, "CancelSprintButton", "Cancelar", false);

            // Botón Iniciar
            CreateActionButton(buttonsContainer.transform, "StartSprintButton", "Iniciar Sprint", true);

            // Desactivar panel por defecto
            panel.SetActive(false);
        }

        private static void CreateRulesPanel(Transform canvasTransform)
        {
            // ========== PANEL DE REGLAS ==========
            GameObject panel = CreateOrFind(canvasTransform, "RulesPanel");
            RectTransform panelRect = SetupRectTransform(panel,
                Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero);

            // Fondo oscuro semi-transparente
            Image panelBg = panel.GetComponent<Image>();
            if (panelBg == null) panelBg = panel.AddComponent<Image>();
            panelBg.color = new Color(0, 0, 0, 0.92f);

            // Panel interior
            GameObject innerPanel = CreateOrFind(panel.transform, "InnerPanel");
            RectTransform innerPanelRect = SetupRectTransform(innerPanel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(750, 850));

            Image innerPanelBg = innerPanel.GetComponent<Image>();
            if (innerPanelBg == null) innerPanelBg = innerPanel.AddComponent<Image>();
            innerPanelBg.color = CARD_BG;

            // Borde neón cyan
            Outline panelOutline = innerPanel.GetComponent<Outline>();
            if (panelOutline == null) panelOutline = innerPanel.AddComponent<Outline>();
            panelOutline.effectColor = CYAN_NEON;
            panelOutline.effectDistance = new Vector2(3, 3);

            // ========== TÍTULO DEL JUEGO ==========
            GameObject gameTitle = CreateOrFind(innerPanel.transform, "GameTitle");
            SetupRectTransform(gameTitle,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -40), new Vector2(-40, 70));

            TextMeshProUGUI titleTmp = gameTitle.GetComponent<TextMeshProUGUI>();
            if (titleTmp == null) titleTmp = gameTitle.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "DIGIT RUSH"; // Se cambia dinámicamente
            titleTmp.fontSize = 38;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = CYAN_NEON;
            titleTmp.alignment = TextAlignmentOptions.Center;

            // ========== SUBTÍTULO "Reglas del juego" ==========
            GameObject subtitle = CreateOrFind(innerPanel.transform, "Subtitle");
            SetupRectTransform(subtitle,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -100), new Vector2(-40, 40));

            TextMeshProUGUI subtitleTmp = subtitle.GetComponent<TextMeshProUGUI>();
            if (subtitleTmp == null) subtitleTmp = subtitle.AddComponent<TextMeshProUGUI>();
            subtitleTmp.text = "Reglas del juego";
            subtitleTmp.fontSize = 24;
            subtitleTmp.color = Color.white;
            subtitleTmp.alignment = TextAlignmentOptions.Center;

            // ========== CONTENEDOR DE REGLAS ==========
            // Valores ajustados: Left 40, Top 120, Right 40, Bottom 200
            GameObject rulesContainer = CreateOrFind(innerPanel.transform, "RulesContainer");
            SetupRectTransform(rulesContainer,
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(40, 200), new Vector2(-40, -120));

            // Texto de reglas (se cambia dinámicamente)
            GameObject rulesText = CreateOrFind(rulesContainer.transform, "RulesText");
            SetupRectTransform(rulesText,
                Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero);

            TextMeshProUGUI rulesTmp = rulesText.GetComponent<TextMeshProUGUI>();
            if (rulesTmp == null) rulesTmp = rulesText.AddComponent<TextMeshProUGUI>();
            rulesTmp.text = "• Regla 1\n• Regla 2\n• Regla 3"; // Placeholder
            rulesTmp.fontSize = 26; // Más grande para mejor legibilidad
            rulesTmp.fontStyle = FontStyles.Bold; // Negrita
            rulesTmp.color = new Color(0.9f, 0.9f, 0.9f, 1f); // Un poco más brillante
            rulesTmp.alignment = TextAlignmentOptions.TopLeft;
            rulesTmp.lineSpacing = 15; // Más espaciado entre líneas

            // ========== CHECKBOX "No volver a mostrar" ==========
            GameObject checkboxContainer = CreateOrFind(innerPanel.transform, "CheckboxContainer");
            SetupRectTransform(checkboxContainer,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 150), new Vector2(500, 50));

            // Toggle
            GameObject toggleObj = CreateOrFind(checkboxContainer.transform, "DontShowToggle");
            SetupRectTransform(toggleObj,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(30, 0), new Vector2(40, 40));

            Image toggleBg = toggleObj.GetComponent<Image>();
            if (toggleBg == null) toggleBg = toggleObj.AddComponent<Image>();
            toggleBg.color = new Color(0.1f, 0.15f, 0.2f, 1f);

            Outline toggleOutline = toggleObj.GetComponent<Outline>();
            if (toggleOutline == null) toggleOutline = toggleObj.AddComponent<Outline>();
            toggleOutline.effectColor = CYAN_DARK;
            toggleOutline.effectDistance = new Vector2(1.5f, 1.5f);

            Toggle toggle = toggleObj.GetComponent<Toggle>();
            if (toggle == null) toggle = toggleObj.AddComponent<Toggle>();

            // Checkmark
            GameObject checkmark = CreateOrFind(toggleObj.transform, "Checkmark");
            SetupRectTransform(checkmark, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-8, -8));
            Image checkImg = checkmark.GetComponent<Image>();
            if (checkImg == null) checkImg = checkmark.AddComponent<Image>();
            checkImg.color = CYAN_NEON;

            toggle.targetGraphic = toggleBg;
            toggle.graphic = checkImg;
            toggle.isOn = false;

            // Label del checkbox
            GameObject toggleLabel = CreateOrFind(checkboxContainer.transform, "ToggleLabel");
            SetupRectTransform(toggleLabel,
                new Vector2(0, 0.5f), new Vector2(1, 0.5f),
                new Vector2(80, 0), new Vector2(-90, 40));

            TextMeshProUGUI labelTmp = toggleLabel.GetComponent<TextMeshProUGUI>();
            if (labelTmp == null) labelTmp = toggleLabel.AddComponent<TextMeshProUGUI>();
            labelTmp.text = "No volver a mostrar estas reglas";
            labelTmp.fontSize = 20;
            labelTmp.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            labelTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // ========== BOTONES ==========
            GameObject buttonsContainer = CreateOrFind(innerPanel.transform, "ButtonsContainer");
            SetupRectTransform(buttonsContainer,
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 50), new Vector2(-60, 80));

            HorizontalLayoutGroup hLayout = buttonsContainer.GetComponent<HorizontalLayoutGroup>();
            if (hLayout == null) hLayout = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 40;
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.childForceExpandWidth = false;
            hLayout.childForceExpandHeight = false;
            hLayout.childControlWidth = false;
            hLayout.childControlHeight = false;

            // Botón Cancelar
            CreateRulesButton(buttonsContainer.transform, "CancelButton", "Cancelar", false);

            // Botón Jugar
            CreateRulesButton(buttonsContainer.transform, "PlayButton", "¡Jugar!", true);

            // Desactivar panel por defecto
            panel.SetActive(false);
        }

        private static void CreateRulesButton(Transform parent, string buttonName, string text, bool isPrimary)
        {
            GameObject btnObj = CreateOrFind(parent, buttonName);

            LayoutElement layout = btnObj.GetComponent<LayoutElement>();
            if (layout == null) layout = btnObj.AddComponent<LayoutElement>();
            layout.preferredWidth = 200;
            layout.preferredHeight = 80; // Consistente con panel Cognitive Sprint
            layout.minWidth = 200;

            Image btnBg = btnObj.GetComponent<Image>();
            if (btnBg == null) btnBg = btnObj.AddComponent<Image>();
            btnBg.color = isPrimary ? CYAN_NEON : new Color(0.25f, 0.25f, 0.25f, 1f);

            Button btn = btnObj.GetComponent<Button>();
            if (btn == null) btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnBg;

            Outline btnOutline = btnObj.GetComponent<Outline>();
            if (btnOutline == null) btnOutline = btnObj.AddComponent<Outline>();
            btnOutline.effectColor = isPrimary ? new Color(0f, 1f, 1f, 0.5f) : CYAN_DARK;
            btnOutline.effectDistance = new Vector2(2, 2);

            GameObject textObj = CreateOrFind(btnObj.transform, "Text");
            SetupRectTransform(textObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            TextMeshProUGUI textTmp = textObj.GetComponent<TextMeshProUGUI>();
            if (textTmp == null) textTmp = textObj.AddComponent<TextMeshProUGUI>();
            textTmp.text = text;
            textTmp.fontSize = 24;
            textTmp.fontStyle = FontStyles.Bold;
            textTmp.color = isPrimary ? DARK_BG : Color.white;
            textTmp.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateSprintToggle(Transform parent, string toggleName, string labelText)
        {
            GameObject toggleObj = CreateOrFind(parent, toggleName);

            // Layout element para el tamaño
            LayoutElement layout = toggleObj.GetComponent<LayoutElement>();
            if (layout == null) layout = toggleObj.AddComponent<LayoutElement>();
            layout.preferredHeight = 60;
            layout.preferredWidth = 500;

            // Background del toggle
            Image toggleBg = toggleObj.GetComponent<Image>();
            if (toggleBg == null) toggleBg = toggleObj.AddComponent<Image>();
            toggleBg.color = new Color(0.08f, 0.12f, 0.18f, 1f);

            // Borde neón para el toggle
            Outline toggleOutline = toggleObj.GetComponent<Outline>();
            if (toggleOutline == null) toggleOutline = toggleObj.AddComponent<Outline>();
            toggleOutline.effectColor = CYAN_DARK;
            toggleOutline.effectDistance = new Vector2(1.5f, 1.5f);

            // Toggle component
            Toggle toggle = toggleObj.GetComponent<Toggle>();
            if (toggle == null) toggle = toggleObj.AddComponent<Toggle>();

            // Checkmark container
            GameObject checkBg = CreateOrFind(toggleObj.transform, "Background");
            SetupRectTransform(checkBg,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(40, 0), new Vector2(40, 40));

            Image checkBgImg = checkBg.GetComponent<Image>();
            if (checkBgImg == null) checkBgImg = checkBg.AddComponent<Image>();
            checkBgImg.color = DARK_BG;

            // Checkmark
            GameObject checkmark = CreateOrFind(checkBg.transform, "Checkmark");
            SetupRectTransform(checkmark,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(30, 30));

            Image checkImg = checkmark.GetComponent<Image>();
            if (checkImg == null) checkImg = checkmark.AddComponent<Image>();
            checkImg.color = CYAN_NEON;

            // Label
            GameObject label = CreateOrFind(toggleObj.transform, "Label");
            SetupRectTransform(label,
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(40, 0), new Vector2(-80, 0));

            TextMeshProUGUI labelTmp = label.GetComponent<TextMeshProUGUI>();
            if (labelTmp == null) labelTmp = label.AddComponent<TextMeshProUGUI>();
            labelTmp.text = labelText;
            labelTmp.fontSize = 28;
            labelTmp.color = Color.white;
            labelTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // Configurar toggle
            toggle.targetGraphic = checkBgImg;
            toggle.graphic = checkImg;
            toggle.isOn = false;
        }

        private static void CreateActionButton(Transform parent, string buttonName, string text, bool isPrimary)
        {
            GameObject btnObj = CreateOrFind(parent, buttonName);

            LayoutElement layout = btnObj.GetComponent<LayoutElement>();
            if (layout == null) layout = btnObj.AddComponent<LayoutElement>();
            layout.preferredWidth = 200;
            layout.preferredHeight = 80;
            layout.minWidth = 200;

            Image btnBg = btnObj.GetComponent<Image>();
            if (btnBg == null) btnBg = btnObj.AddComponent<Image>();
            btnBg.color = isPrimary ? CYAN_NEON : new Color(0.25f, 0.25f, 0.25f, 1f);

            Button btn = btnObj.GetComponent<Button>();
            if (btn == null) btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnBg;

            // Borde neón para los botones
            Outline btnOutline = btnObj.GetComponent<Outline>();
            if (btnOutline == null) btnOutline = btnObj.AddComponent<Outline>();
            btnOutline.effectColor = isPrimary ? new Color(0f, 1f, 1f, 0.5f) : CYAN_DARK;
            btnOutline.effectDistance = new Vector2(2, 2);

            GameObject textObj = CreateOrFind(btnObj.transform, "Text");
            SetupRectTransform(textObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            TextMeshProUGUI textTmp = textObj.GetComponent<TextMeshProUGUI>();
            if (textTmp == null) textTmp = textObj.AddComponent<TextMeshProUGUI>();
            textTmp.text = text;
            textTmp.fontSize = 24;
            textTmp.fontStyle = FontStyles.Bold;
            textTmp.color = isPrimary ? DARK_BG : Color.white;
            textTmp.alignment = TextAlignmentOptions.Center;
            textTmp.enableAutoSizing = false;
            textTmp.overflowMode = TextOverflowModes.Overflow;
        }

        private static void CreateGameCardsOnly()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No se encontró Canvas");
                return;
            }

            Transform gamesPanel = canvas.transform.Find("GamesPanel");
            if (gamesPanel == null)
            {
                Debug.LogError("No se encontró GamesPanel");
                return;
            }

            // Limpiar cards existentes
            while (gamesPanel.childCount > 0)
            {
                DestroyImmediate(gamesPanel.GetChild(0).gameObject);
            }

            // Crear nuevas cards
            CreateGameCard(gamesPanel, "DigitRushButton", "Digit Rush", "IconPlaceholder_DigitRush", false);
            CreateGameCard(gamesPanel, "MemoryPairsButton", "Memory Pairs", "IconPlaceholder_MemoryPairs", false);
            CreateGameCard(gamesPanel, "QuickMathButton", "Quick Math", "IconPlaceholder_QuickMath", false);
            CreateGameCard(gamesPanel, "FlashTapButton", "Flash Tap", "IconPlaceholder_FlashTap", false);
            CreateGameCard(gamesPanel, "OddOneOutButton", "Odd One Out", "IconPlaceholder_OddOneOut", false);
            CreateGameCard(gamesPanel, "CognitiveSprintButton", "Cognitive Sprint", "IconPlaceholder_Sprint", true);

            Debug.Log("Game Cards creadas exitosamente!");
            EditorUtility.SetDirty(canvas.gameObject);
        }

        private static void SetupBackButton(GameObject obj)
        {
            Button btn = obj.GetComponent<Button>();
            if (btn == null) btn = obj.AddComponent<Button>();

            Image img = obj.GetComponent<Image>();
            if (img == null) img = obj.AddComponent<Image>();
            img.color = Color.clear;

            // Texto de flecha
            GameObject arrowText = CreateOrFind(obj.transform, "ArrowText");
            SetupRectTransform(arrowText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            TextMeshProUGUI tmp = arrowText.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = arrowText.AddComponent<TextMeshProUGUI>();
            tmp.text = "<";
            tmp.fontSize = 48;
            tmp.color = CYAN_NEON;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        private static void SetupTitleText(GameObject obj, string text)
        {
            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 38; // Tamaño con presencia pero no excesivo
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = CYAN_NEON;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        // ========== UTILIDADES ==========

        private static GameObject CreateOrFind(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null) return existing.gameObject;

            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            // Agregar RectTransform por defecto
            if (obj.GetComponent<RectTransform>() == null)
            {
                obj.AddComponent<RectTransform>();
            }

            return obj;
        }

        private static RectTransform SetupRectTransform(GameObject obj, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            RectTransform rect = obj.GetComponent<RectTransform>();
            if (rect == null) rect = obj.AddComponent<RectTransform>();

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = sizeDelta;

            return rect;
        }
    }
}
