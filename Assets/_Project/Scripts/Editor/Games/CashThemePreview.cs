using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor tool to preview Normal (cyan/neon) vs Gold (CashTournament) theme
    /// on any minigame scene. Toggle between themes to compare visual states.
    /// Menu: DigitPark/UI Builders/Games/Cash Theme Preview
    /// </summary>
    public class CashThemePreview : EditorWindow
    {
        // Normal neon colors
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color GREEN_NEON = new Color(0.3f, 1f, 0.5f, 1f);
        private static readonly Color MAGENTA_NEON = new Color(1f, 0f, 0.8f, 1f);
        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color PANEL_BG = new Color(0.05f, 0.1f, 0.15f, 0.95f);

        // Gold replacement colors
        private static readonly Color GOLD_PRIMARY = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GOLD_LIGHT = new Color(1f, 0.93f, 0.4f, 1f);
        private static readonly Color GOLD_ACCENT = new Color(0.96f, 0.64f, 0.08f, 1f);
        private static readonly Color GOLD_DARK_BG = new Color(0.06f, 0.04f, 0.02f, 1f);
        private static readonly Color GOLD_PANEL_BG = new Color(0.1f, 0.08f, 0.04f, 0.95f);

        private static readonly float COLOR_TOLERANCE = 0.05f;

        private bool isGoldMode = false;
        private int textsChanged = 0;
        private int imagesChanged = 0;
        private int outlinesChanged = 0;
        private string currentScene = "";

        private static readonly string[] MINIGAME_SCENES = {
            "DigitRush", "FlashTap", "MemoryPairs", "OddOneOut", "QuickMath"
        };

        [MenuItem("DigitPark/UI Builders/Games/Cash Theme Preview", false, 200)]
        public static void ShowWindow()
        {
            var window = GetWindow<CashThemePreview>("Cash Theme Preview");
            window.minSize = new Vector2(400, 350);
        }

        private void OnGUI()
        {
            GUILayout.Label("Cash Tournament Theme Preview", EditorStyles.boldLabel);
            GUILayout.Space(5);

            currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            bool isMinigameScene = System.Array.Exists(MINIGAME_SCENES, s => s == currentScene);

            // Scene status
            EditorGUILayout.BeginHorizontal("box");
            GUILayout.Label("Scene:", GUILayout.Width(50));
            GUI.color = isMinigameScene ? Color.green : Color.yellow;
            GUILayout.Label(currentScene, EditorStyles.boldLabel);
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();

            if (!isMinigameScene)
            {
                EditorGUILayout.HelpBox(
                    "Open a minigame scene to preview themes:\n" +
                    "DigitRush, FlashTap, MemoryPairs, OddOneOut, QuickMath",
                    MessageType.Warning);
                GUILayout.Space(5);
            }

            // Current mode indicator
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUI.backgroundColor = isGoldMode ? new Color(1f, 0.84f, 0f) : new Color(0f, 1f, 1f);
            GUIStyle modeStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };
            GUILayout.Label(isGoldMode ? "GOLD MODE (CashTournament)" : "NORMAL MODE (Neon)", modeStyle, GUILayout.Height(30));
            GUI.backgroundColor = Color.white;

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // Color swatches
            GUILayout.Space(10);
            DrawColorSwatches();

            // Toggle buttons
            GUILayout.Space(15);
            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = isGoldMode ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            if (GUILayout.Button("Apply NORMAL Theme\n(Cyan/Neon)", GUILayout.Height(50)))
            {
                ApplyNormalTheme();
            }

            GUI.backgroundColor = isGoldMode ? new Color(0.5f, 0.5f, 0.5f) : new Color(1f, 0.84f, 0f);
            if (GUILayout.Button("Apply GOLD Theme\n(CashTournament)", GUILayout.Height(50)))
            {
                ApplyGoldTheme();
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            // Stats
            if (textsChanged > 0 || imagesChanged > 0 || outlinesChanged > 0)
            {
                GUILayout.Space(10);
                EditorGUILayout.BeginVertical("box");
                GUILayout.Label("Last operation:", EditorStyles.boldLabel);
                GUILayout.Label($"  Texts changed: {textsChanged}");
                GUILayout.Label($"  Images changed: {imagesChanged}");
                GUILayout.Label($"  Outlines changed: {outlinesChanged}");
                GUILayout.Label($"  Total: {textsChanged + imagesChanged + outlinesChanged}");
                EditorGUILayout.EndVertical();
            }

            // Help
            GUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "This tool swaps UI colors in the current scene between Normal (cyan neon) " +
                "and Gold (CashTournament) themes.\n\n" +
                "Color mapping:\n" +
                "  Cyan (#00FFFF) <-> Gold (#FFD700)\n" +
                "  Green (#4DFF80) <-> Light Gold (#FFED66)\n" +
                "  Magenta (#FF00CC) <-> Warm Gold (#F5A314)\n" +
                "  Dark BG -> Warm Dark BG\n" +
                "  Panel BG -> Warm Panel BG\n\n" +
                "Changes are scene-only (not saved unless you save the scene).\n" +
                "Use Ctrl+Z to undo.",
                MessageType.Info);
        }

        private void DrawColorSwatches()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);

            // Normal colors column
            EditorGUILayout.BeginVertical(GUILayout.Width(150));
            GUILayout.Label("Normal", EditorStyles.centeredGreyMiniLabel);
            DrawSwatch("Cyan", CYAN_NEON);
            DrawSwatch("Green", GREEN_NEON);
            DrawSwatch("Magenta", MAGENTA_NEON);
            EditorGUILayout.EndVertical();

            // Arrow
            GUILayout.Label(isGoldMode ? "<-" : "->", new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20
            }, GUILayout.Width(40), GUILayout.Height(80));

            // Gold colors column
            EditorGUILayout.BeginVertical(GUILayout.Width(150));
            GUILayout.Label("Gold", EditorStyles.centeredGreyMiniLabel);
            DrawSwatch("Gold", GOLD_PRIMARY);
            DrawSwatch("Light Gold", GOLD_LIGHT);
            DrawSwatch("Warm Gold", GOLD_ACCENT);
            EditorGUILayout.EndVertical();

            GUILayout.Space(20);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSwatch(string label, Color color)
        {
            EditorGUILayout.BeginHorizontal();
            Rect rect = GUILayoutUtility.GetRect(20, 16, GUILayout.Width(20));
            EditorGUI.DrawRect(rect, color);
            GUILayout.Label(label, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();
        }

        private void ApplyGoldTheme()
        {
            ResetCounts();
            Undo.RecordObjects(FindObjectsOfType<Component>(true), "Apply Gold Theme");

            // Texts: cyan->gold, green->gold_light, magenta->gold_accent
            foreach (var tmp in FindObjectsOfType<TextMeshProUGUI>(true))
            {
                if (ColorsMatch(tmp.color, CYAN_NEON)) { tmp.color = GOLD_PRIMARY; textsChanged++; }
                else if (ColorsMatch(tmp.color, GREEN_NEON)) { tmp.color = GOLD_LIGHT; textsChanged++; }
                else if (ColorsMatch(tmp.color, MAGENTA_NEON)) { tmp.color = GOLD_ACCENT; textsChanged++; }
            }

            // Images
            foreach (var img in FindObjectsOfType<Image>(true))
            {
                if (ColorsMatch(img.color, CYAN_NEON)) { img.color = GOLD_PRIMARY; imagesChanged++; }
                else if (ColorsMatch(img.color, GREEN_NEON)) { img.color = GOLD_LIGHT; imagesChanged++; }
                else if (ColorsMatch(img.color, MAGENTA_NEON)) { img.color = GOLD_ACCENT; imagesChanged++; }
                else if (ColorsMatch(img.color, DARK_BG)) { img.color = GOLD_DARK_BG; imagesChanged++; }
                else if (ColorsMatch(img.color, PANEL_BG)) { img.color = GOLD_PANEL_BG; imagesChanged++; }
            }

            // Outlines
            foreach (var outline in FindObjectsOfType<Outline>(true))
            {
                if (ColorsMatch(outline.effectColor, CYAN_NEON)) { outline.effectColor = GOLD_PRIMARY; outlinesChanged++; }
                else if (ColorsMatch(outline.effectColor, GREEN_NEON)) { outline.effectColor = GOLD_LIGHT; outlinesChanged++; }
            }

            // Camera
            var cam = Camera.main;
            if (cam != null && ColorsMatch(cam.backgroundColor, DARK_BG))
                cam.backgroundColor = GOLD_DARK_BG;

            isGoldMode = true;
            SceneView.RepaintAll();
            Debug.Log($"[CashThemePreview] Gold theme applied: {textsChanged} texts, {imagesChanged} images, {outlinesChanged} outlines");
        }

        private void ApplyNormalTheme()
        {
            ResetCounts();
            Undo.RecordObjects(FindObjectsOfType<Component>(true), "Apply Normal Theme");

            // Texts: gold->cyan, gold_light->green, gold_accent->magenta
            foreach (var tmp in FindObjectsOfType<TextMeshProUGUI>(true))
            {
                if (ColorsMatch(tmp.color, GOLD_PRIMARY)) { tmp.color = CYAN_NEON; textsChanged++; }
                else if (ColorsMatch(tmp.color, GOLD_LIGHT)) { tmp.color = GREEN_NEON; textsChanged++; }
                else if (ColorsMatch(tmp.color, GOLD_ACCENT)) { tmp.color = MAGENTA_NEON; textsChanged++; }
            }

            // Images
            foreach (var img in FindObjectsOfType<Image>(true))
            {
                if (ColorsMatch(img.color, GOLD_PRIMARY)) { img.color = CYAN_NEON; imagesChanged++; }
                else if (ColorsMatch(img.color, GOLD_LIGHT)) { img.color = GREEN_NEON; imagesChanged++; }
                else if (ColorsMatch(img.color, GOLD_ACCENT)) { img.color = MAGENTA_NEON; imagesChanged++; }
                else if (ColorsMatch(img.color, GOLD_DARK_BG)) { img.color = DARK_BG; imagesChanged++; }
                else if (ColorsMatch(img.color, GOLD_PANEL_BG)) { img.color = PANEL_BG; imagesChanged++; }
            }

            // Outlines
            foreach (var outline in FindObjectsOfType<Outline>(true))
            {
                if (ColorsMatch(outline.effectColor, GOLD_PRIMARY)) { outline.effectColor = CYAN_NEON; outlinesChanged++; }
                else if (ColorsMatch(outline.effectColor, GOLD_LIGHT)) { outline.effectColor = GREEN_NEON; outlinesChanged++; }
            }

            // Camera
            var cam = Camera.main;
            if (cam != null && ColorsMatch(cam.backgroundColor, GOLD_DARK_BG))
                cam.backgroundColor = DARK_BG;

            isGoldMode = false;
            SceneView.RepaintAll();
            Debug.Log($"[CashThemePreview] Normal theme restored: {textsChanged} texts, {imagesChanged} images, {outlinesChanged} outlines");
        }

        private void ResetCounts()
        {
            textsChanged = 0;
            imagesChanged = 0;
            outlinesChanged = 0;
        }

        private static bool ColorsMatch(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) < COLOR_TOLERANCE &&
                   Mathf.Abs(a.g - b.g) < COLOR_TOLERANCE &&
                   Mathf.Abs(a.b - b.b) < COLOR_TOLERANCE;
        }
    }
}
