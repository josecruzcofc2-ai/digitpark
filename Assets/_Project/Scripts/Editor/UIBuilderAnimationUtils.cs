#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Utility class for UI Builders to add animation components.
    /// Use these methods when creating UI elements programmatically.
    ///
    /// Usage in your UI Builder:
    ///   var button = CreateMyButton();
    ///   UIBuilderAnimationUtils.AddButton3D(button); // Optional animation
    /// </summary>
    public static class UIBuilderAnimationUtils
    {
        // ==================== BUTTON ANIMATIONS ====================

        /// <summary>
        /// Add Button3D component to a button for 3D press effect.
        /// Safe to call - won't add if already exists.
        /// </summary>
        public static DigitPark.Animations.Button3D AddButton3D(GameObject buttonObj)
        {
            if (buttonObj == null) return null;

            var existing = buttonObj.GetComponent<DigitPark.Animations.Button3D>();
            if (existing != null) return existing;

            return buttonObj.AddComponent<DigitPark.Animations.Button3D>();
        }

        /// <summary>
        /// Add simple pulse effect to a button (attention grabber).
        /// Great for CTA buttons like "PLAY", "CLAIM", "BUY".
        /// </summary>
        public static DigitPark.Animations.SimplePulse AddPulse(GameObject obj, float minScale = 0.95f, float maxScale = 1.05f)
        {
            if (obj == null) return null;

            var existing = obj.GetComponent<DigitPark.Animations.SimplePulse>();
            if (existing != null) return existing;

            var pulse = obj.AddComponent<DigitPark.Animations.SimplePulse>();

            // Set values via SerializedObject
            SerializedObject so = new SerializedObject(pulse);
            so.FindProperty("minScale").floatValue = minScale;
            so.FindProperty("maxScale").floatValue = maxScale;
            so.ApplyModifiedProperties();

            return pulse;
        }

        /// <summary>
        /// Add floating effect (up/down movement).
        /// Great for icons, rewards, or attention elements.
        /// </summary>
        public static DigitPark.Animations.SimpleFloat AddFloat(GameObject obj, float distance = 10f, float duration = 2f)
        {
            if (obj == null) return null;

            var existing = obj.GetComponent<DigitPark.Animations.SimpleFloat>();
            if (existing != null) return existing;

            var floatEffect = obj.AddComponent<DigitPark.Animations.SimpleFloat>();

            SerializedObject so = new SerializedObject(floatEffect);
            so.FindProperty("distance").floatValue = distance;
            so.FindProperty("duration").floatValue = duration;
            so.ApplyModifiedProperties();

            return floatEffect;
        }

        /// <summary>
        /// Add rotation effect (continuous spin).
        /// Great for loading indicators, coins, stars.
        /// </summary>
        public static DigitPark.Animations.SimpleRotate AddRotation(GameObject obj, float speed = 30f)
        {
            if (obj == null) return null;

            var existing = obj.GetComponent<DigitPark.Animations.SimpleRotate>();
            if (existing != null) return existing;

            var rotate = obj.AddComponent<DigitPark.Animations.SimpleRotate>();

            SerializedObject so = new SerializedObject(rotate);
            so.FindProperty("speed").floatValue = speed;
            so.ApplyModifiedProperties();

            return rotate;
        }

        /// <summary>
        /// Add glow pulse effect to an Image.
        /// Great for highlights, important elements.
        /// </summary>
        public static DigitPark.Animations.GlowPulse AddGlowPulse(GameObject obj, float minAlpha = 0.3f, float maxAlpha = 0.8f)
        {
            if (obj == null) return null;

            // Requires Image component
            if (obj.GetComponent<Image>() == null)
            {
                Debug.LogWarning($"[UIBuilderAnimationUtils] GlowPulse requires Image component on {obj.name}");
                return null;
            }

            var existing = obj.GetComponent<DigitPark.Animations.GlowPulse>();
            if (existing != null) return existing;

            var glow = obj.AddComponent<DigitPark.Animations.GlowPulse>();

            SerializedObject so = new SerializedObject(glow);
            so.FindProperty("minAlpha").floatValue = minAlpha;
            so.FindProperty("maxAlpha").floatValue = maxAlpha;
            so.ApplyModifiedProperties();

            return glow;
        }

        /// <summary>
        /// Add full UIEffects component with multiple effects.
        /// Configure which effects are enabled after adding.
        /// </summary>
        public static DigitPark.Animations.UIEffects AddUIEffects(GameObject obj)
        {
            if (obj == null) return null;

            var existing = obj.GetComponent<DigitPark.Animations.UIEffects>();
            if (existing != null) return existing;

            return obj.AddComponent<DigitPark.Animations.UIEffects>();
        }

        // ==================== COMPLETE BUTTON CREATION ====================

        /// <summary>
        /// Create a complete 3D button with all visual elements.
        /// Returns the button GameObject ready to use.
        /// </summary>
        public static GameObject CreateButton3DComplete(
            Transform parent,
            string name,
            string text,
            Vector2 size,
            Color faceColor,
            Color shadowColor,
            Color textColor)
        {
            // Main button container
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);

            RectTransform buttonRT = buttonObj.AddComponent<RectTransform>();
            buttonRT.sizeDelta = size;

            Button button = buttonObj.AddComponent<Button>();
            Image buttonBg = buttonObj.AddComponent<Image>();
            buttonBg.color = new Color(0, 0, 0, 0); // Transparent

            // Shadow
            GameObject shadowObj = new GameObject("Shadow");
            shadowObj.transform.SetParent(buttonObj.transform, false);
            RectTransform shadowRT = shadowObj.AddComponent<RectTransform>();
            shadowRT.anchorMin = Vector2.zero;
            shadowRT.anchorMax = Vector2.one;
            shadowRT.sizeDelta = new Vector2(0, -6);
            shadowRT.anchoredPosition = new Vector2(0, -3);
            Image shadowImage = shadowObj.AddComponent<Image>();
            shadowImage.color = shadowColor;

            // Face
            GameObject faceObj = new GameObject("Face");
            faceObj.transform.SetParent(buttonObj.transform, false);
            RectTransform faceRT = faceObj.AddComponent<RectTransform>();
            faceRT.anchorMin = Vector2.zero;
            faceRT.anchorMax = Vector2.one;
            faceRT.sizeDelta = new Vector2(0, -6);
            faceRT.anchoredPosition = new Vector2(0, 3);
            Image faceImage = faceObj.AddComponent<Image>();
            faceImage.color = faceColor;

            // Highlight line
            GameObject highlightObj = new GameObject("Highlight");
            highlightObj.transform.SetParent(faceObj.transform, false);
            RectTransform highlightRT = highlightObj.AddComponent<RectTransform>();
            highlightRT.anchorMin = new Vector2(0.05f, 0.75f);
            highlightRT.anchorMax = new Vector2(0.95f, 0.85f);
            highlightRT.sizeDelta = Vector2.zero;
            Image highlightImage = highlightObj.AddComponent<Image>();
            highlightImage.color = new Color(1f, 1f, 1f, 0.3f);

            // Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(faceObj.transform, false);
            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 28;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = textColor;
            tmp.fontStyle = FontStyles.Bold;

            // Add Button3D component
            var button3D = buttonObj.AddComponent<DigitPark.Animations.Button3D>();

            // Configure Button3D via SerializedObject
            SerializedObject so = new SerializedObject(button3D);
            so.FindProperty("buttonFace").objectReferenceValue = faceRT;
            so.FindProperty("buttonShadow").objectReferenceValue = shadowRT;
            so.FindProperty("highlightLine").objectReferenceValue = highlightImage;
            so.FindProperty("normalColor").colorValue = faceColor;
            so.FindProperty("pressedColor").colorValue = new Color(faceColor.r * 0.7f, faceColor.g * 0.7f, faceColor.b * 0.7f, faceColor.a);
            so.ApplyModifiedProperties();

            return buttonObj;
        }

        /// <summary>
        /// Create a CTA (Call To Action) button with pulse effect.
        /// Perfect for "PLAY", "START", "CLAIM" buttons.
        /// </summary>
        public static GameObject CreateCTAButton(
            Transform parent,
            string name,
            string text,
            Vector2 size,
            Color color)
        {
            Color shadowColor = new Color(color.r * 0.5f, color.g * 0.5f, color.b * 0.5f, 1f);
            GameObject button = CreateButton3DComplete(parent, name, text, size, color, shadowColor, Color.white);

            // Add pulse for attention
            AddPulse(button, 0.97f, 1.03f);

            return button;
        }

        // ==================== GLOW CREATION ====================

        /// <summary>
        /// Create a glow effect behind an element.
        /// </summary>
        public static GameObject CreateGlowBehind(Transform parent, Color glowColor, float extraSize = 20f)
        {
            GameObject glowObj = new GameObject("Glow");
            glowObj.transform.SetParent(parent, false);
            glowObj.transform.SetAsFirstSibling(); // Behind other elements

            RectTransform glowRT = glowObj.AddComponent<RectTransform>();
            glowRT.anchorMin = Vector2.zero;
            glowRT.anchorMax = Vector2.one;
            glowRT.sizeDelta = new Vector2(extraSize, extraSize);

            Image glowImage = glowObj.AddComponent<Image>();
            glowImage.color = new Color(glowColor.r, glowColor.g, glowColor.b, 0.5f);
            glowImage.raycastTarget = false;

            // Add pulse
            AddGlowPulse(glowObj, 0.3f, 0.7f);

            return glowObj;
        }

        // ==================== SCENE ANIMATORS ====================

        /// <summary>
        /// Add MainMenuAnimator to a scene.
        /// Call this when building MainMenu UI.
        /// </summary>
        public static DigitPark.Animations.MainMenuAnimator AddMainMenuAnimator(GameObject container)
        {
            if (container == null) return null;

            var existing = container.GetComponent<DigitPark.Animations.MainMenuAnimator>();
            if (existing != null) return existing;

            return container.AddComponent<DigitPark.Animations.MainMenuAnimator>();
        }

        /// <summary>
        /// Add MatchmakingAnimator to a scene.
        /// </summary>
        public static DigitPark.Animations.MatchmakingAnimator AddMatchmakingAnimator(GameObject container)
        {
            if (container == null) return null;

            var existing = container.GetComponent<DigitPark.Animations.MatchmakingAnimator>();
            if (existing != null) return existing;

            return container.AddComponent<DigitPark.Animations.MatchmakingAnimator>();
        }

        /// <summary>
        /// Add RewardClaimAnimator to a scene.
        /// </summary>
        public static DigitPark.Animations.RewardClaimAnimator AddRewardAnimator(GameObject container)
        {
            if (container == null) return null;

            var existing = container.GetComponent<DigitPark.Animations.RewardClaimAnimator>();
            if (existing != null) return existing;

            var animator = container.AddComponent<DigitPark.Animations.RewardClaimAnimator>();

            // Add AudioSource if needed
            if (container.GetComponent<AudioSource>() == null)
                container.AddComponent<AudioSource>();

            return animator;
        }

        /// <summary>
        /// Add GameSelectorAnimator to a scene.
        /// </summary>
        public static DigitPark.Animations.GameSelectorAnimator AddGameSelectorAnimator(GameObject container)
        {
            if (container == null) return null;

            var existing = container.GetComponent<DigitPark.Animations.GameSelectorAnimator>();
            if (existing != null) return existing;

            return container.AddComponent<DigitPark.Animations.GameSelectorAnimator>();
        }

        /// <summary>
        /// Add CurrencyAnimator to a currency display.
        /// </summary>
        public static DigitPark.Animations.CurrencyAnimator AddCurrencyAnimator(GameObject currencyDisplay)
        {
            if (currencyDisplay == null) return null;

            var existing = currencyDisplay.GetComponent<DigitPark.Animations.CurrencyAnimator>();
            if (existing != null) return existing;

            var animator = currencyDisplay.AddComponent<DigitPark.Animations.CurrencyAnimator>();

            if (currencyDisplay.GetComponent<AudioSource>() == null)
                currencyDisplay.AddComponent<AudioSource>();

            return animator;
        }

        /// <summary>
        /// Add TrophyShowcaseAnimator to the Achievements scene.
        /// Handles entrance animations, unlock celebrations, card effects.
        /// </summary>
        public static DigitPark.Animations.TrophyShowcaseAnimator AddTrophyShowcaseAnimator(GameObject container)
        {
            if (container == null) return null;

            var existing = container.GetComponent<DigitPark.Animations.TrophyShowcaseAnimator>();
            if (existing != null) return existing;

            var animator = container.AddComponent<DigitPark.Animations.TrophyShowcaseAnimator>();

            if (container.GetComponent<AudioSource>() == null)
                container.AddComponent<AudioSource>();

            return animator;
        }
    }
}
#endif
