using UnityEngine;
using DigitPark.Managers;

namespace DigitPark.Skillz
{
    /// <summary>
    /// Controlador que integra Skillz con el GameManager de digitPark
    ///
    /// Se encarga de:
    /// - Detectar si estamos en un match de Skillz
    /// - Usar números aleatorios de Skillz (para fairness)
    /// - Reportar el score al finalizar
    /// </summary>
    public class SkillzGameController : MonoBehaviour
    {
        private static SkillzGameController _instance;
        public static SkillzGameController Instance => _instance;

        [Header("Referencias")]
        [SerializeField] private GameManager gameManager;

        [Header("Configuración")]
        [Tooltip("Si es true, usa Skillz Random incluso fuera de torneos (para testing)")]
        [SerializeField] private bool alwaysUseSkillzRandom = false;

        // Estado
        private bool isSkillzMatch = false;
        private float matchStartTime;
        private float currentScore;

        // Propiedades
        public bool IsSkillzMatch => isSkillzMatch;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Verificar si estamos en un match de Skillz
            if (DigitParkSkillzManager.Instance != null && DigitParkSkillzManager.Instance.IsInTournament)
            {
                isSkillzMatch = true;
                matchStartTime = Time.time;
                Debug.Log("[SkillzGame] Iniciando en modo torneo Skillz");

                // Configurar el juego para el match
                SetupSkillzMatch();
            }
            else
            {
                Debug.Log("[SkillzGame] Modo normal (no Skillz)");
            }

            // Buscar GameManager si no está asignado
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
            }
        }

        /// <summary>
        /// Configura el juego para un match de Skillz
        /// </summary>
        private void SetupSkillzMatch()
        {
            var matchInfo = DigitParkSkillzManager.Instance.CurrentMatch;
            if (matchInfo != null)
            {
                Debug.Log($"[SkillzGame] Match: {matchInfo.TournamentName}");
                Debug.Log($"[SkillzGame] Entrada: ${matchInfo.EntryFee}, Pozo: ${matchInfo.PrizePool}");
                Debug.Log($"[SkillzGame] Es dinero real: {matchInfo.IsCash}");
            }
        }

        /// <summary>
        /// Obtiene un número aleatorio (usa Skillz si estamos en torneo)
        /// </summary>
        public float GetRandom()
        {
            if (isSkillzMatch || alwaysUseSkillzRandom)
            {
                return DigitParkSkillzManager.Instance?.GetSkillzRandom() ?? Random.value;
            }
            return Random.value;
        }

        /// <summary>
        /// Obtiene un número aleatorio en rango (usa Skillz si estamos en torneo)
        /// </summary>
        public int GetRandomRange(int min, int max)
        {
            if (isSkillzMatch || alwaysUseSkillzRandom)
            {
                return DigitParkSkillzManager.Instance?.GetSkillzRandomRange(min, max) ?? Random.Range(min, max);
            }
            return Random.Range(min, max);
        }

        /// <summary>
        /// Actualiza el score actual durante el juego
        /// Llamar desde GameManager cuando el jugador progresa
        /// </summary>
        public void UpdateScore(float score)
        {
            currentScore = score;

            // SKILLZ_SDK: Opcional - reportar score en tiempo real
            // SkillzCrossPlatform.UpdatePlayersCurrentScore(score);
        }

        /// <summary>
        /// Finaliza el match y reporta el score a Skillz
        /// En digitPark, el score es el tiempo (menor es mejor)
        /// </summary>
        /// <param name="finalTime">Tiempo final en segundos</param>
        /// <param name="completed">Si el jugador completó el juego o abandonó</param>
        public void EndMatch(float finalTime, bool completed = true)
        {
            if (!isSkillzMatch)
            {
                Debug.Log("[SkillzGame] No estamos en match de Skillz, ignorando EndMatch");
                return;
            }

            Debug.Log($"[SkillzGame] Finalizando match - Tiempo: {finalTime}s, Completado: {completed}");

            if (completed)
            {
                // En digitPark, menor tiempo = mejor score
                // Skillz espera que mayor score = mejor
                // Invertimos: score = 1000000 - (tiempo en ms)
                // Así un tiempo de 5.234s (5234ms) da score de 994766
                float skillzScore = 1000000f - (finalTime * 1000f);
                skillzScore = Mathf.Max(0, skillzScore); // No permitir negativos

                Debug.Log($"[SkillzGame] Score para Skillz: {skillzScore}");
                DigitParkSkillzManager.Instance?.ReportScore(skillzScore);
            }
            else
            {
                // El jugador abandonó
                Debug.Log("[SkillzGame] Match abortado por el jugador");
                DigitParkSkillzManager.Instance?.AbortMatch("Player quit");
            }

            isSkillzMatch = false;
        }

        /// <summary>
        /// Aborta el match actual (si el jugador quiere salir)
        /// </summary>
        public void AbortMatch()
        {
            if (!isSkillzMatch)
            {
                return;
            }

            Debug.Log("[SkillzGame] Abortando match...");
            DigitParkSkillzManager.Instance?.AbortMatch("Player requested abort");
            isSkillzMatch = false;
        }

        /// <summary>
        /// Verifica si debemos bloquear la salida del juego (estamos en torneo)
        /// </summary>
        public bool ShouldBlockExit()
        {
            if (!isSkillzMatch)
            {
                return false;
            }

            // En un torneo de Skillz, advertir antes de salir
            return true;
        }

        /// <summary>
        /// Muestra advertencia antes de salir de un match de Skillz
        /// </summary>
        public void ShowExitWarning(System.Action onConfirmExit, System.Action onCancel)
        {
            // Aquí podrías mostrar un popup de confirmación
            // Por ahora, solo loguear
            Debug.LogWarning("[SkillzGame] El jugador intenta salir de un match de Skillz");

            // Mostrar UI de confirmación (implementar según tu sistema de UI)
            // Por ahora, llamar directamente a confirm
            // onConfirmExit?.Invoke();
        }
    }
}
