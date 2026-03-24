using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor UIBuilder for the Boot/Splash scene.
    /// Creates UI elements ONLY inside the Canvas. Never touches root-level GameObjects
    /// (AuthenticationService, DailyRewardService, AchievementService, etc.)
    ///
    /// Scene hierarchy after build:
    ///   Boot (scene root)
    ///   ├── Main Camera               ← PRESERVED (not touched)
    ///   ├── UIBuilder                  ← PRESERVED (BootManager + runtime BootUIBuilder live here)
    ///   ├── AuthenticationService      ← PRESERVED
    ///   ├── PremiumDebugController     ← PRESERVED
    ///   ├── DailyRewardService         ← PRESERVED
    ///   ├── AchievementService         ← PRESERVED
    ///   ├── AchievementDebugPanel      ← PRESERVED
    ///   ├── EditorBootConfig           ← PRESERVED
    ///   ├── BootAnimator               ← Created/reused (wired automatically)
    ///   └── Canvas                     ← UI lives here (rebuilt by this builder)
    ///       ├── Background
    ///       │   ├── GradientTop
    ///       │   └── Vignette
    ///       ├── NeonParticles
    ///       ├── LogoContainer          ← CanvasGroup (alpha=0, BootAnimator fades in)
    ///       │   ├── BrainLogo
    ///       │   └── TextLogo
    ///       ├── Subtitle
    ///       ├── Subtitle2
    ///       ├── LoadingBarContainer
    ///       │   ├── LoadingBarGlowOuter
    ///       │   └── LoadingBarFill
    ///       ├── LoadingText
    ///       └── VersionText
    ///
    /// Menu: DigitPark > Scenes > Build Scene > Core > Boot
    /// </summary>
    public static class BootUIBuilder
    {
        // Colors (default neon theme - ThemeApplier tints at runtime)
        // Unified normal-mode background (same as all non-CashBattle scenes)
        private static readonly Color PrimaryBackground = new Color(0.02f, 0.04f, 0.08f, 1f); // #050A14
        private static readonly Color CyanAccent = new Color(0f, 1f, 1f, 1f);
        private static readonly Color MagentaAccent = new Color(1f, 0f, 0.5f, 1f);
        private static readonly Color GlowCyan = new Color(0f, 1f, 1f, 0.5f);
        private static readonly Color InputBG = new Color(0.1f, 0.1f, 0.15f, 1f);
        private static readonly Color TextSecondary = new Color(0.7f, 0.7f, 0.7f, 1f);
        private static readonly Color TextDisabled = new Color(0.4f, 0.4f, 0.4f, 1f);
        private static readonly Color TextLoading = new Color(0.8f, 0.8f, 0.8f, 1f);
        // VignetteColor removed — Boot uses solid background like all normal scenes

        // Asset paths
        private const string BRAIN_LOGO_PATH = "Assets/_Project/Resources/UI/LogoDigitPark_Brain.png";
        private const string TEXT_LOGO_PATH = "Assets/_Project/Resources/UI/LogoDigitPark_Text.png";
        private const string FONT_ASSET_PATH = "Assets/_Project/Art/Fonts/Rajdhani/Rajdhani-Medium SDF.asset";

        // Font sizes (match FontSizes.cs)
        private const float FONT_LOGO = 288f;
        private const float FONT_SYMBOL = 88f;
        private const float FONT_H2 = 64f;
        private const float FONT_H3 = 56f;
        private const float FONT_MIN_BODY = 28f;

        private static TMP_FontAsset DefaultFont => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_ASSET_PATH);

        // =================================================================
        // IMPORTANT: Names of root-level scene objects we must NEVER delete.
        // The cleanup only removes children INSIDE the Canvas.
        // =================================================================
        private static readonly string[] CANVAS_CHILDREN_TO_CLEAN = {
            "Background", "NeonParticles", "LogoContainer",
            "Subtitle", "Subtitle2", "LoadingBarContainer",
            "LoadingText", "VersionText"
        };

        [MenuItem("DigitPark/Scenes/Build Scene/Core/Boot", false, 50)]
        public static void RebuildBootScene()
        {
            try
            {
                Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
                if (canvas == null)
                {
                    Debug.LogError("[BootUIBuilder] No Canvas found in scene. Add a Canvas GameObject first.");
                    return;
                }

                Debug.Log("[BootUIBuilder] Starting Boot UI Rebuild...");

                // SAFE cleanup: only remove UI children inside the Canvas
                CleanCanvasChildren(canvas);

                // Build all UI elements inside Canvas
                BuildBackground(canvas);
                var neonParticles = BuildNeonParticles(canvas);
                var (logoContainer, logoCanvasGroup) = BuildAnimatedLogo(canvas);
                BuildSubtitles(canvas);
                var (loadingBar, loadingBarGlow) = BuildLoadingBar(canvas);
                var loadingText = BuildLoadingText(canvas);
                var versionText = BuildVersionText(canvas);

                // Wire BootManager references (finds existing on "UIBuilder" GO)
                WireBootManager(loadingBar, loadingText, versionText);

                // Wire BootAnimator references (creates GO if needed)
                WireBootAnimator(logoCanvasGroup, logoContainer,
                    loadingText, loadingBarGlow, loadingBar, neonParticles);

                Canvas.ForceUpdateCanvases();
                EditorUtility.SetDirty(canvas.gameObject);

                Debug.Log("[BootUIBuilder] Boot UI rebuilt successfully!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BootUIBuilder] Error: {e.Message}\n{e.StackTrace}");
            }
        }

        #region Safe Cleanup (Canvas children only)

        /// <summary>
        /// Only removes known UI children inside the Canvas.
        /// NEVER touches root-level GameObjects (services, managers, camera, etc.)
        /// </summary>
        private static void CleanCanvasChildren(Canvas canvas)
        {
            foreach (string childName in CANVAS_CHILDREN_TO_CLEAN)
            {
                Transform t = canvas.transform.Find(childName);
                if (t != null)
                {
                    Object.DestroyImmediate(t.gameObject);
                    Debug.Log($"[BootUIBuilder] Removed old Canvas child: {childName}");
                }
            }
        }

        #endregion

        #region Background

        private static void BuildBackground(Canvas canvas)
        {
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(canvas.transform, false);
            bg.transform.SetAsFirstSibling();

            RectTransform bgRT = bg.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;

            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = Color.white; // ThemeApplier will tint at runtime
            bgImg.raycastTarget = false;
            // Solid background — no gradient, no vignette (unified with all normal scenes)
        }

        #endregion

        #region Neon Particles

        private static ParticleSystem BuildNeonParticles(Canvas canvas)
        {
            GameObject particlesObj = new GameObject("NeonParticles");
            particlesObj.transform.SetParent(canvas.transform, false);

            RectTransform rt = particlesObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;

            ParticleSystem ps = particlesObj.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.loop = true;
            main.startLifetime = 4f;
            main.startSpeed = 20f;
            main.startSize = new ParticleSystem.MinMaxCurve(15f, 45f);
            main.maxParticles = 50;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.playOnAwake = false;
            main.startColor = new ParticleSystem.MinMaxGradient(CyanAccent, MagentaAccent);

            var emission = ps.emission;
            emission.rateOverTime = 8f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Rectangle;
            shape.scale = new Vector3(800, 1600, 1);

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.6f, 0.2f),
                    new GradientAlphaKey(0.6f, 0.8f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            col.color = gradient;

            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.5f),
                new Keyframe(0.5f, 1f),
                new Keyframe(1f, 0.3f)
            ));

            var vol = ps.velocityOverLifetime;
            vol.enabled = true;
            vol.x = new ParticleSystem.MinMaxCurve(-5f, 5f);
            vol.y = new ParticleSystem.MinMaxCurve(-10f, 10f);
            vol.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            var renderer = particlesObj.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 1;

            return ps;
        }

        #endregion

        #region Logo

        private static (RectTransform logoContainer, CanvasGroup logoCanvasGroup) BuildAnimatedLogo(Canvas canvas)
        {
            GameObject logoObj = new GameObject("LogoContainer");
            logoObj.transform.SetParent(canvas.transform, false);

            RectTransform logoRT = logoObj.AddComponent<RectTransform>();
            logoRT.anchorMin = new Vector2(0.5f, 0.55f);
            logoRT.anchorMax = new Vector2(0.5f, 0.55f);
            logoRT.pivot = new Vector2(0.5f, 0.5f);
            logoRT.sizeDelta = new Vector2(2250, 1650);
            logoRT.anchoredPosition = new Vector2(0, -200);

            CanvasGroup cg = logoObj.AddComponent<CanvasGroup>();
            cg.alpha = 0f; // Starts invisible — BootAnimator fades in at runtime

            // Brain logo
            Sprite brainSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BRAIN_LOGO_PATH);
            if (brainSprite != null)
            {
                GameObject brainObj = new GameObject("BrainLogo");
                brainObj.transform.SetParent(logoObj.transform, false);

                RectTransform brainRT = brainObj.AddComponent<RectTransform>();
                brainRT.anchorMin = new Vector2(0.5f, 0.55f);
                brainRT.anchorMax = new Vector2(0.5f, 0.55f);
                brainRT.pivot = new Vector2(0.5f, 0f);
                brainRT.sizeDelta = new Vector2(840, 840);
                brainRT.anchoredPosition = new Vector2(0, 50);

                Image brainImg = brainObj.AddComponent<Image>();
                brainImg.sprite = brainSprite;
                brainImg.preserveAspect = true;
                brainImg.raycastTarget = false;
            }
            else
            {
                Debug.LogWarning($"[BootUIBuilder] Brain logo not found at: {BRAIN_LOGO_PATH}");
            }

            // Text logo
            Sprite textSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TEXT_LOGO_PATH);
            if (textSprite != null)
            {
                GameObject textLogoObj = new GameObject("TextLogo");
                textLogoObj.transform.SetParent(logoObj.transform, false);

                RectTransform textRT = textLogoObj.AddComponent<RectTransform>();
                textRT.anchorMin = new Vector2(0.5f, 0.55f);
                textRT.anchorMax = new Vector2(0.5f, 0.55f);
                textRT.pivot = new Vector2(0.5f, 1f);
                textRT.sizeDelta = new Vector2(1950, 720);
                textRT.anchoredPosition = new Vector2(0, 185);

                Image textImg = textLogoObj.AddComponent<Image>();
                textImg.sprite = textSprite;
                textImg.preserveAspect = true;
                textImg.raycastTarget = false;
            }
            else
            {
                // Fallback: TMP text
                Debug.LogWarning($"[BootUIBuilder] Text logo not found at: {TEXT_LOGO_PATH}. Using TMP fallback.");
                GameObject fallbackObj = new GameObject("Title");
                fallbackObj.transform.SetParent(logoObj.transform, false);

                RectTransform fRT = fallbackObj.AddComponent<RectTransform>();
                fRT.anchorMin = new Vector2(0.5f, 0.4f);
                fRT.anchorMax = new Vector2(0.5f, 0.4f);
                fRT.pivot = new Vector2(0.5f, 0.5f);
                fRT.sizeDelta = new Vector2(1800, 300);

                TextMeshProUGUI titleTMP = fallbackObj.AddComponent<TextMeshProUGUI>();
                titleTMP.text = "DIGIT PARK";
                titleTMP.fontSize = FONT_LOGO;
                titleTMP.fontStyle = FontStyles.Bold;
                titleTMP.color = CyanAccent;
                titleTMP.alignment = TextAlignmentOptions.Center;
                titleTMP.enableAutoSizing = true;
                titleTMP.fontSizeMin = FONT_H2;
                titleTMP.fontSizeMax = FONT_LOGO;
                titleTMP.outlineWidth = 0.25f;
                titleTMP.outlineColor = CyanAccent;
                if (DefaultFont != null) titleTMP.font = DefaultFont;
            }

            return (logoRT, cg);
        }

        #endregion

        #region Subtitles

        private static void BuildSubtitles(Canvas canvas)
        {
            // Subtitle 1: "ARCADE EXPERIENCE" (localized at runtime via AutoLocalizer)
            GameObject sub1Obj = new GameObject("Subtitle");
            sub1Obj.transform.SetParent(canvas.transform, false);

            RectTransform sub1RT = sub1Obj.AddComponent<RectTransform>();
            sub1RT.anchorMin = new Vector2(0.5f, 0.30f);
            sub1RT.anchorMax = new Vector2(0.5f, 0.30f);
            sub1RT.pivot = new Vector2(0.5f, 0.5f);
            sub1RT.anchoredPosition = Vector2.zero;
            sub1RT.sizeDelta = new Vector2(1500, 90);

            TextMeshProUGUI sub1 = sub1Obj.AddComponent<TextMeshProUGUI>();
            sub1.text = "ARCADE EXPERIENCE";
            sub1.fontSize = FONT_H2;
            sub1.color = TextSecondary;
            sub1.alignment = TextAlignmentOptions.Center;
            sub1.fontStyle = FontStyles.Bold;
            sub1.characterSpacing = 6f;
            sub1.enableAutoSizing = true;
            sub1.fontSizeMin = FONT_MIN_BODY;
            sub1.fontSizeMax = FONT_H2;
            if (DefaultFont != null) sub1.font = DefaultFont;

            // Subtitle 2: "Mind Skills" (localized at runtime)
            GameObject sub2Obj = new GameObject("Subtitle2");
            sub2Obj.transform.SetParent(canvas.transform, false);

            RectTransform sub2RT = sub2Obj.AddComponent<RectTransform>();
            sub2RT.anchorMin = new Vector2(0.5f, 0.26f);
            sub2RT.anchorMax = new Vector2(0.5f, 0.26f);
            sub2RT.pivot = new Vector2(0.5f, 0.5f);
            sub2RT.anchoredPosition = Vector2.zero;
            sub2RT.sizeDelta = new Vector2(1500, 75);

            TextMeshProUGUI sub2 = sub2Obj.AddComponent<TextMeshProUGUI>();
            sub2.text = "Mind Skills";
            sub2.fontSize = FONT_H3;
            sub2.color = CyanAccent;
            sub2.alignment = TextAlignmentOptions.Center;
            sub2.fontStyle = FontStyles.Bold;
            sub2.characterSpacing = 4f;
            sub2.alpha = 0.7f;
            sub2.enableAutoSizing = true;
            sub2.fontSizeMin = FONT_MIN_BODY;
            sub2.fontSizeMax = FONT_H3;
            if (DefaultFont != null) sub2.font = DefaultFont;
        }

        #endregion

        #region Loading Bar

        private static (Image loadingBarFill, Image loadingBarGlow) BuildLoadingBar(Canvas canvas)
        {
            GameObject container = new GameObject("LoadingBarContainer");
            container.transform.SetParent(canvas.transform, false);

            RectTransform containerRT = container.AddComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0.5f, 0.20f);
            containerRT.anchorMax = new Vector2(0.5f, 0.20f);
            containerRT.pivot = new Vector2(0.5f, 0.5f);
            containerRT.sizeDelta = new Vector2(600, 12);
            containerRT.anchoredPosition = Vector2.zero;

            Image barBG = container.AddComponent<Image>();
            barBG.color = InputBG;
            barBG.raycastTarget = false;

            // Glow outer (pulsing — animated by BootAnimator at runtime)
            GameObject glowOuter = new GameObject("LoadingBarGlowOuter");
            glowOuter.transform.SetParent(container.transform, false);
            glowOuter.transform.SetAsFirstSibling();

            RectTransform glowRT = glowOuter.AddComponent<RectTransform>();
            glowRT.anchorMin = Vector2.zero;
            glowRT.anchorMax = Vector2.one;
            glowRT.sizeDelta = new Vector2(20, 20);
            glowRT.anchoredPosition = Vector2.zero;

            Image glowImg = glowOuter.AddComponent<Image>();
            glowImg.color = GlowCyan;
            glowImg.raycastTarget = false;

            // Fill (progress bar)
            GameObject fillObj = new GameObject("LoadingBarFill");
            fillObj.transform.SetParent(container.transform, false);

            RectTransform fillRT = fillObj.AddComponent<RectTransform>();
            fillRT.anchorMin = new Vector2(0, 0.1f);
            fillRT.anchorMax = new Vector2(0, 0.9f);
            fillRT.pivot = new Vector2(0, 0.5f);
            fillRT.anchoredPosition = new Vector2(2, 0);
            fillRT.sizeDelta = Vector2.zero;

            Image fillImg = fillObj.AddComponent<Image>();
            fillImg.color = CyanAccent;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = 0;
            fillImg.fillAmount = 0f;
            fillImg.raycastTarget = false;

            return (fillImg, glowImg);
        }

        #endregion

        #region Loading Text

        private static TextMeshProUGUI BuildLoadingText(Canvas canvas)
        {
            GameObject textObj = new GameObject("LoadingText");
            textObj.transform.SetParent(canvas.transform, false);

            RectTransform rt = textObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.14f);
            rt.anchorMax = new Vector2(0.5f, 0.14f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(900, 120);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "";
            tmp.fontSize = FONT_SYMBOL;
            tmp.color = TextLoading;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = true;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FONT_MIN_BODY;
            tmp.fontSizeMax = FONT_SYMBOL;
            if (DefaultFont != null) tmp.font = DefaultFont;

            return tmp;
        }

        #endregion

        #region Version Text

        private static TextMeshProUGUI BuildVersionText(Canvas canvas)
        {
            GameObject textObj = new GameObject("VersionText");
            textObj.transform.SetParent(canvas.transform, false);

            RectTransform rt = textObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-20, 20);
            rt.sizeDelta = new Vector2(600, 120);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = $"v{Application.version}";
            tmp.fontSize = FONT_H2;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FONT_MIN_BODY;
            tmp.fontSizeMax = tmp.fontSize;
            tmp.color = TextDisabled;
            tmp.alignment = TextAlignmentOptions.BottomRight;
            if (DefaultFont != null) tmp.font = DefaultFont;

            return tmp;
        }

        #endregion

        #region Auto-Wire References

        /// <summary>
        /// Finds the existing BootManager (on "UIBuilder" GO or anywhere in scene)
        /// and wires its public UI references. Never creates a new GO — BootManager
        /// must already exist in the scene.
        /// </summary>
        private static void WireBootManager(Image loadingBar,
            TextMeshProUGUI loadingText, TextMeshProUGUI versionText)
        {
            // Priority 1: Find existing BootManager component anywhere in scene
            var bootManager = Object.FindObjectOfType<Managers.BootManager>();

            if (bootManager == null)
            {
                // Priority 2: Check the "UIBuilder" GameObject (where it lives in Boot scene)
                GameObject uiBuilderObj = GameObject.Find("UIBuilder");
                if (uiBuilderObj != null)
                {
                    bootManager = uiBuilderObj.GetComponent<Managers.BootManager>();
                    if (bootManager == null)
                        bootManager = Undo.AddComponent<Managers.BootManager>(uiBuilderObj);
                }
                else
                {
                    Debug.LogError("[BootUIBuilder] No 'UIBuilder' GameObject found. " +
                        "BootManager must exist in the scene. Add a GameObject named 'UIBuilder' " +
                        "with the BootManager component.");
                    return;
                }
            }

            // Wire public references
            bootManager.loadingBar = loadingBar;
            bootManager.loadingText = loadingText;
            bootManager.versionText = versionText;

            EditorUtility.SetDirty(bootManager);
            Debug.Log($"[BootUIBuilder] BootManager wired on '{bootManager.gameObject.name}' " +
                $"(loadingBar={loadingBar != null}, loadingText={loadingText != null}, " +
                $"versionText={versionText != null})");
        }

        /// <summary>
        /// Finds or creates a BootAnimator and wires all its SerializeField references
        /// via SerializedObject (because the fields are private).
        /// </summary>
        private static void WireBootAnimator(CanvasGroup logoCanvasGroup,
            RectTransform logoContainer, TextMeshProUGUI loadingText,
            Image loadingBarGlow, Image loadingBarFill, ParticleSystem neonParticles)
        {
            // Find existing BootAnimator
            var bootAnimator = Object.FindObjectOfType<UI.BootAnimator>();

            if (bootAnimator == null)
            {
                // Create new BootAnimator GO at root level (not inside Canvas)
                GameObject animObj = new GameObject("BootAnimator");
                Undo.RegisterCreatedObjectUndo(animObj, "Create BootAnimator");
                bootAnimator = animObj.AddComponent<UI.BootAnimator>();
            }

            // Wire private [SerializeField] references via SerializedObject
            SerializedObject so = new SerializedObject(bootAnimator);
            so.FindProperty("loadingText").objectReferenceValue = loadingText;
            so.FindProperty("loadingBarFill").objectReferenceValue = loadingBarFill;
            so.FindProperty("loadingBarGlow").objectReferenceValue = loadingBarGlow;
            so.FindProperty("logoCanvasGroup").objectReferenceValue = logoCanvasGroup;
            so.FindProperty("logoTransform").objectReferenceValue = logoContainer;
            so.FindProperty("neonParticles").objectReferenceValue = neonParticles;
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(bootAnimator);
            Debug.Log($"[BootUIBuilder] BootAnimator wired on '{bootAnimator.gameObject.name}' " +
                $"(logo={logoCanvasGroup != null}, bar={loadingBarFill != null}, " +
                $"glow={loadingBarGlow != null}, particles={neonParticles != null})");
        }

        #endregion
    }
}
