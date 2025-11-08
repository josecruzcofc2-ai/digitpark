using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace DigitPark.UI
{
    /// <summary>
    /// Factory para crear elementos de UI programáticamente
    /// Centraliza la creación de componentes con estilos consistentes
    /// </summary>
    public static class UIFactory
    {
        // Paleta de colores
        public static readonly Color ElectricBlue = new Color(0f, 0.83f, 1f);
        public static readonly Color BrightGreen = new Color(0f, 1f, 0.53f);
        public static readonly Color CoralRed = new Color(1f, 0.42f, 0.42f);
        public static readonly Color NeonYellow = new Color(1f, 0.84f, 0f);
        public static readonly Color Purple = new Color(0.55f, 0.36f, 0.96f);
        public static readonly Color DarkBG1 = new Color(0.06f, 0.06f, 0.14f);
        public static readonly Color DarkBG2 = new Color(0.1f, 0.1f, 0.24f);

        #region Canvas

        /// <summary>
        /// Crea un Canvas principal de pantalla completa
        /// </summary>
        public static Canvas CreateCanvas(string name, int sortOrder = 0)
        {
            // Asegurar que existe un EventSystem en la escena
            EnsureEventSystemExists();

            GameObject canvasObj = new GameObject(name);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920); // Portrait
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            return canvas;
        }

        /// <summary>
        /// Asegura que existe un EventSystem en la escena
        /// </summary>
        private static void EnsureEventSystemExists()
        {
            // Buscar EventSystem existente
            EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();

            if (eventSystem == null)
            {
                // Crear EventSystem si no existe
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystem = eventSystemObj.AddComponent<EventSystem>();
                eventSystemObj.AddComponent<StandaloneInputModule>();

                Debug.Log("[UIFactory] EventSystem creado automáticamente");
            }
        }

        #endregion

        #region Panel

        /// <summary>
        /// Crea un panel base
        /// </summary>
        public static GameObject CreatePanel(Transform parent, string name, Color? bgColor = null)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image image = panel.AddComponent<Image>();
            image.color = bgColor ?? new Color(0, 0, 0, 0.8f);

            return panel;
        }

        /// <summary>
        /// Crea un panel con tamaño específico
        /// </summary>
        public static GameObject CreatePanelWithSize(Transform parent, string name, Vector2 size, Color? bgColor = null)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;

            Image image = panel.AddComponent<Image>();
            image.color = bgColor ?? DarkBG1;

            return panel;
        }

        #endregion

        #region Text

        /// <summary>
        /// Crea un TextMeshPro elemento
        /// </summary>
        public static TextMeshProUGUI CreateText(Transform parent, string name, string content, int fontSize = 24, Color? color = null, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);

            RectTransform rt = textObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(400, 60);

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = fontSize;
            text.color = color ?? Color.white;
            text.alignment = alignment;

            return text;
        }

        /// <summary>
        /// Crea un título grande
        /// </summary>
        public static TextMeshProUGUI CreateTitle(Transform parent, string name, string content)
        {
            TextMeshProUGUI title = CreateText(parent, name, content, 72, ElectricBlue, TextAlignmentOptions.Center);

            RectTransform rt = title.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -100);
            rt.sizeDelta = new Vector2(800, 120);

            // Outline en lugar de Shadow para TMP
            title.outlineWidth = 0.2f;
            title.outlineColor = new Color(0, 0, 0, 0.5f);

            return title;
        }

        #endregion

        #region Button

        /// <summary>
        /// Crea un botón estándar
        /// </summary>
        public static Button CreateButton(Transform parent, string name, string text, Vector2 size, Color? bgColor = null)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);

            RectTransform rt = buttonObj.AddComponent<RectTransform>();
            rt.sizeDelta = size;

            Image image = buttonObj.AddComponent<Image>();
            image.color = bgColor ?? ElectricBlue;

            Button button = buttonObj.AddComponent<Button>();

            ColorBlock colors = button.colors;
            colors.normalColor = bgColor ?? ElectricBlue;
            colors.highlightedColor = new Color(0.2f, 0.9f, 1f);
            colors.pressedColor = new Color(0f, 0.6f, 0.8f);
            colors.disabledColor = Color.gray;
            button.colors = colors;

            // Texto del botón
            if (!string.IsNullOrEmpty(text))
            {
                TextMeshProUGUI buttonText = CreateText(buttonObj.transform, "Text", text, 32, Color.white);
                RectTransform textRT = buttonText.GetComponent<RectTransform>();
                textRT.anchorMin = Vector2.zero;
                textRT.anchorMax = Vector2.one;
                textRT.sizeDelta = Vector2.zero;
            }

            return button;
        }

        /// <summary>
        /// Crea un botón grande (para acciones principales)
        /// </summary>
        public static Button CreateLargeButton(Transform parent, string name, string text)
        {
            Button button = CreateButton(parent, name, text, new Vector2(600, 120), ElectricBlue);

            // Aumentar tamaño de fuente
            TextMeshProUGUI btnText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.fontSize = 48;
                btnText.fontStyle = FontStyles.Bold;
            }

            return button;
        }

        /// <summary>
        /// Crea un botón pequeño (para acciones secundarias)
        /// </summary>
        public static Button CreateSmallButton(Transform parent, string name, string text)
        {
            return CreateButton(parent, name, text, new Vector2(300, 80), new Color(0.3f, 0.3f, 0.4f));
        }

        #endregion

        #region InputField

        /// <summary>
        /// Crea un TMP_InputField
        /// </summary>
        public static TMP_InputField CreateInputField(Transform parent, string name, string placeholder, TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard)
        {
            GameObject inputObj = new GameObject(name);
            inputObj.transform.SetParent(parent, false);

            RectTransform rt = inputObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(600, 80);

            Image image = inputObj.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.25f);

            TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();
            inputField.contentType = contentType;

            // Text Area
            GameObject textAreaObj = new GameObject("Text Area");
            textAreaObj.transform.SetParent(inputObj.transform, false);
            RectTransform textAreaRT = textAreaObj.AddComponent<RectTransform>();
            textAreaRT.anchorMin = Vector2.zero;
            textAreaRT.anchorMax = Vector2.one;
            textAreaRT.sizeDelta = Vector2.zero;
            textAreaRT.offsetMin = new Vector2(20, 0);
            textAreaRT.offsetMax = new Vector2(-20, 0);

            // Placeholder
            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(textAreaObj.transform, false);
            TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderText.text = placeholder;
            placeholderText.fontSize = 28;
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f);
            placeholderText.alignment = TextAlignmentOptions.MidlineLeft;

            RectTransform phRT = placeholderObj.GetComponent<RectTransform>();
            phRT.anchorMin = Vector2.zero;
            phRT.anchorMax = Vector2.one;
            phRT.sizeDelta = Vector2.zero;

            // Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(textAreaObj.transform, false);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.fontSize = 28;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.richText = false;

            RectTransform textRT = textObj.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            inputField.textViewport = textAreaRT;
            inputField.textComponent = text;
            inputField.placeholder = placeholderText;

            return inputField;
        }

        #endregion

        #region Toggle

        /// <summary>
        /// Crea un Toggle (checkbox)
        /// </summary>
        public static Toggle CreateToggle(Transform parent, string name, string label)
        {
            GameObject toggleObj = new GameObject(name);
            toggleObj.transform.SetParent(parent, false);

            RectTransform rt = toggleObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(400, 60);

            Toggle toggle = toggleObj.AddComponent<Toggle>();

            // Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(toggleObj.transform, false);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.15f, 0.15f, 0.25f);

            RectTransform bgRT = bgObj.GetComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0f, 0.5f);
            bgRT.anchorMax = new Vector2(0f, 0.5f);
            bgRT.pivot = new Vector2(0f, 0.5f);
            bgRT.sizeDelta = new Vector2(60, 60);
            bgRT.anchoredPosition = Vector2.zero;

            // Checkmark
            GameObject checkObj = new GameObject("Checkmark");
            checkObj.transform.SetParent(bgObj.transform, false);
            Image checkImage = checkObj.AddComponent<Image>();
            checkImage.color = ElectricBlue;

            RectTransform checkRT = checkObj.GetComponent<RectTransform>();
            checkRT.anchorMin = Vector2.zero;
            checkRT.anchorMax = Vector2.one;
            checkRT.sizeDelta = new Vector2(-20, -20);

            toggle.targetGraphic = bgImage;
            toggle.graphic = checkImage;

            // Label
            if (!string.IsNullOrEmpty(label))
            {
                TextMeshProUGUI labelText = CreateText(toggleObj.transform, "Label", label, 28, Color.white, TextAlignmentOptions.MidlineLeft);
                RectTransform labelRT = labelText.GetComponent<RectTransform>();
                labelRT.anchorMin = new Vector2(0f, 0f);
                labelRT.anchorMax = new Vector2(1f, 1f);
                labelRT.offsetMin = new Vector2(80, 0);
                labelRT.offsetMax = Vector2.zero;
            }

            return toggle;
        }

        #endregion

        #region Slider

        /// <summary>
        /// Crea un Slider
        /// </summary>
        public static Slider CreateSlider(Transform parent, string name, float minValue = 0f, float maxValue = 1f)
        {
            GameObject sliderObj = new GameObject(name);
            sliderObj.transform.SetParent(parent, false);

            RectTransform rt = sliderObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(500, 40);

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = minValue;
            slider.maxValue = maxValue;

            // Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(sliderObj.transform, false);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.15f, 0.15f, 0.25f);

            RectTransform bgRT = bgObj.GetComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0f, 0.25f);
            bgRT.anchorMax = new Vector2(1f, 0.75f);
            bgRT.sizeDelta = Vector2.zero;

            // Fill Area
            GameObject fillAreaObj = new GameObject("Fill Area");
            fillAreaObj.transform.SetParent(sliderObj.transform, false);
            RectTransform fillAreaRT = fillAreaObj.AddComponent<RectTransform>();
            fillAreaRT.anchorMin = new Vector2(0f, 0.25f);
            fillAreaRT.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRT.sizeDelta = new Vector2(-20, 0);

            // Fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillAreaObj.transform, false);
            Image fillImage = fillObj.AddComponent<Image>();
            fillImage.color = ElectricBlue;

            RectTransform fillRT = fillObj.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.sizeDelta = Vector2.zero;

            slider.fillRect = fillRT;

            // Handle Area
            GameObject handleAreaObj = new GameObject("Handle Slide Area");
            handleAreaObj.transform.SetParent(sliderObj.transform, false);
            RectTransform handleAreaRT = handleAreaObj.AddComponent<RectTransform>();
            handleAreaRT.anchorMin = Vector2.zero;
            handleAreaRT.anchorMax = Vector2.one;
            handleAreaRT.sizeDelta = new Vector2(-20, 0);

            // Handle
            GameObject handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(handleAreaObj.transform, false);
            Image handleImage = handleObj.AddComponent<Image>();
            handleImage.color = Color.white;

            RectTransform handleRT = handleObj.GetComponent<RectTransform>();
            handleRT.sizeDelta = new Vector2(40, 40);

            slider.handleRect = handleRT;
            slider.targetGraphic = handleImage;

            return slider;
        }

        #endregion

        #region Dropdown

        /// <summary>
        /// Crea un TMP_Dropdown
        /// </summary>
        public static TMP_Dropdown CreateDropdown(Transform parent, string name, string[] options)
        {
            GameObject dropdownObj = new GameObject(name);
            dropdownObj.transform.SetParent(parent, false);

            RectTransform rt = dropdownObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(500, 80);

            Image image = dropdownObj.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.25f);

            TMP_Dropdown dropdown = dropdownObj.AddComponent<TMP_Dropdown>();

            // Label
            TextMeshProUGUI labelText = CreateText(dropdownObj.transform, "Label", options.Length > 0 ? options[0] : "", 28, Color.white, TextAlignmentOptions.MidlineLeft);
            RectTransform labelRT = labelText.GetComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = new Vector2(20, 0);
            labelRT.offsetMax = new Vector2(-40, 0);

            dropdown.captionText = labelText;

            // Arrow
            GameObject arrowObj = new GameObject("Arrow");
            arrowObj.transform.SetParent(dropdownObj.transform, false);
            TextMeshProUGUI arrowText = arrowObj.AddComponent<TextMeshProUGUI>();
            arrowText.text = "▼";
            arrowText.fontSize = 24;
            arrowText.color = Color.white;
            arrowText.alignment = TextAlignmentOptions.Center;

            RectTransform arrowRT = arrowObj.GetComponent<RectTransform>();
            arrowRT.anchorMin = new Vector2(1f, 0f);
            arrowRT.anchorMax = Vector2.one;
            arrowRT.sizeDelta = new Vector2(40, 0);
            arrowRT.pivot = new Vector2(1f, 0.5f);

            // Template (REQUERIDO para TMP_Dropdown)
            GameObject templateObj = new GameObject("Template");
            templateObj.transform.SetParent(dropdownObj.transform, false);

            RectTransform templateRT = templateObj.AddComponent<RectTransform>();
            templateRT.anchorMin = new Vector2(0f, 0f);
            templateRT.anchorMax = new Vector2(1f, 0f);
            templateRT.pivot = new Vector2(0.5f, 1f);
            templateRT.anchoredPosition = new Vector2(0, 2);
            templateRT.sizeDelta = new Vector2(0, 300);

            Image templateImage = templateObj.AddComponent<Image>();
            templateImage.color = new Color(0.15f, 0.15f, 0.25f);

            // Viewport para el scroll
            GameObject viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(templateObj.transform, false);

            RectTransform viewportRT = viewportObj.AddComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.sizeDelta = Vector2.zero;
            viewportRT.offsetMin = new Vector2(10, 10);
            viewportRT.offsetMax = new Vector2(-10, -10);

            Image viewportImage = viewportObj.AddComponent<Image>();
            viewportImage.color = new Color(0, 0, 0, 0);

            Mask viewportMask = viewportObj.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            // Content (container de items)
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);

            RectTransform contentRT = contentObj.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0f, 1f);
            contentRT.anchorMax = new Vector2(1f, 1f);
            contentRT.pivot = new Vector2(0.5f, 1f);
            contentRT.anchoredPosition = Vector2.zero;
            contentRT.sizeDelta = new Vector2(0, 0);

            // Item (template de cada opción)
            GameObject itemObj = new GameObject("Item");
            itemObj.transform.SetParent(contentObj.transform, false);

            RectTransform itemRT = itemObj.AddComponent<RectTransform>();
            itemRT.anchorMin = new Vector2(0f, 0.5f);
            itemRT.anchorMax = new Vector2(1f, 0.5f);
            itemRT.pivot = new Vector2(0.5f, 0.5f);
            itemRT.sizeDelta = new Vector2(0, 60);

            Image itemImage = itemObj.AddComponent<Image>();
            itemImage.color = new Color(0.2f, 0.2f, 0.3f);

            Toggle itemToggle = itemObj.AddComponent<Toggle>();
            itemToggle.targetGraphic = itemImage;
            itemToggle.isOn = false;

            // Background del toggle
            GameObject itemBgObj = new GameObject("Item Background");
            itemBgObj.transform.SetParent(itemObj.transform, false);

            RectTransform itemBgRT = itemBgObj.AddComponent<RectTransform>();
            itemBgRT.anchorMin = Vector2.zero;
            itemBgRT.anchorMax = Vector2.one;
            itemBgRT.sizeDelta = Vector2.zero;

            Image itemBgImage = itemBgObj.AddComponent<Image>();
            itemBgImage.color = ElectricBlue;
            itemBgImage.enabled = false; // Solo se ve cuando está seleccionado

            itemToggle.graphic = itemBgImage;

            // Checkmark
            GameObject checkmarkObj = new GameObject("Item Checkmark");
            checkmarkObj.transform.SetParent(itemObj.transform, false);

            RectTransform checkmarkRT = checkmarkObj.AddComponent<RectTransform>();
            checkmarkRT.anchorMin = new Vector2(0f, 0.5f);
            checkmarkRT.anchorMax = new Vector2(0f, 0.5f);
            checkmarkRT.pivot = new Vector2(0f, 0.5f);
            checkmarkRT.anchoredPosition = new Vector2(10, 0);
            checkmarkRT.sizeDelta = new Vector2(30, 30);

            TextMeshProUGUI checkmarkText = checkmarkObj.AddComponent<TextMeshProUGUI>();
            checkmarkText.text = "✓";
            checkmarkText.fontSize = 24;
            checkmarkText.color = ElectricBlue;
            checkmarkText.alignment = TextAlignmentOptions.Center;

            // Label del item
            GameObject itemLabelObj = new GameObject("Item Label");
            itemLabelObj.transform.SetParent(itemObj.transform, false);

            RectTransform itemLabelRT = itemLabelObj.AddComponent<RectTransform>();
            itemLabelRT.anchorMin = Vector2.zero;
            itemLabelRT.anchorMax = Vector2.one;
            itemLabelRT.sizeDelta = Vector2.zero;
            itemLabelRT.offsetMin = new Vector2(50, 0);
            itemLabelRT.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI itemLabelText = itemLabelObj.AddComponent<TextMeshProUGUI>();
            itemLabelText.text = "Item";
            itemLabelText.fontSize = 24;
            itemLabelText.color = Color.white;
            itemLabelText.alignment = TextAlignmentOptions.MidlineLeft;

            dropdown.itemText = itemLabelText;

            // Configurar dropdown
            dropdown.template = templateRT;
            templateObj.SetActive(false); // El template debe estar inactivo inicialmente

            // Agregar opciones
            dropdown.options.Clear();
            foreach (string option in options)
            {
                dropdown.options.Add(new TMP_Dropdown.OptionData(option));
            }

            return dropdown;
        }

        #endregion

        #region ScrollRect

        /// <summary>
        /// Crea un ScrollRect para listas
        /// </summary>
        public static ScrollRect CreateScrollView(Transform parent, string name, Vector2 size)
        {
            GameObject scrollObj = new GameObject(name);
            scrollObj.transform.SetParent(parent, false);

            RectTransform rt = scrollObj.AddComponent<RectTransform>();
            rt.sizeDelta = size;

            Image image = scrollObj.AddComponent<Image>();
            image.color = new Color(0.1f, 0.1f, 0.2f);

            ScrollRect scrollRect = scrollObj.AddComponent<ScrollRect>();

            // Viewport
            GameObject viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollObj.transform, false);
            RectTransform viewportRT = viewportObj.AddComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.sizeDelta = Vector2.zero;

            Image viewportImage = viewportObj.AddComponent<Image>();
            viewportImage.color = new Color(0, 0, 0, 0);

            Mask mask = viewportObj.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            // Content
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);
            RectTransform contentRT = contentObj.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0f, 1f);
            contentRT.anchorMax = new Vector2(1f, 1f);
            contentRT.pivot = new Vector2(0.5f, 1f);
            contentRT.sizeDelta = new Vector2(0, 1000);

            VerticalLayoutGroup layoutGroup = contentObj.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 10;
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);

            ContentSizeFitter fitter = contentObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewportRT;
            scrollRect.content = contentRT;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            return scrollRect;
        }

        #endregion

        #region Image

        /// <summary>
        /// Crea una imagen
        /// </summary>
        public static Image CreateImage(Transform parent, string name, Vector2 size, Color color)
        {
            GameObject imageObj = new GameObject(name);
            imageObj.transform.SetParent(parent, false);

            RectTransform rt = imageObj.AddComponent<RectTransform>();
            rt.sizeDelta = size;

            Image image = imageObj.AddComponent<Image>();
            image.color = color;

            return image;
        }

        #endregion

        #region Loading

        /// <summary>
        /// Crea un panel de loading/carga
        /// </summary>
        public static GameObject CreateLoadingPanel(Transform parent)
        {
            GameObject loadingPanel = CreatePanel(parent, "LoadingPanel", new Color(0, 0, 0, 0.9f));

            TextMeshProUGUI loadingText = CreateText(loadingPanel.transform, "LoadingText", "Cargando...", 36, ElectricBlue);
            RectTransform loadingRT = loadingText.GetComponent<RectTransform>();
            loadingRT.anchoredPosition = Vector2.zero;

            loadingPanel.SetActive(false);

            return loadingPanel;
        }

        #endregion
    }
}
