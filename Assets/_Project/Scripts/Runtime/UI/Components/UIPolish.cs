using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DigitPark.UI
{
    /// <summary>
    /// Utilidades de polish visual para UI mobile premium.
    /// Rounded corners, monospace timers, procedural sprites.
    /// </summary>
    public static class UIPolish
    {
        private static readonly Dictionary<int, Sprite> _roundedSpriteCache = new Dictionary<int, Sprite>();

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        static void ClearCacheOnPlayModeChange()
        {
            UnityEditor.EditorApplication.playModeStateChanged += (state) =>
            {
                if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                {
                    _roundedSpriteCache?.Clear();
                }
            };
        }
#endif

        /// <summary>
        /// Genera un sprite 9-slice con esquinas redondeadas.
        /// Cachea por radio para reutilizar.
        /// </summary>
        public static Sprite GetRoundedSprite(int radius = 12)
        {
            if (_roundedSpriteCache.TryGetValue(radius, out var cached))
                return cached;

            int size = Mathf.Max(radius * 4, 16);
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = 0f;
                    bool inCorner = false;

                    if (x < radius && y < radius)
                    {
                        dist = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius));
                        inCorner = true;
                    }
                    else if (x >= size - radius && y < radius)
                    {
                        dist = Vector2.Distance(new Vector2(x, y), new Vector2(size - radius - 1, radius));
                        inCorner = true;
                    }
                    else if (x < radius && y >= size - radius)
                    {
                        dist = Vector2.Distance(new Vector2(x, y), new Vector2(radius, size - radius - 1));
                        inCorner = true;
                    }
                    else if (x >= size - radius && y >= size - radius)
                    {
                        dist = Vector2.Distance(new Vector2(x, y), new Vector2(size - radius - 1, size - radius - 1));
                        inCorner = true;
                    }

                    if (inCorner)
                    {
                        float alpha = Mathf.Clamp01(radius - dist + 0.5f);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.white);
                    }
                }
            }

            tex.Apply();

            var sprite = Sprite.Create(
                tex,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                1f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(radius, radius, radius, radius)
            );

            _roundedSpriteCache[radius] = sprite;
            return sprite;
        }

        /// <summary>
        /// Aplica esquinas redondeadas a un Image usando sprite 9-slice.
        /// El color del Image se mantiene como tint.
        /// </summary>
        public static void ApplyRoundedCorners(Image image, int radius = 12)
        {
            if (image == null) return;
            image.sprite = GetRoundedSprite(radius);
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 1f;
        }

        /// <summary>
        /// Crea un borde glow redondeado detras de un elemento.
        /// Reemplaza Outline que no sigue rounded corners.
        /// </summary>
        public static GameObject CreateRoundedGlowBorder(Transform parent, Color glowColor, int radius = 14, float borderWidth = 3f)
        {
            var glowObj = new GameObject("GlowBorder");
            glowObj.transform.SetParent(parent, false);
            glowObj.transform.SetAsFirstSibling();

            var rt = glowObj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(-borderWidth, -borderWidth);
            rt.offsetMax = new Vector2(borderWidth, borderWidth);

            var img = glowObj.AddComponent<Image>();
            ApplyRoundedCorners(img, radius);
            img.color = glowColor;
            img.raycastTarget = false;

            return glowObj;
        }

        /// <summary>
        /// Formatea timer HH:MM con digitos monoespaciados (TMP rich text).
        /// </summary>
        public static string FormatTimerHHMM(int hours, int minutes)
        {
            return $"<mspace=0.55em>{hours:D2}</mspace>:<mspace=0.55em>{minutes:D2}</mspace>";
        }

        /// <summary>
        /// Formatea timer HH:MM:SS con digitos monoespaciados.
        /// </summary>
        public static string FormatTimerHHMMSS(int hours, int minutes, int seconds)
        {
            return $"<mspace=0.55em>{hours:D2}</mspace>:<mspace=0.55em>{minutes:D2}</mspace>:<mspace=0.55em>{seconds:D2}</mspace>";
        }
    }

    /// <summary>
    /// Pulsa escala y alpha de un glow para atraer atencion.
    /// Agregar al boton de Claim.
    /// </summary>
    public class PulseAnimation : MonoBehaviour
    {
        [SerializeField] private float minScale = 0.97f;
        [SerializeField] private float maxScale = 1.03f;
        [SerializeField] private float speed = 2.5f;
        [SerializeField] private float glowMinAlpha = 0.08f;
        [SerializeField] private float glowMaxAlpha = 0.35f;

        private RectTransform _rt;
        private Image _glowImage;

        /// <summary>
        /// GameObject que sirve de glow detras del boton.
        /// Su alpha pulsa sincronizado con la escala.
        /// </summary>
        public GameObject GlowTarget { get; set; }

        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
        }

        private void Update()
        {
            float t = (Mathf.Sin(Time.unscaledTime * speed) + 1f) * 0.5f;

            if (_rt != null)
            {
                float s = Mathf.Lerp(minScale, maxScale, t);
                _rt.localScale = new Vector3(s, s, 1f);
            }

            if (GlowTarget != null)
            {
                if (_glowImage == null)
                    _glowImage = GlowTarget.GetComponent<Image>();

                if (_glowImage != null)
                {
                    var c = _glowImage.color;
                    c.a = Mathf.Lerp(glowMinAlpha, glowMaxAlpha, t);
                    _glowImage.color = c;
                }
            }
        }

        public void StopPulse()
        {
            if (_rt != null)
                _rt.localScale = Vector3.one;
            if (_glowImage != null)
            {
                var c = _glowImage.color;
                c.a = 0f;
                _glowImage.color = c;
            }
            enabled = false;
        }
    }

    /// <summary>
    /// Fade gradient en la parte inferior de un ScrollRect.
    /// Indica que hay mas contenido debajo.
    /// </summary>
    public class ScrollFadeIndicator : MonoBehaviour
    {
        [SerializeField] private float fadeHeight = 60f;
        [SerializeField] private Color fadeColor = new Color(0.02f, 0.04f, 0.08f, 1f);

        private ScrollRect _scrollRect;
        private CanvasGroup _fadeGroup;

        private void Start()
        {
            _scrollRect = GetComponent<ScrollRect>();
            if (_scrollRect == null) return;
            CreateFadeOverlay();
        }

        private void CreateFadeOverlay()
        {
            var fadeObj = new GameObject("ScrollFade");
            fadeObj.transform.SetParent(transform, false);
            fadeObj.transform.SetAsLastSibling();

            var rt = fadeObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(0, fadeHeight);
            rt.anchoredPosition = Vector2.zero;

            var tex = new Texture2D(1, 32, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < 32; y++)
            {
                float a = 1f - (float)y / 31f;
                tex.SetPixel(0, y, new Color(fadeColor.r, fadeColor.g, fadeColor.b, a));
            }
            tex.Apply();

            var img = fadeObj.AddComponent<Image>();
            img.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 32), new Vector2(0.5f, 0));
            img.type = Image.Type.Simple;
            img.raycastTarget = false;

            _fadeGroup = fadeObj.AddComponent<CanvasGroup>();
            _fadeGroup.blocksRaycasts = false;
            _fadeGroup.interactable = false;
        }

        private void Update()
        {
            if (_scrollRect == null || _fadeGroup == null) return;
            _fadeGroup.alpha = _scrollRect.verticalNormalizedPosition > 0.02f ? 1f : 0f;
        }
    }

    /// <summary>
    /// Animacion de "punch" de escala al reclamar una recompensa.
    /// Escala a targetScale y vuelve a 1.0 con easing.
    /// </summary>
    public class ScalePunch : MonoBehaviour
    {
        public static void Play(GameObject target, float targetScale = 1.15f, float duration = 0.3f)
        {
            if (target == null) return;
            var punch = target.GetComponent<ScalePunch>();
            if (punch == null) punch = target.AddComponent<ScalePunch>();
            punch.StartCoroutine(punch.PunchCoroutine(targetScale, duration));
        }

        private IEnumerator PunchCoroutine(float targetScale, float duration)
        {
            var rt = GetComponent<RectTransform>();
            if (rt == null) yield break;

            float half = duration * 0.4f;
            float elapsed = 0f;

            // Scale up
            while (elapsed < half)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / half;
                float s = Mathf.Lerp(1f, targetScale, EaseOutBack(t));
                rt.localScale = new Vector3(s, s, 1f);
                yield return null;
            }

            // Scale down
            elapsed = 0f;
            float remaining = duration - half;
            while (elapsed < remaining)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / remaining;
                float s = Mathf.Lerp(targetScale, 1f, EaseOutQuad(t));
                rt.localScale = new Vector3(s, s, 1f);
                yield return null;
            }

            rt.localScale = Vector3.one;
        }

        private static float EaseOutBack(float t)
        {
            float c = 1.3f;
            return 1f + (c + 1f) * Mathf.Pow(t - 1f, 3f) + c * Mathf.Pow(t - 1f, 2f);
        }

        private static float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }
    }

    /// <summary>
    /// Shimmer diagonal que barre un card periodicamente.
    /// Efecto premium para cards especiales (Day7, etc).
    /// </summary>
    public class ShimmerEffect : MonoBehaviour
    {
        [SerializeField] private float interval = 4f;
        [SerializeField] private float sweepDuration = 0.8f;
        [SerializeField] private float shimmerWidth = 0.15f;
        [SerializeField] private Color shimmerColor = new Color(1f, 1f, 1f, 0.12f);

        private Image _shimmerImage;
        private RectTransform _shimmerRT;
        private float _timer;

        private void Start()
        {
            var shimmerObj = new GameObject("Shimmer");
            shimmerObj.transform.SetParent(transform, false);
            shimmerObj.transform.SetAsLastSibling();

            _shimmerRT = shimmerObj.AddComponent<RectTransform>();
            _shimmerRT.anchorMin = Vector2.zero;
            _shimmerRT.anchorMax = Vector2.one;
            _shimmerRT.offsetMin = Vector2.zero;
            _shimmerRT.offsetMax = Vector2.zero;

            _shimmerImage = shimmerObj.AddComponent<Image>();
            _shimmerImage.color = Color.clear;
            _shimmerImage.raycastTarget = false;

            // Crear gradiente diagonal
            var tex = CreateShimmerTexture();
            _shimmerImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            _shimmerImage.material = new Material(Shader.Find("UI/Default"));

            _timer = interval * 0.5f; // Primer shimmer mas rapido
        }

        private Texture2D CreateShimmerTexture()
        {
            int w = 64;
            int h = 64;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float diagonal = ((float)x / w + (float)y / h) * 0.5f;
                    float shimmerBand = Mathf.Exp(-Mathf.Pow((diagonal - 0.5f) / shimmerWidth, 2f));
                    var c = new Color(shimmerColor.r, shimmerColor.g, shimmerColor.b, shimmerColor.a * shimmerBand);
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            return tex;
        }

        private void Update()
        {
            _timer += Time.unscaledDeltaTime;

            if (_timer >= interval)
            {
                _timer = 0f;
                StartCoroutine(SweepCoroutine());
            }
        }

        private IEnumerator SweepCoroutine()
        {
            float elapsed = 0f;

            while (elapsed < sweepDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / sweepDuration;

                float alpha = Mathf.Sin(t * Mathf.PI) * shimmerColor.a;
                _shimmerImage.color = new Color(1f, 1f, 1f, alpha);

                // Desplazar UV simulando movimiento diagonal
                if (_shimmerRT != null)
                {
                    float offset = Mathf.Lerp(-0.5f, 1.5f, t);
                    _shimmerRT.anchorMin = new Vector2(offset - 0.3f, 0);
                    _shimmerRT.anchorMax = new Vector2(offset + 0.3f, 1);
                    _shimmerRT.offsetMin = Vector2.zero;
                    _shimmerRT.offsetMax = Vector2.zero;
                }

                yield return null;
            }

            _shimmerImage.color = Color.clear;
            _shimmerRT.anchorMin = Vector2.zero;
            _shimmerRT.anchorMax = Vector2.one;
            _shimmerRT.offsetMin = Vector2.zero;
            _shimmerRT.offsetMax = Vector2.zero;
        }
    }

    /// <summary>
    /// Animacion de monedas/gemas volando hacia el currency pill del header.
    /// Spawn particulas UI que siguen una curva bezier.
    /// </summary>
    public class CoinFlyAnimation : MonoBehaviour
    {
        private static readonly Color COIN_COLOR = new Color(1f, 0.85f, 0.3f, 1f);
        private static readonly Color GEM_COLOR = new Color(0.4f, 0.8f, 1f, 1f);

        /// <summary>
        /// Lanza monedas/gemas desde origin hacia target.
        /// </summary>
        public static void Play(RectTransform origin, RectTransform target, string rewardType = "coins",
            int count = 6, float duration = 0.6f, Sprite icon = null)
        {
            if (origin == null || target == null) return;

            var canvas = origin.GetComponentInParent<Canvas>();
            if (canvas == null) return;

            var host = new GameObject("CoinFlyHost");
            host.transform.SetParent(canvas.transform, false);
            host.transform.SetAsLastSibling();
            host.AddComponent<RectTransform>();

            var anim = host.AddComponent<CoinFlyAnimation>();
            anim.StartCoroutine(anim.FlyCoroutine(origin, target, rewardType, count, duration, icon, host));
        }

        private IEnumerator FlyCoroutine(RectTransform origin, RectTransform target, string rewardType,
            int count, float duration, Sprite icon, GameObject host)
        {
            Color color = string.Equals(rewardType, "gems", System.StringComparison.OrdinalIgnoreCase) ? GEM_COLOR : COIN_COLOR;
            float size = 24f;

            var particles = new List<(RectTransform rt, Image img, Vector2 controlPoint)>();

            Vector2 startPos = GetCanvasPosition(origin);
            Vector2 endPos = GetCanvasPosition(target);

            for (int i = 0; i < count; i++)
            {
                var particle = new GameObject($"Coin_{i}");
                particle.transform.SetParent(host.transform, false);

                var rt = particle.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(size, size);
                rt.anchoredPosition = startPos;

                var img = particle.AddComponent<Image>();
                if (icon != null)
                {
                    img.sprite = icon;
                    img.preserveAspect = true;
                }
                img.color = color;
                img.raycastTarget = false;

                // Random control point para curva bezier
                Vector2 mid = (startPos + endPos) * 0.5f;
                Vector2 randomOffset = new Vector2(
                    Random.Range(-150f, 150f),
                    Random.Range(50f, 200f)
                );

                particles.Add((rt, img, mid + randomOffset));

                // Stagger spawn
                yield return new WaitForSecondsRealtime(0.04f);
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = EaseInQuad(t);

                for (int i = 0; i < particles.Count; i++)
                {
                    var (rt, img, cp) = particles[i];
                    if (rt == null) continue;

                    // Quadratic bezier: start -> control -> end
                    float particleT = Mathf.Clamp01(eased + (i * 0.05f));
                    Vector2 pos = QuadraticBezier(startPos, cp, endPos, Mathf.Clamp01(particleT));
                    rt.anchoredPosition = pos;

                    // Fade out near end
                    float alpha = 1f - Mathf.Max(0f, (particleT - 0.7f) / 0.3f);
                    float scale = Mathf.Lerp(1f, 0.4f, particleT);
                    img.color = new Color(color.r, color.g, color.b, alpha);
                    rt.localScale = new Vector3(scale, scale, 1f);
                }

                yield return null;
            }

            // Scale punch on target
            if (target != null)
                ScalePunch.Play(target.gameObject, 1.2f, 0.25f);

            Destroy(host);
        }

        private Vector2 GetCanvasPosition(RectTransform rt)
        {
            var canvas = rt.GetComponentInParent<Canvas>();
            if (canvas == null) return Vector2.zero;

            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(
                canvas.worldCamera, rt.position);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(),
                screenPos,
                canvas.worldCamera,
                out Vector2 localPos);

            return localPos;
        }

        private static Vector2 QuadraticBezier(Vector2 a, Vector2 b, Vector2 c, float t)
        {
            float u = 1f - t;
            return u * u * a + 2f * u * t * b + t * t * c;
        }

        private static float EaseInQuad(float t)
        {
            return t * t;
        }
    }
}
