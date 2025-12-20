using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.Themes;

namespace DigitPark.Editor
{
    /// <summary>
    /// Aplica estilo Neon a la escena Settings
    /// </summary>
    public class SettingsNeonStyler : EditorWindow
    {
        [MenuItem("DigitPark/Apply Neon Style to Settings")]
        public static void ApplyNeonStyle()
        {
            // Buscar todos los elementos en la escena
            var allObjects = GameObject.FindObjectsOfType<GameObject>(true);
            int styled = 0;

            foreach (var obj in allObjects)
            {
                string name = obj.name.ToLower();

                // Textos de labels de volumen
                if (name.Contains("volumelabel") || name.Contains("soundvolumelabel") || name.Contains("effectsvolumelabel"))
                {
                    SetupThemeApplier(obj, ThemeApplier.ElementType.TextTitle, false, true);
                    styled++;
                }
                // Textos de porcentaje de sliders
                else if (name.Contains("valuetext") || name.Contains("percenttext"))
                {
                    SetupThemeApplier(obj, ThemeApplier.ElementType.Accent, false, true);
                    styled++;
                }
                // Labels de idioma y estilo
                else if (name.Contains("changelanguage") || name.Contains("changestyle") ||
                         name.Contains("cambiaridioma") || name.Contains("cambiarestilo"))
                {
                    SetupThemeApplier(obj, ThemeApplier.ElementType.TextTitle, false, true);
                    // AutoLocalizer ya funciona por nombre de GameObject - no agregar como componente
                    styled++;
                }
                // Título de Settings
                else if (name.Contains("settingstitle"))
                {
                    SetupThemeApplier(obj, ThemeApplier.ElementType.TextTitle, false, true);
                    styled++;
                }
                // Slider Fill
                else if (name == "fill" && obj.GetComponent<Image>() != null)
                {
                    SetupThemeApplier(obj, ThemeApplier.ElementType.SliderFill, true, false);
                    styled++;
                }
                // Slider Handle
                else if (name == "handle" && obj.GetComponent<Image>() != null)
                {
                    SetupThemeApplier(obj, ThemeApplier.ElementType.SliderHandle, true, false);
                    styled++;
                }
                // Slider Track/Background
                else if ((name == "background" || name == "track") &&
                         obj.transform.parent != null &&
                         obj.transform.parent.name.ToLower().Contains("slider"))
                {
                    SetupThemeApplier(obj, ThemeApplier.ElementType.SliderTrack, true, false);
                    styled++;
                }
                // Iconos de idioma/tema
                else if (name.Contains("languageicon") || name.Contains("themeicon") || name.Contains("icon"))
                {
                    if (obj.GetComponent<Image>() != null && !name.Contains("container"))
                    {
                        SetupThemeApplier(obj, ThemeApplier.ElementType.Accent, true, false);
                        styled++;
                    }
                }
            }

            // Aplicar colores cyan a los sliders específicamente
            ApplySliderColors();

            Debug.Log($"[SettingsNeonStyler] Applied neon style to {styled} elements");
            EditorUtility.DisplayDialog("Neon Style Applied",
                $"Se aplicó el estilo Neon a {styled} elementos.\n\nGuarda la escena para conservar los cambios.",
                "OK");
        }

        private static void SetupThemeApplier(GameObject obj, ThemeApplier.ElementType elementType, bool applyToImage, bool applyToText)
        {
            var applier = obj.GetComponent<ThemeApplier>();
            if (applier == null)
            {
                applier = obj.AddComponent<ThemeApplier>();
            }

            // Usar SerializedObject para modificar campos privados
            var so = new SerializedObject(applier);
            so.FindProperty("elementType").enumValueIndex = (int)elementType;
            so.FindProperty("applyToImage").boolValue = applyToImage;
            so.FindProperty("applyToText").boolValue = applyToText;
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(applier);
        }

        private static void ApplySliderColors()
        {
            // Buscar todos los sliders
            var sliders = GameObject.FindObjectsOfType<Slider>(true);

            foreach (var slider in sliders)
            {
                // Fill Area -> Fill
                var fillArea = slider.transform.Find("Fill Area");
                if (fillArea != null)
                {
                    var fill = fillArea.Find("Fill");
                    if (fill != null)
                    {
                        var img = fill.GetComponent<Image>();
                        if (img != null)
                        {
                            img.color = new Color(0, 1, 1, 1); // Cyan
                            EditorUtility.SetDirty(img);
                        }
                    }
                }

                // Handle Slide Area -> Handle
                var handleArea = slider.transform.Find("Handle Slide Area");
                if (handleArea != null)
                {
                    var handle = handleArea.Find("Handle");
                    if (handle != null)
                    {
                        var img = handle.GetComponent<Image>();
                        if (img != null)
                        {
                            img.color = Color.white;
                            EditorUtility.SetDirty(img);
                        }
                    }
                }

                // Background
                var bg = slider.transform.Find("Background");
                if (bg != null)
                {
                    var img = bg.GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = new Color(0.2f, 0.2f, 0.25f, 1); // Dark gray
                        EditorUtility.SetDirty(img);
                    }
                }
            }
        }

        [MenuItem("DigitPark/Set Text to Cyan (Selected)")]
        public static void SetSelectedToCyan()
        {
            foreach (var obj in Selection.gameObjects)
            {
                var tmp = obj.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.color = new Color(0, 1, 1, 1); // Cyan
                    EditorUtility.SetDirty(tmp);
                }

                var text = obj.GetComponent<Text>();
                if (text != null)
                {
                    text.color = new Color(0, 1, 1, 1); // Cyan
                    EditorUtility.SetDirty(text);
                }
            }
        }
    }
}
