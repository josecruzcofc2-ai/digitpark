using UnityEngine;

namespace DigitPark.Tools
{
    /// <summary>
    /// Controlador de debug para el estado Premium.
    /// Permite activar/desactivar premium desde el Inspector en modo Editor.
    ///
    /// USO: Coloca este script en un GameObject en la escena Boot.
    /// En el Inspector, marca/desmarca las opciones para simular estados premium.
    /// </summary>
    public class PremiumDebugController : MonoBehaviour
    {
        // Instancia estática para acceso fácil
        public static PremiumDebugController Instance { get; private set; }

        [Header("=== PREMIUM DEBUG CONTROLLER ===")]
        [Space(10)]

        [Tooltip("Activa para simular que el usuario NO tiene anuncios")]
        [SerializeField] private bool hasNoAds = false;

        [Tooltip("Activa para simular que el usuario puede crear torneos")]
        [SerializeField] private bool canCreateTournaments = false;

        [Tooltip("Activa para simular que el usuario tiene Estilos PRO (temas premium)")]
        [SerializeField] private bool hasStylesPro = false;

        [Space(20)]
        [Header("=== DESBLOQUEAR CAMBIO DE TEMAS ===")]

        [Tooltip("✓ MARCADO = Cambias temas sin restricción\n☐ DESMARCADO = Muestra panel 'necesitas premium' al cambiar tema")]
        [SerializeField] private bool allowThemeChange = false;

        [Space(20)]
        [Header("=== ACCIONES RÁPIDAS ===")]

        [Tooltip("Marca esto para aplicar los cambios inmediatamente")]
        [SerializeField] private bool applyChanges = false;

        [Tooltip("Marca esto para resetear todo el estado premium")]
        [SerializeField] private bool resetPremium = false;

        // Para detectar cambios en el inspector
        private bool _lastHasNoAds;
        private bool _lastCanCreateTournaments;
        private bool _lastHasStylesPro;
        private bool _lastAllowThemeChange;

        /// <summary>
        /// Propiedad pública para saber si se permite cambiar temas sin restricción
        /// </summary>
        public bool AllowThemeChange => allowThemeChange;

        private void Awake()
        {
            // Singleton
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadCurrentState();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Aplicar estado inicial si está configurado
            if (hasNoAds || canCreateTournaments || hasStylesPro)
            {
                ApplyPremiumState();
            }
        }

        private void Update()
        {
            #if UNITY_EDITOR
            // Detectar cambios en el inspector
            if (applyChanges)
            {
                applyChanges = false;
                ApplyPremiumState();
            }

            if (resetPremium)
            {
                resetPremium = false;
                ResetPremiumState();
            }

            // Auto-aplicar si cambian los valores
            if (hasNoAds != _lastHasNoAds || canCreateTournaments != _lastCanCreateTournaments || hasStylesPro != _lastHasStylesPro)
            {
                _lastHasNoAds = hasNoAds;
                _lastCanCreateTournaments = canCreateTournaments;
                _lastHasStylesPro = hasStylesPro;
                ApplyPremiumState();
            }
            #endif
        }

        /// <summary>
        /// Carga el estado actual del PremiumManager
        /// </summary>
        private void LoadCurrentState()
        {
            if (Managers.PremiumManager.Instance != null)
            {
                hasNoAds = Managers.PremiumManager.Instance.HasNoAds;
                canCreateTournaments = Managers.PremiumManager.Instance.CanCreateTournaments;
                hasStylesPro = Managers.PremiumManager.Instance.HasStylesPro;
                _lastHasNoAds = hasNoAds;
                _lastCanCreateTournaments = canCreateTournaments;
                _lastHasStylesPro = hasStylesPro;
            }
        }

        /// <summary>
        /// Aplica el estado premium configurado
        /// </summary>
        private void ApplyPremiumState()
        {
            if (Managers.PremiumManager.Instance == null)
            {
                UnityEngine.Debug.LogWarning("[PremiumDebug] PremiumManager no encontrado");
                return;
            }

            // Guardar en PlayerPrefs directamente
            PlayerPrefs.SetInt("Premium_NoAds", hasNoAds ? 1 : 0);
            PlayerPrefs.SetInt("Premium_CreateTournaments", canCreateTournaments ? 1 : 0);
            PlayerPrefs.SetInt("Premium_StylesPro", hasStylesPro ? 1 : 0);
            PlayerPrefs.Save();

            // Forzar recarga del PremiumManager
            // Usamos reflection para acceder a los campos privados
            var pmType = typeof(Managers.PremiumManager);
            var noAdsField = pmType.GetField("_hasNoAds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var tournamentsField = pmType.GetField("_canCreateTournaments", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var stylesProField = pmType.GetField("_hasStylesPro", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (noAdsField != null)
                noAdsField.SetValue(Managers.PremiumManager.Instance, hasNoAds);

            if (tournamentsField != null)
                tournamentsField.SetValue(Managers.PremiumManager.Instance, canCreateTournaments);

            if (stylesProField != null)
                stylesProField.SetValue(Managers.PremiumManager.Instance, hasStylesPro);

            // Invocar el evento de cambio
            var eventField = pmType.GetField("OnPremiumStatusChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (eventField != null)
            {
                var eventDelegate = eventField.GetValue(null) as System.Action;
                eventDelegate?.Invoke();
            }

            string status = $"NoAds: {hasNoAds}, CreateTournaments: {canCreateTournaments}, StylesPro: {hasStylesPro}";
            UnityEngine.Debug.Log($"[PremiumDebug] ✅ Estado aplicado: {status}");
        }

        /// <summary>
        /// Resetea todo el estado premium
        /// </summary>
        private void ResetPremiumState()
        {
            hasNoAds = false;
            canCreateTournaments = false;
            hasStylesPro = false;
            _lastHasNoAds = false;
            _lastCanCreateTournaments = false;
            _lastHasStylesPro = false;
            ApplyPremiumState();
            UnityEngine.Debug.Log("[PremiumDebug] 🔄 Estado premium reseteado");
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            // Guardar valores en PlayerPrefs cuando se cambian en el Inspector (antes de Play)
            // Esto permite que los valores persistan incluso si se configuran antes de ejecutar
            PlayerPrefs.SetInt("Premium_NoAds", hasNoAds ? 1 : 0);
            PlayerPrefs.SetInt("Premium_CreateTournaments", canCreateTournaments ? 1 : 0);
            PlayerPrefs.SetInt("Premium_StylesPro", hasStylesPro ? 1 : 0);
            PlayerPrefs.Save();

            UnityEngine.Debug.Log($"[PremiumDebug] Inspector values saved: NoAds={hasNoAds}, Tournaments={canCreateTournaments}, StylesPro={hasStylesPro}");
        }
        #endif
    }
}
