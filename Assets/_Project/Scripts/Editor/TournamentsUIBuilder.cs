using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para arreglar la UI de Tournaments con diseño profesional neon
    /// Principalmente para arreglar el scrollbar gris y mejorar consistencia visual
    /// </summary>
    public class TournamentsUIBuilder : EditorWindow
    {
        // Colores del tema neon (mismos que SearchPlayers)
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.4f, 1f);
        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color PANEL_BG = new Color(0.05f, 0.1f, 0.15f, 0.95f);
        private static readonly Color SCROLLBAR_BG = new Color(0.1f, 0.1f, 0.1f, 0.5f);

        [MenuItem("DigitPark/Fix Tournaments Scrollbar")]
        public static void FixScrollbar()
        {
            FixTournamentsScrollbar();
        }

        [MenuItem("DigitPark/Rebuild Tournaments UI")]
        public static void ShowWindow()
        {
            GetWindow<TournamentsUIBuilder>("Tournaments UI Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Tournaments UI Builder", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Este script arregla el scrollbar y mejora la UI de Tournaments.\n" +
                "Asegurate de tener la escena Tournaments abierta.",
                MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("Arreglar Solo Scrollbar", GUILayout.Height(30)))
            {
                FixTournamentsScrollbar();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Arreglar UI Completa", GUILayout.Height(40)))
            {
                RebuildTournamentsUI();
            }
        }

        /// <summary>
        /// Arregla solo el scrollbar con estilo neon
        /// </summary>
        private static void FixTournamentsScrollbar()
        {
            // Buscar todos los ScrollRect en la escena
            ScrollRect[] scrollRects = Resources.FindObjectsOfTypeAll<ScrollRect>();

            int fixedCount = 0;
            foreach (var scrollRect in scrollRects)
            {
                if (scrollRect.verticalScrollbar != null)
                {
                    FixScrollbarStyle(scrollRect.verticalScrollbar);
                    fixedCount++;
                }
                if (scrollRect.horizontalScrollbar != null)
                {
                    FixScrollbarStyle(scrollRect.horizontalScrollbar);
                    fixedCount++;
                }
            }

            // Buscar scrollbars huerfanos
            Scrollbar[] allScrollbars = Resources.FindObjectsOfTypeAll<Scrollbar>();
            foreach (var scrollbar in allScrollbars)
            {
                FixScrollbarStyle(scrollbar);
                fixedCount++;
            }

            Debug.Log($"[TournamentsUIBuilder] {fixedCount} scrollbars arreglados con estilo neon!");

            // Marcar escena como modificada
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        /// <summary>
        /// Aplica estilo neon a un scrollbar
        /// </summary>
        private static void FixScrollbarStyle(Scrollbar scrollbar)
        {
            if (scrollbar == null) return;

            // Fondo del scrollbar
            Image scrollbarBg = scrollbar.GetComponent<Image>();
            if (scrollbarBg != null)
            {
                scrollbarBg.color = SCROLLBAR_BG;
            }

            // Handle del scrollbar
            if (scrollbar.handleRect != null)
            {
                Image handleImage = scrollbar.handleRect.GetComponent<Image>();
                if (handleImage != null)
                {
                    handleImage.color = CYAN_DARK;
                }

                // Agregar glow al handle
                Outline handleGlow = scrollbar.handleRect.GetComponent<Outline>();
                if (handleGlow == null)
                {
                    handleGlow = scrollbar.handleRect.gameObject.AddComponent<Outline>();
                }
                handleGlow.effectColor = new Color(0f, 1f, 1f, 0.3f);
                handleGlow.effectDistance = new Vector2(1, 1);
            }

            // Ajustar ancho del scrollbar
            RectTransform scrollbarRect = scrollbar.GetComponent<RectTransform>();
            if (scrollbarRect != null)
            {
                // Si es vertical, ajustar ancho
                if (scrollbar.direction == Scrollbar.Direction.BottomToTop ||
                    scrollbar.direction == Scrollbar.Direction.TopToBottom)
                {
                    scrollbarRect.sizeDelta = new Vector2(10, scrollbarRect.sizeDelta.y);
                }
            }

            Debug.Log($"[TournamentsUIBuilder] Scrollbar '{scrollbar.name}' arreglado");
        }

        /// <summary>
        /// Reconstruye la UI completa de Tournaments
        /// </summary>
        private static void RebuildTournamentsUI()
        {
            Canvas canvas = GameObject.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No se encontro Canvas en la escena");
                return;
            }

            // 1. Arreglar scrollbars
            FixTournamentsScrollbar();

            // 2. Mejorar tabs
            FixTabsStyle(canvas.transform);

            // 3. Mejorar botones
            FixButtonsStyle(canvas.transform);

            Debug.Log("[TournamentsUIBuilder] UI de Tournaments reconstruida!");

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        /// <summary>
        /// Mejora el estilo de los tabs
        /// </summary>
        private static void FixTabsStyle(Transform canvasTransform)
        {
            // Buscar botones de tabs
            string[] tabNames = { "SearchTab", "MyTournamentsTab", "CreateTab" };

            foreach (string tabName in tabNames)
            {
                Button tab = FindButtonByName(canvasTransform, tabName);
                if (tab != null)
                {
                    // Mejorar colores
                    ColorBlock colors = tab.colors;
                    colors.normalColor = new Color(0.2f, 0.2f, 0.25f, 1f);
                    colors.highlightedColor = new Color(0f, 0.6f, 0.6f, 1f);
                    colors.pressedColor = CYAN_NEON;
                    colors.selectedColor = CYAN_NEON;
                    tab.colors = colors;

                    // Agregar borde neon sutil
                    Outline outline = tab.GetComponent<Outline>();
                    if (outline == null)
                    {
                        outline = tab.gameObject.AddComponent<Outline>();
                    }
                    outline.effectColor = CYAN_DARK;
                    outline.effectDistance = new Vector2(1, 1);

                    Debug.Log($"[TournamentsUIBuilder] Tab '{tabName}' mejorado");
                }
            }
        }

        /// <summary>
        /// Mejora el estilo de los botones principales
        /// </summary>
        private static void FixButtonsStyle(Transform canvasTransform)
        {
            // Botones a mejorar
            string[] buttonNames = {
                "BackButton", "CreateButton", "ApplyButton", "ClearButton",
                "ConfirmButton", "CancelButton", "SearchOptionsButton"
            };

            foreach (string buttonName in buttonNames)
            {
                Button button = FindButtonByName(canvasTransform, buttonName);
                if (button != null)
                {
                    Image bg = button.GetComponent<Image>();
                    if (bg != null)
                    {
                        // Determinar si es primario o secundario
                        bool isPrimary = buttonName.Contains("Create") ||
                                        buttonName.Contains("Apply") ||
                                        buttonName.Contains("Confirm");

                        if (isPrimary)
                        {
                            bg.color = CYAN_NEON;

                            // Texto oscuro para boton claro
                            TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
                            if (text != null)
                            {
                                text.color = DARK_BG;
                            }
                        }
                        else
                        {
                            bg.color = new Color(0.1f, 0.15f, 0.2f, 1f);

                            // Borde neon
                            Outline outline = button.GetComponent<Outline>();
                            if (outline == null)
                            {
                                outline = button.gameObject.AddComponent<Outline>();
                            }
                            outline.effectColor = CYAN_DARK;
                            outline.effectDistance = new Vector2(1.5f, 1.5f);
                        }
                    }

                    Debug.Log($"[TournamentsUIBuilder] Boton '{buttonName}' mejorado");
                }
            }
        }

        /// <summary>
        /// Busca un boton por nombre recursivamente
        /// </summary>
        private static Button FindButtonByName(Transform parent, string name)
        {
            // Buscar directamente
            Transform found = parent.Find(name);
            if (found != null)
            {
                Button btn = found.GetComponent<Button>();
                if (btn != null) return btn;
            }

            // Buscar recursivamente
            foreach (Transform child in parent)
            {
                if (child.name == name)
                {
                    Button btn = child.GetComponent<Button>();
                    if (btn != null) return btn;
                }

                Button result = FindButtonByName(child, name);
                if (result != null) return result;
            }

            return null;
        }
    }
}
