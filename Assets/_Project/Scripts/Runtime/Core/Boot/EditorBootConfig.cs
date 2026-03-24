using UnityEngine;
using UnityEngine.SceneManagement;

namespace DigitPark.Tools
{
    /// <summary>
    /// Configuracion de testing para la escena Boot.
    /// Permite saltar el login y ir directo a una escena en modo Editor.
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

        [Header("=== INFO ===")]
        #pragma warning disable 0414
        [SerializeField] [TextArea(2, 4)]
        private string info = "Este objeto solo funciona en el Editor.\nSe ignora automaticamente en builds.";
        #pragma warning restore 0414

        public enum TargetScene
        {
            MainMenu,
            GameSelector,
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

            if (simulateAuthenticated)
            {
                PlayerPrefs.SetInt("EditorSimulateAuth", 1);
                PlayerPrefs.Save();
                Debug.Log("[EditorBootConfig] Simulating authenticated user");
            }

            StartCoroutine(LoadTargetScene());
        }

        private System.Collections.IEnumerator LoadTargetScene()
        {
            yield return null;
            string sceneName = targetScene.ToString();
            Debug.Log($"[EditorBootConfig] Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
#endif
    }
}
