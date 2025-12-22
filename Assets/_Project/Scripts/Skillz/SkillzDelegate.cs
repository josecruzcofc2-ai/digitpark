using UnityEngine;
using UnityEngine.SceneManagement;
using SkillzSDK;

namespace DigitPark.Skillz
{
    /// <summary>
    /// Delegate que recibe callbacks del SDK de Skillz
    /// Implementa SkillzMatchDelegate para recibir eventos del SDK
    /// </summary>
    public class DigitParkSkillzDelegate : MonoBehaviour, SkillzMatchDelegate
    {
        private static DigitParkSkillzDelegate _instance;
        public static DigitParkSkillzDelegate Instance => _instance;

        [Header("Escenas")]
        [SerializeField] private string gameSceneName = "Game";
        [SerializeField] private string menuSceneName = "MainMenu";

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Llamado por Skillz cuando un match est por comenzar
        /// </summary>
        public void OnMatchWillBegin(Match matchInfo)
        {
            Debug.Log($"[SkillzDelegate] Match comenzando - ID: {matchInfo.ID}");

            // Convertir la info del SDK a nuestro formato
            var info = ConvertMatchInfo(matchInfo);
            DigitParkSkillzManager.Instance?.OnSkillzMatchWillBegin(info);

            // Cargar escena del juego
            LoadGameScene();
        }

        /// <summary>
        /// Llamado por Skillz cuando la UI de Skillz se cierra sin iniciar match
        /// </summary>
        public void OnSkillzWillExit()
        {
            Debug.Log("[SkillzDelegate] Skillz UI cerrada, volviendo al men");

            // Volver al men principal
            SceneManager.LoadScene(menuSceneName);
        }

        /// <summary>
        /// Llamado cuando se necesita obtener progreso del juego
        /// </summary>
        public void OnProgressionRoomEnter()
        {
            Debug.Log("[SkillzDelegate] Entrando a progression room");
            // Sincronizar datos del jugador si es necesario
            // Por ahora, volver a Skillz
            SkillzCrossPlatform.ReturnToSkillz();
        }

        /// <summary>
        /// Llamado cuando un nuevo usuario de pago entra a su primer match con dinero
        /// </summary>
        public void OnNPUConversion()
        {
            Debug.Log("[SkillzDelegate] Nuevo usuario de pago - primer match con dinero real");
            // Aqu puedes enviar analticas o eventos
        }

        /// <summary>
        /// Llamado cuando el juego recibe una advertencia de memoria
        /// </summary>
        public void OnReceivedMemoryWarning()
        {
            Debug.LogWarning("[SkillzDelegate] Advertencia de memoria recibida");
            // Liberar recursos no esenciales si es necesario
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        /// <summary>
        /// Llamado cuando el usuario entra a la pantalla de tutorial
        /// </summary>
        public void OnTutorialScreenEnter()
        {
            Debug.Log("[SkillzDelegate] Tutorial screen solicitado");
            // digitPark no tiene tutorial separado, volver a Skillz
            SkillzCrossPlatform.ReturnToSkillz();
        }

        /// <summary>
        /// Carga la escena del juego para el match de Skillz
        /// </summary>
        private void LoadGameScene()
        {
            Debug.Log($"[SkillzDelegate] Cargando escena: {gameSceneName}");
            SceneManager.LoadScene(gameSceneName);
        }

        /// <summary>
        /// Vuelve a la UI de Skillz despus de terminar el match
        /// </summary>
        public void ReturnToSkillz()
        {
            Debug.Log("[SkillzDelegate] Volviendo a Skillz UI");
            SkillzCrossPlatform.ReturnToSkillz();
        }

        /// <summary>
        /// Convierte la info del match del SDK a nuestro formato interno
        /// </summary>
        private SkillzMatchInfo ConvertMatchInfo(Match matchInfo)
        {
            return new SkillzMatchInfo
            {
                MatchId = matchInfo.ID?.ToString() ?? "unknown",
                TournamentId = matchInfo.TemplateID?.ToString() ?? "unknown",
                TournamentName = matchInfo.Name ?? "Tournament",
                EntryFee = matchInfo.EntryCash ?? 0f,
                PrizePool = (matchInfo.EntryCash ?? 0f) * 2f, // Estimado: pozo = entrada x 2
                IsCash = matchInfo.IsCash ?? false,
                MaxPlayers = matchInfo.BracketRound ?? 2,
                CurrentPlayers = matchInfo.BracketRound ?? 2
            };
        }
    }
}
