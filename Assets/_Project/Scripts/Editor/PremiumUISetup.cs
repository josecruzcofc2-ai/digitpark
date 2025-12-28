using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para configurar la UI de Premium en las escenas
    /// Ejecutar desde el menú: DigitPark > Setup Premium UI
    /// </summary>
    public class PremiumUISetup : EditorWindow
    {
        [MenuItem("DigitPark/Setup Premium UI")]
        public static void ShowWindow()
        {
            GetWindow<PremiumUISetup>("Premium UI Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Premium UI Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.Label("Este script configurará la UI de Premium en las escenas.", EditorStyles.wordWrappedLabel);
            GUILayout.Space(20);

            if (GUILayout.Button("1. Setup MainMenu Scene (Premium Button)", GUILayout.Height(40)))
            {
                SetupMainMenuScene();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("2. Setup Game Scene (Premium Banner)", GUILayout.Height(40)))
            {
                SetupGameScene();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("3. Setup Settings Scene (Premium Section)", GUILayout.Height(40)))
            {
                SetupSettingsScene();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("4. Setup Tournaments Scene (Premium Required Panel)", GUILayout.Height(40)))
            {
                SetupTournamentsScene();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("5. Setup Boot Scene (Debug Controller)", GUILayout.Height(40)))
            {
                SetupBootScene();
            }

            GUILayout.Space(20);
            GUILayout.Label("Instrucciones:", EditorStyles.boldLabel);
            GUILayout.Label("1. Guarda todas las escenas abiertas\n2. Ejecuta cada botón en orden\n3. Revisa que los elementos se hayan creado\n4. Guarda las escenas modificadas", EditorStyles.wordWrappedLabel);

            GUILayout.Space(20);
            GUILayout.Label("Debug Controller:", EditorStyles.boldLabel);
            GUILayout.Label("En la escena Boot, el PremiumDebugController te permite:\n• Activar/desactivar 'Sin Anuncios'\n• Activar/desactivar 'Crear Torneos'\n• Los cambios se aplican en tiempo real", EditorStyles.wordWrappedLabel);
        }

        #region MainMenu Scene Setup

        private static void SetupMainMenuScene()
        {
            // Abrir la escena de MainMenu
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/MainMenu.unity", OpenSceneMode.Single);

            // Buscar el MainMenuPanel
            GameObject mainMenuPanel = GameObject.Find("MainMenuPanel");
            if (mainMenuPanel == null)
            {
                Debug.LogError("[PremiumUISetup] No se encontró MainMenuPanel en la escena MainMenu");
                return;
            }

            // Buscar Canvas para poner el botón fuera del layout
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[PremiumUISetup] No se encontró Canvas en la escena MainMenu");
                return;
            }

            // Verificar si ya existe PremiumButton (puede estar en Canvas o MainMenuPanel)
            GameObject existingButton = GameObject.Find("PremiumButton");
            if (existingButton != null)
            {
                Debug.LogWarning("[PremiumUISetup] PremiumButton ya existe. ¿Deseas recrearlo?");
                if (!EditorUtility.DisplayDialog("PremiumButton existe",
                    "PremiumButton ya existe en la escena. ¿Deseas eliminarlo y recrearlo?",
                    "Sí, recrear", "No, cancelar"))
                {
                    return;
                }
                DestroyImmediate(existingButton);
            }

            // Tamaño compacto
            Vector2 buttonSize = new Vector2(180, 60);

            // Crear PremiumButton como hijo del Canvas (fuera del VerticalLayoutGroup)
            GameObject premiumButton = CreatePremiumButtonCompact(canvas.transform, Vector2.zero, buttonSize);

            // Buscar MainMenuManager y conectar referencias
            var mainMenuManager = Object.FindObjectOfType<DigitPark.Managers.MainMenuManager>();
            if (mainMenuManager != null)
            {
                ConnectMainMenuManagerReferences(mainMenuManager, premiumButton);
            }
            else
            {
                Debug.LogWarning("[PremiumUISetup] No se encontró MainMenuManager. Conecta las referencias manualmente.");
            }

            // Marcar la escena como modificada
            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log("[PremiumUISetup] ✅ MainMenu Scene configurada correctamente. ¡Guarda la escena!");
            EditorUtility.DisplayDialog("Éxito", "PremiumButton creado en MainMenu.\n\n¡Recuerda guardar la escena (Ctrl+S)!", "OK");
        }

        private static GameObject CreatePremiumButtonCompact(Transform parent, Vector2 position, Vector2 size)
        {
            // Colores del tema Neon
            Color neonGold = new Color(1f, 0.84f, 0f, 1f);
            Color bgCard = new Color(0.0627f, 0.0824f, 0.1569f, 1f);

            // Crear el botón
            GameObject btnObj = new GameObject("PremiumButton");
            btnObj.transform.SetParent(parent, false);

            RectTransform btnRT = btnObj.AddComponent<RectTransform>();
            // Anclar a esquina superior derecha
            btnRT.anchorMin = new Vector2(1, 1);
            btnRT.anchorMax = new Vector2(1, 1);
            btnRT.pivot = new Vector2(1, 1);
            btnRT.anchoredPosition = new Vector2(-20, -20); // Margen desde esquina
            btnRT.sizeDelta = size;

            // Image de fondo con borde dorado
            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = bgCard;
            btnImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            btnImage.type = Image.Type.Sliced;

            // Agregar Outline dorado (efecto neon)
            Outline outline = btnObj.AddComponent<Outline>();
            outline.effectColor = neonGold;
            outline.effectDistance = new Vector2(2, 2);

            // Componente Button
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImage;

            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            btn.colors = colors;

            // Contenedor horizontal para icono + texto
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(btnObj.transform, false);

            RectTransform contentRT = contentObj.AddComponent<RectTransform>();
            contentRT.anchorMin = Vector2.zero;
            contentRT.anchorMax = Vector2.one;
            contentRT.offsetMin = new Vector2(8, 5);
            contentRT.offsetMax = new Vector2(-8, -5);

            HorizontalLayoutGroup hlg = contentObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 5;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // Icono de estrella
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(contentObj.transform, false);

            RectTransform iconRT = iconObj.AddComponent<RectTransform>();
            iconRT.sizeDelta = new Vector2(30, 30);

            TextMeshProUGUI iconTMP = iconObj.AddComponent<TextMeshProUGUI>();
            iconTMP.text = "★";
            iconTMP.fontSize = 24;
            iconTMP.color = neonGold;
            iconTMP.alignment = TextAlignmentOptions.Center;

            LayoutElement iconLE = iconObj.AddComponent<LayoutElement>();
            iconLE.preferredWidth = 30;
            iconLE.preferredHeight = 30;

            // Texto del botón
            GameObject textObj = new GameObject("PremiumButtonText");
            textObj.transform.SetParent(contentObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.sizeDelta = new Vector2(100, 30);

            TextMeshProUGUI textTMP = textObj.AddComponent<TextMeshProUGUI>();
            textTMP.text = "PRO";
            textTMP.fontSize = 22;
            textTMP.color = neonGold;
            textTMP.alignment = TextAlignmentOptions.Center;
            textTMP.fontStyle = FontStyles.Bold;

            LayoutElement textLE = textObj.AddComponent<LayoutElement>();
            textLE.preferredWidth = 100;
            textLE.preferredHeight = 30;

            // Crear Badge checkmark (se muestra cuando ya es premium)
            GameObject badgeObj = new GameObject("PremiumBadge");
            badgeObj.transform.SetParent(btnObj.transform, false);

            RectTransform badgeRT = badgeObj.AddComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(1, 1);
            badgeRT.anchorMax = new Vector2(1, 1);
            badgeRT.pivot = new Vector2(0.5f, 0.5f);
            badgeRT.anchoredPosition = new Vector2(5, 5);
            badgeRT.sizeDelta = new Vector2(24, 24);

            Image badgeImage = badgeObj.AddComponent<Image>();
            badgeImage.color = new Color(0.2f, 1f, 0.4f, 1f); // Verde neon

            GameObject badgeTextObj = new GameObject("BadgeText");
            badgeTextObj.transform.SetParent(badgeObj.transform, false);

            RectTransform badgeTextRT = badgeTextObj.AddComponent<RectTransform>();
            badgeTextRT.anchorMin = Vector2.zero;
            badgeTextRT.anchorMax = Vector2.one;
            badgeTextRT.offsetMin = Vector2.zero;
            badgeTextRT.offsetMax = Vector2.zero;

            TextMeshProUGUI badgeTextTMP = badgeTextObj.AddComponent<TextMeshProUGUI>();
            badgeTextTMP.text = "✓";
            badgeTextTMP.fontSize = 16;
            badgeTextTMP.color = Color.black;
            badgeTextTMP.alignment = TextAlignmentOptions.Center;
            badgeTextTMP.fontStyle = FontStyles.Bold;

            // El badge empieza oculto
            badgeObj.SetActive(false);

            return btnObj;
        }

        private static void ConnectMainMenuManagerReferences(DigitPark.Managers.MainMenuManager manager, GameObject premiumButton)
        {
            SerializedObject so = new SerializedObject(manager);

            // Conectar premiumButton
            SerializedProperty premiumBtnProp = so.FindProperty("premiumButton");
            if (premiumBtnProp != null)
                premiumBtnProp.objectReferenceValue = premiumButton.GetComponent<Button>();

            // Conectar premiumBadge
            Transform badge = premiumButton.transform.Find("PremiumBadge");
            if (badge != null)
            {
                SerializedProperty badgeProp = so.FindProperty("premiumBadge");
                if (badgeProp != null)
                    badgeProp.objectReferenceValue = badge.gameObject;
            }

            // Conectar premiumPanel si existe el campo
            SerializedProperty panelProp = so.FindProperty("premiumPanel");
            if (panelProp != null)
            {
                // El panel se crea dinámicamente, dejarlo null
                panelProp.objectReferenceValue = null;
            }

            so.ApplyModifiedProperties();
            Debug.Log("[PremiumUISetup] Referencias conectadas al MainMenuManager");
        }

        #endregion

        #region Game Scene Setup

        private static void SetupGameScene()
        {
            // Abrir la escena de Game
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Game.unity", OpenSceneMode.Single);

            // Buscar el WinMessagePanel (panel de resultados)
            GameObject resultPanel = GameObject.Find("WinMessagePanel");
            if (resultPanel == null)
            {
                // Intentar buscar alternativas
                resultPanel = GameObject.Find("ResultPanel");
                if (resultPanel == null)
                    resultPanel = GameObject.Find("GameOverPanel");
                if (resultPanel == null)
                    resultPanel = GameObject.Find("EndPanel");

                // Buscar en el Canvas
                if (resultPanel == null)
                {
                    Canvas canvas = Object.FindObjectOfType<Canvas>();
                    if (canvas != null)
                    {
                        foreach (Transform child in canvas.transform)
                        {
                            if (child.name.ToLower().Contains("win") ||
                                child.name.ToLower().Contains("result") ||
                                child.name.ToLower().Contains("gameover"))
                            {
                                resultPanel = child.gameObject;
                                break;
                            }
                        }
                    }
                }

                if (resultPanel == null)
                {
                    Debug.LogError("[PremiumUISetup] No se encontró WinMessagePanel en la escena Game.");
                    EditorUtility.DisplayDialog("Error", "No se encontró el panel de resultados (WinMessagePanel).\n\nVerifica que la escena Game tenga este panel.", "OK");
                    return;
                }
            }

            // Verificar si ya existe PremiumBannerContainer
            Transform existingBanner = resultPanel.transform.Find("PremiumBannerContainer");
            if (existingBanner != null)
            {
                Debug.LogWarning("[PremiumUISetup] PremiumBannerContainer ya existe. ¿Deseas recrearlo?");
                if (!EditorUtility.DisplayDialog("PremiumBannerContainer existe",
                    "PremiumBannerContainer ya existe en la escena. ¿Deseas eliminarlo y recrearlo?",
                    "Sí, recrear", "No, cancelar"))
                {
                    return;
                }
                DestroyImmediate(existingBanner.gameObject);
            }

            // Crear PremiumBanner
            GameObject premiumBanner = CreatePremiumBanner(resultPanel.transform);

            // Buscar GameManager y conectar referencias
            var gameManager = Object.FindObjectOfType<DigitPark.Managers.GameManager>();
            if (gameManager != null)
            {
                ConnectGameManagerReferences(gameManager, premiumBanner);
            }
            else
            {
                Debug.LogWarning("[PremiumUISetup] No se encontró GameManager. Conecta las referencias manualmente.");
            }

            // Marcar la escena como modificada
            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log("[PremiumUISetup] ✅ Game Scene configurada correctamente. ¡Guarda la escena!");
            EditorUtility.DisplayDialog("Éxito", "PremiumBanner creado en Game.\n\n¡Recuerda guardar la escena (Ctrl+S)!", "OK");
        }

        private static GameObject CreatePremiumBanner(Transform parent)
        {
            // Colores del tema Neon
            Color neonCyan = new Color(0f, 0.9608f, 1f, 1f);
            Color bgCard = new Color(0.0627f, 0.0824f, 0.1569f, 0.95f);

            // Crear contenedor
            GameObject containerObj = new GameObject("PremiumBannerContainer");
            containerObj.transform.SetParent(parent, false);

            RectTransform containerRT = containerObj.AddComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0.5f, 0);
            containerRT.anchorMax = new Vector2(0.5f, 0);
            containerRT.pivot = new Vector2(0.5f, 0);
            containerRT.anchoredPosition = new Vector2(0, 30);
            containerRT.sizeDelta = new Vector2(600, 80);

            // Fondo con borde neon
            Image bgImage = containerObj.AddComponent<Image>();
            bgImage.color = bgCard;
            bgImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            bgImage.type = Image.Type.Sliced;

            Outline outline = containerObj.AddComponent<Outline>();
            outline.effectColor = neonCyan;
            outline.effectDistance = new Vector2(2, 2);

            // Crear botón
            GameObject btnObj = new GameObject("PremiumBannerButton");
            btnObj.transform.SetParent(containerObj.transform, false);

            RectTransform btnRT = btnObj.AddComponent<RectTransform>();
            btnRT.anchorMin = Vector2.zero;
            btnRT.anchorMax = Vector2.one;
            btnRT.offsetMin = Vector2.zero;
            btnRT.offsetMax = Vector2.zero;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bgImage;

            // Crear texto
            GameObject textObj = new GameObject("PremiumBannerText");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(20, 10);
            textRT.offsetMax = new Vector2(-20, -10);

            TextMeshProUGUI textTMP = textObj.AddComponent<TextMeshProUGUI>();
            textTMP.text = "¡Quita los anuncios por solo $9.99!";
            textTMP.fontSize = 22;
            textTMP.color = neonCyan;
            textTMP.alignment = TextAlignmentOptions.Center;
            textTMP.fontStyle = FontStyles.Bold;

            return containerObj;
        }

        private static void ConnectGameManagerReferences(DigitPark.Managers.GameManager manager, GameObject premiumBanner)
        {
            SerializedObject so = new SerializedObject(manager);

            // Conectar premiumBannerContainer
            SerializedProperty containerProp = so.FindProperty("premiumBannerContainer");
            if (containerProp != null)
                containerProp.objectReferenceValue = premiumBanner;

            // Conectar premiumBannerButton
            Transform btnTransform = premiumBanner.transform.Find("PremiumBannerButton");
            if (btnTransform != null)
            {
                SerializedProperty btnProp = so.FindProperty("premiumBannerButton");
                if (btnProp != null)
                    btnProp.objectReferenceValue = btnTransform.GetComponent<Button>();
            }

            // Conectar premiumBannerText
            Transform textTransform = premiumBanner.transform.Find("PremiumBannerButton/PremiumBannerText");
            if (textTransform != null)
            {
                SerializedProperty textProp = so.FindProperty("premiumBannerText");
                if (textProp != null)
                    textProp.objectReferenceValue = textTransform.GetComponent<TextMeshProUGUI>();
            }

            so.ApplyModifiedProperties();
            Debug.Log("[PremiumUISetup] Referencias conectadas al GameManager");
        }

        #endregion

        #region Settings Scene Setup

        private static void SetupSettingsScene()
        {
            // Abrir la escena de Settings
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Settings.unity", OpenSceneMode.Single);

            // Buscar el SettingsPanel
            GameObject settingsPanel = GameObject.Find("SettingsPanel");
            if (settingsPanel == null)
            {
                Debug.LogError("[PremiumUISetup] No se encontró SettingsPanel en la escena Settings");
                return;
            }

            // Buscar el Canvas para obtener referencias
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[PremiumUISetup] No se encontró Canvas en la escena Settings");
                return;
            }

            // Verificar si ya existe PremiumSection
            Transform existingSection = settingsPanel.transform.Find("PremiumSection");
            if (existingSection != null)
            {
                Debug.LogWarning("[PremiumUISetup] PremiumSection ya existe. ¿Deseas recrearlo?");
                if (!EditorUtility.DisplayDialog("PremiumSection existe",
                    "PremiumSection ya existe en la escena. ¿Deseas eliminarlo y recrearlo?",
                    "Sí, recrear", "No, cancelar"))
                {
                    return;
                }
                DestroyImmediate(existingSection.gameObject);
            }

            // Crear PremiumSection
            GameObject premiumSection = CreatePremiumSection(settingsPanel.transform);

            // Buscar SettingsManager y conectar referencias
            var settingsManager = Object.FindObjectOfType<DigitPark.Managers.SettingsManager>();
            if (settingsManager != null)
            {
                ConnectSettingsManagerReferences(settingsManager, premiumSection);
            }
            else
            {
                Debug.LogWarning("[PremiumUISetup] No se encontró SettingsManager. Conecta las referencias manualmente.");
            }

            // Marcar la escena como modificada
            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log("[PremiumUISetup] ✅ Settings Scene configurada correctamente. ¡Guarda la escena!");
            EditorUtility.DisplayDialog("Éxito", "Premium Section creada en Settings.\n\n¡Recuerda guardar la escena (Ctrl+S)!", "OK");
        }

        private static GameObject CreatePremiumSection(Transform parent)
        {
            // Colores del tema Neon
            Color bgCard = new Color(0.0627f, 0.0824f, 0.1569f, 1f);
            Color neonCyan = new Color(0f, 0.9608f, 1f, 1f);
            Color neonPurple = new Color(0.6157f, 0.2941f, 1f, 1f);
            Color neonGreen = new Color(0.2353f, 1f, 0.4196f, 1f);

            // Crear contenedor PremiumSection
            GameObject sectionObj = new GameObject("PremiumSection");
            sectionObj.transform.SetParent(parent, false);

            RectTransform sectionRT = sectionObj.AddComponent<RectTransform>();
            sectionRT.anchorMin = new Vector2(0.5f, 0.5f);
            sectionRT.anchorMax = new Vector2(0.5f, 0.5f);
            sectionRT.anchoredPosition = new Vector2(0, -320); // Debajo de los demás elementos
            sectionRT.sizeDelta = new Vector2(650, 280);

            // Agregar VerticalLayoutGroup
            VerticalLayoutGroup vlg = sectionObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 15;
            vlg.padding = new RectOffset(20, 20, 20, 20);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Agregar fondo
            Image bgImage = sectionObj.AddComponent<Image>();
            bgImage.color = new Color(bgCard.r, bgCard.g, bgCard.b, 0.8f);
            bgImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            bgImage.type = Image.Type.Sliced;

            // 1. Título "Premium"
            GameObject titleObj = CreateTextElement(sectionObj.transform, "PremiumSectionTitle", "Premium", 32, neonCyan, TextAlignmentOptions.Center);
            LayoutElement titleLE = titleObj.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 40;

            // 2. Botón "Quitar Anuncios - $10 MXN"
            GameObject removeAdsBtn = CreateButton(sectionObj.transform, "RemoveAdsButton", "Quitar Anuncios - $10 MXN", bgCard, neonGreen);
            LayoutElement removeAdsLE = removeAdsBtn.AddComponent<LayoutElement>();
            removeAdsLE.preferredHeight = 55;

            // 3. Botón "Premium Completo - $20 MXN"
            GameObject premiumFullBtn = CreateButton(sectionObj.transform, "PremiumFullButton", "Premium Completo - $20 MXN", bgCard, neonPurple);
            LayoutElement premiumFullLE = premiumFullBtn.AddComponent<LayoutElement>();
            premiumFullLE.preferredHeight = 55;

            // 4. Botón "Restaurar Compras"
            GameObject restoreBtn = CreateButton(sectionObj.transform, "RestorePurchasesButton", "Restaurar Compras", bgCard, Color.gray);
            LayoutElement restoreLE = restoreBtn.AddComponent<LayoutElement>();
            restoreLE.preferredHeight = 45;

            // Hacer el botón de restaurar más pequeño
            TextMeshProUGUI restoreText = restoreBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (restoreText != null) restoreText.fontSize = 18;

            return sectionObj;
        }

        private static GameObject CreateButton(Transform parent, string name, string text, Color bgColor, Color textColor)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            RectTransform btnRT = btnObj.AddComponent<RectTransform>();
            btnRT.sizeDelta = new Vector2(600, 55);

            // Image de fondo
            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = bgColor;
            btnImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            btnImage.type = Image.Type.Sliced;

            // Componente Button
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImage;

            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            btn.colors = colors;

            // Texto del botón
            GameObject textObj = CreateTextElement(btnObj.transform, name + "Text", text, 22, textColor, TextAlignmentOptions.Center);
            RectTransform textRT = textObj.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            return btnObj;
        }

        private static GameObject CreateTextElement(Transform parent, string name, string text, float fontSize, Color color, TextAlignmentOptions alignment)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0.5f, 0.5f);
            textRT.anchorMax = new Vector2(0.5f, 0.5f);
            textRT.sizeDelta = new Vector2(580, 40);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.fontStyle = FontStyles.Bold;

            return textObj;
        }

        private static void ConnectSettingsManagerReferences(DigitPark.Managers.SettingsManager manager, GameObject premiumSection)
        {
            SerializedObject so = new SerializedObject(manager);

            // Conectar premiumSection
            SerializedProperty premiumSectionProp = so.FindProperty("premiumSection");
            if (premiumSectionProp != null)
                premiumSectionProp.objectReferenceValue = premiumSection;

            // Conectar removeAdsButton
            Transform removeAdsBtn = premiumSection.transform.Find("RemoveAdsButton");
            if (removeAdsBtn != null)
            {
                SerializedProperty removeAdsProp = so.FindProperty("removeAdsButton");
                if (removeAdsProp != null)
                    removeAdsProp.objectReferenceValue = removeAdsBtn.GetComponent<Button>();

                SerializedProperty removeAdsTextProp = so.FindProperty("removeAdsButtonText");
                if (removeAdsTextProp != null)
                    removeAdsTextProp.objectReferenceValue = removeAdsBtn.GetComponentInChildren<TextMeshProUGUI>();
            }

            // Conectar premiumFullButton
            Transform premiumFullBtn = premiumSection.transform.Find("PremiumFullButton");
            if (premiumFullBtn != null)
            {
                SerializedProperty premiumFullProp = so.FindProperty("premiumFullButton");
                if (premiumFullProp != null)
                    premiumFullProp.objectReferenceValue = premiumFullBtn.GetComponent<Button>();

                SerializedProperty premiumFullTextProp = so.FindProperty("premiumFullButtonText");
                if (premiumFullTextProp != null)
                    premiumFullTextProp.objectReferenceValue = premiumFullBtn.GetComponentInChildren<TextMeshProUGUI>();
            }

            // Conectar restorePurchasesButton
            Transform restoreBtn = premiumSection.transform.Find("RestorePurchasesButton");
            if (restoreBtn != null)
            {
                SerializedProperty restoreProp = so.FindProperty("restorePurchasesButton");
                if (restoreProp != null)
                    restoreProp.objectReferenceValue = restoreBtn.GetComponent<Button>();
            }

            so.ApplyModifiedProperties();
            Debug.Log("[PremiumUISetup] Referencias conectadas al SettingsManager");
        }

        #endregion

        #region Tournaments Scene Setup

        private static void SetupTournamentsScene()
        {
            // Abrir la escena de Tournaments
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Tournaments.unity", OpenSceneMode.Single);

            // Buscar el Canvas
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[PremiumUISetup] No se encontró Canvas en la escena Tournaments");
                return;
            }

            // Verificar si ya existe PremiumRequiredPanel
            GameObject existingPanel = GameObject.Find("PremiumRequiredPanel");
            if (existingPanel != null)
            {
                Debug.LogWarning("[PremiumUISetup] PremiumRequiredPanel ya existe. ¿Deseas recrearlo?");
                if (!EditorUtility.DisplayDialog("PremiumRequiredPanel existe",
                    "PremiumRequiredPanel ya existe en la escena. ¿Deseas eliminarlo y recrearlo?",
                    "Sí, recrear", "No, cancelar"))
                {
                    return;
                }
                DestroyImmediate(existingPanel);
            }

            // Crear PremiumRequiredPanel
            GameObject premiumPanel = CreatePremiumRequiredPanel(canvas.transform);

            // Buscar TournamentManager y conectar referencias
            var tournamentManager = Object.FindObjectOfType<DigitPark.Managers.TournamentManager>();
            if (tournamentManager != null)
            {
                ConnectTournamentManagerReferences(tournamentManager, premiumPanel);
            }
            else
            {
                Debug.LogWarning("[PremiumUISetup] No se encontró TournamentManager. Conecta las referencias manualmente.");
            }

            // Marcar la escena como modificada
            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log("[PremiumUISetup] ✅ Tournaments Scene configurada correctamente. ¡Guarda la escena!");
            EditorUtility.DisplayDialog("Éxito", "PremiumRequiredPanel creado en Tournaments.\n\n¡Recuerda guardar la escena (Ctrl+S)!", "OK");
        }

        private static GameObject CreatePremiumRequiredPanel(Transform parent)
        {
            // Colores del tema Neon
            Color bgDeepSpace = new Color(0.0196f, 0.0314f, 0.0784f, 0.95f);
            Color bgCard = new Color(0.0627f, 0.0824f, 0.1569f, 1f);
            Color neonCyan = new Color(0f, 0.9608f, 1f, 1f);
            Color neonPurple = new Color(0.6157f, 0.2941f, 1f, 1f);

            // Crear el panel principal (blocker)
            GameObject panelObj = new GameObject("PremiumRequiredPanel");
            panelObj.transform.SetParent(parent, false);
            panelObj.SetActive(true); // Activo para que ConfirmPanelUI funcione

            RectTransform panelRT = panelObj.AddComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            // Image de fondo oscuro (blocker)
            Image blockerImage = panelObj.AddComponent<Image>();
            blockerImage.color = bgDeepSpace;

            // Agregar el componente ConfirmPanelUI
            var confirmPanelUI = panelObj.AddComponent<DigitPark.UI.Panels.ConfirmPanelUI>();

            // Crear el contenedor del panel interior
            GameObject innerPanel = new GameObject("Panel");
            innerPanel.transform.SetParent(panelObj.transform, false);

            RectTransform innerRT = innerPanel.AddComponent<RectTransform>();
            innerRT.anchorMin = new Vector2(0.5f, 0.5f);
            innerRT.anchorMax = new Vector2(0.5f, 0.5f);
            innerRT.sizeDelta = new Vector2(700, 350);

            Image innerImage = innerPanel.AddComponent<Image>();
            innerImage.color = bgCard;
            innerImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            innerImage.type = Image.Type.Sliced;

            // VerticalLayoutGroup para organizar contenido
            VerticalLayoutGroup vlg = innerPanel.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 20;
            vlg.padding = new RectOffset(30, 30, 30, 30);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // 1. Título
            GameObject titleObj = CreateTextElement(innerPanel.transform, "TitleText", "Se Requiere Premium", 36, neonCyan, TextAlignmentOptions.Center);
            LayoutElement titleLE = titleObj.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 50;

            // 2. Mensaje
            GameObject messageObj = CreateTextElement(innerPanel.transform, "MessageText",
                "Necesitas Premium para crear torneos.\n¡Obtén Premium Completo para desbloquear esta función!",
                22, Color.white, TextAlignmentOptions.Center);
            LayoutElement messageLE = messageObj.AddComponent<LayoutElement>();
            messageLE.preferredHeight = 80;
            TextMeshProUGUI msgTMP = messageObj.GetComponent<TextMeshProUGUI>();
            msgTMP.fontStyle = FontStyles.Normal;

            // 3. Contenedor de botones
            GameObject buttonsContainer = new GameObject("ButtonsContainer");
            buttonsContainer.transform.SetParent(innerPanel.transform, false);
            RectTransform buttonsCRT = buttonsContainer.AddComponent<RectTransform>();
            buttonsCRT.sizeDelta = new Vector2(600, 60);

            HorizontalLayoutGroup hlg = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            LayoutElement buttonsLE = buttonsContainer.AddComponent<LayoutElement>();
            buttonsLE.preferredHeight = 60;

            // Botón "Obtener Premium"
            GameObject getPremiumBtn = CreateButton(buttonsContainer.transform, "ConfirmButton", "Obtener Premium", neonPurple, Color.white);
            RectTransform getPremiumRT = getPremiumBtn.GetComponent<RectTransform>();
            getPremiumRT.sizeDelta = new Vector2(280, 55);

            // Botón "Quizás Después"
            GameObject laterBtn = CreateButton(buttonsContainer.transform, "CancelButton", "Quizás Después", bgCard, Color.gray);
            RectTransform laterRT = laterBtn.GetComponent<RectTransform>();
            laterRT.sizeDelta = new Vector2(280, 55);

            // Conectar referencias al ConfirmPanelUI mediante SerializedObject
            SerializedObject so = new SerializedObject(confirmPanelUI);

            SerializedProperty panelProp = so.FindProperty("panel");
            if (panelProp != null) panelProp.objectReferenceValue = innerPanel;

            SerializedProperty blockerProp = so.FindProperty("blockerPanel");
            if (blockerProp != null) blockerProp.objectReferenceValue = panelObj;

            SerializedProperty titleProp = so.FindProperty("titleText");
            if (titleProp != null) titleProp.objectReferenceValue = titleObj.GetComponent<TextMeshProUGUI>();

            SerializedProperty messageProp = so.FindProperty("messageText");
            if (messageProp != null) messageProp.objectReferenceValue = messageObj.GetComponent<TextMeshProUGUI>();

            SerializedProperty confirmBtnProp = so.FindProperty("confirmButton");
            if (confirmBtnProp != null) confirmBtnProp.objectReferenceValue = getPremiumBtn.GetComponent<Button>();

            SerializedProperty cancelBtnProp = so.FindProperty("cancelButton");
            if (cancelBtnProp != null) cancelBtnProp.objectReferenceValue = laterBtn.GetComponent<Button>();

            SerializedProperty confirmTextProp = so.FindProperty("confirmButtonText");
            if (confirmTextProp != null) confirmTextProp.objectReferenceValue = getPremiumBtn.GetComponentInChildren<TextMeshProUGUI>();

            SerializedProperty cancelTextProp = so.FindProperty("cancelButtonText");
            if (cancelTextProp != null) cancelTextProp.objectReferenceValue = laterBtn.GetComponentInChildren<TextMeshProUGUI>();

            so.ApplyModifiedProperties();

            return panelObj;
        }

        private static void ConnectTournamentManagerReferences(DigitPark.Managers.TournamentManager manager, GameObject premiumPanel)
        {
            SerializedObject so = new SerializedObject(manager);

            SerializedProperty premiumPanelProp = so.FindProperty("premiumRequiredPanel");
            if (premiumPanelProp != null)
            {
                var confirmPanelUI = premiumPanel.GetComponent<DigitPark.UI.Panels.ConfirmPanelUI>();
                premiumPanelProp.objectReferenceValue = confirmPanelUI;
            }

            so.ApplyModifiedProperties();
            Debug.Log("[PremiumUISetup] Referencias conectadas al TournamentManager");
        }

        #endregion

        #region Boot Scene Setup

        private static void SetupBootScene()
        {
            // Abrir la escena de Boot
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Boot.unity", OpenSceneMode.Single);

            // Verificar si ya existe PremiumDebugController
            GameObject existingController = GameObject.Find("PremiumDebugController");
            if (existingController != null)
            {
                Debug.LogWarning("[PremiumUISetup] PremiumDebugController ya existe. ¿Deseas recrearlo?");
                if (!EditorUtility.DisplayDialog("PremiumDebugController existe",
                    "PremiumDebugController ya existe en la escena. ¿Deseas eliminarlo y recrearlo?",
                    "Sí, recrear", "No, cancelar"))
                {
                    return;
                }
                DestroyImmediate(existingController);
            }

            // Buscar AuthenticationService para posicionar cerca
            GameObject authService = GameObject.Find("AuthenticationService");

            // Crear PremiumDebugController
            GameObject debugController = new GameObject("PremiumDebugController");

            // Agregar el componente
            debugController.AddComponent<DigitPark.Tools.PremiumDebugController>();

            // Posicionar cerca del AuthenticationService si existe
            if (authService != null)
            {
                debugController.transform.position = authService.transform.position + new Vector3(0, -50, 0);
            }

            // Marcar la escena como modificada
            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log("[PremiumUISetup] ✅ Boot Scene configurada con PremiumDebugController. ¡Guarda la escena!");
            EditorUtility.DisplayDialog("Éxito",
                "PremiumDebugController creado en Boot.\n\n" +
                "Uso:\n" +
                "• Selecciona 'PremiumDebugController' en la jerarquía\n" +
                "• En el Inspector, activa/desactiva las opciones de premium\n" +
                "• Los cambios se aplican automáticamente\n\n" +
                "¡Recuerda guardar la escena (Ctrl+S)!", "OK");
        }

        #endregion
    }
}
