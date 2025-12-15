using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Managers;
using DigitPark.Localization;

namespace DigitPark.UI.Panels
{
    /// <summary>
    /// Panel de Premium reutilizable que muestra las opciones de compra:
    /// - Sin Anuncios ($9.99)
    /// - Premium Completo ($19.99) - Sin anuncios + Crear torneos
    ///
    /// Estilo: Neon theme con colores cyan, yellow/gold y purple
    /// </summary>
    public class PremiumPanelUI : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private GameObject blockerPanel;

        [Header("Title")]
        [SerializeField] private TextMeshProUGUI titleText;

        [Header("No Ads Option")]
        [SerializeField] private GameObject noAdsCard;
        [SerializeField] private TextMeshProUGUI noAdsTitleText;
        [SerializeField] private TextMeshProUGUI noAdsDescriptionText;
        [SerializeField] private TextMeshProUGUI noAdsPriceText;
        [SerializeField] private Button noAdsBuyButton;
        [SerializeField] private TextMeshProUGUI noAdsBuyButtonText;

        [Header("Premium Option")]
        [SerializeField] private GameObject premiumCard;
        [SerializeField] private TextMeshProUGUI premiumTitleText;
        [SerializeField] private TextMeshProUGUI premiumDescriptionText;
        [SerializeField] private TextMeshProUGUI premiumPriceText;
        [SerializeField] private Button premiumBuyButton;
        [SerializeField] private TextMeshProUGUI premiumBuyButtonText;
        [SerializeField] private GameObject recommendedBadge;
        [SerializeField] private TextMeshProUGUI recommendedText;

        [Header("Features List (Premium)")]
        [SerializeField] private TextMeshProUGUI feature1Text;
        [SerializeField] private TextMeshProUGUI feature2Text;
        [SerializeField] private TextMeshProUGUI feature3Text;

        [Header("Footer")]
        [SerializeField] private Button restoreButton;
        [SerializeField] private TextMeshProUGUI restoreButtonText;
        [SerializeField] private Button closeButton;

        [Header("Neon Colors")]
        [SerializeField] private Color neonCyan = new Color(0f, 0.9608f, 1f, 1f);
        [SerializeField] private Color neonGold = new Color(1f, 0.84f, 0f, 1f);
        [SerializeField] private Color neonPurple = new Color(0.6157f, 0.2941f, 1f, 1f);
        [SerializeField] private Color darkBg = new Color(0.02f, 0.05f, 0.1f, 0.98f);
        [SerializeField] private Color cardBg = new Color(0.05f, 0.1f, 0.18f, 0.95f);

        // Callbacks
        private Action onNoAdsPurchase;
        private Action onPremiumPurchase;
        private Action onRestorePurchases;
        private Action onClose;

        private bool listenersConfigured = false;

        private void Awake()
        {
            ConfigureListeners();
            HideVisuals();
        }

        private void OnEnable()
        {
            ConfigureListeners();
            UpdateTexts();
            UpdatePurchaseStates();
        }

        private void ConfigureListeners()
        {
            if (listenersConfigured) return;

            if (noAdsBuyButton != null)
                noAdsBuyButton.onClick.AddListener(OnNoAdsBuyClicked);

            if (premiumBuyButton != null)
                premiumBuyButton.onClick.AddListener(OnPremiumBuyClicked);

            if (restoreButton != null)
                restoreButton.onClick.AddListener(OnRestoreClicked);

            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseClicked);

            listenersConfigured = true;
        }

        private void HideVisuals()
        {
            if (panel != null)
                panel.SetActive(false);

            if (blockerPanel != null && blockerPanel != gameObject)
                blockerPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (noAdsBuyButton != null)
                noAdsBuyButton.onClick.RemoveListener(OnNoAdsBuyClicked);

            if (premiumBuyButton != null)
                premiumBuyButton.onClick.RemoveListener(OnPremiumBuyClicked);

            if (restoreButton != null)
                restoreButton.onClick.RemoveListener(OnRestoreClicked);

            if (closeButton != null)
                closeButton.onClick.RemoveListener(OnCloseClicked);
        }

        /// <summary>
        /// Muestra el panel de premium con callbacks personalizados
        /// </summary>
        public void Show(Action onNoAdsCallback = null, Action onPremiumCallback = null,
                        Action onRestoreCallback = null, Action onCloseCallback = null)
        {
            ConfigureListeners();

            onNoAdsPurchase = onNoAdsCallback;
            onPremiumPurchase = onPremiumCallback;
            onRestorePurchases = onRestoreCallback;
            onClose = onCloseCallback;

            UpdateTexts();
            UpdatePurchaseStates();

            if (!gameObject.activeInHierarchy)
                gameObject.SetActive(true);

            if (blockerPanel != null)
                blockerPanel.SetActive(true);

            if (panel != null)
            {
                panel.SetActive(true);
                panel.transform.SetAsLastSibling();
            }

            Debug.Log("[PremiumPanelUI] Panel mostrado");
        }

        /// <summary>
        /// Muestra el panel usando PremiumManager para las compras
        /// </summary>
        public void ShowWithDefaultHandlers()
        {
            Show(
                onNoAdsCallback: () => PremiumManager.Instance?.PurchaseRemoveAds(),
                onPremiumCallback: () => PremiumManager.Instance?.PurchasePremiumFull(),
                onRestoreCallback: () => PremiumManager.Instance?.RestorePurchases(),
                onCloseCallback: Hide
            );
        }

        /// <summary>
        /// Actualiza los textos con las traducciones
        /// </summary>
        private void UpdateTexts()
        {
            // Titulo
            if (titleText != null)
                titleText.text = AutoLocalizer.Get("premium_title");

            // Sin Anuncios
            if (noAdsTitleText != null)
                noAdsTitleText.text = AutoLocalizer.Get("no_ads_title");
            if (noAdsDescriptionText != null)
                noAdsDescriptionText.text = AutoLocalizer.Get("no_ads_description");
            if (noAdsPriceText != null)
                noAdsPriceText.text = AutoLocalizer.Get("no_ads_price");
            if (noAdsBuyButtonText != null)
                noAdsBuyButtonText.text = AutoLocalizer.Get("buy_button");

            // Premium
            if (premiumTitleText != null)
                premiumTitleText.text = AutoLocalizer.Get("premium_full_title");
            if (premiumDescriptionText != null)
                premiumDescriptionText.text = AutoLocalizer.Get("premium_full_description");
            if (premiumPriceText != null)
                premiumPriceText.text = AutoLocalizer.Get("premium_full_price");
            if (premiumBuyButtonText != null)
                premiumBuyButtonText.text = AutoLocalizer.Get("buy_button");
            if (recommendedText != null)
                recommendedText.text = AutoLocalizer.Get("premium_recommended");

            // Features
            if (feature1Text != null)
                feature1Text.text = "✓ " + AutoLocalizer.Get("premium_feature_no_ads");
            if (feature2Text != null)
                feature2Text.text = "✓ " + AutoLocalizer.Get("premium_feature_tournaments");
            if (feature3Text != null)
                feature3Text.text = "✓ " + AutoLocalizer.Get("premium_feature_badge");

            // Restaurar
            if (restoreButtonText != null)
                restoreButtonText.text = AutoLocalizer.Get("restore_purchases");
        }

        /// <summary>
        /// Actualiza el estado visual según las compras activas
        /// </summary>
        private void UpdatePurchaseStates()
        {
            if (PremiumManager.Instance == null) return;

            bool hasNoAds = PremiumManager.Instance.HasNoAds;
            bool hasPremium = PremiumManager.Instance.CanCreateTournaments;

            // Si ya tiene Premium, deshabilitar ambos botones
            if (hasPremium)
            {
                SetNoAdsCardState(false, AutoLocalizer.Get("premium_active"));
                SetPremiumCardState(false, AutoLocalizer.Get("you_are_premium"));
            }
            // Si solo tiene No Ads
            else if (hasNoAds)
            {
                SetNoAdsCardState(false, AutoLocalizer.Get("no_ads_active"));
                SetPremiumCardState(true, null);
            }
            // No tiene ninguno
            else
            {
                SetNoAdsCardState(true, null);
                SetPremiumCardState(true, null);
            }
        }

        private void SetNoAdsCardState(bool canBuy, string statusText)
        {
            if (noAdsBuyButton != null)
                noAdsBuyButton.interactable = canBuy;

            if (!canBuy && noAdsBuyButtonText != null && !string.IsNullOrEmpty(statusText))
                noAdsBuyButtonText.text = statusText;
        }

        private void SetPremiumCardState(bool canBuy, string statusText)
        {
            if (premiumBuyButton != null)
                premiumBuyButton.interactable = canBuy;

            if (!canBuy && premiumBuyButtonText != null && !string.IsNullOrEmpty(statusText))
                premiumBuyButtonText.text = statusText;
        }

        /// <summary>
        /// Oculta el panel
        /// </summary>
        public void Hide()
        {
            if (panel != null)
                panel.SetActive(false);

            if (blockerPanel != null)
                blockerPanel.SetActive(false);

            onNoAdsPurchase = null;
            onPremiumPurchase = null;
            onRestorePurchases = null;
            onClose = null;

            Debug.Log("[PremiumPanelUI] Panel ocultado");
        }

        /// <summary>
        /// Indica si el panel está visible
        /// </summary>
        public bool IsVisible => panel != null && panel.activeSelf;

        #region Button Handlers

        private void OnNoAdsBuyClicked()
        {
            Debug.Log("[PremiumPanelUI] Comprar Sin Anuncios clickeado");
            onNoAdsPurchase?.Invoke();
        }

        private void OnPremiumBuyClicked()
        {
            Debug.Log("[PremiumPanelUI] Comprar Premium clickeado");
            onPremiumPurchase?.Invoke();
        }

        private void OnRestoreClicked()
        {
            Debug.Log("[PremiumPanelUI] Restaurar compras clickeado");
            onRestorePurchases?.Invoke();
        }

        private void OnCloseClicked()
        {
            Debug.Log("[PremiumPanelUI] Cerrar clickeado");
            onClose?.Invoke();
            Hide();
        }

        #endregion

        #region Runtime Panel Creation (Por código)

        /// <summary>
        /// Crea un panel premium por código con estilo neon
        /// Llamar desde cualquier manager para mostrar el panel
        /// </summary>
        public static PremiumPanelUI CreateAndShow(Transform parent)
        {
            // Crear contenedor principal
            GameObject panelObj = new GameObject("PremiumPanel");
            panelObj.transform.SetParent(parent, false);

            // Agregar componente
            PremiumPanelUI premiumPanel = panelObj.AddComponent<PremiumPanelUI>();

            // Crear estructura UI
            premiumPanel.CreateUIStructure();

            // Mostrar
            premiumPanel.ShowWithDefaultHandlers();

            return premiumPanel;
        }

        /// <summary>
        /// Crea toda la estructura UI por código
        /// </summary>
        private void CreateUIStructure()
        {
            // Canvas para asegurar rendering
            RectTransform rt = gameObject.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // === BLOCKER ===
            blockerPanel = CreateBlocker();

            // === PANEL PRINCIPAL ===
            panel = CreateMainPanel();

            // === TITULO ===
            titleText = CreateTitle(panel.transform);

            // === CARD SIN ANUNCIOS ===
            CreateNoAdsCard(panel.transform);

            // === CARD PREMIUM ===
            CreatePremiumCard(panel.transform);

            // === RESTAURAR COMPRAS ===
            CreateRestoreButton(panel.transform);

            // === BOTON CERRAR ===
            CreateCloseButton(panel.transform);
        }

        private GameObject CreateBlocker()
        {
            GameObject blocker = new GameObject("Blocker");
            blocker.transform.SetParent(transform, false);

            RectTransform rt = blocker.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image img = blocker.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.85f);

            // Hacer clickeable para bloquear
            Button btn = blocker.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;

            return blocker;
        }

        private GameObject CreateMainPanel()
        {
            GameObject mainPanel = new GameObject("MainPanel");
            mainPanel.transform.SetParent(transform, false);

            RectTransform rt = mainPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(550, 750);

            Image bg = mainPanel.AddComponent<Image>();
            bg.color = darkBg;

            // Borde neon
            Outline outline = mainPanel.AddComponent<Outline>();
            outline.effectColor = neonCyan;
            outline.effectDistance = new Vector2(3, 3);

            // Layout
            VerticalLayoutGroup vlg = mainPanel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 20, 20);
            vlg.spacing = 15;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            return mainPanel;
        }

        private TextMeshProUGUI CreateTitle(Transform parent)
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(parent, false);

            RectTransform rt = titleObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 50);

            LayoutElement le = titleObj.AddComponent<LayoutElement>();
            le.preferredHeight = 50;

            TextMeshProUGUI tmp = titleObj.AddComponent<TextMeshProUGUI>();
            tmp.text = AutoLocalizer.Get("premium_title");
            tmp.fontSize = 38;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = neonGold;
            tmp.alignment = TextAlignmentOptions.Center;

            return tmp;
        }

        private void CreateNoAdsCard(Transform parent)
        {
            noAdsCard = CreateCard(parent, "NoAdsCard", neonCyan);

            // Titulo
            noAdsTitleText = CreateCardTitle(noAdsCard.transform, AutoLocalizer.Get("no_ads_title"), neonCyan);

            // Descripcion
            noAdsDescriptionText = CreateCardDescription(noAdsCard.transform, AutoLocalizer.Get("no_ads_description"));

            // Precio
            noAdsPriceText = CreateCardPrice(noAdsCard.transform, AutoLocalizer.Get("no_ads_price"), neonCyan);

            // Boton comprar
            (noAdsBuyButton, noAdsBuyButtonText) = CreateBuyButton(noAdsCard.transform, neonCyan);
        }

        private void CreatePremiumCard(Transform parent)
        {
            premiumCard = CreateCard(parent, "PremiumCard", neonGold);

            // Badge recomendado
            (recommendedBadge, recommendedText) = CreateRecommendedBadge(premiumCard.transform);

            // Titulo
            premiumTitleText = CreateCardTitle(premiumCard.transform, AutoLocalizer.Get("premium_full_title"), neonGold);

            // Descripcion
            premiumDescriptionText = CreateCardDescription(premiumCard.transform, AutoLocalizer.Get("premium_full_description"));

            // Features
            feature1Text = CreateFeatureText(premiumCard.transform, "✓ " + AutoLocalizer.Get("premium_feature_no_ads"));
            feature2Text = CreateFeatureText(premiumCard.transform, "✓ " + AutoLocalizer.Get("premium_feature_tournaments"));
            feature3Text = CreateFeatureText(premiumCard.transform, "✓ " + AutoLocalizer.Get("premium_feature_badge"));

            // Precio
            premiumPriceText = CreateCardPrice(premiumCard.transform, AutoLocalizer.Get("premium_full_price"), neonGold);

            // Boton comprar
            (premiumBuyButton, premiumBuyButtonText) = CreateBuyButton(premiumCard.transform, neonGold);
        }

        private GameObject CreateCard(Transform parent, string name, Color accentColor)
        {
            GameObject card = new GameObject(name);
            card.transform.SetParent(parent, false);

            RectTransform rt = card.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 280);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredHeight = 280;

            Image bg = card.AddComponent<Image>();
            bg.color = cardBg;

            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = new Color(accentColor.r, accentColor.g, accentColor.b, 0.5f);
            outline.effectDistance = new Vector2(2, 2);

            VerticalLayoutGroup vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(25, 25, 20, 20);
            vlg.spacing = 10;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            return card;
        }

        private TextMeshProUGUI CreateCardTitle(Transform parent, string text, Color color)
        {
            GameObject obj = new GameObject("CardTitle");
            obj.transform.SetParent(parent, false);

            LayoutElement le = obj.AddComponent<LayoutElement>();
            le.preferredHeight = 35;

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 26;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;

            return tmp;
        }

        private TextMeshProUGUI CreateCardDescription(Transform parent, string text)
        {
            GameObject obj = new GameObject("CardDescription");
            obj.transform.SetParent(parent, false);

            LayoutElement le = obj.AddComponent<LayoutElement>();
            le.preferredHeight = 25;

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 16;
            tmp.color = new Color(0.8f, 0.8f, 0.85f, 1f);
            tmp.alignment = TextAlignmentOptions.Center;

            return tmp;
        }

        private TextMeshProUGUI CreateCardPrice(Transform parent, string text, Color color)
        {
            GameObject obj = new GameObject("CardPrice");
            obj.transform.SetParent(parent, false);

            LayoutElement le = obj.AddComponent<LayoutElement>();
            le.preferredHeight = 40;

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 32;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;

            return tmp;
        }

        private TextMeshProUGUI CreateFeatureText(Transform parent, string text)
        {
            GameObject obj = new GameObject("Feature");
            obj.transform.SetParent(parent, false);

            LayoutElement le = obj.AddComponent<LayoutElement>();
            le.preferredHeight = 22;

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 14;
            tmp.color = new Color(0.7f, 1f, 0.7f, 1f);
            tmp.alignment = TextAlignmentOptions.Left;

            return tmp;
        }

        private (Button, TextMeshProUGUI) CreateBuyButton(Transform parent, Color color)
        {
            GameObject btnObj = new GameObject("BuyButton");
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(180, 45);

            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.preferredHeight = 45;
            le.preferredWidth = 180;

            Image bg = btnObj.AddComponent<Image>();
            bg.color = color;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;

            // Texto del boton
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = AutoLocalizer.Get("buy_button");
            tmp.fontSize = 20;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            return (btn, tmp);
        }

        private (GameObject, TextMeshProUGUI) CreateRecommendedBadge(Transform parent)
        {
            GameObject badge = new GameObject("RecommendedBadge");
            badge.transform.SetParent(parent, false);

            RectTransform rt = badge.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(-10, -10);
            rt.sizeDelta = new Vector2(120, 30);

            Image bg = badge.AddComponent<Image>();
            bg.color = neonGold;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(badge.transform, false);

            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = AutoLocalizer.Get("premium_recommended");
            tmp.fontSize = 12;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Center;

            return (badge, tmp);
        }

        private void CreateRestoreButton(Transform parent)
        {
            GameObject btnObj = new GameObject("RestoreButton");
            btnObj.transform.SetParent(parent, false);

            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.preferredHeight = 35;

            restoreButton = btnObj.AddComponent<Button>();

            TextMeshProUGUI tmp = btnObj.AddComponent<TextMeshProUGUI>();
            tmp.text = AutoLocalizer.Get("restore_purchases");
            tmp.fontSize = 16;
            tmp.color = neonCyan;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Underline;

            restoreButtonText = tmp;
        }

        private void CreateCloseButton(Transform parent)
        {
            GameObject btnObj = new GameObject("CloseButton");
            // Añadir al panel principal
            btnObj.transform.SetParent(panel.transform, false);

            // Ignorar el VerticalLayoutGroup
            LayoutElement layoutElement = btnObj.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;

            RectTransform rt = btnObj.GetComponent<RectTransform>();
            if (rt == null) rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(-5, -5);
            rt.sizeDelta = new Vector2(45, 45);

            Image bg = btnObj.AddComponent<Image>();
            bg.color = new Color(0.9f, 0.2f, 0.2f, 1f);
            bg.raycastTarget = true;

            closeButton = btnObj.AddComponent<Button>();
            closeButton.targetGraphic = bg;

            // Configurar colores del botón para feedback visual
            ColorBlock colors = closeButton.colors;
            colors.normalColor = new Color(0.9f, 0.2f, 0.2f, 1f);
            colors.highlightedColor = new Color(1f, 0.3f, 0.3f, 1f);
            colors.pressedColor = new Color(0.7f, 0.1f, 0.1f, 1f);
            closeButton.colors = colors;

            // Añadir listener directamente aquí para asegurar que funcione
            closeButton.onClick.AddListener(() => {
                Debug.Log("[PremiumPanelUI] Botón X clickeado");
                Hide();
            });

            // X texto
            GameObject textObj = new GameObject("X");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "X";
            tmp.fontSize = 28;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            // Asegurar que el botón esté al frente
            btnObj.transform.SetAsLastSibling();
        }

        #endregion
    }
}
