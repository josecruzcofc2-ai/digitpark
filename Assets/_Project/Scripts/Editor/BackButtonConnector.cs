using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace DigitPark.Editor
{
    public class BackButtonConnector
    {
        [MenuItem("DigitPark/Connect BackButtons in All Scenes")]
        public static void ConnectBackButtonsInAllScenes()
        {
            string[] scenes = new string[]
            {
                "Assets/_Project/Scenes/Game.unity",
                "Assets/_Project/Scenes/Register.unity",
                "Assets/_Project/Scenes/Scores.unity",
                "Assets/_Project/Scenes/Settings.unity",
                "Assets/_Project/Scenes/Tournaments.unity"
            };

            int connected = 0;

            foreach (string scenePath in scenes)
            {
                if (ConnectBackButtonInScene(scenePath))
                {
                    connected++;
                }
            }

            EditorUtility.DisplayDialog("BackButtons Conectados",
                $"Se conectaron {connected} BackButtons en las escenas.\n\n" +
                "Los cambios han sido guardados.",
                "OK");
        }

        private static bool ConnectBackButtonInScene(string scenePath)
        {
            // Abrir la escena
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Buscar el BackButton
            Button backButton = null;
            foreach (var go in scene.GetRootGameObjects())
            {
                backButton = FindBackButtonRecursive(go);
                if (backButton != null) break;
            }

            if (backButton == null)
            {
                Debug.LogWarning($"No se encontró BackButton en {scenePath}");
                return false;
            }

            bool connected = false;
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            // Conectar según la escena
            switch (sceneName)
            {
                case "Game":
                    connected = ConnectToManager<DigitPark.Managers.GameManager>(backButton, "backButton");
                    break;
                case "Register":
                    connected = ConnectToManager<DigitPark.Managers.RegisterManager>(backButton, "backButton");
                    break;
                case "Scores":
                    connected = ConnectToManager<DigitPark.Managers.LeaderboardManager>(backButton, "backButton");
                    break;
                case "Settings":
                    connected = ConnectToManager<DigitPark.Managers.SettingsManager>(backButton, "backButton");
                    break;
                case "Tournaments":
                    connected = ConnectToManager<DigitPark.Managers.TournamentManager>(backButton, "backButton");
                    break;
            }

            if (connected)
            {
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"BackButton conectado en {sceneName}");
            }

            return connected;
        }

        private static Button FindBackButtonRecursive(GameObject go)
        {
            if (go.name == "BackButton")
            {
                var btn = go.GetComponent<Button>();
                if (btn != null) return btn;
            }

            foreach (Transform child in go.transform)
            {
                var result = FindBackButtonRecursive(child.gameObject);
                if (result != null) return result;
            }

            return null;
        }

        private static bool ConnectToManager<T>(Button backButton, string fieldName) where T : MonoBehaviour
        {
            T manager = Object.FindObjectOfType<T>();
            if (manager == null)
            {
                Debug.LogWarning($"No se encontró {typeof(T).Name}");
                return false;
            }

            var field = typeof(T).GetField(fieldName,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            if (field == null)
            {
                Debug.LogWarning($"No se encontró el campo {fieldName} en {typeof(T).Name}");
                return false;
            }

            field.SetValue(manager, backButton);
            EditorUtility.SetDirty(manager);

            return true;
        }

        [MenuItem("DigitPark/Connect BackButton in Current Scene")]
        public static void ConnectBackButtonInCurrentScene()
        {
            // Buscar el BackButton
            Button backButton = null;
            foreach (var go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                backButton = FindBackButtonRecursive(go);
                if (backButton != null) break;
            }

            if (backButton == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontró un GameObject llamado 'BackButton' con componente Button.", "OK");
                return;
            }

            // Intentar conectar a cualquier manager que tenga backButton
            bool connected = false;

            // GameManager
            var gameManager = Object.FindObjectOfType<DigitPark.Managers.GameManager>();
            if (gameManager != null)
            {
                gameManager.backButton = backButton;
                EditorUtility.SetDirty(gameManager);
                connected = true;
            }

            // RegisterManager
            var registerManager = Object.FindObjectOfType<DigitPark.Managers.RegisterManager>();
            if (registerManager != null)
            {
                registerManager.backButton = backButton;
                EditorUtility.SetDirty(registerManager);
                connected = true;
            }

            // LeaderboardManager
            var leaderboardManager = Object.FindObjectOfType<DigitPark.Managers.LeaderboardManager>();
            if (leaderboardManager != null)
            {
                leaderboardManager.backButton = backButton;
                EditorUtility.SetDirty(leaderboardManager);
                connected = true;
            }

            // TournamentManager
            var tournamentManager = Object.FindObjectOfType<DigitPark.Managers.TournamentManager>();
            if (tournamentManager != null)
            {
                tournamentManager.backButton = backButton;
                EditorUtility.SetDirty(tournamentManager);
                connected = true;
            }

            // SettingsManager (tiene backButton privado, necesita reflection)
            var settingsManager = Object.FindObjectOfType<DigitPark.Managers.SettingsManager>();
            if (settingsManager != null)
            {
                var field = typeof(DigitPark.Managers.SettingsManager).GetField("backButton",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(settingsManager, backButton);
                    EditorUtility.SetDirty(settingsManager);
                    connected = true;
                }
            }

            if (connected)
            {
                EditorUtility.DisplayDialog("Conectado",
                    "BackButton conectado al manager de esta escena.\n\nRecuerda guardar la escena (Ctrl+S).",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "No se encontró ningún manager compatible en esta escena.", "OK");
            }
        }
    }
}
