#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Aplica el estilo Neon Dark directamente en el editor (visible en Scene view)
    /// Colores exactos del backup original
    /// </summary>
    public class NeonStyleApplier : EditorWindow
    {
        // ============================================
        // PALETA NEON DARK ORIGINAL (del backup)
        // ============================================

        // === FONDOS ===
        private static readonly Color BG_DEEP_SPACE = new Color(0.0196f, 0.0314f, 0.0784f, 1f);    // #050814
        private static readonly Color BG_SECONDARY = new Color(0.0314f, 0.0471f, 0.1098f, 1f);     // #080C1C
        private static readonly Color BG_CARD = new Color(0.0627f, 0.0824f, 0.1569f, 1f);          // #101528
        private static readonly Color BG_SPOTLIGHT = new Color(0.0784f, 0.0941f, 0.2f, 1f);        // #141833

        // === NEON COLORS ===
        private static readonly Color NEON_CYAN = new Color(0f, 0.9608f, 1f, 1f);                  // #00F5FF
        private static readonly Color NEON_PURPLE = new Color(0.6157f, 0.2941f, 1f, 1f);           // #9D4BFF
        private static readonly Color NEON_GREEN = new Color(0.2353f, 1f, 0.4196f, 1f);            // #3CFF6B
        private static readonly Color NEON_PINK = new Color(1f, 0.1804f, 0.6235f, 1f);             // #FF2E9F
        private static readonly Color NEON_YELLOW = new Color(1f, 0.7882f, 0.2784f, 1f);           // #FFC947

        // === TEXTO ===
        private static readonly Color TEXT_PRIMARY = Color.white;                                   // #FFFFFF
        private static readonly Color TEXT_SECONDARY = new Color(0.7686f, 0.8f, 1f, 1f);           // #C4CCFF
        private static readonly Color TEXT_DISABLED = new Color(0.4353f, 0.451f, 0.6f, 1f);        // #6F7399
        private static readonly Color TEXT_ON_NEON = new Color(0.02f, 0.03f, 0.08f, 1f);           // Oscuro para contraste

        // === ESTADOS ===
        private static readonly Color ERROR = new Color(1f, 0.2f, 0.4f, 1f);                       // #FF3366
        private static readonly Color BORDER = new Color(0.1255f, 0.1647f, 0.2902f, 1f);           // #202A4A

        // === BOTONES SECUNDARIOS ===
        private static readonly Color BTN_SECONDARY = new Color(0.165f, 0.165f, 0.227f, 1f);       // #2A2A3A

        // ============================================
        // ESCENAS ORIGINALES (no las nuevas)
        // ============================================
        private static readonly string[] ORIGINAL_SCENES = new string[]
        {
            "Assets/_Project/Scenes/Login.unity",
            "Assets/_Project/Scenes/Register.unity",
            "Assets/_Project/Scenes/MainMenu.unity",
            "Assets/_Project/Scenes/Profile.unity",
            "Assets/_Project/Scenes/Scores.unity",
            "Assets/_Project/Scenes/SearchPlayers.unity",
            "Assets/_Project/Scenes/Settings.unity",
            "Assets/_Project/Scenes/Tournaments.unity"
        };

        // ============================================
        // MENU ITEMS
        // ============================================

        [MenuItem("DigitPark/Neon Style/Apply to Current Scene")]
        public static void ApplyToCurrentScene()
        {
            string sceneName = EditorSceneManager.GetActiveScene().name;
            int count = ApplyNeonStyle();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"[NeonStyle] {sceneName}: {count} elementos. Guarda con Ctrl+S");
        }

        [MenuItem("DigitPark/Neon Style/Apply to ORIGINAL Scenes Only")]
        public static void ApplyToOriginalScenes()
        {
            if (!EditorUtility.DisplayDialog("Confirmar",
                "Aplicar estilo Neon a las 8 escenas ORIGINALES?\n\n(No incluye las escenas nuevas de Games)",
                "Si", "Cancelar"))
                return;

            int totalCount = 0;

            foreach (string scenePath in ORIGINAL_SCENES)
            {
                try
                {
                    var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    int count = ApplyNeonStyle();
                    totalCount += count;
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"[NeonStyle] {scene.name}: {count} elementos");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[NeonStyle] Error en {scenePath}: {e.Message}");
                }
            }

            EditorUtility.DisplayDialog("Completado",
                $"Estilo Neon aplicado a {ORIGINAL_SCENES.Length} escenas.\n\nTotal: {totalCount} elementos",
                "OK");
        }

        // ============================================
        // APLICAR ESTILO
        // ============================================

        private static int ApplyNeonStyle()
        {
            int count = 0;

            count += ApplyBackgrounds();
            count += ApplyTexts();
            count += ApplyButtons();
            count += ApplyInputFields();
            count += ApplySliders();
            count += ApplyToggles();

            return count;
        }

        // ============================================
        // FONDOS
        // ============================================

        private static int ApplyBackgrounds()
        {
            int count = 0;
            var images = Object.FindObjectsOfType<Image>(true);

            foreach (var img in images)
            {
                // Saltar botones
                if (img.GetComponent<Button>() != null) continue;

                string name = img.gameObject.name.ToLower();

                // Proteger elementos especiales
                if (IsProtected(name)) continue;

                // Fondo principal
                if (name.Contains("background") || name == "bg" || name.Contains("mainbg"))
                {
                    img.color = BG_DEEP_SPACE;
                    EditorUtility.SetDirty(img);
                    count++;
                }
                // Headers
                else if (name.Contains("header") || name.Contains("topbar") || name.Contains("navbar"))
                {
                    img.color = BG_SPOTLIGHT;
                    EditorUtility.SetDirty(img);
                    count++;
                }
                // Paneles/Cards
                else if (name.Contains("panel") || name.Contains("card") || name.Contains("container") || name.Contains("popup"))
                {
                    img.color = BG_CARD;
                    EditorUtility.SetDirty(img);
                    count++;
                }
                // Overlays
                else if (name.Contains("overlay") || name.Contains("blocker"))
                {
                    img.color = new Color(0, 0, 0, 0.9f);
                    EditorUtility.SetDirty(img);
                    count++;
                }
            }

            return count;
        }

        // ============================================
        // TEXTOS
        // ============================================

        private static int ApplyTexts()
        {
            int count = 0;
            var texts = Object.FindObjectsOfType<TextMeshProUGUI>(true);

            foreach (var text in texts)
            {
                string name = text.gameObject.name.ToLower();
                string parentName = text.transform.parent?.name.ToLower() ?? "";

                // Saltar texto de botones
                if (parentName.Contains("button") || parentName.Contains("btn")) continue;

                // Placeholders
                if (name.Contains("placeholder"))
                {
                    text.color = TEXT_DISABLED;
                    EditorUtility.SetDirty(text);
                    count++;
                    continue;
                }

                // Titulos - CYAN
                if (name.Contains("title") || name.Contains("header") || name.Contains("titulo"))
                {
                    text.color = NEON_CYAN;
                    EditorUtility.SetDirty(text);
                    count++;
                    continue;
                }

                // Valores/Scores - CYAN
                if (name.Contains("value") || name.Contains("score") || name.Contains("count") || name.Contains("number"))
                {
                    text.color = NEON_CYAN;
                    EditorUtility.SetDirty(text);
                    count++;
                    continue;
                }

                // Labels secundarios
                if (name.Contains("label") || name.Contains("subtitle") || name.Contains("desc"))
                {
                    text.color = TEXT_SECONDARY;
                    EditorUtility.SetDirty(text);
                    count++;
                    continue;
                }

                // Errores
                if (name.Contains("error"))
                {
                    text.color = ERROR;
                    EditorUtility.SetDirty(text);
                    count++;
                    continue;
                }

                // Resto - blanco
                text.color = TEXT_PRIMARY;
                EditorUtility.SetDirty(text);
                count++;
            }

            return count;
        }

        // ============================================
        // BOTONES
        // ============================================

        private static int ApplyButtons()
        {
            int count = 0;
            var buttons = Object.FindObjectsOfType<Button>(true);

            foreach (var btn in buttons)
            {
                string name = btn.gameObject.name.ToLower();

                // === PROTEGIDOS ===
                if (name.Contains("apple") || name.Contains("google") || name.Contains("social")) continue;

                // Determinar tipo
                Color btnColor = NEON_CYAN;
                Color hoverColor = new Color(0.4f, 1f, 1f, 1f);
                Color pressedColor = new Color(0f, 0.75f, 0.8f, 1f);
                Color textColor = TEXT_ON_NEON;

                // Botones de peligro
                if (name.Contains("delete") || name.Contains("logout") || name.Contains("remove") || name.Contains("salir"))
                {
                    btnColor = ERROR;
                    hoverColor = new Color(1f, 0.4f, 0.5f, 1f);
                    pressedColor = new Color(0.8f, 0.15f, 0.3f, 1f);
                    textColor = TEXT_PRIMARY;
                }
                // Botones secundarios
                else if (name.Contains("back") || name.Contains("close") || name.Contains("cancel") || name.Contains("volver"))
                {
                    btnColor = BTN_SECONDARY;
                    hoverColor = new Color(0.22f, 0.22f, 0.3f, 1f);
                    pressedColor = new Color(0.12f, 0.12f, 0.18f, 1f);
                    textColor = TEXT_PRIMARY;
                }
                // Botones de exito
                else if (name.Contains("confirm") || name.Contains("accept") || name.Contains("save"))
                {
                    btnColor = NEON_GREEN;
                    hoverColor = new Color(0.4f, 1f, 0.55f, 1f);
                    pressedColor = new Color(0.18f, 0.8f, 0.33f, 1f);
                    textColor = TEXT_ON_NEON;
                }

                // Aplicar color a la imagen
                var img = btn.GetComponent<Image>();
                if (img != null)
                {
                    img.color = btnColor;
                    EditorUtility.SetDirty(img);
                }

                // Aplicar ColorBlock
                ColorBlock colors = btn.colors;
                colors.normalColor = btnColor;
                colors.highlightedColor = hoverColor;
                colors.pressedColor = pressedColor;
                colors.selectedColor = hoverColor;
                colors.disabledColor = new Color(btnColor.r * 0.4f, btnColor.g * 0.4f, btnColor.b * 0.4f, 0.5f);
                btn.colors = colors;
                EditorUtility.SetDirty(btn);

                // Texto del boton
                var btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.color = textColor;
                    EditorUtility.SetDirty(btnText);
                }

                count++;
            }

            return count;
        }

        // ============================================
        // INPUT FIELDS
        // ============================================

        private static int ApplyInputFields()
        {
            int count = 0;
            var inputs = Object.FindObjectsOfType<TMP_InputField>(true);

            foreach (var input in inputs)
            {
                // Fondo
                var bg = input.GetComponent<Image>();
                if (bg != null)
                {
                    bg.color = BG_SECONDARY;
                    EditorUtility.SetDirty(bg);
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
                    var ph = input.placeholder as TextMeshProUGUI;
                    if (ph != null)
                    {
                        ph.color = TEXT_DISABLED;
                        EditorUtility.SetDirty(ph);
                    }
                }

                // Caret
                input.caretColor = NEON_CYAN;
                input.selectionColor = new Color(NEON_CYAN.r, NEON_CYAN.g, NEON_CYAN.b, 0.3f);
                EditorUtility.SetDirty(input);

                count++;
            }

            return count;
        }

        // ============================================
        // SLIDERS
        // ============================================

        private static int ApplySliders()
        {
            int count = 0;
            var sliders = Object.FindObjectsOfType<Slider>(true);

            foreach (var slider in sliders)
            {
                // Track
                var bg = slider.transform.Find("Background")?.GetComponent<Image>();
                if (bg != null)
                {
                    bg.color = BTN_SECONDARY;
                    EditorUtility.SetDirty(bg);
                }

                // Fill - CYAN
                var fill = slider.fillRect?.GetComponent<Image>();
                if (fill != null)
                {
                    fill.color = NEON_CYAN;
                    EditorUtility.SetDirty(fill);
                }

                // Handle - Blanco
                var handle = slider.handleRect?.GetComponent<Image>();
                if (handle != null)
                {
                    handle.color = TEXT_PRIMARY;
                    EditorUtility.SetDirty(handle);
                }

                count++;
            }

            return count;
        }

        // ============================================
        // TOGGLES
        // ============================================

        private static int ApplyToggles()
        {
            int count = 0;
            var toggles = Object.FindObjectsOfType<Toggle>(true);

            foreach (var toggle in toggles)
            {
                // Background
                var bg = toggle.targetGraphic as Image;
                if (bg != null)
                {
                    bg.color = BTN_SECONDARY;
                    EditorUtility.SetDirty(bg);
                }

                // Checkmark - CYAN
                if (toggle.graphic != null)
                {
                    var checkmark = toggle.graphic as Image;
                    if (checkmark != null)
                    {
                        checkmark.color = NEON_CYAN;
                        EditorUtility.SetDirty(checkmark);
                    }
                }

                count++;
            }

            return count;
        }

        // ============================================
        // UTILIDADES
        // ============================================

        private static bool IsProtected(string name)
        {
            return name.Contains("icon") || name.Contains("logo") || name.Contains("avatar") ||
                   name.Contains("image") || name.Contains("sprite") || name.Contains("photo") ||
                   name.Contains("flag") || name.Contains("badge") || name.Contains("apple") ||
                   name.Contains("google");
        }
    }
}
#endif
