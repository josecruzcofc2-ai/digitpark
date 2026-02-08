using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DigitPark.Services;
using DigitPark.Localization;

namespace DigitPark.UI.CashBattle
{
    /// <summary>
    /// Overlay persistente que bloquea el acceso a Cash Battle en ubicaciones restringidas.
    /// Se mantiene activo en todas las escenas de Cash Battle.
    /// </summary>
    public class LocationRestrictionOverlay : MonoBehaviour
    {
        public static LocationRestrictionOverlay Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject overlayPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private TextMeshProUGUI stateText;
        [SerializeField] private Button viewStatesButton;
        [SerializeField] private Button backToMenuButton;
        [SerializeField] private GameObject statesListPanel;
        [SerializeField] private TextMeshProUGUI statesListText;

        // Escenas de Cash Battle donde se debe verificar ubicacion
        private static readonly string[] CASH_BATTLE_SCENES = new string[]
        {
            "CashBattleHub",
            "CashBattle1v1",
            "CashTournaments",
            "CashWallet",
            "CashHistory",
            "AgeVerification"
        };

        private Canvas overlayCanvas;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SetupOverlay();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Suscribirse a cambios de escena
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Setup button listeners
            if (viewStatesButton) viewStatesButton.onClick.AddListener(ToggleStatesList);
            if (backToMenuButton) backToMenuButton.onClick.AddListener(BackToMainMenu);

            // Suscribirse a cambios de ubicacion
            if (LocationRestrictionService.Instance != null)
            {
                LocationRestrictionService.Instance.OnLocationChecked += OnLocationChecked;
            }

            // Ocultar inicialmente
            HideOverlay();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (LocationRestrictionService.Instance != null)
            {
                LocationRestrictionService.Instance.OnLocationChecked -= OnLocationChecked;
            }
        }

        private void SetupOverlay()
        {
            // Asegurar que tenemos un Canvas propio con sorting order alto
            overlayCanvas = GetComponent<Canvas>();
            if (overlayCanvas == null)
            {
                overlayCanvas = gameObject.AddComponent<Canvas>();
            }
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = 9999; // Siempre encima de todo

            // Canvas Scaler
            var scaler = GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = gameObject.AddComponent<CanvasScaler>();
            }
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            // Graphic Raycaster
            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            // Populate states list
            if (statesListText != null)
            {
                statesListText.text = string.Join("\n", LocationRestrictionService.ALLOWED_STATES);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Verificar si estamos en una escena de Cash Battle
            bool isCashBattleScene = IsCashBattleScene(scene.name);

            if (isCashBattleScene)
            {
                // Verificar ubicacion
                CheckAndShowIfRestricted();
            }
            else
            {
                // Ocultar overlay en escenas no-CashBattle
                HideOverlay();
            }
        }

        private bool IsCashBattleScene(string sceneName)
        {
            foreach (string cashScene in CASH_BATTLE_SCENES)
            {
                if (sceneName.Contains(cashScene))
                {
                    return true;
                }
            }
            return false;
        }

        private void CheckAndShowIfRestricted()
        {
            var locationService = LocationRestrictionService.Instance;
            if (locationService == null)
            {
                Debug.LogWarning("[LocationRestrictionOverlay] LocationRestrictionService no encontrado");
                return;
            }

            // Si ya conocemos la ubicacion
            if (locationService.IsLocationKnown)
            {
                if (locationService.IsRestricted)
                {
                    ShowOverlay(locationService.CurrentState);
                }
                else
                {
                    HideOverlay();
                }
            }
            else
            {
                // Verificar ubicacion
                locationService.CheckLocation();
            }
        }

        private void OnLocationChecked(bool isRestricted, string state)
        {
            if (isRestricted)
            {
                ShowOverlay(state);
            }
            else
            {
                HideOverlay();
            }
        }

        public void ShowOverlay(string restrictedState)
        {
            if (overlayPanel != null)
            {
                overlayPanel.SetActive(true);
            }
            else
            {
                // Si no hay panel asignado, activar el objeto completo
                gameObject.SetActive(true);
            }

            // Actualizar textos
            if (stateText != null)
            {
                stateText.text = AutoLocalizer.Get("location_current_state", restrictedState);
            }

            if (statesListPanel != null)
            {
                statesListPanel.SetActive(false);
            }

            Debug.Log($"[LocationRestrictionOverlay] Mostrando overlay - Estado restringido: {restrictedState}");
        }

        public void HideOverlay()
        {
            if (overlayPanel != null)
            {
                overlayPanel.SetActive(false);
            }

            if (statesListPanel != null)
            {
                statesListPanel.SetActive(false);
            }
        }

        private void ToggleStatesList()
        {
            if (statesListPanel != null)
            {
                statesListPanel.SetActive(!statesListPanel.activeSelf);

                // Actualizar texto del boton
                if (viewStatesButton != null)
                {
                    var btnText = viewStatesButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (btnText != null)
                    {
                        btnText.text = statesListPanel.activeSelf ? AutoLocalizer.Get("location_hide_states") : AutoLocalizer.Get("location_show_states");
                    }
                }
            }
        }

        private void BackToMainMenu()
        {
            HideOverlay();
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>
        /// Crea el overlay programaticamente si no existe en la escena
        /// </summary>
        public static LocationRestrictionOverlay CreateIfNeeded()
        {
            if (Instance != null) return Instance;

            GameObject overlayObj = new GameObject("LocationRestrictionOverlay");
            var overlay = overlayObj.AddComponent<LocationRestrictionOverlay>();

            // Crear UI basica programaticamente
            CreateOverlayUI(overlayObj);

            return overlay;
        }

        private static void CreateOverlayUI(GameObject root)
        {
            // Canvas ya se configura en SetupOverlay()

            // Panel de fondo oscuro
            GameObject panelObj = new GameObject("OverlayPanel");
            panelObj.transform.SetParent(root.transform, false);

            RectTransform panelRT = panelObj.AddComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.sizeDelta = Vector2.zero;

            Image panelBg = panelObj.AddComponent<Image>();
            panelBg.color = new Color(0.05f, 0.05f, 0.1f, 0.98f);

            var overlay = root.GetComponent<LocationRestrictionOverlay>();
            overlay.overlayPanel = panelObj;

            // Content container
            GameObject content = new GameObject("Content");
            content.transform.SetParent(panelObj.transform, false);

            RectTransform contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0.1f, 0.2f);
            contentRT.anchorMax = new Vector2(0.9f, 0.8f);
            contentRT.sizeDelta = Vector2.zero;

            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 25;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = false;

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(content.transform, false);
            TextMeshProUGUI iconText = iconObj.AddComponent<TextMeshProUGUI>();
            iconText.text = "📍";
            iconText.fontSize = 72;
            iconText.alignment = TextAlignmentOptions.Center;
            LayoutElement iconLE = iconObj.AddComponent<LayoutElement>();
            iconLE.preferredHeight = 90;

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(content.transform, false);
            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.text = AutoLocalizer.Get("location_unavailable_title");
            title.fontSize = 36;
            title.color = new Color(1f, 0.4f, 0.4f);
            title.alignment = TextAlignmentOptions.Center;
            title.fontStyle = FontStyles.Bold;
            overlay.titleText = title;
            LayoutElement titleLE = titleObj.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 50;

            // Message
            GameObject msgObj = new GameObject("Message");
            msgObj.transform.SetParent(content.transform, false);
            TextMeshProUGUI msg = msgObj.AddComponent<TextMeshProUGUI>();
            msg.text = AutoLocalizer.Get("location_unavailable_message");
            msg.fontSize = 22;
            msg.color = new Color(0.8f, 0.8f, 0.8f);
            msg.alignment = TextAlignmentOptions.Center;
            msg.enableWordWrapping = true;
            overlay.messageText = msg;
            LayoutElement msgLE = msgObj.AddComponent<LayoutElement>();
            msgLE.preferredHeight = 100;

            // State text
            GameObject stateObj = new GameObject("StateText");
            stateObj.transform.SetParent(content.transform, false);
            TextMeshProUGUI stateText = stateObj.AddComponent<TextMeshProUGUI>();
            stateText.text = AutoLocalizer.Get("location_current_state", "Florida");
            stateText.fontSize = 20;
            stateText.color = new Color(1f, 0.84f, 0f);
            stateText.alignment = TextAlignmentOptions.Center;
            overlay.stateText = stateText;
            LayoutElement stateLE = stateObj.AddComponent<LayoutElement>();
            stateLE.preferredHeight = 35;

            // View States Button
            GameObject viewBtn = CreateButton("ViewStatesButton", content.transform, AutoLocalizer.Get("location_show_states"), new Color(0f, 0.6f, 0.8f));
            overlay.viewStatesButton = viewBtn.GetComponent<Button>();

            // States List Panel (hidden by default)
            GameObject statesPanel = new GameObject("StatesListPanel");
            statesPanel.transform.SetParent(content.transform, false);
            statesPanel.SetActive(false);

            RectTransform statesPanelRT = statesPanel.AddComponent<RectTransform>();
            Image statesPanelBg = statesPanel.AddComponent<Image>();
            statesPanelBg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

            LayoutElement statesPanelLE = statesPanel.AddComponent<LayoutElement>();
            statesPanelLE.preferredHeight = 300;

            // Scroll view for states
            GameObject statesTextObj = new GameObject("StatesText");
            statesTextObj.transform.SetParent(statesPanel.transform, false);
            RectTransform statesTextRT = statesTextObj.AddComponent<RectTransform>();
            statesTextRT.anchorMin = Vector2.zero;
            statesTextRT.anchorMax = Vector2.one;
            statesTextRT.offsetMin = new Vector2(20, 20);
            statesTextRT.offsetMax = new Vector2(-20, -20);

            TextMeshProUGUI statesListTxt = statesTextObj.AddComponent<TextMeshProUGUI>();
            statesListTxt.text = string.Join("\n", LocationRestrictionService.ALLOWED_STATES);
            statesListTxt.fontSize = 16;
            statesListTxt.color = new Color(0.7f, 1f, 0.8f);
            statesListTxt.alignment = TextAlignmentOptions.TopLeft;
            overlay.statesListText = statesListTxt;
            overlay.statesListPanel = statesPanel;

            // Back Button
            GameObject backBtn = CreateButton("BackButton", content.transform, AutoLocalizer.Get("location_back_to_menu"), new Color(0.8f, 0.2f, 0.2f));
            overlay.backToMenuButton = backBtn.GetComponent<Button>();
        }

        private static GameObject CreateButton(string name, Transform parent, string text, Color bgColor)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.preferredHeight = 55;
            le.preferredWidth = 350;

            Image bg = btnObj.AddComponent<Image>();
            bg.color = bgColor;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;

            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.text = text;
            btnText.fontSize = 20;
            btnText.color = Color.white;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.fontStyle = FontStyles.Bold;

            return btnObj;
        }
    }
}
