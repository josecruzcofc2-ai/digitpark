using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Builder para la seccion Legal en Settings.
    /// Crea links simples estilo texto azul para documentos legales.
    /// </summary>
    public class LegalSectionBuilder : EditorWindow
    {
        // Colores
        private static readonly Color LINK_COLOR = new Color(0.3f, 0.7f, 1f, 1f); // Azul link
        private static readonly Color TEXT_SECONDARY = new Color(0.5f, 0.5f, 0.55f, 1f);
        private static readonly Color DANGER_COLOR = new Color(1f, 0.4f, 0.4f, 1f);

        [MenuItem("DigitPark/UI Builders/Settings/Build Legal Section", false, 310)]
        public static void BuildLegalSection()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[LegalSectionBuilder] No se encontro Canvas en la escena");
                return;
            }

            Transform settingsContainer = FindSettingsContainer(canvas.transform);
            if (settingsContainer == null)
            {
                settingsContainer = canvas.transform;
            }

            // Verificar si ya existe
            Transform existing = settingsContainer.Find("LegalSection");
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("Legal Section Existe",
                    "Ya existe una LegalSection. Deseas reemplazarla?",
                    "Si, Reemplazar", "Cancelar"))
                    return;

                Undo.DestroyObjectImmediate(existing.gameObject);
            }

            GameObject legalSection = CreateLegalSection(settingsContainer);
            Undo.RegisterCreatedObjectUndo(legalSection, "Create Legal Section");
            Selection.activeGameObject = legalSection;

            Debug.Log("[LegalSectionBuilder] Legal Section creada exitosamente");
        }

        private static Transform FindSettingsContainer(Transform root)
        {
            string[] possibleNames = { "SettingsPanel", "SettingsContent", "Content", "ScrollContent", "MainPanel" };

            foreach (string name in possibleNames)
            {
                Transform found = FindDeep(root, name);
                if (found != null) return found;
            }

            return null;
        }

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

        private static GameObject CreateLegalSection(Transform parent)
        {
            // Contenedor principal - compacto
            GameObject section = new GameObject("LegalSection");
            section.transform.SetParent(parent, false);

            RectTransform rt = section.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(-40, 180);
            rt.anchoredPosition = new Vector2(0, 20);

            // Layout vertical compacto
            VerticalLayoutGroup vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 15, 15);
            vlg.spacing = 8;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // Titulo pequeño
            CreateTitle(section.transform);

            // Links simples
            CreateLinkButton(section.transform, "TermsButton", "Terminos de Servicio");
            CreateLinkButton(section.transform, "PrivacyButton", "Politica de Privacidad");
            CreateLinkButton(section.transform, "ResponsibleGamingButton", "Juego Responsable");
            CreateLinkButton(section.transform, "TriumphTermsButton", "Terminos de Triumph");

            // Link de auto-exclusion (rojo)
            CreateLinkButton(section.transform, "SelfExclusionButton", "Auto-Exclusion", true);

            return section;
        }

        private static void CreateTitle(Transform parent)
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(parent, false);

            RectTransform rt = titleObj.AddComponent<RectTransform>();
            LayoutElement le = titleObj.AddComponent<LayoutElement>();
            le.preferredHeight = 25;

            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.text = "Legal";
            title.fontSize = 16;
            title.color = TEXT_SECONDARY;
            title.alignment = TextAlignmentOptions.Left;
            title.fontStyle = FontStyles.Normal;
        }

        private static void CreateLinkButton(Transform parent, string name, string text, bool isDanger = false)
        {
            GameObject linkObj = new GameObject(name);
            linkObj.transform.SetParent(parent, false);

            RectTransform rt = linkObj.AddComponent<RectTransform>();
            LayoutElement le = linkObj.AddComponent<LayoutElement>();
            le.preferredHeight = 28;

            // Texto estilo link
            TextMeshProUGUI linkText = linkObj.AddComponent<TextMeshProUGUI>();
            linkText.text = text;
            linkText.fontSize = 16;
            linkText.color = isDanger ? DANGER_COLOR : LINK_COLOR;
            linkText.alignment = TextAlignmentOptions.Left;
            linkText.fontStyle = FontStyles.Underline;

            // Boton invisible sobre el texto
            Button btn = linkObj.AddComponent<Button>();
            btn.transition = Selectable.Transition.ColorTint;

            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            btn.colors = colors;

            // El texto es el target del boton
            btn.targetGraphic = linkText;
        }
    }
}
