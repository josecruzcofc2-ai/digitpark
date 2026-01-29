using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Settings UI Builder - Diseño con Cards profesional
    /// Estilo Neon consistente con el resto de la app
    /// </summary>
    public class SettingsUIBuilder : EditorWindow
    {
        #region Colors - Neon Theme

        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.5f, 1f);
        private static readonly Color CYAN_GLOW = new Color(0f, 0.85f, 1f, 0.6f);

        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);

        private static readonly Color DARK_BG = new Color(0.02f, 0.04f, 0.08f, 1f);
        private static readonly Color CARD_BG = new Color(0.06f, 0.08f, 0.12f, 1f);
        private static readonly Color HEADER_BG = new Color(0.04f, 0.06f, 0.1f, 0.98f);
        private static readonly Color ROW_BG = new Color(0.08f, 0.1f, 0.14f, 0.8f);

        private static readonly Color TEXT_WHITE = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.65f, 0.7f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.1f, 0.1f, 0.1f, 1f);

        private static readonly Color RED_DANGER = new Color(1f, 0.3f, 0.3f, 1f);
        private static readonly Color RED_DARK = new Color(0.6f, 0.15f, 0.15f, 1f);

        #endregion

        #region Layout Constants

        private const float HEADER_HEIGHT = 100f;
        private const float CARD_SPACING = 20f;
        private const float CARD_PADDING = 20f;
        private const float ROW_HEIGHT = 60f;
        private const float SECTION_TITLE_HEIGHT = 45f;

        #endregion

        [MenuItem("DigitPark/UI Builders/Core/Settings", false, 152)]
        public static void ShowWindow()
        {
            GetWindow<SettingsUIBuilder>("Settings Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Settings UI Builder", EditorStyles.boldLabel);
            GUILayout.Label("Diseño con Cards - Estilo Neon", EditorStyles.miniLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Estructura:\n\n" +
                "1. Header (Back, Title, PRO badge)\n" +
                "2. Card: Cuenta (Username, Logout, Delete)\n" +
                "3. Card: Audio (Music, Effects sliders)\n" +
                "4. Card: Apariencia (Language, Theme)\n" +
                "5. Card: Legal (Privacy, Terms, Contact)\n" +
                "6. Footer (Version)",
                MessageType.Info);

            GUILayout.Space(15);

            GUI.backgroundColor = CYAN_NEON;
            if (GUILayout.Button("RECONSTRUIR SETTINGS COMPLETO", GUILayout.Height(50)))
            {
                RebuildSettings();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(15);
            GUILayout.Label("Componentes Individuales:", EditorStyles.boldLabel);

            if (GUILayout.Button("1. Header", GUILayout.Height(28)))
                CreateHeader();
            if (GUILayout.Button("2. Card: Cuenta", GUILayout.Height(28)))
                CreateAccountCard();
            if (GUILayout.Button("3. Card: Audio", GUILayout.Height(28)))
                CreateAudioCard();
            if (GUILayout.Button("4. Card: Apariencia", GUILayout.Height(28)))
                CreateAppearanceCard();
            if (GUILayout.Button("5. Card: Legal", GUILayout.Height(28)))
                CreateLegalCard();
            if (GUILayout.Button("6. Footer", GUILayout.Height(28)))
                CreateFooter();

            GUILayout.Space(15);

            GUI.backgroundColor = new Color(1f, 0.8f, 0.3f);
            if (GUILayout.Button("Ver Referencias para SettingsManager", GUILayout.Height(30)))
            {
                ShowReferenceGuide();
            }
            GUI.backgroundColor = Color.white;
        }

        private static void ShowReferenceGuide()
        {
            string guide = @"
=== REFERENCIAS PARA SETTINGSMANAGER ===

Después de reconstruir la UI, conecta estos elementos en el Inspector:

HEADER:
- backButton → Header/BackButton

ACCOUNT CARD:
- changeNameButton → AccountCard/CardContent/UsernameRow
- logoutButton → AccountCard/CardContent/LogoutRow
- deleteAccountButton → AccountCard/CardContent/DeleteRow

AUDIO CARD:
- soundVolumeSlider → AudioCard/CardContent/MusicRow/Slider
- soundValueText → AudioCard/CardContent/MusicRow/LabelRow/Value
- effectsVolumeSlider → AudioCard/CardContent/EffectsRow/Slider
- effectsValueText → AudioCard/CardContent/EffectsRow/LabelRow/Value

APPEARANCE CARD:
- languageDropdown → AppearanceCard/CardContent/LanguageRow/Dropdown
- themeDropdown → AppearanceCard/CardContent/ThemeRow/Dropdown

LEGAL CARD:
- privacyButton → LegalCard/CardContent/PrivacyRow
- termsButton → LegalCard/CardContent/TermsRow
- (contactButton) → LegalCard/CardContent/ContactRow

Tip: Usa el PrefabReferenceAssigner para automatizar!
";
            Debug.Log(guide);
            EditorUtility.DisplayDialog("Guía de Referencias",
                "Las referencias necesarias se han mostrado en la Console.\n\n" +
                "Revisa la ventana Console para ver la guía completa.", "OK");
        }

        #region Main Rebuild

        private static void RebuildSettings()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[SettingsUI] No se encontró Canvas en la escena");
                return;
            }

            CleanOldElements(canvas.transform);

            CreateBackground(canvas.transform);
            CreateHeader();
            CreateScrollView(canvas.transform);
            CreateAccountCard();
            CreateAudioCard();
            CreateAppearanceCard();
            CreateLegalCard();
            CreateFooter();

            Debug.Log("[SettingsUI] Settings REDISEÑADO exitosamente!");
            EditorUtility.SetDirty(canvas.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        }

        private static void CleanOldElements(Transform canvasTransform)
        {
            string[] oldElements = {
                "Background", "Header", "ScrollView", "Content",
                "AccountCard", "AudioCard", "AppearanceCard", "LegalCard",
                "Footer", "SettingsPanel", "MainPanel"
            };

            foreach (string name in oldElements)
            {
                Transform element = canvasTransform.Find(name);
                if (element != null)
                {
                    DestroyImmediate(element.gameObject);
                }
            }
        }

        #endregion

        #region Background

        private static void CreateBackground(Transform parent)
        {
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(parent, false);
            bg.transform.SetAsFirstSibling();

            RectTransform rt = bg.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image img = bg.AddComponent<Image>();
            img.color = DARK_BG;
        }

        #endregion

        #region Header

        private static void CreateHeader()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            GameObject header = FindOrCreate(canvas.transform, "Header");
            header.transform.SetSiblingIndex(1);

            RectTransform rt = GetOrAdd<RectTransform>(header);
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0, HEADER_HEIGHT);

            Image bg = GetOrAdd<Image>(header);
            bg.color = HEADER_BG;

            // Back Button
            GameObject backBtn = FindOrCreate(header.transform, "BackButton");
            RectTransform backRT = GetOrAdd<RectTransform>(backBtn);
            backRT.anchorMin = new Vector2(0, 0.5f);
            backRT.anchorMax = new Vector2(0, 0.5f);
            backRT.pivot = new Vector2(0, 0.5f);
            backRT.anchoredPosition = new Vector2(20, 0);
            backRT.sizeDelta = new Vector2(50, 50);

            Image backBg = GetOrAdd<Image>(backBtn);
            backBg.color = new Color(1, 1, 1, 0.08f);

            Button backButton = GetOrAdd<Button>(backBtn);
            backButton.targetGraphic = backBg;

            // Back Arrow
            GameObject backArrow = FindOrCreate(backBtn.transform, "Arrow");
            RectTransform arrowRT = GetOrAdd<RectTransform>(backArrow);
            arrowRT.anchorMin = Vector2.zero;
            arrowRT.anchorMax = Vector2.one;
            arrowRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI arrowTMP = GetOrAdd<TextMeshProUGUI>(backArrow);
            arrowTMP.text = "<";
            arrowTMP.fontSize = 32;
            arrowTMP.color = CYAN_NEON;
            arrowTMP.fontStyle = FontStyles.Bold;
            arrowTMP.alignment = TextAlignmentOptions.Center;

            // Title
            GameObject title = FindOrCreate(header.transform, "Title");
            RectTransform titleRT = GetOrAdd<RectTransform>(title);
            titleRT.anchorMin = new Vector2(0.5f, 0.5f);
            titleRT.anchorMax = new Vector2(0.5f, 0.5f);
            titleRT.sizeDelta = new Vector2(250, 50);

            TextMeshProUGUI titleTMP = GetOrAdd<TextMeshProUGUI>(title);
            titleTMP.text = "CONFIGURACIÓN";
            titleTMP.fontSize = 28;
            titleTMP.color = CYAN_NEON;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[SettingsUI] Header creado");
        }

        #endregion

        #region ScrollView

        private static void CreateScrollView(Transform canvasTransform)
        {
            GameObject scrollView = FindOrCreate(canvasTransform, "ScrollView");

            RectTransform scrollRT = GetOrAdd<RectTransform>(scrollView);
            scrollRT.anchorMin = new Vector2(0, 0);
            scrollRT.anchorMax = new Vector2(1, 1);
            scrollRT.offsetMin = new Vector2(20, 60); // Padding bottom para footer
            scrollRT.offsetMax = new Vector2(-20, -HEADER_HEIGHT - 10);

            ScrollRect scroll = GetOrAdd<ScrollRect>(scrollView);
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 30f;

            // Viewport
            GameObject viewport = FindOrCreate(scrollView.transform, "Viewport");
            RectTransform vpRT = GetOrAdd<RectTransform>(viewport);
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.sizeDelta = Vector2.zero;
            vpRT.pivot = new Vector2(0.5f, 1);

            RectMask2D mask = GetOrAdd<RectMask2D>(viewport);

            // Content
            GameObject content = FindOrCreate(viewport.transform, "Content");
            RectTransform contentRT = GetOrAdd<RectTransform>(content);
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup vlg = GetOrAdd<VerticalLayoutGroup>(content);
            vlg.spacing = CARD_SPACING;
            vlg.padding = new RectOffset(0, 0, 10, 20);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            ContentSizeFitter csf = GetOrAdd<ContentSizeFitter>(content);
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = vpRT;
            scroll.content = contentRT;
        }

        private static Transform GetContentParent()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return null;

            Transform scrollView = canvas.transform.Find("ScrollView");
            if (scrollView == null) return canvas.transform;

            Transform viewport = scrollView.Find("Viewport");
            if (viewport == null) return canvas.transform;

            Transform content = viewport.Find("Content");
            return content ?? canvas.transform;
        }

        #endregion

        #region Account Card

        private static void CreateAccountCard()
        {
            Transform parent = GetContentParent();
            if (parent == null) return;

            GameObject card = CreateCard(parent, "AccountCard", "CUENTA", CYAN_NEON);

            Transform cardContent = card.transform.Find("CardContent");
            if (cardContent == null) return;

            // Cambiar Username
            CreateSettingsRow(cardContent, "UsernameRow", "Cambiar Nombre de Usuario", "user", CYAN_NEON, true);

            // Cerrar Sesión
            CreateSettingsRow(cardContent, "LogoutRow", "Cerrar Sesión", "logout", CYAN_NEON, true);

            // Eliminar Cuenta (rojo)
            CreateSettingsRow(cardContent, "DeleteRow", "Eliminar Cuenta", "trash", RED_DANGER, true);

            Debug.Log("[SettingsUI] Account Card creado");
        }

        #endregion

        #region Audio Card

        private static void CreateAudioCard()
        {
            Transform parent = GetContentParent();
            if (parent == null) return;

            GameObject card = CreateCard(parent, "AudioCard", "AUDIO", CYAN_NEON);

            Transform cardContent = card.transform.Find("CardContent");
            if (cardContent == null) return;

            // Volumen Música
            CreateSliderRow(cardContent, "MusicRow", "Volumen de Música", 0.5f);

            // Volumen Efectos
            CreateSliderRow(cardContent, "EffectsRow", "Volumen de Efectos", 0.5f);

            Debug.Log("[SettingsUI] Audio Card creado");
        }

        #endregion

        #region Appearance Card

        private static void CreateAppearanceCard()
        {
            Transform parent = GetContentParent();
            if (parent == null) return;

            GameObject card = CreateCard(parent, "AppearanceCard", "APARIENCIA", CYAN_NEON);

            Transform cardContent = card.transform.Find("CardContent");
            if (cardContent == null) return;

            // Idioma
            CreateDropdownRow(cardContent, "LanguageRow", "Idioma", new[] { "Español", "English", "Português" }, 1);

            // Tema
            CreateDropdownRow(cardContent, "ThemeRow", "Tema", new[] { "Neon Dark", "Neon Light", "Classic" }, 0);

            Debug.Log("[SettingsUI] Appearance Card creado");
        }

        #endregion

        #region Legal Card

        private static void CreateLegalCard()
        {
            Transform parent = GetContentParent();
            if (parent == null) return;

            GameObject card = CreateCard(parent, "LegalCard", "LEGAL", CYAN_NEON);

            Transform cardContent = card.transform.Find("CardContent");
            if (cardContent == null) return;

            // Política de Privacidad
            CreateSettingsRow(cardContent, "PrivacyRow", "Política de Privacidad", "doc", TEXT_SECONDARY, true);

            // Términos de Servicio
            CreateSettingsRow(cardContent, "TermsRow", "Términos de Servicio", "doc", TEXT_SECONDARY, true);

            // Contacto
            CreateSettingsRow(cardContent, "ContactRow", "Contacto / Soporte", "mail", TEXT_SECONDARY, true);

            Debug.Log("[SettingsUI] Legal Card creado");
        }

        #endregion

        #region Footer

        private static void CreateFooter()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            GameObject footer = FindOrCreate(canvas.transform, "Footer");

            RectTransform rt = GetOrAdd<RectTransform>(footer);
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, 15);
            rt.sizeDelta = new Vector2(0, 40);

            TextMeshProUGUI versionTMP = GetOrAdd<TextMeshProUGUI>(footer);
            versionTMP.text = "Versión 1.0.0";
            versionTMP.fontSize = 14;
            versionTMP.color = TEXT_SECONDARY;
            versionTMP.alignment = TextAlignmentOptions.Center;

            Debug.Log("[SettingsUI] Footer creado");
        }

        #endregion

        #region Card Factory

        private static GameObject CreateCard(Transform parent, string name, string title, Color accentColor)
        {
            // Eliminar card existente
            Transform existing = parent.Find(name);
            if (existing != null) DestroyImmediate(existing.gameObject);

            GameObject card = new GameObject(name);
            card.transform.SetParent(parent, false);

            RectTransform rt = card.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 0);

            // Card Background - más elegante
            Image bg = card.AddComponent<Image>();
            bg.color = CARD_BG;

            // Glow border elegante
            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = new Color(accentColor.r, accentColor.g, accentColor.b, 0.5f);
            outline.effectDistance = new Vector2(2f, 2f);

            // Shadow para profundidad
            Shadow shadow = card.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.4f);
            shadow.effectDistance = new Vector2(3, -3);

            VerticalLayoutGroup vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 0;
            vlg.padding = new RectOffset(0, 0, 0, 8);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;

            ContentSizeFitter csf = card.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;

            // === Card Header - Elegante con línea de acento ===
            GameObject header = new GameObject("CardHeader");
            header.transform.SetParent(card.transform, false);

            LayoutElement headerLE = header.AddComponent<LayoutElement>();
            headerLE.preferredHeight = SECTION_TITLE_HEIGHT + 5;
            headerLE.minHeight = SECTION_TITLE_HEIGHT + 5;

            // Header con gradiente sutil
            Image headerBg = header.AddComponent<Image>();
            headerBg.color = new Color(accentColor.r * 0.12f, accentColor.g * 0.12f, accentColor.b * 0.12f, 0.95f);

            // Header content layout
            HorizontalLayoutGroup headerHLG = header.AddComponent<HorizontalLayoutGroup>();
            headerHLG.padding = new RectOffset(18, 18, 8, 8);
            headerHLG.spacing = 12;
            headerHLG.childAlignment = TextAnchor.MiddleLeft;
            headerHLG.childControlWidth = false;
            headerHLG.childControlHeight = true;
            headerHLG.childForceExpandWidth = false;

            // Icon con glow
            GameObject icon = new GameObject("Icon");
            icon.transform.SetParent(header.transform, false);
            LayoutElement iconLE = icon.AddComponent<LayoutElement>();
            iconLE.preferredWidth = 30;
            iconLE.preferredHeight = 30;

            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = accentColor;
            iconImg.preserveAspect = true;

            // Title con estilo gaming
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(header.transform, false);
            LayoutElement titleLE = titleObj.AddComponent<LayoutElement>();
            titleLE.preferredWidth = 200;
            titleLE.flexibleWidth = 1;

            TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
            titleTMP.text = title;
            titleTMP.fontSize = 20;
            titleTMP.color = accentColor;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Left;

            // Línea de acento debajo del header
            GameObject accentLine = new GameObject("AccentLine");
            accentLine.transform.SetParent(card.transform, false);

            LayoutElement lineLE = accentLine.AddComponent<LayoutElement>();
            lineLE.preferredHeight = 2;
            lineLE.minHeight = 2;

            Image lineImg = accentLine.AddComponent<Image>();
            lineImg.color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.6f);

            // Card Content container
            GameObject content = new GameObject("CardContent");
            content.transform.SetParent(card.transform, false);

            VerticalLayoutGroup contentVLG = content.AddComponent<VerticalLayoutGroup>();
            contentVLG.spacing = 1;
            contentVLG.padding = new RectOffset(0, 0, 8, 8);
            contentVLG.childAlignment = TextAnchor.UpperCenter;
            contentVLG.childControlWidth = true;
            contentVLG.childControlHeight = false;
            contentVLG.childForceExpandWidth = true;

            ContentSizeFitter contentCSF = content.AddComponent<ContentSizeFitter>();
            contentCSF.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            LayoutElement contentLE = content.AddComponent<LayoutElement>();
            contentLE.flexibleWidth = 1;

            return card;
        }

        #endregion

        #region Row Factory

        private static void CreateSettingsRow(Transform parent, string name, string label, string iconType, Color color, bool hasArrow)
        {
            GameObject row = new GameObject(name);
            row.transform.SetParent(parent, false);

            LayoutElement le = row.AddComponent<LayoutElement>();
            le.preferredHeight = ROW_HEIGHT;
            le.minHeight = ROW_HEIGHT;

            // Background elegante con hover effect
            Image bg = row.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.07f, 0.1f, 0.9f);

            Button btn = row.AddComponent<Button>();
            btn.targetGraphic = bg;

            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.2f, 1f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.selectedColor = Color.white;
            btn.colors = colors;

            // Horizontal layout
            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(18, 18, 12, 12);
            hlg.spacing = 15;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            // Icon con glow
            GameObject icon = new GameObject("Icon");
            icon.transform.SetParent(row.transform, false);
            LayoutElement iconLE = icon.AddComponent<LayoutElement>();
            iconLE.preferredWidth = 26;
            iconLE.preferredHeight = 26;

            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = color;
            iconImg.preserveAspect = true;

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(row.transform, false);
            LayoutElement labelLE = labelObj.AddComponent<LayoutElement>();
            labelLE.flexibleWidth = 1;
            labelLE.preferredHeight = 30;

            TextMeshProUGUI labelTMP = labelObj.AddComponent<TextMeshProUGUI>();
            labelTMP.text = label;
            labelTMP.fontSize = 18;
            labelTMP.color = color == RED_DANGER ? RED_DANGER : TEXT_WHITE;
            labelTMP.alignment = TextAlignmentOptions.Left;

            // Arrow elegante
            if (hasArrow)
            {
                GameObject arrow = new GameObject("Arrow");
                arrow.transform.SetParent(row.transform, false);
                LayoutElement arrowLE = arrow.AddComponent<LayoutElement>();
                arrowLE.preferredWidth = 28;
                arrowLE.preferredHeight = 28;

                TextMeshProUGUI arrowTMP = arrow.AddComponent<TextMeshProUGUI>();
                arrowTMP.text = ">";
                arrowTMP.fontSize = 24;
                arrowTMP.color = color == RED_DANGER ? RED_DANGER : new Color(color.r, color.g, color.b, 0.6f);
                arrowTMP.fontStyle = FontStyles.Bold;
                arrowTMP.alignment = TextAlignmentOptions.Center;
            }
        }

        private static void CreateSliderRow(Transform parent, string name, string label, float defaultValue)
        {
            GameObject row = new GameObject(name);
            row.transform.SetParent(parent, false);

            LayoutElement le = row.AddComponent<LayoutElement>();
            le.preferredHeight = ROW_HEIGHT + 10;
            le.minHeight = ROW_HEIGHT + 10;

            Image bg = row.AddComponent<Image>();
            bg.color = ROW_BG;

            // Vertical layout para label arriba y slider abajo
            VerticalLayoutGroup vlg = row.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 8, 8);
            vlg.spacing = 5;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;

            // Label row
            GameObject labelRow = new GameObject("LabelRow");
            labelRow.transform.SetParent(row.transform, false);
            LayoutElement labelRowLE = labelRow.AddComponent<LayoutElement>();
            labelRowLE.preferredHeight = 25;

            HorizontalLayoutGroup labelHLG = labelRow.AddComponent<HorizontalLayoutGroup>();
            labelHLG.childAlignment = TextAnchor.MiddleLeft;
            labelHLG.childControlWidth = false;
            labelHLG.childForceExpandWidth = false;

            // Icon
            GameObject icon = new GameObject("Icon");
            icon.transform.SetParent(labelRow.transform, false);
            LayoutElement iconLE = icon.AddComponent<LayoutElement>();
            iconLE.preferredWidth = 22;
            iconLE.preferredHeight = 22;

            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = CYAN_NEON;
            iconImg.preserveAspect = true;

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(labelRow.transform, false);
            LayoutElement labelLE = labelObj.AddComponent<LayoutElement>();
            labelLE.flexibleWidth = 1;

            TextMeshProUGUI labelTMP = labelObj.AddComponent<TextMeshProUGUI>();
            labelTMP.text = "  " + label;
            labelTMP.fontSize = 16;
            labelTMP.color = TEXT_WHITE;
            labelTMP.alignment = TextAlignmentOptions.Left;

            // Value
            GameObject valueObj = new GameObject("Value");
            valueObj.transform.SetParent(labelRow.transform, false);
            LayoutElement valueLE = valueObj.AddComponent<LayoutElement>();
            valueLE.preferredWidth = 50;

            TextMeshProUGUI valueTMP = valueObj.AddComponent<TextMeshProUGUI>();
            valueTMP.text = Mathf.RoundToInt(defaultValue * 100) + "%";
            valueTMP.fontSize = 16;
            valueTMP.color = CYAN_NEON;
            valueTMP.alignment = TextAlignmentOptions.Right;

            // Slider
            GameObject sliderObj = new GameObject("Slider");
            sliderObj.transform.SetParent(row.transform, false);
            LayoutElement sliderLE = sliderObj.AddComponent<LayoutElement>();
            sliderLE.preferredHeight = 30;
            sliderLE.flexibleWidth = 1;

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = defaultValue;

            // Slider Background
            GameObject sliderBg = new GameObject("Background");
            sliderBg.transform.SetParent(sliderObj.transform, false);
            RectTransform bgRT = sliderBg.AddComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0, 0.4f);
            bgRT.anchorMax = new Vector2(1, 0.6f);
            bgRT.sizeDelta = Vector2.zero;

            Image bgImg = sliderBg.AddComponent<Image>();
            bgImg.color = CYAN_DARK;

            // Fill Area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform fillAreaRT = fillArea.AddComponent<RectTransform>();
            fillAreaRT.anchorMin = new Vector2(0, 0.4f);
            fillAreaRT.anchorMax = new Vector2(1, 0.6f);
            fillAreaRT.sizeDelta = Vector2.zero;

            // Fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRT = fill.AddComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.sizeDelta = Vector2.zero;

            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = CYAN_NEON;

            // Handle Area
            GameObject handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(sliderObj.transform, false);
            RectTransform handleAreaRT = handleArea.AddComponent<RectTransform>();
            handleAreaRT.anchorMin = Vector2.zero;
            handleAreaRT.anchorMax = Vector2.one;
            handleAreaRT.sizeDelta = Vector2.zero;

            // Handle
            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            RectTransform handleRT = handle.AddComponent<RectTransform>();
            handleRT.sizeDelta = new Vector2(25, 25);

            Image handleImg = handle.AddComponent<Image>();
            handleImg.color = CYAN_NEON;

            // Configure slider references
            slider.fillRect = fillRT;
            slider.handleRect = handleRT;
            slider.targetGraphic = handleImg;
        }

        private static void CreateDropdownRow(Transform parent, string name, string label, string[] options, int defaultIndex)
        {
            GameObject row = new GameObject(name);
            row.transform.SetParent(parent, false);

            LayoutElement le = row.AddComponent<LayoutElement>();
            le.preferredHeight = ROW_HEIGHT;
            le.minHeight = ROW_HEIGHT;

            Image bg = row.AddComponent<Image>();
            bg.color = ROW_BG;

            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(20, 20, 10, 10);
            hlg.spacing = 15;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            // Icon
            GameObject icon = new GameObject("Icon");
            icon.transform.SetParent(row.transform, false);
            LayoutElement iconLE = icon.AddComponent<LayoutElement>();
            iconLE.preferredWidth = 24;
            iconLE.preferredHeight = 24;

            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = CYAN_NEON;
            iconImg.preserveAspect = true;

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(row.transform, false);
            LayoutElement labelLE = labelObj.AddComponent<LayoutElement>();
            labelLE.flexibleWidth = 1;

            TextMeshProUGUI labelTMP = labelObj.AddComponent<TextMeshProUGUI>();
            labelTMP.text = label;
            labelTMP.fontSize = 18;
            labelTMP.color = TEXT_WHITE;
            labelTMP.alignment = TextAlignmentOptions.Left;

            // Dropdown container
            GameObject dropdownObj = new GameObject("Dropdown");
            dropdownObj.transform.SetParent(row.transform, false);
            LayoutElement dropdownLE = dropdownObj.AddComponent<LayoutElement>();
            dropdownLE.preferredWidth = 150;
            dropdownLE.preferredHeight = 40;

            Image dropdownBg = dropdownObj.AddComponent<Image>();
            dropdownBg.color = CARD_BG;

            Outline dropdownOutline = dropdownObj.AddComponent<Outline>();
            dropdownOutline.effectColor = CYAN_DARK;
            dropdownOutline.effectDistance = new Vector2(1, 1);

            TMP_Dropdown dropdown = dropdownObj.AddComponent<TMP_Dropdown>();

            // Dropdown Label
            GameObject dropdownLabel = new GameObject("Label");
            dropdownLabel.transform.SetParent(dropdownObj.transform, false);
            RectTransform labelRT = dropdownLabel.AddComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = new Vector2(10, 0);
            labelRT.offsetMax = new Vector2(-30, 0);

            TextMeshProUGUI dropdownTMP = dropdownLabel.AddComponent<TextMeshProUGUI>();
            dropdownTMP.text = options[defaultIndex];
            dropdownTMP.fontSize = 16;
            dropdownTMP.color = CYAN_NEON;
            dropdownTMP.alignment = TextAlignmentOptions.Left;

            // Arrow
            GameObject arrow = new GameObject("Arrow");
            arrow.transform.SetParent(dropdownObj.transform, false);
            RectTransform arrowRT = arrow.AddComponent<RectTransform>();
            arrowRT.anchorMin = new Vector2(1, 0.5f);
            arrowRT.anchorMax = new Vector2(1, 0.5f);
            arrowRT.pivot = new Vector2(1, 0.5f);
            arrowRT.anchoredPosition = new Vector2(-10, 0);
            arrowRT.sizeDelta = new Vector2(20, 20);

            TextMeshProUGUI arrowTMP = arrow.AddComponent<TextMeshProUGUI>();
            arrowTMP.text = "▼";
            arrowTMP.fontSize = 14;
            arrowTMP.color = CYAN_NEON;
            arrowTMP.alignment = TextAlignmentOptions.Center;

            // Configure dropdown
            dropdown.captionText = dropdownTMP;
            dropdown.ClearOptions();
            dropdown.AddOptions(new System.Collections.Generic.List<string>(options));
            dropdown.value = defaultIndex;
        }

        #endregion

        #region Helpers

        private static GameObject FindOrCreate(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null) return existing.gameObject;

            GameObject newObj = new GameObject(name);
            newObj.transform.SetParent(parent, false);
            return newObj;
        }

        private static T GetOrAdd<T>(GameObject obj) where T : Component
        {
            T component = obj.GetComponent<T>();
            if (component == null) component = obj.AddComponent<T>();
            return component;
        }

        #endregion
    }
}
