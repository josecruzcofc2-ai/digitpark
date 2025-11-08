using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Managers;

namespace DigitPark.UI
{
    /// <summary>
    /// Construye la UI de la escena Tournaments programáticamente
    /// </summary>
    public class TournamentsUIBuilder : MonoBehaviour
    {
        private Canvas canvas;
        private TournamentManager tournamentManager;

        private void Awake()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            // IMPORTANTE: Crear TournamentManager PRIMERO
            CreateTournamentManager();

            canvas = UIFactory.CreateCanvas("MainCanvas");

            CreateBackground();
            CreateTitle();
            CreateTabs();
            CreateTournamentsScrollView();
            CreateCreateTournamentPanel();
            CreateBackButton();
        }

        private void CreateTournamentManager()
        {
            GameObject managerObj = new GameObject("TournamentManager");
            tournamentManager = managerObj.AddComponent<TournamentManager>();
        }

        private void CreateBackground()
        {
            UIFactory.CreatePanel(canvas.transform, "Background", UIFactory.DarkBG1);
        }

        private void CreateTitle()
        {
            TextMeshProUGUI title = UIFactory.CreateTitle(canvas.transform, "Title", "TORNEOS");

            RectTransform titleRT = title.GetComponent<RectTransform>();
            titleRT.anchoredPosition = new Vector2(0, -100);
            titleRT.sizeDelta = new Vector2(600, 100);

            title.outlineWidth = 0.3f;
            title.outlineColor = UIFactory.ElectricBlue;
        }

        private void CreateTabs()
        {
            // Panel que ocupa todo el ancho de la pantalla
            GameObject tabsPanel = new GameObject("TabsPanel");
            tabsPanel.transform.SetParent(canvas.transform, false);

            RectTransform tabsRT = tabsPanel.AddComponent<RectTransform>();
            tabsRT.anchorMin = new Vector2(0, 1);
            tabsRT.anchorMax = new Vector2(1, 1);
            tabsRT.pivot = new Vector2(0.5f, 1);
            tabsRT.anchoredPosition = new Vector2(0, -200);
            tabsRT.sizeDelta = new Vector2(0, 100); // Altura fija, ancho completo

            string[] tabNames = { "TORNEOS ACTIVOS", "MIS TORNEOS", "CREAR TORNEO" };
            Button[] tabButtons = new Button[3];

            // Ancho de cada botón = ancho total / 3
            float buttonWidth = 1080f / 3f; // 360 pixels cada uno

            for (int i = 0; i < tabNames.Length; i++)
            {
                Button tabBtn = UIFactory.CreateButton(
                    tabsPanel.transform,
                    $"{tabNames[i]}Tab",
                    tabNames[i],
                    new Vector2(buttonWidth - 10, 80), // -10 para pequeño espacio entre botones
                    i == 0 ? UIFactory.ElectricBlue : new Color(0.3f, 0.3f, 0.4f)
                );

                RectTransform tabRT = tabBtn.GetComponent<RectTransform>();
                tabRT.anchorMin = new Vector2(i / 3f, 0.5f);
                tabRT.anchorMax = new Vector2(i / 3f, 0.5f);
                tabRT.pivot = new Vector2(0, 0.5f);
                tabRT.anchoredPosition = new Vector2(5, 0); // 5 pixels de padding

                // Ajustar tamaño de fuente para que quepa
                TextMeshProUGUI btnText = tabBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.fontSize = 24;
                    btnText.enableAutoSizing = true;
                    btnText.fontSizeMin = 18;
                    btnText.fontSizeMax = 24;
                }

                AddRoundedCorners(tabBtn.gameObject, 12f);

                tabButtons[i] = tabBtn;
            }

            tournamentManager.activeTournamentsTab = tabButtons[0];
            tournamentManager.myTournamentsTab = tabButtons[1];
            tournamentManager.createTournamentTab = tabButtons[2];
        }

        private void CreateTournamentsScrollView()
        {
            ScrollRect scrollRect = UIFactory.CreateScrollView(
                canvas.transform,
                "TournamentsScrollView",
                new Vector2(1060, 1450) // Ocupa casi todo el ancho y altura disponible
            );

            RectTransform scrollRT = scrollRect.GetComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0.5f, 0);
            scrollRT.anchorMax = new Vector2(0.5f, 1);
            scrollRT.pivot = new Vector2(0.5f, 1);
            scrollRT.anchoredPosition = new Vector2(0, -310); // Justo debajo de los tabs
            scrollRT.sizeDelta = new Vector2(1060, -360); // Se adapta al espacio disponible

            tournamentManager.tournamentsContainer = scrollRect.content;
            tournamentManager.scrollRect = scrollRect;
        }

        private void CreateCreateTournamentPanel()
        {
            GameObject createPanel = UIFactory.CreatePanelWithSize(
                canvas.transform,
                "CreateTournamentPanel",
                new Vector2(1060, 1450),
                new Color(0.05f, 0.05f, 0.15f, 0.98f)
            );

            RectTransform panelRT = createPanel.GetComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.5f, 0);
            panelRT.anchorMax = new Vector2(0.5f, 1);
            panelRT.pivot = new Vector2(0.5f, 1);
            panelRT.anchoredPosition = new Vector2(0, -310); // Alineado con el scroll view
            panelRT.sizeDelta = new Vector2(1060, -360);
            createPanel.SetActive(false);
            AddRoundedCorners(createPanel, 20f);

            // Title
            TextMeshProUGUI titleText = UIFactory.CreateText(
                createPanel.transform,
                "CreateTitle",
                "CREAR TORNEO",
                40,
                UIFactory.ElectricBlue,
                TMPro.TextAlignmentOptions.Center
            );
            RectTransform titleRT = titleText.GetComponent<RectTransform>();
            titleRT.anchoredPosition = new Vector2(0, 400);
            titleRT.sizeDelta = new Vector2(700, 60);

            // Tournament Name Input
            TMP_InputField nameInput = UIFactory.CreateInputField(
                createPanel.transform,
                "TournamentNameInput",
                "Nombre del torneo"
            );
            RectTransform nameRT = nameInput.GetComponent<RectTransform>();
            nameRT.sizeDelta = new Vector2(700, 70);
            nameRT.anchoredPosition = new Vector2(0, 300);
            AddRoundedCorners(nameInput.gameObject, 12f);
            tournamentManager.tournamentNameInput = nameInput;

            // Entry Fee Input
            TMP_InputField entryFeeInput = UIFactory.CreateInputField(
                createPanel.transform,
                "EntryFeeInput",
                "Costo de entrada (monedas)"
            );
            RectTransform feeRT = entryFeeInput.GetComponent<RectTransform>();
            feeRT.sizeDelta = new Vector2(700, 70);
            feeRT.anchoredPosition = new Vector2(0, 200);
            AddRoundedCorners(entryFeeInput.gameObject, 12f);
            tournamentManager.entryFeeInput = entryFeeInput;

            // Max Participants Input
            TMP_InputField maxParticipantsInput = UIFactory.CreateInputField(
                createPanel.transform,
                "MaxParticipantsInput",
                "Máximo de participantes"
            );
            RectTransform maxParticipantsRT = maxParticipantsInput.GetComponent<RectTransform>();
            maxParticipantsRT.sizeDelta = new Vector2(700, 70);
            maxParticipantsRT.anchoredPosition = new Vector2(0, 100);
            AddRoundedCorners(maxParticipantsInput.gameObject, 12f);
            tournamentManager.maxParticipantsInput = maxParticipantsInput;

            // Duration Dropdown
            TMP_Dropdown durationDropdown = UIFactory.CreateDropdown(
                createPanel.transform,
                "DurationDropdown",
                new[] { "1 Hora", "6 Horas", "24 Horas", "3 Días", "1 Semana" }
            );
            RectTransform durationRT = durationDropdown.GetComponent<RectTransform>();
            durationRT.sizeDelta = new Vector2(700, 70);
            durationRT.anchoredPosition = new Vector2(0, 0);
            AddRoundedCorners(durationDropdown.gameObject, 12f);
            tournamentManager.durationDropdown = durationDropdown;

            // Region Dropdown
            TMP_Dropdown regionDropdown = UIFactory.CreateDropdown(
                createPanel.transform,
                "RegionDropdown",
                new[] { "Global", "América", "Europa", "Asia", "Oceanía" }
            );
            RectTransform regionRT = regionDropdown.GetComponent<RectTransform>();
            regionRT.sizeDelta = new Vector2(700, 70);
            regionRT.anchoredPosition = new Vector2(0, -100);
            AddRoundedCorners(regionDropdown.gameObject, 12f);
            tournamentManager.regionDropdown = regionDropdown;

            // Create Button
            Button createBtn = UIFactory.CreateButton(
                createPanel.transform,
                "CreateButton",
                "CONFIRMAR CREACIÓN",
                new Vector2(600, 80),
                UIFactory.ElectricBlue
            );
            RectTransform createBtnRT = createBtn.GetComponent<RectTransform>();
            createBtnRT.anchoredPosition = new Vector2(0, -250);
            AddRoundedCorners(createBtn.gameObject, 20f);
            tournamentManager.createButton = createBtn;

            // Cancel Button
            Button cancelBtn = UIFactory.CreateButton(
                createPanel.transform,
                "CancelCreateButton",
                "CANCELAR",
                new Vector2(500, 70),
                new Color(0.4f, 0.2f, 0.2f)
            );
            RectTransform cancelBtnRT = cancelBtn.GetComponent<RectTransform>();
            cancelBtnRT.anchoredPosition = new Vector2(0, -350);
            AddRoundedCorners(cancelBtn.gameObject, 15f);
            tournamentManager.cancelCreateButton = cancelBtn;

            tournamentManager.createTournamentPanel = createPanel;
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

            tournamentManager.backButton = backBtn;

            Debug.Log("[TournamentsUI] UI construida completamente");
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
