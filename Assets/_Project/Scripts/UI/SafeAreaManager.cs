using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

namespace DigitPark.UI
{
    /// <summary>
    /// Manager global que aplica Safe Area automáticamente a todas las escenas.
    /// Se crea una vez en Boot y persiste entre escenas con DontDestroyOnLoad.
    /// </summary>
    public class SafeAreaManager : MonoBehaviour
    {
        private static SafeAreaManager instance;
        public static SafeAreaManager Instance => instance;

        [Header("Configuración Global")]
        [SerializeField] private bool applyTop = true;
        [SerializeField] private bool applyBottom = true;
        [SerializeField] private bool applySides = true;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        private void Awake()
        {
            // Singleton pattern
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            // Suscribirse al evento de carga de escenas
            SceneManager.sceneLoaded += OnSceneLoaded;

            LogDebug("SafeAreaManager inicializado");
        }

        private void OnDestroy()
        {
            // Desuscribirse del evento
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>
        /// Se ejecuta cada vez que se carga una nueva escena
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            LogDebug($"Escena cargada: {scene.name}");

            // Esperar un frame para que los Canvas se inicialicen
            StartCoroutine(ApplySafeAreaDelayed(scene.name));
        }

        private IEnumerator ApplySafeAreaDelayed(string sceneName)
        {
            // Esperar al final del frame para que todos los objetos estén listos
            yield return new WaitForEndOfFrame();

            ApplySafeAreaToScene();
        }

        /// <summary>
        /// Aplica Safe Area a todos los Canvas de la escena actual
        /// </summary>
        public void ApplySafeAreaToScene()
        {
            // Buscar todos los Canvas en la escena
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);

            foreach (Canvas canvas in canvases)
            {
                // Ignorar Canvas que ya tienen SafeArea aplicado
                if (canvas.GetComponentInChildren<SafeAreaHandler>() != null)
                {
                    LogDebug($"Canvas '{canvas.name}' ya tiene SafeArea");
                    continue;
                }

                // Crear contenedor SafeArea
                ApplySafeAreaToCanvas(canvas);
            }
        }

        /// <summary>
        /// Aplica Safe Area a un Canvas específico
        /// </summary>
        private void ApplySafeAreaToCanvas(Canvas canvas)
        {
            if (canvas == null) return;

            // Crear el contenedor SafeArea
            GameObject safeAreaContainer = new GameObject("SafeAreaContainer");
            safeAreaContainer.transform.SetParent(canvas.transform, false);

            // Configurar RectTransform para ocupar todo el Canvas
            RectTransform rt = safeAreaContainer.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Agregar el handler
            SafeAreaHandler handler = safeAreaContainer.AddComponent<SafeAreaHandler>();
            handler.Configure(applyTop, applyBottom, applySides);

            // Mover el SafeAreaContainer al primer lugar en la jerarquía
            safeAreaContainer.transform.SetAsFirstSibling();

            // Mover todos los demás hijos del Canvas al SafeAreaContainer
            MoveChildrenToSafeArea(canvas.transform, safeAreaContainer.transform);

            LogDebug($"SafeArea aplicado al Canvas: {canvas.name}");
        }

        /// <summary>
        /// Mueve los hijos del Canvas al contenedor SafeArea
        /// </summary>
        private void MoveChildrenToSafeArea(Transform canvasTransform, Transform safeAreaTransform)
        {
            // Recolectar hijos a mover (excluyendo el SafeAreaContainer)
            System.Collections.Generic.List<Transform> childrenToMove = new System.Collections.Generic.List<Transform>();

            for (int i = 0; i < canvasTransform.childCount; i++)
            {
                Transform child = canvasTransform.GetChild(i);
                if (child != safeAreaTransform)
                {
                    childrenToMove.Add(child);
                }
            }

            // Mover los hijos
            foreach (Transform child in childrenToMove)
            {
                child.SetParent(safeAreaTransform, false);
            }
        }

        /// <summary>
        /// Configura las opciones de Safe Area globalmente
        /// </summary>
        public void Configure(bool top, bool bottom, bool sides)
        {
            applyTop = top;
            applyBottom = bottom;
            applySides = sides;
        }

        /// <summary>
        /// Fuerza una actualización del Safe Area en la escena actual
        /// </summary>
        public void ForceUpdate()
        {
            SafeAreaHandler[] handlers = FindObjectsByType<SafeAreaHandler>(FindObjectsSortMode.None);
            foreach (var handler in handlers)
            {
                handler.ForceUpdate();
            }
        }

        private void LogDebug(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[SafeAreaManager] {message}");
            }
        }

        /// <summary>
        /// Crea el SafeAreaManager si no existe (llamar desde Boot)
        /// </summary>
        public static SafeAreaManager Initialize()
        {
            if (instance != null) return instance;

            GameObject managerObj = new GameObject("SafeAreaManager");
            return managerObj.AddComponent<SafeAreaManager>();
        }
    }
}
