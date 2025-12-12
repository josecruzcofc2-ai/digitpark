using UnityEngine;
using UnityEngine.SceneManagement;

namespace DigitPark.Skillz
{
    /// <summary>
    /// Delegate que recibe callbacks del SDK de Skillz
    ///
    /// IMPORTANTE: Esta clase debe implementar SkillzMatchDelegate cuando el SDK esté instalado
    /// Descomentar la herencia: public class SkillzDelegate : SkillzMatchDelegate
    /// </summary>
    public class SkillzDelegate : MonoBehaviour
    // SKILLZ_SDK: Descomentar cuando el SDK esté instalado
    // public class SkillzDelegate : MonoBehaviour, SkillzMatchDelegate
    {
        private static SkillzDelegate _instance;
        public static SkillzDelegate Instance => _instance;

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
        /// Llamado por Skillz cuando un match está a punto de comenzar
        /// </summary>
        public void OnMatchWillBegin(/*SkillzMatchInfo matchInfo*/)
        {
            Debug.Log("[SkillzDelegate] Match a punto de comenzar");

            // SKILLZ_SDK: Cuando el SDK esté instalado, descomentar:
            // var info = ConvertMatchInfo(matchInfo);
            // SkillzManager.Instance.OnSkillzMatchWillBegin(info);

            // Simular inicio de match
            var simulatedInfo = new SkillzMatchInfo
            {
                MatchId = "simulated_match_" + System.DateTime.Now.Ticks,
                TournamentName = "Torneo de Prueba",
                EntryFee = 1.00f,
                PrizePool = 10.00f,
                IsCash = false, // Sandbox siempre es moneda Z
                MaxPlayers = 10,
                CurrentPlayers = 2
            };

            SkillzManager.Instance?.OnSkillzMatchWillBegin(simulatedInfo);

            // Cargar escena del juego
            LoadGameScene();
        }

        /// <summary>
        /// Llamado por Skillz cuando la UI de Skillz se cierra sin iniciar match
        /// </summary>
        public void OnSkillzWillExit()
        {
            Debug.Log("[SkillzDelegate] Skillz UI cerrada, volviendo al menú");

            // Volver al menú principal
            SceneManager.LoadScene(menuSceneName);
        }

        /// <summary>
        /// Llamado cuando se necesita obtener progreso del juego (para sync)
        /// </summary>
        public void OnProgressionRoomEnter()
        {
            Debug.Log("[SkillzDelegate] Entrando a progression room");
            // Aquí puedes sincronizar datos del jugador si es necesario
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
        /// Vuelve a la UI de Skillz después de terminar el match
        /// </summary>
        public void ReturnToSkillz()
        {
            Debug.Log("[SkillzDelegate] Volviendo a Skillz UI");

            // SKILLZ_SDK: Descomentar cuando el SDK esté instalado
            // SkillzCrossPlatform.ReturnToSkillz();

            // Por ahora, volver al menú
            SceneManager.LoadScene(menuSceneName);
        }

        // SKILLZ_SDK: Métodos adicionales del delegate (descomentar cuando el SDK esté instalado)
        /*
        private SkillzMatchInfo ConvertMatchInfo(Skillz.MatchInfo matchInfo)
        {
            return new SkillzMatchInfo
            {
                MatchId = matchInfo.Id.ToString(),
                TournamentId = matchInfo.TournamentId.ToString(),
                TournamentName = matchInfo.Name,
                EntryFee = (float)matchInfo.EntryCash,
                PrizePool = (float)matchInfo.Prize,
                IsCash = matchInfo.IsCash,
                MaxPlayers = matchInfo.TournamentPlayerCount,
                CurrentPlayers = matchInfo.TournamentPlayerCount
            };
        }
        */
    }
}
