using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para reconstruir la UI del Main Menu con diseño profesional
    /// Resolución: Portrait 9:16 (1080x1920)
    /// </summary>
    public class MainMenuUIBuilder : EditorWindow
    {
        // Colores del tema
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 0.83f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.5f, 1f);
        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color PANEL_BG = new Color(0.05f, 0.1f, 0.15f, 0.98f);
        private static readonly Color HEADER_BG = new Color(0.03f, 0.06f, 0.1f, 0.95f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GOLD_DARK = new Color(0.7f, 0.5f, 0.1f, 1f);
        private static readonly Color GREEN_CASH = new Color(0.2f, 0.9f, 0.4f, 1f);
        private static readonly Color GREEN_DARK = new Color(0.1f, 0.5f, 0.2f, 1f);
        private static readonly Color TEXT_PRIMARY = Color.white;
        private static readonly Color TEXT_SECONDARY = new Color(0.7f, 0.7f, 0.7f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.1f, 0.1f, 0.1f, 1f);
        private static readonly Color BUTTON_SECONDARY = new Color(0.15f, 0.2f, 0.25f, 1f);
        private static readonly Color OVERLAY_COLOR = new Color(0f, 0f, 0f, 0.85f);

        [MenuItem("DigitPark/Main Menu/Rebuild Complete UI")]
        public static void ShowWindow()
        {
            GetWindow<MainMenuUIBuilder>("Main Menu Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Main Menu UI Builder", EditorStyles.boldLabel);
            GUILayout.Label("Resolución: Portrait 9:16 (1080x1920)", EditorStyles.miniLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Este script reconstruirá la UI del Main Menu.\n" +
                "Asegúrate de tener la escena MainMenu abierta.\n\n" +
                "Nueva estructura:\n" +
                "• Header (Settings, Notifications, Profile)\n" +
                "• User Section (Avatar + Username)\n" +
                "• Play Button (CTA Principal)\n" +
                "• Secondary Buttons (Scores, Search)\n" +
                "• Cash Battle Button\n" +
                "• Premium Banner + Panel\n" +
                "• Notifications Panel",
                MessageType.Info);

            GUILayout.Space(15);

            if (GUILayout.Button("RECONSTRUIR TODO", GUILayout.Height(50)))
            {
                RebuildMainMenu();
            }

            GUILayout.Space(10);
            GUILayout.Label("Componentes Individuales:", EditorStyles.boldLabel);

            if (GUILayout.Button("1. Crear Header", GUILayout.Height(30)))
            {
                CreateHeader();
            }

            if (GUILayout.Button("2. Crear User Section", GUILayout.Height(30)))
            {
                CreateUserSection();
            }

            if (GUILayout.Button("3. Crear Main Buttons", GUILayout.Height(30)))
            {
                CreateMainButtons();
            }

            if (GUILayout.Button("4. Crear Premium Banner", GUILayout.Height(30)))
            {
                CreatePremiumBanner();
            }

            if (GUILayout.Button("5. Crear Premium Panel", GUILayout.Height(30)))
            {
                CreatePremiumPanel();
            }

            if (GUILayout.Button("6. Crear Notifications Panel", GUILayout.Height(30)))
            {
                CreateNotificationsPanel();
            }
        }

        #region Main Rebuild

        private static void RebuildMainMenu()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[MainMenuUI] No se encontró Canvas en la escena");
                return;
            }

            // PRIMERO: Limpiar elementos viejos
            CleanOldElements(canvas.transform);

            // Crear estructura base (nuevo orden)
            CreateBackground(canvas.transform);
            CreateHeader();
            CreateTitleSection();      // NUEVO: Título arriba
            CreateUserSection();       // Usuario debajo del título
            CreateMainMenuPanel(canvas.transform);
            CreateMainButtons();
            CreatePremiumBanner();
            CreatePremiumPanel();
            CreateNotificationsPanel();

            Debug.Log("[MainMenuUI] Main Menu reconstruido exitosamente!");
            EditorUtility.SetDirty(canvas.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        }

        private static void CleanOldElements(Transform canvasTransform)
        {
            // Lista de elementos viejos a eliminar
            string[] oldElements = new string[]
            {
                "ScoresButton", "SettingsButton", "PlayButton", "CashBattleButton",
                "PremiumButton", "UserButton", "SearchButton", "TitleText",
                "MainMenuPanel", "Header", "UserSection", "PremiumBanner",
                "PremiumPanel", "NotificationsPanel", "TitleSection", "EditProfileButton"
            };

            foreach (string elementName in oldElements)
            {
                // Buscar en el canvas directamente
                Transform element = canvasTransform.Find(elementName);
                if (element != null)
                {
                    Debug.Log($"[MainMenuUI] Eliminando elemento viejo: {elementName}");
                    DestroyImmediate(element.gameObject);
                }

                // También buscar dentro de MainMenuPanel si existe
                Transform mainMenuPanel = canvasTransform.Find("MainMenuPanel");
                if (mainMenuPanel != null)
                {
                    Transform child = mainMenuPanel.Find(elementName);
                    if (child != null)
                    {
                        Debug.Log($"[MainMenuUI] Eliminando de MainMenuPanel: {elementName}");
                        DestroyImmediate(child.gameObject);
                    }
                }
            }

            Debug.Log("[MainMenuUI] Limpieza de elementos viejos completada");
        }

        #endregion

        #region Background

        private static void CreateBackground(Transform canvasTransform)
        {
            // Background principal
            GameObject bg = FindOrCreate(canvasTransform, "Background");
            bg.transform.SetAsFirstSibling();

            RectTransform bgRT = GetOrAdd<RectTransform>(bg);
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            Image bgImage = GetOrAdd<Image>(bg);
            bgImage.color = DARK_BG;
            bgImage.raycastTarget = true;

            // Gradient overlay (para más profundidad)
            GameObject gradient = FindOrCreate(bg.transform, "GradientOverlay");
            RectTransform gradRT = GetOrAdd<RectTransform>(gradient);
            gradRT.anchorMin = Vector2.zero;
            gradRT.anchorMax = Vector2.one;
            gradRT.offsetMin = Vector2.zero;
            gradRT.offsetMax = Vector2.zero;

            Image gradImage = GetOrAdd<Image>(gradient);
            gradImage.color = new Color(0f, 0.1f, 0.15f, 0.5f);
            gradImage.raycastTarget = false;

            Debug.Log("[MainMenuUI] Background creado");
        }

        #endregion

        #region Header

        private static void CreateHeader()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            GameObject header = FindOrCreate(canvas.transform, "Header");
            // Poner después del Background para que se renderice encima
            Transform bg = canvas.transform.Find("Background");
            if (bg != null)
            {
                header.transform.SetSiblingIndex(bg.GetSiblingIndex() + 1);
            }

            RectTransform headerRT = GetOrAdd<RectTransform>(header);
            headerRT.anchorMin = new Vector2(0, 1);
            headerRT.anchorMax = new Vector2(1, 1);
            headerRT.pivot = new Vector2(0.5f, 1);
            headerRT.anchoredPosition = Vector2.zero;
            headerRT.sizeDelta = new Vector2(0, 120); // Más alto

            Image headerBg = GetOrAdd<Image>(header);
            headerBg.color = new Color(0.05f, 0.08f, 0.12f, 0.95f); // Más visible

            // Settings Button (izquierda)
            CreateHeaderButton(header.transform, "SettingsButton", "settings_icon",
                new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(70, 0));

            // Notifications Button (derecha, junto al profile)
            CreateHeaderButton(header.transform, "NotificationsButton", "notifications_icon",
                new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-140, 0));

            // Notification Badge (punto rojo)
            GameObject notifBtn = header.transform.Find("NotificationsButton")?.gameObject;
            if (notifBtn != null)
            {
                GameObject badge = FindOrCreate(notifBtn.transform, "Badge");
                RectTransform badgeRT = GetOrAdd<RectTransform>(badge);
                badgeRT.anchorMin = new Vector2(1, 1);
                badgeRT.anchorMax = new Vector2(1, 1);
                badgeRT.pivot = new Vector2(0.5f, 0.5f);
                badgeRT.anchoredPosition = new Vector2(-5, -5);
                badgeRT.sizeDelta = new Vector2(20, 20);

                Image badgeImg = GetOrAdd<Image>(badge);
                badgeImg.color = new Color(1f, 0.3f, 0.3f, 1f);

                // Badge count text
                GameObject badgeText = FindOrCreate(badge.transform, "Count");
                RectTransform countRT = GetOrAdd<RectTransform>(badgeText);
                countRT.anchorMin = Vector2.zero;
                countRT.anchorMax = Vector2.one;
                countRT.sizeDelta = Vector2.zero;

                TextMeshProUGUI countTMP = GetOrAdd<TextMeshProUGUI>(badgeText);
                countTMP.text = "3";
                countTMP.fontSize = 12;
                countTMP.color = Color.white;
                countTMP.alignment = TextAlignmentOptions.Center;
                countTMP.fontStyle = FontStyles.Bold;

                badge.SetActive(true); // Mostrar badge por defecto para demo
            }

            // Profile Button (extremo derecha)
            CreateHeaderButton(header.transform, "ProfileButton", "profile_icon",
                new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-60, 0));

            Debug.Log("[MainMenuUI] Header creado");
        }

        #endregion

        #region Title Section

        private static void CreateTitleSection()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            GameObject titleSection = FindOrCreate(canvas.transform, "TitleSection");

            RectTransform titleRT = GetOrAdd<RectTransform>(titleSection);
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0.5f, 1);
            titleRT.anchoredPosition = new Vector2(0, -120); // Debajo del header
            titleRT.sizeDelta = new Vector2(0, 80);

            // Título DIGIT PARK
            GameObject title = FindOrCreate(titleSection.transform, "TitleText");
            RectTransform titleTextRT = GetOrAdd<RectTransform>(title);
            titleTextRT.anchorMin = Vector2.zero;
            titleTextRT.anchorMax = Vector2.one;
            titleTextRT.offsetMin = Vector2.zero;
            titleTextRT.offsetMax = Vector2.zero;

            TextMeshProUGUI titleTMP = GetOrAdd<TextMeshProUGUI>(title);
            titleTMP.text = "DIGIT PARK";
            titleTMP.fontSize = 56;
            titleTMP.color = CYAN_NEON;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;

            // Glow del título
            Shadow titleShadow = GetOrAdd<Shadow>(title);
            titleShadow.effectColor = new Color(CYAN_GLOW.r, CYAN_GLOW.g, CYAN_GLOW.b, 0.6f);
            titleShadow.effectDistance = new Vector2(3, -3);

            Debug.Log("[MainMenuUI] Title Section creada");
        }

        private static void CreateHeaderButton(Transform parent, string name, string iconPlaceholder,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 position)
        {
            GameObject btn = FindOrCreate(parent, name);

            RectTransform btnRT = GetOrAdd<RectTransform>(btn);
            btnRT.anchorMin = anchorMin;
            btnRT.anchorMax = anchorMax;
            btnRT.pivot = new Vector2(0.5f, 0.5f);
            btnRT.anchoredPosition = position;
            btnRT.sizeDelta = new Vector2(70, 70); // Más grande

            // Background circular sutil
            Image btnBg = GetOrAdd<Image>(btn);
            btnBg.color = new Color(1f, 1f, 1f, 0.08f); // Muy sutil

            Button button = GetOrAdd<Button>(btn);
            button.targetGraphic = btnBg;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            button.colors = colors;

            // Icon (Image para el icono real)
            GameObject icon = FindOrCreate(btn.transform, "Icon");
            RectTransform iconRT = GetOrAdd<RectTransform>(icon);
            iconRT.anchorMin = new Vector2(0.1f, 0.1f);
            iconRT.anchorMax = new Vector2(0.9f, 0.9f);
            iconRT.offsetMin = Vector2.zero;
            iconRT.offsetMax = Vector2.zero;

            Image iconImg = GetOrAdd<Image>(icon);
            iconImg.color = Color.white; // Blanco para que se vea el icono
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            // El sprite se asignará manualmente en el Inspector
        }

        #endregion

        #region User Section

        private static void CreateUserSection()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            GameObject userSection = FindOrCreate(canvas.transform, "UserSection");

            RectTransform userRT = GetOrAdd<RectTransform>(userSection);
            userRT.anchorMin = new Vector2(0, 1);  // Stretch horizontal
            userRT.anchorMax = new Vector2(1, 1);
            userRT.pivot = new Vector2(0.5f, 1);
            userRT.anchoredPosition = new Vector2(0, -200); // Justo debajo del título
            userRT.sizeDelta = new Vector2(-80, 170); // Padding horizontal 40px cada lado, más alto para separator

            // Layout vertical
            VerticalLayoutGroup vlg = GetOrAdd<VerticalLayoutGroup>(userSection);
            vlg.spacing = 10;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = false;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;

            // Avatar Container (más grande)
            GameObject avatarContainer = FindOrCreate(userSection.transform, "AvatarContainer");
            RectTransform avatarContRT = GetOrAdd<RectTransform>(avatarContainer);
            avatarContRT.sizeDelta = new Vector2(120, 120); // Más grande

            LayoutElement avatarLE = GetOrAdd<LayoutElement>(avatarContainer);
            avatarLE.preferredWidth = 120;
            avatarLE.preferredHeight = 120;

            // Avatar Frame (borde neón)
            Image frameImg = GetOrAdd<Image>(avatarContainer);
            frameImg.color = CYAN_NEON;

            Outline frameOutline = GetOrAdd<Outline>(avatarContainer);
            frameOutline.effectColor = CYAN_GLOW;
            frameOutline.effectDistance = new Vector2(4, 4);

            // Avatar Image (interior - para foto del usuario)
            GameObject avatarImg = FindOrCreate(avatarContainer.transform, "AvatarImage");
            RectTransform avatarImgRT = GetOrAdd<RectTransform>(avatarImg);
            avatarImgRT.anchorMin = new Vector2(0.05f, 0.05f);
            avatarImgRT.anchorMax = new Vector2(0.95f, 0.95f);
            avatarImgRT.offsetMin = Vector2.zero;
            avatarImgRT.offsetMax = Vector2.zero;

            Image avatar = GetOrAdd<Image>(avatarImg);
            avatar.color = PANEL_BG; // Fondo oscuro
            avatar.preserveAspect = true;
            avatar.type = Image.Type.Simple;

            // Default Avatar Icon (icono de persona por defecto)
            GameObject defaultIcon = FindOrCreate(avatarImg.transform, "DefaultIcon");
            RectTransform defaultIconRT = GetOrAdd<RectTransform>(defaultIcon);
            defaultIconRT.anchorMin = new Vector2(0.15f, 0.15f);
            defaultIconRT.anchorMax = new Vector2(0.85f, 0.85f);
            defaultIconRT.offsetMin = Vector2.zero;
            defaultIconRT.offsetMax = Vector2.zero;

            Image defaultIconImg = GetOrAdd<Image>(defaultIcon);
            defaultIconImg.color = new Color(0.5f, 0.5f, 0.5f, 0.8f); // Gris para el icono
            defaultIconImg.preserveAspect = true;
            // Asignar sprite de persona en el Inspector

            // Username
            GameObject usernameObj = FindOrCreate(userSection.transform, "UsernameText");
            LayoutElement userLE = GetOrAdd<LayoutElement>(usernameObj);
            userLE.preferredHeight = 40;

            TextMeshProUGUI usernameTMP = GetOrAdd<TextMeshProUGUI>(usernameObj);
            usernameTMP.text = "@Username";
            usernameTMP.fontSize = 26;
            usernameTMP.color = TEXT_PRIMARY;
            usernameTMP.fontStyle = FontStyles.Bold;
            usernameTMP.alignment = TextAlignmentOptions.Center;

            // Línea separadora cyan (stretch - ancho completo)
            GameObject separator = FindOrCreate(userSection.transform, "Separator");

            RectTransform sepRT = GetOrAdd<RectTransform>(separator);
            sepRT.anchorMin = new Vector2(0.05f, 0.5f);  // 5% desde los bordes
            sepRT.anchorMax = new Vector2(0.95f, 0.5f);
            sepRT.pivot = new Vector2(0.5f, 0.5f);
            sepRT.sizeDelta = new Vector2(0, 2);  // Ancho desde anchors, 2px alto

            LayoutElement sepLE = GetOrAdd<LayoutElement>(separator);
            sepLE.preferredHeight = 2;
            sepLE.minHeight = 2;
            sepLE.flexibleWidth = 1;  // Permitir stretch
            sepLE.flexibleHeight = 0;
            sepLE.ignoreLayout = true;  // Ignorar el VerticalLayoutGroup para usar anchors

            Image sepImg = GetOrAdd<Image>(separator);
            sepImg.color = new Color(CYAN_DARK.r, CYAN_DARK.g, CYAN_DARK.b, 0.6f);

            Debug.Log("[MainMenuUI] User Section creada");
        }

        #endregion

        #region Main Menu Panel

        private static void CreateMainMenuPanel(Transform canvasTransform)
        {
            GameObject panel = FindOrCreate(canvasTransform, "MainMenuPanel");

            RectTransform panelRT = GetOrAdd<RectTransform>(panel);
            panelRT.anchorMin = new Vector2(0, 0.15f); // Espacio para premium banner
            panelRT.anchorMax = new Vector2(1, 0.52f); // Subir los botones (menos espacio vacío)
            panelRT.offsetMin = new Vector2(40, 0);
            panelRT.offsetMax = new Vector2(-40, 0);

            VerticalLayoutGroup vlg = GetOrAdd<VerticalLayoutGroup>(panel);
            vlg.spacing = 12;
            vlg.padding = new RectOffset(0, 0, 5, 5);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
        }

        #endregion

        #region Main Buttons

        private static void CreateMainButtons()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            Transform panel = canvas.transform.Find("MainMenuPanel");
            if (panel == null)
            {
                CreateMainMenuPanel(canvas.transform);
                panel = canvas.transform.Find("MainMenuPanel");
            }

            // Limpiar botones viejos
            string[] oldButtons = { "PlayButton", "TitleText", "SecondaryButtons", "CashBattleButton" };
            foreach (string btnName in oldButtons)
            {
                Transform old = panel.Find(btnName);
                if (old != null) DestroyImmediate(old.gameObject);
            }

            // === PLAY BUTTON (CTA Principal - MÁS GRANDE) ===
            GameObject playBtn = CreateMainButton(panel, "PlayButton", "JUGAR", CYAN_NEON, TEXT_DARK, 95);

            // Agregar glow especial al botón Play
            Outline playOutline = playBtn.GetComponent<Outline>();
            if (playOutline == null) playOutline = playBtn.AddComponent<Outline>();
            playOutline.effectColor = CYAN_GLOW;
            playOutline.effectDistance = new Vector2(4, 4);

            // Icon para Play
            GameObject playIcon = FindOrCreate(playBtn.transform, "Icon");
            RectTransform playIconRT = GetOrAdd<RectTransform>(playIcon);
            playIconRT.anchorMin = new Vector2(0, 0.5f);
            playIconRT.anchorMax = new Vector2(0, 0.5f);
            playIconRT.pivot = new Vector2(0, 0.5f);
            playIconRT.anchoredPosition = new Vector2(30, 0);
            playIconRT.sizeDelta = new Vector2(50, 50);

            Image playIconImg = GetOrAdd<Image>(playIcon);
            playIconImg.color = TEXT_DARK;
            playIconImg.preserveAspect = true;

            // === SECONDARY BUTTONS (50/50) ===
            GameObject secondary = new GameObject("SecondaryButtons");
            secondary.transform.SetParent(panel, false);

            LayoutElement secLE = secondary.AddComponent<LayoutElement>();
            secLE.preferredHeight = 60;
            secLE.minHeight = 60;

            HorizontalLayoutGroup hlg = secondary.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 15;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // Scores Button
            CreateSecondaryButton(secondary.transform, "ScoresButton", "Rankings", "scores_icon");

            // Search Button (AHORA CON TEXTO)
            CreateSecondaryButton(secondary.transform, "SearchButton", "Buscar", "search_icon");

            // === CASH BATTLE BUTTON (Estilo Dorado Premium) ===
            GameObject cashBtn = CreateMainButton(panel, "CashBattleButton", "CASH BATTLE", GOLD, TEXT_DARK, 60);

            // Fondo dorado degradado
            Image cashBg = cashBtn.GetComponent<Image>();
            if (cashBg != null) cashBg.color = new Color(0.85f, 0.65f, 0.1f, 1f); // Dorado rico

            // Glow dorado sutil
            Outline cashOutline = cashBtn.GetComponent<Outline>();
            if (cashOutline == null) cashOutline = cashBtn.AddComponent<Outline>();
            cashOutline.effectColor = new Color(1f, 0.84f, 0f, 0.7f); // Glow dorado
            cashOutline.effectDistance = new Vector2(3, 3);

            // Shadow para más profundidad
            Shadow cashShadow = cashBtn.GetComponent<Shadow>();
            if (cashShadow == null) cashShadow = cashBtn.AddComponent<Shadow>();
            cashShadow.effectColor = new Color(0.6f, 0.4f, 0f, 0.5f);
            cashShadow.effectDistance = new Vector2(2, -2);

            // Icon para Cash
            GameObject cashIcon = FindOrCreate(cashBtn.transform, "Icon");
            RectTransform cashIconRT = GetOrAdd<RectTransform>(cashIcon);
            cashIconRT.anchorMin = new Vector2(0, 0.5f);
            cashIconRT.anchorMax = new Vector2(0, 0.5f);
            cashIconRT.pivot = new Vector2(0, 0.5f);
            cashIconRT.anchoredPosition = new Vector2(20, 0);
            cashIconRT.sizeDelta = new Vector2(35, 35);

            Image cashIconImg = GetOrAdd<Image>(cashIcon);
            cashIconImg.color = TEXT_DARK;
            cashIconImg.preserveAspect = true;

            Debug.Log("[MainMenuUI] Main Buttons creados");
        }

        private static GameObject CreateMainButton(Transform parent, string name, string text, Color bgColor, Color textColor, float height)
        {
            GameObject btn = new GameObject(name);
            btn.transform.SetParent(parent, false);

            LayoutElement le = btn.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.minHeight = height;

            Image bg = btn.AddComponent<Image>();
            bg.color = bgColor;

            Button button = btn.AddComponent<Button>();
            button.targetGraphic = bg;

            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(bgColor.r * 0.9f, bgColor.g * 0.9f, bgColor.b * 0.9f, 1f);
            colors.pressedColor = new Color(bgColor.r * 0.7f, bgColor.g * 0.7f, bgColor.b * 0.7f, 1f);
            button.colors = colors;

            // Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btn.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(80, 0); // Espacio para icono
            textRT.offsetMax = new Vector2(-20, 0);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = height > 100 ? 36 : 24;
            tmp.color = textColor;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;

            return btn;
        }

        private static void CreateSecondaryButton(Transform parent, string name, string text, string iconPlaceholder)
        {
            GameObject btn = new GameObject(name);
            btn.transform.SetParent(parent, false);

            Image bg = btn.AddComponent<Image>();
            bg.color = BUTTON_SECONDARY;

            Button button = btn.AddComponent<Button>();
            button.targetGraphic = bg;

            Outline outline = btn.AddComponent<Outline>();
            outline.effectColor = CYAN_DARK;
            outline.effectDistance = new Vector2(1.5f, 1.5f);

            // Icon
            GameObject icon = new GameObject("Icon");
            icon.transform.SetParent(btn.transform, false);

            RectTransform iconRT = icon.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0, 0.5f);
            iconRT.anchorMax = new Vector2(0, 0.5f);
            iconRT.pivot = new Vector2(0, 0.5f);
            iconRT.anchoredPosition = new Vector2(15, 0);
            iconRT.sizeDelta = new Vector2(30, 30);

            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = CYAN_NEON;
            iconImg.preserveAspect = true;

            // Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btn.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(50, 0);
            textRT.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 20;
            tmp.color = TEXT_PRIMARY;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        #endregion

        #region Premium Banner

        private static void CreatePremiumBanner()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            GameObject banner = FindOrCreate(canvas.transform, "PremiumBanner");

            RectTransform bannerRT = GetOrAdd<RectTransform>(banner);
            bannerRT.anchorMin = new Vector2(0, 0);
            bannerRT.anchorMax = new Vector2(1, 0);
            bannerRT.pivot = new Vector2(0.5f, 0);
            bannerRT.anchoredPosition = new Vector2(0, 30);
            bannerRT.sizeDelta = new Vector2(-60, 100);

            // Gradiente dorado
            Image bannerBg = GetOrAdd<Image>(banner);
            bannerBg.color = new Color(0.3f, 0.25f, 0.1f, 1f);

            Button bannerBtn = GetOrAdd<Button>(banner);
            bannerBtn.targetGraphic = bannerBg;

            Outline bannerOutline = GetOrAdd<Outline>(banner);
            bannerOutline.effectColor = GOLD;
            bannerOutline.effectDistance = new Vector2(2, 2);

            // Star Icon
            GameObject starIcon = FindOrCreate(banner.transform, "StarIcon");
            RectTransform starRT = GetOrAdd<RectTransform>(starIcon);
            starRT.anchorMin = new Vector2(0, 0.5f);
            starRT.anchorMax = new Vector2(0, 0.5f);
            starRT.pivot = new Vector2(0, 0.5f);
            starRT.anchoredPosition = new Vector2(20, 0);
            starRT.sizeDelta = new Vector2(50, 50);

            Image starImg = GetOrAdd<Image>(starIcon);
            starImg.color = GOLD;
            starImg.preserveAspect = true;

            // Title
            GameObject titleObj = FindOrCreate(banner.transform, "TitleText");
            RectTransform titleRT = GetOrAdd<RectTransform>(titleObj);
            titleRT.anchorMin = new Vector2(0, 0.5f);
            titleRT.anchorMax = new Vector2(0.8f, 1);
            titleRT.offsetMin = new Vector2(80, 5);
            titleRT.offsetMax = new Vector2(0, -5);

            TextMeshProUGUI titleTMP = GetOrAdd<TextMeshProUGUI>(titleObj);
            titleTMP.text = "PREMIUM";
            titleTMP.fontSize = 24;
            titleTMP.color = GOLD;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Left;

            // Subtitle
            GameObject subObj = FindOrCreate(banner.transform, "SubtitleText");
            RectTransform subRT = GetOrAdd<RectTransform>(subObj);
            subRT.anchorMin = new Vector2(0, 0);
            subRT.anchorMax = new Vector2(0.8f, 0.5f);
            subRT.offsetMin = new Vector2(80, 5);
            subRT.offsetMax = new Vector2(0, -5);

            TextMeshProUGUI subTMP = GetOrAdd<TextMeshProUGUI>(subObj);
            subTMP.text = "10 Temas | Sin Anuncios";
            subTMP.fontSize = 16;
            subTMP.color = new Color(0.9f, 0.85f, 0.6f, 1f);
            subTMP.alignment = TextAlignmentOptions.Left;

            // Arrow
            GameObject arrow = FindOrCreate(banner.transform, "ArrowIcon");
            RectTransform arrowRT = GetOrAdd<RectTransform>(arrow);
            arrowRT.anchorMin = new Vector2(1, 0.5f);
            arrowRT.anchorMax = new Vector2(1, 0.5f);
            arrowRT.pivot = new Vector2(1, 0.5f);
            arrowRT.anchoredPosition = new Vector2(-20, 0);
            arrowRT.sizeDelta = new Vector2(30, 30);

            TextMeshProUGUI arrowTMP = GetOrAdd<TextMeshProUGUI>(arrow);
            arrowTMP.text = ">";
            arrowTMP.fontSize = 28;
            arrowTMP.color = GOLD;
            arrowTMP.fontStyle = FontStyles.Bold;
            arrowTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[MainMenuUI] Premium Banner creado");
        }

        #endregion

        #region Premium Panel

        private static void CreatePremiumPanel()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            GameObject panel = FindOrCreate(canvas.transform, "PremiumPanel");
            panel.SetActive(false); // Oculto por defecto

            RectTransform panelRT = GetOrAdd<RectTransform>(panel);
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            // Dark Overlay
            GameObject overlay = FindOrCreate(panel.transform, "DarkOverlay");
            RectTransform overlayRT = GetOrAdd<RectTransform>(overlay);
            overlayRT.anchorMin = Vector2.zero;
            overlayRT.anchorMax = Vector2.one;
            overlayRT.offsetMin = Vector2.zero;
            overlayRT.offsetMax = Vector2.zero;

            Image overlayImg = GetOrAdd<Image>(overlay);
            overlayImg.color = OVERLAY_COLOR;

            Button overlayBtn = GetOrAdd<Button>(overlay);
            overlayBtn.targetGraphic = overlayImg;
            // Este botón cierra el panel al hacer clic fuera

            // Panel Container
            GameObject container = FindOrCreate(panel.transform, "PanelContainer");
            RectTransform contRT = GetOrAdd<RectTransform>(container);
            contRT.anchorMin = new Vector2(0.05f, 0.1f);
            contRT.anchorMax = new Vector2(0.95f, 0.9f);
            contRT.offsetMin = Vector2.zero;
            contRT.offsetMax = Vector2.zero;

            Image contBg = GetOrAdd<Image>(container);
            contBg.color = PANEL_BG;

            Outline contOutline = GetOrAdd<Outline>(container);
            contOutline.effectColor = GOLD;
            contOutline.effectDistance = new Vector2(2, 2);

            // Header
            GameObject header = FindOrCreate(container.transform, "Header");
            RectTransform headerRT = GetOrAdd<RectTransform>(header);
            headerRT.anchorMin = new Vector2(0, 1);
            headerRT.anchorMax = new Vector2(1, 1);
            headerRT.pivot = new Vector2(0.5f, 1);
            headerRT.anchoredPosition = Vector2.zero;
            headerRT.sizeDelta = new Vector2(0, 70);

            Image headerBg = GetOrAdd<Image>(header);
            headerBg.color = new Color(0.2f, 0.15f, 0.05f, 1f);

            // Header Title
            GameObject headerTitle = FindOrCreate(header.transform, "TitleText");
            RectTransform htRT = GetOrAdd<RectTransform>(headerTitle);
            htRT.anchorMin = Vector2.zero;
            htRT.anchorMax = Vector2.one;
            htRT.offsetMin = new Vector2(20, 0);
            htRT.offsetMax = new Vector2(-60, 0);

            TextMeshProUGUI htTMP = GetOrAdd<TextMeshProUGUI>(headerTitle);
            htTMP.text = "TIENDA PREMIUM";
            htTMP.fontSize = 28;
            htTMP.color = GOLD;
            htTMP.fontStyle = FontStyles.Bold;
            htTMP.alignment = TextAlignmentOptions.Left;

            // Close Button
            GameObject closeBtn = FindOrCreate(header.transform, "CloseButton");
            RectTransform closeRT = GetOrAdd<RectTransform>(closeBtn);
            closeRT.anchorMin = new Vector2(1, 0.5f);
            closeRT.anchorMax = new Vector2(1, 0.5f);
            closeRT.pivot = new Vector2(1, 0.5f);
            closeRT.anchoredPosition = new Vector2(-15, 0);
            closeRT.sizeDelta = new Vector2(50, 50);

            Image closeBg = GetOrAdd<Image>(closeBtn);
            closeBg.color = new Color(1f, 1f, 1f, 0.1f);

            Button closeButton = GetOrAdd<Button>(closeBtn);
            closeButton.targetGraphic = closeBg;

            GameObject closeX = FindOrCreate(closeBtn.transform, "X");
            RectTransform closeXRT = GetOrAdd<RectTransform>(closeX);
            closeXRT.anchorMin = Vector2.zero;
            closeXRT.anchorMax = Vector2.one;
            closeXRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI closeTMP = GetOrAdd<TextMeshProUGUI>(closeX);
            closeTMP.text = "X";
            closeTMP.fontSize = 24;
            closeTMP.color = TEXT_PRIMARY;
            closeTMP.fontStyle = FontStyles.Bold;
            closeTMP.alignment = TextAlignmentOptions.Center;

            // Content ScrollView
            GameObject scrollView = FindOrCreate(container.transform, "ScrollView");
            RectTransform scrollRT = GetOrAdd<RectTransform>(scrollView);
            scrollRT.anchorMin = new Vector2(0, 0.08f);
            scrollRT.anchorMax = new Vector2(1, 1);
            scrollRT.offsetMin = new Vector2(10, 0);
            scrollRT.offsetMax = new Vector2(-10, -70);

            ScrollRect scroll = GetOrAdd<ScrollRect>(scrollView);
            scroll.horizontal = false;
            scroll.vertical = true;

            // Viewport
            GameObject viewport = FindOrCreate(scrollView.transform, "Viewport");
            RectTransform vpRT = GetOrAdd<RectTransform>(viewport);
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = Vector2.zero;
            vpRT.offsetMax = Vector2.zero;

            RectMask2D vpMask = GetOrAdd<RectMask2D>(viewport);

            // Content
            GameObject content = FindOrCreate(viewport.transform, "Content");
            RectTransform contentRT = GetOrAdd<RectTransform>(content);
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.anchoredPosition = Vector2.zero;
            contentRT.sizeDelta = new Vector2(0, 0);

            VerticalLayoutGroup vlg = GetOrAdd<VerticalLayoutGroup>(content);
            vlg.spacing = 15;
            vlg.padding = new RectOffset(10, 10, 15, 15);
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            ContentSizeFitter csf = GetOrAdd<ContentSizeFitter>(content);
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = vpRT;
            scroll.content = contentRT;

            // Placeholder text
            GameObject placeholder = FindOrCreate(content.transform, "PlaceholderText");
            LayoutElement plLE = GetOrAdd<LayoutElement>(placeholder);
            plLE.preferredHeight = 200;

            TextMeshProUGUI plTMP = GetOrAdd<TextMeshProUGUI>(placeholder);
            plTMP.text = "Contenido de la tienda\n(Temas, paquetes, etc.)\n\nSe definirá después";
            plTMP.fontSize = 18;
            plTMP.color = TEXT_SECONDARY;
            plTMP.alignment = TextAlignmentOptions.Center;

            // Footer - Restore Purchases
            GameObject footer = FindOrCreate(container.transform, "Footer");
            RectTransform footerRT = GetOrAdd<RectTransform>(footer);
            footerRT.anchorMin = new Vector2(0, 0);
            footerRT.anchorMax = new Vector2(1, 0);
            footerRT.pivot = new Vector2(0.5f, 0);
            footerRT.anchoredPosition = Vector2.zero;
            footerRT.sizeDelta = new Vector2(0, 50);

            GameObject restoreBtn = FindOrCreate(footer.transform, "RestoreButton");
            RectTransform restoreRT = GetOrAdd<RectTransform>(restoreBtn);
            restoreRT.anchorMin = new Vector2(0.5f, 0.5f);
            restoreRT.anchorMax = new Vector2(0.5f, 0.5f);
            restoreRT.sizeDelta = new Vector2(250, 40);

            Image restoreBg = GetOrAdd<Image>(restoreBtn);
            restoreBg.color = Color.clear;

            Button restoreButton = GetOrAdd<Button>(restoreBtn);
            restoreButton.targetGraphic = restoreBg;

            // Texto en objeto hijo
            GameObject restoreTextObj = FindOrCreate(restoreBtn.transform, "Text");
            RectTransform restoreTextRT = GetOrAdd<RectTransform>(restoreTextObj);
            restoreTextRT.anchorMin = Vector2.zero;
            restoreTextRT.anchorMax = Vector2.one;
            restoreTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI restoreTMP = GetOrAdd<TextMeshProUGUI>(restoreTextObj);
            restoreTMP.text = "Restaurar Compras";
            restoreTMP.fontSize = 16;
            restoreTMP.color = TEXT_SECONDARY;
            restoreTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[MainMenuUI] Premium Panel creado");
        }

        #endregion

        #region Notifications Panel

        private static void CreateNotificationsPanel()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            GameObject panel = FindOrCreate(canvas.transform, "NotificationsPanel");
            panel.SetActive(false); // Oculto por defecto

            RectTransform panelRT = GetOrAdd<RectTransform>(panel);
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            // Dark Overlay
            GameObject overlay = FindOrCreate(panel.transform, "DarkOverlay");
            RectTransform overlayRT = GetOrAdd<RectTransform>(overlay);
            overlayRT.anchorMin = Vector2.zero;
            overlayRT.anchorMax = Vector2.one;
            overlayRT.offsetMin = Vector2.zero;
            overlayRT.offsetMax = Vector2.zero;

            Image overlayImg = GetOrAdd<Image>(overlay);
            overlayImg.color = OVERLAY_COLOR;

            Button overlayBtn = GetOrAdd<Button>(overlay);
            overlayBtn.targetGraphic = overlayImg;

            // Panel Container (derecha, estilo slide-in)
            GameObject container = FindOrCreate(panel.transform, "PanelContainer");
            RectTransform contRT = GetOrAdd<RectTransform>(container);
            contRT.anchorMin = new Vector2(0.1f, 0.15f);
            contRT.anchorMax = new Vector2(0.9f, 0.85f);
            contRT.offsetMin = Vector2.zero;
            contRT.offsetMax = Vector2.zero;

            Image contBg = GetOrAdd<Image>(container);
            contBg.color = PANEL_BG;

            Outline contOutline = GetOrAdd<Outline>(container);
            contOutline.effectColor = CYAN_DARK;
            contOutline.effectDistance = new Vector2(2, 2);

            // Header
            GameObject header = FindOrCreate(container.transform, "Header");
            RectTransform headerRT = GetOrAdd<RectTransform>(header);
            headerRT.anchorMin = new Vector2(0, 1);
            headerRT.anchorMax = new Vector2(1, 1);
            headerRT.pivot = new Vector2(0.5f, 1);
            headerRT.anchoredPosition = Vector2.zero;
            headerRT.sizeDelta = new Vector2(0, 70);

            Image headerBg = GetOrAdd<Image>(header);
            headerBg.color = HEADER_BG;

            // Header line
            GameObject headerLine = FindOrCreate(header.transform, "Line");
            RectTransform lineRT = GetOrAdd<RectTransform>(headerLine);
            lineRT.anchorMin = new Vector2(0, 0);
            lineRT.anchorMax = new Vector2(1, 0);
            lineRT.sizeDelta = new Vector2(0, 2);

            Image lineImg = GetOrAdd<Image>(headerLine);
            lineImg.color = CYAN_NEON;

            // Header Title
            GameObject headerTitle = FindOrCreate(header.transform, "TitleText");
            RectTransform htRT = GetOrAdd<RectTransform>(headerTitle);
            htRT.anchorMin = Vector2.zero;
            htRT.anchorMax = Vector2.one;
            htRT.offsetMin = new Vector2(20, 0);
            htRT.offsetMax = new Vector2(-60, 0);

            TextMeshProUGUI htTMP = GetOrAdd<TextMeshProUGUI>(headerTitle);
            htTMP.text = "NOTIFICACIONES";
            htTMP.fontSize = 24;
            htTMP.color = CYAN_NEON;
            htTMP.fontStyle = FontStyles.Bold;
            htTMP.alignment = TextAlignmentOptions.Left;

            // Close Button
            GameObject closeBtn = FindOrCreate(header.transform, "CloseButton");
            RectTransform closeRT = GetOrAdd<RectTransform>(closeBtn);
            closeRT.anchorMin = new Vector2(1, 0.5f);
            closeRT.anchorMax = new Vector2(1, 0.5f);
            closeRT.pivot = new Vector2(1, 0.5f);
            closeRT.anchoredPosition = new Vector2(-15, 0);
            closeRT.sizeDelta = new Vector2(50, 50);

            Image closeBg = GetOrAdd<Image>(closeBtn);
            closeBg.color = new Color(1f, 1f, 1f, 0.1f);

            Button closeButton = GetOrAdd<Button>(closeBtn);
            closeButton.targetGraphic = closeBg;

            GameObject closeX = FindOrCreate(closeBtn.transform, "X");
            RectTransform closeXRT = GetOrAdd<RectTransform>(closeX);
            closeXRT.anchorMin = Vector2.zero;
            closeXRT.anchorMax = Vector2.one;
            closeXRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI closeTMP = GetOrAdd<TextMeshProUGUI>(closeX);
            closeTMP.text = "X";
            closeTMP.fontSize = 24;
            closeTMP.color = TEXT_PRIMARY;
            closeTMP.fontStyle = FontStyles.Bold;
            closeTMP.alignment = TextAlignmentOptions.Center;

            // Content ScrollView
            GameObject scrollView = FindOrCreate(container.transform, "ScrollView");
            RectTransform scrollRT = GetOrAdd<RectTransform>(scrollView);
            scrollRT.anchorMin = Vector2.zero;
            scrollRT.anchorMax = Vector2.one;
            scrollRT.offsetMin = new Vector2(10, 10);
            scrollRT.offsetMax = new Vector2(-10, -70);

            ScrollRect scroll = GetOrAdd<ScrollRect>(scrollView);
            scroll.horizontal = false;
            scroll.vertical = true;

            // Viewport
            GameObject viewport = FindOrCreate(scrollView.transform, "Viewport");
            RectTransform vpRT = GetOrAdd<RectTransform>(viewport);
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = Vector2.zero;
            vpRT.offsetMax = Vector2.zero;

            RectMask2D vpMask = GetOrAdd<RectMask2D>(viewport);

            // Content
            GameObject content = FindOrCreate(viewport.transform, "Content");
            RectTransform contentRT = GetOrAdd<RectTransform>(content);
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.anchoredPosition = Vector2.zero;
            contentRT.sizeDelta = new Vector2(0, 0);

            VerticalLayoutGroup vlg = GetOrAdd<VerticalLayoutGroup>(content);
            vlg.spacing = 10;
            vlg.padding = new RectOffset(5, 5, 10, 10);
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            ContentSizeFitter csf = GetOrAdd<ContentSizeFitter>(content);
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = vpRT;
            scroll.content = contentRT;

            // Empty State
            GameObject emptyState = FindOrCreate(content.transform, "EmptyState");
            LayoutElement emptyLE = GetOrAdd<LayoutElement>(emptyState);
            emptyLE.preferredHeight = 150;

            TextMeshProUGUI emptyTMP = GetOrAdd<TextMeshProUGUI>(emptyState);
            emptyTMP.text = "No hay notificaciones\n\nLos torneos concluidos y\nnoticias aparecerán aquí";
            emptyTMP.fontSize = 18;
            emptyTMP.color = TEXT_SECONDARY;
            emptyTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[MainMenuUI] Notifications Panel creado");
        }

        #endregion

        #region Helpers

        private static GameObject FindOrCreate(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null) return existing.gameObject;

            GameObject newObj = new GameObject(name);
            newObj.transform.SetParent(parent, false);
            return newObj;
        }

        private static T GetOrAdd<T>(GameObject obj) where T : Component
        {
            T component = obj.GetComponent<T>();
            if (component == null) component = obj.AddComponent<T>();
            return component;
        }

        #endregion
    }
}
