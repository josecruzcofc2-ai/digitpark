using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;
using DigitPark.UI.Panels;

namespace DigitPark.Editor
{
    /// <summary>
    /// Rebuilds the ErrorPanel prefab with proper dimensions,
    /// dark neon background, and fullscreen blocker overlay.
    /// </summary>
    public static class ErrorPanelPrefabBuilder
    {
        // Theme colors
        private static readonly Color BG_DARK = new Color(0.04f, 0.06f, 0.1f, 1f);
        private static readonly Color CARD_BG = new Color(0.08f, 0.12f, 0.18f, 0.98f);
        private static readonly Color BLOCKER_COLOR = new Color(0f, 0f, 0f, 0.8f);
        private static readonly Color TEXT_WHITE = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.65f, 0.75f, 1f);
        private static readonly Color CYAN = new Color(0f, 0.9f, 1f, 1f);
        private static readonly Color BUTTON_CYAN = new Color(0f, 0.75f, 0.85f, 1f);
        private static readonly Color RED_ERROR = new Color(1f, 0.3f, 0.35f, 1f);

        private const string PREFAB_PATH = "Assets/_Project/Prefabs/Common/ErrorPanel.prefab";

        [MenuItem("DigitPark/Prefabs/Common/Rebuild ErrorPanel", false, 500)]
        public static void RebuildErrorPanel()
        {
            // Create root GO
            var root = new GameObject("ErrorPanel");
            var rootRT = root.AddComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            // Attach ErrorPanelUI component
            var errorPanelUI = root.AddComponent<ErrorPanelUI>();

            // --- Panel (the inner container that gets shown/hidden) ---
            var panel = new GameObject("Panel");
            panel.transform.SetParent(root.transform, false);
            var panelRT = panel.AddComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;
            panel.SetActive(false);

            // CanvasGroup for animations
            var cg = panel.AddComponent<CanvasGroup>();
            cg.alpha = 1f;
            cg.blocksRaycasts = true;
            cg.interactable = true;

            // --- Blocker (fullscreen dark overlay behind card) ---
            var blocker = new GameObject("Blocker");
            blocker.transform.SetParent(panel.transform, false);
            var blockerRT = blocker.AddComponent<RectTransform>();
            blockerRT.anchorMin = Vector2.zero;
            blockerRT.anchorMax = Vector2.one;
            blockerRT.offsetMin = Vector2.zero;
            blockerRT.offsetMax = Vector2.zero;
            var blockerImg = blocker.AddComponent<Image>();
            blockerImg.color = BLOCKER_COLOR;
            blockerImg.raycastTarget = true;

            // --- Card (centered error card) ---
            var card = new GameObject("Card");
            card.transform.SetParent(panel.transform, false);
            var cardRT = card.AddComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0.1f, 0.3f);
            cardRT.anchorMax = new Vector2(0.9f, 0.7f);
            cardRT.offsetMin = Vector2.zero;
            cardRT.offsetMax = Vector2.zero;

            var cardBg = card.AddComponent<Image>();
            cardBg.color = CARD_BG;

            var cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = new Color(RED_ERROR.r, RED_ERROR.g, RED_ERROR.b, 0.4f);
            cardOutline.effectDistance = new Vector2(2, -2);

            // Card layout
            var cardVLG = card.AddComponent<VerticalLayoutGroup>();
            cardVLG.padding = new RectOffset(40, 40, 30, 30);
            cardVLG.spacing = 20;
            cardVLG.childAlignment = TextAnchor.MiddleCenter;
            cardVLG.childControlWidth = true;
            cardVLG.childControlHeight = false;
            cardVLG.childForceExpandWidth = true;
            cardVLG.childForceExpandHeight = false;

            // --- Error Icon (warning triangle) ---
            var iconObj = new GameObject("ErrorIcon");
            iconObj.transform.SetParent(card.transform, false);
            var iconLE = iconObj.AddComponent<LayoutElement>();
            iconLE.preferredHeight = 50;
            var iconTMP = iconObj.AddComponent<TextMeshProUGUI>();
            iconTMP.text = "\u26A0"; // ⚠
            iconTMP.fontSize = 42;
            iconTMP.enableAutoSizing = true;
            iconTMP.fontSizeMin = FontSizes.AutoMinBody;
            iconTMP.fontSizeMax = iconTMP.fontSize;
            iconTMP.fontStyle = FontStyles.Bold;
            iconTMP.color = RED_ERROR;
            iconTMP.alignment = TextAlignmentOptions.Center;

            // --- Title ---
            var titleObj = new GameObject("ErrorTitleText");
            titleObj.transform.SetParent(card.transform, false);
            var titleLE = titleObj.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 40;
            var titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "Error";
            titleTMP.fontSize = FontSizes.H3;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.color = RED_ERROR;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.enableAutoSizing = true;
            titleTMP.fontSizeMin = FontSizes.AutoMinSmall;
            titleTMP.fontSizeMax = FontSizes.H3;

            // --- Error Message Text ---
            var errorTextObj = new GameObject("ErrorText");
            errorTextObj.transform.SetParent(card.transform, false);
            var errorTextLE = errorTextObj.AddComponent<LayoutElement>();
            errorTextLE.preferredHeight = 80;
            errorTextLE.flexibleHeight = 1;
            var errorTMP = errorTextObj.AddComponent<TextMeshProUGUI>();
            errorTMP.text = "An error occurred";
            errorTMP.fontSize = FontSizes.Body;
            errorTMP.fontStyle = FontStyles.Bold;
            errorTMP.color = TEXT_WHITE;
            errorTMP.alignment = TextAlignmentOptions.Center;
            errorTMP.enableAutoSizing = true;
            errorTMP.fontSizeMin = FontSizes.AutoMinSmall;
            errorTMP.fontSizeMax = FontSizes.Body;
            errorTMP.enableWordWrapping = true;

            // --- Accept Button ---
            var btnObj = new GameObject("AcceptButton");
            btnObj.transform.SetParent(card.transform, false);
            var btnLE = btnObj.AddComponent<LayoutElement>();
            btnLE.preferredHeight = 55;
            btnLE.minHeight = 55;

            var btnBg = btnObj.AddComponent<Image>();
            btnBg.color = BUTTON_CYAN;

            var btnOutline = btnObj.AddComponent<Outline>();
            btnOutline.effectColor = new Color(CYAN.r, CYAN.g, CYAN.b, 0.5f);
            btnOutline.effectDistance = new Vector2(1.5f, -1.5f);

            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnBg;
            var colors = btn.colors;
            colors.normalColor = BUTTON_CYAN;
            colors.highlightedColor = CYAN;
            colors.pressedColor = new Color(0f, 0.5f, 0.6f, 1f);
            btn.colors = colors;

            // Button text
            var btnTextObj = new GameObject("AcceptButtonText");
            btnTextObj.transform.SetParent(btnObj.transform, false);
            var btnTextRT = btnTextObj.AddComponent<RectTransform>();
            btnTextRT.anchorMin = Vector2.zero;
            btnTextRT.anchorMax = Vector2.one;
            btnTextRT.offsetMin = Vector2.zero;
            btnTextRT.offsetMax = Vector2.zero;
            var btnTMP = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnTMP.text = "Accept";
            btnTMP.fontSize = FontSizes.Body;
            btnTMP.fontStyle = FontStyles.Bold;
            btnTMP.color = BG_DARK;
            btnTMP.alignment = TextAlignmentOptions.Center;
            btnTMP.enableAutoSizing = true;
            btnTMP.fontSizeMin = FontSizes.AutoMinSmall;
            btnTMP.fontSizeMax = FontSizes.Body;

            // --- Wire ErrorPanelUI references via SerializedObject ---
            var so = new SerializedObject(errorPanelUI);
            so.FindProperty("panel").objectReferenceValue = panel;
            so.FindProperty("errorText").objectReferenceValue = errorTMP;
            so.FindProperty("acceptButton").objectReferenceValue = btn;
            so.FindProperty("acceptButtonText").objectReferenceValue = btnTMP;
            so.ApplyModifiedProperties();

            // Save prefab
            PrefabUtility.SaveAsPrefabAsset(root, PREFAB_PATH);
            Object.DestroyImmediate(root);

            Debug.Log("[ErrorPanelPrefabBuilder] ErrorPanel rebuilt successfully at " + PREFAB_PATH);
        }
    }
}
