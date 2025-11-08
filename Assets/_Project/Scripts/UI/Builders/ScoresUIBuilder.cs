using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Managers;

namespace DigitPark.UI
{
    /// <summary>
    /// Construye la UI de la escena Scores (Leaderboards) programáticamente
    /// </summary>
    public class ScoresUIBuilder : MonoBehaviour
    {
        private Canvas canvas;
        private LeaderboardManager leaderboardManager;

        private void Awake()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            // IMPORTANTE: Crear LeaderboardManager PRIMERO
            CreateLeaderboardManager();

            canvas = UIFactory.CreateCanvas("MainCanvas");

            CreateBackground();
            CreateTitle();
            CreateTabs();
            CreateLeaderboardScrollView();
            CreatePlayerHighlight();
            CreateBackButton();
        }

        private void CreateLeaderboardManager()
        {
            GameObject managerObj = new GameObject("LeaderboardManager");
            leaderboardManager = managerObj.AddComponent<LeaderboardManager>();
        }

        private void CreateBackground()
        {
            UIFactory.CreatePanel(canvas.transform, "Background", UIFactory.DarkBG1);
        }

        private void CreateTitle()
        {
            TextMeshProUGUI title = UIFactory.CreateTitle(canvas.transform, "Title", "SCORES");

            RectTransform titleRT = title.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 1);
            titleRT.anchorMax = new Vector2(0.5f, 1);
            titleRT.pivot = new Vector2(0.5f, 1);
            titleRT.anchoredPosition = new Vector2(0, -120); // Posición desde arriba
            titleRT.sizeDelta = new Vector2(700, 100);

            title.fontSize = 56;
            title.fontStyle = TMPro.FontStyles.Bold;
            title.alignment = TMPro.TextAlignmentOptions.Center;
            title.outlineWidth = 0.3f;
            title.outlineColor = UIFactory.ElectricBlue;
        }

        private void CreateTabs()
        {
            string[] tabNames = { "PERSONALES", "LOCAL", "MUNDIAL" };
            Button[] tabButtons = new Button[3];

            float buttonWidth = 320f;
            float buttonHeight = 80f;
            float spacing = 20f;
            float totalWidth = (buttonWidth * 3) + (spacing * 2);
            float startX = -totalWidth / 2f + buttonWidth / 2f;

            for (int i = 0; i < tabNames.Length; i++)
            {
                Button tabBtn = UIFactory.CreateButton(
                    canvas.transform,
                    $"{tabNames[i]}Tab",
                    tabNames[i],
                    new Vector2(buttonWidth, buttonHeight),
                    i == 0 ? UIFactory.ElectricBlue : new Color(0.3f, 0.3f, 0.4f)
                );

                RectTransform btnRT = tabBtn.GetComponent<RectTransform>();
                btnRT.anchorMin = new Vector2(0.5f, 1);
                btnRT.anchorMax = new Vector2(0.5f, 1);
                btnRT.pivot = new Vector2(0.5f, 1);
                btnRT.anchoredPosition = new Vector2(startX + (i * (buttonWidth + spacing)), -240);

                // Ajustar tamaño de fuente
                TextMeshProUGUI btnText = tabBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.fontSize = 22;
                    btnText.fontStyle = TMPro.FontStyles.Bold;
                }

                AddRoundedCorners(tabBtn.gameObject, 10f);

                tabButtons[i] = tabBtn;
            }

            leaderboardManager.personalTabButton = tabButtons[0];
            leaderboardManager.localTabButton = tabButtons[1];
            leaderboardManager.globalTabButton = tabButtons[2];
        }

        private void CreateLeaderboardScrollView()
        {
            ScrollRect scrollRect = UIFactory.CreateScrollView(
                canvas.transform,
                "LeaderboardScrollView",
                new Vector2(1000, 1300) // Ancho completo menos márgenes
            );

            RectTransform scrollRT = scrollRect.GetComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0.5f, 0.5f);
            scrollRT.anchorMax = new Vector2(0.5f, 0.5f);
            scrollRT.pivot = new Vector2(0.5f, 0.5f);
            scrollRT.anchoredPosition = new Vector2(0, -200); // Centrado
            scrollRT.sizeDelta = new Vector2(1000, 1300); // Tamaño fijo

            // Fondo oscuro semi-transparente para el ScrollView
            Image scrollBg = scrollRect.gameObject.GetComponent<Image>();
            if (scrollBg == null)
            {
                scrollBg = scrollRect.gameObject.AddComponent<Image>();
            }
            scrollBg.color = new Color(0.1f, 0.1f, 0.15f, 0.8f);

            leaderboardManager.leaderboardContainer = scrollRect.content;
            leaderboardManager.scrollRect = scrollRect;
        }

        private void CreatePlayerHighlight()
        {
            GameObject highlightPanel = UIFactory.CreatePanelWithSize(
                canvas.transform,
                "PlayerHighlightPanel",
                new Vector2(850, 80),
                new Color(0.15f, 0.15f, 0.25f, 0.95f)
            );

            RectTransform highlightRT = highlightPanel.GetComponent<RectTransform>();
            highlightRT.anchorMin = new Vector2(0.5f, 1);
            highlightRT.anchorMax = new Vector2(0.5f, 1);
            highlightRT.pivot = new Vector2(0.5f, 1);
            highlightRT.anchoredPosition = new Vector2(0, -340); // Justo debajo de los tabs
            AddRoundedCorners(highlightPanel, 15f);

            // Ocultar por defecto (se muestra cuando hay datos del jugador)
            highlightPanel.SetActive(false);

            // Position Text
            TextMeshProUGUI positionText = UIFactory.CreateText(
                highlightPanel.transform,
                "PlayerPositionText",
                "#1",
                32,
                UIFactory.NeonYellow,
                TMPro.TextAlignmentOptions.MidlineLeft
            );
            RectTransform posRT = positionText.GetComponent<RectTransform>();
            posRT.anchorMin = Vector2.zero;
            posRT.anchorMax = Vector2.one;
            posRT.anchoredPosition = new Vector2(-320, 0);
            posRT.sizeDelta = new Vector2(100, 0);
            leaderboardManager.playerPositionText = positionText;

            // Player Name
            TextMeshProUGUI nameText = UIFactory.CreateText(
                highlightPanel.transform,
                "PlayerNameText",
                "Tú",
                28,
                Color.white,
                TMPro.TextAlignmentOptions.Center
            );
            RectTransform nameRT = nameText.GetComponent<RectTransform>();
            nameRT.anchorMin = Vector2.zero;
            nameRT.anchorMax = Vector2.one;
            nameRT.anchoredPosition = new Vector2(-50, 0);
            nameRT.sizeDelta = new Vector2(300, 0);

            // Time Text
            TextMeshProUGUI timeText = UIFactory.CreateText(
                highlightPanel.transform,
                "PlayerTimeText",
                "0.000s",
                28,
                UIFactory.BrightGreen,
                TMPro.TextAlignmentOptions.MidlineRight
            );
            RectTransform timeRT = timeText.GetComponent<RectTransform>();
            timeRT.anchorMin = Vector2.zero;
            timeRT.anchorMax = Vector2.one;
            timeRT.anchoredPosition = new Vector2(320, 0);
            timeRT.sizeDelta = new Vector2(150, 0);
            leaderboardManager.playerTimeText = timeText;

            leaderboardManager.playerHighlightPanel = highlightPanel;
        }

        private void CreateBackButton()
        {
            Button backBtn = UIFactory.CreateButton(
                canvas.transform,
                "BackButton",
                "← VOLVER",
                new Vector2(350, 80),
                new Color(0.3f, 0.3f, 0.4f)
            );

            RectTransform backRT = backBtn.GetComponent<RectTransform>();
            backRT.anchorMin = new Vector2(0.5f, 0);
            backRT.anchorMax = new Vector2(0.5f, 0);
            backRT.pivot = new Vector2(0.5f, 0);
            backRT.anchoredPosition = new Vector2(0, 80); // Centrado abajo con margen

            // Ajustar texto del botón
            TextMeshProUGUI btnText = backBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.fontSize = 28;
                btnText.fontStyle = TMPro.FontStyles.Bold;
            }

            AddRoundedCorners(backBtn.gameObject, 15f);

            leaderboardManager.backButton = backBtn;

            Debug.Log("[ScoresUI] UI construida completamente");
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
