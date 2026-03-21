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
    /// UI Builder para la escena CashProfile.unity
    /// Redise\u00f1o "Competitive Gamer" — Neon Gold.
    ///
    /// Layout:
    ///   Header (100px) FIJO arriba \u2014 BackButton + Titulo + BalanceWidget
    ///   \u2500\u2500\u2500 gold glow separator \u2500\u2500\u2500
    ///   [ScrollView rellena el resto]
    ///     PlayerCard (280px)     \u2014 Username GOLD + rank badge + earnings bar
    ///     RecordCard (170px)     \u2014 W\u00b7L\u00b7D + win rate progress bar
    ///     StreakCards (120px)    \u2014 Racha Actual + Mejor Racha (side-by-side)
    ///     Section "STATS POR JUEGO"
    ///     GameStatsCard (420px) \u2014 5 game rows con progress bars + accent colors
    ///
    /// Auto-asigna referencias al CashProfileSceneController.
    /// Menu: DigitPark/UI Builders/CashBattle/Cash Profile
    /// </summary>
    public class CashProfileUIBuilder : EditorWindow
    {
        #region Palette \u2014 Neon Gold

        // === GOLDS ===
        private static readonly Color GOLD_PRIMARY      = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GOLD_DARK         = new Color(0.85f, 0.65f, 0.13f, 1f);
        private static readonly Color GOLD_LIGHT        = new Color(1f, 0.93f, 0.55f, 1f);
        private static readonly Color GOLD_GLOW         = new Color(1f, 0.84f, 0f, 0.35f);
        private static readonly Color GOLD_BORDER       = new Color(0.85f, 0.65f, 0.13f, 0.7f);
        private static readonly Color GOLD_BORDER_OUTER = new Color(1f, 0.84f, 0f, 0.25f);

        // === BACKGROUNDS ===
        private static readonly Color BG_DARK           = new Color(0.06f, 0.05f, 0.10f, 1f);
        private static readonly Color BG_HEADER         = new Color(0.04f, 0.03f, 0.08f, 0.95f);
        private static readonly Color CARD_BG           = new Color(0.10f, 0.08f, 0.14f, 0.97f);
        private static readonly Color CARD_BG_ELEVATED  = new Color(0.13f, 0.11f, 0.17f, 0.97f);

        // === TEXT ===
        private static readonly Color TEXT_WHITE        = new Color(1f, 1f, 1f, 1f);
        private static readonly Color TEXT_GOLD         = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color TEXT_MUTED        = new Color(0.55f, 0.53f, 0.60f, 1f);

        // === ACCENTS ===
        private static readonly Color ACCENT_GREEN      = new Color(0.25f, 1f, 0.50f, 1f);
        private static readonly Color ACCENT_RED        = new Color(1f, 0.35f, 0.35f, 1f);
        private static readonly Color ACCENT_CYAN       = new Color(0f, 0.90f, 1f, 1f);

        // === GAME COLORS (matching Profile Normal) ===
        private static readonly Color GAME_DIGITRUSH    = new Color(0f, 1f, 1f, 1f);
        private static readonly Color GAME_MEMORYPAIRS  = new Color(0.6f, 0.3f, 1f, 1f);
        private static readonly Color GAME_QUICKMATH    = new Color(0.25f, 1f, 0.5f, 1f);
        private static readonly Color GAME_FLASHTAP     = new Color(1f, 0.5f, 0f, 1f);
        private static readonly Color GAME_ODDONEOUT    = new Color(1f, 0.84f, 0f, 1f);

        // === BAR BACKGROUNDS ===
        private static readonly Color BAR_BG            = new Color(0.15f, 0.13f, 0.20f, 1f);

        #endregion

        #region Paths

        private const string BACK_BTN_PREFAB           = "Assets/_Project/Prefabs/Common/BackButtonGold.prefab";
        private const string BACK_ICON_GOLD_PATH        = "Assets/_Project/Art/Icons/Navigation/BackIconGold.png";
        // Avatar icons removed — CashProfileHeaderCard uses Username GOLD + Rank badge + earnings bar

        #endregion

        #region Assign State

        private Vector2 scrollPosition;
        private static int assignedCount, failedCount, alreadySetCount;
        private static List<AR> arList = new List<AR>();
        private struct AR { public string field, status; public bool ok; public Object obj; }

        #endregion

        // ==================== MENU ====================

        [MenuItem("DigitPark/Scenes/Build Scene/CashBattle/Profile", false, 183)]
        public static void ShowWindow() => GetWindow<CashProfileUIBuilder>("Cash Profile Builder");

        // ==================== EDITOR WINDOW ====================

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Cash Profile \u2014 Competitive Gamer", EditorStyles.boldLabel);
            GUILayout.Label("Perfil competitivo con rank, record y stats por juego", EditorStyles.miniLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Construye la UI completa para CashProfile.unity.\n\n" +
                "Dise\u00f1o Competitive Gamer (Neon Gold):\n" +
                "  \u2022 Header fijo + ScrollView con todo el contenido\n" +
                "  \u2022 PlayerCard: Username GOLD + rank badge + earnings bar\n" +
                "  \u2022 RecordCard: W\u00b7L\u00b7D + win rate progress bar\n" +
                "  \u2022 StreakCards: Racha actual + mejor racha\n" +
                "  \u2022 GameStats: 5 juegos con progress bars\n\n" +
                "Auto-asigna referencias al CashProfileSceneController.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            GUI.backgroundColor = GOLD_PRIMARY;
            if (GUILayout.Button("CONSTRUIR UI + AUTO-ASIGNAR", GUILayout.Height(45)))
            {
                BuildAndAssign();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // Manual assign button
            GUILayout.Label("Asignaci\u00f3n Manual", EditorStyles.boldLabel);

            string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (scene != "CashProfile")
                EditorGUILayout.HelpBox($"Escena actual: {scene}\nAbre CashProfile primero.", MessageType.Warning);

            MonoBehaviour ctrl = FindController();
            if (ctrl != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Controller:", GUILayout.Width(70));
                EditorGUILayout.ObjectField(ctrl, typeof(MonoBehaviour), true);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(5);

            GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
            if (GUILayout.Button("SOLO ASIGNAR REFERENCIAS", GUILayout.Height(34)))
            {
                ResetAR();
                RunAssignAll();
                Repaint();
            }
            GUI.backgroundColor = Color.white;

            DrawResults();
            EditorGUILayout.EndScrollView();
        }

        // ================================================================
        //  BUILD + AUTO-ASSIGN
        // ================================================================

        private static void BuildAndAssign()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontr\u00f3 Canvas.\nAbre la escena CashProfile primero.", "OK");
                return;
            }

            // Standardize CanvasScaler
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
            }

            if (!EditorUtility.DisplayDialog("Reconstruir UI?",
                "Esto reconstruir\u00e1 toda la UI de CashProfile con el dise\u00f1o Competitive Gamer " +
                "y auto-asignar\u00e1 las referencias.\n\n\u00bfContinuar?",
                "S\u00ed, Construir", "Cancelar")) return;

            // 1) Limpiar
            Cleanup(canvas.transform);

            // 2) Construir toda la UI
            BuildAll(canvas);

            // 2b) Animation managers
            CreateAnimationManagers();

            // 3) Auto-asignar referencias
            ResetAR();
            RunAssignAll();

            // 4) Marcar escena dirty
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            int ok = assignedCount + alreadySetCount;
            int total = ok + failedCount;
            Debug.Log($"[CashProfileUIBuilder] UI construida + {ok}/{total} referencias asignadas!");

            EditorUtility.DisplayDialog("Cash Profile Construido",
                $"UI Competitive Gamer construida exitosamente.\n\n" +
                $"Referencias: {ok}/{total} asignadas.\n" +
                (failedCount > 0 ? $"Fallidas: {failedCount}" : "\u00a1Todo OK!"),
                "OK");
        }

        /// <summary>
        /// Builds the UI silently without confirmation dialogs. Used by batch builders.
        /// </summary>
        public static void BuildSilent()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("[CashProfileUIBuilder] Canvas not found - cannot build silently");
                return;
            }

            // Standardize CanvasScaler
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
            }

            Cleanup(canvas.transform);
            BuildAll(canvas);
            CreateAnimationManagers();

            ResetAR();
            RunAssignAll();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("[CashProfileUIBuilder] UI built silently (batch mode)");
        }

        // ================================================================
        //  BUILD ALL ELEMENTS
        // ================================================================

        private static void BuildAll(Canvas canvas)
        {
            Transform root = canvas.transform;

            // Background
            CreateFullScreenBG(root);

            // SafeArea container
            GameObject safeArea = CreateSafeArea(root);
            Transform sa = safeArea.transform;

            // Header (FIJO arriba, fuera del scroll)
            CreateHeader(sa);

            // ScrollView que contiene TODO el contenido debajo del header
            GameObject scrollContent = CreateMainScrollView(sa);
            Transform content = scrollContent.transform;

            // === CONTENIDO DENTRO DEL SCROLL ===
            CreatePlayerCard(content);
            CreateRecordCard(content);
            CreateStreakCards(content);
            CreateSectionHeader(content, "STATS BY GAME");
            CreateGameStatsCard(content);
            // Bottom spacer
            CreateSpacer(content, 40);

            // Overlay panels (on canvas root, not inside scroll)
            BuildChangeNamePanel(canvas.transform);
            BuildErrorPanel(canvas.transform);
        }

        private static void Cleanup(Transform parent)
        {
            foreach (string name in new[] { "Background", "SafeArea", "ChangeNamePanel", "ErrorPanel", "BackButton", "BackButtonGold" })
            {
                Transform t = parent.Find(name);
                if (t != null) DestroyImmediate(t.gameObject);
            }
        }

        // ================================================================
        //  BACKGROUND
        // ================================================================

        private static void CreateFullScreenBG(Transform parent)
        {
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(parent, false);
            bg.transform.SetAsFirstSibling();

            RectTransform rt = bg.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image img = bg.AddComponent<Image>();
            img.color = BG_DARK;
        }

        // ================================================================
        //  SAFE AREA
        // ================================================================

        private static GameObject CreateSafeArea(Transform parent)
        {
            GameObject sa = new GameObject("SafeArea");
            sa.transform.SetParent(parent, false);

            RectTransform rt = sa.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            return sa;
        }

        // ================================================================
        //  HEADER (100px, fijo)
        // ================================================================

        private static void CreateHeader(Transform parent)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(parent, false);

            RectTransform rt = header.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, -29);
            rt.sizeDelta = new Vector2(0, 100);

            Image bg = header.AddComponent<Image>();
            bg.color = BG_HEADER;

            // === BackButton ===
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BACK_BTN_PREFAB);
            if (prefab != null)
            {
                GameObject btn = (GameObject)PrefabUtility.InstantiatePrefab(prefab, header.transform);
                btn.name = "BackButton";
                RectTransform brt = btn.GetComponent<RectTransform>();
                brt.anchorMin = new Vector2(0, 0.5f);
                brt.anchorMax = new Vector2(0, 0.5f);
                brt.pivot = new Vector2(0, 0.5f);
                brt.sizeDelta = new Vector2(50, 50);
                brt.anchoredPosition = new Vector2(20, 0);
                SetupButtonColorBlock(btn);

                // Assign BackIconGold sprite to Icon child
                Sprite backIcon = AssetDatabase.LoadAssetAtPath<Sprite>(BACK_ICON_GOLD_PATH);
                if (backIcon != null)
                {
                    Transform iconChild = btn.transform.Find("Icon");
                    if (iconChild != null)
                    {
                        Image iconImg = iconChild.GetComponent<Image>();
                        if (iconImg != null) iconImg.sprite = backIcon;
                    }
                }
            }
            else
            {
                // Fallback
                GameObject btn = new GameObject("BackButton");
                btn.transform.SetParent(header.transform, false);
                RectTransform brt = btn.AddComponent<RectTransform>();
                brt.anchorMin = new Vector2(0, 0.5f);
                brt.anchorMax = new Vector2(0, 0.5f);
                brt.pivot = new Vector2(0, 0.5f);
                brt.sizeDelta = new Vector2(50, 50);
                brt.anchoredPosition = new Vector2(20, 0);
                btn.AddComponent<Image>().color = new Color(1, 1, 1, 0);
                btn.AddComponent<Button>();
                SetupButtonColorBlock(btn);

                GameObject arrow = new GameObject("Arrow");
                arrow.transform.SetParent(btn.transform, false);
                RectTransform art = arrow.AddComponent<RectTransform>();
                art.anchorMin = Vector2.zero; art.anchorMax = Vector2.one; art.sizeDelta = Vector2.zero;
                Image atmp = arrow.AddComponent<Image>();
                Sprite arrowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/UI/icon_back_arrow.png");
                if (arrowSprite != null) atmp.sprite = arrowSprite;
                atmp.color = TEXT_WHITE;
                atmp.preserveAspect = true;
                atmp.raycastTarget = false;
            }

            // === Title ===
            GameObject title = new GameObject("CashProfileTitle");
            title.transform.SetParent(header.transform, false);
            RectTransform trt = title.AddComponent<RectTransform>();
            trt.anchorMin = new Vector2(0.07f, 0f);
            trt.anchorMax = new Vector2(0.53f, 1f);
            trt.pivot = new Vector2(0.5f, 0.5f);
            trt.sizeDelta = Vector2.zero;
            trt.anchoredPosition = Vector2.zero;

            TextMeshProUGUI ttmp = title.AddComponent<TextMeshProUGUI>();
            ttmp.text = "My Cash Profile";
            ttmp.fontSize = FontSizes.H4;
            ttmp.color = TEXT_GOLD;
            ttmp.alignment = TextAlignmentOptions.MidlineLeft;
            ttmp.raycastTarget = false;
            ttmp.fontStyle = FontStyles.Bold;
            ttmp.enableAutoSizing = true;
            ttmp.fontSizeMin = FontSizes.Subtitle;
            ttmp.fontSizeMax = FontSizes.H4;
            ttmp.outlineWidth = 0.15f;
            ttmp.outlineColor = new Color(0.5f, 0.35f, 0f, 0.5f);

            // === BalanceWidget ===
            CreateBalanceWidget(header.transform);
        }

        private static void CreateBalanceWidget(Transform parent)
        {
            GameObject w = new GameObject("BalanceWidget");
            w.transform.SetParent(parent, false);

            RectTransform rt = w.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(1, 0.5f);
            rt.sizeDelta = new Vector2(280, 60);
            rt.anchoredPosition = new Vector2(-12, 0);

            Image bg = w.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.06f, 0.04f, 0.85f);

            Outline ol = w.AddComponent<Outline>();
            ol.effectColor = GOLD_BORDER;
            ol.effectDistance = new Vector2(1, -1);

            GameObject txt = new GameObject("BalanceText");
            txt.transform.SetParent(w.transform, false);
            RectTransform trt = txt.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(12, 0); trt.offsetMax = new Vector2(-8, 0);

            TextMeshProUGUI tmp = txt.AddComponent<TextMeshProUGUI>();
            tmp.text = "$0.00";
            tmp.fontSize = FontSizes.Subtitle; tmp.color = TEXT_GOLD;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.enableAutoSizing = true; tmp.fontSizeMin = FontSizes.AutoMinBody; tmp.fontSizeMax = FontSizes.Subtitle;
        }

        // ================================================================
        //  GOLD SEPARATOR LINE
        // ================================================================

        private static void CreateGoldSeparator(Transform parent, float yPos)
        {
            GameObject sep = new GameObject("GoldSeparator");
            sep.transform.SetParent(parent, false);

            RectTransform rt = sep.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.03f, 1);
            rt.anchorMax = new Vector2(0.97f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(0, 3);
            rt.anchoredPosition = new Vector2(0, yPos);

            Image img = sep.AddComponent<Image>();
            img.color = GOLD_GLOW;

            Outline glow = sep.AddComponent<Outline>();
            glow.effectColor = new Color(1f, 0.84f, 0f, 0.15f);
            glow.effectDistance = new Vector2(0, -2);
        }

        // ================================================================
        //  MAIN SCROLLVIEW
        // ================================================================

        private static GameObject CreateMainScrollView(Transform parent)
        {
            GameObject sv = new GameObject("MainScrollView");
            sv.transform.SetParent(parent, false);

            RectTransform svRT = sv.AddComponent<RectTransform>();
            svRT.anchorMin = Vector2.zero;
            svRT.anchorMax = Vector2.one;
            svRT.offsetMin = new Vector2(0, 0);
            svRT.offsetMax = new Vector2(0, -105);

            ScrollRect scroll = sv.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 50;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.elasticity = 0.1f;

            Image svBg = sv.AddComponent<Image>();
            svBg.color = new Color(0, 0, 0, 0);
            svBg.raycastTarget = false;

            // Viewport
            GameObject vp = new GameObject("Viewport");
            vp.transform.SetParent(sv.transform, false);
            RectTransform vpRT = vp.AddComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero; vpRT.anchorMax = Vector2.one;
            vpRT.sizeDelta = Vector2.zero;

            Image vpImg = vp.AddComponent<Image>();
            vpImg.color = Color.clear;
            vpImg.raycastTarget = true;
            vp.AddComponent<RectMask2D>();

            // Content
            GameObject content = new GameObject("ScrollContent");
            content.transform.SetParent(vp.transform, false);
            RectTransform cRT = content.AddComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0, 1);
            cRT.anchorMax = new Vector2(1, 1);
            cRT.pivot = new Vector2(0.5f, 1);
            cRT.sizeDelta = new Vector2(0, 0);

            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 20;
            vlg.padding = new RectOffset(12, 12, 14, 24);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = vpRT;
            scroll.content = cRT;

            return content;
        }

        // ================================================================
        //  PLAYER CARD \u2014 Username GOLD + Rank Badge + Earnings Bar
        // ================================================================

        private static void CreatePlayerCard(Transform parent)
        {
            GameObject card = new GameObject("PlayerCard");
            card.transform.SetParent(parent, false);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredHeight = 280; le.flexibleWidth = 1;

            Image bg = card.AddComponent<Image>();
            bg.color = CARD_BG;

            Outline inner = card.AddComponent<Outline>();
            inner.effectColor = GOLD_BORDER;
            inner.effectDistance = new Vector2(2, -2);

            // === Gold accent line at top ===
            GameObject topLine = new GameObject("GoldTopLine");
            topLine.transform.SetParent(card.transform, false);
            RectTransform tlRT = topLine.AddComponent<RectTransform>();
            tlRT.anchorMin = new Vector2(0, 1); tlRT.anchorMax = new Vector2(1, 1);
            tlRT.pivot = new Vector2(0.5f, 1); tlRT.sizeDelta = new Vector2(0, 3);
            topLine.AddComponent<Image>().color = GOLD_PRIMARY;

            // === CashProfileHeaderCard (compact, no avatar) ===

            // === Username GOLD (top, centered) ===
            GameObject uname = new GameObject("UsernameText");
            uname.transform.SetParent(card.transform, false);
            RectTransform unRT = uname.AddComponent<RectTransform>();
            unRT.anchorMin = new Vector2(0.05f, 1f);
            unRT.anchorMax = new Vector2(0.95f, 1f);
            unRT.pivot = new Vector2(0.5f, 1f);
            unRT.sizeDelta = new Vector2(0, 55);
            unRT.anchoredPosition = new Vector2(0, -15);

            TextMeshProUGUI unTmp = uname.AddComponent<TextMeshProUGUI>();
            unTmp.text = "Player";
            unTmp.fontSize = FontSizes.H2; unTmp.color = GOLD_LIGHT;
            unTmp.alignment = TextAlignmentOptions.Center;
            unTmp.fontStyle = FontStyles.Bold;
            unTmp.enableAutoSizing = true;
            unTmp.fontSizeMin = FontSizes.Body; unTmp.fontSizeMax = FontSizes.H3;

            // === Member Since ===
            GameObject msince = new GameObject("MemberSinceText");
            msince.transform.SetParent(card.transform, false);
            RectTransform msRT = msince.AddComponent<RectTransform>();
            msRT.anchorMin = new Vector2(0.1f, 1f);
            msRT.anchorMax = new Vector2(0.9f, 1f);
            msRT.pivot = new Vector2(0.5f, 1f);
            msRT.sizeDelta = new Vector2(0, 36);
            msRT.anchoredPosition = new Vector2(0, -75);

            TextMeshProUGUI msTmp = msince.AddComponent<TextMeshProUGUI>();
            msTmp.text = "Member since 2024";
            msTmp.fontSize = FontSizes.Body; msTmp.color = TEXT_MUTED;
            msTmp.alignment = TextAlignmentOptions.Center;
            msTmp.fontStyle = FontStyles.Bold;
            msTmp.enableAutoSizing = true;
            msTmp.fontSizeMin = FontSizes.AutoMinBody;
            msTmp.fontSizeMax = msTmp.fontSize;

            // === Rank Badge (centered pill below member since) ===
            GameObject rankBadge = new GameObject("RankBadge");
            rankBadge.transform.SetParent(card.transform, false);
            RectTransform rbRT = rankBadge.AddComponent<RectTransform>();
            rbRT.anchorMin = new Vector2(0.5f, 1f);
            rbRT.anchorMax = new Vector2(0.5f, 1f);
            rbRT.pivot = new Vector2(0.5f, 1f);
            rbRT.sizeDelta = new Vector2(260, 50);
            rbRT.anchoredPosition = new Vector2(0, -115);
            Image rbBg = rankBadge.AddComponent<Image>();
            rbBg.color = new Color(GOLD_DARK.r * 0.25f, GOLD_DARK.g * 0.25f, GOLD_DARK.b * 0.25f, 0.9f);
            Outline rbOutline = rankBadge.AddComponent<Outline>();
            rbOutline.effectColor = GOLD_BORDER;
            rbOutline.effectDistance = new Vector2(1.5f, -1.5f);

            GameObject rankText = new GameObject("RankText");
            rankText.transform.SetParent(rankBadge.transform, false);
            RectTransform rkRT = rankText.AddComponent<RectTransform>();
            rkRT.anchorMin = Vector2.zero; rkRT.anchorMax = Vector2.one;
            rkRT.offsetMin = new Vector2(10, 0); rkRT.offsetMax = new Vector2(-10, 0);
            TextMeshProUGUI rkTmp = rankText.AddComponent<TextMeshProUGUI>();
            rkTmp.text = "Lv. 1 \u00b7 Beginner";
            rkTmp.fontSize = FontSizes.Body; rkTmp.color = GOLD_LIGHT;
            rkTmp.alignment = TextAlignmentOptions.Center;
            rkTmp.fontStyle = FontStyles.Bold;
            rkTmp.enableAutoSizing = true;
            rkTmp.fontSizeMin = FontSizes.Caption; rkTmp.fontSizeMax = FontSizes.Body;

            // === Earnings Bar (bottom of card) ===
            GameObject earningsBar = new GameObject("EarningsBar");
            earningsBar.transform.SetParent(card.transform, false);
            RectTransform ebRT = earningsBar.AddComponent<RectTransform>();
            ebRT.anchorMin = new Vector2(0.08f, 1f);
            ebRT.anchorMax = new Vector2(0.92f, 1f);
            ebRT.pivot = new Vector2(0.5f, 1f);
            ebRT.sizeDelta = new Vector2(0, 40);
            ebRT.anchoredPosition = new Vector2(0, -180);
            earningsBar.AddComponent<Image>().color = new Color(0.1f, 0.09f, 0.14f, 1f);

            GameObject earningsText = new GameObject("EarningsText");
            earningsText.transform.SetParent(earningsBar.transform, false);
            RectTransform etRT = earningsText.AddComponent<RectTransform>();
            etRT.anchorMin = Vector2.zero; etRT.anchorMax = Vector2.one;
            etRT.offsetMin = new Vector2(10, 0); etRT.offsetMax = new Vector2(-10, 0);
            TextMeshProUGUI etTmp = earningsText.AddComponent<TextMeshProUGUI>();
            etTmp.text = "Total Earnings: $0.00";
            etTmp.fontSize = FontSizes.Body; etTmp.color = GOLD_PRIMARY;
            etTmp.alignment = TextAlignmentOptions.Center;
            etTmp.fontStyle = FontStyles.Bold;
            etTmp.enableAutoSizing = true;
            etTmp.fontSizeMin = FontSizes.AutoMinBody;
            etTmp.fontSizeMax = FontSizes.Body;

        }

        // ================================================================
        //  RECORD CARD \u2014 W\u00b7L\u00b7D + Win Rate Progress Bar
        // ================================================================

        private static void CreateRecordCard(Transform parent)
        {
            GameObject card = new GameObject("RecordCard");
            card.transform.SetParent(parent, false);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredHeight = 300; le.flexibleWidth = 1;

            Image bg = card.AddComponent<Image>();
            bg.color = CARD_BG;

            Outline ol = card.AddComponent<Outline>();
            ol.effectColor = GOLD_BORDER;
            ol.effectDistance = new Vector2(2, -2);

            // Gold accent line
            GameObject topLine = new GameObject("GoldTopLine");
            topLine.transform.SetParent(card.transform, false);
            RectTransform tlRT = topLine.AddComponent<RectTransform>();
            tlRT.anchorMin = new Vector2(0, 1); tlRT.anchorMax = new Vector2(1, 1);
            tlRT.pivot = new Vector2(0.5f, 1); tlRT.sizeDelta = new Vector2(0, 3);
            topLine.AddComponent<Image>().color = GOLD_PRIMARY;

            // Section divider: ═══ RECORD GENERAL ═══
            GameObject divider = new GameObject("RecordDivider");
            divider.transform.SetParent(card.transform, false);
            RectTransform divRT = divider.AddComponent<RectTransform>();
            divRT.anchorMin = new Vector2(0, 0.82f); divRT.anchorMax = new Vector2(1, 1f);
            divRT.offsetMin = new Vector2(10, 0); divRT.offsetMax = new Vector2(-10, -6);
            HorizontalLayoutGroup divHLG = divider.AddComponent<HorizontalLayoutGroup>();
            divHLG.spacing = 12;
            divHLG.childAlignment = TextAnchor.MiddleCenter;
            divHLG.childForceExpandWidth = true;
            divHLG.childForceExpandHeight = true;
            divHLG.childControlWidth = true;
            divHLG.childControlHeight = true;

            GameObject ll = new GameObject("LeftLine");
            ll.transform.SetParent(divider.transform, false);
            LayoutElement llLE = ll.AddComponent<LayoutElement>();
            llLE.flexibleWidth = 1; llLE.preferredHeight = 2;
            ll.AddComponent<Image>().color = GOLD_GLOW;

            GameObject titObj = new GameObject("RecordCardTitle");
            titObj.transform.SetParent(divider.transform, false);
            LayoutElement titLE = titObj.AddComponent<LayoutElement>();
            titLE.flexibleWidth = 0; titLE.preferredWidth = 450;
            TextMeshProUGUI titTmp = titObj.AddComponent<TextMeshProUGUI>();
            titTmp.text = "OVERALL RECORD";
            titTmp.fontSize = FontSizes.Subtitle; titTmp.color = TEXT_GOLD;
            titTmp.alignment = TextAlignmentOptions.Center;
            titTmp.fontStyle = FontStyles.Bold;
            titTmp.characterSpacing = 6;
            titTmp.enableAutoSizing = true;
            titTmp.fontSizeMin = FontSizes.AutoMinBody;
            titTmp.fontSizeMax = titTmp.fontSize;

            GameObject rl = new GameObject("RightLine");
            rl.transform.SetParent(divider.transform, false);
            LayoutElement rlLE = rl.AddComponent<LayoutElement>();
            rlLE.flexibleWidth = 1; rlLE.preferredHeight = 2;
            rl.AddComponent<Image>().color = GOLD_GLOW;

            // Record text (big) \u2014 "0W  \u00b7  0L  \u00b7  0D"
            GameObject recObj = new GameObject("RecordText");
            recObj.transform.SetParent(card.transform, false);
            RectTransform recRT = recObj.AddComponent<RectTransform>();
            recRT.anchorMin = new Vector2(0, 0.42f); recRT.anchorMax = new Vector2(1, 0.82f);
            recRT.offsetMin = new Vector2(15, 0); recRT.offsetMax = new Vector2(-15, 0);

            TextMeshProUGUI recTmp = recObj.AddComponent<TextMeshProUGUI>();
            recTmp.text = "0W  \u00b7  0L  \u00b7  0D";
            recTmp.fontSize = FontSizes.H1; recTmp.color = TEXT_WHITE;
            recTmp.alignment = TextAlignmentOptions.Center;
            recTmp.fontStyle = FontStyles.Bold;
            recTmp.enableAutoSizing = true;
            recTmp.fontSizeMin = FontSizes.H4; recTmp.fontSizeMax = FontSizes.H1;

            // Win rate bar background
            GameObject barBG = new GameObject("WinRateBarBG");
            barBG.transform.SetParent(card.transform, false);
            RectTransform barBGRT = barBG.AddComponent<RectTransform>();
            barBGRT.anchorMin = new Vector2(0.05f, 0.20f);
            barBGRT.anchorMax = new Vector2(0.95f, 0.38f);
            barBGRT.offsetMin = Vector2.zero; barBGRT.offsetMax = Vector2.zero;
            Image barBGImg = barBG.AddComponent<Image>();
            barBGImg.color = BAR_BG;

            // Win rate bar fill
            GameObject barFill = new GameObject("WinRateBarFill");
            barFill.transform.SetParent(barBG.transform, false);
            RectTransform barFillRT = barFill.AddComponent<RectTransform>();
            barFillRT.anchorMin = Vector2.zero; barFillRT.anchorMax = Vector2.one;
            barFillRT.offsetMin = Vector2.zero; barFillRT.offsetMax = Vector2.zero;
            Image barFillImg = barFill.AddComponent<Image>();
            barFillImg.color = GOLD_PRIMARY;
            barFillImg.type = Image.Type.Filled;
            barFillImg.fillMethod = Image.FillMethod.Horizontal;
            barFillImg.fillOrigin = 0; // Left
            barFillImg.fillAmount = 0f;

            // Win rate text
            GameObject rateObj = new GameObject("WinRateText");
            rateObj.transform.SetParent(card.transform, false);
            RectTransform rateRT = rateObj.AddComponent<RectTransform>();
            rateRT.anchorMin = new Vector2(0, 0.02f); rateRT.anchorMax = new Vector2(1, 0.20f);
            rateRT.offsetMin = new Vector2(15, 0); rateRT.offsetMax = new Vector2(-15, 0);
            TextMeshProUGUI rateTmp = rateObj.AddComponent<TextMeshProUGUI>();
            rateTmp.text = "0% Win Rate";
            rateTmp.fontSize = FontSizes.Body; rateTmp.color = GOLD_LIGHT;
            rateTmp.alignment = TextAlignmentOptions.Center;
            rateTmp.fontStyle = FontStyles.Bold;
            rateTmp.enableAutoSizing = true;
            rateTmp.fontSizeMin = FontSizes.AutoMinBody;
            rateTmp.fontSizeMax = rateTmp.fontSize;
        }

        // ================================================================
        //  STREAK CARDS \u2014 2 side-by-side neon accent cards
        // ================================================================

        private static void CreateStreakCards(Transform parent)
        {
            GameObject row = new GameObject("StreakCards");
            row.transform.SetParent(parent, false);

            LayoutElement le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 190; le.flexibleWidth = 1;

            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.padding = new RectOffset(0, 0, 0, 0);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            CreateStreakBox(row.transform, "CurrentStreakCard", "Current Streak", "0", ACCENT_GREEN);
            CreateStreakBox(row.transform, "BestStreakCard", "Best Streak", "0 W", GOLD_LIGHT);
        }

        private static void CreateStreakBox(Transform parent, string name, string label, string value, Color accent)
        {
            GameObject box = new GameObject(name);
            box.transform.SetParent(parent, false);

            Image bg = box.AddComponent<Image>();
            bg.color = CARD_BG_ELEVATED;

            // Neon glow borders
            Outline ol1 = box.AddComponent<Outline>();
            ol1.effectColor = new Color(accent.r, accent.g, accent.b, 0.55f);
            ol1.effectDistance = new Vector2(2f, -2f);
            Outline ol2 = box.AddComponent<Outline>();
            ol2.effectColor = new Color(accent.r, accent.g, accent.b, 0.20f);
            ol2.effectDistance = new Vector2(5f, -5f);

            // Accent line at top
            GameObject accentLine = new GameObject("AccentLine");
            accentLine.transform.SetParent(box.transform, false);
            RectTransform aRT = accentLine.AddComponent<RectTransform>();
            aRT.anchorMin = new Vector2(0.1f, 1); aRT.anchorMax = new Vector2(0.9f, 1);
            aRT.pivot = new Vector2(0.5f, 1); aRT.sizeDelta = new Vector2(0, 3);
            accentLine.AddComponent<Image>().color = accent;

            // Value (big, top 60%)
            GameObject vObj = new GameObject("Value");
            vObj.transform.SetParent(box.transform, false);
            RectTransform vRT = vObj.AddComponent<RectTransform>();
            vRT.anchorMin = new Vector2(0, 0.30f); vRT.anchorMax = new Vector2(1, 0.92f);
            vRT.offsetMin = new Vector2(6, 0); vRT.offsetMax = new Vector2(-6, -4);
            TextMeshProUGUI vTmp = vObj.AddComponent<TextMeshProUGUI>();
            vTmp.text = value;
            vTmp.fontSize = FontSizes.H3; vTmp.color = accent;
            vTmp.alignment = TextAlignmentOptions.Center;
            vTmp.fontStyle = FontStyles.Bold;
            vTmp.enableAutoSizing = true;
            vTmp.fontSizeMin = FontSizes.Body; vTmp.fontSizeMax = FontSizes.H3;

            // Label (bottom 35%)
            GameObject lObj = new GameObject(name + "Label");
            lObj.transform.SetParent(box.transform, false);
            RectTransform lRT = lObj.AddComponent<RectTransform>();
            lRT.anchorMin = new Vector2(0, 0); lRT.anchorMax = new Vector2(1, 0.35f);
            lRT.offsetMin = new Vector2(4, 4); lRT.offsetMax = new Vector2(-4, 0);
            TextMeshProUGUI lTmp = lObj.AddComponent<TextMeshProUGUI>();
            lTmp.text = label;
            lTmp.fontSize = FontSizes.BodyLarge; lTmp.color = TEXT_MUTED;
            lTmp.alignment = TextAlignmentOptions.Center;
            lTmp.fontStyle = FontStyles.Bold;
            lTmp.enableAutoSizing = true;
            lTmp.fontSizeMin = FontSizes.AutoMinBody; lTmp.fontSizeMax = FontSizes.BodyLarge;
        }

        // ================================================================
        //  SECTION HEADER \u2014 \u2550\u2550\u2550 TITULO \u2550\u2550\u2550
        // ================================================================

        private static void CreateSectionHeader(Transform parent, string title)
        {
            GameObject row = new GameObject("Section_" + title.Replace(" ", ""));
            row.transform.SetParent(parent, false);

            LayoutElement le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 55; le.flexibleWidth = 1;

            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.padding = new RectOffset(5, 5, 15, 5);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            CreateSectionLine(row.transform, "LeftLine");

            GameObject txt = new GameObject("SectionTitle");
            txt.transform.SetParent(row.transform, false);
            LayoutElement tle = txt.AddComponent<LayoutElement>();
            tle.flexibleWidth = 0; tle.preferredWidth = 500;
            TextMeshProUGUI tmp = txt.AddComponent<TextMeshProUGUI>();
            tmp.text = title;
            tmp.fontSize = FontSizes.Subtitle; tmp.color = TEXT_GOLD;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.characterSpacing = 6;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinBody;
            tmp.fontSizeMax = tmp.fontSize;

            CreateSectionLine(row.transform, "RightLine");
        }

        private static void CreateSectionLine(Transform parent, string name)
        {
            GameObject line = new GameObject(name);
            line.transform.SetParent(parent, false);
            LayoutElement le = line.AddComponent<LayoutElement>();
            le.flexibleWidth = 1; le.preferredHeight = 2;
            line.AddComponent<Image>().color = GOLD_GLOW;
        }

        // ================================================================
        //  GAME STATS CARD \u2014 5 game rows with progress bars
        // ================================================================

        private static void CreateGameStatsCard(Transform parent)
        {
            GameObject card = new GameObject("GameStatsCard");
            card.transform.SetParent(parent, false);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredHeight = 580; le.flexibleWidth = 1;

            Image bg = card.AddComponent<Image>();
            bg.color = CARD_BG;

            Outline ol = card.AddComponent<Outline>();
            ol.effectColor = GOLD_BORDER;
            ol.effectDistance = new Vector2(1.5f, -1.5f);

            // Gold top line
            GameObject topLine = new GameObject("GoldTopLine");
            topLine.transform.SetParent(card.transform, false);
            RectTransform tlRT = topLine.AddComponent<RectTransform>();
            tlRT.anchorMin = new Vector2(0, 1); tlRT.anchorMax = new Vector2(1, 1);
            tlRT.pivot = new Vector2(0.5f, 1); tlRT.sizeDelta = new Vector2(0, 3);
            topLine.AddComponent<Image>().color = GOLD_PRIMARY;

            // 5 game rows using anchor-based layout
            CreateGameRow(card.transform, "GameRow_DigitRush",   "Digit Rush",   GAME_DIGITRUSH,   0);
            CreateGameRow(card.transform, "GameRow_MemoryPairs", "Memory Pairs", GAME_MEMORYPAIRS,  1);
            CreateGameRow(card.transform, "GameRow_QuickMath",   "Quick Math",   GAME_QUICKMATH,    2);
            CreateGameRow(card.transform, "GameRow_FlashTap",    "Flash Tap",    GAME_FLASHTAP,     3);
            CreateGameRow(card.transform, "GameRow_OddOneOut",   "Odd One Out",  GAME_ODDONEOUT,    4);
        }

        private static void CreateGameRow(Transform parent, string name, string gameName, Color accent, int index)
        {
            // Each row occupies ~18% of the card with gaps
            float rowHeight = 0.175f;
            float gap = 0.012f;
            float startY = 0.96f; // just below top line

            float yTop = startY - index * (rowHeight + gap);
            float yBot = yTop - rowHeight;

            GameObject row = new GameObject(name);
            row.transform.SetParent(parent, false);
            RectTransform rt = row.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, yBot);
            rt.anchorMax = new Vector2(1, yTop);
            rt.offsetMin = new Vector2(12, 0);
            rt.offsetMax = new Vector2(-12, 0);

            // Accent bar (left edge)
            GameObject bar = new GameObject("AccentBar");
            bar.transform.SetParent(row.transform, false);
            RectTransform barRT = bar.AddComponent<RectTransform>();
            barRT.anchorMin = new Vector2(0, 0.10f);
            barRT.anchorMax = new Vector2(0, 0.90f);
            barRT.pivot = new Vector2(0, 0.5f);
            barRT.anchoredPosition = Vector2.zero;
            barRT.sizeDelta = new Vector2(4, 0);
            bar.AddComponent<Image>().color = accent;

            // Game name
            GameObject nameObj = new GameObject("GameName");
            nameObj.transform.SetParent(row.transform, false);
            RectTransform nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0);
            nameRT.anchorMax = new Vector2(0.30f, 1);
            nameRT.offsetMin = new Vector2(14, 6);
            nameRT.offsetMax = new Vector2(0, -6);
            TextMeshProUGUI nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
            nameTmp.text = gameName;
            nameTmp.fontSize = FontSizes.Body; nameTmp.color = TEXT_WHITE;
            nameTmp.alignment = TextAlignmentOptions.Left;
            nameTmp.fontStyle = FontStyles.Bold;
            nameTmp.enableAutoSizing = true;
            nameTmp.fontSizeMin = FontSizes.AutoMinBody; nameTmp.fontSizeMax = FontSizes.Body;

            // Progress bar background
            GameObject barBG = new GameObject("BarBG");
            barBG.transform.SetParent(row.transform, false);
            RectTransform barBGRT = barBG.AddComponent<RectTransform>();
            barBGRT.anchorMin = new Vector2(0.32f, 0.25f);
            barBGRT.anchorMax = new Vector2(0.70f, 0.75f);
            barBGRT.offsetMin = Vector2.zero; barBGRT.offsetMax = Vector2.zero;
            barBG.AddComponent<Image>().color = BAR_BG;

            // Progress bar fill
            GameObject barFill = new GameObject("BarFill");
            barFill.transform.SetParent(barBG.transform, false);
            RectTransform fillRT = barFill.AddComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = Vector2.zero; fillRT.offsetMax = Vector2.zero;
            Image fillImg = barFill.AddComponent<Image>();
            fillImg.color = accent;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = 0;
            fillImg.fillAmount = 0f;

            // Value text (right side) \u2014 "-- | 0%"
            GameObject valObj = new GameObject("ValueText");
            valObj.transform.SetParent(row.transform, false);
            RectTransform valRT = valObj.AddComponent<RectTransform>();
            valRT.anchorMin = new Vector2(0.72f, 0);
            valRT.anchorMax = new Vector2(1, 1);
            valRT.offsetMin = new Vector2(4, 6);
            valRT.offsetMax = new Vector2(-4, -6);
            TextMeshProUGUI valTmp = valObj.AddComponent<TextMeshProUGUI>();
            valTmp.text = "-- | 0%";
            valTmp.fontSize = FontSizes.Body; valTmp.color = accent;
            valTmp.alignment = TextAlignmentOptions.Right;
            valTmp.fontStyle = FontStyles.Bold;
            valTmp.enableAutoSizing = true;
            valTmp.fontSizeMin = FontSizes.AutoMinBody; valTmp.fontSizeMax = FontSizes.Body;

            // Bottom separator line
            if (index < 4) // no separator on last row
            {
                GameObject sep = new GameObject("Separator");
                sep.transform.SetParent(row.transform, false);
                RectTransform sepRT = sep.AddComponent<RectTransform>();
                sepRT.anchorMin = new Vector2(0.02f, 0);
                sepRT.anchorMax = new Vector2(0.98f, 0);
                sepRT.pivot = new Vector2(0.5f, 0);
                sepRT.anchoredPosition = Vector2.zero;
                sepRT.sizeDelta = new Vector2(0, 1);
                sep.AddComponent<Image>().color = new Color(1, 1, 1, 0.06f);
            }
        }

        // ================================================================
        //  SPACER
        // ================================================================

        private static void CreateSpacer(Transform parent, float height)
        {
            GameObject sp = new GameObject("Spacer");
            sp.transform.SetParent(parent, false);
            LayoutElement le = sp.AddComponent<LayoutElement>();
            le.preferredHeight = height; le.flexibleWidth = 1;
        }

        // ================================================================
        //  ANIMATION MANAGERS
        // ================================================================

        private static void CreateAnimationManagers()
        {
            GameObject managersRoot = GameObject.Find("---ANIMATION_MANAGERS---");
            if (managersRoot == null)
                managersRoot = new GameObject("---ANIMATION_MANAGERS---");

            Transform existing = managersRoot.transform.Find("CashProfileAnimator");
            if (existing == null)
            {
                GameObject animObj = new GameObject("CashProfileAnimator");
                animObj.transform.SetParent(managersRoot.transform);
                UIBuilderAnimationUtils.AddCashProfileAnimator(animObj);
            }

            var uiAnimMgr = Object.FindFirstObjectByType<DigitPark.Animations.UIAnimationManager>();
            if (uiAnimMgr == null)
            {
                Transform uiAnimT = managersRoot.transform.Find("UIAnimationManager");
                if (uiAnimT == null)
                {
                    GameObject uiAnimObj = new GameObject("UIAnimationManager");
                    uiAnimObj.transform.SetParent(managersRoot.transform);
                    uiAnimObj.AddComponent<DigitPark.Animations.UIAnimationManager>();
                }
            }
        }

        // ================================================================
        //  REFERENCE ASSIGNER
        // ================================================================

        private static MonoBehaviour FindController()
        {
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>(true))
                if (mb.GetType().Name == "CashProfileSceneController") return mb;
            return null;
        }

        private static void ResetAR()
        {
            assignedCount = failedCount = alreadySetCount = 0;
            arList.Clear();
        }

        private static void RunAssignAll()
        {
            Debug.Log("[CashProfileUIBuilder] === ASIGNANDO REFERENCIAS ===");

            var ctrl = FindController();
            if (ctrl == null)
            {
                Debug.LogWarning("[CashProfileUIBuilder] CashProfileSceneController no encontrado.");
                AddAR("Controller", "No encontrado en escena", false, null);
                failedCount++;
                return;
            }

            SerializedObject so = new SerializedObject(ctrl);
            so.Update();

            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            Transform root = canvas != null ? canvas.transform : ctrl.transform.root;

            // \u2500\u2500 Header \u2500\u2500
            Assign(so, "backButton",  FindBtn(root, "BackButton"));
            Assign(so, "titleText",   FindTMP(root, "TitleText"));

            // \u2500\u2500 Avatar \u2500\u2500
            Assign(so, "usernameText",    FindTMP(root, "UsernameText"));
            Assign(so, "memberSinceText", FindTMP(root, "MemberSinceText"));


            // \u2500\u2500 Record Card \u2500\u2500
            Assign(so, "recordText",      FindTMP(root, "RecordText"));
            Transform wrbf = Deep(root, "WinRateBarFill");
            Assign(so, "winRateBarFill",  wrbf != null ? wrbf.GetComponent<Image>() : null);
            Assign(so, "winRateText",     FindTMP(root, "WinRateText"));

            // \u2500\u2500 Streaks \u2500\u2500
            Assign(so, "currentStreakText", FindValue(root, "CurrentStreakCard"));
            Assign(so, "bestStreakText",    FindValue(root, "BestStreakCard"));
            Assign(so, "rankBadgeText",     FindTMP(root, "RankText"));

            //\u2500\u2500 Per-Game Stats \u2500\u2500
            AssignGameRow(so, root, "GameRow_DigitRush",   "digitRush");
            AssignGameRow(so, root, "GameRow_MemoryPairs", "memoryPairs");
            AssignGameRow(so, root, "GameRow_QuickMath",   "quickMath");
            AssignGameRow(so, root, "GameRow_FlashTap",    "flashTap");
            AssignGameRow(so, root, "GameRow_OddOneOut",   "oddOneOut");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(ctrl);
            EditorUtility.SetDirty(ctrl.gameObject);
            EditorSceneManager.MarkSceneDirty(ctrl.gameObject.scene);

            int ok = assignedCount + alreadySetCount;
            int total = ok + failedCount;
            Debug.Log($"[CashProfileUIBuilder] === {ok}/{total} REFERENCIAS ASIGNADAS ===");
        }

        private static void AssignGameRow(SerializedObject so, Transform root, string rowName, string fieldPrefix)
        {
            Transform rowT = Deep(root, rowName);
            if (rowT != null)
            {
                // BarFill is inside BarBG
                Transform barBG = rowT.Find("BarBG");
                Transform barFill = barBG != null ? barBG.Find("BarFill") : null;
                Assign(so, fieldPrefix + "BarFill", barFill != null ? barFill.GetComponent<Image>() : null);

                // ValueText
                Transform valT = rowT.Find("ValueText");
                Assign(so, fieldPrefix + "ValueText", valT != null ? valT.GetComponent<TextMeshProUGUI>() : null);
            }
            else
            {
                Assign(so, fieldPrefix + "BarFill", (Object)null);
                Assign(so, fieldPrefix + "ValueText", (Object)null);
            }
        }

        // \u2500\u2500 Button helpers \u2500\u2500

        private static void SetupButtonColorBlock(GameObject btnObj)
        {
            Button btn = btnObj.GetComponent<Button>();
            if (btn == null) return;

            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1f, 0.95f, 0.7f, 1f);
            cb.pressedColor = new Color(0.8f, 0.67f, 0f, 1f);
            cb.selectedColor = Color.white;
            btn.colors = cb;
        }

        // \u2500\u2500 Deep finders \u2500\u2500

        private static Sprite GenerateCircleSprite()
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/UI/CircleSprite.png");
            if (s != null) return s;
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

        private static Transform Deep(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            foreach (Transform c in root)
            {
                Transform r = Deep(c, name);
                if (r != null) return r;
            }
            return null;
        }

        private static TextMeshProUGUI FindTMP(Transform root, string name)
        {
            Transform t = Deep(root, name);
            return t != null ? t.GetComponent<TextMeshProUGUI>() : null;
        }

        private static Button FindBtn(Transform root, string name)
        {
            Transform t = Deep(root, name);
            return t != null ? t.GetComponent<Button>() : null;
        }

        private static TextMeshProUGUI FindValue(Transform root, string parentName)
        {
            Transform p = Deep(root, parentName);
            if (p == null) return null;
            Transform v = p.Find("Value");
            return v != null ? v.GetComponent<TextMeshProUGUI>() : null;
        }

        // \u2500\u2500 Assignment helper \u2500\u2500

        private static void Assign(SerializedObject so, string prop, Object value)
        {
            var p = so.FindProperty(prop);
            if (p == null)         { AddAR(prop, "Property not found", false, null); failedCount++; return; }
            if (p.objectReferenceValue != null) { AddAR(prop, "Already Set", true, p.objectReferenceValue); alreadySetCount++; return; }
            if (value != null)     { p.objectReferenceValue = value; AddAR(prop, "Assigned", true, value); assignedCount++; }
            else                   { AddAR(prop, "Not found", false, null); failedCount++; }
        }

        private static void AddAR(string f, string s, bool ok, Object o) =>
            arList.Add(new AR { field = f, status = s, ok = ok, obj = o });

        // \u2500\u2500 Results GUI \u2500\u2500

        // ================================================================
        //  CHANGE NAME PANEL + ERROR PANEL
        // ================================================================

        private static void BuildChangeNamePanel(Transform parent)
        {
            GameObject panelRoot = new GameObject("ChangeNamePanel");
            panelRoot.transform.SetParent(parent, false);
            RectTransform rt = panelRoot.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            // Blocker
            GameObject blocker = new GameObject("BlockerPanel");
            blocker.transform.SetParent(panelRoot.transform, false);
            RectTransform bRT = blocker.AddComponent<RectTransform>();
            bRT.anchorMin = Vector2.zero; bRT.anchorMax = Vector2.one;
            bRT.offsetMin = Vector2.zero; bRT.offsetMax = Vector2.zero;
            blocker.AddComponent<Image>().color = new Color(0, 0, 0, 0.7f);

            // Card
            GameObject card = new GameObject("Panel");
            card.transform.SetParent(panelRoot.transform, false);
            RectTransform cRT = card.AddComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0.08f, 0.3f); cRT.anchorMax = new Vector2(0.92f, 0.7f);
            cRT.offsetMin = Vector2.zero; cRT.offsetMax = Vector2.zero;
            card.AddComponent<Image>().color = CARD_BG;

            // Title
            GameObject titleObj = new GameObject("CashChangeNameTitle");
            titleObj.transform.SetParent(card.transform, false);
            RectTransform tRT = titleObj.AddComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0, 0.78f); tRT.anchorMax = Vector2.one;
            tRT.offsetMin = new Vector2(20, 0); tRT.offsetMax = new Vector2(-20, -10);
            var titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
            titleTxt.text = "Change Name";
            titleTxt.fontSize = FontSizes.H4; titleTxt.fontStyle = FontStyles.Bold;
            titleTxt.color = GOLD_PRIMARY; titleTxt.alignment = TextAlignmentOptions.Center;
            titleTxt.enableAutoSizing = true;
            titleTxt.fontSizeMin = FontSizes.AutoMinTitle;
            titleTxt.fontSizeMax = titleTxt.fontSize;

            // Input Field
            GameObject inputObj = new GameObject("InputField");
            inputObj.transform.SetParent(card.transform, false);
            RectTransform iRT = inputObj.AddComponent<RectTransform>();
            iRT.anchorMin = new Vector2(0.08f, 0.48f); iRT.anchorMax = new Vector2(0.92f, 0.72f);
            iRT.offsetMin = Vector2.zero; iRT.offsetMax = Vector2.zero;
            inputObj.AddComponent<Image>().color = new Color(0.15f, 0.13f, 0.20f, 1f);

            GameObject textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputObj.transform, false);
            RectTransform taRT = textArea.AddComponent<RectTransform>();
            taRT.anchorMin = Vector2.zero; taRT.anchorMax = Vector2.one;
            taRT.offsetMin = new Vector2(12, 4); taRT.offsetMax = new Vector2(-12, -4);
            textArea.AddComponent<RectMask2D>();

            GameObject placeholder = new GameObject("Placeholder");
            placeholder.transform.SetParent(textArea.transform, false);
            RectTransform phRT = placeholder.AddComponent<RectTransform>();
            phRT.anchorMin = Vector2.zero; phRT.anchorMax = Vector2.one;
            phRT.offsetMin = Vector2.zero; phRT.offsetMax = Vector2.zero;
            var phTxt = placeholder.AddComponent<TextMeshProUGUI>();
            phTxt.text = "New name...";
            phTxt.fontSize = FontSizes.Body; phTxt.fontStyle = FontStyles.Bold;
            phTxt.color = TEXT_MUTED; phTxt.alignment = TextAlignmentOptions.Left;
            phTxt.enableAutoSizing = true;
            phTxt.fontSizeMin = FontSizes.AutoMinBody;
            phTxt.fontSizeMax = phTxt.fontSize;

            GameObject inputText = new GameObject("InputFieldText");
            inputText.transform.SetParent(textArea.transform, false);
            RectTransform itRT = inputText.AddComponent<RectTransform>();
            itRT.anchorMin = Vector2.zero; itRT.anchorMax = Vector2.one;
            itRT.offsetMin = Vector2.zero; itRT.offsetMax = Vector2.zero;
            var iTxt = inputText.AddComponent<TextMeshProUGUI>();
            iTxt.fontSize = FontSizes.Body; iTxt.fontStyle = FontStyles.Bold; iTxt.color = TEXT_WHITE;
            iTxt.alignment = TextAlignmentOptions.Left;
            iTxt.enableAutoSizing = true;
            iTxt.fontSizeMin = FontSizes.AutoMinBody;
            iTxt.fontSizeMax = iTxt.fontSize;

            TMP_InputField tmpInput = inputObj.AddComponent<TMP_InputField>();
            tmpInput.textViewport = taRT;
            tmpInput.textComponent = iTxt;
            tmpInput.placeholder = phTxt;
            tmpInput.pointSize = FontSizes.Body;
            tmpInput.characterLimit = 20;

            // Confirm Button (Gold theme)
            GameObject confirmObj = new GameObject("ConfirmButton");
            confirmObj.transform.SetParent(card.transform, false);
            RectTransform cfRT = confirmObj.AddComponent<RectTransform>();
            cfRT.anchorMin = new Vector2(0.55f, 0.08f); cfRT.anchorMax = new Vector2(0.92f, 0.35f);
            cfRT.offsetMin = Vector2.zero; cfRT.offsetMax = Vector2.zero;
            Image cfBg = confirmObj.AddComponent<Image>(); cfBg.color = GOLD_PRIMARY;
            Button confirmBtn = confirmObj.AddComponent<Button>(); confirmBtn.targetGraphic = cfBg;

            GameObject cfTxtObj = new GameObject("ConfirmButtonText");
            cfTxtObj.transform.SetParent(confirmObj.transform, false);
            RectTransform cfTxtRT = cfTxtObj.AddComponent<RectTransform>();
            cfTxtRT.anchorMin = Vector2.zero; cfTxtRT.anchorMax = Vector2.one;
            cfTxtRT.offsetMin = Vector2.zero; cfTxtRT.offsetMax = Vector2.zero;
            var cfTxt = cfTxtObj.AddComponent<TextMeshProUGUI>();
            cfTxt.text = "Save"; cfTxt.fontSize = FontSizes.Body;
            cfTxt.fontStyle = FontStyles.Bold; cfTxt.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            cfTxt.alignment = TextAlignmentOptions.Center;
            cfTxt.enableAutoSizing = true;
            cfTxt.fontSizeMin = FontSizes.AutoMinBody;
            cfTxt.fontSizeMax = cfTxt.fontSize;

            // Cancel Button
            GameObject cancelObj = new GameObject("CancelButton");
            cancelObj.transform.SetParent(card.transform, false);
            RectTransform ccRT = cancelObj.AddComponent<RectTransform>();
            ccRT.anchorMin = new Vector2(0.08f, 0.08f); ccRT.anchorMax = new Vector2(0.45f, 0.35f);
            ccRT.offsetMin = Vector2.zero; ccRT.offsetMax = Vector2.zero;
            Image ccBg = cancelObj.AddComponent<Image>(); ccBg.color = new Color(0.15f, 0.13f, 0.20f, 1f);
            Button cancelBtn = cancelObj.AddComponent<Button>(); cancelBtn.targetGraphic = ccBg;

            GameObject ccTxtObj = new GameObject("CancelButtonText");
            ccTxtObj.transform.SetParent(cancelObj.transform, false);
            RectTransform ccTxtRT = ccTxtObj.AddComponent<RectTransform>();
            ccTxtRT.anchorMin = Vector2.zero; ccTxtRT.anchorMax = Vector2.one;
            ccTxtRT.offsetMin = Vector2.zero; ccTxtRT.offsetMax = Vector2.zero;
            var ccTxt = ccTxtObj.AddComponent<TextMeshProUGUI>();
            ccTxt.text = "Cancel"; ccTxt.fontSize = FontSizes.Body;
            ccTxt.fontStyle = FontStyles.Bold; ccTxt.color = TEXT_MUTED; ccTxt.alignment = TextAlignmentOptions.Center;
            ccTxt.enableAutoSizing = true;
            ccTxt.fontSizeMin = FontSizes.AutoMinBody;
            ccTxt.fontSizeMax = ccTxt.fontSize;

            // Wire InputPanelUI component
            var inputComp = panelRoot.AddComponent(System.Type.GetType("DigitPark.UI.Panels.InputPanelUI, Assembly-CSharp"));
            if (inputComp != null)
            {
                var so = new SerializedObject(inputComp);
                var pp = so.FindProperty("panel"); if (pp != null) pp.objectReferenceValue = card;
                var bp = so.FindProperty("blockerPanel"); if (bp != null) bp.objectReferenceValue = blocker;
                var tp = so.FindProperty("titleText"); if (tp != null) tp.objectReferenceValue = titleTxt;
                var ip = so.FindProperty("inputField"); if (ip != null) ip.objectReferenceValue = tmpInput;
                var cb = so.FindProperty("confirmButton"); if (cb != null) cb.objectReferenceValue = confirmBtn;
                var ccb = so.FindProperty("cancelButton"); if (ccb != null) ccb.objectReferenceValue = cancelBtn;
                var ct = so.FindProperty("confirmButtonText"); if (ct != null) ct.objectReferenceValue = cfTxt;
                var cct = so.FindProperty("cancelButtonText"); if (cct != null) cct.objectReferenceValue = ccTxt;
                so.ApplyModifiedProperties();
            }

            panelRoot.SetActive(false);
            Debug.Log("[CashProfileUIBuilder] ChangeNamePanel creado");
        }

        private static void BuildErrorPanel(Transform parent)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Common/ErrorPanel.prefab");
            if (prefab != null)
            {
                GameObject errorPanel = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                errorPanel.name = "ErrorPanel";
                RectTransform rect = errorPanel.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 0);
                    rect.anchorMax = new Vector2(0.5f, 0);
                    rect.pivot = new Vector2(0.5f, 0);
                    rect.anchoredPosition = new Vector2(0, 200);
                }
                Debug.Log("[CashProfileUIBuilder] ErrorPanel instantiated from prefab");
            }
            else
            {
                Debug.LogWarning("[CashProfileUIBuilder] ErrorPanel prefab not found");
            }
        }

        private void DrawResults()
        {
            if (arList.Count == 0) return;

            EditorGUILayout.Space(10);
            int total = arList.Count;
            int good = assignedCount + alreadySetCount;

            EditorGUILayout.BeginVertical("box");

            float rate = total > 0 ? (float)good / total : 0f;
            GUI.color = rate == 1f ? new Color(0.2f, 0.85f, 0.2f) :
                        rate >= 0.7f ? new Color(1f, 0.8f, 0.2f) : new Color(1f, 0.4f, 0.4f);
            GUILayout.Label(rate == 1f ? "TODAS LAS REFERENCIAS ASIGNADAS" : "Algunas referencias faltan", EditorStyles.boldLabel);
            GUI.color = Color.white;

            GUILayout.Label($"Asignados: {assignedCount} | Ya estaban: {alreadySetCount} | Fallidos: {failedCount}");
            EditorGUILayout.Space(5);

            foreach (var r in arList)
            {
                EditorGUILayout.BeginHorizontal();
                GUI.color = r.ok ? (r.status == "Already Set" ? new Color(0.5f, 0.8f, 1f) : Color.green) : Color.red;
                GUILayout.Label(r.ok ? (r.status == "Already Set" ? "o" : "+") : "x", GUILayout.Width(18));
                GUI.color = Color.white;
                GUILayout.Label(r.field, GUILayout.Width(200));
                GUILayout.Label(r.status, GUILayout.Width(120));
                if (r.obj != null)
                    EditorGUILayout.ObjectField(r.obj, typeof(Object), true, GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }
    }
}
