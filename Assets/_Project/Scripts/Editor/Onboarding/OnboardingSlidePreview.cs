using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor tool to preview Main Onboarding steps/slides without entering Play mode.
    /// Toggles Slide1-Slide8 in SlidesContainer.
    ///
    /// Menu: DigitPark/UI Builders/Onboarding/Preview Slides
    /// </summary>
    public class OnboardingSlidePreview : EditorWindow
    {
        private bool detected = false;
        private int activeIndex = 0;
        private GameObject mainSlidesContainer;

        private static readonly string[] MAIN_LABELS =
        {
            "1. Bienvenido (Info)",
            "2. Nombre (Input)",
            "3. Juegos (Info)",
            "4. Torneos (Info)",
            "5. Recompensas (Info)",
            "6. Completado"
        };

        private static readonly Color CYAN = new Color(0f, 1f, 1f, 1f);

        [MenuItem("DigitPark/Debug/Onboarding/Preview Slides", false, 172)]
        public static void ShowWindow()
        {
            var window = GetWindow<OnboardingSlidePreview>("Onboarding Preview");
            window.minSize = new Vector2(320, 300);
            window.Show();
        }

        private void OnEnable()
        {
            DetectScene();
        }

        private void OnGUI()
        {
            GUILayout.Label("Onboarding Preview", EditorStyles.boldLabel);
            GUILayout.Space(5);

            if (!detected || mainSlidesContainer == null)
            {
                EditorGUILayout.HelpBox(
                    "No se detectó OnboardingManager.\n\n" +
                    "Abre la escena Onboarding y presiona Detectar.",
                    MessageType.Warning);
                GUILayout.Space(10);
                if (GUILayout.Button("Detectar Escena", GUILayout.Height(30)))
                    DetectScene();
                return;
            }

            EditorGUILayout.HelpBox("Escena: Main Onboarding\nAlterna slides en SlidesContainer.", MessageType.Info);
            GUILayout.Space(10);

            DrawMainOnboardingPreview();

            GUILayout.Space(15);

            EditorGUILayout.BeginHorizontal();
            GUI.enabled = activeIndex > 0;
            if (GUILayout.Button("◀ Anterior", GUILayout.Height(30)))
                ActivateIndex(activeIndex - 1);

            GUI.enabled = activeIndex < MAIN_LABELS.Length - 1;
            if (GUILayout.Button("Siguiente ▶", GUILayout.Height(30)))
                ActivateIndex(activeIndex + 1);

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            if (GUILayout.Button("Redetectar Escena"))
                DetectScene();
        }

        private void DetectScene()
        {
            detected = false;
            activeIndex = 0;

            var mainManager = Object.FindFirstObjectByType<DigitPark.Managers.OnboardingManager>();
            if (mainManager != null)
            {
                FindMainOnboardingElements();
                detected = true;
                Debug.Log("[Preview] Detectado: Main Onboarding");
                Repaint();
                return;
            }

            Debug.LogWarning("[Preview] No se encontró OnboardingManager en la escena activa.");
        }

        private void ActivateIndex(int index)
        {
            activeIndex = index;
            ActivateMainSlide(index);
        }

        private void DrawMainOnboardingPreview()
        {
            if (mainSlidesContainer == null)
            {
                FindMainOnboardingElements();
                if (mainSlidesContainer == null)
                {
                    EditorGUILayout.HelpBox("SlidesContainer no encontrado.\nEjecuta el Onboarding UIBuilder primero.", MessageType.Warning);
                    return;
                }
            }

            GUILayout.Label("Slides:", EditorStyles.boldLabel);

            for (int i = 0; i < MAIN_LABELS.Length; i++)
            {
                Transform slide = mainSlidesContainer.transform.Find($"Slide{i + 1}");
                bool isActive = slide != null && slide.gameObject.activeSelf;

                Color orig = GUI.backgroundColor;
                if (isActive)
                {
                    GUI.backgroundColor = CYAN;
                    activeIndex = i;
                }

                if (GUILayout.Button(MAIN_LABELS[i], GUILayout.Height(26)))
                    ActivateIndex(i);

                GUI.backgroundColor = orig;
            }

            GUILayout.Space(5);
            GUILayout.Label($"Activo: {MAIN_LABELS[activeIndex]}", EditorStyles.miniLabel);
        }

        private void FindMainOnboardingElements()
        {
            Canvas c = UIBuilderCanvasHelper.FindMainCanvas();
            if (c == null) return;

            Transform container = c.transform.Find("SlidesContainer");
            if (container != null)
                mainSlidesContainer = container.gameObject;
        }

        private void ActivateMainSlide(int index)
        {
            if (mainSlidesContainer == null) return;

            for (int i = 1; i <= MAIN_LABELS.Length; i++)
            {
                Transform slide = mainSlidesContainer.transform.Find($"Slide{i}");
                if (slide != null) slide.gameObject.SetActive(i == index + 1);
            }

            MarkDirty();
        }

        private void MarkDirty()
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            Repaint();
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}
