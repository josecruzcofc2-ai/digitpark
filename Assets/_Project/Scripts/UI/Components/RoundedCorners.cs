using UnityEngine;
using UnityEngine.UI;

namespace DigitPark.UI
{
    /// <summary>
    /// Aplica esquinas redondeadas a un Image usando un sprite generado proceduralmente
    /// Agrega este componente a cualquier GameObject con Image para redondear sus esquinas
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Image))]
    public class RoundedCorners : MonoBehaviour
    {
        [Header("Configuración de Bordes")]
        [Range(0f, 100f)]
        [Tooltip("Radio de las esquinas en píxeles")]
        [SerializeField] private float cornerRadius = 25f;

        [Header("Barra inferior (opcional)")]
        [SerializeField] private bool showBottomBar = false;
        [SerializeField] private float bottomBarHeight = 8f;
        [SerializeField] private Color bottomBarColor = Color.black;

        private Image targetImage;
        private Texture2D generatedTexture;
        private Sprite generatedSprite;
        private GameObject bottomBarObject;

        private float lastRadius = -1f;
        private bool lastShowBar = false;
        private bool initialized = false;

        private void Awake()
        {
            targetImage = GetComponent<Image>();
        }

        private void Start()
        {
            if (!initialized)
            {
                ApplyRoundedCorners();
                initialized = true;
            }
        }

        private void OnEnable()
        {
            if (initialized)
            {
                ApplyRoundedCorners();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (targetImage == null)
                targetImage = GetComponent<Image>();

            // Solo aplicar en editor cuando no está en play mode
            if (!Application.isPlaying)
            {
                ApplyRoundedCorners();
            }
        }
#endif

        public void ApplyRoundedCorners()
        {
            if (targetImage == null)
            {
                targetImage = GetComponent<Image>();
                if (targetImage == null) return;
            }

            // Evitar actualizaciones innecesarias
            if (Mathf.Approximately(lastRadius, cornerRadius) && lastShowBar == showBottomBar)
            {
                return;
            }

            lastRadius = cornerRadius;
            lastShowBar = showBottomBar;

            // Crear textura redondeada
            CreateRoundedTexture();

            // Manejar barra inferior
            HandleBottomBar();
        }

        private void CreateRoundedTexture()
        {
            int width = 64;
            int height = 64;
            int radius = Mathf.RoundToInt(cornerRadius * (width / 100f));

            // Limpiar textura anterior
            if (generatedTexture != null)
            {
                if (Application.isPlaying)
                    Destroy(generatedTexture);
                else
                    DestroyImmediate(generatedTexture);
            }

            generatedTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            generatedTexture.filterMode = FilterMode.Bilinear;
            generatedTexture.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            Color white = Color.white;
            Color transparent = new Color(1, 1, 1, 0);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool inside = IsInsideRoundedRect(x, y, width, height, radius);
                    pixels[y * width + x] = inside ? white : transparent;
                }
            }

            generatedTexture.SetPixels(pixels);
            generatedTexture.Apply();

            // Limpiar sprite anterior
            if (generatedSprite != null)
            {
                if (Application.isPlaying)
                    Destroy(generatedSprite);
                else
                    DestroyImmediate(generatedSprite);
            }

            // Border para 9-slice
            float border = Mathf.Max(radius + 1, 2);
            generatedSprite = Sprite.Create(
                generatedTexture,
                new Rect(0, 0, width, height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(border, border, border, border)
            );

            targetImage.sprite = generatedSprite;
            targetImage.type = Image.Type.Sliced;
        }

        private bool IsInsideRoundedRect(int x, int y, int width, int height, int radius)
        {
            if (radius <= 0) return true;

            // Esquina inferior izquierda
            if (x < radius && y < radius)
            {
                return IsInsideCircle(x, y, radius, radius, radius);
            }
            // Esquina inferior derecha
            if (x >= width - radius && y < radius)
            {
                return IsInsideCircle(x, y, width - radius - 1, radius, radius);
            }
            // Esquina superior izquierda
            if (x < radius && y >= height - radius)
            {
                return IsInsideCircle(x, y, radius, height - radius - 1, radius);
            }
            // Esquina superior derecha
            if (x >= width - radius && y >= height - radius)
            {
                return IsInsideCircle(x, y, width - radius - 1, height - radius - 1, radius);
            }

            return true;
        }

        private bool IsInsideCircle(int x, int y, int cx, int cy, int radius)
        {
            float dx = x - cx;
            float dy = y - cy;
            return (dx * dx + dy * dy) <= (radius * radius);
        }

        private void HandleBottomBar()
        {
            if (showBottomBar)
            {
                if (bottomBarObject == null)
                {
                    bottomBarObject = new GameObject("BottomBar");
                    bottomBarObject.transform.SetParent(transform, false);

                    // Asegurar que esté al frente (último hijo)
                    bottomBarObject.transform.SetAsLastSibling();

                    var rt = bottomBarObject.AddComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0, 0);
                    rt.anchorMax = new Vector2(1, 0);
                    rt.pivot = new Vector2(0.5f, 0);
                    rt.anchoredPosition = Vector2.zero;
                    rt.sizeDelta = new Vector2(0, bottomBarHeight);

                    var barImage = bottomBarObject.AddComponent<Image>();
                    barImage.color = bottomBarColor;
                    barImage.raycastTarget = false; // MUY IMPORTANTE: No bloquear clicks
                }
                else
                {
                    var rt = bottomBarObject.GetComponent<RectTransform>();
                    if (rt != null)
                        rt.sizeDelta = new Vector2(0, bottomBarHeight);

                    var barImage = bottomBarObject.GetComponent<Image>();
                    if (barImage != null)
                    {
                        barImage.color = bottomBarColor;
                        barImage.raycastTarget = false;
                    }
                }

                bottomBarObject.SetActive(true);
            }
            else if (bottomBarObject != null)
            {
                bottomBarObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            CleanupResources();
        }

        private void CleanupResources()
        {
            if (generatedTexture != null)
            {
                if (Application.isPlaying)
                    Destroy(generatedTexture);
                else
                    DestroyImmediate(generatedTexture);
                generatedTexture = null;
            }

            if (generatedSprite != null)
            {
                if (Application.isPlaying)
                    Destroy(generatedSprite);
                else
                    DestroyImmediate(generatedSprite);
                generatedSprite = null;
            }
        }

        /// <summary>
        /// Configura los bordes redondeados desde código
        /// </summary>
        public void SetCornerRadius(float radius)
        {
            cornerRadius = radius;
            lastRadius = -1f; // Forzar actualización
            ApplyRoundedCorners();
        }

        /// <summary>
        /// Configura la barra inferior desde código
        /// </summary>
        public void SetBottomBar(bool show, float height = 8f, Color? color = null)
        {
            showBottomBar = show;
            bottomBarHeight = height;
            if (color.HasValue)
                bottomBarColor = color.Value;
            lastShowBar = !show; // Forzar actualización
            ApplyRoundedCorners();
        }
    }
}
