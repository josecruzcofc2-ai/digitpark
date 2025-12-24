using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Managers;
using DigitPark.Localization;

namespace DigitPark.UI.Panels
{
    /// <summary>
    /// Panel visual que muestra los 6 temas disponibles con Styles PRO.
    /// Diseño: 6 bloques de preview (2 filas x 3 columnas) con botones de compra.
    /// </summary>
    public class StylesProPromptPanel : MonoBehaviour
    {
        private GameObject blocker;
        private GameObject panel;
        private Button buyButton;
        private Button cancelButton;
        private Button closeButton;

        // Colores
        private readonly Color goldColor = new Color(1f, 0.84f, 0f, 1f);
        private readonly Color neonCyan = new Color(0f, 0.96f, 1f, 1f);
        private readonly Color redColor = new Color(0.85f, 0.2f, 0.2f, 1f);
        private readonly Color darkBg = new Color(0.02f, 0.05f, 0.1f, 0.98f);
        private readonly Color panelBg = new Color(0.03f, 0.07f, 0.14f, 0.98f);

        // Datos de los temas
        private readonly ThemePreviewData[] themes = new ThemePreviewData[]
        {
            new ThemePreviewData("Neon Dark", new Color(0f, 0.96f, 1f, 1f), new Color(0.02f, 0.05f, 0.1f, 1f)),
            new ThemePreviewData("Clean Light", new Color(0f, 0.48f, 1f, 1f), new Color(0.96f, 0.96f, 0.97f, 1f)),
            new ThemePreviewData("Cyberpunk", new Color(1f, 0f, 0.5f, 1f), new Color(0.08f, 0.02f, 0.15f, 1f)),
            new ThemePreviewData("Ocean", new Color(0f, 0.8f, 0.8f, 1f), new Color(0.02f, 0.1f, 0.15f, 1f)),
            new ThemePreviewData("Retro Arcade", new Color(1f, 0.84f, 0f, 1f), new Color(0.1f, 0.05f, 0.15f, 1f)),
            new ThemePreviewData("Volcano", new Color(1f, 0.3f, 0.1f, 1f), new Color(0.12f, 0.03f, 0.02f, 1f))
        };

        private struct ThemePreviewData
        {
            public string name;
            public Color accentColor;
            public Color bgColor;

            public ThemePreviewData(string name, Color accent, Color bg)
            {
                this.name = name;
                this.accentColor = accent;
                this.bgColor = bg;
            }
        }

        /// <summary>
        /// Crea y muestra el panel de Styles PRO
        /// </summary>
        public static StylesProPromptPanel CreateAndShow()
        {
            // Cerrar cualquier dropdown abierto
            CloseAllDropdowns();

            // Crear canvas independiente
            GameObject canvasObj = new GameObject("StylesProPromptCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10000;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // Crear el panel
            GameObject panelObj = new GameObject("StylesProPrompt");
            panelObj.transform.SetParent(canvasObj.transform, false);

            StylesProPromptPanel promptPanel = panelObj.AddComponent<StylesProPromptPanel>();
            promptPanel.CreateUI();

            Debug.Log("[StylesProPromptPanel] Panel creado y mostrado");

            return promptPanel;
        }

        private static void CloseAllDropdowns()
        {
            var dropdowns = FindObjectsOfType<TMP_Dropdown>();
            foreach (var dropdown in dropdowns)
            {
                dropdown.Hide();
            }
        }

        private void CreateUI()
        {
            RectTransform rt = gameObject.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            blocker = CreateBlocker();
            panel = CreateMainPanel();
        }

        private GameObject CreateBlocker()
        {
            GameObject obj = new GameObject("Blocker");
            obj.transform.SetParent(transform, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image img = obj.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.9f);
            img.raycastTarget = true;

            return obj;
        }

        private GameObject CreateMainPanel()
        {
            GameObject obj = new GameObject("Panel");
            obj.transform.SetParent(transform, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(580, 520); // Más ancho, menos alto

            Image bg = obj.AddComponent<Image>();
            bg.color = panelBg;

            // Borde dorado
            Outline outline = obj.AddComponent<Outline>();
            outline.effectColor = new Color(goldColor.r, goldColor.g, goldColor.b, 0.7f);
            outline.effectDistance = new Vector2(3, 3);

            // Crear contenido sin LayoutGroup para control total
            CreateTitle(obj.transform);
            CreateThemesGrid(obj.transform);
            CreatePrice(obj.transform);
            CreateButtons(obj.transform);
            CreateCloseButton(obj.transform);

            return obj;
        }

        private void CreateTitle(Transform parent)
        {
            GameObject obj = new GameObject("Title");
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -15);
            rt.sizeDelta = new Vector2(400, 50);

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = "STYLES PRO";
            tmp.fontSize = 38;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = goldColor;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        private void CreateThemesGrid(Transform parent)
        {
            GameObject container = new GameObject("ThemesGrid");
            container.transform.SetParent(parent, false);

            RectTransform rt = container.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, 30); // Un poco más arriba del centro
            rt.sizeDelta = new Vector2(520, 280);

            // GridLayoutGroup para organizar los 6 bloques
            GridLayoutGroup grid = container.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(160, 130); // Bloques más grandes
            grid.spacing = new Vector2(12, 12);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.padding = new RectOffset(5, 5, 5, 5);

            // Crear los 6 bloques de preview
            for (int i = 0; i < themes.Length; i++)
            {
                CreateThemeBlock(container.transform, themes[i], i == 0);
            }
        }

        private void CreateThemeBlock(Transform parent, ThemePreviewData theme, bool isFree)
        {
            GameObject obj = new GameObject("Theme_" + theme.name);
            obj.transform.SetParent(parent, false);

            // Fondo con el color del tema
            Image bg = obj.AddComponent<Image>();
            bg.color = theme.bgColor;

            // Borde con el color de acento
            Outline outline = obj.AddComponent<Outline>();
            outline.effectColor = theme.accentColor;
            outline.effectDistance = new Vector2(3, 3);

            // Nombre del tema (centrado)
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(obj.transform, false);

            RectTransform nameRt = nameObj.AddComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0, 0.3f);
            nameRt.anchorMax = new Vector2(1, 0.7f);
            nameRt.offsetMin = new Vector2(5, 0);
            nameRt.offsetMax = new Vector2(-5, 0);

            TextMeshProUGUI nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
            nameTmp.text = theme.name;
            nameTmp.fontSize = 18;
            nameTmp.fontStyle = FontStyles.Bold;
            nameTmp.color = theme.accentColor;
            nameTmp.alignment = TextAlignmentOptions.Center;
            nameTmp.enableWordWrapping = true;

            // Badge
            GameObject badge = new GameObject(isFree ? "FreeBadge" : "ProBadge");
            badge.transform.SetParent(obj.transform, false);

            RectTransform badgeRt = badge.AddComponent<RectTransform>();
            badgeRt.anchorMin = new Vector2(0.5f, 1f);
            badgeRt.anchorMax = new Vector2(0.5f, 1f);
            badgeRt.pivot = new Vector2(0.5f, 1f);
            badgeRt.anchoredPosition = new Vector2(0, -8);
            badgeRt.sizeDelta = new Vector2(isFree ? 55 : 45, 20);

            Image badgeBg = badge.AddComponent<Image>();
            badgeBg.color = isFree ? new Color(0.2f, 0.8f, 0.3f, 1f) : goldColor;

            GameObject badgeText = new GameObject("Text");
            badgeText.transform.SetParent(badge.transform, false);

            RectTransform textRt = badgeText.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = badgeText.AddComponent<TextMeshProUGUI>();
            tmp.text = isFree ? "FREE" : "PRO";
            tmp.fontSize = 13;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = isFree ? Color.white : Color.black;
            tmp.alignment = TextAlignmentOptions.Center;

            // Líneas decorativas
            CreateThemeDecoration(obj.transform, theme.accentColor);
        }

        private void CreateThemeDecoration(Transform parent, Color accentColor)
        {
            // Línea horizontal decorativa superior
            GameObject line1 = new GameObject("DecoLine1");
            line1.transform.SetParent(parent, false);

            RectTransform line1Rt = line1.AddComponent<RectTransform>();
            line1Rt.anchorMin = new Vector2(0.1f, 0.2f);
            line1Rt.anchorMax = new Vector2(0.9f, 0.2f);
            line1Rt.sizeDelta = new Vector2(0, 3);

            Image line1Img = line1.AddComponent<Image>();
            line1Img.color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.6f);

            // Línea horizontal decorativa inferior
            GameObject line2 = new GameObject("DecoLine2");
            line2.transform.SetParent(parent, false);

            RectTransform line2Rt = line2.AddComponent<RectTransform>();
            line2Rt.anchorMin = new Vector2(0.15f, 0.1f);
            line2Rt.anchorMax = new Vector2(0.85f, 0.1f);
            line2Rt.sizeDelta = new Vector2(0, 2);

            Image line2Img = line2.AddComponent<Image>();
            line2Img.color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.4f);
        }

        private void CreatePrice(Transform parent)
        {
            GameObject obj = new GameObject("Price");
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0, 75); // Arriba de los botones
            rt.sizeDelta = new Vector2(200, 45);

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = "$29 MXN";
            tmp.fontSize = 34;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = goldColor;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        private void CreateButtons(Transform parent)
        {
            GameObject container = new GameObject("Buttons");
            container.transform.SetParent(parent, false);

            RectTransform rt = container.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0, 15);
            rt.sizeDelta = new Vector2(350, 55);

            HorizontalLayoutGroup hlg = container.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 25;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // Botón Comprar (dorado)
            buyButton = CreateButton(container.transform, AutoLocalizer.Get("buy_button"),
                goldColor, Color.black, 150, 50);
            buyButton.onClick.AddListener(OnBuyClicked);

            // Botón Cancelar (rojo)
            cancelButton = CreateButton(container.transform, AutoLocalizer.Get("cancel"),
                redColor, Color.white, 130, 50);
            cancelButton.onClick.AddListener(OnCancelClicked);
        }

        private Button CreateButton(Transform parent, string text, Color bgColor, Color textColor, float width, float height)
        {
            GameObject obj = new GameObject("Button_" + text);
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(width, height);

            LayoutElement le = obj.AddComponent<LayoutElement>();
            le.preferredWidth = width;
            le.preferredHeight = height;

            Image bg = obj.AddComponent<Image>();
            bg.color = bgColor;

            Button btn = obj.AddComponent<Button>();
            btn.targetGraphic = bg;

            ColorBlock colors = btn.colors;
            colors.normalColor = bgColor;
            colors.highlightedColor = new Color(Mathf.Min(bgColor.r * 1.15f, 1f), Mathf.Min(bgColor.g * 1.15f, 1f), Mathf.Min(bgColor.b * 1.15f, 1f), 1f);
            colors.pressedColor = new Color(bgColor.r * 0.8f, bgColor.g * 0.8f, bgColor.b * 0.8f, 1f);
            btn.colors = colors;

            // Texto
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);

            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 20;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            return btn;
        }

        private void CreateCloseButton(Transform parent)
        {
            GameObject obj = new GameObject("CloseButton");
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(-10, -10);
            rt.sizeDelta = new Vector2(40, 40);

            // Fondo con color cyan neon
            Image bg = obj.AddComponent<Image>();
            bg.color = neonCyan;

            closeButton = obj.AddComponent<Button>();
            closeButton.targetGraphic = bg;
            closeButton.onClick.AddListener(OnCancelClicked);

            ColorBlock colors = closeButton.colors;
            colors.normalColor = neonCyan;
            colors.highlightedColor = new Color(0.3f, 1f, 1f, 1f);
            colors.pressedColor = new Color(0f, 0.7f, 0.8f, 1f);
            closeButton.colors = colors;

            // Intentar cargar el icono desde Resources
            Sprite closeIcon = Resources.Load<Sprite>("Icons/CloseIcon");

            if (closeIcon != null)
            {
                // Usar el icono
                GameObject iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(obj.transform, false);

                RectTransform iconRt = iconObj.AddComponent<RectTransform>();
                iconRt.anchorMin = new Vector2(0.15f, 0.15f);
                iconRt.anchorMax = new Vector2(0.85f, 0.85f);
                iconRt.offsetMin = Vector2.zero;
                iconRt.offsetMax = Vector2.zero;

                Image iconImg = iconObj.AddComponent<Image>();
                iconImg.sprite = closeIcon;
                iconImg.color = Color.black; // Icono negro sobre fondo cyan
                iconImg.raycastTarget = false;
            }
            else
            {
                // Fallback: usar texto X
                GameObject textObj = new GameObject("X");
                textObj.transform.SetParent(obj.transform, false);

                RectTransform textRt = textObj.AddComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.offsetMin = Vector2.zero;
                textRt.offsetMax = Vector2.zero;

                TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
                tmp.text = "X";
                tmp.fontSize = 24;
                tmp.fontStyle = FontStyles.Bold;
                tmp.color = Color.black;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.raycastTarget = false;
            }
        }

        private void OnBuyClicked()
        {
            Debug.Log("[StylesProPromptPanel] Comprar clickeado");
            PremiumManager.Instance?.PurchaseStylesPro();
            Hide();
        }

        private void OnCancelClicked()
        {
            Debug.Log("[StylesProPromptPanel] Cancelar/Cerrar clickeado");
            Hide();
        }

        public void Hide()
        {
            Debug.Log("[StylesProPromptPanel] Cerrando panel...");

            if (transform.parent != null)
            {
                Destroy(transform.parent.gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
