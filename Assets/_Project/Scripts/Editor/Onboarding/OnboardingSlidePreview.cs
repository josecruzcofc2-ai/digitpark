using UnityEngine;
using UnityEditor;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor tool to preview Onboarding slides without entering Play mode
    /// Shows buttons to switch between slides in the Scene view
    /// </summary>
    public class OnboardingSlidePreview : EditorWindow
    {
        private GameObject slidesContainer;

        [MenuItem("DigitPark/UI Builders/Onboarding/Preview Slides", false, 301)]
        public static void ShowWindow()
        {
            OnboardingSlidePreview window = GetWindow<OnboardingSlidePreview>("Slide Preview");
            window.minSize = new Vector2(300, 150);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Onboarding Slide Preview", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // Find SlidesContainer
            if (slidesContainer == null)
            {
                FindSlidesContainer();
            }

            if (slidesContainer == null)
            {
                EditorGUILayout.HelpBox(
                    "No se encontró SlidesContainer.\n\n" +
                    "1. Asegúrate de haber ejecutado el OnboardingUIBuilder primero.\n" +
                    "2. Guarda la escena (Ctrl+S).\n" +
                    "3. Abre esta ventana de nuevo.",
                    MessageType.Warning);

                if (GUILayout.Button("Buscar SlidesContainer"))
                {
                    FindSlidesContainer();
                }

                return;
            }

            // Show slide buttons
            EditorGUILayout.HelpBox(
                "Haz clic en un botón para activar ese slide en el editor.\n" +
                "Los cambios se ven inmediatamente en la Scene view.",
                MessageType.Info);

            GUILayout.Space(10);
            GUILayout.Label("Selecciona un Slide:", EditorStyles.label);
            GUILayout.Space(5);

            // Buttons in a row
            EditorGUILayout.BeginHorizontal();
            for (int i = 1; i <= 5; i++)
            {
                Color originalColor = GUI.backgroundColor;

                // Highlight active slide
                Transform slide = slidesContainer.transform.Find($"Slide{i}");
                if (slide != null && slide.gameObject.activeSelf)
                {
                    GUI.backgroundColor = Color.cyan;
                }

                if (GUILayout.Button($"Slide {i}", GUILayout.Height(40), GUILayout.Width(55)))
                {
                    ActivateSlide(i);
                }

                GUI.backgroundColor = originalColor;
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Show current active slide
            string activeSlide = GetActiveSlide();
            if (!string.IsNullOrEmpty(activeSlide))
            {
                EditorGUILayout.HelpBox($"Slide activo: {activeSlide}", MessageType.None);
            }

            GUILayout.Space(10);

            // Refresh button
            if (GUILayout.Button("Actualizar SlidesContainer"))
            {
                FindSlidesContainer();
            }
        }

        private void FindSlidesContainer()
        {
            // Try to find SlidesContainer in the scene
            Canvas canvas = GameObject.FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                Transform safeArea = canvas.transform.Find("SafeArea");
                if (safeArea != null)
                {
                    Transform container = safeArea.Find("SlidesContainer");
                    if (container != null)
                    {
                        slidesContainer = container.gameObject;
                        Debug.Log("✅ SlidesContainer encontrado");
                        return;
                    }
                }
            }

            // Alternative: search by name
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name == "SlidesContainer")
                {
                    slidesContainer = obj;
                    Debug.Log("✅ SlidesContainer encontrado");
                    return;
                }
            }

            Debug.LogWarning("⚠️ SlidesContainer no encontrado. Ejecuta el OnboardingUIBuilder primero.");
        }

        private void ActivateSlide(int slideNumber)
        {
            if (slidesContainer == null)
            {
                Debug.LogError("❌ SlidesContainer no encontrado");
                return;
            }

            // Deactivate all slides
            for (int i = 1; i <= 5; i++)
            {
                Transform slide = slidesContainer.transform.Find($"Slide{i}");
                if (slide != null)
                {
                    slide.gameObject.SetActive(false);
                }
            }

            // Activate selected slide
            Transform targetSlide = slidesContainer.transform.Find($"Slide{slideNumber}");
            if (targetSlide != null)
            {
                targetSlide.gameObject.SetActive(true);

                // Mark scene as dirty to save changes
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

                Debug.Log($"✅ Slide {slideNumber} activado");

                // Refresh the window
                Repaint();
            }
            else
            {
                Debug.LogError($"❌ Slide{slideNumber} no encontrado");
            }
        }

        private string GetActiveSlide()
        {
            if (slidesContainer == null) return null;

            for (int i = 1; i <= 5; i++)
            {
                Transform slide = slidesContainer.transform.Find($"Slide{i}");
                if (slide != null && slide.gameObject.activeSelf)
                {
                    return $"Slide {i}";
                }
            }

            return "Ninguno";
        }

        private void OnInspectorUpdate()
        {
            // Refresh the window periodically
            Repaint();
        }
    }
}
