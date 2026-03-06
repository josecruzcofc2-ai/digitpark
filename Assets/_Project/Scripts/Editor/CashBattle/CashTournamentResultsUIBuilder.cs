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
    /// UI Builder para el panel overlay CashTournamentResults.
    /// Crea un panel de resultados de torneo que se instancia como overlay
    /// dentro de CashTournamentLobby u otra escena.
    /// NO es una escena completa, es un panel overlay con CanvasGroup para fade-in/out.
    /// </summary>
    public class CashTournamentResultsUIBuilder : EditorWindow
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

        [MenuItem("DigitPark/UI Builders/CashBattle/CashTournament Results Panel", false, 186)]
        public static void ShowWindow()
        {
            GetWindow<CashTournamentResultsUIBuilder>("CashTournament Results Builder");
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // ========== SECCION 1: UI BUILDER ==========
            GUILayout.Label("CashTournament Results Panel Builder", EditorStyles.boldLabel);
            GUILayout.Label("Panel overlay de resultados de torneo (no es una escena)", EditorStyles.miniLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Crea un PANEL OVERLAY de resultados dentro del Canvas activo:\n\n" +
                "- DimBackground (fullscreen oscuro)\n" +
                "- Content card con header, stats, posicion, premio y botones\n" +
                "- CanvasGroup para animaciones fade-in/out\n" +
                "- Se usa como prefab o instanciado dentro de CashTournamentLobby\n\n" +
                "NOTA: Este builder agrega el panel al Canvas existente, NO crea una escena nueva.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            GUI.backgroundColor = GOLD_PRIMARY;
            if (GUILayout.Button("CREAR PANEL DE RESULTADOS", GUILayout.Height(40)))
            {
                BuildResultsPanel();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Eliminar Panel Existente", GUILayout.Height(25)))
            {
                RemoveExistingPanel();
            }

            // ========== SEPARADOR ==========
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // ========== SECCION 2: REFERENCE ASSIGNER ==========
            GUILayout.Label("Asignar Referencias", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            MonoBehaviour targetController = FindResultsController();
            if (targetController != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Controller:", GUILayout.Width(70));
                EditorGUILayout.ObjectField(targetController, typeof(MonoBehaviour), true);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("CashTournamentResultsPanelController no encontrado en escena.", MessageType.Warning);
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

        private static void BuildResultsPanel()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontro Canvas. Abre una escena con Canvas primero.", "OK");
                return;
            }

            // Remove existing panel if present
            Transform existing = canvas.transform.Find("ResultPanel");
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("Panel existente",
                    "Ya existe un ResultPanel. Reemplazarlo?", "Si, Reemplazar", "Cancelar"))
                    return;
                DestroyImmediate(existing.gameObject);
            }

            BuildAllElements(canvas.transform);
            SetupControllerReferences();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[CashTournamentResultsUIBuilder] Panel de resultados creado exitosamente!");
        }

        /// <summary>
        /// Builds the UI silently without confirmation dialogs. Used by batch builders.
        /// </summary>
        public static void BuildSilent()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("[CashTournamentResultsUIBuilder] Canvas not found - cannot build silently");
                return;
            }

            // Remove existing panel if present
            Transform existing = canvas.transform.Find("ResultPanel");
            if (existing != null)
                DestroyImmediate(existing.gameObject);

            BuildAllElements(canvas.transform);
            SetupControllerReferences();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("[CashTournamentResultsUIBuilder] UI built silently (batch mode)");
        }

        private static void RemoveExistingPanel()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null) return;

            Transform existing = canvas.transform.Find("ResultPanel");
            if (existing != null)
            {
                DestroyImmediate(existing.gameObject);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                Debug.Log("[CashTournamentResultsUIBuilder] Panel eliminado.");
            }
            else
            {
                Debug.Log("[CashTournamentResultsUIBuilder] No se encontro ResultPanel para eliminar.");
            }
        }

        private static void BuildAllElements(Transform canvasTransform)
        {
            // ResultPanel root (fullscreen overlay)
            GameObject resultPanel = new GameObject("ResultPanel");
            resultPanel.transform.SetParent(canvasTransform, false);

            RectTransform panelRT = resultPanel.AddComponent<RectTransform>();
            SetFullStretch(panelRT);

            // CanvasGroup for fade-in/out (start at alpha=0)
            CanvasGroup cg = resultPanel.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;

            // 1. DimBackground
            CreateDimBackground(resultPanel.transform);

            // 2. Content Card
            GameObject content = CreateContentCard(resultPanel.transform);

            // 3. Header Section
            CreateHeaderSection(content.transform);

            // 4. Stats Section
            CreateStatsSection(content.transform);

            // 5. Position Section
            CreatePositionSection(content.transform);

            // 6. Prize Section
            CreatePrizeSection(content.transform);

            // 7. Buttons Row
            CreateButtonsRow(content.transform);

            // Start hidden
            resultPanel.SetActive(false);
        }

        #endregion

        #region DimBackground

        private static void CreateDimBackground(Transform parent)
        {
            GameObject dimBg = new GameObject("DimBackground");
            dimBg.transform.SetParent(parent, false);

            RectTransform rt = dimBg.AddComponent<RectTransform>();
            SetFullStretch(rt);

            Image img = dimBg.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.85f);
            img.raycastTarget = true;
        }

        #endregion

        #region Content Card (0.05-0.95x, 0.15-0.85y)

        private static GameObject CreateContentCard(Transform parent)
        {
            GameObject content = new GameObject("Content");
            content.transform.SetParent(parent, false);

            RectTransform rt = content.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.05f, 0.15f);
            rt.anchorMax = new Vector2(0.95f, 0.85f);
            rt.sizeDelta = Vector2.zero;

            Image bg = content.AddComponent<Image>();
            bg.color = CARD_BG;

            // Gold border outline
            Outline outline = content.AddComponent<Outline>();
            outline.effectColor = GOLD_PRIMARY;
            outline.effectDistance = new Vector2(2, -2);

            return content;
        }

        #endregion

        #region Header Section (top area of content)

        private static void CreateHeaderSection(Transform parent)
        {
            GameObject header = CreateElement(parent, "HeaderSection",
                new Vector2(0, 0.78f), new Vector2(1, 1));

            // Trophy Icon (centered, 80x80)
            GameObject trophyObj = new GameObject("TrophyIcon");
            trophyObj.transform.SetParent(header.transform, false);

            RectTransform trophyRT = trophyObj.AddComponent<RectTransform>();
            trophyRT.anchorMin = new Vector2(0.5f, 0.55f);
            trophyRT.anchorMax = new Vector2(0.5f, 0.55f);
            trophyRT.pivot = new Vector2(0.5f, 0.5f);
            trophyRT.sizeDelta = new Vector2(80, 80);
            trophyRT.anchoredPosition = new Vector2(0, 10);

            Image trophyImg = trophyObj.AddComponent<Image>();
            trophyImg.color = GOLD_PRIMARY;

            // Result Title Text
            GameObject titleObj = CreateElement(header.transform, "CashResultTitleText",
                new Vector2(0.05f, 0.15f), new Vector2(0.95f, 0.5f));
            TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "Tournament Complete!";
            titleTMP.fontSize = FontSizes.H4;
            titleTMP.color = TEXT_GOLD;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.enableAutoSizing = true;
            titleTMP.fontSizeMin = FontSizes.AutoMinBody;
            titleTMP.fontSizeMax = FontSizes.H4;

            // Result Subtitle Text
            GameObject subtitleObj = CreateElement(header.transform, "ResultSubtitleText",
                new Vector2(0.1f, 0), new Vector2(0.9f, 0.18f));
            TextMeshProUGUI subtitleTMP = subtitleObj.AddComponent<TextMeshProUGUI>();
            subtitleTMP.text = "QuickMath Championship";
            subtitleTMP.fontSize = FontSizes.BodyLarge;
            subtitleTMP.color = TEXT_SECONDARY;
            subtitleTMP.alignment = TextAlignmentOptions.Center;
            subtitleTMP.fontStyle = FontStyles.Bold;
        }

        #endregion

        #region Stats Section (horizontal row, 3 stat boxes)

        private static void CreateStatsSection(Transform parent)
        {
            GameObject statsSection = CreateElement(parent, "StatsSection",
                new Vector2(0.03f, 0.62f), new Vector2(0.97f, 0.78f));

            HorizontalLayoutGroup hlg = statsSection.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.padding = new RectOffset(10, 10, 5, 5);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // Time Stat
            CreateStatBox(statsSection.transform, "TimeStat", "TimeText", "0:00", "Time");

            // Errors Stat
            CreateStatBox(statsSection.transform, "ErrorsStat", "ErrorsText", "0", "Errors");

            // Score Stat
            CreateStatBox(statsSection.transform, "ScoreStat", "ScoreText", "0", "Score");
        }

        private static void CreateStatBox(Transform parent, string boxName, string valueName, string defaultValue, string label)
        {
            GameObject box = new GameObject(boxName);
            box.transform.SetParent(parent, false);

            LayoutElement le = box.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;

            Image bg = box.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.07f, 0.12f, 0.8f);

            // Icon placeholder
            GameObject icon = new GameObject("Icon");
            icon.transform.SetParent(box.transform, false);

            RectTransform iconRT = icon.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.6f);
            iconRT.anchorMax = new Vector2(0.5f, 0.6f);
            iconRT.pivot = new Vector2(0.5f, 0.5f);
            iconRT.sizeDelta = new Vector2(30, 30);

            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = TEXT_GOLD;

            // Value text
            GameObject valueObj = new GameObject(valueName);
            valueObj.transform.SetParent(box.transform, false);

            RectTransform valueRT = valueObj.AddComponent<RectTransform>();
            valueRT.anchorMin = new Vector2(0, 0.15f);
            valueRT.anchorMax = new Vector2(1, 0.55f);
            valueRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI valueTMP = valueObj.AddComponent<TextMeshProUGUI>();
            valueTMP.text = defaultValue;
            valueTMP.fontSize = FontSizes.Body;
            valueTMP.color = TEXT_PRIMARY;
            valueTMP.alignment = TextAlignmentOptions.Center;
            valueTMP.fontStyle = FontStyles.Bold;

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(box.transform, false);

            RectTransform labelRT = labelObj.AddComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0);
            labelRT.anchorMax = new Vector2(1, 0.18f);
            labelRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI labelTMP = labelObj.AddComponent<TextMeshProUGUI>();
            labelTMP.text = label;
            labelTMP.fontSize = FontSizes.Body;
            labelTMP.fontStyle = FontStyles.Bold;
            labelTMP.color = TEXT_SECONDARY;
            labelTMP.alignment = TextAlignmentOptions.Center;
            labelTMP.enableAutoSizing = true;
            labelTMP.fontSizeMin = FontSizes.AutoMinBody;
            labelTMP.fontSizeMax = FontSizes.Body;
        }

        #endregion

        #region Position Section (centered, large area)

        private static void CreatePositionSection(Transform parent)
        {
            GameObject posSection = CreateElement(parent, "PositionSection",
                new Vector2(0.05f, 0.35f), new Vector2(0.95f, 0.62f));

            // Position Text (#1) - large
            GameObject posTextObj = CreateElement(posSection.transform, "PositionText",
                new Vector2(0.1f, 0.55f), new Vector2(0.9f, 1));
            TextMeshProUGUI posTMP = posTextObj.AddComponent<TextMeshProUGUI>();
            posTMP.text = "#1";
            posTMP.fontSize = FontSizes.H2;
            posTMP.color = GOLD_PRIMARY;
            posTMP.alignment = TextAlignmentOptions.Center;
            posTMP.fontStyle = FontStyles.Bold;

            // Position Label (1st Place)
            GameObject posLabelObj = CreateElement(posSection.transform, "CashPositionLabel",
                new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.55f));
            TextMeshProUGUI posLabelTMP = posLabelObj.AddComponent<TextMeshProUGUI>();
            posLabelTMP.text = "1st Place";
            posLabelTMP.fontSize = FontSizes.BodyLarge;
            posLabelTMP.color = TEXT_GOLD;
            posLabelTMP.alignment = TextAlignmentOptions.Center;
            posLabelTMP.fontStyle = FontStyles.Bold;

            // Attempts Text
            GameObject attemptsObj = CreateElement(posSection.transform, "AttemptsText",
                new Vector2(0.05f, 0.12f), new Vector2(0.95f, 0.35f));
            TextMeshProUGUI attemptsTMP = attemptsObj.AddComponent<TextMeshProUGUI>();
            attemptsTMP.text = "Attempts: 2/3";
            attemptsTMP.fontSize = FontSizes.Body;
            attemptsTMP.color = TEXT_SECONDARY;
            attemptsTMP.alignment = TextAlignmentOptions.Center;
            attemptsTMP.fontStyle = FontStyles.Bold;

            // Best Time Text
            GameObject bestTimeObj = CreateElement(posSection.transform, "BestTimeText",
                new Vector2(0.05f, 0), new Vector2(0.95f, 0.15f));
            TextMeshProUGUI bestTimeTMP = bestTimeObj.AddComponent<TextMeshProUGUI>();
            bestTimeTMP.text = "Best: 1:23.45";
            bestTimeTMP.fontSize = FontSizes.Body;
            bestTimeTMP.color = TEXT_SECONDARY;
            bestTimeTMP.alignment = TextAlignmentOptions.Center;
            bestTimeTMP.fontStyle = FontStyles.Bold;
        }

        #endregion

        #region Prize Section (centered, hidden by default)

        private static void CreatePrizeSection(Transform parent)
        {
            GameObject prizeSection = new GameObject("PrizeSection");
            prizeSection.transform.SetParent(parent, false);
            prizeSection.SetActive(false); // Hidden by default

            RectTransform rt = prizeSection.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.1f, 0.20f);
            rt.anchorMax = new Vector2(0.9f, 0.35f);
            rt.sizeDelta = Vector2.zero;

            // Prize Label
            GameObject prizeLabelObj = CreateElement(prizeSection.transform, "PrizeLabel",
                new Vector2(0, 0.55f), new Vector2(1, 1));
            TextMeshProUGUI prizeLabelTMP = prizeLabelObj.AddComponent<TextMeshProUGUI>();
            prizeLabelTMP.text = "You Won!";
            prizeLabelTMP.fontSize = FontSizes.BodyLarge;
            prizeLabelTMP.color = TEXT_GOLD;
            prizeLabelTMP.alignment = TextAlignmentOptions.Center;
            prizeLabelTMP.fontStyle = FontStyles.Bold;

            // Prize Text (amount)
            GameObject prizeTextObj = CreateElement(prizeSection.transform, "PrizeText",
                new Vector2(0, 0), new Vector2(1, 0.55f));
            TextMeshProUGUI prizeTMP = prizeTextObj.AddComponent<TextMeshProUGUI>();
            prizeTMP.text = "$25.00";
            prizeTMP.fontSize = FontSizes.H3;
            prizeTMP.color = GOLD_PRIMARY;
            prizeTMP.alignment = TextAlignmentOptions.Center;
            prizeTMP.fontStyle = FontStyles.Bold;
            prizeTMP.enableAutoSizing = true;
            prizeTMP.fontSizeMin = FontSizes.AutoMinBody;
            prizeTMP.fontSizeMax = FontSizes.H3;
        }

        #endregion

        #region Buttons Row (bottom, HorizontalLayoutGroup)

        private static void CreateButtonsRow(Transform parent)
        {
            GameObject buttonsRow = CreateElement(parent, "ButtonsRow",
                new Vector2(0.03f, 0.02f), new Vector2(0.97f, 0.18f));

            HorizontalLayoutGroup hlg = buttonsRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.padding = new RectOffset(8, 8, 6, 6);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // Retry Button (BUTTON_GOLD bg, dark text)
            CreateActionButton(buttonsRow.transform, "RetryButton", "CashRetryButtonText",
                "Play Again", BUTTON_GOLD, BG_DARK);

            // Leaderboard Button (outline GOLD_DARK, TEXT_GOLD text)
            CreateOutlineButton(buttonsRow.transform, "LeaderboardButton", "LeaderboardButtonText",
                "Leaderboard", GOLD_DARK, TEXT_GOLD);

            // Exit Button (outline TEXT_SECONDARY, TEXT_SECONDARY text)
            CreateOutlineButton(buttonsRow.transform, "ExitButton", "CashExitButtonText",
                "Exit", TEXT_SECONDARY, TEXT_SECONDARY);
        }

        private static void CreateActionButton(Transform parent, string name, string textName,
            string label, Color bgColor, Color textColor)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;

            Image bg = btnObj.AddComponent<Image>();
            bg.color = bgColor;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;

            GameObject textObj = new GameObject(textName);
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            SetFullStretch(textRT);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = FontSizes.Body;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinBody;
            tmp.fontSizeMax = FontSizes.Body;
        }

        private static void CreateOutlineButton(Transform parent, string name, string textName,
            string label, Color outlineColor, Color textColor)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;

            Image bg = btnObj.AddComponent<Image>();
            bg.color = Color.clear;

            Outline outline = btnObj.AddComponent<Outline>();
            outline.effectColor = outlineColor;
            outline.effectDistance = new Vector2(2, -2);

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;

            GameObject textObj = new GameObject(textName);
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            SetFullStretch(textRT);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = FontSizes.Body;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinBody;
            tmp.fontSizeMax = FontSizes.Body;
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

        #region Setup Controller References (post-build auto-wire)

        private static void SetupControllerReferences()
        {
            var controller = FindResultsController();
            if (controller == null)
            {
                Debug.Log("[CashTournamentResultsUIBuilder] Controller no encontrado. Asigna referencias manualmente o usa el assigner.");
                return;
            }

            ResetAssignState();
            RunAssignAllReferences();

            Debug.Log($"[CashTournamentResultsUIBuilder] Auto-wire: {assignedCount} asignadas, {alreadySetCount} ya puestas, {failedCount} fallidas");
        }

        #endregion

        #region Reference Assigner

        private static MonoBehaviour FindResultsController()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "CashTournamentResultsPanelController") return mb;
            return null;
        }

        private static void ResetAssignState()
        {
            assignedCount = 0; failedCount = 0; alreadySetCount = 0;
            assignResults.Clear();
        }

        private static void RunAssignAllReferences()
        {
            var controller = FindResultsController();
            if (controller == null)
            {
                Debug.LogError("[CashTournamentResults] CashTournamentResultsPanelController no encontrado!");
                return;
            }

            SerializedObject so = new SerializedObject(controller);
            so.Update();

            // Find the ResultPanel root
            Transform panelRoot = controller.transform;

            // Main Panel
            AssignRef(so, "canvasGroup", FindCompOnTransform<CanvasGroup>(panelRoot));

            Transform contentT = FindDeep(panelRoot, "Content");
            AssignGO(so, "content", contentT);

            // Header
            AssignRef(so, "titleText", FindText("resulttitletext"));
            AssignRef(so, "subtitleText", FindText("resultsubtitletext"));
            AssignRef(so, "trophyIcon", FindImg("trophyicon"));

            // Stats
            AssignRef(so, "timeText", FindText("timetext"));
            AssignRef(so, "errorsText", FindText("errorstext"));
            AssignRef(so, "scoreText", FindText("scoretext"));

            // Position
            AssignRef(so, "positionText", FindText("positiontext"));
            AssignRef(so, "positionLabel", FindText("positionlabel"));
            AssignRef(so, "attemptsText", FindText("attemptstext"));
            AssignRef(so, "bestTimeText", FindText("besttimetext"));

            // Prize
            Transform prizeSectionT = FindDeep(panelRoot, "PrizeSection");
            AssignGO(so, "prizeSection", prizeSectionT);
            AssignRef(so, "prizeText", FindText("prizetext"));
            AssignRef(so, "prizeLabel", FindText("prizelabel"));

            // Buttons
            AssignRef(so, "retryButton", FindBtn("retrybutton"));
            AssignRef(so, "leaderboardButton", FindBtn("leaderboardbutton"));
            AssignRef(so, "exitButton", FindBtn("exitbutton"));
            AssignRef(so, "retryButtonText", FindText("retrybuttontext"));
            AssignRef(so, "leaderboardButtonText", FindText("leaderboardbuttontext"));
            AssignRef(so, "exitButtonText", FindText("exitbuttontext"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(controller.gameObject);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);

            Debug.Log($"[CashTournamentResults] Referencias: {assignedCount} asignadas, {alreadySetCount} ya puestas, {failedCount} fallidas");
        }

        private static void AssignRef(SerializedObject so, string prop, Object value)
        {
            var p = so.FindProperty(prop);
            if (p == null) { AddAR(prop, "Propiedad no existe", false, null); failedCount++; return; }
            if (p.objectReferenceValue != null) { AddAR(prop, "Ya asignada", true, p.objectReferenceValue); alreadySetCount++; return; }
            if (value != null) { p.objectReferenceValue = value; AddAR(prop, "Asignada", true, value); assignedCount++; }
            else { AddAR(prop, "No encontrada", false, null); failedCount++; }
        }

        private static void AssignGO(SerializedObject so, string prop, Transform t)
        {
            var p = so.FindProperty(prop);
            if (p == null) { AddAR(prop, "Propiedad no existe", false, null); failedCount++; return; }
            if (p.objectReferenceValue != null) { AddAR(prop, "Ya asignada", true, p.objectReferenceValue); alreadySetCount++; return; }
            if (t != null) { p.objectReferenceValue = t.gameObject; AddAR(prop, "Asignada", true, t.gameObject); assignedCount++; }
            else { AddAR(prop, "No encontrada", false, null); failedCount++; }
        }

        private static T FindCompOnTransform<T>(Transform t) where T : Component
        {
            return t != null ? t.GetComponent<T>() : null;
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                Transform result = FindDeep(child, name);
                if (result != null) return result;
            }
            return null;
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
