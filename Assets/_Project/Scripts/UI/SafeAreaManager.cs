using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace DigitPark.UI
{
    public class SafeAreaManager : MonoBehaviour
    {
        private static SafeAreaManager instance;
        public static SafeAreaManager Instance => instance;

        [Header("Configuracion Global")]
        [SerializeField] private bool applyTop = true;
        [SerializeField] private bool applyBottom = true;
        [SerializeField] private bool applySides = true;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            LogDebug("SafeAreaManager inicializado");
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            LogDebug("Escena cargada: " + scene.name);
            StartCoroutine(ApplySafeAreaDelayed());
        }

        private IEnumerator ApplySafeAreaDelayed()
        {
            yield return null;
            yield return null;
            yield return new WaitForEndOfFrame();
            ApplySafeAreaToScene();
        }

        public void ApplySafeAreaToScene()
        {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            int processedCount = 0;

            foreach (Canvas canvas in canvases)
            {
                if (canvas.renderMode == RenderMode.WorldSpace) continue;

                Transform existingSafeArea = canvas.transform.Find("SafeArea");
                if (existingSafeArea == null)
                    existingSafeArea = canvas.transform.Find("SafeAreaContainer");

                if (existingSafeArea != null)
                {
                    SafeAreaHandler handler = existingSafeArea.GetComponent<SafeAreaHandler>();
                    if (handler == null)
                        handler = existingSafeArea.gameObject.AddComponent<SafeAreaHandler>();
                    handler.Configure(applyTop, applyBottom, applySides);
                    processedCount++;
                }
                else
                {
                    if (canvas.GetComponentInChildren<SafeAreaHandler>() != null) continue;
                    CreateSafeAreaForCanvas(canvas);
                    processedCount++;
                }
            }
            LogDebug("SafeArea procesado en " + processedCount + " Canvas");
        }

        private void CreateSafeAreaForCanvas(Canvas canvas)
        {
            GameObject safeAreaObj = new GameObject("SafeArea");
            RectTransform safeAreaRT = safeAreaObj.AddComponent<RectTransform>();

            safeAreaObj.transform.SetParent(canvas.transform, false);
            safeAreaObj.transform.SetAsFirstSibling();

            safeAreaRT.anchorMin = Vector2.zero;
            safeAreaRT.anchorMax = Vector2.one;
            safeAreaRT.pivot = new Vector2(0.5f, 0.5f);
            safeAreaRT.offsetMin = Vector2.zero;
            safeAreaRT.offsetMax = Vector2.zero;

            SafeAreaHandler handler = safeAreaObj.AddComponent<SafeAreaHandler>();
            handler.Configure(applyTop, applyBottom, applySides);

            var childrenToMove = new System.Collections.Generic.List<Transform>();
            for (int i = 0; i < canvas.transform.childCount; i++)
            {
                Transform child = canvas.transform.GetChild(i);
                if (child != safeAreaObj.transform) childrenToMove.Add(child);
            }

            foreach (Transform child in childrenToMove)
            {
                RectTransform childRT = child.GetComponent<RectTransform>();
                if (childRT != null)
                {
                    Vector2 anchorMin = childRT.anchorMin;
                    Vector2 anchorMax = childRT.anchorMax;
                    Vector2 anchoredPosition = childRT.anchoredPosition;
                    Vector2 sizeDelta = childRT.sizeDelta;
                    Vector2 pivot = childRT.pivot;
                    Vector3 localScale = childRT.localScale;

                    child.SetParent(safeAreaRT, false);

                    childRT.anchorMin = anchorMin;
                    childRT.anchorMax = anchorMax;
                    childRT.anchoredPosition = anchoredPosition;
                    childRT.sizeDelta = sizeDelta;
                    childRT.pivot = pivot;
                    childRT.localScale = localScale;
                }
                else
                {
                    child.SetParent(safeAreaRT, false);
                }
            }
            LogDebug("SafeArea creado en Canvas: " + canvas.name);
        }

        public void Configure(bool top, bool bottom, bool sides)
        {
            applyTop = top;
            applyBottom = bottom;
            applySides = sides;
        }

        public void ForceUpdate()
        {
            SafeAreaHandler[] handlers = FindObjectsByType<SafeAreaHandler>(FindObjectsSortMode.None);
            foreach (var handler in handlers) handler.ForceUpdate();
        }

        private void LogDebug(string message)
        {
            if (showDebugLogs) Debug.Log("[SafeAreaManager] " + message);
        }

        public static SafeAreaManager Initialize()
        {
            if (instance != null) return instance;
            GameObject managerObj = new GameObject("SafeAreaManager");
            return managerObj.AddComponent<SafeAreaManager>();
        }
    }
}
