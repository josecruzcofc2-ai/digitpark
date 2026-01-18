using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Helper para agregar entry points a Shop en las escenas existentes.
    ///
    /// ESCENAS PRINCIPALES (MainMenu, Profile):
    /// - BottomNavBar completo
    /// - Currency Display en header
    /// - PremiumCard opcional
    ///
    /// ESCENAS SECUNDARIAS (CashBattleHub, TournamentsBrowser, etc):
    /// - Solo boton de monedas compacto (no solapa UI existente)
    /// </summary>
    public class ShopEntryPointsHelper : EditorWindow
    {
        // ==================== TAMAÑOS CONFIGURABLES ====================
        // Ajusta estos valores para cambiar el tamaño de los elementos en todas las escenas

        // --- BOTTOM NAV BAR ---
        private const float NAVBAR_HEIGHT = 90f;              // Altura total de la barra
        private const float NAVBAR_ICON_SIZE = 30f;           // Tamaño de iconos
        private const float NAVBAR_LABEL_FONTSIZE = 11f;      // Tamaño de texto
        private const float NAVBAR_INDICATOR_HEIGHT = 3f;     // Altura del indicador activo
        private const int NAVBAR_PADDING_HORIZONTAL = 10;     // Padding izquierda/derecha
        private const int NAVBAR_PADDING_TOP = 8;             // Padding arriba
        private const int NAVBAR_PADDING_BOTTOM = 20;         // Padding abajo (safe area)

        // --- CURRENCY DISPLAY (Header) ---
        private const float CURRENCY_DISPLAY_WIDTH = 280f;    // Ancho total del contenedor
        private const float CURRENCY_DISPLAY_HEIGHT = 50f;    // Altura del contenedor
        private const float CURRENCY_ITEM_WIDTH = 125f;       // Ancho de cada item (gemas/monedas)
        private const float CURRENCY_ITEM_HEIGHT = 45f;       // Altura de cada item
        private const float CURRENCY_ICON_SIZE = 28f;         // Tamaño de iconos
        private const float CURRENCY_FONTSIZE = 18f;          // Tamaño del texto de cantidad
        private const float CURRENCY_PLUS_SIZE = 22f;         // Tamaño del botón +
        private const float CURRENCY_SPACING = 15f;           // Espacio entre items

        // --- COMPACT CURRENCY BUTTON (Escenas secundarias) ---
        private const float COMPACT_WIDTH = 115f;             // Ancho del botón compacto
        private const float COMPACT_HEIGHT = 38f;             // Altura del botón compacto
        private const float COMPACT_ICON_SIZE = 24f;          // Tamaño del icono
        private const float COMPACT_FONTSIZE = 15f;           // Tamaño del texto
        private const float COMPACT_PLUS_SIZE = 20f;          // Tamaño del botón +
        private const float COMPACT_OFFSET_X = -20f;          // Offset desde la derecha
        private const float COMPACT_OFFSET_Y = -80f;          // Offset desde arriba

        // --- PREMIUM CARD ---
        private const float PREMIUM_CARD_HEIGHT = 100f;       // Altura de la tarjeta premium
        private const float PREMIUM_ICON_SIZE = 50f;          // Tamaño del icono
        private const float PREMIUM_TITLE_FONTSIZE = 18f;     // Tamaño del título
        private const float PREMIUM_SUBTITLE_FONTSIZE = 13f;  // Tamaño del subtítulo
        private const float PREMIUM_PRICE_WIDTH = 80f;        // Ancho del botón de precio
        private const float PREMIUM_PRICE_HEIGHT = 40f;       // Altura del botón de precio

        // ==================== TEMA NEON (Default) ====================
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.4f, 1f);

        // ==================== TEMA CASH (Gold) ====================
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GOLD_DARK = new Color(0.7f, 0.55f, 0f, 1f);

        // ==================== COLORES COMUNES ====================
        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color HEADER_BG = new Color(0.03f, 0.06f, 0.1f, 0.95f);
        private static readonly Color TEXT_PRIMARY = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color TEXT_SECONDARY = new Color(0.6f, 0.7f, 0.75f, 1f);
        private static readonly Color TEXT_DARK = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color GEM_COLOR = new Color(0.4f, 0.8f, 1f, 1f);
        private static readonly Color COIN_COLOR = new Color(1f, 0.85f, 0.3f, 1f);
        private static readonly Color PURPLE_PREMIUM = new Color(0.6f, 0.3f, 0.9f, 1f);

        private static bool _useCashTheme = false;
        private static Color _accentColor => _useCashTheme ? GOLD : CYAN_NEON;

        // ==================== MENU - ESCENAS PRINCIPALES (Neon) ====================

        [MenuItem("DigitPark/Shop/Entry Points/Main Scenes (Neon)/Add BottomNavBar + Currency", false, 100)]
        public static void AddMainEntryPoints_Neon()
        {
            _useCashTheme = false;
            AddMainSceneEntryPoints();
        }

        [MenuItem("DigitPark/Shop/Entry Points/Main Scenes (Neon)/Add Only BottomNavBar", false, 101)]
        public static void AddBottomNavBar_Neon()
        {
            _useCashTheme = false;
            AddBottomNavBar();
        }

        [MenuItem("DigitPark/Shop/Entry Points/Main Scenes (Neon)/Add Only Currency Display", false, 102)]
        public static void AddCurrency_Neon()
        {
            _useCashTheme = false;
            AddHeaderCurrencyDisplay();
        }

        [MenuItem("DigitPark/Shop/Entry Points/Main Scenes (Neon)/Add Premium Card (MainMenu)", false, 103)]
        public static void AddPremiumCard_Neon()
        {
            _useCashTheme = false;
            AddPremiumCard();
        }

        // ==================== MENU - ESCENAS SECUNDARIAS (Neon) ====================

        [MenuItem("DigitPark/Shop/Entry Points/Secondary Scenes (Neon)/Add Compact Currency Button", false, 110)]
        public static void AddSecondaryEntryPoint_Neon()
        {
            _useCashTheme = false;
            AddCompactCurrencyButton();
        }

        // ==================== MENU - ESCENAS PRINCIPALES (Cash/Gold) ====================

        [MenuItem("DigitPark/Shop/Entry Points/Main Scenes (Cash-Gold)/Add BottomNavBar + Currency", false, 120)]
        public static void AddMainEntryPoints_Cash()
        {
            _useCashTheme = true;
            AddMainSceneEntryPoints();
        }

        [MenuItem("DigitPark/Shop/Entry Points/Main Scenes (Cash-Gold)/Add Only BottomNavBar", false, 121)]
        public static void AddBottomNavBar_Cash()
        {
            _useCashTheme = true;
            AddBottomNavBar();
        }

        // ==================== MENU - ESCENAS SECUNDARIAS (Cash/Gold) ====================

        [MenuItem("DigitPark/Shop/Entry Points/Secondary Scenes (Cash-Gold)/Add Compact Currency Button", false, 130)]
        public static void AddSecondaryEntryPoint_Cash()
        {
            _useCashTheme = true;
            AddCompactCurrencyButton();
        }

        // ==================== MENU - REMOVE ====================

        [MenuItem("DigitPark/Shop/Entry Points/Remove/Remove BottomNavBar", false, 200)]
        public static void RemoveBottomNavBar()
        {
            RemoveElement("BottomNavBar");
        }

        [MenuItem("DigitPark/Shop/Entry Points/Remove/Remove Premium Card", false, 201)]
        public static void RemovePremiumCard()
        {
            RemoveElement("PremiumCard");
        }

        [MenuItem("DigitPark/Shop/Entry Points/Remove/Remove Currency Display", false, 202)]
        public static void RemoveCurrencyDisplay()
        {
            RemoveFromHeader("CurrencyDisplay");
        }

        [MenuItem("DigitPark/Shop/Entry Points/Remove/Remove Compact Currency", false, 203)]
        public static void RemoveCompactCurrency()
        {
            RemoveFromHeader("CompactCurrencyButton");
        }

        [MenuItem("DigitPark/Shop/Entry Points/Remove/Remove ALL Entry Points", false, 210)]
        public static void RemoveAllEntryPoints()
        {
            RemoveElement("BottomNavBar");
            RemoveElement("PremiumCard");
            RemoveFromHeader("CurrencyDisplay");
            RemoveFromHeader("CompactCurrencyButton");
            Debug.Log("[ShopEntryPointsHelper] All entry points removed!");
        }

        [MenuItem("DigitPark/Shop/Entry Points/Remove/Remove Old Premium (MainMenu)", false, 220)]
        public static void RemoveOldPremiumMainMenu()
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null) return;

            int removed = 0;
            string[] oldPremiumNames = { "PremiumBanner", "PremiumPanel", "Premium", "PremiumButton" };

            foreach (string name in oldPremiumNames)
            {
                // Search in SafeArea
                Transform safeArea = canvas.transform.Find("SafeArea");
                if (safeArea != null)
                {
                    Transform element = safeArea.Find(name);
                    if (element != null)
                    {
                        Object.DestroyImmediate(element.gameObject);
                        removed++;
                        Debug.Log($"[ShopEntryPointsHelper] Removed: {name}");
                    }
                }

                // Search directly in Canvas
                Transform canvasElement = canvas.transform.Find(name);
                if (canvasElement != null)
                {
                    Object.DestroyImmediate(canvasElement.gameObject);
                    removed++;
                    Debug.Log($"[ShopEntryPointsHelper] Removed: {name}");
                }

                // Deep search
                foreach (Transform child in canvas.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name == name)
                    {
                        Object.DestroyImmediate(child.gameObject);
                        removed++;
                        Debug.Log($"[ShopEntryPointsHelper] Removed (deep): {name}");
                        break;
                    }
                }
            }

            if (removed > 0)
            {
                MarkSceneDirty();
                Debug.Log($"[ShopEntryPointsHelper] Old Premium elements removed: {removed}");
            }
            else
            {
                Debug.Log("[ShopEntryPointsHelper] No old Premium elements found");
            }
        }

        // ==================== CORE - MAIN SCENES ====================

        private static void AddMainSceneEntryPoints()
        {
            string themeName = _useCashTheme ? "Cash/Gold" : "Neon/Cyan";
            if (!EditorUtility.DisplayDialog("Add Main Scene Entry Points",
                $"Tema: {themeName}\n\nEsto agregara a la escena principal:\n" +
                "- Bottom Nav Bar (5 botones)\n" +
                "- Currency Display en Header\n\n" +
                "Usar en: MainMenu, Profile\n\nContinuar?",
                "Si", "No"))
                return;

            Canvas canvas = GetCanvas();
            if (canvas == null) return;

            // Add Currency Display
            Transform header = FindHeader(canvas);
            if (header != null)
            {
                CreateCurrencyDisplay(header.gameObject);
            }
            else
            {
                Debug.LogWarning("[ShopEntryPointsHelper] No Header found - skipping Currency Display");
            }

            // Add Bottom Nav Bar
            GameObject parent = GetOrCreateParent(canvas);
            CreateBottomNavBar(parent);

            MarkSceneDirty();
            Debug.Log($"[ShopEntryPointsHelper] Main scene entry points added! Theme: {themeName}");
        }

        private static void AddHeaderCurrencyDisplay()
        {
            Canvas canvas = GetCanvas();
            if (canvas == null) return;

            Transform header = FindHeader(canvas);
            if (header == null)
            {
                Debug.LogError("[ShopEntryPointsHelper] No Header found");
                return;
            }

            CreateCurrencyDisplay(header.gameObject);
            MarkSceneDirty();
            Debug.Log($"[ShopEntryPointsHelper] Currency Display added! Theme: {(_useCashTheme ? "Cash" : "Neon")}");
        }

        private static void AddBottomNavBar()
        {
            Canvas canvas = GetCanvas();
            if (canvas == null) return;

            GameObject parent = GetOrCreateParent(canvas);
            CreateBottomNavBar(parent);
            MarkSceneDirty();
            Debug.Log($"[ShopEntryPointsHelper] Bottom Nav Bar added! Theme: {(_useCashTheme ? "Cash" : "Neon")}");
        }

        private static void AddPremiumCard()
        {
            Canvas canvas = GetCanvas();
            if (canvas == null) return;

            GameObject parent = GetOrCreateParent(canvas);
            CreatePremiumCard(parent);
            MarkSceneDirty();
            Debug.Log("[ShopEntryPointsHelper] Premium Card added!");
        }

        // ==================== CORE - SECONDARY SCENES ====================

        /// <summary>
        /// Agrega un boton compacto de monedas que no solapa la UI existente.
        /// Ideal para escenas secundarias como CashBattleHub, TournamentsBrowser.
        /// </summary>
        private static void AddCompactCurrencyButton()
        {
            string themeName = _useCashTheme ? "Cash/Gold" : "Neon/Cyan";
            if (!EditorUtility.DisplayDialog("Add Compact Currency Button",
                $"Tema: {themeName}\n\nEsto agregara un boton compacto de gemas\n" +
                "en la esquina superior derecha.\n\n" +
                "NO agrega BottomNavBar (escena secundaria)\n\n" +
                "Usar en: CashBattleHub, TournamentsBrowser, etc.\n\nContinuar?",
                "Si", "No"))
                return;

            Canvas canvas = GetCanvas();
            if (canvas == null) return;

            Transform header = FindHeader(canvas);
            if (header == null)
            {
                // Create in canvas if no header
                header = canvas.transform;
            }

            CreateCompactCurrencyButton(header.gameObject);
            MarkSceneDirty();
            Debug.Log($"[ShopEntryPointsHelper] Compact Currency Button added! Theme: {themeName}");
        }

        // ==================== CREATE - COMPACT CURRENCY BUTTON ====================

        private static void CreateCompactCurrencyButton(GameObject parent)
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null) return;

            // Remove existing from various locations first
            RemoveExistingCompactButton(canvas);

            // For secondary scenes, use SafeArea as parent (more reliable)
            Transform safeArea = canvas.transform.Find("SafeArea");
            Transform targetParent = safeArea != null ? safeArea : canvas.transform;

            GameObject button = new GameObject("CompactCurrencyButton");
            button.transform.SetParent(targetParent, false);

            RectTransform rt = button.AddComponent<RectTransform>();
            // Position in top-right, below header area (fixed position from top-right)
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(COMPACT_OFFSET_X, COMPACT_OFFSET_Y);
            rt.sizeDelta = new Vector2(COMPACT_WIDTH, COMPACT_HEIGHT);

            // Make sure it's on top
            button.transform.SetAsLastSibling();

            // Background
            Image bg = button.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.08f, 0.12f, 0.95f);

            // Outline with theme color
            Outline outline = button.AddComponent<Outline>();
            outline.effectColor = _accentColor;
            outline.effectDistance = new Vector2(1.5f, 1.5f);

            // Button component
            Button btn = button.AddComponent<Button>();
            SetupButtonColors(btn);

            // Add ShopEntryPoint component
            var entryPoint = button.AddComponent<Monetization.ShopEntryPoint>();
            SerializedObject so = new SerializedObject(entryPoint);
            so.FindProperty("_targetTab").enumValueIndex = 0; // Gems
            so.ApplyModifiedProperties();

            // Horizontal layout
            HorizontalLayoutGroup hlg = button.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6;
            hlg.padding = new RectOffset(10, 8, 5, 5);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            // Gem Icon (Image component - assign sprite in inspector)
            GameObject iconObj = new GameObject("GemIcon");
            iconObj.transform.SetParent(button.transform, false);
            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = GEM_COLOR;
            // Placeholder: white square, user will assign gem sprite
            LayoutElement iconLE = iconObj.AddComponent<LayoutElement>();
            iconLE.minWidth = COMPACT_ICON_SIZE;
            iconLE.minHeight = COMPACT_ICON_SIZE;
            iconLE.preferredWidth = COMPACT_ICON_SIZE;
            iconLE.preferredHeight = COMPACT_ICON_SIZE;

            // Amount Text
            GameObject textObj = new GameObject("Amount");
            textObj.transform.SetParent(button.transform, false);
            TextMeshProUGUI amountText = textObj.AddComponent<TextMeshProUGUI>();
            amountText.text = "1.2K";
            amountText.fontSize = COMPACT_FONTSIZE;
            amountText.fontStyle = FontStyles.Bold;
            amountText.color = TEXT_PRIMARY;
            amountText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement textLE = textObj.AddComponent<LayoutElement>();
            textLE.minWidth = 45;
            textLE.preferredWidth = 45;

            // Plus button
            GameObject plusObj = new GameObject("PlusBtn");
            plusObj.transform.SetParent(button.transform, false);
            Image plusBg = plusObj.AddComponent<Image>();
            plusBg.color = GEM_COLOR;
            LayoutElement plusLE = plusObj.AddComponent<LayoutElement>();
            plusLE.minWidth = COMPACT_PLUS_SIZE;
            plusLE.minHeight = COMPACT_PLUS_SIZE;
            plusLE.preferredWidth = COMPACT_PLUS_SIZE;
            plusLE.preferredHeight = COMPACT_PLUS_SIZE;

            // Plus text inside button
            GameObject plusTextObj = new GameObject("Text");
            plusTextObj.transform.SetParent(plusObj.transform, false);
            TextMeshProUGUI plusTmp = plusTextObj.AddComponent<TextMeshProUGUI>();
            plusTmp.text = "+";
            plusTmp.fontSize = 16;
            plusTmp.fontStyle = FontStyles.Bold;
            plusTmp.color = TEXT_DARK;
            plusTmp.alignment = TextAlignmentOptions.Center;
            RectTransform plusTextRT = plusTextObj.GetComponent<RectTransform>();
            plusTextRT.anchorMin = Vector2.zero;
            plusTextRT.anchorMax = Vector2.one;
            plusTextRT.sizeDelta = Vector2.zero;
            plusTextRT.anchoredPosition = Vector2.zero;

            button.transform.SetAsLastSibling();
        }

        // ==================== CREATE - CURRENCY DISPLAY (MAIN SCENES) ====================

        private static void CreateCurrencyDisplay(GameObject header)
        {
            Transform existing = header.transform.Find("CurrencyDisplay");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            // Check if header already has buttons (Settings, Notifications, etc.)
            bool hasExistingButtons = false;
            float rightOffset = -20f;

            string[] existingButtonNames = { "SettingsButton", "NotificationsButton", "UserButton", "ProfileButton" };
            foreach (var btnName in existingButtonNames)
            {
                if (header.transform.Find(btnName) != null)
                {
                    hasExistingButtons = true;
                    break;
                }
            }

            // If there are existing buttons, position currency display more to the left
            if (hasExistingButtons)
            {
                rightOffset = -140f; // Leave space for existing buttons
                Debug.Log("[ShopEntryPointsHelper] Header has existing buttons - adjusting CurrencyDisplay position");
            }

            GameObject currencyDisplay = new GameObject("CurrencyDisplay");
            currencyDisplay.transform.SetParent(header.transform, false);

            RectTransform currencyRT = currencyDisplay.AddComponent<RectTransform>();
            currencyRT.anchorMin = new Vector2(1, 0.5f);
            currencyRT.anchorMax = new Vector2(1, 0.5f);
            currencyRT.pivot = new Vector2(1, 0.5f);
            currencyRT.anchoredPosition = new Vector2(rightOffset, 0);
            currencyRT.sizeDelta = new Vector2(CURRENCY_DISPLAY_WIDTH, CURRENCY_DISPLAY_HEIGHT);

            HorizontalLayoutGroup hlg = currencyDisplay.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = CURRENCY_SPACING;
            hlg.childAlignment = TextAnchor.MiddleRight;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;

            var headerCurrency = currencyDisplay.AddComponent<Monetization.HeaderCurrencyDisplay>();

            // Gems Display
            GameObject gemsDisplay = CreateCurrencyItem(currencyDisplay, "GemsDisplay", "1,250", GEM_COLOR);
            // Coins Display
            GameObject coinsDisplay = CreateCurrencyItem(currencyDisplay, "CoinsDisplay", "5,430", _useCashTheme ? GOLD : COIN_COLOR);

            // Assign references
            SerializedObject so = new SerializedObject(headerCurrency);
            so.FindProperty("_gemsButton").objectReferenceValue = gemsDisplay.GetComponent<Button>();
            so.FindProperty("_coinsButton").objectReferenceValue = coinsDisplay.GetComponent<Button>();
            so.FindProperty("_gemsPlusButton").objectReferenceValue = gemsDisplay.transform.Find("PlusButton")?.GetComponent<Button>();
            so.FindProperty("_coinsPlusButton").objectReferenceValue = coinsDisplay.transform.Find("PlusButton")?.GetComponent<Button>();
            so.FindProperty("_gemsIcon").objectReferenceValue = gemsDisplay.transform.Find("Icon")?.GetComponent<Image>();
            so.FindProperty("_coinsIcon").objectReferenceValue = coinsDisplay.transform.Find("Icon")?.GetComponent<Image>();
            so.FindProperty("_gemsText").objectReferenceValue = gemsDisplay.transform.Find("Amount")?.GetComponent<TextMeshProUGUI>();
            so.FindProperty("_coinsText").objectReferenceValue = coinsDisplay.transform.Find("Amount")?.GetComponent<TextMeshProUGUI>();
            so.ApplyModifiedProperties();
        }

        private static GameObject CreateCurrencyItem(GameObject parent, string name, string amount, Color color)
        {
            GameObject item = new GameObject(name);
            item.transform.SetParent(parent.transform, false);

            RectTransform itemRT = item.AddComponent<RectTransform>();
            itemRT.sizeDelta = new Vector2(CURRENCY_ITEM_WIDTH, CURRENCY_ITEM_HEIGHT);

            Image itemBg = item.AddComponent<Image>();
            itemBg.color = new Color(0.1f, 0.15f, 0.2f, 0.9f);

            Outline outline = item.AddComponent<Outline>();
            outline.effectColor = color * 0.5f;
            outline.effectDistance = new Vector2(1, 1);

            Button itemBtn = item.AddComponent<Button>();
            SetupButtonColors(itemBtn);

            HorizontalLayoutGroup hlg = item.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.padding = new RectOffset(10, 10, 5, 5);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            LayoutElement itemLE = item.AddComponent<LayoutElement>();
            itemLE.minWidth = CURRENCY_ITEM_WIDTH;
            itemLE.preferredWidth = CURRENCY_ITEM_WIDTH;

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(item.transform, false);
            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = color;
            LayoutElement iconLE = iconObj.AddComponent<LayoutElement>();
            iconLE.minWidth = CURRENCY_ICON_SIZE;
            iconLE.minHeight = CURRENCY_ICON_SIZE;
            iconLE.preferredWidth = CURRENCY_ICON_SIZE;
            iconLE.preferredHeight = CURRENCY_ICON_SIZE;

            // Amount
            GameObject amountObj = new GameObject("Amount");
            amountObj.transform.SetParent(item.transform, false);
            TextMeshProUGUI amountText = amountObj.AddComponent<TextMeshProUGUI>();
            amountText.text = amount;
            amountText.fontSize = CURRENCY_FONTSIZE;
            amountText.fontStyle = FontStyles.Bold;
            amountText.color = TEXT_PRIMARY;
            amountText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement amountLE = amountObj.AddComponent<LayoutElement>();
            amountLE.flexibleWidth = 1;

            // Plus Button
            GameObject plusObj = new GameObject("PlusButton");
            plusObj.transform.SetParent(item.transform, false);
            Image plusBg = plusObj.AddComponent<Image>();
            plusBg.color = color;
            Button plusBtn = plusObj.AddComponent<Button>();
            SetupButtonColors(plusBtn);
            LayoutElement plusLE = plusObj.AddComponent<LayoutElement>();
            plusLE.minWidth = CURRENCY_PLUS_SIZE;
            plusLE.minHeight = CURRENCY_PLUS_SIZE;
            plusLE.preferredWidth = CURRENCY_PLUS_SIZE;
            plusLE.preferredHeight = CURRENCY_PLUS_SIZE;

            GameObject plusTextObj = new GameObject("Text");
            plusTextObj.transform.SetParent(plusObj.transform, false);
            TextMeshProUGUI plusText = plusTextObj.AddComponent<TextMeshProUGUI>();
            plusText.text = "+";
            plusText.fontSize = 18;
            plusText.fontStyle = FontStyles.Bold;
            plusText.color = TEXT_DARK;
            plusText.alignment = TextAlignmentOptions.Center;
            RectTransform plusTextRT = plusTextObj.GetComponent<RectTransform>();
            plusTextRT.anchorMin = Vector2.zero;
            plusTextRT.anchorMax = Vector2.one;
            plusTextRT.sizeDelta = Vector2.zero;

            return item;
        }

        // ==================== CREATE - BOTTOM NAV BAR ====================

        private static void CreateBottomNavBar(GameObject parent)
        {
            Transform existing = parent.transform.Find("BottomNavBar");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            GameObject navBar = new GameObject("BottomNavBar");
            navBar.transform.SetParent(parent.transform, false);

            RectTransform navRT = navBar.AddComponent<RectTransform>();
            navRT.anchorMin = new Vector2(0, 0);
            navRT.anchorMax = new Vector2(1, 0);
            navRT.pivot = new Vector2(0.5f, 0);
            navRT.anchoredPosition = Vector2.zero;
            navRT.sizeDelta = new Vector2(0, NAVBAR_HEIGHT);

            Image navBg = navBar.AddComponent<Image>();
            navBg.color = HEADER_BG;

            navBar.transform.SetAsLastSibling();

            var bottomNav = navBar.AddComponent<UI.BottomNavBar>();

            HorizontalLayoutGroup hlg = navBar.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 0;
            hlg.padding = new RectOffset(NAVBAR_PADDING_HORIZONTAL, NAVBAR_PADDING_HORIZONTAL, NAVBAR_PADDING_TOP, NAVBAR_PADDING_BOTTOM);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;

            // Create nav items
            CreateNavItem(navBar, "HomeButton", "Inicio", _accentColor, true);
            CreateNavItem(navBar, "PlayButton", "Jugar", _accentColor, false);
            CreateNavItem(navBar, "ShopButton", "Tienda", GOLD, false); // Shop always gold
            CreateNavItem(navBar, "MissionsButton", "Misiones", _accentColor, false);
            CreateNavItem(navBar, "ProfileButton", "Perfil", _accentColor, false);

            // Top glow
            GameObject topGlow = new GameObject("TopGlow");
            topGlow.transform.SetParent(navBar.transform, false);
            RectTransform glowRT = topGlow.AddComponent<RectTransform>();
            glowRT.anchorMin = new Vector2(0, 1);
            glowRT.anchorMax = new Vector2(1, 1);
            glowRT.pivot = new Vector2(0.5f, 0);
            glowRT.anchoredPosition = Vector2.zero;
            glowRT.sizeDelta = new Vector2(0, 2);

            Image glowImage = topGlow.AddComponent<Image>();
            glowImage.color = _accentColor;

            // Assign references to BottomNavBar
            AssignNavBarReferences(bottomNav, navBar);
        }

        private static void CreateNavItem(GameObject parent, string name, string label, Color color, bool isActive)
        {
            GameObject item = new GameObject(name);
            item.transform.SetParent(parent.transform, false);

            Button itemBtn = item.AddComponent<Button>();
            SetupButtonColors(itemBtn);

            VerticalLayoutGroup vlg = item.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 4;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            Color inactiveColor = new Color(0.5f, 0.55f, 0.6f, 1f);

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(item.transform, false);
            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = isActive ? color : inactiveColor;
            LayoutElement iconLE = iconObj.AddComponent<LayoutElement>();
            iconLE.minWidth = NAVBAR_ICON_SIZE;
            iconLE.minHeight = NAVBAR_ICON_SIZE;
            iconLE.preferredWidth = NAVBAR_ICON_SIZE;
            iconLE.preferredHeight = NAVBAR_ICON_SIZE;

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(item.transform, false);
            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = NAVBAR_LABEL_FONTSIZE;
            labelText.color = isActive ? color : inactiveColor;
            labelText.alignment = TextAlignmentOptions.Center;
            LayoutElement labelLE = labelObj.AddComponent<LayoutElement>();
            labelLE.minHeight = 16;

            // Active Indicator
            GameObject indicator = new GameObject("Indicator");
            indicator.transform.SetParent(item.transform, false);
            Image indicatorImage = indicator.AddComponent<Image>();
            indicatorImage.color = color;
            LayoutElement indicatorLE = indicator.AddComponent<LayoutElement>();
            indicatorLE.minHeight = NAVBAR_INDICATOR_HEIGHT;
            indicatorLE.preferredHeight = NAVBAR_INDICATOR_HEIGHT;
            indicator.SetActive(isActive);
        }

        private static void AssignNavBarReferences(UI.BottomNavBar bottomNav, GameObject navBar)
        {
            SerializedObject so = new SerializedObject(bottomNav);

            string[] buttonNames = { "HomeButton", "PlayButton", "ShopButton", "MissionsButton", "ProfileButton" };
            string[] propertyNames = { "_homeButton", "_playButton", "_shopButton", "_missionsButton", "_profileButton" };

            for (int i = 0; i < buttonNames.Length; i++)
            {
                Transform btnTransform = navBar.transform.Find(buttonNames[i]);
                if (btnTransform != null)
                {
                    SerializedProperty navBtnProp = so.FindProperty(propertyNames[i]);
                    navBtnProp.FindPropertyRelative("button").objectReferenceValue = btnTransform.GetComponent<Button>();
                    navBtnProp.FindPropertyRelative("icon").objectReferenceValue = btnTransform.Find("Icon")?.GetComponent<Image>();
                    navBtnProp.FindPropertyRelative("label").objectReferenceValue = btnTransform.Find("Label")?.GetComponent<TextMeshProUGUI>();
                    navBtnProp.FindPropertyRelative("indicator").objectReferenceValue = btnTransform.Find("Indicator")?.GetComponent<Image>();
                }
            }

            so.ApplyModifiedProperties();
        }

        // ==================== CREATE - PREMIUM CARD ====================

        private static void CreatePremiumCard(GameObject parent)
        {
            Transform existing = parent.transform.Find("PremiumCard");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            GameObject premiumCard = new GameObject("PremiumCard");
            premiumCard.transform.SetParent(parent.transform, false);

            RectTransform cardRT = premiumCard.AddComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0, 0);
            cardRT.anchorMax = new Vector2(1, 0);
            cardRT.pivot = new Vector2(0.5f, 0);
            cardRT.anchoredPosition = new Vector2(0, NAVBAR_HEIGHT + 10); // Above BottomNavBar
            cardRT.sizeDelta = new Vector2(-40, PREMIUM_CARD_HEIGHT);

            Image cardBg = premiumCard.AddComponent<Image>();
            cardBg.color = new Color(0.15f, 0.1f, 0.25f, 0.95f);

            Outline outline = premiumCard.AddComponent<Outline>();
            outline.effectColor = PURPLE_PREMIUM;
            outline.effectDistance = new Vector2(2, 2);

            Button cardBtn = premiumCard.AddComponent<Button>();
            SetupButtonColors(cardBtn);

            var premiumCardComp = premiumCard.AddComponent<UI.PremiumCard>();

            HorizontalLayoutGroup hlg = premiumCard.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 15;
            hlg.padding = new RectOffset(20, 20, 15, 15);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(premiumCard.transform, false);
            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = PURPLE_PREMIUM;
            LayoutElement iconLE = iconObj.AddComponent<LayoutElement>();
            iconLE.minWidth = PREMIUM_ICON_SIZE;
            iconLE.minHeight = PREMIUM_ICON_SIZE;
            iconLE.preferredWidth = PREMIUM_ICON_SIZE;
            iconLE.preferredHeight = PREMIUM_ICON_SIZE;

            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(premiumCard.transform, false);
            VerticalLayoutGroup contentVlg = content.AddComponent<VerticalLayoutGroup>();
            contentVlg.spacing = 4;
            contentVlg.childAlignment = TextAnchor.MiddleLeft;
            contentVlg.childControlWidth = true;
            contentVlg.childControlHeight = true;
            contentVlg.childForceExpandHeight = false;
            LayoutElement contentLE = content.AddComponent<LayoutElement>();
            contentLE.flexibleWidth = 1;

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(content.transform, false);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "OFERTA ESPECIAL";
            titleText.fontSize = PREMIUM_TITLE_FONTSIZE;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = GOLD;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;

            // Subtitle
            GameObject subtitleObj = new GameObject("Subtitle");
            subtitleObj.transform.SetParent(content.transform, false);
            TextMeshProUGUI subtitleText = subtitleObj.AddComponent<TextMeshProUGUI>();
            subtitleText.text = "500 Gemas + Tema Exclusivo";
            subtitleText.fontSize = PREMIUM_SUBTITLE_FONTSIZE;
            subtitleText.color = TEXT_SECONDARY;
            subtitleText.alignment = TextAlignmentOptions.MidlineLeft;

            // Price button
            GameObject priceBtn = new GameObject("PriceButton");
            priceBtn.transform.SetParent(premiumCard.transform, false);
            Image priceBg = priceBtn.AddComponent<Image>();
            priceBg.color = new Color(0.2f, 0.8f, 0.4f, 1f);
            LayoutElement priceLE = priceBtn.AddComponent<LayoutElement>();
            priceLE.minWidth = PREMIUM_PRICE_WIDTH;
            priceLE.minHeight = PREMIUM_PRICE_HEIGHT;
            priceLE.preferredWidth = PREMIUM_PRICE_WIDTH;

            GameObject priceText = new GameObject("Text");
            priceText.transform.SetParent(priceBtn.transform, false);
            TextMeshProUGUI priceTmp = priceText.AddComponent<TextMeshProUGUI>();
            priceTmp.text = "$2.99";
            priceTmp.fontSize = 16;
            priceTmp.fontStyle = FontStyles.Bold;
            priceTmp.color = TEXT_DARK;
            priceTmp.alignment = TextAlignmentOptions.Center;
            RectTransform priceTextRT = priceText.GetComponent<RectTransform>();
            priceTextRT.anchorMin = Vector2.zero;
            priceTextRT.anchorMax = Vector2.one;
            priceTextRT.sizeDelta = Vector2.zero;

            // Assign references
            SerializedObject so = new SerializedObject(premiumCardComp);
            so.FindProperty("_cardButton").objectReferenceValue = cardBtn;
            so.FindProperty("_backgroundImage").objectReferenceValue = cardBg;
            so.FindProperty("_iconImage").objectReferenceValue = iconImage;
            so.FindProperty("_titleText").objectReferenceValue = titleText;
            so.FindProperty("_subtitleText").objectReferenceValue = subtitleText;
            so.FindProperty("_priceText").objectReferenceValue = priceTmp;
            so.ApplyModifiedProperties();
        }

        // ==================== UTILITY METHODS ====================

        private static Canvas GetCanvas()
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[ShopEntryPointsHelper] No Canvas found in scene");
            }
            return canvas;
        }

        private static Transform FindHeader(Canvas canvas)
        {
            string[] headerPaths = {
                "SafeArea/Header",
                "Header",
                "SafeArea/TopBar",
                "TopBar",
                "SafeArea/HeaderPanel",
                "HeaderPanel"
            };

            foreach (var path in headerPaths)
            {
                Transform found = canvas.transform.Find(path);
                if (found != null) return found;
            }

            return null;
        }

        private static GameObject GetOrCreateParent(Canvas canvas)
        {
            Transform safeArea = canvas.transform.Find("SafeArea");
            if (safeArea != null)
            {
                return safeArea.gameObject;
            }

            Debug.Log("[ShopEntryPointsHelper] No SafeArea found - using Canvas as parent");
            return canvas.gameObject;
        }

        private static void RemoveElement(string elementName)
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null) return;

            Transform safeArea = canvas.transform.Find("SafeArea");
            Transform parent = safeArea != null ? safeArea : canvas.transform;

            Transform element = parent.Find(elementName);
            if (element != null)
            {
                Object.DestroyImmediate(element.gameObject);
                MarkSceneDirty();
                Debug.Log($"[ShopEntryPointsHelper] {elementName} removed!");
            }
        }

        private static void RemoveFromHeader(string elementName)
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null) return;

            Transform header = FindHeader(canvas);
            if (header != null)
            {
                Transform element = header.Find(elementName);
                if (element != null)
                {
                    Object.DestroyImmediate(element.gameObject);
                    MarkSceneDirty();
                    Debug.Log($"[ShopEntryPointsHelper] {elementName} removed from header!");
                    return;
                }
            }

            // Also check canvas directly for compact button
            Transform canvasElement = canvas.transform.Find(elementName);
            if (canvasElement != null)
            {
                Object.DestroyImmediate(canvasElement.gameObject);
                MarkSceneDirty();
                Debug.Log($"[ShopEntryPointsHelper] {elementName} removed!");
            }
        }

        private static void RemoveExistingCompactButton(Canvas canvas)
        {
            string[] searchLocations = { "SafeArea", "SafeArea/Header", "Header", "" };
            foreach (var loc in searchLocations)
            {
                Transform parent = string.IsNullOrEmpty(loc) ? canvas.transform : canvas.transform.Find(loc);
                if (parent != null)
                {
                    Transform existing = parent.Find("CompactCurrencyButton");
                    if (existing != null)
                    {
                        Object.DestroyImmediate(existing.gameObject);
                    }
                }
            }
        }

        private static void SetupButtonColors(Button btn)
        {
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            btn.colors = colors;
        }

        private static void MarkSceneDirty()
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
    }
}
