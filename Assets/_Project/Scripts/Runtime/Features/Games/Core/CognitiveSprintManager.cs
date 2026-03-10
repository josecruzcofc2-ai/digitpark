using System;
using System.Collections.Generic;
using UnityEngine;
using DigitPark.Localization;

namespace DigitPark.Games
{
    /// <summary>
    /// Manager to configure and manage Cognitive Sprints
    /// Allows selecting games and configuring the session before starting
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
        /// Selected games for the current sprint
        /// </summary>
        public List<GameType> SelectedGames { get; private set; } = new List<GameType>();

        /// <summary>
        /// Minimum games allowed
        /// </summary>
        public const int MIN_GAMES = 2;

        /// <summary>
        /// Maximum games allowed
        /// </summary>
        public const int MAX_GAMES = 5;

        /// <summary>
        /// Event when the game selection changes
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
        /// Clears the current selection
        /// </summary>
        public void ClearSelection()
        {
            SelectedGames.Clear();
            OnSelectionChanged?.Invoke(SelectedGames);
        }

        /// <summary>
        /// Adds a game to the selection
        /// </summary>
        public bool AddGame(GameType gameType)
        {
            if (SelectedGames.Count >= MAX_GAMES)
            {
                Debug.LogWarning($"Maximum {MAX_GAMES} games allowed");
                return false;
            }

            if (SelectedGames.Contains(gameType))
            {
                Debug.LogWarning($"{gameType} is already selected");
                return false;
            }

            SelectedGames.Add(gameType);
            OnSelectionChanged?.Invoke(SelectedGames);
            return true;
        }

        /// <summary>
        /// Removes a game from the selection
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
        /// Toggles the selection of a game
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
        /// Checks if a game is selected
        /// </summary>
        public bool IsSelected(GameType gameType)
        {
            return SelectedGames.Contains(gameType);
        }

        /// <summary>
        /// Checks if the selection is valid to start
        /// </summary>
        public bool IsValidSelection()
        {
            return SelectedGames.Count >= MIN_GAMES && SelectedGames.Count <= MAX_GAMES;
        }

        /// <summary>
        /// Gets the number of selected games
        /// </summary>
        public int SelectionCount => SelectedGames.Count;

        /// <summary>
        /// Reorders the selected games
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
        /// Starts the Cognitive Sprint with the current selection
        /// </summary>
        public void StartSprint(string opponentId, string opponentName, decimal entryFee, string matchId)
        {
            if (!IsValidSelection())
            {
                Debug.LogError($"Invalid selection. Between {MIN_GAMES} and {MAX_GAMES} games required");
                return;
            }

            if (GameSessionManager.Instance == null) { Debug.LogError("[CognitiveSprintManager] GameSessionManager not found"); return; }
            GameSessionManager.Instance.StartCognitiveSprintSession(
                new List<GameType>(SelectedGames),
                opponentId,
                opponentName,
                entryFee,
                matchId
            );

            // Clear selection after starting
            ClearSelection();
        }

        /// <summary>
        /// Starts a practice Cognitive Sprint (no opponent)
        /// </summary>
        public void StartPracticeSprint()
        {
            if (!IsValidSelection())
            {
                Debug.LogError($"Invalid selection. Between {MIN_GAMES} and {MAX_GAMES} games required");
                return;
            }

            // For practice, use special context with Practice mode
            var context = new GameContext
            {
                Mode = GameMode.Practice,
                Games = new List<GameType>(SelectedGames),
                EntryFee = 0
            };

            if (GameSessionManager.Instance == null) { Debug.LogError("[CognitiveSprintManager] GameSessionManager not found"); return; }
            GameSessionManager.Instance.SetContext(context);

            // Fire OnSessionStarted manually since SetContext doesn't do it
            GameSessionManager.Instance.NotifySessionStarted();

            // Load first game
            string sceneName = GameSessionManager.Instance.GetSceneNameForGame(SelectedGames[0]);
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);

            ClearSelection();
        }

        /// <summary>
        /// Starts an online Cognitive Sprint (with opponent)
        /// </summary>
        public void StartOnlineSprint()
        {
            if (!IsValidSelection())
            {
                Debug.LogError($"Invalid selection. Between {MIN_GAMES} and {MAX_GAMES} games required");
                return;
            }

            // For online, use online match context
            var context = new GameContext
            {
                Mode = GameMode.Online,
                Games = new List<GameType>(SelectedGames),
                EntryFee = 0
            };

            if (GameSessionManager.Instance == null) { Debug.LogError("[CognitiveSprintManager] GameSessionManager not found"); return; }
            GameSessionManager.Instance.SetContext(context);

            // Fire OnSessionStarted for online sprint
            GameSessionManager.Instance.NotifySessionStarted();

            // Load first game
            string sceneName = GameSessionManager.Instance.GetSceneNameForGame(SelectedGames[0]);
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);

            // Don't clear selection here because it may be needed for the following games
        }

        /// <summary>
        /// Gets the list of selected games (for matchmaking)
        /// </summary>
        public List<GameType> GetSelectedGames()
        {
            return new List<GameType>(SelectedGames);
        }

        /// <summary>
        /// Gets information for all available games
        /// </summary>
        public static GameInfo[] GetAllGameInfos()
        {
            return new GameInfo[]
            {
                new GameInfo
                {
                    Type = GameType.DigitRush,
                    Name = AutoLocalizer.Get("gameinfo_digitrush_name"),
                    Description = AutoLocalizer.Get("gameinfo_digitrush_desc"),
                    Icon = "icon_digit_rush",
                    Skill = AutoLocalizer.Get("gameinfo_digitrush_skill")
                },
                new GameInfo
                {
                    Type = GameType.MemoryPairs,
                    Name = AutoLocalizer.Get("gameinfo_memorypairs_name"),
                    Description = AutoLocalizer.Get("gameinfo_memorypairs_desc"),
                    Icon = "icon_memory_pairs",
                    Skill = AutoLocalizer.Get("gameinfo_memorypairs_skill")
                },
                new GameInfo
                {
                    Type = GameType.QuickMath,
                    Name = AutoLocalizer.Get("gameinfo_quickmath_name"),
                    Description = AutoLocalizer.Get("gameinfo_quickmath_desc"),
                    Icon = "icon_quick_math",
                    Skill = AutoLocalizer.Get("gameinfo_quickmath_skill")
                },
                new GameInfo
                {
                    Type = GameType.FlashTap,
                    Name = AutoLocalizer.Get("gameinfo_flashtap_name"),
                    Description = AutoLocalizer.Get("gameinfo_flashtap_desc"),
                    Icon = "icon_flash_tap",
                    Skill = AutoLocalizer.Get("gameinfo_flashtap_skill")
                },
                new GameInfo
                {
                    Type = GameType.OddOneOut,
                    Name = AutoLocalizer.Get("gameinfo_oddoneout_name"),
                    Description = AutoLocalizer.Get("gameinfo_oddoneout_desc"),
                    Icon = "icon_odd_one_out",
                    Skill = AutoLocalizer.Get("gameinfo_oddoneout_skill")
                }
            };
        }
    }

    /// <summary>
    /// Game information to display in UI
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
