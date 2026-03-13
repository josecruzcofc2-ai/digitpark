using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System;
using System.Collections;

namespace DigitPark.Animations
{
    /// <summary>
    /// Handles animated scene transitions.
    /// Provides various transition effects like fade, circle wipe, slide, etc.
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        [Header("Transition Canvas")]
        [SerializeField] private Canvas transitionCanvas;
        [SerializeField] private Image fadeImage;
        [SerializeField] private Image circleWipeImage;
        [SerializeField] private RectTransform slidePanel;

        [Header("Settings")]
        [SerializeField] private float defaultDuration = 0.5f;
        [SerializeField] private Color fadeColor = Color.black;

        [Header("Circle Wipe Settings")]
        [SerializeField] private Material circleWipeMaterial;

        private bool isTransitioning;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                if (transform.parent != null)
                    transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
                SetupTransitionCanvas();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void SetupTransitionCanvas()
        {
            if (transitionCanvas == null)
            {
                GameObject canvasObj = new GameObject("TransitionCanvas");
                canvasObj.transform.SetParent(transform);

                transitionCanvas = canvasObj.AddComponent<Canvas>();
                transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                transitionCanvas.sortingOrder = 9999;

                canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasObj.AddComponent<GraphicRaycaster>();

                // Fade image
                GameObject fadeObj = new GameObject("FadeImage");
                fadeObj.transform.SetParent(canvasObj.transform, false);
                fadeImage = fadeObj.AddComponent<Image>();
                fadeImage.color = new Color(0, 0, 0, 0);
                fadeImage.raycastTarget = false;

                RectTransform fadeRT = fadeImage.rectTransform;
                fadeRT.anchorMin = Vector2.zero;
                fadeRT.anchorMax = Vector2.one;
                fadeRT.sizeDelta = Vector2.zero;

                fadeObj.SetActive(false);
            }
        }

        // ==================== PUBLIC TRANSITION METHODS ====================

        /// <summary>
        /// Simple fade transition
        /// </summary>
        public void FadeTransition(string sceneName, float duration = -1, Action onMidpoint = null)
        {
            if (isTransitioning) return;
            StartCoroutine(FadeTransitionCoroutine(sceneName, duration < 0 ? defaultDuration : duration, onMidpoint));
        }

        /// <summary>
        /// Fade to scene with callback
        /// </summary>
        public void FadeToScene(string sceneName, Action onComplete = null)
        {
            FadeTransition(sceneName, defaultDuration, onComplete);
        }

        /// <summary>
        /// Circle wipe transition (like old cartoons)
        /// </summary>
        public void CircleWipeTransition(string sceneName, float duration = -1)
        {
            if (isTransitioning) return;
            StartCoroutine(CircleWipeCoroutine(sceneName, duration < 0 ? defaultDuration : duration));
        }

        /// <summary>
        /// Slide transition
        /// </summary>
        public void SlideTransition(string sceneName, Direction direction, float duration = -1)
        {
            if (isTransitioning) return;
            StartCoroutine(SlideTransitionCoroutine(sceneName, direction, duration < 0 ? defaultDuration : duration));
        }

        /// <summary>
        /// Quick fade (no scene change, just visual)
        /// </summary>
        public Tween QuickFadeOut(float duration = 0.3f)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
            return fadeImage.DOFade(1f, duration);
        }

        /// <summary>
        /// Quick fade in (no scene change, just visual)
        /// </summary>
        public Tween QuickFadeIn(float duration = 0.3f)
        {
            return fadeImage.DOFade(0f, duration)
                .OnComplete(() => { if (fadeImage != null) fadeImage.gameObject.SetActive(false); });
        }

        /// <summary>
        /// Flash transition (white flash)
        /// </summary>
        public void FlashTransition(string sceneName, float duration = 0.4f)
        {
            if (isTransitioning) return;
            StartCoroutine(FlashTransitionCoroutine(sceneName, duration));
        }

        // ==================== COROUTINES ====================

        private IEnumerator FadeTransitionCoroutine(string sceneName, float duration, Action onMidpoint)
        {
            isTransitioning = true;

            fadeImage.gameObject.SetActive(true);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);

            // Fade out
            yield return fadeImage.DOFade(1f, duration / 2).WaitForCompletion();

            // Midpoint callback
            onMidpoint?.Invoke();

            // Load scene (scene unload kills all linked tweens via SetLink/OnDestroy)
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // Fade in
            yield return fadeImage.DOFade(0f, duration / 2).WaitForCompletion();

            fadeImage.gameObject.SetActive(false);
            isTransitioning = false;
        }

        private IEnumerator CircleWipeCoroutine(string sceneName, float duration)
        {
            isTransitioning = true;

            if (circleWipeImage != null && circleWipeMaterial != null)
            {
                circleWipeImage.gameObject.SetActive(true);
                circleWipeImage.material = circleWipeMaterial;

                // Animate circle closing
                float progress = 0;
                var wipeInTween = DOTween.To(() => progress, x =>
                {
                    progress = x;
                    if (circleWipeMaterial != null) circleWipeMaterial.SetFloat("_Progress", x);
                }, 1f, duration / 2).SetLink(gameObject);

                yield return new WaitForSeconds(duration / 2);

                // Load scene
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
                while (!asyncLoad.isDone)
                {
                    yield return null;
                }

                // Animate circle opening
                var wipeOutTween = DOTween.To(() => progress, x =>
                {
                    progress = x;
                    if (circleWipeMaterial != null) circleWipeMaterial.SetFloat("_Progress", x);
                }, 0f, duration / 2).SetLink(gameObject);

                yield return new WaitForSeconds(duration / 2);

                circleWipeImage.gameObject.SetActive(false);
            }
            else
            {
                // Fallback to fade
                yield return FadeTransitionCoroutine(sceneName, duration, null);
            }

            isTransitioning = false;
        }

        private IEnumerator SlideTransitionCoroutine(string sceneName, Direction direction, float duration)
        {
            isTransitioning = true;

            if (slidePanel == null)
            {
                yield return FadeTransitionCoroutine(sceneName, duration, null);
                yield break;
            }

            slidePanel.gameObject.SetActive(true);

            // Setup start position
            Vector2 startPos = GetSlideStartPosition(direction);
            Vector2 coverPos = Vector2.zero;
            Vector2 endPos = GetSlideEndPosition(direction);

            slidePanel.anchoredPosition = startPos;

            // Slide in
            yield return slidePanel.DOAnchorPos(coverPos, duration / 2).SetEase(Ease.InQuad).WaitForCompletion();

            // Load scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // Slide out
            yield return slidePanel.DOAnchorPos(endPos, duration / 2).SetEase(Ease.OutQuad).WaitForCompletion();

            slidePanel.gameObject.SetActive(false);
            isTransitioning = false;
        }

        private IEnumerator FlashTransitionCoroutine(string sceneName, float duration)
        {
            isTransitioning = true;

            fadeImage.gameObject.SetActive(true);
            fadeImage.color = new Color(1, 1, 1, 0);

            // Flash to white
            yield return fadeImage.DOFade(1f, duration * 0.3f).SetEase(Ease.OutQuad).WaitForCompletion();

            // Load scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // Fade from white
            yield return fadeImage.DOFade(0f, duration * 0.7f).SetEase(Ease.InQuad).WaitForCompletion();

            fadeImage.gameObject.SetActive(false);
            isTransitioning = false;
        }

        // ==================== HELPERS ====================

        private Vector2 GetSlideStartPosition(Direction direction)
        {
            RectTransform canvasRT = transitionCanvas != null ? transitionCanvas.GetComponent<RectTransform>() : null;
            float w = canvasRT != null ? canvasRT.rect.width : Screen.width;
            float h = canvasRT != null ? canvasRT.rect.height : Screen.height;

            switch (direction)
            {
                case Direction.Left: return new Vector2(-w, 0);
                case Direction.Right: return new Vector2(w, 0);
                case Direction.Up: return new Vector2(0, h);
                case Direction.Down: return new Vector2(0, -h);
                default: return Vector2.zero;
            }
        }

        private Vector2 GetSlideEndPosition(Direction direction)
        {
            RectTransform canvasRT = transitionCanvas != null ? transitionCanvas.GetComponent<RectTransform>() : null;
            float w = canvasRT != null ? canvasRT.rect.width : Screen.width;
            float h = canvasRT != null ? canvasRT.rect.height : Screen.height;

            switch (direction)
            {
                case Direction.Left: return new Vector2(w, 0);
                case Direction.Right: return new Vector2(-w, 0);
                case Direction.Up: return new Vector2(0, -h);
                case Direction.Down: return new Vector2(0, h);
                default: return Vector2.zero;
            }
        }

        /// <summary>
        /// Check if currently transitioning
        /// </summary>
        public bool IsTransitioning => isTransitioning;

        private void OnDestroy()
        {
            StopAllCoroutines();
            DOTween.Kill(gameObject);
        }

        /// <summary>
        /// Set fade color
        /// </summary>
        public void SetFadeColor(Color color)
        {
            fadeColor = color;
        }
    }
}
