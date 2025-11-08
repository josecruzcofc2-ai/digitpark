using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Managers;

namespace DigitPark.UI
{
    /// <summary>
    /// Construye la UI de la escena Game programáticamente
    /// Grid 3x3 con números aleatorios 1-9
    /// </summary>
    public class GameUIBuilder : MonoBehaviour
    {
        private Canvas canvas;
        private GameManager gameManager;

        private void Awake()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            // IMPORTANTE: Crear GameManager PRIMERO
            SetupGameManager();

            canvas = UIFactory.CreateCanvas("MainCanvas");

            CreateBackground();
            CreateHUD();
            CreateGrid();
            CreatePlayAgainButton();
            CreateWinMessage();
        }

        private void SetupGameManager()
        {
            GameObject managerObj = GameObject.Find("GameManager");
            if (managerObj == null)
            {
                managerObj = new GameObject("GameManager");
                gameManager = managerObj.AddComponent<GameManager>();
            }
            else
            {
                gameManager = managerObj.GetComponent<GameManager>();
            }
        }

        private void CreateBackground()
        {
            UIFactory.CreatePanel(canvas.transform, "Background", UIFactory.DarkBG1);
        }

        private void CreateHUD()
        {
            GameObject hudPanel = new GameObject("HUDPanel");
            hudPanel.transform.SetParent(canvas.transform, false);

            RectTransform rt = hudPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, 0);
            rt.sizeDelta = new Vector2(0, 120);

            // Timer Text (centro superior)
            TextMeshProUGUI timerText = UIFactory.CreateText(
                hudPanel.transform,
                "TimerText",
                "0.000s",
                56,
                UIFactory.NeonYellow,
                TMPro.TextAlignmentOptions.Center
            );
            RectTransform timerRT = timerText.GetComponent<RectTransform>();
            timerRT.anchoredPosition = new Vector2(0, -50);
            timerRT.sizeDelta = new Vector2(400, 70);
            gameManager.timerText = timerText;

            // Best Time Text (esquina superior derecha)
            TextMeshProUGUI bestTimeHUD = UIFactory.CreateText(
                hudPanel.transform,
                "BestTimeText",
                "Mejor: --",
                28,
                UIFactory.BrightGreen,
                TMPro.TextAlignmentOptions.TopRight
            );
            RectTransform bestTimeRT = bestTimeHUD.GetComponent<RectTransform>();
            bestTimeRT.anchorMin = new Vector2(1, 1);
            bestTimeRT.anchorMax = new Vector2(1, 1);
            bestTimeRT.anchoredPosition = new Vector2(-30, -30);
            bestTimeRT.sizeDelta = new Vector2(250, 50);
            gameManager.bestTimeText = bestTimeHUD;
        }

        private void CreateGrid()
        {
            GameObject gridPanel = new GameObject("GridPanel");
            gridPanel.transform.SetParent(canvas.transform, false);

            RectTransform gridRT = gridPanel.AddComponent<RectTransform>();
            gridRT.anchorMin = new Vector2(0.5f, 0.5f);
            gridRT.anchorMax = new Vector2(0.5f, 0.5f);
            gridRT.pivot = new Vector2(0.5f, 0.5f);
            gridRT.anchoredPosition = new Vector2(0, 0); // Centrado
            gridRT.sizeDelta = new Vector2(600, 600); // Grid 600x600

            // Crear GridLayoutGroup
            GridLayoutGroup gridLayout = gridPanel.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(190, 190); // Tamaño de cada celda
            gridLayout.spacing = new Vector2(15, 15); // Espaciado entre celdas
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 3; // 3 columnas
            gridLayout.childAlignment = TextAnchor.MiddleCenter;

            // Crear 9 botones (grid 3x3)
            gameManager.gridButtons = new Button[9];

            for (int i = 0; i < 9; i++)
            {
                GameObject cellObj = new GameObject($"Cell_{i}");
                cellObj.transform.SetParent(gridPanel.transform, false);

                // Agregar Image para el fondo del botón
                Image cellBg = cellObj.AddComponent<Image>();
                cellBg.color = new Color(0.15f, 0.15f, 0.25f); // Azul oscuro

                // Agregar Button
                Button cellButton = cellObj.AddComponent<Button>();

                // Configurar colores del botón
                ColorBlock colors = cellButton.colors;
                colors.normalColor = new Color(0.15f, 0.15f, 0.25f);
                colors.highlightedColor = new Color(0.25f, 0.25f, 0.35f);
                colors.pressedColor = new Color(0, 0.83f, 1f); // ElectricBlue
                colors.selectedColor = new Color(0.2f, 0.7f, 0.3f); // Verde cuando correcto
                colors.disabledColor = new Color(0.1f, 0.1f, 0.15f);
                cellButton.colors = colors;

                // Agregar Outline
                Outline outline = cellObj.AddComponent<Outline>();
                outline.effectColor = new Color(0, 0, 0, 0.3f);
                outline.effectDistance = new Vector2(3, -3);

                // Texto del número (inicialmente vacío)
                GameObject textObj = new GameObject("NumberText");
                textObj.transform.SetParent(cellObj.transform, false);

                RectTransform textRT = textObj.AddComponent<RectTransform>();
                textRT.anchorMin = Vector2.zero;
                textRT.anchorMax = Vector2.one;
                textRT.sizeDelta = Vector2.zero;

                TextMeshProUGUI numberText = textObj.AddComponent<TextMeshProUGUI>();
                numberText.text = ""; // Se llenará con números aleatorios
                numberText.fontSize = 80;
                numberText.fontStyle = FontStyles.Bold;
                numberText.color = Color.white;
                numberText.alignment = TMPro.TextAlignmentOptions.Center;

                // Guardar referencia
                gameManager.gridButtons[i] = cellButton;
            }
        }

        private void CreatePlayAgainButton()
        {
            Button playAgainBtn = UIFactory.CreateButton(
                canvas.transform,
                "PlayAgainButton",
                "JUGAR DE NUEVO",
                new Vector2(450, 70),
                UIFactory.ElectricBlue
            );

            RectTransform playAgainRT = playAgainBtn.GetComponent<RectTransform>();
            playAgainRT.anchorMin = new Vector2(0.5f, 0);
            playAgainRT.anchorMax = new Vector2(0.5f, 0);
            playAgainRT.anchoredPosition = new Vector2(0, 80);
            AddRoundedCorners(playAgainBtn.gameObject, 15f);

            gameManager.playAgainButton = playAgainBtn;
        }

        private void CreateWinMessage()
        {
            // Panel de fondo para el mensaje GOOD
            GameObject winPanel = new GameObject("WinMessagePanel");
            winPanel.transform.SetParent(canvas.transform, false);

            RectTransform winRT = winPanel.AddComponent<RectTransform>();
            winRT.anchorMin = Vector2.zero;
            winRT.anchorMax = Vector2.one;
            winRT.sizeDelta = Vector2.zero;

            // CanvasGroup para animación de fade
            CanvasGroup canvasGroup = winPanel.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0; // Invisible inicialmente
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            // Texto GOOD gigante
            TextMeshProUGUI winText = UIFactory.CreateText(
                winPanel.transform,
                "WinText",
                "GOOD!",
                120,
                UIFactory.BrightGreen,
                TMPro.TextAlignmentOptions.Center
            );
            RectTransform winTextRT = winText.GetComponent<RectTransform>();
            winTextRT.anchoredPosition = Vector2.zero;
            winTextRT.sizeDelta = new Vector2(800, 200);
            winText.fontStyle = FontStyles.Bold;
            winText.outlineWidth = 0.3f;
            winText.outlineColor = new Color(0, 0, 0, 0.5f);

            gameManager.winMessagePanel = winPanel;
            gameManager.winMessageCanvasGroup = canvasGroup;

            Debug.Log("[GameUI] UI construida completamente - Grid 3x3");
        }

        private void AddRoundedCorners(GameObject target, float radius)
        {
            Image image = target.GetComponent<Image>();
            if (image != null)
            {
                Outline outline = target.GetComponent<Outline>();
                if (outline == null)
                {
                    outline = target.AddComponent<Outline>();
                }
                outline.effectColor = new Color(0, 0, 0, 0.2f);
                outline.effectDistance = new Vector2(1, -1);
            }
        }
    }
}
