using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitPark.Data
{
    /// <summary>
    /// Modo de partida
    /// </summary>
    public enum MatchMode
    {
        Practice,   // Partida de practica (sin oponente)
        Online      // Partida online 1v1
    }

    /// <summary>
    /// Resultado de una partida general (no cash)
    /// </summary>
    public enum GeneralMatchResult
    {
        Win,
        Loss,
        Draw,
        None        // Practica (sin resultado competitivo)
    }

    /// <summary>
    /// Entrada individual de historial de partidas generales.
    /// Diferente de HistoryEntry (CashBattle) - aqui no hay dinero.
    /// </summary>
    [Serializable]
    public class MatchHistoryEntry
    {
        public string entryId;
        public string gameType;             // "DigitRush", "MemoryPairs", "CognitiveSprint", etc.
        public MatchMode mode;
        public GeneralMatchResult result;

        public float time;                  // Tiempo total en segundos
        public int errors;                  // Errores cometidos
        public float penaltyTime;           // Penalizacion por errores
        public float finalScore;            // time + penaltyTime

        public string opponentName;         // null si es practica
        public string opponentId;           // null si es practica
        public float opponentScore;         // Score del oponente (0 si practica)

        // Cognitive Sprint: juegos incluidos en la sesion
        public string[] gamesPlayed;        // ej: ["DigitRush", "MemoryPairs", "QuickMath"]

        public string timestamp;            // ISO 8601

        public MatchHistoryEntry()
        {
            entryId = Guid.NewGuid().ToString();
            mode = MatchMode.Practice;
            result = GeneralMatchResult.None;
            timestamp = DateTime.Now.ToString("o");
        }

        /// <summary>
        /// Crea entrada de practica
        /// </summary>
        public static MatchHistoryEntry CreatePractice(string gameType, float time, int errors, float penaltyTime)
        {
            return new MatchHistoryEntry
            {
                gameType = gameType,
                mode = MatchMode.Practice,
                result = GeneralMatchResult.None,
                time = time,
                errors = errors,
                penaltyTime = penaltyTime,
                finalScore = time + penaltyTime
            };
        }

        /// <summary>
        /// Crea entrada de partida online
        /// </summary>
        public static MatchHistoryEntry CreateOnlineMatch(
            string gameType, float time, int errors, float penaltyTime,
            string opponentName, string opponentId, float opponentScore)
        {
            float myFinal = time + penaltyTime;
            GeneralMatchResult matchResult;

            if (Mathf.Approximately(myFinal, opponentScore))
                matchResult = GeneralMatchResult.Draw;
            else if (myFinal < opponentScore)
                matchResult = GeneralMatchResult.Win;
            else
                matchResult = GeneralMatchResult.Loss;

            return new MatchHistoryEntry
            {
                gameType = gameType,
                mode = MatchMode.Online,
                result = matchResult,
                time = time,
                errors = errors,
                penaltyTime = penaltyTime,
                finalScore = myFinal,
                opponentName = opponentName,
                opponentId = opponentId,
                opponentScore = opponentScore
            };
        }

        /// <summary>
        /// Crea entrada de Cognitive Sprint (practica)
        /// </summary>
        public static MatchHistoryEntry CreateCognitiveSprintPractice(
            string[] gamesPlayed, float time, int errors, float penaltyTime)
        {
            return new MatchHistoryEntry
            {
                gameType = "CognitiveSprint",
                mode = MatchMode.Practice,
                result = GeneralMatchResult.None,
                time = time,
                errors = errors,
                penaltyTime = penaltyTime,
                finalScore = time + penaltyTime,
                gamesPlayed = gamesPlayed
            };
        }

        /// <summary>
        /// Crea entrada de Cognitive Sprint (online)
        /// </summary>
        public static MatchHistoryEntry CreateCognitiveSprintOnline(
            string[] gamesPlayed, float time, int errors, float penaltyTime,
            string opponentName, string opponentId, float opponentScore)
        {
            float myFinal = time + penaltyTime;
            GeneralMatchResult matchResult;

            if (Mathf.Approximately(myFinal, opponentScore))
                matchResult = GeneralMatchResult.Draw;
            else if (myFinal < opponentScore)
                matchResult = GeneralMatchResult.Win;
            else
                matchResult = GeneralMatchResult.Loss;

            return new MatchHistoryEntry
            {
                gameType = "CognitiveSprint",
                mode = MatchMode.Online,
                result = matchResult,
                time = time,
                errors = errors,
                penaltyTime = penaltyTime,
                finalScore = myFinal,
                opponentName = opponentName,
                opponentId = opponentId,
                opponentScore = opponentScore,
                gamesPlayed = gamesPlayed
            };
        }

        /// <summary>
        /// Es una entrada de Cognitive Sprint?
        /// </summary>
        public bool IsCognitiveSprint => gameType == "CognitiveSprint";

        /// <summary>
        /// Texto compacto de juegos del sprint (ej: "DR + MP + QM")
        /// </summary>
        public string GetSprintGamesText()
        {
            if (gamesPlayed == null || gamesPlayed.Length == 0) return "";

            var abbreviations = new Dictionary<string, string>
            {
                { "DigitRush", "DR" },
                { "MemoryPairs", "MP" },
                { "QuickMath", "QM" },
                { "FlashTap", "FT" },
                { "OddOneOut", "OO" }
            };

            var parts = new List<string>();
            foreach (var g in gamesPlayed)
            {
                parts.Add(abbreviations.ContainsKey(g) ? abbreviations[g] : g);
            }
            return string.Join(" + ", parts);
        }

        public DateTime GetTimestamp()
        {
            if (DateTime.TryParse(timestamp, out DateTime result))
                return result;
            return DateTime.Now;
        }

        /// <summary>
        /// Texto relativo de fecha
        /// </summary>
        public string GetFormattedDate()
        {
            var diff = DateTime.Now - GetTimestamp();

            if (diff.TotalMinutes < 1) return "Ahora";
            if (diff.TotalMinutes < 60) return $"Hace {(int)diff.TotalMinutes} min";
            if (diff.TotalHours < 24) return $"Hace {(int)diff.TotalHours}h";
            if (diff.TotalDays < 7) return $"Hace {(int)diff.TotalDays}d";
            return GetTimestamp().ToString("dd/MM/yyyy");
        }

        /// <summary>
        /// Texto del resultado
        /// </summary>
        public string GetResultText()
        {
            return result switch
            {
                GeneralMatchResult.Win => "WIN",
                GeneralMatchResult.Loss => "LOSS",
                GeneralMatchResult.Draw => "DRAW",
                GeneralMatchResult.None => "PRACT.",
                _ => ""
            };
        }

        /// <summary>
        /// Color del resultado
        /// </summary>
        public Color GetResultColor()
        {
            return result switch
            {
                GeneralMatchResult.Win => new Color(0.2f, 0.9f, 0.4f, 1f),
                GeneralMatchResult.Loss => new Color(1f, 0.3f, 0.3f, 1f),
                GeneralMatchResult.Draw => new Color(1f, 0.8f, 0f, 1f),
                GeneralMatchResult.None => new Color(0.5f, 0.5f, 0.55f, 1f),
                _ => Color.white
            };
        }

        /// <summary>
        /// Tiempo formateado
        /// </summary>
        public string GetFormattedTime()
        {
            return $"{finalScore:F1}s";
        }

        /// <summary>
        /// Subtitulo (oponente, practica, o juegos del sprint)
        /// </summary>
        public string GetSubtitle()
        {
            if (IsCognitiveSprint)
            {
                string gamesText = GetSprintGamesText();
                if (mode == MatchMode.Practice)
                    return $"Practica \u00B7 {gamesText}";
                return $"vs. {opponentName} \u00B7 {gamesText}";
            }

            if (mode == MatchMode.Practice)
                return "Practica";
            return $"vs. {opponentName}";
        }

        /// <summary>
        /// Detalle compacto: tiempo + errores (+ num juegos si sprint)
        /// </summary>
        public string GetDetailText()
        {
            string errText = errors == 0 ? "0 err" : $"{errors} err";
            if (IsCognitiveSprint && gamesPlayed != null)
                return $"{GetFormattedTime()} \u00B7 {errText} \u00B7 {gamesPlayed.Length} juegos";
            return $"{GetFormattedTime()} \u00B7 {errText}";
        }
    }

    /// <summary>
    /// Wrapper para serializar lista de historial
    /// </summary>
    [Serializable]
    public class MatchHistoryWrapper
    {
        public List<MatchHistoryEntry> entries = new List<MatchHistoryEntry>();
    }

    /// <summary>
    /// Servicio de almacenamiento de historial de partidas generales.
    /// Singleton ligero, usa PlayerPrefs.
    /// </summary>
    public class MatchHistoryStorage
    {
        private const string STORAGE_KEY = "MatchHistory_Entries";
        private const int MAX_ENTRIES = 100;

        private static MatchHistoryStorage _instance;
        public static MatchHistoryStorage Instance => _instance ??= new MatchHistoryStorage();

        private List<MatchHistoryEntry> _entries;

        public List<MatchHistoryEntry> Entries
        {
            get
            {
                if (_entries == null) Load();
                return _entries;
            }
        }

        /// <summary>
        /// Agrega una entrada y persiste
        /// </summary>
        public void AddEntry(MatchHistoryEntry entry)
        {
            if (_entries == null) Load();

            _entries.Insert(0, entry); // Mas reciente primero

            // Limitar
            if (_entries.Count > MAX_ENTRIES)
                _entries.RemoveRange(MAX_ENTRIES, _entries.Count - MAX_ENTRIES);

            Save();
        }

        /// <summary>
        /// Obtiene entradas filtradas por juego
        /// </summary>
        public List<MatchHistoryEntry> GetEntries(string gameTypeFilter = null, int limit = 20, int offset = 0)
        {
            if (_entries == null) Load();

            List<MatchHistoryEntry> filtered;

            if (string.IsNullOrEmpty(gameTypeFilter))
            {
                filtered = new List<MatchHistoryEntry>(_entries);
            }
            else
            {
                filtered = _entries.FindAll(e => e.gameType == gameTypeFilter);
            }

            int start = Mathf.Min(offset, filtered.Count);
            int count = Mathf.Min(limit, filtered.Count - start);

            if (count <= 0) return new List<MatchHistoryEntry>();
            return filtered.GetRange(start, count);
        }

        /// <summary>
        /// Total de entradas (con filtro opcional)
        /// </summary>
        public int GetTotalCount(string gameTypeFilter = null)
        {
            if (_entries == null) Load();

            if (string.IsNullOrEmpty(gameTypeFilter))
                return _entries.Count;

            return _entries.FindAll(e => e.gameType == gameTypeFilter).Count;
        }

        private void Load()
        {
            string json = PlayerPrefs.GetString(STORAGE_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                var wrapper = JsonUtility.FromJson<MatchHistoryWrapper>(json);
                _entries = wrapper?.entries ?? new List<MatchHistoryEntry>();
            }
            else
            {
                _entries = new List<MatchHistoryEntry>();
            }
        }

        private void Save()
        {
            var wrapper = new MatchHistoryWrapper { entries = _entries };
            string json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(STORAGE_KEY, json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Limpia todo el historial
        /// </summary>
        public void ClearAll()
        {
            _entries = new List<MatchHistoryEntry>();
            PlayerPrefs.DeleteKey(STORAGE_KEY);
        }
    }
}
