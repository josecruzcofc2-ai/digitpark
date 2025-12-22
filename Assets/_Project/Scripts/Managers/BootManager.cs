using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using DigitPark.Services.Firebase;
using DigitPark.Localization;
using DigitPark.UI;
using DigitPark.Skillz;

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

        private void Start()
        {
            Debug.Log("[Boot] Iniciando BootManager...");

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

            // Paso 1: Inicializar configuraciones básicas
            yield return StartCoroutine(InitializeBasicSettings());
            UpdateLoadingProgress(0.2f, "boot_initializing_config");

            // Paso 2: Inicializar servicios de Firebase
            yield return StartCoroutine(InitializeFirebaseServices());
            UpdateLoadingProgress(0.5f, "boot_connecting_services");

            // Paso 3: Inicializar managers del juego
            yield return StartCoroutine(InitializeGameManagers());
            UpdateLoadingProgress(0.7f, "boot_loading_resources");

            // Paso 4: Verificar estado de autenticación
            yield return StartCoroutine(CheckAuthenticationStatus());
            UpdateLoadingProgress(0.9f, "boot_verifying_user");

            // Asegurar tiempo mínimo de carga (para mostrar logo/branding)
            float elapsedTime = Time.time - startTime;
            if (elapsedTime < minimumLoadTime)
            {
                yield return new WaitForSeconds(minimumLoadTime - elapsedTime);
            }

            UpdateLoadingProgress(1f, "boot_completed");

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

            // Cargar configuraciones guardadas del jugador
            LoadPlayerPreferences();

            yield return null;
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

            // ========== SKILLZ INITIALIZATION ==========
            // Inicializar Skillz para torneos con dinero real
            // Estos managers son persistentes (DontDestroyOnLoad)
            InitializeSkillz();
            // ============================================

            // Precargar recursos críticos
            yield return StartCoroutine(PreloadCriticalResources());

            Debug.Log("[Boot] Managers del juego inicializados");
        }

        /// <summary>
        /// Inicializa los managers de Skillz para torneos con dinero real
        /// Estos managers persisten entre escenas (DontDestroyOnLoad)
        /// </summary>
        private void InitializeSkillz()
        {
            Debug.Log("[Boot] Inicializando Skillz...");

            // Crear DigitParkSkillzManager si no existe
            if (DigitParkSkillzManager.Instance == null)
            {
                GameObject skillzManagerObj = new GameObject("DigitParkSkillzManager");
                skillzManagerObj.AddComponent<DigitParkSkillzManager>();
                Debug.Log("[Boot] DigitParkSkillzManager creado");
            }

            // Crear DigitParkSkillzDelegate si no existe
            if (DigitParkSkillzDelegate.Instance == null)
            {
                GameObject skillzDelegateObj = new GameObject("DigitParkSkillzDelegate");
                skillzDelegateObj.AddComponent<DigitParkSkillzDelegate>();
                Debug.Log("[Boot] DigitParkSkillzDelegate creado");
            }

            Debug.Log("[Boot] Skillz inicializado correctamente");
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
            bool isAuthenticated = AuthenticationService.Instance.IsUserAuthenticated();

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

            if (loadingText != null)
            {
                // Usar localización si está disponible, sino usar la key como fallback
                string displayText = LocalizationManager.Instance != null
                    ? LocalizationManager.Instance.GetText(localizationKey)
                    : localizationKey;
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
