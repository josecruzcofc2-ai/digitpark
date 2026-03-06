using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.Editor.AutoAssigners;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para reconstruir la UI de SearchPlayers con diseño profesional neón
    /// </summary>
    public class SearchPlayersUIBuilder : EditorWindow
    {
        // Colores del tema neón
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.4f, 1f);
        private static readonly Color DARK_BG = new Color(0.02f, 0.04f, 0.08f, 1f);
        private static readonly Color PANEL_BG = new Color(0.05f, 0.1f, 0.15f, 0.95f);
        private static readonly Color INPUT_BG = new Color(0.08f, 0.12f, 0.18f, 1f);
        private static readonly Color PLACEHOLDER_COLOR = new Color(0.4f, 0.4f, 0.4f, 1f);

        private const string BACK_BUTTON_PREFAB = "Assets/_Project/Prefabs/Common/BackButton.prefab";

        [MenuItem("DigitPark/UI Builders/Social/SearchPlayers", false, 221)]
        public static void ShowWindow()
        {
            GetWindow<SearchPlayersUIBuilder>("SearchPlayers UI Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("SearchPlayers UI Builder", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Este script reconstruirá la UI de SearchPlayers.\n" +
                "Asegúrate de tener la escena SearchPlayers abierta.",
                MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("Reconstruir SearchPlayers UI", GUILayout.Height(40)))
            {
                RebuildSearchPlayersUI();
            }

            GUILayout.Space(15);
            GUI.backgroundColor = new Color(0.5f, 0.8f, 1f);
            if (GUILayout.Button("Auto-Asignar Referencias", GUILayout.Height(30)))
            {
                SearchPlayersReferenceAssigner.RunAutoAssign();
            }
            GUI.backgroundColor = Color.white;
        }

        private static void RebuildSearchPlayersUI()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("No se encontró Canvas en la escena");
                return;
            }

            Transform canvasTransform = canvas.transform;

            // Limpiar elementos viejos
            CleanOldElements(canvasTransform);

            // Crear nueva estructura
            CreateSearchPlayersLayout(canvasTransform);

            Debug.Log("SearchPlayers UI reconstruida exitosamente!");
            EditorUtility.SetDirty(canvas.gameObject);

            // Auto-asignar referencias al Manager
            SearchPlayersReferenceAssigner.RunAutoAssign();
        }

        private static void CleanOldElements(Transform canvasTransform)
        {
            string[] oldElements = new string[]
            {
                "HeaderPanel", "TitleText",
                "SearchInputField", "SearchButton", "ClearButton",
                "ResultsScrollView", "ResultsContainer",
                "NoResultsText", "LoadingIndicator",
                // Nuevos
                "Header", "SearchBar", "ResultsPanel", "EmptyState",
                // Limpiar elementos de texto viejos que ahora son Images
                "IconText", "ClearButtonText", "Icon",
                // Limpiar elementos del SearchBar viejo
                "SearchIcon", "InputContainer", "ClearIcon",
                // Player Cards (limpiar cualquier card de prueba)
                "PlayerCard_Template", "PlayerCard_1", "PlayerCard_2", "PlayerCard_3", "PlayerCard"
            };

            foreach (string elementName in oldElements)
            {
                Transform element = canvasTransform.Find(elementName);
                if (element != null)
                {
                    Debug.Log($"Limpiando: {elementName}");
                    DestroyImmediate(element.gameObject);
                }
            }
        }

        private static void CreateSearchPlayersLayout(Transform canvasTransform)
        {
            // ========== BACKGROUND ==========
            GameObject bg = CreateOrFind(canvasTransform, "Background");
            bg.transform.SetAsFirstSibling();
            RectTransform bgRT = bg.GetComponent<RectTransform>();
            if (bgRT == null) bgRT = bg.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            Image bgImg = bg.GetComponent<Image>();
            if (bgImg == null) bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.02f, 0.03f, 0.08f, 1f);
            bgImg.raycastTarget = false;

            // ========== HEADER ==========
            GameObject header = CreateOrFind(canvasTransform, "Header");
            SetupRectTransform(header,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -70), new Vector2(0, 140));

            Image headerBg = header.GetComponent<Image>();
            if (headerBg == null) headerBg = header.AddComponent<Image>();
            headerBg.color = new Color(0.04f, 0.06f, 0.1f, 0.98f);

            // Back Button - Neon Cyan prefab
            GameObject backBtnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BACK_BUTTON_PREFAB);
            GameObject backBtn;
            if (backBtnPrefab != null)
            {
                Transform oldBtn = header.transform.Find("BackButton");
                if (oldBtn != null) DestroyImmediate(oldBtn.gameObject);
                backBtn = (GameObject)PrefabUtility.InstantiatePrefab(backBtnPrefab, header.transform);
                backBtn.name = "BackButton";
            }
            else
            {
                backBtn = CreateOrFind(header.transform, "BackButton");
                Image fallbackBg = backBtn.GetComponent<Image>();
                if (fallbackBg == null) fallbackBg = backBtn.AddComponent<Image>();
                fallbackBg.color = new Color(0, 0, 0, 0);
                if (backBtn.GetComponent<Button>() == null) backBtn.AddComponent<Button>();
                Debug.LogWarning("[SearchPlayersUI] BackButton prefab not found, using fallback");
            }
            RectTransform backRT = backBtn.GetComponent<RectTransform>();
            if (backRT == null) backRT = backBtn.AddComponent<RectTransform>();
            backRT.anchorMin = new Vector2(0, 0.5f);
            backRT.anchorMax = new Vector2(0, 0.5f);
            backRT.pivot = new Vector2(0, 0.5f);
            backRT.anchoredPosition = new Vector2(20, 0);
            backRT.sizeDelta = new Vector2(50, 50);

            GameObject title = CreateOrFind(header.transform, "TitleText");
            var titleRt = SetupRectTransform(title,
                new Vector2(0.07f, 0f), new Vector2(0.53f, 1f),
                Vector2.zero, Vector2.zero);
            titleRt.pivot = new Vector2(0.5f, 0.5f);
            titleRt.sizeDelta = Vector2.zero;
            SetupText(title, "SEARCH PLAYERS", (int)FontSizes.H4, CYAN_NEON, FontStyles.Bold);
            var titleTmp = title.GetComponent<TextMeshProUGUI>();
            if (titleTmp != null) titleTmp.raycastTarget = false;
            if (titleTmp != null)
            {
                titleTmp.alignment = TextAlignmentOptions.MidlineLeft;
                titleTmp.enableAutoSizing = true;
                titleTmp.fontSizeMin = FontSizes.AutoMinTitle;
                titleTmp.fontSizeMax = FontSizes.H4;
                titleTmp.overflowMode = TextOverflowModes.Ellipsis;
            }

            // ========== SEARCH BAR INTEGRADO (Diseño Moderno) ==========
            // Un único campo de búsqueda con lupa y Clear integrados
            GameObject searchBar = CreateOrFind(canvasTransform, "SearchBar");
            SetupRectTransform(searchBar,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -175), new Vector2(-80, 128));

            // Fondo del SearchBar con bordes redondeados simulados
            Image searchBarBg = searchBar.GetComponent<Image>();
            if (searchBarBg == null) searchBarBg = searchBar.AddComponent<Image>();
            searchBarBg.color = INPUT_BG;

            // Borde neón sutil
            Outline searchBarOutline = searchBar.GetComponent<Outline>();
            if (searchBarOutline == null) searchBarOutline = searchBar.AddComponent<Outline>();
            searchBarOutline.effectColor = CYAN_DARK;
            searchBarOutline.effectDistance = new Vector2(1.5f, 1.5f);
            Shadow searchBarShadow = searchBar.GetComponent<Shadow>();
            if (searchBarShadow == null || searchBarShadow is Outline) searchBarShadow = searchBar.AddComponent<Shadow>();
            searchBarShadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            searchBarShadow.effectDistance = new Vector2(3, -4);

            // Icono de Lupa (integrado a la izquierda)
            GameObject searchIcon = CreateOrFind(searchBar.transform, "SearchIcon");
            RectTransform searchIconRect = SetupRectTransform(searchIcon,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(56, 0), new Vector2(64, 64));
            Image searchIconImg = searchIcon.GetComponent<Image>();
            if (searchIconImg == null) searchIconImg = searchIcon.AddComponent<Image>();
            searchIconImg.color = PLACEHOLDER_COLOR;
            searchIconImg.preserveAspect = true;
            searchIconImg.raycastTarget = false;
            // Cargar icono de lupa neon
            Sprite searchSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/Navigation/SearchIcon.png");
            if (searchSprite != null)
            {
                searchIconImg.sprite = searchSprite;
                Debug.Log("[SearchPlayersUI] SearchIcon asignado");
            }

            // Input Field (ocupa el espacio central)
            GameObject inputField = CreateOrFind(searchBar.transform, "SearchInputField");
            SetupRectTransform(inputField,
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(60, 0), new Vector2(-200, 0)); // Espacio para lupa y botón Clear

            TMP_InputField tmpInput = inputField.GetComponent<TMP_InputField>();
            if (tmpInput == null) tmpInput = inputField.AddComponent<TMP_InputField>();

            // Text Area
            GameObject textArea = CreateOrFind(inputField.transform, "Text Area");
            SetupRectTransform(textArea, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Placeholder
            GameObject placeholder = CreateOrFind(textArea.transform, "Placeholder");
            SetupRectTransform(placeholder, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI placeholderTmp = placeholder.GetComponent<TextMeshProUGUI>();
            if (placeholderTmp == null) placeholderTmp = placeholder.AddComponent<TextMeshProUGUI>();
            placeholderTmp.text = "Search by username...";
            placeholderTmp.fontSize = FontSizes.BodyLarge;
            placeholderTmp.color = PLACEHOLDER_COLOR;
            placeholderTmp.fontStyle = FontStyles.Bold;
            placeholderTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // Text
            GameObject inputText = CreateOrFind(textArea.transform, "Text");
            SetupRectTransform(inputText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI inputTmp = inputText.GetComponent<TextMeshProUGUI>();
            if (inputTmp == null) inputTmp = inputText.AddComponent<TextMeshProUGUI>();
            inputTmp.text = "";
            inputTmp.fontSize = FontSizes.BodyLarge;
            inputTmp.color = Color.white;
            inputTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // Configure input field
            tmpInput.textViewport = textArea.GetComponent<RectTransform>();
            tmpInput.textComponent = inputTmp;
            tmpInput.placeholder = placeholderTmp;
            tmpInput.fontAsset = inputTmp.font;
            tmpInput.pointSize = FontSizes.BodyLarge;

            // Clear Button - Botón estilo neón con texto "Clear"
            GameObject clearButton = CreateOrFind(searchBar.transform, "ClearButton");
            RectTransform clearBtnRect = SetupRectTransform(clearButton,
                new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-90, 0), new Vector2(160, 80));

            Image clearBtnBg = clearButton.GetComponent<Image>();
            if (clearBtnBg == null) clearBtnBg = clearButton.AddComponent<Image>();
            clearBtnBg.color = CYAN_NEON;

            Button clearBtn = clearButton.GetComponent<Button>();
            if (clearBtn == null) clearBtn = clearButton.AddComponent<Button>();
            clearBtn.targetGraphic = clearBtnBg;

            // Configurar colores hover del botón clear
            ColorBlock clearColors = clearBtn.colors;
            clearColors.normalColor = Color.white;
            clearColors.highlightedColor = new Color(0.7f, 1f, 1f, 1f);
            clearColors.pressedColor = new Color(0.5f, 0.8f, 0.8f, 1f);
            clearBtn.colors = clearColors;

            // Glow sutil para el botón
            Outline clearGlow = clearButton.GetComponent<Outline>();
            if (clearGlow == null) clearGlow = clearButton.AddComponent<Outline>();
            clearGlow.effectColor = new Color(0f, 1f, 1f, 0.4f);
            clearGlow.effectDistance = new Vector2(2, 2);

            // Texto "Clear"
            GameObject clearText = CreateOrFind(clearButton.transform, "Text");
            SetupRectTransform(clearText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetupText(clearText, "Clear", (int)FontSizes.Body, DARK_BG, FontStyles.Bold);

            // SearchButton oculto (mantener referencia para el Manager pero no visible)
            // La búsqueda es en tiempo real, no necesita botón
            GameObject searchButton = CreateOrFind(searchBar.transform, "SearchButton");
            SetupRectTransform(searchButton, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            Button searchBtn = searchButton.GetComponent<Button>();
            if (searchBtn == null) searchBtn = searchButton.AddComponent<Button>();
            searchButton.SetActive(false); // Oculto - búsqueda en tiempo real

            // ========== RESULTS PANEL ==========
            // Ajustado para el SearchBar compacto (empieza después del SearchBar a y=-250)
            GameObject resultsPanel = CreateOrFind(canvasTransform, "ResultsPanel");
            SetupRectTransform(resultsPanel,
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(0, -80), new Vector2(-80, -350));

            Image resultsBg = resultsPanel.GetComponent<Image>();
            if (resultsBg == null) resultsBg = resultsPanel.AddComponent<Image>();
            resultsBg.color = PANEL_BG;

            Outline resultsOutline = resultsPanel.GetComponent<Outline>();
            if (resultsOutline == null) resultsOutline = resultsPanel.AddComponent<Outline>();
            resultsOutline.effectColor = CYAN_DARK;
            resultsOutline.effectDistance = new Vector2(1, 1);
            Shadow resultsShadow = resultsPanel.GetComponent<Shadow>();
            if (resultsShadow == null || resultsShadow is Outline) resultsShadow = resultsPanel.AddComponent<Shadow>();
            resultsShadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            resultsShadow.effectDistance = new Vector2(3, -4);

            // ========== SCROLL VIEW ==========
            GameObject scrollView = CreateOrFind(resultsPanel.transform, "ResultsScrollView");
            SetupRectTransform(scrollView, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-20, -20));

            ScrollRect scrollRect = scrollView.GetComponent<ScrollRect>();
            if (scrollRect == null) scrollRect = scrollView.AddComponent<ScrollRect>();

            // Viewport - usar RectMask2D en lugar de Mask
            GameObject viewport = CreateOrFind(scrollView.transform, "Viewport");
            SetupRectTransform(viewport, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Eliminar Mask viejo si existe
            Mask oldMask = viewport.GetComponent<Mask>();
            if (oldMask != null) DestroyImmediate(oldMask);

            // Image transparente necesario para raycast/drag detection
            Image vpImg = viewport.GetComponent<Image>();
            if (vpImg == null) vpImg = viewport.AddComponent<Image>();
            vpImg.color = Color.clear;
            vpImg.raycastTarget = true;

            // RectMask2D para clipping
            RectMask2D rectMask = viewport.GetComponent<RectMask2D>();
            if (rectMask == null) rectMask = viewport.AddComponent<RectMask2D>();

            // Content (Results Container) - Anclado desde arriba
            GameObject resultsContainer = CreateOrFind(viewport.transform, "ResultsContainer");
            RectTransform containerRect = SetupRectTransform(resultsContainer,
                new Vector2(0, 1), new Vector2(1, 1),
                Vector2.zero, new Vector2(0, 0));
            containerRect.pivot = new Vector2(0.5f, 1f); // Pivot en la parte superior

            VerticalLayoutGroup contentLayout = resultsContainer.GetComponent<VerticalLayoutGroup>();
            if (contentLayout == null) contentLayout = resultsContainer.AddComponent<VerticalLayoutGroup>();
            contentLayout.childAlignment = TextAnchor.UpperCenter;
            contentLayout.spacing = 15; // Espacio entre cards
            contentLayout.padding = new RectOffset(15, 15, 15, 15);
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childControlHeight = true; // Controlar altura
            contentLayout.childControlWidth = true;

            ContentSizeFitter contentFitter = resultsContainer.GetComponent<ContentSizeFitter>();
            if (contentFitter == null) contentFitter = resultsContainer.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Configure scroll rect
            scrollRect.content = resultsContainer.GetComponent<RectTransform>();
            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;
            scrollRect.scrollSensitivity = 50f;

            // Scrollbar
            GameObject scrollbar = CreateOrFind(scrollView.transform, "Scrollbar Vertical");
            SetupRectTransform(scrollbar,
                new Vector2(1, 0), new Vector2(1, 1),
                new Vector2(5, 0), new Vector2(10, 0));

            Image scrollbarBg = scrollbar.GetComponent<Image>();
            if (scrollbarBg == null) scrollbarBg = scrollbar.AddComponent<Image>();
            scrollbarBg.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);

            Scrollbar sb = scrollbar.GetComponent<Scrollbar>();
            if (sb == null) sb = scrollbar.AddComponent<Scrollbar>();
            sb.direction = Scrollbar.Direction.BottomToTop;

            // Scrollbar Handle
            GameObject slidingArea = CreateOrFind(scrollbar.transform, "Sliding Area");
            SetupRectTransform(slidingArea, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            GameObject handle = CreateOrFind(slidingArea.transform, "Handle");
            SetupRectTransform(handle, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Image handleImg = handle.GetComponent<Image>();
            if (handleImg == null) handleImg = handle.AddComponent<Image>();
            handleImg.color = CYAN_DARK;

            sb.handleRect = handle.GetComponent<RectTransform>();
            sb.targetGraphic = handleImg;
            scrollRect.verticalScrollbar = sb;

            // ========== EMPTY STATE ==========
            GameObject emptyState = CreateOrFind(resultsPanel.transform, "EmptyState");
            SetupRectTransform(emptyState,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 40), new Vector2(800, 700)); // Subido un poco para mejor centrado
            // Mostrar EmptyState por defecto (se oculta cuando hay resultados)
            emptyState.SetActive(true);

            // Empty Icon - Icono grande y prominente
            GameObject emptyIcon = CreateOrFind(emptyState.transform, "EmptyIcon");
            SetupRectTransform(emptyIcon,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 0), new Vector2(240, 240));
            Image emptyIconImg = emptyIcon.GetComponent<Image>();
            if (emptyIconImg == null) emptyIconImg = emptyIcon.AddComponent<Image>();
            emptyIconImg.color = CYAN_NEON; // Cyan brillante para que destaque
            emptyIconImg.preserveAspect = true;
            // Cargar icono de TabBar para empty state
            Sprite tabBarSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/Social/ProfileIcon.png");
            if (tabBarSprite != null)
            {
                emptyIconImg.sprite = tabBarSprite;
                Debug.Log("[SearchPlayersUI] TabBar icon asignado al EmptyIcon");
            }

            // Empty Title - Más grande y prominente
            GameObject emptyTitle = CreateOrFind(emptyState.transform, "Title");
            SetupRectTransform(emptyTitle,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -100), new Vector2(700, 100));
            SetupText(emptyTitle, "Search players", (int)FontSizes.H2, Color.white, FontStyles.Bold);

            // Empty Description
            GameObject emptyDesc = CreateOrFind(emptyState.transform, "Description");
            SetupRectTransform(emptyDesc,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -180), new Vector2(760, 160));
            SetupText(emptyDesc, "Find players to\nadd as friends or challenge", (int)FontSizes.BodyLarge, new Color(0.6f, 0.6f, 0.6f, 1f), FontStyles.Bold);

            // No Results Text (se mostrará cuando no haya resultados)
            GameObject noResultsText = CreateOrFind(resultsPanel.transform, "NoResultsText");
            SetupRectTransform(noResultsText,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(400, 60));
            SetupText(noResultsText, "No players found", (int)FontSizes.Body, PLACEHOLDER_COLOR, FontStyles.Bold);
            noResultsText.SetActive(false);

            // Loading Indicator
            GameObject loadingIndicator = CreateOrFind(resultsPanel.transform, "LoadingIndicator");
            SetupRectTransform(loadingIndicator,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(200, 100));

            GameObject loadingText = CreateOrFind(loadingIndicator.transform, "Text");
            SetupRectTransform(loadingText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetupText(loadingText, "Searching...", (int)FontSizes.Body, CYAN_NEON, FontStyles.Bold);
            loadingIndicator.SetActive(false);

            // ========== CREAR PREFAB DE PLAYER CARD ==========
            // Solo crea el prefab, NO agrega cards al ResultsContainer
            // Arrastra el prefab manualmente para ver el diseño
            CreatePlayerCardPrefab();

            InstantiateSampleCards();
            Debug.Log("PlayerCard prefab creado con 3 sample cards en ResultsContainer.");
        }

        private static void InstantiateSampleCards()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            Transform container = canvas.transform.Find("ResultsPanel/ResultsScrollView/Viewport/ResultsContainer");
            if (container == null) { Debug.LogWarning("[SearchPlayersUI] ResultsContainer no encontrado para sample cards"); return; }

            string prefabPath = "Assets/_Project/Prefabs/Common/PlayerCard.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) { Debug.LogWarning("[SearchPlayersUI] PlayerCard prefab no encontrado"); return; }

            // Remove old samples
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                if (container.GetChild(i).name.StartsWith("SampleCard_"))
                    DestroyImmediate(container.GetChild(i).gameObject);
            }

            string[] names = { "StarPlayer", "QuickMind", "NeonKing" };
            for (int i = 0; i < 3; i++)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.transform.SetParent(container, false);
                instance.name = $"SampleCard_{i + 1}";

                var userText = instance.GetComponentInChildren<TextMeshProUGUI>();
                if (userText != null) userText.text = names[i];
            }

            // Make ResultsPanel visible and hide EmptyState for preview
            Transform resultsPanel = canvas.transform.Find("ResultsPanel");
            if (resultsPanel != null) resultsPanel.gameObject.SetActive(true);
            Transform emptyState = resultsPanel != null ? resultsPanel.Find("EmptyState") : null;
            if (emptyState != null) emptyState.gameObject.SetActive(false);

            Debug.Log("[SearchPlayersUI] 3 sample cards instanciados en ResultsContainer");
        }

        private static GameObject CreatePlayerCardPrefab()
        {
            GameObject card = new GameObject("PlayerCard");
            RectTransform cardRT = card.AddComponent<RectTransform>();

            LayoutElement cardLayout = card.AddComponent<LayoutElement>();
            cardLayout.minHeight = 140;
            cardLayout.preferredHeight = 140;
            cardLayout.flexibleWidth = 1;

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.06f, 0.08f, 0.12f, 1f);

            Outline cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = new Color(CYAN_DARK.r, CYAN_DARK.g, CYAN_DARK.b, 0.35f);
            cardOutline.effectDistance = new Vector2(1.5f, 1.5f);

            Sprite circleSprite = GenerateCircleSprite();

            // ========== CIRCULAR AVATAR (left, vertically centered) ==========
            GameObject avatarFrame = new GameObject("AvatarFrame");
            avatarFrame.transform.SetParent(card.transform, false);
            RectTransform avatarFrameRT = avatarFrame.AddComponent<RectTransform>();
            avatarFrameRT.anchorMin = new Vector2(0, 0.5f);
            avatarFrameRT.anchorMax = new Vector2(0, 0.5f);
            avatarFrameRT.anchoredPosition = new Vector2(50, 0);
            avatarFrameRT.sizeDelta = new Vector2(64, 64);
            Image frameImg = avatarFrame.AddComponent<Image>();
            frameImg.sprite = circleSprite;
            frameImg.color = CYAN_DARK;

            GameObject avatarMask = new GameObject("AvatarMask");
            avatarMask.transform.SetParent(avatarFrame.transform, false);
            RectTransform amRT = avatarMask.AddComponent<RectTransform>();
            amRT.anchorMin = new Vector2(0.06f, 0.06f);
            amRT.anchorMax = new Vector2(0.94f, 0.94f);
            amRT.offsetMin = Vector2.zero;
            amRT.offsetMax = Vector2.zero;
            Image amImg = avatarMask.AddComponent<Image>();
            amImg.sprite = circleSprite;
            amImg.color = new Color(0.08f, 0.1f, 0.14f, 1f);
            avatarMask.AddComponent<Mask>().showMaskGraphic = true;

            GameObject avatarImage = new GameObject("AvatarImage");
            avatarImage.transform.SetParent(avatarMask.transform, false);
            RectTransform avatarImgRT = avatarImage.AddComponent<RectTransform>();
            avatarImgRT.anchorMin = Vector2.zero;
            avatarImgRT.anchorMax = Vector2.one;
            avatarImgRT.offsetMin = Vector2.zero;
            avatarImgRT.offsetMax = Vector2.zero;
            Image avatarImg = avatarImage.AddComponent<Image>();
            avatarImg.color = Color.white;
            avatarImg.preserveAspect = true;
            Sprite defaultAvatar = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/Social/AvatarDefault.png");
            if (defaultAvatar != null) avatarImg.sprite = defaultAvatar;

            // ========== RIGHT CONTENT (anchored right of avatar) ==========
            GameObject content = new GameObject("ContentSection");
            content.transform.SetParent(card.transform, false);
            RectTransform contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 0);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.offsetMin = new Vector2(90, 8);  // left padding past avatar
            contentRT.offsetMax = new Vector2(-10, -8); // right padding

            // --- Row 1: Username + Online status ---
            GameObject topRow = new GameObject("TopRow");
            topRow.transform.SetParent(content.transform, false);
            RectTransform topRT = topRow.AddComponent<RectTransform>();
            topRT.anchorMin = new Vector2(0, 0.7f);
            topRT.anchorMax = new Vector2(1, 1f);
            topRT.offsetMin = Vector2.zero;
            topRT.offsetMax = Vector2.zero;

            // Username (no @ prefix)
            GameObject usernameObj = new GameObject("Username");
            usernameObj.transform.SetParent(topRow.transform, false);
            RectTransform usernameRT = usernameObj.AddComponent<RectTransform>();
            usernameRT.anchorMin = new Vector2(0, 0);
            usernameRT.anchorMax = new Vector2(0.65f, 1);
            usernameRT.offsetMin = Vector2.zero;
            usernameRT.offsetMax = Vector2.zero;
            TextMeshProUGUI usernameTmp = usernameObj.AddComponent<TextMeshProUGUI>();
            usernameTmp.text = "Username";
            usernameTmp.fontSize = FontSizes.BodySmall;
            usernameTmp.color = Color.white;
            usernameTmp.fontStyle = FontStyles.Bold;
            usernameTmp.alignment = TextAlignmentOptions.MidlineLeft;
            usernameTmp.overflowMode = TextOverflowModes.Ellipsis;
            usernameTmp.enableAutoSizing = true;
            usernameTmp.fontSizeMin = FontSizes.Caption;
            usernameTmp.fontSizeMax = FontSizes.BodySmall;

            // Online indicator (dot + label, right-aligned)
            GameObject onlineDot = new GameObject("OnlineStatus");
            onlineDot.transform.SetParent(topRow.transform, false);
            RectTransform dotRT = onlineDot.AddComponent<RectTransform>();
            dotRT.anchorMin = new Vector2(1, 0.5f);
            dotRT.anchorMax = new Vector2(1, 0.5f);
            dotRT.anchoredPosition = new Vector2(-8, 0);
            dotRT.sizeDelta = new Vector2(12, 12);
            Image dotImg = onlineDot.AddComponent<Image>();
            dotImg.sprite = circleSprite;
            dotImg.color = new Color(0.2f, 1f, 0.4f, 1f);

            GameObject onlineLabel = new GameObject("OnlineLabel");
            onlineLabel.transform.SetParent(topRow.transform, false);
            RectTransform labelRT = onlineLabel.AddComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0.65f, 0);
            labelRT.anchorMax = new Vector2(1, 1);
            labelRT.offsetMin = Vector2.zero;
            labelRT.offsetMax = new Vector2(-22, 0); // space for dot
            TextMeshProUGUI labelTmp = onlineLabel.AddComponent<TextMeshProUGUI>();
            labelTmp.text = "Online";
            labelTmp.fontSize = FontSizes.Caption;
            labelTmp.fontStyle = FontStyles.Bold;
            labelTmp.color = new Color(0.2f, 1f, 0.4f, 1f);
            labelTmp.alignment = TextAlignmentOptions.MidlineRight;
            labelTmp.overflowMode = TextOverflowModes.Ellipsis;

            // --- Row 2: Stats (WinRate + Game) ---
            GameObject statsRow = new GameObject("StatsRow");
            statsRow.transform.SetParent(content.transform, false);
            RectTransform statsRT = statsRow.AddComponent<RectTransform>();
            statsRT.anchorMin = new Vector2(0, 0.38f);
            statsRT.anchorMax = new Vector2(1, 0.62f);
            statsRT.offsetMin = Vector2.zero;
            statsRT.offsetMax = Vector2.zero;

            // Single stats text "65% WR · Digit Rush"
            GameObject statsText = new GameObject("StatsText");
            statsText.transform.SetParent(statsRow.transform, false);
            RectTransform statsTextRT = statsText.AddComponent<RectTransform>();
            statsTextRT.anchorMin = Vector2.zero;
            statsTextRT.anchorMax = Vector2.one;
            statsTextRT.offsetMin = Vector2.zero;
            statsTextRT.offsetMax = Vector2.zero;
            TextMeshProUGUI statsTmp = statsText.AddComponent<TextMeshProUGUI>();
            statsTmp.text = "0% WR · Digit Rush";
            statsTmp.fontSize = FontSizes.Caption;
            statsTmp.fontStyle = FontStyles.Bold;
            statsTmp.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            statsTmp.alignment = TextAlignmentOptions.MidlineLeft;
            statsTmp.overflowMode = TextOverflowModes.Ellipsis;

            // --- Row 3: Buttons ---
            GameObject buttonsRow = new GameObject("ButtonsRow");
            buttonsRow.transform.SetParent(content.transform, false);
            RectTransform btnRT = buttonsRow.AddComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0, 0);
            btnRT.anchorMax = new Vector2(1, 0.35f);
            btnRT.offsetMin = Vector2.zero;
            btnRT.offsetMax = Vector2.zero;
            HorizontalLayoutGroup btnLayout = buttonsRow.AddComponent<HorizontalLayoutGroup>();
            btnLayout.childAlignment = TextAnchor.MiddleLeft;
            btnLayout.spacing = 10;
            btnLayout.childForceExpandWidth = true;
            btnLayout.childForceExpandHeight = true;
            btnLayout.childControlWidth = true;
            btnLayout.childControlHeight = true;
            btnLayout.padding = new RectOffset(0, 0, 2, 2);

            CreatePrefabButton(buttonsRow.transform, "AddFriendButton", "+ Add", CYAN_NEON, DARK_BG, 0, false);
            CreatePrefabButton(buttonsRow.transform, "ViewProfileButton", "View Profile", new Color(0.05f, 0.1f, 0.15f, 1f), CYAN_NEON, 0, true);

            // Save prefab
            string prefabPath = "Assets/_Project/Prefabs/Common/PlayerCard.prefab";
            if (!System.IO.Directory.Exists("Assets/_Project/Prefabs/Common"))
                System.IO.Directory.CreateDirectory("Assets/_Project/Prefabs/Common");

            PrefabUtility.SaveAsPrefabAsset(card, prefabPath);
            DestroyImmediate(card);
            return null;
        }

        private static void CreatePrefabButton(Transform parent, string name, string text, Color bgColor, Color textColor, float width, bool isOutline)
        {
            GameObject button = new GameObject(name);
            button.transform.SetParent(parent, false);
            RectTransform btnRect = button.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(width, 40);

            Image btnBg = button.AddComponent<Image>();
            btnBg.color = bgColor;

            Button btn = button.AddComponent<Button>();
            btn.targetGraphic = btnBg;

            // Configurar colores del botón para efecto hover
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            btn.colors = colors;

            if (isOutline)
            {
                // Borde neón cyan para botón secundario
                Outline outline = button.AddComponent<Outline>();
                outline.effectColor = CYAN_NEON;
                outline.effectDistance = new Vector2(1.5f, 1.5f);
            }
            else
            {
                // Glow sutil para botón primario
                Outline glow = button.AddComponent<Outline>();
                glow.effectColor = new Color(0f, 1f, 1f, 0.5f);
                glow.effectDistance = new Vector2(2, 2);
            }

            GameObject btnText = new GameObject("Text");
            btnText.transform.SetParent(button.transform, false);
            RectTransform textRect = btnText.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI tmp = btnText.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = FontSizes.Body;
            tmp.color = textColor;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        // ========== UTILIDADES ==========

        private static void CleanupOldUI()
        {
            string[] toClean = { "Background", "SafeArea" };
            foreach (var canvas in Object.FindObjectsOfType<Canvas>(true))
            {
                if (canvas.transform.parent != null) continue;
                // No tocar TransitionCanvas ni EffectsCanvas
                if (canvas.gameObject.name.Contains("Transition") ||
                    canvas.gameObject.name.Contains("Effects")) continue;
                foreach (string name in toClean)
                {
                    Transform t = canvas.transform.Find(name);
                    if (t != null) Object.DestroyImmediate(t.gameObject);
                }
            }
        }

        private static GameObject CreateOrFind(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null) return existing.gameObject;

            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            if (obj.GetComponent<RectTransform>() == null)
                obj.AddComponent<RectTransform>();

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

        private static void SetupText(GameObject obj, string text, int fontSize, Color color, FontStyles style)
        {
            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = obj.AddComponent<TextMeshProUGUI>();

            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinBody;
            tmp.fontSizeMax = fontSize > 0 ? fontSize : FontSizes.Body;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;
        }

        private static void AddLayoutElement(GameObject obj, float width, float height)
        {
            LayoutElement layout = obj.GetComponent<LayoutElement>();
            if (layout == null) layout = obj.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = height;
        }
        private static Sprite GenerateCircleSprite()
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/UI/CircleSprite.png");
            if (s != null) return s;
            // Fallback: generate at runtime (won't survive prefab save)
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size / 2f;
            float radius = center - 1f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dist = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
                    if (dist <= radius) tex.SetPixel(x, y, Color.white);
                    else if (dist <= radius + 1f) tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(radius + 1f - dist)));
                    else tex.SetPixel(x, y, Color.clear);
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }
    }
}
