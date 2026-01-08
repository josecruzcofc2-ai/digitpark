using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;

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
                new Vector2(0, -80), new Vector2(0, 160));

            // Back Button
            GameObject backButton = CreateOrFind(header.transform, "BackButton");
            SetupRectTransform(backButton,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(60, 0), new Vector2(80, 80));
            SetupBackButton(backButton);

            // Title
            GameObject title = CreateOrFind(header.transform, "TitleText");
            SetupRectTransform(title,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 0), new Vector2(400, 60));
            SetupTitleText(title, "Selecciona un Juego");

            // ========== GAMES GRID ==========
            GameObject gamesPanel = CreateOrFind(canvasTransform, "GamesPanel");
            RectTransform gamesPanelRect = SetupRectTransform(gamesPanel,
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(0, -40), new Vector2(-80, -200));

            // Agregar Grid Layout
            GridLayoutGroup grid = gamesPanel.GetComponent<GridLayoutGroup>();
            if (grid == null) grid = gamesPanel.AddComponent<GridLayoutGroup>();

            grid.cellSize = new Vector2(400, 280);
            grid.spacing = new Vector2(40, 40);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperCenter;
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
        }

        private static void CreateGameCard(Transform parent, string buttonName, string gameName, string iconName, bool isGold)
        {
            // Card Container (Button)
            GameObject card = CreateOrFind(parent, buttonName);

            // Configurar Button
            Button btn = card.GetComponent<Button>();
            if (btn == null) btn = card.AddComponent<Button>();

            // Imagen de fondo del card
            Image cardBg = card.GetComponent<Image>();
            if (cardBg == null) cardBg = card.AddComponent<Image>();
            cardBg.color = CARD_BG;

            // Configurar colores del botón
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.selectedColor = Color.white;
            btn.colors = colors;

            // ========== BORDE NEÓN ==========
            GameObject border = CreateOrFind(card.transform, "Border");
            RectTransform borderRect = SetupRectTransform(border,
                new Vector2(0, 0), new Vector2(1, 1),
                Vector2.zero, Vector2.zero);

            Image borderImg = border.GetComponent<Image>();
            if (borderImg == null) borderImg = border.AddComponent<Image>();
            borderImg.color = isGold ? GOLD : CYAN_NEON;
            borderImg.raycastTarget = false;

            // Crear Outline effect usando un segundo borde interior
            GameObject innerBg = CreateOrFind(card.transform, "InnerBackground");
            RectTransform innerRect = SetupRectTransform(innerBg,
                new Vector2(0, 0), new Vector2(1, 1),
                Vector2.zero, new Vector2(-6, -6));

            Image innerImg = innerBg.GetComponent<Image>();
            if (innerImg == null) innerImg = innerBg.AddComponent<Image>();
            innerImg.color = CARD_BG;
            innerImg.raycastTarget = false;

            // ========== ICONO PLACEHOLDER ==========
            GameObject iconContainer = CreateOrFind(card.transform, "IconContainer");
            RectTransform iconContainerRect = SetupRectTransform(iconContainer,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -100), new Vector2(120, 120));

            Image iconBg = iconContainer.GetComponent<Image>();
            if (iconBg == null) iconBg = iconContainer.AddComponent<Image>();
            iconBg.color = new Color(0.1f, 0.15f, 0.2f, 1f);
            iconBg.raycastTarget = false;

            // Placeholder text para icono
            GameObject iconText = CreateOrFind(iconContainer.transform, "IconText");
            RectTransform iconTextRect = SetupRectTransform(iconText,
                new Vector2(0, 0), new Vector2(1, 1),
                Vector2.zero, Vector2.zero);

            TextMeshProUGUI iconTmp = iconText.GetComponent<TextMeshProUGUI>();
            if (iconTmp == null) iconTmp = iconText.AddComponent<TextMeshProUGUI>();
            iconTmp.text = "ICON";
            iconTmp.fontSize = 24;
            iconTmp.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            iconTmp.alignment = TextAlignmentOptions.Center;
            iconTmp.raycastTarget = false;

            // ========== NOMBRE DEL JUEGO ==========
            GameObject nameText = CreateOrFind(card.transform, "GameNameText");
            RectTransform nameRect = SetupRectTransform(nameText,
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 60), new Vector2(-20, 80));

            TextMeshProUGUI nameTmp = nameText.GetComponent<TextMeshProUGUI>();
            if (nameTmp == null) nameTmp = nameText.AddComponent<TextMeshProUGUI>();
            nameTmp.text = gameName;
            nameTmp.fontSize = 36;
            nameTmp.fontStyle = FontStyles.Bold;
            nameTmp.color = isGold ? GOLD : CYAN_NEON;
            nameTmp.alignment = TextAlignmentOptions.Center;
            nameTmp.raycastTarget = false;

            // ========== GLOW EFFECT (para el borde) ==========
            if (isGold)
            {
                // Agregar indicador especial para Cognitive Sprint
                GameObject specialBadge = CreateOrFind(card.transform, "SpecialBadge");
                RectTransform badgeRect = SetupRectTransform(specialBadge,
                    new Vector2(1, 1), new Vector2(1, 1),
                    new Vector2(-20, -20), new Vector2(80, 30));

                Image badgeImg = specialBadge.GetComponent<Image>();
                if (badgeImg == null) badgeImg = specialBadge.AddComponent<Image>();
                badgeImg.color = GOLD;
                badgeImg.raycastTarget = false;

                GameObject badgeText = CreateOrFind(specialBadge.transform, "BadgeText");
                SetupRectTransform(badgeText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

                TextMeshProUGUI badgeTmp = badgeText.GetComponent<TextMeshProUGUI>();
                if (badgeTmp == null) badgeTmp = badgeText.AddComponent<TextMeshProUGUI>();
                badgeTmp.text = "PRO";
                badgeTmp.fontSize = 18;
                badgeTmp.fontStyle = FontStyles.Bold;
                badgeTmp.color = DARK_BG;
                badgeTmp.alignment = TextAlignmentOptions.Center;
                badgeTmp.raycastTarget = false;
            }

            // Ordenar los children correctamente
            border.transform.SetAsFirstSibling();
            innerBg.transform.SetSiblingIndex(1);

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
                new Vector2(0, 50), new Vector2(600, 400));

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

            // Texto de seleccion
            GameObject selectedText = CreateOrFind(innerPanel.transform, "SelectedCountText");
            SetupRectTransform(selectedText,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 180), new Vector2(400, 40));

            TextMeshProUGUI selectedTmp = selectedText.GetComponent<TextMeshProUGUI>();
            if (selectedTmp == null) selectedTmp = selectedText.AddComponent<TextMeshProUGUI>();
            selectedTmp.text = "0/5 juegos seleccionados";
            selectedTmp.fontSize = 24;
            selectedTmp.color = Color.white;
            selectedTmp.alignment = TextAlignmentOptions.Center;

            // Botones de acción
            GameObject buttonsContainer = CreateOrFind(innerPanel.transform, "ButtonsContainer");
            SetupRectTransform(buttonsContainer,
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 80), new Vector2(-80, 80));

            HorizontalLayoutGroup hLayout = buttonsContainer.GetComponent<HorizontalLayoutGroup>();
            if (hLayout == null) hLayout = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 40;
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.childForceExpandWidth = false;
            hLayout.childForceExpandHeight = true;

            // Botón Cancelar
            CreateActionButton(buttonsContainer.transform, "CancelSprintButton", "Cancelar", false);

            // Botón Iniciar
            CreateActionButton(buttonsContainer.transform, "StartSprintButton", "Iniciar Sprint", true);

            // Desactivar panel por defecto
            panel.SetActive(false);
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
            toggleBg.color = new Color(0.1f, 0.15f, 0.2f, 1f);

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
            layout.preferredWidth = 250;
            layout.preferredHeight = 60;

            Image btnBg = btnObj.GetComponent<Image>();
            if (btnBg == null) btnBg = btnObj.AddComponent<Image>();
            btnBg.color = isPrimary ? CYAN_NEON : new Color(0.3f, 0.3f, 0.3f, 1f);

            Button btn = btnObj.GetComponent<Button>();
            if (btn == null) btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnBg;

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
            tmp.fontSize = 42;
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
