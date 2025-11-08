using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Managers;

namespace DigitPark.UI
{
    /// <summary>
    /// Construye la UI de la escena Settings programáticamente
    /// </summary>
    public class SettingsUIBuilder : MonoBehaviour
    {
        private Canvas canvas;
        private SettingsManager settingsManager;

        private void Awake()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            // IMPORTANTE: Crear SettingsManager PRIMERO
            CreateSettingsManager();

            canvas = UIFactory.CreateCanvas("MainCanvas");

            CreateBackground();
            CreateTitle();
            CreateTabs();
            CreateAccountPanel();
            CreateGamePanel();
            CreateVisualPanel();
            CreateLanguagePanel();
            CreateLogoutPanel();
            CreateBackButton();
        }

        private void CreateSettingsManager()
        {
            GameObject managerObj = new GameObject("SettingsManager");
            settingsManager = managerObj.AddComponent<SettingsManager>();
        }

        private void CreateBackground()
        {
            UIFactory.CreatePanel(canvas.transform, "Background", UIFactory.DarkBG1);
        }

        private void CreateTitle()
        {
            TextMeshProUGUI title = UIFactory.CreateTitle(canvas.transform, "Title", "CONFIGURACIÓN");

            RectTransform titleRT = title.GetComponent<RectTransform>();
            titleRT.anchoredPosition = new Vector2(0, -100);
            titleRT.sizeDelta = new Vector2(800, 100);

            title.outlineWidth = 0.3f;
            title.outlineColor = UIFactory.ElectricBlue;
        }

        private void CreateTabs()
        {
            GameObject tabsPanel = new GameObject("TabsPanel");
            tabsPanel.transform.SetParent(canvas.transform, false);

            RectTransform tabsRT = tabsPanel.AddComponent<RectTransform>();
            tabsRT.anchorMin = new Vector2(0, 0.5f);
            tabsRT.anchorMax = new Vector2(0, 0.5f);
            tabsRT.anchoredPosition = new Vector2(150, 0);
            tabsRT.sizeDelta = new Vector2(200, 750); // Aumentado para 5 botones

            string[] tabNames = { "Cuenta", "Juego", "Visual", "Idioma", "Cerrar Sesión" };
            Button[] tabButtons = new Button[5];

            for (int i = 0; i < tabNames.Length; i++)
            {
                Button tabBtn = UIFactory.CreateButton(
                    tabsPanel.transform,
                    $"{tabNames[i]}Button",
                    tabNames[i].ToUpper(),
                    new Vector2(200, 100),
                    i == 0 ? UIFactory.ElectricBlue : new Color(0.3f, 0.3f, 0.4f)
                );

                RectTransform tabRT = tabBtn.GetComponent<RectTransform>();
                tabRT.anchoredPosition = new Vector2(0, 150 - (i * 120)); // Bajado de 250 a 150
                AddRoundedCorners(tabBtn.gameObject, 15f);

                tabButtons[i] = tabBtn;
            }

            settingsManager.accountButton = tabButtons[0];
            settingsManager.gameButton = tabButtons[1];
            settingsManager.visualButton = tabButtons[2];
            settingsManager.languageButton = tabButtons[3];
            settingsManager.logoutTabButton = tabButtons[4];
        }

        private void CreateAccountPanel()
        {
            GameObject panel = UIFactory.CreatePanelWithSize(
                canvas.transform,
                "AccountPanel",
                new Vector2(700, 1000),
                new Color(0, 0, 0, 0)
            );

            RectTransform panelRT = panel.GetComponent<RectTransform>();
            panelRT.anchoredPosition = new Vector2(200, -650);

            // Username Input
            TMP_InputField usernameInput = UIFactory.CreateInputField(
                panel.transform,
                "UsernameInput",
                "Nombre de usuario"
            );
            RectTransform usernameRT = usernameInput.GetComponent<RectTransform>();
            usernameRT.sizeDelta = new Vector2(600, 70);
            usernameRT.anchoredPosition = new Vector2(0, 400);
            AddRoundedCorners(usernameInput.gameObject, 12f);
            settingsManager.usernameInput = usernameInput;

            // Change Username Button
            Button changeUsernameBtn = UIFactory.CreateButton(
                panel.transform,
                "ChangeUsernameButton",
                "Cambiar nombre",
                new Vector2(600, 70),
                UIFactory.ElectricBlue
            );
            RectTransform changeUsernameRT = changeUsernameBtn.GetComponent<RectTransform>();
            changeUsernameRT.anchoredPosition = new Vector2(0, 300);
            AddRoundedCorners(changeUsernameBtn.gameObject, 15f);
            settingsManager.changeUsernameButton = changeUsernameBtn;

            // User ID Text
            TextMeshProUGUI userIdText = UIFactory.CreateText(
                panel.transform,
                "UserIdText",
                "ID: 123456789",
                24,
                new Color(0.7f, 0.7f, 0.7f),
                TMPro.TextAlignmentOptions.Center
            );
            RectTransform userIdRT = userIdText.GetComponent<RectTransform>();
            userIdRT.anchoredPosition = new Vector2(0, 200);
            userIdRT.sizeDelta = new Vector2(600, 50);
            settingsManager.userIdText = userIdText;

            // Email Text
            TextMeshProUGUI emailText = UIFactory.CreateText(
                panel.transform,
                "EmailText",
                "usuario@email.com",
                24,
                new Color(0.7f, 0.7f, 0.7f),
                TMPro.TextAlignmentOptions.Center
            );
            RectTransform emailRT = emailText.GetComponent<RectTransform>();
            emailRT.anchoredPosition = new Vector2(0, 150);
            emailRT.sizeDelta = new Vector2(600, 50);
            settingsManager.emailText = emailText;

            settingsManager.accountPanel = panel;
        }

        private void CreateGamePanel()
        {
            GameObject panel = UIFactory.CreatePanelWithSize(
                canvas.transform,
                "GamePanel",
                new Vector2(700, 1000),
                new Color(0, 0, 0, 0)
            );

            RectTransform panelRT = panel.GetComponent<RectTransform>();
            panelRT.anchoredPosition = new Vector2(200, 0); // Centrado verticalmente
            panel.SetActive(false);

            float yPos = 350; // Subir los sliders

            // Music Volume
            TextMeshProUGUI musicLabel = UIFactory.CreateText(
                panel.transform,
                "MusicLabel",
                "Volumen Música",
                28,
                Color.white,
                TMPro.TextAlignmentOptions.Center
            );
            musicLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos);
            musicLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 40);

            Slider musicSlider = UIFactory.CreateSlider(panel.transform, "MusicVolumeSlider", 0f, 1f);
            musicSlider.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos - 50);
            musicSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 40);
            settingsManager.musicVolumeSlider = musicSlider;

            TextMeshProUGUI musicVolumeText = UIFactory.CreateText(
                panel.transform,
                "MusicVolumeText",
                "100%",
                24,
                UIFactory.NeonYellow,
                TMPro.TextAlignmentOptions.Center
            );
            musicVolumeText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos - 100);
            musicVolumeText.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 40);
            settingsManager.musicVolumeText = musicVolumeText;

            yPos -= 180;

            // SFX Volume
            TextMeshProUGUI sfxLabel = UIFactory.CreateText(
                panel.transform,
                "SFXLabel",
                "Volumen SFX",
                28,
                Color.white,
                TMPro.TextAlignmentOptions.Center
            );
            sfxLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos);
            sfxLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 40);

            Slider sfxSlider = UIFactory.CreateSlider(panel.transform, "SFXVolumeSlider", 0f, 1f);
            sfxSlider.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos - 50);
            sfxSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 40);
            settingsManager.sfxVolumeSlider = sfxSlider;

            TextMeshProUGUI sfxVolumeText = UIFactory.CreateText(
                panel.transform,
                "SFXVolumeText",
                "100%",
                24,
                UIFactory.NeonYellow,
                TMPro.TextAlignmentOptions.Center
            );
            sfxVolumeText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos - 100);
            sfxVolumeText.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 40);
            settingsManager.sfxVolumeText = sfxVolumeText;

            yPos -= 180;

            // Vibration Toggle
            Toggle vibrationToggle = UIFactory.CreateToggle(panel.transform, "VibrationToggle", "Vibración");
            vibrationToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos);
            settingsManager.vibrationToggle = vibrationToggle;

            yPos -= 80;

            // Notifications Toggle
            Toggle notificationsToggle = UIFactory.CreateToggle(panel.transform, "NotificationsToggle", "Notificaciones");
            notificationsToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos);
            settingsManager.notificationsToggle = notificationsToggle;

            settingsManager.gamePanel = panel;
        }

        private void CreateVisualPanel()
        {
            GameObject panel = UIFactory.CreatePanelWithSize(
                canvas.transform,
                "VisualPanel",
                new Vector2(700, 1000),
                new Color(0, 0, 0, 0)
            );

            RectTransform panelRT = panel.GetComponent<RectTransform>();
            panelRT.anchoredPosition = new Vector2(200, 0); // Centrado verticalmente
            panel.SetActive(false);

            float yPos = 350; // Subir los dropdowns

            // Theme
            TextMeshProUGUI themeLabel = UIFactory.CreateText(
                panel.transform,
                "ThemeLabel",
                "Tema",
                28,
                Color.white,
                TMPro.TextAlignmentOptions.Center
            );
            themeLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos);
            themeLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 40);

            TMP_Dropdown themeDropdown = UIFactory.CreateDropdown(
                panel.transform,
                "ThemeDropdown",
                new[] { "Oscuro", "Claro", "Automático" }
            );
            themeDropdown.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos - 60);
            themeDropdown.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 70);
            AddRoundedCorners(themeDropdown.gameObject, 12f);
            settingsManager.themeDropdown = themeDropdown;

            yPos -= 160;

            // Quality
            TextMeshProUGUI qualityLabel = UIFactory.CreateText(
                panel.transform,
                "QualityLabel",
                "Calidad Gráfica",
                28,
                Color.white,
                TMPro.TextAlignmentOptions.Center
            );
            qualityLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos);
            qualityLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 40);

            TMP_Dropdown qualityDropdown = UIFactory.CreateDropdown(
                panel.transform,
                "QualityDropdown",
                new[] { "Baja", "Media", "Alta", "Ultra" }
            );
            qualityDropdown.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos - 60);
            qualityDropdown.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 70);
            AddRoundedCorners(qualityDropdown.gameObject, 12f);
            settingsManager.qualityDropdown = qualityDropdown;

            yPos -= 160;

            // FPS
            TextMeshProUGUI fpsLabel = UIFactory.CreateText(
                panel.transform,
                "FPSLabel",
                "FPS Objetivo",
                28,
                Color.white,
                TMPro.TextAlignmentOptions.Center
            );
            fpsLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos);
            fpsLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 40);

            TMP_Dropdown fpsDropdown = UIFactory.CreateDropdown(
                panel.transform,
                "FPSDropdown",
                new[] { "30 FPS", "60 FPS", "120 FPS", "Ilimitado" }
            );
            fpsDropdown.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos - 60);
            fpsDropdown.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 70);
            AddRoundedCorners(fpsDropdown.gameObject, 12f);
            settingsManager.fpsDropdown = fpsDropdown;

            settingsManager.visualPanel = panel;
        }

        private void CreateLanguagePanel()
        {
            GameObject panel = UIFactory.CreatePanelWithSize(
                canvas.transform,
                "LanguagePanel",
                new Vector2(700, 1000),
                new Color(0, 0, 0, 0)
            );

            RectTransform panelRT = panel.GetComponent<RectTransform>();
            panelRT.anchoredPosition = new Vector2(200, 0); // Centrado verticalmente
            panel.SetActive(false);

            // Language Label
            TextMeshProUGUI languageLabel = UIFactory.CreateText(
                panel.transform,
                "LanguageLabel",
                "Idioma / Language",
                32,
                Color.white,
                TMPro.TextAlignmentOptions.Center
            );
            languageLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 350); // Subido
            languageLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 50);

            // Language Dropdown
            TMP_Dropdown languageDropdown = UIFactory.CreateDropdown(
                panel.transform,
                "LanguageDropdown",
                new[] { "Español", "English", "Português", "Français", "Deutsch", "日本語", "中文" }
            );
            languageDropdown.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 270); // Subido
            languageDropdown.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 70);
            AddRoundedCorners(languageDropdown.gameObject, 12f);
            settingsManager.languageDropdown = languageDropdown;

            settingsManager.languagePanel = panel;
        }

        private void CreateLogoutPanel()
        {
            GameObject panel = UIFactory.CreatePanelWithSize(
                canvas.transform,
                "LogoutPanel",
                new Vector2(700, 1000),
                new Color(0, 0, 0, 0)
            );

            RectTransform panelRT = panel.GetComponent<RectTransform>();
            panelRT.anchoredPosition = new Vector2(200, 0); // Centrado verticalmente
            panel.SetActive(false);

            // Botón de Cerrar Sesión (centrado)
            Button logoutBtn = UIFactory.CreateButton(
                panel.transform,
                "LogoutButton",
                "CERRAR SESIÓN",
                new Vector2(600, 80),
                UIFactory.CoralRed
            );
            RectTransform logoutRT = logoutBtn.GetComponent<RectTransform>();
            logoutRT.anchoredPosition = new Vector2(0, 0); // Centrado en el panel
            AddRoundedCorners(logoutBtn.gameObject, 20f);
            settingsManager.logoutButton = logoutBtn;

            settingsManager.logoutPanel = panel;
        }

        private void CreateBackButton()
        {
            Button backBtn = UIFactory.CreateButton(
                canvas.transform,
                "BackButton",
                "← VOLVER",
                new Vector2(250, 70),
                new Color(0.3f, 0.3f, 0.4f)
            );

            RectTransform backRT = backBtn.GetComponent<RectTransform>();
            backRT.anchorMin = new Vector2(0, 0);
            backRT.anchorMax = new Vector2(0, 0);
            backRT.anchoredPosition = new Vector2(160, 50);
            AddRoundedCorners(backBtn.gameObject, 15f);

            settingsManager.backButton = backBtn;

            Debug.Log("[SettingsUI] UI construida completamente");
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
