#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace DigitPark.Editor
{
    /// <summary>
    /// Ajusta ScrollViews en Scores, Tournaments y SearchPlayers:
    /// - Left y Right de 20px en el ScrollView
    /// - Oculta la barra vertical (no se ve)
    /// - Solo scroll vertical (no horizontal)
    /// </summary>
    public class ScrollViewAdjuster : EditorWindow
    {
        private const float SCROLLVIEW_LEFT = 20f;
        private const float SCROLLVIEW_RIGHT = 20f;

        [MenuItem("DigitPark/UI/Adjust ScrollViews (Ocultar Scrollbar)")]
        public static void AdjustScrollViews()
        {
            string[] scenes = new string[]
            {
                "Assets/_Project/Scenes/Scores.unity",
                "Assets/_Project/Scenes/Tournaments.unity",
                "Assets/_Project/Scenes/SearchPlayers.unity"
            };

            int totalAdjusted = 0;

            foreach (string scenePath in scenes)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

                Debug.Log($"[ScrollView] Procesando: {sceneName}");

                int adjusted = ProcessScrollRects();
                totalAdjusted += adjusted;

                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[ScrollView] {sceneName}: {adjusted} ScrollViews ajustados");
            }

            EditorUtility.DisplayDialog("ScrollViews Ajustados",
                $"Total: {totalAdjusted} ScrollViews\n\n" +
                "Cambios:\n" +
                $"- ScrollView Left: {SCROLLVIEW_LEFT}px\n" +
                $"- ScrollView Right: {SCROLLVIEW_RIGHT}px\n" +
                "- Barra vertical: OCULTA\n" +
                "- Scroll horizontal: DESACTIVADO\n" +
                "- Scroll vertical: ACTIVADO",
                "OK");
        }

        private static int ProcessScrollRects()
        {
            int count = 0;
            var scrollRects = Object.FindObjectsOfType<ScrollRect>(true);

            foreach (var scrollRect in scrollRects)
            {
                Debug.Log($"  [ScrollRect] {scrollRect.gameObject.name}");

                RectTransform rt = scrollRect.GetComponent<RectTransform>();
                if (rt != null)
                {
                    // Ajustar Left y Right del ScrollView
                    // offsetMin.x = Left, offsetMax.x = -Right
                    float currentTop = -rt.offsetMax.y;  // Mantener Top actual
                    float currentBottom = rt.offsetMin.y; // Mantener Bottom actual

                    rt.offsetMin = new Vector2(SCROLLVIEW_LEFT, currentBottom);  // Left, Bottom
                    rt.offsetMax = new Vector2(-SCROLLVIEW_RIGHT, -currentTop);   // -Right, -Top

                    Debug.Log($"    - Left={SCROLLVIEW_LEFT}, Right={SCROLLVIEW_RIGHT}");
                    EditorUtility.SetDirty(rt);
                }

                // Solo scroll vertical (no horizontal)
                scrollRect.horizontal = false;
                scrollRect.vertical = true;
                Debug.Log($"    - Scroll: vertical=SI, horizontal=NO");

                // Ocultar scrollbar vertical (la barra visual)
                if (scrollRect.verticalScrollbar != null)
                {
                    scrollRect.verticalScrollbar.gameObject.SetActive(false);
                    // Quitar la referencia para que no interfiera
                    scrollRect.verticalScrollbar = null;
                    Debug.Log($"    - Barra vertical OCULTA y referencia removida");
                }

                // Asegurar que el scroll funcione bien
                scrollRect.movementType = ScrollRect.MovementType.Elastic;
                scrollRect.elasticity = 0.1f;
                scrollRect.inertia = true;
                scrollRect.decelerationRate = 0.135f;
                scrollRect.scrollSensitivity = 1f;

                // Buscar y ocultar cualquier scrollbar hijo
                foreach (Transform child in scrollRect.transform)
                {
                    if (child.gameObject.name.ToLower().Contains("scrollbar"))
                    {
                        child.gameObject.SetActive(false);
                        Debug.Log($"    - {child.gameObject.name} OCULTO");
                    }
                }

                EditorUtility.SetDirty(scrollRect);
                count++;
            }

            return count;
        }
    }
}
#endif
