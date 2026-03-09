using UnityEngine;
using UnityEngine.SceneManagement;

namespace DigitPark.Tools
{
    /// <summary>
    /// Configuracion de testing para la escena Boot.
    /// Permite saltar el login y ir directo a una escena en modo Editor.
    /// Agregar al GameObject "EditorBootConfig" en la escena Boot.
    /// </summary>
    public class EditorBootConfig : MonoBehaviour
    {
#if UNITY_EDITOR
        [Header("=== EDITOR TESTING ===")]
        [Tooltip("Activar para saltar el flujo normal de Boot")]
        [SerializeField] private bool skipBootFlow = false;

        [Tooltip("Escena destino cuando skipBootFlow esta activo")]
        [SerializeField] private TargetScene targetScene = TargetScene.MainMenu;

        [Tooltip("Simular usuario autenticado (para escenas que lo requieren)")]
        [SerializeField] private bool simulateAuthenticated = true;

        [Tooltip("Activar bypass de auth en CashBattle (ServiceLocator Mock)")]
        [SerializeField] private bool cashBattleBypassAuth = true;

        [Header("=== INFO ===")]
        #pragma warning disable 0414
        [SerializeField] [TextArea(2, 4)]
        private string info = "Este objeto solo funciona en el Editor.\nSe ignora automaticamente en builds.";
        #pragma warning restore 0414

        public enum TargetScene
        {
            MainMenu,
            GameSelector,
            CashBattleHub,
            CashBattle1v1,
            Settings,
            Profile,
            Shop,
            Achievements,
            DailyMissions,
            DailyRewards,
            Friends,
            Scores,
            TournamentsBrowser
        }

        private void Awake()
        {
            if (!skipBootFlow) return;

            Debug.Log($"[EditorBootConfig] Skip Boot activado -> {targetScene}");

            if (cashBattleBypassAuth)
            {
                PlayerPrefs.SetInt("CashBattleBypassAuth", 1);
                PlayerPrefs.SetInt("Mock_KYC_Status", 3); // KYCStatus.FullyVerified
                PlayerPrefs.SetInt("AgeVerified", 1);     // Legacy fallback
                PlayerPrefs.Save();
                Debug.Log("[EditorBootConfig] CashBattle bypass auth ENABLED (KYC set to FullyVerified)");
            }

            if (simulateAuthenticated)
            {
                PlayerPrefs.SetInt("EditorSimulateAuth", 1);
                PlayerPrefs.Save();
                Debug.Log("[EditorBootConfig] Simulating authenticated user");
            }

            // Cargar escena destino en el siguiente frame
            StartCoroutine(LoadTargetScene());
        }

        private System.Collections.IEnumerator LoadTargetScene()
        {
            // Esperar un frame para que otros Awake() terminen
            yield return null;

            string sceneName = targetScene.ToString();
            Debug.Log($"[EditorBootConfig] Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
#endif
    }
}
