using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor tool to preview Onboarding steps/slides without entering Play mode.
    /// Auto-detects scene type:
    ///   - Main Onboarding (8 slides): toggles Slide1-Slide8 in SlidesContainer
    ///   - Cash Battle Onboarding (5 slides): toggles Slide1-Slide5
    ///
    /// Menu: DigitPark/UI Builders/Onboarding/Preview Slides
    /// </summary>
    public class OnboardingSlidePreview : EditorWindow
    {
        private enum SceneMode { None, MainOnboarding, CashBattleOnboarding }

        private SceneMode currentMode = SceneMode.None;
        private int activeIndex = 0;

        // Cash Battle refs
        private GameObject cashSlidesContainer;

        // Main Onboarding refs
        private GameObject mainSlidesContainer;

        // Step data for Main Onboarding preview
        private static readonly string[] MAIN_LABELS =
        {
            "1. Bienvenido (Info)",
            "2. Nombre (Input)",
            "3. Avatar (Selecci\u00F3n)",
            "4. Juegos (Info)",
            "5. CashBattle (Info)",
            "6. Torneos (Info)",
            "7. Recompensas (Info)",
            "8. Completado"
        };

        // Cash Battle slide labels
        private static readonly string[] CASH_LABELS =
        {
            "1. Bienvenida",
            "2. Verificaci\u00F3n 18+",
            "3. Dep\u00F3sito",
            "4. Juega y Apuesta",
            "5. Gana y Retira"
        };

        // Colors
        private static readonly Color CYAN = new Color(0f, 1f, 1f, 1f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);

        [MenuItem("DigitPark/Debug/Onboarding/Preview Slides", false, 172)]
        public static void ShowWindow()
        {
            var window = GetWindow<OnboardingSlidePreview>("Onboarding Preview");
            window.minSize = new Vector2(320, 350);
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

            switch (currentMode)
            {
                case SceneMode.MainOnboarding:
                    EditorGUILayout.HelpBox("Escena: Main Onboarding (8 slides)\nAlterna slides independientes en SlidesContainer.", MessageType.Info);
                    break;
                case SceneMode.CashBattleOnboarding:
                    EditorGUILayout.HelpBox("Escena: Cash Battle Onboarding (5 slides)\nMuestra/oculta slides individuales.", MessageType.Info);
                    break;
                default:
                    EditorGUILayout.HelpBox(
                        "No se detect\u00F3 ning\u00FAn manager de onboarding.\n\n" +
                        "Abre una escena de Onboarding o\nCashBattleOnboarding y presiona Detectar.",
                        MessageType.Warning);
                    GUILayout.Space(10);
                    if (GUILayout.Button("Detectar Escena", GUILayout.Height(30)))
                        DetectScene();
                    return;
            }

            GUILayout.Space(10);

            if (currentMode == SceneMode.CashBattleOnboarding)
                DrawCashBattlePreview();
            else
                DrawMainOnboardingPreview();

            GUILayout.Space(15);

            // Navigation arrows
            EditorGUILayout.BeginHorizontal();
            int maxIndex = currentMode == SceneMode.CashBattleOnboarding ? 4 : 7;

            GUI.enabled = activeIndex > 0;
            if (GUILayout.Button("\u25C0 Anterior", GUILayout.Height(30)))
                ActivateIndex(activeIndex - 1);

            GUI.enabled = activeIndex < maxIndex;
            if (GUILayout.Button("Siguiente \u25B6", GUILayout.Height(30)))
                ActivateIndex(activeIndex + 1);

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            if (GUILayout.Button("Redetectar Escena"))
                DetectScene();
        }

        private void DetectScene()
        {
            currentMode = SceneMode.None;
            activeIndex = 0;

            var cashManager = Object.FindFirstObjectByType<DigitPark.Managers.CashBattleOnboardingManager>();
            if (cashManager != null)
            {
                currentMode = SceneMode.CashBattleOnboarding;
                FindCashBattleElements();
                Debug.Log("[Preview] Detectado: Cash Battle Onboarding");
                Repaint();
                return;
            }

            var mainManager = Object.FindFirstObjectByType<DigitPark.Managers.OnboardingManager>();
            if (mainManager != null)
            {
                currentMode = SceneMode.MainOnboarding;
                FindMainOnboardingElements();
                Debug.Log("[Preview] Detectado: Main Onboarding");
                Repaint();
                return;
            }

            Debug.LogWarning("[Preview] No se encontr\u00F3 ning\u00FAn manager de onboarding en la escena activa.");
        }

        private void ActivateIndex(int index)
        {
            activeIndex = index;
            if (currentMode == SceneMode.CashBattleOnboarding)
                ActivateCashSlide(index);
            else
                ActivateMainSlide(index);
        }

        #region Cash Battle Preview

        private void DrawCashBattlePreview()
        {
            if (cashSlidesContainer == null)
            {
                FindCashBattleElements();
                if (cashSlidesContainer == null)
                {
                    EditorGUILayout.HelpBox("SlidesContainer no encontrado.\nEjecuta el CashBattle UIBuilder primero.", MessageType.Warning);
                    return;
                }
            }

            GUILayout.Label("Slides:", EditorStyles.boldLabel);

            for (int i = 0; i < CASH_LABELS.Length; i++)
            {
                Transform slide = cashSlidesContainer.transform.Find($"Slide{i + 1}");
                bool isActive = slide != null && slide.gameObject.activeSelf;

                Color orig = GUI.backgroundColor;
                if (isActive)
                {
                    GUI.backgroundColor = GOLD;
                    activeIndex = i;
                }

                if (GUILayout.Button(CASH_LABELS[i], GUILayout.Height(30)))
                    ActivateIndex(i);

                GUI.backgroundColor = orig;
            }

            GUILayout.Space(5);
            GUILayout.Label($"Activo: {CASH_LABELS[activeIndex]}", EditorStyles.miniLabel);
        }

        private void FindCashBattleElements()
        {
            Canvas c = UIBuilderCanvasHelper.FindMainCanvas();
            if (c == null) return;

            Transform container = c.transform.Find("SlidesContainer");
            if (container != null)
            {
                cashSlidesContainer = container.gameObject;
                return;
            }

            Transform safeArea = c.transform.Find("SafeArea");
            if (safeArea != null)
            {
                container = safeArea.Find("SlidesContainer");
                if (container != null) cashSlidesContainer = container.gameObject;
            }
        }

        private void ActivateCashSlide(int index)
        {
            if (cashSlidesContainer == null) return;

            for (int i = 1; i <= 5; i++)
            {
                Transform slide = cashSlidesContainer.transform.Find($"Slide{i}");
                if (slide != null) slide.gameObject.SetActive(i == index + 1);
            }

            MarkDirty();
        }

        #endregion

        #region Main Onboarding Preview

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

            for (int i = 1; i <= 8; i++)
            {
                Transform slide = mainSlidesContainer.transform.Find($"Slide{i}");
                if (slide != null) slide.gameObject.SetActive(i == index + 1);
            }

            MarkDirty();
        }

        #endregion

        #region Helpers

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

        #endregion
    }
}
