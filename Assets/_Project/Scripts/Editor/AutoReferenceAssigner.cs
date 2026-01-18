using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEditor.SceneManagement;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor tool para auto-asignar referencias SerializeField basándose en nombres del Hierarchy.
    /// Busca objetos que coincidan con el nombre del campo y los asigna automáticamente.
    /// </summary>
    public class AutoReferenceAssigner : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<AssignmentResult> lastResults = new List<AssignmentResult>();
        private bool showOnlyUnassigned = false;
        private bool includeInactive = true;
        private MonoBehaviour targetComponent;
        private string filterByType = "All";
        private string[] typeOptions = { "All", "Managers", "UI", "Custom" };

        [MenuItem("DigitPark/Tools/Auto Reference Assigner %#r", false, 2)]
        public static void ShowWindow()
        {
            var window = GetWindow<AutoReferenceAssigner>("Auto Reference Assigner");
            window.minSize = new Vector2(450, 500);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            // Header
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("Auto Reference Assigner", headerStyle);
            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "Esta herramienta busca automáticamente objetos en el Hierarchy que coincidan con los nombres de los campos SerializeField y los asigna.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Options
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Opciones", EditorStyles.boldLabel);

            includeInactive = EditorGUILayout.Toggle("Incluir objetos inactivos", includeInactive);
            showOnlyUnassigned = EditorGUILayout.Toggle("Solo mostrar no asignados", showOnlyUnassigned);

            EditorGUILayout.Space(5);

            // Target component (optional)
            targetComponent = (MonoBehaviour)EditorGUILayout.ObjectField(
                "Componente específico",
                targetComponent,
                typeof(MonoBehaviour),
                true);

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Action buttons
            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.4f, 0.8f, 1f);
            if (GUILayout.Button("Analizar Escena", GUILayout.Height(35)))
            {
                AnalyzeScene();
            }

            GUI.backgroundColor = new Color(0.4f, 1f, 0.6f);
            if (GUILayout.Button("Auto-Asignar Todo", GUILayout.Height(35)))
            {
                AutoAssignAll();
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Results
            if (lastResults.Count > 0)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                int assigned = lastResults.Count(r => r.wasAssigned);
                int failed = lastResults.Count(r => !r.wasAssigned && r.fieldValue == null);
                int alreadySet = lastResults.Count(r => !r.wasAssigned && r.fieldValue != null);

                EditorGUILayout.LabelField($"Resultados: {assigned} asignados, {alreadySet} ya tenían valor, {failed} sin encontrar", EditorStyles.boldLabel);

                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(5);

                // Scrollable results
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                string currentComponent = "";
                foreach (var result in lastResults)
                {
                    if (showOnlyUnassigned && (result.wasAssigned || result.fieldValue != null))
                        continue;

                    // Group by component
                    if (result.componentName != currentComponent)
                    {
                        currentComponent = result.componentName;
                        EditorGUILayout.Space(5);
                        EditorGUILayout.LabelField(currentComponent, EditorStyles.boldLabel);
                    }

                    DrawResultRow(result);
                }

                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.Space(10);

            // Quick actions
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Crear Estructura UI Faltante"))
            {
                CreateMissingUIStructure();
            }
            if (GUILayout.Button("Seleccionar No Asignados"))
            {
                SelectUnassignedObjects();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawResultRow(AssignmentResult result)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            // Status icon
            GUIStyle iconStyle = new GUIStyle(EditorStyles.label) { fontSize = 14 };
            if (result.wasAssigned)
            {
                GUI.color = Color.green;
                EditorGUILayout.LabelField("✓", iconStyle, GUILayout.Width(20));
            }
            else if (result.fieldValue != null)
            {
                GUI.color = Color.cyan;
                EditorGUILayout.LabelField("●", iconStyle, GUILayout.Width(20));
            }
            else
            {
                GUI.color = Color.red;
                EditorGUILayout.LabelField("✗", iconStyle, GUILayout.Width(20));
            }
            GUI.color = Color.white;

            // Field info
            EditorGUILayout.LabelField(result.fieldName, GUILayout.Width(150));
            EditorGUILayout.LabelField(result.fieldType, GUILayout.Width(120));

            // Found object or suggestion
            if (result.fieldValue != null)
            {
                if (GUILayout.Button(result.fieldValue.name, EditorStyles.linkLabel))
                {
                    Selection.activeObject = result.fieldValue;
                    EditorGUIUtility.PingObject(result.fieldValue);
                }
            }
            else
            {
                EditorGUILayout.LabelField($"Buscar: {result.suggestedName}", EditorStyles.miniLabel);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void AnalyzeScene()
        {
            lastResults.Clear();

            MonoBehaviour[] components;

            if (targetComponent != null)
            {
                components = new MonoBehaviour[] { targetComponent };
            }
            else
            {
                components = FindObjectsOfType<MonoBehaviour>(includeInactive);
            }

            foreach (var component in components)
            {
                if (component == null) continue;

                // Filter to project scripts only
                string namespaceName = component.GetType().Namespace ?? "";
                if (!namespaceName.StartsWith("DigitPark") && targetComponent == null)
                    continue;

                AnalyzeComponent(component);
            }

            Debug.Log($"[AutoReferenceAssigner] Análisis completado. {lastResults.Count} campos encontrados.");
        }

        private void AnalyzeComponent(MonoBehaviour component)
        {
            Type type = component.GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                // Check if it's a SerializeField
                bool isSerializable = field.IsPublic || field.GetCustomAttribute<SerializeField>() != null;
                if (!isSerializable) continue;

                // Skip non-Unity types
                if (!IsUnityObjectType(field.FieldType)) continue;

                object currentValue = field.GetValue(component);
                UnityEngine.Object unityValue = currentValue as UnityEngine.Object;

                lastResults.Add(new AssignmentResult
                {
                    component = component,
                    componentName = $"{component.gameObject.name} ({type.Name})",
                    field = field,
                    fieldName = field.Name,
                    fieldType = field.FieldType.Name,
                    fieldValue = unityValue,
                    wasAssigned = false,
                    suggestedName = GetSuggestedObjectName(field.Name)
                });
            }
        }

        private void AutoAssignAll()
        {
            if (lastResults.Count == 0)
            {
                AnalyzeScene();
            }

            int assignedCount = 0;

            // Cache all objects in scene for faster lookup
            var allObjects = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
            var allGameObjects = FindObjectsOfType<GameObject>(includeInactive);

            foreach (var go in allGameObjects)
            {
                if (!allObjects.ContainsKey(go.name))
                    allObjects[go.name] = go;
            }

            foreach (var result in lastResults)
            {
                if (result.fieldValue != null) continue; // Already assigned

                UnityEngine.Object foundObject = FindMatchingObject(result, allObjects, allGameObjects);

                if (foundObject != null)
                {
                    Undo.RecordObject(result.component, "Auto Assign Reference");
                    result.field.SetValue(result.component, foundObject);
                    result.fieldValue = foundObject;
                    result.wasAssigned = true;
                    assignedCount++;

                    EditorUtility.SetDirty(result.component);
                }
            }

            if (assignedCount > 0)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }

            Debug.Log($"[AutoReferenceAssigner] Auto-asignación completada. {assignedCount} referencias asignadas.");
        }

        private UnityEngine.Object FindMatchingObject(AssignmentResult result, Dictionary<string, GameObject> allObjects, GameObject[] allGameObjects)
        {
            string fieldName = result.fieldName;
            Type fieldType = result.field.FieldType;
            Transform componentRoot = result.component.transform;

            // Generate possible names
            List<string> possibleNames = GeneratePossibleNames(fieldName);

            // Strategy 1: Search in children first (most common case)
            foreach (string name in possibleNames)
            {
                var found = FindInChildren(componentRoot, name, fieldType);
                if (found != null) return found;
            }

            // Strategy 2: Search in parent hierarchy
            foreach (string name in possibleNames)
            {
                var found = FindInParent(componentRoot, name, fieldType);
                if (found != null) return found;
            }

            // Strategy 3: Search in entire scene
            foreach (string name in possibleNames)
            {
                if (allObjects.TryGetValue(name, out GameObject go))
                {
                    var found = GetComponentOfType(go, fieldType);
                    if (found != null) return found;
                }
            }

            // Strategy 4: Partial match in scene
            foreach (string name in possibleNames)
            {
                foreach (var go in allGameObjects)
                {
                    if (go.name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        name.IndexOf(go.name, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        var found = GetComponentOfType(go, fieldType);
                        if (found != null) return found;
                    }
                }
            }

            return null;
        }

        private UnityEngine.Object FindInChildren(Transform root, string name, Type type)
        {
            // Direct child search
            foreach (Transform child in root)
            {
                if (child.name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    var found = GetComponentOfType(child.gameObject, type);
                    if (found != null) return found;
                }
            }

            // Recursive search
            var allChildren = root.GetComponentsInChildren<Transform>(includeInactive);
            foreach (var child in allChildren)
            {
                if (child.name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    var found = GetComponentOfType(child.gameObject, type);
                    if (found != null) return found;
                }
            }

            return null;
        }

        private UnityEngine.Object FindInParent(Transform start, string name, Type type)
        {
            Transform current = start.parent;
            while (current != null)
            {
                if (current.name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    var found = GetComponentOfType(current.gameObject, type);
                    if (found != null) return found;
                }

                // Check siblings
                foreach (Transform sibling in current)
                {
                    if (sibling.name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        var found = GetComponentOfType(sibling.gameObject, type);
                        if (found != null) return found;
                    }
                }

                current = current.parent;
            }

            return null;
        }

        private UnityEngine.Object GetComponentOfType(GameObject go, Type type)
        {
            if (type == typeof(GameObject))
                return go;

            if (type == typeof(Transform))
                return go.transform;

            if (type == typeof(RectTransform))
                return go.GetComponent<RectTransform>();

            if (typeof(Component).IsAssignableFrom(type))
                return go.GetComponent(type);

            return null;
        }

        private List<string> GeneratePossibleNames(string fieldName)
        {
            var names = new List<string>();

            // Remove common prefixes
            string cleanName = fieldName;
            if (cleanName.StartsWith("_")) cleanName = cleanName.Substring(1);
            if (cleanName.StartsWith("m_")) cleanName = cleanName.Substring(2);

            // Original and clean name
            names.Add(fieldName);
            names.Add(cleanName);

            // PascalCase
            string pascalCase = char.ToUpper(cleanName[0]) + cleanName.Substring(1);
            names.Add(pascalCase);

            // Remove type suffixes for matching
            string[] suffixes = { "Button", "Text", "Input", "Panel", "Image", "Toggle", "Dropdown", "Slider", "Field", "Display", "Container", "Card", "Icon", "Label", "Animator", "UI" };
            foreach (var suffix in suffixes)
            {
                if (cleanName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    string withoutSuffix = cleanName.Substring(0, cleanName.Length - suffix.Length);
                    names.Add(withoutSuffix);
                    names.Add(char.ToUpper(withoutSuffix[0]) + withoutSuffix.Substring(1));
                }
            }

            // Add variations with spaces and underscores
            string withSpaces = System.Text.RegularExpressions.Regex.Replace(cleanName, "([a-z])([A-Z])", "$1 $2");
            names.Add(withSpaces);

            string withUnderscores = System.Text.RegularExpressions.Regex.Replace(cleanName, "([a-z])([A-Z])", "$1_$2");
            names.Add(withUnderscores);

            // Common UI naming patterns
            names.Add($"Btn{pascalCase}");
            names.Add($"Txt{pascalCase}");
            names.Add($"Img{pascalCase}");
            names.Add($"Pnl{pascalCase}");

            return names.Distinct().ToList();
        }

        private string GetSuggestedObjectName(string fieldName)
        {
            string clean = fieldName;
            if (clean.StartsWith("_")) clean = clean.Substring(1);
            if (clean.StartsWith("m_")) clean = clean.Substring(2);
            return char.ToUpper(clean[0]) + clean.Substring(1);
        }

        private bool IsUnityObjectType(Type type)
        {
            return typeof(UnityEngine.Object).IsAssignableFrom(type);
        }

        private void CreateMissingUIStructure()
        {
            if (lastResults.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "Primero analiza la escena.", "OK");
                return;
            }

            var unassigned = lastResults.Where(r => r.fieldValue == null).ToList();

            if (unassigned.Count == 0)
            {
                EditorUtility.DisplayDialog("Info", "Todos los campos ya están asignados.", "OK");
                return;
            }

            // Group by component
            var grouped = unassigned.GroupBy(r => r.component);

            foreach (var group in grouped)
            {
                Transform parent = group.Key.transform;

                // Create a container if needed
                GameObject container = new GameObject("UI_AutoGenerated");
                container.transform.SetParent(parent);
                container.transform.localPosition = Vector3.zero;

                foreach (var result in group)
                {
                    CreateUIElement(container.transform, result);
                }

                Undo.RegisterCreatedObjectUndo(container, "Create UI Structure");
            }

            // Re-analyze to update
            AnalyzeScene();
            AutoAssignAll();

            Debug.Log($"[AutoReferenceAssigner] Estructura UI creada para {unassigned.Count} elementos.");
        }

        private void CreateUIElement(Transform parent, AssignmentResult result)
        {
            string name = GetSuggestedObjectName(result.fieldName);
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.localPosition = Vector3.zero;

            Type fieldType = result.field.FieldType;

            // Add required components based on type
            if (fieldType == typeof(Button))
            {
                go.AddComponent<RectTransform>();
                go.AddComponent<Image>();
                go.AddComponent<Button>();
            }
            else if (fieldType == typeof(TextMeshProUGUI))
            {
                go.AddComponent<RectTransform>();
                go.AddComponent<TextMeshProUGUI>();
            }
            else if (fieldType == typeof(TMP_InputField))
            {
                go.AddComponent<RectTransform>();
                go.AddComponent<Image>();
                var input = go.AddComponent<TMP_InputField>();

                // Create text area
                var textArea = new GameObject("Text Area");
                textArea.transform.SetParent(go.transform);
                textArea.AddComponent<RectTransform>();

                var text = new GameObject("Text");
                text.transform.SetParent(textArea.transform);
                var tmp = text.AddComponent<TextMeshProUGUI>();
                input.textComponent = tmp;
            }
            else if (fieldType == typeof(Image))
            {
                go.AddComponent<RectTransform>();
                go.AddComponent<Image>();
            }
            else if (fieldType == typeof(Toggle))
            {
                go.AddComponent<RectTransform>();
                go.AddComponent<Toggle>();
            }
            else if (fieldType == typeof(Slider))
            {
                go.AddComponent<RectTransform>();
                go.AddComponent<Slider>();
            }
            else if (fieldType == typeof(Animator))
            {
                go.AddComponent<Animator>();
            }
            else if (fieldType == typeof(RectTransform))
            {
                go.AddComponent<RectTransform>();
            }
            else if (fieldType == typeof(GameObject))
            {
                // Just the GameObject is enough
            }
            else if (typeof(Component).IsAssignableFrom(fieldType))
            {
                // Try to add the component
                try
                {
                    go.AddComponent(fieldType);
                }
                catch
                {
                    Debug.LogWarning($"No se pudo crear componente de tipo {fieldType.Name}");
                }
            }
        }

        private void SelectUnassignedObjects()
        {
            var unassigned = lastResults.Where(r => r.fieldValue == null).Select(r => r.component.gameObject).Distinct().ToArray();
            Selection.objects = unassigned;
        }

        private class AssignmentResult
        {
            public MonoBehaviour component;
            public string componentName;
            public FieldInfo field;
            public string fieldName;
            public string fieldType;
            public UnityEngine.Object fieldValue;
            public bool wasAssigned;
            public string suggestedName;
        }

        // ==================== SHOP MANAGER SPECIFIC ====================

        [MenuItem("DigitPark/Tools/Auto Assign/Shop Manager References", false, 10)]
        public static void AssignShopManagerReferences()
        {
            var shopManager = UnityEngine.Object.FindObjectOfType<Monetization.ShopManager>();
            if (shopManager == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontró ShopManager en la escena.", "OK");
                return;
            }

            int count = 0;
            SerializedObject so = new SerializedObject(shopManager);

            // Tab References
            count += TryAssignButton(so, "_gemsTabButton", "GemsTab");
            count += TryAssignButton(so, "_coinsTabButton", "CoinsTab");
            count += TryAssignButton(so, "_themesTabButton", "CosmeticsTab", "ThemesTab");
            count += TryAssignButton(so, "_offersTabButton", "OffersTab");

            // Content References
            count += TryAssignGameObject(so, "_gemsContent", "GemsSection");
            count += TryAssignGameObject(so, "_coinsContent", "CoinsSection");
            count += TryAssignGameObject(so, "_themesContent", "ThemesSection", "CosmeticsSection");
            count += TryAssignGameObject(so, "_offersContent", "OffersSection", "SpecialOfferBanner");

            // Popups
            count += TryAssignGameObject(so, "_purchasePopup", "PurchasePopup");
            count += TryAssignGameObject(so, "_notEnoughGemsPopup", "NotEnoughPopup", "NotEnoughGemsPopup");

            // Popup UI References
            count += TryAssignImage(so, "_popupItemIcon", "PurchasePopup/Preview/Icon", "Icon");
            count += TryAssignTMP(so, "_popupItemName", "PurchasePopup/Preview/Amount", "ItemName");
            count += TryAssignTMP(so, "_popupItemPrice", "PurchasePopup/Price", "Price");
            count += TryAssignButton(so, "_popupConfirmButton", "ConfirmButton", "Confirm");
            count += TryAssignButton(so, "_popupCancelButton", "CancelButton", "Cancel");
            count += TryAssignButton(so, "_notEnoughCloseButton", "NotEnoughPopup/Buttons/CloseButton", "CloseButton");
            count += TryAssignButton(so, "_notEnoughGetGemsButton", "NotEnoughPopup/Buttons/GetGemsButton", "GetGemsButton");

            // Navigation
            count += TryAssignButton(so, "_backButton", "BackButton");

            // Currency Display
            count += TryAssignComponent<Monetization.CurrencyDisplayUI>(so, "_gemsDisplay", "GemsDisplay");
            count += TryAssignComponent<Monetization.CurrencyDisplayUI>(so, "_coinsDisplay", "CoinsDisplay");
            count += TryAssignTMP(so, "_headerGemsText", "GemsDisplay/Amount", "GemsAmount");
            count += TryAssignTMP(so, "_headerCoinsText", "CoinsDisplay/Amount", "CoinsAmount");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(shopManager);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("Shop Manager",
                $"Se asignaron {count} referencias al ShopManager.",
                "OK");

            Debug.Log($"[AutoReferenceAssigner] ShopManager: {count} referencias asignadas.");
        }

        private static int TryAssignButton(SerializedObject so, string propName, params string[] searchNames)
        {
            SerializedProperty prop = so.FindProperty(propName);
            if (prop == null || prop.objectReferenceValue != null) return 0;

            foreach (string name in searchNames)
            {
                var found = FindObjectByPath<Button>(name);
                if (found != null)
                {
                    prop.objectReferenceValue = found;
                    Debug.Log($"[AutoRef] Assigned {propName} = {found.name}");
                    return 1;
                }
            }
            return 0;
        }

        private static int TryAssignGameObject(SerializedObject so, string propName, params string[] searchNames)
        {
            SerializedProperty prop = so.FindProperty(propName);
            if (prop == null || prop.objectReferenceValue != null) return 0;

            foreach (string name in searchNames)
            {
                var found = FindObjectByPath<Transform>(name);
                if (found != null)
                {
                    prop.objectReferenceValue = found.gameObject;
                    Debug.Log($"[AutoRef] Assigned {propName} = {found.name}");
                    return 1;
                }
            }
            return 0;
        }

        private static int TryAssignImage(SerializedObject so, string propName, params string[] searchNames)
        {
            SerializedProperty prop = so.FindProperty(propName);
            if (prop == null || prop.objectReferenceValue != null) return 0;

            foreach (string name in searchNames)
            {
                var found = FindObjectByPath<Image>(name);
                if (found != null)
                {
                    prop.objectReferenceValue = found;
                    Debug.Log($"[AutoRef] Assigned {propName} = {found.name}");
                    return 1;
                }
            }
            return 0;
        }

        private static int TryAssignTMP(SerializedObject so, string propName, params string[] searchNames)
        {
            SerializedProperty prop = so.FindProperty(propName);
            if (prop == null || prop.objectReferenceValue != null) return 0;

            foreach (string name in searchNames)
            {
                var found = FindObjectByPath<TextMeshProUGUI>(name);
                if (found != null)
                {
                    prop.objectReferenceValue = found;
                    Debug.Log($"[AutoRef] Assigned {propName} = {found.name}");
                    return 1;
                }
            }
            return 0;
        }

        private static int TryAssignComponent<T>(SerializedObject so, string propName, params string[] searchNames) where T : Component
        {
            SerializedProperty prop = so.FindProperty(propName);
            if (prop == null || prop.objectReferenceValue != null) return 0;

            foreach (string name in searchNames)
            {
                var found = FindObjectByPath<T>(name);
                if (found != null)
                {
                    prop.objectReferenceValue = found;
                    Debug.Log($"[AutoRef] Assigned {propName} = {found.name}");
                    return 1;
                }
            }
            return 0;
        }

        private static T FindObjectByPath<T>(string path) where T : Component
        {
            // First try exact path
            GameObject go = GameObject.Find(path);
            if (go != null)
            {
                T comp = go.GetComponent<T>();
                if (comp != null) return comp;
            }

            // Try just the name (last part of path)
            string name = path.Contains("/") ? path.Substring(path.LastIndexOf('/') + 1) : path;

            // Search all objects
            var allObjects = UnityEngine.Object.FindObjectsOfType<T>(true);
            foreach (var obj in allObjects)
            {
                if (obj.name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return obj;
            }

            // Partial match
            foreach (var obj in allObjects)
            {
                if (obj.name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                    return obj;
            }

            return null;
        }

        // ==================== DAILY REWARDS MANAGER SPECIFIC ====================

        [MenuItem("DigitPark/Tools/Auto Assign/Daily Rewards Manager References", false, 11)]
        public static void AssignDailyRewardsManagerReferences()
        {
            var manager = UnityEngine.Object.FindObjectOfType<Managers.DailyRewardsManager>();
            if (manager == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontró DailyRewardsManager en la escena.", "OK");
                return;
            }

            int count = 0;
            SerializedObject so = new SerializedObject(manager);

            // Day cards
            for (int i = 1; i <= 6; i++)
            {
                count += TryAssignGameObject(so, $"_day{i}Card", $"Day{i}");
            }
            count += TryAssignGameObject(so, "_day7Card", "Day7Special", "Day7");

            // UI Elements
            count += TryAssignTMP(so, "_streakText", "StreakCount", "StreakValue");
            count += TryAssignTMP(so, "_timerText", "TimerText", "Countdown");
            count += TryAssignButton(so, "_claimButton", "ClaimButton");

            // Popups
            count += TryAssignGameObject(so, "_claimCelebration", "ClaimCelebration");
            count += TryAssignGameObject(so, "_streakLostPopup", "StreakLostPopup");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("Daily Rewards Manager",
                $"Se asignaron {count} referencias.",
                "OK");
        }

        // ==================== BATTLE PASS MANAGER SPECIFIC ====================

        [MenuItem("DigitPark/Tools/Auto Assign/BattlePass Manager References", false, 12)]
        public static void AssignBattlePassManagerReferences()
        {
            // Find BattlePassManager
            var managers = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true);
            MonoBehaviour manager = null;
            foreach (var m in managers)
            {
                if (m.GetType().Name == "BattlePassManager")
                {
                    manager = m;
                    break;
                }
            }

            if (manager == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontró BattlePassManager en la escena.", "OK");
                return;
            }

            int count = 0;
            SerializedObject so = new SerializedObject(manager);

            // Common references
            count += TryAssignButton(so, "_backButton", "BackButton");
            count += TryAssignTMP(so, "_seasonText", "SeasonText", "SeasonTitle");
            count += TryAssignTMP(so, "_levelText", "LevelText", "CurrentLevel");
            count += TryAssignGameObject(so, "_tiersContainer", "TiersContainer", "Content");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("BattlePass Manager",
                $"Se asignaron {count} referencias.",
                "OK");
        }

        // ==================== ACHIEVEMENTS MANAGER SPECIFIC ====================

        [MenuItem("DigitPark/Tools/Auto Assign/Achievements Manager References", false, 13)]
        public static void AssignAchievementsManagerReferences()
        {
            var managers = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true);
            MonoBehaviour manager = null;
            foreach (var m in managers)
            {
                if (m.GetType().Name == "AchievementsManager")
                {
                    manager = m;
                    break;
                }
            }

            if (manager == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontró AchievementsManager en la escena.", "OK");
                return;
            }

            int count = 0;
            SerializedObject so = new SerializedObject(manager);

            count += TryAssignButton(so, "_backButton", "BackButton");
            count += TryAssignGameObject(so, "_trophyGrid", "TrophyGrid", "Content");
            count += TryAssignGameObject(so, "_detailPanel", "DetailPanel");
            count += TryAssignTMP(so, "_progressText", "ProgressText", "TotalProgress");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("Achievements Manager",
                $"Se asignaron {count} referencias.",
                "OK");
        }
    }
}
