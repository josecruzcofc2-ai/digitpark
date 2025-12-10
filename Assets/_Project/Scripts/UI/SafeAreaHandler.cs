using UnityEngine;

namespace DigitPark.UI
{
    /// <summary>
    /// Ajusta un RectTransform para respetar el Safe Area del dispositivo.
    /// Esto evita que la UI quede tapada por notches, cámaras frontales o barras de navegación.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaHandler : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect lastSafeArea = Rect.zero;
        private Vector2Int lastScreenSize = Vector2Int.zero;
        private ScreenOrientation lastOrientation = ScreenOrientation.AutoRotation;

        [Tooltip("Aplicar safe area en la parte superior (notch, cámara frontal)")]
        [SerializeField] private bool applyTop = true;

        [Tooltip("Aplicar safe area en la parte inferior (barra de gestos, botones virtuales)")]
        [SerializeField] private bool applyBottom = true;

        [Tooltip("Aplicar safe area en los lados (cámaras laterales en algunos dispositivos)")]
        [SerializeField] private bool applySides = true;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void Update()
        {
            // Verificar si cambió el safe area, tamaño de pantalla u orientación
            if (lastSafeArea != Screen.safeArea ||
                lastScreenSize.x != Screen.width ||
                lastScreenSize.y != Screen.height ||
                lastOrientation != Screen.orientation)
            {
                ApplySafeArea();
            }
        }

        /// <summary>
        /// Aplica el Safe Area al RectTransform
        /// </summary>
        public void ApplySafeArea()
        {
            if (rectTransform == null) return;

            Rect safeArea = Screen.safeArea;

            // Guardar valores actuales para detectar cambios
            lastSafeArea = safeArea;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            lastOrientation = Screen.orientation;

            // Evitar división por cero
            if (Screen.width <= 0 || Screen.height <= 0) return;

            // Calcular los anchors basándose en el safe area
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            // Normalizar a valores 0-1
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            // Aplicar selectivamente según configuración
            if (!applyTop)
            {
                anchorMax.y = 1f;
            }
            if (!applyBottom)
            {
                anchorMin.y = 0f;
            }
            if (!applySides)
            {
                anchorMin.x = 0f;
                anchorMax.x = 1f;
            }

            // Aplicar al RectTransform
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            Debug.Log($"[SafeArea] Aplicado - SafeArea: {safeArea}, Screen: {Screen.width}x{Screen.height}, " +
                     $"Anchors: ({anchorMin.x:F2}, {anchorMin.y:F2}) - ({anchorMax.x:F2}, {anchorMax.y:F2})");
        }

        /// <summary>
        /// Configura qué bordes deben respetar el safe area
        /// </summary>
        public void Configure(bool top = true, bool bottom = true, bool sides = true)
        {
            applyTop = top;
            applyBottom = bottom;
            applySides = sides;
            ApplySafeArea();
        }

        /// <summary>
        /// Fuerza una actualización del safe area
        /// </summary>
        public void ForceUpdate()
        {
            lastSafeArea = Rect.zero; // Forzar que detecte cambio
            ApplySafeArea();
        }
    }
}
