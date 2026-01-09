using UnityEngine;
using UnityEngine.UI;

namespace DigitPark.UI
{
    /// <summary>
    /// Aplica esquinas redondeadas a un Image usando un sprite generado proceduralmente
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Image))]
    public class RoundedCorners : MonoBehaviour
    {
        [Header("Configuracion de Bordes")]
        [Range(0f, 100f)]
        [Tooltip("Radio de las esquinas en pixeles")]
        [SerializeField] private float cornerRadius = 25f;

        private Image targetImage;
        private Texture2D generatedTexture;
        private Sprite generatedSprite;
        private float lastRadius = -1f;

        private void Awake()
        {
            targetImage = GetComponent<Image>();
        }

        private void Start()
        {
            ApplyRoundedCorners();
        }

        private void OnEnable()
        {
            ApplyRoundedCorners();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (targetImage == null)
                targetImage = GetComponent<Image>();

            if (!Application.isPlaying)
            {
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if (this != null)
                    {
                        ApplyRoundedCorners();
                    }
                };
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

            // Solo actualizar textura si cambió el radio
            if (!Mathf.Approximately(lastRadius, cornerRadius))
            {
                lastRadius = cornerRadius;
                CreateRoundedTexture();
            }
        }

        private void CreateRoundedTexture()
        {
            int width = 64;
            int height = 64;
            int radius = Mathf.RoundToInt(cornerRadius * (width / 100f));

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

            if (generatedSprite != null)
            {
                if (Application.isPlaying)
                    Destroy(generatedSprite);
                else
                    DestroyImmediate(generatedSprite);
            }

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

            if (x < radius && y < radius)
                return IsInsideCircle(x, y, radius, radius, radius);
            if (x >= width - radius && y < radius)
                return IsInsideCircle(x, y, width - radius - 1, radius, radius);
            if (x < radius && y >= height - radius)
                return IsInsideCircle(x, y, radius, height - radius - 1, radius);
            if (x >= width - radius && y >= height - radius)
                return IsInsideCircle(x, y, width - radius - 1, height - radius - 1, radius);

            return true;
        }

        private bool IsInsideCircle(int x, int y, int cx, int cy, int radius)
        {
            float dx = x - cx;
            float dy = y - cy;
            return (dx * dx + dy * dy) <= (radius * radius);
        }

        private void OnDestroy()
        {
            if (generatedTexture != null)
            {
                if (Application.isPlaying)
                    Destroy(generatedTexture);
                else
                    DestroyImmediate(generatedTexture);
            }

            if (generatedSprite != null)
            {
                if (Application.isPlaying)
                    Destroy(generatedSprite);
                else
                    DestroyImmediate(generatedSprite);
            }
        }

        public void SetCornerRadius(float radius)
        {
            cornerRadius = radius;
            lastRadius = -1f;
            ApplyRoundedCorners();
        }
    }
}
