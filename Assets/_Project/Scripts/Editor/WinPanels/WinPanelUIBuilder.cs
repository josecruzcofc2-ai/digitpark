using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para crear los Win Panels globales como prefabs
    /// - WinPanel_Normal: Para juegos de práctica (estilo neón cyan)
    /// - WinPanel_RealMoney: Para torneos/1v1 con dinero (estilo dorado premium)
    /// </summary>
    public class WinPanelUIBuilder : EditorWindow
    {
        // Colores Neón (Normal)
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color GREEN_NEON = new Color(0.3f, 1f, 0.5f, 1f);
        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 0.98f);
        private static readonly Color PANEL_BG = new Color(0.05f, 0.1f, 0.15f, 0.95f);

        // Colores Dorados (Real Money)
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GOLD_DARK = new Color(0.7f, 0.55f, 0f, 1f);
        private static readonly Color GOLD_LIGHT = new Color(1f, 0.95f, 0.6f, 1f);
        private static readonly Color PREMIUM_BG = new Color(0.08f, 0.06f, 0.02f, 0.98f);
        private static readonly Color PREMIUM_PANEL = new Color(0.15f, 0.12f, 0.05f, 0.95f);
        private static readonly Color LOSE_RED = new Color(1f, 0.3f, 0.3f, 1f);

        [MenuItem("DigitPark/Create Win Panels")]
        public static void ShowWindow()
        {
            GetWindow<WinPanelUIBuilder>("Win Panel Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Win Panel Builder", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Crea los Win/Lose Panels globales:\n\n" +
                "SIN DINERO (Práctica):\n" +
                "  1. WinPanel_Normal - Victoria práctica (cyan)\n" +
                "  2. LosePanel_Normal - Tiempo agotado/derrota práctica (naranja)\n\n" +
                "CON DINERO REAL:\n" +
                "  3. WinPanel_RealMoney - Victoria (dorado)\n" +
                "  4. LosePanel_RealMoney - Derrota (magenta)",
                MessageType.Info);

            GUILayout.Space(15);
            GUILayout.Label("Paneles Sin Dinero (Práctica)", EditorStyles.boldLabel);

            if (GUILayout.Button("Crear WinPanel Normal (Cyan)", GUILayout.Height(35)))
            {
                CreateNormalWinPanel();
            }

            if (GUILayout.Button("Crear LosePanel Normal (Naranja)", GUILayout.Height(35)))
            {
                CreateNormalLosePanel();
            }

            GUILayout.Space(10);
            GUILayout.Label("Paneles Con Dinero Real", EditorStyles.boldLabel);

            if (GUILayout.Button("Crear WinPanel RealMoney (Dorado)", GUILayout.Height(35)))
            {
                CreateRealMoneyWinPanel();
            }

            if (GUILayout.Button("Crear LosePanel RealMoney (Magenta)", GUILayout.Height(35)))
            {
                CreateLosePanel();
            }

            GUILayout.Space(20);

            if (GUILayout.Button("Crear TODOS los Paneles (4)", GUILayout.Height(50)))
            {
                CreateNormalWinPanel();
                CreateNormalLosePanel();
                CreateRealMoneyWinPanel();
                CreateLosePanel();
            }
        }

        // ========================================================================
        // WIN PANEL NORMAL (NEÓN) - Panel más grande con iconos como Image
        // ========================================================================
        private static void CreateNormalWinPanel()
        {
            // Crear raíz
            GameObject panel = new GameObject("WinPanel_Normal");
            RectTransform panelRt = panel.AddComponent<RectTransform>();
            SetFullStretch(panelRt);

            // Overlay oscuro
            Image overlay = panel.AddComponent<Image>();
            overlay.color = new Color(0, 0, 0, 0.9f);

            CanvasGroup cg = panel.AddComponent<CanvasGroup>();

            // Content container - MÁS GRANDE (600x550)
            GameObject content = CreateChild(panel.transform, "Content");
            SetupRectTransform(content, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(600, 550));

            // Panel background con efecto 3D
            CreatePanel3DEffect(content.transform, PANEL_BG, CYAN_NEON);

            // Face del panel (donde va el contenido)
            GameObject face = content.transform.Find("Face").gameObject;

            // === ICONO DE ÉXITO (Image component para sprite) ===
            GameObject iconContainer = CreateChild(face.transform, "IconContainer");
            SetupRectTransform(iconContainer, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -70), new Vector2(100, 100));
            Image iconBg = iconContainer.AddComponent<Image>();
            iconBg.color = new Color(0.1f, 0.2f, 0.15f, 0.9f);
            AddGlow(iconContainer, GREEN_NEON, 3);

            // Icono como Image (arrastrar sprite aquí)
            GameObject iconImage = CreateChild(iconContainer.transform, "Icon");
            SetupRectTransform(iconImage, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(70, 70));
            Image iconImg = iconImage.AddComponent<Image>();
            iconImg.color = GREEN_NEON;
            iconImg.preserveAspect = true;
            // El usuario arrastrará el sprite de checkmark aquí

            // === TÍTULO ===
            GameObject title = CreateChild(face.transform, "Title");
            SetupRectTransform(title, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -170), new Vector2(0, 70));
            TextMeshProUGUI titleTmp = AddText(title, "¡COMPLETADO!", 52, GREEN_NEON, FontStyles.Bold);
            AddGlow(title, GREEN_NEON, 3);

            // === STATS CONTAINER - más espacio ===
            GameObject statsContainer = CreateChild(face.transform, "StatsContainer");
            SetupRectTransform(statsContainer, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -30), new Vector2(450, 160));

            // Tiempo
            CreateStatRow(statsContainer.transform, "TimeRow", "TIEMPO", "00.00s", CYAN_NEON, 25);
            // Errores
            CreateStatRow(statsContainer.transform, "ErrorsRow", "ERRORES", "0", CYAN_NEON, -35);

            // === BOTONES - MÁS GRANDES ===
            GameObject buttonsContainer = CreateChild(face.transform, "ButtonsContainer");
            SetupRectTransform(buttonsContainer, new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 70), new Vector2(-40, 120));

            HorizontalLayoutGroup btnLayout = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
            btnLayout.childAlignment = TextAnchor.MiddleCenter;
            btnLayout.spacing = 25;
            btnLayout.childForceExpandWidth = false;
            btnLayout.childControlWidth = false;

            // Botón Jugar de Nuevo - MÁS GRANDE
            CreateButton3D(buttonsContainer.transform, "PlayAgainButton", "JUGAR DE NUEVO", CYAN_NEON, 240, 70);

            // Botón Aceptar - MÁS GRANDE
            CreateButton3D(buttonsContainer.transform, "AcceptButton", "ACEPTAR", GREEN_NEON, 180, 70);

            // Agregar WinPanelController
            WinPanelController controller = panel.AddComponent<WinPanelController>();
            AssignNormalReferences(controller, panel, content);

            // Guardar como prefab
            SaveAsPrefab(panel, "WinPanel_Normal");
        }

        // ========================================================================
        // LOSE PANEL NORMAL (NARANJA NEÓN) - Para práctica/tiempo agotado
        // ========================================================================
        private static void CreateNormalLosePanel()
        {
            // Colores neón naranja para derrota normal (sin dinero)
            Color ORANGE_NEON = new Color(1f, 0.6f, 0.2f, 1f);        // Naranja neón
            Color ORANGE_DARK = new Color(0.7f, 0.35f, 0.1f, 1f);     // Naranja oscuro
            Color ORANGE_LIGHT = new Color(1f, 0.75f, 0.4f, 1f);      // Naranja claro

            // Crear raíz
            GameObject panel = new GameObject("LosePanel_Normal");
            RectTransform panelRt = panel.AddComponent<RectTransform>();
            SetFullStretch(panelRt);

            // Overlay oscuro
            Image overlay = panel.AddComponent<Image>();
            overlay.color = new Color(0.03f, 0.02f, 0.01f, 0.92f);

            CanvasGroup cg = panel.AddComponent<CanvasGroup>();

            // Content container
            GameObject content = CreateChild(panel.transform, "Content");
            SetupRectTransform(content, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(580, 520));

            // Panel background con efecto 3D naranja
            CreatePanel3DEffect(content.transform, PANEL_BG, ORANGE_NEON);

            GameObject face = content.transform.Find("Face").gameObject;

            // === ICONO (reloj/X - Image component) ===
            GameObject iconContainer = CreateChild(face.transform, "IconContainer");
            SetupRectTransform(iconContainer, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -65), new Vector2(95, 95));
            Image iconBg = iconContainer.AddComponent<Image>();
            iconBg.color = new Color(0.15f, 0.1f, 0.05f, 0.9f);
            AddGlow(iconContainer, ORANGE_NEON, 3);

            GameObject icon = CreateChild(iconContainer.transform, "Icon");
            SetupRectTransform(icon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(65, 65));
            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = ORANGE_LIGHT;
            iconImg.preserveAspect = true;
            // Arrastrar icono de reloj, X, etc.

            // === TÍTULO ===
            GameObject title = CreateChild(face.transform, "Title");
            SetupRectTransform(title, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -160), new Vector2(0, 65));
            TextMeshProUGUI titleTmp = AddText(title, "TIEMPO AGOTADO", 46, ORANGE_LIGHT, FontStyles.Bold);
            AddGlow(title, ORANGE_NEON, 3);

            // === SUBTÍTULO motivacional ===
            GameObject subtitle = CreateChild(face.transform, "Subtitle");
            SetupRectTransform(subtitle, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -210), new Vector2(0, 35));
            AddText(subtitle, "¡Inténtalo de nuevo!", 22, new Color(0.8f, 0.65f, 0.5f), FontStyles.Italic);

            // === STATS CONTAINER ===
            GameObject statsContainer = CreateChild(face.transform, "StatsContainer");
            SetupRectTransform(statsContainer, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -30), new Vector2(420, 140));

            // Tiempo alcanzado
            CreateStatRow(statsContainer.transform, "TimeRow", "TIEMPO", "00.00s", ORANGE_LIGHT, 20);
            // Errores
            CreateStatRow(statsContainer.transform, "ErrorsRow", "ERRORES", "0", ORANGE_LIGHT, -30);

            // === MEJOR TIEMPO (opcional) ===
            GameObject bestTimeContainer = CreateChild(face.transform, "BestTimeContainer");
            SetupRectTransform(bestTimeContainer, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -110), new Vector2(300, 40));

            Image bestBg = bestTimeContainer.AddComponent<Image>();
            bestBg.color = new Color(0.1f, 0.08f, 0.05f, 0.6f);

            GameObject bestTimeText = CreateChild(bestTimeContainer.transform, "BestTimeText");
            SetupRectTransform(bestTimeText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            AddText(bestTimeText, "Mejor: --.-s", 20, new Color(0.6f, 0.55f, 0.45f), FontStyles.Normal);

            // === BOTONES ===
            GameObject buttonsContainer = CreateChild(face.transform, "ButtonsContainer");
            SetupRectTransform(buttonsContainer, new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 65), new Vector2(-40, 110));

            HorizontalLayoutGroup btnLayout = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
            btnLayout.childAlignment = TextAnchor.MiddleCenter;
            btnLayout.spacing = 25;
            btnLayout.childForceExpandWidth = false;
            btnLayout.childControlWidth = false;

            // Botón Reintentar (principal)
            CreateButton3D(buttonsContainer.transform, "PlayAgainButton", "REINTENTAR", ORANGE_NEON, 220, 70);

            // Botón Salir
            CreateButton3D(buttonsContainer.transform, "AcceptButton", "SALIR", new Color(0.45f, 0.4f, 0.35f), 150, 70);

            // Agregar WinPanelController
            WinPanelController controller = panel.AddComponent<WinPanelController>();
            AssignNormalLoseReferences(controller, panel, content);

            // Guardar como prefab
            SaveAsPrefab(panel, "LosePanel_Normal");
        }

        private static void AssignNormalLoseReferences(WinPanelController controller, GameObject panel, GameObject content)
        {
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("isRealMoneyPanel").boolValue = false;
            so.FindProperty("canvasGroup").objectReferenceValue = panel.GetComponent<CanvasGroup>();
            so.FindProperty("content").objectReferenceValue = content;

            Transform face = content.transform.Find("Face");
            if (face != null)
            {
                so.FindProperty("titleText").objectReferenceValue =
                    face.Find("Title")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("timeText").objectReferenceValue =
                    face.Find("StatsContainer/TimeRow/Value")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("errorsText").objectReferenceValue =
                    face.Find("StatsContainer/ErrorsRow/Value")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("acceptButton").objectReferenceValue =
                    face.Find("ButtonsContainer/AcceptButton")?.GetComponent<Button>();
                so.FindProperty("playAgainButton").objectReferenceValue =
                    face.Find("ButtonsContainer/PlayAgainButton")?.GetComponent<Button>();
            }
            so.ApplyModifiedProperties();
        }

        // ========================================================================
        // WIN PANEL REAL MONEY (DORADO PREMIUM) - Panel más grande y espacioso
        // ========================================================================
        private static void CreateRealMoneyWinPanel()
        {
            // Crear raíz
            GameObject panel = new GameObject("WinPanel_RealMoney");
            RectTransform panelRt = panel.AddComponent<RectTransform>();
            SetFullStretch(panelRt);

            // Overlay oscuro premium
            Image overlay = panel.AddComponent<Image>();
            overlay.color = new Color(0.02f, 0.01f, 0f, 0.95f);

            CanvasGroup cg = panel.AddComponent<CanvasGroup>();

            // Content container - MÁS GRANDE (650x720)
            GameObject content = CreateChild(panel.transform, "Content");
            SetupRectTransform(content, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(650, 720));

            // Panel background dorado con efecto 3D premium
            CreatePremiumPanel3DEffect(content.transform);

            GameObject face = content.transform.Find("Face").gameObject;

            // === CORONA/TROFEO ICON - MÁS GRANDE (arriba del título) ===
            GameObject crownContainer = CreateChild(face.transform, "CrownContainer");
            SetupRectTransform(crownContainer, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -60), new Vector2(120, 120));

            GameObject crownIcon = CreateChild(crownContainer.transform, "CrownIcon");
            SetupRectTransform(crownIcon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(100, 100));
            Image crownImg = crownIcon.AddComponent<Image>();
            crownImg.color = GOLD;
            crownImg.preserveAspect = true;
            // El usuario arrastrará el sprite de corona/trofeo aquí

            // === TÍTULO CON GLOW DORADO ===
            GameObject title = CreateChild(face.transform, "Title");
            SetupRectTransform(title, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -175), new Vector2(0, 70));
            TextMeshProUGUI titleTmp = AddText(title, "¡GANASTE!", 56, GOLD, FontStyles.Bold);
            AddGlow(title, GOLD, 4);

            // === DINERO GANADO (elemento principal) ===
            GameObject moneyContainer = CreateChild(face.transform, "MoneyContainer");
            SetupRectTransform(moneyContainer, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -265), new Vector2(400, 100));

            Image moneyBg = moneyContainer.AddComponent<Image>();
            moneyBg.color = new Color(0.2f, 0.15f, 0.02f, 0.8f);
            AddGlow(moneyContainer, GOLD, 3);

            GameObject moneyText = CreateChild(moneyContainer.transform, "MoneyText");
            SetupRectTransform(moneyText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI moneyTmp = AddText(moneyText, "+$10.00", 60, GOLD_LIGHT, FontStyles.Bold);

            // === APUESTA ORIGINAL - MÁS ESPACIO ===
            GameObject wagerText = CreateChild(face.transform, "WagerText");
            SetupRectTransform(wagerText, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -345), new Vector2(350, 40));
            AddText(wagerText, "Apuesta: $5.00", 24, new Color(0.8f, 0.7f, 0.5f), FontStyles.Normal);

            // === VS COMPARISON - MÁS GRANDE ===
            GameObject vsContainer = CreateChild(face.transform, "VSContainer");
            SetupRectTransform(vsContainer, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -50), new Vector2(520, 120));

            // Fondo VS
            Image vsBg = vsContainer.AddComponent<Image>();
            vsBg.color = new Color(0.1f, 0.08f, 0.02f, 0.7f);

            // Player score (izquierda)
            GameObject playerScore = CreateChild(vsContainer.transform, "PlayerScore");
            SetupRectTransform(playerScore, new Vector2(0, 0), new Vector2(0.45f, 1),
                Vector2.zero, Vector2.zero);

            GameObject playerLabel = CreateChild(playerScore.transform, "Label");
            SetupRectTransform(playerLabel, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -15), new Vector2(180, 35));
            AddText(playerLabel, "TÚ", 22, GOLD, FontStyles.Bold);

            GameObject playerValue = CreateChild(playerScore.transform, "Value");
            SetupRectTransform(playerValue, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -5), new Vector2(180, 50));
            AddText(playerValue, "12.45s", 36, Color.white, FontStyles.Bold);

            // VS text
            GameObject vsText = CreateChild(vsContainer.transform, "VSText");
            SetupRectTransform(vsText, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(60, 60));
            AddText(vsText, "VS", 28, GOLD_DARK, FontStyles.Bold);

            // Opponent score (derecha)
            GameObject opponentScore = CreateChild(vsContainer.transform, "OpponentScore");
            SetupRectTransform(opponentScore, new Vector2(0.55f, 0), new Vector2(1, 1),
                Vector2.zero, Vector2.zero);

            GameObject oppLabel = CreateChild(opponentScore.transform, "Label");
            SetupRectTransform(oppLabel, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -15), new Vector2(180, 35));
            AddText(oppLabel, "OPONENTE", 20, new Color(0.7f, 0.6f, 0.5f), FontStyles.Bold);

            GameObject oppValue = CreateChild(opponentScore.transform, "Value");
            SetupRectTransform(oppValue, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -5), new Vector2(180, 50));
            AddText(oppValue, "15.32s", 36, new Color(0.6f, 0.6f, 0.6f), FontStyles.Normal);

            // === STATS (Tiempo y Errores) - MÁS ESPACIO ===
            GameObject statsContainer = CreateChild(face.transform, "StatsContainer");
            SetupRectTransform(statsContainer, new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 160), new Vector2(450, 100));

            CreateStatRow(statsContainer.transform, "TimeRow", "Tu tiempo", "00.00s", GOLD_LIGHT, 25);
            CreateStatRow(statsContainer.transform, "ErrorsRow", "Errores", "0", GOLD_LIGHT, -25);

            // === BOTÓN RECLAMAR - MÁS GRANDE ===
            GameObject buttonContainer = CreateChild(face.transform, "ButtonContainer");
            SetupRectTransform(buttonContainer, new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 55), new Vector2(350, 85));

            CreatePremiumButton(buttonContainer.transform, "AcceptButton", "RECLAMAR", GOLD, 320, 75);

            // === PARTÍCULAS ===
            GameObject particles = CreateChild(panel.transform, "ParticleEffects");
            SetupRectTransform(particles, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            UISparkleEffect sparkle = particles.AddComponent<UISparkleEffect>();

            // Agregar WinPanelController
            WinPanelController controller = panel.AddComponent<WinPanelController>();
            AssignRealMoneyReferences(controller, panel, content);

            // Guardar como prefab
            SaveAsPrefab(panel, "WinPanel_RealMoney");
        }

        // ========================================================================
        // LOSE PANEL REAL MONEY (ESTILO NEÓN PÚRPURA/MAGENTA - Derrota con estilo)
        // ========================================================================
        private static void CreateLosePanel()
        {
            // Colores neón para derrota - Púrpura/Magenta apagado pero con glow
            Color LOSE_NEON = new Color(0.8f, 0.3f, 0.6f, 1f);        // Magenta neón apagado
            Color LOSE_NEON_DARK = new Color(0.5f, 0.15f, 0.35f, 1f); // Magenta oscuro
            Color LOSE_NEON_LIGHT = new Color(1f, 0.5f, 0.8f, 1f);    // Magenta claro
            Color LOSE_RED = new Color(1f, 0.4f, 0.4f, 1f);           // Rojo para dinero perdido
            Color WINNER_GREEN = new Color(0.3f, 1f, 0.5f, 1f);       // Verde neón para ganador

            // Crear raíz
            GameObject panel = new GameObject("LosePanel_RealMoney");
            RectTransform panelRt = panel.AddComponent<RectTransform>();
            SetFullStretch(panelRt);

            // Overlay oscuro con tinte púrpura
            Image overlay = panel.AddComponent<Image>();
            overlay.color = new Color(0.03f, 0.01f, 0.04f, 0.95f);

            CanvasGroup cg = panel.AddComponent<CanvasGroup>();

            // Content container
            GameObject content = CreateChild(panel.transform, "Content");
            SetupRectTransform(content, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(620, 700));

            // Panel background con efecto 3D neón púrpura
            CreateLosePanel3DEffect(content.transform, LOSE_NEON);

            GameObject face = content.transform.Find("Face").gameObject;

            // === ICONO (X o escudo roto - Image component) ===
            GameObject iconContainer = CreateChild(face.transform, "IconContainer");
            SetupRectTransform(iconContainer, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -60), new Vector2(110, 110));
            Image iconBg = iconContainer.AddComponent<Image>();
            iconBg.color = new Color(0.15f, 0.05f, 0.1f, 0.9f);
            AddGlow(iconContainer, LOSE_NEON, 3);

            // Agregar glow pulsante al icono
            GridGlowPulse iconPulse = iconContainer.AddComponent<GridGlowPulse>();

            GameObject icon = CreateChild(iconContainer.transform, "Icon");
            SetupRectTransform(icon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(80, 80));
            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = LOSE_NEON_LIGHT;
            iconImg.preserveAspect = true;
            // Arrastrar icono de X, escudo roto, etc.

            // === TÍTULO NEÓN ===
            GameObject title = CreateChild(face.transform, "Title");
            SetupRectTransform(title, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -170), new Vector2(0, 70));
            TextMeshProUGUI titleTmp = AddText(title, "DERROTA", 50, LOSE_NEON_LIGHT, FontStyles.Bold);
            AddGlow(title, LOSE_NEON, 4);

            // === SUBTÍTULO motivacional ===
            GameObject subtitle = CreateChild(face.transform, "Subtitle");
            SetupRectTransform(subtitle, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -225), new Vector2(0, 40));
            AddText(subtitle, "La próxima será tuya", 24, new Color(0.7f, 0.5f, 0.6f), FontStyles.Italic);

            // === DINERO PERDIDO (destacado pero no agresivo) ===
            GameObject moneyContainer = CreateChild(face.transform, "MoneyContainer");
            SetupRectTransform(moneyContainer, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -295), new Vector2(380, 90));

            Image moneyBg = moneyContainer.AddComponent<Image>();
            moneyBg.color = new Color(0.12f, 0.04f, 0.06f, 0.8f);
            AddGlow(moneyContainer, LOSE_RED, 2);

            GameObject moneyText = CreateChild(moneyContainer.transform, "MoneyText");
            SetupRectTransform(moneyText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI moneyTmp = AddText(moneyText, "-$5.00", 52, LOSE_RED, FontStyles.Bold);

            // === VS COMPARISON con estilo neón ===
            GameObject vsContainer = CreateChild(face.transform, "VSContainer");
            SetupRectTransform(vsContainer, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -70), new Vector2(520, 120));

            Image vsBg = vsContainer.AddComponent<Image>();
            vsBg.color = new Color(0.06f, 0.03f, 0.06f, 0.8f);

            Outline vsOutline = vsContainer.AddComponent<Outline>();
            vsOutline.effectColor = new Color(LOSE_NEON.r, LOSE_NEON.g, LOSE_NEON.b, 0.4f);
            vsOutline.effectDistance = new Vector2(2, -2);

            // Player score (izquierda) - PERDEDOR
            GameObject playerScore = CreateChild(vsContainer.transform, "PlayerScore");
            SetupRectTransform(playerScore, new Vector2(0, 0), new Vector2(0.45f, 1),
                Vector2.zero, Vector2.zero);

            GameObject playerLabel = CreateChild(playerScore.transform, "Label");
            SetupRectTransform(playerLabel, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -15), new Vector2(180, 35));
            AddText(playerLabel, "TÚ", 22, LOSE_NEON_LIGHT, FontStyles.Bold);

            GameObject playerValue = CreateChild(playerScore.transform, "Value");
            SetupRectTransform(playerValue, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -5), new Vector2(180, 50));
            AddText(playerValue, "15.32s", 36, new Color(0.8f, 0.6f, 0.7f), FontStyles.Normal);

            // VS text con glow
            GameObject vsText = CreateChild(vsContainer.transform, "VSText");
            SetupRectTransform(vsText, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(60, 60));
            TextMeshProUGUI vsTmp = AddText(vsText, "VS", 28, LOSE_NEON, FontStyles.Bold);

            // Opponent score (derecha) - GANADOR en verde neón
            GameObject opponentScore = CreateChild(vsContainer.transform, "OpponentScore");
            SetupRectTransform(opponentScore, new Vector2(0.55f, 0), new Vector2(1, 1),
                Vector2.zero, Vector2.zero);

            GameObject oppLabel = CreateChild(opponentScore.transform, "Label");
            SetupRectTransform(oppLabel, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -15), new Vector2(180, 35));
            AddText(oppLabel, "GANADOR", 20, WINNER_GREEN, FontStyles.Bold);

            GameObject oppValue = CreateChild(opponentScore.transform, "Value");
            SetupRectTransform(oppValue, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -5), new Vector2(180, 50));
            TextMeshProUGUI oppTmp = AddText(oppValue, "12.45s", 36, WINNER_GREEN, FontStyles.Bold);
            AddGlow(oppValue, WINNER_GREEN, 2);

            // === STATS con estilo neón ===
            GameObject statsContainer = CreateChild(face.transform, "StatsContainer");
            SetupRectTransform(statsContainer, new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 160), new Vector2(450, 100));

            CreateStatRow(statsContainer.transform, "TimeRow", "Tu tiempo", "00.00s", LOSE_NEON_LIGHT, 25);
            CreateStatRow(statsContainer.transform, "ErrorsRow", "Errores", "0", LOSE_NEON_LIGHT, -25);

            // === BOTONES NEÓN ===
            GameObject buttonsContainer = CreateChild(face.transform, "ButtonsContainer");
            SetupRectTransform(buttonsContainer, new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 55), new Vector2(-40, 95));

            HorizontalLayoutGroup btnLayout = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
            btnLayout.childAlignment = TextAnchor.MiddleCenter;
            btnLayout.spacing = 25;
            btnLayout.childForceExpandWidth = false;
            btnLayout.childControlWidth = false;

            // Botón Revancha (principal) - Color magenta neón
            CreateButton3D(buttonsContainer.transform, "RematchButton", "REVANCHA", LOSE_NEON, 220, 75);

            // Botón Aceptar - Más neutro
            CreateButton3D(buttonsContainer.transform, "AcceptButton", "SALIR", new Color(0.4f, 0.35f, 0.45f), 160, 75);

            // Agregar WinPanelController
            WinPanelController controller = panel.AddComponent<WinPanelController>();

            // Guardar como prefab
            SaveAsPrefab(panel, "LosePanel_RealMoney");
        }

        private static void CreateLosePanel3DEffect(Transform parent, Color accentColor)
        {
            Color panelBg = new Color(0.06f, 0.03f, 0.08f, 0.95f); // Fondo púrpura oscuro

            // Shadow
            GameObject shadow = CreateChild(parent, "Shadow");
            RectTransform shadowRt = shadow.GetComponent<RectTransform>();
            shadowRt.anchorMin = Vector2.zero;
            shadowRt.anchorMax = Vector2.one;
            shadowRt.sizeDelta = Vector2.zero;
            shadowRt.anchoredPosition = new Vector2(8, -15);
            Image shadowImg = shadow.AddComponent<Image>();
            shadowImg.color = new Color(0, 0, 0, 0.6f);

            // Side con color neón
            GameObject side = CreateChild(parent, "Side");
            SetupRectTransform(side, new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, -8), new Vector2(0, 16));
            Image sideImg = side.AddComponent<Image>();
            sideImg.color = new Color(accentColor.r * 0.4f, accentColor.g * 0.2f, accentColor.b * 0.35f, 1f);

            // Face
            GameObject face = CreateChild(parent, "Face");
            SetFullStretch(face.GetComponent<RectTransform>());
            face.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 8);
            Image faceImg = face.AddComponent<Image>();
            faceImg.color = panelBg;

            // Borde neón púrpura con glow
            Outline faceOutline = face.AddComponent<Outline>();
            faceOutline.effectColor = accentColor;
            faceOutline.effectDistance = new Vector2(4, -4);

            // Agregar glow pulsante al panel
            GridGlowPulse panelGlow = face.AddComponent<GridGlowPulse>();
        }

        // ========================================================================
        // HELPER METHODS
        // ========================================================================

        private static void CreatePanel3DEffect(Transform parent, Color bgColor, Color glowColor)
        {
            // Shadow
            GameObject shadow = CreateChild(parent, "Shadow");
            SetupRectTransform(shadow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(6, -12), new Vector2(0, 0));
            RectTransform shadowRt = shadow.GetComponent<RectTransform>();
            shadowRt.anchorMin = Vector2.zero;
            shadowRt.anchorMax = Vector2.one;
            shadowRt.sizeDelta = Vector2.zero;
            shadowRt.anchoredPosition = new Vector2(6, -12);
            Image shadowImg = shadow.AddComponent<Image>();
            shadowImg.color = new Color(0, 0, 0, 0.5f);

            // Side
            GameObject side = CreateChild(parent, "Side");
            SetupRectTransform(side, new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, -6), new Vector2(0, 12));
            Image sideImg = side.AddComponent<Image>();
            sideImg.color = new Color(glowColor.r * 0.4f, glowColor.g * 0.4f, glowColor.b * 0.4f, 1f);

            // Face
            GameObject face = CreateChild(parent, "Face");
            SetFullStretch(face.GetComponent<RectTransform>());
            face.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 6);
            Image faceImg = face.AddComponent<Image>();
            faceImg.color = bgColor;
            AddGlow(face, glowColor, 3);
        }

        private static void CreatePremiumPanel3DEffect(Transform parent)
        {
            // Shadow más pronunciada
            GameObject shadow = CreateChild(parent, "Shadow");
            RectTransform shadowRt = shadow.GetComponent<RectTransform>();
            shadowRt.anchorMin = Vector2.zero;
            shadowRt.anchorMax = Vector2.one;
            shadowRt.sizeDelta = Vector2.zero;
            shadowRt.anchoredPosition = new Vector2(8, -15);
            Image shadowImg = shadow.AddComponent<Image>();
            shadowImg.color = new Color(0, 0, 0, 0.6f);

            // Side dorado
            GameObject side = CreateChild(parent, "Side");
            SetupRectTransform(side, new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, -8), new Vector2(0, 16));
            Image sideImg = side.AddComponent<Image>();
            sideImg.color = GOLD_DARK;

            // Face premium
            GameObject face = CreateChild(parent, "Face");
            SetFullStretch(face.GetComponent<RectTransform>());
            face.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 8);
            Image faceImg = face.AddComponent<Image>();
            faceImg.color = PREMIUM_PANEL;

            // Doble borde dorado
            Outline outerGlow = face.AddComponent<Outline>();
            outerGlow.effectColor = GOLD;
            outerGlow.effectDistance = new Vector2(4, -4);

            // Segundo outline para efecto más rico
            Shadow innerGlow = face.AddComponent<Shadow>();
            innerGlow.effectColor = new Color(GOLD.r, GOLD.g, GOLD.b, 0.3f);
            innerGlow.effectDistance = new Vector2(0, -2);
        }

        private static void CreateStatRow(Transform parent, string name, string label, string value, Color color, float yOffset)
        {
            GameObject row = CreateChild(parent, name);
            SetupRectTransform(row, new Vector2(0, 0.5f), new Vector2(1, 0.5f),
                new Vector2(0, yOffset), new Vector2(0, 40));

            // Label
            GameObject labelObj = CreateChild(row.transform, "Label");
            SetupRectTransform(labelObj, new Vector2(0, 0), new Vector2(0.5f, 1),
                Vector2.zero, Vector2.zero);
            TextMeshProUGUI labelTmp = AddText(labelObj, label, 22, new Color(0.7f, 0.7f, 0.7f), FontStyles.Normal);
            labelTmp.alignment = TextAlignmentOptions.Left;

            // Value
            GameObject valueObj = CreateChild(row.transform, "Value");
            SetupRectTransform(valueObj, new Vector2(0.5f, 0), new Vector2(1, 1),
                Vector2.zero, Vector2.zero);
            TextMeshProUGUI valueTmp = AddText(valueObj, value, 26, color, FontStyles.Bold);
            valueTmp.alignment = TextAlignmentOptions.Right;
        }

        private static void CreateButton3D(Transform parent, string name, string text, Color color, float width, float height)
        {
            GameObject btn = CreateChild(parent, name);
            LayoutElement layout = btn.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = height;

            // Shadow - ajustado
            GameObject shadow = CreateChild(btn.transform, "Shadow");
            SetupRectTransform(shadow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(4, -8), new Vector2(width, height));
            Image shadowImg = shadow.AddComponent<Image>();
            shadowImg.color = new Color(0, 0, 0, 0.4f);

            // Side - más grueso
            GameObject side = CreateChild(btn.transform, "Side");
            SetupRectTransform(side, new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 0), new Vector2(width, 10));
            Image sideImg = side.AddComponent<Image>();
            sideImg.color = new Color(color.r * 0.5f, color.g * 0.5f, color.b * 0.5f, 1f);

            // Face - tamaño completo para que el texto quepa
            GameObject face = CreateChild(btn.transform, "Face");
            SetupRectTransform(face, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 5), new Vector2(width, height - 10));
            Image faceImg = face.AddComponent<Image>();
            faceImg.color = color;

            // Text - tamaño de fuente adaptativo basado en altura
            GameObject textObj = CreateChild(face.transform, "Text");
            SetupRectTransform(textObj, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-10, -6));
            float fontSize = Mathf.Min(height * 0.35f, 28f); // Máximo 28, proporcional a altura
            AddText(textObj, text, fontSize, DARK_BG, FontStyles.Bold);

            // Button component
            Button button = btn.AddComponent<Button>();
            button.targetGraphic = faceImg;

            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;
        }

        private static void CreatePremiumButton(Transform parent, string name, string text, Color color, float width, float height)
        {
            GameObject btn = CreateChild(parent, name);
            SetupRectTransform(btn, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(width, height));

            // Shadow premium
            GameObject shadow = CreateChild(btn.transform, "Shadow");
            SetupRectTransform(shadow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(5, -10), new Vector2(width, height));
            Image shadowImg = shadow.AddComponent<Image>();
            shadowImg.color = new Color(0, 0, 0, 0.5f);

            // Side dorado - más grueso
            GameObject side = CreateChild(btn.transform, "Side");
            SetupRectTransform(side, new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 0), new Vector2(width, 12));
            Image sideImg = side.AddComponent<Image>();
            sideImg.color = GOLD_DARK;

            // Face - tamaño completo
            GameObject face = CreateChild(btn.transform, "Face");
            SetupRectTransform(face, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 6), new Vector2(width, height - 12));
            Image faceImg = face.AddComponent<Image>();
            faceImg.color = color;

            // Glow dorado
            Outline faceGlow = face.AddComponent<Outline>();
            faceGlow.effectColor = GOLD_LIGHT;
            faceGlow.effectDistance = new Vector2(3, -3);

            // Text - tamaño adaptativo
            GameObject textObj = CreateChild(face.transform, "Text");
            SetupRectTransform(textObj, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-10, -6));
            float fontSize = Mathf.Min(height * 0.38f, 32f); // Máximo 32 para botones premium
            AddText(textObj, text, fontSize, PREMIUM_BG, FontStyles.Bold);

            // Button component
            Button button = btn.AddComponent<Button>();
            button.targetGraphic = faceImg;

            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(1.1f, 1f, 0.9f);
            colors.pressedColor = new Color(0.9f, 0.8f, 0.6f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;
        }

        private static void AssignNormalReferences(WinPanelController controller, GameObject panel, GameObject content)
        {
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("isRealMoneyPanel").boolValue = false;
            so.FindProperty("canvasGroup").objectReferenceValue = panel.GetComponent<CanvasGroup>();
            so.FindProperty("content").objectReferenceValue = content;

            Transform face = content.transform.Find("Face");
            if (face != null)
            {
                so.FindProperty("titleText").objectReferenceValue =
                    face.Find("Title")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("timeText").objectReferenceValue =
                    face.Find("StatsContainer/TimeRow/Value")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("errorsText").objectReferenceValue =
                    face.Find("StatsContainer/ErrorsRow/Value")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("acceptButton").objectReferenceValue =
                    face.Find("ButtonsContainer/AcceptButton")?.GetComponent<Button>();
                so.FindProperty("playAgainButton").objectReferenceValue =
                    face.Find("ButtonsContainer/PlayAgainButton")?.GetComponent<Button>();
            }
            so.ApplyModifiedProperties();
        }

        private static void AssignRealMoneyReferences(WinPanelController controller, GameObject panel, GameObject content)
        {
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("isRealMoneyPanel").boolValue = true;
            so.FindProperty("canvasGroup").objectReferenceValue = panel.GetComponent<CanvasGroup>();
            so.FindProperty("content").objectReferenceValue = content;

            Transform face = content.transform.Find("Face");
            if (face != null)
            {
                so.FindProperty("titleText").objectReferenceValue =
                    face.Find("Title")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("moneyWonText").objectReferenceValue =
                    face.Find("MoneyContainer/MoneyText")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("wagerText").objectReferenceValue =
                    face.Find("WagerText")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("vsContainer").objectReferenceValue =
                    face.Find("VSContainer")?.gameObject;
                so.FindProperty("playerScoreText").objectReferenceValue =
                    face.Find("VSContainer/PlayerScore/Value")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("opponentScoreText").objectReferenceValue =
                    face.Find("VSContainer/OpponentScore/Value")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("timeText").objectReferenceValue =
                    face.Find("StatsContainer/TimeRow/Value")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("errorsText").objectReferenceValue =
                    face.Find("StatsContainer/ErrorsRow/Value")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("acceptButton").objectReferenceValue =
                    face.Find("ButtonContainer/AcceptButton")?.GetComponent<Button>();
            }

            so.FindProperty("sparkleEffect").objectReferenceValue =
                panel.transform.Find("ParticleEffects")?.GetComponent<UISparkleEffect>();

            so.ApplyModifiedProperties();
        }

        private static void SaveAsPrefab(GameObject obj, string name)
        {
            // Crear carpeta si no existe
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
            }
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/UI"))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "UI");
            }

            string path = $"Assets/_Project/Prefabs/UI/{name}.prefab";

            // Eliminar prefab existente si existe
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            PrefabUtility.SaveAsPrefabAsset(obj, path);
            DestroyImmediate(obj);

            Debug.Log($"[WinPanelUIBuilder] Prefab creado: {path}");
            AssetDatabase.Refresh();
        }

        // ========================================================================
        // UTILITY METHODS
        // ========================================================================

        private static GameObject CreateChild(Transform parent, string name)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            return obj;
        }

        private static void SetFullStretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }

        private static void SetupRectTransform(GameObject obj, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt == null) rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;
        }

        private static TextMeshProUGUI AddText(GameObject obj, string text, float size, Color color, FontStyles style)
        {
            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static void AddGlow(GameObject obj, Color color, float distance)
        {
            Outline outline = obj.GetComponent<Outline>();
            if (outline == null) outline = obj.AddComponent<Outline>();
            outline.effectColor = new Color(color.r, color.g, color.b, 0.6f);
            outline.effectDistance = new Vector2(distance, -distance);
        }
    }
}
