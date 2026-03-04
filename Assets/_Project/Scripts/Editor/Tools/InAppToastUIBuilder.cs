using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// InApp Toast UI Builder - Crea el prefab del toast de notificaciones in-app
    /// Toast compacto que aparece desde arriba de la pantalla
    /// Portrait 9:16 (1080x1920)
    ///
    /// Menu: DigitPark/UI Builders/Common/InApp Toast
    /// </summary>
    public class InAppToastUIBuilder : EditorWindow
    {
        #region Colors

        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color DARK_BG = new Color(0.04f, 0.06f, 0.12f, 0.96f);
        private static readonly Color CARD_BG = new Color(0.08f, 0.1f, 0.14f, 1f);
        private static readonly Color TEXT_WHITE = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.6f, 0.65f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.1f, 0.1f, 0.1f, 1f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);

        #endregion

        private const string PREFAB_PATH = "Assets/_Project/Prefabs/Common/InAppToast.prefab";

        [MenuItem("DigitPark/UI Builders/Common/InApp Toast", false, 191)]
        public static void ShowWindow()
        {
            GetWindow<InAppToastUIBuilder>("InApp Toast Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("InApp Toast UI Builder", EditorStyles.boldLabel);
            GUILayout.Label("Toast de notificación in-app - Neon Theme", EditorStyles.miniLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Crea el prefab InAppToast.prefab:\n\n" +
                "• Container (slide from top)\n" +
                "• Type Icon (emoji por categoría)\n" +
                "• Title + Body\n" +
                "• Primary/Secondary action buttons\n" +
                "• Dismiss X button\n" +
                "• Border color por categoría",
                MessageType.Info);

            GUILayout.Space(15);

            GUI.backgroundColor = CYAN_NEON;
            if (GUILayout.Button("CREAR INAPP TOAST PREFAB", GUILayout.Height(50)))
                CreateInAppToastPrefab();
            GUI.backgroundColor = Color.white;
        }

        private static void CreateInAppToastPrefab()
        {
            // Root container
            var root = new GameObject("InAppToast");
            var rootRT = root.AddComponent<RectTransform>();
            rootRT.anchorMin = new Vector2(0.03f, 0.92f);
            rootRT.anchorMax = new Vector2(0.97f, 0.985f);
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            var cg = root.AddComponent<CanvasGroup>();
            cg.alpha = 1f;

            var bg = root.AddComponent<Image>();
            bg.color = DARK_BG;

            var outline = root.AddComponent<Outline>();
            outline.effectColor = new Color(CYAN_NEON.r, CYAN_NEON.g, CYAN_NEON.b, 0.6f);
            outline.effectDistance = new Vector2(1.5f, 1.5f);

            var cardBtn = root.AddComponent<Button>();
            cardBtn.targetGraphic = bg;
            var cardColors = cardBtn.colors;
            cardColors.normalColor = Color.white;
            cardColors.highlightedColor = new Color(1, 1, 1, 0.95f);
            cardColors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            cardBtn.colors = cardColors;

            // ---- Type Icon (left) ----
            var typeIcon = new GameObject("TypeIcon");
            typeIcon.transform.SetParent(root.transform, false);
            var tiRT = typeIcon.AddComponent<RectTransform>();
            tiRT.anchorMin = new Vector2(0, 0.1f);
            tiRT.anchorMax = new Vector2(0, 0.9f);
            tiRT.pivot = new Vector2(0, 0.5f);
            tiRT.anchoredPosition = new Vector2(12, 0);
            tiRT.sizeDelta = new Vector2(45, 0);
            var tiBg = typeIcon.AddComponent<Image>();
            tiBg.color = new Color(1, 1, 1, 0.06f);

            var iconText = new GameObject("IconText");
            iconText.transform.SetParent(typeIcon.transform, false);
            var itRT = iconText.AddComponent<RectTransform>();
            itRT.anchorMin = Vector2.zero;
            itRT.anchorMax = Vector2.one;
            itRT.offsetMin = Vector2.zero;
            itRT.offsetMax = Vector2.zero;
            var itTMP = iconText.AddComponent<TextMeshProUGUI>();
            itTMP.text = "🔔";
            itTMP.fontSize = FontSizes.Body;
            itTMP.fontStyle = FontStyles.Bold;
            itTMP.color = CYAN_NEON;
            itTMP.alignment = TextAlignmentOptions.Center;

            // ---- Info Section (center) ----
            var infoSection = new GameObject("InfoSection");
            infoSection.transform.SetParent(root.transform, false);
            var isRT = infoSection.AddComponent<RectTransform>();
            isRT.anchorMin = new Vector2(0, 0);
            isRT.anchorMax = new Vector2(1, 1);
            isRT.offsetMin = new Vector2(65, 5);
            isRT.offsetMax = new Vector2(-45, -5);

            // Title
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(infoSection.transform, false);
            var tTitleRT = titleGO.AddComponent<RectTransform>();
            tTitleRT.anchorMin = new Vector2(0, 0.55f);
            tTitleRT.anchorMax = new Vector2(0.70f, 1);
            tTitleRT.offsetMin = Vector2.zero;
            tTitleRT.offsetMax = Vector2.zero;
            var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "Notification";
            titleTMP.fontSize = FontSizes.Body;
            titleTMP.color = TEXT_WHITE;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Left;
            titleTMP.overflowMode = TextOverflowModes.Ellipsis;
            titleTMP.maxVisibleLines = 1;
            titleTMP.enableAutoSizing = true;
            titleTMP.fontSizeMin = FontSizes.AutoMinBody;
            titleTMP.fontSizeMax = FontSizes.Body;


            // Body
            var bodyGO = new GameObject("Body");
            bodyGO.transform.SetParent(infoSection.transform, false);
            var bodyRT = bodyGO.AddComponent<RectTransform>();
            bodyRT.anchorMin = new Vector2(0, 0);
            bodyRT.anchorMax = new Vector2(0.70f, 0.55f);
            bodyRT.offsetMin = Vector2.zero;
            bodyRT.offsetMax = Vector2.zero;
            var bodyTMP = bodyGO.AddComponent<TextMeshProUGUI>();
            bodyTMP.text = "Notification description";
            bodyTMP.fontSize = FontSizes.Body;
            bodyTMP.fontStyle = FontStyles.Bold;
            bodyTMP.color = TEXT_SECONDARY;
            bodyTMP.alignment = TextAlignmentOptions.Left;
            bodyTMP.overflowMode = TextOverflowModes.Ellipsis;
            bodyTMP.maxVisibleLines = 1;
            bodyTMP.enableAutoSizing = true;
            bodyTMP.fontSizeMin = FontSizes.AutoMinBody;
            bodyTMP.fontSizeMax = FontSizes.Body;


            // ---- Actions (right side) ----
            var actionsRow = new GameObject("ActionsRow");
            actionsRow.transform.SetParent(infoSection.transform, false);
            var arRT = actionsRow.AddComponent<RectTransform>();
            arRT.anchorMin = new Vector2(0.72f, 0.08f);
            arRT.anchorMax = new Vector2(1, 0.92f);
            arRT.offsetMin = Vector2.zero;
            arRT.offsetMax = Vector2.zero;

            var arHLG = actionsRow.AddComponent<HorizontalLayoutGroup>();
            arHLG.spacing = 5;
            arHLG.childAlignment = TextAnchor.MiddleRight;
            arHLG.childControlWidth = true;
            arHLG.childControlHeight = true;
            arHLG.childForceExpandWidth = true;
            arHLG.childForceExpandHeight = true;

            // Primary Button
            var primaryBtn = CreateToastButton(actionsRow.transform, "PrimaryButton", "Accept", CYAN_NEON, TEXT_DARK);

            // Secondary Button
            var secondaryBtn = CreateToastButton(actionsRow.transform, "SecondaryButton", "✕", CARD_BG, TEXT_SECONDARY);

            // ---- Dismiss X (far right) ----
            var dismissGO = new GameObject("DismissButton");
            dismissGO.transform.SetParent(root.transform, false);
            var dRT = dismissGO.AddComponent<RectTransform>();
            dRT.anchorMin = new Vector2(1, 0.5f);
            dRT.anchorMax = new Vector2(1, 0.5f);
            dRT.pivot = new Vector2(1, 0.5f);
            dRT.anchoredPosition = new Vector2(-6, 0);
            dRT.sizeDelta = new Vector2(28, 28);
            var dBg = dismissGO.AddComponent<Image>();
            dBg.color = new Color(1, 1, 1, 0.04f);
            var dBtn = dismissGO.AddComponent<Button>();
            dBtn.targetGraphic = dBg;

            var xText = new GameObject("X");
            xText.transform.SetParent(dismissGO.transform, false);
            var xRT = xText.AddComponent<RectTransform>();
            xRT.anchorMin = Vector2.zero;
            xRT.anchorMax = Vector2.one;
            xRT.offsetMin = Vector2.zero;
            xRT.offsetMax = Vector2.zero;
            var xTMP = xText.AddComponent<TextMeshProUGUI>();
            xTMP.text = "✕";
            xTMP.fontSize = FontSizes.Body;
            xTMP.fontStyle = FontStyles.Bold;
            xTMP.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            xTMP.alignment = TextAlignmentOptions.Center;

            // ---- Add InAppToastUI Component ----
            var toastUI = root.AddComponent<DigitPark.UI.InAppToastUI>();

            // Assign references via SerializedObject
            var so = new SerializedObject(toastUI);
            SetRef(so, "toastContainer", rootRT);
            SetRef(so, "canvasGroup", cg);
            SetRef(so, "background", bg);
            SetRef(so, "borderOutline", outline);
            SetRef(so, "typeIconText", itTMP);
            SetRef(so, "titleText", titleTMP);
            SetRef(so, "bodyText", bodyTMP);
            SetRef(so, "cardButton", cardBtn);
            SetRef(so, "dismissButton", dBtn);
            SetRef(so, "primaryButton", primaryBtn.GetComponent<Button>());
            SetRef(so, "primaryButtonText", primaryBtn.transform.Find("Text")?.GetComponent<TextMeshProUGUI>());
            SetRef(so, "primaryButtonBg", primaryBtn.GetComponent<Image>());
            SetRef(so, "secondaryButton", secondaryBtn.GetComponent<Button>());
            SetRef(so, "secondaryButtonText", secondaryBtn.transform.Find("Text")?.GetComponent<TextMeshProUGUI>());
            so.ApplyModifiedProperties();

            // ---- Save as Prefab ----
            string prefabDir = "Assets/_Project/Prefabs/Common";
            if (!AssetDatabase.IsValidFolder(prefabDir))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Common");
            }

            PrefabUtility.SaveAsPrefabAsset(root, PREFAB_PATH);
            DestroyImmediate(root);

            Debug.Log($"[InAppToastUI] Prefab guardado en: {PREFAB_PATH}");
        }

        private static GameObject CreateToastButton(Transform parent, string name, string label, Color bgColor, Color textColor)
        {
            var btn = new GameObject(name);
            btn.transform.SetParent(parent, false);

            var btnBg = btn.AddComponent<Image>();
            btnBg.color = bgColor;
            var button = btn.AddComponent<Button>();
            button.targetGraphic = btnBg;

            var btnOutline = btn.AddComponent<Outline>();
            btnOutline.effectColor = new Color(textColor.r, textColor.g, textColor.b, 0.2f);
            btnOutline.effectDistance = new Vector2(1, 1);

            var text = new GameObject("Text");
            text.transform.SetParent(btn.transform, false);
            var tRT = text.AddComponent<RectTransform>();
            tRT.anchorMin = Vector2.zero;
            tRT.anchorMax = Vector2.one;
            tRT.offsetMin = new Vector2(3, 0);
            tRT.offsetMax = new Vector2(-3, 0);
            var tTMP = text.AddComponent<TextMeshProUGUI>();
            tTMP.text = label;
            tTMP.fontSize = FontSizes.Body;
            tTMP.color = textColor;
            tTMP.fontStyle = FontStyles.Bold;
            tTMP.alignment = TextAlignmentOptions.Center;
            tTMP.enableAutoSizing = true;
            tTMP.fontSizeMin = FontSizes.AutoMinBody;
            tTMP.fontSizeMax = FontSizes.Body;
            tTMP.overflowMode = TextOverflowModes.Ellipsis;


            return btn;
        }

        private static void SetRef(SerializedObject so, string propName, Object value)
        {
            var prop = so.FindProperty(propName);
            if (prop == null) { Debug.LogWarning($"[InAppToastUI] Property '{propName}' no encontrada"); return; }
            if (value != null) { prop.objectReferenceValue = value; }
        }
    }
}
