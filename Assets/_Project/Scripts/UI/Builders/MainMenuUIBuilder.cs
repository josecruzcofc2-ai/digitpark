using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Managers;

namespace DigitPark.UI
{
    /// <summary>
    /// Construye la UI de la escena MainMenu programáticamente
    /// </summary>
    public class MainMenuUIBuilder : MonoBehaviour
    {
        private Canvas canvas;
        private MainMenuManager mainMenuManager;

        private void Awake()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            // IMPORTANTE: Crear MainMenuManager PRIMERO
            CreateMainMenuManager();

            canvas = UIFactory.CreateCanvas("MainCanvas");

            CreateBackground();
            CreateTitle();
            CreatePlayerInfo();
            CreateMainButtons();
            CreateSecondaryButtons();
        }

        private void CreateMainMenuManager()
        {
            GameObject managerObj = new GameObject("MainMenuManager");
            mainMenuManager = managerObj.AddComponent<MainMenuManager>();
        }

        private void CreateBackground()
        {
            UIFactory.CreatePanel(canvas.transform, "Background", UIFactory.DarkBG1);
        }

        private void CreateTitle()
        {
            TextMeshProUGUI title = UIFactory.CreateTitle(canvas.transform, "Title", "DIGIT PARK");

            RectTransform titleRT = title.GetComponent<RectTransform>();
            titleRT.anchoredPosition = new Vector2(0, -120);
            titleRT.sizeDelta = new Vector2(600, 100);

            title.outlineWidth = 0.3f;
            title.outlineColor = UIFactory.ElectricBlue;

            mainMenuManager.titleText = title;
        }

        private void CreatePlayerInfo()
        {
            GameObject infoPanel = new GameObject("PlayerInfoPanel");
            infoPanel.transform.SetParent(canvas.transform, false);

            RectTransform rt = infoPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, -20);
            rt.sizeDelta = new Vector2(0, 150);

            // Username como botón clickeable
            Button usernameButton = UIFactory.CreateButton(
                infoPanel.transform,
                "UsernameButton",
                "Sin usuario",
                new Vector2(250, 45),
                new Color(0.2f, 0.2f, 0.3f, 0.5f) // Semi-transparente
            );
            RectTransform userBtnRT = usernameButton.GetComponent<RectTransform>();
            userBtnRT.anchorMin = new Vector2(0, 1);
            userBtnRT.anchorMax = new Vector2(0, 1);
            userBtnRT.anchoredPosition = new Vector2(165, -30); // Movido de 40 a 165 (más a la derecha)
            AddRoundedCorners(usernameButton.gameObject, 10f);

            // Ajustar el texto del botón
            TextMeshProUGUI usernameText = usernameButton.GetComponentInChildren<TextMeshProUGUI>();
            if (usernameText != null)
            {
                usernameText.fontSize = 28;
                usernameText.alignment = TMPro.TextAlignmentOptions.Left;
                RectTransform textRT = usernameText.GetComponent<RectTransform>();
                textRT.offsetMin = new Vector2(15, 0); // Padding izquierdo
                textRT.offsetMax = new Vector2(-15, 0); // Padding derecho
            }

            mainMenuManager.usernameButton = usernameButton;
            mainMenuManager.usernameText = usernameText;

            // Level, Coins y BestTime ELIMINADOS según solicitud del usuario
        }

        private void CreateMainButtons()
        {
            Vector2 buttonSize = new Vector2(500, 60);
            float buttonRadius = 20f;

            // Botón PLAY principal (grande y destacado) - CENTRADO
            Button playBtn = UIFactory.CreateButton(
                canvas.transform,
                "PlayButton",
                "JUGAR",
                new Vector2(550, 80),
                UIFactory.ElectricBlue
            );
            RectTransform playRT = playBtn.GetComponent<RectTransform>();
            playRT.anchoredPosition = new Vector2(0, 100); // Centrado en la mitad superior
            AddRoundedCorners(playBtn.gameObject, 25f);

            // Hacer el texto más grande
            TextMeshProUGUI playText = playBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (playText != null)
            {
                playText.fontSize = 42;
                playText.fontStyle = FontStyles.Bold;
            }

            mainMenuManager.playButton = playBtn;

            // Botón Scores
            Button scoresBtn = UIFactory.CreateButton(
                canvas.transform,
                "ScoresButton",
                "CLASIFICACIÓN",
                buttonSize,
                new Color(0.3f, 0.3f, 0.4f)
            );
            RectTransform scoresRT = scoresBtn.GetComponent<RectTransform>();
            scoresRT.anchoredPosition = new Vector2(0, 0); // Centrado exacto
            AddRoundedCorners(scoresBtn.gameObject, buttonRadius);
            mainMenuManager.scoresButton = scoresBtn;

            // Botón Tournaments
            Button tournamentsBtn = UIFactory.CreateButton(
                canvas.transform,
                "TournamentsButton",
                "TORNEOS",
                buttonSize,
                new Color(0.3f, 0.3f, 0.4f)
            );
            RectTransform tournamentsRT = tournamentsBtn.GetComponent<RectTransform>();
            tournamentsRT.anchoredPosition = new Vector2(0, -100); // Centrado en la mitad inferior
            AddRoundedCorners(tournamentsBtn.gameObject, buttonRadius);
            mainMenuManager.tournamentsButton = tournamentsBtn;

            // Botón Settings
            Button settingsBtn = UIFactory.CreateButton(
                canvas.transform,
                "SettingsButton",
                "CONFIGURACIÓN",
                buttonSize,
                new Color(0.3f, 0.3f, 0.4f)
            );
            RectTransform settingsRT = settingsBtn.GetComponent<RectTransform>();
            settingsRT.anchoredPosition = new Vector2(0, -200); // Más abajo
            AddRoundedCorners(settingsBtn.gameObject, buttonRadius);
            mainMenuManager.settingsButton = settingsBtn;
        }

        private void CreateSecondaryButtons()
        {
            // ProfileButton y DailyRewardButton ELIMINADOS según solicitud del usuario
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
