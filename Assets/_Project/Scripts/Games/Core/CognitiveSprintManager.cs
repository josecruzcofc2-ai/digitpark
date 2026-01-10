using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitPark.Games
{
    /// <summary>
    /// Manager para configurar y gestionar Cognitive Sprints
    /// Permite seleccionar juegos y configurar la sesion antes de iniciar
    /// </summary>
    public class CognitiveSprintManager : MonoBehaviour
    {
        private static CognitiveSprintManager _instance;
        public static CognitiveSprintManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("CognitiveSprintManager");
                    _instance = go.AddComponent<CognitiveSprintManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        /// <summary>
        /// Juegos seleccionados para el sprint actual
        /// </summary>
        public List<GameType> SelectedGames { get; private set; } = new List<GameType>();

        /// <summary>
        /// Minimo de juegos permitidos
        /// </summary>
        public const int MIN_GAMES = 2;

        /// <summary>
        /// Maximo de juegos permitidos
        /// </summary>
        public const int MAX_GAMES = 5;

        /// <summary>
        /// Evento cuando la seleccion de juegos cambia
        /// </summary>
        public event Action<List<GameType>> OnSelectionChanged;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Limpia la seleccion actual
        /// </summary>
        public void ClearSelection()
        {
            SelectedGames.Clear();
            OnSelectionChanged?.Invoke(SelectedGames);
        }

        /// <summary>
        /// Agrega un juego a la seleccion
        /// </summary>
        public bool AddGame(GameType gameType)
        {
            if (SelectedGames.Count >= MAX_GAMES)
            {
                Debug.LogWarning($"Maximo {MAX_GAMES} juegos permitidos");
                return false;
            }

            if (SelectedGames.Contains(gameType))
            {
                Debug.LogWarning($"{gameType} ya esta seleccionado");
                return false;
            }

            SelectedGames.Add(gameType);
            OnSelectionChanged?.Invoke(SelectedGames);
            return true;
        }

        /// <summary>
        /// Remueve un juego de la seleccion
        /// </summary>
        public bool RemoveGame(GameType gameType)
        {
            bool removed = SelectedGames.Remove(gameType);
            if (removed)
            {
                OnSelectionChanged?.Invoke(SelectedGames);
            }
            return removed;
        }

        /// <summary>
        /// Alterna la seleccion de un juego
        /// </summary>
        public void ToggleGame(GameType gameType)
        {
            if (SelectedGames.Contains(gameType))
            {
                RemoveGame(gameType);
            }
            else
            {
                AddGame(gameType);
            }
        }

        /// <summary>
        /// Verifica si un juego esta seleccionado
        /// </summary>
        public bool IsSelected(GameType gameType)
        {
            return SelectedGames.Contains(gameType);
        }

        /// <summary>
        /// Verifica si la seleccion es valida para iniciar
        /// </summary>
        public bool IsValidSelection()
        {
            return SelectedGames.Count >= MIN_GAMES && SelectedGames.Count <= MAX_GAMES;
        }

        /// <summary>
        /// Obtiene el numero de juegos seleccionados
        /// </summary>
        public int SelectionCount => SelectedGames.Count;

        /// <summary>
        /// Reordena los juegos seleccionados
        /// </summary>
        public void ReorderGames(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= SelectedGames.Count ||
                toIndex < 0 || toIndex >= SelectedGames.Count)
            {
                return;
            }

            var game = SelectedGames[fromIndex];
            SelectedGames.RemoveAt(fromIndex);
            SelectedGames.Insert(toIndex, game);
            OnSelectionChanged?.Invoke(SelectedGames);
        }

        /// <summary>
        /// Inicia el Cognitive Sprint con la seleccion actual
        /// </summary>
        public void StartSprint(string opponentId, string opponentName, decimal entryFee, string matchId)
        {
            if (!IsValidSelection())
            {
                Debug.LogError($"Seleccion invalida. Se requieren entre {MIN_GAMES} y {MAX_GAMES} juegos");
                return;
            }

            GameSessionManager.Instance.StartCognitiveSprintSession(
                new List<GameType>(SelectedGames),
                opponentId,
                opponentName,
                entryFee,
                matchId
            );

            // Limpiar seleccion despues de iniciar
            ClearSelection();
        }

        /// <summary>
        /// Inicia un Cognitive Sprint de practica (sin oponente)
        /// </summary>
        public void StartPracticeSprint()
        {
            if (!IsValidSelection())
            {
                Debug.LogError($"Seleccion invalida. Se requieren entre {MIN_GAMES} y {MAX_GAMES} juegos");
                return;
            }

            // Para practica, usar contexto especial
            var context = new GameContext
            {
                Mode = GameMode.Practice,
                Games = new List<GameType>(SelectedGames),
                EntryFee = 0
            };

            GameSessionManager.Instance.SetContext(context);

            // Cargar primer juego
            string sceneName = GameSessionManager.Instance.GetSceneNameForGame(SelectedGames[0]);
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);

            ClearSelection();
        }

        /// <summary>
        /// Inicia un Cognitive Sprint online (con oponente)
        /// </summary>
        public void StartOnlineSprint()
        {
            if (!IsValidSelection())
            {
                Debug.LogError($"Seleccion invalida. Se requieren entre {MIN_GAMES} y {MAX_GAMES} juegos");
                return;
            }

            // Para online, usar contexto de partida online
            var context = new GameContext
            {
                Mode = GameMode.Online,
                Games = new List<GameType>(SelectedGames),
                EntryFee = 0
            };

            GameSessionManager.Instance.SetContext(context);

            // Cargar primer juego
            string sceneName = GameSessionManager.Instance.GetSceneNameForGame(SelectedGames[0]);
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);

            // No limpiar seleccion aqui porque puede necesitarse para los siguientes juegos
        }

        /// <summary>
        /// Obtiene la lista de juegos seleccionados (para matchmaking)
        /// </summary>
        public List<GameType> GetSelectedGames()
        {
            return new List<GameType>(SelectedGames);
        }

        /// <summary>
        /// Obtiene informacion de todos los juegos disponibles
        /// </summary>
        public static GameInfo[] GetAllGameInfos()
        {
            return new GameInfo[]
            {
                new GameInfo
                {
                    Type = GameType.DigitRush,
                    Name = "Digit Rush",
                    Description = "Toca los numeros del 1 al 9 en orden lo mas rapido posible",
                    Icon = "icon_digit_rush",
                    Skill = "Velocidad + Atencion"
                },
                new GameInfo
                {
                    Type = GameType.MemoryPairs,
                    Name = "Memory Pairs",
                    Description = "Encuentra todos los pares de cartas iguales",
                    Icon = "icon_memory_pairs",
                    Skill = "Memoria Visual"
                },
                new GameInfo
                {
                    Type = GameType.QuickMath,
                    Name = "Quick Math",
                    Description = "Resuelve 10 operaciones matematicas lo mas rapido posible",
                    Icon = "icon_quick_math",
                    Skill = "Calculo Mental"
                },
                new GameInfo
                {
                    Type = GameType.FlashTap,
                    Name = "Flash Tap",
                    Description = "Reacciona a la senal visual lo mas rapido posible",
                    Icon = "icon_flash_tap",
                    Skill = "Reflejos"
                },
                new GameInfo
                {
                    Type = GameType.OddOneOut,
                    Name = "Odd One Out",
                    Description = "Encuentra el elemento diferente en la cuadricula",
                    Icon = "icon_odd_one_out",
                    Skill = "Percepcion Visual"
                }
            };
        }
    }

    /// <summary>
    /// Informacion de un juego para mostrar en UI
    /// </summary>
    [Serializable]
    public class GameInfo
    {
        public GameType Type;
        public string Name;
        public string Description;
        public string Icon;
        public string Skill;
    }
}
