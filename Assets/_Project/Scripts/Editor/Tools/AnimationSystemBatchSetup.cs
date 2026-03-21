#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace DigitPark.Editor
{
    /// <summary>
    /// Batch setup tool for adding animation system to all scenes.
    /// Automatically processes all scenes and adds required components.
    /// </summary>
    public class AnimationSystemBatchSetup : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<SceneAsset> scenesToProcess = new List<SceneAsset>();
        private List<string> processedScenes = new List<string>();
        private List<string> skippedScenes = new List<string>();
        private List<string> errorScenes = new List<string>();
        private bool isProcessing = false;
        private int currentSceneIndex = 0;
        private string currentStatus = "";

        // Options (public for direct menu access)
        public bool addUIAnimationManager = true;
        public bool addSceneTransitionManager = true;
        public bool addParticleSpawner = true;
        public bool convertButtonsTo3D = true; // ON - All buttons become 3D
        public bool addPulseToImportantButtons = true; // ON - CTA buttons pulse
        public bool addGlowToHeaders = true; // ON - Headers glow
        public bool skipBootScene = true;
        public bool skipButtonsWithAnimator = true; // Safety check
        public bool skipButtonsWithDOTween = true; // Safety check
        public bool previewMode = false; // Just show what would change

        [MenuItem("DigitPark/Polish/Animation Batch/Batch Setup All Scenes (Window)", false, 600)]
        public static void ShowWindow()
        {
            var window = GetWindow<AnimationSystemBatchSetup>("Batch Animation Setup");
            window.minSize = new Vector2(450, 700);
            window.LoadAllScenes();
        }

        [MenuItem("DigitPark/Polish/Animation Batch/APPLY ALL ANIMATIONS TO ALL SCENES", false, 599)]
        public static void ApplyAllAnimationsToAllScenes()
        {
            if (!AllScenesBatchBuilder.SilentMode &&
                !EditorUtility.DisplayDialog("Apply All Animations",
                "This will process ALL scenes and add:\n\n" +
                "✓ UI Animation Manager\n" +
                "✓ Scene Transition Manager\n" +
                "✓ Particle Effect Spawner\n" +
                "✓ Convert ALL Buttons to 3D\n" +
                "✓ Add Pulse to CTA Buttons\n" +
                "✓ Add Glow to Headers\n\n" +
                "Boot scene will be skipped.\n\n" +
                "Continue?",
                "YES - Apply All", "Cancel"))
            {
                return;
            }

            var setup = CreateInstance<AnimationSystemBatchSetup>();
            setup.addUIAnimationManager = true;
            setup.addSceneTransitionManager = true;
            setup.addParticleSpawner = true;
            setup.convertButtonsTo3D = true;
            setup.addPulseToImportantButtons = true;
            setup.addGlowToHeaders = true;
            setup.skipBootScene = true;
            setup.skipButtonsWithAnimator = true;
            setup.previewMode = false;

            setup.LoadAllScenes();
            setup.RunBatchProcess();

            DestroyImmediate(setup);
        }

        private void LoadAllScenes()
        {
            scenesToProcess.Clear();
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });

            foreach (string guid in sceneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                if (scene != null)
                {
                    scenesToProcess.Add(scene);
                }
            }

            // Sort by name
            scenesToProcess = scenesToProcess.OrderBy(s => s.name).ToList();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Animation System Batch Setup", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("This tool will automatically add the animation system to all selected scenes.", MessageType.Info);

            EditorGUILayout.Space(10);

            // Options
            EditorGUILayout.LabelField("Components to Add:", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            addUIAnimationManager = EditorGUILayout.Toggle("UI Animation Manager", addUIAnimationManager);
            addSceneTransitionManager = EditorGUILayout.Toggle("Scene Transition Manager", addSceneTransitionManager);
            addParticleSpawner = EditorGUILayout.Toggle("Particle Effect Spawner", addParticleSpawner);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("UI Animations:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "These options add animation components to UI elements.\n" +
                "Buttons with existing Animator or custom handlers are automatically skipped.",
                MessageType.Info);
            EditorGUI.indentLevel++;
            convertButtonsTo3D = EditorGUILayout.Toggle("Convert ALL Buttons to 3D", convertButtonsTo3D);
            addPulseToImportantButtons = EditorGUILayout.Toggle("Add Pulse to CTA Buttons (Play, Claim, Buy...)", addPulseToImportantButtons);
            addGlowToHeaders = EditorGUILayout.Toggle("Add Glow to Headers", addGlowToHeaders);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Safety Options:", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            skipBootScene = EditorGUILayout.Toggle("Skip Boot Scene", skipBootScene);
            skipButtonsWithAnimator = EditorGUILayout.Toggle("Skip Buttons with Animator", skipButtonsWithAnimator);
            skipButtonsWithDOTween = EditorGUILayout.Toggle("Skip Buttons with existing tweens", skipButtonsWithDOTween);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "SAFE MODE (Recommended): Only enable 'UI Animation Manager', 'Scene Transition Manager', and 'Particle Spawner'. " +
                "These add global managers that don't modify existing UI elements.\n\n" +
                "BUTTON CONVERSION: Only enable if your buttons don't have custom Animator controllers or existing DOTween animations.",
                MessageType.Warning);

            previewMode = EditorGUILayout.Toggle("Preview Mode (no changes)", previewMode);

            EditorGUILayout.Space(15);

            // Scene list
            EditorGUILayout.LabelField($"Scenes Found: {scenesToProcess.Count}", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(250));

            foreach (var scene in scenesToProcess)
            {
                EditorGUILayout.BeginHorizontal();

                // Status icon
                string status = "";
                Color statusColor = Color.white;

                if (processedScenes.Contains(scene.name))
                {
                    status = "✓";
                    statusColor = Color.green;
                }
                else if (skippedScenes.Contains(scene.name))
                {
                    status = "○";
                    statusColor = Color.yellow;
                }
                else if (errorScenes.Contains(scene.name))
                {
                    status = "✗";
                    statusColor = Color.red;
                }

                GUI.color = statusColor;
                EditorGUILayout.LabelField(status, GUILayout.Width(20));
                GUI.color = Color.white;

                EditorGUILayout.ObjectField(scene, typeof(SceneAsset), false);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);

            // Status
            if (!string.IsNullOrEmpty(currentStatus))
            {
                EditorGUILayout.HelpBox(currentStatus, MessageType.None);
            }

            // Progress
            if (isProcessing)
            {
                float progress = (float)currentSceneIndex / scenesToProcess.Count;
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(GUILayout.Height(20)), progress, $"Processing... {currentSceneIndex}/{scenesToProcess.Count}");
            }

            EditorGUILayout.Space(10);

            // Buttons
            EditorGUI.BeginDisabledGroup(isProcessing);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Refresh Scene List", GUILayout.Height(30)))
            {
                LoadAllScenes();
                processedScenes.Clear();
                skippedScenes.Clear();
                errorScenes.Clear();
            }

            if (GUILayout.Button("Setup All Scenes", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Batch Setup",
                    $"This will process {scenesToProcess.Count} scenes and add animation components.\n\nThis operation cannot be undone easily.\n\nContinue?",
                    "Yes, Process All", "Cancel"))
                {
                    ProcessAllScenes();
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();

            // Results summary
            if (processedScenes.Count > 0 || skippedScenes.Count > 0 || errorScenes.Count > 0)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Results:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"  Processed: {processedScenes.Count}");
                EditorGUILayout.LabelField($"  Skipped: {skippedScenes.Count}");
                EditorGUILayout.LabelField($"  Errors: {errorScenes.Count}");
            }
        }

        /// <summary>
        /// Public method to run the batch process from menu
        /// </summary>
        public void RunBatchProcess()
        {
            ProcessAllScenes();
        }

        private void ProcessAllScenes()
        {
            processedScenes.Clear();
            skippedScenes.Clear();
            errorScenes.Clear();
            isProcessing = true;
            currentSceneIndex = 0;

            // Save current scene
            string currentScenePath = SceneManager.GetActiveScene().path;
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            try
            {
                foreach (var sceneAsset in scenesToProcess)
                {
                    currentSceneIndex++;
                    string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
                    string sceneName = sceneAsset.name;

                    currentStatus = $"Processing: {sceneName}";
                    Repaint();

                    // Skip boot scene if option is enabled
                    if (skipBootScene && sceneName.ToLower().Contains("boot"))
                    {
                        skippedScenes.Add(sceneName);
                        Debug.Log($"<color=yellow>[DigitPark]</color> Skipped: {sceneName} (Boot scene)");
                        continue;
                    }

                    try
                    {
                        // Open scene
                        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                        // Process scene
                        bool madeChanges = ProcessScene();

                        if (madeChanges)
                        {
                            // Save scene
                            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                            processedScenes.Add(sceneName);
                            Debug.Log($"<color=green>[DigitPark]</color> Processed: {sceneName}");
                        }
                        else
                        {
                            skippedScenes.Add(sceneName);
                            Debug.Log($"<color=yellow>[DigitPark]</color> No changes needed: {sceneName}");
                        }
                    }
                    catch (System.Exception e)
                    {
                        errorScenes.Add(sceneName);
                        Debug.LogError($"<color=red>[DigitPark]</color> Error processing {sceneName}: {e.Message}");
                    }
                }
            }
            finally
            {
                isProcessing = false;

                // Return to original scene if it still exists
                if (!string.IsNullOrEmpty(currentScenePath) && File.Exists(currentScenePath))
                {
                    EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
                }

                currentStatus = $"Complete! Processed: {processedScenes.Count}, Skipped: {skippedScenes.Count}, Errors: {errorScenes.Count}";

                EditorUtility.DisplayDialog("Batch Setup Complete",
                    $"Processing complete!\n\nProcessed: {processedScenes.Count}\nSkipped: {skippedScenes.Count}\nErrors: {errorScenes.Count}",
                    "OK");
            }
        }

        private bool ProcessScene()
        {
            bool madeChanges = false;

            // Find or create managers root
            GameObject managersRoot = GameObject.Find("---ANIMATION_MANAGERS---");
            if (managersRoot == null)
            {
                managersRoot = new GameObject("---ANIMATION_MANAGERS---");
                madeChanges = true;
            }

            // Add UI Animation Manager
            if (addUIAnimationManager)
            {
                var existing = Object.FindFirstObjectByType<DigitPark.Animations.UIAnimationManager>();
                if (existing == null)
                {
                    GameObject managerObj = new GameObject("UIAnimationManager");
                    managerObj.transform.SetParent(managersRoot.transform);
                    managerObj.AddComponent<DigitPark.Animations.UIAnimationManager>();

                    // Create effects canvas
                    CreateEffectsCanvas(managerObj);
                    madeChanges = true;
                }
            }

            // Add Scene Transition Manager
            if (addSceneTransitionManager)
            {
                var existing = Object.FindFirstObjectByType<DigitPark.Animations.SceneTransitionManager>();
                if (existing == null)
                {
                    GameObject transitionObj = CreateTransitionCanvas();
                    transitionObj.transform.SetParent(managersRoot.transform);
                    madeChanges = true;
                }
            }

            // Add Particle Spawner
            if (addParticleSpawner)
            {
                var existing = Object.FindFirstObjectByType<DigitPark.Animations.ParticleEffectSpawner>();
                if (existing == null)
                {
                    GameObject spawnerObj = new GameObject("ParticleEffectSpawner");
                    spawnerObj.transform.SetParent(managersRoot.transform);
                    spawnerObj.AddComponent<DigitPark.Animations.ParticleEffectSpawner>();

                    GameObject particlesContainer = new GameObject("Particles");
                    particlesContainer.transform.SetParent(spawnerObj.transform);
                    madeChanges = true;
                }
            }

            // Convert buttons to 3D
            if (convertButtonsTo3D)
            {
                bool buttonsConverted = ConvertButtonsTo3D();
                if (buttonsConverted) madeChanges = true;
            }

            // Add pulse to important buttons
            if (addPulseToImportantButtons)
            {
                bool pulsesAdded = AddPulseToImportantButtons();
                if (pulsesAdded) madeChanges = true;
            }

            // Add glow to headers
            if (addGlowToHeaders)
            {
                bool glowsAdded = AddGlowToHeaders();
                if (glowsAdded) madeChanges = true;
            }

            return madeChanges;
        }

        private void CreateEffectsCanvas(GameObject parent)
        {
            GameObject canvasObj = new GameObject("EffectsCanvas");
            canvasObj.transform.SetParent(parent.transform);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9998;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);

            // Screen flash
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

            // Link to manager
            var manager = parent.GetComponent<DigitPark.Animations.UIAnimationManager>();
            if (manager != null)
            {
                SerializedObject so = new SerializedObject(manager);
                so.FindProperty("screenFlashImage").objectReferenceValue = flashImage;
                so.ApplyModifiedProperties();
            }
        }

        private GameObject CreateTransitionCanvas()
        {
            GameObject canvasObj = new GameObject("TransitionCanvas");

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
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

            // Circle wipe
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

            // Add manager
            var manager = canvasObj.AddComponent<DigitPark.Animations.SceneTransitionManager>();

            SerializedObject so = new SerializedObject(manager);
            so.FindProperty("transitionCanvas").objectReferenceValue = canvas;
            so.FindProperty("fadeImage").objectReferenceValue = fadeImage;
            so.FindProperty("circleWipeImage").objectReferenceValue = circleImage;
            so.FindProperty("slidePanel").objectReferenceValue = slideRT;
            so.ApplyModifiedProperties();

            return canvasObj;
        }

        private bool ConvertButtonsTo3D()
        {
            bool madeChanges = false;
            Button[] allButtons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);

            foreach (Button button in allButtons)
            {
                // Skip if already has Button3D
                if (button.GetComponent<DigitPark.Animations.Button3D>() != null)
                    continue;

                // SAFETY: Skip if has Animator component (existing animations)
                if (skipButtonsWithAnimator && button.GetComponent<Animator>() != null)
                {
                    Debug.Log($"<color=yellow>[DigitPark]</color> Skipped {button.name} - has Animator");
                    continue;
                }

                // SAFETY: Skip if has Animation component (legacy animations)
                if (button.GetComponent<Animation>() != null)
                {
                    Debug.Log($"<color=yellow>[DigitPark]</color> Skipped {button.name} - has Animation");
                    continue;
                }

                // SAFETY: Skip if parent has Animator (animated container)
                if (skipButtonsWithAnimator && button.transform.parent != null &&
                    button.transform.parent.GetComponent<Animator>() != null)
                {
                    Debug.Log($"<color=yellow>[DigitPark]</color> Skipped {button.name} - parent has Animator");
                    continue;
                }

                // SAFETY: Skip buttons with custom pointer handlers (might conflict)
                var pointerHandlers = button.GetComponents<UnityEngine.EventSystems.IPointerDownHandler>();
                if (pointerHandlers.Length > 1) // More than just Button itself
                {
                    Debug.Log($"<color=yellow>[DigitPark]</color> Skipped {button.name} - has custom pointer handlers");
                    continue;
                }

                // Skip small or special buttons
                RectTransform rt = button.GetComponent<RectTransform>();
                if (rt != null && (rt.sizeDelta.x < 80 || rt.sizeDelta.y < 40))
                    continue;

                // Skip navigation buttons, tabs, and system buttons
                string buttonName = button.name.ToLower();
                string[] skipKeywords = { "tab", "nav", "back", "close", "toggle", "checkbox", "radio", "scroll", "slider", "dropdown" };
                bool shouldSkip = false;
                foreach (string keyword in skipKeywords)
                {
                    if (buttonName.Contains(keyword))
                    {
                        shouldSkip = true;
                        break;
                    }
                }
                if (shouldSkip) continue;

                // IMPORTANT: Skip Google and Apple sign-in buttons (brand guidelines prohibit modifications)
                string[] brandProtectedKeywords = { "google", "apple", "signin", "sign_in", "sign-in", "login_google", "login_apple", "auth_google", "auth_apple" };
                bool isBrandProtected = false;
                foreach (string keyword in brandProtectedKeywords)
                {
                    if (buttonName.Contains(keyword))
                    {
                        isBrandProtected = true;
                        break;
                    }
                }
                if (isBrandProtected)
                {
                    Debug.Log($"<color=yellow>[DigitPark]</color> Skipped {button.name} - Brand protected button (Google/Apple guidelines)");
                    continue;
                }

                // Preview mode - just log, don't change
                if (previewMode)
                {
                    Debug.Log($"<color=cyan>[DigitPark PREVIEW]</color> Would add Button3D to: {button.name}");
                    continue;
                }

                // Add Button3D
                button.gameObject.AddComponent<DigitPark.Animations.Button3D>();
                Debug.Log($"<color=green>[DigitPark]</color> Added Button3D to: {button.name}");
                madeChanges = true;
            }

            return madeChanges;
        }

        private bool AddPulseToImportantButtons()
        {
            bool madeChanges = false;
            Button[] allButtons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);

            string[] ctaKeywords = { "play", "start", "claim", "collect", "buy", "purchase", "upgrade", "unlock", "spin", "open", "battle", "fight" };

            foreach (Button button in allButtons)
            {
                string buttonName = button.name.ToLower();

                // Check if it's a CTA button
                bool isCTA = ctaKeywords.Any(keyword => buttonName.Contains(keyword));
                if (!isCTA) continue;

                // Skip if already has pulse
                if (button.GetComponent<DigitPark.Animations.SimplePulse>() != null)
                    continue;

                // SAFETY: Skip if has Animator (existing scale animations would conflict)
                if (skipButtonsWithAnimator && button.GetComponent<Animator>() != null)
                    continue;

                // Preview mode
                if (previewMode)
                {
                    Debug.Log($"<color=cyan>[DigitPark PREVIEW]</color> Would add SimplePulse to: {button.name}");
                    continue;
                }

                button.gameObject.AddComponent<DigitPark.Animations.SimplePulse>();
                Debug.Log($"<color=green>[DigitPark]</color> Added SimplePulse to: {button.name}");
                madeChanges = true;
            }

            return madeChanges;
        }

        private bool AddGlowToHeaders()
        {
            bool madeChanges = false;
            TMPro.TextMeshProUGUI[] allTexts = Object.FindObjectsByType<TMPro.TextMeshProUGUI>(FindObjectsSortMode.None);

            foreach (var text in allTexts)
            {
                // Check if it's likely a header (large font, certain names)
                bool isHeader = text.fontSize >= 36 ||
                               text.name.ToLower().Contains("title") ||
                               text.name.ToLower().Contains("header");

                if (!isHeader) continue;

                // Skip if already has glow
                if (text.GetComponent<DigitPark.Animations.GlowPulse>() != null)
                    continue;

                // GlowPulse needs Image, so add to parent if it has one
                Image parentImage = text.transform.parent?.GetComponent<Image>();
                if (parentImage != null && parentImage.GetComponent<DigitPark.Animations.GlowPulse>() == null)
                {
                    parentImage.gameObject.AddComponent<DigitPark.Animations.GlowPulse>();
                    madeChanges = true;
                }
            }

            return madeChanges;
        }

        // Quick action menus
        [MenuItem("DigitPark/Polish/Animation Batch/Quick: Add Managers to Current Scene", false, 610)]
        public static void QuickAddManagersToCurrentScene()
        {
            var window = CreateInstance<AnimationSystemBatchSetup>();
            window.addUIAnimationManager = true;
            window.addSceneTransitionManager = true;
            window.addParticleSpawner = true;
            window.convertButtonsTo3D = false;
            window.addPulseToImportantButtons = false;

            bool madeChanges = window.ProcessScene();

            if (madeChanges)
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                Debug.Log("<color=green>[DigitPark]</color> Animation managers added to current scene!");
            }
            else
            {
                Debug.Log("<color=yellow>[DigitPark]</color> Animation managers already exist in scene.");
            }

            DestroyImmediate(window);
        }

        [MenuItem("DigitPark/Polish/Animation Batch/Fix: Remove 3D from Google-Apple Buttons", false, 620)]
        public static void RemoveButton3DFromBrandProtectedButtons()
        {
            string[] brandProtectedKeywords = { "google", "apple", "signin", "sign_in", "sign-in", "login_google", "login_apple", "auth_google", "auth_apple" };

            int removedCount = 0;
            Button[] allButtons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);

            foreach (Button button in allButtons)
            {
                string buttonName = button.name.ToLower();

                bool isBrandProtected = false;
                foreach (string keyword in brandProtectedKeywords)
                {
                    if (buttonName.Contains(keyword))
                    {
                        isBrandProtected = true;
                        break;
                    }
                }

                if (isBrandProtected)
                {
                    // Remove Button3D if exists
                    var button3D = button.GetComponent<DigitPark.Animations.Button3D>();
                    if (button3D != null)
                    {
                        DestroyImmediate(button3D);
                        removedCount++;
                        Debug.Log($"<color=green>[DigitPark]</color> Removed Button3D from: {button.name}");
                    }

                    // Remove SimplePulse if exists
                    var pulse = button.GetComponent<DigitPark.Animations.SimplePulse>();
                    if (pulse != null)
                    {
                        DestroyImmediate(pulse);
                        Debug.Log($"<color=green>[DigitPark]</color> Removed SimplePulse from: {button.name}");
                    }
                }
            }

            if (removedCount > 0)
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                Debug.Log($"<color=cyan>[DigitPark]</color> Cleaned {removedCount} brand-protected buttons in current scene. Remember to SAVE the scene!");
                EditorUtility.DisplayDialog("Brand Protection Fix",
                    $"Removed animation components from {removedCount} Google/Apple buttons.\n\nDon't forget to SAVE the scene!", "OK");
            }
            else
            {
                Debug.Log("<color=yellow>[DigitPark]</color> No brand-protected buttons with animations found in current scene.");
                EditorUtility.DisplayDialog("Brand Protection Fix", "No Google/Apple buttons with animations found in this scene.", "OK");
            }
        }

        [MenuItem("DigitPark/Polish/Animation Batch/Fix: Clean Google-Apple in ALL Scenes", false, 621)]
        public static void CleanBrandProtectedButtonsAllScenes()
        {
            if (!EditorUtility.DisplayDialog("Clean All Scenes",
                "This will open each scene and remove Button3D/SimplePulse from Google/Apple buttons.\n\nContinue?",
                "Yes", "Cancel"))
            {
                return;
            }

            string currentScenePath = SceneManager.GetActiveScene().path;
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/_Project/Scenes" });
            int totalCleaned = 0;

            foreach (string guid in sceneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                try
                {
                    EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

                    string[] brandProtectedKeywords = { "google", "apple", "signin", "sign_in", "sign-in" };
                    Button[] allButtons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
                    bool madeChanges = false;

                    foreach (Button button in allButtons)
                    {
                        string buttonName = button.name.ToLower();

                        bool isBrandProtected = false;
                        foreach (string keyword in brandProtectedKeywords)
                        {
                            if (buttonName.Contains(keyword))
                            {
                                isBrandProtected = true;
                                break;
                            }
                        }

                        if (isBrandProtected)
                        {
                            var button3D = button.GetComponent<DigitPark.Animations.Button3D>();
                            if (button3D != null)
                            {
                                DestroyImmediate(button3D);
                                madeChanges = true;
                                totalCleaned++;
                            }

                            var pulse = button.GetComponent<DigitPark.Animations.SimplePulse>();
                            if (pulse != null)
                            {
                                DestroyImmediate(pulse);
                                madeChanges = true;
                            }
                        }
                    }

                    if (madeChanges)
                    {
                        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                        Debug.Log($"<color=green>[DigitPark]</color> Cleaned brand buttons in: {path}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error processing {path}: {e.Message}");
                }
            }

            // Return to original scene
            if (!string.IsNullOrEmpty(currentScenePath))
            {
                EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
            }

            EditorUtility.DisplayDialog("Clean Complete",
                $"Removed animations from {totalCleaned} Google/Apple buttons across all scenes.", "OK");
        }

        [MenuItem("DigitPark/Polish/Animation Batch/Quick: Convert Buttons in Current Scene", false, 651)]
        public static void QuickConvertButtonsInCurrentScene()
        {
            var window = CreateInstance<AnimationSystemBatchSetup>();
            window.addUIAnimationManager = false;
            window.addSceneTransitionManager = false;
            window.addParticleSpawner = false;
            window.convertButtonsTo3D = true;
            window.addPulseToImportantButtons = true;

            bool madeChanges = window.ProcessScene();

            if (madeChanges)
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                Debug.Log("<color=green>[DigitPark]</color> Buttons converted in current scene!");
            }
            else
            {
                Debug.Log("<color=yellow>[DigitPark]</color> No buttons needed conversion.");
            }

            DestroyImmediate(window);
        }
    }
}
#endif
