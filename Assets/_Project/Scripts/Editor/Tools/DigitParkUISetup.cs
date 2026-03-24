using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using DigitPark.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// Herramienta avanzada para configurar automáticamente las referencias UI de los Managers.
    /// Específica para el proyecto DigitPark - entiende la estructura y convenciones del proyecto.
    /// </summary>
    public class DigitParkUISetup : EditorWindow
    {
        private Vector2 scrollPosition;
        private string selectedScene = "";
        private Dictionary<string, List<FieldRequirement>> sceneRequirements = new Dictionary<string, List<FieldRequirement>>();
        private List<string> availableScenes = new List<string>();
        private bool showMissingOnly = true;
        private GUIStyle headerStyle;
        private GUIStyle successStyle;
        private GUIStyle errorStyle;
        private GUIStyle warningStyle;

        // Mapeo de escenas a sus managers principales
        private static readonly Dictionary<string, string[]> SceneToManagers = new Dictionary<string, string[]>
        {
            { "Login", new[] { "LoginManager" } },
            { "Register", new[] { "RegisterManager" } },
            { "MainMenu", new[] { "MainMenuManager" } },
            { "Settings", new[] { "SettingsManager" } },
            { "Profile", new[] { "ProfileManager" } },
            { "Scores", new[] { "LeaderboardManager" } },
            { "SearchPlayers", new[] { "SearchPlayersManager" } },
            { "PlayModeSelection", new[] { "PlayModeSelectionManager" } },
            { "GameSelector", new[] { "GameSelectorManager" } },
            { "Matchmaking", new[] { "MatchmakingManager" } },
{ "TournamentsBrowser", new[] { "TournamentsBrowserManager" } },
            { "TournamentCreate", new[] { "TournamentCreateManager" } },
            { "TournamentLobby", new[] { "TournamentLobbyManager" } },
            { "Shop", new[] { "ShopManager" } },
            { "DailyMissions", new[] { "DailyMissionsManager" } },
            { "DailyRewards", new[] { "DailyRewardsManager" } },
            { "Achievements", new[] { "AchievementsManager" } },
            { "Onboarding", new[] { "OnboardingManager" } },
            { "DigitRush", new[] { "DigitRushController" } },
            { "Boot", new[] { "BootManager" } },
        };

        // Convenciones de nombres del proyecto
        private static readonly Dictionary<string, string[]> FieldNamePatterns = new Dictionary<string, string[]>
        {
            // Buttons
            { "backButton", new[] { "BackButton", "BtnBack", "Back", "BackBtn" } },
            { "loginButton", new[] { "LoginButton", "BtnLogin", "Login", "LoginBtn" } },
            { "registerButton", new[] { "RegisterButton", "BtnRegister", "Register", "RegisterBtn" } },
            { "playButton", new[] { "PlayButton", "BtnPlay", "Play", "PlayBtn" } },
            { "confirmButton", new[] { "ConfirmButton", "BtnConfirm", "Confirm", "ConfirmBtn", "OkButton" } },
            { "cancelButton", new[] { "CancelButton", "BtnCancel", "Cancel", "CancelBtn" } },
            { "skipButton", new[] { "SkipButton", "BtnSkip", "Skip", "SkipBtn" } },
            { "nextButton", new[] { "NextButton", "BtnNext", "Next", "NextBtn" } },
            { "prevButton", new[] { "PrevButton", "BtnPrev", "Prev", "PrevBtn", "PreviousButton" } },
            { "claimButton", new[] { "ClaimButton", "BtnClaim", "Claim", "ClaimBtn" } },

            // Panels
            { "loadingPanel", new[] { "LoadingPanel", "Loading", "LoadingOverlay", "PnlLoading" } },
            { "mainPanel", new[] { "MainPanel", "Main", "Content", "PnlMain" } },
            { "loginPanel", new[] { "LoginPanel", "Login", "PnlLogin" } },

            // Text
            { "titleText", new[] { "TitleText", "Title", "TxtTitle", "HeaderText" } },
            { "statusText", new[] { "StatusText", "Status", "TxtStatus", "MessageText" } },

            // Inputs
            { "emailInput", new[] { "EmailInput", "Email", "EmailField", "InputEmail" } },
            { "passwordInput", new[] { "PasswordInput", "Password", "PasswordField", "InputPassword" } },

            // Icons/Images
            { "avatarImage", new[] { "AvatarImage", "Avatar", "ProfileImage", "UserAvatar" } },
        };

        [MenuItem("DigitPark/Setup/UI Setup Tool %#u", false, 1)]
        public static void ShowWindow()
        {
            var window = GetWindow<DigitParkUISetup>("DigitPark UI Setup");
            window.minSize = new Vector2(600, 700);
        }

        private void OnEnable()
        {
            RefreshSceneList();
        }

        private void InitStyles()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 };
                successStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = Color.green } };
                errorStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = Color.red } };
                warningStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = Color.yellow } };
            }
        }

        private void OnGUI()
        {
            InitStyles();

            EditorGUILayout.Space(10);

            // Header
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("DigitPark UI Setup Tool", titleStyle);
            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "Esta herramienta analiza los Managers de cada escena y auto-asigna las referencias UI.\n" +
                "• Analiza campos [SerializeField] de los Managers\n" +
                "• Busca objetos en el Hierarchy con nombres coincidentes\n" +
                "• Puede crear elementos UI faltantes automáticamente",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Scene Selection
            DrawSceneSelection();

            EditorGUILayout.Space(10);

            // Action Buttons
            DrawActionButtons();

            EditorGUILayout.Space(10);

            // Results
            DrawResults();
        }

        private void DrawSceneSelection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Selección de Escena", headerStyle);

            EditorGUILayout.BeginHorizontal();

            // Current scene button
            string currentSceneName = SceneManager.GetActiveScene().name;
            GUI.backgroundColor = new Color(0.4f, 0.8f, 1f);
            if (GUILayout.Button($"Usar Escena Actual: {currentSceneName}", GUILayout.Height(25)))
            {
                selectedScene = currentSceneName;
                AnalyzeCurrentScene();
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Scene dropdown
            if (availableScenes.Count > 0)
            {
                int currentIndex = availableScenes.IndexOf(selectedScene);
                if (currentIndex < 0) currentIndex = 0;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("O selecciona una escena:", GUILayout.Width(150));
                int newIndex = EditorGUILayout.Popup(currentIndex, availableScenes.ToArray());
                if (newIndex != currentIndex || string.IsNullOrEmpty(selectedScene))
                {
                    selectedScene = availableScenes[newIndex];
                }
                EditorGUILayout.EndHorizontal();
            }

            showMissingOnly = EditorGUILayout.Toggle("Solo mostrar faltantes", showMissingOnly);

            EditorGUILayout.EndVertical();
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.3f, 0.7f, 1f);
            if (GUILayout.Button("Analizar Escena", GUILayout.Height(35)))
            {
                AnalyzeCurrentScene();
            }

            GUI.backgroundColor = new Color(0.3f, 1f, 0.5f);
            if (GUILayout.Button("Auto-Asignar Referencias", GUILayout.Height(35)))
            {
                AutoAssignReferences();
            }

            GUI.backgroundColor = new Color(1f, 0.8f, 0.3f);
            if (GUILayout.Button("Crear UI Faltante", GUILayout.Height(35)))
            {
                CreateMissingUI();
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.8f, 0.5f, 1f);
            if (GUILayout.Button("Procesar TODAS las Escenas", GUILayout.Height(30)))
            {
                ProcessAllScenes();
            }

            GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f);
            if (GUILayout.Button("Exportar Reporte", GUILayout.Height(30)))
            {
                ExportReport();
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        private void DrawResults()
        {
            if (!sceneRequirements.ContainsKey(selectedScene) || sceneRequirements[selectedScene].Count == 0)
            {
                EditorGUILayout.HelpBox("Selecciona una escena y presiona 'Analizar' para ver los requisitos.", MessageType.Info);
                return;
            }

            var requirements = sceneRequirements[selectedScene];

            // Summary
            int total = requirements.Count;
            int assigned = requirements.Count(r => r.IsAssigned);
            int missing = total - assigned;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Resumen: {assigned}/{total} asignados ({missing} faltantes)", headerStyle);

            // Progress bar
            Rect progressRect = EditorGUILayout.GetControlRect(false, 20);
            EditorGUI.ProgressBar(progressRect, (float)assigned / total, $"{(assigned * 100f / total):F0}%");

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            // Detailed list
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            string currentManager = "";
            foreach (var req in requirements)
            {
                if (showMissingOnly && req.IsAssigned) continue;

                // Group header
                if (req.ManagerName != currentManager)
                {
                    currentManager = req.ManagerName;
                    EditorGUILayout.Space(10);
                    EditorGUILayout.LabelField(currentManager, headerStyle);
                    DrawSeparator();
                }

                DrawRequirementRow(req);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawRequirementRow(FieldRequirement req)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            // Status icon
            string statusIcon = req.IsAssigned ? "✓" : "✗";
            GUIStyle statusStyle = req.IsAssigned ? successStyle : errorStyle;
            EditorGUILayout.LabelField(statusIcon, statusStyle, GUILayout.Width(20));

            // Field name
            EditorGUILayout.LabelField(req.FieldName, GUILayout.Width(180));

            // Type
            EditorGUILayout.LabelField(req.FieldType, GUILayout.Width(120));

            // Current value or suggestion
            if (req.IsAssigned)
            {
                if (GUILayout.Button(req.CurrentValue?.name ?? "Assigned", EditorStyles.linkLabel))
                {
                    Selection.activeObject = req.CurrentValue;
                    EditorGUIUtility.PingObject(req.CurrentValue);
                }
            }
            else
            {
                EditorGUILayout.LabelField($"Crear: {req.SuggestedName}", warningStyle);

                if (GUILayout.Button("Crear", GUILayout.Width(50)))
                {
                    CreateSingleUIElement(req);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSeparator()
        {
            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        }

        private void RefreshSceneList()
        {
            availableScenes.Clear();

            // Get scenes from build settings
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    string sceneName = System.IO.Path.GetFileNameWithoutExtension(scene.path);
                    availableScenes.Add(sceneName);
                }
            }

            if (string.IsNullOrEmpty(selectedScene) && availableScenes.Count > 0)
            {
                selectedScene = SceneManager.GetActiveScene().name;
            }
        }

        private void AnalyzeCurrentScene()
        {
            if (string.IsNullOrEmpty(selectedScene))
            {
                selectedScene = SceneManager.GetActiveScene().name;
            }

            var requirements = new List<FieldRequirement>();

            // Find all DigitPark MonoBehaviours in the scene
            var allComponents = FindObjectsOfType<MonoBehaviour>(true);

            foreach (var component in allComponents)
            {
                if (component == null) continue;

                Type type = component.GetType();
                string namespaceName = type.Namespace ?? "";

                // Only process DigitPark components
                if (!namespaceName.StartsWith("DigitPark")) continue;

                AnalyzeComponent(component, requirements);
            }

            sceneRequirements[selectedScene] = requirements;

            Debug.Log($"[DigitParkUISetup] Analizados {requirements.Count} campos en escena {selectedScene}");
        }

        private void AnalyzeComponent(MonoBehaviour component, List<FieldRequirement> requirements)
        {
            Type type = component.GetType();
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                // Check if serializable
                bool isSerializable = field.IsPublic || field.GetCustomAttribute<SerializeField>() != null;
                if (!isSerializable) continue;

                // Skip non-Unity types
                if (!typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType)) continue;

                // Skip arrays and lists for now
                if (field.FieldType.IsArray) continue;

                object currentValue = field.GetValue(component);
                UnityEngine.Object unityValue = currentValue as UnityEngine.Object;

                // Get header attribute if exists
                string header = "";
                var headerAttr = field.GetCustomAttribute<HeaderAttribute>();
                if (headerAttr != null) header = headerAttr.header;

                requirements.Add(new FieldRequirement
                {
                    Component = component,
                    ManagerName = $"{component.gameObject.name} ({type.Name})",
                    Field = field,
                    FieldName = field.Name,
                    FieldType = field.FieldType.Name,
                    CurrentValue = unityValue,
                    SuggestedName = GetSuggestedName(field.Name),
                    Header = header
                });
            }
        }

        private string GetSuggestedName(string fieldName)
        {
            // Remove prefixes
            string clean = fieldName;
            if (clean.StartsWith("_")) clean = clean.Substring(1);
            if (clean.StartsWith("m_")) clean = clean.Substring(2);

            // Convert to PascalCase
            return char.ToUpper(clean[0]) + clean.Substring(1);
        }

        private void AutoAssignReferences()
        {
            if (!sceneRequirements.ContainsKey(selectedScene))
            {
                AnalyzeCurrentScene();
            }

            var requirements = sceneRequirements[selectedScene];
            int assignedCount = 0;

            // Cache all objects
            var allObjects = FindObjectsOfType<GameObject>(true);
            var objectsByName = new Dictionary<string, List<GameObject>>(StringComparer.OrdinalIgnoreCase);

            foreach (var go in allObjects)
            {
                if (!objectsByName.ContainsKey(go.name))
                    objectsByName[go.name] = new List<GameObject>();
                objectsByName[go.name].Add(go);
            }

            foreach (var req in requirements)
            {
                if (req.IsAssigned) continue;

                UnityEngine.Object found = FindMatchingObject(req, objectsByName, allObjects);

                if (found != null)
                {
                    Undo.RecordObject(req.Component, "Auto Assign Reference");
                    req.Field.SetValue(req.Component, found);
                    req.CurrentValue = found;
                    assignedCount++;
                    EditorUtility.SetDirty(req.Component);
                }
            }

            if (assignedCount > 0)
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }

            Debug.Log($"[DigitParkUISetup] {assignedCount} referencias asignadas automáticamente.");
            Repaint();
        }

        private UnityEngine.Object FindMatchingObject(FieldRequirement req, Dictionary<string, List<GameObject>> objectsByName, GameObject[] allObjects)
        {
            var possibleNames = GeneratePossibleNames(req.FieldName);
            Type fieldType = req.Field.FieldType;
            Transform root = req.Component.transform;

            // Strategy 1: Search in children
            foreach (string name in possibleNames)
            {
                var found = SearchInHierarchy(root, name, fieldType, true);
                if (found != null) return found;
            }

            // Strategy 2: Search by exact name in scene
            foreach (string name in possibleNames)
            {
                if (objectsByName.TryGetValue(name, out var matches))
                {
                    foreach (var go in matches)
                    {
                        var comp = GetComponent(go, fieldType);
                        if (comp != null) return comp;
                    }
                }
            }

            // Strategy 3: Partial match
            foreach (string name in possibleNames)
            {
                foreach (var go in allObjects)
                {
                    if (go.name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        var comp = GetComponent(go, fieldType);
                        if (comp != null) return comp;
                    }
                }
            }

            return null;
        }

        private UnityEngine.Object SearchInHierarchy(Transform root, string name, Type type, bool searchChildren)
        {
            if (searchChildren)
            {
                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        var comp = GetComponent(child.gameObject, type);
                        if (comp != null) return comp;
                    }
                }
            }
            return null;
        }

        private UnityEngine.Object GetComponent(GameObject go, Type type)
        {
            if (type == typeof(GameObject)) return go;
            if (type == typeof(Transform)) return go.transform;
            if (type == typeof(RectTransform)) return go.GetComponent<RectTransform>();
            if (typeof(Component).IsAssignableFrom(type)) return go.GetComponent(type);
            return null;
        }

        private List<string> GeneratePossibleNames(string fieldName)
        {
            var names = new List<string>();

            string clean = fieldName;
            if (clean.StartsWith("_")) clean = clean.Substring(1);
            if (clean.StartsWith("m_")) clean = clean.Substring(2);

            // Check predefined patterns
            if (FieldNamePatterns.TryGetValue(fieldName, out var patterns))
            {
                names.AddRange(patterns);
            }

            names.Add(fieldName);
            names.Add(clean);
            names.Add(char.ToUpper(clean[0]) + clean.Substring(1));

            // Remove common suffixes
            string[] suffixes = { "Button", "Text", "Input", "Panel", "Image", "Toggle", "Slider", "Field", "Container", "Prefab" };
            foreach (var suffix in suffixes)
            {
                if (clean.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    string withoutSuffix = clean.Substring(0, clean.Length - suffix.Length);
                    if (withoutSuffix.Length > 0)
                    {
                        names.Add(withoutSuffix);
                        names.Add(char.ToUpper(withoutSuffix[0]) + withoutSuffix.Substring(1));
                    }
                }
            }

            return names.Distinct().ToList();
        }

        private void CreateMissingUI()
        {
            if (!sceneRequirements.ContainsKey(selectedScene))
            {
                AnalyzeCurrentScene();
            }

            var requirements = sceneRequirements[selectedScene];
            var missing = requirements.Where(r => !r.IsAssigned).ToList();

            if (missing.Count == 0)
            {
                EditorUtility.DisplayDialog("Info", "Todos los campos ya están asignados.", "OK");
                return;
            }

            // Group by component
            var grouped = missing.GroupBy(r => r.Component);

            foreach (var group in grouped)
            {
                Transform parent = group.Key.transform;

                foreach (var req in group)
                {
                    CreateSingleUIElement(req);
                }
            }

            // Re-analyze and auto-assign
            AnalyzeCurrentScene();
            AutoAssignReferences();
        }

        private void CreateSingleUIElement(FieldRequirement req)
        {
            Transform parent = req.Component.transform;
            string name = req.SuggestedName;
            Type fieldType = req.Field.FieldType;

            // Check if already exists
            var existing = parent.Find(name);
            if (existing != null)
            {
                var comp = GetComponent(existing.gameObject, fieldType);
                if (comp != null)
                {
                    Undo.RecordObject(req.Component, "Assign Existing Reference");
                    req.Field.SetValue(req.Component, comp);
                    req.CurrentValue = comp;
                    EditorUtility.SetDirty(req.Component);
                    return;
                }
            }

            // Create new object
            GameObject go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            go.transform.SetParent(parent);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;

            // Add components based on type
            if (fieldType == typeof(Button) || fieldType.Name.Contains("Button"))
            {
                var rt = go.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(200, 50);
                go.AddComponent<Image>();
                go.AddComponent<Button>();

                // Add text child
                var textGo = new GameObject("Text");
                textGo.transform.SetParent(go.transform);
                var textRt = textGo.AddComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.sizeDelta = Vector2.zero;
                var tmp = textGo.AddComponent<TextMeshProUGUI>();
                tmp.text = name.Replace("Button", "").Replace("Btn", "");
                tmp.fontSize = FontSizes.Body;
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = FontSizes.AutoMinBody;
                tmp.fontSizeMax = tmp.fontSize;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
            }
            else if (fieldType == typeof(TextMeshProUGUI))
            {
                var rt = go.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(300, 50);
                var tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.text = name;
                tmp.fontSize = FontSizes.Body;
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = FontSizes.AutoMinBody;
                tmp.fontSizeMax = tmp.fontSize;
                tmp.alignment = TextAlignmentOptions.Center;
            }
            else if (fieldType == typeof(TMP_InputField))
            {
                var rt = go.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(300, 50);
                go.AddComponent<Image>();
                var input = go.AddComponent<TMP_InputField>();

                var textArea = new GameObject("Text Area");
                textArea.transform.SetParent(go.transform);
                var taRt = textArea.AddComponent<RectTransform>();
                taRt.anchorMin = Vector2.zero;
                taRt.anchorMax = Vector2.one;
                taRt.sizeDelta = new Vector2(-20, -10);

                var text = new GameObject("Text");
                text.transform.SetParent(textArea.transform);
                var tRt = text.AddComponent<RectTransform>();
                tRt.anchorMin = Vector2.zero;
                tRt.anchorMax = Vector2.one;
                tRt.sizeDelta = Vector2.zero;
                var tmp = text.AddComponent<TextMeshProUGUI>();
                tmp.fontSize = FontSizes.Body;
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = FontSizes.AutoMinBody;
                tmp.fontSizeMax = tmp.fontSize;
                input.textComponent = tmp;
            }
            else if (fieldType == typeof(Image))
            {
                var rt = go.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(100, 100);
                go.AddComponent<Image>();
            }
            else if (fieldType == typeof(Toggle))
            {
                var rt = go.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(200, 40);
                go.AddComponent<Toggle>();
            }
            else if (fieldType == typeof(Slider))
            {
                var rt = go.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(300, 30);
                go.AddComponent<Slider>();
            }
            else if (fieldType == typeof(Animator))
            {
                go.AddComponent<Animator>();
            }
            else if (fieldType == typeof(GameObject))
            {
                var rt = go.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(100, 100);
            }
            else if (fieldType == typeof(Transform) || fieldType == typeof(RectTransform))
            {
                go.AddComponent<RectTransform>();
            }
            else if (typeof(Component).IsAssignableFrom(fieldType))
            {
                try { go.AddComponent(fieldType); } catch { }
            }

            // Assign to field
            var created = GetComponent(go, fieldType);
            if (created != null)
            {
                Undo.RecordObject(req.Component, "Assign Created Reference");
                req.Field.SetValue(req.Component, created);
                req.CurrentValue = created;
                EditorUtility.SetDirty(req.Component);
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private void ProcessAllScenes()
        {
            if (!EditorUtility.DisplayDialog("Procesar Todas las Escenas",
                "Esto abrirá y procesará cada escena del proyecto.\n¿Continuar?", "Sí", "Cancelar"))
            {
                return;
            }

            string currentScene = SceneManager.GetActiveScene().path;
            int processed = 0;
            int totalAssigned = 0;

            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled) continue;

                try
                {
                    EditorSceneManager.OpenScene(scene.path);

                    selectedScene = System.IO.Path.GetFileNameWithoutExtension(scene.path);
                    AnalyzeCurrentScene();

                    int before = sceneRequirements[selectedScene].Count(r => r.IsAssigned);
                    AutoAssignReferences();
                    int after = sceneRequirements[selectedScene].Count(r => r.IsAssigned);

                    totalAssigned += (after - before);

                    if (after > before)
                    {
                        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                    }

                    processed++;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error procesando {scene.path}: {e.Message}");
                }
            }

            // Return to original scene
            if (!string.IsNullOrEmpty(currentScene))
            {
                EditorSceneManager.OpenScene(currentScene);
            }

            EditorUtility.DisplayDialog("Completado",
                $"Procesadas {processed} escenas.\n{totalAssigned} referencias asignadas en total.", "OK");
        }

        private void ExportReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("# DigitPark UI Setup Report");
            report.AppendLine($"Generated: {DateTime.Now}");
            report.AppendLine();

            foreach (var kvp in sceneRequirements)
            {
                report.AppendLine($"## {kvp.Key}");
                report.AppendLine();

                int assigned = kvp.Value.Count(r => r.IsAssigned);
                int total = kvp.Value.Count;
                report.AppendLine($"Status: {assigned}/{total} ({(assigned * 100f / total):F0}%)");
                report.AppendLine();

                foreach (var req in kvp.Value)
                {
                    string status = req.IsAssigned ? "[x]" : "[ ]";
                    report.AppendLine($"- {status} {req.FieldName} ({req.FieldType})");
                }
                report.AppendLine();
            }

            string path = EditorUtility.SaveFilePanel("Guardar Reporte", "", "UISetupReport.md", "md");
            if (!string.IsNullOrEmpty(path))
            {
                System.IO.File.WriteAllText(path, report.ToString());
                Debug.Log($"Reporte guardado en: {path}");
            }
        }

        private class FieldRequirement
        {
            public MonoBehaviour Component;
            public string ManagerName;
            public FieldInfo Field;
            public string FieldName;
            public string FieldType;
            public UnityEngine.Object CurrentValue;
            public string SuggestedName;
            public string Header;

            public bool IsAssigned => CurrentValue != null;
        }
    }
}
