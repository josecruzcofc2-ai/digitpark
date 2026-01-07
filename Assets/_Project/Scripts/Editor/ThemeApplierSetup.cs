#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using DigitPark.Themes;

namespace DigitPark.Editor
{
    /// <summary>
    /// Agrega ThemeApplier a todos los elementos UI para que respondan a cambios de tema en runtime
    /// </summary>
    public class ThemeApplierSetup : EditorWindow
    {
        [MenuItem("DigitPark/Themes/Add ThemeApplier to Current Scene")]
        public static void AddToCurrentScene()
        {
            int count = AddThemeAppliers();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorUtility.DisplayDialog("ThemeApplier Setup",
                $"Se agregaron {count} componentes ThemeApplier.\n\nGuarda la escena (Ctrl+S)", "OK");
        }

        [MenuItem("DigitPark/Themes/Apply Theme Colors in Editor (Current Scene)")]
        public static void ApplyThemeColorsInEditor()
        {
            // Cargar el tema por defecto (Neon Dark) - nombre correcto del archivo
            ThemeData theme = Resources.Load<ThemeData>("Themes/Theme_NeonDark");

            if (theme == null)
            {
                // Intentar cargar cualquier tema disponible
                ThemeData[] themes = Resources.LoadAll<ThemeData>("Themes");
                if (themes.Length > 0)
                    theme = themes[0];
                Debug.Log($"[ThemeApplier] Cargando tema alternativo: {(theme != null ? theme.themeName : "ninguno")}");
            }

            if (theme == null)
            {
                EditorUtility.DisplayDialog("Error",
                    "No se encontró ningún tema en Resources/Themes.\n\nAsegúrate de tener al menos un ThemeData.", "OK");
                return;
            }

            int count = ApplyThemeToScene(theme);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("Tema Aplicado",
                $"Tema '{theme.themeName}' aplicado a {count} elementos.\n\nGuarda la escena (Ctrl+S)", "OK");
        }

        [MenuItem("DigitPark/Themes/Apply Theme Colors to ALL Scenes")]
        public static void ApplyThemeColorsToAllScenes()
        {
            ThemeData theme = Resources.Load<ThemeData>("Themes/Theme_NeonDark");

            if (theme == null)
            {
                ThemeData[] themes = Resources.LoadAll<ThemeData>("Themes");
                if (themes.Length > 0)
                    theme = themes[0];
                Debug.Log($"[ThemeApplier] Cargando tema alternativo: {(theme != null ? theme.themeName : "ninguno")}");
            }

            if (theme == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontró ningún tema.", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog("Confirmar",
                $"¿Aplicar tema '{theme.themeName}' a TODAS las escenas?\n\nEsto cambiará los colores de los elementos UI.",
                "Sí", "Cancelar"))
                return;

            string[] scenePaths = GetAllScenePaths();
            int totalCount = 0;

            foreach (string scenePath in scenePaths)
            {
                try
                {
                    var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    int count = ApplyThemeToScene(theme);
                    totalCount += count;
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"[ThemeApplier] {scenePath}: {count} elementos coloreados");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[ThemeApplier] Error en {scenePath}: {e.Message}");
                }
            }

            EditorUtility.DisplayDialog("Completado",
                $"Tema '{theme.themeName}' aplicado.\n\nTotal: {totalCount} elementos en {scenePaths.Length} escenas",
                "OK");
        }

        private static string[] GetAllScenePaths()
        {
            return new string[]
            {
                // Escenas principales (ya tienen diseno perfecto del backup)
                "Assets/_Project/Scenes/Boot.unity",
                "Assets/_Project/Scenes/Login.unity",
                "Assets/_Project/Scenes/Register.unity",
                "Assets/_Project/Scenes/MainMenu.unity",
                "Assets/_Project/Scenes/Scores.unity",
                "Assets/_Project/Scenes/Settings.unity",
                "Assets/_Project/Scenes/Tournaments.unity",
                // Escenas nuevas (necesitan ThemeApplier)
                "Assets/_Project/Scenes/Profile.unity",
                "Assets/_Project/Scenes/SearchPlayers.unity",
                // Escenas de juegos
                "Assets/_Project/Scenes/Games/GameSelector.unity",
                "Assets/_Project/Scenes/Games/DigitRush.unity",
                "Assets/_Project/Scenes/Games/MemoryPairs.unity",
                "Assets/_Project/Scenes/Games/QuickMath.unity",
                "Assets/_Project/Scenes/Games/FlashTap.unity",
                "Assets/_Project/Scenes/Games/OddOneOut.unity",
                "Assets/_Project/Scenes/Games/CashBattle.unity"
            };
        }

        private static int ApplyThemeToScene(ThemeData theme)
        {
            int count = 0;

            // Aplicar a elementos con ThemeApplier
            var appliers = GameObject.FindObjectsOfType<ThemeApplier>(true);
            foreach (var applier in appliers)
            {
                ApplyThemeToApplier(applier, theme);
                EditorUtility.SetDirty(applier.gameObject);
                count++;
            }

            // También aplicar colores base a elementos sin ThemeApplier
            count += ApplyBaseColorsToScene(theme);

            return count;
        }

        private static void ApplyThemeToApplier(ThemeApplier applier, ThemeData theme)
        {
            var so = new SerializedObject(applier);
            var elementTypeProp = so.FindProperty("elementType");
            ThemeApplier.ElementType elementType = (ThemeApplier.ElementType)elementTypeProp.enumValueIndex;

            Color color = GetColorForElementType(theme, elementType);

            // Aplicar a Image
            var image = applier.GetComponent<Image>();
            if (image != null)
            {
                image.color = color;
                EditorUtility.SetDirty(image);
            }

            // Aplicar a TextMeshProUGUI
            var tmpText = applier.GetComponent<TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.color = color;
                EditorUtility.SetDirty(tmpText);
            }

            // Aplicar a Button
            var button = applier.GetComponent<Button>();
            if (button != null)
            {
                ColorBlock colors = button.colors;
                colors.normalColor = color;
                colors.highlightedColor = new Color(color.r * 1.1f, color.g * 1.1f, color.b * 1.1f, color.a);
                colors.pressedColor = new Color(color.r * 0.9f, color.g * 0.9f, color.b * 0.9f, color.a);
                colors.selectedColor = colors.highlightedColor;
                button.colors = colors;
                EditorUtility.SetDirty(button);
            }
        }

        private static int ApplyBaseColorsToScene(ThemeData theme)
        {
            int count = 0;

            // Aplicar fondo primario a Canvas con nombre "Background" o similar
            var images = GameObject.FindObjectsOfType<Image>(true);
            foreach (var img in images)
            {
                if (img.GetComponent<ThemeApplier>() != null) continue;

                string name = img.gameObject.name.ToLower();

                if (name.Contains("background") || name == "bg")
                {
                    img.color = theme.primaryBackground;
                    EditorUtility.SetDirty(img);
                    count++;
                }
                else if (name.Contains("panel") || name.Contains("card"))
                {
                    img.color = theme.cardBackground;
                    EditorUtility.SetDirty(img);
                    count++;
                }
            }

            return count;
        }

        private static Color GetColorForElementType(ThemeData theme, ThemeApplier.ElementType type)
        {
            switch (type)
            {
                case ThemeApplier.ElementType.PrimaryBackground: return theme.primaryBackground;
                case ThemeApplier.ElementType.SecondaryBackground: return theme.secondaryBackground;
                case ThemeApplier.ElementType.TertiaryBackground: return theme.tertiaryBackground;
                case ThemeApplier.ElementType.CardBackground: return theme.cardBackground;
                case ThemeApplier.ElementType.Overlay: return theme.overlayColor;
                case ThemeApplier.ElementType.ButtonPrimary: return theme.buttonPrimary;
                case ThemeApplier.ElementType.ButtonSecondary: return theme.buttonSecondary;
                case ThemeApplier.ElementType.ButtonDanger: return theme.buttonDanger;
                case ThemeApplier.ElementType.ButtonSuccess: return theme.buttonSuccess;
                case ThemeApplier.ElementType.TextPrimary: return theme.textPrimary;
                case ThemeApplier.ElementType.TextSecondary: return theme.textSecondary;
                case ThemeApplier.ElementType.TextDisabled: return theme.textDisabled;
                case ThemeApplier.ElementType.TextTitle: return theme.textTitle;
                case ThemeApplier.ElementType.TextOnPrimary: return theme.textOnPrimary;
                case ThemeApplier.ElementType.Accent: return theme.primaryAccent;
                case ThemeApplier.ElementType.AccentSecondary: return theme.secondaryAccent;
                case ThemeApplier.ElementType.Glow: return theme.glowColor;
                case ThemeApplier.ElementType.Error: return theme.errorColor;
                case ThemeApplier.ElementType.Warning: return theme.warningColor;
                case ThemeApplier.ElementType.Success: return theme.successColor;
                default: return Color.white;
            }
        }

        [MenuItem("DigitPark/Themes/Add ThemeApplier to Prefabs")]
        public static void AddToPrefabs()
        {
            int count = 0;
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project/Prefabs" });

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
                bool modified = false;

                // Procesar todos los botones en el prefab
                var buttons = prefabRoot.GetComponentsInChildren<Button>(true);
                foreach (var btn in buttons)
                {
                    if (btn.GetComponent<ThemeApplier>() == null)
                    {
                        string name = btn.gameObject.name.ToLower();
                        ThemeApplier.ElementType elementType = ThemeApplier.ElementType.ButtonSecondary;

                        if (name.Contains("back") || name.Contains("return") || name.Contains("arrow") ||
                            name.Contains("close") || name.Contains("exit") || path.ToLower().Contains("back"))
                        {
                            elementType = ThemeApplier.ElementType.Accent;
                        }
                        else if (name.Contains("login") || name.Contains("play") || name.Contains("confirm") ||
                            name.Contains("create") || name.Contains("submit") || name.Contains("register") ||
                            name.Contains("save") || name.Contains("accept") || name.Contains("join"))
                        {
                            elementType = ThemeApplier.ElementType.ButtonPrimary;
                        }
                        else if (name.Contains("delete") || name.Contains("logout") || name.Contains("remove"))
                        {
                            elementType = ThemeApplier.ElementType.ButtonDanger;
                        }

                        var applier = btn.gameObject.AddComponent<ThemeApplier>();
                        SetElementType(applier, elementType);
                        count++;
                        modified = true;
                        Debug.Log($"[ThemeApplierSetup] {path} → {btn.gameObject.name} → {elementType}");
                    }
                }

                // También revisar si el root tiene Button
                var rootButton = prefabRoot.GetComponent<Button>();
                if (rootButton != null && prefabRoot.GetComponent<ThemeApplier>() == null)
                {
                    string name = prefabRoot.name.ToLower();
                    ThemeApplier.ElementType elementType = ThemeApplier.ElementType.ButtonSecondary;

                    if (name.Contains("back") || name.Contains("return") || name.Contains("arrow") ||
                        name.Contains("close") || name.Contains("exit"))
                    {
                        elementType = ThemeApplier.ElementType.Accent;
                    }

                    var applier = prefabRoot.AddComponent<ThemeApplier>();
                    SetElementType(applier, elementType);
                    count++;
                    modified = true;
                    Debug.Log($"[ThemeApplierSetup] {path} (root) → {elementType}");
                }

                if (modified)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
                }
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Prefabs Actualizados",
                $"Se agregaron {count} componentes ThemeApplier a prefabs.", "OK");
        }

        [MenuItem("DigitPark/Themes/Add ThemeApplier to ALL Scenes")]
        public static void AddToAllScenes()
        {
            if (!EditorUtility.DisplayDialog("Confirmar",
                "¿Agregar ThemeApplier a TODAS las escenas?\n\nEsto permitirá cambiar temas en runtime.",
                "Sí", "Cancelar"))
                return;

            string[] scenePaths = GetAllScenePaths();

            int totalCount = 0;
            foreach (string scenePath in scenePaths)
            {
                try
                {
                    var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    int count = AddThemeAppliers();
                    totalCount += count;
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"[ThemeApplierSetup] {scenePath}: {count} componentes agregados");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[ThemeApplierSetup] Error en {scenePath}: {e.Message}");
                }
            }

            EditorUtility.DisplayDialog("Completado",
                $"ThemeApplier agregado a todas las escenas.\n\nTotal: {totalCount} componentes",
                "OK");
        }

        private static int AddThemeAppliers()
        {
            int count = 0;

            // Procesar imágenes (fondos, paneles, etc.)
            count += ProcessImages();

            // Procesar textos
            count += ProcessTexts();

            // Procesar botones
            count += ProcessButtons();

            // Procesar sliders
            count += ProcessSliders();

            return count;
        }

        private static int ProcessImages()
        {
            int count = 0;
            var images = GameObject.FindObjectsOfType<Image>(true);

            foreach (var img in images)
            {
                // Saltar si ya tiene ThemeApplier
                if (img.GetComponent<ThemeApplier>() != null) continue;

                // Saltar imágenes de botones (se manejan en ProcessButtons)
                if (img.GetComponent<Button>() != null) continue;

                string name = img.gameObject.name.ToLower();
                ThemeApplier.ElementType elementType = ThemeApplier.ElementType.None;

                // Detectar tipo de elemento
                if (name.Contains("background") || name == "bg")
                {
                    if (name.Contains("main") || name.Contains("screen"))
                        elementType = ThemeApplier.ElementType.PrimaryBackground;
                    else if (name.Contains("card") || name.Contains("panel") || name.Contains("popup"))
                        elementType = ThemeApplier.ElementType.CardBackground;
                    else
                        elementType = ThemeApplier.ElementType.SecondaryBackground;
                }
                else if (name.Contains("panel"))
                {
                    elementType = ThemeApplier.ElementType.CardBackground;
                }
                else if (name.Contains("fill") && img.transform.parent?.name.ToLower().Contains("slider") == true)
                {
                    elementType = ThemeApplier.ElementType.SliderFill;
                }
                else if (name.Contains("handle"))
                {
                    elementType = ThemeApplier.ElementType.SliderHandle;
                }

                if (elementType != ThemeApplier.ElementType.None)
                {
                    var applier = img.gameObject.AddComponent<ThemeApplier>();
                    SetElementType(applier, elementType);
                    EditorUtility.SetDirty(img.gameObject);
                    count++;
                }
            }
            return count;
        }

        private static int ProcessTexts()
        {
            int count = 0;
            var texts = GameObject.FindObjectsOfType<TextMeshProUGUI>(true);

            foreach (var text in texts)
            {
                if (text.GetComponent<ThemeApplier>() != null) continue;

                // Saltar textos de botones
                if (text.transform.parent?.GetComponent<Button>() != null) continue;

                string name = text.gameObject.name.ToLower();
                ThemeApplier.ElementType elementType = ThemeApplier.ElementType.None;

                if (name.Contains("title") || name.Contains("header"))
                    elementType = ThemeApplier.ElementType.TextTitle;
                else if (name.Contains("placeholder"))
                    elementType = ThemeApplier.ElementType.TextDisabled;
                else if (name.Contains("label") || name.Contains("subtitle") || name.Contains("stats") && !name.Contains("value"))
                    elementType = ThemeApplier.ElementType.TextSecondary;
                else if (name.Contains("error"))
                    elementType = ThemeApplier.ElementType.Error;
                else if (name.Contains("value") || name.Contains("score") || name.Contains("timer") ||
                         name.Contains("round") || name.Contains("time") || name.Contains("promedio") ||
                         name.Contains("pares") || name.Contains("reaction"))
                    elementType = ThemeApplier.ElementType.Accent;
                else if (name.Contains("instruction") || name.Contains("problem") || name.Contains("encuentra"))
                    elementType = ThemeApplier.ElementType.TextPrimary;
                else if (name.Contains("username") || name.Contains("user"))
                    elementType = ThemeApplier.ElementType.TextTitle;
                else
                    elementType = ThemeApplier.ElementType.TextPrimary;

                if (elementType != ThemeApplier.ElementType.None)
                {
                    var applier = text.gameObject.AddComponent<ThemeApplier>();
                    SetElementType(applier, elementType);
                    SetApplyToText(applier, true);
                    SetApplyToImage(applier, false);
                    EditorUtility.SetDirty(text.gameObject);
                    count++;
                }
            }
            return count;
        }

        private static int ProcessButtons()
        {
            int count = 0;
            var buttons = GameObject.FindObjectsOfType<Button>(true);

            foreach (var btn in buttons)
            {
                if (btn.GetComponent<ThemeApplier>() != null) continue;

                string name = btn.gameObject.name.ToLower();
                ThemeApplier.ElementType elementType = ThemeApplier.ElementType.ButtonSecondary;

                // Back Buttons - usan Accent para cambiar con el tema
                if (name.Contains("back") || name.Contains("return") || name.Contains("arrow") ||
                    name.Contains("close") || name.Contains("exit"))
                {
                    elementType = ThemeApplier.ElementType.Accent;
                }
                // Botones principales
                else if (name.Contains("login") || name.Contains("play") || name.Contains("confirm") ||
                    name.Contains("create") || name.Contains("submit") || name.Contains("register") ||
                    name.Contains("save") || name.Contains("accept") || name.Contains("join") ||
                    name.Contains("start") || name.Contains("ok") || name.Contains("yes") ||
                    name.Contains("verificar") || name.Contains("agregar") || name.Contains("editar"))
                {
                    elementType = ThemeApplier.ElementType.ButtonPrimary;
                }
                // Botones de peligro
                else if (name.Contains("delete") || name.Contains("logout") || name.Contains("remove") ||
                    name.Contains("retar") || name.Contains("challenge"))
                {
                    elementType = ThemeApplier.ElementType.ButtonDanger;
                }
                // Botones de cancelar
                else if (name.Contains("cancel") || name.Contains("no") || name.Contains("clear"))
                {
                    elementType = ThemeApplier.ElementType.ButtonSecondary;
                }
                // Botones de juego (grids, respuestas, cartas)
                else if (name.Contains("btn_") || name.Contains("card") || name.Contains("answer") ||
                    name.Contains("grid") || name.Contains("left") || name.Contains("right") ||
                    name.Contains("tap"))
                {
                    elementType = ThemeApplier.ElementType.CardBackground;
                }
                // Botones de selección de juego
                else if (name.Contains("digit") || name.Contains("memory") || name.Contains("quick") ||
                    name.Contains("flash") || name.Contains("odd") || name.Contains("cognitive") ||
                    name.Contains("sprint"))
                {
                    elementType = ThemeApplier.ElementType.ButtonPrimary;
                }

                var applier = btn.gameObject.AddComponent<ThemeApplier>();
                SetElementType(applier, elementType);
                EditorUtility.SetDirty(btn.gameObject);
                count++;
            }
            return count;
        }

        private static int ProcessSliders()
        {
            int count = 0;
            var sliders = GameObject.FindObjectsOfType<Slider>(true);

            foreach (var slider in sliders)
            {
                // Fill
                if (slider.fillRect != null)
                {
                    var fill = slider.fillRect.GetComponent<Image>();
                    if (fill != null && fill.GetComponent<ThemeApplier>() == null)
                    {
                        var applier = fill.gameObject.AddComponent<ThemeApplier>();
                        SetElementType(applier, ThemeApplier.ElementType.SliderFill);
                        EditorUtility.SetDirty(fill.gameObject);
                        count++;
                    }
                }

                // Handle
                if (slider.handleRect != null)
                {
                    var handle = slider.handleRect.GetComponent<Image>();
                    if (handle != null && handle.GetComponent<ThemeApplier>() == null)
                    {
                        var applier = handle.gameObject.AddComponent<ThemeApplier>();
                        SetElementType(applier, ThemeApplier.ElementType.SliderHandle);
                        EditorUtility.SetDirty(handle.gameObject);
                        count++;
                    }
                }

                // Background
                var bg = slider.transform.Find("Background")?.GetComponent<Image>();
                if (bg != null && bg.GetComponent<ThemeApplier>() == null)
                {
                    var applier = bg.gameObject.AddComponent<ThemeApplier>();
                    SetElementType(applier, ThemeApplier.ElementType.SliderTrack);
                    EditorUtility.SetDirty(bg.gameObject);
                    count++;
                }
            }
            return count;
        }

        // Helpers para usar SerializedObject
        private static void SetElementType(ThemeApplier applier, ThemeApplier.ElementType type)
        {
            var so = new SerializedObject(applier);
            so.FindProperty("elementType").enumValueIndex = (int)type;
            so.ApplyModifiedProperties();
        }

        private static void SetApplyToText(ThemeApplier applier, bool value)
        {
            var so = new SerializedObject(applier);
            so.FindProperty("applyToText").boolValue = value;
            so.ApplyModifiedProperties();
        }

        private static void SetApplyToImage(ThemeApplier applier, bool value)
        {
            var so = new SerializedObject(applier);
            so.FindProperty("applyToImage").boolValue = value;
            so.ApplyModifiedProperties();
        }
    }
}
#endif
