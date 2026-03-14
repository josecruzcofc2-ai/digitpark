using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Services;
using DigitPark.Localization;

namespace DigitPark.UI
{
    /// <summary>
    /// Banner de estado de red que se muestra cuando no hay conexion
    /// Persiste entre escenas con DontDestroyOnLoad
    /// </summary>
    public class NetworkStatusBanner : MonoBehaviour
    {
        public static NetworkStatusBanner Instance { get; private set; }

        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private Image _backgroundImage;
        private TextMeshProUGUI _statusText;
        private RectTransform _bannerRect;

        private Coroutine _animationCoroutine;
        #pragma warning disable 0414
        private bool _isVisible = false;
        #pragma warning restore 0414

        // Colores
        private readonly Color _offlineColor = new Color(0.9f, 0.2f, 0.2f, 0.95f);
        private readonly Color _reconnectingColor = new Color(0.95f, 0.7f, 0.1f, 0.95f);
        private readonly Color _restoredColor = new Color(0.2f, 0.8f, 0.3f, 0.95f);

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                if (transform.parent != null)
                    transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
                CreateBannerUI();
                HideImmediate();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Suscribirse a eventos de NetworkService
            if (NetworkService.Instance != null)
            {
                NetworkService.Instance.OnConnectionLost += OnConnectionLost;
                NetworkService.Instance.OnConnectionRestored += OnConnectionRestored;
            }
        }

        private void OnDestroy()
        {
            if (NetworkService.Instance != null)
            {
                NetworkService.Instance.OnConnectionLost -= OnConnectionLost;
                NetworkService.Instance.OnConnectionRestored -= OnConnectionRestored;
            }
        }

        /// <summary>
        /// Crea el UI del banner programaticamente
        /// </summary>
        private void CreateBannerUI()
        {
            // Canvas
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 9999; // Siempre encima de todo

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);

            gameObject.AddComponent<GraphicRaycaster>();

            // Banner container
            GameObject bannerObj = new GameObject("Banner");
            bannerObj.transform.SetParent(transform, false);

            _bannerRect = bannerObj.AddComponent<RectTransform>();
            _bannerRect.anchorMin = new Vector2(0, 1);
            _bannerRect.anchorMax = new Vector2(1, 1);
            _bannerRect.pivot = new Vector2(0.5f, 1);
            _bannerRect.sizeDelta = new Vector2(0, 80);
            _bannerRect.anchoredPosition = Vector2.zero;

            _backgroundImage = bannerObj.AddComponent<Image>();
            _backgroundImage.color = _offlineColor;

            _canvasGroup = bannerObj.AddComponent<CanvasGroup>();
            _canvasGroup.blocksRaycasts = false; // No bloquea UI debajo
            _canvasGroup.interactable = false;

            // Texto
            GameObject textObj = new GameObject("StatusText");
            textObj.transform.SetParent(bannerObj.transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(20, 10);
            textRect.offsetMax = new Vector2(-20, -10);

            _statusText = textObj.AddComponent<TextMeshProUGUI>();
            _statusText.fontSize = FontSizes.Body;
            _statusText.enableAutoSizing = true;
            _statusText.fontSizeMin = FontSizes.AutoMinBody;
            _statusText.fontSizeMax = _statusText.fontSize;
            _statusText.alignment = TextAlignmentOptions.Center;
            _statusText.color = Color.white;
            _statusText.fontStyle = FontStyles.Bold;
        }

        private void OnConnectionLost()
        {
            string text = AutoLocalizer.Get("net_offline");

            ShowBanner(text, _offlineColor);
        }

        private void OnConnectionRestored()
        {
            string text = AutoLocalizer.Get("net_restored");

            ShowBanner(text, _restoredColor);

            // Auto-ocultar despues de 3 segundos
            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);
            _animationCoroutine = StartCoroutine(AutoHide(3f));
        }

        /// <summary>
        /// Muestra el banner con texto y color
        /// </summary>
        private void ShowBanner(string text, Color bgColor)
        {
            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);

            _statusText.text = text;
            _backgroundImage.color = bgColor;
            _animationCoroutine = StartCoroutine(AnimateShow());
        }

        private IEnumerator AnimateShow()
        {
            _canvas.enabled = true;
            _isVisible = true;

            float duration = 0.3f;
            float elapsed = 0f;
            _canvasGroup.alpha = 0f;

            // Safe area offset para notch — en iPhone X/12/14 el banner debe bajar por el notch superior
            float topInset = Screen.height - Screen.safeArea.yMax;
            float targetY = -topInset;
            float startY = 80;

            _bannerRect.anchoredPosition = new Vector2(0, startY);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float smoothT = t * t * (3f - 2f * t); // Smoothstep

                _canvasGroup.alpha = smoothT;
                _bannerRect.anchoredPosition = new Vector2(0, Mathf.Lerp(startY, targetY, smoothT));
                yield return null;
            }

            _canvasGroup.alpha = 1f;
            _bannerRect.anchoredPosition = new Vector2(0, targetY);
        }

        private IEnumerator AnimateHide()
        {
            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                _canvasGroup.alpha = 1f - t;
                yield return null;
            }

            _canvasGroup.alpha = 0f;
            _canvas.enabled = false;
            _isVisible = false;
        }

        private IEnumerator AutoHide(float delay)
        {
            yield return new WaitForSeconds(delay);
            _animationCoroutine = StartCoroutine(AnimateHide());
        }

        private void HideImmediate()
        {
            if (_canvasGroup != null)
                _canvasGroup.alpha = 0f;
            if (_canvas != null)
                _canvas.enabled = false;
            _isVisible = false;
        }
    }
}
