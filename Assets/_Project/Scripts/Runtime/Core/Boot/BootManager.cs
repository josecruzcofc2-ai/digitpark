using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using DigitPark.Services;
using DigitPark.Services.Firebase;
using DigitPark.Localization;
using DigitPark.UI;
using DigitPark.Payments;
using DigitPark.Payments.Entitlements;
using Firebase.Database;
using DigitPark.Navigation;
using DigitPark.Themes;
using DigitPark.Progression;
using DigitPark.Cosmetics;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager de la escena Boot
    /// Inicializa servicios, verifica autenticación y redirige al usuario
    /// </summary>
    public class BootManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] public Image loadingBar;
        [SerializeField] public TextMeshProUGUI loadingText;
        [SerializeField] public TextMeshProUGUI versionText;

        [Header("Settings")]
        [SerializeField] public float minimumLoadTime = 2f;

        private bool servicesInitialized = false;
        private DigitPark.UI.BootAnimator bootAnimator;
        private bool _consentGranted = false;

        private void Start()
        {
#if !UNITY_EDITOR
            // S3-NEW-01: Ensure debug bypass key never persists in production builds
            PlayerPrefs.DeleteKey("CashBattleBypassAuth");
#endif
            Debug.Log("[Boot] Iniciando BootManager...");

            // Buscar BootAnimator en la escena
            bootAnimator = FindFirstObjectByType<DigitPark.UI.BootAnimator>();

            // Configurar versión
            if (versionText != null)
            {
                versionText.text = $"v{Application.version}";
            }

            // Iniciar proceso de boot
            StartCoroutine(BootSequence());
        }

        /// <summary>
        /// Secuencia principal de inicialización
        /// </summary>
        private IEnumerator BootSequence()
        {
            float startTime = Time.time;

            // Paso 1: Inicializar configuraciones básicas + NetworkService + ATT
            yield return StartCoroutine(InitializeBasicSettings());
            UpdateLoadingProgress(0.15f, "boot_initializing_config");

            // Paso 1.5: Solicitar consentimiento GDPR si aún no se ha dado
            yield return StartCoroutine(RequestGDPRConsentIfNeeded());

            // Paso 2: Solicitar ATT (iOS) - ANTES de Firebase Analytics
            yield return StartCoroutine(RequestTrackingAuthorization());
            UpdateLoadingProgress(0.25f, "boot_initializing_config");

            // Paso 3: Inicializar servicios de Firebase
            yield return StartCoroutine(InitializeFirebaseServices());
            UpdateLoadingProgress(0.5f, "boot_connecting_services");

            // Paso 3.2: Sincronizar offset de tiempo del servidor (previene manipulación del reloj)
            var serverTimeTask = ServerTimeHelper.RefreshOffsetAsync();
            yield return new WaitUntil(() => serverTimeTask.IsCompleted);

            // Paso 3.5: Inicializar sistema de pagos
            DigitPark.Services.PaymentBridgeWiring.Wire();
            DigitPark.Services.AppleIAPBridge.WireDelegate();
            yield return StartCoroutine(InitializePaymentSystem());
            UpdateLoadingProgress(0.6f, "boot_loading_payments");

            // Paso 4: Inicializar managers del juego
            yield return StartCoroutine(InitializeGameManagers());
            UpdateLoadingProgress(0.7f, "boot_loading_resources");

            // Paso 5: Verificar estado de autenticación
            yield return StartCoroutine(CheckAuthenticationStatus());
            UpdateLoadingProgress(0.9f, "boot_verifying_user");

            // Asegurar tiempo mínimo de carga (para mostrar logo/branding)
            float elapsedTime = Time.time - startTime;
            if (elapsedTime < minimumLoadTime)
            {
                yield return new WaitForSeconds(minimumLoadTime - elapsedTime);
            }

            UpdateLoadingProgress(1f, "boot_completed");

            // Auto-etiquetar accesibilidad de la escena actual
            AccessibilityHelper.AutoLabelScene();

            yield return new WaitForSeconds(0.5f);

            // Redirigir a la escena apropiada
            RedirectToScene();
        }

        /// <summary>
        /// Inicializa configuraciones básicas del juego
        /// </summary>
        private IEnumerator InitializeBasicSettings()
        {
            Debug.Log("[Boot] Inicializando configuraciones básicas...");

            // Configurar target frame rate
            Application.targetFrameRate = 60;

            // Configurar orientación
            Screen.orientation = ScreenOrientation.Portrait;

            // Inicializar Safe Area Manager (para dispositivos con notch/cámara)
            SafeAreaManager.Initialize();
            Debug.Log("[Boot] SafeAreaManager inicializado");

            // Crear NetworkService (antes de Firebase para monitorear conectividad)
            if (NetworkService.Instance == null)
            {
                GameObject networkObj = new GameObject("NetworkService");
                networkObj.AddComponent<NetworkService>();
                Debug.Log("[Boot] NetworkService creado");
            }

            // Crear NetworkStatusBanner (UI de estado de red)
            if (NetworkStatusBanner.Instance == null)
            {
                GameObject bannerObj = new GameObject("NetworkStatusBanner");
                bannerObj.AddComponent<NetworkStatusBanner>();
                Debug.Log("[Boot] NetworkStatusBanner creado");
            }

            // Crear ReviewService
            if (ReviewService.Instance == null)
            {
                GameObject reviewObj = new GameObject("ReviewService");
                reviewObj.AddComponent<ReviewService>();
                Debug.Log("[Boot] ReviewService creado");
            }
            ReviewService.Instance?.IncrementSessionCount();

            // Crear DeepLinkService
            if (DeepLinkService.Instance == null)
            {
                GameObject deepLinkObj = new GameObject("DeepLinkService");
                deepLinkObj.AddComponent<DeepLinkService>();
                Debug.Log("[Boot] DeepLinkService creado");
            }

            // Crear BackButtonManager (Android back button global handler)
            if (DigitPark.Services.BackButtonManager.Instance == null)
            {
                GameObject backBtnObj = new GameObject("BackButtonManager");
                backBtnObj.AddComponent<DigitPark.Services.BackButtonManager>();
                Debug.Log("[Boot] BackButtonManager creado");
            }

            // Cargar configuraciones guardadas del jugador
            LoadPlayerPreferences();

            yield return null;
        }

        /// <summary>
        /// Solicita permiso de App Tracking Transparency (iOS 14.5+)
        /// Debe ejecutarse ANTES de inicializar Firebase Analytics
        /// </summary>
        private IEnumerator RequestTrackingAuthorization()
        {
            // Crear ATTService
            if (ATTService.Instance == null)
            {
                GameObject attObj = new GameObject("ATTService");
                attObj.AddComponent<ATTService>();
                Debug.Log("[Boot] ATTService creado");
            }

            // Solicitar permiso
            ATTService.Instance.RequestTrackingAuthorization();

            // Esperar respuesta (maximo 30 segundos)
            float timeout = 30f;
            float elapsed = 0f;
            while (!ATTService.Instance.RequestCompleted && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            Debug.Log($"[Boot] ATT completado: {ATTService.Instance.TrackingStatus}");
        }

        /// <summary>
        /// Inicializa los servicios de Firebase
        /// </summary>
        private IEnumerator InitializeFirebaseServices()
        {
            Debug.Log("[Boot] Inicializando servicios...");

            // Crear LocalizationManager (automáticamente crea AutoLocalizer)
            if (LocalizationManager.Instance == null)
            {
                GameObject localizationService = new GameObject("LocalizationManager");
                localizationService.AddComponent<LocalizationManager>();
                Debug.Log("[Boot] LocalizationManager creado");
            }

            // Verificar que los servicios existan en la escena o crearlos
            if (AuthenticationService.Instance == null)
            {
                GameObject authService = new GameObject("AuthenticationService");
                authService.AddComponent<AuthenticationService>();
            }

            if (DatabaseService.Instance == null)
            {
                GameObject dbService = new GameObject("DatabaseService");
                dbService.AddComponent<DatabaseService>();
            }

            // Only initialize analytics if user has given GDPR consent
            if (ConsentService.HasConsent() && AnalyticsService.Instance == null)
            {
                GameObject analyticsService = new GameObject("AnalyticsService");
                analyticsService.AddComponent<AnalyticsService>();
            }

            // Crear AchievementService
            if (AchievementService.Instance == null)
            {
                GameObject achievementService = new GameObject("AchievementService");
                achievementService.AddComponent<AchievementService>();
                Debug.Log("[Boot] AchievementService creado");
            }

            // Wait for AuthenticationService to finish Firebase init (max 10s timeout)
            float timeout = 10f;
            float elapsed = 0f;
            while (AuthenticationService.Instance != null && !AuthenticationService.Instance.IsInitialized && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (elapsed >= timeout)
                Debug.LogError("[Boot] Firebase init timeout (10s) — services may not be ready");

            servicesInitialized = true;
            Debug.Log($"[Boot] Todos los servicios inicializados ({elapsed:F1}s)");

            // One-time PlayerPrefs → Firebase migration (economy rebalance V55)
            yield return StartCoroutine(MigratePlayerPrefsToFirebase());
        }

        /// <summary>
        /// One-time migration: reads currency, cosmetics, achievements, progression from PlayerPrefs
        /// and syncs to Firebase as source-of-truth. After migration, Firebase is master, PlayerPrefs is cache.
        ///
        /// D-23/D-25/D-75 FIX: On reinstall, PlayerPrefs are empty so local values are defaults (gems=0).
        /// Before pushing, we check if Firebase already has real data. If it does, this is a reinstall
        /// and we RESTORE from Firebase instead of overwriting with zeroes.
        /// </summary>
        private IEnumerator MigratePlayerPrefsToFirebase()
        {
            const string MIGRATION_FLAG = "playerprefs_migrated_to_firebase";
            if (PlayerPrefs.GetInt(MIGRATION_FLAG, 0) == 1)
            {
                yield break; // Already migrated
            }

            var auth = AuthenticationService.Instance;
            if (auth == null || !auth.IsUserAuthenticated())
            {
                Debug.Log("[Boot] Migration skipped — no authenticated user");
                yield break;
            }

            var playerData = auth.GetCurrentPlayerData();
            if (playerData == null)
            {
                Debug.Log("[Boot] Migration skipped — no player data");
                yield break;
            }

            Debug.Log("[Boot] Starting one-time PlayerPrefs → Firebase migration...");

            // ── D-23 FIX: Check Firebase for existing data BEFORE deciding to push ──
            // On reinstall, PlayerPrefs are wiped → CurrencyManager.Gems == 0 (DEFAULT_GEMS).
            // If Firebase already has real data, this is a REINSTALL — restore, don't overwrite.
            bool firebaseHasData = false;
            int firebaseGems = 0;
            int firebaseCoins = 0;

            var dbRef = FirebaseDatabase.DefaultInstance?.RootReference;
            if (dbRef != null)
            {
                // Read gems from Firebase (5s timeout per read)
                var gemsTask = dbRef.Child("players").Child(playerData.userId).Child("gems").GetValueAsync();
                float migTimeout = 5f;
                float migElapsed = 0f;
                while (!gemsTask.IsCompleted && migElapsed < migTimeout)
                {
                    migElapsed += Time.deltaTime;
                    yield return null;
                }

                if (gemsTask.IsCompleted && !gemsTask.IsFaulted && gemsTask.Result != null && gemsTask.Result.Exists)
                {
                    try { firebaseGems = System.Convert.ToInt32(gemsTask.Result.Value); }
                    catch { firebaseGems = 0; }
                }

                // Read coins from Firebase (5s timeout)
                var coinsTask = dbRef.Child("players").Child(playerData.userId).Child("coins").GetValueAsync();
                migElapsed = 0f;
                while (!coinsTask.IsCompleted && migElapsed < migTimeout)
                {
                    migElapsed += Time.deltaTime;
                    yield return null;
                }

                if (!coinsTask.IsFaulted && coinsTask.Result != null && coinsTask.Result.Exists)
                {
                    try { firebaseCoins = System.Convert.ToInt32(coinsTask.Result.Value); }
                    catch { firebaseCoins = 0; }
                }

                if (firebaseGems > 0 || firebaseCoins > 0)
                {
                    firebaseHasData = true;
                }

                // Also check totalGamesPlayed / username as proxy for "real account"
                if (!firebaseHasData)
                {
                    var loadTask = DatabaseService.Instance?.LoadPlayerData(playerData.userId);
                    if (loadTask != null)
                    {
                        while (!loadTask.IsCompleted)
                            yield return null;

                        if (!loadTask.IsFaulted && loadTask.Result != null)
                        {
                            var existing = loadTask.Result;
                            if (existing.totalGamesPlayed > 0 ||
                                (!string.IsNullOrEmpty(existing.username) && existing.username != "Player"))
                            {
                                firebaseHasData = true;
                            }
                        }
                    }
                }
            }

            // ── REINSTALL PATH: Firebase has real data, PlayerPrefs are empty ──
            if (firebaseHasData)
            {
                Debug.Log($"[Boot] Reinstall detected — Firebase has data (gems={firebaseGems}, coins={firebaseCoins}). Restoring instead of migrating.");

                try
                {
                    var currency = DigitPark.Monetization.CurrencyManager.Instance;
                    if (currency != null)
                    {
                        currency.RestoreFromFirebaseValues(firebaseGems, firebaseCoins);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[Boot] Reinstall restore error: {e.Message}");
                }

                // B4-A/B/C/D/E: Restore cosmetics + progression from Firebase on reinstall
                string uid = playerData.userId;
                var restoreTasks = new List<Task>
                {
                    ThemeManager.Instance?.RestoreFromFirebaseAsync(uid) ?? Task.CompletedTask,
                    PlayerFrameService.Instance?.RestoreFromFirebaseAsync(uid) ?? Task.CompletedTask,
                    PlayerTitleService.Instance?.RestoreFromFirebaseAsync(uid) ?? Task.CompletedTask,
                    PlayerProgressionSystem.Instance?.RestoreFromFirebaseAsync(uid) ?? Task.CompletedTask,
                    // B4-E: BackgroundPatternManager restore
                    BackgroundPatternManager.Instance?.RestoreFromFirebaseAsync(uid) ?? Task.CompletedTask,
                };
                var restoreTask = Task.WhenAll(restoreTasks);
                while (!restoreTask.IsCompleted) yield return null;

                PlayerPrefs.SetInt(MIGRATION_FLAG, 1);
                PlayerPrefs.Save();
                Debug.Log("[Boot] Reinstall restore complete — Firebase data preserved");
                yield break;
            }

            // ── FIRST-TIME PATH: No Firebase data — push local values (fresh account) ──
            Debug.Log("[Boot] First-time migration — pushing local data to Firebase...");

            var updates = new Dictionary<string, object>();

            try
            {
                // Currency
                var currencyMgr = DigitPark.Monetization.CurrencyManager.Instance;
                if (currencyMgr != null)
                {
                    updates["gems"] = currencyMgr.Gems;
                    updates["coins"] = currencyMgr.Coins;
                }

                // Daily Rewards
                string dailyData = PlayerPrefs.GetString("DailyReward_Data", "");
                if (!string.IsNullOrEmpty(dailyData))
                {
                    updates["dailyReward/localData"] = dailyData;
                }

                // Daily Rewards Manager streak
                int streak = PlayerPrefs.GetInt("DailyRewards_Streak", 0);
                int currentDay = PlayerPrefs.GetInt("DailyRewards_CurrentDay", 0);
                if (streak > 0 || currentDay > 0)
                {
                    updates["dailyRewardsManager/streak"] = streak;
                    updates["dailyRewardsManager/currentDay"] = currentDay;
                    updates["dailyRewardsManager/lastClaim"] = PlayerPrefs.GetString("DailyRewards_LastClaim", "");
                }

                // XP / Level
                int xp = PlayerPrefs.GetInt("DP_PlayerXP", 0);
                int level = PlayerPrefs.GetInt("DP_PlayerLevel", 1);
                if (xp > 0 || level > 1)
                {
                    updates["progression/xp"] = xp;
                    updates["progression/level"] = level;
                }

                // B4-F: Cosmetics migration — trigger each service to sync its owned data to Firebase.
                // Each service's Save method already writes the correct format.
                PlayerFrameService.Instance?.TriggerFirebaseSync();
                PlayerTitleService.Instance?.TriggerFirebaseSync();
                var themeManager = ThemeManager.Instance;
                if (themeManager != null)
                {
                    var ownedThemeIds = themeManager.GetOwnedThemeIds();
                    if (ownedThemeIds != null && ownedThemeIds.Count > 0)
                        updates["ownedThemes"] = string.Join(",", ownedThemeIds);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Boot] Migration data collection error: {e.Message}");
            }

            // Push to Firebase (outside try-catch so yield works)
            if (updates.Count > 0 && DatabaseService.Instance != null)
            {
                var task = DatabaseService.Instance.UpdatePlayerFields(playerData.userId, updates);
                while (!task.IsCompleted)
                    yield return null;

                if (task.IsFaulted)
                {
                    Debug.LogWarning($"[Boot] Migration failed: {task.Exception?.GetBaseException().Message}");
                    yield break; // Don't set flag — retry next boot
                }
            }

            PlayerPrefs.SetInt(MIGRATION_FLAG, 1);
            PlayerPrefs.Save();
            Debug.Log($"[Boot] Migration complete — {updates.Count} fields synced to Firebase");
        }

        /// <summary>
        /// Inicializa el sistema de pagos (Stripe + AppleIAP + FeatureFlags + Entitlements)
        /// </summary>
        private IEnumerator InitializePaymentSystem()
        {
            Debug.Log("[Boot] Inicializando sistema de pagos...");

            // 1. Crear RemoteConfigService
            if (RemoteConfigService.Instance == null)
            {
                GameObject remoteConfigObj = new GameObject("RemoteConfigService");
                remoteConfigObj.AddComponent<RemoteConfigService>();
                Debug.Log("[Boot] RemoteConfigService creado");
            }

            // Esperar a que RemoteConfig esté listo (máximo 5 segundos, usa cache si offline)
            float timeout = 5f;
            float elapsed = 0f;
            while (RemoteConfigService.Instance != null &&
                   !RemoteConfigService.Instance.IsReady && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // 2. Inicializar PaymentFeatureFlag
            PaymentFeatureFlag.Initialize(RemoteConfigService.Instance?.GetCurrentConfig());
            Debug.Log($"[Boot] FeatureFlag inicializado. Provider: {PaymentFeatureFlag.ActiveCosmeticProvider}");

            // 3. Crear PaymentManager
            if (PaymentManager.Instance == null)
            {
                GameObject paymentObj = new GameObject("PaymentManager");
                paymentObj.AddComponent<PaymentManager>();
                Debug.Log("[Boot] PaymentManager creado");
            }

            // 4. Crear EntitlementService
            if (EntitlementService.Instance == null)
            {
                GameObject entitlementObj = new GameObject("EntitlementService");
                entitlementObj.AddComponent<EntitlementService>();
                Debug.Log("[Boot] EntitlementService creado");
            }

#if DIGIT_PARK_PRO || UNITY_EDITOR
            // 5. Crear TriumphIsolationGuard (solo en versión Pro)
            if (DigitPark.Payments.Compliance.TriumphIsolationGuard.Instance == null)
            {
                GameObject guardObj = new GameObject("TriumphIsolationGuard");
                guardObj.AddComponent<DigitPark.Payments.Compliance.TriumphIsolationGuard>();
                Debug.Log("[Boot] TriumphIsolationGuard creado");
            }
#endif

            // 6. Sync entitlements con retry — B4-G
            if (EntitlementService.Instance != null)
            {
                _ = SyncEntitlementsWithRetry();
            }

            yield return new WaitForSeconds(0.1f);
            Debug.Log("[Boot] Sistema de pagos inicializado correctamente");
        }

        /// <summary>
        /// B4-G: Sync entitlements with server, retrying up to 3 times on failure.
        /// </summary>
        private async Task SyncEntitlementsWithRetry()
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    await EntitlementService.Instance.SyncWithServer();
                    Debug.Log("[Boot] Entitlements synced successfully");
                    return;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Boot] Entitlement sync attempt {i + 1}/3 failed: {e.Message}");
                    if (i < 2) await Task.Delay(2000 * (i + 1));
                }
            }
            Debug.LogWarning("[Boot] Entitlement sync failed after 3 attempts — using local cache");
        }

        /// <summary>
        /// Inicializa los managers principales del juego
        /// </summary>
        private IEnumerator InitializeGameManagers()
        {
            Debug.Log("[Boot] Inicializando managers del juego...");

            // BattleCardService — cosmético de matchmaking (independiente de ThemeManager)
            if (DigitPark.Cosmetics.BattleCardService.Instance == null)
            {
                GameObject bcObj = new GameObject("BattleCardService");
                bcObj.AddComponent<DigitPark.Cosmetics.BattleCardService>();
                Debug.Log("[Boot] BattleCardService creado");
            }

            // DailyOfferService — daily rotating offers (Economy Rebalance V55)
            if (DailyOfferService.Instance == null)
            {
                GameObject doObj = new GameObject("DailyOfferService");
                doObj.AddComponent<DailyOfferService>();
                Debug.Log("[Boot] DailyOfferService creado");
            }

            // WelcomePackService — new player conversion packs (Economy Rebalance V55)
            if (DigitPark.Monetization.WelcomePackService.Instance == null)
            {
                GameObject wpObj = new GameObject("WelcomePackService");
                wpObj.AddComponent<DigitPark.Monetization.WelcomePackService>();
                Debug.Log("[Boot] WelcomePackService creado");
            }

            // RotatingContentService — whale ceiling expansion (Economy Rebalance V55 / 13F)
            // Seasonal BattleCards + Monthly Frames + Limited Theme Variants
            if (RotatingContentService.Instance == null)
            {
                GameObject rcObj = new GameObject("RotatingContentService");
                rcObj.AddComponent<RotatingContentService>();
                Debug.Log("[Boot] RotatingContentService creado");
            }

            // Estos managers se crearán en sus respectivas escenas
            // Aquí solo preparamos el entorno

            yield return new WaitForSeconds(0.3f);

            Debug.Log("[Boot] Managers del juego inicializados");
        }

        /// <summary>
        /// Verifica el estado de autenticación del usuario
        /// </summary>
        private IEnumerator CheckAuthenticationStatus()
        {
            Debug.Log("[Boot] Verificando estado de autenticación...");

            if (!servicesInitialized)
            {
                Debug.LogWarning("[Boot] Servicios no inicializados, saltando verificación");
                yield break;
            }

            // Verificar si hay un usuario autenticado
            bool isAuthenticated = AuthenticationService.Instance != null && AuthenticationService.Instance.IsUserAuthenticated();

            Debug.Log($"[Boot] Usuario autenticado: {isAuthenticated}");

            yield return null;
        }

        /// <summary>
        /// Redirige a la escena apropiada según el estado de autenticación
        /// </summary>
        private void RedirectToScene()
        {
            string targetScene;

            if (AuthenticationService.Instance != null &&
                AuthenticationService.Instance.IsUserAuthenticated())
            {
                Debug.Log("[Boot] Usuario autenticado, redirigiendo a MainMenu");
                targetScene = "MainMenu";

                // Registrar login en analytics
                var playerData = AuthenticationService.Instance.GetCurrentPlayerData();
                if (playerData != null)
                {
                    AnalyticsService.Instance?.SetUserId(playerData.userId);
                    AnalyticsService.Instance?.SetUserCountry(playerData.countryCode);
                }
            }
            else
            {
                Debug.Log("[Boot] Usuario no autenticado, redirigiendo a Login");
                targetScene = "Login";
            }

            // Cargar escena de destino (SceneNavigator may not be initialized at boot time)
            if (SceneNavigator.Instance != null)
                SceneNavigator.Instance.NavigateTo(targetScene);
            else
                SceneManager.LoadScene(targetScene);
        }

        /// <summary>
        /// Actualiza la barra de progreso y el texto
        /// </summary>
        private void UpdateLoadingProgress(float progress, string localizationKey)
        {
            if (loadingBar != null)
            {
                loadingBar.fillAmount = progress;
            }

            // Obtener texto localizado
            string displayText = AutoLocalizer.Get(localizationKey);

            // Usar BootAnimator para efectos si está disponible
            if (bootAnimator != null)
            {
                // Efecto typewriter para el texto
                bootAnimator.SetLoadingText(displayText);

                // Actualizar color de la barra según progreso
                bootAnimator.UpdateLoadingBarColor(progress);
            }
            else if (loadingText != null)
            {
                // Fallback sin animación
                loadingText.text = displayText;
            }

            Debug.Log($"[Boot] {(progress * 100):F0}% - {localizationKey}");
        }

        /// <summary>
        /// Carga las preferencias del jugador
        /// </summary>
        private void LoadPlayerPreferences()
        {
            // Cargar configuraciones básicas de PlayerPrefs
            if (PlayerPrefs.HasKey("DP_MusicVolume"))
            {
                AudioListener.volume = PlayerPrefs.GetFloat("DP_MusicVolume", 0.7f);
            }

            if (PlayerPrefs.HasKey("DP_TargetFPS"))
            {
                Application.targetFrameRate = PlayerPrefs.GetInt("DP_TargetFPS", 60);
            }

            Debug.Log("[Boot] Preferencias del jugador cargadas");
        }

        /// <summary>
        /// Muestra popup de consentimiento GDPR si el usuario aún no ha aceptado.
        /// Bloquea el boot hasta recibir respuesta. Sin prefab — UI programática.
        /// </summary>
        private IEnumerator RequestGDPRConsentIfNeeded()
        {
            if (ConsentService.HasConsent())
            {
                _consentGranted = true;
                yield break;
            }

            _consentGranted = false;

            // --- Construir popup programáticamente ---
            GameObject popupRoot = new GameObject("ConsentPopup");
            DontDestroyOnLoad(popupRoot);

            Canvas canvas = popupRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            popupRoot.AddComponent<UnityEngine.UI.CanvasScaler>().uiScaleMode =
                UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            popupRoot.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Fondo oscuro semitransparente
            GameObject backdrop = new GameObject("Backdrop");
            backdrop.transform.SetParent(popupRoot.transform, false);
            UnityEngine.UI.Image backdropImg = backdrop.AddComponent<UnityEngine.UI.Image>();
            backdropImg.color = new Color(0f, 0f, 0f, 0.85f);
            RectTransform backdropRT = backdrop.GetComponent<RectTransform>();
            backdropRT.anchorMin = Vector2.zero;
            backdropRT.anchorMax = Vector2.one;
            backdropRT.offsetMin = backdropRT.offsetMax = Vector2.zero;

            // Panel central
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(popupRoot.transform, false);
            UnityEngine.UI.Image panelImg = panel.AddComponent<UnityEngine.UI.Image>();
            panelImg.color = new Color(0.08f, 0.10f, 0.16f, 1f);
            RectTransform panelRT = panel.GetComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.07f, 0.25f);
            panelRT.anchorMax = new Vector2(0.93f, 0.75f);
            panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;

            // Título
            GameObject titleGO = new GameObject("Title");
            titleGO.transform.SetParent(panel.transform, false);
            TextMeshProUGUI titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
            titleTMP.text = AutoLocalizer.Get("gdpr_title");
            titleTMP.fontSize = 22;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.color = Color.white;
            titleTMP.alignment = TextAlignmentOptions.Center;
            RectTransform titleRT = titleGO.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.05f, 0.72f);
            titleRT.anchorMax = new Vector2(0.95f, 0.95f);
            titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;

            // Cuerpo de texto
            GameObject bodyGO = new GameObject("Body");
            bodyGO.transform.SetParent(panel.transform, false);
            TextMeshProUGUI bodyTMP = bodyGO.AddComponent<TextMeshProUGUI>();
            bodyTMP.richText = true;
            bodyTMP.text = AutoLocalizer.Get("gdpr_body");
            bodyTMP.fontSize = 14;
            bodyTMP.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            bodyTMP.alignment = TextAlignmentOptions.Center;
            bodyTMP.enableWordWrapping = true;
            RectTransform bodyRT = bodyGO.GetComponent<RectTransform>();
            bodyRT.anchorMin = new Vector2(0.05f, 0.30f);
            bodyRT.anchorMax = new Vector2(0.95f, 0.72f);
            bodyRT.offsetMin = bodyRT.offsetMax = Vector2.zero;

            // Botón Accept
            bool accepted = false;
            bool responded = false;

            GameObject acceptBtn = new GameObject("AcceptButton");
            acceptBtn.transform.SetParent(panel.transform, false);
            UnityEngine.UI.Image acceptImg = acceptBtn.AddComponent<UnityEngine.UI.Image>();
            acceptImg.color = new Color(0f, 0.898f, 1f, 1f);
            UnityEngine.UI.Button acceptBtnComp = acceptBtn.AddComponent<UnityEngine.UI.Button>();
            acceptBtnComp.onClick.AddListener(() => { accepted = true; responded = true; });
            RectTransform acceptRT = acceptBtn.GetComponent<RectTransform>();
            acceptRT.anchorMin = new Vector2(0.55f, 0.05f);
            acceptRT.anchorMax = new Vector2(0.95f, 0.27f);
            acceptRT.offsetMin = acceptRT.offsetMax = Vector2.zero;

            GameObject acceptTextGO = new GameObject("Text");
            acceptTextGO.transform.SetParent(acceptBtn.transform, false);
            TextMeshProUGUI acceptTMP = acceptTextGO.AddComponent<TextMeshProUGUI>();
            acceptTMP.text = AutoLocalizer.Get("gdpr_accept");
            acceptTMP.fontSize = 14;
            acceptTMP.fontStyle = FontStyles.Bold;
            acceptTMP.color = new Color(0.06f, 0.06f, 0.12f, 1f);
            acceptTMP.alignment = TextAlignmentOptions.Center;
            RectTransform acceptTextRT = acceptTextGO.GetComponent<RectTransform>();
            acceptTextRT.anchorMin = Vector2.zero;
            acceptTextRT.anchorMax = Vector2.one;
            acceptTextRT.offsetMin = acceptTextRT.offsetMax = Vector2.zero;

            // Botón Decline
            GameObject declineBtn = new GameObject("DeclineButton");
            declineBtn.transform.SetParent(panel.transform, false);
            UnityEngine.UI.Image declineImg = declineBtn.AddComponent<UnityEngine.UI.Image>();
            declineImg.color = new Color(0.15f, 0.15f, 0.20f, 1f);
            UnityEngine.UI.Button declineBtnComp = declineBtn.AddComponent<UnityEngine.UI.Button>();
            declineBtnComp.onClick.AddListener(() => { accepted = false; responded = true; });
            RectTransform declineRT = declineBtn.GetComponent<RectTransform>();
            declineRT.anchorMin = new Vector2(0.05f, 0.05f);
            declineRT.anchorMax = new Vector2(0.45f, 0.27f);
            declineRT.offsetMin = declineRT.offsetMax = Vector2.zero;

            GameObject declineTextGO = new GameObject("Text");
            declineTextGO.transform.SetParent(declineBtn.transform, false);
            TextMeshProUGUI declineTMP = declineTextGO.AddComponent<TextMeshProUGUI>();
            declineTMP.text = AutoLocalizer.Get("gdpr_decline");
            declineTMP.fontSize = 14;
            declineTMP.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            declineTMP.alignment = TextAlignmentOptions.Center;
            RectTransform declineTextRT = declineTextGO.GetComponent<RectTransform>();
            declineTextRT.anchorMin = Vector2.zero;
            declineTextRT.anchorMax = Vector2.one;
            declineTextRT.offsetMin = declineTextRT.offsetMax = Vector2.zero;

            // Esperar respuesta del usuario
            yield return new WaitUntil(() => responded);

            if (accepted)
            {
                Destroy(popupRoot);
                ConsentService.Accept();
                _consentGranted = true;
                Debug.Log("[Boot] GDPR consent accepted.");
            }
            else
            {
                // GDPR Art. 7(4): consent must be freely given — allow decline
                // App continues without analytics; user can enable later in Settings
                Destroy(popupRoot);
                _consentGranted = false;
                Debug.Log("[Boot] GDPR consent declined — continuing without analytics.");
            }
        }

    }
}
