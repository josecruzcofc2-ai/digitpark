using UnityEngine;
using System;

namespace DigitPark.Skillz
{
    /// <summary>
    /// Manager principal para integración con Skillz SDK
    /// Maneja la inicialización, torneos y comunicación con Skillz
    ///
    /// IMPORTANTE: Requiere Skillz SDK instalado desde Unity Asset Store
    /// Una vez instalado, descomentar las líneas marcadas con // SKILLZ_SDK
    /// </summary>
    public class SkillzManager : MonoBehaviour
    {
        private static SkillzManager _instance;
        public static SkillzManager Instance => _instance;

        [Header("Configuración Skillz")]
        [SerializeField] private string gameId = "TU_GAME_ID_AQUI"; // Obtener de Skillz Developer Console
        [SerializeField] private SkillzEnvironment environment = SkillzEnvironment.Sandbox;
        [SerializeField] private SkillzOrientation orientation = SkillzOrientation.Portrait;

        [Header("Configuración de Premios")]
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
        /// </summary>
        public void InitializeSkillz()
        {
            if (isSkillzInitialized)
            {
                Debug.LogWarning("[Skillz] Ya está inicializado");
                return;
            }

            Debug.Log($"[Skillz] Inicializando con Game ID: {gameId}");

            // SKILLZ_SDK: Descomentar cuando el SDK esté instalado
            // SkillzCrossPlatform.Initialize(gameId, environment, orientation);

            // Por ahora, simular inicialización
            isSkillzInitialized = true;
            Debug.Log("[Skillz] Inicializado (modo simulación - instalar SDK real)");

            OnSkillzInitialized?.Invoke();
        }

        /// <summary>
        /// Lanza la UI de Skillz (lista de torneos)
        /// </summary>
        public void LaunchSkillz()
        {
            if (!isSkillzInitialized)
            {
                Debug.LogError("[Skillz] No está inicializado");
                return;
            }

            Debug.Log("[Skillz] Lanzando UI de Skillz...");

            // SKILLZ_SDK: Descomentar cuando el SDK esté instalado
            // SkillzCrossPlatform.LaunchSkillz();

            // Por ahora, mostrar mensaje
            Debug.Log("[Skillz] UI lanzada (modo simulación)");
        }

        /// <summary>
        /// Llamado por Skillz cuando un match comienza
        /// Este método debe ser llamado desde SkillzDelegate
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
        /// <param name="score">Score del jugador (en digitPark = tiempo en ms, menor es mejor)</param>
        public void ReportScore(float score)
        {
            if (!isInSkillzTournament)
            {
                Debug.LogWarning("[Skillz] No está en un torneo de Skillz");
                return;
            }

            Debug.Log($"[Skillz] Reportando score: {score}");

            // SKILLZ_SDK: Descomentar cuando el SDK esté instalado
            // SkillzCrossPlatform.ReportFinalScore(score);

            // Simular fin de match
            isInSkillzTournament = false;
            currentMatch = null;

            OnMatchCompleted?.Invoke();

            Debug.Log("[Skillz] Score reportado (modo simulación)");
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

            // SKILLZ_SDK: Descomentar cuando el SDK esté instalado
            // SkillzCrossPlatform.AbortMatch();

            isInSkillzTournament = false;
            currentMatch = null;

            OnMatchAborted?.Invoke(reason);
        }

        /// <summary>
        /// Obtiene un número aleatorio de Skillz (para fairness)
        /// </summary>
        public float GetSkillzRandom()
        {
            // SKILLZ_SDK: Descomentar cuando el SDK esté instalado
            // return SkillzCrossPlatform.Random.Value();

            // Por ahora usar Unity Random (NO es válido para producción)
            return UnityEngine.Random.value;
        }

        /// <summary>
        /// Obtiene un número aleatorio entero de Skillz
        /// </summary>
        public int GetSkillzRandomRange(int min, int max)
        {
            // SKILLZ_SDK: Descomentar cuando el SDK esté instalado
            // return SkillzCrossPlatform.Random.Range(min, max);

            return UnityEngine.Random.Range(min, max);
        }

        /// <summary>
        /// Verifica si el usuario actual es un jugador de Skillz
        /// </summary>
        public bool IsSkillzPlayer()
        {
            // SKILLZ_SDK: Descomentar cuando el SDK esté instalado
            // return SkillzCrossPlatform.IsMatchInProgress;

            return isInSkillzTournament;
        }

        /// <summary>
        /// Obtiene información del jugador de Skillz
        /// </summary>
        public SkillzPlayerInfo GetCurrentPlayer()
        {
            // SKILLZ_SDK: Descomentar cuando el SDK esté instalado
            // var player = SkillzCrossPlatform.GetPlayer();
            // return new SkillzPlayerInfo { ... };

            // Retornar datos simulados
            return new SkillzPlayerInfo
            {
                PlayerId = "simulated_player",
                DisplayName = "Test Player",
                AvatarUrl = ""
            };
        }

        /// <summary>
        /// Calcula la distribución de premios para un torneo
        /// </summary>
        public PrizeBreakdown CalculatePrizeBreakdown(float totalPot, int numberOfWinners = 3)
        {
            if (prizeConfig == null)
            {
                Debug.LogError("[Skillz] PrizeConfig no está configurado");
                return null;
            }

            return prizeConfig.CalculateBreakdown(totalPot, numberOfWinners);
        }
    }

    /// <summary>
    /// Entorno de Skillz
    /// </summary>
    public enum SkillzEnvironment
    {
        Sandbox,    // Para desarrollo y pruebas
        Production  // Para producción (dinero real)
    }

    /// <summary>
    /// Orientación del juego
    /// </summary>
    public enum SkillzOrientation
    {
        Portrait,
        Landscape
    }

    /// <summary>
    /// Información del match de Skillz
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
    /// Información del jugador de Skillz
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
