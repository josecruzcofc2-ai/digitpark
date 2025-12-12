using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor tool para aplicar el tema Neon Gamer a todas las escenas
    /// </summary>
    public class NeonThemeApplier : EditorWindow
    {
        // === COLORES NEON GAMER ===

        // Fondos
        private static readonly Color BG_DEEP_SPACE = new Color(0.0196f, 0.0314f, 0.0784f, 1f); // #050814
        private static readonly Color BG_SECONDARY = new Color(0.0314f, 0.0471f, 0.1098f, 1f); // #080C1C
        private static readonly Color BG_CARD = new Color(0.0627f, 0.0824f, 0.1569f, 1f); // #101528
        private static readonly Color BG_SPOTLIGHT = new Color(0.0784f, 0.0941f, 0.2f, 1f); // #141833

        // Neón
        private static readonly Color NEON_CYAN = new Color(0f, 0.9608f, 1f, 1f); // #00F5FF
        private static readonly Color NEON_PURPLE = new Color(0.6157f, 0.2941f, 1f, 1f); // #9D4BFF
        private static readonly Color NEON_GREEN = new Color(0.2353f, 1f, 0.4196f, 1f); // #3CFF6B
        private static readonly Color NEON_PINK = new Color(1f, 0.1804f, 0.6235f, 1f); // #FF2E9F
        private static readonly Color NEON_YELLOW = new Color(1f, 0.7882f, 0.2784f, 1f); // #FFC947

        // Texto
        private static readonly Color TEXT_PRIMARY = new Color(1f, 1f, 1f, 1f); // #FFFFFF
        private static readonly Color TEXT_SECONDARY = new Color(0.7686f, 0.8f, 1f, 1f); // #C4CCFF
        private static readonly Color TEXT_DISABLED = new Color(0.4353f, 0.451f, 0.6f, 1f); // #6F7399

        // Estados
        private static readonly Color ERROR_COLOR = new Color(1f, 0.2f, 0.4f, 1f); // #FF3366
        private static readonly Color BORDER_NORMAL = new Color(0.1255f, 0.1647f, 0.2902f, 1f); // #202A4A

        // Rankings
        private static readonly Color GOLD = new Color(1f, 0.7882f, 0.2784f, 1f); // #FFC947
        private static readonly Color SILVER = new Color(0.7529f, 0.7529f, 0.7529f, 1f); // #C0C0C0
        private static readonly Color BRONZE = new Color(0.8039f, 0.498f, 0.1961f, 1f); // #CD7F32

        [MenuItem("DigitPark/Theme/Apply Neon Theme to Current Scene")]
        public static void ApplyToCurrentScene()
        {
            ApplyNeonTheme();
            Debug.Log("[NeonTheme] Tema aplicado a la escena actual");
        }

        [MenuItem("DigitPark/Theme/Apply Neon Theme to ALL Scenes")]
        public static void ApplyToAllScenes()
        {
            string[] scenePaths = new string[]
            {
                "Assets/_Project/Scenes/Boot.unity",
                "Assets/_Project/Scenes/Login.unity",
                "Assets/_Project/Scenes/Register.unity",
                "Assets/_Project/Scenes/MainMenu.unity",
                "Assets/_Project/Scenes/Game.unity",
                "Assets/_Project/Scenes/Scores.unity",
                "Assets/_Project/Scenes/Tournaments.unity",
                "Assets/_Project/Scenes/Settings.unity"
            };

            foreach (string scenePath in scenePaths)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                ApplyNeonTheme();
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[NeonTheme] Tema aplicado a: {scenePath}");
            }

            Debug.Log("[NeonTheme] ¡Tema Neon Gamer aplicado a TODAS las escenas!");
        }

        private static void ApplyNeonTheme()
        {
            // Aplicar a todas las imágenes
            ApplyToImages();

            // Aplicar a todos los textos
            ApplyToTexts();

            // Aplicar a todos los botones
            ApplyToButtons();

            // Aplicar a todos los input fields
            ApplyToInputFields();

            // Aplicar a todos los sliders
            ApplyToSliders();

            // Aplicar a todos los toggles
            ApplyToToggles();

            // Marcar escena como modificada
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private static void ApplyToImages()
        {
            var images = GameObject.FindObjectsOfType<Image>(true);
            foreach (var img in images)
            {
                string name = img.gameObject.name.ToLower();
                bool modified = false;

                // === FONDOS PRINCIPALES ===
                if (name.Contains("background") || name == "bg")
                {
                    string parentName = img.transform.parent != null ? img.transform.parent.name.ToLower() : "";
                    if (name.Contains("main") || name.Contains("screen") || parentName == "canvas")
                    {
                        img.color = BG_DEEP_SPACE;
                        modified = true;
                    }
                    else if (name.Contains("card") || name.Contains("panel") ||
                             name.Contains("popup") || name.Contains("confirm") ||
                             name.Contains("error"))
                    {
                        img.color = BG_CARD;
                        modified = true;
                    }
                    else
                    {
                        img.color = BG_SECONDARY;
                        modified = true;
                    }
                }
                // === BLOCKER PANELS ===
                else if (name.Contains("blocker"))
                {
                    img.color = new Color(BG_DEEP_SPACE.r, BG_DEEP_SPACE.g, BG_DEEP_SPACE.b, 0.9f);
                    modified = true;
                }
                // === PANELES ===
                else if (name.Contains("panel"))
                {
                    img.color = BG_CARD;
                    modified = true;
                }
                // === INPUT FIELDS ===
                else if (name.Contains("inputfield") || name.Contains("input field") ||
                         (img.transform.parent != null && img.transform.parent.name.ToLower().Contains("input")))
                {
                    img.color = BG_CARD;
                    modified = true;
                }
                // === TABS ===
                else if (name.Contains("tab"))
                {
                    img.color = BG_CARD;
                    modified = true;
                }

                if (modified)
                {
                    EditorUtility.SetDirty(img);
                }
            }
        }

        private static void ApplyToTexts()
        {
            var texts = GameObject.FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (var text in texts)
            {
                string name = text.gameObject.name.ToLower();
                bool modified = false;

                // === TÍTULOS ===
                if (name.Contains("title") || name.Contains("header") || name.Contains("logo"))
                {
                    text.color = NEON_CYAN;
                    modified = true;
                }
                // === PLACEHOLDERS ===
                else if (name.Contains("placeholder"))
                {
                    text.color = TEXT_DISABLED;
                    modified = true;
                }
                // === LABELS SECUNDARIOS ===
                else if (name.Contains("label") || name.Contains("subtitle") || name.Contains("info"))
                {
                    text.color = TEXT_SECONDARY;
                    modified = true;
                }
                // === ERROR ===
                else if (name.Contains("error"))
                {
                    text.color = ERROR_COLOR;
                    modified = true;
                }
                // === SUCCESS ===
                else if (name.Contains("success"))
                {
                    text.color = NEON_GREEN;
                    modified = true;
                }
                // === VALORES / DATOS ===
                else if (name.Contains("value") || name.Contains("time") || name.Contains("score"))
                {
                    text.color = NEON_CYAN;
                    modified = true;
                }
                // === TEXTO EN BOTONES - será manejado por ApplyToButtons ===
                else if (text.transform.parent?.GetComponent<Button>() == null)
                {
                    // Texto general
                    text.color = TEXT_PRIMARY;
                    modified = true;
                }

                if (modified)
                {
                    EditorUtility.SetDirty(text);
                }
            }
        }

        private static void ApplyToButtons()
        {
            var buttons = GameObject.FindObjectsOfType<Button>(true);
            foreach (var btn in buttons)
            {
                string name = btn.gameObject.name.ToLower();
                var image = btn.GetComponent<Image>();
                var text = btn.GetComponentInChildren<TextMeshProUGUI>();

                // === BOTONES PRINCIPALES (Acción) ===
                if (name.Contains("login") || name.Contains("play") || name.Contains("confirm") ||
                    name.Contains("create") || name.Contains("join") || name.Contains("submit") ||
                    name.Contains("register") || name.Contains("signin") || name.Contains("save") ||
                    name.Contains("accept"))
                {
                    if (image != null)
                    {
                        image.color = NEON_CYAN;
                        EditorUtility.SetDirty(image);
                    }

                    // ColorBlock para estados
                    var colors = btn.colors;
                    colors.normalColor = Color.white;
                    colors.highlightedColor = new Color(1f, 1f, 1f, 0.85f);
                    colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
                    colors.selectedColor = Color.white;
                    colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                    btn.colors = colors;

                    if (text != null)
                    {
                        text.color = BG_DEEP_SPACE;
                        text.fontStyle = FontStyles.Bold;
                        EditorUtility.SetDirty(text);
                    }
                }
                // === BOTÓN PLAY ESPECIAL (Verde) ===
                else if (name == "playbutton" || name == "play button" || name == "btnplay")
                {
                    if (image != null)
                    {
                        image.color = NEON_GREEN;
                        EditorUtility.SetDirty(image);
                    }

                    if (text != null)
                    {
                        text.color = BG_DEEP_SPACE;
                        text.fontStyle = FontStyles.Bold;
                        EditorUtility.SetDirty(text);
                    }
                }
                // === BOTONES DE PELIGRO ===
                else if (name.Contains("delete") || name.Contains("logout") || name.Contains("exit") ||
                         name.Contains("remove"))
                {
                    if (image != null)
                    {
                        image.color = BG_CARD;
                        EditorUtility.SetDirty(image);
                    }

                    if (text != null)
                    {
                        text.color = ERROR_COLOR;
                        EditorUtility.SetDirty(text);
                    }
                }
                // === BOTONES SECUNDARIOS / NAVEGACIÓN ===
                else if (name.Contains("back") || name.Contains("cancel") || name.Contains("close") ||
                         name.Contains("settings") || name.Contains("scores") || name.Contains("tournament") ||
                         name.Contains("google") || name.Contains("guest") || name.Contains("social"))
                {
                    if (image != null)
                    {
                        image.color = BG_CARD;
                        EditorUtility.SetDirty(image);
                    }

                    var colors = btn.colors;
                    colors.normalColor = Color.white;
                    colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
                    colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
                    btn.colors = colors;

                    if (text != null)
                    {
                        text.color = TEXT_SECONDARY;
                        EditorUtility.SetDirty(text);
                    }
                }
                // === BOTONES GENÉRICOS ===
                else
                {
                    if (image != null)
                    {
                        image.color = BG_CARD;
                        EditorUtility.SetDirty(image);
                    }

                    if (text != null)
                    {
                        text.color = TEXT_PRIMARY;
                        EditorUtility.SetDirty(text);
                    }
                }

                EditorUtility.SetDirty(btn);
            }
        }

        private static void ApplyToInputFields()
        {
            var inputFields = GameObject.FindObjectsOfType<TMP_InputField>(true);
            foreach (var input in inputFields)
            {
                // Fondo
                var image = input.GetComponent<Image>();
                if (image != null)
                {
                    image.color = BG_CARD;
                    EditorUtility.SetDirty(image);
                }

                // Texto
                if (input.textComponent != null)
                {
                    input.textComponent.color = TEXT_PRIMARY;
                    EditorUtility.SetDirty(input.textComponent);
                }

                // Placeholder
                if (input.placeholder != null)
                {
                    var placeholderText = input.placeholder as TextMeshProUGUI;
                    if (placeholderText != null)
                    {
                        placeholderText.color = TEXT_DISABLED;
                        EditorUtility.SetDirty(placeholderText);
                    }
                }

                // Caret y selección
                input.caretColor = NEON_CYAN;
                input.selectionColor = new Color(NEON_CYAN.r, NEON_CYAN.g, NEON_CYAN.b, 0.3f);

                EditorUtility.SetDirty(input);
            }
        }

        private static void ApplyToSliders()
        {
            var sliders = GameObject.FindObjectsOfType<Slider>(true);
            foreach (var slider in sliders)
            {
                // Background
                var background = slider.transform.Find("Background")?.GetComponent<Image>();
                if (background != null)
                {
                    background.color = BORDER_NORMAL;
                    EditorUtility.SetDirty(background);
                }

                // Fill
                if (slider.fillRect != null)
                {
                    var fill = slider.fillRect.GetComponent<Image>();
                    if (fill != null)
                    {
                        fill.color = NEON_CYAN;
                        EditorUtility.SetDirty(fill);
                    }
                }

                // Handle
                if (slider.handleRect != null)
                {
                    var handle = slider.handleRect.GetComponent<Image>();
                    if (handle != null)
                    {
                        handle.color = NEON_CYAN;
                        EditorUtility.SetDirty(handle);
                    }
                }

                EditorUtility.SetDirty(slider);
            }
        }

        private static void ApplyToToggles()
        {
            var toggles = GameObject.FindObjectsOfType<Toggle>(true);
            foreach (var toggle in toggles)
            {
                // Background
                var background = toggle.targetGraphic as Image;
                if (background != null)
                {
                    background.color = BG_CARD;
                    EditorUtility.SetDirty(background);
                }

                // Checkmark
                if (toggle.graphic != null)
                {
                    var checkmark = toggle.graphic as Image;
                    if (checkmark != null)
                    {
                        checkmark.color = NEON_CYAN;
                        EditorUtility.SetDirty(checkmark);
                    }
                }

                // Texto
                var text = toggle.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.color = TEXT_PRIMARY;
                    EditorUtility.SetDirty(text);
                }

                // Color states
                var colors = toggle.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
                colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
                toggle.colors = colors;

                EditorUtility.SetDirty(toggle);
            }
        }

        [MenuItem("DigitPark/Theme/Open Theme Window")]
        public static void ShowWindow()
        {
            GetWindow<NeonThemeApplier>("Neon Theme Applier");
        }

        private void OnGUI()
        {
            GUILayout.Label("🎨 Neon Gamer Theme Applier", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.Label("Colores del Tema:", EditorStyles.boldLabel);
            EditorGUILayout.ColorField("BG Deep Space", BG_DEEP_SPACE);
            EditorGUILayout.ColorField("BG Card", BG_CARD);
            EditorGUILayout.ColorField("Neon Cyan", NEON_CYAN);
            EditorGUILayout.ColorField("Neon Purple", NEON_PURPLE);
            EditorGUILayout.ColorField("Neon Green", NEON_GREEN);
            EditorGUILayout.ColorField("Neon Pink", NEON_PINK);
            EditorGUILayout.ColorField("Neon Yellow", NEON_YELLOW);

            GUILayout.Space(20);

            if (GUILayout.Button("🎯 Aplicar a Escena Actual", GUILayout.Height(40)))
            {
                ApplyToCurrentScene();
            }

            GUILayout.Space(10);

            GUI.backgroundColor = NEON_GREEN;
            if (GUILayout.Button("🚀 APLICAR A TODAS LAS ESCENAS", GUILayout.Height(50)))
            {
                if (EditorUtility.DisplayDialog("Aplicar Tema Neon",
                    "¿Estás seguro de aplicar el tema Neon Gamer a TODAS las escenas?\n\nEsto modificará permanentemente los colores de todos los elementos UI.",
                    "¡Sí, aplicar!", "Cancelar"))
                {
                    ApplyToAllScenes();
                }
            }
            GUI.backgroundColor = Color.white;
        }
    }
}
