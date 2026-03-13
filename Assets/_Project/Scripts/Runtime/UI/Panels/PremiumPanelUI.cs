using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using DigitPark.Managers;
using DigitPark.Localization;
using DigitPark.UI;

namespace DigitPark.UI.Panels
{
    /// <summary>
    /// Panel de Premium reutilizable que muestra las opciones de compra:
    /// - Create Tournaments ($3.99 USD) - Crear torneos personalizados
    /// - Tournament Bundle ($8.99 USD) - Crear torneos + Cash Battle
    /// - Estilos PRO - 5 temas visuales exclusivos (legacy)
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

        [Header("Create Tournaments Option")]
        [SerializeField] private GameObject createTournamentsCard;
        [SerializeField] private TextMeshProUGUI createTournamentsTitleText;
        [SerializeField] private TextMeshProUGUI createTournamentsDescriptionText;
        [SerializeField] private TextMeshProUGUI createTournamentsPriceText;
        [SerializeField] private Button createTournamentsBuyButton;
        [SerializeField] private TextMeshProUGUI createTournamentsBuyButtonText;

        [Header("Tournament Bundle Option")]
        [SerializeField] private GameObject bundleCard;
        [SerializeField] private TextMeshProUGUI bundleTitleText;
        [SerializeField] private TextMeshProUGUI bundleDescriptionText;
        [SerializeField] private TextMeshProUGUI bundlePriceText;
        [SerializeField] private Button bundleBuyButton;
        [SerializeField] private TextMeshProUGUI bundleBuyButtonText;
        [SerializeField] private GameObject recommendedBadge;
        [SerializeField] private TextMeshProUGUI recommendedText;

        [Header("Features List (Premium)")]
        [SerializeField] private TextMeshProUGUI feature1Text;
        [SerializeField] private TextMeshProUGUI feature2Text;
        [SerializeField] private TextMeshProUGUI feature3Text;

        [Header("Styles PRO Option")]
        [SerializeField] private GameObject stylesProCard;
        [SerializeField] private TextMeshProUGUI stylesProTitleText;
        [SerializeField] private TextMeshProUGUI stylesProDescriptionText;
        [SerializeField] private TextMeshProUGUI stylesProPriceText;
        [SerializeField] private Button stylesProBuyButton;
        [SerializeField] private TextMeshProUGUI stylesProBuyButtonText;
        [SerializeField] private TextMeshProUGUI stylesProFeature1Text;
        [SerializeField] private TextMeshProUGUI stylesProFeature2Text;

        [Header("Footer")]
        [SerializeField] private Button restoreButton;
        [SerializeField] private TextMeshProUGUI restoreButtonText;
        [SerializeField] private Button closeButton;

        [Header("Acquired Overlays")]
        [SerializeField] private GameObject createTournamentsAcquiredOverlay;
        [SerializeField] private GameObject bundleAcquiredOverlay;
        [SerializeField] private GameObject stylesProAcquiredOverlay;

        [Header("Neon Colors")]
        [SerializeField] private Color neonCyan = new Color(0f, 0.9608f, 1f, 1f);
        [SerializeField] private Color neonGold = new Color(1f, 0.84f, 0f, 1f);
        [SerializeField] private Color neonPurple = new Color(0.6157f, 0.2941f, 1f, 1f);
        [SerializeField] private Color darkBg = new Color(0.02f, 0.05f, 0.1f, 0.98f);
        [SerializeField] private Color cardBg = new Color(0.05f, 0.1f, 0.18f, 0.95f);

        // Callbacks
        private Action onCreateTournamentsPurchase;
        private Action onBundlePurchase;
        private Action onStylesProPurchase;
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

            if (createTournamentsBuyButton != null)
                createTournamentsBuyButton.onClick.AddListener(OnCreateTournamentsBuyClicked);

            if (bundleBuyButton != null)
                bundleBuyButton.onClick.AddListener(OnBundleBuyClicked);

            if (stylesProBuyButton != null)
                stylesProBuyButton.onClick.AddListener(OnStylesProBuyClicked);

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
            if (createTournamentsBuyButton != null)
                createTournamentsBuyButton.onClick.RemoveListener(OnCreateTournamentsBuyClicked);

            if (bundleBuyButton != null)
                bundleBuyButton.onClick.RemoveListener(OnBundleBuyClicked);

            if (stylesProBuyButton != null)
                stylesProBuyButton.onClick.RemoveListener(OnStylesProBuyClicked);

            if (restoreButton != null)
                restoreButton.onClick.RemoveListener(OnRestoreClicked);

            if (closeButton != null)
                closeButton.onClick.RemoveListener(OnCloseClicked);
        }

        /// <summary>
        /// Muestra el panel de premium con callbacks personalizados
        /// </summary>
        public void Show(Action onCreateTournamentsCallback = null, Action onBundleCallback = null,
                        Action onStylesProCallback = null, Action onRestoreCallback = null,
                        Action onCloseCallback = null)
        {
            ConfigureListeners();

            onCreateTournamentsPurchase = onCreateTournamentsCallback;
            onBundlePurchase = onBundleCallback;
            onStylesProPurchase = onStylesProCallback;
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
                AnimateIn(panel.transform);
            }

            if (blockerPanel != null)
            {
                var blockerCG = blockerPanel.GetComponent<CanvasGroup>();
                if (blockerCG == null) blockerCG = blockerPanel.AddComponent<CanvasGroup>();
                blockerCG.alpha = 0f;
                blockerCG.DOFade(1f, 0.2f).SetUpdate(true);
            }

            Debug.Log("[PremiumPanelUI] Panel mostrado");
        }

        /// <summary>
        /// Muestra el panel usando PremiumManager para las compras
        /// </summary>
        public void ShowWithDefaultHandlers()
        {
            Show(
                onCreateTournamentsCallback: () => PremiumManager.Instance?.PurchaseCreateTournaments(),
                onBundleCallback: () => PremiumManager.Instance?.PurchaseTournamentBundle(),
                onStylesProCallback: () => { /* StylesPro is legacy, no purchase method */ },
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

            // Create Tournaments
            if (createTournamentsTitleText != null)
                createTournamentsTitleText.text = AutoLocalizer.Get("create_tournaments_title");
            if (createTournamentsDescriptionText != null)
                createTournamentsDescriptionText.text = AutoLocalizer.Get("create_tournaments_description");
            if (createTournamentsPriceText != null)
                createTournamentsPriceText.text = PremiumManager.PRICE_CREATE_TOURNAMENTS;
            if (createTournamentsBuyButtonText != null)
                createTournamentsBuyButtonText.text = AutoLocalizer.Get("buy_button");

            // Tournament Bundle
            if (bundleTitleText != null)
                bundleTitleText.text = AutoLocalizer.Get("tournament_bundle_title");
            if (bundleDescriptionText != null)
                bundleDescriptionText.text = AutoLocalizer.Get("tournament_bundle_description");
            if (bundlePriceText != null)
                bundlePriceText.text = PremiumManager.PRICE_TOURNAMENT_BUNDLE;
            if (bundleBuyButtonText != null)
                bundleBuyButtonText.text = AutoLocalizer.Get("buy_button");
            if (recommendedText != null)
                recommendedText.text = AutoLocalizer.Get("premium_recommended");

            // Features
            if (feature1Text != null)
                feature1Text.text = "- " + AutoLocalizer.Get("premium_feature_no_ads");
            if (feature2Text != null)
                feature2Text.text = "- " + AutoLocalizer.Get("premium_feature_tournaments");
            if (feature3Text != null)
                feature3Text.text = "- " + AutoLocalizer.Get("premium_feature_badge");

            // Estilos PRO
            if (stylesProTitleText != null)
                stylesProTitleText.text = AutoLocalizer.Get("styles_pro_title");
            if (stylesProDescriptionText != null)
                stylesProDescriptionText.text = AutoLocalizer.Get("styles_pro_description");
            if (stylesProPriceText != null)
                stylesProPriceText.text = AutoLocalizer.Get("styles_pro_price");
            if (stylesProBuyButtonText != null)
                stylesProBuyButtonText.text = AutoLocalizer.Get("buy_button");
            if (stylesProFeature1Text != null)
                stylesProFeature1Text.text = "- " + AutoLocalizer.Get("styles_pro_feature_themes");
            if (stylesProFeature2Text != null)
                stylesProFeature2Text.text = "- " + AutoLocalizer.Get("styles_pro_feature_exclusive");

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

            bool canCreateTournaments = PremiumManager.Instance.CanCreateTournaments;
            bool canCreateCashBattle = PremiumManager.Instance.CanCreateCashBattle;
            bool hasBundle = canCreateTournaments && canCreateCashBattle;
            bool hasStylesPro = PremiumManager.Instance.HasStylesPro;

            // Si ya tiene Tournament Bundle, deshabilitar ambos botones
            if (hasBundle)
            {
                SetCreateTournamentsCardState(false, AutoLocalizer.Get("premium_active"));
                SetBundleCardState(false, AutoLocalizer.Get("premium_active"));
            }
            // Si solo tiene Create Tournaments
            else if (canCreateTournaments)
            {
                SetCreateTournamentsCardState(false, AutoLocalizer.Get("already_purchased"));
                SetBundleCardState(true, null);
            }
            // No tiene ninguno
            else
            {
                SetCreateTournamentsCardState(true, null);
                SetBundleCardState(true, null);
            }

            // Estilos PRO es independiente de los otros productos
            if (hasStylesPro)
            {
                SetStylesProCardState(false, AutoLocalizer.Get("styles_pro_active"));
            }
            else
            {
                SetStylesProCardState(true, null);
            }

            // Ocultar boton de Restore Purchases si tiene TODOS los productos
            bool hasAllProducts = hasBundle && hasStylesPro;
            if (restoreButton != null)
            {
                restoreButton.gameObject.SetActive(!hasAllProducts);
            }
        }

        private void SetCreateTournamentsCardState(bool canBuy, string statusText)
        {
            if (createTournamentsBuyButton != null)
                createTournamentsBuyButton.interactable = canBuy;

            if (!canBuy && createTournamentsBuyButtonText != null && !string.IsNullOrEmpty(statusText))
                createTournamentsBuyButtonText.text = statusText;

            // Mostrar/ocultar overlay de "Adquirido"
            if (createTournamentsAcquiredOverlay != null)
                createTournamentsAcquiredOverlay.SetActive(!canBuy);
        }

        private void SetBundleCardState(bool canBuy, string statusText)
        {
            if (bundleBuyButton != null)
                bundleBuyButton.interactable = canBuy;

            if (!canBuy && bundleBuyButtonText != null && !string.IsNullOrEmpty(statusText))
                bundleBuyButtonText.text = statusText;

            // Mostrar/ocultar overlay de "Adquirido"
            if (bundleAcquiredOverlay != null)
                bundleAcquiredOverlay.SetActive(!canBuy);
        }

        private void SetStylesProCardState(bool canBuy, string statusText)
        {
            if (stylesProBuyButton != null)
                stylesProBuyButton.interactable = canBuy;

            if (!canBuy && stylesProBuyButtonText != null && !string.IsNullOrEmpty(statusText))
                stylesProBuyButtonText.text = statusText;

            // Mostrar/ocultar overlay de "Adquirido"
            if (stylesProAcquiredOverlay != null)
                stylesProAcquiredOverlay.SetActive(!canBuy);

            // Ocultar precio cuando ya tiene StylesPro
            if (stylesProPriceText != null)
                stylesProPriceText.gameObject.SetActive(canBuy);
        }

        /// <summary>
        /// Oculta el panel y destruye el canvas si fue creado dinámicamente
        /// </summary>
        public void Hide()
        {
            Debug.Log("[PremiumPanelUI] Hide() llamado");

            if (panel != null)
                AnimateOut(panel.transform, () => panel.SetActive(false));

            if (blockerPanel != null)
            {
                var blockerCG = blockerPanel.GetComponent<CanvasGroup>();
                if (blockerCG != null)
                    blockerCG.DOFade(0f, 0.2f).OnComplete(() => blockerPanel.SetActive(false)).SetUpdate(true);
                else
                    blockerPanel.SetActive(false);
            }

            onCreateTournamentsPurchase = null;
            onBundlePurchase = null;
            onStylesProPurchase = null;
            onRestorePurchases = null;
            onClose = null;
            listenersConfigured = false;

            // Si el padre es un canvas creado dinámicamente (PremiumPanelCanvas), destruirlo
            if (transform.parent != null && transform.parent.name == "PremiumPanelCanvas")
            {
                Debug.Log("[PremiumPanelUI] Destruyendo canvas dinámico...");
                Destroy(transform.parent.gameObject);
            }
            else
            {
                Debug.Log("[PremiumPanelUI] Panel ocultado (sin destruir canvas)");
            }
        }

        /// <summary>
        /// Indica si el panel está visible
        /// </summary>
        public bool IsVisible => panel != null && panel.activeSelf;

        private void AnimateIn(Transform t)
        {
            t.localScale = Vector3.one * 0.85f;
            var cg = t.GetComponent<CanvasGroup>();
            if (cg == null) cg = t.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            DOTween.Sequence()
                .Join(t.DOScale(1f, 0.35f).SetEase(Ease.OutBack))
                .Join(cg.DOFade(1f, 0.3f))
                .SetLink(t.gameObject)
                .SetUpdate(true);
        }

        private void AnimateOut(Transform t, Action onComplete)
        {
            var cg = t.GetComponent<CanvasGroup>();
            if (cg == null) { t.gameObject.SetActive(false); onComplete?.Invoke(); return; }
            DOTween.Sequence()
                .Join(t.DOScale(0.9f, 0.2f).SetEase(Ease.InQuad))
                .Join(cg.DOFade(0f, 0.2f))
                .OnComplete(() => { if (t != null) t.localScale = Vector3.one; if (cg != null) cg.alpha = 1f; onComplete?.Invoke(); })
                .SetUpdate(true)
                .SetLink(t.gameObject);
        }

        #region Button Handlers

        private void OnCreateTournamentsBuyClicked()
        {
            Debug.Log("[PremiumPanelUI] Comprar Create Tournaments clickeado");
            onCreateTournamentsPurchase?.Invoke();
        }

        private void OnBundleBuyClicked()
        {
            Debug.Log("[PremiumPanelUI] Comprar Tournament Bundle clickeado");
            onBundlePurchase?.Invoke();
        }

        private void OnStylesProBuyClicked()
        {
            Debug.Log("[PremiumPanelUI] Comprar Estilos PRO clickeado");
            onStylesProPurchase?.Invoke();
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

            // === CARD CREATE TOURNAMENTS ===
            CreateCreateTournamentsCard(panel.transform);

            // === CARD TOURNAMENT BUNDLE ===
            CreateBundleCard(panel.transform);

            // === CARD ESTILOS PRO ===
            CreateStylesProCard(panel.transform);

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
            img.raycastTarget = true; // Bloquear clicks que pasen al fondo

            // NO agregamos Button - solo queremos que el Image bloquee raycast
            // Esto evita que clicks en el blocker cierren el panel accidentalmente

            return blocker;
        }

        private GameObject CreateMainPanel()
        {
            GameObject mainPanel = new GameObject("MainPanel");
            mainPanel.transform.SetParent(transform, false);

            RectTransform rt = mainPanel.AddComponent<RectTransform>();
            // Nearly fullscreen for 1080x1920 reference
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(980, 1500);

            Image bg = mainPanel.AddComponent<Image>();
            bg.color = darkBg;

            // Borde neon
            Outline outline = mainPanel.AddComponent<Outline>();
            outline.effectColor = neonCyan;
            outline.effectDistance = new Vector2(3, 3);

            // Layout
            VerticalLayoutGroup vlg = mainPanel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(30, 30, 60, 30);
            vlg.spacing = 20;
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
            rt.sizeDelta = new Vector2(0, 70);

            LayoutElement le = titleObj.AddComponent<LayoutElement>();
            le.preferredHeight = 70;

            TextMeshProUGUI tmp = titleObj.AddComponent<TextMeshProUGUI>();
            tmp.text = AutoLocalizer.Get("premium_title");
            tmp.fontSize = FontSizes.H1;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinTitle;
            tmp.fontSizeMax = FontSizes.H1;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = neonGold;
            tmp.alignment = TextAlignmentOptions.Center;

            return tmp;
        }

        private void CreateCreateTournamentsCard(Transform parent)
        {
            createTournamentsCard = CreateCard(parent, "CreateTournamentsCard", neonCyan);

            // Titulo
            createTournamentsTitleText = CreateCardTitle(createTournamentsCard.transform, AutoLocalizer.Get("create_tournaments_title"), neonCyan);

            // Descripcion
            createTournamentsDescriptionText = CreateCardDescription(createTournamentsCard.transform, AutoLocalizer.Get("create_tournaments_description"));

            // Precio
            createTournamentsPriceText = CreateCardPrice(createTournamentsCard.transform, PremiumManager.PRICE_CREATE_TOURNAMENTS, neonCyan);

            // Boton comprar
            (createTournamentsBuyButton, createTournamentsBuyButtonText) = CreateBuyButton(createTournamentsCard.transform, neonCyan);

            // Overlay de "Adquirido"
            createTournamentsAcquiredOverlay = CreateAcquiredOverlay(createTournamentsCard.transform, neonCyan);
        }

        private void CreateBundleCard(Transform parent)
        {
            bundleCard = CreateCard(parent, "BundleCard", neonGold, true);

            // Titulo con estrellas a los lados
            GameObject titleRow = new GameObject("BundleTitleRow");
            titleRow.transform.SetParent(bundleCard.transform, false);
            LayoutElement titleRowLe = titleRow.AddComponent<LayoutElement>();
            titleRowLe.preferredHeight = 60;
            HorizontalLayoutGroup titleHlg = titleRow.AddComponent<HorizontalLayoutGroup>();
            titleHlg.spacing = 12;
            titleHlg.childAlignment = TextAnchor.MiddleCenter;
            titleHlg.childControlWidth = false;
            titleHlg.childControlHeight = false;
            titleHlg.childForceExpandWidth = false;
            titleHlg.childForceExpandHeight = false;

            Sprite starSprite = Resources.Load<Sprite>("Icons/UI/StarRecommended");
            if (starSprite == null)
                Debug.LogWarning("[PremiumPanelUI] Resource not found: Icons/UI/StarRecommended");

            // Estrella izquierda
            GameObject starL = new GameObject("StarLeft");
            starL.transform.SetParent(titleRow.transform, false);
            RectTransform starLrt = starL.AddComponent<RectTransform>();
            starLrt.sizeDelta = new Vector2(50, 50);
            Image starLimg = starL.AddComponent<Image>();
            starLimg.sprite = starSprite;
            starLimg.preserveAspect = true;
            starLimg.raycastTarget = false;

            // Texto titulo
            GameObject titleObj = new GameObject("CardTitle");
            titleObj.transform.SetParent(titleRow.transform, false);
            RectTransform titleRt = titleObj.AddComponent<RectTransform>();
            titleRt.sizeDelta = new Vector2(400, 60);
            bundleTitleText = titleObj.AddComponent<TextMeshProUGUI>();
            bundleTitleText.text = AutoLocalizer.Get("tournament_bundle_title");
            bundleTitleText.fontSize = FontSizes.H3;
            bundleTitleText.enableAutoSizing = true;
            bundleTitleText.fontSizeMin = FontSizes.Body;
            bundleTitleText.fontSizeMax = FontSizes.H3;
            bundleTitleText.fontStyle = FontStyles.Bold;
            bundleTitleText.color = neonGold;
            bundleTitleText.alignment = TextAlignmentOptions.Center;

            // Estrella derecha
            GameObject starR = new GameObject("StarRight");
            starR.transform.SetParent(titleRow.transform, false);
            RectTransform starRrt = starR.AddComponent<RectTransform>();
            starRrt.sizeDelta = new Vector2(50, 50);
            Image starRimg = starR.AddComponent<Image>();
            starRimg.sprite = starSprite;
            starRimg.preserveAspect = true;
            starRimg.raycastTarget = false;

            // Descripcion (incluye los beneficios en una linea)
            bundleDescriptionText = CreateCardDescription(bundleCard.transform, AutoLocalizer.Get("tournament_bundle_description"));

            // Precio
            bundlePriceText = CreateCardPrice(bundleCard.transform, PremiumManager.PRICE_TOURNAMENT_BUNDLE, neonGold);

            // Boton comprar
            (bundleBuyButton, bundleBuyButtonText) = CreateBuyButton(bundleCard.transform, neonGold);

            // Overlay de "Adquirido"
            bundleAcquiredOverlay = CreateAcquiredOverlay(bundleCard.transform, neonGold);
        }

        private void CreateStylesProCard(Transform parent)
        {
            stylesProCard = CreateCard(parent, "StylesProCard", neonPurple);

            // Titulo
            stylesProTitleText = CreateCardTitle(stylesProCard.transform, AutoLocalizer.Get("styles_pro_title"), neonPurple);

            // Descripcion
            stylesProDescriptionText = CreateCardDescription(stylesProCard.transform, AutoLocalizer.Get("styles_pro_description"));

            // Precio
            stylesProPriceText = CreateCardPrice(stylesProCard.transform, AutoLocalizer.Get("styles_pro_price"), neonPurple);

            // Boton comprar
            (stylesProBuyButton, stylesProBuyButtonText) = CreateBuyButton(stylesProCard.transform, neonPurple);

            // Overlay de "Adquirido"
            stylesProAcquiredOverlay = CreateAcquiredOverlay(stylesProCard.transform, neonPurple);
        }

        /// <summary>
        /// Crea un overlay semi-transparente con texto "Adquirido" sobre una tarjeta
        /// </summary>
        private GameObject CreateAcquiredOverlay(Transform parent, Color accentColor)
        {
            GameObject overlay = new GameObject("AcquiredOverlay");
            overlay.transform.SetParent(parent, false);

            // Ignorar layout para que cubra toda la tarjeta
            LayoutElement le = overlay.AddComponent<LayoutElement>();
            le.ignoreLayout = true;

            RectTransform rt = overlay.GetComponent<RectTransform>();
            if (rt == null) rt = overlay.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Fondo semi-transparente oscuro
            Image bg = overlay.AddComponent<Image>();
            bg.color = new Color(0.02f, 0.05f, 0.1f, 0.92f);
            bg.raycastTarget = true; // Bloquear clicks

            // Borde con el color de acento
            Outline outline = overlay.AddComponent<Outline>();
            outline.effectColor = new Color(accentColor.r, accentColor.g, accentColor.b, 0.8f);
            outline.effectDistance = new Vector2(3, 3);

            // Texto "Adquirido"
            GameObject textObj = new GameObject("AcquiredText");
            textObj.transform.SetParent(overlay.transform, false);

            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = AutoLocalizer.Get("acquired_text");
            tmp.fontSize = FontSizes.H3;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = accentColor;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            // Inicialmente oculto
            overlay.SetActive(false);

            return overlay;
        }

        private GameObject CreateCard(Transform parent, string name, Color accentColor, bool highlighted = false)
        {
            GameObject card = new GameObject(name);
            card.transform.SetParent(parent, false);

            RectTransform rt = card.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 350);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredHeight = 350;

            Image bg = card.AddComponent<Image>();
            bg.color = highlighted ? new Color(0.08f, 0.12f, 0.22f, 0.98f) : cardBg;

            Outline outline = card.AddComponent<Outline>();
            float alpha = highlighted ? 1f : 0.6f;
            outline.effectColor = new Color(accentColor.r, accentColor.g, accentColor.b, alpha);
            outline.effectDistance = highlighted ? new Vector2(5, 5) : new Vector2(3, 3);

            VerticalLayoutGroup vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(25, 25, 20, 20);
            vlg.spacing = 12;
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
            le.preferredHeight = 50;

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = FontSizes.H3;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.Body;
            tmp.fontSizeMax = FontSizes.H3;
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
            le.preferredHeight = 55;

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = FontSizes.Subtitle;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinBody;
            tmp.fontSizeMax = FontSizes.Subtitle;
            tmp.color = new Color(0.8f, 0.8f, 0.85f, 1f);
            tmp.alignment = TextAlignmentOptions.Center;

            return tmp;
        }

        private TextMeshProUGUI CreateCardPrice(Transform parent, string text, Color color)
        {
            GameObject obj = new GameObject("CardPrice");
            obj.transform.SetParent(parent, false);

            LayoutElement le = obj.AddComponent<LayoutElement>();
            le.preferredHeight = 50;

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = FontSizes.H3;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinTitle;
            tmp.fontSizeMax = FontSizes.H3;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;

            return tmp;
        }

        private (Button, TextMeshProUGUI) CreateBuyButton(Transform parent, Color color)
        {
            GameObject btnObj = new GameObject("BuyButton");
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(350, 70);

            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.preferredHeight = 70;
            le.preferredWidth = 350;

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
            tmp.fontSize = FontSizes.H4;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinTitle;
            tmp.fontSizeMax = FontSizes.H4;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            return (btn, tmp);
        }

        private void CreateRestoreButton(Transform parent)
        {
            GameObject btnObj = new GameObject("RestoreButton");
            btnObj.transform.SetParent(parent, false);

            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.preferredHeight = 45;

            restoreButton = btnObj.AddComponent<Button>();

            TextMeshProUGUI tmp = btnObj.AddComponent<TextMeshProUGUI>();
            tmp.text = AutoLocalizer.Get("restore_purchases");
            tmp.fontSize = FontSizes.Subtitle;
            tmp.color = neonCyan;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;

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
            rt.sizeDelta = new Vector2(65, 65);

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
            tmp.fontSize = FontSizes.H3;
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
