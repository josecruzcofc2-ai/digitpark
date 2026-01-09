using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Premium UI Builder for CashBattle scene
    /// Creates a VIP/Premium experience for real money competitions
    /// Features: Gold gradient background, card-based layout, prominent balance display
    /// </summary>
    public class CashBattleUIBuilder : EditorWindow
    {
        // Premium Color Palette
        private static readonly Color GOLD_PRIMARY = new Color(1f, 0.84f, 0f, 1f);           // #FFD700 Gold
        private static readonly Color GOLD_DARK = new Color(0.85f, 0.65f, 0.13f, 1f);        // #D4A520 Dark Gold
        private static readonly Color GOLD_LIGHT = new Color(1f, 0.93f, 0.55f, 1f);          // #FFEE8C Light Gold
        private static readonly Color AMBER = new Color(1f, 0.75f, 0f, 1f);                  // #FFBF00 Amber

        private static readonly Color BG_DARK = new Color(0.08f, 0.06f, 0.12f, 1f);          // Very dark purple-black
        private static readonly Color BG_GRADIENT_TOP = new Color(0.15f, 0.1f, 0.05f, 1f);   // Dark brown/gold tint
        private static readonly Color BG_GRADIENT_BOTTOM = new Color(0.05f, 0.03f, 0.08f, 1f); // Almost black

        private static readonly Color CARD_BG = new Color(0.12f, 0.1f, 0.15f, 0.95f);        // Dark card background
        private static readonly Color CARD_BORDER = new Color(0.85f, 0.65f, 0.13f, 0.6f);    // Gold border

        private static readonly Color TEXT_PRIMARY = new Color(1f, 1f, 1f, 1f);              // White
        private static readonly Color TEXT_GOLD = new Color(1f, 0.84f, 0f, 1f);              // Gold
        private static readonly Color TEXT_SECONDARY = new Color(0.7f, 0.7f, 0.7f, 1f);      // Gray

        private static readonly Color BUTTON_GOLD = new Color(0.85f, 0.65f, 0.13f, 1f);      // Gold button
        private static readonly Color BUTTON_DANGER = new Color(0.8f, 0.2f, 0.2f, 1f);       // Red for warnings

        private static readonly Color CYAN_ACCENT = new Color(0f, 0.9f, 1f, 1f);             // Keep some cyan for contrast

        [MenuItem("DigitPark/UI Builders/Build CashBattle Premium UI")]
        public static void ShowWindow()
        {
            GetWindow<CashBattleUIBuilder>("CashBattle UI Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("CashBattle Premium UI Builder", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This will create a premium UI for the CashBattle scene:\n\n" +
                "• Premium gold gradient background\n" +
                "• Card-based layout for options\n" +
                "• Prominent balance display\n" +
                "• Improved age verification panel\n" +
                "• VIP/Premium aesthetic",
                MessageType.Info);

            EditorGUILayout.Space(10);

            GUI.backgroundColor = GOLD_PRIMARY;
            if (GUILayout.Button("BUILD PREMIUM UI", GUILayout.Height(40)))
            {
                BuildPremiumUI();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Only Rebuild Background", GUILayout.Height(25)))
            {
                RebuildBackground();
            }
        }

        private static void BuildPremiumUI()
        {
            // Find or create Canvas
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error", "No Canvas found. Open CashBattle scene first.", "OK");
                return;
            }

            // Clean existing UI (optional - ask user)
            if (EditorUtility.DisplayDialog("Rebuild UI?",
                "This will rebuild the CashBattle UI.\n\nExisting UI elements will be replaced.\n\nContinue?",
                "Yes, Build", "Cancel"))
            {
                BuildAllElements(canvas);

                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

                EditorUtility.DisplayDialog("Success",
                    "CashBattle Premium UI created!\n\n" +
                    "Don't forget to:\n" +
                    "1. Assign references in CashBattleManager\n" +
                    "2. Save the scene",
                    "OK");
            }
        }

        private static void RebuildBackground()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            // Find and destroy old background
            Transform oldBg = canvas.transform.Find("Background");
            if (oldBg != null)
            {
                DestroyImmediate(oldBg.gameObject);
            }

            CreatePremiumBackground(canvas.transform);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private static void BuildAllElements(Canvas canvas)
        {
            Transform canvasTransform = canvas.transform;

            // Remove old elements
            CleanupOldElements(canvasTransform);

            // 1. Premium Background
            CreatePremiumBackground(canvasTransform);

            // 2. Main Container (SafeArea)
            GameObject safeArea = CreateSafeArea(canvasTransform);

            // 3. Header with Back button and Balance
            CreateHeader(safeArea.transform);

            // 4. Age Verification Panel (shown first if not verified)
            CreateAgeVerificationPanel(safeArea.transform);

            // 5. Main Panel with Cards
            CreateMainPanel(safeArea.transform);

            // 6. Game Selection Panel (for 1v1 battles)
            CreateGameSelectionPanel(safeArea.transform);

            // 7. Tournament List Panel
            CreateTournamentListPanel(safeArea.transform);

            // 8. Matchmaking Panel
            CreateMatchmakingPanel(safeArea.transform);

            // 9. Wallet Panel
            CreateWalletPanel(safeArea.transform);

            // 10. History Panel
            CreateHistoryPanel(safeArea.transform);

            Debug.Log("[CashBattleUIBuilder] Premium UI built successfully!");
        }

        private static void CleanupOldElements(Transform parent)
        {
            // Destroy specific known elements
            string[] toDestroy = {
                "Background", "SafeArea", "MainPanel", "AgeVerificationPanel", "Header",
                "GameSelectionPanel", "TournamentListPanel", "MatchmakingPanel",
                "WalletPanel", "HistoryPanel"
            };

            foreach (string name in toDestroy)
            {
                Transform existing = parent.Find(name);
                if (existing != null)
                {
                    DestroyImmediate(existing.gameObject);
                }
            }
        }

        #region Background

        private static void CreatePremiumBackground(Transform parent)
        {
            // Main background container
            GameObject bgContainer = new GameObject("Background");
            bgContainer.transform.SetParent(parent, false);
            bgContainer.transform.SetAsFirstSibling();

            RectTransform bgRT = bgContainer.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;

            // Base dark layer
            GameObject baseLayer = new GameObject("BaseLayer");
            baseLayer.transform.SetParent(bgContainer.transform, false);

            RectTransform baseRT = baseLayer.AddComponent<RectTransform>();
            baseRT.anchorMin = Vector2.zero;
            baseRT.anchorMax = Vector2.one;
            baseRT.sizeDelta = Vector2.zero;

            Image baseImg = baseLayer.AddComponent<Image>();
            baseImg.color = BG_DARK;

            // Gradient overlay (simulated with multiple layers)
            CreateGradientLayers(bgContainer.transform);

            // Gold particle/glow effects (subtle)
            CreateGoldAccents(bgContainer.transform);

            // Vignette effect
            CreateVignette(bgContainer.transform);
        }

        private static void CreateGradientLayers(Transform parent)
        {
            // Top gradient (gold tint)
            GameObject topGradient = new GameObject("TopGradient");
            topGradient.transform.SetParent(parent, false);

            RectTransform topRT = topGradient.AddComponent<RectTransform>();
            topRT.anchorMin = new Vector2(0, 0.5f);
            topRT.anchorMax = Vector2.one;
            topRT.sizeDelta = Vector2.zero;

            Image topImg = topGradient.AddComponent<Image>();
            topImg.color = new Color(0.2f, 0.15f, 0.05f, 0.3f); // Subtle gold tint at top

            // Bottom darker area
            GameObject bottomGradient = new GameObject("BottomGradient");
            bottomGradient.transform.SetParent(parent, false);

            RectTransform bottomRT = bottomGradient.AddComponent<RectTransform>();
            bottomRT.anchorMin = Vector2.zero;
            bottomRT.anchorMax = new Vector2(1, 0.3f);
            bottomRT.sizeDelta = Vector2.zero;

            Image bottomImg = bottomGradient.AddComponent<Image>();
            bottomImg.color = new Color(0.02f, 0.01f, 0.04f, 0.5f); // Darker at bottom
        }

        private static void CreateGoldAccents(Transform parent)
        {
            // Subtle gold glow at top
            GameObject goldGlow = new GameObject("GoldGlow");
            goldGlow.transform.SetParent(parent, false);

            RectTransform glowRT = goldGlow.AddComponent<RectTransform>();
            glowRT.anchorMin = new Vector2(0.2f, 0.7f);
            glowRT.anchorMax = new Vector2(0.8f, 1f);
            glowRT.sizeDelta = Vector2.zero;

            Image glowImg = goldGlow.AddComponent<Image>();
            glowImg.color = new Color(1f, 0.8f, 0.3f, 0.08f); // Very subtle gold glow

            // Corner accents
            CreateCornerAccent(parent, "TopLeftAccent", new Vector2(0, 1), new Vector2(0.3f, 1), new Vector2(0, 0.7f), new Vector2(0.3f, 0.7f));
            CreateCornerAccent(parent, "TopRightAccent", new Vector2(0.7f, 1), new Vector2(1, 1), new Vector2(0.7f, 0.7f), new Vector2(1, 0.7f));
        }

        private static void CreateCornerAccent(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchorMin2, Vector2 anchorMax2)
        {
            GameObject accent = new GameObject(name);
            accent.transform.SetParent(parent, false);

            RectTransform rt = accent.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(anchorMin.x, anchorMin2.y);
            rt.anchorMax = new Vector2(anchorMax.x, anchorMax.y);
            rt.sizeDelta = Vector2.zero;

            Image img = accent.AddComponent<Image>();
            img.color = new Color(0.85f, 0.65f, 0.13f, 0.03f); // Very subtle gold
        }

        private static void CreateVignette(Transform parent)
        {
            GameObject vignette = new GameObject("Vignette");
            vignette.transform.SetParent(parent, false);

            RectTransform rt = vignette.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image img = vignette.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.4f);

            // Note: For a real vignette, you'd use a radial gradient sprite
            // This is just a simple overlay
        }

        #endregion

        #region Safe Area

        private static GameObject CreateSafeArea(Transform parent)
        {
            GameObject safeArea = new GameObject("SafeArea");
            safeArea.transform.SetParent(parent, false);

            RectTransform rt = safeArea.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);

            // Add SafeAreaHandler if exists
            System.Type safeAreaType = System.Type.GetType("DigitPark.UI.SafeAreaHandler, Assembly-CSharp");
            if (safeAreaType != null)
            {
                safeArea.AddComponent(safeAreaType);
            }

            return safeArea;
        }

        #endregion

        #region Header

        private static void CreateHeader(Transform parent)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(parent, false);

            RectTransform headerRT = header.AddComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0, 1);
            headerRT.anchorMax = new Vector2(1, 1);
            headerRT.pivot = new Vector2(0.5f, 1);
            headerRT.sizeDelta = new Vector2(0, 120);
            headerRT.anchoredPosition = Vector2.zero;

            // Header background (subtle)
            Image headerBg = header.AddComponent<Image>();
            headerBg.color = new Color(0, 0, 0, 0.3f);

            // Back Button
            CreateBackButton(header.transform);

            // Title
            CreateHeaderTitle(header.transform);

            // Balance Widget
            CreateBalanceWidget(header.transform);
        }

        private static void CreateBackButton(Transform parent)
        {
            GameObject backBtn = new GameObject("BackButton");
            backBtn.transform.SetParent(parent, false);

            RectTransform rt = backBtn.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 0.5f);
            rt.sizeDelta = new Vector2(100, 80);
            rt.anchoredPosition = new Vector2(20, 0);

            Image img = backBtn.AddComponent<Image>();
            img.color = Color.clear; // Transparent

            Button btn = backBtn.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.clear;
            colors.highlightedColor = new Color(1, 1, 1, 0.1f);
            colors.pressedColor = new Color(1, 1, 1, 0.2f);
            btn.colors = colors;

            // Arrow text
            GameObject arrowObj = new GameObject("Arrow");
            arrowObj.transform.SetParent(backBtn.transform, false);

            RectTransform arrowRT = arrowObj.AddComponent<RectTransform>();
            arrowRT.anchorMin = Vector2.zero;
            arrowRT.anchorMax = Vector2.one;
            arrowRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI arrow = arrowObj.AddComponent<TextMeshProUGUI>();
            arrow.text = "<";
            arrow.fontSize = 48;
            arrow.color = TEXT_GOLD;
            arrow.alignment = TextAlignmentOptions.Center;
            arrow.fontStyle = FontStyles.Bold;
        }

        private static void CreateHeaderTitle(Transform parent)
        {
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(parent, false);

            RectTransform rt = titleObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(500, 80);
            rt.anchoredPosition = Vector2.zero;

            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.text = "Cash Battle";
            title.fontSize = 52; // Bigger
            title.color = TEXT_GOLD;
            title.alignment = TextAlignmentOptions.Center;
            title.fontStyle = FontStyles.Bold;

            // Gold outline effect
            title.outlineWidth = 0.2f;
            title.outlineColor = new Color(0.5f, 0.35f, 0f, 0.6f);
        }

        private static void CreateBalanceWidget(Transform parent)
        {
            GameObject balanceWidget = new GameObject("BalanceWidget");
            balanceWidget.transform.SetParent(parent, false);

            RectTransform rt = balanceWidget.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(1, 0.5f);
            rt.sizeDelta = new Vector2(180, 50);
            rt.anchoredPosition = new Vector2(-20, 0);

            // Background
            Image bg = balanceWidget.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.08f, 0.05f, 0.8f);

            // Gold border
            Outline outline = balanceWidget.AddComponent<Outline>();
            outline.effectColor = CARD_BORDER;
            outline.effectDistance = new Vector2(1, -1);

            // Coin icon (text emoji for now)
            GameObject coinIcon = new GameObject("CoinIcon");
            coinIcon.transform.SetParent(balanceWidget.transform, false);

            RectTransform coinRT = coinIcon.AddComponent<RectTransform>();
            coinRT.anchorMin = new Vector2(0, 0);
            coinRT.anchorMax = new Vector2(0, 1);
            coinRT.pivot = new Vector2(0, 0.5f);
            coinRT.sizeDelta = new Vector2(40, 0);
            coinRT.anchoredPosition = new Vector2(8, 0);

            TextMeshProUGUI coinText = coinIcon.AddComponent<TextMeshProUGUI>();
            coinText.text = "$";
            coinText.fontSize = 28;
            coinText.color = TEXT_GOLD;
            coinText.alignment = TextAlignmentOptions.Center;
            coinText.fontStyle = FontStyles.Bold;

            // Balance text
            GameObject balanceObj = new GameObject("BalanceText");
            balanceObj.transform.SetParent(balanceWidget.transform, false);

            RectTransform balanceRT = balanceObj.AddComponent<RectTransform>();
            balanceRT.anchorMin = new Vector2(0, 0);
            balanceRT.anchorMax = new Vector2(1, 1);
            balanceRT.sizeDelta = Vector2.zero;
            balanceRT.offsetMin = new Vector2(45, 0);
            balanceRT.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI balanceText = balanceObj.AddComponent<TextMeshProUGUI>();
            balanceText.text = "0.00";
            balanceText.fontSize = 26;
            balanceText.color = TEXT_PRIMARY;
            balanceText.alignment = TextAlignmentOptions.Left;
            balanceText.fontStyle = FontStyles.Bold;
        }

        #endregion

        #region Age Verification Panel

        private static void CreateAgeVerificationPanel(Transform parent)
        {
            GameObject panel = new GameObject("AgeVerificationPanel");
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, -120); // Below header

            // Semi-transparent overlay
            Image overlay = panel.AddComponent<Image>();
            overlay.color = new Color(0, 0, 0, 0.5f);

            // Content container - LARGER
            GameObject content = new GameObject("Content");
            content.transform.SetParent(panel.transform, false);

            RectTransform contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0.5f, 0.5f);
            contentRT.anchorMax = new Vector2(0.5f, 0.5f);
            contentRT.sizeDelta = new Vector2(700, 620);
            contentRT.anchoredPosition = new Vector2(0, 30);

            // Card background
            Image cardBg = content.AddComponent<Image>();
            cardBg.color = CARD_BG;

            Outline cardOutline = content.AddComponent<Outline>();
            cardOutline.effectColor = CARD_BORDER;
            cardOutline.effectDistance = new Vector2(2, -2);

            // Warning Icon
            CreateWarningIcon(content.transform);

            // Title
            CreateVerificationTitle(content.transform);

            // Description
            CreateVerificationDescription(content.transform);

            // Status text
            CreateVerificationStatus(content.transform);

            // Verify Button
            CreateVerifyButton(content.transform);
        }

        private static void CreateWarningIcon(Transform parent)
        {
            GameObject iconObj = new GameObject("WarningIcon");
            iconObj.transform.SetParent(parent, false);

            RectTransform rt = iconObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(120, 120); // Bigger icon
            rt.anchoredPosition = new Vector2(0, -30);

            // Triangle warning background
            Image bg = iconObj.AddComponent<Image>();
            bg.color = AMBER;

            // Exclamation mark
            GameObject exclamation = new GameObject("Exclamation");
            exclamation.transform.SetParent(iconObj.transform, false);

            RectTransform excRT = exclamation.AddComponent<RectTransform>();
            excRT.anchorMin = Vector2.zero;
            excRT.anchorMax = Vector2.one;
            excRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI excText = exclamation.AddComponent<TextMeshProUGUI>();
            excText.text = "!";
            excText.fontSize = 72; // Bigger
            excText.color = BG_DARK;
            excText.alignment = TextAlignmentOptions.Center;
            excText.fontStyle = FontStyles.Bold;
        }

        private static void CreateVerificationTitle(Transform parent)
        {
            GameObject titleObj = new GameObject("VerificationTitle");
            titleObj.transform.SetParent(parent, false);

            RectTransform rt = titleObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(650, 70);
            rt.anchoredPosition = new Vector2(0, -170);

            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.text = "Verificacion de Edad Requerida";
            title.fontSize = 40; // Bigger and bold
            title.color = AMBER;
            title.alignment = TextAlignmentOptions.Center;
            title.fontStyle = FontStyles.Bold;
        }

        private static void CreateVerificationDescription(Transform parent)
        {
            GameObject descObj = new GameObject("VerificationDescription");
            descObj.transform.SetParent(parent, false);

            RectTransform rt = descObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(620, 150);
            rt.anchoredPosition = new Vector2(0, -260);

            TextMeshProUGUI desc = descObj.AddComponent<TextMeshProUGUI>();
            desc.text = "Las competencias con dinero real requieren que seas mayor de 18 anos. Deberas verificar tu identidad para continuar.";
            desc.fontSize = 28; // Bigger
            desc.color = TEXT_SECONDARY;
            desc.alignment = TextAlignmentOptions.Center;
            desc.enableWordWrapping = true;
            desc.fontStyle = FontStyles.Bold; // Bold
        }

        private static void CreateVerificationStatus(Transform parent)
        {
            GameObject statusObj = new GameObject("VerificationStatusText");
            statusObj.transform.SetParent(parent, false);

            RectTransform rt = statusObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(600, 50);
            rt.anchoredPosition = new Vector2(0, -430);

            TextMeshProUGUI status = statusObj.AddComponent<TextMeshProUGUI>();
            status.text = "";
            status.fontSize = 26; // Bigger
            status.color = TEXT_GOLD;
            status.alignment = TextAlignmentOptions.Center;
            status.fontStyle = FontStyles.Bold;
        }

        private static void CreateVerifyButton(Transform parent)
        {
            GameObject btnObj = new GameObject("VerifyAgeButton");
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0);
            rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(450, 90); // Bigger button
            rt.anchoredPosition = new Vector2(0, 50);

            Image bg = btnObj.AddComponent<Image>();
            bg.color = BUTTON_GOLD;

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = BUTTON_GOLD;
            colors.highlightedColor = GOLD_LIGHT;
            colors.pressedColor = GOLD_DARK;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            btn.colors = colors;

            // Button glow
            Outline glow = btnObj.AddComponent<Outline>();
            glow.effectColor = new Color(1f, 0.8f, 0.3f, 0.5f);
            glow.effectDistance = new Vector2(3, -3);

            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.text = "Verificar mi Edad";
            btnText.fontSize = 36; // Bigger
            btnText.color = BG_DARK;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.fontStyle = FontStyles.Bold;
        }

        #endregion

        #region Main Panel

        private static void CreateMainPanel(Transform parent)
        {
            GameObject panel = new GameObject("MainPanel");
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = new Vector2(20, 20); // Less margin - bigger cards
            rt.offsetMax = new Vector2(-20, -130); // Below header

            // Main cards container
            GameObject cardsContainer = new GameObject("CardsContainer");
            cardsContainer.transform.SetParent(panel.transform, false);

            RectTransform cardsRT = cardsContainer.AddComponent<RectTransform>();
            cardsRT.anchorMin = Vector2.zero;
            cardsRT.anchorMax = Vector2.one;
            cardsRT.sizeDelta = Vector2.zero;

            // Create the option cards
            CreateBattles1v1Card(cardsContainer.transform);
            CreateTournamentsCashCard(cardsContainer.transform);
            CreateWalletCard(cardsContainer.transform);
            CreateHistoryCard(cardsContainer.transform);

            // Initially hidden (until age verified)
            panel.SetActive(false);
        }

        private static void CreateBattles1v1Card(Transform parent)
        {
            // Top left - bigger cards covering more space
            GameObject card = CreateOptionCard(parent, "Battles1v1Card",
                "Batallas 1v1",
                "Enfrenta a otros jugadores",
                "$1 - $100",
                new Vector2(0, 1),
                new Vector2(0.49f, 1),
                new Vector2(0, 0.52f),
                new Vector2(0.49f, 0.52f));

            AddCardIcon(card.transform, "⚔");
        }

        private static void CreateTournamentsCashCard(Transform parent)
        {
            // Top right
            GameObject card = CreateOptionCard(parent, "CashTournamentsCard",
                "Torneos Cash",
                "Compite por grandes premios",
                "$5 - $500",
                new Vector2(0.51f, 1),
                new Vector2(1, 1),
                new Vector2(0.51f, 0.52f),
                new Vector2(1, 0.52f));

            AddCardIcon(card.transform, "🏆");
        }

        private static void CreateWalletCard(Transform parent)
        {
            // Bottom left
            GameObject card = CreateOptionCard(parent, "WalletCard",
                "Mi Wallet",
                "Deposita y retira dinero",
                "Fondos",
                new Vector2(0, 0.48f),
                new Vector2(0.49f, 0.48f),
                new Vector2(0, 0),
                new Vector2(0.49f, 0));

            AddCardIcon(card.transform, "💳");
        }

        private static void CreateHistoryCard(Transform parent)
        {
            // Bottom right
            GameObject card = CreateOptionCard(parent, "HistoryCard",
                "Historial",
                "Tus batallas anteriores",
                "",
                new Vector2(0.51f, 0.48f),
                new Vector2(1, 0.48f),
                new Vector2(0.51f, 0),
                new Vector2(1, 0));

            AddCardIcon(card.transform, "📊");
        }

        private static GameObject CreateOptionCard(Transform parent, string name, string title, string subtitle, string detail,
            Vector2 anchorMinTop, Vector2 anchorMaxTop, Vector2 anchorMinBottom, Vector2 anchorMaxBottom)
        {
            GameObject card = new GameObject(name);
            card.transform.SetParent(parent, false);

            RectTransform rt = card.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(anchorMinTop.x, anchorMinBottom.y);
            rt.anchorMax = new Vector2(anchorMaxTop.x, anchorMaxTop.y);
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = new Vector2(5, 5);
            rt.offsetMax = new Vector2(-5, -5);

            // Card background
            Image bg = card.AddComponent<Image>();
            bg.color = CARD_BG;

            // Gold border
            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = CARD_BORDER;
            outline.effectDistance = new Vector2(2, -2);

            // Button
            Button btn = card.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.05f, 1f, 1f);
            colors.pressedColor = new Color(0.9f, 0.85f, 0.8f, 1f);
            btn.colors = colors;
            btn.targetGraphic = bg;

            // Title - BIGGER and BOLD
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(card.transform, false);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.5f);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.sizeDelta = Vector2.zero;
            titleRT.offsetMin = new Vector2(90, 10);
            titleRT.offsetMax = new Vector2(-15, -15);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = title;
            titleText.fontSize = 38; // Bigger
            titleText.color = TEXT_GOLD;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.fontStyle = FontStyles.Bold;

            // Subtitle - BIGGER and BOLD
            GameObject subtitleObj = new GameObject("Subtitle");
            subtitleObj.transform.SetParent(card.transform, false);

            RectTransform subRT = subtitleObj.AddComponent<RectTransform>();
            subRT.anchorMin = new Vector2(0, 0);
            subRT.anchorMax = new Vector2(1, 0.5f);
            subRT.sizeDelta = Vector2.zero;
            subRT.offsetMin = new Vector2(90, 15);
            subRT.offsetMax = new Vector2(-15, -5);

            TextMeshProUGUI subText = subtitleObj.AddComponent<TextMeshProUGUI>();
            subText.text = subtitle;
            subText.fontSize = 26; // Bigger
            subText.color = TEXT_SECONDARY;
            subText.alignment = TextAlignmentOptions.Left;
            subText.fontStyle = FontStyles.Bold; // Bold

            // Detail (price range) - BIGGER
            if (!string.IsNullOrEmpty(detail))
            {
                GameObject detailObj = new GameObject("Detail");
                detailObj.transform.SetParent(card.transform, false);

                RectTransform detailRT = detailObj.AddComponent<RectTransform>();
                detailRT.anchorMin = new Vector2(1, 0.5f);
                detailRT.anchorMax = new Vector2(1, 0.5f);
                detailRT.pivot = new Vector2(1, 0.5f);
                detailRT.sizeDelta = new Vector2(150, 40);
                detailRT.anchoredPosition = new Vector2(-15, 0);

                TextMeshProUGUI detailText = detailObj.AddComponent<TextMeshProUGUI>();
                detailText.text = detail;
                detailText.fontSize = 24; // Bigger
                detailText.color = CYAN_ACCENT;
                detailText.alignment = TextAlignmentOptions.Right;
                detailText.fontStyle = FontStyles.Bold; // Bold
            }

            return card;
        }

        private static void AddCardIcon(Transform cardTransform, string icon)
        {
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(cardTransform, false);

            RectTransform rt = iconObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 0.5f);
            rt.sizeDelta = new Vector2(70, 70); // Bigger icon
            rt.anchoredPosition = new Vector2(15, 0);

            // Icon background
            Image bg = iconObj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.15f, 0.1f, 0.8f);

            Outline outline = iconObj.AddComponent<Outline>();
            outline.effectColor = CARD_BORDER;
            outline.effectDistance = new Vector2(2, -2);

            // Icon text
            GameObject textObj = new GameObject("IconText");
            textObj.transform.SetParent(iconObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = icon;
            text.fontSize = 38; // Bigger
            text.color = TEXT_GOLD;
            text.alignment = TextAlignmentOptions.Center;
        }

        #endregion

        #region Game Selection Panel

        private static void CreateGameSelectionPanel(Transform parent)
        {
            GameObject panel = new GameObject("GameSelectionPanel");
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = new Vector2(20, 20);
            rt.offsetMax = new Vector2(-20, -130);

            // Add the GameSelectionPanel script
            System.Type panelType = System.Type.GetType("DigitPark.UI.CashBattle.GameSelectionPanel, Assembly-CSharp");
            if (panelType != null)
            {
                panel.AddComponent(panelType);
            }

            // Panel Title
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(panel.transform, false);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0.5f, 1);
            titleRT.sizeDelta = new Vector2(0, 60);
            titleRT.anchoredPosition = Vector2.zero;

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Selecciona un Juego";
            titleText.fontSize = 38;
            titleText.color = TEXT_GOLD;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;

            // Games Container (ScrollView area)
            GameObject gamesContainer = new GameObject("GamesContainer");
            gamesContainer.transform.SetParent(panel.transform, false);

            RectTransform gamesRT = gamesContainer.AddComponent<RectTransform>();
            gamesRT.anchorMin = new Vector2(0, 0.25f);
            gamesRT.anchorMax = new Vector2(1, 0.9f);
            gamesRT.sizeDelta = Vector2.zero;
            gamesRT.offsetMin = new Vector2(10, 10);
            gamesRT.offsetMax = new Vector2(-10, -10);

            // Grid Layout for game cards
            GridLayoutGroup gridLayout = gamesContainer.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(320, 140);
            gridLayout.spacing = new Vector2(15, 15);
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.UpperCenter;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 2;

            // Create game cards
            CreateGameCard(gamesContainer.transform, "DigitRush", "Digit Rush", "Toca 1-9 en orden", "🔢");
            CreateGameCard(gamesContainer.transform, "MemoryPairs", "Memory Pairs", "Encuentra los pares", "🃏");
            CreateGameCard(gamesContainer.transform, "QuickMath", "Quick Math", "Resuelve operaciones", "➕");
            CreateGameCard(gamesContainer.transform, "FlashTap", "Flash Tap", "Reflejos rapidos", "⚡");
            CreateGameCard(gamesContainer.transform, "OddOneOut", "Odd One Out", "Encuentra el diferente", "👁");

            // Entry Fee Selection
            CreateEntryFeeSection(panel.transform);

            // Find Opponent Button
            CreateFindOpponentButton(panel.transform);

            // Note: Back button removed - user will add their own prefab

            // Initially hidden
            panel.SetActive(false);
        }

        private static void CreateGameCard(Transform parent, string gameId, string gameName, string description, string icon)
        {
            GameObject card = new GameObject($"GameCard_{gameId}");
            card.transform.SetParent(parent, false);

            // Card background
            Image bg = card.AddComponent<Image>();
            bg.color = CARD_BG;

            // Gold border
            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = CARD_BORDER;
            outline.effectDistance = new Vector2(2, -2);

            // Button
            Button btn = card.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.05f, 1f);
            colors.pressedColor = new Color(0.9f, 0.85f, 0.8f);
            btn.colors = colors;
            btn.targetGraphic = bg;

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(card.transform, false);

            RectTransform iconRT = iconObj.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0, 0.5f);
            iconRT.anchorMax = new Vector2(0, 0.5f);
            iconRT.pivot = new Vector2(0, 0.5f);
            iconRT.sizeDelta = new Vector2(60, 60);
            iconRT.anchoredPosition = new Vector2(15, 0);

            Image iconBg = iconObj.AddComponent<Image>();
            iconBg.color = new Color(0.2f, 0.15f, 0.1f, 0.8f);

            GameObject iconText = new GameObject("IconText");
            iconText.transform.SetParent(iconObj.transform, false);

            RectTransform iconTextRT = iconText.AddComponent<RectTransform>();
            iconTextRT.anchorMin = Vector2.zero;
            iconTextRT.anchorMax = Vector2.one;
            iconTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI iconTMP = iconText.AddComponent<TextMeshProUGUI>();
            iconTMP.text = icon;
            iconTMP.fontSize = 32;
            iconTMP.color = TEXT_GOLD;
            iconTMP.alignment = TextAlignmentOptions.Center;

            // Name
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(card.transform, false);

            RectTransform nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0.5f);
            nameRT.anchorMax = new Vector2(1, 1);
            nameRT.sizeDelta = Vector2.zero;
            nameRT.offsetMin = new Vector2(85, 10);
            nameRT.offsetMax = new Vector2(-50, -10);

            TextMeshProUGUI nameTMP = nameObj.AddComponent<TextMeshProUGUI>();
            nameTMP.text = gameName;
            nameTMP.fontSize = 28;
            nameTMP.color = TEXT_GOLD;
            nameTMP.fontStyle = FontStyles.Bold;
            nameTMP.alignment = TextAlignmentOptions.Left;

            // Description
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(card.transform, false);

            RectTransform descRT = descObj.AddComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0, 0);
            descRT.anchorMax = new Vector2(1, 0.5f);
            descRT.sizeDelta = Vector2.zero;
            descRT.offsetMin = new Vector2(85, 10);
            descRT.offsetMax = new Vector2(-10, -5);

            TextMeshProUGUI descTMP = descObj.AddComponent<TextMeshProUGUI>();
            descTMP.text = description;
            descTMP.fontSize = 20;
            descTMP.color = TEXT_SECONDARY;
            descTMP.fontStyle = FontStyles.Bold;
            descTMP.alignment = TextAlignmentOptions.Left;

            // Checkmark (hidden by default)
            GameObject checkmark = new GameObject("Checkmark");
            checkmark.transform.SetParent(card.transform, false);

            RectTransform checkRT = checkmark.AddComponent<RectTransform>();
            checkRT.anchorMin = new Vector2(1, 1);
            checkRT.anchorMax = new Vector2(1, 1);
            checkRT.pivot = new Vector2(1, 1);
            checkRT.sizeDelta = new Vector2(40, 40);
            checkRT.anchoredPosition = new Vector2(-10, -10);

            Image checkBg = checkmark.AddComponent<Image>();
            checkBg.color = new Color(0.3f, 1f, 0.5f, 1f); // Green

            GameObject checkText = new GameObject("CheckText");
            checkText.transform.SetParent(checkmark.transform, false);

            RectTransform checkTextRT = checkText.AddComponent<RectTransform>();
            checkTextRT.anchorMin = Vector2.zero;
            checkTextRT.anchorMax = Vector2.one;
            checkTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI checkTMP = checkText.AddComponent<TextMeshProUGUI>();
            checkTMP.text = "✓";
            checkTMP.fontSize = 28;
            checkTMP.color = BG_DARK;
            checkTMP.alignment = TextAlignmentOptions.Center;
            checkTMP.fontStyle = FontStyles.Bold;

            checkmark.SetActive(false);
        }

        private static void CreateEntryFeeSection(Transform parent)
        {
            GameObject feeSection = new GameObject("EntryFeeSection");
            feeSection.transform.SetParent(parent, false);

            RectTransform rt = feeSection.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.1f);
            rt.anchorMax = new Vector2(1, 0.22f);
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = new Vector2(10, 0);
            rt.offsetMax = new Vector2(-10, 0);

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(feeSection.transform, false);

            RectTransform labelRT = labelObj.AddComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0);
            labelRT.anchorMax = new Vector2(0.2f, 1);
            labelRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
            label.text = "Entrada:";
            label.fontSize = 28;
            label.color = TEXT_PRIMARY;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Left;

            // Entry fee buttons container
            GameObject buttonsContainer = new GameObject("EntryFeeContainer");
            buttonsContainer.transform.SetParent(feeSection.transform, false);

            RectTransform buttonsRT = buttonsContainer.AddComponent<RectTransform>();
            buttonsRT.anchorMin = new Vector2(0.22f, 0);
            buttonsRT.anchorMax = new Vector2(1, 1);
            buttonsRT.sizeDelta = Vector2.zero;

            HorizontalLayoutGroup layout = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            // Create fee buttons
            decimal[] fees = { 1m, 5m, 10m, 25m, 100m };
            foreach (var fee in fees)
            {
                CreateEntryFeeButton(buttonsContainer.transform, fee);
            }
        }

        private static void CreateEntryFeeButton(Transform parent, decimal fee)
        {
            GameObject btnObj = new GameObject($"Fee_{fee}");
            btnObj.transform.SetParent(parent, false);

            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.preferredWidth = 90;
            le.preferredHeight = 50;

            Image bg = btnObj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.18f, 0.25f, 1f);

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = new Color(0.2f, 0.18f, 0.25f, 1f);
            colors.highlightedColor = new Color(0.85f, 0.65f, 0.13f, 0.5f);
            colors.pressedColor = GOLD_PRIMARY;
            colors.selectedColor = GOLD_PRIMARY;
            btn.colors = colors;

            Outline outline = btnObj.AddComponent<Outline>();
            outline.effectColor = CARD_BORDER;
            outline.effectDistance = new Vector2(1, -1);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = $"${fee}";
            text.fontSize = 24;
            text.color = TEXT_PRIMARY;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateFindOpponentButton(Transform parent)
        {
            GameObject btnObj = new GameObject("FindOpponentButton");
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.15f, 0);
            rt.anchorMax = new Vector2(0.85f, 0.08f);
            rt.sizeDelta = Vector2.zero;

            Image bg = btnObj.AddComponent<Image>();
            bg.color = BUTTON_GOLD;

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = BUTTON_GOLD;
            colors.highlightedColor = GOLD_LIGHT;
            colors.pressedColor = GOLD_DARK;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            btn.colors = colors;

            Outline glow = btnObj.AddComponent<Outline>();
            glow.effectColor = new Color(1f, 0.8f, 0.3f, 0.5f);
            glow.effectDistance = new Vector2(3, -3);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "Buscar Oponente";
            text.fontSize = 32;
            text.color = BG_DARK;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
        }

        // Note: CreatePanelBackButton removed - user will add their own prefab

        #endregion

        #region Tournament List Panel

        private static void CreateTournamentListPanel(Transform parent)
        {
            GameObject panel = new GameObject("TournamentListPanel");
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = new Vector2(20, 20);
            rt.offsetMax = new Vector2(-20, -130);

            // Add the TournamentListPanel script
            System.Type panelType = System.Type.GetType("DigitPark.UI.CashBattle.TournamentListPanel, Assembly-CSharp");
            if (panelType != null)
            {
                panel.AddComponent(panelType);
            }

            // Panel Title
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(panel.transform, false);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0.5f, 1);
            titleRT.sizeDelta = new Vector2(0, 60);
            titleRT.anchoredPosition = Vector2.zero;

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Torneos Disponibles";
            titleText.fontSize = 38;
            titleText.color = TEXT_GOLD;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;

            // Tournaments Container (ScrollView)
            GameObject scrollView = CreateScrollView(panel.transform, "TournamentsScrollView",
                new Vector2(0, 0.05f), new Vector2(1, 0.9f));

            // Create sample tournament cards
            Transform content = scrollView.transform.Find("Viewport/Content");
            if (content != null)
            {
                CreateTournamentCard(content, "Quick Math Championship", "QuickMath", 5m, 100m, "12/16");
                CreateTournamentCard(content, "Flash Tap Masters", "FlashTap", 10m, 250m, "28/32");
                CreateTournamentCard(content, "Cognitive Sprint Elite", "Sprint", 25m, 500m, "8/16");
                CreateTournamentCard(content, "Memory Pairs Daily", "MemoryPairs", 1m, 20m, "18/20");
            }

            // Note: Back button removed - user will add their own prefab

            // Initially hidden
            panel.SetActive(false);
        }

        private static GameObject CreateScrollView(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject scrollView = new GameObject(name);
            scrollView.transform.SetParent(parent, false);

            RectTransform scrollRT = scrollView.AddComponent<RectTransform>();
            scrollRT.anchorMin = anchorMin;
            scrollRT.anchorMax = anchorMax;
            scrollRT.sizeDelta = Vector2.zero;
            scrollRT.offsetMin = new Vector2(10, 10);
            scrollRT.offsetMax = new Vector2(-10, -10);

            ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);

            RectTransform viewportRT = viewport.AddComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.sizeDelta = Vector2.zero;

            Image viewportMask = viewport.AddComponent<Image>();
            viewportMask.color = new Color(1, 1, 1, 0.01f);

            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);

            RectTransform contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.sizeDelta = new Vector2(0, 0);

            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 15;
            layout.padding = new RectOffset(0, 0, 10, 10);
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = false;

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewportRT;
            scrollRect.content = contentRT;

            return scrollView;
        }

        private static void CreateTournamentCard(Transform parent, string tournamentName, string gameType, decimal entryFee, decimal prizePool, string participants)
        {
            GameObject card = new GameObject($"Tournament_{tournamentName.Replace(" ", "_")}");
            card.transform.SetParent(parent, false);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredHeight = 160;

            Image bg = card.AddComponent<Image>();
            bg.color = CARD_BG;

            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = CARD_BORDER;
            outline.effectDistance = new Vector2(2, -2);

            // Info container
            GameObject info = new GameObject("Info");
            info.transform.SetParent(card.transform, false);

            RectTransform infoRT = info.AddComponent<RectTransform>();
            infoRT.anchorMin = new Vector2(0, 0);
            infoRT.anchorMax = new Vector2(0.7f, 1);
            infoRT.sizeDelta = Vector2.zero;
            infoRT.offsetMin = new Vector2(20, 15);
            infoRT.offsetMax = new Vector2(0, -15);

            // Name
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(info.transform, false);

            RectTransform nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0.7f);
            nameRT.anchorMax = new Vector2(1, 1);
            nameRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI nameTMP = nameObj.AddComponent<TextMeshProUGUI>();
            nameTMP.text = tournamentName;
            nameTMP.fontSize = 28;
            nameTMP.color = TEXT_GOLD;
            nameTMP.fontStyle = FontStyles.Bold;

            // Game type
            GameObject gameObj = new GameObject("GameType");
            gameObj.transform.SetParent(info.transform, false);

            RectTransform gameRT = gameObj.AddComponent<RectTransform>();
            gameRT.anchorMin = new Vector2(0, 0.45f);
            gameRT.anchorMax = new Vector2(1, 0.7f);
            gameRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI gameTMP = gameObj.AddComponent<TextMeshProUGUI>();
            gameTMP.text = gameType;
            gameTMP.fontSize = 22;
            gameTMP.color = CYAN_ACCENT;
            gameTMP.fontStyle = FontStyles.Bold;

            // Prize
            GameObject prizeObj = new GameObject("PrizePool");
            prizeObj.transform.SetParent(info.transform, false);

            RectTransform prizeRT = prizeObj.AddComponent<RectTransform>();
            prizeRT.anchorMin = new Vector2(0, 0.2f);
            prizeRT.anchorMax = new Vector2(0.5f, 0.45f);
            prizeRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI prizeTMP = prizeObj.AddComponent<TextMeshProUGUI>();
            prizeTMP.text = $"Premio: ${prizePool}";
            prizeTMP.fontSize = 22;
            prizeTMP.color = new Color(0.3f, 1f, 0.5f);
            prizeTMP.fontStyle = FontStyles.Bold;

            // Entry
            GameObject entryObj = new GameObject("EntryFee");
            entryObj.transform.SetParent(info.transform, false);

            RectTransform entryRT = entryObj.AddComponent<RectTransform>();
            entryRT.anchorMin = new Vector2(0.5f, 0.2f);
            entryRT.anchorMax = new Vector2(1, 0.45f);
            entryRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI entryTMP = entryObj.AddComponent<TextMeshProUGUI>();
            entryTMP.text = $"Entrada: ${entryFee}";
            entryTMP.fontSize = 22;
            entryTMP.color = TEXT_PRIMARY;
            entryTMP.fontStyle = FontStyles.Bold;

            // Participants
            GameObject partObj = new GameObject("Participants");
            partObj.transform.SetParent(info.transform, false);

            RectTransform partRT = partObj.AddComponent<RectTransform>();
            partRT.anchorMin = new Vector2(0, 0);
            partRT.anchorMax = new Vector2(1, 0.2f);
            partRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI partTMP = partObj.AddComponent<TextMeshProUGUI>();
            partTMP.text = $"{participants} jugadores";
            partTMP.fontSize = 20;
            partTMP.color = TEXT_SECONDARY;
            partTMP.fontStyle = FontStyles.Bold;

            // Join button
            GameObject joinBtn = new GameObject("JoinButton");
            joinBtn.transform.SetParent(card.transform, false);

            RectTransform joinRT = joinBtn.AddComponent<RectTransform>();
            joinRT.anchorMin = new Vector2(0.72f, 0.2f);
            joinRT.anchorMax = new Vector2(0.98f, 0.8f);
            joinRT.sizeDelta = Vector2.zero;

            Image joinBg = joinBtn.AddComponent<Image>();
            joinBg.color = BUTTON_GOLD;

            Button btn = joinBtn.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = BUTTON_GOLD;
            colors.highlightedColor = GOLD_LIGHT;
            colors.pressedColor = GOLD_DARK;
            btn.colors = colors;

            GameObject joinText = new GameObject("Text");
            joinText.transform.SetParent(joinBtn.transform, false);

            RectTransform joinTextRT = joinText.AddComponent<RectTransform>();
            joinTextRT.anchorMin = Vector2.zero;
            joinTextRT.anchorMax = Vector2.one;
            joinTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI joinTMP = joinText.AddComponent<TextMeshProUGUI>();
            joinTMP.text = "Unirse";
            joinTMP.fontSize = 26;
            joinTMP.color = BG_DARK;
            joinTMP.fontStyle = FontStyles.Bold;
            joinTMP.alignment = TextAlignmentOptions.Center;
        }

        #endregion

        #region Matchmaking Panel

        private static void CreateMatchmakingPanel(Transform parent)
        {
            GameObject panel = new GameObject("MatchmakingPanel");
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, -120);

            // Semi-transparent overlay
            Image overlay = panel.AddComponent<Image>();
            overlay.color = new Color(0, 0, 0, 0.7f);

            // Content container
            GameObject content = new GameObject("Content");
            content.transform.SetParent(panel.transform, false);

            RectTransform contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0.5f, 0.5f);
            contentRT.anchorMax = new Vector2(0.5f, 0.5f);
            contentRT.sizeDelta = new Vector2(500, 400);

            Image contentBg = content.AddComponent<Image>();
            contentBg.color = CARD_BG;

            Outline contentOutline = content.AddComponent<Outline>();
            contentOutline.effectColor = GOLD_PRIMARY;
            contentOutline.effectDistance = new Vector2(3, -3);

            // Searching animation (simple rotating icon)
            GameObject searchIcon = new GameObject("SearchIcon");
            searchIcon.transform.SetParent(content.transform, false);

            RectTransform iconRT = searchIcon.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.6f);
            iconRT.anchorMax = new Vector2(0.5f, 0.6f);
            iconRT.sizeDelta = new Vector2(120, 120);

            Image iconBg = searchIcon.AddComponent<Image>();
            iconBg.color = AMBER;

            Outline iconOutline = searchIcon.AddComponent<Outline>();
            iconOutline.effectColor = new Color(1f, 0.8f, 0.3f, 0.8f);
            iconOutline.effectDistance = new Vector2(4, -4);

            GameObject iconText = new GameObject("Text");
            iconText.transform.SetParent(searchIcon.transform, false);

            RectTransform iconTextRT = iconText.AddComponent<RectTransform>();
            iconTextRT.anchorMin = Vector2.zero;
            iconTextRT.anchorMax = Vector2.one;
            iconTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI iconTMP = iconText.AddComponent<TextMeshProUGUI>();
            iconTMP.text = "⚔";
            iconTMP.fontSize = 60;
            iconTMP.color = BG_DARK;
            iconTMP.alignment = TextAlignmentOptions.Center;

            // Status text
            GameObject statusObj = new GameObject("MatchmakingStatusText");
            statusObj.transform.SetParent(content.transform, false);

            RectTransform statusRT = statusObj.AddComponent<RectTransform>();
            statusRT.anchorMin = new Vector2(0, 0.3f);
            statusRT.anchorMax = new Vector2(1, 0.5f);
            statusRT.sizeDelta = Vector2.zero;
            statusRT.offsetMin = new Vector2(20, 0);
            statusRT.offsetMax = new Vector2(-20, 0);

            TextMeshProUGUI statusTMP = statusObj.AddComponent<TextMeshProUGUI>();
            statusTMP.text = "Buscando oponente...";
            statusTMP.fontSize = 36;
            statusTMP.color = TEXT_GOLD;
            statusTMP.fontStyle = FontStyles.Bold;
            statusTMP.alignment = TextAlignmentOptions.Center;

            // Cancel button
            GameObject cancelBtn = new GameObject("CancelMatchmakingButton");
            cancelBtn.transform.SetParent(content.transform, false);

            RectTransform cancelRT = cancelBtn.AddComponent<RectTransform>();
            cancelRT.anchorMin = new Vector2(0.2f, 0.08f);
            cancelRT.anchorMax = new Vector2(0.8f, 0.22f);
            cancelRT.sizeDelta = Vector2.zero;

            Image cancelBg = cancelBtn.AddComponent<Image>();
            cancelBg.color = BUTTON_DANGER;

            Button btn = cancelBtn.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = BUTTON_DANGER;
            colors.highlightedColor = new Color(1f, 0.4f, 0.4f);
            colors.pressedColor = new Color(0.6f, 0.15f, 0.15f);
            btn.colors = colors;

            GameObject cancelText = new GameObject("Text");
            cancelText.transform.SetParent(cancelBtn.transform, false);

            RectTransform cancelTextRT = cancelText.AddComponent<RectTransform>();
            cancelTextRT.anchorMin = Vector2.zero;
            cancelTextRT.anchorMax = Vector2.one;
            cancelTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI cancelTMP = cancelText.AddComponent<TextMeshProUGUI>();
            cancelTMP.text = "Cancelar";
            cancelTMP.fontSize = 30;
            cancelTMP.color = TEXT_PRIMARY;
            cancelTMP.fontStyle = FontStyles.Bold;
            cancelTMP.alignment = TextAlignmentOptions.Center;

            // Initially hidden
            panel.SetActive(false);
        }

        #endregion

        #region Wallet Panel

        private static void CreateWalletPanel(Transform parent)
        {
            GameObject panel = new GameObject("WalletPanel");
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = new Vector2(20, 20);
            rt.offsetMax = new Vector2(-20, -130);

            // Panel Title
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(panel.transform, false);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0.5f, 1);
            titleRT.sizeDelta = new Vector2(0, 60);
            titleRT.anchoredPosition = Vector2.zero;

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Mi Wallet";
            titleText.fontSize = 42;
            titleText.color = TEXT_GOLD;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;

            // Balance Card
            CreateBalanceCard(panel.transform);

            // Action Buttons (Deposit/Withdraw)
            CreateWalletActions(panel.transform);

            // Recent Transactions
            CreateRecentTransactions(panel.transform);

            // Note: Back button removed - user will add their own prefab

            // Initially hidden
            panel.SetActive(false);
        }

        private static void CreateBalanceCard(Transform parent)
        {
            GameObject card = new GameObject("BalanceCard");
            card.transform.SetParent(parent, false);

            RectTransform rt = card.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.1f, 0.72f);
            rt.anchorMax = new Vector2(0.9f, 0.92f);
            rt.sizeDelta = Vector2.zero;

            Image bg = card.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.12f, 0.08f, 0.95f);

            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = GOLD_PRIMARY;
            outline.effectDistance = new Vector2(3, -3);

            // Balance Amount
            GameObject amountObj = new GameObject("BalanceAmount");
            amountObj.transform.SetParent(card.transform, false);

            RectTransform amountRT = amountObj.AddComponent<RectTransform>();
            amountRT.anchorMin = new Vector2(0, 0.4f);
            amountRT.anchorMax = new Vector2(1, 0.95f);
            amountRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI amountText = amountObj.AddComponent<TextMeshProUGUI>();
            amountText.text = "$0.00";
            amountText.fontSize = 72;
            amountText.color = GOLD_PRIMARY;
            amountText.alignment = TextAlignmentOptions.Center;
            amountText.fontStyle = FontStyles.Bold;

            // Balance Label
            GameObject labelObj = new GameObject("BalanceLabel");
            labelObj.transform.SetParent(card.transform, false);

            RectTransform labelRT = labelObj.AddComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0.05f);
            labelRT.anchorMax = new Vector2(1, 0.4f);
            labelRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = "Balance Disponible";
            labelText.fontSize = 28;
            labelText.color = TEXT_SECONDARY;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.fontStyle = FontStyles.Bold;
        }

        private static void CreateWalletActions(Transform parent)
        {
            GameObject actionsContainer = new GameObject("ActionsContainer");
            actionsContainer.transform.SetParent(parent, false);

            RectTransform rt = actionsContainer.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.05f, 0.55f);
            rt.anchorMax = new Vector2(0.95f, 0.7f);
            rt.sizeDelta = Vector2.zero;

            HorizontalLayoutGroup layout = actionsContainer.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 30;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            // Deposit Button
            CreateWalletActionButton(actionsContainer.transform, "DepositButton", "DEPOSITAR",
                new Color(0.2f, 0.7f, 0.3f, 1f), new Color(0.3f, 0.85f, 0.4f, 1f));

            // Withdraw Button
            CreateWalletActionButton(actionsContainer.transform, "WithdrawButton", "RETIRAR",
                BUTTON_GOLD, GOLD_LIGHT);
        }

        private static void CreateWalletActionButton(Transform parent, string name, string text, Color normalColor, Color highlightColor)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;

            Image bg = btnObj.AddComponent<Image>();
            bg.color = normalColor;

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = highlightColor;
            colors.pressedColor = new Color(normalColor.r * 0.7f, normalColor.g * 0.7f, normalColor.b * 0.7f, 1f);
            btn.colors = colors;

            Outline glow = btnObj.AddComponent<Outline>();
            glow.effectColor = new Color(normalColor.r, normalColor.g, normalColor.b, 0.5f);
            glow.effectDistance = new Vector2(3, -3);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.text = text;
            btnText.fontSize = 32;
            btnText.color = Color.white;
            btnText.fontStyle = FontStyles.Bold;
            btnText.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateRecentTransactions(Transform parent)
        {
            // Section Header
            GameObject headerObj = new GameObject("TransactionsHeader");
            headerObj.transform.SetParent(parent, false);

            RectTransform headerRT = headerObj.AddComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0.05f, 0.48f);
            headerRT.anchorMax = new Vector2(0.95f, 0.54f);
            headerRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI headerText = headerObj.AddComponent<TextMeshProUGUI>();
            headerText.text = "Transacciones Recientes";
            headerText.fontSize = 28;
            headerText.color = TEXT_PRIMARY;
            headerText.fontStyle = FontStyles.Bold;
            headerText.alignment = TextAlignmentOptions.Left;

            // Transactions ScrollView
            GameObject scrollView = CreateScrollView(parent, "TransactionsScrollView",
                new Vector2(0.02f, 0.08f), new Vector2(0.98f, 0.47f));

            // Sample transactions
            Transform content = scrollView.transform.Find("Viewport/Content");
            if (content != null)
            {
                CreateTransactionItem(content, "+$10.00", "Ganancia - QuickMath vs @Player123", "Hace 2 horas", true);
                CreateTransactionItem(content, "-$5.00", "Entrada - Torneo Flash Tap", "Hace 5 horas", false);
                CreateTransactionItem(content, "+$50.00", "Deposito", "Ayer", true);
                CreateTransactionItem(content, "+$25.00", "Premio - 2do lugar Torneo", "Ayer", true);
                CreateTransactionItem(content, "-$10.00", "Entrada - Battle 1v1", "Hace 2 dias", false);
            }
        }

        private static void CreateTransactionItem(Transform parent, string amount, string description, string time, bool isPositive)
        {
            GameObject item = new GameObject($"Transaction_{description.GetHashCode()}");
            item.transform.SetParent(parent, false);

            LayoutElement le = item.AddComponent<LayoutElement>();
            le.preferredHeight = 80;

            Image bg = item.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.08f, 0.12f, 0.8f);

            // Amount
            GameObject amountObj = new GameObject("Amount");
            amountObj.transform.SetParent(item.transform, false);

            RectTransform amountRT = amountObj.AddComponent<RectTransform>();
            amountRT.anchorMin = new Vector2(0, 0);
            amountRT.anchorMax = new Vector2(0.25f, 1);
            amountRT.sizeDelta = Vector2.zero;
            amountRT.offsetMin = new Vector2(15, 10);
            amountRT.offsetMax = new Vector2(0, -10);

            TextMeshProUGUI amountText = amountObj.AddComponent<TextMeshProUGUI>();
            amountText.text = amount;
            amountText.fontSize = 28;
            amountText.color = isPositive ? new Color(0.3f, 1f, 0.5f) : new Color(1f, 0.4f, 0.4f);
            amountText.fontStyle = FontStyles.Bold;
            amountText.alignment = TextAlignmentOptions.Left;

            // Description
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(item.transform, false);

            RectTransform descRT = descObj.AddComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0.26f, 0.4f);
            descRT.anchorMax = new Vector2(0.85f, 1);
            descRT.sizeDelta = Vector2.zero;
            descRT.offsetMin = new Vector2(5, 0);
            descRT.offsetMax = new Vector2(-5, -5);

            TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = description;
            descText.fontSize = 20;
            descText.color = TEXT_PRIMARY;
            descText.fontStyle = FontStyles.Bold;
            descText.alignment = TextAlignmentOptions.Left;
            descText.enableWordWrapping = true;
            descText.overflowMode = TextOverflowModes.Ellipsis;

            // Time
            GameObject timeObj = new GameObject("Time");
            timeObj.transform.SetParent(item.transform, false);

            RectTransform timeRT = timeObj.AddComponent<RectTransform>();
            timeRT.anchorMin = new Vector2(0.26f, 0);
            timeRT.anchorMax = new Vector2(0.85f, 0.4f);
            timeRT.sizeDelta = Vector2.zero;
            timeRT.offsetMin = new Vector2(5, 5);
            timeRT.offsetMax = new Vector2(-5, 0);

            TextMeshProUGUI timeText = timeObj.AddComponent<TextMeshProUGUI>();
            timeText.text = time;
            timeText.fontSize = 18;
            timeText.color = TEXT_SECONDARY;
            timeText.alignment = TextAlignmentOptions.Left;

            // Status indicator
            GameObject indicator = new GameObject("Indicator");
            indicator.transform.SetParent(item.transform, false);

            RectTransform indicatorRT = indicator.AddComponent<RectTransform>();
            indicatorRT.anchorMin = new Vector2(0.9f, 0.3f);
            indicatorRT.anchorMax = new Vector2(0.95f, 0.7f);
            indicatorRT.sizeDelta = Vector2.zero;

            Image indicatorImg = indicator.AddComponent<Image>();
            indicatorImg.color = isPositive ? new Color(0.3f, 1f, 0.5f) : new Color(1f, 0.4f, 0.4f);
        }

        #endregion

        #region History Panel

        private static void CreateHistoryPanel(Transform parent)
        {
            GameObject panel = new GameObject("HistoryPanel");
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = new Vector2(20, 20);
            rt.offsetMax = new Vector2(-20, -130);

            // Panel Title
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(panel.transform, false);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0.5f, 1);
            titleRT.sizeDelta = new Vector2(0, 60);
            titleRT.anchoredPosition = Vector2.zero;

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Historial de Partidas";
            titleText.fontSize = 42;
            titleText.color = TEXT_GOLD;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;

            // Stats Summary
            CreateHistoryStats(panel.transform);

            // Match History ScrollView
            GameObject scrollView = CreateScrollView(panel.transform, "HistoryScrollView",
                new Vector2(0, 0.05f), new Vector2(1, 0.75f));

            // Sample match history
            Transform content = scrollView.transform.Find("Viewport/Content");
            if (content != null)
            {
                CreateMatchHistoryItem(content, "QuickMath", "@ProGamer99", true, "+$8.50", "Hoy, 14:32", "1250 vs 980");
                CreateMatchHistoryItem(content, "Torneo Flash Tap", "3er Lugar", true, "+$15.00", "Hoy, 12:15", "8 participantes");
                CreateMatchHistoryItem(content, "MemoryPairs", "@SpeedKing", false, "-$5.00", "Ayer, 22:45", "45s vs 38s");
                CreateMatchHistoryItem(content, "Cognitive Sprint", "@MindMaster", true, "+$20.00", "Ayer, 18:20", "3-2 juegos");
                CreateMatchHistoryItem(content, "OddOneOut", "@EagleEye", false, "-$10.00", "Hace 2 dias", "8/10 vs 10/10");
                CreateMatchHistoryItem(content, "FlashTap", "@Lightning", true, "+$4.50", "Hace 2 dias", "0.21s vs 0.28s");
                CreateMatchHistoryItem(content, "Torneo QuickMath", "1er Lugar", true, "+$50.00", "Hace 3 dias", "16 participantes");
            }

            // Note: Back button removed - user will add their own prefab

            // Initially hidden
            panel.SetActive(false);
        }

        private static void CreateHistoryStats(Transform parent)
        {
            GameObject statsContainer = new GameObject("StatsContainer");
            statsContainer.transform.SetParent(parent, false);

            RectTransform rt = statsContainer.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.78f);
            rt.anchorMax = new Vector2(1, 0.95f);
            rt.sizeDelta = Vector2.zero;

            HorizontalLayoutGroup layout = statsContainer.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 15;
            layout.padding = new RectOffset(10, 10, 5, 5);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            // Wins
            CreateStatCard(statsContainer.transform, "Victorias", "24", new Color(0.3f, 1f, 0.5f));

            // Losses
            CreateStatCard(statsContainer.transform, "Derrotas", "12", new Color(1f, 0.4f, 0.4f));

            // Win Rate
            CreateStatCard(statsContainer.transform, "Win Rate", "67%", GOLD_PRIMARY);

            // Total Earned
            CreateStatCard(statsContainer.transform, "Ganado", "$156.50", CYAN_ACCENT);
        }

        private static void CreateStatCard(Transform parent, string label, string value, Color valueColor)
        {
            GameObject card = new GameObject($"Stat_{label}");
            card.transform.SetParent(parent, false);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;

            Image bg = card.AddComponent<Image>();
            bg.color = CARD_BG;

            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = CARD_BORDER;
            outline.effectDistance = new Vector2(1, -1);

            // Value
            GameObject valueObj = new GameObject("Value");
            valueObj.transform.SetParent(card.transform, false);

            RectTransform valueRT = valueObj.AddComponent<RectTransform>();
            valueRT.anchorMin = new Vector2(0, 0.4f);
            valueRT.anchorMax = new Vector2(1, 1);
            valueRT.sizeDelta = Vector2.zero;
            valueRT.offsetMin = new Vector2(5, 0);
            valueRT.offsetMax = new Vector2(-5, -5);

            TextMeshProUGUI valueText = valueObj.AddComponent<TextMeshProUGUI>();
            valueText.text = value;
            valueText.fontSize = 32;
            valueText.color = valueColor;
            valueText.fontStyle = FontStyles.Bold;
            valueText.alignment = TextAlignmentOptions.Center;

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(card.transform, false);

            RectTransform labelRT = labelObj.AddComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0);
            labelRT.anchorMax = new Vector2(1, 0.4f);
            labelRT.sizeDelta = Vector2.zero;
            labelRT.offsetMin = new Vector2(5, 5);
            labelRT.offsetMax = new Vector2(-5, 0);

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = 16;
            labelText.color = TEXT_SECONDARY;
            labelText.fontStyle = FontStyles.Bold;
            labelText.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateMatchHistoryItem(Transform parent, string gameType, string opponent, bool isWin, string amount, string dateTime, string score)
        {
            GameObject item = new GameObject($"Match_{gameType}_{opponent}".Replace(" ", "_").Replace("@", ""));
            item.transform.SetParent(parent, false);

            LayoutElement le = item.AddComponent<LayoutElement>();
            le.preferredHeight = 130;

            Image bg = item.AddComponent<Image>();
            bg.color = CARD_BG;

            Outline outline = item.AddComponent<Outline>();
            outline.effectColor = isWin ? new Color(0.3f, 1f, 0.5f, 0.4f) : new Color(1f, 0.4f, 0.4f, 0.4f);
            outline.effectDistance = new Vector2(2, -2);

            // Result indicator (left side)
            GameObject indicator = new GameObject("ResultIndicator");
            indicator.transform.SetParent(item.transform, false);

            RectTransform indicatorRT = indicator.AddComponent<RectTransform>();
            indicatorRT.anchorMin = new Vector2(0, 0);
            indicatorRT.anchorMax = new Vector2(0.02f, 1);
            indicatorRT.sizeDelta = Vector2.zero;

            Image indicatorImg = indicator.AddComponent<Image>();
            indicatorImg.color = isWin ? new Color(0.3f, 1f, 0.5f) : new Color(1f, 0.4f, 0.4f);

            // Game icon
            GameObject iconObj = new GameObject("GameIcon");
            iconObj.transform.SetParent(item.transform, false);

            RectTransform iconRT = iconObj.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.03f, 0.2f);
            iconRT.anchorMax = new Vector2(0.12f, 0.8f);
            iconRT.sizeDelta = Vector2.zero;

            Image iconBg = iconObj.AddComponent<Image>();
            iconBg.color = new Color(0.2f, 0.15f, 0.1f, 0.9f);

            GameObject iconText = new GameObject("Text");
            iconText.transform.SetParent(iconObj.transform, false);

            RectTransform iconTextRT = iconText.AddComponent<RectTransform>();
            iconTextRT.anchorMin = Vector2.zero;
            iconTextRT.anchorMax = Vector2.one;
            iconTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI iconTMP = iconText.AddComponent<TextMeshProUGUI>();
            iconTMP.text = gameType.Contains("Torneo") ? "🏆" : "⚔";
            iconTMP.fontSize = 28;
            iconTMP.color = TEXT_GOLD;
            iconTMP.alignment = TextAlignmentOptions.Center;

            // Game type
            GameObject gameObj = new GameObject("GameType");
            gameObj.transform.SetParent(item.transform, false);

            RectTransform gameRT = gameObj.AddComponent<RectTransform>();
            gameRT.anchorMin = new Vector2(0.14f, 0.65f);
            gameRT.anchorMax = new Vector2(0.65f, 0.95f);
            gameRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI gameTMP = gameObj.AddComponent<TextMeshProUGUI>();
            gameTMP.text = gameType;
            gameTMP.fontSize = 26;
            gameTMP.color = TEXT_GOLD;
            gameTMP.fontStyle = FontStyles.Bold;
            gameTMP.alignment = TextAlignmentOptions.Left;

            // Opponent
            GameObject oppObj = new GameObject("Opponent");
            oppObj.transform.SetParent(item.transform, false);

            RectTransform oppRT = oppObj.AddComponent<RectTransform>();
            oppRT.anchorMin = new Vector2(0.14f, 0.35f);
            oppRT.anchorMax = new Vector2(0.65f, 0.65f);
            oppRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI oppTMP = oppObj.AddComponent<TextMeshProUGUI>();
            oppTMP.text = opponent.StartsWith("@") ? $"vs {opponent}" : opponent;
            oppTMP.fontSize = 22;
            oppTMP.color = CYAN_ACCENT;
            oppTMP.fontStyle = FontStyles.Bold;
            oppTMP.alignment = TextAlignmentOptions.Left;

            // Date/Time
            GameObject dateObj = new GameObject("DateTime");
            dateObj.transform.SetParent(item.transform, false);

            RectTransform dateRT = dateObj.AddComponent<RectTransform>();
            dateRT.anchorMin = new Vector2(0.14f, 0.08f);
            dateRT.anchorMax = new Vector2(0.65f, 0.35f);
            dateRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI dateTMP = dateObj.AddComponent<TextMeshProUGUI>();
            dateTMP.text = dateTime;
            dateTMP.fontSize = 18;
            dateTMP.color = TEXT_SECONDARY;
            dateTMP.alignment = TextAlignmentOptions.Left;

            // Result & Amount
            GameObject resultObj = new GameObject("Result");
            resultObj.transform.SetParent(item.transform, false);

            RectTransform resultRT = resultObj.AddComponent<RectTransform>();
            resultRT.anchorMin = new Vector2(0.66f, 0.5f);
            resultRT.anchorMax = new Vector2(0.98f, 0.95f);
            resultRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI resultTMP = resultObj.AddComponent<TextMeshProUGUI>();
            resultTMP.text = isWin ? "VICTORIA" : "DERROTA";
            resultTMP.fontSize = 22;
            resultTMP.color = isWin ? new Color(0.3f, 1f, 0.5f) : new Color(1f, 0.4f, 0.4f);
            resultTMP.fontStyle = FontStyles.Bold;
            resultTMP.alignment = TextAlignmentOptions.Right;

            // Amount
            GameObject amountObj = new GameObject("Amount");
            amountObj.transform.SetParent(item.transform, false);

            RectTransform amountRT = amountObj.AddComponent<RectTransform>();
            amountRT.anchorMin = new Vector2(0.66f, 0.25f);
            amountRT.anchorMax = new Vector2(0.98f, 0.55f);
            amountRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI amountTMP = amountObj.AddComponent<TextMeshProUGUI>();
            amountTMP.text = amount;
            amountTMP.fontSize = 28;
            amountTMP.color = isWin ? new Color(0.3f, 1f, 0.5f) : new Color(1f, 0.4f, 0.4f);
            amountTMP.fontStyle = FontStyles.Bold;
            amountTMP.alignment = TextAlignmentOptions.Right;

            // Score
            GameObject scoreObj = new GameObject("Score");
            scoreObj.transform.SetParent(item.transform, false);

            RectTransform scoreRT = scoreObj.AddComponent<RectTransform>();
            scoreRT.anchorMin = new Vector2(0.66f, 0.05f);
            scoreRT.anchorMax = new Vector2(0.98f, 0.28f);
            scoreRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI scoreTMP = scoreObj.AddComponent<TextMeshProUGUI>();
            scoreTMP.text = score;
            scoreTMP.fontSize = 16;
            scoreTMP.color = TEXT_SECONDARY;
            scoreTMP.alignment = TextAlignmentOptions.Right;
        }

        #endregion
    }
}
