#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor tool para configurar y estilizar el dropdown de idioma en la escena Settings
    /// </summary>
    public class LanguageDropdownSetup : EditorWindow
    {
        // Colores del tema neon
        private static readonly Color CyanColor = new Color(0f, 0.9608f, 1f, 1f); // #00F5FF
        private static readonly Color BgColor = new Color(0.02f, 0.08f, 0.16f, 0.9f);
        private static readonly Color BorderColor = new Color(0f, 0.9608f, 1f, 0.6f);

        [MenuItem("DigitPark/UI/Setup Language Dropdown Style")]
        public static void SetupLanguageDropdownStyle()
        {
            // Buscar el dropdown de idioma en la escena
            TMP_Dropdown[] dropdowns = GameObject.FindObjectsOfType<TMP_Dropdown>(true);
            TMP_Dropdown languageDropdown = null;

            foreach (var dd in dropdowns)
            {
                // Buscar por nombre o por padre
                if (dd.gameObject.name.ToLower().Contains("language") ||
                    (dd.transform.parent != null && dd.transform.parent.name.ToLower().Contains("language")))
                {
                    languageDropdown = dd;
                    break;
                }

                // Buscar por opciones (English, Español, etc)
                if (dd.options.Count >= 2)
                {
                    bool hasLanguages = false;
                    foreach (var opt in dd.options)
                    {
                        if (opt.text == "English" || opt.text == "Español" || opt.text == "Français")
                        {
                            hasLanguages = true;
                            break;
                        }
                    }
                    if (hasLanguages)
                    {
                        languageDropdown = dd;
                        break;
                    }
                }
            }

            if (languageDropdown == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontró el dropdown de idioma en la escena actual.", "OK");
                return;
            }

            Undo.RecordObject(languageDropdown.gameObject, "Setup Language Dropdown Style");

            // Aplicar estilos al dropdown
            ApplyDropdownStyles(languageDropdown);

            // Buscar y estilizar el label "Change Language"
            StyleChangeLangLabel(languageDropdown.transform.parent);

            EditorUtility.SetDirty(languageDropdown);
            EditorSceneManager.MarkSceneDirty(languageDropdown.gameObject.scene);

            Debug.Log("[LanguageDropdownSetup] Estilos aplicados exitosamente al dropdown de idioma");
            EditorUtility.DisplayDialog("Éxito", "Estilos del dropdown de idioma aplicados correctamente.", "OK");
        }

        private static void ApplyDropdownStyles(TMP_Dropdown dropdown)
        {
            // Obtener o crear Image component
            Image dropdownImage = dropdown.GetComponent<Image>();
            if (dropdownImage != null)
            {
                Undo.RecordObject(dropdownImage, "Style Dropdown Image");
                dropdownImage.color = BgColor;
            }

            // Agregar Outline para borde cyan
            Outline outline = dropdown.GetComponent<Outline>();
            if (outline == null)
            {
                outline = Undo.AddComponent<Outline>(dropdown.gameObject);
            }
            else
            {
                Undo.RecordObject(outline, "Style Dropdown Outline");
            }
            outline.effectColor = BorderColor;
            outline.effectDistance = new Vector2(2f, 2f);
            outline.useGraphicAlpha = false;

            // Agregar Shadow para glow
            Shadow[] shadows = dropdown.GetComponents<Shadow>();
            Shadow glow = null;
            foreach (var s in shadows)
            {
                if (!(s is Outline))
                {
                    glow = s;
                    break;
                }
            }
            if (glow == null)
            {
                glow = Undo.AddComponent<Shadow>(dropdown.gameObject);
            }
            else
            {
                Undo.RecordObject(glow, "Style Dropdown Glow");
            }
            glow.effectColor = new Color(CyanColor.r, CyanColor.g, CyanColor.b, 0.3f);
            glow.effectDistance = new Vector2(4f, -4f);

            // Estilizar el texto del label seleccionado
            Transform label = dropdown.transform.Find("Label");
            if (label != null)
            {
                TextMeshProUGUI labelTMP = label.GetComponent<TextMeshProUGUI>();
                if (labelTMP != null)
                {
                    Undo.RecordObject(labelTMP, "Style Dropdown Label");
                    labelTMP.color = CyanColor;
                }
            }

            // Estilizar la flecha
            Transform arrow = dropdown.transform.Find("Arrow");
            if (arrow != null)
            {
                Image arrowImage = arrow.GetComponent<Image>();
                if (arrowImage != null)
                {
                    Undo.RecordObject(arrowImage, "Style Dropdown Arrow");
                    arrowImage.color = CyanColor;
                }
            }

            // Configurar colores del dropdown
            ColorBlock colors = dropdown.colors;
            colors.normalColor = BgColor;
            colors.highlightedColor = new Color(CyanColor.r * 0.3f, CyanColor.g * 0.3f, CyanColor.b * 0.3f, 0.8f);
            colors.pressedColor = new Color(CyanColor.r * 0.5f, CyanColor.g * 0.5f, CyanColor.b * 0.5f, 0.9f);
            colors.selectedColor = colors.highlightedColor;
            dropdown.colors = colors;

            // Estilizar el template
            Transform template = dropdown.transform.Find("Template");
            if (template != null)
            {
                Image templateBg = template.GetComponent<Image>();
                if (templateBg != null)
                {
                    Undo.RecordObject(templateBg, "Style Template BG");
                    templateBg.color = new Color(0.03f, 0.06f, 0.12f, 0.95f);
                }

                Outline templateOutline = template.GetComponent<Outline>();
                if (templateOutline == null)
                {
                    templateOutline = Undo.AddComponent<Outline>(template.gameObject);
                }
                else
                {
                    Undo.RecordObject(templateOutline, "Style Template Outline");
                }
                templateOutline.effectColor = BorderColor;
                templateOutline.effectDistance = new Vector2(1.5f, 1.5f);

                // Estilizar items
                StyleTemplateItems(template);
            }
        }

        private static void StyleTemplateItems(Transform template)
        {
            Transform viewport = template.Find("Viewport");
            if (viewport == null) return;

            Transform content = viewport.Find("Content");
            if (content == null) return;

            Transform item = content.Find("Item");
            if (item == null) return;

            // Item Checkmark
            Transform checkmark = item.Find("Item Checkmark");
            if (checkmark != null)
            {
                Image checkImage = checkmark.GetComponent<Image>();
                if (checkImage != null)
                {
                    Undo.RecordObject(checkImage, "Style Checkmark");
                    checkImage.color = CyanColor;
                }
            }

            // Item Label
            Transform itemLabel = item.Find("Item Label");
            if (itemLabel != null)
            {
                TextMeshProUGUI itemTMP = itemLabel.GetComponent<TextMeshProUGUI>();
                if (itemTMP != null)
                {
                    Undo.RecordObject(itemTMP, "Style Item Label");
                    itemTMP.color = new Color(0.9f, 0.95f, 1f, 1f);
                }
            }

            // Toggle colors
            Toggle itemToggle = item.GetComponent<Toggle>();
            if (itemToggle != null)
            {
                Undo.RecordObject(itemToggle, "Style Item Toggle");
                ColorBlock toggleColors = itemToggle.colors;
                toggleColors.normalColor = Color.clear;
                toggleColors.highlightedColor = new Color(CyanColor.r * 0.2f, CyanColor.g * 0.2f, CyanColor.b * 0.2f, 0.5f);
                toggleColors.pressedColor = new Color(CyanColor.r * 0.3f, CyanColor.g * 0.3f, CyanColor.b * 0.3f, 0.7f);
                toggleColors.selectedColor = new Color(CyanColor.r * 0.15f, CyanColor.g * 0.15f, CyanColor.b * 0.15f, 0.4f);
                itemToggle.colors = toggleColors;
            }
        }

        private static void StyleChangeLangLabel(Transform parent)
        {
            if (parent == null) return;

            // Buscar el label "Change Language"
            TextMeshProUGUI changeLangLabel = null;

            foreach (Transform child in parent)
            {
                TextMeshProUGUI tmp = child.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    string text = tmp.text.ToLower();
                    if (text.Contains("language") || text.Contains("idioma") || text.Contains("change"))
                    {
                        changeLangLabel = tmp;
                        break;
                    }
                }
            }

            // Si no encontramos en los hijos directos, buscar en el padre del padre
            if (changeLangLabel == null && parent.parent != null)
            {
                foreach (Transform child in parent.parent)
                {
                    TextMeshProUGUI tmp = child.GetComponent<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        string text = tmp.text.ToLower();
                        if (text.Contains("language") || text.Contains("idioma"))
                        {
                            changeLangLabel = tmp;
                            break;
                        }
                    }
                }
            }

            if (changeLangLabel != null)
            {
                Undo.RecordObject(changeLangLabel, "Style Change Language Label");

                // Aplicar color cyan
                changeLangLabel.color = CyanColor;

                // Remover cualquier emoji Unicode que no se muestre
                string currentText = changeLangLabel.text;
                currentText = currentText.Replace("\U0001F310 ", "").Replace("\u25CF ", "");
                changeLangLabel.text = currentText;

                // Hacer bold
                changeLangLabel.fontStyle = FontStyles.Bold;

                // Crear contenedor horizontal para icono + texto
                CreateLanguageIconContainer(changeLangLabel);

                EditorUtility.SetDirty(changeLangLabel);
                Debug.Log($"[LanguageDropdownSetup] Label estilizado: {changeLangLabel.text}");
            }
            else
            {
                Debug.LogWarning("[LanguageDropdownSetup] No se encontró el label 'Change Language'");
            }
        }

        private static void CreateLanguageIconContainer(TextMeshProUGUI label)
        {
            Transform labelTransform = label.transform;
            Transform parentTransform = labelTransform.parent;

            // Verificar si ya existe el contenedor
            Transform existingContainer = parentTransform.Find("LanguageIconContainer");
            if (existingContainer != null)
            {
                // Actualizar posición del icono existente
                Transform existingIcon = existingContainer.Find("LanguageIcon");
                if (existingIcon != null)
                {
                    RectTransform iconRect = existingIcon.GetComponent<RectTransform>();
                    if (iconRect != null)
                    {
                        Undo.RecordObject(iconRect, "Update Icon Position");
                        iconRect.anchoredPosition = Vector2.zero;
                        EditorUtility.SetDirty(iconRect);
                        Debug.Log("[LanguageDropdownSetup] Posición del icono actualizada a (0, 0)");
                    }
                }
                return;
            }

            // Crear contenedor con HorizontalLayoutGroup
            GameObject container = new GameObject("LanguageIconContainer");
            Undo.RegisterCreatedObjectUndo(container, "Create Language Icon Container");

            container.transform.SetParent(parentTransform, false);

            // Configurar RectTransform del contenedor
            RectTransform containerRect = container.AddComponent<RectTransform>();
            RectTransform labelRect = label.GetComponent<RectTransform>();

            // Copiar posicion del label original
            containerRect.anchorMin = labelRect.anchorMin;
            containerRect.anchorMax = labelRect.anchorMax;
            containerRect.anchoredPosition = labelRect.anchoredPosition;
            containerRect.sizeDelta = new Vector2(labelRect.sizeDelta.x + 60f, labelRect.sizeDelta.y);
            containerRect.pivot = labelRect.pivot;

            // Agregar HorizontalLayoutGroup
            HorizontalLayoutGroup hlg = container.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = 10f;

            // Crear Image para el icono (50x50)
            GameObject iconObj = new GameObject("LanguageIcon");
            Undo.RegisterCreatedObjectUndo(iconObj, "Create Language Icon");
            iconObj.transform.SetParent(container.transform, false);

            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(50f, 50f);
            iconRect.anchoredPosition = Vector2.zero; // Posición X=0, Y=0

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.raycastTarget = false;

            // Cargar el sprite earth.png automáticamente
            Sprite earthSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/earth.png");
            if (earthSprite != null)
            {
                iconImage.sprite = earthSprite;
                iconImage.color = Color.white; // Blanco para mostrar el sprite con sus colores originales
                Debug.Log("[LanguageDropdownSetup] Sprite earth.png asignado automáticamente");
            }
            else
            {
                iconImage.color = CyanColor; // Color cyan por defecto si no encuentra el sprite
                Debug.LogWarning("[LanguageDropdownSetup] No se encontró earth.png, usando color cyan");
            }

            // Mover el label al contenedor
            Undo.SetTransformParent(labelTransform, container.transform, "Move Label to Container");
            labelTransform.SetSiblingIndex(1); // Icono primero, luego texto

            // Ajustar el label
            labelRect.sizeDelta = new Vector2(labelRect.sizeDelta.x, labelRect.sizeDelta.y);

            // Posicionar el contenedor donde estaba el label
            container.transform.SetSiblingIndex(labelTransform.GetSiblingIndex());

            Debug.Log("[LanguageDropdownSetup] Contenedor de icono creado - Agrega tu sprite al Image 'LanguageIcon'");
        }

        [MenuItem("DigitPark/UI/Fix Language Icon Position")]
        public static void FixLanguageIconPosition()
        {
            // Buscar el LanguageIcon en la escena
            GameObject iconObj = GameObject.Find("LanguageIcon");
            if (iconObj == null)
            {
                // Buscar en todos los objetos incluyendo inactivos
                var allImages = GameObject.FindObjectsOfType<Image>(true);
                foreach (var img in allImages)
                {
                    if (img.gameObject.name == "LanguageIcon")
                    {
                        iconObj = img.gameObject;
                        break;
                    }
                }
            }

            if (iconObj != null)
            {
                RectTransform iconRect = iconObj.GetComponent<RectTransform>();
                if (iconRect != null)
                {
                    Undo.RecordObject(iconRect, "Fix Icon Position");
                    iconRect.anchoredPosition = Vector2.zero;
                    EditorUtility.SetDirty(iconRect);
                    EditorSceneManager.MarkSceneDirty(iconObj.scene);
                    Debug.Log("[LanguageDropdownSetup] Posición del icono fijada a (0, 0)");
                    EditorUtility.DisplayDialog("Éxito", "Posición del icono actualizada a (0, 0)", "OK");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "No se encontró LanguageIcon en la escena", "OK");
            }
        }

        [MenuItem("DigitPark/UI/Delete Language Icon Container")]
        public static void DeleteLanguageIconContainer()
        {
            GameObject container = GameObject.Find("LanguageIconContainer");
            if (container != null)
            {
                // Primero, mover el label de vuelta al padre original si existe
                var label = container.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    Transform originalParent = container.transform.parent;
                    Undo.SetTransformParent(label.transform, originalParent, "Move Label Back");
                }

                Undo.DestroyObjectImmediate(container);
                EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                Debug.Log("[LanguageDropdownSetup] Contenedor eliminado");
                EditorUtility.DisplayDialog("Éxito", "Contenedor eliminado. Ejecuta 'Setup Language Dropdown Style' de nuevo.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Info", "No existe LanguageIconContainer en la escena", "OK");
            }
        }

        [MenuItem("DigitPark/UI/Open Settings Scene and Apply Styles")]
        public static void OpenSettingsAndApply()
        {
            string settingsScenePath = "Assets/_Project/Scenes/Settings.unity";

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(settingsScenePath);
                EditorApplication.delayCall += () =>
                {
                    SetupLanguageDropdownStyle();
                };
            }
        }
    }
}
#endif
