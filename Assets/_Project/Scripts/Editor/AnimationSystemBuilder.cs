#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor tools for creating and setting up the animation system.
    /// Access via DigitPark > Animation System menu.
    /// </summary>
    public class AnimationSystemBuilder : EditorWindow
    {
        private const string PREFAB_PATH = "Assets/_Project/Prefabs/Animation";
        private const string MATERIAL_PATH = "Assets/_Project/Materials/UI";

        [MenuItem("DigitPark/Animation/Setup Animation System", false, 400)]
        public static void SetupAnimationSystem()
        {
            EnsureDirectoriesExist();
            CreateAnimationManagers();
            CreateMaterials();

            Debug.Log("<color=cyan>[DigitPark]</color> Animation System setup complete!");
            EditorUtility.DisplayDialog("Animation System", "Animation system has been set up successfully!\n\nCreated:\n- UIAnimationManager\n- SceneTransitionManager\n- ParticleEffectSpawner\n- UI Materials", "OK");
        }

        [MenuItem("DigitPark/Animation/Create Button3D Prefab", false, 401)]
        public static void CreateButton3DPrefab()
        {
            EnsureDirectoriesExist();

            // Create button structure
            GameObject buttonObj = new GameObject("Button3D");

            // Add Canvas (for testing in prefab mode)
            RectTransform buttonRT = buttonObj.AddComponent<RectTransform>();
            buttonRT.sizeDelta = new Vector2(200, 60);

            // Add Button component
            Button button = buttonObj.AddComponent<Button>();
            Image buttonBg = buttonObj.AddComponent<Image>();
            buttonBg.color = new Color(0, 0, 0, 0); // Transparent background

            // Create shadow
            GameObject shadowObj = new GameObject("Shadow");
            shadowObj.transform.SetParent(buttonObj.transform, false);
            RectTransform shadowRT = shadowObj.AddComponent<RectTransform>();
            shadowRT.anchorMin = Vector2.zero;
            shadowRT.anchorMax = Vector2.one;
            shadowRT.sizeDelta = new Vector2(0, -6);
            shadowRT.anchoredPosition = new Vector2(0, -3);
            Image shadowImage = shadowObj.AddComponent<Image>();
            shadowImage.color = new Color(0, 0.5f, 0.5f, 1f);

            // Create face
            GameObject faceObj = new GameObject("Face");
            faceObj.transform.SetParent(buttonObj.transform, false);
            RectTransform faceRT = faceObj.AddComponent<RectTransform>();
            faceRT.anchorMin = Vector2.zero;
            faceRT.anchorMax = Vector2.one;
            faceRT.sizeDelta = new Vector2(0, -6);
            faceRT.anchoredPosition = new Vector2(0, 3);
            Image faceImage = faceObj.AddComponent<Image>();
            faceImage.color = new Color(0, 1f, 1f, 1f);

            // Create highlight line
            GameObject highlightObj = new GameObject("Highlight");
            highlightObj.transform.SetParent(faceObj.transform, false);
            RectTransform highlightRT = highlightObj.AddComponent<RectTransform>();
            highlightRT.anchorMin = new Vector2(0.1f, 0.8f);
            highlightRT.anchorMax = new Vector2(0.9f, 0.9f);
            highlightRT.sizeDelta = Vector2.zero;
            Image highlightImage = highlightObj.AddComponent<Image>();
            highlightImage.color = new Color(1f, 1f, 1f, 0.3f);

            // Create glow
            GameObject glowObj = new GameObject("Glow");
            glowObj.transform.SetParent(buttonObj.transform, false);
            glowObj.transform.SetAsFirstSibling();
            RectTransform glowRT = glowObj.AddComponent<RectTransform>();
            glowRT.anchorMin = Vector2.zero;
            glowRT.anchorMax = Vector2.one;
            glowRT.sizeDelta = new Vector2(20, 20);
            Image glowImage = glowObj.AddComponent<Image>();
            glowImage.color = new Color(0, 1f, 1f, 0.3f);

            // Create text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(faceObj.transform, false);
            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "BUTTON";
            text.fontSize = 30;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            // Add Button3D component
            var button3D = buttonObj.AddComponent<DigitPark.Animations.Button3D>();

            // Use SerializedObject to set private fields
            SerializedObject so = new SerializedObject(button3D);
            so.FindProperty("buttonFace").objectReferenceValue = faceRT;
            so.FindProperty("buttonShadow").objectReferenceValue = shadowRT;
            so.FindProperty("highlightLine").objectReferenceValue = highlightImage;
            so.FindProperty("glowImage").objectReferenceValue = glowImage;
            so.ApplyModifiedProperties();

            // Save prefab
            string prefabPath = $"{PREFAB_PATH}/Button3D.prefab";
            PrefabUtility.SaveAsPrefabAsset(buttonObj, prefabPath);
            DestroyImmediate(buttonObj);

            Debug.Log($"<color=cyan>[DigitPark]</color> Button3D prefab created at {prefabPath}");
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        [MenuItem("DigitPark/Animation/Create Transition Canvas Prefab", false, 402)]
        public static void CreateTransitionCanvasPrefab()
        {
            EnsureDirectoriesExist();

            // Create canvas
            GameObject canvasObj = new GameObject("TransitionCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // Fade image
            GameObject fadeObj = new GameObject("FadeImage");
            fadeObj.transform.SetParent(canvasObj.transform, false);
            RectTransform fadeRT = fadeObj.AddComponent<RectTransform>();
            fadeRT.anchorMin = Vector2.zero;
            fadeRT.anchorMax = Vector2.one;
            fadeRT.sizeDelta = Vector2.zero;
            Image fadeImage = fadeObj.AddComponent<Image>();
            fadeImage.color = new Color(0, 0, 0, 0);
            fadeImage.raycastTarget = false;
            fadeObj.SetActive(false);

            // Circle wipe image
            GameObject circleObj = new GameObject("CircleWipeImage");
            circleObj.transform.SetParent(canvasObj.transform, false);
            RectTransform circleRT = circleObj.AddComponent<RectTransform>();
            circleRT.anchorMin = Vector2.zero;
            circleRT.anchorMax = Vector2.one;
            circleRT.sizeDelta = Vector2.zero;
            Image circleImage = circleObj.AddComponent<Image>();
            circleImage.color = Color.black;
            circleImage.raycastTarget = false;
            circleObj.SetActive(false);

            // Slide panel
            GameObject slideObj = new GameObject("SlidePanel");
            slideObj.transform.SetParent(canvasObj.transform, false);
            RectTransform slideRT = slideObj.AddComponent<RectTransform>();
            slideRT.anchorMin = Vector2.zero;
            slideRT.anchorMax = Vector2.one;
            slideRT.sizeDelta = Vector2.zero;
            Image slideImage = slideObj.AddComponent<Image>();
            slideImage.color = Color.black;
            slideImage.raycastTarget = false;
            slideObj.SetActive(false);

            // Add SceneTransitionManager
            var transitionManager = canvasObj.AddComponent<DigitPark.Animations.SceneTransitionManager>();

            SerializedObject so = new SerializedObject(transitionManager);
            so.FindProperty("transitionCanvas").objectReferenceValue = canvas;
            so.FindProperty("fadeImage").objectReferenceValue = fadeImage;
            so.FindProperty("circleWipeImage").objectReferenceValue = circleImage;
            so.FindProperty("slidePanel").objectReferenceValue = slideRT;
            so.ApplyModifiedProperties();

            // Save prefab
            string prefabPath = $"{PREFAB_PATH}/TransitionCanvas.prefab";
            PrefabUtility.SaveAsPrefabAsset(canvasObj, prefabPath);
            DestroyImmediate(canvasObj);

            Debug.Log($"<color=cyan>[DigitPark]</color> Transition Canvas prefab created at {prefabPath}");
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        [MenuItem("DigitPark/Animation/Create UI Animation Manager Prefab", false, 403)]
        public static void CreateUIAnimationManagerPrefab()
        {
            EnsureDirectoriesExist();

            GameObject managerObj = new GameObject("UIAnimationManager");

            // Create screen flash canvas
            GameObject canvasObj = new GameObject("EffectsCanvas");
            canvasObj.transform.SetParent(managerObj.transform);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9998;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);

            // Screen flash image
            GameObject flashObj = new GameObject("ScreenFlash");
            flashObj.transform.SetParent(canvasObj.transform, false);
            RectTransform flashRT = flashObj.AddComponent<RectTransform>();
            flashRT.anchorMin = Vector2.zero;
            flashRT.anchorMax = Vector2.one;
            flashRT.sizeDelta = Vector2.zero;
            Image flashImage = flashObj.AddComponent<Image>();
            flashImage.color = new Color(1, 1, 1, 0);
            flashImage.raycastTarget = false;
            flashObj.SetActive(false);

            // Add manager component
            var manager = managerObj.AddComponent<DigitPark.Animations.UIAnimationManager>();

            SerializedObject so = new SerializedObject(manager);
            so.FindProperty("screenFlashImage").objectReferenceValue = flashImage;
            so.ApplyModifiedProperties();

            // Save prefab
            string prefabPath = $"{PREFAB_PATH}/UIAnimationManager.prefab";
            PrefabUtility.SaveAsPrefabAsset(managerObj, prefabPath);
            DestroyImmediate(managerObj);

            Debug.Log($"<color=cyan>[DigitPark]</color> UI Animation Manager prefab created at {prefabPath}");
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        [MenuItem("DigitPark/Animation/Create All Prefabs", false, 410)]
        public static void CreateAllPrefabs()
        {
            CreateButton3DPrefab();
            CreateTransitionCanvasPrefab();
            CreateUIAnimationManagerPrefab();
            CreateParticleSpawnerPrefab();
            CreateRewardClaimAnimatorPrefab();

            Debug.Log("<color=cyan>[DigitPark]</color> All animation prefabs created!");
            EditorUtility.DisplayDialog("Animation System", "All animation prefabs have been created!", "OK");
        }

        private static void CreateParticleSpawnerPrefab()
        {
            EnsureDirectoriesExist();

            GameObject spawnerObj = new GameObject("ParticleEffectSpawner");
            spawnerObj.AddComponent<DigitPark.Animations.ParticleEffectSpawner>();

            // Create particles container
            GameObject particlesContainer = new GameObject("Particles");
            particlesContainer.transform.SetParent(spawnerObj.transform);

            string prefabPath = $"{PREFAB_PATH}/ParticleEffectSpawner.prefab";
            PrefabUtility.SaveAsPrefabAsset(spawnerObj, prefabPath);
            DestroyImmediate(spawnerObj);

            Debug.Log($"<color=cyan>[DigitPark]</color> Particle Effect Spawner prefab created at {prefabPath}");
        }

        private static void CreateRewardClaimAnimatorPrefab()
        {
            EnsureDirectoriesExist();

            GameObject rewardObj = new GameObject("RewardClaimAnimator");
            rewardObj.AddComponent<DigitPark.Animations.RewardClaimAnimator>();
            rewardObj.AddComponent<AudioSource>();

            // Create flying icons container
            GameObject iconsContainer = new GameObject("FlyingIcons");
            iconsContainer.transform.SetParent(rewardObj.transform);

            string prefabPath = $"{PREFAB_PATH}/RewardClaimAnimator.prefab";
            PrefabUtility.SaveAsPrefabAsset(rewardObj, prefabPath);
            DestroyImmediate(rewardObj);

            Debug.Log($"<color=cyan>[DigitPark]</color> Reward Claim Animator prefab created at {prefabPath}");
        }

        // ==================== MATERIALS ====================

        [MenuItem("DigitPark/Animation/Create UI Materials", false, 420)]
        public static void CreateMaterials()
        {
            EnsureDirectoriesExist();

            // Glow material
            CreateMaterial("UIGlow", "DigitPark/UIGlow", new Color(0, 1, 1, 1));

            // Shine material
            CreateMaterial("UIShine", "DigitPark/UIShine", Color.white);

            // Gradient material
            CreateMaterial("UIGradient", "DigitPark/UIGradient", Color.white);

            // Circle wipe material
            CreateMaterial("CircleWipe", "DigitPark/CircleWipe", Color.black);

            AssetDatabase.Refresh();
            Debug.Log("<color=cyan>[DigitPark]</color> UI Materials created!");
        }

        private static void CreateMaterial(string name, string shaderName, Color color)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogWarning($"Shader {shaderName} not found. Material {name} not created.");
                return;
            }

            Material mat = new Material(shader);
            mat.color = color;

            string matPath = $"{MATERIAL_PATH}/{name}.mat";
            AssetDatabase.CreateAsset(mat, matPath);
        }

        // ==================== ADD TO SCENE ====================

        [MenuItem("DigitPark/Animation/Add to Current Scene", false, 430)]
        public static void AddToCurrentScene()
        {
            // Check if already exists
            if (Object.FindFirstObjectByType<DigitPark.Animations.UIAnimationManager>() != null)
            {
                Debug.LogWarning("UIAnimationManager already exists in the scene.");
                return;
            }

            // Load and instantiate prefabs
            string managerPath = $"{PREFAB_PATH}/UIAnimationManager.prefab";
            string transitionPath = $"{PREFAB_PATH}/TransitionCanvas.prefab";
            string particlePath = $"{PREFAB_PATH}/ParticleEffectSpawner.prefab";

            GameObject managerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(managerPath);
            GameObject transitionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(transitionPath);
            GameObject particlePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(particlePath);

            if (managerPrefab != null)
                PrefabUtility.InstantiatePrefab(managerPrefab);
            else
                Debug.LogWarning("UIAnimationManager prefab not found. Run 'Create All Prefabs' first.");

            if (transitionPrefab != null)
                PrefabUtility.InstantiatePrefab(transitionPrefab);
            else
                Debug.LogWarning("TransitionCanvas prefab not found.");

            if (particlePrefab != null)
                PrefabUtility.InstantiatePrefab(particlePrefab);
            else
                Debug.LogWarning("ParticleEffectSpawner prefab not found.");

            Debug.Log("<color=cyan>[DigitPark]</color> Animation system added to scene!");
        }

        // ==================== COMPONENT SHORTCUTS ====================

        [MenuItem("GameObject/DigitPark/Add Button3D", false, 10)]
        public static void AddButton3DToSelected()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("Please select a GameObject first.");
                return;
            }

            if (selected.GetComponent<Button>() == null)
            {
                selected.AddComponent<Button>();
            }

            if (selected.GetComponent<DigitPark.Animations.Button3D>() == null)
            {
                selected.AddComponent<DigitPark.Animations.Button3D>();
                Debug.Log("<color=cyan>[DigitPark]</color> Button3D component added!");
            }
        }

        [MenuItem("GameObject/DigitPark/Add UI Effects", false, 11)]
        public static void AddUIEffectsToSelected()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("Please select a GameObject first.");
                return;
            }

            if (selected.GetComponent<DigitPark.Animations.UIEffects>() == null)
            {
                selected.AddComponent<DigitPark.Animations.UIEffects>();
                Debug.Log("<color=cyan>[DigitPark]</color> UIEffects component added!");
            }
        }

        [MenuItem("GameObject/DigitPark/Add Simple Pulse", false, 12)]
        public static void AddSimplePulseToSelected()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("Please select a GameObject first.");
                return;
            }

            if (selected.GetComponent<DigitPark.Animations.SimplePulse>() == null)
            {
                selected.AddComponent<DigitPark.Animations.SimplePulse>();
                Debug.Log("<color=cyan>[DigitPark]</color> SimplePulse component added!");
            }
        }

        [MenuItem("GameObject/DigitPark/Add Simple Float", false, 13)]
        public static void AddSimpleFloatToSelected()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("Please select a GameObject first.");
                return;
            }

            if (selected.GetComponent<DigitPark.Animations.SimpleFloat>() == null)
            {
                selected.AddComponent<DigitPark.Animations.SimpleFloat>();
                Debug.Log("<color=cyan>[DigitPark]</color> SimpleFloat component added!");
            }
        }

        [MenuItem("GameObject/DigitPark/Add Glow Pulse", false, 14)]
        public static void AddGlowPulseToSelected()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("Please select a GameObject first.");
                return;
            }

            if (selected.GetComponent<DigitPark.Animations.GlowPulse>() == null)
            {
                selected.AddComponent<DigitPark.Animations.GlowPulse>();
                Debug.Log("<color=cyan>[DigitPark]</color> GlowPulse component added!");
            }
        }

        // ==================== HELPERS ====================

        private static void CreateAnimationManagers()
        {
            CreateUIAnimationManagerPrefab();
            CreateTransitionCanvasPrefab();
            CreateParticleSpawnerPrefab();
        }

        private static void EnsureDirectoriesExist()
        {
            string[] directories = new string[]
            {
                "Assets/_Project/Prefabs",
                "Assets/_Project/Prefabs/Animation",
                "Assets/_Project/Materials",
                "Assets/_Project/Materials/UI",
                "Assets/_Project/Shaders"
            };

            foreach (string dir in directories)
            {
                if (!AssetDatabase.IsValidFolder(dir))
                {
                    string parent = Path.GetDirectoryName(dir).Replace("\\", "/");
                    string folderName = Path.GetFileName(dir);
                    AssetDatabase.CreateFolder(parent, folderName);
                }
            }
        }
    }
}
#endif
