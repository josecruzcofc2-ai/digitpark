using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace DigitPark.Editor
{
    /// <summary>
    /// Gestiona la estructura de prefabs del proyecto.
    /// Organiza, valida y crea prefabs faltantes.
    /// </summary>
    public class PrefabStructureManager : EditorWindow
    {
        private Vector2 scrollPos;
        private bool showMissing = true;
        private bool showStructure = true;

        // Estructura de carpetas esperada - ACTUALIZADA
        private static readonly Dictionary<string, string[]> ExpectedStructure = new Dictionary<string, string[]>
        {
            // Common - Elementos compartidos en toda la app
            { "Common", new[] { "ConfirmPanel", "ErrorPanel", "BackButton", "PlayerCard" } },

            // Animation - Efectos y animaciones
            { "Animation", new[] { "TransitionCanvas", "UIAnimationManager", "ParticleEffectSpawner", "RewardClaimAnimator", "Button3D" } },

            // Games - Paneles de victoria/derrota
            { "Games/WinPanels", new[] { "WinPanel_Normal", "LosePanel_Normal", "WinPanel_RealMoney", "LosePanel_RealMoney" } },

            // CashBattle - Sistema de dinero real
            { "CashBattle/Wallet", new[] { "TransactionItemUI", "DepositOptionUI" } },
            { "CashBattle/History", new[] { "MatchHistoryItem" } },
            { "CashBattle/Tournaments", new[] { "TournamentCardUI" } },

            // Monetization - Sistema de monetización
            { "Monetization", new[] { "TrophyCard" } },
            { "Monetization/Achievements", new[] { "AchievementItem", "CategoryHeader" } },
            { "Monetization/DailyMissions", new[] { "MissionItem" } },
            { "Monetization/DailyRewards", new[] { "RewardDayItem" } },

            // Social - Sistema social
            { "Social", new[] { "PlayerSearchItem", "LeaderboardEntry" } },

            // Tournaments - Torneos normales
            { "Tournaments/Browser", new[] { "TournamentItem", "TournamentSearchItem", "TournamentMyItem" } },
            { "Tournaments/Lobby", new[] { "PrizeRowItem", "ParticipantItem" } },

            // Onboarding
            { "Onboarding", new[] { "StepDotItem", "AvatarOptionItem" } }
        };

        private const string BASE_PATH = "Assets/_Project/Prefabs";

        [MenuItem("DigitPark/Tools/Prefab Structure Manager", false, 50)]
        public static void ShowWindow()
        {
            GetWindow<PrefabStructureManager>("Prefab Manager");
        }

        private void OnGUI()
        {
            GUILayout.Label("Prefab Structure Manager", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            // === ANÁLISIS ===
            showStructure = EditorGUILayout.Foldout(showStructure, "Estructura Esperada", true);
            if (showStructure)
            {
                DrawExpectedStructure();
            }

            EditorGUILayout.Space(10);

            showMissing = EditorGUILayout.Foldout(showMissing, "Prefabs Faltantes", true);
            if (showMissing)
            {
                DrawMissingPrefabs();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);

            // === ACCIONES ===
            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("Crear Carpetas Faltantes", GUILayout.Height(35)))
            {
                CreateMissingFolders();
            }

            GUI.backgroundColor = new Color(0.3f, 0.6f, 1f);
            if (GUILayout.Button("Reorganizar Prefabs", GUILayout.Height(35)))
            {
                ReorganizePrefabs();
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            GUI.backgroundColor = new Color(1f, 0.8f, 0.3f);
            if (GUILayout.Button("Validar Estructura", GUILayout.Height(30)))
            {
                ValidateStructure();
            }
            GUI.backgroundColor = Color.white;
        }

        private void DrawExpectedStructure()
        {
            EditorGUI.indentLevel++;
            foreach (var folder in ExpectedStructure)
            {
                EditorGUILayout.LabelField($"/{folder.Key}/", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                foreach (var prefab in folder.Value)
                {
                    string fullPath = $"{BASE_PATH}/{folder.Key}/{prefab}.prefab";
                    bool exists = File.Exists(fullPath);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(exists ? "✓" : "✗", GUILayout.Width(20));
                    EditorGUILayout.LabelField($"{prefab}.prefab", exists ? EditorStyles.label : EditorStyles.boldLabel);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(3);
            }
            EditorGUI.indentLevel--;
        }

        private void DrawMissingPrefabs()
        {
            var missing = GetMissingPrefabs();

            if (missing.Count == 0)
            {
                EditorGUILayout.HelpBox("¡Todos los prefabs están presentes!", MessageType.Info);
                return;
            }

            EditorGUILayout.HelpBox($"Faltan {missing.Count} prefabs", MessageType.Warning);

            EditorGUI.indentLevel++;
            foreach (var path in missing)
            {
                EditorGUILayout.LabelField($"• {path}");
            }
            EditorGUI.indentLevel--;
        }

        private List<string> GetMissingPrefabs()
        {
            var missing = new List<string>();

            foreach (var folder in ExpectedStructure)
            {
                foreach (var prefab in folder.Value)
                {
                    string fullPath = $"{BASE_PATH}/{folder.Key}/{prefab}.prefab";
                    if (!File.Exists(fullPath))
                    {
                        missing.Add($"{folder.Key}/{prefab}.prefab");
                    }
                }
            }

            return missing;
        }

        private void CreateMissingFolders()
        {
            foreach (var folder in ExpectedStructure.Keys)
            {
                string fullPath = $"{BASE_PATH}/{folder}";
                if (!AssetDatabase.IsValidFolder(fullPath))
                {
                    CreateFolderRecursive(fullPath);
                    Debug.Log($"[PrefabManager] Carpeta creada: {fullPath}");
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("[PrefabManager] Estructura de carpetas completada!");
        }

        private void CreateFolderRecursive(string path)
        {
            string[] parts = path.Split('/');
            string currentPath = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string newPath = $"{currentPath}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    AssetDatabase.CreateFolder(currentPath, parts[i]);
                }
                currentPath = newPath;
            }
        }

        private void ReorganizePrefabs()
        {
            if (!EditorUtility.DisplayDialog("Reorganizar Prefabs",
                "Esto moverá prefabs a sus carpetas correctas según la estructura definida.\n\n" +
                "Se crearán carpetas faltantes.\n" +
                "Los prefabs duplicados serán consolidados.\n\n" +
                "¿Continuar?",
                "Sí, Reorganizar", "Cancelar"))
            {
                return;
            }

            // Primero crear carpetas
            CreateMissingFolders();

            int moved = 0;

            // Mover prefabs a sus ubicaciones correctas
            // CashBattle/TournamentCardUI -> CashBattle/Tournaments/
            moved += TryMovePrefab("CashBattle/TournamentCardUI.prefab", "CashBattle/Tournaments/TournamentCardUI.prefab");

            // CashBattle raíz -> Wallet
            moved += TryMovePrefab("CashBattle/TransactionItemUI.prefab", "CashBattle/Wallet/TransactionItemUI.prefab");
            moved += TryMovePrefab("CashBattle/DepositOptionUI.prefab", "CashBattle/Wallet/DepositOptionUI.prefab");
            moved += TryMovePrefab("CashBattle/HistoryEntryItemUI.prefab", "CashBattle/History/MatchHistoryItem.prefab");

            // UI/Scores -> Social
            moved += TryMovePrefab("UI/Scores/LeaderboardEntry.prefab", "Social/LeaderboardEntry.prefab");

            // UI/Tournaments -> Tournaments/Browser
            moved += TryMovePrefab("UI/Tournaments/TournamentItem.prefab", "Tournaments/Browser/TournamentItem.prefab");
            moved += TryMovePrefab("UI/Tournaments/TournamentSearchItem.prefab", "Tournaments/Browser/TournamentSearchItem.prefab");
            moved += TryMovePrefab("UI/Tournaments/TournamentMyItem.prefab", "Tournaments/Browser/TournamentMyItem.prefab");

            // UI/WinPanels -> Games/WinPanels
            string[] winPanels = { "WinPanel_Normal", "LosePanel_Normal", "WinPanel_RealMoney", "LosePanel_RealMoney" };
            foreach (var panel in winPanels)
            {
                moved += TryMovePrefab($"UI/WinPanels/{panel}.prefab", $"Games/WinPanels/{panel}.prefab");
            }

            // UI raíz -> Common
            moved += TryMovePrefab("UI/BackButton.prefab", "Common/BackButton.prefab");
            moved += TryMovePrefab("UI/PlayerCard.prefab", "Common/PlayerCard.prefab");

            AssetDatabase.Refresh();
            Debug.Log($"[PrefabManager] Reorganización completada. {moved} prefabs movidos.");
        }

        private int TryMovePrefab(string from, string to)
        {
            string fromPath = $"{BASE_PATH}/{from}";
            string toPath = $"{BASE_PATH}/{to}";

            if (!File.Exists(fromPath))
            {
                return 0;
            }

            if (File.Exists(toPath))
            {
                Debug.LogWarning($"[PrefabManager] Ya existe: {toPath}. Saltando.");
                return 0;
            }

            string error = AssetDatabase.MoveAsset(fromPath, toPath);
            if (string.IsNullOrEmpty(error))
            {
                Debug.Log($"[PrefabManager] Movido: {from} -> {to}");
                return 1;
            }
            else
            {
                Debug.LogError($"[PrefabManager] Error moviendo {from}: {error}");
                return 0;
            }
        }

        private void ValidateStructure()
        {
            var missing = GetMissingPrefabs();
            var issues = new List<string>();

            // Buscar prefabs duplicados
            var allPrefabs = AssetDatabase.FindAssets("t:Prefab", new[] { BASE_PATH });
            var prefabNames = new Dictionary<string, List<string>>();

            foreach (var guid in allPrefabs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string name = Path.GetFileNameWithoutExtension(path);

                if (!prefabNames.ContainsKey(name))
                    prefabNames[name] = new List<string>();

                prefabNames[name].Add(path);
            }

            foreach (var kvp in prefabNames)
            {
                if (kvp.Value.Count > 1)
                {
                    issues.Add($"Duplicado: {kvp.Key} ({kvp.Value.Count} copias)");
                }
            }

            // Mostrar resultados
            string message = $"=== Validación de Estructura ===\n\n";
            message += $"Prefabs faltantes: {missing.Count}\n";
            message += $"Duplicados: {issues.Count}\n\n";

            if (missing.Count > 0)
            {
                message += "FALTANTES:\n";
                foreach (var m in missing) message += $"  • {m}\n";
            }

            if (issues.Count > 0)
            {
                message += "\nDUPLICADOS:\n";
                foreach (var i in issues) message += $"  • {i}\n";
            }

            if (missing.Count == 0 && issues.Count == 0)
            {
                message += "¡Estructura correcta!";
            }

            Debug.Log(message);
            EditorUtility.DisplayDialog("Validación", message, "OK");
        }
    }
}
