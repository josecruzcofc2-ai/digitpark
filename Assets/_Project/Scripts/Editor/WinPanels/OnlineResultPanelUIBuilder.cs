using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using System.IO;

namespace DigitPark.Editor
{
    /// <summary>
    /// Builder para crear prefabs de paneles de resultados online 1v1
    /// Estilo neon profesional que muestra victoria/derrota y diferencia de tiempos
    /// </summary>
    public static class OnlineResultPanelUIBuilder
    {
        // Colores neon
        private static readonly Color WIN_GREEN = new Color(0.2f, 1f, 0.4f);
        private static readonly Color LOSE_RED = new Color(1f, 0.3f, 0.3f);
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f);
        private static readonly Color PURPLE_NEON = new Color(0.7f, 0.3f, 1f);
        private static readonly Color ORANGE_NEON = new Color(1f, 0.6f, 0.2f);
        private static readonly Color DARK_BG = new Color(0.02f, 0.04f, 0.08f, 0.95f);
        private static readonly Color CARD_BG = new Color(0.05f, 0.08f, 0.12f);
        private static readonly Color PANEL_BG = new Color(0.08f, 0.12f, 0.18f);

        private const string PREFAB_PATH = "Assets/_Project/Prefabs/UI/WinPanels";

        [MenuItem("DigitPark/Create Online Win Panel Prefab")]
        public static void BuildWinPanel()
        {
            BuildPanelPrefab(true);
        }

        [MenuItem("DigitPark/Create Online Lose Panel Prefab")]
        public static void BuildLosePanel()
        {
            BuildPanelPrefab(false);
        }

        private static void BuildPanelPrefab(bool isWinVersion)
        {
            // Asegurar que existe la carpeta de prefabs
            if (!Directory.Exists(PREFAB_PATH))
            {
                Directory.CreateDirectory(PREFAB_PATH);
                AssetDatabase.Refresh();
            }

            string panelName = isWinVersion ? "OnlineWinPanel" : "OnlineLosePanel";
            Color mainColor = isWinVersion ? WIN_GREEN : LOSE_RED;
            string resultText = isWinVersion ? "VICTORIA" : "DERROTA";
            string subtitleText = isWinVersion ? "Mejor tiempo que tu oponente" : "Tu oponente fue mas rapido";

            // Crear panel principal (sin parent para prefab)
            GameObject panel = new GameObject(panelName);
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Fondo oscuro semi-transparente
            Image panelBg = panel.AddComponent<Image>();
            panelBg.color = DARK_BG;

            // Canvas Group para animaciones
            CanvasGroup cg = panel.AddComponent<CanvasGroup>();

            // Contenido centrado
            GameObject content = CreateElement(panel.transform, "Content");
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.05f, 0.15f);
            contentRect.anchorMax = new Vector2(0.95f, 0.9f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            Image contentBg = content.AddComponent<Image>();
            contentBg.color = PANEL_BG;

            // Borde neon del contenido
            Outline contentBorder = content.AddComponent<Outline>();
            contentBorder.effectColor = mainColor;
            contentBorder.effectDistance = new Vector2(3, -3);

            // === HEADER - Resultado ===
            CreateResultHeader(content.transform, mainColor, resultText, subtitleText, isWinVersion);

            // === SECCION VS - Comparación de jugadores ===
            CreateVSSection(content.transform, isWinVersion);

            // === DIFERENCIA DE TIEMPO ===
            CreateTimeDifferenceSection(content.transform, mainColor, isWinVersion);

            // === BOTONES ===
            CreateButtons(content.transform, mainColor);

            // Agregar controlador
            panel.AddComponent<DigitPark.UI.OnlineResultPanelController>();

            // Guardar como prefab
            string prefabPath = $"{PREFAB_PATH}/{panelName}.prefab";

            // Eliminar prefab existente si existe
            if (File.Exists(prefabPath))
            {
                AssetDatabase.DeleteAsset(prefabPath);
            }

            // Crear prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(panel, prefabPath);

            // Destruir el objeto temporal
            Object.DestroyImmediate(panel);

            // Seleccionar el prefab en el Project
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);

            Debug.Log($"[OnlineResultPanelUIBuilder] Prefab creado: {prefabPath}");
            EditorUtility.DisplayDialog("Prefab Creado",
                $"{panelName} prefab guardado en:\n{prefabPath}\n\nAsignalo al OnlineResultManager.",
                "OK");
        }

        private static GameObject CreateElement(Transform parent, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private static void CreateResultHeader(Transform parent, Color mainColor, string resultText, string subtitleText, bool isWin)
        {
            // Header container
            GameObject header = CreateElement(parent, "Header");
            RectTransform headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 0.7f);
            headerRect.anchorMax = new Vector2(1, 0.98f);
            headerRect.offsetMin = Vector2.zero;
            headerRect.offsetMax = Vector2.zero;

            // Icono de resultado (trofeo o X)
            GameObject icon = CreateElement(header.transform, "ResultIcon");
            RectTransform iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.35f, 0.5f);
            iconRect.anchorMax = new Vector2(0.65f, 0.95f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = mainColor;
            // Aquí se asignaría el sprite correspondiente

            // Texto de resultado grande
            GameObject resultTitle = CreateElement(header.transform, "ResultTitleText");
            RectTransform titleRect = resultTitle.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.2f);
            titleRect.anchorMax = new Vector2(0.9f, 0.55f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            TextMeshProUGUI titleTMP = resultTitle.AddComponent<TextMeshProUGUI>();
            titleTMP.text = resultText;
            titleTMP.fontSize = 72;
            titleTMP.color = mainColor;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.fontStyle = FontStyles.Bold;

            // Efecto glow
            Outline titleGlow = resultTitle.AddComponent<Outline>();
            titleGlow.effectColor = new Color(mainColor.r, mainColor.g, mainColor.b, 0.5f);
            titleGlow.effectDistance = new Vector2(4, -4);

            // Subtítulo
            GameObject subtitle = CreateElement(header.transform, "ResultSubtitleText");
            RectTransform subtitleRect = subtitle.GetComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0.1f, 0.02f);
            subtitleRect.anchorMax = new Vector2(0.9f, 0.22f);
            subtitleRect.offsetMin = Vector2.zero;
            subtitleRect.offsetMax = Vector2.zero;

            TextMeshProUGUI subtitleTMP = subtitle.AddComponent<TextMeshProUGUI>();
            subtitleTMP.text = subtitleText;
            subtitleTMP.fontSize = 28;
            subtitleTMP.color = new Color(0.8f, 0.8f, 0.8f);
            subtitleTMP.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateVSSection(Transform parent, bool isWin)
        {
            // VS Section container
            GameObject vsSection = CreateElement(parent, "VSSection");
            RectTransform vsRect = vsSection.GetComponent<RectTransform>();
            vsRect.anchorMin = new Vector2(0.02f, 0.35f);
            vsRect.anchorMax = new Vector2(0.98f, 0.68f);
            vsRect.offsetMin = Vector2.zero;
            vsRect.offsetMax = Vector2.zero;

            // Player card (izquierda)
            CreatePlayerCard(vsSection.transform, true, isWin);

            // VS en el centro
            GameObject vsCenter = CreateElement(vsSection.transform, "VSCenter");
            RectTransform vsCenterRect = vsCenter.GetComponent<RectTransform>();
            vsCenterRect.anchorMin = new Vector2(0.4f, 0.35f);
            vsCenterRect.anchorMax = new Vector2(0.6f, 0.65f);
            vsCenterRect.offsetMin = Vector2.zero;
            vsCenterRect.offsetMax = Vector2.zero;

            // VS background
            Image vsBg = vsCenter.AddComponent<Image>();
            vsBg.color = new Color(0.1f, 0.1f, 0.15f);

            // VS text
            GameObject vsText = CreateElement(vsCenter.transform, "VSText");
            RectTransform vsTextRect = vsText.GetComponent<RectTransform>();
            vsTextRect.anchorMin = Vector2.zero;
            vsTextRect.anchorMax = Vector2.one;
            vsTextRect.offsetMin = Vector2.zero;
            vsTextRect.offsetMax = Vector2.zero;

            TextMeshProUGUI vsTMP = vsText.AddComponent<TextMeshProUGUI>();
            vsTMP.text = "VS";
            vsTMP.fontSize = 42;
            vsTMP.color = ORANGE_NEON;
            vsTMP.alignment = TextAlignmentOptions.Center;
            vsTMP.fontStyle = FontStyles.Bold;

            // Opponent card (derecha)
            CreatePlayerCard(vsSection.transform, false, isWin);
        }

        private static void CreatePlayerCard(Transform parent, bool isPlayer, bool playerWon)
        {
            string cardName = isPlayer ? "PlayerCard" : "OpponentCard";
            float anchorMinX = isPlayer ? 0.02f : 0.52f;
            float anchorMaxX = isPlayer ? 0.48f : 0.98f;

            Color cardColor = isPlayer ? CYAN_NEON : PURPLE_NEON;
            bool isWinner = (isPlayer && playerWon) || (!isPlayer && !playerWon);

            GameObject card = CreateElement(parent, cardName);
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(anchorMinX, 0.05f);
            cardRect.anchorMax = new Vector2(anchorMaxX, 0.95f);
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = CARD_BG;

            // Borde del ganador más brillante
            if (isWinner)
            {
                Outline winnerBorder = card.AddComponent<Outline>();
                winnerBorder.effectColor = WIN_GREEN;
                winnerBorder.effectDistance = new Vector2(3, -3);
            }

            // Highlight para el ganador
            GameObject highlight = CreateElement(card.transform, isPlayer ? "PlayerHighlight" : "OpponentHighlight");
            RectTransform highlightRect = highlight.GetComponent<RectTransform>();
            highlightRect.anchorMin = Vector2.zero;
            highlightRect.anchorMax = Vector2.one;
            highlightRect.offsetMin = Vector2.zero;
            highlightRect.offsetMax = Vector2.zero;

            Image highlightImg = highlight.AddComponent<Image>();
            highlightImg.color = new Color(WIN_GREEN.r, WIN_GREEN.g, WIN_GREEN.b, 0.1f);
            highlight.SetActive(isWinner);

            // Nombre del jugador
            GameObject nameObj = CreateElement(card.transform, isPlayer ? "PlayerNameText" : "OpponentNameText");
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.05f, 0.7f);
            nameRect.anchorMax = new Vector2(0.95f, 0.95f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;

            TextMeshProUGUI nameTMP = nameObj.AddComponent<TextMeshProUGUI>();
            nameTMP.text = isPlayer ? "TU" : "OPONENTE";
            nameTMP.fontSize = 28;
            nameTMP.color = cardColor;
            nameTMP.alignment = TextAlignmentOptions.Center;
            nameTMP.fontStyle = FontStyles.Bold;

            // Tiempo
            GameObject timeObj = CreateElement(card.transform, isPlayer ? "PlayerTimeText" : "OpponentTimeText");
            RectTransform timeRect = timeObj.GetComponent<RectTransform>();
            timeRect.anchorMin = new Vector2(0.05f, 0.35f);
            timeRect.anchorMax = new Vector2(0.95f, 0.65f);
            timeRect.offsetMin = Vector2.zero;
            timeRect.offsetMax = Vector2.zero;

            TextMeshProUGUI timeTMP = timeObj.AddComponent<TextMeshProUGUI>();
            timeTMP.text = "12.45s";
            timeTMP.fontSize = 48;
            timeTMP.color = Color.white;
            timeTMP.alignment = TextAlignmentOptions.Center;
            timeTMP.fontStyle = FontStyles.Bold;

            // Errores
            GameObject errorsObj = CreateElement(card.transform, isPlayer ? "PlayerErrorsText" : "OpponentErrorsText");
            RectTransform errorsRect = errorsObj.GetComponent<RectTransform>();
            errorsRect.anchorMin = new Vector2(0.1f, 0.08f);
            errorsRect.anchorMax = new Vector2(0.9f, 0.3f);
            errorsRect.offsetMin = Vector2.zero;
            errorsRect.offsetMax = Vector2.zero;

            TextMeshProUGUI errorsTMP = errorsObj.AddComponent<TextMeshProUGUI>();
            errorsTMP.text = "0 errores";
            errorsTMP.fontSize = 22;
            errorsTMP.color = new Color(0.6f, 0.6f, 0.6f);
            errorsTMP.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateTimeDifferenceSection(Transform parent, Color mainColor, bool isWin)
        {
            // Time difference container
            GameObject timeDiffSection = CreateElement(parent, "TimeDifferenceSection");
            RectTransform timeDiffRect = timeDiffSection.GetComponent<RectTransform>();
            timeDiffRect.anchorMin = new Vector2(0.15f, 0.2f);
            timeDiffRect.anchorMax = new Vector2(0.85f, 0.33f);
            timeDiffRect.offsetMin = Vector2.zero;
            timeDiffRect.offsetMax = Vector2.zero;

            Image timeDiffBg = timeDiffSection.AddComponent<Image>();
            timeDiffBg.color = new Color(mainColor.r * 0.2f, mainColor.g * 0.2f, mainColor.b * 0.2f, 0.5f);

            // Borde
            Outline timeDiffBorder = timeDiffSection.AddComponent<Outline>();
            timeDiffBorder.effectColor = mainColor;
            timeDiffBorder.effectDistance = new Vector2(2, -2);

            // Label
            GameObject label = CreateElement(timeDiffSection.transform, "TimeDifferenceLabel");
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.05f, 0.55f);
            labelRect.anchorMax = new Vector2(0.95f, 0.95f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            TextMeshProUGUI labelTMP = label.AddComponent<TextMeshProUGUI>();
            labelTMP.text = isWin ? "Mas rapido por" : "Mas lento por";
            labelTMP.fontSize = 24;
            labelTMP.color = new Color(0.7f, 0.7f, 0.7f);
            labelTMP.alignment = TextAlignmentOptions.Center;

            // Valor de diferencia
            GameObject diffValue = CreateElement(timeDiffSection.transform, "TimeDifferenceText");
            RectTransform diffRect = diffValue.GetComponent<RectTransform>();
            diffRect.anchorMin = new Vector2(0.1f, 0.05f);
            diffRect.anchorMax = new Vector2(0.9f, 0.6f);
            diffRect.offsetMin = Vector2.zero;
            diffRect.offsetMax = Vector2.zero;

            TextMeshProUGUI diffTMP = diffValue.AddComponent<TextMeshProUGUI>();
            diffTMP.text = isWin ? "-2.35s" : "+2.35s";
            diffTMP.fontSize = 42;
            diffTMP.color = mainColor;
            diffTMP.alignment = TextAlignmentOptions.Center;
            diffTMP.fontStyle = FontStyles.Bold;
        }

        private static void CreateButtons(Transform parent, Color mainColor)
        {
            // Buttons container
            GameObject buttonsContainer = CreateElement(parent, "ButtonsContainer");
            RectTransform buttonsRect = buttonsContainer.GetComponent<RectTransform>();
            buttonsRect.anchorMin = new Vector2(0.05f, 0.02f);
            buttonsRect.anchorMax = new Vector2(0.95f, 0.18f);
            buttonsRect.offsetMin = Vector2.zero;
            buttonsRect.offsetMax = Vector2.zero;

            // Continue button (principal)
            GameObject continueBtn = CreateElement(buttonsContainer.transform, "ContinueButton");
            RectTransform continueBtnRect = continueBtn.GetComponent<RectTransform>();
            continueBtnRect.anchorMin = new Vector2(0.52f, 0.15f);
            continueBtnRect.anchorMax = new Vector2(0.98f, 0.85f);
            continueBtnRect.offsetMin = Vector2.zero;
            continueBtnRect.offsetMax = Vector2.zero;

            Image continueBg = continueBtn.AddComponent<Image>();
            continueBg.color = new Color(mainColor.r * 0.3f, mainColor.g * 0.3f, mainColor.b * 0.3f);

            Outline continueBorder = continueBtn.AddComponent<Outline>();
            continueBorder.effectColor = mainColor;
            continueBorder.effectDistance = new Vector2(2, -2);

            Button continueButton = continueBtn.AddComponent<Button>();

            GameObject continueText = CreateElement(continueBtn.transform, "ContinueButtonText");
            RectTransform continueTextRect = continueText.GetComponent<RectTransform>();
            continueTextRect.anchorMin = Vector2.zero;
            continueTextRect.anchorMax = Vector2.one;
            continueTextRect.offsetMin = Vector2.zero;
            continueTextRect.offsetMax = Vector2.zero;

            TextMeshProUGUI continueTMP = continueText.AddComponent<TextMeshProUGUI>();
            continueTMP.text = "CONTINUAR";
            continueTMP.fontSize = 32;
            continueTMP.color = mainColor;
            continueTMP.alignment = TextAlignmentOptions.Center;
            continueTMP.fontStyle = FontStyles.Bold;

            // Rematch button (secundario)
            GameObject rematchBtn = CreateElement(buttonsContainer.transform, "RematchButton");
            RectTransform rematchBtnRect = rematchBtn.GetComponent<RectTransform>();
            rematchBtnRect.anchorMin = new Vector2(0.02f, 0.15f);
            rematchBtnRect.anchorMax = new Vector2(0.48f, 0.85f);
            rematchBtnRect.offsetMin = Vector2.zero;
            rematchBtnRect.offsetMax = Vector2.zero;

            Image rematchBg = rematchBtn.AddComponent<Image>();
            rematchBg.color = new Color(0.1f, 0.1f, 0.15f);

            Outline rematchBorder = rematchBtn.AddComponent<Outline>();
            rematchBorder.effectColor = CYAN_NEON;
            rematchBorder.effectDistance = new Vector2(2, -2);

            Button rematchButton = rematchBtn.AddComponent<Button>();

            GameObject rematchText = CreateElement(rematchBtn.transform, "RematchButtonText");
            RectTransform rematchTextRect = rematchText.GetComponent<RectTransform>();
            rematchTextRect.anchorMin = Vector2.zero;
            rematchTextRect.anchorMax = Vector2.one;
            rematchTextRect.offsetMin = Vector2.zero;
            rematchTextRect.offsetMax = Vector2.zero;

            TextMeshProUGUI rematchTMP = rematchText.AddComponent<TextMeshProUGUI>();
            rematchTMP.text = "REVANCHA";
            rematchTMP.fontSize = 32;
            rematchTMP.color = CYAN_NEON;
            rematchTMP.alignment = TextAlignmentOptions.Center;
            rematchTMP.fontStyle = FontStyles.Bold;
        }
    }
}
