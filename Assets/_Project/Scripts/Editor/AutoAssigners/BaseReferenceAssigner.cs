using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace DigitPark.Editor.AutoAssigners
{
    /// <summary>
    /// Base class for all reference assigners.
    /// Provides common methods to find and assign UI references.
    /// </summary>
    public abstract class BaseReferenceAssigner
    {
        protected static int assignedCount = 0;
        protected static int skippedCount = 0;
        protected static int failedCount = 0;
        protected static string log = "";

        protected static void ResetCounters()
        {
            assignedCount = 0;
            skippedCount = 0;
            failedCount = 0;
            log = "";
        }

        protected static void Log(string message)
        {
            log += message + "\n";
            Debug.Log($"[AutoAssigner] {message}");
        }

        protected static void MarkDirty(Object obj)
        {
            EditorUtility.SetDirty(obj);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        #region Find Methods

        protected static T FindInScene<T>(params string[] names) where T : Component
        {
            var allObjects = Object.FindObjectsOfType<T>(true);

            foreach (var name in names)
            {
                // Exact match first
                foreach (var obj in allObjects)
                {
                    if (obj.name == name)
                        return obj;
                }

                // Contains match
                foreach (var obj in allObjects)
                {
                    if (obj.name.Contains(name))
                        return obj;
                }
            }

            return null;
        }

        protected static GameObject FindGameObject(params string[] names)
        {
            var allObjects = Object.FindObjectsOfType<Transform>(true);

            foreach (var name in names)
            {
                // Exact match first
                foreach (var obj in allObjects)
                {
                    if (obj.name == name)
                        return obj.gameObject;
                }

                // Contains match
                foreach (var obj in allObjects)
                {
                    if (obj.name.Contains(name))
                        return obj.gameObject;
                }
            }

            return null;
        }

        protected static GameObject FindPrefab(string prefabName, string folder = "")
        {
            string[] searchPaths = new[]
            {
                $"Assets/_Project/Prefabs/{folder}/{prefabName}.prefab",
                $"Assets/_Project/Prefabs/Common/{prefabName}.prefab",
                $"Assets/_Project/Prefabs/Monetization/{prefabName}.prefab",
                $"Assets/_Project/Prefabs/{prefabName}.prefab"
            };

            foreach (var path in searchPaths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null) return prefab;
            }

            // Try Resources
            var fromResources = Resources.Load<GameObject>($"Prefabs/{prefabName}");
            if (fromResources != null) return fromResources;

            return null;
        }

        #endregion

        #region Assign Methods

        protected static bool TryAssign<T>(SerializedObject so, string propertyName, params string[] searchNames) where T : Component
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null)
            {
                failedCount++;
                Log($"  [X] {propertyName}: property not found in script");
                return false;
            }

            if (prop.objectReferenceValue != null)
            {
                skippedCount++;
                Log($"  [-] {propertyName}: already assigned");
                return true;
            }

            var obj = FindInScene<T>(searchNames);
            if (obj != null)
            {
                prop.objectReferenceValue = obj;
                assignedCount++;
                Log($"  [+] {propertyName} <- {obj.name}");
                return true;
            }

            failedCount++;
            Log($"  [X] {propertyName}: not found ({string.Join(", ", searchNames)})");
            return false;
        }

        protected static bool TryAssignButton(SerializedObject so, string propertyName, params string[] searchNames)
        {
            return TryAssign<Button>(so, propertyName, searchNames);
        }

        protected static bool TryAssignTMP(SerializedObject so, string propertyName, params string[] searchNames)
        {
            return TryAssign<TextMeshProUGUI>(so, propertyName, searchNames);
        }

        protected static bool TryAssignImage(SerializedObject so, string propertyName, params string[] searchNames)
        {
            return TryAssign<Image>(so, propertyName, searchNames);
        }

        protected static bool TryAssignSlider(SerializedObject so, string propertyName, params string[] searchNames)
        {
            return TryAssign<Slider>(so, propertyName, searchNames);
        }

        protected static bool TryAssignScrollRect(SerializedObject so, string propertyName, params string[] searchNames)
        {
            return TryAssign<ScrollRect>(so, propertyName, searchNames);
        }

        protected static bool TryAssignCanvasGroup(SerializedObject so, string propertyName, params string[] searchNames)
        {
            return TryAssign<CanvasGroup>(so, propertyName, searchNames);
        }

        protected static bool TryAssignGridLayout(SerializedObject so, string propertyName, params string[] searchNames)
        {
            return TryAssign<GridLayoutGroup>(so, propertyName, searchNames);
        }

        protected static bool TryAssignParticleSystem(SerializedObject so, string propertyName, params string[] searchNames)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) return false;
            if (prop.objectReferenceValue != null)
            {
                skippedCount++;
                return true;
            }

            var obj = FindInScene<ParticleSystem>(searchNames);
            if (obj != null)
            {
                prop.objectReferenceValue = obj;
                assignedCount++;
                Log($"  [+] {propertyName} <- {obj.name}");
                return true;
            }

            // Particles are optional
            Log($"  [?] {propertyName}: not found (optional)");
            return false;
        }

        protected static bool TryAssignTransform(SerializedObject so, string propertyName, params string[] searchNames)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null)
            {
                failedCount++;
                return false;
            }

            if (prop.objectReferenceValue != null)
            {
                skippedCount++;
                Log($"  [-] {propertyName}: already assigned");
                return true;
            }

            var obj = FindGameObject(searchNames);
            if (obj != null)
            {
                prop.objectReferenceValue = obj.transform;
                assignedCount++;
                Log($"  [+] {propertyName} <- {obj.name}");
                return true;
            }

            failedCount++;
            Log($"  [X] {propertyName}: not found");
            return false;
        }

        protected static bool TryAssignRectTransform(SerializedObject so, string propertyName, params string[] searchNames)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null)
            {
                failedCount++;
                return false;
            }

            if (prop.objectReferenceValue != null)
            {
                skippedCount++;
                return true;
            }

            var obj = FindGameObject(searchNames);
            if (obj != null)
            {
                var rt = obj.GetComponent<RectTransform>();
                if (rt != null)
                {
                    prop.objectReferenceValue = rt;
                    assignedCount++;
                    Log($"  [+] {propertyName} <- {obj.name}");
                    return true;
                }
            }

            failedCount++;
            Log($"  [X] {propertyName}: not found");
            return false;
        }

        protected static bool TryAssignGameObject(SerializedObject so, string propertyName, params string[] searchNames)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null)
            {
                failedCount++;
                return false;
            }

            if (prop.objectReferenceValue != null)
            {
                skippedCount++;
                Log($"  [-] {propertyName}: already assigned");
                return true;
            }

            var obj = FindGameObject(searchNames);
            if (obj != null)
            {
                prop.objectReferenceValue = obj;
                assignedCount++;
                Log($"  [+] {propertyName} <- {obj.name}");
                return true;
            }

            failedCount++;
            Log($"  [X] {propertyName}: not found");
            return false;
        }

        protected static bool TryAssignPrefab(SerializedObject so, string propertyName, string prefabName, string folder = "")
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null)
            {
                failedCount++;
                return false;
            }

            if (prop.objectReferenceValue != null)
            {
                skippedCount++;
                Log($"  [-] {propertyName}: already assigned");
                return true;
            }

            var prefab = FindPrefab(prefabName, folder);
            if (prefab != null)
            {
                prop.objectReferenceValue = prefab;
                assignedCount++;
                Log($"  [+] {propertyName} <- {prefab.name} (prefab)");
                return true;
            }

            failedCount++;
            Log($"  [X] {propertyName}: prefab '{prefabName}' not found");
            return false;
        }

        #endregion

        #region Find Manager

        protected static MonoBehaviour FindManager(string managerTypeName)
        {
            var allManagers = Object.FindObjectsOfType<MonoBehaviour>(true);
            foreach (var m in allManagers)
            {
                if (m.GetType().Name == managerTypeName)
                    return m;
            }
            return null;
        }

        #endregion
    }
}
