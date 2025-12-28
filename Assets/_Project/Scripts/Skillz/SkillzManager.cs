using UnityEngine;
using System;
using SkillzSDK;

namespace DigitPark.Skillz
{
    /// <summary>
    /// Manager principal para integracin con Skillz SDK
    /// Maneja la inicializacin, torneos y comunicacin con Skillz
    ///
    /// Ahora usa el SDK real de Skillz
    /// </summary>
    public class DigitParkSkillzManager : MonoBehaviour
    {
        private static DigitParkSkillzManager _instance;
        public static DigitParkSkillzManager Instance => _instance;

        [Header("Configuracin Skillz")]
        [SerializeField] private string gameId = "92406"; // DigitPark Game ID from Skillz Dashboard

        [Header("Configuracin de Premios")]
        [SerializeField] private PrizeDistributionConfig prizeConfig;

        // Estado
        private bool isSkillzInitialized = false;
        private bool isInSkillzTournament = false;
        private SkillzMatchInfo currentMatch;

        // Eventos
        public event Action OnSkillzInitialized;
        public event Action<SkillzMatchInfo> OnMatchStarted;
        public event Action OnMatchCompleted;
        public event Action<string> OnMatchAborted;

        public bool IsInitialized => isSkillzInitialized;
        public bool IsInTournament => isInSkillzTournament;
        public SkillzMatchInfo CurrentMatch => currentMatch;
        public PrizeDistributionConfig PrizeConfig => prizeConfig;
        public string GameId => gameId;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);

                // Crear config de premios por defecto si no existe
                if (prizeConfig == null)
                {
                    prizeConfig = ScriptableObject.CreateInstance<PrizeDistributionConfig>();
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            InitializeSkillz();
        }

        /// <summary>
        /// Inicializa el SDK de Skillz
        /// El SDK se inicializa automticamente a travs del SkillzManager prefab del SDK
        /// </summary>
        public void InitializeSkillz()
        {
            if (isSkillzInitialized)
            {
                Debug.LogWarning("[Skillz] Ya est inicializado");
                return;
            }

            Debug.Log($"[Skillz] Inicializando con Game ID: {gameId}");

            // El SDK de Skillz se inicializa automticamente a travs del prefab SkillzManager
            // que debe estar en la escena. Aqu solo marcamos que estamos listos.
            isSkillzInitialized = true;
            Debug.Log("[Skillz] Inicializado correctamente");

            OnSkillzInitialized?.Invoke();
        }

        /// <summary>
        /// Lanza la UI de Skillz (lista de torneos)
        /// </summary>
        public void LaunchSkillz()
        {
            if (!isSkillzInitialized)
            {
                Debug.LogError("[Skillz] No est inicializado");
                return;
            }

            Debug.Log("[Skillz] Lanzando UI de Skillz...");

            // Usar el SDK real
            SkillzCrossPlatform.LaunchSkillz();
        }

        /// <summary>
        /// Llamado por DigitParkSkillzDelegate cuando un match comienza
        /// </summary>
        public void OnSkillzMatchWillBegin(SkillzMatchInfo matchInfo)
        {
            Debug.Log($"[Skillz] Match comenzando - ID: {matchInfo.MatchId}");

            currentMatch = matchInfo;
            isInSkillzTournament = true;

            OnMatchStarted?.Invoke(matchInfo);
        }

        /// <summary>
        /// Reporta el score final al terminar una partida
        /// </summary>
        /// <param name="score">Score del jugador (en digitPark = tiempo invertido, menor es mejor)</param>
        public void ReportScore(float score)
        {
            if (!isInSkillzTournament)
            {
                Debug.LogWarning("[Skillz] No est en un torneo de Skillz");
                return;
            }

            Debug.Log($"[Skillz] Reportando score: {score}");

            // Usar el SDK real para reportar el score
            SkillzCrossPlatform.DisplayTournamentResultsWithScore(score);

            isInSkillzTournament = false;
            currentMatch = null;

            OnMatchCompleted?.Invoke();

            Debug.Log("[Skillz] Score reportado correctamente");
        }

        /// <summary>
        /// Aborta el match actual (si el jugador sale o hay error)
        /// </summary>
        public void AbortMatch(string reason = "User aborted")
        {
            if (!isInSkillzTournament)
            {
                Debug.LogWarning("[Skillz] No hay match activo para abortar");
                return;
            }

            Debug.Log($"[Skillz] Abortando match: {reason}");

            // Usar el SDK real
            SkillzCrossPlatform.AbortMatch();

            isInSkillzTournament = false;
            currentMatch = null;

            OnMatchAborted?.Invoke(reason);
        }

        /// <summary>
        /// Obtiene un nmero aleatorio de Skillz (para fairness)
        /// </summary>
        public float GetSkillzRandom()
        {
            // Usar el Random de Skillz para garantizar fairness
            return SkillzCrossPlatform.Random.Value();
        }

        /// <summary>
        /// Obtiene un nmero aleatorio entero de Skillz
        /// </summary>
        public int GetSkillzRandomRange(int min, int max)
        {
            // Usar el valor del Random de Skillz y convertirlo a rango
            float random = SkillzCrossPlatform.Random.Value();
            return Mathf.FloorToInt(random * (max - min)) + min;
        }

        /// <summary>
        /// Verifica si estamos actualmente en un match de Skillz
        /// </summary>
        public bool IsSkillzPlayer()
        {
            return SkillzCrossPlatform.IsMatchInProgress();
        }

        /// <summary>
        /// Obtiene informacin del jugador actual de Skillz
        /// </summary>
        public SkillzPlayerInfo GetCurrentPlayer()
        {
            // Obtener info del match actual del SDK
            var match = SkillzCrossPlatform.GetMatchInfo();
            if (match != null && match.Players != null && match.Players.Count > 0)
            {
                var player = match.Players[0] as SkillzSDK.Player;
                if (player != null)
                {
                    return new SkillzPlayerInfo
                    {
                        PlayerId = player.ID?.ToString() ?? "unknown",
                        DisplayName = player.DisplayName ?? "Player",
                        AvatarUrl = player.AvatarURL ?? ""
                    };
                }
            }

            // Retornar datos por defecto si no hay info
            return new SkillzPlayerInfo
            {
                PlayerId = "unknown",
                DisplayName = "Player",
                AvatarUrl = ""
            };
        }

        /// <summary>
        /// Calcula la distribucin de premios para un torneo
        /// </summary>
        public PrizeBreakdown CalculatePrizeBreakdown(float totalPot, int numberOfWinners = 3)
        {
            if (prizeConfig == null)
            {
                Debug.LogError("[Skillz] PrizeConfig no est configurado");
                return null;
            }

            return prizeConfig.CalculateBreakdown(totalPot, numberOfWinners);
        }
    }

    /// <summary>
    /// Entorno de Skillz (ya no se usa, el SDK lo maneja internamente)
    /// </summary>
    public enum SkillzEnvironment
    {
        Sandbox,    // Para desarrollo y pruebas
        Production  // Para produccin (dinero real)
    }

    /// <summary>
    /// Orientacin del juego (ya no se usa, el SDK lo maneja internamente)
    /// </summary>
    public enum SkillzOrientation
    {
        Portrait,
        Landscape
    }

    /// <summary>
    /// Informacin del match de Skillz
    /// </summary>
    [Serializable]
    public class SkillzMatchInfo
    {
        public string MatchId;
        public string TournamentId;
        public string TournamentName;
        public float EntryFee;
        public float PrizePool;
        public bool IsCash;  // true = dinero real, false = moneda Z
        public int MaxPlayers;
        public int CurrentPlayers;
        public SkillzPlayerInfo[] Players;
    }

    /// <summary>
    /// Informacin del jugador de Skillz
    /// </summary>
    [Serializable]
    public class SkillzPlayerInfo
    {
        public string PlayerId;
        public string DisplayName;
        public string AvatarUrl;
        public float CashBalance;
        public float ZBalance;  // Moneda virtual de Skillz
    }
}
