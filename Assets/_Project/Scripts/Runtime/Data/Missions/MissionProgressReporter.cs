using UnityEngine;
using DigitPark.Games;

namespace DigitPark.Data
{
    /// <summary>
    /// Clase estatica que permite a los juegos reportar progreso de misiones
    /// sin depender de DailyMissionsManager (que solo vive en la escena DailyMissions).
    /// Escribe deltas acumulados en PlayerPrefs que el manager lee al iniciar.
    /// </summary>
    public static class MissionProgressReporter
    {
        // PlayerPrefs keys para deltas acumulados
        private const string PREFIX = "MPR_";
        private const string PLAY_GAMES = PREFIX + "PlayGames_delta";
        private const string WIN_GAMES = PREFIX + "WinGames_delta";
        private const string BEST_SCORE = PREFIX + "BestScore_delta";
        private const string TOTAL_SCORE = PREFIX + "TotalScore_delta";
        private const string PRECISION_COUNT = PREFIX + "PrecisionCount_delta";
        private const string WIN_STREAK = PREFIX + "WinStreak_current";
        private const string TOURNAMENT_PLAYED = PREFIX + "TournamentPlayed_delta";
        private const string SPRINT_COMPLETED = PREFIX + "SprintCompleted_delta";
        private const string GAME_TYPES_PLAYED = PREFIX + "GameTypesPlayed_mask";

        // Per-game deltas
        private static string PlaySpecificKey(GameType type) => $"{PREFIX}PlaySpecific_{type}_delta";
        private static string WinSpecificKey(GameType type) => $"{PREFIX}WinSpecific_{type}_delta";

        /// <summary>
        /// Reporta que se jugo una partida.
        /// </summary>
        public static void ReportGamePlayed(GameType type, bool won, int score, float precision)
        {
            // PlayGames delta
            int playDelta = PlayerPrefs.GetInt(PLAY_GAMES, 0);
            PlayerPrefs.SetInt(PLAY_GAMES, playDelta + 1);

            // PlaySpecificGame delta
            int specificPlay = PlayerPrefs.GetInt(PlaySpecificKey(type), 0);
            PlayerPrefs.SetInt(PlaySpecificKey(type), specificPlay + 1);

            // Game types bitmask (para PlayAllGameTypes)
            int typesMask = PlayerPrefs.GetInt(GAME_TYPES_PLAYED, 0);
            typesMask |= (1 << (int)type);
            PlayerPrefs.SetInt(GAME_TYPES_PLAYED, typesMask);

            // Win tracking
            if (won)
            {
                int winDelta = PlayerPrefs.GetInt(WIN_GAMES, 0);
                PlayerPrefs.SetInt(WIN_GAMES, winDelta + 1);

                int specificWin = PlayerPrefs.GetInt(WinSpecificKey(type), 0);
                PlayerPrefs.SetInt(WinSpecificKey(type), specificWin + 1);

                // Win streak
                int streak = PlayerPrefs.GetInt(WIN_STREAK, 0);
                PlayerPrefs.SetInt(WIN_STREAK, streak + 1);
            }
            else
            {
                // Reset win streak on loss
                PlayerPrefs.SetInt(WIN_STREAK, 0);
            }

            // Score tracking
            int bestScore = PlayerPrefs.GetInt(BEST_SCORE, 0);
            if (score > bestScore)
                PlayerPrefs.SetInt(BEST_SCORE, score);

            int totalScore = PlayerPrefs.GetInt(TOTAL_SCORE, 0);
            PlayerPrefs.SetInt(TOTAL_SCORE, totalScore + score);

            // Precision tracking (>= 80% counts)
            if (precision >= 0.8f)
            {
                int precisionCount = PlayerPrefs.GetInt(PRECISION_COUNT, 0);
                PlayerPrefs.SetInt(PRECISION_COUNT, precisionCount + 1);
            }

            PlayerPrefs.Save();
        }

        /// <summary>
        /// Reporta que se jugo un torneo.
        /// </summary>
        public static void ReportTournamentPlayed()
        {
            int delta = PlayerPrefs.GetInt(TOURNAMENT_PLAYED, 0);
            PlayerPrefs.SetInt(TOURNAMENT_PLAYED, delta + 1);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Reporta que se completo un Cognitive Sprint.
        /// </summary>
        public static void ReportCognitiveSprintCompleted()
        {
            int delta = PlayerPrefs.GetInt(SPRINT_COMPLETED, 0);
            PlayerPrefs.SetInt(SPRINT_COMPLETED, delta + 1);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Lee y consume un delta acumulado.
        /// Retorna el valor y lo resetea a 0.
        /// </summary>
        public static int ConsumeIntDelta(string key)
        {
            int value = PlayerPrefs.GetInt(key, 0);
            if (value != 0)
            {
                PlayerPrefs.SetInt(key, 0);
                PlayerPrefs.Save();
            }
            return value;
        }

        /// <summary>
        /// Lee un valor sin consumirlo.
        /// </summary>
        public static int PeekInt(string key)
        {
            return PlayerPrefs.GetInt(key, 0);
        }

        /// <summary>
        /// Resetea todos los deltas acumulados.
        /// Llamado por DailyMissionsManager despues de aplicar los deltas.
        /// </summary>
        public static void ClearAllDeltas()
        {
            PlayerPrefs.DeleteKey(PLAY_GAMES);
            PlayerPrefs.DeleteKey(WIN_GAMES);
            PlayerPrefs.DeleteKey(BEST_SCORE);
            PlayerPrefs.DeleteKey(TOTAL_SCORE);
            PlayerPrefs.DeleteKey(PRECISION_COUNT);
            PlayerPrefs.DeleteKey(WIN_STREAK);
            PlayerPrefs.DeleteKey(TOURNAMENT_PLAYED);
            PlayerPrefs.DeleteKey(SPRINT_COMPLETED);
            PlayerPrefs.DeleteKey(GAME_TYPES_PLAYED);

            // Clear per-game deltas
            foreach (GameType type in System.Enum.GetValues(typeof(GameType)))
            {
                PlayerPrefs.DeleteKey(PlaySpecificKey(type));
                PlayerPrefs.DeleteKey(WinSpecificKey(type));
            }

            PlayerPrefs.Save();
        }

        // Public key accessors for DailyMissionsManager to read deltas
        public static string KeyPlayGames => PLAY_GAMES;
        public static string KeyWinGames => WIN_GAMES;
        public static string KeyBestScore => BEST_SCORE;
        public static string KeyTotalScore => TOTAL_SCORE;
        public static string KeyPrecisionCount => PRECISION_COUNT;
        public static string KeyWinStreak => WIN_STREAK;
        public static string KeyTournamentPlayed => TOURNAMENT_PLAYED;
        public static string KeySprintCompleted => SPRINT_COMPLETED;
        public static string KeyGameTypesPlayed => GAME_TYPES_PLAYED;
        public static string KeyPlaySpecific(GameType type) => PlaySpecificKey(type);
        public static string KeyWinSpecific(GameType type) => WinSpecificKey(type);
    }
}
