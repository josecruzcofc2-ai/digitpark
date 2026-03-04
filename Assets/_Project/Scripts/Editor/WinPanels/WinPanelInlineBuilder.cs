using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// Utilidad estática para crear paneles WinPanelController inline en escenas de juego.
    /// Los UIBuilders de cada juego llaman a estos métodos para crear paneles RealMoney
    /// directamente en la escena (no como prefabs).
    /// </summary>
    public static class WinPanelInlineBuilder
    {
        // Colores Dorados (Real Money Win)
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GOLD_DARK = new Color(0.7f, 0.55f, 0f, 1f);
        private static readonly Color GOLD_LIGHT = new Color(1f, 0.95f, 0.6f, 1f);
        private static readonly Color PREMIUM_BG = new Color(0.08f, 0.06f, 0.02f, 0.98f);
        private static readonly Color PREMIUM_PANEL = new Color(0.15f, 0.12f, 0.05f, 0.95f);

        // Colores Magenta (Real Money Lose)
        private static readonly Color LOSE_NEON = new Color(0.8f, 0.3f, 0.6f, 1f);
        private static readonly Color LOSE_NEON_LIGHT = new Color(1f, 0.5f, 0.8f, 1f);
        private static readonly Color LOSE_RED = new Color(1f, 0.4f, 0.4f, 1f);

        /// <summary>
        /// Crea los paneles WinPanelRealMoney y LosePanelRealMoney como hijos del parent dado.
        /// Retorna (winPanelRM, losePanelRM) ya configurados y ocultos.
        /// </summary>
        public static (GameObject winPanel, GameObject losePanel) CreateRealMoneyPanels(Transform parent)
        {
            var winPanel = CreateWinPanelRealMoney(parent);
            var losePanel = CreateLosePanelRealMoney(parent);
            return (winPanel, losePanel);
        }

        /// <summary>
        /// Asigna las referencias de WinPanelController a un MinigameBase (o DigitRushController)
        /// usando SerializedObject.
        /// </summary>
        public static void AssignWinPanelRefs(SerializedObject so, GameObject winPanelRM, GameObject losePanelRM)
        {
            var winProp = so.FindProperty("winPanelRealMoney");
            if (winProp != null && winPanelRM != null)
                winProp.objectReferenceValue = winPanelRM.GetComponent<WinPanelController>();

            var loseProp = so.FindProperty("losePanelRealMoney");
            if (loseProp != null && losePanelRM != null)
                loseProp.objectReferenceValue = losePanelRM.GetComponent<WinPanelController>();
        }

        private static GameObject CreateWinPanelRealMoney(Transform parent)
        {
            GameObject panel = CreateChild(parent, "WinPanel_RealMoney");
            SetFullStretch(panel.GetComponent<RectTransform>());

            Image overlay = panel.AddComponent<Image>();
            overlay.color = new Color(0.02f, 0.01f, 0f, 0.95f);
            overlay.raycastTarget = true;

            CanvasGroup cg = panel.AddComponent<CanvasGroup>();

            // Content
            GameObject content = CreateChild(panel.transform, "Content");
            SetupRectTransform(content, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(620, 680));

            CreatePremiumPanel3D(content.transform);
            Transform face = content.transform.Find("Face");

            // Title
            GameObject title = CreateChild(face, "Title");
            SetupRectTransform(title, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -80), new Vector2(0, 70));
            AddText(title, "YOU WON!", 52, GOLD, FontStyles.Bold);

            // Money
            GameObject moneyContainer = CreateChild(face, "MoneyContainer");
            SetupRectTransform(moneyContainer, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -170), new Vector2(380, 90));
            Image moneyBg = moneyContainer.AddComponent<Image>();
            moneyBg.color = new Color(0.2f, 0.15f, 0.02f, 0.8f);

            GameObject moneyText = CreateChild(moneyContainer.transform, "MoneyText");
            SetupRectTransform(moneyText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            AddText(moneyText, "+$10.00", 56, GOLD_LIGHT, FontStyles.Bold);

            // Wager
            GameObject wagerText = CreateChild(face, "WagerText");
            SetupRectTransform(wagerText, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -245), new Vector2(350, 35));
            AddText(wagerText, "Wager: $5.00", 22, new Color(0.8f, 0.7f, 0.5f), FontStyles.Bold);

            // VS Container
            GameObject vsContainer = CreateChild(face, "VSContainer");
            SetupRectTransform(vsContainer, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -30), new Vector2(500, 110));
            Image vsBg = vsContainer.AddComponent<Image>();
            vsBg.color = new Color(0.1f, 0.08f, 0.02f, 0.7f);

            // Player score
            GameObject playerScore = CreateChild(vsContainer.transform, "PlayerScore");
            SetupRectTransform(playerScore, new Vector2(0, 0), new Vector2(0.45f, 1), Vector2.zero, Vector2.zero);
            GameObject playerValue = CreateChild(playerScore.transform, "Value");
            SetupRectTransform(playerValue, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(180, 50));
            AddText(playerValue, "12.45s", 34, Color.white, FontStyles.Bold);

            // VS
            GameObject vsText = CreateChild(vsContainer.transform, "VSText");
            SetupRectTransform(vsText, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(60, 60));
            AddText(vsText, "VS", 26, GOLD_DARK, FontStyles.Bold);

            // Opponent score
            GameObject oppScore = CreateChild(vsContainer.transform, "OpponentScore");
            SetupRectTransform(oppScore, new Vector2(0.55f, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            GameObject oppValue = CreateChild(oppScore.transform, "Value");
            SetupRectTransform(oppValue, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(180, 50));
            AddText(oppValue, "15.32s", 34, new Color(0.6f, 0.6f, 0.6f), FontStyles.Bold);

            // Stats
            GameObject statsContainer = CreateChild(face, "StatsContainer");
            SetupRectTransform(statsContainer, new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 140), new Vector2(420, 90));
            CreateStatRow(statsContainer.transform, "TimeRow", "Time", "00.00s", GOLD_LIGHT, 20);
            CreateStatRow(statsContainer.transform, "ErrorsRow", "Errors", "0", GOLD_LIGHT, -20);

            // Accept button
            GameObject btnContainer = CreateChild(face, "ButtonContainer");
            SetupRectTransform(btnContainer, new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 50), new Vector2(320, 75));
            CreateButton(btnContainer.transform, "AcceptButton", "CLAIM", GOLD, 300, 65);

            // WinPanelController
            WinPanelController controller = panel.AddComponent<WinPanelController>();
            AssignWinPanelControllerRefs(controller, panel, content, true);

            panel.SetActive(false);
            return panel;
        }

        private static GameObject CreateLosePanelRealMoney(Transform parent)
        {
            GameObject panel = CreateChild(parent, "LosePanel_RealMoney");
            SetFullStretch(panel.GetComponent<RectTransform>());

            Image overlay = panel.AddComponent<Image>();
            overlay.color = new Color(0.03f, 0.01f, 0.04f, 0.95f);
            overlay.raycastTarget = true;

            CanvasGroup cg = panel.AddComponent<CanvasGroup>();

            // Content
            GameObject content = CreateChild(panel.transform, "Content");
            SetupRectTransform(content, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(600, 660));

            CreateLosePanel3D(content.transform);
            Transform face = content.transform.Find("Face");

            // Title
            GameObject title = CreateChild(face, "Title");
            SetupRectTransform(title, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -80), new Vector2(0, 70));
            AddText(title, "DEFEAT", 48, LOSE_NEON_LIGHT, FontStyles.Bold);

            // Money lost
            GameObject moneyContainer = CreateChild(face, "MoneyContainer");
            SetupRectTransform(moneyContainer, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -170), new Vector2(360, 85));
            Image moneyBg = moneyContainer.AddComponent<Image>();
            moneyBg.color = new Color(0.12f, 0.04f, 0.06f, 0.8f);

            GameObject moneyText = CreateChild(moneyContainer.transform, "MoneyText");
            SetupRectTransform(moneyText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            AddText(moneyText, "-$5.00", 50, LOSE_RED, FontStyles.Bold);

            // VS Container
            GameObject vsContainer = CreateChild(face, "VSContainer");
            SetupRectTransform(vsContainer, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -30), new Vector2(500, 110));
            Image vsBg = vsContainer.AddComponent<Image>();
            vsBg.color = new Color(0.06f, 0.03f, 0.06f, 0.8f);

            // Player score
            GameObject playerScore = CreateChild(vsContainer.transform, "PlayerScore");
            SetupRectTransform(playerScore, new Vector2(0, 0), new Vector2(0.45f, 1), Vector2.zero, Vector2.zero);
            GameObject playerValue = CreateChild(playerScore.transform, "Value");
            SetupRectTransform(playerValue, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(180, 50));
            AddText(playerValue, "15.32s", 34, new Color(0.8f, 0.6f, 0.7f), FontStyles.Bold);

            // VS
            GameObject vsText = CreateChild(vsContainer.transform, "VSText");
            SetupRectTransform(vsText, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(60, 60));
            AddText(vsText, "VS", 26, LOSE_NEON, FontStyles.Bold);

            // Opponent score
            GameObject oppScore = CreateChild(vsContainer.transform, "OpponentScore");
            SetupRectTransform(oppScore, new Vector2(0.55f, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            GameObject oppValue = CreateChild(oppScore.transform, "Value");
            SetupRectTransform(oppValue, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(180, 50));
            AddText(oppValue, "12.45s", 34, new Color(0.3f, 1f, 0.5f), FontStyles.Bold);

            // Stats
            GameObject statsContainer = CreateChild(face, "StatsContainer");
            SetupRectTransform(statsContainer, new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 140), new Vector2(420, 90));
            CreateStatRow(statsContainer.transform, "TimeRow", "Time", "00.00s", LOSE_NEON_LIGHT, 20);
            CreateStatRow(statsContainer.transform, "ErrorsRow", "Errors", "0", LOSE_NEON_LIGHT, -20);

            // Buttons
            GameObject btnsContainer = CreateChild(face, "ButtonsContainer");
            SetupRectTransform(btnsContainer, new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 50), new Vector2(-40, 85));
            HorizontalLayoutGroup btnLayout = btnsContainer.AddComponent<HorizontalLayoutGroup>();
            btnLayout.childAlignment = TextAnchor.MiddleCenter;
            btnLayout.spacing = 20;
            btnLayout.childForceExpandWidth = false;
            btnLayout.childControlWidth = false;

            CreateButton(btnsContainer.transform, "AcceptButton", "EXIT", new Color(0.4f, 0.35f, 0.45f), 150, 65);

            // WinPanelController
            WinPanelController controller = panel.AddComponent<WinPanelController>();
            AssignWinPanelControllerRefs(controller, panel, content, false);

            panel.SetActive(false);
            return panel;
        }

        private static void AssignWinPanelControllerRefs(WinPanelController controller, GameObject panel, GameObject content, bool isWin)
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
                so.FindProperty("timeText").objectReferenceValue =
                    face.Find("StatsContainer/TimeRow/Value")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("errorsText").objectReferenceValue =
                    face.Find("StatsContainer/ErrorsRow/Value")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("vsContainer").objectReferenceValue =
                    face.Find("VSContainer")?.gameObject;
                so.FindProperty("playerScoreText").objectReferenceValue =
                    face.Find("VSContainer/PlayerScore/Value")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("opponentScoreText").objectReferenceValue =
                    face.Find("VSContainer/OpponentScore/Value")?.GetComponent<TextMeshProUGUI>();

                if (isWin)
                {
                    so.FindProperty("wagerText").objectReferenceValue =
                        face.Find("WagerText")?.GetComponent<TextMeshProUGUI>();
                    so.FindProperty("acceptButton").objectReferenceValue =
                        face.Find("ButtonContainer/AcceptButton")?.GetComponent<Button>();
                }
                else
                {
                    so.FindProperty("acceptButton").objectReferenceValue =
                        face.Find("ButtonsContainer/AcceptButton")?.GetComponent<Button>();
                }
            }
            so.ApplyModifiedProperties();
        }

        #region Panel 3D Effects

        private static void CreatePremiumPanel3D(Transform parent)
        {
            GameObject shadow = CreateChild(parent, "Shadow");
            RectTransform srt = shadow.GetComponent<RectTransform>();
            srt.anchorMin = Vector2.zero; srt.anchorMax = Vector2.one;
            srt.sizeDelta = Vector2.zero; srt.anchoredPosition = new Vector2(8, -15);
            shadow.AddComponent<Image>().color = new Color(0, 0, 0, 0.6f);

            GameObject side = CreateChild(parent, "Side");
            SetupRectTransform(side, new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, -8), new Vector2(0, 16));
            side.AddComponent<Image>().color = GOLD_DARK;

            GameObject face = CreateChild(parent, "Face");
            SetFullStretch(face.GetComponent<RectTransform>());
            face.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 8);
            face.AddComponent<Image>().color = PREMIUM_PANEL;

            Outline outerGlow = face.AddComponent<Outline>();
            outerGlow.effectColor = GOLD;
            outerGlow.effectDistance = new Vector2(4, -4);
        }

        private static void CreateLosePanel3D(Transform parent)
        {
            Color panelBg = new Color(0.06f, 0.03f, 0.08f, 0.95f);

            GameObject shadow = CreateChild(parent, "Shadow");
            RectTransform srt = shadow.GetComponent<RectTransform>();
            srt.anchorMin = Vector2.zero; srt.anchorMax = Vector2.one;
            srt.sizeDelta = Vector2.zero; srt.anchoredPosition = new Vector2(8, -15);
            shadow.AddComponent<Image>().color = new Color(0, 0, 0, 0.6f);

            GameObject side = CreateChild(parent, "Side");
            SetupRectTransform(side, new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, -8), new Vector2(0, 16));
            side.AddComponent<Image>().color = new Color(LOSE_NEON.r * 0.4f, LOSE_NEON.g * 0.2f, LOSE_NEON.b * 0.35f, 1f);

            GameObject face = CreateChild(parent, "Face");
            SetFullStretch(face.GetComponent<RectTransform>());
            face.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 8);
            face.AddComponent<Image>().color = panelBg;

            Outline faceOutline = face.AddComponent<Outline>();
            faceOutline.effectColor = LOSE_NEON;
            faceOutline.effectDistance = new Vector2(4, -4);
        }

        #endregion

        #region Helpers

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
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinBody;
            tmp.fontSizeMax = size;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            return tmp;
        }

        private static void CreateStatRow(Transform parent, string name, string label, string value, Color color, float yOffset)
        {
            GameObject row = CreateChild(parent, name);
            SetupRectTransform(row, new Vector2(0, 0.5f), new Vector2(1, 0.5f),
                new Vector2(0, yOffset), new Vector2(0, 35));

            GameObject labelObj = CreateChild(row.transform, "Label");
            SetupRectTransform(labelObj, new Vector2(0, 0), new Vector2(0.5f, 1), Vector2.zero, Vector2.zero);
            TextMeshProUGUI labelTmp = AddText(labelObj, label, 20, new Color(0.7f, 0.7f, 0.7f), FontStyles.Bold);
            labelTmp.alignment = TextAlignmentOptions.Left;

            GameObject valueObj = CreateChild(row.transform, "Value");
            SetupRectTransform(valueObj, new Vector2(0.5f, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            TextMeshProUGUI valueTmp = AddText(valueObj, value, 24, color, FontStyles.Bold);
            valueTmp.alignment = TextAlignmentOptions.Right;
        }

        private static void CreateButton(Transform parent, string name, string text, Color color, float width, float height)
        {
            GameObject btn = CreateChild(parent, name);
            LayoutElement layout = btn.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = height;

            SetupRectTransform(btn, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(width, height));

            GameObject face = CreateChild(btn.transform, "Face");
            SetupRectTransform(face, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image faceImg = face.AddComponent<Image>();
            faceImg.color = color;

            GameObject textObj = CreateChild(face.transform, "Text");
            SetupRectTransform(textObj, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-10, -6));
            AddText(textObj, text, Mathf.Min(height * 0.35f, FontSizes.Body), new Color(0.02f, 0.05f, 0.1f), FontStyles.Bold);

            Button button = btn.AddComponent<Button>();
            button.targetGraphic = faceImg;
        }

        #endregion
    }
}
