using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// UI Builder para la escena CashTournamentLobby.
    /// Construye el lobby de un torneo individual con info, participantes, chat y acciones.
    /// </summary>
    public class CashTournamentLobbyUIBuilder : EditorWindow
    {
        // Reference Assigner state
        private Vector2 scrollPosition;
        private static int assignedCount = 0;
        private static int failedCount = 0;
        private static int alreadySetCount = 0;
        private static List<AssignResult> assignResults = new List<AssignResult>();

        private struct AssignResult
        {
            public string fieldName;
            public string status;
            public bool success;
            public Object assignedObject;
        }

        #region Colors - Gold Theme

        private static readonly Color GOLD_PRIMARY = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GOLD_DARK = new Color(0.85f, 0.65f, 0.13f, 1f);
        private static readonly Color GOLD_LIGHT = new Color(1f, 0.93f, 0.55f, 1f);
        private static readonly Color AMBER = new Color(1f, 0.75f, 0f, 1f);
        private static readonly Color BG_DARK = new Color(0.06f, 0.05f, 0.10f, 1f);
        private static readonly Color CARD_BG = new Color(0.12f, 0.1f, 0.15f, 0.95f);
        private static readonly Color CARD_BORDER = new Color(0.85f, 0.65f, 0.13f, 0.6f);
        private static readonly Color TEXT_PRIMARY = new Color(1f, 1f, 1f, 1f);
        private static readonly Color TEXT_GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.7f, 0.7f, 0.7f, 1f);
        private static readonly Color BUTTON_GOLD = new Color(0.85f, 0.65f, 0.13f, 1f);
        private static readonly Color CYAN_ACCENT = new Color(0f, 0.9f, 1f, 1f);
        private static readonly Color GREEN_GO = new Color(0.24f, 1f, 0.42f, 1f);
        private static readonly Color RED_CANCEL = new Color(1f, 0.2f, 0.4f, 1f);

        private static readonly Color SILVER = new Color(0.75f, 0.75f, 0.75f, 1f);
        private static readonly Color BRONZE = new Color(0.8f, 0.5f, 0.2f, 1f);

        #endregion

        #region Paths

        private const string BACK_BUTTON_GOLD_PREFAB = "Assets/_Project/Prefabs/Common/BackButtonGold.prefab";
        private const string BACK_ICON_GOLD_PATH = "Assets/_Project/Art/Icons/Navigation/BackIconGold.png";

        #endregion

        [MenuItem("DigitPark/UI Builders/CashBattle/CashTournament Lobby", false, 185)]
        public static void ShowWindow()
        {
            GetWindow<CashTournamentLobbyUIBuilder>("CashTournament Lobby Builder");
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // ========== SECCION 1: UI BUILDER ==========
            GUILayout.Label("CashTournament Lobby UI Builder", EditorStyles.boldLabel);
            GUILayout.Label("Lobby de torneo individual con info, participantes, chat y acciones", EditorStyles.miniLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Construye la UI para CashTournamentLobby:\n\n" +
                "- Header con nombre del torneo y badge de estado\n" +
                "- InfoCard: tipo, entrada, pozo, jugadores, countdown, reglas\n" +
                "- Prize Distribution (1st/2nd/3rd)\n" +
                "- Tabs: Participantes / Chat\n" +
                "- ContentArea: ScrollRect participantes + chat\n" +
                "- ActionBar: Join + Share buttons\n" +
                "- Overlays: Loading + Starting",
                MessageType.Info);

            EditorGUILayout.Space(10);

            GUI.backgroundColor = GOLD_PRIMARY;
            if (GUILayout.Button("CONSTRUIR UI COMPLETA", GUILayout.Height(40)))
            {
                BuildCashTournamentLobbyUI();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Limpiar Escena", GUILayout.Height(25)))
            {
                CleanScene();
            }

            // ========== SEPARADOR ==========
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // ========== SECCION 2: REFERENCE ASSIGNER ==========
            GUILayout.Label("Asignar Referencias", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "CashTournamentLobby")
            {
                EditorGUILayout.HelpBox($"Escena actual: {currentScene}\nAbre CashTournamentLobby primero.", MessageType.Warning);
            }

            MonoBehaviour targetManager = FindLobbyManager();
            if (targetManager != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Manager:", GUILayout.Width(60));
                EditorGUILayout.ObjectField(targetManager, typeof(MonoBehaviour), true);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("CashTournamentLobbyManager no encontrado en escena.", MessageType.Warning);
            }

            EditorGUILayout.Space(5);

            GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
            if (GUILayout.Button("ASIGNAR TODAS LAS REFERENCIAS", GUILayout.Height(36)))
            {
                ResetAssignState();
                RunAssignAllReferences();
                Repaint();
            }
            GUI.backgroundColor = Color.white;

            DrawAssignResults();

            EditorGUILayout.EndScrollView();
        }

        #region Main Build Methods

        private static void BuildCashTournamentLobbyUI()
        {
            CleanupOldUI();

            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontro Canvas. Abre la escena CashTournamentLobby primero.", "OK");
                return;
            }

            if (EditorUtility.DisplayDialog("Construir UI?",
                "Esto reconstruira la UI de CashTournamentLobby.\n\nLos elementos existentes seran reemplazados.\n\nContinuar?",
                "Si, Construir", "Cancelar"))
            {
                BuildAllElements(canvas);
                SetupManagerReferences();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                Debug.Log("[CashTournamentLobbyUIBuilder] UI construida exitosamente!");
            }
        }

        /// <summary>
        /// Builds the UI silently without confirmation dialogs. Used by batch builders.
        /// </summary>
        public static void BuildSilent()
        {
            CleanupOldUI();

            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("[CashTournamentLobbyUIBuilder] Canvas not found - cannot build silently");
                return;
            }

            BuildAllElements(canvas);
            SetupManagerReferences();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("[CashTournamentLobbyUIBuilder] UI built silently (batch mode)");
        }

        /// <summary>
        /// Alias for BuildSilent. Called by FontSizeBatchRebuilder and AllScenesBatchBuilder via reflection.
        /// </summary>
        public static void BuildGoldUI() => BuildSilent();

        private static void CleanScene()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            CleanupOldElements(canvas.transform);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[CashTournamentLobbyUIBuilder] Escena limpiada.");
        }

        private static void CleanupOldUI()
        {
            string[] toClean = { "Background", "SafeArea", "LoadingOverlay", "StartingOverlay", "BackButton", "BackButtonGold" };
            foreach (var canvas in Object.FindObjectsOfType<Canvas>(true))
            {
                if (canvas.transform.parent != null) continue;
                if (canvas.gameObject.name.Contains("Transition") ||
                    canvas.gameObject.name.Contains("Effects")) continue;
                foreach (string name in toClean)
                {
                    Transform t = canvas.transform.Find(name);
                    if (t != null) Object.DestroyImmediate(t.gameObject);
                }
            }
        }

        private static void CleanupOldElements(Transform parent)
        {
            List<GameObject> toDestroy = new List<GameObject>();
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                string name = child.gameObject.name;
                if (name == "TransitionCanvas" || name == "EventSystem")
                    continue;
                toDestroy.Add(child.gameObject);
            }

            foreach (var go in toDestroy)
            {
                DestroyImmediate(go);
            }

            Debug.Log($"[CashTournamentLobbyUIBuilder] Limpiados {toDestroy.Count} objetos antiguos del Canvas");
        }

        private static void BuildAllElements(Canvas canvas)
        {
            Transform canvasTransform = canvas.transform;

            CleanupOldElements(canvasTransform);

            // 1. Background
            CreateBackground(canvasTransform);

            // 2. Safe Area Container
            GameObject safeArea = CreateSafeArea(canvasTransform);

            // 3. Header
            CreateHeader(safeArea.transform);

            // 4. InfoCard
            CreateInfoCard(safeArea.transform);

            // 5. Prize Distribution
            CreatePrizeDistribution(safeArea.transform);

            // 6. Tab Bar
            CreateTabBar(safeArea.transform);

            // 7. Content Area (Participants + Chat)
            CreateContentArea(safeArea.transform);

            // 8. Action Bar
            CreateActionBar(safeArea.transform);

            // 9. Loading Overlay
            CreateLoadingOverlay(canvasTransform);

            // 10. Starting Overlay
            CreateStartingOverlay(canvasTransform);
        }

        #endregion

        #region Background

        private static void CreateBackground(Transform parent)
        {
            GameObject bgContainer = new GameObject("Background");
            bgContainer.transform.SetParent(parent, false);
            bgContainer.transform.SetAsFirstSibling();

            RectTransform bgRT = bgContainer.AddComponent<RectTransform>();
            SetFullStretch(bgRT);

            Image baseImg = bgContainer.AddComponent<Image>();
            baseImg.color = BG_DARK;
            baseImg.raycastTarget = false;

            // Gold gradient overlay at top
            GameObject goldGlow = new GameObject("GoldGlow");
            goldGlow.transform.SetParent(bgContainer.transform, false);

            RectTransform glowRT = goldGlow.AddComponent<RectTransform>();
            glowRT.anchorMin = new Vector2(0, 0.7f);
            glowRT.anchorMax = Vector2.one;
            glowRT.sizeDelta = Vector2.zero;

            Image glowImg = goldGlow.AddComponent<Image>();
            glowImg.color = new Color(1f, 0.8f, 0.3f, 0.06f);
            glowImg.raycastTarget = false;
        }

        #endregion

        #region Safe Area

        private static GameObject CreateSafeArea(Transform parent)
        {
            GameObject safeArea = new GameObject("SafeArea");
            safeArea.transform.SetParent(parent, false);

            RectTransform rt = safeArea.AddComponent<RectTransform>();
            SetFullStretch(rt);

            return safeArea;
        }

        #endregion

        #region Header (0-1x, 0.93-1y)

        private static void CreateHeader(Transform parent)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(parent, false);

            RectTransform rt = header.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.93f);
            rt.anchorMax = new Vector2(1, 1f);
            rt.sizeDelta = Vector2.zero;

            Image bg = header.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.3f);

            // Back Button
            CreateBackButton(header.transform);

            // Tournament Name Text (centered on screen)
            GameObject nameObj = new GameObject("TournamentNameText");
            nameObj.transform.SetParent(header.transform, false);
            RectTransform nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0.5f, 0f);
            nameRT.anchorMax = new Vector2(0.5f, 1f);
            nameRT.pivot = new Vector2(0.5f, 0.5f);
            nameRT.sizeDelta = new Vector2(600, 0);
            nameRT.anchoredPosition = Vector2.zero;
            TextMeshProUGUI nameTMP = nameObj.AddComponent<TextMeshProUGUI>();
            nameTMP.text = "QuickMath Championship";
            nameTMP.fontSize = FontSizes.H4;
            nameTMP.color = TEXT_GOLD;
            nameTMP.alignment = TextAlignmentOptions.MidlineLeft;
            nameTMP.raycastTarget = false;
            nameTMP.fontStyle = FontStyles.Bold;
            nameTMP.enableAutoSizing = true;
            nameTMP.fontSizeMin = FontSizes.AutoMinTitle;
            nameTMP.fontSizeMax = FontSizes.H4;

            // Status Badge (right side)
            GameObject badgeObj = new GameObject("StatusBadge");
            badgeObj.transform.SetParent(header.transform, false);

            RectTransform badgeRT = badgeObj.AddComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(1, 0.5f);
            badgeRT.anchorMax = new Vector2(1, 0.5f);
            badgeRT.pivot = new Vector2(1, 0.5f);
            badgeRT.sizeDelta = new Vector2(160, 46);
            badgeRT.anchoredPosition = new Vector2(-15, 0);

            Image badgeBg = badgeObj.AddComponent<Image>();
            badgeBg.color = GOLD_DARK;

            // Badge text
            GameObject badgeTextObj = CreateElement(badgeObj.transform, "StatusBadgeText",
                Vector2.zero, Vector2.one);
            TextMeshProUGUI badgeTMP = badgeTextObj.AddComponent<TextMeshProUGUI>();
            badgeTMP.text = "OPEN";
            badgeTMP.fontSize = FontSizes.Body;
            badgeTMP.color = TEXT_PRIMARY;
            badgeTMP.alignment = TextAlignmentOptions.Center;
            badgeTMP.fontStyle = FontStyles.Bold;
            badgeTMP.enableAutoSizing = true;
            badgeTMP.fontSizeMin = FontSizes.AutoMinBody;
            badgeTMP.fontSizeMax = FontSizes.Body;
        }

        private static void CreateBackButton(Transform parent)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BACK_BUTTON_GOLD_PREFAB);
            if (prefab != null)
            {
                GameObject backBtn = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                backBtn.name = "BackButton";

                RectTransform rect = backBtn.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 0.5f);
                rect.anchorMax = new Vector2(0, 0.5f);
                rect.pivot = new Vector2(0, 0.5f);
                rect.anchoredPosition = new Vector2(20, 0);
                rect.sizeDelta = new Vector2(50, 50);
                // Assign BackIconGold sprite to Icon child
                Sprite backIcon = AssetDatabase.LoadAssetAtPath<Sprite>(BACK_ICON_GOLD_PATH);
                if (backIcon != null)
                {
                    Transform iconChild = backBtn.transform.Find("Icon");
                    if (iconChild != null)
                    {
                        Image iconImg = iconChild.GetComponent<Image>();
                        if (iconImg != null) iconImg.sprite = backIcon;
                    }
                }
            }
            else
            {
                Debug.LogWarning("[CashTournamentLobby] BackButtonGold prefab not found, using fallback");

                GameObject backBtn = new GameObject("BackButton");
                backBtn.transform.SetParent(parent, false);

                RectTransform rt = backBtn.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 0.5f);
                rt.anchorMax = new Vector2(0, 0.5f);
                rt.pivot = new Vector2(0, 0.5f);
                rt.sizeDelta = new Vector2(50, 50);
                rt.anchoredPosition = new Vector2(20, 0);

                Image img = backBtn.AddComponent<Image>();
                img.color = new Color(1, 1, 1, 0.1f);

                Button btn = backBtn.AddComponent<Button>();
                btn.targetGraphic = img;

                GameObject arrowObj = new GameObject("Arrow");
                arrowObj.transform.SetParent(backBtn.transform, false);

                RectTransform arrowRT = arrowObj.AddComponent<RectTransform>();
                SetFullStretch(arrowRT);

                TextMeshProUGUI arrowTMP = arrowObj.AddComponent<TextMeshProUGUI>();
                arrowTMP.text = "<";
                arrowTMP.fontSize = FontSizes.H4;
                arrowTMP.color = TEXT_GOLD;
                arrowTMP.alignment = TextAlignmentOptions.Center;
                arrowTMP.fontStyle = FontStyles.Bold;
            }
        }

        #endregion

        #region InfoCard (0.03-0.97x, 0.72-0.92y)

        private static void CreateInfoCard(Transform parent)
        {
            GameObject infoCard = new GameObject("InfoCard");
            infoCard.transform.SetParent(parent, false);

            RectTransform rt = infoCard.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.03f, 0.72f);
            rt.anchorMax = new Vector2(0.97f, 0.92f);
            rt.sizeDelta = Vector2.zero;

            Image bg = infoCard.AddComponent<Image>();
            bg.color = CARD_BG;

            Outline outline = infoCard.AddComponent<Outline>();
            outline.effectColor = CARD_BORDER;
            outline.effectDistance = new Vector2(2, -2);

            // Row1: GameType (left) + EntryFee (right)
            GameObject row1 = CreateElement(infoCard.transform, "Row1",
                new Vector2(0.03f, 0.82f), new Vector2(0.97f, 0.97f));

            GameObject gameTypeObj = CreateElement(row1.transform, "GameTypeText",
                new Vector2(0, 0), new Vector2(0.5f, 1));
            TextMeshProUGUI gameTypeTMP = gameTypeObj.AddComponent<TextMeshProUGUI>();
            gameTypeTMP.text = "QuickMath";
            gameTypeTMP.fontSize = FontSizes.Body;
            gameTypeTMP.color = TEXT_SECONDARY;
            gameTypeTMP.alignment = TextAlignmentOptions.Left;
            gameTypeTMP.fontStyle = FontStyles.Bold;

            GameObject entryFeeObj = CreateElement(row1.transform, "EntryFeeText",
                new Vector2(0.5f, 0), new Vector2(1, 1));
            TextMeshProUGUI entryFeeTMP = entryFeeObj.AddComponent<TextMeshProUGUI>();
            entryFeeTMP.text = "$5.00 Entry";
            entryFeeTMP.fontSize = FontSizes.BodyLarge;
            entryFeeTMP.color = TEXT_GOLD;
            entryFeeTMP.alignment = TextAlignmentOptions.Right;
            entryFeeTMP.fontStyle = FontStyles.Bold;

            // Row2: Prize Pool (centered, large)
            GameObject prizePoolObj = CreateElement(infoCard.transform, "PrizePoolText",
                new Vector2(0.05f, 0.58f), new Vector2(0.95f, 0.82f));
            TextMeshProUGUI prizePoolTMP = prizePoolObj.AddComponent<TextMeshProUGUI>();
            prizePoolTMP.text = "$80.00";
            prizePoolTMP.fontSize = FontSizes.H3;
            prizePoolTMP.color = GOLD_PRIMARY;
            prizePoolTMP.alignment = TextAlignmentOptions.Center;
            prizePoolTMP.fontStyle = FontStyles.Bold;
            prizePoolTMP.enableAutoSizing = true;
            prizePoolTMP.fontSizeMin = FontSizes.AutoMinBody;
            prizePoolTMP.fontSizeMax = FontSizes.H3;

            // Players Progress Bar
            GameObject progressContainer = CreateElement(infoCard.transform, "PlayersProgress",
                new Vector2(0.05f, 0.38f), new Vector2(0.95f, 0.56f));

            // ProgressBarBG
            GameObject progressBarBG = CreateElement(progressContainer.transform, "ProgressBarBG",
                new Vector2(0, 0), new Vector2(1, 0.5f));
            Image progressBGImg = progressBarBG.AddComponent<Image>();
            progressBGImg.color = new Color(0.15f, 0.13f, 0.18f, 1f);

            // ProgressBarFill
            GameObject progressBarFill = CreateElement(progressBarBG.transform, "ProgressBarFill",
                new Vector2(0, 0), new Vector2(0.5f, 1));
            Image progressFillImg = progressBarFill.AddComponent<Image>();
            progressFillImg.color = GOLD_PRIMARY;

            // PlayersProgressText
            GameObject playersProgressTextObj = CreateElement(progressContainer.transform, "PlayersProgressText",
                new Vector2(0, 0.5f), new Vector2(1, 1));
            TextMeshProUGUI playersProgressTMP = playersProgressTextObj.AddComponent<TextMeshProUGUI>();
            playersProgressTMP.text = "8/16 Players";
            playersProgressTMP.fontSize = FontSizes.Body;
            playersProgressTMP.color = TEXT_PRIMARY;
            playersProgressTMP.alignment = TextAlignmentOptions.Center;
            playersProgressTMP.fontStyle = FontStyles.Bold;

            // Countdown Text
            GameObject countdownObj = CreateElement(infoCard.transform, "CountdownText",
                new Vector2(0.05f, 0.18f), new Vector2(0.95f, 0.36f));
            TextMeshProUGUI countdownTMP = countdownObj.AddComponent<TextMeshProUGUI>();
            countdownTMP.text = "Starts in 45:00";
            countdownTMP.fontSize = FontSizes.Body;
            countdownTMP.color = TEXT_SECONDARY;
            countdownTMP.alignment = TextAlignmentOptions.Center;
            countdownTMP.fontStyle = FontStyles.Bold;

            // Rules Row
            GameObject rulesRow = CreateElement(infoCard.transform, "RulesRow",
                new Vector2(0.03f, 0.02f), new Vector2(0.97f, 0.18f));

            GameObject attemptsObj = CreateElement(rulesRow.transform, "AttemptsRuleText",
                new Vector2(0, 0), new Vector2(0.5f, 1));
            TextMeshProUGUI attemptsTMP = attemptsObj.AddComponent<TextMeshProUGUI>();
            attemptsTMP.text = "3 Attempts";
            attemptsTMP.fontSize = FontSizes.Body;
            attemptsTMP.color = TEXT_SECONDARY;
            attemptsTMP.alignment = TextAlignmentOptions.Left;
            attemptsTMP.fontStyle = FontStyles.Bold;
            attemptsTMP.enableAutoSizing = true;
            attemptsTMP.fontSizeMin = FontSizes.AutoMinBody;
            attemptsTMP.fontSizeMax = FontSizes.Body;

            GameObject timeLimitObj = CreateElement(rulesRow.transform, "TimeLimitRuleText",
                new Vector2(0.5f, 0), new Vector2(1, 1));
            TextMeshProUGUI timeLimitTMP = timeLimitObj.AddComponent<TextMeshProUGUI>();
            timeLimitTMP.text = "2:00 Time Limit";
            timeLimitTMP.fontSize = FontSizes.Body;
            timeLimitTMP.color = TEXT_SECONDARY;
            timeLimitTMP.alignment = TextAlignmentOptions.Right;
            timeLimitTMP.fontStyle = FontStyles.Bold;
            timeLimitTMP.enableAutoSizing = true;
            timeLimitTMP.fontSizeMin = FontSizes.AutoMinBody;
            timeLimitTMP.fontSizeMax = FontSizes.Body;
        }

        #endregion

        #region Prize Distribution (0.03-0.97x, 0.62-0.71y)

        private static void CreatePrizeDistribution(Transform parent)
        {
            GameObject prizeSection = new GameObject("PrizeDistribution");
            prizeSection.transform.SetParent(parent, false);

            RectTransform rt = prizeSection.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.03f, 0.62f);
            rt.anchorMax = new Vector2(0.97f, 0.71f);
            rt.sizeDelta = Vector2.zero;

            Image bg = prizeSection.AddComponent<Image>();
            bg.color = CARD_BG;

            // Title
            GameObject titleObj = CreateElement(prizeSection.transform, "PrizeDistTitle",
                new Vector2(0.03f, 0.55f), new Vector2(0.97f, 1));
            TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "Prize Distribution";
            titleTMP.fontSize = FontSizes.BodyLarge;
            titleTMP.color = TEXT_GOLD;
            titleTMP.alignment = TextAlignmentOptions.Left;
            titleTMP.fontStyle = FontStyles.Bold;

            // Prize Rows Container
            GameObject prizeContainer = CreateElement(prizeSection.transform, "PrizeRowsContainer",
                new Vector2(0.03f, 0), new Vector2(0.97f, 0.55f));

            HorizontalLayoutGroup hlg = prizeContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // 1st Place
            CreatePrizeRow(prizeContainer.transform, "1st", "$40.00", GOLD_PRIMARY);
            // 2nd Place
            CreatePrizeRow(prizeContainer.transform, "2nd", "$24.00", SILVER);
            // 3rd Place
            CreatePrizeRow(prizeContainer.transform, "3rd", "$16.00", BRONZE);
        }

        private static void CreatePrizeRow(Transform parent, string place, string amount, Color color)
        {
            GameObject row = new GameObject("Prize_" + place);
            row.transform.SetParent(parent, false);

            LayoutElement le = row.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;

            // Place label
            GameObject placeObj = new GameObject("Place");
            placeObj.transform.SetParent(row.transform, false);

            RectTransform placeRT = placeObj.AddComponent<RectTransform>();
            placeRT.anchorMin = new Vector2(0, 0.5f);
            placeRT.anchorMax = new Vector2(0.4f, 1);
            placeRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI placeTMP = placeObj.AddComponent<TextMeshProUGUI>();
            placeTMP.text = place;
            placeTMP.fontSize = FontSizes.Body;
            placeTMP.color = color;
            placeTMP.alignment = TextAlignmentOptions.Center;
            placeTMP.fontStyle = FontStyles.Bold;

            // Amount
            GameObject amountObj = new GameObject("Amount");
            amountObj.transform.SetParent(row.transform, false);

            RectTransform amountRT = amountObj.AddComponent<RectTransform>();
            amountRT.anchorMin = new Vector2(0.4f, 0);
            amountRT.anchorMax = new Vector2(1, 0.5f);
            amountRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI amountTMP = amountObj.AddComponent<TextMeshProUGUI>();
            amountTMP.text = amount;
            amountTMP.fontSize = FontSizes.Body;
            amountTMP.color = color;
            amountTMP.alignment = TextAlignmentOptions.Center;
            amountTMP.fontStyle = FontStyles.Bold;
        }

        #endregion

        #region Tab Bar (0.03-0.97x, 0.57-0.62y)

        private static void CreateTabBar(Transform parent)
        {
            GameObject tabBar = new GameObject("TabBar");
            tabBar.transform.SetParent(parent, false);

            RectTransform rt = tabBar.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.03f, 0.57f);
            rt.anchorMax = new Vector2(0.97f, 0.62f);
            rt.sizeDelta = Vector2.zero;

            HorizontalLayoutGroup hlg = tabBar.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 0;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // Participants Tab (active)
            CreateTab(tabBar.transform, "ParticipantsTab", "Participants", true);

            // Chat Tab (inactive)
            GameObject chatTab = CreateTab(tabBar.transform, "ChatTab", "Chat", false);

            // Chat Badge (hidden)
            GameObject chatBadge = new GameObject("ChatBadge");
            chatBadge.transform.SetParent(chatTab.transform, false);
            chatBadge.SetActive(false);

            RectTransform badgeRT = chatBadge.AddComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(1, 1);
            badgeRT.anchorMax = new Vector2(1, 1);
            badgeRT.pivot = new Vector2(1, 1);
            badgeRT.sizeDelta = new Vector2(36, 36);
            badgeRT.anchoredPosition = new Vector2(4, 4);

            Image badgeBg = chatBadge.AddComponent<Image>();
            badgeBg.color = AMBER;

            GameObject badgeTextObj = new GameObject("ChatBadgeText");
            badgeTextObj.transform.SetParent(chatBadge.transform, false);

            RectTransform badgeTextRT = badgeTextObj.AddComponent<RectTransform>();
            SetFullStretch(badgeTextRT);

            TextMeshProUGUI badgeTMP = badgeTextObj.AddComponent<TextMeshProUGUI>();
            badgeTMP.text = "0";
            badgeTMP.fontSize = FontSizes.Body;
            badgeTMP.color = BG_DARK;
            badgeTMP.alignment = TextAlignmentOptions.Center;
            badgeTMP.fontStyle = FontStyles.Bold;
        }

        private static GameObject CreateTab(Transform parent, string name, string label, bool isActive)
        {
            GameObject tab = new GameObject(name);
            tab.transform.SetParent(parent, false);

            LayoutElement le = tab.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;

            Image bg = tab.AddComponent<Image>();
            bg.color = isActive ? new Color(0.15f, 0.12f, 0.2f, 1f) : new Color(0.08f, 0.07f, 0.12f, 1f);

            Button btn = tab.AddComponent<Button>();
            btn.targetGraphic = bg;

            // Tab Label
            GameObject labelObj = new GameObject("TabLabel");
            labelObj.transform.SetParent(tab.transform, false);

            RectTransform labelRT = labelObj.AddComponent<RectTransform>();
            SetFullStretch(labelRT);
            labelRT.offsetMin = new Vector2(0, 4);
            labelRT.offsetMax = new Vector2(0, -4);

            TextMeshProUGUI labelTMP = labelObj.AddComponent<TextMeshProUGUI>();
            labelTMP.text = label;
            labelTMP.fontSize = FontSizes.BodyLarge;
            labelTMP.color = isActive ? TEXT_GOLD : TEXT_SECONDARY;
            labelTMP.alignment = TextAlignmentOptions.Center;
            labelTMP.fontStyle = FontStyles.Bold;

            // Tab Indicator (underline)
            GameObject indicator = new GameObject(name + "Indicator");
            indicator.transform.SetParent(tab.transform, false);

            RectTransform indRT = indicator.AddComponent<RectTransform>();
            indRT.anchorMin = new Vector2(0.1f, 0);
            indRT.anchorMax = new Vector2(0.9f, 0);
            indRT.pivot = new Vector2(0.5f, 0);
            indRT.sizeDelta = new Vector2(0, 4);

            Image indImg = indicator.AddComponent<Image>();
            indImg.color = isActive ? GOLD_PRIMARY : Color.clear;

            return tab;
        }

        #endregion

        #region Content Area (0.03-0.97x, 0.12-0.56y)

        private static void CreateContentArea(Transform parent)
        {
            GameObject contentArea = new GameObject("ContentArea");
            contentArea.transform.SetParent(parent, false);

            RectTransform rt = contentArea.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.03f, 0.12f);
            rt.anchorMax = new Vector2(0.97f, 0.56f);
            rt.sizeDelta = Vector2.zero;

            // Participants Content (visible by default)
            CreateParticipantsContent(contentArea.transform);

            // Chat Content (hidden by default)
            CreateChatContent(contentArea.transform);
        }

        private static void CreateParticipantsContent(Transform parent)
        {
            GameObject participantsContent = new GameObject("ParticipantsContent");
            participantsContent.transform.SetParent(parent, false);

            RectTransform rt = participantsContent.AddComponent<RectTransform>();
            SetFullStretch(rt);

            // ScrollRect
            ScrollRect scroll = participantsContent.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 50;

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(participantsContent.transform, false);

            RectTransform vpRT = viewport.AddComponent<RectTransform>();
            SetFullStretch(vpRT);

            Image vpImg = viewport.AddComponent<Image>();
            vpImg.color = Color.clear;
            vpImg.raycastTarget = true;
            viewport.AddComponent<RectMask2D>();

            // Content container
            GameObject content = new GameObject("ParticipantsContainer");
            content.transform.SetParent(viewport.transform, false);

            RectTransform contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.sizeDelta = new Vector2(0, 0);

            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6;
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = vpRT;
            scroll.content = contentRT;

            // Items instantiated at runtime from ParticipantItem prefab
        }

        private static void CreateParticipantItem(Transform parent, string playerName, string score, int rank)
        {
            GameObject item = new GameObject("ParticipantItem_" + rank);
            item.transform.SetParent(parent, false);

            LayoutElement le = item.AddComponent<LayoutElement>();
            le.preferredHeight = 70;

            Image bg = item.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.09f, 0.14f, 0.8f);

            // Avatar placeholder
            GameObject avatar = new GameObject("Avatar");
            avatar.transform.SetParent(item.transform, false);

            RectTransform avatarRT = avatar.AddComponent<RectTransform>();
            avatarRT.anchorMin = new Vector2(0, 0.5f);
            avatarRT.anchorMax = new Vector2(0, 0.5f);
            avatarRT.pivot = new Vector2(0, 0.5f);
            avatarRT.sizeDelta = new Vector2(50, 50);
            avatarRT.anchoredPosition = new Vector2(10, 0);

            Image avatarImg = avatar.AddComponent<Image>();
            avatarImg.color = new Color(0.3f, 0.3f, 0.35f, 1f);

            // Name
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(item.transform, false);

            RectTransform nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0);
            nameRT.anchorMax = new Vector2(1, 1);
            nameRT.offsetMin = new Vector2(70, 0);
            nameRT.offsetMax = new Vector2(-120, 0);

            TextMeshProUGUI nameTMP = nameObj.AddComponent<TextMeshProUGUI>();
            nameTMP.text = playerName;
            nameTMP.fontSize = FontSizes.Body;
            nameTMP.color = TEXT_PRIMARY;
            nameTMP.alignment = TextAlignmentOptions.Left;
            nameTMP.fontStyle = FontStyles.Bold;

            // Score
            GameObject scoreObj = new GameObject("Score");
            scoreObj.transform.SetParent(item.transform, false);

            RectTransform scoreRT = scoreObj.AddComponent<RectTransform>();
            scoreRT.anchorMin = new Vector2(1, 0);
            scoreRT.anchorMax = new Vector2(1, 1);
            scoreRT.pivot = new Vector2(1, 0.5f);
            scoreRT.sizeDelta = new Vector2(110, 0);
            scoreRT.anchoredPosition = new Vector2(-10, 0);

            TextMeshProUGUI scoreTMP = scoreObj.AddComponent<TextMeshProUGUI>();
            scoreTMP.text = score;
            scoreTMP.fontSize = FontSizes.Body;
            scoreTMP.color = TEXT_GOLD;
            scoreTMP.alignment = TextAlignmentOptions.Right;
            scoreTMP.fontStyle = FontStyles.Bold;
        }

        private static void CreateChatContent(Transform parent)
        {
            GameObject chatContent = new GameObject("ChatContent");
            chatContent.transform.SetParent(parent, false);
            chatContent.SetActive(false);

            RectTransform rt = chatContent.AddComponent<RectTransform>();
            SetFullStretch(rt);

            // Chat Messages ScrollRect
            GameObject chatScroll = new GameObject("ChatScrollRect");
            chatScroll.transform.SetParent(chatContent.transform, false);

            RectTransform scrollRT = chatScroll.AddComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0, 0.12f);
            scrollRT.anchorMax = Vector2.one;
            scrollRT.sizeDelta = Vector2.zero;

            ScrollRect scroll = chatScroll.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(chatScroll.transform, false);

            RectTransform vpRT = viewport.AddComponent<RectTransform>();
            SetFullStretch(vpRT);

            Image vpImg = viewport.AddComponent<Image>();
            vpImg.color = Color.clear;
            vpImg.raycastTarget = true;
            viewport.AddComponent<RectMask2D>();

            // Messages Container
            GameObject messages = new GameObject("ChatMessagesContainer");
            messages.transform.SetParent(viewport.transform, false);

            RectTransform msgRT = messages.AddComponent<RectTransform>();
            msgRT.anchorMin = new Vector2(0, 1);
            msgRT.anchorMax = new Vector2(1, 1);
            msgRT.pivot = new Vector2(0.5f, 1);
            msgRT.sizeDelta = new Vector2(0, 0);

            VerticalLayoutGroup vlg = messages.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6;
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            ContentSizeFitter csf = messages.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = vpRT;
            scroll.content = msgRT;

            // Input Bar
            GameObject inputBar = new GameObject("InputBar");
            inputBar.transform.SetParent(chatContent.transform, false);

            RectTransform inputBarRT = inputBar.AddComponent<RectTransform>();
            inputBarRT.anchorMin = Vector2.zero;
            inputBarRT.anchorMax = new Vector2(1, 0.12f);
            inputBarRT.sizeDelta = Vector2.zero;

            Image inputBarBg = inputBar.AddComponent<Image>();
            inputBarBg.color = new Color(0.08f, 0.07f, 0.12f, 1f);

            // TMP_InputField
            GameObject inputFieldObj = new GameObject("ChatInput");
            inputFieldObj.transform.SetParent(inputBar.transform, false);

            RectTransform inputFieldRT = inputFieldObj.AddComponent<RectTransform>();
            inputFieldRT.anchorMin = new Vector2(0.02f, 0.1f);
            inputFieldRT.anchorMax = new Vector2(0.78f, 0.9f);
            inputFieldRT.sizeDelta = Vector2.zero;

            Image inputBg = inputFieldObj.AddComponent<Image>();
            inputBg.color = new Color(0.15f, 0.13f, 0.2f, 1f);

            TMP_InputField inputField = inputFieldObj.AddComponent<TMP_InputField>();
            inputField.characterLimit = 200;

            // Input Text Area
            GameObject textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputFieldObj.transform, false);

            RectTransform taRT = textArea.AddComponent<RectTransform>();
            SetFullStretch(taRT);
            taRT.offsetMin = new Vector2(10, 0);
            taRT.offsetMax = new Vector2(-10, 0);

            // Placeholder
            GameObject placeholder = new GameObject("Placeholder");
            placeholder.transform.SetParent(textArea.transform, false);

            RectTransform phRT = placeholder.AddComponent<RectTransform>();
            SetFullStretch(phRT);

            TextMeshProUGUI phTMP = placeholder.AddComponent<TextMeshProUGUI>();
            phTMP.text = "Type a message...";
            phTMP.fontSize = FontSizes.Body;
            phTMP.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            phTMP.alignment = TextAlignmentOptions.Left;
            phTMP.fontStyle = FontStyles.Bold;

            // Input Text
            GameObject inputTextObj = new GameObject("ChatInputText");
            inputTextObj.transform.SetParent(textArea.transform, false);

            RectTransform itRT = inputTextObj.AddComponent<RectTransform>();
            SetFullStretch(itRT);

            TextMeshProUGUI inputTMP = inputTextObj.AddComponent<TextMeshProUGUI>();
            inputTMP.text = "";
            inputTMP.fontSize = FontSizes.Body;
            inputTMP.fontStyle = FontStyles.Bold;
            inputTMP.color = TEXT_PRIMARY;
            inputTMP.alignment = TextAlignmentOptions.Left;

            inputField.textComponent = inputTMP;
            inputField.placeholder = phTMP;
            inputField.textViewport = taRT;

            // Send Button
            GameObject sendBtn = new GameObject("SendChatButton");
            sendBtn.transform.SetParent(inputBar.transform, false);

            RectTransform sendRT = sendBtn.AddComponent<RectTransform>();
            sendRT.anchorMin = new Vector2(0.80f, 0.1f);
            sendRT.anchorMax = new Vector2(0.98f, 0.9f);
            sendRT.sizeDelta = Vector2.zero;

            Image sendBg = sendBtn.AddComponent<Image>();
            sendBg.color = BUTTON_GOLD;

            Button sendButton = sendBtn.AddComponent<Button>();
            sendButton.targetGraphic = sendBg;

            GameObject sendTextObj = new GameObject("SendChatButtonText");
            sendTextObj.transform.SetParent(sendBtn.transform, false);

            RectTransform stRT = sendTextObj.AddComponent<RectTransform>();
            SetFullStretch(stRT);

            TextMeshProUGUI sendTMP = sendTextObj.AddComponent<TextMeshProUGUI>();
            sendTMP.text = "Send";
            sendTMP.fontSize = FontSizes.Body;
            sendTMP.color = BG_DARK;
            sendTMP.alignment = TextAlignmentOptions.Center;
            sendTMP.fontStyle = FontStyles.Bold;
        }

        #endregion

        #region Action Bar (0-1x, 0-0.11y)

        private static void CreateActionBar(Transform parent)
        {
            GameObject actionBar = new GameObject("ActionBar");
            actionBar.transform.SetParent(parent, false);

            RectTransform rt = actionBar.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = new Vector2(1, 0.11f);
            rt.sizeDelta = Vector2.zero;

            Image bg = actionBar.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.04f, 0.08f, 0.95f);

            // Join Button
            GameObject joinBtn = new GameObject("JoinButton");
            joinBtn.transform.SetParent(actionBar.transform, false);

            RectTransform joinRT = joinBtn.AddComponent<RectTransform>();
            joinRT.anchorMin = new Vector2(0.05f, 0.15f);
            joinRT.anchorMax = new Vector2(0.70f, 0.85f);
            joinRT.sizeDelta = Vector2.zero;

            Image joinBg = joinBtn.AddComponent<Image>();
            joinBg.color = BUTTON_GOLD;

            Button joinButton = joinBtn.AddComponent<Button>();
            joinButton.targetGraphic = joinBg;

            GameObject joinTextObj = CreateElement(joinBtn.transform, "CashJoinButtonText",
                Vector2.zero, Vector2.one);
            TextMeshProUGUI joinTMP = joinTextObj.AddComponent<TextMeshProUGUI>();
            joinTMP.text = "Join -- $5.00";
            joinTMP.fontSize = FontSizes.Body;
            joinTMP.color = BG_DARK;
            joinTMP.alignment = TextAlignmentOptions.Center;
            joinTMP.fontStyle = FontStyles.Bold;

            // Leave Button (hidden by default, shown after joining)
            GameObject leaveBtn = new GameObject("LeaveButton");
            leaveBtn.transform.SetParent(actionBar.transform, false);
            leaveBtn.SetActive(false);

            RectTransform leaveRT = leaveBtn.AddComponent<RectTransform>();
            leaveRT.anchorMin = new Vector2(0.05f, 0.15f);
            leaveRT.anchorMax = new Vector2(0.70f, 0.85f);
            leaveRT.sizeDelta = Vector2.zero;

            Image leaveBg = leaveBtn.AddComponent<Image>();
            leaveBg.color = RED_CANCEL;

            Button leaveButton = leaveBtn.AddComponent<Button>();
            leaveButton.targetGraphic = leaveBg;

            GameObject leaveTextObj = CreateElement(leaveBtn.transform, "LeaveButtonText",
                Vector2.zero, Vector2.one);
            TextMeshProUGUI leaveTMP = leaveTextObj.AddComponent<TextMeshProUGUI>();
            leaveTMP.text = "Leave Tournament";
            leaveTMP.fontSize = FontSizes.Body;
            leaveTMP.color = TEXT_PRIMARY;
            leaveTMP.alignment = TextAlignmentOptions.Center;
            leaveTMP.fontStyle = FontStyles.Bold;

            // Share Button
            GameObject shareBtn = new GameObject("ShareButton");
            shareBtn.transform.SetParent(actionBar.transform, false);

            RectTransform shareRT = shareBtn.AddComponent<RectTransform>();
            shareRT.anchorMin = new Vector2(0.72f, 0.15f);
            shareRT.anchorMax = new Vector2(0.95f, 0.85f);
            shareRT.sizeDelta = Vector2.zero;

            Image shareBg = shareBtn.AddComponent<Image>();
            shareBg.color = Color.clear;

            Outline shareOutline = shareBtn.AddComponent<Outline>();
            shareOutline.effectColor = GOLD_DARK;
            shareOutline.effectDistance = new Vector2(2, -2);

            Button shareButton = shareBtn.AddComponent<Button>();
            shareButton.targetGraphic = shareBg;

            GameObject shareTextObj = CreateElement(shareBtn.transform, "CashShareButtonText",
                Vector2.zero, Vector2.one);
            TextMeshProUGUI shareTMP = shareTextObj.AddComponent<TextMeshProUGUI>();
            shareTMP.text = "Share";
            shareTMP.fontSize = FontSizes.Body;
            shareTMP.color = TEXT_GOLD;
            shareTMP.alignment = TextAlignmentOptions.Center;
            shareTMP.fontStyle = FontStyles.Bold;
        }

        #endregion

        #region Loading Overlay (fullscreen, hidden)

        private static void CreateLoadingOverlay(Transform parent)
        {
            GameObject overlay = new GameObject("LoadingOverlay");
            overlay.transform.SetParent(parent, false);
            overlay.SetActive(false);

            RectTransform rt = overlay.AddComponent<RectTransform>();
            SetFullStretch(rt);

            Image bg = overlay.AddComponent<Image>();
            bg.color = new Color(CARD_BG.r, CARD_BG.g, CARD_BG.b, 0.9f);
            bg.raycastTarget = true;

            // Status Text
            GameObject statusObj = new GameObject("StatusText");
            statusObj.transform.SetParent(overlay.transform, false);

            RectTransform statusRT = statusObj.AddComponent<RectTransform>();
            statusRT.anchorMin = new Vector2(0.1f, 0.4f);
            statusRT.anchorMax = new Vector2(0.9f, 0.6f);
            statusRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI statusTMP = statusObj.AddComponent<TextMeshProUGUI>();
            statusTMP.text = "Loading...";
            statusTMP.fontSize = FontSizes.H4;
            statusTMP.color = TEXT_GOLD;
            statusTMP.alignment = TextAlignmentOptions.Center;
            statusTMP.fontStyle = FontStyles.Bold;
        }

        #endregion

        #region Starting Overlay (fullscreen, hidden)

        private static void CreateStartingOverlay(Transform parent)
        {
            GameObject overlay = new GameObject("StartingOverlay");
            overlay.transform.SetParent(parent, false);
            overlay.SetActive(false);

            RectTransform rt = overlay.AddComponent<RectTransform>();
            SetFullStretch(rt);

            Image bg = overlay.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.9f);
            bg.raycastTarget = true;

            // Tournament Starting text
            GameObject titleObj = new GameObject("StartingTitle");
            titleObj.transform.SetParent(overlay.transform, false);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.05f, 0.5f);
            titleRT.anchorMax = new Vector2(0.95f, 0.7f);
            titleRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "Tournament Starting!";
            titleTMP.fontSize = FontSizes.H3;
            titleTMP.color = TEXT_GOLD;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.fontStyle = FontStyles.Bold;

            // Countdown
            GameObject countdownObj = new GameObject("StartingCountdownText");
            countdownObj.transform.SetParent(overlay.transform, false);

            RectTransform cdRT = countdownObj.AddComponent<RectTransform>();
            cdRT.anchorMin = new Vector2(0.2f, 0.3f);
            cdRT.anchorMax = new Vector2(0.8f, 0.5f);
            cdRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI cdTMP = countdownObj.AddComponent<TextMeshProUGUI>();
            cdTMP.text = "3";
            cdTMP.fontSize = FontSizes.H1;
            cdTMP.color = GOLD_PRIMARY;
            cdTMP.alignment = TextAlignmentOptions.Center;
            cdTMP.fontStyle = FontStyles.Bold;
        }

        #endregion

        #region Helpers

        private static GameObject CreateElement(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.sizeDelta = Vector2.zero;

            return obj;
        }

        private static void SetFullStretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
        }

        #endregion

        #region Setup Manager References (post-build auto-wire)

        private static void SetupManagerReferences()
        {
            var manager = FindLobbyManager();
            if (manager == null)
            {
                Debug.Log("[CashTournamentLobbyUIBuilder] Manager no encontrado. Asigna referencias manualmente o usa el assigner.");
                return;
            }

            ResetAssignState();
            RunAssignAllReferences();

            Debug.Log($"[CashTournamentLobbyUIBuilder] Auto-wire: {assignedCount} asignadas, {alreadySetCount} ya puestas, {failedCount} fallidas");
        }

        #endregion

        #region Reference Assigner

        private static MonoBehaviour FindLobbyManager()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "CashTournamentLobbyManager") return mb;
            return null;
        }

        private static void ResetAssignState()
        {
            assignedCount = 0; failedCount = 0; alreadySetCount = 0;
            assignResults.Clear();
        }

        private static void RunAssignAllReferences()
        {
            var manager = FindLobbyManager();
            if (manager == null)
            {
                Debug.LogError("[CashTournamentLobby] CashTournamentLobbyManager no encontrado!");
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            so.Update();

            // Header
            AssignRef(so, "backButton", FindBtn("backbutton"));
            AssignRef(so, "tournamentNameText", FindText("tournamentnametext"));
            AssignRef(so, "statusBadgeText", FindText("statusbadgetext"));
            AssignRef(so, "statusBadgeImage", FindImg("statusbadge"));

            // Tournament Info
            AssignRef(so, "gameTypeText", FindText("gametypetext"));
            AssignRef(so, "entryFeeText", FindText("entryfeetext"));
            AssignRef(so, "prizePoolText", FindText("prizepooltext"));
            AssignRef(so, "playersProgressBar", FindImg("progressbarfill"));
            AssignRef(so, "playersProgressText", FindText("playersprogresstext"));
            AssignRef(so, "countdownText", FindText("countdowntext"));

            // Rules
            AssignRef(so, "attemptsRuleText", FindText("attemptsruletext"));
            AssignRef(so, "timeLimitRuleText", FindText("timelimitruletext"));

            // Prize Distribution
            AssignRef(so, "prizeDistributionContainer", FindComp<Transform>("prizerowscontainer"));

            // Tabs
            AssignRef(so, "participantsTabButton", FindBtn("participantstab"));
            AssignRef(so, "chatTabButton", FindBtn("chattab"));
            AssignGO(so, "participantsContent", "ParticipantsContent");
            AssignGO(so, "chatContent", "ChatContent");
            AssignRef(so, "participantsTabIndicator", FindImg("participantstabindicator"));
            AssignRef(so, "chatTabIndicator", FindImg("chattabindicator"));
            AssignRef(so, "chatBadgeText", FindText("chatbadgetext"));

            // Participants
            AssignRef(so, "participantsContainer", FindComp<Transform>("participantscontainer"));

            // Chat
            AssignRef(so, "chatScrollRect", FindComp<ScrollRect>("chatscrollrect"));
            AssignRef(so, "chatMessagesContainer", FindComp<Transform>("chatmessagescontainer"));
            AssignRef(so, "chatInput", FindComp<TMP_InputField>("chatinput"));
            AssignRef(so, "sendChatButton", FindBtn("sendchatbutton"));

            // Actions
            AssignRef(so, "joinButton", FindBtn("joinbutton"));
            AssignRef(so, "leaveButton", FindBtn("leavebutton"));
            AssignRef(so, "shareButton", FindBtn("sharebutton"));
            AssignRef(so, "joinButtonText", FindText("joinbuttontext"));

            // Status
            AssignGO(so, "loadingOverlay", "LoadingOverlay");
            AssignRef(so, "statusText", FindText("statustext"));
            AssignGO(so, "startingOverlay", "StartingOverlay");
            AssignRef(so, "startingCountdownText", FindText("startingcountdowntext"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(manager.gameObject);
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);

            Debug.Log($"[CashTournamentLobby] Referencias: {assignedCount} asignadas, {alreadySetCount} ya puestas, {failedCount} fallidas");
        }

        private static void AssignRef(SerializedObject so, string prop, Object value)
        {
            var p = so.FindProperty(prop);
            if (p == null) { AddAR(prop, "Propiedad no existe", false, null); failedCount++; return; }
            if (p.objectReferenceValue != null) { AddAR(prop, "Ya asignada", true, p.objectReferenceValue); alreadySetCount++; return; }
            if (value != null) { p.objectReferenceValue = value; AddAR(prop, "Asignada", true, value); assignedCount++; }
            else { AddAR(prop, "No encontrada", false, null); failedCount++; }
        }

        private static void AssignGO(SerializedObject so, string prop, params string[] patterns)
        {
            var p = so.FindProperty(prop);
            if (p == null) { AddAR(prop, "Propiedad no existe", false, null); failedCount++; return; }
            if (p.objectReferenceValue != null) { AddAR(prop, "Ya asignada", true, p.objectReferenceValue); alreadySetCount++; return; }
            var all = Object.FindObjectsOfType<Transform>(true);
            foreach (var pat in patterns)
                foreach (var t in all)
                    if (t.gameObject.name.ToLower().Contains(pat.ToLower()))
                    {
                        p.objectReferenceValue = t.gameObject;
                        AddAR(prop, "Asignada", true, t.gameObject);
                        assignedCount++;
                        return;
                    }
            AddAR(prop, "No encontrada", false, null); failedCount++;
        }

        private static T FindComp<T>(params string[] patterns) where T : Component
        {
            var all = Object.FindObjectsOfType<T>(true);
            foreach (var pat in patterns) foreach (var o in all) if (o.gameObject.name.ToLower().Contains(pat.ToLower())) return o;
            return null;
        }

        private static TextMeshProUGUI FindText(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (var p in patterns) foreach (var t in all) if (t.gameObject.name.ToLower().Contains(p.ToLower())) return t;
            return null;
        }

        private static Button FindBtn(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<Button>(true);
            foreach (var p in patterns) foreach (var b in all) if (b.gameObject.name.ToLower().Contains(p.ToLower())) return b;
            return null;
        }

        private static Image FindImg(params string[] patterns)
        {
            var all = Object.FindObjectsOfType<Image>(true);
            foreach (var p in patterns) foreach (var i in all) if (i.gameObject.name.ToLower().Contains(p.ToLower())) return i;
            return null;
        }

        private static void AddAR(string f, string s, bool ok, Object o)
        {
            assignResults.Add(new AssignResult { fieldName = f, status = s, success = ok, assignedObject = o });
        }

        private void DrawAssignResults()
        {
            if (assignResults.Count == 0) return;

            EditorGUILayout.Space(10);
            int total = assignResults.Count;
            int successTotal = assignedCount + alreadySetCount;
            float rate = (float)successTotal / total;

            GUI.color = rate == 1f ? new Color(0.2f, 0.8f, 0.2f) :
                        rate >= 0.7f ? new Color(1f, 0.8f, 0.2f) : new Color(1f, 0.4f, 0.4f);
            GUILayout.Label(rate == 1f ? "TODAS LAS REFERENCIAS ASIGNADAS" : "Algunas referencias faltan", EditorStyles.boldLabel);
            GUI.color = Color.white;

            GUILayout.Label($"Asignadas: {assignedCount} | Ya puestas: {alreadySetCount} | Fallidas: {failedCount}");
            EditorGUILayout.Space(5);

            foreach (var r in assignResults)
            {
                EditorGUILayout.BeginHorizontal();
                GUI.color = r.success ? (r.status == "Ya asignada" ? new Color(0.5f, 0.8f, 1f) : Color.green) : Color.red;
                GUILayout.Label(r.success ? (r.status == "Ya asignada" ? "o" : "+") : "x", GUILayout.Width(16));
                GUI.color = Color.white;
                GUILayout.Label(r.fieldName, GUILayout.Width(200));
                GUILayout.Label(r.status, GUILayout.Width(120));
                if (r.assignedObject != null)
                    EditorGUILayout.ObjectField(r.assignedObject, typeof(Object), true, GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();
            }
        }

        #endregion
    }
}
