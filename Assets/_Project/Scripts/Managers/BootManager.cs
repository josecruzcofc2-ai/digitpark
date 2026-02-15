using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using DigitPark.Services;
using DigitPark.Services.Firebase;
using DigitPark.Localization;
using DigitPark.UI;

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

        private float loadingProgress = 0f;
        private bool servicesInitialized = false;
        private DigitPark.UI.BootAnimator bootAnimator;

        private void Start()
        {
            Debug.Log("[Boot] Iniciando BootManager...");

            // Buscar BootAnimator en la escena
            bootAnimator = FindObjectOfType<DigitPark.UI.BootAnimator>();

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

            // Paso 2: Solicitar ATT (iOS) - ANTES de Firebase Analytics
            yield return StartCoroutine(RequestTrackingAuthorization());
            UpdateLoadingProgress(0.25f, "boot_initializing_config");

            // Paso 3: Inicializar servicios de Firebase
            yield return StartCoroutine(InitializeFirebaseServices());
            UpdateLoadingProgress(0.5f, "boot_connecting_services");

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

            // Evitar que la pantalla se apague
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

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

            if (AnalyticsService.Instance == null)
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

            // Esperar un frame para que los servicios se inicialicen
            yield return new WaitForSeconds(0.5f);

            servicesInitialized = true;
            Debug.Log("[Boot] Todos los servicios inicializados");
        }

        /// <summary>
        /// Inicializa los managers principales del juego
        /// </summary>
        private IEnumerator InitializeGameManagers()
        {
            Debug.Log("[Boot] Inicializando managers del juego...");

            // Estos managers se crearán en sus respectivas escenas
            // Aquí solo preparamos el entorno

            // Inicializar el pool de objetos para optimización
            InitializeObjectPools();

            // Precargar recursos críticos
            yield return StartCoroutine(PreloadCriticalResources());

            Debug.Log("[Boot] Managers del juego inicializados");
        }

        /// <summary>
        /// Inicializa object pools para optimización
        /// </summary>
        private void InitializeObjectPools()
        {
            // Aquí se inicializarían los pools para tiles, partículas, etc.
            Debug.Log("[Boot] Object pools inicializados");
        }

        /// <summary>
        /// Precarga recursos críticos
        /// </summary>
        private IEnumerator PreloadCriticalResources()
        {
            // Precargar sprites, sonidos, etc.
            Debug.Log("[Boot] Precargando recursos críticos...");

            // Simular carga de recursos
            yield return new WaitForSeconds(0.3f);

            Debug.Log("[Boot] Recursos precargados");
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

            // Cargar escena de destino
            SceneManager.LoadScene(targetScene);
        }

        /// <summary>
        /// Actualiza la barra de progreso y el texto
        /// </summary>
        private void UpdateLoadingProgress(float progress, string localizationKey)
        {
            loadingProgress = progress;

            if (loadingBar != null)
            {
                loadingBar.fillAmount = progress;
            }

            // Obtener texto localizado
            string displayText = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.GetText(localizationKey)
                : localizationKey;

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
            if (PlayerPrefs.HasKey("MusicVolume"))
            {
                AudioListener.volume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            }

            if (PlayerPrefs.HasKey("TargetFPS"))
            {
                Application.targetFrameRate = PlayerPrefs.GetInt("TargetFPS", 60);
            }

            Debug.Log("[Boot] Preferencias del jugador cargadas");
        }

        #region Error Handling

        /// <summary>
        /// Maneja errores durante el boot
        /// </summary>
        private void HandleBootError(string error)
        {
            Debug.LogError($"[Boot] Error durante inicialización: {error}");

            // Mostrar mensaje de error al usuario
            if (loadingText != null)
            {
                string errorMessage = LocalizationManager.Instance != null
                    ? LocalizationManager.Instance.GetText("boot_error")
                    : "Error initializing. Please restart.";
                loadingText.text = errorMessage;
                loadingText.color = Color.red;
            }

            // En producción, podrías intentar reiniciar o mostrar un diálogo
        }

        #endregion
    }
}
