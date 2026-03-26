using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;
using DigitPark.Monetization;
using System.Collections.Generic;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para reconstruir la UI del GameSelector con diseño profesional
    /// </summary>
    public class GameSelectorUIBuilder : EditorWindow
    {
        // Colores del tema neón
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color DARK_BG = new Color(0.02f, 0.04f, 0.08f, 1f);
        private static readonly Color CARD_BG = new Color(0.05f, 0.1f, 0.15f, 0.9f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.5f, 0.5f, 1f);

        [MenuItem("DigitPark/Scenes/Build Scene/Games/GameSelector", false, 120)]
        public static void ShowWindow()
        {
            GetWindow<GameSelectorUIBuilder>("GameSelector UI Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("GameSelector UI Builder", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Este script reconstruirá la UI del GameSelector.\n" +
                "Asegúrate de tener la escena GameSelector abierta.",
                MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("Reconstruir GameSelector UI", GUILayout.Height(40)))
            {
                RebuildGameSelectorUI();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Solo crear Cards (mantener estructura)", GUILayout.Height(30)))
            {
                CreateGameCardsOnly();
            }
        }

        private static void RebuildGameSelectorUI()
        {
            // Buscar el Canvas existente
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("No se encontró Canvas en la escena");
                return;
            }

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
            }

            // Buscar o crear el contenedor principal
            Transform canvasTransform = canvas.transform;

            // Buscar Background
            Transform background = canvasTransform.Find("Background");
            if (background == null)
            {
                Debug.LogError("No se encontró Background");
                return;
            }
            Image bgImg = background.GetComponent<Image>();
            if (bgImg != null) bgImg.color = Color.white;

            // Limpiar GamesPanel existente si existe
            Transform existingGamesPanel = canvasTransform.Find("GamesPanel");
            if (existingGamesPanel != null)
            {
                // Guardar referencias antes de destruir
                Debug.Log("Encontrado GamesPanel existente, se reconstruirá");
            }

            // Crear nueva estructura
            CreateNewGameSelectorLayout(canvasTransform);

            AutoAssigners.GameSelectorReferenceAssigner.RunAutoAssign();
            Debug.Log("GameSelector UI reconstruida exitosamente!");
            EditorUtility.SetDirty(canvas.gameObject);
        }

        private static void CreateNewGameSelectorLayout(Transform canvasTransform)
        {
            // ========== HEADER ==========
            GameObject header = CreateOrFind(canvasTransform, "Header");
            RectTransform headerRect = SetupRectTransform(header,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -79), new Vector2(0, 100)); // Header 100px, 29px top margin

            // BackButton - NO crear, el usuario usa su propio prefab

            // Limpiar título viejo con nombre anterior (era "TitleText", ahora es "GameSelectorTitleText")
            Transform oldTitle = header.transform.Find("TitleText");
            if (oldTitle != null)
            {
                Debug.Log("Eliminando TitleText duplicado (renombrado a GameSelectorTitleText)");
                DestroyImmediate(oldTitle.gameObject);
            }

            // Title - valores ajustados por el usuario
            GameObject title = CreateOrFind(header.transform, "GameSelectorTitleText");
            SetupRectTransform(title,
                new Vector2(0.07f, 0f), new Vector2(0.50f, 1f),
                Vector2.zero, Vector2.zero);
            SetupTitleText(title, "SELECT A GAME");

            // Currency pills (right side of header)
            var pills = CurrencyHeaderBarHelper.CreateCurrencyPills(header.transform);
            var pillsRT = pills.GetComponent<RectTransform>();
            pillsRT.anchorMin = new Vector2(0.52f, 0.5f);
            pillsRT.anchorMax = new Vector2(0.95f, 0.5f);
            pillsRT.pivot = new Vector2(0.5f, 0.5f);
            pillsRT.sizeDelta = new Vector2(0, 65);

            // ========== GAMES GRID ==========
            GameObject gamesPanel = CreateOrFind(canvasTransform, "GamesPanel");
            RectTransform gamesPanelRect = SetupRectTransform(gamesPanel,
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(0, -40), new Vector2(-60, -100)); // Below 100px header

            // Agregar Grid Layout
            GridLayoutGroup grid = gamesPanel.GetComponent<GridLayoutGroup>();
            if (grid == null) grid = gamesPanel.AddComponent<GridLayoutGroup>();

            grid.cellSize = new Vector2(420, 420); // Cards cuadrados para las imágenes
            grid.spacing = new Vector2(30, 30);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.MiddleCenter; // CENTRADO
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.padding = new RectOffset(40, 40, 20, 20);

            // ========== CREAR GAME CARDS ==========
            CreateGameCard(gamesPanel.transform, "DigitRushButton", "Digit Rush", "IconPlaceholder_DigitRush", false);
            CreateGameCard(gamesPanel.transform, "MemoryPairsButton", "Memory Pairs", "IconPlaceholder_MemoryPairs", false);
            CreateGameCard(gamesPanel.transform, "QuickMathButton", "Quick Math", "IconPlaceholder_QuickMath", false);
            CreateGameCard(gamesPanel.transform, "FlashTapButton", "Flash Tap", "IconPlaceholder_FlashTap", false);
            CreateGameCard(gamesPanel.transform, "OddOneOutButton", "Odd One Out", "IconPlaceholder_OddOneOut", false);
        }

        private static void CreateGameCard(Transform parent, string buttonName, string gameName, string iconName, bool isGold)
        {
            // Card Container (Button)
            GameObject card = CreateOrFind(parent, buttonName);

            // Limpiar children antiguos que ya no necesitamos
            string[] oldChildren = { "IconContainer", "GameNameText", "IconText", "Border", "InnerBackground", "SpecialBadge",
                                     "DigitRushBtnText", "MemoryPairsBtnText", "QuickMathBtnText", "FlashTapBtnText",
                                     "OddOneOutBtnText", "ArrowText" };
            foreach (string childName in oldChildren)
            {
                Transform oldChild = card.transform.Find(childName);
                if (oldChild != null) DestroyImmediate(oldChild.gameObject);
            }

            // Limpiar Outlines duplicados
            Outline[] existingOutlines = card.GetComponents<Outline>();
            for (int i = existingOutlines.Length - 1; i >= 0; i--)
            {
                DestroyImmediate(existingOutlines[i]);
            }

            // Limpiar CanvasGroup (GameSelectorAnimator lo agrega con alpha 0.6)
            CanvasGroup canvasGroup = card.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                Debug.Log($"Eliminando CanvasGroup de {buttonName} (causaba transparencia)");
                DestroyImmediate(canvasGroup);
            }

            // Configurar Button
            Button btn = card.GetComponent<Button>();
            if (btn == null) btn = card.AddComponent<Button>();

            // Imagen de fondo del card (será la imagen del juego)
            Image cardBg = card.GetComponent<Image>();
            if (cardBg == null) cardBg = card.AddComponent<Image>();
            cardBg.color = Color.white; // Blanco para mostrar la imagen sin tinte
            cardBg.preserveAspect = true;
            // La imagen se asignará manualmente desde el inspector
            // Ruta: Assets/_Project/Art/Icons/Games/[NombreJuego]Icon.png

            // Configurar colores del botón para efecto hover
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); // Brillo sutil en hover
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            colors.selectedColor = Color.white;
            btn.colors = colors;

            // ========== BORDE NEÓN EXTERIOR ==========
            // Usamos Outline para el efecto de borde neón
            Outline cardOutline = card.GetComponent<Outline>();
            if (cardOutline == null) cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = isGold ? GOLD : CYAN_NEON;
            cardOutline.effectDistance = new Vector2(4, 4);

            // Segundo outline para más glow
            Outline cardOutline2 = card.GetComponents<Outline>().Length > 1
                ? card.GetComponents<Outline>()[1]
                : card.AddComponent<Outline>();
            cardOutline2.effectColor = isGold
                ? new Color(1f, 0.84f, 0f, 0.5f)
                : new Color(0f, 1f, 1f, 0.5f);
            cardOutline2.effectDistance = new Vector2(8, 8);

            // Agregar componente de efectos
            GameCardEffect effect = card.GetComponent<GameCardEffect>();
            if (effect == null) effect = card.AddComponent<GameCardEffect>();

            // Configurar colores según si es gold o no
            if (isGold)
            {
                effect.SetColors(GOLD, new Color(1f, 0.9f, 0.4f, 1f), new Color(0.8f, 0.67f, 0f, 1f));
            }
            else
            {
                effect.SetColors(CYAN_NEON, new Color(0.4f, 1f, 1f, 1f), CYAN_DARK);
            }
        }

        private static void CreateActionButton(Transform parent, string buttonName, string text, bool isPrimary)
        {
            GameObject btnObj = CreateOrFind(parent, buttonName);

            var existingOutlines = btnObj.GetComponents<Outline>();
            foreach (var o in existingOutlines) DestroyImmediate(o);

            LayoutElement layout = btnObj.GetComponent<LayoutElement>();
            if (layout == null) layout = btnObj.AddComponent<LayoutElement>();
            layout.preferredWidth = 200;
            layout.preferredHeight = 80;
            layout.minWidth = 200;

            Image btnBg = btnObj.GetComponent<Image>();
            if (btnBg == null) btnBg = btnObj.AddComponent<Image>();
            btnBg.color = isPrimary ? CYAN_NEON : new Color(0.25f, 0.25f, 0.25f, 1f);

            Button btn = btnObj.GetComponent<Button>();
            if (btn == null) btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnBg;

            // Borde neón para los botones
            Outline btnOutline = btnObj.GetComponent<Outline>();
            if (btnOutline == null) btnOutline = btnObj.AddComponent<Outline>();
            btnOutline.effectColor = isPrimary ? new Color(0f, 1f, 1f, 0.5f) : CYAN_DARK;
            btnOutline.effectDistance = new Vector2(2, 2);

            GameObject textObj = CreateOrFind(btnObj.transform, "Text");
            SetupRectTransform(textObj, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            TextMeshProUGUI textTmp = textObj.GetComponent<TextMeshProUGUI>();
            if (textTmp == null) textTmp = textObj.AddComponent<TextMeshProUGUI>();
            textTmp.text = text;
            textTmp.fontSize = FontSizes.Body;
            textTmp.fontStyle = FontStyles.Bold;
            textTmp.color = isPrimary ? DARK_BG : Color.white;
            textTmp.alignment = TextAlignmentOptions.Center;
            textTmp.enableAutoSizing = true;
            textTmp.fontSizeMin = FontSizes.AutoMinBody;
            textTmp.fontSizeMax = FontSizes.Body;
            textTmp.overflowMode = TextOverflowModes.Ellipsis;
        }

        private static void CreateGameCardsOnly()
        {
            Canvas canvas = UIBuilderCanvasHelper.FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("No se encontró Canvas");
                return;
            }

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
            }

            Transform gamesPanel = canvas.transform.Find("GamesPanel");
            if (gamesPanel == null)
            {
                Debug.LogError("No se encontró GamesPanel");
                return;
            }

            // Limpiar cards existentes
            while (gamesPanel.childCount > 0)
            {
                DestroyImmediate(gamesPanel.GetChild(0).gameObject);
            }

            // Crear nuevas cards
            CreateGameCard(gamesPanel, "DigitRushButton", "Digit Rush", "IconPlaceholder_DigitRush", false);
            CreateGameCard(gamesPanel, "MemoryPairsButton", "Memory Pairs", "IconPlaceholder_MemoryPairs", false);
            CreateGameCard(gamesPanel, "QuickMathButton", "Quick Math", "IconPlaceholder_QuickMath", false);
            CreateGameCard(gamesPanel, "FlashTapButton", "Flash Tap", "IconPlaceholder_FlashTap", false);
            CreateGameCard(gamesPanel, "OddOneOutButton", "Odd One Out", "IconPlaceholder_OddOneOut", false);

            Debug.Log("Game Cards creadas exitosamente!");
            EditorUtility.SetDirty(canvas.gameObject);
        }

        private static void SetupBackButton(GameObject obj)
        {
            Button btn = obj.GetComponent<Button>();
            if (btn == null) btn = obj.AddComponent<Button>();

            Image img = obj.GetComponent<Image>();
            if (img == null) img = obj.AddComponent<Image>();
            img.color = Color.clear;

            // Texto de flecha
            GameObject arrowText = CreateOrFind(obj.transform, "ArrowText");
            SetupRectTransform(arrowText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Remove old TMP if present
            TextMeshProUGUI oldTmp = arrowText.GetComponent<TextMeshProUGUI>();
            if (oldTmp != null) Object.DestroyImmediate(oldTmp);
            Image tmp = arrowText.GetComponent<Image>();
            if (tmp == null) tmp = arrowText.AddComponent<Image>();
            Sprite arrowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/UI/icon_back_arrow.png");
            if (arrowSprite != null) tmp.sprite = arrowSprite;
            tmp.color = CYAN_NEON;
            tmp.preserveAspect = true;
            tmp.raycastTarget = false;
        }

        private static void SetupTitleText(GameObject obj, string text)
        {
            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = FontSizes.H4;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = CYAN_NEON;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinTitle;
            tmp.fontSizeMax = FontSizes.H4;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;
        }

        private static void CleanupOldUI()
        {
            // Solo limpiar SafeArea, NO destruir Background (se necesita como base)
            string[] toClean = { "SafeArea" };
            foreach (var canvas in Object.FindObjectsOfType<Canvas>(true))
            {
                if (canvas.transform.parent != null) continue;
                // No tocar TransitionCanvas ni EffectsCanvas
                if (canvas.gameObject.name.Contains("Transition") ||
                    canvas.gameObject.name.Contains("Effects")) continue;
                foreach (string name in toClean)
                {
                    Transform t = canvas.transform.Find(name);
                    if (t != null) Object.DestroyImmediate(t.gameObject);
                }
            }
        }

        // ========== UTILIDADES ==========

        private static GameObject CreateOrFind(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null) return existing.gameObject;

            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            // Agregar RectTransform por defecto
            if (obj.GetComponent<RectTransform>() == null)
            {
                obj.AddComponent<RectTransform>();
            }

            return obj;
        }

        private static RectTransform SetupRectTransform(GameObject obj, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            RectTransform rect = obj.GetComponent<RectTransform>();
            if (rect == null) rect = obj.AddComponent<RectTransform>();

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = sizeDelta;

            return rect;
        }
    }
}
