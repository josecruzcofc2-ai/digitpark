using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// Construye el prefab de notificación toast para logros desbloqueados.
    /// Aparece desde arriba en cualquier escena cuando se desbloquea un logro.
    /// Estilo inspirado en Xbox Achievements.
    /// </summary>
    public class AchievementToastUIBuilder : EditorWindow
    {
        // ==================== COLORES DEL TEMA ====================
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.4f, 1f);

        private static readonly Color DARK_BG = new Color(0.04f, 0.06f, 0.1f, 0.98f);
        private static readonly Color PANEL_BG = new Color(0.05f, 0.08f, 0.12f, 0.95f);

        private static readonly Color TEXT_PRIMARY = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.7f, 0.75f, 0.8f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.02f, 0.05f, 0.1f, 1f);

        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GOLD_DARK = new Color(0.8f, 0.65f, 0f, 1f);
        private static readonly Color GOLD_GLOW = new Color(1f, 0.9f, 0.4f, 0.3f);

        private static readonly Color SUCCESS_GREEN = new Color(0.2f, 0.8f, 0.4f, 1f);
        private static readonly Color SECRET_PURPLE = new Color(0.7f, 0.3f, 1f, 1f);

        // ==================== DIMENSIONES ====================
        private const float TOAST_WIDTH = 950f;
        private const float TOAST_HEIGHT = 200f;
        private const float ICON_SIZE = 110f;
        private const float TOAST_MARGIN_TOP = 80f;
        private const float BORDER_RADIUS = 12f;

        [MenuItem("DigitPark/UI Builders/Common/Achievement Toast Notification", false, 190)]
        public static void BuildUI()
        {
            if (!EditorUtility.DisplayDialog("Achievement Toast Builder",
                "Esto creará el prefab de notificación de logros.\n\n" +
                "Incluye:\n" +
                "- Toast banner con animación slide-in/out\n" +
                "- Icono del logro\n" +
                "- Título y descripción\n" +
                "- Puntos ganados\n" +
                "- Barra de progreso\n" +
                "- Efectos de partículas\n\n" +
                "¿Continuar?",
                "Sí", "No"))
                return;

            CreateAchievementToastPrefab();
        }

        /// <summary>Called by AllScenesBatchBuilder — no dialogs.</summary>
        public static void BuildSilent()
        {
            CreateAchievementToastPrefab();
        }

        private static void CreateAchievementToastPrefab()
        {
            Debug.Log("[AchievementToast] ========== CREANDO PREFAB ==========");

            // Ensure directories exist
            string prefabDir = "Assets/_Project/Prefabs/Common";
            if (!System.IO.Directory.Exists(prefabDir))
            {
                System.IO.Directory.CreateDirectory(prefabDir);
                AssetDatabase.Refresh();
            }

            // Create root object
            GameObject toastRoot = new GameObject("AchievementToast");
            RectTransform rootRT = toastRoot.AddComponent<RectTransform>();

            // Full screen overlay for positioning
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.sizeDelta = Vector2.zero;
            rootRT.anchoredPosition = Vector2.zero;

            // Canvas Group for fade animations
            CanvasGroup rootCG = toastRoot.AddComponent<CanvasGroup>();
            rootCG.alpha = 1f;
            rootCG.interactable = true;
            rootCG.blocksRaycasts = false; // Don't block clicks on game

            // ==================== TOAST CONTAINER ====================
            GameObject toastContainer = CreateChild(toastRoot, "ToastContainer");
            RectTransform containerRT = toastContainer.GetComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0.5f, 1f);
            containerRT.anchorMax = new Vector2(0.5f, 1f);
            containerRT.pivot = new Vector2(0.5f, 1f);
            containerRT.anchoredPosition = new Vector2(0f, -TOAST_MARGIN_TOP);
            containerRT.sizeDelta = new Vector2(TOAST_WIDTH, TOAST_HEIGHT);

            // Toast Background with glow effect
            CreateToastBackground(toastContainer);

            // Content Layout
            GameObject content = CreateChild(toastContainer, "Content");
            RectTransform contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin = Vector2.zero;
            contentRT.anchorMax = Vector2.one;
            contentRT.sizeDelta = Vector2.zero;
            contentRT.offsetMin = new Vector2(15f, 12f);
            contentRT.offsetMax = new Vector2(-15f, -12f);

            HorizontalLayoutGroup contentHLG = content.AddComponent<HorizontalLayoutGroup>();
            contentHLG.spacing = 15f;
            contentHLG.padding = new RectOffset(0, 0, 0, 0);
            contentHLG.childAlignment = TextAnchor.MiddleLeft;
            contentHLG.childControlWidth = true;
            contentHLG.childControlHeight = true;
            contentHLG.childForceExpandWidth = false;
            contentHLG.childForceExpandHeight = false;

            // ==================== ICON SECTION ====================
            CreateIconSection(content);

            // ==================== INFO SECTION ====================
            CreateInfoSection(content);

            // ==================== GLOW EFFECTS ====================
            CreateGlowEffects(toastContainer);

            // ==================== SHINE EFFECT ====================
            CreateShineEffect(toastContainer);

            // ==================== PARTICLE SYSTEM ====================
            CreateParticleEffect(toastContainer);

            // ==================== ADD COMPONENT ====================
            // Add the AchievementToastUI component
            DigitPark.UI.AchievementToastUI toastUI = toastRoot.AddComponent<DigitPark.UI.AchievementToastUI>();

            // Get references and assign via SerializedObject
            // Note: References can be auto-assigned via DigitPark/Auto Assigners menu

            // ==================== SAVE PREFAB ====================
            string prefabPath = $"{prefabDir}/AchievementToast.prefab";

            // Remove old prefab if exists
            if (System.IO.File.Exists(prefabPath))
            {
                AssetDatabase.DeleteAsset(prefabPath);
            }

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(toastRoot, prefabPath);
            Object.DestroyImmediate(toastRoot);

            // Try to auto-connect references in the saved prefab
            ConnectPrefabReferences(savedPrefab);

            AssetDatabase.Refresh();

            Debug.Log($"[AchievementToast] Prefab creado en: {prefabPath}");

            if (!AllScenesBatchBuilder.SilentMode)
                EditorUtility.DisplayDialog("Achievement Toast Completado",
                    $"Prefab creado exitosamente en:\n{prefabPath}\n\n" +
                    "Elementos incluidos:\n" +
                    "- Toast container con fondo y bordes\n" +
                    "- Icono del logro (80x80)\n" +
                    "- Header 'LOGRO DESBLOQUEADO'\n" +
                    "- Título del logro\n" +
                    "- Descripción\n" +
                    "- Puntos ganados\n" +
                    "- Barra de progreso\n" +
                    "- Efectos de glow y partículas\n\n" +
                    "Ahora crea el AchievementNotificationManager.",
                    "OK");
        }

        // ==================== TOAST BACKGROUND ====================
        private static void CreateToastBackground(GameObject parent)
        {
            // Outer glow layer
            GameObject outerGlow = CreateChild(parent, "OuterGlow");
            RectTransform outerGlowRT = outerGlow.GetComponent<RectTransform>();
            outerGlowRT.anchorMin = Vector2.zero;
            outerGlowRT.anchorMax = Vector2.one;
            outerGlowRT.sizeDelta = new Vector2(8f, 8f);
            outerGlowRT.anchoredPosition = Vector2.zero;

            Image outerGlowImg = outerGlow.AddComponent<Image>();
            outerGlowImg.color = GOLD_GLOW;
            outerGlow.transform.SetAsFirstSibling();

            // Main background
            GameObject background = CreateChild(parent, "Background");
            RectTransform bgRT = background.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;

            Image bgImage = background.AddComponent<Image>();
            bgImage.color = DARK_BG;

            // Add outline for border
            Outline bgOutline = background.AddComponent<Outline>();
            bgOutline.effectColor = CYAN_NEON;
            bgOutline.effectDistance = new Vector2(2f, 2f);

            // Inner panel
            GameObject innerPanel = CreateChild(background, "InnerPanel");
            RectTransform innerRT = innerPanel.GetComponent<RectTransform>();
            innerRT.anchorMin = Vector2.zero;
            innerRT.anchorMax = Vector2.one;
            innerRT.sizeDelta = new Vector2(-4f, -4f);

            Image innerImage = innerPanel.AddComponent<Image>();
            innerImage.color = PANEL_BG;

            // Top accent line (gold)
            GameObject topAccent = CreateChild(parent, "TopAccent");
            RectTransform topAccentRT = topAccent.GetComponent<RectTransform>();
            topAccentRT.anchorMin = new Vector2(0f, 1f);
            topAccentRT.anchorMax = new Vector2(1f, 1f);
            topAccentRT.pivot = new Vector2(0.5f, 1f);
            topAccentRT.anchoredPosition = Vector2.zero;
            topAccentRT.sizeDelta = new Vector2(0f, 3f);

            Image topAccentImg = topAccent.AddComponent<Image>();
            topAccentImg.color = GOLD;

            // Bottom accent line (cyan)
            GameObject bottomAccent = CreateChild(parent, "BottomAccent");
            RectTransform bottomAccentRT = bottomAccent.GetComponent<RectTransform>();
            bottomAccentRT.anchorMin = new Vector2(0f, 0f);
            bottomAccentRT.anchorMax = new Vector2(1f, 0f);
            bottomAccentRT.pivot = new Vector2(0.5f, 0f);
            bottomAccentRT.anchoredPosition = Vector2.zero;
            bottomAccentRT.sizeDelta = new Vector2(0f, 2f);

            Image bottomAccentImg = bottomAccent.AddComponent<Image>();
            bottomAccentImg.color = CYAN_NEON;
        }

        // ==================== ICON SECTION ====================
        private static void CreateIconSection(GameObject parent)
        {
            GameObject iconSection = CreateChild(parent, "IconSection");
            RectTransform iconSectionRT = iconSection.GetComponent<RectTransform>();
            iconSectionRT.sizeDelta = new Vector2(ICON_SIZE + 10f, ICON_SIZE + 10f);

            LayoutElement iconLE = iconSection.AddComponent<LayoutElement>();
            iconLE.minWidth = ICON_SIZE + 10f;
            iconLE.minHeight = ICON_SIZE + 10f;
            iconLE.preferredWidth = ICON_SIZE + 10f;
            iconLE.preferredHeight = ICON_SIZE + 10f;

            // Icon glow background
            GameObject iconGlow = CreateChild(iconSection, "IconGlow");
            RectTransform iconGlowRT = iconGlow.GetComponent<RectTransform>();
            iconGlowRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconGlowRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconGlowRT.sizeDelta = new Vector2(ICON_SIZE + 20f, ICON_SIZE + 20f);

            Image iconGlowImg = iconGlow.AddComponent<Image>();
            iconGlowImg.color = GOLD_GLOW;

            // Icon frame
            GameObject iconFrame = CreateChild(iconSection, "IconFrame");
            RectTransform iconFrameRT = iconFrame.GetComponent<RectTransform>();
            iconFrameRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconFrameRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconFrameRT.sizeDelta = new Vector2(ICON_SIZE + 6f, ICON_SIZE + 6f);

            Image iconFrameImg = iconFrame.AddComponent<Image>();
            iconFrameImg.color = GOLD;

            // Icon background
            GameObject iconBg = CreateChild(iconSection, "IconBackground");
            RectTransform iconBgRT = iconBg.GetComponent<RectTransform>();
            iconBgRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconBgRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconBgRT.sizeDelta = new Vector2(ICON_SIZE, ICON_SIZE);

            Image iconBgImg = iconBg.AddComponent<Image>();
            iconBgImg.color = DARK_BG;

            // Achievement Icon (this will be set dynamically)
            GameObject achievementIcon = CreateChild(iconSection, "AchievementIcon");
            RectTransform achievementIconRT = achievementIcon.GetComponent<RectTransform>();
            achievementIconRT.anchorMin = new Vector2(0.5f, 0.5f);
            achievementIconRT.anchorMax = new Vector2(0.5f, 0.5f);
            achievementIconRT.sizeDelta = new Vector2(ICON_SIZE - 8f, ICON_SIZE - 8f);

            Image achievementIconImg = achievementIcon.AddComponent<Image>();
            achievementIconImg.color = Color.white;
            // Placeholder - will be replaced with actual achievement icon
        }

        // ==================== INFO SECTION ====================
        private static void CreateInfoSection(GameObject parent)
        {
            GameObject infoSection = CreateChild(parent, "InfoSection");
            RectTransform infoRT = infoSection.GetComponent<RectTransform>();

            LayoutElement infoLE = infoSection.AddComponent<LayoutElement>();
            infoLE.flexibleWidth = 1f;
            infoLE.minHeight = ICON_SIZE + 10f;

            VerticalLayoutGroup infoVLG = infoSection.AddComponent<VerticalLayoutGroup>();
            infoVLG.spacing = 4f;
            infoVLG.padding = new RectOffset(0, 0, 5, 5);
            infoVLG.childAlignment = TextAnchor.MiddleLeft;
            infoVLG.childControlWidth = true;
            infoVLG.childControlHeight = true;
            infoVLG.childForceExpandWidth = true;
            infoVLG.childForceExpandHeight = false;

            // Header Row (LOGRO DESBLOQUEADO + Points)
            GameObject headerRow = CreateChild(infoSection, "HeaderRow");
            HorizontalLayoutGroup headerHLG = headerRow.AddComponent<HorizontalLayoutGroup>();
            headerHLG.spacing = 10f;
            headerHLG.childAlignment = TextAnchor.MiddleLeft;
            headerHLG.childControlWidth = true;
            headerHLG.childControlHeight = true;
            headerHLG.childForceExpandWidth = false;

            LayoutElement headerLE = headerRow.AddComponent<LayoutElement>();
            headerLE.minHeight = 34f;
            headerLE.preferredHeight = 34f;

            // Star icon
            GameObject starIcon = CreateChild(headerRow, "StarIcon");
            LayoutElement starLE = starIcon.AddComponent<LayoutElement>();
            starLE.minWidth = 28f;
            starLE.minHeight = 28f;

            Image starImg = starIcon.AddComponent<Image>();
            starImg.color = GOLD;

            // Header Text
            GameObject headerText = CreateChild(headerRow, "HeaderText");
            TextMeshProUGUI headerTMP = headerText.AddComponent<TextMeshProUGUI>();
            headerTMP.text = "ACHIEVEMENT UNLOCKED!";
            headerTMP.fontSize = FontSizes.Body;
            headerTMP.fontStyle = FontStyles.Bold;
            headerTMP.color = CYAN_NEON;
            headerTMP.alignment = TextAlignmentOptions.MidlineLeft;
            headerTMP.enableAutoSizing = true;
            headerTMP.fontSizeMin = FontSizes.AutoMinBody;
            headerTMP.fontSizeMax = FontSizes.Body;
            headerTMP.overflowMode = TextOverflowModes.Ellipsis;

            LayoutElement headerTextLE = headerText.AddComponent<LayoutElement>();
            headerTextLE.flexibleWidth = 1f;

            // Achievement Title
            GameObject titleObj = CreateChild(infoSection, "AchievementTitle");
            TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "First Victory";
            titleTMP.fontSize = FontSizes.Body;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.color = GOLD;
            titleTMP.alignment = TextAlignmentOptions.MidlineLeft;
            titleTMP.enableAutoSizing = true;
            titleTMP.fontSizeMin = FontSizes.AutoMinBody;
            titleTMP.fontSizeMax = FontSizes.Body;
            titleTMP.overflowMode = TextOverflowModes.Ellipsis;

            LayoutElement titleLE = titleObj.AddComponent<LayoutElement>();
            titleLE.minHeight = 44f;
            titleLE.preferredHeight = 44f;

            // Achievement Description
            GameObject descObj = CreateChild(infoSection, "AchievementDescription");
            TextMeshProUGUI descTMP = descObj.AddComponent<TextMeshProUGUI>();
            descTMP.text = "Win your first game";
            descTMP.fontSize = FontSizes.Body;
            descTMP.color = TEXT_SECONDARY;
            descTMP.alignment = TextAlignmentOptions.MidlineLeft;
            descTMP.enableAutoSizing = true;
            descTMP.fontSizeMin = FontSizes.AutoMinBody;
            descTMP.fontSizeMax = FontSizes.Body;
            descTMP.overflowMode = TextOverflowModes.Ellipsis;

            LayoutElement descLE = descObj.AddComponent<LayoutElement>();
            descLE.minHeight = 28f;
            descLE.preferredHeight = 28f;

            // Progress Bar Section
            GameObject progressSection = CreateChild(infoSection, "ProgressSection");
            HorizontalLayoutGroup progressHLG = progressSection.AddComponent<HorizontalLayoutGroup>();
            progressHLG.spacing = 10f;
            progressHLG.childAlignment = TextAnchor.MiddleLeft;
            progressHLG.childControlWidth = true;
            progressHLG.childControlHeight = true;

            LayoutElement progressSectionLE = progressSection.AddComponent<LayoutElement>();
            progressSectionLE.minHeight = 24f;
            progressSectionLE.preferredHeight = 24f;

            // Progress Bar
            GameObject progressBar = CreateChild(progressSection, "ProgressBar");
            LayoutElement progressBarLE = progressBar.AddComponent<LayoutElement>();
            progressBarLE.flexibleWidth = 1f;
            progressBarLE.minHeight = 14f;

            Image progressBarBg = progressBar.AddComponent<Image>();
            progressBarBg.color = new Color(0.1f, 0.12f, 0.15f, 1f);

            // Progress Fill
            GameObject progressFill = CreateChild(progressBar, "Fill");
            RectTransform fillRT = progressFill.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = new Vector2(1f, 1f);
            fillRT.sizeDelta = Vector2.zero;

            Image fillImg = progressFill.AddComponent<Image>();
            fillImg.color = SUCCESS_GREEN;

            // Completion Text
            GameObject completionText = CreateChild(progressSection, "CompletionText");
            LayoutElement completionLE = completionText.AddComponent<LayoutElement>();
            completionLE.minWidth = 160f;

            TextMeshProUGUI completionTMP = completionText.AddComponent<TextMeshProUGUI>();
            completionTMP.text = "COMPLETED";
            completionTMP.fontSize = FontSizes.Body;
            completionTMP.fontStyle = FontStyles.Bold;
            completionTMP.color = SUCCESS_GREEN;
            completionTMP.alignment = TextAlignmentOptions.MidlineRight;
            completionTMP.enableAutoSizing = true;
            completionTMP.fontSizeMin = FontSizes.AutoMinBody;
            completionTMP.fontSizeMax = FontSizes.Body;
            completionTMP.overflowMode = TextOverflowModes.Ellipsis;
        }

        // ==================== GLOW EFFECTS ====================
        private static void CreateGlowEffects(GameObject parent)
        {
            // Corner glow effects
            CreateCornerGlow(parent, "TopLeftGlow", new Vector2(0f, 1f), new Vector2(0f, 1f));
            CreateCornerGlow(parent, "TopRightGlow", new Vector2(1f, 1f), new Vector2(1f, 1f));
            CreateCornerGlow(parent, "BottomLeftGlow", new Vector2(0f, 0f), new Vector2(0f, 0f));
            CreateCornerGlow(parent, "BottomRightGlow", new Vector2(1f, 0f), new Vector2(1f, 0f));
        }

        private static void CreateCornerGlow(GameObject parent, string name, Vector2 anchor, Vector2 pivot)
        {
            GameObject glow = CreateChild(parent, name);
            RectTransform glowRT = glow.GetComponent<RectTransform>();
            glowRT.anchorMin = anchor;
            glowRT.anchorMax = anchor;
            glowRT.pivot = pivot;
            glowRT.anchoredPosition = Vector2.zero;
            glowRT.sizeDelta = new Vector2(30f, 30f);

            Image glowImg = glow.AddComponent<Image>();
            glowImg.color = new Color(1f, 0.9f, 0.5f, 0.2f);

            glow.transform.SetAsFirstSibling();
        }

        // ==================== SHINE EFFECT ====================
        private static void CreateShineEffect(GameObject parent)
        {
            GameObject shine = CreateChild(parent, "ShineEffect");
            RectTransform shineRT = shine.GetComponent<RectTransform>();
            shineRT.anchorMin = new Vector2(0f, 0f);
            shineRT.anchorMax = new Vector2(0f, 1f);
            shineRT.pivot = new Vector2(0.5f, 0.5f);
            shineRT.anchoredPosition = new Vector2(-100f, 0f);
            shineRT.sizeDelta = new Vector2(50f, 0f);

            Image shineImg = shine.AddComponent<Image>();
            shineImg.color = new Color(1f, 1f, 1f, 0.15f);

            // Mask to keep shine within bounds
            GameObject mask = CreateChild(parent, "ShineMask");
            RectTransform maskRT = mask.GetComponent<RectTransform>();
            maskRT.anchorMin = Vector2.zero;
            maskRT.anchorMax = Vector2.one;
            maskRT.sizeDelta = Vector2.zero;

            mask.AddComponent<RectMask2D>();
            shine.transform.SetParent(mask.transform, false);
        }

        // ==================== PARTICLE EFFECT ====================
        private static void CreateParticleEffect(GameObject parent)
        {
            GameObject particles = CreateChild(parent, "Particles");
            RectTransform particlesRT = particles.GetComponent<RectTransform>();
            particlesRT.anchorMin = new Vector2(0.5f, 0.5f);
            particlesRT.anchorMax = new Vector2(0.5f, 0.5f);
            particlesRT.sizeDelta = Vector2.zero;

            // Note: ParticleSystem will be configured in the Manager
            // This is just a placeholder position
        }

        // ==================== CONNECT REFERENCES ====================
        private static void ConnectPrefabReferences(GameObject prefab)
        {
            if (prefab == null) return;

            var toastUI = prefab.GetComponent<DigitPark.UI.AchievementToastUI>();
            if (toastUI == null) return;

            // Use SerializedObject to set private serialized fields
            SerializedObject serializedObject = new SerializedObject(toastUI);

            // Find and assign references
            Transform root = prefab.transform;

            // Container
            SetReference(serializedObject, "toastContainer", FindChildRecursive(root, "ToastContainer"));

            // Canvas Group - add if missing
            var canvasGroup = prefab.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = prefab.AddComponent<CanvasGroup>();
            SetComponentReference(serializedObject, "canvasGroup", canvasGroup);

            // Background elements
            SetReference(serializedObject, "outerGlow", FindChildRecursive(root, "OuterGlow"));
            SetReference(serializedObject, "background", FindChildRecursive(root, "Background"));
            SetReference(serializedObject, "topAccent", FindChildRecursive(root, "TopAccent"));
            SetReference(serializedObject, "bottomAccent", FindChildRecursive(root, "BottomAccent"));

            // Icon elements
            SetReference(serializedObject, "iconGlow", FindChildRecursive(root, "IconGlow"));
            SetReference(serializedObject, "iconFrame", FindChildRecursive(root, "IconFrame"));
            SetReference(serializedObject, "achievementIcon", FindChildRecursive(root, "AchievementIcon"));

            // Text elements
            SetReference(serializedObject, "headerText", FindChildRecursive(root, "HeaderText"));
            SetReference(serializedObject, "titleText", FindChildRecursive(root, "AchievementTitle"));
            SetReference(serializedObject, "descriptionText", FindChildRecursive(root, "AchievementDescription"));
            SetReference(serializedObject, "completionText", FindChildRecursive(root, "CompletionText"));

            // Progress
            SetReference(serializedObject, "progressBarFill", FindChildRecursive(root, "Fill"));
            SetReference(serializedObject, "progressSection", FindChildRecursive(root, "ProgressSection"));

            // Effects
            SetReference(serializedObject, "shineEffect", FindChildRecursive(root, "ShineEffect"));

            serializedObject.ApplyModifiedProperties();

            // Save changes to prefab
            PrefabUtility.SavePrefabAsset(prefab);

            Debug.Log("[AchievementToast] Referencias conectadas al prefab");
        }

        private static void SetReference(SerializedObject obj, string propertyName, Transform target)
        {
            if (target == null) return;

            SerializedProperty prop = obj.FindProperty(propertyName);
            if (prop != null)
            {
                // Try to get the component based on property type
                if (prop.propertyType == SerializedPropertyType.ObjectReference)
                {
                    // Check what type is expected
                    string typeName = prop.type;

                    if (typeName.Contains("RectTransform"))
                    {
                        prop.objectReferenceValue = target.GetComponent<RectTransform>();
                    }
                    else if (typeName.Contains("Image"))
                    {
                        prop.objectReferenceValue = target.GetComponent<Image>();
                    }
                    else if (typeName.Contains("TextMeshProUGUI") || typeName.Contains("TMP_Text"))
                    {
                        prop.objectReferenceValue = target.GetComponent<TMPro.TextMeshProUGUI>();
                    }
                    else if (typeName.Contains("GameObject"))
                    {
                        prop.objectReferenceValue = target.gameObject;
                    }
                    else
                    {
                        // Default to component or gameobject
                        var img = target.GetComponent<Image>();
                        if (img != null)
                        {
                            prop.objectReferenceValue = img;
                        }
                        else
                        {
                            prop.objectReferenceValue = target.gameObject;
                        }
                    }
                }
            }
        }

        private static void SetComponentReference<T>(SerializedObject obj, string propertyName, T component) where T : Component
        {
            SerializedProperty prop = obj.FindProperty(propertyName);
            if (prop != null)
            {
                prop.objectReferenceValue = component;
            }
        }

        private static Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent.name == name) return parent;

            foreach (Transform child in parent)
            {
                Transform found = FindChildRecursive(child, name);
                if (found != null) return found;
            }

            return null;
        }

        // ==================== UTILITY METHODS ====================
        private static GameObject CreateChild(GameObject parent, string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent.transform, false);
            child.AddComponent<RectTransform>();
            return child;
        }
    }
}
