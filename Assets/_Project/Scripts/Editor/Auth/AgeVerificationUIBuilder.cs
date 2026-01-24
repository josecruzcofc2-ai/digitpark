using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// UI Builder para la escena AgeVerification.unity
    /// Construye la pantalla de verificación de edad para acceder a Cash Battle.
    /// Triumph SDK maneja el flujo de KYC, esta UI solo muestra el estado y lanza el flujo.
    /// </summary>
    public class AgeVerificationUIBuilder : EditorWindow
    {
        #region Colors

        // Premium Color Palette
        private static readonly Color GOLD_PRIMARY = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GOLD_DARK = new Color(0.85f, 0.65f, 0.13f, 1f);
        private static readonly Color GOLD_LIGHT = new Color(1f, 0.93f, 0.55f, 1f);
        private static readonly Color AMBER = new Color(1f, 0.75f, 0f, 1f);

        private static readonly Color BG_DARK = new Color(0.08f, 0.06f, 0.12f, 1f);
        private static readonly Color CARD_BG = new Color(0.12f, 0.1f, 0.15f, 0.95f);
        private static readonly Color CARD_BORDER = new Color(0.85f, 0.65f, 0.13f, 0.6f);

        private static readonly Color TEXT_PRIMARY = new Color(1f, 1f, 1f, 1f);
        private static readonly Color TEXT_GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.7f, 0.7f, 0.7f, 1f);

        private static readonly Color BUTTON_GOLD = new Color(0.85f, 0.65f, 0.13f, 1f);
        private static readonly Color CYAN_ACCENT = new Color(0f, 0.9f, 1f, 1f);

        private static readonly Color SUCCESS_GREEN = new Color(0.3f, 1f, 0.5f, 1f);
        private static readonly Color ERROR_RED = new Color(1f, 0.4f, 0.4f, 1f);

        #endregion

        #region Menu Items

        [MenuItem("DigitPark/UI Builders/Auth/Age Verification", false, 100)]
        public static void ShowWindow()
        {
            GetWindow<AgeVerificationUIBuilder>("Age Verification Builder");
        }

        #endregion

        private void OnGUI()
        {
            GUILayout.Label("Age Verification UI Builder", EditorStyles.boldLabel);
            GUILayout.Label("Pantalla de verificación de edad (18+)", EditorStyles.miniLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Construye la UI para AgeVerification.unity:\n\n" +
                "- Icono premium de verificación 18+\n" +
                "- Título y descripción\n" +
                "- Indicadores de estado (loading, success, error)\n" +
                "- Botón 'Verificar mi Edad' (lanza Triumph)\n" +
                "- Texto legal pequeño",
                MessageType.Info);

            EditorGUILayout.Space(10);

            GUI.backgroundColor = GOLD_PRIMARY;
            if (GUILayout.Button("CONSTRUIR UI COMPLETA", GUILayout.Height(40)))
            {
                BuildAgeVerificationUI();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(15);
            GUILayout.Label("Construcción por Secciones:", EditorStyles.boldLabel);

            if (GUILayout.Button("Solo Background Premium", GUILayout.Height(28)))
            {
                BuildBackgroundOnly();
            }

            if (GUILayout.Button("Solo Verification Card", GUILayout.Height(28)))
            {
                BuildVerificationCardOnly();
            }

            if (GUILayout.Button("Solo Status Indicators", GUILayout.Height(28)))
            {
                BuildStatusIndicatorsOnly();
            }
        }

        #region Build Methods

        private static void BuildAgeVerificationUI()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontró Canvas. Abre la escena AgeVerification primero.", "OK");
                return;
            }

            if (EditorUtility.DisplayDialog("Reconstruir UI?",
                "Esto reconstruirá la UI de Age Verification.\n\nLos elementos existentes serán reemplazados.\n\n¿Continuar?",
                "Sí, Construir", "Cancelar"))
            {
                BuildAllElements(canvas);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

                EditorUtility.DisplayDialog("Éxito",
                    "Age Verification UI creada!\n\n" +
                    "Recuerda:\n" +
                    "1. Asignar referencias en AgeVerificationManager\n" +
                    "2. Colocar el icono en: Art/Icons/CashBattle/UI/VerificationIcon.png\n" +
                    "3. Guardar la escena",
                    "OK");
            }
        }

        private static void BuildBackgroundOnly()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            Transform oldBg = canvas.transform.Find("Background");
            if (oldBg != null) DestroyImmediate(oldBg.gameObject);

            CreatePremiumBackground(canvas.transform);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private static void BuildVerificationCardOnly()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            Transform safeArea = canvas.transform.Find("SafeArea");
            if (safeArea == null)
            {
                Debug.LogError("SafeArea no encontrado. Construye la UI completa primero.");
                return;
            }

            Transform oldCard = safeArea.Find("VerificationCard");
            if (oldCard != null) DestroyImmediate(oldCard.gameObject);

            CreateVerificationCard(safeArea);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private static void BuildStatusIndicatorsOnly()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            Transform card = FindDeep(canvas.transform, "VerificationCard");
            if (card == null)
            {
                Debug.LogError("VerificationCard no encontrado. Construye la UI completa primero.");
                return;
            }

            Transform oldIndicators = card.Find("StatusIndicators");
            if (oldIndicators != null) DestroyImmediate(oldIndicators.gameObject);

            CreateStatusIndicators(card);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private static void BuildAllElements(Canvas canvas)
        {
            Transform canvasTransform = canvas.transform;

            // Limpiar elementos existentes
            CleanupOldElements(canvasTransform);

            // 1. Background Premium
            CreatePremiumBackground(canvasTransform);

            // 2. Safe Area
            GameObject safeArea = CreateSafeArea(canvasTransform);

            // 3. Header con botón back y título
            CreateHeader(safeArea.transform);

            // 4. Verification Card (contenido principal)
            CreateVerificationCard(safeArea.transform);

            Debug.Log("[AgeVerificationUIBuilder] UI construida exitosamente!");
        }

        private static void CleanupOldElements(Transform parent)
        {
            string[] toDestroy = { "Background", "SafeArea", "Header", "VerificationCard", "VerificationPanel" };

            foreach (string name in toDestroy)
            {
                Transform existing = parent.Find(name);
                if (existing != null)
                {
                    DestroyImmediate(existing.gameObject);
                }
            }
        }

        #endregion

        #region Background

        private static void CreatePremiumBackground(Transform parent)
        {
            GameObject bgContainer = new GameObject("Background");
            bgContainer.transform.SetParent(parent, false);
            bgContainer.transform.SetAsFirstSibling();

            RectTransform bgRT = bgContainer.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;

            // Base dark layer
            Image baseImg = bgContainer.AddComponent<Image>();
            baseImg.color = BG_DARK;

            // Vignette overlay (bordes más oscuros)
            GameObject vignette = new GameObject("Vignette");
            vignette.transform.SetParent(bgContainer.transform, false);

            RectTransform vignetteRT = vignette.AddComponent<RectTransform>();
            vignetteRT.anchorMin = Vector2.zero;
            vignetteRT.anchorMax = Vector2.one;
            vignetteRT.sizeDelta = Vector2.zero;

            Image vignetteImg = vignette.AddComponent<Image>();
            vignetteImg.color = new Color(0, 0, 0, 0.3f);

            // Subtle gold accent at top
            GameObject goldAccent = new GameObject("GoldAccent");
            goldAccent.transform.SetParent(bgContainer.transform, false);

            RectTransform accentRT = goldAccent.AddComponent<RectTransform>();
            accentRT.anchorMin = new Vector2(0, 0.85f);
            accentRT.anchorMax = new Vector2(1, 1);
            accentRT.sizeDelta = Vector2.zero;

            Image accentImg = goldAccent.AddComponent<Image>();
            accentImg.color = new Color(GOLD_DARK.r, GOLD_DARK.g, GOLD_DARK.b, 0.05f);
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

            // Add SafeArea component if exists
            var safeAreaComponent = System.Type.GetType("DigitPark.UI.SafeArea, Assembly-CSharp");
            if (safeAreaComponent != null)
            {
                safeArea.AddComponent(safeAreaComponent);
            }

            return safeArea;
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
            rt.sizeDelta = new Vector2(0, 120);
            rt.anchoredPosition = Vector2.zero;

            // Header background
            Image headerBg = header.AddComponent<Image>();
            headerBg.color = new Color(0, 0, 0, 0.3f);

            // Back Button
            CreateBackButton(header.transform);

            // Title "Cash Battle"
            CreateHeaderTitle(header.transform);

            // Balance display
            CreateBalanceDisplay(header.transform);
        }

        private static void CreateBackButton(Transform parent)
        {
            GameObject btnObj = new GameObject("BackButton");
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 0.5f);
            rt.sizeDelta = new Vector2(80, 80);
            rt.anchoredPosition = new Vector2(20, 0);

            Button btn = btnObj.AddComponent<Button>();
            Image btnBg = btnObj.AddComponent<Image>();
            btnBg.color = new Color(1, 1, 1, 0); // Transparent

            // Arrow icon (texto por ahora, reemplazar con sprite)
            GameObject arrow = new GameObject("Arrow");
            arrow.transform.SetParent(btnObj.transform, false);

            RectTransform arrowRT = arrow.AddComponent<RectTransform>();
            arrowRT.anchorMin = Vector2.zero;
            arrowRT.anchorMax = Vector2.one;
            arrowRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI arrowText = arrow.AddComponent<TextMeshProUGUI>();
            arrowText.text = "<";
            arrowText.fontSize = 48;
            arrowText.color = TEXT_PRIMARY;
            arrowText.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateHeaderTitle(Transform parent)
        {
            GameObject titleObj = new GameObject("HeaderTitle");
            titleObj.transform.SetParent(parent, false);

            RectTransform rt = titleObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(400, 60);
            rt.anchoredPosition = Vector2.zero;

            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.text = "Cash Battle";
            title.fontSize = 36;
            title.color = TEXT_GOLD;
            title.alignment = TextAlignmentOptions.Center;
            title.fontStyle = FontStyles.Bold;
        }

        private static void CreateBalanceDisplay(Transform parent)
        {
            GameObject balanceObj = new GameObject("BalanceDisplay");
            balanceObj.transform.SetParent(parent, false);

            RectTransform rt = balanceObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(1, 0.5f);
            rt.sizeDelta = new Vector2(150, 50);
            rt.anchoredPosition = new Vector2(-20, 0);

            // Balance background
            Image bg = balanceObj.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.5f);

            // Outline
            Outline outline = balanceObj.AddComponent<Outline>();
            outline.effectColor = GOLD_DARK;
            outline.effectDistance = new Vector2(1, -1);

            // Balance text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(balanceObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI balanceText = textObj.AddComponent<TextMeshProUGUI>();
            balanceText.text = "$ 0.00";
            balanceText.fontSize = 24;
            balanceText.color = SUCCESS_GREEN;
            balanceText.alignment = TextAlignmentOptions.Center;
            balanceText.fontStyle = FontStyles.Bold;
        }

        #endregion

        #region Verification Card

        private static void CreateVerificationCard(Transform parent)
        {
            GameObject card = new GameObject("VerificationCard");
            card.transform.SetParent(parent, false);

            RectTransform rt = card.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(700, 680);
            rt.anchoredPosition = new Vector2(0, -30);

            // Card background with border
            Image cardBg = card.AddComponent<Image>();
            cardBg.color = CARD_BG;

            // Gold border outline
            Outline cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = CARD_BORDER;
            cardOutline.effectDistance = new Vector2(3, -3);

            // Second outline for glow effect
            Shadow cardGlow = card.AddComponent<Shadow>();
            cardGlow.effectColor = new Color(GOLD_PRIMARY.r, GOLD_PRIMARY.g, GOLD_PRIMARY.b, 0.3f);
            cardGlow.effectDistance = new Vector2(0, 0);

            // Create card contents
            CreateVerificationIcon(card.transform);
            CreateVerificationTitle(card.transform);
            CreateVerificationDescription(card.transform);
            CreateStatusIndicators(card.transform);
            CreateStatusText(card.transform);
            CreateVerifyButton(card.transform);
            CreateLegalNote(card.transform);
        }

        private static void CreateVerificationIcon(Transform parent)
        {
            GameObject iconContainer = new GameObject("IconContainer");
            iconContainer.transform.SetParent(parent, false);

            RectTransform rt = iconContainer.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(160, 160);
            rt.anchoredPosition = new Vector2(0, -30);

            // Icon image (cargar desde Assets)
            Image iconImg = iconContainer.AddComponent<Image>();
            iconImg.color = Color.white;
            iconImg.preserveAspect = true;

            // Intentar cargar el icono
            string iconPath = "Assets/_Project/Art/Icons/CashBattle/UI/VerificationIcon.png";
            Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);

            if (iconSprite != null)
            {
                iconImg.sprite = iconSprite;
            }
            else
            {
                // Placeholder: cuadrado dorado con "18+"
                iconImg.color = AMBER;

                GameObject placeholder = new GameObject("PlaceholderText");
                placeholder.transform.SetParent(iconContainer.transform, false);

                RectTransform placeholderRT = placeholder.AddComponent<RectTransform>();
                placeholderRT.anchorMin = Vector2.zero;
                placeholderRT.anchorMax = Vector2.one;
                placeholderRT.sizeDelta = Vector2.zero;

                TextMeshProUGUI placeholderText = placeholder.AddComponent<TextMeshProUGUI>();
                placeholderText.text = "18+";
                placeholderText.fontSize = 56;
                placeholderText.color = BG_DARK;
                placeholderText.alignment = TextAlignmentOptions.Center;
                placeholderText.fontStyle = FontStyles.Bold;

                Debug.LogWarning($"[AgeVerificationUIBuilder] Icono no encontrado en: {iconPath}\nUsando placeholder. Coloca tu icono premium ahí.");
            }
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
            rt.anchoredPosition = new Vector2(0, -210);

            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.text = "Verificación de Edad Requerida";
            title.fontSize = 38;
            title.color = TEXT_GOLD;
            title.alignment = TextAlignmentOptions.Center;
            title.fontStyle = FontStyles.Bold;

            // Sombra para el texto
            Shadow titleShadow = titleObj.AddComponent<Shadow>();
            titleShadow.effectColor = new Color(0, 0, 0, 0.5f);
            titleShadow.effectDistance = new Vector2(2, -2);
        }

        private static void CreateVerificationDescription(Transform parent)
        {
            GameObject descObj = new GameObject("VerificationDescription");
            descObj.transform.SetParent(parent, false);

            RectTransform rt = descObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(620, 120);
            rt.anchoredPosition = new Vector2(0, -290);

            TextMeshProUGUI desc = descObj.AddComponent<TextMeshProUGUI>();
            desc.text = "Las competencias con dinero real requieren que seas mayor de 18 años.\n\nDeberás verificar tu identidad para continuar.";
            desc.fontSize = 26;
            desc.color = TEXT_SECONDARY;
            desc.alignment = TextAlignmentOptions.Center;
            desc.enableWordWrapping = true;
        }

        private static void CreateStatusIndicators(Transform parent)
        {
            GameObject indicators = new GameObject("StatusIndicators");
            indicators.transform.SetParent(parent, false);

            RectTransform rt = indicators.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(100, 100);
            rt.anchoredPosition = new Vector2(0, -420);

            // Loading Indicator (spinner)
            GameObject loading = new GameObject("LoadingIndicator");
            loading.transform.SetParent(indicators.transform, false);
            loading.SetActive(false);

            RectTransform loadingRT = loading.AddComponent<RectTransform>();
            loadingRT.anchorMin = Vector2.zero;
            loadingRT.anchorMax = Vector2.one;
            loadingRT.sizeDelta = Vector2.zero;

            Image loadingImg = loading.AddComponent<Image>();
            loadingImg.color = GOLD_PRIMARY;
            // TODO: Agregar animación de rotación via script

            // Success Icon (checkmark)
            GameObject success = new GameObject("SuccessIcon");
            success.transform.SetParent(indicators.transform, false);
            success.SetActive(false);

            RectTransform successRT = success.AddComponent<RectTransform>();
            successRT.anchorMin = Vector2.zero;
            successRT.anchorMax = Vector2.one;
            successRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI successText = success.AddComponent<TextMeshProUGUI>();
            successText.text = "✓";
            successText.fontSize = 72;
            successText.color = SUCCESS_GREEN;
            successText.alignment = TextAlignmentOptions.Center;

            // Error Icon (X)
            GameObject error = new GameObject("ErrorIcon");
            error.transform.SetParent(indicators.transform, false);
            error.SetActive(false);

            RectTransform errorRT = error.AddComponent<RectTransform>();
            errorRT.anchorMin = Vector2.zero;
            errorRT.anchorMax = Vector2.one;
            errorRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI errorText = error.AddComponent<TextMeshProUGUI>();
            errorText.text = "✗";
            errorText.fontSize = 72;
            errorText.color = ERROR_RED;
            errorText.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateStatusText(Transform parent)
        {
            GameObject statusObj = new GameObject("StatusText");
            statusObj.transform.SetParent(parent, false);

            RectTransform rt = statusObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(600, 50);
            rt.anchoredPosition = new Vector2(0, -530);

            TextMeshProUGUI status = statusObj.AddComponent<TextMeshProUGUI>();
            status.text = "";
            status.fontSize = 24;
            status.color = TEXT_GOLD;
            status.alignment = TextAlignmentOptions.Center;
            status.fontStyle = FontStyles.Bold;
        }

        private static void CreateVerifyButton(Transform parent)
        {
            GameObject btnObj = new GameObject("VerifyButton");
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0);
            rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(480, 90);
            rt.anchoredPosition = new Vector2(0, 80);

            // Button background
            Image bg = btnObj.AddComponent<Image>();
            bg.color = BUTTON_GOLD;

            // Button component
            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = BUTTON_GOLD;
            colors.highlightedColor = GOLD_LIGHT;
            colors.pressedColor = GOLD_DARK;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            btn.colors = colors;

            // Glow outline
            Outline glow = btnObj.AddComponent<Outline>();
            glow.effectColor = new Color(1f, 0.8f, 0.3f, 0.5f);
            glow.effectDistance = new Vector2(4, -4);

            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.text = "Verificar mi Edad";
            btnText.fontSize = 34;
            btnText.color = BG_DARK;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.fontStyle = FontStyles.Bold;
        }

        private static void CreateLegalNote(Transform parent)
        {
            GameObject legalObj = new GameObject("LegalNote");
            legalObj.transform.SetParent(parent, false);

            RectTransform rt = legalObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0);
            rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(600, 60);
            rt.anchoredPosition = new Vector2(0, 15);

            TextMeshProUGUI legal = legalObj.AddComponent<TextMeshProUGUI>();
            legal.text = "Al continuar, aceptas los <u>Términos de Servicio</u>\ny la <u>Política de Privacidad</u> de Triumph.";
            legal.fontSize = 18;
            legal.color = new Color(TEXT_SECONDARY.r, TEXT_SECONDARY.g, TEXT_SECONDARY.b, 0.7f);
            legal.alignment = TextAlignmentOptions.Center;
            legal.enableWordWrapping = true;
            legal.richText = true;
        }

        #endregion

        #region Utility

        private static Transform FindDeep(Transform parent, string name)
        {
            if (parent.name == name) return parent;

            foreach (Transform child in parent)
            {
                Transform found = FindDeep(child, name);
                if (found != null) return found;
            }

            return null;
        }

        #endregion
    }
}
