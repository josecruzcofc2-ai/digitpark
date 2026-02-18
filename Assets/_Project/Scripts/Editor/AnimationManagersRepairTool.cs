#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

namespace DigitPark.Editor
{
    /// <summary>
    /// Fase 1 repair tool: Fix broken/missing ANIMATION_MANAGERS across all scenes.
    /// Uses a single unified ProcessScene method for both diagnosis and repair.
    /// Handles 3 cases:
    ///   1A) Create full ANIMATION_MANAGERS where missing entirely
    ///   1B) Fix empty/broken TransitionCanvas (add missing children + wire references)
    ///   1C) Move misplaced popups out of TransitionCanvas (TournamentLobby)
    /// </summary>
    public class AnimationManagersRepairTool
    {
        private enum SceneStatus { OK, NeedsCreation, NeedsRepair, Error }

        private struct ProcessResult
        {
            public bool hasIssues;
            public bool wasCreated;      // true = entire structure was missing
            public List<string> actions;  // what was found/fixed

            public string Description => actions.Count > 0 ? string.Join(", ", actions) : "No issues";
        }

        // ==================== MENU ITEMS ====================

        [MenuItem("DigitPark/Animation/Batch/FASE 1: Repair All ANIMATION_MANAGERS", false, 590)]
        public static void RepairAllScenes()
        {
            if (!EditorUtility.DisplayDialog("Fase 1: Repair ANIMATION_MANAGERS",
                "This will process ALL scenes and:\n\n" +
                "1A) Create ANIMATION_MANAGERS where missing\n" +
                "1B) Fix empty/broken TransitionCanvas\n" +
                "1C) Move misplaced popups in TournamentLobby\n\n" +
                "Continue?",
                "YES - Repair All", "Cancel"))
            {
                return;
            }

            RunOnAllScenes(dryRun: false);
        }

        [MenuItem("DigitPark/Animation/Batch/FASE 1: Repair Current Scene Only", false, 591)]
        public static void RepairCurrentScene()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            ProcessResult result = ProcessScene(sceneName, dryRun: false);

            if (result.hasIssues)
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                Debug.Log($"<color=green>[Fase1]</color> {sceneName}: {result.Description}");
                EditorUtility.DisplayDialog("Repair Complete",
                    $"{sceneName}:\n{result.Description}\n\nRemember to save the scene!", "OK");
            }
            else
            {
                Debug.Log($"<color=green>[Fase1]</color> {sceneName}: No changes needed");
                EditorUtility.DisplayDialog("No Changes", $"{sceneName}: ANIMATION_MANAGERS already correct.", "OK");
            }
        }

        [MenuItem("DigitPark/Animation/Batch/FASE 1: Diagnose All Scenes (Preview)", false, 592)]
        public static void DiagnoseAllScenes()
        {
            RunOnAllScenes(dryRun: true);
        }

        // ==================== CORE BATCH PROCESSOR ====================

        private static void RunOnAllScenes(bool dryRun)
        {
            string currentScenePath = SceneManager.GetActiveScene().path;
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/_Project/Scenes" });

            int okCount = 0, createCount = 0, repairCount = 0, errorCount = 0;
            int totalScenes = 0;

            List<string> createScenes = new List<string>();
            List<string> repairScenes = new List<string>();
            List<string> okScenes = new List<string>();
            List<string> errorScenes = new List<string>();

            // Detailed report lines
            List<string> createDetails = new List<string>();
            List<string> repairDetails = new List<string>();

            foreach (string guid in sceneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string sceneName = Path.GetFileNameWithoutExtension(path);
                totalScenes++;

                if (sceneName == "Boot")
                {
                    okCount++;
                    okScenes.Add(sceneName);
                    continue;
                }

                try
                {
                    EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

                    ProcessResult result = ProcessScene(sceneName, dryRun);

                    if (!result.hasIssues)
                    {
                        okCount++;
                        okScenes.Add(sceneName);
                    }
                    else
                    {
                        if (!dryRun)
                        {
                            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                        }

                        if (result.wasCreated)
                        {
                            createCount++;
                            createScenes.Add(sceneName);
                            createDetails.Add($"  <color=red>[CREATE]</color> {sceneName}\n           -> {result.Description}");
                        }
                        else
                        {
                            repairCount++;
                            repairScenes.Add(sceneName);
                            repairDetails.Add($"  <color=yellow>[REPAIR]</color> {sceneName}\n           -> {result.Description}");
                        }

                        string actionWord = dryRun ? "Found" : "Fixed";
                        Debug.Log($"<color=green>[Fase1]</color> {actionWord} {sceneName}: {result.Description}");
                    }
                }
                catch (System.Exception e)
                {
                    errorCount++;
                    errorScenes.Add(sceneName);
                    Debug.LogError($"<color=red>[Fase1]</color> Error in {sceneName}: {e.Message}");
                }
            }

            // Return to original scene
            if (!string.IsNullOrEmpty(currentScenePath) && File.Exists(currentScenePath))
            {
                EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
            }

            // Build console report
            string mode = dryRun ? "DIAGNOSTIC (Preview)" : "REPAIR (Applied)";
            string separator = new string('-', 70);

            string report = $"\n<color=cyan>================================================================</color>\n";
            report += $"<color=cyan>   FASE 1 {mode} - ANIMATION_MANAGERS</color>\n";
            report += $"<color=cyan>================================================================</color>\n\n";

            report += $"  Total Scenes: {totalScenes}\n";
            report += $"  <color=green>OK: {okCount}</color>  |  ";
            report += $"<color=red>Need Creation: {createCount}</color>  |  ";
            report += $"<color=yellow>Need Repair: {repairCount}</color>  |  ";
            report += $"Errors: {errorCount}\n\n";

            if (createCount > 0)
            {
                report += $"<color=red>{separator}</color>\n";
                report += $"<color=red>  {(dryRun ? "NEED" : "GOT")} FULL CREATION ({createCount} scenes)</color>\n";
                report += $"<color=red>  {(dryRun ? "These have" : "These had")} NO ---ANIMATION_MANAGERS---</color>\n";
                report += $"<color=red>{separator}</color>\n";
                report += string.Join("\n", createDetails) + "\n\n";
            }

            if (repairCount > 0)
            {
                report += $"<color=yellow>{separator}</color>\n";
                report += $"<color=yellow>  {(dryRun ? "NEED" : "GOT")} REPAIR ({repairCount} scenes)</color>\n";
                report += $"<color=yellow>  {(dryRun ? "These have" : "These had")} partial/broken ANIMATION_MANAGERS</color>\n";
                report += $"<color=yellow>{separator}</color>\n";
                report += string.Join("\n", repairDetails) + "\n\n";
            }

            report += $"<color=green>{separator}</color>\n";
            report += $"<color=green>  OK ({okCount} scenes)</color>\n";
            report += $"<color=green>{separator}</color>\n";
            foreach (string s in okScenes)
                report += $"  <color=green>[  OK  ]</color> {s}\n";
            report += "\n";

            report += "<color=cyan>================================================================</color>\n";
            if (dryRun && (createCount + repairCount > 0))
            {
                report += $"<color=cyan>  ACTION: Run 'FASE 1: Repair All ANIMATION_MANAGERS'</color>\n";
                report += $"<color=cyan>  to fix {createCount + repairCount} scenes automatically.</color>\n";
            }
            else if (!dryRun)
            {
                report += $"<color=green>  All {createCount + repairCount} scenes have been repaired and saved.</color>\n";
            }
            else
            {
                report += "<color=green>  ALL SCENES ARE CORRECT - No repairs needed!</color>\n";
            }
            report += "<color=cyan>================================================================</color>\n";

            Debug.Log(report);

            // Dialog with scene names
            string dialogTitle = dryRun ? "Fase 1 Diagnostic" : "Fase 1 Repair Complete";
            string dialogMsg = $"Scanned {totalScenes} scenes:\n\n";

            if (createCount > 0)
            {
                dialogMsg += $"{(dryRun ? "NEED" : "GOT")} CREATION ({createCount}):\n";
                foreach (string s in createScenes) dialogMsg += $"  - {s}\n";
                dialogMsg += "\n";
            }

            if (repairCount > 0)
            {
                dialogMsg += $"{(dryRun ? "NEED" : "GOT")} REPAIR ({repairCount}):\n";
                foreach (string s in repairScenes) dialogMsg += $"  - {s}\n";
                dialogMsg += "\n";
            }

            dialogMsg += $"OK: {okCount}  |  Errors: {errorCount}\n\n";

            if (dryRun && (createCount + repairCount > 0))
                dialogMsg += "Run 'Repair All ANIMATION_MANAGERS' to fix.\nSee Console for full details.";
            else if (!dryRun && (createCount + repairCount > 0))
                dialogMsg += "All scenes repaired and saved.\nSee Console for full details.";
            else
                dialogMsg += "All scenes are correct!";

            EditorUtility.DisplayDialog(dialogTitle, dialogMsg, "OK");
        }

        // ==================== UNIFIED PROCESS (diagnose + repair in one) ====================

        /// <summary>
        /// Single method used by BOTH diagnose and repair.
        /// dryRun=true: only detect issues, don't modify anything.
        /// dryRun=false: detect AND fix issues.
        /// </summary>
        private static ProcessResult ProcessScene(string sceneName, bool dryRun)
        {
            ProcessResult result = new ProcessResult { actions = new List<string>() };

            // === STEP 1: ---ANIMATION_MANAGERS--- root ===
            GameObject managersRoot = GameObject.Find("---ANIMATION_MANAGERS---");
            if (managersRoot == null)
            {
                if (dryRun)
                {
                    result.hasIssues = true;
                    result.wasCreated = true;
                    result.actions.Add("Missing ---ANIMATION_MANAGERS--- (entire structure needed)");
                    return result; // No point checking children
                }
                else
                {
                    managersRoot = new GameObject("---ANIMATION_MANAGERS---");
                    result.hasIssues = true;
                    result.wasCreated = true;
                    result.actions.Add("Created ---ANIMATION_MANAGERS---");
                }
            }

            // === STEP 2: UIAnimationManager + EffectsCanvas ===
            var uiAnimManager = Object.FindFirstObjectByType<DigitPark.Animations.UIAnimationManager>();
            if (uiAnimManager == null)
            {
                if (dryRun)
                {
                    result.hasIssues = true;
                    result.actions.Add("Missing UIAnimationManager + EffectsCanvas");
                }
                else
                {
                    GameObject managerObj = new GameObject("UIAnimationManager");
                    managerObj.transform.SetParent(managersRoot.transform);
                    uiAnimManager = managerObj.AddComponent<DigitPark.Animations.UIAnimationManager>();
                    CreateEffectsCanvas(managerObj, uiAnimManager);
                    result.hasIssues = true;
                    result.actions.Add("Created UIAnimationManager + EffectsCanvas");
                }
            }
            else
            {
                // Check parent
                if (uiAnimManager.transform.parent != managersRoot.transform)
                {
                    if (dryRun)
                    {
                        result.hasIssues = true;
                        result.actions.Add("UIAnimationManager not parented under ---ANIMATION_MANAGERS---");
                    }
                    else
                    {
                        uiAnimManager.transform.SetParent(managersRoot.transform);
                        result.hasIssues = true;
                        result.actions.Add("Reparented UIAnimationManager");
                    }
                }

                // Check EffectsCanvas + ScreenFlash + screenFlashImage ref
                List<string> effectsIssues = CheckOrRepairEffectsCanvas(uiAnimManager, dryRun);
                if (effectsIssues.Count > 0)
                {
                    result.hasIssues = true;
                    result.actions.AddRange(effectsIssues);
                }
            }

            // === STEP 3: TransitionCanvas (SceneTransitionManager) ===
            var transitionManager = Object.FindFirstObjectByType<DigitPark.Animations.SceneTransitionManager>();
            if (transitionManager == null)
            {
                if (dryRun)
                {
                    result.hasIssues = true;
                    result.actions.Add("Missing TransitionCanvas (no SceneTransitionManager)");
                }
                else
                {
                    GameObject transitionObj = CreateTransitionCanvas();
                    transitionObj.transform.SetParent(managersRoot.transform);
                    result.hasIssues = true;
                    result.actions.Add("Created TransitionCanvas (full)");
                }
            }
            else
            {
                // Check parent
                if (transitionManager.transform.parent != managersRoot.transform)
                {
                    if (dryRun)
                    {
                        result.hasIssues = true;
                        result.actions.Add("TransitionCanvas not parented under ---ANIMATION_MANAGERS---");
                    }
                    else
                    {
                        transitionManager.transform.SetParent(managersRoot.transform);
                        result.hasIssues = true;
                        result.actions.Add("Reparented TransitionCanvas");
                    }
                }

                // Check children + serialized refs
                List<string> transitionIssues = CheckOrRepairTransitionCanvas(transitionManager, dryRun);
                if (transitionIssues.Count > 0)
                {
                    result.hasIssues = true;
                    result.actions.AddRange(transitionIssues);
                }

                // Check misplaced children (any scene, not just TournamentLobby)
                List<string> misplacedIssues = CheckOrMoveMisplacedPopups(transitionManager.transform, sceneName, dryRun);
                if (misplacedIssues.Count > 0)
                {
                    result.hasIssues = true;
                    result.actions.AddRange(misplacedIssues);
                }
            }

            // === STEP 4: ParticleEffectSpawner ===
            var particleSpawner = Object.FindFirstObjectByType<DigitPark.Animations.ParticleEffectSpawner>();
            if (particleSpawner == null)
            {
                if (dryRun)
                {
                    result.hasIssues = true;
                    result.actions.Add("Missing ParticleEffectSpawner");
                }
                else
                {
                    GameObject spawnerObj = new GameObject("ParticleEffectSpawner");
                    spawnerObj.transform.SetParent(managersRoot.transform);
                    spawnerObj.AddComponent<DigitPark.Animations.ParticleEffectSpawner>();

                    GameObject particlesContainer = new GameObject("Particles");
                    particlesContainer.transform.SetParent(spawnerObj.transform);

                    result.hasIssues = true;
                    result.actions.Add("Created ParticleEffectSpawner");
                }
            }
            else
            {
                // Check parent
                if (particleSpawner.transform.parent != managersRoot.transform)
                {
                    if (dryRun)
                    {
                        result.hasIssues = true;
                        result.actions.Add("ParticleEffectSpawner not parented under ---ANIMATION_MANAGERS---");
                    }
                    else
                    {
                        particleSpawner.transform.SetParent(managersRoot.transform);
                        result.hasIssues = true;
                        result.actions.Add("Reparented ParticleEffectSpawner");
                    }
                }
            }

            return result;
        }

        // ==================== CHECK/REPAIR HELPERS (unified) ====================

        /// <summary>
        /// Check or repair EffectsCanvas hierarchy and screenFlashImage reference.
        /// Returns list of issues found (empty = all good).
        /// </summary>
        private static List<string> CheckOrRepairEffectsCanvas(DigitPark.Animations.UIAnimationManager manager, bool dryRun)
        {
            List<string> issues = new List<string>();

            Transform effectsCanvas = manager.transform.Find("EffectsCanvas");

            if (effectsCanvas == null)
            {
                if (dryRun)
                {
                    issues.Add("Missing EffectsCanvas under UIAnimationManager");
                }
                else
                {
                    CreateEffectsCanvas(manager.gameObject, manager);
                    issues.Add("Created EffectsCanvas + ScreenFlash");
                }
                return issues;
            }

            // EffectsCanvas exists - check ScreenFlash
            Transform screenFlash = effectsCanvas.Find("ScreenFlash");
            if (screenFlash == null)
            {
                if (dryRun)
                {
                    issues.Add("Missing ScreenFlash under EffectsCanvas");
                }
                else
                {
                    GameObject flashObj = new GameObject("ScreenFlash");
                    flashObj.transform.SetParent(effectsCanvas, false);
                    RectTransform flashRT = flashObj.AddComponent<RectTransform>();
                    flashRT.anchorMin = Vector2.zero;
                    flashRT.anchorMax = Vector2.one;
                    flashRT.sizeDelta = Vector2.zero;
                    Image flashImage = flashObj.AddComponent<Image>();
                    flashImage.color = new Color(1, 1, 1, 0);
                    flashImage.raycastTarget = false;
                    flashObj.SetActive(false);

                    SerializedObject so = new SerializedObject(manager);
                    so.FindProperty("screenFlashImage").objectReferenceValue = flashImage;
                    so.ApplyModifiedProperties();
                    issues.Add("Created ScreenFlash + wired reference");
                }
                return issues;
            }

            // ScreenFlash exists - check serialized reference
            SerializedObject managerSO = new SerializedObject(manager);
            if (managerSO.FindProperty("screenFlashImage").objectReferenceValue == null)
            {
                if (dryRun)
                {
                    issues.Add("screenFlashImage reference is null (broken wire)");
                }
                else
                {
                    Image flashImage = screenFlash.GetComponent<Image>();
                    if (flashImage != null)
                    {
                        managerSO.FindProperty("screenFlashImage").objectReferenceValue = flashImage;
                        managerSO.ApplyModifiedProperties();
                        issues.Add("Re-wired screenFlashImage reference");
                    }
                    else
                    {
                        issues.Add("ScreenFlash exists but has no Image component (manual fix needed)");
                    }
                }
            }

            return issues;
        }

        /// <summary>
        /// Check or repair TransitionCanvas children and serialized references.
        /// Returns list of issues found (empty = all good).
        /// </summary>
        private static List<string> CheckOrRepairTransitionCanvas(DigitPark.Animations.SceneTransitionManager manager, bool dryRun)
        {
            List<string> issues = new List<string>();
            Transform canvasTransform = manager.transform;

            SerializedObject so = new SerializedObject(manager);

            // Check transitionCanvas self-reference
            var canvasProp = so.FindProperty("transitionCanvas");
            if (canvasProp.objectReferenceValue == null)
            {
                if (dryRun)
                {
                    issues.Add("TransitionCanvas: canvas self-reference is null");
                }
                else
                {
                    Canvas canvas = manager.GetComponent<Canvas>();
                    if (canvas != null)
                    {
                        canvasProp.objectReferenceValue = canvas;
                        issues.Add("Re-wired transitionCanvas self-reference");
                    }
                }
            }

            // Check each required child + reference
            issues.AddRange(CheckOrRepairTransitionChild(so, canvasTransform, "fadeImage", "FadeImage", new Color(0, 0, 0, 0), false, dryRun));
            issues.AddRange(CheckOrRepairTransitionChild(so, canvasTransform, "circleWipeImage", "CircleWipeImage", Color.black, false, dryRun));
            issues.AddRange(CheckOrRepairTransitionChild(so, canvasTransform, "slidePanel", "SlidePanel", Color.black, true, dryRun));

            if (!dryRun && issues.Count > 0)
            {
                so.ApplyModifiedProperties();
            }

            return issues;
        }

        /// <summary>
        /// Check or repair a single TransitionCanvas child (FadeImage, CircleWipeImage, or SlidePanel).
        /// </summary>
        private static List<string> CheckOrRepairTransitionChild(
            SerializedObject so, Transform parent,
            string propertyName, string childName,
            Color defaultColor, bool isRectTransformRef, bool dryRun)
        {
            List<string> issues = new List<string>();

            var prop = so.FindProperty(propertyName);
            if (prop.objectReferenceValue != null)
                return issues; // Reference is valid, all good

            // Reference is null - check if child GameObject exists
            Transform existingChild = parent.Find(childName);

            if (existingChild != null)
            {
                // Child exists but reference is broken
                if (dryRun)
                {
                    issues.Add($"TransitionCanvas: {propertyName} reference is null (child '{childName}' exists but not wired)");
                }
                else
                {
                    if (isRectTransformRef)
                        prop.objectReferenceValue = existingChild.GetComponent<RectTransform>();
                    else
                        prop.objectReferenceValue = existingChild.GetComponent<Image>();
                    issues.Add($"Re-wired {propertyName} to existing '{childName}'");
                }
            }
            else
            {
                // Child doesn't exist at all
                if (dryRun)
                {
                    issues.Add($"TransitionCanvas: missing '{childName}' child + {propertyName} = null");
                }
                else
                {
                    GameObject obj = new GameObject(childName);
                    obj.transform.SetParent(parent, false);
                    RectTransform rt = obj.AddComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.sizeDelta = Vector2.zero;
                    Image image = obj.AddComponent<Image>();
                    image.color = defaultColor;
                    image.raycastTarget = false;
                    obj.SetActive(false);

                    if (isRectTransformRef)
                        prop.objectReferenceValue = rt;
                    else
                        prop.objectReferenceValue = image;

                    issues.Add($"Created '{childName}' + wired {propertyName}");
                }
            }

            return issues;
        }

        /// <summary>
        /// Check or move misplaced children in TransitionCanvas.
        /// Valid children: FadeImage, CircleWipeImage, SlidePanel.
        /// </summary>
        private static List<string> CheckOrMoveMisplacedPopups(Transform transitionCanvas, string sceneName, bool dryRun)
        {
            List<string> issues = new List<string>();
            HashSet<string> validChildren = new HashSet<string> { "FadeImage", "CircleWipeImage", "SlidePanel" };

            List<Transform> misplaced = new List<Transform>();
            for (int i = 0; i < transitionCanvas.childCount; i++)
            {
                Transform child = transitionCanvas.GetChild(i);
                if (!validChildren.Contains(child.name))
                {
                    misplaced.Add(child);
                }
            }

            if (misplaced.Count == 0) return issues;

            if (dryRun)
            {
                foreach (var child in misplaced)
                    issues.Add($"Misplaced child in TransitionCanvas: '{child.name}'");
            }
            else
            {
                Canvas mainCanvas = FindMainSceneCanvas(transitionCanvas.gameObject);
                if (mainCanvas == null)
                {
                    foreach (var child in misplaced)
                        issues.Add($"Misplaced '{child.name}' in TransitionCanvas (no main canvas found to move to)");
                    return issues;
                }

                foreach (var child in misplaced)
                {
                    child.SetParent(mainCanvas.transform, false);
                    issues.Add($"Moved '{child.name}' from TransitionCanvas to {mainCanvas.name}");
                }
            }

            return issues;
        }

        // ==================== CREATION HELPERS ====================

        private static void CreateEffectsCanvas(GameObject parent, DigitPark.Animations.UIAnimationManager manager)
        {
            GameObject canvasObj = new GameObject("EffectsCanvas");
            canvasObj.transform.SetParent(parent.transform);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9998;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);

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

            SerializedObject so = new SerializedObject(manager);
            so.FindProperty("screenFlashImage").objectReferenceValue = flashImage;
            so.ApplyModifiedProperties();
        }

        private static GameObject CreateTransitionCanvas()
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

            // FadeImage
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

            // CircleWipeImage
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

            // SlidePanel
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

            // SceneTransitionManager component
            var manager = canvasObj.AddComponent<DigitPark.Animations.SceneTransitionManager>();

            SerializedObject so = new SerializedObject(manager);
            so.FindProperty("transitionCanvas").objectReferenceValue = canvas;
            so.FindProperty("fadeImage").objectReferenceValue = fadeImage;
            so.FindProperty("circleWipeImage").objectReferenceValue = circleImage;
            so.FindProperty("slidePanel").objectReferenceValue = slideRT;
            so.ApplyModifiedProperties();

            return canvasObj;
        }

        // ==================== UTILITY ====================

        private static Canvas FindMainSceneCanvas(GameObject excludeObj)
        {
            Canvas[] allCanvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);

            // First pass: root canvas not under ANIMATION_MANAGERS
            foreach (var canvas in allCanvases)
            {
                if (canvas.gameObject == excludeObj) continue;
                if (IsUnderAnimationManagers(canvas.transform)) continue;

                if (canvas.transform.parent == null || canvas.isRootCanvas)
                    return canvas;
            }

            // Fallback: any canvas not excluded
            foreach (var canvas in allCanvases)
            {
                if (canvas.gameObject == excludeObj) continue;
                if (IsUnderAnimationManagers(canvas.transform)) continue;
                return canvas;
            }

            return null;
        }

        private static bool IsUnderAnimationManagers(Transform t)
        {
            Transform parent = t.parent;
            while (parent != null)
            {
                if (parent.name == "---ANIMATION_MANAGERS---")
                    return true;
                parent = parent.parent;
            }
            return false;
        }
    }
}
#endif
