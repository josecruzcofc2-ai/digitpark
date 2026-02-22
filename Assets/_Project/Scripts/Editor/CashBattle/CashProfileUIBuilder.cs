using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;

namespace DigitPark.Editor
{
    /// <summary>
    /// UI Builder para la escena CashProfile.unity
    /// Diseño ultra profesional neon dorado.
    ///
    /// Layout:
    ///   Header (100px) FIJO arriba — BackButton + Titulo + BalanceWidget
    ///   ─── gold glow separator ───
    ///   [ScrollView rellena el resto]
    ///     Avatar Card (170px)       — foto + username + member since, borde dorado con glow
    ///     3 Hero Stats (150px)      — Total | WinRate | NetProfit, cajas con neon accent
    ///     Section "ESTADÍSTICAS"    — titulo con líneas doradas decorativas
    ///     Stats Grid 5x2 (480px)   — 10 stats con iconos, card con doble borde
    ///
    /// Auto-asigna referencias al controller al final del build.
    /// Menu: DigitPark/UI Builders/CashBattle/Cash Profile
    /// </summary>
    public class CashProfileUIBuilder : EditorWindow
    {
        #region Palette — Neon Gold

        // === GOLDS ===
        private static readonly Color GOLD_PRIMARY     = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GOLD_DARK        = new Color(0.85f, 0.65f, 0.13f, 1f);
        private static readonly Color GOLD_LIGHT       = new Color(1f, 0.93f, 0.55f, 1f);
        private static readonly Color GOLD_GLOW        = new Color(1f, 0.84f, 0f, 0.35f);   // glow lines
        private static readonly Color GOLD_BORDER      = new Color(0.85f, 0.65f, 0.13f, 0.7f);
        private static readonly Color GOLD_BORDER_OUTER= new Color(1f, 0.84f, 0f, 0.25f);   // outer neon ring

        // === BACKGROUNDS ===
        private static readonly Color BG_DARK          = new Color(0.06f, 0.05f, 0.10f, 1f); // almost black-purple
        private static readonly Color BG_HEADER        = new Color(0.04f, 0.03f, 0.08f, 0.95f);
        private static readonly Color CARD_BG          = new Color(0.10f, 0.08f, 0.14f, 0.97f);
        private static readonly Color CARD_BG_ELEVATED = new Color(0.13f, 0.11f, 0.17f, 0.97f);

        // === TEXT ===
        private static readonly Color TEXT_WHITE       = new Color(1f, 1f, 1f, 1f);
        private static readonly Color TEXT_GOLD        = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color TEXT_MUTED       = new Color(0.55f, 0.53f, 0.60f, 1f);

        // === ACCENTS ===
        private static readonly Color ACCENT_GREEN     = new Color(0.25f, 1f, 0.50f, 1f);
        private static readonly Color ACCENT_RED       = new Color(1f, 0.35f, 0.35f, 1f);
        private static readonly Color ACCENT_CYAN      = new Color(0f, 0.90f, 1f, 1f);

        // === PATHS ===
        private static readonly string STAT_ICONS_PATH = "Assets/_Project/Art/Icons/CashBattle/Stats/";
        private const string BACK_BTN_PREFAB = "Assets/_Project/Prefabs/Common/BackButtonGold.prefab";

        #endregion

        #region Assign State

        private Vector2 scrollPosition;
        private static int assignedCount, failedCount, alreadySetCount;
        private static List<AR> arList = new List<AR>();
        private struct AR { public string field, status; public bool ok; public Object obj; }

        #endregion

        // ==================== MENU ====================

        [MenuItem("DigitPark/UI Builders/CashBattle/Cash Profile", false, 254)]
        public static void ShowWindow() => GetWindow<CashProfileUIBuilder>("Cash Profile Builder");

        // ==================== EDITOR WINDOW ====================

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Cash Profile — Neon Gold", EditorStyles.boldLabel);
            GUILayout.Label("Perfil privado de estadísticas Cash Battle", EditorStyles.miniLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Construye la UI completa para CashProfile.unity.\n\n" +
                "Diseño Neon Gold:\n" +
                "  • Header fijo + ScrollView con todo el contenido\n" +
                "  • Avatar card con anillo dorado glow\n" +
                "  • 3 Hero Stats con accent neon\n" +
                "  • Grid 5x2 con doble borde premium\n\n" +
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
            GUILayout.Label("Asignación Manual", EditorStyles.boldLabel);

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
                EditorUtility.DisplayDialog("Error", "No se encontró Canvas.\nAbre la escena CashProfile primero.", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog("Reconstruir UI?",
                "Esto reconstruirá toda la UI de CashProfile con el diseño Neon Gold " +
                "y auto-asignará las referencias.\n\n¿Continuar?",
                "Sí, Construir", "Cancelar")) return;

            // 1) Limpiar
            Cleanup(canvas.transform);

            // 2) Construir toda la UI
            BuildAll(canvas);

            // 2b) Agregar ---ANIMATION_MANAGERS--- con CashProfileAnimator
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
                $"UI Neon Gold construida exitosamente.\n\n" +
                $"Referencias: {ok}/{total} asignadas.\n" +
                (failedCount > 0 ? $"Fallidas: {failedCount}" : "¡Todo OK!"),
                "OK");
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

            // Gold glow separator line debajo del header
            CreateGoldSeparator(sa, -100f);

            // ScrollView que contiene TODO el contenido debajo del header
            GameObject scrollContent = CreateMainScrollView(sa);
            Transform content = scrollContent.transform;

            // === CONTENIDO DENTRO DEL SCROLL ===
            CreateAvatarCard(content);
            CreateHeroStats(content);
            CreateSectionHeader(content, "ESTADÍSTICAS DETALLADAS");
            CreateStatsGrid(content);
            // Bottom spacer
            CreateSpacer(content, 30);
        }

        private static void Cleanup(Transform parent)
        {
            foreach (string name in new[] { "Background", "SafeArea" })
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
                brt.sizeDelta = new Vector2(65, 65);
                brt.anchoredPosition = new Vector2(42, 0);
                SetupButtonColorBlock(btn);
            }
            else
            {
                // Fallback
                GameObject btn = new GameObject("BackButton");
                btn.transform.SetParent(header.transform, false);
                RectTransform brt = btn.AddComponent<RectTransform>();
                brt.anchorMin = new Vector2(0, 0.5f);
                brt.anchorMax = new Vector2(0, 0.5f);
                brt.sizeDelta = new Vector2(65, 65);
                brt.anchoredPosition = new Vector2(42, 0);
                btn.AddComponent<Image>().color = new Color(1, 1, 1, 0);
                btn.AddComponent<Button>();
                SetupButtonColorBlock(btn);

                GameObject arrow = new GameObject("Arrow");
                arrow.transform.SetParent(btn.transform, false);
                RectTransform art = arrow.AddComponent<RectTransform>();
                art.anchorMin = Vector2.zero; art.anchorMax = Vector2.one; art.sizeDelta = Vector2.zero;
                TextMeshProUGUI atmp = arrow.AddComponent<TextMeshProUGUI>();
                atmp.text = "\u2190"; atmp.fontSize = 42; atmp.color = TEXT_WHITE;
                atmp.alignment = TextAlignmentOptions.Center; atmp.fontStyle = FontStyles.Bold;
            }

            // === Title ===
            GameObject title = new GameObject("TitleText");
            title.transform.SetParent(header.transform, false);
            RectTransform trt = title.AddComponent<RectTransform>();
            trt.anchorMin = new Vector2(0.15f, 0);
            trt.anchorMax = new Vector2(0.70f, 1);
            trt.sizeDelta = Vector2.zero;

            TextMeshProUGUI ttmp = title.AddComponent<TextMeshProUGUI>();
            ttmp.text = "MI PERFIL CASH";
            ttmp.fontSize = 78;
            ttmp.color = TEXT_GOLD;
            ttmp.alignment = TextAlignmentOptions.Center;
            ttmp.fontStyle = FontStyles.Bold;
            ttmp.enableAutoSizing = true;
            ttmp.fontSizeMin = 42;
            ttmp.fontSizeMax = 78;
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
            tmp.fontSize = 44; tmp.color = TEXT_GOLD;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.enableAutoSizing = true; tmp.fontSizeMin = 28; tmp.fontSizeMax = 44;
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

            // Doble glow: outer wider line
            Outline glow = sep.AddComponent<Outline>();
            glow.effectColor = new Color(1f, 0.84f, 0f, 0.15f);
            glow.effectDistance = new Vector2(0, -2);
        }

        // ================================================================
        //  MAIN SCROLLVIEW
        // ================================================================

        private static GameObject CreateMainScrollView(Transform parent)
        {
            // ScrollView debajo del header
            GameObject sv = new GameObject("MainScrollView");
            sv.transform.SetParent(parent, false);

            RectTransform svRT = sv.AddComponent<RectTransform>();
            svRT.anchorMin = Vector2.zero;
            svRT.anchorMax = Vector2.one;
            svRT.offsetMin = new Vector2(0, 0);
            svRT.offsetMax = new Vector2(0, -105); // debajo del header + separator

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
            vlg.spacing = 16;
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
        //  AVATAR CARD — Gold Ring
        // ================================================================

        private static void CreateAvatarCard(Transform parent)
        {
            GameObject card = new GameObject("AvatarCard");
            card.transform.SetParent(parent, false);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredHeight = 170; le.flexibleWidth = 1;

            // Card background
            Image bg = card.AddComponent<Image>();
            bg.color = CARD_BG;

            // Double border: inner gold + outer glow
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

            // === Avatar Image con gold ring ===
            // Ring (fondo dorado circular)
            GameObject ring = new GameObject("AvatarRing");
            ring.transform.SetParent(card.transform, false);
            RectTransform ringRT = ring.AddComponent<RectTransform>();
            ringRT.anchorMin = new Vector2(0, 0.5f); ringRT.anchorMax = new Vector2(0, 0.5f);
            ringRT.pivot = new Vector2(0, 0.5f);
            ringRT.sizeDelta = new Vector2(136, 136);
            ringRT.anchoredPosition = new Vector2(20, 0);
            Image ringImg = ring.AddComponent<Image>();
            ringImg.color = GOLD_DARK;
            Outline ringGlow = ring.AddComponent<Outline>();
            ringGlow.effectColor = GOLD_GLOW;
            ringGlow.effectDistance = new Vector2(3, -3);

            // Avatar Image (dentro del ring, un poco más pequeño)
            GameObject avatar = new GameObject("AvatarImage");
            avatar.transform.SetParent(ring.transform, false);
            RectTransform avRT = avatar.AddComponent<RectTransform>();
            avRT.anchorMin = Vector2.zero; avRT.anchorMax = Vector2.one;
            avRT.offsetMin = new Vector2(5, 5); avRT.offsetMax = new Vector2(-5, -5);
            Image avImg = avatar.AddComponent<Image>();
            avImg.color = new Color(0.15f, 0.13f, 0.20f, 1f);

            // === Username ===
            GameObject uname = new GameObject("UsernameText");
            uname.transform.SetParent(card.transform, false);
            RectTransform unRT = uname.AddComponent<RectTransform>();
            unRT.anchorMin = new Vector2(0, 0.50f); unRT.anchorMax = new Vector2(1, 1);
            unRT.offsetMin = new Vector2(175, 0); unRT.offsetMax = new Vector2(-15, -18);
            TextMeshProUGUI unTmp = uname.AddComponent<TextMeshProUGUI>();
            unTmp.text = "@Player";
            unTmp.fontSize = 56; unTmp.color = TEXT_WHITE;
            unTmp.alignment = TextAlignmentOptions.Left;
            unTmp.fontStyle = FontStyles.Bold;
            unTmp.enableAutoSizing = true; unTmp.fontSizeMin = 36; unTmp.fontSizeMax = 56;

            // === Member Since ===
            GameObject msince = new GameObject("MemberSinceText");
            msince.transform.SetParent(card.transform, false);
            RectTransform msRT = msince.AddComponent<RectTransform>();
            msRT.anchorMin = new Vector2(0, 0); msRT.anchorMax = new Vector2(1, 0.50f);
            msRT.offsetMin = new Vector2(175, 12); msRT.offsetMax = new Vector2(-15, 0);
            TextMeshProUGUI msTmp = msince.AddComponent<TextMeshProUGUI>();
            msTmp.text = "Miembro desde 2024";
            msTmp.fontSize = 36; msTmp.color = TEXT_MUTED;
            msTmp.alignment = TextAlignmentOptions.Left;
        }

        // ================================================================
        //  3 HERO STATS — Neon Accent Boxes
        // ================================================================

        private static void CreateHeroStats(Transform parent)
        {
            GameObject row = new GameObject("HeroStats");
            row.transform.SetParent(parent, false);

            LayoutElement le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 150; le.flexibleWidth = 1;

            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.padding = new RectOffset(0, 0, 0, 0);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            CreateHeroBox(row.transform, "SummaryTotalMatches", "TOTAL PARTIDAS", "0", ACCENT_CYAN,   ACCENT_CYAN);
            CreateHeroBox(row.transform, "SummaryWinRate",      "WIN RATE",       "0%", GOLD_PRIMARY,  GOLD_GLOW);
            CreateHeroBox(row.transform, "SummaryNetProfit",    "GANANCIA NETA",  "$0", ACCENT_GREEN,  new Color(0.25f, 1f, 0.5f, 0.25f));
        }

        private static void CreateHeroBox(Transform parent, string name, string label, string value, Color valueColor, Color glowColor)
        {
            GameObject box = new GameObject(name);
            box.transform.SetParent(parent, false);

            Image bg = box.AddComponent<Image>();
            bg.color = CARD_BG_ELEVATED;

            // Triple-layer neon glow (como tournament cards)
            Outline ol1 = box.AddComponent<Outline>();
            ol1.effectColor = new Color(valueColor.r, valueColor.g, valueColor.b, 0.55f);
            ol1.effectDistance = new Vector2(2f, -2f);

            Outline ol2 = box.AddComponent<Outline>();
            ol2.effectColor = new Color(valueColor.r, valueColor.g, valueColor.b, 0.25f);
            ol2.effectDistance = new Vector2(5f, -5f);

            Outline ol3 = box.AddComponent<Outline>();
            ol3.effectColor = new Color(valueColor.r, valueColor.g, valueColor.b, 0.10f);
            ol3.effectDistance = new Vector2(9f, -9f);

            // Accent line at top
            GameObject accent = new GameObject("AccentLine");
            accent.transform.SetParent(box.transform, false);
            RectTransform aRT = accent.AddComponent<RectTransform>();
            aRT.anchorMin = new Vector2(0.1f, 1); aRT.anchorMax = new Vector2(0.9f, 1);
            aRT.pivot = new Vector2(0.5f, 1); aRT.sizeDelta = new Vector2(0, 4);
            Image aImg = accent.AddComponent<Image>();
            aImg.color = valueColor;

            // Value (top 60%)
            GameObject vObj = new GameObject("Value");
            vObj.transform.SetParent(box.transform, false);
            RectTransform vRT = vObj.AddComponent<RectTransform>();
            vRT.anchorMin = new Vector2(0, 0.35f); vRT.anchorMax = new Vector2(1, 1);
            vRT.offsetMin = new Vector2(4, 0); vRT.offsetMax = new Vector2(-4, -12);
            TextMeshProUGUI vTmp = vObj.AddComponent<TextMeshProUGUI>();
            vTmp.text = value;
            vTmp.fontSize = 56; vTmp.color = valueColor;
            vTmp.alignment = TextAlignmentOptions.Center;
            vTmp.fontStyle = FontStyles.Bold;
            vTmp.enableAutoSizing = true; vTmp.fontSizeMin = 32; vTmp.fontSizeMax = 56;

            // Label (bottom 35%)
            GameObject lObj = new GameObject("Label");
            lObj.transform.SetParent(box.transform, false);
            RectTransform lRT = lObj.AddComponent<RectTransform>();
            lRT.anchorMin = new Vector2(0, 0); lRT.anchorMax = new Vector2(1, 0.35f);
            lRT.offsetMin = new Vector2(3, 4); lRT.offsetMax = new Vector2(-3, 0);
            TextMeshProUGUI lTmp = lObj.AddComponent<TextMeshProUGUI>();
            lTmp.text = label;
            lTmp.fontSize = 40; lTmp.color = TEXT_MUTED;
            lTmp.alignment = TextAlignmentOptions.Center;
            lTmp.fontStyle = FontStyles.Bold;
            lTmp.enableAutoSizing = true; lTmp.fontSizeMin = 22; lTmp.fontSizeMax = 40;
        }

        // ================================================================
        //  SECTION HEADER — ═══ TITULO ═══
        // ================================================================

        private static void CreateSectionHeader(Transform parent, string title)
        {
            GameObject row = new GameObject("Section_" + title.Replace(" ", ""));
            row.transform.SetParent(parent, false);

            LayoutElement le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 50; le.flexibleWidth = 1;

            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.padding = new RectOffset(5, 5, 15, 5);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // Left line
            CreateSectionLine(row.transform, "LeftLine");

            // Title text
            GameObject txt = new GameObject("Title");
            txt.transform.SetParent(row.transform, false);
            LayoutElement tle = txt.AddComponent<LayoutElement>();
            tle.flexibleWidth = 0; tle.preferredWidth = 550;
            TextMeshProUGUI tmp = txt.AddComponent<TextMeshProUGUI>();
            tmp.text = title;
            tmp.fontSize = 46; tmp.color = TEXT_GOLD;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.characterSpacing = 6;

            // Right line
            CreateSectionLine(row.transform, "RightLine");
        }

        private static void CreateSectionLine(Transform parent, string name)
        {
            GameObject line = new GameObject(name);
            line.transform.SetParent(parent, false);
            LayoutElement le = line.AddComponent<LayoutElement>();
            le.flexibleWidth = 1; le.preferredHeight = 2;
            Image img = line.AddComponent<Image>();
            img.color = GOLD_GLOW;
        }

        // ================================================================
        //  STATS GRID 5x2 — Double Border Premium
        // ================================================================

        private static void CreateStatsGrid(Transform parent)
        {
            GameObject card = new GameObject("StatsGrid");
            card.transform.SetParent(parent, false);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredHeight = 480; le.flexibleWidth = 1;

            Image bg = card.AddComponent<Image>();
            bg.color = CARD_BG;

            // Doble borde premium
            Outline inner = card.AddComponent<Outline>();
            inner.effectColor = GOLD_BORDER;
            inner.effectDistance = new Vector2(1.5f, -1.5f);

            GridLayoutGroup glg = card.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(200, 220);
            glg.spacing = new Vector2(4, 8);
            glg.padding = new RectOffset(10, 10, 10, 10);
            glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            glg.startAxis = GridLayoutGroup.Axis.Horizontal;
            glg.childAlignment = TextAnchor.MiddleCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 5;

            // Row 1: W / L / D / Streak / Best
            CreateStatItem(card.transform, "stat_victories",     "Victorias",      "24",  ACCENT_GREEN);
            CreateStatItem(card.transform, "stat_defeats",       "Derrotas",       "12",  ACCENT_RED);
            CreateStatItem(card.transform, "stat_draws",         "Empates",        "3",   TEXT_MUTED);
            CreateStatItem(card.transform, "stat_streak",        "Racha Actual",   "5W",  GOLD_PRIMARY);
            CreateStatItem(card.transform, "stat_beststreak",    "Mejor Racha",    "8W",  GOLD_LIGHT);

            // Row 2: Tournaments + Money
            CreateStatItem(card.transform, "stat_tourneysplayed","Torneos Jugados", "6",  ACCENT_CYAN);
            CreateStatItem(card.transform, "stat_tourneyswins",  "Torneos Ganados", "2",  ACCENT_GREEN);
            CreateStatItem(card.transform, "stat_avgearnings",   "Ganancia Prom.",  "$3.90", GOLD_PRIMARY);
            CreateStatItem(card.transform, "stat_totalearned",   "Total Ganado",    "$156",  ACCENT_GREEN);
            CreateStatItem(card.transform, "stat_totalspent",    "Total Gastado",   "$68",   ACCENT_RED);
        }

        private static void CreateStatItem(Transform parent, string iconName, string label, string value, Color valueColor)
        {
            GameObject item = new GameObject("Stat_" + label);
            item.transform.SetParent(parent, false);

            // Image en la celda — fuerza RectTransform + da fondo visual
            Image cellBg = item.AddComponent<Image>();
            cellBg.color = CARD_BG_ELEVATED;

            // Borde sutil por celda
            Outline cellOl = item.AddComponent<Outline>();
            cellOl.effectColor = new Color(GOLD_BORDER.r, GOLD_BORDER.g, GOLD_BORDER.b, 0.3f);
            cellOl.effectDistance = new Vector2(1, -1);

            // Icon (top ~40% de la celda, proporcional)
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(item.transform, false);
            RectTransform iRT = iconObj.AddComponent<RectTransform>();
            iRT.anchorMin = new Vector2(0.20f, 0.58f);
            iRT.anchorMax = new Vector2(0.80f, 0.95f);
            iRT.offsetMin = Vector2.zero;
            iRT.offsetMax = Vector2.zero;

            Image iImg = iconObj.AddComponent<Image>();
            iImg.preserveAspect = true;
            string iconPath = STAT_ICONS_PATH + iconName + ".png";
            Sprite spr = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            if (spr != null) { iImg.sprite = spr; iImg.color = Color.white; }
            else { iImg.color = new Color(valueColor.r, valueColor.g, valueColor.b, 0.30f); }

            // Value (middle ~30%, proporcional)
            GameObject vObj = new GameObject("Value");
            vObj.transform.SetParent(item.transform, false);
            RectTransform vRT = vObj.AddComponent<RectTransform>();
            vRT.anchorMin = new Vector2(0.02f, 0.24f);
            vRT.anchorMax = new Vector2(0.98f, 0.56f);
            vRT.offsetMin = Vector2.zero;
            vRT.offsetMax = Vector2.zero;

            TextMeshProUGUI vTmp = vObj.AddComponent<TextMeshProUGUI>();
            vTmp.text = value; vTmp.fontSize = 50;
            vTmp.color = valueColor;
            vTmp.alignment = TextAlignmentOptions.Center;
            vTmp.fontStyle = FontStyles.Bold;
            vTmp.enableAutoSizing = true; vTmp.fontSizeMin = 28; vTmp.fontSizeMax = 50;

            // Label (bottom ~24%, proporcional)
            GameObject lObj = new GameObject("Label");
            lObj.transform.SetParent(item.transform, false);
            RectTransform lRT = lObj.AddComponent<RectTransform>();
            lRT.anchorMin = new Vector2(0.02f, 0.02f);
            lRT.anchorMax = new Vector2(0.98f, 0.24f);
            lRT.offsetMin = Vector2.zero;
            lRT.offsetMax = Vector2.zero;

            TextMeshProUGUI lTmp = lObj.AddComponent<TextMeshProUGUI>();
            lTmp.text = label; lTmp.fontSize = 36;
            lTmp.color = TEXT_MUTED;
            lTmp.alignment = TextAlignmentOptions.Center;
            lTmp.fontStyle = FontStyles.Bold;
            lTmp.enableAutoSizing = true; lTmp.fontSizeMin = 18; lTmp.fontSizeMax = 36;
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
        //  ANIMATION MANAGERS (GameObject root en hierarchy)
        // ================================================================

        private static void CreateAnimationManagers()
        {
            // Buscar o crear el root ---ANIMATION_MANAGERS---
            GameObject managersRoot = GameObject.Find("---ANIMATION_MANAGERS---");
            if (managersRoot == null)
                managersRoot = new GameObject("---ANIMATION_MANAGERS---");

            // Agregar CashProfileAnimator como hijo
            Transform existing = managersRoot.transform.Find("CashProfileAnimator");
            if (existing == null)
            {
                GameObject animObj = new GameObject("CashProfileAnimator");
                animObj.transform.SetParent(managersRoot.transform);
                UIBuilderAnimationUtils.AddCashProfileAnimator(animObj);
            }

            // Agregar UIAnimationManager si no existe en la escena
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
                Debug.LogWarning("[CashProfileUIBuilder] CashProfileSceneController no encontrado. Las referencias se asignarán cuando el controller esté en la escena.");
                AddAR("Controller", "No encontrado en escena", false, null);
                failedCount++;
                return;
            }

            SerializedObject so = new SerializedObject(ctrl);
            so.Update();

            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            Transform root = canvas != null ? canvas.transform : ctrl.transform.root;

            // ── Header ──
            Assign(so, "backButton",  FindBtn(root, "BackButton"));
            Assign(so, "titleText",   FindTMP(root, "TitleText"));

            // ── Avatar ──
            Transform avT = Deep(root, "AvatarImage");
            Assign(so, "avatarImage", avT != null ? avT.GetComponent<Image>() : null);
            Assign(so, "usernameText",    FindTMP(root, "UsernameText"));
            Assign(so, "memberSinceText", FindTMP(root, "MemberSinceText"));

            // ── Summary (Hero) Stats ──
            Assign(so, "summaryTotalMatchesText", FindValue(root, "SummaryTotalMatches"));
            Assign(so, "summaryWinRateText",      FindValue(root, "SummaryWinRate"));
            Assign(so, "summaryNetProfitText",    FindValue(root, "SummaryNetProfit"));

            // ── Stats Grid ──
            Assign(so, "winsText",              FindValue(root, "Stat_Victorias"));
            Assign(so, "lossesText",            FindValue(root, "Stat_Derrotas"));
            Assign(so, "drawsText",             FindValue(root, "Stat_Empates"));
            Assign(so, "currentStreakText",      FindValue(root, "Stat_Racha Actual"));
            Assign(so, "bestStreakText",         FindValue(root, "Stat_Mejor Racha"));
            Assign(so, "tournamentsPlayedText",  FindValue(root, "Stat_Torneos Jugados"));
            Assign(so, "tournamentWinsText",     FindValue(root, "Stat_Torneos Ganados"));
            Assign(so, "avgEarningsText",        FindValue(root, "Stat_Ganancia Prom."));
            Assign(so, "totalEarningsText",      FindValue(root, "Stat_Total Ganado"));
            Assign(so, "totalSpentText",         FindValue(root, "Stat_Total Gastado"));

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(ctrl);
            EditorUtility.SetDirty(ctrl.gameObject);
            EditorSceneManager.MarkSceneDirty(ctrl.gameObject.scene);

            int ok = assignedCount + alreadySetCount;
            int total = ok + failedCount;
            Debug.Log($"[CashProfileUIBuilder] === {ok}/{total} REFERENCIAS ASIGNADAS ===");
        }

        // ── Button helpers ──

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

        // ── Deep finders ──

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

        /// <summary>
        /// Busca un GameObject por nombre, luego busca hijo "Value" con TMP.
        /// Funciona para SummaryBoxes (Value) y Stat items (Value).
        /// </summary>
        private static TextMeshProUGUI FindValue(Transform root, string parentName)
        {
            Transform p = Deep(root, parentName);
            if (p == null) return null;
            Transform v = p.Find("Value");
            return v != null ? v.GetComponent<TextMeshProUGUI>() : null;
        }

        // ── Assignment helper ──

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

        // ── Results GUI ──

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
