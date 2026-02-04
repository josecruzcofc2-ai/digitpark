using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor tool to preview Onboarding steps/slides without entering Play mode.
    /// Auto-detects scene type:
    ///   - Main Onboarding (8 steps): toggles panels + updates title/description
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
        private GameObject slidesContainer;

        // Main Onboarding refs
        private Canvas canvas;
        private GameObject nameInputPanel;
        private GameObject avatarSelectionPanel;
        private GameObject completionPanel;
        private GameObject stepImageObj;
        private GameObject iconGlowObj;
        private GameObject titleTextObj;
        private GameObject descTextObj;
        private TextMeshProUGUI titleTMP;
        private TextMeshProUGUI descTMP;

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

        private static readonly string[] MAIN_TITLES =
        {
            "\u00A1Bienvenido a DigitPark!",
            "\u00BFC\u00F3mo te llamas?",
            "Elige tu Avatar",
            "Juegos Cognitivos",
            "CashBattle",
            "Torneos",
            "Recompensas Diarias",
            "\u00A1Est\u00E1s Listo!"
        };

        private static readonly string[] MAIN_DESCS =
        {
            "Tu destino para juegos mentales, competencias y diversi\u00F3n.",
            "Elige un nombre para tu perfil.",
            "Selecciona c\u00F3mo quieres que te vean otros jugadores.",
            "Entrena tu mente con 6 minijuegos dise\u00F1ados para mejorar memoria, reflejos, matem\u00E1ticas y m\u00E1s.",
            "\u00BFListo para el desaf\u00EDo? Compite 1v1 contra otros jugadores por premios reales.",
            "Participa en torneos con decenas de jugadores. \u00A1El premio mayor est\u00E1 en juego!",
            "Vuelve cada d\u00EDa para obtener monedas, gemas y m\u00E1s recompensas.",
            "Has completado el tutorial. \u00A1Es hora de jugar!"
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

        [MenuItem("DigitPark/UI Builders/Onboarding/Preview Slides", false, 301)]
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

            // Mode indicator
            switch (currentMode)
            {
                case SceneMode.MainOnboarding:
                    EditorGUILayout.HelpBox("Escena: Main Onboarding (8 pasos)\nAlterna paneles y texto por paso.", MessageType.Info);
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
                ActivateMainStep(index);
        }

        #region Cash Battle Preview

        private void DrawCashBattlePreview()
        {
            if (slidesContainer == null)
            {
                FindCashBattleElements();
                if (slidesContainer == null)
                {
                    EditorGUILayout.HelpBox("SlidesContainer no encontrado.\nEjecuta el CashBattle UIBuilder primero.", MessageType.Warning);
                    return;
                }
            }

            GUILayout.Label("Slides:", EditorStyles.boldLabel);

            for (int i = 0; i < CASH_LABELS.Length; i++)
            {
                Transform slide = slidesContainer.transform.Find($"Slide{i + 1}");
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
            Canvas c = Object.FindFirstObjectByType<Canvas>();
            if (c == null) return;

            // New structure: Canvas -> SlidesContainer
            Transform container = c.transform.Find("SlidesContainer");
            if (container != null)
            {
                slidesContainer = container.gameObject;
                return;
            }

            // Old structure: Canvas -> SafeArea -> SlidesContainer
            Transform safeArea = c.transform.Find("SafeArea");
            if (safeArea != null)
            {
                container = safeArea.Find("SlidesContainer");
                if (container != null) slidesContainer = container.gameObject;
            }
        }

        private void ActivateCashSlide(int index)
        {
            if (slidesContainer == null) return;

            for (int i = 1; i <= 5; i++)
            {
                Transform slide = slidesContainer.transform.Find($"Slide{i}");
                if (slide != null) slide.gameObject.SetActive(i == index + 1);
            }

            MarkDirty();
        }

        #endregion

        #region Main Onboarding Preview

        private void DrawMainOnboardingPreview()
        {
            if (canvas == null)
            {
                FindMainOnboardingElements();
                if (canvas == null)
                {
                    EditorGUILayout.HelpBox("Canvas no encontrado.\nEjecuta el Onboarding UIBuilder primero.", MessageType.Warning);
                    return;
                }
            }

            GUILayout.Label("Pasos:", EditorStyles.boldLabel);

            for (int i = 0; i < MAIN_LABELS.Length; i++)
            {
                Color orig = GUI.backgroundColor;
                if (i == activeIndex) GUI.backgroundColor = CYAN;

                if (GUILayout.Button(MAIN_LABELS[i], GUILayout.Height(26)))
                    ActivateIndex(i);

                GUI.backgroundColor = orig;
            }

            GUILayout.Space(5);
            GUILayout.Label($"Activo: {MAIN_LABELS[activeIndex]}", EditorStyles.miniLabel);
        }

        private void FindMainOnboardingElements()
        {
            canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            Transform r = canvas.transform;

            nameInputPanel = FindChild(r, "NameInputPanel");
            avatarSelectionPanel = FindChild(r, "AvatarSelectionPanel");
            completionPanel = FindChild(r, "CompletionPanel");
            stepImageObj = FindChild(r, "StepImage");
            iconGlowObj = FindChild(r, "IconGlow");
            titleTextObj = FindChild(r, "TitleText");
            descTextObj = FindChild(r, "DescriptionText");

            if (titleTextObj != null) titleTMP = titleTextObj.GetComponent<TextMeshProUGUI>();
            if (descTextObj != null) descTMP = descTextObj.GetComponent<TextMeshProUGUI>();
        }

        private void ActivateMainStep(int stepIndex)
        {
            activeIndex = stepIndex;

            // Hide all special panels
            if (nameInputPanel != null) nameInputPanel.SetActive(false);
            if (avatarSelectionPanel != null) avatarSelectionPanel.SetActive(false);
            if (completionPanel != null) completionPanel.SetActive(false);

            bool isCompletion = stepIndex == 7;

            // Show/hide base content elements
            if (stepImageObj != null) stepImageObj.SetActive(!isCompletion);
            if (iconGlowObj != null) iconGlowObj.SetActive(!isCompletion);
            if (titleTextObj != null) titleTextObj.SetActive(!isCompletion);
            if (descTextObj != null) descTextObj.SetActive(!isCompletion);

            // Update title and description text
            if (!isCompletion && stepIndex < MAIN_TITLES.Length)
            {
                if (titleTMP != null) titleTMP.text = MAIN_TITLES[stepIndex];
                if (descTMP != null) descTMP.text = MAIN_DESCS[stepIndex];
            }

            // Show step-specific panel
            switch (stepIndex)
            {
                case 1: // Name Input
                    if (nameInputPanel != null) nameInputPanel.SetActive(true);
                    break;
                case 2: // Avatar Selection
                    if (avatarSelectionPanel != null) avatarSelectionPanel.SetActive(true);
                    break;
                case 7: // Completion
                    if (completionPanel != null) completionPanel.SetActive(true);
                    break;
            }

            MarkDirty();
        }

        #endregion

        #region Helpers

        private static GameObject FindChild(Transform parent, string name)
        {
            Transform t = parent.Find(name);
            return t != null ? t.gameObject : null;
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

        #endregion
    }
}
