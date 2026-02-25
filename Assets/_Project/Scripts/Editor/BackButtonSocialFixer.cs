using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace DigitPark.Editor
{
    /// <summary>
    /// Assigns the Neon Cyan BackButton prefab (with icon) to the 5 Social scenes
    /// that currently have manual text-based back buttons.
    /// Target: Scores, Profile, MatchHistory, Friends, FriendRequests
    /// </summary>
    public static class BackButtonSocialFixer
    {
        private const string BACK_BUTTON_PREFAB_PATH = "Assets/_Project/Prefabs/Common/BackButton.prefab";

        private static readonly string[] TARGET_SCENES = new string[]
        {
            "Assets/_Project/Scenes/Social/Profile/Scores.unity",
            "Assets/_Project/Scenes/Social/Profile/Profile.unity",
            "Assets/_Project/Scenes/Social/Profile/MatchHistory.unity",
            "Assets/_Project/Scenes/Social/Friends/Friends.unity",
            "Assets/_Project/Scenes/Social/Friends/FriendRequests.unity"
        };

        [MenuItem("DigitPark/UI/Fix BackButton Neon in Social Scenes", false, 204)]
        public static void FixSocialBackButtons()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BACK_BUTTON_PREFAB_PATH);
            if (prefab == null)
            {
                Debug.LogError($"[SocialFixer] Prefab not found at {BACK_BUTTON_PREFAB_PATH}");
                return;
            }

            string currentScenePath = EditorSceneManager.GetActiveScene().path;

            int fixedCount = 0;

            Debug.Log("[SocialFixer] === Fixing Neon BackButton in 5 Social scenes ===");

            foreach (string scenePath in TARGET_SCENES)
            {
                if (FixScene(scenePath, prefab))
                    fixedCount++;
            }

            Debug.Log($"[SocialFixer] === Done! Fixed: {fixedCount}/5 ===");

            if (!string.IsNullOrEmpty(currentScenePath))
                EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);

            AssetDatabase.SaveAssets();
        }

        private static bool FixScene(string scenePath, GameObject prefab)
        {
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            try
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
                if (canvas == null)
                {
                    Debug.LogError($"[SocialFixer] {sceneName}: No Canvas");
                    return false;
                }

                // Find Header
                Transform header = canvas.transform.Find("Header");
                if (header == null)
                {
                    Debug.LogError($"[SocialFixer] {sceneName}: No Header found");
                    return false;
                }

                // Remove old manual BackButton and all children
                Transform oldBtn = header.Find("BackButton");
                if (oldBtn != null)
                {
                    Object.DestroyImmediate(oldBtn.gameObject);
                    Debug.Log($"[SocialFixer] {sceneName}: Removed old manual BackButton");
                }

                // Instantiate neon cyan prefab
                GameObject backBtn = (GameObject)PrefabUtility.InstantiatePrefab(prefab, header);
                backBtn.name = "BackButton";

                RectTransform rt = backBtn.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 0.5f);
                rt.anchorMax = new Vector2(0, 0.5f);
                rt.pivot = new Vector2(0, 0.5f);
                rt.anchoredPosition = new Vector2(15, 0);
                rt.sizeDelta = new Vector2(50, 50);

                backBtn.transform.SetAsFirstSibling();

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);

                Debug.Log($"[SocialFixer] {sceneName}: Neon Cyan BackButton assigned");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SocialFixer] {sceneName}: Error - {e.Message}");
                return false;
            }
        }
    }
}
