using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;

namespace DigitPark.Editor
{
    /// <summary>
    /// Builder para Battle Pass VERTICAL estilo Clash Royale.
    /// Scroll vertical con línea central, tiers alternados, y gran premio en la cima.
    /// </summary>
    public class BattlePassVerticalUIBuilder : EditorWindow
    {
        #region Colors

        private static readonly Color DARK_BG = new Color(0.02f, 0.04f, 0.08f, 1f);
        private static readonly Color CARD_BG = new Color(0.08f, 0.10f, 0.15f, 1f);
        private static readonly Color CARD_LOCKED = new Color(0.06f, 0.07f, 0.10f, 0.9f);

        private static readonly Color CYAN_GLOW = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.5f, 1f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GOLD_DARK = new Color(0.6f, 0.5f, 0f, 1f);
        private static readonly Color PURPLE = new Color(0.7f, 0.3f, 1f, 1f);
        private static readonly Color PURPLE_DARK = new Color(0.3f, 0.1f, 0.5f, 1f);
        private static readonly Color GREEN = new Color(0.2f, 1f, 0.4f, 1f);
        private static readonly Color RED = new Color(1f, 0.3f, 0.3f, 1f);

        private static readonly Color TEXT_WHITE = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.6f, 0.7f, 1f);
        private static readonly Color LINE_LOCKED = new Color(0.2f, 0.2f, 0.25f, 1f);
        private static readonly Color LINE_UNLOCKED = new Color(0f, 0.8f, 1f, 1f);
        private static readonly Color LINE_CLAIMED = new Color(0.3f, 0.8f, 0.4f, 1f);

        #endregion

        #region Layout Constants

        private const float HEADER_HEIGHT = 60f;
        private const float PROGRESS_BAR_HEIGHT = 50f;
        private const float SIDE_PADDING = 16f;

        private const float TIER_HEIGHT = 120f;
        private const float TIER_SPACING = 20f;
        private const float TIER_CARD_WIDTH = 140f;
        private const float TIER_CARD_HEIGHT = 100f;

        private const float LINE_WIDTH = 6f;
        private const float NODE_SIZE = 24f;

        private const int TOTAL_TIERS = 50;
        private const int CURRENT_TIER = 12; // For preview

        #endregion

        #region Paths

        private static readonly string ICONS_PATH = "Assets/_Project/Art/Icons/";
        private static readonly string CHEST_ICONS_PATH = "Assets/_Project/Art/Icons/Chests/";
        private static readonly string BP_ICONS_PATH = "Assets/_Project/Art/Icons/BattlePass/";

        #endregion

        [MenuItem("DigitPark/UI Builders/Monetization/Battle Pass Vertical", false, 260)]
        public static void ShowWindow()
        {
            GetWindow<BattlePassVerticalUIBuilder>("Battle Pass Vertical Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Battle Pass Vertical Builder", EditorStyles.boldLabel);
            GUILayout.Label("Estilo Clash Royale - Scroll Vertical Épico", EditorStyles.miniLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Crea un Battle Pass vertical de alto impacto:\n\n" +
                "• Scroll vertical full-screen\n" +
                "• Línea central brillante con nodos\n" +
                "• Tiers alternados izquierda/derecha\n" +
                "• Gran Premio en la cima\n" +
                "• Efectos de glow en tier actual\n" +
                "• 50 niveles de recompensas",
                MessageType.Info);

            EditorGUILayout.Space(15);

            GUI.backgroundColor = CYAN_GLOW;
            if (GUILayout.Button("CONSTRUIR BATTLE PASS VERTICAL", GUILayout.Height(50)))
            {
                BuildBattlePassUI();
            }
            GUI.backgroundColor = Color.white;
        }

        #region Main Build

        private static void BuildBattlePassUI()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontró Canvas. Abre la escena BattlePass.", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog("Reconstruir Battle Pass?",
                "Esto creará el nuevo diseño vertical del Battle Pass.\n\n¿Continuar?",
                "Sí", "Cancelar"))
            {
                return;
            }

            Debug.Log($"[BattlePassVerticalUIBuilder] Canvas encontrado: {canvas.name}, hijos: {canvas.transform.childCount}");

            // Cleanup - Eliminar TODOS los hijos del Canvas excepto EventSystem
            var toDelete = new List<GameObject>();
            foreach (Transform child in canvas.transform)
            {
                // Keep EventSystem but delete everything else
                if (child.name == "EventSystem" || child.GetComponent<UnityEngine.EventSystems.EventSystem>() != null)
                {
                    Debug.Log($"[BattlePassVerticalUIBuilder] Preservando: {child.name}");
                    continue;
                }

                toDelete.Add(child.gameObject);
                Debug.Log($"[BattlePassVerticalUIBuilder] Marcado para eliminar: {child.name}");
            }

            foreach (var obj in toDelete)
            {
                Debug.Log($"[BattlePassVerticalUIBuilder] Eliminando: {obj.name}");
                DestroyImmediate(obj);
            }

            Debug.Log($"[BattlePassVerticalUIBuilder] Después de limpieza, hijos restantes: {canvas.transform.childCount}");

            // Build UI
            GameObject root = CreateRoot(canvas.transform);

            // Asegurar que el root esté al frente (último sibling = renderizado encima)
            root.transform.SetAsLastSibling();

            CreateHeader(root.transform);
            CreateProgressBar(root.transform);
            CreateMainScrollView(root.transform);
            CreatePremiumButton(root.transform);

            // Forzar actualización del Canvas
            Canvas.ForceUpdateCanvases();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"[BattlePassVerticalUIBuilder] UI construida! Root: {root.name}, activo: {root.activeSelf}");

            // Seleccionar el objeto root para que sea visible en el inspector
            Selection.activeGameObject = root;
        }

        #endregion

        #region Root

        private static GameObject CreateRoot(Transform parent)
        {
            GameObject root = new GameObject("BattlePassUI");
            root.transform.SetParent(parent, false);

            RectTransform rt = root.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = root.AddComponent<Image>();
            bg.color = DARK_BG;

            return root;
        }

        #endregion

        #region Header

        private static void CreateHeader(Transform parent)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(parent, false);

            RectTransform rt = header.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.offsetMin = new Vector2(0, -HEADER_HEIGHT);
            rt.offsetMax = Vector2.zero;

            Image bg = header.AddComponent<Image>();
            bg.color = new Color(0.03f, 0.05f, 0.1f, 0.95f);

            // Back Button
            GameObject backBtn = new GameObject("BackButton");
            backBtn.transform.SetParent(header.transform, false);

            RectTransform backRT = backBtn.AddComponent<RectTransform>();
            backRT.anchorMin = new Vector2(0, 0.5f);
            backRT.anchorMax = new Vector2(0, 0.5f);
            backRT.pivot = new Vector2(0, 0.5f);
            backRT.sizeDelta = new Vector2(45, 45);
            backRT.anchoredPosition = new Vector2(SIDE_PADDING, 0);

            Image backBg = backBtn.AddComponent<Image>();
            backBg.color = CARD_BG;

            Button backButton = backBtn.AddComponent<Button>();
            backButton.targetGraphic = backBg;

            // Arrow
            GameObject arrow = new GameObject("Arrow");
            arrow.transform.SetParent(backBtn.transform, false);
            RectTransform arrowRT = arrow.AddComponent<RectTransform>();
            arrowRT.anchorMin = Vector2.zero;
            arrowRT.anchorMax = Vector2.one;
            arrowRT.offsetMin = Vector2.zero;
            arrowRT.offsetMax = Vector2.zero;

            TextMeshProUGUI arrowTMP = arrow.AddComponent<TextMeshProUGUI>();
            arrowTMP.text = "‹";
            arrowTMP.fontSize = 32;
            arrowTMP.color = TEXT_WHITE;
            arrowTMP.alignment = TextAlignmentOptions.Center;

            // Title
            GameObject title = new GameObject("Title");
            title.transform.SetParent(header.transform, false);

            RectTransform titleRT = title.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.offsetMin = new Vector2(70, 0);
            titleRT.offsetMax = new Vector2(-70, 0);

            TextMeshProUGUI titleTMP = title.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "PASE DE BATALLA";
            titleTMP.fontSize = 22;
            titleTMP.color = CYAN_GLOW;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.verticalAlignment = VerticalAlignmentOptions.Middle;

            // Season Badge
            GameObject season = new GameObject("SeasonBadge");
            season.transform.SetParent(header.transform, false);

            RectTransform seasonRT = season.AddComponent<RectTransform>();
            seasonRT.anchorMin = new Vector2(1, 0.5f);
            seasonRT.anchorMax = new Vector2(1, 0.5f);
            seasonRT.pivot = new Vector2(1, 0.5f);
            seasonRT.sizeDelta = new Vector2(50, 30);
            seasonRT.anchoredPosition = new Vector2(-SIDE_PADDING, 0);

            Image seasonBg = season.AddComponent<Image>();
            seasonBg.color = PURPLE_DARK;

            Outline seasonOutline = season.AddComponent<Outline>();
            seasonOutline.effectColor = PURPLE;
            seasonOutline.effectDistance = new Vector2(1, -1);

            GameObject seasonText = new GameObject("Text");
            seasonText.transform.SetParent(season.transform, false);
            RectTransform seasonTextRT = seasonText.AddComponent<RectTransform>();
            seasonTextRT.anchorMin = Vector2.zero;
            seasonTextRT.anchorMax = Vector2.one;
            seasonTextRT.offsetMin = Vector2.zero;
            seasonTextRT.offsetMax = Vector2.zero;

            TextMeshProUGUI seasonTMP = seasonText.AddComponent<TextMeshProUGUI>();
            seasonTMP.text = "S1";
            seasonTMP.fontSize = 16;
            seasonTMP.color = TEXT_WHITE;
            seasonTMP.fontStyle = FontStyles.Bold;
            seasonTMP.alignment = TextAlignmentOptions.Center;
        }

        #endregion

        #region Progress Bar

        private static void CreateProgressBar(Transform parent)
        {
            float yPos = HEADER_HEIGHT;

            GameObject progressSection = new GameObject("ProgressSection");
            progressSection.transform.SetParent(parent, false);

            RectTransform rt = progressSection.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.offsetMin = new Vector2(SIDE_PADDING, -(yPos + PROGRESS_BAR_HEIGHT));
            rt.offsetMax = new Vector2(-SIDE_PADDING, -yPos);

            // Level Badge (left)
            GameObject levelBadge = new GameObject("LevelBadge");
            levelBadge.transform.SetParent(progressSection.transform, false);

            RectTransform levelRT = levelBadge.AddComponent<RectTransform>();
            levelRT.anchorMin = new Vector2(0, 0.5f);
            levelRT.anchorMax = new Vector2(0, 0.5f);
            levelRT.pivot = new Vector2(0, 0.5f);
            levelRT.sizeDelta = new Vector2(70, 40);
            levelRT.anchoredPosition = Vector2.zero;

            Image levelBg = levelBadge.AddComponent<Image>();
            levelBg.color = CYAN_DARK;

            Outline levelOutline = levelBadge.AddComponent<Outline>();
            levelOutline.effectColor = CYAN_GLOW;
            levelOutline.effectDistance = new Vector2(2, -2);

            GameObject levelText = new GameObject("Text");
            levelText.transform.SetParent(levelBadge.transform, false);
            RectTransform levelTextRT = levelText.AddComponent<RectTransform>();
            levelTextRT.anchorMin = Vector2.zero;
            levelTextRT.anchorMax = Vector2.one;
            levelTextRT.offsetMin = Vector2.zero;
            levelTextRT.offsetMax = Vector2.zero;

            TextMeshProUGUI levelTMP = levelText.AddComponent<TextMeshProUGUI>();
            levelTMP.text = $"Nv.{CURRENT_TIER}";
            levelTMP.fontSize = 18;
            levelTMP.color = TEXT_WHITE;
            levelTMP.fontStyle = FontStyles.Bold;
            levelTMP.alignment = TextAlignmentOptions.Center;

            // Progress Bar (center)
            GameObject barContainer = new GameObject("ProgressBarContainer");
            barContainer.transform.SetParent(progressSection.transform, false);

            RectTransform barContRT = barContainer.AddComponent<RectTransform>();
            barContRT.anchorMin = new Vector2(0, 0.5f);
            barContRT.anchorMax = new Vector2(1, 0.5f);
            barContRT.pivot = new Vector2(0.5f, 0.5f);
            barContRT.offsetMin = new Vector2(80, -12);
            barContRT.offsetMax = new Vector2(-80, 12);

            Image barBg = barContainer.AddComponent<Image>();
            barBg.color = new Color(0.1f, 0.1f, 0.15f, 1f);

            // Fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(barContainer.transform, false);

            RectTransform fillRT = fill.AddComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = new Vector2(0.75f, 1f); // 75% for preview
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;

            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = CYAN_GLOW;

            // Glow effect on fill edge
            GameObject glow = new GameObject("Glow");
            glow.transform.SetParent(fill.transform, false);

            RectTransform glowRT = glow.AddComponent<RectTransform>();
            glowRT.anchorMin = new Vector2(1, 0);
            glowRT.anchorMax = new Vector2(1, 1);
            glowRT.pivot = new Vector2(1, 0.5f);
            glowRT.sizeDelta = new Vector2(20, 0);
            glowRT.anchoredPosition = Vector2.zero;

            Image glowImg = glow.AddComponent<Image>();
            glowImg.color = new Color(0f, 1f, 1f, 0.5f);

            // XP Text (right)
            GameObject xpText = new GameObject("XPText");
            xpText.transform.SetParent(progressSection.transform, false);

            RectTransform xpRT = xpText.AddComponent<RectTransform>();
            xpRT.anchorMin = new Vector2(1, 0.5f);
            xpRT.anchorMax = new Vector2(1, 0.5f);
            xpRT.pivot = new Vector2(1, 0.5f);
            xpRT.sizeDelta = new Vector2(70, 30);
            xpRT.anchoredPosition = Vector2.zero;

            TextMeshProUGUI xpTMP = xpText.AddComponent<TextMeshProUGUI>();
            xpTMP.text = "750/1000";
            xpTMP.fontSize = 14;
            xpTMP.color = TEXT_SECONDARY;
            xpTMP.alignment = TextAlignmentOptions.Right;
            xpTMP.verticalAlignment = VerticalAlignmentOptions.Middle;
        }

        #endregion

        #region Main ScrollView

        private static void CreateMainScrollView(Transform parent)
        {
            float yPos = HEADER_HEIGHT + PROGRESS_BAR_HEIGHT + 10;

            GameObject scrollSection = new GameObject("ScrollSection");
            scrollSection.transform.SetParent(parent, false);

            RectTransform sectionRT = scrollSection.AddComponent<RectTransform>();
            sectionRT.anchorMin = Vector2.zero;
            sectionRT.anchorMax = Vector2.one;
            sectionRT.offsetMin = new Vector2(0, 70); // Space for premium button
            sectionRT.offsetMax = new Vector2(0, -yPos);

            // ScrollView
            GameObject scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(scrollSection.transform, false);

            RectTransform svRT = scrollView.AddComponent<RectTransform>();
            svRT.anchorMin = Vector2.zero;
            svRT.anchorMax = Vector2.one;
            svRT.offsetMin = Vector2.zero;
            svRT.offsetMax = Vector2.zero;

            Image svBg = scrollView.AddComponent<Image>();
            svBg.color = Color.clear;

            ScrollRect sr = scrollView.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Elastic;
            sr.elasticity = 0.1f;
            sr.scrollSensitivity = 30;

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);

            RectTransform vpRT = viewport.AddComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = Vector2.zero;
            vpRT.offsetMax = Vector2.zero;

            viewport.AddComponent<RectMask2D>();
            sr.viewport = vpRT;

            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);

            float totalContentHeight = (TOTAL_TIERS + 1) * (TIER_HEIGHT + TIER_SPACING) + 200; // Extra for grand prize

            RectTransform cRT = content.AddComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0, 0);
            cRT.anchorMax = new Vector2(1, 0);
            cRT.pivot = new Vector2(0.5f, 0);
            cRT.sizeDelta = new Vector2(0, totalContentHeight);
            cRT.anchoredPosition = Vector2.zero;

            sr.content = cRT;

            // Create the vertical path/line
            CreateVerticalPath(content.transform, totalContentHeight);

            // Create all tier nodes
            CreateAllTiers(content.transform);

            // Create Grand Prize at top
            CreateGrandPrize(content.transform, totalContentHeight);
        }

        #endregion

        #region Vertical Path (Central Line)

        private static void CreateVerticalPath(Transform content, float totalHeight)
        {
            GameObject pathContainer = new GameObject("PathContainer");
            pathContainer.transform.SetParent(content, false);

            RectTransform pathRT = pathContainer.AddComponent<RectTransform>();
            pathRT.anchorMin = new Vector2(0.5f, 0);
            pathRT.anchorMax = new Vector2(0.5f, 1);
            pathRT.pivot = new Vector2(0.5f, 0);
            pathRT.sizeDelta = new Vector2(LINE_WIDTH, 0);
            pathRT.anchoredPosition = Vector2.zero;

            // Background line (locked state)
            GameObject lineBg = new GameObject("LineBg");
            lineBg.transform.SetParent(pathContainer.transform, false);

            RectTransform lineBgRT = lineBg.AddComponent<RectTransform>();
            lineBgRT.anchorMin = Vector2.zero;
            lineBgRT.anchorMax = Vector2.one;
            lineBgRT.offsetMin = Vector2.zero;
            lineBgRT.offsetMax = Vector2.zero;

            Image lineBgImg = lineBg.AddComponent<Image>();
            lineBgImg.color = LINE_LOCKED;

            // Progress line (unlocked/claimed)
            float progressPercent = (float)CURRENT_TIER / TOTAL_TIERS;

            GameObject lineProgress = new GameObject("LineProgress");
            lineProgress.transform.SetParent(pathContainer.transform, false);

            RectTransform lineProgRT = lineProgress.AddComponent<RectTransform>();
            lineProgRT.anchorMin = Vector2.zero;
            lineProgRT.anchorMax = new Vector2(1, progressPercent);
            lineProgRT.offsetMin = Vector2.zero;
            lineProgRT.offsetMax = Vector2.zero;

            Image lineProgImg = lineProgress.AddComponent<Image>();
            lineProgImg.color = LINE_UNLOCKED;

            // Glow overlay on progress line
            GameObject lineGlow = new GameObject("LineGlow");
            lineGlow.transform.SetParent(lineProgress.transform, false);

            RectTransform lineGlowRT = lineGlow.AddComponent<RectTransform>();
            lineGlowRT.anchorMin = new Vector2(-2, 0);
            lineGlowRT.anchorMax = new Vector2(3, 1);
            lineGlowRT.offsetMin = Vector2.zero;
            lineGlowRT.offsetMax = Vector2.zero;

            Image lineGlowImg = lineGlow.AddComponent<Image>();
            lineGlowImg.color = new Color(0f, 1f, 1f, 0.2f);
        }

        #endregion

        #region Tier Creation

        private static void CreateAllTiers(Transform content)
        {
            GameObject tiersContainer = new GameObject("TiersContainer");
            tiersContainer.transform.SetParent(content, false);

            RectTransform tiersRT = tiersContainer.AddComponent<RectTransform>();
            tiersRT.anchorMin = Vector2.zero;
            tiersRT.anchorMax = Vector2.one;
            tiersRT.offsetMin = Vector2.zero;
            tiersRT.offsetMax = Vector2.zero;

            for (int i = 1; i <= TOTAL_TIERS; i++)
            {
                CreateTierNode(tiersContainer.transform, i);
            }
        }

        private static void CreateTierNode(Transform parent, int tierNumber)
        {
            bool isLeft = (tierNumber % 2 == 1); // Odd = left, Even = right
            bool isClaimed = tierNumber < CURRENT_TIER;
            bool isCurrent = tierNumber == CURRENT_TIER;
            bool isLocked = tierNumber > CURRENT_TIER;
            bool isSpecial = (tierNumber % 10 == 0); // Every 10th tier is special

            float yPos = tierNumber * (TIER_HEIGHT + TIER_SPACING);
            float xOffset = isLeft ? -100 : 100;

            GameObject tierNode = new GameObject($"Tier_{tierNumber}");
            tierNode.transform.SetParent(parent, false);

            RectTransform nodeRT = tierNode.AddComponent<RectTransform>();
            nodeRT.anchorMin = new Vector2(0.5f, 0);
            nodeRT.anchorMax = new Vector2(0.5f, 0);
            nodeRT.pivot = new Vector2(0.5f, 0.5f);
            nodeRT.sizeDelta = new Vector2(TIER_CARD_WIDTH, TIER_CARD_HEIGHT);
            nodeRT.anchoredPosition = new Vector2(xOffset, yPos);

            // Scale for current tier
            if (isCurrent)
            {
                nodeRT.localScale = new Vector3(1.15f, 1.15f, 1f);
            }

            // Card Background
            Image cardBg = tierNode.AddComponent<Image>();
            if (isCurrent)
            {
                cardBg.color = CYAN_DARK;
            }
            else if (isClaimed)
            {
                cardBg.color = new Color(CARD_BG.r, CARD_BG.g, CARD_BG.b, 0.5f);
            }
            else if (isLocked)
            {
                cardBg.color = CARD_LOCKED;
            }
            else
            {
                cardBg.color = CARD_BG;
            }

            // Outline
            Outline outline = tierNode.AddComponent<Outline>();
            if (isCurrent)
            {
                outline.effectColor = CYAN_GLOW;
                outline.effectDistance = new Vector2(3, -3);
            }
            else if (isSpecial)
            {
                outline.effectColor = GOLD;
                outline.effectDistance = new Vector2(2, -2);
            }
            else
            {
                outline.effectColor = new Color(0.3f, 0.3f, 0.4f, 0.5f);
                outline.effectDistance = new Vector2(1, -1);
            }

            // Button for interaction
            Button btn = tierNode.AddComponent<Button>();
            btn.targetGraphic = cardBg;

            // Tier Number Badge
            CreateTierBadge(tierNode.transform, tierNumber, isCurrent, isSpecial);

            // Reward Icon
            CreateRewardIcon(tierNode.transform, tierNumber, isClaimed, isLocked, isSpecial);

            // Claimed Checkmark
            if (isClaimed)
            {
                CreateClaimedCheckmark(tierNode.transform);
            }

            // Lock Icon for locked tiers
            if (isLocked)
            {
                CreateLockIcon(tierNode.transform);
            }

            // Connection node on the center line
            CreateConnectionNode(tierNode.transform, isLeft, isClaimed, isCurrent);

            // Premium indicator (for even tiers - right side)
            if (!isLeft)
            {
                CreatePremiumBadge(tierNode.transform);
            }
        }

        private static void CreateTierBadge(Transform parent, int tierNumber, bool isCurrent, bool isSpecial)
        {
            GameObject badge = new GameObject("TierBadge");
            badge.transform.SetParent(parent, false);

            RectTransform badgeRT = badge.AddComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(0.5f, 1);
            badgeRT.anchorMax = new Vector2(0.5f, 1);
            badgeRT.pivot = new Vector2(0.5f, 0.5f);
            badgeRT.sizeDelta = new Vector2(40, 24);
            badgeRT.anchoredPosition = new Vector2(0, 5);

            Image badgeBg = badge.AddComponent<Image>();
            if (isCurrent)
            {
                badgeBg.color = CYAN_GLOW;
            }
            else if (isSpecial)
            {
                badgeBg.color = GOLD;
            }
            else
            {
                badgeBg.color = new Color(0.2f, 0.2f, 0.3f, 1f);
            }

            GameObject text = new GameObject("Text");
            text.transform.SetParent(badge.transform, false);

            RectTransform textRT = text.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            TextMeshProUGUI textTMP = text.AddComponent<TextMeshProUGUI>();
            textTMP.text = tierNumber.ToString();
            textTMP.fontSize = 14;
            textTMP.color = (isCurrent || isSpecial) ? Color.black : TEXT_WHITE;
            textTMP.fontStyle = FontStyles.Bold;
            textTMP.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateRewardIcon(Transform parent, int tierNumber, bool isClaimed, bool isLocked, bool isSpecial)
        {
            GameObject iconContainer = new GameObject("RewardIcon");
            iconContainer.transform.SetParent(parent, false);

            RectTransform iconRT = iconContainer.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.pivot = new Vector2(0.5f, 0.5f);
            iconRT.sizeDelta = isSpecial ? new Vector2(60, 60) : new Vector2(50, 50);
            iconRT.anchoredPosition = new Vector2(0, -5);

            Image iconImg = iconContainer.AddComponent<Image>();
            iconImg.preserveAspect = true;

            // Determine reward type and load icon
            string iconPath;
            Color iconColor;

            if (isSpecial)
            {
                // Every 10th level = Chest
                string chestType = (tierNumber == 50) ? "mythic" :
                                   (tierNumber >= 40) ? "legendary" :
                                   (tierNumber >= 30) ? "epic" :
                                   (tierNumber >= 20) ? "rare" : "common";
                iconPath = CHEST_ICONS_PATH + $"icon_chest_{chestType}_closed.png";
                iconColor = Color.white;
            }
            else if (tierNumber % 5 == 0)
            {
                // Every 5th level = Gems
                iconPath = ICONS_PATH + "Currency/icon_gems.png";
                iconColor = CYAN_GLOW;
            }
            else
            {
                // Normal = Coins
                iconPath = ICONS_PATH + "Currency/icon_coins.png";
                iconColor = GOLD;
            }

            Sprite rewardSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            if (rewardSprite != null)
            {
                iconImg.sprite = rewardSprite;
                iconImg.color = Color.white;
            }
            else
            {
                iconImg.color = iconColor;
            }

            // Dim if claimed
            if (isClaimed)
            {
                iconImg.color = new Color(iconImg.color.r, iconImg.color.g, iconImg.color.b, 0.4f);
            }
            else if (isLocked)
            {
                iconImg.color = new Color(0.3f, 0.3f, 0.3f, 0.6f);
            }

            // Reward amount text
            GameObject amountText = new GameObject("Amount");
            amountText.transform.SetParent(parent, false);

            RectTransform amountRT = amountText.AddComponent<RectTransform>();
            amountRT.anchorMin = new Vector2(0.5f, 0);
            amountRT.anchorMax = new Vector2(0.5f, 0);
            amountRT.pivot = new Vector2(0.5f, 0);
            amountRT.sizeDelta = new Vector2(100, 20);
            amountRT.anchoredPosition = new Vector2(0, 5);

            TextMeshProUGUI amountTMP = amountText.AddComponent<TextMeshProUGUI>();

            if (isSpecial)
            {
                amountTMP.text = "COFRE";
                amountTMP.color = GOLD;
            }
            else if (tierNumber % 5 == 0)
            {
                amountTMP.text = $"+{10 + tierNumber} 💎";
                amountTMP.color = CYAN_GLOW;
            }
            else
            {
                amountTMP.text = $"+{50 + tierNumber * 5} 🪙";
                amountTMP.color = GOLD;
            }

            amountTMP.fontSize = 11;
            amountTMP.fontStyle = FontStyles.Bold;
            amountTMP.alignment = TextAlignmentOptions.Center;

            if (isClaimed || isLocked)
            {
                amountTMP.color = new Color(amountTMP.color.r, amountTMP.color.g, amountTMP.color.b, 0.4f);
            }
        }

        private static void CreateClaimedCheckmark(Transform parent)
        {
            GameObject check = new GameObject("ClaimedCheck");
            check.transform.SetParent(parent, false);

            RectTransform checkRT = check.AddComponent<RectTransform>();
            checkRT.anchorMin = new Vector2(1, 1);
            checkRT.anchorMax = new Vector2(1, 1);
            checkRT.pivot = new Vector2(1, 1);
            checkRT.sizeDelta = new Vector2(28, 28);
            checkRT.anchoredPosition = new Vector2(5, 5);

            Image checkBg = check.AddComponent<Image>();
            checkBg.color = GREEN;

            GameObject checkText = new GameObject("Text");
            checkText.transform.SetParent(check.transform, false);

            RectTransform checkTextRT = checkText.AddComponent<RectTransform>();
            checkTextRT.anchorMin = Vector2.zero;
            checkTextRT.anchorMax = Vector2.one;
            checkTextRT.offsetMin = Vector2.zero;
            checkTextRT.offsetMax = Vector2.zero;

            TextMeshProUGUI checkTMP = checkText.AddComponent<TextMeshProUGUI>();
            checkTMP.text = "✓";
            checkTMP.fontSize = 18;
            checkTMP.color = Color.white;
            checkTMP.fontStyle = FontStyles.Bold;
            checkTMP.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateLockIcon(Transform parent)
        {
            GameObject lockIcon = new GameObject("LockIcon");
            lockIcon.transform.SetParent(parent, false);

            RectTransform lockRT = lockIcon.AddComponent<RectTransform>();
            lockRT.anchorMin = new Vector2(1, 1);
            lockRT.anchorMax = new Vector2(1, 1);
            lockRT.pivot = new Vector2(1, 1);
            lockRT.sizeDelta = new Vector2(24, 24);
            lockRT.anchoredPosition = new Vector2(5, 5);

            Image lockBg = lockIcon.AddComponent<Image>();
            lockBg.color = new Color(0.3f, 0.3f, 0.35f, 0.9f);

            GameObject lockText = new GameObject("Text");
            lockText.transform.SetParent(lockIcon.transform, false);

            RectTransform lockTextRT = lockText.AddComponent<RectTransform>();
            lockTextRT.anchorMin = Vector2.zero;
            lockTextRT.anchorMax = Vector2.one;
            lockTextRT.offsetMin = Vector2.zero;
            lockTextRT.offsetMax = Vector2.zero;

            TextMeshProUGUI lockTMP = lockText.AddComponent<TextMeshProUGUI>();
            lockTMP.text = "🔒";
            lockTMP.fontSize = 12;
            lockTMP.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateConnectionNode(Transform parent, bool isLeft, bool isClaimed, bool isCurrent)
        {
            GameObject node = new GameObject("ConnectionNode");
            node.transform.SetParent(parent, false);

            RectTransform nodeRT = node.AddComponent<RectTransform>();
            nodeRT.anchorMin = new Vector2(isLeft ? 1 : 0, 0.5f);
            nodeRT.anchorMax = new Vector2(isLeft ? 1 : 0, 0.5f);
            nodeRT.pivot = new Vector2(0.5f, 0.5f);
            nodeRT.sizeDelta = new Vector2(NODE_SIZE, NODE_SIZE);
            nodeRT.anchoredPosition = new Vector2(isLeft ? 30 : -30, 0);

            Image nodeImg = node.AddComponent<Image>();

            if (isCurrent)
            {
                nodeImg.color = CYAN_GLOW;
            }
            else if (isClaimed)
            {
                nodeImg.color = LINE_CLAIMED;
            }
            else
            {
                nodeImg.color = LINE_LOCKED;
            }

            // Connection line to center
            GameObject line = new GameObject("ConnectionLine");
            line.transform.SetParent(node.transform, false);

            RectTransform lineRT = line.AddComponent<RectTransform>();
            lineRT.anchorMin = new Vector2(0.5f, 0.5f);
            lineRT.anchorMax = new Vector2(0.5f, 0.5f);
            lineRT.pivot = new Vector2(isLeft ? 0 : 1, 0.5f);
            lineRT.sizeDelta = new Vector2(50, 3);
            lineRT.anchoredPosition = new Vector2(isLeft ? NODE_SIZE/2 : -NODE_SIZE/2, 0);

            Image lineImg = line.AddComponent<Image>();
            lineImg.color = nodeImg.color;
        }

        private static void CreatePremiumBadge(Transform parent)
        {
            GameObject badge = new GameObject("PremiumBadge");
            badge.transform.SetParent(parent, false);

            RectTransform badgeRT = badge.AddComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(0, 1);
            badgeRT.anchorMax = new Vector2(0, 1);
            badgeRT.pivot = new Vector2(0, 1);
            badgeRT.sizeDelta = new Vector2(24, 24);
            badgeRT.anchoredPosition = new Vector2(-5, 5);

            Image badgeBg = badge.AddComponent<Image>();
            badgeBg.color = PURPLE;

            GameObject star = new GameObject("Star");
            star.transform.SetParent(badge.transform, false);

            RectTransform starRT = star.AddComponent<RectTransform>();
            starRT.anchorMin = Vector2.zero;
            starRT.anchorMax = Vector2.one;
            starRT.offsetMin = Vector2.zero;
            starRT.offsetMax = Vector2.zero;

            TextMeshProUGUI starTMP = star.AddComponent<TextMeshProUGUI>();
            starTMP.text = "★";
            starTMP.fontSize = 14;
            starTMP.color = Color.white;
            starTMP.alignment = TextAlignmentOptions.Center;
        }

        #endregion

        #region Grand Prize

        private static void CreateGrandPrize(Transform content, float totalHeight)
        {
            float yPos = totalHeight - 150;

            GameObject grandPrize = new GameObject("GrandPrize");
            grandPrize.transform.SetParent(content, false);

            RectTransform prizeRT = grandPrize.AddComponent<RectTransform>();
            prizeRT.anchorMin = new Vector2(0.5f, 0);
            prizeRT.anchorMax = new Vector2(0.5f, 0);
            prizeRT.pivot = new Vector2(0.5f, 0.5f);
            prizeRT.sizeDelta = new Vector2(200, 180);
            prizeRT.anchoredPosition = new Vector2(0, yPos);

            // Glow background
            GameObject glow = new GameObject("Glow");
            glow.transform.SetParent(grandPrize.transform, false);

            RectTransform glowRT = glow.AddComponent<RectTransform>();
            glowRT.anchorMin = new Vector2(0.5f, 0.5f);
            glowRT.anchorMax = new Vector2(0.5f, 0.5f);
            glowRT.sizeDelta = new Vector2(250, 230);
            glowRT.anchoredPosition = Vector2.zero;

            Image glowImg = glow.AddComponent<Image>();
            glowImg.color = new Color(1f, 0.84f, 0f, 0.15f);

            // Card background
            GameObject card = new GameObject("Card");
            card.transform.SetParent(grandPrize.transform, false);

            RectTransform cardRT = card.AddComponent<RectTransform>();
            cardRT.anchorMin = Vector2.zero;
            cardRT.anchorMax = Vector2.one;
            cardRT.offsetMin = Vector2.zero;
            cardRT.offsetMax = Vector2.zero;

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = GOLD_DARK;

            Outline cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = GOLD;
            cardOutline.effectDistance = new Vector2(3, -3);

            // "GRAN PREMIO" banner
            GameObject banner = new GameObject("Banner");
            banner.transform.SetParent(card.transform, false);

            RectTransform bannerRT = banner.AddComponent<RectTransform>();
            bannerRT.anchorMin = new Vector2(0, 1);
            bannerRT.anchorMax = new Vector2(1, 1);
            bannerRT.pivot = new Vector2(0.5f, 1);
            bannerRT.sizeDelta = new Vector2(0, 35);
            bannerRT.anchoredPosition = Vector2.zero;

            Image bannerBg = banner.AddComponent<Image>();
            bannerBg.color = GOLD;

            GameObject bannerText = new GameObject("Text");
            bannerText.transform.SetParent(banner.transform, false);

            RectTransform bannerTextRT = bannerText.AddComponent<RectTransform>();
            bannerTextRT.anchorMin = Vector2.zero;
            bannerTextRT.anchorMax = Vector2.one;
            bannerTextRT.offsetMin = Vector2.zero;
            bannerTextRT.offsetMax = Vector2.zero;

            TextMeshProUGUI bannerTMP = bannerText.AddComponent<TextMeshProUGUI>();
            bannerTMP.text = "🏆 GRAN PREMIO 🏆";
            bannerTMP.fontSize = 16;
            bannerTMP.color = Color.black;
            bannerTMP.fontStyle = FontStyles.Bold;
            bannerTMP.alignment = TextAlignmentOptions.Center;

            // Prize icon
            GameObject prizeIcon = new GameObject("PrizeIcon");
            prizeIcon.transform.SetParent(card.transform, false);

            RectTransform prizeIconRT = prizeIcon.AddComponent<RectTransform>();
            prizeIconRT.anchorMin = new Vector2(0.5f, 0.5f);
            prizeIconRT.anchorMax = new Vector2(0.5f, 0.5f);
            prizeIconRT.sizeDelta = new Vector2(80, 80);
            prizeIconRT.anchoredPosition = new Vector2(0, -5);

            Image prizeIconImg = prizeIcon.AddComponent<Image>();
            prizeIconImg.preserveAspect = true;

            Sprite mythicChest = AssetDatabase.LoadAssetAtPath<Sprite>(CHEST_ICONS_PATH + "icon_chest_mythic_closed.png");
            if (mythicChest != null)
            {
                prizeIconImg.sprite = mythicChest;
                prizeIconImg.color = Color.white;
            }
            else
            {
                prizeIconImg.color = GOLD;
            }

            // Prize name
            GameObject prizeName = new GameObject("PrizeName");
            prizeName.transform.SetParent(card.transform, false);

            RectTransform prizeNameRT = prizeName.AddComponent<RectTransform>();
            prizeNameRT.anchorMin = new Vector2(0, 0);
            prizeNameRT.anchorMax = new Vector2(1, 0);
            prizeNameRT.pivot = new Vector2(0.5f, 0);
            prizeNameRT.sizeDelta = new Vector2(0, 30);
            prizeNameRT.anchoredPosition = new Vector2(0, 10);

            TextMeshProUGUI prizeNameTMP = prizeName.AddComponent<TextMeshProUGUI>();
            prizeNameTMP.text = "Cofre Mítico";
            prizeNameTMP.fontSize = 14;
            prizeNameTMP.color = TEXT_WHITE;
            prizeNameTMP.fontStyle = FontStyles.Bold;
            prizeNameTMP.alignment = TextAlignmentOptions.Center;

            // Level 50 badge
            GameObject lvlBadge = new GameObject("Level50Badge");
            lvlBadge.transform.SetParent(grandPrize.transform, false);

            RectTransform lvlRT = lvlBadge.AddComponent<RectTransform>();
            lvlRT.anchorMin = new Vector2(0.5f, 0);
            lvlRT.anchorMax = new Vector2(0.5f, 0);
            lvlRT.pivot = new Vector2(0.5f, 1);
            lvlRT.sizeDelta = new Vector2(50, 25);
            lvlRT.anchoredPosition = new Vector2(0, -5);

            Image lvlBg = lvlBadge.AddComponent<Image>();
            lvlBg.color = GOLD;

            GameObject lvlText = new GameObject("Text");
            lvlText.transform.SetParent(lvlBadge.transform, false);

            RectTransform lvlTextRT = lvlText.AddComponent<RectTransform>();
            lvlTextRT.anchorMin = Vector2.zero;
            lvlTextRT.anchorMax = Vector2.one;
            lvlTextRT.offsetMin = Vector2.zero;
            lvlTextRT.offsetMax = Vector2.zero;

            TextMeshProUGUI lvlTMP = lvlText.AddComponent<TextMeshProUGUI>();
            lvlTMP.text = "Nv.50";
            lvlTMP.fontSize = 12;
            lvlTMP.color = Color.black;
            lvlTMP.fontStyle = FontStyles.Bold;
            lvlTMP.alignment = TextAlignmentOptions.Center;
        }

        #endregion

        #region Premium Button

        private static void CreatePremiumButton(Transform parent)
        {
            GameObject premiumBar = new GameObject("PremiumBar");
            premiumBar.transform.SetParent(parent, false);

            RectTransform barRT = premiumBar.AddComponent<RectTransform>();
            barRT.anchorMin = new Vector2(0, 0);
            barRT.anchorMax = new Vector2(1, 0);
            barRT.pivot = new Vector2(0.5f, 0);
            barRT.sizeDelta = new Vector2(0, 65);
            barRT.anchoredPosition = Vector2.zero;

            Image barBg = premiumBar.AddComponent<Image>();
            barBg.color = new Color(0.05f, 0.03f, 0.1f, 0.98f);

            // Premium button
            GameObject premiumBtn = new GameObject("PremiumButton");
            premiumBtn.transform.SetParent(premiumBar.transform, false);

            RectTransform btnRT = premiumBtn.AddComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.5f, 0.5f);
            btnRT.anchorMax = new Vector2(0.5f, 0.5f);
            btnRT.sizeDelta = new Vector2(280, 50);
            btnRT.anchoredPosition = Vector2.zero;

            Image btnBg = premiumBtn.AddComponent<Image>();
            btnBg.color = PURPLE_DARK;

            Button btn = premiumBtn.AddComponent<Button>();
            btn.targetGraphic = btnBg;

            Outline btnOutline = premiumBtn.AddComponent<Outline>();
            btnOutline.effectColor = PURPLE;
            btnOutline.effectDistance = new Vector2(2, -2);

            // Button content
            HorizontalLayoutGroup hlg = premiumBtn.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(20, 20, 8, 8);
            hlg.spacing = 15;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childControlWidth = false;

            // Star icon
            GameObject star = new GameObject("StarIcon");
            star.transform.SetParent(premiumBtn.transform, false);

            LayoutElement starLE = star.AddComponent<LayoutElement>();
            starLE.preferredWidth = 30;
            starLE.preferredHeight = 30;

            TextMeshProUGUI starTMP = star.AddComponent<TextMeshProUGUI>();
            starTMP.text = "⭐";
            starTMP.fontSize = 24;
            starTMP.alignment = TextAlignmentOptions.Center;

            // Text
            GameObject text = new GameObject("Text");
            text.transform.SetParent(premiumBtn.transform, false);

            LayoutElement textLE = text.AddComponent<LayoutElement>();
            textLE.flexibleWidth = 1;

            TextMeshProUGUI textTMP = text.AddComponent<TextMeshProUGUI>();
            textTMP.text = "OBTENER PASE PREMIUM";
            textTMP.fontSize = 16;
            textTMP.color = TEXT_WHITE;
            textTMP.fontStyle = FontStyles.Bold;
            textTMP.alignment = TextAlignmentOptions.Center;

            // Price
            GameObject price = new GameObject("Price");
            price.transform.SetParent(premiumBtn.transform, false);

            LayoutElement priceLE = price.AddComponent<LayoutElement>();
            priceLE.preferredWidth = 60;

            TextMeshProUGUI priceTMP = price.AddComponent<TextMeshProUGUI>();
            priceTMP.text = "$9.99";
            priceTMP.fontSize = 16;
            priceTMP.color = GOLD;
            priceTMP.fontStyle = FontStyles.Bold;
            priceTMP.alignment = TextAlignmentOptions.Right;
        }

        #endregion
    }
}
