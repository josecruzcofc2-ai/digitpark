#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Crea el panel de selección de juego para retos
    /// </summary>
    public class GameSelectionPanelCreator : EditorWindow
    {
        // Colores
        private static readonly Color32 OVERLAY_COLOR = new Color32(0, 0, 0, 180);      // Negro semi-transparente
        private static readonly Color32 PANEL_BG_COLOR = new Color32(13, 27, 42, 255);  // #0D1B2A
        private static readonly Color32 BUTTON_COLOR = new Color32(0, 255, 255, 255);   // Cyan
        private static readonly Color32 CANCEL_COLOR = new Color32(136, 136, 136, 255); // Gris
        private static readonly Color32 TITLE_COLOR = new Color32(0, 255, 255, 255);    // Cyan
        private static readonly Color32 TEXT_COLOR = new Color32(255, 255, 255, 255);   // Blanco

        // Tamaños
        private const float PANEL_WIDTH = 800f;
        private const float PANEL_HEIGHT = 900f;
        private const float BUTTON_WIDTH = 600f;
        private const float BUTTON_HEIGHT = 100f;
        private const float BUTTON_SPACING = 20f;
        private const float TITLE_FONT_SIZE = 48f;
        private const float BUTTON_FONT_SIZE = 36f;

        // Juegos
        private static readonly string[] GAME_NAMES = {
            "Digit Rush",
            "Memory Pairs",
            "Quick Math",
            "Flash Tap",
            "Odd One Out"
        };

        private static readonly string[] GAME_IDS = {
            "DigitRush",
            "MemoryPairs",
            "QuickMath",
            "FlashTap",
            "OddOneOut"
        };

        [MenuItem("DigitPark/UI/Create Game Selection Panel")]
        public static void CreatePanel()
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontró Canvas en la escena", "OK");
                return;
            }

            // Verificar si ya existe
            if (GameObject.Find("GameSelectionPanel") != null)
            {
                EditorUtility.DisplayDialog("Ya existe",
                    "GameSelectionPanel ya existe en la escena.\n" +
                    "Elimínalo primero si quieres crear uno nuevo.",
                    "OK");
                return;
            }

            // ============ CREAR PANEL PRINCIPAL ============
            GameObject mainPanel = new GameObject("GameSelectionPanel");
            mainPanel.transform.SetParent(canvas.transform, false);

            RectTransform mainRT = mainPanel.AddComponent<RectTransform>();
            mainRT.anchorMin = Vector2.zero;
            mainRT.anchorMax = Vector2.one;
            mainRT.offsetMin = Vector2.zero;
            mainRT.offsetMax = Vector2.zero;

            // ============ DARK OVERLAY ============
            GameObject overlay = new GameObject("DarkOverlay");
            overlay.transform.SetParent(mainPanel.transform, false);

            Image overlayImg = overlay.AddComponent<Image>();
            overlayImg.color = OVERLAY_COLOR;
            overlayImg.raycastTarget = true;

            RectTransform overlayRT = overlay.GetComponent<RectTransform>();
            overlayRT.anchorMin = Vector2.zero;
            overlayRT.anchorMax = Vector2.one;
            overlayRT.offsetMin = Vector2.zero;
            overlayRT.offsetMax = Vector2.zero;

            // Agregar botón para cerrar al tocar overlay
            Button overlayBtn = overlay.AddComponent<Button>();
            overlayBtn.transition = Selectable.Transition.None;

            // ============ PANEL BACKGROUND ============
            GameObject panelBG = new GameObject("PanelBackground");
            panelBG.transform.SetParent(mainPanel.transform, false);

            Image panelImg = panelBG.AddComponent<Image>();
            panelImg.color = PANEL_BG_COLOR;
            panelImg.raycastTarget = true;

            RectTransform panelRT = panelBG.GetComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.5f, 0.5f);
            panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.pivot = new Vector2(0.5f, 0.5f);
            panelRT.sizeDelta = new Vector2(PANEL_WIDTH, PANEL_HEIGHT);

            // Agregar outline/borde
            Outline panelOutline = panelBG.AddComponent<Outline>();
            panelOutline.effectColor = BUTTON_COLOR;
            panelOutline.effectDistance = new Vector2(3, 3);

            // ============ TITLE ============
            GameObject title = new GameObject("TitleText");
            title.transform.SetParent(panelBG.transform, false);

            TextMeshProUGUI titleTMP = title.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "Elige un juego";
            titleTMP.fontSize = TITLE_FONT_SIZE;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.color = TITLE_COLOR;
            titleTMP.alignment = TextAlignmentOptions.Center;

            RectTransform titleRT = title.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0.5f, 1);
            titleRT.anchoredPosition = new Vector2(0, -40);
            titleRT.sizeDelta = new Vector2(0, 80);

            // ============ DIVIDER ============
            GameObject divider = new GameObject("Divider");
            divider.transform.SetParent(panelBG.transform, false);

            Image dividerImg = divider.AddComponent<Image>();
            dividerImg.color = new Color32(0, 255, 255, 128);
            dividerImg.raycastTarget = false;

            RectTransform dividerRT = divider.GetComponent<RectTransform>();
            dividerRT.anchorMin = new Vector2(0, 1);
            dividerRT.anchorMax = new Vector2(1, 1);
            dividerRT.pivot = new Vector2(0.5f, 1);
            dividerRT.anchoredPosition = new Vector2(0, -130);
            dividerRT.sizeDelta = new Vector2(-80, 2);

            // ============ GAMES CONTAINER ============
            GameObject gamesContainer = new GameObject("GamesContainer");
            gamesContainer.transform.SetParent(panelBG.transform, false);

            RectTransform gamesRT = gamesContainer.GetComponent<RectTransform>();
            if (gamesRT == null) gamesRT = gamesContainer.AddComponent<RectTransform>();

            gamesRT.anchorMin = new Vector2(0.5f, 1);
            gamesRT.anchorMax = new Vector2(0.5f, 1);
            gamesRT.pivot = new Vector2(0.5f, 1);
            gamesRT.anchoredPosition = new Vector2(0, -160);
            gamesRT.sizeDelta = new Vector2(BUTTON_WIDTH, 600);

            VerticalLayoutGroup vlg = gamesContainer.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = false;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            vlg.spacing = BUTTON_SPACING;
            vlg.padding = new RectOffset(0, 0, 20, 20);

            // ============ GAME BUTTONS ============
            for (int i = 0; i < GAME_NAMES.Length; i++)
            {
                CreateGameButton(gamesContainer.transform, GAME_NAMES[i], GAME_IDS[i]);
            }

            // ============ CANCEL BUTTON ============
            GameObject cancelBtn = new GameObject("CancelButton");
            cancelBtn.transform.SetParent(panelBG.transform, false);

            Image cancelImg = cancelBtn.AddComponent<Image>();
            cancelImg.color = new Color32(40, 40, 40, 255);

            Button cancelButton = cancelBtn.AddComponent<Button>();
            ColorBlock cancelColors = cancelButton.colors;
            cancelColors.normalColor = new Color32(40, 40, 40, 255);
            cancelColors.highlightedColor = new Color32(60, 60, 60, 255);
            cancelColors.pressedColor = new Color32(30, 30, 30, 255);
            cancelButton.colors = cancelColors;

            // Outline para cancel
            Outline cancelOutline = cancelBtn.AddComponent<Outline>();
            cancelOutline.effectColor = CANCEL_COLOR;
            cancelOutline.effectDistance = new Vector2(2, 2);

            RectTransform cancelRT = cancelBtn.GetComponent<RectTransform>();
            cancelRT.anchorMin = new Vector2(0.5f, 0);
            cancelRT.anchorMax = new Vector2(0.5f, 0);
            cancelRT.pivot = new Vector2(0.5f, 0);
            cancelRT.anchoredPosition = new Vector2(0, 40);
            cancelRT.sizeDelta = new Vector2(300, 80);

            // Cancel Text
            GameObject cancelText = new GameObject("CancelText");
            cancelText.transform.SetParent(cancelBtn.transform, false);

            TextMeshProUGUI cancelTMP = cancelText.AddComponent<TextMeshProUGUI>();
            cancelTMP.text = "Cancelar";
            cancelTMP.fontSize = 32;
            cancelTMP.color = CANCEL_COLOR;
            cancelTMP.alignment = TextAlignmentOptions.Center;

            RectTransform cancelTextRT = cancelText.GetComponent<RectTransform>();
            cancelTextRT.anchorMin = Vector2.zero;
            cancelTextRT.anchorMax = Vector2.one;
            cancelTextRT.offsetMin = Vector2.zero;
            cancelTextRT.offsetMax = Vector2.zero;

            // ============ DESACTIVAR PANEL INICIALMENTE ============
            mainPanel.SetActive(false);

            // Seleccionar y marcar dirty
            Selection.activeGameObject = mainPanel;
            EditorUtility.SetDirty(mainPanel);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("[GameSelectionPanel] Panel creado exitosamente");

            EditorUtility.DisplayDialog("Panel Creado",
                "GameSelectionPanel creado exitosamente.\n\n" +
                "Estructura:\n" +
                "- DarkOverlay (fondo oscuro)\n" +
                "- PanelBackground (popup)\n" +
                "- 5 botones de juegos\n" +
                "- Botón Cancelar\n\n" +
                "El panel está DESACTIVADO por defecto.\n" +
                "Se activa cuando tocas RETAR.\n\n" +
                "IMPORTANTE: Asigna el panel en ProfileManager inspector.",
                "OK");
        }

        private static void CreateGameButton(Transform parent, string gameName, string gameId)
        {
            GameObject btn = new GameObject(gameId + "Button");
            btn.transform.SetParent(parent, false);

            Image btnImg = btn.AddComponent<Image>();
            btnImg.color = new Color32(20, 35, 55, 255);

            Button button = btn.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color32(20, 35, 55, 255);
            colors.highlightedColor = new Color32(0, 80, 80, 255);
            colors.pressedColor = new Color32(0, 50, 50, 255);
            button.colors = colors;

            // Outline cyan
            Outline outline = btn.AddComponent<Outline>();
            outline.effectColor = BUTTON_COLOR;
            outline.effectDistance = new Vector2(2, 2);

            RectTransform btnRT = btn.GetComponent<RectTransform>();
            btnRT.sizeDelta = new Vector2(BUTTON_WIDTH, BUTTON_HEIGHT);

            // Texto del botón
            GameObject text = new GameObject("Text");
            text.transform.SetParent(btn.transform, false);

            TextMeshProUGUI tmp = text.AddComponent<TextMeshProUGUI>();
            tmp.text = gameName;
            tmp.fontSize = BUTTON_FONT_SIZE;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = TEXT_COLOR;
            tmp.alignment = TextAlignmentOptions.Center;

            RectTransform textRT = text.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            EditorUtility.SetDirty(btn);
        }
    }
}
#endif
