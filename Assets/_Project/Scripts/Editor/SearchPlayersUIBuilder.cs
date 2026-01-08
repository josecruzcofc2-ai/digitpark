using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

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
        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color PANEL_BG = new Color(0.05f, 0.1f, 0.15f, 0.95f);
        private static readonly Color INPUT_BG = new Color(0.08f, 0.12f, 0.18f, 1f);
        private static readonly Color PLACEHOLDER_COLOR = new Color(0.4f, 0.4f, 0.4f, 1f);

        [MenuItem("DigitPark/Rebuild SearchPlayers UI")]
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
        }

        private static void RebuildSearchPlayersUI()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No se encontró Canvas en la escena");
                return;
            }

            Transform canvasTransform = canvas.transform;

            Transform background = canvasTransform.Find("Background");
            if (background == null)
            {
                Debug.LogError("No se encontró Background");
                return;
            }

            // Limpiar elementos viejos
            CleanOldElements(canvasTransform);

            // Crear nueva estructura
            CreateSearchPlayersLayout(canvasTransform);

            Debug.Log("SearchPlayers UI reconstruida exitosamente!");
            EditorUtility.SetDirty(canvas.gameObject);
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
            // ========== HEADER ==========
            GameObject header = CreateOrFind(canvasTransform, "Header");
            SetupRectTransform(header,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -70), new Vector2(0, 140));

            GameObject title = CreateOrFind(header.transform, "TitleText");
            SetupRectTransform(title,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 0), new Vector2(500, 60));
            SetupText(title, "BUSCAR JUGADORES", 38, CYAN_NEON, FontStyles.Bold);

            // ========== SEARCH BAR ==========
            GameObject searchBar = CreateOrFind(canvasTransform, "SearchBar");
            SetupRectTransform(searchBar,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -200), new Vector2(-80, 80));

            Image searchBarBg = searchBar.GetComponent<Image>();
            if (searchBarBg == null) searchBarBg = searchBar.AddComponent<Image>();
            searchBarBg.color = PANEL_BG;

            HorizontalLayoutGroup searchLayout = searchBar.GetComponent<HorizontalLayoutGroup>();
            if (searchLayout == null) searchLayout = searchBar.AddComponent<HorizontalLayoutGroup>();
            searchLayout.childAlignment = TextAnchor.MiddleCenter;
            searchLayout.spacing = 15;
            searchLayout.padding = new RectOffset(20, 20, 12, 12);
            searchLayout.childForceExpandWidth = false;
            searchLayout.childForceExpandHeight = true;
            searchLayout.childControlHeight = true;

            // Search Icon - Image para arrastrar sprite de lupa
            GameObject searchIcon = CreateOrFind(searchBar.transform, "SearchIcon");
            AddLayoutElement(searchIcon, 50, 50);
            Image searchIconImg = searchIcon.GetComponent<Image>();
            if (searchIconImg == null) searchIconImg = searchIcon.AddComponent<Image>();
            searchIconImg.color = CYAN_NEON;
            searchIconImg.preserveAspect = true;
            // ARRASTRA TU SPRITE DE LUPA AQUI en el Inspector

            // Input Field Container
            GameObject inputContainer = CreateOrFind(searchBar.transform, "InputContainer");
            AddLayoutElement(inputContainer, 600, 56);

            Image inputBg = inputContainer.GetComponent<Image>();
            if (inputBg == null) inputBg = inputContainer.AddComponent<Image>();
            inputBg.color = INPUT_BG;

            Outline inputOutline = inputContainer.GetComponent<Outline>();
            if (inputOutline == null) inputOutline = inputContainer.AddComponent<Outline>();
            inputOutline.effectColor = CYAN_DARK;
            inputOutline.effectDistance = new Vector2(1, 1);

            // Input Field
            GameObject inputField = CreateOrFind(inputContainer.transform, "SearchInputField");
            SetupRectTransform(inputField, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-20, -10));

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
            placeholderTmp.text = "Buscar por nombre de usuario...";
            placeholderTmp.fontSize = 24;
            placeholderTmp.color = PLACEHOLDER_COLOR;
            placeholderTmp.fontStyle = FontStyles.Italic;
            placeholderTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // Text
            GameObject inputText = CreateOrFind(textArea.transform, "Text");
            SetupRectTransform(inputText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI inputTmp = inputText.GetComponent<TextMeshProUGUI>();
            if (inputTmp == null) inputTmp = inputText.AddComponent<TextMeshProUGUI>();
            inputTmp.text = "";
            inputTmp.fontSize = 24;
            inputTmp.color = Color.white;
            inputTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // Configure input field
            tmpInput.textViewport = textArea.GetComponent<RectTransform>();
            tmpInput.textComponent = inputTmp;
            tmpInput.placeholder = placeholderTmp;
            tmpInput.fontAsset = inputTmp.font;
            tmpInput.pointSize = 24;

            // Search Button
            GameObject searchButton = CreateOrFind(searchBar.transform, "SearchButton");
            AddLayoutElement(searchButton, 120, 56);

            Image searchBtnBg = searchButton.GetComponent<Image>();
            if (searchBtnBg == null) searchBtnBg = searchButton.AddComponent<Image>();
            searchBtnBg.color = CYAN_NEON;

            Button searchBtn = searchButton.GetComponent<Button>();
            if (searchBtn == null) searchBtn = searchButton.AddComponent<Button>();
            searchBtn.targetGraphic = searchBtnBg;

            GameObject searchBtnText = CreateOrFind(searchButton.transform, "Text");
            SetupRectTransform(searchBtnText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetupText(searchBtnText, "Buscar", 22, DARK_BG, FontStyles.Bold);

            // Clear Button - Image para arrastrar sprite de X
            GameObject clearButton = CreateOrFind(searchBar.transform, "ClearButton");
            AddLayoutElement(clearButton, 50, 50);

            Image clearBtnBg = clearButton.GetComponent<Image>();
            if (clearBtnBg == null) clearBtnBg = clearButton.AddComponent<Image>();
            clearBtnBg.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);

            Button clearBtn = clearButton.GetComponent<Button>();
            if (clearBtn == null) clearBtn = clearButton.AddComponent<Button>();
            clearBtn.targetGraphic = clearBtnBg;

            // Icono X - Image para arrastrar sprite
            GameObject clearIcon = CreateOrFind(clearButton.transform, "ClearIcon");
            SetupRectTransform(clearIcon, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-10, -10));
            Image clearIconImg = clearIcon.GetComponent<Image>();
            if (clearIconImg == null) clearIconImg = clearIcon.AddComponent<Image>();
            clearIconImg.color = Color.white;
            clearIconImg.preserveAspect = true;
            clearIconImg.raycastTarget = false;
            // ARRASTRA TU SPRITE DE X AQUI en el Inspector

            // ========== RESULTS PANEL ==========
            GameObject resultsPanel = CreateOrFind(canvasTransform, "ResultsPanel");
            SetupRectTransform(resultsPanel,
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(0, -80), new Vector2(-80, -320));

            Image resultsBg = resultsPanel.GetComponent<Image>();
            if (resultsBg == null) resultsBg = resultsPanel.AddComponent<Image>();
            resultsBg.color = PANEL_BG;

            Outline resultsOutline = resultsPanel.GetComponent<Outline>();
            if (resultsOutline == null) resultsOutline = resultsPanel.AddComponent<Outline>();
            resultsOutline.effectColor = CYAN_DARK;
            resultsOutline.effectDistance = new Vector2(1, 1);

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

            // Eliminar Image viejo si existe (RectMask2D no lo necesita)
            Image oldImg = viewport.GetComponent<Image>();
            if (oldImg != null) DestroyImmediate(oldImg);

            // Usar RectMask2D - funciona sin necesidad de Image
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
                Vector2.zero, new Vector2(400, 300));
            // Mostrar EmptyState por defecto (se oculta cuando hay resultados)
            emptyState.SetActive(true);

            // Empty Icon - Image para arrastrar sprite de personas
            GameObject emptyIcon = CreateOrFind(emptyState.transform, "EmptyIcon");
            SetupRectTransform(emptyIcon,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -50), new Vector2(100, 100));
            Image emptyIconImg = emptyIcon.GetComponent<Image>();
            if (emptyIconImg == null) emptyIconImg = emptyIcon.AddComponent<Image>();
            emptyIconImg.color = CYAN_DARK;
            emptyIconImg.preserveAspect = true;
            // ARRASTRA TU SPRITE DE PERSONAS AQUI en el Inspector

            // Empty Title
            GameObject emptyTitle = CreateOrFind(emptyState.transform, "Title");
            SetupRectTransform(emptyTitle,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 10), new Vector2(350, 50));
            SetupText(emptyTitle, "Busca jugadores", 28, Color.white, FontStyles.Bold);

            // Empty Description
            GameObject emptyDesc = CreateOrFind(emptyState.transform, "Description");
            SetupRectTransform(emptyDesc,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -50), new Vector2(350, 80));
            SetupText(emptyDesc, "Encuentra jugadores para\nagregar como amigos o retar", 22, PLACEHOLDER_COLOR, FontStyles.Normal);

            // No Results Text (se mostrará cuando no haya resultados)
            GameObject noResultsText = CreateOrFind(resultsPanel.transform, "NoResultsText");
            SetupRectTransform(noResultsText,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(400, 60));
            SetupText(noResultsText, "No se encontraron jugadores", 24, PLACEHOLDER_COLOR, FontStyles.Normal);
            noResultsText.SetActive(false);

            // Loading Indicator
            GameObject loadingIndicator = CreateOrFind(resultsPanel.transform, "LoadingIndicator");
            SetupRectTransform(loadingIndicator,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(200, 100));

            GameObject loadingText = CreateOrFind(loadingIndicator.transform, "Text");
            SetupRectTransform(loadingText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetupText(loadingText, "Buscando...", 24, CYAN_NEON, FontStyles.Normal);
            loadingIndicator.SetActive(false);

            // ========== CREAR PREFAB DE PLAYER CARD ==========
            // Solo crea el prefab, NO agrega cards al ResultsContainer
            // Arrastra el prefab manualmente para ver el diseño
            CreatePlayerCardPrefab();

            Debug.Log("PlayerCard prefab creado en: Assets/_Project/Prefabs/UI/PlayerCard.prefab");
            Debug.Log("Arrastra el prefab al ResultsContainer para ver el diseño.");
        }

        private static GameObject CreatePlayerCardPrefab()
        {
            // Crear objeto temporal para el prefab
            GameObject card = new GameObject("PlayerCard");
            card.AddComponent<RectTransform>();

            // LayoutElement con altura fija
            LayoutElement cardLayout = card.AddComponent<LayoutElement>();
            cardLayout.minHeight = 150;
            cardLayout.preferredHeight = 150;
            cardLayout.flexibleWidth = 1;

            // Card Background - Color VISIBLE que contrasta con el fondo
            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.08f, 0.15f, 0.22f, 1f); // Azul oscuro visible

            // Card Outline - Borde neón cyan brillante
            Outline cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = CYAN_NEON;
            cardOutline.effectDistance = new Vector2(3f, 3f);

            // ========== AVATAR ==========
            GameObject avatarContainer = new GameObject("AvatarContainer");
            avatarContainer.transform.SetParent(card.transform, false);
            RectTransform avatarRect = avatarContainer.AddComponent<RectTransform>();
            avatarRect.anchorMin = new Vector2(0, 0.5f);
            avatarRect.anchorMax = new Vector2(0, 0.5f);
            avatarRect.anchoredPosition = new Vector2(55, 0);
            avatarRect.sizeDelta = new Vector2(70, 70);
            Image avatarBg = avatarContainer.AddComponent<Image>();
            avatarBg.color = CYAN_NEON; // Cyan brillante para que sea visible
            // Borde neón para avatar
            Outline avatarOutline = avatarContainer.AddComponent<Outline>();
            avatarOutline.effectColor = Color.white;
            avatarOutline.effectDistance = new Vector2(2, 2);

            GameObject avatarImage = new GameObject("AvatarImage");
            avatarImage.transform.SetParent(avatarContainer.transform, false);
            RectTransform avatarImgRect = avatarImage.AddComponent<RectTransform>();
            avatarImgRect.anchorMin = Vector2.zero;
            avatarImgRect.anchorMax = Vector2.one;
            avatarImgRect.sizeDelta = new Vector2(-8, -8);
            Image avatarImg = avatarImage.AddComponent<Image>();
            avatarImg.color = CYAN_DARK; // Placeholder oscuro
            avatarImg.preserveAspect = true;

            // ========== INFO SECTION ==========
            GameObject infoSection = new GameObject("InfoSection");
            infoSection.transform.SetParent(card.transform, false);
            RectTransform infoRect = infoSection.AddComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0, 0.5f);
            infoRect.anchorMax = new Vector2(1, 0.5f);
            infoRect.anchoredPosition = new Vector2(55, 15);
            infoRect.sizeDelta = new Vector2(-220, 70);

            // Username
            GameObject usernameObj = new GameObject("Username");
            usernameObj.transform.SetParent(infoSection.transform, false);
            RectTransform usernameRect = usernameObj.AddComponent<RectTransform>();
            usernameRect.anchorMin = new Vector2(0, 1);
            usernameRect.anchorMax = new Vector2(0.7f, 1);
            usernameRect.anchoredPosition = new Vector2(0, -8);
            usernameRect.sizeDelta = new Vector2(0, 28);
            TextMeshProUGUI usernameTmp = usernameObj.AddComponent<TextMeshProUGUI>();
            usernameTmp.text = "Username";
            usernameTmp.fontSize = 22;
            usernameTmp.color = Color.white;
            usernameTmp.fontStyle = FontStyles.Bold;
            usernameTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // Handle
            GameObject handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(infoSection.transform, false);
            RectTransform handleRect = handleObj.AddComponent<RectTransform>();
            handleRect.anchorMin = new Vector2(0, 1);
            handleRect.anchorMax = new Vector2(0.5f, 1);
            handleRect.anchoredPosition = new Vector2(0, -32);
            handleRect.sizeDelta = new Vector2(0, 22);
            TextMeshProUGUI handleTmp = handleObj.AddComponent<TextMeshProUGUI>();
            handleTmp.text = "@handle";
            handleTmp.fontSize = 16;
            handleTmp.color = PLACEHOLDER_COLOR;
            handleTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // Stats Row
            GameObject statsRow = new GameObject("StatsRow");
            statsRow.transform.SetParent(infoSection.transform, false);
            RectTransform statsRect = statsRow.AddComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0, 0);
            statsRect.anchorMax = new Vector2(1, 0);
            statsRect.anchoredPosition = new Vector2(0, 18);
            statsRect.sizeDelta = new Vector2(0, 24);

            // WinRate Icon
            GameObject winIcon = new GameObject("WinRateIcon");
            winIcon.transform.SetParent(statsRow.transform, false);
            RectTransform winIconRect = winIcon.AddComponent<RectTransform>();
            winIconRect.anchorMin = new Vector2(0, 0.5f);
            winIconRect.anchorMax = new Vector2(0, 0.5f);
            winIconRect.anchoredPosition = new Vector2(10, 0);
            winIconRect.sizeDelta = new Vector2(18, 18);
            Image winIconImg = winIcon.AddComponent<Image>();
            winIconImg.color = new Color(1f, 0.85f, 0.2f, 1f);
            winIconImg.preserveAspect = true;

            // WinRate Text
            GameObject winText = new GameObject("WinRateText");
            winText.transform.SetParent(statsRow.transform, false);
            RectTransform winTextRect = winText.AddComponent<RectTransform>();
            winTextRect.anchorMin = new Vector2(0, 0.5f);
            winTextRect.anchorMax = new Vector2(0, 0.5f);
            winTextRect.anchoredPosition = new Vector2(65, 0);
            winTextRect.sizeDelta = new Vector2(80, 24);
            TextMeshProUGUI winTextTmp = winText.AddComponent<TextMeshProUGUI>();
            winTextTmp.text = "0%";
            winTextTmp.fontSize = 15;
            winTextTmp.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            winTextTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // Separator
            GameObject sep = new GameObject("Separator");
            sep.transform.SetParent(statsRow.transform, false);
            RectTransform sepRect = sep.AddComponent<RectTransform>();
            sepRect.anchorMin = new Vector2(0, 0.5f);
            sepRect.anchorMax = new Vector2(0, 0.5f);
            sepRect.anchoredPosition = new Vector2(115, 0);
            sepRect.sizeDelta = new Vector2(20, 20);
            TextMeshProUGUI sepTmp = sep.AddComponent<TextMeshProUGUI>();
            sepTmp.text = "•";
            sepTmp.fontSize = 14;
            sepTmp.color = PLACEHOLDER_COLOR;
            sepTmp.alignment = TextAlignmentOptions.Center;

            // FavGame Icon
            GameObject gameIcon = new GameObject("FavGameIcon");
            gameIcon.transform.SetParent(statsRow.transform, false);
            RectTransform gameIconRect = gameIcon.AddComponent<RectTransform>();
            gameIconRect.anchorMin = new Vector2(0, 0.5f);
            gameIconRect.anchorMax = new Vector2(0, 0.5f);
            gameIconRect.anchoredPosition = new Vector2(135, 0);
            gameIconRect.sizeDelta = new Vector2(18, 18);
            Image gameIconImg = gameIcon.AddComponent<Image>();
            gameIconImg.color = CYAN_NEON;
            gameIconImg.preserveAspect = true;

            // FavGame Text
            GameObject gameText = new GameObject("FavGameText");
            gameText.transform.SetParent(statsRow.transform, false);
            RectTransform gameTextRect = gameText.AddComponent<RectTransform>();
            gameTextRect.anchorMin = new Vector2(0, 0.5f);
            gameTextRect.anchorMax = new Vector2(0, 0.5f);
            gameTextRect.anchoredPosition = new Vector2(200, 0);
            gameTextRect.sizeDelta = new Vector2(100, 24);
            TextMeshProUGUI gameTextTmp = gameText.AddComponent<TextMeshProUGUI>();
            gameTextTmp.text = "Game";
            gameTextTmp.fontSize = 15;
            gameTextTmp.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            gameTextTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // ========== ONLINE STATUS ==========
            GameObject onlineStatus = new GameObject("OnlineStatus");
            onlineStatus.transform.SetParent(card.transform, false);
            RectTransform onlineRect = onlineStatus.AddComponent<RectTransform>();
            onlineRect.anchorMin = new Vector2(1, 1);
            onlineRect.anchorMax = new Vector2(1, 1);
            onlineRect.anchoredPosition = new Vector2(-25, -20);
            onlineRect.sizeDelta = new Vector2(14, 14);
            Image onlineImg = onlineStatus.AddComponent<Image>();
            onlineImg.color = new Color(0.2f, 1f, 0.4f, 1f);

            GameObject onlineLabel = new GameObject("OnlineLabel");
            onlineLabel.transform.SetParent(card.transform, false);
            RectTransform labelRect = onlineLabel.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(1, 1);
            labelRect.anchorMax = new Vector2(1, 1);
            labelRect.anchoredPosition = new Vector2(-70, -20);
            labelRect.sizeDelta = new Vector2(70, 20);
            TextMeshProUGUI labelTmp = onlineLabel.AddComponent<TextMeshProUGUI>();
            labelTmp.text = "Online";
            labelTmp.fontSize = 13;
            labelTmp.color = new Color(0.2f, 1f, 0.4f, 1f);
            labelTmp.alignment = TextAlignmentOptions.MidlineRight;

            // ========== BUTTONS ==========
            GameObject buttonsRow = new GameObject("ButtonsRow");
            buttonsRow.transform.SetParent(card.transform, false);
            RectTransform btnRowRect = buttonsRow.AddComponent<RectTransform>();
            btnRowRect.anchorMin = new Vector2(0.5f, 0);
            btnRowRect.anchorMax = new Vector2(0.5f, 0);
            btnRowRect.anchoredPosition = new Vector2(30, 35);
            btnRowRect.sizeDelta = new Vector2(320, 45);
            HorizontalLayoutGroup btnLayout = buttonsRow.AddComponent<HorizontalLayoutGroup>();
            btnLayout.childAlignment = TextAnchor.MiddleCenter;
            btnLayout.spacing = 20;
            btnLayout.childForceExpandWidth = false;
            btnLayout.childForceExpandHeight = true;
            btnLayout.childControlWidth = false;
            btnLayout.childControlHeight = true;

            // Botón primario cyan brillante
            CreatePrefabButton(buttonsRow.transform, "AddFriendButton", "+ Agregar", CYAN_NEON, DARK_BG, 140, false);
            // Botón secundario con borde neón
            CreatePrefabButton(buttonsRow.transform, "ViewProfileButton", "Ver Perfil", new Color(0.05f, 0.1f, 0.15f, 1f), CYAN_NEON, 140, true);

            // Guardar como prefab
            string prefabPath = "Assets/_Project/Prefabs/UI/PlayerCard.prefab";

            // Asegurar que el directorio existe
            if (!System.IO.Directory.Exists("Assets/_Project/Prefabs/UI"))
            {
                System.IO.Directory.CreateDirectory("Assets/_Project/Prefabs/UI");
            }

            PrefabUtility.SaveAsPrefabAsset(card, prefabPath);

            // Destruir el objeto temporal
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
            tmp.fontSize = 16;
            tmp.color = textColor;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        // ========== UTILIDADES ==========

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
            tmp.raycastTarget = false;
        }

        private static void AddLayoutElement(GameObject obj, float width, float height)
        {
            LayoutElement layout = obj.GetComponent<LayoutElement>();
            if (layout == null) layout = obj.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = height;
        }
    }
}
