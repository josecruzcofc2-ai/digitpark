#if UNITY_EDITOR || DEVELOPMENT_BUILD
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

        [Tooltip("Activa para simular que el usuario puede crear torneos")]
        [SerializeField] private bool canCreateTournaments = false;

        [Tooltip("Activa para simular que el usuario puede crear Cash Battles")]
        [SerializeField] private bool canCreateCashBattle = false;

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
        private bool _lastCanCreateTournaments;
        private bool _lastCanCreateCashBattle;
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
            // Aplicar estado inicial si esta configurado
            if (canCreateTournaments || canCreateCashBattle || hasStylesPro)
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
            if (canCreateTournaments != _lastCanCreateTournaments || canCreateCashBattle != _lastCanCreateCashBattle || hasStylesPro != _lastHasStylesPro)
            {
                _lastCanCreateTournaments = canCreateTournaments;
                _lastCanCreateCashBattle = canCreateCashBattle;
                _lastHasStylesPro = hasStylesPro;
                ApplyPremiumState();
            }

            // Auto-aplicar si cambia allowThemeChange
            if (allowThemeChange != _lastAllowThemeChange)
            {
                _lastAllowThemeChange = allowThemeChange;
                NotifyThemeChangeToggled();
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
                canCreateTournaments = Managers.PremiumManager.Instance.CanCreateTournaments;
                canCreateCashBattle = Managers.PremiumManager.Instance.CanCreateCashBattle;
                hasStylesPro = Managers.PremiumManager.Instance.HasStylesPro;
                _lastCanCreateTournaments = canCreateTournaments;
                _lastCanCreateCashBattle = canCreateCashBattle;
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
            PlayerPrefs.SetInt("DP_Premium_CreateTournaments", canCreateTournaments ? 1 : 0);
            PlayerPrefs.SetInt("DP_Premium_CashBattleCreate", canCreateCashBattle ? 1 : 0);
            PlayerPrefs.SetInt("DP_Premium_StylesPro", hasStylesPro ? 1 : 0);
            PlayerPrefs.Save();

            // Forzar recarga del PremiumManager
            // Usamos reflection para acceder a los campos privados
            var pmType = typeof(Managers.PremiumManager);
            var tournamentsField = pmType.GetField("_canCreateTournaments", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var cashBattleField = pmType.GetField("_canCreateCashBattle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var stylesProField = pmType.GetField("_hasStylesPro", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (tournamentsField != null)
                tournamentsField.SetValue(Managers.PremiumManager.Instance, canCreateTournaments);

            if (cashBattleField != null)
                cashBattleField.SetValue(Managers.PremiumManager.Instance, canCreateCashBattle);

            if (stylesProField != null)
                stylesProField.SetValue(Managers.PremiumManager.Instance, hasStylesPro);

            // Invocar el evento de cambio
            var eventField = pmType.GetField("OnPremiumStatusChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (eventField != null)
            {
                var eventDelegate = eventField.GetValue(null) as System.Action;
                eventDelegate?.Invoke();
            }

            string status = $"CreateTournaments: {canCreateTournaments}, CashBattle: {canCreateCashBattle}, StylesPro: {hasStylesPro}";
            UnityEngine.Debug.Log($"[PremiumDebug] ✅ Estado aplicado: {status}");
        }

        /// <summary>
        /// Resetea todo el estado premium
        /// </summary>
        private void ResetPremiumState()
        {
            canCreateTournaments = false;
            canCreateCashBattle = false;
            hasStylesPro = false;
            _lastCanCreateTournaments = false;
            _lastCanCreateCashBattle = false;
            _lastHasStylesPro = false;
            ApplyPremiumState();
            UnityEngine.Debug.Log("[PremiumDebug] 🔄 Estado premium reseteado");
        }

        /// <summary>
        /// Notifica que allowThemeChange cambió, refrescando el dropdown de temas
        /// </summary>
        private void NotifyThemeChangeToggled()
        {
            // Disparar el evento de PremiumManager para que el ThemeDropdownController se refresque
            var pmType = typeof(Managers.PremiumManager);
            var eventField = pmType.GetField("OnPremiumStatusChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (eventField != null)
            {
                var eventDelegate = eventField.GetValue(null) as System.Action;
                eventDelegate?.Invoke();
            }
            UnityEngine.Debug.Log($"[PremiumDebug] AllowThemeChange = {allowThemeChange}");
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            // Guardar valores en PlayerPrefs cuando se cambian en el Inspector (antes de Play)
            // Esto permite que los valores persistan incluso si se configuran antes de ejecutar
            PlayerPrefs.SetInt("DP_Premium_CreateTournaments", canCreateTournaments ? 1 : 0);
            PlayerPrefs.SetInt("DP_Premium_CashBattleCreate", canCreateCashBattle ? 1 : 0);
            PlayerPrefs.SetInt("DP_Premium_StylesPro", hasStylesPro ? 1 : 0);
            PlayerPrefs.Save();

            UnityEngine.Debug.Log($"[PremiumDebug] Inspector values saved: Tournaments={canCreateTournaments}, CashBattle={canCreateCashBattle}, StylesPro={hasStylesPro}");
        }
        #endif
    }
}
#endif
