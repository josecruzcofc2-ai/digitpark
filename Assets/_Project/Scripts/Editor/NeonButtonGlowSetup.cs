using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using DigitPark.UI.Components;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor tool to add NeonButtonGlow to UI buttons in scenes
    /// Automatically detects and SKIPS gameplay buttons (grids, cards, game elements)
    /// </summary>
    public class NeonButtonGlowSetup : EditorWindow
    {
        private Vector2 scrollPosition;
        private Vector2 skippedScrollPosition;
        private bool includeInactiveButtons = true;
        private bool overwriteExisting = false;
        private bool showSkippedButtons = false;
        private List<ButtonInfo> uiButtons = new List<ButtonInfo>();
        private List<ButtonInfo> skippedButtons = new List<ButtonInfo>();

        private class ButtonInfo
        {
            public Button button;
            public string name;
            public string fullPath;
            public NeonButtonGlow.GlowStyle suggestedStyle;
            public bool hasGlow;
            public bool selected;
            public string skipReason;
        }

        // Scenes that have ONLY UI buttons (safe to add glow to all)
        private static readonly string[] UI_ONLY_SCENES = new string[]
        {
            "Assets/_Project/Scenes/Boot.unity",
            "Assets/_Project/Scenes/Login.unity",
            "Assets/_Project/Scenes/Register.unity",
            "Assets/_Project/Scenes/MainMenu.unity",
            "Assets/_Project/Scenes/Scores.unity",
            "Assets/_Project/Scenes/Settings.unity",
            "Assets/_Project/Scenes/Tournaments.unity",
            "Assets/_Project/Scenes/HowToPlay.unity",
            "Assets/_Project/Scenes/CountrySelector.unity",
            "Assets/_Project/Scenes/Profile.unity",
            "Assets/_Project/Scenes/SearchPlayers.unity",
            "Assets/_Project/Scenes/CashBattle.unity",
            "Assets/_Project/Scenes/Games/GameSelector.unity"
        };

        // Game scenes - need careful filtering
        private static readonly string[] GAME_SCENES = new string[]
        {
            "Assets/_Project/Scenes/Games/DigitRush.unity",
            "Assets/_Project/Scenes/Games/MemoryPairs.unity",
            "Assets/_Project/Scenes/Games/QuickMath.unity",
            "Assets/_Project/Scenes/Games/FlashTap.unity",
            "Assets/_Project/Scenes/Games/OddOneOut.unity"
        };

        // Parent object names that indicate gameplay area (skip buttons inside these)
        private static readonly string[] GAMEPLAY_PARENT_NAMES = new string[]
        {
            "grid", "board", "playarea", "play_area", "gamearea", "game_area",
            "cards", "cardgrid", "memorygrid", "numbergrid", "digitgrid",
            "leftgrid", "rightgrid", "answerpanel", "answerscontainer",
            "taparea", "flasharea", "buttonsgrid", "numbersgrid"
        };

        // Button names that are DEFINITELY gameplay (skip these)
        private static readonly string[] GAMEPLAY_BUTTON_PATTERNS = new string[]
        {
            // Numbered buttons (DigitRush 1-9, QuickMath answers)
            "button_", "btn_", "cell_", "card_", "tile_",
            // Array-style naming
            "button (", "cell (", "card (", "element (",
            // Specific game elements
            "numberbutton", "digitbutton", "answerbutton", "optionbutton",
            "memorycard", "paircard", "gridbutton", "gridcell",
            "tapbutton", "flashbutton", "targetbutton",
            "leftbutton", "rightbutton"
        };

        // Button names that are DEFINITELY UI (always include)
        private static readonly string[] UI_BUTTON_PATTERNS = new string[]
        {
            "back", "menu", "pause", "resume", "restart", "quit", "exit", "return",
            "play", "start", "begin", "continue", "jugar", "iniciar", "empezar",
            "settings", "options", "config", "ajustes", "opciones",
            "home", "main", "inicio", "principal",
            "confirm", "cancel", "ok", "yes", "no", "accept", "decline", "aceptar", "cancelar",
            "next", "prev", "previous", "skip", "siguiente", "anterior",
            "login", "register", "signup", "signin", "logout", "ingresar", "registrar",
            "save", "load", "apply", "reset", "guardar", "cargar", "aplicar",
            "close", "open", "show", "hide", "cerrar", "abrir", "mostrar", "ocultar",
            "buy", "purchase", "shop", "store", "cash", "premium", "comprar", "tienda",
            "share", "invite", "friend", "compartir", "invitar", "amigo",
            "retry", "again", "replay", "reintentar", "otra vez",
            "rules", "help", "info", "reglas", "ayuda"
        };

        [MenuItem("DigitPark/UI/Add Neon Glow to Buttons")]
        public static void ShowWindow()
        {
            var window = GetWindow<NeonButtonGlowSetup>("Neon Button Glow Setup");
            window.minSize = new Vector2(550, 700);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("Neon Button Glow Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "This tool adds NeonButtonGlow to UI buttons ONLY.\n\n" +
                "AUTOMATICALLY SKIPS:\n" +
                "- Grid buttons (DigitRush 3x3, OddOneOut 4x4)\n" +
                "- Card buttons (MemoryPairs)\n" +
                "- Answer buttons (QuickMath)\n" +
                "- Tap button (FlashTap)\n" +
                "- Any button inside GridLayoutGroup\n\n" +
                "INCLUDES: Back, Play, Menu, Settings, etc.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Options
            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
            includeInactiveButtons = EditorGUILayout.Toggle("Include Inactive Buttons", includeInactiveButtons);
            overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing Glow", overwriteExisting);

            EditorGUILayout.Space(10);

            // Current Scene Actions
            EditorGUILayout.LabelField("Current Scene", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Scan Buttons", GUILayout.Height(25)))
            {
                ScanCurrentScene();
            }

            using (new EditorGUI.DisabledGroupScope(uiButtons.Count == 0))
            {
                if (GUILayout.Button($"Add Glow to UI Buttons ({uiButtons.Count(b => !b.hasGlow)})", GUILayout.Height(25)))
                {
                    AddGlowToUIButtons();
                }
            }
            EditorGUILayout.EndHorizontal();

            // UI Buttons list
            if (uiButtons.Count > 0)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField($"UI Buttons (will receive glow): {uiButtons.Count}", EditorStyles.boldLabel);

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(180));

                foreach (var info in uiButtons)
                {
                    EditorGUILayout.BeginHorizontal();

                    info.selected = EditorGUILayout.Toggle(info.selected, GUILayout.Width(20));

                    GUI.color = info.hasGlow ? Color.green : Color.yellow;
                    EditorGUILayout.LabelField(info.hasGlow ? "[OK]" : "[--]", GUILayout.Width(35));
                    GUI.color = Color.white;

                    EditorGUILayout.LabelField(info.name, GUILayout.Width(150));
                    EditorGUILayout.LabelField($"-> {info.suggestedStyle}", GUILayout.Width(90));

                    if (GUILayout.Button("Select", GUILayout.Width(50)))
                    {
                        Selection.activeGameObject = info.button.gameObject;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();

                if (GUILayout.Button("Add Glow to Selected", GUILayout.Height(22)))
                {
                    AddGlowToSelected();
                }
            }

            // Skipped Buttons list (collapsible)
            if (skippedButtons.Count > 0)
            {
                EditorGUILayout.Space(10);
                showSkippedButtons = EditorGUILayout.Foldout(showSkippedButtons,
                    $"Skipped Gameplay Buttons: {skippedButtons.Count} (click to expand)");

                if (showSkippedButtons)
                {
                    skippedScrollPosition = EditorGUILayout.BeginScrollView(skippedScrollPosition, GUILayout.Height(120));

                    foreach (var info in skippedButtons)
                    {
                        EditorGUILayout.BeginHorizontal();

                        GUI.color = Color.gray;
                        EditorGUILayout.LabelField("[SKIP]", GUILayout.Width(45));
                        EditorGUILayout.LabelField(info.name, GUILayout.Width(120));
                        EditorGUILayout.LabelField(info.skipReason, GUILayout.Width(150));
                        GUI.color = Color.white;

                        if (GUILayout.Button("View", GUILayout.Width(45)))
                        {
                            Selection.activeGameObject = info.button.gameObject;
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndScrollView();
                }
            }

            EditorGUILayout.Space(20);

            // All Scenes Actions
            EditorGUILayout.LabelField("All Scenes (Safe Mode)", EditorStyles.boldLabel);

            GUI.backgroundColor = new Color(0.6f, 1f, 0.6f);
            if (GUILayout.Button("ADD NEON GLOW TO ALL UI BUTTONS\n(Skips gameplay buttons automatically)", GUILayout.Height(45)))
            {
                if (EditorUtility.DisplayDialog("Confirm",
                    "This will add NeonButtonGlow to UI buttons in all scenes.\n\n" +
                    "WILL SKIP:\n" +
                    "- DigitRush: 9 number buttons\n" +
                    "- MemoryPairs: 16 card buttons\n" +
                    "- OddOneOut: 32 grid buttons\n" +
                    "- QuickMath: 3 answer buttons\n" +
                    "- FlashTap: tap button\n\n" +
                    "Continue?",
                    "Yes, Add to UI Buttons Only", "Cancel"))
                {
                    AddGlowToAllScenes();
                }
            }
            GUI.backgroundColor = Color.white;
        }

        private void ScanCurrentScene()
        {
            uiButtons.Clear();
            skippedButtons.Clear();

            var allButtons = Object.FindObjectsOfType<Button>(includeInactiveButtons);
            string currentScene = EditorSceneManager.GetActiveScene().name;
            bool isGameScene = GAME_SCENES.Any(s => s.Contains(currentScene));

            foreach (var button in allButtons)
            {
                string skipReason;
                bool shouldSkip = ShouldSkipButton(button, isGameScene, out skipReason);

                var info = new ButtonInfo
                {
                    button = button,
                    name = button.name,
                    fullPath = GetFullPath(button.transform),
                    suggestedStyle = GetSuggestedStyleByColor(button),
                    hasGlow = button.GetComponent<NeonButtonGlow>() != null,
                    selected = true,
                    skipReason = skipReason
                };

                if (shouldSkip)
                {
                    skippedButtons.Add(info);
                }
                else
                {
                    uiButtons.Add(info);
                }
            }

            uiButtons = uiButtons.OrderBy(b => b.name).ToList();
            skippedButtons = skippedButtons.OrderBy(b => b.name).ToList();
            Repaint();
        }

        private bool ShouldSkipButton(Button button, bool isGameScene, out string reason)
        {
            string buttonName = button.name.ToLower();
            string fullPath = GetFullPath(button.transform).ToLower();

            // Check if button is inside a GridLayoutGroup (usually gameplay)
            if (button.GetComponentInParent<GridLayoutGroup>() != null)
            {
                // But allow if it matches UI patterns
                if (!MatchesAnyPattern(buttonName, UI_BUTTON_PATTERNS))
                {
                    reason = "Inside GridLayout";
                    return true;
                }
            }

            // Check if parent name suggests gameplay area
            foreach (var parentPattern in GAMEPLAY_PARENT_NAMES)
            {
                if (fullPath.Contains(parentPattern))
                {
                    if (!MatchesAnyPattern(buttonName, UI_BUTTON_PATTERNS))
                    {
                        reason = $"Parent: {parentPattern}";
                        return true;
                    }
                }
            }

            // Check if button name matches gameplay patterns
            foreach (var pattern in GAMEPLAY_BUTTON_PATTERNS)
            {
                if (buttonName.Contains(pattern) || buttonName.StartsWith(pattern))
                {
                    reason = $"Name: {pattern}";
                    return true;
                }
            }

            // In game scenes, be extra careful - skip numbered buttons
            if (isGameScene)
            {
                // Skip single digit buttons (1, 2, 3, etc.)
                if (buttonName.Length <= 2 && int.TryParse(buttonName, out _))
                {
                    reason = "Numbered button";
                    return true;
                }

                // Skip Button0, Button1, etc.
                if (System.Text.RegularExpressions.Regex.IsMatch(buttonName, @"^button\d+$"))
                {
                    reason = "Indexed button";
                    return true;
                }
            }

            reason = "";
            return false;
        }

        private bool MatchesAnyPattern(string text, string[] patterns)
        {
            foreach (var pattern in patterns)
            {
                if (text.Contains(pattern))
                    return true;
            }
            return false;
        }

        private string GetFullPath(Transform t)
        {
            string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }

        private NeonButtonGlow.GlowStyle GetSuggestedStyle(string buttonName)
        {
            // Only used as fallback - prefer GetSuggestedStyleByColor when button is available
            string lower = buttonName.ToLower();

            // Premium/Gold buttons
            if (lower.Contains("cash") || lower.Contains("premium") || lower.Contains("gold") ||
                lower.Contains("buy") || lower.Contains("purchase") || lower.Contains("store") ||
                lower.Contains("comprar") || lower.Contains("tienda"))
            {
                return NeonButtonGlow.GlowStyle.Premium;
            }

            // Secondary/subtle buttons
            if (lower.Contains("back") || lower.Contains("cancel") || lower.Contains("close") ||
                lower.Contains("exit") || lower.Contains("return") || lower.Contains("volver") ||
                lower.Contains("cancelar") || lower.Contains("cerrar") || lower.Contains("salir"))
            {
                return NeonButtonGlow.GlowStyle.Secondary;
            }

            // Danger buttons
            if (lower.Contains("delete") || lower.Contains("remove") || lower.Contains("destroy") ||
                lower.Contains("logout") || lower.Contains("signout") || lower.Contains("eliminar") ||
                lower.Contains("borrar"))
            {
                return NeonButtonGlow.GlowStyle.Danger;
            }

            // Success buttons
            if (lower.Contains("confirm") || lower.Contains("accept") || lower.Contains("ok") ||
                lower.Contains("save") || lower.Contains("apply") || lower.Contains("done") ||
                lower.Contains("aceptar") || lower.Contains("guardar") || lower.Contains("listo"))
            {
                return NeonButtonGlow.GlowStyle.Success;
            }

            // Tournament/special buttons
            if (lower.Contains("tournament") || lower.Contains("challenge") || lower.Contains("battle") ||
                lower.Contains("torneo") || lower.Contains("desafio") || lower.Contains("batalla"))
            {
                return NeonButtonGlow.GlowStyle.Purple;
            }

            // Default to primary cyan
            return NeonButtonGlow.GlowStyle.Primary;
        }

        /// <summary>
        /// Gets the suggested glow style based on button's actual color
        /// This is more accurate than name-based detection
        /// </summary>
        private NeonButtonGlow.GlowStyle GetSuggestedStyleByColor(Button button)
        {
            string lower = button.name.ToLower();

            // First check name-based patterns for specific categories
            // These override color detection for semantic meaning

            // Premium/Gold buttons - always gold glow
            if (lower.Contains("cash") || lower.Contains("premium") || lower.Contains("gold") ||
                lower.Contains("buy") || lower.Contains("purchase") || lower.Contains("store") ||
                lower.Contains("comprar") || lower.Contains("tienda"))
            {
                return NeonButtonGlow.GlowStyle.Premium;
            }

            // Danger buttons - always red/orange glow
            if (lower.Contains("delete") || lower.Contains("remove") || lower.Contains("destroy") ||
                lower.Contains("logout") || lower.Contains("signout") || lower.Contains("eliminar") ||
                lower.Contains("borrar"))
            {
                return NeonButtonGlow.GlowStyle.Danger;
            }

            // Success buttons - always green glow
            if (lower.Contains("confirm") || lower.Contains("accept") ||
                lower.Contains("save") || lower.Contains("apply") || lower.Contains("done") ||
                lower.Contains("aceptar") || lower.Contains("guardar") || lower.Contains("listo"))
            {
                return NeonButtonGlow.GlowStyle.Success;
            }

            // Now check the actual button color
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                Color color = buttonImage.color;
                return DetectStyleFromColor(color);
            }

            // Fallback to name-based for secondary buttons
            if (lower.Contains("back") || lower.Contains("cancel") || lower.Contains("close") ||
                lower.Contains("exit") || lower.Contains("return") || lower.Contains("volver") ||
                lower.Contains("cancelar") || lower.Contains("cerrar") || lower.Contains("salir"))
            {
                return NeonButtonGlow.GlowStyle.Secondary;
            }

            return NeonButtonGlow.GlowStyle.Primary;
        }

        /// <summary>
        /// Detects the appropriate glow style based on the button's background color
        /// </summary>
        private NeonButtonGlow.GlowStyle DetectStyleFromColor(Color color)
        {
            // Convert to HSV for better color detection
            Color.RGBToHSV(color, out float h, out float s, out float v);

            // Navy/Dark Blue buttons (0.05-0.15 range for RGB, blue dominant)
            // These are the dark blue buttons in Tournaments, Settings, etc.
            bool isNavyBlue = IsNavyBlueColor(color);
            if (isNavyBlue)
            {
                return NeonButtonGlow.GlowStyle.Navy; // Cyan glow
            }

            // Cyan buttons (hue around 0.5, high saturation)
            if (h >= 0.45f && h <= 0.55f && s > 0.7f && v > 0.7f)
            {
                return NeonButtonGlow.GlowStyle.Primary; // Magenta glow for contrast
            }

            // Purple buttons (hue around 0.75-0.85)
            if (h >= 0.7f && h <= 0.9f && s > 0.3f)
            {
                return NeonButtonGlow.GlowStyle.Purple; // Magenta glow
            }

            // Gold/Yellow buttons (hue around 0.1-0.15)
            if (h >= 0.08f && h <= 0.18f && s > 0.5f && v > 0.7f)
            {
                return NeonButtonGlow.GlowStyle.Premium; // Orange glow
            }

            // Red buttons (hue around 0 or 1)
            if ((h <= 0.05f || h >= 0.95f) && s > 0.5f)
            {
                return NeonButtonGlow.GlowStyle.Danger; // Orange-red glow
            }

            // Green buttons (hue around 0.25-0.4)
            if (h >= 0.25f && h <= 0.4f && s > 0.5f)
            {
                return NeonButtonGlow.GlowStyle.Success; // Light green glow
            }

            // Gray/neutral buttons (low saturation)
            if (s < 0.2f)
            {
                return NeonButtonGlow.GlowStyle.Secondary; // Soft cyan glow
            }

            // Default to Primary
            return NeonButtonGlow.GlowStyle.Primary;
        }

        /// <summary>
        /// Checks if a color is a navy blue (dark blue) color
        /// Navy blue buttons have low RGB values with blue being dominant
        /// Common values: (0.02-0.1, 0.05-0.15, 0.1-0.2)
        /// </summary>
        private bool IsNavyBlueColor(Color color)
        {
            float r = color.r;
            float g = color.g;
            float b = color.b;

            // Navy blue characteristics:
            // 1. Overall dark (all values below 0.25)
            // 2. Blue is the highest or close to highest value
            // 3. Has a blue tint (b > r by some margin)

            bool isDark = r < 0.25f && g < 0.25f && b < 0.35f;
            bool hasBlueHint = b >= r && b >= g * 0.8f;
            bool notPureBlack = (r + g + b) > 0.05f;

            // Also check specific navy blue ranges used in the app
            // DARK_BG = (0.02, 0.05, 0.1)
            // PANEL_BG = (0.05, 0.1, 0.15)
            // BUTTON_BG = (0.08, 0.12, 0.18)
            bool inNavyRange = r < 0.15f && g < 0.2f && b < 0.25f && b > r;

            return isDark && hasBlueHint && notPureBlack && inNavyRange;
        }

        private void AddGlowToUIButtons()
        {
            int added = 0;

            foreach (var info in uiButtons.Where(b => !b.hasGlow))
            {
                if (AddGlowToButton(info.button, info.suggestedStyle))
                {
                    added++;
                }
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            ScanCurrentScene();

            EditorUtility.DisplayDialog("Complete",
                $"Added NeonButtonGlow to {added} UI button(s).\n" +
                $"Skipped {skippedButtons.Count} gameplay button(s).\n\n" +
                "Remember to save the scene!",
                "OK");
        }

        private void AddGlowToSelected()
        {
            int added = 0;

            foreach (var info in uiButtons.Where(b => b.selected && !b.hasGlow))
            {
                if (AddGlowToButton(info.button, info.suggestedStyle))
                {
                    added++;
                }
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            ScanCurrentScene();

            EditorUtility.DisplayDialog("Complete",
                $"Added NeonButtonGlow to {added} button(s).\n\nRemember to save the scene!",
                "OK");
        }

        private bool AddGlowToButton(Button button, NeonButtonGlow.GlowStyle style)
        {
            if (button == null) return false;

            var existingGlow = button.GetComponent<NeonButtonGlow>();

            if (existingGlow != null && !overwriteExisting)
            {
                return false;
            }

            if (existingGlow != null)
            {
                Undo.DestroyObjectImmediate(existingGlow);
            }

            var glow = Undo.AddComponent<NeonButtonGlow>(button.gameObject);

            var serializedObject = new SerializedObject(glow);

            // Set glow style
            var styleProperty = serializedObject.FindProperty("glowStyle");
            if (styleProperty != null)
            {
                styleProperty.enumValueIndex = (int)style;
            }

            // Set visible glow values - contrasting neon effect
            var alphaProperty = serializedObject.FindProperty("glowAlpha");
            if (alphaProperty != null)
            {
                alphaProperty.floatValue = 0.7f;
            }

            var distanceProperty = serializedObject.FindProperty("glowDistance");
            if (distanceProperty != null)
            {
                distanceProperty.vector2Value = new Vector2(4, 4);
            }

            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(button.gameObject);
            return true;
        }

        private void AddGlowToAllScenes()
        {
            string currentScenePath = EditorSceneManager.GetActiveScene().path;
            int totalAdded = 0;
            int totalSkipped = 0;
            int scenesModified = 0;

            string[] allScenes = UI_ONLY_SCENES.Concat(GAME_SCENES).ToArray();

            try
            {
                for (int i = 0; i < allScenes.Length; i++)
                {
                    string scenePath = allScenes[i];

                    EditorUtility.DisplayProgressBar("Adding Neon Glow",
                        $"Processing: {System.IO.Path.GetFileName(scenePath)}",
                        (float)i / allScenes.Length);

                    if (!System.IO.File.Exists(scenePath)) continue;

                    var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    bool isGameScene = GAME_SCENES.Contains(scenePath);

                    var allButtons = Object.FindObjectsOfType<Button>(true);
                    int added = 0;
                    int skipped = 0;

                    foreach (var button in allButtons)
                    {
                        string skipReason;
                        if (ShouldSkipButton(button, isGameScene, out skipReason))
                        {
                            skipped++;
                            continue;
                        }

                        if (AddGlowToButtonNoUndo(button))
                        {
                            added++;
                        }
                    }

                    if (added > 0)
                    {
                        EditorSceneManager.SaveScene(scene);
                        scenesModified++;
                        Debug.Log($"[NeonGlowSetup] {scenePath}: Added {added}, Skipped {skipped}");
                    }

                    totalAdded += added;
                    totalSkipped += skipped;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();

                if (!string.IsNullOrEmpty(currentScenePath) && System.IO.File.Exists(currentScenePath))
                {
                    EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
                }
            }

            EditorUtility.DisplayDialog("Complete",
                $"Added NeonButtonGlow to {totalAdded} UI buttons.\n" +
                $"Skipped {totalSkipped} gameplay buttons.\n" +
                $"Modified {scenesModified} scenes.",
                "OK");
        }

        private bool AddGlowToButtonNoUndo(Button button)
        {
            if (button == null) return false;

            var existingGlow = button.GetComponent<NeonButtonGlow>();

            if (existingGlow != null && !overwriteExisting)
            {
                return false;
            }

            if (existingGlow != null)
            {
                DestroyImmediate(existingGlow);
            }

            var glow = button.gameObject.AddComponent<NeonButtonGlow>();
            var style = GetSuggestedStyleByColor(button);

            var serializedObject = new SerializedObject(glow);

            // Set glow style
            var styleProperty = serializedObject.FindProperty("glowStyle");
            if (styleProperty != null)
            {
                styleProperty.enumValueIndex = (int)style;
            }

            // Set visible glow values - contrasting neon effect
            var alphaProperty = serializedObject.FindProperty("glowAlpha");
            if (alphaProperty != null)
            {
                alphaProperty.floatValue = 0.7f;
            }

            var distanceProperty = serializedObject.FindProperty("glowDistance");
            if (distanceProperty != null)
            {
                distanceProperty.vector2Value = new Vector2(4, 4);
            }

            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(button.gameObject);
            return true;
        }
    }
}
